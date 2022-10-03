// Program: SI_EAB_RECEIVE_WAGE_INC_SOURCE, ID: 371790522, model: 746.
// Short name: SWEXIL02
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_EAB_RECEIVE_WAGE_INC_SOURCE.
/// </summary>
[Serializable]
public partial class SiEabReceiveWageIncSource: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EAB_RECEIVE_WAGE_INC_SOURCE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEabReceiveWageIncSource(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEabReceiveWageIncSource.
  /// </summary>
  public SiEabReceiveWageIncSource(IContext context, Import import,
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
    GetService<IEabStub>().Execute("SWEXIL02", context, import, export, 0);
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
    /// A value of RestartOutputErrRec.
    /// </summary>
    [JsonPropertyName("restartOutputErrRec")]
    [Member(Index = 1, Members = new[] { "Count" })]
    public Common RestartOutputErrRec
    {
      get => restartOutputErrRec ??= new();
      set => restartOutputErrRec = value;
    }

    /// <summary>
    /// A value of RestartInputFileRec.
    /// </summary>
    [JsonPropertyName("restartInputFileRec")]
    [Member(Index = 2, Members = new[] { "Count" })]
    public Common RestartInputFileRec
    {
      get => restartInputFileRec ??= new();
      set => restartInputFileRec = value;
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

    private Common restartOutputErrRec;
    private Common restartInputFileRec;
    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SiWageIncomeSourceRec.
    /// </summary>
    [JsonPropertyName("siWageIncomeSourceRec")]
    [Member(Index = 1, Members = new[]
    {
      "CseIndicator",
      "PersonNumber",
      "PersonSsn",
      "RecordTypeIndicator",
      "BwQtr",
      "BwYr",
      "NhUiDate",
      "WageOrUiAmt",
      "EmpId",
      "EmpName",
      "Street1",
      "City",
      "State",
      "ZipCode",
      "Zip4",
      "UiBeginningBalance",
      "UiStartDate",
      "UiEndDate"
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
    [Member(Index = 2, Members = new[]
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

    private SiWageIncomeSourceRec siWageIncomeSourceRec;
    private External external;
  }
#endregion
}
