using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SharedNetworkFramework.Authentication.Register;
using SharedNetworkFramework.Authentication.Login;

namespace MM_API.ErrorHandler
{
    [Serializable, JsonObject]
    public class AuthenticationErrorHandler : IRegistrationResponse, ILoginResponse
    {
        [JsonProperty("errors")] 
        public IdentityError[] Errors { get; set; }
    }
}
