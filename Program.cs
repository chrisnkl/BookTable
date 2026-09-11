using BookTable.Clients;
using BookTable.Database;
using BookTable.Services;
using BookTable.Services.impl;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddHttpClient<NotificationClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5172");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(
                "http://localhost:5174",
                "http://127.0.0.1:5174"
                )
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddScoped<IBookService, BookService>();

builder.Services.AddSingleton<IStaticContentService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["AzureStorage:ConnectionString"] ?? "UseDevelopmentStorage=true";
    var containerName = config["AzureStorage:ContainerName"] ?? "static-content";
    return new StaticContentService(connectionString, containerName);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var staticContent = scope.ServiceProvider.GetRequiredService<IStaticContentService>();
        await staticContent.InitializeContainerAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not initialise Azure Storage container. Azurite may not be running.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
