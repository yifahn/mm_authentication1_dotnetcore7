﻿using MM_API.Database.Postgres.DbSchema;
using MM_API.ErrorHandler;
using MM_API.Database.Postgres;

using Newtonsoft.Json;

using Microsoft.IdentityModel.Tokens;

using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity;

using System.Text;

using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;
using System.Security.Cryptography;

using MonoMonarchNetworkFramework.Authentication.RefreshToken;
using MonoMonarchNetworkFramework.Authentication.Register;
using MonoMonarchNetworkFramework.Authentication.Login;
using MonoMonarchNetworkFramework.Authentication.Logout;

using MonoMonarchGameFramework.Game.Kingdom.Nodes;
using MonoMonarchGameFramework.Game.Kingdom;

using MonoMonarchGameFramework.Game.Character;

using MonoMonarchGameFramework.Game.Character.Attribute;
using MonoMonarchGameFramework.Game.Character.Attribute.CharacterLevel;
using MonoMonarchGameFramework.Game.Character.Attribute.Constitution;
using MonoMonarchGameFramework.Game.Character.Attribute.Defence;
using MonoMonarchGameFramework.Game.Character.Attribute.Luck;
using MonoMonarchGameFramework.Game.Character.Attribute.Stamina;
using MonoMonarchGameFramework.Game.Character.Attribute.Strength;

using MonoMonarchGameFramework.Game.Armoury.Equipment;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Arms;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Hands;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Head;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Legs;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Feet;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Torso;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon.Axe;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon.Spear;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon.Staff;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon.Sword;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Jewellery;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Jewellery.Amulet;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Jewellery.Ring;

using MonoMonarchGameFramework.Game;
using MonoMonarchGameFramework.Game.Armoury;
using MonoMonarchGameFramework.Game.Kingdom.Nodes.Grassland;
using MonoMonarchGameFramework.Game.Treasury;
using MonoMonarchGameFramework.Game.Treasury.Currency;
using MonoMonarchGameFramework.Game.Treasury.GoldBag;
using MonoMonarchGameFramework.Game.Soupkitchen;
using MonoMonarchNetworkFramework;
using System.Numerics;


namespace MM_API.Services
{

    public interface IAuthenticationService
    {
        public Task<IRegistrationResponse> RegisterAsync(RegistrationPayload registrationPayload);
        public Task<ILoginResponse> LoginAsync(LoginPayload loginPayload);
        public Task<ILogoutResponse> LogoutAsync(LogoutPayload logoutPayload);
        public Task<IRefreshTokenResponse> RefreshTokenAsync(RefreshTokenPayload refreshTokenPayload);
    }

    /// <summary>
    /// PRODUCTION
    /// </summary>
    #region Production
    public class AuthenticationService : IAuthenticationService
    {
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AuthenticationService(MM_DbContext dbContext, IWebHostEnvironment env ,UserManager<ApplicationUser> identityUser, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _userManager = identityUser;
            _signInManager = signInManager;
            _configuration = configuration;
            _env = env;
        }

        public async Task<IRegistrationResponse> RegisterAsync(RegistrationPayload registrationPayload)
        {
            try
            {
                DateTimeOffset currentTickAsDateTime = GameUtilities.GetCurrentTickAsDateTime();

                var newGrasslandMapSeed1980FilePath = Path.Combine(_env.ContentRootPath, "assets", "newGrasslandMapSeed1980.json");

                var newCharacterSheetSeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newCharacterSheetSeed.json");

                var newCharacterWeaponSeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newCharacterWeaponSeed.json");
                var newCharacterArmourSeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newCharacterArmourSeed.json");
                var newCharacterJewellerySeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newCharacterJewellerySeed.json");

                var newArmouryWeaponSeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newArmouryWeaponSeed.json");
                var newArmouryArmourSeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newArmouryArmourSeed.json");
                var newArmouryJewellerySeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newArmouryJewellerySeed.json");

                var newKingdomStateSeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newKingdomStateSeed.json");
                var newCharacterStateSeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newCharacterStateSeed.json");
                var newTreasuryStateSeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newTreasuryStateSeed.json");
                var newSoupkitchenStateSeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newSoupkitchenStateSeed.json");

                var newRefreshTokenSeedFilePath = Path.Combine(_env.ContentRootPath, "assets", "newRefreshTokenSeed.json");

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var user = new t_User()
                        {
                            user_name = registrationPayload.Email.Substring(0, registrationPayload.Email.IndexOf('@')).ToLower(),
                        };
                        await _dbContext.AddAsync(user);
                        await _dbContext.SaveChangesAsync();

                        var session = new t_Session()
                        {
                            fk_user_id = user.user_id,

                            session_loggedin = DateTimeOffset.MaxValue,
                            session_loggedout = DateTimeOffset.MaxValue,

                            refreshtoken = await File.ReadAllTextAsync(newRefreshTokenSeedFilePath),
                        };

                        var kingdom = new t_Kingdom()
                        {
                            fk_user_id = user.user_id,

                            kingdom_state = await File.ReadAllTextAsync(newKingdomStateSeedFilePath),
                            kingdom_map = await File.ReadAllTextAsync(newGrasslandMapSeed1980FilePath),
                        };

                        var character = new t_Character()
                        {
                            fk_user_id = user.user_id,

                            character_weapons = await File.ReadAllTextAsync(newCharacterWeaponSeedFilePath),
                            character_armour = await File.ReadAllTextAsync(newCharacterArmourSeedFilePath),
                            character_jewellery = await File.ReadAllTextAsync(newCharacterJewellerySeedFilePath),

                            character_attributes = await File.ReadAllTextAsync(newCharacterSheetSeedFilePath),
                            character_state = await File.ReadAllTextAsync(newCharacterStateSeedFilePath),
                        };

                        var soupkitchen = new t_Soupkitchen()
                        {
                            fk_user_id = user.user_id,

                            soupkitchen_state = await File.ReadAllTextAsync(newSoupkitchenStateSeedFilePath),

                            soupkitchen_updated_at_as_gametick = 0,
                            soupkitchen_updated_at_datetime = currentTickAsDateTime,
                        };

                        var treasury = new t_Treasury()
                        {
                            fk_user_id = user.user_id,

                            treasury_total = 0,

                            treasury_updated_at_as_gametick = 0,
                            treasury_updated_at_datetime = currentTickAsDateTime,

                            treasury_state = await File.ReadAllTextAsync(newTreasuryStateSeedFilePath),

                        };

                        var armoury = new t_Armoury()
                        {
                            fk_user_id = user.user_id,

                            armoury_weapons = await File.ReadAllTextAsync(newArmouryWeaponSeedFilePath),
                            armoury_armour = await File.ReadAllTextAsync(newArmouryArmourSeedFilePath),
                            armoury_jewellery = await File.ReadAllTextAsync(newArmouryJewellerySeedFilePath),
                        };


                        await _dbContext.AddAsync(session);
                        await _dbContext.AddAsync(kingdom);
                        await _dbContext.AddAsync(treasury);
                        await _dbContext.AddAsync(armoury);
                        await _dbContext.AddAsync(character);
                        await _dbContext.AddAsync(soupkitchen);


                        var identityUser = new ApplicationUser()
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserName = registrationPayload.Email,
                            Email = registrationPayload.Email,
                            CustomUserId = user.user_id,
                        };
                        var userResult = await _userManager.CreateAsync(identityUser, registrationPayload.Password);
                        var claimsResult = await _userManager.AddToRoleAsync(identityUser, "User");

                        await transaction.CommitAsync();
                        var result = new RegistrationResponse();
                        return result;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        var result = new ErrorResponse("Transaction failed, rolling back");
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                var result = new ErrorResponse("Registration failed");
                return result;
            }
        }

