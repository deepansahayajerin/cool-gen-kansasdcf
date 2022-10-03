// Program: OE_FCR_SVES_GEN_INFO, ID: 945074686, model: 746.
// Short name: SWE03660
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCR_SVES_GEN_INFO.
/// </summary>
[Serializable]
public partial class OeFcrSvesGenInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCR_SVES_GEN_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrSvesGenInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrSvesGenInfo.
  /// </summary>
  public OeFcrSvesGenInfo(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // * ----------  -----------------  ---------   -----------------------
    // * 07/08/11    LSS                CQ5577      Initial Coding.
    // *
    // ***********************************************************************
    // Fields needing formatted (dates, ssn, phone) are being exported as 
    // formatted work views.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Person.Text15 = "00000" + import.CsePersonsWorkSet.Number;

    if (ReadFcrSvesGenInfo())
    {
      MoveFcrSvesGenInfo(entities.ExistingFcrSvesGenInfo, export.FcrSvesGenInfo);
        
      local.DateWorkArea.Date =
        entities.ExistingFcrSvesGenInfo.SubmittedDateOfBirth;
      UseCabFormatDateOnline1();
      local.DateWorkArea.Date =
        entities.ExistingFcrSvesGenInfo.ReturnedDateOfBirth;
      UseCabFormatDateOnline2();
      local.DateWorkArea.Date =
        entities.ExistingFcrSvesGenInfo.ReturnedDateOfDeath;
      UseCabFormatDateOnline3();
      local.Ssn.SsnText9 = entities.ExistingFcrSvesGenInfo.Ssn ?? Spaces(9);
      UseCabFormatSsnOnline1();
      local.Ssn.SsnText9 = entities.ExistingFcrSvesGenInfo.MultipleSsn ?? Spaces
        (9);
      UseCabFormatSsnOnline2();

      // ***********************************************
      // Address types:
      // 01 - Residential Address
      // 02 - Person/Claimant Address
      // 03 - District Office Address
      // 04 - Payee Mailing Address
      // 05 - Prison Address
      // ***********************************************
      if (ReadFcrSvesAddress())
      {
        export.FcrSvesAddress.Assign(entities.ExistingFcrSvesAddress);
      }
    }
    else
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
    }
  }

  private static void MoveFcrSvesGenInfo(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.SvesMatchType = source.SvesMatchType;
    target.ReturnedFirstName = source.ReturnedFirstName;
    target.ReturnedMiddleName = source.ReturnedMiddleName;
    target.ReturnedLastName = source.ReturnedLastName;
    target.SexCode = source.SexCode;
    target.UserField = source.UserField;
    target.LocateClosedIndicator = source.LocateClosedIndicator;
    target.FipsCountyCode = source.FipsCountyCode;
    target.LocateRequestType = source.LocateRequestType;
    target.LocateResponseCode = source.LocateResponseCode;
    target.MultipleSsnIndicator = source.MultipleSsnIndicator;
    target.MultipleSsn = source.MultipleSsn;
    target.ParticipantType = source.ParticipantType;
    target.FamilyViolenceState1 = source.FamilyViolenceState1;
    target.FamilyViolenceState2 = source.FamilyViolenceState2;
    target.FamilyViolenceState3 = source.FamilyViolenceState3;
  }

  private void UseCabFormatDateOnline1()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedDobSub.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline2()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedDobRet.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline3()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedDodRet.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatSsnOnline1()
  {
    var useImport = new CabFormatSsnOnline.Import();
    var useExport = new CabFormatSsnOnline.Export();

    useImport.Ssn.SsnText9 = local.Ssn.SsnText9;

    Call(CabFormatSsnOnline.Execute, useImport, useExport);

    export.FormattedSsn.Text11 = useExport.FormattedSsn.Text11;
  }

  private void UseCabFormatSsnOnline2()
  {
    var useImport = new CabFormatSsnOnline.Import();
    var useExport = new CabFormatSsnOnline.Export();

    useImport.Ssn.SsnText9 = local.Ssn.SsnText9;

    Call(CabFormatSsnOnline.Execute, useImport, useExport);

    export.FormattedSsnMulti.Text11 = useExport.FormattedSsn.Text11;
  }

  private bool ReadFcrSvesAddress()
  {
    entities.ExistingFcrSvesAddress.Populated = false;

    return Read("ReadFcrSvesAddress",
      (db, command) =>
      {
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesGenInfo.MemberId);
        db.SetString(
          command, "fcgLSRspAgy",
          entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesAddress.FcgMemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesAddress.FcgLSRspAgy = db.GetString(reader, 1);
        entities.ExistingFcrSvesAddress.SvesAddressTypeCode =
          db.GetString(reader, 2);
        entities.ExistingFcrSvesAddress.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingFcrSvesAddress.State = db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesAddress.ZipCode5 =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesAddress.City = db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesAddress.AddressLine1 =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesAddress.AddressLine2 =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrSvesAddress.AddressLine3 =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesAddress.AddressLine4 =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrSvesAddress.Populated = true;
      });
  }

  private bool ReadFcrSvesGenInfo()
  {
    entities.ExistingFcrSvesGenInfo.Populated = false;

    return Read("ReadFcrSvesGenInfo",
      (db, command) =>
      {
        db.SetString(command, "memberId", local.Person.Text15);
        db.SetString(
          command, "locSrcRspAgyCd",
          import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesGenInfo.MemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo =
          db.GetString(reader, 1);
        entities.ExistingFcrSvesGenInfo.SvesMatchType =
          db.GetNullableString(reader, 2);
        entities.ExistingFcrSvesGenInfo.TransmitterStateTerritoryCode =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesGenInfo.SexCode =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesGenInfo.ReturnedDateOfBirth =
          db.GetNullableDate(reader, 5);
        entities.ExistingFcrSvesGenInfo.ReturnedDateOfDeath =
          db.GetNullableDate(reader, 6);
        entities.ExistingFcrSvesGenInfo.SubmittedDateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.ExistingFcrSvesGenInfo.Ssn = db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesGenInfo.LocateClosedIndicator =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrSvesGenInfo.FipsCountyCode =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesGenInfo.LocateRequestType =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrSvesGenInfo.LocateResponseCode =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrSvesGenInfo.MultipleSsnIndicator =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrSvesGenInfo.MultipleSsn =
          db.GetNullableString(reader, 14);
        entities.ExistingFcrSvesGenInfo.ParticipantType =
          db.GetNullableString(reader, 15);
        entities.ExistingFcrSvesGenInfo.FamilyViolenceState1 =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrSvesGenInfo.FamilyViolenceState2 =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrSvesGenInfo.FamilyViolenceState3 =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrSvesGenInfo.SortStateCode =
          db.GetNullableString(reader, 19);
        entities.ExistingFcrSvesGenInfo.RequestDate =
          db.GetNullableDate(reader, 20);
        entities.ExistingFcrSvesGenInfo.ResponseReceivedDate =
          db.GetNullableDate(reader, 21);
        entities.ExistingFcrSvesGenInfo.CreatedBy = db.GetString(reader, 22);
        entities.ExistingFcrSvesGenInfo.CreatedTimestamp =
          db.GetDateTime(reader, 23);
        entities.ExistingFcrSvesGenInfo.LastUpdatedBy =
          db.GetNullableString(reader, 24);
        entities.ExistingFcrSvesGenInfo.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 25);
        entities.ExistingFcrSvesGenInfo.ReturnedFirstName =
          db.GetNullableString(reader, 26);
        entities.ExistingFcrSvesGenInfo.ReturnedMiddleName =
          db.GetNullableString(reader, 27);
        entities.ExistingFcrSvesGenInfo.ReturnedLastName =
          db.GetNullableString(reader, 28);
        entities.ExistingFcrSvesGenInfo.SubmittedFirstName =
          db.GetNullableString(reader, 29);
        entities.ExistingFcrSvesGenInfo.SubmittedMiddleName =
          db.GetNullableString(reader, 30);
        entities.ExistingFcrSvesGenInfo.SubmittedLastName =
          db.GetNullableString(reader, 31);
        entities.ExistingFcrSvesGenInfo.UserField =
          db.GetNullableString(reader, 32);
        entities.ExistingFcrSvesGenInfo.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private FcrSvesGenInfo fcrSvesGenInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FormattedDobSub.
    /// </summary>
    [JsonPropertyName("formattedDobSub")]
    public WorkArea FormattedDobSub
    {
      get => formattedDobSub ??= new();
      set => formattedDobSub = value;
    }

    /// <summary>
    /// A value of FormattedDobRet.
    /// </summary>
    [JsonPropertyName("formattedDobRet")]
    public WorkArea FormattedDobRet
    {
      get => formattedDobRet ??= new();
      set => formattedDobRet = value;
    }

    /// <summary>
    /// A value of FormattedDodRet.
    /// </summary>
    [JsonPropertyName("formattedDodRet")]
    public WorkArea FormattedDodRet
    {
      get => formattedDodRet ??= new();
      set => formattedDodRet = value;
    }

    /// <summary>
    /// A value of FormattedSsn.
    /// </summary>
    [JsonPropertyName("formattedSsn")]
    public WorkArea FormattedSsn
    {
      get => formattedSsn ??= new();
      set => formattedSsn = value;
    }

    /// <summary>
    /// A value of FormattedSsnMulti.
    /// </summary>
    [JsonPropertyName("formattedSsnMulti")]
    public WorkArea FormattedSsnMulti
    {
      get => formattedSsnMulti ??= new();
      set => formattedSsnMulti = value;
    }

    /// <summary>
    /// A value of SvesErrorMsgs.
    /// </summary>
    [JsonPropertyName("svesErrorMsgs")]
    public WorkArea SvesErrorMsgs
    {
      get => svesErrorMsgs ??= new();
      set => svesErrorMsgs = value;
    }

    /// <summary>
    /// A value of FcrSvesAddress.
    /// </summary>
    [JsonPropertyName("fcrSvesAddress")]
    public FcrSvesAddress FcrSvesAddress
    {
      get => fcrSvesAddress ??= new();
      set => fcrSvesAddress = value;
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    private WorkArea formattedDobSub;
    private WorkArea formattedDobRet;
    private WorkArea formattedDodRet;
    private WorkArea formattedSsn;
    private WorkArea formattedSsnMulti;
    private WorkArea svesErrorMsgs;
    private FcrSvesAddress fcrSvesAddress;
    private FcrSvesGenInfo fcrSvesGenInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Ssn.
    /// </summary>
    [JsonPropertyName("ssn")]
    public SsnWorkArea Ssn
    {
      get => ssn ??= new();
      set => ssn = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Person.
    /// </summary>
    [JsonPropertyName("person")]
    public WorkArea Person
    {
      get => person ??= new();
      set => person = value;
    }

    private SsnWorkArea ssn;
    private DateWorkArea dateWorkArea;
    private WorkArea person;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("existingFcrSvesGenInfo")]
    public FcrSvesGenInfo ExistingFcrSvesGenInfo
    {
      get => existingFcrSvesGenInfo ??= new();
      set => existingFcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of ExistingFcrSvesAddress.
    /// </summary>
    [JsonPropertyName("existingFcrSvesAddress")]
    public FcrSvesAddress ExistingFcrSvesAddress
    {
      get => existingFcrSvesAddress ??= new();
      set => existingFcrSvesAddress = value;
    }

    private FcrSvesGenInfo existingFcrSvesGenInfo;
    private FcrSvesAddress existingFcrSvesAddress;
  }
#endregion
}
