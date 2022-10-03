// Program: FN_B790_POTENTIAL_RECOVERY_RPT, ID: 373320873, model: 746.
// Short name: SWEF790B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B790_POTENTIAL_RECOVERY_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB790PotentialRecoveryRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B790_POTENTIAL_RECOVERY_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB790PotentialRecoveryRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB790PotentialRecoveryRpt.
  /// </summary>
  public FnB790PotentialRecoveryRpt(IContext context, Import import,
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
    // -------------------------------------------------------------------------------
    // 02/03/2002                  Vithal Madhira            Initial 
    // Development.
    // This batch program will create the "Potential Recovery Report". This is a
    // monthly batch program.
    // -------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ----------------------------------------------------
    //        House Keeping.
    // ---------------------------------------------------
    local.MaximumDate.Date = new DateTime(2099, 12, 31);
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ----------------------------------------------------
    //           Open The Report.
    // ---------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.EabReportSend.ReportNumber = 1;
    UseEabWriteReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------------------------------------------------
    //         Write the Report Heading.
    // ---------------------------------------------------
    local.EabReportSend.Assign(local.Initialized);
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.Command = "HEADING";
    local.EabFileHandling.Status = "";
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.EabReportSend.ReportNumber = 1;
    local.EabReportSend.ReportNoPart2 = "01";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.RunDate = Now().Date;
    local.EabReportSend.RunTime = Time(Now());
    local.EabReportSend.BlankLineAfterHeading = "Y";
    local.EabReportSend.RptHeading1 =
      "     KANSAS DEPARTMENT OF SOCIAL AND REHABILITATION SERVICES";
    local.EabReportSend.RptHeading2 =
      "                    CHILD SUPPORT ENFORCEMENT";
    local.EabReportSend.RptHeading3 =
      "                    POTENTIAL RECOVERY REPORT";
    UseEabWriteReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------------------------------------------------
    //         Write  Column  Headings.
    // ---------------------------------------------------
    local.EabReportSend.Command = "WRITE";
    local.EabReportSend.Command = "DETAIL";
    local.EabFileHandling.Status = "";
    local.EabReportSend.ReportNumber = 1;
    local.EabReportSend.RptDetail = "CODE           CREATED BY   " + "COUNT         " +
      "AMOUNT        ";
    UseEabWriteReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------------------------------------------------
    //         Write  Detail   Records.
    // ---------------------------------------------------
    foreach(var item in ReadPaymentStatusHistoryPaymentStatusPaymentRequest())
    {
      // ----------------------------------------------------
      //         Write  the Records.
      // ---------------------------------------------------
      local.TempAmountTextWorkArea.Text12 =
        NumberToString((long)local.TempPrrAmount.AverageCurrency, 7, 9) + TrimEnd
        (".") + NumberToString
        ((long)(local.TempPrrAmount.AverageCurrency * 100), 14, 2);
      local.EabReportSend.RptDetail = local.TempPaymentStatus.Code + "     " + local
        .TempPaymentStatusHistory.CreatedBy + "     " + NumberToString
        (local.PshCount.Count, 7, 9) + "     " + local
        .TempAmountTextWorkArea.Text12;
      UseEabWriteReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // -----------------------------------------------------------------
      //     Reinitialize all populated views before populating again.
      // ----------------------------------------------------------------
      local.EabReportSend.RptDetail = "";
      local.TempPaymentStatus.Code = "";
      local.TempPaymentStatusHistory.CreatedBy = "";
      local.PshCount.Count = 0;
      local.TempAmountPaymentRequest.Amount = 0;
      local.TempAmountTextWorkArea.Text12 = "";
    }

    // ----------------------------------------------------
    //         Write the Report Heading.
    // ---------------------------------------------------
    local.EabReportSend.Assign(local.Initialized);
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.Command = "HEADING";
    local.EabFileHandling.Status = "";
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.EabReportSend.ReportNumber = 1;
    local.EabReportSend.ReportNoPart2 = "01";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.RunDate = Now().Date;
    local.EabReportSend.RunTime = Time(Now());
    local.EabReportSend.BlankLineAfterHeading = "Y";
    local.EabReportSend.RptHeading1 =
      "     KANSAS DEPARTMENT OF SOCIAL AND REHABILITATION SERVICES";
    local.EabReportSend.RptHeading2 =
      "                    CHILD SUPPORT ENFORCEMENT";
    local.EabReportSend.RptHeading3 =
      "                     MANUAL RECOVERY REPORT";
    UseEabWriteReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------------------------------------------------
    //         Write  Column  Headings.
    // ---------------------------------------------------
    local.EabReportSend.Command = "WRITE";
    local.EabReportSend.Command = "DETAIL";
    local.EabFileHandling.Status = "";
    local.EabReportSend.ReportNumber = 1;
    local.EabReportSend.RptDetail = "CREATED BY     " + "COUNT          " + "AMOUNT        ";
      
    UseEabWriteReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -------------------------------------------------------------------------------------
    // FOR EACH USER (stored in local_temp obligation created_by) :
    // 1. First READ all the Recovery obligations (Manually Created + Created 
    // from Potential recoveries) .
    //            The first Summarize statement will do the above READ. The '
    // COUNT' and 'SUM(Ob_Tran Amount) will be stored in temporary views_1.
    // 2. Then READ the Recovery obligations created from Potential Recoveries 
    // by the SAME above USER (local_temp obligation Created_By) . In case of
    // obligations created from potentail recoveries, a relationship will be
    // established between 'Obligation' and 'Payment_Request'. Use this
    // relationship to read the obligations created from 'Potential Recoveries'.
    //             The second SUMMARIZE statement will complete the above READ. 
    // The COUNT and SUM(Ob_Tran Amount) will be stored in temporary views_2.
    // 3. Finally to get the Manually created obligations subtract the  second 
    // SUMMARIZE data ( temp view_2) from first SUMMARIZE data (temp_view_1)
    // --------------------------------------------------------------------------------------
    foreach(var item in ReadObligationObligationTransactionObligationType1())
    {
      foreach(var item1 in ReadObligationObligationTransactionObligationType2())
      {
      }

      local.FinalObTranAmount.AverageCurrency =
        local.TempObTranAmount1.AverageCurrency - local
        .TempObTranAmount2.AverageCurrency;
      local.FinalCount.Count = local.Count1.Count - local.Count2.Count;

      // ----------------------------------------------------
      //         Write  the Records.
      // ---------------------------------------------------
      local.TempAmountTextWorkArea.Text12 =
        NumberToString((long)local.FinalObTranAmount.AverageCurrency, 7, 9) + TrimEnd
        (".") + NumberToString
        ((long)(local.FinalObTranAmount.AverageCurrency * 100), 14, 2);
      local.EabReportSend.RptDetail = local.TempObligation.CreatedBy + "       " +
        NumberToString(local.FinalCount.Count, 7, 9) + "      " + local
        .TempAmountTextWorkArea.Text12;
      UseEabWriteReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // -----------------------------------------------------------------
      //     Reinitialize all populated views before populating again.
      // ----------------------------------------------------------------
      local.EabReportSend.RptDetail = "";
      local.TempObligation.CreatedBy = "";
      local.FinalCount.Count = 0;
      local.Count1.Count = 0;
      local.Count2.Count = 0;
      local.TempObTranAmount1.AverageCurrency = 0;
      local.TempObTranAmount2.AverageCurrency = 0;
      local.FinalObTranAmount.AverageCurrency = 0;
      local.TempAmountTextWorkArea.Text12 = "";
    }

    // ----------------------------------------------------
    //         Close the Extract File.
    // ---------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    local.EabReportSend.ReportNumber = 1;
    UseEabWriteReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private void UseEabWriteReport()
  {
    var useImport = new EabWriteReport.Import();
    var useExport = new EabWriteReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.Assign(local.EabReportSend);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.EabReportReturn.Assign(local.EabReportReturn);

    Call(EabWriteReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadObligationObligationTransactionObligationType1()
  {
    local.TempObligation.Populated = false;

    return ReadEach("ReadObligationObligationTransactionObligationType1",
      null,
      (db, reader) =>
      {
        local.TempObligation.CreatedBy = db.GetString(reader, 0);
        local.Count1.Count = db.GetInt32(reader, 1);
        local.TempObTranAmount1.AverageCurrency = db.GetDecimal(reader, 2);
        local.TempObligation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationTransactionObligationType2()
  {
    local.TempObligation.Populated = false;

    return ReadEach("ReadObligationObligationTransactionObligationType2",
      (db, command) =>
      {
        db.SetString(command, "createdBy", local.TempObligation.CreatedBy);
      },
      (db, reader) =>
      {
        local.TempObligation.CreatedBy = db.GetString(reader, 0);
        local.Count2.Count = db.GetInt32(reader, 1);
        local.TempObTranAmount2.AverageCurrency = db.GetDecimal(reader, 2);
        local.TempObligation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadPaymentStatusHistoryPaymentStatusPaymentRequest()
  {
    local.TempPaymentStatus.Populated = false;
    local.TempPaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentStatusHistoryPaymentStatusPaymentRequest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.TempPaymentStatus.Code = db.GetString(reader, 0);
        local.TempPaymentStatusHistory.CreatedBy = db.GetString(reader, 1);
        local.PshCount.Count = db.GetInt32(reader, 2);
        local.TempPrrAmount.AverageCurrency = db.GetDecimal(reader, 3);
        local.TempPaymentStatus.Populated = true;
        local.TempPaymentStatusHistory.Populated = true;

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
    /// A value of FinalAmount.
    /// </summary>
    [JsonPropertyName("finalAmount")]
    public ObligationTransaction FinalAmount
    {
      get => finalAmount ??= new();
      set => finalAmount = value;
    }

    /// <summary>
    /// A value of FinalCount.
    /// </summary>
    [JsonPropertyName("finalCount")]
    public Common FinalCount
    {
      get => finalCount ??= new();
      set => finalCount = value;
    }

    /// <summary>
    /// A value of Count2.
    /// </summary>
    [JsonPropertyName("count2")]
    public Common Count2
    {
      get => count2 ??= new();
      set => count2 = value;
    }

    /// <summary>
    /// A value of Count1.
    /// </summary>
    [JsonPropertyName("count1")]
    public Common Count1
    {
      get => count1 ??= new();
      set => count1 = value;
    }

    /// <summary>
    /// A value of TempAmount2.
    /// </summary>
    [JsonPropertyName("tempAmount2")]
    public ObligationTransaction TempAmount2
    {
      get => tempAmount2 ??= new();
      set => tempAmount2 = value;
    }

    /// <summary>
    /// A value of TempAmount1.
    /// </summary>
    [JsonPropertyName("tempAmount1")]
    public ObligationTransaction TempAmount1
    {
      get => tempAmount1 ??= new();
      set => tempAmount1 = value;
    }

    /// <summary>
    /// A value of TempObligation.
    /// </summary>
    [JsonPropertyName("tempObligation")]
    public Obligation TempObligation
    {
      get => tempObligation ??= new();
      set => tempObligation = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public EabReportSend Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of TempAmountPaymentRequest.
    /// </summary>
    [JsonPropertyName("tempAmountPaymentRequest")]
    public PaymentRequest TempAmountPaymentRequest
    {
      get => tempAmountPaymentRequest ??= new();
      set => tempAmountPaymentRequest = value;
    }

    /// <summary>
    /// A value of TempPaymentStatus.
    /// </summary>
    [JsonPropertyName("tempPaymentStatus")]
    public PaymentStatus TempPaymentStatus
    {
      get => tempPaymentStatus ??= new();
      set => tempPaymentStatus = value;
    }

    /// <summary>
    /// A value of TempPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("tempPaymentStatusHistory")]
    public PaymentStatusHistory TempPaymentStatusHistory
    {
      get => tempPaymentStatusHistory ??= new();
      set => tempPaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of MaximumDate.
    /// </summary>
    [JsonPropertyName("maximumDate")]
    public DateWorkArea MaximumDate
    {
      get => maximumDate ??= new();
      set => maximumDate = value;
    }

    /// <summary>
    /// A value of PshCount.
    /// </summary>
    [JsonPropertyName("pshCount")]
    public Common PshCount
    {
      get => pshCount ??= new();
      set => pshCount = value;
    }

    /// <summary>
    /// A value of EabReportReturn.
    /// </summary>
    [JsonPropertyName("eabReportReturn")]
    public EabReportReturn EabReportReturn
    {
      get => eabReportReturn ??= new();
      set => eabReportReturn = value;
    }

    /// <summary>
    /// A value of TempAmountTextWorkArea.
    /// </summary>
    [JsonPropertyName("tempAmountTextWorkArea")]
    public TextWorkArea TempAmountTextWorkArea
    {
      get => tempAmountTextWorkArea ??= new();
      set => tempAmountTextWorkArea = value;
    }

    /// <summary>
    /// A value of TempPrrAmount.
    /// </summary>
    [JsonPropertyName("tempPrrAmount")]
    public Common TempPrrAmount
    {
      get => tempPrrAmount ??= new();
      set => tempPrrAmount = value;
    }

    /// <summary>
    /// A value of FinalObTranAmount.
    /// </summary>
    [JsonPropertyName("finalObTranAmount")]
    public Common FinalObTranAmount
    {
      get => finalObTranAmount ??= new();
      set => finalObTranAmount = value;
    }

    /// <summary>
    /// A value of TempObTranAmount2.
    /// </summary>
    [JsonPropertyName("tempObTranAmount2")]
    public Common TempObTranAmount2
    {
      get => tempObTranAmount2 ??= new();
      set => tempObTranAmount2 = value;
    }

    /// <summary>
    /// A value of TempObTranAmount1.
    /// </summary>
    [JsonPropertyName("tempObTranAmount1")]
    public Common TempObTranAmount1
    {
      get => tempObTranAmount1 ??= new();
      set => tempObTranAmount1 = value;
    }

    private ObligationTransaction finalAmount;
    private Common finalCount;
    private Common count2;
    private Common count1;
    private ObligationTransaction tempAmount2;
    private ObligationTransaction tempAmount1;
    private Obligation tempObligation;
    private EabReportSend initialized;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private EabReportSend eabReportSend;
    private PaymentRequest tempAmountPaymentRequest;
    private PaymentStatus tempPaymentStatus;
    private PaymentStatusHistory tempPaymentStatusHistory;
    private DateWorkArea maximumDate;
    private Common pshCount;
    private EabReportReturn eabReportReturn;
    private TextWorkArea tempAmountTextWorkArea;
    private Common tempPrrAmount;
    private Common finalObTranAmount;
    private Common tempObTranAmount2;
    private Common tempObTranAmount1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public PaymentRequest Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private PaymentRequest temp;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private Obligation obligation;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private PaymentRequest paymentRequest;
  }
#endregion
}
