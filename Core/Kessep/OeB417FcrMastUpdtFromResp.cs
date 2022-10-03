// Program: OE_B417_FCR_MAST_UPDT_FROM_RESP, ID: 374565426, model: 746.
// Short name: SWEE417B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B417_FCR_MAST_UPDT_FROM_RESP.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB417FcrMastUpdtFromResp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B417_FCR_MAST_UPDT_FROM_RESP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB417FcrMastUpdtFromResp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB417FcrMastUpdtFromResp.
  /// </summary>
  public OeB417FcrMastUpdtFromResp(IContext context, Import import,
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
    // **************************************************************************************
    // This Batch Procedure updates the CSE FCR Master information tables (
    // Master & Members).
    // This procedure will read the CSE Master File/CSE FCR transmission file/
    // FCR Response
    // file and update CSE FCR Tables based on the information sent and received
    // from FCR.
    // **************************************************************************************
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 07/31/2009  Raj S              CQ7190      ***** Initial Coding *****
    // *
    // *
    // 
    // *
    // ***************************************************************************************
    // * 11/09/2010  LSS                CQ23059     Set action type code of "L" 
    // to "A" to    *
    // *
    // 
    // keep from having missing FCR Member due  *
    // *
    // 
    // to Locate Request                        *
    // *
    // 
    // *
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB417Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // ***************************************************************************************
      // Program SWEEB417 was terminated abonormally in the earlier run, program
      // has to retrieve
      // the restart information and updated the appropriate views to restart at
      // right point.
      // ***************************************************************************************
      local.RestartCaseId.CaseId =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
      local.CaseCreCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 11, 7));
      local.CaseDelCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 18, 7));
      local.CaseSkipCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 25, 7));
      local.CaseUpdCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 32, 7));
      local.PersonsCreCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 39, 7));
      local.PersonDelCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 46, 7));
      local.PersonSkipCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 53, 7));
      local.PersonUpdCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 60, 7));
      local.TotalCreCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 67, 7));
      local.TotalDelCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 74, 7));
      local.TotalSkipCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 81, 7));
      local.TotalUpdCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 88, 7));
    }

    if (Equal(local.ProgramProcessingInfo.ParameterList, 1, 10, "MAST"))
    {
      // **************************************************************************************
      // If the Program Processing information record parameter list has a value
      // of MAST then
      // the program execution is to load all the existing FCR recrods both Case
      // Master and Case
      // Member records to DB2 table. This parameter can be used for initial 
      // loading and yearly
      // reconciliation process.
      // **************************************************************************************
      local.ExtractNResponseFlag.Flag = "M";
    }
    else if (Equal(local.ProgramProcessingInfo.ParameterList, 1, 10, "RECO"))
    {
      // **************************************************************************************
      // This flag will indicate that program will process the reconciliation 
      // records.  This way
      // this program can handle reconciliation process also.   The latest file 
      // we received from
      // FCR/OCSE will be considered as latest MASTER file and process will 
      // Update/Delete the
      // existing records.
      // **************************************************************************************
      local.ExtractNResponseFlag.Flag = "V";
    }
    else
    {
      // **************************************************************************************
      // This flag will indicate the process that it is trying to prcess the FCR
      // Response Records.
      // **************************************************************************************
      local.ExtractNResponseFlag.Flag = "R";
    }

    do
    {
      local.Eab.FileInstruction = "READ";

      // **************************************************************************************
      // The below mentioned External Action Block (EAB) will read the input 
      // records from the
      // FCR Master Dataset to load the data to FCR DB2 master tables(Case & 
      // Case Member Table).
      // **************************************************************************************
      UseOeEabReadFcrMasterRecord();

      if (Equal(local.Eab.TextReturnCode, "EF"))
      {
        break;
      }

      if (!IsEmpty(local.Eab.TextReturnCode))
      {
        local.EabReportSend.RptDetail = local.Eab.FileInstruction + local
          .Eab.TextLine8 + local.Eab.TextReturnCode + local.Eab.TextLine80 + local
          .FcrMasterCaseRecord.ErrorCode4 + local
          .FcrMasterCaseRecord.ErrorCode5;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      ++local.FcrMasterReadCount.Count;

      if (!IsEmpty(local.FcrMasterCaseRecord.CaseId))
      {
        local.FcrCaseMaster.CaseId = local.FcrMasterCaseRecord.CaseId;
      }
      else
      {
        local.FcrCaseMaster.CaseId = local.FcrMasterMemberRecord.CaseId;
      }

      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y' && !
        IsEmpty(local.RestartCaseId.CaseId) && !
        Lt(local.RestartCaseId.CaseId, local.FcrCaseMaster.CaseId))
      {
        continue;
      }

      // CQ23059
      // Don't want to skip "L" action types so change them to "A" to be 
      // processed
      // in the action block appropriately.
      if (AsChar(local.FcrMasterMemberRecord.ActionTypeCode) == 'L')
      {
        local.FcrMasterMemberRecord.ActionTypeCode = "A";
      }

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() && !
        Equal(local.FcrCaseMaster.CaseId, local.PreviousFcrCaseId.CaseId))
      {
        UseExtToDoACommit();

        if (local.Eab.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.EabReportSend.RptDetail =
          "Commit Taken after Commit Count reached: " + NumberToString
          (local.Commit.Count, 15);
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.Commit.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ProgramCheckpointRestart.RestartInfo =
          local.FcrCaseMaster.CaseId + NumberToString
          (local.CaseCreCount.Count, 9, 7) + NumberToString
          (local.CaseDelCount.Count, 9, 7) + NumberToString
          (local.CaseSkipCount.Count, 9, 7) + NumberToString
          (local.CaseUpdCount.Count, 9, 7) + NumberToString
          (local.PersonsCreCount.Count, 9, 7) + NumberToString
          (local.PersonDelCount.Count, 9, 7) + NumberToString
          (local.PersonSkipCount.Count, 9, 7) + NumberToString
          (local.PersonUpdCount.Count, 9, 7) + NumberToString
          (local.TotalCreCount.Count, 9, 7) + NumberToString
          (local.TotalDelCount.Count, 9, 7) + NumberToString
          (local.TotalSkipCount.Count, 9, 7) + NumberToString
          (local.TotalUpdCount.Count, 9, 7);
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      local.PreviousFcrCaseId.CaseId = local.FcrCaseMaster.CaseId;

      if (Equal(local.FcrMasterCaseRecord.PreviousCaseId, "0000000000"))
      {
        local.FcrMasterCaseRecord.PreviousCaseId = "";
      }

      local.FcrMasterCaseRecord.CreatedTimestamp = Now();
      local.FcrMasterCaseRecord.CreatedBy = "SWEEB417";
      local.FcrMasterCaseRecord.FcrCaseResponseDate =
        local.ProgramProcessingInfo.ProcessDate;

      if (AsChar(local.ExtractNResponseFlag.Flag) == 'M' && !
        IsEmpty(local.FcrMasterCaseRecord.CaseId))
      {
        local.FcrMasterCaseRecord.CaseSentDateToFcr = new DateTime(1990, 1, 1);
        local.FcrMasterCaseRecord.FcrCaseResponseDate =
          AddDays(local.FcrMasterCaseRecord.CaseSentDateToFcr, 3);

        if (Verify(Substring(
          local.FcrMasterCaseRecord.CaseUserField,
          FcrMasterCaseRecord.CaseUserField_MaxLength, 1, 6), "0123456789") == 0
          && Length(TrimEnd(local.FcrMasterCaseRecord.CaseUserField)) == 6)
        {
          if (Lt("59", Substring(local.FcrMasterCaseRecord.CaseUserField,
            FcrMasterCaseRecord.CaseUserField_MaxLength, 1, 2)))
          {
            local.FcrMasterCaseRecord.CaseSentDateToFcr =
              IntToDate((int)StringToNumber("19" +
              Substring(local.FcrMasterCaseRecord.CaseUserField,
              FcrMasterCaseRecord.CaseUserField_MaxLength, 1, 6)));
          }
          else
          {
            local.FcrMasterCaseRecord.CaseSentDateToFcr =
              IntToDate((int)StringToNumber("20" +
              Substring(local.FcrMasterCaseRecord.CaseUserField,
              FcrMasterCaseRecord.CaseUserField_MaxLength, 1, 6)));
          }

          local.FcrMasterCaseRecord.FcrCaseResponseDate =
            AddDays(local.FcrMasterCaseRecord.CaseSentDateToFcr, 3);
        }

        if (Verify(local.FcrMasterCaseRecord.BatchNumber, " 0123456789") > 0)
        {
          local.FcrMasterCaseRecord.BatchNumber = "999999";
        }

        if (Verify(local.FcrMasterCaseRecord.FipsCountyCode, "0123456789") > 0)
        {
          local.FcrMasterCaseRecord.FipsCountyCode = "";
        }

        foreach(var item in ReadInfrastructure())
        {
          if (Equal(entities.Existing.ReferenceDate, local.NullDate.Date))
          {
            continue;
          }

          local.FcrMasterCaseRecord.FcrCaseResponseDate =
            entities.Existing.ReferenceDate;

          if (Equal(local.FcrMasterCaseRecord.CaseSentDateToFcr,
            new DateTime(1990, 1, 1)))
          {
            local.FcrMasterCaseRecord.CaseSentDateToFcr =
              AddDays(entities.Existing.ReferenceDate, -3);
          }

          local.FcrMasterCaseRecord.CreatedTimestamp =
            entities.Existing.CreatedTimestamp;
          local.FcrMasterCaseRecord.CreatedBy = entities.Existing.CreatedBy;

          break;
        }
      }

      UseOeB417CreateNUpdateFcrMast();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // **************************************************************************************
        // The following condition will not happen, if it happend we don't want 
        // the program abort
        // instead we will report this record in the error report and continue 
        // our FCR Case and
        // Member load process. After reporting reset the exit state to 
        // Processing OK.
        // **************************************************************************************
        if (IsExitState("CASE_NF"))
        {
          local.EabReportSend.RptDetail = local.FcrMasterMemberRecord.CaseId + "    " +
            local.FcrMasterMemberRecord.MemberId + "    " + local
            .FcrMasterMemberRecord.ActionTypeCode + "  *******  FCR Case Not found in FCR Case Master while adding the Part Record *******";
            
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.PersonSkipCount.Count;
          ++local.TotalSkipCount.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }

        return;
      }

      ++local.Commit.Count;
    }
    while(!Equal(local.Eab.TextReturnCode, "EF"));

    // **************************************************************************************
    // The program reached the end process either Normal or abnormal based on 
    // the termination
    // process will call the appropritate action blocks and write error message 
    // to sysout.
    // **************************************************************************************
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // **************************************************************************************
      // The program encountered an exception, the message needs to retrieved 
      // from exit state
      // and same needs to be printed in sysout for reference.
      // **************************************************************************************
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB417Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseOeB417Close();

      // **************************************************************************************
      // The processing completed successfully, time to update the program 
      // checkpoint restart
      // record for program SWEEB417 with NO restart information.
      // **************************************************************************************
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.CheckpointCount = 0;
      UseUpdatePgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.NeededToWrite.RptDetail =
          "Successful End of job, but error in update checkpoint restart.  Exitstate msg is: " +
          local.ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_AE0000_BATCH_ABEND";
      }
    }
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
    target.TextLine8 = source.TextLine8;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.Eab.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.Eab.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeB417Close()
  {
    var useImport = new OeB417Close.Import();
    var useExport = new OeB417Close.Export();

    useImport.ProcessingDate.Date = local.ProcessingDate.Date;
    useImport.TotalFcrMasterCount.Count = local.FcrMasterReadCount.Count;
    useImport.TotalCreCount.Count = local.TotalCreCount.Count;
    useImport.TotalUpdCount.Count = local.TotalUpdCount.Count;
    useImport.TotalDelCount.Count = local.TotalDelCount.Count;
    useImport.TotalSkipCount.Count = local.TotalSkipCount.Count;
    useImport.CaseCreCount.Count = local.CaseCreCount.Count;
    useImport.CaseUpdCount.Count = local.CaseUpdCount.Count;
    useImport.CaseDelCount.Count = local.CaseDelCount.Count;
    useImport.CaseSkipCount.Count = local.CaseSkipCount.Count;
    useImport.PersonsCreCount.Count = local.PersonsCreCount.Count;
    useImport.PersonsUpdCount.Count = local.PersonUpdCount.Count;
    useImport.PersonsDelCount.Count = local.PersonDelCount.Count;
    useImport.PersonsSkipCount.Count = local.PersonSkipCount.Count;

    Call(OeB417Close.Execute, useImport, useExport);
  }

  private void UseOeB417CreateNUpdateFcrMast()
  {
    var useImport = new OeB417CreateNUpdateFcrMast.Import();
    var useExport = new OeB417CreateNUpdateFcrMast.Export();

    useImport.FcrMasterCaseRecord.Assign(local.FcrMasterCaseRecord);
    useImport.FcrMasterMemberRecord.Assign(local.FcrMasterMemberRecord);
    useImport.PersonsCreCount.Count = local.PersonsCreCount.Count;
    useImport.PersonUpdCount.Count = local.PersonUpdCount.Count;
    useImport.PersonDelCount.Count = local.PersonDelCount.Count;
    useImport.PersonSkipCount.Count = local.PersonSkipCount.Count;
    useImport.CaseCreCount.Count = local.CaseCreCount.Count;
    useImport.CaseUpdCount.Count = local.CaseUpdCount.Count;
    useImport.CaseDelCount.Count = local.CaseDelCount.Count;
    useImport.CaseSkipCount.Count = local.CaseSkipCount.Count;
    useImport.TotalCreCount.Count = local.TotalCreCount.Count;
    useImport.TotalUpdCount.Count = local.TotalUpdCount.Count;
    useImport.TotalDelCount.Count = local.TotalDelCount.Count;
    useImport.TotalSkipCount.Count = local.TotalSkipCount.Count;
    useImport.Commit.Count = local.Commit.Count;
    useImport.ExtractNResponseFlag.Flag = local.ExtractNResponseFlag.Flag;
    useImport.CaseDelSendDate.CaseSentDateToFcr =
      local.CaseDelSendDate.CaseSentDateToFcr;

    Call(OeB417CreateNUpdateFcrMast.Execute, useImport, useExport);

    local.PersonsCreCount.Count = useExport.PersonsCreCount.Count;
    local.PersonUpdCount.Count = useExport.PersonUpdCount.Count;
    local.PersonDelCount.Count = useExport.PersonDelCount.Count;
    local.PersonSkipCount.Count = useExport.PersonSkipCount.Count;
    local.CaseCreCount.Count = useExport.CaseCreCount.Count;
    local.CaseUpdCount.Count = useExport.CaseUpdCount.Count;
    local.CaseDelCount.Count = useExport.CaseDelCount.Count;
    local.CaseSkipCount.Count = useExport.CaseSkipCount.Count;
    local.TotalCreCount.Count = useExport.TotalCreCount.Count;
    local.TotalUpdCount.Count = useExport.TotalUpdCount.Count;
    local.TotalDelCount.Count = useExport.TotalDelCount.Count;
    local.TotalSkipCount.Count = useExport.TotalSkipCount.Count;
    local.Commit.Count = useExport.Commit.Count;
    local.CaseDelSendDate.CaseSentDateToFcr =
      useExport.CaseDelSendDate.CaseSentDateToFcr;
  }

  private void UseOeB417Housekeeping()
  {
    var useImport = new OeB417Housekeeping.Import();
    var useExport = new OeB417Housekeeping.Export();

    Call(OeB417Housekeeping.Execute, useImport, useExport);

    local.ProcessingDate.Date = useExport.ProcessingDate.Date;
    local.CurrentDateLessFive.Date = useExport.CurrentDateLessFive.Date;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseOeEabReadFcrMasterRecord()
  {
    var useImport = new OeEabReadFcrMasterRecord.Import();
    var useExport = new OeEabReadFcrMasterRecord.Export();

    MoveExternal(local.Eab, useImport.ExternalFileStatus);
    MoveExternal(local.Eab, useExport.ExternalFileStatus);
    useExport.FcrMasterCaseRecord.Assign(local.FcrMasterCaseRecord);
    useExport.FcrMasterMemberRecord.Assign(local.FcrMasterMemberRecord);

    Call(OeEabReadFcrMasterRecord.Execute, useImport, useExport);

    local.Eab.Assign(useExport.ExternalFileStatus);
    local.FcrMasterCaseRecord.Assign(useExport.FcrMasterCaseRecord);
    local.FcrMasterMemberRecord.Assign(useExport.FcrMasterMemberRecord);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.FcrCaseMaster.CaseId);
      },
      (db, reader) =>
      {
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.ReasonCode = db.GetString(reader, 1);
        entities.Existing.CaseNumber = db.GetNullableString(reader, 2);
        entities.Existing.CreatedBy = db.GetString(reader, 3);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Existing.ReferenceDate = db.GetNullableDate(reader, 5);
        entities.Existing.Populated = true;

        return true;
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
    /// A value of CaseDelSendDate.
    /// </summary>
    [JsonPropertyName("caseDelSendDate")]
    public FcrCaseMaster CaseDelSendDate
    {
      get => caseDelSendDate ??= new();
      set => caseDelSendDate = value;
    }

    /// <summary>
    /// A value of FcrCaseMaster.
    /// </summary>
    [JsonPropertyName("fcrCaseMaster")]
    public FcrCaseMaster FcrCaseMaster
    {
      get => fcrCaseMaster ??= new();
      set => fcrCaseMaster = value;
    }

    /// <summary>
    /// A value of PreviousFcrCaseId.
    /// </summary>
    [JsonPropertyName("previousFcrCaseId")]
    public FcrCaseMaster PreviousFcrCaseId
    {
      get => previousFcrCaseId ??= new();
      set => previousFcrCaseId = value;
    }

    /// <summary>
    /// A value of RestartCaseId.
    /// </summary>
    [JsonPropertyName("restartCaseId")]
    public FcrCaseMaster RestartCaseId
    {
      get => restartCaseId ??= new();
      set => restartCaseId = value;
    }

    /// <summary>
    /// A value of FcrMasterMemberRecord.
    /// </summary>
    [JsonPropertyName("fcrMasterMemberRecord")]
    public FcrMasterMemberRecord FcrMasterMemberRecord
    {
      get => fcrMasterMemberRecord ??= new();
      set => fcrMasterMemberRecord = value;
    }

    /// <summary>
    /// A value of FcrMasterCaseRecord.
    /// </summary>
    [JsonPropertyName("fcrMasterCaseRecord")]
    public FcrMasterCaseRecord FcrMasterCaseRecord
    {
      get => fcrMasterCaseRecord ??= new();
      set => fcrMasterCaseRecord = value;
    }

    /// <summary>
    /// A value of ExtractNResponseFlag.
    /// </summary>
    [JsonPropertyName("extractNResponseFlag")]
    public Common ExtractNResponseFlag
    {
      get => extractNResponseFlag ??= new();
      set => extractNResponseFlag = value;
    }

    /// <summary>
    /// A value of FcrMasterReadCount.
    /// </summary>
    [JsonPropertyName("fcrMasterReadCount")]
    public Common FcrMasterReadCount
    {
      get => fcrMasterReadCount ??= new();
      set => fcrMasterReadCount = value;
    }

    /// <summary>
    /// A value of TotalCreCount.
    /// </summary>
    [JsonPropertyName("totalCreCount")]
    public Common TotalCreCount
    {
      get => totalCreCount ??= new();
      set => totalCreCount = value;
    }

    /// <summary>
    /// A value of TotalUpdCount.
    /// </summary>
    [JsonPropertyName("totalUpdCount")]
    public Common TotalUpdCount
    {
      get => totalUpdCount ??= new();
      set => totalUpdCount = value;
    }

    /// <summary>
    /// A value of PersonsCreCount.
    /// </summary>
    [JsonPropertyName("personsCreCount")]
    public Common PersonsCreCount
    {
      get => personsCreCount ??= new();
      set => personsCreCount = value;
    }

    /// <summary>
    /// A value of CaseCreCount.
    /// </summary>
    [JsonPropertyName("caseCreCount")]
    public Common CaseCreCount
    {
      get => caseCreCount ??= new();
      set => caseCreCount = value;
    }

    /// <summary>
    /// A value of PersonUpdCount.
    /// </summary>
    [JsonPropertyName("personUpdCount")]
    public Common PersonUpdCount
    {
      get => personUpdCount ??= new();
      set => personUpdCount = value;
    }

    /// <summary>
    /// A value of CaseUpdCount.
    /// </summary>
    [JsonPropertyName("caseUpdCount")]
    public Common CaseUpdCount
    {
      get => caseUpdCount ??= new();
      set => caseUpdCount = value;
    }

    /// <summary>
    /// A value of TotalSkipCount.
    /// </summary>
    [JsonPropertyName("totalSkipCount")]
    public Common TotalSkipCount
    {
      get => totalSkipCount ??= new();
      set => totalSkipCount = value;
    }

    /// <summary>
    /// A value of TotalDelCount.
    /// </summary>
    [JsonPropertyName("totalDelCount")]
    public Common TotalDelCount
    {
      get => totalDelCount ??= new();
      set => totalDelCount = value;
    }

    /// <summary>
    /// A value of CaseSkipCount.
    /// </summary>
    [JsonPropertyName("caseSkipCount")]
    public Common CaseSkipCount
    {
      get => caseSkipCount ??= new();
      set => caseSkipCount = value;
    }

    /// <summary>
    /// A value of CaseDelCount.
    /// </summary>
    [JsonPropertyName("caseDelCount")]
    public Common CaseDelCount
    {
      get => caseDelCount ??= new();
      set => caseDelCount = value;
    }

    /// <summary>
    /// A value of PersonSkipCount.
    /// </summary>
    [JsonPropertyName("personSkipCount")]
    public Common PersonSkipCount
    {
      get => personSkipCount ??= new();
      set => personSkipCount = value;
    }

    /// <summary>
    /// A value of PersonDelCount.
    /// </summary>
    [JsonPropertyName("personDelCount")]
    public Common PersonDelCount
    {
      get => personDelCount ??= new();
      set => personDelCount = value;
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
    /// A value of ProcessingDate.
    /// </summary>
    [JsonPropertyName("processingDate")]
    public DateWorkArea ProcessingDate
    {
      get => processingDate ??= new();
      set => processingDate = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of CurrentDateLessFive.
    /// </summary>
    [JsonPropertyName("currentDateLessFive")]
    public DateWorkArea CurrentDateLessFive
    {
      get => currentDateLessFive ??= new();
      set => currentDateLessFive = value;
    }

    /// <summary>
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public External Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    private FcrCaseMaster caseDelSendDate;
    private FcrCaseMaster fcrCaseMaster;
    private FcrCaseMaster previousFcrCaseId;
    private FcrCaseMaster restartCaseId;
    private FcrMasterMemberRecord fcrMasterMemberRecord;
    private FcrMasterCaseRecord fcrMasterCaseRecord;
    private Common extractNResponseFlag;
    private Common fcrMasterReadCount;
    private Common totalCreCount;
    private Common totalUpdCount;
    private Common personsCreCount;
    private Common caseCreCount;
    private Common personUpdCount;
    private Common caseUpdCount;
    private Common totalSkipCount;
    private Common totalDelCount;
    private Common caseSkipCount;
    private Common caseDelCount;
    private Common personSkipCount;
    private Common personDelCount;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea processingDate;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private EabReportSend eabReportSend;
    private DateWorkArea nullDate;
    private DateWorkArea currentDateLessFive;
    private External eab;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabReportSend neededToWrite;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Infrastructure Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Infrastructure existing;
  }
#endregion
}
