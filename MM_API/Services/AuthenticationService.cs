using SharedNetworkFramework.ErrorHandling;
using SharedNetworkFramework.Authentication.Firebase.SignOut;
using SharedNetworkFramework.Authentication.Firebase.Register;
using SharedNetworkFramework.Authentication.Firebase.SignIn;
using SharedNetworkFramework.Authentication.Firebase.RefreshToken;

using Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

///


namespace MM_API.Servicesc
{

    public interface IAuthenticationService
    {
        public Task<IRegistrationResponse> RegisterAsync(RegistrationPayload registrationPayload);
        public Task<ISignInResponse> LoginAsync(SignInPayload loginPayload);
        public Task<ISignOutResponse> LogoutAsync(SignOutPayload logoutPayload);
        public Task<IRefreshTokenResponse> RefreshIdToken(RefreshTokenPayload refreshTokenPayload);
    }

    /// <summary>
    /// PRODUCTION
    /// </summary>
    #region Production
    public class AuthenticationService : IAuthenticationService
    {

        private readonly MM_DbContext _dbContext;

        public AuthenticationService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private readonly string FB_URL = "https://identitytoolkit.googleapis.com/v1";
        private readonly string FB_URL_AUTH = "/accounts";
        private readonly string FB_URL_TOKEN = "https://securetoken.googleapis.com/v1/token";
        private readonly string FB_URL_APIKEY = $"?key={Environment.GetEnvironmentVariable("FIREBASE_API_KEY")}";

        /// <summary>
        /// Register user with FirebaseAuth and PostgresDB
        /// </summary>
        /// 
        /// <param name="registrationPayload"></param>
        /// <returns>
        /// RegistrationResponse or FirebaseError
        /// </returns>
        public async Task<IRegistrationResponse> RegisterAsync(RegistrationPayload registrationPayload)
        {
            try
            {
                string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signUp{FB_URL_APIKEY}";
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(registrationPayload), Encoding.UTF8, "application/json"));
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        RegistrationResponse registrationResponse = JsonConvert.DeserializeObject<RegistrationResponse>(responseBody);

                        t_User user = new t_User
                        {
                            user_name = registrationResponse.Email.Substring(0, registrationResponse.Email.IndexOf('@')),
                            user_fb_uuid = registrationResponse.LocalId
                        };
                        t_Session session = new t_Session()
                        {
                            user = user,
                            session_authtoken = registrationResponse.AuthToken,
                            session_refreshtoken = registrationResponse.RefreshToken,
                            session_loggedin = DateTimeOffset.UtcNow.UtcDateTime,
                            session_sessiontoken = "0"
                        };

                        await _dbContext.AddAsync(user);
                        await _dbContext.SaveChangesAsync();
                        await _dbContext.AddAsync(session);
                        await _dbContext.SaveChangesAsync();

