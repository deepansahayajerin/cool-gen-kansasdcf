// Program: OE_PATM_GENETIC_WORK_URA_LR_MENU, ID: 371791970, model: 746.
// Short name: SWEPATMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_PATM_GENETIC_WORK_URA_LR_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OePatmGeneticWorkUraLrMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PATM_GENETIC_WORK_URA_LR_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OePatmGeneticWorkUraLrMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OePatmGeneticWorkUraLrMenu.
  /// </summary>
  public OePatmGeneticWorkUraLrMenu(IContext context, Import import,
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
    // Date		Developer	Request #	Description
    // 02/03/96	Sid Chowdhary			Initial Code
    // 02/05/96        Dale Brokaw-DIR               	Retrofit
    // 10/16/96        R. Marchman                     Add new security and next
    // tran
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // **** begin group B ****
    MoveStandard(import.Standard, export.Standard);
    export.Current.Number = import.Current.Number;
    export.PersonName.FormattedName = import.PersonName.FormattedName;
    export.StartCase.Number = import.StartCase.Number;
    export.StartCsePerson.Number = import.StartCsePerson.Number;

    if (!IsEmpty(import.Current.Number) && IsEmpty(import.StartCase.Number))
    {
      export.StartCase.Number = import.Current.Number;
    }

    if (IsEmpty(import.StartCsePerson.Number))
    {
      export.PersonName.FormattedName = "";
    }

    if (!IsEmpty(export.StartCase.Number))
    {
      local.TextWorkArea.Text10 = export.StartCase.Number;
      UseEabPadLeftWithZeros();
      export.StartCase.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.StartCsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.StartCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.StartCsePerson.Number = local.TextWorkArea.Text10;
    }

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
      local.NextTranInfo.CaseNumber = export.StartCase.Number;
      local.NextTranInfo.CsePersonNumber = export.StartCsePerson.Number;
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
      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // This is not always necessary in the case of a menu procedure..
        // ****
        export.Current.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.StartCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
        UseScCabNextTranGet();
        export.StartCase.Number = local.NextTranInfo.CaseNumber ?? Spaces(10);
        export.StartCsePerson.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
          (10);
      }

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // **** end   group B ****
    // ---------------------------------------------
    //         	N E X T   T R A N
    // Use the Common Action Block to nexttran to
    // another procedure.
    // ---------------------------------------------
    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
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
      case "SIGNOFF":
        // **** begin group D ****
        UseScCabSignoff();

        // **** end   group D ****
        break;
      case "ENTER":
        switch(export.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_MENU_TO_GTSC";

            break;
          case 2:
            ExitState = "ECO_XFR_MENU_TO_GTDS";

            break;
          case 3:
            ExitState = "ECO_XFR_MENU_TO_GTSL";

            break;
          case 4:
            ExitState = "ECO_XFR_MENU_TO_GTAL";

            break;
          case 5:
            ExitState = "ECO_XFR_MENU_TO_VEND";

            break;
          case 6:
            ExitState = "ECO_XFR_MENU_TO_VENL";

            break;
          case 7:
            ExitState = "ECO_XFR_MENU_TO_IMHH";

            break;
          case 8:
            ExitState = "ECO_XFR_MENU_TO_CURA";

            break;
          case 9:
            ExitState = "ECO_XFR_MENU_TO_URAH";

            break;
          case 10:
            ExitState = "ECO_XFR_MENU_TO_URAC";

            break;
          case 11:
            ExitState = "ECO_XFR_MENU_TO_WORK";

            break;
          case 12:
            ExitState = "ECO_XFR_MENU_TO_CSWL";

            break;
          case 13:
            ExitState = "ECO_XFR_MENU_TO_LGRQ";

            break;
          case 14:
            ExitState = "EXO_XFR_MENU_TO_UCOL";

            break;
          case 15:
            ExitState = "ECO_XFR_MENU_TO_UHMM";

            break;
          case 16:
            ExitState = "ECO_XFR_MENU_TO_URAL";

            break;
          default:
            var field = GetField(export.Standard, "menuOption");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_OPTION";

            break;
        }

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

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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
    /// A value of PersonName.
    /// </summary>
    [JsonPropertyName("personName")]
    public CsePersonsWorkSet PersonName
    {
      get => personName ??= new();
      set => personName = value;
    }

    /// <summary>
    /// A value of StartCsePerson.
    /// </summary>
    [JsonPropertyName("startCsePerson")]
    public CsePerson StartCsePerson
    {
      get => startCsePerson ??= new();
      set => startCsePerson = value;
    }

    /// <summary>
    /// A value of StartCase.
    /// </summary>
    [JsonPropertyName("startCase")]
    public Case1 StartCase
    {
      get => startCase ??= new();
      set => startCase = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Case1 Current
    {
      get => current ??= new();
      set => current = value;
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

    private CsePersonsWorkSet personName;
    private CsePerson startCsePerson;
    private Case1 startCase;
    private Case1 current;
    private Standard standard;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PersonName.
    /// </summary>
    [JsonPropertyName("personName")]
    public CsePersonsWorkSet PersonName
    {
      get => personName ??= new();
      set => personName = value;
    }

    /// <summary>
    /// A value of StartCsePerson.
    /// </summary>
    [JsonPropertyName("startCsePerson")]
    public CsePerson StartCsePerson
    {
      get => startCsePerson ??= new();
      set => startCsePerson = value;
    }

    /// <summary>
    /// A value of StartCase.
    /// </summary>
    [JsonPropertyName("startCase")]
    public Case1 StartCase
    {
      get => startCase ??= new();
      set => startCase = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Case1 Current
    {
      get => current ??= new();
      set => current = value;
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

    private CsePersonsWorkSet personName;
    private CsePerson startCsePerson;
    private Case1 startCase;
    private Case1 current;
    private Standard standard;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private TextWorkArea textWorkArea;
    private NextTranInfo nextTranInfo;
  }
#endregion
}
