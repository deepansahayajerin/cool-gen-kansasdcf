// Program: QA_QAMN_QUALITY_ASSURANCE_MENU, ID: 372227931, model: 746.
// Short name: SWEQAMNP
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
/// A program: QA_QAMN_QUALITY_ASSURANCE_MENU.
/// </para>
/// <para>
/// RESP:   QA - Quality Assurance.
/// Quick Reference Screen Menu.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class QaQamnQualityAssuranceMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the QA_QAMN_QUALITY_ASSURANCE_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new QaQamnQualityAssuranceMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of QaQamnQualityAssuranceMenu.
  /// </summary>
  public QaQamnQualityAssuranceMenu(IContext context, Import import,
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
    // ** Maintenance Log
    // *******************************************************************
    // 02/18/1998     JF.Caillouet         Initial Design and Construction
    // 02/23/1999     JF.Caillouet         Added flow to QARM
    // 10/12/2009     A Hockman        Added flow to QUCK
    // *******************************************************************
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    export.Hidden.Assign(import.NextTranInfo);
    MoveStandard(import.Standard, export.Standard);

    if (!IsEmpty(export.Standard.NextTransaction))
    {
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

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        switch(export.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_LNK_TO_QARM";

            break;
          case 2:
            ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

            break;
          case 3:
            ExitState = "ECO_LNK_TO_QUCK";

            break;
          default:
            ExitState = "INVALID_OPTION_SELECTED";

            var field = GetField(export.Standard, "menuOption");

            field.Error = true;

            break;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        ExitState = "BXP_RETURN";

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
