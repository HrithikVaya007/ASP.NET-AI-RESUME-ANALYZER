using AIResumeAnalyzer.MVC.Config;
using AIResumeAnalyzer.MVC.Middleware;
using AIResumeAnalyzer.MVC.Repositories;
using AIResumeAnalyzer.MVC.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Restored to serve the Frontend MVC UI
builder.Services.AddControllers(); // API Controllers

// Configure Application Settings
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<LlmSettings>(builder.Configuration.GetSection("LlmSettings"));
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));

// Configure Database Services
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ResumeRepository>();
builder.Services.AddScoped<InterviewSessionRepository>();
builder.Services.AddScoped<ResumeService>();
builder.Services.AddScoped<InterviewService>();

// Configure Core Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<SmsService>();
builder.Services.AddHttpClient<LlmService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings != null)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };

            // Read JWT from the "AuthToken" cookie so server-rendered pages can authenticate
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Cookies["AuthToken"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                },
                // When authentication fails (no token / expired), redirect to login page
                OnChallenge = context =>
                {
                    // Only redirect for non-API requests (MVC page navigations)
                    if (!context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/login");
                    }
                    return Task.CompletedTask;
                }
            };
        });
    builder.Services.AddAuthorization();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"); // This connects your existing Frontend UI

app.MapControllers(); // Direct mapping for the /api/... Backend

app.Run();
