using GoogleHelper.Services;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Services;
using SurePet2Google.Blazor.Server.Services.Devices;

namespace SurePet2Google.Blazor.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Configuration.AddEnvironmentVariables();

            builder.WebHost.UseUrls(builder.Configuration["APPLICATION_URL"] ?? string.Empty);

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddLogging();

            builder.Services.AddScoped(typeof(IDeviceService), typeof(DualSmartFlapService));
            builder.Services.AddScoped(typeof(IDeviceService), typeof(SmartHubService));

            builder.Services.AddSingleton<GoogleService<PetContext>>();
            builder.Services.AddSingleton<SurePetService>();

            builder.Services.AddSingleton<PersistenceService>();
            builder.Services.AddSingleton<NotificationService>();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
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
            NotificationService? notificationService = serviceProvider.GetService<NotificationService>();

            notificationService?.StartLoop();

            app.Run();
        }
    }
}
