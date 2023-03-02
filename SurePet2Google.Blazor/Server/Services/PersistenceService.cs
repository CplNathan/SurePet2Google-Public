using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Shared;
using System.Collections.ObjectModel;

namespace SurePet2Google.Blazor.Server.Services
{
    public class PersistenceService
    {
        protected volatile Dictionary<string, GoogleAuth> AuthenticationState;

        protected volatile Dictionary<string, PetContext> GooglePetContext;

        public ReadOnlyDictionary<string, PetContext> GooglePetContextReadOnly => new(this.GooglePetContext.AsReadOnly());

        public PersistenceService()
        {
            this.AuthenticationState = new();
            this.GooglePetContext = new();
        }

        public void AddOrUpdateGoogleAuth(GoogleAuth auth)
        {
            this.AuthenticationState[auth.state] = auth;
        }

        public GoogleAuth? GetGoogleAuth(string state)
        {
            return this.AuthenticationState.GetValueOrDefault(state);
        }

        public void AddOrUpdatePetContext(PetContext context, string refreshToken)
        {
            if (refreshToken == null || context == null)
            {
                return;
            }

            this.GooglePetContext[refreshToken] = context;
        }

        public PetContext? GetPetContextByRefresh(string refreshToken)
        {
            return refreshToken == null ? null : this.GooglePetContext.GetValueOrDefault(refreshToken);
        }

        public void DeletePetContextByRefresh(string refreshToken)
        {
            if (refreshToken == null)
            {
                return;
            }

            this.GooglePetContext.Remove(refreshToken);
        }

        public PetContext? GetPetContextByAccess(string accessToken)
        {
            return accessToken == null
                ? null
                : this.GooglePetContext.FirstOrDefault(context => context.Value.GoogleAccessToken != null && context.Value.GoogleAccessToken == accessToken).Value;
        }
    }
}
