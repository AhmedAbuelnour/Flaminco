using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace WebApplication1.Controllers
{
    [Route("connect")]
    public class AuthorizationController : Controller
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthorizationController(IOpenIddictApplicationManager userManager, SignInManager<IdentityUser> signInManager)
        {
            _applicationManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("authorize")]
        public async Task<IActionResult> Authorize()
        {
            // Implement the authorization logic here
            // ...

            return Ok();
        }

        [HttpPost("token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request.IsPasswordGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                    throw new InvalidOperationException("The application cannot be found.");

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token, a token or a code.
                var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

                // Use the client_id as the subject identifier.
                identity.SetClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application));
                identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));

                identity.SetDestinations(static claim => claim.Type switch
                {
                    // Allow the "name" claim to be stored in both the access and identity tokens
                    // when the "profile" scope was granted (by calling principal.SetScopes(...)).
                    Claims.Name when claim.Subject.HasScope(Scopes.Profile)
                        => [Destinations.AccessToken, Destinations.IdentityToken],

                    // Otherwise, only store the claim in the access tokens.
                    _ => [Destinations.AccessToken]
                });

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException("The specified grant is not implemented.");
        }
    }

}
