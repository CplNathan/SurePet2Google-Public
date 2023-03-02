using GoogleHelper.Models;
using System.Text.Json.Nodes;

namespace SurePet2Google.Blazor.Server.Models
{
    public class FlapModel : BaseDeviceModel
    {
        public FlapModel()
            : base()
        {
            this.type = "LOCK";
            this.traits = new List<string> { "LockUnlock", "EnergyStorage", "ObjectDetection" };
            this.WillReportState = true;
            this.NotificationSupportedByAgent = true;
            this.SupportedCommands = new List<string> { "LockUnlock" };
            this.AlternativeNames = new List<string> { "Cat Flap" };
            this.ModelIdentifiers = new List<string> { "3" };
            this.Attributes = new JsonObject()
            {
                { "isRechargeable", false },
                { "queryOnlyEnergyStorage", true }
            };
        }
    }
}
