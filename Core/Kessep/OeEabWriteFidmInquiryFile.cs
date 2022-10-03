// Program: OE_EAB_WRITE_FIDM_INQUIRY_FILE, ID: 374401543, model: 746.
// Short name: SWEXEE30
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_WRITE_FIDM_INQUIRY_FILE.
/// </para>
/// <para>
/// This EAB write FIDM data to output file.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabWriteFidmInquiryFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_WRITE_FIDM_INQUIRY_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabWriteFidmInquiryFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabWriteFidmInquiryFile.
  /// </summary>
  public OeEabWriteFidmInquiryFile(IContext context, Import import,
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
      "SWEXEE30", context, import, export, EabOptions.Hpvp);
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
    /// A value of ExternalFidmTrailer.
    /// </summary>
    [JsonPropertyName("externalFidmTrailer")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "RecordType", "TotalNoInquiryRec" })]
    public ExternalFidmTrailer ExternalFidmTrailer
    {
      get => externalFidmTrailer ??= new();
      set => externalFidmTrailer = value;
    }

    /// <summary>
    /// A value of ExternalFidmHeader.
    /// </summary>
    [JsonPropertyName("externalFidmHeader")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "RecordType",
      "Yyyymm",
      "DataMatchFi"
    })]
    public ExternalFidmHeader ExternalFidmHeader
    {
      get => externalFidmHeader ??= new();
      set => externalFidmHeader = value;
    }

    /// <summary>
    /// A value of ExternalFidmDetail.
    /// </summary>
    [JsonPropertyName("externalFidmDetail")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "RecordType",
      "Ssn",
      "LastName",
      "FirstName",
      "CsePersonNo",
      "Fips"
    })]
    public ExternalFidmDetail ExternalFidmDetail
    {
      get => externalFidmDetail ??= new();
      set => externalFidmDetail = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 4, AccessFields = false, Members
      = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private ExternalFidmTrailer externalFidmTrailer;
    private ExternalFidmHeader externalFidmHeader;
    private ExternalFidmDetail externalFidmDetail;
    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }
#endregion
}
