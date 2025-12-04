using BackEnd.Domain.Contracts;
using BackEnd.Domain.Entities;
using BackEnd.Infrastructure.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BackEnd.Extensions
{
    public static partial class ApplicationService
    {
        // Allow any origin, method, and header
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
            {
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireUppercase = false;
                o.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        }

        public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))

            {
                throw new InvalidOperationException("JWT secret key is not configured.");
            }

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidAudience = jwtSettings.ValidAudience,
                    IssuerSigningKey = secretKey,
                };
                o.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

                        var jti = context.SecurityToken.Id;
                        var isExist = await dbContext.BlacklistTokens.AnyAsync(t => t.Jti == jti);
                        if (isExist)
                        {
                            context.Fail("Token has been revoked or blacklisted.");
                        }
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var errorResponse = new ErrorResponse
                        {
                            StatusCode = context.Response.StatusCode,
                            Title = "Unauthorized",
                            Message = "You are not authorized to access this resource. Please authenticate."
                        };
                        return context.Response.WriteAsJsonAsync(errorResponse);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 402; 
                        context.Response.ContentType = "application/json";
                        var errorResponse = new ErrorResponse
                        {
                            StatusCode = context.Response.StatusCode,
                            Title = "Forbidden",
                            Message = "You do not have permission to access this resource."
                        };
                        return context.Response.WriteAsJsonAsync(errorResponse);
                    }
                };
            });
        }
    }
}
