// Program: FN_COPY_DISTRIBUTION_POLICY, ID: 371959582, model: 746.
// Short name: SWE00330
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
/// A program: FN_COPY_DISTRIBUTION_POLICY.
/// </para>
/// <para>
/// RESP: FINANCE
/// This Action Block copies a Distribution Policy, and all related entities.
/// </para>
/// </summary>
[Serializable]
public partial class FnCopyDistributionPolicy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COPY_DISTRIBUTION_POLICY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCopyDistributionPolicy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCopyDistributionPolicy.
  /// </summary>
  public FnCopyDistributionPolicy(IContext context, Import import, Export export)
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
    // ***********************************************************
    // 03/23/98	Siraj Konkader		ZDEL Cleanup.
    // ** MAIN-LINE AREA *****
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadDistributionPolicy())
    {
      // ** OK **
    }
    else
    {
      ExitState = "FN0000_DIST_PLCY_NF";

      return;
    }

    // : The Collection Type is the one that is to be associated with the NEW D.
    // P.
    //   The Collection Type does not have to be the same as the D.P. being 
    // copied.
    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      if (ReadCollectionType())
      {
        // ** OK **
      }
      else
      {
        ExitState = "FN0000_COLLECTION_TYPE_NF";

        return;
      }
    }

    try
    {
      CreateDistributionPolicy();

      // : Distribution Policy successfully created.
      export.DistributionPolicy.SystemGeneratedIdentifier =
        entities.NewDistributionPolicy.SystemGeneratedIdentifier;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DIST_PLCY_AE_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DIST_PLCY_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // : Read related entities, and associate new Distribution Policy to the 
    // same ones.
    //   - A Distribution Policy can have 1 or more Distribution Policy Rules.
    //   - A Distribution Policy Rule may have at least one Obligation type,
    //     and at least one Program.
    foreach(var item in ReadDistributionPolicyRule())
    {
      CreateDistributionPolicyRule();

      // ** OK **
      // : Any exception will cause an abort.
      // : Read all the Obligation Types associated to the existing Distribution
      // Policy Rule.
      foreach(var item1 in ReadObligationType())
      {
        AssociateDistributionPolicyRule();
      }

      // ***********************************************************
      //  Now associate the new Distribution Policy Rule with the rest of
      //   the programs associated to the existing Distribution Policy Rule.
      // sak: Deleted the following code
      // += READ EACH program
      // ¦	 WHERE DESIRED program zdel_describes CURRENT existing 
      // distribution_policy_rule
      // ¦  ASSOCIATE program
      // ¦	   WITH new distribution_policy_rule WHICH zdelescribed_by IT
      // +--
      // ************************************************************
    }

    // If the discontinue date of the existing Distribution Policy is max date, 
    // set it to the effective date of the new Distribution Policy.
    if (Equal(import.New1.DiscontinueDt, local.Maximum.Date))
    {
      UpdateDistributionPolicy();

      // ** OK **
      // : Any exception will cause an abort.
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseFnAssignDistPlcyId()
  {
    var useImport = new FnAssignDistPlcyId.Import();
    var useExport = new FnAssignDistPlcyId.Export();

    Call(FnAssignDistPlcyId.Execute, useImport, useExport);

    return useExport.DistributionPolicy.SystemGeneratedIdentifier;
  }

  private int UseFnAssignDistPlcyRuleId()
  {
    var useImport = new FnAssignDistPlcyRuleId.Import();
    var useExport = new FnAssignDistPlcyRuleId.Export();

    useImport.DistributionPolicy.Assign(entities.NewDistributionPolicy);

    Call(FnAssignDistPlcyRuleId.Execute, useImport, useExport);

    return useExport.DistributionPolicyRule.SystemGeneratedIdentifier;
  }

  private void AssociateDistributionPolicyRule()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewDistributionPolicyRule.Populated);
    Update("AssociateDistributionPolicyRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyRlId",
          entities.NewDistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.NewDistributionPolicyRule.DbpGeneratedId);
        db.SetInt32(
          command, "debtTypId",
          entities.ObligationType.SystemGeneratedIdentifier);
      });
  }

  private void CreateDistributionPolicy()
  {
    var systemGeneratedIdentifier = UseFnAssignDistPlcyId();
    var name = import.New1.Name;
    var effectiveDt = import.New1.EffectiveDt;
    var discontinueDt = import.New1.DiscontinueDt;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var cltIdentifier = import.P.SequentialIdentifier;
    var description = import.New1.Description;

    entities.NewDistributionPolicy.Populated = false;
    Update("CreateDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(command, "distPlcyId", systemGeneratedIdentifier);
        db.SetString(command, "distPlcyNm", name);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableDate(command, "maxPrcdDt", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetString(command, "distPlcyDsc", description);
      });

    entities.NewDistributionPolicy.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewDistributionPolicy.Name = name;
    entities.NewDistributionPolicy.EffectiveDt = effectiveDt;
    entities.NewDistributionPolicy.DiscontinueDt = discontinueDt;
    entities.NewDistributionPolicy.MaximumProcessedDt = null;
    entities.NewDistributionPolicy.CreatedBy = createdBy;
    entities.NewDistributionPolicy.CreatedTmst = createdTmst;
    entities.NewDistributionPolicy.LastUpdatedBy = "";
    entities.NewDistributionPolicy.LastUpdatedTmst = null;
    entities.NewDistributionPolicy.CltIdentifier = cltIdentifier;
    entities.NewDistributionPolicy.Description = description;
    entities.NewDistributionPolicy.Populated = true;
  }

  private void CreateDistributionPolicyRule()
  {
    var dbpGeneratedId =
      entities.NewDistributionPolicy.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseFnAssignDistPlcyRuleId();
    var debtFunctionType =
      entities.ExistingDistributionPolicyRule.DebtFunctionType;
    var debtState = entities.ExistingDistributionPolicyRule.DebtState;
    var applyTo = entities.ExistingDistributionPolicyRule.ApplyTo;
    var createdBy = global.UserId;
    var createdTmst = Now();

    CheckValid<DistributionPolicyRule>("DebtFunctionType", debtFunctionType);
    CheckValid<DistributionPolicyRule>("DebtState", debtState);
    CheckValid<DistributionPolicyRule>("ApplyTo", applyTo);
    entities.NewDistributionPolicyRule.Populated = false;

    // WARNING: An attribute SYSTEM_GENERATED_IDENTIFIER(371433964) refers to 
    // view EXISTING(371960348) instead of view (371960349).
    Update("CreateDistributionPolicyRule",
      (db, command) =>
      {
        db.SetInt32(command, "dbpGeneratedId", dbpGeneratedId);
        db.SetInt32(command, "distPlcyRlId", systemGeneratedIdentifier);
        db.SetNullableString(
          command, "firstLastInd", GetImplicitValue<DistributionPolicyRule,
          string>("FirstLastIndicator"));
        db.SetString(command, "debtFuncTyp", debtFunctionType);
        db.SetString(command, "debtState", debtState);
        db.SetString(command, "applyTo", applyTo);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "distToOrdTypCd", "");
      });

    entities.NewDistributionPolicyRule.DbpGeneratedId = dbpGeneratedId;
    entities.NewDistributionPolicyRule.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewDistributionPolicyRule.DebtFunctionType = debtFunctionType;
    entities.NewDistributionPolicyRule.DebtState = debtState;
    entities.NewDistributionPolicyRule.ApplyTo = applyTo;
    entities.NewDistributionPolicyRule.CreatedBy = createdBy;
    entities.NewDistributionPolicyRule.CreatedTmst = createdTmst;
    entities.NewDistributionPolicyRule.DprNextId = null;
    entities.NewDistributionPolicyRule.Populated = true;
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingDistributionPolicy.Populated);
    import.P.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.ExistingDistributionPolicy.CltIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        import.P.SequentialIdentifier = db.GetInt32(reader, 0);
        import.P.Code = db.GetString(reader, 1);
        import.P.Name = db.GetString(reader, 2);
        import.P.EffectiveDate = db.GetDate(reader, 3);
        import.P.DiscontinueDate = db.GetNullableDate(reader, 4);
        import.P.Populated = true;
      });
  }

  private bool ReadDistributionPolicy()
  {
    entities.ExistingDistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy",
      (db, command) =>
      {
        db.
          SetInt32(command, "distPlcyId", import.New1.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingDistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingDistributionPolicy.Name = db.GetString(reader, 1);
        entities.ExistingDistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.ExistingDistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.ExistingDistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.ExistingDistributionPolicy.CreatedBy = db.GetString(reader, 5);
        entities.ExistingDistributionPolicy.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.ExistingDistributionPolicy.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.ExistingDistributionPolicy.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ExistingDistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.ExistingDistributionPolicy.Description =
          db.GetString(reader, 10);
        entities.ExistingDistributionPolicy.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDistributionPolicyRule()
  {
    entities.ExistingDistributionPolicyRule.Populated = false;

    return ReadEach("ReadDistributionPolicyRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingDistributionPolicyRule.DbpGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingDistributionPolicyRule.DebtFunctionType =
          db.GetString(reader, 2);
        entities.ExistingDistributionPolicyRule.DebtState =
          db.GetString(reader, 3);
        entities.ExistingDistributionPolicyRule.ApplyTo =
          db.GetString(reader, 4);
        entities.ExistingDistributionPolicyRule.CreatedBy =
          db.GetString(reader, 5);
        entities.ExistingDistributionPolicyRule.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.ExistingDistributionPolicyRule.DprNextId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingDistributionPolicyRule.Populated = true;
        CheckValid<DistributionPolicyRule>("DebtFunctionType",
          entities.ExistingDistributionPolicyRule.DebtFunctionType);
        CheckValid<DistributionPolicyRule>("DebtState",
          entities.ExistingDistributionPolicyRule.DebtState);
        CheckValid<DistributionPolicyRule>("ApplyTo",
          entities.ExistingDistributionPolicyRule.ApplyTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingDistributionPolicyRule.Populated);
    entities.ObligationType.Populated = false;

    return ReadEach("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dprGeneratedId",
          entities.ExistingDistributionPolicyRule.DbpGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Populated = true;

        return true;
      });
  }

  private void UpdateDistributionPolicy()
  {
    var discontinueDt = import.New1.EffectiveDt;

    entities.ExistingDistributionPolicy.Populated = false;
    Update("UpdateDistributionPolicy",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetInt32(
          command, "distPlcyId",
          entities.ExistingDistributionPolicy.SystemGeneratedIdentifier);
      });

    entities.ExistingDistributionPolicy.DiscontinueDt = discontinueDt;
    entities.ExistingDistributionPolicy.Populated = true;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public DistributionPolicy Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DistributionPolicy New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public CollectionType P
    {
      get => p ??= new();
      set => p = value;
    }

    private DistributionPolicy existing;
    private DistributionPolicy new1;
    private CollectionType p;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private DistributionPolicy distributionPolicy;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Created.
    /// </summary>
    [JsonPropertyName("created")]
    public Common Created
    {
      get => created ??= new();
      set => created = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public Program First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private Common created;
    private Program first;
    private DateWorkArea maximum;
    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingDistributionPolicy.
    /// </summary>
    [JsonPropertyName("existingDistributionPolicy")]
    public DistributionPolicy ExistingDistributionPolicy
    {
      get => existingDistributionPolicy ??= new();
      set => existingDistributionPolicy = value;
    }

    /// <summary>
    /// A value of NewDistributionPolicy.
    /// </summary>
    [JsonPropertyName("newDistributionPolicy")]
    public DistributionPolicy NewDistributionPolicy
    {
      get => newDistributionPolicy ??= new();
      set => newDistributionPolicy = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ExistingDistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("existingDistributionPolicyRule")]
    public DistributionPolicyRule ExistingDistributionPolicyRule
    {
      get => existingDistributionPolicyRule ??= new();
      set => existingDistributionPolicyRule = value;
    }

    /// <summary>
    /// A value of NewDistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("newDistributionPolicyRule")]
    public DistributionPolicyRule NewDistributionPolicyRule
    {
      get => newDistributionPolicyRule ??= new();
      set => newDistributionPolicyRule = value;
    }

    private DistributionPolicy existingDistributionPolicy;
    private DistributionPolicy newDistributionPolicy;
    private Program program;
    private ObligationType obligationType;
    private DistributionPolicyRule existingDistributionPolicyRule;
    private DistributionPolicyRule newDistributionPolicyRule;
  }
#endregion
}
