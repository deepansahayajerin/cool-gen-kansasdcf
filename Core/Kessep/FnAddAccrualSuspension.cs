// Program: FN_ADD_ACCRUAL_SUSPENSION, ID: 372082711, model: 746.
// Short name: SWE00512
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_ADD_ACCRUAL_SUSPENSION.
/// </summary>
[Serializable]
public partial class FnAddAccrualSuspension: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ADD_ACCRUAL_SUSPENSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAddAccrualSuspension(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAddAccrualSuspension.
  /// </summary>
  public FnAddAccrualSuspension(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***11/18/98  Bud Adams     Removed Export View
    // ***7/22/99 - Bud Adams  -  Removed Read of CSE_Person,
    // ***  CSE_Person_Account, and Obligation.  Their existence
    // ***  has already been validated; currency not needed.
    // ***
    // ***12/9/99 - bud adams  -  removed obsolete views.
    // *** 02/24/2000  PR# 88880  Eliminate the overlapping time frames on 
    // multiple suspensions.
    if (ReadObligationTransaction())
    {
      if (ReadAccrualInstructions())
      {
        if (!Lt(entities.AccrualInstructions.LastAccrualDt,
          import.AccrualSuspension.SuspendDt))
        {
          ExitState = "FN0000_ACCRUAL_SUS_DATE_LESS_LST";
        }

        if (!Lt(import.AccrualSuspension.SuspendDt,
          entities.AccrualInstructions.DiscontinueDt) && AsChar
          (import.SuspendFromDayOne.Flag) != 'Y')
        {
          ExitState = "FN0000_SUSP_DT_AFTER_DISCNT_DATE";
        }
      }
      else
      {
        ExitState = "CO0000_ACCRUAL_INSTRUCTN_NF";
      }
    }
    else
    {
      ExitState = "FN0000_OBLIG_TRANS_NF";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // : Aug 5, 1999, mfb - Removed the 'desired current accrual_suspension 
    // system_generated_identifier is not equal to import accrual_suspension
    // system_generated_identifier'.  It was causing duplicates to be created.
    if (ReadAccrualSuspension())
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";

      return;
    }
    else
    {
      // *****
      // OK
      // *****
    }

    try
    {
      CreateAccrualSuspension();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_ACCRUAL_SUSPENSION_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_ACCRUAL_SUSPENSION_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private int UseFnCabSetAccrualSuspensionId()
  {
    var useImport = new FnCabSetAccrualSuspensionId.Import();
    var useExport = new FnCabSetAccrualSuspensionId.Export();

    Call(FnCabSetAccrualSuspensionId.Execute, useImport, useExport);

    return useExport.AccrualSuspension.SystemGeneratedIdentifier;
  }

  private void CreateAccrualSuspension()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);

    var systemGeneratedIdentifier = UseFnCabSetAccrualSuspensionId();
    var suspendDt = import.AccrualSuspension.SuspendDt;
    var resumeDt = import.AccrualSuspension.ResumeDt;
    var reductionPercentage =
      import.AccrualSuspension.ReductionPercentage.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTmst = import.AccrualSuspension.CreatedTmst;
    var otrType = entities.AccrualInstructions.OtrType;
    var otyId = entities.AccrualInstructions.OtyId;
    var obgId = entities.AccrualInstructions.ObgGeneratedId;
    var cspNumber = entities.AccrualInstructions.CspNumber;
    var cpaType = entities.AccrualInstructions.CpaType;
    var otrId = entities.AccrualInstructions.OtrGeneratedId;
    var reductionAmount =
      import.AccrualSuspension.ReductionAmount.GetValueOrDefault();
    var reasonTxt = import.AccrualSuspension.ReasonTxt ?? "";

    CheckValid<AccrualSuspension>("OtrType", otrType);
    CheckValid<AccrualSuspension>("CpaType", cpaType);
    entities.AccrualSuspension.Populated = false;
    Update("CreateAccrualSuspension",
      (db, command) =>
      {
        db.SetInt32(command, "frqSuspId", systemGeneratedIdentifier);
        db.SetDate(command, "suspendDt", suspendDt);
        db.SetNullableDate(command, "resumeDt", resumeDt);
        db.SetNullableDecimal(command, "redPct", reductionPercentage);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otyId", otyId);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otrId", otrId);
        db.SetNullableDecimal(command, "reductionAmount", reductionAmount);
        db.SetNullableString(command, "frqSuspRsnTxt", reasonTxt);
      });

    entities.AccrualSuspension.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.AccrualSuspension.SuspendDt = suspendDt;
    entities.AccrualSuspension.ResumeDt = resumeDt;
    entities.AccrualSuspension.ReductionPercentage = reductionPercentage;
    entities.AccrualSuspension.CreatedBy = createdBy;
    entities.AccrualSuspension.CreatedTmst = createdTmst;
    entities.AccrualSuspension.OtrType = otrType;
    entities.AccrualSuspension.OtyId = otyId;
    entities.AccrualSuspension.ObgId = obgId;
    entities.AccrualSuspension.CspNumber = cspNumber;
    entities.AccrualSuspension.CpaType = cpaType;
    entities.AccrualSuspension.OtrId = otrId;
    entities.AccrualSuspension.ReductionAmount = reductionAmount;
    entities.AccrualSuspension.ReasonTxt = reasonTxt;
    entities.AccrualSuspension.Populated = true;
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadAccrualSuspension()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Current.Populated = false;

    return Read("ReadAccrualSuspension",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgId", entities.ObligationTransaction.ObgGeneratedId);
        db.SetDate(
          command, "suspendDt",
          import.AccrualSuspension.ResumeDt.GetValueOrDefault());
        db.SetNullableDate(
          command, "resumeDt",
          import.AccrualSuspension.SuspendDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Current.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Current.SuspendDt = db.GetDate(reader, 1);
        entities.Current.ResumeDt = db.GetNullableDate(reader, 2);
        entities.Current.OtrType = db.GetString(reader, 3);
        entities.Current.OtyId = db.GetInt32(reader, 4);
        entities.Current.ObgId = db.GetInt32(reader, 5);
        entities.Current.CspNumber = db.GetString(reader, 6);
        entities.Current.CpaType = db.GetString(reader, 7);
        entities.Current.OtrId = db.GetInt32(reader, 8);
        entities.Current.Populated = true;
        CheckValid<AccrualSuspension>("OtrType", entities.Current.OtrType);
        CheckValid<AccrualSuspension>("CpaType", entities.Current.CpaType);
      });
  }

  private bool ReadObligationTransaction()
  {
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnId",
          import.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
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
    /// A value of SuspendFromDayOne.
    /// </summary>
    [JsonPropertyName("suspendFromDayOne")]
    public Common SuspendFromDayOne
    {
      get => suspendFromDayOne ??= new();
      set => suspendFromDayOne = value;
    }

    /// <summary>
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

    /// <summary>
    /// A value of ZdelImportCurrent.
    /// </summary>
    [JsonPropertyName("zdelImportCurrent")]
    public DateWorkArea ZdelImportCurrent
    {
      get => zdelImportCurrent ??= new();
      set => zdelImportCurrent = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    private Common suspendFromDayOne;
    private CsePersonAccount hcCpaObligor;
    private DateWorkArea zdelImportCurrent;
    private CsePerson csePerson;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private AccrualSuspension accrualSuspension;
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
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private DateWorkArea null1;
    private DateWorkArea max;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public AccrualSuspension Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private Obligation obligation;
    private AccrualSuspension current;
    private ObligationTransaction obligationTransaction;
    private AccrualInstructions accrualInstructions;
    private AccrualSuspension accrualSuspension;
  }
#endregion
}
