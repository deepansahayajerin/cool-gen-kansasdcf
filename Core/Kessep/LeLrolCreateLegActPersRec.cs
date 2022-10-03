// Program: LE_LROL_CREATE_LEG_ACT_PERS_REC, ID: 371986318, model: 746.
// Short name: SWE00742
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LROL_CREATE_LEG_ACT_PERS_REC.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will create a Legal Action Person related to the CSE 
/// Person and the Legal Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeLrolCreateLegActPersRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LROL_CREATE_LEG_ACT_PERS_REC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLrolCreateLegActPersRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLrolCreateLegActPersRec.
  /// </summary>
  public LeLrolCreateLegActPersRec(IContext context, Import import,
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
    // Date		Developer	Request #	Description
    // 05/22/95	Dave Allen			Initial Code
    // 10/25/97	govind				Modified to use random generator for Identifier
    // 10/31/97	govind				Removed persistent views
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    if (!Lt(local.Zero.Date, import.LegalActionPerson.EndDate))
    {
      local.LegalActionPerson.EndDate = UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.LegalActionPerson.EndDate = import.LegalActionPerson.EndDate;
    }

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    local.Dummy.Flag = "";

    if (IsEmpty(local.Dummy.Flag))
    {
      for(local.NoOfRetries.Count = 1; local.NoOfRetries.Count <= 10; ++
        local.NoOfRetries.Count)
      {
        try
        {
          CreateLegalActionPerson();
          export.LegalActionPerson.Assign(entities.LegalActionPerson);

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
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateLegalActionPerson()
  {
    var identifier = UseGenerate9DigitRandomNumber();
    var cspNumber = entities.CsePerson.Number;
    var lgaIdentifier = entities.LegalAction.Identifier;
    var effectiveDate = import.LegalActionPerson.EffectiveDate;
    var role = import.LegalActionPerson.Role;
    var endDate = local.LegalActionPerson.EndDate;
    var endReason = import.LegalActionPerson.EndReason ?? "";
    var createdTstamp = local.Current.Timestamp;
    var createdBy = global.UserId;

    entities.LegalActionPerson.Populated = false;
    Update("CreateLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", identifier);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetString(command, "role", role);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetNullableString(command, "endReason", endReason);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "accountType", "");
        db.SetNullableDecimal(command, "arrearsAmount", 0M);
      });

    entities.LegalActionPerson.Identifier = identifier;
    entities.LegalActionPerson.CspNumber = cspNumber;
    entities.LegalActionPerson.LgaIdentifier = lgaIdentifier;
    entities.LegalActionPerson.EffectiveDate = effectiveDate;
    entities.LegalActionPerson.Role = role;
    entities.LegalActionPerson.EndDate = endDate;
    entities.LegalActionPerson.EndReason = endReason;
    entities.LegalActionPerson.CreatedTstamp = createdTstamp;
    entities.LegalActionPerson.CreatedBy = createdBy;
    entities.LegalActionPerson.Populated = true;
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
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
        entities.LegalAction.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private LegalAction legalAction;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private LegalActionPerson legalActionPerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

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
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private DateWorkArea current;
    private DateWorkArea zero;
    private Common dummy;
    private Common noOfRetries;
    private SystemGenerated systemGenerated;
    private LegalActionPerson legalActionPerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private LegalAction legalAction;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
  }
#endregion
}
