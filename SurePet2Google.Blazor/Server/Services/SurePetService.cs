using Flurl;
using Flurl.Http;
using GoogleHelper.Models;
using GoogleHelper.Services;
using SurePet2Google.Blazor.Server.Models.SurePet.API.Devices;
using SurePet2Google.Blazor.Server.Models.SurePet.API.Pets;
using SurePet2Google.Blazor.Server.Models.SurePet.Auth;

namespace SurePet2Google.Blazor.Server.Services
{
    public enum LockStatus
    {
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
        public static string PositionEndpoint = "pet/{pet}/position";
        public static string ControlEndpoint = "device/{device}/control";
        public static string StatusEndpoint = "device/{device}/status";

        public async Task<string?> AuthenticateWithCredentials(string username, string password, CancellationToken cancellationToken)
        {
            try
            {
                AuthResponse response = await BaseUrl
                    .AppendPathSegment(AuthEndpoint)
                    .PostUrlEncodedAsync(
                        new Dictionary<string, string>()
                        {
                            { "email_address", username },
                            { "password", password },
                            { "device_id", Random.Shared.NextInt64().ToString() }
                        }, cancellationToken: cancellationToken)
                    .ReceiveJson<AuthResponse>();

                return response.data.token;
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<GetPosition?> GetPosition(string bearer, string petId, CancellationToken cancellationToken)
        {
            try
            {
                GetPosition response = await BaseUrl
                    .AppendPathSegment(PositionEndpoint.Replace("{pet}", petId))
                    .WithOAuthBearerToken(bearer)
                    .GetJsonAsync<GetPosition>(cancellationToken: cancellationToken);

                return response;
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<GetPets?> GetPets(string bearer, CancellationToken cancellationToken)
        {
            try
            {
                GetPets response = await BaseUrl
                    .AppendPathSegment(PetEndpoint)
                    .SetQueryParams(new Dictionary<string, string>()
                    {
                        { "with", "[photo,breed,conditions,tag,food_type,species,position,status]" }
                    })
                    .WithOAuthBearerToken(bearer)
                    .GetJsonAsync<GetPets>(cancellationToken: cancellationToken);

                return response;
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<GetDevices?> GetDevices(string bearer, CancellationToken cancellationToken)
        {
            try
            {
                GetDevices response = await BaseUrl
                    .AppendPathSegment(DevicesEndpoint)
                    .SetQueryParams(new Dictionary<string, string>()
                    {
                        { "with", "[children,tags,control,status]" }
                    })
                    .WithOAuthBearerToken(bearer)
                    .GetJsonAsync<GetDevices>(cancellationToken: cancellationToken);

                return response;
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<LockStatus?> GetLock(string bearer, string deviceId, CancellationToken cancellationToken)
        {
            try
            {
                GetDevice response = await BaseUrl
                    .AppendPathSegment(StatusEndpoint.Replace("{device}", deviceId))
                    .WithOAuthBearerToken(bearer)
                    .GetJsonAsync<GetDevice>(cancellationToken: cancellationToken);

                int? lockData = response.data["locking"]?["mode"]?.GetValue<int?>();

                return (LockStatus?)lockData ?? null;
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<double?> GetBattery(string bearer, string deviceId, CancellationToken cancellationToken)
        {
            try
            {
                GetDevice response = await BaseUrl
                    .AppendPathSegment(StatusEndpoint.Replace("{device}", deviceId))
                    .WithOAuthBearerToken(bearer)
                    .GetJsonAsync<GetDevice>(cancellationToken: cancellationToken);

                return response.data["battery"]?.GetValue<double?>();
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<bool?> GetOnline(string bearer, string deviceId, CancellationToken cancellationToken)
        {
            try
            {
                GetDevice response = await BaseUrl
                    .AppendPathSegment(StatusEndpoint.Replace("{device}", deviceId))
                    .WithOAuthBearerToken(bearer)
                    .GetJsonAsync<GetDevice>(cancellationToken: cancellationToken);

                return response.data["online"]?.GetValue<bool?>();
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<LockStatus?> UpdateLock(string bearer, string deviceId, LockStatus newStatus, CancellationToken cancellationToken)
        {
            try
            {
                ControlDevice response = await BaseUrl
                    .AppendPathSegment(ControlEndpoint.Replace("{device}", deviceId))
                    .WithOAuthBearerToken(bearer)
                    .PutJsonAsync(
                        new Dictionary<string, string>()
                        {
                            { "locking", ((int)newStatus).ToString() },
                        }, cancellationToken: cancellationToken)
                    .ReceiveJson<ControlDevice>();

                int? lockData = response.data["locking"]?.GetValue<int?>();

                return (LockStatus?)lockData ?? null;
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public Dictionary<string, BaseDeviceModel> ParseDevices(GetDevices devices, IEnumerable<IDeviceService> supportedDevices)
        {
            Dictionary<string, BaseDeviceModel> parsedDevices = new();

            foreach (Datum device in devices.data)
            {
                string productId = device.product_id.ToString();
                IDeviceService? supportedDevice = supportedDevices.FirstOrDefault(model => model.ModelIdentifiers.Contains(productId));
                if (supportedDevice == null)
                {
                    continue;
                }

                string deviceId = device.id.ToString();

                BaseDeviceModel? newDevice = (BaseDeviceModel)Activator.CreateInstance(supportedDevice.ModelType);
                newDevice.Name = device.name;
                newDevice.HardwareVersion = device.version;

                parsedDevices.Add(deviceId, newDevice);
            }

            return parsedDevices;
        }
    }
}
