// Program: SP_SPAD_SRVC_PRVDR_ALERT_DETAILS, ID: 371748083, model: 746.
// Short name: SWESPADP
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
/// A program: SP_SPAD_SRVC_PRVDR_ALERT_DETAILS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpSpadSrvcPrvdrAlertDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_SPAD_SRVC_PRVDR_ALERT_DETAILS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpSpadSrvcPrvdrAlertDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpSpadSrvcPrvdrAlertDetails.
  /// </summary>
  public SpSpadSrvcPrvdrAlertDetails(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 10/24/96 Regan Welborn               Initial Development
    // 04/15/97    Siraj Konkader           Allow delete if logged
    // on user owns alert or is at a supervisory level.
    // 11/13/00   Anita Massey    WR237 enhancements list:  default send to
    //                            to user signed on.   Allow entry of
    //                            data in the case number, person number
    //                            and court order number fields on an add,
    //                            At least one of the three is mandatory.
    // 9/25/02	SWSRPRM
    // Completed WR 237 work; fixed returning information on
    // prompt for receiving SVPO; allowed user id information to
    // remain after a clear; added xref checks for legal action
    // based upon data inputs
    // 11/12/02
    // PR # 163229 - FIPS info not displaying on ALRT flows.
    // 
    // 4/17/03 - PR# 170817
    // Read of legal action court case # added to ensure validity before 
    // subsequent
    // Read Eaches.  JeH
    // 02/28/2019  R.Mathews  CQ65237
    // On return from 'Add', clear case/person fields and leave them unprotected
    // to
    // allow additional alerts to be added with the same message text.
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      export.Sender.UserId = global.UserId;
      export.ServiceProvider.UserId = global.UserId;

      if (import.Office.SystemGeneratedId == 0 || IsEmpty
        (import.OfficeServiceProvider.RoleCode))
      {
        global.Command = "XXNEXTXX";
      }
      else
      {
        export.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
        export.OfficeServiceProvider.RoleCode =
          import.OfficeServiceProvider.RoleCode;
        ExitState = "ACO_NI0000_FIELDS_CLEARED";

        return;
      }
    }
    else
    {
      export.OfficeServiceProviderAlert.
        Assign(import.OfficeServiceProviderAlert);
      export.Svpo.PromptField = import.Svpo.PromptField;
      MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
      export.Infrastructure.Assign(import.Infrastructure);
      MoveLegalAction(import.LegalAction, export.LegalAction);
      export.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
      MoveOfficeServiceProvider(import.OfficeServiceProvider,
        export.OfficeServiceProvider);
      MoveServiceProvider(import.ServiceProvider, export.ServiceProvider);
      export.Standard.NextTransaction = import.Standard.NextTransaction;
      export.WorkArea.Text80 = import.WorkArea.Text80;
      export.Sender.UserId = import.Sender.UserId;
      MoveFips(import.Fips, export.Fips);
      export.FipsTribAddress.Country = import.FipsTribAddress.Country;
      local.CurrentDate.Date = Now().Date;
    }

    switch(TrimEnd(global.Command))
    {
      case "XXFMMENU":
        // -----------------
        // no logic required
        // -----------------
        break;
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          local.NextTranInfo.Assign(import.Hidden);

          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          // ---------------------------------------------------------------
          // Set up local next_tran_info for saving the current values for
          // the next screen
          // ---------------------------------------------------------------
          UseScCabNextTranPut();

          return;
        }

        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      case "XXNEXTXX":
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();

        // ----------------------------------------------------------
        // Populate export views from local next_tran_info view read
        // from the data base
        // Set command to initial command required or ESCAPE
        // ----------------------------------------------------------
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        if (AsChar(export.Svpo.PromptField) == 'S')
        {
          export.Svpo.PromptField = "";
          ExitState = "ECO_LNK_TO_SVPO";

          return;
        }
        else
        {
          var field1 = GetField(export.Svpo, "promptField");

          field1.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          return;
        }

        break;
      case "ADD":
        // -----------------
        // no logic required
        // -----------------
        break;
      case "DELETE":
        // -----------------
        // no logic required
        // -----------------
        break;
      case "DISPLAY":
        if (export.OfficeServiceProviderAlert.SystemGeneratedIdentifier > 0)
        {
          if (ReadInfrastructure())
          {
            MoveInfrastructure2(entities.Infrastructure, export.Infrastructure);
          }
          else
          {
            // --------------------------------------------------------
            // Not an error - many MANual alerts have no infrastructure
            // --------------------------------------------------------
          }
        }
        else
        {
          // ---------------------------------------------------
          // USER flowed from ALRT w/out selecting a record
          // ---------------------------------------------------
          var field1 = GetField(export.OfficeServiceProvider, "roleCode");

          field1.Protected = false;

          var field2 = GetField(export.Office, "systemGeneratedId");

          field2.Protected = false;
        }

        break;
      case "RETSVPO":
        if (import.SelectedOffice.SystemGeneratedId != 0)
        {
          export.Office.SystemGeneratedId =
            import.SelectedOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(import.SelectedOfficeServiceProvider,
            export.OfficeServiceProvider);
          MoveServiceProvider(import.SelectedServiceProvider,
            export.ServiceProvider);
          export.OfficeServiceProviderAlert.Message = "";
          export.OfficeServiceProviderAlert.Description = "";
          export.OfficeServiceProviderAlert.TypeCode = "";
          export.OfficeServiceProviderAlert.DistributionDate = null;
          export.Infrastructure.CaseNumber = "";
          export.Infrastructure.CsePersonNumber = "";
          export.Infrastructure.CaseUnitNumber = 0;
          export.LegalAction.CourtCaseNumber = "";
          export.WorkArea.Text80 = "";
        }

        var field = GetField(export.Svpo, "promptField");

        field.Protected = false;
        field.Focused = true;

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
    }

    // *** WR 237 added read below to find the first occurence of the signed on 
    // service provider and fill the receiver in as that person when you next
    // tran in here and there is no osp coming into the screen like there is on
    // a flow from alrt.      Anita
    if (IsEmpty(import.OfficeServiceProviderAlert.CreatedBy) || Equal
      (global.Command, "DISPLAY"))
    {
      local.Max.Date = new DateTime(2099, 12, 31);

      if (Equal(global.Command, "RETSVPO"))
      {
        local.Work.UserId = import.SelectedServiceProvider.UserId;
        export.ServiceProvider.UserId = local.Work.UserId;
      }
      else if (Equal(global.Command, "XXNEXTXX"))
      {
        export.Sender.UserId = global.UserId;
        local.Work.UserId = global.UserId;
      }
      else if (Equal(global.Command, "ADD"))
      {
        local.Work.UserId = import.ServiceProvider.UserId;
      }
      else if (Equal(global.Command, "DISPLAY"))
      {
        if (export.Infrastructure.SystemGeneratedIdentifier == 0 && export
          .OfficeServiceProviderAlert.SystemGeneratedIdentifier == 0)
        {
          // ---------------------------------------------------
          // USER flowed from ALRT w/out selecting a record - no
          // further processing necessary
          // ---------------------------------------------------
          export.Sender.UserId = global.UserId;
          local.Work.UserId = global.UserId;
          global.Command = "";
        }
        else
        {
          goto Test1;
        }
      }
      else
      {
        local.Work.UserId = global.UserId;
        export.Sender.UserId = local.Work.UserId;
      }

      if (ReadServiceProvider())
      {
        export.ServiceProvider.UserId = entities.ServiceProvider.UserId;

        if (Equal(global.Command, "ADD"))
        {
          if (ReadOfficeOfficeServiceProvider1())
          {
            export.OfficeServiceProvider.EffectiveDate =
              entities.OfficeServiceProvider.EffectiveDate;
            export.ServiceProvider.SystemGeneratedId =
              entities.ServiceProvider.SystemGeneratedId;
            export.OfficeServiceProvider.RoleCode =
              entities.OfficeServiceProvider.RoleCode;
            export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
          }
        }
        else if (ReadOfficeOfficeServiceProvider2())
        {
          export.OfficeServiceProvider.EffectiveDate =
            entities.OfficeServiceProvider.EffectiveDate;
          export.ServiceProvider.SystemGeneratedId =
            entities.ServiceProvider.SystemGeneratedId;
          export.OfficeServiceProvider.RoleCode =
            entities.OfficeServiceProvider.RoleCode;
          export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
        }
      }
      else
      {
        var field = GetField(export.Sender, "userId");

        field.Color = "cyan";
        field.Protected = false;
        field.Focused = true;

        ExitState = "SERVICE_PROVIDER_NF";

        return;
      }
    }
    else if (Equal(global.Command, "DELETE"))
    {
      // -----------------
      // no logic required
      // -----------------
    }
    else
    {
      export.Sender.UserId = import.Sender.UserId;

      if (Equal(global.Command, "RETSVPO"))
      {
        // -----------------
        // no logic required
        // -----------------
      }
      else
      {
        var field1 = GetField(export.Infrastructure, "caseNumber");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Infrastructure, "csePersonNumber");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.LegalAction, "courtCaseNumber");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.Fips, "countyAbbreviation");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Fips, "stateAbbreviation");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.FipsTribAddress, "country");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Infrastructure, "caseUnitNumber");

        field7.Color = "cyan";
        field7.Highlighting = Highlighting.Normal;
        field7.Protected = true;

        // **** 11/18/98 no validation on the sender field *****
      }
    }

