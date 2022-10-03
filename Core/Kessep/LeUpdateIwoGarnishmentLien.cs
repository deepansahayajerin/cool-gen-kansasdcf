// Program: LE_UPDATE_IWO_GARNISHMENT_LIEN, ID: 372029004, model: 746.
// Short name: SWE00831
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
/// A program: LE_UPDATE_IWO_GARNISHMENT_LIEN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action diagram will update information about an Income Withholding 
/// Order (IWO), Garnishment, or Lien for a specific Legal Action and a given
/// CSE Person.
/// </para>
/// </summary>
[Serializable]
public partial class LeUpdateIwoGarnishmentLien: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_UPDATE_IWO_GARNISHMENT_LIEN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeUpdateIwoGarnishmentLien(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeUpdateIwoGarnishmentLien.
  /// </summary>
  public LeUpdateIwoGarnishmentLien(IContext context, Import import,
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
    if (!Lt(local.Zero.Date, import.New1.EndDate))
    {
      local.LegalActionIncomeSource.EndDate = UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.LegalActionIncomeSource.EndDate = import.New1.EndDate;
    }

    if (AsChar(import.Type1.Text1) == 'I' || AsChar(import.Type1.Text1) == 'G'
      && AsChar(import.New1.WageOrNonWage) == 'W')
    {
      // ---------------------------------------------
      // Check for any overlapping iwo/garnishment
      // ---------------------------------------------
      if (ReadLegalActionIncomeSource1())
      {
        foreach(var item in ReadLegalActionIncomeSource2())
        {
          // ****************************************************************
          // * If there exists an IWGL record (same source, same legal action
          // * don't allowupdate of an IWGL record if the existing
          // * IWGL record's end date is zero (still TBD) or equal to or greater
          // than
          // * the current date (still active).  In addition, if an existing 
          // IWGL
          // * record's end date is less than current date (expired) don't
          // * allow an effective date to be less or equal to it; unless the 
          // effective
          // * date is zero (which case the eff/end dates are TBD).
          // ****************************************************************
          if (Equal(entities.OverlappingLegalActionIncomeSource.EndDate,
            local.Zero.Date) || Equal
            (entities.OverlappingLegalActionIncomeSource.EndDate,
            new DateTime(2099, 12, 31)))
          {
            // ** Open-ended IWGL exists
            ExitState = "LE0000_OVERLAPPING_IWGL_EXISTS";

            return;
          }
          else if (Lt(entities.OverlappingLegalActionIncomeSource.EndDate,
            Now().Date))
          {
            // ** Defined ended and expired IWGL exists
            if (Lt(local.Zero.Date, import.New1.EffectiveDate) && !
              Lt(entities.OverlappingLegalActionIncomeSource.EndDate,
              import.New1.EffectiveDate))
            {
              ExitState = "LE0000_OVERLAPPING_IWGL_EXISTS";

              return;
            }
          }
          else
          {
            // ** Defined ended (in the future) IWGL exists
            ExitState = "LE0000_OVERLAPPING_IWGL_EXISTS";

            return;
          }
        }

        try
        {
          UpdateLegalActionIncomeSource();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LEGAL_ACTION_PERSON_RESOURCE_NU";

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
        ExitState = "LE0000_LEGAL_ACTN_INCOME_SRCE_NF";
      }
    }
    else if (AsChar(import.Type1.Text1) == 'L' || AsChar
      (import.Type1.Text1) == 'G' && AsChar(import.New1.WageOrNonWage) == 'N')
    {
      if (ReadLegalActionPersonResource1())
      {
        foreach(var item in ReadLegalActionPersonResource2())
        {
          // ****************************************************************
          // * If there exists an IWGL record (same source, same legal action
          // * don't allowupdate of an IWGL record if the existing
          // * IWGL record's end date is zero (still TBD) or equal to or greater
          // than
          // * the current date (still active).  In addition, if an existing 
          // IWGL
          // * record's end date is less than current date (expired) don't
          // * allow an effective date to be less or equal to it; unless the 
          // effective
          // * date is zero (which case the eff/end dates are TBD).
          // ****************************************************************
          if (Equal(entities.OverlappingLegalActionPersonResource.EndDate,
            local.Zero.Date))
          {
            // ** Open-ended IWGL exists
            ExitState = "LE0000_OVERLAPPING_IWGL_EXISTS";

            return;
          }
          else if (Lt(entities.OverlappingLegalActionPersonResource.EndDate,
            Now().Date))
          {
            // ** Defined ended and expired IWGL exists
            if (Lt(local.Zero.Date, import.New1.EffectiveDate) && !
              Lt(entities.OverlappingLegalActionPersonResource.EndDate,
              import.New1.EffectiveDate))
            {
              ExitState = "LE0000_OVERLAPPING_IWGL_EXISTS";

              return;
            }
          }
          else
          {
            // ** Defined ended (in the future) IWGL exists
            ExitState = "LE0000_OVERLAPPING_IWGL_EXISTS";

            return;
          }
        }

        try
        {
          UpdateLegalActionPersonResource();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LEGAL_ACTION_PERSON_RESOURCE_NU";

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
        ExitState = "LEGAL_ACTION_PERSON_RESOURCE_NF";
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Zero.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadLegalActionIncomeSource1()
  {
    entities.ExistingLegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSource1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDateTime(
          command, "isrIdentifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetInt32(command, "identifier", import.Current.Identifier);
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
    entities.OverlappingLegalActionIncomeSource.Populated = false;

    return ReadEach("ReadLegalActionIncomeSource2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "isrIdentifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalActionIncomeSource.Identifier);
      },
      (db, reader) =>
      {
        entities.OverlappingLegalActionIncomeSource.CspNumber =
          db.GetString(reader, 0);
        entities.OverlappingLegalActionIncomeSource.LgaIdentifier =
          db.GetInt32(reader, 1);
        entities.OverlappingLegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.OverlappingLegalActionIncomeSource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.OverlappingLegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.OverlappingLegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 5);
        entities.OverlappingLegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 6);
        entities.OverlappingLegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 7);
        entities.OverlappingLegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 8);
        entities.OverlappingLegalActionIncomeSource.Identifier =
          db.GetInt32(reader, 9);
        entities.OverlappingLegalActionIncomeSource.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionPersonResource1()
  {
    entities.ExistingLegalActionPersonResource.Populated = false;

    return Read("ReadLegalActionPersonResource1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(
          command, "cprResourceNo", import.LienCsePersonResource.ResourceNo);
        db.SetInt32(
          command, "identifier",
          import.LienLegalActionPersonResource.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionPersonResource.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingLegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.ExistingLegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingLegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingLegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingLegalActionPersonResource.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.ExistingLegalActionPersonResource.CreatedBy =
          db.GetString(reader, 7);
        entities.ExistingLegalActionPersonResource.Identifier =
          db.GetInt32(reader, 8);
        entities.ExistingLegalActionPersonResource.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonResource2()
  {
    entities.OverlappingLegalActionPersonResource.Populated = false;

    return ReadEach("ReadLegalActionPersonResource2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo", import.LienCsePersonResource.ResourceNo);
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalActionPersonResource.Identifier);
      },
      (db, reader) =>
      {
        entities.OverlappingLegalActionPersonResource.CspNumber =
          db.GetString(reader, 0);
        entities.OverlappingLegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.OverlappingLegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.OverlappingLegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.OverlappingLegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.OverlappingLegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.OverlappingLegalActionPersonResource.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.OverlappingLegalActionPersonResource.Identifier =
          db.GetInt32(reader, 7);
        entities.OverlappingLegalActionPersonResource.Populated = true;

        return true;
      });
  }

  private void UpdateLegalActionIncomeSource()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingLegalActionIncomeSource.Populated);

    var effectiveDate = import.New1.EffectiveDate;
    var withholdingType = import.New1.WithholdingType;
    var endDate = local.LegalActionIncomeSource.EndDate;
    var wageOrNonWage = import.New1.WageOrNonWage ?? "";
    var orderType = import.New1.OrderType ?? "";

    entities.ExistingLegalActionIncomeSource.Populated = false;
    Update("UpdateLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetString(command, "withholdingType", withholdingType);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetNullableString(command, "wageOrNonWage", wageOrNonWage);
        db.SetNullableString(command, "orderType", orderType);
        db.SetString(
          command, "cspNumber",
          entities.ExistingLegalActionIncomeSource.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier",
          entities.ExistingLegalActionIncomeSource.LgaIdentifier);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.ExistingLegalActionIncomeSource.IsrIdentifier.
            GetValueOrDefault());
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalActionIncomeSource.Identifier);
      });

    entities.ExistingLegalActionIncomeSource.EffectiveDate = effectiveDate;
    entities.ExistingLegalActionIncomeSource.WithholdingType = withholdingType;
    entities.ExistingLegalActionIncomeSource.EndDate = endDate;
    entities.ExistingLegalActionIncomeSource.WageOrNonWage = wageOrNonWage;
    entities.ExistingLegalActionIncomeSource.OrderType = orderType;
    entities.ExistingLegalActionIncomeSource.Populated = true;
  }

  private void UpdateLegalActionPersonResource()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingLegalActionPersonResource.Populated);

    var effectiveDate = import.New1.EffectiveDate;
    var lienType = import.LienLegalActionPersonResource.LienType ?? "";
    var endDate = local.LegalActionIncomeSource.EndDate;

    entities.ExistingLegalActionPersonResource.Populated = false;
    Update("UpdateLegalActionPersonResource",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableString(command, "lienType", lienType);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetString(
          command, "cspNumber",
          entities.ExistingLegalActionPersonResource.CspNumber);
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingLegalActionPersonResource.CprResourceNo);
        db.SetInt32(
          command, "lgaIdentifier",
          entities.ExistingLegalActionPersonResource.LgaIdentifier);
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalActionPersonResource.Identifier);
      });

    entities.ExistingLegalActionPersonResource.EffectiveDate = effectiveDate;
    entities.ExistingLegalActionPersonResource.LienType = lienType;
    entities.ExistingLegalActionPersonResource.EndDate = endDate;
    entities.ExistingLegalActionPersonResource.Populated = true;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public LegalActionIncomeSource Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Type1.
    /// </summary>
    [JsonPropertyName("type1")]
    public WorkArea Type1
    {
      get => type1 ??= new();
      set => type1 = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public LegalActionIncomeSource New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    /// <summary>
    /// A value of LienLegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("lienLegalActionPersonResource")]
    public LegalActionPersonResource LienLegalActionPersonResource
    {
      get => lienLegalActionPersonResource ??= new();
      set => lienLegalActionPersonResource = value;
    }

    /// <summary>
    /// A value of LienCsePersonResource.
    /// </summary>
    [JsonPropertyName("lienCsePersonResource")]
    public CsePersonResource LienCsePersonResource
    {
      get => lienCsePersonResource ??= new();
      set => lienCsePersonResource = value;
    }

    private LegalActionIncomeSource current;
    private LegalAction legalAction;
    private WorkArea type1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalActionIncomeSource new1;
    private IncomeSource incomeSource;
    private LegalActionPersonResource lienLegalActionPersonResource;
    private CsePersonResource lienCsePersonResource;
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

    private LegalActionIncomeSource legalActionIncomeSource;
    private DateWorkArea zero;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("existingLegalActionIncomeSource")]
    public LegalActionIncomeSource ExistingLegalActionIncomeSource
    {
      get => existingLegalActionIncomeSource ??= new();
      set => existingLegalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("existingLegalActionPersonResource")]
    public LegalActionPersonResource ExistingLegalActionPersonResource
    {
      get => existingLegalActionPersonResource ??= new();
      set => existingLegalActionPersonResource = value;
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
    /// A value of OverlappingLegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("overlappingLegalActionIncomeSource")]
    public LegalActionIncomeSource OverlappingLegalActionIncomeSource
    {
      get => overlappingLegalActionIncomeSource ??= new();
      set => overlappingLegalActionIncomeSource = value;
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
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of OverlappingLegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("overlappingLegalActionPersonResource")]
    public LegalActionPersonResource OverlappingLegalActionPersonResource
    {
      get => overlappingLegalActionPersonResource ??= new();
      set => overlappingLegalActionPersonResource = value;
    }

    private LegalActionIncomeSource existingLegalActionIncomeSource;
    private LegalActionPersonResource existingLegalActionPersonResource;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private LegalActionIncomeSource overlappingLegalActionIncomeSource;
    private LegalAction legalAction;
    private CsePersonResource csePersonResource;
    private LegalActionPersonResource overlappingLegalActionPersonResource;
  }
#endregion
}
