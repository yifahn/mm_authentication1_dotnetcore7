using Microsoft.EntityFrameworkCore;
using MM_API.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using System.Text;


namespace MM_API
{
    public class Program
    {

        public static void Main(string[] args)
        {
            //CREATE WEBAPP BUILDER
            var builder = WebApplication.CreateBuilder(args);

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
                .AddScoped<Services.IAuthenticationService, TestAuthenticationService>()
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
            //app.UseAuthentication();
            //app.UseAuthorization();
            app.MapControllers();

            // RUN
            app.Run();
        }
    }
}
