using GameStore.Api.Data;
using GameStore.Api.Features.Games;
using GameStore.Api.Features.Genres;
using GameStore.Api.Shared.ErrorHandling;
using GameStore.Api.Shared.FileUpload;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails()
                .AddExceptionHandler<GlobalExceptionHandler>();

var connString = builder.Configuration.GetConnectionString("GameStore");
builder.Services.AddSqlite<GameStoreContext>(connString);

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod |
                            HttpLoggingFields.RequestPath |
                            HttpLoggingFields.ResponseStatusCode |
                            HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor()
                .AddSingleton<FileUploader>();

var app = builder.Build();

app.UseStaticFiles();

app.MapGames();
app.MapGenres();

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();

await app.InitializeDbAsync();

app.Run();