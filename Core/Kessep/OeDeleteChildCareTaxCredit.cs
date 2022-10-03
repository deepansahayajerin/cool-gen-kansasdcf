// Program: OE_DELETE_CHILD_CARE_TAX_CREDIT, ID: 371895122, model: 746.
// Short name: SWE00901
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_DELETE_CHILD_CARE_TAX_CREDIT.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeDeleteChildCareTaxCredit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DELETE_CHILD_CARE_TAX_CREDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDeleteChildCareTaxCredit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDeleteChildCareTaxCredit.
  /// </summary>
  public OeDeleteChildCareTaxCredit(IContext context, Import import,
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
    if (ReadChildCareTaxCreditFactors())
    {
      DeleteChildCareTaxCreditFactors();
      ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
    }
    else
    {
      ExitState = "CHILD_CARE_CREDIT_NF";
    }
  }

  private void DeleteChildCareTaxCreditFactors()
  {
    Update("DeleteChildCareTaxCreditFactors",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.ChildCareTaxCreditFactors.Identifier);
          
      });
  }

  private bool ReadChildCareTaxCreditFactors()
  {
    entities.ChildCareTaxCreditFactors.Populated = false;

    return Read("ReadChildCareTaxCreditFactors",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.ChildCareTaxCreditFactors.Identifier);
      },
      (db, reader) =>
      {
        entities.ChildCareTaxCreditFactors.Identifier = db.GetInt32(reader, 0);
        entities.ChildCareTaxCreditFactors.ExpirationDate =
          db.GetDate(reader, 1);
        entities.ChildCareTaxCreditFactors.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ChildCareTaxCreditFactors.AdjustedGrossIncomeMaximum =
          db.GetInt32(reader, 3);
        entities.ChildCareTaxCreditFactors.AdjustedGrossIncomeMinimum =
          db.GetInt32(reader, 4);
        entities.ChildCareTaxCreditFactors.KansasTaxCreditPercent =
          db.GetDecimal(reader, 5);
        entities.ChildCareTaxCreditFactors.FederalTaxCreditPercent =
          db.GetDecimal(reader, 6);
        entities.ChildCareTaxCreditFactors.MaxMonthlyCreditMultChildren =
          db.GetInt32(reader, 7);
        entities.ChildCareTaxCreditFactors.MaxMonthlyCredit1Child =
          db.GetInt32(reader, 8);
        entities.ChildCareTaxCreditFactors.CreatedBy = db.GetString(reader, 9);
        entities.ChildCareTaxCreditFactors.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.ChildCareTaxCreditFactors.LastUpdatedBy =
          db.GetString(reader, 11);
        entities.ChildCareTaxCreditFactors.LastUpdatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ChildCareTaxCreditFactors.Populated = true;
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
    /// A value of ChildCareTaxCreditFactors.
    /// </summary>
    [JsonPropertyName("childCareTaxCreditFactors")]
    public ChildCareTaxCreditFactors ChildCareTaxCreditFactors
    {
      get => childCareTaxCreditFactors ??= new();
      set => childCareTaxCreditFactors = value;
    }

    private ChildCareTaxCreditFactors childCareTaxCreditFactors;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ChildCareTaxCreditFactors.
    /// </summary>
    [JsonPropertyName("childCareTaxCreditFactors")]
    public ChildCareTaxCreditFactors ChildCareTaxCreditFactors
    {
      get => childCareTaxCreditFactors ??= new();
      set => childCareTaxCreditFactors = value;
    }

    private ChildCareTaxCreditFactors childCareTaxCreditFactors;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChildCareTaxCreditFactors.
    /// </summary>
    [JsonPropertyName("childCareTaxCreditFactors")]
    public ChildCareTaxCreditFactors ChildCareTaxCreditFactors
    {
      get => childCareTaxCreditFactors ??= new();
      set => childCareTaxCreditFactors = value;
    }

    private ChildCareTaxCreditFactors childCareTaxCreditFactors;
  }
#endregion
}
