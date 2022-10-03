using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Data.SqlClient;
using System.Data.Common;
using System.Security.Principal;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Microsoft.Identity.Web;

using Bphx.Cool;
using Bphx.Cool.Data;
using Bphx.Cool.Impl;
using Bphx.Cool.Cobol;
using Bphx.Cool.Log;

namespace Gov.KansasDCF.Cse.App;

public class Startup
{
  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;
  }

  public IConfiguration Configuration { get; }

  // This method gets called by the runtime. Use this method to add services to the container.
  public void ConfigureServices(IServiceCollection services)
  {
    if (Configuration.GetValue("Debug:Authentication", false))
    {
      services.
        Configure<List<BasicUser>>(Configuration.GetSection("Debug:Users")).
        AddSingleton<BasicUsers>().
        AddAuthentication("BasicAuthentication").
        AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
          "BasicAuthentication",
          null);
    }
    else
    {
      services.AddMicrosoftIdentityWebAppAuthentication(Configuration);
    }

    services.AddAuthorization(options =>
    {
      options.FallbackPolicy = new AuthorizationPolicyBuilder().
        RequireAuthenticatedUser().
        Build();
    });

    services.
      AddSession().
      AddResponseCompression().
      AddHttpContextAccessor().
      Configure<AppSettings>(Configuration.GetSection("Settings")).
      AddSingleton<ISerializer, Serializer>().
      AddTransient<ISessions, State>().
      AddSingleton<IEnvironment>(services => new Bphx.Cool.Impl.Environment()
      {
        ServiceProvider = services,
        Mode = services.GetService<IWebHostEnvironment>().EnvironmentName,
        LockManager = new LockManager(),
        UserIdProvider = GetUserId,
        Serializer = services.GetService<ISerializer>(),
        Resources = new CompositeResources(),
        DataConverter = new SqlDataConverter(),
        ErrorCodeResolver = new SqlErrorCodeResolver(),
        SessionFactory = CreateSession,
        TransactionFactory = CreateTransaction,
        Clock = GetClock,
        Collator = new EbcdicCollator(),
        Encoding = Encoding.Latin1
      }).
      AddSingleton<IEabStub>(new EabStub
      {
        Encoding = Encoding.Latin1,
        EabContextFactory = context => new EabContext { Context = context }
      });

    DbProviderFactories.RegisterFactory(
      "System.Data.SqlClient", 
      SqlClientFactory.Instance);

    services.AddControllersWithViews().
      AddJsonOptions(options =>
      {
        var jsonOptions = options.JsonSerializerOptions;

        jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        jsonOptions.AllowTrailingCommas = true;
        jsonOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        jsonOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString | 
          JsonNumberHandling.AllowReadingFromString;
        jsonOptions.Converters.Add(
          new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        jsonOptions.Converters.Add(new DateTimeConverter());
      });

    // In production, the Angular files will be served from this directory
    services.AddSpaStaticFiles(configuration =>
    {
      configuration.RootPath = "ClientApp/dist/app-client";
    });
  }

  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    app.UseExceptionHandler("/api/error");
    app.UseHttpsRedirection();
    app.UseSession();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseStaticFiles();

    if(!env.IsDevelopment())
    {
      app.UseSpaStaticFiles();
    }

    app.UseEndpoints(endpoints =>
    {
      endpoints.
        MapControllerRoute(
          name: "default",
          pattern: "{controller}/{action=Index}/{id?}").
        RequireAuthorization();
    });

    app.UseSpa(spa =>
    {
      // To learn more about options for serving an Angular SPA from ASP.NET Core,
      // see https://go.microsoft.com/fwlink/?linkid=864501

      spa.Options.SourcePath = "ClientApp";

      if(env.IsDevelopment())
      {
        //spa.UseAngularCliServer(npmScript: "start");
        spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
      }
    });
  }

  private ISessionManager CreateSession(
    IServiceProvider serviceProvider,
    Dictionary<string, object> options,
    ISessionManager original)
  {
    var environment = serviceProvider.GetService<IEnvironment>();

    if (original != null)
    {
      return original.Copy(environment);
    }

    if (options != null)
    {
      return new SessionManager(options);
    }

    options = new();

    var httpContext =
      serviceProvider?.GetService<IHttpContextAccessor>()?.HttpContext;
    var user = httpContext.User;
    var request = httpContext.Request;

    var canSelectMode = true;
    var canSetDate = true;
    var canImpersonate = true;

    if(canSelectMode)
    {
      string mode = request.Query["mode"];

      if(!string.IsNullOrWhiteSpace(mode))
      {
        options["app:mode"] = mode;
      }
    }

    if (canSetDate)
    {
      string currentDate = request.Query["currentDate"];

      if(!string.IsNullOrWhiteSpace(currentDate))
      {
        options["app:dateDiff"] = DateTime.Today -
          DateTime.ParseExact(currentDate, "yyyy-MM-dd", null);
      }
    }

    if (canImpersonate)
    {
      string userId = request.Query["userId"];

      if(!string.IsNullOrWhiteSpace(userId))
      {
        options["app:userId"] = userId;
      }
    }

    return new SessionManager(options)
    {
      Profiler = CreateProfiler(serviceProvider, httpContext, environment)
    };
  }

  private string GetUserId(IDialogManager dialog, IPrincipal principal) =>
    dialog.SessionManager.Options.TryGetValue("app:userId", out var value) ?
      (string)value : principal?.Identity?.Name?.ToUpper();

  private DateTime GetClock(ISessionManager session)
  {
    var now = DateTime.Now;
    return now.AddTicks(-(now.Ticks % (TimeSpan.TicksPerMillisecond / 1000))) -
      (session.Options.TryGetValue("app:dateDiff", out var value) ?
      (TimeSpan)value : TimeSpan.Zero);
  }

  private ITransaction CreateTransaction(IContext context)
  {
    var appSettings = context.GetService<IOptions<AppSettings>>()?.Value;
    var mode = context.Dialog.SessionManager.Options.
      TryGetValue("app:mode", out var value) ? (string)value : "Default";

    var connectionString = (appSettings?.Connections.Contains(mode) != true ? 
      null : Configuration.GetConnectionString(mode)) ??
      throw new InvalidOperationException($"Invalid mode {mode}.");

    return new DatabaseTransaction(
      SqlClientFactory.Instance,
      connectionString,
      IsolationLevel.ReadCommitted);
  }

  private IProfiler CreateProfiler(
    IServiceProvider services,
    HttpContext httpContext,
    IEnvironment environment)
  {
    var appSettings = services.GetService<IOptions<AppSettings>>()?.Value;

    if(!(appSettings?.AllowTrace == true))
    {
      return null;
    }

    var connectionString = Configuration.GetConnectionString("trace");

    if (string.IsNullOrEmpty(connectionString))
    {
      return null;
    }

    string traceId = httpContext.Request.Query["traceId"];

    if (string.IsNullOrEmpty(traceId))
    {
      return null;
    }

    IDbConnection factory()
    {
      var connection = SqlClientFactory.Instance.CreateConnection();

      connection.ConnectionString = connectionString;

      return connection;
    }

    var exists = DbLogger.HasTrace(factory, traceId);
    var serializer = environment.Serializer;

    if (httpContext.Request.Query.ContainsKey("newTrace"))
    {
      if (exists)
      {
        throw new InvalidOperationException(
          $"Trace alread exists for: {traceId}");
      }

      return new DbLogger(factory, traceId, serializer);
    }
    else
    {
      if (!exists)
      {
        throw new InvalidOperationException(
          $"No trace found for: {traceId}");
      }

      string bypass = httpContext.Request.Query["bypass"];

      return null;
      //return new PlaybackLogger(serializer, path, bypass, true);
    }
  }
}
