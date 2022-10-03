// Program: CAB_READ_ADMIN_APPEAL_HEARING, ID: 372582881, model: 746.
// Short name: SWE00075
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_READ_ADMIN_APPEAL_HEARING.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads for a the Hearing for an Administrative Appeal.
/// </para>
/// </summary>
[Serializable]
public partial class CabReadAdminAppealHearing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_READ_ADMIN_APPEAL_HEARING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabReadAdminAppealHearing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabReadAdminAppealHearing.
  /// </summary>
  public CabReadAdminAppealHearing(IContext context, Import import,
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
    export.AdministrativeAppeal.Assign(import.AdministrativeAppeal);
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    UseCabReadAdminAppeal();

    if (ReadAdministrativeAppeal())
    {
      export.AdministrativeAppeal.Assign(entities.ExistingAdministrativeAppeal);
    }
    else
    {
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

      return;
    }

    // ************************************************
    // *Find the Current Hearing.                     *
    // ************************************************
    if (ReadHearing())
    {
      export.Hearing.Assign(entities.ExistingHearing);

      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadHearingAddress())
      {
        export.Export1.Update.HearingAddress.Assign(
          entities.ExistingHearingAddress);

        if (ReadFips())
        {
          export.Export1.Update.Detail.CountyDescription =
            entities.ExistingFips.CountyDescription;
        }

        export.Export1.Next();
      }

      return;
    }

    ExitState = "CO0000_HEARING_NF";
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveAdministrativeAppeal1(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
  }

  private static void MoveAdministrativeAppeal2(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
    target.Type1 = source.Type1;
  }

  private void UseCabReadAdminAppeal()
  {
    var useImport = new CabReadAdminAppeal.Import();
    var useExport = new CabReadAdminAppeal.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveAdministrativeAppeal1(export.AdministrativeAppeal,
      useImport.AdministrativeAppeal);

    Call(CabReadAdminAppeal.Execute, useImport, useExport);

    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AdmActTakenDate.Date = useExport.DateWorkArea.Date;
    MoveAdministrativeAppeal2(useExport.AdministrativeAppeal,
      export.AdministrativeAppeal);
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.ExistingAdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", export.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAdministrativeAppeal.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingAdministrativeAppeal.Number =
          db.GetNullableString(reader, 1);
        entities.ExistingAdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.ExistingAdministrativeAppeal.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation",
          export.Export1.Item.HearingAddress.StateProvince);
        db.SetNullableString(
          command, "countyAbbr", export.Export1.Item.HearingAddress.County ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.StateDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 4);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 5);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 6);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadHearing()
  {
    entities.ExistingHearing.Populated = false;

    return Read("ReadHearing",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "aapIdentifier", export.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingHearing.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingHearing.AapIdentifier = db.GetNullableInt32(reader, 1);
        entities.ExistingHearing.ConductedDate = db.GetDate(reader, 2);
        entities.ExistingHearing.ConductedTime = db.GetTimeSpan(reader, 3);
        entities.ExistingHearing.Type1 = db.GetNullableString(reader, 4);
        entities.ExistingHearing.LastName = db.GetString(reader, 5);
        entities.ExistingHearing.FirstName = db.GetString(reader, 6);
        entities.ExistingHearing.MiddleInt = db.GetNullableString(reader, 7);
        entities.ExistingHearing.Suffix = db.GetNullableString(reader, 8);
        entities.ExistingHearing.Title = db.GetNullableString(reader, 9);
        entities.ExistingHearing.OutcomeReceivedDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingHearing.CreatedBy = db.GetString(reader, 11);
        entities.ExistingHearing.CreatedTstamp = db.GetDateTime(reader, 12);
        entities.ExistingHearing.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.ExistingHearing.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 14);
        entities.ExistingHearing.Outcome = db.GetNullableString(reader, 15);
        entities.ExistingHearing.Populated = true;
      });
  }

  private IEnumerable<bool> ReadHearingAddress()
  {
    return ReadEach("ReadHearingAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "hrgGeneratedId",
          entities.ExistingHearing.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingHearingAddress.HrgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingHearingAddress.Type1 = db.GetString(reader, 1);
        entities.ExistingHearingAddress.Location =
          db.GetNullableString(reader, 2);
        entities.ExistingHearingAddress.Street1 = db.GetString(reader, 3);
        entities.ExistingHearingAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingHearingAddress.City = db.GetString(reader, 5);
        entities.ExistingHearingAddress.StateProvince = db.GetString(reader, 6);
        entities.ExistingHearingAddress.County =
          db.GetNullableString(reader, 7);
        entities.ExistingHearingAddress.ZipCode =
          db.GetNullableString(reader, 8);
        entities.ExistingHearingAddress.Zip4 = db.GetNullableString(reader, 9);
        entities.ExistingHearingAddress.Zip3 = db.GetNullableString(reader, 10);
        entities.ExistingHearingAddress.Populated = true;

        return true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAppeal administrativeAppeal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Fips Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailCountyPrompt.
      /// </summary>
      [JsonPropertyName("detailCountyPrompt")]
      public Common DetailCountyPrompt
      {
        get => detailCountyPrompt ??= new();
        set => detailCountyPrompt = value;
      }

      /// <summary>
      /// A value of DetailStatePrompt.
      /// </summary>
      [JsonPropertyName("detailStatePrompt")]
      public Common DetailStatePrompt
      {
        get => detailStatePrompt ??= new();
        set => detailStatePrompt = value;
      }

      /// <summary>
      /// A value of DetailAddrTypePrmpt.
      /// </summary>
      [JsonPropertyName("detailAddrTypePrmpt")]
      public Common DetailAddrTypePrmpt
      {
        get => detailAddrTypePrmpt ??= new();
        set => detailAddrTypePrmpt = value;
      }

      /// <summary>
      /// A value of DetailSelectRecord.
      /// </summary>
      [JsonPropertyName("detailSelectRecord")]
      public Common DetailSelectRecord
      {
        get => detailSelectRecord ??= new();
        set => detailSelectRecord = value;
      }

      /// <summary>
      /// A value of HearingAddress.
      /// </summary>
      [JsonPropertyName("hearingAddress")]
      public HearingAddress HearingAddress
      {
        get => hearingAddress ??= new();
        set => hearingAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Fips detail;
      private Common detailCountyPrompt;
      private Common detailStatePrompt;
      private Common detailAddrTypePrmpt;
      private Common detailSelectRecord;
      private HearingAddress hearingAddress;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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
    /// A value of AdmActTakenDate.
    /// </summary>
    [JsonPropertyName("admActTakenDate")]
    public DateWorkArea AdmActTakenDate
    {
      get => admActTakenDate ??= new();
      set => admActTakenDate = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    private AdministrativeAction administrativeAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea admActTakenDate;
    private Array<ExportGroup> export1;
    private AdministrativeAppeal administrativeAppeal;
    private Hearing hearing;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingHearingAddress.
    /// </summary>
    [JsonPropertyName("existingHearingAddress")]
    public HearingAddress ExistingHearingAddress
    {
      get => existingHearingAddress ??= new();
      set => existingHearingAddress = value;
    }

    /// <summary>
    /// A value of ExistingAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("existingAdministrativeAppeal")]
    public AdministrativeAppeal ExistingAdministrativeAppeal
    {
      get => existingAdministrativeAppeal ??= new();
      set => existingAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of ExistingHearing.
    /// </summary>
    [JsonPropertyName("existingHearing")]
    public Hearing ExistingHearing
    {
      get => existingHearing ??= new();
      set => existingHearing = value;
    }

    private Fips existingFips;
    private HearingAddress existingHearingAddress;
    private AdministrativeAppeal existingAdministrativeAppeal;
    private Hearing existingHearing;
  }
#endregion
}
