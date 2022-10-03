// Program: SP_SVPO_OFFC_SERV_PROVIDER_MAINT, ID: 371790998, model: 746.
// Short name: SWESVPOP
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
/// A program: SP_SVPO_OFFC_SERV_PROVIDER_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpSvpoOffcServProviderMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_SVPO_OFFC_SERV_PROVIDER_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpSvpoOffcServProviderMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpSvpoOffcServProviderMaint.
  /// </summary>
  public SpSvpoOffcServProviderMaint(IContext context, Import import,
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
    // ***********************************************************************
    // ** DATE      *  DESCRIPTION
    // ** 04/13/95     J. Kemp
    // ** 10/24/95     J. Kemp
    // ** ADD GENETIC TEST ACCOUNT NUMBER PER
    // ** OBLIGATION ESTABLISHMENTS REQUIREMENTS
    // ** 02/06/96      a. hackler    retro fits
    // ** 04/22/96      j. Rookard    final review and debugging prior to 
    // acceptance test.
    // ** 04/30/96	 J. Rookard    add "local contact code for IRS" attribute to 
    // Office Service Provider code and screen, including appropriate validation
    // routines.
    // ** 05/06/96	 J. Rookard	final test and debug.
    // ** 01/08/97      J. Rookard     refit to new Security, begin adding of "
    // assignment" validations needed for delete and discontinue processing.
    // ** 04/29/97      J. Rookard     Current Date retrofit to Local Current 
    // Date Work Area Date.
    // ** 06/16/99   Anita Massey   reviewed read property and set
    //    to select only if appropriate .
    // ** 02/15/01   Madhu Kumar   The certification number on the      screen 
    // will be display only now , and it's going to pull the certification
    // number  from service provider instead of office service provider .
    // ** 1/18/08  G. Pan CQ431  added Invalid command for exit state when
    //                    enter key pressed and there is no next transaction.
    // ***********************************************************************
    // ********************************************************************************************
    // **12/22/15 LSS  CQ50114 Removed the Display Discontinued OSP's prompt 
    // from SVPO screen;
    //   therefore, commented out the code related to the prompt.
    //   (left code in as commented in case business wants to add the prompt 
    // back)
    // ********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();
    }

    export.Office.Assign(import.Office);
    export.Search.LastName = import.Search.LastName;

    if (Equal(global.Command, "RTLIST"))
    {
      export.ListOffice.SelectChar = "";

      var field = GetField(export.ListOffice, "selectChar");

      field.Protected = false;
      field.Focused = true;

      // The only time the command should be changed to Display is when 
      // returning from List Office.  The following if statement enforces that.
      if (import.HiddenSelectionOffice.SystemGeneratedId != 0 && import
        .HiddenSelectionServiceProvider.SystemGeneratedId == 0)
      {
        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (AsChar(import.Import1.Item.Common.SelectChar) == 'S')
          {
            goto Test1;
          }
        }

        export.Office.Assign(import.HiddenSelectionOffice);
        global.Command = "DISPLAY";
      }
    }

