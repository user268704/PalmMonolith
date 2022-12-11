using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Palm;
using Palm.Abstractions.Interfaces.Caching;
using Palm.Abstractions.Interfaces.Managers;
using Palm.Abstractions.Interfaces.Repositories;
using Palm.Caching;
using Palm.Infrastructure;
using Palm.Infrastructure.Repos;
using Palm.Mapper.Mapping;
using Palm.Models.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMvc();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(MappingProfiler).Assembly);

builder.Services.AddScoped<ISessionСaching, SessionCaching>();
builder.Services.AddScoped<IQuestionsCaching, QuestionsCaching>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ISessionManager, SessionManager>();

builder.Services.AddSignalR(); 
builder.Services.AddDistributedMemoryCache();

builder.Services.AddIdentity<User, IdentityRole>(options => { options.User.RequireUniqueEmail = true; })
    .AddEntityFrameworkStores<UserDataContext>()
    .AddRoles<IdentityRole>();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("teacher",
        policy => policy.RequireRole("teacher"));
    options.AddPolicy("student",
        policy => policy.RequireRole("student"));
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/denied";
    });

builder.Services.AddDbContext<UserDataContext>(options =>
{
    options.UseNpgsql("Host=localhost;Port=5432;Database=Palm.Users;Username=postgres;Password=str33tf1ght3r",
        b => b.MigrationsAssembly("Palm"));
}, ServiceLifetime.Transient);

builder.Services.AddDbContext<SessionDataContext>(options =>
{
    options.UseNpgsql("Host=localhost;Port=5432;Database=Palm.Users;Username=postgres;Password=str33tf1ght3r",
        b => b.MigrationsAssembly("Palm"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<SessionHub>("api/signalr/session");

app.MapControllerRoute(
    "default",
    "{controller}/{action=Index}/{id?}");

app.Run();