// Program: FN_CREATE_DISTRIB_POLICY_RULE, ID: 371962156, model: 746.
// Short name: SWE00366
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
/// A program: FN_CREATE_DISTRIB_POLICY_RULE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block creates a Distribution Policy Rule.  It Associates the 
/// Distribution Policy Rule to one or more Obligation Types nd Programs.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateDistribPolicyRule: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_DISTRIB_POLICY_RULE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateDistribPolicyRule(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateDistribPolicyRule.
  /// </summary>
  public FnCreateDistribPolicyRule(IContext context, Import import,
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
    if (ReadDistributionPolicy())
    {
      if (!Equal(entities.DistributionPolicy.MaximumProcessedDt,
        local.Initialized.Date))
      {
        ExitState = "CANNOT_CHANGE_DUE_TO_ACTIVITY";

        return;
      }

      if (Lt(entities.DistributionPolicy.EffectiveDt, Now().Date))
      {
        ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

        return;
      }

      if (!Lt(Now().Date, entities.DistributionPolicy.DiscontinueDt))
      {
        ExitState = "FN0000_DIST_PLCY_DISCONTINUED";

        return;
      }

      // : Distribution Policy successfully retrieved.
    }
    else
    {
      ExitState = "FN0000_DIST_PLCY_NF";

      return;
    }

    try
    {
      CreateDistributionPolicyRule();

      // : Distribution Policy Rule successfully created.
      export.DistributionPolicyRule.SystemGeneratedIdentifier =
        entities.DistributionPolicyRule.SystemGeneratedIdentifier;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DIST_PLCY_RULE_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DIST_PLCY_RULE_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // : Distribution Policy Rule has been created.
    //   Now Associate each Program and Obligation Type.
    for(import.Program.Index = 0; import.Program.Index < import.Program.Count; ++
      import.Program.Index)
    {
      if (ReadProgram())
      {
        // : Make sure the dates are compatible.
        if (!Lt(entities.Program1.EffectiveDate,
          entities.DistributionPolicy.EffectiveDt))
        {
          ExitState = "EFFECTIVE_DATE_LESS_THAN_ALLWD";

          return;
        }

        if (Lt(entities.Program1.DiscontinueDate,
          entities.DistributionPolicy.DiscontinueDt))
        {
          ExitState = "DISCONTINUE_DATE_GRTR_THAN_ALLWD";

          return;
        }

        try
        {
          CreateDprProgram();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CO0000_DPR_PROGRAM_AE_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CO0000_DPR_PROGRAM_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // : Not Found exception will cause the program to abort.
      }
      else
      {
        ExitState = "ZD_FN0000_PROGRAM_NF_AB";

        return;
      }
    }

    for(import.Ot.Index = 0; import.Ot.Index < import.Ot.Count; ++
      import.Ot.Index)
    {
      if (ReadObligationType())
      {
        // : Make sure the dates are compatible.
        if (!Lt(entities.ObligationType.EffectiveDt,
          entities.DistributionPolicy.EffectiveDt))
        {
          ExitState = "EFFECTIVE_DATE_LESS_THAN_ALLWD";

          return;
        }

        if (Lt(entities.ObligationType.DiscontinueDt,
          entities.DistributionPolicy.DiscontinueDt))
        {
          ExitState = "DISCONTINUE_DATE_GRTR_THAN_ALLWD";

          return;
        }

        try
        {
          CreateDprObligType();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CO0000_DPR_OBLIG_TYPE_AE_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CO0000_DPR_OBLIG_TYPE_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // : Not Found exception will cause the program to abort.
      }
      else
      {
        ExitState = "FN0000_OBLIG_TYPE_NF_RB";

        return;
      }
    }
  }

  private int UseFnAssignDistPlcyRuleId()
  {
    var useImport = new FnAssignDistPlcyRuleId.Import();
    var useExport = new FnAssignDistPlcyRuleId.Export();

    useImport.DistributionPolicy.Assign(entities.DistributionPolicy);

    Call(FnAssignDistPlcyRuleId.Execute, useImport, useExport);

    return useExport.DistributionPolicyRule.SystemGeneratedIdentifier;
  }

  private void CreateDistributionPolicyRule()
  {
    var dbpGeneratedId = entities.DistributionPolicy.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseFnAssignDistPlcyRuleId();
    var debtFunctionType = import.DistributionPolicyRule.DebtFunctionType;
    var debtState = import.DistributionPolicyRule.DebtState;
    var applyTo = import.DistributionPolicyRule.ApplyTo;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var distributeToOrderTypeCode =
      import.DistributionPolicyRule.DistributeToOrderTypeCode;

    CheckValid<DistributionPolicyRule>("DebtFunctionType", debtFunctionType);
    CheckValid<DistributionPolicyRule>("DebtState", debtState);
    CheckValid<DistributionPolicyRule>("ApplyTo", applyTo);
    CheckValid<DistributionPolicyRule>("DistributeToOrderTypeCode",
      distributeToOrderTypeCode);
    entities.DistributionPolicyRule.Populated = false;
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
        db.SetString(command, "distToOrdTypCd", distributeToOrderTypeCode);
      });

    entities.DistributionPolicyRule.DbpGeneratedId = dbpGeneratedId;
    entities.DistributionPolicyRule.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DistributionPolicyRule.DebtFunctionType = debtFunctionType;
    entities.DistributionPolicyRule.DebtState = debtState;
    entities.DistributionPolicyRule.ApplyTo = applyTo;
    entities.DistributionPolicyRule.CreatedBy = createdBy;
    entities.DistributionPolicyRule.CreatedTmst = createdTmst;
    entities.DistributionPolicyRule.DprNextId = null;
    entities.DistributionPolicyRule.DistributeToOrderTypeCode =
      distributeToOrderTypeCode;
    entities.DistributionPolicyRule.Populated = true;
  }

  private void CreateDprObligType()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);

    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var otyGeneratedId = entities.ObligationType.SystemGeneratedIdentifier;
    var dbpGeneratedId = entities.DistributionPolicyRule.DbpGeneratedId;
    var dprGeneratedId =
      entities.DistributionPolicyRule.SystemGeneratedIdentifier;

    entities.DprObligType.Populated = false;
    Update("CreateDprObligType",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetInt32(command, "otyGeneratedId", otyGeneratedId);
        db.SetInt32(command, "dbpGeneratedId", dbpGeneratedId);
        db.SetInt32(command, "dprGeneratedId", dprGeneratedId);
      });

    entities.DprObligType.CreatedBy = createdBy;
    entities.DprObligType.CreatedTimestamp = createdTimestamp;
    entities.DprObligType.OtyGeneratedId = otyGeneratedId;
    entities.DprObligType.DbpGeneratedId = dbpGeneratedId;
    entities.DprObligType.DprGeneratedId = dprGeneratedId;
    entities.DprObligType.Populated = true;
  }

  private void CreateDprProgram()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);

    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var dbpGeneratedId = entities.DistributionPolicyRule.DbpGeneratedId;
    var dprGeneratedId =
      entities.DistributionPolicyRule.SystemGeneratedIdentifier;
    var prgGeneratedId = entities.Program1.SystemGeneratedIdentifier;

    entities.DprProgram.Populated = false;
    Update("CreateDprProgram",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetInt32(command, "dbpGeneratedId", dbpGeneratedId);
        db.SetInt32(command, "dprGeneratedId", dprGeneratedId);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
        db.SetString(command, "programState", "");
        db.SetNullableString(command, "assistanceInd", "");
      });

    entities.DprProgram.CreatedBy = createdBy;
    entities.DprProgram.CreatedTimestamp = createdTimestamp;
    entities.DprProgram.DbpGeneratedId = dbpGeneratedId;
    entities.DprProgram.DprGeneratedId = dprGeneratedId;
    entities.DprProgram.PrgGeneratedId = prgGeneratedId;
    entities.DprProgram.ProgramState = "";
    entities.DprProgram.Populated = true;
  }

  private bool ReadDistributionPolicy()
  {
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyId",
          import.DistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          import.Ot.Item.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 1);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 2);
        entities.ObligationType.Populated = true;
      });
  }

  private bool ReadProgram()
  {
    entities.Program1.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId",
          import.Program.Item.Program1.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Program1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program1.EffectiveDate = db.GetDate(reader, 1);
        entities.Program1.DiscontinueDate = db.GetDate(reader, 2);
        entities.Program1.Populated = true;
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
    /// <summary>A OtGroup group.</summary>
    [Serializable]
    public class OtGroup
    {
      /// <summary>
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType obligationType;
    }

    /// <summary>A ProgramGroup group.</summary>
    [Serializable]
    public class ProgramGroup
    {
      /// <summary>
      /// A value of Program1.
      /// </summary>
      [JsonPropertyName("program1")]
      public Program Program1
      {
        get => program1 ??= new();
        set => program1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program program1;
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
    /// Gets a value of Ot.
    /// </summary>
    [JsonIgnore]
    public Array<OtGroup> Ot => ot ??= new(OtGroup.Capacity);

    /// <summary>
    /// Gets a value of Ot for json serialization.
    /// </summary>
    [JsonPropertyName("ot")]
    [Computed]
    public IList<OtGroup> Ot_Json
    {
      get => ot;
      set => Ot.Assign(value);
    }

    /// <summary>
    /// Gets a value of Program.
    /// </summary>
    [JsonIgnore]
    public Array<ProgramGroup> Program =>
      program ??= new(ProgramGroup.Capacity);

    /// <summary>
    /// Gets a value of Program for json serialization.
    /// </summary>
    [JsonPropertyName("program")]
    [Computed]
    public IList<ProgramGroup> Program_Json
    {
      get => program;
      set => Program.Assign(value);
    }

    private DistributionPolicy distributionPolicy;
    private DistributionPolicyRule distributionPolicyRule;
    private Array<OtGroup> ot;
    private Array<ProgramGroup> program;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    /// <summary>
    /// A value of DprObligType.
    /// </summary>
    [JsonPropertyName("dprObligType")]
    public DprObligType DprObligType
    {
      get => dprObligType ??= new();
      set => dprObligType = value;
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
    /// A value of Program1.
    /// </summary>
    [JsonPropertyName("program1")]
    public Program Program1
    {
      get => program1 ??= new();
      set => program1 = value;
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
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public DistributionPolicyRule Last
    {
      get => last ??= new();
      set => last = value;
    }

    private DprProgram dprProgram;
    private DprObligType dprObligType;
    private DistributionPolicy distributionPolicy;
    private ObligationType obligationType;
    private Program program1;
    private DistributionPolicyRule distributionPolicyRule;
    private DistributionPolicyRule last;
  }
#endregion
}
