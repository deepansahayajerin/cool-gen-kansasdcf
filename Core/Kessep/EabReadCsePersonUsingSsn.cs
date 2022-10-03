// Program: EAB_READ_CSE_PERSON_USING_SSN, ID: 372405499, model: 746.
// Short name: SWEXGR20
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_CSE_PERSON_USING_SSN.
/// </summary>
[Serializable]
public partial class EabReadCsePersonUsingSsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_CSE_PERSON_USING_SSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadCsePersonUsingSsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadCsePersonUsingSsn.
  /// </summary>
  public EabReadCsePersonUsingSsn(IContext context, Import import, Export export)
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
    GetService<IEabStub>().Execute("SWEXGR20", context, import, export, 0);
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
    [Member(Index = 1, Members = new[] { "Ssn" })]
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
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName",
      "Sex",
      "Dob",
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
  }
#endregion
}
