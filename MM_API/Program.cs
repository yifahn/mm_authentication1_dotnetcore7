using Microsoft.EntityFrameworkCore;
using MM_API.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Google.Apis.Auth.OAuth2;
using PSQLLibrary.Game.MM_Framework.Treasury;
using PSQLLibrary.Game.MM_Framework.Soupkitchen;

namespace MM_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CREATE WEBAPP BUILDER
            var builder = WebApplication.CreateBuilder(args);

            // CLIENT AUTHORISATION via FIREBASE idToken(authorisation idToken as request header) && API Authorisation - configure bearer token 
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var projectId = "monomonarch";
                    options.Authority = $"https://securetoken.google.com/{projectId}";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"https://securetoken.google.com/{projectId}",
                        ValidateAudience = true,
                        ValidAudience = projectId,
                        ValidateLifetime = true
                    };
                });

            // ADD SERVICES - DI ALLOWS TEST / NONTEST SERVICES
            builder.Services
                .AddScoped<IArmouryService, TestArmouryService>()
                .AddScoped<IAuthenticationService, TestAuthenticationService>()
                .AddScoped<IBattleboardService, TestBattleboardService>()
                .AddScoped<ICharacterService, TestCharacterService>()
                .AddScoped<IKingdomService, TestKingdomService>()
                .AddScoped<ISoupkitchenService, TestSoupkitchenService>()
                .AddScoped<ITreasuryService, TestTreasuryService>();

            // POSTGRESQL CONNECTION
            builder.Services.AddDbContext<MM_DbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Db")));
            System.Diagnostics.Debug.WriteLine($"Connection String: {builder.Configuration.GetConnectionString("Db")}");

            // ADD CONTROLLERS
            builder.Services.AddControllers().AddNewtonsoftJson(o => { });

            // ADD SWASHBUCKLE/SWAGGER
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // NEW WEBAPP
            var app = builder.Build();

            // USE SWAGGER IF DEVELOPING
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // MISC
            app.UseAuthorization();
            app.MapControllers();

            // RUN
            app.Run();
        }
    }
}
