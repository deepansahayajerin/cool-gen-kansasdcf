// Program: OE_OBLM_OBLIGATION_ESTB_MENU, ID: 371791647, model: 746.
// Short name: SWEOBLMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_OBLM_OBLIGATION_ESTB_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeOblmObligationEstbMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_OBLM_OBLIGATION_ESTB_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeOblmObligationEstbMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeOblmObligationEstbMenu.
  /// </summary>
  public OeOblmObligationEstbMenu(IContext context, Import import, Export export)
    :
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
    // Date		Developer	Request #	Description
    // 01/24/95	Sid Chowdhary			Initial Code
    // 10/16/96        R. Marchman
    //         Add new security and next tran
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    MoveStandard(import.Standard, export.Standard);
    export.CurrentHCase.Number = import.CurrentHCase.Number;
    export.CurrentHCsePerson.Number = import.CurrentHCsePerson.Number;
    export.PersonNameH.FormattedName = import.PersonNameH.FormattedName;

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);

    // **** end   group A ****
    // ---------------------------------------------
    //         	N E X T   T R A N
    // Use the Common Action Block to nexttran to
    // another procedure.
    // ---------------------------------------------
    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // **** begin group B ****
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      local.NextTranInfo.CaseNumber = import.CurrentHCase.Number;
      local.NextTranInfo.CsePersonNumber = import.CurrentHCsePerson.Number;
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

    if (Equal(global.Command, "XXNEXTXX") || Equal(global.Command, "XXFMMENU"))
    {
      if (Equal(global.Command, "XXFMMENU"))
      {
        return;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // This is not always necessary in the case of a menu procedure..
        // ****
        export.CurrentHCase.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.CurrentHCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
        UseScCabNextTranGet();
      }

      return;
    }

    // **** end   group B ****
    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    UseScCabTestSecurity();

    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "ENTER":
        switch(export.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_PATM_MENU";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_PLOM_MENU";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_HINM_MENU";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_ALOM_MENU";

            break;
          default:
            var field = GetField(export.Standard, "menuOption");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_OPTION";

            break;
        }

        break;
      case "SIGNOFF":
        // **** begin group D ****
        UseScCabSignoff();

        // **** end   group D ****
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.MenuOption = source.MenuOption;
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
    MoveNextTranInfo(local.NextTranInfo, useImport.NextTranInfo);

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
    /// A value of CurrentHCase.
    /// </summary>
    [JsonPropertyName("currentHCase")]
    public Case1 CurrentHCase
    {
      get => currentHCase ??= new();
      set => currentHCase = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of CurrentHCsePerson.
    /// </summary>
    [JsonPropertyName("currentHCsePerson")]
    public CsePerson CurrentHCsePerson
    {
      get => currentHCsePerson ??= new();
      set => currentHCsePerson = value;
    }

    /// <summary>
    /// A value of PersonNameH.
    /// </summary>
    [JsonPropertyName("personNameH")]
    public CsePersonsWorkSet PersonNameH
    {
      get => personNameH ??= new();
      set => personNameH = value;
    }

    private Case1 currentHCase;
    private Standard standard;
    private NextTranInfo hidden;
    private CsePerson currentHCsePerson;
    private CsePersonsWorkSet personNameH;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CurrentHCase.
    /// </summary>
    [JsonPropertyName("currentHCase")]
    public Case1 CurrentHCase
    {
      get => currentHCase ??= new();
      set => currentHCase = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of CurrentHCsePerson.
    /// </summary>
    [JsonPropertyName("currentHCsePerson")]
    public CsePerson CurrentHCsePerson
    {
      get => currentHCsePerson ??= new();
      set => currentHCsePerson = value;
    }

    /// <summary>
    /// A value of PersonNameH.
    /// </summary>
    [JsonPropertyName("personNameH")]
    public CsePersonsWorkSet PersonNameH
    {
      get => personNameH ??= new();
      set => personNameH = value;
    }

    private Case1 currentHCase;
    private Standard standard;
    private NextTranInfo hidden;
    private CsePerson currentHCsePerson;
    private CsePersonsWorkSet personNameH;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private NextTranInfo nextTranInfo;
  }
#endregion
}
