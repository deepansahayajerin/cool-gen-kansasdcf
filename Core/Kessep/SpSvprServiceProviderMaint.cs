// Program: SP_SVPR_SERVICE_PROVIDER_MAINT, ID: 371454563, model: 746.
// Short name: SWESVPRP
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
/// A program: SP_SVPR_SERVICE_PROVIDER_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpSvprServiceProviderMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_SVPR_SERVICE_PROVIDER_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpSvprServiceProviderMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpSvprServiceProviderMaint.
  /// </summary>
  public SpSvprServiceProviderMaint(IContext context, Import import,
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
    // -----------------------------------------------------------------------------
    // DATE		DEVELOPER	REQUEST		DESCRIPTION
    // -----------------------------------------------------------------------------
    // 05/15/95	a. hackler			Initial development
    // 09/25/95	j. kemp				removed confirm delete logic to conform to standards
    // 01/06/96	A. HACKLER			ADD CODE TO REQUIRE A USERID, ADD CALL TO TOP-
    // SECRET TO VALIDATE THE USERID
    // 02/02/96	A. HACKLER			RETRO FITS
    // 01/04/97	R. Marchman			Add new security/next tran.
    // 11/16/98	A Massey			Fixes per screen assessment
    // 11/12/99	SWSRCHF	H00079599		Validate that a changed USER_ID does not 
    // exist on an UPDATE (PF6 key).  On an ADD (PF5 key), added check on Middle
    // Initial to the read of Service_Provider
    // 02/12/2001	M Ramirez	WR# 187		Add eMail address
    // 02/14/2001    Madhu Kumar                  WR#286 A
    // Add certification number
    // -----------------------------------------------------------------------------
    // *****************************************************************
    //           PR to add edit checks for zip code .
    // PR# 114679  and PR# 116889.
    //                                                    
    // Madhu Kumar
    // *****************************************************************
    // --------------------------------------------------------------------------------------
    // Per WR# 010349  : User can add/update First_Name, MI, Last_Name, USER_ID 
    // only if s/he has "SECURITY" profile.
    //                                                      
    // --- Vithal (05/01/2001)
    // ---------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------
    // Per PR# 139713  : User can delete an address by entering an 'S' in Sel 
    // field,   if s/he has "DEVELOPERS" profile. (Change requested by SME, Sana
    // Beall).
    //                                                      
    // --- Vithal (03/07/2002)
    // ---------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------------------------
    // Per PR# 189075   :  Changed code so return from list goes thru the 
    // security check.
    // 
    // --- B. Lee
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }
    else if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";
    }

    export.Standard.Assign(import.Standard);
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.PromptServiceProvider.Flag = import.PromptServiceProvider.Flag;
    export.PromptRoleCode.Flag = import.PromptRoleCode.Flag;
    export.HiddenServiceProvider.Assign(import.HiddenServiceProvider);
    export.Discontinue.Date = import.Discontinue.Date;
    export.HiddenDiscontinue.Date = import.HiddenDiscontinue.Date;

    if (!Lt(local.Null1.Date, import.ServiceProvider.EffectiveDate))
    {
      var field = GetField(export.ServiceProvider, "effectiveDate");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
    }
    else
    {
      var field = GetField(export.ServiceProvider, "effectiveDate");

      field.Color = "cyan";
      field.Highlighting = Highlighting.Normal;
      field.Protected = true;
    }

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.ServiceProviderAddress.Assign(
        import.Group.Item.ServiceProviderAddress);
      export.Group.Update.Hidden.Type1 = import.Group.Item.Hidden.Type1;
      export.Group.Update.PromptState.Flag = import.Group.Item.PromptState.Flag;
      export.Group.Update.PromptAddressType.Flag =
        import.Group.Item.PromptAddressType.Flag;
      export.Group.Update.AddressTypeDesc.Description =
        import.Group.Item.AddressTypeDesc.Description;
      ++local.TotalCount.Count;

      if (AsChar(import.Group.Item.Common.SelectChar) == 'S')
      {
        ++local.SelectedCount.Count;
      }

      export.Group.Next();
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "LIST") || Equal
      (global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "PREV") || Equal(global.Command, "DELETE"))
    {
    }
    else
    {
      // if the next tran info is not equal to spaces, this implies the user 
      // requested a next tran action. now validate
      if (IsEmpty(import.Standard.NextTransaction))
      {
      }
      else
      {
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
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    if (Equal(global.Command, "RLCVAL"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (IsEmpty(export.Group.Item.PromptAddressType.Flag))
        {
        }
        else
        {
          var field = GetField(export.Group.Item.PromptAddressType, "flag");

          field.Protected = false;
          field.Focused = true;

          export.Group.Update.PromptAddressType.Flag = "";
          export.Group.Update.ServiceProviderAddress.Type1 =
            import.HiddenFromCodValList.Cdvalue;
          export.Group.Update.AddressTypeDesc.Description =
            import.HiddenFromCodValList.Description;

          return;
        }

        if (IsEmpty(export.Group.Item.PromptState.Flag))
        {
        }
        else
        {
          var field = GetField(export.Group.Item.PromptState, "flag");

          field.Protected = false;
          field.Focused = true;

          export.Group.Update.PromptState.Flag = "";
          export.Group.Update.ServiceProviderAddress.StateProvince =
            import.HiddenFromCodValList.Cdvalue;
          export.Group.Update.AddressTypeDesc.Description =
            import.HiddenFromCodValList.Description;

          return;
        }
      }

      if (IsEmpty(export.PromptServiceProvider.Flag))
      {
      }
      else
      {
        var field = GetField(export.PromptServiceProvider, "flag");

        field.Protected = false;
        field.Focused = true;

        export.PromptServiceProvider.Flag = "";

        if (import.HiddenFromList.SystemGeneratedId > 0)
        {
          export.ServiceProvider.SystemGeneratedId =
            import.HiddenFromList.SystemGeneratedId;
          global.Command = "DISPLAY";
        }
      }

      if (IsEmpty(export.PromptRoleCode.Flag))
      {
      }
      else
      {
        var field = GetField(export.PromptRoleCode, "flag");

        field.Protected = false;
        field.Focused = true;

        export.PromptRoleCode.Flag = "";
        export.ServiceProvider.RoleCode = import.HiddenFromCodValList.Cdvalue;
      }
    }

    if (Equal(global.Command, "RLCVAL"))
    {
    }
    else
    {
      // --------------------------------------------------------------------------------------
      // Per PR# 139713  : User can delete an address by entering an 'S' in Sel 
      // field,   if s/he has "DEVELOPERS" profile. (Change requested by SME,
      // Sana Beall).
      //                                                      
      // --- Vithal (03/07/2002)
      // ---------------------------------------------------------------------------------------
      if (Equal(global.Command, "DELETE"))
      {
        if (local.SelectedCount.Count >= 1)
        {
          if (ReadProfileServiceProviderProfile())
          {
            // ----------------------------------------------------------
            // User has the "DEVELOPERS" profile to DELETE the selected address 
            // record. Bypass the Security cab.
            // --------------------------------------------------------
            goto Test1;
          }
        }
      }

      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

Test1:

    if (Equal(global.Command, "ADD"))
    {
      if (!IsEmpty(import.ServiceProvider.UserId))
      {
        if (ReadServiceProvider3())
        {
          local.UserIdPrefix.Text4 =
            Substring(import.ServiceProvider.UserId, 1, 4);
          local.Code.CodeName = "SERVICE PROVIDERS CSS";
          local.CodeValue.Cdvalue = local.UserIdPrefix.Text4;
          UseCabValidateCodeValue1();

          if (AsChar(local.ValidCode.Flag) == 'Y')
          {
            var field = GetField(export.ServiceProvider, "userId");

            field.Error = true;

            ExitState = "USERID_AE";

            return;
          }

          if (Lt(Now().Date, entities.ServiceProvider.DiscontinueDate))
          {
            var field = GetField(export.ServiceProvider, "userId");

            field.Error = true;

            ExitState = "USERID_AE";

            return;
          }
        }
      }
    }

    // *** Problem report H00079599
    // *** 11/12/99 SWSRCHF
    // changes this check from user id to system generated id is the actual 
    // identifier not user id swdpdjd 10/2018
    // *** start
    if (Equal(global.Command, "UPDATE"))
    {
      if (export.ServiceProvider.SystemGeneratedId != export
        .HiddenServiceProvider.SystemGeneratedId)
      {
        if (ReadServiceProvider2())
        {
          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }
      }
    }

    // *** end
    // *** 11/12/99 SWSRCHF
    // *** Problem report H00079599
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      if (import.ServiceProvider.SystemGeneratedId == import
        .HiddenServiceProvider.SystemGeneratedId)
      {
      }
      else
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

        return;
      }
    }

    if (Equal(global.Command, "UPDATE"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          if (AsChar(export.Group.Item.ServiceProviderAddress.Type1) != AsChar
            (export.Group.Item.Hidden.Type1) && !
            IsEmpty(export.Group.Item.Hidden.Type1))
          {
            var field =
              GetField(export.Group.Item.ServiceProviderAddress, "type1");

            field.Error = true;

            export.Group.Update.ServiceProviderAddress.Type1 =
              export.Group.Item.Hidden.Type1;
            ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";

            return;
          }
        }
      }
    }

    // validate the data
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.ServiceProvider.FirstName))
      {
        var field = GetField(export.ServiceProvider, "firstName");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.ServiceProvider.LastName))
      {
        var field = GetField(export.ServiceProvider, "lastName");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }

      if (IsEmpty(export.ServiceProvider.UserId))
      {
        var field = GetField(export.ServiceProvider, "userId");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }

      if (!Lt(local.Null1.Date, import.ServiceProvider.EffectiveDate))
      {
        export.ServiceProvider.EffectiveDate = local.Current.Date;
      }

      if (!Lt(local.Null1.Date, import.Discontinue.Date) && !
        Lt(local.Null1.Date, import.ServiceProvider.DiscontinueDate))
      {
        export.ServiceProvider.DiscontinueDate =
          UseCabSetMaximumDiscontinueDate();
      }

      if (!Lt(export.ServiceProvider.EffectiveDate,
        export.ServiceProvider.DiscontinueDate))
      {
        var field = GetField(export.Discontinue, "date");

        field.Error = true;

        ExitState = "ACO_NE0000_DATE_MUST_BE_FUTURE";

        return;
      }

      if (!Lt(local.Current.Date, export.ServiceProvider.DiscontinueDate))
      {
        if (!Lt(local.Null1.Date, export.Discontinue.Date) || Equal
          (export.Discontinue.Date, local.Max.Date))
        {
          // activing an inactive service provider so blanking out or max 
          // discontinue date is fine
          goto Test2;
        }

        if (!Equal(export.ServiceProvider.CertificationNumber,
          export.HiddenServiceProvider.CertificationNumber))
        {
          var field = GetField(export.ServiceProvider, "certificationNumber");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_INACTIVE";

          return;
        }

        if (!Equal(export.HiddenServiceProvider.EmailAddress,
          export.ServiceProvider.EmailAddress))
        {
          var field = GetField(export.ServiceProvider, "emailAddress");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_INACTIVE";

          return;
        }

        if (!Equal(export.HiddenServiceProvider.FirstName,
          export.ServiceProvider.FirstName))
        {
          var field = GetField(export.ServiceProvider, "firstName");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_INACTIVE";

          return;
        }

        if (!Equal(export.HiddenServiceProvider.LastName,
          export.ServiceProvider.LastName))
        {
          var field = GetField(export.ServiceProvider, "lastName");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_INACTIVE";

          return;
        }

        if (AsChar(export.HiddenServiceProvider.MiddleInitial) != AsChar
          (export.ServiceProvider.MiddleInitial))
        {
          var field = GetField(export.ServiceProvider, "middleInitial");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_INACTIVE";

          return;
        }

        if (!Equal(export.HiddenServiceProvider.RoleCode,
          export.ServiceProvider.RoleCode))
        {
          var field = GetField(export.ServiceProvider, "roleCode");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_INACTIVE";

          return;
        }

        if (!Equal(export.HiddenServiceProvider.UserId,
          export.ServiceProvider.UserId))
        {
          var field = GetField(export.ServiceProvider, "userId");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_INACTIVE";

          return;
        }

        if (export.HiddenServiceProvider.PhoneAreaCode.GetValueOrDefault() != export
          .ServiceProvider.PhoneAreaCode.GetValueOrDefault())
        {
          var field = GetField(export.ServiceProvider, "phoneAreaCode");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_INACTIVE";

          return;
        }

        if (export.HiddenServiceProvider.PhoneNumber.GetValueOrDefault() != export
          .ServiceProvider.PhoneNumber.GetValueOrDefault())
        {
          var field = GetField(export.ServiceProvider, "phoneNumber");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_INACTIVE";

          return;
        }

        if (export.HiddenServiceProvider.PhoneExtension.GetValueOrDefault() != export
          .ServiceProvider.PhoneExtension.GetValueOrDefault())
        {
          var field = GetField(export.ServiceProvider, "phoneExtension");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_INACTIVE";

          return;
        }
      }

