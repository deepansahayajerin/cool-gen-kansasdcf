// Program: FN_UPDATE_OBLIGATION_PYMNT_SCH, ID: 372041916, model: 746.
// Short name: SWE00663
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_OBLIGATION_PYMNT_SCH.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process results in the updating of an existing obligation payment 
/// schedule for its related debt detail.  This process permits updating of
/// amount, effective &amp; discontinued dates for inactive obligation payment
/// schedule.  Inactive payment schedules have effective dates greater than
/// current date.  However, Active debts with effective date less than or =
/// current date will be discontinued.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateObligationPymntSch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_OBLIGATION_PYMNT_SCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateObligationPymntSch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateObligationPymntSch.
  /// </summary>
  public FnUpdateObligationPymntSch(IContext context, Import import,
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
    // -----------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // -----------------------------------------------
    // -----------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // 01/22/95   Holly Kennedy-MTW			Modify initial code.
    // 06/11/97   Holly Kennedy-MTW                Added logic to prevent
    //                                             
    // the obligation payment
    //                                             
    // schedule from
    //                                             
    // validating against
    //                                             
    // itself.
    // 07/18/97   Paul R. Egger        Can have an effective date equal to 
    // current date.
    // 10/27/98   G Sharp              Phase 2         Clean up, Delete unused 
    // export views.
    // -----------------------------------------------------------------------
    if (Lt(import.ObligationPaymentSchedule.StartDt, Now().Date))
    {
      ExitState = "CANNOT_CHANGE_EFFECTIVE_RECORD";

      return;
    }

    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadObligation())
    {
      // Okay, continue processing.
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    if (ReadObligationPaymentSchedule1())
    {
      // *****
      // Obligation Payment Schedule successfully retrieved.
      // *****
    }
    else
    {
      ExitState = "FN0000_OBLIG_PYMNT_SCH_NF";

      return;
    }

    if (!Equal(entities.ObligationPaymentSchedule.EndDt,
      import.ObligationPaymentSchedule.EndDt))
    {
      if (Equal(entities.ObligationPaymentSchedule.EndDt, local.Maximum.Date) &&
        Equal(import.ObligationPaymentSchedule.EndDt, local.Null1.Date))
      {
      }
      else
      {
        // *****
        // Disallow date overlap
        // H. Kennedy 06-11-97  Added logic so that the obligation was not 
        // validated against itself.
        // R.B.M      08-22-97  Changed the Overlap Validation Logic
        // *****
        if (ReadObligationPaymentSchedule2())
        {
          ExitState = "OVERLAPPING_DATE_RANGE";

          return;
        }
        else
        {
          // *****
          // OK, No conflicts found for End Date change.
          // *****
        }
      }
    }

    try
    {
      UpdateObligationPaymentSchedule();

      // : Obligation Payment Schedule successfully updated.
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_OBLIG_PYMNT_SCH_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_OBLIG_PYMNT_SCH_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
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

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
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

  private bool ReadObligationPaymentSchedule1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule1",
      (db, command) =>
      {
        db.SetDate(
          command, "startDt",
          import.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.LastUpdateBy =
          db.GetNullableString(reader, 7);
        entities.ObligationPaymentSchedule.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 9);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 10);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 11);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 12);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 13);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
      });
  }

  private bool ReadObligationPaymentSchedule2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Existing.Populated = false;

    return Read("ReadObligationPaymentSchedule2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "startDt1",
          import.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
        db.SetDate(
          command, "startDt2",
          import.ObligationPaymentSchedule.EndDt.GetValueOrDefault());
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
    System.Diagnostics.Debug.
      Assert(entities.ObligationPaymentSchedule.Populated);

    var amount = import.ObligationPaymentSchedule.Amount.GetValueOrDefault();
    var endDt = import.ObligationPaymentSchedule.EndDt;
    var lastUpdateBy = global.UserId;
    var lastUpdateTmst = Now();
    var frequencyCode = import.ObligationPaymentSchedule.FrequencyCode;
    var dayOfWeek =
      import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault();
    var dayOfMonth1 =
      import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
    var dayOfMonth2 =
      import.ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault();
    var periodInd = import.ObligationPaymentSchedule.PeriodInd ?? "";

    CheckValid<ObligationPaymentSchedule>("FrequencyCode", frequencyCode);
    CheckValid<ObligationPaymentSchedule>("PeriodInd", periodInd);
    entities.ObligationPaymentSchedule.Populated = false;
    Update("UpdateObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "obligPschAmt", amount);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetNullableString(command, "lastUpdateBy", lastUpdateBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "frqPrdCd", frequencyCode);
        db.SetNullableInt32(command, "dayOfWeek", dayOfWeek);
        db.SetNullableInt32(command, "dayOfMonth1", dayOfMonth1);
        db.SetNullableInt32(command, "dayOfMonth2", dayOfMonth2);
        db.SetNullableString(command, "periodInd", periodInd);
        db.SetInt32(
          command, "otyType", entities.ObligationPaymentSchedule.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationPaymentSchedule.ObgGeneratedId);
        db.SetString(
          command, "obgCspNumber",
          entities.ObligationPaymentSchedule.ObgCspNumber);
        db.SetString(
          command, "obgCpaType", entities.ObligationPaymentSchedule.ObgCpaType);
          
        db.SetDate(
          command, "startDt",
          entities.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
      });

    entities.ObligationPaymentSchedule.Amount = amount;
    entities.ObligationPaymentSchedule.EndDt = endDt;
    entities.ObligationPaymentSchedule.LastUpdateBy = lastUpdateBy;
    entities.ObligationPaymentSchedule.LastUpdateTmst = lastUpdateTmst;
    entities.ObligationPaymentSchedule.FrequencyCode = frequencyCode;
    entities.ObligationPaymentSchedule.DayOfWeek = dayOfWeek;
    entities.ObligationPaymentSchedule.DayOfMonth1 = dayOfMonth1;
    entities.ObligationPaymentSchedule.DayOfMonth2 = dayOfMonth2;
    entities.ObligationPaymentSchedule.PeriodInd = periodInd;
    entities.ObligationPaymentSchedule.Populated = true;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    private Obligation obligation;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    private DateWorkArea null1;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public ObligationPaymentSchedule Existing
    {
      get => existing ??= new();
      set => existing = value;
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
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private Obligation obligation;
    private ObligationPaymentSchedule existing;
    private ObligationPaymentSchedule obligationPaymentSchedule;
  }
#endregion
}
