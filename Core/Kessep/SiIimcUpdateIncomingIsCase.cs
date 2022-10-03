// Program: SI_IIMC_UPDATE_INCOMING_IS_CASE, ID: 372505529, model: 746.
// Short name: SWE02138
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
/// A program: SI_IIMC_UPDATE_INCOMING_IS_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB updates the Interstate Request which contains the basic information
/// for a referral for a specific Case and to a specific State.
/// </para>
/// </summary>
[Serializable]
public partial class SiIimcUpdateIncomingIsCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIMC_UPDATE_INCOMING_IS_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIimcUpdateIncomingIsCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIimcUpdateIncomingIsCase.
  /// </summary>
  public SiIimcUpdateIncomingIsCase(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------
    //                          M O D I F I C A T I O N     L O G
    // ---------------------------------------------------------------------------------------
    //   Date   Developer   PR#/WR#   Description
    // ---------------------------------------------------------------------------------------
    // 03/01/99 C Deghand             Per a request from the finance group, 
    // removed the code
    //                                
    // that updated person program
    // information.
    // 03/04/99 C Deghand             Modified the CAB to only update Interstate
    // Contacts,
    //                                
    // Interstate Contact Address and
    // Interstate Payment
    //                                
    // Address instead of end dating
    // the old and creating new.
    // 07/26/99 C Scroggins           Modified prad to update date to current 
    // date on Reopen,
    //                                
    // Close, or Open. Also protected
    // Case Status, Case Status
    //                                
    // Date, and Reason if case is
    // closed.
    // 03/22/00 C Scroggins 91087     Moved read of Interstate Contact Address 
    // nested within
    //        & C Ott                 successful read for Interstate Contact.
    // 06/01/00 C Scroggins 95574     Updated code to close each interstate 
    // request associated
    //                                
    // with the Interstate case when
    // closed.
    // 08/02/00 C Scroggins 99133     Implemented Business Rules for Duplicate 
    // Case Indicator.
    // 04/25/01 swsrchf     010337    Per work request added TWO new business 
    // rules for
    //                                
    // closing an Interstate case from
    // IIMC.
    //                                    
    // a) CLOSE the case, when AP has NO
    // money in Suspense.
    //                                    
    // b) CLOSE the case, when all debts
    // Deactivated.
    //                                
    // Both edits must be passed,
    // prior to Closing an
    //                                
    // Interstate case
    //                                
    // Removed OLD commented out code.
    //                                
    // Set a local view to the literal
    // value and replaced
    //                                
    // literal with local view in
    // WHERE clause.
    // ---------------------------------------------------------------------------------------
    // 10/16/2002 T.Bobb PR0012569    When updating the contact and address 
    // information,
    //                                
    // if they do not exist, create
    // them.
    // ---------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------
    // 07/18/2003 B Lee  PR166311   Added check so FIPS read will not be done if
    // it is a country.
    // -----------------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------------------
    //   Date   Developer       PR#/WR#        Description
    // ------------------------------------------------------------------------------------------
    // 05/10/06 GVandy 	 WR230751	Add support for Tribal IV-D agencies.
    // 06/13/18 JHarden         CQ62215        Add a field for Fax # on IIMC
    // 10/09/18 Raj S           CQ58033        Modified to set max date value to
    // Address End Date
    //                                         
    // while creating Interstate Payment
    // Address.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Closure.Flag = "N";
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.Ap.Number = import.Ap.Number;
    export.InterstateRequest.Assign(import.InterstateRequest);
    export.InterstateContact.Assign(import.InterstateContact);
    export.InterstateContactAddress.Assign(import.InterstateContactAddress);
    export.InterstatePaymentAddress.Assign(import.InterstatePaymentAddress);
    export.InterstateRequestHistory.Note = import.InterstateRequestHistory.Note;

    if (Equal(export.InterstateRequest.OtherStateCaseClosureDate,
      local.ZeroDateWorkArea.Date))
    {
      export.InterstateRequest.OtherStateCaseClosureDate = local.Current.Date;
    }

    // *** Work request 010337
    // *** 04/25/01 swsrchf
    // *** start
    local.Yes.Flag = "Y";
    local.Open.Flag = "O";
    local.Iicnv.ActionReasonCode = "IICNV";
    local.ZeroFips.County = 0;
    local.ZeroFips.Location = 0;

    if (AsChar(import.CaseClosed.Flag) == 'Y')
    {
      // ***
      // *** Required database EDITS for an Interstate case marked for closure:
      // ***    a)  AP has NO money in Suspense
      // ***    b)  All debts Deactivated for AP
      // ***
      // *** Both these edits must be passed, prior to Closing an Interstate 
      // case
      // ***
      // ***
      // *** Retrieve hard coded values for Cash Receipting
      // ***
      UseFnHardcodedCashReceipting();

      // ***
      // *** Retrieve hard coded values for Debt Distribution
      // ***
      UseFnHardcodedDebtDistribution();

      // ***
      // *** Obtain all Cash Receipt Details (CRD) for AP
      // ***
      foreach(var item in ReadCashReceiptDetail())
      {
        // ***
        // *** Obtain status for CRD
        // ***
        if (ReadCashReceiptDetailStatHistory())
        {
          // >>
          // 01/23/02 T.Bobb  WR 010337
          // Check for obligation on interstate case trying to match
          //  court order number to legal_action. If found, do not allow 
          // closure.
          if (!IsEmpty(entities.ExistingCashReceiptDetail.CourtOrderNumber))
          {
            if (ReadObligationLegalAction())
            {
              ExitState = "SI0000_MONEY_IN_SUSP";

              return;
            }
          }
          else
          {
            // ***
            // *** Exit......AP has money in SUSPENSE, CANNOT close Interstate 
            // case
            // ***
            ExitState = "SI0000_MONEY_IN_SUSP";

            return;
          }
        }
        else
        {
          // ***
          // *** Continue processing......No money in SUSPENSE
          // ***
        }
      }

      // ***
      // *** Retrieve all (EXCEPT Voluntary) obligations for AP
      // ***
      // >>
      // Only check for interstate obligation
      foreach(var item in ReadObligationObligationType())
      {
        local.SavedObligationType.Assign(entities.ExistingObligationType);
        local.SavedObligation.SystemGeneratedIdentifier =
          entities.ExistingObligation.SystemGeneratedIdentifier;

        // ***
        // *** Determine the status of each Obligation:
        // ***      ACTIVE, DEACTIVATED or INACTIVE
        // ***
        UseFnGetObligationStatus();

        if (AsChar(local.Derived.ObligationStatus) == 'A' || AsChar
          (local.Derived.ObligationStatus) == 'I')
        {
          // ***
          // *** Exit......AP has an ACTIVE or INACTIVE obligation. CANNOT close
          // Interstate case
          // ***
          ExitState = "SI0000_CASE_HAS_ACTIVE_DEBTS";

          return;
        }
      }

      // ***
      // *** Continue processing...... Passed database edits
      // ***
    }

    // *** end
    // *** 04/25/01 swsrchf
    // *** Work request 010337
    local.InterstateExist.Flag = "N";

    if (AsChar(import.Case1.DuplicateCaseIndicator) == 'N')
    {
      // *** Work request 010337
      // *** 04/25/01 swsrchf
      // *** Replaced literal with a local view in WHERE clause.
      if (ReadInterstateRequestAbsentParent3())
      {
        local.InterstateExist.Flag = "Y";

        goto Test;
      }

      if (AsChar(local.InterstateExist.Flag) == 'N')
      {
        ExitState = "SP0000_INVALID_VALUE_ENTERED";

        return;
      }
    }

Test:

    if (ReadCase())
    {
      switch(AsChar(import.Case1.DuplicateCaseIndicator))
      {
        case ' ':
          if (!IsEmpty(entities.Case1.DuplicateCaseIndicator))
          {
            ExitState = "ACO_NE0000_INVALID_CODE";

            return;
          }

          break;
        case 'N':
          if (IsEmpty(entities.Case1.DuplicateCaseIndicator))
          {
            ExitState = "SI0000_INVALID_CODE_CASE_NEVER_D";

            return;
          }

          break;
        default:
          break;
      }

      if (AsChar(import.CaseMarkedDuplicate.Flag) == 'Y')
      {
        local.InterstateExist.Flag = "N";

        // *** Work request 010337
        // *** 04/25/01 swsrchf
        // *** Replaced literal with a local view in WHERE clause.
        foreach(var item in ReadInterstateRequest5())
        {
          local.InterstateExist.Flag = "Y";
        }

        try
        {
          UpdateCase2();

          // ------------------------------------------------------------
          // Create the History for manually marking a Duplicate Case.
          // ------------------------------------------------------------
          local.Infrastructure.InitiatingStateCode = "OS";
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.EventId = 5;
          local.Infrastructure.ReasonCode = "MANUAL_DUP";
          local.Infrastructure.BusinessObjectCd = "CAS";
          local.Infrastructure.CaseNumber = import.Case1.Number;
          local.Infrastructure.UserId = "IIMC";
          local.Infrastructure.ReferenceDate = local.Current.Date;

          if (!IsEmpty(import.OtherState.StateAbbreviation))
          {
            local.Infrastructure.Detail =
              "Case manually marked as a Duplicate Case;" + " Initiating State :" +
              import.OtherState.StateAbbreviation;
          }
          else if (!IsEmpty(export.InterstateRequest.Country))
          {
            local.Infrastructure.Detail =
              "Case manually marked as a Duplicate Case;" + " Initiating Country :" +
              (export.InterstateRequest.Country ?? "");
          }
          else if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            local.Infrastructure.Detail =
              "Case manually marked as a Duplicate Case;" + " Initiating Tribal Agency :" +
              (export.InterstateRequest.TribalAgency ?? "");
          }

          foreach(var item in ReadCaseUnit2())
          {
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            UseSpCabCreateInfrastructure();
          }

          if (entities.CaseUnit.CuNumber == 0)
          {
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            UseSpCabCreateInfrastructure();
          }

          if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
            IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_PV";

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
        local.InterstateExist.Flag = "N";

        foreach(var item in ReadInterstateRequest5())
        {
          local.InterstateExist.Flag = "Y";
        }

        try
        {
          UpdateCase2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      export.Case1.Assign(entities.Case1);
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadAbsentParentCsePerson())
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    // ------------------------------------------------------------
    // Update the Interstate Case.
    // ------------------------------------------------------------
    if (ReadInterstateCase())
    {
      export.InterstateCase.Assign(entities.InterstateCase);
    }
    else
    {
      UseSiGenCsenetTransactSerialNo();

      try
      {
        CreateInterstateCase();
        export.InterstateCase.Assign(entities.InterstateCase);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      try
      {
        UpdateCase1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
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

    // ------------------------------------------------------------
    // Update the Interstate Request for the Case and the AP.
    // ------------------------------------------------------------
    if (ReadInterstateRequest2())
    {
      if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'C')
      {
        if (AsChar(export.InterstateRequest.OtherStateCaseStatus) != AsChar
          (entities.InterstateRequest.OtherStateCaseStatus))
        {
          local.Closure.Flag = "Y";
        }

        local.InterstateRequest.OtherStateCaseClosureDate = local.Current.Date;
      }
      else
      {
        local.InterstateRequest.OtherStateCaseClosureDate =
          export.InterstateRequest.OtherStateCaseClosureDate;
      }

      if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) != AsChar
        (import.InterstateRequest.OtherStateCaseStatus))
      {
        local.InterstateRequest.OtherStateCaseClosureDate =
          entities.InterstateRequest.OtherStateCaseClosureDate;
      }
      else
      {
        local.InterstateRequest.OtherStateCaseClosureDate = local.Current.Date;
      }

      if (AsChar(import.InterstateRequest.OtherStateCaseStatus) == AsChar
        (entities.InterstateRequest.OtherStateCaseStatus))
      {
        export.CaseStatusChanged.Flag = "N";
      }
      else
      {
        export.CaseStatusChanged.Flag = "Y";
      }

      if (!Lt(entities.InterstateRequest.OtherStateCaseClosureDate,
        local.Max.Date) && Equal
        (export.InterstateRequest.OtherStateCaseClosureDate,
        local.ZeroDateWorkArea.Date))
      {
        local.InterstateRequest.OtherStateCaseClosureDate =
          entities.InterstateRequest.OtherStateCaseClosureDate;
      }

      if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) != AsChar
        (export.InterstateRequest.OtherStateCaseStatus))
      {
        local.InterstateRequest.OtherStateCaseClosureDate = local.Current.Date;
      }
      else
      {
        local.InterstateRequest.OtherStateCaseClosureDate =
          entities.InterstateRequest.OtherStateCaseClosureDate;
      }

      try
      {
        UpdateInterstateRequest();
        export.InterstateRequest.Assign(entities.InterstateRequest);
        MoveInterstateRequest2(entities.InterstateRequest,
          local.InterstateRequest);
        export.IreqUpdated.Date =
          Date(entities.InterstateRequest.LastUpdatedTimestamp);
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        // *** Work request 010337
        // *** 04/25/01 swsrchf
        // *** Replaced literal with a local view in WHERE clause.
        if (ReadInterstateRequestHistory())
        {
          try
          {
            UpdateInterstateRequestHistory();
            export.InterstateRequestHistory.Assign(
              entities.InterstateRequestHistory);
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
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

        MoveInterstateRequest2(entities.InterstateRequest,
          local.InterstateRequest);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INTERSTATE_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INTERSTATE_PV";

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
      local.InterstateRequestCount.Count = 0;

      foreach(var item in ReadInterstateRequestAbsentParentCsePerson3())
      {
        if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'C')
        {
          if (AsChar(export.InterstateRequest.OtherStateCaseStatus) != AsChar
            (entities.InterstateRequest.OtherStateCaseStatus))
          {
            local.Closure.Flag = "Y";
          }

          local.InterstateRequest.OtherStateCaseClosureDate =
            local.Current.Date;
        }
        else
        {
          local.InterstateRequest.OtherStateCaseClosureDate =
            export.InterstateRequest.OtherStateCaseClosureDate;
        }

        if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) != AsChar
          (import.InterstateRequest.OtherStateCaseStatus))
        {
          local.InterstateRequest.OtherStateCaseClosureDate =
            entities.InterstateRequest.OtherStateCaseClosureDate;
        }
        else
        {
          local.InterstateRequest.OtherStateCaseClosureDate =
            local.Current.Date;
        }

        if (AsChar(import.InterstateRequest.OtherStateCaseStatus) == AsChar
          (entities.InterstateRequest.OtherStateCaseStatus))
        {
          export.CaseStatusChanged.Flag = "N";
        }
        else
        {
          export.CaseStatusChanged.Flag = "Y";
        }

        if (!Lt(entities.InterstateRequest.OtherStateCaseClosureDate,
          local.Max.Date) && Equal
          (export.InterstateRequest.OtherStateCaseClosureDate,
          local.ZeroDateWorkArea.Date))
        {
          local.InterstateRequest.OtherStateCaseClosureDate =
            entities.InterstateRequest.OtherStateCaseClosureDate;
        }

        if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) != AsChar
          (export.InterstateRequest.OtherStateCaseStatus))
        {
          local.InterstateRequest.OtherStateCaseClosureDate =
            local.Current.Date;
        }
        else
        {
          local.InterstateRequest.OtherStateCaseClosureDate =
            entities.InterstateRequest.OtherStateCaseClosureDate;
        }

        try
        {
          UpdateInterstateRequest();
          export.InterstateRequest.Assign(entities.InterstateRequest);
          MoveInterstateRequest2(entities.InterstateRequest,
            local.InterstateRequest);
          export.IreqUpdated.Date =
            Date(entities.InterstateRequest.LastUpdatedTimestamp);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          // *** Work request 010337
          // *** 04/25/01 swsrchf
          // *** Replaced literal with a local view in WHERE clause.
          if (ReadInterstateRequestHistory())
          {
            try
            {
              UpdateInterstateRequestHistory();
              export.InterstateRequestHistory.Assign(
                entities.InterstateRequestHistory);
              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
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
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INTERSTATE_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        ++local.InterstateRequestCount.Count;
        MoveInterstateRequest2(entities.InterstateRequest,
          local.InterstateRequest);
      }

      if (local.InterstateRequestCount.Count == 0)
      {
        ExitState = "INTERSTATE_REQUEST_NF";

        return;
      }
    }

    // ------------------------------------------------------------
    // Update the Interstate Request of each one associated with the case if 
    // closing the Interstate case.
    // ------------------------------------------------------------
    if (AsChar(local.Closure.Flag) == 'Y')
    {
      foreach(var item in ReadInterstateRequestAbsentParent9())
      {
        try
        {
          UpdateInterstateRequest();
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INTERSTATE_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        MoveInterstateRequest2(entities.InterstateRequest,
          local.InterstateRequest);
      }
    }

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      return;
    }

    if (ReadInterstateRequestAbsentParentCsePerson1())
    {
      MoveInterstateRequest2(entities.InterstateRequest, local.InterstateRequest);
        
    }
    else
    {
      // @@@  -  Not sure that this can ever happen...but it was here before so 
      // I'm adding tribal agency to the IF.
      if (IsEmpty(import.OtherState.StateAbbreviation) && IsEmpty
        (import.InterstateRequest.Country) && IsEmpty
        (import.InterstateRequest.TribalAgency))
      {
        local.MultipleIrForAp.Flag = "";
        local.InterstateRequestCount.Count = 0;
        local.InterstateCase.Flag = "Y";

        foreach(var item in ReadInterstateRequestAbsentParent8())
        {
          ++local.InterstateRequestCount.Count;

          if (AsChar(entities.InterstateRequest.KsCaseInd) != 'Y')
          {
            local.InterstateCase.Flag = "Y";
          }

          if (AsChar(local.MultipleIrForAp.Flag) == 'Y')
          {
            ExitState = "SI0000_MULTIPLE_IR_EXISTS_FOR_AP";

            return;
          }

          MoveInterstateRequest2(entities.InterstateRequest,
            local.InterstateRequest);
          local.MultipleIrForAp.Flag = "Y";
          export.OtherState.State = entities.InterstateRequest.OtherStateFips;
        }

        if (local.InterstateRequestCount.Count == 0)
        {
          foreach(var item in ReadInterstateRequestAbsentParentCsePerson2())
          {
            ++local.InterstateRequestCount.Count;
            MoveInterstateRequest2(entities.InterstateRequest,
              local.InterstateRequest);

            if (AsChar(entities.InterstateRequest.KsCaseInd) != 'Y')
            {
              local.InterstateCase.Flag = "Y";
            }

            if (AsChar(local.MultipleIrForAp.Flag) == 'Y')
            {
              ExitState = "SI0000_MULTIPLE_IR_EXISTS_FOR_AP";

              return;
            }

            local.MultipleIrForAp.Flag = "Y";
            export.OtherState.State = entities.InterstateRequest.OtherStateFips;
            local.InterstateRequest.IntHGeneratedId =
              entities.InterstateRequest.IntHGeneratedId;
          }
        }

        if (local.InterstateRequestCount.Count == 0)
        {
          ExitState = "CASE_NOT_IC_INTERSTATE";

          return;
        }

        if (AsChar(local.InterstateCase.Flag) != 'Y')
        {
          ExitState = "CASE_NOT_IC_INTERSTATE";

          return;
        }

        if (AsChar(local.MultipleIrForAp.Flag) != 'Y')
        {
          if (ReadInterstateRequest4())
          {
            if (AsChar(export.Case1.Status) == 'O')
            {
              ExitState = "AP_NOT_INVOLVED_IN_INTERSTATE";

              return;
            }

            MoveInterstateRequest2(entities.InterstateRequest,
              local.InterstateRequest);
          }
          else if (AsChar(export.Case1.Status) == 'O')
          {
            ExitState = "INTERSTATE_REQUEST_NF";

            return;
          }
        }

        if (ReadInterstateRequest3())
        {
          MoveInterstateRequest2(entities.InterstateRequest,
            local.InterstateRequest);
        }
        else
        {
          ExitState = "INTERSTATE_REQUEST_NF";

          return;
        }

        if (entities.InterstateRequest.OtherStateFips > 0)
        {
          // *** Work request 010337
          // *** 04/25/01 swsrchf
          // *** Replaced zeros with a local view in WHERE clause.
          if (ReadFips2())
          {
            export.OtherState.Assign(entities.State);
          }
          else
          {
            ExitState = "FIPS_NF";

            return;
          }
        }
      }
      else
      {
        if (!IsEmpty(import.OtherState.StateAbbreviation) && IsEmpty
          (import.InterstateRequest.Country))
        {
          // *** Work request 010337
          // *** 04/25/01 swsrchf
          // *** Replaced zeros with a local view in WHERE clause.
          if (ReadFips3())
          {
            export.OtherState.Assign(entities.State);
          }
          else
          {
            ExitState = "FIPS_NF";

            return;
          }

          if (ReadInterstateRequestAbsentParent7())
          {
            if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
            {
              ExitState = "CASE_NOT_IC_INTERSTATE";

              return;
            }

            MoveInterstateRequest2(entities.InterstateRequest,
              local.InterstateRequest);
          }
          else if (ReadInterstateRequestAbsentParent4())
          {
            if (AsChar(export.Case1.Status) == 'O')
            {
              ExitState = "AP_NOT_INVOLVED_IN_INTERSTATE";

              return;
            }

            MoveInterstateRequest2(entities.InterstateRequest,
              local.InterstateRequest);
          }
          else
          {
            local.InterstateRequestCount.Count = 0;

            foreach(var item in ReadInterstateRequest6())
            {
              ++local.InterstateRequestCount.Count;
              MoveInterstateRequest2(entities.InterstateRequest,
                local.InterstateRequest);
            }

            if (AsChar(export.Case1.Status) == 'O')
            {
              if (local.InterstateRequestCount.Count == 0)
              {
                ExitState = "CASE_NOT_INTERSTATE";
              }
              else
              {
                ExitState = "AP_NOT_INVOLVED_IN_INTERSTATE";
              }

              return;
            }
          }
        }

        if (!IsEmpty(import.InterstateRequest.Country))
        {
          if (ReadInterstateRequestAbsentParent5())
          {
            if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
            {
              ExitState = "CASE_NOT_IC_INTERSTATE";

              return;
            }

            MoveInterstateRequest2(entities.InterstateRequest,
              local.InterstateRequest);
          }
          else if (ReadInterstateRequestAbsentParent1())
          {
            MoveInterstateRequest2(entities.InterstateRequest,
              local.InterstateRequest);
          }
          else
          {
            ExitState = "CO0000_ABSENT_PARENT_NF";

            return;
          }
        }

        if (!IsEmpty(import.InterstateRequest.TribalAgency))
        {
          if (ReadInterstateRequestAbsentParent6())
          {
            if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
            {
              ExitState = "CASE_NOT_IC_INTERSTATE";

              return;
            }

            MoveInterstateRequest2(entities.InterstateRequest,
              local.InterstateRequest);
          }
          else if (ReadInterstateRequestAbsentParent2())
          {
            MoveInterstateRequest2(entities.InterstateRequest,
              local.InterstateRequest);
          }
          else
          {
            ExitState = "CO0000_ABSENT_PARENT_NF";

            return;
          }
        }
      }
    }

    // ------------------------------------------------------------
    // Update the Interstate Request Contact.
    // ------------------------------------------------------------
    // 10/16/2002 T.Bobb PR00125639
    // Update contact, contact address, and payment address if
    // found, otherwise create them.
    // Note: Some of the contact and address information was lost during 
    // conversion.
    // -------------------------------------------------------------
    if (!IsEmpty(export.InterstateContactAddress.State))
    {
      local.InterstateContactAddress.LocationType = "D";
    }
    else
    {
      local.InterstateContactAddress.LocationType = "F";
    }

    if (ReadInterstateContact())
    {
      if (!Equal(export.InterstateContact.NameFirst,
        entities.InterstateContact.NameFirst) || !
        Equal(export.InterstateContact.NameLast,
        entities.InterstateContact.NameLast) || !
        Equal(export.InterstateContact.ContactPhoneNum.GetValueOrDefault(),
        entities.InterstateContact.ContactPhoneNum) || AsChar
        (export.InterstateContact.NameMiddle) != AsChar
        (entities.InterstateContact.NameMiddle) || !
        Equal(export.InterstateContact.AreaCode.GetValueOrDefault(),
        entities.InterstateContact.AreaCode) || !
        Equal(export.InterstateContact.ContactInternetAddress,
        entities.InterstateContact.ContactInternetAddress) || !
        Equal(export.InterstateContact.ContactPhoneExtension,
        entities.InterstateContact.ContactPhoneExtension) || !
        Equal(export.InterstateContact.ContactFaxAreaCode.GetValueOrDefault(),
        entities.InterstateContact.ContactFaxAreaCode) || !
        Equal(export.InterstateContact.ContactFaxNumber.GetValueOrDefault(),
        entities.InterstateContact.ContactFaxNumber))
      {
        try
        {
          UpdateInterstateContact();
          export.InterstateContact.Assign(entities.InterstateContact);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INTERSTATE_CONTACT_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_CONTACT_PV";

              return;
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
      try
      {
        CreateInterstateContact();
        export.InterstateContact.Assign(entities.InterstateContact);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
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

    if (ReadInterstateContactAddress())
    {
      if (!Equal(export.InterstateContactAddress.Street1,
        entities.InterstateContactAddress.Street1) || !
        Equal(export.InterstateContactAddress.Street2,
        entities.InterstateContactAddress.Street2) || !
        Equal(export.InterstateContactAddress.City,
        entities.InterstateContactAddress.City) || !
        Equal(export.InterstateContactAddress.State,
        entities.InterstateContactAddress.State) || !
        Equal(export.InterstateContactAddress.ZipCode,
        entities.InterstateContactAddress.ZipCode) || !
        Equal(export.InterstateContactAddress.Zip4,
        entities.InterstateContactAddress.Zip4) || !
        Equal(export.InterstateContactAddress.Zip3,
        entities.InterstateContactAddress.Zip3) || !
        Equal(export.InterstateContactAddress.Street3,
        entities.InterstateContactAddress.Street3) || !
        Equal(export.InterstateContactAddress.Street4,
        entities.InterstateContactAddress.Street4) || !
        Equal(export.InterstateContactAddress.Province,
        entities.InterstateContactAddress.Province) || !
        Equal(export.InterstateContactAddress.PostalCode,
        entities.InterstateContactAddress.PostalCode) || !
        Equal(export.InterstateContactAddress.Country,
        entities.InterstateContactAddress.Country))
      {
        try
        {
          UpdateInterstateContactAddress();
          export.InterstateContactAddress.Assign(
            entities.InterstateContactAddress);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INTERSTATE_CONTACT_ADDRESS_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_CONTACT_ADDRESS_PV";

              return;
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
      try
      {
        CreateInterstateContactAddress();
        export.InterstateContactAddress.
          Assign(entities.InterstateContactAddress);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
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

    // ------------------------------------------------------------
    // Create the Interstate Request Payment Address.
    // ------------------------------------------------------------
    if (!IsEmpty(export.InterstatePaymentAddress.State))
    {
      export.InterstatePaymentAddress.LocationType = "D";
    }
    else
    {
      export.InterstatePaymentAddress.LocationType = "F";
    }

    if (ReadInterstatePaymentAddress())
    {
      if (!Equal(entities.InterstatePaymentAddress.Street1,
        export.InterstatePaymentAddress.Street1) || !
        Equal(entities.InterstatePaymentAddress.Street2,
        export.InterstatePaymentAddress.Street2) || !
        Equal(entities.InterstatePaymentAddress.City,
        export.InterstatePaymentAddress.City) || !
        Equal(entities.InterstatePaymentAddress.State,
        export.InterstatePaymentAddress.State) || !
        Equal(entities.InterstatePaymentAddress.ZipCode,
        export.InterstatePaymentAddress.ZipCode) || !
        Equal(entities.InterstatePaymentAddress.Zip4,
        export.InterstatePaymentAddress.Zip4) || !
        Equal(entities.InterstatePaymentAddress.Street3,
        export.InterstatePaymentAddress.Street3) || !
        Equal(entities.InterstatePaymentAddress.Street4,
        export.InterstatePaymentAddress.Street4) || !
        Equal(entities.InterstatePaymentAddress.Province,
        export.InterstatePaymentAddress.Province) || !
        Equal(entities.InterstatePaymentAddress.PostalCode,
        export.InterstatePaymentAddress.PostalCode) || !
        Equal(entities.InterstatePaymentAddress.Country,
        export.InterstatePaymentAddress.Country) || !
        Equal(entities.InterstatePaymentAddress.PayableToName,
        export.InterstatePaymentAddress.PayableToName) || !
        Equal(entities.InterstatePaymentAddress.FipsState,
        export.InterstatePaymentAddress.FipsState) || !
        Equal(entities.InterstatePaymentAddress.FipsCounty,
        export.InterstatePaymentAddress.FipsCounty) || !
        Equal(entities.InterstatePaymentAddress.FipsLocation,
        export.InterstatePaymentAddress.FipsCounty))
      {
        // -- 7/18/03 BLee PR166311 Added check so FIPS read will not be done if
        // it is a country.
        if (IsEmpty(export.InterstatePaymentAddress.Country))
        {
          if (ReadFips1())
          {
            export.InterstatePaymentAddress.FipsState =
              NumberToString(entities.State.State, 2);
          }
          else
          {
            ExitState = "FIPS_NF";

            return;
          }
        }

        try
        {
          UpdateInterstatePaymentAddress();
          export.InterstatePaymentAddress.Assign(
            entities.InterstatePaymentAddress);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INTERSTATE_PAYMENT_ADDRESS_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_PAYMENT_ADDRESS_PV";

              return;
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
      try
      {
        CreateInterstatePaymentAddress();
        export.InterstatePaymentAddress.
          Assign(entities.InterstatePaymentAddress);
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
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

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      return;
    }

    // ------------------------------------------------------------
    // Create the Interstate Request History for manual closure.
    // ------------------------------------------------------------
    if (AsChar(import.CaseClosed.Flag) == 'Y')
    {
      if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'C')
      {
        export.InterstateRequestHistory.ActionReasonCode = "IICLS";
        export.InterstateRequestHistory.ActionCode = "";
        export.InterstateRequestHistory.FunctionalTypeCode = "";
        export.InterstateRequestHistory.TransactionDirectionInd = "";
        export.InterstateRequestHistory.AttachmentIndicator = "";
        export.InterstateRequestHistory.Note =
          Spaces(InterstateRequestHistory.Note_MaxLength);
        export.InterstateRequestHistory.TransactionSerialNum = 0;
        export.InterstateRequestHistory.Note =
          import.InterstateRequestHistory.Note ?? "";
        export.InterstateRequestHistory.ActionResolutionDate =
          local.ZeroDateWorkArea.Date;
        export.InterstateRequestHistory.TransactionDate = local.Current.Date;
        ExitState = "ACO_NN0000_ALL_OK";
        UseSiCreateIsRequestHistory();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ------------------------------------------------------------
        // Create the History for manual Interstate Case closure.
        // ------------------------------------------------------------
        local.Infrastructure.InitiatingStateCode = "OS";
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.EventId = 5;
        local.Infrastructure.ReasonCode = "IN_INTST_MCLOSE";
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.CaseNumber = import.Case1.Number;
        local.Infrastructure.UserId = "IIMC";
        local.Infrastructure.ReferenceDate = local.Current.Date;

        foreach(var item in ReadCaseUnit1())
        {
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

          if (!IsEmpty(import.OtherState.StateAbbreviation))
          {
            local.Infrastructure.Detail =
              "Incoming Interstate Case Closed Manually;" + " Initiating State :" +
              import.OtherState.StateAbbreviation;
          }
          else if (!IsEmpty(export.InterstateRequest.Country))
          {
            local.Infrastructure.Detail =
              "Incoming Interstate Case Closed Manually;" + " Initiating Country :" +
              (export.InterstateRequest.Country ?? "");
          }
          else if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            local.Infrastructure.Detail =
              "Incoming Interstate Case Closed Manually;" + " Initiating Tribal Agency :" +
              (export.InterstateRequest.TribalAgency ?? "");
          }

          UseSpCabCreateInfrastructure();
        }

        if (entities.CaseUnit.CuNumber == 0)
        {
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
          local.Infrastructure.ReasonCode = "IN_INTST_MCLNAP";

          if (!IsEmpty(import.OtherState.StateAbbreviation))
          {
            local.Infrastructure.Detail =
              "Incoming Interstate Case (no Case Units) Closed Manually;" + " IN State :" +
              import.OtherState.StateAbbreviation;
          }
          else if (!IsEmpty(export.InterstateRequest.Country))
          {
            local.Infrastructure.Detail =
              "Incoming Interstate Case (no Case Units) Closed Manually;" + " IN Country :" +
              (export.InterstateRequest.Country ?? "");
          }
          else if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            local.Infrastructure.Detail =
              "Incoming Interstate Case (no Case Units) Closed Manually;" + " IN Tribal Agency :" +
              (export.InterstateRequest.TribalAgency ?? "");
          }

          UseSpCabCreateInfrastructure();
        }

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ------------------------------------------------------------
        // Check to see if there are any more Interstate Case open
        // for a Duplicate Case.
        // ------------------------------------------------------------
        if (AsChar(entities.Case1.DuplicateCaseIndicator) == 'Y')
        {
          // *** Work request 010337
          // *** 04/25/01 swsrchf
          // *** Replaced literal with a local view in WHERE clause.
          if (ReadInterstateRequest1())
          {
            MoveInterstateRequest2(entities.InterstateRequest,
              local.InterstateRequest);
          }
          else
          {
            // ------------------------------------------------------------
            // No more Interstate Case open for a Duplicate Case. Change
            // the Duplicate indicator flag to "N" and write to History.
            // ------------------------------------------------------------
            try
            {
              UpdateCase3();

              // ------------------------------------------------------------
              // Create the History for marking a Case as not duplicate.
              // ------------------------------------------------------------
              local.Infrastructure.InitiatingStateCode = "OS";
              local.Infrastructure.ProcessStatus = "Q";
              local.Infrastructure.EventId = 5;
              local.Infrastructure.ReasonCode = "IS_CASE_CLS_NOD";
              local.Infrastructure.BusinessObjectCd = "CAS";
              local.Infrastructure.CaseNumber = import.Case1.Number;
              local.Infrastructure.UserId = "IIMC";
              local.Infrastructure.ReferenceDate = local.Current.Date;
              local.Infrastructure.Detail =
                "Case no longer marked Duplicate; Last Interstate Case involvement closed";
                

              foreach(var item in ReadCaseUnit1())
              {
                local.Infrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
                UseSpCabCreateInfrastructure();
              }

              if (entities.CaseUnit.CuNumber == 0)
              {
                local.Infrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
                UseSpCabCreateInfrastructure();
              }

              if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
                IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              export.Case1.Assign(entities.Case1);
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CASE_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CASE_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
      }
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateCaseClosureReason = source.OtherStateCaseClosureReason;
    target.OtherStateCaseClosureDate = source.OtherStateCaseClosureDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnGetObligationStatus()
  {
    var useImport = new FnGetObligationStatus.Import();
    var useExport = new FnGetObligationStatus.Export();

    useImport.CsePerson.Number = import.Ap.Number;
    useImport.HcOtCVoluntary.Classification =
      local.HcOtCVoluntary.Classification;
    useImport.HcOtCAccruing.Classification = local.HcOtCAccruing.Classification;
    useImport.CsePersonAccount.Type1 = local.HcCpaObligor.Type1;
    useImport.ObligationType.Assign(local.SavedObligationType);
    useImport.Obligation.SystemGeneratedIdentifier =
      local.SavedObligation.SystemGeneratedIdentifier;
    useImport.Current.Date = local.Current.Date;

    Call(FnGetObligationStatus.Execute, useImport, useExport);

    local.Derived.ObligationStatus =
      useExport.ScreenObligationStatus.ObligationStatus;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.Susp.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.Pend.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HcOtCVoluntary.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    local.HcOtCAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HcCpaObligor.Type1 = useExport.CpaObligor.Type1;
  }

  private void UseSiCreateIsRequestHistory()
  {
    var useImport = new SiCreateIsRequestHistory.Import();
    var useExport = new SiCreateIsRequestHistory.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Ap.Number = entities.Ap.Number;
    useImport.InterstateRequestHistory.Assign(export.InterstateRequestHistory);
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      

    Call(SiCreateIsRequestHistory.Execute, useImport, useExport);

    export.InterstateRequestHistory.Assign(useExport.InterstateRequestHistory);
  }

  private void UseSiGenCsenetTransactSerialNo()
  {
    var useImport = new SiGenCsenetTransactSerialNo.Import();
    var useExport = new SiGenCsenetTransactSerialNo.Export();

    Call(SiGenCsenetTransactSerialNo.Execute, useImport, useExport);

    MoveInterstateCase(useExport.InterstateCase, export.InterstateCase);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void CreateInterstateCase()
  {
    var transSerialNumber = export.InterstateCase.TransSerialNumber;
    var transactionDate = export.InterstateCase.TransactionDate;

    entities.InterstateCase.Populated = false;
    Update("CreateInterstateCase",
      (db, command) =>
      {
        db.SetInt32(command, "localFipsState", 0);
        db.SetNullableInt32(command, "localFipsCounty", 0);
        db.SetInt64(command, "transSerialNbr", transSerialNumber);
        db.SetString(command, "actionCode", "");
        db.SetString(command, "functionalTypeCo", "");
        db.SetDate(command, "transactionDate", transactionDate);
        db.SetNullableString(command, "ksCaseId", "");
        db.SetNullableString(command, "actionReasonCode", "");
        db.SetNullableDate(command, "actionResolution", default(DateTime));
        db.SetNullableInt32(command, "caseDataInd", 0);
        db.SetNullableTimeSpan(command, "sentTime", TimeSpan.Zero);
        db.SetNullableString(command, "paymentMailingAd", "");
        db.SetNullableString(command, "paymentState", "");
        db.SetNullableString(command, "paymentZipCode4", "");
        db.SetNullableString(command, "contactNameLast", "");
        db.SetNullableString(command, "contactNameFirst", "");
        db.SetNullableInt32(command, "contactPhoneNum", 0);
        db.SetNullableString(command, "memo", "");
        db.SetNullableString(command, "contactPhoneExt", "");
        db.SetNullableString(command, "conInternetAddr", "");
        db.SetNullableString(command, "sendPaymBankAcc", "");
        db.SetNullableInt32(command, "sendPaymRtCode", 0);
      });

    entities.InterstateCase.TransSerialNumber = transSerialNumber;
    entities.InterstateCase.TransactionDate = transactionDate;
    entities.InterstateCase.SendPaymentsBankAccount = "";
    entities.InterstateCase.SendPaymentsRoutingCode = 0;
    entities.InterstateCase.Populated = true;
  }

  private void CreateInterstateContact()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var contactPhoneNum =
      export.InterstateContact.ContactPhoneNum.GetValueOrDefault();
    var endDate = export.InterstateContact.EndDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var nameLast = export.InterstateContact.NameLast ?? "";
    var nameFirst = export.InterstateContact.NameFirst ?? "";
    var nameMiddle = export.InterstateContact.NameMiddle ?? "";
    var contactNameSuffix = export.InterstateContact.ContactNameSuffix ?? "";
    var areaCode = export.InterstateContact.AreaCode.GetValueOrDefault();
    var contactPhoneExtension =
      export.InterstateContact.ContactPhoneExtension ?? "";
    var contactFaxNumber =
      export.InterstateContact.ContactFaxNumber.GetValueOrDefault();
    var contactFaxAreaCode =
      export.InterstateContact.ContactFaxAreaCode.GetValueOrDefault();
    var contactInternetAddress =
      export.InterstateContact.ContactInternetAddress ?? "";

    entities.InterstateContact.Populated = false;
    Update("CreateInterstateContact",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "startDate", null);
        db.SetNullableInt32(command, "contactPhoneNum", contactPhoneNum);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "nameLast", nameLast);
        db.SetNullableString(command, "nameFirst", nameFirst);
        db.SetNullableString(command, "nameMiddle", nameMiddle);
        db.SetNullableString(command, "contactNameSuffi", contactNameSuffix);
        db.SetNullableInt32(command, "areaCode", areaCode);
        db.SetNullableString(command, "contactPhoneExt", contactPhoneExtension);
        db.SetNullableInt32(command, "contactFaxNumber", contactFaxNumber);
        db.SetNullableInt32(command, "contFaxAreaCode", contactFaxAreaCode);
        db.
          SetNullableString(command, "contInternetAddr", contactInternetAddress);
          
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTimes", null);
      });

    entities.InterstateContact.IntGeneratedId = intGeneratedId;
    entities.InterstateContact.StartDate = null;
    entities.InterstateContact.ContactPhoneNum = contactPhoneNum;
    entities.InterstateContact.EndDate = endDate;
    entities.InterstateContact.CreatedBy = createdBy;
    entities.InterstateContact.CreatedTstamp = createdTstamp;
    entities.InterstateContact.NameLast = nameLast;
    entities.InterstateContact.NameFirst = nameFirst;
    entities.InterstateContact.NameMiddle = nameMiddle;
    entities.InterstateContact.ContactNameSuffix = contactNameSuffix;
    entities.InterstateContact.AreaCode = areaCode;
    entities.InterstateContact.ContactPhoneExtension = contactPhoneExtension;
    entities.InterstateContact.ContactFaxNumber = contactFaxNumber;
    entities.InterstateContact.ContactFaxAreaCode = contactFaxAreaCode;
    entities.InterstateContact.ContactInternetAddress = contactInternetAddress;
    entities.InterstateContact.LastUpdatedBy = "";
    entities.InterstateContact.LastUpdatedTimestamp = null;
    entities.InterstateContact.Populated = true;
  }

  private void CreateInterstateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);

    var icoContStartDt = entities.InterstateContact.StartDate;
    var intGeneratedId = entities.InterstateContact.IntGeneratedId;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = "CT";
    var street1 = export.InterstateContactAddress.Street1 ?? "";
    var street2 = export.InterstateContactAddress.Street2 ?? "";
    var city = export.InterstateContactAddress.City ?? "";
    var endDate = export.InterstateContactAddress.EndDate;
    var county = export.InterstateContactAddress.County ?? "";
    var state = export.InterstateContactAddress.State ?? "";
    var zipCode = export.InterstateContactAddress.ZipCode ?? "";
    var zip4 = export.InterstateContactAddress.Zip4 ?? "";
    var zip3 = export.InterstateContactAddress.Zip3 ?? "";
    var street3 = export.InterstateContactAddress.Street3 ?? "";
    var street4 = export.InterstateContactAddress.Street4 ?? "";
    var province = export.InterstateContactAddress.Province ?? "";
    var postalCode = export.InterstateContactAddress.PostalCode ?? "";
    var country = export.InterstateContactAddress.Country ?? "";
    var locationType = local.InterstateContactAddress.LocationType;

    CheckValid<InterstateContactAddress>("LocationType", locationType);
    entities.InterstateContactAddress.Populated = false;
    Update("CreateInterstateContactAddress",
      (db, command) =>
      {
        db.SetDate(command, "icoContStartDt", icoContStartDt);
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "startDate", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "locationType", locationType);
      });

    entities.InterstateContactAddress.IcoContStartDt = icoContStartDt;
    entities.InterstateContactAddress.IntGeneratedId = intGeneratedId;
    entities.InterstateContactAddress.StartDate = null;
    entities.InterstateContactAddress.CreatedBy = createdBy;
    entities.InterstateContactAddress.CreatedTimestamp = createdTimestamp;
    entities.InterstateContactAddress.LastUpdatedBy = createdBy;
    entities.InterstateContactAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstateContactAddress.Type1 = type1;
    entities.InterstateContactAddress.Street1 = street1;
    entities.InterstateContactAddress.Street2 = street2;
    entities.InterstateContactAddress.City = city;
    entities.InterstateContactAddress.EndDate = endDate;
    entities.InterstateContactAddress.County = county;
    entities.InterstateContactAddress.State = state;
    entities.InterstateContactAddress.ZipCode = zipCode;
    entities.InterstateContactAddress.Zip4 = zip4;
    entities.InterstateContactAddress.Zip3 = zip3;
    entities.InterstateContactAddress.Street3 = street3;
    entities.InterstateContactAddress.Street4 = street4;
    entities.InterstateContactAddress.Province = province;
    entities.InterstateContactAddress.PostalCode = postalCode;
    entities.InterstateContactAddress.Country = country;
    entities.InterstateContactAddress.LocationType = locationType;
    entities.InterstateContactAddress.Populated = true;
  }

  private void CreateInterstatePaymentAddress()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var addressStartDate = Now().Date;
    var type1 = "PY";
    var street1 = export.InterstatePaymentAddress.Street1;
    var street2 = export.InterstatePaymentAddress.Street2 ?? "";
    var city = export.InterstatePaymentAddress.City;
    var zip5 = export.InterstatePaymentAddress.Zip5 ?? "";
    var addressEndDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var payableToName = export.InterstatePaymentAddress.PayableToName ?? "";
    var state = export.InterstatePaymentAddress.State ?? "";
    var zipCode = export.InterstatePaymentAddress.ZipCode ?? "";
    var zip4 = export.InterstatePaymentAddress.Zip4 ?? "";
    var zip3 = export.InterstatePaymentAddress.Zip3 ?? "";
    var county = export.InterstatePaymentAddress.County ?? "";
    var street3 = export.InterstatePaymentAddress.Street3 ?? "";
    var street4 = export.InterstatePaymentAddress.Street4 ?? "";
    var province = export.InterstatePaymentAddress.Province ?? "";
    var postalCode = export.InterstatePaymentAddress.PostalCode ?? "";
    var country = export.InterstatePaymentAddress.Country ?? "";
    var locationType = export.InterstatePaymentAddress.LocationType;
    var fipsCounty = export.InterstatePaymentAddress.FipsCounty ?? "";
    var fipsState = export.InterstatePaymentAddress.FipsState ?? "";
    var fipsLocation = export.InterstatePaymentAddress.FipsLocation ?? "";

    CheckValid<InterstatePaymentAddress>("LocationType", locationType);
    entities.InterstatePaymentAddress.Populated = false;
    Update("CreateInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "addressStartDate", addressStartDate);
        db.SetNullableString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetNullableString(command, "zip5", zip5);
        db.SetNullableDate(command, "addressEndDate", addressEndDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableString(command, "payableToName", payableToName);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "locationType", locationType);
        db.SetNullableString(command, "fipsCounty", fipsCounty);
        db.SetNullableString(command, "fipsState", fipsState);
        db.SetNullableString(command, "fipsLocation", fipsLocation);
        db.SetNullableInt32(command, "routingNumberAba", 0);
        db.SetNullableString(command, "accountNumberDfi", "");
        db.SetNullableString(command, "accountType", "");
      });

    entities.InterstatePaymentAddress.IntGeneratedId = intGeneratedId;
    entities.InterstatePaymentAddress.AddressStartDate = addressStartDate;
    entities.InterstatePaymentAddress.Type1 = type1;
    entities.InterstatePaymentAddress.Street1 = street1;
    entities.InterstatePaymentAddress.Street2 = street2;
    entities.InterstatePaymentAddress.City = city;
    entities.InterstatePaymentAddress.Zip5 = zip5;
    entities.InterstatePaymentAddress.AddressEndDate = addressEndDate;
    entities.InterstatePaymentAddress.CreatedBy = createdBy;
    entities.InterstatePaymentAddress.CreatedTimestamp = createdTimestamp;
    entities.InterstatePaymentAddress.LastUpdatedBy = createdBy;
    entities.InterstatePaymentAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstatePaymentAddress.PayableToName = payableToName;
    entities.InterstatePaymentAddress.State = state;
    entities.InterstatePaymentAddress.ZipCode = zipCode;
    entities.InterstatePaymentAddress.Zip4 = zip4;
    entities.InterstatePaymentAddress.Zip3 = zip3;
    entities.InterstatePaymentAddress.County = county;
    entities.InterstatePaymentAddress.Street3 = street3;
    entities.InterstatePaymentAddress.Street4 = street4;
    entities.InterstatePaymentAddress.Province = province;
    entities.InterstatePaymentAddress.PostalCode = postalCode;
    entities.InterstatePaymentAddress.Country = country;
    entities.InterstatePaymentAddress.LocationType = locationType;
    entities.InterstatePaymentAddress.FipsCounty = fipsCounty;
    entities.InterstatePaymentAddress.FipsState = fipsState;
    entities.InterstatePaymentAddress.FipsLocation = fipsLocation;
    entities.InterstatePaymentAddress.RoutingNumberAba = 0;
    entities.InterstatePaymentAddress.AccountNumberDfi = "";
    entities.InterstatePaymentAddress.AccountType = "";
    entities.InterstatePaymentAddress.Populated = true;
  }

  private bool ReadAbsentParentCsePerson()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadAbsentParentCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 3);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 5);
        entities.Case1.IcTransactionDate = db.GetDate(reader, 6);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 7);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", export.Case1.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CasNo = db.GetString(reader, 5);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CasNo = db.GetString(reader, 5);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    entities.ExistingCashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingCashReceiptDetail.CaseNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceiptDetail.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.Susp.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.Pend.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingCashReceiptDetailStatHistory.Populated = true;
      });
  }

  private bool ReadFips1()
  {
    entities.State.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation",
          export.InterstatePaymentAddress.State ?? "");
        db.SetInt32(command, "county", local.ZeroFips.County);
        db.SetInt32(command, "location", local.ZeroFips.Location);
      },
      (db, reader) =>
      {
        entities.State.State = db.GetInt32(reader, 0);
        entities.State.County = db.GetInt32(reader, 1);
        entities.State.Location = db.GetInt32(reader, 2);
        entities.State.StateAbbreviation = db.GetString(reader, 3);
        entities.State.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.State.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.OtherState.State);
        db.SetInt32(command, "county", local.ZeroFips.County);
        db.SetInt32(command, "location", local.ZeroFips.Location);
      },
      (db, reader) =>
      {
        entities.State.State = db.GetInt32(reader, 0);
        entities.State.County = db.GetInt32(reader, 1);
        entities.State.Location = db.GetInt32(reader, 2);
        entities.State.StateAbbreviation = db.GetString(reader, 3);
        entities.State.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    entities.State.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", export.OtherState.StateAbbreviation);
        db.SetInt32(command, "county", local.ZeroFips.County);
        db.SetInt32(command, "location", local.ZeroFips.Location);
      },
      (db, reader) =>
      {
        entities.State.State = db.GetInt32(reader, 0);
        entities.State.County = db.GetInt32(reader, 1);
        entities.State.Location = db.GetInt32(reader, 2);
        entities.State.StateAbbreviation = db.GetString(reader, 3);
        entities.State.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", entities.Case1.IcTransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          entities.Case1.IcTransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 2);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 3);
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return Read("ReadInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", local.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContact.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateContact.StartDate = db.GetDate(reader, 1);
        entities.InterstateContact.ContactPhoneNum =
          db.GetNullableInt32(reader, 2);
        entities.InterstateContact.EndDate = db.GetNullableDate(reader, 3);
        entities.InterstateContact.CreatedBy = db.GetString(reader, 4);
        entities.InterstateContact.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.InterstateContact.NameLast = db.GetNullableString(reader, 6);
        entities.InterstateContact.NameFirst = db.GetNullableString(reader, 7);
        entities.InterstateContact.NameMiddle = db.GetNullableString(reader, 8);
        entities.InterstateContact.ContactNameSuffix =
          db.GetNullableString(reader, 9);
        entities.InterstateContact.AreaCode = db.GetNullableInt32(reader, 10);
        entities.InterstateContact.ContactPhoneExtension =
          db.GetNullableString(reader, 11);
        entities.InterstateContact.ContactFaxNumber =
          db.GetNullableInt32(reader, 12);
        entities.InterstateContact.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.InterstateContact.ContactInternetAddress =
          db.GetNullableString(reader, 14);
        entities.InterstateContact.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.InterstateContact.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.InterstateContact.Populated = true;
      });
  }

  private bool ReadInterstateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);
    entities.InterstateContactAddress.Populated = false;

    return Read("ReadInterstateContactAddress",
      (db, command) =>
      {
        db.SetDate(
          command, "icoContStartDt",
          entities.InterstateContact.StartDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.InterstateContactAddress.IcoContStartDt =
          db.GetDate(reader, 0);
        entities.InterstateContactAddress.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateContactAddress.StartDate = db.GetDate(reader, 2);
        entities.InterstateContactAddress.CreatedBy = db.GetString(reader, 3);
        entities.InterstateContactAddress.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.InterstateContactAddress.LastUpdatedBy =
          db.GetString(reader, 5);
        entities.InterstateContactAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateContactAddress.Type1 =
          db.GetNullableString(reader, 7);
        entities.InterstateContactAddress.Street1 =
          db.GetNullableString(reader, 8);
        entities.InterstateContactAddress.Street2 =
          db.GetNullableString(reader, 9);
        entities.InterstateContactAddress.City =
          db.GetNullableString(reader, 10);
        entities.InterstateContactAddress.EndDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateContactAddress.County =
          db.GetNullableString(reader, 12);
        entities.InterstateContactAddress.State =
          db.GetNullableString(reader, 13);
        entities.InterstateContactAddress.ZipCode =
          db.GetNullableString(reader, 14);
        entities.InterstateContactAddress.Zip4 =
          db.GetNullableString(reader, 15);
        entities.InterstateContactAddress.Zip3 =
          db.GetNullableString(reader, 16);
        entities.InterstateContactAddress.Street3 =
          db.GetNullableString(reader, 17);
        entities.InterstateContactAddress.Street4 =
          db.GetNullableString(reader, 18);
        entities.InterstateContactAddress.Province =
          db.GetNullableString(reader, 19);
        entities.InterstateContactAddress.PostalCode =
          db.GetNullableString(reader, 20);
        entities.InterstateContactAddress.Country =
          db.GetNullableString(reader, 21);
        entities.InterstateContactAddress.LocationType =
          db.GetString(reader, 22);
        entities.InterstateContactAddress.Populated = true;
        CheckValid<InterstateContactAddress>("LocationType",
          entities.InterstateContactAddress.LocationType);
      });
  }

  private bool ReadInterstatePaymentAddress()
  {
    entities.InterstatePaymentAddress.Populated = false;

    return Read("ReadInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", local.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstatePaymentAddress.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstatePaymentAddress.AddressStartDate =
          db.GetDate(reader, 1);
        entities.InterstatePaymentAddress.Type1 =
          db.GetNullableString(reader, 2);
        entities.InterstatePaymentAddress.Street1 = db.GetString(reader, 3);
        entities.InterstatePaymentAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.InterstatePaymentAddress.City = db.GetString(reader, 5);
        entities.InterstatePaymentAddress.Zip5 =
          db.GetNullableString(reader, 6);
        entities.InterstatePaymentAddress.AddressEndDate =
          db.GetNullableDate(reader, 7);
        entities.InterstatePaymentAddress.CreatedBy = db.GetString(reader, 8);
        entities.InterstatePaymentAddress.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.InterstatePaymentAddress.LastUpdatedBy =
          db.GetString(reader, 10);
        entities.InterstatePaymentAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.InterstatePaymentAddress.PayableToName =
          db.GetNullableString(reader, 12);
        entities.InterstatePaymentAddress.State =
          db.GetNullableString(reader, 13);
        entities.InterstatePaymentAddress.ZipCode =
          db.GetNullableString(reader, 14);
        entities.InterstatePaymentAddress.Zip4 =
          db.GetNullableString(reader, 15);
        entities.InterstatePaymentAddress.Zip3 =
          db.GetNullableString(reader, 16);
        entities.InterstatePaymentAddress.County =
          db.GetNullableString(reader, 17);
        entities.InterstatePaymentAddress.Street3 =
          db.GetNullableString(reader, 18);
        entities.InterstatePaymentAddress.Street4 =
          db.GetNullableString(reader, 19);
        entities.InterstatePaymentAddress.Province =
          db.GetNullableString(reader, 20);
        entities.InterstatePaymentAddress.PostalCode =
          db.GetNullableString(reader, 21);
        entities.InterstatePaymentAddress.Country =
          db.GetNullableString(reader, 22);
        entities.InterstatePaymentAddress.LocationType =
          db.GetString(reader, 23);
        entities.InterstatePaymentAddress.FipsCounty =
          db.GetNullableString(reader, 24);
        entities.InterstatePaymentAddress.FipsState =
          db.GetNullableString(reader, 25);
        entities.InterstatePaymentAddress.FipsLocation =
          db.GetNullableString(reader, 26);
        entities.InterstatePaymentAddress.RoutingNumberAba =
          db.GetNullableInt64(reader, 27);
        entities.InterstatePaymentAddress.AccountNumberDfi =
          db.GetNullableString(reader, 28);
        entities.InterstatePaymentAddress.AccountType =
          db.GetNullableString(reader, 29);
        entities.InterstatePaymentAddress.Populated = true;
        CheckValid<InterstatePaymentAddress>("LocationType",
          entities.InterstatePaymentAddress.LocationType);
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "othStCaseStatus", local.Open.Flag);
        db.SetNullableDate(
          command, "othStateClsDte", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest3()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest3",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest4()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest4",
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
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private IEnumerable<bool> ReadInterstateRequest5()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest5",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetString(command, "ksCaseInd", local.Yes.Flag);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest6()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest6",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadInterstateRequestAbsentParent1()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadInterstateRequestAbsentParent1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
        db.SetNullableString(
          command, "country", import.InterstateRequest.Country ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadInterstateRequestAbsentParent2()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadInterstateRequestAbsentParent2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
        db.SetNullableString(
          command, "tribalAgency", import.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadInterstateRequestAbsentParent3()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadInterstateRequestAbsentParent3",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", import.Case1.Number);
        db.SetString(command, "ksCaseInd", local.Yes.Flag);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadInterstateRequestAbsentParent4()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadInterstateRequestAbsentParent4",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetInt32(command, "othrStateFipsCd", export.OtherState.State);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadInterstateRequestAbsentParent5()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadInterstateRequestAbsentParent5",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
        db.SetNullableString(
          command, "country", import.InterstateRequest.Country ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadInterstateRequestAbsentParent6()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadInterstateRequestAbsentParent6",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
        db.SetNullableString(
          command, "tribalAgency", import.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadInterstateRequestAbsentParent7()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadInterstateRequestAbsentParent7",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
        db.SetInt32(command, "othrStateFipsCd", export.OtherState.State);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private IEnumerable<bool> ReadInterstateRequestAbsentParent8()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadInterstateRequestAbsentParent8",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestAbsentParent9()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadInterstateRequestAbsentParent9",
      (db, command) =>
      {
        db.SetInt32(
          command, "otherStateFips", export.InterstateRequest.OtherStateFips);
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", export.InterstateRequest.TribalAgency ?? ""
          );
        db.SetString(command, "ksCaseInd", local.Yes.Flag);
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private bool ReadInterstateRequestAbsentParentCsePerson1()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadInterstateRequestAbsentParentCsePerson1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.Ap.Number = db.GetString(reader, 14);
        entities.Ap.Number = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private IEnumerable<bool> ReadInterstateRequestAbsentParentCsePerson2()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadInterstateRequestAbsentParentCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.Ap.Number = db.GetString(reader, 14);
        entities.Ap.Number = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestAbsentParentCsePerson3()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadInterstateRequestAbsentParentCsePerson3",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.Ap.Number = db.GetString(reader, 14);
        entities.Ap.Number = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

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
        db.SetNullableString(
          command, "actionReasonCode", local.Iicnv.ActionReasonCode ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 4);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 5);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 6);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 7);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 8);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private bool ReadObligationLegalAction()
  {
    entities.LegalAction.Populated = false;
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligationLegalAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Ap.Number);
        db.SetNullableString(
          command, "standardNo",
          entities.ExistingCashReceiptDetail.CourtOrderNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.ExistingObligation.OrderTypeCode = db.GetString(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.Populated = true;
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingObligation.OrderTypeCode);
      });
  }

  private IEnumerable<bool> ReadObligationObligationType()
  {
    entities.ExistingObligationType.Populated = false;
    entities.ExistingObligation.Populated = false;

    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "cpaType", local.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.Ap.Number);
        db.SetString(
          command, "debtTypClass", local.HcOtCVoluntary.Classification);
        db.SetInt32(
          command, "intGeneratedId", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingObligation.OrderTypeCode = db.GetString(reader, 5);
        entities.ExistingObligationType.Code = db.GetString(reader, 6);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 7);
        entities.ExistingObligationType.Populated = true;
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingObligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);

        return true;
      });
  }

  private void UpdateCase1()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var icTransSerialNumber = export.InterstateCase.TransSerialNumber;
    var icTransactionDate = entities.InterstateCase.TransactionDate;

    entities.Case1.Populated = false;
    Update("UpdateCase1",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt64(command, "icTransSerNo", icTransSerialNumber);
        db.SetDate(command, "icTransDate", icTransactionDate);
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.IcTransSerialNumber = icTransSerialNumber;
    entities.Case1.IcTransactionDate = icTransactionDate;
    entities.Case1.Populated = true;
  }

  private void UpdateCase2()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var duplicateCaseIndicator = import.Case1.DuplicateCaseIndicator ?? "";

    entities.Case1.Populated = false;
    Update("UpdateCase2",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableString(command, "dupCaseIndicator", duplicateCaseIndicator);
          
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.DuplicateCaseIndicator = duplicateCaseIndicator;
    entities.Case1.Populated = true;
  }

  private void UpdateCase3()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var duplicateCaseIndicator = "N";

    entities.Case1.Populated = false;
    Update("UpdateCase3",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableString(command, "dupCaseIndicator", duplicateCaseIndicator);
          
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.DuplicateCaseIndicator = duplicateCaseIndicator;
    entities.Case1.Populated = true;
  }

  private void UpdateInterstateContact()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);

    var contactPhoneNum =
      export.InterstateContact.ContactPhoneNum.GetValueOrDefault();
    var endDate = export.InterstateContact.EndDate;
    var nameLast = export.InterstateContact.NameLast ?? "";
    var nameFirst = export.InterstateContact.NameFirst ?? "";
    var nameMiddle = export.InterstateContact.NameMiddle ?? "";
    var contactNameSuffix = export.InterstateContact.ContactNameSuffix ?? "";
    var areaCode = export.InterstateContact.AreaCode.GetValueOrDefault();
    var contactPhoneExtension =
      export.InterstateContact.ContactPhoneExtension ?? "";
    var contactFaxNumber =
      export.InterstateContact.ContactFaxNumber.GetValueOrDefault();
    var contactFaxAreaCode =
      export.InterstateContact.ContactFaxAreaCode.GetValueOrDefault();
    var contactInternetAddress =
      export.InterstateContact.ContactInternetAddress ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.InterstateContact.Populated = false;
    Update("UpdateInterstateContact",
      (db, command) =>
      {
        db.SetNullableInt32(command, "contactPhoneNum", contactPhoneNum);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "nameLast", nameLast);
        db.SetNullableString(command, "nameFirst", nameFirst);
        db.SetNullableString(command, "nameMiddle", nameMiddle);
        db.SetNullableString(command, "contactNameSuffi", contactNameSuffix);
        db.SetNullableInt32(command, "areaCode", areaCode);
        db.SetNullableString(command, "contactPhoneExt", contactPhoneExtension);
        db.SetNullableInt32(command, "contactFaxNumber", contactFaxNumber);
        db.SetNullableInt32(command, "contFaxAreaCode", contactFaxAreaCode);
        db.
          SetNullableString(command, "contInternetAddr", contactInternetAddress);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
        db.SetDate(
          command, "startDate",
          entities.InterstateContact.StartDate.GetValueOrDefault());
      });

    entities.InterstateContact.ContactPhoneNum = contactPhoneNum;
    entities.InterstateContact.EndDate = endDate;
    entities.InterstateContact.NameLast = nameLast;
    entities.InterstateContact.NameFirst = nameFirst;
    entities.InterstateContact.NameMiddle = nameMiddle;
    entities.InterstateContact.ContactNameSuffix = contactNameSuffix;
    entities.InterstateContact.AreaCode = areaCode;
    entities.InterstateContact.ContactPhoneExtension = contactPhoneExtension;
    entities.InterstateContact.ContactFaxNumber = contactFaxNumber;
    entities.InterstateContact.ContactFaxAreaCode = contactFaxAreaCode;
    entities.InterstateContact.ContactInternetAddress = contactInternetAddress;
    entities.InterstateContact.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateContact.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterstateContact.Populated = true;
  }

  private void UpdateInterstateContactAddress()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateContactAddress.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var street1 = export.InterstateContactAddress.Street1 ?? "";
    var street2 = export.InterstateContactAddress.Street2 ?? "";
    var city = export.InterstateContactAddress.City ?? "";
    var endDate = entities.InterstateContactAddress.EndDate;
    var county = export.InterstateContactAddress.County ?? "";
    var state = export.InterstateContactAddress.State ?? "";
    var zipCode = export.InterstateContactAddress.ZipCode ?? "";
    var zip4 = export.InterstateContactAddress.Zip4 ?? "";
    var zip3 = export.InterstateContactAddress.Zip3 ?? "";
    var street3 = export.InterstateContactAddress.Street3 ?? "";
    var street4 = export.InterstateContactAddress.Street4 ?? "";
    var province = export.InterstateContactAddress.Province ?? "";
    var postalCode = export.InterstateContactAddress.PostalCode ?? "";
    var country = export.InterstateContactAddress.Country ?? "";
    var locationType = local.InterstateContactAddress.LocationType;

    CheckValid<InterstateContactAddress>("LocationType", locationType);
    entities.InterstateContactAddress.Populated = false;
    Update("UpdateInterstateContactAddress",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "locationType", locationType);
        db.SetDate(
          command, "icoContStartDt",
          entities.InterstateContactAddress.IcoContStartDt.GetValueOrDefault());
          
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateContactAddress.IntGeneratedId);
        db.SetDate(
          command, "startDate",
          entities.InterstateContactAddress.StartDate.GetValueOrDefault());
      });

    entities.InterstateContactAddress.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateContactAddress.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateContactAddress.Street1 = street1;
    entities.InterstateContactAddress.Street2 = street2;
    entities.InterstateContactAddress.City = city;
    entities.InterstateContactAddress.County = county;
    entities.InterstateContactAddress.State = state;
    entities.InterstateContactAddress.ZipCode = zipCode;
    entities.InterstateContactAddress.Zip4 = zip4;
    entities.InterstateContactAddress.Zip3 = zip3;
    entities.InterstateContactAddress.Street3 = street3;
    entities.InterstateContactAddress.Street4 = street4;
    entities.InterstateContactAddress.Province = province;
    entities.InterstateContactAddress.PostalCode = postalCode;
    entities.InterstateContactAddress.Country = country;
    entities.InterstateContactAddress.LocationType = locationType;
    entities.InterstateContactAddress.Populated = true;
  }

  private void UpdateInterstatePaymentAddress()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstatePaymentAddress.Populated);

    var street1 = export.InterstatePaymentAddress.Street1;
    var street2 = export.InterstatePaymentAddress.Street2 ?? "";
    var city = export.InterstatePaymentAddress.City;
    var zip5 = export.InterstatePaymentAddress.Zip5 ?? "";
    var addressEndDate = export.InterstatePaymentAddress.AddressEndDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var payableToName = export.InterstatePaymentAddress.PayableToName ?? "";
    var state = export.InterstatePaymentAddress.State ?? "";
    var zipCode = export.InterstatePaymentAddress.ZipCode ?? "";
    var zip4 = export.InterstatePaymentAddress.Zip4 ?? "";
    var zip3 = export.InterstatePaymentAddress.Zip3 ?? "";
    var county = export.InterstatePaymentAddress.County ?? "";
    var street3 = export.InterstatePaymentAddress.Street3 ?? "";
    var street4 = export.InterstatePaymentAddress.Street4 ?? "";
    var province = export.InterstatePaymentAddress.Province ?? "";
    var postalCode = export.InterstatePaymentAddress.PostalCode ?? "";
    var country = export.InterstatePaymentAddress.Country ?? "";
    var locationType = export.InterstatePaymentAddress.LocationType;
    var fipsCounty = export.InterstatePaymentAddress.FipsCounty ?? "";
    var fipsState = export.InterstatePaymentAddress.FipsState ?? "";
    var fipsLocation = export.InterstatePaymentAddress.FipsLocation ?? "";

    CheckValid<InterstatePaymentAddress>("LocationType", locationType);
    entities.InterstatePaymentAddress.Populated = false;
    Update("UpdateInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetNullableString(command, "zip5", zip5);
        db.SetNullableDate(command, "addressEndDate", addressEndDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableString(command, "payableToName", payableToName);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "locationType", locationType);
        db.SetNullableString(command, "fipsCounty", fipsCounty);
        db.SetNullableString(command, "fipsState", fipsState);
        db.SetNullableString(command, "fipsLocation", fipsLocation);
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstatePaymentAddress.IntGeneratedId);
        db.SetDate(
          command, "addressStartDate",
          entities.InterstatePaymentAddress.AddressStartDate.
            GetValueOrDefault());
      });

    entities.InterstatePaymentAddress.Street1 = street1;
    entities.InterstatePaymentAddress.Street2 = street2;
    entities.InterstatePaymentAddress.City = city;
    entities.InterstatePaymentAddress.Zip5 = zip5;
    entities.InterstatePaymentAddress.AddressEndDate = addressEndDate;
    entities.InterstatePaymentAddress.LastUpdatedBy = lastUpdatedBy;
    entities.InterstatePaymentAddress.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstatePaymentAddress.PayableToName = payableToName;
    entities.InterstatePaymentAddress.State = state;
    entities.InterstatePaymentAddress.ZipCode = zipCode;
    entities.InterstatePaymentAddress.Zip4 = zip4;
    entities.InterstatePaymentAddress.Zip3 = zip3;
    entities.InterstatePaymentAddress.County = county;
    entities.InterstatePaymentAddress.Street3 = street3;
    entities.InterstatePaymentAddress.Street4 = street4;
    entities.InterstatePaymentAddress.Province = province;
    entities.InterstatePaymentAddress.PostalCode = postalCode;
    entities.InterstatePaymentAddress.Country = country;
    entities.InterstatePaymentAddress.LocationType = locationType;
    entities.InterstatePaymentAddress.FipsCounty = fipsCounty;
    entities.InterstatePaymentAddress.FipsState = fipsState;
    entities.InterstatePaymentAddress.FipsLocation = fipsLocation;
    entities.InterstatePaymentAddress.Populated = true;
  }

  private void UpdateInterstateRequest()
  {
    var otherStateCaseId = export.InterstateRequest.OtherStateCaseId ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var otherStateCaseStatus = export.InterstateRequest.OtherStateCaseStatus;
    var caseType = export.InterstateRequest.CaseType ?? "";
    var otherStateCaseClosureReason =
      export.InterstateRequest.OtherStateCaseClosureReason ?? "";
    var otherStateCaseClosureDate =
      local.InterstateRequest.OtherStateCaseClosureDate;

    entities.InterstateRequest.Populated = false;
    Update("UpdateInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "otherStateCasId", otherStateCaseId);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "othStCaseStatus", otherStateCaseStatus);
        db.SetNullableString(command, "caseType", caseType);
        db.SetNullableString(
          command, "othStateClsRes", otherStateCaseClosureReason);
        db.
          SetNullableDate(command, "othStateClsDte", otherStateCaseClosureDate);
          
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.OtherStateCaseId = otherStateCaseId;
    entities.InterstateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterstateRequest.OtherStateCaseStatus = otherStateCaseStatus;
    entities.InterstateRequest.CaseType = caseType;
    entities.InterstateRequest.OtherStateCaseClosureReason =
      otherStateCaseClosureReason;
    entities.InterstateRequest.OtherStateCaseClosureDate =
      otherStateCaseClosureDate;
    entities.InterstateRequest.Populated = true;
  }

  private void UpdateInterstateRequestHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateRequestHistory.Populated);

    var note = export.InterstateRequestHistory.Note ?? "";

    entities.InterstateRequestHistory.Populated = false;
    Update("UpdateInterstateRequestHistory",
      (db, command) =>
      {
        db.SetNullableString(command, "note", note);
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequestHistory.IntGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          entities.InterstateRequestHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.InterstateRequestHistory.Note = note;
    entities.InterstateRequestHistory.Populated = true;
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
    /// A value of CaseMarkedDuplicate.
    /// </summary>
    [JsonPropertyName("caseMarkedDuplicate")]
    public Common CaseMarkedDuplicate
    {
      get => caseMarkedDuplicate ??= new();
      set => caseMarkedDuplicate = value;
    }

    /// <summary>
    /// A value of CaseClosed.
    /// </summary>
    [JsonPropertyName("caseClosed")]
    public Common CaseClosed
    {
      get => caseClosed ??= new();
      set => caseClosed = value;
    }

    /// <summary>
    /// A value of ChangeProgram.
    /// </summary>
    [JsonPropertyName("changeProgram")]
    public Common ChangeProgram
    {
      get => changeProgram ??= new();
      set => changeProgram = value;
    }

    /// <summary>
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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

    private Common caseMarkedDuplicate;
    private Common caseClosed;
    private Common changeProgram;
    private Fips otherState;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private InterstateRequestHistory interstateRequestHistory;
    private CsePerson ap;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseStatusChanged.
    /// </summary>
    [JsonPropertyName("caseStatusChanged")]
    public Common CaseStatusChanged
    {
      get => caseStatusChanged ??= new();
      set => caseStatusChanged = value;
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
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of IreqCreated.
    /// </summary>
    [JsonPropertyName("ireqCreated")]
    public DateWorkArea IreqCreated
    {
      get => ireqCreated ??= new();
      set => ireqCreated = value;
    }

    /// <summary>
    /// A value of IreqUpdated.
    /// </summary>
    [JsonPropertyName("ireqUpdated")]
    public DateWorkArea IreqUpdated
    {
      get => ireqUpdated ??= new();
      set => ireqUpdated = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    private Common caseStatusChanged;
    private Case1 case1;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private DateWorkArea ireqCreated;
    private DateWorkArea ireqUpdated;
    private InterstateCase interstateCase;
    private CsePersonsWorkSet ap;
    private Fips otherState;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of ZeroFips.
    /// </summary>
    [JsonPropertyName("zeroFips")]
    public Fips ZeroFips
    {
      get => zeroFips ??= new();
      set => zeroFips = value;
    }

    /// <summary>
    /// A value of Iicnv.
    /// </summary>
    [JsonPropertyName("iicnv")]
    public InterstateRequestHistory Iicnv
    {
      get => iicnv ??= new();
      set => iicnv = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public Common Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Yes.
    /// </summary>
    [JsonPropertyName("yes")]
    public Common Yes
    {
      get => yes ??= new();
      set => yes = value;
    }

    /// <summary>
    /// A value of HcOtCVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntary")]
    public ObligationType HcOtCVoluntary
    {
      get => hcOtCVoluntary ??= new();
      set => hcOtCVoluntary = value;
    }

    /// <summary>
    /// A value of HcOtCAccruing.
    /// </summary>
    [JsonPropertyName("hcOtCAccruing")]
    public ObligationType HcOtCAccruing
    {
      get => hcOtCAccruing ??= new();
      set => hcOtCAccruing = value;
    }

    /// <summary>
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

    /// <summary>
    /// A value of Susp.
    /// </summary>
    [JsonPropertyName("susp")]
    public CashReceiptDetailStatus Susp
    {
      get => susp ??= new();
      set => susp = value;
    }

    /// <summary>
    /// A value of Pend.
    /// </summary>
    [JsonPropertyName("pend")]
    public CashReceiptDetailStatus Pend
    {
      get => pend ??= new();
      set => pend = value;
    }

    /// <summary>
    /// A value of SavedObligationType.
    /// </summary>
    [JsonPropertyName("savedObligationType")]
    public ObligationType SavedObligationType
    {
      get => savedObligationType ??= new();
      set => savedObligationType = value;
    }

    /// <summary>
    /// A value of SavedObligation.
    /// </summary>
    [JsonPropertyName("savedObligation")]
    public Obligation SavedObligation
    {
      get => savedObligation ??= new();
      set => savedObligation = value;
    }

    /// <summary>
    /// A value of Derived.
    /// </summary>
    [JsonPropertyName("derived")]
    public ScreenObligationStatus Derived
    {
      get => derived ??= new();
      set => derived = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Closure.
    /// </summary>
    [JsonPropertyName("closure")]
    public Common Closure
    {
      get => closure ??= new();
      set => closure = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of InterstateProgram.
    /// </summary>
    [JsonPropertyName("interstateProgram")]
    public Program InterstateProgram
    {
      get => interstateProgram ??= new();
      set => interstateProgram = value;
    }

    /// <summary>
    /// A value of InterstateChild.
    /// </summary>
    [JsonPropertyName("interstateChild")]
    public CsePersonsWorkSet InterstateChild
    {
      get => interstateChild ??= new();
      set => interstateChild = value;
    }

    /// <summary>
    /// A value of InterstatePersonProgram.
    /// </summary>
    [JsonPropertyName("interstatePersonProgram")]
    public PersonProgram InterstatePersonProgram
    {
      get => interstatePersonProgram ??= new();
      set => interstatePersonProgram = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of ZeroDateWorkArea.
    /// </summary>
    [JsonPropertyName("zeroDateWorkArea")]
    public DateWorkArea ZeroDateWorkArea
    {
      get => zeroDateWorkArea ??= new();
      set => zeroDateWorkArea = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of InterstateExist.
    /// </summary>
    [JsonPropertyName("interstateExist")]
    public Common InterstateExist
    {
      get => interstateExist ??= new();
      set => interstateExist = value;
    }

    /// <summary>
    /// A value of InterstateRequestCount.
    /// </summary>
    [JsonPropertyName("interstateRequestCount")]
    public Common InterstateRequestCount
    {
      get => interstateRequestCount ??= new();
      set => interstateRequestCount = value;
    }

    /// <summary>
    /// A value of MultipleIrForAp.
    /// </summary>
    [JsonPropertyName("multipleIrForAp")]
    public Common MultipleIrForAp
    {
      get => multipleIrForAp ??= new();
      set => multipleIrForAp = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public Common InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateContactAddress interstateContactAddress;
    private Fips zeroFips;
    private InterstateRequestHistory iicnv;
    private Common open;
    private Common yes;
    private ObligationType hcOtCVoluntary;
    private ObligationType hcOtCAccruing;
    private CsePersonAccount hcCpaObligor;
    private CashReceiptDetailStatus susp;
    private CashReceiptDetailStatus pend;
    private ObligationType savedObligationType;
    private Obligation savedObligation;
    private ScreenObligationStatus derived;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common closure;
    private InterstateRequest interstateRequest;
    private Infrastructure infrastructure;
    private Program interstateProgram;
    private CsePersonsWorkSet interstateChild;
    private PersonProgram interstatePersonProgram;
    private DateWorkArea max;
    private DateWorkArea zeroDateWorkArea;
    private DateWorkArea current;
    private Common interstateExist;
    private Common interstateRequestCount;
    private Common multipleIrForAp;
    private Common interstateCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public CashReceiptDetailStatus KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingCashReceiptDetailStatHistory
    {
      get => existingCashReceiptDetailStatHistory ??= new();
      set => existingCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CaseRole Child1
    {
      get => child1 ??= new();
      set => child1 = value;
    }

    /// <summary>
    /// A value of Child2.
    /// </summary>
    [JsonPropertyName("child2")]
    public CsePerson Child2
    {
      get => child2 ??= new();
      set => child2 = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ExistingPersonProgram.
    /// </summary>
    [JsonPropertyName("existingPersonProgram")]
    public PersonProgram ExistingPersonProgram
    {
      get => existingPersonProgram ??= new();
      set => existingPersonProgram = value;
    }

    /// <summary>
    /// A value of ExistingProgram.
    /// </summary>
    [JsonPropertyName("existingProgram")]
    public Program ExistingProgram
    {
      get => existingProgram ??= new();
      set => existingProgram = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Fips State
    {
      get => state ??= new();
      set => state = value;
    }

    private InterstateRequestObligation interstateRequestObligation;
    private ObligationTransaction obligationTransaction;
    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private CsePersonAccount obligor;
    private CashReceiptDetailStatus keyOnly;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
    private CashReceiptDetail existingCashReceiptDetail;
    private InterstateCase interstateCase;
    private CaseUnit caseUnit;
    private Infrastructure infrastructure;
    private CaseRole child1;
    private CsePerson child2;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private InterstateRequestHistory interstateRequestHistory;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseRole absentParent;
    private CsePerson ap;
    private PersonProgram existingPersonProgram;
    private Program existingProgram;
    private Fips state;
  }
#endregion
}
