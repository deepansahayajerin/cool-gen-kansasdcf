// Program: OE_HIPL_INSUR_COV_BY_PERSON_LIST, ID: 371855684, model: 746.
// Short name: SWEHIPLP
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
/// A program: OE_HIPL_INSUR_COV_BY_PERSON_LIST.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeHiplInsurCovByPersonList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HIPL_INSUR_COV_BY_PERSON_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHiplInsurCovByPersonList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHiplInsurCovByPersonList.
  /// </summary>
  public OeHiplInsurCovByPersonList(IContext context, Import import,
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
    // Sid		08/16/96	String Test Fixes.
    // R. Marchman	11/14/96	Add new security and next tran.
    // 
    // S.Johnson       03/01/1999      Fixed next tran
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      export.Case1.Number = import.Case1.Number;
      export.CsePerson.Number = import.CsePerson.Number;
      export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

      return;
    }

    // ---------------------------------
    // Move Imports to Exports
    // ---------------------------------
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.CsePersonPrompt.SelectChar = import.CsePersonPrompt.SelectChar;
    export.SelectedCase.Number = import.SelectedCase.Number;
    export.SelectedCsePerson.Number = import.SelectedCsePerson.Number;
    export.SelectedCsePersonsWorkSet.Assign(import.SelectedCsePersonsWorkSet);

    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DetHealthInsuranceCompany.Assign(
          import.Import1.Item.DetHealthInsuranceCompany);
        export.Export1.Update.SelectionPrompt.SelectChar =
          import.Import1.Item.SelectionPrompt.SelectChar;
        export.Export1.Update.DetHealthInsuranceCoverage.Assign(
          import.Import1.Item.DetHealthInsuranceCoverage);
        export.Export1.Update.DetPolicyHolder.Number =
          import.Import1.Item.DetPolicyHolder.Number;
        export.Export1.Update.PolicyHolderName.FormattedNameText =
          import.Import1.Item.PolicyHolderName.FormattedNameText;
        export.Export1.Update.DetInsurancActive.Flag =
          import.Import1.Item.DetInsurancActive.Flag;
        export.Export1.Update.DetCase.Number =
          import.Import1.Item.DetCase.Number;
        export.Export1.Update.DetCovered.Number =
          import.Import1.Item.DetCovered.Number;
        export.Export1.Update.CoveredPersonName.FormattedNameText =
          import.Import1.Item.CoveredPersonName.FormattedNameText;
        export.Export1.Next();
      }
    }

    if (!IsEmpty(export.Case1.Number))
    {
      local.TextWorkArea.Text10 = export.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.CsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.TextWorkArea.Text10;
    }

    local.SearchCase.Number = export.Case1.Number;
    local.SearchCsePerson.Number = export.CsePerson.Number;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
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

      if (!IsEmpty(export.Hidden.CaseNumber))
      {
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      }

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }

      local.SearchCase.Number = export.Case1.Number;
      local.SearchCsePerson.Number = export.CsePerson.Number;
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    if (!export.Export1.IsEmpty)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        switch(AsChar(export.Export1.Item.SelectionPrompt.SelectChar))
        {
          case 'S':
            // ---------------------------------------------
            // Check that identifying attributes in the line
            // selected are not blank.
            // ---------------------------------------------
            if (!Equal(global.Command, "RETURN"))
            {
              goto Test;
            }

            // ---------------------------------------------
            // If input key_value is EQUAL to SPACES or ZEROES
            // ---------------------------------------------
            if (export.SelectedDetHealthInsuranceCompany.Identifier == 0 && IsExitState
              ("ACO_NN0000_ALL_OK"))
            {
              export.SelectedDetCase.Number =
                export.Export1.Item.DetCase.Number;
              export.SelectedDetHealthInsuranceCompany.Assign(
                export.Export1.Item.DetHealthInsuranceCompany);
              export.SelectedDetHealthInsuranceCoverage.Assign(
                export.Export1.Item.DetHealthInsuranceCoverage);
              export.SelectedDetCovered.Number =
                export.Export1.Item.DetCovered.Number;
              export.Export1.Update.DetPolicyHolder.Number =
                export.Export1.Item.DetPolicyHolder.Number;
            }

            ++export.SelectionCount.Count;

            if (export.SelectionCount.Count > 1)
            {
              // ---------------------------------------------
              // Make selection character ERROR.
              // ---------------------------------------------
              var field1 =
                GetField(export.Export1.Item.SelectionPrompt, "selectChar");

              field1.Error = true;

              ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";
            }

            break;
          case ' ':
            break;
          default:
            var field =
              GetField(export.Export1.Item.SelectionPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }
      }
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(export.SelectedCsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = export.SelectedCsePersonsWorkSet.Number;
        export.CsePersonsWorkSet.FormattedName =
          export.SelectedCsePersonsWorkSet.FormattedName;
        local.SearchCsePerson.Number = export.SelectedCsePersonsWorkSet.Number;
      }

      export.CsePersonPrompt.SelectChar = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "LIST"))
    {
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
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SPACES":
        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

        // ---------------------------------------------
        // This command will only be executed if the
        // transaction is EXECUTE FIRST on a flow and
        // the COMMAND has not been set in the flow or
        // if the transaction is started from clear
        // screen.
        // ---------------------------------------------
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        // ---------------------------------------------
        // Identify selected view values for updates.
        // Verify that only one selection was made and
        // the selection character was valid ("S").
        // ---------------------------------------------
        if (AsChar(import.CsePersonPrompt.SelectChar) == 'S')
        {
          if (!IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }
        }
        else
        {
          var field = GetField(export.CsePersonPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      case "DISPLAY":
        // ---------------------------------------------
        // The CSE Person Number and/or the Case number
        // is required input by the user.
        // ---------------------------------------------
        if (!IsEmpty(export.CsePerson.Number))
        {
          UseOeCabCheckCaseMember3();

          if (!IsEmpty(local.WorkError.Flag))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;

            ExitState = "CSE_PERSON_NF";
          }
        }

        if (!IsEmpty(export.Case1.Number))
        {
          UseOeCabCheckCaseMember1();

          if (!IsEmpty(local.WorkError.Flag))
          {
            var field = GetField(export.Case1, "number");

            field.Error = true;

            ExitState = "CASE_NF";
          }
        }

        if (IsEmpty(export.Case1.Number) && IsEmpty(export.CsePerson.Number))
        {
          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          var field2 = GetField(export.CsePerson, "number");

          field2.Error = true;

          ExitState = "OE0000_CASE_OR_PERSON_NO_REQD";
        }

        // ---------------------------------------------
        // If both the CSE Person Number and the Case
        // number is entered check for valid case role.
        // ---------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!IsEmpty(export.CsePerson.Number) && !
            IsEmpty(export.Case1.Number))
          {
            UseOeCabCheckCaseMember2();

            if (!IsEmpty(local.WorkError.Flag))
            {
              var field1 = GetField(export.CsePerson, "number");

              field1.Error = true;

              var field2 = GetField(export.Case1, "number");

              field2.Error = true;

              ExitState = "OE0000_CASE_MEMBER_NE";
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.SelectionPrompt.SelectChar =
              local.RefreshCommon.SelectChar;
            export.Export1.Update.DetInsurancActive.Flag =
              local.RefreshCommon.Flag;
            export.Export1.Update.DetCase.Number = local.RefreshCase.Number;
            export.Export1.Update.DetCovered.Number =
              local.RefreshCsePerson.Number;
            export.Export1.Update.DetPolicyHolder.Number =
              local.RefreshCsePerson.Number;
            export.Export1.Update.CoveredPersonName.FormattedNameText =
              local.RefreshPersonName.FormattedNameText;
            export.Export1.Update.PolicyHolderName.FormattedNameText =
              local.RefreshPersonName.FormattedNameText;
            export.Export1.Update.DetHealthInsuranceCompany.Assign(
              local.RefreshHealthInsuranceCompany);
            export.Export1.Update.DetHealthInsuranceCoverage.Assign(
              local.RefreshHealthInsuranceCoverage);
          }

          return;
        }

        UseOeListAllHealthInsuCoverage();

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.DetCovered.Number))
          {
            local.CsePersons.Number = export.Export1.Item.DetCovered.Number;
            UseSiReadCsePerson();
            export.Export1.Update.CoveredPersonName.FormattedNameText =
              local.CsePersons.FormattedName;
          }

          if (!IsEmpty(export.Export1.Item.DetPolicyHolder.Number))
          {
            local.CsePersons.Number =
              export.Export1.Item.DetPolicyHolder.Number;
            UseSiReadCsePerson();
            export.Export1.Update.PolicyHolderName.FormattedNameText =
              local.CsePersons.FormattedName;
          }
        }

        if (!IsExitState("HEALTH_INSURANCE_COVERAGE_NF_RB"))
        {
          if (export.Export1.IsEmpty)
          {
            export.CsePersonsWorkSet.Assign(local.RefreshCsePersonsWorkSet);
            ExitState = "HEALTH_INSURANCE_COVERAGE_NF_RB";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }

        break;
      default:
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

  private static void MoveExport1(OeListAllHealthInsuCoverage.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetHealthInsuranceCompany.Assign(source.DetHealthInsuranceCompany);
    target.SelectionPrompt.SelectChar = source.DetWork.SelectChar;
    target.DetHealthInsuranceCoverage.Assign(source.DetHealthInsuranceCoverage);
    target.DetPolicyHolder.Number = source.DetPolicyHolder.Number;
    target.PolicyHolderName.FormattedNameText =
      source.PolicyHolderName.FormattedNameText;
    target.DetInsurancActive.Flag = source.DetInsurancActive.Flag;
    target.DetCase.Number = source.DetCase.Number;
    target.DetCovered.Number = source.DetCovered.Number;
    target.CoveredPersonName.FormattedNameText =
      source.CoveredPersonName.FormattedNameText;
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

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeCabCheckCaseMember1()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = local.SearchCase.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
  }

  private void UseOeCabCheckCaseMember2()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = local.SearchCase.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
  }

  private void UseOeCabCheckCaseMember3()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseOeListAllHealthInsuCoverage()
  {
    var useImport = new OeListAllHealthInsuCoverage.Import();
    var useExport = new OeListAllHealthInsuCoverage.Export();

    useImport.Case1.Number = local.SearchCase.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeListAllHealthInsuCoverage.Execute, useImport, useExport);

    export.Case1.Number = useExport.Case1.Number;
    export.CsePerson.Number = useExport.CsePerson.Number;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
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
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersons.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersons);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetHealthInsuranceCompany.
      /// </summary>
      [JsonPropertyName("detHealthInsuranceCompany")]
      public HealthInsuranceCompany DetHealthInsuranceCompany
      {
        get => detHealthInsuranceCompany ??= new();
        set => detHealthInsuranceCompany = value;
      }

      /// <summary>
      /// A value of SelectionPrompt.
      /// </summary>
      [JsonPropertyName("selectionPrompt")]
      public Common SelectionPrompt
      {
        get => selectionPrompt ??= new();
        set => selectionPrompt = value;
      }

      /// <summary>
      /// A value of DetHealthInsuranceCoverage.
      /// </summary>
      [JsonPropertyName("detHealthInsuranceCoverage")]
      public HealthInsuranceCoverage DetHealthInsuranceCoverage
      {
        get => detHealthInsuranceCoverage ??= new();
        set => detHealthInsuranceCoverage = value;
      }

      /// <summary>
      /// A value of DetPolicyHolder.
      /// </summary>
      [JsonPropertyName("detPolicyHolder")]
      public CsePerson DetPolicyHolder
      {
        get => detPolicyHolder ??= new();
        set => detPolicyHolder = value;
      }

      /// <summary>
      /// A value of PolicyHolderName.
      /// </summary>
      [JsonPropertyName("policyHolderName")]
      public OeWorkGroup PolicyHolderName
      {
        get => policyHolderName ??= new();
        set => policyHolderName = value;
      }

      /// <summary>
      /// A value of DetInsurancActive.
      /// </summary>
      [JsonPropertyName("detInsurancActive")]
      public Common DetInsurancActive
      {
        get => detInsurancActive ??= new();
        set => detInsurancActive = value;
      }

      /// <summary>
      /// A value of DetCase.
      /// </summary>
      [JsonPropertyName("detCase")]
      public Case1 DetCase
      {
        get => detCase ??= new();
        set => detCase = value;
      }

      /// <summary>
      /// A value of DetCovered.
      /// </summary>
      [JsonPropertyName("detCovered")]
      public CsePerson DetCovered
      {
        get => detCovered ??= new();
        set => detCovered = value;
      }

      /// <summary>
      /// A value of CoveredPersonName.
      /// </summary>
      [JsonPropertyName("coveredPersonName")]
      public OeWorkGroup CoveredPersonName
      {
        get => coveredPersonName ??= new();
        set => coveredPersonName = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private HealthInsuranceCompany detHealthInsuranceCompany;
      private Common selectionPrompt;
      private HealthInsuranceCoverage detHealthInsuranceCoverage;
      private CsePerson detPolicyHolder;
      private OeWorkGroup policyHolderName;
      private Common detInsurancActive;
      private Case1 detCase;
      private CsePerson detCovered;
      private OeWorkGroup coveredPersonName;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of SelectedCase.
    /// </summary>
    [JsonPropertyName("selectedCase")]
    public Case1 SelectedCase
    {
      get => selectedCase ??= new();
      set => selectedCase = value;
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
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Standard standard;
    private Case1 case1;
    private Case1 selectedCase;
    private CsePerson csePerson;
    private CsePerson selectedCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Common csePersonPrompt;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
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
      /// A value of DetHealthInsuranceCompany.
      /// </summary>
      [JsonPropertyName("detHealthInsuranceCompany")]
      public HealthInsuranceCompany DetHealthInsuranceCompany
      {
        get => detHealthInsuranceCompany ??= new();
        set => detHealthInsuranceCompany = value;
      }

      /// <summary>
      /// A value of SelectionPrompt.
      /// </summary>
      [JsonPropertyName("selectionPrompt")]
      public Common SelectionPrompt
      {
        get => selectionPrompt ??= new();
        set => selectionPrompt = value;
      }

      /// <summary>
      /// A value of DetHealthInsuranceCoverage.
      /// </summary>
      [JsonPropertyName("detHealthInsuranceCoverage")]
      public HealthInsuranceCoverage DetHealthInsuranceCoverage
      {
        get => detHealthInsuranceCoverage ??= new();
        set => detHealthInsuranceCoverage = value;
      }

      /// <summary>
      /// A value of DetPolicyHolder.
      /// </summary>
      [JsonPropertyName("detPolicyHolder")]
      public CsePerson DetPolicyHolder
      {
        get => detPolicyHolder ??= new();
        set => detPolicyHolder = value;
      }

      /// <summary>
      /// A value of PolicyHolderName.
      /// </summary>
      [JsonPropertyName("policyHolderName")]
      public OeWorkGroup PolicyHolderName
      {
        get => policyHolderName ??= new();
        set => policyHolderName = value;
      }

      /// <summary>
      /// A value of DetInsurancActive.
      /// </summary>
      [JsonPropertyName("detInsurancActive")]
      public Common DetInsurancActive
      {
        get => detInsurancActive ??= new();
        set => detInsurancActive = value;
      }

      /// <summary>
      /// A value of DetCase.
      /// </summary>
      [JsonPropertyName("detCase")]
      public Case1 DetCase
      {
        get => detCase ??= new();
        set => detCase = value;
      }

      /// <summary>
      /// A value of DetCovered.
      /// </summary>
      [JsonPropertyName("detCovered")]
      public CsePerson DetCovered
      {
        get => detCovered ??= new();
        set => detCovered = value;
      }

      /// <summary>
      /// A value of CoveredPersonName.
      /// </summary>
      [JsonPropertyName("coveredPersonName")]
      public OeWorkGroup CoveredPersonName
      {
        get => coveredPersonName ??= new();
        set => coveredPersonName = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private HealthInsuranceCompany detHealthInsuranceCompany;
      private Common selectionPrompt;
      private HealthInsuranceCoverage detHealthInsuranceCoverage;
      private CsePerson detPolicyHolder;
      private OeWorkGroup policyHolderName;
      private Common detInsurancActive;
      private Case1 detCase;
      private CsePerson detCovered;
      private OeWorkGroup coveredPersonName;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of SelectedCase.
    /// </summary>
    [JsonPropertyName("selectedCase")]
    public Case1 SelectedCase
    {
      get => selectedCase ??= new();
      set => selectedCase = value;
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
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectionCount.
    /// </summary>
    [JsonPropertyName("selectionCount")]
    public Common SelectionCount
    {
      get => selectionCount ??= new();
      set => selectionCount = value;
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
    /// A value of SelectedDetCase.
    /// </summary>
    [JsonPropertyName("selectedDetCase")]
    public Case1 SelectedDetCase
    {
      get => selectedDetCase ??= new();
      set => selectedDetCase = value;
    }

    /// <summary>
    /// A value of SelectedDetPolicyHldr.
    /// </summary>
    [JsonPropertyName("selectedDetPolicyHldr")]
    public CsePerson SelectedDetPolicyHldr
    {
      get => selectedDetPolicyHldr ??= new();
      set => selectedDetPolicyHldr = value;
    }

    /// <summary>
    /// A value of SelectedDetCovered.
    /// </summary>
    [JsonPropertyName("selectedDetCovered")]
    public CsePerson SelectedDetCovered
    {
      get => selectedDetCovered ??= new();
      set => selectedDetCovered = value;
    }

    /// <summary>
    /// A value of SelectedDetHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("selectedDetHealthInsuranceCompany")]
    public HealthInsuranceCompany SelectedDetHealthInsuranceCompany
    {
      get => selectedDetHealthInsuranceCompany ??= new();
      set => selectedDetHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of SelectedDetHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("selectedDetHealthInsuranceCoverage")]
    public HealthInsuranceCoverage SelectedDetHealthInsuranceCoverage
    {
      get => selectedDetHealthInsuranceCoverage ??= new();
      set => selectedDetHealthInsuranceCoverage = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Standard standard;
    private Case1 case1;
    private Case1 selectedCase;
    private CsePerson csePerson;
    private CsePerson selectedCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Common selectionCount;
    private Common csePersonPrompt;
    private Case1 selectedDetCase;
    private CsePerson selectedDetPolicyHldr;
    private CsePerson selectedDetCovered;
    private HealthInsuranceCompany selectedDetHealthInsuranceCompany;
    private HealthInsuranceCoverage selectedDetHealthInsuranceCoverage;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RefreshCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("refreshCsePersonsWorkSet")]
    public CsePersonsWorkSet RefreshCsePersonsWorkSet
    {
      get => refreshCsePersonsWorkSet ??= new();
      set => refreshCsePersonsWorkSet = value;
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
    /// A value of CsePersons.
    /// </summary>
    [JsonPropertyName("csePersons")]
    public CsePersonsWorkSet CsePersons
    {
      get => csePersons ??= new();
      set => csePersons = value;
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
    /// A value of RefreshCommon.
    /// </summary>
    [JsonPropertyName("refreshCommon")]
    public Common RefreshCommon
    {
      get => refreshCommon ??= new();
      set => refreshCommon = value;
    }

    /// <summary>
    /// A value of RefreshHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("refreshHealthInsuranceCoverage")]
    public HealthInsuranceCoverage RefreshHealthInsuranceCoverage
    {
      get => refreshHealthInsuranceCoverage ??= new();
      set => refreshHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of RefreshCsePerson.
    /// </summary>
    [JsonPropertyName("refreshCsePerson")]
    public CsePerson RefreshCsePerson
    {
      get => refreshCsePerson ??= new();
      set => refreshCsePerson = value;
    }

    /// <summary>
    /// A value of RefreshPersonName.
    /// </summary>
    [JsonPropertyName("refreshPersonName")]
    public OeWorkGroup RefreshPersonName
    {
      get => refreshPersonName ??= new();
      set => refreshPersonName = value;
    }

    /// <summary>
    /// A value of RefreshCase.
    /// </summary>
    [JsonPropertyName("refreshCase")]
    public Case1 RefreshCase
    {
      get => refreshCase ??= new();
      set => refreshCase = value;
    }

    /// <summary>
    /// A value of WorkError.
    /// </summary>
    [JsonPropertyName("workError")]
    public Common WorkError
    {
      get => workError ??= new();
      set => workError = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    private CsePersonsWorkSet refreshCsePersonsWorkSet;
    private TextWorkArea textWorkArea;
    private CsePersonsWorkSet csePersons;
    private HealthInsuranceCompany refreshHealthInsuranceCompany;
    private Common refreshCommon;
    private HealthInsuranceCoverage refreshHealthInsuranceCoverage;
    private CsePerson refreshCsePerson;
    private OeWorkGroup refreshPersonName;
    private Case1 refreshCase;
    private Common workError;
    private CsePerson searchCsePerson;
    private Case1 searchCase;
    private Common work;
  }
#endregion
}
