// Program: SI_INRD_INQUIRY_MAINTENANCE, ID: 371426525, model: 746.
// Short name: SWEINRDP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_INRD_INQUIRY_MAINTENANCE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiInrdInquiryMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INRD_INQUIRY_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiInrdInquiryMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiInrdInquiryMaintenance.
  /// </summary>
  public SiInrdInquiryMaintenance(IContext context, Import import, Export export)
    :
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
    //        M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 07-10-95  Ken Evans		Initial Development
    // 02-02-96  Bruce Moore		Retrofit
    // 05-02-96  Rao Mulpuri		Changes to Date Sent
    // 				IDCR# 131
    // 06-24-96  Rod Grey		Add Print functionality
    // 11/05/96  G. Lofton - MTW	Add new security and removed
    // 				old.
    // 12/06/96  G. Lofton - MTW	Add changes for referral
    // 				denial.
    // 01/02/97  Raju      - MTW	event insertion code
    // ------- no event is to be raised from this prad -------
    // Contact Jack Rookard/SID/Rod before raising
    //    events in this prad - Raju
    //    01/02/1997 1300 hrs CST
    // 03/06/97  Sid			IDCR # 323 Correct problem with
    // 				phone number.
    // 04/02/97  Siraj Konkader      	IDCR 268. Add new letter - NADC
    // 				application denied or incomplete.
    // 07/09/97  Sid Chowdhary		Raise events for Srv Type change.
    // 10/13/98  C Deghand             Removed the code and the USE
    //                                 
    // statement that called the action
    //                                 
    // block to raise events.  The
    // events
    //                                 
    // will now be raised from CADS,
    // not
    //                                 
    // from here.
    // 12/15/1998	M Ramirez	Revised print process
    // 12/15/1998	M Ramirez	Modified security to check CRUD actions only
    // ---------------------------------------------------------------
    // 03/05/99 W.Campbell             Changed the view
    //                                 
    // matching on a USE stmt so that
    //                                 
    // the import view
    // information_request
    //                                 
    // of called CAB
    //                                 
    // SI_INRD_READ_INFORMATION_REQ
    //                                 
    // is matched to the export view
    //                                 
    // information_req of this PRAD.
    // ------------------------------------------------------------
    // 05/26/99 W.Campbell             Replaced zd exit states.
    // ---------------------------------------------------
    // 01/06/2000	M Ramirez	83300	NEXT TRAN needs to be cleared
    // 					before invoking print process
    // ---------------------------------------------------
    // 04/05/00 C. Scroggins Added read for case for security for family 
    // violence.
    // ---------------------------------------------------
    // 08/07/00 W.Campbell             Added PFK 24 to
    //                                 
    // Screen definition with new
    // Command
    //                                 
    // APPLPROC.  Added logic to Pstep
    //                                 
    // for processing of the new
    // Command.
    //                                 
    // Logic added for new attribute,
    //                                 
    // Application Processed IND added
    // to
    //                                 
    // entity type information_request.
    //                                 
    // Work done on PR# 100532.
    // -----------------------------------------------------------------------------------------------
    // 10/23/01 GVandy			Performance issue.  Replaced read of case (for family 
    // violence)
    // 				which was using non-indexed denormalized attribute
    // 				info_request_id.  Now using the relationship from case
    // 				to information_request.
    // -----------------------------------------------------------------------------------------------
    // SWSRKXD PR149011 08/16/2002
    // - Fix screen Help Id.
    // -----------------------------------------------------------------------------------------------
    // *********************************************
    // With this PRAD you can create, modify, or
    // delete an Inquiry.
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpDocSetLiterals();
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    MoveInformationRequest3(import.InformationRequest, export.InformationRequest);
      
    export.LocateInd.SelectChar = import.LocateInd.SelectChar;
    export.WithMedicalInd.SelectChar = import.WithMedicalInd.SelectChar;
    export.LimitedInd.SelectChar = import.LimitedInd.SelectChar;
    export.Namelist.Assign(import.Namelist);
    export.HiddenInformationRequest.Assign(import.HiddenInformationRequest);
    export.StatePrompt.PromptField = import.StatePrompt.PromptField;
    export.TypePrompt.PromptField = import.TypePrompt.PromptField;
    export.ReopenTypr.PromptField = import.ReopenType.PromptField;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.DeniedIncompleteCommon.SelectChar =
      import.DeniedIncompleteCommon.SelectChar;
    MoveInformationRequest5(import.DeniedIncompleteInformationRequest,
      export.DeniedIncompleteInformationRequest);
    export.WorkerId.Text8 = import.WorkerId.Text8;
    MoveWorkArea(import.ReopenFromComn, export.ReopenFromComn);
    export.Ar.Number = import.Ar.Number;
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Assign(import.CsePerson);

    if (IsEmpty(export.WorkerId.Text8))
    {
      export.WorkerId.Text8 = global.UserId;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      if (import.InformationRequest.Number > 0)
      {
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    // to validate action level security
    // ------------------------------------------------------------
    // 08/07/00 W.Campbell - Added clause to
    // include Command APPLPROC in the
    // following IF stmt for security checking.
    // Work done on PR# 100532.
    // ------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "APPLPROC") || Equal
      (global.Command, "PRINT"))
    {
      UseScCabTestSecurity1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else if (Equal(global.Command, "DISPLAY"))
    {
      // ---------------------------------------------------------------------------------------
      // Added read for case for security for family violence.
      // ---------------------------------------------------------------------------------------
      if (export.InformationRequest.Number > 0)
      {
        if (ReadCase1())
        {
          local.Case1.Number = entities.Case1.Number;
        }
      }

      UseScCabTestSecurity2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // *********************************************
      // SCREEN VALIDATIONS
      // *********************************************
      var field = GetField(export.InformationRequest, "number");

      field.Protected = true;

      if (export.InformationRequest.ApplicantPhone.GetValueOrDefault() > 0 || export
        .InformationRequest.ApplicantAreaCode.GetValueOrDefault() > 0)
      {
        if (export.InformationRequest.ApplicantPhone.GetValueOrDefault() <= 0)
        {
          var field1 = GetField(export.InformationRequest, "applicantPhone");

          field1.Error = true;

          ExitState = "OE0000_PHONE_NBR_REQD";
        }

        if (export.InformationRequest.ApplicantAreaCode.GetValueOrDefault() <= 0
          )
        {
          var field1 = GetField(export.InformationRequest, "applicantAreaCode");

          field1.Error = true;

          ExitState = "CO0000_PHONE_AREA_CODE_REQD";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (!IsEmpty(export.InformationRequest.Type1))
      {
        local.Code.CodeName = "INFORMATION REQUEST TYPE";
        local.CodeValue.Cdvalue = export.InformationRequest.Type1;
        UseCabValidateCodeValue1();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          var field1 = GetField(export.InformationRequest, "type1");

          field1.Error = true;

          ExitState = "INVALID_TYPE_CODE";
        }
      }
      else
      {
        var field1 = GetField(export.InformationRequest, "type1");

        field1.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (AsChar(export.InformationRequest.Type1) == 'R')
      {
        if (!IsEmpty(export.InformationRequest.ReopenReasonType))
        {
          if (!IsEmpty(export.InformationRequest.MiscellaneousReason))
          {
            if (AsChar(export.InformationRequest.ReopenReasonType) != 'S')
            {
              var field1 =
                GetField(export.InformationRequest, "reopenReasonType");

              field1.Error = true;

              ExitState = "MIS_REASON_ONLY_USED_W_MIS_REPN";

              goto Test1;
            }
          }

          if (AsChar(export.InformationRequest.ReopenReasonType) == 'S' && IsEmpty
            (export.InformationRequest.MiscellaneousReason))
          {
            var field1 =
              GetField(export.InformationRequest, "reopenReasonType");

            field1.Error = true;

            ExitState = "ADDITIONAL_MISC_REASON_MISSING";

            goto Test1;
          }

          local.Code.CodeName = "INFORMATION REQUEST REOPEN RSN";
          local.CodeValue.Cdvalue =
            export.InformationRequest.ReopenReasonType ?? Spaces(10);
          UseCabValidateCodeValue1();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            var field1 = GetField(export.InformationRequest, "type1");

            field1.Error = true;

            ExitState = "INVALID_TYPE_CODE";
          }
        }
        else
        {
          var field1 = GetField(export.InformationRequest, "reopenReasonType");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }
      else if (!IsEmpty(export.InformationRequest.ReopenReasonType))
      {
        var field1 = GetField(export.InformationRequest, "reopenReasonType");

        field1.Error = true;

        var field2 = GetField(export.InformationRequest, "type1");

        field2.Error = true;

        ExitState = "INVALID_ENROLL_TYPE";
      }

Test1:

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (AsChar(import.InformationRequest.Type1) == 'P' || AsChar
        (import.InformationRequest.Type1) == 'R' && AsChar
        (import.InformationRequest.ReopenReasonType) == 'P')
      {
        // ncp enrollment
        if (!IsEmpty(import.Case1.Number))
        {
          if (!IsEmpty(import.InformationRequest.ApplicantStreet1))
          {
            if (ReadCaseRoleCsePerson6())
            {
              local.Ar.Number = entities.CsePerson.Number;
            }

            local.CsePerson.Number = local.Ar.Number;
            UseFnCabReadCsePersonAddress();

            if (Equal(export.InformationRequest.ApplicantStreet1,
              local.CsePersonAddress.Street1) && Equal
              (export.InformationRequest.ApplicantStreet2,
              local.CsePersonAddress.Street2) && Equal
              (export.InformationRequest.ApplicantCity,
              local.CsePersonAddress.City) && Equal
              (export.InformationRequest.ApplicantState,
              local.CsePersonAddress.State) && Equal
              (export.InformationRequest.ApplicantZip5,
              local.CsePersonAddress.ZipCode))
            {
              var field1 =
                GetField(export.InformationRequest, "applicantStreet1");

              field1.Error = true;

              var field2 =
                GetField(export.InformationRequest, "applicantStreet2");

              field2.Error = true;

              var field3 = GetField(export.InformationRequest, "applicantCity");

              field3.Error = true;

              var field4 =
                GetField(export.InformationRequest, "applicantState");

              field4.Error = true;

              var field5 = GetField(export.InformationRequest, "applicantZip5");

              field5.Error = true;

              ExitState = "CAN_NOT_USE_AR_ADDRESS";

              return;
            }
            else
            {
              // assuming that this address belongs to the AP
            }
          }
          else
          {
            if (ReadCaseRoleCsePerson4())
            {
              local.Ar.Number = entities.CsePerson.Number;
            }

            local.CsePerson.Number = local.Ar.Number;
            UseFnCabReadCsePersonAddress();
            export.InformationRequest.ApplicantStreet1 =
              local.CsePersonAddress.Street1 ?? "";
            export.InformationRequest.ApplicantStreet2 =
              local.CsePersonAddress.Street2 ?? "";
            export.InformationRequest.ApplicantCity =
              local.CsePersonAddress.City ?? "";
            export.InformationRequest.ApplicantState =
              local.CsePersonAddress.State ?? "";
            export.InformationRequest.ApplicantZip5 =
              local.CsePersonAddress.ZipCode ?? "";
            export.InformationRequest.ApplicantZip4 =
              local.CsePersonAddress.Zip4 ?? "";
          }
        }
        else
        {
          // there is no case for this information request record yet
          // address is consider for the AP since this is new
        }
      }

      if (IsEmpty(import.InformationRequest.ApplicantLastName))
      {
        if (IsEmpty(import.InformationRequest.CallerLastName))
        {
          var field1 = GetField(export.InformationRequest, "applicantLastName");

          field1.Error = true;

          var field2 = GetField(export.InformationRequest, "callerLastName");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }
        }

        if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
          (import.InformationRequest.Type1) == 'I' || AsChar
          (import.InformationRequest.Type1) == 'M' || AsChar
          (import.InformationRequest.Type1) == 'U' || AsChar
          (import.InformationRequest.Type1) == 'J' || AsChar
          (import.InformationRequest.Type1) == 'F')
        {
          goto Test3;
        }

        switch(AsChar(import.InformationRequest.ApplicationSentIndicator))
        {
          case 'N':
            break;
          case ' ':
            break;
          default:
            var field1 =
              GetField(export.InformationRequest, "applicationSentIndicator");

            field1.Error = true;

            ExitState = "INVALID_INDICATOR_N_OR_SPACE";

            return;
        }
      }
      else
      {
        if (IsEmpty(import.InformationRequest.ApplicantFirstName))
        {
          var field1 =
            GetField(export.InformationRequest, "applicantFirstName");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }
        }

        if (IsEmpty(import.InformationRequest.ApplicantStreet1) || IsEmpty
          (import.InformationRequest.ApplicantCity) || IsEmpty
          (import.InformationRequest.ApplicantState) || IsEmpty
          (import.InformationRequest.ApplicantZip5))
        {
          if (AsChar(import.InformationRequest.Type1) == 'I' || AsChar
            (import.InformationRequest.Type1) == 'J' || AsChar
            (import.InformationRequest.Type1) == 'F' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'F')
          {
            // no address allowed for these enrollment types
            if (!IsEmpty(import.InformationRequest.ApplicantStreet1) || !
              IsEmpty(import.InformationRequest.ApplicantCity) || !
              IsEmpty(import.InformationRequest.ApplicantState) || !
              IsEmpty(import.InformationRequest.ApplicantZip5))
            {
              var field6 =
                GetField(export.InformationRequest, "applicantStreet1");

              field6.Error = true;

              var field7 = GetField(export.InformationRequest, "applicantCity");

              field7.Error = true;

              var field8 =
                GetField(export.InformationRequest, "applicantState");

              field8.Error = true;

              var field9 = GetField(export.InformationRequest, "applicantZip5");

              field9.Error = true;

              ExitState = "ADDRESS_NT_ALLOWED_ENROLLMENT_TY";

              return;
            }

            goto Test2;
          }

          var field1 = GetField(export.InformationRequest, "applicantStreet1");

          field1.Error = true;

          var field2 = GetField(export.InformationRequest, "applicantStreet2");

          field2.Error = true;

          var field3 = GetField(export.InformationRequest, "applicantCity");

          field3.Error = true;

          var field4 = GetField(export.InformationRequest, "applicantState");

          field4.Error = true;

          var field5 = GetField(export.InformationRequest, "applicantZip5");

          field5.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }
        else
        {
          if (AsChar(import.InformationRequest.Type1) == 'I' || AsChar
            (import.InformationRequest.Type1) == 'J' || AsChar
            (import.InformationRequest.Type1) == 'F' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'F')
          {
            // no address allowed for these enrollment types
            var field1 =
              GetField(export.InformationRequest, "applicantStreet1");

            field1.Error = true;

            var field2 = GetField(export.InformationRequest, "applicantCity");

            field2.Error = true;

            var field3 = GetField(export.InformationRequest, "applicantState");

            field3.Error = true;

            var field4 = GetField(export.InformationRequest, "applicantZip5");

            field4.Error = true;

            ExitState = "ADDRESS_NT_ALLOWED_ENROLLMENT_TY";

            return;
          }

          local.CheckZip.ZipCode = export.InformationRequest.ApplicantZip5 ?? ""
            ;
          UseSiCheckZipIsNumeric();

          if (AsChar(local.NumericZip.Flag) == 'N')
          {
            var field1 = GetField(export.InformationRequest, "applicantZip5");

            field1.Error = true;

            return;
          }

          // :lss 05/14/2007 PR# 00209846  The following statements are added to
          // verify the zip4 field.
          if (Length(TrimEnd(export.InformationRequest.ApplicantZip5)) > 0 && Length
            (TrimEnd(export.InformationRequest.ApplicantZip4)) > 0)
          {
            if (Length(TrimEnd(export.InformationRequest.ApplicantZip4)) < 4)
            {
              var field1 = GetField(export.InformationRequest, "applicantZip4");

              field1.Error = true;

              ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

              return;
            }
            else if (Verify(export.InformationRequest.ApplicantZip4,
              "0123456789") != 0)
            {
              var field1 = GetField(export.InformationRequest, "applicantZip4");

              field1.Error = true;

              ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

              return;
            }
          }

          if (!Equal(import.HiddenInformationRequest.ApplicantState,
            import.InformationRequest.ApplicantState))
          {
            local.Code.CodeName = "STATE CODE";
            local.CodeValue.Cdvalue =
              import.InformationRequest.ApplicantState ?? Spaces(10);
            UseCabValidateCodeValue2();

            if (AsChar(local.RtnCode.Flag) == 'N')
            {
              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field1 =
                  GetField(export.InformationRequest, "applicantState");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_STATE_CODE";
              }
            }
          }
        }

Test2:

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
          (import.InformationRequest.Type1) == 'I' || AsChar
          (import.InformationRequest.Type1) == 'M' || AsChar
          (import.InformationRequest.Type1) == 'U' || AsChar
          (import.InformationRequest.Type1) == 'J' || AsChar
          (import.InformationRequest.Type1) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'C' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'M' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'E' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'S' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'U')
        {
        }
        else if (IsEmpty(import.InformationRequest.ApplicationSentIndicator))
        {
          var field1 =
            GetField(export.InformationRequest, "applicationSentIndicator");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
          (import.InformationRequest.Type1) == 'I' || AsChar
          (import.InformationRequest.Type1) == 'M' || AsChar
          (import.InformationRequest.Type1) == 'U' || AsChar
          (import.InformationRequest.Type1) == 'J' || AsChar
          (import.InformationRequest.Type1) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'C' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'M' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'E' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'S' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'U')
        {
        }
        else if (IsEmpty(import.InformationRequest.NonparentQuestionnaireSent) &&
          IsEmpty(import.InformationRequest.ParentQuestionnaireSent) && IsEmpty
          (import.InformationRequest.PaternityQuestionnaireSent))
        {
          var field1 =
            GetField(export.InformationRequest, "parentQuestionnaireSent");

          field1.Error = true;

          var field2 =
            GetField(export.InformationRequest, "nonparentQuestionnaireSent");

          field2.Error = true;

          var field3 =
            GetField(export.InformationRequest, "paternityQuestionnaireSent");

          field3.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (!IsEmpty(import.InformationRequest.CallerLastName))
        {
          if (IsEmpty(import.InformationRequest.CallerFirstName))
          {
            var field1 = GetField(export.InformationRequest, "callerFirstName");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              return;
            }
          }
        }

        if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
          (import.InformationRequest.Type1) == 'I' || AsChar
          (import.InformationRequest.Type1) == 'M' || AsChar
          (import.InformationRequest.Type1) == 'U' || AsChar
          (import.InformationRequest.Type1) == 'J' || AsChar
          (import.InformationRequest.Type1) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'C' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'M' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'E' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'S' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'U')
        {
        }
        else
        {
          switch(AsChar(import.InformationRequest.ApplicationSentIndicator))
          {
            case 'Y':
              if (IsEmpty(import.InformationRequest.NonparentQuestionnaireSent) &&
                IsEmpty(import.InformationRequest.ParentQuestionnaireSent) && IsEmpty
                (import.InformationRequest.PaternityQuestionnaireSent))
              {
                var field2 =
                  GetField(export.InformationRequest, "parentQuestionnaireSent");
                  

                field2.Error = true;

                var field3 =
                  GetField(export.InformationRequest,
                  "nonparentQuestionnaireSent");

                field3.Error = true;

                var field4 =
                  GetField(export.InformationRequest,
                  "paternityQuestionnaireSent");

                field4.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_NO_SELECTION_MADE";
                }
              }

              break;
            case 'N':
              break;
            case ' ':
              break;
            default:
              var field1 =
                GetField(export.InformationRequest, "applicationSentIndicator");
                

              field1.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
              }

              break;
          }
        }

        if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
          (import.InformationRequest.Type1) == 'I' || AsChar
          (import.InformationRequest.Type1) == 'M' || AsChar
          (import.InformationRequest.Type1) == 'U' || AsChar
          (import.InformationRequest.Type1) == 'J' || AsChar
          (import.InformationRequest.Type1) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'C' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'M' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'E' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'S' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'U')
        {
        }
        else
        {
          switch(AsChar(import.InformationRequest.NonparentQuestionnaireSent))
          {
            case ' ':
              break;
            case 'Y':
              if (AsChar(import.InformationRequest.ParentQuestionnaireSent) == 'Y'
                )
              {
                var field2 =
                  GetField(export.InformationRequest, "parentQuestionnaireSent");
                  

                field2.Error = true;

                var field3 =
                  GetField(export.InformationRequest,
                  "nonparentQuestionnaireSent");

                field3.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";
                }
              }

              break;
            default:
              var field1 =
                GetField(export.InformationRequest, "nonparentQuestionnaireSent");
                

              field1.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
              }

              break;
          }

          switch(AsChar(import.InformationRequest.ParentQuestionnaireSent))
          {
            case ' ':
              break;
            case 'Y':
              break;
            default:
              var field1 =
                GetField(export.InformationRequest, "parentQuestionnaireSent");

              field1.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
              }

              break;
          }
        }

        switch(AsChar(import.LocateInd.SelectChar))
        {
          case ' ':
            break;
          case 'Y':
            if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
              (import.InformationRequest.Type1) == 'I' || AsChar
              (import.InformationRequest.Type1) == 'M' || AsChar
              (import.InformationRequest.Type1) == 'U' || AsChar
              (import.InformationRequest.Type1) == 'J' || AsChar
              (import.InformationRequest.Type1) == 'F' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'C' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'M' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'E' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'F' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'S' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'U')
            {
            }
            else if (Equal(import.InformationRequest.DateReceivedByCseComplete,
              null))
            {
              var field2 =
                GetField(export.InformationRequest, "dateReceivedByCseComplete");
                

              field2.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            break;
          default:
            var field1 = GetField(export.LocateInd, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
            }

            break;
        }

        switch(AsChar(import.WithMedicalInd.SelectChar))
        {
          case ' ':
            break;
          case 'Y':
            if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
              (import.InformationRequest.Type1) == 'I' || AsChar
              (import.InformationRequest.Type1) == 'M' || AsChar
              (import.InformationRequest.Type1) == 'U' || AsChar
              (import.InformationRequest.Type1) == 'J' || AsChar
              (import.InformationRequest.Type1) == 'F' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'C' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'M' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'E' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'F' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'S' || AsChar
              (import.InformationRequest.ReopenReasonType) == 'U')
            {
            }
            else if (Equal(import.InformationRequest.DateReceivedByCseComplete,
              null))
            {
              var field2 =
                GetField(export.InformationRequest, "dateReceivedByCseComplete");
                

              field2.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            break;
          default:
            var field1 = GetField(export.WithMedicalInd, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
            }

            break;
        }

        switch(AsChar(import.LimitedInd.SelectChar))
        {
          case ' ':
            break;
          case 'Y':
            break;
          default:
            var field1 = GetField(export.LimitedInd, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
            }

            break;
        }

        if (AsChar(import.LocateInd.SelectChar) == 'Y' && AsChar
          (import.WithMedicalInd.SelectChar) == 'Y' && AsChar
          (import.LimitedInd.SelectChar) == 'Y')
        {
          var field1 = GetField(export.LimitedInd, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.WithMedicalInd, "selectChar");

          field2.Error = true;

          var field3 = GetField(export.LocateInd, "selectChar");

          field3.Error = true;

          ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";
        }

        if (Equal(global.Command, "ADD"))
        {
          if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
            (import.InformationRequest.Type1) == 'I' || AsChar
            (import.InformationRequest.Type1) == 'M' || AsChar
            (import.InformationRequest.Type1) == 'U' || AsChar
            (import.InformationRequest.Type1) == 'J' || AsChar
            (import.InformationRequest.Type1) == 'F' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'C' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'M' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'E' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'F' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'S' || AsChar
            (import.InformationRequest.ReopenReasonType) == 'U')
          {
            if (IsEmpty(import.LocateInd.SelectChar) && IsEmpty
              (import.WithMedicalInd.SelectChar) && IsEmpty
              (import.LimitedInd.SelectChar))
            {
              var field1 = GetField(export.LimitedInd, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.WithMedicalInd, "selectChar");

              field2.Error = true;

              var field3 = GetField(export.LocateInd, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            }
          }
        }
        else if (IsEmpty(import.LocateInd.SelectChar) && IsEmpty
          (import.WithMedicalInd.SelectChar) && IsEmpty
          (import.LimitedInd.SelectChar))
        {
          var field1 = GetField(export.LimitedInd, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.WithMedicalInd, "selectChar");

          field2.Error = true;

          var field3 = GetField(export.LocateInd, "selectChar");

          field3.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
          (import.InformationRequest.Type1) == 'I' || AsChar
          (import.InformationRequest.Type1) == 'M' || AsChar
          (import.InformationRequest.Type1) == 'U' || AsChar
          (import.InformationRequest.Type1) == 'J' || AsChar
          (import.InformationRequest.Type1) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'C' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'M' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'E' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'S' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'U')
        {
        }
        else if (!Equal(import.InformationRequest.DateReceivedByCseComplete,
          null))
        {
          if (IsEmpty(import.LocateInd.SelectChar) && IsEmpty
            (import.WithMedicalInd.SelectChar) && IsEmpty
            (import.LimitedInd.SelectChar))
          {
            var field1 = GetField(export.WithMedicalInd, "selectChar");

            field1.Error = true;

            var field2 = GetField(export.LocateInd, "selectChar");

            field2.Error = true;

            var field3 = GetField(export.LimitedInd, "selectChar");

            field3.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            }
          }
        }

        if (!IsEmpty(import.DeniedIncompleteCommon.SelectChar))
        {
          if (AsChar(import.DeniedIncompleteCommon.SelectChar) == 'D' || AsChar
            (import.DeniedIncompleteCommon.SelectChar) == 'I')
          {
            if (Equal(import.DeniedIncompleteInformationRequest.
              DateReceivedByCseIncomplete, null))
            {
              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field1 =
                  GetField(export.DeniedIncompleteInformationRequest,
                  "dateReceivedByCseIncomplete");

                field1.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }
            }

            if (IsEmpty(import.DeniedIncompleteInformationRequest.
              ReasonIncomplete))
            {
              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field1 =
                  GetField(export.DeniedIncompleteInformationRequest,
                  "reasonIncomplete");

                field1.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }
            }
          }
          else if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field1 = GetField(export.DeniedIncompleteCommon, "selectChar");

            field1.Error = true;

            ExitState = "INVALID_DENIED_INCOMPLETE_IND";
          }
        }

        if (AsChar(import.LocateInd.SelectChar) == 'Y')
        {
          export.InformationRequest.ServiceCode = "LO";
        }
        else if (AsChar(import.WithMedicalInd.SelectChar) == 'Y')
        {
          export.InformationRequest.ServiceCode = "WI";
        }
        else if (AsChar(import.LimitedInd.SelectChar) == 'Y')
        {
          export.InformationRequest.ServiceCode = "LS";
        }
      }

