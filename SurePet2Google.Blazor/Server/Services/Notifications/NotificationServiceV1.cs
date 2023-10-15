using GoogleHelper.Services;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Models.Responses.Pets;

namespace SurePet2Google.Blazor.Server.Services.Notifications
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Obsolete("Now only used as a fallback option as a new preferred method (v2) is used for notifications.")]
    public class NotificationServiceV1 : NotificationServiceBase
    {
        public NotificationServiceV1(SurePetService surePetService, GoogleService<PetContext> googleService, PersistenceService persistenceService, IConfiguration configuration)
            : base(surePetService, googleService, persistenceService, configuration)
        {
        }

        private readonly List<(PetContext context, PositionDataEnriched positionData)> currentStatus = new();

        protected override async Task DoNotifications()
        {
            List<(PetContext context, PositionDataEnriched positionData)> previousStatus = new List<(PetContext context, PositionDataEnriched positionData)>(this.currentStatus);

            try
            {
                this.currentStatus.Clear();
                foreach (KeyValuePair<string, PetContext> context in this.PersistenceService.GooglePetContextReadOnly)
                {
                    if (context.Value.Pets?.Any() ?? false)
                    {
                        foreach (var pet in context.Value.Pets)
                        {
                            var locationResult = await this.SurePetService.GetPosition(context.Value.SurePetBearerToken, pet.id.ToString(), this.cancellationToken);
                            locationResult.data.pet_name = pet.name;

                            this.currentStatus.Add((context.Value, locationResult.data));
                        }
                    }
                    else
                    {
                        var pets = await this.SurePetService.GetPets(context.Value.SurePetBearerToken, this.cancellationToken);

                        context.Value.Pets = pets.data.Where(x => x is not null).ToList();

                        this.currentStatus.AddRange(context.Value.Pets.Where(pet => pet?.position is not null)
                            .Select(pet =>
                            {
                                var status = pet.position;
                                PositionDataEnriched position = new PositionDataEnriched()
                                {
                                    pet_name = pet.name,
                                    device_id = status.device_id,
                                    pet_id = pet.id,
                                    since = status.since,
                                    tag_id = status.tag_id,
                                    where = status.where
                                };

                                return (context.Value, position);
                            })
                        );
                    }
                }

                foreach ((PetContext currentContext, PositionDataEnriched currentPosition) in this.currentStatus)
                {
                    Console.WriteLine($"Querying pet {currentPosition.pet_name} - status {currentPosition.where} - {currentPosition.since}");

                    if (currentPosition is null)
                    {
                        continue;
                    }

                    var previousPosition = previousStatus.FirstOrDefault(position => position.positionData.pet_id == currentPosition.pet_id);
                    if (previousPosition.context is not null && previousPosition.positionData is not null && previousPosition.positionData?.since != currentPosition?.since)
                    {
                        int? triggeredDeviceId = currentPosition?.device_id;
                        if (triggeredDeviceId is null)
                        {
                            continue;
                        }

                        KeyValuePair<string, GoogleHelper.Models.BaseDeviceModel> triggeredDevice = currentContext.Devices.FirstOrDefault(device => device.Key == triggeredDeviceId.ToString());
                        if (triggeredDevice.Value is null)
                        {
                            continue;
                        }

                        var currentLocation = (PetPosition)currentPosition.where;
                        string objectName = currentLocation == PetPosition.Outside ? $"{currentPosition.pet_name} Left" : $"{currentPosition.pet_name} Entered";

                        this.GoogleService.ProvideObjectDetection(this.Configuration["Google:Homegraph:private_key"], this.Configuration["Google:Homegraph:private_key_id"], this.Configuration["Google:Homegraph:client_email"], currentContext.GoogleAccessToken, triggeredDevice.Key, objectName, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
