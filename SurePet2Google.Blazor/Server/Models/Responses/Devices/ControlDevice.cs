using System.Text.Json.Nodes;

namespace SurePet2Google.Blazor.Server.Models.Responses.Devices
{
    public class GetDevice
    {
        public JsonObject? data { get; set; }
    }

    public class ControlDevice
    {
        public JsonObject? data { get; set; }
        //public Result[] results { get; set; }
    }

    public class Result
    {
        public string? requestId { get; set; }
        public string? responseId { get; set; }
        public int status { get; set; }
        public int timeToSend { get; set; }
        public int timeToRespond { get; set; }
        public JsonObject? data { get; set; }
    }
}
