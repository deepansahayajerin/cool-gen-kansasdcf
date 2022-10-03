// Program: FN_DISC_OBLIGATION_PYMNT_SCH, ID: 372041919, model: 746.
// Short name: SWE00442
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DISC_OBLIGATION_PYMNT_SCH.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process results in the dicontinuance of active obligation payment 
/// schedule with effective date less than or equal to current date.
/// </para>
/// </summary>
[Serializable]
public partial class FnDiscObligationPymntSch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISC_OBLIGATION_PYMNT_SCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDiscObligationPymntSch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDiscObligationPymntSch.
  /// </summary>
  public FnDiscObligationPymntSch(IContext context, Import import, Export export)
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
    // -----------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // -----------------------------------------------
    // -----------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // 01/22/95   Holly Kennedy-MTW			Modify initial code.
    // -----------------------------------------------------------------------
    try
    {
      UpdateObligationPaymentSchedule();

      // : OBLIGATION PAYMENT SCHEDULE successfully discontinued.
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

  private void UpdateObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(import.ObligationPaymentSchedule.Populated);

    var endDt = import.PymntSchedule.Date;
    var lastUpdateBy = global.UserId;
    var lastUpdateTmst = Now();

    import.ObligationPaymentSchedule.Populated = false;
    Update("UpdateObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", endDt);
        db.SetNullableString(command, "lastUpdateBy", lastUpdateBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.
          SetInt32(command, "otyType", import.ObligationPaymentSchedule.OtyType);
          
        db.SetInt32(
          command, "obgGeneratedId",
          import.ObligationPaymentSchedule.ObgGeneratedId);
        db.SetString(
          command, "obgCspNumber",
          import.ObligationPaymentSchedule.ObgCspNumber);
        db.SetString(
          command, "obgCpaType", import.ObligationPaymentSchedule.ObgCpaType);
        db.SetDate(
          command, "startDt",
          import.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
      });

    import.ObligationPaymentSchedule.EndDt = endDt;
    import.ObligationPaymentSchedule.LastUpdateBy = lastUpdateBy;
    import.ObligationPaymentSchedule.LastUpdateTmst = lastUpdateTmst;
    import.ObligationPaymentSchedule.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of PymntSchedule.
    /// </summary>
    [JsonPropertyName("pymntSchedule")]
    public DateWorkArea PymntSchedule
    {
      get => pymntSchedule ??= new();
      set => pymntSchedule = value;
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

    private DateWorkArea pymntSchedule;
    private ObligationPaymentSchedule obligationPaymentSchedule;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
