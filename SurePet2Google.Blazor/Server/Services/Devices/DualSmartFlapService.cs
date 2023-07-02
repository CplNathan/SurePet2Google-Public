using GoogleHelper.Json;
using GoogleHelper.Services;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Models.Devices;
using System.Text.Json.Nodes;

namespace SurePet2Google.Blazor.Server.Services.Devices
{
    public sealed class DualSmartFlapService : BaseDeviceService<PetContext, FlapModel>
    {
        private SurePetService SurePetService { get; set; }

        private GoogleService<PetContext> GoogleService { get; set; }

        private IConfiguration Configuration { get; set; }

        public DualSmartFlapService(SurePetService surePetService, GoogleService<PetContext> googleService, IConfiguration configuration)
        {
            this.SurePetService = surePetService;
            this.GoogleService = googleService;
            this.Configuration = configuration;
        }

        // THIS WAS HELLLLLLLLLLLLL ON EARTH BLOODY FIRE TO DEBUG WOW.
        public override async Task<TResponse> ExecuteAsyncImplementation<TResponse>(PetContext session, FlapModel deviceModel, string deviceId, string requestId, JsonObject data, CancellationToken token)
        {
            CancellationTokenSource cancellationToken = new();

            LockStatus lockRequest = data["lock"]?.GetValue<bool?>() ?? true ? LockStatus.EnterOnly : LockStatus.Unlocked;
            string? followUpToken = data["followUpToken"]?.GetValue<string?>();

            Task<LockStatus?> lockExecute = this.SurePetService.UpdateLock(session.SurePetBearerToken, deviceId, lockRequest, token);
            Task<double?> batteryQuery = this.SurePetService.GetBattery(session.SurePetBearerToken, deviceId, token);
            Task<bool?> onlineQuery = this.SurePetService.GetOnline(session.SurePetBearerToken, deviceId, token);
            Task<LockStatus?> lockQuery = this.SurePetService.GetLock(session.SurePetBearerToken, deviceId, token);

            if (lockQuery is null)
            {
                return (TResponse)(ExecuteDeviceData)new ExecuteDeviceDataError()
                {
                    status = "FAILURE",
                    errorCode = "relinkRequired"
                };
            }
            else if ((await lockQuery) == lockRequest)
            {
                return (TResponse)(ExecuteDeviceData)new ExecuteDeviceDataError()
                {
                    status = "FAILURE",
                    errorCode = (await lockQuery is LockStatus.EnterOnly or LockStatus.Locked) ? "alreadyLocked" : "alreadyUnlocked"
                };
            }
            else
            {
                bool allCompleted = Task.WaitAll(new Task[] { batteryQuery, lockExecute, lockQuery, onlineQuery }, 2500);

                if ((await onlineQuery) == false)
                {
                    return (TResponse)(ExecuteDeviceData)new ExecuteDeviceDataError()
                    {
                        status = "FAILURE",
                        states = new JsonObject()
                        {
                            { "online", false },
                        },
                        errorCode = "deviceOffline"
                    };
                }
                else if (followUpToken is not null && !allCompleted)
                {
                    new Thread(async () =>
                    {
                        var completedSuccess = Task.WaitAll(new Task[] { lockExecute }, 10000);

                        JsonObject followUpData = new JsonObject()
                        {
                            { "priority", 0 },
                            {
                                "followUpResponse",
                                !completedSuccess ? new JsonObject()
                                {
                                    { "status", "FAILURE" },
                                    { "errorCode", "deviceOffline" },
                                    { "followUpToken", followUpToken }
                                } : new JsonObject()
                                {
                                    { "status", "SUCCESS" },
                                    { "isLocked", await lockExecute == LockStatus.EnterOnly || await lockExecute == LockStatus.Locked },
                                    { "followUpToken",  followUpToken }
                                }
                            }
                        };

                        await this.GoogleService.ProvideFollowUp(this.Configuration["Google:Homegraph:private_key"], this.Configuration["Google:Homegraph:private_key_id"], this.Configuration["Google:Homegraph:client_email"], session.GoogleAccessToken, requestId, deviceId, "LockUnlock", followUpData);
                    }).Start();

                    return (TResponse)new ExecuteDeviceData()
                    {
                        status = "PENDING",
                        states = new JsonObject()
                        {
                            { "online", await lockQuery != null },
                            { "isLocked", await lockQuery == LockStatus.EnterOnly || await lockQuery == LockStatus.Locked },
                            { "isJammed", false },
                            { "descriptiveCapacityRemaining", this.GetDescriptiveBattery(await batteryQuery, 4) },
                        }
                    };
                }
                else
                {
                    return (TResponse)new ExecuteDeviceData()
                    {
                        status = "SUCCESS",
                        states = new JsonObject()
                        {
                            { "online", await lockExecute != null },
                            { "isLocked", await lockExecute == LockStatus.EnterOnly || await lockExecute == LockStatus.Locked },
                            { "isJammed", false },
                            { "descriptiveCapacityRemaining", this.GetDescriptiveBattery(await batteryQuery, 4) },
                        }
                    };
                }
            }
        }

        public override Task<bool> FetchAsyncImplementation(PetContext session, FlapModel deviceModel, string deviceId, bool forceFetch = false)
        {
            throw new NotImplementedException();
        }

        public override async Task<TResponse> QueryAsyncImplementation<TResponse>(PetContext session, FlapModel deviceModel, string deviceId)
        {
            CancellationTokenSource cancellationToken = new();

            Task<LockStatus?> lockResult = this.SurePetService.GetLock(session.SurePetBearerToken, deviceId, cancellationToken.Token);
            Task<double?> batteryResult = this.SurePetService.GetBattery(session.SurePetBearerToken, deviceId, cancellationToken.Token);

            Task.WaitAll(lockResult, batteryResult);

            return (TResponse)(QueryDeviceData)new LockDeviceData()
            {
                online = await lockResult != null,
                status = "SUCCESS",
                isJammed = false,
                isLocked = await lockResult is LockStatus.Locked or LockStatus.EnterOnly,
                descriptiveCapacityRemaining = this.GetDescriptiveBattery(await batteryResult, 4)
            };
        }

        private string GetDescriptiveBattery(double? voltage, int batteries)
        {
            double fullVoltage = 1.5d * batteries;
            double deadVoltage = 1.2d * batteries;

            double percentage = ((voltage ?? fullVoltage) - deadVoltage) / (fullVoltage - deadVoltage);

            List<string> descriptions = new()
            {
                "CRITICALLY_LOW",
                "LOW",
                "MEDIUM",
                "HIGH",
                "FULL"
            };

            int description = (int)(percentage * (descriptions.Count - 1));

            return descriptions[description];
        }
    }
}
