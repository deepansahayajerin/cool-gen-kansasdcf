// Program: FN_EXT_WRITE_EMPLOYER_FILE, ID: 374400825, model: 746.
// Short name: SWEXF202
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EXT_WRITE_EMPLOYER_FILE.
/// </summary>
[Serializable]
public partial class FnExtWriteEmployerFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EXT_WRITE_EMPLOYER_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnExtWriteEmployerFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnExtWriteEmployerFile.
  /// </summary>
  public FnExtWriteEmployerFile(IContext context, Import import, Export export):
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
      "SWEXF202", context, import, export, EabOptions.Hpvp);
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
    /// A value of KpcExternalParms.
    /// </summary>
    [JsonPropertyName("kpcExternalParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public KpcExternalParms KpcExternalParms
    {
      get => kpcExternalParms ??= new();
      set => kpcExternalParms = value;
    }

    /// <summary>
    /// A value of EmployerRecord.
    /// </summary>
    [JsonPropertyName("employerRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Name",
      "Street1",
      "Street2",
      "Street3",
      "City",
      "State",
      "ZipCode",
      "PhoneNumber",
      "Extension",
      "Ein"
    })]
    public EmployerRecord EmployerRecord
    {
      get => employerRecord ??= new();
      set => employerRecord = value;
    }

    private KpcExternalParms kpcExternalParms;
    private EmployerRecord employerRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of KpcExternalParms.
    /// </summary>
    [JsonPropertyName("kpcExternalParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public KpcExternalParms KpcExternalParms
    {
      get => kpcExternalParms ??= new();
      set => kpcExternalParms = value;
    }

    private KpcExternalParms kpcExternalParms;
  }
#endregion
}
