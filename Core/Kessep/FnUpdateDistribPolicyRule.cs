// Program: FN_UPDATE_DISTRIB_POLICY_RULE, ID: 371962155, model: 746.
// Short name: SWE00650
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_DISTRIB_POLICY_RULE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block updates a Distribution Policy Rule.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateDistribPolicyRule: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_DISTRIB_POLICY_RULE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateDistribPolicyRule(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateDistribPolicyRule.
  /// </summary>
  public FnUpdateDistribPolicyRule(IContext context, Import import,
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

    if (ReadDistributionPolicyRule())
    {
      // : Distribution Policy Rule successfully retrieved.
    }
    else
    {
      ExitState = "FN0000_DIST_PLCY_RULE_NF";
    }

    // ***** MAIN-LINE AREA *****
    try
    {
      UpdateDistributionPolicyRule();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DIST_PLCY_RULE_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DIST_PLCY_RULE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
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

  private bool ReadDistributionPolicyRule()
  {
    entities.DistributionPolicyRule.Populated = false;

    return Read("ReadDistributionPolicyRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyRlId",
          import.DistributionPolicyRule.SystemGeneratedIdentifier);
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
        entities.DistributionPolicyRule.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DistributionPolicyRule.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DistributionPolicyRule.DprNextId =
          db.GetNullableInt32(reader, 7);
        entities.DistributionPolicyRule.DistributeToOrderTypeCode =
          db.GetString(reader, 8);
        entities.DistributionPolicyRule.Populated = true;
        CheckValid<DistributionPolicyRule>("DebtFunctionType",
          entities.DistributionPolicyRule.DebtFunctionType);
        CheckValid<DistributionPolicyRule>("DebtState",
          entities.DistributionPolicyRule.DebtState);
        CheckValid<DistributionPolicyRule>("ApplyTo",
          entities.DistributionPolicyRule.ApplyTo);
        CheckValid<DistributionPolicyRule>("DistributeToOrderTypeCode",
          entities.DistributionPolicyRule.DistributeToOrderTypeCode);
      });
  }

  private void UpdateDistributionPolicyRule()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);

    var debtFunctionType = import.DistributionPolicyRule.DebtFunctionType;
    var debtState = import.DistributionPolicyRule.DebtState;
    var applyTo = import.DistributionPolicyRule.ApplyTo;
    var lastUpdatedBy = import.DistributionPolicyRule.LastUpdatedBy ?? "";
    var lastUpdatedTmst = import.DistributionPolicyRule.LastUpdatedTmst;
    var distributeToOrderTypeCode =
      import.DistributionPolicyRule.DistributeToOrderTypeCode;

    CheckValid<DistributionPolicyRule>("DebtFunctionType", debtFunctionType);
    CheckValid<DistributionPolicyRule>("DebtState", debtState);
    CheckValid<DistributionPolicyRule>("ApplyTo", applyTo);
    CheckValid<DistributionPolicyRule>("DistributeToOrderTypeCode",
      distributeToOrderTypeCode);
    entities.DistributionPolicyRule.Populated = false;
    Update("UpdateDistributionPolicyRule",
      (db, command) =>
      {
        db.SetString(command, "debtFuncTyp", debtFunctionType);
        db.SetString(command, "debtState", debtState);
        db.SetString(command, "applyTo", applyTo);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "distToOrdTypCd", distributeToOrderTypeCode);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.DistributionPolicyRule.DbpGeneratedId);
        db.SetInt32(
          command, "distPlcyRlId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
      });

    entities.DistributionPolicyRule.DebtFunctionType = debtFunctionType;
    entities.DistributionPolicyRule.DebtState = debtState;
    entities.DistributionPolicyRule.ApplyTo = applyTo;
    entities.DistributionPolicyRule.LastUpdatedBy = lastUpdatedBy;
    entities.DistributionPolicyRule.LastUpdatedTmst = lastUpdatedTmst;
    entities.DistributionPolicyRule.DistributeToOrderTypeCode =
      distributeToOrderTypeCode;
    entities.DistributionPolicyRule.Populated = true;
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
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public DistributionPolicy Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    private DistributionPolicy distributionPolicy;
    private DistributionPolicyRule distributionPolicyRule;
    private DistributionPolicy persistent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
