using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GoogleHelper.Json
{
    public class SyncResponse
    {
        // TODO: Convert these into nice names, decide on a naming convention and enforce it... then assign JsonProperty names for funky ones.
        public string? id { get; set; }
        public string? type { get; set; }
        public string[]? traits { get; set; }
        public Name? name { get; set; }
        public bool willReportState { get; set; }
        public bool notificationSupportedByAgent { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonObject? attributes { get; set; }
        public Deviceinfo? deviceInfo { get; set; }
    }

    public class Name
    {
        public string? name { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? nicknames { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? defaultNames { get; set; }
    }

    public class Deviceinfo
    {
        public string? manufacturer { get; set; }
        public string? model { get; set; }
        public string? hwVersion { get; set; }
        public string? swVersion { get; set; }
    }

}
