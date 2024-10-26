using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WAYMORR_MS_Product;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Parse("::ffff:172.17.0.1")); // IP прокси/контейнера
});

// Подключает БД
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Подключаем Redis
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

var app = builder.Build();

app.UseCors(policyBuilder => policyBuilder
    .WithOrigins(
        "http://localhost:5173", 
        "http://localhost:5174", 
        "http://localhost:5175", 
        "https://chimp-hot-racer.ngrok-free.app", 
        "https://careful-quick-swift.ngrok-free.app"
    )
    .AllowAnyHeader()
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .AllowCredentials()
);

app.MapGet("/", () => "Hello World!");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();