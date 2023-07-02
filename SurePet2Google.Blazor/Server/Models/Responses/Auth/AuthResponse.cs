namespace SurePet2Google.Blazor.Server.Models.Responses.Auth
{

    public class AuthResponse
    {
        public Data? data { get; set; }
    }

    public class Data
    {
        public User? user { get; set; }
        public string? token { get; set; }
    }

    public class User
    {
        public long id { get; set; }
        public string? email_address { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public int country_id { get; set; }
        public int language_id { get; set; }
        public bool marketing_opt_in { get; set; }
        public DateTime terms_accepted { get; set; }
        public int weight_units { get; set; }
        public int time_format { get; set; }
        public string? version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Notifications? notifications { get; set; }
    }

    public class Notifications
    {
        public bool device_status { get; set; }
        public bool animal_movement { get; set; }
        public bool intruder_movements { get; set; }
        public bool new_device_pet { get; set; }
        public bool household_management { get; set; }
        public bool photos { get; set; }
        public bool low_battery { get; set; }
        public bool curfew { get; set; }
        public bool feeding_activity { get; set; }
        public bool drinking_activity { get; set; }
        public bool feeding_topup { get; set; }
        public bool drinking_topup { get; set; }
    }
}
