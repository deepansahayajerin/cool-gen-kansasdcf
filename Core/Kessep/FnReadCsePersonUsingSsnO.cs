// Program: FN_READ_CSE_PERSON_USING_SSN_O, ID: 371770030, model: 746.
// Short name: SWEXGR21
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_READ_CSE_PERSON_USING_SSN_O.
/// </summary>
[Serializable]
public partial class FnReadCsePersonUsingSsnO: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_CSE_PERSON_USING_SSN_O program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCsePersonUsingSsnO(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCsePersonUsingSsnO.
  /// </summary>
  public FnReadCsePersonUsingSsnO(IContext context, Import import, Export export)
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
    GetService<IEabStub>().Execute(
      "SWEXGR21", context, import, export, EabOptions.NoIefParams);
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    [Member(Index = 1, Members = new[]
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 2, Members = new[]
    {
      "Number",
      "Sex",
      "Dob",
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "FormattedName",
      "LastName"
    })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
  }
#endregion
}
