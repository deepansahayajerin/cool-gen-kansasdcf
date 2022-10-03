// Program: SI_QUICK_FN_TEST, ID: 374552697, model: 746.
// Short name: QKFNTSTP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_QUICK_FN_TEST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiQuickFnTest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_FN_TEST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickFnTest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickFnTest.
  /// </summary>
  public SiQuickFnTest(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // :
    //    Date             Developer        WR#         Description
    //  
    // --------------------------------------------------------------------
    //    09-29-2009       Linda Smith      211         Initial Development
    //  NOTE:
    //        END of   M A I N T E N A N C E   L O G
    //  
    // --------------------------------------------------------------------
    // ***********************************************************************************************************************************************
    // Action block:  The action block SI_QUICK_FINANCIAL (SWE03120) is used to 
    // retrieve the Financial data for the QUICK system. 
    // In Production, this action block is called by a COBOL stored procedure 
    // that is stored in and executed by DB2. 
    // The stored procedure in DB2 is triggered by a call from a .NET web 
    // service.
    // PStep:  For the action block to be gen'd, it must have a PStep that calls
    // it.  That PStep is SI_QUICK_FN_TEST (QKFNTSTP). 
    // The PStep is used to gen the action block and facilitate testing, but is 
    // never executed in Production. 
    // The PStep also uses the external action block 
    // SI_EAB_PROCESS_QUICK_INPUT_FILE (SWEXQR01) to read query records from a
    // data set.
    // Batch Job:  To facilitate testing of this action block, a batch job has 
    // been created.  The batch job is SRRUNQFN. 
    // SRRUNQFN executes the QKFNTSTP PStep, which in turn executes the SWE03120
    // action block. 
    // SRRUNQFN is to be used only for testing and should never be migrated to 
    // or executed in the Production environment.
    // ***********************************************************************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // Open control report
    local.Report.Action = "OPEN";
    local.ReportOpen.ProcessDate = Now().Date;
    local.ReportOpen.ProgramName = "QKFNTEST";
    UseCabControlReport2();

    if (!Equal(local.Report.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // Open input file
    local.Input.Action = "OPEN";
    UseSiEabProcessQuickInputFile1();

    if (!Equal(local.Input.Status, "OK"))
    {
      ExitState = "ACO_RC_AB0001_ERROR_ON_OPEN_FILE";

      return;
    }

    local.MoreData.Flag = "Y";
    local.Input.Action = "READ";
    local.Report.Action = "WRITE";

    while(AsChar(local.MoreData.Flag) == 'Y')
    {
      // Read next record
      UseSiEabProcessQuickInputFile2();

      // Delete   -----   start
      // Delete   -----   end
      switch(TrimEnd(local.Input.Status))
      {
        case "OK":
          ExitState = "ACO_NN0000_ALL_OK";
          UseSiQuickFinancial();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            for(local.Common.Count = 1; local.Common.Count <= 21; ++
              local.Common.Count)
            {
              switch(local.Common.Count)
              {
                case 0:
                  break;
                case 1:
                  local.ReportLabel.Text32 = "INPUT CASE NUMBER:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                    .QuickInQuery.CaseId;

                  break;
                case 2:
                  local.ReportLabel.Text32 = "OUTPUT CASE NUMBER:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                    .Case1.Number;

                  break;
                case 3:
                  local.ReportLabel.Text32 = "HEADER AP FIRST NAME:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                    .QuickCpHeader.NcpFirstName;

                  break;
                case 4:
                  local.ReportLabel.Text32 = "HEADER AP MIDDLE INITIAL";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                    .QuickCpHeader.NcpMiddleInitial;

                  break;
                case 5:
                  local.ReportLabel.Text32 = "HEADER AP LAST NAME:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                    .QuickCpHeader.NcpLastName;

                  break;
                case 6:
                  if (AsChar(local.QuickCpHeader.CpTypeCode) == 'O')
                  {
                    local.ReportLabel.Text32 = "ORGANIZATION NAME:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                      .QuickCpHeader.CpOrganizationName;
                  }
                  else
                  {
                    local.ReportLabel.Text32 = "HEADER AR FIRST NAME:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                      .QuickCpHeader.CpFirstName;
                    UseCabControlReport3();

                    if (!Equal(local.Report.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.ReportLabel.Text32 = "HEADER AR MIDDLE INITIAL:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                      .QuickCpHeader.CpMiddleInitial;
                    UseCabControlReport3();

                    if (!Equal(local.Report.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.ReportLabel.Text32 = "HEADER AR LAST NAME:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                      .QuickCpHeader.CpLastName;
                  }

                  break;
                case 7:
                  local.ReportLabel.Text32 = "LAST PAYMENT AMOUNT:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.LastPaymentAmount, 15);

                  break;
                case 8:
                  local.ReportLabel.Text32 = "MONTHLY ARREARS AMOUNT:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.MonthlyArrearsAmount, 15);

                  break;
                case 9:
                  local.ReportLabel.Text32 = "MONTHLY SUPPORT AMOUNT:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.MonthlySupportAmount, 15);

                  break;
                case 10:
                  local.ReportLabel.Text32 = "OTHER MONTHLY AMOUNT:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.OtherMonthlyAmount, 15);

                  break;
                case 11:
                  local.ReportLabel.Text32 = "TOTAL ARREARS OWED:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.TotalArrearsOwed, 15);

                  break;
                case 12:
                  local.ReportLabel.Text32 = "TOTAL ASSIGNED ARREARS:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.TotalAssignedArrears, 15);

                  break;
                case 13:
                  local.ReportLabel.Text32 = "TOTAL INTEREST OWED:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.TotalInterestOwed, 15);

                  break;
                case 14:
                  local.ReportLabel.Text32 = "TOTAL JUDGMENT AMOUNT:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.TotalJudgmentAmount, 15);

                  break;
                case 15:
                  local.ReportLabel.Text32 = "TOTAL MONTHLY AMOUNT:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.TotalMonthlyAmount, 15);

                  break;
                case 16:
                  local.ReportLabel.Text32 = "TOTAL NCP FEES OWED:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.TotalNcpFeesOwed, 15);

                  break;
                case 17:
                  local.ReportLabel.Text32 = "TOTAL OWED AMOUNT:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                    ((long)local.QuickFinanceSummary.TotalOwedAmount, 15);

                  break;
                case 18:
                  local.ReportLabel.Text32 = "LAST PAYMENT DATE:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                    .QuickFinanceSummary.LastPaymentDate;

                  break;
                case 19:
                  local.ReportLabel.Text32 = "START DATE:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                    .QuickInQuery.StDate;

                  break;
                case 20:
                  local.ReportLabel.Text32 = "END DATE:";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                    .QuickInQuery.EndDate;

                  break;
                case 21:
                  local.ReportLabel.Text32 = ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>";
                  local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                    .ReportLabel.Text32;

                  break;
                default:
                  break;
              }

              UseCabControlReport3();

              if (!Equal(local.Report.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }

            local.ExportDisbursements.Index = 0;

            for(var limit = local.ExportDisbursements.Count; local
              .ExportDisbursements.Index < limit; ++
              local.ExportDisbursements.Index)
            {
              if (!local.ExportDisbursements.CheckSize())
              {
                break;
              }

              for(local.Common.Count = 1; local.Common.Count <= 5; ++
                local.Common.Count)
              {
                switch(local.Common.Count)
                {
                  case 0:
                    break;
                  case 1:
                    local.ReportLabel.Text32 = "INSTRUMENT NUMBER:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                      .ExportDisbursements.Item.QuickFinanceDisbursement.
                        InstrumentNumber;

                    break;
                  case 2:
                    local.ReportLabel.Text32 = "RECIPIENT NAME:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                      .ExportDisbursements.Item.QuickFinanceDisbursement.
                        RecipientName;

                    break;
                  case 3:
                    local.ReportLabel.Text32 = "DISBURSEMENT DATE:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                      (DateToInt(
                        local.ExportDisbursements.Item.QuickFinanceDisbursement.
                        Date), 15);

                    break;
                  case 4:
                    local.ReportLabel.Text32 = "DISBURSEMENT AMOUNT:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                      ((long)local.ExportDisbursements.Item.
                        QuickFinanceDisbursement.Amount, 15);

                    break;
                  case 5:
                    local.ReportLabel.Text32 =
                      "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                      .ReportLabel.Text32;

                    break;
                  default:
                    break;
                }

                UseCabControlReport3();

                if (!Equal(local.Report.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }
              }
            }

            local.ExportDisbursements.CheckIndex();
            local.ExportPayments.Index = 0;

            for(var limit = local.ExportPayments.Count; local
              .ExportPayments.Index < limit; ++local.ExportPayments.Index)
            {
              if (!local.ExportPayments.CheckSize())
              {
                break;
              }

              for(local.Common.Count = 1; local.Common.Count <= 4; ++
                local.Common.Count)
              {
                switch(local.Common.Count)
                {
                  case 0:
                    break;
                  case 1:
                    local.ReportLabel.Text32 = "PAYMENT DATE:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                      (DateToInt(
                        local.ExportPayments.Item.QuickFinancePayment.Date),
                      15);

                    break;
                  case 2:
                    local.ReportLabel.Text32 = "PAYMENT AMOUNT:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + NumberToString
                      ((long)local.ExportPayments.Item.QuickFinancePayment.
                        Amount, 15);

                    break;
                  case 3:
                    local.ReportLabel.Text32 = "SOURCE CODE:";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                      .ExportPayments.Item.QuickFinancePayment.SourceCode;

                    break;
                  case 4:
                    local.ReportLabel.Text32 = "//////////////////////////////";
                    local.ReportWrite.RptDetail = local.ReportLabel.Text32 + local
                      .ReportLabel.Text32;

                    break;
                  default:
                    break;
                }

                UseCabControlReport3();

                if (!Equal(local.Report.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }
              }
            }

            local.ExportPayments.CheckIndex();
          }
          else
          {
            for(local.Common.Count = 1; local.Common.Count <= 3; ++
              local.Common.Count)
            {
              switch(local.Common.Count)
              {
                case 1:
                  local.ReportWrite.RptDetail = "INPUT CASE NUMBER: " + local
                    .QuickInQuery.CaseId;

                  break;
                case 2:
                  local.ReportWrite.RptDetail = "FILLED INPUT CASE NUMBER" + local
                    .QuickInQuery.CaseId;

                  break;
                case 3:
                  if (IsExitState("CASE_NF"))
                  {
                    local.ReportWrite.RptDetail = "CASE NOT FOUND.";
                  }
                  else if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
                  {
                    local.ReportWrite.RptDetail =
                      "Disclosure prohibited on the case requested.";
                  }
                  else
                  {
                    local.ReportWrite.RptDetail = "ERROR.";
                  }

                  break;
                default:
                  break;
              }

              UseCabControlReport3();

              if (!Equal(local.Report.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
          }

          local.ReportWrite.RptDetail =
            "**************************************************";

          // Write record to control report
          UseCabControlReport3();

          if (!Equal(local.Report.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          break;
        case "EOF":
          local.MoreData.Flag = "N";

          goto AfterCycle;
        default:
          ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

          return;
      }
    }

AfterCycle:

    // Close input file
    local.Input.Action = "CLOSE";
    UseSiEabProcessQuickInputFile1();

    if (!Equal(local.Input.Status, "OK"))
    {
      ExitState = "ACO_RC_AB0002_ERROR_CLOSING_FILE";

      return;
    }

    // Close control report
    local.Report.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.Report.Status, "OK"))
    {
      ExitState = "ACO_RC_AB0002_ERROR_CLOSING_FILE";
    }
  }

  private static void MoveDisbursements(SiQuickFinancial.Export.
    DisbursementsGroup source, Local.ExportDisbursementsGroup target)
  {
    target.QuickFinanceDisbursement.Assign(source.QuickFinanceDisbursement);
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MovePayments(SiQuickFinancial.Export.PaymentsGroup source,
    Local.ExportPaymentsGroup target)
  {
    target.QuickFinancePayment.Assign(source.QuickFinancePayment);
  }

  private static void MoveQuickInQuery(QuickInQuery source, QuickInQuery target)
  {
    target.EndDate = source.EndDate;
    target.StDate = source.StDate;
    target.CaseId = source.CaseId;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Report.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Report.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Report.Action;
    MoveEabReportSend(local.ReportOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.Report.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Report.Action;
    useImport.NeededToWrite.RptDetail = local.ReportWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Report.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiEabProcessQuickInputFile1()
  {
    var useImport = new SiEabProcessQuickInputFile.Import();
    var useExport = new SiEabProcessQuickInputFile.Export();

    useImport.EabFileHandling.Action = local.Input.Action;
    useExport.EabFileHandling.Status = local.Input.Status;

    Call(SiEabProcessQuickInputFile.Execute, useImport, useExport);

    local.Input.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiEabProcessQuickInputFile2()
  {
    var useImport = new SiEabProcessQuickInputFile.Import();
    var useExport = new SiEabProcessQuickInputFile.Export();

    useImport.EabFileHandling.Action = local.Input.Action;
    MoveQuickInQuery(local.QuickInQuery, useExport.QuickInQuery);
    useExport.EabFileHandling.Status = local.Input.Status;

    Call(SiEabProcessQuickInputFile.Execute, useImport, useExport);

    local.QuickInQuery.Assign(useExport.QuickInQuery);
    local.Input.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiQuickFinancial()
  {
    var useImport = new SiQuickFinancial.Import();
    var useExport = new SiQuickFinancial.Export();

    useImport.QuickInQuery.Assign(local.QuickInQuery);

    Call(SiQuickFinancial.Execute, useImport, useExport);

    local.QuickCpHeader.Assign(useExport.QuickCpHeader);
    local.QuickFinanceSummary.Assign(useExport.QuickFinanceSummary);
    useExport.Payments.CopyTo(local.ExportPayments, MovePayments);
    useExport.Disbursements.
      CopyTo(local.ExportDisbursements, MoveDisbursements);
    local.Case1.Number = useExport.Case1.Number;
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
    /// <summary>A ExportPaymentsGroup group.</summary>
    [Serializable]
    public class ExportPaymentsGroup
    {
      /// <summary>
      /// A value of QuickFinancePayment.
      /// </summary>
      [JsonPropertyName("quickFinancePayment")]
      public QuickFinancePayment QuickFinancePayment
      {
        get => quickFinancePayment ??= new();
        set => quickFinancePayment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private QuickFinancePayment quickFinancePayment;
    }

    /// <summary>A ExportDisbursementsGroup group.</summary>
    [Serializable]
    public class ExportDisbursementsGroup
    {
      /// <summary>
      /// A value of QuickFinanceDisbursement.
      /// </summary>
      [JsonPropertyName("quickFinanceDisbursement")]
      public QuickFinanceDisbursement QuickFinanceDisbursement
      {
        get => quickFinanceDisbursement ??= new();
        set => quickFinanceDisbursement = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private QuickFinanceDisbursement quickFinanceDisbursement;
    }

    /// <summary>
    /// A value of Sp.
    /// </summary>
    [JsonPropertyName("sp")]
    public Common Sp
    {
      get => sp ??= new();
      set => sp = value;
    }

    /// <summary>
    /// A value of In1.
    /// </summary>
    [JsonPropertyName("in1")]
    public DateWorkArea In1
    {
      get => in1 ??= new();
      set => in1 = value;
    }

    /// <summary>
    /// A value of ReportLabel.
    /// </summary>
    [JsonPropertyName("reportLabel")]
    public WorkArea ReportLabel
    {
      get => reportLabel ??= new();
      set => reportLabel = value;
    }

    /// <summary>
    /// A value of ReportValue.
    /// </summary>
    [JsonPropertyName("reportValue")]
    public WorkArea ReportValue
    {
      get => reportValue ??= new();
      set => reportValue = value;
    }

    /// <summary>
    /// A value of Report.
    /// </summary>
    [JsonPropertyName("report")]
    public EabFileHandling Report
    {
      get => report ??= new();
      set => report = value;
    }

    /// <summary>
    /// A value of ReportOpen.
    /// </summary>
    [JsonPropertyName("reportOpen")]
    public EabReportSend ReportOpen
    {
      get => reportOpen ??= new();
      set => reportOpen = value;
    }

    /// <summary>
    /// A value of Input.
    /// </summary>
    [JsonPropertyName("input")]
    public EabFileHandling Input
    {
      get => input ??= new();
      set => input = value;
    }

    /// <summary>
    /// A value of MoreData.
    /// </summary>
    [JsonPropertyName("moreData")]
    public Common MoreData
    {
      get => moreData ??= new();
      set => moreData = value;
    }

    /// <summary>
    /// A value of QuickInQuery.
    /// </summary>
    [JsonPropertyName("quickInQuery")]
    public QuickInQuery QuickInQuery
    {
      get => quickInQuery ??= new();
      set => quickInQuery = value;
    }

    /// <summary>
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
    }

    /// <summary>
    /// A value of QuickFinanceSummary.
    /// </summary>
    [JsonPropertyName("quickFinanceSummary")]
    public QuickFinanceSummary QuickFinanceSummary
    {
      get => quickFinanceSummary ??= new();
      set => quickFinanceSummary = value;
    }

    /// <summary>
    /// Gets a value of ExportPayments.
    /// </summary>
    [JsonIgnore]
    public Array<ExportPaymentsGroup> ExportPayments => exportPayments ??= new(
      ExportPaymentsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ExportPayments for json serialization.
    /// </summary>
    [JsonPropertyName("exportPayments")]
    [Computed]
    public IList<ExportPaymentsGroup> ExportPayments_Json
    {
      get => exportPayments;
      set => ExportPayments.Assign(value);
    }

    /// <summary>
    /// Gets a value of ExportDisbursements.
    /// </summary>
    [JsonIgnore]
    public Array<ExportDisbursementsGroup> ExportDisbursements =>
      exportDisbursements ??= new(ExportDisbursementsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ExportDisbursements for json serialization.
    /// </summary>
    [JsonPropertyName("exportDisbursements")]
    [Computed]
    public IList<ExportDisbursementsGroup> ExportDisbursements_Json
    {
      get => exportDisbursements;
      set => ExportDisbursements.Assign(value);
    }

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
    /// A value of ReportWrite.
    /// </summary>
    [JsonPropertyName("reportWrite")]
    public EabReportSend ReportWrite
    {
      get => reportWrite ??= new();
      set => reportWrite = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common sp;
    private DateWorkArea in1;
    private WorkArea reportLabel;
    private WorkArea reportValue;
    private EabFileHandling report;
    private EabReportSend reportOpen;
    private EabFileHandling input;
    private Common moreData;
    private QuickInQuery quickInQuery;
    private QuickCpHeader quickCpHeader;
    private QuickFinanceSummary quickFinanceSummary;
    private Array<ExportPaymentsGroup> exportPayments;
    private Array<ExportDisbursementsGroup> exportDisbursements;
    private Case1 case1;
    private EabReportSend reportWrite;
    private Common common;
  }
#endregion
}
