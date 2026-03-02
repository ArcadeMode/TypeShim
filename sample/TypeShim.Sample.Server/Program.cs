using TypeShim.Sample;
using TypeShim.Sample.Server.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(PeopleController).Assembly); ;
var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
//app.UseBlazorFrameworkFiles();
app.UseWebAssemblyDebugging();
app.UseSpa(spa =>
{
    spa.UseProxyToSpaDevelopmentServer("http://localhost:5173/");
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
