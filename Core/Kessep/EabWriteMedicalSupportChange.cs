// Program: EAB_WRITE_MEDICAL_SUPPORT_CHANGE, ID: 372971207, model: 746.
// Short name: SWEXEE34
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_WRITE_MEDICAL_SUPPORT_CHANGE.
/// </summary>
[Serializable]
public partial class EabWriteMedicalSupportChange: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_MEDICAL_SUPPORT_CHANGE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWriteMedicalSupportChange(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWriteMedicalSupportChange.
  /// </summary>
  public EabWriteMedicalSupportChange(IContext context, Import import,
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
      "SWEXEE34", context, import, export, EabOptions.Hpvp);
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
    /// <summary>A EmployerInfoGroup group.</summary>
    [Serializable]
    public class EmployerInfoGroup
    {
      /// <summary>
      /// A value of EmployerEmployer.
      /// </summary>
      [JsonPropertyName("employerEmployer")]
      [Member(Index = 1, AccessFields = false, Members = new[]
      {
        "Name",
        "PhoneNo",
        "AreaCode"
      })]
      public Employer EmployerEmployer
      {
        get => employerEmployer ??= new();
        set => employerEmployer = value;
      }

      /// <summary>
      /// A value of EmployerEmployerAddress.
      /// </summary>
      [JsonPropertyName("employerEmployerAddress")]
      [Member(Index = 2, AccessFields = false, Members = new[]
      {
        "LocationType",
        "Street1",
        "Street2",
        "City",
        "Street3",
        "Street4",
        "Province",
        "Country",
        "PostalCode",
        "State",
        "ZipCode",
        "Zip4",
        "Zip3"
      })]
      public EmployerAddress EmployerEmployerAddress
      {
        get => employerEmployerAddress ??= new();
        set => employerEmployerAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Employer employerEmployer;
      private EmployerAddress employerEmployerAddress;
    }

    /// <summary>A ChildrenInformationGroup group.</summary>
    [Serializable]
    public class ChildrenInformationGroup
    {
      /// <summary>
      /// A value of Child.
      /// </summary>
      [JsonPropertyName("child")]
      [Member(Index = 1, AccessFields = false, Members = new[]
      {
        "Dob",
        "Ssn",
        "FirstName",
        "MiddleInitial",
        "LastName"
      })]
      public CsePersonsWorkSet Child
      {
        get => child ??= new();
        set => child = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private CsePersonsWorkSet child;
    }

    /// <summary>A InsurancePolicyGroup group.</summary>
    [Serializable]
    public class InsurancePolicyGroup
    {
      /// <summary>
      /// A value of InsHealthInsuranceCompany.
      /// </summary>
      [JsonPropertyName("insHealthInsuranceCompany")]
      [Member(Index = 1, AccessFields = false, Members
        = new[] { "CarrierCode", "InsurancePolicyCarrier" })]
      public HealthInsuranceCompany InsHealthInsuranceCompany
      {
        get => insHealthInsuranceCompany ??= new();
        set => insHealthInsuranceCompany = value;
      }

      /// <summary>
      /// A value of InsHealthInsuranceCoverage.
      /// </summary>
      [JsonPropertyName("insHealthInsuranceCoverage")]
      [Member(Index = 2, AccessFields = false, Members = new[]
      {
        "InsuranceGroupNumber",
        "InsurancePolicyNumber",
        "PolicyExpirationDate",
        "CoverageCode1",
        "CoverageCode2",
        "CoverageCode3",
        "CoverageCode4",
        "CoverageCode5",
        "CoverageCode6",
        "CoverageCode7",
        "PolicyEffectiveDate"
      })]
      public HealthInsuranceCoverage InsHealthInsuranceCoverage
      {
        get => insHealthInsuranceCoverage ??= new();
        set => insHealthInsuranceCoverage = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private HealthInsuranceCompany insHealthInsuranceCompany;
      private HealthInsuranceCoverage insHealthInsuranceCoverage;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of OldNewXref.
    /// </summary>
    [JsonPropertyName("oldNewXref")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "CaecsesCaseNbr" })]
    public OldNewXref OldNewXref
    {
      get => oldNewXref ??= new();
      set => oldNewXref = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "Number",
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode",
      "Zip4",
      "Zip3",
      "Street3",
      "Street4",
      "Province",
      "PostalCode",
      "Country"
    })]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "Number",
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ArCsePersonAddress.
    /// </summary>
    [JsonPropertyName("arCsePersonAddress")]
    [Member(Index = 6, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode",
      "Zip4",
      "Zip3",
      "Street3",
      "Street4",
      "Province",
      "PostalCode",
      "Country"
    })]
    public CsePersonAddress ArCsePersonAddress
    {
      get => arCsePersonAddress ??= new();
      set => arCsePersonAddress = value;
    }

    /// <summary>
    /// Gets a value of EmployerInfo.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 7)]
    public Array<EmployerInfoGroup> EmployerInfo => employerInfo ??= new(
      EmployerInfoGroup.Capacity);

    /// <summary>
    /// Gets a value of EmployerInfo for json serialization.
    /// </summary>
    [JsonPropertyName("employerInfo")]
    [Computed]
    public IList<EmployerInfoGroup> EmployerInfo_Json
    {
      get => employerInfo;
      set => EmployerInfo.Assign(value);
    }

    /// <summary>
    /// Gets a value of ChildrenInformation.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 8)]
    public Array<ChildrenInformationGroup> ChildrenInformation =>
      childrenInformation ??= new(ChildrenInformationGroup.Capacity);

    /// <summary>
    /// Gets a value of ChildrenInformation for json serialization.
    /// </summary>
    [JsonPropertyName("childrenInformation")]
    [Computed]
    public IList<ChildrenInformationGroup> ChildrenInformation_Json
    {
      get => childrenInformation;
      set => ChildrenInformation.Assign(value);
    }

    /// <summary>
    /// Gets a value of InsurancePolicy.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 9)]
    public Array<InsurancePolicyGroup> InsurancePolicy =>
      insurancePolicy ??= new(InsurancePolicyGroup.Capacity);

    /// <summary>
    /// Gets a value of InsurancePolicy for json serialization.
    /// </summary>
    [JsonPropertyName("insurancePolicy")]
    [Computed]
    public IList<InsurancePolicyGroup> InsurancePolicy_Json
    {
      get => insurancePolicy;
      set => InsurancePolicy.Assign(value);
    }

    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Flag" })]
    public Common NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of DisbursementsMa.
    /// </summary>
    [JsonPropertyName("disbursementsMa")]
    [Member(Index = 11, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common DisbursementsMa
    {
      get => disbursementsMa ??= new();
      set => disbursementsMa = value;
    }

    /// <summary>
    /// A value of LastDisbursement.
    /// </summary>
    [JsonPropertyName("lastDisbursement")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea LastDisbursement
    {
      get => lastDisbursement ??= new();
      set => lastDisbursement = value;
    }

    /// <summary>
    /// A value of Trailer.
    /// </summary>
    [JsonPropertyName("trailer")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Date", "Time" })
      ]
    public DateWorkArea Trailer
    {
      get => trailer ??= new();
      set => trailer = value;
    }

    /// <summary>
    /// A value of TrailerRecord.
    /// </summary>
    [JsonPropertyName("trailerRecord")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Count" })]
    public Common TrailerRecord
    {
      get => trailerRecord ??= new();
      set => trailerRecord = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Case1 case1;
    private OldNewXref oldNewXref;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonAddress apCsePersonAddress;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonAddress arCsePersonAddress;
    private Array<EmployerInfoGroup> employerInfo;
    private Array<ChildrenInformationGroup> childrenInformation;
    private Array<InsurancePolicyGroup> insurancePolicy;
    private Common nonCooperation;
    private Common disbursementsMa;
    private DateWorkArea lastDisbursement;
    private DateWorkArea trailer;
    private Common trailerRecord;
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
