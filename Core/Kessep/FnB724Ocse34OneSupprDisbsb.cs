// Program: FN_B724_OCSE34_ONE_SUPPR_DISBSB, ID: 371227355, model: 746.
// Short name: SWEF724B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B724_OCSE34_ONE_SUPPR_DISBSB.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB724Ocse34OneSupprDisbsb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B724_OCSE34_ONE_SUPPR_DISBSB program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB724Ocse34OneSupprDisbsb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB724Ocse34OneSupprDisbsb.
  /// </summary>
  public FnB724Ocse34OneSupprDisbsb(IContext context, Import import,
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
    // 12/17/2004 CM JOHNSON   PR 224067       one time run for disbursements 
    // suppression aged in 7 catagories for the ocse34-a report.
    ExitState = "ACO_NN0000_ALL_OK";

    // open the control report for processing the report
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // --------------------------------------------------------------------------------------------
    // --get the suppressed fdso and calculate the aging using the suppressed 
    // disbursements for the FDSO processing.
    // --------------------------------------------------------------------------------------------
    local.ReportPeriodEndDate.Date = new DateTime(2004, 12, 30);

    // set dates for compare processing later
    local.Local2DaysDateWorkArea2.Date =
      AddDays(local.ReportPeriodEndDate.Date, -2);
    local.Local3Days.Date = AddDays(local.ReportPeriodEndDate.Date, -3);
    local.Local30Days.Date = AddDays(local.ReportPeriodEndDate.Date, -30);
    local.Local31Days.Date = AddDays(local.ReportPeriodEndDate.Date, -31);
    local.Local6Mos.Date = AddDays(local.ReportPeriodEndDate.Date, -182);
    local.Local6Mos1Day.Date = AddDays(local.ReportPeriodEndDate.Date, -183);
    local.Local1Year.Date = AddDays(local.ReportPeriodEndDate.Date, -365);
    local.Local1Year1Day.Date = AddDays(local.ReportPeriodEndDate.Date, -366);
    local.Local3Year.Date = AddDays(local.ReportPeriodEndDate.Date, -1095);
    local.Local3Year1Day.Date = AddDays(local.ReportPeriodEndDate.Date, -1096);
    local.Local5YearDateWorkArea.Date =
      AddDays(local.ReportPeriodEndDate.Date, -1825);
    local.Local5Year1DayDateWorkArea.Date =
      AddDays(local.ReportPeriodEndDate.Date, -1826);

    // get the disbursements for aging eliminating the fdso
    foreach(var item in ReadDisbursementTransactionCsePersonDisbursementStatusHistory())
      
    {
      if (ReadDisbSuppressionStatusHistory())
      {
        continue;

        // continue processing
      }

      if (ReadDisbursementTransactionRln())
      {
        // continue processing
      }
      else
      {
        continue;
      }

      ReadDisbursementTransaction();

      if (ReadDisbursementTransaction())
      {
        // continue processing
      }
      else
      {
        continue;
      }

      if (ReadCollectionType())
      {
        continue;

        // fdso is eliminated
      }
      else
      {
        // continue processing
      }

      // calculate the aging process by using the effective date from 2 day to 
      // over 5 years
      // aging process for 2 days
      if (Lt(local.Local2DaysDateWorkArea2.Date,
        entities.DisbursementStatusHistory.EffectiveDate))
      {
        local.Local2DaysCommon2.TotalCurrency += entities.Debit.Amount;
        ++local.Local2DaysCommon1.Count;
      }
      else
      {
        // aging process for 3days to 30
        if (!Lt(entities.DisbursementStatusHistory.EffectiveDate,
          local.Local30Days.Date))
        {
          local.Local3Day30DayCommon2.TotalCurrency += entities.Debit.Amount;
          ++local.Local3Day30DayCommon1.Count;
        }
        else
        {
          // aging process for 31 days to 6 mos
          if (!Lt(entities.DisbursementStatusHistory.EffectiveDate,
            local.Local6Mos.Date))
          {
            local.Local31Day6Mos.TotalCurrency += entities.Debit.Amount;
            ++local.Local31Days6Mos.Count;
          }
          else
          {
            // aging process for 6mos 1 day to 1 year
            if (!Lt(entities.DisbursementStatusHistory.EffectiveDate,
              local.Local1Year.Date))
            {
              local.Local6Mos1YearCommon2.TotalCurrency += entities.Debit.
                Amount;
              ++local.Local6Mos1YearCommon1.Count;
            }
            else
            {
              // aging process for 1year 1 day to 3 years
              if (!Lt(entities.DisbursementStatusHistory.EffectiveDate,
                local.Local3Year.Date))
              {
                local.Local1Year3YearCommon2.TotalCurrency += entities.Debit.
                  Amount;
                ++local.Local1Year3YearCommon1.Count;
              }
              else
              {
                // aging process for 3 year 1 day to 5 years
                if (!Lt(entities.DisbursementStatusHistory.EffectiveDate,
                  local.Local5YearDateWorkArea.Date))
                {
                  local.Local3Year5YearCommon2.TotalCurrency += entities.Debit.
                    Amount;
                  ++local.Local3Year5YearCommon1.Count;
                }
                else
                {
                  // aging process for over 5 years
                  local.Local5YearCommon.TotalCurrency += entities.Debit.Amount;
                  ++local.Local5Year1DayCommon.Count;
                }
              }
            }
          }
        }
      }
    }

    // write totals to the control report
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "Aging Report  for Ocse34-A";
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

    local.EabReportSend.RptDetail = "Suppressed disbursements";
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
    local.FinalTotal.TotalCurrency = local.Local2DaysCommon2.TotalCurrency;
    local.EabReportSend.RptDetail = "Up to 2 business days";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 3 days to 30 days
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local3Day30DayCommon2.TotalCurrency;
    local.EabReportSend.RptDetail = "Aging 3 days  to 30 days";
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
    local.EabReportSend.RptDetail = "Aging 31 day to 6 mos";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 6mos 1 day to 1 year
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local6Mos1YearCommon2.TotalCurrency;
    local.EabReportSend.RptDetail = "Aging 6mos 1 day to 1 year";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 1 year 1day to 3 years
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local1Year3YearCommon2.TotalCurrency;
    local.EabReportSend.RptDetail = "Aging 1 year 1 day to 3 years";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      ((long)local.FinalTotal.TotalCurrency, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // 3 years 1 day to 5 years
    local.FinalTotal.TotalCurrency = 0;
    local.FinalTotal.TotalCurrency = local.Local3Year5YearCommon2.TotalCurrency;
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
    local.FinalTotal.TotalCurrency = local.Local5YearCommon.TotalCurrency;
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

    // put out count totals
    local.EabReportSend.RptDetail = "aging 2 day counts" + NumberToString
      (local.Local2DaysCommon1.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "aging 3 to 30 counts" + NumberToString
      (local.Local3Day30DayCommon1.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "aging 31 days to 6 mos count" + NumberToString
      (local.Local31Days6Mos.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "aging 6 mos 1 day 1 year count" + NumberToString
      (local.Local6Mos1YearCommon1.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "aging 1 year 1 day 3 years count" + NumberToString
      (local.Local1Year3YearCommon1.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "aging 3 years 1 day 5 years" + NumberToString
      (local.Local3Year5YearCommon1.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "aging 5 years 1 day count" + NumberToString
      (local.Local5Year1DayCommon.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

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

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.
          SetInt32(command, "crdId", entities.Credit.CrdId.GetValueOrDefault());
          
        db.SetInt32(
          command, "crvIdentifier", entities.Credit.CrvId.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier", entities.Credit.CstId.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier", entities.Credit.CrtId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadDisbSuppressionStatusHistory()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "currentDate", date);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.Populated = true;
      });
  }

  private bool ReadDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbursementTransactionRln.Populated);
    entities.Credit.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransactionRln.DtrPGeneratedId);
        db.SetString(
          command, "cpaType", entities.DisbursementTransactionRln.CpaPType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransactionRln.CspPNumber);
          
      },
      (db, reader) =>
      {
        entities.Credit.CpaType = db.GetString(reader, 0);
        entities.Credit.CspNumber = db.GetString(reader, 1);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Credit.Type1 = db.GetString(reader, 3);
        entities.Credit.Amount = db.GetDecimal(reader, 4);
        entities.Credit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Credit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Credit.OtyId = db.GetNullableInt32(reader, 7);
        entities.Credit.OtrTypeDisb = db.GetNullableString(reader, 8);
        entities.Credit.OtrId = db.GetNullableInt32(reader, 9);
        entities.Credit.CpaTypeDisb = db.GetNullableString(reader, 10);
        entities.Credit.CspNumberDisb = db.GetNullableString(reader, 11);
        entities.Credit.ObgId = db.GetNullableInt32(reader, 12);
        entities.Credit.CrdId = db.GetNullableInt32(reader, 13);
        entities.Credit.CrvId = db.GetNullableInt32(reader, 14);
        entities.Credit.CstId = db.GetNullableInt32(reader, 15);
        entities.Credit.CrtId = db.GetNullableInt32(reader, 16);
        entities.Credit.ColId = db.GetNullableInt32(reader, 17);
        entities.Credit.ReferenceNumber = db.GetNullableString(reader, 18);
        entities.Credit.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadDisbursementTransactionCsePersonDisbursementStatusHistory()
  {
    entities.DisbursementStatusHistory.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Debit.Populated = false;

    return ReadEach(
      "ReadDisbursementTransactionCsePersonDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ReportPeriodEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debit.CpaType = db.GetString(reader, 0);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.Debit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.Debit.Type1 = db.GetString(reader, 3);
        entities.Debit.Amount = db.GetDecimal(reader, 4);
        entities.Debit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Debit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Debit.ReferenceNumber = db.GetNullableString(reader, 7);
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 8);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 10);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.DisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 12);
        entities.DisbursementStatusHistory.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Debit.Populated = true;

        return true;
      });
  }

  private bool ReadDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.DisbursementTransactionRln.Populated = false;

    return Read("ReadDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId", entities.Debit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debit.CpaType);
        db.SetString(command, "cspNumber", entities.Debit.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionRln.Description =
          db.GetNullableString(reader, 1);
        entities.DisbursementTransactionRln.CreatedBy = db.GetString(reader, 2);
        entities.DisbursementTransactionRln.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.DisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 4);
        entities.DisbursementTransactionRln.CspNumber = db.GetString(reader, 5);
        entities.DisbursementTransactionRln.CpaType = db.GetString(reader, 6);
        entities.DisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 7);
        entities.DisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 8);
        entities.DisbursementTransactionRln.CpaPType = db.GetString(reader, 9);
        entities.DisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 10);
        entities.DisbursementTransactionRln.Populated = true;
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
    /// A value of Local5Year1DayCommon.
    /// </summary>
    [JsonPropertyName("local5Year1DayCommon")]
    public Common Local5Year1DayCommon
    {
      get => local5Year1DayCommon ??= new();
      set => local5Year1DayCommon = value;
    }

    /// <summary>
    /// A value of Local3Year5YearCommon1.
    /// </summary>
    [JsonPropertyName("local3Year5YearCommon1")]
    public Common Local3Year5YearCommon1
    {
      get => local3Year5YearCommon1 ??= new();
      set => local3Year5YearCommon1 = value;
    }

    /// <summary>
    /// A value of Local1Year3YearCommon1.
    /// </summary>
    [JsonPropertyName("local1Year3YearCommon1")]
    public Common Local1Year3YearCommon1
    {
      get => local1Year3YearCommon1 ??= new();
      set => local1Year3YearCommon1 = value;
    }

    /// <summary>
    /// A value of Local6Mos1YearCommon1.
    /// </summary>
    [JsonPropertyName("local6Mos1YearCommon1")]
    public Common Local6Mos1YearCommon1
    {
      get => local6Mos1YearCommon1 ??= new();
      set => local6Mos1YearCommon1 = value;
    }

    /// <summary>
    /// A value of Local31Days6Mos.
    /// </summary>
    [JsonPropertyName("local31Days6Mos")]
    public Common Local31Days6Mos
    {
      get => local31Days6Mos ??= new();
      set => local31Days6Mos = value;
    }

    /// <summary>
    /// A value of Local3Day30DayCommon1.
    /// </summary>
    [JsonPropertyName("local3Day30DayCommon1")]
    public Common Local3Day30DayCommon1
    {
      get => local3Day30DayCommon1 ??= new();
      set => local3Day30DayCommon1 = value;
    }

    /// <summary>
    /// A value of Local2DaysCommon1.
    /// </summary>
    [JsonPropertyName("local2DaysCommon1")]
    public Common Local2DaysCommon1
    {
      get => local2DaysCommon1 ??= new();
      set => local2DaysCommon1 = value;
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
    /// A value of Local3Year1Day.
    /// </summary>
    [JsonPropertyName("local3Year1Day")]
    public DateWorkArea Local3Year1Day
    {
      get => local3Year1Day ??= new();
      set => local3Year1Day = value;
    }

    /// <summary>
    /// A value of Local5Year1DayDateWorkArea.
    /// </summary>
    [JsonPropertyName("local5Year1DayDateWorkArea")]
    public DateWorkArea Local5Year1DayDateWorkArea
    {
      get => local5Year1DayDateWorkArea ??= new();
      set => local5Year1DayDateWorkArea = value;
    }

    /// <summary>
    /// A value of Local5YearDateWorkArea.
    /// </summary>
    [JsonPropertyName("local5YearDateWorkArea")]
    public DateWorkArea Local5YearDateWorkArea
    {
      get => local5YearDateWorkArea ??= new();
      set => local5YearDateWorkArea = value;
    }

    /// <summary>
    /// A value of Local3Year.
    /// </summary>
    [JsonPropertyName("local3Year")]
    public DateWorkArea Local3Year
    {
      get => local3Year ??= new();
      set => local3Year = value;
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
    /// A value of Local2DaysDateWorkArea1.
    /// </summary>
    [JsonPropertyName("local2DaysDateWorkArea1")]
    public DateWorkArea Local2DaysDateWorkArea1
    {
      get => local2DaysDateWorkArea1 ??= new();
      set => local2DaysDateWorkArea1 = value;
    }

    /// <summary>
    /// A value of Local2DaysCommon2.
    /// </summary>
    [JsonPropertyName("local2DaysCommon2")]
    public Common Local2DaysCommon2
    {
      get => local2DaysCommon2 ??= new();
      set => local2DaysCommon2 = value;
    }

    /// <summary>
    /// A value of Local5YearCommon.
    /// </summary>
    [JsonPropertyName("local5YearCommon")]
    public Common Local5YearCommon
    {
      get => local5YearCommon ??= new();
      set => local5YearCommon = value;
    }

    /// <summary>
    /// A value of Local3Year5YearCommon2.
    /// </summary>
    [JsonPropertyName("local3Year5YearCommon2")]
    public Common Local3Year5YearCommon2
    {
      get => local3Year5YearCommon2 ??= new();
      set => local3Year5YearCommon2 = value;
    }

    /// <summary>
    /// A value of Local1Year3YearCommon2.
    /// </summary>
    [JsonPropertyName("local1Year3YearCommon2")]
    public Common Local1Year3YearCommon2
    {
      get => local1Year3YearCommon2 ??= new();
      set => local1Year3YearCommon2 = value;
    }

    /// <summary>
    /// A value of Local6Mos1YearCommon2.
    /// </summary>
    [JsonPropertyName("local6Mos1YearCommon2")]
    public Common Local6Mos1YearCommon2
    {
      get => local6Mos1YearCommon2 ??= new();
      set => local6Mos1YearCommon2 = value;
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
    /// A value of Local3Day30DayCommon2.
    /// </summary>
    [JsonPropertyName("local3Day30DayCommon2")]
    public Common Local3Day30DayCommon2
    {
      get => local3Day30DayCommon2 ??= new();
      set => local3Day30DayCommon2 = value;
    }

    /// <summary>
    /// A value of Created.
    /// </summary>
    [JsonPropertyName("created")]
    public DateWorkArea Created
    {
      get => created ??= new();
      set => created = value;
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
    /// A value of FileNumber.
    /// </summary>
    [JsonPropertyName("fileNumber")]
    public Common FileNumber
    {
      get => fileNumber ??= new();
      set => fileNumber = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of TotalSuppressedDisb.
    /// </summary>
    [JsonPropertyName("totalSuppressedDisb")]
    public Common TotalSuppressedDisb
    {
      get => totalSuppressedDisb ??= new();
      set => totalSuppressedDisb = value;
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
    /// A value of Local20990101.
    /// </summary>
    [JsonPropertyName("local20990101")]
    public DateWorkArea Local20990101
    {
      get => local20990101 ??= new();
      set => local20990101 = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of Suppression.
    /// </summary>
    [JsonPropertyName("suppression")]
    public Common Suppression
    {
      get => suppression ??= new();
      set => suppression = value;
    }

    /// <summary>
    /// A value of RestartLine.
    /// </summary>
    [JsonPropertyName("restartLine")]
    public TextWorkArea RestartLine
    {
      get => restartLine ??= new();
      set => restartLine = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Local2DaysDateWorkArea2.
    /// </summary>
    [JsonPropertyName("local2DaysDateWorkArea2")]
    public DateWorkArea Local2DaysDateWorkArea2
    {
      get => local2DaysDateWorkArea2 ??= new();
      set => local2DaysDateWorkArea2 = value;
    }

    private Common local5Year1DayCommon;
    private Common local3Year5YearCommon1;
    private Common local1Year3YearCommon1;
    private Common local6Mos1YearCommon1;
    private Common local31Days6Mos;
    private Common local3Day30DayCommon1;
    private Common local2DaysCommon1;
    private Common finalTotal;
    private DateWorkArea local3Year1Day;
    private DateWorkArea local5Year1DayDateWorkArea;
    private DateWorkArea local5YearDateWorkArea;
    private DateWorkArea local3Year;
    private DateWorkArea local1Year1Day;
    private DateWorkArea local1Year;
    private DateWorkArea local6Mos1Day;
    private DateWorkArea local6Mos;
    private DateWorkArea local31Days;
    private DateWorkArea local30Days;
    private DateWorkArea local3Days;
    private DateWorkArea local2DaysDateWorkArea1;
    private Common local2DaysCommon2;
    private Common local5YearCommon;
    private Common local3Year5YearCommon2;
    private Common local1Year3YearCommon2;
    private Common local6Mos1YearCommon2;
    private Common local31Day6Mos;
    private Common local3Day30DayCommon2;
    private DateWorkArea created;
    private DateWorkArea null1;
    private Common fileNumber;
    private External external;
    private DateWorkArea dateWorkArea;
    private Common totalSuppressedDisb;
    private TextWorkArea textWorkArea;
    private DateWorkArea local20990101;
    private CsePerson prev;
    private Common suppression;
    private TextWorkArea restartLine;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea reportPeriodEndDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea local2DaysDateWorkArea2;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

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
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
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

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePersonAccount obligee;
    private DisbursementStatus disbursementStatus;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction credit;
    private DisbursementStatusHistory disbursementStatusHistory;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private DisbursementTransaction debit;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
