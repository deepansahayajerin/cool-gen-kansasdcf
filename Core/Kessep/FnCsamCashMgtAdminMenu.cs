// Program: FN_CSAM_CASH_MGT_ADMIN_MENU, ID: 371800286, model: 746.
// Short name: SWECSAMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CSAM_CASH_MGT_ADMIN_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCsamCashMgtAdminMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CSAM_CASH_MGT_ADMIN_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCsamCashMgtAdminMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCsamCashMgtAdminMenu.
  /// </summary>
  public FnCsamCashMgtAdminMenu(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------
    // Date 	 Developer Name		Description
    // 02/21/96 Holly Kennedy-MTW	Source/Retrofits
    // 11/26/96 R. Marchman		Add new security and next tran
    // ------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (IsEmpty(global.Command))
    {
      return;
    }

    export.Hidden.Assign(import.Hidden);
    MoveStandard(import.Standard, export.Standard);

    // *****
    // Next Tran Logic
    // *****
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
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

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_XFR_TO_MTN_CASH_RCPT_SOURCE";

            break;
          case 2:
            ExitState = "ECO_LNK_MTN_CR_DELETE_RSN";
            export.MenuIndicator.Flag = "Y";

            break;
          case 3:
            ExitState = "ECO_LNK_MTN_CR_TYPE";
            ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

            break;
          case 4:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_LNK_MTN_CR_BALANCE_RSN";

            break;
          case 5:
            ExitState = "ECO_LNK_MTN_CR_DETAIL";
            ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_MTN_CASH_RCPT_STATUS";
            ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

            break;
          case 7:
            ExitState = "ECO_LNK_MTN_COLLECTION_TYPE";
            ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

            break;
          case 8:
            ExitState = "ECO_XFR_TO_MTN_CASH_RCPT_DTL_ST";
            ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

            break;
          case 9:
            ExitState = "ECO_LNK_MTN_CR_FEE_TYPE";

            break;
          case 10:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_LNK_LST_CASH_SOURCES";

            break;
          case 11:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_LNK_LST_CR_DELETE_RSN";

            break;
          case 12:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_LNK_TO_LST_CASH_RECIEPT_TYPE";

            break;
          case 13:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_LNK_LST_CR_BALANCE_RSN";

            break;
          case 14:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_LNK_TO_LST_CR_BAL_RSN_CODE";

            break;
          case 15:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_LNK_TO_LST_CASH_RCPT_STATUS";

            break;
          case 16:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_LNK_LST_COLLECTION_TYPES";

            break;
          case 17:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_LNK_TO_LST_CASH_RCPT_DTL_ST";

            break;
          case 18:
            export.MenuIndicator.Flag = "Y";
            ExitState = "ECO_LNK_LST_FEE_TYPES";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_OPTION";

            break;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

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
    /// A value of MenuIndicator.
    /// </summary>
    [JsonPropertyName("menuIndicator")]
    public Common MenuIndicator
    {
      get => menuIndicator ??= new();
      set => menuIndicator = value;
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

    private Common menuIndicator;
    private Standard standard;
    private NextTranInfo hidden;
  }
#endregion
}
