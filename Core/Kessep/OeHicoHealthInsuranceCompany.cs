// Program: OE_HICO_HEALTH_INSURANCE_COMPANY, ID: 371861615, model: 746.
// Short name: SWEHICOP
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
/// A program: OE_HICO_HEALTH_INSURANCE_COMPANY.
/// </para>
/// <para>
/// Resp:OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeHicoHealthInsuranceCompany: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICO_HEALTH_INSURANCE_COMPANY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicoHealthInsuranceCompany(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicoHealthInsuranceCompany.
  /// </summary>
  public OeHicoHealthInsuranceCompany(IContext context, Import import,
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE 		DESCRIPTION
    // Rebecca Grimes	01/02/95	Initial Code
    // Sid		02/01/95	Rework/Completion
    // T.O.Redmond	02/15/96	Retrofit
    // G.Lofton	04/08/96	Unit test corrections
    // R. Marchman	11/14/96	Add new security and next tran.
    // Vithal Madhira  01/22/01        PR# 111764 Health Ins. Co. w/o address  
    // not
    //                                 
    // displaying on screen. Fixed this
    // problem.
    // Madhu Kumar     05/15/01        PR# 116889 Edit check for 4 and 5 digit 
    // zip
    //                                 
    // codes .
    // Mark Ashworth   10/02/01        WR# 20215 Add Start and End dates
    // 
    // Bonnie Lee      10/20/03        PR# 190664 Getting end date error on all
    // updates.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Curr.Date = Now().Date;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    UseOeCabSetMnemonics();
    local.Conversion.Date = new DateTime(1999, 9, 6);

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.HiddenCase.Number = import.HiddenCase.Number;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    export.HiddenCsePersonsWorkSet.Assign(import.HiddenCsePersonsWorkSet);
    export.HealthInsuranceCompanyAddress.Assign(
      import.HealthInsuranceCompanyAddress);
    export.HealthInsuranceCompany.Assign(import.HealthInsuranceCompany);
    export.WorkPromptCarrierCode.TextLength01 =
      import.WorkPromptCarrierCode.TextLength01;
    export.PromptState.SelectChar = import.PromptState.SelectChar;
    export.SelectedState.Assign(import.SelectedState);
    local.HealthInsuranceCompanyAddress.Assign(
      import.HealthInsuranceCompanyAddress);
    local.HealthInsuranceCompany.Assign(import.HealthInsuranceCompany);
    export.HiddenHealthInsuranceCompany.Assign(
      import.HiddenHealthInsuranceCompany);

    if (Equal(export.HiddenHealthInsuranceCompany.EndDate,
      local.MaxDate.ExpirationDate))
    {
      export.HiddenHealthInsuranceCompany.EndDate = null;
    }

    export.HiddenHealthInsuranceCompanyAddress.Assign(
      import.HiddenHealthInsuranceCompanyAddress);

    if (!IsEmpty(export.HealthInsuranceCompany.CarrierCode))
    {
      local.TextWorkArea.Text10 = export.HealthInsuranceCompany.CarrierCode ?? Spaces
        (10);
      UseEabPadLeftWithZeros();
      export.HealthInsuranceCompany.CarrierCode =
        Substring(local.TextWorkArea.Text10, 4, 7);
      local.HealthInsuranceCompany.CarrierCode =
        Substring(local.TextWorkArea.Text10, 4, 7);
    }

    // ---------------------------------------------
    // If any new value is keyed in, set the health
    // insurance company identifier to spaces.
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
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
      // ************************************************
      // *Example: If not equal to spaces or zeroes     *
      // *Set export cse person number to export next   *
      // *tran hidden info cse_person_number.           *
      // ************************************************
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      export.HiddenHealthInsuranceCompany.Identifier = 0;
    }

    if (Equal(global.Command, "RETHICL"))
    {
      if (import.HiclSelectionCount.Count < 1)
      {
        // ---------------------------------------------
        // Nothing was returned from the List.
        // ---------------------------------------------
        export.WorkPromptCarrierCode.TextLength01 = "";
        export.HealthInsuranceCompany.
          Assign(import.HiddenHealthInsuranceCompany);
        export.HealthInsuranceCompanyAddress.Assign(
          import.HiddenHealthInsuranceCompanyAddress);
        local.HealthInsuranceCompany.
          Assign(import.HiddenHealthInsuranceCompany);
        local.HealthInsuranceCompanyAddress.Assign(
          import.HiddenHealthInsuranceCompanyAddress);
        global.Command = "DISPLAY";
      }
      else
      {
        // ---------------------------------------------
        // Populate the screen with data sent.
        // ---------------------------------------------
        export.WorkPromptCarrierCode.TextLength01 = "";
        local.ReturnCode.Count = 1;
        local.HealthInsuranceCompany.
          Assign(import.PromptHealthInsuranceCompany);
        local.HealthInsuranceCompanyAddress.Assign(
          import.PromptHealthInsuranceCompanyAddress);
        export.HealthInsuranceCompany.
          Assign(import.PromptHealthInsuranceCompany);
        export.HealthInsuranceCompanyAddress.Assign(
          import.PromptHealthInsuranceCompanyAddress);
        export.HiddenHealthInsuranceCompany.
          Assign(export.HealthInsuranceCompany);
        export.HiddenHealthInsuranceCompanyAddress.Assign(
          export.HealthInsuranceCompanyAddress);
        export.WorkPromptCarrierCode.TextLength01 = "";
        global.Command = "BYPASS";
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }
    }

    if (Equal(global.Command, "XFERHICL"))
    {
      // ---------------------------------------------
      // Populate the screen with data sent.
      // ---------------------------------------------
      local.HealthInsuranceCompany.Assign(import.PromptHealthInsuranceCompany);
      local.HealthInsuranceCompanyAddress.Assign(
        import.PromptHealthInsuranceCompanyAddress);
      export.HealthInsuranceCompany.Assign(import.PromptHealthInsuranceCompany);
      export.HealthInsuranceCompanyAddress.Assign(
        import.PromptHealthInsuranceCompanyAddress);
      export.HiddenHealthInsuranceCompany.Assign(export.HealthInsuranceCompany);
      export.HiddenHealthInsuranceCompanyAddress.Assign(
        export.HealthInsuranceCompanyAddress);
      export.WorkPromptCarrierCode.TextLength01 = "";
      global.Command = "BYPASS";
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }

    if (IsEmpty(global.Command))
    {
      // ---------------------------------------------
      // Clear Screen Input was received if command is
      // spaces.
      // ---------------------------------------------
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "RETHICL") || Equal(global.Command, "BYPASS"))
    {
      // ************************************************
      // Added BYPASS to fix  PR# 111764. Check the above code for 'IF command 
      // is equal to  XFERHICL'
      //                                                      
      // Vithal (01/19/2001)
      // ************************************************
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
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
      case "RETURN":
        if (AsChar(export.PromptState.SelectChar) == 'S')
        {
          export.PromptState.SelectChar = "";
          export.HealthInsuranceCompanyAddress.State =
            import.SelectedState.Cdvalue;
          global.Command = "DISPLAY";
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";

          return;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        if (IsEmpty(import.WorkPromptCarrierCode.TextLength01) && IsEmpty
          (import.PromptState.SelectChar))
        {
          var field1 = GetField(export.WorkPromptCarrierCode, "textLength01");

          field1.Error = true;

          var field2 = GetField(export.PromptState, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (!IsEmpty(import.WorkPromptCarrierCode.TextLength01) && AsChar
          (import.WorkPromptCarrierCode.TextLength01) != 'S')
        {
          var field = GetField(export.WorkPromptCarrierCode, "textLength01");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (!IsEmpty(import.PromptState.SelectChar) && AsChar
          (import.PromptState.SelectChar) != 'S')
        {
          var field = GetField(export.PromptState, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE"))
        {
          return;
        }

        if (AsChar(export.WorkPromptCarrierCode.TextLength01) == 'S')
        {
          export.HiddenHealthInsuranceCompany.Assign(
            local.HealthInsuranceCompany);
          export.HiddenHealthInsuranceCompanyAddress.Assign(
            local.HealthInsuranceCompanyAddress);
          ExitState = "ECO_LNK_TO_LIST_INSURANCE_CO";
        }
        else if (AsChar(import.PromptState.SelectChar) == 'S')
        {
          export.State.CodeName = local.State.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }

        break;
      case "RETCDVL":
        if (AsChar(export.PromptState.SelectChar) == 'S')
        {
          export.PromptState.SelectChar = "";

          if (!IsEmpty(export.SelectedState.Cdvalue))
          {
            export.HealthInsuranceCompanyAddress.State =
              export.SelectedState.Cdvalue;

            var field =
              GetField(export.HealthInsuranceCompanyAddress, "zipCode5");

            field.Protected = false;
            field.Focused = true;
          }
          else if (!IsEmpty(export.HealthInsuranceCompanyAddress.State))
          {
            var field =
              GetField(export.HealthInsuranceCompanyAddress, "zipCode5");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.HealthInsuranceCompanyAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        break;
      case "CREATE":
        ExitState = "ACO_NN0000_ALL_OK";
        local.HealthInsuranceCompany.Identifier = 0;

        // ---------------------------------------------
        // To add a record required fields are code,name,
        // street address line1, city, and state.
        // ---------------------------------------------
        if (IsEmpty(import.HealthInsuranceCompany.CarrierCode))
        {
          var field = GetField(export.HealthInsuranceCompany, "carrierCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (IsEmpty(import.HealthInsuranceCompany.InsurancePolicyCarrier))
        {
          var field =
            GetField(export.HealthInsuranceCompany, "insurancePolicyCarrier");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        // ---------------------------------------------
        // Validate phone and fax number
        // ---------------------------------------------
        if (import.HealthInsuranceCompany.InsurerPhoneAreaCode.
          GetValueOrDefault() == 0 && (
            import.HealthInsuranceCompany.InsurerPhone.GetValueOrDefault() > 0
          || !IsEmpty(import.HealthInsuranceCompany.InsurerPhoneExt)))
        {
          var field =
            GetField(export.HealthInsuranceCompany, "insurerPhoneAreaCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "CO0000_PHONE_AREA_CODE_REQD";
          }
        }

        if (import.HealthInsuranceCompany.InsurerPhone.GetValueOrDefault() == 0
          && (
            import.HealthInsuranceCompany.InsurerPhoneAreaCode.
            GetValueOrDefault() > 0 || !
          IsEmpty(import.HealthInsuranceCompany.InsurerPhoneExt)))
        {
          var field = GetField(export.HealthInsuranceCompany, "insurerPhone");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_PHONE_NO_REQD";
          }
        }

        if (import.HealthInsuranceCompany.InsurerFax.GetValueOrDefault() > 0
          && import
          .HealthInsuranceCompany.InsurerFaxAreaCode.GetValueOrDefault() == 0)
        {
          var field =
            GetField(export.HealthInsuranceCompany, "insurerFaxAreaCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_FAX_AREA_CODE_REQD";
          }
        }

        if (import.HealthInsuranceCompany.InsurerFaxAreaCode.
          GetValueOrDefault() > 0 && import
          .HealthInsuranceCompany.InsurerFax.GetValueOrDefault() == 0)
        {
          var field = GetField(export.HealthInsuranceCompany, "insurerFax");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_FAX_NO_REQD";
          }
        }

        // ***********************************************************************
        // **         Validate start and end dates.
        // ***********************************************************************
        if (Equal(export.HealthInsuranceCompany.StartDate, local.Null1.Date))
        {
          local.HealthInsuranceCompany.StartDate = local.Conversion.Date;
        }

        if (Lt(local.Curr.Date, export.HealthInsuranceCompany.StartDate))
        {
          var field = GetField(export.HealthInsuranceCompany, "startDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_NO_FUTURE_DATE_ALLOW";
          }
        }

        if (Lt(local.Null1.Date, export.HealthInsuranceCompany.EndDate))
        {
          var field = GetField(export.HealthInsuranceCompany, "endDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_END_DATE_INVALID_FOR_ADD";
          }
        }

        // ---------------------------------------------
        // The address is mandatory.
        // ---------------------------------------------
        if (IsEmpty(import.HealthInsuranceCompanyAddress.Street1) && IsEmpty
          (import.HealthInsuranceCompanyAddress.Street2) && IsEmpty
          (import.HealthInsuranceCompanyAddress.City) && IsEmpty
          (import.HealthInsuranceCompanyAddress.State) && IsEmpty
          (import.HealthInsuranceCompanyAddress.ZipCode5) && IsEmpty
          (import.HealthInsuranceCompanyAddress.ZipCode4))
        {
          var field1 = GetField(export.HealthInsuranceCompany, "carrierCode");

          field1.Error = true;

          var field2 =
            GetField(export.HealthInsuranceCompany, "insurancePolicyCarrier");

          field2.Error = true;

          var field3 =
            GetField(export.HealthInsuranceCompanyAddress, "street1");

          field3.Error = true;

          var field4 = GetField(export.HealthInsuranceCompanyAddress, "city");

          field4.Error = true;

          var field5 = GetField(export.HealthInsuranceCompanyAddress, "state");

          field5.Error = true;

          var field6 =
            GetField(export.HealthInsuranceCompanyAddress, "zipCode5");

          field6.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else
        {
          // ---------------------------------------------
          // Some data has been keyed in the address
          // fields. Do the edit check for address here.
          // ---------------------------------------------
          if (IsEmpty(import.HealthInsuranceCompanyAddress.Country))
          {
            local.HealthInsuranceCompanyAddress.Country = "US";
          }

          if (IsEmpty(import.HealthInsuranceCompanyAddress.ZipCode5))
          {
            var field =
              GetField(export.HealthInsuranceCompanyAddress, "zipCode5");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            do
            {
              ++local.CheckZip.Count;
              local.CheckZip.Flag =
                Substring(export.HealthInsuranceCompanyAddress.ZipCode5,
                local.CheckZip.Count, 1);

              if (AsChar(local.CheckZip.Flag) < '0' || AsChar
                (local.CheckZip.Flag) > '9')
              {
                var field =
                  GetField(export.HealthInsuranceCompanyAddress, "zipCode5");

                field.Error = true;

                ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
              }
            }
            while(local.CheckZip.Count < 5);
          }

          if (Verify(export.HealthInsuranceCompanyAddress.ZipCode4, "0123456789")
            != 0 && Length
            (TrimEnd(export.HealthInsuranceCompanyAddress.ZipCode4)) > 0)
          {
            var field =
              GetField(export.HealthInsuranceCompanyAddress, "zipCode4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";
          }

          if (Length(TrimEnd(export.HealthInsuranceCompanyAddress.ZipCode4)) < 4
            && Length
            (TrimEnd(export.HealthInsuranceCompanyAddress.ZipCode4)) > 0)
          {
            var field =
              GetField(export.HealthInsuranceCompanyAddress, "zipCode4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";
          }

          if (IsEmpty(import.HealthInsuranceCompanyAddress.State))
          {
            var field = GetField(export.HealthInsuranceCompanyAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            local.CodeValue.Cdvalue =
              export.HealthInsuranceCompanyAddress.State ?? Spaces(10);
            UseCabValidateCodeValue();

            if (local.ReturnCode.Count != 0)
            {
              var field =
                GetField(export.HealthInsuranceCompanyAddress, "state");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }

          if (IsEmpty(import.HealthInsuranceCompanyAddress.City))
          {
            var field = GetField(export.HealthInsuranceCompanyAddress, "city");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.HealthInsuranceCompanyAddress.Street1))
          {
            var field =
              GetField(export.HealthInsuranceCompanyAddress, "street1");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(export.HealthInsuranceCompany.EndDate, local.Null1.Date))
        {
          local.HealthInsuranceCompany.EndDate = local.MaxDate.ExpirationDate;
        }

        // ---------------------------------------------
        // Insert the USE statement here to call the
        // CREATE action block.
        // ---------------------------------------------
        UseOeHicoAddHealthInsCompany();

        if (IsExitState("OE0000_CARRIER_CODE_AE"))
        {
          var field = GetField(export.HealthInsuranceCompany, "carrierCode");

          field.Error = true;
        }

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          export.HiddenHealthInsuranceCompany.Assign(
            export.HealthInsuranceCompany);
          export.HiddenHealthInsuranceCompanyAddress.Assign(
            export.HealthInsuranceCompanyAddress);
        }

        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Verify that all mandatory fields for a
        // display have been entered.
        // ---------------------------------------------
        // ---------------------------------------------
        // In order to display a record the user must
        // input either a carrier code or a combination
        // of name, street address line1, city and
        // state.  Must allow for a selection being made
        // from a list screen and an identifier being
        // carrier across the views.
        // ---------------------------------------------
        if (IsEmpty(export.HealthInsuranceCompany.CarrierCode))
        {
          var field = GetField(export.HealthInsuranceCompany, "carrierCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (!Equal(import.HiddenHealthInsuranceCompany.CarrierCode,
          export.HealthInsuranceCompany.CarrierCode) && local
          .ReturnCode.Count == 0)
        {
          local.HealthInsuranceCompany.Identifier = 0;
        }

        // ---------------------------------------------
        // Insert the USE statement here that calls the
        // READ action block.
        // ---------------------------------------------
        UseOeReadHealthInsCompany();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          export.HiddenHealthInsuranceCompany.Assign(
            export.HealthInsuranceCompany);
          export.HiddenHealthInsuranceCompanyAddress.Assign(
            export.HealthInsuranceCompanyAddress);
          export.WorkPromptCarrierCode.TextLength01 = "";
          export.PromptState.SelectChar = "";
        }
        else if (export.HealthInsuranceCompany.Identifier > 0)
        {
          export.HiddenHealthInsuranceCompany.Assign(
            export.HealthInsuranceCompany);
          export.WorkPromptCarrierCode.TextLength01 = "";
          export.PromptState.SelectChar = "";
        }
        else
        {
          var field = GetField(export.HealthInsuranceCompany, "carrierCode");

          field.Error = true;

          export.HealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          export.HiddenHealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          MoveHealthInsuranceCompanyAddress(local.
            RefreshHealthInsuranceCompanyAddress,
            export.HealthInsuranceCompanyAddress);
          MoveHealthInsuranceCompanyAddress(local.
            RefreshHealthInsuranceCompanyAddress,
            export.HiddenHealthInsuranceCompanyAddress);
          export.HealthInsuranceCompany.CarrierCode =
            import.HealthInsuranceCompany.CarrierCode ?? "";
          export.WorkPromptCarrierCode.TextLength01 = "";
          export.PromptState.SelectChar = "";
        }

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Verify that a display has been performed
        // before the update can take place.
        // ---------------------------------------------
        if (import.HiddenHealthInsuranceCompany.Identifier == 0)
        {
          ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";

        if (IsEmpty(import.HealthInsuranceCompany.CarrierCode))
        {
          var field = GetField(export.HealthInsuranceCompany, "carrierCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (IsEmpty(import.HealthInsuranceCompany.InsurancePolicyCarrier))
        {
          var field =
            GetField(export.HealthInsuranceCompany, "insurancePolicyCarrier");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        // ---------------------------------------------
        // Validate phone and fax number
        // ---------------------------------------------
        if (import.HealthInsuranceCompany.InsurerPhoneAreaCode.
          GetValueOrDefault() == 0 && (
            import.HealthInsuranceCompany.InsurerPhone.GetValueOrDefault() > 0
          || !IsEmpty(import.HealthInsuranceCompany.InsurerPhoneExt)))
        {
          var field =
            GetField(export.HealthInsuranceCompany, "insurerPhoneAreaCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "CO0000_PHONE_AREA_CODE_REQD";
          }
        }

        if (import.HealthInsuranceCompany.InsurerPhone.GetValueOrDefault() == 0
          && (
            import.HealthInsuranceCompany.InsurerPhoneAreaCode.
            GetValueOrDefault() > 0 || !
          IsEmpty(import.HealthInsuranceCompany.InsurerPhoneExt)))
        {
          var field = GetField(export.HealthInsuranceCompany, "insurerPhone");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_PHONE_NO_REQD";
          }
        }

        if (import.HealthInsuranceCompany.InsurerFax.GetValueOrDefault() > 0
          && import
          .HealthInsuranceCompany.InsurerFaxAreaCode.GetValueOrDefault() == 0)
        {
          var field =
            GetField(export.HealthInsuranceCompany, "insurerFaxAreaCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_FAX_AREA_CODE_REQD";
          }
        }

        if (import.HealthInsuranceCompany.InsurerFaxAreaCode.
          GetValueOrDefault() > 0 && import
          .HealthInsuranceCompany.InsurerFax.GetValueOrDefault() == 0)
        {
          var field = GetField(export.HealthInsuranceCompany, "insurerFax");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_FAX_NO_REQD";
          }
        }

        // ***********************************************************************
        // **         Validate start and end dates.
        // ***********************************************************************
        if (Lt(local.Curr.Date, export.HealthInsuranceCompany.StartDate))
        {
          var field = GetField(export.HealthInsuranceCompany, "startDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_NO_FUTURE_DATE_ALLOW";
          }
        }

        if (!Equal(export.HealthInsuranceCompany.EndDate,
          export.HiddenHealthInsuranceCompany.EndDate))
        {
          if (!Equal(export.HealthInsuranceCompany.EndDate,
            local.MaxDate.ExpirationDate))
          {
            var field = GetField(export.HealthInsuranceCompany, "endDate");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0000_END_DATE_INVALID_FOR_UPD";
            }
          }
        }

        // ---------------------------------------------
        // If an address existed before, it cannot be
        // spaced out.
        // ---------------------------------------------
        if (!IsEmpty(import.HiddenHealthInsuranceCompanyAddress.Street1) && IsEmpty
          (import.HealthInsuranceCompanyAddress.Street1) && IsEmpty
          (import.HealthInsuranceCompanyAddress.Street2) && IsEmpty
          (import.HealthInsuranceCompanyAddress.City) && IsEmpty
          (import.HealthInsuranceCompanyAddress.State) && IsEmpty
          (import.HealthInsuranceCompanyAddress.Country) && IsEmpty
          (import.HealthInsuranceCompanyAddress.ZipCode5))
        {
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_ADDR_CANNOT_BE_DELETED";
          }

          return;
        }

        if (!IsEmpty(import.HealthInsuranceCompanyAddress.Street1) || !
          IsEmpty(import.HealthInsuranceCompanyAddress.Street2) || !
          IsEmpty(import.HealthInsuranceCompanyAddress.City) || !
          IsEmpty(import.HealthInsuranceCompanyAddress.State) || !
          IsEmpty(import.HealthInsuranceCompanyAddress.ZipCode5) || !
          IsEmpty(import.HealthInsuranceCompanyAddress.ZipCode4))
        {
          // ---------------------------------------------
          // Some data has been keyed in the address
          // fields. Do the edit check for address here.
          // ---------------------------------------------
          if (IsEmpty(import.HealthInsuranceCompanyAddress.Country))
          {
            local.HealthInsuranceCompanyAddress.Country = "US";
          }

          if (IsEmpty(import.HealthInsuranceCompanyAddress.ZipCode5))
          {
            var field =
              GetField(export.HealthInsuranceCompanyAddress, "zipCode5");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            do
            {
              ++local.CheckZip.Count;
              local.CheckZip.Flag =
                Substring(export.HealthInsuranceCompanyAddress.ZipCode5,
                local.CheckZip.Count, 1);

              if (AsChar(local.CheckZip.Flag) < '0' || AsChar
                (local.CheckZip.Flag) > '9')
              {
                var field =
                  GetField(export.HealthInsuranceCompanyAddress, "zipCode5");

                field.Error = true;

                ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
              }
            }
            while(local.CheckZip.Count < 5);
          }

          if (Verify(export.HealthInsuranceCompanyAddress.ZipCode4, "0123456789")
            != 0 && Length
            (TrimEnd(export.HealthInsuranceCompanyAddress.ZipCode4)) > 0)
          {
            var field =
              GetField(export.HealthInsuranceCompanyAddress, "zipCode4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";
          }

          if (Length(TrimEnd(export.HealthInsuranceCompanyAddress.ZipCode4)) < 4
            && Length
            (TrimEnd(export.HealthInsuranceCompanyAddress.ZipCode4)) > 0)
          {
            var field =
              GetField(export.HealthInsuranceCompanyAddress, "zipCode4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";
          }

          if (IsEmpty(import.HealthInsuranceCompanyAddress.State))
          {
            var field = GetField(export.HealthInsuranceCompanyAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            local.CodeValue.Cdvalue =
              export.HealthInsuranceCompanyAddress.State ?? Spaces(10);
            UseCabValidateCodeValue();

            if (local.ReturnCode.Count != 0)
            {
              var field =
                GetField(export.HealthInsuranceCompanyAddress, "state");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }

          if (IsEmpty(import.HealthInsuranceCompanyAddress.City))
          {
            var field = GetField(export.HealthInsuranceCompanyAddress, "city");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.HealthInsuranceCompanyAddress.Street1))
          {
            var field =
              GetField(export.HealthInsuranceCompanyAddress, "street1");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(export.HealthInsuranceCompany.EndDate, local.Null1.Date))
        {
          local.HealthInsuranceCompany.EndDate = local.MaxDate.ExpirationDate;
        }

        // ---------------------------------------------
        // Insert the USE statement here to call the UPDATE action block.
        // ---------------------------------------------
        UseOeUpdateHealthInsCompany();

        if (IsExitState("OE0000_CARRIER_CODE_AE"))
        {
          var field = GetField(export.HealthInsuranceCompany, "carrierCode");

          field.Error = true;
        }

        if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          export.HiddenHealthInsuranceCompany.Assign(
            export.HealthInsuranceCompany);
          export.HiddenHealthInsuranceCompanyAddress.Assign(
            export.HealthInsuranceCompanyAddress);
        }

        break;
      case "BYPASS":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (Equal(export.HealthInsuranceCompany.EndDate, new DateTime(2099, 12, 31)))
      
    {
      export.HealthInsuranceCompany.EndDate = null;
    }

    global.Command = "";
  }

  private static void MoveHealthInsuranceCompanyAddress(
    HealthInsuranceCompanyAddress source, HealthInsuranceCompanyAddress target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
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

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.State.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
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

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
    local.State.CodeName = useExport.State.CodeName;
  }

  private void UseOeHicoAddHealthInsCompany()
  {
    var useImport = new OeHicoAddHealthInsCompany.Import();
    var useExport = new OeHicoAddHealthInsCompany.Export();

    useImport.HealthInsuranceCompanyAddress.Assign(
      local.HealthInsuranceCompanyAddress);
    useImport.HealthInsuranceCompany.Assign(local.HealthInsuranceCompany);

    Call(OeHicoAddHealthInsCompany.Execute, useImport, useExport);

    export.HealthInsuranceCompanyAddress.Assign(
      useExport.HealthInsuranceCompanyAddress);
    export.HealthInsuranceCompany.Assign(useExport.HealthInsuranceCompany);
  }

  private void UseOeReadHealthInsCompany()
  {
    var useImport = new OeReadHealthInsCompany.Import();
    var useExport = new OeReadHealthInsCompany.Export();

    useImport.HealthInsuranceCompanyAddress.Assign(
      local.HealthInsuranceCompanyAddress);
    useImport.HealthInsuranceCompany.Assign(local.HealthInsuranceCompany);

    Call(OeReadHealthInsCompany.Execute, useImport, useExport);

    export.HealthInsuranceCompanyAddress.Assign(
      useExport.HealthInsuranceCompanyAddress);
    export.HealthInsuranceCompany.Assign(useExport.HealthInsuranceCompany);
  }

  private void UseOeUpdateHealthInsCompany()
  {
    var useImport = new OeUpdateHealthInsCompany.Import();
    var useExport = new OeUpdateHealthInsCompany.Export();

    useImport.HealthInsuranceCompanyAddress.Assign(
      local.HealthInsuranceCompanyAddress);
    useImport.HealthInsuranceCompany.Assign(local.HealthInsuranceCompany);

    Call(OeUpdateHealthInsCompany.Execute, useImport, useExport);

    export.HealthInsuranceCompanyAddress.Assign(
      useExport.HealthInsuranceCompanyAddress);
    export.HealthInsuranceCompany.Assign(useExport.HealthInsuranceCompany);
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
    /// <summary>
    /// A value of HiclSelectionCount.
    /// </summary>
    [JsonPropertyName("hiclSelectionCount")]
    public Common HiclSelectionCount
    {
      get => hiclSelectionCount ??= new();
      set => hiclSelectionCount = value;
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
    /// A value of SelectedState.
    /// </summary>
    [JsonPropertyName("selectedState")]
    public CodeValue SelectedState
    {
      get => selectedState ??= new();
      set => selectedState = value;
    }

    /// <summary>
    /// A value of PromptHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("promptHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress PromptHealthInsuranceCompanyAddress
    {
      get => promptHealthInsuranceCompanyAddress ??= new();
      set => promptHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of PromptHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("promptHealthInsuranceCompany")]
    public HealthInsuranceCompany PromptHealthInsuranceCompany
    {
      get => promptHealthInsuranceCompany ??= new();
      set => promptHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of WorkPromptCarrierCode.
    /// </summary>
    [JsonPropertyName("workPromptCarrierCode")]
    public AaWork WorkPromptCarrierCode
    {
      get => workPromptCarrierCode ??= new();
      set => workPromptCarrierCode = value;
    }

    /// <summary>
    /// A value of HiddenHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("hiddenHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HiddenHealthInsuranceCompanyAddress
    {
      get => hiddenHealthInsuranceCompanyAddress ??= new();
      set => hiddenHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of HiddenHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("hiddenHealthInsuranceCompany")]
    public HealthInsuranceCompany HiddenHealthInsuranceCompany
    {
      get => hiddenHealthInsuranceCompany ??= new();
      set => hiddenHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
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
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    private Common hiclSelectionCount;
    private Common promptState;
    private CodeValue selectedState;
    private HealthInsuranceCompanyAddress promptHealthInsuranceCompanyAddress;
    private HealthInsuranceCompany promptHealthInsuranceCompany;
    private AaWork workPromptCarrierCode;
    private HealthInsuranceCompanyAddress hiddenHealthInsuranceCompanyAddress;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompany hiddenHealthInsuranceCompany;
    private HealthInsuranceCompany healthInsuranceCompany;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Case1 hiddenCase;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SelectedState.
    /// </summary>
    [JsonPropertyName("selectedState")]
    public CodeValue SelectedState
    {
      get => selectedState ??= new();
      set => selectedState = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of WorkPromptCarrierCode.
    /// </summary>
    [JsonPropertyName("workPromptCarrierCode")]
    public AaWork WorkPromptCarrierCode
    {
      get => workPromptCarrierCode ??= new();
      set => workPromptCarrierCode = value;
    }

    /// <summary>
    /// A value of HiddenHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("hiddenHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HiddenHealthInsuranceCompanyAddress
    {
      get => hiddenHealthInsuranceCompanyAddress ??= new();
      set => hiddenHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of HiddenHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("hiddenHealthInsuranceCompany")]
    public HealthInsuranceCompany HiddenHealthInsuranceCompany
    {
      get => hiddenHealthInsuranceCompany ??= new();
      set => hiddenHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
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
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    private CodeValue selectedState;
    private Code state;
    private AaWork workPromptCarrierCode;
    private HealthInsuranceCompanyAddress hiddenHealthInsuranceCompanyAddress;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompany hiddenHealthInsuranceCompany;
    private HealthInsuranceCompany healthInsuranceCompany;
    private Common promptState;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Case1 hiddenCase;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Curr.
    /// </summary>
    [JsonPropertyName("curr")]
    public DateWorkArea Curr
    {
      get => curr ??= new();
      set => curr = value;
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
    /// A value of Conversion.
    /// </summary>
    [JsonPropertyName("conversion")]
    public DateWorkArea Conversion
    {
      get => conversion ??= new();
      set => conversion = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public Common CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of RefreshHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("refreshHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress RefreshHealthInsuranceCompanyAddress
    {
      get => refreshHealthInsuranceCompanyAddress ??= new();
      set => refreshHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of RefreshHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("refreshHealthInsuranceCompany")]
    public HealthInsuranceCompany RefreshHealthInsuranceCompany
    {
      get => refreshHealthInsuranceCompany ??= new();
      set => refreshHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
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

    private DateWorkArea curr;
    private DateWorkArea null1;
    private DateWorkArea conversion;
    private Code maxDate;
    private Common checkZip;
    private Common returnCode;
    private CodeValue codeValue;
    private Code state;
    private HealthInsuranceCompanyAddress refreshHealthInsuranceCompanyAddress;
    private HealthInsuranceCompany refreshHealthInsuranceCompany;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompany healthInsuranceCompany;
    private TextWorkArea textWorkArea;
  }
#endregion
}
