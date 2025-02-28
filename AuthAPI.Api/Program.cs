using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using AuthAPI.Application.CQRS.Commands.RefreshToken;
using AuthAPI.Application.CQRS.Commands.Role;
using AuthAPI.Application.CQRS.Commands.User;
using AuthAPI.Application.CQRS.Commands.User.CreateUser;
using AuthAPI.Application.CQRS.Commands.UserAuditLogs;
using AuthAPI.Application.CQRS.Queries.RefreshToken;
using AuthAPI.Application.CQRS.Queries.Role;
using AuthAPI.Application.CQRS.Queries.User;
using AuthAPI.Application.CQRS.Queries.UserAuditLogs;
using AuthAPI.Application.Interface;
using AuthAPI.Application.Middleware;
using AuthAPI.Application.Services;
using AuthAPI.Application.Services.Authentication;
using AuthAPI.Application.Services.Messaging;
using AuthAPI.Application.Services.Role;
using AuthAPI.Application.Services.User;
using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using AuthAPI.DAL.Repository;
using AuthAPI.Domain.Enums;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddLogging();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Authorization API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
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
});

// Configure JWT authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
        };
    });

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    // Политики для каждой роли
    options.AddPolicy($"MinimumRole{UserRole.User}", policy => 
        policy.RequireClaim(ClaimTypes.Role, 
            UserRole.User.ToString(), 
            UserRole.Moderator.ToString(), 
            UserRole.Admin.ToString(), 
            UserRole.SuperAdmin.ToString()));

    options.AddPolicy($"MinimumRole{UserRole.Moderator}", policy => 
        policy.RequireClaim(ClaimTypes.Role, 
            UserRole.Moderator.ToString(), 
            UserRole.Admin.ToString(), 
            UserRole.SuperAdmin.ToString()));

    options.AddPolicy($"MinimumRole{UserRole.Admin}", policy => 
        policy.RequireClaim(ClaimTypes.Role, 
            UserRole.Admin.ToString(), 
            UserRole.SuperAdmin.ToString()));

    options.AddPolicy($"MinimumRole{UserRole.SuperAdmin}", policy => 
        policy.RequireClaim(ClaimTypes.Role, 
            UserRole.SuperAdmin.ToString()));
});

// Register services
// Add DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// RabbitMQ Services
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

// Other services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<RoleManagementService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ILoginActivityService, LoginActivityService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<CreateRefreshTokenHandler>();
builder.Services.AddScoped<GetByIdAsyncHandler>();
builder.Services.AddScoped<DeleteRefreshTokenHandler>();
builder.Services.AddScoped<RevokeTokensHandler>();
builder.Services.AddScoped<AddUserHandler>();
builder.Services.AddScoped<UpdateRoleUserHandler>();
builder.Services.AddScoped<GetRefreshTokenHandler>();
builder.Services.AddScoped<GetStatisticsByRoleHandler>();
builder.Services.AddScoped<CountAsyncHandler>();
builder.Services.AddScoped<ExistingUserHandler>();
builder.Services.AddScoped<GetByIdAsyncHandler>();
builder.Services.AddScoped<GetUserByEmailHandler>();
builder.Services.AddScoped<GetUsersByRoleAsyncHandler>();
builder.Services.AddScoped<GetAuditLogsByUser>();
builder.Services.AddScoped<CreateAuditLog>();
builder.Services.AddScoped<GetUserRoleStatisticsHandler>();


// Добавляем MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

// Либо, если обработчики в другой сборке:
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetByIdAsyncHandler).Assembly));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        corsPolicyBuilder =>
        {
            corsPolicyBuilder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ValidationMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();