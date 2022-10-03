// Program: SI_READ_CSE_PERSON_BATCH, ID: 371801928, model: 746.
// Short name: SWE01211
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_CSE_PERSON_BATCH.
/// </para>
/// <para>
/// Resp: SRVINIT	
/// This Common Action Block(CAB) is designed to read details for a CSE Person. 
/// Please note that the Calls in this CAB are designed for use by Batch
/// Process.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadCsePersonBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CSE_PERSON_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCsePersonBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCsePersonBatch.
  /// </summary>
  public SiReadCsePersonBatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************
    // *         M A I N T E N A N C E   L O G        *
    // * Date   Developer	Description
    // *
    // * 5-28-96 Tom Redmond	Initial Development
    // * 3-18-98 R Grey	Modify view matching and exit states.
    // 03/16/2000	M Ramirez	WR 000162	Added attributes to export cse_person
    // ************************************************
    if (ReadCsePerson())
    {
      export.CsePerson.Assign(entities.CsePerson);

      if (AsChar(entities.CsePerson.Type1) == 'C')
      {
        // ************************************************
        // *If the CSE Person is a Client (Type C)        *
        // ************************************************
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

        // ************************************************
        // *Call EAB to retrieve information about a CSE  *
        // *PERSON from the ADABAS system.                *
        // ************************************************
        UseCabReadAdabasPersonBatch();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ************************************************
        // *Format the name retrieved from the ADABAS     *
        // *files.
        // 
        // *
        // ************************************************
        UseSiFormatCsePersonName();
        export.CsePersonsWorkSet.FormattedName =
          local.CsePersonsWorkSet.FormattedName;
      }
      else if (AsChar(entities.CsePerson.Type1) == 'O')
      {
        // ************************************************
        // *If the CSE Person is an Organization (Type O) *
        // ************************************************
        export.CsePersonsWorkSet.FormattedName =
          entities.CsePerson.OrganizationName ?? Spaces(33);
        export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";
    }
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.ErrOnAdabasUnavailable.Flag = import.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    export.Ae.Flag = useExport.Ae.Flag;
    export.AbendData.Assign(useExport.AbendData);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
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
        entities.CsePerson.TaxId = db.GetNullableString(reader, 20);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 21);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 22);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 23);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 24);
        entities.CsePerson.CreatedBy = db.GetString(reader, 25);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 26);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 27);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 28);
        entities.CsePerson.KscaresNumber = db.GetNullableString(reader, 29);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 30);
        entities.CsePerson.EmergencyAreaCode = db.GetNullableInt32(reader, 31);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 32);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 33);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 34);
        entities.CsePerson.WorkPhoneExt = db.GetNullableString(reader, 35);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 36);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 37);
        entities.CsePerson.FederalInd = db.GetNullableString(reader, 38);
        entities.CsePerson.TaxIdSuffix = db.GetNullableString(reader, 39);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 40);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 41);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 42);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 43);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 44);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 45);
        entities.CsePerson.BirthCertFathersLastName =
          db.GetNullableString(reader, 46);
        entities.CsePerson.BirthCertFathersFirstName =
          db.GetNullableString(reader, 47);
        entities.CsePerson.BirthCertFathersMi =
          db.GetNullableString(reader, 48);
        entities.CsePerson.BirthCertificateSignature =
          db.GetNullableString(reader, 49);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 50);
        entities.CsePerson.BirthplaceCountry = db.GetNullableString(reader, 51);
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
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

    private Common errOnAdabasUnavailable;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePerson csePerson;
    private Common ae;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private CsePersonsWorkSet csePersonsWorkSet;
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
