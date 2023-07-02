using GoogleHelper.Services;
using SurePet2Google.Blazor.Server.Context;

namespace SurePet2Google.Blazor.Server.Services.Notifications
{
    public interface INotificationService
    {
        void StartNotifications();
    }

    public abstract class NotificationServiceBase : INotificationService, IDisposable
    {
        protected SurePetService SurePetService { get; set; }

        protected GoogleService<PetContext> GoogleService { get; set; }

        protected PersistenceService PersistenceService { get; set; }

        protected IConfiguration Configuration { get; set; }

        public NotificationServiceBase(SurePetService surePetService, GoogleService<PetContext> googleService, PersistenceService persistenceService, IConfiguration configuration)
        {
            this.SurePetService = surePetService;
            this.GoogleService = googleService;
            this.PersistenceService = persistenceService;
            this.Configuration = configuration;
            this.cancellationTokenSource = new CancellationTokenSource();
            this.notificationThread = new Thread(new ThreadStart(async () =>
            {
                while (true)
                {
                    await this.DoNotifications();

                    await Task.Delay(5000, this.cancellationTokenSource.Token);
                    if (this.cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }));
        }

        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly Thread notificationThread;

        protected CancellationToken cancellationToken { get => cancellationTokenSource.Token; }

        protected abstract Task DoNotifications();

        public void StartNotifications()
        {
            if (!this.notificationThread.ThreadState.HasFlag(ThreadState.Unstarted))
            {
                return;
            }

            this.notificationThread.Start();
        }

        public void Dispose()
        {
            this.cancellationTokenSource.Cancel();
        }
    }
}
