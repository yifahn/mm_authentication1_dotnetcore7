using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
namespace MM_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CREATE WEBAPP BUILDER
            var builder = WebApplication.CreateBuilder(args);



            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
                };
            });

    //        builder.Services.AddIdentityApiEndpoints<IdentityUser>(options =>
    //        {
    //            options.Password.RequiredLength = 6;
    //            options.Password.RequireNonAlphanumeric = false;
    //            options.Password.RequireDigit = false;
    //            options.Password.RequireUppercase = false;
    //            options.Password.RequireLowercase = false;
    //        })
    // .AddRoles<IdentityRole>()
    //.AddEntityFrameworkStores<MM_DbContext>();


            //builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme, o =>
            //{
            //    o.BearerTokenExpiration = TimeSpan.FromSeconds(60);
            //});

            // ADD SERVICES - DI ALLOWS TEST / NONTEST SERVICES
            builder.Services

                .AddScoped<Services.IArmouryService, Services.TestArmouryService>()
                //.AddScoped<Services.IAuthenticationService, Services.TestAuthenticationService>()
                .AddScoped<Services.IBattleboardService, Services.TestBattleboardService>()
                .AddScoped<Services.ICharacterService, Services.TestCharacterService>()
                .AddScoped<Services.IKingdomService, Services.TestKingdomService>()
                .AddScoped<Services.ISoupkitchenService, Services.TestSoupkitchenService>()
                .AddScoped<Services.ITreasuryService, Services.TestTreasuryService>();

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
