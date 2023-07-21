namespace SurePet2Google.Blazor.Server.Models.Responses.Timeline
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public enum Direction
    {
        Looked = 0,
        Entered = 1,
        Left = 2,
    }

    public enum MovementType
    {
        UnknownPeeked = 13,
        UnknownMoved = 11,
        KnownMoved = 6,
        KnownPeeked = 4,
    }

    public class GetTimeline
    {
        public TimelineDatum[] data { get; set; }
        public Meta meta { get; set; }
    }

    public class Meta
    {
        public int page { get; set; }
        public int page_size { get; set; }
        public int count { get; set; }
    }

    public class TimelineDatum
    {
        public long id { get; set; }
        public int type { get; set; }
        public string data { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Household[] households { get; set; }
        public User[] users { get; set; }
        public Device[] devices { get; set; }
        public Movement[] movements { get; set; }
        public Pet[] pets { get; set; }
        public Tag[] tags { get; set; }
    }

    public class Household
    {
        public int id { get; set; }
        public string name { get; set; }
        public long created_user_id { get; set; }
        public string share_code { get; set; }
        public int timezone_id { get; set; }
        public string version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class User
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class Device
    {
        public int id { get; set; }
        public int parent_device_id { get; set; }
        public int product_id { get; set; }
        public int household_id { get; set; }
        public string name { get; set; }
        public string mac_address { get; set; }
        public int index { get; set; }
        public string version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime pairing_at { get; set; }
    }

    public class Movement
    {
        public long id { get; set; }
        public int device_id { get; set; }
        public int tag_id { get; set; }
        public string time { get; set; }
        public Direction direction { get; set; }
        public MovementType type { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Pet
    {
        public int id { get; set; }
        public string name { get; set; }
        public int gender { get; set; }
        public DateTime date_of_birth { get; set; }
        public string weight { get; set; }
        public string comments { get; set; }
        public int household_id { get; set; }
        public int spayed { get; set; }
        public int breed_id { get; set; }
        public int breed_id2 { get; set; }
        public int food_type_id { get; set; }
        public int photo_id { get; set; }
        public int species_id { get; set; }
        public int tag_id { get; set; }
        public string version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Tag
    {
        public int id { get; set; }
        public string tag { get; set; }
        public string version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int[] supported_product_ids { get; set; }
    }
}
