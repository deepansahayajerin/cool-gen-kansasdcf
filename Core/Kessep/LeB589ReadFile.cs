// Program: LE_B589_READ_FILE, ID: 1902492980, model: 746.
// Short name: SWEXLR07
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B589_READ_FILE.
/// </summary>
[Serializable]
public partial class LeB589ReadFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B589_READ_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB589ReadFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB589ReadFile.
  /// </summary>
  public LeB589ReadFile(IContext context, Import import, Export export):
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
      "SWEXLR07", context, import, export, EabOptions.Hpvp);
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
    /// A value of DocCode.
    /// </summary>
    [JsonPropertyName("docCode")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea DocCode
    {
      get => docCode ??= new();
      set => docCode = value;
    }

    /// <summary>
    /// A value of ControlNum.
    /// </summary>
    [JsonPropertyName("controlNum")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text22" })]
    public WorkArea ControlNum
    {
      get => controlNum ??= new();
      set => controlNum = value;
    }

    /// <summary>
    /// A value of StFipsCode.
    /// </summary>
    [JsonPropertyName("stFipsCode")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text5" })]
    public WorkArea StFipsCode
    {
      get => stFipsCode ??= new();
      set => stFipsCode = value;
    }

    /// <summary>
    /// A value of HeaderEin.
    /// </summary>
    [JsonPropertyName("headerEin")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text9" })]
    public WorkArea HeaderEin
    {
      get => headerEin ??= new();
      set => headerEin = value;
    }

    /// <summary>
    /// A value of HeaderPrimaryEin.
    /// </summary>
    [JsonPropertyName("headerPrimaryEin")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text9" })]
    public WorkArea HeaderPrimaryEin
    {
      get => headerPrimaryEin ??= new();
      set => headerPrimaryEin = value;
    }

    /// <summary>
    /// A value of HeaderCreateDate.
    /// </summary>
    [JsonPropertyName("headerCreateDate")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea HeaderCreateDate
    {
      get => headerCreateDate ??= new();
      set => headerCreateDate = value;
    }

    /// <summary>
    /// A value of HeaderCreateTime.
    /// </summary>
    [JsonPropertyName("headerCreateTime")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Time" })]
    public DateWorkArea HeaderCreateTime
    {
      get => headerCreateTime ??= new();
      set => headerCreateTime = value;
    }

    /// <summary>
    /// A value of ErrFieldName.
    /// </summary>
    [JsonPropertyName("errFieldName")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Text18" })]
    public WorkArea ErrFieldName
    {
      get => errFieldName ??= new();
      set => errFieldName = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Count" })]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Count" })]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
    }

    /// <summary>
    /// A value of EmployeeSent.
    /// </summary>
    [JsonPropertyName("employeeSent")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Count" })]
    public Common EmployeeSent
    {
      get => employeeSent ??= new();
      set => employeeSent = value;
    }

    /// <summary>
    /// A value of StateSent.
    /// </summary>
    [JsonPropertyName("stateSent")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Count" })]
    public Common StateSent
    {
      get => stateSent ??= new();
      set => stateSent = value;
    }

    /// <summary>
    /// A value of DocActionCode.
    /// </summary>
    [JsonPropertyName("docActionCode")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea DocActionCode
    {
      get => docActionCode ??= new();
      set => docActionCode = value;
    }

    /// <summary>
    /// A value of CaseId.
    /// </summary>
    [JsonPropertyName("caseId")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Text15" })]
    public WorkArea CaseId
    {
      get => caseId ??= new();
      set => caseId = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Ein" })]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of Employee.
    /// </summary>
    [JsonPropertyName("employee")]
    [Member(Index = 16, AccessFields = false, Members = new[]
    {
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet Employee
    {
      get => employee ??= new();
      set => employee = value;
    }

    /// <summary>
    /// A value of EmployeeSuffix.
    /// </summary>
    [JsonPropertyName("employeeSuffix")]
    [Member(Index = 17, AccessFields = false, Members = new[] { "Text4" })]
    public WorkArea EmployeeSuffix
    {
      get => employeeSuffix ??= new();
      set => employeeSuffix = value;
    }

    /// <summary>
    /// A value of DocTrackingNumb.
    /// </summary>
    [JsonPropertyName("docTrackingNumb")]
    [Member(Index = 18, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea DocTrackingNumb
    {
      get => docTrackingNumb ??= new();
      set => docTrackingNumb = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Member(Index = 19, AccessFields = false, Members
      = new[] { "StandardNumber" })]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of RecDispStatsCd.
    /// </summary>
    [JsonPropertyName("recDispStatsCd")]
    [Member(Index = 20, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea RecDispStatsCd
    {
      get => recDispStatsCd ??= new();
      set => recDispStatsCd = value;
    }

    /// <summary>
    /// A value of DisposionReasonCd.
    /// </summary>
    [JsonPropertyName("disposionReasonCd")]
    [Member(Index = 21, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea DisposionReasonCd
    {
      get => disposionReasonCd ??= new();
      set => disposionReasonCd = value;
    }

    /// <summary>
    /// A value of TerminationDate.
    /// </summary>
    [JsonPropertyName("terminationDate")]
    [Member(Index = 22, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea TerminationDate
    {
      get => terminationDate ??= new();
      set => terminationDate = value;
    }

    /// <summary>
    /// A value of NcpLast.
    /// </summary>
    [JsonPropertyName("ncpLast")]
    [Member(Index = 23, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode",
      "Zip4"
    })]
    public CsePersonAddress NcpLast
    {
      get => ncpLast ??= new();
      set => ncpLast = value;
    }

    /// <summary>
    /// A value of FinallyPayment.
    /// </summary>
    [JsonPropertyName("finallyPayment")]
    [Member(Index = 24, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea FinallyPayment
    {
      get => finallyPayment ??= new();
      set => finallyPayment = value;
    }

    /// <summary>
    /// A value of FinalPayment.
    /// </summary>
    [JsonPropertyName("finalPayment")]
    [Member(Index = 25, AccessFields = false, Members
      = new[] { "AverageCurrency" })]
    public Common FinalPayment
    {
      get => finalPayment ??= new();
      set => finalPayment = value;
    }

    /// <summary>
    /// A value of NewEmployer.
    /// </summary>
    [JsonPropertyName("newEmployer")]
    [Member(Index = 26, AccessFields = false, Members = new[] { "Name" })]
    public Employer NewEmployer
    {
      get => newEmployer ??= new();
      set => newEmployer = value;
    }

    /// <summary>
    /// A value of NewEmployerAddress.
    /// </summary>
    [JsonPropertyName("newEmployerAddress")]
    [Member(Index = 27, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode",
      "Zip4"
    })]
    public EmployerAddress NewEmployerAddress
    {
      get => newEmployerAddress ??= new();
      set => newEmployerAddress = value;
    }

    /// <summary>
    /// A value of LumpSumDateWorkArea.
    /// </summary>
    [JsonPropertyName("lumpSumDateWorkArea")]
    [Member(Index = 28, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea LumpSumDateWorkArea
    {
      get => lumpSumDateWorkArea ??= new();
      set => lumpSumDateWorkArea = value;
    }

    /// <summary>
    /// A value of LumpSumCommon.
    /// </summary>
    [JsonPropertyName("lumpSumCommon")]
    [Member(Index = 29, AccessFields = false, Members
      = new[] { "AverageCurrency" })]
    public Common LumpSumCommon
    {
      get => lumpSumCommon ??= new();
      set => lumpSumCommon = value;
    }

    /// <summary>
    /// A value of LumpSumType.
    /// </summary>
    [JsonPropertyName("lumpSumType")]
    [Member(Index = 30, AccessFields = false, Members = new[] { "Text35" })]
    public WorkArea LumpSumType
    {
      get => lumpSumType ??= new();
      set => lumpSumType = value;
    }

    /// <summary>
    /// A value of NcpLastKnown.
    /// </summary>
    [JsonPropertyName("ncpLastKnown")]
    [Member(Index = 31, AccessFields = false, Members = new[]
    {
      "Type1",
      "OtherAreaCode",
      "OtherNumber"
    })]
    public CsePerson NcpLastKnown
    {
      get => ncpLastKnown ??= new();
      set => ncpLastKnown = value;
    }

    /// <summary>
    /// A value of Export1StErrorFieldName.
    /// </summary>
    [JsonPropertyName("export1StErrorFieldName")]
    [Member(Index = 32, AccessFields = false, Members = new[] { "Text32" })]
    public WorkArea Export1StErrorFieldName
    {
      get => export1StErrorFieldName ??= new();
      set => export1StErrorFieldName = value;
    }

    /// <summary>
    /// A value of Export2NdErrorFieldName.
    /// </summary>
    [JsonPropertyName("export2NdErrorFieldName")]
    [Member(Index = 33, AccessFields = false, Members = new[] { "Text32" })]
    public WorkArea Export2NdErrorFieldName
    {
      get => export2NdErrorFieldName ??= new();
      set => export2NdErrorFieldName = value;
    }

    /// <summary>
    /// A value of MultipleErrorInd.
    /// </summary>
    [JsonPropertyName("multipleErrorInd")]
    [Member(Index = 34, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea MultipleErrorInd
    {
      get => multipleErrorInd ??= new();
      set => multipleErrorInd = value;
    }

    /// <summary>
    /// A value of Corrected.
    /// </summary>
    [JsonPropertyName("corrected")]
    [Member(Index = 35, AccessFields = false, Members = new[] { "Ein" })]
    public Employer Corrected
    {
      get => corrected ??= new();
      set => corrected = value;
    }

    /// <summary>
    /// A value of MultiIwoStCode.
    /// </summary>
    [JsonPropertyName("multiIwoStCode")]
    [Member(Index = 36, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea MultiIwoStCode
    {
      get => multiIwoStCode ??= new();
      set => multiIwoStCode = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 37, AccessFields = false, Members = new[]
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

    private WorkArea docCode;
    private WorkArea controlNum;
    private WorkArea stFipsCode;
    private WorkArea headerEin;
    private WorkArea headerPrimaryEin;
    private DateWorkArea headerCreateDate;
    private DateWorkArea headerCreateTime;
    private WorkArea errFieldName;
    private Common batch;
    private Common record;
    private Common employeeSent;
    private Common stateSent;
    private WorkArea docActionCode;
    private WorkArea caseId;
    private Employer employer;
    private CsePersonsWorkSet employee;
    private WorkArea employeeSuffix;
    private TextWorkArea docTrackingNumb;
    private LegalAction legalAction;
    private WorkArea recDispStatsCd;
    private WorkArea disposionReasonCd;
    private DateWorkArea terminationDate;
    private CsePersonAddress ncpLast;
    private DateWorkArea finallyPayment;
    private Common finalPayment;
    private Employer newEmployer;
    private EmployerAddress newEmployerAddress;
    private DateWorkArea lumpSumDateWorkArea;
    private Common lumpSumCommon;
    private WorkArea lumpSumType;
    private CsePerson ncpLastKnown;
    private WorkArea export1StErrorFieldName;
    private WorkArea export2NdErrorFieldName;
    private WorkArea multipleErrorInd;
    private Employer corrected;
    private WorkArea multiIwoStCode;
    private External external;
  }
#endregion
}
