﻿using GoogleHelper.Context;
using SurePet2Google.Blazor.Server.Models.Responses.Pets;

namespace SurePet2Google.Blazor.Server.Context
{
    public class PetContext : BaseContext
    {
        public List<PetDatum> Pets { get; set; }

        public string SurePetBearerToken { get; set; }

        public string GoogleAccessToken { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public PetContext(string sessionId, string username, string passsword)
            : base(sessionId)
        {
            this.Username = username;
            this.Password = passsword;
            this.SurePetBearerToken = sessionId;
            this.Devices = new();
        }
    }
}
