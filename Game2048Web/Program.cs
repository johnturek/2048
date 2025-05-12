using Game2048Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on port 8080 for Fly.io
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080);
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve property names
        options.JsonSerializerOptions.Converters.Add(new Game2048Web.Controllers.DirectionJsonConverter()); // Add custom converter for Direction enum
    });

// Add game service
builder.Services.AddSingleton<GameService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Important: Use CORS before routing
app.UseCors("AllowAll");

// Static files should be served before routing
app.UseStaticFiles();

// Set up the routing pipeline
app.UseRouting();

// Authentication and authorization should be after routing
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints using top-level route registrations
app.MapControllers();
app.MapRazorPages();

// Remove these lines as they're not standard ASP.NET Core methods
// app.MapStaticAssets();
// app.MapRazorPages()
//    .WithStaticAssets();

app.Run();
