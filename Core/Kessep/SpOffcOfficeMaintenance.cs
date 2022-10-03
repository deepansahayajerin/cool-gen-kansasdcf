// Program: SP_OFFC_OFFICE_MAINTENANCE, ID: 371782915, model: 746.
// Short name: SWEOFFCP
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
/// <para>
/// A program: SP_OFFC_OFFICE_MAINTENANCE.
/// </para>
/// <para>
/// RESP:
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpOffcOfficeMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_OFFC_OFFICE_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpOffcOfficeMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpOffcOfficeMaintenance.
  /// </summary>
  public SpOffcOfficeMaintenance(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // ** DATE      *  DESCRIPTION
    // ** 05/15/95     a. hackler
    // *********************************************
    // **  09/25/95    j. kemp  removed confirm delete logic to conform to 
    // standards
    //     02/06/96     a. hackler  retro fits
    //     04/08/96	J. Rookard added validation
    // routine for Display processing, must have
    // import of Office Sys Gen Id.  Added local max
    // and null dates for date reference.  Corrected
    // Office effective and discontinue date bugs during add, update, and 
    // display processing.
    // Corrected dialog flow matching between this procedure and OFCL-Office 
    // List so that OFCL returns selected office sys gen id to Import Hidden
    // From List view.  Added RTLIST command to handle processing on return from
    // list.
    // **  04/19/96 J.Rookard  Added dialog flow to FIPL to allow selection and 
    // carry back of FIPS code information.
    // **  04/19/96 J.Rookard  Modified delete validation code to allow for 
    // deletion of office addresses without deleting office.
    // **  04/19/96 J.Rookard  Modified SP_UPDATE_OFFICE to 1) handle 
    // association of office from one county to another (removed disassociate
    // statement),and 2) handle associate from no FIPS code to a FIPS code, and
    // from FIPS code to no FIPS code, from one FIPS code to another FIPS code.
    // **  01/03-97 R. Marchman  Add new security/next tran.	
    //                                             
    // **   11/7/98  Anita Massey  fixes per screen
    // assessment
    //                            document.
    // ********************************************
    // *************************************************************
    //     04/25/01     Madhu Kumar    Added edit check for 4
    //                                 
    // and 5 digit  zip code
    // *************************************************************
    // ******************************************************************************************************
    // 12/02/2004     M J Quinn   WR040802    Expanded the Garden City
    // Customer Service Center pilot.                   Modules SWE02231,
    // SWEOFFCS, SWEOFFCP, SWE01441, SWE01311, SWE00091 are included in this
    // work request.
    // ******************************************************************************************************
    // ****************************************************************************************************
    // 03/30/10    L. Smith   PR#361227 CQ#10317
    // Added edit to not allow an office to be end dated if active service 
    // provider exists for that office.
    // ****************************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    // Set local null, current & max dates for reference purposes.
    local.Current.Date = Now().Date;
    local.Null1.Date = null;
    local.Max.Date = UseCabSetMaximumDiscontinueDate2();
    export.Office2.Assign(import.Office2);
    export.Office1.SelectChar = import.Office1.SelectChar;
    export.PromptOffice.Flag = import.PromptOffice.Flag;
    export.PromptCustomerServiceC.Flag = import.PromptCustomerServiceC.Flag;
    MoveOffice(import.CustomerServiceCenter, export.CustomerServiceCenter);
    export.HiddenPrev.EffectiveDate = import.HiddenPrev.EffectiveDate;
    export.PromptOfficeType.Flag = import.PromptOfficeType.Flag;
    export.OfficeTypeDesc.Description = import.OfficeTypeDesc.Description;
    MoveCseOrganization(import.CseOrganization, export.CseOrganization);
    export.Fips.Assign(import.Fips);
    MoveCseOrganization(import.HiddenCseOrganization,
      export.HiddenCseOrganization);
    export.HiddenFips.Assign(import.HiddenFips);
    export.ListCounty.Flag = import.ListCounty.Flag;
    export.ListFips.Flag = import.ListFips.Flag;
    MoveOffice(import.HiddenOffice, export.HiddenOffice);

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
      export.Group.Update.OfficeAddress.Assign(import.Group.Item.OfficeAddress);
      export.Group.Update.Hidden.Type1 = import.Group.Item.Hidden.Type1;
      export.Group.Update.PromptState.Flag = import.Group.Item.PromptState.Flag;
      export.Group.Update.PromptAddressType.Flag =
        import.Group.Item.PromptAddressType.Flag;
      export.Group.Update.AddTypeDesc.Description =
        import.Group.Item.AddTypeDesc.Description;
      ++local.TotalCount.Count;

      if (AsChar(import.Group.Item.Common.SelectChar) == 'S')
      {
        ++local.SelectedCount.Count;
      }

      export.Group.Next();
    }

    if (Equal(global.Command, "ENTER"))
    {
      export.Standard.Assign(import.Standard);

      // if the next tran info is not equal to spaces, this implies the user 
      // requested a next tran action. now validate
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        UseScCabNextTranPut();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
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

    if (Equal(global.Command, "RTLIST"))
    {
      if (AsChar(export.PromptOffice.Flag) == 'S')
      {
        export.PromptOffice.Flag = "+";

        var field = GetField(export.PromptOffice, "flag");

        field.Protected = false;
        field.Focused = true;

        if (import.HiddenFromListOffice.SystemGeneratedId > 0)
        {
          MoveOffice(import.HiddenFromListOffice, export.Office2);
          MoveOffice(import.HiddenFromListOffice, export.HiddenOffice);
          global.Command = "DISPLAY";
        }
      }

      if (AsChar(export.PromptCustomerServiceC.Flag) == 'S')
      {
        export.PromptCustomerServiceC.Flag = "+";

        var field = GetField(export.PromptCustomerServiceC, "flag");

        field.Protected = false;
        field.Focused = true;

        if (import.HiddenFromListOffice.SystemGeneratedId > 0)
        {
          MoveOffice(import.HiddenFromListOffice, export.CustomerServiceCenter);
          global.Command = "DISPLAY";
        }
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!IsEmpty(export.Group.Item.Hidden.Type1))
        {
          var field = GetField(export.Group.Item.OfficeAddress, "type1");

          field.Color = "cyan";
          field.Protected = true;
        }
      }
    }

    if (Equal(global.Command, "RLCVAL") || Equal(global.Command, "RTLIST"))
    {
    }
    else
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

    // ensure the key has not changed before a delete or update
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      if (import.Office2.SystemGeneratedId == import
        .HiddenOffice.SystemGeneratedId)
      {
      }
      else
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

        return;
      }
    }

    // validate the data
    if (Equal(global.Command, "DISPLAY"))
    {
      if (export.Office2.SystemGeneratedId == 0)
      {
        var field = GetField(export.Office2, "systemGeneratedId");

        field.Error = true;

        ExitState = "SP0000_REQUIRED_FIELD_MISSING";

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(import.Office2.Name))
      {
        var field = GetField(export.Office2, "name");

        field.Error = true;

        ExitState = "SP0000_REQUIRED_FIELD_MISSING";
      }

      if (IsEmpty(import.Office2.TypeCode))
      {
        var field = GetField(export.Office2, "typeCode");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "SP0000_REQUIRED_FIELD_MISSING";
        }
      }

      if (import.Office2.MainPhoneAreaCode.GetValueOrDefault() == 0)
      {
        var field = GetField(export.Office2, "mainPhoneAreaCode");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "SP0000_REQUIRED_FIELD_MISSING";
        }
      }

      if (export.Office2.MainPhoneNumber.GetValueOrDefault() == 0)
      {
        var field = GetField(export.Office2, "mainPhoneNumber");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "SP0000_REQUIRED_FIELD_MISSING";
        }
      }

      if (import.Office2.MainFaxAreaCode.GetValueOrDefault() == 0 && import
        .Office2.MainFaxPhoneNumber.GetValueOrDefault() == 0)
      {
        // the fax number is optional.  If number or area code is entered both 
        // must be present
      }
      else if (import.Office2.MainFaxAreaCode.GetValueOrDefault() == 0 || import
        .Office2.MainFaxPhoneNumber.GetValueOrDefault() == 0)
      {
        var field1 = GetField(export.Office2, "mainFaxAreaCode");

        field1.Error = true;

        var field2 = GetField(export.Office2, "mainFaxPhoneNumber");

        field2.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "SP0000_REQUIRED_FIELD_MISSING";
        }
      }

      if (IsEmpty(export.CseOrganization.Code))
      {
        var field = GetField(export.CseOrganization, "code");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "SP0000_REQUIRED_FIELD_MISSING";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (Equal(global.Command, "ADD"))
      {
        if (ReadOffice1())
        {
          var field = GetField(export.Office2, "name");

          field.Error = true;

          ExitState = "OFFICE_AE";

          return;
        }
      }

      local.Code.CodeName = "OFFICE TYPE";
      local.CodeValue.Cdvalue = export.Office2.TypeCode;
      UseCabValidateCodeValue2();

      if (AsChar(local.ValidCode.Flag) == 'Y')
      {
      }
      else
      {
        var field = GetField(export.Office2, "typeCode");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (Equal(import.Office2.EffectiveDate, local.Null1.Date))
      {
        export.Office2.EffectiveDate = local.Current.Date;
      }

      if (Equal(import.Office2.DiscontinueDate, local.Null1.Date))
      {
        export.Office2.DiscontinueDate = local.Max.Date;
      }
      else
      {
        export.Office2.DiscontinueDate = import.Office2.DiscontinueDate;
      }

      if (Lt(export.Office2.DiscontinueDate, export.Office2.EffectiveDate))
      {
        var field = GetField(export.Office2, "discontinueDate");

        field.Error = true;

        ExitState = "DISC_DATE_CANNOT_LT_EFF_DATE";
      }

      if (Lt(export.Office2.DiscontinueDate, local.Current.Date))
      {
        var field = GetField(export.Office2, "discontinueDate");

        field.Error = true;

        ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // PR#361227 CQ#10317  edit for office end date field - check for active 
      // service provider
      if (Equal(global.Command, "UPDATE") && !
        Equal(export.Office2.DiscontinueDate, local.Max.Date))
      {
        if (ReadOffice2())
        {
          if (ReadOfficeServiceProvider())
          {
            var field = GetField(export.Office2, "discontinueDate");

            field.Error = true;

            ExitState = "CANNOT_END_DT_REL_TO_OFC_SRV_PRV";

            return;
          }
        }
      }

      if (export.Group.IsEmpty)
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          var field1 = GetField(export.Group.Item.OfficeAddress, "street1");

          field1.Error = true;

          var field2 = GetField(export.Group.Item.OfficeAddress, "type1");

          field2.Error = true;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "MUST_HAVE_AT_LEAST_ONE_ADDRESS";

          return;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (export.Fips.State != 0 && export.Fips.County != 0)
      {
        if (!ReadFips())
        {
          var field1 = GetField(export.Fips, "state");

          field1.Error = true;

          var field2 = GetField(export.Fips, "county");

          field2.Error = true;

          var field3 = GetField(export.Fips, "location");

          field3.Error = true;

          ExitState = "FIPS_NF";

          return;
        }
      }

      if (!ReadCseOrganization())
      {
        var field = GetField(export.CseOrganization, "code");

        field.Error = true;

        ExitState = "COUNTY_NF";

        return;
      }

      if (export.CustomerServiceCenter.SystemGeneratedId > 0)
      {
        if (!ReadOffice3())
        {
          var field =
            GetField(export.CustomerServiceCenter, "systemGeneratedId");

          field.Error = true;

          ExitState = "FN0000_CUSTOMER_SERVICE_CNTR_NF";

          return;
        }

        if (export.CustomerServiceCenter.SystemGeneratedId == export
          .Office2.SystemGeneratedId)
        {
          var field =
            GetField(export.CustomerServiceCenter, "systemGeneratedId");

          field.Error = true;

          ExitState = "FN0000_OFFICE_CAN_NOT_BE_OWN_CSC";

          return;
        }
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        local.Code.CodeName = "ADDRESS TYPE";
        local.CodeValue.Cdvalue = export.Group.Item.OfficeAddress.Type1;
        UseCabValidateCodeValue3();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
        }
        else
        {
          var field = GetField(export.Group.Item.OfficeAddress, "type1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_TYPE_CODE";
          }
        }

        if (IsEmpty(export.Group.Item.OfficeAddress.Street1))
        {
          var field = GetField(export.Group.Item.OfficeAddress, "street1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SP0000_REQUIRED_FIELD_MISSING";
          }
        }

        if (IsEmpty(export.Group.Item.OfficeAddress.City))
        {
          var field = GetField(export.Group.Item.OfficeAddress, "city");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SP0000_REQUIRED_FIELD_MISSING";
          }
        }

        if (IsEmpty(export.Group.Item.OfficeAddress.StateProvince))
        {
          export.Group.Update.OfficeAddress.StateProvince = "KS";
        }

        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue = export.Group.Item.OfficeAddress.StateProvince;
        UseCabValidateCodeValue1();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
        }
        else
        {
          var field =
            GetField(export.Group.Item.OfficeAddress, "stateProvince");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        if (!Equal(export.Group.Item.OfficeAddress.Country, "USA"))
        {
          export.Group.Update.OfficeAddress.Country = "USA";
        }

        if (IsEmpty(export.Group.Item.OfficeAddress.Zip))
        {
          var field = GetField(export.Group.Item.OfficeAddress, "zip");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // Add logic to differentiate between delete of a selected office address 
    // and office. J.Rookard 04/19/96
    if (Equal(global.Command, "DELETE"))
    {
      if (local.SelectedCount.Count == 0)
      {
        // user is trying to delete the office and if successful all office 
        // addresses will be deleted in a cascade delete.
        UseSpCabValidateOfficeDelete();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test1;
        }
      }

      if (local.SelectedCount.Count > 0)
      {
        // user is trying to delete one or more occurrences of office address.
        // there has to be at least 1 address for an office if it is currently 
        // being used.
        local.Remaining.Count = local.TotalCount.Count - local
          .SelectedCount.Count;

        if (local.Remaining.Count == 0)
        {
          ExitState = "CANNOT_DELETE_THE_LAST_ADDRESS";

          return;
        }
      }
    }

Test1:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "RTLIST":
        if (Equal(export.Office2.DiscontinueDate, local.Max.Date))
        {
          export.Office2.DiscontinueDate = UseCabSetMaximumDiscontinueDate1();
        }

        if (IsEmpty(export.PromptOffice.Flag))
        {
        }
        else
        {
          export.PromptOffice.Flag = "";

          var field = GetField(export.PromptOffice, "flag");

          field.Protected = false;
          field.Focused = true;

          if (import.HiddenFromListOffice.SystemGeneratedId != 0)
          {
            export.Office2.SystemGeneratedId =
              import.HiddenFromListOffice.SystemGeneratedId;
          }

          return;
        }

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "RLCVAL":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Hidden.Type1))
          {
            var field = GetField(export.Group.Item.OfficeAddress, "type1");

            field.Color = "cyan";
            field.Protected = true;
          }
        }

        if (IsEmpty(export.PromptOffice.Flag))
        {
        }
        else
        {
          export.PromptOffice.Flag = "";

          var field = GetField(export.PromptOffice, "flag");

          field.Protected = false;
          field.Focused = true;

          if (import.HiddenFromListOffice.SystemGeneratedId != 0)
          {
            export.Office2.SystemGeneratedId =
              import.HiddenFromListOffice.SystemGeneratedId;
          }

          return;
        }

        if (!IsEmpty(export.ListFips.Flag))
        {
          if (import.HiddenSelectionFips.State > 0)
          {
            export.Fips.Assign(import.HiddenSelectionFips);
          }

          var field = GetField(export.ListFips, "flag");

          field.Protected = false;
          field.Focused = true;

          export.ListFips.Flag = "";

          return;
        }

        if (!IsEmpty(export.ListCounty.Flag))
        {
          if (!IsEmpty(import.HiddenSelectionCseOrganization.Code))
          {
            MoveCseOrganization(import.HiddenSelectionCseOrganization,
              export.CseOrganization);
          }
          else
          {
            MoveCseOrganization(export.HiddenCseOrganization,
              export.CseOrganization);
          }

          var field = GetField(export.ListCounty, "flag");

          field.Protected = false;
          field.Focused = true;

          export.ListCounty.Flag = "";

          return;
        }

        if (IsEmpty(export.PromptOfficeType.Flag))
        {
        }
        else
        {
          export.PromptOfficeType.Flag = "";

          var field = GetField(export.PromptOfficeType, "flag");

          field.Protected = false;
          field.Focused = true;

          if (!IsEmpty(import.HiddenFromListCodeValue.Cdvalue))
          {
            export.Office2.TypeCode = import.HiddenFromListCodeValue.Cdvalue;
            export.OfficeTypeDesc.Description =
              import.HiddenFromListCodeValue.Description;
          }

          return;
        }

        if (Equal(export.Office2.DiscontinueDate, local.Max.Date))
        {
          export.Office2.DiscontinueDate = UseCabSetMaximumDiscontinueDate1();
        }

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

            if (!IsEmpty(import.HiddenFromListCodeValue.Cdvalue) && IsEmpty
              (export.Group.Item.OfficeAddress.Type1))
            {
              export.Group.Update.OfficeAddress.Type1 =
                import.HiddenFromListCodeValue.Cdvalue;
              export.Group.Update.AddTypeDesc.Description =
                import.HiddenFromListCodeValue.Description;
            }
            else
            {
              export.Group.Update.OfficeAddress.Type1 =
                export.Group.Item.Hidden.Type1;

              if (!IsEmpty(export.Group.Item.OfficeAddress.Type1))
              {
                var field1 = GetField(export.Group.Item.OfficeAddress, "type1");

                field1.Color = "cyan";
                field1.Protected = true;
              }
            }

            goto Test3;
          }
        }

        break;
      case "DISPLAY":
        UseSpCabReadOfcAndOfcAddr1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveOffice(export.Office2, export.HiddenOffice);
          MoveCseOrganization(export.CseOrganization,
            export.HiddenCseOrganization);
          export.HiddenFips.Assign(export.Fips);
          export.PromptOffice.Flag = "";
          export.Office1.SelectChar = "";
          export.PromptOfficeType.Flag = "";
        }
        else
        {
          export.Office2.Assign(local.ClearOffice);
          MoveOffice(local.ClearOffice, export.HiddenOffice);
          export.OfficeTypeDesc.Description =
            Spaces(CodeValue.Description_MaxLength);
          export.Office2.SystemGeneratedId = import.Office2.SystemGeneratedId;

          export.Group.Index = 0;
          export.Group.Clear();

          for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
            import.Group.Index)
          {
            if (export.Group.IsFull)
            {
              break;
            }

            export.Group.Next();

            break;

            export.Group.Next();
          }

          var field = GetField(export.Office2, "systemGeneratedId");

          field.Error = true;

          return;
        }

        local.Code.CodeName = "OFFICE TYPE";
        local.CodeValue.Cdvalue = export.Office2.TypeCode;
        UseCabValidateCodeValue2();

        if (Equal(export.Office2.DiscontinueDate, local.Max.Date))
        {
          export.Office2.DiscontinueDate = UseCabSetMaximumDiscontinueDate1();
        }

        local.Code.CodeName = "ADDRESS TYPE";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          export.Group.Update.Hidden.Type1 =
            export.Group.Item.OfficeAddress.Type1;
          local.CodeValue.Cdvalue = export.Group.Item.OfficeAddress.Type1;

          var field = GetField(export.Group.Item.OfficeAddress, "type1");

          field.Color = "cyan";
          field.Protected = true;

          UseCabValidateCodeValue3();
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "NO_ADDRESS_FOR_OFFICE";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "ADD":
        if (Lt(import.Office2.EffectiveDate, local.Current.Date))
        {
          export.Office2.EffectiveDate = local.Current.Date;
        }

        UseSpCreateOffice();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCseOrganization(export.CseOrganization,
            export.HiddenCseOrganization);
          export.HiddenFips.Assign(export.Fips);
        }
        else
        {
          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (Length(TrimEnd(export.Group.Item.OfficeAddress.Zip)) > 0 && Length
              (TrimEnd(export.Group.Item.OfficeAddress.Zip)) < 5)
            {
              var field = GetField(export.Group.Item.OfficeAddress, "zip");

              field.Error = true;

              ExitState = "PLEASE_ENTER_FIVE_DIGIT_ZIP_RB";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.OfficeAddress.Zip)) > 0 && Verify
              (export.Group.Item.OfficeAddress.Zip, "0123456789") != 0)
            {
              var field = GetField(export.Group.Item.OfficeAddress, "zip");

              field.Error = true;

              ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.OfficeAddress.Zip)) == 0 && Length
              (TrimEnd(export.Group.Item.OfficeAddress.Zip4)) > 0)
            {
              var field = GetField(export.Group.Item.OfficeAddress, "zip");

              field.Error = true;

              ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.OfficeAddress.Zip)) > 0 && Length
              (TrimEnd(export.Group.Item.OfficeAddress.Zip4)) > 0)
            {
              if (Length(TrimEnd(export.Group.Item.OfficeAddress.Zip4)) < 4)
              {
                var field = GetField(export.Group.Item.OfficeAddress, "zip4");

                field.Error = true;

                ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

                return;
              }
              else if (Verify(export.Group.Item.OfficeAddress.Zip4, "0123456789")
                != 0)
              {
                var field = GetField(export.Group.Item.OfficeAddress, "zip4");

                field.Error = true;

                ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

                return;
              }
            }

            UseSpCreateOfficeAddress();
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Group.Update.Common.SelectChar = "";

            var field = GetField(export.Group.Item.OfficeAddress, "type1");

            field.Color = "cyan";
            field.Protected = true;
          }
          else
          {
            ExitState = "DUPLICATE_ADDRESS_NOT_ALLOWED";

            var field = GetField(export.Group.Item.OfficeAddress, "type1");

            field.Error = true;

            break;
          }
        }

        if (Equal(export.Office2.DiscontinueDate, local.Max.Date))
        {
          export.Office2.DiscontinueDate = UseCabSetMaximumDiscontinueDate1();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        MoveOffice(export.Office2, export.HiddenOffice);

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          export.Group.Update.Hidden.Type1 =
            export.Group.Item.OfficeAddress.Type1;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "DELETE":
        if (local.SelectedCount.Count == 0)
        {
          UseSpDeleteOffice();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
          }
        }
        else
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseSpDeleteOfficeAddress();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.OfficeAddress.Assign(
                  local.ClearOfficeAddress);
                export.Group.Update.Hidden.Type1 =
                  local.ClearOfficeAddress.Type1;
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field = GetField(export.Group.Item.OfficeAddress, "type1");

                field.Error = true;
              }
            }
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            goto Test2;
          }

          UseSpCabReadOfcAndOfcAddr2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            export.Office2.SystemGeneratedId = import.Office2.SystemGeneratedId;

            var field = GetField(export.Office2, "name");

            field.Error = true;
          }
        }