                        return registrationResponse;
                    }
                    else //https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-8.0
                    {
                        FirebaseError firebaseErrorResponse = JsonConvert.DeserializeObject<FirebaseError>(responseBody);
                        return firebaseErrorResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
                //TODO: Implement error logging from service classes and throw from service->controller->client and force client to exit to menu

                //In future implementations:
                //1. May implement PostgresDB error logging - cross referencing error log and with user's existing sign in/out history

            }
            return null; //Reaches here only if Internal Server Error?
        }

        /*       SIGNIN DTO
        ________________________________|
        SignInPayload                   |
        ________________________________|
                                        |
        email	          |  string	    |
        password	      |  string	    |
        returnSecureToken |	 boolean	|
                          |             |
        ________________________________|
        SignInResponse    |             |
        ________________________________|
                          |             |
        idToken	          |  string	    |
        email	          |  string	    |
        refreshToken	  |  string	    |
        expiresIn         |  string	    |
        localId	          |  string	    |
        registered        |  boolean	|
         */

        //[RequireHttps]
        public async Task<ISignInResponse> LoginAsync(SignInPayload loginPayload)
        {
            try
            {
                string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signInWithPassword{FB_URL_APIKEY}";
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(loginPayload), Encoding.UTF8, "application/json"));
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        SignInResponse signInResponse = JsonConvert.DeserializeObject<SignInResponse>(responseBody);

                        t_User user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == signInResponse.LocalId);  //fk_user_id == user.user_id
                        var t_RecentSession = _dbContext.t_session.FirstOrDefault(s => s.user == user);
                        t_RecentSession.session_authtoken = signInResponse.AuthToken;
                        t_RecentSession.session_refreshtoken = signInResponse.RefreshToken;
                        t_RecentSession.session_loggedin = DateTimeOffset.UtcNow.UtcDateTime;
                        t_RecentSession.session_sessiontoken = "0";

                        await _dbContext.SaveChangesAsync();

                        return signInResponse;
                    }
                    else
                    {
                        FirebaseError firebaseErrorResponse = JsonConvert.DeserializeObject<FirebaseError>(responseBody);
                        return firebaseErrorResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login failed: {ex.Message}");
            }
            return null;
        }

        //[RequireHttps]
        public async Task<ISignOutResponse> LogoutAsync(SignOutPayload logoutPayload)
        {
            return null;
        }

        public async Task<IRefreshTokenResponse> RefreshIdToken(RefreshTokenPayload refreshTokenPayload)
        {

            return null;
        }
    }


    //FIREBASE AUTH ENDPOINT SIGN-IN USER
    /*
    Request Body Payload

    email	            string	
    password	        string	
    returnSecureToken	boolean	

    Response Payload

    idToken	        string	
    email	            string	
    refreshToken	    string	
    expiresIn      	string	
    localId	        string	
    registered     	boolean	
     */
    //[RequireHttps]




    #endregion

    /// <summary>
    /// DEVELOPMENT
    /// </summary>
    #region Development
    public class TestAuthenticationService : IAuthenticationService
    {

        private readonly MM_DbContext _dbContext;

        public TestAuthenticationService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private readonly string FB_URL = "https://identitytoolkit.googleapis.com/v1";
        private readonly string FB_URL_AUTH = "/accounts";
        private readonly string FB_URL_TOKEN = "https://securetoken.googleapis.com/v1/token";
        private readonly string FB_URL_APIKEY = $"?key={Environment.GetEnvironmentVariable("FIREBASE_API_KEY")}";

        /// <summary>
        /// Register user with FirebaseAuth and PostgresDB
        /// </summary>
        /// 
        /// <param name="registrationPayload"></param>
        /// <returns>
        /// RegistrationResponse or FirebaseError
        /// </returns>
        public async Task<IRegistrationResponse> RegisterAsync(RegistrationPayload registrationPayload)
        {
            try
            {
                string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signUp{FB_URL_APIKEY}";
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(registrationPayload), Encoding.UTF8, "application/json"));
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        RegistrationResponse registrationResponse = JsonConvert.DeserializeObject<RegistrationResponse>(responseBody);

                        t_User user = new t_User
                        {
                            user_name = registrationResponse.Email.Substring(0, registrationResponse.Email.IndexOf('@')),
                            user_fb_uuid = registrationResponse.LocalId
                        };

                        await _dbContext.AddAsync(user);
                        await _dbContext.SaveChangesAsync();

                        return registrationResponse;
                    }
                    else //https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-8.0
                    {
                        FirebaseError firebaseErrorResponse = JsonConvert.DeserializeObject<FirebaseError>(responseBody);
                        return firebaseErrorResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
                //TODO: Implement error logging from service classes and throw from service->controller->client and force client to exit to menu

                //In future implementations:
                //1. May implement PostgresDB error logging

            }
            return null; //Reaches here only if Internal Server Error?
        }

        /*       SIGNIN DTO
        ________________________________|
        SignInPayload                   |
        ________________________________|
                                        |
        email	          |  string	    |
        password	      |  string	    |
        returnSecureToken |	 boolean	|
                          |             |
        ________________________________|
        SignInResponse    |             |
        ________________________________|
                          |             |
        idToken	          |  string	    |
        email	          |  string	    |
        refreshToken	  |  string	    |
        expiresIn         |  string	    |
        localId	          |  string	    |
        registered        |  boolean	|
         */

        //[RequireHttps]
        public async Task<ISignInResponse> LoginAsync(SignInPayload loginPayload)
        {
            try
            {
                string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signInWithPassword{FB_URL_APIKEY}";
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(loginPayload), Encoding.UTF8, "application/json"));
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        SignInResponse signInResponse = JsonConvert.DeserializeObject<SignInResponse>(responseBody);

                        t_User user = await _dbContext.t_user.FirstAsync(u => u.user_fb_uuid == signInResponse.LocalId);  //fk_user_id == user.user_id
                        System.Diagnostics.Debug.WriteLine($"{user.user_id} - {user.user_fb_uuid} - {user.user_name}");
                        t_Session session = new t_Session()
                        {
                            fk_user_id = user.user_id,
                            session_authtoken = signInResponse.AuthToken,
                            session_refreshtoken = signInResponse.RefreshToken,
                            session_loggedin = DateTimeOffset.UtcNow.UtcDateTime,
                            session_loggedout = DateTimeOffset.MinValue,
                            session_sessiontoken = "0"
                        };

                        await _dbContext.t_session.AddAsync(session);
                        await _dbContext.SaveChangesAsync();

                        return signInResponse;
                    }
                    else
                    {
                        FirebaseError firebaseErrorResponse = JsonConvert.DeserializeObject<FirebaseError>(responseBody);
                        return firebaseErrorResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login failed: {ex.Message}");
            }
            return null;
        }

        //[RequireHttps]
        public async Task<ISignOutResponse> LogoutAsync(SignOutPayload logoutPayload)
        {
            try
            {
                t_User user = await _dbContext.t_user.FirstAsync(u => u.user_fb_uuid == logoutPayload.LocalId);
                t_Session recentSession = await _dbContext.t_session
                    .Where(s => s.fk_user_id == user.user_id)
                    .OrderByDescending(s => s.session_loggedin)
                    .FirstAsync();

                if (recentSession.session_loggedout == DateTimeOffset.MinValue)
                {
                    recentSession.session_authtoken = "0";
                    recentSession.session_refreshtoken = "0";
                    recentSession.session_loggedout = DateTimeOffset.UtcNow.UtcDateTime;

                    await _dbContext.SaveChangesAsync();

                    SignOutResponse signOutResponse = new SignOutResponse()
                    {
                        AuthToken = recentSession.session_sessiontoken,
                        RefreshToken = recentSession.session_refreshtoken,
                        LocalId = user.user_fb_uuid
                    };
                    return signOutResponse;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Logout failed: {user.user_id} - {recentSession.session_id}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout failed: {ex.Message}");

            }
            return null;
        }

        public async Task<IRefreshTokenResponse> RefreshIdToken(RefreshTokenPayload refreshTokenPayload)
        {
            ////needs to update user's existing refresh + id tokens in PSQL DB
            //try
            //{
            //    string fb_uri = $"{FB_URL_TOKEN}{FB_URL_APIKEY}";
            //    // var deserialisedRefreshTokenPayload = JsonConvert.DeserializeObject<RefreshTokenPayload>(refreshTokenPayload);

            //    //var content = new FormUrlEncodedContent(new[]
            //    // {
            //    //   new KeyValuePair<string, string>("grant_type", "refresh_token"),
            //    //   new KeyValuePair<string, string>("refresh_token", payload.RefreshToken)
            //    // });
            //    using (var client = new HttpClient())
            //    {
            //        var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(refreshTokenPayload), Encoding.UTF8, "application/json"));
            //        if (response.IsSuccessStatusCode)
            //        {
            //            // Read the response content
            //            string responseBody = await response.Content.ReadAsStringAsync();

            //            // Deserialize the response JSON to a RefreshTokenResponse object
            //            var refreshTokenResponse = JsonConvert.DeserializeObject<RefreshTokenResponse>(responseBody);

            //            //IS THIS SERIALISED ALREADY? IF SO NO NEED TO RESERIALISE

            //            // Return the RefreshTokenResponse object

            //            return refreshTokenResponse;
            //        }
            //        else
            //        {
            //            string error = await response.Content.ReadAsStringAsync();

            //            if (error.Contains("TOKEN_EXPIRED"))//MOVE TO CONTROLLER???
            //            {
            //                Console.WriteLine(error);
            //            }

            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine($"RefreshToken Failed: {ex}");
            //}
            return null;
        }
    }

}

