// Program: FN_B623_MO_DISB_TYP_SUMMARY_RPT, ID: 372816470, model: 746.
// Short name: SWEB623P
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B623_MO_DISB_TYP_SUMMARY_RPT.
/// </para>
/// <para>
/// Reports the number of disbursements and dollar amount of disbursements by 
/// disbursement type for audit purposes.
/// This report only includes Disbursement Transactions for subtype D.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB623MoDisbTypSummaryRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B623_MO_DISB_TYP_SUMMARY_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB623MoDisbTypSummaryRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB623MoDisbTypSummaryRpt.
  /// </summary>
  public FnB623MoDisbTypSummaryRpt(IContext context, Import import,
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
    // ---------------------------------------------------------------------
    //                        MODIFICATION LOG
    // ---------------------------------------------------------------------
    // DATE		DEVELOPER	DESCRIPTION
    // ---------------------------------------------------------------------
    // 07/05/99	J Katz		Created procedure.
    // ---------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.CurrentRun.Date = Now().Date;
    local.ProgramProcessingInfo.Name = global.UserId;

    // ---------------------------------------------------------------------
    // Retrieve the run parameters for this program.
    // ---------------------------------------------------------------------
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------------------------------
    // Process Date represents the start date for the report.
    // Cannot continue processing if Process Date is null.
    // ---------------------------------------------------------------------
    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Null1.Date))
    {
      ExitState = "INVALID_PROCESS_DATE_RB";

      return;
    }

    // ---------------------------------------------------------------------
    // Process the report.
    // ---------------------------------------------------------------------
    UseRptMonthlyDisbByDisbTyp();

    // ---------------------------------------------------------------------
    // Any exit state returned other than 'all ok' should terminate
    // processing with abort.  Logic performs this check in case
    // additional code is added at a later time.
    // ---------------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseRptMonthlyDisbByDisbTyp()
  {
    var useImport = new RptMonthlyDisbByDisbTyp.Import();
    var useExport = new RptMonthlyDisbByDisbTyp.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Run.Date = local.CurrentRun.Date;

    Call(RptMonthlyDisbByDisbTyp.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of CurrentRun.
    /// </summary>
    [JsonPropertyName("currentRun")]
    public DateWorkArea CurrentRun
    {
      get => currentRun ??= new();
      set => currentRun = value;
    }

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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    private DateWorkArea null1;
    private DateWorkArea currentRun;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
  }
#endregion
}
