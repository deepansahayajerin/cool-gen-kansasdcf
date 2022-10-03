// Program: SI_GET_DATA_INTERSTATE_AP_ID_DB, ID: 373008043, model: 746.
// Short name: SWE02736
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_GET_DATA_INTERSTATE_AP_ID_DB.
/// </summary>
[Serializable]
public partial class SiGetDataInterstateApIdDb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GET_DATA_INTERSTATE_AP_ID_DB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGetDataInterstateApIdDb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGetDataInterstateApIdDb.
  /// </summary>
  public SiGetDataInterstateApIdDb(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // ----------------------------------------------------------------------------
    // 06/25/2001	M Ramirez			Initial development
    // ----------------------------------------------------------------------------
    export.InterstateCase.ApIdentificationInd = 0;

    if (IsEmpty(import.Ap.Number))
    {
      ExitState = "SI0000_CSENET_AP_ID_ERROR_RB";

      return;
    }

    if (AsChar(import.Batch.Flag) == 'Y')
    {
      UseEabReadCsePersonBatch();
    }
    else
    {
      local.Current.Date = Now().Date;
      UseEabReadCsePerson();
    }

    if (!IsEmpty(local.AbendData.Type1))
    {
      switch(AsChar(local.AbendData.Type1))
      {
        case 'A':
          switch(TrimEnd(local.AbendData.AdabasResponseCd))
          {
            case "0113":
              ExitState = "ACO_ADABAS_PERSON_NF_113";

              break;
            case "0148":
              ExitState = "ACO_ADABAS_UNAVAILABLE";

              break;
            default:
              ExitState = "ADABAS_READ_UNSUCCESSFUL";

              break;
          }

          break;
        case 'C':
          ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

          break;
        default:
          ExitState = "ADABAS_INVALID_RETURN_CODE";

          break;
      }

      return;
    }

    if (ReadCsePerson())
    {
      if (Lt(local.Null1.Date, entities.CsePerson.DateOfDeath))
      {
        export.ApIsDeceased.Flag = "Y";
      }
    }
    else
    {
      // mjr
      // ----------------------------------------
      // 06/25/2001
      // Not an error:  the person may not be known to CSE
      // -----------------------------------------------------
    }

    UseCabFcrFormatNames();
    export.InterstateApIdentification.NameLast =
      local.CsePersonsWorkSet.LastName;
    export.InterstateApIdentification.NameFirst =
      local.CsePersonsWorkSet.FirstName;

    if (!IsEmpty(entities.CsePerson.NameMiddle))
    {
      export.InterstateApIdentification.MiddleName =
        entities.CsePerson.NameMiddle;
    }
    else
    {
      export.InterstateApIdentification.MiddleName =
        local.CsePersonsWorkSet.MiddleInitial;
    }

    export.InterstateApIdentification.Ssn = local.CsePersonsWorkSet.Ssn;
    export.InterstateApIdentification.DateOfBirth = local.CsePersonsWorkSet.Dob;
    export.InterstateApIdentification.Race = entities.CsePerson.Race;
    export.InterstateApIdentification.Sex = local.CsePersonsWorkSet.Sex;

    if (!IsEmpty(entities.CsePerson.BirthPlaceCity) && !
      IsEmpty(entities.CsePerson.BirthPlaceState))
    {
      export.InterstateApIdentification.PlaceOfBirth =
        TrimEnd(entities.CsePerson.BirthPlaceCity) + ", " + entities
        .CsePerson.BirthPlaceState;
    }
    else if (!IsEmpty(entities.CsePerson.BirthPlaceCity))
    {
      export.InterstateApIdentification.PlaceOfBirth =
        entities.CsePerson.BirthPlaceCity;
    }
    else if (!IsEmpty(entities.CsePerson.BirthPlaceState))
    {
      export.InterstateApIdentification.PlaceOfBirth =
        entities.CsePerson.BirthPlaceState;
    }
    else
    {
    }

    export.InterstateApIdentification.HeightFt = entities.CsePerson.HeightFt;
    export.InterstateApIdentification.HeightIn = entities.CsePerson.HeightIn;
    export.InterstateApIdentification.Weight = entities.CsePerson.Weight;
    export.InterstateApIdentification.HairColor = entities.CsePerson.HairColor;
    export.InterstateApIdentification.EyeColor = entities.CsePerson.EyeColor;
    UseCabRetrieveAliasesAndAltSsn();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    for(local.Ssns.Index = 0; local.Ssns.Index < local.Ssns.Count; ++
      local.Ssns.Index)
    {
      if (!local.Ssns.CheckSize())
      {
        break;
      }

      switch(local.Ssns.Index + 1)
      {
        case 1:
          export.InterstateApIdentification.AliasSsn1 =
            local.Ssns.Item.GlocalSsn.Ssn;

          break;
        case 2:
          export.InterstateApIdentification.AliasSsn2 =
            local.Ssns.Item.GlocalSsn.Ssn;

          break;
        default:
          goto AfterCycle;
      }
    }

AfterCycle:

    local.Ssns.CheckIndex();
    export.InterstateApIdentification.MaidenName =
      entities.CsePerson.NameMaiden;
    export.InterstateCase.ApIdentificationInd = 1;
  }

  private static void MoveAlternateSsnToSsns(CabRetrieveAliasesAndAltSsn.Export.
    AlternateSsnGroup source, Local.SsnsGroup target)
  {
    target.GlocalSsn.Ssn = source.Gssn.Ssn;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
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

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseCabFcrFormatNames()
  {
    var useImport = new CabFcrFormatNames.Import();
    var useExport = new CabFcrFormatNames.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(CabFcrFormatNames.Execute, useImport, useExport);

    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
      
  }

  private void UseCabRetrieveAliasesAndAltSsn()
  {
    var useImport = new CabRetrieveAliasesAndAltSsn.Import();
    var useExport = new CabRetrieveAliasesAndAltSsn.Export();

    useImport.Batch.Flag = import.Batch.Flag;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(CabRetrieveAliasesAndAltSsn.Execute, useImport, useExport);

    useExport.AlternateSsn.CopyTo(local.Ssns, MoveAlternateSsnToSsns);
  }

  private void UseEabReadCsePerson()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.Ap.Number;
    useImport.Current.Date = local.Current.Date;
    MoveCsePersonsWorkSet1(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
      
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = import.Ap.Number;
    useExport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 3);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 4);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 5);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 6);
        entities.CsePerson.Race = db.GetNullableString(reader, 7);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 8);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 9);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 10);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 11);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 12);
        entities.CsePerson.Populated = true;
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
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    private CsePersonsWorkSet ap;
    private Common batch;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    /// <summary>
    /// A value of ApIsDeceased.
    /// </summary>
    [JsonPropertyName("apIsDeceased")]
    public Common ApIsDeceased
    {
      get => apIsDeceased ??= new();
      set => apIsDeceased = value;
    }

    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private Common apIsDeceased;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A SsnsGroup group.</summary>
    [Serializable]
    public class SsnsGroup
    {
      /// <summary>
      /// A value of GlocalSsn.
      /// </summary>
      [JsonPropertyName("glocalSsn")]
      public CsePersonsWorkSet GlocalSsn
      {
        get => glocalSsn ??= new();
        set => glocalSsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet glocalSsn;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// Gets a value of Ssns.
    /// </summary>
    [JsonIgnore]
    public Array<SsnsGroup> Ssns => ssns ??= new(SsnsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ssns for json serialization.
    /// </summary>
    [JsonPropertyName("ssns")]
    [Computed]
    public IList<SsnsGroup> Ssns_Json
    {
      get => ssns;
      set => Ssns.Assign(value);
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private DateWorkArea null1;
    private Array<SsnsGroup> ssns;
    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
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
