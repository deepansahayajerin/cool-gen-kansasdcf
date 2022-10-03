// Program: FN_EST_OBLIGATION_PYMNT_SCH, ID: 372041914, model: 746.
// Short name: SWE00452
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_EST_OBLIGATION_PYMNT_SCH.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process results in the initial recording of the payment schedule for 
/// its related debt detail.  The effective or start date of the schedule is
/// recorded.  A discontinued date in the future is also specified.  If no
/// discontinued date is specified the date defaults to 12-31-2099.
/// </para>
/// </summary>
[Serializable]
public partial class FnEstObligationPymntSch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EST_OBLIGATION_PYMNT_SCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEstObligationPymntSch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEstObligationPymntSch.
  /// </summary>
  public FnEstObligationPymntSch(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // -----------------------------------------------
    // -------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // 01/22/95   Holly Kennedy-MTW			Modify initial code.
    // 10/27/98   G Sharp              Phase 2        1. Add Day of Month 1 and 
    // 2 and Day of Week
    //                                                
    // 2. Delete export views not used.
    //                                                
    // 3. Added import concurrent obligation_type.
    // Also added to read of concurrent obligation,
    // check for concurrent obligation_type.
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // *****
    // HARD CODE AREA
    // *****
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedDebtDistribution();

    // *****
    // MAIN-LINE AREA
    // *****
    if (ReadObligation1())
    {
      // Okay, continue processing.
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    // *****
    // Check for active obligation payment schedule that conflicts with add 
    // schedule entry.  If active exists, display screen message.
    // *****
    if (ReadObligationPaymentSchedule())
    {
      if (Lt(entities.Existing.EndDt, local.Maximum.Date))
      {
        if (!Lt(entities.Existing.EndDt,
          import.ObligationPaymentSchedule.StartDt))
        {
          ExitState = "FN0000_OBLIG_PYMNT_SCH_ACTIVE";

          return;
        }
      }
      else if (!Lt(entities.Existing.StartDt,
        import.ObligationPaymentSchedule.StartDt))
      {
        ExitState = "FN0000_OBLIG_PYMNT_SCH_ACTIVE";

        return;
      }
    }

    // *****
    // Found the most current Obligation Payment Schedule and it's start date is
    // before the start date of the Obligation Payment Schedule start date
    // being entered.  If this is found, we can discontinue this one and go
    // ahead and add the new one.
    // *****
    if (!Lt(entities.Existing.EndDt, import.ObligationPaymentSchedule.StartDt))
    {
      try
      {
        UpdateObligationPaymentSchedule();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLG_PYMNT_SCH_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLG_PYMNT_SCH_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (!IsEmpty(import.ConcurrentCsePerson.Number))
    {
      if (ReadObligation2())
      {
        // NOT FOUND exception will result in abort.
      }
      else
      {
        ExitState = "FN0000_CONCURRENT_OBLIG_NF_RB";

        return;
      }
    }

    // *****
    // Ready to add the new Obligation Payment Schedule.
    // *****
    try
    {
      CreateObligationPaymentSchedule1();

      // OBLIGATION PAYMENT SCHEDULE successfully created.
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_OBIG_PYMNT_SCH_AE_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_OBLG_PYMNT_SCH_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (!IsEmpty(import.ConcurrentCsePerson.Number))
    {
      // : Create the same Payment Schedule for the concurrent obligor.
      try
      {
        CreateObligationPaymentSchedule2();

        // Concurrent OBLIGATION PAYMENT SCHEDULE successfully created.
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBIG_PYMNT_SCH_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLG_PYMNT_SCH_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
  }

  private void CreateObligationPaymentSchedule1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otyType = entities.Obligation.DtyGeneratedId;
    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var obgCspNumber = entities.Obligation.CspNumber;
    var obgCpaType = entities.Obligation.CpaType;
    var startDt = import.ObligationPaymentSchedule.StartDt;
    var amount = import.ObligationPaymentSchedule.Amount.GetValueOrDefault();
    var endDt = import.ObligationPaymentSchedule.EndDt;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var frequencyCode = import.ObligationPaymentSchedule.FrequencyCode;
    var dayOfWeek =
      import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault();
    var dayOfMonth1 =
      import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
    var dayOfMonth2 =
      import.ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault();
    var periodInd = import.ObligationPaymentSchedule.PeriodInd ?? "";

    CheckValid<ObligationPaymentSchedule>("ObgCpaType", obgCpaType);
    CheckValid<ObligationPaymentSchedule>("FrequencyCode", frequencyCode);
    CheckValid<ObligationPaymentSchedule>("PeriodInd", periodInd);
    entities.New1.Populated = false;
    Update("CreateObligationPaymentSchedule1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "obgCspNumber", obgCspNumber);
        db.SetString(command, "obgCpaType", obgCpaType);
        db.SetDate(command, "startDt", startDt);
        db.SetNullableDecimal(command, "obligPschAmt", amount);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdateBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetString(command, "frqPrdCd", frequencyCode);
        db.SetNullableInt32(command, "dayOfWeek", dayOfWeek);
        db.SetNullableInt32(command, "dayOfMonth1", dayOfMonth1);
        db.SetNullableInt32(command, "dayOfMonth2", dayOfMonth2);
        db.SetNullableString(command, "periodInd", periodInd);
        db.SetNullableDate(command, "repymtLtrPrtDt", default(DateTime));
      });

    entities.New1.OtyType = otyType;
    entities.New1.ObgGeneratedId = obgGeneratedId;
    entities.New1.ObgCspNumber = obgCspNumber;
    entities.New1.ObgCpaType = obgCpaType;
    entities.New1.StartDt = startDt;
    entities.New1.Amount = amount;
    entities.New1.EndDt = endDt;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.FrequencyCode = frequencyCode;
    entities.New1.DayOfWeek = dayOfWeek;
    entities.New1.DayOfMonth1 = dayOfMonth1;
    entities.New1.DayOfMonth2 = dayOfMonth2;
    entities.New1.PeriodInd = periodInd;
    entities.New1.Populated = true;
  }

  private void CreateObligationPaymentSchedule2()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);

    var otyType = entities.ConcurrentObligation.DtyGeneratedId;
    var obgGeneratedId =
      entities.ConcurrentObligation.SystemGeneratedIdentifier;
    var obgCspNumber = entities.ConcurrentObligation.CspNumber;
    var obgCpaType = entities.ConcurrentObligation.CpaType;
    var startDt = import.ObligationPaymentSchedule.StartDt;
    var amount = import.ObligationPaymentSchedule.Amount.GetValueOrDefault();
    var endDt = import.ObligationPaymentSchedule.EndDt;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var frequencyCode = import.ObligationPaymentSchedule.FrequencyCode;
    var dayOfWeek =
      import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault();
    var dayOfMonth1 =
      import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
    var dayOfMonth2 =
      import.ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault();
    var periodInd = import.ObligationPaymentSchedule.PeriodInd ?? "";

    CheckValid<ObligationPaymentSchedule>("ObgCpaType", obgCpaType);
    CheckValid<ObligationPaymentSchedule>("FrequencyCode", frequencyCode);
    CheckValid<ObligationPaymentSchedule>("PeriodInd", periodInd);
    entities.New1.Populated = false;
    Update("CreateObligationPaymentSchedule2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "obgCspNumber", obgCspNumber);
        db.SetString(command, "obgCpaType", obgCpaType);
        db.SetDate(command, "startDt", startDt);
        db.SetNullableDecimal(command, "obligPschAmt", amount);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdateBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetString(command, "frqPrdCd", frequencyCode);
        db.SetNullableInt32(command, "dayOfWeek", dayOfWeek);
        db.SetNullableInt32(command, "dayOfMonth1", dayOfMonth1);
        db.SetNullableInt32(command, "dayOfMonth2", dayOfMonth2);
        db.SetNullableString(command, "periodInd", periodInd);
        db.SetNullableDate(command, "repymtLtrPrtDt", default(DateTime));
      });

    entities.New1.OtyType = otyType;
    entities.New1.ObgGeneratedId = obgGeneratedId;
    entities.New1.ObgCspNumber = obgCspNumber;
    entities.New1.ObgCpaType = obgCpaType;
    entities.New1.StartDt = startDt;
    entities.New1.Amount = amount;
    entities.New1.EndDt = endDt;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.FrequencyCode = frequencyCode;
    entities.New1.DayOfWeek = dayOfWeek;
    entities.New1.DayOfMonth1 = dayOfMonth1;
    entities.New1.DayOfMonth2 = dayOfMonth2;
    entities.New1.PeriodInd = periodInd;
    entities.New1.Populated = true;
  }

  private bool ReadObligation1()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", local.HardcodeObligor.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
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

  private bool ReadObligation2()
  {
    entities.ConcurrentObligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", local.HardcodeObligor.Type1);
        db.SetString(command, "cspNumber", import.ConcurrentCsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ConcurrentObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ConcurrentObligation.CpaType = db.GetString(reader, 0);
        entities.ConcurrentObligation.CspNumber = db.GetString(reader, 1);
        entities.ConcurrentObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ConcurrentObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ConcurrentObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ConcurrentObligation.CpaType);
          
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Existing.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Existing.OtyType = db.GetInt32(reader, 0);
        entities.Existing.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.Existing.ObgCspNumber = db.GetString(reader, 2);
        entities.Existing.ObgCpaType = db.GetString(reader, 3);
        entities.Existing.StartDt = db.GetDate(reader, 4);
        entities.Existing.EndDt = db.GetNullableDate(reader, 5);
        entities.Existing.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.Existing.ObgCpaType);
      });
  }

  private void UpdateObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);

    var endDt = AddDays(import.ObligationPaymentSchedule.StartDt, -1);

    entities.Existing.Populated = false;
    Update("UpdateObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", endDt);
        db.SetInt32(command, "otyType", entities.Existing.OtyType);
        db.
          SetInt32(command, "obgGeneratedId", entities.Existing.ObgGeneratedId);
          
        db.SetString(command, "obgCspNumber", entities.Existing.ObgCspNumber);
        db.SetString(command, "obgCpaType", entities.Existing.ObgCpaType);
        db.SetDate(
          command, "startDt", entities.Existing.StartDt.GetValueOrDefault());
      });

    entities.Existing.EndDt = endDt;
    entities.Existing.Populated = true;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of ConcurrentObligationType.
    /// </summary>
    [JsonPropertyName("concurrentObligationType")]
    public ObligationType ConcurrentObligationType
    {
      get => concurrentObligationType ??= new();
      set => concurrentObligationType = value;
    }

    /// <summary>
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
    }

    /// <summary>
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationType concurrentObligationType;
    private CsePerson concurrentCsePerson;
    private Obligation concurrentObligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
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
    /// A value of HardcodeObligor.
    /// </summary>
    [JsonPropertyName("hardcodeObligor")]
    public CsePersonAccount HardcodeObligor
    {
      get => hardcodeObligor ??= new();
      set => hardcodeObligor = value;
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

    private CsePersonAccount hardcodeObligor;
    private DateWorkArea maximum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
    }

    /// <summary>
    /// A value of ConcurrentCsePersonAccount.
    /// </summary>
    [JsonPropertyName("concurrentCsePersonAccount")]
    public CsePersonAccount ConcurrentCsePersonAccount
    {
      get => concurrentCsePersonAccount ??= new();
      set => concurrentCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public ObligationPaymentSchedule Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ObligationPaymentSchedule New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private ObligationType obligationType;
    private CsePerson concurrentCsePerson;
    private CsePersonAccount concurrentCsePersonAccount;
    private Obligation concurrentObligation;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationPaymentSchedule existing;
    private ObligationPaymentSchedule new1;
  }
#endregion
}
