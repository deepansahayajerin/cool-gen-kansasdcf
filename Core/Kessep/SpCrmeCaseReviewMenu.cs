// Program: SP_CRME_CASE_REVIEW_MENU, ID: 371800730, model: 746.
// Short name: SWECRMEP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CRME_CASE_REVIEW_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCrmeCaseReviewMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CRME_CASE_REVIEW_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCrmeCaseReviewMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCrmeCaseReviewMenu.
  /// </summary>
  public SpCrmeCaseReviewMenu(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer       req #   Description
    // 03/14/96 Alan Hackler            Retro fits
    // 10/16/96 R. Marchman             Add new security and next tran
    // 07/22/97 R. Grey		 Add review closed case
    // 03/03/99 N.Engoor                Changed name for the options. Added text
    // on the screen.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);
    export.ClosedCaseIndicator.Flag = import.ClosedCaseIndicator.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.Case1.Number = import.Case1.Number;
    MoveStandard(import.Standard, export.Standard);

    if (Equal(global.Command, "XXNEXTXX") || Equal(global.Command, "XXFMMENU"))
    {
      if (Equal(global.Command, "XXFMMENU"))
      {
        return;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ---------------
        // This is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // ---------------
        UseScCabNextTranGet();
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      }

      local.Prev.Command = global.Command;
      global.Command = "ENTER";
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ENTER":
        // if the next tran info is not equal to spaces, this implies the user 
        // requested a next tran action. now validate
        if (IsEmpty(import.Standard.NextTransaction))
        {
        }
        else
        {
          // -----------------
          // This is where you would set the local next_tran_info attributes to 
          // the import view attributes for the data to be passed to the next
          // transaction.
          // -----------------
          local.NextTranInfo.CaseNumber = export.Case1.Number;
          UseScCabNextTranPut();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.Standard, "nextTransaction");

            field.Error = true;
          }

          return;
        }

        if (!IsEmpty(export.Case1.Number))
        {
          local.TextWorkArea.Text10 = export.Case1.Number;
          UseEabPadLeftWithZeros();
          export.Case1.Number = local.TextWorkArea.Text10;

          if (ReadCase())
          {
            export.Case1.Number = entities.Existing.Number;
            export.ClosedCaseIndicator.Flag = "";

            if (AsChar(entities.Existing.Status) == 'C')
            {
              export.ClosedCaseIndicator.Flag = "Y";
            }
          }
          else
          {
            var field = GetField(export.Case1, "number");

            field.Error = true;

            ExitState = "CASE_NF";

            return;
          }
        }

        switch(export.Standard.MenuOption)
        {
          case 1:
            if (AsChar(export.ClosedCaseIndicator.Flag) == 'Y')
            {
              ExitState = "SP0000_CASE_CLOSED_TO_RVW";

              return;
            }

            export.PassHiddenReviewType.ActionEntry = "P";

            break;
          case 2:
            if (AsChar(export.ClosedCaseIndicator.Flag) == 'Y')
            {
              ExitState = "SP0000_CASE_CLOSED_TO_RVW";

              return;
            }

            export.PassHiddenReviewType.ActionEntry = "M";

            break;
          case 3:
            export.PassHiddenReviewType.ActionEntry = "R";

            break;
          default:
            if (IsEmpty(global.Command))
            {
              return;
            }

            if (!IsEmpty(local.Prev.Command))
            {
              return;
            }

            var field = GetField(export.Standard, "menuOption");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_OPTION";

            return;
        }

        export.Flag.Flag = "Y";

        if (!IsEmpty(export.Case1.Number))
        {
          ExitState = "ECO_XFR_TO_CR_INITIAL";
        }
        else
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        if (IsEmpty(global.Command))
        {
          return;
        }

        ExitState = "ACO_NE0000_INVALID_PF_KEY";

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

    export.Hidden.Assign(useExport.NextTranInfo);
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

  private bool ReadCase()
  {
    entities.Existing.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Existing.ClosureReason = db.GetNullableString(reader, 0);
        entities.Existing.Number = db.GetString(reader, 1);
        entities.Existing.Status = db.GetNullableString(reader, 2);
        entities.Existing.StatusDate = db.GetNullableDate(reader, 3);
        entities.Existing.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Existing.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ClosedCaseIndicator.
    /// </summary>
    [JsonPropertyName("closedCaseIndicator")]
    public Common ClosedCaseIndicator
    {
      get => closedCaseIndicator ??= new();
      set => closedCaseIndicator = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private Common closedCaseIndicator;
    private Case1 case1;
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
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// A value of ClosedCaseIndicator.
    /// </summary>
    [JsonPropertyName("closedCaseIndicator")]
    public Common ClosedCaseIndicator
    {
      get => closedCaseIndicator ??= new();
      set => closedCaseIndicator = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of PassHiddenReviewType.
    /// </summary>
    [JsonPropertyName("passHiddenReviewType")]
    public Common PassHiddenReviewType
    {
      get => passHiddenReviewType ??= new();
      set => passHiddenReviewType = value;
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

    private Common flag;
    private Common closedCaseIndicator;
    private Case1 case1;
    private Common passHiddenReviewType;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Common Prev
    {
      get => prev ??= new();
      set => prev = value;
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

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private Common prev;
    private TextWorkArea textWorkArea;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Case1 existing;
  }
#endregion
}
