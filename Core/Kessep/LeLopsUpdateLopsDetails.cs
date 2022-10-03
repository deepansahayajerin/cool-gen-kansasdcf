// Program: LE_LOPS_UPDATE_LOPS_DETAILS, ID: 372006649, model: 746.
// Short name: SWE00835
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LOPS_UPDATE_LOPS_DETAILS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will create or update the Legal Action Person related to 
/// the CSE Case number entered and CSE Person selected. It also creates and
/// associates related entity types namely Legal Action Caserole and LA Person
/// LA Caserole.
/// </para>
/// </summary>
[Serializable]
public partial class LeLopsUpdateLopsDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LOPS_UPDATE_LOPS_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLopsUpdateLopsDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLopsUpdateLopsDetails.
  /// </summary>
  public LeLopsUpdateLopsDetails(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 08/07/95	Dave Allen			Initial Code
    // 10/09/96	govind		Modified to relate legal actions to cse cases.
    // 10/24/97	govind		Changed Legal Action Person Identifier to a random 
    // number;
    // 				Moved the CREATE and UPDATE to separate acblks.
    // 11/07/97	govind		Modified to maintain the history of changes (
    // discontinuance) to the supported persons.
    // 03/15/98	Rod Grey	H00030734/24917  Modify creation of Legal Action 
    // Persons and associations.
    // ------------------------------------------------------------
    // ------------------------------------------------------------
    // Read the Legal Action Detail passed from the "Legal Detail"
    // screen.
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    MoveLegalActionPerson(import.LegalActionPerson, local.LegalActionPerson);

    if (!Lt(local.Zero.Date, import.LegalActionPerson.EndDate))
    {
      local.LegalActionPerson.EndDate = local.MaxDate.Date;
    }

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

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ------------------------------------------------------------
    // Read the Person selected from the list.
    // ------------------------------------------------------------
    if (import.LegalActionPerson.Identifier != 0)
    {
      // --- It is an update to an existing legal action person
      if (!ReadLegalActionPerson2())
      {
        ExitState = "CO0000_LEGAL_ACTION_PERSON_NF";

        return;
      }

      if (IsEmpty(import.LegalActionPerson.AccountType))
      {
        ExitState = "LE0000_INVALID_OBLIG_PERSON_TYPE";

        return;
      }

      UseLeCabUpdateLegActPersRec();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ****************************************************************
      // * In order to prevent further updates of converted LOPS
      // * modify the legal detail created by user id which is used
      // * in determining whether or not to allow LOPS to be unprotected.
      // ****************************************************************
      if (Equal(entities.LegalActionDetail.CreatedBy, "CONVERSN"))
      {
        try
        {
          UpdateLegalActionDetail();
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
    }
    else
    {
      // --- It is a new legal action person being created
      // ------------------------------------------------------------
      // Check if a legal action person with an overlapping period already 
      // exists for the Legal Action Detail - CSE Person.
      // We need to be able to create more than one Legal Action Person for a 
      // Legal Detail - CSE Person, by discontinuing one and creating another
      // for two different time periods. For e.g. we want to end date one of the
      // children but without losing the original obligor's obligation details.
      //   Period 1: Obligor   $200
      //             Child 1   $100
      //             Child 2   $100
      //   Period 2: Obligor   $100
      //             Child 2   $100 (i.e. Ch1 end dated)
      // And we want to be able to see both the original and the new updated 
      // records for the purpose of history. We don't want to just update the
      // original LOPS records
      // ------------------------------------------------------------
      if (ReadLegalActionPerson1())
      {
        // ---  RCG 03/98  Add qualifier for Account Type.
        // --- Legal Action Person already created. So skip it without any 
        // message. The reason is the user may specify the same person as
        // obligor under more than one case(role). i.e. Obligor R1 is to pay Ch1
        // in C1, Ch2 in C2 $100 each. This may be entered with { R/AP/C1, S/
        // CH,C1}, { R/AP/C2, S/CH,C2 } as 4 entries in LOPS. We want to create
        // only one R record (not two) in Legal Action Person; but we want to
        // create two legal action case role record for the R ( AP/C1, AP/C2 )
      }

      // --- The Legal Action Person record has not been created yet. Allow 
      // amount fields to be zero so that we will be able to correct the
      // amounts.
      if (IsEmpty(import.LegalActionPerson.AccountType))
      {
        ExitState = "LE0000_INVALID_OBLIG_PERSON_TYPE";

        return;
      }

      // -- Check if there is another EP detail for the supported person...
      if (AsChar(entities.LegalAction.Classification) == 'J' && Equal
        (entities.LegalActionDetail.NonFinOblgType, "EP") && AsChar
        (local.LegalActionPerson.AccountType) == 'S' && !
        Lt(local.Current.Date, local.LegalActionPerson.EffectiveDate) && !
        Lt(local.LegalActionPerson.EndDate, local.Current.Date))
      {
        if (ReadLegalActionPerson4())
        {
          // -- Cannot simultaneously be a supported person on two active EP 
          // legal details.
          ExitState = "LE0000_ONE_ACTIVE_EP_LDET_FOR_CH";

          return;
        }
        else
        {
          // -- Continue.
        }
      }

      UseLeLopsCreateLegActPers();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // --- Get the currency for subsequent ASSOCIATEs
      if (!ReadLegalActionPerson3())
      {
        ExitState = "CO0000_LEGAL_ACTION_PERSON_NF";

        return;
      }

      // --- Create Legal Action Case Role record if the LOPS role is assoc with
      // a cse case role.
      if (!IsEmpty(import.Case1.Number) && !IsEmpty(import.CaseRole.Type1))
      {
        if (!ReadCaseRoleCase())
        {
          ExitState = "CASE_ROLE_NF";

          return;
        }

        if (!ReadLegalActionCaseRole())
        {
          ExitState = "LE0000_LEGAL_ROLE_NF";

          return;
        }

        // --- Assoc the legal action case role with legal action person in 
        // order to identify whether the legal action case role was created for
        // LROL or LOPS.
        try
        {
          CreateLaPersonLaCaseRole();
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

      // -- If the legal action has a filed date and it is ZCS obligation type 
      // then create
      // infrastructure record so the life cycle can be processed.
      if (AsChar(entities.LegalAction.Classification) == 'J' && !
        Equal(entities.LegalAction.FiledDate, local.Zero.Date) && Equal
        (entities.LegalActionDetail.NonFinOblgType, "ZCS") && AsChar
        (entities.LegalActionDetail.DetailType) == 'N' && !
        Lt(local.Current.Date, entities.LegalActionDetail.EffectiveDate) && !
        Lt(entities.LegalActionDetail.EndDate, local.Current.Date) && AsChar
        (entities.LegalActionPerson.AccountType) == 'R' && !
        Lt(local.Current.Date, entities.LegalActionPerson.EffectiveDate) && !
        Lt(local.LegalActionPerson.EndDate, local.Current.Date))
      {
        // -- Raise the ZCS event.
        local.Infrastructure.UserId = "LACT";
        local.Infrastructure.DenormNumeric12 = entities.LegalAction.Identifier;
        local.Infrastructure.EventId = 45;
        local.Infrastructure.DenormText12 =
          entities.LegalAction.CourtCaseNumber;
        local.Infrastructure.BusinessObjectCd = "LEA";
        local.Infrastructure.ReasonCode = "OBLGZCSRECKS";
        local.Infrastructure.Detail =
          "ZCS OBLIGATION TRANSACTION CREATED FOR AP: " + entities
          .CsePerson.Number;
        local.Infrastructure.ReferenceDate = import.LegalAction.FiledDate;
        UseOeCabRaiseEvent();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

          return;
        }
      }

      // -- If the legal action has a filed date then set the paternity 
      // indicator and the date paternity established.
      if (AsChar(entities.LegalAction.Classification) == 'J' && !
        Equal(entities.LegalAction.FiledDate, local.Zero.Date) && Equal
        (entities.LegalActionDetail.NonFinOblgType, "EP") && AsChar
        (entities.LegalActionDetail.DetailType) == 'N' && !
        Lt(local.Current.Date, entities.LegalActionDetail.EffectiveDate) && !
        Lt(entities.LegalActionDetail.EndDate, local.Current.Date) && AsChar
        (entities.LegalActionPerson.AccountType) == 'S' && !
        Lt(local.Current.Date, entities.LegalActionPerson.EffectiveDate) && !
        Lt(local.LegalActionPerson.EndDate, local.Current.Date))
      {
        // -- Verify that setting the paternity established indicator to 'Y' 
        // will
        //    not result in an invalid combination of born out of wedlock, cse 
        // to
        //    establish paternity, and paternity established indicators.
        local.CsePerson.Assign(entities.CsePerson);
        local.CsePerson.PaternityEstablishedIndicator = "Y";
        UseSiCabValidatePaternityComb();

        if (IsExitState("SI0000_INV_PATERNITY_IND_COMB"))
        {
          ExitState = "LE0000_INVALID_PATERNITY_FLAGS";

          return;
        }

        // -- Edit for more than one active AP on any case where the
        //    child has an active child role.  Can not establish paternity
        //    if more than one AP.
        UseSiCabCountActiveMaleAps();

        if (local.ActiveApOnCase.Count > 1)
        {
          ExitState = "SI0000_INV_PATERNITY_ESTAB2";

          return;
        }

        // -- Set the paternity established indicator to 'Y' and the date 
        // paternity
        //    established to the filed date of the legal action.
        try
        {
          UpdateCsePerson();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CSE_PERSON_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CSE_PERSON_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // -- Raise the paternity established event.
        local.Infrastructure.EventId = 11;
        local.Infrastructure.ReasonCode = "PATESTAB";
        local.Infrastructure.Detail =
          "Journal Entry establishing paternity filed on " + NumberToString
          (Month(entities.LegalAction.FiledDate), 14, 2) + NumberToString
          (Day(entities.LegalAction.FiledDate), 14, 2) + NumberToString
          (Year(entities.LegalAction.FiledDate), 12, 4);
        UseSiChdsRaiseEvent();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_ERROR_ON_EVENT_CREATION";
        }
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveLegalActionPerson(LegalActionPerson source,
    LegalActionPerson target)
  {
    target.Identifier = source.Identifier;
    target.AccountType = source.AccountType;
    target.EffectiveDate = source.EffectiveDate;
    target.EndDate = source.EndDate;
    target.EndReason = source.EndReason;
    target.ArrearsAmount = source.ArrearsAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.JudgementAmount = source.JudgementAmount;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseLeCabUpdateLegActPersRec()
  {
    var useImport = new LeCabUpdateLegActPersRec.Import();
    var useExport = new LeCabUpdateLegActPersRec.Export();

    useImport.LegalActionPerson.Assign(local.LegalActionPerson);

    Call(LeCabUpdateLegActPersRec.Execute, useImport, useExport);
  }

  private void UseLeLopsCreateLegActPers()
  {
    var useImport = new LeLopsCreateLegActPers.Import();
    var useExport = new LeLopsCreateLegActPers.Export();

    useImport.LegalActionDetail.Number = import.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = import.LegalAction.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.LegalActionPerson.Assign(local.LegalActionPerson);

    Call(LeLopsCreateLegActPers.Execute, useImport, useExport);

    local.LegalActionPerson.Assign(useExport.LegalActionPerson);
  }

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(OeCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseSiCabCountActiveMaleAps()
  {
    var useImport = new SiCabCountActiveMaleAps.Import();
    var useExport = new SiCabCountActiveMaleAps.Export();

    useImport.Child.Number = entities.CsePerson.Number;
    MoveDateWorkArea(local.Current, useImport.Current);

    Call(SiCabCountActiveMaleAps.Execute, useImport, useExport);

    local.ActiveApOnCase.Count = useExport.ActiveApOnCase.Count;
  }

  private void UseSiCabValidatePaternityComb()
  {
    var useImport = new SiCabValidatePaternityComb.Import();
    var useExport = new SiCabValidatePaternityComb.Export();

    useImport.CsePerson.Assign(local.CsePerson);

    Call(SiCabValidatePaternityComb.Execute, useImport, useExport);
  }

  private void UseSiChdsRaiseEvent()
  {
    var useImport = new SiChdsRaiseEvent.Import();
    var useExport = new SiChdsRaiseEvent.Export();

    useImport.Ch.Number = entities.CsePerson.Number;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SiChdsRaiseEvent.Execute, useImport, useExport);
  }

  private void CreateLaPersonLaCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);

    var identifier = 1;
    var croId = entities.LegalActionCaseRole.CroIdentifier;
    var croType = entities.LegalActionCaseRole.CroType;
    var cspNum = entities.LegalActionCaseRole.CspNumber;
    var casNum = entities.LegalActionCaseRole.CasNumber;
    var lgaId = entities.LegalActionCaseRole.LgaId;
    var lapId = entities.LegalActionPerson.Identifier;

    CheckValid<LaPersonLaCaseRole>("CroType", croType);
    entities.LaPersonLaCaseRole.Populated = false;
    Update("CreateLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetInt32(command, "croId", croId);
        db.SetString(command, "croType", croType);
        db.SetString(command, "cspNum", cspNum);
        db.SetString(command, "casNum", casNum);
        db.SetInt32(command, "lgaId", lgaId);
        db.SetInt32(command, "lapId", lapId);
      });

    entities.LaPersonLaCaseRole.Identifier = identifier;
    entities.LaPersonLaCaseRole.CroId = croId;
    entities.LaPersonLaCaseRole.CroType = croType;
    entities.LaPersonLaCaseRole.CspNum = cspNum;
    entities.LaPersonLaCaseRole.CasNum = casNum;
    entities.LaPersonLaCaseRole.LgaId = lgaId;
    entities.LaPersonLaCaseRole.LapId = lapId;
    entities.LaPersonLaCaseRole.Populated = true;
  }

  private bool ReadCaseRoleCase()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", import.CaseRole.Identifier);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 4);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 5);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 6);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 7);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.AttorneyApproval = db.GetNullableString(reader, 2);
        entities.LegalAction.ApprovalSentDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.PetitionerApproval =
          db.GetNullableString(reader, 4);
        entities.LegalAction.ApprovalReceivedDate =
          db.GetNullableDate(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.ActionTaken = db.GetString(reader, 7);
        entities.LegalAction.Type1 = db.GetString(reader, 8);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.ForeignOrderRegistrationDate =
          db.GetNullableDate(reader, 10);
        entities.LegalAction.UresaSentDate = db.GetNullableDate(reader, 11);
        entities.LegalAction.UresaAcknowledgedDate =
          db.GetNullableDate(reader, 12);
        entities.LegalAction.UifsaSentDate = db.GetNullableDate(reader, 13);
        entities.LegalAction.UifsaAcknowledgedDate =
          db.GetNullableDate(reader, 14);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 15);
        entities.LegalAction.InitiatingCounty =
          db.GetNullableString(reader, 16);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 17);
        entities.LegalAction.RespondingCounty =
          db.GetNullableString(reader, 18);
        entities.LegalAction.OrderAuthority = db.GetString(reader, 19);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 20);
        entities.LegalAction.RefileDate = db.GetNullableDate(reader, 21);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 22);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 23);
        entities.LegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 24);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 25);
        entities.LegalAction.LongArmStatuteIndicator =
          db.GetNullableString(reader, 26);
        entities.LegalAction.DismissalCode = db.GetNullableString(reader, 27);
        entities.LegalAction.LastUpdatedBy = db.GetNullableString(reader, 28);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 29);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 30);
        entities.LegalAction.ForeignFipsState = db.GetNullableInt32(reader, 31);
        entities.LegalAction.ForeignFipsCounty =
          db.GetNullableInt32(reader, 32);
        entities.LegalAction.ForeignFipsLocation =
          db.GetNullableInt32(reader, 33);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 34);
        entities.LegalAction.NonCsePetitioner =
          db.GetNullableString(reader, 35);
        entities.LegalAction.DateNonCpReqIwoBegin =
          db.GetNullableDate(reader, 36);
        entities.LegalAction.DateCpReqIwoBegin = db.GetNullableDate(reader, 37);
        entities.LegalAction.CtOrdAltBillingAddrInd =
          db.GetNullableString(reader, 38);
        entities.LegalAction.InitiatingCountry =
          db.GetNullableString(reader, 39);
        entities.LegalAction.RespondingCountry =
          db.GetNullableString(reader, 40);
        entities.LegalAction.FiledDtEntredOn = db.GetNullableDate(reader, 41);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", import.LegalAction.Identifier);
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 5);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 7);
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
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
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CreatedBy = db.GetString(reader, 5);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionDetail.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 13);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 14);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 15);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 16);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 18);
        entities.LegalActionDetail.PeriodInd = db.GetNullableString(reader, 19);
        entities.LegalActionDetail.Description = db.GetString(reader, 20);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionPerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDt",
          import.LegalActionPerson.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDt",
          import.LegalActionPerson.EndDate.GetValueOrDefault());
        db.SetNullableString(
          command, "accountType", import.LegalActionPerson.AccountType ?? "");
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 5);
        entities.LegalActionPerson.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionPerson.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 9);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 10);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 13);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", import.LegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 5);
        entities.LegalActionPerson.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionPerson.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 9);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 10);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 13);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson3()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson3",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", local.LegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 5);
        entities.LegalActionPerson.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionPerson.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 9);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 10);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 13);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson4()
  {
    entities.OtherSupported.Populated = false;

    return Read("ReadLegalActionPerson4",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OtherSupported.Identifier = db.GetInt32(reader, 0);
        entities.OtherSupported.CspNumber = db.GetNullableString(reader, 1);
        entities.OtherSupported.EffectiveDate = db.GetDate(reader, 2);
        entities.OtherSupported.EndDate = db.GetNullableDate(reader, 3);
        entities.OtherSupported.LgaRIdentifier = db.GetNullableInt32(reader, 4);
        entities.OtherSupported.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.OtherSupported.AccountType = db.GetNullableString(reader, 6);
        entities.OtherSupported.Populated = true;
      });
  }

  private void UpdateCsePerson()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var paternityEstablishedIndicator = "Y";
    var datePaternEstab = entities.LegalAction.FiledDate;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(
          command, "patEstabInd", paternityEstablishedIndicator);
        db.SetDate(command, "datePaternEstab", datePaternEstab);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.PaternityEstablishedIndicator =
      paternityEstablishedIndicator;
    entities.CsePerson.DatePaternEstab = datePaternEstab;
    entities.CsePerson.Populated = true;
  }

  private void UpdateLegalActionDetail()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);

    var createdBy = "CONVERSX";

    entities.LegalActionDetail.Populated = false;
    Update("UpdateLegalActionDetail",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetInt32(command, "laDetailNo", entities.LegalActionDetail.Number);
      });

    entities.LegalActionDetail.CreatedBy = createdBy;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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

    private CaseRole caseRole;
    private Case1 case1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
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
    /// A value of ActiveApOnCase.
    /// </summary>
    [JsonPropertyName("activeApOnCase")]
    public Common ActiveApOnCase
    {
      get => activeApOnCase ??= new();
      set => activeApOnCase = value;
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
    /// A value of NoOfRetries.
    /// </summary>
    [JsonPropertyName("noOfRetries")]
    public Common NoOfRetries
    {
      get => noOfRetries ??= new();
      set => noOfRetries = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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

    private Common activeApOnCase;
    private DateWorkArea current;
    private Common noOfRetries;
    private CsePerson csePerson;
    private DateWorkArea zero;
    private DateWorkArea maxDate;
    private LegalActionPerson legalActionPerson;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OtherLegalAction.
    /// </summary>
    [JsonPropertyName("otherLegalAction")]
    public LegalAction OtherLegalAction
    {
      get => otherLegalAction ??= new();
      set => otherLegalAction = value;
    }

    /// <summary>
    /// A value of OtherLegalActionDetail.
    /// </summary>
    [JsonPropertyName("otherLegalActionDetail")]
    public LegalActionDetail OtherLegalActionDetail
    {
      get => otherLegalActionDetail ??= new();
      set => otherLegalActionDetail = value;
    }

    /// <summary>
    /// A value of OtherSupported.
    /// </summary>
    [JsonPropertyName("otherSupported")]
    public LegalActionPerson OtherSupported
    {
      get => otherSupported ??= new();
      set => otherSupported = value;
    }

    /// <summary>
    /// A value of ZdelNew.
    /// </summary>
    [JsonPropertyName("zdelNew")]
    public LegalActionPerson ZdelNew
    {
      get => zdelNew ??= new();
      set => zdelNew = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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

    private LegalAction otherLegalAction;
    private LegalActionDetail otherLegalActionDetail;
    private LegalActionPerson otherSupported;
    private LegalActionPerson zdelNew;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
  }
#endregion
}
