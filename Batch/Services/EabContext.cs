using System;
using System.Collections.Generic;
using System.Data.Common;

using Microsoft.Extensions.DependencyInjection;

using Bphx.Cool;
using Bphx.Cool.Cobol;

using MDSY.Framework.Core;
using MDSY.Framework.Data.SQL;

using GOV.KS.DCF.CSS.Common.BL;
using MDSY.Framework.Control.CICS;
using System.Runtime.ExceptionServices;

namespace KansasDCF.Cse.Batch;

public class EabContext: IEabContext, IDisposable
{
  public IContext Context { get; init; }

  public void Dispose()
  {
    Functions.Dispose(programs);
    programs.Clear();
  }

  public int Execute(string name, byte[][] args)
  {
    if (!programs.TryGetValue(name, out var program))
    {
      var type = ProgramUtilities.GetBLType(name) ??
        throw new InvalidOperationException($"Program {name} not found.");

      program = (EABBase)Activator.CreateInstance(type);
      programs.Add(name, program);
    }

    // Set Passed connection if needed
    var db = Context.Transaction.
      GetService<IDbConnectionProvider>();
    var dbConv = program.DbConv ??= new DBConversation();

    dbConv.SetNewConnection((DbConnection)db.Connection);
    dbConv.SetNewTransaction((DbTransaction)db.Transaction);

    program.Control.ExitProgram = false;
    program.Control.CancelProgram = false;
    ServiceControl.CurrentException = null;

    var result = program.ExecuteMain(args);
    var error = ServiceControl.CurrentException;

    if (error != null)
    {
      ExceptionDispatchInfo.Capture(error).Throw();
    }

    return result;
  }

  private readonly Dictionary<string, EABBase> programs = new();
}
