// Program: CO_STBM_SUPPORT_TABLES_MENU, ID: 371800830, model: 746.
// Short name: SWESTBMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CO_STBM_SUPPORT_TABLES_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class CoStbmSupportTablesMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_STBM_SUPPORT_TABLES_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoStbmSupportTablesMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoStbmSupportTablesMenu.
  /// </summary>
  public CoStbmSupportTablesMenu(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // Date      Author              Reason
    // 04-26-95  S. Benton     Initial Dev
    // 01/23/96  a. Hackler    add Security/nexttran
    // 11/19/96  R. Marchman   Add new security and next tran.
    // *********************************************
    export.Hidden.Assign(import.Hidden);
    MoveStandard(import.Standard, export.Standard);

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
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // This is not always necessary in the case of a menu procedure..
        // ****
      }

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU_CSMM";

        break;
      case "":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_CO_MAINTAIN_CODE_NAM";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_CO_LIST_CODE_NAMES";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_CO_MAINTAIN_CODE_VAL";

            break;
          case 4:
            export.AllowChangingCodeName.Flag = "Y";
            ExitState = "ECO_XFR_TO_CO_LIST_CODE_VALUES";

            break;
          case 5:
            ExitState = "ECO_XFR_TO_CO_MNTN_COD_VAL_COMB";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_MAINTAIN_FIPS";

            break;
          case 7:
            ExitState = "ECO_XFR_TO_MAINTAIN_TRIBUNAL";

            break;
          case 8:
            ExitState = "ECO_XFR_TO_LIST_TRIBUNALS";

            break;
          case 9:
            ExitState = "ECO_XFR_TO_ADMIN_ACTIONS";

            break;
          case 10:
            ExitState = "ECO_XFR_TO_MAIN_ADM_ACTION_CRIT";

            break;
          case 11:
            ExitState = "ECO_XFR_MENU_TO_CARE";

            break;
          case 12:
            ExitState = "ECO_XFR_MENU_TO_CSSC";

            break;
          default:
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
    /// A value of AllowChangingCodeName.
    /// </summary>
    [JsonPropertyName("allowChangingCodeName")]
    public Common AllowChangingCodeName
    {
      get => allowChangingCodeName ??= new();
      set => allowChangingCodeName = value;
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

    private Common allowChangingCodeName;
    private Standard standard;
    private NextTranInfo hidden;
  }
#endregion
}
