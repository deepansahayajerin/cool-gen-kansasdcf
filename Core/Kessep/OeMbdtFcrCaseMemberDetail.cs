// Program: OE_MBDT_FCR_CASE_MEMBER_DETAIL, ID: 374574555, model: 746.
// Short name: SWEMBDTP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_MBDT_FCR_CASE_MEMBER_DETAIL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeMbdtFcrCaseMemberDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MBDT_FCR_CASE_MEMBER_DETAIL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMbdtFcrCaseMemberDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMbdtFcrCaseMemberDetail.
  /// </summary>
  public OeMbdtFcrCaseMemberDetail(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    // Date		Developer	Request #      Description
    // -------------------------------------------------------------------------------------
    // 09/08/2009	M Fan		CQ7190	       Initial Dev
    // 02/18/2010      Raj S           CQ7190         Added code to give access 
    // to the data
    //                                                
    // if the logged in user is a supervisor
    //                                                
    // to th selected case service provider.
    // -------------------------------------------------------------------------------------
    // *********************************************************************************
    // Process all the Exit and Return commands before commencing the Process 
    // Logic
    // *********************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_XFR_TO_ALOM_MENU";

      return;
    }

    // *********************************************************************************
    // When user issues a CLEAR command, preservce only the next tran info and 
    // go back to
    // screen to clear all screen values.
    // *********************************************************************************
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN_NM";

      return;
    }

    // *********************************************************************************
    // Move Import views to export views
    // *********************************************************************************
    export.FcrCaseMaster.CaseId = import.FcrCaseMaster.CaseId;

    if (!IsEmpty(export.FcrCaseMaster.CaseId))
    {
      local.ZeroFill.Number = export.FcrCaseMaster.CaseId;
      UseCabZeroFillNumber2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.FcrCaseMaster, "caseId");

        field.Error = true;

        ExitState = "CASE_NUMBER_NOT_NUMERIC";

        return;
      }
      else
      {
        export.FcrCaseMaster.CaseId = local.ZeroFill.Number;
      }
    }

    export.HiddnStartPrevCaseId.CaseId = import.HiddnStartPrevCaseId.CaseId;
    export.FcrMember.Assign(import.FcrMember);

    if (!IsEmpty(export.FcrMember.MemberId))
    {
      local.CsePerson.Number = export.FcrMember.MemberId;
      UseCabZeroFillNumber1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.FcrMember, "memberId");

        field.Error = true;

        ExitState = "PERSON_NUMBER_NOT_NUMERIC";

        return;
      }
      else
      {
        export.FcrMember.MemberId = local.CsePerson.Number;
      }
    }

    export.HiddnPrevMemberId.MemberId = import.HiddnPrevMemberId.MemberId;
    MoveFcrCaseMembers6(import.CseMemberInfo, export.CseMemberInfo);
    MoveFcrCaseMembers4(import.FcrMemberInfo, export.FcrMemberInfo);
    export.FcrSsaLastResi.Text30 = import.FcrSsaLastResi.Text30;
    export.FcrSsaLastLsum.Text30 = import.FcrSsaLastLsum.Text30;
    export.Hidden.Assign(import.Hidden);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.CurrentMemberCount.Count = import.CurrentMemberCount.Count;
    export.PassCaseNumber.Number = export.FcrCaseMaster.CaseId;

    for(import.FcrMemberList.Index = 0; import.FcrMemberList.Index < import
      .FcrMemberList.Count; ++import.FcrMemberList.Index)
    {
      if (!import.FcrMemberList.CheckSize())
      {
        break;
      }

      export.FcrMemberList.Index = import.FcrMemberList.Index;
      export.FcrMemberList.CheckSize();

      export.FcrMemberList.Update.FcrCaseMembers.Assign(
        import.FcrMemberList.Item.FcrCaseMembers);
      export.FcrMemberList.Update.Comma.SelectChar =
        import.FcrMemberList.Item.Comma.SelectChar;
      export.FcrMemberList.Update.SelectChar.SelectChar =
        import.FcrMemberList.Item.SelectChar.SelectChar;

      switch(TrimEnd(export.FcrMemberList.Item.FcrCaseMembers.
        ParticipantType ?? ""))
      {
        case "AP":
          export.PassAp.Number =
            export.FcrMemberList.Item.FcrCaseMembers.MemberId;
          export.PassAp.FirstName =
            export.FcrMemberList.Item.FcrCaseMembers.FirstName ?? Spaces(12);
          export.PassAp.LastName =
            export.FcrMemberList.Item.FcrCaseMembers.LastName ?? Spaces(17);
          export.PassAp.MiddleInitial =
            export.FcrMemberList.Item.FcrCaseMembers.MiddleName ?? Spaces(1);
          export.PassAp.Sex =
            export.FcrMemberList.Item.FcrCaseMembers.SexCode ?? Spaces(1);
          export.PassAp.Ssn = export.FcrMemberList.Item.FcrCaseMembers.Ssn ?? Spaces
            (9);

          break;
        case "AR":
          export.PassAr.Number =
            export.FcrMemberList.Item.FcrCaseMembers.MemberId;
          export.PassAr.FirstName =
            export.FcrMemberList.Item.FcrCaseMembers.FirstName ?? Spaces(12);
          export.PassAr.LastName =
            export.FcrMemberList.Item.FcrCaseMembers.LastName ?? Spaces(17);
          export.PassAr.MiddleInitial =
            export.FcrMemberList.Item.FcrCaseMembers.MiddleName ?? Spaces(1);
          export.PassAr.Sex =
            export.FcrMemberList.Item.FcrCaseMembers.SexCode ?? Spaces(1);
          export.PassAr.Ssn = export.FcrMemberList.Item.FcrCaseMembers.Ssn ?? Spaces
            (9);

          break;
        default:
          break;
      }
    }

    import.FcrMemberList.CheckIndex();

    // *********************************************************************************
    // Check to see user requested next tran action.
    // *********************************************************************************
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.FcrCaseMaster.CaseId;
      export.Hidden.CsePersonNumber = export.FcrMember.MemberId;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    // *********************************************************************************
    // Check to see the user is coming to this procedure using NEXT TRAN 
    // command.
    // *********************************************************************************
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.FcrCaseMaster.CaseId = export.Hidden.CaseNumber ?? Spaces(10);
        export.FcrMember.MemberId = export.Hidden.CsePersonNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    // *********************************************************************************
    // Check to see the user is flowing Main Menu Screen.
    // *********************************************************************************
    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // *********************************************************************************
    // Check to see whether Case Id or Member Id is modified by the user and the
    // command
    // is NOT DISPLAY then dipslay error message stating display first.
    // *********************************************************************************
    if (!Equal(global.Command, "DISPLAY") && (
      !Equal(export.FcrCaseMaster.CaseId, export.HiddnStartPrevCaseId.CaseId) ||
      !Equal(export.FcrMember.MemberId, export.HiddnPrevMemberId.MemberId)))
    {
      ExitState = "OE_0206_MUST_DISPLAY_FIRST";

      return;
    }

    // *********************************************************************************
    // Perform required secutiy check to see whether the user has access to this
    // screen,
    // Case & case member etc., if not, give appropriate error messages.
    // *********************************************************************************
    if (Equal(global.Command, "DISPLAY"))
    {
      local.Pass.Number = export.FcrCaseMaster.CaseId;
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ********************************************************************************************
        // The below code ESCAPE code will be removed and supervisor check code 
        // will be uncommented out
        // once business decides to give access to field workers. The code to 
        // check the logged in user
        // is the supervisor for the selected CASE is very similar to FPLS 
        // screen code.
        // ********************************************************************************************
        MoveFcrCaseMembers2(local.NullFcrCaseMembers, export.FcrMember);
        MoveFcrCaseMembers5(local.NullFcrCaseMembers, export.CseMemberInfo);
        MoveFcrCaseMembers3(local.NullFcrCaseMembers, export.FcrMemberInfo);
        export.FcrSsaLastResi.Text30 = local.NullFcrSsaLastResi.Text30;
        export.FcrSsaLastLsum.Text30 = local.NullFcrSsaLastLsum.Text30;

        return;
      }
    }

    // *********************************************************************************
    // Perform required screen edits:
    //      1.  Check to see Case Number is entered by the user.
    // Case Id is mandatory, if member id is not entered, display the AP person 
    // from the
    // case.
    // *********************************************************************************
    if (IsEmpty(export.FcrCaseMaster.CaseId) || Equal
      (export.FcrCaseMaster.CaseId, "0000000000"))
    {
      var field = GetField(export.FcrCaseMaster, "caseId");

      field.Error = true;

      ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

      return;
    }

    // *********************************************************************************
    // Check to see user has pressed below mentioned flow related function keys 
    // and
    // validate required data is entered by the user before flowing to those 
    // screens.
    //       1.  APDS - AP Details
    //       2.  ARDS - AR Details
    //       3.  CHDS - Child Details
    //       4.  ALTS - Alternate SSN and Alias
    // *********************************************************************************
    if (Equal(global.Command, "APDS") || Equal(global.Command, "ARDS") || Equal
      (global.Command, "CHDS") || Equal(global.Command, "ALTS"))
    {
      if (IsEmpty(export.FcrMember.MemberId) || IsEmpty
        (export.FcrCaseMaster.CaseId))
      {
        var field = GetField(export.FcrMember, "memberId");

        field.Error = true;

        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

        return;
      }
    }

    // *********************************************************************************
    //                    M A I N   C A S E   O F   C O M M A N D
    // Based on command issued by the user, the procedure will carry out 
    // respective
    // actions.
    // *********************************************************************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.HiddnStartPrevCaseId.CaseId = export.FcrCaseMaster.CaseId;
        UseOeFcrvBuildFcrCaseView();

        if (export.FcrMemberList.IsEmpty)
        {
          export.CurrentMemberCount.Count = 0;
          MoveFcrCaseMembers2(local.NullFcrCaseMembers, export.FcrMember);
          MoveFcrCaseMembers5(local.NullFcrCaseMembers, export.CseMemberInfo);
          MoveFcrCaseMembers3(local.NullFcrCaseMembers, export.FcrMemberInfo);
          export.FcrSsaLastResi.Text30 = local.NullFcrSsaLastResi.Text30;
          export.FcrSsaLastLsum.Text30 = local.NullFcrSsaLastLsum.Text30;

          if (IsExitState("OE_0205_FCR_CASE_NOT_FOUND"))
          {
            var field1 = GetField(export.FcrCaseMaster, "caseId");

            field1.Error = true;

            return;
          }

          var field = GetField(export.FcrMember, "memberId");

          field.Error = true;

          ExitState = "ACO_NI0000_NO_ITEMS_FOUND";

          return;
        }
        else
        {
          if (!IsEmpty(export.FcrMember.MemberId))
          {
            for(export.FcrMemberList.Index = 0; export.FcrMemberList.Index < export
              .FcrMemberList.Count; ++export.FcrMemberList.Index)
            {
              if (!export.FcrMemberList.CheckSize())
              {
                break;
              }

              if (Equal(export.FcrMember.MemberId,
                export.FcrMemberList.Item.FcrCaseMembers.MemberId))
              {
                export.CurrentMemberCount.Count = export.FcrMemberList.Index + 1
                  ;

                goto Test;
              }
            }

            export.FcrMemberList.CheckIndex();
            MoveFcrCaseMembers2(local.NullFcrCaseMembers, export.FcrMember);
            MoveFcrCaseMembers5(local.NullFcrCaseMembers, export.CseMemberInfo);
            MoveFcrCaseMembers3(local.NullFcrCaseMembers, export.FcrMemberInfo);
            export.FcrSsaLastResi.Text30 = local.NullFcrSsaLastResi.Text30;
            export.FcrSsaLastLsum.Text30 = local.NullFcrSsaLastLsum.Text30;

            var field = GetField(export.FcrMember, "memberId");

            field.Error = true;

            ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";

            return;
          }
          else
          {
            export.CurrentMemberCount.Count = 1;
          }

Test:

          if (export.FcrMemberList.IsFull)
          {
            ExitState = "ACO_NI0000_LST_RETURNED_FULL";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }

        break;
      case "NXTMBR":
        if (export.CurrentMemberCount.Count >= export.FcrMemberList.Count)
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          return;
        }
        else
        {
          ++export.CurrentMemberCount.Count;

          export.FcrMemberList.Index = export.CurrentMemberCount.Count - 1;
          export.FcrMemberList.CheckSize();
        }

        break;
      case "PRVMBR":
        if (export.CurrentMemberCount.Count <= 1)
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }
        else
        {
          --export.CurrentMemberCount.Count;
        }

        break;
      case "CHDS":
        if (Equal(export.FcrMember.ParticipantType, "CH"))
        {
          export.PassCh.Number = export.FcrMember.MemberId;
          ExitState = "ECO_LNK_TO_CHDS";

          return;
        }

        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

        return;
      case "ALTS":
        export.PassCaseNumber.Number = export.FcrCaseMaster.CaseId;
        ExitState = "ECO_XFR_TO_ALT_SSN_AND_ALIAS";

        return;
      case "APDS":
        ExitState = "ECO_LNK_TO_AP_DETAILS";

        return;
      case "ARDS":
        ExitState = "ECO_LNK_TO_AR_DETAILS";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // *********************************************************************************
    // Set selected(current subscript value) FCR Member Id to screen field for 
    // display.
    // *********************************************************************************
    export.FcrMemberList.Index = export.CurrentMemberCount.Count - 1;
    export.FcrMemberList.CheckSize();

    MoveFcrCaseMembers1(export.FcrMemberList.Item.FcrCaseMembers,
      export.FcrMember);

    // *********************************************************************************
    // Use the below mentioned Action Block to retrieve FCR Case member 
    // information for
    // the selected member id.
    // *********************************************************************************
    UseOeMbdtGetFcrCaseMemberDet();

    // *********************************************************************************
    // Check to see any exceptions returned from the action block, if so, 
    // highlight the
    // member id and display message stating Invlalid member id.
    // *********************************************************************************
    if (IsExitState("FN0000_PERSON_NUMBER_NOT_FOUND"))
    {
      MoveFcrCaseMembers2(local.NullFcrCaseMembers, export.FcrMember);
      MoveFcrCaseMembers5(local.NullFcrCaseMembers, export.CseMemberInfo);
      MoveFcrCaseMembers3(local.NullFcrCaseMembers, export.FcrMemberInfo);
      export.FcrSsaLastResi.Text30 = local.NullFcrSsaLastResi.Text30;
      export.FcrSsaLastLsum.Text30 = local.NullFcrSsaLastLsum.Text30;

      var field = GetField(export.FcrMember, "memberId");

      field.Error = true;

      return;
    }

    // *********************************************************************************
    // On successful retrieval of FCR member information, format the Lump sum 
    // and
    // Resiential addresses to display in one line.  Set also the hidden field 
    // vlaues to
    // Track the member ID change by the user.
    // *********************************************************************************
    export.HiddnPrevMemberId.MemberId = export.FcrMember.MemberId;
    export.FcrSsaLastResi.Text30 =
      TrimEnd(export.FcrMemberInfo.SsaCityOfLastResidence) + " " + TrimEnd
      (export.FcrMemberInfo.SsaStateOfLastResidence) + " " + TrimEnd
      (export.FcrMemberInfo.SsaZipCodeOfLastResidence);
    export.FcrSsaLastLsum.Text30 =
      TrimEnd(export.FcrMemberInfo.SsaCityOfLumpSumPayment) + " " + TrimEnd
      (export.FcrMemberInfo.SsaStateOfLumpSumPayment) + " " + TrimEnd
      (export.FcrMemberInfo.SsaZipCodeOfLumpSumPayment);
  }

  private static void MoveFcrCaseMaster(FcrCaseMaster source,
    FcrCaseMaster target)
  {
    target.CaseSentDateToFcr = source.CaseSentDateToFcr;
    target.FcrCaseResponseDate = source.FcrCaseResponseDate;
  }

  private static void MoveFcrCaseMembers1(FcrCaseMembers source,
    FcrCaseMembers target)
  {
    target.MemberId = source.MemberId;
    target.ActionTypeCode = source.ActionTypeCode;
    target.ParticipantType = source.ParticipantType;
    target.BatchNumber = source.BatchNumber;
  }

  private static void MoveFcrCaseMembers2(FcrCaseMembers source,
    FcrCaseMembers target)
  {
    target.ActionTypeCode = source.ActionTypeCode;
    target.ParticipantType = source.ParticipantType;
    target.BatchNumber = source.BatchNumber;
  }

  private static void MoveFcrCaseMembers3(FcrCaseMembers source,
    FcrCaseMembers target)
  {
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleName = source.MiddleName;
    target.LastName = source.LastName;
    target.FamilyViolence = source.FamilyViolence;
    target.PreviousSsn = source.PreviousSsn;
    target.AdditionalSsn1 = source.AdditionalSsn1;
    target.AdditionalSsn2 = source.AdditionalSsn2;
    target.AdditionalFirstName1 = source.AdditionalFirstName1;
    target.AdditionalMiddleName1 = source.AdditionalMiddleName1;
    target.AdditionalLastName1 = source.AdditionalLastName1;
    target.AdditionalFirstName2 = source.AdditionalFirstName2;
    target.AdditionalMiddleName2 = source.AdditionalMiddleName2;
    target.AdditionalLastName2 = source.AdditionalLastName2;
    target.AdditionalFirstName3 = source.AdditionalFirstName3;
    target.AdditionalMiddleName3 = source.AdditionalMiddleName3;
    target.AdditionalLastName3 = source.AdditionalLastName3;
    target.AdditionalFirstName4 = source.AdditionalFirstName4;
    target.AdditionalMiddleName4 = source.AdditionalMiddleName4;
    target.AdditionalLastName4 = source.AdditionalLastName4;
    target.SsnValidityCode = source.SsnValidityCode;
    target.ProvidedOrCorrectedSsn = source.ProvidedOrCorrectedSsn;
    target.SsaDateOfBirthIndicator = source.SsaDateOfBirthIndicator;
    target.DateOfDeath = source.DateOfDeath;
    target.SsaZipCodeOfLastResidence = source.SsaZipCodeOfLastResidence;
    target.SsaZipCodeOfLumpSumPayment = source.SsaZipCodeOfLumpSumPayment;
    target.FcrPrimarySsn = source.FcrPrimarySsn;
    target.FcrPrimaryFirstName = source.FcrPrimaryFirstName;
    target.FcrPrimaryMiddleName = source.FcrPrimaryMiddleName;
    target.FcrPrimaryLastName = source.FcrPrimaryLastName;
    target.AdditionalSsn1ValidityCode = source.AdditionalSsn1ValidityCode;
    target.AdditionalSsn2ValidityCode = source.AdditionalSsn2ValidityCode;
    target.SsaCityOfLastResidence = source.SsaCityOfLastResidence;
    target.SsaStateOfLastResidence = source.SsaStateOfLastResidence;
    target.SsaCityOfLumpSumPayment = source.SsaCityOfLumpSumPayment;
    target.SsaStateOfLumpSumPayment = source.SsaStateOfLumpSumPayment;
  }

  private static void MoveFcrCaseMembers4(FcrCaseMembers source,
    FcrCaseMembers target)
  {
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleName = source.MiddleName;
    target.LastName = source.LastName;
    target.FamilyViolence = source.FamilyViolence;
    target.PreviousSsn = source.PreviousSsn;
    target.AdditionalSsn1 = source.AdditionalSsn1;
    target.AdditionalSsn2 = source.AdditionalSsn2;
    target.AdditionalFirstName1 = source.AdditionalFirstName1;
    target.AdditionalMiddleName1 = source.AdditionalMiddleName1;
    target.AdditionalLastName1 = source.AdditionalLastName1;
    target.AdditionalFirstName2 = source.AdditionalFirstName2;
    target.AdditionalMiddleName2 = source.AdditionalMiddleName2;
    target.AdditionalLastName2 = source.AdditionalLastName2;
    target.AdditionalFirstName3 = source.AdditionalFirstName3;
    target.AdditionalMiddleName3 = source.AdditionalMiddleName3;
    target.AdditionalLastName3 = source.AdditionalLastName3;
    target.AdditionalFirstName4 = source.AdditionalFirstName4;
    target.AdditionalMiddleName4 = source.AdditionalMiddleName4;
    target.AdditionalLastName4 = source.AdditionalLastName4;
    target.SsnValidityCode = source.SsnValidityCode;
    target.ProvidedOrCorrectedSsn = source.ProvidedOrCorrectedSsn;
    target.SsaDateOfBirthIndicator = source.SsaDateOfBirthIndicator;
    target.DateOfDeath = source.DateOfDeath;
    target.FcrPrimarySsn = source.FcrPrimarySsn;
    target.FcrPrimaryFirstName = source.FcrPrimaryFirstName;
    target.FcrPrimaryMiddleName = source.FcrPrimaryMiddleName;
    target.FcrPrimaryLastName = source.FcrPrimaryLastName;
    target.AdditionalSsn1ValidityCode = source.AdditionalSsn1ValidityCode;
    target.AdditionalSsn2ValidityCode = source.AdditionalSsn2ValidityCode;
  }

  private static void MoveFcrCaseMembers5(FcrCaseMembers source,
    FcrCaseMembers target)
  {
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleName = source.MiddleName;
    target.LastName = source.LastName;
    target.FamilyViolence = source.FamilyViolence;
    target.PreviousSsn = source.PreviousSsn;
    target.AdditionalSsn1 = source.AdditionalSsn1;
    target.AdditionalSsn2 = source.AdditionalSsn2;
    target.AdditionalFirstName1 = source.AdditionalFirstName1;
    target.AdditionalMiddleName1 = source.AdditionalMiddleName1;
    target.AdditionalLastName1 = source.AdditionalLastName1;
    target.AdditionalFirstName2 = source.AdditionalFirstName2;
    target.AdditionalMiddleName2 = source.AdditionalMiddleName2;
    target.AdditionalLastName2 = source.AdditionalLastName2;
    target.AdditionalFirstName3 = source.AdditionalFirstName3;
    target.AdditionalMiddleName3 = source.AdditionalMiddleName3;
    target.AdditionalLastName3 = source.AdditionalLastName3;
    target.AdditionalFirstName4 = source.AdditionalFirstName4;
    target.AdditionalMiddleName4 = source.AdditionalMiddleName4;
    target.AdditionalLastName4 = source.AdditionalLastName4;
    target.SsnValidityCode = source.SsnValidityCode;
    target.ProvidedOrCorrectedSsn = source.ProvidedOrCorrectedSsn;
    target.SsaDateOfBirthIndicator = source.SsaDateOfBirthIndicator;
    target.SsaZipCodeOfLastResidence = source.SsaZipCodeOfLastResidence;
    target.SsaZipCodeOfLumpSumPayment = source.SsaZipCodeOfLumpSumPayment;
    target.AdditionalSsn1ValidityCode = source.AdditionalSsn1ValidityCode;
    target.AdditionalSsn2ValidityCode = source.AdditionalSsn2ValidityCode;
  }

  private static void MoveFcrCaseMembers6(FcrCaseMembers source,
    FcrCaseMembers target)
  {
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleName = source.MiddleName;
    target.LastName = source.LastName;
    target.FamilyViolence = source.FamilyViolence;
    target.PreviousSsn = source.PreviousSsn;
    target.AdditionalSsn1 = source.AdditionalSsn1;
    target.AdditionalSsn2 = source.AdditionalSsn2;
    target.AdditionalFirstName1 = source.AdditionalFirstName1;
    target.AdditionalMiddleName1 = source.AdditionalMiddleName1;
    target.AdditionalLastName1 = source.AdditionalLastName1;
    target.AdditionalFirstName2 = source.AdditionalFirstName2;
    target.AdditionalMiddleName2 = source.AdditionalMiddleName2;
    target.AdditionalLastName2 = source.AdditionalLastName2;
    target.AdditionalFirstName3 = source.AdditionalFirstName3;
    target.AdditionalMiddleName3 = source.AdditionalMiddleName3;
    target.AdditionalLastName3 = source.AdditionalLastName3;
    target.AdditionalFirstName4 = source.AdditionalFirstName4;
    target.AdditionalMiddleName4 = source.AdditionalMiddleName4;
    target.AdditionalLastName4 = source.AdditionalLastName4;
    target.SsnValidityCode = source.SsnValidityCode;
    target.ProvidedOrCorrectedSsn = source.ProvidedOrCorrectedSsn;
    target.SsaDateOfBirthIndicator = source.SsaDateOfBirthIndicator;
    target.AdditionalSsn1ValidityCode = source.AdditionalSsn1ValidityCode;
    target.AdditionalSsn2ValidityCode = source.AdditionalSsn2ValidityCode;
  }

  private static void MoveFcrMemberList(OeFcrvBuildFcrCaseView.Export.
    FcrMemberListGroup source, Export.FcrMemberListGroup target)
  {
    MoveFcrCaseMaster(source.FcrCase, target.FcrCase);
    target.Comma.SelectChar = source.Comma.SelectChar;
    target.SelectChar.SelectChar = source.SelChar.SelectChar;
    target.FcrCaseMembers.Assign(source.FcrMember);
  }

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = local.ZeroFill.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.ZeroFill.Number = useImport.Case1.Number;
  }

  private void UseOeFcrvBuildFcrCaseView()
  {
    var useImport = new OeFcrvBuildFcrCaseView.Import();
    var useExport = new OeFcrvBuildFcrCaseView.Export();

    useImport.FcrCaseId.CaseId = export.FcrCaseMaster.CaseId;

    Call(OeFcrvBuildFcrCaseView.Execute, useImport, useExport);

    useExport.FcrMemberList.CopyTo(export.FcrMemberList, MoveFcrMemberList);
  }

  private void UseOeMbdtGetFcrCaseMemberDet()
  {
    var useImport = new OeMbdtGetFcrCaseMemberDet.Import();
    var useExport = new OeMbdtGetFcrCaseMemberDet.Export();

    useImport.FcrCaseMaster.CaseId = export.FcrCaseMaster.CaseId;
    useImport.FcrMember.MemberId = export.FcrMember.MemberId;

    Call(OeMbdtGetFcrCaseMemberDet.Execute, useImport, useExport);

    export.FcrMember.Assign(useExport.FcrMember);
    export.CseMemberInfo.Assign(useExport.CseMemberInfo);
    export.FcrMemberInfo.Assign(useExport.FcrMemberInfo);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.Case1.Number = local.Pass.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A FcrMemberListGroup group.</summary>
    [Serializable]
    public class FcrMemberListGroup
    {
      /// <summary>
      /// A value of FcrCase.
      /// </summary>
      [JsonPropertyName("fcrCase")]
      public FcrCaseMaster FcrCase
      {
        get => fcrCase ??= new();
        set => fcrCase = value;
      }

      /// <summary>
      /// A value of Comma.
      /// </summary>
      [JsonPropertyName("comma")]
      public Common Comma
      {
        get => comma ??= new();
        set => comma = value;
      }

      /// <summary>
      /// A value of SelectChar.
      /// </summary>
      [JsonPropertyName("selectChar")]
      public Common SelectChar
      {
        get => selectChar ??= new();
        set => selectChar = value;
      }

      /// <summary>
      /// A value of FcrCaseMembers.
      /// </summary>
      [JsonPropertyName("fcrCaseMembers")]
      public FcrCaseMembers FcrCaseMembers
      {
        get => fcrCaseMembers ??= new();
        set => fcrCaseMembers = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 65;

      private FcrCaseMaster fcrCase;
      private Common comma;
      private Common selectChar;
      private FcrCaseMembers fcrCaseMembers;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of FcrCaseMaster.
    /// </summary>
    [JsonPropertyName("fcrCaseMaster")]
    public FcrCaseMaster FcrCaseMaster
    {
      get => fcrCaseMaster ??= new();
      set => fcrCaseMaster = value;
    }

    /// <summary>
    /// A value of HiddnStartPrevCaseId.
    /// </summary>
    [JsonPropertyName("hiddnStartPrevCaseId")]
    public FcrCaseMaster HiddnStartPrevCaseId
    {
      get => hiddnStartPrevCaseId ??= new();
      set => hiddnStartPrevCaseId = value;
    }

    /// <summary>
    /// A value of FcrMember.
    /// </summary>
    [JsonPropertyName("fcrMember")]
    public FcrCaseMembers FcrMember
    {
      get => fcrMember ??= new();
      set => fcrMember = value;
    }

    /// <summary>
    /// A value of HiddnPrevMemberId.
    /// </summary>
    [JsonPropertyName("hiddnPrevMemberId")]
    public FcrCaseMembers HiddnPrevMemberId
    {
      get => hiddnPrevMemberId ??= new();
      set => hiddnPrevMemberId = value;
    }

    /// <summary>
    /// A value of CseMemberInfo.
    /// </summary>
    [JsonPropertyName("cseMemberInfo")]
    public FcrCaseMembers CseMemberInfo
    {
      get => cseMemberInfo ??= new();
      set => cseMemberInfo = value;
    }

    /// <summary>
    /// A value of FcrMemberInfo.
    /// </summary>
    [JsonPropertyName("fcrMemberInfo")]
    public FcrCaseMembers FcrMemberInfo
    {
      get => fcrMemberInfo ??= new();
      set => fcrMemberInfo = value;
    }

    /// <summary>
    /// A value of FcrSsaLastResi.
    /// </summary>
    [JsonPropertyName("fcrSsaLastResi")]
    public TextWorkArea FcrSsaLastResi
    {
      get => fcrSsaLastResi ??= new();
      set => fcrSsaLastResi = value;
    }

    /// <summary>
    /// A value of FcrSsaLastLsum.
    /// </summary>
    [JsonPropertyName("fcrSsaLastLsum")]
    public TextWorkArea FcrSsaLastLsum
    {
      get => fcrSsaLastLsum ??= new();
      set => fcrSsaLastLsum = value;
    }

    /// <summary>
    /// Gets a value of FcrMemberList.
    /// </summary>
    [JsonIgnore]
    public Array<FcrMemberListGroup> FcrMemberList => fcrMemberList ??= new(
      FcrMemberListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FcrMemberList for json serialization.
    /// </summary>
    [JsonPropertyName("fcrMemberList")]
    [Computed]
    public IList<FcrMemberListGroup> FcrMemberList_Json
    {
      get => fcrMemberList;
      set => FcrMemberList.Assign(value);
    }

    /// <summary>
    /// A value of NextTran.
    /// </summary>
    [JsonPropertyName("nextTran")]
    public Transaction NextTran
    {
      get => nextTran ??= new();
      set => nextTran = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of CurrentMemberCount.
    /// </summary>
    [JsonPropertyName("currentMemberCount")]
    public Common CurrentMemberCount
    {
      get => currentMemberCount ??= new();
      set => currentMemberCount = value;
    }

    private Standard standard;
    private FcrCaseMaster fcrCaseMaster;
    private FcrCaseMaster hiddnStartPrevCaseId;
    private FcrCaseMembers fcrMember;
    private FcrCaseMembers hiddnPrevMemberId;
    private FcrCaseMembers cseMemberInfo;
    private FcrCaseMembers fcrMemberInfo;
    private TextWorkArea fcrSsaLastResi;
    private TextWorkArea fcrSsaLastLsum;
    private Array<FcrMemberListGroup> fcrMemberList;
    private Transaction nextTran;
    private NextTranInfo hidden;
    private Common currentMemberCount;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FcrMemberListGroup group.</summary>
    [Serializable]
    public class FcrMemberListGroup
    {
      /// <summary>
      /// A value of FcrCase.
      /// </summary>
      [JsonPropertyName("fcrCase")]
      public FcrCaseMaster FcrCase
      {
        get => fcrCase ??= new();
        set => fcrCase = value;
      }

      /// <summary>
      /// A value of Comma.
      /// </summary>
      [JsonPropertyName("comma")]
      public Common Comma
      {
        get => comma ??= new();
        set => comma = value;
      }

      /// <summary>
      /// A value of SelectChar.
      /// </summary>
      [JsonPropertyName("selectChar")]
      public Common SelectChar
      {
        get => selectChar ??= new();
        set => selectChar = value;
      }

      /// <summary>
      /// A value of FcrCaseMembers.
      /// </summary>
      [JsonPropertyName("fcrCaseMembers")]
      public FcrCaseMembers FcrCaseMembers
      {
        get => fcrCaseMembers ??= new();
        set => fcrCaseMembers = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 65;

      private FcrCaseMaster fcrCase;
      private Common comma;
      private Common selectChar;
      private FcrCaseMembers fcrCaseMembers;
    }

    /// <summary>
    /// A value of NextTran.
    /// </summary>
    [JsonPropertyName("nextTran")]
    public Transaction NextTran
    {
      get => nextTran ??= new();
      set => nextTran = value;
    }

    /// <summary>
    /// A value of FcrCaseMaster.
    /// </summary>
    [JsonPropertyName("fcrCaseMaster")]
    public FcrCaseMaster FcrCaseMaster
    {
      get => fcrCaseMaster ??= new();
      set => fcrCaseMaster = value;
    }

    /// <summary>
    /// A value of HiddnStartPrevCaseId.
    /// </summary>
    [JsonPropertyName("hiddnStartPrevCaseId")]
    public FcrCaseMaster HiddnStartPrevCaseId
    {
      get => hiddnStartPrevCaseId ??= new();
      set => hiddnStartPrevCaseId = value;
    }

    /// <summary>
    /// A value of FcrMember.
    /// </summary>
    [JsonPropertyName("fcrMember")]
    public FcrCaseMembers FcrMember
    {
      get => fcrMember ??= new();
      set => fcrMember = value;
    }

    /// <summary>
    /// A value of HiddnPrevMemberId.
    /// </summary>
    [JsonPropertyName("hiddnPrevMemberId")]
    public FcrCaseMembers HiddnPrevMemberId
    {
      get => hiddnPrevMemberId ??= new();
      set => hiddnPrevMemberId = value;
    }

    /// <summary>
    /// A value of CseMemberInfo.
    /// </summary>
    [JsonPropertyName("cseMemberInfo")]
    public FcrCaseMembers CseMemberInfo
    {
      get => cseMemberInfo ??= new();
      set => cseMemberInfo = value;
    }

    /// <summary>
    /// A value of FcrMemberInfo.
    /// </summary>
    [JsonPropertyName("fcrMemberInfo")]
    public FcrCaseMembers FcrMemberInfo
    {
      get => fcrMemberInfo ??= new();
      set => fcrMemberInfo = value;
    }

    /// <summary>
    /// A value of FcrSsaLastResi.
    /// </summary>
    [JsonPropertyName("fcrSsaLastResi")]
    public TextWorkArea FcrSsaLastResi
    {
      get => fcrSsaLastResi ??= new();
      set => fcrSsaLastResi = value;
    }

    /// <summary>
    /// A value of FcrSsaLastLsum.
    /// </summary>
    [JsonPropertyName("fcrSsaLastLsum")]
    public TextWorkArea FcrSsaLastLsum
    {
      get => fcrSsaLastLsum ??= new();
      set => fcrSsaLastLsum = value;
    }

    /// <summary>
    /// Gets a value of FcrMemberList.
    /// </summary>
    [JsonIgnore]
    public Array<FcrMemberListGroup> FcrMemberList => fcrMemberList ??= new(
      FcrMemberListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FcrMemberList for json serialization.
    /// </summary>
    [JsonPropertyName("fcrMemberList")]
    [Computed]
    public IList<FcrMemberListGroup> FcrMemberList_Json
    {
      get => fcrMemberList;
      set => FcrMemberList.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of CurrentMemberCount.
    /// </summary>
    [JsonPropertyName("currentMemberCount")]
    public Common CurrentMemberCount
    {
      get => currentMemberCount ??= new();
      set => currentMemberCount = value;
    }

    /// <summary>
    /// A value of PassCaseNumber.
    /// </summary>
    [JsonPropertyName("passCaseNumber")]
    public Case1 PassCaseNumber
    {
      get => passCaseNumber ??= new();
      set => passCaseNumber = value;
    }

    /// <summary>
    /// A value of PassAp.
    /// </summary>
    [JsonPropertyName("passAp")]
    public CsePersonsWorkSet PassAp
    {
      get => passAp ??= new();
      set => passAp = value;
    }

    /// <summary>
    /// A value of PassAr.
    /// </summary>
    [JsonPropertyName("passAr")]
    public CsePersonsWorkSet PassAr
    {
      get => passAr ??= new();
      set => passAr = value;
    }

    /// <summary>
    /// A value of PassCh.
    /// </summary>
    [JsonPropertyName("passCh")]
    public CsePersonsWorkSet PassCh
    {
      get => passCh ??= new();
      set => passCh = value;
    }

    private Transaction nextTran;
    private FcrCaseMaster fcrCaseMaster;
    private FcrCaseMaster hiddnStartPrevCaseId;
    private FcrCaseMembers fcrMember;
    private FcrCaseMembers hiddnPrevMemberId;
    private FcrCaseMembers cseMemberInfo;
    private FcrCaseMembers fcrMemberInfo;
    private TextWorkArea fcrSsaLastResi;
    private TextWorkArea fcrSsaLastLsum;
    private Array<FcrMemberListGroup> fcrMemberList;
    private NextTranInfo hidden;
    private Standard standard;
    private Common currentMemberCount;
    private Case1 passCaseNumber;
    private CsePersonsWorkSet passAp;
    private CsePersonsWorkSet passAr;
    private CsePersonsWorkSet passCh;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullFcrSsaLastResi.
    /// </summary>
    [JsonPropertyName("nullFcrSsaLastResi")]
    public TextWorkArea NullFcrSsaLastResi
    {
      get => nullFcrSsaLastResi ??= new();
      set => nullFcrSsaLastResi = value;
    }

    /// <summary>
    /// A value of NullFcrSsaLastLsum.
    /// </summary>
    [JsonPropertyName("nullFcrSsaLastLsum")]
    public TextWorkArea NullFcrSsaLastLsum
    {
      get => nullFcrSsaLastLsum ??= new();
      set => nullFcrSsaLastLsum = value;
    }

    /// <summary>
    /// A value of NullFcrCaseMaster.
    /// </summary>
    [JsonPropertyName("nullFcrCaseMaster")]
    public FcrCaseMaster NullFcrCaseMaster
    {
      get => nullFcrCaseMaster ??= new();
      set => nullFcrCaseMaster = value;
    }

    /// <summary>
    /// A value of NullFcrCaseMembers.
    /// </summary>
    [JsonPropertyName("nullFcrCaseMembers")]
    public FcrCaseMembers NullFcrCaseMembers
    {
      get => nullFcrCaseMembers ??= new();
      set => nullFcrCaseMembers = value;
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
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public Case1 ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Case1 Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public Common Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    /// <summary>
    /// A value of AccessOnThisCase.
    /// </summary>
    [JsonPropertyName("accessOnThisCase")]
    public Common AccessOnThisCase
    {
      get => accessOnThisCase ??= new();
      set => accessOnThisCase = value;
    }

    private TextWorkArea nullFcrSsaLastResi;
    private TextWorkArea nullFcrSsaLastLsum;
    private FcrCaseMaster nullFcrCaseMaster;
    private FcrCaseMembers nullFcrCaseMembers;
    private CsePerson csePerson;
    private Case1 zeroFill;
    private Case1 pass;
    private Common supervisor;
    private Common accessOnThisCase;
  }
#endregion
}
