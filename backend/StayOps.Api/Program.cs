using Microsoft.EntityFrameworkCore;
using StayOps.Application.Abstractions;
using StayOps.Application.Identity.Abstractions;
using StayOps.Application.Identity.Services;
using StayOps.Infrastructure.Data;
using StayOps.Infrastructure.Identity;
using StayOps.Infrastructure.Identity.Repositories;
using StayOps.Infrastructure.Security;
using StayOps.Infrastructure.Time;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.WithOrigins("http://localhost:5101")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
builder.Services.AddOpenApi();

builder.Services.AddDbContext<StayOpsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IRoleRepository, EfRoleRepository>();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddSingleton<ITokenService, SimpleTokenService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("DevCors");

app.UseAuthorization();

app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StayOpsDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await IdentitySeeder.SeedAsync(dbContext);
}

await app.RunAsync();
