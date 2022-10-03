// Program: FN_CAMM_CASH_MANAGEMENT_MENU, ID: 371727433, model: 746.
// Short name: SWECAMMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAMM_CASH_MANAGEMENT_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCammCashManagementMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAMM_CASH_MANAGEMENT_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCammCashManagementMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCammCashManagementMenu.
  /// </summary>
  public FnCammCashManagementMenu(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ----------------------------------------------------------------------------------------
    // Date 	  	Developer Name		Request #	Description
    // 02/01/96	Holly Kennedy-MTW			Source/Retrofits
    // 10/17/96	Holly Kennedy-MTW			New Security Next Tran Retrofit
    // 11/26/96	R. Marchman				Fix next tran
    // 12/19/98        M. Fangman
    // 
    // Add flows for EFT
    // ----------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    // Since the Cash_Receipt_Transmittal procedures are not ready, using Option
    // "2" or "11" causes the system to Abend. So , for the time being, until
    // these two procedures are genned and tested, these two options will be
    // treated as Invalid Options.
    // I have deleted 'Case "2"' and 'Case "11"' from the CASE-OF-COMMAND 
    // structure.
    // RB Mohapatra       02/06/1997
    // ----------------------------------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------------------
    // Use the following two exitstates when the "CAsh Transmittal" procedures 
    // will be ready.
    // RBM     02/06/1997
    // ------------------------------------------------------------------------------------------
    ExitState = "ECO_XFR_TO_CASH_TRANSMITTAL";
    ExitState = "ECO_XFR_TO_LIST_CASH_TRANSMITTL";
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR") || IsEmpty(global.Command))
    {
      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      return;

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // *****
    // Move imports to exports
    // *****
    export.Common.ActionEntry = import.Common.ActionEntry;

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
        switch(TrimEnd(export.Common.ActionEntry))
        {
          case "1":
            ExitState = "ECO_LNK_TO_CASH_RECEIPT";

            break;
          case "2":
            ExitState = "ECO_XFR_TO_REC_CRRC";

            break;
          case "3":
            ExitState = "ECO_XFR_TO_CRTB";

            break;
          case "4":
            ExitState = "ECO_LNK_TO_REFUND_COLLECTION";

            break;
          case "5":
            ExitState = "ECO_XFR_TO_RECORD_MISC_REFUNDS";

            break;
          case "6":
            ExitState = "ECO_LNK_TO_OFFSET_ADVANCEMENT";

            break;
          case "7":
            ExitState = "ECO_LNK_MTN_CR_SOURCE_ASSGN";

            break;
          case "10":
            ExitState = "ECO_LNK_LST_CASH_RECEIPTS";

            break;
          case "12":
            ExitState = "ECO_LNK_LST_CR_DETAIL";

            break;
          case "13":
            ExitState = "ECO_LNK_LST_UNDISTR_COLLCTN";

            break;
          case "14":
            ExitState = "ECO_LNK_LST_DEPOSITS";

            break;
          case "15":
            ExitState = "ECO_LNK_LST_SEL_REFUNDS";

            break;
          case "16":
            ExitState = "ECO_LNK_LST_ADVANCEMENTS";

            break;
          case "17":
            ExitState = "ECO_LNK_TO_BALANCE_INTERFACE";

            break;
          case "18":
            export.EftTransmissionFileInfo.TransmissionType = "I";
            ExitState = "ECO_LNK_LST_EFT_TRAN_INFO";

            break;
          case "19":
            export.ElectronicFundTransmission.TransmissionType = "I";
            export.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
            ExitState = "ECO_LNK_LST_EFT_TRANSMISSION";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_OPTION";

            var field = GetField(export.Common, "actionEntry");

            field.Error = true;

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

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Standard standard;
    private NextTranInfo hidden;
    private Common common;
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

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of EftTransmissionFileInfo.
    /// </summary>
    [JsonPropertyName("eftTransmissionFileInfo")]
    public EftTransmissionFileInfo EftTransmissionFileInfo
    {
      get => eftTransmissionFileInfo ??= new();
      set => eftTransmissionFileInfo = value;
    }

    private Standard standard;
    private Common common;
    private NextTranInfo hidden;
    private ElectronicFundTransmission electronicFundTransmission;
    private EftTransmissionFileInfo eftTransmissionFileInfo;
  }
#endregion
}
