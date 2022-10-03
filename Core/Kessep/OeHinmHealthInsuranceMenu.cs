// Program: OE_HINM_HEALTH_INSURANCE_MENU, ID: 371791541, model: 746.
// Short name: SWEHINMP
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
/// A program: OE_HINM_HEALTH_INSURANCE_MENU.
/// </para>
/// <para>
/// Resp: OBLGEST
/// This is the Main Menu Procedure for the Health Insurance SCREENS.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeHinmHealthInsuranceMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HINM_HEALTH_INSURANCE_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHinmHealthInsuranceMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHinmHealthInsuranceMenu.
  /// </summary>
  public OeHinmHealthInsuranceMenu(IContext context, Import import,
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
    // 01/27/96      Sid Chowdhary - MTW    		Initial Code
    // 02/05/96      Sherri Newman - DIR               Retrofit
    // 10/16/96      R. Marchman - MTW                 Add new security and next
    // tran
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // Move Imports to Exports.
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

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
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

    if (Equal(global.Command, "XXNEXTXX") || Equal(global.Command, "XXFMMENU"))
    {
      if (Equal(global.Command, "XXFMMENU"))
      {
        return;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Current.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.StartCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
        UseScCabNextTranGet();
      }

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
            ExitState = "ECO_LNK_TO_HICO";

            break;
          case 2:
            ExitState = "ECO_LNK_TO_HICL";

            break;
          case 3:
            ExitState = "ECO_LNK_TO_HIPH";

            break;
          case 4:
            ExitState = "ECO_LNK_TO_HICP";

            break;
          case 5:
            ExitState = "ECO_LNK_TO_HIPL";

            break;
          case 6:
            ExitState = "ECO_LNK_TO_HICV1";

            break;
          case 7:
            ExitState = "ECO_XFR_MENU_TO_HIAV";

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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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

    private NextTranInfo nextTranInfo;
    private TextWorkArea textWorkArea;
  }
#endregion
}
