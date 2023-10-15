// Copyright (c) Nathan Ford. All rights reserved. Class1.cs

using Flurl;
using Flurl.Http;
using GoogleHelper.Context;
using GoogleHelper.Json;
using GoogleHelper.Models;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text.Json.Nodes;

namespace GoogleHelper.Services
{
    public class GoogleService<TContext>
        where TContext : BaseContext
    {
        private string BuildSignedJWT(string privateKey, string privateKeyId, string clientEmail, string agentUserId)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>()
            {
                { "iss", clientEmail },
                { "sub", clientEmail },
                { "aud", "https://homegraph.googleapis.com/" },
                { "iat", (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds },
                { "exp", (int)(DateTime.UtcNow - DateTime.UnixEpoch + TimeSpan.FromSeconds(3600)).TotalSeconds }
            };

            Dictionary<string, object> headers = new Dictionary<string, object>()
            {
                { "alg", "RS256" },
                { "typ", "JWT" },
                { "kid", privateKeyId }
            };

            RSAParameters rsaParams;
            using (StringReader tr = new StringReader(privateKey))
            {
                PemReader pemReader = new PemReader(tr);
                if (pemReader.ReadObject() is not RsaPrivateCrtKeyParameters privateRsaParams)
                {
                    throw new Exception("Could not read RSA private key");
                }

                rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
            }

            string token = string.Empty;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaParams);
                token = Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256, headers);
            }

            return token;
        }

        public async Task ProvideFollowUp(string privateKey, string privateKeyId, string clientEmail, string agentUserId, string requestId, string deviceId, string deviceAction, JsonObject data, CancellationToken token)
        {
            string authToken = this.BuildSignedJWT(privateKey, privateKeyId, clientEmail, agentUserId);

            try
            {
                IFlurlResponse unused = await "https://homegraph.googleapis.com/v1/devices:reportStateAndNotification"
                    .WithOAuthBearerToken(authToken)
                    .PostJsonAsync(new HomegraphResponse()
                    {
                        agentUserId = agentUserId,
                        requestId = requestId,
                        eventId = Guid.NewGuid().ToString(),
                        payload = new Payload()
                        {
                            devices = new Devices()
                            {
                                notifications = new JsonObject()
                                {
                                    {
                                        deviceId, new JsonObject()
                                        {
                                            {
                                                deviceAction, data
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task ProvideObjectDetection(string privateKey, string privateKeyId, string clientEmail, string agentUserId, string deviceId, string objectName, CancellationToken token)
        {
            string authToken = this.BuildSignedJWT(privateKey, privateKeyId, clientEmail, agentUserId);

            try
            {
                IFlurlResponse unused = await "https://homegraph.googleapis.com/v1/devices:reportStateAndNotification"
                    .WithOAuthBearerToken(authToken)
                    .PostJsonAsync(new HomegraphResponse()
                    {
                        agentUserId = agentUserId,
                        requestId = Guid.NewGuid().ToString(),
                        eventId = Guid.NewGuid().ToString(),
                        payload = new Payload()
                        {
                            devices = new Devices()
                            {
                                notifications = new JsonObject()
                                {
                                    {
                                        deviceId, new JsonObject()
                                        {
                                            {
                                                "ObjectDetection", new JsonObject()
                                                {
                                                    { "priority", 0 },
                                                    { "detectionTimestamp", (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds },
                                                    { "objects", new JsonObject()
                                                        {
                                                            { "named", new JsonArray()
                                                                { objectName }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task<GoogleIntentResponse> HandleGoogleResponse(TContext? context, GoogleIntentRequest request, IEnumerable<IDeviceService> supportedDevices, string sessionId, CancellationToken token)
        {
            if (context is null)
            {
                throw new InvalidOperationException("Invalid context provided to Google handler.");
            }

            GoogleIntentResponse response = new(request);

            foreach (Input action in request?.inputs ?? Array.Empty<Input>())
            {
                string[] intent = (action.intent ?? string.Empty).Split("action.devices.");

                switch (intent.ElementAtOrDefault(1))
                {
                    case "SYNC":
                        {
                            List<SyncResponse> deviceList = context.Devices.Select(device => device.Value.Sync(device.Key)).ToList();

                            response.payload = new SyncPayload()
                            {
                                devices = deviceList,
                                agentUserId = sessionId
                            };

                            break;
                        }

                    case "QUERY":
                        {
                            IEnumerable<IGrouping<IDeviceService, KeyValuePair<string, BaseDeviceModel>>> groupedServiceModels = this.GroupedSupportedDevices(context, supportedDevices, action.payload?.devices);

                            Dictionary<string, Task<QueryDeviceData>> deviceQueryTasks = groupedServiceModels.SelectMany(gp => gp.Select(device => (device.Key, gp.Key.QueryAsync<QueryDeviceData>(context, device.Value, device.Key, token))))
                                .ToDictionary(item => item.Key, item => item.Item2);

                            IEnumerable<(string First, QueryDeviceData Second)> deviceQueryResults = deviceQueryTasks.Keys.Zip(await Task.WhenAll(deviceQueryTasks.Values));

                            JsonObject deviceQuery = new();
                            foreach ((string First, QueryDeviceData Second) in deviceQueryResults)
                            {
                                deviceQuery.Add(First, JsonValue.Create(Second));
                            }

                            response.payload = new QueryPayload()
                            {
                                devices = deviceQuery,
                                agentUserId = sessionId
                            };

                            break;
                        }
                    case "EXECUTE":
                        {
                            List<ExecuteDeviceData> executedCommands = new();
                            string? errorCode = null;
                            string? debugMessage = null;

                            try
                            {
                                foreach (Command? command in action.payload?.commands ?? Array.Empty<Command>())
                                {
                                    IEnumerable<IGrouping<IDeviceService, KeyValuePair<string, BaseDeviceModel>>> groupedServiceModels = this.GroupedSupportedDevices(context, supportedDevices, command?.devices);

                                    foreach (Execution execution in command?.execution ?? Enumerable.Empty<Execution>())
                                    {
                                        List<string> updatedIds = new();

                                        // string parsedCommand = execution.command.Split("action.devices.commands.")[1];

                                        Dictionary<string, Task<ExecuteDeviceData>> deviceExecuteTasks = groupedServiceModels.SelectMany(gp => gp.Select(device => (device.Key, gp.Key.ExecuteAsync<ExecuteDeviceData>(context, device.Value, device.Key, request.requestId/*parsedCommand*/, execution._params, token))))
                                            .ToDictionary(item => item.Key, item => item.Item2);

                                        IEnumerable<(string deviceKey, ExecuteDeviceData deviceData)> deviceExecuteResults = deviceExecuteTasks.Keys.Zip(await Task.WhenAll(deviceExecuteTasks.Values));

                                        foreach ((string deviceKey, ExecuteDeviceData deviceData) in deviceExecuteResults)
                                        {
                                            ExecuteDeviceData state = deviceData;
                                            state.ids = new List<string>() { deviceKey };

                                            executedCommands.Add(state);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                errorCode = "relinkRequired";
                                debugMessage = ex.ToString();
                            }

                            if (executedCommands.Count <= 0)
                            {
                                errorCode = "deviceNotFound";
                            }

                            response.payload = new ExecutePayload()
                            {
                                errorCode = errorCode,
                                debugString = debugMessage,
                                commands = executedCommands,
                                agentUserId = sessionId
                            };

                            break;
                        }
                }
            }

            return response;
        }

        private IEnumerable<IGrouping<IDeviceService, KeyValuePair<string, BaseDeviceModel>>> GroupedSupportedDevices(TContext context, IEnumerable<IDeviceService> supportedDevices, RequestDevice[]? requestDevices)
        {
            Dictionary<string, BaseDeviceModel> devices = context?.Devices ?? new();

            IEnumerable<string?> requestedDeviceIds = (requestDevices ?? Enumerable.Empty<RequestDevice>()).Select(device => device.id);
            IEnumerable<KeyValuePair<string, BaseDeviceModel>> requestedDeviceModels = devices.Where(device => requestedDeviceIds.Contains(device.Key));
            IEnumerable<IGrouping<IDeviceService, KeyValuePair<string, BaseDeviceModel>>> groupedServiceModels = requestedDeviceModels.GroupBy(device => supportedDevices.First(service => device.Value.GetType() == service.ModelType));

            return groupedServiceModels;
        }
    }
}
