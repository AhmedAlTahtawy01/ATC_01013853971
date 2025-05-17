using BusinessLogic.Services;
using BusinessLogic.Helpers;
using DataAccess.DataConnection;
using DataAccess.Repositories;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500",
            "http://127.0.0.1:5501", "https://localhost:7107",
            "https://ahmedhamdy-areeb-eventbooking.netlify.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Event Booking API",
        Version = "v1",
        Description = "API for event booking system",
    });
});

// Register dependencies (DI)
// Connection Settings
builder.Services.AddScoped<ConnectionSettings>();

// Shared
builder.Services.AddScoped<Shared>();

// User Repo
builder.Services.AddScoped<UserRepo>();
// User Service
builder.Services.AddScoped<UserService>();

// Role Repo
builder.Services.AddScoped<RoleRepo>();
// Role Service
builder.Services.AddScoped<RoleService>();

// Event Repo
builder.Services.AddScoped<EventRepo>();
// Event Service
builder.Services.AddScoped<EventService>();

// Booking Repo
builder.Services.AddScoped<BookingRepo>();
// Booking Service
builder.Services.AddScoped<BookingService>();

// Tag Repo
builder.Services.AddScoped<TagRepo>();
// Tag Service
builder.Services.AddScoped<TagService>();

// EventTag Repo
builder.Services.AddScoped<EventTagRepo>();
// EventTag Service
builder.Services.AddScoped<EventTagService>();

// Register controllers with different namespace
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly);

// Bind JWT settings from configuration
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JWT>(jwtSection);


// Register JWT service as a singleton for DI
var jwtSettings = jwtSection.Get<JWT>() ?? throw new InvalidOperationException("JWT settings are missing in configuration.");
builder.Services.AddSingleton(jwtSettings);

// Register Token Service
builder.Services.AddScoped<TokenService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                message = "Unauthorized: Invalid or missing token"
            });
            return context.Response.WriteAsync(result);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                message = "Forbidden: You do not have access to this resource"
            });
            return context.Response.WriteAsync(result);
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Booking API v1");
        c.RoutePrefix = "";
    });
//}

// Middleware pipeline
app.UseHttpsRedirection();

// Add CORS middleware before routing
app.UseCors("AllowFrontend");

// User routing for attribute-based controllers
app.UseRouting();

// For Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

app.Run();