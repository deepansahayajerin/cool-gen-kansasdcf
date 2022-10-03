// Program: LE_B589_INCOMING_EMP_EIWO_ACK, ID: 1902491829, model: 746.
// Short name: SWEL589P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B589_INCOMING_EMP_EIWO_ACK.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB589IncomingEmpEiwoAck: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B589_INCOMING_EMP_EIWO_ACK program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB589IncomingEmpEiwoAck(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB589IncomingEmpEiwoAck.
  /// </summary>
  public LeB589IncomingEmpEiwoAck(IContext context, Import import, Export export)
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
    // ***********************************************************************
    // *                   Maintenance Log
    // 
    // *
    // ***********************************************************************
    // *    DATE       NAME      REQUEST      DESCRIPTION                    *
    // * ----------  ---------  
    // ---------
    // --------------------------------
    // *
    // * 07/10/2015  DDupree    CQ22212     Initial Coding.                  *
    // *
    // 
    // *
    // * 11/17/2015  GVandy     CQ50342     Additional validation on document*
    // *
    // 
    // tracking number.                 *
    // *
    // 
    // *
    // * 02/05/2016  GVandy	 CQ50523     Set case number for Event 51     *
    // *				     non employer initiated details   *
    // *				     to the case number on the        *
    // *				     outging document.  Alerts will   *
    // *				     now be sent to those caseworkers *
    // *				     along with the assigned attorney *
    // *
    // 
    // *
    // * 03/28/2016  GVandy	 CQ51062     Accomodate legal action standard *
    // *
    // 
    // number in the case id field.     *
    // *
    // 
    // *
    // ***********************************************************************
    // ***********************************************************************
    // First run through the file to make sure it is complete
    // ***********************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    UseLeB589ReadFile2();

    switch(TrimEnd(local.ReturnCode.TextReturnCode))
    {
      case "00":
        break;
      case "EF":
        break;
      default:
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "ERROR OPENING EIWO EMPLOYER INPUT FILE";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
    }

    local.ErrorFound.Flag = "";
    ExitState = "ACO_NN0000_ALL_OK";

    do
    {
      local.EabFileHandling.Action = "READ";
      UseLeB589ReadFile1();

      switch(TrimEnd(local.ReturnCode.TextReturnCode))
      {
        case "00":
          break;
        case "EF":
          goto AfterCycle1;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING EIWO EMPLOYER INPUT FILE";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
      }

      switch(TrimEnd(local.DocCode.Text3))
      {
        case "FHA":
          local.FileHeaderControlNumber.Text22 = local.ControlNum.Text22;

          break;
        case "BHA":
          local.CurrentEmployer.Ein = local.HeaderEin.Text9;
          local.BatchControlNum.Text22 = local.ControlNum.Text22;
          ++local.NumberOfBatchesReceived.Count;

          break;
        case "FTA":
          if (Equal(local.ControlNum.Text22,
            local.FileHeaderControlNumber.Text22))
          {
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "NO FILE RECORD WAS RECEIVED FOR CONTROL NUMBER:  " + local
              .ControlNum.Text22;
            UseCabErrorReport1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.ErrorFound.Flag = "Y";
          }

          if (local.NumberOfBatchesReceived.Count == local.Batch.Count)
          {
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.Sent.Text15 = NumberToString(local.Batch.Count, 15);
            local.Received.Text15 =
              NumberToString(local.NumberOfBatchesReceived.Count, 15);
            local.NeededToWrite.RptDetail =
              "INCORRECT NUMBER OF BATCHES;  RECEIVED:" + local
              .Received.Text15 + " SENT:" + local.Sent.Text15 + " " + "  ";
            UseCabErrorReport1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.ErrorFound.Flag = "Y";
          }

          local.NumberOfBatchesReceived.Count = 0;

          break;
        case "BTA":
          if (Equal(local.ControlNum.Text22, local.BatchControlNum.Text22))
          {
            if (local.NumberEmpRecsRecevied.Count == local.Record.Count)
            {
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              local.Sent.Text15 = NumberToString(local.EmployeeSent.Count, 15);
              local.Received.Text15 =
                NumberToString(local.NumberEmpRecsRecevied.Count, 15);
              local.NeededToWrite.RptDetail =
                "INCORRECT NUMBER OF RECORDS;  RECEIVED:" + local
                .Received.Text15 + " SENT:" + local.Sent.Text15 + " FROM BATCH CONTROL NUMBER:" +
                local.ControlNum.Text22;
              UseCabErrorReport1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.ErrorFound.Flag = "Y";
            }

            if (local.NumberEmpRecsRecevied.Count <= 0)
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "NO DETAIL RECORD WAS RECEIVED FOR BATCH CONTORL NUMBER: " + local
                .ControlNum.Text22;
              UseCabErrorReport1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.ErrorFound.Flag = "Y";
            }

            local.TotalNumEmpRecsRecievd.Count += local.NumberEmpRecsRecevied.
              Count;
            local.NumberEmpRecsRecevied.Count = 0;
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "NO BATCH RECORD OR DETIAL RECORD WAS RECEIVED FOR CONTROL NUMBER:  " +
              local.ControlNum.Text22;
            UseCabErrorReport1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.ErrorFound.Flag = "Y";
          }

          break;
        case "ACK":
          ++local.NumberEmpRecsRecevied.Count;

          break;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "INVALID DOCUMENT CODE RECEIVED: " + local
            .DocCode.Text3;
          UseCabErrorReport1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.ErrorFound.Flag = "Y";

          break;
      }
    }
    while(!Equal(local.ReturnCode.TextReturnCode, "EF"));

AfterCycle1:

    local.EabFileHandling.Action = "CLOSE";
    UseLeB589ReadFile2();

    switch(TrimEnd(local.ReturnCode.TextReturnCode))
    {
      case "00":
        break;
      case "EF":
        break;
      default:
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "ERROR OPENING EIWO EMPLOYER INPUT FILE";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (AsChar(local.ErrorFound.Flag) == 'Y')
    {
      // There is something wrong with the file so we will not process it till 
      // it has been corrected.
      ExitState = "ACO_NE0000_FILE_ERROR";

      return;
    }

    // ***********************************************************************
    // The file has passed the initial check now it will be processed.
    // ***********************************************************************
    local.Max.Date = new DateTime(2099, 12, 31);
    local.Today.Date = Now().Date;
    UseLeB589Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.RecordsRead.Count = 0;
    local.FhiRecords.Count = 0;
    local.BhiRecords.Count = 0;
    local.FtiRecords.Count = 0;
    local.BhiRecords.Count = 0;
    local.DtlRecords.Count = 0;
    local.NoErrRecords.Count = 0;

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // **********************************************************
      // If the program is being restarted then the program will go threw the 
      // until it got to the last committed record.
      // **********************************************************
      local.Restart.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 15));

      do
      {
        local.EabFileHandling.Action = "READ";
        UseLeB589ReadFile1();

        switch(TrimEnd(local.ReturnCode.TextReturnCode))
        {
          case "00":
            break;
          case "EF":
            break;
          default:
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "ERROR READING EIWO EMPLOYER INPUT FILE";
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
        }

        ++local.RecordsRead.Count;

        if (local.Restart.Count == local.RecordsRead.Count)
        {
          goto Test1;
        }
      }
      while(!Equal(local.ReturnCode.TextReturnCode, "EF"));
    }

