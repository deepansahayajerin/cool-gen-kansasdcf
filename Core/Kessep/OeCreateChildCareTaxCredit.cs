// Program: OE_CREATE_CHILD_CARE_TAX_CREDIT, ID: 371895121, model: 746.
// Short name: SWE00888
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CREATE_CHILD_CARE_TAX_CREDIT.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeCreateChildCareTaxCredit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CREATE_CHILD_CARE_TAX_CREDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCreateChildCareTaxCredit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCreateChildCareTaxCredit.
  /// </summary>
  public OeCreateChildCareTaxCredit(IContext context, Import import,
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

    if (import.ChildCareTaxCreditFactors.AdjustedGrossIncomeMaximum == 0)
    {
      export.ChildCareTaxCreditFactors.AdjustedGrossIncomeMaximum = 999999;
    }

    // ****************************************************************
    // Do you allow to add new record if there active records.
    // ****************************************************************
    if (ReadChildCareTaxCreditFactors1())
    {
      ExitState = "OE0178_CHILD_CARE_TAX_ADD_ERRIR";

      return;
    }

    ReadChildCareTaxCreditFactors2();
    export.ChildCareTaxCreditFactors.Identifier =
      entities.ChildCareTaxCreditFactors.Identifier + 1;

    try
    {
      CreateChildCareTaxCreditFactors();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
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

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void CreateChildCareTaxCreditFactors()
  {
    var identifier = export.ChildCareTaxCreditFactors.Identifier;
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
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ChildCareTaxCreditFactors.Populated = false;
    Update("CreateChildCareTaxCreditFactors",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetInt32(command, "adjGrossIncMax", adjustedGrossIncomeMaximum);
        db.SetInt32(command, "adjGrossIncMin", adjustedGrossIncomeMinimum);
        db.SetDecimal(command, "ksTaxCrPercent", kansasTaxCreditPercent);
        db.SetDecimal(command, "fedTaxCrPercent", federalTaxCreditPercent);
        db.SetInt32(command, "maxMthlyCrMch", maxMonthlyCreditMultChildren);
        db.SetInt32(command, "maxMthlyCr1Ch", maxMonthlyCredit1Child);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.ChildCareTaxCreditFactors.Identifier = identifier;
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
    entities.ChildCareTaxCreditFactors.CreatedBy = createdBy;
    entities.ChildCareTaxCreditFactors.CreatedTimestamp = createdTimestamp;
    entities.ChildCareTaxCreditFactors.LastUpdatedBy = createdBy;
    entities.ChildCareTaxCreditFactors.LastUpdatedTimestamp = createdTimestamp;
    entities.ChildCareTaxCreditFactors.Populated = true;
  }

  private bool ReadChildCareTaxCreditFactors1()
  {
    entities.ChildCareTaxCreditFactors.Populated = false;

    return Read("ReadChildCareTaxCreditFactors1",
      (db, command) =>
      {
        db.SetDate(
          command, "expirationDate",
          local.MaxDate.ExpirationDate.GetValueOrDefault());
        db.SetDecimal(
          command, "ksTaxCrPercent",
          import.ChildCareTaxCreditFactors.KansasTaxCreditPercent);
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

  private bool ReadChildCareTaxCreditFactors2()
  {
    entities.ChildCareTaxCreditFactors.Populated = false;

    return Read("ReadChildCareTaxCreditFactors2",
      null,
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
