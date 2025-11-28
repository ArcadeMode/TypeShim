using TypeShim.Sample;
using TypeShim.Sample.Server.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddApplicationPart(typeof(PeopleController).Assembly); ;
builder.Services.AddSingleton<PersonRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.Use((context, next) =>
{
    //context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
    //context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
    //context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204;
        return Task.CompletedTask;
    }
    return next();
});
//app.UseAuthorization();
app.UseBlazorFrameworkFiles(); // Serves _framework and related boot files
app.UseWebAssemblyDebugging();

app.UseDefaultFiles();      // Looks for index.html, index.htm, etc. in wwwroot
app.UseStaticFiles();       // Serves files from wwwroot

app.MapControllers();


//app.MapFallbackToFile("index.html");

app.Run();
