// Program: OE_FCR_SVES_PRISON, ID: 945075487, model: 746.
// Short name: SWE03664
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCR_SVES_PRISON.
/// </summary>
[Serializable]
public partial class OeFcrSvesPrison: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCR_SVES_PRISON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrSvesPrison(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrSvesPrison.
  /// </summary>
  public OeFcrSvesPrison(IContext context, Import import, Export export):
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
      if (ReadFcrSvesPrison())
      {
        MoveFcrSvesPrison(entities.ExistingFcrSvesPrison, export.FcrSvesPrison);
        MoveFcrSvesGenInfo(entities.ExistingFcrSvesGenInfo,
          export.FcrSvesGenInfo);
        local.SsnWorkArea.SsnText9 = entities.ExistingFcrSvesGenInfo.Ssn ?? Spaces
          (9);
        UseCabFormatSsnOnline3();
        local.SsnWorkArea.SsnText9 =
          entities.ExistingFcrSvesGenInfo.MultipleSsn ?? Spaces(9);
        UseCabFormatSsnOnline2();
        local.SsnWorkArea.SsnText9 =
          entities.ExistingFcrSvesPrison.PrisonReportedSsn ?? Spaces(9);
        UseCabFormatSsnOnline1();
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesPrison.ConfinementDate;
        UseCabFormatDateOnline3();
        local.DateWorkArea.Date = entities.ExistingFcrSvesPrison.ReleaseDate;
        UseCabFormatDateOnline2();
        local.DateWorkArea.Date = entities.ExistingFcrSvesPrison.ReportDate;
        UseCabFormatDateOnline1();
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesGenInfo.ReturnedDateOfBirth;
        UseCabFormatDateOnline4();
        local.Phone.Text10 =
          entities.ExistingFcrSvesPrison.PrisonFacilityPhone ?? Spaces(10);
        UseCabFormatPhoneOnline2();
        local.Phone.Text10 =
          entities.ExistingFcrSvesPrison.PrisonFacilityFaxNum ?? Spaces(10);
        UseCabFormatPhoneOnline1();

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
    else
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
    }
  }

  private static void MoveFcrSvesGenInfo(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.ReturnedFirstName = source.ReturnedFirstName;
    target.ReturnedMiddleName = source.ReturnedMiddleName;
    target.ReturnedLastName = source.ReturnedLastName;
    target.SexCode = source.SexCode;
  }

  private static void MoveFcrSvesPrison(FcrSvesPrison source,
    FcrSvesPrison target)
  {
    target.PrisonFacilityType = source.PrisonFacilityType;
    target.PrisonFacilityName = source.PrisonFacilityName;
    target.PrisonFacilityContactName = source.PrisonFacilityContactName;
    target.PrisonerReporterName = source.PrisonerReporterName;
    target.PrisonerIdNumber = source.PrisonerIdNumber;
    target.PrisonReportedSuffix = source.PrisonReportedSuffix;
  }

  private void UseCabFormatDateOnline1()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedReportDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline2()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedReleaseDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline3()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedConfineDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline4()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedDob.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatPhoneOnline1()
  {
    var useImport = new CabFormatPhoneOnline.Import();
    var useExport = new CabFormatPhoneOnline.Export();

    useImport.Phone.Text10 = local.Phone.Text10;

    Call(CabFormatPhoneOnline.Execute, useImport, useExport);

    export.FormattedFax.Text12 = useExport.FormattedPhone.Text12;
  }

  private void UseCabFormatPhoneOnline2()
  {
    var useImport = new CabFormatPhoneOnline.Import();
    var useExport = new CabFormatPhoneOnline.Export();

    useImport.Phone.Text10 = local.Phone.Text10;

    Call(CabFormatPhoneOnline.Execute, useImport, useExport);

    export.FormattedPhone.Text12 = useExport.FormattedPhone.Text12;
  }

  private void UseCabFormatSsnOnline1()
  {
    var useImport = new CabFormatSsnOnline.Import();
    var useExport = new CabFormatSsnOnline.Export();

    useImport.Ssn.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabFormatSsnOnline.Execute, useImport, useExport);

    export.FormattedReportedSsn.Text11 = useExport.FormattedSsn.Text11;
  }

  private void UseCabFormatSsnOnline2()
  {
    var useImport = new CabFormatSsnOnline.Import();
    var useExport = new CabFormatSsnOnline.Export();

    useImport.Ssn.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabFormatSsnOnline.Execute, useImport, useExport);

    export.FormattedMultiSsn.Text11 = useExport.FormattedSsn.Text11;
  }

  private void UseCabFormatSsnOnline3()
  {
    var useImport = new CabFormatSsnOnline.Import();
    var useExport = new CabFormatSsnOnline.Export();

    useImport.Ssn.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabFormatSsnOnline.Execute, useImport, useExport);

    export.FormattedSsn.Text11 = useExport.FormattedSsn.Text11;
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
        entities.ExistingFcrSvesAddress.CreatedBy = db.GetString(reader, 3);
        entities.ExistingFcrSvesAddress.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingFcrSvesAddress.State = db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesAddress.ZipCode5 =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesAddress.ZipCode4 =
          db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesAddress.City = db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesAddress.AddressLine1 =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrSvesAddress.AddressLine2 =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesAddress.AddressLine3 =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrSvesAddress.AddressLine4 =
          db.GetNullableString(reader, 12);
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
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesGenInfo.MemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo =
          db.GetString(reader, 1);
        entities.ExistingFcrSvesGenInfo.SexCode =
          db.GetNullableString(reader, 2);
        entities.ExistingFcrSvesGenInfo.ReturnedDateOfBirth =
          db.GetNullableDate(reader, 3);
        entities.ExistingFcrSvesGenInfo.Ssn = db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesGenInfo.FipsCountyCode =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesGenInfo.LocateResponseCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesGenInfo.MultipleSsn =
          db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesGenInfo.ParticipantType =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesGenInfo.ReturnedFirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrSvesGenInfo.ReturnedMiddleName =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesGenInfo.ReturnedLastName =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrSvesGenInfo.UserField =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrSvesGenInfo.Populated = true;
      });
  }

  private bool ReadFcrSvesPrison()
  {
    entities.ExistingFcrSvesPrison.Populated = false;

    return Read("ReadFcrSvesPrison",
      (db, command) =>
      {
        db.SetString(
          command, "fcgLSRspAgy",
          entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo);
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesGenInfo.MemberId);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesPrison.FcgMemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesPrison.FcgLSRspAgy = db.GetString(reader, 1);
        entities.ExistingFcrSvesPrison.SeqNo = db.GetInt32(reader, 2);
        entities.ExistingFcrSvesPrison.PrisonFacilityType =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesPrison.PrisonFacilityPhone =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesPrison.PrisonFacilityFaxNum =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesPrison.PrisonerIdNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesPrison.PrisonReportedSsn =
          db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesPrison.PrisonReportedSuffix =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesPrison.ConfinementDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingFcrSvesPrison.ReleaseDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingFcrSvesPrison.ReportDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingFcrSvesPrison.CreatedBy = db.GetString(reader, 12);
        entities.ExistingFcrSvesPrison.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.ExistingFcrSvesPrison.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingFcrSvesPrison.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.ExistingFcrSvesPrison.PrisonFacilityName =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrSvesPrison.PrisonFacilityContactName =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrSvesPrison.PrisonerReporterName =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrSvesPrison.Populated = true;
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
    /// A value of FormattedReportDate.
    /// </summary>
    [JsonPropertyName("formattedReportDate")]
    public WorkArea FormattedReportDate
    {
      get => formattedReportDate ??= new();
      set => formattedReportDate = value;
    }

    /// <summary>
    /// A value of FormattedReleaseDate.
    /// </summary>
    [JsonPropertyName("formattedReleaseDate")]
    public WorkArea FormattedReleaseDate
    {
      get => formattedReleaseDate ??= new();
      set => formattedReleaseDate = value;
    }

    /// <summary>
    /// A value of FormattedConfineDate.
    /// </summary>
    [JsonPropertyName("formattedConfineDate")]
    public WorkArea FormattedConfineDate
    {
      get => formattedConfineDate ??= new();
      set => formattedConfineDate = value;
    }

    /// <summary>
    /// A value of FormattedDob.
    /// </summary>
    [JsonPropertyName("formattedDob")]
    public WorkArea FormattedDob
    {
      get => formattedDob ??= new();
      set => formattedDob = value;
    }

    /// <summary>
    /// A value of FormattedFax.
    /// </summary>
    [JsonPropertyName("formattedFax")]
    public TextWorkArea FormattedFax
    {
      get => formattedFax ??= new();
      set => formattedFax = value;
    }

    /// <summary>
    /// A value of FormattedPhone.
    /// </summary>
    [JsonPropertyName("formattedPhone")]
    public TextWorkArea FormattedPhone
    {
      get => formattedPhone ??= new();
      set => formattedPhone = value;
    }

    /// <summary>
    /// A value of FormattedReportedSsn.
    /// </summary>
    [JsonPropertyName("formattedReportedSsn")]
    public WorkArea FormattedReportedSsn
    {
      get => formattedReportedSsn ??= new();
      set => formattedReportedSsn = value;
    }

    /// <summary>
    /// A value of FormattedMultiSsn.
    /// </summary>
    [JsonPropertyName("formattedMultiSsn")]
    public WorkArea FormattedMultiSsn
    {
      get => formattedMultiSsn ??= new();
      set => formattedMultiSsn = value;
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

    /// <summary>
    /// A value of FcrSvesPrison.
    /// </summary>
    [JsonPropertyName("fcrSvesPrison")]
    public FcrSvesPrison FcrSvesPrison
    {
      get => fcrSvesPrison ??= new();
      set => fcrSvesPrison = value;
    }

    /// <summary>
    /// A value of Do03.
    /// </summary>
    [JsonPropertyName("do03")]
    public FcrSvesAddress Do03
    {
      get => do03 ??= new();
      set => do03 = value;
    }

    private WorkArea formattedReportDate;
    private WorkArea formattedReleaseDate;
    private WorkArea formattedConfineDate;
    private WorkArea formattedDob;
    private TextWorkArea formattedFax;
    private TextWorkArea formattedPhone;
    private WorkArea formattedReportedSsn;
    private WorkArea formattedMultiSsn;
    private WorkArea formattedSsn;
    private WorkArea svesErrorMsgs;
    private FcrSvesAddress fcrSvesAddress;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesPrison fcrSvesPrison;
    private FcrSvesAddress do03;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Phone.
    /// </summary>
    [JsonPropertyName("phone")]
    public TextWorkArea Phone
    {
      get => phone ??= new();
      set => phone = value;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
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

    private TextWorkArea phone;
    private SsnWorkArea ssnWorkArea;
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
    /// A value of ExistingFcrSvesPrison.
    /// </summary>
    [JsonPropertyName("existingFcrSvesPrison")]
    public FcrSvesPrison ExistingFcrSvesPrison
    {
      get => existingFcrSvesPrison ??= new();
      set => existingFcrSvesPrison = value;
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
    private FcrSvesPrison existingFcrSvesPrison;
    private FcrSvesAddress existingFcrSvesAddress;
  }
#endregion
}
