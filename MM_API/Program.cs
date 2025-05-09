using System.Text;

using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI;

using MM_API.Database.Postgres.DbSchema;
using MM_API.Database.Postgres;
using System.Security.Claims;

using Npgsql.EntityFrameworkCore.PostgreSQL;
using Npgsql;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using MonoMonarchGameFramework.Game;

namespace MM_API
{
    public class Program
    {

        public static void Main(string[] args)
        {
            //CREATE WEBAPP BUILDER
            var builder = WebApplication.CreateBuilder(args);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new DeserialisationSupport());



            //NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

            // Npgsql JSON.NET Configuration (Use Newtonsoft.Json)


            //var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("Db"));
            //dataSourceBuilder.UseJsonNet(settings); //settings // Switch to Newtonsoft.Json
            //var dataSource = dataSourceBuilder.Build();

            //builder.Services.AddDbContext<MM_DbContext>(options =>
            //options.UseNpgsql(dataSource));

            //// POSTGRESQL CONNECTION
            builder.Services.AddDbContext<MM_DbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Db")));
            System.Diagnostics.Debug.WriteLine($"Connection String: {builder.Configuration.GetConnectionString("Db")}");


            #region Authentication & Authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
                // options.AddPolicy("NewGamePolicy", policy => policy.RequireClaim("NewGame", "true"));
                options.AddPolicy("AllowExpiredTokens", policy =>
                {
                    policy.AddAuthenticationSchemes("RefreshTokenScheme");
                    policy.RequireAuthenticatedUser();
                });

            });


            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<MM_DbContext>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.Email,  //"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                    RoleClaimType = ClaimTypes.Role, //"http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)),

                    ClockSkew = TimeSpan.Zero,
                    RequireSignedTokens = true,
                    SaveSigninToken = true //required to access token from header
                };
            })
                .AddJwtBearer("RefreshTokenScheme", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.Email,
                        RoleClaimType = ClaimTypes.Role,

                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = false, // Skip lifetime validation for refresh tokens
                        ValidateIssuerSigningKey = true,

                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)),

                        ClockSkew = TimeSpan.Zero
                    };
                }); 
            #endregion

           // builder.Services.AddHttpContextAccessor();

            // ADD SERVICES - DI ALLOWS TEST / NONTEST SERVICES
            builder.Services

                .AddScoped<Services.IArmouryService, Services.TestArmouryService>()
                .AddScoped<Services.IAuthenticationService, Services.TestAuthenticationService>()
                .AddScoped<Services.IBattleboardService, Services.TestBattleboardService>()
                .AddScoped<Services.ICharacterService, Services.TestCharacterService>()
                .AddScoped<Services.IKingdomService, Services.TestKingdomService>()
                .AddScoped<Services.ISoupkitchenService, Services.TestSoupkitchenService>()
                .AddScoped<Services.ITreasuryService, Services.TestTreasuryService>();


            // ADD CONTROLLERS
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                //options.SerializerSettings.Converters.Add(new DeserialisationSupport());

            });

            // ADD SWASHBUCKLE/SWAGGER
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
         
            // NEW WEBAPP
            var app = builder.Build();
       
            //app.MapIdentityApi<IdentityUser>();

            // USE SWAGGER IF DEVELOPING
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // MISC
            app.UseAuthentication();

            app.UseAuthorization();
            app.MapControllers();

            // RUN
            app.Run();
        }
    }
}
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
//{
//    builder.Configuration.Bind("JwtSettings", options);


//});

//builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
//{
//    options.Password.RequiredLength = 6;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequireDigit = false;
//    options.Password.RequireUppercase = false;
//    options.Password.RequireLowercase = false;
//});
//.AddEntityFrameworkStores<MM_DbContext>()
//.AddDefaultTokenProviders();



//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
//    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
//});




//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(o =>
//    {
//        o.
//    });


//adding jwt auth
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            //define which claim requires to check
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            //store the value in appsettings.json
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Issuer"],
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//        };
//    });

// CLIENT AUTHORISATION via FIREBASE idToken(authorisation idToken as request header) && API Authorisation - configure bearer token
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        var projectId = "monomonarch";
//        options.Authority = $"https://securetoken.google.com/{projectId}";
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidIssuer = $"https://securetoken.google.com/{projectId}",
//            ValidateAudience = true,
//            ValidAudience = projectId,
//            ValidateLifetime = true
//        };
//    });