        public async Task<ILoginResponse> LoginAsync(LoginPayload loginPayload)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginPayload.Email);
                if (user == null)
                    return new ErrorResponse("Invalid email or password"); // email

                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginPayload.Password, lockoutOnFailure: false);

                if (!signInResult.Succeeded)
                    return new ErrorResponse("Invalid email or password"); // password

                string authToken = await GenerateAuthToken(user);
                RefreshToken refreshToken = await GenerateRefreshTokenAsync();

                string serialisedRefreshToken;
                JsonSerializer serialiser = new JsonSerializer();
                using (StringWriter sw = new StringWriter())
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serialiser.Serialize(writer, refreshToken);
                        serialisedRefreshToken = sw.ToString();
                        sw.GetStringBuilder().Clear();
                    }
                }
                t_Session session = new t_Session
                {
                    fk_user_id = user.CustomUserId,
                    session_loggedin = GameUtilities.GetCurrentTickAsDateTime(),
                    session_loggedout = DateTimeOffset.MaxValue,
                    refreshtoken = serialisedRefreshToken
                };
                await _dbContext.t_session.AddAsync(session);
                // }

                await _dbContext.SaveChangesAsync();

                return new LoginResponse
                {
                    AuthToken = authToken,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception)
            {
                return new ErrorResponse("Login failed");
            }
        }
        public async Task<ILogoutResponse> LogoutAsync(LogoutPayload logoutPayload)
        {

            try
            {
                var principle = GetTokenPrinciple(logoutPayload.AuthToken);
                List<Claim> claims = principle.Claims.ToList();
                if (principle == null)
                    return new ErrorResponse("Invalid authToken");
                var identityUser = await _userManager.FindByIdAsync(principle.Claims.ElementAt(0).Value);//claims.ElementAt(0).Value
                if (identityUser == null)
                    return new ErrorResponse("User not found");
                t_Session session = await _dbContext.t_session
                    .Where(w => w.fk_user_id == identityUser.CustomUserId && w.session_loggedout == DateTimeOffset.MaxValue)
                    .OrderBy(s => s.session_loggedin)
                    .FirstOrDefaultAsync();
                if (session == null)
                    return new ErrorResponse("No active session found");
                RefreshToken refreshToken;
                JsonSerializer serialiser = new JsonSerializer();
                using (StringReader sr = new StringReader(session.refreshtoken))
                {
                    using (JsonReader reader = new JsonTextReader(sr)) refreshToken = serialiser.Deserialize<RefreshToken>(reader);
                }
                if (!refreshToken.Token.Equals(logoutPayload.RefreshToken.Token))
                    session.refreshtoken = "Refresh token not equal to payload on Logout";

                session.session_loggedout = GameUtilities.GetCurrentTickAsDateTime();
                await _dbContext.SaveChangesAsync();
                return new LogoutResponse();
            }
            catch (Exception ex)
            {
                return new ErrorResponse("Logout failed");
            }
        }

        public async Task<IRefreshTokenResponse> RefreshTokenAsync(RefreshTokenPayload refreshTokenPayload)
        {
            try
            {
                if (refreshTokenPayload.RefreshToken.Expires <= DateTimeOffset.UtcNow.UtcDateTime)
                    return new ErrorResponse("Payload refresh token expired");
                var principle = GetTokenPrinciple(refreshTokenPayload.AuthToken);
                if (principle == null)
                    return new ErrorResponse("Invalid authToken");
                List<Claim> claims = principle.Claims.ToList();
                var identityUser = await _userManager.FindByIdAsync(principle.Claims.ElementAt(0).Value);
                if (identityUser == null)
                    return new ErrorResponse("User not found");
                t_Session session = await _dbContext.t_session
                    .Where(w => w.fk_user_id == identityUser.CustomUserId && w.session_loggedout == DateTimeOffset.MaxValue)
                    .OrderBy(s => s.session_loggedin)
                    .FirstOrDefaultAsync();
                if (session == null)
                    return new ErrorResponse("No active session found");
                RefreshToken refreshToken;
                JsonSerializer serialiser = new JsonSerializer();
                using (StringReader sr = new StringReader(session.refreshtoken))
                {
                    using (JsonReader reader = new JsonTextReader(sr)) refreshToken = serialiser.Deserialize<RefreshToken>(reader);
                }
                if (!refreshToken.Token.Equals(refreshTokenPayload.RefreshToken.Token))
                {
                    session.refreshtoken = "Refresh token not equal to payload on RefreshToken";
                    await _dbContext.SaveChangesAsync();
                    return new ErrorResponse($"{session.refreshtoken}");
                }

                var newAuthToken = await GenerateAuthToken(identityUser);
                var newRefreshToken = await GenerateRefreshTokenAsync();

                string serialisedNewRefreshToken = string.Empty;
                using (StringWriter sw = new StringWriter())
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {

                        serialiser.Serialize(writer, newRefreshToken);
                        serialisedNewRefreshToken = sw.ToString();
                        sw.GetStringBuilder().Clear();
                    }
                }
                session.refreshtoken = serialisedNewRefreshToken;
                await _dbContext.SaveChangesAsync();

                var response = new RefreshTokenResponse()
                {
                    AuthToken = newAuthToken,
                    RefreshToken = newRefreshToken
                };
                return response;
            }
            catch (Exception)
            {
                return new ErrorResponse("Refreshing token pair failed");
            }
        }
        private async Task<string> GenerateAuthToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),

            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(

                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTimeOffset.UtcNow.UtcDateTime.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiresInMinutes"])),
                //expires: DateTimeOffset.UtcNow.UtcDateTime.AddMinutes(3),
                signingCredentials: creds
            );
            string writtenToken = new JwtSecurityTokenHandler().WriteToken(token);
            return writtenToken;
        }
        public async Task<RefreshToken> GenerateRefreshTokenAsync()
        {
            RefreshToken refreshToken = new RefreshToken
            {
                Token = await GetUniqueTokenAsync(),
                Created = DateTimeOffset.UtcNow.UtcDateTime,
                Expires = DateTimeOffset.UtcNow.UtcDateTime.AddDays(1)
                //Expires = DateTimeOffset.UtcNow.UtcDateTime.AddMinutes(6)
            };

            return refreshToken;
        }

        private async Task<string> GetUniqueTokenAsync()
        {
            string token;
            bool tokenIsUnique;
            do
            {
                token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

                string serialisedToken = JsonConvert.SerializeObject(token);

                var existingToken = await _dbContext.t_session.FirstOrDefaultAsync(s => s.refreshtoken == serialisedToken);
                tokenIsUnique = existingToken == null;
            } while (!tokenIsUnique);
            return token;
        }

        private ClaimsPrincipal? GetTokenPrinciple(string token)
        {
            try
            {
                var validation = new TokenValidationParameters
                {
                    ValidateActor = false,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = false,
                    ValidateSignatureLast = false,
                    ValidateTokenReplay = false,
                    ValidateWithLKG = false,

                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!)),

                    NameClaimType = ClaimTypes.Email,//"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                    RoleClaimType = ClaimTypes.Role,//"http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                };
                ClaimsPrincipal claimsPrinciple = new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
                return claimsPrinciple;
            }
            catch (Exception)
            {
                return null;
            }

        }
    }

    #endregion

    /// <summary>
    /// DEVELOPMENT
    /// </summary>
    #region Development
    public class TestAuthenticationService : IAuthenticationService
    {

        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public TestAuthenticationService(MM_DbContext dbContext, UserManager<ApplicationUser> identityUser, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _userManager = identityUser;
            _signInManager = signInManager;
            _configuration = configuration;
            _env = env;
        }

        public async Task<IRegistrationResponse> RegisterAsync(RegistrationPayload registrationPayload)
        {
            try
            {
                DateTimeOffset currentTickAsDateTime = GameUtilities.GetCurrentTickAsDateTime();
               
                var newGrasslandMapSeed1980FilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newGrasslandMapSeed1980.json");

                var newCharacterSheetSeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newCharacterSheetSeed.json");

                var newCharacterWeaponSeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newCharacterWeaponSeed.json");
                var newCharacterArmourSeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newCharacterArmourSeed.json");
                var newCharacterJewellerySeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newCharacterJewellerySeed.json");

                var newArmouryWeaponSeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newArmouryWeaponSeed.json");
                var newArmouryArmourSeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newArmouryArmourSeed.json");
                var newArmouryJewellerySeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newArmouryJewellerySeed.json");

                var newKingdomStateSeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newKingdomStateSeed.json");
                var newCharacterStateSeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newCharacterStateSeed.json");
                var newTreasuryStateSeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newTreasuryStateSeed.json");
                var newSoupkitchenStateSeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newSoupkitchenStateSeed.json");

                var newRefreshTokenSeedFilePath = Path.Combine(_env.ContentRootPath, "Assets\\NewUserSeededData", "newRefreshTokenSeed.json");

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var user = new t_User()
                        {
                            user_name = registrationPayload.Email.Substring(0, registrationPayload.Email.IndexOf('@')).ToLower(),
                        };
                        await _dbContext.AddAsync(user);
                        await _dbContext.SaveChangesAsync();

                        var session = new t_Session()
                        {
                            fk_user_id = user.user_id,

                            session_loggedin = DateTimeOffset.MaxValue,
                            session_loggedout = DateTimeOffset.MaxValue,

                            refreshtoken = await File.ReadAllTextAsync(newRefreshTokenSeedFilePath),
                        };

                        var kingdom = new t_Kingdom()
                        {
                            fk_user_id = user.user_id,

                            kingdom_state = await File.ReadAllTextAsync(newKingdomStateSeedFilePath),
                            kingdom_map = await File.ReadAllTextAsync(newGrasslandMapSeed1980FilePath),
                        };

                        var character = new t_Character()
                        {
                            fk_user_id = user.user_id,
                            

                            character_weapons = await File.ReadAllTextAsync(newCharacterWeaponSeedFilePath),
                            character_armour = await File.ReadAllTextAsync(newCharacterArmourSeedFilePath),
                            character_jewellery = await File.ReadAllTextAsync(newCharacterJewellerySeedFilePath),

                            character_attributes = await File.ReadAllTextAsync(newCharacterSheetSeedFilePath),
                            character_state = await File.ReadAllTextAsync(newCharacterStateSeedFilePath),

                            character_isalive = true,
                        };

                        var soupkitchen = new t_Soupkitchen()
                        {
                            fk_user_id = user.user_id,

                            soupkitchen_state = await File.ReadAllTextAsync(newSoupkitchenStateSeedFilePath),

                            soupkitchen_updated_at_as_gametick = 0,
                            soupkitchen_updated_at_datetime = currentTickAsDateTime,
                        };

                        var treasury = new t_Treasury()
                        {
                            fk_user_id = user.user_id,

                            treasury_total = 0,

                            treasury_updated_at_as_gametick = 0,
                            treasury_updated_at_datetime = currentTickAsDateTime,

                            treasury_state = await File.ReadAllTextAsync(newTreasuryStateSeedFilePath),

                        };

                        var armoury = new t_Armoury()
                        {
                            fk_user_id = user.user_id,
                            
                            armoury_weapons = await File.ReadAllTextAsync(newArmouryWeaponSeedFilePath),
                            armoury_armour = await File.ReadAllTextAsync(newArmouryArmourSeedFilePath),
                            armoury_jewellery = await File.ReadAllTextAsync(newArmouryJewellerySeedFilePath),
                        };


                        await _dbContext.AddAsync(session);
                        await _dbContext.AddAsync(kingdom);
                        await _dbContext.AddAsync(treasury);
                        await _dbContext.AddAsync(armoury);
                        await _dbContext.AddAsync(character);
                        await _dbContext.AddAsync(soupkitchen);


                        var identityUser = new ApplicationUser()
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserName = registrationPayload.Email,
                            Email = registrationPayload.Email,
                            CustomUserId = user.user_id,
                        };
                        var userResult = await _userManager.CreateAsync(identityUser, registrationPayload.Password);
                        var claimsResult = await _userManager.AddToRoleAsync(identityUser, "User");

                        await transaction.CommitAsync();
                        var result = new RegistrationResponse();
                        return result;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        var result = new ErrorResponse("Transaction failed, rolling back");
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                var result = new ErrorResponse("Registration failed");
                return result;
            }
        }

        public async Task<ILoginResponse> LoginAsync(LoginPayload loginPayload)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginPayload.Email);
                if (user == null)
                    return new ErrorResponse("Invalid email or password"); // email

                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginPayload.Password, lockoutOnFailure: false);

                if (!signInResult.Succeeded)
                    return new ErrorResponse("Invalid email or password"); // password

                string authToken = await GenerateAuthToken(user);
                RefreshToken refreshToken = await GenerateRefreshTokenAsync();

                string serialisedRefreshToken;
                JsonSerializer serialiser = new JsonSerializer();
                using (StringWriter sw = new StringWriter())
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serialiser.Serialize(writer, refreshToken);
                        serialisedRefreshToken = sw.ToString();
                        sw.GetStringBuilder().Clear();
                    }
                }
                t_Session session = new t_Session
                {
                    fk_user_id = user.CustomUserId,
                    session_loggedin = GameUtilities.GetCurrentTickAsDateTime(),
                    session_loggedout = DateTimeOffset.MaxValue,
                    refreshtoken = serialisedRefreshToken
                };
                await _dbContext.t_session.AddAsync(session);
                // }

                await _dbContext.SaveChangesAsync();

                return new LoginResponse
                {
                    AuthToken = authToken,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception)
            {
                return new ErrorResponse("Login failed");
            }
        }
        public async Task<ILogoutResponse> LogoutAsync(LogoutPayload logoutPayload)
        {

            try
            {
                var principle = GetTokenPrinciple(logoutPayload.AuthToken);
                List<Claim> claims = principle.Claims.ToList();
                if (principle == null)
                    return new ErrorResponse("Invalid authToken");
                var identityUser = await _userManager.FindByIdAsync(principle.Claims.ElementAt(0).Value);//claims.ElementAt(0).Value
                if (identityUser == null)
                    return new ErrorResponse("User not found");
                t_Session session = await _dbContext.t_session
                    .Where(w => w.fk_user_id == identityUser.CustomUserId && w.session_loggedout == DateTimeOffset.MaxValue)
                    .OrderBy(s => s.session_loggedin)
                    .FirstOrDefaultAsync();
                if (session == null)
                    return new ErrorResponse("No active session found");
                RefreshToken refreshToken;
                JsonSerializer serialiser = new JsonSerializer();
                using (StringReader sr = new StringReader(session.refreshtoken))
                {
                    using (JsonReader reader = new JsonTextReader(sr)) refreshToken = serialiser.Deserialize<RefreshToken>(reader);
                }
                if (!refreshToken.Token.Equals(logoutPayload.RefreshToken.Token))
                    session.refreshtoken = "Refresh token not equal to payload on Logout";

                session.session_loggedout = GameUtilities.GetCurrentTickAsDateTime();
                await _dbContext.SaveChangesAsync();
                return new LogoutResponse();
            }
            catch (Exception ex)
            {
                return new ErrorResponse("Logout failed");
            }
        }

        public async Task<IRefreshTokenResponse> RefreshTokenAsync(RefreshTokenPayload refreshTokenPayload)
        {
            try
            {
                if (refreshTokenPayload.RefreshToken.Expires <= DateTimeOffset.UtcNow.UtcDateTime)
                    return new ErrorResponse("Payload refresh token expired");
                var principle = GetTokenPrinciple(refreshTokenPayload.AuthToken);
                if (principle == null)
                    return new ErrorResponse("Invalid authToken");
                List<Claim> claims = principle.Claims.ToList();
                var identityUser = await _userManager.FindByIdAsync(principle.Claims.ElementAt(0).Value);
                if (identityUser == null)
                    return new ErrorResponse("User not found");
                t_Session session = await _dbContext.t_session
                    .Where(w => w.fk_user_id == identityUser.CustomUserId && w.session_loggedout == DateTimeOffset.MaxValue)
                    .OrderBy(s => s.session_loggedin)
                    .FirstOrDefaultAsync();
                if (session == null)
                    return new ErrorResponse("No active session found");
                RefreshToken refreshToken;
                JsonSerializer serialiser = new JsonSerializer();
                using (StringReader sr = new StringReader(session.refreshtoken))
                {
                    using (JsonReader reader = new JsonTextReader(sr)) refreshToken = serialiser.Deserialize<RefreshToken>(reader);
                }
                if (!refreshToken.Token.Equals(refreshTokenPayload.RefreshToken.Token))
                {
                    session.refreshtoken = "Refresh token not equal to payload on RefreshToken";
                    await _dbContext.SaveChangesAsync();
                    return new ErrorResponse($"{session.refreshtoken}");
                }

                var newAuthToken = await GenerateAuthToken(identityUser);
                var newRefreshToken = await GenerateRefreshTokenAsync();

                string serialisedNewRefreshToken = string.Empty;
                using (StringWriter sw = new StringWriter())
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {

                        serialiser.Serialize(writer, newRefreshToken);
                        serialisedNewRefreshToken = sw.ToString();
                        sw.GetStringBuilder().Clear();
                    }
                }
                session.refreshtoken = serialisedNewRefreshToken;
                await _dbContext.SaveChangesAsync();

                var response = new RefreshTokenResponse()
                {
                    AuthToken = newAuthToken,
                    RefreshToken = newRefreshToken
                };
                return response;
            }
            catch (Exception)
            {
                return new ErrorResponse("Refreshing token pair failed");
            }
        }
        private async Task<string> GenerateAuthToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),

            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(

                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTimeOffset.UtcNow.UtcDateTime.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiresInMinutes"])),
                //expires: DateTimeOffset.UtcNow.UtcDateTime.AddMinutes(3),
                signingCredentials: creds
            );
            string writtenToken = new JwtSecurityTokenHandler().WriteToken(token);
            return writtenToken;
        }
        public async Task<RefreshToken> GenerateRefreshTokenAsync()
        {
            RefreshToken refreshToken = new RefreshToken
            {
                Token = await GetUniqueTokenAsync(),
                Created = DateTimeOffset.UtcNow.UtcDateTime,
                Expires = DateTimeOffset.UtcNow.UtcDateTime.AddDays(1)
                //Expires = DateTimeOffset.UtcNow.UtcDateTime.AddMinutes(6)
            };

            return refreshToken;
        }

        private async Task<string> GetUniqueTokenAsync()
        {
            string token;
            bool tokenIsUnique;
            do
            {
                token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

                string serialisedToken = JsonConvert.SerializeObject(token);

                var existingToken = await _dbContext.t_session.FirstOrDefaultAsync(s => s.refreshtoken == serialisedToken);
                tokenIsUnique = existingToken == null;
            } while (!tokenIsUnique);
            return token;
        }

        private ClaimsPrincipal? GetTokenPrinciple(string token)
        {
            try
            {
                var validation = new TokenValidationParameters
                {
                    ValidateActor = false,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = false,
                    ValidateSignatureLast = false,
                    ValidateTokenReplay = false,
                    ValidateWithLKG = false,

                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!)),

                    NameClaimType = ClaimTypes.Email,//"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                    RoleClaimType = ClaimTypes.Role,//"http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                };
                ClaimsPrincipal claimsPrinciple = new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
                return claimsPrinciple;
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}
#endregion

