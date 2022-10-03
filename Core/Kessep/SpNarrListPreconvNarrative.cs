// Program: SP_NARR_LIST_PRECONV_NARRATIVE, ID: 371751509, model: 746.
// Short name: SWENARRP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_NARR_LIST_PRECONV_NARRATIVE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpNarrListPreconvNarrative: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_NARR_LIST_PRECONV_NARRATIVE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpNarrListPreconvNarrative(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpNarrListPreconvNarrative.
  /// </summary>
  public SpNarrListPreconvNarrative(IContext context, Import import,
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
    // ---------------------------------------------
    //       M A I N T E N A N C E   L O G
    //   Date   | Developer    | Description
    // --------------------------------------------
    // 	
    // 01/13/97 |  d BROKAW    |
    // 09/30/98 |  swsrkeh     |  Phase 2 changes
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.HoldNarr.KaecsesEndDate = new DateTime(2099, 12, 31);
    local.HoldNarr.KaecsesStartDate = new DateTime(1999, 7, 1);
    local.Current.NarrativeDate = Now().Date;

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Flow.KaecsesCaseNumber = import.Flow.KaecsesCaseNumber;
    export.KcasSearchPerson.Number = import.KcasSearchPerson.Number;
    export.Hidden.Assign(import.Hidden);
    export.Next.Number = import.Next.Number;
    MoveNarrWorkData(import.NarrWorkData, export.NarrWorkData);
    MoveCsePersonsWorkSet(import.Ap, export.HiddenAp);
    MoveCsePersonsWorkSet(import.Ar, export.HiddenAr);
    export.Case1.Number = import.Case1.Number;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    MoveCsePersonsWorkSet(import.Ap, export.Ap);
    MoveCsePersonsWorkSet(import.HiddenAp, export.HiddenAp);

    export.Narr.Index = 0;
    export.Narr.Clear();

    for(import.Narr.Index = 0; import.Narr.Index < import.Narr.Count; ++
      import.Narr.Index)
    {
      if (export.Narr.IsFull)
      {
        break;
      }

      export.Narr.Update.NarrWorkData.Assign(import.Narr.Item.NarrWorkData);
      export.Narr.Next();
    }

    export.NarrWorkData.CseCaseNumber = import.Flow.KaecsesCaseNumber;

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        break;
      case "ENTER":
        if (IsEmpty(import.Standard.NextTransaction))
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";
        }
        else
        {
          // ------------------------------------------------------------
          // This is where you would set the local next_tran_info attributes to 
          // the import view attributes for the data to be passed to the next
          // transaction
          // -------------------------------------------------------------
          UseScCabNextTranPut();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.Next, "number");

            field.Error = true;

            return;
          }

          return;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        return;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "XXFMMENU":
        return;
      case "XXNEXTXX":
        UseScCabNextTranGet();
        export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
        UseCabZeroFillNumber();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          return;
        }

        if (!IsEmpty(export.Case1.Number))
        {
          global.Command = "DISPLAY";
        }
        else
        {
          return;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      if (!IsEmpty(import.Flow.KaecsesCaseNumber))
      {
        local.WorkArea.Text12 = import.Flow.KaecsesCaseNumber;
        UseCabTestForNumericText();

        if (AsChar(local.Common.Flag) == 'Y')
        {
          local.PreconvCaseHist.KaecsesCaseNumber =
            NumberToString(StringToNumber(import.Flow.KaecsesCaseNumber), 12);
        }
        else
        {
          ExitState = "CASE_NUMBER_NOT_NUMERIC";

          return;
        }
      }

      if (Equal(export.NarrWorkData.NarrativeDate, local.Blank.NarrativeDate))
      {
        foreach(var item in ReadPreconvCaseHist2())
        {
          export.NarrWorkData.NarrativeDate =
            entities.PreconvCaseHist.KaecsesStartDate;
        }
      }

      if (Equal(export.NarrWorkData.NarrativeDate, local.Blank.NarrativeDate))
      {
        if (Lt(local.Current.NarrativeDate, local.HoldNarr.KaecsesStartDate))
        {
          export.NarrWorkData.NarrativeDate =
            AddDays(local.Current.NarrativeDate, -30);
        }
        else
        {
          export.NarrWorkData.NarrativeDate =
            AddDays(local.HoldNarr.KaecsesStartDate, -30);
        }
      }

      local.Count.Count = 0;

      foreach(var item in ReadPreconvCaseHist1())
      {
        ++local.Count.Count;
        local.CsePersonsWorkSet.Number =
          entities.PreconvCaseHist.CsePersonNumber;
        UseSiReadCsePerson();

        if (Equal(entities.PreconvCaseHist.KaecsesRelationship, "AP"))
        {
          MoveCsePersonsWorkSet(local.CsePersonsWorkSet, export.Ap);
        }
        else if (Equal(entities.PreconvCaseHist.KaecsesRelationship, "AR"))
        {
          MoveCsePersonsWorkSet(local.CsePersonsWorkSet, export.Ar);
        }

        // **** If a specific date not requested then use the  ****
        // **** earliest date either the create timestamp or   ****
        // **** the pre_case_hist end date                     ****
        if (Equal(export.NarrWorkData.NarrativeDate, local.Blank.NarrativeDate))
        {
          // **** Determine which date to pass to ADABAS ***
          if (Equal(entities.PreconvCaseHist.KaecsesEndDate,
            local.HoldNarr.KaecsesEndDate))
          {
            // **** if = to 2099-12-31 use create timestamp ***
            local.NarrConvert.KaecsesEndDate =
              Date(entities.PreconvCaseHist.CreatedTimestamp);
          }
          else
          {
            local.NarrConvert.KaecsesEndDate =
              entities.PreconvCaseHist.KaecsesEndDate;
          }

          if (Lt(local.NarrConvert.KaecsesEndDate,
            local.HoldNarr.KaecsesStartDate))
          {
            local.HoldNarr.KaecsesStartDate = local.NarrConvert.KaecsesEndDate;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_ADABAS_PERSON_NF_113";

          return;
        }
      }

      if (local.Count.Count > 0)
      {
      }
      else
      {
        ExitState = "CASE_NF";

        var field = GetField(export.PreconvCaseHist, "kaecsesCaseNumber");

        field.Error = true;

        return;
      }

      UseCabReadNarrative();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.NarrWorkData, "cseCaseNumber");

        field.Error = true;

        return;
      }

      if (export.Narr.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
      else
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGroupToNarr(CabReadNarrative.Export.GroupGroup source,
    Export.NarrGroup target)
  {
    target.NarrWorkData.Assign(source.NarrWorkData);
  }

  private static void MoveNarrWorkData(NarrWorkData source, NarrWorkData target)
  {
    target.CseCaseNumber = source.CseCaseNumber;
    target.NarrativeDate = source.NarrativeDate;
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

  private void UseCabReadNarrative()
  {
    var useImport = new CabReadNarrative.Import();
    var useExport = new CabReadNarrative.Export();

    MoveNarrWorkData(export.NarrWorkData, useImport.NarrWorkData);

    Call(CabReadNarrative.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Narr, MoveGroupToNarr);
  }

  private void UseCabTestForNumericText()
  {
    var useImport = new CabTestForNumericText.Import();
    var useExport = new CabTestForNumericText.Export();

    useImport.WorkArea.Text12 = local.WorkArea.Text12;

    Call(CabTestForNumericText.Execute, useImport, useExport);

    local.Common.Flag = useExport.NumericText.Flag;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
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

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadPreconvCaseHist1()
  {
    entities.PreconvCaseHist.Populated = false;

    return ReadEach("ReadPreconvCaseHist1",
      (db, command) =>
      {
        db.SetString(command, "kaecsesCaseNum", import.Flow.KaecsesCaseNumber);
      },
      (db, reader) =>
      {
        entities.PreconvCaseHist.CsePersonNumber = db.GetString(reader, 0);
        entities.PreconvCaseHist.KaecsesCaseNumber = db.GetString(reader, 1);
        entities.PreconvCaseHist.KaecsesRelationship = db.GetString(reader, 2);
        entities.PreconvCaseHist.KaecsesStartDate = db.GetDate(reader, 3);
        entities.PreconvCaseHist.KaecsesEndDate = db.GetDate(reader, 4);
        entities.PreconvCaseHist.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PreconvCaseHist.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPreconvCaseHist2()
  {
    entities.PreconvCaseHist.Populated = false;

    return ReadEach("ReadPreconvCaseHist2",
      (db, command) =>
      {
        db.SetString(command, "kaecsesCaseNum", import.Flow.KaecsesCaseNumber);
      },
      (db, reader) =>
      {
        entities.PreconvCaseHist.CsePersonNumber = db.GetString(reader, 0);
        entities.PreconvCaseHist.KaecsesCaseNumber = db.GetString(reader, 1);
        entities.PreconvCaseHist.KaecsesRelationship = db.GetString(reader, 2);
        entities.PreconvCaseHist.KaecsesStartDate = db.GetDate(reader, 3);
        entities.PreconvCaseHist.KaecsesEndDate = db.GetDate(reader, 4);
        entities.PreconvCaseHist.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PreconvCaseHist.Populated = true;

        return true;
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
    /// <summary>A NarrGroup group.</summary>
    [Serializable]
    public class NarrGroup
    {
      /// <summary>
      /// A value of NarrWorkData.
      /// </summary>
      [JsonPropertyName("narrWorkData")]
      public NarrWorkData NarrWorkData
      {
        get => narrWorkData ??= new();
        set => narrWorkData = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 174;

      private NarrWorkData narrWorkData;
    }

    /// <summary>
    /// A value of KcasSearchPerson.
    /// </summary>
    [JsonPropertyName("kcasSearchPerson")]
    public CsePersonsWorkSet KcasSearchPerson
    {
      get => kcasSearchPerson ??= new();
      set => kcasSearchPerson = value;
    }

    /// <summary>
    /// Gets a value of Narr.
    /// </summary>
    [JsonIgnore]
    public Array<NarrGroup> Narr => narr ??= new(NarrGroup.Capacity);

    /// <summary>
    /// Gets a value of Narr for json serialization.
    /// </summary>
    [JsonPropertyName("narr")]
    [Computed]
    public IList<NarrGroup> Narr_Json
    {
      get => narr;
      set => Narr.Assign(value);
    }

    /// <summary>
    /// A value of NarrWorkData.
    /// </summary>
    [JsonPropertyName("narrWorkData")]
    public NarrWorkData NarrWorkData
    {
      get => narrWorkData ??= new();
      set => narrWorkData = value;
    }

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public PreconvCaseHist Flow
    {
      get => flow ??= new();
      set => flow = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of HiddenNext.
    /// </summary>
    [JsonPropertyName("hiddenNext")]
    public Case1 HiddenNext
    {
      get => hiddenNext ??= new();
      set => hiddenNext = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
    }

    /// <summary>
    /// A value of HiddenAr.
    /// </summary>
    [JsonPropertyName("hiddenAr")]
    public CsePersonsWorkSet HiddenAr
    {
      get => hiddenAr ??= new();
      set => hiddenAr = value;
    }

    private CsePersonsWorkSet kcasSearchPerson;
    private Array<NarrGroup> narr;
    private NarrWorkData narrWorkData;
    private PreconvCaseHist flow;
    private NextTranInfo hidden;
    private Case1 next;
    private Case1 hiddenNext;
    private Case1 case1;
    private Standard standard;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet hiddenAp;
    private CsePersonsWorkSet hiddenAr;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A NarrGroup group.</summary>
    [Serializable]
    public class NarrGroup
    {
      /// <summary>
      /// A value of NarrWorkData.
      /// </summary>
      [JsonPropertyName("narrWorkData")]
      public NarrWorkData NarrWorkData
      {
        get => narrWorkData ??= new();
        set => narrWorkData = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 174;

      private NarrWorkData narrWorkData;
    }

    /// <summary>
    /// A value of KcasSearchPerson.
    /// </summary>
    [JsonPropertyName("kcasSearchPerson")]
    public CsePersonsWorkSet KcasSearchPerson
    {
      get => kcasSearchPerson ??= new();
      set => kcasSearchPerson = value;
    }

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public PreconvCaseHist Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    /// <summary>
    /// Gets a value of Narr.
    /// </summary>
    [JsonIgnore]
    public Array<NarrGroup> Narr => narr ??= new(NarrGroup.Capacity);

    /// <summary>
    /// Gets a value of Narr for json serialization.
    /// </summary>
    [JsonPropertyName("narr")]
    [Computed]
    public IList<NarrGroup> Narr_Json
    {
      get => narr;
      set => Narr.Assign(value);
    }

    /// <summary>
    /// A value of NarrWorkData.
    /// </summary>
    [JsonPropertyName("narrWorkData")]
    public NarrWorkData NarrWorkData
    {
      get => narrWorkData ??= new();
      set => narrWorkData = value;
    }

    /// <summary>
    /// A value of PreconvCaseHist.
    /// </summary>
    [JsonPropertyName("preconvCaseHist")]
    public PreconvCaseHist PreconvCaseHist
    {
      get => preconvCaseHist ??= new();
      set => preconvCaseHist = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of HiddenNext.
    /// </summary>
    [JsonPropertyName("hiddenNext")]
    public Case1 HiddenNext
    {
      get => hiddenNext ??= new();
      set => hiddenNext = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
    }

    /// <summary>
    /// A value of HiddenAr.
    /// </summary>
    [JsonPropertyName("hiddenAr")]
    public CsePersonsWorkSet HiddenAr
    {
      get => hiddenAr ??= new();
      set => hiddenAr = value;
    }

    private CsePersonsWorkSet kcasSearchPerson;
    private PreconvCaseHist flow;
    private Array<NarrGroup> narr;
    private NarrWorkData narrWorkData;
    private PreconvCaseHist preconvCaseHist;
    private NextTranInfo hidden;
    private Case1 next;
    private Case1 hiddenNext;
    private Case1 case1;
    private Standard standard;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet hiddenAp;
    private CsePersonsWorkSet hiddenAr;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public NarrWorkData Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of NarrConvert.
    /// </summary>
    [JsonPropertyName("narrConvert")]
    public PreconvCaseHist NarrConvert
    {
      get => narrConvert ??= new();
      set => narrConvert = value;
    }

    /// <summary>
    /// A value of HoldNarr.
    /// </summary>
    [JsonPropertyName("holdNarr")]
    public PreconvCaseHist HoldNarr
    {
      get => holdNarr ??= new();
      set => holdNarr = value;
    }

    /// <summary>
    /// A value of PreconvCaseHist.
    /// </summary>
    [JsonPropertyName("preconvCaseHist")]
    public PreconvCaseHist PreconvCaseHist
    {
      get => preconvCaseHist ??= new();
      set => preconvCaseHist = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public NarrWorkData Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private NarrWorkData current;
    private PreconvCaseHist narrConvert;
    private PreconvCaseHist holdNarr;
    private PreconvCaseHist preconvCaseHist;
    private Common count;
    private Common common;
    private WorkArea workArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private NarrWorkData blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PreconvCaseHist.
    /// </summary>
    [JsonPropertyName("preconvCaseHist")]
    public PreconvCaseHist PreconvCaseHist
    {
      get => preconvCaseHist ??= new();
      set => preconvCaseHist = value;
    }

    private PreconvCaseHist preconvCaseHist;
  }
#endregion
}
