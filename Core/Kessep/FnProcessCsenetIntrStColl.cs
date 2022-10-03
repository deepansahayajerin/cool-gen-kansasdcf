// Program: FN_PROCESS_CSENET_INTR_ST_COLL, ID: 372623964, model: 746.
// Short name: SWE01638
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PROCESS_CSENET_INTR_ST_COLL.
/// </summary>
[Serializable]
public partial class FnProcessCsenetIntrStColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_CSENET_INTR_ST_COLL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessCsenetIntrStColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessCsenetIntrStColl.
  /// </summary>
  public FnProcessCsenetIntrStColl(IContext context, Import import,
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
    // 04/01/2000   swsrpdp   Added code to retrieve information from AP_ID 
    // block if Case Not valid
    // 08/26/2000   swsrpdp   H00102107 Added code to set Kansas Case Number 
    // depending on position of first character enetered.
    // 05/16/2001   swsrpdp   H00118617 Fix logic for Retrieving Legal_Action - 
    // Person Information
    // *****
    // Hardcoded Area.
    // *****
    UseFnHardcodedCashReceipting();
    local.HardcodedViews.HardcodedCsenet.SystemGeneratedIdentifier = 8;
    UseFnHardcodedDebtDistribution();

    // *****
    // Determine if there is enough information to release the CRD for 
    // distribution.  If not suspend it
    // *****
    local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedRelease.SystemGeneratedIdentifier;
    local.CashReceiptDetailStatHistory.ReasonCodeId = "";

    // *****
    // Make sure currency has not been lost.  If it has, reread the persistent 
    // view.
    // *****
    if (!import.P.Populated)
    {
      if (!ReadCashReceipt())
      {
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }
    }

    // * * Read the Interstate_Case based on the PASSED KEY Information
    if (ReadInterstateCaseInterstateCollection())
    {
      // The location of the case number and format have NOT been finalized as 
      // of when this went into Production
      // 08/26/2000   swsrpdp   H00102107 Added code to set Kansas Case Number 
      // depending on position of first character enetered.
      // * *     LEFT Justify whatever is in the Case_ID field
      // * *     and - IF NEEDED - Truncate to 10 bytes
      local.Test.Number = "";

      if (!IsEmpty(entities.InterstateCase.KsCaseId))
      {
        if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 1, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 1, 10);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 2, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 2, 10);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 3, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 3, 10);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 4, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 4, 10);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 5, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 5, 10);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 6, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 6, 10);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 7, 1)))
        {
          local.Test.Number = Substring(entities.InterstateCase.KsCaseId, 7, 9);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 8, 1)))
        {
          local.Test.Number = Substring(entities.InterstateCase.KsCaseId, 8, 8);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 9, 1)))
        {
          local.Test.Number = Substring(entities.InterstateCase.KsCaseId, 9, 7);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 10, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 10, 6);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 11, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 11, 5);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 12, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 12, 4);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 13, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 13, 3);
        }
        else if (!IsEmpty(Substring(entities.InterstateCase.KsCaseId, 14, 1)))
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 14, 2);
        }
        else
        {
          local.Test.Number =
            Substring(entities.InterstateCase.KsCaseId, 15, 1);
        }
      }

      // 05/15/2001  I00118617
      // * * VALIDATE the Kansas Case Number
      local.CashReceiptDetail.CaseNumber = local.Test.Number;

      if (!IsEmpty(local.Test.Number))
      {
        if (!ReadCase())
        {
          // INVALID CASE
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;

          // *****
          // Changed reason_code to "INVCASENBR" per "UNIT TEST PLAN" by Tim 
          // Hood             for SWEB612.
          // *****
          local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCASENBR";
        }
      }
      else
      {
        // INVALID CASE ID = SPACES
        local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
        local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCASENBR";
      }

      // * * We need the number whether it is Valid or Not
      local.CashReceiptDetail.CaseNumber = local.Test.Number;

      // * * IF there is an AP_IDENTIFICATION record sent with the 
      // Interstate_Case
      // * *    - RETRIEVE it
      // * *    - and place data in local Cash_Receipt_Detail (work area)
      if (ReadInterstateApIdentification())
      {
        local.CashReceiptDetail.ObligorFirstName =
          entities.InterstateApIdentification.NameFirst;
        local.CashReceiptDetail.ObligorMiddleName =
          entities.InterstateApIdentification.MiddleName;
        local.CashReceiptDetail.ObligorLastName =
          entities.InterstateApIdentification.NameLast;
        local.CashReceiptDetail.ObligorSocialSecurityNumber =
          entities.InterstateApIdentification.Ssn;
      }

      // ****************************************************************
      // If SSN is ALL ZERO's -- Set it to ALL SPACES.
      // ****************************************************************
      if (Equal(local.CashReceiptDetail.ObligorSocialSecurityNumber, "000000000"))
        
      {
        local.CashReceiptDetail.ObligorSocialSecurityNumber = "";
      }

      // * * TRY to determine the CSE_PERSON NUMBER
      if (IsEmpty(local.CashReceiptDetail.ObligorSocialSecurityNumber))
      {
        // * * No SSN sent from CSENet
        if (!IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
        {
          // * * SUSPENDED for Invalid Case - Cannot determine CSE_Person
          goto Test1;
        }

        // * * VALIDATE CASE _ Person
        // * * Determine How Many AP Cse_Persons are on this case
        local.CountAps.Count = 0;

        foreach(var item in ReadCaseRoleCsePersonCsePersonAccount3())
        {
          if (Equal(entities.CsePerson.Number,
            local.CashReceiptDetail.ObligorPersonNumber))
          {
            continue;
          }

          ++local.CountAps.Count;
          local.CashReceiptDetail.ObligorPersonNumber =
            entities.CsePerson.Number;
        }

        if (local.CountAps.Count == 0)
        {
          // * * NO Person_Number FOUND for this Case --  SUSPEND
          if (IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
          {
            local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
              local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
              
            local.CashReceiptDetailStatHistory.ReasonCodeId = "INVPERSNBR";
          }

          local.CashReceiptDetail.ObligorPersonNumber = "";

          goto Test1;
        }

        if (local.CountAps.Count > 1)
        {
          // * * MULTIPLE Person_Number FOUND for this Case
          // * *  --  SUSPEND
          // * * We Did NOT recieve an SSN
          // * *      so we Do Not know which Cse_Person is the one to use
          if (IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
          {
            local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
              local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
              
            local.CashReceiptDetailStatHistory.ReasonCodeId = "INVPERSNBR";
          }

          local.CashReceiptDetail.ObligorPersonNumber = "";

          goto Test1;
        }

        // * * Retrieve the CSE_Person Information
        local.Adabas.Assign(local.BlankAdabas);
        local.PersonNumber.Number =
          local.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
        UseSiReadCsePersonBatch();
        ExitState = "ACO_NN0000_ALL_OK";
      }
      else
      {
        // * * We Recieved an SSN from CSENet
        // * * --- Check if AP SSN is valid and get the corresponding 
        // Person_Number ---
        local.CsePersonsWorkSet.Assign(local.Blank);
        local.SsnSearch.Flag = "1";
        local.CsePersonsWorkSet.Ssn =
          local.CashReceiptDetail.ObligorSocialSecurityNumber ?? Spaces(9);
        UseEabReadCsePersonUsingSsn();

        if (IsEmpty(local.CsePersonsWorkSet.Number))
        {
          // * * INVALID AP_SSN - NO DATA Retrieved
          if (IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
          {
            local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
              local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
              
            local.CashReceiptDetailStatHistory.ReasonCodeId = "INVSSN";
          }

          goto Test1;
        }

        if (!IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
        {
          // * * SUSPENDED for Invalid Case - Cannot get for a specific CASE
          // * * VALIDATE CSE _ Person is AP on ONLY one CASE
          ExitState = "ACO_NN0000_ALL_OK";
          local.CashReceiptDetail.ObligorPersonNumber = "";
          local.CountAps.Count = 0;

          foreach(var item in ReadCaseRoleCsePersonCsePersonAccount2())
          {
            ++local.CountAps.Count;
            local.CashReceiptDetail.ObligorPersonNumber =
              entities.CsePerson.Number;
          }

          if (local.CountAps.Count == 0)
          {
            // * * Person_Number -- is NOT involved in ANY CASEs
            local.CashReceiptDetail.ObligorPersonNumber = "";

            goto Test1;
          }

          if (local.CountAps.Count > 1)
          {
            // * * Person_Number -- is involved in MULTIPLE CASEs
            local.CashReceiptDetail.ObligorPersonNumber = "";

            goto Test1;
          }
        }
        else
        {
          // * * NOT SUSPENDED - Get for a specific CASE
          // * * VALIDATE CSE _ Person is AP on SPECIFIC CASE
          ExitState = "ACO_NN0000_ALL_OK";
          local.CashReceiptDetail.ObligorPersonNumber = "";
          local.CountAps.Count = 0;

          if (ReadCaseRoleCsePersonCsePersonAccount1())
          {
            ++local.CountAps.Count;
            local.CashReceiptDetail.ObligorPersonNumber =
              entities.CsePerson.Number;
          }

          if (local.CountAps.Count == 0)
          {
            // * * INVALID Person_Number -- CASE NUMBER combination
            if (IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
            {
              local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
                local.HardcodedViews.HardcodedSuspended.
                  SystemGeneratedIdentifier;
              local.CashReceiptDetailStatHistory.ReasonCodeId = "INVSSN";
              local.CashReceiptDetail.ObligorPersonNumber = "";
            }

            goto Test1;
          }
        }

        // * * The CSE_Person Number is valid - Get Persons Information
        local.Adabas.Assign(local.BlankAdabas);
        local.PersonNumber.Number =
          local.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
        UseSiReadCsePersonBatch();
      }

Test1:

      // ***  Retrieve and VALIDATE Court_Order NUMBER
      if (IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
      {
        // * * This collection HAS NOT been SUSPENDED
        // * * Retrieve and VALIDATE Court_Order NUMBER
        // * * By PERSON and CASE
        local.CashReceiptDetail.CourtOrderNumber = "";
        local.CountLegalActions.Count = 0;

        foreach(var item in ReadLegalAction1())
        {
          if (Equal(local.CashReceiptDetail.CourtOrderNumber,
            entities.LegalAction.StandardNumber))
          {
            continue;
          }

          local.CashReceiptDetail.CourtOrderNumber =
            entities.LegalAction.StandardNumber;
          ++local.CountLegalActions.Count;
        }

        if (local.CountLegalActions.Count != 1)
        {
          // * * IF there are NONE or if MULTIPLE Legal Actions exist
          // * *  SUSPEND
          local.CashReceiptDetail.CourtOrderNumber = "";
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
          local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCTORDER";
        }
      }
      else
      {
        // * * This collection HAS been SUSPENDED
        // * * Determine if it is POSSIBLE
        // * *       to Retrieve and VALIDATE Court_Order NUMBER
        if (!Equal(local.CashReceiptDetailStatHistory.ReasonCodeId, "INVCASENBR"))
          
        {
          // * * This collection HAS NOT been SUSPENDED for INVALID CASE
          // * * Since the CASE is Valid - see if it has Only ONE Legal Action
          local.CashReceiptDetail.CourtOrderNumber = "";
          local.CountLegalActions.Count = 0;

          foreach(var item in ReadLegalAction2())
          {
            if (Equal(local.CashReceiptDetail.CourtOrderNumber,
              entities.LegalAction.StandardNumber))
            {
              continue;
            }

            ++local.CountLegalActions.Count;
            local.CashReceiptDetail.CourtOrderNumber =
              entities.LegalAction.StandardNumber;
          }

          if (local.CountLegalActions.Count != 1)
          {
            // * * IF there are NONE or MULTIPLE Legal Actions
            // * * Blank Out Court Order
            if (!IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
            {
              local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCTORDER";
            }

            local.CashReceiptDetail.CourtOrderNumber = "";
          }

          // * * At this point the Court Order is either blank
          // * * - or the ONE Valid Court Order for this Case Number
        }
        else
        {
          // * * The CASE is NOT Valid - IF we have a CSE_Person Number
          // * * It is from the SSN They Sent
          if (!IsEmpty(local.CsePersonsWorkSet.Number))
          {
            // * * See if this Person has Only ONE Court Case
            local.CashReceiptDetail.CourtOrderNumber = "";
            local.CountLegalActions.Count = 0;

            foreach(var item in ReadLegalAction3())
            {
              if (Equal(local.CashReceiptDetail.CourtOrderNumber,
                entities.LegalAction.StandardNumber))
              {
                continue;
              }

              ++local.CountLegalActions.Count;
              local.CashReceiptDetail.CourtOrderNumber =
                entities.LegalAction.StandardNumber;
            }

            if (local.CountLegalActions.Count != 1)
            {
              // * * IF there are NONE or MULTIPLE Legal Actions
              // * * Blank Out Court Order
              // * *      - This has already been suspended
              // * *         for INVALID CASE
              local.CashReceiptDetail.CourtOrderNumber = "";
            }

            // * * At this point the Court Order is either blank
            // * * - or the ONE Valid Court Order for this Person
          }
        }
      }

      ExitState = "ACO_NN0000_ALL_OK";

      // * * * * * *
      // * * * SET UP the Remaining Fields for  CASH RECEIPT DETAIL
      // * * * * * *
      // 01/06/1999  SWSRPDP  Format the Obligor Home-Phone for the 
      // Cash_Receipt_Detail
      local.CashReceiptDetail.ObligorPhoneNumber = "";

      if (local.HomePhone.HomePhone.GetValueOrDefault() > 0)
      {
        if (local.HomePhone.HomePhoneAreaCode.GetValueOrDefault() > 0)
        {
          local.CashReceiptDetail.ObligorPhoneNumber =
            NumberToString(local.HomePhone.HomePhoneAreaCode.
              GetValueOrDefault(), 13, 3);
          local.CashReceiptDetail.ObligorPhoneNumber =
            Substring(local.CashReceiptDetail.ObligorPhoneNumber, 12, 1, 3) + NumberToString
            (local.HomePhone.HomePhone.GetValueOrDefault(), 9, 7);
        }
        else
        {
          local.CashReceiptDetail.ObligorPhoneNumber =
            NumberToString(local.HomePhone.HomePhone.GetValueOrDefault(), 9, 7);
            
        }
      }

      // * * * * * * * * * * * * * * * *
      // Set up the cash receipt detail in preparation for recording it
      // * * * * * * * * * * * * * * * *
      local.CashReceiptDetail.SequentialIdentifier = 1;

      if (ReadCashReceiptDetail1())
      {
        local.CashReceiptDetail.SequentialIdentifier =
          entities.GetSequence.SequentialIdentifier + 1;
      }

      local.CashReceiptDetail.CaseNumber = local.Test.Number;
      local.CashReceiptDetail.InterfaceTransId = "CSENET";
      local.CashReceiptDetail.ReceivedAmount =
        import.InterstateCollection.PaymentAmount.GetValueOrDefault();
      local.CashReceiptDetail.OffsetTaxid =
        (int?)StringToNumber(local.PersonNumber.Ssn);
      local.CashReceiptDetail.CollectionDate =
        import.InterstateCollection.DateOfCollection;
      local.CashReceiptDetail.OffsetTaxYear = Now().Date.Year;
      local.CashReceiptDetail.CollectionAmount =
        import.InterstateCollection.PaymentAmount.GetValueOrDefault();
      local.CashReceiptDetail.Reference =
        NumberToString(import.InterstateCollection.SystemGeneratedSequenceNum,
        15);

      // * * CSENet did NOT send Person Information
      // * * Use what we retrieved
      if (IsEmpty(local.CashReceiptDetail.ObligorFirstName) && IsEmpty
        (local.CashReceiptDetail.ObligorLastName) && IsEmpty
        (local.CashReceiptDetail.ObligorMiddleName))
      {
        local.CashReceiptDetail.ObligorFirstName = local.PersonNumber.FirstName;
        local.CashReceiptDetail.ObligorLastName = local.PersonNumber.LastName;
        local.CashReceiptDetail.ObligorMiddleName =
          local.PersonNumber.MiddleInitial;
      }

      if (IsEmpty(local.CashReceiptDetail.ObligorSocialSecurityNumber))
      {
        local.CashReceiptDetail.ObligorSocialSecurityNumber =
          local.PersonNumber.Ssn;

        // ****************************************************************
        // If SSN is ALL ZERO's -- Set it to ALL SPACES.
        // ****************************************************************
        if (Equal(local.CashReceiptDetail.ObligorSocialSecurityNumber,
          "000000000"))
        {
          local.CashReceiptDetail.ObligorSocialSecurityNumber = "";
        }
      }

      // * *
      // 08/26/2000   swsrpdp   H00102107 Added code to set Kansas Case Number 
      // depending on position of first character enetered.
      // * * This will allow DISPLAY of EXACTLY what CSENet sent
      local.CashReceiptDetail.Notes =
        "CSENET COLLECTION - KANSAS CASE # RECEIVED '" + entities
        .InterstateCase.KsCaseId + "'";
      ExitState = "ACO_NN0000_ALL_OK";

      // * * Determine what Multi-Payor Flag should be
      local.CashReceiptDetail.MultiPayor = "";

      if (!IsEmpty(local.CashReceiptDetail.ObligorPersonNumber) && !
        IsEmpty(local.CashReceiptDetail.CourtOrderNumber))
      {
        // * * Get Multi Payor Flag
        local.NumberOfObligors.Count = 0;
        local.ObligorList.Index = -1;
        local.ForComparison.StandardNumber =
          local.CashReceiptDetail.CourtOrderNumber ?? "";

        foreach(var item in ReadCsePersonLegalActionObligation())
        {
          if (local.ObligorList.Index == -1)
          {
          }
          else if (Equal(entities.CsePerson.Number,
            local.ObligorList.Item.Detail.Number))
          {
            // --- cse person already moved to group export
            continue;
          }

          if (local.ObligorList.Index >= 1)
          {
            break;
          }

          ++local.ObligorList.Index;
          local.ObligorList.CheckSize();

          local.ObligorList.Update.Detail.Number = entities.CsePerson.Number;
          ++local.NumberOfObligors.Count;
        }

        if (local.NumberOfObligors.Count > 1)
        {
          local.ObligorList.Index = 0;
          local.ObligorList.CheckSize();

          if (Equal(local.ObligorList.Item.Detail.Number,
            local.CashReceiptDetail.ObligorPersonNumber))
          {
            switch(AsChar(local.CsePersonsWorkSet.Sex))
            {
              case 'M':
                // "M"ale = "F"ather
                local.CashReceiptDetail.MultiPayor = "F";

                break;
              case 'F':
                // "F"emale = "M"other
                local.CashReceiptDetail.MultiPayor = "M";

                break;
              default:
                local.CashReceiptDetail.MultiPayor = "";

                break;
            }
          }
          else
          {
            local.ObligorList.Index = 1;
            local.ObligorList.CheckSize();

            if (Equal(local.ObligorList.Item.Detail.Number,
              local.CashReceiptDetail.ObligorPersonNumber))
            {
              switch(AsChar(local.CsePersonsWorkSet.Sex))
              {
                case 'M':
                  // "M"ale = "F"ather
                  local.CashReceiptDetail.MultiPayor = "F";

                  break;
                case 'F':
                  // "F"emale = "M"other
                  local.CashReceiptDetail.MultiPayor = "M";

                  break;
                default:
                  local.CashReceiptDetail.MultiPayor = "";

                  break;
              }
            }
          }
        }
        else
        {
          local.CashReceiptDetail.MultiPayor = "";
        }
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }
    else
    {
      ExitState = "CO0000_INTERSTATE_ENTITIES_NF_RB";

      return;
    }

    // *****
    // Read the appropriate Collection Type to pass to the Record CAB.
    // *****
    local.ValidCollectionType.Flag = "N";

    if (import.CollectionType.SequentialIdentifier > 0)
    {
      if (ReadCollectionType())
      {
        MoveCollectionType(entities.CollectionType, local.CollectionType);
        local.ValidCollectionType.Flag = "Y";
      }
      else
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // INVALID COLLECTION TYPE - SUSPEND
        // DO NOT overlay Previous Suspense Reason's
        // This should NEVER occur
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier)
        {
          goto Test2;
        }

        local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
        local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCOLTYPE";
      }
    }

Test2:

    // ****************************************************************
    // Validate the suspense reason code assigned to the cash receipt
    // detail status history record if a value has been assigned.
    // JLK  04/09/99
    // ****************************************************************
    if (!IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
    {
      local.ValidateCode.CodeName = "PEND/SUSP REASON";
      local.ValidateCodeValue.Cdvalue =
        local.CashReceiptDetailStatHistory.ReasonCodeId ?? Spaces(10);
      UseCabValidateCodeValue();

      if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
      {
        ExitState = "CODE_VALUE_NF_RB";

        return;
      }
    }

    // *****
    // Create / RECORD the cash receipt detail now.
    // *****
    ExitState = "ACO_NN0000_ALL_OK";
    UseRecordCollection();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // * *  RE-Read the Just Created Cash_Receipt_Detail to update the STATUS / 
    // Address
    if (ReadCashReceiptDetail2())
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // IF NO errors - RELEASE
      // Otherwise - SUSPEND
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      UseFnChangeCashRcptDtlStatHis();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }
    else
    {
      ExitState = "CASH_RECEIPT_DETAIL_NF";

      return;
    }

    // *****
    // 
    // Determine Type of STATUS on CR_Detail for REPORT TOTALS
    // *****
    export.ImportNumberOfRel.Count = 0;
    export.ImportNumberOfSusp.Count = 0;

    if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedRelease.SystemGeneratedIdentifier)
    {
      export.ImportNumberOfRel.Count = 1;
    }
    else
    {
      export.ImportNumberOfSusp.Count = 1;
    }

    // *****
    // 
    // RETRIEVE and ADD the Cash_Receipt_Detail_Address from our Data Base
    // *****
    // * * Use the ADDRESS "WE" have on file
    local.GetAddress.Number = local.CashReceiptDetail.ObligorPersonNumber ?? Spaces
      (10);
    UseSiGetCsePersonMailingAddr();

    if (!IsEmpty(local.Returned.Street1) || !IsEmpty(local.Returned.City))
    {
      local.Create.City = local.Returned.City ?? Spaces(30);
      local.Create.State = local.Returned.State ?? Spaces(2);
      local.Create.Street1 = local.Returned.Street1 ?? Spaces(25);
      local.Create.Street2 = local.Returned.Street2 ?? "";
      local.Create.ZipCode3 = local.Returned.Zip3 ?? "";
      local.Create.ZipCode4 = local.Returned.Zip4 ?? "";
      local.Create.ZipCode5 = local.Returned.ZipCode ?? Spaces(5);
    }

    // *****
    // 
    // ADD the Cash_Receipt_Detail_Address here if the input Record is found
    // *****
    if (!IsEmpty(local.Create.City) || !IsEmpty(local.Create.Street1))
    {
      try
      {
        CreateCashReceiptDetailAddress();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0038_CASH_RCPT_DTL_ADDR_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_CASH_REC_DTL_ADD_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****
    // Update the Interstate Collection to show date processed.
    // *****
    try
    {
      UpdateInterstateCollection();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "INTERSTATE_COLLECTION_PV";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_COLLECTION_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****
    // Update the Transaction Envelope to show it has been "P"rocessed.
    // *****
    if (ReadCsenetTransactionEnvelop())
    {
      try
      {
        UpdateCsenetTransactionEnvelop();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CSENET_ENVELOPE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CSENET_ENVELOPE_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveAbendData(AbendData source, AbendData target)
  {
    target.AdabasFileNumber = source.AdabasFileNumber;
    target.AdabasFileAction = source.AdabasFileAction;
    target.AdabasResponseCd = source.AdabasResponseCd;
    target.CicsResourceNm = source.CicsResourceNm;
    target.CicsFunctionCd = source.CicsFunctionCd;
    target.CicsResponseCd = source.CicsResponseCd;
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
    target.OffsetTaxid = source.OffsetTaxid;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.JointReturnInd = source.JointReturnInd;
    target.JointReturnName = source.JointReturnName;
    target.DefaultedCollectionDateInd = source.DefaultedCollectionDateInd;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
    target.ObligorPhoneNumber = source.ObligorPhoneNumber;
    target.Reference = source.Reference;
    target.Notes = source.Notes;
  }

  private static void MoveCashReceiptDetailStatHistory(
    CashReceiptDetailStatHistory source, CashReceiptDetailStatHistory target)
  {
    target.ReasonCodeId = source.ReasonCodeId;
    target.ReasonText = source.ReasonText;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.HomePhone = source.HomePhone;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.TaxId = source.TaxId;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.ValidateCode.CodeName;
    useImport.CodeValue.Cdvalue = local.ValidateCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.Adabas);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveAbendData(useExport.AbendData, local.Adabas);
  }

  private void UseFnChangeCashRcptDtlStatHis()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.Persistent.Assign(entities.Persistant);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      local.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedCsenet.SystemGeneratedIdentifier;
    MoveCashReceiptDetailStatHistory(local.CashReceiptDetailStatHistory,
      useImport.New1);
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfRel.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfSusp.Count;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);

    entities.Persistant.Assign(useImport.Persistent);
    export.ImportNumberOfRel.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfSusp.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedViews.HardcodedRelease.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedViews.HardcodedObligor.Type1 = useExport.CpaObligor.Type1;
  }

  private void UseRecordCollection()
  {
    var useImport = new RecordCollection.Import();
    var useExport = new RecordCollection.Export();

    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    MoveCashReceiptDetail(local.CashReceiptDetail, useImport.CashReceiptDetail);
    MoveCollectionType(local.CollectionType, useImport.CollectionType);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfRel.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfSusp.Count;

    Call(RecordCollection.Execute, useImport, useExport);

    MoveCashReceiptDetail(useExport.CashReceiptDetail, local.CashReceiptDetail);
    export.ImportNumberOfRel.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfSusp.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.GetAddress.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.Returned);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.PersonNumber.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.PersonNumber);
    local.CseAddressFound.Flag = useExport.Ae.Flag;
    local.Adabas.Assign(useExport.AbendData);
    MoveCsePerson(useExport.CsePerson, local.HomePhone);
  }

  private void CreateCashReceiptDetailAddress()
  {
    System.Diagnostics.Debug.Assert(entities.Persistant.Populated);

    var systemGeneratedIdentifier = Now();
    var street1 = local.Create.Street1;
    var street2 = local.Create.Street2 ?? "";
    var city = local.Create.City;
    var state = local.Create.State;
    var zipCode5 = local.Create.ZipCode5;
    var zipCode4 = local.Create.ZipCode4 ?? "";
    var zipCode3 = local.Create.ZipCode3 ?? "";
    var crtIdentifier = entities.Persistant.CrtIdentifier;
    var cstIdentifier = entities.Persistant.CstIdentifier;
    var crvIdentifier = entities.Persistant.CrvIdentifier;
    var crdIdentifier = entities.Persistant.SequentialIdentifier;

    entities.Create.Populated = false;
    Update("CreateCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(command, "crdetailAddressI", systemGeneratedIdentifier);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zipCode3", zipCode3);
        db.SetNullableInt32(command, "crtIdentifier", crtIdentifier);
        db.SetNullableInt32(command, "cstIdentifier", cstIdentifier);
        db.SetNullableInt32(command, "crvIdentifier", crvIdentifier);
        db.SetNullableInt32(command, "crdIdentifier", crdIdentifier);
      });

    entities.Create.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Create.Street1 = street1;
    entities.Create.Street2 = street2;
    entities.Create.City = city;
    entities.Create.State = state;
    entities.Create.ZipCode5 = zipCode5;
    entities.Create.ZipCode4 = zipCode4;
    entities.Create.ZipCode3 = zipCode3;
    entities.Create.CrtIdentifier = crtIdentifier;
    entities.Create.CstIdentifier = cstIdentifier;
    entities.Create.CrvIdentifier = crvIdentifier;
    entities.Create.CrdIdentifier = crdIdentifier;
    entities.Create.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Test.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePersonCsePersonAccount1()
  {
    entities.CaseRole.Populated = false;
    entities.CsePersonAccount.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePersonCsePersonAccount1",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber", local.CashReceiptDetail.CaseNumber ?? "");
        db.SetString(command, "numb", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CsePerson.Type1 = db.GetString(reader, 4);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 5);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 6);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 7);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 8);
        entities.CaseRole.Populated = true;
        entities.CsePersonAccount.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCsePersonAccount2()
  {
    entities.CaseRole.Populated = false;
    entities.CsePersonAccount.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCsePersonAccount2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CsePerson.Type1 = db.GetString(reader, 4);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 5);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 6);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 7);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 8);
        entities.CaseRole.Populated = true;
        entities.CsePersonAccount.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCsePersonAccount3()
  {
    entities.CaseRole.Populated = false;
    entities.CsePersonAccount.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCsePersonAccount3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CsePerson.Type1 = db.GetString(reader, 4);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 5);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 6);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 7);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 8);
        entities.CaseRole.Populated = true;
        entities.CsePersonAccount.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);

        return true;
      });
  }

  private bool ReadCashReceipt()
  {
    import.P.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        import.P.CrvIdentifier = db.GetInt32(reader, 0);
        import.P.CstIdentifier = db.GetInt32(reader, 1);
        import.P.CrtIdentifier = db.GetInt32(reader, 2);
        import.P.SequentialNumber = db.GetInt32(reader, 3);
        import.P.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail1()
  {
    entities.GetSequence.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.GetSequence.CrvIdentifier = db.GetInt32(reader, 0);
        entities.GetSequence.CstIdentifier = db.GetInt32(reader, 1);
        entities.GetSequence.CrtIdentifier = db.GetInt32(reader, 2);
        entities.GetSequence.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.GetSequence.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);
    entities.Persistant.Populated = false;

    return Read("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", local.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crtIdentifier", import.P.CrtIdentifier);
        db.SetInt32(command, "cstIdentifier", import.P.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", import.P.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.Persistant.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Persistant.CstIdentifier = db.GetInt32(reader, 1);
        entities.Persistant.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Persistant.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.Persistant.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonLegalActionObligation()
  {
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadCsePersonLegalActionObligation",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.ForComparison.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 2);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 3);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.Obligation.CpaType = db.GetString(reader, 8);
        entities.Obligation.CspNumber = db.GetString(reader, 9);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 11);
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
      });
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.UpdateCsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        entities.UpdateCsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.UpdateCsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.UpdateCsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.UpdateCsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.UpdateCsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 4);
        entities.UpdateCsenetTransactionEnvelop.Populated = true;
      });
  }

  private bool ReadInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;

    return Read("ReadInterstateApIdentification",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 2);
        entities.InterstateApIdentification.Sex =
          db.GetNullableString(reader, 3);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 4);
        entities.InterstateApIdentification.NameFirst = db.GetString(reader, 5);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 6);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 7);
        entities.InterstateApIdentification.Populated = true;
      });
  }

  private bool ReadInterstateCaseInterstateCollection()
  {
    entities.InterstateCollection.Populated = false;
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCaseInterstateCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "sysGeneratedId",
          import.InterstateCollection.SystemGeneratedSequenceNum);
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.InterstateCollection.CcaTransSerNum = db.GetInt64(reader, 1);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 2);
        entities.InterstateCollection.CcaTransactionDt = db.GetDate(reader, 2);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 3);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 4);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 5);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 6);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 7);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 8);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 9);
        entities.InterstateCollection.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 10);
        entities.InterstateCollection.DateOfPosting =
          db.GetNullableDate(reader, 11);
        entities.InterstateCollection.PaymentSource =
          db.GetNullableString(reader, 12);
        entities.InterstateCollection.Populated = true;
        entities.InterstateCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction1",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber", local.CashReceiptDetail.CaseNumber ?? "");
        db.SetString(
          command, "cspNumber", local.CashReceiptDetail.ObligorPersonNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber", local.CashReceiptDetail.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private void UpdateCsenetTransactionEnvelop()
  {
    System.Diagnostics.Debug.Assert(
      entities.UpdateCsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var processingStatusCode = "P";

    entities.UpdateCsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "processingStatus", processingStatusCode);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.UpdateCsenetTransactionEnvelop.CcaTransactionDt.
            GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.UpdateCsenetTransactionEnvelop.CcaTransSerNum);
      });

    entities.UpdateCsenetTransactionEnvelop.LastUpdatedBy = lastUpdatedBy;
    entities.UpdateCsenetTransactionEnvelop.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.UpdateCsenetTransactionEnvelop.ProcessingStatusCode =
      processingStatusCode;
    entities.UpdateCsenetTransactionEnvelop.Populated = true;
  }

  private void UpdateInterstateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateCollection.Populated);

    var dateOfPosting = import.ProgramProcessingInfo.ProcessDate;

    entities.InterstateCollection.Populated = false;
    Update("UpdateInterstateCollection",
      (db, command) =>
      {
        db.SetNullableDate(command, "dateOfPosting", dateOfPosting);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCollection.CcaTransactionDt.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.InterstateCollection.CcaTransSerNum);
        db.SetInt32(
          command, "sysGeneratedId",
          entities.InterstateCollection.SystemGeneratedSequenceNum);
      });

    entities.InterstateCollection.DateOfPosting = dateOfPosting;
    entities.InterstateCollection.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
    }

    /// <summary>
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public CashReceipt P
    {
      get => p ??= new();
      set => p = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private InterstateCase interstateCase;
    private CashReceipt cashReceipt;
    private CollectionType collectionType;
    private InterstateCollection interstateCollection;
    private CashReceipt p;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImportNextCsenetId.
    /// </summary>
    [JsonPropertyName("importNextCsenetId")]
    public CashReceiptDetail ImportNextCsenetId
    {
      get => importNextCsenetId ??= new();
      set => importNextCsenetId = value;
    }

    /// <summary>
    /// A value of ImportNumberOfRel.
    /// </summary>
    [JsonPropertyName("importNumberOfRel")]
    public Common ImportNumberOfRel
    {
      get => importNumberOfRel ??= new();
      set => importNumberOfRel = value;
    }

    /// <summary>
    /// A value of ImportNumberOfSusp.
    /// </summary>
    [JsonPropertyName("importNumberOfSusp")]
    public Common ImportNumberOfSusp
    {
      get => importNumberOfSusp ??= new();
      set => importNumberOfSusp = value;
    }

    private CashReceiptDetail importNextCsenetId;
    private Common importNumberOfRel;
    private Common importNumberOfSusp;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ObligorListGroup group.</summary>
    [Serializable]
    public class ObligorListGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePersonsWorkSet detail;
    }

    /// <summary>A HardcodedViewsGroup group.</summary>
    [Serializable]
    public class HardcodedViewsGroup
    {
      /// <summary>
      /// A value of HardcodedObligor.
      /// </summary>
      [JsonPropertyName("hardcodedObligor")]
      public CsePersonAccount HardcodedObligor
      {
        get => hardcodedObligor ??= new();
        set => hardcodedObligor = value;
      }

      /// <summary>
      /// A value of HardcodedRelease.
      /// </summary>
      [JsonPropertyName("hardcodedRelease")]
      public CashReceiptDetailStatus HardcodedRelease
      {
        get => hardcodedRelease ??= new();
        set => hardcodedRelease = value;
      }

      /// <summary>
      /// A value of HardcodedCsenet.
      /// </summary>
      [JsonPropertyName("hardcodedCsenet")]
      public CashReceiptType HardcodedCsenet
      {
        get => hardcodedCsenet ??= new();
        set => hardcodedCsenet = value;
      }

      /// <summary>
      /// A value of HardcodedRecorded.
      /// </summary>
      [JsonPropertyName("hardcodedRecorded")]
      public CashReceiptDetailStatus HardcodedRecorded
      {
        get => hardcodedRecorded ??= new();
        set => hardcodedRecorded = value;
      }

      /// <summary>
      /// A value of HardcodedSuspended.
      /// </summary>
      [JsonPropertyName("hardcodedSuspended")]
      public CashReceiptDetailStatus HardcodedSuspended
      {
        get => hardcodedSuspended ??= new();
        set => hardcodedSuspended = value;
      }

      private CsePersonAccount hardcodedObligor;
      private CashReceiptDetailStatus hardcodedRelease;
      private CashReceiptType hardcodedCsenet;
      private CashReceiptDetailStatus hardcodedRecorded;
      private CashReceiptDetailStatus hardcodedSuspended;
    }

    /// <summary>
    /// A value of CountLegalActions.
    /// </summary>
    [JsonPropertyName("countLegalActions")]
    public Common CountLegalActions
    {
      get => countLegalActions ??= new();
      set => countLegalActions = value;
    }

    /// <summary>
    /// A value of CountAps.
    /// </summary>
    [JsonPropertyName("countAps")]
    public Common CountAps
    {
      get => countAps ??= new();
      set => countAps = value;
    }

    /// <summary>
    /// Gets a value of ObligorList.
    /// </summary>
    [JsonIgnore]
    public Array<ObligorListGroup> ObligorList => obligorList ??= new(
      ObligorListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ObligorList for json serialization.
    /// </summary>
    [JsonPropertyName("obligorList")]
    [Computed]
    public IList<ObligorListGroup> ObligorList_Json
    {
      get => obligorList;
      set => ObligorList.Assign(value);
    }

    /// <summary>
    /// A value of BlankAdabas.
    /// </summary>
    [JsonPropertyName("blankAdabas")]
    public AbendData BlankAdabas
    {
      get => blankAdabas ??= new();
      set => blankAdabas = value;
    }

    /// <summary>
    /// A value of PersonNumber.
    /// </summary>
    [JsonPropertyName("personNumber")]
    public CsePersonsWorkSet PersonNumber
    {
      get => personNumber ??= new();
      set => personNumber = value;
    }

    /// <summary>
    /// A value of Multipayor.
    /// </summary>
    [JsonPropertyName("multipayor")]
    public CashReceiptDetail Multipayor
    {
      get => multipayor ??= new();
      set => multipayor = value;
    }

    /// <summary>
    /// A value of NumberOfObligors.
    /// </summary>
    [JsonPropertyName("numberOfObligors")]
    public Common NumberOfObligors
    {
      get => numberOfObligors ??= new();
      set => numberOfObligors = value;
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
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public Case1 Test
    {
      get => test ??= new();
      set => test = value;
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
    /// Gets a value of HardcodedViews.
    /// </summary>
    [JsonPropertyName("hardcodedViews")]
    public HardcodedViewsGroup HardcodedViews
    {
      get => hardcodedViews ?? (hardcodedViews = new());
      set => hardcodedViews = value;
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CseAddressFound.
    /// </summary>
    [JsonPropertyName("cseAddressFound")]
    public Common CseAddressFound
    {
      get => cseAddressFound ??= new();
      set => cseAddressFound = value;
    }

    /// <summary>
    /// A value of ValidCollectionType.
    /// </summary>
    [JsonPropertyName("validCollectionType")]
    public Common ValidCollectionType
    {
      get => validCollectionType ??= new();
      set => validCollectionType = value;
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
    /// A value of SsnSearch.
    /// </summary>
    [JsonPropertyName("ssnSearch")]
    public Common SsnSearch
    {
      get => ssnSearch ??= new();
      set => ssnSearch = value;
    }

    /// <summary>
    /// A value of Adabas.
    /// </summary>
    [JsonPropertyName("adabas")]
    public AbendData Adabas
    {
      get => adabas ??= new();
      set => adabas = value;
    }

    /// <summary>
    /// A value of ValidSsn.
    /// </summary>
    [JsonPropertyName("validSsn")]
    public Common ValidSsn
    {
      get => validSsn ??= new();
      set => validSsn = value;
    }

    /// <summary>
    /// A value of CsePersonFound.
    /// </summary>
    [JsonPropertyName("csePersonFound")]
    public Common CsePersonFound
    {
      get => csePersonFound ??= new();
      set => csePersonFound = value;
    }

    /// <summary>
    /// A value of GetAddress.
    /// </summary>
    [JsonPropertyName("getAddress")]
    public CsePerson GetAddress
    {
      get => getAddress ??= new();
      set => getAddress = value;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public CsePersonAddress Returned
    {
      get => returned ??= new();
      set => returned = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public CashReceiptDetailAddress Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of HomePhone.
    /// </summary>
    [JsonPropertyName("homePhone")]
    public CsePerson HomePhone
    {
      get => homePhone ??= new();
      set => homePhone = value;
    }

    /// <summary>
    /// A value of ValidateCode.
    /// </summary>
    [JsonPropertyName("validateCode")]
    public Code ValidateCode
    {
      get => validateCode ??= new();
      set => validateCode = value;
    }

    /// <summary>
    /// A value of ValidateCodeValue.
    /// </summary>
    [JsonPropertyName("validateCodeValue")]
    public CodeValue ValidateCodeValue
    {
      get => validateCodeValue ??= new();
      set => validateCodeValue = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of ForComparison.
    /// </summary>
    [JsonPropertyName("forComparison")]
    public LegalAction ForComparison
    {
      get => forComparison ??= new();
      set => forComparison = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CsePersonsWorkSet Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private Common countLegalActions;
    private Common countAps;
    private Array<ObligorListGroup> obligorList;
    private AbendData blankAdabas;
    private CsePersonsWorkSet personNumber;
    private CashReceiptDetail multipayor;
    private Common numberOfObligors;
    private External external;
    private Case1 test;
    private LegalAction legalAction;
    private HardcodedViewsGroup hardcodedViews;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private Common cseAddressFound;
    private Common validCollectionType;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common ssnSearch;
    private AbendData adabas;
    private Common validSsn;
    private Common csePersonFound;
    private CsePerson getAddress;
    private CsePersonAddress returned;
    private CashReceiptDetailAddress create;
    private CsePerson homePhone;
    private Code validateCode;
    private CodeValue validateCodeValue;
    private Common returnCode;
    private LegalAction forComparison;
    private CsePersonsWorkSet blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public CashReceiptDetailAddress Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of Persistant.
    /// </summary>
    [JsonPropertyName("persistant")]
    public CashReceiptDetail Persistant
    {
      get => persistant ??= new();
      set => persistant = value;
    }

    /// <summary>
    /// A value of GetSequence.
    /// </summary>
    [JsonPropertyName("getSequence")]
    public CashReceiptDetail GetSequence
    {
      get => getSequence ??= new();
      set => getSequence = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of UpdateCsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("updateCsenetTransactionEnvelop")]
    public CsenetTransactionEnvelop UpdateCsenetTransactionEnvelop
    {
      get => updateCsenetTransactionEnvelop ??= new();
      set => updateCsenetTransactionEnvelop = value;
    }

    /// <summary>
    /// A value of UpdatePaReferral.
    /// </summary>
    [JsonPropertyName("updatePaReferral")]
    public PaReferral UpdatePaReferral
    {
      get => updatePaReferral ??= new();
      set => updatePaReferral = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Tbd.
    /// </summary>
    [JsonPropertyName("tbd")]
    public InterstateApIdentification Tbd
    {
      get => tbd ??= new();
      set => tbd = value;
    }

    private InterstateApIdentification interstateApIdentification;
    private CashReceiptDetailAddress create;
    private CashReceiptDetail persistant;
    private CashReceiptDetail getSequence;
    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole caseRole;
    private CsenetTransactionEnvelop updateCsenetTransactionEnvelop;
    private PaReferral updatePaReferral;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private Case1 case1;
    private InterstateCollection interstateCollection;
    private InterstateCase interstateCase;
    private CollectionType collectionType;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private InterstateApIdentification tbd;
  }
#endregion
}
