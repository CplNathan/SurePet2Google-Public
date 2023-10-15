// Copyright (c) Nathan Ford. All rights reserved. IntentResponse.cs

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GoogleHelper.Json
{

    public class GoogleIntentResponse
    {
        public GoogleIntentResponse(GoogleIntentRequest request)
        {
            this.requestId = request.requestId;
        }

        public string requestId { get; set; }

        public ResponsePayload payload { get; set; }
    }

    // Important for polymorphism
    [JsonDerivedType(typeof(SyncPayload))]
    [JsonDerivedType(typeof(QueryPayload))]
    [JsonDerivedType(typeof(ExecutePayload))]
    public class ResponsePayload
    {
        public string agentUserId { get; set; }
    }

    public class SyncPayload : ResponsePayload
    {
        public List<SyncResponse> devices { get; set; }
    }

    public class QueryPayload : ResponsePayload
    {
        public JsonObject devices { get; set; }
    }

    public class ExecutePayload : ResponsePayload
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? errorCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? debugString { get; set; }

        public List<ExecuteDeviceData> commands { get; set; }
    }

    // Important for polymorphism
    [JsonDerivedType(typeof(LockDeviceData))]
    public class QueryDeviceData
    {
        public string status { get; set; }
        public bool online { get; set; }
    }

    public class LockDeviceData : QueryDeviceData
    {
        public bool isLocked { get; set; }
        public bool isJammed { get; set; }
        public string descriptiveCapacityRemaining { get; set; }
    }

    [JsonDerivedType(typeof(ExecuteDeviceDataSuccess))]
    [JsonDerivedType(typeof(ExecuteDeviceDataError))]
    public class ExecuteDeviceData
    {
        public List<string> ids { get; set; }
        public string status { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonObject states { get; set; }
    }

    public class ExecuteDeviceDataSuccess : ExecuteDeviceData
    {
    }

    public class ExecuteDeviceDataError : ExecuteDeviceData
    {
        public ExecuteDeviceDataError() { }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string errorCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string errorCodeReason { get; set; }
    }
}
