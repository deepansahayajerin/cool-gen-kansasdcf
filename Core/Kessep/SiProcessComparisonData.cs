// Program: SI_PROCESS_COMPARISON_DATA, ID: 372515753, model: 746.
// Short name: SWE01193
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PROCESS_COMPARISON_DATA.
/// </para>
/// <para>
/// RESP:  SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiProcessComparisonData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PROCESS_COMPARISON_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiProcessComparisonData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiProcessComparisonData.
  /// </summary>
  public SiProcessComparisonData(IContext context, Import import, Export export):
    
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
    // 5-22-95  Ken Evans   Initial development
    // ---------------------------------------------
    // ****************************************************************
    // 4/22/99     C. Ott     Added call of an action block to create a Person
    //                        Driver's license when none exist.
    // ****************************************************************
    // ****************************************************************
    // 8/25/99     C. Ott     Added call of an action block to create a Person
    //                        alternate SSN when none exist.
    // ****************************************************************
    // ****************************************************************
    // 10/06/99     C. Ott    Removed termination of existing license and 
    // creation of new license.  Existing license record is update with new
    // state & number.  Problem report # 76435.
    // ****************************************************************
    // *********************************************
    // This AB will process the updates from both of
    // the SI comparison screens
    // *********************************************
    if (AsChar(import.UpdCsePersonInd.Flag) == 'Y')
    {
      UseSiUpdateCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (AsChar(import.UpdCsePersnWrkSetInd.Flag) == 'Y')
    {
      UseCabUpdateAdabasPerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (AsChar(import.UpdAlias1.Flag) == 'C')
    {
      MoveCsePersonsWorkSet(import.CsePersonsWorkSet, local.Alias);
      local.Alias.Ssn = import.Alias1.Ssn;
      local.Alias.UniqueKey = import.Alias1.UniqueKey;
      UseSiAltsCabCreateAlias();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (AsChar(import.UpdAlias1.Flag) == 'U')
    {
      MoveCsePersonsWorkSet(import.CsePersonsWorkSet, local.Alias);
      local.Alias.Ssn = import.Alias1.Ssn;
      local.Alias.UniqueKey = import.Alias1.UniqueKey;
      UseSiAltsCabUpdateAlias();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (AsChar(import.UpdAlias2.Flag) == 'C')
    {
      MoveCsePersonsWorkSet(import.CsePersonsWorkSet, local.Alias);
      local.Alias.Ssn = import.Alias2.Ssn;
      local.Alias.UniqueKey = import.Alias2.UniqueKey;
      UseSiAltsCabCreateAlias();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (AsChar(import.UpdAlias2.Flag) == 'U')
    {
      MoveCsePersonsWorkSet(import.CsePersonsWorkSet, local.Alias);
      local.Alias.Ssn = import.Alias2.Ssn;
      local.Alias.UniqueKey = import.Alias2.UniqueKey;
      UseSiAltsCabUpdateAlias();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (AsChar(import.UpdCsePersnLicenseInd.Flag) == 'Y')
    {
      if (import.Old.Identifier == 0)
      {
        // ****************************************************************
        // 4/22/99     C. Ott     Added call of the following action block to 
        // create a Person Driver's license when none exist.
        // ****************************************************************
        MoveCsePersonLicense(import.New1, local.CsePersonLicense);
        local.CsePersonLicense.Type1 = "D";
        local.CsePersonLicense.StartDt = Now().Date;
        local.CsePersonLicense.ExpirationDt = new DateTime(2099, 12, 31);
        UseSiCreateCsePersonLicense();
      }
      else
      {
        // ****************************************************************
        // 4/22/99     C. Ott     In this situation, when replacing Driver 
        // License data, the existing license should be terminated and a new
        // license occurrence created.
        // ****************************************************************
        // ****************************************************************
        // 10/06/99     C. Ott    Removed termination of existing license and 
        // creation of new license.  Existing license record is update with new
        // state & number.  Problem report # 76435.
        // ****************************************************************
        local.CsePersonLicense.Identifier = import.Old.Identifier;
        local.CsePersonLicense.IssuingState = import.New1.IssuingState ?? "";
        local.CsePersonLicense.Number = import.New1.Number ?? "";
        local.CsePersonLicense.StartDt = Now().Date;
        local.CsePersonLicense.ExpirationDt = new DateTime(2099, 12, 31);
        UseSiUpdateCsePersonLicense();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
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
    target.TextMessageIndicator = source.TextMessageIndicator;
  }

  private static void MoveCsePersonLicense(CsePersonLicense source,
    CsePersonLicense target)
  {
    target.Identifier = source.Identifier;
    target.Note = source.Note;
    target.IssuingState = source.IssuingState;
    target.IssuingAgencyName = source.IssuingAgencyName;
    target.Number = source.Number;
    target.Description = source.Description;
    target.ExpirationDt = source.ExpirationDt;
    target.StartDt = source.StartDt;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseCabUpdateAdabasPerson()
  {
    var useImport = new CabUpdateAdabasPerson.Import();
    var useExport = new CabUpdateAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    Call(CabUpdateAdabasPerson.Execute, useImport, useExport);
  }

  private void UseSiAltsCabCreateAlias()
  {
    var useImport = new SiAltsCabCreateAlias.Import();
    var useExport = new SiAltsCabCreateAlias.Export();

    useImport.CsePersonsWorkSet.Assign(local.Alias);

    Call(SiAltsCabCreateAlias.Execute, useImport, useExport);
  }

  private void UseSiAltsCabUpdateAlias()
  {
    var useImport = new SiAltsCabUpdateAlias.Import();
    var useExport = new SiAltsCabUpdateAlias.Export();

    useImport.CsePersonsWorkSet.Assign(local.Alias);

    Call(SiAltsCabUpdateAlias.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePersonLicense()
  {
    var useImport = new SiCreateCsePersonLicense.Import();
    var useExport = new SiCreateCsePersonLicense.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonLicense.Assign(local.CsePersonLicense);

    Call(SiCreateCsePersonLicense.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePerson()
  {
    var useImport = new SiUpdateCsePerson.Import();
    var useExport = new SiUpdateCsePerson.Export();

    MoveCsePerson(import.CsePerson, useImport.CsePerson);

    Call(SiUpdateCsePerson.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePersonLicense()
  {
    var useImport = new SiUpdateCsePersonLicense.Import();
    var useExport = new SiUpdateCsePersonLicense.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonLicense.Assign(local.CsePersonLicense);

    Call(SiUpdateCsePersonLicense.Execute, useImport, useExport);
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
    /// A value of UpdAlias2.
    /// </summary>
    [JsonPropertyName("updAlias2")]
    public Common UpdAlias2
    {
      get => updAlias2 ??= new();
      set => updAlias2 = value;
    }

    /// <summary>
    /// A value of UpdAlias1.
    /// </summary>
    [JsonPropertyName("updAlias1")]
    public Common UpdAlias1
    {
      get => updAlias1 ??= new();
      set => updAlias1 = value;
    }

    /// <summary>
    /// A value of Alias2.
    /// </summary>
    [JsonPropertyName("alias2")]
    public CsePersonsWorkSet Alias2
    {
      get => alias2 ??= new();
      set => alias2 = value;
    }

    /// <summary>
    /// A value of Alias1.
    /// </summary>
    [JsonPropertyName("alias1")]
    public CsePersonsWorkSet Alias1
    {
      get => alias1 ??= new();
      set => alias1 = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CsePersonLicense New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of UpdCsePersnLicenseInd.
    /// </summary>
    [JsonPropertyName("updCsePersnLicenseInd")]
    public Common UpdCsePersnLicenseInd
    {
      get => updCsePersnLicenseInd ??= new();
      set => updCsePersnLicenseInd = value;
    }

    /// <summary>
    /// A value of UpdIncmeSrceCntactInd.
    /// </summary>
    [JsonPropertyName("updIncmeSrceCntactInd")]
    public Common UpdIncmeSrceCntactInd
    {
      get => updIncmeSrceCntactInd ??= new();
      set => updIncmeSrceCntactInd = value;
    }

    /// <summary>
    /// A value of UpdCsePersonInd.
    /// </summary>
    [JsonPropertyName("updCsePersonInd")]
    public Common UpdCsePersonInd
    {
      get => updCsePersonInd ??= new();
      set => updCsePersonInd = value;
    }

    /// <summary>
    /// A value of UpdCsePersnWrkSetInd.
    /// </summary>
    [JsonPropertyName("updCsePersnWrkSetInd")]
    public Common UpdCsePersnWrkSetInd
    {
      get => updCsePersnWrkSetInd ??= new();
      set => updCsePersnWrkSetInd = value;
    }

    /// <summary>
    /// A value of ZdelimportCreateCsePersonLic.
    /// </summary>
    [JsonPropertyName("zdelimportCreateCsePersonLic")]
    public Common ZdelimportCreateCsePersonLic
    {
      get => zdelimportCreateCsePersonLic ??= new();
      set => zdelimportCreateCsePersonLic = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public CsePersonLicense Old
    {
      get => old ??= new();
      set => old = value;
    }

    private Common updAlias2;
    private Common updAlias1;
    private CsePersonsWorkSet alias2;
    private CsePersonsWorkSet alias1;
    private CsePersonLicense new1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Common updCsePersnLicenseInd;
    private Common updIncmeSrceCntactInd;
    private Common updCsePersonInd;
    private Common updCsePersnWrkSetInd;
    private Common zdelimportCreateCsePersonLic;
    private CsePersonLicense old;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private AbendData abendData;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MaxDisc.
    /// </summary>
    [JsonPropertyName("maxDisc")]
    public DateWorkArea MaxDisc
    {
      get => maxDisc ??= new();
      set => maxDisc = value;
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

    /// <summary>
    /// A value of Alias.
    /// </summary>
    [JsonPropertyName("alias")]
    public CsePersonsWorkSet Alias
    {
      get => alias ??= new();
      set => alias = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public CsePersonsWorkSet Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CsePersonsWorkSet New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private DateWorkArea maxDisc;
    private CsePersonLicense csePersonLicense;
    private CsePersonsWorkSet alias;
    private CsePersonsWorkSet old;
    private CsePersonsWorkSet new1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private CsePerson csePerson;
  }
#endregion
}