Test1:

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        // ** this is where you would set the local next_tran_info attributes to
        // the import view attributes for the data to be passed to the next
        // transaction
        UseScCabNextTranPut();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }

        return;

        // *********************************************
        // 1/18/08  G. Pan CQ431  added ELSE statement
        // *********************************************
      }
      else
      {
        ExitState = "ACO_NE0000_INVALID_COMMAND";
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        // this is where you set your export value to the export hidden next 
        // tran values if the user is coming into this procedure on a next tran
        // action.
        UseScCabNextTranGet();

        return;
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        // You should get this information from the Dialog Flow Diagram.  It is 
        // the SEND CMD on the propertis for a Transfer from one  procedure to
        // another.
        // *** if the dialog flow property was display first, just add an escape
        // completely out of the procedure  ****
        return;
      }
    }

    if (Equal(global.Command, "RTLIST"))
    {
    }
    else
    {
      // to validate action level security
      // *********************************************
      // 1/18/08  G. Pan CQ431  added IF statement
      // *********************************************
      if (Equal(global.Command, "ENTER"))
      {
        goto Test2;
      }

      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

Test2:

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      MoveOfficeAddress(import.OfficeAddress, export.OfficeAddress);
      MoveCommon(import.ListOffice, export.ListOffice);
      export.HiddenOffice.Assign(import.HiddenOffice);
      export.OfficeType.Description = import.OfficeType.Description;

      // CQ50114 12/22/15
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.Common.SelectChar =
          import.Import1.Item.Common.SelectChar;
        export.Export1.Update.ServiceProvider.Assign(
          import.Import1.Item.ServiceProvider);
        MoveCommon(import.Import1.Item.ListServiceProvider,
          export.Export1.Update.ListServiceProvider);
        export.Export1.Update.ListRole.SelectChar =
          import.Import1.Item.ListRole.SelectChar;
        export.Export1.Update.OfficeServiceProvider.Assign(
          import.Import1.Item.OfficeServiceProvider);
        export.Export1.Update.HiddenServiceProvider.Assign(
          import.Import1.Item.HiddenServiceProvider);
        MoveOfficeServiceProvider2(import.Import1.Item.
          HiddenOfficeServiceProvider,
          export.Export1.Update.HiddenOfficeServiceProvider);
        export.Export1.Update.GeneticTestAccount.AccountNumber =
          import.Import1.Item.GeneticTestAccount.AccountNumber;
        export.Export1.Update.HiddenGeneticTestAccount.AccountNumber =
          import.Import1.Item.HiddenGeneticTestAccount.AccountNumber;

        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          ++local.Count.Count;
          export.HiddenSelectionOffice.Assign(import.Office);
          export.HiddenSelectionServiceProvider.Assign(
            export.Export1.Item.ServiceProvider);
          export.HiddenSelectionOfficeServiceProvider.Assign(
            export.Export1.Item.OfficeServiceProvider);
        }

        if (Equal(global.Command, "LIST"))
        {
        }
        else if (Equal(global.Command, "RTLIST") || Equal
          (global.Command, "ADD") || Equal(global.Command, "UPDATE"))
        {
          if (IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            var field1 =
              GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Export1.Item.OfficeServiceProvider,
              "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Export1.Item.ListServiceProvider, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Export1.Item.ListRole, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Export1.Item.OfficeServiceProvider,
              "discontinueDate");

            field6.Color = "cyan";
            field6.Protected = true;

            if (!Lt(local.Current.Date,
              export.Export1.Item.OfficeServiceProvider.DiscontinueDate) && !
              Equal(export.Export1.Item.OfficeServiceProvider.DiscontinueDate,
              local.Null1.Date))
            {
              var field7 =
                GetField(export.Export1.Item.OfficeServiceProvider,
                "discontinueDate");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 =
                GetField(export.Export1.Item.OfficeServiceProvider,
                "workPhoneAreaCode");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 =
                GetField(export.Export1.Item.OfficeServiceProvider,
                "workPhoneNumber");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Export1.Item.OfficeServiceProvider,
                "workPhoneExtension");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 =
                GetField(export.Export1.Item.OfficeServiceProvider,
                "workFaxAreaCode");

              field11.Color = "cyan";
              field11.Protected = true;

              var field12 =
                GetField(export.Export1.Item.OfficeServiceProvider,
                "workFaxNumber");

              field12.Color = "cyan";
              field12.Protected = true;

              var field13 =
                GetField(export.Export1.Item.OfficeServiceProvider,
                "localContactCodeForIrs");

              field13.Color = "cyan";
              field13.Protected = true;

              var field14 =
                GetField(export.Export1.Item.GeneticTestAccount, "accountNumber");
                

              field14.Color = "cyan";
              field14.Protected = true;
            }
          }
        }
        else if (IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          var field1 =
            GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Export1.Item.OfficeServiceProvider, "effectiveDate");
            

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.Common, "selectChar");

          field4.Protected = false;
          field4.Focused = false;

          var field5 =
            GetField(export.Export1.Item.ListServiceProvider, "selectChar");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.ListRole, "selectChar");

          field6.Color = "cyan";
          field6.Protected = true;

          if (!Lt(local.Current.Date,
            export.Export1.Item.OfficeServiceProvider.DiscontinueDate) && !
            Equal(export.Export1.Item.OfficeServiceProvider.DiscontinueDate,
            local.Null1.Date))
          {
            var field7 =
              GetField(export.Export1.Item.OfficeServiceProvider,
              "discontinueDate");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Export1.Item.OfficeServiceProvider,
              "workPhoneAreaCode");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 =
              GetField(export.Export1.Item.OfficeServiceProvider,
              "workPhoneNumber");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.Export1.Item.OfficeServiceProvider,
              "workPhoneExtension");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.Export1.Item.OfficeServiceProvider,
              "workFaxAreaCode");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 =
              GetField(export.Export1.Item.OfficeServiceProvider,
              "workFaxNumber");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 =
              GetField(export.Export1.Item.OfficeServiceProvider,
              "localContactCodeForIrs");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 =
              GetField(export.Export1.Item.GeneticTestAccount, "accountNumber");
              

            field14.Color = "cyan";
            field14.Protected = true;
          }
        }

        if (Equal(global.Command, "RTLIST"))
        {
          if (AsChar(export.ListOffice.SelectChar) == 'S')
          {
            export.ListOffice.SelectChar = "";
          }

          if (AsChar(export.Export1.Item.ListServiceProvider.SelectChar) == 'S')
          {
            if (import.HiddenSelectionServiceProvider.SystemGeneratedId != 0)
            {
              MoveServiceProvider1(import.HiddenSelectionServiceProvider,
                export.Export1.Update.ServiceProvider);

              // **************************************************************
              //    The certification number field is populated after reading
              //  the service provider .
              // **************************************************************
              if (ReadServiceProvider2())
              {
                export.Export1.Update.ServiceProvider.DiscontinueDate =
                  entities.ServiceProvider.DiscontinueDate;
                export.Export1.Update.ServiceProvider.RoleCode =
                  entities.ServiceProvider.RoleCode;
                export.Export1.Update.ServiceProvider.CertificationNumber =
                  entities.ServiceProvider.CertificationNumber;
                export.Export1.Update.OfficeServiceProvider.RoleCode =
                  entities.ServiceProvider.RoleCode ?? Spaces(2);
                export.Export1.Update.OfficeServiceProvider.WorkPhoneAreaCode =
                  entities.ServiceProvider.PhoneAreaCode.GetValueOrDefault();
                export.Export1.Update.OfficeServiceProvider.WorkPhoneNumber =
                  entities.ServiceProvider.PhoneNumber.GetValueOrDefault();

                if (Lt(0, entities.ServiceProvider.PhoneExtension))
                {
                  export.Export1.Update.OfficeServiceProvider.
                    WorkPhoneExtension =
                      NumberToString(entities.ServiceProvider.PhoneExtension.
                      GetValueOrDefault(), 11, 5);
                }
              }
              else
              {
                ExitState = "SERVICE_PROVIDER_NF";
                export.Export1.Next();

                return;
              }
            }

            export.Export1.Update.ListServiceProvider.SelectChar = "";

            var field1 =
              GetField(export.Export1.Item.ListServiceProvider, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            var field2 =
              GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");

            field2.Protected = false;
            field2.Focused = true;
          }

          if (AsChar(export.Export1.Item.ListRole.SelectChar) == 'S')
          {
            if (!IsEmpty(import.HiddenSelectionCodeValue.Cdvalue))
            {
              export.Export1.Update.OfficeServiceProvider.RoleCode =
                import.HiddenSelectionCodeValue.Cdvalue;
            }

            export.Export1.Update.ListRole.SelectChar = "";

            var field = GetField(export.Export1.Item.ListRole, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }
        }

        export.Export1.Next();
      }
    }

    if (Equal(global.Command, "RTLIST"))
    {
      return;
    }

    if (Equal(global.Command, "LIST"))
    {
      switch(AsChar(export.ListOffice.SelectChar))
      {
        case 'S':
          export.HiddenSelectionOffice.Assign(export.Office);
          ExitState = "ECO_LNK_TO_LIST_OFFICE";

          return;
        case ' ':
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          var field = GetField(export.ListOffice, "selectChar");

          field.Error = true;

          return;
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        local.SelectCount.Count = 0;

        if (!IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            ++local.SelectCount.Count;
          }
        }

        if (AsChar(export.Export1.Item.ListRole.SelectChar) == 'S' && AsChar
          (export.Export1.Item.ListServiceProvider.SelectChar) == 'S')
        {
          var field1 = GetField(export.Export1.Item.ListRole, "selectChar");

          field1.Error = true;

          var field2 =
            GetField(export.Export1.Item.ListServiceProvider, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          goto Test3;
        }

        switch(AsChar(export.Export1.Item.ListServiceProvider.SelectChar))
        {
          case 'S':
            export.SortBy.SelectChar = "A";
            export.HiddenSelectionServiceProvider.Assign(
              export.Export1.Item.ServiceProvider);
            ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

            return;
          case ' ':
            break;
          default:
            var field =
              GetField(export.Export1.Item.ListServiceProvider, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            goto Test3;
        }

        switch(AsChar(export.Export1.Item.ListRole.SelectChar))
        {
          case 'S':
            export.HiddenCode.CodeName = "OFFICE SERVICE PROVIDER ROLE";
            export.HiddenCodeValue.Cdvalue =
              export.Export1.Item.OfficeServiceProvider.RoleCode;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          case ' ':
            break;
          default:
            var field =
              GetField(export.Export1.Item.ListServiceProvider, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            goto Test3;
        }
      }

      if (local.SelectCount.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        goto Test3;
      }

      ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
    }

Test3:

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE"))
    {
      if (import.Office.SystemGeneratedId == 0)
      {
        var field = GetField(export.Office, "systemGeneratedId");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        goto Test4;
      }

      if (local.Count.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        goto Test4;
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_DELETE"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            if (export.Export1.Item.ServiceProvider.SystemGeneratedId == 0)
            {
              var field =
                GetField(export.Export1.Item.ServiceProvider,
                "systemGeneratedId");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              goto Test4;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // ********************************************
            // VERIFY THAT THE IDENTIFIERS HAVE NOT CHANGED
            // ********************************************
            if (Equal(global.Command, "UPDATE") || Equal
              (global.Command, "DELETE"))
            {
              if (export.Export1.Item.ServiceProvider.SystemGeneratedId != export
                .Export1.Item.HiddenServiceProvider.SystemGeneratedId)
              {
                ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

                var field =
                  GetField(export.Export1.Item.ServiceProvider,
                  "systemGeneratedId");

                field.Error = true;

                return;
              }

              if (!Equal(export.Export1.Item.OfficeServiceProvider.RoleCode,
                export.Export1.Item.HiddenOfficeServiceProvider.RoleCode))
              {
                ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

                var field =
                  GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");
                  

                field.Error = true;

                return;
              }

              if (!Equal(export.Export1.Item.OfficeServiceProvider.
                EffectiveDate,
                export.Export1.Item.HiddenOfficeServiceProvider.EffectiveDate))
              {
                ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

                var field =
                  GetField(export.Export1.Item.OfficeServiceProvider,
                  "effectiveDate");

                field.Error = true;

                return;
              }

              if (Equal(global.Command, "UPDATE") && !
                Equal(export.Export1.Item.OfficeServiceProvider.DiscontinueDate,
                export.Export1.Item.HiddenOfficeServiceProvider.
                  DiscontinueDate) && Lt
                (export.Export1.Item.OfficeServiceProvider.EffectiveDate,
                export.Export1.Item.OfficeServiceProvider.DiscontinueDate) || Equal
                (global.Command, "DELETE"))
              {
                // User is attempting to discontinue or delete the currently 
                // selected Office Service Provider.  See if any active
                // assignments exist for the selected OSP.
                UseSpCabValidateForOspAssigns();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                }
                else
                {
                  goto Test4;
                }
              }
            }

            if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
            {
              if (IsEmpty(export.Export1.Item.OfficeServiceProvider.RoleCode))
              {
                if (ReadServiceProvider1())
                {
                  export.Export1.Update.ServiceProvider.Assign(
                    entities.ServiceProvider);
                  export.Export1.Update.OfficeServiceProvider.RoleCode =
                    entities.ServiceProvider.RoleCode ?? Spaces(2);
                  export.Export1.Update.OfficeServiceProvider.
                    WorkPhoneAreaCode =
                      entities.ServiceProvider.PhoneAreaCode.
                      GetValueOrDefault();
                  export.Export1.Update.OfficeServiceProvider.WorkPhoneNumber =
                    entities.ServiceProvider.PhoneNumber.GetValueOrDefault();
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.ServiceProvider,
                    "systemGeneratedId");

                  field.Error = true;

                  ExitState = "SERVICE_PROVIDER_NF";

                  return;
                }
              }

              if (IsEmpty(export.Export1.Item.OfficeServiceProvider.RoleCode))
              {
                var field =
                  GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");
                  

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              if (!Equal(export.Export1.Item.OfficeServiceProvider.RoleCode,
                export.Export1.Item.ServiceProvider.RoleCode) && !
                IsEmpty(export.Export1.Item.ServiceProvider.RoleCode))
              {
                var field =
                  GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");
                  

                field.Error = true;

                ExitState = "ROLES_DO_NOT_MATCH";

                return;
              }

              if (Lt(export.Export1.Item.ServiceProvider.DiscontinueDate,
                local.Current.Date))
              {
                ExitState = "SERVICE_PROVIDE_INACTIVE";

                return;
              }

              if (Equal(export.Export1.Item.OfficeServiceProvider.EffectiveDate,
                local.Null1.Date))
              {
                export.Export1.Update.OfficeServiceProvider.EffectiveDate =
                  local.Current.Date;
              }

              // Validate that the IRS code provided in the import view is not 
              // already assigned to another Office Service Provider.
              if (export.Export1.Item.OfficeServiceProvider.
                LocalContactCodeForIrs.GetValueOrDefault() != export
                .Export1.Item.HiddenOfficeServiceProvider.
                  LocalContactCodeForIrs.GetValueOrDefault())
              {
                // The import view of the IRS code has been changed by the user.
                foreach(var item in ReadOfficeServiceProviderOfficeServiceProvider())
                  
                {
                  if (entities.IdServiceProvider.SystemGeneratedId == export
                    .Export1.Item.ServiceProvider.SystemGeneratedId && Equal
                    (entities.OfficeServiceProvider.RoleCode,
                    export.Export1.Item.OfficeServiceProvider.RoleCode) && Equal
                    (entities.OfficeServiceProvider.EffectiveDate,
                    export.Export1.Item.OfficeServiceProvider.EffectiveDate) &&
                    export.Office.SystemGeneratedId == entities
                    .IdOffice.SystemGeneratedId)
                  {
                    continue;
                  }
                  else
                  {
                    if (!Lt(local.Current.Date,
                      entities.OfficeServiceProvider.DiscontinueDate))
                    {
                      continue;
                    }

                    var field =
                      GetField(export.Export1.Item.OfficeServiceProvider,
                      "localContactCodeForIrs");

                    field.Color = "red";
                    field.Intensity = Intensity.High;
                    field.Protected = false;
                    field.Focused = true;

                    ExitState = "SP0000_IRS_CODE_ALREADY_ASGN";

                    return;
                  }
                }
              }

              local.DateWorkArea.Date =
                export.Export1.Item.OfficeServiceProvider.DiscontinueDate;
              export.Export1.Update.OfficeServiceProvider.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate3();

              if (Equal(export.Export1.Item.OfficeServiceProvider.
                DiscontinueDate, local.Null1.Date))
              {
                export.Export1.Update.OfficeServiceProvider.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate2();
              }

              if (Lt(export.Export1.Item.OfficeServiceProvider.DiscontinueDate,
                export.Export1.Item.OfficeServiceProvider.EffectiveDate))
              {
                ExitState = "ACO_NE0000_DATE_MUST_BE_FUTURE";

                var field =
                  GetField(export.Export1.Item.OfficeServiceProvider,
                  "discontinueDate");

                field.Error = true;

                return;
              }

              if (export.Export1.Item.OfficeServiceProvider.WorkPhoneNumber == 0
                )
              {
                // *******************************************
                // IF WORK PHONE IS NOT ENTERED, DEFAULT TO THE MAIN OFFICE 
                // PHONE
                // *******************************************
                export.Export1.Update.OfficeServiceProvider.WorkPhoneNumber =
                  export.Export1.Item.ServiceProvider.PhoneNumber.
                    GetValueOrDefault();
                export.Export1.Update.OfficeServiceProvider.WorkPhoneAreaCode =
                  export.Export1.Item.ServiceProvider.PhoneAreaCode.
                    GetValueOrDefault();
              }
              else if (export.Export1.Item.OfficeServiceProvider.
                WorkPhoneAreaCode == 0)
              {
                // *******************************************
                // IF WORK PHONE AREA CODE IS NOT ENTERED, DEFAULT TO THE MAIN 
                // OFFICE AREA CODE
                // *******************************************
                export.Export1.Update.OfficeServiceProvider.WorkPhoneNumber =
                  export.Export1.Item.ServiceProvider.PhoneNumber.
                    GetValueOrDefault();
                export.Export1.Update.OfficeServiceProvider.WorkPhoneAreaCode =
                  export.Export1.Item.ServiceProvider.PhoneAreaCode.
                    GetValueOrDefault();
              }

              if (export.Export1.Item.OfficeServiceProvider.WorkPhoneAreaCode ==
                0)
              {
                export.Export1.Update.OfficeServiceProvider.WorkPhoneNumber =
                  7572445;
                export.Export1.Update.OfficeServiceProvider.WorkPhoneAreaCode =
                  888;
              }
            }

            // If Add, call validate code value to validate the role. Role code 
            // cannot be changed via update since it is part of the identifier
            // for Office Service Provider.
            if (Equal(global.Command, "ADD"))
            {
              export.HiddenCode.CodeName = "OFFICE SERVICE PROVIDER ROLE";
              export.HiddenCodeValue.Cdvalue =
                export.Export1.Item.OfficeServiceProvider.RoleCode;
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnCode.Flag) != 'Y')
              {
                ExitState = "INVALID_ROLE_CODE";

                var field =
                  GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");
                  

                field.Error = true;

                return;
              }

              if (Lt(export.Export1.Item.OfficeServiceProvider.EffectiveDate,
                local.Current.Date))
              {
                ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

                var field =
                  GetField(export.Export1.Item.OfficeServiceProvider,
                  "effectiveDate");

                field.Error = true;

                return;
              }

              // *********************************************
              // VALIDATE THERE IS NOT AN ACTIVE SERVICE PROVIDER AND ROLE IN 
              // THE SAME OFFICE
              // *********************************************
              if (ReadServiceProvider1())
              {
                if (!Equal(export.Export1.Item.OfficeServiceProvider.RoleCode,
                  entities.ServiceProvider.RoleCode))
                {
                  var field =
                    GetField(export.Export1.Item.OfficeServiceProvider,
                    "roleCode");

                  field.Error = true;

                  ExitState = "ROLES_DO_NOT_MATCH";

                  return;
                }

                export.Export1.Update.ServiceProvider.Assign(
                  entities.ServiceProvider);
                MoveServiceProvider2(entities.ServiceProvider,
                  export.Export1.Update.HiddenServiceProvider);
              }
              else
              {
                ExitState = "SERVICE_PROVIDER_NF";

                var field =
                  GetField(export.Export1.Item.ServiceProvider,
                  "systemGeneratedId");

                field.Error = true;

                return;
              }

              if (ReadOfficeServiceProvider1())
              {
                ExitState = "SERVICE_PROVIDER_ALREADY_ACTIVE";

                var field6 =
                  GetField(export.Export1.Item.ServiceProvider,
                  "systemGeneratedId");

                field6.Error = true;

                var field7 = GetField(export.Export1.Item.Common, "selectChar");

                field7.Error = true;

                return;
              }

              // Now validate that the new role does not have starting date 
              // overlap with an existing role of the same type in the same
              // office that has a discontinue date greater than the new
              // occurrence starting date.
              if (ReadOfficeServiceProvider2())
              {
                ExitState = "SERVICE_PROVIDER_ALREADY_ACTIVE";

                var field6 =
                  GetField(export.Export1.Item.ServiceProvider,
                  "systemGeneratedId");

                field6.Error = true;

                var field7 = GetField(export.Export1.Item.Common, "selectChar");

                field7.Error = true;

                return;
              }

              if (ReadOfficeServiceProvider3())
              {
                ExitState = "ALREADY_ACT_DIFFERENT_ROLE_CODE";

                var field6 =
                  GetField(export.Export1.Item.ServiceProvider,
                  "systemGeneratedId");

                field6.Error = true;

                var field7 =
                  GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");
                  

                field7.Error = true;

                var field8 = GetField(export.Export1.Item.Common, "selectChar");

                field8.Error = true;

                return;
              }
            }

            switch(TrimEnd(global.Command))
            {
              case "ADD":
                UseSpCreateOfficeServicePrvdr2();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  var field =
                    GetField(export.Export1.Item.Common, "selectChar");

                  field.Error = true;
                }
                else
                {
                  if (!IsEmpty(export.Export1.Item.GeneticTestAccount.
                    AccountNumber))
                  {
                    UseOeUpdateGeneticTestAccount2();
                  }

                  if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                    ("ACO_NI0000_SUCCESSFUL_TRANSFER"))
                  {
                    ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
                    export.Export1.Update.HiddenServiceProvider.Assign(
                      export.Export1.Item.ServiceProvider);
                    export.Export1.Update.HiddenOfficeServiceProvider.Assign(
                      export.Export1.Item.OfficeServiceProvider);

                    // now check to see if they have a current supervisor, if 
                    // they do then tie them to them
                    if (ReadOfficeServiceProviderServiceProviderOffice())
                    {
                      if (ReadOfficeServiceProvider4())
                      {
                        // good to create relationship with worker
                        local.New1.Assign(
                          entities.Leader2NdReadOfficeServiceProvider);
                      }
                      else
                      {
                        // not a supervisor in a office so we need to create a 
                        // office service provider record for this service
                        // provider
                        local.New1.WorkFaxAreaCode =
                          entities.LeaderOfficeServiceProvider.WorkFaxAreaCode;
                        local.New1.WorkFaxNumber =
                          entities.LeaderOfficeServiceProvider.WorkFaxNumber;
                        local.New1.WorkPhoneAreaCode =
                          entities.LeaderOfficeServiceProvider.
                            WorkPhoneAreaCode;
                        local.New1.WorkPhoneNumber =
                          entities.LeaderOfficeServiceProvider.WorkPhoneNumber;
                        local.New1.WorkPhoneExtension =
                          entities.LeaderOfficeServiceProvider.
                            WorkPhoneExtension;
                        local.New1.LocalContactCodeForIrs =
                          entities.LeaderOfficeServiceProvider.
                            LocalContactCodeForIrs;
                        local.New1.RoleCode =
                          entities.LeaderOfficeServiceProvider.RoleCode;
                        local.New1.EffectiveDate = local.Current.Date;
                        local.New1.DiscontinueDate =
                          UseCabSetMaximumDiscontinueDate1();
                        ExitState = "ACO_NN0000_ALL_OK";
                        UseSpCreateOfficeServicePrvdr1();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          break;
                        }
                      }

                      UseSpCreateServicePrvdrRelation();

                      if (!IsExitState("ACO_NI0000_ADD_SUCCESSFUL"))
                      {
                        break;
                      }
                    }
                  }
                }

                break;
              case "UPDATE":
                UseSpUpdateOfficeServicePrvdr();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  var field =
                    GetField(export.Export1.Item.Common, "selectChar");

                  field.Error = true;
                }
                else
                {
                  UseOeUpdateGeneticTestAccount1();

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
                    export.Export1.Update.HiddenServiceProvider.Assign(
                      export.Export1.Item.ServiceProvider);
                    export.Export1.Update.HiddenOfficeServiceProvider.Assign(
                      export.Export1.Item.OfficeServiceProvider);
                  }
                  else if (IsExitState("ACO_NI0000_SUCCESSFUL_TRANSFER"))
                  {
                    export.Export1.Update.HiddenServiceProvider.Assign(
                      export.Export1.Item.ServiceProvider);
                    export.Export1.Update.HiddenOfficeServiceProvider.Assign(
                      export.Export1.Item.OfficeServiceProvider);
                  }
                  else
                  {
                    var field =
                      GetField(export.Export1.Item.Common, "selectChar");

                    field.Error = true;
                  }
                }

                break;
              case "DELETE":
                UseSpDeleteOfficeServicePrvdr();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.Common, "selectChar");

                  field.Error = true;

                  goto Test4;
                }

                break;
              default:
                break;
            }

            local.DateWorkArea.Date =
              export.Export1.Item.OfficeServiceProvider.DiscontinueDate;
            export.Export1.Update.OfficeServiceProvider.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate3();

            if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
              IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") && !
              IsExitState("ACO_NI0000_SUCCESSFUL_DELETE") && !
              IsExitState("ACO_NN0000_ALL_OK") && !
              IsExitState("ACO_NI0000_SUCCESSFUL_TRANSFER"))
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              goto Test4;
            }
            else
            {
              export.Export1.Update.Common.SelectChar = "*";

              var field6 =
                GetField(export.Export1.Item.ServiceProvider,
                "systemGeneratedId");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 =
                GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");
                

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 =
                GetField(export.Export1.Item.OfficeServiceProvider,
                "effectiveDate");

              field8.Color = "cyan";
              field8.Protected = true;
            }
          }
          else if (AsChar(export.Export1.Item.Common.SelectChar) == '*')
          {
            export.Export1.Update.Common.SelectChar = "";
          }
          else
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            continue;
          }

          var field1 =
            GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Export1.Item.OfficeServiceProvider, "effectiveDate");
            

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 =
            GetField(export.Export1.Item.ListServiceProvider, "selectChar");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.Export1.Item.ListRole, "selectChar");

          field5.Color = "cyan";
          field5.Protected = true;
        }
      }
    }
    else
    {
      // Main CASE OF COMMAND.
      switch(TrimEnd(global.Command))
      {
        case "RETURN":
          if (local.Count.Count > 1)
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }
          else
          {
            ExitState = "ACO_NE0000_RETURN";
          }

          break;
        case "DISPLAY":
          if (AsChar(export.ListOffice.SelectChar) == 'S')
          {
            export.ListOffice.SelectChar = "";
          }

          if (export.Office.SystemGeneratedId == 0)
          {
            var field = GetField(export.Office, "systemGeneratedId");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            goto Test4;
          }

          // CQ50114 12/22/15 prompt for flag removed from screen
          if (export.Office.SystemGeneratedId > 0)
          {
            var field = GetField(export.Search, "lastName");

            field.Protected = false;
            field.Focused = true;
          }

          if (ReadOfficeOfficeAddress())
          {
            export.Office.Assign(entities.Office);
            MoveOfficeAddress(entities.OfficeAddress, export.OfficeAddress);
            export.HiddenOffice.Assign(entities.Office);

            if (ReadCodeValue())
            {
              export.OfficeType.Description = entities.CodeValue.Description;
            }
            else
            {
              ExitState = "CODE_VALUE_NF";
            }

            // CQ50114 12/22/15 prompt for flag removed from screen
            // CQ50114 12/22/15 prompt for flag removed from screen
            // CQ50114 12/22/15 Removed the READ out of the IF statement for the
            // discontinued OSP flag
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadServiceProviderOfficeServiceProvider())
            {
              if (ReadGeneticTestAccount())
              {
                export.Export1.Update.GeneticTestAccount.AccountNumber =
                  entities.GeneticTestAccount.AccountNumber;
                export.Export1.Update.HiddenGeneticTestAccount.AccountNumber =
                  entities.GeneticTestAccount.AccountNumber;
              }

              export.Export1.Update.ServiceProvider.Assign(
                entities.ServiceProvider);
              export.Export1.Update.OfficeServiceProvider.Assign(
                entities.OfficeServiceProvider);
              MoveServiceProvider2(entities.ServiceProvider,
                export.Export1.Update.HiddenServiceProvider);
              MoveOfficeServiceProvider3(entities.OfficeServiceProvider,
                export.Export1.Update.HiddenOfficeServiceProvider);

              // if no phone nnumber in office service provide then get if from 
              // service provider. if no phone number on service prodvider then
              // use default call center number 888-757-2448
              if (entities.OfficeServiceProvider.WorkPhoneAreaCode <= 0)
              {
                export.Export1.Update.OfficeServiceProvider.WorkPhoneAreaCode =
                  entities.ServiceProvider.PhoneAreaCode.GetValueOrDefault();
                export.Export1.Update.OfficeServiceProvider.WorkPhoneNumber =
                  entities.ServiceProvider.PhoneNumber.GetValueOrDefault();

                if (Lt(0, entities.ServiceProvider.PhoneExtension))
                {
                  export.Export1.Update.OfficeServiceProvider.
                    WorkPhoneExtension =
                      NumberToString(entities.ServiceProvider.PhoneExtension.
                      GetValueOrDefault(), 11, 5);
                  local.PhoneExtCount.Count =
                    Verify(export.Export1.Item.OfficeServiceProvider.
                      WorkPhoneExtension, "123456789");

                  if (local.PhoneExtCount.Count > 1)
                  {
                    export.Export1.Update.OfficeServiceProvider.
                      WorkPhoneExtension =
                        Substring(export.Export1.Item.OfficeServiceProvider.
                        WorkPhoneExtension, local.PhoneExtCount.Count, 5 - local
                      .PhoneExtCount.Count + 1);
                  }
                }

                export.Export1.Update.HiddenOfficeServiceProvider.
                  WorkPhoneNumber =
                    export.Export1.Item.OfficeServiceProvider.WorkPhoneNumber;
              }

              export.Export1.Update.Common.SelectChar = "";
              local.DateWorkArea.Date =
                export.Export1.Item.OfficeServiceProvider.DiscontinueDate;
              export.Export1.Update.OfficeServiceProvider.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate3();

              var field1 =
                GetField(export.Export1.Item.ServiceProvider,
                "systemGeneratedId");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");
                

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Export1.Item.OfficeServiceProvider,
                "effectiveDate");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 =
                GetField(export.Export1.Item.ListServiceProvider, "selectChar");
                

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.Export1.Item.ListRole, "selectChar");

              field5.Color = "cyan";
              field5.Protected = true;

              export.Export1.Next();
            }

            if (export.Export1.IsEmpty)
            {
              ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
            }
            else
            {
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }
          }
          else
          {
            ExitState = "FN0000_OFFICE_NF";

            var field = GetField(export.Office, "systemGeneratedId");

            field.Error = true;
          }

          break;
        case "LIST":
          break;
        case "SIGNOFF":
          UseScCabSignoff();

          return;
        case "NEXT":
          ExitState = "ACO_NI0000_LIST_IS_FULL";

          break;
        case "PREV":
          ExitState = "ACO_NI0000_TOP_OF_PG_4_CRITERIA";

          break;
        case "ENTER":
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_COMMAND";

          break;
      }
    }

