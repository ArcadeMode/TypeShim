WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly); ;

WebApplication app = builder.Build();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseWebAssemblyDebugging();

#pragma warning disable ASP0014 // Suggest using top level route registrations - not compatible with usespa and spaproxy is a bit of a hassle to use
app.UseEndpoints(ep => ep.MapControllers());
#pragma warning restore ASP0014 // Suggest using top level route registrations
app.UseSpa(spa =>
{
    spa.UseProxyToSpaDevelopmentServer("http://localhost:5173/");
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
