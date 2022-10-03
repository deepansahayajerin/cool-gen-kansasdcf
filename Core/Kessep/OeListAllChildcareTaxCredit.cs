// Program: OE_LIST_ALL_CHILDCARE_TAX_CREDIT, ID: 371895120, model: 746.
// Short name: SWE00940
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
/// A program: OE_LIST_ALL_CHILDCARE_TAX_CREDIT.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeListAllChildcareTaxCredit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LIST_ALL_CHILDCARE_TAX_CREDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeListAllChildcareTaxCredit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeListAllChildcareTaxCredit.
  /// </summary>
  public OeListAllChildcareTaxCredit(IContext context, Import import,
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
    // ---------------------------------------------
    // Date          Author           Reason
    // 04/06/95       Sid           Initial Creation
    // 04/30/97      G P Kim	     Change Current Date
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    UseOeCabSetMnemonics();

    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadChildCareTaxCreditFactors())
    {
      export.Export1.Update.Detail.Assign(entities.ChildCareTaxCreditFactors);

      if (entities.ChildCareTaxCreditFactors.AdjustedGrossIncomeMaximum == 999999
        )
      {
        export.Export1.Update.Detail.AdjustedGrossIncomeMaximum = 0;
      }

      export.Export1.Next();
    }

    if (!export.Export1.IsEmpty)
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      MoveChildCareTaxCreditFactors(entities.ChildCareTaxCreditFactors,
        export.ChildCareTaxCreditFactors);

      if (Equal(export.ChildCareTaxCreditFactors.ExpirationDate,
        local.MaxDate.ExpirationDate))
      {
        export.ChildCareTaxCreditFactors.ExpirationDate = null;
      }
    }
  }

  private static void MoveChildCareTaxCreditFactors(
    ChildCareTaxCreditFactors source, ChildCareTaxCreditFactors target)
  {
    target.ExpirationDate = source.ExpirationDate;
    target.EffectiveDate = source.EffectiveDate;
    target.Identifier = source.Identifier;
    target.KansasTaxCreditPercent = source.KansasTaxCreditPercent;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private IEnumerable<bool> ReadChildCareTaxCreditFactors()
  {
    return ReadEach("ReadChildCareTaxCreditFactors",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

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
    /// A value of ChildCareTaxCreditFactors.
    /// </summary>
    [JsonPropertyName("childCareTaxCreditFactors")]
    public ChildCareTaxCreditFactors ChildCareTaxCreditFactors
    {
      get => childCareTaxCreditFactors ??= new();
      set => childCareTaxCreditFactors = value;
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

    private ChildCareTaxCreditFactors childCareTaxCreditFactors;
    private Array<ExportGroup> export1;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private DateWorkArea current;
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
