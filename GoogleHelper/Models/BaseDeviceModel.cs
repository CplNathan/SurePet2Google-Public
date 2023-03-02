// Copyright (c) Nathan Ford. All rights reserved. BaseDeviceModel.cs

using GoogleHelper.Json;
using System.Text.Json.Nodes;

namespace GoogleHelper.Models
{
    public class BaseDeviceQuery
    {
        public BaseDeviceQuery() { }

        public string DeviceId { get; set; }
    }

    public abstract class BaseDeviceModel
    {
        public BaseDeviceModel() { }

        public List<string> ModelIdentifiers { get; set; }

        protected string type { get; set; } = string.Empty;

        public string Type
        {
            get => $"action.devices.types.{this.type}";
            set => this.type = value;
        }

        protected List<string> traits { get; set; } = new();

        public List<string> Traits => this.traits.Select(trait => $"action.devices.traits.{trait}").ToList();

        public string Name { get; set; } = string.Empty;

        public List<string> AlternativeNames { get; set; }

        public string HardwareVersion { get; set; } = string.Empty;

        public bool WillReportState { get; set; }

        public bool NotificationSupportedByAgent { get; set; }

        public JsonObject Attributes { get; set; }

        private List<string> _supportedCommands { get; } = new();

        public List<string> SupportedCommands
        {
            get => this._supportedCommands.Select(command => $"action.devices.commands.{command}").ToList();
            set
            {
                this._supportedCommands.Clear();
                this._supportedCommands.AddRange(value);
            }
        }

        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

        public bool WillFetch => DateTime.UtcNow - this.LastUpdated > TimeSpan.FromSeconds(10);

        public SyncResponse Sync(string Id)
        {
            return new()
            {
                id = Id,
                type = Type,
                traits = this.Traits.ToArray(),
                name = new Name()
                {
                    name = Name,
                    defaultNames = AlternativeNames,
                    nicknames = AlternativeNames
                },
                willReportState = WillReportState,
                notificationSupportedByAgent = NotificationSupportedByAgent,
                attributes = Attributes,
                deviceInfo = new Deviceinfo()
                {
                    manufacturer = "Nathan SurePet2Google",
                    model = "SurePet",
                    hwVersion = HardwareVersion,
                    swVersion = "1.0"
                }
            };
        }
    }
}