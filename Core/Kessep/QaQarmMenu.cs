// Program: QA_QARM_MENU, ID: 372233020, model: 746.
// Short name: SWEQARMP
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
/// A program: QA_QARM_MENU.
/// </para>
/// <para>
/// RESP:   QA - Quality Assurance.
/// Quick Reference Screen Menu.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class QaQarmMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the QA_QARM_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new QaQarmMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of QaQarmMenu.
  /// </summary>
  public QaQarmMenu(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ** Maintenance Log
    // *******************************************************************
    // 10/28/1998     JF.Caillouet         Initial Design and Construction
    // *******************************************************************
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Search.Number = import.Search.Number;
    export.Hidden.Assign(import.NextTranInfo);
    MoveStandard(import.Standard, export.Standard);

    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(export.Standard.NextTransaction))
      {
        export.Hidden.CaseNumber = export.Search.Number;
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
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.Search.Number = export.Hidden.CaseNumber ?? Spaces(10);

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

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (IsEmpty(global.Command))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        switch(export.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_QAPD";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_QINC";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_QARD";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_QCHD";

            break;
          case 5:
            ExitState = "ECO_XFR_TO_QDBT";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_QCOL";

            break;
          default:
            ExitState = "INVALID_OPTION_SELECTED";

            var field = GetField(export.Standard, "menuOption");

            field.Error = true;

            break;
        }

        break;
      case "NAME":
        ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        ExitState = "BXP_RETURN";

        break;
      case "BYPASS":
        break;
      case "":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Case1 Search
    {
      get => search ??= new();
      set => search = value;
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

    private Standard standard;
    private Common common;
    private Case1 search;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Case1 Search
    {
      get => search ??= new();
      set => search = value;
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
    private Common common;
    private Case1 search;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Case1 Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    private Case1 prompt;
  }
#endregion
}
