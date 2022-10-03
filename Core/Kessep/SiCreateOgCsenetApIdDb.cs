// Program: SI_CREATE_OG_CSENET_AP_ID_DB, ID: 372382227, model: 746.
// Short name: SWE01139
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_OG_CSENET_AP_ID_DB.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the outgoing AP Identification Data Block which contains 
/// information about an Interstate (CSENet) referral AP's physical
/// characteristics.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateOgCsenetApIdDb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_OG_CSENET_AP_ID_DB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateOgCsenetApIdDb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateOgCsenetApIdDb.
  /// </summary>
  public SiCreateOgCsenetApIdDb(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************************
    // 4/29/97		SHERAZ MALIK	CHANGE CURRENT_DATE
    // 2/16/99         Carl Ott        IDCR # 501; add three attributes
    // ********************************************************
    local.ApCsePersonsWorkSet.Number = import.Ap.Number;

    // ***   Call CAB to read the AP CSE_Person details   ***
    UseSiReadCsePerson();

    // -------------------------------------------
    // Populate the CSENET_AP_ID entity type with
    // the data returned from the CAB.
    // -------------------------------------------
    local.InterstateApIdentification.NameLast =
      local.ApCsePersonsWorkSet.LastName;
    local.InterstateApIdentification.NameFirst =
      local.ApCsePersonsWorkSet.FirstName;

    if (!IsEmpty(local.ApCsePerson.NameMiddle))
    {
      local.InterstateApIdentification.MiddleName =
        local.ApCsePerson.NameMiddle ?? "";
    }
    else
    {
      local.InterstateApIdentification.MiddleName =
        local.ApCsePersonsWorkSet.MiddleInitial;
    }

    local.InterstateApIdentification.NameSuffix = "";
    local.InterstateApIdentification.Ssn = local.ApCsePersonsWorkSet.Ssn;
    local.InterstateApIdentification.DateOfBirth =
      local.ApCsePersonsWorkSet.Dob;
    local.InterstateApIdentification.Race = local.ApCsePerson.Race ?? "";
    local.InterstateApIdentification.Sex = local.ApCsePersonsWorkSet.Sex;
    local.InterstateApIdentification.PlaceOfBirth =
      TrimEnd(local.ApCsePerson.BirthPlaceCity) + "," + " " + (
        local.ApCsePerson.BirthPlaceState ?? "");
    local.InterstateApIdentification.HeightFt =
      local.ApCsePerson.HeightFt.GetValueOrDefault();
    local.InterstateApIdentification.HeightIn =
      local.ApCsePerson.HeightIn.GetValueOrDefault();
    local.InterstateApIdentification.Weight =
      local.ApCsePerson.Weight.GetValueOrDefault();
    local.InterstateApIdentification.HairColor =
      local.ApCsePerson.HairColor ?? "";
    local.InterstateApIdentification.EyeColor = local.ApCsePerson.EyeColor ?? ""
      ;
    local.InterstateApIdentification.OtherIdInfo =
      local.ApCsePerson.OtherIdInfo ?? "";
    local.InterstateApIdentification.MaidenName =
      local.ApCsePerson.NameMaiden ?? "";

    // ***   Call CAB to get the AP's alias SSNs   ***
    UseSiAltsBuildAliasAndSsn();

    if (!local.ApAliasSsn.IsEmpty)
    {
      for(local.ApAliasSsn.Index = 0; local.ApAliasSsn.Index < local
        .ApAliasSsn.Count; ++local.ApAliasSsn.Index)
      {
        if (!local.ApAliasSsn.CheckSize())
        {
          break;
        }

        if (!Equal(local.ApAliasSsn.Item.GapCsePersonsWorkSet.Ssn,
          local.ApCsePersonsWorkSet.Ssn) && !
          IsEmpty(local.ApAliasSsn.Item.GapCsePersonsWorkSet.Ssn))
        {
          ++local.AliasSsn.Count;

          if (local.AliasSsn.Count == 1)
          {
            local.InterstateApIdentification.AliasSsn1 =
              local.ApAliasSsn.Item.GapCsePersonsWorkSet.Ssn;
          }
          else if (local.AliasSsn.Count == 2)
          {
            local.InterstateApIdentification.AliasSsn2 =
              local.ApAliasSsn.Item.GapCsePersonsWorkSet.Ssn;
          }
        }
      }

      local.ApAliasSsn.CheckIndex();
    }

    if (ReadInterstateCase())
    {
      try
      {
        CreateInterstateApIdentification();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_INTERSTATE_AP_IDENT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SI0000_INTERSTATE_AP_IDENT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "CSENET_CASE_NF";
    }
  }

  private static void MoveAp(SiAltsBuildAliasAndSsn.Export.ApGroup source,
    Local.ApAliasSsnGroup target)
  {
    target.GapCommon.SelectChar = source.GapCommon.SelectChar;
    target.GapCsePersonsWorkSet.Assign(source.GapCsePersonsWorkSet);
    target.GapSsn3.Text3 = source.GapSsn3.Text3;
    target.GapSsn2.Text3 = source.GapSsn2.Text3;
    target.GapSsn4.Text4 = source.GapSsn4.Text4;
    target.Temp1.Flag = source.GapKscares.Flag;
    target.Temp2.Flag = source.GapKanpay.Flag;
    target.Temp3.Flag = source.GapCse.Flag;
    target.Temp4.Flag = source.GapAe.Flag;
    target.Temp5.Flag = source.GapDbOccurrence.Flag;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
  }

  private void UseSiAltsBuildAliasAndSsn()
  {
    var useImport = new SiAltsBuildAliasAndSsn.Import();
    var useExport = new SiAltsBuildAliasAndSsn.Export();

    useImport.Ap1.Number = local.ApCsePersonsWorkSet.Number;

    Call(SiAltsBuildAliasAndSsn.Execute, useImport, useExport);

    useExport.Ap.CopyTo(local.ApAliasSsn, MoveAp);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.ApCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, local.ApCsePerson);
    local.ApCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void CreateInterstateApIdentification()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var aliasSsn2 = local.InterstateApIdentification.AliasSsn2 ?? "";
    var aliasSsn1 = local.InterstateApIdentification.AliasSsn1 ?? "";
    var otherIdInfo = local.InterstateApIdentification.OtherIdInfo ?? "";
    var eyeColor = local.InterstateApIdentification.EyeColor ?? "";
    var hairColor = local.InterstateApIdentification.HairColor ?? "";
    var weight = local.InterstateApIdentification.Weight.GetValueOrDefault();
    var heightIn =
      local.InterstateApIdentification.HeightIn.GetValueOrDefault();
    var heightFt =
      local.InterstateApIdentification.HeightFt.GetValueOrDefault();
    var placeOfBirth = local.InterstateApIdentification.PlaceOfBirth ?? "";
    var ssn = local.InterstateApIdentification.Ssn ?? "";
    var race = local.InterstateApIdentification.Race ?? "";
    var sex = local.InterstateApIdentification.Sex ?? "";
    var dateOfBirth = local.InterstateApIdentification.DateOfBirth;
    var nameSuffix = local.InterstateApIdentification.NameSuffix ?? "";
    var nameFirst = local.InterstateApIdentification.NameFirst;
    var nameLast = local.InterstateApIdentification.NameLast ?? "";
    var middleName = local.InterstateApIdentification.MiddleName ?? "";
    var possiblyDangerous =
      local.InterstateApIdentification.PossiblyDangerous ?? "";
    var maidenName = local.InterstateApIdentification.MaidenName ?? "";
    var mothersMaidenOrFathersName =
      local.InterstateApIdentification.MothersMaidenOrFathersName ?? "";

    entities.InterstateApIdentification.Populated = false;
    Update("CreateInterstateApIdentification",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetNullableString(command, "altSsn2", aliasSsn2);
        db.SetNullableString(command, "altSsn1", aliasSsn1);
        db.SetNullableString(command, "otherIdInfo", otherIdInfo);
        db.SetNullableString(command, "eyeColor", eyeColor);
        db.SetNullableString(command, "hairColor", hairColor);
        db.SetNullableInt32(command, "weight", weight);
        db.SetNullableInt32(command, "heightIn", heightIn);
        db.SetNullableInt32(command, "heightFt", heightFt);
        db.SetNullableString(command, "birthPlaceCity", placeOfBirth);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "race", race);
        db.SetNullableString(command, "sex", sex);
        db.SetNullableDate(command, "birthDate", dateOfBirth);
        db.SetNullableString(command, "suffix", nameSuffix);
        db.SetString(command, "nameFirst", nameFirst);
        db.SetNullableString(command, "nameLast", nameLast);
        db.SetNullableString(command, "middleName", middleName);
        db.SetNullableString(command, "possiblyDangerous", possiblyDangerous);
        db.SetNullableString(command, "maidenName", maidenName);
        db.SetNullableString(
          command, "mthMaidOrFathN", mothersMaidenOrFathersName);
      });

    entities.InterstateApIdentification.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateApIdentification.CcaTransSerNum = ccaTransSerNum;
    entities.InterstateApIdentification.AliasSsn2 = aliasSsn2;
    entities.InterstateApIdentification.AliasSsn1 = aliasSsn1;
    entities.InterstateApIdentification.OtherIdInfo = otherIdInfo;
    entities.InterstateApIdentification.EyeColor = eyeColor;
    entities.InterstateApIdentification.HairColor = hairColor;
    entities.InterstateApIdentification.Weight = weight;
    entities.InterstateApIdentification.HeightIn = heightIn;
    entities.InterstateApIdentification.HeightFt = heightFt;
    entities.InterstateApIdentification.PlaceOfBirth = placeOfBirth;
    entities.InterstateApIdentification.Ssn = ssn;
    entities.InterstateApIdentification.Race = race;
    entities.InterstateApIdentification.Sex = sex;
    entities.InterstateApIdentification.DateOfBirth = dateOfBirth;
    entities.InterstateApIdentification.NameSuffix = nameSuffix;
    entities.InterstateApIdentification.NameFirst = nameFirst;
    entities.InterstateApIdentification.NameLast = nameLast;
    entities.InterstateApIdentification.MiddleName = middleName;
    entities.InterstateApIdentification.PossiblyDangerous = possiblyDangerous;
    entities.InterstateApIdentification.MaidenName = maidenName;
    entities.InterstateApIdentification.MothersMaidenOrFathersName =
      mothersMaidenOrFathersName;
    entities.InterstateApIdentification.Populated = true;
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.Populated = true;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private CsePerson ap;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    private InterstateApIdentification interstateApIdentification;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ApAliasSsnGroup group.</summary>
    [Serializable]
    public class ApAliasSsnGroup
    {
      /// <summary>
      /// A value of GapCommon.
      /// </summary>
      [JsonPropertyName("gapCommon")]
      public Common GapCommon
      {
        get => gapCommon ??= new();
        set => gapCommon = value;
      }

      /// <summary>
      /// A value of GapCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gapCsePersonsWorkSet")]
      public CsePersonsWorkSet GapCsePersonsWorkSet
      {
        get => gapCsePersonsWorkSet ??= new();
        set => gapCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GapSsn3.
      /// </summary>
      [JsonPropertyName("gapSsn3")]
      public WorkArea GapSsn3
      {
        get => gapSsn3 ??= new();
        set => gapSsn3 = value;
      }

      /// <summary>
      /// A value of GapSsn2.
      /// </summary>
      [JsonPropertyName("gapSsn2")]
      public WorkArea GapSsn2
      {
        get => gapSsn2 ??= new();
        set => gapSsn2 = value;
      }

      /// <summary>
      /// A value of GapSsn4.
      /// </summary>
      [JsonPropertyName("gapSsn4")]
      public WorkArea GapSsn4
      {
        get => gapSsn4 ??= new();
        set => gapSsn4 = value;
      }

      /// <summary>
      /// A value of Temp1.
      /// </summary>
      [JsonPropertyName("temp1")]
      public Common Temp1
      {
        get => temp1 ??= new();
        set => temp1 = value;
      }

      /// <summary>
      /// A value of Temp2.
      /// </summary>
      [JsonPropertyName("temp2")]
      public Common Temp2
      {
        get => temp2 ??= new();
        set => temp2 = value;
      }

      /// <summary>
      /// A value of Temp3.
      /// </summary>
      [JsonPropertyName("temp3")]
      public Common Temp3
      {
        get => temp3 ??= new();
        set => temp3 = value;
      }

      /// <summary>
      /// A value of Temp4.
      /// </summary>
      [JsonPropertyName("temp4")]
      public Common Temp4
      {
        get => temp4 ??= new();
        set => temp4 = value;
      }

      /// <summary>
      /// A value of Temp5.
      /// </summary>
      [JsonPropertyName("temp5")]
      public Common Temp5
      {
        get => temp5 ??= new();
        set => temp5 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common gapCommon;
      private CsePersonsWorkSet gapCsePersonsWorkSet;
      private WorkArea gapSsn3;
      private WorkArea gapSsn2;
      private WorkArea gapSsn4;
      private Common temp1;
      private Common temp2;
      private Common temp3;
      private Common temp4;
      private Common temp5;
    }

    /// <summary>
    /// A value of AliasSsn.
    /// </summary>
    [JsonPropertyName("aliasSsn")]
    public Common AliasSsn
    {
      get => aliasSsn ??= new();
      set => aliasSsn = value;
    }

    /// <summary>
    /// Gets a value of ApAliasSsn.
    /// </summary>
    [JsonIgnore]
    public Array<ApAliasSsnGroup> ApAliasSsn => apAliasSsn ??= new(
      ApAliasSsnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ApAliasSsn for json serialization.
    /// </summary>
    [JsonPropertyName("apAliasSsn")]
    [Computed]
    public IList<ApAliasSsnGroup> ApAliasSsn_Json
    {
      get => apAliasSsn;
      set => ApAliasSsn.Assign(value);
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    private Common aliasSsn;
    private Array<ApAliasSsnGroup> apAliasSsn;
    private CsePerson apCsePerson;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private InterstateApIdentification interstateApIdentification;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ZdelAp.
    /// </summary>
    [JsonPropertyName("zdelAp")]
    public CsePerson ZdelAp
    {
      get => zdelAp ??= new();
      set => zdelAp = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    private CsePerson zdelAp;
    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
  }
#endregion
}
