// Program: OE_EAB_READ_OUT_FCR_ALERT_RECORD, ID: 371416712, model: 746.
// Short name: SWEXER13
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_EAB_READ_OUT_FCR_ALERT_RECORD.
/// </summary>
[Serializable]
public partial class OeEabReadOutFcrAlertRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_READ_OUT_FCR_ALERT_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabReadOutFcrAlertRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabReadOutFcrAlertRecord.
  /// </summary>
  public OeEabReadOutFcrAlertRecord(IContext context, Import import,
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
      "SWEXER13", context, import, export, EabOptions.Hpvp);
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
    /// A value of ExternalFileStatus.
    /// </summary>
    [JsonPropertyName("externalFileStatus")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine130",
      "TextLine80",
      "TextLine8"
    })]
    public External ExternalFileStatus
    {
      get => externalFileStatus ??= new();
      set => externalFileStatus = value;
    }

    private External externalFileStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExternalFileStatus.
    /// </summary>
    [JsonPropertyName("externalFileStatus")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine130",
      "TextLine80",
      "TextLine8"
    })]
    public External ExternalFileStatus
    {
      get => externalFileStatus ??= new();
      set => externalFileStatus = value;
    }

    /// <summary>
    /// A value of FcrAlert.
    /// </summary>
    [JsonPropertyName("fcrAlert")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "CaseId",
      "PersonId",
      "FcrSsn",
      "CseSsn",
      "CseAdditionalSsn1",
      "CseAdditionalSsn2",
      "FcrAdditionalSsn1",
      "FcrAdditionalSsn2"
    })]
    public FcrAlertRecord FcrAlert
    {
      get => fcrAlert ??= new();
      set => fcrAlert = value;
    }

    private External externalFileStatus;
    private FcrAlertRecord fcrAlert;
  }
#endregion
}
