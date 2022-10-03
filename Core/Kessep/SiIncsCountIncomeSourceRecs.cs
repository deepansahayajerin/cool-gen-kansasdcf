// Program: SI_INCS_COUNT_INCOME_SOURCE_RECS, ID: 371763115, model: 746.
// Short name: SWE01118
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_INCS_COUNT_INCOME_SOURCE_RECS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiIncsCountIncomeSourceRecs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INCS_COUNT_INCOME_SOURCE_RECS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIncsCountIncomeSourceRecs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIncsCountIncomeSourceRecs.
  /// </summary>
  public SiIncsCountIncomeSourceRecs(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------
    // Check for more than one Income Source
    // record for the given person.
    // -------------------------------------
    local.Record.Count = 0;

    foreach(var item in ReadIncomeSource())
    {
      export.IncomeSource.Identifier = entities.IncomeSource.Identifier;
      ++local.Record.Count;

      if (local.Record.Count > 1)
      {
        export.Result.Command = "MORE THAN ONE RECORD";

        return;
      }
    }

    if (local.Record.Count == 0)
    {
      export.Result.Command = "NO RECORDS";
    }
    else
    {
      export.Result.Command = "ONE RECORD";
    }
  }

  private IEnumerable<bool> ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Result.
    /// </summary>
    [JsonPropertyName("result")]
    public Common Result
    {
      get => result ??= new();
      set => result = value;
    }

    private IncomeSource incomeSource;
    private Common result;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
    }

    private Common record;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    private CsePerson csePerson;
    private IncomeSource incomeSource;
  }
#endregion
}
