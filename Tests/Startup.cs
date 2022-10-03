using System.IO;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Bphx.Cool;
using Bphx.Cool.Data;
using Bphx.Cool.Impl;
using Bphx.Cool.Cobol;

using Gov.KansasDCF.Cse.App;

namespace Gov.KansasDCF.Cse.Test;

public static class Startup
{
  public static IHostBuilder CreateHostBuilder(string[] args = null) => Host.
    CreateDefaultBuilder(args).
    ConfigureAppConfiguration((context, builder) =>
    {
      var environment = context.HostingEnvironment;

      if (environment.IsDevelopment())
      {
        builder.AddUserSecrets<TestAction>();
      }

      //var config = builder.Build();
      //var appConfigConnectionString = config.GetValue<string>("AppConfigConnectionString");

      //builder.Sources.Clear();
      //builder.AddConfiguration(config);
      //builder.AddAzureAppConfiguration(appConfigConnectionString);

      builder.
        AddEnvironmentVariables().
        AddJsonFile(
          Path.Combine(environment.ContentRootPath, "appsettings.json")).
        AddJsonFile(
          Path.Combine(
            environment.ContentRootPath,
            $"appsettings.{environment.EnvironmentName}.json"));

      if (args != null)
      {
        builder.AddCommandLine(args);
      }
    }).
    ConfigureServices((context, services) =>
    {
      var configuration = context.Configuration;

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
          TransactionFactory = context => new DatabaseTransaction(
            SqlClientFactory.Instance,
            configuration.GetConnectionString("Default"),
            IsolationLevel.ReadCommitted),
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

  public static void Run(
    this IHost host, 
    System.Action<IContext> action, 
    Global global,
    bool rollback)
  {
    var environment = host.Services.GetService<IEnvironment>();
    using var session = environment.CreateSession(host.Services);
    var dialog = environment.CreateDialog(host.Services, session, null);
    var procedure = dialog.CreateProcedure(new(typeof(TestAction)));
    var context = environment.CreateContext(procedure);

    context.Dialog = dialog;

    if (global != null)
    {
      context.Global.Assign(global);
    }

    try
    {
      context.Run(action);

      if (rollback)
      {
        context.Transaction.Rollback();
      }
    }
    finally
    {
      if (global != null)
      {
        global.Assign(context.Global);
      }
    }
  }

  private class TestAction
  {
    public class Import { }
    public class Export { }

    [Entry]
    public static void Execute(IContext context, Import import, Export export)
    { 
    }
  }
}
