// Program: EAB_XCTL_TO_KAECSES_W_USERID, ID: 371451676, model: 746.
// Short name: SWEXGC00
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_XCTL_TO_KAECSES_W_USERID.
/// </para>
/// <para>
/// This external executes an XTCL for program SWASS000 - the KAECSES System 
/// Selection menu from which KESSEP is entered.  A view will be set to pass the
/// current userid, but the first itteration of this program will not utilize
/// it in hopes that the kaelin area in KAECSES will maintain infomation
/// executed prior to entering KESSEP.
/// If this message from here forth still exists, then the userid is NOT being 
/// passed to SWASS000.
/// Regan Welborn MTW Consulting Senior
/// 3/5/97
/// </para>
/// </summary>
[Serializable]
public partial class EabXctlToKaecsesWUserid: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_XCTL_TO_KAECSES_W_USERID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabXctlToKaecsesWUserid(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabXctlToKaecsesWUserid.
  /// </summary>
  public EabXctlToKaecsesWUserid(IContext context, Import import, Export export):
    
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
      "SWEXGC00", context, import, export, EabOptions.NoIefParams);
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
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    [Member(Index = 1, Members = new[] { "Userid" })]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
    }

    private Security2 security;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
