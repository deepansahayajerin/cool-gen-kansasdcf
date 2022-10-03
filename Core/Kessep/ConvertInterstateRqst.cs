// Program: CONVERT_INTERSTATE_RQST, ID: 373309586, model: 746.
// Short name: CONVERTI
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CONVERT_INTERSTATE_RQST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class ConvertInterstateRqst: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CONVERT_INTERSTATE_RQST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ConvertInterstateRqst(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ConvertInterstateRqst.
  /// </summary>
  public ConvertInterstateRqst(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";

    // ----------------------------First Pass
    // ---------------------------------------------
    // Convert the history direction indicator
    // Set the direction indicator to spaces if the action_reason_code is equal 
    // to OICNV or IICNV
    // --------------------------------------------------------------------------------------
    foreach(var item in ReadInterstateRequestHistory2())
    {
      if (Equal(entities.InterstateRequestHistory.CreatedBy, "SWEIB295"))
      {
        continue;
      }

      try
      {
        UpdateInterstateRequestHistory3();
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

    // --------------------------Second Pass
    // -------------------------------------
    // Set Created_By to 'SWEIOINR' if the functional_type_code is equal to '
    // PAT' or 'EST' or 'ENF'.
    // -------------------------------------------------------------------------------------
    foreach(var item in ReadInterstateRequestHistory3())
    {
      if (Equal(entities.InterstateRequestHistory.CreatedBy, "SWEIB295"))
      {
        continue;
      }

      try
      {
        UpdateInterstateRequestHistory2();
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

    // -------------------------Third  Pass
    // -------------------------------------
    // Set Created_By to 'SWEIIIMC' if the action_reason_code is equal to IICNV'
    // Set Created_By to 'SWEIOINR' if the action_reason_code is equal to '
    // OICNV'..
    // -------------------------------------------------------------------------------------
    foreach(var item in ReadInterstateRequestHistory1())
    {
      if (Equal(entities.InterstateRequestHistory.CreatedBy, "SWEIB295"))
      {
        continue;
      }

      if (Equal(entities.InterstateRequestHistory.ActionReasonCode, "OICNV"))
      {
        try
        {
          UpdateInterstateRequestHistory2();
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
      else
      {
        try
        {
          UpdateInterstateRequestHistory1();
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
    }

    // -------------------------Fourth  Pass
    // -------------------------------------
    // Set KS_Ind to spaces' if the Function_type_code is equal to 'CSI' or '
    // LO1'. If there are more than one history record associated with the
    // request, do not update.
    // -------------------------------------------------------------------------------------
  }

  private IEnumerable<bool> ReadInterstateRequestHistory1()
  {
    entities.InterstateRequestHistory.Populated = false;

    return ReadEach("ReadInterstateRequestHistory1",
      null,
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 4);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 5);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 6);
        entities.InterstateRequestHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestHistory2()
  {
    entities.InterstateRequestHistory.Populated = false;

    return ReadEach("ReadInterstateRequestHistory2",
      null,
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 4);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 5);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 6);
        entities.InterstateRequestHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestHistory3()
  {
    entities.InterstateRequestHistory.Populated = false;

    return ReadEach("ReadInterstateRequestHistory3",
      null,
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 4);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 5);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 6);
        entities.InterstateRequestHistory.Populated = true;

        return true;
      });
  }

  private void UpdateInterstateRequestHistory1()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateRequestHistory.Populated);

    var createdBy = "SWEIIIMC";

    entities.InterstateRequestHistory.Populated = false;
    Update("UpdateInterstateRequestHistory1",
      (db, command) =>
      {
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequestHistory.IntGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          entities.InterstateRequestHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.InterstateRequestHistory.CreatedBy = createdBy;
    entities.InterstateRequestHistory.Populated = true;
  }

  private void UpdateInterstateRequestHistory2()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateRequestHistory.Populated);

    var createdBy = "SWEIOINR";

    entities.InterstateRequestHistory.Populated = false;
    Update("UpdateInterstateRequestHistory2",
      (db, command) =>
      {
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequestHistory.IntGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          entities.InterstateRequestHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.InterstateRequestHistory.CreatedBy = createdBy;
    entities.InterstateRequestHistory.Populated = true;
  }

  private void UpdateInterstateRequestHistory3()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateRequestHistory.Populated);
    entities.InterstateRequestHistory.Populated = false;
    Update("UpdateInterstateRequestHistory3",
      (db, command) =>
      {
        db.SetString(command, "transactionDirect", "");
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequestHistory.IntGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          entities.InterstateRequestHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.InterstateRequestHistory.TransactionDirectionInd = "";
    entities.InterstateRequestHistory.Populated = true;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateRequestHistory G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private InterstateRequestHistory g;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private Array<GroupGroup> group;
    private Common common;
    private InterstateRequest interstateRequest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
  }
#endregion
}
