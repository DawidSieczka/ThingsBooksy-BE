using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ThingsBooksy.Shared.Abstractions;
using ThingsBooksy.Shared.Infrastructure;
using ThingsBooksy.Shared.Infrastructure.Logging;
using ThingsBooksy.Shared.Infrastructure.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureModules().UseLogging();

var assemblies = ModuleLoader.LoadAssemblies(builder.Configuration, "ThingsBooksy.Modules.");
var modules = ModuleLoader.LoadModules(assemblies);

builder.Services.AddModularInfrastructure(builder.Configuration, assemblies, modules);

foreach (var module in modules)
{
    module.Register(builder.Services, builder.Configuration);
}

var app = builder.Build();
app.UseModularInfrastructure();

foreach (var module in modules)
{
    module.Use(app);
}

app.MapGet("/", (AppInfo appInfo) => appInfo).WithTags("API").WithName("Info");
app.MapGet("/ping", () => "pong").WithTags("API").WithName("Pong");
app.MapGet("/modules", (ModuleInfoProvider moduleInfoProvider) => moduleInfoProvider.Modules);

foreach (var module in modules)
{
    module.Expose(app);
}

assemblies.Clear();
modules.Clear();

app.Run();
