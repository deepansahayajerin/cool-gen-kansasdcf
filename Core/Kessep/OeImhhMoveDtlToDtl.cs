// Program: OE_IMHH_MOVE_DTL_TO_DTL, ID: 374455689, model: 746.
// Short name: SWE02695
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_IMHH_MOVE_DTL_TO_DTL.
/// </summary>
[Serializable]
public partial class OeImhhMoveDtlToDtl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_IMHH_MOVE_DTL_TO_DTL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeImhhMoveDtlToDtl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeImhhMoveDtlToDtl.
  /// </summary>
  public OeImhhMoveDtlToDtl(IContext context, Import import, Export export):
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
    // Mike Fangman     05/00    PRWORA     New AB to move an occurrance of the 
    // IM HH detail information from one group view to another.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Grp.DtlImHousehold.AeCaseNo = import.Grp.DtlImHousehold.AeCaseNo;
    export.Grp.DtlImHouseholdMbrMnthlySum.Relationship =
      import.Grp.DtlImHouseholdMbrMnthlySum.Relationship;
    export.Grp.DtlCsePerson.Number = import.Grp.DtlCsePerson.Number;
    export.Grp.DtlCsePersonsWorkSet.FormattedName =
      import.Grp.DtlCsePersonsWorkSet.FormattedName;
    export.Grp.DtlFrom.YearMonth = import.Grp.DtlFrom.YearMonth;
    export.Grp.DtlTo.YearMonth = import.Grp.DtlTo.YearMonth;
    export.Grp.DtlLegalAction.StandardNumber =
      import.Grp.DtlLegalAction.StandardNumber;
    export.Grp.DtlMultCtOrdMsg.Text30 = import.Grp.DtlMultCtOrdMsg.Text30;
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
      /// A value of DtlMultCtOrdMsg.
      /// </summary>
      [JsonPropertyName("dtlMultCtOrdMsg")]
      public TextWorkArea DtlMultCtOrdMsg
      {
        get => dtlMultCtOrdMsg ??= new();
        set => dtlMultCtOrdMsg = value;
      }

      private ImHousehold dtlImHousehold;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private CsePerson dtlCsePerson;
      private CsePersonsWorkSet dtlCsePersonsWorkSet;
      private DateWorkArea dtlFrom;
      private DateWorkArea dtlTo;
      private LegalAction dtlLegalAction;
      private TextWorkArea dtlMultCtOrdMsg;
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

    private GrpGroup grp;
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
      /// A value of DtlMultCtOrdMsg.
      /// </summary>
      [JsonPropertyName("dtlMultCtOrdMsg")]
      public TextWorkArea DtlMultCtOrdMsg
      {
        get => dtlMultCtOrdMsg ??= new();
        set => dtlMultCtOrdMsg = value;
      }

      private ImHousehold dtlImHousehold;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private CsePerson dtlCsePerson;
      private CsePersonsWorkSet dtlCsePersonsWorkSet;
      private DateWorkArea dtlFrom;
      private DateWorkArea dtlTo;
      private LegalAction dtlLegalAction;
      private TextWorkArea dtlMultCtOrdMsg;
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

    private GrpGroup grp;
  }
#endregion
}