Test1:

    // **********************************************************
    // Read each input record from the file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      UseLeB589ReadFile2();

      switch(TrimEnd(local.ReturnCode.TextReturnCode))
      {
        case "00":
          break;
        case "EF":
          goto AfterCycle2;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING EIWO EMPLOYER INPUT FILE";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle2;
      }

      local.CheckCode.Text1 = Substring(local.DisposionReasonCd.Text3, 1, 1);

      if (!IsEmpty(local.CheckCode.Text1))
      {
      }
      else
      {
        local.CheckCode.Text1 = Substring(local.DisposionReasonCd.Text3, 2, 1);

        if (!IsEmpty(local.CheckCode.Text1))
        {
          local.DisposionReasonCd.Text3 = "";
          local.DisposionReasonCd.Text3 = local.CheckCode.Text1;
        }
        else
        {
          local.CheckCode.Text1 =
            Substring(local.DisposionReasonCd.Text3, 3, 1);

          if (!IsEmpty(local.CheckCode.Text1))
          {
            local.DisposionReasonCd.Text3 = "";
            local.DisposionReasonCd.Text3 = local.CheckCode.Text1;
          }
          else
          {
          }
        }
      }

      local.CheckCode.Text1 = Substring(local.RecDispStatsCd.Text2, 1, 1);

      if (!IsEmpty(local.RecDispStatsCd.Text2))
      {
        if (!IsEmpty(local.CheckCode.Text1))
        {
        }
        else
        {
          local.CheckCode.Text1 = Substring(local.RecDispStatsCd.Text2, 2, 1);

          if (!IsEmpty(local.CheckCode.Text1))
          {
            local.RecDispStatsCd.Text2 = "";
            local.RecDispStatsCd.Text2 = local.CheckCode.Text1;
          }
        }
      }

      local.Infrastructure.Assign(local.NullInfrastructure);
      ++local.RecordsRead.Count;
      local.DocTrackNumber12.Text12 =
        Substring(local.DocTrackingNumb.Text30, 19, 12);

      switch(TrimEnd(local.DocCode.Text3))
      {
        case "FHA":
          continue;
        case "BHA":
          continue;
        case "FTA":
          break;
        case "BTA":
          continue;
        case "ACK":
          if (Equal(local.DocActionCode.Text3, "EMP"))
          {
            // -------------------------------------------------------------------------------------
            // --
            // -- Employer initiated eIWOs.
            // --
            // -------------------------------------------------------------------------------------
            if (!IsEmpty(local.Employee.Ssn))
            {
              local.SearchCommon.Flag = "1";
              local.SearchCommon.Percentage = 0;

              local.Local1.Index = 0;
              local.Local1.Clear();

              for(local.Null1.Index = 0; local.Null1.Index < local.Null1.Count; ++
                local.Null1.Index)
              {
                if (local.Local1.IsFull)
                {
                  break;
                }

                local.Local1.Next();
              }

              local.SearchCsePersonsWorkSet.Ssn = local.Employee.Ssn;
              UseCabSearchClientBatch();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error for EMP record for SSN:  " + local.Employee.Ssn + "  " +
                  local.ExitStateWorkArea.Message;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                continue;
              }
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";

              switch(TrimEnd(local.RecDispStatsCd.Text2))
              {
                case "L":
                  local.Status.Text12 = "LUMP SUM";

                  break;
                case "S":
                  local.Status.Text12 = "SUSPENDED";

                  break;
                case "T":
                  local.Status.Text12 = "TERMINATION";

                  break;
                default:
                  local.Status.Text12 = "";

                  break;
              }

              local.ToWrite.RptDetail = local.Employee.LastName + "     " + local
                .Employee.FirstName + "     " + local.Employee.Ssn + "  " + (
                  local.Employer.Ein ?? "") + "  " + local.CaseId.Text15 + "  " +
                (local.LegalAction.StandardNumber ?? "") + "              " + local
                .Status.Text12 + "   ";
              UseCabBusinessReport02();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered when writing to the non IV-D error file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabFileHandling.Action = "WRITE";

              switch(TrimEnd(local.RecDispStatsCd.Text2))
              {
                case "L":
                  UseCabDate2TextWithHyphens2();

                  if (local.LumpSumCommon.AverageCurrency > 0)
                  {
                    local.ConvertAmt.Text15 =
                      NumberToString((long)(local.LumpSumCommon.
                        AverageCurrency * 100), 15);
                    local.ConvertAmt.Text15 =
                      Substring(local.ConvertAmt.Text15,
                      WorkArea.Text15_MaxLength, 2, 12) + "." + Substring
                      (local.ConvertAmt.Text15, WorkArea.Text15_MaxLength, 14, 2);
                      
                  }
                  else
                  {
                    local.ConvertAmt.Text15 = "0.00";
                  }

                  local.ToWrite.RptDetail =
                    "        Lump Sum Payment Date: " + local
                    .DateConverted.Text10 + "  Amt: $" + local
                    .ConvertAmt.Text15 + "  " + local.LumpSumType.Text35;

                  break;
                case "S":
                  local.ToWrite.RptDetail = "";

                  break;
                case "T":
                  UseCabDate2TextWithHyphens3();
                  UseCabDate2TextWithHyphens1();

                  if (local.FinalPaymentCommon.AverageCurrency > 0)
                  {
                    local.ConvertAmt.Text15 =
                      NumberToString((long)(local.FinalPaymentCommon.
                        AverageCurrency * 100), 15);
                    local.ConvertAmt.Text15 =
                      Substring(local.ConvertAmt.Text15,
                      WorkArea.Text15_MaxLength, 2, 12) + "." + Substring
                      (local.ConvertAmt.Text15, WorkArea.Text15_MaxLength, 14, 2);
                      
                  }
                  else
                  {
                    local.ConvertAmt.Text15 = "0.00";
                  }

                  local.ToWrite.RptDetail = "        Termination Date: " + local
                    .DateConverted.Text10 + "  Final Payment Date: " + local
                    .FinalConvert.Text10 + " Final Payment Amount: $" + local
                    .ConvertAmt.Text15;

                  break;
                default:
                  local.ToWrite.RptDetail =
                    "Invalid record disposition status code received for the employer initiated transaction.";
                    

                  break;
              }

              UseCabBusinessReport02();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered when writing to the non IV-D error file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.ToWrite.RptDetail = "DID NOT RECEIVE SSN FROM EMPLOYER";
              UseCabBusinessReport02();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered when writing to the non IV-D error file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.ToWrite.RptDetail = "";
              UseCabBusinessReport02();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered when writing to the non IV-D error file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              continue;
            }

            if (local.Local1.IsEmpty)
            {
              local.EabFileHandling.Action = "WRITE";

              switch(TrimEnd(local.RecDispStatsCd.Text2))
              {
                case "L":
                  local.Status.Text12 = "LUMP SUM";

                  break;
                case "S":
                  local.Status.Text12 = "SUSPENDED";

                  break;
                case "T":
                  local.Status.Text12 = "TERMINATION";

                  break;
                default:
                  local.Status.Text12 = "";

                  break;
              }

              local.ToWrite.RptDetail = local.Employee.LastName + "     " + local
                .Employee.FirstName + "     " + local.Employee.Ssn + "  " + (
                  local.Employer.Ein ?? "") + "  " + local.CaseId.Text15 + "  " +
                (local.LegalAction.StandardNumber ?? "") + "              " + local
                .Status.Text12 + "   ";
              UseCabBusinessReport02();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered opening driver file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabFileHandling.Action = "WRITE";

              switch(TrimEnd(local.RecDispStatsCd.Text2))
              {
                case "L":
                  UseCabDate2TextWithHyphens2();

                  if (local.LumpSumCommon.AverageCurrency > 0)
                  {
                    local.ConvertAmt.Text15 =
                      NumberToString((long)(local.LumpSumCommon.
                        AverageCurrency * 100), 15);
                    local.ConvertAmt.Text15 =
                      Substring(local.ConvertAmt.Text15,
                      WorkArea.Text15_MaxLength, 2, 12) + "." + Substring
                      (local.ConvertAmt.Text15, WorkArea.Text15_MaxLength, 14, 2);
                      
                  }
                  else
                  {
                    local.ConvertAmt.Text15 = "0.00";
                  }

                  local.ToWrite.RptDetail =
                    "        Lump Sum Payment Date: " + local
                    .DateConverted.Text10 + "  Amt: $" + local
                    .ConvertAmt.Text15 + "  " + local.LumpSumType.Text35;

                  break;
                case "S":
                  local.ToWrite.RptDetail = "";

                  break;
                case "T":
                  UseCabDate2TextWithHyphens3();
                  UseCabDate2TextWithHyphens1();

                  if (local.FinalPaymentCommon.AverageCurrency > 0)
                  {
                    local.ConvertAmt.Text15 =
                      NumberToString((long)(local.FinalPaymentCommon.
                        AverageCurrency * 100), 15);
                    local.ConvertAmt.Text15 =
                      Substring(local.ConvertAmt.Text15,
                      WorkArea.Text15_MaxLength, 2, 12) + "." + Substring
                      (local.ConvertAmt.Text15, WorkArea.Text15_MaxLength, 14, 2);
                      
                  }
                  else
                  {
                    local.ConvertAmt.Text15 = "0.00";
                  }

                  local.ToWrite.RptDetail = "        Termination Date: " + local
                    .DateConverted.Text10 + "  Final Payment Date: " + local
                    .FinalConvert.Text10 + " Final Payment Amount: $" + local
                    .ConvertAmt.Text15;

                  break;
                default:
                  local.ToWrite.RptDetail =
                    "Invalid record disposition status code received for the employer initiated transaction.";
                    

                  break;
              }

              UseCabBusinessReport02();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered opening driver file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.ToWrite.RptDetail =
                "SSN NOT FOUND IN THE SYSTEM AT ALL   " + local.Employee.Ssn;
              UseCabBusinessReport02();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered opening driver file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.ToWrite.RptDetail = "";
              UseCabBusinessReport02();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered opening driver file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              continue;
            }

            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (Equal(local.Local1.Item.Detail.Ssn, local.Employee.Ssn))
              {
                if (ReadCsePersonCase())
                {
                  local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
                  MoveCsePerson1(entities.CsePerson, local.CsePerson);
                  local.Found.Number = entities.Case1.Number;
                  local.Infrastructure.SituationNumber = 0;

                  switch(TrimEnd(local.RecDispStatsCd.Text2))
                  {
                    case "L":
                      UseCabDate2TextWithHyphens2();

                      if (local.LumpSumCommon.AverageCurrency > 0)
                      {
                        local.ConvertAmt.Text15 =
                          NumberToString((long)(local.LumpSumCommon.
                            AverageCurrency * 100), 15);
                        local.ConvertAmt.Text15 =
                          Substring(local.ConvertAmt.Text15,
                          WorkArea.Text15_MaxLength, 2, 12) + "." + Substring
                          (local.ConvertAmt.Text15, WorkArea.Text15_MaxLength,
                          14, 2);
                      }
                      else
                      {
                        local.ConvertAmt.Text15 = "0.00";
                      }

                      local.Infrastructure.ReasonCode = "EIWOEMPINILSNOT";
                      local.Infrastructure.EventId = 51;
                      local.Infrastructure.CsenetInOutCode = "";
                      local.Infrastructure.ProcessStatus = "Q";
                      local.Infrastructure.UserId = "INCS";
                      local.Infrastructure.BusinessObjectCd = "LEA";
                      local.Infrastructure.ReferenceDate = local.Today.Date;
                      local.Infrastructure.CreatedBy = global.UserId;
                      local.Infrastructure.CsePersonNumber =
                        entities.CsePerson.Number;
                      local.Infrastructure.Detail =
                        "See CSLN for Lump Sum Information received from FEIN: " +
                        (local.Employer.Ein ?? "");
                      local.EmpRecord.Name = "FEIN " + (
                        local.Employer.Ein ?? "");
                      local.Processed.StandardNumber = "";

                      foreach(var item in ReadLegalAction3())
                      {
                        if (Equal(entities.Distinct.StandardNumber,
                          local.Processed.StandardNumber))
                        {
                          continue;
                        }

                        local.Processed.StandardNumber =
                          entities.Distinct.StandardNumber;

                        if (ReadLegalAction2())
                        {
                          if (!IsEmpty(entities.LegalAction.InitiatingState) &&
                            !Equal(entities.LegalAction.InitiatingState, "KS"))
                          {
                            local.Infrastructure.InitiatingStateCode = "OS";
                          }
                          else
                          {
                            local.Infrastructure.InitiatingStateCode = "KS";
                          }

                          local.Infrastructure.DenormNumeric12 =
                            entities.LegalAction.Identifier;
                          local.Infrastructure.DenormText12 =
                            entities.LegalAction.StandardNumber;
                          local.Infrastructure.CreatedTimestamp = Now();
                          local.Infrastructure.SituationNumber = 0;
                          local.Infrastructure.CaseNumber = "";
                          UseSpCabCreateInfrastructure();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }

                          local.NarrativeDetail.InfrastructureId =
                            local.Infrastructure.SystemGeneratedIdentifier;
                          local.NarrativeDetail.CaseNumber = "";
                          local.NarrativeDetail.CreatedBy = global.UserId;
                          local.NarrativeDetail.CreatedTimestamp =
                            local.Infrastructure.CreatedTimestamp;
                          local.NarrativeDetail.LineNumber = 1;
                          local.NarrativeDetail.NarrativeText =
                            "Lump Sum information received from " + (
                              local.Employer.Ein ?? "");
                          UseSpCabCreateNarrativeDetail();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }

                          local.NarrativeDetail.LineNumber = 2;
                          local.NarrativeDetail.NarrativeText =
                            "Lump Sum Date:  " + local.DateConverted.Text10;
                          UseSpCabCreateNarrativeDetail();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }

                          local.NarrativeDetail.LineNumber = 3;
                          local.NarrativeDetail.NarrativeText =
                            "Lump Sum Amount: $ " + local.ConvertAmt.Text15;
                          UseSpCabCreateNarrativeDetail();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }

                          local.NarrativeDetail.LineNumber = 4;
                          local.NarrativeDetail.NarrativeText =
                            "Lump Sum Type: " + local.LumpSumType.Text35;
                          UseSpCabCreateNarrativeDetail();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }
                        }
                      }

                      local.Infrastructure.EventId = 52;
                      local.Infrastructure.BusinessObjectCd = "CAS";
                      local.Infrastructure.InitiatingStateCode = "";
                      local.Infrastructure.DenormNumeric12 = 0;
                      local.Infrastructure.DenormText12 = "";

                      foreach(var item in ReadCase())
                      {
                        local.Infrastructure.CreatedTimestamp = Now();
                        local.Infrastructure.CaseNumber = entities.Case1.Number;
                        local.Infrastructure.SituationNumber = 0;
                        UseSpCabCreateInfrastructure();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }

                        local.NarrativeDetail.InfrastructureId =
                          local.Infrastructure.SystemGeneratedIdentifier;
                        local.NarrativeDetail.CaseNumber =
                          entities.Case1.Number;
                        local.NarrativeDetail.CreatedBy = global.UserId;
                        local.NarrativeDetail.CreatedTimestamp =
                          local.Infrastructure.CreatedTimestamp;
                        local.NarrativeDetail.LineNumber = 1;
                        local.NarrativeDetail.NarrativeText =
                          "Lump Sum information received from FEIN: " + (
                            local.Employer.Ein ?? "");
                        UseSpCabCreateNarrativeDetail();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }

                        local.NarrativeDetail.LineNumber = 2;
                        local.NarrativeDetail.NarrativeText =
                          "Lump Sum Date:  " + local.DateConverted.Text10;
                        UseSpCabCreateNarrativeDetail();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }

                        local.NarrativeDetail.LineNumber = 3;
                        local.NarrativeDetail.NarrativeText =
                          "Lump Sum Amount: $ " + local.ConvertAmt.Text15;
                        UseSpCabCreateNarrativeDetail();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }

                        local.NarrativeDetail.LineNumber = 4;
                        local.NarrativeDetail.NarrativeText =
                          "Lump Sum Type: " + local.LumpSumType.Text35;
                        UseSpCabCreateNarrativeDetail();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      if (!IsEmpty(local.NcpLast.Street1))
                      {
                        UseLeSetUpNcplkaddress2();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      if (!IsEmpty(local.NewEmployer.Name))
                      {
                        UseLeSetUpNcpnewemployer3();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      break;
                    case "S":
                      local.Infrastructure.ReasonCode = "EIWOEMPINISUSPN";
                      local.Infrastructure.EventId = 51;
                      local.Infrastructure.CsenetInOutCode = "";
                      local.Infrastructure.ProcessStatus = "Q";
                      local.Infrastructure.UserId = "INCS";
                      local.Infrastructure.BusinessObjectCd = "LEA";
                      local.Infrastructure.ReferenceDate = local.Today.Date;
                      local.Infrastructure.CreatedBy = global.UserId;
                      local.Infrastructure.CsePersonNumber =
                        entities.CsePerson.Number;
                      local.Infrastructure.Detail =
                        "Employee Suspended Notification received from FEIN: " +
                        (local.Employer.Ein ?? "");
                      local.EmpRecord.Name = "FEIN " + (
                        local.Employer.Ein ?? "");
                      local.Processed.StandardNumber = "";

                      foreach(var item in ReadLegalAction3())
                      {
                        if (Equal(entities.Distinct.StandardNumber,
                          local.Processed.StandardNumber))
                        {
                          continue;
                        }

                        local.Processed.StandardNumber =
                          entities.Distinct.StandardNumber;

                        if (ReadLegalAction2())
                        {
                          local.Infrastructure.DenormNumeric12 =
                            entities.LegalAction.Identifier;
                          local.Infrastructure.DenormText12 =
                            entities.LegalAction.StandardNumber;
                          local.Infrastructure.SituationNumber = 0;

                          if (!IsEmpty(entities.LegalAction.InitiatingState) &&
                            !Equal(entities.LegalAction.InitiatingState, "KS"))
                          {
                            local.Infrastructure.InitiatingStateCode = "OS";
                          }
                          else
                          {
                            local.Infrastructure.InitiatingStateCode = "KS";
                          }

                          local.Infrastructure.CaseNumber = "";
                          local.Infrastructure.CreatedTimestamp = Now();
                          UseSpCabCreateInfrastructure();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }

                          local.NarrativeDetail.InfrastructureId =
                            local.Infrastructure.SystemGeneratedIdentifier;
                          local.NarrativeDetail.CaseNumber = "";
                          local.NarrativeDetail.CreatedBy = global.UserId;
                          local.NarrativeDetail.CreatedTimestamp =
                            local.Infrastructure.CreatedTimestamp;
                          local.NarrativeDetail.LineNumber = 1;
                          local.NarrativeDetail.NarrativeText =
                            "Employee Suspended Notification received from  " +
                            (local.Employer.Ein ?? "");
                          UseSpCabCreateNarrativeDetail();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }
                        }
                      }

                      local.Infrastructure.EventId = 52;
                      local.Infrastructure.BusinessObjectCd = "CAS";
                      local.Infrastructure.DenormText12 = "";
                      local.Infrastructure.DenormNumeric12 = 0;
                      local.Infrastructure.InitiatingStateCode = "";

                      foreach(var item in ReadCase())
                      {
                        local.Infrastructure.CaseNumber = entities.Case1.Number;
                        local.Infrastructure.CreatedTimestamp = Now();
                        local.Infrastructure.SituationNumber = 0;
                        UseSpCabCreateInfrastructure();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }

                        local.NarrativeDetail.InfrastructureId =
                          local.Infrastructure.SystemGeneratedIdentifier;
                        local.NarrativeDetail.CaseNumber =
                          entities.Case1.Number;
                        local.NarrativeDetail.CreatedBy = global.UserId;
                        local.NarrativeDetail.CreatedTimestamp =
                          local.Infrastructure.CreatedTimestamp;
                        local.NarrativeDetail.LineNumber = 1;
                        local.NarrativeDetail.NarrativeText =
                          "Suspension Information received from FEIN: " + (
                            local.Employer.Ein ?? "");
                        UseSpCabCreateNarrativeDetail();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      if (!IsEmpty(local.NcpLast.Street1))
                      {
                        UseLeSetUpNcplkaddress2();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      if (!IsEmpty(local.NewEmployer.Name))
                      {
                        UseLeSetUpNcpnewemployer3();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      break;
                    case "T":
                      UseCabDate2TextWithHyphens3();
                      UseCabDate2TextWithHyphens1();

                      if (local.FinalPaymentCommon.AverageCurrency > 0)
                      {
                        local.ConvertAmt.Text15 =
                          NumberToString((long)(local.FinalPaymentCommon.
                            AverageCurrency * 100), 15);
                        local.ConvertAmt.Text15 =
                          Substring(local.ConvertAmt.Text15,
                          WorkArea.Text15_MaxLength, 2, 12) + "." + Substring
                          (local.ConvertAmt.Text15, WorkArea.Text15_MaxLength,
                          14, 2);
                      }
                      else
                      {
                        local.ConvertAmt.Text15 = "0.00";
                      }

                      local.Infrastructure.ReasonCode = "EIWOEMPINITERMN";
                      local.Infrastructure.EventId = 51;
                      local.Infrastructure.CsenetInOutCode = "";
                      local.Infrastructure.ProcessStatus = "Q";
                      local.Infrastructure.UserId = "INCS";
                      local.Infrastructure.BusinessObjectCd = "LEA";
                      local.Infrastructure.ReferenceDate = local.Today.Date;
                      local.Infrastructure.CreatedBy = global.UserId;
                      local.Infrastructure.CsePersonNumber =
                        entities.CsePerson.Number;
                      local.Infrastructure.Detail =
                        "See CSLN for Termination Information received from FEIN: " +
                        (local.Employer.Ein ?? "");
                      local.EmpRecord.Name = "FEIN " + (
                        local.Employer.Ein ?? "");
                      local.Processed.StandardNumber = "";

                      foreach(var item in ReadLegalAction3())
                      {
                        if (Equal(entities.Distinct.StandardNumber,
                          local.Processed.StandardNumber))
                        {
                          continue;
                        }

                        local.Processed.StandardNumber =
                          entities.Distinct.StandardNumber;

                        if (ReadLegalAction2())
                        {
                          local.Infrastructure.DenormNumeric12 =
                            entities.LegalAction.Identifier;
                          local.Infrastructure.DenormText12 =
                            entities.LegalAction.StandardNumber;
                          local.Infrastructure.CaseNumber = "";
                          local.Infrastructure.CreatedTimestamp = Now();
                          local.Infrastructure.SituationNumber = 0;

                          if (!IsEmpty(entities.LegalAction.InitiatingState) &&
                            !Equal(entities.LegalAction.InitiatingState, "KS"))
                          {
                            local.Infrastructure.InitiatingStateCode = "OS";
                          }
                          else
                          {
                            local.Infrastructure.InitiatingStateCode = "KS";
                          }

                          UseSpCabCreateInfrastructure();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }

                          local.NarrativeDetail.InfrastructureId =
                            local.Infrastructure.SystemGeneratedIdentifier;
                          local.NarrativeDetail.CaseNumber = "";
                          local.NarrativeDetail.CreatedBy = global.UserId;
                          local.NarrativeDetail.CreatedTimestamp =
                            local.Infrastructure.CreatedTimestamp;
                          local.NarrativeDetail.LineNumber = 1;
                          local.NarrativeDetail.NarrativeText =
                            "Termination Information received from  " + (
                              local.Employer.Ein ?? "");
                          UseSpCabCreateNarrativeDetail();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }

                          local.NarrativeDetail.LineNumber = 2;
                          local.NarrativeDetail.NarrativeText =
                            "Termination Date:   " + TrimEnd
                            (local.DateConverted.Text10) + " Final Payment Date:   " +
                            local.FinalConvert.Text10;
                          UseSpCabCreateNarrativeDetail();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }

                          local.NarrativeDetail.LineNumber = 3;
                          local.NarrativeDetail.NarrativeText =
                            "Final Payment Amount: $ " + local
                            .ConvertAmt.Text15;
                          UseSpCabCreateNarrativeDetail();

                          if (!IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            return;
                          }
                        }
                      }

                      local.Infrastructure.EventId = 52;
                      local.Infrastructure.BusinessObjectCd = "CAS";
                      local.Infrastructure.InitiatingStateCode = "";
                      local.Infrastructure.DenormNumeric12 = 0;
                      local.Infrastructure.DenormText12 = "";

                      foreach(var item in ReadCase())
                      {
                        local.Infrastructure.CreatedTimestamp = Now();
                        local.Infrastructure.CaseNumber = entities.Case1.Number;
                        local.Infrastructure.SituationNumber = 0;
                        UseSpCabCreateInfrastructure();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }

                        local.NarrativeDetail.InfrastructureId =
                          local.Infrastructure.SystemGeneratedIdentifier;
                        local.NarrativeDetail.CaseNumber =
                          entities.Case1.Number;
                        local.NarrativeDetail.CreatedBy = global.UserId;
                        local.NarrativeDetail.CreatedTimestamp =
                          local.Infrastructure.CreatedTimestamp;
                        local.NarrativeDetail.LineNumber = 1;
                        local.NarrativeDetail.NarrativeText =
                          "Termination Information received from FEIN: " + (
                            local.Employer.Ein ?? "");
                        UseSpCabCreateNarrativeDetail();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }

                        local.NarrativeDetail.LineNumber = 2;
                        local.NarrativeDetail.NarrativeText =
                          "Termination Date:   " + TrimEnd
                          (local.DateConverted.Text10) + " Final Payment Date:   " +
                          local.FinalConvert.Text10;
                        UseSpCabCreateNarrativeDetail();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }

                        local.NarrativeDetail.LineNumber = 3;
                        local.NarrativeDetail.NarrativeText =
                          "Final Payment Amount: $ " + local.ConvertAmt.Text15;
                        UseSpCabCreateNarrativeDetail();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      if (!IsEmpty(local.NcpLast.Street1))
                      {
                        UseLeSetUpNcplkaddress2();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      if (!IsEmpty(local.NewEmployer.Name))
                      {
                        UseLeSetUpNcpnewemployer3();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      break;
                    default:
                      local.EabFileHandling.Action = "WRITE";
                      local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                        local.Employee.FirstName + "    " + local
                        .Employee.Ssn + "  " + "Invalid Action/Status Code Combination Received  ";
                        
                      UseCabBusinessReport01();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Error encountered when writing to IV-D error file.";
                        UseCabErrorReport2();
                        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                        return;
                      }

                      local.EabFileHandling.Action = "WRITE";
                      local.ToWrite.RptDetail = "       " + local
                        .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                        (local.Employer.Ein ?? "") + "        " + local
                        .DocTrackNumber12.Text12 + "                    " + (
                          local.LegalAction.StandardNumber ?? "") + "             " +
                        local.RecDispStatsCd.Text2 + "       " + local
                        .DisposionReasonCd.Text3;
                      UseCabBusinessReport01();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Error encountered when writing to IV-D error file.";
                        UseCabErrorReport2();
                        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                        return;
                      }

                      local.ToWrite.RptDetail = "";
                      UseCabBusinessReport01();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Error encountered when writing to IV-D error file.";
                        UseCabErrorReport2();
                        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                        return;
                      }

                      goto Next;
                  }

                  break;
                }
                else
                {
                  local.EabFileHandling.Action = "WRITE";

                  switch(TrimEnd(local.RecDispStatsCd.Text2))
                  {
                    case "L":
                      local.Status.Text12 = "LUMP SUM";

                      break;
                    case "S":
                      local.Status.Text12 = "SUSPENDED";

                      break;
                    case "T":
                      local.Status.Text12 = "TERMINATION";

                      break;
                    default:
                      local.Status.Text12 = "";

                      break;
                  }

                  local.ToWrite.RptDetail = local.Employee.LastName + "     " +
                    local.Employee.FirstName + "     " + local.Employee.Ssn + "  " +
                    (local.Employer.Ein ?? "") + "  " + local.CaseId.Text15 + "  " +
                    (local.LegalAction.StandardNumber ?? "") + "              " +
                    local.Status.Text12 + "   ";
                  UseCabBusinessReport02();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered opening driver file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  local.EabFileHandling.Action = "WRITE";

                  switch(TrimEnd(local.RecDispStatsCd.Text2))
                  {
                    case "L":
                      UseCabDate2TextWithHyphens2();

                      if (local.LumpSumCommon.AverageCurrency > 0)
                      {
                        local.ConvertAmt.Text15 =
                          NumberToString((long)(local.LumpSumCommon.
                            AverageCurrency * 100), 15);
                        local.ConvertAmt.Text15 =
                          Substring(local.ConvertAmt.Text15,
                          WorkArea.Text15_MaxLength, 2, 12) + "." + Substring
                          (local.ConvertAmt.Text15, WorkArea.Text15_MaxLength,
                          14, 2);
                      }
                      else
                      {
                        local.ConvertAmt.Text15 = "0.00";
                      }

                      local.ToWrite.RptDetail =
                        "        Lump Sum Payment Date: " + local
                        .DateConverted.Text10 + "  Amt: $" + local
                        .ConvertAmt.Text15 + "  " + local.LumpSumType.Text35;

                      break;
                    case "S":
                      local.ToWrite.RptDetail = "";

                      break;
                    case "T":
                      UseCabDate2TextWithHyphens3();
                      UseCabDate2TextWithHyphens1();

                      if (local.FinalPaymentCommon.AverageCurrency > 0)
                      {
                        local.ConvertAmt.Text15 =
                          NumberToString((long)(local.FinalPaymentCommon.
                            AverageCurrency * 100), 15);
                        local.ConvertAmt.Text15 =
                          Substring(local.ConvertAmt.Text15,
                          WorkArea.Text15_MaxLength, 2, 12) + "." + Substring
                          (local.ConvertAmt.Text15, WorkArea.Text15_MaxLength,
                          14, 2);
                      }
                      else
                      {
                        local.ConvertAmt.Text15 = "0.00";
                      }

                      local.ToWrite.RptDetail = "        Termination Date: " + local
                        .DateConverted.Text10 + "  Final Payment Date: " + local
                        .FinalConvert.Text10 + " Final Payment Amount: $" + local
                        .ConvertAmt.Text15;

                      break;
                    default:
                      local.ToWrite.RptDetail =
                        "Invalid record disposition status code received for the employer initiated transaction.";
                        

                      break;
                  }

                  UseCabBusinessReport02();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered opening driver file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  local.ToWrite.RptDetail =
                    "SSN FOUND BUT THE CSE PERSON IS NOT CURRENTLY ACTIVE AP  " +
                    local.Local1.Item.Detail.Number;
                  UseCabBusinessReport02();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered opening driver file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  local.ToWrite.RptDetail = "";
                  UseCabBusinessReport02();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered opening driver file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  goto Next;
                }
              }
            }

            continue;
          }
          else
          {
            // -------------------------------------------------------------------------------------
            // --
            // -- Employer responses to state initiated eIWOs.
            // --
            // -------------------------------------------------------------------------------------
            // 11/17/2015  GVandy  CQ50342  Additional validation on document 
            // tracking number.
            // -- Validate document tracking number.
            if (!Equal(local.DocTrackingNumb.Text30, 1, 18, "200000000000000000")
              || IsEmpty(local.DocTrackNumber12.Text12))
            {
              local.EabFileHandling.Action = "WRITE";
              local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                "Acknowledgement Rec'd for Invalid Tracking Number";
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered when writing to IV-D error file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabFileHandling.Action = "WRITE";
              local.ToWrite.RptDetail = "       " + local
                .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " + (
                  local.Employer.Ein ?? "") + "        " + local
                .DocTrackingNumb.Text30 + "  " + (
                  local.LegalAction.StandardNumber ?? "") + "             " + local
                .RecDispStatsCd.Text2 + "       " + local
                .DisposionReasonCd.Text3;
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered when writing to IV-D error file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.ToWrite.RptDetail = "";
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered when writing to IV-D error file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              continue;
            }

            if (ReadIwoAction())
            {
              if (AsChar(entities.IwoAction.StatusCd) == 'A' || AsChar
                (entities.IwoAction.StatusCd) == 'J')
              {
                local.EabFileHandling.Action = "WRITE";
                local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                  local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                  "Duplicate Acknowledgement Rec'd for Tracking Number";
                UseCabBusinessReport01();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error encountered when writing to IV-D error file.";
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.EabFileHandling.Action = "WRITE";
                local.ToWrite.RptDetail = "       " + local
                  .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                  (local.Employer.Ein ?? "") + "        " + local
                  .DocTrackNumber12.Text12 + "                    " + (
                    local.LegalAction.StandardNumber ?? "") + "             " +
                  local.RecDispStatsCd.Text2 + "       " + local
                  .DisposionReasonCd.Text3;
                UseCabBusinessReport01();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error encountered when writing to IV-D error file.";
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.ToWrite.RptDetail = "";
                UseCabBusinessReport01();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error encountered when writing to IV-D error file.";
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                continue;
              }

              if (!IsEmpty(local.DocActionCode.Text3))
              {
                if (ReadLegalAction1())
                {
                  MoveLegalAction(entities.LegalAction, local.LegalAction);
                  MoveIwoAction(entities.IwoAction, local.IwoAction);

                  if (Equal(entities.LegalAction.ActionTaken, "IWO"))
                  {
                    if (Equal(local.DocActionCode.Text3, "ORG"))
                    {
                      goto Test2;
                    }
                    else
                    {
                    }
                  }

                  if (Equal(entities.LegalAction.ActionTaken, "ORDIWO2"))
                  {
                    if (Equal(local.DocActionCode.Text3, "ORG"))
                    {
                      goto Test2;
                    }
                    else
                    {
                    }
                  }

                  if (Equal(entities.LegalAction.ActionTaken, "ORDIWO2A"))
                  {
                    if (Equal(local.DocActionCode.Text3, "ORG"))
                    {
                      goto Test2;
                    }
                    else
                    {
                    }
                  }

                  if (Equal(entities.LegalAction.ActionTaken, "IWOMODO"))
                  {
                    if (Equal(local.DocActionCode.Text3, "AMD"))
                    {
                      goto Test2;
                    }
                    else
                    {
                    }
                  }

                  if (Equal(entities.LegalAction.ActionTaken, "ORDIWOLS"))
                  {
                    if (Equal(local.DocActionCode.Text3, "LUM"))
                    {
                      goto Test2;
                    }
                    else
                    {
                    }
                  }

                  if (Equal(entities.LegalAction.ActionTaken, "ORDIWOPT"))
                  {
                    if (Equal(local.DocActionCode.Text3, "TRM"))
                    {
                      goto Test2;
                    }
                    else
                    {
                    }
                  }

                  if (Equal(entities.LegalAction.ActionTaken, "IWOTERM"))
                  {
                    if (Equal(local.DocActionCode.Text3, "TRM"))
                    {
                      goto Test2;
                    }
                    else
                    {
                    }
                  }

                  local.EabFileHandling.Action = "WRITE";
                  local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                    local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                    "Action Code Mismatch for Tracking Number ";
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered when writing to IV-D error file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  local.EabFileHandling.Action = "WRITE";
                  local.ToWrite.RptDetail = "       " + local
                    .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                    (local.Employer.Ein ?? "") + "        " + local
                    .DocTrackNumber12.Text12 + "                    " + (
                      local.LegalAction.StandardNumber ?? "") + "             " +
                    local.RecDispStatsCd.Text2 + "       " + local
                    .DisposionReasonCd.Text3;
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered when writing to IV-D error file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  local.ToWrite.RptDetail = "";
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered when writing to IV-D error file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  continue;
                }
                else
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                    local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                    "Tracking Number Received Not Tied To A Legal Action  ";
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered when writing to IV-D error file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  local.EabFileHandling.Action = "WRITE";
                  local.ToWrite.RptDetail = "       " + local
                    .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                    (local.Employer.Ein ?? "") + "        " + local
                    .DocTrackNumber12.Text12 + "                    " + (
                      local.LegalAction.StandardNumber ?? "") + "             " +
                    local.RecDispStatsCd.Text2 + "       " + local
                    .DisposionReasonCd.Text3;
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered when writing to IV-D error file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  local.ToWrite.RptDetail = "";
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered when writing to IV-D error file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  continue;
                }
              }
              else
              {
                local.EabFileHandling.Action = "WRITE";
                local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                  local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                  "No Document Action Code Received";
                UseCabBusinessReport01();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error encountered when writing to IV-D error file.";
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.EabFileHandling.Action = "WRITE";
                local.ToWrite.RptDetail = "       " + local
                  .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                  (local.Employer.Ein ?? "") + "        " + local
                  .DocTrackNumber12.Text12 + "                    " + (
                    local.LegalAction.StandardNumber ?? "") + "             " +
                  local.RecDispStatsCd.Text2 + "       " + local
                  .DisposionReasonCd.Text3;
                UseCabBusinessReport01();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error encountered when writing to IV-D error file.";
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.ToWrite.RptDetail = "";
                UseCabBusinessReport01();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error encountered when writing to IV-D error file.";
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                continue;
              }

