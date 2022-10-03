// Program: SI_ARDS_READ_AR_CASE_ROLE_DETAIL, ID: 371756980, model: 746.
// Short name: SWE01200
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_ARDS_READ_AR_CASE_ROLE_DETAIL.
/// </para>
/// <para>
/// Retrieves the information from a Case Role for an AR on this particular 
/// Case.
/// </para>
/// </summary>
[Serializable]
public partial class SiArdsReadArCaseRoleDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ARDS_READ_AR_CASE_ROLE_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiArdsReadArCaseRoleDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiArdsReadArCaseRoleDetail.
  /// </summary>
  public SiArdsReadArCaseRoleDetail(IContext context, Import import,
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
    // ------------------------------------------------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer     Request #     Description
    // --------    ------------  -----------	
    // --------------------------------------------------------------
    // 06/28/95    J Howard			Initial Development
    // 08/02/95    H Sharland MTW
    // 06/13/97    Sid				Cleanup and fixes.
    // 03/11/08    GVandy  	CQ296		Read and export CSE_Person Prior_TAF_Ind
    // 04/07/09    GVandy	CQ405		Performance fix
    // ------------------------------------------------------------------------------------------------------
    // 09/11/19  CQ66290  JHarden  add an indicator for threats made by DP or 
    // NCP on staff
    // 12/23/2020  GVandy  CQ68785  Add customer service code.
    // ---------------------------------------------
    // This process retrieves the details about a
    // CSE-Person and the Case Role that they hold on a specific Case.
    // ---------------------------------------------
    UseCabSetMaximumDiscontinueDate();
    local.ErrOnAdabasUnavailable.Flag = "Y";
    local.SuppressZeroDob.Flag = "Y";
    UseSiReadCsePerson();

    if (!IsEmpty(export.AbendData.Type1))
    {
      return;
    }

    // WR 020205 - Add POB Country
    if (ReadCsePerson())
    {
      export.CsePerson.BirthplaceCountry = entities.CsePerson.BirthplaceCountry;
      export.CsePerson.PriorTafInd = entities.CsePerson.PriorTafInd;
      export.CsePerson.TribalCode = entities.CsePerson.TribalCode;
      export.CsePerson.ThreatOnStaff = entities.CsePerson.ThreatOnStaff;
      export.CsePerson.CustomerServiceCode =
        entities.CsePerson.CustomerServiceCode;
    }

    // 04/07/09 GVandy  CQ405  Performance fix.  Restructure so that join uses 
    // index CKI01505 on CASE_ROLE table.
    if (ReadCaseCaseRole())
    {
      export.Ar.Assign(entities.Ar);
      MoveCase1(entities.Case1, export.Case1);

      if (Equal(entities.Case1.AdcCloseDate, local.Max.Date))
      {
        export.Case1.AdcCloseDate = null;
      }
    }
    else
    {
      // ---------------------------------------------
      // Read Case first to determine whether it is the Case that is not found 
      // or the Case Role is missing.
      // ---------------------------------------------
      if (ReadCase())
      {
        if (ReadCsePerson())
        {
          ExitState = "CASE_ROLE_NF";
        }
        else
        {
          ExitState = "CSE_PERSON_NF";
        }
      }
      else
      {
        ExitState = "CASE_NF";
      }
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.ExpeditedPaternityInd = source.ExpeditedPaternityInd;
    target.FullServiceWithoutMedInd = source.FullServiceWithoutMedInd;
    target.FullServiceWithMedInd = source.FullServiceWithMedInd;
    target.LocateInd = source.LocateInd;
    target.ClosureReason = source.ClosureReason;
    target.Status = source.Status;
    target.StatusDate = source.StatusDate;
    target.CseOpenDate = source.CseOpenDate;
    target.AdcOpenDate = source.AdcOpenDate;
    target.AdcCloseDate = source.AdcCloseDate;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Number = source.Number;
    target.KscaresNumber = source.KscaresNumber;
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
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.EmergencyAreaCode = source.EmergencyAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.BirthplaceCountry = source.BirthplaceCountry;
    target.TextMessageIndicator = source.TextMessageIndicator;
    target.TaxId = source.TaxId;
    target.OrganizationName = source.OrganizationName;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.SuppressZeroDob.Flag = local.SuppressZeroDob.Flag;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.Ae.Flag = useExport.Ae.Flag;
    export.AbendData.Assign(useExport.AbendData);
    MoveCsePerson(useExport.CsePerson, export.CsePerson);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.FullServiceWithoutMedInd =
          db.GetNullableString(reader, 0);
        entities.Case1.FullServiceWithMedInd = db.GetNullableString(reader, 1);
        entities.Case1.LocateInd = db.GetNullableString(reader, 2);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 3);
        entities.Case1.Number = db.GetString(reader, 4);
        entities.Case1.Status = db.GetNullableString(reader, 5);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 6);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 7);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 8);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 9);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 10);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseCaseRole()
  {
    entities.Case1.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
        db.SetString(command, "number", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Case1.FullServiceWithoutMedInd =
          db.GetNullableString(reader, 0);
        entities.Case1.FullServiceWithMedInd = db.GetNullableString(reader, 1);
        entities.Case1.LocateInd = db.GetNullableString(reader, 2);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 3);
        entities.Case1.Number = db.GetString(reader, 4);
        entities.Ar.CasNumber = db.GetString(reader, 4);
        entities.Case1.Status = db.GetNullableString(reader, 5);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 6);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 7);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 8);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 9);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 10);
        entities.Ar.CspNumber = db.GetString(reader, 11);
        entities.Ar.Type1 = db.GetString(reader, 12);
        entities.Ar.Identifier = db.GetInt32(reader, 13);
        entities.Ar.StartDate = db.GetNullableDate(reader, 14);
        entities.Ar.EndDate = db.GetNullableDate(reader, 15);
        entities.Ar.OnSsInd = db.GetNullableString(reader, 16);
        entities.Ar.HealthInsuranceIndicator = db.GetNullableString(reader, 17);
        entities.Ar.MedicalSupportIndicator = db.GetNullableString(reader, 18);
        entities.Ar.ContactFirstName = db.GetNullableString(reader, 19);
        entities.Ar.ContactMiddleInitial = db.GetNullableString(reader, 20);
        entities.Ar.ContactPhone = db.GetNullableString(reader, 21);
        entities.Ar.ContactLastName = db.GetNullableString(reader, 22);
        entities.Ar.ChildCareExpenses = db.GetNullableDecimal(reader, 23);
        entities.Ar.AssignmentDate = db.GetNullableDate(reader, 24);
        entities.Ar.AssignmentTerminationCode =
          db.GetNullableString(reader, 25);
        entities.Ar.AssignmentOfRights = db.GetNullableString(reader, 26);
        entities.Ar.AssignmentTerminatedDt = db.GetNullableDate(reader, 27);
        entities.Ar.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 28);
        entities.Ar.LastUpdatedBy = db.GetNullableString(reader, 29);
        entities.Ar.CreatedTimestamp = db.GetDateTime(reader, 30);
        entities.Ar.CreatedBy = db.GetString(reader, 31);
        entities.Ar.ArChgProcReqInd = db.GetNullableString(reader, 32);
        entities.Ar.ArChgProcessedDate = db.GetNullableDate(reader, 33);
        entities.Ar.ArInvalidInd = db.GetNullableString(reader, 34);
        entities.Ar.Note = db.GetNullableString(reader, 35);
        entities.Case1.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ar.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
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
        entities.CsePerson.BirthplaceCountry = db.GetNullableString(reader, 32);
        entities.CsePerson.PriorTafInd = db.GetNullableString(reader, 33);
        entities.CsePerson.TribalCode = db.GetNullableString(reader, 34);
        entities.CsePerson.ThreatOnStaff = db.GetNullableString(reader, 35);
        entities.CsePerson.CustomerServiceCode =
          db.GetNullableString(reader, 36);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Case1 case1;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Case1 case1;
    private Common ae;
    private AbendData abendData;
    private CsePerson csePerson;
    private CaseRole ar;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of SuppressZeroDob.
    /// </summary>
    [JsonPropertyName("suppressZeroDob")]
    public Common SuppressZeroDob
    {
      get => suppressZeroDob ??= new();
      set => suppressZeroDob = value;
    }

    private DateWorkArea max;
    private Common errOnAdabasUnavailable;
    private Common suppressZeroDob;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IncomeSourceContact.
    /// </summary>
    [JsonPropertyName("incomeSourceContact")]
    public IncomeSourceContact IncomeSourceContact
    {
      get => incomeSourceContact ??= new();
      set => incomeSourceContact = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private IncomeSourceContact incomeSourceContact;
    private Case1 case1;
    private CsePerson csePerson;
    private CaseRole ar;
  }
#endregion
}
