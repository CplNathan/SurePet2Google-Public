namespace SurePet2Google.Blazor.Server.Models.Responses.Devices
{
    public class GetDevices
    {
        public Datum[]? data { get; set; }
    }
    public class Datum
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public int household_id { get; set; }
        public string? name { get; set; }
        public string? serial_number { get; set; }
        public string? mac_address { get; set; }
        public string? version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Child[]? children { get; set; }
        public Control? control { get; set; }
        public Status? status { get; set; }
        public int parent_device_id { get; set; }
        public int index { get; set; }
        public DateTime pairing_at { get; set; }
        public Parent? parent { get; set; }
        public Tag[]? tags { get; set; }
    }

    public class Control
    {
        public int led_mode { get; set; }
        public int pairing_mode { get; set; }
        public Curfew? curfew { get; set; }
        public bool fast_polling { get; set; }
    }

    public class Curfew
    {
        public bool enabled { get; set; }
        public string? lock_time { get; set; }
        public string? unlock_time { get; set; }
    }

    public class Status
    {
        public int led_mode { get; set; }
        public int pairing_mode { get; set; }
        public Version? version { get; set; }
        public bool online { get; set; }
        public float battery { get; set; }
        public Locking? locking { get; set; }
        public bool learn_mode { get; set; }
        public Signal? signal { get; set; }
    }

    public class Version
    {
        public Device? device { get; set; }
        public Lcd? lcd { get; set; }
        public Rf? rf { get; set; }
    }

    public class Device
    {
        public int hardware { get; set; }
        public float firmware { get; set; }
    }

    public class Lcd
    {
        public int hardware { get; set; }
        public int firmware { get; set; }
    }

    public class Rf
    {
        public int hardware { get; set; }
        public float firmware { get; set; }
    }

    public class Locking
    {
        public int mode { get; set; }
        public Curfew1? curfew { get; set; }
    }

    public class Curfew1
    {
        public int delay_time { get; set; }
        public string? lock_time { get; set; }
        public int permission { get; set; }
        public string? unlock_time { get; set; }
        public bool locked { get; set; }
    }

    public class Signal
    {
        public int device_rssi { get; set; }
        public float hub_rssi { get; set; }
    }

    public class Parent
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public int household_id { get; set; }
        public string? name { get; set; }
        public string? serial_number { get; set; }
        public string? mac_address { get; set; }
        public string? version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Child
    {
        public int id { get; set; }
        public int parent_device_id { get; set; }
        public int product_id { get; set; }
        public int household_id { get; set; }
        public string? name { get; set; }
        public string? mac_address { get; set; }
        public int index { get; set; }
        public string? version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime pairing_at { get; set; }
    }

    public class Tag
    {
        public int id { get; set; }
        public int index { get; set; }
        public string? version { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
