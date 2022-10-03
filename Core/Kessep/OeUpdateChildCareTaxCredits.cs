// Program: OE_UPDATE_CHILD_CARE_TAX_CREDITS, ID: 371895119, model: 746.
// Short name: SWE00966
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_UPDATE_CHILD_CARE_TAX_CREDITS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeUpdateChildCareTaxCredits: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UPDATE_CHILD_CARE_TAX_CREDITS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUpdateChildCareTaxCredits(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUpdateChildCareTaxCredits.
  /// </summary>
  public OeUpdateChildCareTaxCredits(IContext context, Import import,
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
    UseOeCabSetMnemonics();
    export.ChildCareTaxCreditFactors.Assign(import.ChildCareTaxCreditFactors);

    if (Equal(import.ChildCareTaxCreditFactors.EffectiveDate, null))
    {
      export.ChildCareTaxCreditFactors.EffectiveDate = Now().Date;
    }

    if (Equal(import.ChildCareTaxCreditFactors.ExpirationDate, null))
    {
      export.ChildCareTaxCreditFactors.ExpirationDate =
        local.MaxDate.ExpirationDate;
    }

    if (ReadChildCareTaxCreditFactors())
    {
      try
      {
        UpdateChildCareTaxCreditFactors();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CHILD_CARE_CREDIT_NU";

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
      ExitState = "CHILD_CARE_CREDIT_NF";
    }
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
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

  private void UpdateChildCareTaxCreditFactors()
  {
    var expirationDate = export.ChildCareTaxCreditFactors.ExpirationDate;
    var effectiveDate = export.ChildCareTaxCreditFactors.EffectiveDate;
    var adjustedGrossIncomeMaximum =
      export.ChildCareTaxCreditFactors.AdjustedGrossIncomeMaximum;
    var adjustedGrossIncomeMinimum =
      export.ChildCareTaxCreditFactors.AdjustedGrossIncomeMinimum;
    var kansasTaxCreditPercent =
      export.ChildCareTaxCreditFactors.KansasTaxCreditPercent;
    var federalTaxCreditPercent =
      export.ChildCareTaxCreditFactors.FederalTaxCreditPercent;
    var maxMonthlyCreditMultChildren =
      export.ChildCareTaxCreditFactors.MaxMonthlyCreditMultChildren;
    var maxMonthlyCredit1Child =
      export.ChildCareTaxCreditFactors.MaxMonthlyCredit1Child;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ChildCareTaxCreditFactors.Populated = false;
    Update("UpdateChildCareTaxCreditFactors",
      (db, command) =>
      {
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetInt32(command, "adjGrossIncMax", adjustedGrossIncomeMaximum);
        db.SetInt32(command, "adjGrossIncMin", adjustedGrossIncomeMinimum);
        db.SetDecimal(command, "ksTaxCrPercent", kansasTaxCreditPercent);
        db.SetDecimal(command, "fedTaxCrPercent", federalTaxCreditPercent);
        db.SetInt32(command, "maxMthlyCrMch", maxMonthlyCreditMultChildren);
        db.SetInt32(command, "maxMthlyCr1Ch", maxMonthlyCredit1Child);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "identifier", entities.ChildCareTaxCreditFactors.Identifier);
          
      });

    entities.ChildCareTaxCreditFactors.ExpirationDate = expirationDate;
    entities.ChildCareTaxCreditFactors.EffectiveDate = effectiveDate;
    entities.ChildCareTaxCreditFactors.AdjustedGrossIncomeMaximum =
      adjustedGrossIncomeMaximum;
    entities.ChildCareTaxCreditFactors.AdjustedGrossIncomeMinimum =
      adjustedGrossIncomeMinimum;
    entities.ChildCareTaxCreditFactors.KansasTaxCreditPercent =
      kansasTaxCreditPercent;
    entities.ChildCareTaxCreditFactors.FederalTaxCreditPercent =
      federalTaxCreditPercent;
    entities.ChildCareTaxCreditFactors.MaxMonthlyCreditMultChildren =
      maxMonthlyCreditMultChildren;
    entities.ChildCareTaxCreditFactors.MaxMonthlyCredit1Child =
      maxMonthlyCredit1Child;
    entities.ChildCareTaxCreditFactors.LastUpdatedBy = lastUpdatedBy;
    entities.ChildCareTaxCreditFactors.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ChildCareTaxCreditFactors.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private Code maxDate;
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