#endregion



#region Legacy Code

///*
// *  private readonly string FB_URL = "https://identitytoolkit.googleapis.com/v1";
//        private readonly string FB_URL_AUTH = "/accounts";

//        //private readonly string FB_URL_TOKEN = 
//        private readonly string FB_URL_TOKEN = "https://securetoken.googleapis.com/v1/token";

//        private readonly string FB_URL_APIKEY = $"?key={Environment.GetEnvironmentVariable("FIREBASE_API_KEY")}";

//        //https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=[API_KEY]
//        //https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=[API_KEY]
//        //https://securetoken.googleapis.com/v1/token?key=[API_KEY]
//        public async Task<IRegistrationResponse> RegisterAsync(RegistrationPayload registrationPayload)
//        {
//            try
//            {
//                string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signUp{FB_URL_APIKEY}";
//                using (var client = new HttpClient())
//                {
//                    var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(registrationPayload), Encoding.UTF8, "application/json"));
//                    string responseBody = await response.Content.ReadAsStringAsync();
//                    if (response.IsSuccessStatusCode)
//                    {

//                        RegistrationResponse deserialisedResponse = JsonConvert.DeserializeObject<RegistrationResponse>(responseBody);

//                        t_User user = new t_User
//                        {
//                            user_name = deserialisedResponse.Email.Substring(0, deserialisedResponse.Email.IndexOf('@')),
//                            user_fb_uuid = deserialisedResponse.LocalId
//                        };
//                        t_Session session = new t_Session()
//                        {
//                            user = user,
//                            session_authtoken = deserialisedResponse.AuthToken,
//                            session_refreshtoken = deserialisedResponse.RefreshToken,
//                            session_loggedin = DateTimeOffset.UtcNow.UtcDateTime,
//                            session_sessiontoken = "0" //edit this out - requires PSQL update
//                                                       //session_loggedout = DateTimeOffset.UtcNow.UtcDateTime
//                        };

//                        await _dbContext.AddAsync(user);
//                        await _dbContext.SaveChangesAsync();
//                        await _dbContext.AddAsync(session);
//                        await _dbContext.SaveChangesAsync();


//                        //return JsonConvert.SerializeObject(signInResponse); //edit from RegistrationResponse type to string for a serialised string which when deserialised will be content in RegistractionResponse structure
//                        return deserialisedResponse;
//                    }
//                    else
//                    {
//                        var errorResponse = JsonConvert.DeserializeObject<FirebaseError>(responseBody);
//                        return errorResponse;
//                        //string firebaseErrorResponse = await response.Content.ReadAsStringAsync();
//                        //switch (firebaseErrorResponse)
//                        //{
//                        //    case string s when s.Contains("EMAIL_NOT_FOUND"):
//                        //        System.Diagnostics.Debug.WriteLine("Email not found.");
//                        //        break;
//                        //    default:
//                        //        System.Diagnostics.Debug.WriteLine($"Login failed: {firebaseErrorResponse}");
//                        //        break;
//                        //}
//                    }

//                    return JsonConvert.DeserializeObject<RegistrationResponse>(responseBody);


//                    //RegistrationResponse result = JsonConvert.DeserializeObject<RegistrationResponse>(responseBody);
//                    // return JsonConvert.DeserializeObject<RegistrationResponse>(responseBody);

//                    //https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-8.0
//                    //above link details much more sophisticated methods of error handling - current way requires no deserialising due to serialising/deserialising only required when importing data into datamodels, and in this case im simply hardcoding error case handling based on the message string 
//                    //string firebaseErrorResponse = await response.Content.ReadAsStringAsync();
//                    //switch (firebaseErrorResponse)
//                    //{
//                    //    case string s when s.Contains("EMAIL_EXISTS"):
//                    //        throw new InvalidOperationException($"Email address is already in use: {firebaseErrorResponse}");
//                    //    default:
//                    //        throw new Exception($"Registration failed: {firebaseErrorResponse}");

