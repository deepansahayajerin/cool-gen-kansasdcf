// Program: FN_DISPLAY_DIST_POLICY_RULES, ID: 371962157, model: 746.
// Short name: SWE00444
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
/// A program: FN_DISPLAY_DIST_POLICY_RULES.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block reads Distribution Policy Rules in sequence, to be 
/// displayed on the List/Maintain Distribution Policy Rule screen.
/// </para>
/// </summary>
[Serializable]
public partial class FnDisplayDistPolicyRules: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISPLAY_DIST_POLICY_RULES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisplayDistPolicyRules(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisplayDistPolicyRules.
  /// </summary>
  public FnDisplayDistPolicyRules(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      if (ReadDistributionPolicy())
      {
        // ** OK **
        // NOT FOUND condition will cause an abort.
      }
    }

    // : READ EACH for selection list.
    // : Read first in sequence Distribution Policy Rule.
    export.Export1.Index = -1;

    foreach(var item in ReadDistributionPolicyRule())
    {
      ++export.Export1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.DistributionPolicyRule.Assign(
        entities.DistributionPolicyRule);
      UseFnSetDprFieldLiterals();

      if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
      {
        return;
      }
    }
  }

  private void UseFnSetDprFieldLiterals()
  {
    var useImport = new FnSetDprFieldLiterals.Import();
    var useExport = new FnSetDprFieldLiterals.Export();

    useImport.DistributionPolicyRule.Assign(
      export.Export1.Item.DistributionPolicyRule);

    Call(FnSetDprFieldLiterals.Execute, useImport, useExport);

    export.Export1.Update.FunctionType.Text13 = useExport.Function.Text13;
    export.Export1.Update.DebtState.TextLine10 = useExport.DebtState.TextLine10;
    export.Export1.Update.Apply.TextLine10 = useExport.Apply.TextLine10;
  }

  private bool ReadDistributionPolicy()
  {
    import.Persistent.Populated = false;

    return Read("ReadDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyId",
          import.DistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        import.Persistent.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        import.Persistent.Name = db.GetString(reader, 1);
        import.Persistent.EffectiveDt = db.GetDate(reader, 2);
        import.Persistent.DiscontinueDt = db.GetNullableDate(reader, 3);
        import.Persistent.MaximumProcessedDt = db.GetNullableDate(reader, 4);
        import.Persistent.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDistributionPolicyRule()
  {
    entities.DistributionPolicyRule.Populated = false;

    return ReadEach("ReadDistributionPolicyRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbpGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicyRule.DbpGeneratedId = db.GetInt32(reader, 0);
        entities.DistributionPolicyRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.DistributionPolicyRule.DebtFunctionType =
          db.GetString(reader, 2);
        entities.DistributionPolicyRule.DebtState = db.GetString(reader, 3);
        entities.DistributionPolicyRule.ApplyTo = db.GetString(reader, 4);
        entities.DistributionPolicyRule.DprNextId =
          db.GetNullableInt32(reader, 5);
        entities.DistributionPolicyRule.DistributeToOrderTypeCode =
          db.GetString(reader, 6);
        entities.DistributionPolicyRule.Populated = true;
        CheckValid<DistributionPolicyRule>("DebtFunctionType",
          entities.DistributionPolicyRule.DebtFunctionType);
        CheckValid<DistributionPolicyRule>("DebtState",
          entities.DistributionPolicyRule.DebtState);
        CheckValid<DistributionPolicyRule>("ApplyTo",
          entities.DistributionPolicyRule.ApplyTo);
        CheckValid<DistributionPolicyRule>("DistributeToOrderTypeCode",
          entities.DistributionPolicyRule.DistributeToOrderTypeCode);

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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public DistributionPolicy Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    private DistributionPolicy persistent;
    private DistributionPolicy distributionPolicy;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of DistributionPolicyRule.
      /// </summary>
      [JsonPropertyName("distributionPolicyRule")]
      public DistributionPolicyRule DistributionPolicyRule
      {
        get => distributionPolicyRule ??= new();
        set => distributionPolicyRule = value;
      }

      /// <summary>
      /// A value of FunctionType.
      /// </summary>
      [JsonPropertyName("functionType")]
      public WorkArea FunctionType
      {
        get => functionType ??= new();
        set => functionType = value;
      }

      /// <summary>
      /// A value of DebtState.
      /// </summary>
      [JsonPropertyName("debtState")]
      public ListScreenWorkArea DebtState
      {
        get => debtState ??= new();
        set => debtState = value;
      }

      /// <summary>
      /// A value of Apply.
      /// </summary>
      [JsonPropertyName("apply")]
      public ListScreenWorkArea Apply
      {
        get => apply ??= new();
        set => apply = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private DistributionPolicyRule distributionPolicyRule;
      private WorkArea functionType;
      private ListScreenWorkArea debtState;
      private ListScreenWorkArea apply;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public DistributionPolicyRule Last
    {
      get => last ??= new();
      set => last = value;
    }

    private Array<ExportGroup> export1;
    private DistributionPolicyRule last;
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
    public DistributionPolicyRule Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public DistributionPolicyRule First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of LastFound.
    /// </summary>
    [JsonPropertyName("lastFound")]
    public Common LastFound
    {
      get => lastFound ??= new();
      set => lastFound = value;
    }

    private DistributionPolicyRule current;
    private DistributionPolicyRule first;
    private Common lastFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
    }

    private DistributionPolicyRule distributionPolicyRule;
  }
#endregion
}
