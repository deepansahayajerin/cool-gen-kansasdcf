// Program: FN_UPDATE_INTERSTATE_RQST_OBLIGN, ID: 372159447, model: 746.
// Short name: SWE00009
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_INTERSTATE_RQST_OBLIGN.
/// </para>
/// <para>
/// Deletes the existing record and adds a new one.  The business rules enforced
/// by the procedure will only allow interstate request // obligation
/// information to be updated on the same day that it was created - and should,
/// therefore, treat it as if it had never existed.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateInterstateRqstOblign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_INTERSTATE_RQST_OBLIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateInterstateRqstOblign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateInterstateRqstOblign.
  /// </summary>
  public FnUpdateInterstateRqstOblign(IContext context, Import import,
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
    // =================================================
    // 2/23/1999 - Bud Adams  -  Creation
    // _________________________________________________
    //   Interstate information can only be updated on the same
    //   day that the obligation was created.  So, whenever this
    //   type of 'update' is being done, what we really want to do is
    //   Delete the old one (if there was one) and add the new one
    //   (if there is a new one).
    //   That is a business rule enforced in the PrADs.
    // =================================================
    if (ReadObligation())
    {
      if (import.Old.IntHGeneratedId == 0)
      {
      }
      else if (ReadInterstateRequest2())
      {
        if (ReadInterstateRequestObligation())
        {
          DeleteInterstateRequestObligation();
        }
        else
        {
          ExitState = "FN0000_INTERSTATE_REQ_OBLG_NF_RB";
        }
      }
      else
      {
        ExitState = "FN0000_INTERSTATE_REQUEST_NF_RB";
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (import.New1.IntHGeneratedId == 0)
    {
    }
    else if (ReadInterstateRequest1())
    {
      try
      {
        CreateInterstateRequestObligation();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0001_INT_REQ_OBLIG_AE_RB";

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
      ExitState = "FN0000_INTERSTATE_REQUEST_NF_RB";
    }
  }

  private void CreateInterstateRequestObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otyType = entities.Obligation.DtyGeneratedId;
    var cpaType = entities.Obligation.CpaType;
    var cspNumber = entities.Obligation.CspNumber;
    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var lastUpdatedTimestamp = local.Null1.LastUpdatedTimestamp;
    var lastUpdatedBy = local.Null1.LastUpdatedBy;
    var orderFreqAmount = 0M;
    var orderEffectiveDate = import.Current.Date;
    var orderEndDate = import.Max.Date;

    CheckValid<InterstateRequestObligation>("CpaType", cpaType);
    entities.InterstateRequestObligation.Populated = false;
    Update("CreateInterstateRequestObligation",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDecimal(command, "orderFreqAmount", orderFreqAmount);
        db.SetNullableDate(command, "orderEffDate", orderEffectiveDate);
        db.SetNullableDate(command, "orderEndDate", orderEndDate);
      });

    entities.InterstateRequestObligation.OtyType = otyType;
    entities.InterstateRequestObligation.CpaType = cpaType;
    entities.InterstateRequestObligation.CspNumber = cspNumber;
    entities.InterstateRequestObligation.ObgGeneratedId = obgGeneratedId;
    entities.InterstateRequestObligation.IntGeneratedId = intGeneratedId;
    entities.InterstateRequestObligation.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateRequestObligation.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequestObligation.OrderFreqAmount = orderFreqAmount;
    entities.InterstateRequestObligation.OrderEffectiveDate =
      orderEffectiveDate;
    entities.InterstateRequestObligation.OrderEndDate = orderEndDate;
    entities.InterstateRequestObligation.Populated = true;
  }

  private void DeleteInterstateRequestObligation()
  {
    Update("DeleteInterstateRequestObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.InterstateRequestObligation.OtyType);
        db.SetString(
          command, "cpaType", entities.InterstateRequestObligation.CpaType);
        db.SetString(
          command, "cspNumber", entities.InterstateRequestObligation.CspNumber);
          
        db.SetInt32(
          command, "obgGeneratedId",
          entities.InterstateRequestObligation.ObgGeneratedId);
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequestObligation.IntGeneratedId);
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.New1.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest2()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Old.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequestObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.InterstateRequestObligation.Populated = false;

    return Read("ReadInterstateRequestObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.InterstateRequestObligation.OtyType = db.GetInt32(reader, 0);
        entities.InterstateRequestObligation.CpaType = db.GetString(reader, 1);
        entities.InterstateRequestObligation.CspNumber =
          db.GetString(reader, 2);
        entities.InterstateRequestObligation.ObgGeneratedId =
          db.GetInt32(reader, 3);
        entities.InterstateRequestObligation.IntGeneratedId =
          db.GetInt32(reader, 4);
        entities.InterstateRequestObligation.LastUpdatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.InterstateRequestObligation.LastUpdatedBy =
          db.GetString(reader, 6);
        entities.InterstateRequestObligation.OrderFreqAmount =
          db.GetDecimal(reader, 7);
        entities.InterstateRequestObligation.OrderEffectiveDate =
          db.GetNullableDate(reader, 8);
        entities.InterstateRequestObligation.OrderEndDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestObligation.Populated = true;
        CheckValid<InterstateRequestObligation>("CpaType",
          entities.InterstateRequestObligation.CpaType);
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public InterstateRequest Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public InterstateRequest New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private DateWorkArea current;
    private DateWorkArea max;
    private InterstateRequest old;
    private InterstateRequest new1;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public InterstateRequestObligation Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private InterstateRequestObligation null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
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

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private InterstateRequestObligation interstateRequestObligation;
    private InterstateRequest interstateRequest;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
