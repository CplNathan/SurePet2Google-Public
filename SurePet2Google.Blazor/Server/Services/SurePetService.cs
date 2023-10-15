using Flurl;
using Flurl.Http;
using GoogleHelper.Models;
using GoogleHelper.Services;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Models.Responses.Auth;
using SurePet2Google.Blazor.Server.Models.Responses.Devices;
using SurePet2Google.Blazor.Server.Models.Responses.Pets;
using SurePet2Google.Blazor.Server.Models.Responses.Timeline;

namespace SurePet2Google.Blazor.Server.Services
{
    public enum LockStatus
    {
        Unknown = -1,
        Unlocked = 0,
        EnterOnly = 1,
        ExitOnly = 2,
        Locked = 3
    }

    public class SurePetService
    {
        public static string BaseUrl = "https://app.api.surehub.io/api/";

        public static string AuthEndpoint = "auth/login";
        public static string DevicesEndpoint = "device";
        public static string PetEndpoint = "pet";
        public static string TimelineEndpoint = "timeline";
        public static string PositionEndpoint = "pet/{pet}/position";
        public static string ControlEndpoint = "device/{device}/control";
        public static string StatusEndpoint = "device/{device}/status";

        private IFlurlRequest MakeRequest(string request, string endpoint, string bearer = "")
        {
            IFlurlRequest flurlRequest = request
                .AppendPathSegment(endpoint)
                .WithOAuthBearerToken(bearer);

            return flurlRequest;
        }

        public async Task<string?> AuthenticateWithCredentials(string username, string password, CancellationToken cancellationToken)
        {
            AuthResponse response = await this
                .MakeRequest(BaseUrl, AuthEndpoint)
                .PostUrlEncodedAsync(
                    new Dictionary<string, string>()
                    {
                        { "email_address", username },
                        { "password", password },
                        { "device_id", Random.Shared.NextInt64().ToString() }
                    }, cancellationToken: cancellationToken)
                .ReceiveJson<AuthResponse>();

            return response?.data?.token ?? string.Empty;
        }

        public async Task<GetPosition?> GetPosition(string bearer, string petId, CancellationToken cancellationToken)
        {
            GetPosition response = await this
                .MakeRequest(BaseUrl, PositionEndpoint.Replace("{pet}", petId), bearer)
                .GetJsonAsync<GetPosition>(cancellationToken: cancellationToken);

            return response;
        }

        public async Task<GetPets?> GetPets(string bearer, CancellationToken cancellationToken)
            => await this.GetPets(bearer, cancellationToken, "photo", "breed", "conditions", "tag", "food_type", "species", "position", "status");

        public async Task<GetPets?> GetPets(string bearer, CancellationToken cancellationToken, params string[] flags)
        {
            GetPets response = await this
                .MakeRequest(BaseUrl, PetEndpoint, bearer)
                .SetQueryParams(new Dictionary<string, string>()
                {
                    { "with", $"[{string.Join(",", flags)}]" }
                })
                .GetJsonAsync<GetPets>(cancellationToken: cancellationToken);

            return response;
        }

        public async Task<GetDevices?> GetDevices(string bearer, CancellationToken cancellationToken)
        {
            GetDevices response = await this
                .MakeRequest(BaseUrl, DevicesEndpoint, bearer)
                .SetQueryParams(new Dictionary<string, string>()
                {
                    { "with", "[children,tags,control,status]" }
                })
                .GetJsonAsync<GetDevices>(cancellationToken: cancellationToken);

            return response;
        }

        public async Task<(LockStatus lockStatus, double? batteryStatus, bool onlineStatus)> GetStatus(string bearer, string deviceId, CancellationToken cancellationToken)
        {
            try
            {
                GetDevice response = await this
                    .MakeRequest(BaseUrl, StatusEndpoint.Replace("{device}", deviceId), bearer)
                    .GetJsonAsync<GetDevice>(cancellationToken: cancellationToken);

                int? lockData = response?.data?["locking"]?["mode"]?.GetValue<int?>();
                double? batteryData = response?.data?["battery"]?.GetValue<double?>();
                bool onlineData = response?.data?["online"]?.GetValue<bool?>() ?? false;

                return ((LockStatus)(lockData ?? -1), batteryData, onlineData);
            }
            catch
            {
                return (LockStatus.Unknown, null, false);
            }
        }

        public async Task<GetTimeline?> GetTimeline(string bearer, CancellationToken cancellationToken)
        {
            var response = await GlobalHttpContext.BuildRetryPolicy().ExecuteAsync(async () =>
            {
                return (await this
                    .MakeRequest(BaseUrl, TimelineEndpoint, bearer)
                    .GetAsync()).ResponseMessage;
            });

            return await response.Content.ReadFromJsonAsync<GetTimeline>();
        }

        public async Task<LockStatus> UpdateLock(string bearer, string deviceId, LockStatus newStatus, CancellationToken cancellationToken)
        {
            ControlDevice response = await this
                .MakeRequest(BaseUrl, ControlEndpoint.Replace("{device}", deviceId), bearer)
                .PutJsonAsync(
                    new Dictionary<string, string>()
                    {
                        { "locking", ((int)newStatus).ToString() },
                    }, cancellationToken: cancellationToken)
                .ReceiveJson<ControlDevice>();

            int? lockData = response?.data?["locking"]?.GetValue<int?>();

            return ((LockStatus?)lockData) ?? LockStatus.Unknown;
        }

        public Dictionary<string, BaseDeviceModel> ParseDevices(GetDevices devices, IEnumerable<IDeviceService> supportedDevices)
        {
            Dictionary<string, BaseDeviceModel> parsedDevices = new();

            foreach (Datum device in devices.data ?? Enumerable.Empty<Datum>())
            {
                string productId = device.product_id.ToString();
                IDeviceService? supportedDevice = supportedDevices.FirstOrDefault(model => model.ModelIdentifiers.Contains(productId));
                if (supportedDevice == null)
                {
                    Console.WriteLine($"Unable to create device, unsupported type for product {device.product_id}");
                    continue;
                }

                string deviceId = device.id.ToString();

                BaseDeviceModel? newDevice = (BaseDeviceModel?)Activator.CreateInstance(supportedDevice.ModelType);
                if (newDevice == null)
                {
                    Console.WriteLine($"Unable to create device, can not initialize type for {supportedDevice.ModelType}");
                    continue;
                }

                newDevice.Name = device.name ?? string.Empty;
                newDevice.HardwareVersion = device.version ?? "Unknown";

                parsedDevices.Add(deviceId, newDevice);
            }

            return parsedDevices;
        }
    }
}
