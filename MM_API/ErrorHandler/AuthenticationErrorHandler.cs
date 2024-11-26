using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using MonoMonarchNetworkFramework.Authentication.Register;
using MonoMonarchNetworkFramework.Authentication.Login;

namespace MM_API.ErrorHandler
{
    [Serializable, JsonObject]
    public class AuthenticationErrorHandler : IRegistrationResponse, ILoginResponse
    {
        [JsonProperty("errors")] 
        public IdentityError[] Errors { get; set; }
    }
}
