// Program: OE_PLOM_PERSON_LOCATE_MENU, ID: 371792438, model: 746.
// Short name: SWEPLOMP
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
/// A program: OE_PLOM_PERSON_LOCATE_MENU.
/// </para>
/// <para>
/// Resp:OBLGEST
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OePlomPersonLocateMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PLOM_PERSON_LOCATE_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OePlomPersonLocateMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OePlomPersonLocateMenu.
  /// </summary>
  public OePlomPersonLocateMenu(IContext context, Import import, Export export):
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
    // Date	  Developer		Description
    // 02/03/96  Sid Chowdhary		Initial Code
    // 10/16/96  R. Marchman		Add new security and next tran
    // 09/01/10  RMathews  CQ553       Add FACL to menu
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    MoveStandard(import.Standard, export.Standard);
    export.Current.Number = import.Current.Number;
    export.StartCase.Number = import.StartCase.Number;
    export.PersonName.FormattedName = import.PersonName.FormattedName;
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
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      export.Hidden.CaseNumber = export.StartCase.Number;
      export.Hidden.CsePersonNumber = export.StartCsePerson.Number;
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
      // This is not always necessary in the case of a menu procedure..
      UseScCabNextTranGet();
      export.Current.Number = export.Hidden.CaseNumber ?? Spaces(10);
      export.StartCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
        (10);

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    UseScCabTestSecurity();

    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_MENU_TO_PCON";

            break;
          case 2:
            ExitState = "ECO_XFR_MENU_TO_PCOL";

            break;
          case 3:
            ExitState = "ECO_XFR_MENU_TO_MARH";

            break;
          case 4:
            ExitState = "ECO_XFR_MENU_TO_MARL";

            break;
          case 5:
            ExitState = "ECO_XFR_MENU_TO_APRE";

            break;
          case 6:
            ExitState = "ECO_XFR_MENU_TO_APRL";

            break;
          case 7:
            ExitState = "ECO_XFR_MENU_TO_CARS";

            break;
          case 8:
            ExitState = "ECO_XFR_MENU_TO_JAIL";

            break;
          case 9:
            ExitState = "ECO_XFR_TO_FACL";

            break;
          case 10:
            ExitState = "ECO_XFR_MENU_TO_MILI";

            break;
          case 11:
            ExitState = "ECO_XFR_TO_ADDRESS";

            break;
          case 12:
            ExitState = "ECO_XFR_TO_FOREIGN_ADDRESS";

            break;
          case 13:
            ExitState = "ECO_XFR_TO_INCS_INCOME_SRC_DTL";

            break;
          case 14:
            ExitState = "ECO_XFR_TO_INCOME_HISTORY";

            break;
          case 15:
            ExitState = "ECO_XFR_TO_INCOME_SOURCE_LIST1";

            break;
          case 16:
            ExitState = "ECO_XFR_TO_REG_AGENT_FOR_EMPL";

            break;
          case 17:
            ExitState = "ECO_XFR_TO_EMPLOYER_MAINTENANCE";

            break;
          case 18:
            ExitState = "ECO_XFR_TO_REGISTERED_AGENTS";

            break;
          case 19:
            ExitState = "ECO_XFER_TO_DEDE";

            break;
          case 20:
            ExitState = "ECO_XFR_TO_EMAIL_ADDR";

            break;
          case 21:
            ExitState = "ECO_XFR_TO_KDOR";

            break;
          case 22:
            ExitState = "ECO_XFR_TO_WKCL";

            break;
          case 23:
            ExitState = "ECO_XFR_TO_WKCD";

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

    private TextWorkArea textWorkArea;
  }
#endregion
}
