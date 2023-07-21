using GoogleHelper.Services;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Models.Responses.Pets;

namespace SurePet2Google.Blazor.Server.Services.Notifications
{
    public class NotificationServiceV2 : NotificationServiceBase
    {
        public NotificationServiceV2(SurePetService surePetService, GoogleService<PetContext> googleService, PersistenceService persistenceService, IConfiguration configuration)
            : base(surePetService, googleService, persistenceService, configuration)
        {
        }

        private DateTime lastUpdated = DateTime.UtcNow;

        private List<Models.Responses.Timeline.MovementType> peekedType = new List<Models.Responses.Timeline.MovementType>() { Models.Responses.Timeline.MovementType.UnknownPeeked, Models.Responses.Timeline.MovementType.KnownPeeked };
        private List<Models.Responses.Timeline.MovementType> movedType = new List<Models.Responses.Timeline.MovementType>() { Models.Responses.Timeline.MovementType.UnknownMoved, Models.Responses.Timeline.MovementType.KnownMoved };

        protected override async Task DoNotifications()
        {
            try
            {
                foreach (KeyValuePair<string, PetContext> context in this.PersistenceService.GooglePetContextReadOnly)
                {
                    List<(string device, string text)> newEvents = new();

                    if (!context.Value.Pets?.Any() ?? true)
                    {
                        var pets = await this.SurePetService.GetPets(context.Value.SurePetBearerToken, this.cancellationToken);

                        context.Value.Pets = pets?.data?.Where(x => x is not null).ToList() ?? Enumerable.Empty<PetDatum>().ToList();
                    }

                    var results = (await this.SurePetService.GetTimeline(context.Value.SurePetBearerToken, this.cancellationToken))?.data
                        .Where(x => x?.movements != null)
                        .Where(x => x.movements?.Any(y => y.created_at >= lastUpdated) ?? false);

                    if (results == null)
                        continue;

                    results.Where(x => x.movements.Any(y => this.peekedType.Contains(y.type))).ToList()
                        .ForEach(x => newEvents.Add((x.movements[0].device_id.ToString(), x.pets?.Any() ?? false ? $"{x.pets[0].name} Peeked" : "An Animal")));

                    results.Where(x => x.movements.Any(y => this.movedType.Contains(y.type) && y.direction != Models.Responses.Timeline.Direction.Looked)).Where(x => x.pets.Any()).ToList()
                        .ForEach(x => newEvents.Add((x.movements[0].device_id.ToString(), (x.pets?.Any() ?? false ? x.pets[0].name : "An Animal") + " " + (x.movements[0].direction == Models.Responses.Timeline.Direction.Left ? "Left" : "Entered"))));

                    foreach (var movementEvent in newEvents)
                    {
                        KeyValuePair<string, GoogleHelper.Models.BaseDeviceModel> triggeredDevice = context.Value.Devices.FirstOrDefault(device => device.Key == movementEvent.device);
                        if (triggeredDevice.Value is null)
                        {
                            continue;
                        }

                        _ = this.GoogleService.ProvideObjectDetection(this.Configuration["Google:Homegraph:private_key"], this.Configuration["Google:Homegraph:private_key_id"], this.Configuration["Google:Homegraph:client_email"], context.Value.GoogleAccessToken, triggeredDevice.Key, movementEvent.text);
                    }
                }

                lastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
