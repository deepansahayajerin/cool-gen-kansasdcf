// Program: OE_EAB_RECEIVE_1099_RESPONSE, ID: 372374987, model: 746.
// Short name: SWEXEE03
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_RECEIVE_1099_RESPONSE.
/// </para>
/// <para>
/// Resp:OBMGMT
/// This EXTERNAL procedure carries view that will be used for receiving the 
/// 1099_RESPONSE from the IRS.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabReceive1099Response: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_RECEIVE_1099_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabReceive1099Response(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabReceive1099Response.
  /// </summary>
  public OeEabReceive1099Response(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXEE03", context, import, export, 0);
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
    /// A value of RestartAfterThisCount.
    /// </summary>
    [JsonPropertyName("restartAfterThisCount")]
    [Member(Index = 1, Members = new[] { "Count" })]
    public Common RestartAfterThisCount
    {
      get => restartAfterThisCount ??= new();
      set => restartAfterThisCount = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, Members = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private Common restartAfterThisCount;
    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExternalOcse1099Response.
    /// </summary>
    [JsonPropertyName("externalOcse1099Response")]
    [Member(Index = 1, Members = new[]
    {
      "Ssn",
      "SubmittingStateCode",
      "LocalFipsCode",
      "CsePersonNumber",
      "Attribute1099RequestIdentifier",
      "LastName",
      "FirstName",
      "OcseMatchCode",
      "CaseTypeAdcNadc",
      "CourtAdminOrderIndicator",
      "PayeeName1",
      "PayeeName2",
      "PayeeStreet",
      "PayeeCity",
      "PayeeState",
      "PayeeZipCode",
      "PayorEin",
      "PayorName1",
      "PayorName2",
      "PayorStreet",
      "PayorCityStateZip",
      "TaxYear",
      "AccountCode",
      "DocumentCode",
      "AmountInd1",
      "Amount1",
      "AmountInd2",
      "Amount2",
      "AmountInd3",
      "Amount3",
      "AmountInd4",
      "Amount4",
      "AmountInd5",
      "Amount5",
      "AmountInd6",
      "Amount6",
      "AmountInd7",
      "Amount7",
      "AmountInd8",
      "Amount8",
      "AmountInd9",
      "Amount9",
      "AmountInd10",
      "Amount10",
      "AmountInd11",
      "Amount11",
      "AmountInd12",
      "Amount12"
    })]
    public ExternalOcse1099Response ExternalOcse1099Response
    {
      get => externalOcse1099Response ??= new();
      set => externalOcse1099Response = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, Members = new[]
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

    private ExternalOcse1099Response externalOcse1099Response;
    private External external;
  }
#endregion
}
