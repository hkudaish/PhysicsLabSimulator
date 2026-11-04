using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.StaticFiles;
using VirtualStem.Client;
using VirtualStem.Science;
using VirtualStem.Browser;

var builder = WebApplication.CreateBuilder(args);
int port=builder.Configuration.GetValue<int?>("port")??5080;
if(port<1024||port>65535)throw new ArgumentException("Choose an unprivileged loopback port between 1024 and 65535.");
builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 4 * 1024 * 1024);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
});
var app = builder.Build();
Elements.Load(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Catalog/elements.json")));
app.Use(async (context,next) =>
{
    if (context.Request.Host.Host is not ("localhost" or "127.0.0.1" or "[::1]" or "::1"))
    { context.Response.StatusCode=403;return; }
    if (context.Request.Headers.TryGetValue("Origin",out var origin) && origin != $"{context.Request.Scheme}://{context.Request.Host}")
    { context.Response.StatusCode=403;return; }
    context.Response.Headers["X-Content-Type-Options"]="nosniff";
    context.Response.Headers["Referrer-Policy"]="no-referrer";
    context.Response.Headers["Content-Security-Policy"]="default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; connect-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self'";
    await next();
});
app.UseDefaultFiles();
var types=new FileExtensionContentTypeProvider();
types.Mappings[".hdr"]="application/octet-stream";
app.UseStaticFiles(new StaticFileOptions{ContentTypeProvider=types});
app.MapGet("/api/health",()=>new {status="ok",version="0.5.0-browser",chemistry=SolutionProperties.ModelVersion,electricity=PiecewiseDc.ModelVersion});
app.MapGet("/api/catalog",()=>new {tools=ToolCatalog.All,elements=Elements.All,reagents=Bench.Reagents});
app.MapGet("/api/workspace",()=>Bench.View(new WorkspaceData {ClientVersion="0.5.0-browser"}));
app.MapPost("/api/command",(Command request)=>
{
    try {return Results.Ok(Bench.Execute(request));}
    catch(Exception error) when(error is ArgumentException or InvalidDataException or JsonException or InvalidOperationException or KeyNotFoundException)
    { return Results.BadRequest(new {error=error.Message}); }
});
app.Run();