Test4:

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!Lt(local.Current.Date,
        export.Export1.Item.OfficeServiceProvider.EffectiveDate))
      {
        if (!IsEmpty(export.Export1.Item.OfficeServiceProvider.RoleCode))
        {
          var field =
            GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");

          field.Color = "cyan";
          field.Protected = true;
        }

        if (export.Export1.Item.ServiceProvider.SystemGeneratedId > 0)
        {
          var field =
            GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");

          field.Color = "cyan";
          field.Protected = true;
        }

        if (Lt(export.Export1.Item.OfficeServiceProvider.EffectiveDate,
          local.Current.Date) && !
          Equal(export.Export1.Item.OfficeServiceProvider.EffectiveDate,
          local.Null1.Date))
        {
          var field =
            GetField(export.Export1.Item.OfficeServiceProvider, "effectiveDate");
            

          field.Color = "cyan";
          field.Protected = true;
        }
      }
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
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

  private static void MoveOfficeAddress(OfficeAddress source,
    OfficeAddress target)
  {
    target.City = source.City;
    target.StateProvince = source.StateProvince;
  }

  private static void MoveOfficeServiceProvider1(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.WorkPhoneExtension = source.WorkPhoneExtension;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.RoleCode = source.RoleCode;
    target.WorkPhoneNumber = source.WorkPhoneNumber;
    target.WorkFaxNumber = source.WorkFaxNumber;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.LocalContactCodeForIrs = source.LocalContactCodeForIrs;
  }

  private static void MoveOfficeServiceProvider2(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.WorkPhoneNumber = source.WorkPhoneNumber;
    target.WorkFaxNumber = source.WorkFaxNumber;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveOfficeServiceProvider3(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.WorkPhoneNumber = source.WorkPhoneNumber;
    target.WorkFaxNumber = source.WorkFaxNumber;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.LocalContactCodeForIrs = source.LocalContactCodeForIrs;
  }

  private static void MoveOfficeServiceProvider4(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveServiceProvider1(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.CertificationNumber = source.CertificationNumber;
  }

  private static void MoveServiceProvider2(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.CertificationNumber = source.CertificationNumber;
    target.RoleCode = source.RoleCode;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Null1.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate3()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = export.HiddenCodeValue.Cdvalue;
    useImport.Code.CodeName = export.HiddenCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseOeUpdateGeneticTestAccount1()
  {
    var useImport = new OeUpdateGeneticTestAccount.Import();
    var useExport = new OeUpdateGeneticTestAccount.Export();

    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.New1.AccountNumber =
      export.Export1.Item.GeneticTestAccount.AccountNumber;
    useImport.OfficeServiceProvider.Assign(
      export.Export1.Item.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;
    useImport.Old.AccountNumber =
      export.Export1.Item.HiddenGeneticTestAccount.AccountNumber;

    Call(OeUpdateGeneticTestAccount.Execute, useImport, useExport);
  }

  private void UseOeUpdateGeneticTestAccount2()
  {
    var useImport = new OeUpdateGeneticTestAccount.Import();
    var useExport = new OeUpdateGeneticTestAccount.Export();

    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.New1.AccountNumber =
      export.Export1.Item.GeneticTestAccount.AccountNumber;
    useImport.OfficeServiceProvider.Assign(
      export.Export1.Item.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;

    Call(OeUpdateGeneticTestAccount.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

  private void UseSpCabValidateForOspAssigns()
  {
    var useImport = new SpCabValidateForOspAssigns.Import();
    var useExport = new SpCabValidateForOspAssigns.Export();

    useImport.Office.SystemGeneratedId = export.Office.SystemGeneratedId;
    MoveOfficeServiceProvider4(export.Export1.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;

    Call(SpCabValidateForOspAssigns.Execute, useImport, useExport);
  }

  private void UseSpCreateOfficeServicePrvdr1()
  {
    var useImport = new SpCreateOfficeServicePrvdr.Import();
    var useExport = new SpCreateOfficeServicePrvdr.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      entities.LeaderServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.OfficeServiceProvider.Assign(local.New1);

    Call(SpCreateOfficeServicePrvdr.Execute, useImport, useExport);
  }

  private void UseSpCreateOfficeServicePrvdr2()
  {
    var useImport = new SpCreateOfficeServicePrvdr.Import();
    var useExport = new SpCreateOfficeServicePrvdr.Export();

    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.OfficeServiceProvider.Assign(
      export.Export1.Item.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;

    Call(SpCreateOfficeServicePrvdr.Execute, useImport, useExport);

    export.Office.SystemGeneratedId = useExport.Office.SystemGeneratedId;
    MoveOfficeServiceProvider1(useExport.OfficeServiceProvider,
      export.Export1.Update.OfficeServiceProvider);
    export.Export1.Update.ServiceProvider.SystemGeneratedId =
      useExport.ServiceProvider.SystemGeneratedId;
  }

  private void UseSpCreateServicePrvdrRelation()
  {
    var useImport = new SpCreateServicePrvdrRelation.Import();
    var useExport = new SpCreateServicePrvdrRelation.Export();

    useImport.LeadingServiceProvider.SystemGeneratedId =
      entities.LeaderServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    MoveOfficeServiceProvider4(local.New1,
      useImport.LeadingOfficeServiceProvider);
    MoveOfficeServiceProvider4(export.Export1.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;

    Call(SpCreateServicePrvdrRelation.Execute, useImport, useExport);
  }

  private void UseSpDeleteOfficeServicePrvdr()
  {
    var useImport = new SpDeleteOfficeServicePrvdr.Import();
    var useExport = new SpDeleteOfficeServicePrvdr.Export();

    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.OfficeServiceProvider.Assign(
      export.Export1.Item.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;

    Call(SpDeleteOfficeServicePrvdr.Execute, useImport, useExport);

    MoveOfficeServiceProvider2(useExport.OfficeServiceProvider,
      export.Export1.Update.OfficeServiceProvider);
  }

  private void UseSpUpdateOfficeServicePrvdr()
  {
    var useImport = new SpUpdateOfficeServicePrvdr.Import();
    var useExport = new SpUpdateOfficeServicePrvdr.Export();

    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.OfficeServiceProvider.Assign(
      export.Export1.Item.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;

    Call(SpUpdateOfficeServicePrvdr.Execute, useImport, useExport);

    MoveOfficeServiceProvider1(useExport.OfficeServiceProvider,
      export.Export1.Update.OfficeServiceProvider);
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetString(command, "typeCode", entities.Office.TypeCode);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Description = db.GetString(reader, 3);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadGeneticTestAccount()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.GeneticTestAccount.Populated = false;

    return Read("ReadGeneticTestAccount",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.GeneticTestAccount.AccountNumber = db.GetString(reader, 0);
        entities.GeneticTestAccount.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.GeneticTestAccount.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.GeneticTestAccount.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.GeneticTestAccount.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.GeneticTestAccount.Populated = true;
      });
  }

  private bool ReadOfficeOfficeAddress()
  {
    entities.OfficeAddress.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeOfficeAddress",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.TypeCode = db.GetString(reader, 2);
        entities.Office.Name = db.GetString(reader, 3);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.OfficeAddress.Type1 = db.GetString(reader, 6);
        entities.OfficeAddress.City = db.GetString(reader, 7);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 8);
        entities.OfficeAddress.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetString(
          command, "roleCode",
          export.Export1.Item.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.OfficeServiceProvider.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 8);
        entities.OfficeServiceProvider.CreatedBy = db.GetString(reader, 9);
        entities.OfficeServiceProvider.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 11);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 12);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 13);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 14);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider2()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetString(
          command, "roleCode",
          export.Export1.Item.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "discontinueDate",
          export.Export1.Item.OfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.OfficeServiceProvider.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 8);
        entities.OfficeServiceProvider.CreatedBy = db.GetString(reader, 9);
        entities.OfficeServiceProvider.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 11);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 12);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 13);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 14);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider3()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider3",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetString(
          command, "roleCode", entities.ServiceProvider.RoleCode ?? "");
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.OfficeServiceProvider.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 8);
        entities.OfficeServiceProvider.CreatedBy = db.GetString(reader, 9);
        entities.OfficeServiceProvider.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 11);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 12);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 13);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 14);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider4()
  {
    entities.Leader2NdReadOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.LeaderServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Leader2NdReadOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.Leader2NdReadOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.Leader2NdReadOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.Leader2NdReadOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.Leader2NdReadOfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 4);
        entities.Leader2NdReadOfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.Leader2NdReadOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.Leader2NdReadOfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.Leader2NdReadOfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.Leader2NdReadOfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.Leader2NdReadOfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.Leader2NdReadOfficeServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider()
  {
    entities.IdOffice.Populated = false;
    entities.IdServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "locContForIrs",
          export.Export1.Item.OfficeServiceProvider.LocalContactCodeForIrs.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.IdServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.IdOffice.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.OfficeServiceProvider.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 8);
        entities.OfficeServiceProvider.CreatedBy = db.GetString(reader, 9);
        entities.OfficeServiceProvider.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 11);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 12);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 13);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 14);
        entities.IdOffice.OffOffice = db.GetNullableInt32(reader, 15);
        entities.IdOffice.Populated = true;
        entities.IdServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice()
  {
    entities.LeaderOffice.Populated = false;
    entities.OfficeServiceProvRelationship.Populated = false;
    entities.LeaderOfficeServiceProvider.Populated = false;
    entities.LeaderServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          export.Export1.Item.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.LeaderOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.LeaderServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProvRelationship.SpdRGeneratedId =
          db.GetInt32(reader, 0);
        entities.LeaderOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.OfficeServiceProvRelationship.OffRGeneratedId =
          db.GetInt32(reader, 1);
        entities.LeaderOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvRelationship.OspRRoleCode =
          db.GetString(reader, 2);
        entities.LeaderOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.OfficeServiceProvRelationship.OspREffectiveDt =
          db.GetDate(reader, 3);
        entities.LeaderOfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 4);
        entities.LeaderOfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.LeaderOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LeaderOfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.LeaderOfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.LeaderOfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.LeaderOfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.LeaderServiceProvider.UserId = db.GetString(reader, 11);
        entities.LeaderServiceProvider.RoleCode =
          db.GetNullableString(reader, 12);
        entities.LeaderServiceProvider.EffectiveDate =
          db.GetNullableDate(reader, 13);
        entities.LeaderServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 14);
        entities.LeaderOffice.SystemGeneratedId = db.GetInt32(reader, 15);
        entities.LeaderOffice.TypeCode = db.GetString(reader, 16);
        entities.LeaderOffice.Name = db.GetString(reader, 17);
        entities.LeaderOffice.OffOffice = db.GetNullableInt32(reader, 18);
        entities.OfficeServiceProvRelationship.OspEffectiveDate =
          db.GetDate(reader, 19);
        entities.OfficeServiceProvRelationship.OspRoleCode =
          db.GetString(reader, 20);
        entities.OfficeServiceProvRelationship.OffGeneratedId =
          db.GetInt32(reader, 21);
        entities.OfficeServiceProvRelationship.SpdGeneratedId =
          db.GetInt32(reader, 22);
        entities.OfficeServiceProvRelationship.ReasonCode =
          db.GetString(reader, 23);
        entities.LeaderOffice.Populated = true;
        entities.OfficeServiceProvRelationship.Populated = true;
        entities.LeaderOfficeServiceProvider.Populated = true;
        entities.LeaderServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          export.Export1.Item.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ServiceProvider.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.LastName = db.GetString(reader, 6);
        entities.ServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 8);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 9);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 10);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 11);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.ServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.ServiceProvider.PhoneNumber = db.GetNullableInt32(reader, 14);
        entities.ServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 15);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          import.HiddenSelectionServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ServiceProvider.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.LastName = db.GetString(reader, 6);
        entities.ServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 8);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 9);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 10);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 11);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.ServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.ServiceProvider.PhoneNumber = db.GetNullableInt32(reader, 14);
        entities.ServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 15);
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderOfficeServiceProvider()
  {
    return ReadEach("ReadServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "lastName", import.Search.LastName);
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ServiceProvider.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.LastName = db.GetString(reader, 6);
        entities.ServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 8);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 9);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 10);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 11);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.ServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.ServiceProvider.PhoneNumber = db.GetNullableInt32(reader, 14);
        entities.ServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 15);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 16);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 17);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 18);
        entities.OfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 19);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 20);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 21);
        entities.OfficeServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.OfficeServiceProvider.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 23);
        entities.OfficeServiceProvider.CreatedBy = db.GetString(reader, 24);
        entities.OfficeServiceProvider.CreatedTimestamp =
          db.GetDateTime(reader, 25);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 26);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 27);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 28);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 29);
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of HiddenGeneticTestAccount.
      /// </summary>
      [JsonPropertyName("hiddenGeneticTestAccount")]
      public GeneticTestAccount HiddenGeneticTestAccount
      {
        get => hiddenGeneticTestAccount ??= new();
        set => hiddenGeneticTestAccount = value;
      }

      /// <summary>
      /// A value of GeneticTestAccount.
      /// </summary>
      [JsonPropertyName("geneticTestAccount")]
      public GeneticTestAccount GeneticTestAccount
      {
        get => geneticTestAccount ??= new();
        set => geneticTestAccount = value;
      }

      /// <summary>
      /// A value of ListRole.
      /// </summary>
      [JsonPropertyName("listRole")]
      public Common ListRole
      {
        get => listRole ??= new();
        set => listRole = value;
      }

      /// <summary>
      /// A value of HiddenOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenOfficeServiceProvider")]
      public OfficeServiceProvider HiddenOfficeServiceProvider
      {
        get => hiddenOfficeServiceProvider ??= new();
        set => hiddenOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of HiddenServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenServiceProvider")]
      public ServiceProvider HiddenServiceProvider
      {
        get => hiddenServiceProvider ??= new();
        set => hiddenServiceProvider = value;
      }

      /// <summary>
      /// A value of ListServiceProvider.
      /// </summary>
      [JsonPropertyName("listServiceProvider")]
      public Common ListServiceProvider
      {
        get => listServiceProvider ??= new();
        set => listServiceProvider = value;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private GeneticTestAccount hiddenGeneticTestAccount;
      private GeneticTestAccount geneticTestAccount;
      private Common listRole;
      private OfficeServiceProvider hiddenOfficeServiceProvider;
      private ServiceProvider hiddenServiceProvider;
      private Common listServiceProvider;
      private OfficeServiceProvider officeServiceProvider;
      private Common common;
      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// A value of DisplayDisc.
    /// </summary>
    [JsonPropertyName("displayDisc")]
    public Common DisplayDisc
    {
      get => displayDisc ??= new();
      set => displayDisc = value;
    }

    /// <summary>
    /// A value of OfficeType.
    /// </summary>
    [JsonPropertyName("officeType")]
    public CodeValue OfficeType
    {
      get => officeType ??= new();
      set => officeType = value;
    }

    /// <summary>
    /// A value of HiddenSelectionCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenSelectionCodeValue")]
    public CodeValue HiddenSelectionCodeValue
    {
      get => hiddenSelectionCodeValue ??= new();
      set => hiddenSelectionCodeValue = value;
    }

    /// <summary>
    /// A value of HiddenSelectionOffice.
    /// </summary>
    [JsonPropertyName("hiddenSelectionOffice")]
    public Office HiddenSelectionOffice
    {
      get => hiddenSelectionOffice ??= new();
      set => hiddenSelectionOffice = value;
    }

    /// <summary>
    /// A value of HiddenSelectionServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSelectionServiceProvider")]
    public ServiceProvider HiddenSelectionServiceProvider
    {
      get => hiddenSelectionServiceProvider ??= new();
      set => hiddenSelectionServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenOffice.
    /// </summary>
    [JsonPropertyName("hiddenOffice")]
    public Office HiddenOffice
    {
      get => hiddenOffice ??= new();
      set => hiddenOffice = value;
    }

    /// <summary>
    /// A value of ListOffice.
    /// </summary>
    [JsonPropertyName("listOffice")]
    public Common ListOffice
    {
      get => listOffice ??= new();
      set => listOffice = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ServiceProvider Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Leader.
    /// </summary>
    [JsonPropertyName("leader")]
    public ServiceProvider Leader
    {
      get => leader ??= new();
      set => leader = value;
    }

    private Common displayDisc;
    private CodeValue officeType;
    private CodeValue hiddenSelectionCodeValue;
    private Office hiddenSelectionOffice;
    private ServiceProvider hiddenSelectionServiceProvider;
    private Office hiddenOffice;
    private Common listOffice;
    private OfficeAddress officeAddress;
    private Office office;
    private ServiceProvider search;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private ServiceProvider leader;
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
      /// A value of HiddenGeneticTestAccount.
      /// </summary>
      [JsonPropertyName("hiddenGeneticTestAccount")]
      public GeneticTestAccount HiddenGeneticTestAccount
      {
        get => hiddenGeneticTestAccount ??= new();
        set => hiddenGeneticTestAccount = value;
      }

      /// <summary>
      /// A value of GeneticTestAccount.
      /// </summary>
      [JsonPropertyName("geneticTestAccount")]
      public GeneticTestAccount GeneticTestAccount
      {
        get => geneticTestAccount ??= new();
        set => geneticTestAccount = value;
      }

      /// <summary>
      /// A value of ListRole.
      /// </summary>
      [JsonPropertyName("listRole")]
      public Common ListRole
      {
        get => listRole ??= new();
        set => listRole = value;
      }

      /// <summary>
      /// A value of HiddenServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenServiceProvider")]
      public ServiceProvider HiddenServiceProvider
      {
        get => hiddenServiceProvider ??= new();
        set => hiddenServiceProvider = value;
      }

      /// <summary>
      /// A value of HiddenOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenOfficeServiceProvider")]
      public OfficeServiceProvider HiddenOfficeServiceProvider
      {
        get => hiddenOfficeServiceProvider ??= new();
        set => hiddenOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of ListServiceProvider.
      /// </summary>
      [JsonPropertyName("listServiceProvider")]
      public Common ListServiceProvider
      {
        get => listServiceProvider ??= new();
        set => listServiceProvider = value;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private GeneticTestAccount hiddenGeneticTestAccount;
      private GeneticTestAccount geneticTestAccount;
      private Common listRole;
      private ServiceProvider hiddenServiceProvider;
      private OfficeServiceProvider hiddenOfficeServiceProvider;
      private Common listServiceProvider;
      private OfficeServiceProvider officeServiceProvider;
      private Common common;
      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// A value of DisplayDisc.
    /// </summary>
    [JsonPropertyName("displayDisc")]
    public Common DisplayDisc
    {
      get => displayDisc ??= new();
      set => displayDisc = value;
    }

    /// <summary>
    /// A value of OfficeType.
    /// </summary>
    [JsonPropertyName("officeType")]
    public CodeValue OfficeType
    {
      get => officeType ??= new();
      set => officeType = value;
    }

    /// <summary>
    /// A value of HiddenSelectionOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSelectionOfficeServiceProvider")]
    public OfficeServiceProvider HiddenSelectionOfficeServiceProvider
    {
      get => hiddenSelectionOfficeServiceProvider ??= new();
      set => hiddenSelectionOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenCodeValue")]
    public CodeValue HiddenCodeValue
    {
      get => hiddenCodeValue ??= new();
      set => hiddenCodeValue = value;
    }

    /// <summary>
    /// A value of HiddenCode.
    /// </summary>
    [JsonPropertyName("hiddenCode")]
    public Code HiddenCode
    {
      get => hiddenCode ??= new();
      set => hiddenCode = value;
    }

    /// <summary>
    /// A value of HiddenOffice.
    /// </summary>
    [JsonPropertyName("hiddenOffice")]
    public Office HiddenOffice
    {
      get => hiddenOffice ??= new();
      set => hiddenOffice = value;
    }

    /// <summary>
    /// A value of HiddenSelectionOffice.
    /// </summary>
    [JsonPropertyName("hiddenSelectionOffice")]
    public Office HiddenSelectionOffice
    {
      get => hiddenSelectionOffice ??= new();
      set => hiddenSelectionOffice = value;
    }

    /// <summary>
    /// A value of ListOffice.
    /// </summary>
    [JsonPropertyName("listOffice")]
    public Common ListOffice
    {
      get => listOffice ??= new();
      set => listOffice = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ServiceProvider Search
    {
      get => search ??= new();
      set => search = value;
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

    /// <summary>
    /// A value of HiddenSelectionServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSelectionServiceProvider")]
    public ServiceProvider HiddenSelectionServiceProvider
    {
      get => hiddenSelectionServiceProvider ??= new();
      set => hiddenSelectionServiceProvider = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    private Common displayDisc;
    private CodeValue officeType;
    private OfficeServiceProvider hiddenSelectionOfficeServiceProvider;
    private CodeValue hiddenCodeValue;
    private Code hiddenCode;
    private Office hiddenOffice;
    private Office hiddenSelectionOffice;
    private Common listOffice;
    private OfficeAddress officeAddress;
    private Office office;
    private ServiceProvider search;
    private Array<ExportGroup> export1;
    private ServiceProvider hiddenSelectionServiceProvider;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common sortBy;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A XferGroup group.</summary>
    [Serializable]
    public class XferGroup
    {
      /// <summary>
      /// A value of GeneticTestAccount.
      /// </summary>
      [JsonPropertyName("geneticTestAccount")]
      public GeneticTestAccount GeneticTestAccount
      {
        get => geneticTestAccount ??= new();
        set => geneticTestAccount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private GeneticTestAccount geneticTestAccount;
    }

    /// <summary>
    /// A value of PhoneExtCount.
    /// </summary>
    [JsonPropertyName("phoneExtCount")]
    public Common PhoneExtCount
    {
      get => phoneExtCount ??= new();
      set => phoneExtCount = value;
    }

    /// <summary>
    /// A value of Xfer1.
    /// </summary>
    [JsonPropertyName("xfer1")]
    public Common Xfer1
    {
      get => xfer1 ??= new();
      set => xfer1 = value;
    }

    /// <summary>
    /// Gets a value of Xfer.
    /// </summary>
    [JsonIgnore]
    public Array<XferGroup> Xfer => xfer ??= new(XferGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Xfer for json serialization.
    /// </summary>
    [JsonPropertyName("xfer")]
    [Computed]
    public IList<XferGroup> Xfer_Json
    {
      get => xfer;
      set => Xfer.Assign(value);
    }

    /// <summary>
    /// A value of SelectCount.
    /// </summary>
    [JsonPropertyName("selectCount")]
    public Common SelectCount
    {
      get => selectCount ??= new();
      set => selectCount = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public OfficeServiceProvider New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Common phoneExtCount;
    private Common xfer1;
    private Array<XferGroup> xfer;
    private Common selectCount;
    private DateWorkArea current;
    private DateWorkArea null1;
    private DateWorkArea dateWorkArea;
    private Common returnCode;
    private Common count;
    private OfficeServiceProvider new1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Leader2NdReadOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("leader2NdReadOfficeServiceProvider")]
    public OfficeServiceProvider Leader2NdReadOfficeServiceProvider
    {
      get => leader2NdReadOfficeServiceProvider ??= new();
      set => leader2NdReadOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of Leader2NdReadServiceProvider.
    /// </summary>
    [JsonPropertyName("leader2NdReadServiceProvider")]
    public ServiceProvider Leader2NdReadServiceProvider
    {
      get => leader2NdReadServiceProvider ??= new();
      set => leader2NdReadServiceProvider = value;
    }

    /// <summary>
    /// A value of LeaderOffice.
    /// </summary>
    [JsonPropertyName("leaderOffice")]
    public Office LeaderOffice
    {
      get => leaderOffice ??= new();
      set => leaderOffice = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
    }

    /// <summary>
    /// A value of LeaderOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderOfficeServiceProvider")]
    public OfficeServiceProvider LeaderOfficeServiceProvider
    {
      get => leaderOfficeServiceProvider ??= new();
      set => leaderOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of LeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderServiceProvider")]
    public ServiceProvider LeaderServiceProvider
    {
      get => leaderServiceProvider ??= new();
      set => leaderServiceProvider = value;
    }

    /// <summary>
    /// A value of IdOffice.
    /// </summary>
    [JsonPropertyName("idOffice")]
    public Office IdOffice
    {
      get => idOffice ??= new();
      set => idOffice = value;
    }

    /// <summary>
    /// A value of IdServiceProvider.
    /// </summary>
    [JsonPropertyName("idServiceProvider")]
    public ServiceProvider IdServiceProvider
    {
      get => idServiceProvider ??= new();
      set => idServiceProvider = value;
    }

    /// <summary>
    /// A value of GeneticTestAccount.
    /// </summary>
    [JsonPropertyName("geneticTestAccount")]
    public GeneticTestAccount GeneticTestAccount
    {
      get => geneticTestAccount ??= new();
      set => geneticTestAccount = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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

    private OfficeServiceProvider leader2NdReadOfficeServiceProvider;
    private ServiceProvider leader2NdReadServiceProvider;
    private Office leaderOffice;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private OfficeServiceProvider leaderOfficeServiceProvider;
    private ServiceProvider leaderServiceProvider;
    private Office idOffice;
    private ServiceProvider idServiceProvider;
    private GeneticTestAccount geneticTestAccount;
    private Code code;
    private CodeValue codeValue;
    private OfficeServiceProvider officeServiceProvider;
    private OfficeAddress officeAddress;
    private Office office;
    private ServiceProvider serviceProvider;
  }
#endregion
}