Test2:

      if (!IsEmpty(export.ServiceProvider.RoleCode))
      {
        local.Code.CodeName = "OFFICE SERVICE PROVIDER ROLE";
        local.CodeValue.Cdvalue = export.ServiceProvider.RoleCode ?? Spaces(10);
        UseCabValidateCodeValue1();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
        }
        else
        {
          var field = GetField(export.ServiceProvider, "roleCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_ROLE_CODE";

            return;
          }
        }
      }
      else
      {
        var field = GetField(export.ServiceProvider, "roleCode");

        field.Error = true;

        ExitState = "SP0000_ROLE_CODE_REQUIRED";

        return;
      }

      if (Equal(global.Command, "UPDATE") && !
        IsEmpty(export.ServiceProvider.RoleCode) && !
        IsEmpty(export.HiddenServiceProvider.RoleCode) && !
        Equal(export.ServiceProvider.RoleCode,
        export.HiddenServiceProvider.RoleCode) || Lt
        (local.Null1.Date, export.Discontinue.Date) && !
        Equal(export.Discontinue.Date, export.ServiceProvider.DiscontinueDate))
      {
        foreach(var item in ReadServiceProviderOfficeOfficeServiceProvider1())
        {
          UseSpCabValidateForOspAssigns();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            var field1 = GetField(export.ServiceProvider, "systemGeneratedId");

            field1.Error = true;

            var field2 = GetField(export.ServiceProvider, "roleCode");

            field2.Error = true;

            return;
          }
        }

        if (Lt(local.Null1.Date, export.Discontinue.Date) && Equal
          (export.ServiceProvider.DiscontinueDate, local.Max.Date))
        {
          local.EndingServiceProvider.Flag = "Y";
        }
      }

      if (export.ServiceProvider.PhoneAreaCode.GetValueOrDefault() == 0 && export
        .ServiceProvider.PhoneNumber.GetValueOrDefault() == 0)
      {
        export.ServiceProvider.PhoneAreaCode = 888;
        export.ServiceProvider.PhoneNumber = 7572445;
        export.ServiceProvider.PhoneExtension = 0;
      }

      if (export.ServiceProvider.PhoneAreaCode.GetValueOrDefault() == 0 && export
        .ServiceProvider.PhoneNumber.GetValueOrDefault() > 0)
      {
        var field = GetField(export.ServiceProvider, "phoneAreaCode");

        field.Error = true;

        ExitState = "CO0000_PHONE_AREA_CODE_REQD";

        return;
      }

      if (export.ServiceProvider.PhoneAreaCode.GetValueOrDefault() > 0 && export
        .ServiceProvider.PhoneNumber.GetValueOrDefault() == 0)
      {
        var field = GetField(export.ServiceProvider, "phoneNumber");

        field.Error = true;

        ExitState = "ACO_NE0000_PHONE_NO_REQD";

        return;
      }

      if (export.ServiceProvider.PhoneAreaCode.GetValueOrDefault() == 0 && export
        .ServiceProvider.PhoneNumber.GetValueOrDefault() == 0 && export
        .ServiceProvider.PhoneExtension.GetValueOrDefault() > 0)
      {
        var field = GetField(export.ServiceProvider, "phoneExtension");

        field.Error = true;

        ExitState = "ACO_NE0000_EXTENSION_WO_PHONE_NO";

        return;
      }

      // ***** VERIFY THAT THE USERID HAS BEEN SET UP IN TOP SECRET   *****
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        local.Code.CodeName = "ADDRESS TYPE";
        local.CodeValue.Cdvalue =
          export.Group.Item.ServiceProviderAddress.Type1;
        UseCabValidateCodeValue2();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
        }
        else
        {
          var field =
            GetField(export.Group.Item.ServiceProviderAddress, "type1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_TYPE_CODE";
          }
        }

        if (IsEmpty(export.Group.Item.ServiceProviderAddress.Street1))
        {
          var field =
            GetField(export.Group.Item.ServiceProviderAddress, "street1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
          }
        }

        if (IsEmpty(export.Group.Item.ServiceProviderAddress.City))
        {
          var field =
            GetField(export.Group.Item.ServiceProviderAddress, "city");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
          }
        }

        if (IsEmpty(export.Group.Item.ServiceProviderAddress.StateProvince))
        {
          export.Group.Update.ServiceProviderAddress.StateProvince = "KS";
        }

        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue =
          export.Group.Item.ServiceProviderAddress.StateProvince;
        UseCabValidateCodeValue1();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
        }
        else
        {
          var field =
            GetField(export.Group.Item.ServiceProviderAddress, "stateProvince");
            

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        if (IsEmpty(export.Group.Item.ServiceProviderAddress.Zip))
        {
          var field = GetField(export.Group.Item.ServiceProviderAddress, "zip");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
          }
        }

        if (IsEmpty(export.Group.Item.ServiceProviderAddress.Country))
        {
          export.Group.Update.ServiceProviderAddress.Country = "USA";
        }
      }
    }

    // --------------------------------------------------------------------------------------
    // Per WR# 010349  : User can add/update First_Name, MI, Last_Name, USER_ID 
    // only if s/he has "SECURITY" profile.
    //                                                      
    // --- Vithal (05/01/2001)
    // ---------------------------------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (Equal(global.Command, "DELETE"))
    {
      if (local.SelectedCount.Count == 0)
      {
        // If local_selected_count is equal to 0, user is attempting to delete 
        // an occurrence of Service Provider, which includes a cascade delete of
        // all Service Provider Addresses for that Service Provider.  If
        // local_selected_count is greater than 0, user is attempting to delete
        // a specific occurrence of Service Provider Address for the current
        // Service Provider and the cab validating the delete of a Service
        // Provider does not need to be invoked.
        // JRookard 04-11-96
        UseSpCabValidateSrvPrvdrDelete();

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
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "RLCVAL":
        if (import.HiddenFromList.SystemGeneratedId == 0 && IsEmpty
          (import.HiddenFromCodValList.Cdvalue))
        {
          // NOTHING WAS RETURNED FROM LIST, PRESENT INITIAL VALUE IN FIELD.
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "DISPLAY":
        if (export.ServiceProvider.SystemGeneratedId == 0)
        {
          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        UseCabSpReadSrvcProvAndAddr();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.ServiceProvider, "effectiveDate");

          field.Color = "cyan";
          field.Protected = true;

          export.HiddenServiceProvider.Assign(export.ServiceProvider);
          export.PromptServiceProvider.Flag = "";

          if (Equal(export.ServiceProvider.DiscontinueDate, local.Max.Date))
          {
            export.Discontinue.Date = local.Null1.Date;
          }
          else
          {
            export.Discontinue.Date = export.ServiceProvider.DiscontinueDate;
          }

          export.HiddenDiscontinue.Date = export.Discontinue.Date;
        }
        else
        {
          export.ServiceProvider.Assign(local.ClearServiceProvider);
          export.HiddenServiceProvider.Assign(local.ClearServiceProvider);
          export.ServiceProvider.SystemGeneratedId =
            import.ServiceProvider.SystemGeneratedId;
          export.Discontinue.Date = local.Null1.Date;
          export.HiddenDiscontinue.Date = export.Discontinue.Date;

          export.Group.Index = 0;
          export.Group.Clear();

          for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
            import.Group.Index)
          {
            if (export.Group.IsFull)
            {
              break;
            }

            export.Group.Update.ServiceProviderAddress.City = "";
            export.Group.Update.ServiceProviderAddress.Country = "";
            export.Group.Update.ServiceProviderAddress.PostalCode = "";
            export.Group.Update.ServiceProviderAddress.StateProvince = "";
            export.Group.Update.ServiceProviderAddress.Street1 = "";
            export.Group.Update.ServiceProviderAddress.Street2 = "";
            export.Group.Update.ServiceProviderAddress.Type1 = "";
            export.Group.Update.ServiceProviderAddress.Zip = "";
            export.Group.Update.ServiceProviderAddress.Zip4 = "";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Next();
          }

          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          return;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "LIST":
        local.Count.Count = 0;

        // count the number of selected attributes
        if (AsChar(export.PromptServiceProvider.Flag) == 'S')
        {
          ++local.Count.Count;
        }

        if (AsChar(export.PromptRoleCode.Flag) == 'S')
        {
          ++local.Count.Count;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.PromptAddressType.Flag) == 'S')
          {
            ++local.Count.Count;
          }

          if (AsChar(export.Group.Item.PromptState.Flag) == 'S')
          {
            ++local.Count.Count;
          }
        }

        if (local.Count.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        if (local.Count.Count > 1)
        {
          if (AsChar(export.PromptServiceProvider.Flag) == 'S')
          {
            var field = GetField(export.PromptServiceProvider, "flag");

            field.Error = true;
          }

          if (AsChar(export.PromptRoleCode.Flag) == 'S')
          {
            var field = GetField(export.PromptRoleCode, "flag");

            field.Error = true;
          }

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.PromptAddressType.Flag) == 'S')
            {
              var field = GetField(export.Group.Item.PromptAddressType, "flag");

              field.Error = true;
            }

            if (AsChar(export.Group.Item.PromptState.Flag) == 'S')
            {
              var field = GetField(export.Group.Item.PromptState, "flag");

              field.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(export.PromptServiceProvider.Flag) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

          return;
        }

        if (AsChar(export.PromptRoleCode.Flag) == 'S')
        {
          export.HiddenToCodeValueList.CodeName =
            "OFFICE SERVICE PROVIDER ROLE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.PromptAddressType.Flag) == 'S')
          {
            export.HiddenToCodeValueList.CodeName = "ADDRESS TYPE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }

          if (AsChar(export.Group.Item.PromptState.Flag) == 'S')
          {
            export.HiddenToCodeValueList.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
        }

        break;
      case "ADD":
        if (Lt(local.Null1.Date, export.Discontinue.Date))
        {
          export.ServiceProvider.DiscontinueDate = export.Discontinue.Date;
        }

        export.ServiceProvider.EffectiveDate = Now().Date;
        UseSpCreateServiceProvider();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.ServiceProviderAddress.StateProvince))
          {
            if (Length(TrimEnd(export.Group.Item.ServiceProviderAddress.Zip)) >
              0 && Length
              (TrimEnd(export.Group.Item.ServiceProviderAddress.Zip)) < 5)
            {
              var field =
                GetField(export.Group.Item.ServiceProviderAddress, "zip");

              field.Error = true;

              ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.ServiceProviderAddress.Zip)) >
              0 && Verify
              (export.Group.Item.ServiceProviderAddress.Zip, "0123456789") != 0
              )
            {
              var field =
                GetField(export.Group.Item.ServiceProviderAddress, "zip");

              field.Error = true;

              ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.ServiceProviderAddress.Zip)) ==
              0 && Length
              (TrimEnd(export.Group.Item.ServiceProviderAddress.Zip4)) > 0)
            {
              var field =
                GetField(export.Group.Item.ServiceProviderAddress, "zip4");

              field.Error = true;

              ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.ServiceProviderAddress.Zip)) >
              0 && Length
              (TrimEnd(export.Group.Item.ServiceProviderAddress.Zip4)) > 0)
            {
              if (Length(TrimEnd(export.Group.Item.ServiceProviderAddress.Zip4)) <
                4)
              {
                var field =
                  GetField(export.Group.Item.ServiceProviderAddress, "zip4");

                field.Error = true;

                ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

                return;
              }
              else if (Verify(export.Group.Item.ServiceProviderAddress.Zip4,
                "0123456789") != 0)
              {
                var field =
                  GetField(export.Group.Item.ServiceProviderAddress, "zip4");

                field.Error = true;

                ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

                return;
              }
            }

            UseSpCreateServicePrvdrAddress();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field =
                GetField(export.Group.Item.ServiceProviderAddress, "type1");

              field.Protected = true;

              export.Group.Update.Hidden.Type1 =
                export.Group.Item.ServiceProviderAddress.Type1;
            }
            else
            {
              return;
            }
          }
        }

        export.HiddenServiceProvider.Assign(export.ServiceProvider);
        export.HiddenDiscontinue.Date = export.Discontinue.Date;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        // *** Problem report H00079599
        // *** 11/12/99 SWSRCHF
        // *** added check for middle initial
        // *** start
        if (ReadServiceProvider1())
        {
          ExitState = "SERVICE_PROVIDER_NAME_AE";
        }

        // *** end
        // *** 11/12/99 SWSRCHF
        // *** Problem report H00079599
        break;
      case "DELETE":
        if (local.SelectedCount.Count == 0)
        {
          local.DeletedAll.Flag = "Y";
          UseSpDeleteServiceProvider();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
          }
        }
        else
        {
          local.DeletedAll.Flag = "N";

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseSpDeleteServicePrvdrAddress();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.ServiceProviderAddress.Assign(
                  local.ClearServiceProviderAddress);
                export.Group.Update.Hidden.Type1 =
                  local.ClearServiceProviderAddress.Type1;
              }
              else
              {
                var field =
                  GetField(export.Group.Item.ServiceProviderAddress, "type1");

                field.Error = true;

                break;
              }
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

        if (AsChar(local.DeletedAll.Flag) == 'Y')
        {
        }
        else
        {
          UseCabSpReadSrvcProvAndAddr();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          export.ServiceProvider.SystemGeneratedId =
            import.ServiceProvider.SystemGeneratedId;

          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          var field =
            GetField(export.Group.Item.ServiceProviderAddress, "type1");

          field.Protected = true;
        }

        export.HiddenServiceProvider.Assign(local.ClearServiceProvider);

        // ---------------------------------------------
        // Remove confirmation message and confirmation from the screen.
        // ---------------------------------------------
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "UPDATE":
        local.Count.Count = 0;

        if (local.Count.Count == 0)
        {
          if (export.HiddenServiceProvider.PhoneAreaCode.GetValueOrDefault() !=
            export.ServiceProvider.PhoneAreaCode.GetValueOrDefault())
          {
            local.ChangeOfficeSerProvider.Flag = "Y";
            ++local.Count.Count;

            goto Test3;
          }

          if (export.HiddenServiceProvider.PhoneNumber.GetValueOrDefault() != export
            .ServiceProvider.PhoneNumber.GetValueOrDefault())
          {
            local.ChangeOfficeSerProvider.Flag = "Y";
            ++local.Count.Count;

            goto Test3;
          }

          if (!Equal(export.HiddenServiceProvider.CertificationNumber,
            export.ServiceProvider.CertificationNumber))
          {
            ++local.Count.Count;

            goto Test3;
          }

          if (!Equal(export.HiddenServiceProvider.EffectiveDate,
            export.ServiceProvider.EffectiveDate))
          {
            ++local.Count.Count;

            goto Test3;
          }

          if (!Equal(export.HiddenServiceProvider.EmailAddress,
            export.ServiceProvider.EmailAddress))
          {
            ++local.Count.Count;

            goto Test3;
          }

          if (!Equal(export.HiddenServiceProvider.FirstName,
            export.ServiceProvider.FirstName))
          {
            ++local.Count.Count;

            goto Test3;
          }

          if (!Equal(export.HiddenServiceProvider.LastName,
            export.ServiceProvider.LastName))
          {
            ++local.Count.Count;

            goto Test3;
          }

          if (AsChar(export.HiddenServiceProvider.MiddleInitial) != AsChar
            (export.ServiceProvider.MiddleInitial))
          {
            ++local.Count.Count;

            goto Test3;
          }

          if (!Equal(export.HiddenServiceProvider.UserId,
            export.ServiceProvider.UserId))
          {
            ++local.Count.Count;

            goto Test3;
          }

          if (!Equal(export.HiddenServiceProvider.RoleCode,
            export.ServiceProvider.RoleCode))
          {
            ++local.Count.Count;

            goto Test3;
          }

          if (export.HiddenServiceProvider.PhoneExtension.GetValueOrDefault() !=
            export.ServiceProvider.PhoneExtension.GetValueOrDefault())
          {
            local.ChangeOfficeSerProvider.Flag = "Y";
            ++local.Count.Count;

            goto Test3;
          }

          if (!Equal(export.HiddenDiscontinue.Date, export.Discontinue.Date))
          {
            local.EndingServiceProvider.Flag = "Y";
            ++local.Count.Count;
          }
        }

