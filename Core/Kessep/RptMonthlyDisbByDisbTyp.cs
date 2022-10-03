// Program: RPT_MONTHLY_DISB_BY_DISB_TYP, ID: 372816511, model: 746.
// Short name: SWE00223
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: RPT_MONTHLY_DISB_BY_DISB_TYP.
/// </para>
/// <para>
/// Reports the number of disbursements and dollar amount of disbursements by 
/// disbursement type for audit purposes.
/// This report only includes Disbursement Transactions for subtype D.
/// </para>
/// </summary>
[Serializable]
public partial class RptMonthlyDisbByDisbTyp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the RPT_MONTHLY_DISB_BY_DISB_TYP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new RptMonthlyDisbByDisbTyp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of RptMonthlyDisbByDisbTyp.
  /// </summary>
  public RptMonthlyDisbByDisbTyp(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ----------------------------------------------------------------
    // Set up the date range for the report.
    // ----------------------------------------------------------------
    local.BegOfRpt.Date = import.ProgramProcessingInfo.ProcessDate;
    local.EndOfRpt.Date = AddDays(AddMonths(local.BegOfRpt.Date, 1), -1);

    // ----------------------------------------------------------------
    // OPEN the Report File.
    // ----------------------------------------------------------------
    local.Send.Parm1 = "OF";
    local.Send.Parm2 = "";
    UseEabRptMonthlyDisbursement2();

    if (!IsEmpty(local.Return1.Parm1))
    {
      UseCabRc4ErrorCodeTranslation();

      return;
    }

    // ----------------------------------------------------------------
    // Set the Report Parameters to Generate Report.
    // ----------------------------------------------------------------
    local.Send.Parm1 = "GR";
    local.Send.Parm2 = "";

    // ----------------------------------------------------------------
    // Determine the number of disbursements and the total dollar
    // amount disbursed for each disbursement type for the
    // specified month.  Only include the 'D' type disbursement
    // transactions.
    // ----------------------------------------------------------------
    foreach(var item in ReadDisbursementType())
    {
      local.Key.SystemGeneratedIdentifier =
        entities.DisbursementType.SystemGeneratedIdentifier;
      MoveDisbursementType(entities.DisbursementType, local.DisbursementType);
      local.MonthlyDisb.Count = 0;
      local.MonthlyDisb.TotalCurrency = 0;
      ReadDisbursementTransaction();

      if (local.MonthlyDisb.Count > 0)
      {
        // ---------------------------------------------------------------
        // Write totals for the current disbursement type to the report.
        // ---------------------------------------------------------------
        UseEabRptMonthlyDisbursement1();

        if (!IsEmpty(local.Return1.Parm1))
        {
          UseCabRc4ErrorCodeTranslation();

          return;
        }
      }
    }

    // ----------------------------------------------------------------
    // CLOSE the Report File.
    // ----------------------------------------------------------------
    local.Send.Parm1 = "CF";
    local.Send.Parm2 = "";
    UseEabRptMonthlyDisbursement2();

    if (!IsEmpty(local.Return1.Parm1))
    {
      UseCabRc4ErrorCodeTranslation();
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveDisbursementType(DisbursementType source,
    DisbursementType target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabRc4ErrorCodeTranslation()
  {
    var useImport = new CabRc4ErrorCodeTranslation.Import();
    var useExport = new CabRc4ErrorCodeTranslation.Export();

    MoveReportParms(local.Return1, useImport.Report1Composer);

    Call(CabRc4ErrorCodeTranslation.Execute, useImport, useExport);
  }

  private void UseEabRptMonthlyDisbursement1()
  {
    var useImport = new EabRptMonthlyDisbursement.Import();
    var useExport = new EabRptMonthlyDisbursement.Export();

    MoveReportParms(local.Send, useImport.ReportParms);
    useImport.RunDate.Date = import.Run.Date;
    useImport.BegOfRpt.Date = local.BegOfRpt.Date;
    useImport.EndOfRpt.Date = local.EndOfRpt.Date;
    MoveDisbursementType(local.DisbursementType, useImport.DisbursementType);
    MoveCommon(local.MonthlyDisb, useImport.MonthlyDisb);
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabRptMonthlyDisbursement.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private void UseEabRptMonthlyDisbursement2()
  {
    var useImport = new EabRptMonthlyDisbursement.Import();
    var useExport = new EabRptMonthlyDisbursement.Export();

    MoveReportParms(local.Send, useImport.ReportParms);
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabRptMonthlyDisbursement.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private bool ReadDisbursementTransaction()
  {
    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.
          SetDecimal(command, "totalCurrency", local.MonthlyDisb.TotalCurrency);
          
        db.SetNullableInt32(
          command, "dbtGeneratedId", local.Key.SystemGeneratedIdentifier);
        db.SetDate(command, "date1", local.BegOfRpt.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.EndOfRpt.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.MonthlyDisb.Count = db.GetInt32(reader, 0);
        local.MonthlyDisb.TotalCurrency = db.GetDecimal(reader, 1);
      });
  }

  private IEnumerable<bool> ReadDisbursementType()
  {
    entities.DisbursementType.Populated = false;

    return ReadEach("ReadDisbursementType",
      null,
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.Name = db.GetString(reader, 2);
        entities.DisbursementType.Populated = true;

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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Run.
    /// </summary>
    [JsonPropertyName("run")]
    public DateWorkArea Run
    {
      get => run ??= new();
      set => run = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea run;
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
    /// A value of BegOfRpt.
    /// </summary>
    [JsonPropertyName("begOfRpt")]
    public DateWorkArea BegOfRpt
    {
      get => begOfRpt ??= new();
      set => begOfRpt = value;
    }

    /// <summary>
    /// A value of EndOfRpt.
    /// </summary>
    [JsonPropertyName("endOfRpt")]
    public DateWorkArea EndOfRpt
    {
      get => endOfRpt ??= new();
      set => endOfRpt = value;
    }

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
    /// A value of Key.
    /// </summary>
    [JsonPropertyName("key")]
    public DisbursementType Key
    {
      get => key ??= new();
      set => key = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of MonthlyDisb.
    /// </summary>
    [JsonPropertyName("monthlyDisb")]
    public Common MonthlyDisb
    {
      get => monthlyDisb ??= new();
      set => monthlyDisb = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public ReportParms Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public ReportParms Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    private DateWorkArea begOfRpt;
    private DateWorkArea endOfRpt;
    private DateWorkArea null1;
    private DisbursementType key;
    private DisbursementType disbursementType;
    private Common monthlyDisb;
    private ReportParms send;
    private ReportParms return1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    private DisbursementType disbursementType;
    private DisbursementTransaction disbursementTransaction;
  }
#endregion
}
