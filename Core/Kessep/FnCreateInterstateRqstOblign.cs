// Program: FN_CREATE_INTERSTATE_RQST_OBLIGN, ID: 372159445, model: 746.
// Short name: SWE00007
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_INTERSTATE_RQST_OBLIGN.
/// </para>
/// <para>
/// Creates Interstate_Request_Obligation to resolve the many-to-many logical 
/// relationship between Obligation and Interstate_Request.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateInterstateRqstOblign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_INTERSTATE_RQST_OBLIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateInterstateRqstOblign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateInterstateRqstOblign.
  /// </summary>
  public FnCreateInterstateRqstOblign(IContext context, Import import,
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
    // Required imports:
    // 	Obligation
    // 		System_Generated_ID
    // 	Obligation_Type
    // 		System_Generated_ID
    // 	Interstate_Request
    // 		Int_H_Generated_ID
    // 	CSE_Person
    // 		Number
    // 	CSE_Person_Account
    // 		Type  (should be "R" - Obligor)
    // EXPORTS:
    // 	NONE
    // =================================================
    // =================================================
    // 2/23/1999 - Bud Adams  -  Creation
    // =================================================
    if (ReadObligation())
    {
      if (ReadInterstateRequest())
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
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";
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

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

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

    private DateWorkArea max;
    private DateWorkArea current;
    private ObligationType obligationType;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private InterstateRequest interstateRequest;
    private Obligation obligation;
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

    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private ObligationType obligationType;
    private InterstateRequestObligation interstateRequestObligation;
    private InterstateRequest interstateRequest;
    private Obligation obligation;
  }
#endregion
}
