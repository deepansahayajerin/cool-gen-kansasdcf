// Program: EAB_READ_FCR_SVES_RESPONSE_FILE, ID: 374518464, model: 746.
// Short name: SWEXIC07
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_READ_FCR_SVES_RESPONSE_FILE.
/// </para>
/// <para>
/// This External Action Block is created to read the SVES response file from 
/// FCR.   This file will contain the below mentioend record SVES Record Types:
/// 1.  Title II Pending Claim - E04
/// 2.  Title II Pending          - E05
/// 3.  Title XVI                     - E06
/// 4.  Prisoner                     - E07
/// 5.  Not Found                 - E10
/// </para>
/// </summary>
[Serializable]
public partial class EabReadFcrSvesResponseFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_FCR_SVES_RESPONSE_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadFcrSvesResponseFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadFcrSvesResponseFile.
  /// </summary>
  public EabReadFcrSvesResponseFile(IContext context, Import import,
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
      "SWEXIC07", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SvesTitleIiPending.
    /// </summary>
    [JsonPropertyName("svesTitleIiPending")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "RecordIdentifier",
      "MatchTypeCode",
      "TransmitterStateTerritoryCode",
      "LocSrcResponseAgencyCode",
      "NameMatchedCode",
      "FirstName",
      "MiddleName",
      "LastName",
      "AdditionalFirstName1",
      "AdditionalMiddleName1",
      "AdditionalLastName1",
      "AdditionalFirstName2",
      "AdditionalMiddleName2",
      "AdditionalLastName2",
      "ReturnedFirstName",
      "ReturnedMiddleName",
      "ReturnedLastName",
      "Ssn",
      "MemberIdentifier",
      "FipsCountyCode",
      "ResponseDate",
      "LocateResponseCode",
      "CorrAdditlMultipleSsn",
      "SsnMatchCode",
      "ClaimTypeCode",
      "ClaimantAddress",
      "DoMailingAddressLine1",
      "DoMailingAddressLine2",
      "DoMailingAddressLine3",
      "DoMailingAddressLine4",
      "DoMailingAddressCity",
      "DoMailingAddressState",
      "DoMainlingAddressZip",
      "ParticipantTypeCode",
      "StateSortCode"
    })]
    public FcrSvesTitleIiPendingClaim SvesTitleIiPending
    {
      get => svesTitleIiPending ??= new();
      set => svesTitleIiPending = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private FcrSvesTitleIiPendingClaim svesTitleIiPending;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