//    CharacterState characterState = new CharacterState()
//    {
//        State = new State()
//        {
//            character_cooldown = 0,
//            character_died = DateTimeOffset.MinValue,
//            character_isactive = true,
//            character_isalive = true,
//            character_iscooldown = false
//        }
//};
//CharacterSheet characterSheet = new CharacterSheet()
//{
//    AttributeList = new List<BaseAttribute>
//            {
//                new CharacterLevel { Level = 1 },
//                new Constitution { Level = 50 },
//                new Defence { Level = 1 },
//                new Luck{ Level = 1 },
//                new Stamina{ Level = 1 },
//                new Strength{ Level = 1 }
//            }
//};
//CharacterInventory characterInventory = new CharacterInventory()
//{
//    WeaponList = new List<BaseWeapon>
//    {

//    },
//    ArmourList = new List<BaseArmour>
//            {
//                new Torso {
//                    Name = "Sheep's-wool Woven Shirt",
//                    ArmourTier = 1,
//                    DefenceRating = 2
//                },
//                new Legs
//                {
//                    Name = "Leather Chaps",
//                    ArmourTier = 1,
//                    DefenceRating = 5
//                },
//                new Feet
//                {
//                    Name = "Leather Sandals",
//                    ArmourTier = 1,
//                    DefenceRating = 2
//                }
//            },
//    JewelleryList = new List<BaseJewellery>
//            {
//                new Ring
//                {
//                    JewelleryTier = 1,
//                    Name = "Wedding Ring",
//                    ConstitutionBoon = 5,
//                    LuckBoon = 1,
//                    StaminaBoon = 1
//                }
//            }
//};
//EquipmentInventory armouryInventory = new EquipmentInventory()
//{
//    WeaponList = new List<BaseWeapon>
//    {
//        new Sword
//        {
//            Name = "Iron Sword",
//            DamageRating = 10,
//            Unique = false
//        }
//    },
//    ArmourList = new List<BaseArmour>
//    {
//    },
//    JewelleryList = new List<BaseJewellery>
//    {
//    }
//};

