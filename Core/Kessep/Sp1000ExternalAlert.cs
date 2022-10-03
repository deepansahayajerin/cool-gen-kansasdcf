// Program: SP_1000_EXTERNAL_ALERT, ID: 372545052, model: 746.
// Short name: SWE01855
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_1000_EXTERNAL_ALERT.
/// </para>
/// <para>
/// This AB creates interface alerts for alert code 47;  AR Received $1000 
/// disbursement in one month.
/// </para>
/// </summary>
[Serializable]
public partial class Sp1000ExternalAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_1000_EXTERNAL_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Sp1000ExternalAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Sp1000ExternalAlert.
  /// </summary>
  public Sp1000ExternalAlert(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.CsePersonsWorkSet.Number = import.CsePerson.Number;
    UseCabReadAdabasPersonBatch();

    if (!IsEmpty(local.ReadCsePerson.Type1))
    {
      return;
    }

    if (IsEmpty(local.Ae.Flag))
    {
      return;
    }

    // --------------------------------------------------------------------
    // ALERT CODE:  47
    // ALERT TEXT:  AR NAME        1-  7
    //              DATE           8- 11
    //              AMOUNT        12- 17
    // --------------------------------------------------------------------
    export.InterfaceAlert.AlertCode = "47";
    export.InterfaceAlert.CsePersonNumber = import.CsePerson.Number;

    // ---------------------------------------------------------------------
    //     AR NAME
    // ---------------------------------------------------------------------
    local.Temp.Value =
      Substring(local.CsePersonsWorkSet.LastName,
      CsePersonsWorkSet.LastName_MaxLength, 1, 5) + " ";
    local.Temp.Value = (local.Temp.Value ?? "") + Substring
      (local.CsePersonsWorkSet.FirstName, CsePersonsWorkSet.FirstName_MaxLength,
      1, 1);

    // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
    // *****************
    local.LastPosition.Count = 0;
    local.Final.Value = local.Temp.Value ?? "";

    // **************************************************************************
    // ---------------------------------------------------------------------
    //     MONTH AND YEAR (MMYY)
    // ---------------------------------------------------------------------
    local.BatchTimestampWorkArea.TestDateDd =
      NumberToString(import.MonthlyObligeeSummary.Year, 14, 2);
    local.BatchTimestampWorkArea.TextDateMm =
      NumberToString(import.MonthlyObligeeSummary.Month, 14, 2);
    local.Temp.Value = local.BatchTimestampWorkArea.TextDateMm + local
      .BatchTimestampWorkArea.TestDateDd;

    // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
    // *****************
    local.LastPosition.Count = 7;
    local.Final.Value =
      Substring(local.Final.Value, 1, local.LastPosition.Count) + (
        local.Temp.Value ?? "");

    // **************************************************************************
    // ---------------------------------------------------------------------
    //     AMOUNT
    // ---------------------------------------------------------------------
    local.Temp.Value =
      NumberToString((long)import.MonthlyObligeeSummary.
        CollectionsDisbursedToAr.GetValueOrDefault(), 10, 6);

    // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
    // *****************
    local.LastPosition.Count = 11;
    local.Final.Value =
      Substring(local.Final.Value, 1, local.LastPosition.Count) + (
        local.Temp.Value ?? "");

    // **************************************************************************
    export.InterfaceAlert.NoteText = local.Final.Value ?? "";
    UseSpCreateOutgoingExtAlert();
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.ReadCsePerson.Type1 = useExport.AbendData.Type1;
    local.Ae.Flag = useExport.Ae.Flag;
  }

  private void UseSpCreateOutgoingExtAlert()
  {
    var useImport = new SpCreateOutgoingExtAlert.Import();
    var useExport = new SpCreateOutgoingExtAlert.Export();

    useImport.KscParticipation.Flag = local.Kscares.Flag;
    useImport.InterfaceAlert.Assign(export.InterfaceAlert);

    Call(SpCreateOutgoingExtAlert.Execute, useImport, useExport);
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
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

    private DateWorkArea dateWorkArea;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    private InterfaceAlert interfaceAlert;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public MonthlyObligeeSummary Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public FieldValue Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of LastPosition.
    /// </summary>
    [JsonPropertyName("lastPosition")]
    public Common LastPosition
    {
      get => lastPosition ??= new();
      set => lastPosition = value;
    }

    /// <summary>
    /// A value of Final.
    /// </summary>
    [JsonPropertyName("final")]
    public FieldValue Final
    {
      get => final ??= new();
      set => final = value;
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
    /// A value of ReadCsePerson.
    /// </summary>
    [JsonPropertyName("readCsePerson")]
    public AbendData ReadCsePerson
    {
      get => readCsePerson ??= new();
      set => readCsePerson = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Kscares.
    /// </summary>
    [JsonPropertyName("kscares")]
    public Common Kscares
    {
      get => kscares ??= new();
      set => kscares = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    private MonthlyObligeeSummary hold;
    private DateWorkArea dateWorkArea;
    private FieldValue temp;
    private Common lastPosition;
    private FieldValue final;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData readCsePerson;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common kscares;
    private Common ae;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    private CsePersonAccount obligee1;
    private CsePerson obligee2;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private CsePerson child;
  }
#endregion
}
