// Program: SI_EAB_FORMAT_CSENET_OUT_TRANS, ID: 372617472, model: 746.
// Short name: SWEXIE01
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_EAB_FORMAT_CSENET_OUT_TRANS.
/// </summary>
[Serializable]
public partial class SiEabFormatCsenetOutTrans: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EAB_FORMAT_CSENET_OUT_TRANS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEabFormatCsenetOutTrans(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEabFormatCsenetOutTrans.
  /// </summary>
  public SiEabFormatCsenetOutTrans(IContext context, Import import,
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
    GetService<IEabStub>().Execute("SWEXIE01", context, import, export, 0);
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
    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of InterstateParticipant.
      /// </summary>
      [JsonPropertyName("interstateParticipant")]
      [Member(Index = 1, AccessFields = false, Members = new[]
      {
        "NameLast",
        "NameFirst",
        "NameMiddle",
        "NameSuffix",
        "DateOfBirth",
        "Ssn",
        "Sex",
        "Race",
        "Relationship",
        "Status",
        "DependentRelationCp",
        "AddressLine1",
        "AddressLine2",
        "City",
        "State",
        "ZipCode5",
        "ZipCode4",
        "EmployerName",
        "EmployerAddressLine1",
        "EmployerAddressLine2",
        "EmployerCity",
        "EmployerState",
        "EmployerZipCode5",
        "EmployerZipCode4",
        "EmployerEin",
        "AddressVerifiedDate",
        "EmployerVerifiedDate",
        "WorkAreaCode",
        "WorkPhone",
        "PlaceOfBirth",
        "ChildStateOfResidence",
        "ChildPaternityStatus",
        "EmployerConfirmedInd",
        "AddressConfirmedInd"
      })]
      public InterstateParticipant InterstateParticipant
      {
        get => interstateParticipant ??= new();
        set => interstateParticipant = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateParticipant interstateParticipant;
    }

    /// <summary>A OrderGroup group.</summary>
    [Serializable]
    public class OrderGroup
    {
      /// <summary>
      /// A value of InterstateSupportOrder.
      /// </summary>
      [JsonPropertyName("interstateSupportOrder")]
      [Member(Index = 1, AccessFields = false, Members = new[]
      {
        "FipsState",
        "FipsCounty",
        "FipsLocation",
        "Number",
        "OrderFilingDate",
        "Type1",
        "DebtType",
        "PaymentFreq",
        "AmountOrdered",
        "EffectiveDate",
        "EndDate",
        "CancelDate",
        "ArrearsFreq",
        "ArrearsFreqAmount",
        "ArrearsTotalAmount",
        "ArrearsAfdcFromDate",
        "ArrearsAfdcThruDate",
        "ArrearsAfdcAmount",
        "ArrearsNonAfdcFromDate",
        "ArrearsNonAfdcThruDate",
        "ArrearsNonAfdcAmount",
        "FosterCareFromDate",
        "FosterCareThruDate",
        "FosterCareAmount",
        "MedicalFromDate",
        "MedicalThruDate",
        "MedicalAmount",
        "MedicalOrderedInd",
        "TribunalCaseNumber",
        "DateOfLastPayment",
        "ControllingOrderFlag",
        "NewOrderFlag",
        "DocketNumber"
      })]
      public InterstateSupportOrder InterstateSupportOrder
      {
        get => interstateSupportOrder ??= new();
        set => interstateSupportOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder interstateSupportOrder;
    }

    /// <summary>A CollectionGroup group.</summary>
    [Serializable]
    public class CollectionGroup
    {
      /// <summary>
      /// A value of InterstateCollection.
      /// </summary>
      [JsonPropertyName("interstateCollection")]
      [Member(Index = 1, AccessFields = false, Members = new[]
      {
        "DateOfCollection",
        "DateOfPosting",
        "PaymentAmount",
        "PaymentSource",
        "InterstatePaymentMethod",
        "RdfiId",
        "RdfiAccountNum"
      })]
      public InterstateCollection InterstateCollection
      {
        get => interstateCollection ??= new();
        set => interstateCollection = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateCollection interstateCollection;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "LocalFipsState",
      "LocalFipsCounty",
      "LocalFipsLocation",
      "OtherFipsState",
      "OtherFipsCounty",
      "OtherFipsLocation",
      "TransSerialNumber",
      "ActionCode",
      "FunctionalTypeCode",
      "TransactionDate",
      "KsCaseId",
      "InterstateCaseId",
      "ActionReasonCode",
      "ActionResolutionDate",
      "AttachmentsInd",
      "CaseDataInd",
      "ApIdentificationInd",
      "ApLocateDataInd",
      "ParticipantDataInd",
      "OrderDataInd",
      "CollectionDataInd",
      "InformationInd",
      "SentDate",
      "SentTime",
      "DueDate",
      "OverdueInd",
      "DateReceived",
      "TimeReceived",
      "AttachmentsDueDate",
      "InterstateFormsPrinted",
      "CaseType",
      "CaseStatus",
      "PaymentMailingAddressLine1",
      "PaymentAddressLine2",
      "PaymentCity",
      "PaymentState",
      "PaymentZipCode5",
      "PaymentZipCode4",
      "ContactNameLast",
      "ContactNameFirst",
      "ContactNameMiddle",
      "ContactNameSuffix",
      "ContactAddressLine1",
      "ContactAddressLine2",
      "ContactCity",
      "ContactState",
      "ContactZipCode5",
      "ContactZipCode4",
      "ContactAreaCode",
      "ContactPhoneNum",
      "ContactPhoneExtension",
      "RespondingDocketNumber",
      "ContactFaxAreaCode",
      "ContactFaxNumber",
      "ContactInternetAddress",
      "InitiatingDocketNumber",
      "SendPaymentsBankAccount",
      "SendPaymentsRoutingCode",
      "StateWithCej",
      "PaymentFipsState",
      "PaymentFipsCounty",
      "PaymentFipsLocation",
      "NondisclosureFinding"
    })]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "NameLast",
      "NameFirst",
      "MiddleName",
      "NameSuffix",
      "Ssn",
      "DateOfBirth",
      "Race",
      "Sex",
      "PlaceOfBirth",
      "HeightFt",
      "HeightIn",
      "Weight",
      "HairColor",
      "EyeColor",
      "OtherIdInfo",
      "AliasSsn1",
      "AliasSsn2",
      "PossiblyDangerous",
      "MaidenName",
      "MothersMaidenOrFathersName"
    })]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "ResidentialAddressLine1",
      "ResidentialAddressLine2",
      "ResidentialCity",
      "ResidentialState",
      "ResidentialZipCode5",
      "ResidentialZipCode4",
      "MailingAddressLine1",
      "MailingAddressLine2",
      "MailingCity",
      "MailingState",
      "MailingZipCode5",
      "MailingZipCode4",
      "ResidentialAddressEffectvDate",
      "ResidentialAddressEndDate",
      "ResidentialAddressConfirmInd",
      "MailingAddressEffectiveDate",
      "MailingAddressEndDate",
      "MailingAddressConfirmedInd",
      "HomeAreaCode",
      "HomePhoneNumber",
      "WorkAreaCode",
      "WorkPhoneNumber",
      "DriversLicState",
      "DriversLicenseNum",
      "Alias1FirstName",
      "Alias1MiddleName",
      "Alias1LastName",
      "Alias1Suffix",
      "Alias2FirstName",
      "Alias2MiddleName",
      "Alias2LastName",
      "Alias2Suffix",
      "Alias3FirstName",
      "Alias3MiddleName",
      "Alias3LastName",
      "Alias3Suffix",
      "CurrentSpouseLastName",
      "CurrentSpouseFirstName",
      "CurrentSpouseMiddleName",
      "CurrentSpouseSuffix",
      "Occupation",
      "EmployerEin",
      "EmployerName",
      "EmployerAddressLine1",
      "EmployerAddressLine2",
      "EmployerCity",
      "EmployerState",
      "EmployerZipCode5",
      "EmployerZipCode4",
      "EmployerAreaCode",
      "EmployerPhoneNum",
      "EmployerEffectiveDate",
      "EmployerEndDate",
      "EmployerConfirmedInd",
      "WageQtr",
      "WageYear",
      "WageAmount",
      "InsuranceCarrierName",
      "InsurancePolicyNum",
      "LastResAddressLine1",
      "LastResAddressLine2",
      "LastResCity",
      "LastResState",
      "LastResZipCode5",
      "LastResZipCode4",
      "LastResAddressDate",
      "LastMailAddressLine1",
      "LastMailAddressLine2",
      "LastMailCity",
      "LastMailState",
      "LastMailZipCode5",
      "LastMailZipCode4",
      "LastMailAddressDate",
      "LastEmployerName",
      "LastEmployerDate",
      "LastEmployerAddressLine1",
      "LastEmployerAddressLine2",
      "LastEmployerCity",
      "LastEmployerState",
      "LastEmployerZipCode5",
      "LastEmployerZipCode4",
      "LastEmployerEndDate",
      "Employer2Ein",
      "Employer2Name",
      "Employer2AddressLine1",
      "Employer2AddressLine2",
      "Employer2City",
      "Employer2State",
      "Employer2ZipCode5",
      "Employer2ZipCode4",
      "Employer2AreaCode",
      "Employer2PhoneNumber",
      "Employer2EffectiveDate",
      "Employer2EndDate",
      "Employer2ConfirmedIndicator",
      "Employer2WageQuarter",
      "Employer2WageYear",
      "Employer2WageAmount",
      "Employer3Ein",
      "Employer3Name",
      "Employer3AddressLine1",
      "Employer3AddressLine2",
      "Employer3City",
      "Employer3State",
      "Employer3ZipCode5",
      "Employer3ZipCode4",
      "Employer3AreaCode",
      "Employer3PhoneNumber",
      "Employer3EffectiveDate",
      "Employer3EndDate",
      "Employer3ConfirmedIndicator",
      "Employer3WageQuarter",
      "Employer3WageYear",
      "Employer3WageAmount",
      "ProfessionalLicenses"
    })]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// Gets a value of Participant.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 4)]
    public Array<ParticipantGroup> Participant => participant ??= new(
      ParticipantGroup.Capacity);

    /// <summary>
    /// Gets a value of Participant for json serialization.
    /// </summary>
    [JsonPropertyName("participant")]
    [Computed]
    public IList<ParticipantGroup> Participant_Json
    {
      get => participant;
      set => Participant.Assign(value);
    }

    /// <summary>
    /// Gets a value of Order.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 5)]
    public Array<OrderGroup> Order => order ??= new(OrderGroup.Capacity);

    /// <summary>
    /// Gets a value of Order for json serialization.
    /// </summary>
    [JsonPropertyName("order")]
    [Computed]
    public IList<OrderGroup> Order_Json
    {
      get => order;
      set => Order.Assign(value);
    }

    /// <summary>
    /// Gets a value of Collection.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 6)]
    public Array<CollectionGroup> Collection => collection ??= new(
      CollectionGroup.Capacity);

    /// <summary>
    /// Gets a value of Collection for json serialization.
    /// </summary>
    [JsonPropertyName("collection")]
    [Computed]
    public IList<CollectionGroup> Collection_Json
    {
      get => collection;
      set => Collection.Assign(value);
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    [Member(Index = 7, AccessFields = false, Members = new[]
    {
      "StatusChangeCode",
      "NewCaseId",
      "InformationTextLine1",
      "InformationTextLine2",
      "InformationTextLine3",
      "InformationTextLine4",
      "InformationTextLine5"
    })]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 8, AccessFields = false, Members
      = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private Array<ParticipantGroup> participant;
    private Array<OrderGroup> order;
    private Array<CollectionGroup> collection;
    private InterstateMiscellaneous interstateMiscellaneous;
    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of InterstateParticipant.
      /// </summary>
      [JsonPropertyName("interstateParticipant")]
      [Member(Index = 1, AccessFields = false, Members = new[]
      {
        "NameLast",
        "NameFirst",
        "NameMiddle",
        "NameSuffix",
        "DateOfBirth",
        "Ssn",
        "Sex",
        "Race",
        "Relationship",
        "Status",
        "DependentRelationCp",
        "AddressLine1",
        "AddressLine2",
        "City",
        "State",
        "ZipCode5",
        "ZipCode4",
        "EmployerName",
        "EmployerAddressLine1",
        "EmployerAddressLine2",
        "EmployerCity",
        "EmployerState",
        "EmployerZipCode5",
        "EmployerZipCode4",
        "EmployerEin",
        "AddressVerifiedDate",
        "EmployerVerifiedDate",
        "WorkAreaCode",
        "WorkPhone",
        "PlaceOfBirth",
        "ChildStateOfResidence",
        "ChildPaternityStatus",
        "EmployerConfirmedInd",
        "AddressConfirmedInd"
      })]
      public InterstateParticipant InterstateParticipant
      {
        get => interstateParticipant ??= new();
        set => interstateParticipant = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateParticipant interstateParticipant;
    }

    /// <summary>A OrderGroup group.</summary>
    [Serializable]
    public class OrderGroup
    {
      /// <summary>
      /// A value of InterstateSupportOrder.
      /// </summary>
      [JsonPropertyName("interstateSupportOrder")]
      [Member(Index = 1, AccessFields = false, Members = new[]
      {
        "FipsState",
        "FipsCounty",
        "FipsLocation",
        "Number",
        "OrderFilingDate",
        "Type1",
        "DebtType",
        "PaymentFreq",
        "AmountOrdered",
        "EffectiveDate",
        "EndDate",
        "CancelDate",
        "ArrearsFreq",
        "ArrearsFreqAmount",
        "ArrearsTotalAmount",
        "ArrearsAfdcFromDate",
        "ArrearsAfdcThruDate",
        "ArrearsAfdcAmount",
        "ArrearsNonAfdcFromDate",
        "ArrearsNonAfdcThruDate",
        "ArrearsNonAfdcAmount",
        "FosterCareFromDate",
        "FosterCareThruDate",
        "FosterCareAmount",
        "MedicalFromDate",
        "MedicalThruDate",
        "MedicalAmount",
        "MedicalOrderedInd",
        "TribunalCaseNumber",
        "DateOfLastPayment",
        "ControllingOrderFlag",
        "NewOrderFlag",
        "DocketNumber"
      })]
      public InterstateSupportOrder InterstateSupportOrder
      {
        get => interstateSupportOrder ??= new();
        set => interstateSupportOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder interstateSupportOrder;
    }

    /// <summary>A CollectionGroup group.</summary>
    [Serializable]
    public class CollectionGroup
    {
      /// <summary>
      /// A value of InterstateCollection.
      /// </summary>
      [JsonPropertyName("interstateCollection")]
      [Member(Index = 1, AccessFields = false, Members = new[]
      {
        "DateOfCollection",
        "DateOfPosting",
        "PaymentAmount",
        "PaymentSource",
        "InterstatePaymentMethod",
        "RdfiId",
        "RdfiAccountNum"
      })]
      public InterstateCollection InterstateCollection
      {
        get => interstateCollection ??= new();
        set => interstateCollection = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateCollection interstateCollection;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "LocalFipsState",
      "LocalFipsCounty",
      "LocalFipsLocation",
      "OtherFipsState",
      "OtherFipsCounty",
      "OtherFipsLocation",
      "TransSerialNumber",
      "ActionCode",
      "FunctionalTypeCode",
      "TransactionDate",
      "KsCaseId",
      "InterstateCaseId",
      "ActionReasonCode",
      "ActionResolutionDate",
      "AttachmentsInd",
      "CaseDataInd",
      "ApIdentificationInd",
      "ApLocateDataInd",
      "ParticipantDataInd",
      "OrderDataInd",
      "CollectionDataInd",
      "InformationInd",
      "SentDate",
      "SentTime",
      "DueDate",
      "OverdueInd",
      "DateReceived",
      "TimeReceived",
      "AttachmentsDueDate",
      "InterstateFormsPrinted",
      "CaseType",
      "CaseStatus",
      "PaymentMailingAddressLine1",
      "PaymentAddressLine2",
      "PaymentCity",
      "PaymentState",
      "PaymentZipCode5",
      "PaymentZipCode4",
      "ContactNameLast",
      "ContactNameFirst",
      "ContactNameMiddle",
      "ContactNameSuffix",
      "ContactAddressLine1",
      "ContactAddressLine2",
      "ContactCity",
      "ContactState",
      "ContactZipCode5",
      "ContactZipCode4",
      "ContactAreaCode",
      "ContactPhoneNum",
      "ContactPhoneExtension",
      "RespondingDocketNumber",
      "ContactFaxAreaCode",
      "ContactFaxNumber",
      "ContactInternetAddress",
      "InitiatingDocketNumber",
      "SendPaymentsBankAccount",
      "SendPaymentsRoutingCode",
      "StateWithCej",
      "PaymentFipsState",
      "PaymentFipsCounty",
      "PaymentFipsLocation",
      "NondisclosureFinding"
    })]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "NameLast",
      "NameFirst",
      "MiddleName",
      "NameSuffix",
      "Ssn",
      "DateOfBirth",
      "Race",
      "Sex",
      "PlaceOfBirth",
      "HeightFt",
      "HeightIn",
      "Weight",
      "HairColor",
      "EyeColor",
      "OtherIdInfo",
      "AliasSsn1",
      "AliasSsn2",
      "PossiblyDangerous",
      "MaidenName",
      "MothersMaidenOrFathersName"
    })]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "ResidentialAddressLine1",
      "ResidentialAddressLine2",
      "ResidentialCity",
      "ResidentialState",
      "ResidentialZipCode5",
      "ResidentialZipCode4",
      "MailingAddressLine1",
      "MailingAddressLine2",
      "MailingCity",
      "MailingState",
      "MailingZipCode5",
      "MailingZipCode4",
      "ResidentialAddressEffectvDate",
      "ResidentialAddressEndDate",
      "ResidentialAddressConfirmInd",
      "MailingAddressEffectiveDate",
      "MailingAddressEndDate",
      "MailingAddressConfirmedInd",
      "HomeAreaCode",
      "HomePhoneNumber",
      "WorkAreaCode",
      "WorkPhoneNumber",
      "DriversLicState",
      "DriversLicenseNum",
      "Alias1FirstName",
      "Alias1MiddleName",
      "Alias1LastName",
      "Alias1Suffix",
      "Alias2FirstName",
      "Alias2MiddleName",
      "Alias2LastName",
      "Alias2Suffix",
      "Alias3FirstName",
      "Alias3MiddleName",
      "Alias3LastName",
      "Alias3Suffix",
      "CurrentSpouseLastName",
      "CurrentSpouseFirstName",
      "CurrentSpouseMiddleName",
      "CurrentSpouseSuffix",
      "Occupation",
      "EmployerEin",
      "EmployerName",
      "EmployerAddressLine1",
      "EmployerAddressLine2",
      "EmployerCity",
      "EmployerState",
      "EmployerZipCode5",
      "EmployerZipCode4",
      "EmployerAreaCode",
      "EmployerPhoneNum",
      "EmployerEffectiveDate",
      "EmployerEndDate",
      "EmployerConfirmedInd",
      "WageQtr",
      "WageYear",
      "WageAmount",
      "InsuranceCarrierName",
      "InsurancePolicyNum",
      "LastResAddressLine1",
      "LastResAddressLine2",
      "LastResCity",
      "LastResState",
      "LastResZipCode5",
      "LastResZipCode4",
      "LastResAddressDate",
      "LastMailAddressLine1",
      "LastMailAddressLine2",
      "LastMailCity",
      "LastMailState",
      "LastMailZipCode5",
      "LastMailZipCode4",
      "LastMailAddressDate",
      "LastEmployerName",
      "LastEmployerDate",
      "LastEmployerAddressLine1",
      "LastEmployerAddressLine2",
      "LastEmployerCity",
      "LastEmployerState",
      "LastEmployerZipCode5",
      "LastEmployerZipCode4",
      "LastEmployerEndDate",
      "Employer2Ein",
      "Employer2Name",
      "Employer2AddressLine1",
      "Employer2AddressLine2",
      "Employer2City",
      "Employer2State",
      "Employer2ZipCode5",
      "Employer2ZipCode4",
      "Employer2AreaCode",
      "Employer2PhoneNumber",
      "Employer2EffectiveDate",
      "Employer2EndDate",
      "Employer2ConfirmedIndicator",
      "Employer2WageQuarter",
      "Employer2WageYear",
      "Employer2WageAmount",
      "Employer3Ein",
      "Employer3Name",
      "Employer3AddressLine1",
      "Employer3AddressLine2",
      "Employer3City",
      "Employer3State",
      "Employer3ZipCode5",
      "Employer3ZipCode4",
      "Employer3AreaCode",
      "Employer3PhoneNumber",
      "Employer3EffectiveDate",
      "Employer3EndDate",
      "Employer3ConfirmedIndicator",
      "Employer3WageQuarter",
      "Employer3WageYear",
      "Employer3WageAmount",
      "ProfessionalLicenses"
    })]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// Gets a value of Participant.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 4)]
    public Array<ParticipantGroup> Participant => participant ??= new(
      ParticipantGroup.Capacity);

    /// <summary>
    /// Gets a value of Participant for json serialization.
    /// </summary>
    [JsonPropertyName("participant")]
    [Computed]
    public IList<ParticipantGroup> Participant_Json
    {
      get => participant;
      set => Participant.Assign(value);
    }

    /// <summary>
    /// Gets a value of Order.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 5)]
    public Array<OrderGroup> Order => order ??= new(OrderGroup.Capacity);

    /// <summary>
    /// Gets a value of Order for json serialization.
    /// </summary>
    [JsonPropertyName("order")]
    [Computed]
    public IList<OrderGroup> Order_Json
    {
      get => order;
      set => Order.Assign(value);
    }

    /// <summary>
    /// Gets a value of Collection.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 6)]
    public Array<CollectionGroup> Collection => collection ??= new(
      CollectionGroup.Capacity);

    /// <summary>
    /// Gets a value of Collection for json serialization.
    /// </summary>
    [JsonPropertyName("collection")]
    [Computed]
    public IList<CollectionGroup> Collection_Json
    {
      get => collection;
      set => Collection.Assign(value);
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    [Member(Index = 7, AccessFields = false, Members = new[]
    {
      "StatusChangeCode",
      "NewCaseId",
      "InformationTextLine1",
      "InformationTextLine2",
      "InformationTextLine3",
      "InformationTextLine4",
      "InformationTextLine5"
    })]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 8, AccessFields = false, Members
      = new[] { "NumericReturnCode", "TextReturnCode" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private Array<ParticipantGroup> participant;
    private Array<OrderGroup> order;
    private Array<CollectionGroup> collection;
    private InterstateMiscellaneous interstateMiscellaneous;
    private External external;
  }
#endregion
}
