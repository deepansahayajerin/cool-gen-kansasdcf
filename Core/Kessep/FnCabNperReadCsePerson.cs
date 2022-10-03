// Program: FN_CAB_NPER_READ_CSE_PERSON, ID: 372254686, model: 746.
// Short name: SWE02290
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_NPER_READ_CSE_PERSON.
/// </para>
/// <para>
/// RESP: FINANCE
/// This CAB reads the details of a CSE Person.  It is a copy of 
/// SI_READ_CSE_PERSON, with additional code added for non Case person
/// requirements.
/// </para>
/// </summary>
[Serializable]
public partial class FnCabNperReadCsePerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_NPER_READ_CSE_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabNperReadCsePerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabNperReadCsePerson.
  /// </summary>
  public FnCabNperReadCsePerson(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    //  Date     Developer		Description
    // 12/21/98 Maureen Brown
    // Dec 8, 1999, PR#80435 Maureen Brown - Change to allow update of adabas 
    // system flag to non-case for those persons who were case related before
    // conversion, and are now not on the DB2 table, but still exist in adabas
    // with a system flag of 'Y', which means case related.
    // : September, 2000, mbrown, pr#103286 - added ability to update a person 
    // to non-case on the system if they are known to another system, but not to
    // cse.  Before, we only allowed add of a non case person if they were not
    // known to any other system.
    // ---------------------------------------------------------
    if (!IsEmpty(import.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = import.CsePersonsWorkSet.Number;
    }
    else
    {
      export.CsePersonsWorkSet.Number = "";
      export.CsePerson.Number = "";
      ExitState = "CSE_PERSON_NF";

      return;
    }

    UseEabPadLeftWithZeros();
    export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    export.CsePerson.Number = local.TextWorkArea.Text10;

    if (ReadCsePerson())
    {
      export.CsePerson.Assign(entities.CsePerson);

      if (AsChar(entities.CsePerson.Type1) == 'C')
      {
        // ---------------------------------------------
        // Call EAB to retrieve information about a CSE
        // PERSON from ADABAS.
        // ---------------------------------------------
        UseCabReadAdabasPerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(local.Cse.Flag) == 'N')
        {
          // *** This is a non-case person, so no need to set the protect flag.
        }
        else
        {
          // : Any other flag value means that this is not a non-case person.
          export.Protect.Flag = "Y";
        }

        // ---------------------------------------------
        // Format the name retrieved from the ADABAS files.
        // ---------------------------------------------
        UseSiFormatCsePersonName();
        export.CsePersonsWorkSet.FormattedName =
          local.CsePersonsWorkSet.FormattedName;
      }
      else if (AsChar(entities.CsePerson.Type1) == 'O')
      {
        export.CsePersonsWorkSet.FormattedName =
          entities.CsePerson.OrganizationName ?? Spaces(33);
        export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      }
    }
    else
    {
      // Dec 8, 1999, PR#80435 - Check to see if the person is on adabas.  This 
      // may be a case where the person was known to cse, but was not converted.
      UseCabReadAdabasPerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }

      if (AsChar(local.Cse.Flag) == 'Y')
      {
        // Dec 8, 1999, PR#80435 - This person is on adabas, and is not on our 
        // database, and the system flag is "Y", which means known to CSE.
        // Therefore, this person can be converted to non case on adabas, and
        // added to the DB2 table.
        // : Any other flag value means that this is not a non-case person.
        export.Converted.Flag = "Y";
      }
      else if (AsChar(local.Cse.Flag) != 'N')
      {
        // : September, 2000, mbrown, pr#103286 - added ability to update a 
        // person to non-case on the system if they are known to another system,
        // but not to cse.  Before, we only allowed add of a non case person if
        // they were not known to any other system.
        export.Converted.Flag = "Y";
      }

      // ---------------------------------------------
      // Format the name retrieved from the ADABAS files.
      // ---------------------------------------------
      UseSiFormatCsePersonName();
      export.CsePersonsWorkSet.FormattedName =
        local.CsePersonsWorkSet.FormattedName;
    }
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = import.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.Cse.Flag = useExport.Cse.Flag;
    export.AbendData.Assign(useExport.AbendData);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
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
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 37);
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
    /// A value of SuppressZeroDob.
    /// </summary>
    [JsonPropertyName("suppressZeroDob")]
    public Common SuppressZeroDob
    {
      get => suppressZeroDob ??= new();
      set => suppressZeroDob = value;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    private Common suppressZeroDob;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common errOnAdabasUnavailable;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Converted.
    /// </summary>
    [JsonPropertyName("converted")]
    public Common Converted
    {
      get => converted ??= new();
      set => converted = value;
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
    /// A value of Protect.
    /// </summary>
    [JsonPropertyName("protect")]
    public Common Protect
    {
      get => protect ??= new();
      set => protect = value;
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

    private Common converted;
    private AbendData abendData;
    private Common protect;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private Common cse;
    private CsePersonsWorkSet csePersonsWorkSet;
    private TextWorkArea textWorkArea;
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
