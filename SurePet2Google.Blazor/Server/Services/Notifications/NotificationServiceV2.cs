using GoogleHelper.Services;
using Polly;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Models.Responses.Devices;
using SurePet2Google.Blazor.Server.Models.Responses.Pets;
using SurePet2Google.Blazor.Server.Models.Responses.Timeline;

namespace SurePet2Google.Blazor.Server.Services.Notifications
{
    public class NotificationServiceV2 : NotificationServiceBase
    {
        public NotificationServiceV2(SurePetService surePetService, GoogleService<PetContext> googleService, PersistenceService persistenceService, IConfiguration configuration)
            : base(surePetService, googleService, persistenceService, configuration)
        {
        }

        private DateTime lastUpdated = DateTime.UtcNow;

        protected override async Task DoNotifications()
        {
            try
            {
                List<Task> notificationTasks = new();

                foreach (KeyValuePair<string, PetContext> context in this.PersistenceService.GooglePetContextReadOnly)
                {
                    notificationTasks.Add(Task.Run(async () =>
                    {
                        List<(string? device, string text)> newEvents = await this.GetEvents(context.Value);

                        await this.DispatchEvents(context.Value, newEvents);
                    }));
                }

                await Task.WhenAll(notificationTasks);

                lastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task<List<(string? device, string text)>> GetEvents(PetContext petContext)
        {
            List<(string? device, string text)> newEvents = new();

            if (!petContext.Pets?.Any() ?? true)
            {
                var pets = await this.SurePetService.GetPets(petContext.SurePetBearerToken, this.cancellationToken);

                petContext.Pets = pets?.data?.Where(x => x is not null).ToList() ?? Enumerable.Empty<PetDatum>().ToList();
            }

            var results = (await this.SurePetService.GetTimeline(petContext.SurePetBearerToken, this.cancellationToken))?.data
                .Where(x => x?.movements != null)
                .Where(x => x.movements?.Any(y => y.created_at.ToLocalTime() >= lastUpdated.ToLocalTime()) ?? false);

            if (results != null)
            {
                foreach (var result in results)
                {
                    var pet = petContext.Pets?.FirstOrDefault(x => x.tag_id == result.tags?[0].id);
                    var petName = pet?.name ?? "An Animal";

                    string actionDescription = this.GetActionDescription(result.movements?[0]);

                    newEvents.Add((result.movements?[0].device_id.ToString(), $"{petName} {actionDescription}"));
                }
            }

            return newEvents;
        }

        private async Task DispatchEvents(PetContext petContext, List<(string? device, string text)> events)
        {
            foreach (var movementEvent in events.Where(x => x.device is not null))
            {
                KeyValuePair<string, GoogleHelper.Models.BaseDeviceModel> triggeredDevice = petContext.Devices.FirstOrDefault(device => device.Key == movementEvent.device);
                if (triggeredDevice.Value is null)
                {
                    continue;
                }

                await this.GoogleService.ProvideObjectDetection(this.Configuration["Google:Homegraph:private_key"], this.Configuration["Google:Homegraph:private_key_id"], this.Configuration["Google:Homegraph:client_email"], petContext.GoogleAccessToken, triggeredDevice.Key, movementEvent.text, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private string GetActionDescription(Movement? movement)
        {
            return movement switch
            {
                {
                    direction: Direction.Looked,
                    type: MovementType.UnknownMovedOrPeekedA or MovementType.UnknownMovedOrPeekedB or MovementType.KnownPeeked
                } => "Peeked",
                {
                    direction: Direction.Entered,
                    type: MovementType.KnownMoved
                } => "Entered",
                {
                    direction: Direction.Left
                } => "Left",
                _ => string.Empty
            };
        }
    }
}
