// Program: EAB_PROCESS_LOCATE_RESPONSE, ID: 374436712, model: 746.
// Short name: SWEXEE39
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_PROCESS_LOCATE_RESPONSE.
/// </para>
/// <para>
/// open, read, and close the locate response incoming file.
/// </para>
/// </summary>
[Serializable]
public partial class EabProcessLocateResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_PROCESS_LOCATE_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabProcessLocateResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabProcessLocateResponse.
  /// </summary>
  public EabProcessLocateResponse(IContext context, Import import, Export export)
    :
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
      "SWEXEE39", context, import, export, EabOptions.Hpvp);
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "CsePersonNumber",
      "AgencyNumber",
      "SequenceNumber"
    })]
    public LocateRequest Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "FileInstruction", "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private LocateRequest restart;
    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReturnedExternal.
    /// </summary>
    [JsonPropertyName("returnedExternal")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "TextReturnCode", "TextLine80" })]
    public External ReturnedExternal
    {
      get => returnedExternal ??= new();
      set => returnedExternal = value;
    }

    /// <summary>
    /// A value of ReturnedTextWorkArea.
    /// </summary>
    [JsonPropertyName("returnedTextWorkArea")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea ReturnedTextWorkArea
    {
      get => returnedTextWorkArea ??= new();
      set => returnedTextWorkArea = value;
    }

    /// <summary>
    /// A value of ReturnedLocateRequest.
    /// </summary>
    [JsonPropertyName("returnedLocateRequest")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "SocialSecurityNumber",
      "DateOfBirth",
      "CsePersonNumber",
      "RequestDate",
      "ResponseDate",
      "LicenseIssuedDate",
      "LicenseExpirationDate",
      "LicenseSuspendedDate",
      "LicenseNumber",
      "AgencyNumber",
      "SequenceNumber",
      "LicenseSourceName",
      "Street1",
      "AddressType",
      "Street2",
      "Street3",
      "Street4",
      "City",
      "State",
      "ZipCode5",
      "ZipCode4",
      "ZipCode3",
      "Province",
      "PostalCode",
      "Country",
      "LicenseSuspensionInd"
    })]
    public LocateRequest ReturnedLocateRequest
    {
      get => returnedLocateRequest ??= new();
      set => returnedLocateRequest = value;
    }

    private External returnedExternal;
    private TextWorkArea returnedTextWorkArea;
    private LocateRequest returnedLocateRequest;
  }
#endregion
}
