// Program: FN_B793_CALL_CENTER_NCP_EXTRACT, ID: 1902420532, model: 746.
// Short name: SWEF793B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B793_CALL_CENTER_NCP_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB793CallCenterNcpExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B793_CALL_CENTER_NCP_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB793CallCenterNcpExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB793CallCenterNcpExtract.
  /// </summary>
  public FnB793CallCenterNcpExtract(IContext context, Import import,
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
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // -------------------------------------------------------
    // 10/09/2013  GVandy	CQ38344		Initial Development.
    // -----------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------
    // --
    // --  This program extracts NCP payment (i.e. cash receipt detail) 
    // information for submission
    // --  to the call center IVR (interactive voice response) system.
    // --
    // --  If a NCP number and reporting timeframe is not specified on the PPI 
    // record then the
    // --  program defaults to all NCP payments within the previous 90 days.
    // --
    // --  The extract file created by this program is FTPd to the call center 
    // and the most recent
    // --  30 versions of the file will be available via GDG backup.
    // --
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";

    // -----------------------------------------------------------------------------------------------
    // --  General Housekeeping and Initializations.
    // -----------------------------------------------------------------------------------------------
    UseFnB793BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (!IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Initialization Cab Error..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // --  Read all Cash Receipt Details with collection dates during the 
    // reporting timeframe.
    // -----------------------------------------------------------------------------------------------
    foreach(var item in ReadCashReceiptDetailCashReceiptTypeCashReceipt())
    {
      // --  Eliminate non-cash collections.
      if (ReadCollectionType())
      {
        if (AsChar(entities.CollectionType.CashNonCashInd) != 'C')
        {
          continue;
        }
      }
      else
      {
        // --  write to error file...
        local.EabReportSend.RptDetail =
          "Collection type not found for cash receipt detail...Obligor = " + entities
          .CashReceiptDetail.ObligorPersonNumber + "  Cash Receipt Detail Number = " +
          NumberToString(entities.CashReceipt.SequentialNumber, 7, 9) + "-" + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // -- Skip the collection.
        continue;
      }

      // -- Skip payments from deceased NCPs.
      if (ReadCsePerson())
      {
        if (!Lt(Now().Date, entities.CsePerson.DateOfDeath) && Lt
          (local.Null1.Date, entities.CsePerson.DateOfDeath))
        {
          // -- Skip deceased NCPs.
          continue;
        }
      }
      else
      {
        continue;
      }

      ++local.ReadCount.Count;

      if (!Equal(entities.CsePerson.Number, local.Previous.Number))
      {
        // -- Retrieve the obligor name, date of birth, and SSN.
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseEabReadCsePersonBatch();

        if (IsEmpty(local.AbendData.Type1))
        {
          // -- Successful Adabas read occurred.
        }
        else
        {
          switch(AsChar(local.AbendData.Type1))
          {
            case 'A':
              // -- Unsuccessful Adabas read occurred.
              switch(TrimEnd(local.AbendData.AdabasResponseCd))
              {
                case "0113":
                  local.EabReportSend.RptDetail =
                    "Adabas response code 113, Obligor cse person number " + entities
                    .CsePerson.Number + " not found in Adabas.";

                  break;
                case "0148":
                  local.EabReportSend.RptDetail =
                    "Adabas response code 148, Adabas unavailable.  Obligor cse person number " +
                    entities.CsePerson.Number + ".";

                  break;
                default:
                  local.EabReportSend.RptDetail =
                    "Adabas error, response code = " + local
                    .AbendData.AdabasResponseCd + ", type = " + local
                    .AbendData.Type1 + ", Obligor cse person number = " + entities
                    .CsePerson.Number;

                  break;
              }

              break;
            case 'C':
              // -- CICS action failed.
              local.EabReportSend.RptDetail = "CICS error, response code = " + local
                .AbendData.CicsResponseCd + ", for Obligor cse person number = " +
                entities.CsePerson.Number;

              break;
            default:
              // -- Action failed.
              local.EabReportSend.RptDetail =
                "Unknown Adabas error, type = " + local.AbendData.Type1 + ", for Obligor cse person number = " +
                entities.CsePerson.Number;

              break;
          }

          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }

          if (AsChar(local.AbendData.Type1) == 'A' && Equal
            (local.AbendData.AdabasResponseCd, "0113"))
          {
            // -- No need to abend if the NCP is not found on Adabas, just log 
            // to the error file.
            continue;
          }
          else
          {
            // -- Any errors beside the NCP not being found on Adabas should 
            // abend.
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        // -- Do not include the payment if the person does not have a date of 
        // birth.
        if (!Lt(local.Null1.Date, local.CsePersonsWorkSet.Dob))
        {
          continue;
        }

        // -- Do not include the payment if the person does not have a SSN.
        if (!Lt("000000000", local.CsePersonsWorkSet.Ssn))
        {
          continue;
        }

        local.Previous.Number = entities.CsePerson.Number;

        // -- Only commiting after all collections for any particular obligor 
        // are read.
        // -- Checkpointing is deliberately excluded.
        // -- The program will always start from the beginning and create the 
        // entire file.
        if (local.ReadCount.Count > local
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
        {
          local.ReadCount.Count = 0;
          UseExtToDoACommit();

          if (local.External.NumericReturnCode != 0)
          {
            local.EabReportSend.RptDetail =
              "(01) Error in External Commit Routine.  Return Code = " + NumberToString
              (local.External.NumericReturnCode, 14, 2);
            UseCabErrorReport2();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

      // -- Keep a running detail record count and total payment amount to use 
      // on the footer record.
      ++local.Extract.Count;
      local.Extract.TotalCurrency += entities.CashReceiptDetail.
        CollectionAmount;

      // -----------------------------------------------------------------------------------------------
      // --  Write Payment Info (Record Type = 2) to the Extract File.
      // -----------------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "WRITE";
      local.RecordType.Text1 = "2";
      UseFnB793WriteExtractFile1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // --  write to error file...
        local.EabReportSend.RptDetail =
          "(01) Error writing payment info to extract file...  Returned Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        return;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // --  Write Footer (Record Type = 3) to the Extract File.
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.RecordType.Text1 = "3";
    UseFnB793WriteExtractFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // --  write to error file...
      local.EabReportSend.RptDetail =
        "(02) Error writing footer info to extract file...  Returned Status = " +
        local.EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ERROR_WRITING_TO_FILE_AB";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -----------------------------------------------------------------------------------------------
    for(local.Counter.Count = 1; local.Counter.Count <= 3; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Number of NCP Payments Extracted . ." + NumberToString
            (local.Extract.Count, 9, 7);

          break;
        case 2:
          local.WorkArea.Text15 =
            NumberToString((long)(local.Extract.TotalCurrency * 100), 15);
          local.EabReportSend.RptDetail =
            "Amount of NCP Payments Extracted . ." + Substring
            (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 1, 13) + "." + Substring
            (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);

          break;
        case 3:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(01) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // --  Take a final Checkpoint.
    // -----------------------------------------------------------------------------------------------
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      local.EabReportSend.RptDetail =
        "(04) Error in External Commit Routine.  Return Code = " + NumberToString
        (local.External.NumericReturnCode, 14, 2);
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // --  Close the Extract File.
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseFnB793WriteExtractFile3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Extract File...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB793BatchInitialization()
  {
    var useImport = new FnB793BatchInitialization.Import();
    var useExport = new FnB793BatchInitialization.Export();

    Call(FnB793BatchInitialization.Execute, useImport, useExport);

    local.ReportingPeriodStarting.Date = useExport.ReportingPeriodStarting.Date;
    local.ReportingPeriodEnding.Date = useExport.ReportingPeriodEnding.Date;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseFnB793WriteExtractFile1()
  {
    var useImport = new FnB793WriteExtractFile.Import();
    var useExport = new FnB793WriteExtractFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.RecordType.Text1 = local.RecordType.Text1;
    MoveCashReceiptDetail(entities.CashReceiptDetail,
      useImport.DetailCashReceiptDetail);
    useImport.DetailCsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB793WriteExtractFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB793WriteExtractFile2()
  {
    var useImport = new FnB793WriteExtractFile.Import();
    var useExport = new FnB793WriteExtractFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.RecordType.Text1 = local.RecordType.Text1;
    MoveCommon(local.Extract, useImport.Footer);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB793WriteExtractFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB793WriteExtractFile3()
  {
    var useImport = new FnB793WriteExtractFile.Import();
    var useExport = new FnB793WriteExtractFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB793WriteExtractFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptTypeCashReceipt()
  {
    entities.CashReceiptType.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptTypeCashReceipt",
      (db, command) =>
      {
        db.SetDate(
          command, "date1",
          local.ReportingPeriodStarting.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2",
          local.ReportingPeriodEnding.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 7);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 9);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 10);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 11);
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.CashNonCashInd = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
        CheckValid<CollectionType>("CashNonCashInd",
          entities.CollectionType.CashNonCashInd);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.CashReceiptDetail.ObligorPersonNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Extract.
    /// </summary>
    [JsonPropertyName("extract")]
    public Common Extract
    {
      get => extract ??= new();
      set => extract = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEnding.
    /// </summary>
    [JsonPropertyName("reportingPeriodEnding")]
    public DateWorkArea ReportingPeriodEnding
    {
      get => reportingPeriodEnding ??= new();
      set => reportingPeriodEnding = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStarting.
    /// </summary>
    [JsonPropertyName("reportingPeriodStarting")]
    public DateWorkArea ReportingPeriodStarting
    {
      get => reportingPeriodStarting ??= new();
      set => reportingPeriodStarting = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of ReadCount.
    /// </summary>
    [JsonPropertyName("readCount")]
    public Common ReadCount
    {
      get => readCount ??= new();
      set => readCount = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    private TextWorkArea recordType;
    private WorkArea workArea;
    private Common extract;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private DateWorkArea reportingPeriodEnding;
    private DateWorkArea reportingPeriodStarting;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private Common readCount;
    private CsePerson previous;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private External external;
    private DateWorkArea null1;
    private Common counter;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CsePerson csePerson;
    private CashReceiptType cashReceiptType;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
  }
#endregion
}
