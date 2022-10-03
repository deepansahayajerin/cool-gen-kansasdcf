// Program: FN_B643_COURT_ORDER, ID: 372683821, model: 746.
// Short name: SWE02388
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_COURT_ORDER.
/// </summary>
[Serializable]
public partial class FnB643CourtOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_COURT_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643CourtOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643CourtOrder.
  /// </summary>
  public FnB643CourtOrder(IContext context, Import import, Export export):
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
    // REMOVE THE ASTERISK FROM COURT ORDER NUMBER
    // **************************************************************
    if (CharAt(import.LegalAction.StandardNumber, 6) == '*')
    {
      local.LegalAction.StandardNumber =
        Substring(import.LegalAction.StandardNumber, 20, 3, 3) + " " + Substring
        (import.LegalAction.StandardNumber, 20, 7, 14);
    }
    else
    {
      local.LegalAction.StandardNumber = import.LegalAction.StandardNumber ?? ""
        ;
    }

    // **************************************************************
    // WRITE COURT ORDER (REC TYPE = 5)
    // **************************************************************
    local.RecordType.ActionEntry = "05";
    local.EabFileHandling.Action = "WRITE";
    ++export.VendorFileRecordCount.Count;
    ++export.SortSequenceNumber.Count;

    if (ReadCodeCodeValue())
    {
      local.VariableLine1.RptDetail = "Activity for Court Order: " + TrimEnd
        (local.LegalAction.StandardNumber) + " - " + TrimEnd
        (entities.CodeValue.Description) + " County" + "";
    }
    else if (ReadFips())
    {
      local.VariableLine1.RptDetail = "Activity for Court Order: " + TrimEnd
        (local.LegalAction.StandardNumber) + " - " + TrimEnd
        (entities.Fips.CountyDescription) + " County" + "";
    }
    else
    {
      local.VariableLine1.RptDetail = "Activity for Court Order: " + TrimEnd
        (local.LegalAction.StandardNumber) + " - " + TrimEnd
        (import.FipsTribAddress.County) + " " + "County";
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
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCodeCodeValue()
  {
    entities.Code.Populated = false;
    entities.CodeValue.Populated = false;

    return Read("ReadCodeCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.County.Id);
        db.SetNullableString(
          command, "county", import.FipsTribAddress.County ?? "");
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 0);
        entities.CodeValue.Id = db.GetInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.Code.Populated = true;
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetNullableString(
          command, "countyAbbr", import.FipsTribAddress.County ?? "");
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public Code County
    {
      get => county ??= new();
      set => county = value;
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

    private Common sortSequenceNumber;
    private Common vendorFileRecordCount;
    private FipsTribAddress fipsTribAddress;
    private LegalAction legalAction;
    private Code county;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private Common stmtNumber;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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

    private LegalAction legalAction;
    private Common recordType;
    private EabFileHandling eabFileHandling;
    private EabReportSend variableLine1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    private Fips fips;
    private Code code;
    private CodeValue codeValue;
  }
#endregion
}
