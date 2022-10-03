// Program: OE_HIAV_AVAILABLE_INS_BY_PERSON, ID: 371857344, model: 746.
// Short name: SWEHIAVP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_HIAV_AVAILABLE_INS_BY_PERSON.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeHiavAvailableInsByPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HIAV_AVAILABLE_INS_BY_PERSON program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHiavAvailableInsByPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHiavAvailableInsByPerson.
  /// </summary>
  public OeHiavAvailableInsByPerson(IContext context, Import import,
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
    // C. Chhun	01/05/99	Initial Code
    // C. Scroggins    04/24/00        Modifications for family violence.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    // Keep these three views all the time
    // *****************************************************************
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Import Views to Export Views
    // ---------------------------------------------
    export.CsePersonPrompt.SelectChar = import.CsePersonPrompt.SelectChar;
    export.StatePrompt.SelectChar = import.StatePrompt.SelectChar;
    export.EmpStatePrompt.SelectChar = import.EmpStatePrompt.SelectChar;
    export.CostPrompt.SelectChar = import.CostPrompt.SelectChar;
    export.Cost.Description = import.Cost.Description;
    export.HealthInsuranceAvailability.
      Assign(import.HealthInsuranceAvailability);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.ScrollingAttributes.Assign(import.ScrollingAttributes);

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenNextTranInfo.CsePersonNumber =
      import.HiddenNextTranInfo.CsePersonNumber;
    export.HiddenHealthInsuranceAvailability.Assign(
      import.HiddenHealthInsuranceAvailability);

    if (IsEmpty(import.HealthInsuranceAvailability.CostFrequency))
    {
      export.Cost.Description = Spaces(CodeValue.Description_MaxLength);
    }

    if (Equal(export.HealthInsuranceAvailability.VerifiedDate, new DateTime(1,
      1, 1)))
    {
      export.HealthInsuranceAvailability.VerifiedDate =
        local.Initialized.VerifiedDate;
    }

    if (Equal(export.HealthInsuranceAvailability.EndDate, new DateTime(1, 1, 1)))
      
    {
      export.HealthInsuranceAvailability.EndDate =
        local.Initialized.VerifiedDate;
    }

    if (!IsEmpty(export.CsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.HiddenNextTranInfo.CsePersonNumber = export.CsePerson.Number;
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

      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumber))
      {
        export.CsePerson.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
          (10);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number))
      {
        export.CsePersonsWorkSet.Assign(import.SelectedCsePersonsWorkSet);
        export.CsePerson.Number = import.SelectedCsePersonsWorkSet.Number;
      }

      export.CsePersonPrompt.SelectChar = "";

      // ****************************************************************
      // Make sure selected prompt fields remain highlight
      // ****************************************************************
      if (!IsEmpty(export.StatePrompt.SelectChar))
      {
        var field = GetField(export.StatePrompt, "selectChar");

        field.Error = true;
      }

      if (!IsEmpty(export.CostPrompt.SelectChar))
      {
        var field = GetField(export.CostPrompt, "selectChar");

        field.Error = true;
      }

      if (!IsEmpty(export.EmpStatePrompt.SelectChar))
      {
        var field = GetField(export.EmpStatePrompt, "selectChar");

        field.Error = true;
      }

      global.Command = "DISPLAY";
    }

    // ************************************************
    // *Security
    // 
    // *
    // ************************************************
    if (Equal(global.Command, "ENTER") || Equal(global.Command, "RETNAME") || Equal
      (global.Command, "RETCDVL") || Equal(global.Command, "RETHIPH") || Equal
      (global.Command, "HIPH"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    UseOeCabSetMnemonics();

    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.CsePerson.Number))
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

        var field = GetField(export.CsePerson, "number");

        field.Error = true;
      }

      if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";
        }

        var field = GetField(export.CsePerson, "number");

        field.Error = true;
      }

      if (IsEmpty(export.HealthInsuranceAvailability.InsuranceName))
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        var field =
          GetField(export.HealthInsuranceAvailability, "insuranceName");

        field.Error = true;
      }

      if (!IsEmpty(export.HealthInsuranceAvailability.State))
      {
        local.CodeValue.Cdvalue = export.HealthInsuranceAvailability.State ?? Spaces
          (10);
        local.Code.CodeName = local.State.CodeName;
        UseCabValidateCodeValue1();

        if (local.ReturnCode.Count != 0)
        {
          var field = GetField(export.HealthInsuranceAvailability, "state");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }
      }

      if (!IsEmpty(export.HealthInsuranceAvailability.Zip5))
      {
        do
        {
          ++local.ZipCheck.Count;
          local.ZipCheck.Flag =
            Substring(export.HealthInsuranceAvailability.Zip5,
            local.ZipCheck.Count, 1);

          if (AsChar(local.ZipCheck.Flag) < '0' || AsChar
            (local.ZipCheck.Flag) > '9')
          {
            var field = GetField(export.HealthInsuranceAvailability, "zip5");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
            }

            break;
          }
        }
        while(local.ZipCheck.Count < 5);
      }

      if (!IsEmpty(export.HealthInsuranceAvailability.Zip4))
      {
        if (IsEmpty(export.HealthInsuranceAvailability.Zip5))
        {
          var field = GetField(export.HealthInsuranceAvailability, "zip5");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        local.ZipCheck.Count = 0;

        do
        {
          ++local.ZipCheck.Count;
          local.ZipCheck.Flag =
            Substring(export.HealthInsuranceAvailability.Zip4,
            local.ZipCheck.Count, 1);

          if (AsChar(local.ZipCheck.Flag) < '0' || AsChar
            (local.ZipCheck.Flag) > '9')
          {
            var field = GetField(export.HealthInsuranceAvailability, "zip4");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
            }

            break;
          }
        }
        while(local.ZipCheck.Count < 4);
      }

      if (!IsEmpty(export.HealthInsuranceAvailability.CostFrequency) && export
        .HealthInsuranceAvailability.Cost.GetValueOrDefault() == 0)
      {
        var field = GetField(export.HealthInsuranceAvailability, "cost");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }

      if (IsEmpty(export.HealthInsuranceAvailability.CostFrequency) && export
        .HealthInsuranceAvailability.Cost.GetValueOrDefault() > 0)
      {
        var field =
          GetField(export.HealthInsuranceAvailability, "costFrequency");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }

      if (!IsEmpty(export.HealthInsuranceAvailability.CostFrequency))
      {
        local.CodeValue.Cdvalue =
          export.HealthInsuranceAvailability.CostFrequency ?? Spaces(10);
        local.Code.CodeName = "HEALTH INSURANCE COST FREQUENCY";
        UseCabValidateCodeValue1();

        if (local.ReturnCode.Count != 0)
        {
          var field =
            GetField(export.HealthInsuranceAvailability, "costFrequency");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0193_INVALID_COST_FREQUENCY";
          }
        }
      }

      if (!Lt(local.Initialized.VerifiedDate,
        export.HealthInsuranceAvailability.VerifiedDate))
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        var field =
          GetField(export.HealthInsuranceAvailability, "verifiedDate");

        field.Error = true;
      }
      else if (Lt(Now().Date, export.HealthInsuranceAvailability.VerifiedDate))
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        var field =
          GetField(export.HealthInsuranceAvailability, "verifiedDate");

        field.Error = true;
      }
      else if (Equal(export.HealthInsuranceAvailability.VerifiedDate,
        export.LocalMax.ExpirationDate))
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_DATE";
        }

        var field =
          GetField(export.HealthInsuranceAvailability, "verifiedDate");

        field.Error = true;
      }

      if (Equal(export.HealthInsuranceAvailability.EndDate,
        export.LocalMax.ExpirationDate))
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_DATE";
        }

        var field = GetField(export.HealthInsuranceAvailability, "endDate");

        field.Error = true;
      }

      if (Lt(export.HealthInsuranceAvailability.EndDate,
        export.HealthInsuranceAvailability.VerifiedDate) && Lt
        (local.Initialized.EndDate, export.HealthInsuranceAvailability.EndDate))
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "OE0192_VERIFIED_DATE_END_DATE";
        }

        var field =
          GetField(export.HealthInsuranceAvailability, "verifiedDate");

        field.Error = true;
      }

      if (IsEmpty(export.HealthInsuranceAvailability.EmployerName) && (
        !IsEmpty(export.HealthInsuranceAvailability.EmpCity) || export
        .HealthInsuranceAvailability.EmpPhoneAreaCode > 0 || export
        .HealthInsuranceAvailability.EmpPhoneNo.GetValueOrDefault() > 0 || !
        IsEmpty(export.HealthInsuranceAvailability.EmpState) || !
        IsEmpty(export.HealthInsuranceAvailability.EmpStreet1) || !
        IsEmpty(export.HealthInsuranceAvailability.EmpStreet2) || !
        IsEmpty(export.HealthInsuranceAvailability.EmpZip4) || !
        IsEmpty(export.HealthInsuranceAvailability.EmpZip5)))
      {
        var field =
          GetField(export.HealthInsuranceAvailability, "employerName");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }

      if (!IsEmpty(export.HealthInsuranceAvailability.EmpState))
      {
        local.CodeValue.Cdvalue =
          export.HealthInsuranceAvailability.EmpState ?? Spaces(10);
        local.Code.CodeName = local.State.CodeName;
        UseCabValidateCodeValue1();

        if (local.ReturnCode.Count != 0)
        {
          var field = GetField(export.HealthInsuranceAvailability, "empState");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }
      }

      if (!IsEmpty(export.HealthInsuranceAvailability.EmpZip5))
      {
        local.ZipCheck.Count = 0;

        do
        {
          ++local.ZipCheck.Count;
          local.ZipCheck.Flag =
            Substring(export.HealthInsuranceAvailability.EmpZip5,
            local.ZipCheck.Count, 1);

          if (AsChar(local.ZipCheck.Flag) < '0' || AsChar
            (local.ZipCheck.Flag) > '9')
          {
            var field = GetField(export.HealthInsuranceAvailability, "empZip5");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
            }

            break;
          }
        }
        while(local.ZipCheck.Count < 5);
      }

      if (!IsEmpty(export.HealthInsuranceAvailability.EmpZip4))
      {
        if (IsEmpty(export.HealthInsuranceAvailability.EmpZip5))
        {
          var field = GetField(export.HealthInsuranceAvailability, "empZip5");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        local.ZipCheck.Count = 0;

        do
        {
          ++local.ZipCheck.Count;
          local.ZipCheck.Flag =
            Substring(export.HealthInsuranceAvailability.EmpZip4,
            local.ZipCheck.Count, 1);

          if (AsChar(local.ZipCheck.Flag) < '0' || AsChar
            (local.ZipCheck.Flag) > '9')
          {
            var field = GetField(export.HealthInsuranceAvailability, "empZip4");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
            }

            break;
          }
        }
        while(local.ZipCheck.Count < 4);
      }

      if (export.HealthInsuranceAvailability.EmpPhoneAreaCode == 0 && export
        .HealthInsuranceAvailability.EmpPhoneNo.GetValueOrDefault() > 0)
      {
        var field =
          GetField(export.HealthInsuranceAvailability, "empPhoneAreaCode");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }

      if (export.HealthInsuranceAvailability.EmpPhoneAreaCode > 0 && export
        .HealthInsuranceAvailability.EmpPhoneNo.GetValueOrDefault() == 0)
      {
        var field = GetField(export.HealthInsuranceAvailability, "empPhoneNo");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "RETCDVL":
        if (AsChar(export.StatePrompt.SelectChar) == 'S')
        {
          export.StatePrompt.SelectChar = "";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            export.HealthInsuranceAvailability.State =
              import.SelectedCodeValue.Cdvalue;

            var field = GetField(export.HealthInsuranceAvailability, "zip5");

            field.Protected = false;
            field.Focused = true;
          }
        }
        else if (AsChar(export.CostPrompt.SelectChar) == 'S')
        {
          export.CostPrompt.SelectChar = "";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            export.HealthInsuranceAvailability.CostFrequency =
              import.SelectedCodeValue.Cdvalue;
            export.Cost.Description = import.SelectedCodeValue.Description;
          }

          var field =
            GetField(export.HealthInsuranceAvailability, "verifiedDate");

          field.Protected = false;
          field.Focused = true;
        }
        else if (AsChar(export.EmpStatePrompt.SelectChar) == 'S')
        {
          export.EmpStatePrompt.SelectChar = "";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            export.HealthInsuranceAvailability.EmpState =
              import.SelectedCodeValue.Cdvalue;
          }

          var field = GetField(export.HealthInsuranceAvailability, "empZip5");

          field.Protected = false;
          field.Focused = true;
        }

        if (AsChar(export.StatePrompt.SelectChar) == 'S')
        {
          var field = GetField(export.StatePrompt, "selectChar");

          field.Error = true;
        }

        if (AsChar(export.CostPrompt.SelectChar) == 'S')
        {
          var field = GetField(export.CostPrompt, "selectChar");

          field.Error = true;
        }

        if (AsChar(export.EmpStatePrompt.SelectChar) == 'S')
        {
          var field = GetField(export.EmpStatePrompt, "selectChar");

          field.Error = true;
        }

        break;
      case "HIPH":
        export.Selected.InsuranceGroupNumber =
          export.HealthInsuranceAvailability.InsuranceGroupNumber ?? "";
        export.Selected.InsurancePolicyNumber =
          export.HealthInsuranceAvailability.InsurancePolicyNumber ?? "";
        ExitState = "ECO_LNK_TO_HIPH";

        break;
      case "LIST":
        if (!IsEmpty(export.CsePersonPrompt.SelectChar) && AsChar
          (export.CsePersonPrompt.SelectChar) != 'S')
        {
          var field = GetField(export.CsePersonPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (!IsEmpty(export.StatePrompt.SelectChar) && AsChar
          (export.StatePrompt.SelectChar) != 'S')
        {
          var field = GetField(export.StatePrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (!IsEmpty(export.CostPrompt.SelectChar) && AsChar
          (export.CostPrompt.SelectChar) != 'S')
        {
          var field = GetField(export.CostPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (!IsEmpty(export.EmpStatePrompt.SelectChar) && AsChar
          (export.EmpStatePrompt.SelectChar) != 'S')
        {
          var field = GetField(export.EmpStatePrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (IsEmpty(export.CostPrompt.SelectChar) && IsEmpty
          (export.CsePersonPrompt.SelectChar) && IsEmpty
          (export.EmpStatePrompt.SelectChar) && IsEmpty
          (export.StatePrompt.SelectChar))
        {
          var field1 = GetField(export.CsePersonPrompt, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.StatePrompt, "selectChar");

          field2.Error = true;

          var field3 = GetField(export.CostPrompt, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.EmpStatePrompt, "selectChar");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(export.CsePersonPrompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
        }
        else if (AsChar(export.StatePrompt.SelectChar) == 'S')
        {
          export.State.CodeName = local.State.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }
        else if (AsChar(export.CostPrompt.SelectChar) == 'S')
        {
          export.State.CodeName = "HEALTH INSURANCE COST FREQUENCY";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }
        else if (AsChar(export.EmpStatePrompt.SelectChar) == 'S')
        {
          export.State.CodeName = local.State.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }

        break;
      case "CREATE":
        UseOeHiavAvailableInsCreate();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          local.Event1.Flag = "Y";
          export.HiddenHealthInsuranceAvailability.Assign(
            export.HealthInsuranceAvailability);
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }
        else
        {
        }

        break;
      case "NEXT":
        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0169_MILI_DISPLAY";
          }

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        UseOeHiavAvailableInsRead1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HealthInsuranceAvailability.Assign(
            local.HealthInsuranceAvailability);
          export.HiddenHealthInsuranceAvailability.Assign(
            export.HealthInsuranceAvailability);
          export.ScrollingAttributes.MinusFlag = "-";
          export.ScrollingAttributes.PlusFlag = "";

          if (AsChar(local.More.Flag) == 'Y')
          {
            export.ScrollingAttributes.PlusFlag = "+";
          }

          export.Cost.Description = Spaces(CodeValue.Description_MaxLength);

          // ****************************************************************
          // Get description
          // ****************************************************************
          if (!IsEmpty(export.HealthInsuranceAvailability.CostFrequency))
          {
            local.CodeValue.Cdvalue =
              export.HealthInsuranceAvailability.CostFrequency ?? Spaces(10);
            local.Code.CodeName = "HEALTH INSURANCE COST FREQUENCY";
            UseCabValidateCodeValue2();

            if (local.ReturnCode.Count == 0)
            {
              export.Cost.Description = local.CodeValue.Description;
            }
          }

          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else if (IsExitState("OE0189_AVAILABLE_HEALTH_INS_NF"))
        {
          if (export.HealthInsuranceAvailability.InsuranceId > 0)
          {
            ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
          }
          else
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }
        }
        else
        {
        }

        break;
      case "PREV":
        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0169_MILI_DISPLAY";
          }

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        UseOeHiavAvailableInsRead1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HealthInsuranceAvailability.Assign(
            local.HealthInsuranceAvailability);
          export.HiddenHealthInsuranceAvailability.Assign(
            export.HealthInsuranceAvailability);
          export.ScrollingAttributes.PlusFlag = "+";
          export.ScrollingAttributes.MinusFlag = "";

          if (AsChar(local.More.Flag) == 'Y')
          {
            export.ScrollingAttributes.MinusFlag = "-";
          }

          export.Cost.Description = Spaces(CodeValue.Description_MaxLength);

          // ****************************************************************
          // Get description
          // ****************************************************************
          if (!IsEmpty(export.HealthInsuranceAvailability.CostFrequency))
          {
            local.CodeValue.Cdvalue =
              export.HealthInsuranceAvailability.CostFrequency ?? Spaces(10);
            local.Code.CodeName = "HEALTH INSURANCE COST FREQUENCY";
            UseCabValidateCodeValue2();

            if (local.ReturnCode.Count == 0)
            {
              export.Cost.Description = local.CodeValue.Description;
            }
          }

          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else if (IsExitState("OE0189_AVAILABLE_HEALTH_INS_NF"))
        {
          if (export.HealthInsuranceAvailability.InsuranceId > 0)
          {
            ExitState = "ACO_NI0000_TOP_OF_LIST";
          }
          else
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }
        }
        else
        {
        }

        break;
      case "DISPLAY":
        if (IsEmpty(export.CsePerson.Number))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        export.HiddenCsePerson.Number = export.CsePerson.Number;
        export.ScrollingAttributes.PageNumber = 1;
        UseOeCabCheckCaseMember();

        if (!IsEmpty(local.ReturnCode.Flag))
        {
          export.HealthInsuranceAvailability.Assign(local.Initialized);
          export.ScrollingAttributes.MinusFlag = "";
          export.ScrollingAttributes.PlusFlag = "";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          return;
        }

        UseOeHiavAvailableInsRead2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenHealthInsuranceAvailability.Assign(
            export.HealthInsuranceAvailability);
          export.ScrollingAttributes.MinusFlag = "";
          export.ScrollingAttributes.PlusFlag = "";

          if (AsChar(local.More.Flag) == 'Y')
          {
            export.ScrollingAttributes.MinusFlag = "-";
          }

          export.Cost.Description = Spaces(CodeValue.Description_MaxLength);

          // ****************************************************************
          // Get description
          // ****************************************************************
          if (!IsEmpty(export.HealthInsuranceAvailability.CostFrequency))
          {
            local.CodeValue.Cdvalue =
              export.HealthInsuranceAvailability.CostFrequency ?? Spaces(10);
            local.Code.CodeName = "HEALTH INSURANCE COST FREQUENCY";
            UseCabValidateCodeValue2();

            if (local.ReturnCode.Count == 0)
            {
              export.Cost.Description = local.CodeValue.Description;
            }
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }
        else if (IsExitState("OE0189_AVAILABLE_HEALTH_INS_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }
        else
        {
        }

        break;
      case "UPDATE":
        if (Equal(export.HealthInsuranceAvailability.City,
          export.HiddenHealthInsuranceAvailability.City) && export
          .HealthInsuranceAvailability.Cost.GetValueOrDefault() == export
          .HiddenHealthInsuranceAvailability.Cost.GetValueOrDefault() && Equal
          (export.HealthInsuranceAvailability.CostFrequency,
          export.HiddenHealthInsuranceAvailability.CostFrequency) && Equal
          (export.HealthInsuranceAvailability.EmpCity,
          export.HiddenHealthInsuranceAvailability.EmpCity) && export
          .HealthInsuranceAvailability.EmpPhoneAreaCode == export
          .HiddenHealthInsuranceAvailability.EmpPhoneAreaCode && export
          .HealthInsuranceAvailability.EmpPhoneNo.GetValueOrDefault() == export
          .HiddenHealthInsuranceAvailability.EmpPhoneNo.GetValueOrDefault() && Equal
          (export.HealthInsuranceAvailability.EmpState,
          export.HiddenHealthInsuranceAvailability.EmpState) && Equal
          (export.HealthInsuranceAvailability.EmpStreet1,
          export.HiddenHealthInsuranceAvailability.EmpStreet1) && Equal
          (export.HealthInsuranceAvailability.EmpStreet2,
          export.HiddenHealthInsuranceAvailability.EmpStreet2) && Equal
          (export.HealthInsuranceAvailability.EmpZip4,
          export.HiddenHealthInsuranceAvailability.EmpZip4) && Equal
          (export.HealthInsuranceAvailability.EmpZip5,
          export.HiddenHealthInsuranceAvailability.EmpZip5) && Equal
          (export.HealthInsuranceAvailability.EmployerName,
          export.HiddenHealthInsuranceAvailability.EmployerName) && Equal
          (export.HealthInsuranceAvailability.EndDate,
          export.HiddenHealthInsuranceAvailability.EndDate) && Equal
          (export.HealthInsuranceAvailability.InsuranceGroupNumber,
          export.HiddenHealthInsuranceAvailability.InsuranceGroupNumber) && Equal
          (export.HealthInsuranceAvailability.InsuranceName,
          export.HiddenHealthInsuranceAvailability.InsuranceName) && Equal
          (export.HealthInsuranceAvailability.InsurancePolicyNumber,
          export.HiddenHealthInsuranceAvailability.InsurancePolicyNumber) && Equal
          (export.HealthInsuranceAvailability.State,
          export.HiddenHealthInsuranceAvailability.State) && Equal
          (export.HealthInsuranceAvailability.Street1,
          export.HiddenHealthInsuranceAvailability.Street1) && Equal
          (export.HealthInsuranceAvailability.Street2,
          export.HiddenHealthInsuranceAvailability.Street2) && Equal
          (export.HealthInsuranceAvailability.VerifiedDate,
          export.HiddenHealthInsuranceAvailability.VerifiedDate) && Equal
          (export.HealthInsuranceAvailability.Zip4,
          export.HiddenHealthInsuranceAvailability.Zip4) && Equal
          (export.HealthInsuranceAvailability.Zip5,
          export.HiddenHealthInsuranceAvailability.Zip5))
        {
          ExitState = "ACO_NI0000_DATA_WAS_NOT_CHANGED";

          return;
        }

        UseOeHaivAvailableInsUpdate();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          if (!Equal(export.HealthInsuranceAvailability.EndDate,
            export.HiddenHealthInsuranceAvailability.EndDate))
          {
            local.Event1.Flag = "Y";
          }

          export.HiddenHealthInsuranceAvailability.Assign(
            export.HealthInsuranceAvailability);
        }
        else if (IsExitState("OE0189_AVAILABLE_HEALTH_INS_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }
        else
        {
        }

        break;
      case "DELETE":
        if (IsEmpty(export.CsePerson.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }

        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";
          }

          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeHiavAvailableInsDelete();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          export.HealthInsuranceAvailability.Assign(local.Initialized);
          export.HiddenHealthInsuranceAvailability.Assign(local.Initialized);
        }
        else if (IsExitState("OE0189_AVAILABLE_HEALTH_INS_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }
        else
        {
        }

        break;
      case "RETHIPH":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
    }

    if (AsChar(local.Event1.Flag) == 'Y')
    {
      local.Infrastructure.EventId = 80;
      local.Infrastructure.CsePersonNumber = export.CsePerson.Number;
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.ReasonCode = "HINSAVSETUP";
      local.Infrastructure.BusinessObjectCd = "PHI";
      local.Infrastructure.InitiatingStateCode = "KS";
      local.Infrastructure.LastUpdatedBy = global.UserId;
      local.Infrastructure.UserId = global.UserId;
      local.Infrastructure.ReferenceDate = Now().Date;

      if (!Lt(local.Initialized.EndDate,
        export.HealthInsuranceAvailability.EndDate))
      {
        local.Infrastructure.Detail = "HINS AV for CSE person, " + export
          .CsePerson.Number + " on " + NumberToString
          (DateToInt(Now().Date), 8, 8) + " was created.";
      }
      else
      {
        local.Infrastructure.Detail = "HINS AV for CSE person, " + export
          .CsePerson.Number + " on " + NumberToString
          (DateToInt(Now().Date), 8, 8) + " was ended.";
      }

      UseSpCabCreateInfrastructure();
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsePersonNumber = source.CsePersonNumber;
    target.UserId = source.UserId;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
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

  private void UseOeCabCheckCaseMember()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.Work.Flag;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.State.CodeName = useExport.State.CodeName;
    export.LocalMax.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseOeHaivAvailableInsUpdate()
  {
    var useImport = new OeHaivAvailableInsUpdate.Import();
    var useExport = new OeHaivAvailableInsUpdate.Export();

    useImport.HealthInsuranceAvailability.Assign(
      export.HealthInsuranceAvailability);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeHaivAvailableInsUpdate.Execute, useImport, useExport);

    export.HealthInsuranceAvailability.Assign(
      useExport.HealthInsuranceAvailability);
  }

  private void UseOeHiavAvailableInsCreate()
  {
    var useImport = new OeHiavAvailableInsCreate.Import();
    var useExport = new OeHiavAvailableInsCreate.Export();

    useImport.HealthInsuranceAvailability.Assign(
      export.HealthInsuranceAvailability);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeHiavAvailableInsCreate.Execute, useImport, useExport);

    export.HealthInsuranceAvailability.Assign(
      useExport.HealthInsuranceAvailability);
  }

  private void UseOeHiavAvailableInsDelete()
  {
    var useImport = new OeHiavAvailableInsDelete.Import();
    var useExport = new OeHiavAvailableInsDelete.Export();

    useImport.HealthInsuranceAvailability.InsuranceId =
      export.HealthInsuranceAvailability.InsuranceId;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeHiavAvailableInsDelete.Execute, useImport, useExport);
  }

  private void UseOeHiavAvailableInsRead1()
  {
    var useImport = new OeHiavAvailableInsRead.Import();
    var useExport = new OeHiavAvailableInsRead.Export();

    useImport.HealthInsuranceAvailability.InsuranceId =
      export.HealthInsuranceAvailability.InsuranceId;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeHiavAvailableInsRead.Execute, useImport, useExport);

    local.HealthInsuranceAvailability.Assign(
      useExport.HealthInsuranceAvailability);
    local.More.Flag = useExport.More.Flag;
  }

  private void UseOeHiavAvailableInsRead2()
  {
    var useImport = new OeHiavAvailableInsRead.Import();
    var useExport = new OeHiavAvailableInsRead.Export();

    useImport.HealthInsuranceAvailability.InsuranceId =
      local.Starting.InsuranceId;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeHiavAvailableInsRead.Execute, useImport, useExport);

    export.HealthInsuranceAvailability.Assign(
      useExport.HealthInsuranceAvailability);
    local.More.Flag = useExport.More.Flag;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.CsePersonNumber =
      useExport.NextTranInfo.CsePersonNumber;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.CsePersonNumber =
      export.HiddenNextTranInfo.CsePersonNumber;

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

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
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
    /// A value of Cost.
    /// </summary>
    [JsonPropertyName("cost")]
    public CodeValue Cost
    {
      get => cost ??= new();
      set => cost = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of EmpStatePrompt.
    /// </summary>
    [JsonPropertyName("empStatePrompt")]
    public Common EmpStatePrompt
    {
      get => empStatePrompt ??= new();
      set => empStatePrompt = value;
    }

    /// <summary>
    /// A value of CostPrompt.
    /// </summary>
    [JsonPropertyName("costPrompt")]
    public Common CostPrompt
    {
      get => costPrompt ??= new();
      set => costPrompt = value;
    }

    /// <summary>
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Common StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
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
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
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
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
    }

    /// <summary>
    /// A value of HiddenHealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("hiddenHealthInsuranceAvailability")]
    public HealthInsuranceAvailability HiddenHealthInsuranceAvailability
    {
      get => hiddenHealthInsuranceAvailability ??= new();
      set => hiddenHealthInsuranceAvailability = value;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
    }

    private CodeValue cost;
    private ScrollingAttributes scrollingAttributes;
    private Common empStatePrompt;
    private Common costPrompt;
    private Common statePrompt;
    private Common csePersonPrompt;
    private CsePerson csePerson;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private HealthInsuranceAvailability healthInsuranceAvailability;
    private HealthInsuranceAvailability hiddenHealthInsuranceAvailability;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private CodeValue selectedCodeValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LocalMax.
    /// </summary>
    [JsonPropertyName("localMax")]
    public Code LocalMax
    {
      get => localMax ??= new();
      set => localMax = value;
    }

    /// <summary>
    /// A value of Cost.
    /// </summary>
    [JsonPropertyName("cost")]
    public CodeValue Cost
    {
      get => cost ??= new();
      set => cost = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of EmpStatePrompt.
    /// </summary>
    [JsonPropertyName("empStatePrompt")]
    public Common EmpStatePrompt
    {
      get => empStatePrompt ??= new();
      set => empStatePrompt = value;
    }

    /// <summary>
    /// A value of CostPrompt.
    /// </summary>
    [JsonPropertyName("costPrompt")]
    public Common CostPrompt
    {
      get => costPrompt ??= new();
      set => costPrompt = value;
    }

    /// <summary>
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Common StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
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
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
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
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
    }

    /// <summary>
    /// A value of HiddenHealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("hiddenHealthInsuranceAvailability")]
    public HealthInsuranceAvailability HiddenHealthInsuranceAvailability
    {
      get => hiddenHealthInsuranceAvailability ??= new();
      set => hiddenHealthInsuranceAvailability = value;
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
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public HealthInsuranceCoverage Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    private Code localMax;
    private CodeValue cost;
    private ScrollingAttributes scrollingAttributes;
    private Common empStatePrompt;
    private Common costPrompt;
    private Common statePrompt;
    private Common csePersonPrompt;
    private CsePerson csePerson;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private HealthInsuranceAvailability healthInsuranceAvailability;
    private HealthInsuranceAvailability hiddenHealthInsuranceAvailability;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Code state;
    private HealthInsuranceCoverage selected;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Common Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of More.
    /// </summary>
    [JsonPropertyName("more")]
    public Common More
    {
      get => more ??= new();
      set => more = value;
    }

    /// <summary>
    /// A value of ZipCheck.
    /// </summary>
    [JsonPropertyName("zipCheck")]
    public Common ZipCheck
    {
      get => zipCheck ??= new();
      set => zipCheck = value;
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
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public HealthInsuranceAvailability Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public HealthInsuranceAvailability Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private Infrastructure infrastructure;
    private Common event1;
    private Common more;
    private Common zipCheck;
    private Code code;
    private Common returnCode;
    private CodeValue codeValue;
    private HealthInsuranceAvailability healthInsuranceAvailability;
    private HealthInsuranceAvailability starting;
    private TextWorkArea textWorkArea;
    private Code state;
    private HealthInsuranceAvailability initialized;
  }
#endregion
}
