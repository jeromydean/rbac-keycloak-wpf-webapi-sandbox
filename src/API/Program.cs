
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API
{
  public class Program
  {
    public static void Main(string[] args)
    {
      WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      builder.Services.AddControllers();

      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      //add keycloak token validation
      builder.Services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        options.MetadataAddress = "https://localhost:8443/realms/rbac-application/.well-known/openid-configuration";
        options.Authority = "https://localhost:8443/realms/rbac-application";
        options.Audience = "account";
      });

      WebApplication app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();
      app.UseAuthorization();
      app.MapControllers();

      app.Run();
    }
  }
}