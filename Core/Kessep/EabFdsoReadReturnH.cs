// Program: EAB_FDSO_READ_RETURN_H, ID: 372668119, model: 746.
// Short name: SWEXLE55
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_FDSO_READ_RETURN_H.
/// </para>
/// <para>
/// Read external file sent from OCSE for FDSO return processing.
/// </para>
/// </summary>
[Serializable]
public partial class EabFdsoReadReturnH: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_FDSO_READ_RETURN_H program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabFdsoReadReturnH(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabFdsoReadReturnH.
  /// </summary>
  public EabFdsoReadReturnH(IContext context, Import import, Export export):
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
      "SWEXLE55", context, import, export, EabOptions.Hpvp);
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
    /// A value of FdsoReturnHTotal.
    /// </summary>
    [JsonPropertyName("fdsoReturnHTotal")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "SubmittingState",
      "Control",
      "TanfAccepted",
      "TanfRejected",
      "NontanfAccepted",
      "NontanfRejected"
    })]
    public FdsoReturnHTotal FdsoReturnHTotal
    {
      get => fdsoReturnHTotal ??= new();
      set => fdsoReturnHTotal = value;
    }

    /// <summary>
    /// A value of FdsoReturnH.
    /// </summary>
    [JsonPropertyName("fdsoReturnH")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "SubmittingState",
      "LocalCode",
      "Ssn",
      "CaseNumber",
      "LastName",
      "FirstName",
      "AmountOwed",
      "TransactionType",
      "CaseTypeInd",
      "TransferState",
      "LocalForTransfer",
      "ProcessYear",
      "OffsetExclusionType",
      "Errcode1",
      "Errcode2",
      "Errcode3",
      "Errcode4",
      "Errcode5",
      "Errcode6",
      "FedReturnedLastName"
    })]
    public FdsoReturnH FdsoReturnH
    {
      get => fdsoReturnH ??= new();
      set => fdsoReturnH = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private FdsoReturnHTotal fdsoReturnHTotal;
    private FdsoReturnH fdsoReturnH;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