//                }

//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");

//            }
//            return null;
//        }







//        //FIREBASE AUTH ENDPOINT SIGN-IN USER
//        /*
//        Request Body Payload

//        email	            string	
//        password	        string	
//        returnSecureToken	boolean	

//        Response Payload

//        idToken	        string	
//        email	            string	
//        refreshToken	    string	
//        expiresIn      	string	
//        localId	        string	
//        registered     	boolean	
//         */
////[RequireHttps]
//public async Task<ISignInResponse> LoginAsync(SignInPayload loginPayload)
//{
//    try
//    {
//        string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signInWithPassword{FB_URL_APIKEY}";

//        using (var client = new HttpClient())
//        {
//            var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(loginPayload), Encoding.UTF8, "application/json"));
//            if (response.IsSuccessStatusCode)
//            {
//                string responseBody = await response.Content.ReadAsStringAsync();
//                SignInResponse deserialisedResponse = JsonConvert.DeserializeObject<SignInResponse>(responseBody);

//                t_User user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == deserialisedResponse.LocalId);  //fk_user_id == user.user_id
//                var session = _dbContext.t_session.FirstOrDefault(s => s.user == user);
//                session.session_authtoken = deserialisedResponse.AuthToken;
//                session.session_refreshtoken = deserialisedResponse.RefreshToken;
//                session.session_loggedin = DateTimeOffset.UtcNow.UtcDateTime;
//                session.session_sessiontoken = "0";

//                await _dbContext.SaveChangesAsync();

//                return deserialisedResponse;
//            }
//            else
//            {
//                string errorResponse = await response.Content.ReadAsStringAsync();
//                switch (errorResponse)
//                {
//                    case string s when s.Contains("EMAIL_NOT_FOUND"):
//                        System.Diagnostics.Debug.WriteLine("Email not found.");
//                        break;
//                    default:
//                        System.Diagnostics.Debug.WriteLine($"Login failed: {errorResponse}");
//                        break;
//                }
//            }
//        }

//    }
//    catch (Exception ex)
//    {
//        System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");

//    }
//    return null;
//}



////[RequireHttps]
//public async Task<ISignOutResponse> LogoutAsync(SignOutPayload logoutPayload)
//{
//    //var deserialisedLogoutPayload = JsonConvert.DeserializeObject<SignOutPayload>(logoutPayload);
//    await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(JsonConvert.SerializeObject(logoutPayload.LocalId));

//    var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == logoutPayload.LocalId);
//    var session = _dbContext.t_session.FirstOrDefault(s => s.fk_user_id == t_user.user_id);// change .FirstOrDefault on this and login endpoints to .Last in t_session find as user will eventually own many sesssions, instead of replacing the same session entry each time
//    try
//    {
//        session.session_authtoken = "0";
//        //session.session_sessiontoken = "0"; - commented out, not required as already not null with == "0"
//        session.session_refreshtoken = "0";
//        session.session_loggedout = DateTimeOffset.UtcNow.UtcDateTime;

//        System.Diagnostics.Debug.WriteLine("PSQL LOGOUT SUCCESS");
//        await _dbContext.SaveChangesAsync();
//    }
//    catch (Exception)
//    {
//        System.Diagnostics.Debug.WriteLine("PSQL LOGOUT FAILURE");
//        return false;
//    }
//    return true;
//}

//public async Task<IRefreshTokenResponse> RefreshIdToken(RefreshTokenPayload refreshTokenPayload)
//{
//    //needs to update user's existing refresh + id tokens in PSQL DB
//    try
//    {
//        string fb_uri = $"{FB_URL_TOKEN}{FB_URL_APIKEY}";
//        // var deserialisedRefreshTokenPayload = JsonConvert.DeserializeObject<RefreshTokenPayload>(refreshTokenPayload);

//        //var content = new FormUrlEncodedContent(new[]
//        // {
//        //   new KeyValuePair<string, string>("grant_type", "refresh_token"),
//        //   new KeyValuePair<string, string>("refresh_token", payload.RefreshToken)
//        // });
//        using (var client = new HttpClient())
//        {
//            var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(refreshTokenPayload), Encoding.UTF8, "application/json"));
//            if (response.IsSuccessStatusCode)
//            {
//                // Read the response content
//                string responseBody = await response.Content.ReadAsStringAsync();

//                // Deserialize the response JSON to a RefreshTokenResponse object
//                var refreshTokenResponse = JsonConvert.DeserializeObject<RefreshTokenResponse>(responseBody);

//                //IS THIS SERIALISED ALREADY? IF SO NO NEED TO RESERIALISE

//                // Return the RefreshTokenResponse object

//                return refreshTokenResponse;
//            }
//            else
//            {
//                string error = await response.Content.ReadAsStringAsync();

//                if (error.Contains("TOKEN_EXPIRED"))//MOVE TO CONTROLLER???
//                {
//                    Console.WriteLine(error);
//                }

//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Diagnostics.Debug.WriteLine($"RefreshToken Failed: {ex}");
//    }
//}
//    }

//    //FIREBASE AUTH ENDPOINT SIGN-IN USER
//    /*
//    Request Body Payload

//    email	            string	
//    password	        string	
//    returnSecureToken	boolean	

//    Response Payload

//    idToken	        string	
//    email	            string	
//    refreshToken	    string	
//    expiresIn      	string	
//    localId	        string	
//    registered     	boolean	
//     */
//    //[RequireHttps]

