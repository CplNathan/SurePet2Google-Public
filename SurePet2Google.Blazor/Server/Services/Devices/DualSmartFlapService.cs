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
            LockStatus lockRequest = data["lock"]?.GetValue<bool?>() ?? true ? LockStatus.EnterOnly : LockStatus.Unlocked;
            string? followUpToken = data["followUpToken"]?.GetValue<string?>();

            var deviceStatus = await this.SurePetService.GetStatus(session.SurePetBearerToken, deviceId, token);
            if (deviceStatus.lockStatus is LockStatus.Unknown)
            {
                return (TResponse)(ExecuteDeviceData)new ExecuteDeviceDataError()
                {
                    status = "FAILURE",
                    errorCode = "relinkRequired"
                };
            }
            else if (deviceStatus.lockStatus == lockRequest)
            {
                return (TResponse)(ExecuteDeviceData)new ExecuteDeviceDataError()
                {
                    status = "FAILURE",
                    errorCode = (deviceStatus.lockStatus is LockStatus.EnterOnly or LockStatus.Locked) ? "alreadyLocked" : "alreadyUnlocked"
                };
            }

            if (deviceStatus.onlineStatus == false)
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

            Task<LockStatus> lockExecute = Task.FromResult(LockStatus.Unknown);
            if (!token.IsCancellationRequested)
            {
                lockExecute = this.SurePetService.UpdateLock(session.SurePetBearerToken, deviceId, lockRequest, token);
            }

            if (!string.IsNullOrEmpty(followUpToken))
            {
                var followUpTask = Task.Run(async () =>
                {
                    var lockExecuteResult = await lockExecute;

                    JsonObject followUpData = new JsonObject()
                    {
                            { "priority", 0 },
                            {
                                "followUpResponse",
                                lockExecuteResult is LockStatus.Unknown ? new JsonObject()
                                {
                                    { "status", "FAILURE" },
                                    { "errorCode", "deviceOffline" },
                                    { "followUpToken", followUpToken }
                                } : new JsonObject()
                                {
                                    { "status", "SUCCESS" },
                                    { "isLocked", (await lockExecute) is LockStatus.EnterOnly or LockStatus.Locked },
                                    { "followUpToken",  followUpToken }
                                }
                            }
                    };

                    await this.GoogleService.ProvideFollowUp(this.Configuration["Google:Homegraph:private_key"], this.Configuration["Google:Homegraph:private_key_id"], this.Configuration["Google:Homegraph:client_email"], session.GoogleAccessToken, requestId, deviceId, "LockUnlock", followUpData, token);
                });

                var completedInTime = followUpTask.Wait(2500);

                if (!completedInTime)
                {
                    return (TResponse)new ExecuteDeviceData()
                    {
                        status = "PENDING",
                        states = new JsonObject()
                        {
                            { "online", deviceStatus.onlineStatus },
                            { "isLocked", deviceStatus.lockStatus is LockStatus.EnterOnly or LockStatus.Locked },
                            { "isJammed", false },
                            { "descriptiveCapacityRemaining", this.GetDescriptiveBattery(deviceStatus.batteryStatus, 4) },
                        }
                    };
                }
            }

            return (TResponse)new ExecuteDeviceData()
            {
                status = "SUCCESS",
                states = new JsonObject()
                {
                    { "online", deviceStatus.onlineStatus },
                    { "isLocked", (await lockExecute) is LockStatus.EnterOnly or LockStatus.Locked },
                    { "isJammed", false },
                    { "descriptiveCapacityRemaining", this.GetDescriptiveBattery(deviceStatus.batteryStatus, 4) },
                }
            };
        }

        public override Task<bool> FetchAsyncImplementation(PetContext session, FlapModel deviceModel, string deviceId, bool forceFetch = false)
        {
            throw new NotImplementedException();
        }

        public override async Task<TResponse> QueryAsyncImplementation<TResponse>(PetContext session, FlapModel deviceModel, string deviceId, CancellationToken token)
        {
            CancellationTokenSource cancellationToken = new();

            var deviceStatus = await this.SurePetService.GetStatus(session.SurePetBearerToken, deviceId, token);

            return (TResponse)(QueryDeviceData)new LockDeviceData()
            {
                online = deviceStatus.onlineStatus,
                status = "SUCCESS",
                isJammed = false,
                isLocked = deviceStatus.lockStatus is LockStatus.Locked or LockStatus.EnterOnly,
                descriptiveCapacityRemaining = this.GetDescriptiveBattery(deviceStatus.batteryStatus, 4)
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
