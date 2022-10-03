// Program: EAB_READ_FEDERAL_NEW_HIRE_FILE, ID: 371058248, model: 746.
// Short name: SWEXIC03
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_FEDERAL_NEW_HIRE_FILE.
/// </summary>
[Serializable]
public partial class EabReadFederalNewHireFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_FEDERAL_NEW_HIRE_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadFederalNewHireFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadFederalNewHireFile.
  /// </summary>
  public EabReadFederalNewHireFile(IContext context, Import import,
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
      "SWEXIC03", context, import, export, EabOptions.Hpvp);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Number",
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName",
      "Flag"
    })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Ein",
      "KansasId",
      "Name",
      "PhoneNo",
      "AreaCode"
    })]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "Note",
      "Street3",
      "Street4",
      "Province",
      "Country",
      "PostalCode",
      "County",
      "State",
      "ZipCode",
      "Zip4",
      "Zip3"
    })]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of DateOfHire.
    /// </summary>
    [JsonPropertyName("dateOfHire")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "TextDate" })]
    public DateWorkArea DateOfHire
    {
      get => dateOfHire ??= new();
      set => dateOfHire = value;
    }

    /// <summary>
    /// A value of StateHiredIn.
    /// </summary>
    [JsonPropertyName("stateHiredIn")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text2" })]
    public TextWorkArea StateHiredIn
    {
      get => stateHiredIn ??= new();
      set => stateHiredIn = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of MilitaryStatus.
    /// </summary>
    [JsonPropertyName("militaryStatus")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea MilitaryStatus
    {
      get => militaryStatus ??= new();
      set => militaryStatus = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Employer employer;
    private EmployerAddress employerAddress;
    private DateWorkArea dateOfHire;
    private TextWorkArea stateHiredIn;
    private TextWorkArea recordType;
    private TextWorkArea militaryStatus;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
