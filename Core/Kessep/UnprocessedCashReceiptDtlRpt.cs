// Program: UNPROCESSED_CASH_RECEIPT_DTL_RPT, ID: 372736363, model: 746.
// Short name: SWEF730B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: UNPROCESSED_CASH_RECEIPT_DTL_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class UnprocessedCashReceiptDtlRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UNPROCESSED_CASH_RECEIPT_DTL_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UnprocessedCashReceiptDtlRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UnprocessedCashReceiptDtlRpt.
  /// </summary>
  public UnprocessedCashReceiptDtlRpt(IContext context, Import import,
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
    // HEAT NUM.      DATE     FIXED BY       DESCRIPTION
    // =========    ========   ===========    ====================
    // H00075710    10/05/99   pphinney       Modified logic to refresh
    // and select correct
    // Worker-ID.
    // I00150964    07/23/02   pphinney       Add code for OFFICE and NEW Sort 
    // key
    // 00179243     05/22/03   GVandy         Performance modifications.
    // =========    ========   ===========    ====================
    ExitState = "ACO_NN0000_ALL_OK";

    // *** set the MAX date to '2099-12-31'
    local.Max.Date = new DateTime(2099, 12, 31);

    // *** set the LOCAL CURRENT date to the system date
    local.Current.Date = Now().Date;
    local.Found.Flag = "N";

    // *** get Process Date from Program Processing Info,
    // *** where NAME is SWEFB730
    // ***
    // *** This date is used for the Report Date in the page heading
    if (ReadProgramProcessingInfo())
    {
      export.ProgramProcessingInfo.ProcessDate =
        entities.ProgramProcessingInfo.ProcessDate;
      local.Found.Flag = "Y";
    }

    // ***
    // *** OPEN the Error Report
    // ***
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProcessDate = entities.ProgramProcessingInfo.ProcessDate;
    local.NeededToOpen.ProgramName = entities.ProgramProcessingInfo.Name;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (AsChar(local.Found.Flag) == 'N')
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF";

      // ***
      // *** WRITE to the Error Report
      // ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Program Processing Info not found";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      return;
    }

    // *** Open the report (SRRUN249)
    local.ReportParms.Parm1 = "OF";
    local.ReportParms.Parm2 = "";
    UseEabUnprocessedCashReceiptDtl1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** ERROR opening the report
      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    // ***
    // *** get each Cash Receipt/Cash Receipt Detail/
    // *** Cash Receipt  Detail Stat History/Cash Receipt Detail Status
    // *** combination
    // ***       Removed SORT as Part of PR 150964
    // *** (sort ASCENDING Received_Date)
    // ***
    // * * Retrieve ONLY Cash Receipts received  10 days ago or OLDER
    local.Local10DaysPlus.ReceivedDate = AddDays(local.Current.Date, -9);

    // 05/22/03  GVandy  PR 00179243 Performance modifications.  Original code 
    // is disabled following this read each.
    foreach(var item in ReadCashReceiptDetail())
    {
      if (!ReadCashReceipt())
      {
        // *** Ignore this Cash Receipt since it is not yet 10 days old.
        continue;
      }

      if (ReadCashReceiptDetailStatus())
      {
        switch(TrimEnd(entities.CashReceiptDetailStatus.Code))
        {
          case "PEND":
            break;
          case "REC":
            break;
          case "REF":
            break;
          case "SUSP":
            break;
          default:
            // *** ignore the cash receipt detail if the current status is not 
            // one of the 4 above.
            continue;
        }
      }
      else
      {
        continue;
      }

      // ***
      // *** Cash Receipt's are 10 days or older
      // ***
      // *** populate Export views
      MoveCashReceipt(entities.CashReceipt, export.CashReceipt);
      MoveCashReceiptDetail(entities.CashReceiptDetail, export.CashReceiptDetail);
        
      export.CashReceiptDetailStatus.Code =
        entities.CashReceiptDetailStatus.Code;

      // ***
      // *** calculate Undistributed amount
      // *** (collection amount - distributed amount - refunded amount)
      // ***
      export.Undist.CollectionAmount =
        entities.CashReceiptDetail.CollectionAmount - entities
        .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
        .CashReceiptDetail.RefundedAmount.GetValueOrDefault();

      // ***
      // *** format the Obligor name as follows removing all trailing spaces
      // *** (LastName, FirstName MiddleInitial)
      // ***
      export.ConcatenateName.FormattedName =
        TrimEnd(entities.CashReceiptDetail.ObligorLastName) + ", " + TrimEnd
        (entities.CashReceiptDetail.ObligorFirstName) + " " + Substring
        (entities.CashReceiptDetail.ObligorMiddleName,
        CashReceiptDetail.ObligorMiddleName_MaxLength, 1, 1);

      // ***
      // *** Get Cash Receipt Source Type for current Cash Receipt
      // ***
      if (ReadCashReceiptSourceTypeCashReceiptEvent())
      {
        // *** populate Export view
        export.CashReceiptSourceType.Code = entities.CashReceiptSourceType.Code;
      }
      else
      {
        ExitState = "CASH_RECEIPT_SOURCE_TYPE_NF";

        // ***
        // *** WRITE to the Error Report
        // ***
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Cash Receipt Source Type not found for Cash Receipt Event " + NumberToString
          (entities.CashReceiptEvent.SystemGeneratedIdentifier, 15);
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      // H00075710    10/05/99   pphinney
      // vvvvvvvvvvvvvvvvvvvvv H00075710 vvvvvvvvvvvvvvvvvvvvvvvv
      export.ServiceProvider.UserId = "";

      // I00150964    07/23/02   pphinney       Add code for OFFICE and NEW Sort
      // key
      export.Office.Name = "";

      // *** is Court Order Number populated??
      if (!IsEmpty(entities.CashReceiptDetail.CourtOrderNumber))
      {
        // Try to find Service Provide using Court Order Number
        // ***
        // *** get each Legal Action/Case Role/Case Assignment/
        // *** Service Provider combination
        // ***
        if (ReadLegalActionCaseRoleCaseAssignmentServiceProvider())
        {
          // *** populate Export view
          // I00150964    07/23/02   pphinney       Add code for OFFICE and NEW 
          // Sort key
          if (ReadOffice())
          {
            MoveOffice(entities.Office, export.Office);
          }
          else
          {
            // * * CONTINUE
          }

          export.ServiceProvider.UserId = entities.ServiceProvider.UserId;
        }
      }

      if (IsEmpty(export.ServiceProvider.UserId))
      {
        // Service Provide NOT FOUND using Court Order Number
        // Try to find Service Provide using Person Number
        if (!IsEmpty(entities.CashReceiptDetail.ObligorPersonNumber))
        {
          // ***
          // *** get each CSE Person/Case Role/Case Assignment/Service Provider
          // *** combination
          // ***
          if (ReadCsePersonCaseRoleCaseAssignmentServiceProvider())
          {
            // *** populate Export view
            // I00150964    07/23/02   pphinney       Add code for OFFICE and 
            // NEW Sort key
            if (ReadOffice())
            {
              MoveOffice(entities.Office, export.Office);
            }
            else
            {
              // * * CONTINUE
            }

            export.ServiceProvider.UserId = entities.ServiceProvider.UserId;
          }
        }
      }

      // IF Service Provider NOT FOUND it will REMAIN SPACES
      // This is a valid condition
      // ^^^^^^^^^^^^^^^^^^^^^^^^^^ H00075710 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
      // *** is the Cash Receipt older than 14 days??
      if (!Lt(AddDays(local.Current.Date, -14),
        entities.CashReceipt.ReceivedDate))
      {
        // ***
        // *** Cash Receipt Details - Out of Compliance 15 days or more
        // ***
        // *** Write to the report (SRRUN249)
        local.ReportParms.Parm1 = "GR";
        local.ReportParms.Parm2 = "";
        local.ReportParms.SubreportCode = "MAIN";
        UseEabUnprocessedCashReceiptDtl2();

        if (!IsEmpty(local.ReportParms.Parm1))
        {
          // *** ERROR writing to the report
          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        continue;
      }

      // ***
      // *** Cash Receipt Details - Unprocessed for 10-14 Days
      // ***
      // *** Write to the report (SRRUN249)
      local.ReportParms.Parm1 = "GR";
      local.ReportParms.Parm2 = "";
      local.ReportParms.SubreportCode = "SUB";
      UseEabUnprocessedCashReceiptDtl2();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        // *** ERROR writing to the report
        ExitState = "FILE_WRITE_ERROR_RB";

        return;
      }
    }

    // *** Close the report (SRRUN249)
    local.ReportParms.Parm1 = "CF";
    local.ReportParms.Parm2 = "";
    UseEabUnprocessedCashReceiptDtl1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** ERROR closing the report
      ExitState = "FILE_CLOSE_ERROR";

      return;
    }

    // ***
    // *** CLOSE the Error Report
    // ***
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.SequentialNumber = source.SequentialNumber;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionAmount = source.CollectionAmount;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabUnprocessedCashReceiptDtl1()
  {
    var useImport = new EabUnprocessedCashReceiptDtl.Import();
    var useExport = new EabUnprocessedCashReceiptDtl.Export();

    useImport.ReportParms.Assign(local.ReportParms);
    useExport.ReportParms.Assign(local.ReportParms);

    Call(EabUnprocessedCashReceiptDtl.Execute, useImport, useExport);

    local.ReportParms.Assign(useExport.ReportParms);
  }

  private void UseEabUnprocessedCashReceiptDtl2()
  {
    var useImport = new EabUnprocessedCashReceiptDtl.Import();
    var useExport = new EabUnprocessedCashReceiptDtl.Export();

    useImport.ReportParms.Assign(local.ReportParms);
    useImport.ConcatenateName.FormattedName =
      export.ConcatenateName.FormattedName;
    useImport.ProgramProcessingInfo.ProcessDate =
      export.ProgramProcessingInfo.ProcessDate;
    useImport.ServiceProvider.UserId = export.ServiceProvider.UserId;
    MoveCashReceipt(export.CashReceipt, useImport.CashReceipt);
    useImport.Undist.CollectionAmount = export.Undist.CollectionAmount;
    useImport.CashReceiptDetail.Assign(export.CashReceiptDetail);
    useImport.CashReceiptDetailStatus.Code =
      export.CashReceiptDetailStatus.Code;
    useImport.CashReceiptSourceType.Code = export.CashReceiptSourceType.Code;
    useImport.Office.Name = export.Office.Name;
    useExport.ReportParms.Assign(local.ReportParms);

    Call(EabUnprocessedCashReceiptDtl.Execute, useImport, useExport);

    local.ReportParms.Assign(useExport.ReportParms);
  }

  private bool ReadCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetDate(
          command, "receivedDate",
          local.Local10DaysPlus.ReceivedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 4);
        entities.CashReceipt.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      null,
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 13);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceTypeCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceTypeCashReceiptEvent",
      (db, command) =>
      {
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCsePersonCaseRoleCaseAssignmentServiceProvider()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CaseAssignment.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadCsePersonCaseRoleCaseAssignmentServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          entities.CashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 4);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 7);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 8);
        entities.CaseAssignment.OspCode = db.GetString(reader, 9);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 10);
        entities.CaseAssignment.CasNo = db.GetString(reader, 11);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 12);
        entities.ServiceProvider.UserId = db.GetString(reader, 13);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CaseAssignment.Populated = true;
        entities.ServiceProvider.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadLegalActionCaseRoleCaseAssignmentServiceProvider()
  {
    entities.CaseRole.Populated = false;
    entities.CaseAssignment.Populated = false;
    entities.LegalAction.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadLegalActionCaseRoleCaseAssignmentServiceProvider",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo",
          entities.CashReceiptDetail.CourtOrderNumber ?? "");
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 2);
        entities.CaseRole.CspNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 6);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 7);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 9);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 10);
        entities.CaseAssignment.OspCode = db.GetString(reader, 11);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 12);
        entities.CaseAssignment.CasNo = db.GetString(reader, 13);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 14);
        entities.ServiceProvider.UserId = db.GetString(reader, 15);
        entities.CaseRole.Populated = true;
        entities.CaseAssignment.Populated = true;
        entities.LegalAction.Populated = true;
        entities.ServiceProvider.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", entities.CaseAssignment.OffId);
        db.SetInt32(
          command, "servicePrvderId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "spdId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      null,
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ConcatenateName.
    /// </summary>
    [JsonPropertyName("concatenateName")]
    public ConcatenateName ConcatenateName
    {
      get => concatenateName ??= new();
      set => concatenateName = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of Undist.
    /// </summary>
    [JsonPropertyName("undist")]
    public CashReceiptDetail Undist
    {
      get => undist ??= new();
      set => undist = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private ConcatenateName concatenateName;
    private ProgramProcessingInfo programProcessingInfo;
    private ServiceProvider serviceProvider;
    private CashReceipt cashReceipt;
    private CashReceiptDetail undist;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptSourceType cashReceiptSourceType;
    private Office office;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Local10DaysPlus.
    /// </summary>
    [JsonPropertyName("local10DaysPlus")]
    public CashReceipt Local10DaysPlus
    {
      get => local10DaysPlus ??= new();
      set => local10DaysPlus = value;
    }

    /// <summary>
    /// A value of Discard.
    /// </summary>
    [JsonPropertyName("discard")]
    public Common Discard
    {
      get => discard ??= new();
      set => discard = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private CashReceipt local10DaysPlus;
    private Common discard;
    private Common found;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private EabFileHandling eabFileHandling;
    private ReportParms reportParms;
    private DateWorkArea current;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private ProgramProcessingInfo programProcessingInfo;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private ObligationTransaction obligationTransaction;
    private Collection collection;
    private CashReceiptEvent cashReceiptEvent;
    private ServiceProvider serviceProvider;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private Office office;
  }
#endregion
}
