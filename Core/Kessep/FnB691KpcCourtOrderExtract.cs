// Program: FN_B691_KPC_COURT_ORDER_EXTRACT, ID: 374396524, model: 746.
// Short name: SWEF691B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B691_KPC_COURT_ORDER_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB691KpcCourtOrderExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B691_KPC_COURT_ORDER_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB691KpcCourtOrderExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB691KpcCourtOrderExtract.
  /// </summary>
  public FnB691KpcCourtOrderExtract(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------------------------------------------------------------------------------------------------------------
    // 03/16/00  SWTTREM			Initial Code
    // 10/12/00  VMadhira	WR#000172N	Modified the code and redesigned the 
    // process. Now the
    // 					process will use  NCO, RCO and  NDBT action codes only.
    // 					See the Technical Document for details.
    // 01/23/03  GVandy	PR 162868	Correct logic setting old court order number, 
    // city, and county.
    // --------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------
    // 11/07/07  G. Pan   CQ614
    // Added a statement in first READ EACH legal_action_detail to check if the
    // record is qualified with obligation.
    // -------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB691Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.FileCount.Count = 1;
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    local.KpcHighDate.Date = new DateTime(9999, 12, 31);
    local.LowDate.Date = new DateTime(1901, 1, 1);
    local.Default1.StartDate =
      NumberToString(DateToInt(local.CurrentRun.Date), 8, 8);
    local.Default1.EndDate =
      NumberToString(DateToInt(local.KpcHighDate.Date), 8, 8);
    local.HeaderRecord.RecordType = "1";
    local.HeaderRecord.TransactionDate =
      NumberToString(DateToInt(local.CurrentRun.Date), 8, 8);
    local.HeaderRecord.Userid = "SWEFB691";

    // ****************************************************************************
    // * Transactions created by this program to be sent to the KPC:
    // *
    // *  Court Order Change.
    // *      Rec Type 1 - Action Code RCO       one per transaction
    // *      Rec Type 2 - Court Order Info      two per transaction
    // *          first has new court order, second has old court order
    // *
    // *   New Court Order and New Debt
    // *      Rec Type 1 - Action Code NCO       one per transaction
    // *      Rec Type 2 - Court Order Info      one per transaction
    // *      Rec Type 3 - Debt Info             one per debt
    // *      Rec Type 4 - Obligation Info       one per debt
    // *      Rec Type 5 - Participant Info      one per participant
    // *      Rec Type 6 - Participant Address   one per participant
    // *
    // *   New Debt, Existing Court Order
    // *      Rec Type 1 - Action Code NBDT      one per transaction
    // *      Rec Type 2 - Court Order Info      one per transaction
    // *      Rec Type 3 - Debt Info             one per transaction
    // *      Rec Type 4 - Obligation Info       one per transaction
    // *      Rec Type 5 - Participant Info      one per participant
    // *      Rec Type 6 - Participant Address   one per participant
    // ****************************************************************************
    // ****************************************************************************
    // * First create all court order change transactions.  This will allow
    // * subsequent new debts on this court order to be recognized.
    // ****************************************************************************
    foreach(var item in ReadLegalActionTribunal1())
    {
      local.HeaderRecord.ActionCode = "RCO";
      UseFnB691FormatCourtOrder();
      UseFnB691WriteRecsType1And2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ++local.RcoRecsWritten.Count;

      try
      {
        UpdateLegalAction3();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "LEGAL_ACTION_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ****************************************************************************
    // * Next,  process all New Court Orders.
    // ****************************************************************************
    foreach(var item in ReadLegalActionTribunal2())
    {
      // *****************************************************************
      // If the standard number is spaces or standard number greater than
      // 12 characters, set an error message
      // *****************************************************************
      if (IsEmpty(entities.LegalAction.StandardNumber) || Length
        (TrimEnd(entities.LegalAction.StandardNumber)) > 12)
      {
        ExitState = "FN0000_LEGAL_ACTION_NOT_VALID";
        UseFnB691PrintErrorLine1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        continue;
      }

      // ---------------------------------------------------------------------------------------
      // Check if a legal_action exists with the same court_case_number and for 
      // the same tribunal  having the KPC_FLAG set to Y. If one exists, means,
      // we already sent this court order to KPC .
      // --------------------------------------------------------------------------------------
      if (ReadLegalAction())
      {
        // ---------------------------------------------------------------------------------------
        // We already sent this court order information. Upate the  new  
        // Legal_Action with KPC_Flag set  to Y and KPC_Date to old Legal_Action
        // KPC_Date.
        // --------------------------------------------------------------------------------------
        if (!Equal(entities.Old.StandardNumber,
          entities.LegalAction.StandardNumber))
        {
          ExitState = "FN0000_LEGAL_ACTION_STAND_NO_ERR";
          UseFnB691PrintErrorLine1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          continue;
        }
        else
        {
          try
          {
            UpdateLegalAction2();

            continue;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "LEGAL_ACTION_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "LEGAL_ACTION_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ++local.LegalActionAlreadySent.Count;
        }
      }

      // *******************************
      // Set header record attributes
      // *******************************
      local.HeaderRecord.ActionCode = "NCO";
      UseFnB691FormatCourtOrder();
      UseFnB691WriteRecsType1And2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ++local.NcoRecsWritten.Count;

      foreach(var item1 in ReadLegalActionDetail())
      {
        UseFnB691WriteRecsType3456();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        try
        {
          UpdateLegalActionDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LEGAL_ACTION_DETAIL_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "LEGAL_ACTION_DETAIL_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      try
      {
        UpdateLegalAction1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "LEGAL_ACTION_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ****************************************************************************
    // * Next,  process all New Debts.
    // ****************************************************************************
    foreach(var item in ReadLegalActionDetailLegalAction())
    {
      // *****************************************************************
      // If the standard number is spaces or standard number greater than
      // 12 characters, set an error message
      // *****************************************************************
      if (IsEmpty(entities.LegalAction.StandardNumber) || Length
        (TrimEnd(entities.LegalAction.StandardNumber)) > 12)
      {
        ExitState = "FN0000_LEGAL_ACTION_NOT_VALID";
        UseFnB691PrintErrorLine1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        continue;
      }

      // *******************************
      // Set header record attributes
      // *******************************
      local.HeaderRecord.ActionCode = "NDBT";
      UseFnB691FormatCourtOrder();
      UseFnB691WriteRecsType1And2();
      UseFnB691WriteRecsType3456();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      try
      {
        UpdateLegalActionDetail();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_DETAIL_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "LEGAL_ACTION_DETAIL_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ++local.NdbtRecsWritten.Count;
    }

    local.CloseInd.Flag = "Y";

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ****************************
      // CLOSE OUTPUT FILE
      // ****************************
      local.KpcExternalParms.Parm1 = "CF";
      UseFnExtWriteInterfaceFile();

      if (!IsEmpty(local.Return1.Parm1))
      {
        ExitState = "FILE_CLOSE_ERROR";

        return;
      }

      // : Close the Error Report.
      UseFnB691PrintErrorLine2();
      UseFnB691PrintControlTotals();
    }
    else
    {
      // : Report the error that occurred and close the Error Report.
      //   ABEND the procedure once reporting is complete.
      UseFnB691PrintErrorLine1();
      UseFnB691PrintControlTotals();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    UseSiCloseAdabas();
  }

  private static void MoveCourtOrderRecord(CourtOrderRecord source,
    CourtOrderRecord target)
  {
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveKpcExternalParms(KpcExternalParms source,
    KpcExternalParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalActionDetail(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.FreqPeriodCode = source.FreqPeriodCode;
    target.Number = source.Number;
    target.DetailType = source.DetailType;
    target.EndDate = source.EndDate;
    target.EffectiveDate = source.EffectiveDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTstamp = source.LastUpdatedTstamp;
    target.KpcDate = source.KpcDate;
    target.CurrentAmount = source.CurrentAmount;
  }

  private void UseFnB691FormatCourtOrder()
  {
    var useImport = new FnB691FormatCourtOrder.Import();
    var useExport = new FnB691FormatCourtOrder.Export();

    useImport.LegalAction.Assign(entities.LegalAction);
    MoveCourtOrderRecord(local.Default1, useImport.Default1);

    Call(FnB691FormatCourtOrder.Execute, useImport, useExport);

    local.CourtOrderRecord.Assign(useExport.CourtOrderRecord);
    local.Old.Assign(useExport.Old);
  }

  private void UseFnB691Housekeeping()
  {
    var useImport = new FnB691Housekeeping.Import();
    var useExport = new FnB691Housekeeping.Export();

    Call(FnB691Housekeeping.Execute, useImport, useExport);

    local.RunInTestMode.Flag = useExport.RunInTestMode.Flag;
    local.LastRun.Date = useExport.LastRun.Date;
    local.CurrentRun.Date = useExport.CurrentRun.Date;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseFnB691PrintControlTotals()
  {
    var useImport = new FnB691PrintControlTotals.Import();
    var useExport = new FnB691PrintControlTotals.Export();

    useImport.LegalActionAlreadySent.Count = local.LegalActionAlreadySent.Count;
    useImport.RcoRecsWritten.Count = local.RcoRecsWritten.Count;
    useImport.NcoRecsWritten.Count = local.NcoRecsWritten.Count;
    useImport.NdbtRecsWritten.Count = local.NdbtRecsWritten.Count;
    useImport.TotalRecsWritten.Count = local.TotalRecsWritten.Count;

    Call(FnB691PrintControlTotals.Execute, useImport, useExport);
  }

  private void UseFnB691PrintErrorLine1()
  {
    var useImport = new FnB691PrintErrorLine.Import();
    var useExport = new FnB691PrintErrorLine.Export();

    MoveLegalAction(entities.LegalAction, useImport.LegalAction);
    useImport.CloseInd.Flag = local.CloseInd.Flag;

    Call(FnB691PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnB691PrintErrorLine2()
  {
    var useImport = new FnB691PrintErrorLine.Import();
    var useExport = new FnB691PrintErrorLine.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;

    Call(FnB691PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnB691WriteRecsType1And2()
  {
    var useImport = new FnB691WriteRecsType1And2.Import();
    var useExport = new FnB691WriteRecsType1And2.Export();

    useImport.TotalWritten.Count = local.TotalRecsWritten.Count;
    useImport.HeaderRecord.Assign(local.HeaderRecord);
    useImport.CourtOrderRecord.Assign(local.CourtOrderRecord);
    useImport.Old.Assign(local.Old);

    Call(FnB691WriteRecsType1And2.Execute, useImport, useExport);

    local.TotalRecsWritten.Count = useExport.TotalWritten.Count;
  }

  private void UseFnB691WriteRecsType3456()
  {
    var useImport = new FnB691WriteRecsType3456.Import();
    var useExport = new FnB691WriteRecsType3456.Export();

    useImport.LegalAction.Assign(entities.LegalAction);
    useImport.LegalActionDetail.Assign(entities.LegalActionDetail);
    useImport.TotalWritten.Count = local.TotalRecsWritten.Count;
    useImport.Current.Date = local.CurrentRun.Date;
    useImport.Max.Date = local.MaxDate.Date;

    Call(FnB691WriteRecsType3456.Execute, useImport, useExport);

    MoveLegalActionDetail(useImport.LegalActionDetail,
      entities.LegalActionDetail);
    local.TotalRecsWritten.Count = useExport.TotalWritten.Count;
  }

  private void UseFnExtWriteInterfaceFile()
  {
    var useImport = new FnExtWriteInterfaceFile.Import();
    var useExport = new FnExtWriteInterfaceFile.Export();

    useImport.FileCount.Count = local.FileCount.Count;
    MoveKpcExternalParms(local.KpcExternalParms, useImport.KpcExternalParms);
    MoveKpcExternalParms(local.KpcExternalParms, useExport.KpcExternalParms);

    Call(FnExtWriteInterfaceFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.KpcExternalParms);
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private bool ReadLegalAction()
  {
    entities.Old.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Old.Identifier = db.GetInt32(reader, 0);
        entities.Old.LastModificationReviewDate = db.GetNullableDate(reader, 1);
        entities.Old.Classification = db.GetString(reader, 2);
        entities.Old.FiledDate = db.GetNullableDate(reader, 3);
        entities.Old.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.Old.EndDate = db.GetNullableDate(reader, 5);
        entities.Old.StandardNumber = db.GetNullableString(reader, 6);
        entities.Old.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Old.LastUpdatedTstamp = db.GetNullableDateTime(reader, 8);
        entities.Old.TrbId = db.GetNullableInt32(reader, 9);
        entities.Old.KpcStandardNo = db.GetNullableString(reader, 10);
        entities.Old.KpcFlag = db.GetNullableString(reader, 11);
        entities.Old.KpcDate = db.GetNullableDate(reader, 12);
        entities.Old.KpcStdNoChgFlag = db.GetNullableString(reader, 13);
        entities.Old.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 7);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.KpcDate = db.GetNullableDate(reader, 9);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalAction()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "kpcDate", local.BlankDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 7);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.KpcDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 10);
        entities.LegalAction.Classification = db.GetString(reader, 11);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 12);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 13);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 14);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 15);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 16);
        entities.LegalAction.LastUpdatedBy = db.GetNullableString(reader, 17);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 18);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 19);
        entities.LegalAction.KpcStandardNo = db.GetNullableString(reader, 20);
        entities.LegalAction.KpcFlag = db.GetNullableString(reader, 21);
        entities.LegalAction.KpcDate = db.GetNullableDate(reader, 22);
        entities.LegalAction.KpcStdNoChgFlag = db.GetNullableString(reader, 23);
        entities.LegalAction.KpcTribunalId = db.GetNullableInt32(reader, 24);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal1()
  {
    entities.LegalAction.Populated = false;
    entities.Tribunal.Populated = false;

    return ReadEach("ReadLegalActionTribunal1",
      null,
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.Classification = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 10);
        entities.Tribunal.Identifier = db.GetInt32(reader, 10);
        entities.LegalAction.KpcStandardNo = db.GetNullableString(reader, 11);
        entities.LegalAction.KpcFlag = db.GetNullableString(reader, 12);
        entities.LegalAction.KpcDate = db.GetNullableDate(reader, 13);
        entities.LegalAction.KpcStdNoChgFlag = db.GetNullableString(reader, 14);
        entities.LegalAction.KpcTribunalId = db.GetNullableInt32(reader, 15);
        entities.LegalAction.Populated = true;
        entities.Tribunal.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal2()
  {
    entities.LegalAction.Populated = false;
    entities.Tribunal.Populated = false;

    return ReadEach("ReadLegalActionTribunal2",
      null,
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.Classification = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 10);
        entities.Tribunal.Identifier = db.GetInt32(reader, 10);
        entities.LegalAction.KpcStandardNo = db.GetNullableString(reader, 11);
        entities.LegalAction.KpcFlag = db.GetNullableString(reader, 12);
        entities.LegalAction.KpcDate = db.GetNullableDate(reader, 13);
        entities.LegalAction.KpcStdNoChgFlag = db.GetNullableString(reader, 14);
        entities.LegalAction.KpcTribunalId = db.GetNullableInt32(reader, 15);
        entities.LegalAction.Populated = true;
        entities.Tribunal.Populated = true;

        return true;
      });
  }

  private void UpdateLegalAction1()
  {
    var lastUpdatedBy = "SWEFB691";
    var lastUpdatedTstamp = Now();
    var kpcStandardNo = entities.LegalAction.StandardNumber;
    var kpcFlag = "Y";
    var kpcDate = local.CurrentRun.Date;
    var kpcTribunalId = entities.Tribunal.Identifier;

    entities.LegalAction.Populated = false;
    Update("UpdateLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "kpcStandardNo", kpcStandardNo);
        db.SetNullableString(command, "kpcFlag", kpcFlag);
        db.SetNullableDate(command, "kpcDate", kpcDate);
        db.SetNullableInt32(command, "kpcTribunalId", kpcTribunalId);
        db.SetInt32(command, "legalActionId", entities.LegalAction.Identifier);
      });

    entities.LegalAction.LastUpdatedBy = lastUpdatedBy;
    entities.LegalAction.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.LegalAction.KpcFlag = kpcFlag;
    entities.LegalAction.KpcDate = kpcDate;
    entities.LegalAction.KpcTribunalId = kpcTribunalId;
    entities.LegalAction.Populated = true;
  }

  private void UpdateLegalAction2()
  {
    var lastUpdatedBy = "SWEFB691";
    var lastUpdatedTstamp = Now();
    var kpcStandardNo = entities.LegalAction.StandardNumber;
    var kpcFlag = "Y";
    var kpcDate = entities.Old.KpcDate;
    var kpcTribunalId = entities.Tribunal.Identifier;

    entities.LegalAction.Populated = false;
    Update("UpdateLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "kpcStandardNo", kpcStandardNo);
        db.SetNullableString(command, "kpcFlag", kpcFlag);
        db.SetNullableDate(command, "kpcDate", kpcDate);
        db.SetNullableInt32(command, "kpcTribunalId", kpcTribunalId);
        db.SetInt32(command, "legalActionId", entities.LegalAction.Identifier);
      });

    entities.LegalAction.LastUpdatedBy = lastUpdatedBy;
    entities.LegalAction.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.LegalAction.KpcFlag = kpcFlag;
    entities.LegalAction.KpcDate = kpcDate;
    entities.LegalAction.KpcTribunalId = kpcTribunalId;
    entities.LegalAction.Populated = true;
  }

  private void UpdateLegalAction3()
  {
    var lastUpdatedBy = "SWEFB691";
    var lastUpdatedTstamp = Now();
    var kpcStandardNo = entities.LegalAction.StandardNumber;
    var kpcStdNoChgFlag = "N";
    var kpcTribunalId = entities.Tribunal.Identifier;

    entities.LegalAction.Populated = false;
    Update("UpdateLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "kpcStandardNo", kpcStandardNo);
        db.SetNullableString(command, "kpcStdNoChgFlg", kpcStdNoChgFlag);
        db.SetNullableInt32(command, "kpcTribunalId", kpcTribunalId);
        db.SetInt32(command, "legalActionId", entities.LegalAction.Identifier);
      });

    entities.LegalAction.LastUpdatedBy = lastUpdatedBy;
    entities.LegalAction.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.LegalAction.KpcStdNoChgFlag = kpcStdNoChgFlag;
    entities.LegalAction.KpcTribunalId = kpcTribunalId;
    entities.LegalAction.Populated = true;
  }

  private void UpdateLegalActionDetail()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);

    var lastUpdatedBy = local.HeaderRecord.Userid;
    var lastUpdatedTstamp = Now();
    var kpcDate = local.CurrentRun.Date;

    entities.LegalActionDetail.Populated = false;
    Update("UpdateLegalActionDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableDate(command, "kpcDate", kpcDate);
        db.SetInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetInt32(command, "laDetailNo", entities.LegalActionDetail.Number);
      });

    entities.LegalActionDetail.LastUpdatedBy = lastUpdatedBy;
    entities.LegalActionDetail.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.LegalActionDetail.KpcDate = kpcDate;
    entities.LegalActionDetail.Populated = true;
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
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecord HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
    }

    /// <summary>
    /// A value of FileCount.
    /// </summary>
    [JsonPropertyName("fileCount")]
    public Common FileCount
    {
      get => fileCount ??= new();
      set => fileCount = value;
    }

    private HeaderRecord headerRecord;
    private Common fileCount;
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
    /// A value of LegalActionAlreadySent.
    /// </summary>
    [JsonPropertyName("legalActionAlreadySent")]
    public Common LegalActionAlreadySent
    {
      get => legalActionAlreadySent ??= new();
      set => legalActionAlreadySent = value;
    }

    /// <summary>
    /// A value of RcoRecsWritten.
    /// </summary>
    [JsonPropertyName("rcoRecsWritten")]
    public Common RcoRecsWritten
    {
      get => rcoRecsWritten ??= new();
      set => rcoRecsWritten = value;
    }

    /// <summary>
    /// A value of NcoRecsWritten.
    /// </summary>
    [JsonPropertyName("ncoRecsWritten")]
    public Common NcoRecsWritten
    {
      get => ncoRecsWritten ??= new();
      set => ncoRecsWritten = value;
    }

    /// <summary>
    /// A value of NdbtRecsWritten.
    /// </summary>
    [JsonPropertyName("ndbtRecsWritten")]
    public Common NdbtRecsWritten
    {
      get => ndbtRecsWritten ??= new();
      set => ndbtRecsWritten = value;
    }

    /// <summary>
    /// A value of TotalRecsWritten.
    /// </summary>
    [JsonPropertyName("totalRecsWritten")]
    public Common TotalRecsWritten
    {
      get => totalRecsWritten ??= new();
      set => totalRecsWritten = value;
    }

    /// <summary>
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public CourtOrderRecord Default1
    {
      get => default1 ??= new();
      set => default1 = value;
    }

    /// <summary>
    /// A value of LowDate.
    /// </summary>
    [JsonPropertyName("lowDate")]
    public DateWorkArea LowDate
    {
      get => lowDate ??= new();
      set => lowDate = value;
    }

    /// <summary>
    /// A value of FileCount.
    /// </summary>
    [JsonPropertyName("fileCount")]
    public Common FileCount
    {
      get => fileCount ??= new();
      set => fileCount = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of KpcHighDate.
    /// </summary>
    [JsonPropertyName("kpcHighDate")]
    public DateWorkArea KpcHighDate
    {
      get => kpcHighDate ??= new();
      set => kpcHighDate = value;
    }

    /// <summary>
    /// A value of Find.
    /// </summary>
    [JsonPropertyName("find")]
    public Common Find
    {
      get => find ??= new();
      set => find = value;
    }

    /// <summary>
    /// A value of RunInTestMode.
    /// </summary>
    [JsonPropertyName("runInTestMode")]
    public Common RunInTestMode
    {
      get => runInTestMode ??= new();
      set => runInTestMode = value;
    }

    /// <summary>
    /// A value of LastRun.
    /// </summary>
    [JsonPropertyName("lastRun")]
    public DateWorkArea LastRun
    {
      get => lastRun ??= new();
      set => lastRun = value;
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
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
    }

    /// <summary>
    /// A value of KpcExternalParms.
    /// </summary>
    [JsonPropertyName("kpcExternalParms")]
    public KpcExternalParms KpcExternalParms
    {
      get => kpcExternalParms ??= new();
      set => kpcExternalParms = value;
    }

    /// <summary>
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecord HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
    }

    /// <summary>
    /// A value of CourtOrderRecord.
    /// </summary>
    [JsonPropertyName("courtOrderRecord")]
    public CourtOrderRecord CourtOrderRecord
    {
      get => courtOrderRecord ??= new();
      set => courtOrderRecord = value;
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
    /// A value of BlankDate.
    /// </summary>
    [JsonPropertyName("blankDate")]
    public DateWorkArea BlankDate
    {
      get => blankDate ??= new();
      set => blankDate = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public CourtOrderRecord Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public KpcExternalParms Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public KpcExternalParms Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of PrintFileRecord.
    /// </summary>
    [JsonPropertyName("printFileRecord")]
    public PrintFileRecord PrintFileRecord
    {
      get => printFileRecord ??= new();
      set => printFileRecord = value;
    }

    private Common legalActionAlreadySent;
    private Common rcoRecsWritten;
    private Common ncoRecsWritten;
    private Common ndbtRecsWritten;
    private Common totalRecsWritten;
    private CourtOrderRecord default1;
    private DateWorkArea lowDate;
    private Common fileCount;
    private DateWorkArea maxDate;
    private DateWorkArea kpcHighDate;
    private Common find;
    private Common runInTestMode;
    private DateWorkArea lastRun;
    private DateWorkArea currentRun;
    private Common closeInd;
    private KpcExternalParms kpcExternalParms;
    private HeaderRecord headerRecord;
    private CourtOrderRecord courtOrderRecord;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea blankDate;
    private CourtOrderRecord old;
    private KpcExternalParms return1;
    private KpcExternalParms send;
    private PrintFileRecord printFileRecord;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public LegalAction Old
    {
      get => old ??= new();
      set => old = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalAction old;
    private Obligation obligation;
    private Tribunal tribunal;
  }
#endregion
}
