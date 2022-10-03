// Program: SI_RETRIEVE_AP_LOC_COMPARE_INFO, ID: 372541973, model: 746.
// Short name: SWE01236
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_RETRIEVE_AP_LOC_COMPARE_INFO.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiRetrieveApLocCompareInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RETRIEVE_AP_LOC_COMPARE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRetrieveApLocCompareInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRetrieveApLocCompareInfo.
  /// </summary>
  public SiRetrieveApLocCompareInfo(IContext context, Import import,
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
    // ---------------------------------------------
    //         M A I N T E N A N C E   L O G
    //  Date	 Developer   Description
    // 7-12-95  Ken Evans   Initial development
    // ---------------------------------------------
    // *****************************************************************
    // 4/23/99   C. Ott   Modified READ of CSE PERSON LICENSE to return only a 
    // current license.
    // ****************************************************************
    // **************************************************************
    // 10/06/99   C. Ott   Changed READ of CSE PERSON LICENSE to be consistent 
    // with how table is read in APDS screen.
    // **************************************************************
    // *********************************************
    // This AB retrieves the information needed
    // for the comparison.
    // *********************************************
    if (ReadCsePerson())
    {
      export.CsePerson.Assign(entities.CsePerson);
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // **************************************************************
    // 10/06/99   C. Ott   Changed READ of CSE PERSON LICENSE to be consistent 
    // with how table is read in APDS screen.
    // **************************************************************
    if (ReadCsePersonLicense())
    {
      export.CsePersonLicense.Assign(entities.CsePersonLicense);
    }

    UseSiReadCsenetApLocate();
  }

  private static void MoveInterstateApLocate(InterstateApLocate source,
    InterstateApLocate target)
  {
    target.HomePhoneNumber = source.HomePhoneNumber;
    target.WorkPhoneNumber = source.WorkPhoneNumber;
    target.DriversLicState = source.DriversLicState;
    target.DriversLicenseNum = source.DriversLicenseNum;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.CurrentSpouseMiddleName = source.CurrentSpouseMiddleName;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Occupation = source.Occupation;
    target.InsuranceCarrierName = source.InsuranceCarrierName;
    target.InsurancePolicyNum = source.InsurancePolicyNum;
    target.WorkAreaCode = source.WorkAreaCode;
    target.HomeAreaCode = source.HomeAreaCode;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private void UseSiReadCsenetApLocate()
  {
    var useImport = new SiReadCsenetApLocate.Import();
    var useExport = new SiReadCsenetApLocate.Export();

    MoveInterstateCase(import.InterstateCase, useImport.InterstateCase);

    Call(SiReadCsenetApLocate.Execute, useImport, useExport);

    MoveInterstateApLocate(useExport.InterstateApLocate,
      export.InterstateApLocate);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 20);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 21);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 22);
        entities.CsePerson.KscaresNumber = db.GetNullableString(reader, 23);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 24);
        entities.CsePerson.EmergencyAreaCode = db.GetNullableInt32(reader, 25);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 26);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 27);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 28);
        entities.CsePerson.WorkPhoneExt = db.GetNullableString(reader, 29);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 30);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 31);
        entities.CsePerson.TextMessageIndicator =
          db.GetNullableString(reader, 32);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return Read("ReadCsePersonLicense",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(command, "expirationDt", date);
      },
      (db, reader) =>
      {
        entities.CsePersonLicense.Identifier = db.GetInt32(reader, 0);
        entities.CsePersonLicense.CspNumber = db.GetString(reader, 1);
        entities.CsePersonLicense.IssuingState =
          db.GetNullableString(reader, 2);
        entities.CsePersonLicense.IssuingAgencyName =
          db.GetNullableString(reader, 3);
        entities.CsePersonLicense.Number = db.GetNullableString(reader, 4);
        entities.CsePersonLicense.ExpirationDt = db.GetNullableDate(reader, 5);
        entities.CsePersonLicense.StartDt = db.GetNullableDate(reader, 6);
        entities.CsePersonLicense.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonLicense.Description = db.GetNullableString(reader, 8);
        entities.CsePersonLicense.Note = db.GetNullableString(reader, 9);
        entities.CsePersonLicense.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePerson csePerson;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    private InterstateApLocate interstateApLocate;
    private CsePerson csePerson;
    private CsePersonLicense csePersonLicense;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CsePersonLicense csePersonLicense;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
