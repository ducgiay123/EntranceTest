using AuthenticationTest.Core.RepoAbstract;
using AuthenticationTest.Core.src.RepoAbstract;
using AuthenticationTest.Infrastructure.Database;
using AuthenticationTest.Infrastructure.Repo;
using AuthenticationTest.Service.src.Abstracts;
using AuthenticationTest.Service.src.Mapper;
using AuthenticationTest.Service.src.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Web", Version = "v1" });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
#pragma warning disable CS8604 // Possible null reference argument.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
    };
#pragma warning restore CS8604 // Possible null reference argument.
});
builder.Services.AddAuthorization(options =>
{
    // Policy requiring authenticated user
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());

    // Policy requiring NameIdentifier claim
    options.AddPolicy("HasUserId", policy =>
        policy.RequireClaim(ClaimTypes.NameIdentifier));
});

// Add controllers
builder.Services.AddControllers();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});
// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add DbContext
#pragma warning disable CS8602 // Dereference of a possibly null reference.
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
#pragma warning restore CS8602 // Dereference of a possibly null reference.

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Register services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAutoMapper(typeof(AutoMappingProfile));

// Add Tasks Service
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Add Tasks Service
builder.Services.AddScoped<IAdminService, AdminService>();
// builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Authentication API V1");
    c.RoutePrefix = "swagger"; // Swagger UI served at /swagger
    c.DocumentTitle = "The API Documentation";
    c.DocExpansion(DocExpansion.None);
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();
}
// Enable detailed error messages
app.UseDeveloperExceptionPage();
// Swagger middleware


// Middleware pipeline
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Root endpoint for testing
app.MapGet("/", () => "Server is running!");

app.Run();