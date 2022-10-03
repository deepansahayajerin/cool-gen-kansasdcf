// Program: LE_EAB_WRITE_KDOL_MATCH_FILE, ID: 945091911, model: 746.
// Short name: SWEXLW03
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_EAB_WRITE_KDOL_MATCH_FILE.
/// </para>
/// <para>
/// Write records to be sent on KDOL file for UI matching
/// </para>
/// </summary>
[Serializable]
public partial class LeEabWriteKdolMatchFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EAB_WRITE_KDOL_MATCH_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEabWriteKdolMatchFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEabWriteKdolMatchFile.
  /// </summary>
  public LeEabWriteKdolMatchFile(IContext context, Import import, Export export):
    
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
      "SWEXLW03", context, import, export, EabOptions.Hpvp);
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
    /// A value of TotalAmount.
    /// </summary>
    [JsonPropertyName("totalAmount")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common TotalAmount
    {
      get => totalAmount ??= new();
      set => totalAmount = value;
    }

    /// <summary>
    /// A value of TotalCount.
    /// </summary>
    [JsonPropertyName("totalCount")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Count" })]
    public Common TotalCount
    {
      get => totalCount ??= new();
      set => totalCount = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "TextDate" })]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of RecType.
    /// </summary>
    [JsonPropertyName("recType")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea RecType
    {
      get => recType ??= new();
      set => recType = value;
    }

    /// <summary>
    /// A value of KdolFile.
    /// </summary>
    [JsonPropertyName("kdolFile")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName",
      "ClientNumber",
      "ExtractDate",
      "Amount",
      "MaxPercent"
    })]
    public KdolFile KdolFile
    {
      get => kdolFile ??= new();
      set => kdolFile = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 6, AccessFields = false, Members
      = new[] { "FileInstruction", "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private Common totalAmount;
    private Common totalCount;
    private DateWorkArea dateWorkArea;
    private TextWorkArea recType;
    private KdolFile kdolFile;
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
      "TextReturnCode",
      "TextLine80"
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