#region Legacy Code

//public TestAuthenticationService(MM_DbContext dbContext)
//{
//    _dbContext = dbContext;
//}

////private readonly string FB_URL = "https://identitytoolkit.googleapis.com/v1";
////private readonly string FB_URL_AUTH = "/accounts";
////private readonly string FB_URL_TOKEN = "https://securetoken.googleapis.com/v1/token";
////private readonly string FB_URL_APIKEY = $"?key={Environment.GetEnvironmentVariable("FIREBASE_API_KEY")}";

///// <summary>
///// Register user with FirebaseAuth and PostgresDB
///// </summary>
///// 
///// <param name="registrationPayload"></param>
///// <returns>
///// RegistrationResponse or FirebaseError
///// </returns>
//public async Task<IRegistrationResponse> RegisterAsync(RegistrationPayload registrationPayload)
//{
//    try
//    {
//        string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signUp{FB_URL_APIKEY}";
//        using (var client = new HttpClient())
//        {
//            var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(registrationPayload), Encoding.UTF8, "application/json"));
//            string responseBody = await response.Content.ReadAsStringAsync();

//            if (response.IsSuccessStatusCode)
//            {
//                RegistrationResponse registrationResponse = JsonConvert.DeserializeObject<RegistrationResponse>(responseBody);

