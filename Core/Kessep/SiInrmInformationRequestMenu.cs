// Program: SI_INRM_INFORMATION_REQUEST_MENU, ID: 371426956, model: 746.
// Short name: SWEINRMP
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
/// A program: SI_INRM_INFORMATION_REQUEST_MENU.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiInrmInformationRequestMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INRM_INFORMATION_REQUEST_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiInrmInformationRequestMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiInrmInformationRequestMenu.
  /// </summary>
  public SiInrmInformationRequestMenu(IContext context, Import import,
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
    //        M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 07-10-95  Ken Evans		Initial Development
    // 01-26-96  Bruce Moore		Retrofit
    // 06-27-96  Rao Mulpuri		INRD option changes
    // 10/16/96  R. Marchman		Add new security and next tran
    // ------------------------------------------------------------
    // *********************************************
    // *    This PRAD directs Inquiry requests to  *
    // *    the various destinations where Inquiry *
    // *    processing will take place.            *
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    MoveStandard(import.Standard, export.Standard);
    export.InformationRequest.Assign(import.InformationRequest);
    export.Next.Number = import.Next.Number;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the next_tran_info attributes to the import
      // view attributes for the data to be passed to the next transaction
      export.Hidden.CaseNumber = import.Next.Number;
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
      // values if the user is coming into this procedure on a next tran action.
      // This is not always necessary in the case of a menu procedure..
      UseScCabNextTranGet();
      export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_MAINTAIN_INQUIRY";

            break;
          case 2:
            if (IsEmpty(import.InformationRequest.ApplicantLastName) || IsEmpty
              (import.InformationRequest.ApplicantFirstName))
            {
              var field1 =
                GetField(export.InformationRequest, "applicantLastName");

              field1.Error = true;

              var field2 =
                GetField(export.InformationRequest, "applicantFirstName");

              field2.Error = true;

              ExitState = "NAME_NOT_ENTERED";
            }
            else
            {
              ExitState = "ECO_XFR_TO_APPL_LIST_INQUIRY";
            }

            break;
          case 3:
            if (IsEmpty(import.InformationRequest.ApplicantLastName) || IsEmpty
              (import.InformationRequest.ApplicantFirstName))
            {
              var field1 =
                GetField(export.InformationRequest, "applicantLastName");

              field1.Error = true;

              var field2 =
                GetField(export.InformationRequest, "applicantFirstName");

              field2.Error = true;

              ExitState = "NAME_NOT_ENTERED";
            }
            else
            {
              export.InformationRequest.CallerFirstName =
                import.InformationRequest.ApplicantFirstName ?? Spaces(12);
              export.InformationRequest.CallerLastName =
                import.InformationRequest.ApplicantLastName ?? "";
              export.InformationRequest.CallerMiddleInitial =
                import.InformationRequest.ApplicantMiddleInitial ?? Spaces(1);
              ExitState = "ECO_XFR_TO_CALLER_LIST_INQUIRY";
            }

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
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    private InformationRequest informationRequest;
    private Case1 next;
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
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    private InformationRequest informationRequest;
    private Case1 next;
    private NextTranInfo hidden;
  }
#endregion
}
