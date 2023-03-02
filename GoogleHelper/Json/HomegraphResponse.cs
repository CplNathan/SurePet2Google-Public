using System.Text.Json.Nodes;

namespace GoogleHelper.Json
{

    public class HomegraphResponse
    {
        public string? agentUserId { get; set; }
        public string? eventId { get; set; }
        public string? requestId { get; set; }
        public Payload? payload { get; set; }
    }

    public class Payload
    {
        public Devices? devices { get; set; }
    }

    public class Devices
    {
        public JsonObject? notifications { get; set; }
    }
}
