// Program: LE_EXGR_EXMPTS_GRNTD_BY_CSE_PRSN, ID: 372588053, model: 746.
// Short name: SWEEXGRP
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
/// A program: LE_EXGR_EXMPTS_GRNTD_BY_CSE_PRSN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeExgrExmptsGrntdByCsePrsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EXGR_EXMPTS_GRNTD_BY_CSE_PRSN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeExgrExmptsGrntdByCsePrsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeExgrExmptsGrntdByCsePrsn.
  /// </summary>
  public LeExgrExmptsGrntdByCsePrsn(IContext context, Import import,
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
    // *******************************************************************
    // Date  	  Developer      Request #  Description
    // 08/08/95  S. Benton                 Initial development
    // 11/06/98  P. Sharp     Made changes based on Phase II assessment sheets 
    // and unit test plans. Person # and SSN validation was not returning error
    // correctly. Fixed exit states and errored fields.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      // **** begin group F ****
      UseScCabSignoff();

      return;

      // **** end   group F ****
    }

    local.Common.Count = 0;
    local.Exemptions1.Count = 0;

    // *********************************************
    // Move Imports to Exports.
    // *********************************************
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveAdministrativeAction(import.AdministrativeAction,
      export.AdministrativeAction);
    export.Starting.EffectiveDate = import.Starting.EffectiveDate;
    export.Required.Assign(import.Required);
    export.PromptObligationType.PromptField =
      import.PromptObligationType.PromptField;
    export.PromptAdminAction.PromptField = import.PromptAdminAction.PromptField;
    export.Display.Flag = import.Display.Flag;
    export.SsnWorkArea.Assign(import.SsnWorkArea);

    if (export.SsnWorkArea.SsnNumPart1 > 0 || export.SsnWorkArea.SsnNumPart2 > 0
      || export.SsnWorkArea.SsnNumPart3 > 0)
    {
      export.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.CsePersonsWorkSet.Ssn = export.SsnWorkArea.SsnText9;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else if (!import.Exemptions.IsEmpty)
    {
      export.Exemptions.Index = 0;
      export.Exemptions.Clear();

      for(import.Exemptions.Index = 0; import.Exemptions.Index < import
        .Exemptions.Count; ++import.Exemptions.Index)
      {
        if (export.Exemptions.IsFull)
        {
          break;
        }

        MoveObligation(import.Exemptions.Item.Obligation,
          export.Exemptions.Update.Obligation);
        export.Exemptions.Update.ObligationType.Assign(
          import.Exemptions.Item.ObligationType);
        export.Exemptions.Update.ObligationAdmActionExemption.Assign(
          import.Exemptions.Item.ObligationAdmActionExemption);
        export.Exemptions.Update.CsePersonsWorkSet.FormattedName =
          import.Exemptions.Item.CsePersonsWorkSet.FormattedName;
        export.Exemptions.Update.AdministrativeAction.Type1 =
          import.Exemptions.Item.AdministrativeAction.Type1;

        // *********************************************
        // Check how many selections have been made.
        // Do not allow scrolling when a selection has
        // been made.
        // *********************************************
        switch(AsChar(import.Exemptions.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Common.Count;
            export.SelectedObligationAdmActionExemption.Assign(
              export.Exemptions.Item.ObligationAdmActionExemption);
            export.SelectedObligation.SystemGeneratedIdentifier =
              export.Exemptions.Item.Obligation.SystemGeneratedIdentifier;
            export.SelectedAdministrativeAction.Type1 =
              export.Exemptions.Item.AdministrativeAction.Type1;
            MoveObligationType1(export.Exemptions.Item.ObligationType,
              export.SelectedObligationType);

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Exemptions.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        export.Exemptions.Next();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.CsePersonsWorkSet.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
        (10);

      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
      }

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETADAA") && !
      Equal(global.Command, "RETOBTL"))
    {
      export.PromptObligationType.PromptField = "";
      export.PromptAdminAction.PromptField = "";
    }

    MoveCsePersonsWorkSet1(export.CsePersonsWorkSet, local.Saved);

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        if (IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

          field2.Error = true;

          var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

          field3.Error = true;

          var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

          field4.Error = true;

          ExitState = "LE0000_CSE_PERSON_NO_OR_SSN_REQD";

          if (AsChar(export.Display.Flag) == 'Y')
          {
            MoveAdministrativeAction(local.ClearAdministrativeAction,
              export.AdministrativeAction);
            export.Starting.EffectiveDate =
              local.ClearObligationAdmActionExemption.EffectiveDate;
            export.Required.Assign(local.ClearObligationType);
          }

          return;
        }
        else
        {
          local.SearchPerson.Flag = "1";
          UseCabMatchCsePerson();
          local.NoOfSsnMatchesFound.Count = 0;

          if (!local.Local1.IsEmpty)
          {
            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              ++local.NoOfSsnMatchesFound.Count;

              if (local.NoOfSsnMatchesFound.Count == 1)
              {
                MoveCsePersonsWorkSet2(local.Local1.Item.Detail,
                  export.CsePersonsWorkSet);

                if (IsEmpty(export.CsePersonsWorkSet.Number))
                {
                  var field1 = GetField(export.CsePersonsWorkSet, "number");

                  field1.Error = true;

                  var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

                  field2.Error = true;

                  var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

                  field3.Error = true;

                  var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

                  field4.Error = true;

                  ExitState = "LE0000_CSE_PERSON_NF_FOR_SSN";

                  return;
                }

                UseSiFormatCsePersonName();
              }

              if (local.NoOfSsnMatchesFound.Count > 1)
              {
                ExitState = "LE0000_MULTIPLE_PERSONS_FOR_SSN";

                return;
              }
            }
          }
          else
          {
            var field1 = GetField(export.SsnWorkArea, "ssnNumPart1");

            field1.Error = true;

            var field2 = GetField(export.SsnWorkArea, "ssnNumPart2");

            field2.Error = true;

            var field3 = GetField(export.SsnWorkArea, "ssnNumPart3");

            field3.Error = true;

            MoveCsePersonsWorkSet1(local.Saved, export.CsePersonsWorkSet);

            var field4 = GetField(export.CsePersonsWorkSet, "number");

            field4.Error = true;

            ExitState = "LE0000_CSE_PERSON_NF_FOR_SSN";

            if (AsChar(export.Display.Flag) == 'Y')
            {
              MoveAdministrativeAction(local.ClearAdministrativeAction,
                export.AdministrativeAction);
              export.Starting.EffectiveDate =
                local.ClearObligationAdmActionExemption.EffectiveDate;
              export.Required.Assign(local.ClearObligationType);
            }

            return;
          }
        }
      }
      else
      {
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCsePersonsWorkSet1(local.Saved, export.CsePersonsWorkSet);

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "OE0026_INVALID_CSE_PERSON_NO";

          if (AsChar(export.Display.Flag) == 'Y')
          {
            MoveAdministrativeAction(local.ClearAdministrativeAction,
              export.AdministrativeAction);
            export.Starting.EffectiveDate =
              local.ClearObligationAdmActionExemption.EffectiveDate;
            export.Required.Assign(local.ClearObligationType);
            export.SsnWorkArea.SsnNumPart1 = 0;
            export.SsnWorkArea.SsnNumPart2 = 0;
            export.SsnWorkArea.SsnNumPart3 = 0;
          }

          return;
        }
      }

      if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
      {
        export.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
        UseCabSsnConvertTextToNum();
      }
    }

    // *********************************************
    //        P F   K E Y   P R O C E S S I N G
    // *********************************************
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        if (!IsEmpty(export.PromptAdminAction.PromptField) && AsChar
          (export.PromptAdminAction.PromptField) != 'S')
        {
          var field = GetField(export.PromptAdminAction, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        if (!IsEmpty(export.PromptObligationType.PromptField) && AsChar
          (export.PromptObligationType.PromptField) != 'S')
        {
          var field = GetField(export.PromptObligationType, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          return;
        }

        if (AsChar(export.PromptAdminAction.PromptField) == 'S' && AsChar
          (export.PromptObligationType.PromptField) == 'S')
        {
          var field1 = GetField(export.PromptAdminAction, "promptField");

          field1.Error = true;

          var field2 = GetField(export.PromptObligationType, "promptField");

          field2.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(export.PromptAdminAction.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_ADMIN_ACTION_AVAIL";

          return;
        }

        if (AsChar(export.PromptObligationType.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_LST_OBLIGATION_TYPES";

          return;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "RETADAA":
        export.PromptAdminAction.PromptField = "";

        if (IsEmpty(export.AdministrativeAction.Type1))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          var field = GetField(export.Required, "code");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "RETOBTL":
        export.PromptObligationType.PromptField = "";

        if (IsEmpty(export.Required.Code))
        {
          var field = GetField(export.Required, "code");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          if (ReadObligationType())
          {
            export.Required.Assign(entities.ObligationType);
          }

          var field = GetField(export.Starting, "effectiveDate");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "DISPLAY":
        // ************************************************************
        // Insert the USE statement to read for the exemptions.
        // ************************************************************
        UseLeReadEachObligationExempt();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Display.Flag = "Y";

          if (export.Exemptions.IsEmpty)
          {
            ExitState = "NO_EXEMPTIONS_FOR_CSE_PERSON";
          }
          else if (export.Exemptions.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
        else
        {
          export.Display.Flag = "N";

          if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;
          }
          else if (IsExitState("ADMINISTRATIVE_ACTION_NF"))
          {
            var field = GetField(export.AdministrativeAction, "type1");

            field.Error = true;
          }
          else if (IsExitState("FN0000_OBLIGATION_TYPE_NF"))
          {
            var field = GetField(export.Required, "code");

            field.Error = true;
          }
          else
          {
          }
        }

        break;
      case "HELP":
        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        if (local.Common.Count > 1)
        {
          for(export.Exemptions.Index = 0; export.Exemptions.Index < export
            .Exemptions.Count; ++export.Exemptions.Index)
          {
            if (!IsEmpty(export.Exemptions.Item.Common.SelectChar))
            {
              var field = GetField(export.Exemptions.Item.Common, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN1";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXMP":
        if (local.Common.Count > 1)
        {
          for(export.Exemptions.Index = 0; export.Exemptions.Index < export
            .Exemptions.Count; ++export.Exemptions.Index)
          {
            if (!IsEmpty(export.Exemptions.Item.Common.SelectChar))
            {
              var field = GetField(export.Exemptions.Item.Common, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN1";

          return;
        }

        export.AllObligations.Flag = "N";
        ExitState = "ECO_LNK_2_EXMP_ADM_ACT_EXEMPTION";

        break;
      case "SIGNOFF":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Percentage = source.Percentage;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveExport1ToExemptions(LeReadEachObligationExempt.Export.
    ExportGroup source, Export.ExemptionsGroup target)
  {
    target.ObligationType.Assign(source.DetailObligationType);
    target.Common.SelectChar = source.DetailCommon.SelectChar;
    target.AdministrativeAction.Type1 = source.DetailAdministrativeAction.Type1;
    target.CsePersonsWorkSet.FormattedName =
      source.DetailCsePersonsWorkSet.FormattedName;
    MoveObligation(source.DetailObligation, target.Obligation);
    target.ObligationAdmActionExemption.Assign(
      source.DetailObligationAdmActionExemption);
  }

  private static void MoveExport1ToLocal1(CabMatchCsePerson.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly but only head.");
      
    target.Detail.Assign(source.Detail);
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
  }

  private static void MoveObligationType1(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveObligationType2(ObligationType source,
    ObligationType target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.ConvertOption = source.ConvertOption;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabMatchCsePerson()
  {
    var useImport = new CabMatchCsePerson.Import();
    var useExport = new CabMatchCsePerson.Export();

    MoveCommon(local.SearchPerson, useImport.Search);
    MoveCsePersonsWorkSet4(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

    Call(CabMatchCsePerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(export.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = export.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
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

  private void UseLeReadEachObligationExempt()
  {
    var useImport = new LeReadEachObligationExempt.Import();
    var useExport = new LeReadEachObligationExempt.Export();

    useImport.Required.Code = export.Required.Code;
    useImport.Starting.EffectiveDate = export.Starting.EffectiveDate;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;

    Call(LeReadEachObligationExempt.Execute, useImport, useExport);

    MoveObligationType2(useExport.ObligationType, export.Required);
    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
    MoveCsePersonsWorkSet3(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    useExport.Export1.CopyTo(export.Exemptions, MoveExport1ToExemptions);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId", import.Required.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Description = db.GetNullableString(reader, 3);
        entities.ObligationType.Populated = true;
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
    /// <summary>A ExemptionsGroup group.</summary>
    [Serializable]
    public class ExemptionsGroup
    {
      /// <summary>
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
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
      /// A value of AdministrativeAction.
      /// </summary>
      [JsonPropertyName("administrativeAction")]
      public AdministrativeAction AdministrativeAction
      {
        get => administrativeAction ??= new();
        set => administrativeAction = value;
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
      /// A value of Obligation.
      /// </summary>
      [JsonPropertyName("obligation")]
      public Obligation Obligation
      {
        get => obligation ??= new();
        set => obligation = value;
      }

      /// <summary>
      /// A value of ObligationAdmActionExemption.
      /// </summary>
      [JsonPropertyName("obligationAdmActionExemption")]
      public ObligationAdmActionExemption ObligationAdmActionExemption
      {
        get => obligationAdmActionExemption ??= new();
        set => obligationAdmActionExemption = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType obligationType;
      private Common common;
      private AdministrativeAction administrativeAction;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Obligation obligation;
      private ObligationAdmActionExemption obligationAdmActionExemption;
    }

    /// <summary>
    /// A value of PromptAdminAction.
    /// </summary>
    [JsonPropertyName("promptAdminAction")]
    public Standard PromptAdminAction
    {
      get => promptAdminAction ??= new();
      set => promptAdminAction = value;
    }

    /// <summary>
    /// A value of PromptObligationType.
    /// </summary>
    [JsonPropertyName("promptObligationType")]
    public Standard PromptObligationType
    {
      get => promptObligationType ??= new();
      set => promptObligationType = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public ObligationType Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ObligationAdmActionExemption Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// Gets a value of Exemptions.
    /// </summary>
    [JsonIgnore]
    public Array<ExemptionsGroup> Exemptions => exemptions ??= new(
      ExemptionsGroup.Capacity);

    /// <summary>
    /// Gets a value of Exemptions for json serialization.
    /// </summary>
    [JsonPropertyName("exemptions")]
    [Computed]
    public IList<ExemptionsGroup> Exemptions_Json
    {
      get => exemptions;
      set => Exemptions.Assign(value);
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public Common Display
    {
      get => display ??= new();
      set => display = value;
    }

    private Standard promptAdminAction;
    private Standard promptObligationType;
    private ObligationType required;
    private ObligationAdmActionExemption starting;
    private AdministrativeAction administrativeAction;
    private Array<ExemptionsGroup> exemptions;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard standard;
    private SsnWorkArea ssnWorkArea;
    private Common display;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExemptionsGroup group.</summary>
    [Serializable]
    public class ExemptionsGroup
    {
      /// <summary>
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
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
      /// A value of AdministrativeAction.
      /// </summary>
      [JsonPropertyName("administrativeAction")]
      public AdministrativeAction AdministrativeAction
      {
        get => administrativeAction ??= new();
        set => administrativeAction = value;
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
      /// A value of Obligation.
      /// </summary>
      [JsonPropertyName("obligation")]
      public Obligation Obligation
      {
        get => obligation ??= new();
        set => obligation = value;
      }

      /// <summary>
      /// A value of ObligationAdmActionExemption.
      /// </summary>
      [JsonPropertyName("obligationAdmActionExemption")]
      public ObligationAdmActionExemption ObligationAdmActionExemption
      {
        get => obligationAdmActionExemption ??= new();
        set => obligationAdmActionExemption = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType obligationType;
      private Common common;
      private AdministrativeAction administrativeAction;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Obligation obligation;
      private ObligationAdmActionExemption obligationAdmActionExemption;
    }

    /// <summary>A Export1Group group.</summary>
    [Serializable]
    public class Export1Group
    {
      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailAdministrativeAction")]
      public AdministrativeAction DetailAdministrativeAction
      {
        get => detailAdministrativeAction ??= new();
        set => detailAdministrativeAction = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailObligationAdmActionExemption.
      /// </summary>
      [JsonPropertyName("detailObligationAdmActionExemption")]
      public ObligationAdmActionExemption DetailObligationAdmActionExemption
      {
        get => detailObligationAdmActionExemption ??= new();
        set => detailObligationAdmActionExemption = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType detailObligationType;
      private Common detailCommon;
      private AdministrativeAction detailAdministrativeAction;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private Obligation detailObligation;
      private ObligationAdmActionExemption detailObligationAdmActionExemption;
    }

    /// <summary>
    /// A value of SelectedObligationType.
    /// </summary>
    [JsonPropertyName("selectedObligationType")]
    public ObligationType SelectedObligationType
    {
      get => selectedObligationType ??= new();
      set => selectedObligationType = value;
    }

    /// <summary>
    /// A value of PromptAdminAction.
    /// </summary>
    [JsonPropertyName("promptAdminAction")]
    public Standard PromptAdminAction
    {
      get => promptAdminAction ??= new();
      set => promptAdminAction = value;
    }

    /// <summary>
    /// A value of PromptObligationType.
    /// </summary>
    [JsonPropertyName("promptObligationType")]
    public Standard PromptObligationType
    {
      get => promptObligationType ??= new();
      set => promptObligationType = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public ObligationType Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ObligationAdmActionExemption Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of AllObligations.
    /// </summary>
    [JsonPropertyName("allObligations")]
    public Common AllObligations
    {
      get => allObligations ??= new();
      set => allObligations = value;
    }

    /// <summary>
    /// A value of SelectedAdministrativeAction.
    /// </summary>
    [JsonPropertyName("selectedAdministrativeAction")]
    public AdministrativeAction SelectedAdministrativeAction
    {
      get => selectedAdministrativeAction ??= new();
      set => selectedAdministrativeAction = value;
    }

    /// <summary>
    /// A value of SelectedObligation.
    /// </summary>
    [JsonPropertyName("selectedObligation")]
    public Obligation SelectedObligation
    {
      get => selectedObligation ??= new();
      set => selectedObligation = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of SelectedObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("selectedObligationAdmActionExemption")]
    public ObligationAdmActionExemption SelectedObligationAdmActionExemption
    {
      get => selectedObligationAdmActionExemption ??= new();
      set => selectedObligationAdmActionExemption = value;
    }

    /// <summary>
    /// Gets a value of Exemptions.
    /// </summary>
    [JsonIgnore]
    public Array<ExemptionsGroup> Exemptions => exemptions ??= new(
      ExemptionsGroup.Capacity);

    /// <summary>
    /// Gets a value of Exemptions for json serialization.
    /// </summary>
    [JsonPropertyName("exemptions")]
    [Computed]
    public IList<ExemptionsGroup> Exemptions_Json
    {
      get => exemptions;
      set => Exemptions.Assign(value);
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<Export1Group> Export1 => export1 ??= new(
      Export1Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<Export1Group> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public Common Display
    {
      get => display ??= new();
      set => display = value;
    }

    private ObligationType selectedObligationType;
    private Standard promptAdminAction;
    private Standard promptObligationType;
    private ObligationType required;
    private ObligationAdmActionExemption starting;
    private Common allObligations;
    private AdministrativeAction selectedAdministrativeAction;
    private Obligation selectedObligation;
    private AdministrativeAction administrativeAction;
    private ObligationAdmActionExemption selectedObligationAdmActionExemption;
    private Array<ExemptionsGroup> exemptions;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard standard;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private Obligation obligation;
    private SsnWorkArea ssnWorkArea;
    private Array<Export1Group> export1;
    private Common display;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet detail;
    }

    /// <summary>
    /// A value of ClearObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("clearObligationAdmActionExemption")]
    public ObligationAdmActionExemption ClearObligationAdmActionExemption
    {
      get => clearObligationAdmActionExemption ??= new();
      set => clearObligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of ClearAdministrativeAction.
    /// </summary>
    [JsonPropertyName("clearAdministrativeAction")]
    public AdministrativeAction ClearAdministrativeAction
    {
      get => clearAdministrativeAction ??= new();
      set => clearAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ClearObligationType.
    /// </summary>
    [JsonPropertyName("clearObligationType")]
    public ObligationType ClearObligationType
    {
      get => clearObligationType ??= new();
      set => clearObligationType = value;
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
    /// A value of NoOfSsnMatchesFound.
    /// </summary>
    [JsonPropertyName("noOfSsnMatchesFound")]
    public Common NoOfSsnMatchesFound
    {
      get => noOfSsnMatchesFound ??= new();
      set => noOfSsnMatchesFound = value;
    }

    /// <summary>
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public CsePersonsWorkSet Saved
    {
      get => saved ??= new();
      set => saved = value;
    }

    /// <summary>
    /// A value of Exemptions1.
    /// </summary>
    [JsonPropertyName("exemptions1")]
    public Common Exemptions1
    {
      get => exemptions1 ??= new();
      set => exemptions1 = value;
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
    /// A value of SearchPerson.
    /// </summary>
    [JsonPropertyName("searchPerson")]
    public Common SearchPerson
    {
      get => searchPerson ??= new();
      set => searchPerson = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
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

    private ObligationAdmActionExemption clearObligationAdmActionExemption;
    private AdministrativeAction clearAdministrativeAction;
    private ObligationType clearObligationType;
    private TextWorkArea textWorkArea;
    private Common noOfSsnMatchesFound;
    private CsePersonsWorkSet saved;
    private Common exemptions1;
    private Common common;
    private Common searchPerson;
    private Array<LocalGroup> local1;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private ObligationType obligationType;
  }
#endregion
}
