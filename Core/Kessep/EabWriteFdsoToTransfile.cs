// Program: EAB_WRITE_FDSO_TO_TRANSFILE, ID: 372667655, model: 746.
// Short name: SWEXLE88
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_WRITE_FDSO_TO_TRANSFILE.
/// </para>
/// <para>
/// write fdso work view to fdso transmission file.
/// </para>
/// </summary>
[Serializable]
public partial class EabWriteFdsoToTransfile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_FDSO_TO_TRANSFILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWriteFdsoToTransfile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWriteFdsoToTransfile.
  /// </summary>
  public EabWriteFdsoToTransfile(IContext context, Import import, Export export):
    
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
      "SWEXLE88", context, import, export, EabOptions.Hpvp);
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
    /// A value of FdsoCertificationTapeRecord.
    /// </summary>
    [JsonPropertyName("fdsoCertificationTapeRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "SubmittingState",
      "LocalCode",
      "Ssn",
      "CaseNumber",
      "LastName",
      "FirstName",
      "TransactionType",
      "CaseTypeInd",
      "TransferState",
      "LocalForTransfer",
      "ProcessYear",
      "AddressLine1",
      "AddressLine2",
      "City",
      "StateCode",
      "ZipCode",
      "OffsetExclusionType",
      "AdcAmount",
      "NonAdcAmount"
    })]
    public FdsoCertificationTapeRecord FdsoCertificationTapeRecord
    {
      get => fdsoCertificationTapeRecord ??= new();
      set => fdsoCertificationTapeRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private FdsoCertificationTapeRecord fdsoCertificationTapeRecord;
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
