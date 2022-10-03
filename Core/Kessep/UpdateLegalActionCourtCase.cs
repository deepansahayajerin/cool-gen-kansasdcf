// Program: UPDATE_LEGAL_ACTION_COURT_CASE, ID: 373551695, model: 746.
// Short name: SWE01036
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
/// A program: UPDATE_LEGAL_ACTION_COURT_CASE.
/// </para>
/// <para>
/// This is used to update the legal action court case number
/// </para>
/// </summary>
[Serializable]
public partial class UpdateLegalActionCourtCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_LEGAL_ACTION_COURT_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateLegalActionCourtCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateLegalActionCourtCase.
  /// </summary>
  public UpdateLegalActionCourtCase(IContext context, Import import,
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
    // ------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ---------- 	------------	
    // --------------------------------------------------------------
    // 01/11/00  R. Jean	PR81987		New CAB to update only the court case number, 
    // tribunal, and
    // 					the standard number.
    // 05/09/00  J. Magat	PR94087		Added logic to update MONTHLY_COURT_ORDER_FEE
    // with the
    // 					changed court case number.
    // 07/24/00  J. Magat	PR93654		1. In PrAD, the "apply changes to like legal 
    // actions" flag
    // 					   is always set to "Y".
    // 					2. Added cash_receipts automatic adjustment and update with
    // 					   the change of standard number.
    // 					3. Use direct relationship between obligation and
    // 					   legal_action (as per Alan D.)
    // 07/24/00  J. Magat	PR99985		Change in Standard number and tribunal id 
    // should be applied
    // 					to Legal Referral as well.
    // 08/04/00  GVandy	PR 101045	Ensure that the last_updated_timestamp is 
    // unique on the
    // 					cash_receipt_detail record.
    // 08/24/00  GVandy	PR101722,	Remove edit preventing tribunal changes if 
    // there there are
    // 			PR101780	existing collections.
    // 08/30/00  GVandy	PR 102554	Re-coded per new business rules.
    // 11/13/00  GVandy	WR 209		Re-coded again per new business rules.
    // 04/02/01  GVandy	PR 108247	Use standard number when updating monthly 
    // court order fees.
    // 08/21/01  GVandy	WR 10346	New rules for updating 'B' class actions verses
    // non 'B' class actions.
    // 06/21/02  GVandy	PR 00128461	Don't read for FIPS if interstate request is
    // from a foreign country.
    // 03/05/03  GVandy	PR 172386	Flag legal actions where only the tribunal is 
    // changed so they
    // 					are picked up by the KPC extract.
    // ------------------------------------------------------------------------------------------------------
    // 02/13/09    AHockman       cq 962 old pr229836  added qualifier in read 
    // each of interstate request to make sure that we actually have a case with
    // the other state before we create a transaction.   We needed to be
    // checking for KS case_ind not set to spaces.  We were sending these on CSI
    // and Quick locate only cases that we had never had an actual case with
    // the other state.   Plus for some reason multiples of these are being
    // created per court order key change rather than just one.   That is being
    // optimized by changes I am making in si_create_is_request_history.
    // 
    // ----------------------------------------------------------------------------------------------------
    // 06/22/10  R.Mathews  Moved read for duplicate csenet transactions into 
    // this action block
    // prior to calling si_create_is_request_history
    // -------------------------------------------------------------------------------------------
    local.CurrentDateWorkArea.Date = Now().Date;

    if (!ReadTribunal1())
    {
      ExitState = "LE0000_TRIBUNAL_NF";

      return;
    }

    if (!ReadFips1())
    {
      if (!ReadFipsTribAddress1())
      {
        ExitState = "CO0000_FIPS_TRIB_ADDR_NF_DB_ERRR";

        return;
      }
    }

    if (import.Tribunal.Identifier == import.PriorTribunal.Identifier)
    {
      local.NewFipsTribAddress.Country = entities.FipsTribAddress.Country;
      local.NewFips.Assign(entities.Fips);
    }
    else
    {
      if (!ReadTribunal2())
      {
        ExitState = "LE0000_TRIBUNAL_NF";

        return;
      }

      if (ReadFips3())
      {
        local.NewFips.Assign(entities.NewFips);
      }
      else if (ReadFipsTribAddress2())
      {
        local.NewFipsTribAddress.Country = entities.NewFipsTribAddress.Country;
      }
      else
      {
        ExitState = "CO0000_FIPS_TRIB_ADDR_NF_DB_ERRR";

        return;
      }
    }

    // ***********************************************************************
    // Update all legal actions under the old tribunal and court case
    // number with the new tribunal, court case number, and
    // standard number.  Set the court order key change date to
    // the current date.  If the standard number had previously
    // been reported to the KPC then set the
    // KPC_std_no_chg_flag to Y.
    // ***********************************************************************
    foreach(var item in ReadLegalAction())
    {
      if (AsChar(import.LegalAction.Classification) == 'B' && AsChar
        (entities.LegalAction.Classification) != 'B')
      {
        // -- The user is key changing a 'B' class legal action.
        // Only update 'B' class legal actions for the court case.
        continue;
      }

      if (AsChar(import.LegalAction.Classification) != 'B' && AsChar
        (entities.LegalAction.Classification) == 'B')
      {
        export.NoBClassUpdated.Flag = "Y";

        // -- The user is key changing a non 'B' class legal action.
        // Only update non 'B' class legal actions for the court case.
        continue;
      }

      if (IsEmpty(import.PriorLegalAction.CourtCaseNumber) && import
        .PriorLegalAction.Identifier != entities.LegalAction.Identifier)
      {
        // -- If the previous court case number was blank then only update the 
        // original legal action.  Do NOT update all legal actions with blank
        // court case numbers.
        continue;
      }

      local.CurrentDateWorkArea.Timestamp = Now();
      local.LegalAction.KeyChangeDate = Now().Date;

      if (!Equal(entities.LegalAction.KpcDate, local.NullDateWorkArea.Date) && (
        !Equal(import.LegalAction.StandardNumber,
        import.PriorLegalAction.StandardNumber) || import
        .Tribunal.Identifier != import.PriorTribunal.Identifier))
      {
        local.LegalAction.KpcStdNoChgFlag = "Y";
      }
      else
      {
        local.LegalAction.KpcStdNoChgFlag =
          entities.LegalAction.KpcStdNoChgFlag;
      }

      try
      {
        UpdateLegalAction();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (import.Tribunal.Identifier != import.PriorTribunal.Identifier)
      {
        DisassociateLegalAction();
        AssociateLegalAction();
      }

      // ***********************************************************************
      // Create a history record documenting the changes for one AP
      // on each case associated to the legal action.  
      // ***********************************************************************
      local.Previous.Number = "";

      foreach(var item1 in ReadAbsentParentCase())
      {
        if (Equal(entities.Case1.Number, local.Previous.Number))
        {
          // -- Only create a history record for one AP on each case.
          continue;
        }

        local.Previous.Number = entities.Case1.Number;
        local.Infrastructure.Assign(local.NullInfrastructure);
        local.Infrastructure.CsenetInOutCode = "";
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.UserId = "LACC";
        local.Infrastructure.BusinessObjectCd = "LEA";
        local.Infrastructure.DenormNumeric12 = entities.LegalAction.Identifier;
        local.Infrastructure.DenormText12 =
          entities.LegalAction.CourtCaseNumber;
        local.Infrastructure.ReferenceDate = entities.LegalAction.KeyChangeDate;
        local.Infrastructure.Detail = "Changed";
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " " +
          NumberToString(import.PriorTribunal.Identifier, 12, 4);
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + ", " +
          (import.PriorLegalAction.CourtCaseNumber ?? "");
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + ", " +
          Substring(import.PriorLegalAction.StandardNumber, 20, 1, 12);
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " to " +
          NumberToString(import.Tribunal.Identifier, 12, 4);
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + ", " +
          (import.LegalAction.CourtCaseNumber ?? "");
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + ", " +
          Substring(import.LegalAction.StandardNumber, 20, 1, 12);

        if (!IsEmpty(entities.LegalAction.InitiatingState) && !
          Equal(entities.LegalAction.InitiatingState, "KS"))
        {
          local.Infrastructure.InitiatingStateCode = "OS";
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }

        local.Infrastructure.EventId = 106;
        local.Infrastructure.ReasonCode = "LACTKEYCH";
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        if (ReadCaseUnit())
        {
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        }
        else
        {
          // -- Continue.  The relationship from case to case unit is not 
          // mandatory.
        }

        if (ReadCsePerson())
        {
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
        }
        else
        {
          ExitState = "CSE_PERSON_NF";

          return;
        }

        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(import.PriorLegalAction.CourtCaseNumber,
          import.LegalAction.CourtCaseNumber))
        {
          continue;
        }
        else
        {
          // **********************************************************************
          // Continue.  If the court case number is changed and the case is
          // interstate then send a CSENet transaction to the other state.
          // **********************************************************************
        }

        // ***********************************************************************
        // If there are interstate cases associated to the legal action
        // and Kansas has a CSENet agreement with the
        // corresponding state then send a miscellaneous CSENet
        // transaction to the other state indicating the court order
        // number change.
        // ***********************************************************************
        // CQ962 change made in read each of interstate request adding a 
        // qualifier to look
        //  at KS_case_ind  not equal to spaces.   That field is set to Y for 
        // outgoing or N
        // for incoming cases.  If it is not Y or N then it is not a true 
        // interstate case and
        // there is only a record in interstate request table because we have 
        // had a Lo1,
        // CSI or MSC only type of transaction.  We don't want to send things to
        // other
        // states on those types.    Anita  Hockman    2-13-09   
        // ******************************************************************************
        foreach(var item2 in ReadInterstateRequest())
        {
          if (entities.InterstateRequest.OtherStateFips == 0)
          {
            // 06/21/02  GVandy  PR 00128461  Don't read for FIPS if interstate 
            // request is from a foreign country.
            continue;
          }

          if (ReadFips2())
          {
            local.CsenetStateTable.StateCode =
              entities.Interstate.StateAbbreviation;
          }
          else
          {
            ExitState = "FIPS_NF";

            return;
          }

          UseSiReadCsenetStateTable();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (AsChar(local.CsenetStateTable.CsenetReadyInd) == 'Y' && AsChar
            (local.CsenetStateTable.RecStateInd) == 'Y')
          {
            local.InterstateRequestHistory.Assign(
              local.NullInterstateRequestHistory);
            local.InterstateRequestHistory.Note =
              "Court order number changed from " + TrimEnd
              (import.PriorLegalAction.CourtCaseNumber) + " to " + (
                import.LegalAction.CourtCaseNumber ?? "");
            local.InterstateRequestHistory.TransactionDate =
              local.CurrentDateWorkArea.Date;
            local.InterstateRequestHistory.TransactionDirectionInd = "O";
            local.InterstateRequestHistory.FunctionalTypeCode = "MSC";
            local.InterstateRequestHistory.ActionCode = "P";
            local.InterstateRequestHistory.ActionReasonCode = "GSUPD";

            // 06/22/10  R.Mathews  Check if court order change history record 
            // has already been written
            // for this interstate request
            if (ReadInterstateRequestHistory())
            {
              goto Test;
            }

            UseSiCreateIsRequestHistory();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              return;
            }

            local.InterstateCase.InformationInd = 1;
            local.InterstateCase.CaseDataInd = 1;
            UseSiCreateOgCsenetCaseDb();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else if (IsExitState("SI0000_CSENET_DEACT_STATUS_RB"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              return;
            }
          }

Test:
          ;
        }
      }
    }

    if (AsChar(import.LegalAction.Classification) == 'B')
    {
      // -- For 'B' class legal actions, do not update legal referrals,
      // monthly court order fees, person private attorneys, marriage
      // history, cash receipt details, or collections.  Escape,
      // we're done.
      return;
    }

    // ***********************************************************************
    // Update all legal referrals under the old tribunal and court
    // case number with the new tribunal and court case number.
    // ***********************************************************************
    if ((!Equal(
      import.LegalAction.CourtCaseNumber,
      import.PriorLegalAction.CourtCaseNumber) || import
      .Tribunal.Identifier != import.PriorTribunal.Identifier) && !
      IsEmpty(import.PriorLegalAction.CourtCaseNumber))
    {
      foreach(var item in ReadLegalReferral())
      {
        try
        {
          UpdateLegalReferral();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LEGAL_REFERRAL_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    // ***********************************************************************
    // Update all monthly court order fees from the old standard
    // number to the new standard number.
    // ***********************************************************************
    if (!Equal(import.PriorLegalAction.StandardNumber,
      import.LegalAction.StandardNumber) && !
      IsEmpty(import.PriorLegalAction.StandardNumber))
    {
      // *** Change in Legal_Action Standard_Number occurred.
      foreach(var item in ReadMonthlyCourtOrderFeeObligee())
      {
        // *** Create a replacement.
        local.CurrentDateWorkArea.Timestamp = Now();

        try
        {
          CreateMonthlyCourtOrderFee();

          // *** Delete original.
          DeleteMonthlyCourtOrderFee();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              // *** Not likely to happen.
              // If it does, keep the most recent.
              if (ReadMonthlyCourtOrderFee())
              {
                // *** Determine most recent date: create or update date for the
                // current entry.
                local.CurrentMonthlyCourtOrderFee.LastUpdatedTmst =
                  entities.MonthlyCourtOrderFee.CreatedTimestamp;

                if (Lt(local.CurrentMonthlyCourtOrderFee.LastUpdatedTmst,
                  entities.MonthlyCourtOrderFee.LastUpdatedTmst))
                {
                  local.CurrentMonthlyCourtOrderFee.LastUpdatedTmst =
                    entities.MonthlyCourtOrderFee.LastUpdatedTmst;
                }

                // *** Determine most recent date: create or update date for the
                // duplicate entry.
                local.Duplicate.LastUpdatedTmst =
                  entities.Replacement.CreatedTimestamp;

                if (Lt(local.Duplicate.LastUpdatedTmst,
                  entities.Replacement.LastUpdatedTmst))
                {
                  local.Duplicate.LastUpdatedTmst =
                    entities.Replacement.LastUpdatedTmst;
                }

                // *** Determine which is more recent.
                if (!Lt(local.Duplicate.LastUpdatedTmst,
                  local.CurrentMonthlyCourtOrderFee.LastUpdatedTmst))
                {
                  // *** Duplicate entry is more recent... update only timestamp
                  // and updated by.
                  local.CurrentDateWorkArea.Timestamp = Now();

                  try
                  {
                    UpdateMonthlyCourtOrderFee2();
                  }
                  catch(Exception e1)
                  {
                    switch(GetErrorCode(e1))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "LE0000_MTHLY_COURT_ORDR_FEE_NU";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
                else
                {
                  // *** Current entry is more recent... update with "Current" 
                  // values.
                  local.CurrentDateWorkArea.Timestamp = Now();

                  try
                  {
                    UpdateMonthlyCourtOrderFee1();
                  }
                  catch(Exception e1)
                  {
                    switch(GetErrorCode(e1))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "LE0000_MTHLY_COURT_ORDR_FEE_NU";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
              }
              else
              {
                ExitState = "LE0000_MTHLY_COURT_ORDR_FEE_NF";

                return;
              }

              // *** Delete original.
              DeleteMonthlyCourtOrderFee();

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    if ((!Equal(
      import.LegalAction.CourtCaseNumber,
      import.PriorLegalAction.CourtCaseNumber) || import
      .Tribunal.Identifier != import.PriorTribunal.Identifier) && !
      IsEmpty(import.PriorLegalAction.CourtCaseNumber))
    {
      // ***********************************************************************
      // Update private attorney information from the old tribunal and
      // court case number to the new tribunal and court case
      // number.
      // ***********************************************************************
      if (entities.Fips.Populated)
      {
        foreach(var item in ReadPersonPrivateAttorney1())
        {
          try
          {
            UpdatePersonPrivateAttorney();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PERSONS_ATTORNEY_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else if (entities.FipsTribAddress.Populated)
      {
        foreach(var item in ReadPersonPrivateAttorney2())
        {
          try
          {
            UpdatePersonPrivateAttorney();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PERSONS_ATTORNEY_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      // ***********************************************************************
      // Update marriage history information from the old tribunal and
      // court case number to the new tribunal and court case
      // number.
      // ***********************************************************************
      local.MarriageHistory.DivorceCounty =
        Substring(entities.Fips.CountyDescription, 1, 15);
      local.NewMarriageHistory.DivorceCounty =
        Substring(local.NewFips.CountyDescription, 1, 15);

      if (entities.Fips.Populated)
      {
        foreach(var item in ReadMarriageHistory1())
        {
          try
          {
            UpdateMarriageHistory();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0017_ERR_UPD_MARRIAGE_HIST";

                return;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else if (entities.FipsTribAddress.Populated)
      {
        foreach(var item in ReadMarriageHistory2())
        {
          try
          {
            UpdateMarriageHistory();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0017_ERR_UPD_MARRIAGE_HIST";

                return;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }

    if (IsEmpty(import.PriorLegalAction.StandardNumber))
    {
      // -- Do not have to update collections if the previous standard number 
      // was spaces.  We do NOT want to change all collections with standard
      // number of spaces to the new standard number.
      return;
    }

    // ***  Determine:  1. if there are collections for the old standard number
    // ***              2. the oldest collection date for those collections
    ReadCollection2();

    if (local.OldCollection.Count > 0)
    {
      // *** Collections exist under the old standard number.  ***
      // ***********************************************************************
      // Update cash receipt details from the old standard number to
      // the new standard number.  Also create a cash receipt detail
      // history record to log the change.
      // ***********************************************************************
      foreach(var item in ReadCashReceiptDetail())
      {
        local.CashReceiptDetail.Assign(entities.CashReceiptDetail);

        if (!ReadCollectionType())
        {
          continue;
        }

        if (!ReadCashReceiptCashReceiptEventCashReceiptType())
        {
          continue;
        }

        local.CurrentDateWorkArea.Timestamp = Now();

        try
        {
          UpdateCashReceiptDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CASH_RCPT_DTL_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        local.CashReceiptDetailHistory.Attribute1SupportedPersonFirstName =
          local.CashReceiptDetail.Attribute1SupportedPersonFirstName ?? "";
        local.CashReceiptDetailHistory.Attribute1SupportedPersonLastName =
          local.CashReceiptDetail.Attribute1SupportedPersonLastName ?? "";
        local.CashReceiptDetailHistory.Attribute1SupportedPersonMiddleName =
          local.CashReceiptDetail.Attribute1SupportedPersonMiddleName ?? "";
        local.CashReceiptDetailHistory.Attribute2SupportedPersonFirstName =
          local.CashReceiptDetail.Attribute2SupportedPersonFirstName ?? "";
        local.CashReceiptDetailHistory.Attribute2SupportedPersonLastName =
          local.CashReceiptDetail.Attribute2SupportedPersonLastName ?? "";
        local.CashReceiptDetailHistory.Attribute2SupportedPersonMiddleName =
          local.CashReceiptDetail.Attribute2SupportedPersonMiddleName ?? "";
        local.CashReceiptDetailHistory.AdjustmentInd =
          local.CashReceiptDetail.AdjustmentInd ?? "";
        local.CashReceiptDetailHistory.CaseNumber =
          local.CashReceiptDetail.CaseNumber ?? "";
        local.CashReceiptDetailHistory.CollectionAmount =
          local.CashReceiptDetail.CollectionAmount;
        local.CashReceiptDetailHistory.CollectionAmtFullyAppliedInd =
          local.CashReceiptDetail.CollectionAmtFullyAppliedInd ?? "";
        local.CashReceiptDetailHistory.CollectionDate =
          local.CashReceiptDetail.CollectionDate;
        local.CashReceiptDetailHistory.CourtOrderNumber =
          local.CashReceiptDetail.CourtOrderNumber ?? "";
        local.CashReceiptDetailHistory.CreatedBy =
          local.CashReceiptDetail.CreatedBy;
        local.CashReceiptDetailHistory.CreatedTmst =
          local.CashReceiptDetail.CreatedTmst;
        local.CashReceiptDetailHistory.DefaultedCollectionDateInd =
          local.CashReceiptDetail.DefaultedCollectionDateInd ?? "";
        local.CashReceiptDetailHistory.DistributedAmount =
          local.CashReceiptDetail.DistributedAmount.GetValueOrDefault();
        local.CashReceiptDetailHistory.InterfaceTransId =
          local.CashReceiptDetail.InterfaceTransId ?? "";
        local.CashReceiptDetailHistory.JointReturnInd =
          local.CashReceiptDetail.JointReturnInd ?? "";
        local.CashReceiptDetailHistory.JointReturnName =
          local.CashReceiptDetail.JointReturnName ?? "";

        if (IsEmpty(local.CashReceiptDetail.LastUpdatedBy))
        {
          local.CashReceiptDetailHistory.LastUpdatedBy =
            local.CashReceiptDetail.CreatedBy;
          local.CashReceiptDetailHistory.LastUpdatedTmst =
            local.CashReceiptDetail.CreatedTmst;
        }
        else
        {
          local.CashReceiptDetailHistory.LastUpdatedBy =
            local.CashReceiptDetail.LastUpdatedBy ?? Spaces(8);
          local.CashReceiptDetailHistory.LastUpdatedTmst =
            local.CashReceiptDetail.LastUpdatedTmst;
        }

        local.CashReceiptDetailHistory.MultiPayor =
          local.CashReceiptDetail.MultiPayor ?? "";
        local.CashReceiptDetailHistory.Notes =
          local.CashReceiptDetail.Notes ?? "";
        local.CashReceiptDetailHistory.ObligorFirstName =
          local.CashReceiptDetail.ObligorFirstName ?? "";
        local.CashReceiptDetailHistory.ObligorLastName =
          local.CashReceiptDetail.ObligorLastName ?? "";
        local.CashReceiptDetailHistory.ObligorMiddleName =
          local.CashReceiptDetail.ObligorMiddleName ?? "";
        local.CashReceiptDetailHistory.ObligorPersonNumber =
          local.CashReceiptDetail.ObligorPersonNumber ?? "";
        local.CashReceiptDetailHistory.ObligorPhoneNumber =
          local.CashReceiptDetail.ObligorPhoneNumber ?? "";
        local.CashReceiptDetailHistory.ObligorSocialSecurityNumber =
          local.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
        local.CashReceiptDetailHistory.OffsetTaxYear =
          local.CashReceiptDetail.OffsetTaxYear.GetValueOrDefault();
        local.CashReceiptDetailHistory.OffsetTaxid =
          local.CashReceiptDetail.OffsetTaxid.GetValueOrDefault();
        local.CashReceiptDetailHistory.PayeeFirstName =
          local.CashReceiptDetail.PayeeFirstName ?? "";
        local.CashReceiptDetailHistory.PayeeLastName =
          local.CashReceiptDetail.PayeeLastName ?? "";
        local.CashReceiptDetailHistory.PayeeMiddleName =
          local.CashReceiptDetail.PayeeMiddleName ?? "";
        local.CashReceiptDetailHistory.ReceivedAmount =
          local.CashReceiptDetail.ReceivedAmount;
        local.CashReceiptDetailHistory.Reference =
          local.CashReceiptDetail.Reference ?? "";
        local.CashReceiptDetailHistory.RefundedAmount =
          local.CashReceiptDetail.RefundedAmount.GetValueOrDefault();
        local.CashReceiptDetailHistory.SequentialIdentifier =
          local.CashReceiptDetail.SequentialIdentifier;
        local.CashReceiptDetailHistory.CollectionTypeIdentifier =
          entities.CollectionType.SequentialIdentifier;
        local.CashReceiptDetailHistory.CashReceiptEventNumber =
          entities.CashReceiptEvent.SystemGeneratedIdentifier;
        local.CashReceiptDetailHistory.CashReceiptNumber =
          entities.CashReceipt.SequentialNumber;
        local.CashReceiptDetailHistory.CashReceiptSourceType =
          entities.CashReceiptSourceType.SystemGeneratedIdentifier;
        local.CashReceiptDetailHistory.CashReceiptType =
          entities.CashReceiptType.SystemGeneratedIdentifier;

        // *** Log "before change" Image.
        UseFnLogCashRcptDtlHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // ***  Determine:  1. if there are collections for the new standard 
      // number
      // ***              2. the oldest collection date for those collections
      ReadCollection1();

      if (local.NewCollection1.Count == 0)
      {
        // ***********************************************************************
        // There were no existing collections found for the new
        // standard number.  For each collection with the old standard
        // number, update the standard number to the new standard
        // number.
        // ***********************************************************************
        foreach(var item in ReadCollection3())
        {
          local.CurrentDateWorkArea.Timestamp = Now();

          // ***********************************************************************
          // Update collections from the old standard number to the new
          // standard number.
          // ***********************************************************************
          try
          {
            UpdateCollection();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_COLLECTION_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_COLLECTION_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else if (local.NewCollection1.Count > 0)
      {
        if (!Equal(import.PriorLegalAction.StandardNumber,
          import.LegalAction.StandardNumber))
        {
          ExitState = "LE0000_COLLECTIONS_EXIST_NO_UPD";
        }

        // ***********************************************************************
        // There is an edit in the PrAD that should not allow this
        // scenario to occur.
        // The logic below was left at Kit's request in case the decision
        // is changed so that they can update the legal actions even if
        // collections exist under both the old and new standard
        // number.  The logic below sets a trigger so that the
        // collections will be backed off and re-applied.  The trigger
        // logic was previously tested in both unit and acceptance
        // regions.
        // ***********************************************************************
      }
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.CaseDataInd = source.CaseDataInd;
    target.InformationInd = source.InformationInd;
  }

  private static void MoveInterstateRequest(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.TransactionDate = source.TransactionDate;
  }

  private void UseFnLogCashRcptDtlHistory()
  {
    var useImport = new FnLogCashRcptDtlHistory.Import();
    var useExport = new FnLogCashRcptDtlHistory.Export();

    useImport.CashReceiptDetailHistory.Assign(local.CashReceiptDetailHistory);

    Call(FnLogCashRcptDtlHistory.Execute, useImport, useExport);
  }

  private void UseSiCreateIsRequestHistory()
  {
    var useImport = new SiCreateIsRequestHistory.Import();
    var useExport = new SiCreateIsRequestHistory.Export();

    useImport.Ap.Number = entities.CsePerson.Number;
    MoveInterstateRequest(entities.InterstateRequest,
      useImport.InterstateRequest);
    useImport.Case1.Number = entities.Case1.Number;
    useImport.InterstateRequestHistory.Assign(local.InterstateRequestHistory);

    Call(SiCreateIsRequestHistory.Execute, useImport, useExport);

    local.InterstateRequestHistory.Assign(useExport.InterstateRequestHistory);
  }

  private void UseSiCreateOgCsenetCaseDb()
  {
    var useImport = new SiCreateOgCsenetCaseDb.Import();
    var useExport = new SiCreateOgCsenetCaseDb.Export();

    MoveInterstateRequest(entities.InterstateRequest,
      useImport.InterstateRequest);
    MoveCase1(entities.Case1, useImport.Case1);
    MoveInterstateRequestHistory(local.InterstateRequestHistory,
      useImport.InterstateRequestHistory);
    MoveInterstateCase(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetCaseDb.Execute, useImport, useExport);
  }

  private void UseSiReadCsenetStateTable()
  {
    var useImport = new SiReadCsenetStateTable.Import();
    var useExport = new SiReadCsenetStateTable.Export();

    useImport.CsenetStateTable.StateCode = local.CsenetStateTable.StateCode;

    Call(SiReadCsenetStateTable.Execute, useImport, useExport);

    local.CsenetStateTable.Assign(useExport.CsenetStateTable);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void AssociateLegalAction()
  {
    var trbId = entities.NewTribunal.Identifier;

    entities.LegalAction.Populated = false;
    Update("AssociateLegalAction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", trbId);
        db.SetInt32(command, "legalActionId", entities.LegalAction.Identifier);
      });

    entities.LegalAction.TrbId = trbId;
    entities.LegalAction.Populated = true;
  }

  private void CreateMonthlyCourtOrderFee()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);

    var cpaType = entities.Obligee.Type1;
    var cspNumber = entities.Obligee.CspNumber;
    var courtOrderNumber = import.LegalAction.StandardNumber ?? "";
    var yearMonth = entities.MonthlyCourtOrderFee.YearMonth;
    var amount = entities.MonthlyCourtOrderFee.Amount;
    var createdBy = entities.MonthlyCourtOrderFee.CreatedBy;
    var createdTimestamp = entities.MonthlyCourtOrderFee.CreatedTimestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.CurrentDateWorkArea.Timestamp;

    CheckValid<MonthlyCourtOrderFee>("CpaType", cpaType);
    entities.Replacement.Populated = false;
    Update("CreateMonthlyCourtOrderFee",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "courtOrderNumber", courtOrderNumber);
        db.SetInt32(command, "yearMonth", yearMonth);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
      });

    entities.Replacement.CpaType = cpaType;
    entities.Replacement.CspNumber = cspNumber;
    entities.Replacement.CourtOrderNumber = courtOrderNumber;
    entities.Replacement.YearMonth = yearMonth;
    entities.Replacement.Amount = amount;
    entities.Replacement.CreatedBy = createdBy;
    entities.Replacement.CreatedTimestamp = createdTimestamp;
    entities.Replacement.LastUpdatedBy = lastUpdatedBy;
    entities.Replacement.LastUpdatedTmst = lastUpdatedTmst;
    entities.Replacement.Populated = true;
  }

  private void DeleteMonthlyCourtOrderFee()
  {
    Update("DeleteMonthlyCourtOrderFee",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.MonthlyCourtOrderFee.CpaType);
        db.SetString(
          command, "cspNumber", entities.MonthlyCourtOrderFee.CspNumber);
        db.SetString(
          command, "courtOrderNumber",
          entities.MonthlyCourtOrderFee.CourtOrderNumber);
        db.SetInt32(
          command, "yearMonth", entities.MonthlyCourtOrderFee.YearMonth);
      });
  }

  private void DisassociateLegalAction()
  {
    entities.LegalAction.Populated = false;
    Update("DisassociateLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", entities.LegalAction.Identifier);
      });

    entities.LegalAction.TrbId = null;
    entities.LegalAction.Populated = true;
  }

  private IEnumerable<bool> ReadAbsentParentCase()
  {
    entities.Case1.Populated = false;
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadAbsentParentCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCashReceiptCashReceiptEventCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptType.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptEventCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceipt.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtOrderNumber",
          import.PriorLegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.OffsetTaxid = db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 9);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 10);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 11);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 13);
        entities.CashReceiptDetail.JointReturnInd =
          db.GetNullableString(reader, 14);
        entities.CashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 15);
        entities.CashReceiptDetail.DefaultedCollectionDateInd =
          db.GetNullableString(reader, 16);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 17);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 18);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 19);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 21);
        entities.CashReceiptDetail.ObligorPhoneNumber =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetail.PayeeFirstName =
          db.GetNullableString(reader, 23);
        entities.CashReceiptDetail.PayeeMiddleName =
          db.GetNullableString(reader, 24);
        entities.CashReceiptDetail.PayeeLastName =
          db.GetNullableString(reader, 25);
        entities.CashReceiptDetail.Attribute1SupportedPersonFirstName =
          db.GetNullableString(reader, 26);
        entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName =
          db.GetNullableString(reader, 27);
        entities.CashReceiptDetail.Attribute1SupportedPersonLastName =
          db.GetNullableString(reader, 28);
        entities.CashReceiptDetail.Attribute2SupportedPersonFirstName =
          db.GetNullableString(reader, 29);
        entities.CashReceiptDetail.Attribute2SupportedPersonLastName =
          db.GetNullableString(reader, 30);
        entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName =
          db.GetNullableString(reader, 31);
        entities.CashReceiptDetail.CreatedBy = db.GetString(reader, 32);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 33);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 34);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 35);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 36);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 37);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 38);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 39);
        entities.CashReceiptDetail.Reference = db.GetNullableString(reader, 40);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 41);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.CashReceiptDetail.JointReturnInd);
        CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
          entities.CashReceiptDetail.DefaultedCollectionDateInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadCollection1()
  {
    local.NewCollection2.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ctOrdAppliedTo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        local.NewCollection2.CollectionDt = db.GetDate(reader, 0);
        local.NewCollection1.Count = db.GetInt32(reader, 1);
        local.NewCollection2.Populated = true;
      });
  }

  private bool ReadCollection2()
  {
    local.Old.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ctOrdAppliedTo", import.PriorLegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        local.Old.CollectionDt = db.GetDate(reader, 0);
        local.OldCollection.Count = db.GetInt32(reader, 1);
        local.Old.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollection3()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ctOrdAppliedTo", import.PriorLegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 13);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 14);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 15);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.AbsentParent.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadFips1()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Interstate.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.
          SetInt32(command, "state", entities.InterstateRequest.OtherStateFips);
          
      },
      (db, reader) =>
      {
        entities.Interstate.State = db.GetInt32(reader, 0);
        entities.Interstate.County = db.GetInt32(reader, 1);
        entities.Interstate.Location = db.GetInt32(reader, 2);
        entities.Interstate.StateAbbreviation = db.GetString(reader, 3);
        entities.Interstate.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    System.Diagnostics.Debug.Assert(entities.NewTribunal.Populated);
    entities.NewFips.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.NewTribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.NewTribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.NewTribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NewFips.State = db.GetInt32(reader, 0);
        entities.NewFips.County = db.GetInt32(reader, 1);
        entities.NewFips.Location = db.GetInt32(reader, 2);
        entities.NewFips.CountyDescription = db.GetNullableString(reader, 3);
        entities.NewFips.StateAbbreviation = db.GetString(reader, 4);
        entities.NewFips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.NewFips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.NewFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.NewTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.NewFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.NewFipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.NewFipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.NewFipsTribAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequestHistory()
  {
    entities.InterstateRequestHistory.Populated = false;

    return Read("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetDate(
          command, "transactionDate",
          local.InterstateRequestHistory.TransactionDate.GetValueOrDefault());
        db.SetNullableString(
          command, "note", local.InterstateRequestHistory.Note ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 2);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 3);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 4);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 5);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 6);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 7);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.PriorLegalAction.CourtCaseNumber ?? ""
          );
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.LegalAction.KpcDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.KpcStdNoChgFlag = db.GetNullableString(reader, 10);
        entities.LegalAction.KeyChangeDate = db.GetNullableDate(reader, 11);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.PriorLegalAction.CourtCaseNumber ?? ""
          );
        db.SetNullableInt32(command, "trbId", import.PriorTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.LastUpdatedBy = db.GetString(reader, 2);
        entities.LegalReferral.LastUpdatedTimestamp = db.GetDateTime(reader, 3);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 5);
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMarriageHistory1()
  {
    entities.MarriageHistory.Populated = false;

    return ReadEach("ReadMarriageHistory1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNumber",
          import.PriorLegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "divorceState", entities.Fips.StateAbbreviation);
        db.SetNullableString(
          command, "divorceCounty", local.MarriageHistory.DivorceCounty ?? "");
      },
      (db, reader) =>
      {
        entities.MarriageHistory.CspRNumber = db.GetString(reader, 0);
        entities.MarriageHistory.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 1);
        entities.MarriageHistory.DivorceCounty =
          db.GetNullableString(reader, 2);
        entities.MarriageHistory.DivorceState = db.GetNullableString(reader, 3);
        entities.MarriageHistory.DivorceCountry =
          db.GetNullableString(reader, 4);
        entities.MarriageHistory.LastUpdatedBy = db.GetString(reader, 5);
        entities.MarriageHistory.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.MarriageHistory.Identifier = db.GetInt32(reader, 7);
        entities.MarriageHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMarriageHistory2()
  {
    entities.MarriageHistory.Populated = false;

    return ReadEach("ReadMarriageHistory2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNumber",
          import.PriorLegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "divorceCountry", entities.FipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.MarriageHistory.CspRNumber = db.GetString(reader, 0);
        entities.MarriageHistory.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 1);
        entities.MarriageHistory.DivorceCounty =
          db.GetNullableString(reader, 2);
        entities.MarriageHistory.DivorceState = db.GetNullableString(reader, 3);
        entities.MarriageHistory.DivorceCountry =
          db.GetNullableString(reader, 4);
        entities.MarriageHistory.LastUpdatedBy = db.GetString(reader, 5);
        entities.MarriageHistory.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.MarriageHistory.Identifier = db.GetInt32(reader, 7);
        entities.MarriageHistory.Populated = true;

        return true;
      });
  }

  private bool ReadMonthlyCourtOrderFee()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);
    entities.Replacement.Populated = false;

    return Read("ReadMonthlyCourtOrderFee",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligee.Type1);
        db.SetString(command, "cspNumber", entities.Obligee.CspNumber);
        db.SetString(
          command, "courtOrderNumber", import.LegalAction.StandardNumber ?? ""
          );
        db.SetInt32(
          command, "yearMonth", entities.MonthlyCourtOrderFee.YearMonth);
      },
      (db, reader) =>
      {
        entities.Replacement.CpaType = db.GetString(reader, 0);
        entities.Replacement.CspNumber = db.GetString(reader, 1);
        entities.Replacement.CourtOrderNumber = db.GetString(reader, 2);
        entities.Replacement.YearMonth = db.GetInt32(reader, 3);
        entities.Replacement.Amount = db.GetDecimal(reader, 4);
        entities.Replacement.CreatedBy = db.GetString(reader, 5);
        entities.Replacement.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Replacement.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Replacement.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.Replacement.Populated = true;
        CheckValid<MonthlyCourtOrderFee>("CpaType", entities.Replacement.CpaType);
          
      });
  }

  private IEnumerable<bool> ReadMonthlyCourtOrderFeeObligee()
  {
    entities.Obligee.Populated = false;
    entities.MonthlyCourtOrderFee.Populated = false;

    return ReadEach("ReadMonthlyCourtOrderFeeObligee",
      (db, command) =>
      {
        db.SetString(
          command, "courtOrderNumber",
          import.PriorLegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.MonthlyCourtOrderFee.CpaType = db.GetString(reader, 0);
        entities.Obligee.Type1 = db.GetString(reader, 0);
        entities.MonthlyCourtOrderFee.CspNumber = db.GetString(reader, 1);
        entities.Obligee.CspNumber = db.GetString(reader, 1);
        entities.MonthlyCourtOrderFee.CourtOrderNumber =
          db.GetString(reader, 2);
        entities.MonthlyCourtOrderFee.YearMonth = db.GetInt32(reader, 3);
        entities.MonthlyCourtOrderFee.Amount = db.GetDecimal(reader, 4);
        entities.MonthlyCourtOrderFee.CreatedBy = db.GetString(reader, 5);
        entities.MonthlyCourtOrderFee.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.MonthlyCourtOrderFee.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.MonthlyCourtOrderFee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.Obligee.Populated = true;
        entities.MonthlyCourtOrderFee.Populated = true;
        CheckValid<MonthlyCourtOrderFee>("CpaType",
          entities.MonthlyCourtOrderFee.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.Obligee.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonPrivateAttorney1()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return ReadEach("ReadPersonPrivateAttorney1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNumber",
          import.PriorLegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "fipsStateAbbrev", entities.Fips.StateAbbreviation);
        db.SetNullableString(
          command, "fipsCountyAbbrev", entities.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.LastUpdatedBy = db.GetString(reader, 2);
        entities.PersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.PersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 5);
        entities.PersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 6);
        entities.PersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 7);
        entities.PersonPrivateAttorney.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonPrivateAttorney2()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return ReadEach("ReadPersonPrivateAttorney2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNumber",
          import.PriorLegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "tribCountry", entities.FipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.LastUpdatedBy = db.GetString(reader, 2);
        entities.PersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.PersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 5);
        entities.PersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 6);
        entities.PersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 7);
        entities.PersonPrivateAttorney.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.PriorTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    entities.NewTribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.NewTribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.NewTribunal.Identifier = db.GetInt32(reader, 1);
        entities.NewTribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.NewTribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.NewTribunal.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var courtOrderNumber = import.LegalAction.StandardNumber ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.CurrentDateWorkArea.Timestamp;

    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.CurrentDateWorkArea.Timestamp;
    var courtOrderAppliedTo = import.LegalAction.StandardNumber ?? "";

    entities.Collection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "ctOrdAppliedTo", courtOrderAppliedTo);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otrId", entities.Collection.OtrId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
      });

    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.CourtOrderAppliedTo = courtOrderAppliedTo;
    entities.Collection.Populated = true;
  }

  private void UpdateLegalAction()
  {
    var courtCaseNumber = import.LegalAction.CourtCaseNumber ?? "";
    var standardNumber = import.LegalAction.StandardNumber ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = local.CurrentDateWorkArea.Timestamp;
    var kpcStdNoChgFlag = local.LegalAction.KpcStdNoChgFlag ?? "";
    var keyChangeDate = local.LegalAction.KeyChangeDate;

    entities.LegalAction.Populated = false;
    Update("UpdateLegalAction",
      (db, command) =>
      {
        db.SetNullableString(command, "courtCaseNo", courtCaseNumber);
        db.SetNullableString(command, "standardNo", standardNumber);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "kpcStdNoChgFlg", kpcStdNoChgFlag);
        db.SetNullableDate(command, "keyChangeDate", keyChangeDate);
        db.SetInt32(command, "legalActionId", entities.LegalAction.Identifier);
      });

    entities.LegalAction.CourtCaseNumber = courtCaseNumber;
    entities.LegalAction.StandardNumber = standardNumber;
    entities.LegalAction.LastUpdatedBy = lastUpdatedBy;
    entities.LegalAction.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.LegalAction.KpcStdNoChgFlag = kpcStdNoChgFlag;
    entities.LegalAction.KeyChangeDate = keyChangeDate;
    entities.LegalAction.Populated = true;
  }

  private void UpdateLegalReferral()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var courtCaseNumber = import.LegalAction.CourtCaseNumber ?? "";
    var tribunalId = import.Tribunal.Identifier;

    entities.LegalReferral.Populated = false;
    Update("UpdateLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "courtCaseNo", courtCaseNumber);
        db.SetNullableInt32(command, "trbId", tribunalId);
        db.SetString(command, "casNumber", entities.LegalReferral.CasNumber);
        db.SetInt32(command, "identifier", entities.LegalReferral.Identifier);
      });

    entities.LegalReferral.LastUpdatedBy = lastUpdatedBy;
    entities.LegalReferral.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.LegalReferral.CourtCaseNumber = courtCaseNumber;
    entities.LegalReferral.TribunalId = tribunalId;
    entities.LegalReferral.Populated = true;
  }

  private void UpdateMarriageHistory()
  {
    System.Diagnostics.Debug.Assert(entities.MarriageHistory.Populated);

    var divorceCourtOrderNumber =
      Substring(import.LegalAction.CourtCaseNumber, 1, 15);
    var divorceCounty = local.NewMarriageHistory.DivorceCounty ?? "";
    var divorceState = local.NewFips.StateAbbreviation;
    var divorceCountry = local.NewFipsTribAddress.Country ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.MarriageHistory.Populated = false;
    Update("UpdateMarriageHistory",
      (db, command) =>
      {
        db.SetNullableString(command, "divCtordNo", divorceCourtOrderNumber);
        db.SetNullableString(command, "divorceCounty", divorceCounty);
        db.SetNullableString(command, "divorceState", divorceState);
        db.SetNullableString(command, "divorceCountry", divorceCountry);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.
          SetString(command, "cspRNumber", entities.MarriageHistory.CspRNumber);
          
        db.SetInt32(command, "identifier", entities.MarriageHistory.Identifier);
      });

    entities.MarriageHistory.DivorceCourtOrderNumber = divorceCourtOrderNumber;
    entities.MarriageHistory.DivorceCounty = divorceCounty;
    entities.MarriageHistory.DivorceState = divorceState;
    entities.MarriageHistory.DivorceCountry = divorceCountry;
    entities.MarriageHistory.LastUpdatedBy = lastUpdatedBy;
    entities.MarriageHistory.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.MarriageHistory.Populated = true;
  }

  private void UpdateMonthlyCourtOrderFee1()
  {
    System.Diagnostics.Debug.Assert(entities.Replacement.Populated);

    var amount = entities.MonthlyCourtOrderFee.Amount;
    var createdBy = entities.MonthlyCourtOrderFee.CreatedBy;
    var createdTimestamp = entities.MonthlyCourtOrderFee.CreatedTimestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.CurrentDateWorkArea.Timestamp;

    entities.Replacement.Populated = false;
    Update("UpdateMonthlyCourtOrderFee1",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "cpaType", entities.Replacement.CpaType);
        db.SetString(command, "cspNumber", entities.Replacement.CspNumber);
        db.SetString(
          command, "courtOrderNumber", entities.Replacement.CourtOrderNumber);
        db.SetInt32(command, "yearMonth", entities.Replacement.YearMonth);
      });

    entities.Replacement.Amount = amount;
    entities.Replacement.CreatedBy = createdBy;
    entities.Replacement.CreatedTimestamp = createdTimestamp;
    entities.Replacement.LastUpdatedBy = lastUpdatedBy;
    entities.Replacement.LastUpdatedTmst = lastUpdatedTmst;
    entities.Replacement.Populated = true;
  }

  private void UpdateMonthlyCourtOrderFee2()
  {
    System.Diagnostics.Debug.Assert(entities.Replacement.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.CurrentDateWorkArea.Timestamp;

    entities.Replacement.Populated = false;
    Update("UpdateMonthlyCourtOrderFee2",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "cpaType", entities.Replacement.CpaType);
        db.SetString(command, "cspNumber", entities.Replacement.CspNumber);
        db.SetString(
          command, "courtOrderNumber", entities.Replacement.CourtOrderNumber);
        db.SetInt32(command, "yearMonth", entities.Replacement.YearMonth);
      });

    entities.Replacement.LastUpdatedBy = lastUpdatedBy;
    entities.Replacement.LastUpdatedTmst = lastUpdatedTmst;
    entities.Replacement.Populated = true;
  }

  private void UpdatePersonPrivateAttorney()
  {
    System.Diagnostics.Debug.Assert(entities.PersonPrivateAttorney.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var courtCaseNumber = import.LegalAction.CourtCaseNumber ?? "";
    var fipsStateAbbreviation = local.NewFips.StateAbbreviation;
    var fipsCountyAbbreviation = local.NewFips.CountyAbbreviation ?? "";
    var tribCountry = local.NewFipsTribAddress.Country ?? "";

    entities.PersonPrivateAttorney.Populated = false;
    Update("UpdatePersonPrivateAttorney",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "courtCaseNumber", courtCaseNumber);
        db.SetNullableString(command, "fipsStateAbbrev", fipsStateAbbreviation);
        db.
          SetNullableString(command, "fipsCountyAbbrev", fipsCountyAbbreviation);
          
        db.SetNullableString(command, "tribCountry", tribCountry);
        db.SetString(
          command, "cspNumber", entities.PersonPrivateAttorney.CspNumber);
        db.SetInt32(
          command, "identifier", entities.PersonPrivateAttorney.Identifier);
      });

    entities.PersonPrivateAttorney.LastUpdatedBy = lastUpdatedBy;
    entities.PersonPrivateAttorney.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.PersonPrivateAttorney.CourtCaseNumber = courtCaseNumber;
    entities.PersonPrivateAttorney.FipsStateAbbreviation =
      fipsStateAbbreviation;
    entities.PersonPrivateAttorney.FipsCountyAbbreviation =
      fipsCountyAbbreviation;
    entities.PersonPrivateAttorney.TribCountry = tribCountry;
    entities.PersonPrivateAttorney.Populated = true;
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
    /// A value of PriorTribunal.
    /// </summary>
    [JsonPropertyName("priorTribunal")]
    public Tribunal PriorTribunal
    {
      get => priorTribunal ??= new();
      set => priorTribunal = value;
    }

    /// <summary>
    /// A value of PriorLegalAction.
    /// </summary>
    [JsonPropertyName("priorLegalAction")]
    public LegalAction PriorLegalAction
    {
      get => priorLegalAction ??= new();
      set => priorLegalAction = value;
    }

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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private Tribunal priorTribunal;
    private LegalAction priorLegalAction;
    private LegalAction legalAction;
    private Tribunal tribunal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NoBClassUpdated.
    /// </summary>
    [JsonPropertyName("noBClassUpdated")]
    public Common NoBClassUpdated
    {
      get => noBClassUpdated ??= new();
      set => noBClassUpdated = value;
    }

    private Common noBClassUpdated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("nullInterstateRequestHistory")]
    public InterstateRequestHistory NullInterstateRequestHistory
    {
      get => nullInterstateRequestHistory ??= new();
      set => nullInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
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

    /// <summary>
    /// A value of NullInfrastructure.
    /// </summary>
    [JsonPropertyName("nullInfrastructure")]
    public Infrastructure NullInfrastructure
    {
      get => nullInfrastructure ??= new();
      set => nullInfrastructure = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of NewMarriageHistory.
    /// </summary>
    [JsonPropertyName("newMarriageHistory")]
    public MarriageHistory NewMarriageHistory
    {
      get => newMarriageHistory ??= new();
      set => newMarriageHistory = value;
    }

    /// <summary>
    /// A value of MarriageHistory.
    /// </summary>
    [JsonPropertyName("marriageHistory")]
    public MarriageHistory MarriageHistory
    {
      get => marriageHistory ??= new();
      set => marriageHistory = value;
    }

    /// <summary>
    /// A value of NewFipsTribAddress.
    /// </summary>
    [JsonPropertyName("newFipsTribAddress")]
    public FipsTribAddress NewFipsTribAddress
    {
      get => newFipsTribAddress ??= new();
      set => newFipsTribAddress = value;
    }

    /// <summary>
    /// A value of NewFips.
    /// </summary>
    [JsonPropertyName("newFips")]
    public Fips NewFips
    {
      get => newFips ??= new();
      set => newFips = value;
    }

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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of NewCollection1.
    /// </summary>
    [JsonPropertyName("newCollection1")]
    public Common NewCollection1
    {
      get => newCollection1 ??= new();
      set => newCollection1 = value;
    }

    /// <summary>
    /// A value of NewCollection2.
    /// </summary>
    [JsonPropertyName("newCollection2")]
    public Collection NewCollection2
    {
      get => newCollection2 ??= new();
      set => newCollection2 = value;
    }

    /// <summary>
    /// A value of OldCollection.
    /// </summary>
    [JsonPropertyName("oldCollection")]
    public Common OldCollection
    {
      get => oldCollection ??= new();
      set => oldCollection = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public Collection Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of Duplicate.
    /// </summary>
    [JsonPropertyName("duplicate")]
    public MonthlyCourtOrderFee Duplicate
    {
      get => duplicate ??= new();
      set => duplicate = value;
    }

    /// <summary>
    /// A value of CurrentMonthlyCourtOrderFee.
    /// </summary>
    [JsonPropertyName("currentMonthlyCourtOrderFee")]
    public MonthlyCourtOrderFee CurrentMonthlyCourtOrderFee
    {
      get => currentMonthlyCourtOrderFee ??= new();
      set => currentMonthlyCourtOrderFee = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailHistory")]
    public CashReceiptDetailHistory CashReceiptDetailHistory
    {
      get => cashReceiptDetailHistory ??= new();
      set => cashReceiptDetailHistory = value;
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

    private InterstateRequestHistory nullInterstateRequestHistory;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateCase interstateCase;
    private CsenetStateTable csenetStateTable;
    private Common common;
    private Infrastructure nullInfrastructure;
    private Case1 previous;
    private Infrastructure infrastructure;
    private MarriageHistory newMarriageHistory;
    private MarriageHistory marriageHistory;
    private FipsTribAddress newFipsTribAddress;
    private Fips newFips;
    private LegalAction legalAction;
    private Collection collection;
    private Common newCollection1;
    private Collection newCollection2;
    private Common oldCollection;
    private Collection old;
    private DateWorkArea currentDateWorkArea;
    private DateWorkArea nullDateWorkArea;
    private MonthlyCourtOrderFee duplicate;
    private MonthlyCourtOrderFee currentMonthlyCourtOrderFee;
    private CashReceiptDetailHistory cashReceiptDetailHistory;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of Interstate.
    /// </summary>
    [JsonPropertyName("interstate")]
    public Fips Interstate
    {
      get => interstate ??= new();
      set => interstate = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of NewFipsTribAddress.
    /// </summary>
    [JsonPropertyName("newFipsTribAddress")]
    public FipsTribAddress NewFipsTribAddress
    {
      get => newFipsTribAddress ??= new();
      set => newFipsTribAddress = value;
    }

    /// <summary>
    /// A value of NewFips.
    /// </summary>
    [JsonPropertyName("newFips")]
    public Fips NewFips
    {
      get => newFips ??= new();
      set => newFips = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of MarriageHistory.
    /// </summary>
    [JsonPropertyName("marriageHistory")]
    public MarriageHistory MarriageHistory
    {
      get => marriageHistory ??= new();
      set => marriageHistory = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of NewTribunal.
    /// </summary>
    [JsonPropertyName("newTribunal")]
    public Tribunal NewTribunal
    {
      get => newTribunal ??= new();
      set => newTribunal = value;
    }

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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of MonthlyCourtOrderFee.
    /// </summary>
    [JsonPropertyName("monthlyCourtOrderFee")]
    public MonthlyCourtOrderFee MonthlyCourtOrderFee
    {
      get => monthlyCourtOrderFee ??= new();
      set => monthlyCourtOrderFee = value;
    }

    /// <summary>
    /// A value of Replacement.
    /// </summary>
    [JsonPropertyName("replacement")]
    public MonthlyCourtOrderFee Replacement
    {
      get => replacement ??= new();
      set => replacement = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
    private Fips interstate;
    private LegalActionPerson legalActionPerson;
    private CaseUnit caseUnit;
    private CsePerson csePerson;
    private InterstateRequest interstateRequest;
    private LegalActionCaseRole legalActionCaseRole;
    private Case1 case1;
    private CaseRole absentParent;
    private FipsTribAddress newFipsTribAddress;
    private Fips newFips;
    private FipsTribAddress fipsTribAddress;
    private Fips fips;
    private MarriageHistory marriageHistory;
    private PersonPrivateAttorney personPrivateAttorney;
    private CsePersonAccount supported;
    private ObligationTransaction debt;
    private CollectionType collectionType;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private Tribunal tribunal;
    private Tribunal newTribunal;
    private LegalAction legalAction;
    private CsePersonAccount obligee;
    private MonthlyCourtOrderFee monthlyCourtOrderFee;
    private MonthlyCourtOrderFee replacement;
    private LegalReferral legalReferral;
  }
#endregion
}