//*/
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~




/*
 *     //public interface IAuthenticationService
    //{
    //    public Task<RegistrationPayload> RegisterAsync(RegistrationPayload registrationPayload);
    //    public Task<SignInPayload> LoginAsync(SignInPayload loginPayload);
    //    public Task<SignOutPayload> LogoutAsync(SignOutPayload logoutPayload);
    //    public Task<RefreshTokenPayload> RefreshIdToken(RefreshTokenPayload refreshTokenPayload);
    //}

    ///// <summary>
    ///// PRODUCTION
    ///// </summary>
    //#region Production
    //public class AuthenticationService : IAuthenticationService
    //{
    //    private readonly MM_DbContext _dbContext;
    //    public AuthenticationService(MM_DbContext dbContext)
    //    {
    //        _dbContext = dbContext;
    //    }
    //    private readonly string FB_URL = "https://identitytoolkit.googleapis.com/v1";
    //    private readonly string FB_URL_AUTH = "/accounts";
    //    private readonly string FB_URL_TOKEN = "/token";
    //    private readonly string FB_URL_APIKEY = $"?key={Environment.GetEnvironmentVariable("FIREBASE_API_KEY")}";
    //    public async Task<RegistrationPayload> RegisterAsync(RegistrationPayload registrationPayload)
    //    {
    //        try
    //        {
    //            string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signUp{FB_URL_APIKEY}";
    //            using (var client = new HttpClient())
    //            {
    //                var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(registrationPayload), Encoding.UTF8, "application/json"));
    //                if (response.IsSuccessStatusCode)
    //                {
    //                    string responseBody = await response.Content.ReadAsStringAsync();
    //                    var userRecord = JsonConvert.DeserializeObject<RegistrationResponse>(responseBody);

    //                    t_User user = new t_User
    //                    {
    //                        user_name = userRecord.Email.Substring(0, userRecord.Email.IndexOf('@')),
    //                        user_fb_uuid = userRecord.LocalId
    //                    };
    //                    t_Session session = new t_Session()
    //                    {
    //                        user = user,
    //                        session_authtoken = userRecord.AuthToken,
    //                        session_refreshtoken = userRecord.RefreshToken,
    //                        session_loggedin = DateTimeOffset.UtcNow.UtcDateTime,
    //                        session_sessiontoken = "0" //edit this out - requires PSQL update
    //                                                   //session_loggedout = DateTimeOffset.UtcNow.UtcDateTime
    //                    };

    //                    await _dbContext.AddAsync(user);
    //                    await _dbContext.SaveChangesAsync();
    //                    await _dbContext.AddAsync(session);
    //                    await _dbContext.SaveChangesAsync();


    //                    //return JsonConvert.SerializeObject(signInResponse); //edit from RegistrationResponse type to string for a serialised string which when deserialised will be content in RegistractionResponse structure
    //                    return userRecord 
    //                }
    //                else
    //                {//check Registered bool == false???
    //                    string firebaseErrorResponse = await response.Content.ReadAsStringAsync();
    //                    switch (firebaseErrorResponse)
    //                    {
    //                        case string s when s.Contains("EMAIL_EXISTS"):
    //                            throw new InvalidOperationException($"Email address is already in use: {firebaseErrorResponse}");
    //                        default:
    //                            throw new Exception($"Registration failed: {firebaseErrorResponse}");
    //                    }
    //                }
    //            }
    //        }


    //        catch (Exception ex)
    //        {
    //            System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
    //            throw;
    //        }
    //    }
    //    //FIREBASE AUTH ENDPOINT SIGN-IN USER
    //    /*
    //Request Body Payload

    //email	            string	
    //password	        string	
    //returnSecureToken	boolean	

    //Response Payload

    //idToken	        string	
    //email	            string	
    //refreshToken	    string	
    //expiresIn      	string	
    //localId	        string	
    //registered     	boolean	
    //     */
//    //[RequireHttps]
//    public async Task<string> LoginAsync(SignInPayload loginPayload)
//    {
//        SignInResponse userRecord = null;
//        try
//        {
//            string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signInWithPassword{FB_URL_APIKEY}";

//            using (var client = new HttpClient())
//            {
//                var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(loginPayload), Encoding.UTF8, "application/json"));
//                if (response.IsSuccessStatusCode)
//                {
//                    string responseBody = await response.Content.ReadAsStringAsync();
//                    userRecord = JsonConvert.DeserializeObject<SignInResponse>(responseBody);
//                    var handler = new JwtSecurityTokenHandler();
//                    //var jsonToken = handler.ReadToken(signInResponse.AuthToken) as JwtSecurityToken;
//                    //if (jsonToken == null)
//                    //{
//                    //    throw new InvalidOperationException("Invalid JWT token");
//                    //}
//                    t_User user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == userRecord.LocalId);
//                    var session = _dbContext.t_session.FirstOrDefault(s => s.user == user);//fk_user_id == user.user_id
//                    session.session_authtoken = userRecord.AuthToken;
//                    session.session_refreshtoken = userRecord.RefreshToken;
//                    session.session_loggedin = DateTimeOffset.UtcNow.UtcDateTime;
//                    session.session_sessiontoken = "0";
//                    await _dbContext.SaveChangesAsync();
//                }
//                else
//                {
//                    string firebaseErrorResponse = await response.Content.ReadAsStringAsync();
//                    switch (firebaseErrorResponse)
//                    {
//                        case string s when s.Contains("EMAIL_NOT_FOUND"):
//                            throw new InvalidOperationException("Email not found.");
//                        default:
//                            throw new Exception($"Login failed: {firebaseErrorResponse}");
//                    }
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
//            throw;
//        }
//        return JsonConvert.SerializeObject(userRecord);
//    }



