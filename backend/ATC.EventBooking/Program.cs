using ATC.BusinessLogic.Services;
using ATC.DataAccess.DataConnection;
using ATC.DataAccess.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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
builder.Services.AddScoped<ConnectionSettings>();
builder.Services.AddScoped<UserRepo>();
builder.Services.AddScoped<UserService>();

// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Booking API v1");
        c.RoutePrefix = ""; 
    });
}

// Middleware pipeline
app.UseHttpsRedirection();

// User routing for attribute-based controllers
app.UseRouting();

// For Authentication/Authorization later
// app.UseAuthentication();
// app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

app.Run();