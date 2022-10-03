// Program: FN_CAB_DENY_POTENTIAL_RECOVERY, ID: 372046584, model: 746.
// Short name: SWE01876
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_DENY_POTENTIAL_RECOVERY.
/// </summary>
[Serializable]
public partial class FnCabDenyPotentialRecovery: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_DENY_POTENTIAL_RECOVERY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabDenyPotentialRecovery(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabDenyPotentialRecovery.
  /// </summary>
  public FnCabDenyPotentialRecovery(IContext context, Import import,
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
    // -------------------------------------------------------------------------------
    // Date      Programmer      Reason #      Description
    // 10/27/98  G Sharp         Phase 2       Clean up exit state's that are 
    // zdelete
    // 12/22/2000   Vithal Madhira   WR# 262    Update  the value of Reason_text
    // in Payment_Status_History table
    // 06/05/2007   G. Pan        PR229561  Modified to set process_date to
    //                                      
    // current_date in Payment_Request table
    //                                      
    // when F6 deny.
    // -------------------------------------------------------------------------------
    if (ReadPaymentStatus())
    {
      if (ReadPaymentRequestPaymentStatusHistory())
      {
        local.PaymentStatusHistory.SystemGeneratedIdentifier =
          entities.PaymentStatusHistory.SystemGeneratedIdentifier;

        // =================================================
        // 5/25/99 - Bud Adams  -  replaced an unusual piece of code
        //   with this Read Each.  Replaced was REPEAT -- UNTIL with
        //   a Read inside of it, and the local sys-gen-id being incremented
        //   by one if the payment status codes didn't match.
        // =================================================
        foreach(var item in ReadPaymentStatusPaymentStatusHistory())
        {
          if (Equal(entities.ExistingPaymentStatus.Code,
            import.PaymentStatus.Code))
          {
            ExitState = "FN0000_RECOVERY_ALREADY_DENIED";

            return;
          }
        }

        try
        {
          UpdatePaymentStatusHistory();

          try
          {
            CreatePaymentStatusHistory();

            try
            {
              UpdatePaymentRequest();
            }
            catch(Exception e2)
            {
              switch(GetErrorCode(e2))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_PAYMENT_REQUEST_NU";

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
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PAYMENT_STATUS_HIST_AE_RB";

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
        ExitState = "FN0000_PAYMENT_REQUEST_NF_RB";
      }
    }
    else
    {
      ExitState = "FN0000_PYMNT_STAT_NF_RB";
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Null1.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = entities.PaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      entities.ExistingPaymentStatusHistory.SystemGeneratedIdentifier + 1;
    var effectiveDate = import.Current.Date;
    var discontinueDate = UseCabSetMaximumDiscontinueDate();
    var createdBy = global.UserId;
    var createdTimestamp = import.Current.Timestamp;
    var reasonText = import.PmntStatHistReasonTxt.Text76;

    entities.New1.Populated = false;
    Update("CreatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.New1.PstGeneratedId = pstGeneratedId;
    entities.New1.PrqGeneratedId = prqGeneratedId;
    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.ReasonText = reasonText;
    entities.New1.Populated = true;
  }

  private bool ReadPaymentRequestPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequestPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          import.PaymentRequest.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 3);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 5);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentRequest.Populated = true;
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentStatus.Code);
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentStatusPaymentStatusHistory()
  {
    entities.ExistingPaymentStatus.Populated = false;
    entities.ExistingPaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentStatusPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "pymntStatHistId",
          local.PaymentStatusHistory.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingPaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentStatus.Code = db.GetString(reader, 1);
        entities.ExistingPaymentStatusHistory.PrqGeneratedId =
          db.GetInt32(reader, 2);
        entities.ExistingPaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingPaymentStatus.Populated = true;
        entities.ExistingPaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private void UpdatePaymentRequest()
  {
    var processDate = import.Current.Date;

    entities.PaymentRequest.Populated = false;
    Update("UpdatePaymentRequest",
      (db, command) =>
      {
        db.SetDate(command, "processDate", processDate);
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      });

    entities.PaymentRequest.ProcessDate = processDate;
    entities.PaymentRequest.Populated = true;
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = import.Current.Date;

    entities.PaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.PaymentStatusHistory.SystemGeneratedIdentifier);
      });

    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.Populated = true;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of PmntStatHistReasonTxt.
    /// </summary>
    [JsonPropertyName("pmntStatHistReasonTxt")]
    public NewWorkSet PmntStatHistReasonTxt
    {
      get => pmntStatHistReasonTxt ??= new();
      set => pmntStatHistReasonTxt = value;
    }

    private DateWorkArea current;
    private PaymentStatus paymentStatus;
    private PaymentRequest paymentRequest;
    private NewWorkSet pmntStatHistReasonTxt;
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
    /// A value of Successful.
    /// </summary>
    [JsonPropertyName("successful")]
    public Common Successful
    {
      get => successful ??= new();
      set => successful = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Common successful;
    private PaymentStatusHistory paymentStatusHistory;
    private DateWorkArea null1;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPaymentStatus.
    /// </summary>
    [JsonPropertyName("existingPaymentStatus")]
    public PaymentStatus ExistingPaymentStatus
    {
      get => existingPaymentStatus ??= new();
      set => existingPaymentStatus = value;
    }

    /// <summary>
    /// A value of ExistingPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("existingPaymentStatusHistory")]
    public PaymentStatusHistory ExistingPaymentStatusHistory
    {
      get => existingPaymentStatusHistory ??= new();
      set => existingPaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public PaymentStatusHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    private PaymentStatus existingPaymentStatus;
    private PaymentStatusHistory existingPaymentStatusHistory;
    private PaymentStatusHistory new1;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentRequest paymentRequest;
    private PaymentStatus paymentStatus;
  }
#endregion
}
