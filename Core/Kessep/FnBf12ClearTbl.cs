// Program: FN_BF12_CLEAR_TBL, ID: 373333805, model: 746.
// Short name: SWE02742
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF12_CLEAR_TBL.
/// </summary>
[Serializable]
public partial class FnBf12ClearTbl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF12_CLEAR_TBL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf12ClearTbl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf12ClearTbl.
  /// </summary>
  public FnBf12ClearTbl(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // 2002-02-21  WR 000235  Fangman - New AB to clear out the recap tablew.
    // ***************************************************
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ProcessDtTblGroup group.</summary>
    [Serializable]
    public class ProcessDtTblGroup
    {
      /// <summary>
      /// A value of MoSumTblUpdatedInd.
      /// </summary>
      [JsonPropertyName("moSumTblUpdatedInd")]
      public Common MoSumTblUpdatedInd
      {
        get => moSumTblUpdatedInd ??= new();
        set => moSumTblUpdatedInd = value;
      }

      /// <summary>
      /// A value of MonthlyObligeeSummary.
      /// </summary>
      [JsonPropertyName("monthlyObligeeSummary")]
      public MonthlyObligeeSummary MonthlyObligeeSummary
      {
        get => monthlyObligeeSummary ??= new();
        set => monthlyObligeeSummary = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 360;

      private Common moSumTblUpdatedInd;
      private MonthlyObligeeSummary monthlyObligeeSummary;
    }

    /// <summary>
    /// Gets a value of ProcessDtTbl.
    /// </summary>
    [JsonIgnore]
    public Array<ProcessDtTblGroup> ProcessDtTbl => processDtTbl ??= new(
      ProcessDtTblGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ProcessDtTbl for json serialization.
    /// </summary>
    [JsonPropertyName("processDtTbl")]
    [Computed]
    public IList<ProcessDtTblGroup> ProcessDtTbl_Json
    {
      get => processDtTbl;
      set => ProcessDtTbl.Assign(value);
    }

    private Array<ProcessDtTblGroup> processDtTbl;
  }
#endregion
}
