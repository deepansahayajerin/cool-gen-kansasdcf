// Program: SP_INFM_INFRASTRUCTURE_MGMT_MENU, ID: 371751055, model: 746.
// Short name: SWEINFMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_INFM_INFRASTRUCTURE_MGMT_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpInfmInfrastructureMgmtMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_INFM_INFRASTRUCTURE_MGMT_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpInfmInfrastructureMgmtMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpInfmInfrastructureMgmtMenu.
  /// </summary>
  public SpInfmInfrastructureMgmtMenu(IContext context, Import import,
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
    // Date	  Developer	Request #	Description
    // 11/14/96 Alan Samuels                Complete Development
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.Hidden.Assign(local.NextTranInfo);

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    export.Selection.Count = import.Selection.Count;

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          local.NextTranInfo.Assign(import.Hidden);
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

        switch(import.Selection.Count)
        {
          case 1:
            ExitState = "ECO_XFR_TO_ATLM";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_ADLM";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_ASLM";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_TRLM";

            break;
          case 5:
            ExitState = "ECO_XFR_TO_ALLS";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_ALMN";

            break;
          case 7:
            ExitState = "ECO_XFR_TO_DRLM";

            break;
          case 8:
            ExitState = "ECO_XFR_TO_EVLS";

            break;
          case 9:
            ExitState = "ECO_XFR_TO_EVMN";

            break;
          case 10:
            ExitState = "ECO_XFR_TO_EVDT";

            break;
          case 11:
            ExitState = "ECO_XFR_TO_LSLM";

            break;
          case 12:
            ExitState = "ECO_XFR_TO_LTLM";

            break;
          case 13:
            ExitState = "ECO_XFR_TO_MENU";

            break;
          default:
            var field = GetField(export.Selection, "count");

            field.Error = true;

            ExitState = "SP0000_INVALID_VALUE_ENTERED";

            break;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "XXFMMENU":
        break;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        break;
    }
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
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public Common Selection
    {
      get => selection ??= new();
      set => selection = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common selection;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public Common Selection
    {
      get => selection ??= new();
      set => selection = value;
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

    private Common selection;
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

    private NextTranInfo nextTranInfo;
  }
#endregion
}
