// Program: FN_BF15_DELETE_DUPLICATE_PYMTS, ID: 371171407, model: 746.
// Short name: SWEFX15B
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
/// A program: FN_BF15_DELETE_DUPLICATE_PYMTS.
/// </para>
/// <para>
/// Procedure Step Description:
///                                                                                                                                
/// Action Block Description:
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBf15DeleteDuplicatePymts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF15_DELETE_DUPLICATE_PYMTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf15DeleteDuplicatePymts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf15DeleteDuplicatePymts.
  /// </summary>
  public FnBf15DeleteDuplicatePymts(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------
    // 08/19/2003   Lee   WR   184562
    // 
    // KPC reissued payment file clean up.  KPC resent all resisses in regular
    // nightly file and it                                             was
    // processed and it caused duplicate payments.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.CurrentDate.Date = Now().Date;
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    UseFnBf15Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (local.FileInError.ReportNumber == 99)
      {
        // : There was an error opening the error file, so we can't use
        //   the error report .
      }
      else
      {
        UseEabExtractExitStateMessage();

        if (local.FileInError.ReportNumber == 98)
        {
          // : Problem opening the control file.
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " Control File";
        }
        else
        {
          local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport1();
      }

      return;
    }

    foreach(var item in ReadPaymentRequest2())
    {
      if (ReadPaymentRequest1())
      {
        if (ReadPaymentStatusHistoryPaymentStatus1())
        {
          if (!Equal(entities.OriginalPaymentStatusHistory.EffectiveDate,
            new DateTime(2003, 7, 25)))
          {
            foreach(var item1 in ReadPaymentStatusHistory2())
            {
              ++local.NoOfDupStatusDeleted.Count;
              local.EabReportSend.RptDetail =
                "Deleted - Payment Status History for Payment Request ID: " + NumberToString
                (entities.DuplicatePaymentRequest.SystemGeneratedIdentifier, 15) +
                " Warrant Number: " + entities
                .DuplicatePaymentRequest.Number + " CSE Person Number: " + entities
                .DuplicatePaymentRequest.CsePersonNumber;
              local.EabFileHandling.Action = "WRITE";
              UseCabControlReport1();
              DeletePaymentStatusHistory1();
            }

            ++local.NoOfDupPymtsDeleted.Count;
            local.EabReportSend.RptDetail = "Deleted - Payment Request ID: " + NumberToString
              (entities.DuplicatePaymentRequest.SystemGeneratedIdentifier, 15) +
              " Warrant Number: " + entities.DuplicatePaymentRequest.Number + " CSE Person Number: " +
              entities.DuplicatePaymentRequest.CsePersonNumber;
            local.EabFileHandling.Action = "WRITE";
            UseCabControlReport1();
            DeletePaymentRequest();
          }
          else
          {
            ++local.NoOfDupStatusDeleted.Count;
            local.EabReportSend.RptDetail =
              "Deleted - Payment Status History for Payment Request ID: " + NumberToString
              (entities.OriginalPaymentRequest.SystemGeneratedIdentifier, 15) +
              " Warrant Number: " + entities.OriginalPaymentRequest.Number + " CSE Person Number: " +
              entities.OriginalPaymentRequest.CsePersonNumber;
            local.EabFileHandling.Action = "WRITE";
            UseCabControlReport1();
            DeletePaymentStatusHistory2();

            if (ReadPaymentStatusHistoryPaymentStatus2())
            {
              try
              {
                UpdatePaymentStatusHistory();
                ++local.NoOfPymtStatusUpdated.Count;
                local.EabReportSend.RptDetail =
                  "Updated - Payment Status History for Payment Request ID: " +
                  NumberToString
                  (entities.OriginalPaymentRequest.SystemGeneratedIdentifier, 15)
                  + " Warrant Number: " + entities
                  .OriginalPaymentRequest.Number + " CSE Person Number: " + entities
                  .OriginalPaymentRequest.CsePersonNumber;
                local.EabFileHandling.Action = "WRITE";
                UseCabControlReport1();

                if (ReadPaymentStatusHistory1())
                {
                  ++local.NoOfDupStatusDeleted.Count;
                  local.EabReportSend.RptDetail =
                    "Deleted - Payment Status History for Payment Request ID: " +
                    NumberToString
                    (entities.DuplicatePaymentRequest.SystemGeneratedIdentifier,
                    15) + " Warrant Number: " + entities
                    .DuplicatePaymentRequest.Number + " CSE Person Number: " + entities
                    .DuplicatePaymentRequest.CsePersonNumber;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabControlReport1();
                  DeletePaymentStatusHistory1();
                  ++local.NoOfDupPymtsDeleted.Count;
                  local.EabReportSend.RptDetail =
                    "Deleted - Payment Request ID: " + NumberToString
                    (entities.DuplicatePaymentRequest.SystemGeneratedIdentifier,
                    15) + " Warrant Number: " + entities
                    .DuplicatePaymentRequest.Number + " CSE Person Number: " + entities
                    .DuplicatePaymentRequest.CsePersonNumber;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabControlReport1();
                  DeletePaymentRequest();
                }
                else
                {
                  local.EabReportSend.RptDetail =
                    "ERROR - Payment Status History not found for Payment Request ID: " +
                    NumberToString
                    (entities.DuplicatePaymentRequest.SystemGeneratedIdentifier,
                    15);
                  local.EabFileHandling.Action = "WRITE";
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_PMNT_STAT_HIST_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

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
              local.EabReportSend.RptDetail =
                "ERROR - Payment Status History not found for Payment Request ID: " +
                NumberToString
                (entities.OriginalPaymentRequest.SystemGeneratedIdentifier, 15);
                
              local.EabFileHandling.Action = "WRITE";
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
        }
        else
        {
          local.EabReportSend.RptDetail =
            "ERROR - Payment Status History not found for Payment Request ID : " +
            NumberToString
            (entities.OriginalPaymentRequest.SystemGeneratedIdentifier, 15);
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      else
      {
        // Continue
      }
    }

    // *** NORMAL TERMINATION OF THE PROCEDURE EXECUTION ***
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport1();
      local.EabReportSend.RptDetail =
        "Abended while deleting duplicate payments";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();
    local.EabReportSend.RptDetail =
      "Total number of Deleted Payment Requests:   " + NumberToString
      (local.NoOfDupPymtsDeleted.Count, 15);
    UseCabControlReport1();
    local.EabReportSend.RptDetail =
      "Total number of Deleted Payment Status Histories:   " + NumberToString
      (local.NoOfDupStatusDeleted.Count, 15);
    UseCabControlReport1();
    local.EabReportSend.RptDetail =
      "Total number of Updated Payment Status Histories:   " + NumberToString
      (local.NoOfPymtStatusUpdated.Count, 15);
    UseCabControlReport1();
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport2();
    UseCabErrorReport3();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnBf15Housekeeping()
  {
    var useImport = new FnBf15Housekeeping.Import();
    var useExport = new FnBf15Housekeeping.Export();

    Call(FnBf15Housekeeping.Execute, useImport, useExport);

    local.FileInError.ReportNumber = useExport.FileInError.ReportNumber;
  }

  private void DeletePaymentRequest()
  {
    bool exists;

    Update("DeletePaymentRequest#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.DuplicatePaymentRequest.SystemGeneratedIdentifier);
      });

    exists = Read("DeletePaymentRequest#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.DuplicatePaymentRequest.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ELEC_FUND_TRAN\".",
        "50001");
    }

    Update("DeletePaymentRequest#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.DuplicatePaymentRequest.SystemGeneratedIdentifier);
      });

    Update("DeletePaymentRequest#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.DuplicatePaymentRequest.SystemGeneratedIdentifier);
      });

    exists = Read("DeletePaymentRequest#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.DuplicatePaymentRequest.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_POT_RECOVERY\".",
        "50001");
    }

    Update("DeletePaymentRequest#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.DuplicatePaymentRequest.SystemGeneratedIdentifier);
      });
  }

  private void DeletePaymentStatusHistory1()
  {
    Update("DeletePaymentStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "pstGeneratedId",
          entities.DuplicatePaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.DuplicatePaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.DuplicatePaymentStatusHistory.SystemGeneratedIdentifier);
      });
  }

  private void DeletePaymentStatusHistory2()
  {
    Update("DeletePaymentStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "pstGeneratedId",
          entities.OriginalPaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.OriginalPaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.OriginalPaymentStatusHistory.SystemGeneratedIdentifier);
      });
  }

  private bool ReadPaymentRequest1()
  {
    entities.OriginalPaymentRequest.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNumber",
          entities.DuplicatePaymentRequest.CsePersonNumber ?? "");
        db.
          SetDecimal(command, "amount", entities.DuplicatePaymentRequest.Amount);
          
        db.SetNullableString(
          command, "number", entities.DuplicatePaymentRequest.Number ?? "");
        db.SetDateTime(
          command, "createdTimestamp",
          entities.DuplicatePaymentRequest.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OriginalPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalPaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.OriginalPaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.OriginalPaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.OriginalPaymentRequest.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.OriginalPaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.OriginalPaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.OriginalPaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.OriginalPaymentRequest.Classification =
          db.GetString(reader, 8);
        entities.OriginalPaymentRequest.Number =
          db.GetNullableString(reader, 9);
        entities.OriginalPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 10);
        entities.OriginalPaymentRequest.Type1 = db.GetString(reader, 11);
        entities.OriginalPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.OriginalPaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 13);
        entities.OriginalPaymentRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest2()
  {
    entities.DuplicatePaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp1", new DateTime(2003, 7, 25, 2, 30, 0));
        db.SetDateTime(
          command, "createdTimestamp2", new DateTime(2003, 7, 25, 3, 0, 0));
      },
      (db, reader) =>
      {
        entities.DuplicatePaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DuplicatePaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.DuplicatePaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.DuplicatePaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.DuplicatePaymentRequest.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.DuplicatePaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.DuplicatePaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.DuplicatePaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.DuplicatePaymentRequest.Classification =
          db.GetString(reader, 8);
        entities.DuplicatePaymentRequest.Number =
          db.GetNullableString(reader, 9);
        entities.DuplicatePaymentRequest.PrintDate =
          db.GetNullableDate(reader, 10);
        entities.DuplicatePaymentRequest.Type1 = db.GetString(reader, 11);
        entities.DuplicatePaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.DuplicatePaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 13);
        entities.DuplicatePaymentRequest.Populated = true;

        return true;
      });
  }

  private bool ReadPaymentStatusHistory1()
  {
    entities.DuplicatePaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", new DateTime(2099, 12, 31));
        db.SetInt32(
          command, "prqGeneratedId",
          entities.DuplicatePaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DuplicatePaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 0);
        entities.DuplicatePaymentStatusHistory.PrqGeneratedId =
          db.GetInt32(reader, 1);
        entities.DuplicatePaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DuplicatePaymentStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DuplicatePaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DuplicatePaymentStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DuplicatePaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DuplicatePaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.DuplicatePaymentStatusHistory.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentStatusHistory2()
  {
    entities.DuplicatePaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.DuplicatePaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DuplicatePaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 0);
        entities.DuplicatePaymentStatusHistory.PrqGeneratedId =
          db.GetInt32(reader, 1);
        entities.DuplicatePaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DuplicatePaymentStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DuplicatePaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DuplicatePaymentStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DuplicatePaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DuplicatePaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.DuplicatePaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private bool ReadPaymentStatusHistoryPaymentStatus1()
  {
    entities.PaymentStatus.Populated = false;
    entities.OriginalPaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistoryPaymentStatus1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prqGeneratedId",
          entities.OriginalPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalPaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalPaymentStatusHistory.PrqGeneratedId =
          db.GetInt32(reader, 1);
        entities.OriginalPaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalPaymentStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.OriginalPaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OriginalPaymentStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.OriginalPaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.OriginalPaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatus.Code = db.GetString(reader, 8);
        entities.PaymentStatus.Populated = true;
        entities.OriginalPaymentStatusHistory.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistoryPaymentStatus2()
  {
    entities.PaymentStatus.Populated = false;
    entities.OriginalPaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistoryPaymentStatus2",
      (db, command) =>
      {
        db.
          SetNullableDate(command, "discontinueDate", new DateTime(2003, 7, 25));
          
        db.SetInt32(
          command, "prqGeneratedId",
          entities.OriginalPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalPaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalPaymentStatusHistory.PrqGeneratedId =
          db.GetInt32(reader, 1);
        entities.OriginalPaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalPaymentStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.OriginalPaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OriginalPaymentStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.OriginalPaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.OriginalPaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatus.Code = db.GetString(reader, 8);
        entities.PaymentStatus.Populated = true;
        entities.OriginalPaymentStatusHistory.Populated = true;
      });
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalPaymentStatusHistory.Populated);

    var discontinueDate = local.Maximum.Date;

    entities.OriginalPaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.OriginalPaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.OriginalPaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.OriginalPaymentStatusHistory.SystemGeneratedIdentifier);
      });

    entities.OriginalPaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.OriginalPaymentStatusHistory.Populated = true;
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
    /// <summary>
    /// A value of NoOfPymtStatusUpdated.
    /// </summary>
    [JsonPropertyName("noOfPymtStatusUpdated")]
    public Common NoOfPymtStatusUpdated
    {
      get => noOfPymtStatusUpdated ??= new();
      set => noOfPymtStatusUpdated = value;
    }

    /// <summary>
    /// A value of NoOfDupStatusDeleted.
    /// </summary>
    [JsonPropertyName("noOfDupStatusDeleted")]
    public Common NoOfDupStatusDeleted
    {
      get => noOfDupStatusDeleted ??= new();
      set => noOfDupStatusDeleted = value;
    }

    /// <summary>
    /// A value of NoOfDupPymtsDeleted.
    /// </summary>
    [JsonPropertyName("noOfDupPymtsDeleted")]
    public Common NoOfDupPymtsDeleted
    {
      get => noOfDupPymtsDeleted ??= new();
      set => noOfDupPymtsDeleted = value;
    }

    /// <summary>
    /// A value of FileInError.
    /// </summary>
    [JsonPropertyName("fileInError")]
    public EabReportSend FileInError
    {
      get => fileInError ??= new();
      set => fileInError = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    private Common noOfPymtStatusUpdated;
    private Common noOfDupStatusDeleted;
    private Common noOfDupPymtsDeleted;
    private EabReportSend fileInError;
    private Common counter;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea maximum;
    private DateWorkArea currentDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DuplicatePaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("duplicatePaymentStatusHistory")]
    public PaymentStatusHistory DuplicatePaymentStatusHistory
    {
      get => duplicatePaymentStatusHistory ??= new();
      set => duplicatePaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of OriginalPaymentRequest.
    /// </summary>
    [JsonPropertyName("originalPaymentRequest")]
    public PaymentRequest OriginalPaymentRequest
    {
      get => originalPaymentRequest ??= new();
      set => originalPaymentRequest = value;
    }

    /// <summary>
    /// A value of DuplicatePaymentRequest.
    /// </summary>
    [JsonPropertyName("duplicatePaymentRequest")]
    public PaymentRequest DuplicatePaymentRequest
    {
      get => duplicatePaymentRequest ??= new();
      set => duplicatePaymentRequest = value;
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
    /// A value of OriginalPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("originalPaymentStatusHistory")]
    public PaymentStatusHistory OriginalPaymentStatusHistory
    {
      get => originalPaymentStatusHistory ??= new();
      set => originalPaymentStatusHistory = value;
    }

    private PaymentStatusHistory duplicatePaymentStatusHistory;
    private PaymentRequest originalPaymentRequest;
    private PaymentRequest duplicatePaymentRequest;
    private PaymentStatus paymentStatus;
    private PaymentStatusHistory originalPaymentStatusHistory;
  }
#endregion
}
