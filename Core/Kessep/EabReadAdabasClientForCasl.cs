// Program: EAB_READ_ADABAS_CLIENT_FOR_CASL, ID: 371032422, model: 746.
// Short name: SWEXGR30
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_ADABAS_CLIENT_FOR_CASL.
/// </summary>
[Serializable]
public partial class EabReadAdabasClientForCasl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_ADABAS_CLIENT_FOR_CASL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadAdabasClientForCasl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadAdabasClientForCasl.
  /// </summary>
  public EabReadAdabasClientForCasl(IContext context, Import import,
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
      "SWEXGR30", context, import, export, EabOptions.NoIefParams);
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
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Sex",
      "Dob",
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName",
      "Number"
    })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Type1",
      "AdabasFileNumber",
      "AdabasFileAction",
      "AdabasResponseCd",
      "CicsResourceNm",
      "CicsFunctionCd",
      "CicsResponseCd"
    })]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Flag" })]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Flag" })]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of Kanpay.
    /// </summary>
    [JsonPropertyName("kanpay")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Flag" })]
    public Common Kanpay
    {
      get => kanpay ??= new();
      set => kanpay = value;
    }

    /// <summary>
    /// A value of Kscares.
    /// </summary>
    [JsonPropertyName("kscares")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Flag" })]
    public Common Kscares
    {
      get => kscares ??= new();
      set => kscares = value;
    }

    /// <summary>
    /// A value of Facts.
    /// </summary>
    [JsonPropertyName("facts")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Flag" })]
    public Common Facts
    {
      get => facts ??= new();
      set => facts = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private Common ae;
    private Common cse;
    private Common kanpay;
    private Common kscares;
    private Common facts;
  }
#endregion
}
