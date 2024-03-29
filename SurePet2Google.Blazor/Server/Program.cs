using Flurl.Http;
using GoogleHelper.Services;
using SurePet2Google.Blazor.Client;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Services;
using SurePet2Google.Blazor.Server.Services.Devices;
using SurePet2Google.Blazor.Server.Services.Notifications;

namespace SurePet2Google.Blazor.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Configuration.AddEnvironmentVariables();

            //builder.WebHost.UseUrls(builder.Configuration["APPLICATION_URL"] ?? string.Empty);
            builder.WebHost.UseUrls("http://*:8080");

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddEndpointsApiExplorer();
            }
            builder.Services.AddLogging();

            builder.Services.AddScoped(typeof(IDeviceService), typeof(DualSmartFlapService));
            builder.Services.AddScoped(typeof(IDeviceService), typeof(SmartHubService));

            FlurlHttp.Configure(settings =>
            {
                Action<FlurlCall> beforeCall = (httpCall) =>
                {
                    httpCall.Request.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; SurePet2Google/ProdContainer; +github.com/CplNathan/SurePet2Google-Public)");
                };

                settings.BeforeCall = beforeCall;
                settings.BeforeCallAsync = async (httpCall) => await Task.Run(() => beforeCall.Invoke(httpCall));
                settings.FlurlClientFactory = new GlobalHttpContext();
            });

            builder.Services.AddSingleton<GoogleService<PetContext>>();
            builder.Services.AddSingleton<SurePetService>();

            builder.Services.AddSingleton<PersistenceService>();
            //builder.Services.AddSingleton<INotificationService, NotificationServiceV1>();
            builder.Services.AddSingleton<INotificationService, NotificationServiceV2>();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseAuthorization();

            app.UseRouting();

            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            IServiceProvider serviceProvider = app.Services;
            INotificationService? notificationService = serviceProvider.GetService<INotificationService>();

            notificationService?.StartNotifications();

            app.Run();
        }
    }
}
