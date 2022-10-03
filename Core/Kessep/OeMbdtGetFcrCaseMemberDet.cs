// Program: OE_MBDT_GET_FCR_CASE_MEMBER_DET, ID: 374574881, model: 746.
// Short name: SWE00417
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_MBDT_GET_FCR_CASE_MEMBER_DET.
/// </para>
/// <para>
/// This Action Block retrieves the FCR Case member details for a given FCR Case
/// Number and FCR Member Id.
/// </para>
/// </summary>
[Serializable]
public partial class OeMbdtGetFcrCaseMemberDet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MBDT_GET_FCR_CASE_MEMBER_DET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMbdtGetFcrCaseMemberDet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMbdtGetFcrCaseMemberDet.
  /// </summary>
  public OeMbdtGetFcrCaseMemberDet(IContext context, Import import,
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
    // ---------------------------------------------------------------
    // Date		Developer	Request #      Description
    // ---------------------------------------------------------------
    // 09/10/2009	M Fan		CQ7190	       Initial Dev
    // ---------------------------------------------------------------
    // *********************************************************************************
    // This Action retrives the FCR Case Member details for the given FCR Case 
    // number &
    // FCR Case Member Id.  After retrieving the information splits the 
    // information sent
    // by CSE and FCR response information as well.
    // *********************************************************************************
    if (ReadFcrCaseMembers())
    {
      MoveFcrCaseMembers3(entities.ExistingFcrCaseMembers, export.CseMemberInfo);
        
      MoveFcrCaseMembers2(entities.ExistingFcrCaseMembers, export.FcrMemberInfo);
        
      MoveFcrCaseMembers1(entities.ExistingFcrCaseMembers, export.FcrMember);
    }
    else
    {
      ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";
    }
  }

  private static void MoveFcrCaseMembers1(FcrCaseMembers source,
    FcrCaseMembers target)
  {
    target.MemberId = source.MemberId;
    target.ActionTypeCode = source.ActionTypeCode;
    target.ParticipantType = source.ParticipantType;
    target.BatchNumber = source.BatchNumber;
  }

  private static void MoveFcrCaseMembers2(FcrCaseMembers source,
    FcrCaseMembers target)
  {
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleName = source.MiddleName;
    target.LastName = source.LastName;
    target.FamilyViolence = source.FamilyViolence;
    target.PreviousSsn = source.PreviousSsn;
    target.AdditionalSsn1 = source.AdditionalSsn1;
    target.AdditionalSsn2 = source.AdditionalSsn2;
    target.AdditionalFirstName1 = source.AdditionalFirstName1;
    target.AdditionalMiddleName1 = source.AdditionalMiddleName1;
    target.AdditionalLastName1 = source.AdditionalLastName1;
    target.AdditionalFirstName2 = source.AdditionalFirstName2;
    target.AdditionalMiddleName2 = source.AdditionalMiddleName2;
    target.AdditionalLastName2 = source.AdditionalLastName2;
    target.AdditionalFirstName3 = source.AdditionalFirstName3;
    target.AdditionalMiddleName3 = source.AdditionalMiddleName3;
    target.AdditionalLastName3 = source.AdditionalLastName3;
    target.AdditionalFirstName4 = source.AdditionalFirstName4;
    target.AdditionalMiddleName4 = source.AdditionalMiddleName4;
    target.AdditionalLastName4 = source.AdditionalLastName4;
    target.SsnValidityCode = source.SsnValidityCode;
    target.ProvidedOrCorrectedSsn = source.ProvidedOrCorrectedSsn;
    target.SsaDateOfBirthIndicator = source.SsaDateOfBirthIndicator;
    target.DateOfDeath = source.DateOfDeath;
    target.SsaZipCodeOfLastResidence = source.SsaZipCodeOfLastResidence;
    target.SsaZipCodeOfLumpSumPayment = source.SsaZipCodeOfLumpSumPayment;
    target.FcrPrimarySsn = source.FcrPrimarySsn;
    target.FcrPrimaryFirstName = source.FcrPrimaryFirstName;
    target.FcrPrimaryMiddleName = source.FcrPrimaryMiddleName;
    target.FcrPrimaryLastName = source.FcrPrimaryLastName;
    target.AdditionalSsn1ValidityCode = source.AdditionalSsn1ValidityCode;
    target.AdditionalSsn2ValidityCode = source.AdditionalSsn2ValidityCode;
    target.SsaCityOfLastResidence = source.SsaCityOfLastResidence;
    target.SsaStateOfLastResidence = source.SsaStateOfLastResidence;
    target.SsaCityOfLumpSumPayment = source.SsaCityOfLumpSumPayment;
    target.SsaStateOfLumpSumPayment = source.SsaStateOfLumpSumPayment;
  }

  private static void MoveFcrCaseMembers3(FcrCaseMembers source,
    FcrCaseMembers target)
  {
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleName = source.MiddleName;
    target.LastName = source.LastName;
    target.FamilyViolence = source.FamilyViolence;
    target.PreviousSsn = source.PreviousSsn;
    target.AdditionalSsn1 = source.AdditionalSsn1;
    target.AdditionalSsn2 = source.AdditionalSsn2;
    target.AdditionalFirstName1 = source.AdditionalFirstName1;
    target.AdditionalMiddleName1 = source.AdditionalMiddleName1;
    target.AdditionalLastName1 = source.AdditionalLastName1;
    target.AdditionalFirstName2 = source.AdditionalFirstName2;
    target.AdditionalMiddleName2 = source.AdditionalMiddleName2;
    target.AdditionalLastName2 = source.AdditionalLastName2;
    target.AdditionalFirstName3 = source.AdditionalFirstName3;
    target.AdditionalMiddleName3 = source.AdditionalMiddleName3;
    target.AdditionalLastName3 = source.AdditionalLastName3;
    target.AdditionalFirstName4 = source.AdditionalFirstName4;
    target.AdditionalMiddleName4 = source.AdditionalMiddleName4;
    target.AdditionalLastName4 = source.AdditionalLastName4;
    target.SsnValidityCode = source.SsnValidityCode;
    target.ProvidedOrCorrectedSsn = source.ProvidedOrCorrectedSsn;
    target.SsaDateOfBirthIndicator = source.SsaDateOfBirthIndicator;
    target.SsaZipCodeOfLastResidence = source.SsaZipCodeOfLastResidence;
    target.SsaZipCodeOfLumpSumPayment = source.SsaZipCodeOfLumpSumPayment;
    target.AdditionalSsn1ValidityCode = source.AdditionalSsn1ValidityCode;
    target.AdditionalSsn2ValidityCode = source.AdditionalSsn2ValidityCode;
  }

  private bool ReadFcrCaseMembers()
  {
    entities.ExistingFcrCaseMembers.Populated = false;

    return Read("ReadFcrCaseMembers",
      (db, command) =>
      {
        db.SetString(command, "memberId", import.FcrMember.MemberId);
        db.SetString(command, "fcmCaseId", import.FcrCaseMaster.CaseId);
      },
      (db, reader) =>
      {
        entities.ExistingFcrCaseMembers.FcmCaseId = db.GetString(reader, 0);
        entities.ExistingFcrCaseMembers.MemberId = db.GetString(reader, 1);
        entities.ExistingFcrCaseMembers.ActionTypeCode =
          db.GetNullableString(reader, 2);
        entities.ExistingFcrCaseMembers.LocateRequestType =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrCaseMembers.RecordIdentifier =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrCaseMembers.ParticipantType =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrCaseMembers.SexCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrCaseMembers.DateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.ExistingFcrCaseMembers.Ssn = db.GetNullableString(reader, 8);
        entities.ExistingFcrCaseMembers.FirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrCaseMembers.MiddleName =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrCaseMembers.LastName =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrCaseMembers.FipsCountyCode =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrCaseMembers.FamilyViolence =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrCaseMembers.PreviousSsn =
          db.GetNullableString(reader, 14);
        entities.ExistingFcrCaseMembers.CityOfBirth =
          db.GetNullableString(reader, 15);
        entities.ExistingFcrCaseMembers.StateOrCountryOfBirth =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrCaseMembers.FathersFirstName =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrCaseMembers.FathersMiddleInitial =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrCaseMembers.FathersLastName =
          db.GetNullableString(reader, 19);
        entities.ExistingFcrCaseMembers.MothersFirstName =
          db.GetNullableString(reader, 20);
        entities.ExistingFcrCaseMembers.MothersMiddleInitial =
          db.GetNullableString(reader, 21);
        entities.ExistingFcrCaseMembers.MothersMaidenNm =
          db.GetNullableString(reader, 22);
        entities.ExistingFcrCaseMembers.IrsUSsn =
          db.GetNullableString(reader, 23);
        entities.ExistingFcrCaseMembers.AdditionalSsn1 =
          db.GetNullableString(reader, 24);
        entities.ExistingFcrCaseMembers.AdditionalSsn2 =
          db.GetNullableString(reader, 25);
        entities.ExistingFcrCaseMembers.AdditionalFirstName1 =
          db.GetNullableString(reader, 26);
        entities.ExistingFcrCaseMembers.AdditionalMiddleName1 =
          db.GetNullableString(reader, 27);
        entities.ExistingFcrCaseMembers.AdditionalLastName1 =
          db.GetNullableString(reader, 28);
        entities.ExistingFcrCaseMembers.AdditionalFirstName2 =
          db.GetNullableString(reader, 29);
        entities.ExistingFcrCaseMembers.AdditionalMiddleName2 =
          db.GetNullableString(reader, 30);
        entities.ExistingFcrCaseMembers.AdditionalLastName2 =
          db.GetNullableString(reader, 31);
        entities.ExistingFcrCaseMembers.AdditionalFirstName3 =
          db.GetNullableString(reader, 32);
        entities.ExistingFcrCaseMembers.AdditionalMiddleName3 =
          db.GetNullableString(reader, 33);
        entities.ExistingFcrCaseMembers.AdditionalLastName3 =
          db.GetNullableString(reader, 34);
        entities.ExistingFcrCaseMembers.AdditionalFirstName4 =
          db.GetNullableString(reader, 35);
        entities.ExistingFcrCaseMembers.AdditionalMiddleName4 =
          db.GetNullableString(reader, 36);
        entities.ExistingFcrCaseMembers.AdditionalLastName4 =
          db.GetNullableString(reader, 37);
        entities.ExistingFcrCaseMembers.NewMemberId =
          db.GetNullableString(reader, 38);
        entities.ExistingFcrCaseMembers.Irs1099 =
          db.GetNullableString(reader, 39);
        entities.ExistingFcrCaseMembers.LocateSource1 =
          db.GetNullableString(reader, 40);
        entities.ExistingFcrCaseMembers.LocateSource2 =
          db.GetNullableString(reader, 41);
        entities.ExistingFcrCaseMembers.LocateSource3 =
          db.GetNullableString(reader, 42);
        entities.ExistingFcrCaseMembers.LocateSource4 =
          db.GetNullableString(reader, 43);
        entities.ExistingFcrCaseMembers.LocateSource5 =
          db.GetNullableString(reader, 44);
        entities.ExistingFcrCaseMembers.LocateSource6 =
          db.GetNullableString(reader, 45);
        entities.ExistingFcrCaseMembers.LocateSource7 =
          db.GetNullableString(reader, 46);
        entities.ExistingFcrCaseMembers.LocateSource8 =
          db.GetNullableString(reader, 47);
        entities.ExistingFcrCaseMembers.SsnValidityCode =
          db.GetNullableString(reader, 48);
        entities.ExistingFcrCaseMembers.ProvidedOrCorrectedSsn =
          db.GetNullableString(reader, 49);
        entities.ExistingFcrCaseMembers.MultipleSsn1 =
          db.GetNullableString(reader, 50);
        entities.ExistingFcrCaseMembers.MultipleSsn2 =
          db.GetNullableString(reader, 51);
        entities.ExistingFcrCaseMembers.MultipleSsn3 =
          db.GetNullableString(reader, 52);
        entities.ExistingFcrCaseMembers.SsaDateOfBirthIndicator =
          db.GetNullableString(reader, 53);
        entities.ExistingFcrCaseMembers.BatchNumber =
          db.GetNullableString(reader, 54);
        entities.ExistingFcrCaseMembers.DateOfDeath =
          db.GetNullableDate(reader, 55);
        entities.ExistingFcrCaseMembers.SsaZipCodeOfLastResidence =
          db.GetNullableString(reader, 56);
        entities.ExistingFcrCaseMembers.SsaZipCodeOfLumpSumPayment =
          db.GetNullableString(reader, 57);
        entities.ExistingFcrCaseMembers.FcrPrimarySsn =
          db.GetNullableString(reader, 58);
        entities.ExistingFcrCaseMembers.FcrPrimaryFirstName =
          db.GetNullableString(reader, 59);
        entities.ExistingFcrCaseMembers.FcrPrimaryMiddleName =
          db.GetNullableString(reader, 60);
        entities.ExistingFcrCaseMembers.FcrPrimaryLastName =
          db.GetNullableString(reader, 61);
        entities.ExistingFcrCaseMembers.AcknowledgementCode =
          db.GetNullableString(reader, 62);
        entities.ExistingFcrCaseMembers.ErrorCode1 =
          db.GetNullableString(reader, 63);
        entities.ExistingFcrCaseMembers.ErrorCode2 =
          db.GetNullableString(reader, 64);
        entities.ExistingFcrCaseMembers.ErrorCode3 =
          db.GetNullableString(reader, 65);
        entities.ExistingFcrCaseMembers.ErrorCode4 =
          db.GetNullableString(reader, 66);
        entities.ExistingFcrCaseMembers.ErrorCode5 =
          db.GetNullableString(reader, 67);
        entities.ExistingFcrCaseMembers.AdditionalSsn1ValidityCode =
          db.GetNullableString(reader, 68);
        entities.ExistingFcrCaseMembers.AdditionalSsn2ValidityCode =
          db.GetNullableString(reader, 69);
        entities.ExistingFcrCaseMembers.BundleFplsLocateResults =
          db.GetNullableString(reader, 70);
        entities.ExistingFcrCaseMembers.SsaCityOfLastResidence =
          db.GetNullableString(reader, 71);
        entities.ExistingFcrCaseMembers.SsaStateOfLastResidence =
          db.GetNullableString(reader, 72);
        entities.ExistingFcrCaseMembers.SsaCityOfLumpSumPayment =
          db.GetNullableString(reader, 73);
        entities.ExistingFcrCaseMembers.SsaStateOfLumpSumPayment =
          db.GetNullableString(reader, 74);
        entities.ExistingFcrCaseMembers.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of FcrCaseMaster.
    /// </summary>
    [JsonPropertyName("fcrCaseMaster")]
    public FcrCaseMaster FcrCaseMaster
    {
      get => fcrCaseMaster ??= new();
      set => fcrCaseMaster = value;
    }

    /// <summary>
    /// A value of FcrMember.
    /// </summary>
    [JsonPropertyName("fcrMember")]
    public FcrCaseMembers FcrMember
    {
      get => fcrMember ??= new();
      set => fcrMember = value;
    }

    private FcrCaseMaster fcrCaseMaster;
    private FcrCaseMembers fcrMember;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FcrMember.
    /// </summary>
    [JsonPropertyName("fcrMember")]
    public FcrCaseMembers FcrMember
    {
      get => fcrMember ??= new();
      set => fcrMember = value;
    }

    /// <summary>
    /// A value of CseMemberInfo.
    /// </summary>
    [JsonPropertyName("cseMemberInfo")]
    public FcrCaseMembers CseMemberInfo
    {
      get => cseMemberInfo ??= new();
      set => cseMemberInfo = value;
    }

    /// <summary>
    /// A value of FcrMemberInfo.
    /// </summary>
    [JsonPropertyName("fcrMemberInfo")]
    public FcrCaseMembers FcrMemberInfo
    {
      get => fcrMemberInfo ??= new();
      set => fcrMemberInfo = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public FcrCaseMaster Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private FcrCaseMembers fcrMember;
    private FcrCaseMembers cseMemberInfo;
    private FcrCaseMembers fcrMemberInfo;
    private FcrCaseMaster zdel;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFcrCaseMaster.
    /// </summary>
    [JsonPropertyName("existingFcrCaseMaster")]
    public FcrCaseMaster ExistingFcrCaseMaster
    {
      get => existingFcrCaseMaster ??= new();
      set => existingFcrCaseMaster = value;
    }

    /// <summary>
    /// A value of ExistingFcrCaseMembers.
    /// </summary>
    [JsonPropertyName("existingFcrCaseMembers")]
    public FcrCaseMembers ExistingFcrCaseMembers
    {
      get => existingFcrCaseMembers ??= new();
      set => existingFcrCaseMembers = value;
    }

    private FcrCaseMaster existingFcrCaseMaster;
    private FcrCaseMembers existingFcrCaseMembers;
  }
#endregion
}
