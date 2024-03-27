using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace self_service_core.Handler;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
   private readonly IConfiguration _configuration;
   public BasicAuthenticationHandler(IConfiguration configuration, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
   {
         _configuration = configuration;
   }

   protected override Task<AuthenticateResult> HandleAuthenticateAsync()
   {
       if (!Request.Headers.ContainsKey("Authorization"))
       {
           return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
       }

       var authenticationHeaderValue = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
       if (authenticationHeaderValue.Parameter != null)
       {
           var bytes = Convert.FromBase64String(authenticationHeaderValue.Parameter);
           string[] credentials = Encoding.UTF8.GetString(bytes).Split(":");
           string username = credentials[0];
           string password = credentials[1];

           if ((username != _configuration.GetSection("Company").GetSection("Username").Value ||
                password != _configuration.GetSection("Company").GetSection("Password").Value) &&  (username != _configuration.GetSection("User").GetSection("Username").Value || password != _configuration.GetSection("User").GetSection("Password").Value))
           {
               return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
           }
       
           var role = username == _configuration.GetSection("Company").GetSection("Username").Value ? "Admin" : "User";

           var claims = new[]
           {
               new Claim(ClaimTypes.NameIdentifier, username),
               new Claim(ClaimTypes.Name, username),
               new Claim(ClaimTypes.Role, role)
           };

           var identity = new ClaimsIdentity(claims, Scheme.Name);
           var principal = new ClaimsPrincipal(identity);
           var ticket = new AuthenticationTicket(principal, Scheme.Name);

           return Task.FromResult(AuthenticateResult.Success(ticket));
       }else
         {
              return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
         }
   }
}