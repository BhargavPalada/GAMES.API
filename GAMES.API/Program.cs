using First.API.Services;
using GAMES.CORE.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Bind MongoDB settings from appsettings.json
builder.Services.Configure<GamesDBSettings>(
    builder.Configuration.GetSection(nameof(GamesDBSettings)));
builder.Services.AddScoped<GameServices>();
// Register IMongoClient as a singleton
builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetValue<string>("GamesDBSettings:ConnectionString")));

// Register IGameServices with its implementation
builder.Services.AddScoped<IGamesServices, GameServices>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();