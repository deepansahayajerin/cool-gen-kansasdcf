// Program: LE_ADMN_ADMINISTRATIVE_ACTN_MENU, ID: 371800916, model: 746.
// Short name: SWEADMNP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_ADMN_ADMINISTRATIVE_ACTN_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeAdmnAdministrativeActnMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_ADMN_ADMINISTRATIVE_ACTN_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAdmnAdministrativeActnMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAdmnAdministrativeActnMenu.
  /// </summary>
  public LeAdmnAdministrativeActnMenu(IContext context, Import import,
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
    // *********************************************
    // Date      Author              Reason
    // 03-21-95  S. Benton     Initial Dev
    // 04-17-95  S. Benton     Add Exit States
    // 02-06-96  a. hackler    retro fits
    // 09-14-98  P. Sharp      Removed COAG
    // *********************************************
    // 08-14-02  P. Phinney    PR 154979 - Change flow to OBLO from Display/None
    // TO  Execute/Display
    // *********************************************
    export.Hidden.Assign(import.Hidden);
    MoveStandard(import.Standard, export.Standard);
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_IDENTIFY_ADMIN_ACTN";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_CRED_CREDIT_REPORTNG";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_ADMIN_ACTION_EXEMPTN";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_ADMIN_APPEALS";

            break;
          case 5:
            ExitState = "ECO_XFR_TO_ADMIN_APPEAL_ADDRESS";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_ADMIN_APPEAL_HEARING";

            break;
          case 7:
            ExitState = "ECO_XFR_TO_POSITION_STATEMENT";

            break;
          case 8:
            ExitState = "ECO_XFR_TO_ADMIN_ACTIONS_AVAIL";

            break;
          case 9:
            ExitState = "ECO_XFR_TO_AACC_ADM_ACT_BY_CCNO";

            break;
          case 10:
            ExitState = "ECO_XFR_TO_OBL_ADM_ACT_BY_OBLGN";

            break;
          case 11:
            ExitState = "ECO_XFR_TO_OBL_ADM_ACT_BY_OBLGR";

            break;
          case 12:
            ExitState = "ECO_XFR_TO_LIST_ADMIN_APPEALS";

            break;
          case 13:
            ExitState = "ECO_XFR_2_EXEMPT_GRANTD_CSE_PERS";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_OPTION";

            break;
        }

        break;
      case "XXFMMENU":
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
    /// A value of XxxImportHidden.
    /// </summary>
    [JsonPropertyName("xxxImportHidden")]
    public Security2 XxxImportHidden
    {
      get => xxxImportHidden ??= new();
      set => xxxImportHidden = value;
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
    private Security2 xxxImportHidden;
    private NextTranInfo hidden;
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
    /// A value of XxxExportHidden.
    /// </summary>
    [JsonPropertyName("xxxExportHidden")]
    public Security2 XxxExportHidden
    {
      get => xxxExportHidden ??= new();
      set => xxxExportHidden = value;
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
    private Security2 xxxExportHidden;
    private NextTranInfo hidden;
  }
#endregion
}
