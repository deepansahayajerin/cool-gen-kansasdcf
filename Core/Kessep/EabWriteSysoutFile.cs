// Program: EAB_WRITE_SYSOUT_FILE, ID: 373027960, model: 746.
// Short name: SWEXFW05
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_WRITE_SYSOUT_FILE.
/// </para>
/// <para>
/// this action block writes a file with sysout information that may be used as 
/// a sysin in another job or another step of the same job that runs the program
/// calling this eab.
/// </para>
/// </summary>
[Serializable]
public partial class EabWriteSysoutFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_SYSOUT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWriteSysoutFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWriteSysoutFile.
  /// </summary>
  public EabWriteSysoutFile(IContext context, Import import, Export export):
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
      "SWEXFW05", context, import, export, EabOptions.Hpvp);
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
    /// A value of SysoutInfo.
    /// </summary>
    [JsonPropertyName("sysoutInfo")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "TextLine80" })]
    public External SysoutInfo
    {
      get => sysoutInfo ??= new();
      set => sysoutInfo = value;
    }

    private External sysoutInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "NumericReturnCode", "TextReturnCode" })]
    public External ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    private External returnCode;
  }
#endregion
}
