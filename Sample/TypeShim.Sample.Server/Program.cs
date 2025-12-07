using TypeShim.Sample;
using TypeShim.Sample.Server.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(PeopleController).Assembly); ;
builder.Services.AddSingleton<PersonRepository>();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseBlazorFrameworkFiles();
app.UseWebAssemblyDebugging();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
