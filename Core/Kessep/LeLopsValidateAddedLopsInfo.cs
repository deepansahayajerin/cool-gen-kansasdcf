// Program: LE_LOPS_VALIDATE_ADDED_LOPS_INFO, ID: 372006662, model: 746.
// Short name: SWE01621
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
/// A program: LE_LOPS_VALIDATE_ADDED_LOPS_INFO.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block performs validation checks on the legal obligation person 
/// details added/updated/deleted.
/// </para>
/// </summary>
[Serializable]
public partial class LeLopsValidateAddedLopsInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LOPS_VALIDATE_ADDED_LOPS_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLopsValidateAddedLopsInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLopsValidateAddedLopsInfo.
  /// </summary>
  public LeLopsValidateAddedLopsInfo(IContext context, Import import,
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
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // ??????	govind		Initial code
    // 110797	govind		Fixed to look only for active Legal Action Persons.
    // 020298  R Grey	24917	Now we want to display and message
    // 			end-dated legal action persons.
    // 021698	govind	31908	Fixed to consider end-dated supported persons also.
    // 			Fixed to consider for validation only distinct supported persons' 
    // amounts.
    // 			Fixed to consider discontinued caseroles/ future caseroles when 
    // validating valid AP/AR/CH combinations for the obligor/supported persons
    // in LOPS.
    // 021898	govind		Split the READ EACH for checking caserole combination into
    // two READ EACH
    // 02/01/98 P. Sharp	Changes made per Phase II assessment.
    // 01/24/2000 A. Katuri    PR# H00081822 Each Supported Person must have an 
    // amount entered.
    // 05/02/2000 J. Magat     PR# 93497 If supported person is required for the
    // obligation type, add edit to allow only person with role of CH to be set
    // up as supported person [except obligation type=SP,SAJ which requires an
    // AR role].
    // 05/10/2000 J. Magat	PR# 94603 Allow the flow to OACC for converted legal 
    // details even with zero $ amounts.
    // ---------------------------------------------
    // 02/08/08  M. Fan   CQ2681(HEAT #327444) Changed to allow legal detail 
    // types WA & WC to have the ability
    //                                         
    // to be 0 amount so that the attorney does
    // not have to do an IWO Term.
    // ------------------------------------------------------------------------------------------------
    local.LdetDetail.Code = import.ObligationType.Code;
    local.Current.Date = Now().Date;

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (!ReadLegalActionDetail())
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";

      return;
    }

    // *** Rules for obligors:
    // 1) No more than two obligors can be specified except
    // for obligation type=FEE, IVD RC, there must only be 1;
    // 2) For non-financial legal detail, at least 1 obligor must be specified;
    // 3) For financial legal detail, at least 1 obligor must be specified, and 
    // must have non-zero $ amounts. If with $0 amounts, the Legal Detail must
    // be from "CONVERSN".
    // 02/08/08  M. Fan   CQ2681(HEAT #327444) Changed to allow legal detail 
    // types WA & WC to have the ability
    //                                         
    // to be 0 amount so that the attorney does
    // not have to do an IWO Term.
    // ------------------------------------------------------------------------------------------------
    local.NoOfObligors.Count = 0;

    // *** PR#94603
    foreach(var item in ReadLegalActionPerson4())
    {
      ++local.NoOfObligors.Count;

      if (local.NoOfObligors.Count > 2)
      {
        ExitState = "LE0000_MORE_THAN_TWO_OBLIGORS";

        return;
      }
    }

    if (local.NoOfObligors.Count == 0)
    {
      ExitState = "LE0000_OBLIGOR_OR_AMNT_NOT_SPEC";

      return;
    }

    if (local.NoOfObligors.Count == 2 && (
      Equal(local.LdetDetail.Code, "FEE") || Equal
      (local.LdetDetail.Code, "IVD RC")))
    {
      ExitState = "LE0000_ONE_OBLIGOR_FOR_OBLG_TYPE";

      return;
    }

    // --- No of obligees cannot be more than 1. (It can be 0)
    local.NoOfObligees.Count = 0;

    foreach(var item in ReadLegalActionPerson2())
    {
      ++local.NoOfObligees.Count;

      if (local.NoOfObligees.Count > 1)
      {
        ExitState = "LE0000_MORE_THAN_ONE_OBLIGEE";

        return;
      }
    }

    // --- Now validate supported persons
    // --- Assumption is:
    //     Supported person is required for non-fin obligation (HIC)
    //     For financial obligations, it will be specified by obligation_type
    if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
    {
      local.LdetDetail.SupportedPersonReqInd = "Y";
    }
    else if (ReadObligationType())
    {
      MoveObligationType(entities.ObligationType, local.LdetDetail);
    }
    else
    {
      ExitState = "OBLIGATION_TYPE_NF";

      return;
    }

    // --- Now check the no of supported persons specified.
    // 02/08/08  M. Fan   CQ2681(HEAT #327444) Changed to allow legal detail 
    // types WA & WC to have the ability
    //                                         
    // to be 0 amount so that the attorney does
    // not have to do an IWO Term.
    // ------------------------------------------------------------------------------------------------
    local.NoOfSupportedPersons.Count = 0;
    local.NoSuppEndDate.Count = 0;

    if (ReadLegalActionPerson1())
    {
      ++local.NoOfSupportedPersons.Count;

      if (!Lt(local.Current.Date, entities.SupportedLegalActionPerson.EndDate))
      {
        ++local.NoSuppEndDate.Count;
      }
    }

    switch(AsChar(local.LdetDetail.SupportedPersonReqInd))
    {
      case 'Y':
        if (local.NoSuppEndDate.Count == local.NoOfSupportedPersons.Count)
        {
          break;
        }

        if (local.NoOfSupportedPersons.Count == 0)
        {
          ExitState = "LE0000_ATLEAST_1_SUPP_PERS_REQD";

          return;
        }

        break;
      case 'N':
        if (local.NoOfSupportedPersons.Count != 0)
        {
          ExitState = "LE0000_SUPP_PERSON_NOT_REQD";

          return;
        }

        break;
      default:
        ExitState = "LE0000_INVALID_SUP_PERS_IND_OBTY";

        return;
    }

    // --- Now validate the obligor amount fields against the legal action 
    // detail amount fields.
    // --- It is possible to have concurrent obligors.
    // 02/08/08  M. Fan   CQ2681(HEAT #327444) Changed to allow legal detail 
    // types WA & WC to have the ability
    //                                         
    // to be 0 amount so that the attorney does
    // not have to do an IWO Term.
    // ------------------------------------------------------------------------------------------------
    foreach(var item in ReadLegalActionPerson3())
    {
      if (!Equal(entities.ObligorLegalActionPerson.CurrentAmount,
        entities.LegalActionDetail.CurrentAmount) || !
        Equal(entities.ObligorLegalActionPerson.ArrearsAmount,
        entities.LegalActionDetail.ArrearsAmount) || !
        Equal(entities.ObligorLegalActionPerson.JudgementAmount,
        entities.LegalActionDetail.JudgementAmount))
      {
        ExitState = "LE0000_TOTAL_NOT_TALLY_LDET_OBGR";

        return;
      }
    }

    // --- Now validate the supported person amount fields against the legal 
    // action detail amount fields.
    if (AsChar(local.LdetDetail.SupportedPersonReqInd) == 'Y')
    {
      if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
      {
        local.Total.CurrentAmount = 0;
        local.Total.ArrearsAmount = 0;
        local.Total.JudgementAmount = 0;
        local.NoOfSupportedPersons.Count = 0;

        foreach(var item in ReadLegalActionPersonCsePerson2())
        {
          if (Equal(entities.SupportedCsePerson.Number,
            local.PrevSupported.Number))
          {
            continue;
          }

          local.PrevSupported.Number = entities.SupportedCsePerson.Number;
          local.Total.CurrentAmount =
            local.Total.CurrentAmount.GetValueOrDefault() + entities
            .SupportedLegalActionPerson.CurrentAmount.GetValueOrDefault();
          local.Total.ArrearsAmount =
            local.Total.ArrearsAmount.GetValueOrDefault() + entities
            .SupportedLegalActionPerson.ArrearsAmount.GetValueOrDefault();
          local.Total.JudgementAmount =
            local.Total.JudgementAmount.GetValueOrDefault() + entities
            .SupportedLegalActionPerson.JudgementAmount.GetValueOrDefault();

          // *** PR H00081822 : Introduced the following 3 statements to force 
          // amounts for Supported Persons.  - Anand
          if (Lt(0, entities.LegalActionDetail.CurrentAmount))
          {
            if (!Lt(0, entities.SupportedLegalActionPerson.CurrentAmount))
            {
              ExitState = "LE0000_AMT_REQ_FOR_SUPP_PERSONS";

              return;
            }
          }

          if (Lt(0, entities.LegalActionDetail.ArrearsAmount))
          {
            if (!Lt(0, entities.SupportedLegalActionPerson.ArrearsAmount))
            {
              ExitState = "LE0000_AMT_REQ_FOR_SUPP_PERSONS";

              return;
            }
          }

          if (Lt(0, entities.LegalActionDetail.JudgementAmount))
          {
            if (!Lt(0, entities.SupportedLegalActionPerson.JudgementAmount))
            {
              ExitState = "LE0000_AMT_REQ_FOR_SUPP_PERSONS";

              return;
            }
          }
        }

        if (!Equal(local.Total.CurrentAmount.GetValueOrDefault(),
          entities.LegalActionDetail.CurrentAmount) || !
          Equal(local.Total.ArrearsAmount.GetValueOrDefault(),
          entities.LegalActionDetail.ArrearsAmount) || !
          Equal(local.Total.JudgementAmount.GetValueOrDefault(),
          entities.LegalActionDetail.JudgementAmount))
        {
          ExitState = "LE0000_TOTALS_NO_TALLY_FOR_SUPP";

          return;
        }
      }
    }

    // --- Now validate against the cse case roles.
    if (AsChar(local.LdetDetail.SupportedPersonReqInd) == 'Y')
    {
      foreach(var item in ReadLegalActionPersonCsePerson1())
      {
        if (!ReadLaPersonLaCaseRoleLegalActionCaseRoleCaseRole())
        {
          // --- This supported person is a non cse person since no legal action
          // case role record exists. So dont look for a matching cse case.
          continue;
        }

        local.ObligorCaseRoleMatched.Flag = "N";

        // *** If the obligor has three children in three different cse cases, 
        // then the user needs to specify only one of the AP case role and the
        // three child case roles. The user need not specify R/AP/C1, R/AP/C2, R
        // /AP/C3 ..
        // This implies that LEGAL ACTION CASE ROLE will be created only for one
        // of the cse cases for the obligor. This should not be a problem since
        // it will create Legal Action Case Roles for each supported person's
        // caserole.
        // The Legal Action Case Role (for the LOPS Detail) is used only to 
        // derive related cse cases. So as long as LA Case role is created for
        // CH role, it is okay.
        foreach(var item1 in ReadCaseCaseRoleLegalActionCaseRole())
        {
          switch(TrimEnd(local.LdetDetail.Code))
          {
            case "SP":
              if (!Equal(entities.SupportedCaseRole.Type1, "AR"))
              {
                continue;
              }

              break;
            case "SAJ":
              if (!Equal(entities.SupportedCaseRole.Type1, "AR"))
              {
                continue;
              }

              break;
            default:
              // *** PR#93497
              if (!Equal(entities.SupportedCaseRole.Type1, "CH"))
              {
                continue;
              }

              break;
          }

          foreach(var item2 in ReadCaseRoleLegalActionCaseRoleLaPersonLaCaseRole())
            
          {
            switch(TrimEnd(local.LdetDetail.Code))
            {
              case "CS":
                if (!Equal(entities.ObligorCaseRole.Type1, "AP"))
                {
                  continue;
                }

                break;
              case "SP":
                if (!Equal(entities.ObligorCaseRole.Type1, "AP"))
                {
                  continue;
                }

                break;
              case "SAJ":
                if (!Equal(entities.ObligorCaseRole.Type1, "AP"))
                {
                  continue;
                }

                break;
              case "718B":
                if (!Equal(entities.ObligorCaseRole.Type1, "AP"))
                {
                  continue;
                }

                break;
              case "ARRJ":
                if (!Equal(entities.ObligorCaseRole.Type1, "AP"))
                {
                  continue;
                }

                break;
              default:
                // --- We are not sure. So long as the obligor and the supported
                // person are in the same cse case, let us allow.
                break;
            }

            local.ObligorCaseRoleMatched.Flag = "Y";

            goto ReadEach;
          }
        }

ReadEach:

        if (AsChar(local.ObligorCaseRoleMatched.Flag) == 'N')
        {
          ExitState = "LE0000_INVALID_COMBO_ENTERED";

          return;
        }
      }
    }
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SupportedPersonReqInd = source.SupportedPersonReqInd;
    target.Code = source.Code;
  }

  private IEnumerable<bool> ReadCaseCaseRoleLegalActionCaseRole()
  {
    entities.SupportedLegalActionCaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.SupportedCaseRole.Populated = false;

    return ReadEach("ReadCaseCaseRoleLegalActionCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.SupportedCsePerson.Number);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.SupportedCaseRole.CasNumber = db.GetString(reader, 0);
        entities.SupportedLegalActionCaseRole.CasNumber =
          db.GetString(reader, 0);
        entities.SupportedLegalActionCaseRole.CasNumber =
          db.GetString(reader, 0);
        entities.SupportedCaseRole.CspNumber = db.GetString(reader, 1);
        entities.SupportedLegalActionCaseRole.CspNumber =
          db.GetString(reader, 1);
        entities.SupportedCaseRole.Type1 = db.GetString(reader, 2);
        entities.SupportedLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.SupportedCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.SupportedLegalActionCaseRole.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.SupportedCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.SupportedCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.SupportedLegalActionCaseRole.LgaId = db.GetInt32(reader, 6);
        entities.SupportedLegalActionCaseRole.CreatedBy =
          db.GetString(reader, 7);
        entities.SupportedLegalActionCaseRole.CreatedTstamp =
          db.GetDateTime(reader, 8);
        entities.SupportedLegalActionCaseRole.Populated = true;
        entities.Case1.Populated = true;
        entities.SupportedCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.SupportedCaseRole.Type1);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.SupportedLegalActionCaseRole.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleLegalActionCaseRoleLaPersonLaCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligorLaPersonLaCaseRole.Populated = false;
    entities.ObligorLegalActionCaseRole.Populated = false;
    entities.ObligorCaseRole.Populated = false;
    entities.ObligorCsePerson.Populated = false;
    entities.ObligorLegalActionPerson.Populated = false;

    return ReadEach("ReadCaseRoleLegalActionCaseRoleLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ObligorCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ObligorCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ObligorCsePerson.Number = db.GetString(reader, 1);
        entities.ObligorCaseRole.Type1 = db.GetString(reader, 2);
        entities.ObligorCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ObligorCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ObligorCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ObligorLegalActionCaseRole.CasNumber = db.GetString(reader, 6);
        entities.ObligorLaPersonLaCaseRole.CasNum = db.GetString(reader, 6);
        entities.ObligorLegalActionCaseRole.CspNumber = db.GetString(reader, 7);
        entities.ObligorLaPersonLaCaseRole.CspNum = db.GetString(reader, 7);
        entities.ObligorLegalActionCaseRole.CroType = db.GetString(reader, 8);
        entities.ObligorLaPersonLaCaseRole.CroType = db.GetString(reader, 8);
        entities.ObligorLegalActionCaseRole.CroIdentifier =
          db.GetInt32(reader, 9);
        entities.ObligorLaPersonLaCaseRole.CroId = db.GetInt32(reader, 9);
        entities.ObligorLegalActionCaseRole.LgaId = db.GetInt32(reader, 10);
        entities.ObligorLaPersonLaCaseRole.LgaId = db.GetInt32(reader, 10);
        entities.ObligorLegalActionCaseRole.CreatedBy =
          db.GetString(reader, 11);
        entities.ObligorLegalActionCaseRole.CreatedTstamp =
          db.GetDateTime(reader, 12);
        entities.ObligorLaPersonLaCaseRole.Identifier = db.GetInt32(reader, 13);
        entities.ObligorLaPersonLaCaseRole.LapId = db.GetInt32(reader, 14);
        entities.ObligorLegalActionPerson.Identifier = db.GetInt32(reader, 15);
        entities.ObligorLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 16);
        entities.ObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 17);
        entities.ObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 18);
        entities.ObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 19);
        entities.ObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 20);
        entities.ObligorLegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 21);
        entities.ObligorLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 22);
        entities.ObligorLegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 23);
        entities.ObligorLaPersonLaCaseRole.Populated = true;
        entities.ObligorLegalActionCaseRole.Populated = true;
        entities.ObligorCaseRole.Populated = true;
        entities.ObligorCsePerson.Populated = true;
        entities.ObligorLegalActionPerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ObligorCaseRole.Type1);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.ObligorLegalActionCaseRole.CroType);
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.ObligorLaPersonLaCaseRole.CroType);

        return true;
      });
  }

  private bool ReadLaPersonLaCaseRoleLegalActionCaseRoleCaseRole()
  {
    entities.SupportedLaPersonLaCaseRole.Populated = false;
    entities.SupportedLegalActionCaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.SupportedCaseRole.Populated = false;

    return Read("ReadLaPersonLaCaseRoleLegalActionCaseRoleCaseRole",
      (db, command) =>
      {
        db.SetInt32(
          command, "lapId", entities.SupportedLegalActionPerson.Identifier);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.SupportedLaPersonLaCaseRole.Identifier =
          db.GetInt32(reader, 0);
        entities.SupportedLaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.SupportedLegalActionCaseRole.CroIdentifier =
          db.GetInt32(reader, 1);
        entities.SupportedCaseRole.Identifier = db.GetInt32(reader, 1);
        entities.SupportedLaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.SupportedLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.SupportedCaseRole.Type1 = db.GetString(reader, 2);
        entities.SupportedLaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.SupportedLegalActionCaseRole.CspNumber =
          db.GetString(reader, 3);
        entities.SupportedCaseRole.CspNumber = db.GetString(reader, 3);
        entities.SupportedLaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.SupportedLegalActionCaseRole.CasNumber =
          db.GetString(reader, 4);
        entities.SupportedCaseRole.CasNumber = db.GetString(reader, 4);
        entities.Case1.Number = db.GetString(reader, 4);
        entities.Case1.Number = db.GetString(reader, 4);
        entities.SupportedLaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.SupportedLegalActionCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.SupportedLaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.SupportedLegalActionCaseRole.CreatedBy =
          db.GetString(reader, 7);
        entities.SupportedLegalActionCaseRole.CreatedTstamp =
          db.GetDateTime(reader, 8);
        entities.SupportedCaseRole.StartDate = db.GetNullableDate(reader, 9);
        entities.SupportedCaseRole.EndDate = db.GetNullableDate(reader, 10);
        entities.SupportedLaPersonLaCaseRole.Populated = true;
        entities.SupportedLegalActionCaseRole.Populated = true;
        entities.Case1.Populated = true;
        entities.SupportedCaseRole.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.SupportedLaPersonLaCaseRole.CroType);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.SupportedLegalActionCaseRole.CroType);
        CheckValid<CaseRole>("Type1", entities.SupportedCaseRole.Type1);
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
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.CreatedBy = db.GetString(reader, 2);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 3);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 6);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 7);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 8);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionPerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.SupportedLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetString(
          command, "detailType", entities.LegalActionDetail.DetailType);
        db.SetString(command, "code", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.SupportedLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.SupportedLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.SupportedLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 2);
        entities.SupportedLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 3);
        entities.SupportedLegalActionPerson.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.SupportedLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.SupportedLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 6);
        entities.SupportedLegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.SupportedLegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 8);
        entities.SupportedLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 9);
        entities.SupportedLegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 10);
        entities.SupportedLegalActionPerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Obligee.Populated = false;

    return ReadEach("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Obligee.Identifier = db.GetInt32(reader, 0);
        entities.Obligee.EffectiveDate = db.GetDate(reader, 1);
        entities.Obligee.EndDate = db.GetNullableDate(reader, 2);
        entities.Obligee.LgaRIdentifier = db.GetNullableInt32(reader, 3);
        entities.Obligee.LadRNumber = db.GetNullableInt32(reader, 4);
        entities.Obligee.AccountType = db.GetNullableString(reader, 5);
        entities.Obligee.ArrearsAmount = db.GetNullableDecimal(reader, 6);
        entities.Obligee.CurrentAmount = db.GetNullableDecimal(reader, 7);
        entities.Obligee.JudgementAmount = db.GetNullableDecimal(reader, 8);
        entities.Obligee.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPerson3()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligorLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPerson3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "detailType", entities.LegalActionDetail.DetailType);
        db.SetString(command, "code", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligorLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ObligorLegalActionPerson.EffectiveDate = db.GetDate(reader, 1);
        entities.ObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 2);
        entities.ObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.ObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 4);
        entities.ObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 5);
        entities.ObligorLegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ObligorLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ObligorLegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 8);
        entities.ObligorLegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPerson4()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligorLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPerson4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetString(
          command, "detailType", entities.LegalActionDetail.DetailType);
        db.
          SetString(command, "createdBy", entities.LegalActionDetail.CreatedBy);
          
        db.SetString(command, "code", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligorLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ObligorLegalActionPerson.EffectiveDate = db.GetDate(reader, 1);
        entities.ObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 2);
        entities.ObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.ObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 4);
        entities.ObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 5);
        entities.ObligorLegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ObligorLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ObligorLegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 8);
        entities.ObligorLegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.SupportedCsePerson.Populated = false;
    entities.SupportedLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.SupportedLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.SupportedLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.SupportedCsePerson.Number = db.GetString(reader, 1);
        entities.SupportedLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 2);
        entities.SupportedLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 3);
        entities.SupportedLegalActionPerson.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.SupportedLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.SupportedLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 6);
        entities.SupportedLegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.SupportedLegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 8);
        entities.SupportedLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 9);
        entities.SupportedLegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 10);
        entities.SupportedCsePerson.Populated = true;
        entities.SupportedLegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.SupportedCsePerson.Populated = false;
    entities.SupportedLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.SupportedLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.SupportedLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.SupportedCsePerson.Number = db.GetString(reader, 1);
        entities.SupportedLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 2);
        entities.SupportedLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 3);
        entities.SupportedLegalActionPerson.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.SupportedLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.SupportedLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 6);
        entities.SupportedLegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.SupportedLegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 8);
        entities.SupportedLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 9);
        entities.SupportedLegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 10);
        entities.SupportedCsePerson.Populated = true;
        entities.SupportedLegalActionPerson.Populated = true;

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 5);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private ObligationType obligationType;
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
    /// A value of NoSuppEndDate.
    /// </summary>
    [JsonPropertyName("noSuppEndDate")]
    public Common NoSuppEndDate
    {
      get => noSuppEndDate ??= new();
      set => noSuppEndDate = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of ObligorCaseRoleMatched.
    /// </summary>
    [JsonPropertyName("obligorCaseRoleMatched")]
    public Common ObligorCaseRoleMatched
    {
      get => obligorCaseRoleMatched ??= new();
      set => obligorCaseRoleMatched = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public LegalActionDetail Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of NoOfObligees.
    /// </summary>
    [JsonPropertyName("noOfObligees")]
    public Common NoOfObligees
    {
      get => noOfObligees ??= new();
      set => noOfObligees = value;
    }

    /// <summary>
    /// A value of NoOfSupportedPersons.
    /// </summary>
    [JsonPropertyName("noOfSupportedPersons")]
    public Common NoOfSupportedPersons
    {
      get => noOfSupportedPersons ??= new();
      set => noOfSupportedPersons = value;
    }

    /// <summary>
    /// A value of NoOfObligors.
    /// </summary>
    [JsonPropertyName("noOfObligors")]
    public Common NoOfObligors
    {
      get => noOfObligors ??= new();
      set => noOfObligors = value;
    }

    /// <summary>
    /// A value of LdetDetail.
    /// </summary>
    [JsonPropertyName("ldetDetail")]
    public ObligationType LdetDetail
    {
      get => ldetDetail ??= new();
      set => ldetDetail = value;
    }

    private Common noSuppEndDate;
    private CsePerson prevSupported;
    private DateWorkArea current;
    private Common obligorCaseRoleMatched;
    private LegalActionDetail total;
    private Common noOfObligees;
    private Common noOfSupportedPersons;
    private Common noOfObligors;
    private ObligationType ldetDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligorLaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("obligorLaPersonLaCaseRole")]
    public LaPersonLaCaseRole ObligorLaPersonLaCaseRole
    {
      get => obligorLaPersonLaCaseRole ??= new();
      set => obligorLaPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of SupportedLaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("supportedLaPersonLaCaseRole")]
    public LaPersonLaCaseRole SupportedLaPersonLaCaseRole
    {
      get => supportedLaPersonLaCaseRole ??= new();
      set => supportedLaPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of ObligorLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("obligorLegalActionCaseRole")]
    public LegalActionCaseRole ObligorLegalActionCaseRole
    {
      get => obligorLegalActionCaseRole ??= new();
      set => obligorLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of SupportedLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("supportedLegalActionCaseRole")]
    public LegalActionCaseRole SupportedLegalActionCaseRole
    {
      get => supportedLegalActionCaseRole ??= new();
      set => supportedLegalActionCaseRole = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public LegalActionPerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of ObligorCaseRole.
    /// </summary>
    [JsonPropertyName("obligorCaseRole")]
    public CaseRole ObligorCaseRole
    {
      get => obligorCaseRole ??= new();
      set => obligorCaseRole = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
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
    /// A value of ObligorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("obligorLegalActionPerson")]
    public LegalActionPerson ObligorLegalActionPerson
    {
      get => obligorLegalActionPerson ??= new();
      set => obligorLegalActionPerson = value;
    }

    /// <summary>
    /// A value of SupportedLegalActionPerson.
    /// </summary>
    [JsonPropertyName("supportedLegalActionPerson")]
    public LegalActionPerson SupportedLegalActionPerson
    {
      get => supportedLegalActionPerson ??= new();
      set => supportedLegalActionPerson = value;
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

    private LaPersonLaCaseRole obligorLaPersonLaCaseRole;
    private LaPersonLaCaseRole supportedLaPersonLaCaseRole;
    private LegalActionCaseRole obligorLegalActionCaseRole;
    private LegalActionCaseRole supportedLegalActionCaseRole;
    private Case1 case1;
    private ObligationType obligationType;
    private LegalActionPerson obligee;
    private CaseRole obligorCaseRole;
    private CaseRole supportedCaseRole;
    private CsePerson obligorCsePerson;
    private CsePerson supportedCsePerson;
    private LegalActionPerson obligorLegalActionPerson;
    private LegalActionPerson supportedLegalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }
#endregion
}
