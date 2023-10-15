using GoogleHelper.Services;
using Microsoft.AspNetCore.Mvc;
using SurePet2Google.Blazor.Server.Context;
using SurePet2Google.Blazor.Server.Models.Responses.Devices;
using SurePet2Google.Blazor.Server.Services;
using SurePet2Google.Blazor.Shared;

namespace SurePet2Google.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        public SurePetService SurePetService { get; set; }

        public PersistenceService PersistenceService { get; set; }

        public IEnumerable<IDeviceService> SupportedDevices { get; set; }

        public IConfiguration Configuration { get; set; }

        public AuthenticationController(SurePetService surePetService, PersistenceService persistenceService, IEnumerable<IDeviceService> supportedDevices, IConfiguration configuration)
        {
            this.SurePetService = surePetService;
            this.PersistenceService = persistenceService;
            this.SupportedDevices = supportedDevices;
            this.Configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel form)
        {
            if (form != null && form.Data != null)
            {
                switch (form.request)
                {
                    case RequestState.Initial:
                        {
                            if (form.Data.client_id != this.Configuration["Google:client_id"])
                            {
                                return this.Json(this.BadRequest());
                            }

                            string? redirect_application = new Uri(form.Data.redirect_uri).AbsolutePath.Split('/')
                                .Where(item => !string.IsNullOrEmpty(item))
                                .Skip(1)
                                .Take(1)
                                .FirstOrDefault();

                            if (redirect_application != this.Configuration["Google:client_reference"])
                            {
                                return this.Json(this.BadRequest());
                            }

                            GoogleAuth auth = new()
                            {
                                redirect_uri = form.Data.redirect_uri,
                                client_id = form.Data.client_id,
                                state = form.Data.state
                            };

                            this.PersistenceService.AddOrUpdateGoogleAuth(auth);

                            return this.Json(new RegisterResponse
                            {
                                message = ResponseState.Success,
                                success = true,
                                Data = auth,
                                request = RequestState.Final
                            });
                        }
                    case RequestState.Final:
                        {
                            GoogleAuth? authEntity = this.PersistenceService.GetGoogleAuth(form.Data.state);
                            if (authEntity == null)
                            {
                                return this.Json(this.BadRequest());
                            }

                            RegisterResponse response = new()
                            {
                                Data = authEntity
                            };

                            string? bearer = await this.SurePetService.AuthenticateWithCredentials(form.Username, form.Password, CancellationToken.None);
                            if (!string.IsNullOrEmpty(bearer))
                            {
                                PetContext context = new PetContext(bearer, form.Username, form.Password);

                                Guid refreshToken = Guid.NewGuid();

                                GetDevices? devices = await this.SurePetService.GetDevices(bearer, CancellationToken.None);

                                context.Devices = this.SurePetService.ParseDevices(devices, this.SupportedDevices);

                                this.PersistenceService.DeletePetContextByUsername(context.Username);
                                this.PersistenceService.AddOrUpdatePetContext(context, refreshToken.ToString());

                                response.success = true;
                                response.Data = form.Data;
                                response.message = ResponseState.Success;
                                response.code = refreshToken;
                            }
                            else
                            {
                                response.success = false;
                                response.message = ResponseState.InvalidCredentials;
                            }

                            return this.Json(response);
                        }
                    default:
                        return this.Json(new RegisterResponse { message = ResponseState.BadRequest, success = false });
                }
            }

            return this.BadRequest();
        }
    }
}