Test2:

              switch(TrimEnd(local.DocActionCode.Text3))
              {
                case "AMD":
                  if (Equal(local.RecDispStatsCd.Text2, "A"))
                  {
                    switch(TrimEnd(local.DisposionReasonCd.Text3))
                    {
                      case "B":
                        break;
                      case "S":
                        break;
                      case "W":
                        break;
                      case "":
                        break;
                      default:
                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                          local.Employee.FirstName + "    " + local
                          .Employee.Ssn + "  " + "Invalid Status/Reason Code Combination Received";
                          
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = "       " + local
                          .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                          (local.Employer.Ein ?? "") + "        " + local
                          .DocTrackNumber12.Text12 + "                    " + (
                            local.LegalAction.StandardNumber ?? "") + "             " +
                          local.RecDispStatsCd.Text2 + "       " + local
                          .DisposionReasonCd.Text3;
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.ToWrite.RptDetail = "";
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        continue;
                    }
                  }
                  else if (Equal(local.RecDispStatsCd.Text2, "R"))
                  {
                    switch(TrimEnd(local.DisposionReasonCd.Text3))
                    {
                      case "B":
                        break;
                      case "S":
                        break;
                      case "W":
                        break;
                      case "M":
                        break;
                      case "N":
                        break;
                      case "O":
                        break;
                      case "U":
                        break;
                      case "X":
                        break;
                      case "D":
                        break;
                      default:
                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                          local.Employee.FirstName + "    " + local
                          .Employee.Ssn + "  " + "Invalid Status/Reason Code Combination Received";
                          
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = "       " + local
                          .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                          (local.Employer.Ein ?? "") + "        " + local
                          .DocTrackNumber12.Text12 + "                    " + (
                            local.LegalAction.StandardNumber ?? "") + "             " +
                          local.RecDispStatsCd.Text2 + "       " + local
                          .DisposionReasonCd.Text3;
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.ToWrite.RptDetail = "";
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        continue;
                    }
                  }
                  else
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                      local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                      "Invalid Action/Status Code Combination Received";
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    local.EabFileHandling.Action = "WRITE";
                    local.ToWrite.RptDetail = "       " + local
                      .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                      (local.Employer.Ein ?? "") + "        " + local
                      .DocTrackNumber12.Text12 + "                    " + (
                        local.LegalAction.StandardNumber ?? "") + "             " +
                      local.RecDispStatsCd.Text2 + "       " + local
                      .DisposionReasonCd.Text3;
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    local.ToWrite.RptDetail = "";
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    continue;
                  }

                  break;
                case "LUM":
                  if (Equal(local.RecDispStatsCd.Text2, "A"))
                  {
                    switch(TrimEnd(local.DisposionReasonCd.Text3))
                    {
                      case "B":
                        break;
                      case "S":
                        break;
                      case "W":
                        break;
                      case "":
                        break;
                      default:
                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                          local.Employee.FirstName + "    " + local
                          .Employee.Ssn + "  " + "Invalid Status/Reason Code Combination Received";
                          
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = "       " + local
                          .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                          (local.Employer.Ein ?? "") + "        " + local
                          .DocTrackNumber12.Text12 + "                    " + (
                            local.LegalAction.StandardNumber ?? "") + "             " +
                          local.RecDispStatsCd.Text2 + "       " + local
                          .DisposionReasonCd.Text3;
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.ToWrite.RptDetail = "";
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        continue;
                    }
                  }
                  else if (Equal(local.RecDispStatsCd.Text2, "R"))
                  {
                    switch(TrimEnd(local.DisposionReasonCd.Text3))
                    {
                      case "B":
                        break;
                      case "S":
                        break;
                      case "W":
                        break;
                      case "M":
                        break;
                      case "N":
                        break;
                      case "O":
                        break;
                      case "U":
                        break;
                      case "X":
                        break;
                      case "D":
                        break;
                      default:
                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                          local.Employee.FirstName + "    " + local
                          .Employee.Ssn + "  " + "Invalid Status/Reason Code Combination Received";
                          
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = "       " + local
                          .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                          (local.Employer.Ein ?? "") + "        " + local
                          .DocTrackNumber12.Text12 + "                    " + (
                            local.LegalAction.StandardNumber ?? "") + "             " +
                          local.RecDispStatsCd.Text2 + "       " + local
                          .DisposionReasonCd.Text3;
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.ToWrite.RptDetail = "";
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        continue;
                    }
                  }
                  else
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                      local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                      "Invalid Action /Status Code Combination Received";
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    local.EabFileHandling.Action = "WRITE";
                    local.ToWrite.RptDetail = "       " + local
                      .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                      (local.Employer.Ein ?? "") + "        " + local
                      .DocTrackNumber12.Text12 + "                    " + (
                        local.LegalAction.StandardNumber ?? "") + "             " +
                      local.RecDispStatsCd.Text2 + "       " + local
                      .DisposionReasonCd.Text3;
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    local.ToWrite.RptDetail = "";
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    continue;
                  }

                  break;
                case "ORG":
                  if (Equal(local.RecDispStatsCd.Text2, "A"))
                  {
                    switch(TrimEnd(local.DisposionReasonCd.Text3))
                    {
                      case "B":
                        break;
                      case "S":
                        break;
                      case "W":
                        break;
                      case "":
                        break;
                      default:
                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                          local.Employee.FirstName + "    " + local
                          .Employee.Ssn + "  " + "Invalid Status/Reason Code Combination Received";
                          
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = "       " + local
                          .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                          (local.Employer.Ein ?? "") + "        " + local
                          .DocTrackNumber12.Text12 + "                    " + (
                            local.LegalAction.StandardNumber ?? "") + "             " +
                          local.RecDispStatsCd.Text2 + "       " + local
                          .DisposionReasonCd.Text3;
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.ToWrite.RptDetail = "";
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        continue;
                    }
                  }
                  else if (Equal(local.RecDispStatsCd.Text2, "R"))
                  {
                    switch(TrimEnd(local.DisposionReasonCd.Text3))
                    {
                      case "B":
                        break;
                      case "S":
                        break;
                      case "W":
                        break;
                      case "M":
                        break;
                      case "N":
                        break;
                      case "O":
                        break;
                      case "U":
                        break;
                      case "X":
                        break;
                      case "D":
                        break;
                      default:
                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                          local.Employee.FirstName + "    " + local
                          .Employee.Ssn + "  " + "Invalid Status/Reason Code Combination Received";
                          
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = "       " + local
                          .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                          (local.Employer.Ein ?? "") + "        " + local
                          .DocTrackNumber12.Text12 + "                    " + (
                            local.LegalAction.StandardNumber ?? "") + "             " +
                          local.RecDispStatsCd.Text2 + "       " + local
                          .DisposionReasonCd.Text3;
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.ToWrite.RptDetail = "";
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        continue;
                    }
                  }
                  else
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                      local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                      "Invalid Action/Status Code Combination Received";
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    local.EabFileHandling.Action = "WRITE";
                    local.ToWrite.RptDetail = "       " + local
                      .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                      (local.Employer.Ein ?? "") + "        " + local
                      .DocTrackNumber12.Text12 + "                    " + (
                        local.LegalAction.StandardNumber ?? "") + "             " +
                      local.RecDispStatsCd.Text2 + "       " + local
                      .DisposionReasonCd.Text3;
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    local.ToWrite.RptDetail = "";
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    continue;
                  }

                  break;
                case "TRM":
                  if (Equal(local.RecDispStatsCd.Text2, "A"))
                  {
                    switch(TrimEnd(local.DisposionReasonCd.Text3))
                    {
                      case "B":
                        break;
                      case "S":
                        break;
                      case "W":
                        break;
                      case "":
                        break;
                      default:
                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                          local.Employee.FirstName + "    " + local
                          .Employee.Ssn + "  " + "Invalid Status/Reason Code Combination Received";
                          
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = "       " + local
                          .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                          (local.Employer.Ein ?? "") + "        " + local
                          .DocTrackNumber12.Text12 + "                    " + (
                            local.LegalAction.StandardNumber ?? "") + "             " +
                          local.RecDispStatsCd.Text2 + "       " + local
                          .DisposionReasonCd.Text3;
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.ToWrite.RptDetail = "";
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        continue;
                    }
                  }
                  else if (Equal(local.RecDispStatsCd.Text2, "R"))
                  {
                    switch(TrimEnd(local.DisposionReasonCd.Text3))
                    {
                      case "B":
                        break;
                      case "S":
                        break;
                      case "W":
                        break;
                      case "M":
                        break;
                      case "N":
                        break;
                      case "O":
                        break;
                      case "U":
                        break;
                      case "X":
                        break;
                      case "D":
                        break;
                      case "Z":
                        break;
                      default:
                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                          local.Employee.FirstName + "    " + local
                          .Employee.Ssn + "  " + "Invalid Status/Reason Code Combination Received";
                          
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.EabFileHandling.Action = "WRITE";
                        local.ToWrite.RptDetail = "       " + local
                          .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                          (local.Employer.Ein ?? "") + "        " + local
                          .DocTrackNumber12.Text12 + "                    " + (
                            local.LegalAction.StandardNumber ?? "") + "             " +
                          local.RecDispStatsCd.Text2 + "       " + local
                          .DisposionReasonCd.Text3;
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.ToWrite.RptDetail = "";
                        UseCabBusinessReport01();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error encountered when writing to IV-D error file.";
                            
                          UseCabErrorReport2();
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        continue;
                    }
                  }
                  else
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                      local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                      "Invalid Action/Status Code Combination Received";
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    local.EabFileHandling.Action = "WRITE";
                    local.ToWrite.RptDetail = "       " + local
                      .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                      (local.Employer.Ein ?? "") + "        " + local
                      .DocTrackNumber12.Text12 + "                    " + (
                        local.LegalAction.StandardNumber ?? "") + "             " +
                      local.RecDispStatsCd.Text2 + "       " + local
                      .DisposionReasonCd.Text3;
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    local.ToWrite.RptDetail = "";
                    UseCabBusinessReport01();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error encountered when writing to IV-D error file.";
                      UseCabErrorReport2();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    continue;
                  }

                  break;
                default:
                  local.EabFileHandling.Action = "WRITE";
                  local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                    local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                    "Invalid Action Code Received";
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered when writing to IV-D error file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  local.EabFileHandling.Action = "WRITE";
                  local.ToWrite.RptDetail = "       " + local
                    .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " +
                    (local.Employer.Ein ?? "") + "        " + local
                    .DocTrackNumber12.Text12 + "                    " + (
                      local.LegalAction.StandardNumber ?? "") + "             " +
                    local.RecDispStatsCd.Text2 + "       " + local
                    .DisposionReasonCd.Text3;
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered when writing to IV-D error file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  local.ToWrite.RptDetail = "";
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error encountered when writing to IV-D error file.";
                    UseCabErrorReport2();
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  continue;
              }
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              local.ToWrite.RptDetail = local.Employee.LastName + "         " +
                local.Employee.FirstName + "    " + local.Employee.Ssn + "  " +
                "Invalid Tracking Number Received  ";
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered when writing to IV-D error file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabFileHandling.Action = "WRITE";
              local.ToWrite.RptDetail = "       " + local
                .DocActionCode.Text3 + "   " + local.CaseId.Text15 + "     " + (
                  local.Employer.Ein ?? "") + "        " + local
                .DocTrackNumber12.Text12 + "                    " + (
                  local.LegalAction.StandardNumber ?? "") + "             " + local
                .RecDispStatsCd.Text2 + "       " + local
                .DisposionReasonCd.Text3;
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered when writing to IV-D error file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.ToWrite.RptDetail = "";
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered when writing to IV-D error file.";
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              continue;
            }
          }

          // -- 02/05/2016  GVandy  CQ50523 Set case number for Event 51 non 
          // employer initiated
          // details to the case number on the outging document.  Alerts will 
          // now be sent to those
          // caseworkers along with the assigned attorney.
          if (ReadFieldValue())
          {
            local.Event51.Number = Substring(entities.FieldValue.Value, 1, 10);
          }
          else
          {
            local.Event51.Number = "";
          }

          if (ReadCsePerson())
          {
            local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            MoveCsePerson1(entities.CsePerson, local.CsePerson);
          }
          else
          {
            ExitState = "CSE_PERSON_NF";
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Could not find the CSE Person tied to the document tracking # " +
              local.DocTrackNumber12.Text12 + "  " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

          ReadLegalAction1();

          if (Equal(local.RecDispStatsCd.Text2, "A"))
          {
            switch(TrimEnd(local.DisposionReasonCd.Text3))
            {
              case "B":
                local.Infrastructure.ReasonCode = "EIWOACCNAMEMM";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer1())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "S":
                local.Infrastructure.ReasonCode = "EIWOACCEMPSUSP";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer1())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "W":
                local.Infrastructure.ReasonCode = "EIWOACCINCFEIN";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                  local.Infrastructure.Detail =
                    TrimEnd(local.Infrastructure.Detail) + ";  New FEIN: " + (
                      local.Corrected.Ein ?? "");
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "":
                if (ReadEmployer1())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                local.Infrastructure.SituationNumber = 0;
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              default:
                break;
            }
          }
          else if (Equal(local.RecDispStatsCd.Text2, "R"))
          {
            switch(TrimEnd(local.DisposionReasonCd.Text3))
            {
              case "B":
                local.Infrastructure.ReasonCode = "EIWOREJNAMEMM";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "D":
                local.Infrastructure.ReasonCode = "EIWOREJDUPIWO";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "S":
                local.Infrastructure.ReasonCode = "EIWOREJEMPSUSP";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "W":
                local.Infrastructure.ReasonCode = "EIWOREJINCFEIN";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                  local.Infrastructure.Detail =
                    TrimEnd(local.Infrastructure.Detail) + ";  New FEIN: " + (
                      local.Corrected.Ein ?? "");
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "M":
                local.Infrastructure.ReasonCode = "EIWOREJMULTIPLE";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                  local.Infrastructure.Detail =
                    TrimEnd(local.Infrastructure.Detail) + "; IWO Received from another state: " +
                    local.MultiIwoStCode.Text2;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "N":
                foreach(var item in ReadIncomeSource())
                {
                  MoveIncomeSource1(entities.IncomeSource, local.IncomeSource);

                  if (Equal(entities.IncomeSource.EndDt, local.Max.Date))
                  {
                    if (Lt(new DateTime(1, 1, 1), local.TerminationDate.Date))
                    {
                      local.IncomeSource.EndDt = local.TerminationDate.Date;
                    }
                    else
                    {
                      local.IncomeSource.EndDt = local.Process.Date;
                    }

                    local.IncomeSource.ReturnDt = local.Process.Date;
                    local.IncomeSource.ReturnCd = "Q";
                    local.IncomeSource.WorkerId = global.UserId;
                    UseSiIncsUpdateIncomeSource();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }

                  foreach(var item1 in ReadLegalActionIncomeSource())
                  {
                    MoveLegalActionIncomeSource(entities.
                      LegalActionIncomeSource,
                      local.CurrentLegalActionIncomeSource);
                    local.NewLegalActionIncomeSource.Assign(
                      entities.LegalActionIncomeSource);
                    local.NewLegalActionIncomeSource.EndDate =
                      local.Process.Date;
                    local.LeinType.Text1 = "I";
                    UseLeUpdateIwoGarnishmentLien();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }
                }

                local.Infrastructure.ReasonCode = "EIWOREJNCPTERM";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "O":
                local.Infrastructure.ReasonCode = "EIWOREJOTHER";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "U":
                foreach(var item in ReadIncomeSource())
                {
                  MoveIncomeSource1(entities.IncomeSource, local.IncomeSource);

                  if (Equal(entities.IncomeSource.EndDt, local.Max.Date))
                  {
                    if (Lt(new DateTime(1, 1, 1), local.TerminationDate.Date))
                    {
                      local.IncomeSource.EndDt = local.TerminationDate.Date;
                    }
                    else
                    {
                      local.IncomeSource.EndDt = local.Process.Date;
                    }

                    local.IncomeSource.ReturnDt = local.Process.Date;
                    local.IncomeSource.ReturnCd = "N";
                    local.IncomeSource.WorkerId = global.UserId;
                    UseSiIncsUpdateIncomeSource();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }

                  foreach(var item1 in ReadLegalActionIncomeSource())
                  {
                    MoveLegalActionIncomeSource(entities.
                      LegalActionIncomeSource,
                      local.CurrentLegalActionIncomeSource);
                    local.NewLegalActionIncomeSource.Assign(
                      entities.LegalActionIncomeSource);
                    local.NewLegalActionIncomeSource.EndDate =
                      local.Process.Date;
                    local.LeinType.Text1 = "I";
                    UseLeUpdateIwoGarnishmentLien();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }
                }

                local.Infrastructure.ReasonCode = "EIWOREJNCPNK";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "X":
                local.Infrastructure.ReasonCode = "EIWOREJNOPROCES";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              case "Z":
                local.Infrastructure.ReasonCode = "EIWOREJNOTERM";
                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;

                if (ReadEmployer2())
                {
                  local.AreaCode.Text3 =
                    NumberToString(entities.Employer.AreaCode.
                      GetValueOrDefault(), 13, 3);
                  local.Infrastructure.Detail =
                    TrimEnd(entities.Employer.Name) + " " + local
                    .AreaCode.Text3 + " " + entities.Employer.PhoneNo;
                }
                else
                {
                  local.Infrastructure.Detail =
                    Spaces(Infrastructure.Detail_MaxLength);
                }

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.CaseNumber = local.Event51.Number;
                local.Infrastructure.SituationNumber = 0;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!IsEmpty(local.NcpLast.Street1))
                {
                  UseLeSetUpNcplkaddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (!IsEmpty(local.NewEmployer.Name))
                {
                  UseLeSetUpNcpnewemployer2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                break;
              default:
                break;
            }
          }

          if (ReadIwoTransaction())
          {
            local.IwoTransaction.Identifier =
              entities.IwoTransaction.Identifier;
            local.IwoAction.StatusReasonCode = local.DisposionReasonCd.Text3;
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.ToWrite.RptDetail = local.Employee.LastName + "         " + local
              .Employee.FirstName + "    " + local.Employee.Ssn + "  " + "IWO Transaction Record Not Found";
              
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error encountered when writing to IV-D error file.";
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.EabFileHandling.Action = "WRITE";
            local.ToWrite.RptDetail = "       " + local.DocActionCode.Text3 + "   " +
              local.CaseId.Text15 + "     " + (local.Employer.Ein ?? "") + "        " +
              local.DocTrackNumber12.Text12 + "                    " + (
                local.LegalAction.StandardNumber ?? "") + "             " + local
              .RecDispStatsCd.Text2 + "       " + local
              .DisposionReasonCd.Text3;
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error encountered when writing to IV-D error file.";
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.ToWrite.RptDetail = "";
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error encountered when writing to IV-D error file.";
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            continue;
          }

          if (Equal(local.RecDispStatsCd.Text2, "R"))
          {
            local.IwoAction.StatusCd = "J";
          }
          else
          {
            local.IwoAction.StatusCd = "A";
          }

          UseLeUpdateIwoActionStatus();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error for IWO ACTION CONTROL # " + (
                local.IwoAction.FileControlId ?? "") + "  " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

          break;
        default:
          break;
      }

      // *************** Check to see if commit is needed ********************
      ++local.Commit.Count;

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.ProgramCheckpointRestart.CheckpointCount =
          local.RecordsRead.Count;
        local.ProgramCheckpointRestart.RestartInfo =
          NumberToString(local.RecordsRead.Count, 250);
        UseUpdatePgmCheckpointRestart();
        UseExtToDoACommit();
        local.Commit.Count = 0;
        local.EabFileHandling.Action = "WRITE";
        local.DateDel.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-" + NumberToString
          (Now().Date.Month, 14, 2) + "-" + NumberToString
          (Now().Date.Day, 14, 2);
        local.TimeDel.Text8 =
          NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
        local.NeededToWrite.RptDetail = "Commit taken after record number: " + NumberToString
          (local.RecordsRead.Count, 15) + "  Date: " + local.DateDel.Text10 + "  Time: " +
          local.TimeDel.Text8;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          break;
        }
      }

