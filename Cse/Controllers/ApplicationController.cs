using System;
using System.Security;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Bphx.Cool;

using Gov.KansasDCF.Cse.App;

using static Bphx.Cool.Functions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace Gov.KansasDCF.Cse.Controllers;

public class ApplicationInfo
{
  public string ApplicationName { get; set; }

  public string Version { get; set; }
  public string EnvironmentName { get; set; }
  public string[] Connections { get; set; }
  public bool AllowTrace { get; set; }
  public bool AllowChangeDate { get; set; }
  public string Credits { get; set; }
  public string Copyright { get; set; }
}

/// <summary>
/// The application controller.
/// </summary>
[ApiController]
[Route("api")]
public class ApplicationController: ControllerBase
{
  /// <summary>
  /// Creates an application controller.
  /// </summary>
  /// <param name="logger"></param>
  public ApplicationController(ILogger<ApplicationController> logger)
  {
    this.logger = logger;
  }

  /// <summary>
  /// Gets application info.
  /// </summary>
  [HttpGet("about")]
  public ApplicationInfo About()
  {
    var services = HttpContext.RequestServices;
    var env = services.GetService<IWebHostEnvironment>();
    var appSettings = services.GetService<IOptions<AppSettings>>()?.Value;

    return new()
    {
      ApplicationName = env.ApplicationName,
      Version = GetType().Assembly.GetName().Version.ToString(),
      EnvironmentName = env.EnvironmentName,
      Connections = appSettings.Connections,
      AllowTrace = appSettings.AllowTrace,
      AllowChangeDate = string.Compare(env.EnvironmentName, "production", true) != 0,
      Credits = 
@"Praises to developers, including tool developers.

Kansas DCF:

Conduent:

Advanced:
  Adva Michaeli,
  Arthur Nesterovsky,
  Dileep Kanoor,
  John Regan,
  Mauro Fantini,
  Tanya Miller,
  Vladimir Nesterovsky",
      Copyright = "(c) 2022, dcf.ks.gov, oneadvanced.com, conduent.com"
    };
  }

  /// <summary>
  /// Gets current application state.
  /// </summary>
  /// <param name="index">A state index.</param>
  /// <returns>An application response.</returns>
  [HttpGet("current")]
  public Client.ApplicationResponse Current([FromQuery] int index) =>
    Client.Current(HttpContext.RequestServices, User, index);

  /// <summary>
  /// Finishes the application.
  /// </summary>
  /// <param name="index">A state index.</param>
  /// <returns>An application response.</returns>
  [HttpPost("logout")]
  public Client.ApplicationResponse Logout([FromQuery] int index) =>
    Client.Logout(HttpContext.RequestServices, User, index);

  /// <summary>
  /// Forks the state of the application.
  /// </summary>
  /// <param name="index">A state index.</param>
  /// <returns>An application response.</returns>
  [HttpPost("fork")]
  public Client.ApplicationResponse Fork([FromQuery] int index) =>
    Client.Fork(HttpContext.RequestServices, User, index);

  /// <summary>
  /// <para>
  /// Starts the application from the specified procedure step name or 
  /// transaction code with optional clear screen input parameters.
  /// </para>
  /// <para>
  /// <b>Note:</b> when procedure step name is defined then it will be 
  /// executed, otherwise the transaction code from command line arguments
  /// will be taken. When neither procedure nor command line arguments are 
  /// specified the exception will be thrown.
  /// </para>
  /// </summary>
  /// <param name="request">
  /// A <see cref="Client.StartRequest"/> instance that contains either
  /// a procedure step name to execute or a transaction code with optional
  /// clear screen input parameters and displayFirst flag.
  /// </param>
  /// <returns>An application response.</returns>
  [HttpPost("start")]
  public Client.ApplicationResponse Start(Client.StartRequest request)
  {
    if ((request != null) && 
      IsEmpty(request.CommandLine) && 
      IsEmpty(request.Procedure))
    {
      request.Procedure = "CoCsmmChildSupMainMenu";
    }

    return Client.Start(HttpContext.RequestServices, User, request);
  }

  /// <summary>
  /// Changes the current dialect.
  /// </summary>
  /// <param name="request">
  /// A <see cref="Client.ChangeDialectRequest"/> instance to change 
  /// the current dialect. The request should contain <b>currentDialect</b>
  /// property that will become a new value of the 
  /// <c>global.currentDialect</c>.
  /// </param>
  /// <returns>An application response.</returns>
  [HttpPost("changeDialect")]
  public Client.ApplicationResponse changeDialect(
    Client.ChangeDialectRequest request) =>
    Client.ChangeDialect(HttpContext.RequestServices, User, request);

  [Route("error")]
  public ErrorMessage error()
  {
    var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
    var error = context?.Error;

    if (error == null)
    {
      return null;
    }

    logger.LogError(error, "Application error");

    if ((error is ArgumentException) ||
      (error is ArgumentNullException) ||
      (error is NullReferenceException))
    {
      Response.StatusCode = StatusCodes.Status400BadRequest;
    }
    else if (error is NotSupportedException)
    {
      Response.StatusCode = StatusCodes.Status501NotImplemented;
    }
    else if (error is NotSupportedException)
    {
      Response.StatusCode = StatusCodes.Status501NotImplemented;
    }
    else if ((error is UnauthorizedAccessException) ||
      (error is SecurityException))
    {
      Response.StatusCode = StatusCodes.Status401Unauthorized;
    }
    else //if (error is )
    {
      Response.StatusCode = StatusCodes.Status500InternalServerError;
    }

    return new()
    {
      ExceptionType = error.GetType().FullName,
      ExceptionMessage = error.Message,
      StackTrace = error.ToString()
    };
  }

  private readonly ILogger logger;
}

