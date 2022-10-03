// Program: OE_EAB_WRITE_AP_DEBT_DERIVATION, ID: 945103319, model: 746.
// Short name: SWEXEW15
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_EAB_WRITE_AP_DEBT_DERIVATION.
/// </summary>
[Serializable]
public partial class OeEabWriteApDebtDerivation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_WRITE_AP_DEBT_DERIVATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabWriteApDebtDerivation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabWriteApDebtDerivation.
  /// </summary>
  public OeEabWriteApDebtDerivation(IContext context, Import import,
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
      "SWEXEW15", context, import, export, EabOptions.Hpvp);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Number",
      "FirstName",
      "LastName"
    })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "StandardNumber" })]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Cases.
    /// </summary>
    [JsonPropertyName("cases")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text60" })]
    public WorkArea Cases
    {
      get => cases ??= new();
      set => cases = value;
    }

    /// <summary>
    /// A value of SummedAmount.
    /// </summary>
    [JsonPropertyName("summedAmount")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text11" })]
    public WorkArea SummedAmount
    {
      get => summedAmount ??= new();
      set => summedAmount = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode",
      "Zip4"
    })]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 6, Members = new[] { "FileInstruction", "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private WorkArea cases;
    private WorkArea summedAmount;
    private CsePersonAddress csePersonAddress;
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
    [Member(Index = 1, Members = new[]
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
