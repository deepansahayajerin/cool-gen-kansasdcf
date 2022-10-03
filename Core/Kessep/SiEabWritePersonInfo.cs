// Program: SI_EAB_WRITE_PERSON_INFO, ID: 371790660, model: 746.
// Short name: SWEXIL03
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_EAB_WRITE_PERSON_INFO.
/// </summary>
[Serializable]
public partial class SiEabWritePersonInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EAB_WRITE_PERSON_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEabWritePersonInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEabWritePersonInfo.
  /// </summary>
  public SiEabWritePersonInfo(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXIL03", context, import, export, 0);
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
    /// A value of RestartFileRec.
    /// </summary>
    [JsonPropertyName("restartFileRec")]
    [Member(Index = 1, Members = new[] { "Count" })]
    public Common RestartFileRec
    {
      get => restartFileRec ??= new();
      set => restartFileRec = value;
    }

    /// <summary>
    /// A value of SiWageIncomeSourceRec.
    /// </summary>
    [JsonPropertyName("siWageIncomeSourceRec")]
    [Member(Index = 2, Members = new[]
    {
      "CseIndicator",
      "PersonNumber",
      "PersonSsn"
    })]
    public SiWageIncomeSourceRec SiWageIncomeSourceRec
    {
      get => siWageIncomeSourceRec ??= new();
      set => siWageIncomeSourceRec = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 3, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private Common restartFileRec;
    private SiWageIncomeSourceRec siWageIncomeSourceRec;
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
      "TextReturnCode"
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
