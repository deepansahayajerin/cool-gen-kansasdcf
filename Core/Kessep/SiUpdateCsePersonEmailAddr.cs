// Program: SI_UPDATE_CSE_PERSON_EMAIL_ADDR, ID: 1902521498, model: 746.
// Short name: SWE03751
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_UPDATE_CSE_PERSON_EMAIL_ADDR.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD creates an address for a given CSE Person
/// </para>
/// </summary>
[Serializable]
public partial class SiUpdateCsePersonEmailAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_CSE_PERSON_EMAIL_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateCsePersonEmailAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateCsePersonEmailAddr.
  /// </summary>
  public SiUpdateCsePersonEmailAddr(IContext context, Import import,
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
    // ------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    // Date	   Developer	Description
    // 02/01/2016 D Dupree     Initial Development
    // ------------------------------------------------------------
    // ---------------------------------------------
    // This PAD updates an email address for a CSE Person.
    // ---------------------------------------------
    local.Current.Timestamp = Now();

    if (Equal(import.CsePersonEmailAddress.EndDate, null))
    {
      local.CsePersonEmailAddress.EndDate = UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.CsePersonEmailAddress.EndDate =
        import.CsePersonEmailAddress.EndDate;
    }

    if (ReadCsePersonEmailAddress())
    {
      try
      {
        UpdateCsePersonEmailAddress();
        MoveCsePersonEmailAddress(entities.CsePersonEmailAddress,
          export.CsePersonEmailAddress);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CSE_PERSON_EMAIL_ADDR_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CSE_PERSON_EMAIL_ADDR_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "CSE_PERSON_EMAIL_ADDRS_NF";
    }
  }

  private static void MoveCsePersonEmailAddress(CsePersonEmailAddress source,
    CsePersonEmailAddress target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadCsePersonEmailAddress()
  {
    entities.CsePersonEmailAddress.Populated = false;

    return Read("ReadCsePersonEmailAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.CsePersonEmailAddress.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonEmailAddress.CspNumber = db.GetString(reader, 0);
        entities.CsePersonEmailAddress.EffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.CsePersonEmailAddress.EndDate = db.GetNullableDate(reader, 2);
        entities.CsePersonEmailAddress.EmailSource =
          db.GetNullableString(reader, 3);
        entities.CsePersonEmailAddress.CreatedBy = db.GetString(reader, 4);
        entities.CsePersonEmailAddress.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CsePersonEmailAddress.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.CsePersonEmailAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CsePersonEmailAddress.EmailAddress =
          db.GetNullableString(reader, 8);
        entities.CsePersonEmailAddress.Populated = true;
      });
  }

  private void UpdateCsePersonEmailAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonEmailAddress.Populated);

    var effectiveDate = import.CsePersonEmailAddress.EffectiveDate;
    var endDate = local.CsePersonEmailAddress.EndDate;
    var emailSource = import.CsePersonEmailAddress.EmailSource ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var emailAddress = import.CsePersonEmailAddress.EmailAddress ?? "";

    entities.CsePersonEmailAddress.Populated = false;
    Update("UpdateCsePersonEmailAddress",
      (db, command) =>
      {
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "emailSource", emailSource);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "emailAddr", emailAddress);
        db.SetString(
          command, "cspNumber", entities.CsePersonEmailAddress.CspNumber);
        db.SetDateTime(
          command, "createdTmst",
          entities.CsePersonEmailAddress.CreatedTimestamp.GetValueOrDefault());
      });

    entities.CsePersonEmailAddress.EffectiveDate = effectiveDate;
    entities.CsePersonEmailAddress.EndDate = endDate;
    entities.CsePersonEmailAddress.EmailSource = emailSource;
    entities.CsePersonEmailAddress.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonEmailAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePersonEmailAddress.EmailAddress = emailAddress;
    entities.CsePersonEmailAddress.Populated = true;
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
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
    }

    private CsePerson csePerson;
    private CsePersonEmailAddress csePersonEmailAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
    }

    private CsePersonEmailAddress csePersonEmailAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
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

    private CsePersonEmailAddress csePersonEmailAddress;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private CsePersonEmailAddress csePersonEmailAddress;
    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
  }
#endregion
}
