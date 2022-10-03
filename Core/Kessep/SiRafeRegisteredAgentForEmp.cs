// Program: SI_RAFE_REGISTERED_AGENT_FOR_EMP, ID: 371078378, model: 746.
// Short name: SWERAFEP
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
/// A program: SI_RAFE_REGISTERED_AGENT_FOR_EMP.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure adds, updates and deletes details about a CSE Persons' 
/// employer.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiRafeRegisteredAgentForEmp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RAFE_REGISTERED_AGENT_FOR_EMP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRafeRegisteredAgentForEmp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRafeRegisteredAgentForEmp.
  /// </summary>
  public SiRafeRegisteredAgentForEmp(IContext context, Import import,
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
    // ------------------------------------------------------------
    //     M A I N T E N A N C E    L O G
    // Date	  Developer	   Description
    // 10-21-95  H. Sharland	   Initial Development
    // 02-04-95  P. Elie	   Retrofit Security & Next Tran.
    // 11/03/96  G. Lofton - MTW  Add new security and removed
    // 				old.
    // 10/30/98 W. Campbell       Removed an IF stmt
    //                            "IF export registered_agent
    //                            identifier IS EQUAL TO SPACES"
    //                            which was preventing a positive
    //                            response to a DISPLAY command.
    // 10/30/98 W. Campbell       Added an IF stmt to
    //                            give a positive response to a
    //                            DISPLAY command.
    // 10/30/98 W. Campbell       Modified USE'd CAB
    //                            SI_CREATE_EMPL_REG_AGENT
    //                            so that it would only allow one
    //                            EMPL_REG_AGENT per employer.
    // --------------------------------------------
    // 05/26/99 W.Campbell        Replaced zd exit states.
    // ---------------------------------------------
    // 02/01/01 G Vandy           Added RETURN function key and logic.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Next.Number = import.Next.Number;
    MoveEmployer(import.Employer1, export.Employer1);
    export.Employer2.Assign(import.Employer2);
    export.EmployerPrompt.Flag = import.EmployerPrompt.Flag;
    export.RegAgentPrompt.Flag = import.RegAgentPrompt.Flag;
    MoveRegisteredAgent(import.RegisteredAgent, export.RegisteredAgent);
    export.RegisteredAgentAddress.Assign(import.RegisteredAgentAddress);
    export.HiddenEmployerRegisteredAgent.Identifier =
      import.HiddenEmployerRegisteredAgent.Identifier;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ---------------------------------------------
    //         	N E X T   T R A N
    // Use the CAB to nexttran to another procedure.
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      export.HiddenNextTranInfo.CaseNumber = import.Next.Number;
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
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (!IsEmpty(export.HiddenNextTranInfo.CaseNumber))
        {
          export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
            (10);
        }

        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "RTLIST") || Equal(global.Command, "RETURN"))
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

    // ---------------------------------------------
    //      P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "HELP":
        // ---------------------------------------------
        // All logic pertaining to using the  IEF help
        // facility should be placed here.
        // At this time, this is not available.
        // ---------------------------------------------
        break;
      case "EXIT":
        // --------------------------------------------
        // Allows the user to flow back to the previous
        // screen.
        // --------------------------------------------
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        switch(AsChar(export.EmployerPrompt.Flag))
        {
          case 'S':
            ++local.Common.Count;

            break;
          case ' ':
            break;
          default:
            ++local.Common.Count;

            var field = GetField(export.EmployerPrompt, "flag");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            break;
        }

        switch(AsChar(export.RegAgentPrompt.Flag))
        {
          case 'S':
            ++local.Common.Count;

            break;
          case ' ':
            break;
          default:
            ++local.Common.Count;

            var field = GetField(export.RegAgentPrompt, "flag");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // for the cases where you link from 1 procedure to another procedure, 
        // you must set the export_hidden security link_indicator to "L".
        // this will tell the called procedure that we are on a link and not a 
        // transfer.  Don't forget to do the view matching on the dialog design
        // screen.
        if (AsChar(export.EmployerPrompt.Flag) == 'S')
        {
          export.RegisteredAgent.Identifier = "";
          export.RegisteredAgent.Name = "";
          export.RegisteredAgentAddress.City = "";
          export.RegisteredAgentAddress.Identifier = "";
          export.RegisteredAgentAddress.State = "";
          export.RegisteredAgentAddress.Street1 = "";
          export.RegisteredAgentAddress.Street2 = "";
          export.RegisteredAgentAddress.ZipCode4 = "";
          export.RegisteredAgentAddress.ZipCode5 = "";
          ExitState = "ECO_LNK_TO_EMPLOYER";

          return;
        }
        else
        {
        }

        if (AsChar(export.RegAgentPrompt.Flag) == 'S')
        {
          ExitState = "ECO_LNK_TO_REGISTERED_AGENTS";
        }
        else
        {
        }

        break;
      case "ADD":
        if (import.Employer1.Identifier == 0)
        {
          var field = GetField(export.EmployerPrompt, "flag");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "EMPLOYER_REQUIRED";
          }
        }

        if (IsEmpty(import.RegisteredAgent.Identifier))
        {
          var field = GetField(export.RegAgentPrompt, "flag");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "REGISTERED_AGENT_REQUIRED";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ------------------------------------------------------------
        // Verify that there is not already a relationship in place for
        // these two choices.
        // End-date any other occurrences before creating the new
        // occurrence
        // ------------------------------------------------------------
        UseSiCreateEmplRegAgent();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "DELETE":
        UseSiDeleteEmployerRegAgent();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // 05/26/99 W.Campbell - Replaced zd exit states.
          // ---------------------------------------------
          ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
        }

        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Verify that all mandatory fields for a
        // display have been entered.
        // ---------------------------------------------
        if (import.Employer1.Identifier == 0)
        {
          var field1 = GetField(export.EmployerPrompt, "flag");

          field1.Protected = false;
          field1.Focused = true;

          var field2 = GetField(export.EmployerPrompt, "flag");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "EMPLOYER_REQUIRED";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(export.EmployerPrompt.Flag) == 'S')
        {
          export.EmployerPrompt.Flag = "";
        }
        else
        {
        }

        // --------------------------------------------
        // 10/30/98 W. Campbell  Removed an IF stmt
        // "IF export registered_agent identifier
        // IS EQUAL TO SPACES"
        // which was around the following USE stmt
        // to give a positive response to a DISPLAY
        // command.
        // --------------------------------------------
        UseSiReadEmployerRegAgent();

        if (IsExitState("EMPLOYER_REGISTERED_AGENT_NF") || IsExitState
          ("REGISTERED_AGENT_NF"))
        {
          var field1 = GetField(export.RegAgentPrompt, "flag");

          field1.Protected = false;
          field1.Focused = true;

          var field2 = GetField(export.RegAgentPrompt, "flag");

          field2.Error = true;
        }

        // --------------------------------------------
        // 10/30/98 W. Campbell  Added the following
        // IF stmt to give a positive response to a DISPLAY
        // command.
        // --------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "RTLIST":
        if (AsChar(export.RegAgentPrompt.Flag) == 'S')
        {
          export.RegAgentPrompt.Flag = "";
          ExitState = "ADD_NECESSARY_TO_ESTAB_RELATIONS";
        }
        else
        {
        }

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

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

  private static void MoveRegisteredAgent(RegisteredAgent source,
    RegisteredAgent target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveRegisteredAgentAddress(RegisteredAgentAddress source,
    RegisteredAgentAddress target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
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

    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateEmplRegAgent()
  {
    var useImport = new SiCreateEmplRegAgent.Import();
    var useExport = new SiCreateEmplRegAgent.Export();

    useImport.RegisteredAgent.Identifier = import.RegisteredAgent.Identifier;
    useImport.Employer.Identifier = import.Employer1.Identifier;

    Call(SiCreateEmplRegAgent.Execute, useImport, useExport);

    export.HiddenEmployerRegisteredAgent.Identifier =
      useExport.EmployerRegisteredAgent.Identifier;
  }

  private void UseSiDeleteEmployerRegAgent()
  {
    var useImport = new SiDeleteEmployerRegAgent.Import();
    var useExport = new SiDeleteEmployerRegAgent.Export();

    useImport.RegisteredAgent.Identifier = import.RegisteredAgent.Identifier;
    useImport.Employer.Identifier = import.Employer1.Identifier;
    useImport.EmployerRegisteredAgent.Identifier =
      export.HiddenEmployerRegisteredAgent.Identifier;

    Call(SiDeleteEmployerRegAgent.Execute, useImport, useExport);
  }

  private void UseSiReadEmployerRegAgent()
  {
    var useImport = new SiReadEmployerRegAgent.Import();
    var useExport = new SiReadEmployerRegAgent.Export();

    useImport.Employer.Identifier = import.Employer1.Identifier;

    Call(SiReadEmployerRegAgent.Execute, useImport, useExport);

    export.HiddenEmployerRegisteredAgent.Identifier =
      useExport.EmployerRegisteredAgent.Identifier;
    MoveRegisteredAgentAddress(useExport.RegisteredAgentAddress,
      export.RegisteredAgentAddress);
    MoveRegisteredAgent(useExport.RegisteredAgent, export.RegisteredAgent);
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
    /// A value of HiddenEmployerRegisteredAgent.
    /// </summary>
    [JsonPropertyName("hiddenEmployerRegisteredAgent")]
    public EmployerRegisteredAgent HiddenEmployerRegisteredAgent
    {
      get => hiddenEmployerRegisteredAgent ??= new();
      set => hiddenEmployerRegisteredAgent = value;
    }

    /// <summary>
    /// A value of RegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("registeredAgentAddress")]
    public RegisteredAgentAddress RegisteredAgentAddress
    {
      get => registeredAgentAddress ??= new();
      set => registeredAgentAddress = value;
    }

    /// <summary>
    /// A value of RegisteredAgent.
    /// </summary>
    [JsonPropertyName("registeredAgent")]
    public RegisteredAgent RegisteredAgent
    {
      get => registeredAgent ??= new();
      set => registeredAgent = value;
    }

    /// <summary>
    /// A value of RegAgentPrompt.
    /// </summary>
    [JsonPropertyName("regAgentPrompt")]
    public Common RegAgentPrompt
    {
      get => regAgentPrompt ??= new();
      set => regAgentPrompt = value;
    }

    /// <summary>
    /// A value of EmployerPrompt.
    /// </summary>
    [JsonPropertyName("employerPrompt")]
    public Common EmployerPrompt
    {
      get => employerPrompt ??= new();
      set => employerPrompt = value;
    }

    /// <summary>
    /// A value of Employer1.
    /// </summary>
    [JsonPropertyName("employer1")]
    public Employer Employer1
    {
      get => employer1 ??= new();
      set => employer1 = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Employer2.
    /// </summary>
    [JsonPropertyName("employer2")]
    public EmployerAddress Employer2
    {
      get => employer2 ??= new();
      set => employer2 = value;
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

    private EmployerRegisteredAgent hiddenEmployerRegisteredAgent;
    private RegisteredAgentAddress registeredAgentAddress;
    private RegisteredAgent registeredAgent;
    private Common regAgentPrompt;
    private Common employerPrompt;
    private Employer employer1;
    private Case1 next;
    private EmployerAddress employer2;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HiddenEmployerRegisteredAgent.
    /// </summary>
    [JsonPropertyName("hiddenEmployerRegisteredAgent")]
    public EmployerRegisteredAgent HiddenEmployerRegisteredAgent
    {
      get => hiddenEmployerRegisteredAgent ??= new();
      set => hiddenEmployerRegisteredAgent = value;
    }

    /// <summary>
    /// A value of RegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("registeredAgentAddress")]
    public RegisteredAgentAddress RegisteredAgentAddress
    {
      get => registeredAgentAddress ??= new();
      set => registeredAgentAddress = value;
    }

    /// <summary>
    /// A value of RegisteredAgent.
    /// </summary>
    [JsonPropertyName("registeredAgent")]
    public RegisteredAgent RegisteredAgent
    {
      get => registeredAgent ??= new();
      set => registeredAgent = value;
    }

    /// <summary>
    /// A value of RegAgentPrompt.
    /// </summary>
    [JsonPropertyName("regAgentPrompt")]
    public Common RegAgentPrompt
    {
      get => regAgentPrompt ??= new();
      set => regAgentPrompt = value;
    }

    /// <summary>
    /// A value of EmployerPrompt.
    /// </summary>
    [JsonPropertyName("employerPrompt")]
    public Common EmployerPrompt
    {
      get => employerPrompt ??= new();
      set => employerPrompt = value;
    }

    /// <summary>
    /// A value of Employer1.
    /// </summary>
    [JsonPropertyName("employer1")]
    public Employer Employer1
    {
      get => employer1 ??= new();
      set => employer1 = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Employer2.
    /// </summary>
    [JsonPropertyName("employer2")]
    public EmployerAddress Employer2
    {
      get => employer2 ??= new();
      set => employer2 = value;
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

    private EmployerRegisteredAgent hiddenEmployerRegisteredAgent;
    private RegisteredAgentAddress registeredAgentAddress;
    private RegisteredAgent registeredAgent;
    private Common regAgentPrompt;
    private Common employerPrompt;
    private Employer employer1;
    private Case1 next;
    private EmployerAddress employer2;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
  }
#endregion
}