//    //[RequireHttps]
//    public async Task<bool> LogoutAsync(SignOutPayload logoutPayload)
//    {
//        //var deserialisedLogoutPayload = JsonConvert.DeserializeObject<SignOutPayload>(logoutPayload);
//        await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(JsonConvert.SerializeObject(logoutPayload.LocalId));

//        var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == logoutPayload.LocalId);
//        var session = _dbContext.t_session.FirstOrDefault(s => s.fk_user_id == t_user.user_id);// change .FirstOrDefault on this and login endpoints to .Last in t_session find as user will eventually own many sesssions, instead of replacing the same session entry each time
//        try
//        {
//            session.session_authtoken = "0";
//            //session.session_sessiontoken = "0"; - commented out, not required as already not null with == "0"
//            session.session_refreshtoken = "0";
//            session.session_loggedout = DateTimeOffset.UtcNow.UtcDateTime;

//            System.Diagnostics.Debug.WriteLine("PSQL LOGOUT SUCCESS");
//            await _dbContext.SaveChangesAsync();
//        }
//        catch (Exception)
//        {
//            System.Diagnostics.Debug.WriteLine("PSQL LOGOUT FAILURE");
//            return false;
//        }
//        return true;
//    }

//    public async Task<string> RefreshIdToken(RefreshTokenPayload refreshTokenPayload)
//    {
//        //needs to update user's existing refresh + id tokens in PSQL DB
//        try
//        {
//            string fb_uri = $"{FB_URL_TOKEN}{FB_URL_APIKEY}";
//            // var deserialisedRefreshTokenPayload = JsonConvert.DeserializeObject<RefreshTokenPayload>(refreshTokenPayload);

//            //var content = new FormUrlEncodedContent(new[]
//            // {
//            //   new KeyValuePair<string, string>("grant_type", "refresh_token"),
//            //   new KeyValuePair<string, string>("refresh_token", payload.RefreshToken)
//            // });
//            using (var client = new HttpClient())
//            {
//                var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(refreshTokenPayload), Encoding.UTF8, "application/json"));
//                if (response.IsSuccessStatusCode)
//                {
//                    // Read the response content
//                    string responseBody = await response.Content.ReadAsStringAsync();

//                    // Deserialize the response JSON to a RefreshTokenResponse object
//                    var refreshTokenResponse = JsonConvert.DeserializeObject<RefreshTokenResponse>(responseBody);

//                    //IS THIS SERIALISED ALREADY? IF SO NO NEED TO RESERIALISE

//                    // Return the RefreshTokenResponse object
//                    return JsonConvert.SerializeObject(refreshTokenResponse);
//                }
//                else
//                {
//                    string error = await response.Content.ReadAsStringAsync();

//                    if (error.Contains("TOKEN_EXPIRED"))//MOVE TO CONTROLLER???
//                    {
//                        Console.WriteLine(error);
//                    }
//                    return error;
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            throw ex;
//        }
//    }
//}



//#endregion


//    public async Task<RegistrationResponse> RegisterAsync(RegistrationPayload payload)
//    {
//        try
//        {
//            string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signUp{FB_URL_APIKEY}";
//            string jsonPayload = JsonConvert.SerializeObject(payload);
//            using (var client = new HttpClient())
//            {
//                var response = await client.PostAsync(fb_uri, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));//application/json-patch+json //https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-8.0
//                if (response.IsSuccessStatusCode)
//                {
//                    string responseBody = await response.Content.ReadAsStringAsync();
//                    var signInResponse = JsonConvert.DeserializeObject<RegistrationResponse>(responseBody);

//                    var handler = new JwtSecurityTokenHandler();
//                    var jsonToken = handler.ReadToken(signInResponse.AuthToken) as JwtSecurityToken;
//                    if (jsonToken == null)
//                    {
//                        throw new InvalidOperationException("Invalid JWT token");
//                    }
//                    t_User user = new t_User
//                    {
//                        user_name = signInResponse.Email.Substring(0, payload.Email.IndexOf('@')),
//                        user_fb_uuid = signInResponse.LocalId
//                    };
//                    t_Session session = new t_Session()
//                    {
//                        user = user,
//                        session_authtoken = signInResponse.AuthToken,
//                        session_refreshtoken = signInResponse.RefreshToken,
//                        session_loggedin = DateTimeOffset.UtcNow.UtcDateTime,
//                        session_sessiontoken = "0"

//                    };
//                    await _dbContext.AddAsync(user);
//                    await _dbContext.SaveChangesAsync();
//                    await _dbContext.AddAsync(session);
//                    await _dbContext.SaveChangesAsync();
//                    return signInResponse;
//                }
//                else
//                {
//                    string firebaseErrorResponse = await response.Content.ReadAsStringAsync();
//                    switch (firebaseErrorResponse)
//                    {
//                        case string s when s.Contains("EMAIL_EXISTS"):
//                            throw new InvalidOperationException($"Email address is already in use: {firebaseErrorResponse}");
//                        default:
//                            throw new Exception($"Registration failed: {firebaseErrorResponse}");
//                    }
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
//            throw;
//        }
//    }

