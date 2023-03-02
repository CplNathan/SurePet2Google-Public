// Copyright (c) Nathan Ford. All rights reserved. AuthViewModel.cs

using System.ComponentModel.DataAnnotations;

namespace SurePet2Google.Blazor.Shared
{
    public enum ResponseState
    {
        BadRequest,
        InvalidCredentials,
        Success
    }

    public enum RequestState
    {
        Initial,
        Final
    }

    public class GoogleAuth
    {
        public string client_id { get; set; } = string.Empty;
        public string? redirect_uri { get; set; }
        public string state { get; set; } = string.Empty;
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Email address is not a valid format.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterModel : LoginModel
    {
        public RequestState request { get; set; }

        public GoogleAuth? Data { get; set; }
    }

    public class LoginResponse : LoginModel
    {
        public string? sessionId { get; set; }
        public string? jwtBearer { get; set; }

        public ResponseState message { get; set; }
        public bool success { get; set; }
    }

    public class RegisterResponse : RegisterModel
    {
        public ResponseState message { get; set; }
        public Guid code { get; set; }
        public bool success { get; set; }
    }
}