using Server;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

builder.Services.AddSingleton<InvertedIndex>();
builder.Services.AddControllers();

builder.Services.Configure<ServerOptions>(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapControllers();

app.Services.GetRequiredService<InvertedIndex>().Start();

app.Run();