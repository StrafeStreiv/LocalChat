using RealtimeHub.Hubs;
using RealtimeHub.Services;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Добавляем логгирование
builder.Services.AddLogging();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add RabbitMQ Consumer Service
builder.Services.AddHostedService<RabbitMqConsumerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ПРАВИЛЬНЫЙ ПОРЯДОК MIDDLEWARE:
app.UseRouting();
app.UseAuthorization();

// Метрики ПОСЛЕ UseRouting
app.UseHttpMetrics();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

// Health endpoint - БЕЗ ПАРАМЕТРОВ!
app.MapGet("/", () => "RealtimeHub is running!");
app.MapGet("/health", () => new
{
    status = "OK",
    service = "RealtimeHub",
    time = DateTime.UtcNow,
    endpoints = new[] { "/chatHub", "/api/health", "/metrics" }
});

// MapMetrics должен быть ПОСЛЕ всех Map методов
app.MapMetrics();

app.Run();