// Program: FN_BF12_UPDATE_MO_OBLIGEE_SUM, ID: 373335003, model: 746.
// Short name: SWE02731
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF12_UPDATE_MO_OBLIGEE_SUM.
/// </summary>
[Serializable]
public partial class FnBf12UpdateMoObligeeSum: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF12_UPDATE_MO_OBLIGEE_SUM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf12UpdateMoObligeeSum(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf12UpdateMoObligeeSum.
  /// </summary>
  public FnBf12UpdateMoObligeeSum(IContext context, Import import, Export export)
    :
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
    // 2001-03-05  WR 000235  Fangman - New AB to update or create the monthly 
    // obligee summary.
    // ***************************************************
    // Set the "process date" recapture fields index.
    // Convert process date to an index.  index = (process year - 1977) * 12 + 
    // process month
    local.ProcessDtIndex.Number4 =
      (import.MonthlyObligeeSummary.Year - 1977) * 12 + import
      .MonthlyObligeeSummary.Month;

    if (local.ProcessDtIndex.Number4 > Export.ProcessDtTblGroup.Capacity)
    {
      ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

      return;
    }

    export.ProcessDtTbl.Index = local.ProcessDtIndex.Number4 - 1;
    export.ProcessDtTbl.CheckSize();

    export.ProcessDtTbl.Update.MoSumTblUpdatedInd.Flag = "Y";

    if (ReadObligee())
    {
      // Continue
    }
    else
    {
      ExitState = "FN0000_OBLIGEE_CSE_PERSON_NF";

      return;
    }

    if (ReadMonthlyObligeeSummary())
    {
      if (AsChar(import.Test.TestDisplayInd.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "updating Mo Obligee Sum row.";
        UseCabControlReport();
      }

      if (!Equal(entities.MonthlyObligeeSummary.CollectionsAmount,
        import.MonthlyObligeeSummary.CollectionsAmount.GetValueOrDefault()) || !
        Equal(entities.MonthlyObligeeSummary.CollectionsDisbursedToAr,
        import.MonthlyObligeeSummary.CollectionsDisbursedToAr.
          GetValueOrDefault()) || !
        Equal(entities.MonthlyObligeeSummary.FeeAmount,
        import.MonthlyObligeeSummary.FeeAmount.GetValueOrDefault()) || entities
        .MonthlyObligeeSummary.PassthruAmount != import
        .MonthlyObligeeSummary.PassthruAmount || !
        Equal(entities.MonthlyObligeeSummary.TotExcessUraAmt,
        import.MonthlyObligeeSummary.TotExcessUraAmt.GetValueOrDefault()))
      {
        ++import.CountsAndAmounts.NbrOfRowsNotMatching.Count;

        if (AsChar(import.Test.TestDisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "The new calculated total does not match the existing total below:";
            
          UseCabControlReport();
          UseFnBf12PrintMoObligeeSum();
        }
      }

      if (import.MonthlyObligeeSummary.CollectionsAmount.GetValueOrDefault() < 0
        || import
        .MonthlyObligeeSummary.AdcReimbursedAmount.GetValueOrDefault() < 0 || import
        .MonthlyObligeeSummary.CollectionsDisbursedToAr.GetValueOrDefault() < 0
        || import
        .MonthlyObligeeSummary.DisbursementsSuppressed.GetValueOrDefault() < 0
        || import.MonthlyObligeeSummary.FeeAmount.GetValueOrDefault() < 0 || import
        .MonthlyObligeeSummary.PassthruAmount < 0 || import
        .MonthlyObligeeSummary.RecapturedAmt.GetValueOrDefault() < 0 || import
        .MonthlyObligeeSummary.TotExcessUraAmt.GetValueOrDefault() < 0)
      {
        ++import.CountsAndAmounts.NbrOfRowsWithNegNbr.Count;

        if (AsChar(import.Test.TestDisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "The new calculated totals contain a negative number.";
          UseCabControlReport();
          UseFnBf12PrintMoObligeeSum();
        }
      }

      try
      {
        UpdateMonthlyObligeeSummary();

        // Continue
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_MTH_OBLIGEE_SUMMARY_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_MTH_OBLIGEE_SUM_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ++import.CountsAndAmounts.NbrOfMoSumRowsUpdated.Count;
    }
    else
    {
      if (AsChar(import.Test.TestRunInd.Flag) == 'N')
      {
        try
        {
          CreateMonthlyObligeeSummary();

          // Continue
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_MTH_OBLIGEE_SUM_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_MTH_OBLIGEE_SUM_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (AsChar(import.Test.TestDisplayInd.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "creating Mo Obligee Sum row.";
        UseCabControlReport();
      }

      ++import.CountsAndAmounts.NbrOfMoSumRowsCreated.Count;
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Warning:  AR " + import
        .Obligee.Number + " " + NumberToString
        (import.MonthlyObligeeSummary.Month, 14, 2) + "/" + NumberToString
        (import.MonthlyObligeeSummary.Year, 14, 2) + "  Monthly summary row did not already exist - had to do a create.";
        
      UseCabErrorReport();
    }

    import.CountsAndAmounts.NbrOfColl.Count += import.MonthlyObligeeSummary.
      NumberOfCollections.GetValueOrDefault();
    import.CountsAndAmounts.AmtOfColl.TotalCurrency += import.
      MonthlyObligeeSummary.CollectionsAmount.GetValueOrDefault();
    import.CountsAndAmounts.AmtOfAf.TotalCurrency += import.
      MonthlyObligeeSummary.AdcReimbursedAmount.GetValueOrDefault();
    import.CountsAndAmounts.AmtOfNa.TotalCurrency += import.
      MonthlyObligeeSummary.CollectionsDisbursedToAr.GetValueOrDefault();
    import.CountsAndAmounts.AmtOfFees.TotalCurrency += import.
      MonthlyObligeeSummary.FeeAmount.GetValueOrDefault();
    import.CountsAndAmounts.AmtOfSuppr.TotalCurrency += import.
      MonthlyObligeeSummary.DisbursementsSuppressed.GetValueOrDefault();
    import.CountsAndAmounts.AmtOfRecap.TotalCurrency += import.
      MonthlyObligeeSummary.RecapturedAmt.GetValueOrDefault();
    import.CountsAndAmounts.AmtOfPt.TotalCurrency += import.
      MonthlyObligeeSummary.PassthruAmount;
    import.CountsAndAmounts.AmtOfXUra.TotalCurrency += import.
      MonthlyObligeeSummary.TotExcessUraAmt.GetValueOrDefault();

    if (AsChar(import.Test.TestDisplayInd.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "";
      UseCabControlReport();
    }
  }

  private static void MoveMonthlyObligeeSummary(MonthlyObligeeSummary source,
    MonthlyObligeeSummary target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
    target.DisbursementsSuppressed = source.DisbursementsSuppressed;
    target.RecapturedAmt = source.RecapturedAmt;
    target.PassthruAmount = source.PassthruAmount;
    target.CollectionsAmount = source.CollectionsAmount;
    target.CollectionsDisbursedToAr = source.CollectionsDisbursedToAr;
    target.FeeAmount = source.FeeAmount;
    target.AdcReimbursedAmount = source.AdcReimbursedAmount;
    target.TotExcessUraAmt = source.TotExcessUraAmt;
    target.NumberOfCollections = source.NumberOfCollections;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnBf12PrintMoObligeeSum()
  {
    var useImport = new FnBf12PrintMoObligeeSum.Import();
    var useExport = new FnBf12PrintMoObligeeSum.Export();

    MoveMonthlyObligeeSummary(entities.MonthlyObligeeSummary,
      useImport.MonthlyObligeeSummary);

    Call(FnBf12PrintMoObligeeSum.Execute, useImport, useExport);
  }

  private void CreateMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee2.Populated);

    var year = import.MonthlyObligeeSummary.Year;
    var month = import.MonthlyObligeeSummary.Month;
    var passthruRecapAmt =
      export.ProcessDtTbl.Item.MonthlyObligeeSummary.PassthruRecapAmt.
        GetValueOrDefault();
    var disbursementsSuppressed =
      import.MonthlyObligeeSummary.DisbursementsSuppressed.GetValueOrDefault();
    var recapturedAmt =
      import.MonthlyObligeeSummary.RecapturedAmt.GetValueOrDefault();
    var naArrearsRecapAmt =
      export.ProcessDtTbl.Item.MonthlyObligeeSummary.NaArrearsRecapAmt.
        GetValueOrDefault();
    var passthruAmount = import.MonthlyObligeeSummary.PassthruAmount;
    var createdBy = global.UserId;
    var createdTimestamp = import.DateWorkArea.Timestamp;
    var adcReimbursedAmount =
      import.MonthlyObligeeSummary.AdcReimbursedAmount.GetValueOrDefault();
    var cpaSType = entities.Obligee2.Type1;
    var cspSNumber = entities.Obligee2.CspNumber;
    var collectionsAmount =
      import.MonthlyObligeeSummary.CollectionsAmount.GetValueOrDefault();
    var collectionsDisbursedToAr =
      import.MonthlyObligeeSummary.CollectionsDisbursedToAr.GetValueOrDefault();
      
    var feeAmount = import.MonthlyObligeeSummary.FeeAmount.GetValueOrDefault();
    var totExcessUraAmt =
      import.MonthlyObligeeSummary.TotExcessUraAmt.GetValueOrDefault();
    var numberOfCollections =
      import.MonthlyObligeeSummary.NumberOfCollections.GetValueOrDefault();
    var naCurrRecapAmt =
      export.ProcessDtTbl.Item.MonthlyObligeeSummary.NaCurrRecapAmt.
        GetValueOrDefault();

    CheckValid<MonthlyObligeeSummary>("CpaSType", cpaSType);
    entities.MonthlyObligeeSummary.Populated = false;
    Update("CreateMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetInt32(command, "yer", year);
        db.SetInt32(command, "mnth", month);
        db.SetNullableDecimal(command, "ptRecapAmt", passthruRecapAmt);
        db.SetNullableDecimal(
          command, "disbursementsSupp", disbursementsSuppressed);
        db.SetNullableDecimal(command, "recapturedAmt", recapturedAmt);
        db.SetNullableDecimal(command, "naArsRecapAmt", naArrearsRecapAmt);
        db.SetDecimal(command, "passthruAmount", passthruAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableDecimal(command, "adcReimbursedAmt", adcReimbursedAmount);
        db.SetString(command, "cpaSType", cpaSType);
        db.SetString(command, "cspSNumber", cspSNumber);
        db.SetNullableDecimal(command, "collectionAmt", collectionsAmount);
        db.SetNullableDecimal(
          command, "collctnDsbToAr", collectionsDisbursedToAr);
        db.SetNullableDecimal(command, "feeAmt", feeAmount);
        db.SetNullableString(command, "zdelType", "");
        db.SetNullableDecimal(command, "totExcessUraAmt", totExcessUraAmt);
        db.SetNullableInt32(command, "nbrOfCollections", numberOfCollections);
        db.SetNullableDecimal(command, "naCurrRecapAmt", naCurrRecapAmt);
      });

    entities.MonthlyObligeeSummary.Year = year;
    entities.MonthlyObligeeSummary.Month = month;
    entities.MonthlyObligeeSummary.PassthruRecapAmt = passthruRecapAmt;
    entities.MonthlyObligeeSummary.DisbursementsSuppressed =
      disbursementsSuppressed;
    entities.MonthlyObligeeSummary.RecapturedAmt = recapturedAmt;
    entities.MonthlyObligeeSummary.NaArrearsRecapAmt = naArrearsRecapAmt;
    entities.MonthlyObligeeSummary.PassthruAmount = passthruAmount;
    entities.MonthlyObligeeSummary.CreatedBy = createdBy;
    entities.MonthlyObligeeSummary.CreatedTimestamp = createdTimestamp;
    entities.MonthlyObligeeSummary.LastUpdatedBy = createdBy;
    entities.MonthlyObligeeSummary.LastUpdatedTmst = createdTimestamp;
    entities.MonthlyObligeeSummary.AdcReimbursedAmount = adcReimbursedAmount;
    entities.MonthlyObligeeSummary.CpaSType = cpaSType;
    entities.MonthlyObligeeSummary.CspSNumber = cspSNumber;
    entities.MonthlyObligeeSummary.CollectionsAmount = collectionsAmount;
    entities.MonthlyObligeeSummary.CollectionsDisbursedToAr =
      collectionsDisbursedToAr;
    entities.MonthlyObligeeSummary.FeeAmount = feeAmount;
    entities.MonthlyObligeeSummary.TotExcessUraAmt = totExcessUraAmt;
    entities.MonthlyObligeeSummary.NumberOfCollections = numberOfCollections;
    entities.MonthlyObligeeSummary.NaCurrRecapAmt = naCurrRecapAmt;
    entities.MonthlyObligeeSummary.Populated = true;
  }

  private bool ReadMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee2.Populated);
    entities.MonthlyObligeeSummary.Populated = false;

    return Read("ReadMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetString(command, "cpaSType", entities.Obligee2.Type1);
        db.SetString(command, "cspSNumber", entities.Obligee2.CspNumber);
        db.SetInt32(command, "yer", import.MonthlyObligeeSummary.Year);
        db.SetInt32(command, "mnth", import.MonthlyObligeeSummary.Month);
      },
      (db, reader) =>
      {
        entities.MonthlyObligeeSummary.Year = db.GetInt32(reader, 0);
        entities.MonthlyObligeeSummary.Month = db.GetInt32(reader, 1);
        entities.MonthlyObligeeSummary.PassthruRecapAmt =
          db.GetNullableDecimal(reader, 2);
        entities.MonthlyObligeeSummary.DisbursementsSuppressed =
          db.GetNullableDecimal(reader, 3);
        entities.MonthlyObligeeSummary.RecapturedAmt =
          db.GetNullableDecimal(reader, 4);
        entities.MonthlyObligeeSummary.NaArrearsRecapAmt =
          db.GetNullableDecimal(reader, 5);
        entities.MonthlyObligeeSummary.PassthruAmount =
          db.GetDecimal(reader, 6);
        entities.MonthlyObligeeSummary.CreatedBy = db.GetString(reader, 7);
        entities.MonthlyObligeeSummary.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.MonthlyObligeeSummary.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.MonthlyObligeeSummary.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 10);
        entities.MonthlyObligeeSummary.AdcReimbursedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.MonthlyObligeeSummary.CpaSType = db.GetString(reader, 12);
        entities.MonthlyObligeeSummary.CspSNumber = db.GetString(reader, 13);
        entities.MonthlyObligeeSummary.CollectionsAmount =
          db.GetNullableDecimal(reader, 14);
        entities.MonthlyObligeeSummary.CollectionsDisbursedToAr =
          db.GetNullableDecimal(reader, 15);
        entities.MonthlyObligeeSummary.FeeAmount =
          db.GetNullableDecimal(reader, 16);
        entities.MonthlyObligeeSummary.TotExcessUraAmt =
          db.GetNullableDecimal(reader, 17);
        entities.MonthlyObligeeSummary.NumberOfCollections =
          db.GetNullableInt32(reader, 18);
        entities.MonthlyObligeeSummary.NaCurrRecapAmt =
          db.GetNullableDecimal(reader, 19);
        entities.MonthlyObligeeSummary.Populated = true;
      });
  }

  private bool ReadObligee()
  {
    entities.Obligee2.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee2.CspNumber = db.GetString(reader, 0);
        entities.Obligee2.Type1 = db.GetString(reader, 1);
        entities.Obligee2.Populated = true;
      });
  }

  private void UpdateMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.MonthlyObligeeSummary.Populated);

    var passthruRecapAmt =
      export.ProcessDtTbl.Item.MonthlyObligeeSummary.PassthruRecapAmt.
        GetValueOrDefault();
    var disbursementsSuppressed =
      import.MonthlyObligeeSummary.DisbursementsSuppressed.GetValueOrDefault();
    var recapturedAmt =
      import.MonthlyObligeeSummary.RecapturedAmt.GetValueOrDefault();
    var naArrearsRecapAmt =
      export.ProcessDtTbl.Item.MonthlyObligeeSummary.NaArrearsRecapAmt.
        GetValueOrDefault();
    var passthruAmount = import.MonthlyObligeeSummary.PassthruAmount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = import.DateWorkArea.Timestamp;
    var adcReimbursedAmount =
      import.MonthlyObligeeSummary.AdcReimbursedAmount.GetValueOrDefault();
    var collectionsAmount =
      import.MonthlyObligeeSummary.CollectionsAmount.GetValueOrDefault();
    var collectionsDisbursedToAr =
      import.MonthlyObligeeSummary.CollectionsDisbursedToAr.GetValueOrDefault();
      
    var feeAmount = import.MonthlyObligeeSummary.FeeAmount.GetValueOrDefault();
    var totExcessUraAmt =
      import.MonthlyObligeeSummary.TotExcessUraAmt.GetValueOrDefault();
    var numberOfCollections =
      import.MonthlyObligeeSummary.NumberOfCollections.GetValueOrDefault();
    var naCurrRecapAmt =
      export.ProcessDtTbl.Item.MonthlyObligeeSummary.NaCurrRecapAmt.
        GetValueOrDefault();

    entities.MonthlyObligeeSummary.Populated = false;
    Update("UpdateMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "ptRecapAmt", passthruRecapAmt);
        db.SetNullableDecimal(
          command, "disbursementsSupp", disbursementsSuppressed);
        db.SetNullableDecimal(command, "recapturedAmt", recapturedAmt);
        db.SetNullableDecimal(command, "naArsRecapAmt", naArrearsRecapAmt);
        db.SetDecimal(command, "passthruAmount", passthruAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "adcReimbursedAmt", adcReimbursedAmount);
        db.SetNullableDecimal(command, "collectionAmt", collectionsAmount);
        db.SetNullableDecimal(
          command, "collctnDsbToAr", collectionsDisbursedToAr);
        db.SetNullableDecimal(command, "feeAmt", feeAmount);
        db.SetNullableDecimal(command, "totExcessUraAmt", totExcessUraAmt);
        db.SetNullableInt32(command, "nbrOfCollections", numberOfCollections);
        db.SetNullableDecimal(command, "naCurrRecapAmt", naCurrRecapAmt);
        db.SetInt32(command, "yer", entities.MonthlyObligeeSummary.Year);
        db.SetInt32(command, "mnth", entities.MonthlyObligeeSummary.Month);
        db.SetString(
          command, "cpaSType", entities.MonthlyObligeeSummary.CpaSType);
        db.SetString(
          command, "cspSNumber", entities.MonthlyObligeeSummary.CspSNumber);
      });

    entities.MonthlyObligeeSummary.PassthruRecapAmt = passthruRecapAmt;
    entities.MonthlyObligeeSummary.DisbursementsSuppressed =
      disbursementsSuppressed;
    entities.MonthlyObligeeSummary.RecapturedAmt = recapturedAmt;
    entities.MonthlyObligeeSummary.NaArrearsRecapAmt = naArrearsRecapAmt;
    entities.MonthlyObligeeSummary.PassthruAmount = passthruAmount;
    entities.MonthlyObligeeSummary.LastUpdatedBy = lastUpdatedBy;
    entities.MonthlyObligeeSummary.LastUpdatedTmst = lastUpdatedTmst;
    entities.MonthlyObligeeSummary.AdcReimbursedAmount = adcReimbursedAmount;
    entities.MonthlyObligeeSummary.CollectionsAmount = collectionsAmount;
    entities.MonthlyObligeeSummary.CollectionsDisbursedToAr =
      collectionsDisbursedToAr;
    entities.MonthlyObligeeSummary.FeeAmount = feeAmount;
    entities.MonthlyObligeeSummary.TotExcessUraAmt = totExcessUraAmt;
    entities.MonthlyObligeeSummary.NumberOfCollections = numberOfCollections;
    entities.MonthlyObligeeSummary.NaCurrRecapAmt = naCurrRecapAmt;
    entities.MonthlyObligeeSummary.Populated = true;
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
    /// <summary>A CountsAndAmountsGroup group.</summary>
    [Serializable]
    public class CountsAndAmountsGroup
    {
      /// <summary>
      /// A value of NbrOfDisbRead.
      /// </summary>
      [JsonPropertyName("nbrOfDisbRead")]
      public Common NbrOfDisbRead
      {
        get => nbrOfDisbRead ??= new();
        set => nbrOfDisbRead = value;
      }

      /// <summary>
      /// A value of AmtOfDisbRead.
      /// </summary>
      [JsonPropertyName("amtOfDisbRead")]
      public Common AmtOfDisbRead
      {
        get => amtOfDisbRead ??= new();
        set => amtOfDisbRead = value;
      }

      /// <summary>
      /// A value of NbrOfArs.
      /// </summary>
      [JsonPropertyName("nbrOfArs")]
      public Common NbrOfArs
      {
        get => nbrOfArs ??= new();
        set => nbrOfArs = value;
      }

      /// <summary>
      /// A value of NbrOfMoSumRowsUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfMoSumRowsUpdated")]
      public Common NbrOfMoSumRowsUpdated
      {
        get => nbrOfMoSumRowsUpdated ??= new();
        set => nbrOfMoSumRowsUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfMoSumRowsCreated.
      /// </summary>
      [JsonPropertyName("nbrOfMoSumRowsCreated")]
      public Common NbrOfMoSumRowsCreated
      {
        get => nbrOfMoSumRowsCreated ??= new();
        set => nbrOfMoSumRowsCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoSumRowsDeleted.
      /// </summary>
      [JsonPropertyName("nbrOfMoSumRowsDeleted")]
      public Common NbrOfMoSumRowsDeleted
      {
        get => nbrOfMoSumRowsDeleted ??= new();
        set => nbrOfMoSumRowsDeleted = value;
      }

      /// <summary>
      /// A value of NbrOfRowsNotMatching.
      /// </summary>
      [JsonPropertyName("nbrOfRowsNotMatching")]
      public Common NbrOfRowsNotMatching
      {
        get => nbrOfRowsNotMatching ??= new();
        set => nbrOfRowsNotMatching = value;
      }

      /// <summary>
      /// A value of NbrOfRowsWithNegNbr.
      /// </summary>
      [JsonPropertyName("nbrOfRowsWithNegNbr")]
      public Common NbrOfRowsWithNegNbr
      {
        get => nbrOfRowsWithNegNbr ??= new();
        set => nbrOfRowsWithNegNbr = value;
      }

      /// <summary>
      /// A value of NbrOfColl.
      /// </summary>
      [JsonPropertyName("nbrOfColl")]
      public Common NbrOfColl
      {
        get => nbrOfColl ??= new();
        set => nbrOfColl = value;
      }

      /// <summary>
      /// A value of AmtOfColl.
      /// </summary>
      [JsonPropertyName("amtOfColl")]
      public Common AmtOfColl
      {
        get => amtOfColl ??= new();
        set => amtOfColl = value;
      }

      /// <summary>
      /// A value of AmtOfAf.
      /// </summary>
      [JsonPropertyName("amtOfAf")]
      public Common AmtOfAf
      {
        get => amtOfAf ??= new();
        set => amtOfAf = value;
      }

      /// <summary>
      /// A value of AmtOfNa.
      /// </summary>
      [JsonPropertyName("amtOfNa")]
      public Common AmtOfNa
      {
        get => amtOfNa ??= new();
        set => amtOfNa = value;
      }

      /// <summary>
      /// A value of AmtOfFees.
      /// </summary>
      [JsonPropertyName("amtOfFees")]
      public Common AmtOfFees
      {
        get => amtOfFees ??= new();
        set => amtOfFees = value;
      }

      /// <summary>
      /// A value of AmtOfSuppr.
      /// </summary>
      [JsonPropertyName("amtOfSuppr")]
      public Common AmtOfSuppr
      {
        get => amtOfSuppr ??= new();
        set => amtOfSuppr = value;
      }

      /// <summary>
      /// A value of AmtOfRecap.
      /// </summary>
      [JsonPropertyName("amtOfRecap")]
      public Common AmtOfRecap
      {
        get => amtOfRecap ??= new();
        set => amtOfRecap = value;
      }

      /// <summary>
      /// A value of AmtOfPt.
      /// </summary>
      [JsonPropertyName("amtOfPt")]
      public Common AmtOfPt
      {
        get => amtOfPt ??= new();
        set => amtOfPt = value;
      }

      /// <summary>
      /// A value of AmtOfXUra.
      /// </summary>
      [JsonPropertyName("amtOfXUra")]
      public Common AmtOfXUra
      {
        get => amtOfXUra ??= new();
        set => amtOfXUra = value;
      }

      private Common nbrOfDisbRead;
      private Common amtOfDisbRead;
      private Common nbrOfArs;
      private Common nbrOfMoSumRowsUpdated;
      private Common nbrOfMoSumRowsCreated;
      private Common nbrOfMoSumRowsDeleted;
      private Common nbrOfRowsNotMatching;
      private Common nbrOfRowsWithNegNbr;
      private Common nbrOfColl;
      private Common amtOfColl;
      private Common amtOfAf;
      private Common amtOfNa;
      private Common amtOfFees;
      private Common amtOfSuppr;
      private Common amtOfRecap;
      private Common amtOfPt;
      private Common amtOfXUra;
    }

    /// <summary>A TestGroup group.</summary>
    [Serializable]
    public class TestGroup
    {
      /// <summary>
      /// A value of TestRunInd.
      /// </summary>
      [JsonPropertyName("testRunInd")]
      public Common TestRunInd
      {
        get => testRunInd ??= new();
        set => testRunInd = value;
      }

      /// <summary>
      /// A value of TestDisplayInd.
      /// </summary>
      [JsonPropertyName("testDisplayInd")]
      public Common TestDisplayInd
      {
        get => testDisplayInd ??= new();
        set => testDisplayInd = value;
      }

      /// <summary>
      /// A value of TestFirstObligee.
      /// </summary>
      [JsonPropertyName("testFirstObligee")]
      public CsePerson TestFirstObligee
      {
        get => testFirstObligee ??= new();
        set => testFirstObligee = value;
      }

      /// <summary>
      /// A value of TestLastObligee.
      /// </summary>
      [JsonPropertyName("testLastObligee")]
      public CsePerson TestLastObligee
      {
        get => testLastObligee ??= new();
        set => testLastObligee = value;
      }

      private Common testRunInd;
      private Common testDisplayInd;
      private CsePerson testFirstObligee;
      private CsePerson testLastObligee;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
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
    /// Gets a value of CountsAndAmounts.
    /// </summary>
    [JsonPropertyName("countsAndAmounts")]
    public CountsAndAmountsGroup CountsAndAmounts
    {
      get => countsAndAmounts ?? (countsAndAmounts = new());
      set => countsAndAmounts = value;
    }

    /// <summary>
    /// Gets a value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public TestGroup Test
    {
      get => test ?? (test = new());
      set => test = value;
    }

    private CsePerson obligee;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private DateWorkArea dateWorkArea;
    private CountsAndAmountsGroup countsAndAmounts;
    private TestGroup test;
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

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of ProcessDtIndex.
    /// </summary>
    [JsonPropertyName("processDtIndex")]
    public NumericWorkSet ProcessDtIndex
    {
      get => processDtIndex ??= new();
      set => processDtIndex = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private NumericWorkSet processDtIndex;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePerson Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePersonAccount Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
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

    private CsePerson obligee1;
    private CsePersonAccount obligee2;
    private MonthlyObligeeSummary monthlyObligeeSummary;
  }
#endregion
}