//                t_User user = new t_User
//                {
//                    user_name = registrationResponse.Email.Substring(0, registrationResponse.Email.IndexOf('@')),
//                };

//                await _dbContext.AddAsync(user);
//                await _dbContext.SaveChangesAsync();

//                return registrationResponse;
//            }
//            else //https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-8.0
//            {
//                FirebaseError firebaseErrorResponse = JsonConvert.DeserializeObject<FirebaseError>(responseBody);
//                return firebaseErrorResponse;
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
//        //TODO: Implement error logging from service classes and throw from service->controller->client and force client to exit to menu

//        //In future implementations:
//        //1. May implement PostgresDB error logging

//    }
//    return null; //Reaches here only if Internal Server Error?
//}

///*       SIGNIN DTO
//________________________________|
//LoginPayload                   |
//________________________________|
//                                |
//email	          |  string	    |
//password	      |  string	    |
//returnSecureToken |	 boolean	|
//                  |             |
//________________________________|
//LoginResponse    |             |
//________________________________|
//                  |             |
//idToken	          |  string	    |
//email	          |  string	    |
//refreshToken	  |  string	    |
//expiresIn         |  string	    |
//localId	          |  string	    |
//registered        |  boolean	|
// */

////[RequireHttps]
//public async Task<ILoginResponse> LoginAsync(LoginPayload loginPayload)
//{
//    try
//    {
//        string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signInWithPassword{FB_URL_APIKEY}";
//        using (var client = new HttpClient())
//        {
//            var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(loginPayload), Encoding.UTF8, "application/json"));
//            string responseBody = await response.Content.ReadAsStringAsync();

