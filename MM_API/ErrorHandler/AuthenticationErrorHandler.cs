using SharedNetworkFramework.Authentication.Firebase.Register;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SharedNetworkFramework.Authentication.Firebase.SignIn;

namespace MM_API.ErrorHandler
{
    [Serializable, JsonObject]
    public class AuthenticationErrorHandler : IRegistrationResponse, ISignInResponse
    {
        [JsonProperty("errors")] 
        public IdentityError[] Errors { get; set; }


    }
}
