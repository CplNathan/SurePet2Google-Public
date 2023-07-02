namespace SurePet2Google.Blazor.Server.Models.Responses.Pets
{
    public enum PetPosition
    {
        Unknown = 0,
        Inside = 1,
        Outside = 2
    }

    public class GetPosition
    {
        public PositionDataEnriched? data { get; set; }
    }

    public class PositionData
    {
        public int tag_id { get; set; }
        public int device_id { get; set; }
        public int where { get; set; }
        public DateTime since { get; set; }
    }

    public class PositionDataEnriched
    {
        public string? pet_name { get; set; }
        public int pet_id { get; set; }
        public int tag_id { get; set; }
        public int device_id { get; set; }
        public int where { get; set; }
        public DateTime since { get; set; }
    }

}