//            if (response.IsSuccessStatusCode)
//            {
//                LoginResponse signInResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody);

//                t_User user = await _dbContext.t_user.FirstAsync(u => u.user_fb_uuid == signInResponse.ServerId);  //fk_user_id == user.user_id
//                System.Diagnostics.Debug.WriteLine($"{user.user_id} - {user.user_fb_uuid} - {user.user_name}");
//                t_Session session = new t_Session()
//                {
//                    fk_user_id = user.user_id,
//                    session_authtoken = signInResponse.AccessToken,
//                    session_refreshtoken = signInResponse.RefreshToken,
//                    session_loggedin = DateTimeOffset.UtcNow.UtcDateTime,
//                    session_loggedout = DateTimeOffset.MinValue,
//                };

//                await _dbContext.AddAsync(session);
//                await _dbContext.SaveChangesAsync();

//                return signInResponse;
//            }
//            else
//            {
//                FirebaseError firebaseErrorResponse = JsonConvert.DeserializeObject<FirebaseError>(responseBody);
//                return firebaseErrorResponse;
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Diagnostics.Debug.WriteLine($"Login failed: {ex.Message}");
//    }
//    return null;
//}

////[RequireHttps]
//public async Task<ILogoutResponse> LogoutAsync(LogoutPayload logoutPayload)
//{
//    try
//    {
//        t_User user = await _dbContext.t_user.FirstAsync(u => u.user_fb_uuid == logoutPayload.ServerId);
//        t_Session recentSession = await _dbContext.t_session
//            .Where(s => s.fk_user_id == user.user_id)
//            .OrderByDescending(s => s.session_loggedin)
//            .FirstAsync();

