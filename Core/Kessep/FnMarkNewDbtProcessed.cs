// Program: FN_MARK_NEW_DBT_PROCESSED, ID: 372277470, model: 746.
// Short name: SWE01616
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_MARK_NEW_DBT_PROCESSED.
/// </summary>
[Serializable]
public partial class FnMarkNewDbtProcessed: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_MARK_NEW_DBT_PROCESSED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnMarkNewDbtProcessed(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnMarkNewDbtProcessed.
  /// </summary>
  public FnMarkNewDbtProcessed(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // ??????	Tanmoy		Initial Code
    // 090497	Govind          Fixed to use PPI Process Date.
    // 120598  Ed Lyman        Cleaned up the mess that Govind made.
    // ---------------------------------------------
    try
    {
      UpdateDebt();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DEBT_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DEBT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UpdateDebt()
  {
    System.Diagnostics.Debug.Assert(import.Debt.Populated);

    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTmst = Now();
    var newDebtProcessDate = import.ProgramProcessingInfo.ProcessDate;

    import.Debt.Populated = false;
    Update("UpdateDebt",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "newDebtProcDt", newDebtProcessDate);
        db.SetInt32(command, "obgGeneratedId", import.Debt.ObgGeneratedId);
        db.SetString(command, "cspNumber", import.Debt.CspNumber);
        db.SetString(command, "cpaType", import.Debt.CpaType);
        db.SetInt32(command, "obTrnId", import.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", import.Debt.Type1);
        db.SetInt32(command, "otyType", import.Debt.OtyType);
      });

    import.Debt.LastUpdatedBy = lastUpdatedBy;
    import.Debt.LastUpdatedTmst = lastUpdatedTmst;
    import.Debt.NewDebtProcessDate = newDebtProcessDate;
    import.Debt.Populated = true;
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
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ObligationTransaction debt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
