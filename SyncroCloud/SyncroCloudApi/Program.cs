using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.Extensions;
using SyncroApplicationLayer.Interfaces;
using SyncroApplicationLayer.Services;
using SyncroInfraLayer.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SyncroDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IDeviceSensorService, DeviceSensorService>();
builder.Services.AddScoped<IDeviceReadingService, DeviceReadingService>();
builder.Services.AddScoped<IAlarmLookupService, AlarmLookupService>();
builder.Services.AddScoped<IDeviceScenarioService, DeviceScenarioService>();

builder.Services.AddMqttService();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply pending migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SyncroDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
