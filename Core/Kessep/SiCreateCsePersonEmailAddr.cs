// Program: SI_CREATE_CSE_PERSON_EMAIL_ADDR, ID: 1902521499, model: 746.
// Short name: SWE03752
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_CSE_PERSON_EMAIL_ADDR.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD creates an address for a given CSE Person
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateCsePersonEmailAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_CSE_PERSON_EMAIL_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateCsePersonEmailAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateCsePersonEmailAddr.
  /// </summary>
  public SiCreateCsePersonEmailAddr(IContext context, Import import,
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
    //   Date	   Developer	  Description
    // 02/01/2016 D Dupree       Initial Development
    // ------------------------------------------------------------
    export.CsePersonEmailAddress.Assign(import.CsePersonEmailAddress);
    local.Current.Timestamp = Now();

    if (Equal(import.CsePersonEmailAddress.EndDate, null))
    {
      local.CsePersonEmailAddress.EndDate = UseCabSetMaximumDiscontinueDate1();
    }
    else
    {
      local.CsePersonEmailAddress.EndDate =
        import.CsePersonEmailAddress.EndDate;
    }

    // ---------------------------------------------
    // This PAD creates an email address for a CSE Person.
    // ---------------------------------------------
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    try
    {
      CreateCsePersonEmailAddress();
      export.CsePersonEmailAddress.Assign(entities.CsePersonEmailAddress);
      local.EndDate.Date = export.CsePersonEmailAddress.EndDate;
      export.CsePersonEmailAddress.EndDate = UseCabSetMaximumDiscontinueDate2();

      // ---------------------------------------------
      // 12/18/98 W. Campbell - inserted set statements
      // to set END_DATE in export view.
      // Work done on IDCR454.
      // ---------------------------------------------
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CSE_PERSON_EMAIL_ADDR_AE";

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

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.EndDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateCsePersonEmailAddress()
  {
    var cspNumber = entities.CsePerson.Number;
    var effectiveDate = import.CsePersonEmailAddress.EffectiveDate;
    var endDate = local.CsePersonEmailAddress.EndDate;
    var emailSource = import.CsePersonEmailAddress.EmailSource ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var emailAddress = import.CsePersonEmailAddress.EmailAddress ?? "";

    entities.CsePersonEmailAddress.Populated = false;
    Update("CreateCsePersonEmailAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "emailSource", emailSource);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "emailAddr", emailAddress);
      });

    entities.CsePersonEmailAddress.CspNumber = cspNumber;
    entities.CsePersonEmailAddress.EffectiveDate = effectiveDate;
    entities.CsePersonEmailAddress.EndDate = endDate;
    entities.CsePersonEmailAddress.EmailSource = emailSource;
    entities.CsePersonEmailAddress.CreatedBy = createdBy;
    entities.CsePersonEmailAddress.CreatedTimestamp = createdTimestamp;
    entities.CsePersonEmailAddress.LastUpdatedBy = createdBy;
    entities.CsePersonEmailAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.CsePersonEmailAddress.EmailAddress = emailAddress;
    entities.CsePersonEmailAddress.Populated = true;
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
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
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
    private DateWorkArea endDate;
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

    private CsePersonEmailAddress csePersonEmailAddress;
    private CsePerson csePerson;
  }
#endregion
}
