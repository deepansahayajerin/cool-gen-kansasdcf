// Program: INFORM_RELATED_PARTY_OF_HEAR, ID: 372582884, model: 746.
// Short name: SWE00721
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: INFORM_RELATED_PARTY_OF_HEAR.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process updates ADMINISTRATIVE APPEAL and HEARING.
/// </para>
/// </summary>
[Serializable]
public partial class InformRelatedPartyOfHear: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the INFORM_RELATED_PARTY_OF_HEAR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new InformRelatedPartyOfHear(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of InformRelatedPartyOfHear.
  /// </summary>
  public InformRelatedPartyOfHear(IContext context, Import import, Export export)
    :
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
    // Date	By	IDCR#	Description
    // ??????	??????		Initial code
    // 102597	govind		Modified to use random number for identifier
    // ---------------------------------------------
    export.AdministrativeAppeal.Assign(import.AdministrativeAppeal);

    if (ReadAdministrativeAppeal())
    {
      export.AdministrativeAppeal.Assign(entities.AdministrativeAppeal);
    }
    else
    {
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

      return;
    }

    // ---------------------------------------------
    // Only one hearing will exist for one admin appeal as per users. However 
    // data model provides multtiple hearings. If multiple hearings are
    // possible, then we need either a list screen or prev/next  keys to view
    // the other hearings on the same appeal.
    // ---------------------------------------------
    if (ReadHearing())
    {
      ExitState = "HEARING_AE";

      return;
    }

    for(local.NoOfRetries.Count = 1; local.NoOfRetries.Count <= 10; ++
      local.NoOfRetries.Count)
    {
      try
      {
        CreateHearing();
        export.Hearing.Assign(entities.Hearing);

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            continue;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateHearing()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var aapIdentifier = entities.AdministrativeAppeal.Identifier;
    var conductedDate = import.Hearing.ConductedDate;
    var conductedTime = import.Hearing.ConductedTime;
    var type1 = "A";
    var lastName = import.Hearing.LastName;
    var firstName = import.Hearing.FirstName;
    var middleInt = import.Hearing.MiddleInt ?? "";
    var suffix = import.Hearing.Suffix ?? "";
    var title = import.Hearing.Title ?? "";
    var outcomeReceivedDate = import.Hearing.OutcomeReceivedDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var outcome = import.Hearing.Outcome ?? "";

    entities.Hearing.Populated = false;
    Update("CreateHearing",
      (db, command) =>
      {
        db.SetInt32(command, "hearingId", systemGeneratedIdentifier);
        db.SetNullableInt32(command, "aapIdentifier", aapIdentifier);
        db.SetDate(command, "hearingDt", conductedDate);
        db.SetTimeSpan(command, "hearingTime", conductedTime);
        db.SetNullableString(command, "type", type1);
        db.SetString(command, "lastNm", lastName);
        db.SetString(command, "firstNm", firstName);
        db.SetNullableString(command, "middleInt", middleInt);
        db.SetNullableString(command, "suffix", suffix);
        db.SetNullableString(command, "title", title);
        db.SetNullableDate(command, "outcomeReceiveDt", outcomeReceivedDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetNullableString(command, "outcome", outcome);
        db.SetNullableString(command, "note", "");
      });

    entities.Hearing.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Hearing.AapIdentifier = aapIdentifier;
    entities.Hearing.ConductedDate = conductedDate;
    entities.Hearing.ConductedTime = conductedTime;
    entities.Hearing.Type1 = type1;
    entities.Hearing.LastName = lastName;
    entities.Hearing.FirstName = firstName;
    entities.Hearing.MiddleInt = middleInt;
    entities.Hearing.Suffix = suffix;
    entities.Hearing.Title = title;
    entities.Hearing.OutcomeReceivedDate = outcomeReceivedDate;
    entities.Hearing.CreatedBy = createdBy;
    entities.Hearing.CreatedTstamp = createdTstamp;
    entities.Hearing.Outcome = outcome;
    entities.Hearing.Populated = true;
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", import.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.Populated = true;
      });
  }

  private bool ReadHearing()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "aapIdentifier", entities.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.AapIdentifier = db.GetNullableInt32(reader, 1);
        entities.Hearing.ConductedDate = db.GetDate(reader, 2);
        entities.Hearing.ConductedTime = db.GetTimeSpan(reader, 3);
        entities.Hearing.Type1 = db.GetNullableString(reader, 4);
        entities.Hearing.LastName = db.GetString(reader, 5);
        entities.Hearing.FirstName = db.GetString(reader, 6);
        entities.Hearing.MiddleInt = db.GetNullableString(reader, 7);
        entities.Hearing.Suffix = db.GetNullableString(reader, 8);
        entities.Hearing.Title = db.GetNullableString(reader, 9);
        entities.Hearing.OutcomeReceivedDate = db.GetNullableDate(reader, 10);
        entities.Hearing.CreatedBy = db.GetString(reader, 11);
        entities.Hearing.CreatedTstamp = db.GetDateTime(reader, 12);
        entities.Hearing.Outcome = db.GetNullableString(reader, 13);
        entities.Hearing.Populated = true;
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

    private AdministrativeAppeal administrativeAppeal;
    private Hearing hearing;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    private AdministrativeAppeal administrativeAppeal;
    private Hearing hearing;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NoOfRetries.
    /// </summary>
    [JsonPropertyName("noOfRetries")]
    public Common NoOfRetries
    {
      get => noOfRetries ??= new();
      set => noOfRetries = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Hearing Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
    }

    private Common noOfRetries;
    private Hearing last;
    private SystemGenerated systemGenerated;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public Hearing ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
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

    private Hearing existingLast;
    private HearingAddress hearingAddress;
    private AdministrativeAppeal administrativeAppeal;
    private Hearing hearing;
  }
#endregion
}
