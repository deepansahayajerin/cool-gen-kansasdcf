// Program: LE_B588_READ_FILE, ID: 1902488501, model: 746.
// Short name: SWEXLR06
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B588_READ_FILE.
/// </summary>
[Serializable]
public partial class LeB588ReadFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B588_READ_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB588ReadFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB588ReadFile.
  /// </summary>
  public LeB588ReadFile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXLR06", context, import, export, EabOptions.Hpvp);
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
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ErrFieldName.
    /// </summary>
    [JsonPropertyName("errFieldName")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Text18" })]
    public WorkArea ErrFieldName
    {
      get => errFieldName ??= new();
      set => errFieldName = value;
    }

    /// <summary>
    /// A value of HeaderCreateTime.
    /// </summary>
    [JsonPropertyName("headerCreateTime")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Time" })]
    public DateWorkArea HeaderCreateTime
    {
      get => headerCreateTime ??= new();
      set => headerCreateTime = value;
    }

    /// <summary>
    /// A value of HeaderCreateDate.
    /// </summary>
    [JsonPropertyName("headerCreateDate")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea HeaderCreateDate
    {
      get => headerCreateDate ??= new();
      set => headerCreateDate = value;
    }

    /// <summary>
    /// A value of HeaderPrimaryEin.
    /// </summary>
    [JsonPropertyName("headerPrimaryEin")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text9" })]
    public WorkArea HeaderPrimaryEin
    {
      get => headerPrimaryEin ??= new();
      set => headerPrimaryEin = value;
    }

    /// <summary>
    /// A value of HeaderEin.
    /// </summary>
    [JsonPropertyName("headerEin")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text9" })]
    public WorkArea HeaderEin
    {
      get => headerEin ??= new();
      set => headerEin = value;
    }

    /// <summary>
    /// A value of StFipsCode.
    /// </summary>
    [JsonPropertyName("stFipsCode")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text5" })]
    public WorkArea StFipsCode
    {
      get => stFipsCode ??= new();
      set => stFipsCode = value;
    }

    /// <summary>
    /// A value of ControlNum.
    /// </summary>
    [JsonPropertyName("controlNum")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text22" })]
    public WorkArea ControlNum
    {
      get => controlNum ??= new();
      set => controlNum = value;
    }

    /// <summary>
    /// A value of DocCode.
    /// </summary>
    [JsonPropertyName("docCode")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea DocCode
    {
      get => docCode ??= new();
      set => docCode = value;
    }

    /// <summary>
    /// A value of DocActionCode.
    /// </summary>
    [JsonPropertyName("docActionCode")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea DocActionCode
    {
      get => docActionCode ??= new();
      set => docActionCode = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Ein" })]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of DocTrackingNumb.
    /// </summary>
    [JsonPropertyName("docTrackingNumb")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea DocTrackingNumb
    {
      get => docTrackingNumb ??= new();
      set => docTrackingNumb = value;
    }

    /// <summary>
    /// A value of RecDispStatsCd.
    /// </summary>
    [JsonPropertyName("recDispStatsCd")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea RecDispStatsCd
    {
      get => recDispStatsCd ??= new();
      set => recDispStatsCd = value;
    }

    /// <summary>
    /// A value of DisposionReasonCd.
    /// </summary>
    [JsonPropertyName("disposionReasonCd")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea DisposionReasonCd
    {
      get => disposionReasonCd ??= new();
      set => disposionReasonCd = value;
    }

    /// <summary>
    /// A value of Export1StErrorFieldName.
    /// </summary>
    [JsonPropertyName("export1StErrorFieldName")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Text32" })]
    public WorkArea Export1StErrorFieldName
    {
      get => export1StErrorFieldName ??= new();
      set => export1StErrorFieldName = value;
    }

    /// <summary>
    /// A value of Export2NdErrorFieldName.
    /// </summary>
    [JsonPropertyName("export2NdErrorFieldName")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Text32" })]
    public WorkArea Export2NdErrorFieldName
    {
      get => export2NdErrorFieldName ??= new();
      set => export2NdErrorFieldName = value;
    }

    /// <summary>
    /// A value of MultipleErrorInd.
    /// </summary>
    [JsonPropertyName("multipleErrorInd")]
    [Member(Index = 16, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea MultipleErrorInd
    {
      get => multipleErrorInd ??= new();
      set => multipleErrorInd = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 17, AccessFields = false, Members = new[]
    {
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private WorkArea errFieldName;
    private DateWorkArea headerCreateTime;
    private DateWorkArea headerCreateDate;
    private WorkArea headerPrimaryEin;
    private WorkArea headerEin;
    private WorkArea stFipsCode;
    private WorkArea controlNum;
    private WorkArea docCode;
    private WorkArea docActionCode;
    private Employer employer;
    private TextWorkArea docTrackingNumb;
    private WorkArea recDispStatsCd;
    private WorkArea disposionReasonCd;
    private WorkArea export1StErrorFieldName;
    private WorkArea export2NdErrorFieldName;
    private WorkArea multipleErrorInd;
    private External external;
  }
#endregion
}
