// Program: SI_CADS_DETERMINE_CASE_CLOSURE, ID: 373300586, model: 746.
// Short name: SWE00373
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CADS_DETERMINE_CASE_CLOSURE.
/// </summary>
[Serializable]
public partial class SiCadsDetermineCaseClosure: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CADS_DETERMINE_CASE_CLOSURE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCadsDetermineCaseClosure(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCadsDetermineCaseClosure.
  /// </summary>
  public SiCadsDetermineCaseClosure(IContext context, Import import,
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
    //                           M A I N T E N A N C E    L O G
    // ---------------------------------------------------------------------------------------
    //   Date   Developer  WR#/PR#   Description
    // ---------------------------------------------------------------------------------------
    // 03/26/01 C Fairley  000240    Initial Development
    //                               
    // Business Rules:
    //                               
    // 1) A case can be CLOSED, if
    // the only debt(s) still ACTIVE
    //                                  
    // or INACTIVE is(are) Recovery type
    // debt(s).
    //                               
    // 2) Do not allow the case to be
    // closed, if the Obligor is
    //                                  
    // active on one case only and there
    // is a Fee type debt.
    //                               
    // 3) If an Obligor and Supported
    // person combination are
    //                                  
    // known to more than one case and
    // there is(are) ACTIVE
    //                                  
    // or INACTIVE debt(s), allow the
    // case to be closed as
    //                                  
    // long as one case remains open.
    //                               
    // 4) Do not allow the case to be
    // closed, when an Obligor
    //                                  
    // and Supported person combination
    // are only known to
    //                                  
    // one case and all debts are not
    // DEACTIVATED.
    //                               
    // 5) A case can be closed, if NO
    // Cash Receipt Details exist
    //                                  
    // with a status of "SUSP" or "PEND"
    // for the Obligor.
    //                               
    // Definitions:
    //                                  
    // INACTIVE    - Balance owed NOT
    // accruing
    //                                  
    // ACTIVE      - Balance owed still
    // accruing
    //                                  
    // DEACTIVATED - No Balance owed
    // 06/25/01 C Fairley  I00122456 CADS abending on an UPDATE (PF6) with a -
    // 811
    //                               
    // (embedded SELECT returns more
    // than one row).
    // 02/11/02 M. Lachowicz  PR137133 Allow to close case if debt supports 
    // person from
    // 				different case.
    // 03/22/02 M. Lachowicz  PR142183 Allow to close case if supporting person 
    // and debtor
    // 				are on the other case (active or inactive).
    // 05/01/02 M. Lachowicz  PR144846 Process inactive obligation in the same 
    // way as an
    // 				active one.
    // 05/22/02 M. Lachowicz  PR146724 Do not check if CH role is active or 
    // inactive.
    // 07/02/02 M. Lachowicz  PR148261 Fixed problem with money in suspense.
    // 		       PR148363
    // 09/24/02 M. Lachowicz  PR158385 Allow to close a case when AP/AR 
    // combination exist as
    // 				a AP/CH combination..
    // 05/31/11 GVandy		CQ27884 Do not allow case closure if a suspended or 
    // pended
    // 				non-court ordered cash receipt detail exists and no
    // 				other open case exists for the AP.
    // 03/07/19 GVandy 	CQ65422	Do not allow case to close if any AR (active or
    // 				inactive) has a warrant in REQ status.
    // 07/02/19  GVandy	CQ65423	Do not allow case to close if any AR (active or
    // 				inactive) has a suppressed disbursement.
    // 08/20/20  GVandy	CQ66660	Modifying edit added for CQ65423 to now be...
    // 				Do not allow case to close if any AR (active or
    // 				inactive) has a suppressed disbursement that originated
    // 				from a payment from an AP on this case.
    // ---------------------------------------------------------------------------------------
    UseFnHardcodedDebtDistribution();

    // ***
    // *** Set initial values
    // ***
    local.ApCsePerson.Number = import.Ap.Number;
    local.Obligor.ObligorPersonNumber = import.Ap.Number;
    local.Max.Date = new DateTime(2099, 12, 31);
    local.Current.Date = Now().Date;
    local.Fees.Flag = "N";
    local.Others.Flag = "N";
    local.Recovery.Flag = "N";
    local.Active.Flag = "N";
    local.Deactivated.Flag = "N";
    local.Inactive.Flag = "N";
    local.Open.Status = "O";
    local.ApRoles.Count = 0;
    local.ApCaseRole.Type1 = "AP";
    local.Suspended.SystemGeneratedIdentifier = 3;
    local.Pended.SystemGeneratedIdentifier = 7;
    local.AobligationTransaction.DebtType = "A";
    local.D.DebtType = "D";
    local.AobligationType.Classification = "A";

    // *** Problem report I00122476
    // *** 06/25/01 swsrchf
    // *** start
    local.Ar.Type1 = "AR";
    local.Ch.Type1 = "CH";

    // *** end
    // *** 06/25/01 swsrchf
    // *** Problem report I00122476
    // ***
    // *** Determine the debt status. Are there CRD's (Cash Receipt Detail) with
    // a status of
    // *** "SUSP" or "PEND"????
    // ***
    foreach(var item in ReadCashReceiptDetail())
    {
      local.LegalAction.StandardNumber =
        entities.ExistingCashReceiptDetail.CourtOrderNumber;

      if (ReadCashReceiptDetailStatHistory())
      {
        if (IsEmpty(entities.ExistingCashReceiptDetail.CourtOrderNumber))
        {
          // --  05/31/11 GVandy  CQ 27884  Do not allow case closure if a 
          // suspended or pended non-court
          // --  ordered cash receipt detail exists and no other open case 
          // exists for the AP.
          if (ReadCaseRole1())
          {
            // -- There is at least one other open case for the AP.  Do not 
            // restrict case closure.
            continue;
          }
          else
          {
            // -- There are no other open cases for the AP.  Restrict case 
            // closure.  (Exit state is set below)
          }
        }
        else
        {
          // 07/02/02 M.Lachowicz Start
          if (Equal(local.LegalAction.StandardNumber, local.Prev.StandardNumber))
            
          {
            continue;
          }

          local.Prev.StandardNumber = local.LegalAction.StandardNumber ?? "";

          if (ReadLegalAction2())
          {
            // Check if exists another open case with the same court order
            // and the same AP.
            if (ReadLegalAction1())
            {
              continue;
            }
          }
          else
          {
            continue;
          }

          // 07/02/02 M.Lachowicz End
        }

        // ***
        // *** AP has money in SUSPENSE, therefore case CANNOT be CLOSED
        // ***
        ExitState = "SI0000_MONEY_IN_SUSP";

        return;
      }
      else
      {
        // ***
        // *** Continue processing.........not SUSPENDED or PENDING
        // ***
      }
    }

    // ***
    // *** AP has NO money in SUSPENSE, continue processing
    // ***
    // --03/07/19 GVandy CQ65422 Do not allow case to close if any AR (active or
    // inactive) has a warrant in REQ status.
    // --Note: The read each below is set to return only distinct occurrences.
    foreach(var item in ReadCsePerson())
    {
      if (ReadPaymentRequest())
      {
        // --AR has a warrant in REQ status that resulted from a payment from an
        // AP on this case, therefore case CANNOT be CLOSED
        ExitState = "SI0000_WARRANT_IN_REQ_STATUS";

        return;
      }
      else
      {
        // --Continue
      }

      if (ReadDisbursementStatusHistory())
      {
        // --AR has a suppressed disbursement that resulted from a payment from 
        // an AP on this case, therefore case CANNOT be CLOSED
        ExitState = "SI0000_SUPPR_DISB_PREVENTS_CLOSE";

        return;
      }
      else
      {
        // --Continue
      }
    }

    // 02/11/02 M. Lachowicz Start
    local.ActiveObligation.Index = -1;

    // 02/11/02 M. Lachowicz End
    foreach(var item in ReadObligation2())
    {
      local.Obligation.SystemGeneratedIdentifier =
        entities.ExistingObligation.SystemGeneratedIdentifier;

      if (ReadObligationType())
      {
        // ***
        // *** Type of Obligation
        // ***
        switch(AsChar(entities.ExistingObligationType.Classification))
        {
          case 'F':
            // ***
            // *** FEES obligation
            // ***
            local.Fees.Flag = "Y";

            break;
          case 'R':
            // ***
            // *** RECOVERY obligation
            // ***
            local.Recovery.Flag = "Y";

            break;
          case 'V':
            // ***
            // *** VOLUNTARY obligation.......ignore
            // ***
            continue;
          default:
            // ***
            // *** All other Obligation types except FEES, RECOVERY and 
            // VOLUNTARY
            // ***
            local.Others.Flag = "Y";

            break;
        }

        MoveObligationType(entities.ExistingObligationType, local.ObligationType);
          
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_TYPE_NF";

        return;
      }

      // ***
      // *** Obtain the status of the Obligation:
      // ***    ACTIVE, INACTIVE or DEACTIVATED
      // ***
      UseFnGetObligationStatus();

      switch(AsChar(local.Derived.ObligationStatus))
      {
        case 'A':
          // ***
          // *** Active status
          // ***
          local.Active.Flag = "Y";

          // 02/11/02 M. Lachowicz Start
          if (local.ActiveObligation.Index + 1 >= Local
            .ActiveObligationGroup.Capacity)
          {
            break;
          }

          ++local.ActiveObligation.Index;
          local.ActiveObligation.CheckSize();

          local.ActiveObligation.Update.ActiveObligation1.
            SystemGeneratedIdentifier =
              entities.ExistingObligation.SystemGeneratedIdentifier;
          local.ActiveObligation.Update.ActiveObligationType.
            SystemGeneratedIdentifier =
              entities.ExistingObligationType.SystemGeneratedIdentifier;

          // 02/11/02 M. Lachowicz End
          break;
        case 'D':
          // ***
          // *** Deactivated status
          // ***
          local.Deactivated.Flag = "Y";

          break;
        case 'I':
          // ***
          // *** Inactive status
          // ***
          local.Inactive.Flag = "Y";

          // 05/01/02 M. Lachowicz Start
          if (local.ActiveObligation.Index + 1 >= Local
            .ActiveObligationGroup.Capacity)
          {
            break;
          }

          ++local.ActiveObligation.Index;
          local.ActiveObligation.CheckSize();

          local.ActiveObligation.Update.ActiveObligation1.
            SystemGeneratedIdentifier =
              entities.ExistingObligation.SystemGeneratedIdentifier;
          local.ActiveObligation.Update.ActiveObligationType.
            SystemGeneratedIdentifier =
              entities.ExistingObligationType.SystemGeneratedIdentifier;

          // 05/01/02 M. Lachowicz End
          break;
        default:
          break;
      }
    }

    if (AsChar(local.Deactivated.Flag) == 'N' && AsChar(local.Active.Flag) == 'N'
      && AsChar(local.Inactive.Flag) == 'N' || AsChar
      (local.Deactivated.Flag) == 'Y' && AsChar(local.Active.Flag) == 'N' && AsChar
      (local.Inactive.Flag) == 'N' || AsChar(local.Recovery.Flag) == 'Y' && AsChar
      (local.Fees.Flag) == 'N' && AsChar(local.Others.Flag) == 'N')
    {
      // ***
      // *** No debt(s) exist
      // ***         OR
      // *** All debt(s) have been DEACTIVATED
      // ***         OR
      // *** RECOVERY type debt(s) only
      // ***
      // *** Therefore case CAN be CLOSED
      return;
    }

    if (AsChar(local.Fees.Flag) == 'Y' && AsChar(local.Others.Flag) == 'N' && AsChar
      (local.Recovery.Flag) == 'N')
    {
      // ***
      // *** FEES type debt only
      // ***
      ReadCaseRole3();

      if (local.ApRoles.Count == 0)
      {
        // ***
        // *** AP has active FEE type debts, therefore case CANNOT be CLOSED
        // ***
        ExitState = "SI0000_CASE_HAS_ACTIVE_DEBTS";

        return;
      }

      // ***
      // *** AP has NO active FEE type debts, therefore case CAN be CLOSED
      // ***
      return;
    }

    // ***
    // *** Get the CSE Person number for the Supported person
    // ***
    for(local.Supported.Index = 0; local.Supported.Index < Local
      .SupportedGroup.Capacity; ++local.Supported.Index)
    {
      if (!local.Supported.CheckSize())
      {
        break;
      }

      local.Supported.Update.SupportedCsePerson.Number = "";
    }

    local.Supported.CheckIndex();

    // 02/11/02 M. Lachowicz Start
    local.Supported.Index = -1;

    for(local.ActiveObligation.Index = 0; local.ActiveObligation.Index < local
      .ActiveObligation.Count; ++local.ActiveObligation.Index)
    {
      if (!local.ActiveObligation.CheckSize())
      {
        break;
      }

      if (!ReadObligation1())
      {
        ExitState = "FN0000_OBLIGATION_NF";

        return;
      }

      local.PrevSupported.Number = "0000000000";
      local.Count.Count = 0;

      do
      {
        foreach(var item in ReadObligationTransactionCsePerson())
        {
          local.PrevSupported.Number = entities.SupportedCsePerson.Number;

          // 05/22/2002 M. Lachowicz Removed check for active case roles.
          if (ReadCaseRole2())
          {
            // 05/22/2002 M. Lachowicz Start
            if (Equal(entities.SupportedCaseRole.Type1, "AR") && !
              Lt(local.Current.Date, entities.SupportedCaseRole.EndDate))
            {
              continue;
            }

            // 05/22/2002 M. Lachowicz End
            if (local.Supported.Index + 1 >= Local.SupportedGroup.Capacity)
            {
              goto AfterCycle;
            }

            ++local.Supported.Index;
            local.Supported.CheckSize();

            local.Supported.Update.SupportedCaseRole.Type1 =
              entities.SupportedCaseRole.Type1;
            local.Supported.Update.SupportedCsePerson.Number =
              entities.SupportedCsePerson.Number;
          }

          goto Next1;
        }

        local.Count.Count = 1;

        goto Next2;

Next1:
        ;
      }
      while(local.Count.Count != 0);

Next2:
      ;
    }

AfterCycle:

    local.ActiveObligation.CheckIndex();

    for(local.Supported.Index = 0; local.Supported.Index < Local
      .SupportedGroup.Capacity; ++local.Supported.Index)
    {
      if (!local.Supported.CheckSize())
      {
        break;
      }

      switch(TrimEnd(local.Supported.Item.SupportedCaseRole.Type1))
      {
        case "AR":
          local.CaseUnits.Count = 0;
          ReadCaseUnit1();

          // 09/24/02 M. Lachowicz Start
          if (local.CaseUnits.Count == 0)
          {
            ReadCaseUnit2();
          }

          // 09/24/02 M. Lachowicz End
          break;
        case "CH":
          local.CaseUnits.Count = 0;

          // 03/22/02 M. Lachowicz do not check if case unti is opened.
          ReadCaseUnit2();

          break;
        default:
          // ***
          // *** All AP/Supported combinations processed
          // ***
          return;
      }

      if (local.CaseUnits.Count == 0)
      {
        ExitState = "SI0000_CASE_HAS_ACTIVE_DEBTS";

        return;
      }
    }

    local.Supported.CheckIndex();

    // 02/11/02 M. Lachowicz End
    // 02/11/02 M. Lachowicz Start
    // 02/11/02 M. Lachowicz End
    // ***
    // *** ALL AP/Supported combinations in more than ONE case, therefore case 
    // CAN be CLOSED
    // ***
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseFnGetObligationStatus()
  {
    var useImport = new FnGetObligationStatus.Import();
    var useExport = new FnGetObligationStatus.Export();

    useImport.Current.Date = import.Current.Date;
    useImport.CsePerson.Number = local.ApCsePerson.Number;
    MoveObligationType(local.ObligationType, useImport.ObligationType);
    useImport.Obligation.SystemGeneratedIdentifier =
      local.Obligation.SystemGeneratedIdentifier;
    useImport.CsePersonAccount.Type1 = local.HcCpaObligor.Type1;
    useImport.HcOtCAccruing.Classification =
      local.HardcodeAccruing.Classification;
    useImport.HcOtCVoluntary.Classification =
      local.HcOtCVoluntary.Classification;

    Call(FnGetObligationStatus.Execute, useImport, useExport);

    local.Derived.ObligationStatus =
      useExport.ScreenObligationStatus.ObligationStatus;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.OtCFeesClassification.Classification =
      useExport.OtCFeesClassification.Classification;
    local.OtCRecoverClassifica.Classification =
      useExport.OtCRecoverClassification.Classification;
    local.OtCMedicalClassifica.Classification =
      useExport.OtCMedicalClassification.Classification;
    local.HcCpaObligor.Type1 = useExport.CpaObligor.Type1;
    local.HcCpaSupported.Type1 = useExport.CpaSupportedPerson.Type1;
    local.HardcodeAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.DeactiveStatus.Code = useExport.DdshDeactivedStatus.Code;
    local.NonAccruingClassifica.Classification =
      useExport.OtCNonAccruingClassification.Classification;
    local.HcOtCVoluntary.Classification =
      useExport.OtCVoluntaryClassification.Classification;
  }

  private bool ReadCaseRole1()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Ap.Number);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.SupportedCaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", entities.SupportedCsePerson.Number);
        db.SetString(command, "type1", local.Ar.Type1);
        db.SetString(command, "type2", local.Ch.Type1);
      },
      (db, reader) =>
      {
        entities.SupportedCaseRole.CasNumber = db.GetString(reader, 0);
        entities.SupportedCaseRole.CspNumber = db.GetString(reader, 1);
        entities.SupportedCaseRole.Type1 = db.GetString(reader, 2);
        entities.SupportedCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.SupportedCaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.SupportedCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.SupportedCaseRole.Type1);
      });
  }

  private bool ReadCaseRole3()
  {
    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.ApCsePerson.Number);
        db.SetString(command, "type", local.ApCaseRole.Type1);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "status", local.Open.Status ?? "");
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        local.ApRoles.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCaseUnit1()
  {
    return Read("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAr", local.Supported.Item.SupportedCsePerson.Number);
        db.SetNullableString(command, "cspNoAp", local.ApCsePerson.Number);
        db.SetNullableDate(
          command, "closureDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "status", local.Open.Status ?? "");
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        local.CaseUnits.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCaseUnit2()
  {
    return Read("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoChild",
          local.Supported.Item.SupportedCsePerson.Number);
        db.SetNullableString(command, "cspNoAp", local.ApCsePerson.Number);
        db.SetNullableString(command, "status", local.Open.Status ?? "");
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        local.CaseUnits.Count = db.GetInt32(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    entities.ExistingCashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr", local.Obligor.ObligorPersonNumber ?? "");
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
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 5);
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
          local.Pended.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.Suspended.SystemGeneratedIdentifier);
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

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.ArCsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ArCsePerson.Number = db.GetString(reader, 0);
        entities.ArCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadDisbursementStatusHistory()
  {
    entities.DisbursementStatusHistory.Populated = false;

    return Read("ReadDisbursementStatusHistory",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 7);
        entities.DisbursementStatusHistory.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ap.Number);
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligation1()
  {
    entities.KeyOnlyObligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId",
          local.ActiveObligation.Item.ActiveObligation1.
            SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          local.ActiveObligation.Item.ActiveObligationType.
            SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", local.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", local.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.KeyOnlyObligation.CpaType = db.GetString(reader, 0);
        entities.KeyOnlyObligation.CspNumber = db.GetString(reader, 1);
        entities.KeyOnlyObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.KeyOnlyObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.KeyOnlyObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.KeyOnlyObligation.CpaType);
      });
  }

  private IEnumerable<bool> ReadObligation2()
  {
    entities.ExistingObligation.Populated = false;

    return ReadEach("ReadObligation2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", local.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", local.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.KeyOnlyObligation.Populated);
    entities.Debt.Populated = false;
    entities.SupportedCsePerson.Populated = false;

    return ReadEach("ReadObligationTransactionCsePerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.KeyOnlyObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.KeyOnlyObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.KeyOnlyObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.KeyOnlyObligation.CpaType);
        db.
          SetNullableString(command, "cspSupNumber", local.PrevSupported.Number);
          
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.DebtType = db.GetString(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.SupportedCsePerson.Number = db.GetString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.Debt.Populated = true;
        entities.SupportedCsePerson.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId", entities.ExistingObligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 1);
        entities.ExistingObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableString(
          command, "csePersonNumber", entities.ArCsePerson.Number);
        db.SetDate(command, "processDate", new DateTime(1, 1, 1));
        db.SetNullableDate(command, "discontinueDate", date);
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 2);
        entities.PaymentRequest.Type1 = db.GetString(reader, 3);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Case1 case1;
    private CsePersonsWorkSet ap;
    private DateWorkArea current;
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
    /// <summary>A ActiveObligationGroup group.</summary>
    [Serializable]
    public class ActiveObligationGroup
    {
      /// <summary>
      /// A value of ActiveObligation1.
      /// </summary>
      [JsonPropertyName("activeObligation1")]
      public Obligation ActiveObligation1
      {
        get => activeObligation1 ??= new();
        set => activeObligation1 = value;
      }

      /// <summary>
      /// A value of ActiveObligationType.
      /// </summary>
      [JsonPropertyName("activeObligationType")]
      public ObligationType ActiveObligationType
      {
        get => activeObligationType ??= new();
        set => activeObligationType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Obligation activeObligation1;
      private ObligationType activeObligationType;
    }

    /// <summary>A SupportedGroup group.</summary>
    [Serializable]
    public class SupportedGroup
    {
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
      /// A value of SupportedCaseRole.
      /// </summary>
      [JsonPropertyName("supportedCaseRole")]
      public CaseRole SupportedCaseRole
      {
        get => supportedCaseRole ??= new();
        set => supportedCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson supportedCsePerson;
      private CaseRole supportedCaseRole;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public LegalAction Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of PrevSupported.
    /// </summary>
    [JsonPropertyName("prevSupported")]
    public CsePerson PrevSupported
    {
      get => prevSupported ??= new();
      set => prevSupported = value;
    }

    /// <summary>
    /// Gets a value of ActiveObligation.
    /// </summary>
    [JsonIgnore]
    public Array<ActiveObligationGroup> ActiveObligation =>
      activeObligation ??= new(ActiveObligationGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ActiveObligation for json serialization.
    /// </summary>
    [JsonPropertyName("activeObligation")]
    [Computed]
    public IList<ActiveObligationGroup> ActiveObligation_Json
    {
      get => activeObligation;
      set => ActiveObligation.Assign(value);
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CaseRole Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of AobligationType.
    /// </summary>
    [JsonPropertyName("aobligationType")]
    public ObligationType AobligationType
    {
      get => aobligationType ??= new();
      set => aobligationType = value;
    }

    /// <summary>
    /// A value of D.
    /// </summary>
    [JsonPropertyName("d")]
    public ObligationTransaction D
    {
      get => d ??= new();
      set => d = value;
    }

    /// <summary>
    /// A value of AobligationTransaction.
    /// </summary>
    [JsonPropertyName("aobligationTransaction")]
    public ObligationTransaction AobligationTransaction
    {
      get => aobligationTransaction ??= new();
      set => aobligationTransaction = value;
    }

    /// <summary>
    /// Gets a value of Supported.
    /// </summary>
    [JsonIgnore]
    public Array<SupportedGroup> Supported => supported ??= new(
      SupportedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Supported for json serialization.
    /// </summary>
    [JsonPropertyName("supported")]
    [Computed]
    public IList<SupportedGroup> Supported_Json
    {
      get => supported;
      set => Supported.Assign(value);
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CashReceiptDetail Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Pended.
    /// </summary>
    [JsonPropertyName("pended")]
    public CashReceiptDetailStatus Pended
    {
      get => pended ??= new();
      set => pended = value;
    }

    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public CashReceiptDetailStatus Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public Case1 Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of CaseUnits.
    /// </summary>
    [JsonPropertyName("caseUnits")]
    public Common CaseUnits
    {
      get => caseUnits ??= new();
      set => caseUnits = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of OtCFeesClassification.
    /// </summary>
    [JsonPropertyName("otCFeesClassification")]
    public ObligationType OtCFeesClassification
    {
      get => otCFeesClassification ??= new();
      set => otCFeesClassification = value;
    }

    /// <summary>
    /// A value of OtCRecoverClassifica.
    /// </summary>
    [JsonPropertyName("otCRecoverClassifica")]
    public ObligationType OtCRecoverClassifica
    {
      get => otCRecoverClassifica ??= new();
      set => otCRecoverClassifica = value;
    }

    /// <summary>
    /// A value of OtCMedicalClassifica.
    /// </summary>
    [JsonPropertyName("otCMedicalClassifica")]
    public ObligationType OtCMedicalClassifica
    {
      get => otCMedicalClassifica ??= new();
      set => otCMedicalClassifica = value;
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
    /// A value of HcCpaSupported.
    /// </summary>
    [JsonPropertyName("hcCpaSupported")]
    public CsePersonAccount HcCpaSupported
    {
      get => hcCpaSupported ??= new();
      set => hcCpaSupported = value;
    }

    /// <summary>
    /// A value of HardcodeAccruing.
    /// </summary>
    [JsonPropertyName("hardcodeAccruing")]
    public ObligationType HardcodeAccruing
    {
      get => hardcodeAccruing ??= new();
      set => hardcodeAccruing = value;
    }

    /// <summary>
    /// A value of DeactiveStatus.
    /// </summary>
    [JsonPropertyName("deactiveStatus")]
    public DebtDetailStatusHistory DeactiveStatus
    {
      get => deactiveStatus ??= new();
      set => deactiveStatus = value;
    }

    /// <summary>
    /// A value of NonAccruingClassifica.
    /// </summary>
    [JsonPropertyName("nonAccruingClassifica")]
    public ObligationType NonAccruingClassifica
    {
      get => nonAccruingClassifica ??= new();
      set => nonAccruingClassifica = value;
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
    /// A value of Fees.
    /// </summary>
    [JsonPropertyName("fees")]
    public Common Fees
    {
      get => fees ??= new();
      set => fees = value;
    }

    /// <summary>
    /// A value of Others.
    /// </summary>
    [JsonPropertyName("others")]
    public Common Others
    {
      get => others ??= new();
      set => others = value;
    }

    /// <summary>
    /// A value of Recovery.
    /// </summary>
    [JsonPropertyName("recovery")]
    public Common Recovery
    {
      get => recovery ??= new();
      set => recovery = value;
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of Deactivated.
    /// </summary>
    [JsonPropertyName("deactivated")]
    public Common Deactivated
    {
      get => deactivated ??= new();
      set => deactivated = value;
    }

    /// <summary>
    /// A value of Inactive.
    /// </summary>
    [JsonPropertyName("inactive")]
    public Common Inactive
    {
      get => inactive ??= new();
      set => inactive = value;
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
    /// A value of ApRoles.
    /// </summary>
    [JsonPropertyName("apRoles")]
    public Common ApRoles
    {
      get => apRoles ??= new();
      set => apRoles = value;
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

    private LegalAction prev;
    private LegalAction legalAction;
    private Common count;
    private CsePerson prevSupported;
    private Array<ActiveObligationGroup> activeObligation;
    private CaseRole ch;
    private CaseRole ar;
    private ObligationType aobligationType;
    private ObligationTransaction d;
    private ObligationTransaction aobligationTransaction;
    private Array<SupportedGroup> supported;
    private CashReceiptDetail obligor;
    private DateWorkArea current;
    private CashReceiptDetailStatus pended;
    private CashReceiptDetailStatus suspended;
    private Case1 open;
    private Common caseUnits;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationType otCFeesClassification;
    private ObligationType otCRecoverClassifica;
    private ObligationType otCMedicalClassifica;
    private CsePersonAccount hcCpaObligor;
    private CsePersonAccount hcCpaSupported;
    private ObligationType hardcodeAccruing;
    private DebtDetailStatusHistory deactiveStatus;
    private ObligationType nonAccruingClassifica;
    private ObligationType hcOtCVoluntary;
    private Common fees;
    private Common others;
    private Common recovery;
    private Common active;
    private Common deactivated;
    private Common inactive;
    private ScreenObligationStatus derived;
    private Common apRoles;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
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
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of SupportedCaseRole.
    /// </summary>
    [JsonPropertyName("supportedCaseRole")]
    public CaseRole SupportedCaseRole
    {
      get => supportedCaseRole ??= new();
      set => supportedCaseRole = value;
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
    /// A value of KeyOnlyObligation.
    /// </summary>
    [JsonPropertyName("keyOnlyObligation")]
    public Obligation KeyOnlyObligation
    {
      get => keyOnlyObligation ??= new();
      set => keyOnlyObligation = value;
    }

    /// <summary>
    /// A value of KeyOnlyCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("keyOnlyCashReceiptDetailStatus")]
    public CashReceiptDetailStatus KeyOnlyCashReceiptDetailStatus
    {
      get => keyOnlyCashReceiptDetailStatus ??= new();
      set => keyOnlyCashReceiptDetailStatus = value;
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
    /// A value of ExistingCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingCashReceiptDetailStatHistory
    {
      get => existingCashReceiptDetailStatHistory ??= new();
      set => existingCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of ExistingCaseUnit.
    /// </summary>
    [JsonPropertyName("existingCaseUnit")]
    public CaseUnit ExistingCaseUnit
    {
      get => existingCaseUnit ??= new();
      set => existingCaseUnit = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
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
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
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

    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction credit;
    private DisbursementStatus disbursementStatus;
    private CsePersonAccount obligee;
    private DisbursementTransaction debit;
    private DisbursementStatusHistory disbursementStatusHistory;
    private CsePerson arCsePerson;
    private CaseRole arCaseRole;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private PaymentRequest paymentRequest;
    private Case1 case1;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private CaseRole supportedCaseRole;
    private ObligationTransaction debt;
    private Obligation keyOnlyObligation;
    private CashReceiptDetailStatus keyOnlyCashReceiptDetailStatus;
    private CashReceiptDetail existingCashReceiptDetail;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
    private CsePerson apCsePerson;
    private CsePerson supportedCsePerson;
    private CaseUnit existingCaseUnit;
    private CaseRole apCaseRole;
    private CsePersonAccount obligor;
    private CsePersonAccount supportedCsePersonAccount;
    private Case1 existingCase;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
  }
#endregion
}
