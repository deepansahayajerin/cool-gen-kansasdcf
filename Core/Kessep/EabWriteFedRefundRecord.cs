// Program: EAB_WRITE_FED_REFUND_RECORD, ID: 372537534, model: 746.
// Short name: SWEXFE04
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_WRITE_FED_REFUND_RECORD.
/// </summary>
[Serializable]
public partial class EabWriteFedRefundRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_FED_REFUND_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWriteFedRefundRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWriteFedRefundRecord.
  /// </summary>
  public EabWriteFedRefundRecord(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXFE04", context, import, export, 0);
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
    /// A value of FdsoNtfyFedRefTapeRec1999.
    /// </summary>
    [JsonPropertyName("fdsoNtfyFedRefTapeRec1999")]
    [Member(Index = 1, Members = new[]
    {
      "SubmittingState",
      "LocalCode",
      "Ssn",
      "CaseNumber",
      "LastName",
      "FirstName",
      "RefundAmount",
      "TransactionType",
      "CaseType",
      "TransferState",
      "TransferLocalCode",
      "ProcessYear",
      "AddressLine1",
      "AddressLine2",
      "City",
      "StateCode",
      "ZipCode9",
      "DateIssuedPreOffset",
      "OffsetExclusionIndicatorType",
      "Filler"
    })]
    public FdsoNtfyFedRefTapeRec1999 FdsoNtfyFedRefTapeRec1999
    {
      get => fdsoNtfyFedRefTapeRec1999 ??= new();
      set => fdsoNtfyFedRefTapeRec1999 = value;
    }

    /// <summary>
    /// A value of ExternalParms.
    /// </summary>
    [JsonPropertyName("externalParms")]
    [Member(Index = 2, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine130",
      "TextLine80",
      "TextLine8"
    })]
    public External ExternalParms
    {
      get => externalParms ??= new();
      set => externalParms = value;
    }

    private FdsoNtfyFedRefTapeRec1999 fdsoNtfyFedRefTapeRec1999;
    private External externalParms;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExternalParms.
    /// </summary>
    [JsonPropertyName("externalParms")]
    [Member(Index = 1, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine130",
      "TextLine80",
      "TextLine8"
    })]
    public External ExternalParms
    {
      get => externalParms ??= new();
      set => externalParms = value;
    }

    private External externalParms;
  }
#endregion
}
