// Program: OBTAIN_HEARING_DATE, ID: 372012087, model: 746.
// Short name: SWE00853
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OBTAIN_HEARING_DATE.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates a Legal HEARING associates it to LEGAL ACTION.
/// </para>
/// </summary>
[Serializable]
public partial class ObtainHearingDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OBTAIN_HEARING_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ObtainHearingDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ObtainHearingDate.
  /// </summary>
  public ObtainHearingDate(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 06/30/95	Dave Allen			Initial Code
    // 11/21/98	R. Jean			        Eliminate read of TRIBUNAL; changed TRIBUNAL 
    // qualifiers in subsequent reads
    // ------------------------------------------------------------
    // 9/28/17    JHarden    CQ58574   Allow a note to be added to the HEAR 
    // screen.
    MoveHearing(import.Hearing, export.Hearing);

    if (ReadLegalAction())
    {
      if (ReadHearing())
      {
        export.Hearing.Assign(entities.Hearing);
        ExitState = "CO0000_HEARING_AE_FOR_THIS_DATE";
      }
      else
      {
        try
        {
          CreateHearing();
          export.Hearing.Assign(entities.Hearing);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "HEARING_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        export.Addresses.Index = 0;
        export.Addresses.Clear();

        foreach(var item in ReadFipsTribAddress())
        {
          export.Addresses.Update.FipsTribAddress.Assign(
            entities.FipsTribAddress);
          export.Addresses.Next();
        }
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";
    }
  }

  private static void MoveHearing(Hearing source, Hearing target)
  {
    target.ConductedDate = source.ConductedDate;
    target.ConductedTime = source.ConductedTime;
    target.Type1 = source.Type1;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInt = source.MiddleInt;
    target.Suffix = source.Suffix;
    target.Title = source.Title;
    target.Outcome = source.Outcome;
    target.OutcomeReceivedDate = source.OutcomeReceivedDate;
    target.Note = source.Note;
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
    var lgaIdentifier = entities.LegalAction.Identifier;
    var conductedDate = import.Hearing.ConductedDate;
    var conductedTime = import.Hearing.ConductedTime;
    var type1 = import.Hearing.Type1 ?? "";
    var lastName = import.Hearing.LastName;
    var firstName = import.Hearing.FirstName;
    var middleInt = import.Hearing.MiddleInt ?? "";
    var suffix = import.Hearing.Suffix ?? "";
    var title = import.Hearing.Title ?? "";
    var outcomeReceivedDate = import.Hearing.OutcomeReceivedDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var outcome = import.Hearing.Outcome ?? "";
    var note = import.Hearing.Note ?? "";

    entities.Hearing.Populated = false;
    Update("CreateHearing",
      (db, command) =>
      {
        db.SetInt32(command, "hearingId", systemGeneratedIdentifier);
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
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
        db.SetNullableString(command, "note", note);
      });

    entities.Hearing.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Hearing.LgaIdentifier = lgaIdentifier;
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
    entities.Hearing.Note = note;
    entities.Hearing.Populated = true;
  }

  private IEnumerable<bool> ReadFipsTribAddress()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);

    return ReadEach("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Addresses.IsFull)
        {
          return false;
        }

        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 9);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 10);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 11);
        entities.FipsTribAddress.Populated = true;

        return true;
      });
  }

  private bool ReadHearing()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "hearingDt",
          import.Hearing.ConductedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
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
        entities.Hearing.Note = db.GetNullableString(reader, 14);
        entities.Hearing.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.LegalAction.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    private LegalAction legalAction;
    private Hearing hearing;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AddressesGroup group.</summary>
    [Serializable]
    public class AddressesGroup
    {
      /// <summary>
      /// A value of FipsTribAddress.
      /// </summary>
      [JsonPropertyName("fipsTribAddress")]
      public FipsTribAddress FipsTribAddress
      {
        get => fipsTribAddress ??= new();
        set => fipsTribAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FipsTribAddress fipsTribAddress;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// Gets a value of Addresses.
    /// </summary>
    [JsonIgnore]
    public Array<AddressesGroup> Addresses => addresses ??= new(
      AddressesGroup.Capacity);

    /// <summary>
    /// Gets a value of Addresses for json serialization.
    /// </summary>
    [JsonPropertyName("addresses")]
    [Computed]
    public IList<AddressesGroup> Addresses_Json
    {
      get => addresses;
      set => Addresses.Assign(value);
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

    private Tribunal tribunal;
    private Array<AddressesGroup> addresses;
    private Hearing hearing;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private LegalAction legalAction;
    private Hearing hearing;
  }
#endregion
}
