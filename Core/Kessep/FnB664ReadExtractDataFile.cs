// Program: FN_B664_READ_EXTRACT_DATA_FILE, ID: 371232928, model: 746.
// Short name: SWEXFE41
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B664_READ_EXTRACT_DATA_FILE.
/// </summary>
[Serializable]
public partial class FnB664ReadExtractDataFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B664_READ_EXTRACT_DATA_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB664ReadExtractDataFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB664ReadExtractDataFile.
  /// </summary>
  public FnB664ReadExtractDataFile(IContext context, Import import,
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
      "SWEXFE41", context, import, export, EabOptions.Hpvp);
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "CourtOrderAppliedTo",
      "CollectionDt",
      "AppliedToCode"
    })]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Retained.
    /// </summary>
    [JsonPropertyName("retained")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Amount" })]
    public Collection Retained
    {
      get => retained ??= new();
      set => retained = value;
    }

    /// <summary>
    /// A value of ForwardedToFamily.
    /// </summary>
    [JsonPropertyName("forwardedToFamily")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Amount" })]
    public Collection ForwardedToFamily
    {
      get => forwardedToFamily ??= new();
      set => forwardedToFamily = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    [Member(Index = 7, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier" })]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private CsePerson ar;
    private CsePerson obligor;
    private Collection collection;
    private Collection retained;
    private Collection forwardedToFamily;
    private EabFileHandling eabFileHandling;
    private ObligationType obligationType;
  }
#endregion
}
