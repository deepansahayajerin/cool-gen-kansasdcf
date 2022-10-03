// Program: FN_B717_CHECK_IF_CASE_IS_COUNTED, ID: 373348537, model: 746.
// Short name: SWE03046
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_CHECK_IF_CASE_IS_COUNTED.
/// </summary>
[Serializable]
public partial class FnB717CheckIfCaseIsCounted: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_CHECK_IF_CASE_IS_COUNTED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717CheckIfCaseIsCounted(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717CheckIfCaseIsCounted.
  /// </summary>
  public FnB717CheckIfCaseIsCounted(IContext context, Import import,
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
    if (ReadStatsVerifi())
    {
      export.CaseAlreadyCounted.Flag = "Y";
    }
    else
    {
      export.CaseAlreadyCounted.Flag = "N";
    }
  }

  private bool ReadStatsVerifi()
  {
    entities.StatsVerifi.Populated = false;

    return Read("ReadStatsVerifi",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "yearMonth",
          import.StatsReport.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "firstRunNumber",
          import.StatsReport.FirstRunNumber.GetValueOrDefault());
        db.SetInt32(command, "count", import.LineNumber.Count);
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.StatsVerifi.YearMonth = db.GetNullableInt32(reader, 0);
        entities.StatsVerifi.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.StatsVerifi.LineNumber = db.GetNullableInt32(reader, 2);
        entities.StatsVerifi.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.StatsVerifi.CaseNumber = db.GetNullableString(reader, 4);
        entities.StatsVerifi.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of LineNumber.
    /// </summary>
    [JsonPropertyName("lineNumber")]
    public Common LineNumber
    {
      get => lineNumber ??= new();
      set => lineNumber = value;
    }

    /// <summary>
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
    }

    private Case1 case1;
    private Common lineNumber;
    private StatsReport statsReport;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseAlreadyCounted.
    /// </summary>
    [JsonPropertyName("caseAlreadyCounted")]
    public Common CaseAlreadyCounted
    {
      get => caseAlreadyCounted ??= new();
      set => caseAlreadyCounted = value;
    }

    private Common caseAlreadyCounted;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of StatsVerifi.
    /// </summary>
    [JsonPropertyName("statsVerifi")]
    public StatsVerifi StatsVerifi
    {
      get => statsVerifi ??= new();
      set => statsVerifi = value;
    }

    private StatsVerifi statsVerifi;
  }
#endregion
}
