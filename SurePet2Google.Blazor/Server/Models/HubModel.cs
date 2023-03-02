using GoogleHelper.Models;
using System.Text.Json.Nodes;

namespace SurePet2Google.Blazor.Server.Models
{
    public class HubModel : BaseDeviceModel
    {
        public HubModel()
            : base()
        {
            this.type = "CAMERA";
            this.traits = new List<string> { "CameraStream" };
            this.WillReportState = true;
            this.SupportedCommands = new List<string> { "GetCameraStream" };
            this.AlternativeNames = new List<string> { "Cat Hub" };
            this.ModelIdentifiers = new List<string> { "1" };
            this.Attributes = new JsonObject()
            {
                { "cameraStreamNeedAuthToken", false },
                { "cameraStreamNeedDrmEncryption", false },
                { "cameraStreamSupportedProtocols", new JsonArray()
                    {
                        { "progressive_mp4" }
                    }
                }
            };
        }
    }
}