Next:
      ;
    }
    while(!Equal(local.ReturnCode.TextReturnCode, "EF"));

AfterCycle2:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseLeB589Close();
      local.ProgramCheckpointRestart.CheckpointCount = -1;
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.ProgramName =
        local.ProgramProcessingInfo.Name;
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ALL_OK";
      UseLeB589Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCommon1(Common source, Common target)
  {
    target.Percentage = source.Percentage;
    target.Flag = source.Flag;
  }

  private static void MoveCommon2(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.OtherNumber = source.OtherNumber;
    target.OtherAreaCode = source.OtherAreaCode;
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.NumberOfColHeadings = source.NumberOfColHeadings;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
    target.ColHeading1 = source.ColHeading1;
    target.ColHeading2 = source.ColHeading2;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExport1ToLocal1(CabSearchClientBatch.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.G.SelectChar = source.G.SelectChar;
    target.Detail.Assign(source.Detail);
    MoveCommon2(source.Alt, target.Alt);
    target.Kscares.Flag = source.Kscares.Flag;
    target.Kanpay.Flag = source.Kanpay.Flag;
    target.Cse.Flag = source.Cse.Flag;
    target.Ae.Flag = source.Ae.Flag;
    target.Facts.Flag = source.Facts.Flag;
  }

  private static void MoveIncomeSource1(IncomeSource source, IncomeSource target)
    
  {
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.LastQtrIncome = source.LastQtrIncome;
    target.LastQtr = source.LastQtr;
    target.LastQtrYr = source.LastQtrYr;
    target.Attribute2NdQtrIncome = source.Attribute2NdQtrIncome;
    target.Attribute2NdQtr = source.Attribute2NdQtr;
    target.Attribute2NdQtrYr = source.Attribute2NdQtrYr;
    target.Attribute3RdQtrIncome = source.Attribute3RdQtrIncome;
    target.Attribute3RdQtr = source.Attribute3RdQtr;
    target.Attribute3RdQtrYr = source.Attribute3RdQtrYr;
    target.Attribute4ThQtrIncome = source.Attribute4ThQtrIncome;
    target.Attribute4ThQtr = source.Attribute4ThQtr;
    target.Attribute4ThQtrYr = source.Attribute4ThQtrYr;
    target.SentDt = source.SentDt;
    target.SendTo = source.SendTo;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.WorkerId = source.WorkerId;
    target.Note = source.Note;
    target.Name = source.Name;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
  }

  private static void MoveIncomeSource2(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.LastQtrIncome = source.LastQtrIncome;
    target.LastQtr = source.LastQtr;
    target.LastQtrYr = source.LastQtrYr;
    target.Attribute2NdQtrIncome = source.Attribute2NdQtrIncome;
    target.Attribute2NdQtr = source.Attribute2NdQtr;
    target.Attribute2NdQtrYr = source.Attribute2NdQtrYr;
    target.Attribute3RdQtrIncome = source.Attribute3RdQtrIncome;
    target.Attribute3RdQtr = source.Attribute3RdQtr;
    target.Attribute3RdQtrYr = source.Attribute3RdQtrYr;
    target.Attribute4ThQtrIncome = source.Attribute4ThQtrIncome;
    target.Attribute4ThQtr = source.Attribute4ThQtr;
    target.Attribute4ThQtrYr = source.Attribute4ThQtrYr;
    target.SentDt = source.SentDt;
    target.SendTo = source.SendTo;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.WorkerId = source.WorkerId;
    target.Note = source.Note;
    target.Name = source.Name;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
    target.Code = source.Code;
  }

  private static void MoveIwoAction(IwoAction source, IwoAction target)
  {
    target.Identifier = source.Identifier;
    target.StatusCd = source.StatusCd;
    target.DocumentTrackingIdentifier = source.DocumentTrackingIdentifier;
    target.FileControlId = source.FileControlId;
    target.BatchControlId = source.BatchControlId;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.Type1 = source.Type1;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalActionIncomeSource(
    LegalActionIncomeSource source, LegalActionIncomeSource target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.Identifier = source.Identifier;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.ToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.Open, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport02()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.NeededToWrite.RptDetail = local.ToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.Open, useImport.NeededToOpen);

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.FinalPaymentDateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.FinalConvert.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.LumpSumDateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.DateConverted.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens3()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.TerminationDate.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.DateConverted.Text10 = useExport.TextWorkArea.Text10;
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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabSearchClientBatch()
  {
    var useImport = new CabSearchClientBatch.Import();
    var useExport = new CabSearchClientBatch.Export();

    useImport.CsePersonsWorkSet.Ssn = local.SearchCsePersonsWorkSet.Ssn;
    MoveCommon1(local.SearchCommon, useImport.Search);

    Call(CabSearchClientBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
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

    useExport.External.NumericReturnCode = local.ReturnCode.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ReturnCode.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeB589Close()
  {
    var useImport = new LeB589Close.Import();
    var useExport = new LeB589Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;

    Call(LeB589Close.Execute, useImport, useExport);
  }

  private void UseLeB589Housekeeping()
  {
    var useImport = new LeB589Housekeeping.Import();
    var useExport = new LeB589Housekeeping.Export();

    Call(LeB589Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseLeB589ReadFile1()
  {
    var useImport = new LeB589ReadFile.Import();
    var useExport = new LeB589ReadFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.DocCode.Text3 = local.DocCode.Text3;
    useExport.ControlNum.Text22 = local.ControlNum.Text22;
    useExport.StFipsCode.Text5 = local.StFipsCode.Text5;
    useExport.HeaderEin.Text9 = local.HeaderEin.Text9;
    useExport.HeaderPrimaryEin.Text9 = local.HeaderPrimaryEin.Text9;
    useExport.HeaderCreateDate.Date = local.HeaderCreateDate.Date;
    useExport.HeaderCreateTime.Time = local.HeaderCreateTime.Time;
    useExport.ErrFieldName.Text18 = local.ErrFieldName.Text18;
    useExport.Batch.Count = local.Batch.Count;
    useExport.Record.Count = local.Record.Count;
    useExport.EmployeeSent.Count = local.EmployeeSent.Count;
    useExport.StateSent.Count = local.StateSent.Count;
    useExport.DocActionCode.Text3 = local.DocActionCode.Text3;
    useExport.Employer.Ein = local.Employer.Ein;
    useExport.Employee.Assign(local.Employee);
    useExport.DocTrackingNumb.Text30 = local.DocTrackingNumb.Text30;
    useExport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useExport.RecDispStatsCd.Text2 = local.RecDispStatsCd.Text2;
    useExport.DisposionReasonCd.Text3 = local.DisposionReasonCd.Text3;
    useExport.TerminationDate.Date = local.TerminationDate.Date;
    useExport.NcpLast.Assign(local.NcpLast);
    useExport.FinallyPayment.Date = local.FinalPaymentDateWorkArea.Date;
    useExport.FinalPayment.AverageCurrency =
      local.FinalPaymentCommon.AverageCurrency;
    useExport.NewEmployer.Name = local.NewEmployer.Name;
    useExport.NewEmployerAddress.Assign(local.NewEmployerAddress);
    useExport.LumpSumDateWorkArea.Date = local.LumpSumDateWorkArea.Date;
    useExport.LumpSumCommon.AverageCurrency =
      local.LumpSumCommon.AverageCurrency;
    useExport.LumpSumType.Text35 = local.LumpSumType.Text35;
    useExport.NcpLastKnown.Assign(local.NcpLastKnown);
    useExport.Export1StErrorFieldName.Text32 =
      local.Local1StErrorFieldName.Text32;
    useExport.Export2NdErrorFieldName.Text32 =
      local.Local2NdErrorFieldName.Text32;
    useExport.MultipleErrorInd.Text1 = local.MultipleErrorInd.Text1;
    useExport.Corrected.Ein = local.Corrected.Ein;
    useExport.MultiIwoStCode.Text2 = local.MultiIwoStCode.Text2;
    useExport.External.Assign(local.ReturnCode);
    useExport.CaseId.Text15 = local.CaseId.Text15;

    Call(LeB589ReadFile.Execute, useImport, useExport);

    local.DocCode.Text3 = useExport.DocCode.Text3;
    local.ControlNum.Text22 = useExport.ControlNum.Text22;
    local.StFipsCode.Text5 = useExport.StFipsCode.Text5;
    local.HeaderEin.Text9 = useExport.HeaderEin.Text9;
    local.HeaderPrimaryEin.Text9 = useExport.HeaderPrimaryEin.Text9;
    local.HeaderCreateDate.Date = useExport.HeaderCreateDate.Date;
    local.HeaderCreateTime.Time = useExport.HeaderCreateTime.Time;
    local.ErrFieldName.Text18 = useExport.ErrFieldName.Text18;
    local.Batch.Count = useExport.Batch.Count;
    local.Record.Count = useExport.Record.Count;
    local.EmployeeSent.Count = useExport.EmployeeSent.Count;
    local.StateSent.Count = useExport.StateSent.Count;
    local.DocActionCode.Text3 = useExport.DocActionCode.Text3;
    local.Employer.Ein = useExport.Employer.Ein;
    local.Employee.Assign(useExport.Employee);
    local.DocTrackingNumb.Text30 = useExport.DocTrackingNumb.Text30;
    local.LegalAction.StandardNumber = useExport.LegalAction.StandardNumber;
    local.RecDispStatsCd.Text2 = useExport.RecDispStatsCd.Text2;
    local.DisposionReasonCd.Text3 = useExport.DisposionReasonCd.Text3;
    local.TerminationDate.Date = useExport.TerminationDate.Date;
    local.NcpLast.Assign(useExport.NcpLast);
    local.FinalPaymentDateWorkArea.Date = useExport.FinallyPayment.Date;
    local.FinalPaymentCommon.AverageCurrency =
      useExport.FinalPayment.AverageCurrency;
    local.NewEmployer.Name = useExport.NewEmployer.Name;
    local.NewEmployerAddress.Assign(useExport.NewEmployerAddress);
    local.LumpSumDateWorkArea.Date = useExport.LumpSumDateWorkArea.Date;
    local.LumpSumCommon.AverageCurrency =
      useExport.LumpSumCommon.AverageCurrency;
    local.LumpSumType.Text35 = useExport.LumpSumType.Text35;
    MoveCsePerson2(useExport.NcpLastKnown, local.NcpLastKnown);
    local.Local1StErrorFieldName.Text32 =
      useExport.Export1StErrorFieldName.Text32;
    local.Local2NdErrorFieldName.Text32 =
      useExport.Export2NdErrorFieldName.Text32;
    local.MultipleErrorInd.Text1 = useExport.MultipleErrorInd.Text1;
    local.Corrected.Ein = useExport.Corrected.Ein;
    local.MultiIwoStCode.Text2 = useExport.MultiIwoStCode.Text2;
    local.ReturnCode.Assign(useExport.External);
    local.CaseId.Text15 = useExport.CaseId.Text15;
  }

  private void UseLeB589ReadFile2()
  {
    var useImport = new LeB589ReadFile.Import();
    var useExport = new LeB589ReadFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.DocCode.Text3 = local.DocCode.Text3;
    useExport.ControlNum.Text22 = local.ControlNum.Text22;
    useExport.StFipsCode.Text5 = local.StFipsCode.Text5;
    useExport.HeaderEin.Text9 = local.HeaderEin.Text9;
    useExport.HeaderPrimaryEin.Text9 = local.HeaderPrimaryEin.Text9;
    useExport.HeaderCreateDate.Date = local.HeaderCreateDate.Date;
    useExport.HeaderCreateTime.Time = local.HeaderCreateTime.Time;
    useExport.ErrFieldName.Text18 = local.ErrFieldName.Text18;
    useExport.DocActionCode.Text3 = local.DocActionCode.Text3;
    useExport.Employer.Ein = local.Employer.Ein;
    useExport.Employee.Assign(local.Employee);
    useExport.DocTrackingNumb.Text30 = local.DocTrackingNumb.Text30;
    useExport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useExport.RecDispStatsCd.Text2 = local.RecDispStatsCd.Text2;
    useExport.DisposionReasonCd.Text3 = local.DisposionReasonCd.Text3;
    useExport.TerminationDate.Date = local.TerminationDate.Date;
    useExport.NcpLast.Assign(local.NcpLast);
    useExport.FinallyPayment.Date = local.FinalPaymentDateWorkArea.Date;
    useExport.FinalPayment.AverageCurrency =
      local.FinalPaymentCommon.AverageCurrency;
    useExport.NewEmployer.Name = local.NewEmployer.Name;
    useExport.NewEmployerAddress.Assign(local.NewEmployerAddress);
    useExport.LumpSumDateWorkArea.Date = local.LumpSumDateWorkArea.Date;
    useExport.LumpSumCommon.AverageCurrency =
      local.LumpSumCommon.AverageCurrency;
    useExport.LumpSumType.Text35 = local.LumpSumType.Text35;
    useExport.NcpLastKnown.Assign(local.NcpLastKnown);
    useExport.Export1StErrorFieldName.Text32 =
      local.Local1StErrorFieldName.Text32;
    useExport.Export2NdErrorFieldName.Text32 =
      local.Local2NdErrorFieldName.Text32;
    useExport.MultipleErrorInd.Text1 = local.MultipleErrorInd.Text1;
    useExport.Corrected.Ein = local.Corrected.Ein;
    useExport.MultiIwoStCode.Text2 = local.MultiIwoStCode.Text2;
    useExport.External.Assign(local.ReturnCode);
    useExport.CaseId.Text15 = local.CaseId.Text15;

    Call(LeB589ReadFile.Execute, useImport, useExport);

    local.DocCode.Text3 = useExport.DocCode.Text3;
    local.ControlNum.Text22 = useExport.ControlNum.Text22;
    local.StFipsCode.Text5 = useExport.StFipsCode.Text5;
    local.HeaderEin.Text9 = useExport.HeaderEin.Text9;
    local.HeaderPrimaryEin.Text9 = useExport.HeaderPrimaryEin.Text9;
    local.HeaderCreateDate.Date = useExport.HeaderCreateDate.Date;
    local.HeaderCreateTime.Time = useExport.HeaderCreateTime.Time;
    local.ErrFieldName.Text18 = useExport.ErrFieldName.Text18;
    local.DocActionCode.Text3 = useExport.DocActionCode.Text3;
    local.Employer.Ein = useExport.Employer.Ein;
    local.Employee.Assign(useExport.Employee);
    local.DocTrackingNumb.Text30 = useExport.DocTrackingNumb.Text30;
    local.LegalAction.StandardNumber = useExport.LegalAction.StandardNumber;
    local.RecDispStatsCd.Text2 = useExport.RecDispStatsCd.Text2;
    local.DisposionReasonCd.Text3 = useExport.DisposionReasonCd.Text3;
    local.TerminationDate.Date = useExport.TerminationDate.Date;
    local.NcpLast.Assign(useExport.NcpLast);
    local.FinalPaymentDateWorkArea.Date = useExport.FinallyPayment.Date;
    local.FinalPaymentCommon.AverageCurrency =
      useExport.FinalPayment.AverageCurrency;
    local.NewEmployer.Name = useExport.NewEmployer.Name;
    local.NewEmployerAddress.Assign(useExport.NewEmployerAddress);
    local.LumpSumDateWorkArea.Date = useExport.LumpSumDateWorkArea.Date;
    local.LumpSumCommon.AverageCurrency =
      useExport.LumpSumCommon.AverageCurrency;
    local.LumpSumType.Text35 = useExport.LumpSumType.Text35;
    MoveCsePerson2(useExport.NcpLastKnown, local.NcpLastKnown);
    local.Local1StErrorFieldName.Text32 =
      useExport.Export1StErrorFieldName.Text32;
    local.Local2NdErrorFieldName.Text32 =
      useExport.Export2NdErrorFieldName.Text32;
    local.MultipleErrorInd.Text1 = useExport.MultipleErrorInd.Text1;
    local.Corrected.Ein = useExport.Corrected.Ein;
    local.MultiIwoStCode.Text2 = useExport.MultiIwoStCode.Text2;
    local.ReturnCode.Assign(useExport.External);
    local.CaseId.Text15 = useExport.CaseId.Text15;
  }

  private void UseLeSetUpNcplkaddress1()
  {
    var useImport = new LeSetUpNcplkaddress.Import();
    var useExport = new LeSetUpNcplkaddress.Export();

    MoveCsePerson1(entities.CsePerson, useImport.CsePerson);
    useImport.Employer.Assign(entities.Employer);
    useImport.NcpLastKnown.Assign(local.NcpLastKnown);
    useImport.NcpLast.Assign(local.NcpLast);
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.TodayDate.Date = local.Today.Date;
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(LeSetUpNcplkaddress.Execute, useImport, useExport);
  }

  private void UseLeSetUpNcplkaddress2()
  {
    var useImport = new LeSetUpNcplkaddress.Import();
    var useExport = new LeSetUpNcplkaddress.Export();

    MoveCsePerson1(entities.CsePerson, useImport.CsePerson);
    useImport.Employer.Name = local.EmpRecord.Name;
    useImport.NcpLastKnown.Assign(local.NcpLastKnown);
    useImport.NcpLast.Assign(local.NcpLast);

    useImport.TodayDate.Date = local.Today.Date;
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(LeSetUpNcplkaddress.Execute, useImport, useExport);
  }

  private void UseLeSetUpNcpnewemployer1()
  {
    var useImport = new LeSetUpNcpnewemployer.Import();
    var useExport = new LeSetUpNcpnewemployer.Export();

    useImport.From.Assign(entities.Employer);
    MoveCsePerson1(entities.CsePerson, useImport.CsePerson);
    useImport.NewEmployer.Assign(entities.Employer);
    useImport.NewEmployerAddress.Assign(local.NewEmployerAddress);
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.TodayDate.Date = local.Today.Date;
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(LeSetUpNcpnewemployer.Execute, useImport, useExport);
  }

  private void UseLeSetUpNcpnewemployer2()
  {
    var useImport = new LeSetUpNcpnewemployer.Import();
    var useExport = new LeSetUpNcpnewemployer.Export();

    useImport.From.Assign(entities.Employer);
    MoveCsePerson1(entities.CsePerson, useImport.CsePerson);
    useImport.NewEmployer.Name = local.NewEmployer.Name;
    useImport.NewEmployerAddress.Assign(local.NewEmployerAddress);
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.TodayDate.Date = local.Today.Date;
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(LeSetUpNcpnewemployer.Execute, useImport, useExport);
  }

  private void UseLeSetUpNcpnewemployer3()
  {
    var useImport = new LeSetUpNcpnewemployer.Import();
    var useExport = new LeSetUpNcpnewemployer.Export();

    useImport.From.Name = local.EmpRecord.Name;
    MoveCsePerson1(entities.CsePerson, useImport.CsePerson);
    useImport.NewEmployer.Name = local.NewEmployer.Name;
    useImport.NewEmployerAddress.Assign(local.NewEmployerAddress);

    useImport.TodayDate.Date = local.Today.Date;
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(LeSetUpNcpnewemployer.Execute, useImport, useExport);
  }

  private void UseLeUpdateIwoActionStatus()
  {
    var useImport = new LeUpdateIwoActionStatus.Import();
    var useExport = new LeUpdateIwoActionStatus.Export();

    useImport.IwoTransaction.Identifier = local.IwoTransaction.Identifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.LegalAction.Identifier = local.LegalAction.Identifier;
    useImport.IwoAction.Assign(local.IwoAction);

    Call(LeUpdateIwoActionStatus.Execute, useImport, useExport);
  }

  private void UseLeUpdateIwoGarnishmentLien()
  {
    var useImport = new LeUpdateIwoGarnishmentLien.Import();
    var useExport = new LeUpdateIwoGarnishmentLien.Export();

    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;
    useImport.New1.Assign(local.NewLegalActionIncomeSource);
    MoveLegalActionIncomeSource(local.CurrentLegalActionIncomeSource,
      useImport.Current);
    useImport.Type1.Text1 = local.LeinType.Text1;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.IncomeSource.Identifier = local.IncomeSource.Identifier;

    Call(LeUpdateIwoGarnishmentLien.Execute, useImport, useExport);
  }

  private void UseSiIncsUpdateIncomeSource()
  {
    var useImport = new SiIncsUpdateIncomeSource.Import();
    var useExport = new SiIncsUpdateIncomeSource.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.CsePerson.Assign(local.CsePerson);
    useImport.IncomeSource.Assign(local.IncomeSource);

    Call(SiIncsUpdateIncomeSource.Execute, useImport, useExport);

    local.CsePerson.Assign(useImport.CsePerson);
    MoveIncomeSource2(useImport.IncomeSource, local.IncomeSource);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCabCreateNarrativeDetail()
  {
    var useImport = new SpCabCreateNarrativeDetail.Import();
    var useExport = new SpCabCreateNarrativeDetail.Export();

    useImport.NarrativeDetail.Assign(local.NarrativeDetail);

    Call(SpCabCreateNarrativeDetail.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "docTrackingId", local.DocTrackNumber12.Text12);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonCase()
  {
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCsePersonCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Local1.Item.Detail.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.Case1.Number = db.GetString(reader, 2);
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadEmployer1()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", local.Employer.Ein ?? "");
        db.SetNullableString(
          command, "docTrackingId", local.DocTrackNumber12.Text12);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 3);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 4);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployer2()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "docTrackingId", local.DocTrackNumber12.Text12);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 3);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 4);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetInt32(
          command, "infIdentifier",
          entities.IwoAction.InfId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetNullableString(
          command, "docTrackingId", local.DocTrackNumber12.Text12);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.IncomeSource.LastQtr = db.GetNullableString(reader, 3);
        entities.IncomeSource.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.IncomeSource.Attribute2NdQtr = db.GetNullableString(reader, 6);
        entities.IncomeSource.Attribute2NdQtrYr =
          db.GetNullableInt32(reader, 7);
        entities.IncomeSource.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 8);
        entities.IncomeSource.Attribute3RdQtr = db.GetNullableString(reader, 9);
        entities.IncomeSource.Attribute3RdQtrYr =
          db.GetNullableInt32(reader, 10);
        entities.IncomeSource.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 11);
        entities.IncomeSource.Attribute4ThQtr =
          db.GetNullableString(reader, 12);
        entities.IncomeSource.Attribute4ThQtrYr =
          db.GetNullableInt32(reader, 13);
        entities.IncomeSource.SentDt = db.GetNullableDate(reader, 14);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 15);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 16);
        entities.IncomeSource.Name = db.GetNullableString(reader, 17);
        entities.IncomeSource.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.IncomeSource.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.IncomeSource.CreatedTimestamp = db.GetDateTime(reader, 20);
        entities.IncomeSource.CreatedBy = db.GetString(reader, 21);
        entities.IncomeSource.CspINumber = db.GetString(reader, 22);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 23);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 24);
        entities.IncomeSource.WorkerId = db.GetNullableString(reader, 25);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 26);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 27);
        entities.IncomeSource.Note = db.GetNullableString(reader, 28);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);

        return true;
      });
  }

  private bool ReadIwoAction()
  {
    entities.IwoAction.Populated = false;

    return Read("ReadIwoAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "docTrackingId", local.DocTrackNumber12.Text12);
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 2);
        entities.IwoAction.DocumentTrackingIdentifier =
          db.GetNullableString(reader, 3);
        entities.IwoAction.FileControlId = db.GetNullableString(reader, 4);
        entities.IwoAction.BatchControlId = db.GetNullableString(reader, 5);
        entities.IwoAction.ErrorRecordType = db.GetNullableString(reader, 6);
        entities.IwoAction.ErrorField1 = db.GetNullableString(reader, 7);
        entities.IwoAction.ErrorField2 = db.GetNullableString(reader, 8);
        entities.IwoAction.CspNumber = db.GetString(reader, 9);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 10);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 11);
        entities.IwoAction.InfId = db.GetNullableInt32(reader, 12);
        entities.IwoAction.Populated = true;
      });
  }

  private bool ReadIwoTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);
    entities.IwoTransaction.Populated = false;

    return Read("ReadIwoTransaction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "identifier", entities.IwoAction.IwtIdentifier);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 1);
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 2);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 4);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 5);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 6);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 7);
        entities.IwoTransaction.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "docTrackingId", local.DocTrackNumber12.Text12);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.Type1 = db.GetString(reader, 3);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.Distinct.StandardNumber ?? "");
        db.SetNullableString(command, "cspNumber", local.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.Type1 = db.GetString(reader, 3);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.Distinct.Populated = false;

    return ReadEach("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Distinct.Identifier = db.GetInt32(reader, 0);
        entities.Distinct.StandardNumber = db.GetNullableString(reader, 1);
        entities.Distinct.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.LegalActionIncomeSource.Populated = false;

    return ReadEach("ReadLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.
          SetNullableDate(command, "endDt", local.Max.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 4);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 6);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 7);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 8);
        entities.LegalActionIncomeSource.Populated = true;

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
    /// <summary>A NullGroup group.</summary>
    [Serializable]
    public class NullGroup
    {
      /// <summary>
      /// A value of GlocalNull.
      /// </summary>
      [JsonPropertyName("glocalNull")]
      public Common GlocalNull
      {
        get => glocalNull ??= new();
        set => glocalNull = value;
      }

      /// <summary>
      /// A value of DetialNull.
      /// </summary>
      [JsonPropertyName("detialNull")]
      public CsePersonsWorkSet DetialNull
      {
        get => detialNull ??= new();
        set => detialNull = value;
      }

      /// <summary>
      /// A value of AltNull.
      /// </summary>
      [JsonPropertyName("altNull")]
      public Common AltNull
      {
        get => altNull ??= new();
        set => altNull = value;
      }

      /// <summary>
      /// A value of KscaresNull.
      /// </summary>
      [JsonPropertyName("kscaresNull")]
      public Common KscaresNull
      {
        get => kscaresNull ??= new();
        set => kscaresNull = value;
      }

      /// <summary>
      /// A value of KanpayNull.
      /// </summary>
      [JsonPropertyName("kanpayNull")]
      public Common KanpayNull
      {
        get => kanpayNull ??= new();
        set => kanpayNull = value;
      }

      /// <summary>
      /// A value of CseNull.
      /// </summary>
      [JsonPropertyName("cseNull")]
      public Common CseNull
      {
        get => cseNull ??= new();
        set => cseNull = value;
      }

      /// <summary>
      /// A value of AeNull.
      /// </summary>
      [JsonPropertyName("aeNull")]
      public Common AeNull
      {
        get => aeNull ??= new();
        set => aeNull = value;
      }

      /// <summary>
      /// A value of FactsNull.
      /// </summary>
      [JsonPropertyName("factsNull")]
      public Common FactsNull
      {
        get => factsNull ??= new();
        set => factsNull = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 117;

      private Common glocalNull;
      private CsePersonsWorkSet detialNull;
      private Common altNull;
      private Common kscaresNull;
      private Common kanpayNull;
      private Common cseNull;
      private Common aeNull;
      private Common factsNull;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Common G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of Alt.
      /// </summary>
      [JsonPropertyName("alt")]
      public Common Alt
      {
        get => alt ??= new();
        set => alt = value;
      }

      /// <summary>
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>
      /// A value of Kanpay.
      /// </summary>
      [JsonPropertyName("kanpay")]
      public Common Kanpay
      {
        get => kanpay ??= new();
        set => kanpay = value;
      }

      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>
      /// A value of Ae.
      /// </summary>
      [JsonPropertyName("ae")]
      public Common Ae
      {
        get => ae ??= new();
        set => ae = value;
      }

      /// <summary>
      /// A value of Facts.
      /// </summary>
      [JsonPropertyName("facts")]
      public Common Facts
      {
        get => facts ??= new();
        set => facts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 117;

      private Common g;
      private CsePersonsWorkSet detail;
      private Common alt;
      private Common kscares;
      private Common kanpay;
      private Common cse;
      private Common ae;
      private Common facts;
    }

    /// <summary>
    /// A value of CaseId.
    /// </summary>
    [JsonPropertyName("caseId")]
    public WorkArea CaseId
    {
      get => caseId ??= new();
      set => caseId = value;
    }

    /// <summary>
    /// A value of Event51.
    /// </summary>
    [JsonPropertyName("event51")]
    public Case1 Event51
    {
      get => event51 ??= new();
      set => event51 = value;
    }

    /// <summary>
    /// A value of NullInfrastructure.
    /// </summary>
    [JsonPropertyName("nullInfrastructure")]
    public Infrastructure NullInfrastructure
    {
      get => nullInfrastructure ??= new();
      set => nullInfrastructure = value;
    }

    /// <summary>
    /// Gets a value of Null1.
    /// </summary>
    [JsonIgnore]
    public Array<NullGroup> Null1 => null1 ??= new(NullGroup.Capacity);

    /// <summary>
    /// Gets a value of Null1 for json serialization.
    /// </summary>
    [JsonPropertyName("null1")]
    [Computed]
    public IList<NullGroup> Null1_Json
    {
      get => null1;
      set => Null1.Assign(value);
    }

    /// <summary>
    /// A value of EmpRecord.
    /// </summary>
    [JsonPropertyName("empRecord")]
    public Employer EmpRecord
    {
      get => empRecord ??= new();
      set => empRecord = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public WorkArea Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Case1 Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of RecordFound.
    /// </summary>
    [JsonPropertyName("recordFound")]
    public WorkArea RecordFound
    {
      get => recordFound ??= new();
      set => recordFound = value;
    }

    /// <summary>
    /// A value of CheckCode.
    /// </summary>
    [JsonPropertyName("checkCode")]
    public WorkArea CheckCode
    {
      get => checkCode ??= new();
      set => checkCode = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public CsePersonsWorkSet Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of FileHeaderControlNumber.
    /// </summary>
    [JsonPropertyName("fileHeaderControlNumber")]
    public WorkArea FileHeaderControlNumber
    {
      get => fileHeaderControlNumber ??= new();
      set => fileHeaderControlNumber = value;
    }

    /// <summary>
    /// A value of BatchControlNum.
    /// </summary>
    [JsonPropertyName("batchControlNum")]
    public WorkArea BatchControlNum
    {
      get => batchControlNum ??= new();
      set => batchControlNum = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Common Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
    }

    /// <summary>
    /// A value of Received.
    /// </summary>
    [JsonPropertyName("received")]
    public WorkArea Received
    {
      get => received ??= new();
      set => received = value;
    }

    /// <summary>
    /// A value of Sent.
    /// </summary>
    [JsonPropertyName("sent")]
    public WorkArea Sent
    {
      get => sent ??= new();
      set => sent = value;
    }

    /// <summary>
    /// A value of TotalNumEmpRecsRecievd.
    /// </summary>
    [JsonPropertyName("totalNumEmpRecsRecievd")]
    public Common TotalNumEmpRecsRecievd
    {
      get => totalNumEmpRecsRecievd ??= new();
      set => totalNumEmpRecsRecievd = value;
    }

    /// <summary>
    /// A value of TotalStRecReceived.
    /// </summary>
    [JsonPropertyName("totalStRecReceived")]
    public Common TotalStRecReceived
    {
      get => totalStRecReceived ??= new();
      set => totalStRecReceived = value;
    }

    /// <summary>
    /// A value of StRecReceived.
    /// </summary>
    [JsonPropertyName("stRecReceived")]
    public Common StRecReceived
    {
      get => stRecReceived ??= new();
      set => stRecReceived = value;
    }

    /// <summary>
    /// A value of NumberEmpRecsRecevied.
    /// </summary>
    [JsonPropertyName("numberEmpRecsRecevied")]
    public Common NumberEmpRecsRecevied
    {
      get => numberEmpRecsRecevied ??= new();
      set => numberEmpRecsRecevied = value;
    }

    /// <summary>
    /// A value of NumberOfBatchesReceived.
    /// </summary>
    [JsonPropertyName("numberOfBatchesReceived")]
    public Common NumberOfBatchesReceived
    {
      get => numberOfBatchesReceived ??= new();
      set => numberOfBatchesReceived = value;
    }

    /// <summary>
    /// A value of CurrentEmployer.
    /// </summary>
    [JsonPropertyName("currentEmployer")]
    public Employer CurrentEmployer
    {
      get => currentEmployer ??= new();
      set => currentEmployer = value;
    }

    /// <summary>
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
    }

    /// <summary>
    /// A value of StateSent.
    /// </summary>
    [JsonPropertyName("stateSent")]
    public Common StateSent
    {
      get => stateSent ??= new();
      set => stateSent = value;
    }

    /// <summary>
    /// A value of EmployeeSent.
    /// </summary>
    [JsonPropertyName("employeeSent")]
    public Common EmployeeSent
    {
      get => employeeSent ??= new();
      set => employeeSent = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    /// <summary>
    /// A value of DocTrackNumber12.
    /// </summary>
    [JsonPropertyName("docTrackNumber12")]
    public WorkArea DocTrackNumber12
    {
      get => docTrackNumber12 ??= new();
      set => docTrackNumber12 = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public LegalAction Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of FinalConvert.
    /// </summary>
    [JsonPropertyName("finalConvert")]
    public TextWorkArea FinalConvert
    {
      get => finalConvert ??= new();
      set => finalConvert = value;
    }

    /// <summary>
    /// A value of FinalPaymentDateWorkArea.
    /// </summary>
    [JsonPropertyName("finalPaymentDateWorkArea")]
    public DateWorkArea FinalPaymentDateWorkArea
    {
      get => finalPaymentDateWorkArea ??= new();
      set => finalPaymentDateWorkArea = value;
    }

    /// <summary>
    /// A value of FinalPaymentCommon.
    /// </summary>
    [JsonPropertyName("finalPaymentCommon")]
    public Common FinalPaymentCommon
    {
      get => finalPaymentCommon ??= new();
      set => finalPaymentCommon = value;
    }

    /// <summary>
    /// A value of ConvertAmt.
    /// </summary>
    [JsonPropertyName("convertAmt")]
    public WorkArea ConvertAmt
    {
      get => convertAmt ??= new();
      set => convertAmt = value;
    }

    /// <summary>
    /// A value of DateConverted.
    /// </summary>
    [JsonPropertyName("dateConverted")]
    public TextWorkArea DateConverted
    {
      get => dateConverted ??= new();
      set => dateConverted = value;
    }

    /// <summary>
    /// A value of LumpSumType.
    /// </summary>
    [JsonPropertyName("lumpSumType")]
    public WorkArea LumpSumType
    {
      get => lumpSumType ??= new();
      set => lumpSumType = value;
    }

    /// <summary>
    /// A value of LumpSumCommon.
    /// </summary>
    [JsonPropertyName("lumpSumCommon")]
    public Common LumpSumCommon
    {
      get => lumpSumCommon ??= new();
      set => lumpSumCommon = value;
    }

    /// <summary>
    /// A value of LumpSumDateWorkArea.
    /// </summary>
    [JsonPropertyName("lumpSumDateWorkArea")]
    public DateWorkArea LumpSumDateWorkArea
    {
      get => lumpSumDateWorkArea ??= new();
      set => lumpSumDateWorkArea = value;
    }

    /// <summary>
    /// A value of MultiIwoStCode.
    /// </summary>
    [JsonPropertyName("multiIwoStCode")]
    public WorkArea MultiIwoStCode
    {
      get => multiIwoStCode ??= new();
      set => multiIwoStCode = value;
    }

    /// <summary>
    /// A value of TerminationDate.
    /// </summary>
    [JsonPropertyName("terminationDate")]
    public DateWorkArea TerminationDate
    {
      get => terminationDate ??= new();
      set => terminationDate = value;
    }

    /// <summary>
    /// A value of NewLegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("newLegalActionIncomeSource")]
    public LegalActionIncomeSource NewLegalActionIncomeSource
    {
      get => newLegalActionIncomeSource ??= new();
      set => newLegalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of CurrentLegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("currentLegalActionIncomeSource")]
    public LegalActionIncomeSource CurrentLegalActionIncomeSource
    {
      get => currentLegalActionIncomeSource ??= new();
      set => currentLegalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of LeinType.
    /// </summary>
    [JsonPropertyName("leinType")]
    public WorkArea LeinType
    {
      get => leinType ??= new();
      set => leinType = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Corrected.
    /// </summary>
    [JsonPropertyName("corrected")]
    public Employer Corrected
    {
      get => corrected ??= new();
      set => corrected = value;
    }

    /// <summary>
    /// A value of NcpPhoneNumber.
    /// </summary>
    [JsonPropertyName("ncpPhoneNumber")]
    public WorkArea NcpPhoneNumber
    {
      get => ncpPhoneNumber ??= new();
      set => ncpPhoneNumber = value;
    }

    /// <summary>
    /// A value of NcpLastKnown.
    /// </summary>
    [JsonPropertyName("ncpLastKnown")]
    public CsePerson NcpLastKnown
    {
      get => ncpLastKnown ??= new();
      set => ncpLastKnown = value;
    }

    /// <summary>
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    /// <summary>
    /// A value of ToWrite.
    /// </summary>
    [JsonPropertyName("toWrite")]
    public EabReportSend ToWrite
    {
      get => toWrite ??= new();
      set => toWrite = value;
    }

    /// <summary>
    /// A value of AreaCode.
    /// </summary>
    [JsonPropertyName("areaCode")]
    public WorkArea AreaCode
    {
      get => areaCode ??= new();
      set => areaCode = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of SearchCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchCsePersonsWorkSet
    {
      get => searchCsePersonsWorkSet ??= new();
      set => searchCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchCommon.
    /// </summary>
    [JsonPropertyName("searchCommon")]
    public Common SearchCommon
    {
      get => searchCommon ??= new();
      set => searchCommon = value;
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
    /// A value of Employee.
    /// </summary>
    [JsonPropertyName("employee")]
    public CsePersonsWorkSet Employee
    {
      get => employee ??= new();
      set => employee = value;
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
    /// A value of NewEmployerAddress.
    /// </summary>
    [JsonPropertyName("newEmployerAddress")]
    public EmployerAddress NewEmployerAddress
    {
      get => newEmployerAddress ??= new();
      set => newEmployerAddress = value;
    }

    /// <summary>
    /// A value of NewEmployer.
    /// </summary>
    [JsonPropertyName("newEmployer")]
    public Employer NewEmployer
    {
      get => newEmployer ??= new();
      set => newEmployer = value;
    }

    /// <summary>
    /// A value of NcpLast.
    /// </summary>
    [JsonPropertyName("ncpLast")]
    public CsePersonAddress NcpLast
    {
      get => ncpLast ??= new();
      set => ncpLast = value;
    }

    /// <summary>
    /// A value of NoErrRecords.
    /// </summary>
    [JsonPropertyName("noErrRecords")]
    public Common NoErrRecords
    {
      get => noErrRecords ??= new();
      set => noErrRecords = value;
    }

    /// <summary>
    /// A value of DtlRecords.
    /// </summary>
    [JsonPropertyName("dtlRecords")]
    public Common DtlRecords
    {
      get => dtlRecords ??= new();
      set => dtlRecords = value;
    }

    /// <summary>
    /// A value of BtiRecords.
    /// </summary>
    [JsonPropertyName("btiRecords")]
    public Common BtiRecords
    {
      get => btiRecords ??= new();
      set => btiRecords = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of MultipleErrorInd.
    /// </summary>
    [JsonPropertyName("multipleErrorInd")]
    public WorkArea MultipleErrorInd
    {
      get => multipleErrorInd ??= new();
      set => multipleErrorInd = value;
    }

    /// <summary>
    /// A value of Local2NdErrorFieldName.
    /// </summary>
    [JsonPropertyName("local2NdErrorFieldName")]
    public WorkArea Local2NdErrorFieldName
    {
      get => local2NdErrorFieldName ??= new();
      set => local2NdErrorFieldName = value;
    }

    /// <summary>
    /// A value of Local1StErrorFieldName.
    /// </summary>
    [JsonPropertyName("local1StErrorFieldName")]
    public WorkArea Local1StErrorFieldName
    {
      get => local1StErrorFieldName ??= new();
      set => local1StErrorFieldName = value;
    }

    /// <summary>
    /// A value of DisposionReasonCd.
    /// </summary>
    [JsonPropertyName("disposionReasonCd")]
    public WorkArea DisposionReasonCd
    {
      get => disposionReasonCd ??= new();
      set => disposionReasonCd = value;
    }

    /// <summary>
    /// A value of RecDispStatsCd.
    /// </summary>
    [JsonPropertyName("recDispStatsCd")]
    public WorkArea RecDispStatsCd
    {
      get => recDispStatsCd ??= new();
      set => recDispStatsCd = value;
    }

    /// <summary>
    /// A value of DocTrackingNumb.
    /// </summary>
    [JsonPropertyName("docTrackingNumb")]
    public TextWorkArea DocTrackingNumb
    {
      get => docTrackingNumb ??= new();
      set => docTrackingNumb = value;
    }

    /// <summary>
    /// A value of DocActionCode.
    /// </summary>
    [JsonPropertyName("docActionCode")]
    public WorkArea DocActionCode
    {
      get => docActionCode ??= new();
      set => docActionCode = value;
    }

    /// <summary>
    /// A value of ControlNum.
    /// </summary>
    [JsonPropertyName("controlNum")]
    public WorkArea ControlNum
    {
      get => controlNum ??= new();
      set => controlNum = value;
    }

    /// <summary>
    /// A value of StFipsCode.
    /// </summary>
    [JsonPropertyName("stFipsCode")]
    public WorkArea StFipsCode
    {
      get => stFipsCode ??= new();
      set => stFipsCode = value;
    }

    /// <summary>
    /// A value of HeaderEin.
    /// </summary>
    [JsonPropertyName("headerEin")]
    public WorkArea HeaderEin
    {
      get => headerEin ??= new();
      set => headerEin = value;
    }

    /// <summary>
    /// A value of HeaderPrimaryEin.
    /// </summary>
    [JsonPropertyName("headerPrimaryEin")]
    public WorkArea HeaderPrimaryEin
    {
      get => headerPrimaryEin ??= new();
      set => headerPrimaryEin = value;
    }

    /// <summary>
    /// A value of HeaderCreateDate.
    /// </summary>
    [JsonPropertyName("headerCreateDate")]
    public DateWorkArea HeaderCreateDate
    {
      get => headerCreateDate ??= new();
      set => headerCreateDate = value;
    }

    /// <summary>
    /// A value of HeaderCreateTime.
    /// </summary>
    [JsonPropertyName("headerCreateTime")]
    public DateWorkArea HeaderCreateTime
    {
      get => headerCreateTime ??= new();
      set => headerCreateTime = value;
    }

    /// <summary>
    /// A value of ErrFieldName.
    /// </summary>
    [JsonPropertyName("errFieldName")]
    public WorkArea ErrFieldName
    {
      get => errFieldName ??= new();
      set => errFieldName = value;
    }

    /// <summary>
    /// A value of DocCode.
    /// </summary>
    [JsonPropertyName("docCode")]
    public WorkArea DocCode
    {
      get => docCode ??= new();
      set => docCode = value;
    }

    /// <summary>
    /// A value of Change.
    /// </summary>
    [JsonPropertyName("change")]
    public Employer Change
    {
      get => change ??= new();
      set => change = value;
    }

    /// <summary>
    /// A value of FhiRecords.
    /// </summary>
    [JsonPropertyName("fhiRecords")]
    public Common FhiRecords
    {
      get => fhiRecords ??= new();
      set => fhiRecords = value;
    }

    /// <summary>
    /// A value of TimeDel.
    /// </summary>
    [JsonPropertyName("timeDel")]
    public TextWorkArea TimeDel
    {
      get => timeDel ??= new();
      set => timeDel = value;
    }

    /// <summary>
    /// A value of DateDel.
    /// </summary>
    [JsonPropertyName("dateDel")]
    public TextWorkArea DateDel
    {
      get => dateDel ??= new();
      set => dateDel = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of BhiRecords.
    /// </summary>
    [JsonPropertyName("bhiRecords")]
    public Common BhiRecords
    {
      get => bhiRecords ??= new();
      set => bhiRecords = value;
    }

    /// <summary>
    /// A value of FtiRecords.
    /// </summary>
    [JsonPropertyName("ftiRecords")]
    public Common FtiRecords
    {
      get => ftiRecords ??= new();
      set => ftiRecords = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public External ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Today.
    /// </summary>
    [JsonPropertyName("today")]
    public DateWorkArea Today
    {
      get => today ??= new();
      set => today = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
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

    private WorkArea caseId;
    private Case1 event51;
    private Infrastructure nullInfrastructure;
    private Array<NullGroup> null1;
    private Employer empRecord;
    private WorkArea status;
    private Case1 found;
    private WorkArea recordFound;
    private WorkArea checkCode;
    private CsePersonsWorkSet temp;
    private WorkArea fileHeaderControlNumber;
    private WorkArea batchControlNum;
    private Common restart;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common errorFound;
    private WorkArea received;
    private WorkArea sent;
    private Common totalNumEmpRecsRecievd;
    private Common totalStRecReceived;
    private Common stRecReceived;
    private Common numberEmpRecsRecevied;
    private Common numberOfBatchesReceived;
    private Employer currentEmployer;
    private Common record;
    private Common stateSent;
    private Common employeeSent;
    private Common batch;
    private IwoTransaction iwoTransaction;
    private WorkArea docTrackNumber12;
    private LegalAction processed;
    private TextWorkArea finalConvert;
    private DateWorkArea finalPaymentDateWorkArea;
    private Common finalPaymentCommon;
    private WorkArea convertAmt;
    private TextWorkArea dateConverted;
    private WorkArea lumpSumType;
    private Common lumpSumCommon;
    private DateWorkArea lumpSumDateWorkArea;
    private WorkArea multiIwoStCode;
    private DateWorkArea terminationDate;
    private LegalActionIncomeSource newLegalActionIncomeSource;
    private LegalActionIncomeSource currentLegalActionIncomeSource;
    private WorkArea leinType;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private Employer corrected;
    private WorkArea ncpPhoneNumber;
    private CsePerson ncpLastKnown;
    private NarrativeDetail narrativeDetail;
    private EabReportSend toWrite;
    private WorkArea areaCode;
    private Array<LocalGroup> local1;
    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private Common searchCommon;
    private Case1 case1;
    private CsePersonsWorkSet employee;
    private LegalAction legalAction;
    private EmployerAddress newEmployerAddress;
    private Employer newEmployer;
    private CsePersonAddress ncpLast;
    private Common noErrRecords;
    private Common dtlRecords;
    private Common btiRecords;
    private IwoAction iwoAction;
    private WorkArea multipleErrorInd;
    private WorkArea local2NdErrorFieldName;
    private WorkArea local1StErrorFieldName;
    private WorkArea disposionReasonCd;
    private WorkArea recDispStatsCd;
    private TextWorkArea docTrackingNumb;
    private WorkArea docActionCode;
    private WorkArea controlNum;
    private WorkArea stFipsCode;
    private WorkArea headerEin;
    private WorkArea headerPrimaryEin;
    private DateWorkArea headerCreateDate;
    private DateWorkArea headerCreateTime;
    private WorkArea errFieldName;
    private WorkArea docCode;
    private Employer change;
    private Common fhiRecords;
    private TextWorkArea timeDel;
    private TextWorkArea dateDel;
    private Common recordsRead;
    private Common bhiRecords;
    private Common ftiRecords;
    private Common commit;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private DateWorkArea max;
    private DateWorkArea nullDateWorkArea;
    private Employer employer;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private EabReportSend eabReportSend;
    private Infrastructure infrastructure;
    private DateWorkArea today;
    private EabReportSend open;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public CaseRole Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of Distinct.
    /// </summary>
    [JsonPropertyName("distinct")]
    public LegalAction Distinct
    {
      get => distinct ??= new();
      set => distinct = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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

    private DocumentField documentField;
    private Field field;
    private FieldValue fieldValue;
    private OutgoingDocument outgoingDocument;
    private CaseRole other;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalAction distinct;
    private LegalActionIncomeSource legalActionIncomeSource;
    private Employer employer;
    private IncomeSource incomeSource;
    private IwoTransaction iwoTransaction;
    private LegalActionPerson legalActionPerson;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private IwoAction iwoAction;
    private LegalAction legalAction;
    private Case1 case1;
    private CaseRole absentParent;
    private LegalActionCaseRole legalActionCaseRole;
  }
#endregion
}
