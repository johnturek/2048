using Game2048Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Let ASP.NET Core handle the default port binding
// In production environments like Fly.io, the PORT environment variable will be used
// through the configuration in Dockerfile and fly.toml

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve property names
        // Reference the DirectionJsonConverter from the Controllers namespace
        options.JsonSerializerOptions.Converters.Add(new Game2048Web.Controllers.DirectionJsonConverter()); 
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
