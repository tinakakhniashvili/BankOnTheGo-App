using BankOnTheGo.API.Setup;

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

var app = builder.Build();

app.UseForwardedHeadersConfig();
app.UseSwaggerDocs();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();