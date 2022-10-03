// Program: OE_IMHH_ADD_PER_GRP_TO_TOT_GRP, ID: 374455517, model: 746.
// Short name: SWE02430
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_IMHH_ADD_PER_GRP_TO_TOT_GRP.
/// </summary>
[Serializable]
public partial class OeImhhAddPerGrpToTotGrp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_IMHH_ADD_PER_GRP_TO_TOT_GRP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeImhhAddPerGrpToTotGrp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeImhhAddPerGrpToTotGrp.
  /// </summary>
  public OeImhhAddPerGrpToTotGrp(IContext context, Import import, Export export):
    
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
    // Mike Fangman     05/00    PRWORA     New AB to add a single occurrance of
    // the group view to a group view.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";

    // Add the dtl for the next person to the dtl for all of the people.
    import.Per.Index = -1;

    import.Tot.Index = import.Tot.Count - 1;
    import.Tot.CheckSize();

    while(import.Per.Index + 1 < import.Per.Count)
    {
      ++import.Per.Index;
      import.Per.CheckSize();

      ++import.Tot.Index;
      import.Tot.CheckSize();

      if (import.Tot.Index >= Import.TotGroup.Capacity)
      {
        ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

        return;
      }

      import.Tot.Update.TotGrpDtlImHousehold.AeCaseNo =
        import.Per.Item.PerGrpDtlImHousehold.AeCaseNo;
      import.Tot.Update.TotGrpDtlImHouseholdMbrMnthlySum.Relationship =
        import.Per.Item.PerGrpDtlImHouseholdMbrMnthlySum.Relationship;
      import.Tot.Update.TotGrpDtlCsePerson.Number =
        import.Per.Item.PerGrpDtlCsePerson.Number;
      import.Tot.Update.TotGrpDtlCsePersonsWorkSet.FormattedName =
        import.Per.Item.PerGrpDtlCsePersonsWorkSet.FormattedName;
      import.Tot.Update.TotGrpDtlFrom.YearMonth =
        import.Per.Item.PerGrpDtlFrom.YearMonth;
      import.Tot.Update.TotGrpDtlTo.YearMonth =
        import.Per.Item.PerGrpDtlTo.YearMonth;
      import.Tot.Update.TotGrpDtlLegalAction.StandardNumber =
        import.Per.Item.PerGrpDtlLegalAction.StandardNumber;
      import.Tot.Update.TotGrpDtlMultCtOrdMsg.Text30 =
        import.Per.Item.PerGrpDtlMultCtOrdMsg.Text30;
    }
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
    /// <summary>A PerGroup group.</summary>
    [Serializable]
    public class PerGroup
    {
      /// <summary>
      /// A value of PerGrpDtlImHousehold.
      /// </summary>
      [JsonPropertyName("perGrpDtlImHousehold")]
      public ImHousehold PerGrpDtlImHousehold
      {
        get => perGrpDtlImHousehold ??= new();
        set => perGrpDtlImHousehold = value;
      }

      /// <summary>
      /// A value of PerGrpDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("perGrpDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum PerGrpDtlImHouseholdMbrMnthlySum
      {
        get => perGrpDtlImHouseholdMbrMnthlySum ??= new();
        set => perGrpDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of PerGrpDtlCsePerson.
      /// </summary>
      [JsonPropertyName("perGrpDtlCsePerson")]
      public CsePerson PerGrpDtlCsePerson
      {
        get => perGrpDtlCsePerson ??= new();
        set => perGrpDtlCsePerson = value;
      }

      /// <summary>
      /// A value of PerGrpDtlCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("perGrpDtlCsePersonsWorkSet")]
      public CsePersonsWorkSet PerGrpDtlCsePersonsWorkSet
      {
        get => perGrpDtlCsePersonsWorkSet ??= new();
        set => perGrpDtlCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of PerGrpDtlFrom.
      /// </summary>
      [JsonPropertyName("perGrpDtlFrom")]
      public DateWorkArea PerGrpDtlFrom
      {
        get => perGrpDtlFrom ??= new();
        set => perGrpDtlFrom = value;
      }

      /// <summary>
      /// A value of PerGrpDtlTo.
      /// </summary>
      [JsonPropertyName("perGrpDtlTo")]
      public DateWorkArea PerGrpDtlTo
      {
        get => perGrpDtlTo ??= new();
        set => perGrpDtlTo = value;
      }

      /// <summary>
      /// A value of PerGrpDtlLegalAction.
      /// </summary>
      [JsonPropertyName("perGrpDtlLegalAction")]
      public LegalAction PerGrpDtlLegalAction
      {
        get => perGrpDtlLegalAction ??= new();
        set => perGrpDtlLegalAction = value;
      }

      /// <summary>
      /// A value of PerGrpDtlMultCtOrdMsg.
      /// </summary>
      [JsonPropertyName("perGrpDtlMultCtOrdMsg")]
      public TextWorkArea PerGrpDtlMultCtOrdMsg
      {
        get => perGrpDtlMultCtOrdMsg ??= new();
        set => perGrpDtlMultCtOrdMsg = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 170;

      private ImHousehold perGrpDtlImHousehold;
      private ImHouseholdMbrMnthlySum perGrpDtlImHouseholdMbrMnthlySum;
      private CsePerson perGrpDtlCsePerson;
      private CsePersonsWorkSet perGrpDtlCsePersonsWorkSet;
      private DateWorkArea perGrpDtlFrom;
      private DateWorkArea perGrpDtlTo;
      private LegalAction perGrpDtlLegalAction;
      private TextWorkArea perGrpDtlMultCtOrdMsg;
    }

    /// <summary>A TotGroup group.</summary>
    [Serializable]
    public class TotGroup
    {
      /// <summary>
      /// A value of TotGrpDtlImHousehold.
      /// </summary>
      [JsonPropertyName("totGrpDtlImHousehold")]
      public ImHousehold TotGrpDtlImHousehold
      {
        get => totGrpDtlImHousehold ??= new();
        set => totGrpDtlImHousehold = value;
      }

      /// <summary>
      /// A value of TotGrpDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("totGrpDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum TotGrpDtlImHouseholdMbrMnthlySum
      {
        get => totGrpDtlImHouseholdMbrMnthlySum ??= new();
        set => totGrpDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of TotGrpDtlCsePerson.
      /// </summary>
      [JsonPropertyName("totGrpDtlCsePerson")]
      public CsePerson TotGrpDtlCsePerson
      {
        get => totGrpDtlCsePerson ??= new();
        set => totGrpDtlCsePerson = value;
      }

      /// <summary>
      /// A value of TotGrpDtlCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("totGrpDtlCsePersonsWorkSet")]
      public CsePersonsWorkSet TotGrpDtlCsePersonsWorkSet
      {
        get => totGrpDtlCsePersonsWorkSet ??= new();
        set => totGrpDtlCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of TotGrpDtlFrom.
      /// </summary>
      [JsonPropertyName("totGrpDtlFrom")]
      public DateWorkArea TotGrpDtlFrom
      {
        get => totGrpDtlFrom ??= new();
        set => totGrpDtlFrom = value;
      }

      /// <summary>
      /// A value of TotGrpDtlTo.
      /// </summary>
      [JsonPropertyName("totGrpDtlTo")]
      public DateWorkArea TotGrpDtlTo
      {
        get => totGrpDtlTo ??= new();
        set => totGrpDtlTo = value;
      }

      /// <summary>
      /// A value of TotGrpDtlLegalAction.
      /// </summary>
      [JsonPropertyName("totGrpDtlLegalAction")]
      public LegalAction TotGrpDtlLegalAction
      {
        get => totGrpDtlLegalAction ??= new();
        set => totGrpDtlLegalAction = value;
      }

      /// <summary>
      /// A value of TotGrpDtlMultCtOrdMsg.
      /// </summary>
      [JsonPropertyName("totGrpDtlMultCtOrdMsg")]
      public TextWorkArea TotGrpDtlMultCtOrdMsg
      {
        get => totGrpDtlMultCtOrdMsg ??= new();
        set => totGrpDtlMultCtOrdMsg = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 170;

      private ImHousehold totGrpDtlImHousehold;
      private ImHouseholdMbrMnthlySum totGrpDtlImHouseholdMbrMnthlySum;
      private CsePerson totGrpDtlCsePerson;
      private CsePersonsWorkSet totGrpDtlCsePersonsWorkSet;
      private DateWorkArea totGrpDtlFrom;
      private DateWorkArea totGrpDtlTo;
      private LegalAction totGrpDtlLegalAction;
      private TextWorkArea totGrpDtlMultCtOrdMsg;
    }

    /// <summary>
    /// Gets a value of Per.
    /// </summary>
    [JsonIgnore]
    public Array<PerGroup> Per => per ??= new(PerGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Per for json serialization.
    /// </summary>
    [JsonPropertyName("per")]
    [Computed]
    public IList<PerGroup> Per_Json
    {
      get => per;
      set => Per.Assign(value);
    }

    /// <summary>
    /// Gets a value of Tot.
    /// </summary>
    [JsonIgnore]
    public Array<TotGroup> Tot => tot ??= new(TotGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Tot for json serialization.
    /// </summary>
    [JsonPropertyName("tot")]
    [Computed]
    public IList<TotGroup> Tot_Json
    {
      get => tot;
      set => Tot.Assign(value);
    }

    private Array<PerGroup> per;
    private Array<TotGroup> tot;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
