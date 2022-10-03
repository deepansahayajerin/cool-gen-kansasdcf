// Program: EAB_ADD_ALIAS, ID: 371755422, model: 746.
// Short name: SWEXIC20
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_ADD_ALIAS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class EabAddAlias: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_ADD_ALIAS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabAddAlias(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabAddAlias.
  /// </summary>
  public EabAddAlias(IContext context, Import import, Export export):
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
      "SWEXIC20", context, import, export, EabOptions.NoIefParams);
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
      "Number",
      "Sex",
      "Dob"
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
    /// A value of Key.
    /// </summary>
    [JsonPropertyName("key")]
    [Member(Index = 1, Members = new[] { "Number", "UniqueKey" })]
    public CsePersonsWorkSet Key
    {
      get => key ??= new();
      set => key = value;
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

    private CsePersonsWorkSet key;
    private AbendData abendData;
  }
#endregion
}
