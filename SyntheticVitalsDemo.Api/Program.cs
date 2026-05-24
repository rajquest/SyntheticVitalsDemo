using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalUi", policy =>
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=synthetic_vitals_demo;User=synthetic;Password=synthetic;";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.Parse("8.0.36-mysql"),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<ClinicService>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<VitalsService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CsvExportService>();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddScoped<IVitalsGenerationService, VitalsGenerationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("LocalUi");
app.MapControllers();

if (app.Configuration.GetValue("DemoSeed:SeedOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DemoSeed");

    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Demo seed was skipped because the database is not available.");
    }
}

app.Run();