//    public async Task<SignInResponse> LoginAsync(SignInPayload payload)
//    {
//        SignInResponse signInResponse = null;
//        try
//        {
//            string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signInWithPassword{FB_URL_APIKEY}";
//            string jsonPayload = JsonConvert.SerializeObject(payload);
//            using (var client = new HttpClient())
//            {
//                var response = await client.PostAsync(fb_uri, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
//                if (response.IsSuccessStatusCode)
//                {
//                    string responseBody = await response.Content.ReadAsStringAsync();
//                    signInResponse = JsonConvert.DeserializeObject<SignInResponse>(responseBody);
//                    var handler = new JwtSecurityTokenHandler();
//                    var jsonToken = handler.ReadToken(signInResponse.AuthToken) as JwtSecurityToken;
//                    if (jsonToken == null)
//                    {
//                        throw new InvalidOperationException("Invalid JWT token");
//                    }
//                    t_User user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == signInResponse.LocalId);
//                    var session = _dbContext.t_session.FirstOrDefault(s => s.user == user);//fk_user_id == user.user_id
//                    session.session_authtoken = signInResponse.AuthToken;
//                    session.session_refreshtoken = signInResponse.RefreshToken;
//                    session.session_loggedin = DateTimeOffset.UtcNow.UtcDateTime;
//                    session.session_sessiontoken = "0";
//                    await _dbContext.SaveChangesAsync();
//                }
//                else
//                {
//                    string firebaseErrorResponse = await response.Content.ReadAsStringAsync();
//                    switch (firebaseErrorResponse)
//                    {
//                        case string s when s.Contains("EMAIL_NOT_FOUND"):
//                            throw new InvalidOperationException("Email not found.");
//                        default:
//                            throw new Exception($"Login failed: {firebaseErrorResponse}");
//                    }
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
//            throw;
//        }
//        return signInResponse;
//    }

//    public async Task<bool> LogoutAsync(SignOutPayload payload)
//    {
//        var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == payload.LocalId);
//        var session = _dbContext.t_session.Last(s => s.fk_user_id == t_user.user_id);
//        try
//        {
//            session.session_authtoken = payload.AuthToken;
//            session.session_refreshtoken = payload.RefreshToken;
//            session.session_loggedout = DateTimeOffset.UtcNow.UtcDateTime;
//            System.Diagnostics.Debug.WriteLine("PSQL LOGIN SUCCESS");
//            await _dbContext.SaveChangesAsync();
//        }
//        catch (Exception)
//        {
//            System.Diagnostics.Debug.WriteLine("PSQL LOGOUT FAILURE");
//            return false;
//        }
//        return true;
//    }
//}

#endregion


#region Misc Code


//public async Task<RefreshTokenResponse> RefreshIdToken(RefreshTokenPayload payload)
//{
//    //var response = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(payload.RefreshToken);
//    try
//    {
//        using 
//        // Send the POST request to the Firebase token endpoint
//        var response = await client.PostAsync(fb_uri, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

//        if (response.IsSuccessStatusCode)
//        {
//            // Read the response content
//            string responseBody = await response.Content.ReadAsStringAsync();

//            // Deserialize the response JSON to a RefreshTokenResponse object
//            RefreshTokenResponse refreshTokenResponse = JsonConvert.DeserializeObject<RefreshTokenResponse>(responseBody);

//            // Return the RefreshTokenResponse object
//            return refreshTokenResponse;
//        }
//        else
//        {
//            // Read the response content to determine the error code
//            string responseBody = await response.Content.ReadAsStringAsync();

//            // Parse the response JSON to extract the error code
//            string errorCode = JObject.Parse(responseBody).Value<string>("error");

//            // Handle the error based on the error code
//            if (errorCode == "TOKEN_EXPIRED")
//            {
//                // Handle TOKEN_EXPIRED error
//            }
//            else if (errorCode == "USER_DISABLED")
//            {
//                // Handle USER_DISABLED error
//            }
//            // Add more else if statements for other error codes
//            else
//            {
//                // Handle unknown error codes or unexpected errors
//            }

//            // You may choose to throw an exception here or return null/other appropriate response
//            return null;
//        }
//    }
//    catch (Exception ex)
//    {
//        // Handle exceptions that might occur during the request
//        // You may choose to rethrow the exception, log it, or return null/other appropriate response
//        return null;
//    }
//}



//FIREBASE ADMIN SDK CREATEUSER
//FirebaseAuthException is resource hungry, consider rewriting so user creation is not executed only if an exception is caught
/*        [RequireHttps]
        public async Task<string> RegisterAsync(CredentialsModel credModel)
        {
            string fb_uuid = string.Empty;
            try
            {//CHECK IF EMAIL EXISTS
                await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(credModel.UserEmail);
                fb_uuid = "0";
                return fb_uuid;
            }
            catch (FirebaseAuthException)
            {//CREATE USER
             //FIREBASE
                UserRecordArgs args = new UserRecordArgs
                {
                    Email = credModel.UserEmail,
                    Password = credModel.UserPassword,
                };
                UserRecord signInResponse = await FirebaseAuth.DefaultInstance.CreateUserAsync(args);

                //POSTGRES
                var user = new t_User
                {
                    user_name = credModel.UserEmail.Substring(0, credModel.UserEmail.IndexOf('@')),
                    user_fb_uuid = signInResponse.Uid
                };
                await _dbContext.AddAsync(user);
                await _dbContext.SaveChangesAsync();

                fb_uuid = signInResponse.Uid;
                System.Diagnostics.Debug.WriteLine(signInResponse.Uid);
            }
            return fb_uuid;
        }*/



