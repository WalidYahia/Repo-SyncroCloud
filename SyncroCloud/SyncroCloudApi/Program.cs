using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.Extensions;
using SyncroApplicationLayer.Interfaces;
using SyncroApplicationLayer.Services;
using SyncroCloudApi.Auth.Extensions;
using SyncroCloudApi.Auth.Middleware;
using SyncroInfraLayer.Data;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<SyncroDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Auth module (Identity + JWT + API Key services)
builder.Services.AddAuthModule(builder.Configuration);

// Domain services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IDeviceSensorService, DeviceSensorService>();
builder.Services.AddScoped<IDeviceReadingService, DeviceReadingService>();
builder.Services.AddScoped<IAlarmLookupService, AlarmLookupService>();
builder.Services.AddScoped<IDeviceScenarioService, DeviceScenarioService>();

// MQTT background service
builder.Services.AddMqttService();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

var app = builder.Build();

// Auto-apply migrations and seed roles on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SyncroDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<SyncroInfraLayer.Identity.AppRole>>();
    foreach (var role in new[] { "SuperAdmin", "TenantAdmin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new SyncroInfraLayer.Identity.AppRole(role));
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();
