// Program: OE_VEND_MAINTAIN_VENDOR, ID: 371796206, model: 746.
// Short name: SWEVENDP
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
/// A program: OE_VEND_MAINTAIN_VENDOR.
/// </para>
/// <para>
/// RESP: OBLGESTB
///  This procedure maintains vendor and it's address. Allows to create new 
/// vendor and updates existing vendor and vendor-address. Control can be passed
/// over from List-Vendor procedure for selected vendor for updating.
/// Processing;
/// 1. PF4 - Display Vendor and current effective Vendor-Address.
/// 2. PF5 - Creates Vendor and Vendor-Address.  Vendor-Address expiry-date is 
/// set to 12312999 at initial creation time. Vendor-Address effective-date
/// should be &lt;= current date.
/// 3. PF6 - Updates Vendor and/or Vendor-Address. Imported Vendor attributes 
/// are compared with existing Vendor attributes. When any of imported
/// attributes are different from existing attributes, Vendor-update flag is set
/// to update the Vendor.  Same process is applied to Vendor-Address if
/// imported Vendor-address effective-date is same with hidden Vendor-Address
/// effective-date.  When Vendor-Address effective-date is > current effective
/// vendor-address effective-date, current effective vendor-address expiry-date
/// is update with imported effective-date - 1 day. Then new future effective
/// Vendor-Address is created.
/// 4. PF7 - Display effective Vendor-Address which has effective-date &lt; 
/// imported effective-date. If no previous effective vendor-address found,
/// imported vendor-address is exported to screen with proper message.
/// 5. PF8 - Display effective Vendor-Address which has effective-date > 
/// imported effective-date. If no next effective vendor-address found, imported
/// vendor-address is redisplayed with proper message.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeVendMaintainVendor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_VEND_MAINTAIN_VENDOR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeVendMaintainVendor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeVendMaintainVendor.
  /// </summary>
  public OeVendMaintainVendor(IContext context, Import import, Export export):
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
    // SYSTEM:             KESSEP
    // MODULE:
    // MODULE TYPE:
    // ACTION BLOCKS:
    //        CREATE-VENDOR-ADDRESS
    //        CREATE-VENDOR
    // ENTITY TYPES USED:
    //        VENDOR            -R- -C- -U-
    //        VENDOR-ADDRESS        -C- -U-
    // DATABASE FILES USED:
    // MAINTENANCE LOG
    // AUTHOR          DATE           DESCRIPTION
    // Grace P Kim     1/24/95        	Initial Coding
    // Sherri Newman   3/18/96        	Retrofit
    // Gary Lofton	4/02/96		Added prompt to VENL
    // R. Marchman	11/19/96	Add new security and next tran.
    // SHERAZ		4/29/97		CHANGE CURRENT_DATE
    // Srini Ganji	9/20/99		PR#H00074468
    // Madhu Kumar      05/15/01     PR# 116889
    //     Edit check for  4 digit and 5 digit zip codes.
    // *********************************************
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // Move Imports to Exports.
    // ========================
    export.Vendor.Assign(import.Vendor);
    MoveVendor(import.HiddenVendor, export.HiddenVendor);
    export.VendorAddress.Assign(import.VendorAddress);
    export.HiddenVendorAddress.EffectiveDate =
      import.HiddenVendorAddress.EffectiveDate;
    export.ListVendors.PromptField = import.ListVendors.PromptField;
    export.ListStateCodes.PromptField = import.ListStateCodes.PromptField;
    export.ListVendorTypes.PromptField = import.ListVendorTypes.PromptField;
    export.VendorType.Description = import.VendorType.Description;
    MoveCodeValue(import.DlgflwSelected, export.DlgflwSelected);
    export.HiddenSelected.Assign(import.HiddenSelected);

    if (Equal(global.Command, "RSVENL"))
    {
      export.ListVendors.PromptField = "";

      if (export.HiddenSelected.Identifier > 0)
      {
        export.Vendor.Identifier = export.HiddenSelected.Identifier;
      }
      else
      {
        global.Command = "DISPLAY";
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.Vendor.Name) && export.Vendor.Identifier == 0)
      {
        var field = GetField(export.Vendor, "name");

        field.Error = true;

        ExitState = "OE0014_MANDATORY_FIELD_MISSING";

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.Vendor.Name))
      {
        var field = GetField(export.Vendor, "name");

        field.Error = true;

        ExitState = "OE0014_MANDATORY_FIELD_MISSING";
      }

      if (!Equal(export.Vendor.ServiceTypeCode, "HOME"))
      {
        if (IsEmpty(export.VendorAddress.Street1))
        {
          var field = GetField(export.VendorAddress, "street1");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }

        if (IsEmpty(export.VendorAddress.City))
        {
          var field = GetField(export.VendorAddress, "city");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }

        if (IsEmpty(export.VendorAddress.State))
        {
          var field = GetField(export.VendorAddress, "state");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }

        if (IsEmpty(export.VendorAddress.ZipCode5))
        {
          var field = GetField(export.VendorAddress, "zipCode5");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test1;
      }

      local.ErrorDecoding.Flag = "";
      local.Code.CodeName = "SERVICE TYPE";
      local.CodeValue.Cdvalue = import.Vendor.ServiceTypeCode ?? Spaces(10);
      UseCabGetCodeValueDescription1();

      if (AsChar(local.ErrorDecoding.Flag) == 'Y')
      {
        var field = GetField(export.Vendor, "serviceTypeCode");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_CODE";

        goto Test1;
      }

      if (!IsEmpty(export.VendorAddress.State))
      {
        local.ValidCode.Flag = "";
        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue = import.VendorAddress.State ?? Spaces(10);
        UseCabValidateCodeValue();

        if (local.ReturnCode.Count > 1)
        {
          var field = GetField(export.VendorAddress, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE";

          goto Test1;
        }
      }

      if (export.VendorAddress.EffectiveDate == null)
      {
        export.VendorAddress.EffectiveDate = local.Current.Date;
      }

      if (export.VendorAddress.ExpiryDate == null)
      {
        export.VendorAddress.ExpiryDate = new DateTime(2099, 12, 31);
      }
    }

Test1:

    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "RSVENL"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "LIST":
        if (!IsEmpty(export.ListVendors.PromptField) && AsChar
          (export.ListVendors.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListVendors, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListStateCodes.PromptField) && AsChar
          (export.ListStateCodes.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListStateCodes, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListVendorTypes.PromptField) && AsChar
          (export.ListVendorTypes.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListVendorTypes, "promptField");

          field.Error = true;
        }

        if (IsEmpty(export.ListStateCodes.PromptField) && IsEmpty
          (export.ListVendorTypes.PromptField) && IsEmpty
          (export.ListVendors.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field3 = GetField(export.ListVendors, "promptField");

          field3.Error = true;

          var field4 = GetField(export.ListStateCodes, "promptField");

          field4.Error = true;

          var field5 = GetField(export.ListVendorTypes, "promptField");

          field5.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(export.ListVendors.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_VENDOR";
        }
        else if (AsChar(export.ListStateCodes.PromptField) == 'S')
        {
          export.RequiredCode.CodeName = "STATE CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }
        else if (AsChar(export.ListVendorTypes.PromptField) == 'S')
        {
          export.RequiredCode.CodeName = "SERVICE TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }

        break;
      case "RETCDVL":
        if (AsChar(export.ListStateCodes.PromptField) == 'S')
        {
          export.ListStateCodes.PromptField = "";

          if (IsEmpty(export.DlgflwSelected.Cdvalue))
          {
            var field = GetField(export.VendorAddress, "state");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.VendorAddress.State = export.DlgflwSelected.Cdvalue;

            var field = GetField(export.VendorAddress, "zipCode5");

            field.Protected = false;
            field.Focused = true;
          }
        }
        else if (AsChar(export.ListVendorTypes.PromptField) == 'S')
        {
          export.ListVendorTypes.PromptField = "";

          if (IsEmpty(export.DlgflwSelected.Cdvalue))
          {
            var field = GetField(export.Vendor, "serviceTypeCode");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.Vendor.ServiceTypeCode = export.DlgflwSelected.Cdvalue;
            export.VendorType.Description = export.DlgflwSelected.Description;

            var field = GetField(export.Vendor, "name");

            field.Protected = false;
            field.Focused = true;
          }
        }

        break;
      case "ADD":
        if ((!IsEmpty(export.VendorAddress.Street2) || !
          IsEmpty(export.VendorAddress.City) || !
          IsEmpty(export.VendorAddress.State) || !
          IsEmpty(export.VendorAddress.ZipCode5) || !
          IsEmpty(export.VendorAddress.ZipCode4)) && IsEmpty
          (export.VendorAddress.Street1))
        {
          var field = GetField(export.VendorAddress, "street1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if ((!IsEmpty(export.VendorAddress.Street1) || !
          IsEmpty(export.VendorAddress.Street2) || !
          IsEmpty(export.VendorAddress.State) || !
          IsEmpty(export.VendorAddress.ZipCode5) || !
          IsEmpty(export.VendorAddress.ZipCode4)) && IsEmpty
          (export.VendorAddress.City))
        {
          var field = GetField(export.VendorAddress, "city");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if ((!IsEmpty(export.VendorAddress.Street1) || !
          IsEmpty(export.VendorAddress.Street2) || !
          IsEmpty(export.VendorAddress.City) || !
          IsEmpty(export.VendorAddress.ZipCode5) || !
          IsEmpty(export.VendorAddress.ZipCode4)) && IsEmpty
          (export.VendorAddress.State))
        {
          var field = GetField(export.VendorAddress, "state");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if ((!IsEmpty(export.VendorAddress.Street1) || !
          IsEmpty(export.VendorAddress.Street2) || !
          IsEmpty(export.VendorAddress.City) || !
          IsEmpty(export.VendorAddress.State) || !
          IsEmpty(export.VendorAddress.ZipCode4)) && IsEmpty
          (export.VendorAddress.ZipCode5))
        {
          var field = GetField(export.VendorAddress, "zipCode5");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if (!IsEmpty(export.VendorAddress.ZipCode4) && IsEmpty
          (export.VendorAddress.ZipCode5))
        {
          var field = GetField(export.VendorAddress, "zipCode5");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if (!IsEmpty(export.VendorAddress.ZipCode5))
        {
          do
          {
            ++local.Common.Count;
            local.Common.Flag =
              Substring(export.VendorAddress.ZipCode5, local.Common.Count, 1);

            if (AsChar(local.Common.Flag) < '0' || AsChar(local.Common.Flag) > '9'
              )
            {
              var field = GetField(export.VendorAddress, "zipCode5");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
              }

              goto Test2;
            }
          }
          while(local.Common.Count < 5);
        }

Test2:

        if (Length(TrimEnd(export.VendorAddress.ZipCode4)) > 0 && Length
          (TrimEnd(export.VendorAddress.ZipCode4)) < 4)
        {
          var field = GetField(export.VendorAddress, "zipCode4");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";
          }
        }

        if (Verify(export.VendorAddress.ZipCode4, "0123456789") != 0 && Length
          (TrimEnd(export.VendorAddress.ZipCode4)) > 0)
        {
          var field = GetField(export.VendorAddress, "zipCode4");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";
          }
        }

        if (Lt(local.Current.Date, export.VendorAddress.EffectiveDate))
        {
          var field = GetField(export.VendorAddress, "effectiveDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0020_VEN_ADDR_FUTURE_EFF_DATE";
          }
        }

        if (Lt(export.VendorAddress.ExpiryDate,
          export.VendorAddress.EffectiveDate))
        {
          var field = GetField(export.VendorAddress, "expiryDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "DISC_DATE_CANNOT_LT_EFF_DATE";
          }
        }

        if ((!IsEmpty(export.Vendor.PhoneExt) || export
          .Vendor.PhoneNumber.GetValueOrDefault() > 0) && export
          .Vendor.PhoneAreaCode.GetValueOrDefault() == 0)
        {
          var field = GetField(export.Vendor, "phoneAreaCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if ((!IsEmpty(export.Vendor.PhoneExt) || export
          .Vendor.PhoneAreaCode.GetValueOrDefault() > 0) && export
          .Vendor.PhoneNumber.GetValueOrDefault() == 0)
        {
          var field = GetField(export.Vendor, "phoneNumber");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if (!IsEmpty(export.Vendor.PhoneExt))
        {
          local.Common.Count = 0;

          do
          {
            ++local.Common.Count;
            local.Common.Flag =
              Substring(export.Vendor.PhoneExt, local.Common.Count, 1);

            if (!IsEmpty(local.Common.Flag) && (AsChar(local.Common.Flag) < '0'
              || AsChar(local.Common.Flag) > '9'))
            {
              var field = GetField(export.Vendor, "phoneExt");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "PHONE_NUMBER_NOT_NUMERIC";

                break;
              }
            }
          }
          while(local.Common.Count < 5);
        }

        if (export.Vendor.Fax.GetValueOrDefault() > 0 && export
          .Vendor.FaxAreaCode.GetValueOrDefault() == 0)
        {
          var field = GetField(export.Vendor, "faxAreaCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if (export.Vendor.FaxAreaCode.GetValueOrDefault() > 0 && export
          .Vendor.Fax.GetValueOrDefault() == 0)
        {
          var field = GetField(export.Vendor, "fax");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        UseOeVendCreateVendor();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Vendor, "name");

          field.Error = true;

          break;
        }

        UseOeVendCreateVendorAddress();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field3 = GetField(export.Vendor, "name");

          field3.Error = true;

          var field4 = GetField(export.VendorAddress, "street1");

          field4.Error = true;

          UseEabRollbackCics();

          break;
        }

        MoveVendor(export.Vendor, export.HiddenVendor);
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "DISPLAY":
        export.ListVendors.PromptField = "";
        export.ListStateCodes.PromptField = "";
        export.ListVendorTypes.PromptField = "";
        export.VendorType.Description = Spaces(CodeValue.Description_MaxLength);

        if (ReadVendorVendorAddress())
        {
          export.Vendor.Assign(entities.Vendor);
          MoveVendor(entities.Vendor, export.HiddenVendor);
          MoveVendorAddress2(entities.VendorAddress, export.VendorAddress);
          export.HiddenVendorAddress.EffectiveDate =
            entities.VendorAddress.EffectiveDate;
          local.Code.CodeName = "SERVICE TYPE";
          local.CodeValue.Cdvalue = entities.Vendor.ServiceTypeCode ?? Spaces
            (10);
          UseCabGetCodeValueDescription2();
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

          break;
        }

        export.Vendor.Assign(entities.Vendor);
        MoveVendor(entities.Vendor, export.HiddenVendor);
        MoveVendorAddress2(entities.VendorAddress, export.VendorAddress);
        export.HiddenVendorAddress.EffectiveDate =
          entities.VendorAddress.EffectiveDate;
        export.Vendor.Name = import.Vendor.Name;
        ExitState = "FN0000_VENDOR_NF";

        var field1 = GetField(export.Vendor, "name");

        field1.Error = true;

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "RSVENL":
        if (export.HiddenSelected.Identifier == 0)
        {
          break;
        }

        var field2 = GetField(export.VendorAddress, "street1");

        field2.Protected = false;
        field2.Focused = true;

        if (ReadVendor())
        {
          export.Vendor.Assign(entities.Vendor);
          MoveVendor(entities.Vendor, export.HiddenVendor);
          local.Code.CodeName = "SERVICE TYPE";
          local.CodeValue.Cdvalue = entities.Vendor.ServiceTypeCode ?? Spaces
            (10);
          UseCabGetCodeValueDescription2();

          if (ReadVendorAddress())
          {
            MoveVendorAddress2(entities.VendorAddress, export.VendorAddress);
            export.HiddenVendorAddress.EffectiveDate =
              entities.VendorAddress.EffectiveDate;

            break;
          }

          ExitState = "OE0000_VENDOR_ADDRESS_NF";
        }
        else
        {
          ExitState = "OE0000_NF_VENDOR";

          var field = GetField(export.Vendor, "identifier");

          field.Error = true;
        }

        break;
      case "UPDATE":
        // *****************************************************************
        // Following Code commented by -Srini Ganji on 09/20/1999
        // PR#H00074468
        // *****************************************************************
        // *****************************************************************
        // End of commented code by -Srini Ganji on 09/20/1999
        // PR#H00074468
        // *****************************************************************
        if (export.Vendor.Identifier != export.HiddenVendor.Identifier)
        {
          var field = GetField(export.Vendor, "identifier");

          field.Error = true;

          ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";

          break;
        }

        if ((!IsEmpty(export.VendorAddress.Street2) || !
          IsEmpty(export.VendorAddress.City) || !
          IsEmpty(export.VendorAddress.State) || !
          IsEmpty(export.VendorAddress.ZipCode5) || !
          IsEmpty(export.VendorAddress.ZipCode4)) && IsEmpty
          (export.VendorAddress.Street1))
        {
          var field = GetField(export.VendorAddress, "street1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if ((!IsEmpty(export.VendorAddress.Street1) || !
          IsEmpty(export.VendorAddress.Street2) || !
          IsEmpty(export.VendorAddress.State) || !
          IsEmpty(export.VendorAddress.ZipCode5) || !
          IsEmpty(export.VendorAddress.ZipCode4)) && IsEmpty
          (export.VendorAddress.City))
        {
          var field = GetField(export.VendorAddress, "city");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if ((!IsEmpty(export.VendorAddress.Street1) || !
          IsEmpty(export.VendorAddress.Street2) || !
          IsEmpty(export.VendorAddress.City) || !
          IsEmpty(export.VendorAddress.ZipCode5) || !
          IsEmpty(export.VendorAddress.ZipCode4)) && IsEmpty
          (export.VendorAddress.State))
        {
          var field = GetField(export.VendorAddress, "state");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if ((!IsEmpty(export.VendorAddress.Street1) || !
          IsEmpty(export.VendorAddress.Street2) || !
          IsEmpty(export.VendorAddress.City) || !
          IsEmpty(export.VendorAddress.State) || !
          IsEmpty(export.VendorAddress.ZipCode4)) && IsEmpty
          (export.VendorAddress.ZipCode5))
        {
          var field = GetField(export.VendorAddress, "zipCode5");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if (!IsEmpty(export.VendorAddress.ZipCode4) && IsEmpty
          (export.VendorAddress.ZipCode5))
        {
          var field = GetField(export.VendorAddress, "zipCode5");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if (!IsEmpty(export.VendorAddress.ZipCode5))
        {
          do
          {
            ++local.Common.Count;
            local.Common.Flag =
              Substring(export.VendorAddress.ZipCode5, local.Common.Count, 1);

            if (AsChar(local.Common.Flag) < '0' || AsChar(local.Common.Flag) > '9'
              )
            {
              var field = GetField(export.VendorAddress, "zipCode5");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
              }

              goto Test3;
            }
          }
          while(local.Common.Count < 5);
        }

Test3:

        if (Length(TrimEnd(export.VendorAddress.ZipCode4)) > 0 && Length
          (TrimEnd(export.VendorAddress.ZipCode4)) < 4)
        {
          var field = GetField(export.VendorAddress, "zipCode4");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";
          }
        }

        if (Verify(export.VendorAddress.ZipCode4, "0123456789") != 0 && Length
          (TrimEnd(export.VendorAddress.ZipCode4)) > 0)
        {
          var field = GetField(export.VendorAddress, "zipCode4");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";
          }
        }

        if (!Equal(export.VendorAddress.EffectiveDate,
          export.HiddenVendorAddress.EffectiveDate))
        {
          export.VendorAddress.EffectiveDate =
            export.HiddenVendorAddress.EffectiveDate;

          var field = GetField(export.VendorAddress, "effectiveDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "CANNOT_CHANGE_EFFECTIVE_DATE";
          }
        }

        if (Lt(export.VendorAddress.ExpiryDate,
          export.VendorAddress.EffectiveDate))
        {
          var field = GetField(export.VendorAddress, "expiryDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "DISC_DATE_CANNOT_LT_EFF_DATE";
          }
        }

        if ((!IsEmpty(export.Vendor.PhoneExt) || export
          .Vendor.PhoneNumber.GetValueOrDefault() > 0) && export
          .Vendor.PhoneAreaCode.GetValueOrDefault() == 0)
        {
          var field = GetField(export.Vendor, "phoneAreaCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if ((!IsEmpty(export.Vendor.PhoneExt) || export
          .Vendor.PhoneAreaCode.GetValueOrDefault() > 0) && export
          .Vendor.PhoneNumber.GetValueOrDefault() == 0)
        {
          var field = GetField(export.Vendor, "phoneNumber");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if (!IsEmpty(export.Vendor.PhoneExt))
        {
          local.Common.Count = 0;

          do
          {
            ++local.Common.Count;
            local.Common.Flag =
              Substring(export.Vendor.PhoneExt, local.Common.Count, 1);

            if (!IsEmpty(local.Common.Flag) && (AsChar(local.Common.Flag) < '0'
              || AsChar(local.Common.Flag) > '9'))
            {
              var field = GetField(export.Vendor, "phoneExt");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "PHONE_NUMBER_NOT_NUMERIC";

                break;
              }
            }
          }
          while(local.Common.Count < 5);
        }

        if (export.Vendor.Fax.GetValueOrDefault() > 0 && export
          .Vendor.FaxAreaCode.GetValueOrDefault() == 0)
        {
          var field = GetField(export.Vendor, "faxAreaCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if (export.Vendor.FaxAreaCode.GetValueOrDefault() > 0 && export
          .Vendor.Fax.GetValueOrDefault() == 0)
        {
          var field = GetField(export.Vendor, "fax");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0014_MANDATORY_FIELD_MISSING";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        UseOeVendUpdVendorAndAddress();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Vendor, "identifier");

          field.Error = true;

          UseEabRollbackCics();

          break;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(export.VendorAddress.ExpiryDate, new DateTime(2099, 12, 31)))
    {
      export.VendorAddress.ExpiryDate = null;
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
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

  private static void MoveVendor(Vendor source, Vendor target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveVendorAddress1(VendorAddress source,
    VendorAddress target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.ExpiryDate = source.ExpiryDate;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveVendorAddress2(VendorAddress source,
    VendorAddress target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.ExpiryDate = source.ExpiryDate;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private void UseCabGetCodeValueDescription1()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.ErrorDecoding.Flag = useExport.ErrorInDecoding.Flag;
    export.VendorType.Description = useExport.CodeValue.Description;
  }

  private void UseCabGetCodeValueDescription2()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.VendorType.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeVendCreateVendor()
  {
    var useImport = new OeVendCreateVendor.Import();
    var useExport = new OeVendCreateVendor.Export();

    useImport.Vendor.Assign(export.Vendor);

    Call(OeVendCreateVendor.Execute, useImport, useExport);

    export.Vendor.Identifier = useExport.Vendor.Identifier;
  }

  private void UseOeVendCreateVendorAddress()
  {
    var useImport = new OeVendCreateVendorAddress.Import();
    var useExport = new OeVendCreateVendorAddress.Export();

    MoveVendorAddress1(export.VendorAddress, useImport.VendorAddress);
    useImport.New1.Identifier = export.Vendor.Identifier;

    Call(OeVendCreateVendorAddress.Execute, useImport, useExport);
  }

  private void UseOeVendUpdVendorAndAddress()
  {
    var useImport = new OeVendUpdVendorAndAddress.Import();
    var useExport = new OeVendUpdVendorAndAddress.Export();

    MoveVendorAddress1(export.VendorAddress, useImport.VendorAddress);
    useImport.Vendor.Assign(export.Vendor);

    Call(OeVendUpdVendorAndAddress.Execute, useImport, useExport);
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

  private bool ReadVendor()
  {
    entities.Vendor.Populated = false;

    return Read("ReadVendor",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", export.HiddenSelected.Identifier);
      },
      (db, reader) =>
      {
        entities.Vendor.Identifier = db.GetInt32(reader, 0);
        entities.Vendor.Name = db.GetString(reader, 1);
        entities.Vendor.Number = db.GetNullableString(reader, 2);
        entities.Vendor.PhoneNumber = db.GetNullableInt32(reader, 3);
        entities.Vendor.Fax = db.GetNullableInt32(reader, 4);
        entities.Vendor.ContactPerson = db.GetNullableString(reader, 5);
        entities.Vendor.ServiceTypeCode = db.GetNullableString(reader, 6);
        entities.Vendor.LastUpdatedBy = db.GetString(reader, 7);
        entities.Vendor.LastUpdatedTimestamp = db.GetDateTime(reader, 8);
        entities.Vendor.FaxExt = db.GetNullableString(reader, 9);
        entities.Vendor.PhoneExt = db.GetNullableString(reader, 10);
        entities.Vendor.FaxAreaCode = db.GetNullableInt32(reader, 11);
        entities.Vendor.PhoneAreaCode = db.GetNullableInt32(reader, 12);
        entities.Vendor.Populated = true;
      });
  }

  private bool ReadVendorAddress()
  {
    entities.VendorAddress.Populated = false;

    return Read("ReadVendorAddress",
      (db, command) =>
      {
        db.SetInt32(command, "venIdentifier", entities.Vendor.Identifier);
      },
      (db, reader) =>
      {
        entities.VendorAddress.VenIdentifier = db.GetInt32(reader, 0);
        entities.VendorAddress.EffectiveDate = db.GetDate(reader, 1);
        entities.VendorAddress.ExpiryDate = db.GetDate(reader, 2);
        entities.VendorAddress.Street1 = db.GetNullableString(reader, 3);
        entities.VendorAddress.Street2 = db.GetNullableString(reader, 4);
        entities.VendorAddress.City = db.GetNullableString(reader, 5);
        entities.VendorAddress.State = db.GetNullableString(reader, 6);
        entities.VendorAddress.ZipCode5 = db.GetNullableString(reader, 7);
        entities.VendorAddress.ZipCode4 = db.GetNullableString(reader, 8);
        entities.VendorAddress.LastUpdatedBy = db.GetString(reader, 9);
        entities.VendorAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.VendorAddress.Populated = true;
      });
  }

  private bool ReadVendorVendorAddress()
  {
    entities.VendorAddress.Populated = false;
    entities.Vendor.Populated = false;

    return Read("ReadVendorVendorAddress",
      (db, command) =>
      {
        db.SetString(command, "name", export.Vendor.Name);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Vendor.Identifier = db.GetInt32(reader, 0);
        entities.VendorAddress.VenIdentifier = db.GetInt32(reader, 0);
        entities.Vendor.Name = db.GetString(reader, 1);
        entities.Vendor.Number = db.GetNullableString(reader, 2);
        entities.Vendor.PhoneNumber = db.GetNullableInt32(reader, 3);
        entities.Vendor.Fax = db.GetNullableInt32(reader, 4);
        entities.Vendor.ContactPerson = db.GetNullableString(reader, 5);
        entities.Vendor.ServiceTypeCode = db.GetNullableString(reader, 6);
        entities.Vendor.LastUpdatedBy = db.GetString(reader, 7);
        entities.Vendor.LastUpdatedTimestamp = db.GetDateTime(reader, 8);
        entities.Vendor.FaxExt = db.GetNullableString(reader, 9);
        entities.Vendor.PhoneExt = db.GetNullableString(reader, 10);
        entities.Vendor.FaxAreaCode = db.GetNullableInt32(reader, 11);
        entities.Vendor.PhoneAreaCode = db.GetNullableInt32(reader, 12);
        entities.VendorAddress.EffectiveDate = db.GetDate(reader, 13);
        entities.VendorAddress.ExpiryDate = db.GetDate(reader, 14);
        entities.VendorAddress.Street1 = db.GetNullableString(reader, 15);
        entities.VendorAddress.Street2 = db.GetNullableString(reader, 16);
        entities.VendorAddress.City = db.GetNullableString(reader, 17);
        entities.VendorAddress.State = db.GetNullableString(reader, 18);
        entities.VendorAddress.ZipCode5 = db.GetNullableString(reader, 19);
        entities.VendorAddress.ZipCode4 = db.GetNullableString(reader, 20);
        entities.VendorAddress.LastUpdatedBy = db.GetString(reader, 21);
        entities.VendorAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 22);
        entities.VendorAddress.Populated = true;
        entities.Vendor.Populated = true;
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
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public Vendor HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// A value of RequiredCode.
    /// </summary>
    [JsonPropertyName("requiredCode")]
    public Code RequiredCode
    {
      get => requiredCode ??= new();
      set => requiredCode = value;
    }

    /// <summary>
    /// A value of StartingCodeValue.
    /// </summary>
    [JsonPropertyName("startingCodeValue")]
    public CodeValue StartingCodeValue
    {
      get => startingCodeValue ??= new();
      set => startingCodeValue = value;
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
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public CodeValue DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// A value of ListVendors.
    /// </summary>
    [JsonPropertyName("listVendors")]
    public Standard ListVendors
    {
      get => listVendors ??= new();
      set => listVendors = value;
    }

    /// <summary>
    /// A value of ListStateCodes.
    /// </summary>
    [JsonPropertyName("listStateCodes")]
    public Standard ListStateCodes
    {
      get => listStateCodes ??= new();
      set => listStateCodes = value;
    }

    /// <summary>
    /// A value of ListVendorTypes.
    /// </summary>
    [JsonPropertyName("listVendorTypes")]
    public Standard ListVendorTypes
    {
      get => listVendorTypes ??= new();
      set => listVendorTypes = value;
    }

    /// <summary>
    /// A value of HiddenVendorAddress.
    /// </summary>
    [JsonPropertyName("hiddenVendorAddress")]
    public VendorAddress HiddenVendorAddress
    {
      get => hiddenVendorAddress ??= new();
      set => hiddenVendorAddress = value;
    }

    /// <summary>
    /// A value of VendorType.
    /// </summary>
    [JsonPropertyName("vendorType")]
    public CodeValue VendorType
    {
      get => vendorType ??= new();
      set => vendorType = value;
    }

    /// <summary>
    /// A value of HiddenVendor.
    /// </summary>
    [JsonPropertyName("hiddenVendor")]
    public Vendor HiddenVendor
    {
      get => hiddenVendor ??= new();
      set => hiddenVendor = value;
    }

    /// <summary>
    /// A value of VendorAddress.
    /// </summary>
    [JsonPropertyName("vendorAddress")]
    public VendorAddress VendorAddress
    {
      get => vendorAddress ??= new();
      set => vendorAddress = value;
    }

    /// <summary>
    /// A value of Vendor.
    /// </summary>
    [JsonPropertyName("vendor")]
    public Vendor Vendor
    {
      get => vendor ??= new();
      set => vendor = value;
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

    private Vendor hiddenSelected;
    private Code requiredCode;
    private CodeValue startingCodeValue;
    private Standard standard;
    private CodeValue dlgflwSelected;
    private Standard listVendors;
    private Standard listStateCodes;
    private Standard listVendorTypes;
    private VendorAddress hiddenVendorAddress;
    private CodeValue vendorType;
    private Vendor hiddenVendor;
    private VendorAddress vendorAddress;
    private Vendor vendor;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public Vendor HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
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
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public CodeValue DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// A value of ListVendors.
    /// </summary>
    [JsonPropertyName("listVendors")]
    public Standard ListVendors
    {
      get => listVendors ??= new();
      set => listVendors = value;
    }

    /// <summary>
    /// A value of ListStateCodes.
    /// </summary>
    [JsonPropertyName("listStateCodes")]
    public Standard ListStateCodes
    {
      get => listStateCodes ??= new();
      set => listStateCodes = value;
    }

    /// <summary>
    /// A value of ListVendorTypes.
    /// </summary>
    [JsonPropertyName("listVendorTypes")]
    public Standard ListVendorTypes
    {
      get => listVendorTypes ??= new();
      set => listVendorTypes = value;
    }

    /// <summary>
    /// A value of HiddenVendorAddress.
    /// </summary>
    [JsonPropertyName("hiddenVendorAddress")]
    public VendorAddress HiddenVendorAddress
    {
      get => hiddenVendorAddress ??= new();
      set => hiddenVendorAddress = value;
    }

    /// <summary>
    /// A value of VendorType.
    /// </summary>
    [JsonPropertyName("vendorType")]
    public CodeValue VendorType
    {
      get => vendorType ??= new();
      set => vendorType = value;
    }

    /// <summary>
    /// A value of VendorAddress.
    /// </summary>
    [JsonPropertyName("vendorAddress")]
    public VendorAddress VendorAddress
    {
      get => vendorAddress ??= new();
      set => vendorAddress = value;
    }

    /// <summary>
    /// A value of Vendor.
    /// </summary>
    [JsonPropertyName("vendor")]
    public Vendor Vendor
    {
      get => vendor ??= new();
      set => vendor = value;
    }

    /// <summary>
    /// A value of HiddenVendor.
    /// </summary>
    [JsonPropertyName("hiddenVendor")]
    public Vendor HiddenVendor
    {
      get => hiddenVendor ??= new();
      set => hiddenVendor = value;
    }

    /// <summary>
    /// A value of RequiredCode.
    /// </summary>
    [JsonPropertyName("requiredCode")]
    public Code RequiredCode
    {
      get => requiredCode ??= new();
      set => requiredCode = value;
    }

    /// <summary>
    /// A value of StartingCodeValue.
    /// </summary>
    [JsonPropertyName("startingCodeValue")]
    public CodeValue StartingCodeValue
    {
      get => startingCodeValue ??= new();
      set => startingCodeValue = value;
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

    private Vendor hiddenSelected;
    private Standard standard;
    private CodeValue dlgflwSelected;
    private Standard listVendors;
    private Standard listStateCodes;
    private Standard listVendorTypes;
    private VendorAddress hiddenVendorAddress;
    private CodeValue vendorType;
    private VendorAddress vendorAddress;
    private Vendor vendor;
    private Vendor hiddenVendor;
    private Code requiredCode;
    private CodeValue startingCodeValue;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of ErrorDecoding.
    /// </summary>
    [JsonPropertyName("errorDecoding")]
    public Common ErrorDecoding
    {
      get => errorDecoding ??= new();
      set => errorDecoding = value;
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

    private Common common;
    private DateWorkArea current;
    private DateWorkArea max;
    private Common returnCode;
    private Common validCode;
    private Common errorDecoding;
    private CodeValue codeValue;
    private Code code;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of VendorAddress.
    /// </summary>
    [JsonPropertyName("vendorAddress")]
    public VendorAddress VendorAddress
    {
      get => vendorAddress ??= new();
      set => vendorAddress = value;
    }

    /// <summary>
    /// A value of Vendor.
    /// </summary>
    [JsonPropertyName("vendor")]
    public Vendor Vendor
    {
      get => vendor ??= new();
      set => vendor = value;
    }

    private VendorAddress vendorAddress;
    private Vendor vendor;
  }
#endregion
}
