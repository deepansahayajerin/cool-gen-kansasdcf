// Program: OE_IMHH_BY_PER_HH, ID: 374455623, model: 746.
// Short name: SWE02451
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_IMHH_BY_PER_HH.
/// </summary>
[Serializable]
public partial class OeImhhByPerHh: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_IMHH_BY_PER_HH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeImhhByPerHh(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeImhhByPerHh.
  /// </summary>
  public OeImhhByPerHh(IContext context, Import import, Export export):
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
    // AUTHOR    	 DATE  	  CHG REQ#   DESCRIPTION
    // Mike Fangman     05/00    PRWORA     New AB to get HH info by person and 
    // household (AE Case #).
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    local.FirstTimeThru.Flag = "Y";
    local.CompleteDetail.Flag = "N";
    export.Grp.Index = -1;
    local.Grp.DtlCsePerson.Number = import.CsePerson.Number;
    local.Grp.DtlImHousehold.AeCaseNo = import.ImHousehold.AeCaseNo;
    local.CsePersonsWorkSet.Number = import.CsePerson.Number;

    if (AsChar(import.TraceMode.Flag) != 'Y')
    {
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.Grp.DtlCsePersonsWorkSet.FormattedName =
          "Not able to get name from ADABAS.";
        ExitState = "ACO_NN0000_ALL_OK";
      }
    }

    foreach(var item in ReadImHouseholdMbrMnthlySum())
    {
      if (AsChar(local.FirstTimeThru.Flag) == 'Y')
      {
        local.FirstTimeThru.Flag = "N";
        local.Grp.DtlImHouseholdMbrMnthlySum.Relationship =
          entities.ImHouseholdMbrMnthlySum.Relationship;
        local.Grp.DtlFrom.YearMonth = entities.ImHouseholdMbrMnthlySum.Year * 100
          + entities.ImHouseholdMbrMnthlySum.Month;
        local.Grp.DtlTo.YearMonth = entities.ImHouseholdMbrMnthlySum.Year * 100
          + entities.ImHouseholdMbrMnthlySum.Month;
        local.Grp.DtlMultCtOrderMsg.Text30 = "";
      }
      else if (!Equal(entities.ImHouseholdMbrMnthlySum.Relationship,
        local.Grp.DtlImHouseholdMbrMnthlySum.Relationship))
      {
        // Change in relationship - this will end the current detail.
        local.CompleteDetail.Flag = "Y";
      }
      else
      {
        // Check for gap in time frame by subtracting 1 month from the "From" 
        // date & comparing it to the HH_MBR_MTHLY_SUM YEAR and MONTH.
        local.Year.Number4 = local.Grp.DtlFrom.YearMonth / 100;
        local.Month.Number2 = local.Grp.DtlFrom.YearMonth - local
          .Year.Number4 * 100;

        if (local.Month.Number2 == 1)
        {
          local.Month.Number2 = 12;
          --local.Year.Number4;
        }
        else
        {
          --local.Month.Number2;
        }

        if (entities.ImHouseholdMbrMnthlySum.Year == local.Year.Number4 && entities
          .ImHouseholdMbrMnthlySum.Month == local.Month.Number2)
        {
          // No gap in time frame - set the "new" from date on the current 
          // detail.
          local.Grp.DtlFrom.YearMonth = local.Year.Number4 * 100 + local
            .Month.Number2;
        }
        else
        {
          // Gap in time frame - this will end the current detail.
          local.CompleteDetail.Flag = "Y";
        }
      }

      if (AsChar(local.CompleteDetail.Flag) == 'Y')
      {
        local.CompleteDetail.Flag = "N";
        UseOeImhhMultCtOrderCheck();

        // Load the current detail (in the local group view) to the export group
        // view.
        if (export.Grp.Index + 1 >= Export.GrpGroup.Capacity)
        {
          ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

          return;
        }

        ++export.Grp.Index;
        export.Grp.CheckSize();

        UseOeImhhMoveDtlToDtl();

        // Set up the next detail (in the hold group view)
        local.Grp.DtlImHouseholdMbrMnthlySum.Relationship =
          entities.ImHouseholdMbrMnthlySum.Relationship;
        local.Grp.DtlFrom.YearMonth = entities.ImHouseholdMbrMnthlySum.Year * 100
          + entities.ImHouseholdMbrMnthlySum.Month;
        local.Grp.DtlTo.YearMonth = entities.ImHouseholdMbrMnthlySum.Year * 100
          + entities.ImHouseholdMbrMnthlySum.Month;
        local.Grp.DtlMultCtOrderMsg.Text30 = "";
      }
    }

    // ****  If at least one record was found then perform the following  ****
    if (entities.ImHouseholdMbrMnthlySum.Month > 0)
    {
      UseOeImhhMultCtOrderCheck();

      // Load the current detail (in the local group view) to the export group 
      // view.
      if (export.Grp.Index + 1 >= Export.GrpGroup.Capacity)
      {
        ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

        return;
      }

      ++export.Grp.Index;
      export.Grp.CheckSize();

      UseOeImhhMoveDtlToDtl();
    }
  }

  private static void MoveGrp1(Local.GrpGroup source,
    OeImhhMoveDtlToDtl.Import.GrpGroup target)
  {
    target.DtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.DtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.DtlCsePerson.Number = source.DtlCsePerson.Number;
    target.DtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.DtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.DtlTo.YearMonth = source.DtlTo.YearMonth;
    target.DtlLegalAction.StandardNumber = source.DtlLegalAction.StandardNumber;
    target.DtlMultCtOrdMsg.Text30 = source.DtlMultCtOrderMsg.Text30;
  }

  private static void MoveGrp2(OeImhhMoveDtlToDtl.Export.GrpGroup source,
    Export.GrpGroup target)
  {
    target.DtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.DtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.DtlCsePerson.Number = source.DtlCsePerson.Number;
    target.DtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.DtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.DtlTo.YearMonth = source.DtlTo.YearMonth;
    target.DtlLegalAction.StandardNumber = source.DtlLegalAction.StandardNumber;
    target.DtlMultCtOrderMsg.Text30 = source.DtlMultCtOrdMsg.Text30;
  }

  private void UseOeImhhMoveDtlToDtl()
  {
    var useImport = new OeImhhMoveDtlToDtl.Import();
    var useExport = new OeImhhMoveDtlToDtl.Export();

    MoveGrp1(local.Grp, useImport.Grp);

    Call(OeImhhMoveDtlToDtl.Execute, useImport, useExport);

    export.Grp.Clear();
    MoveGrp2(useExport.Grp, export.Grp[0]);
  }

  private void UseOeImhhMultCtOrderCheck()
  {
    var useImport = new OeImhhMultCtOrderCheck.Import();
    var useExport = new OeImhhMultCtOrderCheck.Export();

    useImport.From.YearMonth = local.Grp.DtlFrom.YearMonth;
    useImport.To.YearMonth = local.Grp.DtlTo.YearMonth;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(OeImhhMultCtOrderCheck.Execute, useImport, useExport);

    local.Grp.DtlLegalAction.StandardNumber =
      useExport.LegalAction.StandardNumber;
    local.Grp.DtlMultCtOrderMsg.Text30 = useExport.MultCtOrderMsg.Text30;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Grp.DtlCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private IEnumerable<bool> ReadImHouseholdMbrMnthlySum()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "imhAeCaseNo", import.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 3);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 4);
        entities.ImHouseholdMbrMnthlySum.Populated = true;

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
    /// A value of TraceMode.
    /// </summary>
    [JsonPropertyName("traceMode")]
    public Common TraceMode
    {
      get => traceMode ??= new();
      set => traceMode = value;
    }

    private CsePerson csePerson;
    private ImHousehold imHousehold;
    private Common traceMode;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of DtlImHousehold.
      /// </summary>
      [JsonPropertyName("dtlImHousehold")]
      public ImHousehold DtlImHousehold
      {
        get => dtlImHousehold ??= new();
        set => dtlImHousehold = value;
      }

      /// <summary>
      /// A value of DtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DtlImHouseholdMbrMnthlySum
      {
        get => dtlImHouseholdMbrMnthlySum ??= new();
        set => dtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DtlCsePerson.
      /// </summary>
      [JsonPropertyName("dtlCsePerson")]
      public CsePerson DtlCsePerson
      {
        get => dtlCsePerson ??= new();
        set => dtlCsePerson = value;
      }

      /// <summary>
      /// A value of DtlCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dtlCsePersonsWorkSet")]
      public CsePersonsWorkSet DtlCsePersonsWorkSet
      {
        get => dtlCsePersonsWorkSet ??= new();
        set => dtlCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DtlFrom.
      /// </summary>
      [JsonPropertyName("dtlFrom")]
      public DateWorkArea DtlFrom
      {
        get => dtlFrom ??= new();
        set => dtlFrom = value;
      }

      /// <summary>
      /// A value of DtlTo.
      /// </summary>
      [JsonPropertyName("dtlTo")]
      public DateWorkArea DtlTo
      {
        get => dtlTo ??= new();
        set => dtlTo = value;
      }

      /// <summary>
      /// A value of DtlLegalAction.
      /// </summary>
      [JsonPropertyName("dtlLegalAction")]
      public LegalAction DtlLegalAction
      {
        get => dtlLegalAction ??= new();
        set => dtlLegalAction = value;
      }

      /// <summary>
      /// A value of DtlMultCtOrderMsg.
      /// </summary>
      [JsonPropertyName("dtlMultCtOrderMsg")]
      public TextWorkArea DtlMultCtOrderMsg
      {
        get => dtlMultCtOrderMsg ??= new();
        set => dtlMultCtOrderMsg = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 170;

      private ImHousehold dtlImHousehold;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private CsePerson dtlCsePerson;
      private CsePersonsWorkSet dtlCsePersonsWorkSet;
      private DateWorkArea dtlFrom;
      private DateWorkArea dtlTo;
      private LegalAction dtlLegalAction;
      private TextWorkArea dtlMultCtOrderMsg;
    }

    /// <summary>
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
    }

    private Array<GrpGroup> grp;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of DtlImHousehold.
      /// </summary>
      [JsonPropertyName("dtlImHousehold")]
      public ImHousehold DtlImHousehold
      {
        get => dtlImHousehold ??= new();
        set => dtlImHousehold = value;
      }

      /// <summary>
      /// A value of DtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DtlImHouseholdMbrMnthlySum
      {
        get => dtlImHouseholdMbrMnthlySum ??= new();
        set => dtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DtlCsePerson.
      /// </summary>
      [JsonPropertyName("dtlCsePerson")]
      public CsePerson DtlCsePerson
      {
        get => dtlCsePerson ??= new();
        set => dtlCsePerson = value;
      }

      /// <summary>
      /// A value of DtlCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dtlCsePersonsWorkSet")]
      public CsePersonsWorkSet DtlCsePersonsWorkSet
      {
        get => dtlCsePersonsWorkSet ??= new();
        set => dtlCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DtlFrom.
      /// </summary>
      [JsonPropertyName("dtlFrom")]
      public DateWorkArea DtlFrom
      {
        get => dtlFrom ??= new();
        set => dtlFrom = value;
      }

      /// <summary>
      /// A value of DtlTo.
      /// </summary>
      [JsonPropertyName("dtlTo")]
      public DateWorkArea DtlTo
      {
        get => dtlTo ??= new();
        set => dtlTo = value;
      }

      /// <summary>
      /// A value of DtlLegalAction.
      /// </summary>
      [JsonPropertyName("dtlLegalAction")]
      public LegalAction DtlLegalAction
      {
        get => dtlLegalAction ??= new();
        set => dtlLegalAction = value;
      }

      /// <summary>
      /// A value of DtlMultCtOrderMsg.
      /// </summary>
      [JsonPropertyName("dtlMultCtOrderMsg")]
      public TextWorkArea DtlMultCtOrderMsg
      {
        get => dtlMultCtOrderMsg ??= new();
        set => dtlMultCtOrderMsg = value;
      }

      private ImHousehold dtlImHousehold;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private CsePerson dtlCsePerson;
      private CsePersonsWorkSet dtlCsePersonsWorkSet;
      private DateWorkArea dtlFrom;
      private DateWorkArea dtlTo;
      private LegalAction dtlLegalAction;
      private TextWorkArea dtlMultCtOrderMsg;
    }

    /// <summary>
    /// A value of NextExpectedFrom.
    /// </summary>
    [JsonPropertyName("nextExpectedFrom")]
    public DateWorkArea NextExpectedFrom
    {
      get => nextExpectedFrom ??= new();
      set => nextExpectedFrom = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public NumericWorkSet Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public NumericWorkSet Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
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
    /// Gets a value of Grp.
    /// </summary>
    [JsonPropertyName("grp")]
    public GrpGroup Grp
    {
      get => grp ?? (grp = new());
      set => grp = value;
    }

    /// <summary>
    /// A value of FirstTimeThru.
    /// </summary>
    [JsonPropertyName("firstTimeThru")]
    public Common FirstTimeThru
    {
      get => firstTimeThru ??= new();
      set => firstTimeThru = value;
    }

    /// <summary>
    /// A value of CompleteDetail.
    /// </summary>
    [JsonPropertyName("completeDetail")]
    public Common CompleteDetail
    {
      get => completeDetail ??= new();
      set => completeDetail = value;
    }

    private DateWorkArea nextExpectedFrom;
    private NumericWorkSet year;
    private NumericWorkSet month;
    private DateWorkArea from;
    private DateWorkArea to;
    private CsePersonsWorkSet csePersonsWorkSet;
    private GrpGroup grp;
    private Common firstTimeThru;
    private Common completeDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    private CsePerson csePerson;
    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
  }
#endregion
}