//        if (recentSession.session_loggedout == DateTimeOffset.MinValue)
//        {
//            recentSession.session_authtoken = "0";
//            recentSession.session_refreshtoken = "0";
//            recentSession.session_loggedout = DateTimeOffset.UtcNow.UtcDateTime;

//            await _dbContext.SaveChangesAsync();

//            LogoutResponse signOutResponse = new LogoutResponse()
//            {
//                AccessToken = recentSession.session_sessiontoken,
//                RefreshToken = recentSession.session_refreshtoken,
//                ServerId = user.user_fb_uuid
//            };
//            return signOutResponse;
//        }
//        else
//        {
//            System.Diagnostics.Debug.WriteLine($"Logout failed: {user.user_id} - {recentSession.session_id}");
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Diagnostics.Debug.WriteLine($"Logout failed: {ex.Message}");

//    }
//    return null;
//}

//public async Task<IRefreshTokenResponse> RefreshTokenAsync(RefreshTokenPayload refreshTokenPayload)
//{
//    ////needs to update user's existing refresh + id tokens in PSQL DB
//    //try
//    //{
//    //    string fb_uri = $"{FB_URL_TOKEN}{FB_URL_APIKEY}";
//    //    // var deserialisedRefreshTokenPayload = JsonConvert.DeserializeObject<RefreshTokenPayload>(refreshTokenPayload);

//    //    //var content = new FormUrlEncodedContent(new[]
//    //    // {
//    //    //   new KeyValuePair<string, string>("grant_type", "refresh_token"),
//    //    //   new KeyValuePair<string, string>("refresh_token", payload.RefreshToken)
//    //    // });
//    //    using (var client = new HttpClient())
//    //    {
//    //        var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(refreshTokenPayload), Encoding.UTF8, "application/json"));
//    //        if (response.IsSuccessStatusCode)
//    //        {
//    //            // Read the response content
//    //            string responseBody = await response.Content.ReadAsStringAsync();

//    //            // Deserialize the response JSON to a RefreshTokenResponse object
//    //            var refreshTokenResponse = JsonConvert.DeserializeObject<RefreshTokenResponse>(responseBody);

//    //            //IS THIS SERIALISED ALREADY? IF SO NO NEED TO RESERIALISE

//    //            // Return the RefreshTokenResponse object

//    //            return refreshTokenResponse;
//    //        }
//    //        else
//    //        {
//    //            string error = await response.Content.ReadAsStringAsync();

//    //            if (error.Contains("TOKEN_EXPIRED"))//MOVE TO CONTROLLER???
//    //            {
//    //                Console.WriteLine(error);
//    //            }

//    //        }
//    //    }
//    //}
//    //catch (Exception ex)
//    //{
//    //    System.Diagnostics.Debug.WriteLine($"RefreshToken Failed: {ex}");
//    //}
//    return null;
//}
//    }

//}



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
//                            user_fb_uuid = deserialisedResponse.ServerId
//                        };
//                        t_Session session = new t_Session()
//                        {
//                            user = user,
//                            session_authtoken = deserialisedResponse.AccessToken,
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
//public async Task<ILoginResponse> LoginAsync(LoginPayload loginPayload)
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
//                LoginResponse deserialisedResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody);

//                t_User user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == deserialisedResponse.ServerId);  //fk_user_id == user.user_id
//                var session = _dbContext.t_session.FirstOrDefault(s => s.user == user);
//                session.session_authtoken = deserialisedResponse.AccessToken;
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
//public async Task<ILogoutResponse> LogoutAsync(LogoutPayload logoutPayload)
//{
//    //var deserialisedLogoutPayload = JsonConvert.DeserializeObject<LogoutPayload>(logoutPayload);
//    await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(JsonConvert.SerializeObject(logoutPayload.ServerId));

//    var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == logoutPayload.ServerId);
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