Test2:

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          var field = GetField(export.Group.Item.OfficeAddress, "type1");

          field.Color = "cyan";
          field.Protected = true;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        MoveOffice(local.ClearOffice, export.HiddenOffice);
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "UPDATE":
        if (!Equal(export.Office2.EffectiveDate, export.HiddenPrev.EffectiveDate)
          && Lt(export.Office2.EffectiveDate, local.Current.Date))
        {
          var field = GetField(export.Office2, "effectiveDate");

          field.Error = true;

          ExitState = "EFF_DTE_MUST_BE_EQ_GT_CURRENT_DT";

          break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseSpUpdateOffice();

        if (Equal(export.Office2.DiscontinueDate, local.Max.Date))
        {
          export.Office2.DiscontinueDate = UseCabSetMaximumDiscontinueDate1();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCseOrganization(export.CseOrganization,
            export.HiddenCseOrganization);
          export.HiddenFips.Assign(export.Fips);
        }
        else
        {
          return;
        }

        if (IsExitState("FN0000_CUSTOMER_SERVICE_CNTR_NF") || IsExitState
          ("FN_0000_OFFICE_IS_A_CUST_SER_CNT"))
        {
          var field =
            GetField(export.CustomerServiceCenter, "systemGeneratedId");

          field.Error = true;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (Length(TrimEnd(export.Group.Item.OfficeAddress.Zip)) > 0 && Length
              (TrimEnd(export.Group.Item.OfficeAddress.Zip)) < 5)
            {
              var field = GetField(export.Group.Item.OfficeAddress, "zip");

              field.Error = true;

              ExitState = "PLEASE_ENTER_FIVE_DIGIT_ZIP_RB";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.OfficeAddress.Zip)) > 0 && Verify
              (export.Group.Item.OfficeAddress.Zip, "0123456789") != 0)
            {
              var field = GetField(export.Group.Item.OfficeAddress, "zip");

              field.Error = true;

              ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.OfficeAddress.Zip)) == 0 && Length
              (TrimEnd(export.Group.Item.OfficeAddress.Zip4)) > 0)
            {
              var field = GetField(export.Group.Item.OfficeAddress, "zip4");

              field.Error = true;

              ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

              return;
            }

            if (Length(TrimEnd(export.Group.Item.OfficeAddress.Zip)) > 0 && Length
              (TrimEnd(export.Group.Item.OfficeAddress.Zip4)) > 0)
            {
              if (Length(TrimEnd(export.Group.Item.OfficeAddress.Zip4)) < 4)
              {
                var field = GetField(export.Group.Item.OfficeAddress, "zip4");

                field.Error = true;

                ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

                return;
              }
              else if (Verify(export.Group.Item.OfficeAddress.Zip4, "0123456789")
                != 0)
              {
                var field = GetField(export.Group.Item.OfficeAddress, "zip4");

                field.Error = true;

                ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

                return;
              }
            }

            if (!IsEmpty(export.Group.Item.OfficeAddress.Type1) && IsEmpty
              (export.Group.Item.Hidden.Type1))
            {
              // PREVIOUS VALUE IS BLANK, MUST BE AN ADD
              UseSpCreateOfficeAddress();
            }
            else
            {
              // PREVIOUS VALUE IS NOT BLANK, MUST BE AN UPDATE
              UseSpUpdateOfficeAddress();
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Group.Update.Common.SelectChar = "";
              export.Group.Update.Hidden.Type1 =
                export.Group.Item.OfficeAddress.Type1;

              var field = GetField(export.Group.Item.OfficeAddress, "type1");

              field.Color = "cyan";
              field.Protected = true;
            }
            else if (IsExitState("OFFICE_ADDRESS_AE"))
            {
              var field = GetField(export.Group.Item.OfficeAddress, "type1");

              field.Error = true;

              export.Group.Update.Common.SelectChar = "";
              ExitState = "DUPLICATE_ADDRESS_NOT_ALLOWED";
            }
            else
            {
              break;
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Office1.SelectChar = "";
        }
        else
        {
          return;
        }

        MoveOffice(export.Office2, export.HiddenOffice);
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "LIST":
        local.Count.Count = 0;

        // count the number of selected attributes
        if (AsChar(export.PromptOffice.Flag) == 'S')
        {
          ++local.Count.Count;
        }

        if (AsChar(export.PromptCustomerServiceC.Flag) == 'S')
        {
          ++local.Count.Count;
        }

        if (AsChar(export.PromptOfficeType.Flag) == 'S')
        {
          ++local.Count.Count;
        }

        if (AsChar(export.ListFips.Flag) == 'S')
        {
          ++local.Count.Count;
        }

        if (AsChar(export.ListCounty.Flag) == 'S')
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
        }

        if (local.Count.Count == 0)
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          return;
        }

        if (local.Count.Count > 1)
        {
          if (AsChar(export.PromptOffice.Flag) == 'S')
          {
            var field = GetField(export.PromptOffice, "flag");

            field.Error = true;
          }

          if (AsChar(export.PromptCustomerServiceC.Flag) == 'S')
          {
            var field = GetField(export.PromptCustomerServiceC, "flag");

            field.Error = true;
          }

          if (AsChar(export.PromptOfficeType.Flag) == 'S')
          {
            var field = GetField(export.PromptOfficeType, "flag");

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
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(export.PromptOffice.Flag) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_OFFICE";

          return;
        }

        if (AsChar(export.PromptCustomerServiceC.Flag) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_OFFICE";

          return;
        }

        if (AsChar(export.PromptOfficeType.Flag) == 'S')
        {
          export.HiddenToCodeTableList.CodeName = "OFFICE TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.ListFips.Flag) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_FIPS";

          return;
        }

        if (AsChar(export.ListCounty.Flag) == 'S')
        {
          export.CseOrganization.Type1 = "C";
          MoveCseOrganization(export.CseOrganization,
            export.HiddenCseOrganization);
          export.CseOrganization.Code = "";
          ExitState = "ECO_LNK_TO_CSE_ORGANIZATION";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.PromptAddressType.Flag) == 'S')
          {
            export.HiddenToCodeTableList.CodeName = "ADDRESS TYPE";
            export.Group.Update.Hidden.Type1 =
              export.Group.Item.OfficeAddress.Type1;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
    }

