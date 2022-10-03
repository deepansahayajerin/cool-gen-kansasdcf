// Program: LE_DELETE_IWO_GARNISHMENT_LIEN, ID: 372028998, model: 746.
// Short name: SWE00756
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DELETE_IWO_GARNISHMENT_LIEN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action diagram will delete information about an Income Withholding 
/// Order (IWO), Garnishment, or Lien for a specific Legal Action and a given
/// CSE Person.
/// </para>
/// </summary>
[Serializable]
public partial class LeDeleteIwoGarnishmentLien: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DELETE_IWO_GARNISHMENT_LIEN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDeleteIwoGarnishmentLien(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDeleteIwoGarnishmentLien.
  /// </summary>
  public LeDeleteIwoGarnishmentLien(IContext context, Import import,
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
    // ------------------------------------------------------------
    if (AsChar(import.Type1.Text1) == 'I' || AsChar(import.Type1.Text1) == 'G'
      && AsChar(import.LegalActionIncomeSource.WageOrNonWage) == 'W')
    {
      if (ReadLegalActionIncomeSource())
      {
        DeleteLegalActionIncomeSource();
      }
      else
      {
        ExitState = "LE0000_LEGAL_ACTN_INCOME_SRCE_NF";
      }
    }
    else if (AsChar(import.Type1.Text1) == 'L' || AsChar
      (import.Type1.Text1) == 'G' && AsChar
      (import.LegalActionIncomeSource.WageOrNonWage) == 'N')
    {
      if (ReadLegalActionPersonResource())
      {
        DeleteLegalActionPersonResource();
      }
      else
      {
        ExitState = "LEGAL_ACTION_PERSON_RESOURCE_NF";
      }
    }
  }

  private void DeleteLegalActionIncomeSource()
  {
    Update("DeleteLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.LegalActionIncomeSource.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier",
          entities.LegalActionIncomeSource.LgaIdentifier);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.LegalActionIncomeSource.IsrIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "identifier", entities.LegalActionIncomeSource.Identifier);
      });
  }

  private void DeleteLegalActionPersonResource()
  {
    Update("DeleteLegalActionPersonResource",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.LegalActionPersonResource.CspNumber);
        db.SetInt32(
          command, "cprResourceNo",
          entities.LegalActionPersonResource.CprResourceNo);
        db.SetInt32(
          command, "lgaIdentifier",
          entities.LegalActionPersonResource.LgaIdentifier);
        db.SetInt32(
          command, "identifier", entities.LegalActionPersonResource.Identifier);
          
      });
  }

  private bool ReadLegalActionIncomeSource()
  {
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "isrIdentifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(
          command, "identifier", import.LegalActionIncomeSource.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 4);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 5);
        entities.LegalActionIncomeSource.Populated = true;
      });
  }

  private bool ReadLegalActionPersonResource()
  {
    entities.LegalActionPersonResource.Populated = false;

    return Read("ReadLegalActionPersonResource",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo", import.CsePersonResource.ResourceNo);
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(
          command, "identifier", import.LegalActionIncomeSource.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPersonResource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.LegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.LegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.LegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionPersonResource.Identifier = db.GetInt32(reader, 6);
        entities.LegalActionPersonResource.Populated = true;
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
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
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
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
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

    private CsePersonResource csePersonResource;
    private IncomeSource incomeSource;
    private LegalActionIncomeSource legalActionIncomeSource;
    private LegalAction legalAction;
    private WorkArea type1;
    private CsePersonsWorkSet csePersonsWorkSet;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    private DateWorkArea zero;
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

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private LegalAction legalAction;
    private LegalActionIncomeSource legalActionIncomeSource;
    private IncomeSource incomeSource;
    private LegalActionPersonResource legalActionPersonResource;
    private CsePersonResource csePersonResource;
    private CsePerson csePerson;
  }
#endregion
}
