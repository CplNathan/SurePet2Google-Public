using System.Text.Json.Serialization;

namespace GoogleHelper.Models
{
    [JsonDerivedType(typeof(AccessTokenDto))]
    public abstract class TokenDto
    {
        public string token_type { get; set; } = "Bearer";
    }

    [JsonDerivedType(typeof(RefreshTokenDto))]
    public class AccessTokenDto : TokenDto
    {
        public AccessTokenDto(string accessToken)
        {
            this.access_token = accessToken;
            this.expires_in = (int)TimeSpan.FromDays(365).TotalSeconds;
        }

        public string access_token { get; set; }
        public int expires_in { get; set; }
    }

    public class RefreshTokenDto : AccessTokenDto
    {
        public RefreshTokenDto(string refreshToken, string accessToken) : base(accessToken)
        {
            this.refresh_token = refreshToken;
        }

        public string refresh_token { get; set; }
    }
}
