// Program: ADD_LEGAL_ACTION, ID: 371985168, model: 746.
// Short name: SWE00008
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
/// A program: ADD_LEGAL_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates LEGAL ACTION, and RELATED LEGAL ACTION.
/// </para>
/// </summary>
[Serializable]
public partial class AddLegalAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ADD_LEGAL_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new AddLegalAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of AddLegalAction.
  /// </summary>
  public AddLegalAction(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------------------
    // Date	 Developer	Request #	Description
    // 04/27/95 Dave Allen			Initial Code
    // 10/07/97 R Grey				Set Infrastructure CSE Person Number
    // 10/24/97 govind				Changed to use random number for Legal Action 
    // Identifier and Legal
    // 					Action Person Identifier
    // 03/05/98 RCG		H00024917/30734	Modify code which adds previous le act 
    // persons to new legal action.
    // 12/14/98  P. Sharp   	IDCR 338	Cleaned up action block per Phase II 
    // assessment. Added Init
    // 					Country and Resp Country to Legal Action per IDCR 338
    // 10/19/99 R. Jean	PR#H73506	Do not copy service provider assigned to 'N' 
    // classification legal
    // 					actions to subsequent legal actions.
    // 9/14/00 GVandy		PR#H103139	Modify how service provider assignments are 
    // carried forward.  When
    // 					adding a U class legal action only consider assignments to previous
    // 					U class legal actions.  When adding a non-U class legal action
    // 					only consider assignments to previous non-U class legal actions.
    // 10/24/00 GVandy		PR 105926	When carrying forward assignments, do not set 
    // the effective date on
    // 					the assignment to a date earlier than the current date.
    // 11/30/00 GVandy		PR 105706	When carrying forward roles, do not create 
    // legal_action_case_role if
    // 					it already exists.
    // 06/01/01 GVandy		PR 120164	Copy forward legal action assignments whose 
    // discontinue date is equal
    // 					to today (in addition to greater than today).
    // 09/19/01 GVandy		PR#s 122186/	Correct case_role_nf errors.  These result 
    // from attempting to copy
    // 			     126998	forward orphaned legal_action_case_roles (i.e. the case 
    // role that the
    // 					LACR points to no longer exists, apparently manually deleted during
    // 					data cleanup).  Per Evelyn, we should ignore these LACR records when
    // 					copying forward legal role information.
    // 11/01/02  GVandy	WR010492	Set new attribute system_gen_ind.
    // 05/30/03  GVandy	PR179593	Correct copy forward of non-case related legal 
    // roles.
    // -----------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    export.LegalAction.Assign(import.LegalAction);
    MoveLegalAction(import.LegalAction, local.LegalAction);
    export.Export1StLegActCourtCaseNo.Flag =
      import.Import1StLegActCourtCaseNo.Flag;

    // ------------------------------------------------------------
    // Read the Tribunal so the new Legal Action can be associated
    // to it.
    // ------------------------------------------------------------
    if (!ReadTribunal())
    {
      ExitState = "LE0000_TRIBUNAL_MUST_BE_SELECTED";

      return;
    }

    // -- Insure the system generated indicator is set to Y or spaces.
    switch(AsChar(local.LegalAction.SystemGenInd))
    {
      case 'Y':
        break;
      case ' ':
        break;
      default:
        local.LegalAction.SystemGenInd = "";

        break;
    }

    if (!IsEmpty(import.LegalAction.CourtCaseNumber))
    {
      // ---------------------------------------------------------------
      // Read for the most recently added non-N class legal action.  The legal 
      // action case roles will be copied from this legal action.
      // ---------------------------------------------------------------
      foreach(var item in ReadLegalAction1())
      {
        // ---------------------------------------------------------------
        // Order by Timestamp but save the identifier. The Identifier is
        // a random number.
        // ---------------------------------------------------------------
        // ---------------------------------------------------------------
        // 10/19/99	R. Jean
        // PR#H73506 - Bypass any class N legal actions, thus not allowing 
        // anything to be copied from an N legal action.
        // ---------------------------------------------------------------
        if (AsChar(entities.PreviousLegalAction.Classification) == 'N')
        {
          continue;
        }

        export.Export1StLegActCourtCaseNo.Flag = "N";

        break;
      }

      // ---------------------------------------------------------------
      // 9/14/00	GVandy
      // PR#H103139 -  Read for a previous legal action from which we will copy 
      // forward the service provider assignment.
      // When adding a U class legal action only consider assignments to 
      // previous U class legal actions.  When adding a non-U class legal action
      // only consider assignments to previous non-U class legal actions.
      // ---------------------------------------------------------------
      foreach(var item in ReadLegalAction2())
      {
        // ---------------------------------------------------------------
        // Order by Timestamp but save the identifier. The Identifier is
        // a random number.
        // ---------------------------------------------------------------
        if (AsChar(entities.PreviousAssignment.Classification) == 'N')
        {
          continue;
        }

        if (AsChar(import.LegalAction.Classification) == 'U')
        {
          // ---------------------------------------------------------------
          // When adding a U class legal action copy only the service provider 
          // assigned to a previous U class legal action.
          // ---------------------------------------------------------------
          if (AsChar(entities.PreviousAssignment.Classification) != 'U')
          {
            continue;
          }
        }
        else
        {
          // ---------------------------------------------------------------
          // When adding a non-U class legal action copy only the service 
          // provider assigned to a previous non-U class legal action.
          // ---------------------------------------------------------------
          if (AsChar(entities.PreviousAssignment.Classification) == 'U')
          {
            continue;
          }
        }

        break;
      }

      if (AsChar(export.Export1StLegActCourtCaseNo.Flag) == 'N')
      {
        if (export.LegalAction.ForeignFipsState.GetValueOrDefault() == 0)
        {
          local.LegalAction.ForeignFipsState =
            entities.PreviousLegalAction.ForeignFipsState;
        }

        if (export.LegalAction.ForeignFipsCounty.GetValueOrDefault() == 0)
        {
          local.LegalAction.ForeignFipsCounty =
            entities.PreviousLegalAction.ForeignFipsCounty;
        }

        if (export.LegalAction.ForeignFipsLocation.GetValueOrDefault() == 0)
        {
          local.LegalAction.ForeignFipsLocation =
            entities.PreviousLegalAction.ForeignFipsLocation;
        }

        if (IsEmpty(export.LegalAction.ForeignOrderNumber))
        {
          local.LegalAction.ForeignOrderNumber =
            entities.PreviousLegalAction.ForeignOrderNumber;
        }

        if (IsEmpty(export.LegalAction.NonCsePetitioner))
        {
          local.LegalAction.NonCsePetitioner =
            entities.PreviousLegalAction.NonCsePetitioner;
        }

        if (IsEmpty(export.LegalAction.StandardNumber))
        {
          local.LegalAction.StandardNumber =
            entities.PreviousLegalAction.StandardNumber;
        }
      }
    }

    for(local.NoOfRetries.Count = 1; local.NoOfRetries.Count <= 10; ++
      local.NoOfRetries.Count)
    {
      try
      {
        CreateLegalAction();
        export.LegalAction.Assign(entities.NewLegalAction);
        export.LegalAction.EndDate = null;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            continue;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!IsEmpty(import.AltBillingLocn.Number))
      {
        if (ReadCsePerson())
        {
          UseFnCabCheckAltAddr();

          if (IsExitState("FN0000_ALTERNATE_ADD_NF"))
          {
            return;
          }

          AssociateLegalAction();
        }
        else
        {
          ExitState = "LE0000_ALT_BILL_LOCN_CSE_PERS_NF";

          return;
        }
      }

      if (entities.PreviousAssignment.Populated)
      {
        // ---------------------------------------------
        // Carry forward the legal action assignments
        // ---------------------------------------------
        // ---------------------------------------------------------------
        // 10/19/99	R. Jean
        // PR#H73506 - Bypass copying assignments if adding a class N legal 
        // action.
        // ---------------------------------------------------------------
        if (AsChar(import.LegalAction.Classification) == 'N')
        {
        }
        else
        {
          // 06/01/01 GVandy	PR 120164 - Copy forward legal action assignments 
          // whose discontinue date is equal to today (in addition to greater
          // than today).
          foreach(var item in ReadLegalActionAssigmentOfficeServiceProvider())
          {
            // 10/24/00 GVandy
            // PR 105926 - When carrying forward assignments, do not set the 
            // effective date on the assignment to a date earlier than the
            // current date.
            if (Lt(entities.ExistingPrevLegalActionAssigment.EffectiveDate,
              local.Current.Date))
            {
              local.LegalActionAssigment.EffectiveDate = local.Current.Date;
            }
            else
            {
              local.LegalActionAssigment.EffectiveDate =
                entities.ExistingPrevLegalActionAssigment.EffectiveDate;
            }

            try
            {
              CreateLegalActionAssigment();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  // --- Cannot happen. Ignore the error
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
      }

      if (AsChar(export.Export1StLegActCourtCaseNo.Flag) == 'N')
      {
        // *********************************************
        // *             03-29-96     H. HOOKS         *
        // * If a previous Legal Action existed with   *
        // * the same Court Case Number as the newly   *
        // * created Legal Action, then tie the Case   *
        // * Roles of the previous Legal Action to the *
        // * newly created Legal Action.               *
        // *********************************************
        // ---------------------------------------------
        // Carry forward the legal action case roles
        // ---------------------------------------------
        foreach(var item in ReadLegalActionPersonCsePerson())
        {
          // -- 09/19/01 GVandy PR 122186/126998 Don't create the new 
          // legal_action_person until we know that the legal_action_case_role
          // record points to a valid case_role record.
          local.NewLegalPersonCreated.Flag = "N";

          if (ReadLaPersonLaCaseRole())
          {
            // -- Previous legal_action_person is case related.  Create a 
            // legal_action_person, legal_action_case_role, and
            // la_person_la_case_role record for the new legal action.
            foreach(var item1 in ReadLegalActionCaseRoleLaPersonLaCaseRole())
            {
              if (ReadCaseRoleCase())
              {
                if (AsChar(local.NewLegalPersonCreated.Flag) == 'N')
                {
                  UseLeLrolCreateLegActPersRec();

                  if (!ReadLegalActionPerson())
                  {
                    ExitState = "CO0000_LEGAL_ACTION_PERSON_NF";

                    return;
                  }

                  local.NewLegalPersonCreated.Flag = "Y";
                }

                if (!ReadLegalActionCaseRole())
                {
                  try
                  {
                    CreateLegalActionCaseRole();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "LE0000_LEGAL_ACTION_CASE_ROLE_AE";

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
              else
              {
                // -- 09/19/01 GVandy PR 122186/126998 Ignore this error.  There
                // are legal_action_case_role records that point to case_roles
                // that no longer exist, apparently the result of manual data
                // cleanup.  The code has been restructured so that
                // legal_action_person, legal_action_case_role, and
                // la_person_la_case_role records are not created when this
                // occurs.  The orphaned records are simply ignored.
              }
            }
          }
          else
          {
            // -- Previous legal_action_person is NOT case related.  Create  
            // alegal_action_person for the new legal action.
            UseLeLrolCreateLegActPersRec();
          }
        }
      }

      // --- Legal action and related records successfully added
      return;
    }

    ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.ForeignFipsState = source.ForeignFipsState;
    target.ForeignFipsCounty = source.ForeignFipsCounty;
    target.ForeignFipsLocation = source.ForeignFipsLocation;
    target.ForeignOrderNumber = source.ForeignOrderNumber;
    target.Identifier = source.Identifier;
    target.LastModificationReviewDate = source.LastModificationReviewDate;
    target.AttorneyApproval = source.AttorneyApproval;
    target.ApprovalSentDate = source.ApprovalSentDate;
    target.PetitionerApproval = source.PetitionerApproval;
    target.ApprovalReceivedDate = source.ApprovalReceivedDate;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.Type1 = source.Type1;
    target.FiledDate = source.FiledDate;
    target.EndDate = source.EndDate;
    target.ForeignOrderRegistrationDate = source.ForeignOrderRegistrationDate;
    target.UresaSentDate = source.UresaSentDate;
    target.UresaAcknowledgedDate = source.UresaAcknowledgedDate;
    target.UifsaSentDate = source.UifsaSentDate;
    target.UifsaAcknowledgedDate = source.UifsaAcknowledgedDate;
    target.InitiatingState = source.InitiatingState;
    target.InitiatingCounty = source.InitiatingCounty;
    target.RespondingState = source.RespondingState;
    target.RespondingCounty = source.RespondingCounty;
    target.OrderAuthority = source.OrderAuthority;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
    target.LongArmStatuteIndicator = source.LongArmStatuteIndicator;
    target.PaymentLocation = source.PaymentLocation;
    target.EstablishmentCode = source.EstablishmentCode;
    target.DismissedWithoutPrejudiceInd = source.DismissedWithoutPrejudiceInd;
    target.DismissalCode = source.DismissalCode;
    target.RefileDate = source.RefileDate;
    target.CreatedTstamp = source.CreatedTstamp;
    target.NonCsePetitioner = source.NonCsePetitioner;
    target.DateCpReqIwoBegin = source.DateCpReqIwoBegin;
    target.DateNonCpReqIwoBegin = source.DateNonCpReqIwoBegin;
    target.CtOrdAltBillingAddrInd = source.CtOrdAltBillingAddrInd;
    target.InitiatingCountry = source.InitiatingCountry;
    target.RespondingCountry = source.RespondingCountry;
    target.SystemGenInd = source.SystemGenInd;
  }

  private static void MoveLegalActionPerson(LegalActionPerson source,
    LegalActionPerson target)
  {
    target.Identifier = source.Identifier;
    target.Role = source.Role;
    target.EffectiveDate = source.EffectiveDate;
    target.EndDate = source.EndDate;
    target.EndReason = source.EndReason;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCabCheckAltAddr()
  {
    var useImport = new FnCabCheckAltAddr.Import();
    var useExport = new FnCabCheckAltAddr.Export();

    useImport.Alternate.Number = import.AltBillingLocn.Number;
    useImport.Current.Date = local.Current.Date;

    Call(FnCabCheckAltAddr.Execute, useImport, useExport);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void UseLeLrolCreateLegActPersRec()
  {
    var useImport = new LeLrolCreateLegActPersRec.Import();
    var useExport = new LeLrolCreateLegActPersRec.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.LegalActionPerson.Assign(entities.PreviousLegalActionPerson);
    useImport.LegalAction.Identifier = entities.NewLegalAction.Identifier;

    Call(LeLrolCreateLegActPersRec.Execute, useImport, useExport);

    MoveLegalActionPerson(useExport.LegalActionPerson, local.NewlyCreated);
  }

  private void AssociateLegalAction()
  {
    var cspNumber = entities.NewAltBillingLocn.Number;

    entities.NewLegalAction.Populated = false;
    Update("AssociateLegalAction",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.
          SetInt32(command, "legalActionId", entities.NewLegalAction.Identifier);
          
      });

    entities.NewLegalAction.CspNumber = cspNumber;
    entities.NewLegalAction.Populated = true;
  }

  private void CreateLaPersonLaCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.NewLegalActionCaseRole.Populated);

    var identifier = 1;
    var croId = entities.NewLegalActionCaseRole.CroIdentifier;
    var croType = entities.NewLegalActionCaseRole.CroType;
    var cspNum = entities.NewLegalActionCaseRole.CspNumber;
    var casNum = entities.NewLegalActionCaseRole.CasNumber;
    var lgaId = entities.NewLegalActionCaseRole.LgaId;
    var lapId = entities.NewlyCreated.Identifier;

    CheckValid<LaPersonLaCaseRole>("CroType", croType);
    entities.NewLaPersonLaCaseRole.Populated = false;
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

    entities.NewLaPersonLaCaseRole.Identifier = identifier;
    entities.NewLaPersonLaCaseRole.CroId = croId;
    entities.NewLaPersonLaCaseRole.CroType = croType;
    entities.NewLaPersonLaCaseRole.CspNum = cspNum;
    entities.NewLaPersonLaCaseRole.CasNum = casNum;
    entities.NewLaPersonLaCaseRole.LgaId = lgaId;
    entities.NewLaPersonLaCaseRole.LapId = lapId;
    entities.NewLaPersonLaCaseRole.Populated = true;
  }

  private void CreateLegalAction()
  {
    var identifier = UseGenerate9DigitRandomNumber();
    var lastModificationReviewDate =
      import.LegalAction.LastModificationReviewDate;
    var attorneyApproval = import.LegalAction.AttorneyApproval ?? "";
    var approvalSentDate = import.LegalAction.ApprovalSentDate;
    var petitionerApproval = import.LegalAction.PetitionerApproval ?? "";
    var approvalReceivedDate = import.LegalAction.ApprovalReceivedDate;
    var classification = import.LegalAction.Classification;
    var actionTaken = import.LegalAction.ActionTaken;
    var type1 = import.LegalAction.Type1;
    var filedDate = import.LegalAction.FiledDate;
    var foreignOrderRegistrationDate =
      import.LegalAction.ForeignOrderRegistrationDate;
    var uresaSentDate = import.LegalAction.UresaSentDate;
    var uresaAcknowledgedDate = import.LegalAction.UresaAcknowledgedDate;
    var uifsaSentDate = import.LegalAction.UifsaSentDate;
    var uifsaAcknowledgedDate = import.LegalAction.UifsaAcknowledgedDate;
    var initiatingState = import.LegalAction.InitiatingState ?? "";
    var initiatingCounty = import.LegalAction.InitiatingCounty ?? "";
    var respondingState = import.LegalAction.RespondingState ?? "";
    var respondingCounty = import.LegalAction.RespondingCounty ?? "";
    var orderAuthority = import.LegalAction.OrderAuthority;
    var courtCaseNumber = import.LegalAction.CourtCaseNumber ?? "";
    var refileDate = import.LegalAction.RefileDate;
    var endDate = UseCabSetMaximumDiscontinueDate();
    var paymentLocation = import.LegalAction.PaymentLocation ?? "";
    var dismissedWithoutPrejudiceInd =
      import.LegalAction.DismissedWithoutPrejudiceInd ?? "";
    var standardNumber = local.LegalAction.StandardNumber ?? "";
    var longArmStatuteIndicator =
      import.LegalAction.LongArmStatuteIndicator ?? "";
    var dismissalCode = import.LegalAction.DismissalCode ?? "";
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var establishmentCode = import.LegalAction.EstablishmentCode ?? "";
    var foreignFipsState =
      local.LegalAction.ForeignFipsState.GetValueOrDefault();
    var foreignFipsCounty =
      local.LegalAction.ForeignFipsCounty.GetValueOrDefault();
    var foreignFipsLocation =
      local.LegalAction.ForeignFipsLocation.GetValueOrDefault();
    var foreignOrderNumber = local.LegalAction.ForeignOrderNumber ?? "";
    var trbId = entities.Tribunal.Identifier;
    var nonCsePetitioner = local.LegalAction.NonCsePetitioner ?? "";
    var dateNonCpReqIwoBegin = import.LegalAction.DateNonCpReqIwoBegin;
    var dateCpReqIwoBegin = import.LegalAction.DateCpReqIwoBegin;
    var ctOrdAltBillingAddrInd = import.LegalAction.CtOrdAltBillingAddrInd ?? ""
      ;
    var initiatingCountry = import.LegalAction.InitiatingCountry ?? "";
    var respondingCountry = import.LegalAction.RespondingCountry ?? "";
    var systemGenInd = local.LegalAction.SystemGenInd ?? "";

    entities.NewLegalAction.Populated = false;
    Update("CreateLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", identifier);
        db.SetNullableDate(
          command, "lastModReviewDt", lastModificationReviewDate);
        db.SetNullableString(command, "attorneyApproval", attorneyApproval);
        db.SetNullableDate(command, "approvalSentDt", approvalSentDate);
        db.SetNullableString(command, "petitionerApprval", petitionerApproval);
        db.SetNullableDate(command, "approvalRecdDt", approvalReceivedDate);
        db.SetString(command, "classification", classification);
        db.SetString(command, "actionTaken", actionTaken);
        db.SetString(command, "type", type1);
        db.SetNullableDate(command, "filedDt", filedDate);
        db.SetNullableDate(
          command, "foreignOrdRegDt", foreignOrderRegistrationDate);
        db.SetNullableDate(command, "uresaSentDt", uresaSentDate);
        db.SetNullableDate(command, "uresaAcknowldgDt", uresaAcknowledgedDate);
        db.SetNullableDate(command, "uifsaSentDt", uifsaSentDate);
        db.SetNullableDate(command, "uifsaAcknowldgDt", uifsaAcknowledgedDate);
        db.SetNullableString(command, "initiatingState", initiatingState);
        db.SetNullableString(command, "initiatingCounty", initiatingCounty);
        db.SetNullableString(command, "respondingState", respondingState);
        db.SetNullableString(command, "respondingCounty", respondingCounty);
        db.SetString(command, "orderAuthority", orderAuthority);
        db.SetNullableString(command, "courtCaseNo", courtCaseNumber);
        db.SetNullableDate(command, "refileDt", refileDate);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetNullableString(command, "paymentLocation", paymentLocation);
        db.SetNullableString(
          command, "dismissedInd", dismissedWithoutPrejudiceInd);
        db.SetNullableString(command, "standardNo", standardNumber);
        db.
          SetNullableString(command, "longArmStatInd", longArmStatuteIndicator);
          
        db.SetNullableString(command, "dismissalCd", dismissalCode);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetNullableString(command, "establishmentCd", establishmentCode);
        db.SetNullableInt32(command, "foreignFipsSt", foreignFipsState);
        db.SetNullableInt32(command, "foreignFipsCount", foreignFipsCounty);
        db.SetNullableInt32(command, "foreignFipsLo", foreignFipsLocation);
        db.SetNullableString(command, "foreignOrderNo", foreignOrderNumber);
        db.SetNullableInt32(command, "trbId", trbId);
        db.SetNullableString(command, "nonCsePetitioner", nonCsePetitioner);
        db.SetNullableDate(command, "dtNcpReqIwoBgn", dateNonCpReqIwoBegin);
        db.SetNullableDate(command, "dtCpReqIwoBgn", dateCpReqIwoBegin);
        db.SetNullableString(command, "ctOrdAltBaInd", ctOrdAltBillingAddrInd);
        db.SetNullableString(command, "initiatingCountry", initiatingCountry);
        db.SetNullableString(command, "respondingCountry", respondingCountry);
        db.SetNullableString(command, "kpcStandardNo", "");
        db.SetNullableString(command, "kpcFlag", "");
        db.SetNullableDate(command, "kpcDate", default(DateTime));
        db.SetNullableString(command, "systemGenInd", systemGenInd);
        db.SetNullableInt32(command, "kpcTribunalId", 0);
      });

    entities.NewLegalAction.Identifier = identifier;
    entities.NewLegalAction.LastModificationReviewDate =
      lastModificationReviewDate;
    entities.NewLegalAction.AttorneyApproval = attorneyApproval;
    entities.NewLegalAction.ApprovalSentDate = approvalSentDate;
    entities.NewLegalAction.PetitionerApproval = petitionerApproval;
    entities.NewLegalAction.ApprovalReceivedDate = approvalReceivedDate;
    entities.NewLegalAction.Classification = classification;
    entities.NewLegalAction.ActionTaken = actionTaken;
    entities.NewLegalAction.Type1 = type1;
    entities.NewLegalAction.FiledDate = filedDate;
    entities.NewLegalAction.ForeignOrderRegistrationDate =
      foreignOrderRegistrationDate;
    entities.NewLegalAction.UresaSentDate = uresaSentDate;
    entities.NewLegalAction.UresaAcknowledgedDate = uresaAcknowledgedDate;
    entities.NewLegalAction.UifsaSentDate = uifsaSentDate;
    entities.NewLegalAction.UifsaAcknowledgedDate = uifsaAcknowledgedDate;
    entities.NewLegalAction.InitiatingState = initiatingState;
    entities.NewLegalAction.InitiatingCounty = initiatingCounty;
    entities.NewLegalAction.RespondingState = respondingState;
    entities.NewLegalAction.RespondingCounty = respondingCounty;
    entities.NewLegalAction.OrderAuthority = orderAuthority;
    entities.NewLegalAction.CourtCaseNumber = courtCaseNumber;
    entities.NewLegalAction.RefileDate = refileDate;
    entities.NewLegalAction.EndDate = endDate;
    entities.NewLegalAction.PaymentLocation = paymentLocation;
    entities.NewLegalAction.DismissedWithoutPrejudiceInd =
      dismissedWithoutPrejudiceInd;
    entities.NewLegalAction.StandardNumber = standardNumber;
    entities.NewLegalAction.LongArmStatuteIndicator = longArmStatuteIndicator;
    entities.NewLegalAction.DismissalCode = dismissalCode;
    entities.NewLegalAction.CreatedBy = createdBy;
    entities.NewLegalAction.CreatedTstamp = createdTstamp;
    entities.NewLegalAction.EstablishmentCode = establishmentCode;
    entities.NewLegalAction.ForeignFipsState = foreignFipsState;
    entities.NewLegalAction.ForeignFipsCounty = foreignFipsCounty;
    entities.NewLegalAction.ForeignFipsLocation = foreignFipsLocation;
    entities.NewLegalAction.ForeignOrderNumber = foreignOrderNumber;
    entities.NewLegalAction.TrbId = trbId;
    entities.NewLegalAction.NonCsePetitioner = nonCsePetitioner;
    entities.NewLegalAction.DateNonCpReqIwoBegin = dateNonCpReqIwoBegin;
    entities.NewLegalAction.DateCpReqIwoBegin = dateCpReqIwoBegin;
    entities.NewLegalAction.CtOrdAltBillingAddrInd = ctOrdAltBillingAddrInd;
    entities.NewLegalAction.CspNumber = null;
    entities.NewLegalAction.InitiatingCountry = initiatingCountry;
    entities.NewLegalAction.RespondingCountry = respondingCountry;
    entities.NewLegalAction.SystemGenInd = systemGenInd;
    entities.NewLegalAction.Populated = true;
  }

  private void CreateLegalActionAssigment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevOfficeServiceProvider.Populated);

    var lgaIdentifier = entities.NewLegalAction.Identifier;
    var ospEffectiveDate =
      entities.ExistingPrevOfficeServiceProvider.EffectiveDate;
    var ospRoleCode = entities.ExistingPrevOfficeServiceProvider.RoleCode;
    var offGeneratedId =
      entities.ExistingPrevOfficeServiceProvider.OffGeneratedId;
    var spdGeneratedId =
      entities.ExistingPrevOfficeServiceProvider.SpdGeneratedId;
    var effectiveDate = local.LegalActionAssigment.EffectiveDate;
    var discontinueDate =
      entities.ExistingPrevLegalActionAssigment.DiscontinueDate;
    var reasonCode = entities.ExistingPrevLegalActionAssigment.ReasonCode;
    var overrideInd = entities.ExistingPrevLegalActionAssigment.OverrideInd;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.NewLegalActionAssigment.Populated = false;
    Update("CreateLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableDate(command, "endDt", discontinueDate);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
      });

    entities.NewLegalActionAssigment.LgaIdentifier = lgaIdentifier;
    entities.NewLegalActionAssigment.OspEffectiveDate = ospEffectiveDate;
    entities.NewLegalActionAssigment.OspRoleCode = ospRoleCode;
    entities.NewLegalActionAssigment.OffGeneratedId = offGeneratedId;
    entities.NewLegalActionAssigment.SpdGeneratedId = spdGeneratedId;
    entities.NewLegalActionAssigment.EffectiveDate = effectiveDate;
    entities.NewLegalActionAssigment.DiscontinueDate = discontinueDate;
    entities.NewLegalActionAssigment.ReasonCode = reasonCode;
    entities.NewLegalActionAssigment.OverrideInd = overrideInd;
    entities.NewLegalActionAssigment.CreatedBy = createdBy;
    entities.NewLegalActionAssigment.CreatedTimestamp = createdTimestamp;
    entities.NewLegalActionAssigment.Populated = true;
  }

  private void CreateLegalActionCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CopyThisCaseRole.Populated);

    var casNumber = entities.CopyThisCaseRole.CasNumber;
    var cspNumber = entities.CopyThisCaseRole.CspNumber;
    var croType = entities.CopyThisCaseRole.Type1;
    var croIdentifier = entities.CopyThisCaseRole.Identifier;
    var lgaId = entities.NewLegalAction.Identifier;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var initialCreationInd = "Y";

    CheckValid<LegalActionCaseRole>("CroType", croType);
    entities.NewLegalActionCaseRole.Populated = false;
    Update("CreateLegalActionCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "croIdentifier", croIdentifier);
        db.SetInt32(command, "lgaId", lgaId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "initCrInd", initialCreationInd);
      });

    entities.NewLegalActionCaseRole.CasNumber = casNumber;
    entities.NewLegalActionCaseRole.CspNumber = cspNumber;
    entities.NewLegalActionCaseRole.CroType = croType;
    entities.NewLegalActionCaseRole.CroIdentifier = croIdentifier;
    entities.NewLegalActionCaseRole.LgaId = lgaId;
    entities.NewLegalActionCaseRole.CreatedBy = createdBy;
    entities.NewLegalActionCaseRole.CreatedTstamp = createdTstamp;
    entities.NewLegalActionCaseRole.InitialCreationInd = initialCreationInd;
    entities.NewLegalActionCaseRole.Populated = true;
  }

  private bool ReadCaseRoleCase()
  {
    System.Diagnostics.Debug.Assert(
      entities.PreviousLegalActionCaseRole.Populated);
    entities.CopyThisCase.Populated = false;
    entities.CopyThisCaseRole.Populated = false;

    return Read("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetInt32(
          command, "caseRoleId",
          entities.PreviousLegalActionCaseRole.CroIdentifier);
        db.SetString(
          command, "type", entities.PreviousLegalActionCaseRole.CroType);
        db.SetString(
          command, "cspNumber", entities.PreviousLegalActionCaseRole.CspNumber);
          
        db.SetString(
          command, "casNumber", entities.PreviousLegalActionCaseRole.CasNumber);
          
      },
      (db, reader) =>
      {
        entities.CopyThisCaseRole.CasNumber = db.GetString(reader, 0);
        entities.CopyThisCase.Number = db.GetString(reader, 0);
        entities.CopyThisCaseRole.CspNumber = db.GetString(reader, 1);
        entities.CopyThisCaseRole.Type1 = db.GetString(reader, 2);
        entities.CopyThisCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CopyThisCase.Populated = true;
        entities.CopyThisCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CopyThisCaseRole.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.NewAltBillingLocn.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.AltBillingLocn.Number);
      },
      (db, reader) =>
      {
        entities.NewAltBillingLocn.Number = db.GetString(reader, 0);
        entities.NewAltBillingLocn.Populated = true;
      });
  }

  private bool ReadLaPersonLaCaseRole()
  {
    entities.PreviousLaPersonLaCaseRole.Populated = false;

    return Read("ReadLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetInt32(
          command, "lapId", entities.PreviousLegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.PreviousLaPersonLaCaseRole.Identifier = db.GetInt32(reader, 0);
        entities.PreviousLaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.PreviousLaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.PreviousLaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.PreviousLaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.PreviousLaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.PreviousLaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.PreviousLaPersonLaCaseRole.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.PreviousLaPersonLaCaseRole.CroType);
      });
  }

  private IEnumerable<bool> ReadLegalAction1()
  {
    entities.PreviousLegalAction.Populated = false;

    return ReadEach("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.PreviousLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.PreviousLegalAction.Classification = db.GetString(reader, 1);
        entities.PreviousLegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.PreviousLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.PreviousLegalAction.StandardNumber =
          db.GetNullableString(reader, 4);
        entities.PreviousLegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.PreviousLegalAction.ForeignFipsState =
          db.GetNullableInt32(reader, 6);
        entities.PreviousLegalAction.ForeignFipsCounty =
          db.GetNullableInt32(reader, 7);
        entities.PreviousLegalAction.ForeignFipsLocation =
          db.GetNullableInt32(reader, 8);
        entities.PreviousLegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 9);
        entities.PreviousLegalAction.TrbId = db.GetNullableInt32(reader, 10);
        entities.PreviousLegalAction.NonCsePetitioner =
          db.GetNullableString(reader, 11);
        entities.PreviousLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.PreviousAssignment.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.PreviousAssignment.Identifier = db.GetInt32(reader, 0);
        entities.PreviousAssignment.Classification = db.GetString(reader, 1);
        entities.PreviousAssignment.FiledDate = db.GetNullableDate(reader, 2);
        entities.PreviousAssignment.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.PreviousAssignment.StandardNumber =
          db.GetNullableString(reader, 4);
        entities.PreviousAssignment.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.PreviousAssignment.ForeignFipsState =
          db.GetNullableInt32(reader, 6);
        entities.PreviousAssignment.ForeignFipsCounty =
          db.GetNullableInt32(reader, 7);
        entities.PreviousAssignment.ForeignFipsLocation =
          db.GetNullableInt32(reader, 8);
        entities.PreviousAssignment.ForeignOrderNumber =
          db.GetNullableString(reader, 9);
        entities.PreviousAssignment.TrbId = db.GetNullableInt32(reader, 10);
        entities.PreviousAssignment.NonCsePetitioner =
          db.GetNullableString(reader, 11);
        entities.PreviousAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionAssigmentOfficeServiceProvider()
  {
    entities.ExistingPrevOfficeServiceProvider.Populated = false;
    entities.ExistingPrevLegalActionAssigment.Populated = false;

    return ReadEach("ReadLegalActionAssigmentOfficeServiceProvider",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.PreviousAssignment.Identifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevLegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingPrevLegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingPrevOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingPrevLegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.ExistingPrevOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingPrevLegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.ExistingPrevOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingPrevLegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingPrevOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 4);
        entities.ExistingPrevLegalActionAssigment.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ExistingPrevLegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingPrevLegalActionAssigment.ReasonCode =
          db.GetString(reader, 7);
        entities.ExistingPrevLegalActionAssigment.OverrideInd =
          db.GetString(reader, 8);
        entities.ExistingPrevLegalActionAssigment.CreatedBy =
          db.GetString(reader, 9);
        entities.ExistingPrevLegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.ExistingPrevLegalActionAssigment.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.ExistingPrevLegalActionAssigment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.ExistingPrevOfficeServiceProvider.Populated = true;
        entities.ExistingPrevLegalActionAssigment.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CopyThisCaseRole.Populated);
    entities.NewLegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.NewLegalAction.Identifier);
        db.SetInt32(
          command, "croIdentifier", entities.CopyThisCaseRole.Identifier);
        db.SetString(command, "croType", entities.CopyThisCaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CopyThisCaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CopyThisCaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.NewLegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.NewLegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.NewLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.NewLegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.NewLegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.NewLegalActionCaseRole.CreatedBy = db.GetString(reader, 5);
        entities.NewLegalActionCaseRole.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.NewLegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 7);
        entities.NewLegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.NewLegalActionCaseRole.CroType);
      });
  }

  private IEnumerable<bool> ReadLegalActionCaseRoleLaPersonLaCaseRole()
  {
    entities.PreviousLaPersonLaCaseRole.Populated = false;
    entities.PreviousLegalActionCaseRole.Populated = false;

    return ReadEach("ReadLegalActionCaseRoleLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetInt32(
          command, "lapId", entities.PreviousLegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.PreviousLegalActionCaseRole.CasNumber =
          db.GetString(reader, 0);
        entities.PreviousLaPersonLaCaseRole.CasNum = db.GetString(reader, 0);
        entities.PreviousLegalActionCaseRole.CspNumber =
          db.GetString(reader, 1);
        entities.PreviousLaPersonLaCaseRole.CspNum = db.GetString(reader, 1);
        entities.PreviousLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.PreviousLaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.PreviousLegalActionCaseRole.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.PreviousLaPersonLaCaseRole.CroId = db.GetInt32(reader, 3);
        entities.PreviousLegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.PreviousLaPersonLaCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.PreviousLegalActionCaseRole.CreatedBy =
          db.GetString(reader, 5);
        entities.PreviousLegalActionCaseRole.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.PreviousLaPersonLaCaseRole.Identifier = db.GetInt32(reader, 7);
        entities.PreviousLaPersonLaCaseRole.LapId = db.GetInt32(reader, 8);
        entities.PreviousLaPersonLaCaseRole.Populated = true;
        entities.PreviousLegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.PreviousLegalActionCaseRole.CroType);
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.PreviousLaPersonLaCaseRole.CroType);

        return true;
      });
  }

  private bool ReadLegalActionPerson()
  {
    entities.NewlyCreated.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", local.NewlyCreated.Identifier);
      },
      (db, reader) =>
      {
        entities.NewlyCreated.Identifier = db.GetInt32(reader, 0);
        entities.NewlyCreated.EffectiveDate = db.GetDate(reader, 1);
        entities.NewlyCreated.Role = db.GetString(reader, 2);
        entities.NewlyCreated.EndDate = db.GetNullableDate(reader, 3);
        entities.NewlyCreated.EndReason = db.GetNullableString(reader, 4);
        entities.NewlyCreated.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.NewlyCreated.CreatedBy = db.GetString(reader, 6);
        entities.NewlyCreated.AccountType = db.GetNullableString(reader, 7);
        entities.NewlyCreated.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.PreviousLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.PreviousLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.PreviousLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.PreviousLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.PreviousLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.PreviousLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 3);
        entities.PreviousLegalActionPerson.Role = db.GetString(reader, 4);
        entities.PreviousLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 5);
        entities.PreviousLegalActionPerson.EndReason =
          db.GetNullableString(reader, 6);
        entities.PreviousLegalActionPerson.CreatedTstamp =
          db.GetDateTime(reader, 7);
        entities.PreviousLegalActionPerson.CreatedBy = db.GetString(reader, 8);
        entities.PreviousLegalActionPerson.AccountType =
          db.GetNullableString(reader, 9);
        entities.CsePerson.Populated = true;
        entities.PreviousLegalActionPerson.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 2);
        entities.Tribunal.Identifier = db.GetInt32(reader, 3);
        entities.Tribunal.Populated = true;
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
    /// A value of AltBillingLocn.
    /// </summary>
    [JsonPropertyName("altBillingLocn")]
    public CsePersonsWorkSet AltBillingLocn
    {
      get => altBillingLocn ??= new();
      set => altBillingLocn = value;
    }

    /// <summary>
    /// A value of Import1StLegActCourtCaseNo.
    /// </summary>
    [JsonPropertyName("import1StLegActCourtCaseNo")]
    public Common Import1StLegActCourtCaseNo
    {
      get => import1StLegActCourtCaseNo ??= new();
      set => import1StLegActCourtCaseNo = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private CsePersonsWorkSet altBillingLocn;
    private Common import1StLegActCourtCaseNo;
    private Tribunal tribunal;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Export1StLegActCourtCaseNo.
    /// </summary>
    [JsonPropertyName("export1StLegActCourtCaseNo")]
    public Common Export1StLegActCourtCaseNo
    {
      get => export1StLegActCourtCaseNo ??= new();
      set => export1StLegActCourtCaseNo = value;
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

    private Common export1StLegActCourtCaseNo;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NewLegalPersonCreated.
    /// </summary>
    [JsonPropertyName("newLegalPersonCreated")]
    public Common NewLegalPersonCreated
    {
      get => newLegalPersonCreated ??= new();
      set => newLegalPersonCreated = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of NewlyCreated.
    /// </summary>
    [JsonPropertyName("newlyCreated")]
    public LegalActionPerson NewlyCreated
    {
      get => newlyCreated ??= new();
      set => newlyCreated = value;
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
    /// A value of NoOfRetries.
    /// </summary>
    [JsonPropertyName("noOfRetries")]
    public Common NoOfRetries
    {
      get => noOfRetries ??= new();
      set => noOfRetries = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    private Common newLegalPersonCreated;
    private LegalActionAssigment legalActionAssigment;
    private LegalAction legalAction;
    private LegalActionPerson newlyCreated;
    private LegalActionPerson legalActionPerson;
    private Common noOfRetries;
    private DateWorkArea current;
    private DateWorkArea nullDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PreviousAssignment.
    /// </summary>
    [JsonPropertyName("previousAssignment")]
    public LegalAction PreviousAssignment
    {
      get => previousAssignment ??= new();
      set => previousAssignment = value;
    }

    /// <summary>
    /// A value of CopyThisCase.
    /// </summary>
    [JsonPropertyName("copyThisCase")]
    public Case1 CopyThisCase
    {
      get => copyThisCase ??= new();
      set => copyThisCase = value;
    }

    /// <summary>
    /// A value of NewAltBillingLocn.
    /// </summary>
    [JsonPropertyName("newAltBillingLocn")]
    public CsePerson NewAltBillingLocn
    {
      get => newAltBillingLocn ??= new();
      set => newAltBillingLocn = value;
    }

    /// <summary>
    /// A value of NewLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("newLegalActionAssigment")]
    public LegalActionAssigment NewLegalActionAssigment
    {
      get => newLegalActionAssigment ??= new();
      set => newLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of ExistingPrevOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingPrevOfficeServiceProvider")]
    public OfficeServiceProvider ExistingPrevOfficeServiceProvider
    {
      get => existingPrevOfficeServiceProvider ??= new();
      set => existingPrevOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingPrevLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("existingPrevLegalActionAssigment")]
    public LegalActionAssigment ExistingPrevLegalActionAssigment
    {
      get => existingPrevLegalActionAssigment ??= new();
      set => existingPrevLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of NewLaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("newLaPersonLaCaseRole")]
    public LaPersonLaCaseRole NewLaPersonLaCaseRole
    {
      get => newLaPersonLaCaseRole ??= new();
      set => newLaPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of PreviousLaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("previousLaPersonLaCaseRole")]
    public LaPersonLaCaseRole PreviousLaPersonLaCaseRole
    {
      get => previousLaPersonLaCaseRole ??= new();
      set => previousLaPersonLaCaseRole = value;
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
    /// A value of NewlyCreated.
    /// </summary>
    [JsonPropertyName("newlyCreated")]
    public LegalActionPerson NewlyCreated
    {
      get => newlyCreated ??= new();
      set => newlyCreated = value;
    }

    /// <summary>
    /// A value of PreviousLegalActionPerson.
    /// </summary>
    [JsonPropertyName("previousLegalActionPerson")]
    public LegalActionPerson PreviousLegalActionPerson
    {
      get => previousLegalActionPerson ??= new();
      set => previousLegalActionPerson = value;
    }

    /// <summary>
    /// A value of NewLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("newLegalActionCaseRole")]
    public LegalActionCaseRole NewLegalActionCaseRole
    {
      get => newLegalActionCaseRole ??= new();
      set => newLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of CopyThisCaseRole.
    /// </summary>
    [JsonPropertyName("copyThisCaseRole")]
    public CaseRole CopyThisCaseRole
    {
      get => copyThisCaseRole ??= new();
      set => copyThisCaseRole = value;
    }

    /// <summary>
    /// A value of PreviousLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("previousLegalActionCaseRole")]
    public LegalActionCaseRole PreviousLegalActionCaseRole
    {
      get => previousLegalActionCaseRole ??= new();
      set => previousLegalActionCaseRole = value;
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
    /// A value of NewLegalAction.
    /// </summary>
    [JsonPropertyName("newLegalAction")]
    public LegalAction NewLegalAction
    {
      get => newLegalAction ??= new();
      set => newLegalAction = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
    }

    private LegalAction previousAssignment;
    private Case1 copyThisCase;
    private CsePerson newAltBillingLocn;
    private LegalActionAssigment newLegalActionAssigment;
    private OfficeServiceProvider existingPrevOfficeServiceProvider;
    private LegalActionAssigment existingPrevLegalActionAssigment;
    private LaPersonLaCaseRole newLaPersonLaCaseRole;
    private LaPersonLaCaseRole previousLaPersonLaCaseRole;
    private CsePerson csePerson;
    private LegalActionPerson newlyCreated;
    private LegalActionPerson previousLegalActionPerson;
    private LegalActionCaseRole newLegalActionCaseRole;
    private CaseRole copyThisCaseRole;
    private LegalActionCaseRole previousLegalActionCaseRole;
    private Tribunal tribunal;
    private LegalAction newLegalAction;
    private LegalAction previousLegalAction;
  }
#endregion
}
