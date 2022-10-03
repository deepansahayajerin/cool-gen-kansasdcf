// Program: FN_B725_OCSE34_ONE_DISB_CREDIT, ID: 371228522, model: 746.
// Short name: SWEF725B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B725_OCSE34_ONE_DISB_CREDIT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB725Ocse34OneDisbCredit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B725_OCSE34_ONE_DISB_CREDIT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB725Ocse34OneDisbCredit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB725Ocse34OneDisbCredit.
  /// </summary>
  public FnB725Ocse34OneDisbCredit(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // ----------------------------------------------
    // 12/17/2004 CM JOHNSON   PR 224067       one time run for getting 
    // disbursements credits. 7 catagories for the ocse34-a report.
    ExitState = "ACO_NN0000_ALL_OK";

    // open the control report for processing the report
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // get the disbursements credits for aging process
    // set the dates for processing
    local.ReportPeriodEndDate.Timestamp =
      AddMicroseconds(new DateTime(2004, 12, 30, 23, 59, 59), 999999);
    local.Null1.Date = new DateTime(1, 1, 1);
    local.ReportPeriodEndDate.Date = new DateTime(2004, 12, 30);

    // calculates the work areas for comparing the dates later.
    local.Local2DaysDateWorkArea.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -2);
    local.Local3Days.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -3);
    local.Local30Days.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -30);
    local.Local31Days.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -31);
    local.Local6Mos.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -182);
    local.Local6Mos1Day.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -183);
    local.Local1Year.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -365);
    local.Local1Year1Day.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -366);
    local.Local3Years.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -1095);
    local.Local3Year1Day.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -1096);
    local.Local5Years.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -1825);
    local.Local5Years1Day.Date =
      AddDays(Date(local.ReportPeriodEndDate.Timestamp), -1826);

    // read necessary data to process the distributions credits.
    foreach(var item in ReadCollectionCashReceiptDetailCashReceiptObligation())
    {
      if (!ReadObligationType())
      {
        // -- This shouldn't happen....skip this collection if not found.
        continue;
      }

      if (!ReadDebtDetail())
      {
        // -- This shouldn't happen....skip this collection if not found.
        continue;
      }

      if (!ReadCsePerson())
      {
        // -- This shouldn't happen....skip this collection if not found.
        continue;
      }

      // set the compare date for the created timestamp
      local.Compare.Date = Date(entities.Collection.CreatedTmst);

      // calculate the aging process from 2 days
      if (Lt(local.Local2DaysDateWorkArea.Date, local.Compare.Date))
      {
        local.Local2DaysCommon.TotalCurrency += entities.Collection.Amount;
        ++local.Local2DaysCommon.Count;
      }
      else
      {
        // calculate the aging process from 3 days to 30 days
        if (!Lt(local.Compare.Date, local.Local30Days.Date))
        {
          local.Local3Day30Day.TotalCurrency += entities.Collection.Amount;
          ++local.Local3Day30Day.Count;
        }
        else
        {
          // calculate the aging process from 31 days to  6 mos
          if (!Lt(local.Compare.Date, local.Local6Mos.Date))
          {
            local.Local31Day6Mos.TotalCurrency += entities.Collection.Amount;
            ++local.Local31Day6Mos.Count;
          }
          else
          {
            // calculate the aging process from 6mos 1 day to 1 year
            if (!Lt(local.Compare.Date, local.Local1Year.Date))
            {
              local.Local6Mos1Year.TotalCurrency += entities.Collection.Amount;
              ++local.Local6Mos1Year.Count;
            }
            else
            {
              // calculate the aging process from 1 year  1 day to 3 years
              if (!Lt(local.Compare.Date, local.Local3Years.Date))
              {
                local.Local1Year1Day3Year.TotalCurrency += entities.Collection.
                  Amount;
                ++local.Local1Year1Day3Year.Count;
              }
              else
              {
                // calculate the aging process from 3 years 1 day to 5 years
                if (!Lt(local.Compare.Date, local.Local5Years.Date))
                {
                  local.Local3Year1Day5Year.TotalCurrency += entities.
                    Collection.Amount;
                  ++local.Local3Year1Day5Year.Count;
                }
                else
                {
                  // calculate the aging process for over 5 years 1 day
                  local.Local5Year.TotalCurrency += entities.Collection.Amount;
                  ++local.Local5Year.Count;
                }
              }
            }
          }
        }
      }
    }

    // write totals to the control report
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // put in to start regular program processing
    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Disbursements Credits";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // 2 business days
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local2DaysCommon.TotalCurrency;
    local.EabReportSend.RptDetail = " Aging Up to 2 days Undistributed";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 2 days to 30 days
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local3Day30Day.TotalCurrency;
    local.EabReportSend.RptDetail = "Aging 3 days to 30 days";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 31 days to 6 mos
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local31Day6Mos.TotalCurrency;
    local.EabReportSend.RptDetail = "Aging 31 days to 6 mos";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 6mos 1 day  to 1 year
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local6Mos1Year.TotalCurrency;
    local.EabReportSend.RptDetail = "Aging 6 mos 1 day to 1 year";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 1 year 1 day to 3 years
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local1Year1Day3Year.TotalCurrency;
    local.EabReportSend.RptDetail = "Aging 1 year 1 day to 3 years";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 3 years 1 day  to 5 years
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local3Year1Day5Year.TotalCurrency;
    local.EabReportSend.RptDetail = "Aging 3 years 1 day to 5 years";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // over 5 years
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local5Year.TotalCurrency;
    local.EabReportSend.RptDetail = "Aging over 5 years";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // put out the counts
    local.EabReportSend.RptDetail = "Aging Counts 2days" + NumberToString
      (local.Local2DaysCommon.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // counts for 3 days to 30
    local.EabReportSend.RptDetail = "Aging 3 to 30 counts" + NumberToString
      (local.Local3Day30Day.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 31 day to 6 mos count
    local.EabReportSend.RptDetail = "31 days to 6 mos count" + NumberToString
      (local.Local31Day6Mos.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 6mos 1 day 1 year count
    local.EabReportSend.RptDetail = "6mos 1 day to 1 year count" + NumberToString
      (local.Local6Mos1Year.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 1 year 1 day to 3 years
    local.EabReportSend.RptDetail = "1 year 1 day to 3 years count" + NumberToString
      (local.Local1Year1Day3Year.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 3 years 1 day to 5 years
    local.EabReportSend.RptDetail = "3 years 1 day to 5 years count " + NumberToString
      (local.Local3Year1Day5Year.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // over 5 years count
    local.EabReportSend.RptDetail = "over 5 years 1 day count" + NumberToString
      (local.Local5Year.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Close the Control report.
    // -------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool>
    ReadCollectionCashReceiptDetailCashReceiptObligation()
  {
    entities.Collection.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.Obligation.Populated = false;
    entities.Obligor1.Populated = false;

    return ReadEach("ReadCollectionCashReceiptDetailCashReceiptObligation",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          local.ReportPeriodEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "disbDt1",
          local.ReportPeriodEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "disbDt2", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Obligation.CspNumber = db.GetString(reader, 10);
        entities.Obligor1.Number = db.GetString(reader, 10);
        entities.Obligor1.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Obligation.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 18);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 19);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 20);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 21);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 22);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 23);
        entities.Collection.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.Obligation.Populated = true;
        entities.Obligor1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Supported1.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Supported1.Number = db.GetString(reader, 0);
        entities.Supported1.Populated = true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otrGeneratedId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of FinalTotal.
    /// </summary>
    [JsonPropertyName("finalTotal")]
    public Common FinalTotal
    {
      get => finalTotal ??= new();
      set => finalTotal = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of DebtDetailDueDate.
    /// </summary>
    [JsonPropertyName("debtDetailDueDate")]
    public DateWorkArea DebtDetailDueDate
    {
      get => debtDetailDueDate ??= new();
      set => debtDetailDueDate = value;
    }

    /// <summary>
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public DateWorkArea Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    /// <summary>
    /// A value of Local5Years1Day.
    /// </summary>
    [JsonPropertyName("local5Years1Day")]
    public DateWorkArea Local5Years1Day
    {
      get => local5Years1Day ??= new();
      set => local5Years1Day = value;
    }

    /// <summary>
    /// A value of Local5Years.
    /// </summary>
    [JsonPropertyName("local5Years")]
    public DateWorkArea Local5Years
    {
      get => local5Years ??= new();
      set => local5Years = value;
    }

    /// <summary>
    /// A value of Local3Year1Day.
    /// </summary>
    [JsonPropertyName("local3Year1Day")]
    public DateWorkArea Local3Year1Day
    {
      get => local3Year1Day ??= new();
      set => local3Year1Day = value;
    }

    /// <summary>
    /// A value of Local3Years.
    /// </summary>
    [JsonPropertyName("local3Years")]
    public DateWorkArea Local3Years
    {
      get => local3Years ??= new();
      set => local3Years = value;
    }

    /// <summary>
    /// A value of Local1Year1Day.
    /// </summary>
    [JsonPropertyName("local1Year1Day")]
    public DateWorkArea Local1Year1Day
    {
      get => local1Year1Day ??= new();
      set => local1Year1Day = value;
    }

    /// <summary>
    /// A value of Local1Year.
    /// </summary>
    [JsonPropertyName("local1Year")]
    public DateWorkArea Local1Year
    {
      get => local1Year ??= new();
      set => local1Year = value;
    }

    /// <summary>
    /// A value of Local6Mos1Day.
    /// </summary>
    [JsonPropertyName("local6Mos1Day")]
    public DateWorkArea Local6Mos1Day
    {
      get => local6Mos1Day ??= new();
      set => local6Mos1Day = value;
    }

    /// <summary>
    /// A value of Local6Mos.
    /// </summary>
    [JsonPropertyName("local6Mos")]
    public DateWorkArea Local6Mos
    {
      get => local6Mos ??= new();
      set => local6Mos = value;
    }

    /// <summary>
    /// A value of Local31Days.
    /// </summary>
    [JsonPropertyName("local31Days")]
    public DateWorkArea Local31Days
    {
      get => local31Days ??= new();
      set => local31Days = value;
    }

    /// <summary>
    /// A value of Local30Days.
    /// </summary>
    [JsonPropertyName("local30Days")]
    public DateWorkArea Local30Days
    {
      get => local30Days ??= new();
      set => local30Days = value;
    }

    /// <summary>
    /// A value of Local3Days.
    /// </summary>
    [JsonPropertyName("local3Days")]
    public DateWorkArea Local3Days
    {
      get => local3Days ??= new();
      set => local3Days = value;
    }

    /// <summary>
    /// A value of Local2DaysDateWorkArea.
    /// </summary>
    [JsonPropertyName("local2DaysDateWorkArea")]
    public DateWorkArea Local2DaysDateWorkArea
    {
      get => local2DaysDateWorkArea ??= new();
      set => local2DaysDateWorkArea = value;
    }

    /// <summary>
    /// A value of Local2DaysCommon.
    /// </summary>
    [JsonPropertyName("local2DaysCommon")]
    public Common Local2DaysCommon
    {
      get => local2DaysCommon ??= new();
      set => local2DaysCommon = value;
    }

    /// <summary>
    /// A value of Local5Year.
    /// </summary>
    [JsonPropertyName("local5Year")]
    public Common Local5Year
    {
      get => local5Year ??= new();
      set => local5Year = value;
    }

    /// <summary>
    /// A value of Local3Year1Day5Year.
    /// </summary>
    [JsonPropertyName("local3Year1Day5Year")]
    public Common Local3Year1Day5Year
    {
      get => local3Year1Day5Year ??= new();
      set => local3Year1Day5Year = value;
    }

    /// <summary>
    /// A value of Local1Year1Day3Year.
    /// </summary>
    [JsonPropertyName("local1Year1Day3Year")]
    public Common Local1Year1Day3Year
    {
      get => local1Year1Day3Year ??= new();
      set => local1Year1Day3Year = value;
    }

    /// <summary>
    /// A value of Local6Mos1Year.
    /// </summary>
    [JsonPropertyName("local6Mos1Year")]
    public Common Local6Mos1Year
    {
      get => local6Mos1Year ??= new();
      set => local6Mos1Year = value;
    }

    /// <summary>
    /// A value of Local31Day6Mos.
    /// </summary>
    [JsonPropertyName("local31Day6Mos")]
    public Common Local31Day6Mos
    {
      get => local31Day6Mos ??= new();
      set => local31Day6Mos = value;
    }

    /// <summary>
    /// A value of Local3Day30Day.
    /// </summary>
    [JsonPropertyName("local3Day30Day")]
    public Common Local3Day30Day
    {
      get => local3Day30Day ??= new();
      set => local3Day30Day = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of ReportPeriodEndDate.
    /// </summary>
    [JsonPropertyName("reportPeriodEndDate")]
    public DateWorkArea ReportPeriodEndDate
    {
      get => reportPeriodEndDate ??= new();
      set => reportPeriodEndDate = value;
    }

    /// <summary>
    /// A value of TotalDisbursementCredit.
    /// </summary>
    [JsonPropertyName("totalDisbursementCredit")]
    public Common TotalDisbursementCredit
    {
      get => totalDisbursementCredit ??= new();
      set => totalDisbursementCredit = value;
    }

    private DateWorkArea collection;
    private Common finalTotal;
    private TextWorkArea textWorkArea;
    private DateWorkArea debtDetailDueDate;
    private DateWorkArea compare;
    private DateWorkArea local5Years1Day;
    private DateWorkArea local5Years;
    private DateWorkArea local3Year1Day;
    private DateWorkArea local3Years;
    private DateWorkArea local1Year1Day;
    private DateWorkArea local1Year;
    private DateWorkArea local6Mos1Day;
    private DateWorkArea local6Mos;
    private DateWorkArea local31Days;
    private DateWorkArea local30Days;
    private DateWorkArea local3Days;
    private DateWorkArea local2DaysDateWorkArea;
    private Common local2DaysCommon;
    private Common local5Year;
    private Common local3Year1Day5Year;
    private Common local1Year1Day3Year;
    private Common local6Mos1Year;
    private Common local31Day6Mos;
    private Common local3Day30Day;
    private DateWorkArea null1;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea reportPeriodEndDate;
    private Common totalDisbursementCredit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    private ObligationType obligationType;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private Obligation obligation;
    private CsePerson obligor1;
    private CashReceiptType cashReceiptType;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount obligor2;
    private DebtDetail debtDetail;
    private CsePerson supported1;
    private CsePersonAccount supported2;
  }
#endregion
}