Test3:

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "APPLPROC":
        // ------------------------------------------------------------
        // 08/07/00 W.Campbell - Added PFK 24 to
        // Screen definition with new Command
        // APPLPROC.  Added logic to Pstep
        // for processing of the new Command.
        // logic added for new attribute,
        // Application Processed IND added to
        // entity type information_request.
        // Work done on PR# 100532.
        // ------------------------------------------------------------
        if (import.InformationRequest.Number != import
          .HiddenInformationRequest.Number)
        {
          var field = GetField(export.InformationRequest, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // ------------------------------------------------------------
        // 08/07/00 W.Campbell - Following READ
        // marked as Select only.
        // Work done on PR# 100532.
        // ------------------------------------------------------------
        if (ReadInformationRequest())
        {
          // ------------------------------------------------------------
          // 08/07/00 W.Campbell - PF16 ApplProc
          // also requires that the 'Date Received Complete'
          // must have a date > 00010101 to be valid.
          // Work done on PR# 100532.
          // ------------------------------------------------------------
          if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
            (import.InformationRequest.Type1) == 'I' || AsChar
            (import.InformationRequest.Type1) == 'M' || AsChar
            (import.InformationRequest.Type1) == 'U' || AsChar
            (import.InformationRequest.Type1) == 'J' || AsChar
            (import.InformationRequest.Type1) == 'F' || AsChar
            (import.InformationRequest.Type1) == 'E')
          {
          }
          else if (Equal(entities.InformationRequest.DateReceivedByCseComplete,
            local.Blank.DateReceivedByCseComplete))
          {
            var field =
              GetField(export.InformationRequest, "dateReceivedByCseComplete");

            field.Error = true;

            ExitState = "SI0000_INVALID_INQ_DT_RECVD_CMPL";

            return;
          }

          try
          {
            UpdateInformationRequest();
            export.InformationRequest.ApplicationProcessedInd = "Y";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "INQUIRY_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "INQUIRY_PV";

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
          ExitState = "INQUIRY_NF";

          return;
        }

        ExitState = "SI0000_INFO_REQ_PROCESSED";

        return;
      case "PRINT":
        // mjr
        // ----------------------------------------
        // 12/15/1998
        // Removed Escapes.  Added exitstate check
        // -----------------------------------------------------
        if (export.InformationRequest.Number == 0)
        {
          var field = GetField(export.InformationRequest, "number");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
        }

        if (IsEmpty(export.InformationRequest.ApplicantLastName))
        {
          var field = GetField(export.InformationRequest, "applicantLastName");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
        }

        if (IsEmpty(export.InformationRequest.ApplicantStreet1))
        {
          var field = GetField(export.InformationRequest, "applicantStreet1");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
        }

        if (IsEmpty(export.InformationRequest.ApplicantCity))
        {
          var field = GetField(export.InformationRequest, "applicantCity");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
        }

        if (IsEmpty(export.InformationRequest.ApplicantState))
        {
          var field = GetField(export.InformationRequest, "applicantState");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (export.InformationRequest.Number != export
          .HiddenInformationRequest.Number)
        {
          var field = GetField(export.InformationRequest, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";

          return;
        }

        if (!IsEmpty(export.DeniedIncompleteCommon.SelectChar))
        {
          local.Document.Name = "NAAPPLXX";
        }
        else
        {
          local.Document.Name = "APPLICAT";
        }

        // mjr
        // ------------------------------------------
        // 01/06/2000
        // NEXT TRAN needs to be cleared before invoking print process
        // -------------------------------------------------------
        export.HiddenNextTranInfo.Assign(local.Null1);
        export.Standard.NextTransaction = "DKEY";
        export.HiddenNextTranInfo.MiscText2 =
          TrimEnd(local.SpDocLiteral.IdDocument) + local.Document.Name;

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(local.SpDocLiteral.IdInfoRequest) + NumberToString
          (export.InformationRequest.Number, 6, 10);
        UseScCabNextTranPut2();

        // mjr---> DKEY's trancode = SRPD
        //  Can change this to do a READ instead of hardcoding
        global.NextTran = "SRPD PRINT";

        return;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 12/15/1998
        // After the document is Printed (the user may still be looking
        // at WordPerfect), control is returned here.  Any cleanup
        // processing which is necessary after a print, should be done
        // now.
        // ------------------------------------------------------------
        UseScCabNextTranGet();

        // mjr
        // ----------------------------------------------------
        // Extract identifiers from next tran
        // -------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText1,
          NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdInfoRequest));

        if (local.Position.Count <= 0)
        {
          break;
        }

        local.BatchConvertNumToText.Number15 =
          StringToNumber(Substring(
            export.HiddenNextTranInfo.MiscText1, 50, local.Position.Count +
          5, 10));
        export.InformationRequest.Number = local.BatchConvertNumToText.Number15;
        global.Command = "DISPLAY";

        break;
      case "DISPLAY":
        break;
      case "ADD":
        local.ControlTable.Identifier = "INQUIRY";

        if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
          (import.InformationRequest.Type1) == 'I' || AsChar
          (import.InformationRequest.Type1) == 'M' || AsChar
          (import.InformationRequest.Type1) == 'U' || AsChar
          (import.InformationRequest.Type1) == 'J' || AsChar
          (import.InformationRequest.Type1) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'C' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'M' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'E' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'S' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'U')
        {
        }
        else if (AsChar(import.InformationRequest.ApplicationSentIndicator) == 'Y'
          )
        {
          export.InformationRequest.DateApplicationRequested = Now().Date;

          if (Equal(import.InformationRequest.DateApplicationSent, null))
          {
            export.InformationRequest.DateApplicationSent = Now().Date;
          }
          else if (Lt(import.InformationRequest.DateApplicationSent,
            export.InformationRequest.DateApplicationRequested))
          {
            var field =
              GetField(export.InformationRequest, "dateApplicationSent");

            field.Error = true;

            ExitState = "DATE_LESS_THAN_REQUESTED_DATE";

            return;
          }

          if (Lt(Now().Date.AddDays(7),
            import.InformationRequest.DateApplicationSent))
          {
            var field =
              GetField(export.InformationRequest, "dateApplicationSent");

            field.Error = true;

            ExitState = "DATE_GREATER_THAN_CURR_PLUS_FIVE";

            return;
          }

          if (Lt(import.InformationRequest.DateReceivedByCseComplete,
            export.InformationRequest.DateApplicationSent) && !
            Equal(import.InformationRequest.DateReceivedByCseComplete, null))
          {
            var field =
              GetField(export.InformationRequest, "dateReceivedByCseComplete");

            field.Error = true;

            ExitState = "DATE_LESS_THAN_SENT_DATE";

            return;
          }

          if (Lt(import.DeniedIncompleteInformationRequest.
            DateReceivedByCseIncomplete,
            export.InformationRequest.DateApplicationSent) && !
            Equal(import.DeniedIncompleteInformationRequest.
              DateReceivedByCseIncomplete, null))
          {
            var field =
              GetField(export.DeniedIncompleteInformationRequest,
              "dateReceivedByCseIncomplete");

            field.Error = true;

            ExitState = "DATE_LESS_THAN_SENT_DATE";

            return;
          }
        }
        else
        {
          export.InformationRequest.DateApplicationRequested = null;

          if (!Equal(import.InformationRequest.DateApplicationSent, null))
          {
            var field =
              GetField(export.InformationRequest, "dateApplicationSent");

            field.Error = true;

            ExitState = "DATE_NOT_VALID_FOR_INDICATOR";

            return;
          }

          if (!Equal(import.InformationRequest.DateReceivedByCseComplete, null))
          {
            var field =
              GetField(export.InformationRequest, "dateReceivedByCseComplete");

            field.Error = true;

            ExitState = "DATE_NOT_VALID_FOR_INDICATOR";

            return;
          }

          if (!Equal(import.DeniedIncompleteInformationRequest.
            DateReceivedByCseIncomplete, null))
          {
            var field =
              GetField(export.DeniedIncompleteInformationRequest,
              "dateReceivedByCseIncomplete");

            field.Error = true;

            ExitState = "DATE_NOT_VALID_FOR_INDICATOR";

            return;
          }
        }

        if (Equal(import.InformationRequest.Note,
          import.HiddenInformationRequest.Note))
        {
          export.InformationRequest.Note = "";
        }

        UseAccessControlTable();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.InformationRequest, "number");

          field.Error = true;

          return;
        }

        export.InformationRequest.Number = local.ControlTable.LastUsedNumber;

        switch(AsChar(import.DeniedIncompleteCommon.SelectChar))
        {
          case 'D':
            export.InformationRequest.DateDenied =
              import.DeniedIncompleteInformationRequest.
                DateReceivedByCseIncomplete;
            export.InformationRequest.ReasonDenied =
              import.DeniedIncompleteInformationRequest.ReasonIncomplete ?? "";

            break;
          case 'I':
            export.InformationRequest.DateReceivedByCseIncomplete =
              import.DeniedIncompleteInformationRequest.
                DateReceivedByCseIncomplete;
            export.InformationRequest.ReasonIncomplete =
              import.DeniedIncompleteInformationRequest.ReasonIncomplete ?? "";

            break;
          case ' ':
            break;
          default:
            break;
        }

        UseSiInrdCreateInformationReq();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveInformationRequest4(export.InformationRequest,
            export.HiddenInformationRequest);
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else
        {
          UseEabRollbackCics();

          var field = GetField(export.InformationRequest, "number");

          field.Error = true;
        }

        break;
      case "UPDATE":
        if (import.InformationRequest.Number != import
          .HiddenInformationRequest.Number)
        {
          var field = GetField(export.InformationRequest, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        if (AsChar(import.InformationRequest.Type1) == 'C' || AsChar
          (import.InformationRequest.Type1) == 'I' || AsChar
          (import.InformationRequest.Type1) == 'M' || AsChar
          (import.InformationRequest.Type1) == 'U' || AsChar
          (import.InformationRequest.Type1) == 'J' || AsChar
          (import.InformationRequest.Type1) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'C' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'I' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'M' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'E' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'J' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'F' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'S' || AsChar
          (import.InformationRequest.ReopenReasonType) == 'U')
        {
        }
        else
        {
          if ((AsChar(import.InformationRequest.ApplicationSentIndicator) == 'N'
            || IsEmpty(import.InformationRequest.ApplicationSentIndicator)) && AsChar
            (import.HiddenInformationRequest.ApplicationSentIndicator) == 'Y')
          {
            var field =
              GetField(export.InformationRequest, "applicationSentIndicator");

            field.Error = true;

            ExitState = "INVALID_CHANGE";

            return;
          }

          if (AsChar(import.InformationRequest.ApplicationSentIndicator) == 'Y')
          {
            if (AsChar(import.HiddenInformationRequest.ApplicationSentIndicator) ==
              'N' || IsEmpty
              (import.HiddenInformationRequest.ApplicationSentIndicator))
            {
              export.InformationRequest.DateApplicationRequested = Now().Date;

              if (Equal(import.InformationRequest.DateApplicationSent, null))
              {
                export.InformationRequest.DateApplicationSent = Now().Date;
              }
            }
            else if (!Equal(import.InformationRequest.DateApplicationSent,
              import.HiddenInformationRequest.DateApplicationSent))
            {
              if (Lt(import.InformationRequest.DateApplicationSent, Now().Date))
              {
                var field =
                  GetField(export.InformationRequest, "dateApplicationSent");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

                return;
              }
            }

            if (Lt(Now().Date.AddDays(7),
              import.InformationRequest.DateApplicationSent))
            {
              var field =
                GetField(export.InformationRequest, "dateApplicationSent");

              field.Error = true;

              ExitState = "DATE_GREATER_THAN_CURR_PLUS_FIVE";

              return;
            }

            if (Lt(import.InformationRequest.DateReceivedByCseComplete,
              export.InformationRequest.DateApplicationSent) && !
              Equal(import.InformationRequest.DateReceivedByCseComplete, null))
            {
              var field =
                GetField(export.InformationRequest, "dateReceivedByCseComplete");
                

              field.Error = true;

              ExitState = "DATE_LESS_THAN_SENT_DATE";

              return;
            }

            if (Lt(import.DeniedIncompleteInformationRequest.
              DateReceivedByCseIncomplete,
              export.InformationRequest.DateApplicationSent) && !
              Equal(import.DeniedIncompleteInformationRequest.
                DateReceivedByCseIncomplete, null))
            {
              var field =
                GetField(export.DeniedIncompleteInformationRequest,
                "dateReceivedByCseIncomplete");

              field.Error = true;

              ExitState = "DATE_LESS_THAN_SENT_DATE";

              return;
            }
          }
          else
          {
            if (!Equal(import.InformationRequest.DateApplicationSent, null))
            {
              var field =
                GetField(export.InformationRequest, "dateApplicationSent");

              field.Error = true;

              ExitState = "DATE_NOT_VALID_FOR_INDICATOR";

              return;
            }

            if (!Equal(import.InformationRequest.DateReceivedByCseComplete, null))
              
            {
              var field =
                GetField(export.InformationRequest, "dateReceivedByCseComplete");
                

              field.Error = true;

              ExitState = "DATE_NOT_VALID_FOR_INDICATOR";

              return;
            }

            if (!Equal(import.DeniedIncompleteInformationRequest.
              DateReceivedByCseIncomplete, null))
            {
              var field =
                GetField(export.DeniedIncompleteInformationRequest,
                "dateReceivedByCseIncomplete");

              field.Error = true;

              ExitState = "DATE_NOT_VALID_FOR_INDICATOR";

              return;
            }
          }

          if (!Equal(import.InformationRequest.DateApplicationRequested,
            import.HiddenInformationRequest.DateApplicationRequested))
          {
            if (Lt(import.InformationRequest.DateApplicationRequested,
              Now().Date))
            {
              var field =
                GetField(export.InformationRequest, "dateApplicationRequested");
                

              field.Error = true;

              ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

              return;
            }
          }
        }

        switch(AsChar(import.DeniedIncompleteCommon.SelectChar))
        {
          case 'D':
            export.InformationRequest.DateDenied =
              import.DeniedIncompleteInformationRequest.
                DateReceivedByCseIncomplete;
            export.InformationRequest.ReasonDenied =
              import.DeniedIncompleteInformationRequest.ReasonIncomplete ?? "";

            break;
          case 'I':
            export.InformationRequest.DateReceivedByCseIncomplete =
              import.DeniedIncompleteInformationRequest.
                DateReceivedByCseIncomplete;
            export.InformationRequest.ReasonIncomplete =
              import.DeniedIncompleteInformationRequest.ReasonIncomplete ?? "";

            break;
          case ' ':
            break;
          default:
            break;
        }

        UseSiInrdUpdateInformationReq();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
          var field = GetField(export.InformationRequest, "number");

          field.Error = true;

          UseEabRollbackCics();

          return;
        }

        break;
      case "DELETE":
        // ------------------------------------------------------------
        // 08/07/00 W.Campbell - Added following
        // IF stmt to insure that a display is done
        // before a Delete.
        // Work done on PR# 100532.
        // ------------------------------------------------------------
        if (import.InformationRequest.Number != import
          .HiddenInformationRequest.Number)
        {
          var field = GetField(export.InformationRequest, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        UseSiInrdDeleteInformationReq();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveInformationRequest1(local.Blank, export.InformationRequest);
          export.LocateInd.SelectChar = "";
          export.WithMedicalInd.SelectChar = "";
          export.DeniedIncompleteInformationRequest.
            DateReceivedByCseIncomplete = null;
          export.DeniedIncompleteInformationRequest.ReasonIncomplete = "";
          export.DeniedIncompleteCommon.SelectChar = "";

          var field = GetField(export.InformationRequest, "number");

          field.Protected = false;
          field.Focused = true;

          ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
        }
        else
        {
          var field = GetField(export.InformationRequest, "number");

          field.Error = true;

          UseEabRollbackCics();

          return;
        }

        break;
      case "LIST":
        switch(AsChar(import.StatePrompt.PromptField))
        {
          case ' ':
            break;
          case 'S':
            export.Code.CodeName = "STATE CODE";
            local.SelectCnt.Count = 1;
            ExitState = "ECO_LNK_TO_CODE_TABLES";

            break;
          default:
            var field = GetField(export.StatePrompt, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(import.TypePrompt.PromptField))
        {
          case ' ':
            break;
          case 'S':
            export.Code.CodeName = "INFORMATION REQUEST TYPE";
            ++local.SelectCnt.Count;
            ExitState = "ECO_LNK_TO_CODE_TABLES";

            break;
          default:
            var field = GetField(export.TypePrompt, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(import.ReopenType.PromptField))
        {
          case ' ':
            break;
          case 'S':
            export.Code.CodeName = "INFORMATION REQUEST REOPEN RSN";
            ++local.SelectCnt.Count;
            ExitState = "ECO_LNK_TO_CODE_TABLES";

            break;
          default:
            var field = GetField(export.ReopenTypr, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        if (local.SelectCnt.Count < 1)
        {
          // --------------------------------------
          // 05/26/99 W.Campbell - Replaced zd exit states.
          // --------------------------------------
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else if (local.SelectCnt.Count > 1)
        {
          var field1 = GetField(export.TypePrompt, "promptField");

          field1.Error = true;

          var field2 = GetField(export.StatePrompt, "promptField");

          field2.Error = true;

          var field3 = GetField(export.ReopenTypr, "promptField");

          field3.Error = true;

          // --------------------------------------
          // 05/26/99 W.Campbell - Replaced zd exit states.
          // --------------------------------------
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }

        break;
      case "RTLIST":
        if (AsChar(import.StatePrompt.PromptField) == 'S')
        {
          if (!IsEmpty(import.CodeValue.Cdvalue))
          {
            export.InformationRequest.ApplicantState = import.CodeValue.Cdvalue;
          }

          export.StatePrompt.PromptField = "";

          var field = GetField(export.StatePrompt, "promptField");

          field.Protected = false;
          field.Focused = true;
        }
        else if (AsChar(import.TypePrompt.PromptField) == 'S')
        {
          if (!IsEmpty(import.CodeValue.Cdvalue))
          {
            export.InformationRequest.Type1 = import.CodeValue.Cdvalue;
          }

          export.TypePrompt.PromptField = "";

          var field = GetField(export.TypePrompt, "promptField");

          field.Protected = false;
          field.Focused = true;
        }
        else if (AsChar(export.ReopenTypr.PromptField) == 'S')
        {
          if (!IsEmpty(import.CodeValue.Cdvalue))
          {
            export.InformationRequest.ReopenReasonType =
              import.CodeValue.Cdvalue;
          }

          export.ReopenTypr.PromptField = "";

          var field = GetField(export.ReopenTypr, "promptField");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "NMSRCH":
        export.Namelist.FirstName =
          export.InformationRequest.ApplicantFirstName ?? Spaces(12);
        export.Namelist.LastName =
          export.InformationRequest.ApplicantLastName ?? Spaces(17);
        export.Namelist.MiddleInitial =
          export.InformationRequest.ApplicantMiddleInitial ?? Spaces(1);
        export.Phonetic.Flag = "N";
        export.FromInrd.Flag = "Y";
        export.Phonetic.Percentage = 100;
        ExitState = "ECO_XFR_TO_NAME_LIST";

        break;
      case "EXIT":
        if (!IsEmpty(import.InformationRequest.ApplicantLastName))
        {
          MoveInformationRequest6(import.InformationRequest, export.Menu);
        }
        else
        {
          export.Menu.ApplicantFirstName =
            import.InformationRequest.CallerFirstName;
          export.Menu.ApplicantLastName =
            import.InformationRequest.CallerLastName ?? "";
          export.Menu.ApplicantMiddleInitial =
            import.InformationRequest.CallerMiddleInitial;
          export.Menu.Number = import.InformationRequest.Number;
        }

        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        if (AsChar(import.ReopenFromComn.Text1) == 'Y')
        {
          ExitState = "RETURN_FROM_LINK";
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // mjr
    // -----------------------------------------
    // 12/15/1998
    // Pulled command Display out of main case of command
    // construct so it would be executed after a PrintRet.
    // ------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      // ------------------------------------------------------------
      // 03/05/99 W.Campbell - Changed the view
      // matching on the following USE stmt so that
      // the import view information_request of called
      // CAB SI_INRD_READ_INFORMATION_REQ is
      // matched to the export view information_req
      // of this PRAD.
      // ------------------------------------------------------------
      if (export.InformationRequest.Number > 0)
      {
        UseSiInrdReadInformationReq();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.InformationRequest, "number");

          field1.Error = true;

          return;
        }
        else
        {
          export.WorkerId.Text8 = export.InformationRequest.LastUpdatedBy ?? Spaces
            (8);

          // mjr
          // -----------------------------------------------
          // 12/15/1998
          // Added check for an exitstate returned from Print
          // ------------------------------------------------------------
          local.Position.Count =
            Find(String(
              export.HiddenNextTranInfo.MiscText2,
            NextTranInfo.MiscText2_MaxLength),
            TrimEnd(local.SpDocLiteral.IdDocument));

          if (local.Position.Count <= 0)
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
          else
          {
            // mjr---> Determines the appropriate exitstate for the Print 
            // process
            local.WorkArea.Text50 = export.HiddenNextTranInfo.MiscText2 ?? Spaces
              (50);
            UseSpPrintDecodeReturnCode();
            export.HiddenNextTranInfo.MiscText2 = local.WorkArea.Text50;
          }
        }

        if (AsChar(import.ReopenFromComn.Text1) == 'Y')
        {
          if (ReadCase2())
          {
            export.Case1.Number = entities.Case1.Number;
          }
          else
          {
            ExitState = "CASE_NF";

            var field1 = GetField(export.Case1, "number");

            field1.Error = true;

            return;
          }

          local.Ar.Number = "";

          if (!IsEmpty(import.Ar.Number))
          {
            local.Ar.Number = "";

            // this is from comn, could be the AR or AP
            if (ReadCaseRoleCsePerson2())
            {
              local.Ar.Number = entities.CsePerson.Number;
            }
          }
          else
          {
            // we do not know what type of reopen this is and we do not have a 
            // person nummber so we
            // will get the last know AR for the case.
            if (ReadCaseRoleCsePerson6())
            {
              local.Ar.Number = entities.CsePerson.Number;
            }
          }

          if (IsEmpty(local.Ar.Number))
          {
            // no one found so do not try to find address
            return;
          }

          if (IsEmpty(export.InformationRequest.ApplicantStreet1))
          {
            local.CsePerson.Number = local.Ar.Number;
            UseFnCabReadCsePersonAddress();
            export.InformationRequest.ApplicantStreet1 =
              local.CsePersonAddress.Street1 ?? "";
            export.InformationRequest.ApplicantStreet2 =
              local.CsePersonAddress.Street2 ?? "";
            export.InformationRequest.ApplicantCity =
              local.CsePersonAddress.City ?? "";
            export.InformationRequest.ApplicantState =
              local.CsePersonAddress.State ?? "";
            export.InformationRequest.ApplicantZip5 =
              local.CsePersonAddress.ZipCode ?? "";
            export.InformationRequest.ApplicantZip4 =
              local.CsePersonAddress.Zip4 ?? "";
          }

          local.CsePersonsWorkSet.Number = local.Ar.Number;
          UseSiReadCsePerson();

          if (AsChar(export.CsePerson.Type1) == 'C')
          {
            export.InformationRequest.ApplicantFirstName =
              local.Returned.FirstName;
            export.InformationRequest.ApplicantLastName =
              local.Returned.LastName;
            export.InformationRequest.ApplicantMiddleInitial =
              local.Returned.MiddleInitial;
          }
          else
          {
            export.InformationRequest.ApplicantFirstName = "";
            export.InformationRequest.ApplicantLastName = "";
            export.InformationRequest.ApplicantMiddleInitial = "";
          }

          export.InformationRequest.Type1 = "R";
        }
        else
        {
          // not from comn
          if (ReadCase1())
          {
            export.Case1.Number = entities.Case1.Number;
          }
          else
          {
            export.Case1.Number = "";
          }

          if (!IsEmpty(export.Case1.Number))
          {
            if (AsChar(export.InformationRequest.Type1) == 'P' || AsChar
              (export.InformationRequest.Type1) == 'R' && AsChar
              (export.InformationRequest.ReopenReasonType) == 'P')
            {
              local.Ar.Number = "";

              if (!IsEmpty(import.Ar.Number))
              {
                local.Ar.Number = "";

                // this is from comn, could be the AR or AP
                if (ReadCaseRoleCsePerson1())
                {
                  local.Ar.Number = entities.CsePerson.Number;
                }
              }
              else
              {
                // this is an AP open/reopen
                if (ReadCaseRoleCsePerson3())
                {
                  local.Ar.Number = entities.CsePerson.Number;
                }
              }

              if (IsEmpty(local.Ar.Number))
              {
                // no one found so do not try to find address
                return;
              }

              if (IsEmpty(export.InformationRequest.ApplicantStreet1))
              {
                local.CsePerson.Number = local.Ar.Number;
                UseFnCabReadCsePersonAddress();
                export.InformationRequest.ApplicantStreet1 =
                  local.CsePersonAddress.Street1 ?? "";
                export.InformationRequest.ApplicantStreet2 =
                  local.CsePersonAddress.Street2 ?? "";
                export.InformationRequest.ApplicantCity =
                  local.CsePersonAddress.City ?? "";
                export.InformationRequest.ApplicantState =
                  local.CsePersonAddress.State ?? "";
                export.InformationRequest.ApplicantZip5 =
                  local.CsePersonAddress.ZipCode ?? "";
                export.InformationRequest.ApplicantZip4 =
                  local.CsePersonAddress.Zip4 ?? "";
              }

              local.CsePersonsWorkSet.Number = local.Ar.Number;
              UseSiReadCsePerson();

              if (AsChar(export.CsePerson.Type1) == 'C')
              {
                export.InformationRequest.ApplicantFirstName =
                  local.Returned.FirstName;
                export.InformationRequest.ApplicantLastName =
                  local.Returned.LastName;
                export.InformationRequest.ApplicantMiddleInitial =
                  local.Returned.MiddleInitial;
              }
              else
              {
                export.InformationRequest.ApplicantFirstName = "";
                export.InformationRequest.ApplicantLastName = "";
                export.InformationRequest.ApplicantMiddleInitial = "";
              }
            }
            else
            {
              // if there is no address for the ar, then populate it
              if (AsChar(export.InformationRequest.Type1) == 'I' || AsChar
                (export.InformationRequest.Type1) == 'J' || AsChar
                (export.InformationRequest.Type1) == 'F' || AsChar
                (export.InformationRequest.ReopenReasonType) == 'I' || AsChar
                (export.InformationRequest.ReopenReasonType) == 'J' || AsChar
                (export.InformationRequest.ReopenReasonType) == 'F')
              {
              }
              else
              {
                local.Ar.Number = "";

                if (ReadCaseRoleCsePerson7())
                {
                  local.Ar.Number = entities.CsePerson.Number;
                }

                if (IsEmpty(local.Ar.Number))
                {
                  // no one found so do not try to find address
                  return;
                }

                if (IsEmpty(export.InformationRequest.ApplicantStreet1))
                {
                  local.CsePerson.Number = local.Ar.Number;
                  UseFnCabReadCsePersonAddress();
                  export.InformationRequest.ApplicantStreet1 =
                    local.CsePersonAddress.Street1 ?? "";
                  export.InformationRequest.ApplicantStreet2 =
                    local.CsePersonAddress.Street2 ?? "";
                  export.InformationRequest.ApplicantCity =
                    local.CsePersonAddress.City ?? "";
                  export.InformationRequest.ApplicantState =
                    local.CsePersonAddress.State ?? "";
                  export.InformationRequest.ApplicantZip5 =
                    local.CsePersonAddress.ZipCode ?? "";
                  export.InformationRequest.ApplicantZip4 =
                    local.CsePersonAddress.Zip4 ?? "";
                }
              }

              local.Ar.Number = "";

              if (!IsEmpty(import.Ar.Number))
              {
                // this is from comn, could be the AR or AP
                if (ReadCaseRoleCsePerson1())
                {
                  local.Ar.Number = entities.CsePerson.Number;
                }
              }
              else if (ReadCaseRoleCsePerson5())
              {
                local.Ar.Number = entities.CsePerson.Number;
              }

              if (IsEmpty(local.Ar.Number))
              {
                // no one found so do not try to find address
                return;
              }

              local.CsePersonsWorkSet.Number = local.Ar.Number;
              UseSiReadCsePerson();

              if (AsChar(export.CsePerson.Type1) == 'C')
              {
                export.InformationRequest.ApplicantFirstName =
                  local.Returned.FirstName;
                export.InformationRequest.ApplicantLastName =
                  local.Returned.LastName;
                export.InformationRequest.ApplicantMiddleInitial =
                  local.Returned.MiddleInitial;
              }
              else
              {
                export.InformationRequest.ApplicantFirstName = "";
                export.InformationRequest.ApplicantLastName = "";
                export.InformationRequest.ApplicantMiddleInitial = "";
              }
            }
          }
        }
      }
      else
      {
        // no information req num
        // if we flowed from comn, so this could be a new information request 
        // record. need to populate the screen
        if (!IsEmpty(import.Case1.Number))
        {
          if (ReadCase2())
          {
            export.Case1.Number = entities.Case1.Number;

            // this could be the AR or AP
            local.Ar.Number = "";

            if (!IsEmpty(import.Ar.Number))
            {
              if (ReadCaseRoleCsePerson2())
              {
                local.Ar.Number = entities.CsePerson.Number;
              }
            }
            else if (ReadCaseRoleCsePerson6())
            {
              local.Ar.Number = entities.CsePerson.Number;
            }

            if (IsEmpty(local.Ar.Number))
            {
              // no one found so do not try to find address
              return;
            }

            if (IsEmpty(export.InformationRequest.ApplicantStreet1))
            {
              if (AsChar(export.InformationRequest.Type1) == 'I' || AsChar
                (export.InformationRequest.Type1) == 'J' || AsChar
                (export.InformationRequest.Type1) == 'F' || AsChar
                (export.InformationRequest.ReopenReasonType) == 'I' || AsChar
                (export.InformationRequest.ReopenReasonType) == 'J' || AsChar
                (export.InformationRequest.ReopenReasonType) == 'F')
              {
              }
              else
              {
                local.CsePerson.Number = local.Ar.Number;
                UseFnCabReadCsePersonAddress();
                export.InformationRequest.ApplicantStreet1 =
                  local.CsePersonAddress.Street1 ?? "";
                export.InformationRequest.ApplicantStreet2 =
                  local.CsePersonAddress.Street2 ?? "";
                export.InformationRequest.ApplicantCity =
                  local.CsePersonAddress.City ?? "";
                export.InformationRequest.ApplicantState =
                  local.CsePersonAddress.State ?? "";
                export.InformationRequest.ApplicantZip5 =
                  local.CsePersonAddress.ZipCode ?? "";
                export.InformationRequest.ApplicantZip4 =
                  local.CsePersonAddress.Zip4 ?? "";
              }
            }

            local.CsePersonsWorkSet.Number = local.Ar.Number;
            UseSiReadCsePerson();

            if (AsChar(export.CsePerson.Type1) == 'C')
            {
              export.InformationRequest.ApplicantFirstName =
                local.Returned.FirstName;
              export.InformationRequest.ApplicantLastName =
                local.Returned.LastName;
              export.InformationRequest.ApplicantMiddleInitial =
                local.Returned.MiddleInitial;
            }
            else
            {
              export.InformationRequest.ApplicantFirstName = "";
              export.InformationRequest.ApplicantLastName = "";
              export.InformationRequest.ApplicantMiddleInitial = "";
            }

            export.InformationRequest.Type1 = "R";
          }
          else
          {
            ExitState = "CASE_NF";

            var field1 = GetField(export.Case1, "number");

            field1.Error = true;

            return;
          }
        }
      }

      export.LocateInd.SelectChar = local.LocateInd.SelectChar;
      export.WithMedicalInd.SelectChar = local.WithMedicalInd.SelectChar;
      export.DeniedIncompleteCommon.SelectChar =
        local.DeniedIncompleteCommon.SelectChar;
      MoveInformationRequest5(local.DeniedIncompleteInformationRequest,
        export.DeniedIncompleteInformationRequest);
      MoveInformationRequest4(export.InformationRequest,
        export.HiddenInformationRequest);

      var field = GetField(export.InformationRequest, "number");

      field.Protected = false;
      field.Focused = true;

      switch(TrimEnd(export.InformationRequest.ServiceCode ?? ""))
      {
        case "LO":
          export.LocateInd.SelectChar = "Y";

          break;
        case "WI":
          export.WithMedicalInd.SelectChar = "Y";

          break;
        default:
          break;
      }

      if (!Equal(export.InformationRequest.DateDenied, null))
      {
        export.DeniedIncompleteCommon.SelectChar = "D";
        export.DeniedIncompleteInformationRequest.DateReceivedByCseIncomplete =
          export.InformationRequest.DateDenied;
        export.DeniedIncompleteInformationRequest.ReasonIncomplete =
          export.InformationRequest.ReasonDenied ?? "";
      }
      else if (!Equal(export.InformationRequest.DateReceivedByCseIncomplete,
        null))
      {
        export.DeniedIncompleteCommon.SelectChar = "I";
        export.DeniedIncompleteInformationRequest.DateReceivedByCseIncomplete =
          export.InformationRequest.DateReceivedByCseIncomplete;
        export.DeniedIncompleteInformationRequest.ReasonIncomplete =
          export.InformationRequest.ReasonIncomplete ?? "";
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.OrganizationName = source.OrganizationName;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveInformationRequest1(InformationRequest source,
    InformationRequest target)
  {
    target.CreatedBy = source.CreatedBy;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.ReasonIncomplete = source.ReasonIncomplete;
    target.ServiceCode = source.ServiceCode;
    target.NonparentQuestionnaireSent = source.NonparentQuestionnaireSent;
    target.ParentQuestionnaireSent = source.ParentQuestionnaireSent;
    target.PaternityQuestionnaireSent = source.PaternityQuestionnaireSent;
    target.ApplicationSentIndicator = source.ApplicationSentIndicator;
    target.QuestionnaireTypeIndicator = source.QuestionnaireTypeIndicator;
    target.DateReceivedByCseComplete = source.DateReceivedByCseComplete;
    target.DateReceivedByCseIncomplete = source.DateReceivedByCseIncomplete;
    target.Number = source.Number;
    target.DateApplicationRequested = source.DateApplicationRequested;
    target.CallerLastName = source.CallerLastName;
    target.CallerFirstName = source.CallerFirstName;
    target.CallerMiddleInitial = source.CallerMiddleInitial;
    target.ApplicantLastName = source.ApplicantLastName;
    target.ApplicantFirstName = source.ApplicantFirstName;
    target.ApplicantMiddleInitial = source.ApplicantMiddleInitial;
    target.ApplicantStreet1 = source.ApplicantStreet1;
    target.ApplicantStreet2 = source.ApplicantStreet2;
    target.ApplicantCity = source.ApplicantCity;
    target.ApplicantState = source.ApplicantState;
    target.ApplicantZip5 = source.ApplicantZip5;
    target.ApplicantZip4 = source.ApplicantZip4;
    target.ApplicantZip3 = source.ApplicantZip3;
    target.ApplicantPhone = source.ApplicantPhone;
    target.DateApplicationSent = source.DateApplicationSent;
    target.Type1 = source.Type1;
    target.Note = source.Note;
    target.ReasonDenied = source.ReasonDenied;
    target.DateDenied = source.DateDenied;
    target.ApplicantAreaCode = source.ApplicantAreaCode;
    target.ReopenReasonType = source.ReopenReasonType;
    target.MiscellaneousReason = source.MiscellaneousReason;
  }

  private static void MoveInformationRequest2(InformationRequest source,
    InformationRequest target)
  {
    target.CreatedBy = source.CreatedBy;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.ReasonIncomplete = source.ReasonIncomplete;
    target.ServiceCode = source.ServiceCode;
    target.NonparentQuestionnaireSent = source.NonparentQuestionnaireSent;
    target.ParentQuestionnaireSent = source.ParentQuestionnaireSent;
    target.PaternityQuestionnaireSent = source.PaternityQuestionnaireSent;
    target.ApplicationSentIndicator = source.ApplicationSentIndicator;
    target.QuestionnaireTypeIndicator = source.QuestionnaireTypeIndicator;
    target.DateReceivedByCseComplete = source.DateReceivedByCseComplete;
    target.DateReceivedByCseIncomplete = source.DateReceivedByCseIncomplete;
    target.Number = source.Number;
    target.DateApplicationRequested = source.DateApplicationRequested;
    target.CallerLastName = source.CallerLastName;
    target.CallerFirstName = source.CallerFirstName;
    target.CallerMiddleInitial = source.CallerMiddleInitial;
    target.ApplicantLastName = source.ApplicantLastName;
    target.ApplicantFirstName = source.ApplicantFirstName;
    target.ApplicantMiddleInitial = source.ApplicantMiddleInitial;
    target.ApplicantStreet1 = source.ApplicantStreet1;
    target.ApplicantStreet2 = source.ApplicantStreet2;
    target.ApplicantCity = source.ApplicantCity;
    target.ApplicantState = source.ApplicantState;
    target.ApplicantZip5 = source.ApplicantZip5;
    target.ApplicantZip4 = source.ApplicantZip4;
    target.ApplicantZip3 = source.ApplicantZip3;
    target.ApplicantPhone = source.ApplicantPhone;
    target.DateApplicationSent = source.DateApplicationSent;
    target.Type1 = source.Type1;
    target.Note = source.Note;
    target.ReasonDenied = source.ReasonDenied;
    target.DateDenied = source.DateDenied;
    target.ApplicantAreaCode = source.ApplicantAreaCode;
    target.ApplicationProcessedInd = source.ApplicationProcessedInd;
    target.ReopenReasonType = source.ReopenReasonType;
    target.MiscellaneousReason = source.MiscellaneousReason;
  }

  private static void MoveInformationRequest3(InformationRequest source,
    InformationRequest target)
  {
    target.CreatedBy = source.CreatedBy;
    target.ReasonIncomplete = source.ReasonIncomplete;
    target.ServiceCode = source.ServiceCode;
    target.NonparentQuestionnaireSent = source.NonparentQuestionnaireSent;
    target.ParentQuestionnaireSent = source.ParentQuestionnaireSent;
    target.PaternityQuestionnaireSent = source.PaternityQuestionnaireSent;
    target.ApplicationSentIndicator = source.ApplicationSentIndicator;
    target.QuestionnaireTypeIndicator = source.QuestionnaireTypeIndicator;
    target.DateReceivedByCseComplete = source.DateReceivedByCseComplete;
    target.DateReceivedByCseIncomplete = source.DateReceivedByCseIncomplete;
    target.Number = source.Number;
    target.DateApplicationRequested = source.DateApplicationRequested;
    target.CallerLastName = source.CallerLastName;
    target.CallerFirstName = source.CallerFirstName;
    target.CallerMiddleInitial = source.CallerMiddleInitial;
    target.ApplicantLastName = source.ApplicantLastName;
    target.ApplicantFirstName = source.ApplicantFirstName;
    target.ApplicantMiddleInitial = source.ApplicantMiddleInitial;
    target.ApplicantStreet1 = source.ApplicantStreet1;
    target.ApplicantStreet2 = source.ApplicantStreet2;
    target.ApplicantCity = source.ApplicantCity;
    target.ApplicantState = source.ApplicantState;
    target.ApplicantZip5 = source.ApplicantZip5;
    target.ApplicantZip4 = source.ApplicantZip4;
    target.ApplicantZip3 = source.ApplicantZip3;
    target.ApplicantPhone = source.ApplicantPhone;
    target.DateApplicationSent = source.DateApplicationSent;
    target.Type1 = source.Type1;
    target.Note = source.Note;
    target.ReasonDenied = source.ReasonDenied;
    target.DateDenied = source.DateDenied;
    target.ApplicantAreaCode = source.ApplicantAreaCode;
    target.ApplicationProcessedInd = source.ApplicationProcessedInd;
    target.ReopenReasonType = source.ReopenReasonType;
    target.MiscellaneousReason = source.MiscellaneousReason;
  }

  private static void MoveInformationRequest4(InformationRequest source,
    InformationRequest target)
  {
    target.CreatedBy = source.CreatedBy;
    target.ServiceCode = source.ServiceCode;
    target.ApplicationSentIndicator = source.ApplicationSentIndicator;
    target.DateReceivedByCseComplete = source.DateReceivedByCseComplete;
    target.DateReceivedByCseIncomplete = source.DateReceivedByCseIncomplete;
    target.Number = source.Number;
    target.DateApplicationRequested = source.DateApplicationRequested;
    target.ApplicantLastName = source.ApplicantLastName;
    target.ApplicantFirstName = source.ApplicantFirstName;
    target.ApplicantMiddleInitial = source.ApplicantMiddleInitial;
    target.ApplicantStreet1 = source.ApplicantStreet1;
    target.ApplicantStreet2 = source.ApplicantStreet2;
    target.ApplicantCity = source.ApplicantCity;
    target.ApplicantState = source.ApplicantState;
    target.ApplicantZip5 = source.ApplicantZip5;
    target.ApplicantZip4 = source.ApplicantZip4;
    target.ApplicantZip3 = source.ApplicantZip3;
    target.ApplicantPhone = source.ApplicantPhone;
    target.DateApplicationSent = source.DateApplicationSent;
    target.Type1 = source.Type1;
    target.Note = source.Note;
    target.ApplicantAreaCode = source.ApplicantAreaCode;
    target.ReopenReasonType = source.ReopenReasonType;
    target.MiscellaneousReason = source.MiscellaneousReason;
  }

  private static void MoveInformationRequest5(InformationRequest source,
    InformationRequest target)
  {
    target.ReasonIncomplete = source.ReasonIncomplete;
    target.DateReceivedByCseIncomplete = source.DateReceivedByCseIncomplete;
  }

  private static void MoveInformationRequest6(InformationRequest source,
    InformationRequest target)
  {
    target.Number = source.Number;
    target.ApplicantLastName = source.ApplicantLastName;
    target.ApplicantFirstName = source.ApplicantFirstName;
    target.ApplicantMiddleInitial = source.ApplicantMiddleInitial;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdInfoRequest = source.IdInfoRequest;
  }

  private static void MoveWorkArea(WorkArea source, WorkArea target)
  {
    target.Text1 = source.Text1;
    target.Text20 = source.Text20;
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.RtnCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnCabReadCsePersonAddress()
  {
    var useImport = new FnCabReadCsePersonAddress.Import();
    var useExport = new FnCabReadCsePersonAddress.Export();

    useImport.CsePersonsWorkSet.Number = import.Ar.Number;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(FnCabReadCsePersonAddress.Execute, useImport, useExport);

    local.AddressFound.Flag = useExport.AddressFound.Flag;
    MoveCsePersonAddress1(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity1()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity2()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.Case1.Number = local.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCheckZipIsNumeric()
  {
    var useImport = new SiCheckZipIsNumeric.Import();
    var useExport = new SiCheckZipIsNumeric.Export();

    MoveCsePersonAddress2(local.CheckZip, useImport.CsePersonAddress);

    Call(SiCheckZipIsNumeric.Execute, useImport, useExport);

    local.NumericZip.Flag = useExport.NumericZip.Flag;
  }

  private void UseSiInrdCreateInformationReq()
  {
    var useImport = new SiInrdCreateInformationReq.Import();
    var useExport = new SiInrdCreateInformationReq.Export();

    MoveInformationRequest2(export.InformationRequest,
      useImport.InformationRequest);

    Call(SiInrdCreateInformationReq.Execute, useImport, useExport);
  }

  private void UseSiInrdDeleteInformationReq()
  {
    var useImport = new SiInrdDeleteInformationReq.Import();
    var useExport = new SiInrdDeleteInformationReq.Export();

    useImport.InformationRequest.Number = export.InformationRequest.Number;

    Call(SiInrdDeleteInformationReq.Execute, useImport, useExport);
  }

  private void UseSiInrdReadInformationReq()
  {
    var useImport = new SiInrdReadInformationReq.Import();
    var useExport = new SiInrdReadInformationReq.Export();

    useImport.InformationRequest.Number = export.InformationRequest.Number;

    Call(SiInrdReadInformationReq.Execute, useImport, useExport);

    export.InformationRequest.Assign(useExport.InformationRequest);
  }

  private void UseSiInrdUpdateInformationReq()
  {
    var useImport = new SiInrdUpdateInformationReq.Import();
    var useExport = new SiInrdUpdateInformationReq.Export();

    MoveInformationRequest2(export.InformationRequest,
      useImport.InformationRequest);

    Call(SiInrdUpdateInformationReq.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Returned.Assign(useExport.CsePersonsWorkSet);
    MoveCsePerson(useExport.CsePerson, export.CsePerson);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.WorkArea.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.WorkArea.Text50 = useExport.WorkArea.Text50;
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetInt64(command, "numb", export.InformationRequest.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.InformationRequestNumber =
          db.GetNullableInt64(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.InformationRequestNumber =
          db.GetNullableInt64(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ar.Number);
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ar.Number);
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson3()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson4()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson5()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson6()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson6",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson7()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson7",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Number = db.GetString(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadInformationRequest()
  {
    entities.InformationRequest.Populated = false;

    return Read("ReadInformationRequest",
      (db, command) =>
      {
        db.SetInt64(command, "numb", export.InformationRequest.Number);
      },
      (db, reader) =>
      {
        entities.InformationRequest.Number = db.GetInt64(reader, 0);
        entities.InformationRequest.DateReceivedByCseComplete =
          db.GetNullableDate(reader, 1);
        entities.InformationRequest.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.InformationRequest.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.InformationRequest.ApplicationProcessedInd =
          db.GetNullableString(reader, 4);
        entities.InformationRequest.FkCktCasenumb = db.GetString(reader, 5);
        entities.InformationRequest.Populated = true;
      });
  }

  private void UpdateInformationRequest()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var applicationProcessedInd = "Y";

    entities.InformationRequest.Populated = false;
    Update("UpdateInformationRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "applProcInd", applicationProcessedInd);
        db.SetInt64(command, "numb", entities.InformationRequest.Number);
      });

    entities.InformationRequest.LastUpdatedBy = lastUpdatedBy;
    entities.InformationRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InformationRequest.ApplicationProcessedInd =
      applicationProcessedInd;
    entities.InformationRequest.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ReopenType.
    /// </summary>
    [JsonPropertyName("reopenType")]
    public Standard ReopenType
    {
      get => reopenType ??= new();
      set => reopenType = value;
    }

    /// <summary>
    /// A value of ReopenFromComn.
    /// </summary>
    [JsonPropertyName("reopenFromComn")]
    public WorkArea ReopenFromComn
    {
      get => reopenFromComn ??= new();
      set => reopenFromComn = value;
    }

    /// <summary>
    /// A value of DeniedIncompleteInformationRequest.
    /// </summary>
    [JsonPropertyName("deniedIncompleteInformationRequest")]
    public InformationRequest DeniedIncompleteInformationRequest
    {
      get => deniedIncompleteInformationRequest ??= new();
      set => deniedIncompleteInformationRequest = value;
    }

    /// <summary>
    /// A value of DeniedIncompleteCommon.
    /// </summary>
    [JsonPropertyName("deniedIncompleteCommon")]
    public Common DeniedIncompleteCommon
    {
      get => deniedIncompleteCommon ??= new();
      set => deniedIncompleteCommon = value;
    }

    /// <summary>
    /// A value of TypePrompt.
    /// </summary>
    [JsonPropertyName("typePrompt")]
    public Standard TypePrompt
    {
      get => typePrompt ??= new();
      set => typePrompt = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Standard StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

    /// <summary>
    /// A value of HiddenInformationRequest.
    /// </summary>
    [JsonPropertyName("hiddenInformationRequest")]
    public InformationRequest HiddenInformationRequest
    {
      get => hiddenInformationRequest ??= new();
      set => hiddenInformationRequest = value;
    }

    /// <summary>
    /// A value of Namelist.
    /// </summary>
    [JsonPropertyName("namelist")]
    public CsePersonsWorkSet Namelist
    {
      get => namelist ??= new();
      set => namelist = value;
    }

    /// <summary>
    /// A value of WithoutMedicalInd.
    /// </summary>
    [JsonPropertyName("withoutMedicalInd")]
    public Common WithoutMedicalInd
    {
      get => withoutMedicalInd ??= new();
      set => withoutMedicalInd = value;
    }

    /// <summary>
    /// A value of WithMedicalInd.
    /// </summary>
    [JsonPropertyName("withMedicalInd")]
    public Common WithMedicalInd
    {
      get => withMedicalInd ??= new();
      set => withMedicalInd = value;
    }

    /// <summary>
    /// A value of LocateInd.
    /// </summary>
    [JsonPropertyName("locateInd")]
    public Common LocateInd
    {
      get => locateInd ??= new();
      set => locateInd = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of WorkerId.
    /// </summary>
    [JsonPropertyName("workerId")]
    public TextWorkArea WorkerId
    {
      get => workerId ??= new();
      set => workerId = value;
    }

    /// <summary>
    /// A value of LimitedInd.
    /// </summary>
    [JsonPropertyName("limitedInd")]
    public Common LimitedInd
    {
      get => limitedInd ??= new();
      set => limitedInd = value;
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

    private CsePerson csePerson;
    private CsePersonsWorkSet ar;
    private Standard reopenType;
    private WorkArea reopenFromComn;
    private InformationRequest deniedIncompleteInformationRequest;
    private Common deniedIncompleteCommon;
    private Standard typePrompt;
    private CodeValue codeValue;
    private Code code;
    private Standard statePrompt;
    private InformationRequest hiddenInformationRequest;
    private CsePersonsWorkSet namelist;
    private Common withoutMedicalInd;
    private Common withMedicalInd;
    private Common locateInd;
    private InformationRequest informationRequest;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private TextWorkArea workerId;
    private Common limitedInd;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ReopenTypr.
    /// </summary>
    [JsonPropertyName("reopenTypr")]
    public Standard ReopenTypr
    {
      get => reopenTypr ??= new();
      set => reopenTypr = value;
    }

    /// <summary>
    /// A value of ReopenFromComn.
    /// </summary>
    [JsonPropertyName("reopenFromComn")]
    public WorkArea ReopenFromComn
    {
      get => reopenFromComn ??= new();
      set => reopenFromComn = value;
    }

    /// <summary>
    /// A value of FromInrd.
    /// </summary>
    [JsonPropertyName("fromInrd")]
    public Common FromInrd
    {
      get => fromInrd ??= new();
      set => fromInrd = value;
    }

    /// <summary>
    /// A value of DeniedIncompleteInformationRequest.
    /// </summary>
    [JsonPropertyName("deniedIncompleteInformationRequest")]
    public InformationRequest DeniedIncompleteInformationRequest
    {
      get => deniedIncompleteInformationRequest ??= new();
      set => deniedIncompleteInformationRequest = value;
    }

    /// <summary>
    /// A value of DeniedIncompleteCommon.
    /// </summary>
    [JsonPropertyName("deniedIncompleteCommon")]
    public Common DeniedIncompleteCommon
    {
      get => deniedIncompleteCommon ??= new();
      set => deniedIncompleteCommon = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
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
    /// A value of TypePrompt.
    /// </summary>
    [JsonPropertyName("typePrompt")]
    public Standard TypePrompt
    {
      get => typePrompt ??= new();
      set => typePrompt = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of Menu.
    /// </summary>
    [JsonPropertyName("menu")]
    public InformationRequest Menu
    {
      get => menu ??= new();
      set => menu = value;
    }

    /// <summary>
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Standard StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

    /// <summary>
    /// A value of HiddenInformationRequest.
    /// </summary>
    [JsonPropertyName("hiddenInformationRequest")]
    public InformationRequest HiddenInformationRequest
    {
      get => hiddenInformationRequest ??= new();
      set => hiddenInformationRequest = value;
    }

    /// <summary>
    /// A value of Namelist.
    /// </summary>
    [JsonPropertyName("namelist")]
    public CsePersonsWorkSet Namelist
    {
      get => namelist ??= new();
      set => namelist = value;
    }

    /// <summary>
    /// A value of WithoutMedicalInd.
    /// </summary>
    [JsonPropertyName("withoutMedicalInd")]
    public Common WithoutMedicalInd
    {
      get => withoutMedicalInd ??= new();
      set => withoutMedicalInd = value;
    }

    /// <summary>
    /// A value of WithMedicalInd.
    /// </summary>
    [JsonPropertyName("withMedicalInd")]
    public Common WithMedicalInd
    {
      get => withMedicalInd ??= new();
      set => withMedicalInd = value;
    }

    /// <summary>
    /// A value of LocateInd.
    /// </summary>
    [JsonPropertyName("locateInd")]
    public Common LocateInd
    {
      get => locateInd ??= new();
      set => locateInd = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of WorkerId.
    /// </summary>
    [JsonPropertyName("workerId")]
    public TextWorkArea WorkerId
    {
      get => workerId ??= new();
      set => workerId = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public LegalAction Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of LimitedInd.
    /// </summary>
    [JsonPropertyName("limitedInd")]
    public Common LimitedInd
    {
      get => limitedInd ??= new();
      set => limitedInd = value;
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

    private CsePerson csePerson;
    private CsePersonsWorkSet ar;
    private Standard reopenTypr;
    private WorkArea reopenFromComn;
    private Common fromInrd;
    private InformationRequest deniedIncompleteInformationRequest;
    private Common deniedIncompleteCommon;
    private Common phonetic;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard typePrompt;
    private CodeValue codeValue;
    private Code code;
    private InformationRequest menu;
    private Standard statePrompt;
    private InformationRequest hiddenInformationRequest;
    private CsePersonsWorkSet namelist;
    private Common withoutMedicalInd;
    private Common withMedicalInd;
    private Common locateInd;
    private InformationRequest informationRequest;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private TextWorkArea workerId;
    private LegalAction filter;
    private Common limitedInd;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public CsePersonsWorkSet Returned
    {
      get => returned ??= new();
      set => returned = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of AddressFound.
    /// </summary>
    [JsonPropertyName("addressFound")]
    public Common AddressFound
    {
      get => addressFound ??= new();
      set => addressFound = value;
    }

    /// <summary>
    /// A value of AsOfDate.
    /// </summary>
    [JsonPropertyName("asOfDate")]
    public DateWorkArea AsOfDate
    {
      get => asOfDate ??= new();
      set => asOfDate = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public NextTranInfo Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of WithoutMedicalInd.
    /// </summary>
    [JsonPropertyName("withoutMedicalInd")]
    public Common WithoutMedicalInd
    {
      get => withoutMedicalInd ??= new();
      set => withoutMedicalInd = value;
    }

    /// <summary>
    /// A value of WithMedicalInd.
    /// </summary>
    [JsonPropertyName("withMedicalInd")]
    public Common WithMedicalInd
    {
      get => withMedicalInd ??= new();
      set => withMedicalInd = value;
    }

    /// <summary>
    /// A value of LocateInd.
    /// </summary>
    [JsonPropertyName("locateInd")]
    public Common LocateInd
    {
      get => locateInd ??= new();
      set => locateInd = value;
    }

    /// <summary>
    /// A value of DeniedIncompleteInformationRequest.
    /// </summary>
    [JsonPropertyName("deniedIncompleteInformationRequest")]
    public InformationRequest DeniedIncompleteInformationRequest
    {
      get => deniedIncompleteInformationRequest ??= new();
      set => deniedIncompleteInformationRequest = value;
    }

    /// <summary>
    /// A value of DeniedIncompleteCommon.
    /// </summary>
    [JsonPropertyName("deniedIncompleteCommon")]
    public Common DeniedIncompleteCommon
    {
      get => deniedIncompleteCommon ??= new();
      set => deniedIncompleteCommon = value;
    }

    /// <summary>
    /// A value of NumericZip.
    /// </summary>
    [JsonPropertyName("numericZip")]
    public Common NumericZip
    {
      get => numericZip ??= new();
      set => numericZip = value;
    }

    /// <summary>
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public CsePersonAddress CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SelectCnt.
    /// </summary>
    [JsonPropertyName("selectCnt")]
    public Common SelectCnt
    {
      get => selectCnt ??= new();
      set => selectCnt = value;
    }

    /// <summary>
    /// A value of RtnCode.
    /// </summary>
    [JsonPropertyName("rtnCode")]
    public Common RtnCode
    {
      get => rtnCode ??= new();
      set => rtnCode = value;
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
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public InformationRequest Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    private CsePersonsWorkSet returned;
    private CsePerson ap;
    private CsePerson ar;
    private CsePerson csePerson;
    private Common addressFound;
    private DateWorkArea asOfDate;
    private CsePersonAddress csePersonAddress;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Case1 case1;
    private NextTranInfo null1;
    private SpDocLiteral spDocLiteral;
    private BatchConvertNumToText batchConvertNumToText;
    private Common position;
    private WorkArea workArea;
    private Common withoutMedicalInd;
    private Common withMedicalInd;
    private Common locateInd;
    private InformationRequest deniedIncompleteInformationRequest;
    private Common deniedIncompleteCommon;
    private Common numericZip;
    private CsePersonAddress checkZip;
    private Document document;
    private Common selectCnt;
    private Common rtnCode;
    private Code code;
    private CodeValue codeValue;
    private ControlTable controlTable;
    private InformationRequest blank;
    private Common validCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
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

    private CaseRole caseRole;
    private CsePerson csePerson;
    private InformationRequest informationRequest;
    private Case1 case1;
  }
#endregion
}
