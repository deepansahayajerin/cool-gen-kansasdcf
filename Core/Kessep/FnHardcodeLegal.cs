// Program: FN_HARDCODE_LEGAL, ID: 372084589, model: 746.
// Short name: SWE00482
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_HARDCODE_LEGAL.
/// </summary>
[Serializable]
public partial class FnHardcodeLegal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_HARDCODE_LEGAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnHardcodeLegal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnHardcodeLegal.
  /// </summary>
  public FnHardcodeLegal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************
    // 03/23/98	Siraj Konkader		ZDEL cleanup
    // ************************************************************
    // *** The last four export views were left without attributes, so the 
    // genned code would not compile.  Could not delete them, so I renamed and
    // added an attribute.  Delete whenever possible.  ***
    export.NonFinancial.DetailType = "N";
    export.Financial.DetailType = "F";
    export.Motion.Classification = "M";
    export.Order.Classification = "O";
    export.Petition.Classification = "P";
    export.Answer.Classification = "A";
    export.Notice.Classification = "N";
    export.Transmittal.Classification = "T";
    export.Judgement.Classification = "J";
    export.Supported.AccountType = "S";
    export.Obligor.AccountType = "R";
    export.Obligee.AccountType = "E";
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Judgement.
    /// </summary>
    [JsonPropertyName("judgement")]
    public LegalAction Judgement
    {
      get => judgement ??= new();
      set => judgement = value;
    }

    /// <summary>
    /// A value of Financial.
    /// </summary>
    [JsonPropertyName("financial")]
    public LegalActionDetail Financial
    {
      get => financial ??= new();
      set => financial = value;
    }

    /// <summary>
    /// A value of NonFinancial.
    /// </summary>
    [JsonPropertyName("nonFinancial")]
    public LegalActionDetail NonFinancial
    {
      get => nonFinancial ??= new();
      set => nonFinancial = value;
    }

    /// <summary>
    /// A value of Motion.
    /// </summary>
    [JsonPropertyName("motion")]
    public LegalAction Motion
    {
      get => motion ??= new();
      set => motion = value;
    }

    /// <summary>
    /// A value of Order.
    /// </summary>
    [JsonPropertyName("order")]
    public LegalAction Order
    {
      get => order ??= new();
      set => order = value;
    }

    /// <summary>
    /// A value of Petition.
    /// </summary>
    [JsonPropertyName("petition")]
    public LegalAction Petition
    {
      get => petition ??= new();
      set => petition = value;
    }

    /// <summary>
    /// A value of Transmittal.
    /// </summary>
    [JsonPropertyName("transmittal")]
    public LegalAction Transmittal
    {
      get => transmittal ??= new();
      set => transmittal = value;
    }

    /// <summary>
    /// A value of Notice.
    /// </summary>
    [JsonPropertyName("notice")]
    public LegalAction Notice
    {
      get => notice ??= new();
      set => notice = value;
    }

    /// <summary>
    /// A value of Answer.
    /// </summary>
    [JsonPropertyName("answer")]
    public LegalAction Answer
    {
      get => answer ??= new();
      set => answer = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public LegalActionPerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public LegalActionPerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public LegalActionPerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of ZdelExportSpousal.
    /// </summary>
    [JsonPropertyName("zdelExportSpousal")]
    public LegalActionDetail ZdelExportSpousal
    {
      get => zdelExportSpousal ??= new();
      set => zdelExportSpousal = value;
    }

    /// <summary>
    /// A value of ZdelExportMedical.
    /// </summary>
    [JsonPropertyName("zdelExportMedical")]
    public LegalActionDetail ZdelExportMedical
    {
      get => zdelExportMedical ??= new();
      set => zdelExportMedical = value;
    }

    /// <summary>
    /// A value of ZdelExportHealth.
    /// </summary>
    [JsonPropertyName("zdelExportHealth")]
    public LegalActionDetail ZdelExportHealth
    {
      get => zdelExportHealth ??= new();
      set => zdelExportHealth = value;
    }

    /// <summary>
    /// A value of ZdelExportChildSupport.
    /// </summary>
    [JsonPropertyName("zdelExportChildSupport")]
    public LegalActionDetail ZdelExportChildSupport
    {
      get => zdelExportChildSupport ??= new();
      set => zdelExportChildSupport = value;
    }

    private LegalAction judgement;
    private LegalActionDetail financial;
    private LegalActionDetail nonFinancial;
    private LegalAction motion;
    private LegalAction order;
    private LegalAction petition;
    private LegalAction transmittal;
    private LegalAction notice;
    private LegalAction answer;
    private LegalActionPerson supported;
    private LegalActionPerson obligor;
    private LegalActionPerson obligee;
    private LegalActionDetail zdelExportSpousal;
    private LegalActionDetail zdelExportMedical;
    private LegalActionDetail zdelExportHealth;
    private LegalActionDetail zdelExportChildSupport;
  }
#endregion
}
