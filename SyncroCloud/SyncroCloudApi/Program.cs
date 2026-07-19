using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.Extensions;
using SyncroApplicationLayer.Interfaces;
using SyncroApplicationLayer.Services;
using SyncroCloudApi.Auth.Extensions;
using SyncroCloudApi.Auth.Middleware;
using SyncroCloudApi.Exceptions;
using SyncroCloudApi.Hubs;
using SyncroCloudApi.Services;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Identity;

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
builder.Services.AddHttpClient<ILocationSyncService, LocationSyncService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IDeviceSensorService, DeviceSensorService>();
builder.Services.AddScoped<IDeviceReadingService, DeviceReadingService>();
builder.Services.AddScoped<IAlarmLookupService, AlarmLookupService>();
builder.Services.AddScoped<IDeviceScenarioService, DeviceScenarioService>();
builder.Services.AddScoped<IDeviceActionLogService, DeviceActionLogService>();
builder.Services.AddScoped<ISmartHomeService, SmartHomeService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// SignalR — INotificationService must be registered before MqttService is resolved
builder.Services.AddSignalR();
builder.Services.AddSingleton<INotificationService, SignalRNotificationService>();

// MQTT background service
builder.Services.AddMqttService();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        // Allow any origin. SetIsOriginAllowed reflects the request origin, which
        // (unlike AllowAnyOrigin) is compatible with AllowCredentials.
        policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));
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

    var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<AppRole>>();
    foreach (var role in new[] { "SuperAdmin", "TenantAdmin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new AppRole(role));
    }

    // Seed privileges
    foreach (var (code, name) in PrivilegeCodes.All)
    {
        if (!await db.Privileges.AnyAsync(p => p.Code == code))
            db.Privileges.Add(new Privilege { Id = Guid.NewGuid(), Code = code, Name = name });
    }
    await db.SaveChangesAsync();

    // Assign all privileges to SuperAdmin and TenantAdmin
    var allPrivileges = await db.Privileges.ToListAsync();
    foreach (var roleName in new[] { AppRoles.SuperAdmin, AppRoles.TenantAdmin })
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null) continue;
        foreach (var priv in allPrivileges)
        {
            if (!await db.RolePrivileges.AnyAsync(rp => rp.RoleId == role.Id && rp.PrivilegeId == priv.Id))
                db.RolePrivileges.Add(new RolePrivilege { RoleId = role.Id, PrivilegeId = priv.Id });
        }
    }
    await db.SaveChangesAsync();

    // Seed the bootstrap SuperAdmin user (skipped if a user with the same phone already exists)
    const string superAdminPhone = "01068406116";
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<AppUser>>();
    if (!await userManager.Users.AnyAsync(u => u.PhoneNumber == superAdminPhone))
    {
        var superAdmin = new AppUser
        {
            Id          = Guid.NewGuid(),
            UserName    = superAdminPhone,
            PhoneNumber = superAdminPhone,
            FirstName   = "superAdmin",
            LastName    = string.Empty,
            IsActive    = true
        };
        var created = await userManager.CreateAsync(superAdmin, "ww123456");
        if (created.Succeeded)
            await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseCors();

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();
app.MapHub<SyncroHub>("/hubs/syncro");

app.Run();
