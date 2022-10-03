// Program: SI_DEDE_DELETE_DUPLICATE_EMPLOYR, ID: 371076569, model: 746.
// Short name: SWEDEDEP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_DEDE_DELETE_DUPLICATE_EMPLOYR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiDedeDeleteDuplicateEmployr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_DEDE_DELETE_DUPLICATE_EMPLOYR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiDedeDeleteDuplicateEmployr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiDedeDeleteDuplicateEmployr.
  /// </summary>
  public SiDedeDeleteDuplicateEmployr(IContext context, Import import,
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
    // ----------------------------------------------------------------------------------------------
    //     M A I N T E N A N C E    L O G
    // Date	  Developer   Request	Description
    // 01-23-01  GVandy      WR267	Initial Development
    // 05-21-01  GVandy      PR119962	Reset Headquarters, Worksite, and 
    // Registered Agent indicators
    // 				when returning from HQWS and RAFE.
    // ----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.HiddenCase.Number = import.HiddenCase.Number;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.CorrectCommon.SelectChar = import.CorrectCommon.SelectChar;
    export.CorrectEmployer.Assign(import.CorrectEmployer);
    export.CorrectStandard.PromptField = import.CorrectStandard.PromptField;
    export.CorrectEmployerAddress.Assign(import.CorrectEmployerAddress);
    export.CorrectEmploymentTies.Flag = import.CorrectEmploymentTies.Flag;
    export.CorrectHeadquarters.Flag = import.CorrectHeadquarters.Flag;
    export.CorrectRegisteredAgnt.Flag = import.CorrectRegisteredAgent.Flag;
    export.CorrectWorksite.Flag = import.CorrectWorksite.Flag;
    export.DuplicateCommon.SelectChar = import.DuplicateCommon.SelectChar;
    export.DuplicateEmployer.Assign(import.DuplicateEmployer);
    export.DuplicateStandard.PromptField = import.DuplicateStandard.PromptField;
    export.DuplicateEmployerAddress.Assign(import.DuplicateEmployerAddress);
    export.DuplicateEmploymentTies.Flag = import.DuplicateEmploymentTies.Flag;
    export.DuplicateHeadquarters.Flag = import.DuplicateHeadquarters.Flag;
    export.DuplicateRegisteredAgnt.Flag = import.DuplicateRegisteredAgnt.Flag;
    export.DuplicateWorksite.Flag = import.DuplicateWorksite.Flag;

    // ============================================================
    //             NEXTTRAN LOGIC
    // ============================================================
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      export.HiddenNextTranInfo.CaseNumber = export.HiddenCase.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.HiddenCsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
        export.HiddenCase.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
          (10);
      }

      return;
    }

    if (Equal(global.Command, "FROMEMPL"))
    {
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "DELETE"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "RAFE"))
    {
      if (IsEmpty(export.CorrectCommon.SelectChar) && IsEmpty
        (export.DuplicateCommon.SelectChar))
      {
        var field1 = GetField(export.CorrectCommon, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.DuplicateCommon, "selectChar");

        field2.Error = true;

        ExitState = "OE0000_SELECTION_MUST_BE_MADE";

        return;
      }

      if (!IsEmpty(export.CorrectCommon.SelectChar) && !
        IsEmpty(export.DuplicateCommon.SelectChar))
      {
        var field1 = GetField(export.CorrectCommon, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.DuplicateCommon, "selectChar");

        field2.Error = true;

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

        return;
      }

      switch(AsChar(export.CorrectCommon.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          if (export.CorrectEmployer.Identifier == 0)
          {
            var field1 = GetField(export.CorrectCommon, "selectChar");

            field1.Error = true;

            ExitState = "FN0000_SELECT_EMPLOYR_FROM_EMPL2";
          }

          break;
        default:
          var field = GetField(export.CorrectCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }

      switch(AsChar(export.DuplicateCommon.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          if (export.DuplicateEmployer.Identifier == 0)
          {
            var field1 = GetField(export.DuplicateCommon, "selectChar");

            field1.Error = true;

            ExitState = "FN0000_SELECT_EMPLOYR_FROM_EMPL2";
          }

          break;
        default:
          var field = GetField(export.DuplicateCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "FROMEMPL":
        UseSiDisplayDedeEmployer3();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field = GetField(export.DuplicateEmployer, "name");

          field.Error = true;
        }

        break;
      case "DISPLAY":
        if (export.CorrectEmployer.Identifier == 0)
        {
          var field = GetField(export.CorrectStandard, "promptField");

          field.Error = true;

          ExitState = "FN0000_SELECT_EMPLOYER_FROM_EMPL";
        }

        if (export.DuplicateEmployer.Identifier == 0)
        {
          var field = GetField(export.DuplicateStandard, "promptField");

          field.Error = true;

          ExitState = "FN0000_SELECT_EMPLOYER_FROM_EMPL";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseSiDisplayDedeEmployer5();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field = GetField(export.CorrectEmployer, "name");

          field.Error = true;
        }

        UseSiDisplayDedeEmployer4();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else
        {
          var field = GetField(export.DuplicateEmployer, "name");

          field.Error = true;
        }

        export.CorrectStandard.PromptField = "";
        export.DuplicateStandard.PromptField = "";

        break;
      case "DELETE":
        if (export.CorrectEmployer.Identifier == 0)
        {
          var field = GetField(export.CorrectStandard, "promptField");

          field.Error = true;

          ExitState = "FN0000_SELECT_EMPLOYR_FROM_EMPL2";
        }

        if (export.DuplicateEmployer.Identifier == 0)
        {
          var field = GetField(export.DuplicateStandard, "promptField");

          field.Error = true;

          ExitState = "FN0000_SELECT_EMPLOYR_FROM_EMPL2";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (export.CorrectEmployer.Identifier == export
          .DuplicateEmployer.Identifier)
        {
          var field1 = GetField(export.DuplicateEmployer, "name");

          field1.Error = true;

          var field2 = GetField(export.CorrectEmployer, "name");

          field2.Error = true;

          ExitState = "SI0000_EMPLOYERS_CANNOT_BE_SAME";

          return;
        }

        UseSiDeleteDedeEmployer();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }
        else
        {
          var field1 = GetField(export.CorrectEmployer, "name");

          field1.Error = true;

          var field2 = GetField(export.DuplicateEmployer, "name");

          field2.Error = true;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        if (IsEmpty(export.CorrectStandard.PromptField) && IsEmpty
          (export.DuplicateStandard.PromptField))
        {
          var field1 = GetField(export.CorrectStandard, "promptField");

          field1.Error = true;

          var field2 = GetField(export.DuplicateStandard, "promptField");

          field2.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          return;
        }

        if (!IsEmpty(export.CorrectStandard.PromptField) && !
          IsEmpty(export.DuplicateStandard.PromptField))
        {
          var field1 = GetField(export.CorrectStandard, "promptField");

          field1.Error = true;

          var field2 = GetField(export.DuplicateStandard, "promptField");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        switch(AsChar(export.CorrectStandard.PromptField))
        {
          case ' ':
            break;
          case 'S':
            break;
          default:
            var field = GetField(export.CorrectStandard, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.DuplicateStandard.PromptField))
        {
          case ' ':
            break;
          case 'S':
            break;
          default:
            var field = GetField(export.DuplicateStandard, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ExitState = "ECO_LNK_TO_EMPLOYER";

        break;
      case "RETEMPL":
        if (AsChar(export.DuplicateStandard.PromptField) == 'S')
        {
          export.DuplicateStandard.PromptField = "";

          if (import.FromEmpl.Identifier == 0)
          {
          }
          else
          {
            UseSiDisplayDedeEmployer1();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              var field = GetField(export.DuplicateEmployer, "name");

              field.Error = true;
            }
          }
        }

        if (AsChar(export.CorrectStandard.PromptField) == 'S')
        {
          export.CorrectStandard.PromptField = "";

          if (import.FromEmpl.Identifier == 0)
          {
          }
          else
          {
            UseSiDisplayDedeEmployer2();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              var field = GetField(export.CorrectEmployer, "name");

              field.Error = true;
            }
          }
        }

        break;
      case "RETLINK":
        if (AsChar(export.CorrectCommon.SelectChar) == 'S')
        {
          export.CorrectCommon.SelectChar = "";
        }
        else
        {
        }

        if (AsChar(export.DuplicateCommon.SelectChar) == 'S')
        {
          export.DuplicateCommon.SelectChar = "";
        }
        else
        {
        }

        // ----------------------------------------------------------------------------------------------
        // 05-21-01  GVandy      PR119962	Reset Headquarters, Worksite, and 
        // Registered Agent indicators
        // 				when returning from RAFE.
        // ----------------------------------------------------------------------------------------------
        if (export.CorrectEmployer.Identifier != 0)
        {
          UseSiDisplayDedeEmployer5();
        }

        if (export.DuplicateEmployer.Identifier != 0)
        {
          UseSiDisplayDedeEmployer4();
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "RAFE":
        if (AsChar(export.DuplicateCommon.SelectChar) == 'S')
        {
          MoveEmployer(export.DuplicateEmployer, export.ToRafeEmployer);
          export.ToRafeEmployerAddress.Assign(export.DuplicateEmployerAddress);
        }

        if (AsChar(export.CorrectCommon.SelectChar) == 'S')
        {
          MoveEmployer(export.CorrectEmployer, export.ToRafeEmployer);
          export.ToRafeEmployerAddress.Assign(export.CorrectEmployerAddress);
        }

        ExitState = "ECO_LNK_TO_RAFE";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
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

  private void UseSiDeleteDedeEmployer()
  {
    var useImport = new SiDeleteDedeEmployer.Import();
    var useExport = new SiDeleteDedeEmployer.Export();

    useImport.Correct.Identifier = export.CorrectEmployer.Identifier;
    useImport.Duplicate.Identifier = export.DuplicateEmployer.Identifier;

    Call(SiDeleteDedeEmployer.Execute, useImport, useExport);
  }

  private void UseSiDisplayDedeEmployer1()
  {
    var useImport = new SiDisplayDedeEmployer.Import();
    var useExport = new SiDisplayDedeEmployer.Export();

    useImport.Employer.Identifier = import.FromEmpl.Identifier;

    Call(SiDisplayDedeEmployer.Execute, useImport, useExport);

    export.DuplicateEmploymentTies.Flag = useExport.EmploymentTies.Flag;
    export.DuplicateRegisteredAgnt.Flag = useExport.RegisteredAgent.Flag;
    export.DuplicateWorksite.Flag = useExport.Worksite.Flag;
    export.DuplicateHeadquarters.Flag = useExport.Headquarters.Flag;
    export.DuplicateEmployer.Assign(useExport.Employer);
    export.DuplicateEmployerAddress.Assign(useExport.EmployerAddress);
  }

  private void UseSiDisplayDedeEmployer2()
  {
    var useImport = new SiDisplayDedeEmployer.Import();
    var useExport = new SiDisplayDedeEmployer.Export();

    useImport.Employer.Identifier = import.FromEmpl.Identifier;

    Call(SiDisplayDedeEmployer.Execute, useImport, useExport);

    export.CorrectEmploymentTies.Flag = useExport.EmploymentTies.Flag;
    export.CorrectRegisteredAgnt.Flag = useExport.RegisteredAgent.Flag;
    export.CorrectWorksite.Flag = useExport.Worksite.Flag;
    export.CorrectHeadquarters.Flag = useExport.Headquarters.Flag;
    export.CorrectEmployer.Assign(useExport.Employer);
    export.CorrectEmployerAddress.Assign(useExport.EmployerAddress);
  }

  private void UseSiDisplayDedeEmployer3()
  {
    var useImport = new SiDisplayDedeEmployer.Import();
    var useExport = new SiDisplayDedeEmployer.Export();

    useImport.Employer.Identifier = import.DuplicateEmployer.Identifier;

    Call(SiDisplayDedeEmployer.Execute, useImport, useExport);

    export.DuplicateEmploymentTies.Flag = useExport.EmploymentTies.Flag;
    export.DuplicateRegisteredAgnt.Flag = useExport.RegisteredAgent.Flag;
    export.DuplicateWorksite.Flag = useExport.Worksite.Flag;
    export.DuplicateHeadquarters.Flag = useExport.Headquarters.Flag;
    export.DuplicateEmployer.Assign(useExport.Employer);
    export.DuplicateEmployerAddress.Assign(useExport.EmployerAddress);
  }

  private void UseSiDisplayDedeEmployer4()
  {
    var useImport = new SiDisplayDedeEmployer.Import();
    var useExport = new SiDisplayDedeEmployer.Export();

    useImport.Employer.Identifier = export.DuplicateEmployer.Identifier;

    Call(SiDisplayDedeEmployer.Execute, useImport, useExport);

    export.DuplicateEmploymentTies.Flag = useExport.EmploymentTies.Flag;
    export.DuplicateRegisteredAgnt.Flag = useExport.RegisteredAgent.Flag;
    export.DuplicateWorksite.Flag = useExport.Worksite.Flag;
    export.DuplicateHeadquarters.Flag = useExport.Headquarters.Flag;
    export.DuplicateEmployer.Assign(useExport.Employer);
    export.DuplicateEmployerAddress.Assign(useExport.EmployerAddress);
  }

  private void UseSiDisplayDedeEmployer5()
  {
    var useImport = new SiDisplayDedeEmployer.Import();
    var useExport = new SiDisplayDedeEmployer.Export();

    useImport.Employer.Identifier = export.CorrectEmployer.Identifier;

    Call(SiDisplayDedeEmployer.Execute, useImport, useExport);

    export.CorrectEmploymentTies.Flag = useExport.EmploymentTies.Flag;
    export.CorrectRegisteredAgnt.Flag = useExport.RegisteredAgent.Flag;
    export.CorrectWorksite.Flag = useExport.Worksite.Flag;
    export.CorrectHeadquarters.Flag = useExport.Headquarters.Flag;
    export.CorrectEmployer.Assign(useExport.Employer);
    export.CorrectEmployerAddress.Assign(useExport.EmployerAddress);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
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
    /// A value of FromEmpl.
    /// </summary>
    [JsonPropertyName("fromEmpl")]
    public Employer FromEmpl
    {
      get => fromEmpl ??= new();
      set => fromEmpl = value;
    }

    /// <summary>
    /// A value of DuplicateEmploymentTies.
    /// </summary>
    [JsonPropertyName("duplicateEmploymentTies")]
    public Common DuplicateEmploymentTies
    {
      get => duplicateEmploymentTies ??= new();
      set => duplicateEmploymentTies = value;
    }

    /// <summary>
    /// A value of DuplicateRegisteredAgnt.
    /// </summary>
    [JsonPropertyName("duplicateRegisteredAgnt")]
    public Common DuplicateRegisteredAgnt
    {
      get => duplicateRegisteredAgnt ??= new();
      set => duplicateRegisteredAgnt = value;
    }

    /// <summary>
    /// A value of DuplicateWorksite.
    /// </summary>
    [JsonPropertyName("duplicateWorksite")]
    public Common DuplicateWorksite
    {
      get => duplicateWorksite ??= new();
      set => duplicateWorksite = value;
    }

    /// <summary>
    /// A value of DuplicateHeadquarters.
    /// </summary>
    [JsonPropertyName("duplicateHeadquarters")]
    public Common DuplicateHeadquarters
    {
      get => duplicateHeadquarters ??= new();
      set => duplicateHeadquarters = value;
    }

    /// <summary>
    /// A value of CorrectEmploymentTies.
    /// </summary>
    [JsonPropertyName("correctEmploymentTies")]
    public Common CorrectEmploymentTies
    {
      get => correctEmploymentTies ??= new();
      set => correctEmploymentTies = value;
    }

    /// <summary>
    /// A value of CorrectRegisteredAgent.
    /// </summary>
    [JsonPropertyName("correctRegisteredAgent")]
    public Common CorrectRegisteredAgent
    {
      get => correctRegisteredAgent ??= new();
      set => correctRegisteredAgent = value;
    }

    /// <summary>
    /// A value of CorrectWorksite.
    /// </summary>
    [JsonPropertyName("correctWorksite")]
    public Common CorrectWorksite
    {
      get => correctWorksite ??= new();
      set => correctWorksite = value;
    }

    /// <summary>
    /// A value of CorrectHeadquarters.
    /// </summary>
    [JsonPropertyName("correctHeadquarters")]
    public Common CorrectHeadquarters
    {
      get => correctHeadquarters ??= new();
      set => correctHeadquarters = value;
    }

    /// <summary>
    /// A value of CorrectCommon.
    /// </summary>
    [JsonPropertyName("correctCommon")]
    public Common CorrectCommon
    {
      get => correctCommon ??= new();
      set => correctCommon = value;
    }

    /// <summary>
    /// A value of CorrectEmployer.
    /// </summary>
    [JsonPropertyName("correctEmployer")]
    public Employer CorrectEmployer
    {
      get => correctEmployer ??= new();
      set => correctEmployer = value;
    }

    /// <summary>
    /// A value of CorrectEmployerAddress.
    /// </summary>
    [JsonPropertyName("correctEmployerAddress")]
    public EmployerAddress CorrectEmployerAddress
    {
      get => correctEmployerAddress ??= new();
      set => correctEmployerAddress = value;
    }

    /// <summary>
    /// A value of DuplicateCommon.
    /// </summary>
    [JsonPropertyName("duplicateCommon")]
    public Common DuplicateCommon
    {
      get => duplicateCommon ??= new();
      set => duplicateCommon = value;
    }

    /// <summary>
    /// A value of DuplicateEmployer.
    /// </summary>
    [JsonPropertyName("duplicateEmployer")]
    public Employer DuplicateEmployer
    {
      get => duplicateEmployer ??= new();
      set => duplicateEmployer = value;
    }

    /// <summary>
    /// A value of DuplicateEmployerAddress.
    /// </summary>
    [JsonPropertyName("duplicateEmployerAddress")]
    public EmployerAddress DuplicateEmployerAddress
    {
      get => duplicateEmployerAddress ??= new();
      set => duplicateEmployerAddress = value;
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
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of DuplicateStandard.
    /// </summary>
    [JsonPropertyName("duplicateStandard")]
    public Standard DuplicateStandard
    {
      get => duplicateStandard ??= new();
      set => duplicateStandard = value;
    }

    /// <summary>
    /// A value of CorrectStandard.
    /// </summary>
    [JsonPropertyName("correctStandard")]
    public Standard CorrectStandard
    {
      get => correctStandard ??= new();
      set => correctStandard = value;
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

    private CsePerson hiddenCsePerson;
    private Employer fromEmpl;
    private Common duplicateEmploymentTies;
    private Common duplicateRegisteredAgnt;
    private Common duplicateWorksite;
    private Common duplicateHeadquarters;
    private Common correctEmploymentTies;
    private Common correctRegisteredAgent;
    private Common correctWorksite;
    private Common correctHeadquarters;
    private Common correctCommon;
    private Employer correctEmployer;
    private EmployerAddress correctEmployerAddress;
    private Common duplicateCommon;
    private Employer duplicateEmployer;
    private EmployerAddress duplicateEmployerAddress;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 hiddenCase;
    private Standard duplicateStandard;
    private Standard correctStandard;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ToRafeEmployerAddress.
    /// </summary>
    [JsonPropertyName("toRafeEmployerAddress")]
    public EmployerAddress ToRafeEmployerAddress
    {
      get => toRafeEmployerAddress ??= new();
      set => toRafeEmployerAddress = value;
    }

    /// <summary>
    /// A value of ToHqwsEmployerAddress.
    /// </summary>
    [JsonPropertyName("toHqwsEmployerAddress")]
    public EmployerAddress ToHqwsEmployerAddress
    {
      get => toHqwsEmployerAddress ??= new();
      set => toHqwsEmployerAddress = value;
    }

    /// <summary>
    /// A value of ToRafeEmployer.
    /// </summary>
    [JsonPropertyName("toRafeEmployer")]
    public Employer ToRafeEmployer
    {
      get => toRafeEmployer ??= new();
      set => toRafeEmployer = value;
    }

    /// <summary>
    /// A value of ToHqwsEmployer.
    /// </summary>
    [JsonPropertyName("toHqwsEmployer")]
    public Employer ToHqwsEmployer
    {
      get => toHqwsEmployer ??= new();
      set => toHqwsEmployer = value;
    }

    /// <summary>
    /// A value of DuplicateEmploymentTies.
    /// </summary>
    [JsonPropertyName("duplicateEmploymentTies")]
    public Common DuplicateEmploymentTies
    {
      get => duplicateEmploymentTies ??= new();
      set => duplicateEmploymentTies = value;
    }

    /// <summary>
    /// A value of DuplicateRegisteredAgnt.
    /// </summary>
    [JsonPropertyName("duplicateRegisteredAgnt")]
    public Common DuplicateRegisteredAgnt
    {
      get => duplicateRegisteredAgnt ??= new();
      set => duplicateRegisteredAgnt = value;
    }

    /// <summary>
    /// A value of DuplicateWorksite.
    /// </summary>
    [JsonPropertyName("duplicateWorksite")]
    public Common DuplicateWorksite
    {
      get => duplicateWorksite ??= new();
      set => duplicateWorksite = value;
    }

    /// <summary>
    /// A value of DuplicateHeadquarters.
    /// </summary>
    [JsonPropertyName("duplicateHeadquarters")]
    public Common DuplicateHeadquarters
    {
      get => duplicateHeadquarters ??= new();
      set => duplicateHeadquarters = value;
    }

    /// <summary>
    /// A value of CorrectEmploymentTies.
    /// </summary>
    [JsonPropertyName("correctEmploymentTies")]
    public Common CorrectEmploymentTies
    {
      get => correctEmploymentTies ??= new();
      set => correctEmploymentTies = value;
    }

    /// <summary>
    /// A value of CorrectRegisteredAgnt.
    /// </summary>
    [JsonPropertyName("correctRegisteredAgnt")]
    public Common CorrectRegisteredAgnt
    {
      get => correctRegisteredAgnt ??= new();
      set => correctRegisteredAgnt = value;
    }

    /// <summary>
    /// A value of CorrectWorksite.
    /// </summary>
    [JsonPropertyName("correctWorksite")]
    public Common CorrectWorksite
    {
      get => correctWorksite ??= new();
      set => correctWorksite = value;
    }

    /// <summary>
    /// A value of CorrectHeadquarters.
    /// </summary>
    [JsonPropertyName("correctHeadquarters")]
    public Common CorrectHeadquarters
    {
      get => correctHeadquarters ??= new();
      set => correctHeadquarters = value;
    }

    /// <summary>
    /// A value of CorrectCommon.
    /// </summary>
    [JsonPropertyName("correctCommon")]
    public Common CorrectCommon
    {
      get => correctCommon ??= new();
      set => correctCommon = value;
    }

    /// <summary>
    /// A value of CorrectEmployer.
    /// </summary>
    [JsonPropertyName("correctEmployer")]
    public Employer CorrectEmployer
    {
      get => correctEmployer ??= new();
      set => correctEmployer = value;
    }

    /// <summary>
    /// A value of CorrectEmployerAddress.
    /// </summary>
    [JsonPropertyName("correctEmployerAddress")]
    public EmployerAddress CorrectEmployerAddress
    {
      get => correctEmployerAddress ??= new();
      set => correctEmployerAddress = value;
    }

    /// <summary>
    /// A value of DuplicateCommon.
    /// </summary>
    [JsonPropertyName("duplicateCommon")]
    public Common DuplicateCommon
    {
      get => duplicateCommon ??= new();
      set => duplicateCommon = value;
    }

    /// <summary>
    /// A value of DuplicateEmployer.
    /// </summary>
    [JsonPropertyName("duplicateEmployer")]
    public Employer DuplicateEmployer
    {
      get => duplicateEmployer ??= new();
      set => duplicateEmployer = value;
    }

    /// <summary>
    /// A value of DuplicateEmployerAddress.
    /// </summary>
    [JsonPropertyName("duplicateEmployerAddress")]
    public EmployerAddress DuplicateEmployerAddress
    {
      get => duplicateEmployerAddress ??= new();
      set => duplicateEmployerAddress = value;
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
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of DuplicateStandard.
    /// </summary>
    [JsonPropertyName("duplicateStandard")]
    public Standard DuplicateStandard
    {
      get => duplicateStandard ??= new();
      set => duplicateStandard = value;
    }

    /// <summary>
    /// A value of CorrectStandard.
    /// </summary>
    [JsonPropertyName("correctStandard")]
    public Standard CorrectStandard
    {
      get => correctStandard ??= new();
      set => correctStandard = value;
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

    private CsePerson hiddenCsePerson;
    private EmployerAddress toRafeEmployerAddress;
    private EmployerAddress toHqwsEmployerAddress;
    private Employer toRafeEmployer;
    private Employer toHqwsEmployer;
    private Common duplicateEmploymentTies;
    private Common duplicateRegisteredAgnt;
    private Common duplicateWorksite;
    private Common duplicateHeadquarters;
    private Common correctEmploymentTies;
    private Common correctRegisteredAgnt;
    private Common correctWorksite;
    private Common correctHeadquarters;
    private Common correctCommon;
    private Employer correctEmployer;
    private EmployerAddress correctEmployerAddress;
    private Common duplicateCommon;
    private Employer duplicateEmployer;
    private EmployerAddress duplicateEmployerAddress;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 hiddenCase;
    private Standard duplicateStandard;
    private Standard correctStandard;
    private Standard standard;
  }
#endregion
}
