// Program: SI_PERM_PERSON_MANAGEMENT_MENU, ID: 371742797, model: 746.
// Short name: SWEPERMP
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
/// A program: SI_PERM_PERSON_MANAGEMENT_MENU.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists all of the available procedures for the CSE Person 
/// Management area
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiPermPersonManagementMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PERM_PERSON_MANAGEMENT_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPermPersonManagementMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPermPersonManagementMenu.
  /// </summary>
  public SiPermPersonManagementMenu(IContext context, Import import,
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
    // Date      Author              Reason
    // 1-4-95    Helen Sharland      Initial dev
    // 10/16/96  R. Marchman         Add new security and next tran
    // ------------------------------------------------------------
    // 03/03/00  C. Ott            Modified for PRWORA Paternity redesign.
    //                             Added transfer to CPAT screen.
    // ---------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // --------------------------------------------
    export.Case1.Number = import.Case1.Number;
    MoveStandard(import.Standard, export.Standard);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.Next.Number = import.Next.Number;

    var field = GetField(export.Standard, "menuOption");

    field.Protected = false;
    field.Focused = true;

    UseCabZeroFillNumber1();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field1 = GetField(export.Next, "number");

      field1.Error = true;

      return;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.Next.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // This is not always necessary in the case of a menu procedure..
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      }

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    UseCabZeroFillNumber2();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsExitState("CASE_NUMBER_NOT_NUMERIC"))
      {
        var field1 = GetField(export.Case1, "number");

        field1.Error = true;

        return;
      }
      else
      {
        var field1 = GetField(export.CsePersonsWorkSet, "number");

        field1.Error = true;

        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        // ---------------------------------------------
        // This interprets the option entered and will
        // flow to the appropriate screen based upon
        // that option using an exit state.
        // Add all options required for your menu to the
        // CASE OF statement.
        // Be sure you use the same exit states on your
        // dialog flows.
        // ---------------------------------------------
        switch(import.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_NAME_LIST";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_CASE_PARTICIPATION";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_CASE_COMPOSITION";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_LIST_CASES_BY_PERSON";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_AP_DETAILS";

            break;
          case 7:
            ExitState = "ECO_XFR_TO_AR_DETAILS";

            break;
          case 8:
            ExitState = "ECO_XFR_TO_CHILD_DETAILS";

            break;
          case 9:
            ExitState = "ECO_XFR_TO_OTHER_PEOPLE";

            break;
          case 10:
            ExitState = "ECO_XFR_TO_FOSTER_CARE_CHILD";

            break;
          case 11:
            ExitState = "ECO_XFR_TO_CASE_DETAILS";

            break;
          case 12:
            ExitState = "ECO_XFR_TO_CASE_ROLE";

            break;
          case 13:
            ExitState = "ECO_XFR_TO_ALT_SSN_AND_ALIAS";

            break;
          case 14:
            ExitState = "ECO_XFR_TO_PERSON_PROGRAM_MAINT";

            break;
          case 15:
            ExitState = "ECO_XFR_TO_ORGANIZATIONS";

            break;
          case 16:
            ExitState = "ECO_XFR_TO_CASU";

            break;
          case 17:
            ExitState = "ECO_LNK_TO_CPAT";

            break;
          case 18:
            ExitState = "ECO_LNK_TO_NCOP";

            break;
          default:
            var field1 = GetField(export.Standard, "menuOption");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_OPTION";

            break;
        }

        break;
      case "":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
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

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Case1.Number = useImport.Case1.Number;
    export.CsePersonsWorkSet.Number = useImport.CsePersonsWorkSet.Number;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private Case1 next;
    private Case1 case1;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private Case1 next;
    private Case1 case1;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private NextTranInfo hidden;
  }
#endregion
}
