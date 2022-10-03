using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;

using Bphx.Cool;
using Bphx.Cool.Cobol;
using Bphx.Cool.Data;
using Bphx.Cool.Impl;
using Bphx.Cool.Log;

using Gov.KansasDCF.Cse.App;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using static Bphx.Cool.Functions;

namespace KansasDCF.Cse.Batch;

public class Program
{
  public static void Main(string[] args)
  {
    var host = CreateHostBuilder(args).Build();

    host.Start();

    try
    {
      Run(host);
    }
    catch(Exception e)
    {
      Console.WriteLine(e);

      throw;
    }
    finally
    {
      host.StopAsync().Wait();
    }
  }

  private static IHostBuilder CreateHostBuilder(string[] args = null) => Host.
    CreateDefaultBuilder(args).
    ConfigureAppConfiguration((context, builder) =>
    {
      var environment = context.HostingEnvironment;

      if (environment.IsDevelopment())
      {
        builder.AddUserSecrets<Program>();
      }

      //var config = builder.Build();
      //var appConfigConnectionString = config.GetValue<string>("AppConfigConnectionString");

      //builder.Sources.Clear();
      //builder.AddConfiguration(config);
      //builder.AddAzureAppConfiguration(appConfigConnectionString);

      builder.
        AddEnvironmentVariables("DCF_BATCH_").
        AddJsonFile(
          Path.Combine(environment.ContentRootPath, "appsettings.json"),
          optional: true).
        AddJsonFile(
          Path.Combine(
            environment.ContentRootPath, 
            $"appsettings.{environment.EnvironmentName}.json"),
          optional: true);

      if (args != null)
      {
        builder.AddCommandLine(args);
      }
    }
    ).
    ConfigureServices((context, services) =>
    {
      var configuration = context.Configuration;

      if (configuration.GetValue<bool>("debug"))
      {
        System.Diagnostics.Debugger.Launch();
      }

      services.
        AddSingleton<ISerializer, Serializer>().
        AddSingleton<IEnvironment>(services => new Bphx.Cool.Impl.Environment()
        {
          ServiceProvider = services,
          UserIdProvider = (dialog, principal) => principal?.Identity?.Name?.ToUpper(),
          LockManager = new LockManager(),
          Serializer = services.GetService<ISerializer>(),
          Resources = new CompositeResources(),
          DataConverter = new SqlDataConverter(),
          ErrorCodeResolver = new SqlErrorCodeResolver(),
          SessionFactory = CreateSession,
          TransactionFactory = CreateTransaction,
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
    });

  private static ISessionManager CreateSession(
    IServiceProvider serviceProvider,
    Dictionary<string, object> options,
    ISessionManager original)
  {
    var environment = serviceProvider.GetService<IEnvironment>();

    return original?.Copy(environment) ??
      new SessionManager(options)
      {
        Profiler = CreateProfiler(serviceProvider, environment)
      };
  }

  private static ITransaction CreateTransaction(IContext context)
  {
    var configuration = context.GetService<IConfiguration>();
    var connection = configuration["connection"] ?? "Default";

    var connectionString = configuration.GetConnectionString(connection) ??
      configuration.GetConnectionString(connection + ":connectionString") ??
      throw new InvalidOperationException("Invalid connection.");

    return new DatabaseTransaction(
      SqlClientFactory.Instance,
      connectionString,
      IsolationLevel.ReadCommitted);
  }

  private static IProfiler CreateProfiler(
    IServiceProvider services,
    IEnvironment environment)
  {
    var configuration = services.GetService<IConfiguration>();
    var traceId = configuration.GetValue("traceId", "");

    if (string.IsNullOrWhiteSpace(traceId))
    {
      return null;
    }

    var connectionString = configuration.GetConnectionString("trace");

    if (string.IsNullOrEmpty(connectionString))
    {
      return null;
    }

    IDbConnection factory()
    {
      var connection = SqlClientFactory.Instance.CreateConnection();

      connection.ConnectionString = connectionString;

      return connection;
    }

    var serializer = environment.Serializer;

    var newTrace = configuration.GetValue("newTrace", false);

    if (newTrace)
    {
      var timestampedTraceId =
        traceId + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");

      if (DbLogger.HasTrace(factory, timestampedTraceId))
      {
        throw new InvalidOperationException(
          $"Trace alread exists for: {traceId}");
      }

      return new DbLogger(factory, timestampedTraceId, serializer);
    }
    else
    {
      if (!DbLogger.HasTrace(factory, traceId))
      {
        throw new InvalidOperationException(
          $"No trace found for: {traceId}");
      }

      string bypass = configuration.GetValue("traceBypass", "");

      return null;
      //return new PlaybackLogger(serializer, path, bypass, true);
    }
  }

  private static void Run(IHost host)
  {
    var services = host.Services;
    var configuration = services.GetService<IConfiguration>();
    var environment = host.Services.GetService<IEnvironment>();
    using var session = environment.CreateSession(services);
    var dialog = environment.CreateDialog(services, session, null);

    var trancode = configuration["trancode"];
    var arguments = configuration["arguments"];
    var procedure = 
      environment.Resources.GetProcedureByTransactionCode(trancode, 0);

    dialog.Run(procedure, 0, ParseCommandLine(arguments), null);
  }
}
