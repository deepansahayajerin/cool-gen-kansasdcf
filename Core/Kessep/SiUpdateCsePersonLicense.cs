// Program: SI_UPDATE_CSE_PERSON_LICENSE, ID: 371755705, model: 746.
// Short name: SWE01248
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_UPDATE_CSE_PERSON_LICENSE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This will UPDATE an occurrance of a professional license held by a cse 
/// person		
/// e.g.  Driver's license	
///       Lawyer's license		
/// </para>
/// </summary>
[Serializable]
public partial class SiUpdateCsePersonLicense: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_CSE_PERSON_LICENSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateCsePersonLicense(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateCsePersonLicense.
  /// </summary>
  public SiUpdateCsePersonLicense(IContext context, Import import, Export export)
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
    // ---------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    //  Date	   Developer		Request #	Description
    // 4-09-95	  Helen Sharland - MTW		0	Initial Development
    // ---------------------------------------------------------
    // 06/21/99 W.Campbell        Modified the properties
    //                            of 2 READ statements to
    //                            Select Only.
    // ---------------------------------------------------------
    // ---------------------------------------------------------
    // 06/21/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCsePersonLicense())
    {
      // ---------------------------------------------------------
      // 06/21/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCsePerson())
      {
        ExitState = "CSE_PERSON_LICENSE_NF";

        return;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    try
    {
      UpdateCsePersonLicense();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CSE_PERSON_LICENSE_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CSE_PERSON_LICENSE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
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
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return Read("ReadCsePersonLicense",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.CsePersonLicense.Identifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
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
        entities.CsePersonLicense.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CsePersonLicense.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.CsePersonLicense.Description =
          db.GetNullableString(reader, 10);
        entities.CsePersonLicense.Note = db.GetNullableString(reader, 11);
        entities.CsePersonLicense.Populated = true;
      });
  }

  private void UpdateCsePersonLicense()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonLicense.Populated);

    var issuingState = import.CsePersonLicense.IssuingState ?? "";
    var issuingAgencyName = import.CsePersonLicense.IssuingAgencyName ?? "";
    var number = import.CsePersonLicense.Number ?? "";
    var expirationDt = import.CsePersonLicense.ExpirationDt;
    var startDt = import.CsePersonLicense.StartDt;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var description = import.CsePersonLicense.Description ?? "";
    var note = import.CsePersonLicense.Note ?? "";

    entities.CsePersonLicense.Populated = false;
    Update("UpdateCsePersonLicense",
      (db, command) =>
      {
        db.SetNullableString(command, "issuingState", issuingState);
        db.SetNullableString(command, "issuingAgencyNm", issuingAgencyName);
        db.SetNullableString(command, "numb", number);
        db.SetNullableDate(command, "expirationDt", expirationDt);
        db.SetNullableDate(command, "startDt", startDt);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "description", description);
        db.SetNullableString(command, "note", note);
        db.
          SetInt32(command, "identifier", entities.CsePersonLicense.Identifier);
          
        db.SetString(command, "cspNumber", entities.CsePersonLicense.CspNumber);
      });

    entities.CsePersonLicense.IssuingState = issuingState;
    entities.CsePersonLicense.IssuingAgencyName = issuingAgencyName;
    entities.CsePersonLicense.Number = number;
    entities.CsePersonLicense.ExpirationDt = expirationDt;
    entities.CsePersonLicense.StartDt = startDt;
    entities.CsePersonLicense.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePersonLicense.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonLicense.Description = description;
    entities.CsePersonLicense.Note = note;
    entities.CsePersonLicense.Populated = true;
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

    private CsePerson csePerson;
    private CsePersonLicense csePersonLicense;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private CsePersonLicense csePersonLicense;
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

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    private CsePerson csePerson;
    private CsePersonLicense csePersonLicense;
  }
#endregion
}
