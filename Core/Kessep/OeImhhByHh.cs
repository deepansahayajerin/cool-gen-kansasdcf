// Program: OE_IMHH_BY_HH, ID: 374455611, model: 746.
// Short name: SWE02590
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_IMHH_BY_HH.
/// </summary>
[Serializable]
public partial class OeImhhByHh: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_IMHH_BY_HH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeImhhByHh(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeImhhByHh.
  /// </summary>
  public OeImhhByHh(IContext context, Import import, Export export):
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
    // Mike Fangman     05/00    PRWORA     New AB to get HH info by household (
    // AE Case #).
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";

    // Get all of the people in the specified household.
    foreach(var item in ReadCsePerson())
    {
      // Filter the person by the people on the case # if entered.
      if (!import.PersonsOnCase.IsEmpty)
      {
        for(import.PersonsOnCase.Index = 0; import.PersonsOnCase.Index < import
          .PersonsOnCase.Count; ++import.PersonsOnCase.Index)
        {
          if (Equal(entities.CsePerson.Number,
            import.PersonsOnCase.Item.PersonsOnCaseDtl.Number))
          {
            goto Test1;
          }
        }

        continue;
      }

Test1:

      // Filter the person by the people on the court order # if entered.
      if (!import.PersonsOnCtOrder.IsEmpty)
      {
        for(import.PersonsOnCtOrder.Index = 0; import.PersonsOnCtOrder.Index < import
          .PersonsOnCtOrder.Count; ++import.PersonsOnCtOrder.Index)
        {
          if (Equal(entities.CsePerson.Number,
            import.PersonsOnCtOrder.Item.PersonsOnCtOrderDtl.Number))
          {
            goto Test2;
          }
        }

        continue;
      }

Test2:

      // For each person get that person's detail & group by date range.
      UseOeImhhByPerHh();

      // Add each person's dtls to the dtls for all persons.
      UseOeImhhAddPerGrpToTotGrp();
    }
  }

  private static void MoveGrp(OeImhhByPerHh.Export.GrpGroup source,
    Local.GrpGroup target)
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

  private void UseOeImhhByPerHh()
  {
    var useImport = new OeImhhByPerHh.Import();
    var useExport = new OeImhhByPerHh.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ImHousehold.AeCaseNo = import.ImHousehold.AeCaseNo;
    useImport.TraceMode.Flag = import.TraceMode.Flag;

    Call(OeImhhByPerHh.Execute, useImport, useExport);

    useExport.Grp.CopyTo(local.Grp, MoveGrp);
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", import.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

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
    /// <summary>A PersonsOnCaseGroup group.</summary>
    [Serializable]
    public class PersonsOnCaseGroup
    {
      /// <summary>
      /// A value of PersonsOnCaseDtl.
      /// </summary>
      [JsonPropertyName("personsOnCaseDtl")]
      public CsePerson PersonsOnCaseDtl
      {
        get => personsOnCaseDtl ??= new();
        set => personsOnCaseDtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePerson personsOnCaseDtl;
    }

    /// <summary>A PersonsOnCtOrderGroup group.</summary>
    [Serializable]
    public class PersonsOnCtOrderGroup
    {
      /// <summary>
      /// A value of PersonsOnCtOrderDtl.
      /// </summary>
      [JsonPropertyName("personsOnCtOrderDtl")]
      public CsePerson PersonsOnCtOrderDtl
      {
        get => personsOnCtOrderDtl ??= new();
        set => personsOnCtOrderDtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePerson personsOnCtOrderDtl;
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
    /// Gets a value of PersonsOnCase.
    /// </summary>
    [JsonIgnore]
    public Array<PersonsOnCaseGroup> PersonsOnCase => personsOnCase ??= new(
      PersonsOnCaseGroup.Capacity);

    /// <summary>
    /// Gets a value of PersonsOnCase for json serialization.
    /// </summary>
    [JsonPropertyName("personsOnCase")]
    [Computed]
    public IList<PersonsOnCaseGroup> PersonsOnCase_Json
    {
      get => personsOnCase;
      set => PersonsOnCase.Assign(value);
    }

    /// <summary>
    /// Gets a value of PersonsOnCtOrder.
    /// </summary>
    [JsonIgnore]
    public Array<PersonsOnCtOrderGroup> PersonsOnCtOrder =>
      personsOnCtOrder ??= new(PersonsOnCtOrderGroup.Capacity);

    /// <summary>
    /// Gets a value of PersonsOnCtOrder for json serialization.
    /// </summary>
    [JsonPropertyName("personsOnCtOrder")]
    [Computed]
    public IList<PersonsOnCtOrderGroup> PersonsOnCtOrder_Json
    {
      get => personsOnCtOrder;
      set => PersonsOnCtOrder.Assign(value);
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

    private ImHousehold imHousehold;
    private Array<PersonsOnCaseGroup> personsOnCase;
    private Array<PersonsOnCtOrderGroup> personsOnCtOrder;
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
