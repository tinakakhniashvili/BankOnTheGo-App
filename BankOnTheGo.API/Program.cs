using BankOnTheGo.API.Setup;
using BankOnTheGo.API.Startup;              
using BankOnTheGo.API.Startup.Seed;        

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services
    .AddAppDb(config)
    .AddAppIdentity(config)
    .AddJwtAuth(config)
    .AddEmailing(config)
    .AddAppServices()
    .AddSwaggerDocs();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

builder.Services.AddTransient<SeedData>();

var app = builder.Build();

app.UseForwardedHeadersConfig();
app.UseSwaggerDocs();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.UseMigrationsAndSeedAsync();

app.Run();