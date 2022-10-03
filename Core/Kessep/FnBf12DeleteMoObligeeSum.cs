// Program: FN_BF12_DELETE_MO_OBLIGEE_SUM, ID: 373333823, model: 746.
// Short name: SWE02730
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF12_DELETE_MO_OBLIGEE_SUM.
/// </summary>
[Serializable]
public partial class FnBf12DeleteMoObligeeSum: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF12_DELETE_MO_OBLIGEE_SUM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf12DeleteMoObligeeSum(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf12DeleteMoObligeeSum.
  /// </summary>
  public FnBf12DeleteMoObligeeSum(IContext context, Import import, Export export)
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
    // 2001-05-31  WR 000235  Fangman - New AB to look for and delete monthly 
    // obligee summary rows that do not have disbursement transactions to
    // support them.
    // 2002-01-27  WR 000235  Fangman - Added code to update the Recapture 
    // fields kept by process date.
    // ***************************************************
    for(export.ProcessDtTbl.Index = 0; export.ProcessDtTbl.Index < export
      .ProcessDtTbl.Count; ++export.ProcessDtTbl.Index)
    {
      if (!export.ProcessDtTbl.CheckSize())
      {
        break;
      }

      if (AsChar(export.ProcessDtTbl.Item.MoSumTblUpdatedInd.Flag) == 'N')
      {
        if (!entities.Obligee.Populated)
        {
          if (!ReadObligee())
          {
            ExitState = "FN0000_OBLIGEE_CSE_PERSON_NF";

            return;
          }
        }

        // Convert index to a year and month.
        local.ProcessDtIndex.Number4 = export.ProcessDtTbl.Index + 1;
        local.ProcessDtYear.Number4 = (local.ProcessDtIndex.Number4 - 1) / 12;
        local.ProcessDtMonth.Number2 = local.ProcessDtIndex.Number4 - local
          .ProcessDtYear.Number4 * 12;
        local.ProcessDtYear.Number4 += 1977;
        local.ForUpdate.Year = local.ProcessDtYear.Number4;
        local.ForUpdate.Month = local.ProcessDtMonth.Number2;

        if (ReadMonthlyObligeeSummary1())
        {
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
                ExitState = "FN0000_MTH_OBLIGEE_SUM_NU";

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
        else
        {
          try
          {
            CreateMonthlyObligeeSummary();
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
      }
    }

    export.ProcessDtTbl.CheckIndex();
    local.PersonNumberDisplayedInd.Flag = "N";

    foreach(var item in ReadMonthlyObligeeSummary2())
    {
      if (AsChar(import.Test.TestDisplayInd.Flag) == 'Y')
      {
        if (AsChar(local.PersonNumberDisplayedInd.Flag) == 'N')
        {
          local.PersonNumberDisplayedInd.Flag = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Deleting monthly obligee summary records for " + import
            .Obligee.Number;
          UseCabControlReport();
        }

        UseFnBf12PrintMoObligeeSum();
      }

      ++import.CountsAndAmounts.NbrOfMoSumRowsDeleted.Count;
      DeleteMonthlyObligeeSummary();
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
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);

    var year = local.ForUpdate.Year;
    var month = local.ForUpdate.Month;
    var passthruRecapAmt =
      export.ProcessDtTbl.Item.MonthlyObligeeSummary.PassthruRecapAmt.
        GetValueOrDefault();
    var disbursementsSuppressed = 0M;
    var naArrearsRecapAmt =
      export.ProcessDtTbl.Item.MonthlyObligeeSummary.NaArrearsRecapAmt.
        GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = import.DateWorkArea.Timestamp;
    var cpaSType = entities.Obligee.Type1;
    var cspSNumber = entities.Obligee.CspNumber;
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
        db.
          SetNullableDecimal(command, "recapturedAmt", disbursementsSuppressed);
          
        db.SetNullableDecimal(command, "naArsRecapAmt", naArrearsRecapAmt);
        db.SetDecimal(command, "passthruAmount", disbursementsSuppressed);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableDecimal(
          command, "adcReimbursedAmt", disbursementsSuppressed);
        db.SetString(command, "cpaSType", cpaSType);
        db.SetString(command, "cspSNumber", cspSNumber);
        db.
          SetNullableDecimal(command, "collectionAmt", disbursementsSuppressed);
          
        db.
          SetNullableDecimal(command, "collctnDsbToAr", disbursementsSuppressed);
          
        db.SetNullableDecimal(command, "feeAmt", disbursementsSuppressed);
        db.SetNullableString(command, "zdelType", "");
        db.SetNullableDecimal(
          command, "totExcessUraAmt", disbursementsSuppressed);
        db.SetNullableInt32(command, "nbrOfCollections", 0);
        db.SetNullableDecimal(command, "naCurrRecapAmt", naCurrRecapAmt);
      });

    entities.MonthlyObligeeSummary.Year = year;
    entities.MonthlyObligeeSummary.Month = month;
    entities.MonthlyObligeeSummary.PassthruRecapAmt = passthruRecapAmt;
    entities.MonthlyObligeeSummary.DisbursementsSuppressed =
      disbursementsSuppressed;
    entities.MonthlyObligeeSummary.RecapturedAmt = disbursementsSuppressed;
    entities.MonthlyObligeeSummary.NaArrearsRecapAmt = naArrearsRecapAmt;
    entities.MonthlyObligeeSummary.PassthruAmount = disbursementsSuppressed;
    entities.MonthlyObligeeSummary.CreatedBy = createdBy;
    entities.MonthlyObligeeSummary.CreatedTimestamp = createdTimestamp;
    entities.MonthlyObligeeSummary.LastUpdatedBy = createdBy;
    entities.MonthlyObligeeSummary.LastUpdatedTmst = createdTimestamp;
    entities.MonthlyObligeeSummary.AdcReimbursedAmount =
      disbursementsSuppressed;
    entities.MonthlyObligeeSummary.CpaSType = cpaSType;
    entities.MonthlyObligeeSummary.CspSNumber = cspSNumber;
    entities.MonthlyObligeeSummary.CollectionsAmount = disbursementsSuppressed;
    entities.MonthlyObligeeSummary.CollectionsDisbursedToAr =
      disbursementsSuppressed;
    entities.MonthlyObligeeSummary.FeeAmount = disbursementsSuppressed;
    entities.MonthlyObligeeSummary.TotExcessUraAmt = disbursementsSuppressed;
    entities.MonthlyObligeeSummary.NumberOfCollections = 0;
    entities.MonthlyObligeeSummary.NaCurrRecapAmt = naCurrRecapAmt;
    entities.MonthlyObligeeSummary.Populated = true;
  }

  private void DeleteMonthlyObligeeSummary()
  {
    Update("DeleteMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetInt32(command, "yer", entities.MonthlyObligeeSummary.Year);
        db.SetInt32(command, "mnth", entities.MonthlyObligeeSummary.Month);
        db.SetString(
          command, "cpaSType", entities.MonthlyObligeeSummary.CpaSType);
        db.SetString(
          command, "cspSNumber", entities.MonthlyObligeeSummary.CspSNumber);
      });
  }

  private bool ReadMonthlyObligeeSummary1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);
    entities.MonthlyObligeeSummary.Populated = false;

    return Read("ReadMonthlyObligeeSummary1",
      (db, command) =>
      {
        db.SetString(command, "cpaSType", entities.Obligee.Type1);
        db.SetString(command, "cspSNumber", entities.Obligee.CspNumber);
        db.SetInt32(command, "yer", local.ForUpdate.Year);
        db.SetInt32(command, "mnth", local.ForUpdate.Month);
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

  private IEnumerable<bool> ReadMonthlyObligeeSummary2()
  {
    entities.MonthlyObligeeSummary.Populated = false;

    return ReadEach("ReadMonthlyObligeeSummary2",
      (db, command) =>
      {
        db.SetString(command, "cspSNumber", import.Obligee.Number);
        db.SetNullableDateTime(
          command, "lastUpdatedTmst",
          import.DateWorkArea.Timestamp.GetValueOrDefault());
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

        return true;
      });
  }

  private bool ReadObligee()
  {
    entities.Obligee.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee.CspNumber = db.GetString(reader, 0);
        entities.Obligee.Type1 = db.GetString(reader, 1);
        entities.Obligee.Populated = true;
      });
  }

  private void UpdateMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.MonthlyObligeeSummary.Populated);

    var passthruRecapAmt =
      export.ProcessDtTbl.Item.MonthlyObligeeSummary.PassthruRecapAmt.
        GetValueOrDefault();
    var disbursementsSuppressed = 0M;
    var naArrearsRecapAmt =
      export.ProcessDtTbl.Item.MonthlyObligeeSummary.NaArrearsRecapAmt.
        GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = import.DateWorkArea.Timestamp;
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
        db.
          SetNullableDecimal(command, "recapturedAmt", disbursementsSuppressed);
          
        db.SetNullableDecimal(command, "naArsRecapAmt", naArrearsRecapAmt);
        db.SetDecimal(command, "passthruAmount", disbursementsSuppressed);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableDecimal(
          command, "adcReimbursedAmt", disbursementsSuppressed);
        db.
          SetNullableDecimal(command, "collectionAmt", disbursementsSuppressed);
          
        db.
          SetNullableDecimal(command, "collctnDsbToAr", disbursementsSuppressed);
          
        db.SetNullableDecimal(command, "feeAmt", disbursementsSuppressed);
        db.SetNullableDecimal(
          command, "totExcessUraAmt", disbursementsSuppressed);
        db.SetNullableInt32(command, "nbrOfCollections", 0);
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
    entities.MonthlyObligeeSummary.RecapturedAmt = disbursementsSuppressed;
    entities.MonthlyObligeeSummary.NaArrearsRecapAmt = naArrearsRecapAmt;
    entities.MonthlyObligeeSummary.PassthruAmount = disbursementsSuppressed;
    entities.MonthlyObligeeSummary.CreatedBy = createdBy;
    entities.MonthlyObligeeSummary.CreatedTimestamp = createdTimestamp;
    entities.MonthlyObligeeSummary.LastUpdatedBy = createdBy;
    entities.MonthlyObligeeSummary.LastUpdatedTmst = createdTimestamp;
    entities.MonthlyObligeeSummary.AdcReimbursedAmount =
      disbursementsSuppressed;
    entities.MonthlyObligeeSummary.CollectionsAmount = disbursementsSuppressed;
    entities.MonthlyObligeeSummary.CollectionsDisbursedToAr =
      disbursementsSuppressed;
    entities.MonthlyObligeeSummary.FeeAmount = disbursementsSuppressed;
    entities.MonthlyObligeeSummary.TotExcessUraAmt = disbursementsSuppressed;
    entities.MonthlyObligeeSummary.NumberOfCollections = 0;
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
    /// A value of ProcessDtIndex.
    /// </summary>
    [JsonPropertyName("processDtIndex")]
    public NumericWorkSet ProcessDtIndex
    {
      get => processDtIndex ??= new();
      set => processDtIndex = value;
    }

    /// <summary>
    /// A value of ProcessDtYear.
    /// </summary>
    [JsonPropertyName("processDtYear")]
    public NumericWorkSet ProcessDtYear
    {
      get => processDtYear ??= new();
      set => processDtYear = value;
    }

    /// <summary>
    /// A value of ProcessDtMonth.
    /// </summary>
    [JsonPropertyName("processDtMonth")]
    public NumericWorkSet ProcessDtMonth
    {
      get => processDtMonth ??= new();
      set => processDtMonth = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public MonthlyObligeeSummary ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of PersonNumberDisplayedInd.
    /// </summary>
    [JsonPropertyName("personNumberDisplayedInd")]
    public Common PersonNumberDisplayedInd
    {
      get => personNumberDisplayedInd ??= new();
      set => personNumberDisplayedInd = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private NumericWorkSet processDtIndex;
    private NumericWorkSet processDtYear;
    private NumericWorkSet processDtMonth;
    private MonthlyObligeeSummary forUpdate;
    private Common personNumberDisplayedInd;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePersonAccount obligee;
    private CsePerson csePerson;
    private MonthlyObligeeSummary monthlyObligeeSummary;
  }
#endregion
}
