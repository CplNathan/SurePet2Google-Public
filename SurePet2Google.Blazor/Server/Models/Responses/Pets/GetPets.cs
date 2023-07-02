namespace SurePet2Google.Blazor.Server.Models.Responses.Pets
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class GetPets
    {
        public PetDatum[]? data { get; set; }
    }

    public class Status
    {
        public Activity activity { get; set; }
    }

    public class Activity
    {
        public int tag_id { get; set; }
        public int device_id { get; set; }
        public int where { get; set; }
        public DateTime since { get; set; }
    }

    public class PetDatum
    {
        public int id { get; set; }
        public string? name { get; set; }
        public int gender { get; set; }
        public DateTime date_of_birth { get; set; }
        public string? weight { get; set; }
        public string? comments { get; set; }
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
        public Breed breed { get; set; }
        public Condition[] conditions { get; set; }
        public Photo photo { get; set; }
        public PositionData position { get; set; }
        public Species species { get; set; }
        public Status status { get; set; }
        public Tag tag { get; set; }
    }

    public class Breed
    {
        public int id { get; set; }
        public string? name { get; set; }
        public int species_id { get; set; }
        public string? version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Photo
    {
        public int id { get; set; }
        public string? location { get; set; }
        public long uploading_user_id { get; set; }
        public string? version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Species
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? version { get; set; }
    }

    public class Tag
    {
        public int id { get; set; }
        public string? tag { get; set; }
        public string? version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int[]? supported_product_ids { get; set; }
    }

    public class Condition
    {
        public int id { get; set; }
        public string? version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

}