Test3:

    export.HiddenPrev.EffectiveDate = export.Office2.EffectiveDate;
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Type1 = source.Type1;
  }

  private static void MoveGroup(SpCabReadOfcAndOfcAddr.Export.GroupGroup source,
    Export.GroupGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.OfficeAddress.Assign(source.OfficeAddress);
    target.AddTypeDesc.Description = source.AddTypeDesc.Description;
    target.PromptAddressType.Flag = source.PromptAddressType.Flag;
    target.PromptState.Flag = source.PromptState.Flag;
    target.Hidden.Type1 = source.Hidden.Type1;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveOfficeAddress(OfficeAddress source,
    OfficeAddress target)
  {
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.StateProvince = source.StateProvince;
    target.PostalCode = source.PostalCode;
    target.Zip = source.Zip;
    target.Zip4 = source.Zip4;
    target.Country = source.Country;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

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
    export.OfficeTypeDesc.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue3()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
    local.ValidCode.Flag = useExport.ValidCode.Flag;
    export.Group.Update.AddTypeDesc.Description =
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

  private void UseSpCabReadOfcAndOfcAddr1()
  {
    var useImport = new SpCabReadOfcAndOfcAddr.Import();
    var useExport = new SpCabReadOfcAndOfcAddr.Export();

    useImport.PromptCustomerServiceC.Flag = import.PromptCustomerServiceC.Flag;
    useImport.PromtOffice.Flag = import.PromptOffice.Flag;
    useImport.CustomerServiceCenter.SystemGeneratedId =
      export.CustomerServiceCenter.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = export.Office2.SystemGeneratedId;

    Call(SpCabReadOfcAndOfcAddr.Execute, useImport, useExport);

    MoveOffice(useExport.CustomerServiceCenter, export.CustomerServiceCenter);
    export.Fips.Assign(useExport.Fips);
    MoveCseOrganization(useExport.CseOrganization, export.CseOrganization);
    export.Office2.Assign(useExport.Office);
    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseSpCabReadOfcAndOfcAddr2()
  {
    var useImport = new SpCabReadOfcAndOfcAddr.Import();
    var useExport = new SpCabReadOfcAndOfcAddr.Export();

    useImport.CustomerServiceCenter.SystemGeneratedId =
      export.CustomerServiceCenter.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = export.Office2.SystemGeneratedId;

    Call(SpCabReadOfcAndOfcAddr.Execute, useImport, useExport);

    export.Office2.Assign(useExport.Office);
    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseSpCabValidateOfficeDelete()
  {
    var useImport = new SpCabValidateOfficeDelete.Import();
    var useExport = new SpCabValidateOfficeDelete.Export();

    useImport.Office.SystemGeneratedId = export.Office2.SystemGeneratedId;

    Call(SpCabValidateOfficeDelete.Execute, useImport, useExport);
  }

  private void UseSpCreateOffice()
  {
    var useImport = new SpCreateOffice.Import();
    var useExport = new SpCreateOffice.Export();

    useImport.CustomerServiceCenter.SystemGeneratedId =
      export.CustomerServiceCenter.SystemGeneratedId;
    useImport.CseOrganization.Assign(entities.CseOrganization);
    useImport.Fips.Assign(entities.Fips);
    useImport.Office.Assign(export.Office2);

    Call(SpCreateOffice.Execute, useImport, useExport);

    MoveCseOrganization(useImport.CseOrganization, entities.CseOrganization);
    entities.Fips.Assign(useImport.Fips);
    export.Office2.SystemGeneratedId = useExport.Office.SystemGeneratedId;
  }

  private void UseSpCreateOfficeAddress()
  {
    var useImport = new SpCreateOfficeAddress.Import();
    var useExport = new SpCreateOfficeAddress.Export();

    useImport.Office.SystemGeneratedId = export.Office2.SystemGeneratedId;
    useImport.OfficeAddress.Assign(export.Group.Item.OfficeAddress);

    Call(SpCreateOfficeAddress.Execute, useImport, useExport);
  }

  private void UseSpDeleteOffice()
  {
    var useImport = new SpDeleteOffice.Import();
    var useExport = new SpDeleteOffice.Export();

    useImport.Office.SystemGeneratedId = export.Office2.SystemGeneratedId;

    Call(SpDeleteOffice.Execute, useImport, useExport);
  }

  private void UseSpDeleteOfficeAddress()
  {
    var useImport = new SpDeleteOfficeAddress.Import();
    var useExport = new SpDeleteOfficeAddress.Export();

    useImport.Office.SystemGeneratedId = export.Office2.SystemGeneratedId;
    useImport.OfficeAddress.Type1 = export.Group.Item.OfficeAddress.Type1;

    Call(SpDeleteOfficeAddress.Execute, useImport, useExport);
  }

  private void UseSpUpdateOffice()
  {
    var useImport = new SpUpdateOffice.Import();
    var useExport = new SpUpdateOffice.Export();

    useImport.CustomerServiceCenter.SystemGeneratedId =
      export.CustomerServiceCenter.SystemGeneratedId;
    useImport.CseOrganization.Assign(entities.CseOrganization);
    useImport.Fips.Assign(entities.Fips);
    useImport.OldFips.Assign(import.HiddenFips);
    MoveCseOrganization(import.HiddenCseOrganization,
      useImport.OldCseOrganization);
    useImport.Office.Assign(export.Office2);

    Call(SpUpdateOffice.Execute, useImport, useExport);

    MoveCseOrganization(useImport.CseOrganization, entities.CseOrganization);
    entities.Fips.Assign(useImport.Fips);
    MoveOffice(useExport.CustomerServiceCenter, export.CustomerServiceCenter);
  }

  private void UseSpUpdateOfficeAddress()
  {
    var useImport = new SpUpdateOfficeAddress.Import();
    var useExport = new SpUpdateOfficeAddress.Export();

    useImport.Office.SystemGeneratedId = export.Office2.SystemGeneratedId;
    MoveOfficeAddress(export.Group.Item.OfficeAddress, useImport.OfficeAddress);

    Call(SpUpdateOfficeAddress.Execute, useImport, useExport);
  }

  private bool ReadCseOrganization()
  {
    entities.CseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "organztnId", export.CseOrganization.Code);
      },
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.Fips.State);
        db.SetInt32(command, "county", export.Fips.County);
        db.SetInt32(command, "location", export.Fips.Location);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadOffice1()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.SetString(command, "name", import.Office2.Name);
        db.SetString(command, "typeCode", import.Office2.TypeCode);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOffice2()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice2",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office2.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOffice3()
  {
    entities.CustomerServiceCenter.Populated = false;

    return Read("ReadOffice3",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", export.CustomerServiceCenter.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.CustomerServiceCenter.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CustomerServiceCenter.Name = db.GetString(reader, 1);
        entities.CustomerServiceCenter.OffOffice =
          db.GetNullableInt32(reader, 2);
        entities.CustomerServiceCenter.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        var date = Now().Date;

        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetDate(command, "effectiveDate", date);
        db.SetNullableDate(
          command, "discontinueDate",
          export.Office2.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of AddTypeDesc.
      /// </summary>
      [JsonPropertyName("addTypeDesc")]
      public CodeValue AddTypeDesc
      {
        get => addTypeDesc ??= new();
        set => addTypeDesc = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public OfficeAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 16;

      private Common common;
      private OfficeAddress officeAddress;
      private CodeValue addTypeDesc;
      private Common promptAddressType;
      private Common promptState;
      private OfficeAddress hidden;
    }

    /// <summary>
    /// A value of PromptCustomerServiceC.
    /// </summary>
    [JsonPropertyName("promptCustomerServiceC")]
    public Common PromptCustomerServiceC
    {
      get => promptCustomerServiceC ??= new();
      set => promptCustomerServiceC = value;
    }

    /// <summary>
    /// A value of CustomerServiceCenter.
    /// </summary>
    [JsonPropertyName("customerServiceCenter")]
    public Office CustomerServiceCenter
    {
      get => customerServiceCenter ??= new();
      set => customerServiceCenter = value;
    }

    /// <summary>
    /// A value of Office1.
    /// </summary>
    [JsonPropertyName("office1")]
    public Common Office1
    {
      get => office1 ??= new();
      set => office1 = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Office HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of HiddenSelectionFips.
    /// </summary>
    [JsonPropertyName("hiddenSelectionFips")]
    public Fips HiddenSelectionFips
    {
      get => hiddenSelectionFips ??= new();
      set => hiddenSelectionFips = value;
    }

    /// <summary>
    /// A value of HiddenSelectionCseOrganization.
    /// </summary>
    [JsonPropertyName("hiddenSelectionCseOrganization")]
    public CseOrganization HiddenSelectionCseOrganization
    {
      get => hiddenSelectionCseOrganization ??= new();
      set => hiddenSelectionCseOrganization = value;
    }

    /// <summary>
    /// A value of ListCounty.
    /// </summary>
    [JsonPropertyName("listCounty")]
    public Common ListCounty
    {
      get => listCounty ??= new();
      set => listCounty = value;
    }

    /// <summary>
    /// A value of ListFips.
    /// </summary>
    [JsonPropertyName("listFips")]
    public Common ListFips
    {
      get => listFips ??= new();
      set => listFips = value;
    }

    /// <summary>
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    /// <summary>
    /// A value of HiddenCseOrganization.
    /// </summary>
    [JsonPropertyName("hiddenCseOrganization")]
    public CseOrganization HiddenCseOrganization
    {
      get => hiddenCseOrganization ??= new();
      set => hiddenCseOrganization = value;
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
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of OfficeTypeDesc.
    /// </summary>
    [JsonPropertyName("officeTypeDesc")]
    public CodeValue OfficeTypeDesc
    {
      get => officeTypeDesc ??= new();
      set => officeTypeDesc = value;
    }

    /// <summary>
    /// A value of HiddenFromListCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenFromListCodeValue")]
    public CodeValue HiddenFromListCodeValue
    {
      get => hiddenFromListCodeValue ??= new();
      set => hiddenFromListCodeValue = value;
    }

    /// <summary>
    /// A value of HiddenFromListOffice.
    /// </summary>
    [JsonPropertyName("hiddenFromListOffice")]
    public Office HiddenFromListOffice
    {
      get => hiddenFromListOffice ??= new();
      set => hiddenFromListOffice = value;
    }

    /// <summary>
    /// A value of Office2.
    /// </summary>
    [JsonPropertyName("office2")]
    public Office Office2
    {
      get => office2 ??= new();
      set => office2 = value;
    }

    /// <summary>
    /// A value of PromptOffice.
    /// </summary>
    [JsonPropertyName("promptOffice")]
    public Common PromptOffice
    {
      get => promptOffice ??= new();
      set => promptOffice = value;
    }

    /// <summary>
    /// A value of PromptOfficeType.
    /// </summary>
    [JsonPropertyName("promptOfficeType")]
    public Common PromptOfficeType
    {
      get => promptOfficeType ??= new();
      set => promptOfficeType = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private Common promptCustomerServiceC;
    private Office customerServiceCenter;
    private Common office1;
    private Office hiddenPrev;
    private Fips hiddenSelectionFips;
    private CseOrganization hiddenSelectionCseOrganization;
    private Common listCounty;
    private Common listFips;
    private Fips hiddenFips;
    private CseOrganization hiddenCseOrganization;
    private Fips fips;
    private CseOrganization cseOrganization;
    private CodeValue officeTypeDesc;
    private CodeValue hiddenFromListCodeValue;
    private Office hiddenFromListOffice;
    private Office office2;
    private Common promptOffice;
    private Common promptOfficeType;
    private Office hiddenOffice;
    private Standard standard;
    private Array<GroupGroup> group;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of AddTypeDesc.
      /// </summary>
      [JsonPropertyName("addTypeDesc")]
      public CodeValue AddTypeDesc
      {
        get => addTypeDesc ??= new();
        set => addTypeDesc = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public OfficeAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 16;

      private Common common;
      private OfficeAddress officeAddress;
      private CodeValue addTypeDesc;
      private Common promptAddressType;
      private Common promptState;
      private OfficeAddress hidden;
    }

    /// <summary>
    /// A value of PromptCustomerServiceC.
    /// </summary>
    [JsonPropertyName("promptCustomerServiceC")]
    public Common PromptCustomerServiceC
    {
      get => promptCustomerServiceC ??= new();
      set => promptCustomerServiceC = value;
    }

    /// <summary>
    /// A value of CustomerServiceCenter.
    /// </summary>
    [JsonPropertyName("customerServiceCenter")]
    public Office CustomerServiceCenter
    {
      get => customerServiceCenter ??= new();
      set => customerServiceCenter = value;
    }

    /// <summary>
    /// A value of Office1.
    /// </summary>
    [JsonPropertyName("office1")]
    public Common Office1
    {
      get => office1 ??= new();
      set => office1 = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Office HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of ListCounty.
    /// </summary>
    [JsonPropertyName("listCounty")]
    public Common ListCounty
    {
      get => listCounty ??= new();
      set => listCounty = value;
    }

    /// <summary>
    /// A value of ListFips.
    /// </summary>
    [JsonPropertyName("listFips")]
    public Common ListFips
    {
      get => listFips ??= new();
      set => listFips = value;
    }

    /// <summary>
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    /// <summary>
    /// A value of HiddenCseOrganization.
    /// </summary>
    [JsonPropertyName("hiddenCseOrganization")]
    public CseOrganization HiddenCseOrganization
    {
      get => hiddenCseOrganization ??= new();
      set => hiddenCseOrganization = value;
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
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of OfficeTypeDesc.
    /// </summary>
    [JsonPropertyName("officeTypeDesc")]
    public CodeValue OfficeTypeDesc
    {
      get => officeTypeDesc ??= new();
      set => officeTypeDesc = value;
    }

    /// <summary>
    /// A value of HiddenToCodeTableList.
    /// </summary>
    [JsonPropertyName("hiddenToCodeTableList")]
    public Code HiddenToCodeTableList
    {
      get => hiddenToCodeTableList ??= new();
      set => hiddenToCodeTableList = value;
    }

    /// <summary>
    /// A value of Office2.
    /// </summary>
    [JsonPropertyName("office2")]
    public Office Office2
    {
      get => office2 ??= new();
      set => office2 = value;
    }

    /// <summary>
    /// A value of PromptOffice.
    /// </summary>
    [JsonPropertyName("promptOffice")]
    public Common PromptOffice
    {
      get => promptOffice ??= new();
      set => promptOffice = value;
    }

    /// <summary>
    /// A value of PromptOfficeType.
    /// </summary>
    [JsonPropertyName("promptOfficeType")]
    public Common PromptOfficeType
    {
      get => promptOfficeType ??= new();
      set => promptOfficeType = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private Common promptCustomerServiceC;
    private Office customerServiceCenter;
    private Common office1;
    private Office hiddenPrev;
    private Common listCounty;
    private Common listFips;
    private Fips hiddenFips;
    private CseOrganization hiddenCseOrganization;
    private Fips fips;
    private CseOrganization cseOrganization;
    private CodeValue officeTypeDesc;
    private Code hiddenToCodeTableList;
    private Office office2;
    private Common promptOffice;
    private Common promptOfficeType;
    private Office hiddenOffice;
    private Standard standard;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ProtectedAddrType.
    /// </summary>
    [JsonPropertyName("protectedAddrType")]
    public Common ProtectedAddrType
    {
      get => protectedAddrType ??= new();
      set => protectedAddrType = value;
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
    /// A value of UpdateCount.
    /// </summary>
    [JsonPropertyName("updateCount")]
    public Common UpdateCount
    {
      get => updateCount ??= new();
      set => updateCount = value;
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
    /// A value of ClearOffice.
    /// </summary>
    [JsonPropertyName("clearOffice")]
    public Office ClearOffice
    {
      get => clearOffice ??= new();
      set => clearOffice = value;
    }

    /// <summary>
    /// A value of ClearOfficeAddress.
    /// </summary>
    [JsonPropertyName("clearOfficeAddress")]
    public OfficeAddress ClearOfficeAddress
    {
      get => clearOfficeAddress ??= new();
      set => clearOfficeAddress = value;
    }

    /// <summary>
    /// A value of Remaining.
    /// </summary>
    [JsonPropertyName("remaining")]
    public Common Remaining
    {
      get => remaining ??= new();
      set => remaining = value;
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
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    private Common protectedAddrType;
    private DateWorkArea current;
    private Common updateCount;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Office clearOffice;
    private OfficeAddress clearOfficeAddress;
    private Common remaining;
    private Common totalCount;
    private Common selectedCount;
    private Common returnCode;
    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private Common count;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CustomerServiceCenter.
    /// </summary>
    [JsonPropertyName("customerServiceCenter")]
    public Office CustomerServiceCenter
    {
      get => customerServiceCenter ??= new();
      set => customerServiceCenter = value;
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
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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

    private OfficeServiceProvider officeServiceProvider;
    private Office customerServiceCenter;
    private Office office;
    private CseOrganization cseOrganization;
    private Fips fips;
  }
#endregion
}
