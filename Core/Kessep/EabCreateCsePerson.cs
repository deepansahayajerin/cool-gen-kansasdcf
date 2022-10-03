// Program: EAB_CREATE_CSE_PERSON, ID: 371728384, model: 746.
// Short name: SWEXIC10
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_CREATE_CSE_PERSON.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block sends the basic CSE PERSON details to ADABAS for creation 
/// of a new CSE PERSON.  ADABAS returns a CSE PERSON number to enable creation
/// on the CSE system
/// </para>
/// </summary>
[Serializable]
public partial class EabCreateCsePerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CREATE_CSE_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCreateCsePerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCreateCsePerson.
  /// </summary>
  public EabCreateCsePerson(IContext context, Import import, Export export):
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
      "SWEXIC10", context, import, export, EabOptions.NoIefParams);
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    [Member(Index = 1, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial",
      "Ssn",
      "Dob",
      "Sex"
    })]
    public CsePersonsWorkSet New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private CsePersonsWorkSet new1;
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
    [Member(Index = 1, Members = new[] { "Number" })]
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
  }
#endregion
}
