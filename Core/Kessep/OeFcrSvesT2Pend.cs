// Program: OE_FCR_SVES_T2_PEND, ID: 945076870, model: 746.
// Short name: SWE03661
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCR_SVES_T2_PEND.
/// </summary>
[Serializable]
public partial class OeFcrSvesT2Pend: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCR_SVES_T2_PEND program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrSvesT2Pend(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrSvesT2Pend.
  /// </summary>
  public OeFcrSvesT2Pend(IContext context, Import import, Export export):
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
      if (ReadFcrSvesTitleIiPend())
      {
        MoveFcrSvesTitleIiPend(entities.ExistingFcrSvesTitleIiPend,
          export.FcrSvesTitleIiPend);
        export.FcrSvesGenInfo.Assign(entities.ExistingFcrSvesGenInfo);
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleIiPend.ResponseDate;
        UseCabFormatDateOnline();
        local.SsnWorkArea.SsnText9 = entities.ExistingFcrSvesGenInfo.Ssn ?? Spaces
          (9);
        UseCabFormatSsnOnline3();
        local.SsnWorkArea.SsnText9 = entities.ExistingFcrSvesGenInfo.Ssn ?? Spaces
          (9);
        UseCabFormatSsnOnline2();
        local.SsnWorkArea.SsnText9 =
          entities.ExistingFcrSvesTitleIiPend.OtherSsn ?? Spaces(9);
        UseCabFormatSsnOnline1();

        // ***********************************************
        // Address types:
        // 01 - Residential Address
        // 02 - Person/Claimant Address
        // 03 - District Office Address
        // 04 - Payee Mailing Address
        // 05 - Prison Address
        // ***********************************************
        if (ReadFcrSvesAddress1())
        {
          export.Cl02.Assign(entities.ExistingFcrSvesAddress);
        }
        else
        {
          export.Cl02.AddressLine1 = "Not Available";
        }

        if (ReadFcrSvesAddress2())
        {
          export.Do03.Assign(entities.ExistingFcrSvesAddress);
        }
        else
        {
          export.Do03.AddressLine1 = "Address not available";
          export.Do03.AddressLine2 = "Contact local SSA Office";
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

  private static void MoveFcrSvesTitleIiPend(FcrSvesTitleIiPend source,
    FcrSvesTitleIiPend target)
  {
    target.FirstNameText = source.FirstNameText;
    target.MiddleNameText = source.MiddleNameText;
    target.LastNameText = source.LastNameText;
    target.AdditionalFirstName1Text = source.AdditionalFirstName1Text;
    target.AdditionalMiddleName1Text = source.AdditionalMiddleName1Text;
    target.AdditionalLastName1Text = source.AdditionalLastName1Text;
    target.AdditionalFirstName2Text = source.AdditionalFirstName2Text;
    target.AdditionalMiddleName2Text = source.AdditionalMiddleName2Text;
    target.AdditionalLastName2Text = source.AdditionalLastName2Text;
    target.ResponseDate = source.ResponseDate;
    target.OtherSsn = source.OtherSsn;
    target.ClaimTypeCode = source.ClaimTypeCode;
  }

  private void UseCabFormatDateOnline()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedT2ResponseDat.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatSsnOnline1()
  {
    var useImport = new CabFormatSsnOnline.Import();
    var useExport = new CabFormatSsnOnline.Export();

    useImport.Ssn.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabFormatSsnOnline.Execute, useImport, useExport);

    export.FormattedOtherSsn.Text11 = useExport.FormattedSsn.Text11;
  }

  private void UseCabFormatSsnOnline2()
  {
    var useImport = new CabFormatSsnOnline.Import();
    var useExport = new CabFormatSsnOnline.Export();

    useImport.Ssn.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabFormatSsnOnline.Execute, useImport, useExport);

    export.FormattedVerifiedSsn.Text11 = useExport.FormattedSsn.Text11;
  }

  private void UseCabFormatSsnOnline3()
  {
    var useImport = new CabFormatSsnOnline.Import();
    var useExport = new CabFormatSsnOnline.Export();

    useImport.Ssn.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabFormatSsnOnline.Execute, useImport, useExport);

    export.FormattedPrimarySsn.Text11 = useExport.FormattedSsn.Text11;
  }

  private bool ReadFcrSvesAddress1()
  {
    entities.ExistingFcrSvesAddress.Populated = false;

    return Read("ReadFcrSvesAddress1",
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

  private bool ReadFcrSvesAddress2()
  {
    entities.ExistingFcrSvesAddress.Populated = false;

    return Read("ReadFcrSvesAddress2",
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
        entities.ExistingFcrSvesGenInfo.Ssn = db.GetNullableString(reader, 2);
        entities.ExistingFcrSvesGenInfo.FipsCountyCode =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesGenInfo.LocateResponseCode =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesGenInfo.ParticipantType =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesGenInfo.UserField =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesGenInfo.Populated = true;
      });
  }

  private bool ReadFcrSvesTitleIiPend()
  {
    entities.ExistingFcrSvesTitleIiPend.Populated = false;

    return Read("ReadFcrSvesTitleIiPend",
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
        entities.ExistingFcrSvesTitleIiPend.FcgMemberId =
          db.GetString(reader, 0);
        entities.ExistingFcrSvesTitleIiPend.FcgLSRspAgy =
          db.GetString(reader, 1);
        entities.ExistingFcrSvesTitleIiPend.SeqNo = db.GetInt32(reader, 2);
        entities.ExistingFcrSvesTitleIiPend.NameMatchedCode =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesTitleIiPend.ResponseDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingFcrSvesTitleIiPend.OtherSsn =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesTitleIiPend.SsnMatchCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesTitleIiPend.ClaimTypeCode =
          db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesTitleIiPend.CreatedBy = db.GetString(reader, 8);
        entities.ExistingFcrSvesTitleIiPend.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingFcrSvesTitleIiPend.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesTitleIiPend.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingFcrSvesTitleIiPend.FirstNameText =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrSvesTitleIiPend.MiddleNameText =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrSvesTitleIiPend.LastNameText =
          db.GetNullableString(reader, 14);
        entities.ExistingFcrSvesTitleIiPend.AdditionalFirstName1Text =
          db.GetNullableString(reader, 15);
        entities.ExistingFcrSvesTitleIiPend.AdditionalMiddleName1Text =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrSvesTitleIiPend.AdditionalLastName1Text =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrSvesTitleIiPend.AdditionalFirstName2Text =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrSvesTitleIiPend.AdditionalMiddleName2Text =
          db.GetNullableString(reader, 19);
        entities.ExistingFcrSvesTitleIiPend.AdditionalLastName2Text =
          db.GetNullableString(reader, 20);
        entities.ExistingFcrSvesTitleIiPend.Populated = true;
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
    /// A value of FormattedT2ResponseDat.
    /// </summary>
    [JsonPropertyName("formattedT2ResponseDat")]
    public WorkArea FormattedT2ResponseDat
    {
      get => formattedT2ResponseDat ??= new();
      set => formattedT2ResponseDat = value;
    }

    /// <summary>
    /// A value of FormattedFcrRecDate.
    /// </summary>
    [JsonPropertyName("formattedFcrRecDate")]
    public WorkArea FormattedFcrRecDate
    {
      get => formattedFcrRecDate ??= new();
      set => formattedFcrRecDate = value;
    }

    /// <summary>
    /// A value of FormattedOtherSsn.
    /// </summary>
    [JsonPropertyName("formattedOtherSsn")]
    public WorkArea FormattedOtherSsn
    {
      get => formattedOtherSsn ??= new();
      set => formattedOtherSsn = value;
    }

    /// <summary>
    /// A value of FormattedVerifiedSsn.
    /// </summary>
    [JsonPropertyName("formattedVerifiedSsn")]
    public WorkArea FormattedVerifiedSsn
    {
      get => formattedVerifiedSsn ??= new();
      set => formattedVerifiedSsn = value;
    }

    /// <summary>
    /// A value of FormattedPrimarySsn.
    /// </summary>
    [JsonPropertyName("formattedPrimarySsn")]
    public WorkArea FormattedPrimarySsn
    {
      get => formattedPrimarySsn ??= new();
      set => formattedPrimarySsn = value;
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
    /// A value of SvesErrorMsgs.
    /// </summary>
    [JsonPropertyName("svesErrorMsgs")]
    public WorkArea SvesErrorMsgs
    {
      get => svesErrorMsgs ??= new();
      set => svesErrorMsgs = value;
    }

    /// <summary>
    /// A value of Cl02.
    /// </summary>
    [JsonPropertyName("cl02")]
    public FcrSvesAddress Cl02
    {
      get => cl02 ??= new();
      set => cl02 = value;
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

    /// <summary>
    /// A value of FcrSvesTitleIiPend.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIiPend")]
    public FcrSvesTitleIiPend FcrSvesTitleIiPend
    {
      get => fcrSvesTitleIiPend ??= new();
      set => fcrSvesTitleIiPend = value;
    }

    private WorkArea formattedT2ResponseDat;
    private WorkArea formattedFcrRecDate;
    private WorkArea formattedOtherSsn;
    private WorkArea formattedVerifiedSsn;
    private WorkArea formattedPrimarySsn;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private WorkArea svesErrorMsgs;
    private FcrSvesAddress cl02;
    private FcrSvesAddress do03;
    private FcrSvesTitleIiPend fcrSvesTitleIiPend;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ExistingFcrSvesTitleIiPend.
    /// </summary>
    [JsonPropertyName("existingFcrSvesTitleIiPend")]
    public FcrSvesTitleIiPend ExistingFcrSvesTitleIiPend
    {
      get => existingFcrSvesTitleIiPend ??= new();
      set => existingFcrSvesTitleIiPend = value;
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
    private FcrSvesTitleIiPend existingFcrSvesTitleIiPend;
    private FcrSvesAddress existingFcrSvesAddress;
  }
#endregion
}
