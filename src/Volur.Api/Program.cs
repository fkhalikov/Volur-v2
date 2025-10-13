using Serilog;
using Volur.Application;
using Volur.Infrastructure;
using Volur.Infrastructure.Persistence;
using Volur.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Volur API", Version = "v1", Description = "Financial market data API" });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongodbConnectionString: builder.Configuration["Mongo:ConnectionString"] ?? "mongodb://localhost:27017",
        name: "mongodb",
        timeout: TimeSpan.FromSeconds(3));

// Application layers
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Ensure MongoDB indexes on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        await mongoContext.EnsureIndexesAsync();
        Log.Information("MongoDB indexes initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to initialize MongoDB indexes");
        throw;
    }
}

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

Log.Information("Starting Volur API...");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }

