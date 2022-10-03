// Program: EAB_WRITE_HINS_AVAILABILITY_CHGS, ID: 372872148, model: 746.
// Short name: SWEXEE35
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_WRITE_HINS_AVAILABILITY_CHGS.
/// </summary>
[Serializable]
public partial class EabWriteHinsAvailabilityChgs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_HINS_AVAILABILITY_CHGS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWriteHinsAvailabilityChgs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWriteHinsAvailabilityChgs.
  /// </summary>
  public EabWriteHinsAvailabilityChgs(IContext context, Import import,
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
      "SWEXEE35", context, import, export, EabOptions.Hpvp);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Number",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "InsuranceId",
      "InsurancePolicyNumber",
      "InsuranceGroupNumber",
      "InsuranceName",
      "Street1",
      "Street2",
      "City",
      "State",
      "Zip5",
      "Zip4",
      "VerifiedDate",
      "EndDate",
      "EmployerName",
      "EmpStreet1",
      "EmpStreet2",
      "EmpCity",
      "EmpState",
      "EmpZip5",
      "EmpZip4",
      "EmpPhoneAreaCode",
      "EmpPhoneNo"
    })]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Date", "Time" })]
      
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Count" })]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private HealthInsuranceAvailability healthInsuranceAvailability;
    private DateWorkArea dateWorkArea;
    private Common record;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }
#endregion
}
