// Program: ESTAB_LEGAL_ACTION_INC_SOURCE, ID: 372029005, model: 746.
// Short name: SWE00233
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: ESTAB_LEGAL_ACTION_INC_SOURCE.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates LEGAL ACTION INCOME SOURCE and associates it to LEGAL 
/// ACTION and INCOME SOURCE.
/// </para>
/// </summary>
[Serializable]
public partial class EstabLegalActionIncSource: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ESTAB_LEGAL_ACTION_INC_SOURCE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EstabLegalActionIncSource(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EstabLegalActionIncSource.
  /// </summary>
  public EstabLegalActionIncSource(IContext context, Import import,
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
    // 06/29/95	Dave Allen			Initial Code
    // 01/05/96	Maryrose Mallari		Rewrote PAD
    // ------------------------------------------------------------
    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (Equal(import.LegalActionIncomeSource.EndDate, null))
    {
      local.LegalActionIncomeSource.EndDate = UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.LegalActionIncomeSource.EndDate =
        import.LegalActionIncomeSource.EndDate;
    }

    if (ReadIncomeSource())
    {
      // ------------------------------------------------------------
      // Don't allow a duplicate to be added, even though the data
      // model allows it. (Business Rule)
      // ------------------------------------------------------------
      foreach(var item in ReadLegalActionIncomeSource2())
      {
        // ****************************************************************
        // * If there exists an IWGL record (same source, same legal action
        // * don't allow a new IWGL record to be added if the existing
        // * IWGL record's end date is zero (still TBD) or equal to or greater 
        // than
        // * the current date (still active).  In addition, if an existing IWGL
        // * record's end date is less than current date (expired) don't
        // * allow an effective date to be less or equal to it; unless the 
        // effective
        // * date is zero (which case the eff/end dates are TBD).
        // ****************************************************************
        if (Equal(entities.ExistingLegalActionIncomeSource.EndDate,
          local.Zero.Date) || Equal
          (entities.ExistingLegalActionIncomeSource.EndDate, new DateTime(2099,
          12, 31)))
        {
          // ** Open-ended IWGL exists
          ExitState = "LE0000_IWO_GARN_ALREADY_EXISTS";

          return;
        }
        else if (Lt(entities.ExistingLegalActionIncomeSource.EndDate, Now().Date))
          
        {
          // ** Defined ended and expired IWGL exists
          if (Lt(local.Zero.Date, import.LegalActionIncomeSource.EffectiveDate) &&
            !
            Lt(entities.ExistingLegalActionIncomeSource.EndDate,
            import.LegalActionIncomeSource.EffectiveDate))
          {
            ExitState = "LE0000_IWO_GARN_ALREADY_EXISTS";

            return;
          }
        }
        else
        {
          // ** Defined ended (in the future) IWGL exists
          ExitState = "LE0000_IWO_GARN_ALREADY_EXISTS";

          return;
        }
      }

      if (ReadLegalActionIncomeSource1())
      {
        local.LastSeqNo.Identifier =
          entities.ExistingLegalActionIncomeSource.Identifier;
      }

      try
      {
        CreateLegalActionIncomeSource();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LE0000_IWO_GARN_ALREADY_EXISTS";

            break;
          case ErrorCode.PermittedValueViolation:
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
      ExitState = "LE0000_INCOME_SOURCE_NOT_SPECIFD";
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateLegalActionIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingIncomeSource.Populated);

    var cspNumber = entities.ExistingIncomeSource.CspINumber;
    var lgaIdentifier = entities.ExistingLegalAction.Identifier;
    var isrIdentifier = entities.ExistingIncomeSource.Identifier;
    var effectiveDate = import.LegalActionIncomeSource.EffectiveDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var withholdingType = import.LegalActionIncomeSource.WithholdingType;
    var endDate = local.LegalActionIncomeSource.EndDate;
    var wageOrNonWage = import.LegalActionIncomeSource.WageOrNonWage ?? "";
    var orderType = import.LegalActionIncomeSource.OrderType ?? "";
    var identifier = local.LastSeqNo.Identifier + 1;

    entities.ExistingLegalActionIncomeSource.Populated = false;
    Update("CreateLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetDateTime(command, "isrIdentifier", isrIdentifier);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "withholdingType", withholdingType);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetNullableString(command, "wageOrNonWage", wageOrNonWage);
        db.SetNullableString(command, "orderType", orderType);
        db.SetInt32(command, "identifier", identifier);
      });

    entities.ExistingLegalActionIncomeSource.CspNumber = cspNumber;
    entities.ExistingLegalActionIncomeSource.LgaIdentifier = lgaIdentifier;
    entities.ExistingLegalActionIncomeSource.IsrIdentifier = isrIdentifier;
    entities.ExistingLegalActionIncomeSource.EffectiveDate = effectiveDate;
    entities.ExistingLegalActionIncomeSource.CreatedBy = createdBy;
    entities.ExistingLegalActionIncomeSource.CreatedTstamp = createdTstamp;
    entities.ExistingLegalActionIncomeSource.WithholdingType = withholdingType;
    entities.ExistingLegalActionIncomeSource.EndDate = endDate;
    entities.ExistingLegalActionIncomeSource.WageOrNonWage = wageOrNonWage;
    entities.ExistingLegalActionIncomeSource.OrderType = orderType;
    entities.ExistingLegalActionIncomeSource.Identifier = identifier;
    entities.ExistingLegalActionIncomeSource.Populated = true;
  }

  private bool ReadIncomeSource()
  {
    entities.ExistingIncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingIncomeSource.CspINumber = db.GetString(reader, 1);
        entities.ExistingIncomeSource.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionIncomeSource1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingIncomeSource.Populated);
    entities.ExistingLegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSource1",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.ExistingLegalAction.Identifier);
        db.SetString(
          command, "cspNumber", entities.ExistingIncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.ExistingIncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionIncomeSource.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingLegalActionIncomeSource.LgaIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingLegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.ExistingLegalActionIncomeSource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingLegalActionIncomeSource.CreatedBy =
          db.GetString(reader, 4);
        entities.ExistingLegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 5);
        entities.ExistingLegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 6);
        entities.ExistingLegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingLegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 8);
        entities.ExistingLegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 9);
        entities.ExistingLegalActionIncomeSource.Identifier =
          db.GetInt32(reader, 10);
        entities.ExistingLegalActionIncomeSource.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionIncomeSource2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingIncomeSource.Populated);
    entities.ExistingLegalActionIncomeSource.Populated = false;

    return ReadEach("ReadLegalActionIncomeSource2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingIncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.ExistingIncomeSource.Identifier.GetValueOrDefault());
        db.SetInt32(
          command, "lgaIdentifier", entities.ExistingLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionIncomeSource.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingLegalActionIncomeSource.LgaIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingLegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.ExistingLegalActionIncomeSource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingLegalActionIncomeSource.CreatedBy =
          db.GetString(reader, 4);
        entities.ExistingLegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 5);
        entities.ExistingLegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 6);
        entities.ExistingLegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingLegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 8);
        entities.ExistingLegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 9);
        entities.ExistingLegalActionIncomeSource.Identifier =
          db.GetInt32(reader, 10);
        entities.ExistingLegalActionIncomeSource.Populated = true;

        return true;
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
    /// A value of IwglInd.
    /// </summary>
    [JsonPropertyName("iwglInd")]
    public WorkArea IwglInd
    {
      get => iwglInd ??= new();
      set => iwglInd = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    private WorkArea iwglInd;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalActionIncomeSource legalActionIncomeSource;
    private IncomeSource incomeSource;
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
    /// A value of LastSeqNo.
    /// </summary>
    [JsonPropertyName("lastSeqNo")]
    public LegalActionIncomeSource LastSeqNo
    {
      get => lastSeqNo ??= new();
      set => lastSeqNo = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
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
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    private LegalActionIncomeSource lastSeqNo;
    private LegalActionIncomeSource legalActionIncomeSource;
    private DateWorkArea zero;
    private LegalActionPersonResource legalActionPersonResource;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingIncomeSource.
    /// </summary>
    [JsonPropertyName("existingIncomeSource")]
    public IncomeSource ExistingIncomeSource
    {
      get => existingIncomeSource ??= new();
      set => existingIncomeSource = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("existingLegalActionIncomeSource")]
    public LegalActionIncomeSource ExistingLegalActionIncomeSource
    {
      get => existingLegalActionIncomeSource ??= new();
      set => existingLegalActionIncomeSource = value;
    }

    private LegalAction existingLegalAction;
    private CsePerson existingCsePerson;
    private IncomeSource existingIncomeSource;
    private LegalActionIncomeSource existingLegalActionIncomeSource;
  }
#endregion
}
