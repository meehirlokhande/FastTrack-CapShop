using CapShop.NotificationService.Services;
using CapShop.NotificationService.Workers;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

var rmq = builder.Configuration.GetSection("RabbitMq");
builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName = rmq["Host"] ?? "localhost",
        Port = int.Parse(rmq["Port"] ?? "5672"),
        UserName = rmq["Username"] ?? "guest",
        Password = rmq["Password"] ?? "guest"
    };

    const int maxRetries = 10;
    for (int attempt = 0; attempt < maxRetries; attempt++)
    {
        try
        {
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        }
        catch (Exception) when (attempt < maxRetries - 1)
        {
            Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }
    }

    throw new InvalidOperationException("Failed to connect to RabbitMQ after retries.");
});

builder.Services.AddHttpClient<IAuthHttpClient, AuthHttpClient>(client =>
{
    var authUrl = builder.Configuration["Services:AuthService"]
        ?? throw new InvalidOperationException("Services:AuthService is missing");
    client.BaseAddress = new Uri(authUrl);
});

builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddHostedService<PaymentNotificationWorker>();
builder.Services.AddHostedService<OrderNotificationWorker>();

var host = builder.Build();
host.Run();
