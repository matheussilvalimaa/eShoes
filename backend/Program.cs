using eShoes.Context;
using eShoes.Authentication;
using eShoes.Services;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.AspNetCore.StaticFiles;

// Load .env
Env.Load();

var builder = WebApplication.CreateBuilder(args);

//CORS
builder.Services.AddCors(options => 
{
    options.AddPolicy("Allow5500", policy => 
    {
        policy.WithOrigins("https://localhost:5500")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

//Logger configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5032, listenOptions =>
    {
        var certPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".aspnet", "https", "backend.pfx");
        
        if (!System.IO.File.Exists(certPath))
        {
            throw new FileNotFoundException($"File not found: {certPath}");
        }

        listenOptions.UseHttps(certPath, "SenhaForte123");
    });
});

//Stripe config
var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
if (!string.IsNullOrEmpty(stripeSecretKey))
{
    StripeConfiguration.ApiKey = stripeSecretKey;
}

//JWT secret
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
if (string.IsNullOrEmpty(jwtSecretKey) || jwtSecretKey.Length < 32)
{
    throw new Exception("JWT_SECRET_KEY not configured or too short.");
}

//Database
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("CONNECTION_STRING not configured.");
}

builder.Services.AddDbContext<eShoesDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

//Services
builder.Services.AddScoped<JwtService>(provider =>
    new JwtService(
        Environment.GetEnvironmentVariable("JWT_SECRET_KEY"),
        provider.GetRequiredService<ILogger<JwtService>>()
    ));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<eShoes.Services.ProductService>();
builder.Services.AddHttpClient("Stripe", client => 
{
    client.BaseAddress = new Uri("https://api.stripe.com/");
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", stripeSecretKey);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//JWT authentication configuration
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, 
        ValidateAudience = false, 
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero 
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["JWT"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings = { [".svg"] = "image/svg+xml" }
    },
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "https://localhost:5500");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
    }
});
app.UseExceptionHandler("/error");

// Endpoint para tratar exceções
app.Map("/error", (HttpContext context) =>
{
    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
    var exception = exceptionHandlerPathFeature?.Error;

    var result = System.Text.Json.JsonSerializer.Serialize(new { message = exception?.Message });
    context.Response.ContentType = "application/json";
    return context.Response.WriteAsync(result);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Allow5500");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
