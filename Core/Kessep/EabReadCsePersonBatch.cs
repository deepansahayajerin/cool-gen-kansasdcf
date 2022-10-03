// Program: EAB_READ_CSE_PERSON_BATCH, ID: 371790750, model: 746.
// Short name: SWEXGR15
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_READ_CSE_PERSON_BATCH.
/// </para>
/// <para>
/// RESP: SRVINIT		
///              Use for Batch Processing Only
/// This External Action Block(EAB) will provide a Stub for a process which will
/// retrieve details about a CSE PERSON from the ADABAS system.
/// </para>
/// </summary>
[Serializable]
public partial class EabReadCsePersonBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_CSE_PERSON_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadCsePersonBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadCsePersonBatch.
  /// </summary>
  public EabReadCsePersonBatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXGR15", context, import, export, 0);
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
    [Member(Index = 1, Members = new[] { "Number" })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
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
    [Member(Index = 1, Members = new[]
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
    [Member(Index = 2, Members = new[]
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
    [Member(Index = 3, Members = new[] { "Flag" })]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    [Member(Index = 4, Members = new[] { "Flag" })]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of Kanpay.
    /// </summary>
    [JsonPropertyName("kanpay")]
    [Member(Index = 5, Members = new[] { "Flag" })]
    public Common Kanpay
    {
      get => kanpay ??= new();
      set => kanpay = value;
    }

    /// <summary>
    /// A value of Kscares.
    /// </summary>
    [JsonPropertyName("kscares")]
    [Member(Index = 6, Members = new[] { "Flag" })]
    public Common Kscares
    {
      get => kscares ??= new();
      set => kscares = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private Common ae;
    private Common cse;
    private Common kanpay;
    private Common kscares;
  }
#endregion
}
