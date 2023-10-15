using GoogleHelper.Json;
using GoogleHelper.Models;
using GoogleHelper.Services;
using Microsoft.AspNetCore.Mvc;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Services;

namespace SurePet2Google.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]/{id?}")]
    public class GoogleController : Controller
    {
        public GoogleService<PetContext> GoogleService { get; set; }

        public PersistenceService PersistenceService { get; set; }

        public IEnumerable<IDeviceService> SupportedDevices { get; set; }

        public IConfiguration Configuration { get; set; }

        public GoogleController(GoogleService<PetContext> googleService, PersistenceService persistenceService, IEnumerable<IDeviceService> supportedDevices, IConfiguration configuration)
        {
            this.GoogleService = googleService;
            this.PersistenceService = persistenceService;
            this.SupportedDevices = supportedDevices;
            this.Configuration = configuration;
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<ActionResult> Fulfillment([FromBody] GoogleIntentRequest request, CancellationToken token)
        {
            //contacts.Save("data.xml");

            string? bearer = this.HttpContext.Request.Headers.Authorization;
            bearer = bearer?.Split("Bearer ")?[1];

            if (bearer != null)
            {
                PetContext? context = this.PersistenceService.GetPetContextByAccess(bearer);

                if (context != null)
                {
                    GoogleIntentResponse response = await this.GoogleService.HandleGoogleResponse(context, request, this.SupportedDevices, bearer, token);

                    return this.Json(response);
                }
            }
            return this.BadRequest();
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult Token([FromForm] IFormCollection form)
        {
            if (form == null || form.Count <= 0)
            {
                return this.BadRequest();
            }

            if (form["client_secret"] != this.Configuration["Google:client_secret"] || form["client_id"] != this.Configuration["Google:client_id"])
            {
                return this.BadRequest();
            }

            switch (form["grant_type"])
            {
                case "authorization_code":
                    {
                        if (string.IsNullOrEmpty(form["code"]) || string.IsNullOrEmpty(form["redirect_uri"]))
                        {
                            return this.Json(this.BadRequest());
                        }

                        string? refreshToken = form?["code"];
                        if (refreshToken != null)
                        {
                            PetContext? context = this.PersistenceService.GetPetContextByRefresh(refreshToken);
                            if (context != null)
                            {
                                this.PersistenceService.DeletePetContextByRefresh(refreshToken);

                                refreshToken = Guid.NewGuid().ToString();

                                this.PersistenceService.AddOrUpdatePetContext(context, refreshToken);
                                context.GoogleAccessToken = Guid.NewGuid().ToString();

                                return this.Json(new RefreshTokenDto(refreshToken, context.GoogleAccessToken));
                            }
                        }

                        return this.BadRequest();
                    }
                case "refresh_token":
                    {
                        PetContext? context = this.PersistenceService.GetPetContextByRefresh(form["refresh_token"].ToString());
                        if (context != null)
                        {

                            context.GoogleAccessToken = Guid.NewGuid().ToString();

                            return this.Json(new AccessTokenDto(context.GoogleAccessToken));
                        }

                        return this.BadRequest();
                    }

                default:
                    return this.BadRequest();
            }
        }
    }
}
