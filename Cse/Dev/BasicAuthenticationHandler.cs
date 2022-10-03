using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gov.KansasDCF.Cse.App
{
  public class BasicAuthenticationHandler: 
    AuthenticationHandler<AuthenticationSchemeOptions>
  {
    public BasicAuthenticationHandler(
      IOptionsMonitor<AuthenticationSchemeOptions> options,
      ILoggerFactory logger,
      UrlEncoder encoder,
      ISystemClock clock,
      BasicUsers users): 
      base(options, logger, encoder, clock)
    {
      this.users = users;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
      // skip authentication if endpoint has [AllowAnonymous] attribute
      var endpoint = Context.GetEndpoint();

      if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
      {
        return Task.FromResult(AuthenticateResult.NoResult());
      }

      if (!Request.Headers.ContainsKey("Authorization"))
      {
        return Task.FromResult(
          AuthenticateResult.Fail("Missing Authorization Header"));
      }
      
      try
      {
        var authHeader = 
          AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
        var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
        var credentials = 
          Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
        var username = credentials[0];
        var password = credentials[1];

        if (!users.Users.TryGetValue(username, out var user) || 
          ((user.Password ?? "") != (password ?? "")))
        {
          return Task.FromResult(
            AuthenticateResult.Fail("Invalid Username or Password"));
        }

        var claims = new[] 
        {
          new Claim(ClaimTypes.NameIdentifier, user.UserId ?? user.UserName),
          new Claim(ClaimTypes.Name, user.UserName)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
      }
      catch
      {
        return Task.FromResult(
          AuthenticateResult.Fail("Invalid Authorization Header"));
      }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
      Response.Headers["WWW-Authenticate"] = "Basic realm=\"\", charset=\"UTF-8\"";

      return base.HandleChallengeAsync(properties);
    }

    private readonly BasicUsers users;
  }
}