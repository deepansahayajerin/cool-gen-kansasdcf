// Program: OE_IMHH_SORT_DTL, ID: 374456723, model: 746.
// Short name: SWE02697
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_IMHH_SORT_DTL.
/// </summary>
[Serializable]
public partial class OeImhhSortDtl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_IMHH_SORT_DTL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeImhhSortDtl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeImhhSortDtl.
  /// </summary>
  public OeImhhSortDtl(IContext context, Import import, Export export):
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
    // Mike Fangman     05/00    PRWORA     This AB will sort the table by TO 
    // date descending then by FROM date descending then by Person # ascending.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    local.DtlWasMoved.Flag = "Y";
    local.NumberOfTimesThru.Count = 0;

    while(AsChar(local.DtlWasMoved.Flag) == 'Y' && local
      .NumberOfTimesThru.Count < Import.TotGroup.Capacity)
    {
      local.DtlWasMoved.Flag = "N";
      ++local.NumberOfTimesThru.Count;

      import.Tot.Index = 0;
      import.Tot.CheckSize();

      UseOeImhhMoveDtlToDtl1();

      while(import.Tot.Index + 1 < import.Tot.Count)
      {
        ++import.Tot.Index;
        import.Tot.CheckSize();

        if (import.Tot.Item.TotGrpDtlTo.YearMonth < local
          .Wk1.Wk1GrpDtlTo.YearMonth || import
          .Tot.Item.TotGrpDtlTo.YearMonth == local
          .Wk1.Wk1GrpDtlTo.YearMonth && import
          .Tot.Item.TotGrpDtlFrom.YearMonth < local
          .Wk1.Wk1GrpDtlFrom.YearMonth || import
          .Tot.Item.TotGrpDtlTo.YearMonth == local
          .Wk1.Wk1GrpDtlTo.YearMonth && import
          .Tot.Item.TotGrpDtlFrom.YearMonth == local
          .Wk1.Wk1GrpDtlFrom.YearMonth && Lt
          (local.Wk1.Wk1GrpDtlCsePerson.Number,
          import.Tot.Item.TotGrpDtlCsePerson.Number))
        {
          // No switch needed.
          if (import.Tot.Index + 1 < import.Tot.Count)
          {
            UseOeImhhMoveDtlToDtl1();
          }
        }
        else
        {
          // Switch
          UseOeImhhMoveDtlToDtl2();

          --import.Tot.Index;
          import.Tot.CheckSize();

          UseOeImhhMoveDtlToDtl4();

          ++import.Tot.Index;
          import.Tot.CheckSize();

          UseOeImhhMoveDtlToDtl3();
          local.DtlWasMoved.Flag = "Y";
        }
      }
    }
  }

  private static void MoveGrpToTot(OeImhhMoveDtlToDtl.Export.GrpGroup source,
    Import.TotGroup target)
  {
    target.TotGrpDtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.TotGrpDtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.TotGrpDtlCsePerson.Number = source.DtlCsePerson.Number;
    target.TotGrpDtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.TotGrpDtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.TotGrpDtlTo.YearMonth = source.DtlTo.YearMonth;
    target.TotGrpDtlLegalAction.StandardNumber =
      source.DtlLegalAction.StandardNumber;
    target.TotGrpDtlMultCtOrdMsg.Text30 = source.DtlMultCtOrdMsg.Text30;
  }

  private static void MoveGrpToWk1(OeImhhMoveDtlToDtl.Export.GrpGroup source,
    Local.Wk1Group target)
  {
    target.Wk1GrpDtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.Wk1GrpDtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.Wk1GrpDtlCsePerson.Number = source.DtlCsePerson.Number;
    target.Wk1GrpDtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.Wk1GrpDtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.Wk1GrpDtlTo.YearMonth = source.DtlTo.YearMonth;
    target.Wk1GrpDtlLegalAction.StandardNumber =
      source.DtlLegalAction.StandardNumber;
    target.Wk1GrpDtlMultCtOrdMsg.Text30 = source.DtlMultCtOrdMsg.Text30;
  }

  private static void MoveGrpToWk2(OeImhhMoveDtlToDtl.Export.GrpGroup source,
    Local.Wk2Group target)
  {
    target.Wk2GrpDtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.Wk2GrpDtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.Wk2GrpDtlCsePerson.Number = source.DtlCsePerson.Number;
    target.Wk2GrpDtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.Wk2GrpDtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.Wk2GrpDtlTo.YearMonth = source.DtlTo.YearMonth;
    target.Wk2GrpDtlLegalAction.StandardNumber =
      source.DtlLegalAction.StandardNumber;
    target.Wk2GrpDtlMultCtOrdMsg.Text30 = source.DtlMultCtOrdMsg.Text30;
  }

  private static void MoveTotToGrp(Import.TotGroup source,
    OeImhhMoveDtlToDtl.Import.GrpGroup target)
  {
    target.DtlImHousehold.AeCaseNo = source.TotGrpDtlImHousehold.AeCaseNo;
    target.DtlImHouseholdMbrMnthlySum.Relationship =
      source.TotGrpDtlImHouseholdMbrMnthlySum.Relationship;
    target.DtlCsePerson.Number = source.TotGrpDtlCsePerson.Number;
    target.DtlCsePersonsWorkSet.FormattedName =
      source.TotGrpDtlCsePersonsWorkSet.FormattedName;
    target.DtlFrom.YearMonth = source.TotGrpDtlFrom.YearMonth;
    target.DtlTo.YearMonth = source.TotGrpDtlTo.YearMonth;
    target.DtlLegalAction.StandardNumber =
      source.TotGrpDtlLegalAction.StandardNumber;
    target.DtlMultCtOrdMsg.Text30 = source.TotGrpDtlMultCtOrdMsg.Text30;
  }

  private static void MoveWk1ToGrp(Local.Wk1Group source,
    OeImhhMoveDtlToDtl.Import.GrpGroup target)
  {
    target.DtlImHousehold.AeCaseNo = source.Wk1GrpDtlImHousehold.AeCaseNo;
    target.DtlImHouseholdMbrMnthlySum.Relationship =
      source.Wk1GrpDtlImHouseholdMbrMnthlySum.Relationship;
    target.DtlCsePerson.Number = source.Wk1GrpDtlCsePerson.Number;
    target.DtlCsePersonsWorkSet.FormattedName =
      source.Wk1GrpDtlCsePersonsWorkSet.FormattedName;
    target.DtlFrom.YearMonth = source.Wk1GrpDtlFrom.YearMonth;
    target.DtlTo.YearMonth = source.Wk1GrpDtlTo.YearMonth;
    target.DtlLegalAction.StandardNumber =
      source.Wk1GrpDtlLegalAction.StandardNumber;
    target.DtlMultCtOrdMsg.Text30 = source.Wk1GrpDtlMultCtOrdMsg.Text30;
  }

  private static void MoveWk2ToGrp(Local.Wk2Group source,
    OeImhhMoveDtlToDtl.Import.GrpGroup target)
  {
    target.DtlImHousehold.AeCaseNo = source.Wk2GrpDtlImHousehold.AeCaseNo;
    target.DtlImHouseholdMbrMnthlySum.Relationship =
      source.Wk2GrpDtlImHouseholdMbrMnthlySum.Relationship;
    target.DtlCsePerson.Number = source.Wk2GrpDtlCsePerson.Number;
    target.DtlCsePersonsWorkSet.FormattedName =
      source.Wk2GrpDtlCsePersonsWorkSet.FormattedName;
    target.DtlFrom.YearMonth = source.Wk2GrpDtlFrom.YearMonth;
    target.DtlTo.YearMonth = source.Wk2GrpDtlTo.YearMonth;
    target.DtlLegalAction.StandardNumber =
      source.Wk2GrpDtlLegalAction.StandardNumber;
    target.DtlMultCtOrdMsg.Text30 = source.Wk2GrpDtlMultCtOrdMsg.Text30;
  }

  private void UseOeImhhMoveDtlToDtl1()
  {
    var useImport = new OeImhhMoveDtlToDtl.Import();
    var useExport = new OeImhhMoveDtlToDtl.Export();

    MoveTotToGrp(import.Tot.Item, useImport.Grp);

    Call(OeImhhMoveDtlToDtl.Execute, useImport, useExport);

    MoveGrpToWk1(useExport.Grp, local.Wk1);
  }

  private void UseOeImhhMoveDtlToDtl2()
  {
    var useImport = new OeImhhMoveDtlToDtl.Import();
    var useExport = new OeImhhMoveDtlToDtl.Export();

    MoveTotToGrp(import.Tot.Item, useImport.Grp);

    Call(OeImhhMoveDtlToDtl.Execute, useImport, useExport);

    MoveGrpToWk2(useExport.Grp, local.Wk2);
  }

  private void UseOeImhhMoveDtlToDtl3()
  {
    var useImport = new OeImhhMoveDtlToDtl.Import();
    var useExport = new OeImhhMoveDtlToDtl.Export();

    MoveWk1ToGrp(local.Wk1, useImport.Grp);

    Call(OeImhhMoveDtlToDtl.Execute, useImport, useExport);

    import.Tot.Clear();
    MoveGrpToTot(useExport.Grp, import.Tot[0]);
  }

  private void UseOeImhhMoveDtlToDtl4()
  {
    var useImport = new OeImhhMoveDtlToDtl.Import();
    var useExport = new OeImhhMoveDtlToDtl.Export();

    MoveWk2ToGrp(local.Wk2, useImport.Grp);

    Call(OeImhhMoveDtlToDtl.Execute, useImport, useExport);

    import.Tot.Clear();
    MoveGrpToTot(useExport.Grp, import.Tot[0]);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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

    private Array<TotGroup> tot;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A Wk1Group group.</summary>
    [Serializable]
    public class Wk1Group
    {
      /// <summary>
      /// A value of Wk1GrpDtlImHousehold.
      /// </summary>
      [JsonPropertyName("wk1GrpDtlImHousehold")]
      public ImHousehold Wk1GrpDtlImHousehold
      {
        get => wk1GrpDtlImHousehold ??= new();
        set => wk1GrpDtlImHousehold = value;
      }

      /// <summary>
      /// A value of Wk1GrpDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("wk1GrpDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum Wk1GrpDtlImHouseholdMbrMnthlySum
      {
        get => wk1GrpDtlImHouseholdMbrMnthlySum ??= new();
        set => wk1GrpDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of Wk1GrpDtlCsePerson.
      /// </summary>
      [JsonPropertyName("wk1GrpDtlCsePerson")]
      public CsePerson Wk1GrpDtlCsePerson
      {
        get => wk1GrpDtlCsePerson ??= new();
        set => wk1GrpDtlCsePerson = value;
      }

      /// <summary>
      /// A value of Wk1GrpDtlCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("wk1GrpDtlCsePersonsWorkSet")]
      public CsePersonsWorkSet Wk1GrpDtlCsePersonsWorkSet
      {
        get => wk1GrpDtlCsePersonsWorkSet ??= new();
        set => wk1GrpDtlCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Wk1GrpDtlFrom.
      /// </summary>
      [JsonPropertyName("wk1GrpDtlFrom")]
      public DateWorkArea Wk1GrpDtlFrom
      {
        get => wk1GrpDtlFrom ??= new();
        set => wk1GrpDtlFrom = value;
      }

      /// <summary>
      /// A value of Wk1GrpDtlTo.
      /// </summary>
      [JsonPropertyName("wk1GrpDtlTo")]
      public DateWorkArea Wk1GrpDtlTo
      {
        get => wk1GrpDtlTo ??= new();
        set => wk1GrpDtlTo = value;
      }

      /// <summary>
      /// A value of Wk1GrpDtlLegalAction.
      /// </summary>
      [JsonPropertyName("wk1GrpDtlLegalAction")]
      public LegalAction Wk1GrpDtlLegalAction
      {
        get => wk1GrpDtlLegalAction ??= new();
        set => wk1GrpDtlLegalAction = value;
      }

      /// <summary>
      /// A value of Wk1GrpDtlMultCtOrdMsg.
      /// </summary>
      [JsonPropertyName("wk1GrpDtlMultCtOrdMsg")]
      public TextWorkArea Wk1GrpDtlMultCtOrdMsg
      {
        get => wk1GrpDtlMultCtOrdMsg ??= new();
        set => wk1GrpDtlMultCtOrdMsg = value;
      }

      private ImHousehold wk1GrpDtlImHousehold;
      private ImHouseholdMbrMnthlySum wk1GrpDtlImHouseholdMbrMnthlySum;
      private CsePerson wk1GrpDtlCsePerson;
      private CsePersonsWorkSet wk1GrpDtlCsePersonsWorkSet;
      private DateWorkArea wk1GrpDtlFrom;
      private DateWorkArea wk1GrpDtlTo;
      private LegalAction wk1GrpDtlLegalAction;
      private TextWorkArea wk1GrpDtlMultCtOrdMsg;
    }

    /// <summary>A Wk2Group group.</summary>
    [Serializable]
    public class Wk2Group
    {
      /// <summary>
      /// A value of Wk2GrpDtlImHousehold.
      /// </summary>
      [JsonPropertyName("wk2GrpDtlImHousehold")]
      public ImHousehold Wk2GrpDtlImHousehold
      {
        get => wk2GrpDtlImHousehold ??= new();
        set => wk2GrpDtlImHousehold = value;
      }

      /// <summary>
      /// A value of Wk2GrpDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("wk2GrpDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum Wk2GrpDtlImHouseholdMbrMnthlySum
      {
        get => wk2GrpDtlImHouseholdMbrMnthlySum ??= new();
        set => wk2GrpDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of Wk2GrpDtlCsePerson.
      /// </summary>
      [JsonPropertyName("wk2GrpDtlCsePerson")]
      public CsePerson Wk2GrpDtlCsePerson
      {
        get => wk2GrpDtlCsePerson ??= new();
        set => wk2GrpDtlCsePerson = value;
      }

      /// <summary>
      /// A value of Wk2GrpDtlCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("wk2GrpDtlCsePersonsWorkSet")]
      public CsePersonsWorkSet Wk2GrpDtlCsePersonsWorkSet
      {
        get => wk2GrpDtlCsePersonsWorkSet ??= new();
        set => wk2GrpDtlCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Wk2GrpDtlFrom.
      /// </summary>
      [JsonPropertyName("wk2GrpDtlFrom")]
      public DateWorkArea Wk2GrpDtlFrom
      {
        get => wk2GrpDtlFrom ??= new();
        set => wk2GrpDtlFrom = value;
      }

      /// <summary>
      /// A value of Wk2GrpDtlTo.
      /// </summary>
      [JsonPropertyName("wk2GrpDtlTo")]
      public DateWorkArea Wk2GrpDtlTo
      {
        get => wk2GrpDtlTo ??= new();
        set => wk2GrpDtlTo = value;
      }

      /// <summary>
      /// A value of Wk2GrpDtlLegalAction.
      /// </summary>
      [JsonPropertyName("wk2GrpDtlLegalAction")]
      public LegalAction Wk2GrpDtlLegalAction
      {
        get => wk2GrpDtlLegalAction ??= new();
        set => wk2GrpDtlLegalAction = value;
      }

      /// <summary>
      /// A value of Wk2GrpDtlMultCtOrdMsg.
      /// </summary>
      [JsonPropertyName("wk2GrpDtlMultCtOrdMsg")]
      public TextWorkArea Wk2GrpDtlMultCtOrdMsg
      {
        get => wk2GrpDtlMultCtOrdMsg ??= new();
        set => wk2GrpDtlMultCtOrdMsg = value;
      }

      private ImHousehold wk2GrpDtlImHousehold;
      private ImHouseholdMbrMnthlySum wk2GrpDtlImHouseholdMbrMnthlySum;
      private CsePerson wk2GrpDtlCsePerson;
      private CsePersonsWorkSet wk2GrpDtlCsePersonsWorkSet;
      private DateWorkArea wk2GrpDtlFrom;
      private DateWorkArea wk2GrpDtlTo;
      private LegalAction wk2GrpDtlLegalAction;
      private TextWorkArea wk2GrpDtlMultCtOrdMsg;
    }

    /// <summary>
    /// A value of NumberOfTimesThru.
    /// </summary>
    [JsonPropertyName("numberOfTimesThru")]
    public Common NumberOfTimesThru
    {
      get => numberOfTimesThru ??= new();
      set => numberOfTimesThru = value;
    }

    /// <summary>
    /// A value of DtlWasMoved.
    /// </summary>
    [JsonPropertyName("dtlWasMoved")]
    public Common DtlWasMoved
    {
      get => dtlWasMoved ??= new();
      set => dtlWasMoved = value;
    }

    /// <summary>
    /// Gets a value of Wk1.
    /// </summary>
    [JsonPropertyName("wk1")]
    public Wk1Group Wk1
    {
      get => wk1 ?? (wk1 = new());
      set => wk1 = value;
    }

    /// <summary>
    /// Gets a value of Wk2.
    /// </summary>
    [JsonPropertyName("wk2")]
    public Wk2Group Wk2
    {
      get => wk2 ?? (wk2 = new());
      set => wk2 = value;
    }

    private Common numberOfTimesThru;
    private Common dtlWasMoved;
    private Wk1Group wk1;
    private Wk2Group wk2;
  }
#endregion
}
