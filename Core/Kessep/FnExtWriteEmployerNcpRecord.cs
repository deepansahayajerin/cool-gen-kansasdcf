// Program: FN_EXT_WRITE_EMPLOYER_NCP_RECORD, ID: 374411566, model: 746.
// Short name: SWEXF203
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EXT_WRITE_EMPLOYER_NCP_RECORD.
/// </summary>
[Serializable]
public partial class FnExtWriteEmployerNcpRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EXT_WRITE_EMPLOYER_NCP_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnExtWriteEmployerNcpRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnExtWriteEmployerNcpRecord.
  /// </summary>
  public FnExtWriteEmployerNcpRecord(IContext context, Import import,
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
    GetService<IEabStub>().Execute(
      "SWEXF203", context, import, export, EabOptions.Hpvp);
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
    /// A value of EmployerNcpRecord.
    /// </summary>
    [JsonPropertyName("employerNcpRecord")]
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
      "Ein",
      "LastName",
      "FirstName",
      "MiddleInitial",
      "Suffix",
      "Ssn",
      "CountyId",
      "CourtOrderNumber"
    })]
    public EmployerNcpRecord EmployerNcpRecord
    {
      get => employerNcpRecord ??= new();
      set => employerNcpRecord = value;
    }

    private KpcExternalParms kpcExternalParms;
    private EmployerNcpRecord employerNcpRecord;
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
