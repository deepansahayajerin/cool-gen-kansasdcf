// Program: OE_ALOM_LOCATE_INTERFACES_MENU, ID: 371792544, model: 746.
// Short name: SWEALOMP
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
/// A program: OE_ALOM_LOCATE_INTERFACES_MENU.
/// </para>
/// <para>
/// Resp: OBLGEST	
/// This is a sub-menu used for Selection of Automated Locate Interface 
/// procedures. This Menu allows a flow to either FPLS(Federal Parent Locator
/// Service) procedure or 1099 (IRS Locate) procedure.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeAlomLocateInterfacesMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_ALOM_LOCATE_INTERFACES_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeAlomLocateInterfacesMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeAlomLocateInterfacesMenu.
  /// </summary>
  public OeAlomLocateInterfacesMenu(IContext context, Import import,
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
    // --------------------------------------------------------------------------
    // Date	   Developer		Description
    // 02/03/96   Sid Chowdhary	Initial Code
    // 08/18/96   T.O.Redmond		Remove security check if command is XXMENU
    // 09/03/97   T.O.Redmond		Add Pad with Zeroes
    // 10/16/96   R. Marchman          Add new security and next tran
    // 07/11/2000	PMcElderry
    // Added menu selections 5 and 6; moved views AFTER
    // call to sc_cab_next_tran_get and corrected view matches
    // 08/12/10   JHuss		CQ# 513.  Send person and case numbers to LOCA screen.
    // 08/25/10   LSS                  CQ# 21409.  Added menu selection 8.
    // --------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    MoveStandard(import.Standard, export.Standard);
    export.Current.Number = import.Current.Number;
    export.PersonName.FormattedName = import.PersonName.FormattedName;

    if (IsEmpty(import.Start.Number))
    {
      export.Start.Number = "";
    }
    else
    {
      local.TextWorkArea.Text10 = import.Start.Number;
      UseEabPadLeftWithZeros();
      export.Start.Number = local.TextWorkArea.Text10;
    }

    if (IsEmpty(import.Starting.Number))
    {
      export.PersonName.FormattedName = "";
      export.Starting.Number = "";
    }
    else
    {
      local.TextWorkArea.Text10 = import.Starting.Number;
      UseEabPadLeftWithZeros();
      export.Starting.Number = local.TextWorkArea.Text10;
    }

    // --------------------------------------------------------------
    // if the next tran info is not equal to spaces, this implies the
    // user requested a next tran action. now validate
    // --------------------------------------------------------------
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      local.NextTranInfo.CaseNumber = export.Start.Number;
      local.NextTranInfo.CsePersonNumber = export.Starting.Number;
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
        UseScCabNextTranGet();
        export.Start.Number = local.NextTranInfo.CaseNumber ?? Spaces(10);
        export.Starting.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
          (10);
      }

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "SPACES":
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
            ExitState = "ECO_XFR_MENU_TO_FPLS";

            break;
          case 2:
            ExitState = "ECO_XFR_MENU_TO_TEN9";

            break;
          case 3:
            ExitState = "ECO_LINK_MENU_PROACTIVE_MATCH";

            break;
          case 4:
            ExitState = "OE_FLOW_TO_FIDL";

            break;
          case 5:
            ExitState = "ECO_LNK_TO_LOCL";

            break;
          case 6:
            ExitState = "ECO_LNK_TO_LOCA";

            break;
          case 7:
            ExitState = "OE_XFR_TO_FCRV";

            break;
          case 8:
            ExitState = "OE_XFR_TO_CALS";

            break;
          default:
            var field = GetField(export.Standard, "menuOption");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_OPTION";

            break;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CsePerson Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Case1 Start
    {
      get => start ??= new();
      set => start = value;
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

    private CsePerson csePerson;
    private CsePersonsWorkSet personName;
    private CsePerson starting;
    private Case1 start;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CsePerson Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Case1 Start
    {
      get => start ??= new();
      set => start = value;
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
    private CsePerson starting;
    private Case1 start;
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
