// Program: CAB_GENERATE_3_DIGIT_RANDOM_NUM, ID: 371752885, model: 746.
// Short name: SWE00051
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_GENERATE_3_DIGIT_RANDOM_NUM.
/// </para>
/// <para>
/// RESP: FNCLMGMT
/// This action block will generate a 3 digit random number using the last three
/// digits of the time.
/// </para>
/// </summary>
[Serializable]
public partial class CabGenerate3DigitRandomNum: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_GENERATE_3_DIGIT_RANDOM_NUM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabGenerate3DigitRandomNum(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabGenerate3DigitRandomNum.
  /// </summary>
  public CabGenerate3DigitRandomNum(IContext context, Import import,
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
    export.SystemGenerated.Attribute3DigitRandomNumber = Microsecond(Now());
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
    }

    private SystemGenerated systemGenerated;
  }
#endregion
}
