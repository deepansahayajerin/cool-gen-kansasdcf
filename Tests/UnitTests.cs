using Bphx.Cool;
using Bphx.Cool.Log;
using Gov.Kansas.DCF.Cse.Kessep;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gov.KansasDCF.Cse.Test;

[TestClass]
public class UnitTests
{
  readonly IHost host = Startup.CreateHostBuilder().Build();

  [TestInitialize]
  public void Init()
  {
    host.Start();
  }

  [TestCleanup]
  public void Cleanup()
  {
    host.StopAsync().Wait();
  }

  [TestMethod]
  public void TestEabSearchClient()
  {
    var global = Utils.FromJson<Global>(@"
{
  ""userId"": ""SWCOJRE"",
  ""exitstate"": ""ACO_NI0000_SUCCESSFUL_DISPLAY"",
  ""trancode"": ""SR1W"",
  ""command"": ""DISPLAY"",
  ""exitStateId"": 7615526,
  ""terminationAction"": ""normal"",
  ""messageType"": ""Info""
}");

    var import = Utils.FromJson<EabSearchClient.Import>(@"
{
  ""phonetic"": {
    ""percentage"": 35
  },
  ""csePersonsWorkSet"": {
    ""ssn"": ""785980283"",
    ""firstName"": ""TANYA"",
    ""lastName"": ""REP-BRYSON""
  },
  ""search"": {
    ""flag"": ""1""
  },
}");

    var export = Utils.FromJson<EabSearchClient.Export>("{}");

    host.Run(
      action: context => context.Call(EabSearchClient.Execute, import, export),
      global: global,
      rollback: true);

    Assert.IsTrue(export.Next.UniqueKey.Length > 0);
  }

  [TestMethod]
  public void TestEabCreateCsePerson()
  {
    var global = Utils.FromJson<Global>(@"
{
  ""currentDialect"": ""DEFAULT"",
  ""scrollAmt"": ""PAGE"",
  ""userId"": ""SWCOJRE"",
  ""exitstate"": ""ACO_NN0000_ALL_OK"",
  ""trancode"": ""SR2G"",
  ""command"": ""ADD"",
  ""exitStateId"": 25430152,
  ""terminationAction"": ""normal"",
  ""messageType"": ""None""
}");

    var import = Utils.FromJson<EabCreateCsePerson.Import>(@"
{
  ""new1"": {
    ""sex"": ""F"",
    ""dob"": ""1989-12-12T00:00:00"",
    ""ssn"": ""785980283"",
    ""firstName"": ""TANYA"",
    ""lastName"": ""REP-BRYSON""
  }
}");

    var export = Utils.FromJson<EabCreateCsePerson.Export>("{}");

    host.Run(
      action: context => context.Call(EabCreateCsePerson.Execute, import, export),
      global: global,
      rollback: true);

    Assert.IsTrue(export.CsePersonsWorkSet.Number.Length > 0);
  }

  [TestMethod]
  public void TestEabReadCsePerson()
  {
    var global = Utils.FromJson<Global>(@"
{
  ""currentDialect"": ""DEFAULT"",
  ""scrollAmt"": ""PAGE"",
  ""userId"": ""SWCOJRE"",
  ""exitstate"": ""ACO_NN0000_ALL_OK"",
  ""trancode"": ""SR2G"",
  ""command"": ""CREATE"",
  ""exitStateId"": 25430152
}");

    var import = Utils.FromJson<EabReadCsePerson.Import>(@"
{
  ""csePersonsWorkSet"": { ""number"": ""0000000030"" },
  ""current"": { ""date"": ""2022-02-18"" }
}");

    var export = Utils.FromJson<EabReadCsePerson.Export>(@"
{
  ""csePersonsWorkSet"": { ""number"": ""0000000030"" },
}");

    host.Run(
      action: context => context.Call(EabReadCsePerson.Execute, import, export),
      global: global,
      rollback: true);

    // Insert here condition that should be satisfied.
    Assert.IsTrue(true);
  }

  [TestMethod]
  public void TestEabRollbackCics()
  {
    var import = new EabRollbackCics.Import();
    var export = new EabRollbackCics.Export();

    host.Run(
      action: context => context.GetService<IEabStub>().
        Execute("SWEXGRLB", context, import, export, EabOptions.NoIefParams),
      global: null,
      rollback: true);
 
    // Insert here condition that should be satisfied.
    Assert.IsTrue(true);
  }
}