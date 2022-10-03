// Program: FN_READ_SUPPORT_OBLIGATION, ID: 372095436, model: 746.
// Short name: SWE00585
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
/// A program: FN_READ_SUPPORT_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This CAB will read the CSE Person, Obligation, Obligation Transaction (&amp;
/// related entities) and all of the support CSE Person's associated to the
/// Obligation.
/// Required Import Views:
/// 	CSE Person Number
/// 	Obligation Sys Gen Id
/// 	Obligation Transaction Sys Gen ID
/// </para>
/// </summary>
[Serializable]
public partial class FnReadSupportObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_SUPPORT_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadSupportObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadSupportObligation.
  /// </summary>
  public FnReadSupportObligation(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // =================================================
    // ==
    // ==		Used by ONAC only
    // ==
    // =================================================
    // ************************************************
    // * Date	Developer	Description
    // *050197	Sumanta - MTW   Changed the use of DB2 current date to IEF 
    // current date
    // 3/23/98		Siraj Konkader			ZDEL cleanup
    // ************************************************
    // 9-3-98   B Adams    Deleted fn-hardcode-debt-detail; imp values
    // 1/22/99 - b adams  -  Read properties set
    // 5-12-99  B Adams   Added export Interstate_Request view to
    // 		   get the Other_State_Case_ID to the screen
    // ************************************************
    // PR# 237: 8/27/99 - b adams  -  Case assignment not found
    //   returned from CAB when a related, inactive case had no
    //   Case assignment.  No need to even process that situation.
    // =================================================
    // Oct 12, 1999, PR# 76385, M Brown: Changed the way export Obligation 
    // Amount is set.
    // It was not being set properly if the obligation is not active.
    // =================================================================================
    // 06/22/2006               GVandy              WR# 230751
    // Add capability to select tribal interstate request.
    // ===================================================================================
    // ***** HARDCODE AREA *****
    local.HardcodeOrderClassification.Classification = "J";

    // ***** MAIN-LINE AREA *****
    if (ReadCsePerson4())
    {
      export.ObligorCsePerson.Number = entities.ObligorCsePerson.Number;

      // =================================================
      // 11/10/98 - B Adams  -  Moved Read of obligation_type from
      //   being a separate action.
      // =================================================
      if (ReadObligationObligationType1())
      {
        export.Obligation.Assign(entities.Obligation);
        export.ObligationType.Assign(entities.ObligationType);

        // =================================================
        // 2/17/1999 - B Adams  -  removed outdated notes;  new rules.
        //   If alternate billing location has been added in LACT, then
        //   it must be protected and will apply to all debts for that
        //   Legal_Action.  Send "LE" flag so PrAD knows to protect
        //   the alternate address field.
        // =================================================
        if (ReadCsePerson1())
        {
          local.Alt.Number = entities.AltAddr.Number;
          export.Alternate.Char2 = "LE";
          local.TextWorkArea.Text10 = local.Alt.Number;
          UseEabPadLeftWithZeros();
          export.Alternate.Number = entities.AltAddr.Number;
        }
        else if (ReadCsePerson2())
        {
          local.Alt.Number = entities.AltAddr.Number;
          local.TextWorkArea.Text10 = local.Alt.Number;
          UseEabPadLeftWithZeros();
          export.Alternate.Number = entities.AltAddr.Number;
        }

        // ***--- Sumanta - MTW - 05/01/97
        // ***--- Added code to get the Interstate Case #
        // ***---
        // =================================================
        // 5/12/99 - bud adams  -  Interstate Request Case Number was
        //   not being exported.  Add MOVE statement
        // =================================================
        if (ReadCaseInterstateRequestInterstateRequestObligation())
        {
          export.Case1.Number = entities.Case1.Number;
          export.InterstateRequest.Assign(entities.InterstateRequest);

          if (!IsEmpty(export.InterstateRequest.Country))
          {
            local.Code.CodeName = "COUNTRY CODE";
            local.CodeValue.Cdvalue = export.InterstateRequest.Country ?? Spaces
              (10);
            UseCabValidateCodeValue();
          }

          if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            local.Code.CodeName = "TRIBAL IV-D AGENCIES";
            local.CodeValue.Cdvalue = export.InterstateRequest.TribalAgency ?? Spaces
              (10);
            UseCabValidateCodeValue();
          }
        }

        // ************************************************
        // *Set beginning, End, and Retired Dates of the  *
        // *Obligation by reading each Debt Detail.       *
        // ************************************************
        if (ReadDebtDetail2())
        {
          export.Header.CoveredPrdStartDt =
            entities.DebtDetail.CoveredPrdStartDt;
        }

        if (ReadDebtDetail3())
        {
          export.Header.CoveredPrdEndDt = entities.DebtDetail.CoveredPrdEndDt;
        }

        // ***---  This Read has the default property.  May be one, may be many.
        if (ReadDebtDetail1())
        {
          // ************************************************
          // *Still at least one active Debt Detail.  Leave *
          // *the end date blank                            *
          // ************************************************
        }
        else if (ReadDebtDetail5())
        {
          export.Header.RetiredDt = entities.DebtDetail.RetiredDt;
        }

        if (ReadDebtDetail4())
        {
          export.Header.DueDt = entities.DebtDetail.DueDt;
        }

        // ************************************************
        // *Get the Court Order Number                    *
        // ************************************************
        if (ReadLegalAction())
        {
          export.LegalAction.Assign(entities.LegalAction);

          // <<< Find the Legal Action Detail >>>
          if (import.LegalActionDetail.Number == 0)
          {
            // ================================================
            // 3/17/1999 - bud adams  -  This read did not uniquely qualify
            //   one row.  Happy St. Patrick's Day!
            // ================================================
            if (ReadLegalActionDetail())
            {
              export.LegalActionDetail.Number =
                entities.LegalActionDetail.Number;
            }
            else
            {
              ExitState = "LEGAL_ACTION_DETAIL_NF";

              return;
            }
          }
          else
          {
            export.LegalActionDetail.Number = import.LegalActionDetail.Number;
          }
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF";

          return;
        }

        UseFnCheckObligationForActivity();

        // ************************************************
        // *Check to see if there is an Obligation Payment*
        // *Schedule.
        // 
        // *
        // ************************************************
        if (ReadObligationPaymentSchedule())
        {
          export.PaymentScheduleInd.Flag = "Y";
          MoveObligationPaymentSchedule(entities.ObligationPaymentSchedule,
            export.ObligationPaymentSchedule);
        }
        else
        {
          export.PaymentScheduleInd.Flag = "N";
        }

        if (ReadInterestSuppStatusHistory())
        {
          export.InterestSuspendedInd.Flag = "Y";
        }
        else
        {
          export.InterestSuspendedInd.Flag = "N";
        }

        // ************************************************
        // *Check to see if there is a concurrent         *
        // *obligation. Primary-Secondary code is spaces  *
        // *if there is no concurrent obligation.         *
        // ************************************************
        if (AsChar(entities.Obligation.PrimarySecondaryCode) == AsChar
          (import.HcObJointSeveralConcu.PrimarySecondaryCode))
        {
          if (!ReadObligationObligationType3())
          {
            if (!ReadObligationObligationType2())
            {
              ExitState = "FN0000_CONCURRENT_OBLIGATION_NF";

              return;
            }
          }

          export.ConcurrentObligation.SystemGeneratedIdentifier =
            entities.ConcurrentObligation.SystemGeneratedIdentifier;
          export.ConcurrentObligationType.SystemGeneratedIdentifier =
            entities.ConcurrentObligationType.SystemGeneratedIdentifier;

          if (ReadCsePerson3())
          {
            export.ConcurrentCsePerson.Number =
              entities.ConcurrentCsePerson.Number;
          }
          else
          {
            ExitState = "FN0000_CONCURRENT_OBLIGOR_NF";

            return;
          }
        }

        // : Get the Supported Persons and Obligation Details.
        local.Temp.Count = 0;

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadObligationTransactionLegalActionPersonCsePerson())
          
        {
          if (entities.ObligationTransaction.Amount <= 0)
          {
            export.Export1.Next();

            continue;
          }

          ++local.Temp.Count;
          export.Export1.Update.ObligationTransaction.Assign(
            entities.ObligationTransaction);
          export.Export1.Update.Prev.Amount =
            entities.ObligationTransaction.Amount;
          export.ObligationAmt.TotalCurrency += entities.ObligationTransaction.
            Amount;

          // =================================================
          // 03/07/2000 - K.Price - Modified read to go straight
          // to the legal action person.  Going through the legal
          // action detail caused Obligation transactions to be listed
          // with no name.
          // =================================================
          export.Export1.Update.SupportedCsePerson.Number =
            entities.CsePerson.Number;

          // =================================================
          // 3/26/1999 - bud adams  -  The Reads of Case / Case_Role
          //   specifies and End_Date for Case_Role.  Here, we're looking
          //   for non-case related persons - meaning persons who were
          //   NEVER related to a Case; ever.  Not just today.
          // 7/15/99 - b adams  -  This action diagram is OK for this
          //   purpose, because we don't care what case the supported
          //   person is associated to.  ANY case is OK, and that's what
          //   this one will return.
          // =================================================
          local.SearchDiscontinue.Date = new DateTime(1900, 1, 1);
          UseFnReadCaseNoAndWorkerId();

          if (IsExitState("NO_CASE_RL_FOUND_FOR_SUPP_PERSON"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
            export.Export1.Update.ObligationTransaction.Amount =
              entities.LegalActionPerson.CurrentAmount.GetValueOrDefault();
            export.Export1.Update.SupportedCsePersonsWorkSet.Flag = "Z";

            if (Equal(entities.LegalActionPerson.EndDate, import.Max.Date))
            {
              export.Export1.Update.DebtDetail.RetiredDt =
                entities.LegalActionPerson.EndDate;
            }
            else
            {
              export.Export1.Update.DebtDetail.RetiredDt = local.Blank.Date;
            }
          }

          if (ReadDebtDetail6())
          {
            export.Export1.Update.DebtDetail.Assign(entities.DebtDetail);

            if (Equal(export.Export1.Item.DebtDetail.RetiredDt, import.Max.Date))
              
            {
              export.Export1.Update.DebtDetail.RetiredDt = local.Blank.Date;
            }
            else
            {
              export.Export1.Update.DebtDetail.RetiredDt =
                entities.DebtDetail.RetiredDt;
            }

            export.BalanceOwed.TotalCurrency += entities.DebtDetail.
              BalanceDueAmt;
            export.InterestOwed.TotalCurrency += entities.DebtDetail.
              InterestBalanceDueAmt.GetValueOrDefault();
          }
          else
          {
            ExitState = "FN0211_DEBT_DETAIL_NF";
            export.Export1.Next();

            return;
          }

          // ************************************************
          // *If the Obligation Primary-Secondary code is   *
          // *not spaces, there is a concurrent obligor.    *
          // *  For every Obligation Transaction on the     *
          // *Obligation, there will be a concurrent        *
          // *Obligation Transaction under the Concurrent   *
          // *Obligation.
          // 
          // *
          // ************************************************
          // =================================================
          // 3/17/1999 - Bud Adams  -  IF test was for <> SPACES.  But
          //   that was allowing Primary / Secondary obligations to slip
          //   through here and the CURRENT references were then
          //   blowing up.  (We do not care about seeing anything about
          //   P / S on ONAC.  Only Joint & Several.)
          //   FYI, the 'primary_secondary_code' is misleading because
          //   it is ALSO used for joint-and-several obligations.  It's "P" or
          //   "S" for those obligations that are related as primary /
          //   secondary; or both are "J" for Joint and Several.
          // =================================================
          if (AsChar(entities.Obligation.PrimarySecondaryCode) == AsChar
            (import.HcObJointSeveralConcu.PrimarySecondaryCode))
          {
            // <<< RBM  02-09-1998  For Joint Obligations, both the related 
            // Obligations will have the Primary-Secondary_Code = 'J'. >>>
            if (ReadObligationTransaction2())
            {
              export.Export1.Update.Concurrent.SystemGeneratedIdentifier =
                entities.ConcurrentObligationTransaction.
                  SystemGeneratedIdentifier;
            }
            else if (ReadObligationTransaction1())
            {
              export.Export1.Update.Concurrent.SystemGeneratedIdentifier =
                entities.ConcurrentObligationTransaction.
                  SystemGeneratedIdentifier;
            }
            else
            {
              ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_NF";
              export.Export1.Next();

              return;
            }
          }

          // <<< RBM   10/10/97 >>>
          // <<< Get the Designated Payee for the Supported Person >>>
          // ================================================
          // Removed the designated-payee retrieval, since non-accruing
          // debts will have none.
          // ================================================
          // ****----  end of READ EACH Obligation_Transaction
          export.Export1.Next();
        }

        if (local.Temp.Count == 0)
        {
          ExitState = "FN0000_OBLIG_TRANS_NF";

          return;
        }

        if (ReadManualDistributionAudit())
        {
          export.ManualDistributionInd.Flag = "Y";
        }
        else
        {
          export.ManualDistributionInd.Flag = "N";
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_NF";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGOR_NF";

      return;
    }

    export.TotalOwed.TotalCurrency = export.BalanceOwed.TotalCurrency + export
      .InterestOwed.TotalCurrency;
  }

  private static void MoveObligationPaymentSchedule(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.Amount = source.Amount;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    export.Country.Cdvalue = useExport.CodeValue.Cdvalue;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCheckObligationForActivity()
  {
    var useImport = new FnCheckObligationForActivity.Import();
    var useExport = new FnCheckObligationForActivity.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.HcCpaObligor.Type1 = import.HcCpaObligor.Type1;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      import.HcOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.Obligor.Number = import.Obligor.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;

    Call(FnCheckObligationForActivity.Execute, useImport, useExport);

    export.ObligationActive.Flag = useExport.ActiveObligation.Flag;
  }

  private void UseFnReadCaseNoAndWorkerId()
  {
    var useImport = new FnReadCaseNoAndWorkerId.Import();
    var useExport = new FnReadCaseNoAndWorkerId.Export();

    useImport.Supported.Number = entities.CsePerson.Number;
    useImport.Obligor.Number = import.Obligor.Number;
    useImport.SearchDiscontinue.Date = local.SearchDiscontinue.Date;

    Call(FnReadCaseNoAndWorkerId.Execute, useImport, useExport);
  }

  private bool ReadCaseInterstateRequestInterstateRequestObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Case1.Populated = false;
    entities.InterstateRequest.Populated = false;
    entities.InterstateRequestObligation.Populated = false;

    return Read("ReadCaseInterstateRequestInterstateRequestObligation",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableDate(
          command, "orderEffDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 0);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 1);
        entities.InterstateRequestObligation.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 2);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 3);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 4);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 5);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 6);
        entities.InterstateRequestObligation.OtyType = db.GetInt32(reader, 7);
        entities.InterstateRequestObligation.CpaType = db.GetString(reader, 8);
        entities.InterstateRequestObligation.CspNumber =
          db.GetString(reader, 9);
        entities.InterstateRequestObligation.ObgGeneratedId =
          db.GetInt32(reader, 10);
        entities.InterstateRequestObligation.OrderEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequestObligation.OrderEndDate =
          db.GetNullableDate(reader, 12);
        entities.Case1.Populated = true;
        entities.InterstateRequest.Populated = true;
        entities.InterstateRequestObligation.Populated = true;
        CheckValid<InterstateRequestObligation>("CpaType",
          entities.InterstateRequestObligation.CpaType);
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AltAddr.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AltAddr.Number = db.GetString(reader, 0);
        entities.AltAddr.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AltAddr.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Obligation.CspPNumber ?? "");
      },
      (db, reader) =>
      {
        entities.AltAddr.Number = db.GetString(reader, 0);
        entities.AltAddr.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);
    entities.ConcurrentCsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ConcurrentObligation.CspNumber);
        db.SetString(command, "cpaType", entities.ConcurrentObligation.CpaType);
        db.SetString(command, "type", import.HcCpaObligor.Type1);
      },
      (db, reader) =>
      {
        entities.ConcurrentCsePerson.Number = db.GetString(reader, 0);
        entities.ConcurrentCsePerson.Type1 = db.GetString(reader, 1);
        entities.ConcurrentCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ConcurrentCsePerson.Type1);
      });
  }

  private bool ReadCsePerson4()
  {
    entities.ObligorCsePerson.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ObligorCsePerson.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ObligorCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ObligorCsePerson.Type1);
      });
  }

  private bool ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableDate(
          command, "retiredDt", local.Blank.Date.GetValueOrDefault());
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail3",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail4()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail4",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail5()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail5",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail6()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail6",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadInterestSuppStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.InterestSuppStatusHistory.Populated = false;

    return Read("ReadInterestSuppStatusHistory",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterestSuppStatusHistory.ObgId = db.GetInt32(reader, 0);
        entities.InterestSuppStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.InterestSuppStatusHistory.CpaType = db.GetString(reader, 2);
        entities.InterestSuppStatusHistory.OtyId = db.GetInt32(reader, 3);
        entities.InterestSuppStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.InterestSuppStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.InterestSuppStatusHistory.DiscontinueDate =
          db.GetDate(reader, 6);
        entities.InterestSuppStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.InterestSuppStatusHistory.CreatedBy = db.GetString(reader, 8);
        entities.InterestSuppStatusHistory.CreatedTmst =
          db.GetDateTime(reader, 9);
        entities.InterestSuppStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.InterestSuppStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.InterestSuppStatusHistory.Populated = true;
        CheckValid<InterestSuppStatusHistory>("CpaType",
          entities.InterestSuppStatusHistory.CpaType);
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "laDetailNo",
          entities.Obligation.LadNumber.GetValueOrDefault());
        db.SetInt32(
          command, "lgaIdentifier",
          entities.Obligation.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 2);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadManualDistributionAudit()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ManualDistributionAudit.Populated = false;

    return Read("ReadManualDistributionAudit",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ManualDistributionAudit.OtyType = db.GetInt32(reader, 0);
        entities.ManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ManualDistributionAudit.CspNumber = db.GetString(reader, 2);
        entities.ManualDistributionAudit.CpaType = db.GetString(reader, 3);
        entities.ManualDistributionAudit.EffectiveDt = db.GetDate(reader, 4);
        entities.ManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ManualDistributionAudit.CpaType);
      });
  }

  private bool ReadObligationObligationType1()
  {
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationObligationType1",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 4);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 5);
        entities.Obligation.Description = db.GetNullableString(reader, 6);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 7);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 8);
        entities.Obligation.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 9);
        entities.Obligation.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 10);
        entities.Obligation.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 11);
        entities.Obligation.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 12);
        entities.Obligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 13);
        entities.Obligation.AsOdDtRecIntBal = db.GetNullableDecimal(reader, 14);
        entities.Obligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 15);
        entities.Obligation.AsOfDtFeeIntBal = db.GetNullableDecimal(reader, 16);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 17);
        entities.Obligation.CreatedBy = db.GetString(reader, 18);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 21);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 22);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 23);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 24);
        entities.ObligationType.Code = db.GetString(reader, 25);
        entities.ObligationType.Classification = db.GetString(reader, 26);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadObligationObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ConcurrentObligationType.Populated = false;
    entities.ConcurrentObligation.Populated = false;

    return Read("ReadObligationObligationType2",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "orrGeneratedId",
          import.HcOrrJointSeveral.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ConcurrentObligation.CpaType = db.GetString(reader, 0);
        entities.ConcurrentObligation.CspNumber = db.GetString(reader, 1);
        entities.ConcurrentObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ConcurrentObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ConcurrentObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ConcurrentObligationType.Populated = true;
        entities.ConcurrentObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ConcurrentObligation.CpaType);
          
      });
  }

  private bool ReadObligationObligationType3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ConcurrentObligationType.Populated = false;
    entities.ConcurrentObligation.Populated = false;

    return Read("ReadObligationObligationType3",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "orrGeneratedId",
          import.HcOrrJointSeveral.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ConcurrentObligation.CpaType = db.GetString(reader, 0);
        entities.ConcurrentObligation.CspNumber = db.GetString(reader, 1);
        entities.ConcurrentObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ConcurrentObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ConcurrentObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ConcurrentObligationType.Populated = true;
        entities.ConcurrentObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ConcurrentObligation.CpaType);
          
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetDate(command, "startDt", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
      });
  }

  private bool ReadObligationTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.ConcurrentObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyTypeSecondary", entities.ObligationTransaction.OtyType);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType1", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber1", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId1",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetInt32(
          command, "onrGeneratedId",
          import.HcOtrrConcurrentObliga.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", entities.ConcurrentObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId2",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber2", entities.ConcurrentObligation.CspNumber);
        db.
          SetString(command, "cpaType2", entities.ConcurrentObligation.CpaType);
          
      },
      (db, reader) =>
      {
        entities.ConcurrentObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 0);
        entities.ConcurrentObligationTransaction.CspNumber =
          db.GetString(reader, 1);
        entities.ConcurrentObligationTransaction.CpaType =
          db.GetString(reader, 2);
        entities.ConcurrentObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ConcurrentObligationTransaction.Type1 =
          db.GetString(reader, 4);
        entities.ConcurrentObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ConcurrentObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ConcurrentObligationTransaction.OtyType =
          db.GetInt32(reader, 7);
        entities.ConcurrentObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ConcurrentObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ConcurrentObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ConcurrentObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligationTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.ConcurrentObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyTypePrimary", entities.ObligationTransaction.OtyType);
        db.SetString(command, "otrPType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrPGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaPType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspPNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgPGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetInt32(
          command, "onrGeneratedId",
          import.HcOtrrConcurrentObliga.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", entities.ConcurrentObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ConcurrentObligation.CspNumber);
        db.SetString(command, "cpaType", entities.ConcurrentObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ConcurrentObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 0);
        entities.ConcurrentObligationTransaction.CspNumber =
          db.GetString(reader, 1);
        entities.ConcurrentObligationTransaction.CpaType =
          db.GetString(reader, 2);
        entities.ConcurrentObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ConcurrentObligationTransaction.Type1 =
          db.GetString(reader, 4);
        entities.ConcurrentObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ConcurrentObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ConcurrentObligationTransaction.OtyType =
          db.GetInt32(reader, 7);
        entities.ConcurrentObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ConcurrentObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ConcurrentObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ConcurrentObligationTransaction.CpaSupType);
      });
  }

  private IEnumerable<bool>
    ReadObligationTransactionLegalActionPersonCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    return ReadEach("ReadObligationTransactionLegalActionPersonCsePerson",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 6);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 7);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 9);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 10);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 11);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 11);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 12);
        entities.CsePerson.Number = db.GetString(reader, 12);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 13);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 14);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 15);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 16);
        entities.LegalActionPerson.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.ObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of HcOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hcOtrnTDebt")]
    public ObligationTransaction HcOtrnTDebt
    {
      get => hcOtrnTDebt ??= new();
      set => hcOtrnTDebt = value;
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
    /// A value of HcDdshActiveStatus.
    /// </summary>
    [JsonPropertyName("hcDdshActiveStatus")]
    public DebtDetailStatusHistory HcDdshActiveStatus
    {
      get => hcDdshActiveStatus ??= new();
      set => hcDdshActiveStatus = value;
    }

    /// <summary>
    /// A value of HcCpaSupportedPerson.
    /// </summary>
    [JsonPropertyName("hcCpaSupportedPerson")]
    public CsePersonAccount HcCpaSupportedPerson
    {
      get => hcCpaSupportedPerson ??= new();
      set => hcCpaSupportedPerson = value;
    }

    /// <summary>
    /// A value of HcOrrJointSeveral.
    /// </summary>
    [JsonPropertyName("hcOrrJointSeveral")]
    public ObligationRlnRsn HcOrrJointSeveral
    {
      get => hcOrrJointSeveral ??= new();
      set => hcOrrJointSeveral = value;
    }

    /// <summary>
    /// A value of HcOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hcOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HcOtrrConcurrentObliga
    {
      get => hcOtrrConcurrentObliga ??= new();
      set => hcOtrrConcurrentObliga = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of HcObJointSeveralConcu.
    /// </summary>
    [JsonPropertyName("hcObJointSeveralConcu")]
    public Obligation HcObJointSeveralConcu
    {
      get => hcObJointSeveralConcu ??= new();
      set => hcObJointSeveralConcu = value;
    }

    private DateWorkArea max;
    private DateWorkArea current;
    private ObligationTransaction hcOtrnTDebt;
    private CsePersonAccount hcCpaObligor;
    private DebtDetailStatusHistory hcDdshActiveStatus;
    private CsePersonAccount hcCpaSupportedPerson;
    private ObligationRlnRsn hcOrrJointSeveral;
    private ObligationTransactionRlnRsn hcOtrrConcurrentObliga;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private CsePerson obligor;
    private Obligation obligation;
    private Obligation hcObJointSeveralConcu;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of ZdelExportGrpDetail.
      /// </summary>
      [JsonPropertyName("zdelExportGrpDetail")]
      public Program ZdelExportGrpDetail
      {
        get => zdelExportGrpDetail ??= new();
        set => zdelExportGrpDetail = value;
      }

      /// <summary>
      /// A value of SupportedCsePerson.
      /// </summary>
      [JsonPropertyName("supportedCsePerson")]
      public CsePerson SupportedCsePerson
      {
        get => supportedCsePerson ??= new();
        set => supportedCsePerson = value;
      }

      /// <summary>
      /// A value of SupportedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedCsePersonsWorkSet
      {
        get => supportedCsePersonsWorkSet ??= new();
        set => supportedCsePersonsWorkSet = value;
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
      /// A value of ObligationTransaction.
      /// </summary>
      [JsonPropertyName("obligationTransaction")]
      public ObligationTransaction ObligationTransaction
      {
        get => obligationTransaction ??= new();
        set => obligationTransaction = value;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of ObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("obligationPaymentSchedule")]
      public ObligationPaymentSchedule ObligationPaymentSchedule
      {
        get => obligationPaymentSchedule ??= new();
        set => obligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of Concurrent.
      /// </summary>
      [JsonPropertyName("concurrent")]
      public ObligationTransaction Concurrent
      {
        get => concurrent ??= new();
        set => concurrent = value;
      }

      /// <summary>
      /// A value of Prev.
      /// </summary>
      [JsonPropertyName("prev")]
      public ObligationTransaction Prev
      {
        get => prev ??= new();
        set => prev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program zdelExportGrpDetail;
      private CsePerson supportedCsePerson;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private ObligationTransaction obligationTransaction;
      private DebtDetail debtDetail;
      private ServiceProvider serviceProvider;
      private ObligationPaymentSchedule obligationPaymentSchedule;
      private ObligationTransaction concurrent;
      private ObligationTransaction prev;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
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
    /// A value of ConcurrentObligationType.
    /// </summary>
    [JsonPropertyName("concurrentObligationType")]
    public ObligationType ConcurrentObligationType
    {
      get => concurrentObligationType ??= new();
      set => concurrentObligationType = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
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
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public DebtDetail Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of ConcurrentCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("concurrentCsePersonsWorkSet")]
    public CsePersonsWorkSet ConcurrentCsePersonsWorkSet
    {
      get => concurrentCsePersonsWorkSet ??= new();
      set => concurrentCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
    }

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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of PaymentScheduleInd.
    /// </summary>
    [JsonPropertyName("paymentScheduleInd")]
    public Common PaymentScheduleInd
    {
      get => paymentScheduleInd ??= new();
      set => paymentScheduleInd = value;
    }

    /// <summary>
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of ObligationActive.
    /// </summary>
    [JsonPropertyName("obligationActive")]
    public Common ObligationActive
    {
      get => obligationActive ??= new();
      set => obligationActive = value;
    }

    /// <summary>
    /// A value of ObligationAmt.
    /// </summary>
    [JsonPropertyName("obligationAmt")]
    public Common ObligationAmt
    {
      get => obligationAmt ??= new();
      set => obligationAmt = value;
    }

    /// <summary>
    /// A value of BalanceOwed.
    /// </summary>
    [JsonPropertyName("balanceOwed")]
    public Common BalanceOwed
    {
      get => balanceOwed ??= new();
      set => balanceOwed = value;
    }

    /// <summary>
    /// A value of InterestOwed.
    /// </summary>
    [JsonPropertyName("interestOwed")]
    public Common InterestOwed
    {
      get => interestOwed ??= new();
      set => interestOwed = value;
    }

    /// <summary>
    /// A value of TotalOwed.
    /// </summary>
    [JsonPropertyName("totalOwed")]
    public Common TotalOwed
    {
      get => totalOwed ??= new();
      set => totalOwed = value;
    }

    /// <summary>
    /// A value of InterestSuspendedInd.
    /// </summary>
    [JsonPropertyName("interestSuspendedInd")]
    public Common InterestSuspendedInd
    {
      get => interestSuspendedInd ??= new();
      set => interestSuspendedInd = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private CodeValue country;
    private InterstateRequest interstateRequest;
    private ObligationType concurrentObligationType;
    private LegalActionDetail legalActionDetail;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CsePerson obligorCsePerson;
    private Obligation obligation;
    private Obligation concurrentObligation;
    private DebtDetail header;
    private CsePersonsWorkSet concurrentCsePersonsWorkSet;
    private CsePerson concurrentCsePerson;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private Common paymentScheduleInd;
    private Common manualDistributionInd;
    private Common obligationActive;
    private Common obligationAmt;
    private Common balanceOwed;
    private Common interestOwed;
    private Common totalOwed;
    private Common interestSuspendedInd;
    private CsePersonsWorkSet alternate;
    private Case1 case1;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of SearchDiscontinue.
    /// </summary>
    [JsonPropertyName("searchDiscontinue")]
    public DateWorkArea SearchDiscontinue
    {
      get => searchDiscontinue ??= new();
      set => searchDiscontinue = value;
    }

    /// <summary>
    /// A value of ForDebtScreens.
    /// </summary>
    [JsonPropertyName("forDebtScreens")]
    public Common ForDebtScreens
    {
      get => forDebtScreens ??= new();
      set => forDebtScreens = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of HardcodeOrderClassification.
    /// </summary>
    [JsonPropertyName("hardcodeOrderClassification")]
    public LegalAction HardcodeOrderClassification
    {
      get => hardcodeOrderClassification ??= new();
      set => hardcodeOrderClassification = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public AbendData Eab
    {
      get => eab ??= new();
      set => eab = value;
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
    /// A value of Alt.
    /// </summary>
    [JsonPropertyName("alt")]
    public CsePerson Alt
    {
      get => alt ??= new();
      set => alt = value;
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
    /// A value of Discontinue.
    /// </summary>
    [JsonPropertyName("discontinue")]
    public DateWorkArea Discontinue
    {
      get => discontinue ??= new();
      set => discontinue = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of HardcodeSpouse.
    /// </summary>
    [JsonPropertyName("hardcodeSpouse")]
    public CaseRole HardcodeSpouse
    {
      get => hardcodeSpouse ??= new();
      set => hardcodeSpouse = value;
    }

    /// <summary>
    /// A value of HardcodeChild.
    /// </summary>
    [JsonPropertyName("hardcodeChild")]
    public CaseRole HardcodeChild
    {
      get => hardcodeChild ??= new();
      set => hardcodeChild = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      validCode = null;
      code = null;
      codeValue = null;
      searchDiscontinue = null;
      forDebtScreens = null;
      temp = null;
      hardcodeOrderClassification = null;
      blank = null;
      csePersonsWorkSet = null;
      eab = null;
      textWorkArea = null;
      alt = null;
      current = null;
      discontinue = null;
      hardcodeSpouse = null;
      hardcodeChild = null;
    }

    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private DateWorkArea searchDiscontinue;
    private Common forDebtScreens;
    private Common temp;
    private LegalAction hardcodeOrderClassification;
    private DateWorkArea blank;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData eab;
    private TextWorkArea textWorkArea;
    private CsePerson alt;
    private DateWorkArea current;
    private DateWorkArea discontinue;
    private CsePerson obligor;
    private CaseRole hardcodeSpouse;
    private CaseRole hardcodeChild;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ConcurrentObligationType.
    /// </summary>
    [JsonPropertyName("concurrentObligationType")]
    public ObligationType ConcurrentObligationType
    {
      get => concurrentObligationType ??= new();
      set => concurrentObligationType = value;
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
    /// A value of AltAddr.
    /// </summary>
    [JsonPropertyName("altAddr")]
    public CsePerson AltAddr
    {
      get => altAddr ??= new();
      set => altAddr = value;
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
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
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
    /// A value of ConcurrentObligationTransaction.
    /// </summary>
    [JsonPropertyName("concurrentObligationTransaction")]
    public ObligationTransaction ConcurrentObligationTransaction
    {
      get => concurrentObligationTransaction ??= new();
      set => concurrentObligationTransaction = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
    }

    /// <summary>
    /// A value of ObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationRlnRsn")]
    public ObligationRlnRsn ObligationRlnRsn
    {
      get => obligationRlnRsn ??= new();
      set => obligationRlnRsn = value;
    }

    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of ManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("manualDistributionAudit")]
    public ManualDistributionAudit ManualDistributionAudit
    {
      get => manualDistributionAudit ??= new();
      set => manualDistributionAudit = value;
    }

    /// <summary>
    /// A value of InterestSuppStatusHistory.
    /// </summary>
    [JsonPropertyName("interestSuppStatusHistory")]
    public InterestSuppStatusHistory InterestSuppStatusHistory
    {
      get => interestSuppStatusHistory ??= new();
      set => interestSuppStatusHistory = value;
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
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
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

    private LegalActionPerson legalActionPerson;
    private ObligationType concurrentObligationType;
    private LegalAction legalAction;
    private CsePerson altAddr;
    private Obligation obligation;
    private Obligation concurrentObligation;
    private ObligationTransaction obligationTransaction;
    private ObligationTransaction concurrentObligationTransaction;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private CsePerson concurrentCsePerson;
    private ObligationRlnRsn obligationRlnRsn;
    private ObligationRln obligationRln;
    private CsePerson obligorCsePerson;
    private CsePersonAccount obligorCsePersonAccount;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private DebtDetail debtDetail;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ManualDistributionAudit manualDistributionAudit;
    private InterestSuppStatusHistory interestSuppStatusHistory;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private InterstateRequestObligation interstateRequestObligation;
    private CsePerson csePerson;
  }
#endregion
}
