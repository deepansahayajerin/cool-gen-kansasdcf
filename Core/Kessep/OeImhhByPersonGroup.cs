// Program: OE_IMHH_BY_PERSON_GROUP, ID: 374456639, model: 746.
// Short name: SWE02694
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_IMHH_BY_PERSON_GROUP.
/// </summary>
[Serializable]
public partial class OeImhhByPersonGroup: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_IMHH_BY_PERSON_GROUP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeImhhByPersonGroup(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeImhhByPersonGroup.
  /// </summary>
  public OeImhhByPersonGroup(IContext context, Import import, Export export):
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
    // Mike Fangman     05/00    PRWORA     New AB to get HH info by one or more
    // people.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";

    for(import.PersonsGrp1.Index = 0; import.PersonsGrp1.Index < import
      .PersonsGrp1.Count; ++import.PersonsGrp1.Index)
    {
      if (!import.PersonsGrp2.IsEmpty)
      {
        for(import.PersonsGrp2.Index = 0; import.PersonsGrp2.Index < import
          .PersonsGrp2.Count; ++import.PersonsGrp2.Index)
        {
          if (Equal(import.PersonsGrp1.Item.PersonsDtl1.Number,
            import.PersonsGrp2.Item.PersonsDtl2.Number))
          {
            goto Test;
          }
        }

        continue;
      }

Test:

      UseOeImhhByPerson();
      UseOeImhhAddPerGrpToTotGrp();
    }
  }

  private static void MoveGrpToPer(Local.GrpGroup source,
    OeImhhAddPerGrpToTotGrp.Import.PerGroup target)
  {
    target.PerGrpDtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.PerGrpDtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.PerGrpDtlCsePerson.Number = source.DtlCsePerson.Number;
    target.PerGrpDtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.PerGrpDtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.PerGrpDtlTo.YearMonth = source.DtlTo.YearMonth;
    target.PerGrpDtlLegalAction.StandardNumber =
      source.DtlLegalAction.StandardNumber;
    target.PerGrpDtlMultCtOrdMsg.Text30 = source.DtlMultCtOrderMsg.Text30;
  }

  private static void MoveHhPersonToGrp(OeImhhByPerson.Export.
    HhPersonGroup source, Local.GrpGroup target)
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
    target.DtlMultCtOrderMsg.Text30 = source.DtlMultCtOrderMsg.Text30;
  }

  private static void MoveHhPersonToTot(Export.HhPersonGroup source,
    OeImhhAddPerGrpToTotGrp.Import.TotGroup target)
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
    target.TotGrpDtlMultCtOrdMsg.Text30 = source.DtlMultCtOrderMsg.Text30;
  }

  private static void MoveTotToHhPerson(OeImhhAddPerGrpToTotGrp.Import.
    TotGroup source, Export.HhPersonGroup target)
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
    target.DtlMultCtOrderMsg.Text30 = source.TotGrpDtlMultCtOrdMsg.Text30;
  }

  private void UseOeImhhAddPerGrpToTotGrp()
  {
    var useImport = new OeImhhAddPerGrpToTotGrp.Import();
    var useExport = new OeImhhAddPerGrpToTotGrp.Export();

    local.Grp.CopyTo(useImport.Per, MoveGrpToPer);
    export.HhPerson.CopyTo(useImport.Tot, MoveHhPersonToTot);

    Call(OeImhhAddPerGrpToTotGrp.Execute, useImport, useExport);

    useImport.Tot.CopyTo(export.HhPerson, MoveTotToHhPerson);
  }

  private void UseOeImhhByPerson()
  {
    var useImport = new OeImhhByPerson.Import();
    var useExport = new OeImhhByPerson.Export();

    useImport.TraceMode.Flag = import.TraceMode.Flag;
    useImport.CsePerson.Number = import.PersonsGrp1.Item.PersonsDtl1.Number;

    Call(OeImhhByPerson.Execute, useImport, useExport);

    useExport.HhPerson.CopyTo(local.Grp, MoveHhPersonToGrp);
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
    /// <summary>A PersonsGrp1Group group.</summary>
    [Serializable]
    public class PersonsGrp1Group
    {
      /// <summary>
      /// A value of PersonsDtl1.
      /// </summary>
      [JsonPropertyName("personsDtl1")]
      public CsePerson PersonsDtl1
      {
        get => personsDtl1 ??= new();
        set => personsDtl1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CsePerson personsDtl1;
    }

    /// <summary>A PersonsGrp2Group group.</summary>
    [Serializable]
    public class PersonsGrp2Group
    {
      /// <summary>
      /// A value of PersonsDtl2.
      /// </summary>
      [JsonPropertyName("personsDtl2")]
      public CsePerson PersonsDtl2
      {
        get => personsDtl2 ??= new();
        set => personsDtl2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CsePerson personsDtl2;
    }

    /// <summary>
    /// Gets a value of PersonsGrp1.
    /// </summary>
    [JsonIgnore]
    public Array<PersonsGrp1Group> PersonsGrp1 => personsGrp1 ??= new(
      PersonsGrp1Group.Capacity);

    /// <summary>
    /// Gets a value of PersonsGrp1 for json serialization.
    /// </summary>
    [JsonPropertyName("personsGrp1")]
    [Computed]
    public IList<PersonsGrp1Group> PersonsGrp1_Json
    {
      get => personsGrp1;
      set => PersonsGrp1.Assign(value);
    }

    /// <summary>
    /// Gets a value of PersonsGrp2.
    /// </summary>
    [JsonIgnore]
    public Array<PersonsGrp2Group> PersonsGrp2 => personsGrp2 ??= new(
      PersonsGrp2Group.Capacity);

    /// <summary>
    /// Gets a value of PersonsGrp2 for json serialization.
    /// </summary>
    [JsonPropertyName("personsGrp2")]
    [Computed]
    public IList<PersonsGrp2Group> PersonsGrp2_Json
    {
      get => personsGrp2;
      set => PersonsGrp2.Assign(value);
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

    private Array<PersonsGrp1Group> personsGrp1;
    private Array<PersonsGrp2Group> personsGrp2;
    private Common traceMode;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HhPersonGroup group.</summary>
    [Serializable]
    public class HhPersonGroup
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
    /// Gets a value of HhPerson.
    /// </summary>
    [JsonIgnore]
    public Array<HhPersonGroup> HhPerson => hhPerson ??= new(
      HhPersonGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HhPerson for json serialization.
    /// </summary>
    [JsonPropertyName("hhPerson")]
    [Computed]
    public IList<HhPersonGroup> HhPerson_Json
    {
      get => hhPerson;
      set => HhPerson.Assign(value);
    }

    private Array<HhPersonGroup> hhPerson;
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
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity);

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
#endregion
}
