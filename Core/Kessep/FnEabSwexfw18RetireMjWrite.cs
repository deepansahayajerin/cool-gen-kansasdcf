// Program: FN_EAB_SWEXFW18_RETIRE_MJ_WRITE, ID: 371359359, model: 746.
// Short name: SWEXFW18
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EAB_SWEXFW18_RETIRE_MJ_WRITE.
/// </summary>
[Serializable]
public partial class FnEabSwexfw18RetireMjWrite: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EAB_SWEXFW18_RETIRE_MJ_WRITE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEabSwexfw18RetireMjWrite(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEabSwexfw18RetireMjWrite.
  /// </summary>
  public FnEabSwexfw18RetireMjWrite(IContext context, Import import,
    Export export):
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
      "SWEXFW18", context, import, export, EabOptions.Hpvp);
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

    /// <summary>
    /// A value of NcpInfo.
    /// </summary>
    [JsonPropertyName("ncpInfo")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Number",
      "FirstName",
      "LastName"
    })]
    public CsePersonsWorkSet NcpInfo
    {
      get => ncpInfo ??= new();
      set => ncpInfo = value;
    }

    /// <summary>
    /// A value of ObligCreatedDate.
    /// </summary>
    [JsonPropertyName("obligCreatedDate")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea ObligCreatedDate
    {
      get => obligCreatedDate ??= new();
      set => obligCreatedDate = value;
    }

    /// <summary>
    /// A value of CourtOrderNumber.
    /// </summary>
    [JsonPropertyName("courtOrderNumber")]
    [Member(Index = 4, AccessFields = false, Members
      = new[] { "StandardNumber" })]
    public LegalAction CourtOrderNumber
    {
      get => courtOrderNumber ??= new();
      set => courtOrderNumber = value;
    }

    /// <summary>
    /// A value of CourtOrderAmt.
    /// </summary>
    [JsonPropertyName("courtOrderAmt")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Amount" })]
    public ObligationTransaction CourtOrderAmt
    {
      get => courtOrderAmt ??= new();
      set => courtOrderAmt = value;
    }

    /// <summary>
    /// A value of Covered.
    /// </summary>
    [JsonPropertyName("covered")]
    [Member(Index = 6, AccessFields = false, Members
      = new[] { "CoveredPrdStartDt", "CoveredPrdEndDt" })]
    public DebtDetail Covered
    {
      get => covered ??= new();
      set => covered = value;
    }

    /// <summary>
    /// A value of DueDate.
    /// </summary>
    [JsonPropertyName("dueDate")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "DueDt" })]
    public DebtDetail DueDate
    {
      get => dueDate ??= new();
      set => dueDate = value;
    }

    /// <summary>
    /// A value of BalDueAtRetiredDt.
    /// </summary>
    [JsonPropertyName("balDueAtRetiredDt")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "BalanceDueAmt" })
      ]
    public DebtDetail BalDueAtRetiredDt
    {
      get => balDueAtRetiredDt ??= new();
      set => balDueAtRetiredDt = value;
    }

    /// <summary>
    /// A value of SupportedInfo.
    /// </summary>
    [JsonPropertyName("supportedInfo")]
    [Member(Index = 9, AccessFields = false, Members = new[]
    {
      "Number",
      "FirstName",
      "LastName"
    })]
    public CsePersonsWorkSet SupportedInfo
    {
      get => supportedInfo ??= new();
      set => supportedInfo = value;
    }

    /// <summary>
    /// A value of CaseNbr.
    /// </summary>
    [JsonPropertyName("caseNbr")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Number" })]
    public Case1 CaseNbr
    {
      get => caseNbr ??= new();
      set => caseNbr = value;
    }

    /// <summary>
    /// A value of SpName.
    /// </summary>
    [JsonPropertyName("spName")]
    [Member(Index = 11, AccessFields = false, Members
      = new[] { "LastName", "FirstName" })]
    public ServiceProvider SpName
    {
      get => spName ??= new();
      set => spName = value;
    }

    /// <summary>
    /// A value of OfficeInfo.
    /// </summary>
    [JsonPropertyName("officeInfo")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "SystemGeneratedId", "Name" })]
    public Office OfficeInfo
    {
      get => officeInfo ??= new();
      set => officeInfo = value;
    }

    /// <summary>
    /// A value of Remarks.
    /// </summary>
    [JsonPropertyName("remarks")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea Remarks
    {
      get => remarks ??= new();
      set => remarks = value;
    }

    /// <summary>
    /// A value of TypeOfProcess.
    /// </summary>
    [JsonPropertyName("typeOfProcess")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea TypeOfProcess
    {
      get => typeOfProcess ??= new();
      set => typeOfProcess = value;
    }

    /// <summary>
    /// A value of ObligationId.
    /// </summary>
    [JsonPropertyName("obligationId")]
    [Member(Index = 15, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier" })]
    public Obligation ObligationId
    {
      get => obligationId ??= new();
      set => obligationId = value;
    }

    /// <summary>
    /// A value of CurrentBalDue.
    /// </summary>
    [JsonPropertyName("currentBalDue")]
    [Member(Index = 16, AccessFields = false, Members
      = new[] { "BalanceDueAmt" })]
    public DebtDetail CurrentBalDue
    {
      get => currentBalDue ??= new();
      set => currentBalDue = value;
    }

    /// <summary>
    /// A value of CreatedBy.
    /// </summary>
    [JsonPropertyName("createdBy")]
    [Member(Index = 17, AccessFields = false, Members = new[] { "CreatedBy" })]
    public DebtDetailStatusHistory CreatedBy
    {
      get => createdBy ??= new();
      set => createdBy = value;
    }

    private EabFileHandling eabFileHandling;
    private CsePersonsWorkSet ncpInfo;
    private DateWorkArea obligCreatedDate;
    private LegalAction courtOrderNumber;
    private ObligationTransaction courtOrderAmt;
    private DebtDetail covered;
    private DebtDetail dueDate;
    private DebtDetail balDueAtRetiredDt;
    private CsePersonsWorkSet supportedInfo;
    private Case1 caseNbr;
    private ServiceProvider spName;
    private Office officeInfo;
    private TextWorkArea remarks;
    private TextWorkArea typeOfProcess;
    private Obligation obligationId;
    private DebtDetail currentBalDue;
    private DebtDetailStatusHistory createdBy;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }
#endregion
}
