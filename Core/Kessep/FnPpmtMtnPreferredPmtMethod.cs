// Program: FN_PPMT_MTN_PREFERRED_PMT_METHOD, ID: 371881686, model: 746.
// Short name: SWEPPMTP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PPMT_MTN_PREFERRED_PMT_METHOD.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPpmtMtnPreferredPmtMethod: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PPMT_MTN_PREFERRED_PMT_METHOD program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPpmtMtnPreferredPmtMethod(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPpmtMtnPreferredPmtMethod.
  /// </summary>
  public FnPpmtMtnPreferredPmtMethod(IContext context, Import import,
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
    // ************************************************
    // 05/07/97  A Samuels MTW  Development
    // 12/31/98  N Engoor
    // Made Changes to Add, Update and Delete Functionalities. Added flows to 
    // PPLT and to the Menu screen - DMEN.
    // 01/26/99  N Engoor
    // Added code for computing the 9th digit of the routing number using the 
    // ACH algorithm.
    // 11/29/99 - PR# 80797 - Kalpesh Doshi
    // Protected fields must remain protected when PF9 Key is depressed.
    // ************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // Move all IMPORTs to EXPORTs.
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    MovePaymentMethodType(import.PaymentMethodType, export.PaymentMethodType);
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    export.PersonPreferredPaymentMethod.Assign(
      import.PersonPreferredPaymentMethod);
    export.Export9Digit.Text1 = import.Import9Digit.Text1;
    MovePaymentMethodType(import.HiddenPaymentMethodType,
      export.HiddenPaymentMethodType);
    export.HiddenId.Assign(import.HiddenId);
    export.CsePerson.PromptField = import.CsePerson.PromptField;
    export.Type1.PromptField = import.Type1.PromptField;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.Common.Count = 0;
    local.Total.Count = 0;

    if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETPPLT"))
    {
      if (import.PersonPreferredPaymentMethod.SystemGeneratedIdentifier == 0)
      {
        export.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          export.HiddenId.SystemGeneratedIdentifier;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETNAME"))
    {
      if (IsEmpty(import.CsePersonsWorkSet.Number))
      {
        export.CsePersonsWorkSet.Number = export.HiddenCsePerson.Number;
      }

      global.Command = "DISPLAY";
    }

    // -------------
    // If the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate.
    // -------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;

        // -----------------------------------------------------
        // SWSRKXD PR# 80797 - 11/29/99
        // Protected fields must remain protected when PF9 Key is
        // depressed.
        // -----------------------------------------------------
        var field2 = GetField(export.PaymentMethodType, "code");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.CsePersonsWorkSet, "number");

        field3.Color = "cyan";
        field3.Protected = true;

        if (Lt(export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date) &&
          !Equal(export.PersonPreferredPaymentMethod.EffectiveDate, null))
        {
          var field4 =
            GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.PersonPreferredPaymentMethod, "accountType");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 =
            GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.Export9Digit, "text1");

          field8.Color = "cyan";
          field8.Protected = true;

          if (Lt(export.PersonPreferredPaymentMethod.DiscontinueDate, Now().Date)
            && !
            Equal(export.PersonPreferredPaymentMethod.DiscontinueDate, null))
          {
            var field9 =
              GetField(export.PersonPreferredPaymentMethod, "discontinueDate");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.PersonPreferredPaymentMethod, "description");

            field10.Color = "cyan";
            field10.Protected = true;
          }
        }

        if (Equal(export.PaymentMethodType.Code, "WAR"))
        {
          var field4 =
            GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.PersonPreferredPaymentMethod, "accountType");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.Export9Digit, "text1");

          field7.Color = "cyan";
          field7.Protected = true;
        }
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "PPLT"))
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

    // ****************************************************
    // Must display first before maintenance can be performed.
    // ****************************************************
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (!Equal(import.CsePersonsWorkSet.Number, import.HiddenCsePerson.Number))
        
      {
        ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";

        return;
      }
    }

    // *****************************************************
    // Discontinue date must be >= start date.
    // *****************************************************
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD"))
    {
      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      if (IsEmpty(export.PaymentMethodType.Code))
      {
        export.PaymentMethodType.Code = "EFT";
      }

      if (!Equal(export.PaymentMethodType.Code, "EFT"))
      {
        var field = GetField(export.PaymentMethodType, "code");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_CODE";

        return;
      }

      if (AsChar(export.PersonPreferredPaymentMethod.AccountType) != 'C' && AsChar
        (export.PersonPreferredPaymentMethod.AccountType) != 'S')
      {
        var field =
          GetField(export.PersonPreferredPaymentMethod, "accountType");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_CODE";

        return;
      }

      if (Equal(export.PersonPreferredPaymentMethod.EffectiveDate, null))
      {
        export.PersonPreferredPaymentMethod.EffectiveDate = Now().Date;
      }

      if (Equal(export.PersonPreferredPaymentMethod.DiscontinueDate, null))
      {
        export.PersonPreferredPaymentMethod.DiscontinueDate =
          new DateTime(2099, 12, 31);
      }

      if (Equal(export.PaymentMethodType.Code, "EFT"))
      {
        if (IsEmpty(export.PersonPreferredPaymentMethod.AccountType))
        {
          var field =
            GetField(export.PersonPreferredPaymentMethod, "accountType");

          field.Error = true;

          ExitState = "FN0000_FIELDS_REQ_FOR_EFT";
        }

        if (export.PersonPreferredPaymentMethod.AbaRoutingNumber.
          GetValueOrDefault() == 0)
        {
          var field =
            GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");

          field.Error = true;

          ExitState = "FN0000_FIELDS_REQ_FOR_EFT";
        }

        if (IsEmpty(export.PersonPreferredPaymentMethod.DfiAccountNumber))
        {
          var field =
            GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");

          field.Error = true;

          ExitState = "FN0000_FIELDS_REQ_FOR_EFT";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ------------------------------------------
      // N.Engoor - ACH Algorithm to compute the 9th digit of the routing number
      // entered.
      // ------------------------------------------
      local.PassRoutingTextWorkArea.Text8 =
        NumberToString(export.PersonPreferredPaymentMethod.AbaRoutingNumber.
          GetValueOrDefault(), 8, 8);
      local.RoutingTextWorkArea.Text8 =
        NumberToString(export.PersonPreferredPaymentMethod.AbaRoutingNumber.
          GetValueOrDefault(), 8, 8);

      for(local.Common.Count = 1; local.Common.Count <= 8; ++local.Common.Count)
      {
        if (local.Common.Count == 1 || local.Common.Count == 4 || local
          .Common.Count == 7)
        {
          local.Digit.Text1 =
            Substring(local.RoutingTextWorkArea.Text8, local.Common.Count, 1);
          local.Total.Count = (int)(local.Total.Count + StringToNumber
            (local.Digit.Text1) * 3);
        }

        if (local.Common.Count == 2 || local.Common.Count == 5 || local
          .Common.Count == 8)
        {
          local.Digit.Text1 =
            Substring(local.RoutingTextWorkArea.Text8, local.Common.Count, 1);
          local.Total.Count = (int)(local.Total.Count + StringToNumber
            (local.Digit.Text1) * 7);
        }

        if (local.Common.Count == 3 || local.Common.Count == 6)
        {
          local.Digit.Text1 =
            Substring(local.RoutingTextWorkArea.Text8, local.Common.Count, 1);
          local.Total.Count = (int)(local.Total.Count + StringToNumber
            (local.Digit.Text1) * 1);
        }
      }

      local.TempDigit.AverageCurrency = (decimal)local.Total.Count / 10;
      local.Modulus.AverageInteger = local.Total.Count / 10;
      local.ActualTotal.Count = local.Total.Count;
      local.Diff.AverageCurrency = local.TempDigit.AverageCurrency - local
        .Modulus.AverageInteger;

      if (local.Diff.AverageCurrency == 0)
      {
      }
      else
      {
        local.Common.Count = 0;
        local.Increment.Count = 1;

        for(local.Common.Count = 1; local.Common.Count <= 9; ++
          local.Common.Count)
        {
          local.Total.Count += local.Increment.Count;
          local.Modulus.AverageInteger = local.Total.Count / 10;
          local.TempDigit.AverageCurrency = (decimal)local.Total.Count / 10;
          local.Diff.AverageCurrency = local.TempDigit.AverageCurrency - local
            .Modulus.AverageInteger;

          if (local.Diff.AverageCurrency == 0)
          {
            break;
          }
        }
      }

      local.LastDigitCommon.Count = local.Total.Count - local.ActualTotal.Count;
      local.LastDigitTextWorkArea.Text1 =
        NumberToString(local.LastDigitCommon.Count, 15, 1);

      if (AsChar(export.Export9Digit.Text1) != AsChar
        (local.LastDigitTextWorkArea.Text1))
      {
        var field = GetField(export.Export9Digit, "text1");

        field.Error = true;

        ExitState = "FN0000_INVALID_ROUTING_NUMBER";

        return;
      }

      local.PassRoutingPersonPreferredPaymentMethod.AbaRoutingNumber =
        StringToNumber(local.PassRoutingTextWorkArea.Text8 +
        local.LastDigitTextWorkArea.Text1);

      // ------------------------------------------
      // End of Algorithm computation.
      // ------------------------------------------
      if (Equal(global.Command, "ADD"))
      {
        if (Lt(export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date))
        {
          var field =
            GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

          field.Error = true;

          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          return;
        }

        if (Lt(export.PersonPreferredPaymentMethod.DiscontinueDate, Now().Date))
        {
          var field =
            GetField(export.PersonPreferredPaymentMethod, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }

        if (!Lt(export.PersonPreferredPaymentMethod.EffectiveDate,
          export.PersonPreferredPaymentMethod.DiscontinueDate))
        {
          var field =
            GetField(export.PersonPreferredPaymentMethod, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

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
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(export.PaymentMethodType.Code))
        {
          export.PaymentMethodType.Code = "EFT";
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          export.Type1.PromptField = "";
          export.PersonPreferredPaymentMethod.Description =
            Spaces(PersonPreferredPaymentMethod.Description_MaxLength);
          export.PersonPreferredPaymentMethod.DfiAccountNumber = "";
          export.CsePersonsWorkSet.FormattedName = "";
          export.PersonPreferredPaymentMethod.LastUpdateBy = "";
          export.PersonPreferredPaymentMethod.LastUpdateTmst = null;
          export.PersonPreferredPaymentMethod.EffectiveDate = null;
          export.PersonPreferredPaymentMethod.DiscontinueDate = null;
          export.PersonPreferredPaymentMethod.AbaRoutingNumber = 0;
          export.PersonPreferredPaymentMethod.AccountType = "";
          export.PersonPreferredPaymentMethod.SystemGeneratedIdentifier = 0;
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        UseSiReadCsePerson2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCsePersonsWorkSet(local.ReturnFromCsePersonsWorkSet,
            export.CsePersonsWorkSet);
          export.HiddenCsePerson.Number = export.CsePersonsWorkSet.Number;
        }
        else
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          export.CsePersonsWorkSet.FormattedName = "";

          return;
        }

        if (AsChar(import.Flag.Flag) == 'Y')
        {
          if (!ReadPersonPreferredPaymentMethodPaymentMethodType1())
          {
            // -------------------------------------
            // If no record is currently active fetches the next available 
            // record.
            // -------------------------------------
            ReadPersonPreferredPaymentMethodPaymentMethodType3();

            if (entities.PersonPreferredPaymentMethod.
              SystemGeneratedIdentifier == 0)
            {
              ReadPersonPreferredPaymentMethodPaymentMethodType4();
            }
          }
        }
        else
        {
          if (!ReadPersonPreferredPaymentMethodPaymentMethodType2())
          {
            // -------------------------------------
            // If no record is currently active it fetches the next available 
            // record.
            // -------------------------------------
            ReadPersonPreferredPaymentMethodPaymentMethodType3();

            if (entities.PersonPreferredPaymentMethod.
              SystemGeneratedIdentifier == 0)
            {
              ReadPersonPreferredPaymentMethodPaymentMethodType4();
            }
          }

          if (entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier ==
            0)
          {
            export.Type1.PromptField = "";
            export.CsePerson.PromptField = "";
            export.PersonPreferredPaymentMethod.Description =
              Spaces(PersonPreferredPaymentMethod.Description_MaxLength);
            export.PersonPreferredPaymentMethod.DfiAccountNumber = "";
            export.PersonPreferredPaymentMethod.LastUpdateBy = "";
            export.PersonPreferredPaymentMethod.LastUpdateTmst = null;
            export.PersonPreferredPaymentMethod.EffectiveDate = null;
            export.PersonPreferredPaymentMethod.DiscontinueDate = null;
            export.PersonPreferredPaymentMethod.AbaRoutingNumber = 0;
            export.PersonPreferredPaymentMethod.AccountType = "";
            export.PersonPreferredPaymentMethod.SystemGeneratedIdentifier = 0;
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            return;
          }
        }

        export.PersonPreferredPaymentMethod.Assign(
          entities.PersonPreferredPaymentMethod);
        local.Digit.Text1 =
          NumberToString(export.PersonPreferredPaymentMethod.AbaRoutingNumber.
            GetValueOrDefault(), 15, 1);
        export.Export9Digit.Text1 = local.Digit.Text1;
        local.PassRoutingTextWorkArea.Text8 =
          NumberToString(export.PersonPreferredPaymentMethod.AbaRoutingNumber.
            GetValueOrDefault(), 7, 8);
        export.PersonPreferredPaymentMethod.AbaRoutingNumber =
          StringToNumber(local.PassRoutingTextWorkArea.Text8);
        MovePersonPreferredPaymentMethod(entities.PersonPreferredPaymentMethod,
          export.HiddenId);
        MovePaymentMethodType(entities.PaymentMethodType,
          export.PaymentMethodType);
        MovePaymentMethodType(entities.PaymentMethodType,
          export.HiddenPaymentMethodType);

        var field1 = GetField(export.CsePersonsWorkSet, "number");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.PaymentMethodType, "code");

        field2.Color = "cyan";
        field2.Protected = true;

        if (Lt(export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date) &&
          !Equal(export.PersonPreferredPaymentMethod.EffectiveDate, null))
        {
          var field9 =
            GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 =
            GetField(export.PersonPreferredPaymentMethod, "accountType");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 =
            GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 =
            GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.Export9Digit, "text1");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 =
            GetField(export.PersonPreferredPaymentMethod, "lastUpdateBy");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 =
            GetField(export.PersonPreferredPaymentMethod, "lastUpdateTmst");

          field15.Color = "cyan";
          field15.Protected = true;

          if (Lt(export.PersonPreferredPaymentMethod.DiscontinueDate, Now().Date)
            && !
            Equal(export.PersonPreferredPaymentMethod.DiscontinueDate, null))
          {
            var field16 =
              GetField(export.PersonPreferredPaymentMethod, "discontinueDate");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 =
              GetField(export.PersonPreferredPaymentMethod, "description");

            field17.Color = "cyan";
            field17.Protected = true;
          }
        }

        export.CsePerson.PromptField = "";

        if (Equal(export.PaymentMethodType.Code, "WAR"))
        {
          var field9 = GetField(export.Export9ThDigit, "count");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 =
            GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 =
            GetField(export.PersonPreferredPaymentMethod, "accountType");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 =
            GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.Export9Digit, "text1");

          field13.Color = "cyan";
          field13.Protected = true;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        break;
      case "ADD":
        UseSiReadCsePerson1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCsePersonsWorkSet(local.ReturnFromCsePersonsWorkSet,
            export.CsePersonsWorkSet);
          export.HiddenCsePerson.Number = export.CsePersonsWorkSet.Number;
        }
        else
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          export.CsePersonsWorkSet.FormattedName = "";

          return;
        }

        // ------------------------------
        // Validate Type code.
        // ------------------------------
        if (ReadPaymentMethodType())
        {
          export.PaymentMethodType.SystemGeneratedIdentifier =
            entities.PaymentMethodType.SystemGeneratedIdentifier;
        }
        else
        {
          var field = GetField(export.PaymentMethodType, "code");

          field.Error = true;

          ExitState = "FN0000_CODE_OUT_OF_DATE_RANGE";
        }

        // *****************************************************
        // Check for data in required fields.
        // *****************************************************
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        UseFnCreatePersonPrefPmtMthd();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.PassRoutingTextWorkArea.Text8 =
            NumberToString(export.PersonPreferredPaymentMethod.AbaRoutingNumber.
              GetValueOrDefault(), 7, 8);
          export.PersonPreferredPaymentMethod.AbaRoutingNumber =
            StringToNumber(local.PassRoutingTextWorkArea.Text8);

          var field9 = GetField(export.CsePersonsWorkSet, "number");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.PaymentMethodType, "code");

          field10.Color = "cyan";
          field10.Protected = true;

          MovePersonPreferredPaymentMethod(export.PersonPreferredPaymentMethod,
            export.HiddenId);
          export.CsePerson.PromptField = "";
          export.Type1.PromptField = "";
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

          return;
        }

        if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          var field9 =
            GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

          field9.Error = true;

          var field10 =
            GetField(export.PersonPreferredPaymentMethod, "discontinueDate");

          field10.Error = true;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field9 = GetField(export.CsePersonsWorkSet, "number");

          field9.Error = true;

          var field10 = GetField(export.CsePersonsWorkSet, "formattedName");

          field10.Error = true;
        }
        else if (IsExitState("FN0000_CODE_OUT_OF_DATE_RANGE"))
        {
          var field = GetField(export.PaymentMethodType, "code");

          field.Error = true;
        }
        else
        {
        }

        local.PassRoutingTextWorkArea.Text8 =
          NumberToString(local.PassRoutingPersonPreferredPaymentMethod.
            AbaRoutingNumber.GetValueOrDefault(), 7, 8);
        export.PersonPreferredPaymentMethod.AbaRoutingNumber =
          StringToNumber(local.PassRoutingTextWorkArea.Text8);

        break;
      case "UPDATE":
        // *****************************************************
        // Effective date and method type code cannot be modified.
        // *****************************************************
        var field3 = GetField(export.CsePersonsWorkSet, "number");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.PaymentMethodType, "code");

        field4.Color = "cyan";
        field4.Protected = true;

        if (Equal(export.PersonPreferredPaymentMethod.DiscontinueDate,
          export.HiddenId.DiscontinueDate) && Lt
          (export.PersonPreferredPaymentMethod.DiscontinueDate, Now().Date))
        {
          var field9 =
            GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 =
            GetField(export.PersonPreferredPaymentMethod, "discontinueDate");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 =
            GetField(export.PersonPreferredPaymentMethod, "accountType");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 =
            GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 =
            GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.Export9Digit, "text1");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 =
            GetField(export.PersonPreferredPaymentMethod, "description");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 =
            GetField(export.PersonPreferredPaymentMethod, "lastUpdateBy");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 =
            GetField(export.PersonPreferredPaymentMethod, "lastUpdateTmst");

          field17.Color = "cyan";
          field17.Protected = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }
        else if (!Equal(export.PersonPreferredPaymentMethod.DiscontinueDate,
          export.HiddenId.DiscontinueDate) || !
          Equal(export.PersonPreferredPaymentMethod.EffectiveDate,
          export.HiddenId.EffectiveDate))
        {
          if (Equal(export.PersonPreferredPaymentMethod.EffectiveDate,
            export.HiddenId.EffectiveDate) && Lt
            (export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date))
          {
            var field9 =
              GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.PersonPreferredPaymentMethod, "accountType");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");
              

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 =
              GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");
              

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 = GetField(export.Export9Digit, "text1");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 =
              GetField(export.PersonPreferredPaymentMethod, "lastUpdateBy");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 =
              GetField(export.PersonPreferredPaymentMethod, "lastUpdateTmst");

            field15.Color = "cyan";
            field15.Protected = true;
          }

          if (!Equal(export.PersonPreferredPaymentMethod.EffectiveDate,
            export.HiddenId.EffectiveDate) && Lt
            (export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date))
          {
            var field =
              GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

            field.Error = true;

            ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

            return;
          }

          if (Lt(export.PersonPreferredPaymentMethod.DiscontinueDate, Now().Date))
            
          {
            var field =
              GetField(export.PersonPreferredPaymentMethod, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            return;
          }

          if (!Lt(export.PersonPreferredPaymentMethod.EffectiveDate,
            export.PersonPreferredPaymentMethod.DiscontinueDate))
          {
            var field =
              GetField(export.PersonPreferredPaymentMethod, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            return;
          }
        }

        UseFnUpdatePersonPrefPmtMthd();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.PassRoutingTextWorkArea.Text8 =
            NumberToString(local.ReturnFromPersonPreferredPaymentMethod.
              AbaRoutingNumber.GetValueOrDefault(), 7, 8);
          export.PersonPreferredPaymentMethod.AbaRoutingNumber =
            StringToNumber(local.PassRoutingTextWorkArea.Text8);
          export.PersonPreferredPaymentMethod.LastUpdateBy =
            local.ReturnFromPersonPreferredPaymentMethod.LastUpdateBy ?? "";
          export.PersonPreferredPaymentMethod.LastUpdateTmst =
            local.ReturnFromPersonPreferredPaymentMethod.LastUpdateTmst;
          export.CsePerson.PromptField = "";
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          local.PassRoutingTextWorkArea.Text8 =
            NumberToString(local.PassRoutingPersonPreferredPaymentMethod.
              AbaRoutingNumber.GetValueOrDefault(), 7, 8);
          export.PersonPreferredPaymentMethod.AbaRoutingNumber =
            StringToNumber(local.PassRoutingTextWorkArea.Text8);

          var field9 =
            GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

          field9.Error = true;

          var field10 =
            GetField(export.PersonPreferredPaymentMethod, "discontinueDate");

          field10.Error = true;
        }
        else if (IsExitState("FN0000_CODE_OUT_OF_DATE_RANGE"))
        {
          var field = GetField(export.PaymentMethodType, "code");

          field.Error = true;
        }
        else
        {
        }

        var field5 = GetField(export.CsePersonsWorkSet, "number");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.PaymentMethodType, "code");

        field6.Color = "cyan";
        field6.Protected = true;

        if (Equal(export.PaymentMethodType.Code, "WAR"))
        {
          var field9 =
            GetField(export.PersonPreferredPaymentMethod, "accountType");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 =
            GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 =
            GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.Export9Digit, "text1");

          field12.Color = "cyan";
          field12.Protected = true;
        }

        if (Lt(export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date))
        {
          if (!IsExitState("ACO_NE0000_DATE_OVERLAP"))
          {
            var field =
              GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

            field.Color = "cyan";
            field.Protected = true;
          }

          var field9 =
            GetField(export.PersonPreferredPaymentMethod, "accountType");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 =
            GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 =
            GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.Export9Digit, "text1");

          field12.Color = "cyan";
          field12.Protected = true;
        }

        break;
      case "DELETE":
        if (export.PersonPreferredPaymentMethod.SystemGeneratedIdentifier != 0)
        {
          if (Lt(export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date))
          {
            if (Lt(export.PersonPreferredPaymentMethod.DiscontinueDate,
              Now().Date) && !
              Equal(export.PersonPreferredPaymentMethod.DiscontinueDate, null))
            {
              var field18 =
                GetField(export.PersonPreferredPaymentMethod, "discontinueDate");
                

              field18.Color = "cyan";
              field18.Protected = true;

              var field19 =
                GetField(export.PersonPreferredPaymentMethod, "description");

              field19.Color = "cyan";
              field19.Protected = true;
            }

            var field9 = GetField(export.CsePersonsWorkSet, "number");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 = GetField(export.PaymentMethodType, "code");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 =
              GetField(export.PersonPreferredPaymentMethod, "accountType");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 =
              GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");
              

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 =
              GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");
              

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 = GetField(export.Export9Digit, "text1");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 =
              GetField(export.PersonPreferredPaymentMethod, "lastUpdateBy");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 =
              GetField(export.PersonPreferredPaymentMethod, "lastUpdateTmst");

            field17.Color = "cyan";
            field17.Protected = true;

            ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";
          }

          if (Equal(export.PersonPreferredPaymentMethod.EffectiveDate,
            Now().Date) || Equal
            (export.PersonPreferredPaymentMethod.DiscontinueDate, Now().Date) ||
            Lt
            (export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date) && Lt
            (Now().Date, export.PersonPreferredPaymentMethod.DiscontinueDate))
          {
            ExitState = "FN0000_RECORD_CURR_ACTIVE";
          }
        }
        else
        {
          ExitState = "FN0000_NO_DATA_TO_DELETE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseFnDeletePersonPrefPmt();

        // Set the hidden key field to spaces or zero.
        export.HiddenId.SystemGeneratedIdentifier = 0;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.CsePersonsWorkSet.Number = "";
          export.CsePersonsWorkSet.FormattedName = "";
          export.PaymentMethodType.Code = "";
          export.PersonPreferredPaymentMethod.Description =
            Spaces(PersonPreferredPaymentMethod.Description_MaxLength);
          export.PersonPreferredPaymentMethod.DfiAccountNumber = "";
          export.PersonPreferredPaymentMethod.LastUpdateBy = "";
          export.PersonPreferredPaymentMethod.LastUpdateTmst = null;
          export.PersonPreferredPaymentMethod.EffectiveDate = null;
          export.PersonPreferredPaymentMethod.DiscontinueDate = null;
          export.PersonPreferredPaymentMethod.AbaRoutingNumber = 0;
          export.Export9Digit.Text1 = "";
          export.PersonPreferredPaymentMethod.SystemGeneratedIdentifier = 0;
          export.PersonPreferredPaymentMethod.AccountType = "";
          ExitState = "FN0000_DELETE_SUCCESSFUL";
        }

        break;
      case "PPLT":
        ExitState = "ECO_LNK_TO_PREFERRED_PMT_LIST";

        break;
      case "LIST":
        if (AsChar(export.CsePerson.PromptField) != 'S')
        {
          if (export.PersonPreferredPaymentMethod.SystemGeneratedIdentifier != 0
            )
          {
            var field9 = GetField(export.CsePersonsWorkSet, "number");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 = GetField(export.PaymentMethodType, "code");

            field10.Color = "cyan";
            field10.Protected = true;

            if (Equal(export.PaymentMethodType.Code, "WAR"))
            {
              var field11 =
                GetField(export.PersonPreferredPaymentMethod, "accountType");

              field11.Color = "cyan";
              field11.Protected = true;

              var field12 =
                GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");
                

              field12.Color = "cyan";
              field12.Protected = true;

              var field13 =
                GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");
                

              field13.Color = "cyan";
              field13.Protected = true;

              var field14 = GetField(export.Export9Digit, "text1");

              field14.Color = "cyan";
              field14.Protected = true;
            }

            if (Lt(export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date)
              && !
              Equal(export.PersonPreferredPaymentMethod.EffectiveDate, null))
            {
              var field11 =
                GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

              field11.Color = "cyan";
              field11.Protected = true;

              var field12 =
                GetField(export.PersonPreferredPaymentMethod, "accountType");

              field12.Color = "cyan";
              field12.Protected = true;

              var field13 =
                GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");
                

              field13.Color = "cyan";
              field13.Protected = true;

              var field14 =
                GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");
                

              field14.Color = "cyan";
              field14.Protected = true;

              var field15 = GetField(export.Export9Digit, "text1");

              field15.Color = "cyan";
              field15.Protected = true;

              var field16 =
                GetField(export.PersonPreferredPaymentMethod, "lastUpdateBy");

              field16.Color = "cyan";
              field16.Protected = true;

              var field17 =
                GetField(export.PersonPreferredPaymentMethod, "lastUpdateTmst");
                

              field17.Color = "cyan";
              field17.Protected = true;

              if (Lt(export.PersonPreferredPaymentMethod.DiscontinueDate,
                Now().Date) && !
                Equal(export.PersonPreferredPaymentMethod.DiscontinueDate, null))
                
              {
                var field18 =
                  GetField(export.PersonPreferredPaymentMethod,
                  "discontinueDate");

                field18.Color = "cyan";
                field18.Protected = true;

                var field19 =
                  GetField(export.PersonPreferredPaymentMethod, "description");

                field19.Color = "cyan";
                field19.Protected = true;
              }
            }
          }

          var field = GetField(export.CsePerson, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }
        else
        {
          export.CsePerson.PromptField = "";
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        // -----------------------------------------------------
        // SWSRKXD PR# 80797 - 11/29/99
        // Protected fields must remain protected when PF9 Key is
        // depressed.
        // -----------------------------------------------------
        var field7 = GetField(export.PaymentMethodType, "code");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.CsePersonsWorkSet, "number");

        field8.Color = "cyan";
        field8.Protected = true;

        if (Lt(export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date) &&
          !Equal(export.PersonPreferredPaymentMethod.EffectiveDate, null))
        {
          var field9 =
            GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 =
            GetField(export.PersonPreferredPaymentMethod, "accountType");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 =
            GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 =
            GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.Export9Digit, "text1");

          field13.Color = "cyan";
          field13.Protected = true;

          if (Lt(export.PersonPreferredPaymentMethod.DiscontinueDate, Now().Date)
            && !
            Equal(export.PersonPreferredPaymentMethod.DiscontinueDate, null))
          {
            var field14 =
              GetField(export.PersonPreferredPaymentMethod, "discontinueDate");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 =
              GetField(export.PersonPreferredPaymentMethod, "description");

            field15.Color = "cyan";
            field15.Protected = true;
          }
        }

        if (Equal(export.PaymentMethodType.Code, "WAR"))
        {
          var field9 =
            GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 =
            GetField(export.PersonPreferredPaymentMethod, "accountType");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 =
            GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.Export9Digit, "text1");

          field12.Color = "cyan";
          field12.Protected = true;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        if (export.PersonPreferredPaymentMethod.SystemGeneratedIdentifier != 0)
        {
          var field9 = GetField(export.CsePersonsWorkSet, "number");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.PaymentMethodType, "code");

          field10.Color = "cyan";
          field10.Protected = true;

          if (Equal(export.PaymentMethodType.Code, "WAR"))
          {
            var field11 =
              GetField(export.PersonPreferredPaymentMethod, "accountType");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 =
              GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");
              

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 =
              GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");
              

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 = GetField(export.Export9Digit, "text1");

            field14.Color = "cyan";
            field14.Protected = true;
          }

          if (Lt(export.PersonPreferredPaymentMethod.EffectiveDate, Now().Date) &&
            !Equal(export.PersonPreferredPaymentMethod.EffectiveDate, null))
          {
            var field11 =
              GetField(export.PersonPreferredPaymentMethod, "effectiveDate");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 =
              GetField(export.PersonPreferredPaymentMethod, "accountType");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 =
              GetField(export.PersonPreferredPaymentMethod, "dfiAccountNumber");
              

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 =
              GetField(export.PersonPreferredPaymentMethod, "abaRoutingNumber");
              

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 = GetField(export.Export9Digit, "text1");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 =
              GetField(export.PersonPreferredPaymentMethod, "lastUpdateBy");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 =
              GetField(export.PersonPreferredPaymentMethod, "lastUpdateTmst");

            field17.Color = "cyan";
            field17.Protected = true;

            if (Lt(export.PersonPreferredPaymentMethod.DiscontinueDate,
              Now().Date) && !
              Equal(export.PersonPreferredPaymentMethod.DiscontinueDate, null))
            {
              var field18 =
                GetField(export.PersonPreferredPaymentMethod, "discontinueDate");
                

              field18.Color = "cyan";
              field18.Protected = true;

              var field19 =
                GetField(export.PersonPreferredPaymentMethod, "description");

              field19.Color = "cyan";
              field19.Protected = true;
            }
          }
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private static void MovePaymentMethodType(PaymentMethodType source,
    PaymentMethodType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MovePersonPreferredPaymentMethod(
    PersonPreferredPaymentMethod source, PersonPreferredPaymentMethod target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void UseFnCreatePersonPrefPmtMthd()
  {
    var useImport = new FnCreatePersonPrefPmtMthd.Import();
    var useExport = new FnCreatePersonPrefPmtMthd.Export();

    useImport.Routing.AbaRoutingNumber =
      local.PassRoutingPersonPreferredPaymentMethod.AbaRoutingNumber;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.PersonPreferredPaymentMethod.Assign(
      export.PersonPreferredPaymentMethod);
    MovePaymentMethodType(export.PaymentMethodType, useImport.PaymentMethodType);
      

    Call(FnCreatePersonPrefPmtMthd.Execute, useImport, useExport);

    export.PersonPreferredPaymentMethod.Assign(
      useExport.PersonPreferredPaymentMethod);
  }

  private void UseFnDeletePersonPrefPmt()
  {
    var useImport = new FnDeletePersonPrefPmt.Import();
    var useExport = new FnDeletePersonPrefPmt.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.PaymentMethodType.SystemGeneratedIdentifier =
      import.PaymentMethodType.SystemGeneratedIdentifier;
    MovePersonPreferredPaymentMethod(import.PersonPreferredPaymentMethod,
      useImport.PersonPreferredPaymentMethod);

    Call(FnDeletePersonPrefPmt.Execute, useImport, useExport);
  }

  private void UseFnUpdatePersonPrefPmtMthd()
  {
    var useImport = new FnUpdatePersonPrefPmtMthd.Import();
    var useExport = new FnUpdatePersonPrefPmtMthd.Export();

    useImport.Routing.AbaRoutingNumber =
      local.PassRoutingPersonPreferredPaymentMethod.AbaRoutingNumber;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MovePaymentMethodType(export.PaymentMethodType, useImport.PaymentMethodType);
      
    useImport.PersonPreferredPaymentMethod.Assign(
      export.PersonPreferredPaymentMethod);

    Call(FnUpdatePersonPrefPmtMthd.Execute, useImport, useExport);

    local.ReturnFromPersonPreferredPaymentMethod.Assign(
      useExport.PersonPreferredPaymentMethod);
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

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ReturnFromCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ReturnFromCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadPaymentMethodType()
  {
    entities.PaymentMethodType.Populated = false;

    return Read("ReadPaymentMethodType",
      (db, command) =>
      {
        db.SetString(command, "code", export.PaymentMethodType.Code);
        db.SetDate(
          command, "effectiveDate",
          export.PersonPreferredPaymentMethod.EffectiveDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          export.PersonPreferredPaymentMethod.DiscontinueDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.Code = db.GetString(reader, 1);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 2);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.PaymentMethodType.Populated = true;
      });
  }

  private bool ReadPersonPreferredPaymentMethodPaymentMethodType1()
  {
    entities.PaymentMethodType.Populated = false;
    entities.PersonPreferredPaymentMethod.Populated = false;

    return Read("ReadPersonPreferredPaymentMethodPaymentMethodType1",
      (db, command) =>
      {
        db.SetString(command, "cspPNumber", export.CsePersonsWorkSet.Number);
        db.SetInt32(
          command, "persnPmntMethId",
          export.PersonPreferredPaymentMethod.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.AbaRoutingNumber =
          db.GetNullableInt64(reader, 2);
        entities.PersonPreferredPaymentMethod.DfiAccountNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 4);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonPreferredPaymentMethod.CreatedBy =
          db.GetString(reader, 6);
        entities.PersonPreferredPaymentMethod.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.PersonPreferredPaymentMethod.LastUpdateBy =
          db.GetNullableString(reader, 8);
        entities.PersonPreferredPaymentMethod.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 10);
        entities.PersonPreferredPaymentMethod.Description =
          db.GetNullableString(reader, 11);
        entities.PersonPreferredPaymentMethod.AccountType =
          db.GetNullableString(reader, 12);
        entities.PaymentMethodType.Code = db.GetString(reader, 13);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 14);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.PaymentMethodType.Populated = true;
        entities.PersonPreferredPaymentMethod.Populated = true;
      });
  }

  private bool ReadPersonPreferredPaymentMethodPaymentMethodType2()
  {
    entities.PaymentMethodType.Populated = false;
    entities.PersonPreferredPaymentMethod.Populated = false;

    return Read("ReadPersonPreferredPaymentMethodPaymentMethodType2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "cspPNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.AbaRoutingNumber =
          db.GetNullableInt64(reader, 2);
        entities.PersonPreferredPaymentMethod.DfiAccountNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 4);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonPreferredPaymentMethod.CreatedBy =
          db.GetString(reader, 6);
        entities.PersonPreferredPaymentMethod.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.PersonPreferredPaymentMethod.LastUpdateBy =
          db.GetNullableString(reader, 8);
        entities.PersonPreferredPaymentMethod.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 10);
        entities.PersonPreferredPaymentMethod.Description =
          db.GetNullableString(reader, 11);
        entities.PersonPreferredPaymentMethod.AccountType =
          db.GetNullableString(reader, 12);
        entities.PaymentMethodType.Code = db.GetString(reader, 13);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 14);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.PaymentMethodType.Populated = true;
        entities.PersonPreferredPaymentMethod.Populated = true;
      });
  }

  private bool ReadPersonPreferredPaymentMethodPaymentMethodType3()
  {
    entities.PaymentMethodType.Populated = false;
    entities.PersonPreferredPaymentMethod.Populated = false;

    return Read("ReadPersonPreferredPaymentMethodPaymentMethodType3",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "cspPNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.AbaRoutingNumber =
          db.GetNullableInt64(reader, 2);
        entities.PersonPreferredPaymentMethod.DfiAccountNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 4);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonPreferredPaymentMethod.CreatedBy =
          db.GetString(reader, 6);
        entities.PersonPreferredPaymentMethod.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.PersonPreferredPaymentMethod.LastUpdateBy =
          db.GetNullableString(reader, 8);
        entities.PersonPreferredPaymentMethod.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 10);
        entities.PersonPreferredPaymentMethod.Description =
          db.GetNullableString(reader, 11);
        entities.PersonPreferredPaymentMethod.AccountType =
          db.GetNullableString(reader, 12);
        entities.PaymentMethodType.Code = db.GetString(reader, 13);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 14);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.PaymentMethodType.Populated = true;
        entities.PersonPreferredPaymentMethod.Populated = true;
      });
  }

  private bool ReadPersonPreferredPaymentMethodPaymentMethodType4()
  {
    entities.PaymentMethodType.Populated = false;
    entities.PersonPreferredPaymentMethod.Populated = false;

    return Read("ReadPersonPreferredPaymentMethodPaymentMethodType4",
      (db, command) =>
      {
        db.SetString(command, "cspPNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.AbaRoutingNumber =
          db.GetNullableInt64(reader, 2);
        entities.PersonPreferredPaymentMethod.DfiAccountNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 4);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonPreferredPaymentMethod.CreatedBy =
          db.GetString(reader, 6);
        entities.PersonPreferredPaymentMethod.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.PersonPreferredPaymentMethod.LastUpdateBy =
          db.GetNullableString(reader, 8);
        entities.PersonPreferredPaymentMethod.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 10);
        entities.PersonPreferredPaymentMethod.Description =
          db.GetNullableString(reader, 11);
        entities.PersonPreferredPaymentMethod.AccountType =
          db.GetNullableString(reader, 12);
        entities.PaymentMethodType.Code = db.GetString(reader, 13);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 14);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.PaymentMethodType.Populated = true;
        entities.PersonPreferredPaymentMethod.Populated = true;
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
    /// A value of Import9Digit.
    /// </summary>
    [JsonPropertyName("import9Digit")]
    public TextWorkArea Import9Digit
    {
      get => import9Digit ??= new();
      set => import9Digit = value;
    }

    /// <summary>
    /// A value of Import9ThDigit.
    /// </summary>
    [JsonPropertyName("import9ThDigit")]
    public Common Import9ThDigit
    {
      get => import9ThDigit ??= new();
      set => import9ThDigit = value;
    }

    /// <summary>
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Type1.
    /// </summary>
    [JsonPropertyName("type1")]
    public Standard Type1
    {
      get => type1 ??= new();
      set => type1 = value;
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
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of HiddenPaymentMethodType.
    /// </summary>
    [JsonPropertyName("hiddenPaymentMethodType")]
    public PaymentMethodType HiddenPaymentMethodType
    {
      get => hiddenPaymentMethodType ??= new();
      set => hiddenPaymentMethodType = value;
    }

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public PersonPreferredPaymentMethod HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
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

    private TextWorkArea import9Digit;
    private Common import9ThDigit;
    private Common flag;
    private CsePerson hiddenCsePerson;
    private Standard csePerson;
    private Standard type1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PaymentMethodType paymentMethodType;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private PaymentMethodType hiddenPaymentMethodType;
    private PersonPreferredPaymentMethod hiddenId;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Export9Digit.
    /// </summary>
    [JsonPropertyName("export9Digit")]
    public TextWorkArea Export9Digit
    {
      get => export9Digit ??= new();
      set => export9Digit = value;
    }

    /// <summary>
    /// A value of Export9ThDigit.
    /// </summary>
    [JsonPropertyName("export9ThDigit")]
    public Common Export9ThDigit
    {
      get => export9ThDigit ??= new();
      set => export9ThDigit = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Type1.
    /// </summary>
    [JsonPropertyName("type1")]
    public Standard Type1
    {
      get => type1 ??= new();
      set => type1 = value;
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
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of HiddenPaymentMethodType.
    /// </summary>
    [JsonPropertyName("hiddenPaymentMethodType")]
    public PaymentMethodType HiddenPaymentMethodType
    {
      get => hiddenPaymentMethodType ??= new();
      set => hiddenPaymentMethodType = value;
    }

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public PersonPreferredPaymentMethod HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
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

    private TextWorkArea export9Digit;
    private Common export9ThDigit;
    private CsePerson hiddenCsePerson;
    private Standard csePerson;
    private Standard type1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PaymentMethodType paymentMethodType;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private PaymentMethodType hiddenPaymentMethodType;
    private PersonPreferredPaymentMethod hiddenId;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Increment.
    /// </summary>
    [JsonPropertyName("increment")]
    public Common Increment
    {
      get => increment ??= new();
      set => increment = value;
    }

    /// <summary>
    /// A value of PassRoutingPersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("passRoutingPersonPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PassRoutingPersonPreferredPaymentMethod
    {
      get => passRoutingPersonPreferredPaymentMethod ??= new();
      set => passRoutingPersonPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of PassRoutingTextWorkArea.
    /// </summary>
    [JsonPropertyName("passRoutingTextWorkArea")]
    public TextWorkArea PassRoutingTextWorkArea
    {
      get => passRoutingTextWorkArea ??= new();
      set => passRoutingTextWorkArea = value;
    }

    /// <summary>
    /// A value of TempDigit.
    /// </summary>
    [JsonPropertyName("tempDigit")]
    public Common TempDigit
    {
      get => tempDigit ??= new();
      set => tempDigit = value;
    }

    /// <summary>
    /// A value of Digit.
    /// </summary>
    [JsonPropertyName("digit")]
    public TextWorkArea Digit
    {
      get => digit ??= new();
      set => digit = value;
    }

    /// <summary>
    /// A value of Modulus.
    /// </summary>
    [JsonPropertyName("modulus")]
    public Common Modulus
    {
      get => modulus ??= new();
      set => modulus = value;
    }

    /// <summary>
    /// A value of Final.
    /// </summary>
    [JsonPropertyName("final")]
    public TextWorkArea Final
    {
      get => final ??= new();
      set => final = value;
    }

    /// <summary>
    /// A value of LastDigitTextWorkArea.
    /// </summary>
    [JsonPropertyName("lastDigitTextWorkArea")]
    public TextWorkArea LastDigitTextWorkArea
    {
      get => lastDigitTextWorkArea ??= new();
      set => lastDigitTextWorkArea = value;
    }

    /// <summary>
    /// A value of LastDigitCommon.
    /// </summary>
    [JsonPropertyName("lastDigitCommon")]
    public Common LastDigitCommon
    {
      get => lastDigitCommon ??= new();
      set => lastDigitCommon = value;
    }

    /// <summary>
    /// A value of ActualTotal.
    /// </summary>
    [JsonPropertyName("actualTotal")]
    public Common ActualTotal
    {
      get => actualTotal ??= new();
      set => actualTotal = value;
    }

    /// <summary>
    /// A value of Diff.
    /// </summary>
    [JsonPropertyName("diff")]
    public Common Diff
    {
      get => diff ??= new();
      set => diff = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Common Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of RoutingTextWorkArea.
    /// </summary>
    [JsonPropertyName("routingTextWorkArea")]
    public TextWorkArea RoutingTextWorkArea
    {
      get => routingTextWorkArea ??= new();
      set => routingTextWorkArea = value;
    }

    /// <summary>
    /// A value of RoutingCommon.
    /// </summary>
    [JsonPropertyName("routingCommon")]
    public Common RoutingCommon
    {
      get => routingCommon ??= new();
      set => routingCommon = value;
    }

    /// <summary>
    /// A value of Third.
    /// </summary>
    [JsonPropertyName("third")]
    public Common Third
    {
      get => third ??= new();
      set => third = value;
    }

    /// <summary>
    /// A value of Second.
    /// </summary>
    [JsonPropertyName("second")]
    public Common Second
    {
      get => second ??= new();
      set => second = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public Common First
    {
      get => first ??= new();
      set => first = value;
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
    /// A value of ReturnFromPersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("returnFromPersonPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod ReturnFromPersonPreferredPaymentMethod
    {
      get => returnFromPersonPreferredPaymentMethod ??= new();
      set => returnFromPersonPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of ReturnFromCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("returnFromCsePersonsWorkSet")]
    public CsePersonsWorkSet ReturnFromCsePersonsWorkSet
    {
      get => returnFromCsePersonsWorkSet ??= new();
      set => returnFromCsePersonsWorkSet = value;
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

    private Common increment;
    private PersonPreferredPaymentMethod passRoutingPersonPreferredPaymentMethod;
      
    private TextWorkArea passRoutingTextWorkArea;
    private Common tempDigit;
    private TextWorkArea digit;
    private Common modulus;
    private TextWorkArea final;
    private TextWorkArea lastDigitTextWorkArea;
    private Common lastDigitCommon;
    private Common actualTotal;
    private Common diff;
    private Common total;
    private TextWorkArea routingTextWorkArea;
    private Common routingCommon;
    private Common third;
    private Common second;
    private Common first;
    private Common common;
    private PersonPreferredPaymentMethod returnFromPersonPreferredPaymentMethod;
    private CsePersonsWorkSet returnFromCsePersonsWorkSet;
    private DateWorkArea discontinue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private PaymentMethodType paymentMethodType;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
  }
#endregion
}