//public async Task<IRefreshTokenResponse> RefreshTokenAsync(RefreshTokenPayload refreshTokenPayload)
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
    //    public Task<LoginPayload> LoginAsync(LoginPayload loginPayload);
    //    public Task<LogoutPayload> LogoutAsync(LogoutPayload logoutPayload);
    //    public Task<RefreshTokenPayload> RefreshTokenAsync(RefreshTokenPayload refreshTokenPayload);
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
    //                        user_fb_uuid = userRecord.ServerId
    //                    };
    //                    t_Session session = new t_Session()
    //                    {
    //                        user = user,
    //                        session_authtoken = userRecord.AccessToken,
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
//    public async Task<string> LoginAsync(LoginPayload loginPayload)
//    {
//        LoginResponse userRecord = null;
//        try
//        {
//            string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signInWithPassword{FB_URL_APIKEY}";

//            using (var client = new HttpClient())
//            {
//                var response = await client.PostAsync(fb_uri, new StringContent(JsonConvert.SerializeObject(loginPayload), Encoding.UTF8, "application/json"));
//                if (response.IsSuccessStatusCode)
//                {
//                    string responseBody = await response.Content.ReadAsStringAsync();
//                    userRecord = JsonConvert.DeserializeObject<LoginResponse>(responseBody);
//                    var handler = new JwtSecurityTokenHandler();
//                    //var jsonToken = handler.ReadToken(signInResponse.AccessToken) as JwtSecurityToken;
//                    //if (jsonToken == null)
//                    //{
//                    //    throw new InvalidOperationException("Invalid JWT token");
//                    //}
//                    t_User user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == userRecord.ServerId);
//                    var session = _dbContext.t_session.FirstOrDefault(s => s.user == user);//fk_user_id == user.user_id
//                    session.session_authtoken = userRecord.AccessToken;
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
//    public async Task<bool> LogoutAsync(LogoutPayload logoutPayload)
//    {
//        //var deserialisedLogoutPayload = JsonConvert.DeserializeObject<LogoutPayload>(logoutPayload);
//        await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(JsonConvert.SerializeObject(logoutPayload.ServerId));

//        var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == logoutPayload.ServerId);
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

//    public async Task<string> RefreshTokenAsync(RefreshTokenPayload refreshTokenPayload)
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
//                    var jsonToken = handler.ReadToken(signInResponse.AccessToken) as JwtSecurityToken;
//                    if (jsonToken == null)
//                    {
//                        throw new InvalidOperationException("Invalid JWT token");
//                    }
//                    t_User user = new t_User
//                    {
//                        user_name = signInResponse.Email.Substring(0, payload.Email.IndexOf('@')),
//                        user_fb_uuid = signInResponse.ServerId
//                    };
//                    t_Session session = new t_Session()
//                    {
//                        user = user,
//                        session_authtoken = signInResponse.AccessToken,
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

//    public async Task<LoginResponse> LoginAsync(LoginPayload payload)
//    {
//        LoginResponse signInResponse = null;
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
//                    signInResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody);
//                    var handler = new JwtSecurityTokenHandler();
//                    var jsonToken = handler.ReadToken(signInResponse.AccessToken) as JwtSecurityToken;
//                    if (jsonToken == null)
//                    {
//                        throw new InvalidOperationException("Invalid JWT token");
//                    }
//                    t_User user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == signInResponse.ServerId);
//                    var session = _dbContext.t_session.FirstOrDefault(s => s.user == user);//fk_user_id == user.user_id
//                    session.session_authtoken = signInResponse.AccessToken;
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

//    public async Task<bool> LogoutAsync(LogoutPayload payload)
//    {
//        var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == payload.ServerId);
//        var session = _dbContext.t_session.Last(s => s.fk_user_id == t_user.user_id);
//        try
//        {
//            session.session_authtoken = payload.AccessToken;
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

//private readonly string FB_URL = "https://identitytoolkit.googleapis.com/v1";
//private readonly string FB_URL_AUTH = "/accounts";
//private readonly string FB_URL_TOKEN = "https://securetoken.googleapis.com/v1/token";
//private readonly string FB_URL_APIKEY = $"?key={Environment.GetEnvironmentVariable("FIREBASE_API_KEY")}";
#endregion


#region Misc Code


//public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenPayload payload)
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
        var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(authModel.AccessToken);
        var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == decodedToken.Subject);
        // await FirebaseAuth.DefaultInstance.
        t_Session session = new t_Session()
        {
            user = t_user, //BELONGS TO...

            //STORE ENCODED CLIENT TOKENS
            session_authtoken = authModel.AccessToken,
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
//            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(authModel.AccessToken);
//            var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == decodedToken.Subject);

//            t_Session session = new t_Session()
//            {
//                user = t_user, //BELONGS TO...

//                //STORE ENCODED CLIENT TOKENS
//                session_authtoken = authModel.AccessToken,
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
//        var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(authModel.AccessToken);
//        var t_user = _dbContext.t_user.FirstOrDefault(u => u.user_fb_uuid == decodedToken.Subject);

//        //GET POSTGRES SESSION FOR USER
//        var session = _dbContext.t_session.Last(s => s.fk_user_id == t_user.user_id);

//        try
//        {
//            //OVERWRITE LATEST SESSION WITH ENCODED CLIENT TOKENS AND LOGOUT TIME
//            session.session_authtoken = authModel.AccessToken;
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