Test1:

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (AsChar(export.Svpo.PromptField) == 'S')
    {
      var field = GetField(export.Svpo, "promptField");

      field.Error = true;

      if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE"))
      {
        ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";

        return;
      }
      else if (Equal(global.Command, "DISPLAY") || Equal
        (global.Command, "EXIT") || !Equal(global.Command, "HELP"))
      {
        ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "ADD":
        local.Error.Count = 0;

        if (import.Infrastructure.SystemGeneratedIdentifier != 0)
        {
          ExitState = "SP0000_CLEAR_BEFORE_ADD";
        }

        if (IsEmpty(export.ServiceProvider.UserId))
        {
          var field8 = GetField(export.ServiceProvider, "userId");

          field8.Error = true;

          var field9 = GetField(export.Svpo, "promptField");

          field9.Protected = false;
          field9.Focused = true;

          ++local.Error.Count;
          ExitState = "SP0000_SRVC_PRVDR";
        }

        if (IsEmpty(import.OfficeServiceProviderAlert.Message))
        {
          var field = GetField(export.OfficeServiceProviderAlert, "message");

          field.Error = true;

          ++local.Error.Count;
          ExitState = "SP0000_OSP_ALERT_MESSAGE_REQD";
        }

        if (Equal(export.OfficeServiceProviderAlert.DistributionDate, null))
        {
          export.OfficeServiceProviderAlert.DistributionDate =
            local.CurrentDate.Date;
        }
        else if (Lt(export.OfficeServiceProviderAlert.DistributionDate,
          local.CurrentDate.Date))
        {
          var field =
            GetField(export.OfficeServiceProviderAlert, "distributionDate");

          field.Error = true;

          ++local.Error.Count;
          ExitState = "SP0000_DATE_LESS_THAN_CURRENT";
        }

        // ***  WR237     Anita Massey   Added the following four statements to 
        // make sure the worker enters at least one of the fields at the bottom
        if (IsEmpty(export.Infrastructure.CaseNumber) && IsEmpty
          (export.Infrastructure.CsePersonNumber) && IsEmpty
          (export.LegalAction.CourtCaseNumber))
        {
          var field8 = GetField(export.Infrastructure, "caseNumber");

          field8.Error = true;

          var field9 = GetField(export.Infrastructure, "csePersonNumber");

          field9.Error = true;

          var field10 = GetField(export.LegalAction, "courtCaseNumber");

          field10.Error = true;

          var field11 = GetField(export.Fips, "countyAbbreviation");

          field11.Error = true;

          var field12 = GetField(export.Fips, "stateAbbreviation");

          field12.Error = true;

          var field13 = GetField(export.FipsTribAddress, "country");

          field13.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        // ----------------------
        // Case # validity checks
        // ----------------------
        if (!IsEmpty(export.Infrastructure.CaseNumber))
        {
          local.TextWorkArea.Text10 = export.Infrastructure.CaseNumber ?? Spaces
            (10);
          UseEabPadLeftWithZeros();
          export.Infrastructure.CaseNumber = local.TextWorkArea.Text10;

          if (ReadCase())
          {
            local.Infrastructure.CaseNumber = entities.Case1.Number;

            if (export.Infrastructure.CaseUnitNumber.GetValueOrDefault() > 0)
            {
              if (ReadCaseUnit())
              {
                local.Infrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
              }
              else
              {
                var field = GetField(export.Infrastructure, "caseUnitNumber");

                field.Error = true;

                ExitState = "CASE_UNIT_NF";

                return;
              }
            }
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

            var field = GetField(export.Infrastructure, "caseNumber");

            field.Error = true;

            return;
          }
        }

        // ----------------------------
        // CSE Person # validity checks
        // ----------------------------
        if (!IsEmpty(import.Infrastructure.CsePersonNumber))
        {
          local.TextWorkArea.Text10 = import.Infrastructure.CsePersonNumber ?? Spaces
            (10);
          UseEabPadLeftWithZeros();
          export.Infrastructure.CsePersonNumber = local.TextWorkArea.Text10;

          if (ReadCsePerson())
          {
            local.Infrastructure.CsePersonNumber =
              export.Infrastructure.CsePersonNumber ?? "";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

            var field = GetField(export.Infrastructure, "csePersonNumber");

            field.Error = true;

            return;
          }
        }

        // --------------------
        // CC # validity checks
        // --------------------
        // ----------------------------------------------------------
        // USER is allowed to put in only CC # - SME did not want any
        // restriction on CC unless USER also puts in case and/or
        // person information
        // ----------------------------------------------------------
        if (IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          if (!IsEmpty(export.Fips.CountyAbbreviation) || !
            IsEmpty(export.Fips.StateAbbreviation) || !
            IsEmpty(export.FipsTribAddress.Country))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "OE0014_MANDATORY_FIELD_MISSING";

            return;
          }
        }
        else
        {
          // --------------------
          // CC # validity check:  the following Read was added 
          // to ensure an existing CC#.
          // 
          // PR# 170817  JeH
          // --------------------
          if (!ReadLegalAction1())
          {
            ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            return;
          }

          if (IsEmpty(export.Fips.CountyAbbreviation) || IsEmpty
            (export.Fips.StateAbbreviation))
          {
            if (IsEmpty(export.Fips.CountyAbbreviation) && !
              IsEmpty(export.Fips.StateAbbreviation))
            {
              if (!IsEmpty(export.FipsTribAddress.Country))
              {
                var field8 = GetField(export.FipsTribAddress, "country");

                field8.Error = true;

                var field9 = GetField(export.Fips, "stateAbbreviation");

                field9.Error = true;

                ExitState = "ACO_NE0000_INVALID_SELECTION";

                return;
              }
              else
              {
                var field = GetField(export.Fips, "countyAbbreviation");

                field.Error = true;

                ExitState = "OE0014_MANDATORY_FIELD_MISSING";

                return;
              }
            }

            if (IsEmpty(export.Fips.StateAbbreviation) && !
              IsEmpty(export.Fips.CountyAbbreviation))
            {
              if (!IsEmpty(export.FipsTribAddress.Country))
              {
                var field8 = GetField(export.FipsTribAddress, "country");

                field8.Error = true;

                var field9 = GetField(export.Fips, "countyAbbreviation");

                field9.Error = true;

                ExitState = "ACO_NE0000_INVALID_SELECTION";

                return;
              }
              else
              {
                var field = GetField(export.Fips, "stateAbbreviation");

                field.Error = true;

                ExitState = "OE0014_MANDATORY_FIELD_MISSING";

                return;
              }
            }

            if (IsEmpty(export.FipsTribAddress.Country))
            {
              var field8 = GetField(export.FipsTribAddress, "country");

              field8.Error = true;

              var field9 = GetField(export.Fips, "stateAbbreviation");

              field9.Error = true;

              var field10 = GetField(export.Fips, "countyAbbreviation");

              field10.Error = true;

              ExitState = "OE0014_MANDATORY_FIELD_MISSING";

              return;
            }
          }
          else if (!IsEmpty(export.FipsTribAddress.Country))
          {
            var field8 = GetField(export.FipsTribAddress, "country");

            field8.Error = true;

            var field9 = GetField(export.Fips, "stateAbbreviation");

            field9.Error = true;

            var field10 = GetField(export.Fips, "countyAbbreviation");

            field10.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECTION";

            return;
          }
        }

        // ----------
        // All fields
        // ----------
        if (!IsEmpty(export.Infrastructure.CaseNumber) && !
          IsEmpty(export.Infrastructure.CsePersonNumber) && !
          IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          foreach(var item in ReadLegalActionLegalActionCaseRole1())
          {
            if (IsEmpty(export.FipsTribAddress.Country) && !
              IsEmpty(export.Fips.CountyAbbreviation))
            {
              if (ReadFips2())
              {
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.CourtCaseNumber;

                goto Test2;
              }
              else
              {
                continue;
              }
            }
            else if (ReadFipsTribAddress1())
            {
              local.Infrastructure.DenormText12 =
                entities.LegalAction.CourtCaseNumber;

              goto Test2;
            }
            else
            {
              continue;
            }
          }

          if (IsEmpty(entities.Fips.CountyAbbreviation) && !
            IsEmpty(export.Fips.CountyAbbreviation))
          {
            var field8 = GetField(export.LegalAction, "courtCaseNumber");

            field8.Error = true;

            var field9 = GetField(export.Fips, "countyAbbreviation");

            field9.Error = true;

            var field10 = GetField(export.Fips, "stateAbbreviation");

            field10.Error = true;

            ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

            return;
          }

          if (!IsEmpty(export.FipsTribAddress.Country) && IsEmpty
            (entities.FipsTribAddress.Country))
          {
            var field8 = GetField(export.LegalAction, "courtCaseNumber");

            field8.Error = true;

            var field9 = GetField(export.FipsTribAddress, "country");

            field9.Error = true;

            ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

            return;
          }
        }
        else
        {
          // ---------------
          // Case # and CC #
          // ---------------
          if (!IsEmpty(export.Infrastructure.CaseNumber) && IsEmpty
            (export.Infrastructure.CsePersonNumber) && !
            IsEmpty(export.LegalAction.CourtCaseNumber))
          {
            foreach(var item in ReadLegalActionLegalActionCaseRole3())
            {
              if (IsEmpty(export.FipsTribAddress.Country) && !
                IsEmpty(export.Fips.CountyAbbreviation))
              {
                if (ReadFips1())
                {
                  local.Infrastructure.DenormText12 =
                    entities.LegalAction.CourtCaseNumber;

                  goto Test2;
                }
                else
                {
                  continue;
                }
              }
              else if (ReadFipsTribAddress1())
              {
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.CourtCaseNumber;

                goto Test2;
              }
              else
              {
                continue;
              }
            }

            if (IsEmpty(entities.Fips.CountyAbbreviation) && !
              IsEmpty(export.Fips.CountyAbbreviation))
            {
              var field8 = GetField(export.LegalAction, "courtCaseNumber");

              field8.Error = true;

              var field9 = GetField(export.Fips, "countyAbbreviation");

              field9.Error = true;

              var field10 = GetField(export.Fips, "stateAbbreviation");

              field10.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              return;
            }

            if (!IsEmpty(export.FipsTribAddress.Country) && IsEmpty
              (entities.FipsTribAddress.Country))
            {
              var field8 = GetField(export.LegalAction, "courtCaseNumber");

              field8.Error = true;

              var field9 = GetField(export.FipsTribAddress, "country");

              field9.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              return;
            }
          }
          else if (IsEmpty(export.Infrastructure.CaseNumber) && !
            IsEmpty(export.Infrastructure.CsePersonNumber) && !
            IsEmpty(export.LegalAction.CourtCaseNumber))
          {
            // ------------------
            // CSE Person #, CC #
            // ------------------
            foreach(var item in ReadLegalActionLegalActionCaseRole2())
            {
              if (IsEmpty(export.FipsTribAddress.Country) && !
                IsEmpty(export.Fips.CountyAbbreviation))
              {
                if (ReadFips1())
                {
                  local.Infrastructure.DenormText12 =
                    entities.LegalAction.CourtCaseNumber;

                  goto Test2;
                }
                else
                {
                  continue;
                }
              }
              else if (ReadFipsTribAddress1())
              {
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.CourtCaseNumber;

                goto Test2;
              }
              else
              {
                continue;
              }
            }

            if (IsEmpty(entities.Fips.CountyAbbreviation) && !
              IsEmpty(export.Fips.CountyAbbreviation))
            {
              var field8 = GetField(export.LegalAction, "courtCaseNumber");

              field8.Error = true;

              var field9 = GetField(export.Fips, "countyAbbreviation");

              field9.Error = true;

              var field10 = GetField(export.Fips, "stateAbbreviation");

              field10.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              return;
            }

            if (!IsEmpty(export.FipsTribAddress.Country) && IsEmpty
              (entities.FipsTribAddress.Country))
            {
              var field8 = GetField(export.LegalAction, "courtCaseNumber");

              field8.Error = true;

              var field9 = GetField(export.FipsTribAddress, "country");

              field9.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              return;
            }
          }
          else if (IsEmpty(export.Infrastructure.CaseNumber) && IsEmpty
            (export.Infrastructure.CsePersonNumber) && !
            IsEmpty(export.LegalAction.CourtCaseNumber))
          {
            // ---------
            // CC # only
            // ---------
            foreach(var item in ReadLegalAction3())
            {
              if (IsEmpty(export.FipsTribAddress.Country) && !
                IsEmpty(export.Fips.CountyAbbreviation))
              {
                if (ReadFips1())
                {
                  local.Infrastructure.DenormText12 =
                    entities.LegalAction.CourtCaseNumber;

                  goto Test2;
                }
                else
                {
                  continue;
                }
              }
              else if (ReadFipsTribAddress1())
              {
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.CourtCaseNumber;

                goto Test2;
              }
              else
              {
                continue;
              }
            }

            if (IsEmpty(entities.Fips.CountyAbbreviation) && !
              IsEmpty(export.Fips.CountyAbbreviation))
            {
              var field8 = GetField(export.LegalAction, "courtCaseNumber");

              field8.Error = true;

              var field9 = GetField(export.Fips, "countyAbbreviation");

              field9.Error = true;

              var field10 = GetField(export.Fips, "stateAbbreviation");

              field10.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              return;
            }

            if (!IsEmpty(export.FipsTribAddress.Country) && IsEmpty
              (entities.FipsTribAddress.Country))
            {
              var field8 = GetField(export.LegalAction, "courtCaseNumber");

              field8.Error = true;

              var field9 = GetField(export.FipsTribAddress, "country");

              field9.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              return;
            }
          }
        }

Test2:

        switch(local.Error.Count)
        {
          case 0:
            break;
          case 1:
            return;
          default:
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            return;
        }

        // ***  WR237     Anita Massey    Added this create infrastructure
        local.Infrastructure.ProcessStatus = "P";
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.UserId = global.UserId;
        local.Infrastructure.ReferenceDate = local.CurrentDate.Date;
        local.Infrastructure.EventId = 500;
        local.Infrastructure.ReasonCode = "MANUALALERT";
        local.Infrastructure.Detail = "MANUAL ALERT CREATED ON SPAD SCREEN";

        // -----------------------------------------------------------
        // Without knowing the LA identifier in the process, when
        // flowing from ALRT there is no way to determine the correct
        // FIPS information unless that information is appended here
        // -----------------------------------------------------------
        if (!IsEmpty(entities.Fips.CountyAbbreviation))
        {
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + ";"
            + export.Fips.StateAbbreviation + (
              export.Fips.CountyAbbreviation ?? "");
        }
        else if (!IsEmpty(export.FipsTribAddress.Country))
        {
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + ";"
            + (export.FipsTribAddress.Country ?? "");
        }

        UseSpCabCreateInfrastructure();
        MoveOfficeServiceProviderAlert1(import.OfficeServiceProviderAlert,
          local.Pass);

        if (Equal(import.OfficeServiceProviderAlert.DistributionDate,
          local.Initialized.Date))
        {
          export.OfficeServiceProviderAlert.DistributionDate =
            local.CurrentDate.Date;
          local.Pass.DistributionDate = local.CurrentDate.Date;
        }

        local.Pass.OptimizationInd = "N";

        // *****************************************************************
        //   It is now February 25, 1997.  There is a beastly entity called
        //   The Event Processor that does "things" with OSP Alerts, including
        //   optimization.  The values it sets are 0 thru 3 for automatic
        //   alerts.  Therefore, for Manual alerts, the value will be set to
        //   4.  This will distinguish them from automatic alerts.
        //   This decision was Gus Voegli's and his alone.  RVW 2/25/97
        // ****************************************************************
        local.Pass.OptimizedFlag = "9";
        local.Pass.PrioritizationCode = 1;
        local.Pass.TypeCode = "MAN";
        local.Pass.RecipientUserId = export.ServiceProvider.UserId;
        UseSpCabCreateOfcSrvPrvdAlert();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Infrastructure.CaseNumber = "";
          export.Infrastructure.CsePersonNumber = "";
          export.Infrastructure.CaseUnitNumber = 0;
          export.LegalAction.CourtCaseNumber = "";
          export.Fips.CountyAbbreviation = "";
          export.Fips.StateAbbreviation = "";
          export.FipsTribAddress.Country = "";

          var field8 = GetField(export.Infrastructure, "caseNumber");

          field8.Protected = false;
          field8.Focused = true;

          var field9 = GetField(export.Infrastructure, "csePersonNumber");

          field9.Protected = false;

          var field10 = GetField(export.Infrastructure, "caseUnitNumber");

          field10.Protected = false;

          var field11 = GetField(export.LegalAction, "courtCaseNumber");

          field11.Protected = false;

          var field12 = GetField(export.Fips, "countyAbbreviation");

          field12.Protected = false;

          var field13 = GetField(export.Fips, "stateAbbreviation");

          field13.Protected = false;

          var field14 = GetField(export.FipsTribAddress, "country");

          field14.Protected = false;

          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "DELETE":
        if (import.Infrastructure.SystemGeneratedIdentifier <= 0 && !
          Equal(export.OfficeServiceProviderAlert.TypeCode, "MAN"))
        {
          ExitState = "SP0000_FLOW_FROM_ALRT_TO_DELETE";

          return;
        }
        else if (Equal(export.OfficeServiceProviderAlert.TypeCode, "MAN") && export
          .OfficeServiceProviderAlert.SystemGeneratedIdentifier <= 0)
        {
          ExitState = "SP0000_MAN_OSP_DELETE_RULES";

          return;
        }

        // ------------------------------------------------------------
        // If the Alert does not belong to the current user, check if
        // USER has authority to delete.
        // ------------------------------------------------------------
        if (!Equal(global.UserId, export.ServiceProvider.UserId) && !
          Equal(global.UserId, export.OfficeServiceProviderAlert.CreatedBy))
        {
          local.LoggedOnUser.UserId = global.UserId;
          UseCoCabIsPersonSupervisor();

          if (AsChar(local.IsSupervisor.Flag) == 'N')
          {
            ExitState = "CO0000_MUST_HAVE_SUPERVSRY_ROLE";

            return;
          }
        }

        UseSpDeleteOspAlerts();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.OfficeServiceProviderAlert.Message = "";
          export.OfficeServiceProviderAlert.Description = "";
          export.OfficeServiceProviderAlert.TypeCode = "";
          export.OfficeServiceProviderAlert.DistributionDate = null;
          export.Infrastructure.CaseNumber = "";
          export.Infrastructure.CsePersonNumber = "";
          export.Infrastructure.CaseUnitNumber = 0;
          export.LegalAction.CourtCaseNumber = "";
          export.Fips.CountyAbbreviation = "";
          export.Fips.StateAbbreviation = "";
          export.FipsTribAddress.Country = "";
          export.WorkArea.Text80 = "";
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      case "DISPLAY":
        // ************************************************************
        // This will only happen on a flow in from ALRT where a record
        // was selected.
        // ************************************************************
        if (export.Infrastructure.DenormNumeric12.GetValueOrDefault() > 0)
        {
          if (ReadLegalAction2())
          {
            if (ReadTribunalFips())
            {
              MoveFips(entities.Fips, export.Fips);

              if (ReadFipsTribAddress2())
              {
                export.FipsTribAddress.Country =
                  entities.FipsTribAddress.Country;
              }
              else
              {
                ExitState = "CO0000_FIPS_TRIB_ADDR_NF_DB_ERRR";

                return;
              }
            }
            else
            {
              var field8 = GetField(export.Fips, "countyAbbreviation");

              field8.Error = true;

              var field9 = GetField(export.Fips, "stateAbbreviation");

              field9.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";
            }
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";

            return;
          }

          if (IsEmpty(entities.LegalAction.CourtCaseNumber))
          {
            var field8 = GetField(export.LegalAction, "courtCaseNumber");

            field8.Error = true;

            var field9 = GetField(export.Fips, "countyAbbreviation");

            field9.Error = true;

            var field10 = GetField(export.Fips, "stateAbbreviation");

            field10.Error = true;

            ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

            return;
          }
          else
          {
            // -----------------
            // no logic required
            // -----------------
          }
        }

        local.Infrastructure.EventId = export.Infrastructure.EventId;
        UseSpCabReadOffSrvPrvAlert();

        if (Find(entities.Infrastructure.Detail, ";") > 0 && Equal
          (export.OfficeServiceProviderAlert.TypeCode, "MAN") && local
          .Infrastructure.EventId == 500)
        {
          local.TotalDetailLength.Count =
            Length(TrimEnd(entities.Infrastructure.Detail));
          local.FipsInfo.Count =
            Find(TrimEnd(entities.Infrastructure.Detail), ";");
          local.Fips.Text4 =
            Substring(TrimEnd(entities.Infrastructure.Detail),
            local.FipsInfo.Count + 1, 4);

          if (Length(TrimEnd(local.Fips.Text4)) == 2)
          {
            export.FipsTribAddress.Country =
              Substring(entities.Infrastructure.Detail,
              local.TotalDetailLength.Count - 1, 2);
          }
          else if (Length(TrimEnd(local.Fips.Text4)) == 4)
          {
            export.Fips.StateAbbreviation =
              Substring(entities.Infrastructure.Detail,
              local.TotalDetailLength.Count - 3, 2);
            export.Fips.CountyAbbreviation =
              Substring(entities.Infrastructure.Detail,
              local.TotalDetailLength.Count - 1, 2);
          }
          else
          {
            ExitState = "FIPS_NF";

            return;
          }

          export.LegalAction.CourtCaseNumber =
            entities.Infrastructure.DenormText12;
          export.WorkArea.Text80 =
            Substring(export.WorkArea.Text80, 1, local.FipsInfo.Count - 1);
        }

        var field1 = GetField(export.Infrastructure, "caseNumber");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Infrastructure, "csePersonNumber");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Infrastructure, "caseUnitNumber");

        field3.Color = "cyan";
        field3.Highlighting = Highlighting.Normal;
        field3.Protected = true;

        var field4 = GetField(export.LegalAction, "courtCaseNumber");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Fips, "stateAbbreviation");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Fips, "countyAbbreviation");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.FipsTribAddress, "country");

        field7.Color = "cyan";
        field7.Protected = true;

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      default:
        // -----------------
        // no logic required
        // -----------------
        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EventId = source.EventId;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure3(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveOfficeServiceProviderAlert1(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.TypeCode = source.TypeCode;
    target.Message = source.Message;
    target.Description = source.Description;
    target.DistributionDate = source.DistributionDate;
    target.SituationIdentifier = source.SituationIdentifier;
    target.RecipientUserId = source.RecipientUserId;
  }

  private static void MoveOfficeServiceProviderAlert2(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.TypeCode = source.TypeCode;
    target.Message = source.Message;
    target.Description = source.Description;
    target.DistributionDate = source.DistributionDate;
    target.SituationIdentifier = source.SituationIdentifier;
    target.RecipientUserId = source.RecipientUserId;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveOfficeServiceProviderAlert3(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.TypeCode = source.TypeCode;
    target.Message = source.Message;
    target.Description = source.Description;
    target.DistributionDate = source.DistributionDate;
    target.SituationIdentifier = source.SituationIdentifier;
    target.PrioritizationCode = source.PrioritizationCode;
    target.OptimizationInd = source.OptimizationInd;
    target.OptimizedFlag = source.OptimizedFlag;
    target.RecipientUserId = source.RecipientUserId;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private void UseCoCabIsPersonSupervisor()
  {
    var useImport = new CoCabIsPersonSupervisor.Import();
    var useExport = new CoCabIsPersonSupervisor.Export();

    useImport.ServiceProvider.UserId = local.LoggedOnUser.UserId;
    useImport.ProcessDtOrCurrentDt.Date = local.CurrentDate.Date;

    Call(CoCabIsPersonSupervisor.Execute, useImport, useExport);

    local.IsSupervisor.Flag = useExport.IsSupervisor.Flag;
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
    useImport.NextTranInfo.Assign(import.Hidden);

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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabCreateOfcSrvPrvdAlert()
  {
    var useImport = new SpCabCreateOfcSrvPrvdAlert.Import();
    var useExport = new SpCabCreateOfcSrvPrvdAlert.Export();

    MoveOfficeServiceProviderAlert3(local.Pass,
      useImport.OfficeServiceProviderAlert);
    useImport.Alerts.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    MoveOfficeServiceProvider(export.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Office.SystemGeneratedId = export.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;

    Call(SpCabCreateOfcSrvPrvdAlert.Execute, useImport, useExport);

    export.OfficeServiceProviderAlert.Assign(
      useExport.OfficeServiceProviderAlert);
  }

  private void UseSpCabReadOffSrvPrvAlert()
  {
    var useImport = new SpCabReadOffSrvPrvAlert.Import();
    var useExport = new SpCabReadOffSrvPrvAlert.Export();

    useImport.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
      import.OfficeServiceProviderAlert.SystemGeneratedIdentifier;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Infrastructure.SystemGeneratedIdentifier;

    Call(SpCabReadOffSrvPrvAlert.Execute, useImport, useExport);

    export.Sender.UserId = useExport.Sender.UserId;
    MoveOfficeServiceProvider(useExport.OfficeServiceProvider,
      export.OfficeServiceProvider);
    export.WorkArea.Text80 = useExport.WorkArea.Text80;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    MoveLegalAction(useExport.LegalAction, export.LegalAction);
    MoveInfrastructure3(useExport.Infrastructure, export.Infrastructure);
    export.Office.SystemGeneratedId = useExport.Office.SystemGeneratedId;
    MoveServiceProvider(useExport.ServiceProvider, export.ServiceProvider);
    MoveOfficeServiceProviderAlert2(useExport.OfficeServiceProviderAlert,
      export.OfficeServiceProviderAlert);
  }

  private void UseSpDeleteOspAlerts()
  {
    var useImport = new SpDeleteOspAlerts.Import();
    var useExport = new SpDeleteOspAlerts.Export();

    useImport.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
      export.OfficeServiceProviderAlert.SystemGeneratedIdentifier;

    Call(SpDeleteOspAlerts.Execute, useImport, useExport);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetInt32(
          command, "cuNumber",
          export.Infrastructure.CaseUnitNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadFips1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.LegalAction.TrbId.GetValueOrDefault());
        db.SetNullableString(
          command, "country", export.FipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 5);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Fips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Fips.County);
        db.SetNullableInt32(command, "fipState", entities.Fips.State);
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 5);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.OfficeServiceProviderAlert.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.Infrastructure.EventId = db.GetInt32(reader, 2);
        entities.Infrastructure.EventType = db.GetString(reader, 3);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 4);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 5);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 6);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 7);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 8);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 9);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 11);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 12);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 13);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 14);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 15);
        entities.Infrastructure.UserId = db.GetString(reader, 16);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 17);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 18);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 19);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 20);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 21);
        entities.Infrastructure.Function = db.GetNullableString(reader, 22);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 23);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 24);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          export.Infrastructure.DenormNumeric12.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionCaseRole1()
  {
    entities.LegalActionCaseRole.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionCaseRole1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 3);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 4);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 5);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 6);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionCaseRole.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionCaseRole2()
  {
    entities.LegalActionCaseRole.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionCaseRole2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 3);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 4);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 5);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 6);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 7);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 8);
        entities.LegalActionCaseRole.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionCaseRole3()
  {
    entities.LegalActionCaseRole.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionCaseRole3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 3);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 4);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 5);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 6);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionCaseRole.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);

        return true;
      });
  }

  private bool ReadOfficeOfficeServiceProvider1()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "officeId", export.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 3);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProvider2()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 3);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", local.Work.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadTribunalFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadTribunalFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Fips.Location = db.GetInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Fips.County = db.GetInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Fips.State = db.GetInt32(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
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
    /// A value of SelectedOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedOfficeServiceProvider")]
    public OfficeServiceProvider SelectedOfficeServiceProvider
    {
      get => selectedOfficeServiceProvider ??= new();
      set => selectedOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SelectedOffice.
    /// </summary>
    [JsonPropertyName("selectedOffice")]
    public Office SelectedOffice
    {
      get => selectedOffice ??= new();
      set => selectedOffice = value;
    }

    /// <summary>
    /// A value of SelectedServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedServiceProvider")]
    public ServiceProvider SelectedServiceProvider
    {
      get => selectedServiceProvider ??= new();
      set => selectedServiceProvider = value;
    }

    /// <summary>
    /// A value of Sender.
    /// </summary>
    [JsonPropertyName("sender")]
    public ServiceProvider Sender
    {
      get => sender ??= new();
      set => sender = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of Svpo.
    /// </summary>
    [JsonPropertyName("svpo")]
    public Standard Svpo
    {
      get => svpo ??= new();
      set => svpo = value;
    }

    private FipsTribAddress fipsTribAddress;
    private Fips fips;
    private OfficeServiceProvider selectedOfficeServiceProvider;
    private Office selectedOffice;
    private ServiceProvider selectedServiceProvider;
    private ServiceProvider sender;
    private OfficeServiceProvider officeServiceProvider;
    private WorkArea workArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private Infrastructure infrastructure;
    private Office office;
    private ServiceProvider serviceProvider;
    private Standard standard;
    private NextTranInfo hidden;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private Standard svpo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Sender.
    /// </summary>
    [JsonPropertyName("sender")]
    public ServiceProvider Sender
    {
      get => sender ??= new();
      set => sender = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of Svpo.
    /// </summary>
    [JsonPropertyName("svpo")]
    public Standard Svpo
    {
      get => svpo ??= new();
      set => svpo = value;
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

    private ServiceProvider sender;
    private OfficeServiceProvider officeServiceProvider;
    private WorkArea workArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private Infrastructure infrastructure;
    private Office office;
    private ServiceProvider serviceProvider;
    private Standard standard;
    private NextTranInfo hidden;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private Standard svpo;
    private FipsTribAddress fipsTribAddress;
    private Fips fips;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public WorkArea Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of FipsInfo.
    /// </summary>
    [JsonPropertyName("fipsInfo")]
    public Common FipsInfo
    {
      get => fipsInfo ??= new();
      set => fipsInfo = value;
    }

    /// <summary>
    /// A value of TotalDetailLength.
    /// </summary>
    [JsonPropertyName("totalDetailLength")]
    public Common TotalDetailLength
    {
      get => totalDetailLength ??= new();
      set => totalDetailLength = value;
    }

    /// <summary>
    /// A value of SubstInfoFipsTribAddress.
    /// </summary>
    [JsonPropertyName("substInfoFipsTribAddress")]
    public FipsTribAddress SubstInfoFipsTribAddress
    {
      get => substInfoFipsTribAddress ??= new();
      set => substInfoFipsTribAddress = value;
    }

    /// <summary>
    /// A value of SubstInfoFips.
    /// </summary>
    [JsonPropertyName("substInfoFips")]
    public Fips SubstInfoFips
    {
      get => substInfoFips ??= new();
      set => substInfoFips = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public ServiceProvider Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of LoggedOnUser.
    /// </summary>
    [JsonPropertyName("loggedOnUser")]
    public ServiceProvider LoggedOnUser
    {
      get => loggedOnUser ??= new();
      set => loggedOnUser = value;
    }

    /// <summary>
    /// A value of IsSupervisor.
    /// </summary>
    [JsonPropertyName("isSupervisor")]
    public Common IsSupervisor
    {
      get => isSupervisor ??= new();
      set => isSupervisor = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public OfficeServiceProviderAlert Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private WorkArea fips;
    private Common fipsInfo;
    private Common totalDetailLength;
    private FipsTribAddress substInfoFipsTribAddress;
    private Fips substInfoFips;
    private TextWorkArea textWorkArea;
    private Infrastructure infrastructure;
    private DateWorkArea max;
    private ServiceProvider work;
    private Common error;
    private ServiceProvider loggedOnUser;
    private Common isSupervisor;
    private DateWorkArea currentDate;
    private OfficeServiceProviderAlert pass;
    private NextTranInfo nextTranInfo;
    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private FipsTribAddress fipsTribAddress;
    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private Fips fips;
    private Tribunal tribunal;
    private CaseUnit caseUnit;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private Infrastructure infrastructure;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private Case1 case1;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
