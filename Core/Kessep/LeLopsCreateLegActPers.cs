// Program: LE_LOPS_CREATE_LEG_ACT_PERS, ID: 372007460, model: 746.
// Short name: SWE02134
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LOPS_CREATE_LEG_ACT_PERS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block creates a Legal Action Person record for Legal 
/// Obligation
/// </para>
/// </summary>
[Serializable]
public partial class LeLopsCreateLegActPers: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LOPS_CREATE_LEG_ACT_PERS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLopsCreateLegActPers(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLopsCreateLegActPers.
  /// </summary>
  public LeLopsCreateLegActPers(IContext context, Import import, Export export):
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
    // ---------------------------------------------
    // 103097	govind		Initial code
    // ---------------------------------------------
    local.Current.Timestamp = Now();

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadLegalActionDetail())
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";

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

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);

    var identifier = UseGenerate9DigitRandomNumber();
    var cspNumber = entities.CsePerson.Number;
    var effectiveDate = import.LegalActionPerson.EffectiveDate;
    var endDate = import.LegalActionPerson.EndDate;
    var endReason = import.LegalActionPerson.EndReason ?? "";
    var createdTstamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var lgaRIdentifier = entities.LegalActionDetail.LgaIdentifier;
    var ladRNumber = entities.LegalActionDetail.Number;
    var accountType = import.LegalActionPerson.AccountType ?? "";
    var arrearsAmount =
      import.LegalActionPerson.ArrearsAmount.GetValueOrDefault();
    var currentAmount =
      import.LegalActionPerson.CurrentAmount.GetValueOrDefault();
    var judgementAmount =
      import.LegalActionPerson.JudgementAmount.GetValueOrDefault();

    entities.LegalActionPerson.Populated = false;
    Update("CreateLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", identifier);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetString(command, "role", "");
        db.SetNullableDate(command, "endDt", endDate);
        db.SetNullableString(command, "endReason", endReason);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableInt32(command, "lgaRIdentifier", lgaRIdentifier);
        db.SetNullableInt32(command, "ladRNumber", ladRNumber);
        db.SetNullableString(command, "accountType", accountType);
        db.SetNullableDecimal(command, "arrearsAmount", arrearsAmount);
        db.SetNullableDecimal(command, "currentAmount", currentAmount);
        db.SetNullableDecimal(command, "judgementAmount", judgementAmount);
      });

    entities.LegalActionPerson.Identifier = identifier;
    entities.LegalActionPerson.CspNumber = cspNumber;
    entities.LegalActionPerson.EffectiveDate = effectiveDate;
    entities.LegalActionPerson.Role = "";
    entities.LegalActionPerson.EndDate = endDate;
    entities.LegalActionPerson.EndReason = endReason;
    entities.LegalActionPerson.CreatedTstamp = createdTstamp;
    entities.LegalActionPerson.CreatedBy = createdBy;
    entities.LegalActionPerson.LgaRIdentifier = lgaRIdentifier;
    entities.LegalActionPerson.LadRNumber = ladRNumber;
    entities.LegalActionPerson.AccountType = accountType;
    entities.LegalActionPerson.ArrearsAmount = arrearsAmount;
    entities.LegalActionPerson.CurrentAmount = currentAmount;
    entities.LegalActionPerson.JudgementAmount = judgementAmount;
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
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.Populated = true;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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

    private LegalActionDetail legalActionDetail;
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
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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

    private Common dummy;
    private DateWorkArea current;
    private LegalActionPerson legalActionPerson;
    private Common noOfRetries;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private CsePerson csePerson;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
