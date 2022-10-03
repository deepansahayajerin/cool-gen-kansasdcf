// Program: FN_B651_APPLY_SUPPRESSION_RULES, ID: 371004908, model: 746.
// Short name: SWE02701
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_APPLY_SUPPRESSION_RULES.
/// </summary>
[Serializable]
public partial class FnB651ApplySuppressionRules: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_APPLY_SUPPRESSION_RULES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651ApplySuppressionRules(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651ApplySuppressionRules.
  /// </summary>
  public FnB651ApplySuppressionRules(IContext context, Import import,
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
    // *********************************************************************
    // 03/29/01  PR 116634  K.Doshi -  Fix FDSO letter logic.
    // *********************************************************************
    // ****************************************************************
    // 05/29/02  PR 144630  Changed code to used the suppression reason of the 
    // original disbursement for the backed out disbursement for X URA
    // disbursements being suppressed.
    // ****************************************************************
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    Added changes for suppression by court 
    // order number.
    // *******************************************************************
    // ******************************************************************
    // 07/02/19  GVandy	CQ65423		Add System suppressions Y (Deceased) and Z (No 
    // active address).
    // *******************************************************************
    local.EabFileHandling.Action = "WRITE";
    local.HardcodeCollectonType.Type1 = "C";
    export.HighestSuppressionDate.Date = local.Initialized.Date;

    // **** Check to see if disbursement suppression is turned on for this 
    // person at the collection type level.
    UseFnCheckForCollDisbSup();
    export.HighestSuppressionDate.Date =
      local.CollectionSuppresEndDt.DiscontinueDate;

    if (Lt(local.Initialized.Date, local.CollectionSuppresEndDt.DiscontinueDate))
      
    {
      export.ForCreate.SuppressionReason = "C";
    }

    if (AsChar(import.TestDisplayInd.Flag) == 'Y' && Lt
      (local.Initialized.Date, local.CollectionSuppresEndDt.DiscontinueDate))
    {
      local.EabReportSend.RptDetail =
        "  Collection suppresion turned on for type: " + NumberToString
        (import.PerCollectionType.SequentialIdentifier, 14, 2) + " with a release date of " +
        NumberToString
        (DateToInt(local.CollectionSuppresEndDt.DiscontinueDate), 15);
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (Lt(Now().Date, local.CollectionSuppresEndDt.DiscontinueDate) && import
      .PerCollectionType.SequentialIdentifier == 3)
    {
      // **** For FDSO collection type suppressions we do not need to check 
      // person level or automatic.  ****
    }
    else
    {
      if (AsChar(import.PerCollection.AppliedToFuture) == 'Y')
      {
        UseFnCheckForAutomaticDisbSupp();

        if (Lt(export.HighestSuppressionDate.Date,
          local.AutomaticSuppressEndDt.DiscontinueDate))
        {
          export.HighestSuppressionDate.Date =
            local.AutomaticSuppressEndDt.DiscontinueDate;
          export.ForCreate.SuppressionReason = "A";
        }

        if (AsChar(import.TestDisplayInd.Flag) == 'Y' && Lt
          (local.Initialized.Date, local.AutomaticSuppressEndDt.DiscontinueDate))
          
        {
          local.EabReportSend.RptDetail =
            "  Auto suppression turned on with release date of " + NumberToString
            (DateToInt(local.AutomaticSuppressEndDt.DiscontinueDate), 15);
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }

      // **** Check to see if disbursement suppression is turned on for this 
      // person at the person level (all disbursements).
      UseFnCheckForPerDisbSup();

      if (Lt(export.HighestSuppressionDate.Date,
        local.PersonSuppressEndDt.DiscontinueDate))
      {
        export.HighestSuppressionDate.Date =
          local.PersonSuppressEndDt.DiscontinueDate;
        export.ForCreate.SuppressionReason = "P";
      }

      if (AsChar(import.TestDisplayInd.Flag) == 'Y' && Lt
        (local.Initialized.Date, local.PersonSuppressEndDt.DiscontinueDate))
      {
        local.EabReportSend.RptDetail =
          "  Person suppression turned on with release date of " + NumberToString
          (DateToInt(local.PersonSuppressEndDt.DiscontinueDate), 15);
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      // **** Check for URA suppression.  *****
      if (AsChar(import.PerDisbursementTransaction.ExcessUraInd) == 'Y')
      {
        if (import.PerDisbursementTransaction.Amount < 0)
        {
          local.UraSuppressEndDt.DiscontinueDate = local.Initialized.Date;

          // **** For an adjustment set the disb suppression release date to the
          // same date as the original disb.  ****
          if (ReadDisbursementTransaction1())
          {
            if (ReadDisbursementTransaction2())
            {
              if (ReadDisbursementStatusHistoryDisbursementStatus())
              {
                if (Lt(import.ProgramProcessingInfo.ProcessDate,
                  entities.OriginalDisbursementStatusHistory.DiscontinueDate) &&
                  entities
                  .OriginalDisbursementStatus.SystemGeneratedIdentifier == 3)
                {
                  local.UraSuppressEndDt.DiscontinueDate =
                    entities.OriginalDisbursementStatusHistory.DiscontinueDate;
                  export.ForCreate.SuppressionReason =
                    entities.OriginalDisbursementStatusHistory.
                      SuppressionReason;
                }
              }

              if (!entities.OriginalDisbursementStatusHistory.Populated)
              {
                ExitState = "FN0000_DISB_STAT_HIST_NF";

                return;
              }
            }
            else
            {
              ExitState = "FN0000_ORIGINAL_DISB_NF";

              return;
            }

            if (Equal(local.UraSuppressEndDt.DiscontinueDate,
              local.Initialized.Date))
            {
              // ****The original ura disbursement is no longer suppressed so 
              // the ura adjustment needs to go out without delay.  ****
              export.HighestSuppressionDate.Date = local.Initialized.Date;

              return;
            }
          }
          else
          {
            ExitState = "FN0000_DISB_CREDIT_NF_FOR_ADJUST";
          }
        }
        else
        {
          local.UraSuppressEndDt.DiscontinueDate =
            AddDays(import.ProgramProcessingInfo.ProcessDate,
            import.UraSuppressionLength.LastUsedNumber);

          // **** If an X suppr already exists for the same discontinue date 
          // then skip the create  *****
          if (ReadDisbSuppressionStatusHistory1())
          {
            // **** Do not create the X suppression rule if one already exists 
            // for the same date.  *****
          }
          else
          {
            // **** Create the X suppression rule  *****
            local.LastId.SystemGeneratedIdentifier = 0;

            if (ReadDisbSuppressionStatusHistory2())
            {
              local.LastId.SystemGeneratedIdentifier =
                entities.ReadForId.SystemGeneratedIdentifier;
            }

            try
            {
              CreateDisbSuppressionStatusHistory();
              import.ExpDatabaseUpdated.Flag = "Y";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_DISB_SUPP_STAT_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_DISB_SUPP_STAT_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        if (Lt(export.HighestSuppressionDate.Date,
          local.UraSuppressEndDt.DiscontinueDate))
        {
          export.HighestSuppressionDate.Date =
            local.UraSuppressEndDt.DiscontinueDate;

          if (import.PerDisbursementTransaction.Amount > 0)
          {
            export.ForCreate.SuppressionReason = "X";
          }
        }

        if (AsChar(import.TestDisplayInd.Flag) == 'Y' && Lt
          (local.Initialized.Date, local.UraSuppressEndDt.DiscontinueDate))
        {
          local.EabReportSend.RptDetail =
            "  URA suppression turned on with release date of " + NumberToString
            (DateToInt(local.UraSuppressEndDt.DiscontinueDate), 15);
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (Lt(local.Initialized.Date, local.UraSuppressEndDt.DiscontinueDate))
        {
          // **** Create an event  *****
          UseFnB651CreateUraDsbSupEvent();

          // **** We do not want to abend on an error creating an event  *****
          import.ExpDatabaseUpdated.Flag = "N";

          return;
        }
      }
    }

    // *****  changes for WR 040796
    if (Lt(Now().Date, local.CollectionSuppresEndDt.DiscontinueDate))
    {
      // This disbursement is already being suppressed.
    }
    else
    {
      // Check for O type suppression
      UseFnCheckForCourtOrderSuppr();

      if (Lt(export.HighestSuppressionDate.Date,
        local.CourtOrderSuppresEndDt.DiscontinueDate))
      {
        export.HighestSuppressionDate.Date =
          local.CourtOrderSuppresEndDt.DiscontinueDate;
        export.ForCreate.SuppressionReason = "O";
      }
    }

    // *****  changes for WR 040796
    // -- 07/02/19  GVandy  CQ65423  Add System suppressions Y (Deceased) and Z 
    // (No active address).
    if (Lt(Now().Date, export.HighestSuppressionDate.Date))
    {
      // This disbursement is already being suppressed for a different reason.
    }
    else
    {
      // Check for System suppressions Y (Deceased) and Z (No Address Address)
      UseFnCheckForSystemSuppression();

      if (Lt(export.HighestSuppressionDate.Date,
        local.SystemSuppressEndDt.DiscontinueDate))
      {
        export.HighestSuppressionDate.Date =
          local.SystemSuppressEndDt.DiscontinueDate;
        export.ForCreate.SuppressionReason = local.SystemSuppressEndDt.Type1;
      }

      if (AsChar(import.TestDisplayInd.Flag) == 'Y' && Lt
        (local.Initialized.Date, local.SystemSuppressEndDt.DiscontinueDate))
      {
        local.EabReportSend.RptDetail = " System suppression " + local
          .SystemSuppressEndDt.Type1 + " (" + (
            local.SystemSuppressEndDt.ReasonText ?? "") + ") turned on with release date of " +
          NumberToString
          (DateToInt(local.SystemSuppressEndDt.DiscontinueDate), 15);
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        }
      }
    }

    // ****************************************************************
    // FDSO letter action block:
    // If there is a suppression on the Collection Type: Code of  F (fdso) and 
    // the Collection: Program applied to is equal to NA, then the trigger for
    // the letter should be created. (only 1 letter per CR.)
    // ****************************************************************
  }

  private static void MoveApEvent1(Export.ApEventGroup source,
    FnB651CreateUraDsbSupEvent.Export.ApEventGroup target)
  {
    target.ApGrpDtl.Number = source.ApGrpDtl.Number;
    target.RegUraGrpDtl.Flag = source.RegUraGrpDtl.Flag;
    target.RegUraAdjGrpDtl.Flag = source.RegUraAdjGrpDtl.Flag;
    target.MedUraGrpDtl.Flag = source.MedUraGrpDtl.Flag;
    target.MedUraAdjGrpDtl.Flag = source.MedUraAdjGrpDtl.Flag;
  }

  private static void MoveApEvent2(FnB651CreateUraDsbSupEvent.Export.
    ApEventGroup source, Export.ApEventGroup target)
  {
    target.ApGrpDtl.Number = source.ApGrpDtl.Number;
    target.RegUraGrpDtl.Flag = source.RegUraGrpDtl.Flag;
    target.RegUraAdjGrpDtl.Flag = source.RegUraAdjGrpDtl.Flag;
    target.MedUraGrpDtl.Flag = source.MedUraGrpDtl.Flag;
    target.MedUraAdjGrpDtl.Flag = source.MedUraAdjGrpDtl.Flag;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB651CreateUraDsbSupEvent()
  {
    var useImport = new FnB651CreateUraDsbSupEvent.Import();
    var useExport = new FnB651CreateUraDsbSupEvent.Export();

    useImport.Ar.Number = import.PerObligeeCsePerson.Number;
    useImport.Collection.Assign(import.PerCollection);
    useImport.Ap.Number = import.PerObligor.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.PerObligationType.SystemGeneratedIdentifier;
    useImport.UraSuppressionLength.LastUsedNumber =
      import.UraSuppressionLength.LastUsedNumber;
    useImport.HighestSuppression.Date = export.HighestSuppressionDate.Date;
    export.ApEvent.CopyTo(useExport.ApEvent, MoveApEvent1);

    Call(FnB651CreateUraDsbSupEvent.Execute, useImport, useExport);

    useExport.ApEvent.CopyTo(export.ApEvent, MoveApEvent2);
  }

  private void UseFnCheckForAutomaticDisbSupp()
  {
    var useImport = new FnCheckForAutomaticDisbSupp.Import();
    var useExport = new FnCheckForAutomaticDisbSupp.Export();

    useImport.CsePerson.Number = import.PerObligeeCsePerson.Number;
    useImport.DebtDetail.DueDt = import.PerDebtDetail.DueDt;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    Call(FnCheckForAutomaticDisbSupp.Execute, useImport, useExport);

    local.AutomaticSuppressEndDt.DiscontinueDate =
      useExport.DisbSuppressionStatusHistory.DiscontinueDate;
  }

  private void UseFnCheckForCollDisbSup()
  {
    var useImport = new FnCheckForCollDisbSup.Import();
    var useExport = new FnCheckForCollDisbSup.Export();

    useImport.CsePerson.Number = import.PerObligeeCsePerson.Number;
    useImport.DisbursementTransaction.CollectionDate =
      import.PerDisbursementTransaction.CollectionDate;
    useImport.CollectionType.SequentialIdentifier =
      import.PerCollectionType.SequentialIdentifier;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.HardcodeCollectionType.Type1 = local.HardcodeCollectonType.Type1;

    Call(FnCheckForCollDisbSup.Execute, useImport, useExport);

    local.CollectionSuppresEndDt.DiscontinueDate =
      useExport.DisbSuppressionStatusHistory.DiscontinueDate;
  }

  private void UseFnCheckForCourtOrderSuppr()
  {
    var useImport = new FnCheckForCourtOrderSuppr.Import();
    var useExport = new FnCheckForCourtOrderSuppr.Export();

    useImport.PerObligee.Assign(import.PerObligeeCsePersonAccount);
    useImport.Per.Assign(import.PerDisbursementTransaction);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.TestDisplayInd.Flag = import.TestDisplayInd.Flag;

    Call(FnCheckForCourtOrderSuppr.Execute, useImport, useExport);

    import.PerObligeeCsePersonAccount.Assign(useImport.PerObligee);
    import.PerDisbursementTransaction.Assign(useImport.Per);
    local.CourtOrderSuppresEndDt.DiscontinueDate =
      useExport.DisbSuppressionStatusHistory.DiscontinueDate;
  }

  private void UseFnCheckForPerDisbSup()
  {
    var useImport = new FnCheckForPerDisbSup.Import();
    var useExport = new FnCheckForPerDisbSup.Export();

    useImport.CsePerson.Number = import.PerObligeeCsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    Call(FnCheckForPerDisbSup.Execute, useImport, useExport);

    local.PersonSuppressEndDt.DiscontinueDate =
      useExport.DisbSuppressionStatusHistory.DiscontinueDate;
  }

  private void UseFnCheckForSystemSuppression()
  {
    var useImport = new FnCheckForSystemSuppression.Import();
    var useExport = new FnCheckForSystemSuppression.Export();

    useImport.Ar.Number = import.PerObligeeCsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Persistent.Assign(import.PerDisbursementTransaction);

    Call(FnCheckForSystemSuppression.Execute, useImport, useExport);

    local.SystemSuppressEndDt.Assign(useExport.DisbSuppressionStatusHistory);
  }

  private void CreateDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.
      Assert(import.PerObligeeCsePersonAccount.Populated);

    var cpaType = import.PerObligeeCsePersonAccount.Type1;
    var cspNumber = import.PerObligeeCsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = local.LastId.SystemGeneratedIdentifier + 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.UraSuppressEndDt.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = "X";

    CheckValid<DisbSuppressionStatusHistory>("CpaType", cpaType);
    CheckValid<DisbSuppressionStatusHistory>("Type1", type1);
    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("CreateDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "dssGeneratedId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "personDisbFiller", "");
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.DisbSuppressionStatusHistory.CpaType = cpaType;
    entities.DisbSuppressionStatusHistory.CspNumber = cspNumber;
    entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbSuppressionStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbSuppressionStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbSuppressionStatusHistory.CreatedBy = createdBy;
    entities.DisbSuppressionStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbSuppressionStatusHistory.Type1 = type1;
    entities.DisbSuppressionStatusHistory.ReasonText = "";
    entities.DisbSuppressionStatusHistory.Populated = true;
  }

  private bool ReadDisbSuppressionStatusHistory1()
  {
    System.Diagnostics.Debug.
      Assert(import.PerObligeeCsePersonAccount.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory1",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", import.PerObligeeCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", import.PerObligeeCsePersonAccount.CspNumber);
        db.SetDate(
          command, "discontinueDate",
          local.UraSuppressEndDt.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory2()
  {
    System.Diagnostics.Debug.
      Assert(import.PerObligeeCsePersonAccount.Populated);
    entities.ReadForId.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory2",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", import.PerObligeeCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", import.PerObligeeCsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.ReadForId.CpaType = db.GetString(reader, 0);
        entities.ReadForId.CspNumber = db.GetString(reader, 1);
        entities.ReadForId.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ReadForId.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.ReadForId.CpaType);
      });
  }

  private bool ReadDisbursementStatusHistoryDisbursementStatus()
  {
    System.Diagnostics.Debug.Assert(entities.OriginalDebit.Populated);
    entities.OriginalDisbursementStatusHistory.Populated = false;
    entities.OriginalDisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatusHistoryDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.OriginalDebit.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.OriginalDebit.CspNumber);
        db.SetString(command, "cpaType", entities.OriginalDebit.CpaType);
      },
      (db, reader) =>
      {
        entities.OriginalDisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.OriginalDisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalDisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.OriginalDisbursementStatusHistory.CspNumber =
          db.GetString(reader, 2);
        entities.OriginalDisbursementStatusHistory.CpaType =
          db.GetString(reader, 3);
        entities.OriginalDisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.OriginalDisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OriginalDisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.OriginalDisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 7);
        entities.OriginalDisbursementStatusHistory.Populated = true;
        entities.OriginalDisbursementStatus.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.OriginalDisbursementStatusHistory.CpaType);
      });
  }

  private bool ReadDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(import.PerCollection.Populated);
    entities.OriginalCredit.Populated = false;

    return Read("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId", import.PerCollection.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "otyId", import.PerCollection.OtyId);
        db.SetNullableInt32(command, "obgId", import.PerCollection.ObgId);
        db.SetNullableString(
          command, "cspNumberDisb", import.PerCollection.CspNumber);
        db.SetNullableString(
          command, "cpaTypeDisb", import.PerCollection.CpaType);
        db.SetNullableInt32(command, "otrId", import.PerCollection.OtrId);
        db.SetNullableString(
          command, "otrTypeDisb", import.PerCollection.OtrType);
        db.SetNullableInt32(command, "crtId", import.PerCollection.CrtType);
        db.SetNullableInt32(command, "cstId", import.PerCollection.CstId);
        db.SetNullableInt32(command, "crvId", import.PerCollection.CrvId);
        db.SetNullableInt32(command, "crdId", import.PerCollection.CrdId);
        db.SetInt32(
          command, "disbTranId",
          import.PerDisbursementTransaction.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalCredit.CpaType = db.GetString(reader, 0);
        entities.OriginalCredit.CspNumber = db.GetString(reader, 1);
        entities.OriginalCredit.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalCredit.Type1 = db.GetString(reader, 3);
        entities.OriginalCredit.OtyId = db.GetNullableInt32(reader, 4);
        entities.OriginalCredit.OtrTypeDisb = db.GetNullableString(reader, 5);
        entities.OriginalCredit.OtrId = db.GetNullableInt32(reader, 6);
        entities.OriginalCredit.CpaTypeDisb = db.GetNullableString(reader, 7);
        entities.OriginalCredit.CspNumberDisb = db.GetNullableString(reader, 8);
        entities.OriginalCredit.ObgId = db.GetNullableInt32(reader, 9);
        entities.OriginalCredit.CrdId = db.GetNullableInt32(reader, 10);
        entities.OriginalCredit.CrvId = db.GetNullableInt32(reader, 11);
        entities.OriginalCredit.CstId = db.GetNullableInt32(reader, 12);
        entities.OriginalCredit.CrtId = db.GetNullableInt32(reader, 13);
        entities.OriginalCredit.ColId = db.GetNullableInt32(reader, 14);
        entities.OriginalCredit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.OriginalCredit.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.OriginalCredit.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.OriginalCredit.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.OriginalCredit.CpaTypeDisb);
      });
  }

  private bool ReadDisbursementTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.OriginalCredit.Populated);
    entities.OriginalDebit.Populated = false;

    return Read("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.OriginalCredit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.OriginalCredit.CpaType);
        db.SetString(command, "cspPNumber", entities.OriginalCredit.CspNumber);
      },
      (db, reader) =>
      {
        entities.OriginalDebit.CpaType = db.GetString(reader, 0);
        entities.OriginalDebit.CspNumber = db.GetString(reader, 1);
        entities.OriginalDebit.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalDebit.Type1 = db.GetString(reader, 3);
        entities.OriginalDebit.ProcessDate = db.GetNullableDate(reader, 4);
        entities.OriginalDebit.DbtGeneratedId = db.GetNullableInt32(reader, 5);
        entities.OriginalDebit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.OriginalDebit.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.OriginalDebit.Type1);
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
    /// A value of ExpDatabaseUpdated.
    /// </summary>
    [JsonPropertyName("expDatabaseUpdated")]
    public Common ExpDatabaseUpdated
    {
      get => expDatabaseUpdated ??= new();
      set => expDatabaseUpdated = value;
    }

    /// <summary>
    /// A value of PerObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("perObligeeCsePerson")]
    public CsePerson PerObligeeCsePerson
    {
      get => perObligeeCsePerson ??= new();
      set => perObligeeCsePerson = value;
    }

    /// <summary>
    /// A value of PerObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("perObligeeCsePersonAccount")]
    public CsePersonAccount PerObligeeCsePersonAccount
    {
      get => perObligeeCsePersonAccount ??= new();
      set => perObligeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of PerDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("perDisbursementTransaction")]
    public DisbursementTransaction PerDisbursementTransaction
    {
      get => perDisbursementTransaction ??= new();
      set => perDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of PerCollection.
    /// </summary>
    [JsonPropertyName("perCollection")]
    public Collection PerCollection
    {
      get => perCollection ??= new();
      set => perCollection = value;
    }

    /// <summary>
    /// A value of PerCashReceiptType.
    /// </summary>
    [JsonPropertyName("perCashReceiptType")]
    public CashReceiptType PerCashReceiptType
    {
      get => perCashReceiptType ??= new();
      set => perCashReceiptType = value;
    }

    /// <summary>
    /// A value of PerCollectionType.
    /// </summary>
    [JsonPropertyName("perCollectionType")]
    public CollectionType PerCollectionType
    {
      get => perCollectionType ??= new();
      set => perCollectionType = value;
    }

    /// <summary>
    /// A value of PerDebtDetail.
    /// </summary>
    [JsonPropertyName("perDebtDetail")]
    public DebtDetail PerDebtDetail
    {
      get => perDebtDetail ??= new();
      set => perDebtDetail = value;
    }

    /// <summary>
    /// A value of PerObligor.
    /// </summary>
    [JsonPropertyName("perObligor")]
    public CsePerson PerObligor
    {
      get => perObligor ??= new();
      set => perObligor = value;
    }

    /// <summary>
    /// A value of PerObligationType.
    /// </summary>
    [JsonPropertyName("perObligationType")]
    public ObligationType PerObligationType
    {
      get => perObligationType ??= new();
      set => perObligationType = value;
    }

    /// <summary>
    /// A value of PerSupported.
    /// </summary>
    [JsonPropertyName("perSupported")]
    public CsePerson PerSupported
    {
      get => perSupported ??= new();
      set => perSupported = value;
    }

    /// <summary>
    /// A value of Per3Suppressed.
    /// </summary>
    [JsonPropertyName("per3Suppressed")]
    public DisbursementStatus Per3Suppressed
    {
      get => per3Suppressed ??= new();
      set => per3Suppressed = value;
    }

    /// <summary>
    /// A value of UraSuppressionLength.
    /// </summary>
    [JsonPropertyName("uraSuppressionLength")]
    public ControlTable UraSuppressionLength
    {
      get => uraSuppressionLength ??= new();
      set => uraSuppressionLength = value;
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
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    private Common expDatabaseUpdated;
    private CsePerson perObligeeCsePerson;
    private CsePersonAccount perObligeeCsePersonAccount;
    private DisbursementTransaction perDisbursementTransaction;
    private Collection perCollection;
    private CashReceiptType perCashReceiptType;
    private CollectionType perCollectionType;
    private DebtDetail perDebtDetail;
    private CsePerson perObligor;
    private ObligationType perObligationType;
    private CsePerson perSupported;
    private DisbursementStatus per3Suppressed;
    private ControlTable uraSuppressionLength;
    private ProgramProcessingInfo programProcessingInfo;
    private Common testDisplayInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ApEventGroup group.</summary>
    [Serializable]
    public class ApEventGroup
    {
      /// <summary>
      /// A value of ApGrpDtl.
      /// </summary>
      [JsonPropertyName("apGrpDtl")]
      public CsePerson ApGrpDtl
      {
        get => apGrpDtl ??= new();
        set => apGrpDtl = value;
      }

      /// <summary>
      /// A value of RegUraGrpDtl.
      /// </summary>
      [JsonPropertyName("regUraGrpDtl")]
      public Common RegUraGrpDtl
      {
        get => regUraGrpDtl ??= new();
        set => regUraGrpDtl = value;
      }

      /// <summary>
      /// A value of RegUraAdjGrpDtl.
      /// </summary>
      [JsonPropertyName("regUraAdjGrpDtl")]
      public Common RegUraAdjGrpDtl
      {
        get => regUraAdjGrpDtl ??= new();
        set => regUraAdjGrpDtl = value;
      }

      /// <summary>
      /// A value of MedUraGrpDtl.
      /// </summary>
      [JsonPropertyName("medUraGrpDtl")]
      public Common MedUraGrpDtl
      {
        get => medUraGrpDtl ??= new();
        set => medUraGrpDtl = value;
      }

      /// <summary>
      /// A value of MedUraAdjGrpDtl.
      /// </summary>
      [JsonPropertyName("medUraAdjGrpDtl")]
      public Common MedUraAdjGrpDtl
      {
        get => medUraAdjGrpDtl ??= new();
        set => medUraAdjGrpDtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson apGrpDtl;
      private Common regUraGrpDtl;
      private Common regUraAdjGrpDtl;
      private Common medUraGrpDtl;
      private Common medUraAdjGrpDtl;
    }

    /// <summary>
    /// A value of HighestSuppressionDate.
    /// </summary>
    [JsonPropertyName("highestSuppressionDate")]
    public DateWorkArea HighestSuppressionDate
    {
      get => highestSuppressionDate ??= new();
      set => highestSuppressionDate = value;
    }

    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public DisbursementStatusHistory ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

    /// <summary>
    /// Gets a value of ApEvent.
    /// </summary>
    [JsonIgnore]
    public Array<ApEventGroup> ApEvent => apEvent ??= new(
      ApEventGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ApEvent for json serialization.
    /// </summary>
    [JsonPropertyName("apEvent")]
    [Computed]
    public IList<ApEventGroup> ApEvent_Json
    {
      get => apEvent;
      set => ApEvent.Assign(value);
    }

    private DateWorkArea highestSuppressionDate;
    private DisbursementStatusHistory forCreate;
    private Array<ApEventGroup> apEvent;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SystemSuppressEndDt.
    /// </summary>
    [JsonPropertyName("systemSuppressEndDt")]
    public DisbSuppressionStatusHistory SystemSuppressEndDt
    {
      get => systemSuppressEndDt ??= new();
      set => systemSuppressEndDt = value;
    }

    /// <summary>
    /// A value of CheckForDupPmt.
    /// </summary>
    [JsonPropertyName("checkForDupPmt")]
    public Common CheckForDupPmt
    {
      get => checkForDupPmt ??= new();
      set => checkForDupPmt = value;
    }

    /// <summary>
    /// A value of CollectionSuppresEndDt.
    /// </summary>
    [JsonPropertyName("collectionSuppresEndDt")]
    public DisbSuppressionStatusHistory CollectionSuppresEndDt
    {
      get => collectionSuppresEndDt ??= new();
      set => collectionSuppresEndDt = value;
    }

    /// <summary>
    /// A value of AutomaticSuppressEndDt.
    /// </summary>
    [JsonPropertyName("automaticSuppressEndDt")]
    public DisbSuppressionStatusHistory AutomaticSuppressEndDt
    {
      get => automaticSuppressEndDt ??= new();
      set => automaticSuppressEndDt = value;
    }

    /// <summary>
    /// A value of PersonSuppressEndDt.
    /// </summary>
    [JsonPropertyName("personSuppressEndDt")]
    public DisbSuppressionStatusHistory PersonSuppressEndDt
    {
      get => personSuppressEndDt ??= new();
      set => personSuppressEndDt = value;
    }

    /// <summary>
    /// A value of UraSuppressEndDt.
    /// </summary>
    [JsonPropertyName("uraSuppressEndDt")]
    public DisbSuppressionStatusHistory UraSuppressEndDt
    {
      get => uraSuppressEndDt ??= new();
      set => uraSuppressEndDt = value;
    }

    /// <summary>
    /// A value of CourtOrderSuppresEndDt.
    /// </summary>
    [JsonPropertyName("courtOrderSuppresEndDt")]
    public DisbSuppressionStatusHistory CourtOrderSuppresEndDt
    {
      get => courtOrderSuppresEndDt ??= new();
      set => courtOrderSuppresEndDt = value;
    }

    /// <summary>
    /// A value of LastId.
    /// </summary>
    [JsonPropertyName("lastId")]
    public DisbSuppressionStatusHistory LastId
    {
      get => lastId ??= new();
      set => lastId = value;
    }

    /// <summary>
    /// A value of NbrSuprDisbForArAndCr.
    /// </summary>
    [JsonPropertyName("nbrSuprDisbForArAndCr")]
    public Common NbrSuprDisbForArAndCr
    {
      get => nbrSuprDisbForArAndCr ??= new();
      set => nbrSuprDisbForArAndCr = value;
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
    /// A value of FdsoLetter.
    /// </summary>
    [JsonPropertyName("fdsoLetter")]
    public SpDocKey FdsoLetter
    {
      get => fdsoLetter ??= new();
      set => fdsoLetter = value;
    }

    /// <summary>
    /// A value of FdsoDocument.
    /// </summary>
    [JsonPropertyName("fdsoDocument")]
    public Document FdsoDocument
    {
      get => fdsoDocument ??= new();
      set => fdsoDocument = value;
    }

    /// <summary>
    /// A value of FdsoInfrastructure.
    /// </summary>
    [JsonPropertyName("fdsoInfrastructure")]
    public Infrastructure FdsoInfrastructure
    {
      get => fdsoInfrastructure ??= new();
      set => fdsoInfrastructure = value;
    }

    /// <summary>
    /// A value of ExitStateMessage.
    /// </summary>
    [JsonPropertyName("exitStateMessage")]
    public ExitStateWorkArea ExitStateMessage
    {
      get => exitStateMessage ??= new();
      set => exitStateMessage = value;
    }

    /// <summary>
    /// A value of HardcodeCollectonType.
    /// </summary>
    [JsonPropertyName("hardcodeCollectonType")]
    public DisbSuppressionStatusHistory HardcodeCollectonType
    {
      get => hardcodeCollectonType ??= new();
      set => hardcodeCollectonType = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private DisbSuppressionStatusHistory systemSuppressEndDt;
    private Common checkForDupPmt;
    private DisbSuppressionStatusHistory collectionSuppresEndDt;
    private DisbSuppressionStatusHistory automaticSuppressEndDt;
    private DisbSuppressionStatusHistory personSuppressEndDt;
    private DisbSuppressionStatusHistory uraSuppressEndDt;
    private DisbSuppressionStatusHistory courtOrderSuppresEndDt;
    private DisbSuppressionStatusHistory lastId;
    private Common nbrSuprDisbForArAndCr;
    private Case1 case1;
    private SpDocKey fdsoLetter;
    private Document fdsoDocument;
    private Infrastructure fdsoInfrastructure;
    private ExitStateWorkArea exitStateMessage;
    private DisbSuppressionStatusHistory hardcodeCollectonType;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OriginalCredit.
    /// </summary>
    [JsonPropertyName("originalCredit")]
    public DisbursementTransaction OriginalCredit
    {
      get => originalCredit ??= new();
      set => originalCredit = value;
    }

    /// <summary>
    /// A value of OriginalDebit.
    /// </summary>
    [JsonPropertyName("originalDebit")]
    public DisbursementTransaction OriginalDebit
    {
      get => originalDebit ??= new();
      set => originalDebit = value;
    }

    /// <summary>
    /// A value of OriginalDisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("originalDisbursementStatusHistory")]
    public DisbursementStatusHistory OriginalDisbursementStatusHistory
    {
      get => originalDisbursementStatusHistory ??= new();
      set => originalDisbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of OriginalDisbursementStatus.
    /// </summary>
    [JsonPropertyName("originalDisbursementStatus")]
    public DisbursementStatus OriginalDisbursementStatus
    {
      get => originalDisbursementStatus ??= new();
      set => originalDisbursementStatus = value;
    }

    /// <summary>
    /// A value of OriginalDisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("originalDisbursementTransactionRln")]
    public DisbursementTransactionRln OriginalDisbursementTransactionRln
    {
      get => originalDisbursementTransactionRln ??= new();
      set => originalDisbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of OriginalDisbursementType.
    /// </summary>
    [JsonPropertyName("originalDisbursementType")]
    public DisbursementType OriginalDisbursementType
    {
      get => originalDisbursementType ??= new();
      set => originalDisbursementType = value;
    }

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
    /// A value of ReadForId.
    /// </summary>
    [JsonPropertyName("readForId")]
    public DisbSuppressionStatusHistory ReadForId
    {
      get => readForId ??= new();
      set => readForId = value;
    }

    /// <summary>
    /// A value of ReadForDupFdsoLettersDisbursement.
    /// </summary>
    [JsonPropertyName("readForDupFdsoLettersDisbursement")]
    public DisbursementTransaction ReadForDupFdsoLettersDisbursement
    {
      get => readForDupFdsoLettersDisbursement ??= new();
      set => readForDupFdsoLettersDisbursement = value;
    }

    /// <summary>
    /// A value of ReadForDupFdsoLettersDisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("readForDupFdsoLettersDisbursementStatusHistory")]
    public DisbursementStatusHistory ReadForDupFdsoLettersDisbursementStatusHistory
      
    {
      get => readForDupFdsoLettersDisbursementStatusHistory ??= new();
      set => readForDupFdsoLettersDisbursementStatusHistory = value;
    }

    private DisbursementTransaction originalCredit;
    private DisbursementTransaction originalDebit;
    private DisbursementStatusHistory originalDisbursementStatusHistory;
    private DisbursementStatus originalDisbursementStatus;
    private DisbursementTransactionRln originalDisbursementTransactionRln;
    private DisbursementType originalDisbursementType;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DisbSuppressionStatusHistory readForId;
    private DisbursementTransaction readForDupFdsoLettersDisbursement;
    private DisbursementStatusHistory readForDupFdsoLettersDisbursementStatusHistory;
      
  }
#endregion
}
