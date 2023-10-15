using GoogleHelper.Json;
using GoogleHelper.Services;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Models.Devices;
using System.Text.Json.Nodes;

namespace SurePet2Google.Blazor.Server.Services.Devices
{
    public sealed class SmartHubService : BaseDeviceService<PetContext, HubModel>
    {
        private SurePetService SurePetService { get; set; }

        private GoogleService<PetContext> GoogleService { get; set; }

        private IConfiguration Configuration { get; set; }

        public SmartHubService(SurePetService surePetService, GoogleService<PetContext> googleService, IConfiguration configuration)
        {
            this.SurePetService = surePetService;
            this.GoogleService = googleService;
            this.Configuration = configuration;
        }

        public override async Task<TResponse> ExecuteAsyncImplementation<TResponse>(PetContext session, HubModel deviceModel, string deviceId, string requestId, JsonObject data, CancellationToken token)
        {
            return await Task.Run(() => new TResponse());
        }

        public override Task<bool> FetchAsyncImplementation(PetContext session, HubModel deviceModel, string deviceId, bool forceFetch = false)
        {
            return Task.FromResult(false);
        }

        public override async Task<TResponse> QueryAsyncImplementation<TResponse>(PetContext session, HubModel deviceModel, string deviceId, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                return (TResponse)(QueryDeviceData)new LockDeviceData()
                {
                    online = true,
                    status = "SUCCESS"
                };
            });
        }

        // https://developers.home.google.com/cloud-to-cloud/guides/camera#response

        private string GetCameraStreamUrl()
        {
            // TODO: make stream service
            return string.Empty;
        }
    }
}
