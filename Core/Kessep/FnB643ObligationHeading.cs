// Program: FN_B643_OBLIGATION_HEADING, ID: 372683792, model: 746.
// Short name: SWE02391
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_OBLIGATION_HEADING.
/// </summary>
[Serializable]
public partial class FnB643ObligationHeading: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_OBLIGATION_HEADING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643ObligationHeading(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643ObligationHeading.
  /// </summary>
  public FnB643ObligationHeading(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.VendorFileRecordCount.Count = import.VendorFileRecordCount.Count;
    export.SortSequenceNumber.Count = import.SortSequenceNumber.Count;

    // **************************************************************
    // WRITE ACTIVITY HEADER (REC TYPE = 6)
    // **************************************************************
    local.RecordType.ActionEntry = "06";
    local.EabFileHandling.Action = "WRITE";
    ++export.VendorFileRecordCount.Count;
    ++export.SortSequenceNumber.Count;

    if (Equal(import.ObligationType.Code, "IRS NEG"))
    {
      local.VariableLine2.RptDetail = "RECOVERY";
    }
    else
    {
      local.VariableLine2.RptDetail = import.ObligationType.Name;
    }

    UseEabCreateVendorFile();
  }

  private void UseEabCreateVendorFile()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.VariableLine2.RptDetail = local.VariableLine2.RptDetail;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public Code County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of NewCourtOrder.
    /// </summary>
    [JsonPropertyName("newCourtOrder")]
    public Common NewCourtOrder
    {
      get => newCourtOrder ??= new();
      set => newCourtOrder = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    /// <summary>
    /// A value of StmtNumber.
    /// </summary>
    [JsonPropertyName("stmtNumber")]
    public Common StmtNumber
    {
      get => stmtNumber ??= new();
      set => stmtNumber = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    private Code county;
    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private ObligationType obligationType;
    private Common newCourtOrder;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private Common stmtNumber;
    private Common vendorFileRecordCount;
    private Common sortSequenceNumber;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Common sortSequenceNumber;
    private Common vendorFileRecordCount;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of VariableLine1.
    /// </summary>
    [JsonPropertyName("variableLine1")]
    public EabReportSend VariableLine1
    {
      get => variableLine1 ??= new();
      set => variableLine1 = value;
    }

    /// <summary>
    /// A value of GlobalStatementMessage.
    /// </summary>
    [JsonPropertyName("globalStatementMessage")]
    public GlobalStatementMessage GlobalStatementMessage
    {
      get => globalStatementMessage ??= new();
      set => globalStatementMessage = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of VariableLine2.
    /// </summary>
    [JsonPropertyName("variableLine2")]
    public EabReportSend VariableLine2
    {
      get => variableLine2 ??= new();
      set => variableLine2 = value;
    }

    private Common recordType;
    private EabReportSend variableLine1;
    private GlobalStatementMessage globalStatementMessage;
    private CsePersonAddress csePersonAddress;
    private EabFileHandling eabFileHandling;
    private EabReportSend variableLine2;
  }
#endregion
}
