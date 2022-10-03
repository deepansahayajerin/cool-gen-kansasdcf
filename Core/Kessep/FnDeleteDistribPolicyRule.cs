// Program: FN_DELETE_DISTRIB_POLICY_RULE, ID: 371962159, model: 746.
// Short name: SWE00411
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
/// A program: FN_DELETE_DISTRIB_POLICY_RULE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block deletes a Distribution Policy Rule.
/// </para>
/// </summary>
[Serializable]
public partial class FnDeleteDistribPolicyRule: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_DISTRIB_POLICY_RULE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteDistribPolicyRule(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteDistribPolicyRule.
  /// </summary>
  public FnDeleteDistribPolicyRule(IContext context, Import import,
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
    // : Get Distribution Policy.
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
      if (!Equal(import.Persistent.MaximumProcessedDt, local.NullDate.Date) || Lt
        (import.Persistent.EffectiveDt, Now().Date))
      {
        ExitState = "CANNOT_CHANGE_DUE_TO_ACTIVITY";

        return;
      }

      // *****
      // Deleting the DPR entities will automatically disassociate them from the
      // program and Obligation Types as well as the DPR.  After all DPR
      // entities have been deleted the DPR can be deleted.
      // *****
      foreach(var item in ReadDprObligType())
      {
        DeleteDprObligType();
      }

      foreach(var item in ReadDprProgram())
      {
        DeleteDprProgram();
      }

      // : Distribution Policy Rule successfully retrieved.
      // NOT FOUND exception will result in abort.
    }

    DeleteDistributionPolicyRule();
  }

  private void DeleteDistributionPolicyRule()
  {
    Update("DeleteDistributionPolicyRule#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dbpNextId", entities.DistributionPolicyRule.DbpGeneratedId);
          
        db.SetNullableInt32(
          command, "dprNextId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
      });

    Update("DeleteDistributionPolicyRule#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dbpNextId", entities.DistributionPolicyRule.DbpGeneratedId);
          
        db.SetNullableInt32(
          command, "dprNextId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
      });

    Update("DeleteDistributionPolicyRule#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dbpNextId", entities.DistributionPolicyRule.DbpGeneratedId);
          
        db.SetNullableInt32(
          command, "dprNextId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
      });

    Update("DeleteDistributionPolicyRule#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dbpNextId", entities.DistributionPolicyRule.DbpGeneratedId);
          
        db.SetNullableInt32(
          command, "dprNextId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
      });
  }

  private void DeleteDprObligType()
  {
    Update("DeleteDprObligType",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyGeneratedId", entities.DprObligType.OtyGeneratedId);
        db.SetInt32(
          command, "dbpGeneratedId", entities.DprObligType.DbpGeneratedId);
        db.SetInt32(
          command, "dprGeneratedId", entities.DprObligType.DprGeneratedId);
      });
  }

  private void DeleteDprProgram()
  {
    Update("DeleteDprProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbpGeneratedId", entities.DprProgram.DbpGeneratedId);
        db.SetInt32(
          command, "dprGeneratedId", entities.DprProgram.DprGeneratedId);
        db.SetInt32(
          command, "prgGeneratedId", entities.DprProgram.PrgGeneratedId);
        db.SetString(command, "programState", entities.DprProgram.ProgramState);
      });
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
        entities.DistributionPolicyRule.DprNextId =
          db.GetNullableInt32(reader, 2);
        entities.DistributionPolicyRule.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDprObligType()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);
    entities.DprObligType.Populated = false;

    return ReadEach("ReadDprObligType",
      (db, command) =>
      {
        db.SetInt32(
          command, "dprGeneratedId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.DistributionPolicyRule.DbpGeneratedId);
      },
      (db, reader) =>
      {
        entities.DprObligType.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.DprObligType.OtyGeneratedId = db.GetInt32(reader, 1);
        entities.DprObligType.DbpGeneratedId = db.GetInt32(reader, 2);
        entities.DprObligType.DprGeneratedId = db.GetInt32(reader, 3);
        entities.DprObligType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDprProgram()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);
    entities.DprProgram.Populated = false;

    return ReadEach("ReadDprProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "dprGeneratedId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.DistributionPolicyRule.DbpGeneratedId);
      },
      (db, reader) =>
      {
        entities.DprProgram.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.DprProgram.DbpGeneratedId = db.GetInt32(reader, 1);
        entities.DprProgram.DprGeneratedId = db.GetInt32(reader, 2);
        entities.DprProgram.PrgGeneratedId = db.GetInt32(reader, 3);
        entities.DprProgram.ProgramState = db.GetString(reader, 4);
        entities.DprProgram.Populated = true;

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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public NullDate NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    private NullDate nullDate;
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
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public DistributionPolicyRule Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public DistributionPolicyRule Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    private DprProgram dprProgram;
    private DprObligType dprObligType;
    private DistributionPolicyRule distributionPolicyRule;
    private DistributionPolicyRule next;
    private DistributionPolicyRule previous;
  }
#endregion
}