Test3:

        if (local.Count.Count == 0)
        {
          ExitState = "ACO_NI0000_DATA_WAS_NOT_CHANGED";

          return;
        }

        if (Lt(local.Null1.Date, export.Discontinue.Date))
        {
          // end dating person
          export.ServiceProvider.DiscontinueDate = export.Discontinue.Date;
        }
        else if (!Lt(local.Null1.Date, export.Discontinue.Date) && Lt
          (export.ServiceProvider.DiscontinueDate, local.Max.Date))
        {
          // reactiving the person or adding a person
          export.ServiceProvider.DiscontinueDate = local.Max.Date;
        }
        else if (Equal(export.Discontinue.Date, local.Max.Date))
        {
          // new person
          export.ServiceProvider.DiscontinueDate = local.Max.Date;
          export.Discontinue.Date = local.Null1.Date;
        }

        if (AsChar(local.EndingServiceProvider.Flag) == 'Y')
        {
          // we have already checked that there are no assignments tied to these
          // service provider
          // so we can close all open office service providers that they are 
          // tied to and then end
          // date this service provider.
          foreach(var item in ReadServiceProviderOfficeOfficeServiceProvider1())
          {
            local.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
            local.OfficeServiceProvider.DiscontinueDate =
              export.Discontinue.Date;
            UseSpUpdateOfficeServicePrvdr1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }

        if (AsChar(local.ChangeOfficeSerProvider.Flag) == 'Y')
        {
          foreach(var item in ReadServiceProviderOfficeOfficeServiceProvider2())
          {
            local.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
            local.OfficeServiceProvider.WorkPhoneAreaCode =
              export.ServiceProvider.PhoneAreaCode.GetValueOrDefault();
            local.OfficeServiceProvider.WorkPhoneNumber =
              export.ServiceProvider.PhoneNumber.GetValueOrDefault();

            if (export.ServiceProvider.PhoneExtension.GetValueOrDefault() > 0)
            {
              local.OfficeServiceProvider.WorkPhoneExtension =
                NumberToString(export.ServiceProvider.PhoneExtension.
                  GetValueOrDefault(), 5);
            }
            else
            {
              local.OfficeServiceProvider.WorkPhoneExtension = "";
            }

            local.OfficeServiceProvider.WorkPhoneExtension =
              NumberToString(export.ServiceProvider.PhoneExtension.
                GetValueOrDefault(), 11, 5);
            UseSpUpdateOfficeServicePrvdr1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }

        if (Equal(global.Command, "UPDATE") && !
          IsEmpty(export.ServiceProvider.RoleCode) && !
          IsEmpty(export.HiddenServiceProvider.RoleCode) && !
          Equal(export.ServiceProvider.RoleCode,
          export.HiddenServiceProvider.RoleCode))
        {
          local.Read.Timestamp = Now();
          local.PreviouslyProcessedOfice.Flag = "";
          local.Processed.SystemGeneratedId = 0;

          // looking for old active role codes
          foreach(var item in ReadServiceProviderOfficeOfficeServiceProvider3())
          {
            if (entities.Office.SystemGeneratedId == local
              .Processed.SystemGeneratedId)
            {
              local.PreviouslyProcessedOfice.Flag = "Y";
            }
            else
            {
              local.PreviouslyProcessedOfice.Flag = "";
            }

            local.Processed.SystemGeneratedId =
              entities.Office.SystemGeneratedId;
            local.Old.Assign(entities.OfficeServiceProvider);
            local.SupervisorFound.Flag = "";

            if (ReadOfficeServiceProviderOfficeServiceProvRelationship())
            {
              // DELETE old supervisor record
              UseSpDeleteServicePrvdrRelation();

              if (!IsExitState("ACO_NI0000_DELETE_SUCCESSFUL"))
              {
                return;
              }

              local.SupervisorFound.Flag = "Y";
            }

            local.Old.DiscontinueDate = Now().Date.AddDays(-1);

            // make old role code inactive
            UseSpUpdateOfficeServicePrvdr2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (AsChar(local.PreviouslyProcessedOfice.Flag) != 'Y')
            {
              local.New1.Assign(entities.OfficeServiceProvider);
              local.New1.RoleCode = entities.ServiceProvider.RoleCode ?? Spaces
                (2);
              local.New1.EffectiveDate = Now().Date;
              local.New1.DiscontinueDate = local.Max.Date;

              // make new role code active
              UseSpCreateOfficeServicePrvdr();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (AsChar(local.SupervisorFound.Flag) == 'Y')
              {
                // make new relationship with supervisor
                UseSpCreateServicePrvdrRelation();

                if (!IsExitState("ACO_NI0000_ADD_SUCCESSFUL"))
                {
                  return;
                }
              }
            }
          }
        }

        UseSpUpdateServiceProvider();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          return;
        }

        // Note that once a Service Provider is established, the update command 
        // is used to add new occurrences of Service Provider address.
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (Length(TrimEnd(export.Group.Item.ServiceProviderAddress.Zip)) >
              0 && Length
              (TrimEnd(export.Group.Item.ServiceProviderAddress.Zip)) < 5)
            {
              var field =
                GetField(export.Group.Item.ServiceProviderAddress, "zip");

              field.Error = true;

              ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.ServiceProviderAddress.Zip)) >
              0 && Verify
              (export.Group.Item.ServiceProviderAddress.Zip, "0123456789") != 0
              )
            {
              var field =
                GetField(export.Group.Item.ServiceProviderAddress, "zip");

              field.Error = true;

              ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.ServiceProviderAddress.Zip)) ==
              0 && Length
              (TrimEnd(export.Group.Item.ServiceProviderAddress.Zip4)) > 0)
            {
              var field =
                GetField(export.Group.Item.ServiceProviderAddress, "zip4");

              field.Error = true;

              ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.ServiceProviderAddress.Zip)) >
              0 && Length
              (TrimEnd(export.Group.Item.ServiceProviderAddress.Zip4)) > 0)
            {
              if (Length(TrimEnd(export.Group.Item.ServiceProviderAddress.Zip4)) <
                4)
              {
                var field =
                  GetField(export.Group.Item.ServiceProviderAddress, "zip4");

                field.Error = true;

                ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

                return;
              }
              else if (Verify(export.Group.Item.ServiceProviderAddress.Zip4,
                "0123456789") != 0)
              {
                var field =
                  GetField(export.Group.Item.ServiceProviderAddress, "zip4");

                field.Error = true;

                ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

                return;
              }
            }

            UseSpUpdateServiceProvider();

            if (!IsEmpty(export.Group.Item.ServiceProviderAddress.Type1) && IsEmpty
              (export.Group.Item.Hidden.Type1))
            {
              // PREVIOUS VALUE IS BLANK, MUST BE AN ADD
              UseSpCreateServicePrvdrAddress();
            }
            else
            {
              // PREVIOUS VALUE IS NOT BLANK, MUST BE AN UPDATE
              UseSpUpdateServicePrvdrAddress();
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Group.Update.Common.SelectChar = "";
              export.Group.Update.Hidden.Type1 =
                export.Group.Item.ServiceProviderAddress.Type1;

              var field =
                GetField(export.Group.Item.ServiceProviderAddress, "type1");

              field.Protected = true;
            }
            else
            {
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

        export.HiddenServiceProvider.Assign(export.ServiceProvider);
        export.HiddenDiscontinue.Date = export.Discontinue.Date;
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveGroup(CabSpReadSrvcProvAndAddr.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.AddressTypeDesc.Description = source.AddressTypeDesc.Description;
    target.PromptAddressType.Flag = source.PromptAddressType.Flag;
    target.PromptState.Flag = source.PromptState.Flag;
    target.Common.SelectChar = source.Common.SelectChar;
    target.Hidden.Type1 = source.Hidden.Type1;
    target.ServiceProviderAddress.Assign(source.ServiceProviderAddress);
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

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Null1.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabSpReadSrvcProvAndAddr()
  {
    var useImport = new CabSpReadSrvcProvAndAddr.Import();
    var useExport = new CabSpReadSrvcProvAndAddr.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;

    Call(CabSpReadSrvcProvAndAddr.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
    export.ServiceProvider.Assign(useExport.ServiceProvider);
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
    local.ValidCode.Flag = useExport.ValidCode.Flag;
    export.Group.Update.AddressTypeDesc.Description =
      useExport.CodeValue.Description;
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

    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;

    Call(SpCabValidateForOspAssigns.Execute, useImport, useExport);
  }

  private void UseSpCabValidateSrvPrvdrDelete()
  {
    var useImport = new SpCabValidateSrvPrvdrDelete.Import();
    var useExport = new SpCabValidateSrvPrvdrDelete.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;

    Call(SpCabValidateSrvPrvdrDelete.Execute, useImport, useExport);
  }

  private void UseSpCreateOfficeServicePrvdr()
  {
    var useImport = new SpCreateOfficeServicePrvdr.Import();
    var useExport = new SpCreateOfficeServicePrvdr.Export();

    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;
    useImport.OfficeServiceProvider.Assign(local.New1);

    Call(SpCreateOfficeServicePrvdr.Execute, useImport, useExport);
  }

  private void UseSpCreateServiceProvider()
  {
    var useImport = new SpCreateServiceProvider.Import();
    var useExport = new SpCreateServiceProvider.Export();

    useImport.ServiceProvider.Assign(export.ServiceProvider);

    Call(SpCreateServiceProvider.Execute, useImport, useExport);

    export.ServiceProvider.SystemGeneratedId =
      useExport.ServiceProvider.SystemGeneratedId;
  }

  private void UseSpCreateServicePrvdrAddress()
  {
    var useImport = new SpCreateServicePrvdrAddress.Import();
    var useExport = new SpCreateServicePrvdrAddress.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;
    useImport.ServiceProviderAddress.Assign(
      export.Group.Item.ServiceProviderAddress);

    Call(SpCreateServicePrvdrAddress.Execute, useImport, useExport);
  }

  private void UseSpCreateServicePrvdrRelation()
  {
    var useImport = new SpCreateServicePrvdrRelation.Import();
    var useExport = new SpCreateServicePrvdrRelation.Export();

    useImport.LeadingServiceProvider.SystemGeneratedId =
      entities.LeaderServiceProvider.SystemGeneratedId;
    useImport.OfficeServiceProvRelationship.ReasonCode =
      entities.Old.ReasonCode;
    MoveOfficeServiceProvider(entities.LeaderOfficeServiceProvider,
      useImport.LeadingOfficeServiceProvider);
    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;

    Call(SpCreateServicePrvdrRelation.Execute, useImport, useExport);
  }

  private void UseSpDeleteServiceProvider()
  {
    var useImport = new SpDeleteServiceProvider.Import();
    var useExport = new SpDeleteServiceProvider.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;

    Call(SpDeleteServiceProvider.Execute, useImport, useExport);
  }

  private void UseSpDeleteServicePrvdrAddress()
  {
    var useImport = new SpDeleteServicePrvdrAddress.Import();
    var useExport = new SpDeleteServicePrvdrAddress.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;
    useImport.ServiceProviderAddress.Type1 =
      export.Group.Item.ServiceProviderAddress.Type1;

    Call(SpDeleteServicePrvdrAddress.Execute, useImport, useExport);
  }

  private void UseSpDeleteServicePrvdrRelation()
  {
    var useImport = new SpDeleteServicePrvdrRelation.Import();
    var useExport = new SpDeleteServicePrvdrRelation.Export();

    useImport.LeaderServiceProvider.SystemGeneratedId =
      entities.LeaderServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.LeaderOfficeServiceProvider,
      useImport.LeaderOfficeServiceProvider);
    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(local.Old, useImport.OfficeServiceProvider);

    Call(SpDeleteServicePrvdrRelation.Execute, useImport, useExport);
  }

  private void UseSpUpdateOfficeServicePrvdr1()
  {
    var useImport = new SpUpdateOfficeServicePrvdr.Import();
    var useExport = new SpUpdateOfficeServicePrvdr.Export();

    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.OfficeServiceProvider.Assign(local.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;

    Call(SpUpdateOfficeServicePrvdr.Execute, useImport, useExport);
  }

  private void UseSpUpdateOfficeServicePrvdr2()
  {
    var useImport = new SpUpdateOfficeServicePrvdr.Import();
    var useExport = new SpUpdateOfficeServicePrvdr.Export();

    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.OfficeServiceProvider.Assign(local.Old);
    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;

    Call(SpUpdateOfficeServicePrvdr.Execute, useImport, useExport);
  }

  private void UseSpUpdateServiceProvider()
  {
    var useImport = new SpUpdateServiceProvider.Import();
    var useExport = new SpUpdateServiceProvider.Export();

    useImport.ServiceProvider.Assign(export.ServiceProvider);

    Call(SpUpdateServiceProvider.Execute, useImport, useExport);
  }

  private void UseSpUpdateServicePrvdrAddress()
  {
    var useImport = new SpUpdateServicePrvdrAddress.Import();
    var useExport = new SpUpdateServicePrvdrAddress.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;
    useImport.ServiceProviderAddress.Assign(
      export.Group.Item.ServiceProviderAddress);

    Call(SpUpdateServicePrvdrAddress.Execute, useImport, useExport);
  }

  private bool ReadOfficeServiceProviderOfficeServiceProvRelationship()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Old.Populated = false;
    entities.LeaderOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOfficeServiceProvRelationship",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "discontinueDate", date);
        db.SetString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LeaderOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.Old.SpdRGeneratedId = db.GetInt32(reader, 0);
        entities.LeaderOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.Old.OffRGeneratedId = db.GetInt32(reader, 1);
        entities.LeaderOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.Old.OspRRoleCode = db.GetString(reader, 2);
        entities.LeaderOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.Old.OspREffectiveDt = db.GetDate(reader, 3);
        entities.LeaderOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Old.OspEffectiveDate = db.GetDate(reader, 5);
        entities.Old.OspRoleCode = db.GetString(reader, 6);
        entities.Old.OffGeneratedId = db.GetInt32(reader, 7);
        entities.Old.SpdGeneratedId = db.GetInt32(reader, 8);
        entities.Old.ReasonCode = db.GetString(reader, 9);
        entities.Old.Populated = true;
        entities.LeaderOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadProfileServiceProviderProfile()
  {
    entities.ServiceProviderProfile.Populated = false;
    entities.Profile.Populated = false;

    return Read("ReadProfileServiceProviderProfile",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.Profile.Name = db.GetString(reader, 0);
        entities.ServiceProviderProfile.ProName = db.GetString(reader, 0);
        entities.ServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ServiceProviderProfile.EffectiveDate = db.GetDate(reader, 2);
        entities.ServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ServiceProviderProfile.SpdGenId = db.GetInt32(reader, 4);
        entities.ServiceProviderProfile.Populated = true;
        entities.Profile.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "lastName", import.ServiceProvider.LastName);
        db.SetString(command, "firstName", import.ServiceProvider.FirstName);
        db.SetString(
          command, "middleInitial", import.ServiceProvider.MiddleInitial);
        db.SetInt32(
          command, "servicePrvderId", export.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.UserId = db.GetString(reader, 3);
        entities.ServiceProvider.LastName = db.GetString(reader, 4);
        entities.ServiceProvider.FirstName = db.GetString(reader, 5);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 6);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 7);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 8);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.PhoneNumber = db.GetNullableInt32(reader, 11);
        entities.ServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 12);
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
          command, "servicePrvderId", export.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.UserId = db.GetString(reader, 3);
        entities.ServiceProvider.LastName = db.GetString(reader, 4);
        entities.ServiceProvider.FirstName = db.GetString(reader, 5);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 6);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 7);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 8);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.PhoneNumber = db.GetNullableInt32(reader, 11);
        entities.ServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 12);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider3()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider3",
      (db, command) =>
      {
        db.SetString(command, "userId", import.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.UserId = db.GetString(reader, 3);
        entities.ServiceProvider.LastName = db.GetString(reader, 4);
        entities.ServiceProvider.FirstName = db.GetString(reader, 5);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 6);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 7);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 8);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.PhoneNumber = db.GetNullableInt32(reader, 11);
        entities.ServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 12);
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderOfficeOfficeServiceProvider1()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderOfficeOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", import.ServiceProvider.SystemGeneratedId);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.UserId = db.GetString(reader, 3);
        entities.ServiceProvider.LastName = db.GetString(reader, 4);
        entities.ServiceProvider.FirstName = db.GetString(reader, 5);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 6);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 7);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 8);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.PhoneNumber = db.GetNullableInt32(reader, 11);
        entities.ServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 12);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 13);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 13);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 14);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 15);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 16);
        entities.OfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 17);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 18);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 19);
        entities.OfficeServiceProvider.CreatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.OfficeServiceProvider.ZdelCertificationNumber =
          db.GetNullableString(reader, 21);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 22);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 23);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 24);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 25);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderOfficeOfficeServiceProvider2()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderOfficeOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", import.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.UserId = db.GetString(reader, 3);
        entities.ServiceProvider.LastName = db.GetString(reader, 4);
        entities.ServiceProvider.FirstName = db.GetString(reader, 5);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 6);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 7);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 8);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.PhoneNumber = db.GetNullableInt32(reader, 11);
        entities.ServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 12);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 13);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 13);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 14);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 15);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 16);
        entities.OfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 17);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 18);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 19);
        entities.OfficeServiceProvider.CreatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.OfficeServiceProvider.ZdelCertificationNumber =
          db.GetNullableString(reader, 21);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 22);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 23);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 24);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 25);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderOfficeOfficeServiceProvider3()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderOfficeOfficeServiceProvider3",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", import.ServiceProvider.SystemGeneratedId);
          
        db.SetDateTime(
          command, "createdTimestamp",
          local.Read.Timestamp.GetValueOrDefault());
        db.
          SetString(command, "roleCode", export.ServiceProvider.RoleCode ?? "");
          
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.UserId = db.GetString(reader, 3);
        entities.ServiceProvider.LastName = db.GetString(reader, 4);
        entities.ServiceProvider.FirstName = db.GetString(reader, 5);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 6);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 7);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 8);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.PhoneNumber = db.GetNullableInt32(reader, 11);
        entities.ServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 12);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 13);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 13);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 14);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 15);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 16);
        entities.OfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 17);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 18);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 19);
        entities.OfficeServiceProvider.CreatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.OfficeServiceProvider.ZdelCertificationNumber =
          db.GetNullableString(reader, 21);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 22);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 23);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 24);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 25);
        entities.Office.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of AddressTypeDesc.
      /// </summary>
      [JsonPropertyName("addressTypeDesc")]
      public CodeValue AddressTypeDesc
      {
        get => addressTypeDesc ??= new();
        set => addressTypeDesc = value;
      }

      /// <summary>
      /// A value of PromptAddressType.
      /// </summary>
      [JsonPropertyName("promptAddressType")]
      public Common PromptAddressType
      {
        get => promptAddressType ??= new();
        set => promptAddressType = value;
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
      /// A value of ServiceProviderAddress.
      /// </summary>
      [JsonPropertyName("serviceProviderAddress")]
      public ServiceProviderAddress ServiceProviderAddress
      {
        get => serviceProviderAddress ??= new();
        set => serviceProviderAddress = value;
      }

      /// <summary>
      /// A value of PromptState.
      /// </summary>
      [JsonPropertyName("promptState")]
      public Common PromptState
      {
        get => promptState ??= new();
        set => promptState = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ServiceProviderAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 16;

      private CodeValue addressTypeDesc;
      private Common promptAddressType;
      private Common common;
      private ServiceProviderAddress serviceProviderAddress;
      private Common promptState;
      private ServiceProviderAddress hidden;
    }

    /// <summary>
    /// A value of HiddenDiscontinue.
    /// </summary>
    [JsonPropertyName("hiddenDiscontinue")]
    public DateWorkArea HiddenDiscontinue
    {
      get => hiddenDiscontinue ??= new();
      set => hiddenDiscontinue = value;
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
    /// A value of PromptRoleCode.
    /// </summary>
    [JsonPropertyName("promptRoleCode")]
    public Common PromptRoleCode
    {
      get => promptRoleCode ??= new();
      set => promptRoleCode = value;
    }

    /// <summary>
    /// A value of HiddenFromCodValList.
    /// </summary>
    [JsonPropertyName("hiddenFromCodValList")]
    public CodeValue HiddenFromCodValList
    {
      get => hiddenFromCodValList ??= new();
      set => hiddenFromCodValList = value;
    }

    /// <summary>
    /// A value of HiddenFromList.
    /// </summary>
    [JsonPropertyName("hiddenFromList")]
    public ServiceProvider HiddenFromList
    {
      get => hiddenFromList ??= new();
      set => hiddenFromList = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of PromptServiceProvider.
    /// </summary>
    [JsonPropertyName("promptServiceProvider")]
    public Common PromptServiceProvider
    {
      get => promptServiceProvider ??= new();
      set => promptServiceProvider = value;
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
    /// A value of HiddenServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("hiddenServiceProviderAddress")]
    public ServiceProviderAddress HiddenServiceProviderAddress
    {
      get => hiddenServiceProviderAddress ??= new();
      set => hiddenServiceProviderAddress = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private DateWorkArea hiddenDiscontinue;
    private DateWorkArea discontinue;
    private Common promptRoleCode;
    private CodeValue hiddenFromCodValList;
    private ServiceProvider hiddenFromList;
    private Array<GroupGroup> group;
    private Common promptServiceProvider;
    private ServiceProvider hiddenServiceProvider;
    private ServiceProviderAddress hiddenServiceProviderAddress;
    private ServiceProvider serviceProvider;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of AddressTypeDesc.
      /// </summary>
      [JsonPropertyName("addressTypeDesc")]
      public CodeValue AddressTypeDesc
      {
        get => addressTypeDesc ??= new();
        set => addressTypeDesc = value;
      }

      /// <summary>
      /// A value of PromptAddressType.
      /// </summary>
      [JsonPropertyName("promptAddressType")]
      public Common PromptAddressType
      {
        get => promptAddressType ??= new();
        set => promptAddressType = value;
      }

      /// <summary>
      /// A value of PromptState.
      /// </summary>
      [JsonPropertyName("promptState")]
      public Common PromptState
      {
        get => promptState ??= new();
        set => promptState = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ServiceProviderAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of ServiceProviderAddress.
      /// </summary>
      [JsonPropertyName("serviceProviderAddress")]
      public ServiceProviderAddress ServiceProviderAddress
      {
        get => serviceProviderAddress ??= new();
        set => serviceProviderAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 16;

      private CodeValue addressTypeDesc;
      private Common promptAddressType;
      private Common promptState;
      private Common common;
      private ServiceProviderAddress hidden;
      private ServiceProviderAddress serviceProviderAddress;
    }

    /// <summary>
    /// A value of HiddenDiscontinue.
    /// </summary>
    [JsonPropertyName("hiddenDiscontinue")]
    public DateWorkArea HiddenDiscontinue
    {
      get => hiddenDiscontinue ??= new();
      set => hiddenDiscontinue = value;
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
    /// A value of PromptRoleCode.
    /// </summary>
    [JsonPropertyName("promptRoleCode")]
    public Common PromptRoleCode
    {
      get => promptRoleCode ??= new();
      set => promptRoleCode = value;
    }

    /// <summary>
    /// A value of HiddenToCodeValueList.
    /// </summary>
    [JsonPropertyName("hiddenToCodeValueList")]
    public Code HiddenToCodeValueList
    {
      get => hiddenToCodeValueList ??= new();
      set => hiddenToCodeValueList = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of PromptServiceProvider.
    /// </summary>
    [JsonPropertyName("promptServiceProvider")]
    public Common PromptServiceProvider
    {
      get => promptServiceProvider ??= new();
      set => promptServiceProvider = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private DateWorkArea hiddenDiscontinue;
    private DateWorkArea discontinue;
    private Common promptRoleCode;
    private Code hiddenToCodeValueList;
    private Array<GroupGroup> group;
    private Common promptServiceProvider;
    private ServiceProvider hiddenServiceProvider;
    private ServiceProvider serviceProvider;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of UserIdPrefix.
    /// </summary>
    [JsonPropertyName("userIdPrefix")]
    public WorkArea UserIdPrefix
    {
      get => userIdPrefix ??= new();
      set => userIdPrefix = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public DateWorkArea Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of PreviouslyProcessedOfice.
    /// </summary>
    [JsonPropertyName("previouslyProcessedOfice")]
    public Common PreviouslyProcessedOfice
    {
      get => previouslyProcessedOfice ??= new();
      set => previouslyProcessedOfice = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public Office Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of SupervisorFound.
    /// </summary>
    [JsonPropertyName("supervisorFound")]
    public Common SupervisorFound
    {
      get => supervisorFound ??= new();
      set => supervisorFound = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public OfficeServiceProvider Old
    {
      get => old ??= new();
      set => old = value;
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

    /// <summary>
    /// A value of ChangeOfficeSerProvider.
    /// </summary>
    [JsonPropertyName("changeOfficeSerProvider")]
    public Common ChangeOfficeSerProvider
    {
      get => changeOfficeSerProvider ??= new();
      set => changeOfficeSerProvider = value;
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
    /// A value of EndingServiceProvider.
    /// </summary>
    [JsonPropertyName("endingServiceProvider")]
    public Common EndingServiceProvider
    {
      get => endingServiceProvider ??= new();
      set => endingServiceProvider = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of DeletedAll.
    /// </summary>
    [JsonPropertyName("deletedAll")]
    public Common DeletedAll
    {
      get => deletedAll ??= new();
      set => deletedAll = value;
    }

    /// <summary>
    /// A value of ClearServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("clearServiceProviderAddress")]
    public ServiceProviderAddress ClearServiceProviderAddress
    {
      get => clearServiceProviderAddress ??= new();
      set => clearServiceProviderAddress = value;
    }

    /// <summary>
    /// A value of ClearServiceProvider.
    /// </summary>
    [JsonPropertyName("clearServiceProvider")]
    public ServiceProvider ClearServiceProvider
    {
      get => clearServiceProvider ??= new();
      set => clearServiceProvider = value;
    }

    /// <summary>
    /// A value of TotalCount.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public Common TotalCount
    {
      get => totalCount ??= new();
      set => totalCount = value;
    }

    /// <summary>
    /// A value of SelectedCount.
    /// </summary>
    [JsonPropertyName("selectedCount")]
    public Common SelectedCount
    {
      get => selectedCount ??= new();
      set => selectedCount = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private WorkArea userIdPrefix;
    private DateWorkArea read;
    private Common previouslyProcessedOfice;
    private Office processed;
    private Common supervisorFound;
    private OfficeServiceProvider old;
    private OfficeServiceProvider new1;
    private Common changeOfficeSerProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Common endingServiceProvider;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Common deletedAll;
    private ServiceProviderAddress clearServiceProviderAddress;
    private ServiceProvider clearServiceProvider;
    private Common totalCount;
    private Common selectedCount;
    private Common count;
    private Common returnCode;
    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderServiceProvider")]
    public ServiceProvider LeaderServiceProvider
    {
      get => leaderServiceProvider ??= new();
      set => leaderServiceProvider = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public OfficeServiceProvRelationship Old
    {
      get => old ??= new();
      set => old = value;
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
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    /// <summary>
    /// A value of ProfileAuthorization.
    /// </summary>
    [JsonPropertyName("profileAuthorization")]
    public ProfileAuthorization ProfileAuthorization
    {
      get => profileAuthorization ??= new();
      set => profileAuthorization = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private ServiceProvider leaderServiceProvider;
    private OfficeServiceProvRelationship old;
    private OfficeServiceProvider leaderOfficeServiceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private ServiceProviderProfile serviceProviderProfile;
    private ProfileAuthorization profileAuthorization;
    private Profile profile;
  }
#endregion
}
