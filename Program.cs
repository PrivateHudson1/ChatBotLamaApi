using ChatBotLamaApi.Handlers;
using ChatBotLamaApi.Interfaces;
using ChatBotLamaApi.Services;
using Microsoft.AspNetCore.Authentication;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["ApiKey"] ?? "default-secret-key-123";
var corsHost = builder.Configuration["CorsHost"];
var redisHost= builder.Configuration["RedisHost"];
// Add services to the container.



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddHttpClient<ChatHub>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "ChatBotApp");
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsHost)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

builder.Services.AddSingleton<IRateLimiter, RedisRateLimiter>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisHost));



builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "CookieAuth";
    options.DefaultChallengeScheme = "CookieAuth";
    options.DefaultScheme = "CookieAuth";
})
.AddScheme<AuthenticationSchemeOptions, CookieAuthenticationHandler>(
    "CookieAuth",
    options =>
    {

    });


builder.Services.AddAuthorization();

var app = builder.Build();


app.UseMiddleware<UserIdMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();



app.MapHub<ChatHub>("/chatHub");

app.MapControllers();


app.Run();
