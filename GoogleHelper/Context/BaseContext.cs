using GoogleHelper.Models;

namespace GoogleHelper.Context
{
    public class BaseContext
    {
        public string ContextIdentifier { get; set; }

        public BaseContext(string cId)
        {
            this.ContextIdentifier = cId;
        }

        public Dictionary<string, BaseDeviceModel> Devices = new();
    }
}
