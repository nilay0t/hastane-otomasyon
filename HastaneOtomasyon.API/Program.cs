using HastaneOtomasyon.API.Endpoints;
using HastaneOtomasyon.API.Middlewares;
using HastaneOtomasyon.Data.Context;
using HastaneOtomasyon.Data.SeedData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedAsync(context);
}

app.MapGet("/", () => "Hastane Otomasyon API");

app.MapAuthEndpoints();
app.MapPoliklinikEndpoints();
app.MapDoktorEndpoints();
app.MapHastaEndpoints();
app.MapRandevuEndpoints();
app.MapAdminEndpoints();

app.Run();