//[RequireHttps]
/*public async Task<bool> LoginAsync(AuthenticationModel authModel)
{
    try
    {
        //GET POSTGRES USER_ID FROM CLIENT PROVIDED FIREBASE TOKEN
        var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(authModel.AuthToken);
        var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == decodedToken.Subject);
        // await FirebaseAuth.DefaultInstance.
        t_Session session = new t_Session()
        {
            user = t_user, //BELONGS TO...

            //STORE ENCODED CLIENT TOKENS
            session_authtoken = authModel.AuthToken,
            session_sessiontoken = authModel.SessionToken,
            session_refreshtoken = authModel.RefreshToken,

            session_loggedin = DateTimeOffset.UtcNow.UtcDateTime,
            session_loggedout = DateTimeOffset.UtcNow.UtcDateTime //logout initialised as UtcNow, replaced during logout
        };
        await _dbContext.AddAsync(session);
        await _dbContext.SaveChangesAsync();
    }
    catch (ArgumentNullException)
    {
        System.Diagnostics.Debug.WriteLine("PSQL LOGIN FAIL"); // REPLACE WITH DI LOGGER?
        return false;
    }
    System.Diagnostics.Debug.WriteLine("PSQL LOGIN SUCCESS");
    return true;
}*/

/// <summary>
/// OLD PRODUCTION
/// </summary>
//public class AuthenticationService : IAuthenticationService
//{
//    private readonly MM_DbContext _dbContext;
//    public AuthenticationService(MM_DbContext dbContext)
//    {
//        _dbContext = dbContext;
//    }
//    [RequireHttps]
//    public async Task<string> RegisterAsync(CredentialsModel credModel)
//    {
//        System.Diagnostics.Debug.WriteLine("REACHED REGISTER ENDPOINT");
//        string fb_uuid = string.Empty;
//        try
//        {//CHECK IF EMAIL EXISTS
//            await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(credModel.UserEmail);
//            fb_uuid = "0";
//            return fb_uuid;
//        }
//        catch (FirebaseAuthException)
//        {//CREATE USER
//         //FIREBASE
//            UserRecordArgs args = new UserRecordArgs
//            {
//                Email = credModel.UserEmail,
//                Password = credModel.UserPassword,
//            };
//            UserRecord signInResponse = await FirebaseAuth.DefaultInstance.CreateUserAsync(args);

//            //POSTGRES
//            var user = new t_User
//            {
//                user_name = credModel.UserEmail.Substring(0, credModel.UserEmail.IndexOf('@')),
//                user_fb_uuid = signInResponse.Uid
//            };
//            await _dbContext.AddAsync(user);
//            await _dbContext.SaveChangesAsync();

//            fb_uuid = signInResponse.Uid;
//            System.Diagnostics.Debug.WriteLine(signInResponse.Uid);
//        }
//        return fb_uuid;
//    }

//    [RequireHttps]
//    public async Task<bool> LoginAsync(AuthenticationModel authModel)
//    {
//        try
//        {
//            //GET POSTGRES USER_ID FROM CLIENT PROVIDED FIREBASE TOKEN
//            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(authModel.AuthToken);
//            var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == decodedToken.Subject);

//            t_Session session = new t_Session()
//            {
//                user = t_user, //BELONGS TO...

//                //STORE ENCODED CLIENT TOKENS
//                session_authtoken = authModel.AuthToken,
//                session_sessiontoken = authModel.SessionToken,
//                session_refreshtoken = authModel.RefreshToken,

//                session_loggedin = DateTimeOffset.UtcNow.UtcDateTime,
//                session_loggedout = DateTimeOffset.UtcNow.UtcDateTime //logout initialised as UtcNow, replaced during logout
//            };
//            await _dbContext.AddAsync(session);
//            await _dbContext.SaveChangesAsync();
//        }
//        catch (ArgumentNullException)
//        {
//            System.Diagnostics.Debug.WriteLine("PSQL LOGIN FAIL"); // REPLACE WITH DI LOGGER?
//            return false;
//        }
//        System.Diagnostics.Debug.WriteLine("PSQL LOGIN SUCCESS");
//        return true;
//    }

//    [RequireHttps]
//    public async Task<bool> LogoutAsync(AuthenticationModel authModel)
//    {
//        //GET POSTGRES USER_ID FROM CLIENT PROVIDED FIREBASE TOKEN
//        var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(authModel.AuthToken);
//        var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == decodedToken.Subject);

//        //GET POSTGRES SESSION FOR USER
//        var session = _dbContext.t_session.Last(s => s.fk_user_id == t_user.user_id);

//        try
//        {
//            //OVERWRITE LATEST SESSION WITH ENCODED CLIENT TOKENS AND LOGOUT TIME
//            session.session_authtoken = authModel.AuthToken;
//            session.session_sessiontoken = authModel.SessionToken;
//            session.session_refreshtoken = authModel.RefreshToken;
//            session.session_loggedout = DateTimeOffset.UtcNow.UtcDateTime;

//            await _dbContext.SaveChangesAsync();
//        }
//        catch (ArgumentNullException)
//        {
//            System.Diagnostics.Debug.WriteLine("PSQL LOGOUT FAILURE");
//            return false;
//        }
//        System.Diagnostics.Debug.WriteLine("PSQL LOGIN SUCCESS");
//        return true;
//    }
//}
#endregion

