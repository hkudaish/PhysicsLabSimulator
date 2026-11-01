var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
// Phase 0 scaffold: no auth, tenant APIs, or student data routes are implemented.
if (app.Environment.IsDevelopment())
{
    app.MapGet("/health", () => Results.Ok(new { status = "scaffold", phase = 0 }));
}
app.Run();
