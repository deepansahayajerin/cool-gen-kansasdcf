// Program: OE_URAA_URA_ADJUSTMENT, ID: 372637451, model: 746.
// Short name: SWEURAAP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_URAA_URA_ADJUSTMENT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeUraaUraAdjustment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_URAA_URA_ADJUSTMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUraaUraAdjustment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUraaUraAdjustment.
  /// </summary>
  public OeUraaUraAdjustment(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	      CHG REQ# DESCRIPTION
    // Madhu Kumar      05-16-2000   Initial Code
    // Fangman          08-04-2000   Added code for oldest debt & redid I/O for 
    // efficiency.
    // January, 2002 - M Brown - Work Order Number: 010504 - Retro Processing.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // -----------------------------------------------------------
    // Move imports to exports
    // -----------------------------------------------------------
    // -----------------------------------------------------------
    // Static display-only fields
    // -----------------------------------------------------------
    export.ImHousehold.AeCaseNo = import.ImHousehold.AeCaseNo;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.ImHholdMember.Number = import.CsePerson.Number;
    export.ImHouseholdMbrMnthlySum.Relationship =
      import.ImHouseholdMbrMnthlySum.Relationship;

    // January, 2002 - M Brown - Work Order Number: 010504 - Added obligation 
    // fields that may be populated by the create adjustment cab.
    // If they are populated, a flow to COLP is valid.
    // *******************************************************************
    export.Obligation.Assign(import.Obligation);
    export.ObligationType.Assign(import.ObligationType);
    export.ObligorCsePerson.Number = import.ObligorCsePerson.Number;
    export.ObligorCsePersonsWorkSet.Number =
      import.ObligorCsePersonsWorkSet.Number;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // -----------------------------------------------------------
    // Enterable fields
    // -----------------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveDateWorkArea(import.ForAdjustments, export.ForAdjustments);
    export.ImHouseholdMbrMnthlyAdj.Assign(import.ImHouseholdMbrMnthlyAdj);

    // -----------------------------------------------------------
    // Dynamic display-only fields
    // -----------------------------------------------------------
    MoveDateWorkArea(import.FirstAfGrant, export.FirstAfGrant);
    MoveDateWorkArea(import.FirstMedGrant, export.FirstMedGrant);
    export.Tot.MbrAfGrant.TotalCurrency = import.Tot.MbrAfGrant.TotalCurrency;
    export.Tot.MbrAfColl.TotalCurrency = import.Tot.MbrAfColl.TotalCurrency;
    export.Tot.MbrAfAdj.TotalCurrency = import.Tot.MbrAfAdj.TotalCurrency;
    export.Tot.MbrAfUra.TotalCurrency = import.Tot.MbrAfUra.TotalCurrency;
    export.Tot.MbrMedGrant.TotalCurrency = import.Tot.MbrMedGrant.TotalCurrency;
    export.Tot.MbrMedColl.TotalCurrency = import.Tot.MbrMedColl.TotalCurrency;
    export.Tot.MbrMedAdj.TotalCurrency = import.Tot.MbrMedAdj.TotalCurrency;
    export.Tot.MbrMedUra.TotalCurrency = import.Tot.MbrMedUra.TotalCurrency;
    export.Tot.HhAfGrant.TotalCurrency = import.Tot.HhAfGrant.TotalCurrency;
    export.Tot.HhAfColl.TotalCurrency = import.Tot.HhAfColl.TotalCurrency;
    export.Tot.HhAfAdj.TotalCurrency = import.Tot.HhAfAdj.TotalCurrency;
    export.Tot.HhAfUra.TotalCurrency = import.Tot.HhAfUra.TotalCurrency;
    export.Tot.HhMedGrant.TotalCurrency = import.Tot.HhMedGrant.TotalCurrency;
    export.Tot.HhMedColl.TotalCurrency = import.Tot.HhMedColl.TotalCurrency;
    export.Tot.HhMedAdj.TotalCurrency = import.Tot.HhMedAdj.TotalCurrency;
    export.Tot.HhMedUra.TotalCurrency = import.Tot.HhMedUra.TotalCurrency;
    export.Tot.TotMbrAfGrant.TotalCurrency =
      import.Tot.TotMbrAfGrant.TotalCurrency;
    export.Tot.TotMbrAfColl.TotalCurrency =
      import.Tot.TotMbrAfColl.TotalCurrency;
    export.Tot.TotMbrAfAdj.TotalCurrency = import.Tot.TotMbrAfAdj.TotalCurrency;
    export.Tot.TotMbrAfUra.TotalCurrency = import.Tot.TotMbrAfUra.TotalCurrency;
    export.Tot.TotMbrMedGrant.TotalCurrency =
      import.Tot.TotMbrMedGrant.TotalCurrency;
    export.Tot.TotMbrMedColl.TotalCurrency =
      import.Tot.TotMbrMedColl.TotalCurrency;
    export.Tot.TotMbrMedAdj.TotalCurrency =
      import.Tot.TotMbrMedAdj.TotalCurrency;
    export.Tot.TotMbrMedUra.TotalCurrency =
      import.Tot.TotMbrMedUra.TotalCurrency;
    export.Tot.TotHhAfGrant.TotalCurrency =
      import.Tot.TotHhAfGrant.TotalCurrency;
    export.Tot.TotHhAfColl.TotalCurrency = import.Tot.TotHhAfColl.TotalCurrency;
    export.Tot.TotHhAfAdj.TotalCurrency = import.Tot.TotHhAfAdj.TotalCurrency;
    export.Tot.TotHhAfUra.TotalCurrency = import.Tot.TotHhAfUra.TotalCurrency;
    export.Tot.TotHhMedGrant.TotalCurrency =
      import.Tot.TotHhMedGrant.TotalCurrency;
    export.Tot.TotHhMedColl.TotalCurrency =
      import.Tot.TotHhMedColl.TotalCurrency;
    export.Tot.TotHhMedAdj.TotalCurrency = import.Tot.TotHhMedAdj.TotalCurrency;
    export.Tot.TotHhMedUra.TotalCurrency = import.Tot.TotHhMedUra.TotalCurrency;
    export.CollProtExists.Flag = import.CollProtExists.Flag;

    if (!IsEmpty(import.CsePerson.Number))
    {
      export.CsePersonsWorkSet.Number = import.CsePerson.Number;
    }

    if (import.Selected.Month == 0 || import.Selected.Year == 0)
    {
    }
    else
    {
      export.ForAdjustments.Month = import.Selected.Month;
      export.ForAdjustments.Year = import.Selected.Year;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";
      MoveCsePersonsWorkSet(local.InitializedCsePersonsWorkSet,
        export.CsePersonsWorkSet);

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
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
      UseScCabNextTranGet();
    }

    if (Equal(global.Command, "ADD"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (export.ForAdjustments.Month < 0 || export.ForAdjustments.Month > 12)
        {
          var field = GetField(export.ForAdjustments, "month");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "OE0000_INVALID_MONTH";

          return;
        }

        UseOeUraaFetchCollMembHhInfo();

        if (export.ForAdjustments.Month == 0 || export.ForAdjustments.Year == 0)
        {
          MoveDateWorkArea(import.ForAdjustments, export.ForAdjustments);
        }

        // : January, 2002 - M Brown - Work Order Number: 010504 - Retro 
        // Processing.
        // : Set collection protection flag to 'Y' if there are protected 
        // collections for
        //   the month and year entered, and 'N' if not.
        //   This read is done on the household level.
        if (ReadCollection())
        {
          export.CollProtExists.Flag = "Y";
        }
        else
        {
          export.CollProtExists.Flag = "N";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // March, 2002, M Brown.
          // : Ob id is used for flowing to COLP, and is populated by the create
          // cab.
          //   Clear it out after displaying.
          export.Obligation.SystemGeneratedIdentifier = 0;
          ExitState = "OE0000_URAS_DISPLAY_SUCCESSFULLY";
        }

        break;
      case "URAL":
        ExitState = "ECO_LNK_TO_URAL";

        break;
      case "UCOL":
        export.ImHholdMember.Number = export.CsePersonsWorkSet.Number;
        ExitState = "ECO_LNK_TO_UCOL";

        break;
      case "UHMM":
        ExitState = "ECO_LNK_TO_UHMM";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "ADD":
        if (IsEmpty(export.ImHouseholdMbrMnthlyAdj.AdjustmentReason))
        {
          var field =
            GetField(export.ImHouseholdMbrMnthlyAdj, "adjustmentReason");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "OE0000_URA_ADJ_REASON_REQD";
        }

        if (AsChar(export.ImHouseholdMbrMnthlyAdj.LevelAppliedTo) != 'H' && AsChar
          (export.ImHouseholdMbrMnthlyAdj.LevelAppliedTo) != 'M')
        {
          var field =
            GetField(export.ImHouseholdMbrMnthlyAdj, "levelAppliedTo");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "OE0000_INVALID_APPLY_TO_LEVEL";
        }

        if (export.ImHouseholdMbrMnthlyAdj.AdjustmentAmount == 0)
        {
          var field =
            GetField(export.ImHouseholdMbrMnthlyAdj, "adjustmentAmount");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "FN0000_ADJUSTMENT_AMOUNT_ERROR";
        }

        if (AsChar(export.ImHouseholdMbrMnthlyAdj.Type1) != 'A' && AsChar
          (export.ImHouseholdMbrMnthlyAdj.Type1) != 'M')
        {
          var field = GetField(export.ImHouseholdMbrMnthlyAdj, "type1");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "OE0000_INVALID_APPLY_TO_TYPE";
        }

        if (export.ForAdjustments.Year == 0)
        {
          var field = GetField(export.ForAdjustments, "year");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "OE0000_YEAR_NOT_BE_LEFT_BLANK";
        }

        if (export.ForAdjustments.Month < 1 || export.ForAdjustments.Month > 12)
        {
          var field = GetField(export.ForAdjustments, "month");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "OE0000_INVALID_MONTH";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeUraaCreateUraAdjustments();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeUraaFetchCollMembHhInfo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // March, 2002, M Brown.
          // : Ob id is used for flowing to COLP, and is populated by the create
          // cab.
          //   Clear it out after displaying.
          export.Obligation.SystemGeneratedIdentifier = 0;
          ExitState = "OE0000_ADJUSTMENT_SUCCESSFUL";
          export.ImHouseholdMbrMnthlyAdj.Assign(
            local.InitializedImHouseholdMbrMnthlyAdj);
        }
        else
        {
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "IMHH":
        ExitState = "ECO_LNK_TO_IMHH";

        break;
      case "URAH":
        ExitState = "ECO_LNK_TO_URAH";

        break;
      case "URAC":
        ExitState = "ECO_LNK_TO_URAC";

        break;
      case "CURA":
        ExitState = "ECO_LNK_TO_CURA";

        break;
      case "COLP":
        // : M Brown, March, 2002, WO010504 - COLP needs obligation keys.  If 
        // this info was not
        //  populated by the create adjustment cab, the flow to COLP will not be
        // allowed.
        if (export.Obligation.SystemGeneratedIdentifier == 0)
        {
          ExitState = "FN0000_FLOW_TO_COLP_INVALID";
        }
        else
        {
          ExitState = "ECO_LNK_TO_COLP";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Month = source.Month;
    target.Year = source.Year;
  }

  private static void MoveImHouseholdMbrMnthlyAdj(
    ImHouseholdMbrMnthlyAdj source, ImHouseholdMbrMnthlyAdj target)
  {
    target.Type1 = source.Type1;
    target.AdjustmentAmount = source.AdjustmentAmount;
    target.LevelAppliedTo = source.LevelAppliedTo;
    target.AdjustmentReason = source.AdjustmentReason;
  }

  private static void MoveTot(OeUraaFetchCollMembHhInfo.Export.TotGroup source,
    Export.TotGroup target)
  {
    target.MbrAfGrant.TotalCurrency = source.MbrAfGrant.TotalCurrency;
    target.MbrAfColl.TotalCurrency = source.MbrAfColl.TotalCurrency;
    target.MbrAfAdj.TotalCurrency = source.MbrAfAdj.TotalCurrency;
    target.MbrAfUra.TotalCurrency = source.MbrAfUra.TotalCurrency;
    target.MbrMedGrant.TotalCurrency = source.MbrMedGrant.TotalCurrency;
    target.MbrMedColl.TotalCurrency = source.MbrMedColl.TotalCurrency;
    target.MbrMedAdj.TotalCurrency = source.MbrMedAdj.TotalCurrency;
    target.MbrMedUra.TotalCurrency = source.MbrMedUra.TotalCurrency;
    target.HhAfGrant.TotalCurrency = source.HhAfGrant.TotalCurrency;
    target.HhAfColl.TotalCurrency = source.HhAfColl.TotalCurrency;
    target.HhAfAdj.TotalCurrency = source.HhAfAdj.TotalCurrency;
    target.HhAfUra.TotalCurrency = source.HhAfUra.TotalCurrency;
    target.HhMedGrant.TotalCurrency = source.HhMedGrant.TotalCurrency;
    target.HhMedColl.TotalCurrency = source.HhMedColl.TotalCurrency;
    target.HhMedAdj.TotalCurrency = source.HhMedAdj.TotalCurrency;
    target.HhMedUra.TotalCurrency = source.HhMedUra.TotalCurrency;
    target.TotMbrAfGrant.TotalCurrency = source.TotMbrAfGrant.TotalCurrency;
    target.TotMbrAfColl.TotalCurrency = source.TotMbrAfColl.TotalCurrency;
    target.TotMbrAfAdj.TotalCurrency = source.TotMbrAfAdj.TotalCurrency;
    target.TotMbrAfUra.TotalCurrency = source.TotMbrAfUra.TotalCurrency;
    target.TotMbrMedGrant.TotalCurrency = source.TotMbrMedGrant.TotalCurrency;
    target.TotMbrMedColl.TotalCurrency = source.TotMbrMedColl.TotalCurrency;
    target.TotMbrMedAdj.TotalCurrency = source.TotMbrMedAdj.TotalCurrency;
    target.TotMbrMedUra.TotalCurrency = source.TotMbrMedUra.TotalCurrency;
    target.TotHhAfGrant.TotalCurrency = source.TotHhAfGrant.TotalCurrency;
    target.TotHhAfColl.TotalCurrency = source.TotHhAfColl.TotalCurrency;
    target.TotHhAfAdj.TotalCurrency = source.TotHhAfAdj.TotalCurrency;
    target.TotHhAfUra.TotalCurrency = source.TotHhAfUra.TotalCurrency;
    target.TotHhMedGrant.TotalCurrency = source.TotHhMedGrant.TotalCurrency;
    target.TotHhMedColl.TotalCurrency = source.TotHhMedColl.TotalCurrency;
    target.TotHhMedAdj.TotalCurrency = source.TotHhMedAdj.TotalCurrency;
    target.TotHhMedUra.TotalCurrency = source.TotHhMedUra.TotalCurrency;
  }

  private void UseOeUraaCreateUraAdjustments()
  {
    var useImport = new OeUraaCreateUraAdjustments.Import();
    var useExport = new OeUraaCreateUraAdjustments.Export();

    MoveImHouseholdMbrMnthlyAdj(export.ImHouseholdMbrMnthlyAdj,
      useImport.ImHouseholdMbrMnthlyAdj);
    MoveDateWorkArea(export.ForAdjustments, useImport.DateWorkArea);
    useImport.ImHousehold.AeCaseNo = export.ImHousehold.AeCaseNo;
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      
    MoveDateWorkArea(import.FirstAfGrant, useImport.FirstAfGrant);
    MoveDateWorkArea(import.FirstMedGrant, useImport.FirstMedGrant);

    Call(OeUraaCreateUraAdjustments.Execute, useImport, useExport);

    export.Obligation.Assign(useExport.ProtCollObligation);
    export.ObligationType.Assign(useExport.ProtCollObligationType);
    export.ObligorCsePerson.Number = useExport.ProtCollObligor.Number;
    export.ObligorCsePersonsWorkSet.Number = useExport.Obligor.Number;
  }

  private void UseOeUraaFetchCollMembHhInfo()
  {
    var useImport = new OeUraaFetchCollMembHhInfo.Import();
    var useExport = new OeUraaFetchCollMembHhInfo.Export();

    MoveDateWorkArea(export.ForAdjustments, useImport.ForAdjustments);
    useImport.ImHousehold.AeCaseNo = export.ImHousehold.AeCaseNo;
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useImport.Member);

    Call(OeUraaFetchCollMembHhInfo.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.ForAdjustments, export.ForAdjustments);
    MoveCsePersonsWorkSet(useExport.Member, export.CsePersonsWorkSet);
    export.ImHouseholdMbrMnthlySum.Relationship =
      useExport.ImHouseholdMbrMnthlySum.Relationship;
    MoveDateWorkArea(useExport.FirstAfGrant, export.FirstAfGrant);
    MoveDateWorkArea(useExport.FirstMedGrant, export.FirstMedGrant);
    MoveTot(useExport.Tot, export.Tot);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", export.ImHousehold.AeCaseNo);
        db.SetInt32(command, "imsYear", export.ForAdjustments.Year);
        db.SetInt32(command, "imsMonth", export.ForAdjustments.Month);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.DistributionMethod = db.GetString(reader, 12);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
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
    /// <summary>A TotGroup group.</summary>
    [Serializable]
    public class TotGroup
    {
      /// <summary>
      /// A value of MbrAfGrant.
      /// </summary>
      [JsonPropertyName("mbrAfGrant")]
      public Common MbrAfGrant
      {
        get => mbrAfGrant ??= new();
        set => mbrAfGrant = value;
      }

      /// <summary>
      /// A value of MbrAfColl.
      /// </summary>
      [JsonPropertyName("mbrAfColl")]
      public Common MbrAfColl
      {
        get => mbrAfColl ??= new();
        set => mbrAfColl = value;
      }

      /// <summary>
      /// A value of MbrAfAdj.
      /// </summary>
      [JsonPropertyName("mbrAfAdj")]
      public Common MbrAfAdj
      {
        get => mbrAfAdj ??= new();
        set => mbrAfAdj = value;
      }

      /// <summary>
      /// A value of MbrAfUra.
      /// </summary>
      [JsonPropertyName("mbrAfUra")]
      public Common MbrAfUra
      {
        get => mbrAfUra ??= new();
        set => mbrAfUra = value;
      }

      /// <summary>
      /// A value of MbrMedGrant.
      /// </summary>
      [JsonPropertyName("mbrMedGrant")]
      public Common MbrMedGrant
      {
        get => mbrMedGrant ??= new();
        set => mbrMedGrant = value;
      }

      /// <summary>
      /// A value of MbrMedColl.
      /// </summary>
      [JsonPropertyName("mbrMedColl")]
      public Common MbrMedColl
      {
        get => mbrMedColl ??= new();
        set => mbrMedColl = value;
      }

      /// <summary>
      /// A value of MbrMedAdj.
      /// </summary>
      [JsonPropertyName("mbrMedAdj")]
      public Common MbrMedAdj
      {
        get => mbrMedAdj ??= new();
        set => mbrMedAdj = value;
      }

      /// <summary>
      /// A value of MbrMedUra.
      /// </summary>
      [JsonPropertyName("mbrMedUra")]
      public Common MbrMedUra
      {
        get => mbrMedUra ??= new();
        set => mbrMedUra = value;
      }

      /// <summary>
      /// A value of HhAfGrant.
      /// </summary>
      [JsonPropertyName("hhAfGrant")]
      public Common HhAfGrant
      {
        get => hhAfGrant ??= new();
        set => hhAfGrant = value;
      }

      /// <summary>
      /// A value of HhAfColl.
      /// </summary>
      [JsonPropertyName("hhAfColl")]
      public Common HhAfColl
      {
        get => hhAfColl ??= new();
        set => hhAfColl = value;
      }

      /// <summary>
      /// A value of HhAfAdj.
      /// </summary>
      [JsonPropertyName("hhAfAdj")]
      public Common HhAfAdj
      {
        get => hhAfAdj ??= new();
        set => hhAfAdj = value;
      }

      /// <summary>
      /// A value of HhAfUra.
      /// </summary>
      [JsonPropertyName("hhAfUra")]
      public Common HhAfUra
      {
        get => hhAfUra ??= new();
        set => hhAfUra = value;
      }

      /// <summary>
      /// A value of HhMedGrant.
      /// </summary>
      [JsonPropertyName("hhMedGrant")]
      public Common HhMedGrant
      {
        get => hhMedGrant ??= new();
        set => hhMedGrant = value;
      }

      /// <summary>
      /// A value of HhMedColl.
      /// </summary>
      [JsonPropertyName("hhMedColl")]
      public Common HhMedColl
      {
        get => hhMedColl ??= new();
        set => hhMedColl = value;
      }

      /// <summary>
      /// A value of HhMedAdj.
      /// </summary>
      [JsonPropertyName("hhMedAdj")]
      public Common HhMedAdj
      {
        get => hhMedAdj ??= new();
        set => hhMedAdj = value;
      }

      /// <summary>
      /// A value of HhMedUra.
      /// </summary>
      [JsonPropertyName("hhMedUra")]
      public Common HhMedUra
      {
        get => hhMedUra ??= new();
        set => hhMedUra = value;
      }

      /// <summary>
      /// A value of TotMbrAfGrant.
      /// </summary>
      [JsonPropertyName("totMbrAfGrant")]
      public Common TotMbrAfGrant
      {
        get => totMbrAfGrant ??= new();
        set => totMbrAfGrant = value;
      }

      /// <summary>
      /// A value of TotMbrAfColl.
      /// </summary>
      [JsonPropertyName("totMbrAfColl")]
      public Common TotMbrAfColl
      {
        get => totMbrAfColl ??= new();
        set => totMbrAfColl = value;
      }

      /// <summary>
      /// A value of TotMbrAfAdj.
      /// </summary>
      [JsonPropertyName("totMbrAfAdj")]
      public Common TotMbrAfAdj
      {
        get => totMbrAfAdj ??= new();
        set => totMbrAfAdj = value;
      }

      /// <summary>
      /// A value of TotMbrAfUra.
      /// </summary>
      [JsonPropertyName("totMbrAfUra")]
      public Common TotMbrAfUra
      {
        get => totMbrAfUra ??= new();
        set => totMbrAfUra = value;
      }

      /// <summary>
      /// A value of TotMbrMedGrant.
      /// </summary>
      [JsonPropertyName("totMbrMedGrant")]
      public Common TotMbrMedGrant
      {
        get => totMbrMedGrant ??= new();
        set => totMbrMedGrant = value;
      }

      /// <summary>
      /// A value of TotMbrMedColl.
      /// </summary>
      [JsonPropertyName("totMbrMedColl")]
      public Common TotMbrMedColl
      {
        get => totMbrMedColl ??= new();
        set => totMbrMedColl = value;
      }

      /// <summary>
      /// A value of TotMbrMedAdj.
      /// </summary>
      [JsonPropertyName("totMbrMedAdj")]
      public Common TotMbrMedAdj
      {
        get => totMbrMedAdj ??= new();
        set => totMbrMedAdj = value;
      }

      /// <summary>
      /// A value of TotMbrMedUra.
      /// </summary>
      [JsonPropertyName("totMbrMedUra")]
      public Common TotMbrMedUra
      {
        get => totMbrMedUra ??= new();
        set => totMbrMedUra = value;
      }

      /// <summary>
      /// A value of TotHhAfGrant.
      /// </summary>
      [JsonPropertyName("totHhAfGrant")]
      public Common TotHhAfGrant
      {
        get => totHhAfGrant ??= new();
        set => totHhAfGrant = value;
      }

      /// <summary>
      /// A value of TotHhAfColl.
      /// </summary>
      [JsonPropertyName("totHhAfColl")]
      public Common TotHhAfColl
      {
        get => totHhAfColl ??= new();
        set => totHhAfColl = value;
      }

      /// <summary>
      /// A value of TotHhAfAdj.
      /// </summary>
      [JsonPropertyName("totHhAfAdj")]
      public Common TotHhAfAdj
      {
        get => totHhAfAdj ??= new();
        set => totHhAfAdj = value;
      }

      /// <summary>
      /// A value of TotHhAfUra.
      /// </summary>
      [JsonPropertyName("totHhAfUra")]
      public Common TotHhAfUra
      {
        get => totHhAfUra ??= new();
        set => totHhAfUra = value;
      }

      /// <summary>
      /// A value of TotHhMedGrant.
      /// </summary>
      [JsonPropertyName("totHhMedGrant")]
      public Common TotHhMedGrant
      {
        get => totHhMedGrant ??= new();
        set => totHhMedGrant = value;
      }

      /// <summary>
      /// A value of TotHhMedColl.
      /// </summary>
      [JsonPropertyName("totHhMedColl")]
      public Common TotHhMedColl
      {
        get => totHhMedColl ??= new();
        set => totHhMedColl = value;
      }

      /// <summary>
      /// A value of TotHhMedAdj.
      /// </summary>
      [JsonPropertyName("totHhMedAdj")]
      public Common TotHhMedAdj
      {
        get => totHhMedAdj ??= new();
        set => totHhMedAdj = value;
      }

      /// <summary>
      /// A value of TotHhMedUra.
      /// </summary>
      [JsonPropertyName("totHhMedUra")]
      public Common TotHhMedUra
      {
        get => totHhMedUra ??= new();
        set => totHhMedUra = value;
      }

      private Common mbrAfGrant;
      private Common mbrAfColl;
      private Common mbrAfAdj;
      private Common mbrAfUra;
      private Common mbrMedGrant;
      private Common mbrMedColl;
      private Common mbrMedAdj;
      private Common mbrMedUra;
      private Common hhAfGrant;
      private Common hhAfColl;
      private Common hhAfAdj;
      private Common hhAfUra;
      private Common hhMedGrant;
      private Common hhMedColl;
      private Common hhMedAdj;
      private Common hhMedUra;
      private Common totMbrAfGrant;
      private Common totMbrAfColl;
      private Common totMbrAfAdj;
      private Common totMbrAfUra;
      private Common totMbrMedGrant;
      private Common totMbrMedColl;
      private Common totMbrMedAdj;
      private Common totMbrMedUra;
      private Common totHhAfGrant;
      private Common totHhAfColl;
      private Common totHhAfAdj;
      private Common totHhAfUra;
      private Common totHhMedGrant;
      private Common totHhMedColl;
      private Common totHhMedAdj;
      private Common totHhMedUra;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ForAdjustments.
    /// </summary>
    [JsonPropertyName("forAdjustments")]
    public DateWorkArea ForAdjustments
    {
      get => forAdjustments ??= new();
      set => forAdjustments = value;
    }

    /// <summary>
    /// A value of ForAdjPrevious.
    /// </summary>
    [JsonPropertyName("forAdjPrevious")]
    public DateWorkArea ForAdjPrevious
    {
      get => forAdjPrevious ??= new();
      set => forAdjPrevious = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public ImHouseholdMbrMnthlySum Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
    }

    /// <summary>
    /// A value of FirstAfGrant.
    /// </summary>
    [JsonPropertyName("firstAfGrant")]
    public DateWorkArea FirstAfGrant
    {
      get => firstAfGrant ??= new();
      set => firstAfGrant = value;
    }

    /// <summary>
    /// A value of FirstMedGrant.
    /// </summary>
    [JsonPropertyName("firstMedGrant")]
    public DateWorkArea FirstMedGrant
    {
      get => firstMedGrant ??= new();
      set => firstMedGrant = value;
    }

    /// <summary>
    /// Gets a value of Tot.
    /// </summary>
    [JsonPropertyName("tot")]
    public TotGroup Tot
    {
      get => tot ?? (tot = new());
      set => tot = value;
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
    /// A value of CollProtExists.
    /// </summary>
    [JsonPropertyName("collProtExists")]
    public Common CollProtExists
    {
      get => collProtExists ??= new();
      set => collProtExists = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    private ImHousehold imHousehold;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private DateWorkArea forAdjustments;
    private DateWorkArea forAdjPrevious;
    private ImHouseholdMbrMnthlySum selected;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
    private DateWorkArea firstAfGrant;
    private DateWorkArea firstMedGrant;
    private TotGroup tot;
    private Standard standard;
    private Common collProtExists;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson obligorCsePerson;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A TotGroup group.</summary>
    [Serializable]
    public class TotGroup
    {
      /// <summary>
      /// A value of MbrAfGrant.
      /// </summary>
      [JsonPropertyName("mbrAfGrant")]
      public Common MbrAfGrant
      {
        get => mbrAfGrant ??= new();
        set => mbrAfGrant = value;
      }

      /// <summary>
      /// A value of MbrAfColl.
      /// </summary>
      [JsonPropertyName("mbrAfColl")]
      public Common MbrAfColl
      {
        get => mbrAfColl ??= new();
        set => mbrAfColl = value;
      }

      /// <summary>
      /// A value of MbrAfAdj.
      /// </summary>
      [JsonPropertyName("mbrAfAdj")]
      public Common MbrAfAdj
      {
        get => mbrAfAdj ??= new();
        set => mbrAfAdj = value;
      }

      /// <summary>
      /// A value of MbrAfUra.
      /// </summary>
      [JsonPropertyName("mbrAfUra")]
      public Common MbrAfUra
      {
        get => mbrAfUra ??= new();
        set => mbrAfUra = value;
      }

      /// <summary>
      /// A value of MbrMedGrant.
      /// </summary>
      [JsonPropertyName("mbrMedGrant")]
      public Common MbrMedGrant
      {
        get => mbrMedGrant ??= new();
        set => mbrMedGrant = value;
      }

      /// <summary>
      /// A value of MbrMedColl.
      /// </summary>
      [JsonPropertyName("mbrMedColl")]
      public Common MbrMedColl
      {
        get => mbrMedColl ??= new();
        set => mbrMedColl = value;
      }

      /// <summary>
      /// A value of MbrMedAdj.
      /// </summary>
      [JsonPropertyName("mbrMedAdj")]
      public Common MbrMedAdj
      {
        get => mbrMedAdj ??= new();
        set => mbrMedAdj = value;
      }

      /// <summary>
      /// A value of MbrMedUra.
      /// </summary>
      [JsonPropertyName("mbrMedUra")]
      public Common MbrMedUra
      {
        get => mbrMedUra ??= new();
        set => mbrMedUra = value;
      }

      /// <summary>
      /// A value of HhAfGrant.
      /// </summary>
      [JsonPropertyName("hhAfGrant")]
      public Common HhAfGrant
      {
        get => hhAfGrant ??= new();
        set => hhAfGrant = value;
      }

      /// <summary>
      /// A value of HhAfColl.
      /// </summary>
      [JsonPropertyName("hhAfColl")]
      public Common HhAfColl
      {
        get => hhAfColl ??= new();
        set => hhAfColl = value;
      }

      /// <summary>
      /// A value of HhAfAdj.
      /// </summary>
      [JsonPropertyName("hhAfAdj")]
      public Common HhAfAdj
      {
        get => hhAfAdj ??= new();
        set => hhAfAdj = value;
      }

      /// <summary>
      /// A value of HhAfUra.
      /// </summary>
      [JsonPropertyName("hhAfUra")]
      public Common HhAfUra
      {
        get => hhAfUra ??= new();
        set => hhAfUra = value;
      }

      /// <summary>
      /// A value of HhMedGrant.
      /// </summary>
      [JsonPropertyName("hhMedGrant")]
      public Common HhMedGrant
      {
        get => hhMedGrant ??= new();
        set => hhMedGrant = value;
      }

      /// <summary>
      /// A value of HhMedColl.
      /// </summary>
      [JsonPropertyName("hhMedColl")]
      public Common HhMedColl
      {
        get => hhMedColl ??= new();
        set => hhMedColl = value;
      }

      /// <summary>
      /// A value of HhMedAdj.
      /// </summary>
      [JsonPropertyName("hhMedAdj")]
      public Common HhMedAdj
      {
        get => hhMedAdj ??= new();
        set => hhMedAdj = value;
      }

      /// <summary>
      /// A value of HhMedUra.
      /// </summary>
      [JsonPropertyName("hhMedUra")]
      public Common HhMedUra
      {
        get => hhMedUra ??= new();
        set => hhMedUra = value;
      }

      /// <summary>
      /// A value of TotMbrAfGrant.
      /// </summary>
      [JsonPropertyName("totMbrAfGrant")]
      public Common TotMbrAfGrant
      {
        get => totMbrAfGrant ??= new();
        set => totMbrAfGrant = value;
      }

      /// <summary>
      /// A value of TotMbrAfColl.
      /// </summary>
      [JsonPropertyName("totMbrAfColl")]
      public Common TotMbrAfColl
      {
        get => totMbrAfColl ??= new();
        set => totMbrAfColl = value;
      }

      /// <summary>
      /// A value of TotMbrAfAdj.
      /// </summary>
      [JsonPropertyName("totMbrAfAdj")]
      public Common TotMbrAfAdj
      {
        get => totMbrAfAdj ??= new();
        set => totMbrAfAdj = value;
      }

      /// <summary>
      /// A value of TotMbrAfUra.
      /// </summary>
      [JsonPropertyName("totMbrAfUra")]
      public Common TotMbrAfUra
      {
        get => totMbrAfUra ??= new();
        set => totMbrAfUra = value;
      }

      /// <summary>
      /// A value of TotMbrMedGrant.
      /// </summary>
      [JsonPropertyName("totMbrMedGrant")]
      public Common TotMbrMedGrant
      {
        get => totMbrMedGrant ??= new();
        set => totMbrMedGrant = value;
      }

      /// <summary>
      /// A value of TotMbrMedColl.
      /// </summary>
      [JsonPropertyName("totMbrMedColl")]
      public Common TotMbrMedColl
      {
        get => totMbrMedColl ??= new();
        set => totMbrMedColl = value;
      }

      /// <summary>
      /// A value of TotMbrMedAdj.
      /// </summary>
      [JsonPropertyName("totMbrMedAdj")]
      public Common TotMbrMedAdj
      {
        get => totMbrMedAdj ??= new();
        set => totMbrMedAdj = value;
      }

      /// <summary>
      /// A value of TotMbrMedUra.
      /// </summary>
      [JsonPropertyName("totMbrMedUra")]
      public Common TotMbrMedUra
      {
        get => totMbrMedUra ??= new();
        set => totMbrMedUra = value;
      }

      /// <summary>
      /// A value of TotHhAfGrant.
      /// </summary>
      [JsonPropertyName("totHhAfGrant")]
      public Common TotHhAfGrant
      {
        get => totHhAfGrant ??= new();
        set => totHhAfGrant = value;
      }

      /// <summary>
      /// A value of TotHhAfColl.
      /// </summary>
      [JsonPropertyName("totHhAfColl")]
      public Common TotHhAfColl
      {
        get => totHhAfColl ??= new();
        set => totHhAfColl = value;
      }

      /// <summary>
      /// A value of TotHhAfAdj.
      /// </summary>
      [JsonPropertyName("totHhAfAdj")]
      public Common TotHhAfAdj
      {
        get => totHhAfAdj ??= new();
        set => totHhAfAdj = value;
      }

      /// <summary>
      /// A value of TotHhAfUra.
      /// </summary>
      [JsonPropertyName("totHhAfUra")]
      public Common TotHhAfUra
      {
        get => totHhAfUra ??= new();
        set => totHhAfUra = value;
      }

      /// <summary>
      /// A value of TotHhMedGrant.
      /// </summary>
      [JsonPropertyName("totHhMedGrant")]
      public Common TotHhMedGrant
      {
        get => totHhMedGrant ??= new();
        set => totHhMedGrant = value;
      }

      /// <summary>
      /// A value of TotHhMedColl.
      /// </summary>
      [JsonPropertyName("totHhMedColl")]
      public Common TotHhMedColl
      {
        get => totHhMedColl ??= new();
        set => totHhMedColl = value;
      }

      /// <summary>
      /// A value of TotHhMedAdj.
      /// </summary>
      [JsonPropertyName("totHhMedAdj")]
      public Common TotHhMedAdj
      {
        get => totHhMedAdj ??= new();
        set => totHhMedAdj = value;
      }

      /// <summary>
      /// A value of TotHhMedUra.
      /// </summary>
      [JsonPropertyName("totHhMedUra")]
      public Common TotHhMedUra
      {
        get => totHhMedUra ??= new();
        set => totHhMedUra = value;
      }

      private Common mbrAfGrant;
      private Common mbrAfColl;
      private Common mbrAfAdj;
      private Common mbrAfUra;
      private Common mbrMedGrant;
      private Common mbrMedColl;
      private Common mbrMedAdj;
      private Common mbrMedUra;
      private Common hhAfGrant;
      private Common hhAfColl;
      private Common hhAfAdj;
      private Common hhAfUra;
      private Common hhMedGrant;
      private Common hhMedColl;
      private Common hhMedAdj;
      private Common hhMedUra;
      private Common totMbrAfGrant;
      private Common totMbrAfColl;
      private Common totMbrAfAdj;
      private Common totMbrAfUra;
      private Common totMbrMedGrant;
      private Common totMbrMedColl;
      private Common totMbrMedAdj;
      private Common totMbrMedUra;
      private Common totHhAfGrant;
      private Common totHhAfColl;
      private Common totHhAfAdj;
      private Common totHhAfUra;
      private Common totHhMedGrant;
      private Common totHhMedColl;
      private Common totHhMedAdj;
      private Common totHhMedUra;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
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
    /// A value of ImHholdMember.
    /// </summary>
    [JsonPropertyName("imHholdMember")]
    public CsePerson ImHholdMember
    {
      get => imHholdMember ??= new();
      set => imHholdMember = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ForAdjustments.
    /// </summary>
    [JsonPropertyName("forAdjustments")]
    public DateWorkArea ForAdjustments
    {
      get => forAdjustments ??= new();
      set => forAdjustments = value;
    }

    /// <summary>
    /// A value of ForAdjPrevious.
    /// </summary>
    [JsonPropertyName("forAdjPrevious")]
    public DateWorkArea ForAdjPrevious
    {
      get => forAdjPrevious ??= new();
      set => forAdjPrevious = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public ImHouseholdMbrMnthlyAdj Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of FirstAfGrant.
    /// </summary>
    [JsonPropertyName("firstAfGrant")]
    public DateWorkArea FirstAfGrant
    {
      get => firstAfGrant ??= new();
      set => firstAfGrant = value;
    }

    /// <summary>
    /// A value of FirstMedGrant.
    /// </summary>
    [JsonPropertyName("firstMedGrant")]
    public DateWorkArea FirstMedGrant
    {
      get => firstMedGrant ??= new();
      set => firstMedGrant = value;
    }

    /// <summary>
    /// Gets a value of Tot.
    /// </summary>
    [JsonPropertyName("tot")]
    public TotGroup Tot
    {
      get => tot ?? (tot = new());
      set => tot = value;
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
    /// A value of CollProtExists.
    /// </summary>
    [JsonPropertyName("collProtExists")]
    public Common CollProtExists
    {
      get => collProtExists ??= new();
      set => collProtExists = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    private ImHousehold imHousehold;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson imHholdMember;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private DateWorkArea forAdjustments;
    private DateWorkArea forAdjPrevious;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
    private ImHouseholdMbrMnthlyAdj previous;
    private DateWorkArea firstAfGrant;
    private DateWorkArea firstMedGrant;
    private TotGroup tot;
    private Standard standard;
    private Common collProtExists;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson obligorCsePerson;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InitializedImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("initializedImHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj InitializedImHouseholdMbrMnthlyAdj
    {
      get => initializedImHouseholdMbrMnthlyAdj ??= new();
      set => initializedImHouseholdMbrMnthlyAdj = value;
    }

    /// <summary>
    /// A value of InitializedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("initializedCsePersonsWorkSet")]
    public CsePersonsWorkSet InitializedCsePersonsWorkSet
    {
      get => initializedCsePersonsWorkSet ??= new();
      set => initializedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private ImHouseholdMbrMnthlyAdj initializedImHouseholdMbrMnthlyAdj;
    private CsePersonsWorkSet initializedCsePersonsWorkSet;
    private DateWorkArea current;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of UraCollectionApplication.
    /// </summary>
    [JsonPropertyName("uraCollectionApplication")]
    public UraCollectionApplication UraCollectionApplication
    {
      get => uraCollectionApplication ??= new();
      set => uraCollectionApplication = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private CsePerson csePerson;
    private ImHousehold imHousehold;
    private UraCollectionApplication uraCollectionApplication;
    private Collection collection;
  }
#endregion
}
