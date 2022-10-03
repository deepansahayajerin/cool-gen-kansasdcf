// Program: OE_UPDATE_CHILD_CARE_CREDIT_DATE, ID: 371895123, model: 746.
// Short name: SWE00965
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
/// A program: OE_UPDATE_CHILD_CARE_CREDIT_DATE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeUpdateChildCareCreditDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UPDATE_CHILD_CARE_CREDIT_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUpdateChildCareCreditDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUpdateChildCareCreditDate.
  /// </summary>
  public OeUpdateChildCareCreditDate(IContext context, Import import,
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

    if (Equal(export.ChildCareTaxCreditFactors.EffectiveDate, null))
    {
      export.ChildCareTaxCreditFactors.EffectiveDate = Now().Date;
    }

    if (Equal(export.ChildCareTaxCreditFactors.ExpirationDate, null))
    {
      export.ChildCareTaxCreditFactors.ExpirationDate =
        local.MaxDate.ExpirationDate;
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
      export.Export1.Update.Detail.EffectiveDate =
        export.ChildCareTaxCreditFactors.EffectiveDate;
      export.Export1.Update.Detail.ExpirationDate =
        export.ChildCareTaxCreditFactors.ExpirationDate;
      export.Export1.Update.Detail.KansasTaxCreditPercent =
        export.ChildCareTaxCreditFactors.KansasTaxCreditPercent;

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
        export.Export1.Next();

        return;
      }

      export.Export1.Next();
    }

    if (Equal(export.ChildCareTaxCreditFactors.ExpirationDate,
      local.MaxDate.ExpirationDate))
    {
      export.ChildCareTaxCreditFactors.ExpirationDate = null;
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
        db.
          SetInt32(command, "identifier", import.Import1.Item.Detail.Identifier);
          
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
    var expirationDate = export.Export1.Item.Detail.ExpirationDate;
    var effectiveDate = export.Export1.Item.Detail.EffectiveDate;
    var kansasTaxCreditPercent =
      export.Export1.Item.Detail.KansasTaxCreditPercent;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ChildCareTaxCreditFactors.Populated = false;
    Update("UpdateChildCareTaxCreditFactors",
      (db, command) =>
      {
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDecimal(command, "ksTaxCrPercent", kansasTaxCreditPercent);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "identifier", entities.ChildCareTaxCreditFactors.Identifier);
          
      });

    entities.ChildCareTaxCreditFactors.ExpirationDate = expirationDate;
    entities.ChildCareTaxCreditFactors.EffectiveDate = effectiveDate;
    entities.ChildCareTaxCreditFactors.KansasTaxCreditPercent =
      kansasTaxCreditPercent;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ChildCareTaxCreditFactors Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of Work.
      /// </summary>
      [JsonPropertyName("work")]
      public Common Work
      {
        get => work ??= new();
        set => work = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ChildCareTaxCreditFactors detail;
      private Common work;
    }

    /// <summary>
    /// A value of ChildCareTaxCreditFactors.
    /// </summary>
    [JsonPropertyName("childCareTaxCreditFactors")]
    public ChildCareTaxCreditFactors ChildCareTaxCreditFactors
    {
      get => childCareTaxCreditFactors ??= new();
      set => childCareTaxCreditFactors = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private ChildCareTaxCreditFactors childCareTaxCreditFactors;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ChildCareTaxCreditFactors Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of Work.
      /// </summary>
      [JsonPropertyName("work")]
      public Common Work
      {
        get => work ??= new();
        set => work = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ChildCareTaxCreditFactors detail;
      private Common work;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of ChildCareTaxCreditFactors.
    /// </summary>
    [JsonPropertyName("childCareTaxCreditFactors")]
    public ChildCareTaxCreditFactors ChildCareTaxCreditFactors
    {
      get => childCareTaxCreditFactors ??= new();
      set => childCareTaxCreditFactors = value;
    }

    private Array<ExportGroup> export1;
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
