using GoogleHelper.Services;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Models.SurePet.API.Pets;

namespace SurePet2Google.Blazor.Server.Services
{
    public class NotificationService : IDisposable
    {
        protected SurePetService SurePetService { get; set; }

        protected GoogleService<PetContext> GoogleService { get; set; }

        protected PersistenceService PersistenceService { get; set; }

        protected IConfiguration Configuration { get; set; }

        public NotificationService(SurePetService surePetService, GoogleService<PetContext> googleService, PersistenceService persistenceService, IConfiguration configuration)
        {
            this.SurePetService = surePetService;
            this.GoogleService = googleService;
            this.PersistenceService = persistenceService;
            this.Configuration = configuration;
            this.cancellationToken = new CancellationTokenSource();
        }

        private bool disposedValue;
        private readonly CancellationTokenSource cancellationToken;

        private bool alreadyStarted = false;

        private readonly List<(PetContext context, PositionDataEnriched positionData)> currentStatus = new();

        public void StartLoop()
        {
            if (this.alreadyStarted)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await this.DoNotifications();

                    await Task.Delay(5000, this.cancellationToken.Token);
                    if (this.cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            });

            this.alreadyStarted = true;
        }

        private async Task DoNotifications()
        {
            List<(PetContext context, PositionDataEnriched positionData)> previousStatus = new List<(PetContext context, PositionDataEnriched positionData)>(this.currentStatus);

            try
            {
                this.currentStatus.Clear();
                foreach (KeyValuePair<string, PetContext> context in this.PersistenceService.GooglePetContextReadOnly)
                {
                    GetPets? pets = await this.SurePetService.GetPets(context.Value.SurePetBearerToken, this.cancellationToken.Token);
                    if (pets is null || pets?.data is null)
                    {
                        continue;
                    }

                    this.currentStatus.AddRange(pets.data.Where(pet => pet?.position is not null)
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

                foreach ((PetContext currentContext, PositionDataEnriched currentPosition) in this.currentStatus)
                {
                    if (currentPosition is null)
                    {
                        continue;
                    }

                    var previousPosition = previousStatus.FirstOrDefault(position => position.positionData.pet_id == currentPosition.pet_id);
                    if (previousPosition.context is not null && previousPosition.positionData is not null && (previousPosition.positionData?.since != currentPosition?.since))
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
                        string objectName = (currentLocation == PetPosition.Outside) ? $"{currentPosition.pet_name} Left" : $"{currentPosition.pet_name} Entered";

                        this.GoogleService.ProvideObjectDetection(this.Configuration["Google:Homegraph:private_key"], this.Configuration["Google:Homegraph:private_key_id"], this.Configuration["Google:Homegraph:client_email"], currentContext.GoogleAccessToken, triggeredDevice.Key, objectName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.cancellationToken.Cancel();
                }

                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
