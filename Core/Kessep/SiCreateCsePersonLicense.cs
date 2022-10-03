// Program: SI_CREATE_CSE_PERSON_LICENSE, ID: 371755707, model: 746.
// Short name: SWE01125
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_CSE_PERSON_LICENSE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This will create an occurrance of a professional license held by a cse 
/// person		
/// e.g.  Driver's license	
///       Lawyer's license		
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateCsePersonLicense: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_CSE_PERSON_LICENSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateCsePersonLicense(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateCsePersonLicense.
  /// </summary>
  public SiCreateCsePersonLicense(IContext context, Import import, Export export)
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
    //                            of a READ statement to
    //                            Select Only.
    // ---------------------------------------------------------
    // 10/06/99 W.Campbell        Added view matching for
    //                            the import view for a USE
    //                            statement.  This was done
    //                            after the USE'd CAB
    //                            SI_GEN_CSE_PERSON_LICENSE_ID
    //                            was modified to accept an import view.
    //                            This work was done on PR# H00076435.
    // ---------------------------------------------------------
    export.CsePersonLicense.Assign(import.CsePersonLicense);

    // ---------------------------------------------------------
    // 06/21/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // Call the action block that creates the unique
    // identifier for this license.
    // ---------------------------------------------
    // ---------------------------------------------------------
    // 10/06/99 W.Campbell - Added view matching for
    // the import view for the following
    // USE statement.  This was done
    // after the USE'd CAB was modified
    // to accept an import view.  This work
    // was done on PR# H00076435.
    // ---------------------------------------------------------
    UseSiGenCsePersonLicenseId();
    export.CsePersonLicense.Identifier = local.CsePersonLicense.Identifier;

    try
    {
      CreateCsePersonLicense();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CSE_PERSON_LICENSE_AE";

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

  private void UseSiGenCsePersonLicenseId()
  {
    var useImport = new SiGenCsePersonLicenseId.Import();
    var useExport = new SiGenCsePersonLicenseId.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(SiGenCsePersonLicenseId.Execute, useImport, useExport);

    local.CsePersonLicense.Identifier = useExport.CsePersonLicense.Identifier;
  }

  private void CreateCsePersonLicense()
  {
    var identifier = local.CsePersonLicense.Identifier;
    var cspNumber = entities.CsePerson.Number;
    var issuingState = import.CsePersonLicense.IssuingState ?? "";
    var issuingAgencyName = import.CsePersonLicense.IssuingAgencyName ?? "";
    var number = import.CsePersonLicense.Number ?? "";
    var expirationDt = import.CsePersonLicense.ExpirationDt;
    var startDt = import.CsePersonLicense.StartDt;
    var type1 = import.CsePersonLicense.Type1 ?? "";
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var description = import.CsePersonLicense.Description ?? "";
    var note = import.CsePersonLicense.Note ?? "";

    entities.CsePersonLicense.Populated = false;
    Update("CreateCsePersonLicense",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "issuingState", issuingState);
        db.SetNullableString(command, "issuingAgencyNm", issuingAgencyName);
        db.SetNullableString(command, "numb", number);
        db.SetNullableDate(command, "expirationDt", expirationDt);
        db.SetNullableDate(command, "startDt", startDt);
        db.SetNullableString(command, "type", type1);
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "description", description);
        db.SetNullableString(command, "note", note);
      });

    entities.CsePersonLicense.Identifier = identifier;
    entities.CsePersonLicense.CspNumber = cspNumber;
    entities.CsePersonLicense.IssuingState = issuingState;
    entities.CsePersonLicense.IssuingAgencyName = issuingAgencyName;
    entities.CsePersonLicense.Number = number;
    entities.CsePersonLicense.ExpirationDt = expirationDt;
    entities.CsePersonLicense.StartDt = startDt;
    entities.CsePersonLicense.Type1 = type1;
    entities.CsePersonLicense.CreatedTimestamp = createdTimestamp;
    entities.CsePersonLicense.CreatedBy = createdBy;
    entities.CsePersonLicense.Description = description;
    entities.CsePersonLicense.Note = note;
    entities.CsePersonLicense.Populated = true;
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
