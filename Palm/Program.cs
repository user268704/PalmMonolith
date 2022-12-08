using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Palm;
using Palm.Cash;
using Palm.Infrastructure;
using Palm.Models.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// builder.Services.AddControllers();
// builder.Services.AddControllersWithViews();
builder.Services.AddMvc();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(Palm.Mapper.Mapping.MappingProfiler).Assembly);

builder.Services.AddScoped<ISessionСaching, SessionCaching>();
builder.Services.AddScoped<IQuestionsCaching, QuestionsCaching>();
builder.Services.AddScoped<SessionManager>();

builder.Services.AddSignalR();
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        // TODO: Добавить уникальность никнейма пользователя
        // options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<UserDataContext>()
    .AddRoles<IdentityRole>();

builder.Services.AddSwaggerGen(options =>
{
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile); 
    
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
    options.UseNpgsql("Host=localhost;Port=5432;Database=Palm.Users;Username=postgres;Password=str33tf1ght3r", b => b.MigrationsAssembly("Palm"));
}, ServiceLifetime.Transient);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();

app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(enpoints =>
{
    enpoints.MapHub<SessionHub>("/api/signalr/session");
});

app.UseStaticFiles();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.Run();