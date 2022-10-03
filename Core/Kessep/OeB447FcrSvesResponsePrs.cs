// Program: OE_B447_FCR_SVES_RESPONSE_PRS, ID: 945065799, model: 746.
// Short name: SWEE447B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B447_FCR_SVES_RESPONSE_PRS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB447FcrSvesResponsePrs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B447_FCR_SVES_RESPONSE_PRS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB447FcrSvesResponsePrs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB447FcrSvesResponsePrs.
  /// </summary>
  public OeB447FcrSvesResponsePrs(IContext context, Import import, Export export)
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
    // ******************************************************************************************
    // * This batch process reads the SVES responses from FCR this includes 
    // Title-II Pending,   *
    // * Title-II, Title-XVI, Prisoner and
    // Not Found records.
    // 
    // *
    // *
    // 
    // *
    // * During this first phase implementation, the process will process only 
    // Title-II pending *
    // * SVES records and generate required worker alerts & information required
    // for Narrative  *
    // * Detail screen.
    // 
    // *
    // *
    // 
    // *
    // * This process will be extended in the future to process other SVES type 
    // records.        *
    // ******************************************************************************************
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
    // * 12/03/2010  Raj S              CQ5577      Initial Coding.
    // *
    // *
    // 
    // *
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseOeB447Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **********************************************************
    // Read each input record from the file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      local.FcrSvesTitleIiPendingClaim.Assign(
        local.NullFcrSvesTitleIiPendingClaim);
      UseEabReadAllFcrSvesTypeRecs();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          break;
        case "EOF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING FCR SVES RESPONSE INPUT FILE";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      ++local.TotalSvesInputRecords.Count;
      local.Kaecses.Number = Substring(local.FcrSvesGenInfo.MemberId, 6, 10);
      local.CsePerson.Number = Substring(local.FcrSvesGenInfo.MemberId, 6, 10);

      if (ReadCsePerson())
      {
        // ******************************************************************************************
        // * Check the FCR SVES Response SSN is in CSE Bad SSN list, if so, skip
        // from processing    *
        // *
        // 
        // *
        // * Check whether SVES response record has Corrected SSN,if so, use 
        // that SSN for           *
        // * Processing.
        // 
        // *
        // ******************************************************************************************
        if (!IsEmpty(local.FcrSvesGenInfo.MultipleSsn))
        {
          local.ConvertSsn.SsnNum9 =
            (int)StringToNumber(local.FcrSvesGenInfo.MultipleSsn);
        }
        else
        {
          local.ConvertSsn.SsnNum9 =
            (int)StringToNumber(local.FcrSvesGenInfo.Ssn);
        }

        if (ReadInvalidSsn())
        {
          if (!IsEmpty(local.FcrSvesGenInfo.MultipleSsn))
          {
            local.ConvertMessage.SsnTextPart1 =
              Substring(local.FcrSvesGenInfo.MultipleSsn, 1, 3);
            local.ConvertMessage.SsnTextPart2 =
              Substring(local.FcrSvesGenInfo.MultipleSsn, 4, 2);
            local.ConvertMessage.SsnTextPart3 =
              Substring(local.FcrSvesGenInfo.MultipleSsn, 6, 4);
          }
          else
          {
            local.ConvertMessage.SsnTextPart1 =
              Substring(local.FcrSvesGenInfo.Ssn, 1, 3);
            local.ConvertMessage.SsnTextPart2 =
              Substring(local.FcrSvesGenInfo.Ssn, 4, 2);
            local.ConvertMessage.SsnTextPart3 =
              Substring(local.FcrSvesGenInfo.Ssn, 6, 4);
          }

          local.Message2.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
            .ConvertMessage.SsnTextPart2 + "-" + local
            .ConvertMessage.SsnTextPart3;
          local.Message1.Text8 = "Bad SSN";
          local.Message1.Text6 = ", Per";
          local.Message1.Text16 = ": Rec not used -";
          local.Message1.Text2 = ",";
          local.Message1.Text1 = "";
          local.Message1.Text80 =
            TrimEnd(local.FcrSvesGenInfo.LocateSourceResponseAgencyCo) + local
            .Message1.Text2 + TrimEnd("") + "" + TrimEnd("") + local
            .Message1.Text2 + TrimEnd("") + local.Message1.Text1 + TrimEnd
            ("") + local.Message1.Text2 + "FCR SVES Response Record  ";
          local.NeededToWrite.RptDetail = local.Message1.Text8 + local
            .Message2.Text11 + local.Message1.Text6 + local
            .FcrSvesGenInfo.MemberId + local.Message1.Text16 + local
            .Message1.Text80;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.TotalBadSsnRecords.Count;
          local.Message1.Text8 = "";
          local.Message1.Text6 = "";
          local.Message1.Text16 = "";
          local.Message1.Text2 = "";
          local.Message1.Text1 = "";
          local.Message1.Text80 = "";
          local.Message2.Text11 = "";
          local.NeededToWrite.RptDetail = "";

          continue;
        }
        else
        {
          // this is fine, there is not invalid ssn record for this combination 
          // of cse person number and ssn number
        }

        UseSiCabReadAdabasBatch();

        if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
        {
          ++local.TotalSvesT16Records.Count;
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ACO_ADABAS_PERSON_NF_113"))
        {
          ++local.TotalSvesT16Records.Count;
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ADABAS_INVALID_SSN_W_RB"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          break;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          break;
        }
      }
      else
      {
        ++local.TotalPersonNfRecods.Count;
        local.NeededToWrite.RptDetail = "SVES Person Not Found - (" + TrimEnd
          (local.FcrSvesGenInfo.LocateSourceResponseAgencyCo) + ") - " + local
          .FcrSvesGenInfo.MemberId + local.FcrSvesGenInfo.MemberId + "  " + TrimEnd
          (local.FcrSvesGenInfo.ReturnedLastName) + ", " + TrimEnd
          (local.FcrSvesGenInfo.ReturnedFirstName) + " " + "" + (
            local.FcrSvesGenInfo.ReturnedMiddleName ?? "");
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }

        continue;
      }

      // **************************************************************************************
      // ** Create/Update SVES General Information for all SVES Responses
      // **
      // **
      // 
      // **
      // **************************************************************************************
      local.FcrSvesGenInfo.ResponseReceivedDate = local.Process.Date;
      local.FcrSvesGenInfo.CreatedBy =
        local.ProgramCheckpointRestart.ProgramName;
      local.FcrSvesGenInfo.CreatedTimestamp = Now();
      local.FcrSvesGenInfo.LastUpdatedBy =
        local.ProgramCheckpointRestart.ProgramName;
      local.FcrSvesGenInfo.LastUpdatedTimestamp = Now();

      if (Equal(local.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E04") || Equal
        (local.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E05") || Equal
        (local.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E06") || Equal
        (local.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E07") || Equal
        (local.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E10"))
      {
        // ****************************************************************************************
        // ** Skip E10 Records from processing because we don't need to store 
        // this information   **
        // ** to new SVES tables.
        // 
        // **
        // ****************************************************************************************
        if (Equal(local.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E10"))
        {
          goto Test;
        }

        // ****************************************************************************************
        // ** Skip Prision records with release date is 3 year older than the 
        // processing date     **
        // ****************************************************************************************
        if (Equal(local.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E07") && !
          Equal(local.FcrSvesPrison.ReleaseDate, new DateTime(1, 1, 1)))
        {
          local.DatePlus3Years.Date =
            AddYears(local.FcrSvesPrison.ReleaseDate, 3);

          if (Lt(local.DatePlus3Years.Date, local.Process.Date))
          {
            ++local.TotalPrisonSkipRecs.Count;
            ++local.TotalSvesPrisonRecords.Count;
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Prison record skipped release date is 3 year old:" + local
              .FcrSvesGenInfo.MemberId + " Relaase Date: " + NumberToString
              (DateToInt(local.FcrSvesPrison.ReleaseDate), 8, 8);
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            continue;
          }
        }

        // ****************************************************************************************
        // ** Check for Valid FCR SVES response record and continue processing
        // **
        // ****************************************************************************************
        UseOeB447SvesGeneralInfoPrs();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++local.TotalSvesSkipRecords.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
      }
      else
      {
        // ****************************************************************************************
        // ** Invliad FCR SVES response record skip the record and continue to 
        // read next record. **
        // ****************************************************************************************
        ++local.TotalSvesSkipRecords.Count;

        continue;
      }

Test:

      // ****************************************************************************************
      // ** Set default vlaues to infrastructure record so that respective SVES 
      // action block   **
      // ** will use these common value to generate History/Alert Records.
      // **
      // ****************************************************************************************
      local.Infrastructure.EventId = 10;
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.ReferenceDate = local.Process.Date;
      local.Infrastructure.UserId = local.ProgramProcessingInfo.Name;
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.BusinessObjectCd = "FCR";
      local.Infrastructure.CreatedBy = global.UserId;
      local.Infrastructure.LastUpdatedBy = "";
      local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;

      if (!IsEmpty(local.FcrSvesGenInfo.MultipleSsn))
      {
        local.Infrastructure.Detail = "FCR SSN: " + (
          local.FcrSvesGenInfo.MultipleSsn ?? "") + "  Use FCR web application to view SVES/SSA Info.";
          
      }
      else
      {
        local.Infrastructure.Detail = "FCR SSN: " + (
          local.FcrSvesGenInfo.Ssn ?? "") + "  Use FCR web application to view SVES/SSA Info.";
          
      }

      // **************************************************************************************
      // ** Check SVES response agency code value (E04/E05/E06 etc.) based on 
      // the value call **
      // ** respection action block to 
      // process the SVES record.
      // 
      // **
      // **************************************************************************************
      switch(TrimEnd(local.FcrSvesGenInfo.LocateSourceResponseAgencyCo))
      {
        case "E04":
          ++local.TotalSvesT2PendRecords.Count;

          // ****************************************************************************************
          // ** Current SVES record is SVES Title-II pending claim record and 
          // use Title-II pending **
          // ** claim action block to process the currently read record.
          // **
          // ****************************************************************************************
          local.FcrSvesTitleIiPend.SeqNo = 1;
          local.FcrSvesTitleIiPend.CreatedBy =
            local.ProgramCheckpointRestart.ProgramName;
          local.FcrSvesTitleIiPend.CreatedTimestamp = Now();
          local.FcrSvesTitleIiPend.LastUpdatedBy =
            local.ProgramCheckpointRestart.ProgramName;
          local.FcrSvesTitleIiPend.LastUpdatedTimestamp = Now();

          // ****************************************************************************************
          // ** Find out recent FPLS request for that person, this will 
          // populated form FPLS Request**
          // ** table, this is only for worker's 
          // information.
          // 
          // **
          // ****************************************************************************************
          UseOeB447SvesT2PenRespPrs();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";

            local.SvesAddressList.Index = 0;
            local.SvesAddressList.CheckSize();

            local.NeededToWrite.RptDetail = "Title-II Pending Update Error:" + local
              .FcrSvesGenInfo.MemberId + local
              .FcrSvesGenInfo.LocateSourceResponseAgencyCo;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            ++local.TotalT2PendSkipRecs.Count;

            continue;
          }

          break;
        case "E05":
          // ****************************************************************************************
          // ** Current SVES record is SVES Title-II record and use Title-II 
          // action block to       **
          // ** process the current SVES Response 
          // record.
          // 
          // **
          // ****************************************************************************************
          ++local.TotalSvesT2Records.Count;
          local.FcrSvesTitleIi.SeqNo = 1;
          local.FcrSvesTitleIi.CreatedBy =
            local.ProgramCheckpointRestart.ProgramName;
          local.FcrSvesTitleIi.CreatedTimestamp = Now();
          local.FcrSvesTitleIi.LastUpdatedBy =
            local.ProgramCheckpointRestart.ProgramName;
          local.FcrSvesTitleIi.LastUpdatedTimestamp = Now();
          UseOeB447SvesTitleiiRespPrs();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";

            local.SvesAddressList.Index = 0;
            local.SvesAddressList.CheckSize();

            local.NeededToWrite.RptDetail = "Title-II Update Error:" + local
              .FcrSvesGenInfo.MemberId + local
              .FcrSvesGenInfo.LocateSourceResponseAgencyCo;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            ++local.TotalT2SkipRecs.Count;

            continue;
          }
          else
          {
          }

          break;
        case "E06":
          // ****************************************************************************************
          // ** Current SVES record is SVES Title-XVI record and use Title-XVI 
          // action block to     **
          // ** process the current SVES Response 
          // record.
          // 
          // **
          // ****************************************************************************************
          ++local.TotalSvesT16Records.Count;
          local.FcrSvesTitleXvi.SeqNo = 1;
          local.FcrSvesTitleXvi.CreatedBy =
            local.ProgramCheckpointRestart.ProgramName;
          local.FcrSvesTitleXvi.CreatedTimestamp = Now();
          local.FcrSvesTitleXvi.LastUpdatedBy =
            local.ProgramCheckpointRestart.ProgramName;
          local.FcrSvesTitleXvi.LastUpdatedTimestamp = Now();
          UseOeB447SvesTitlexviRespPrs();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";

            local.SvesAddressList.Index = 0;
            local.SvesAddressList.CheckSize();

            local.NeededToWrite.RptDetail = "Title-XVI Update Error:" + local
              .FcrSvesGenInfo.MemberId + local
              .FcrSvesGenInfo.LocateSourceResponseAgencyCo;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            ++local.TotalT16SkipRecs.Count;

            continue;
          }
          else
          {
          }

          break;
        case "E07":
          // ****************************************************************************************
          // ** Current SVES record is SVES Prison response record and use 
          // prinson action block to **
          // ** process the current SVES Response 
          // record.
          // 
          // **
          // ****************************************************************************************
          ++local.TotalSvesPrisonRecords.Count;
          local.FcrSvesPrison.SeqNo = 1;
          local.FcrSvesPrison.CreatedBy =
            local.ProgramCheckpointRestart.ProgramName;
          local.FcrSvesPrison.CreatedTimestamp = Now();
          local.FcrSvesPrison.LastUpdatedBy =
            local.ProgramCheckpointRestart.ProgramName;
          local.FcrSvesPrison.LastUpdatedTimestamp = Now();
          UseOeB447SvesPrisonRecordPrs();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";

            local.SvesAddressList.Index = 0;
            local.SvesAddressList.CheckSize();

            local.NeededToWrite.RptDetail = "Prisoner Record Update Error:" + local
              .FcrSvesGenInfo.MemberId + local
              .FcrSvesGenInfo.LocateSourceResponseAgencyCo;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            ++local.TotalPrisonSkipRecs.Count;

            continue;
          }

          break;
        case "E10":
          // ****************************************************************************************
          // ** SVES NOT FOUND response, as per business requirement we need to 
          // generate a history **
          // ** record for the person found in the SVES response record.
          // **
          // ****************************************************************************************
          ++local.TotalSvesNotfoundRecord.Count;

          if (!IsEmpty(local.FcrSvesGenInfo.MultipleSsn))
          {
            local.Infrastructure.Detail = "SSN: " + (
              local.FcrSvesGenInfo.MultipleSsn ?? "") + NumberToString
              (DateToInt(local.FcrSvesGenInfo.SubmittedDateOfBirth), 15) + "Jail/Prison/Title II & Pending Claims/Title XVI." +
              "";
          }
          else
          {
            local.Infrastructure.Detail = "SSN: " + (
              local.FcrSvesGenInfo.Ssn ?? "") + NumberToString
              (DateToInt(local.FcrSvesGenInfo.SubmittedDateOfBirth), 15) + "Jail/Prison/Title II & Pending Claims/Title XVI." +
              "";
          }

          local.Infrastructure.ProcessStatus = "H";
          UseOeB447SvesAlertNIwoGen();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";

            local.SvesAddressList.Index = 0;
            local.SvesAddressList.CheckSize();

            local.NeededToWrite.RptDetail =
              "Not Found History Generation  Error:" + local
              .FcrSvesGenInfo.MemberId + local
              .FcrSvesGenInfo.LocateSourceResponseAgencyCo;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            ++local.TotalNfSkipRecs.Count;

            continue;
          }
          else
          {
            local.TotalNfAlertCreated.Count += local.TotAlertRecsCreated.Count;
            local.TotalNfAlertExists.Count += local.TotAlertExistsRecs.Count;
            local.TotalNfHistCreated.Count += local.TotHistRecsCreated.Count;
            local.TotalNfHistExists.Count += local.TotHistExistsRecs.Count;
          }

          break;
        default:
          // ****************************************************************************************
          // ** Invalid SVES agency response code, generate a error record and 
          // increment record    **
          // ** skip count.
          // 
          // **
          // ****************************************************************************************
          local.EabFileHandling.Action = "WRITE";

          local.SvesAddressList.Index = 0;
          local.SvesAddressList.CheckSize();

          local.NeededToWrite.RptDetail =
            "Invalid SVES Record Type(Agency Code):" + local
            .FcrSvesGenInfo.MemberId + local
            .FcrSvesGenInfo.LocateSourceResponseAgencyCo;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.TotalSvesSkipRecords.Count;

          continue;
      }

      // *************** Check to see if commit is needed ********************
      ++local.Commit.Count;

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.Commit.Count = 0;
        local.EabFileHandling.Action = "WRITE";
        local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-" + NumberToString
          (Now().Date.Month, 14, 2) + "-" + NumberToString
          (Now().Date.Day, 14, 2);
        local.Time.Text8 = NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
        local.NeededToWrite.RptDetail = "Commit taken after record number: " + NumberToString
          (local.TotalSvesInputRecords.Count, 15) + "  Date: " + local
          .Date.Text10 + "  Time: " + local.Time.Text8;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          break;
        }
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EOF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB447Close();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB447Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveFcrSvesGenInfo1(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.MemberId = source.MemberId;
    target.LocateSourceResponseAgencyCo = source.LocateSourceResponseAgencyCo;
  }

  private static void MoveFcrSvesGenInfo2(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.MemberId = source.MemberId;
    target.LocateSourceResponseAgencyCo = source.LocateSourceResponseAgencyCo;
    target.Ssn = source.Ssn;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private static void MoveSvesAddressList1(Local.SvesAddressListGroup source,
    OeB447SvesGeneralInfoPrs.Import.SvesAddressListGroup target)
  {
    target.FcrSvesAddress.Assign(source.FcrSvesAddress);
  }

  private static void MoveSvesAddressList2(Local.SvesAddressListGroup source,
    EabReadAllFcrSvesTypeRecs.Export.SvesAddressListGroup target)
  {
    target.FcrSvesAddress.Assign(source.FcrSvesAddress);
  }

  private static void MoveSvesAddressList3(EabReadAllFcrSvesTypeRecs.Export.
    SvesAddressListGroup source, Local.SvesAddressListGroup target)
  {
    target.FcrSvesAddress.Assign(source.FcrSvesAddress);
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseEabReadAllFcrSvesTypeRecs()
  {
    var useImport = new EabReadAllFcrSvesTypeRecs.Import();
    var useExport = new EabReadAllFcrSvesTypeRecs.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.FcrSvesGenInfo.Assign(local.FcrSvesGenInfo);
    local.SvesAddressList.
      CopyTo(useExport.SvesAddressList, MoveSvesAddressList2);
    useExport.FcrSvesTitleIiPend.Assign(local.FcrSvesTitleIiPend);
    useExport.FcrSvesTitleIi.Assign(local.FcrSvesTitleIi);
    useExport.FcrSvesTitleXvi.Assign(local.FcrSvesTitleXvi);
    useExport.FcrSvesPrison.Assign(local.FcrSvesPrison);

    Call(EabReadAllFcrSvesTypeRecs.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.FcrSvesGenInfo.Assign(useExport.FcrSvesGenInfo);
    useExport.SvesAddressList.
      CopyTo(local.SvesAddressList, MoveSvesAddressList3);
    local.FcrSvesTitleIiPend.Assign(useExport.FcrSvesTitleIiPend);
    local.FcrSvesTitleIi.Assign(useExport.FcrSvesTitleIi);
    local.FcrSvesTitleXvi.Assign(useExport.FcrSvesTitleXvi);
    local.FcrSvesPrison.Assign(useExport.FcrSvesPrison);
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ReturnCode.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ReturnCode.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeB447Close()
  {
    var useImport = new OeB447Close.Import();
    var useExport = new OeB447Close.Export();

    useImport.TotSvesInputRecords.Count = local.TotalSvesInputRecords.Count;
    useImport.TotalPersonNfRecords.Count = local.TotalPersonNfRecods.Count;
    useImport.TotalBadSsnRecords.Count = local.TotalBadSsnRecords.Count;
    useImport.TotSvesT2PendRecords.Count = local.TotalSvesT2PendRecords.Count;
    useImport.TotSvesT2Records.Count = local.TotalSvesT2Records.Count;
    useImport.TotSvesT16Records.Count = local.TotalSvesT16Records.Count;
    useImport.TotSvesPrisonRecords.Count = local.TotalSvesPrisonRecords.Count;
    useImport.TotSvesNotfoundRecords.Count =
      local.TotalSvesNotfoundRecord.Count;
    useImport.TotSvesSkipRecords.Count = local.TotalSvesSkipRecords.Count;
    useImport.TotT2PendSkipRecs.Count = local.TotalT2PendSkipRecs.Count;
    useImport.TotT2SkipRecs.Count = local.TotalT2SkipRecs.Count;
    useImport.TotT16SkipRecs.Count = local.TotalT16SkipRecs.Count;
    useImport.TotPrisonSkipRecs.Count = local.TotalPrisonSkipRecs.Count;
    useImport.TotNfSkipRecs.Count = local.TotalNfSkipRecs.Count;
    useImport.TotSvesGeninfoCreated.Count = local.TotalSvesGeninfoCreated.Count;
    useImport.TotSvesGeninfoUpdated.Count = local.TotalSvesGeninfoUpdated.Count;
    useImport.TotResaddrCreated.Count = local.TotalResaddrCreated.Count;
    useImport.TotResaddrUpdated.Count = local.TotalResaddrUpdated.Count;
    useImport.TotPeraddrCreated.Count = local.TotalPeraddrCreated.Count;
    useImport.TotPeraddrUpdated.Count = local.TotalPeraddrUpdated.Count;
    useImport.TotDisaddrCreated.Count = local.TotalDisaddrCreated.Count;
    useImport.TotDisaddrUpdated.Count = local.TotalDisaddrUpdated.Count;
    useImport.TotPayaddrCreated.Count = local.TotalPayaddrCreated.Count;
    useImport.TotPayaddrUpdated.Count = local.TotalPayaddrUpdated.Count;
    useImport.TotPriaddrCreated.Count = local.TotalPriaddrCreated.Count;
    useImport.TotPriaddrUpdated.Count = local.TotalPriaddrUpdated.Count;
    useImport.TotT2PendCreated.Count = local.TotalT2PendCreated.Count;
    useImport.TotT2PendUpdated.Count = local.TotalT2PendUpdated.Count;
    useImport.TotT2PendAlertCreated.Count = local.TotalT2PendAlertCreated.Count;
    useImport.TotT2PendHistCreated.Count = local.TotalT2PendHistCreated.Count;
    useImport.TotT2PendAlertExists.Count = local.TotalT2PendAlertExists.Count;
    useImport.TotT2PendHistExists.Count = local.TotalT2PendHistExists.Count;
    useImport.TotT2PendArletterCreat.Count =
      local.TotalT2PendArletterCrea.Count;
    useImport.TotT2PendIwoGenerated.Count = local.TotalT2PendIwoGenerated.Count;
    useImport.TotT2Created.Count = local.TotalT2Created.Count;
    useImport.TotT2Updated.Count = local.TotalT2Updated.Count;
    useImport.TotT2AlertCreated.Count = local.TotalT2AlertCreated.Count;
    useImport.TotT2AlertExists.Count = local.TotalT2AlertExists.Count;
    useImport.TotT2HistCreated.Count = local.TotalT2HistCreated.Count;
    useImport.TotT2HistExists.Count = local.TotalT2HistExists.Count;
    useImport.TotT16Created.Count = local.TotalT16Created.Count;
    useImport.TotT16Updated.Count = local.TotalT16Updated.Count;
    useImport.TotT16AlertCreated.Count = local.TotalT16AlertCreated.Count;
    useImport.TotT16AlertExists.Count = local.TotalT16AlertExists.Count;
    useImport.TotT16HistCreated.Count = local.TotalT16HistCreated.Count;
    useImport.TotT16HistExists.Count = local.TotalT16HistExists.Count;
    useImport.TotPrisonCreated.Count = local.TotalPrisonCreated.Count;
    useImport.TotPrisonUpdated.Count = local.TotalPrisonUpdated.Count;
    useImport.TotPrisonAlertCreated.Count = local.TotalPrisonAlertCreated.Count;
    useImport.TotPrisonAlertExists.Count = local.TotalPrisonAlertExists.Count;
    useImport.TotPrisonHistCreated.Count = local.TotalPrisonHistCreated.Count;
    useImport.TotPrisonHistExists.Count = local.TotalPrisonHistExists.Count;
    useImport.TotNfHistCreated.Count = local.TotalNfHistCreated.Count;
    useImport.TotNfHistExists.Count = local.TotalNfHistExists.Count;

    Call(OeB447Close.Execute, useImport, useExport);
  }

  private void UseOeB447Housekeeping()
  {
    var useImport = new OeB447Housekeeping.Import();
    var useExport = new OeB447Housekeeping.Export();

    Call(OeB447Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    local.AutomaticGenerateIwo.Flag = useExport.AutomaticGenerateIwo.Flag;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.IwoGenerationSkipFl.Flag = useExport.IwoGenerationSkipFl.Flag;
    local.AlertGenerationSkipFl.Flag = useExport.AlertGenerationSkipFl.Flag;
  }

  private void UseOeB447SvesAlertNIwoGen()
  {
    var useImport = new OeB447SvesAlertNIwoGen.Import();
    var useExport = new OeB447SvesAlertNIwoGen.Export();

    useImport.IwoGenerationSkipFl.Flag = local.IwoGenerationSkipFl.Flag;
    useImport.AlertGenerationSkipFl.Flag = local.AlertGenerationSkipFl.Flag;
    useImport.Max.Date = local.Max.Date;
    MoveFcrSvesGenInfo2(local.FcrSvesGenInfo, useImport.FcrSvesGenInfo);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.ProcessingDate.Date = local.Process.Date;

    Call(OeB447SvesAlertNIwoGen.Execute, useImport, useExport);

    local.TotAlertRecsCreated.Count = useExport.TotAlertRecsCreated.Count;
    local.TotHistRecsCreated.Count = useExport.TotHistRecsCreated.Count;
    local.TotAlertExistsRecs.Count = useExport.TotAlertExistsRecs.Count;
    local.TotHistExistsRecs.Count = useExport.TotHistExistsRecs.Count;
  }

  private void UseOeB447SvesGeneralInfoPrs()
  {
    var useImport = new OeB447SvesGeneralInfoPrs.Import();
    var useExport = new OeB447SvesGeneralInfoPrs.Export();

    useImport.FcrSvesGenInfo.Assign(local.FcrSvesGenInfo);
    local.SvesAddressList.
      CopyTo(useImport.SvesAddressList, MoveSvesAddressList1);
    useImport.TotSvesGeninfoCreated.Count = local.TotalSvesGeninfoCreated.Count;
    useImport.TotSvesGeninfoUpdated.Count = local.TotalSvesGeninfoUpdated.Count;
    useImport.TotResaddrCreated.Count = local.TotalResaddrCreated.Count;
    useImport.TotResaddrUpdated.Count = local.TotalResaddrUpdated.Count;
    useImport.TotPeraddrCreated.Count = local.TotalPeraddrCreated.Count;
    useImport.TotPeraddrUpdated.Count = local.TotalPeraddrUpdated.Count;
    useImport.TotDisaddrCreated.Count = local.TotalDisaddrCreated.Count;
    useImport.TotDisaddrUpdated.Count = local.TotalDisaddrUpdated.Count;
    useImport.TotPayaddrCreated.Count = local.TotalPayaddrCreated.Count;
    useImport.TotPayaddrUpdated.Count = local.TotalPayaddrUpdated.Count;
    useImport.TotPriaddrCreated.Count = local.TotalPriaddrCreated.Count;
    useImport.TotPriaddrUpdated.Count = local.TotalPriaddrUpdated.Count;

    Call(OeB447SvesGeneralInfoPrs.Execute, useImport, useExport);

    local.TotalSvesGeninfoCreated.Count = useImport.TotSvesGeninfoCreated.Count;
    local.TotalSvesGeninfoUpdated.Count = useImport.TotSvesGeninfoUpdated.Count;
    local.TotalResaddrCreated.Count = useImport.TotResaddrCreated.Count;
    local.TotalResaddrUpdated.Count = useImport.TotResaddrUpdated.Count;
    local.TotalPeraddrCreated.Count = useImport.TotPeraddrCreated.Count;
    local.TotalPeraddrUpdated.Count = useImport.TotPeraddrUpdated.Count;
    local.TotalDisaddrCreated.Count = useImport.TotDisaddrCreated.Count;
    local.TotalDisaddrUpdated.Count = useImport.TotDisaddrUpdated.Count;
    local.TotalPayaddrCreated.Count = useImport.TotPayaddrCreated.Count;
    local.TotalPayaddrUpdated.Count = useImport.TotPayaddrUpdated.Count;
    local.TotalPriaddrCreated.Count = useImport.TotPriaddrCreated.Count;
    local.TotalPriaddrUpdated.Count = useImport.TotPriaddrUpdated.Count;
  }

  private void UseOeB447SvesPrisonRecordPrs()
  {
    var useImport = new OeB447SvesPrisonRecordPrs.Import();
    var useExport = new OeB447SvesPrisonRecordPrs.Export();

    useImport.AlertGenerationSkipFl.Flag = local.AlertGenerationSkipFl.Flag;
    useImport.IwoGenerationSkipFl.Flag = local.IwoGenerationSkipFl.Flag;
    useImport.MaxDate.Date = local.Max.Date;
    MoveFcrSvesGenInfo1(local.FcrSvesGenInfo, useImport.FcrSvesGenInfo);
    useImport.FcrSvesPrison.Assign(local.FcrSvesPrison);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.TotPrisonCreated.Count = local.TotalPrisonCreated.Count;
    useImport.TotPrisonUpdated.Count = local.TotalPrisonUpdated.Count;
    useImport.TotPrisonAlertCreated.Count = local.TotalPrisonAlertCreated.Count;
    useImport.TotPrisonAlertExists.Count = local.TotalPrisonAlertExists.Count;
    useImport.TotPrisonHistCreated.Count = local.TotalPrisonHistCreated.Count;
    useImport.TotPrisonHistExists.Count = local.TotalPrisonHistExists.Count;

    Call(OeB447SvesPrisonRecordPrs.Execute, useImport, useExport);

    local.TotalPrisonCreated.Count = useImport.TotPrisonCreated.Count;
    local.TotalPrisonUpdated.Count = useImport.TotPrisonUpdated.Count;
    local.TotalPrisonAlertCreated.Count = useImport.TotPrisonAlertCreated.Count;
    local.TotalPrisonAlertExists.Count = useImport.TotPrisonAlertExists.Count;
    local.TotalPrisonHistCreated.Count = useImport.TotPrisonHistCreated.Count;
    local.TotalPrisonHistExists.Count = useImport.TotPrisonHistExists.Count;
  }

  private void UseOeB447SvesT2PenRespPrs()
  {
    var useImport = new OeB447SvesT2PenRespPrs.Import();
    var useExport = new OeB447SvesT2PenRespPrs.Export();

    useImport.AlertGenerationSkipFl.Flag = local.AlertGenerationSkipFl.Flag;
    useImport.IwoGenerationSkipFl.Flag = local.IwoGenerationSkipFl.Flag;
    useImport.MaxDate.Date = local.Max.Date;
    MoveFcrSvesGenInfo1(local.FcrSvesGenInfo, useImport.FcrSvesGenInfo);
    useImport.FcrSvesTitleIiPend.Assign(local.FcrSvesTitleIiPend);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.TotT2PendAlertExists.Count = local.TotalT2PendAlertExists.Count;
    useImport.TotT2PendHistExists.Count = local.TotalT2PendHistExists.Count;
    useImport.TotT2PendArlettCreated.Count =
      local.TotalT2PendArletterCrea.Count;
    useImport.TotT2PendIwoGenerated.Count = local.TotalT2PendIwoGenerated.Count;
    useImport.TotT2PendAlertCreated.Count = local.TotalT2PendAlertCreated.Count;
    useImport.TotT2PendHistCreated.Count = local.TotalT2PendHistCreated.Count;
    useImport.TotT2PendUpdated.Count = local.TotalT2PendUpdated.Count;
    useImport.TotT2PendCreated.Count = local.TotalT2PendCreated.Count;

    Call(OeB447SvesT2PenRespPrs.Execute, useImport, useExport);

    local.TotalT2PendAlertExists.Count = useImport.TotT2PendAlertExists.Count;
    local.TotalT2PendHistExists.Count = useImport.TotT2PendHistExists.Count;
    local.TotalT2PendArletterCrea.Count =
      useImport.TotT2PendArlettCreated.Count;
    local.TotalT2PendIwoGenerated.Count = useImport.TotT2PendIwoGenerated.Count;
    local.TotalT2PendAlertCreated.Count = useImport.TotT2PendAlertCreated.Count;
    local.TotalT2PendHistCreated.Count = useImport.TotT2PendHistCreated.Count;
    local.TotalT2PendUpdated.Count = useImport.TotT2PendUpdated.Count;
    local.TotalT2PendCreated.Count = useImport.TotT2PendCreated.Count;
  }

  private void UseOeB447SvesTitleiiRespPrs()
  {
    var useImport = new OeB447SvesTitleiiRespPrs.Import();
    var useExport = new OeB447SvesTitleiiRespPrs.Export();

    useImport.AlertGenerationSkipFl.Flag = local.AlertGenerationSkipFl.Flag;
    useImport.IwoGenerationSkipFl.Flag = local.IwoGenerationSkipFl.Flag;
    useImport.MaxDate.Date = local.Max.Date;
    MoveFcrSvesGenInfo1(local.FcrSvesGenInfo, useImport.FcrSvesGenInfo);
    useImport.FcrSvesTitleIi.Assign(local.FcrSvesTitleIi);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.TotT2Created.Count = local.TotalT2Created.Count;
    useImport.TotT2Updated.Count = local.TotalT2Updated.Count;
    useImport.TotT2AlertCreated.Count = local.TotalT2AlertCreated.Count;
    useImport.TotT2AlertExists.Count = local.TotalT2AlertExists.Count;
    useImport.TotT2HistCreated.Count = local.TotalT2HistCreated.Count;
    useImport.TotT2HistExists.Count = local.TotalT2HistExists.Count;

    Call(OeB447SvesTitleiiRespPrs.Execute, useImport, useExport);

    local.TotalT2Created.Count = useImport.TotT2Created.Count;
    local.TotalT2Updated.Count = useImport.TotT2Updated.Count;
    local.TotalT2AlertCreated.Count = useImport.TotT2AlertCreated.Count;
    local.TotalT2AlertExists.Count = useImport.TotT2AlertExists.Count;
    local.TotalT2HistCreated.Count = useImport.TotT2HistCreated.Count;
    local.TotalT2HistExists.Count = useImport.TotT2HistExists.Count;
  }

  private void UseOeB447SvesTitlexviRespPrs()
  {
    var useImport = new OeB447SvesTitlexviRespPrs.Import();
    var useExport = new OeB447SvesTitlexviRespPrs.Export();

    useImport.AlertGenerationSkipFl.Flag = local.AlertGenerationSkipFl.Flag;
    useImport.IwoGenerationSkipFl.Flag = local.IwoGenerationSkipFl.Flag;
    useImport.MaxDate.Date = local.Max.Date;
    MoveFcrSvesGenInfo1(local.FcrSvesGenInfo, useImport.FcrSvesGenInfo);
    useImport.FcrSvesTitleXvi.Assign(local.FcrSvesTitleXvi);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.TotT16Created.Count = local.TotalT16Created.Count;
    useImport.TotT16Updated.Count = local.TotalT16Updated.Count;
    useImport.TotT16AlertCreated.Count = local.TotalT16AlertCreated.Count;
    useImport.TotT16AlertExists.Count = local.TotalT16AlertExists.Count;
    useImport.TotT16HistCreated.Count = local.TotalT16HistCreated.Count;
    useImport.TotT16HistExists.Count = local.TotalT16HistExists.Count;

    Call(OeB447SvesTitlexviRespPrs.Execute, useImport, useExport);

    local.TotalT16Created.Count = useImport.TotT16Created.Count;
    local.TotalT16Updated.Count = useImport.TotT16Updated.Count;
    local.TotalT16AlertCreated.Count = useImport.TotT16AlertCreated.Count;
    local.TotalT16AlertExists.Count = useImport.TotT16AlertExists.Count;
    local.TotalT16HistCreated.Count = useImport.TotT16HistCreated.Count;
    local.TotalT16HistExists.Count = useImport.TotT16HistExists.Count;
  }

  private void UseSiCabReadAdabasBatch()
  {
    var useImport = new SiCabReadAdabasBatch.Import();
    var useExport = new SiCabReadAdabasBatch.Export();

    useImport.Obligor.Number = local.Kaecses.Number;

    Call(SiCabReadAdabasBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.Obligor, local.Kaecses);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.AbendData.Assign(useExport.AbendData);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "ssn", local.ConvertSsn.SsnNum9);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
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
    /// <summary>A SvesAddressListGroup group.</summary>
    [Serializable]
    public class SvesAddressListGroup
    {
      /// <summary>
      /// A value of FcrSvesAddress.
      /// </summary>
      [JsonPropertyName("fcrSvesAddress")]
      public FcrSvesAddress FcrSvesAddress
      {
        get => fcrSvesAddress ??= new();
        set => fcrSvesAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private FcrSvesAddress fcrSvesAddress;
    }

    /// <summary>
    /// A value of TotHistExistsRecs.
    /// </summary>
    [JsonPropertyName("totHistExistsRecs")]
    public Common TotHistExistsRecs
    {
      get => totHistExistsRecs ??= new();
      set => totHistExistsRecs = value;
    }

    /// <summary>
    /// A value of TotAlertExistsRecs.
    /// </summary>
    [JsonPropertyName("totAlertExistsRecs")]
    public Common TotAlertExistsRecs
    {
      get => totAlertExistsRecs ??= new();
      set => totAlertExistsRecs = value;
    }

    /// <summary>
    /// A value of TotHistRecsCreated.
    /// </summary>
    [JsonPropertyName("totHistRecsCreated")]
    public Common TotHistRecsCreated
    {
      get => totHistRecsCreated ??= new();
      set => totHistRecsCreated = value;
    }

    /// <summary>
    /// A value of TotAlertRecsCreated.
    /// </summary>
    [JsonPropertyName("totAlertRecsCreated")]
    public Common TotAlertRecsCreated
    {
      get => totAlertRecsCreated ??= new();
      set => totAlertRecsCreated = value;
    }

    /// <summary>
    /// A value of AlertGenerationSkipFl.
    /// </summary>
    [JsonPropertyName("alertGenerationSkipFl")]
    public Common AlertGenerationSkipFl
    {
      get => alertGenerationSkipFl ??= new();
      set => alertGenerationSkipFl = value;
    }

    /// <summary>
    /// A value of IwoGenerationSkipFl.
    /// </summary>
    [JsonPropertyName("iwoGenerationSkipFl")]
    public Common IwoGenerationSkipFl
    {
      get => iwoGenerationSkipFl ??= new();
      set => iwoGenerationSkipFl = value;
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
    /// A value of FcrSvesPrison.
    /// </summary>
    [JsonPropertyName("fcrSvesPrison")]
    public FcrSvesPrison FcrSvesPrison
    {
      get => fcrSvesPrison ??= new();
      set => fcrSvesPrison = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleXvi.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleXvi")]
    public FcrSvesTitleXvi FcrSvesTitleXvi
    {
      get => fcrSvesTitleXvi ??= new();
      set => fcrSvesTitleXvi = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleIi.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIi")]
    public FcrSvesTitleIi FcrSvesTitleIi
    {
      get => fcrSvesTitleIi ??= new();
      set => fcrSvesTitleIi = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleIiPend.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIiPend")]
    public FcrSvesTitleIiPend FcrSvesTitleIiPend
    {
      get => fcrSvesTitleIiPend ??= new();
      set => fcrSvesTitleIiPend = value;
    }

    /// <summary>
    /// Gets a value of SvesAddressList.
    /// </summary>
    [JsonIgnore]
    public Array<SvesAddressListGroup> SvesAddressList =>
      svesAddressList ??= new(SvesAddressListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SvesAddressList for json serialization.
    /// </summary>
    [JsonPropertyName("svesAddressList")]
    [Computed]
    public IList<SvesAddressListGroup> SvesAddressList_Json
    {
      get => svesAddressList;
      set => SvesAddressList.Assign(value);
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of AutomaticGenerateIwo.
    /// </summary>
    [JsonPropertyName("automaticGenerateIwo")]
    public Common AutomaticGenerateIwo
    {
      get => automaticGenerateIwo ??= new();
      set => automaticGenerateIwo = value;
    }

    /// <summary>
    /// A value of NullFcrSvesTitleIiPendingClaim.
    /// </summary>
    [JsonPropertyName("nullFcrSvesTitleIiPendingClaim")]
    public FcrSvesTitleIiPendingClaim NullFcrSvesTitleIiPendingClaim
    {
      get => nullFcrSvesTitleIiPendingClaim ??= new();
      set => nullFcrSvesTitleIiPendingClaim = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleIiPendingClaim.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIiPendingClaim")]
    public FcrSvesTitleIiPendingClaim FcrSvesTitleIiPendingClaim
    {
      get => fcrSvesTitleIiPendingClaim ??= new();
      set => fcrSvesTitleIiPendingClaim = value;
    }

    /// <summary>
    /// A value of ConvertSsn.
    /// </summary>
    [JsonPropertyName("convertSsn")]
    public SsnWorkArea ConvertSsn
    {
      get => convertSsn ??= new();
      set => convertSsn = value;
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
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
    /// A value of DatePlus3Years.
    /// </summary>
    [JsonPropertyName("datePlus3Years")]
    public DateWorkArea DatePlus3Years
    {
      get => datePlus3Years ??= new();
      set => datePlus3Years = value;
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
    /// A value of Kaecses.
    /// </summary>
    [JsonPropertyName("kaecses")]
    public CsePersonsWorkSet Kaecses
    {
      get => kaecses ??= new();
      set => kaecses = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    /// <summary>
    /// A value of NarrativeDetailLineCnt.
    /// </summary>
    [JsonPropertyName("narrativeDetailLineCnt")]
    public Common NarrativeDetailLineCnt
    {
      get => narrativeDetailLineCnt ??= new();
      set => narrativeDetailLineCnt = value;
    }

    /// <summary>
    /// A value of ConvertMessage.
    /// </summary>
    [JsonPropertyName("convertMessage")]
    public SsnWorkArea ConvertMessage
    {
      get => convertMessage ??= new();
      set => convertMessage = value;
    }

    /// <summary>
    /// A value of Message2.
    /// </summary>
    [JsonPropertyName("message2")]
    public WorkArea Message2
    {
      get => message2 ??= new();
      set => message2 = value;
    }

    /// <summary>
    /// A value of Message1.
    /// </summary>
    [JsonPropertyName("message1")]
    public WorkArea Message1
    {
      get => message1 ??= new();
      set => message1 = value;
    }

    /// <summary>
    /// A value of TotalSvesInputRecords.
    /// </summary>
    [JsonPropertyName("totalSvesInputRecords")]
    public Common TotalSvesInputRecords
    {
      get => totalSvesInputRecords ??= new();
      set => totalSvesInputRecords = value;
    }

    /// <summary>
    /// A value of TotalSvesSkipRecords.
    /// </summary>
    [JsonPropertyName("totalSvesSkipRecords")]
    public Common TotalSvesSkipRecords
    {
      get => totalSvesSkipRecords ??= new();
      set => totalSvesSkipRecords = value;
    }

    /// <summary>
    /// A value of TotalSvesT2PendRecords.
    /// </summary>
    [JsonPropertyName("totalSvesT2PendRecords")]
    public Common TotalSvesT2PendRecords
    {
      get => totalSvesT2PendRecords ??= new();
      set => totalSvesT2PendRecords = value;
    }

    /// <summary>
    /// A value of TotalSvesT2Records.
    /// </summary>
    [JsonPropertyName("totalSvesT2Records")]
    public Common TotalSvesT2Records
    {
      get => totalSvesT2Records ??= new();
      set => totalSvesT2Records = value;
    }

    /// <summary>
    /// A value of TotalSvesT16Records.
    /// </summary>
    [JsonPropertyName("totalSvesT16Records")]
    public Common TotalSvesT16Records
    {
      get => totalSvesT16Records ??= new();
      set => totalSvesT16Records = value;
    }

    /// <summary>
    /// A value of TotalSvesPrisonRecords.
    /// </summary>
    [JsonPropertyName("totalSvesPrisonRecords")]
    public Common TotalSvesPrisonRecords
    {
      get => totalSvesPrisonRecords ??= new();
      set => totalSvesPrisonRecords = value;
    }

    /// <summary>
    /// A value of TotalSvesNotfoundRecord.
    /// </summary>
    [JsonPropertyName("totalSvesNotfoundRecord")]
    public Common TotalSvesNotfoundRecord
    {
      get => totalSvesNotfoundRecord ??= new();
      set => totalSvesNotfoundRecord = value;
    }

    /// <summary>
    /// A value of TotalBadSsnRecords.
    /// </summary>
    [JsonPropertyName("totalBadSsnRecords")]
    public Common TotalBadSsnRecords
    {
      get => totalBadSsnRecords ??= new();
      set => totalBadSsnRecords = value;
    }

    /// <summary>
    /// A value of TotalPersonNfRecods.
    /// </summary>
    [JsonPropertyName("totalPersonNfRecods")]
    public Common TotalPersonNfRecods
    {
      get => totalPersonNfRecods ??= new();
      set => totalPersonNfRecods = value;
    }

    /// <summary>
    /// A value of TotalSvesGeninfoCreated.
    /// </summary>
    [JsonPropertyName("totalSvesGeninfoCreated")]
    public Common TotalSvesGeninfoCreated
    {
      get => totalSvesGeninfoCreated ??= new();
      set => totalSvesGeninfoCreated = value;
    }

    /// <summary>
    /// A value of TotalSvesGeninfoUpdated.
    /// </summary>
    [JsonPropertyName("totalSvesGeninfoUpdated")]
    public Common TotalSvesGeninfoUpdated
    {
      get => totalSvesGeninfoUpdated ??= new();
      set => totalSvesGeninfoUpdated = value;
    }

    /// <summary>
    /// A value of TotalT2AlertCreated.
    /// </summary>
    [JsonPropertyName("totalT2AlertCreated")]
    public Common TotalT2AlertCreated
    {
      get => totalT2AlertCreated ??= new();
      set => totalT2AlertCreated = value;
    }

    /// <summary>
    /// A value of TotalT2AlertExists.
    /// </summary>
    [JsonPropertyName("totalT2AlertExists")]
    public Common TotalT2AlertExists
    {
      get => totalT2AlertExists ??= new();
      set => totalT2AlertExists = value;
    }

    /// <summary>
    /// A value of TotalT2HistCreated.
    /// </summary>
    [JsonPropertyName("totalT2HistCreated")]
    public Common TotalT2HistCreated
    {
      get => totalT2HistCreated ??= new();
      set => totalT2HistCreated = value;
    }

    /// <summary>
    /// A value of TotalT2HistExists.
    /// </summary>
    [JsonPropertyName("totalT2HistExists")]
    public Common TotalT2HistExists
    {
      get => totalT2HistExists ??= new();
      set => totalT2HistExists = value;
    }

    /// <summary>
    /// A value of TotalT16Created.
    /// </summary>
    [JsonPropertyName("totalT16Created")]
    public Common TotalT16Created
    {
      get => totalT16Created ??= new();
      set => totalT16Created = value;
    }

    /// <summary>
    /// A value of TotalT16Updated.
    /// </summary>
    [JsonPropertyName("totalT16Updated")]
    public Common TotalT16Updated
    {
      get => totalT16Updated ??= new();
      set => totalT16Updated = value;
    }

    /// <summary>
    /// A value of TotalT16AlertCreated.
    /// </summary>
    [JsonPropertyName("totalT16AlertCreated")]
    public Common TotalT16AlertCreated
    {
      get => totalT16AlertCreated ??= new();
      set => totalT16AlertCreated = value;
    }

    /// <summary>
    /// A value of TotalT16AlertExists.
    /// </summary>
    [JsonPropertyName("totalT16AlertExists")]
    public Common TotalT16AlertExists
    {
      get => totalT16AlertExists ??= new();
      set => totalT16AlertExists = value;
    }

    /// <summary>
    /// A value of TotalT16HistCreated.
    /// </summary>
    [JsonPropertyName("totalT16HistCreated")]
    public Common TotalT16HistCreated
    {
      get => totalT16HistCreated ??= new();
      set => totalT16HistCreated = value;
    }

    /// <summary>
    /// A value of TotalT16HistExists.
    /// </summary>
    [JsonPropertyName("totalT16HistExists")]
    public Common TotalT16HistExists
    {
      get => totalT16HistExists ??= new();
      set => totalT16HistExists = value;
    }

    /// <summary>
    /// A value of TotalPrisonCreated.
    /// </summary>
    [JsonPropertyName("totalPrisonCreated")]
    public Common TotalPrisonCreated
    {
      get => totalPrisonCreated ??= new();
      set => totalPrisonCreated = value;
    }

    /// <summary>
    /// A value of TotalPrisonUpdated.
    /// </summary>
    [JsonPropertyName("totalPrisonUpdated")]
    public Common TotalPrisonUpdated
    {
      get => totalPrisonUpdated ??= new();
      set => totalPrisonUpdated = value;
    }

    /// <summary>
    /// A value of TotalPrisonAlertCreated.
    /// </summary>
    [JsonPropertyName("totalPrisonAlertCreated")]
    public Common TotalPrisonAlertCreated
    {
      get => totalPrisonAlertCreated ??= new();
      set => totalPrisonAlertCreated = value;
    }

    /// <summary>
    /// A value of TotalPrisonAlertExists.
    /// </summary>
    [JsonPropertyName("totalPrisonAlertExists")]
    public Common TotalPrisonAlertExists
    {
      get => totalPrisonAlertExists ??= new();
      set => totalPrisonAlertExists = value;
    }

    /// <summary>
    /// A value of TotalPrisonHistCreated.
    /// </summary>
    [JsonPropertyName("totalPrisonHistCreated")]
    public Common TotalPrisonHistCreated
    {
      get => totalPrisonHistCreated ??= new();
      set => totalPrisonHistCreated = value;
    }

    /// <summary>
    /// A value of TotalPrisonHistExists.
    /// </summary>
    [JsonPropertyName("totalPrisonHistExists")]
    public Common TotalPrisonHistExists
    {
      get => totalPrisonHistExists ??= new();
      set => totalPrisonHistExists = value;
    }

    /// <summary>
    /// A value of TotalNfCreated.
    /// </summary>
    [JsonPropertyName("totalNfCreated")]
    public Common TotalNfCreated
    {
      get => totalNfCreated ??= new();
      set => totalNfCreated = value;
    }

    /// <summary>
    /// A value of TotalNfUpdated.
    /// </summary>
    [JsonPropertyName("totalNfUpdated")]
    public Common TotalNfUpdated
    {
      get => totalNfUpdated ??= new();
      set => totalNfUpdated = value;
    }

    /// <summary>
    /// A value of TotalNfAlertCreated.
    /// </summary>
    [JsonPropertyName("totalNfAlertCreated")]
    public Common TotalNfAlertCreated
    {
      get => totalNfAlertCreated ??= new();
      set => totalNfAlertCreated = value;
    }

    /// <summary>
    /// A value of TotalNfAlertExists.
    /// </summary>
    [JsonPropertyName("totalNfAlertExists")]
    public Common TotalNfAlertExists
    {
      get => totalNfAlertExists ??= new();
      set => totalNfAlertExists = value;
    }

    /// <summary>
    /// A value of TotalNfHistCreated.
    /// </summary>
    [JsonPropertyName("totalNfHistCreated")]
    public Common TotalNfHistCreated
    {
      get => totalNfHistCreated ??= new();
      set => totalNfHistCreated = value;
    }

    /// <summary>
    /// A value of TotalNfHistExists.
    /// </summary>
    [JsonPropertyName("totalNfHistExists")]
    public Common TotalNfHistExists
    {
      get => totalNfHistExists ??= new();
      set => totalNfHistExists = value;
    }

    /// <summary>
    /// A value of TotalT2PendCreated.
    /// </summary>
    [JsonPropertyName("totalT2PendCreated")]
    public Common TotalT2PendCreated
    {
      get => totalT2PendCreated ??= new();
      set => totalT2PendCreated = value;
    }

    /// <summary>
    /// A value of TotalT2PendUpdated.
    /// </summary>
    [JsonPropertyName("totalT2PendUpdated")]
    public Common TotalT2PendUpdated
    {
      get => totalT2PendUpdated ??= new();
      set => totalT2PendUpdated = value;
    }

    /// <summary>
    /// A value of TotalT2PendAlertCreated.
    /// </summary>
    [JsonPropertyName("totalT2PendAlertCreated")]
    public Common TotalT2PendAlertCreated
    {
      get => totalT2PendAlertCreated ??= new();
      set => totalT2PendAlertCreated = value;
    }

    /// <summary>
    /// A value of TotalT2PendHistCreated.
    /// </summary>
    [JsonPropertyName("totalT2PendHistCreated")]
    public Common TotalT2PendHistCreated
    {
      get => totalT2PendHistCreated ??= new();
      set => totalT2PendHistCreated = value;
    }

    /// <summary>
    /// A value of TotalT2PendAlertExists.
    /// </summary>
    [JsonPropertyName("totalT2PendAlertExists")]
    public Common TotalT2PendAlertExists
    {
      get => totalT2PendAlertExists ??= new();
      set => totalT2PendAlertExists = value;
    }

    /// <summary>
    /// A value of TotalT2PendHistExists.
    /// </summary>
    [JsonPropertyName("totalT2PendHistExists")]
    public Common TotalT2PendHistExists
    {
      get => totalT2PendHistExists ??= new();
      set => totalT2PendHistExists = value;
    }

    /// <summary>
    /// A value of TotalT2PendArletterCrea.
    /// </summary>
    [JsonPropertyName("totalT2PendArletterCrea")]
    public Common TotalT2PendArletterCrea
    {
      get => totalT2PendArletterCrea ??= new();
      set => totalT2PendArletterCrea = value;
    }

    /// <summary>
    /// A value of TotalT2PendIwoGenerated.
    /// </summary>
    [JsonPropertyName("totalT2PendIwoGenerated")]
    public Common TotalT2PendIwoGenerated
    {
      get => totalT2PendIwoGenerated ??= new();
      set => totalT2PendIwoGenerated = value;
    }

    /// <summary>
    /// A value of TotalT2Created.
    /// </summary>
    [JsonPropertyName("totalT2Created")]
    public Common TotalT2Created
    {
      get => totalT2Created ??= new();
      set => totalT2Created = value;
    }

    /// <summary>
    /// A value of TotalT2Updated.
    /// </summary>
    [JsonPropertyName("totalT2Updated")]
    public Common TotalT2Updated
    {
      get => totalT2Updated ??= new();
      set => totalT2Updated = value;
    }

    /// <summary>
    /// A value of TotalResaddrCreated.
    /// </summary>
    [JsonPropertyName("totalResaddrCreated")]
    public Common TotalResaddrCreated
    {
      get => totalResaddrCreated ??= new();
      set => totalResaddrCreated = value;
    }

    /// <summary>
    /// A value of TotalResaddrUpdated.
    /// </summary>
    [JsonPropertyName("totalResaddrUpdated")]
    public Common TotalResaddrUpdated
    {
      get => totalResaddrUpdated ??= new();
      set => totalResaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotalPeraddrCreated.
    /// </summary>
    [JsonPropertyName("totalPeraddrCreated")]
    public Common TotalPeraddrCreated
    {
      get => totalPeraddrCreated ??= new();
      set => totalPeraddrCreated = value;
    }

    /// <summary>
    /// A value of TotalPeraddrUpdated.
    /// </summary>
    [JsonPropertyName("totalPeraddrUpdated")]
    public Common TotalPeraddrUpdated
    {
      get => totalPeraddrUpdated ??= new();
      set => totalPeraddrUpdated = value;
    }

    /// <summary>
    /// A value of TotalDisaddrCreated.
    /// </summary>
    [JsonPropertyName("totalDisaddrCreated")]
    public Common TotalDisaddrCreated
    {
      get => totalDisaddrCreated ??= new();
      set => totalDisaddrCreated = value;
    }

    /// <summary>
    /// A value of TotalDisaddrUpdated.
    /// </summary>
    [JsonPropertyName("totalDisaddrUpdated")]
    public Common TotalDisaddrUpdated
    {
      get => totalDisaddrUpdated ??= new();
      set => totalDisaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotalPayaddrCreated.
    /// </summary>
    [JsonPropertyName("totalPayaddrCreated")]
    public Common TotalPayaddrCreated
    {
      get => totalPayaddrCreated ??= new();
      set => totalPayaddrCreated = value;
    }

    /// <summary>
    /// A value of TotalPayaddrUpdated.
    /// </summary>
    [JsonPropertyName("totalPayaddrUpdated")]
    public Common TotalPayaddrUpdated
    {
      get => totalPayaddrUpdated ??= new();
      set => totalPayaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotalPriaddrCreated.
    /// </summary>
    [JsonPropertyName("totalPriaddrCreated")]
    public Common TotalPriaddrCreated
    {
      get => totalPriaddrCreated ??= new();
      set => totalPriaddrCreated = value;
    }

    /// <summary>
    /// A value of TotalPriaddrUpdated.
    /// </summary>
    [JsonPropertyName("totalPriaddrUpdated")]
    public Common TotalPriaddrUpdated
    {
      get => totalPriaddrUpdated ??= new();
      set => totalPriaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotalT2PendSkipRecs.
    /// </summary>
    [JsonPropertyName("totalT2PendSkipRecs")]
    public Common TotalT2PendSkipRecs
    {
      get => totalT2PendSkipRecs ??= new();
      set => totalT2PendSkipRecs = value;
    }

    /// <summary>
    /// A value of TotalT2SkipRecs.
    /// </summary>
    [JsonPropertyName("totalT2SkipRecs")]
    public Common TotalT2SkipRecs
    {
      get => totalT2SkipRecs ??= new();
      set => totalT2SkipRecs = value;
    }

    /// <summary>
    /// A value of TotalT16SkipRecs.
    /// </summary>
    [JsonPropertyName("totalT16SkipRecs")]
    public Common TotalT16SkipRecs
    {
      get => totalT16SkipRecs ??= new();
      set => totalT16SkipRecs = value;
    }

    /// <summary>
    /// A value of TotalPrisonSkipRecs.
    /// </summary>
    [JsonPropertyName("totalPrisonSkipRecs")]
    public Common TotalPrisonSkipRecs
    {
      get => totalPrisonSkipRecs ??= new();
      set => totalPrisonSkipRecs = value;
    }

    /// <summary>
    /// A value of TotalNfSkipRecs.
    /// </summary>
    [JsonPropertyName("totalNfSkipRecs")]
    public Common TotalNfSkipRecs
    {
      get => totalNfSkipRecs ??= new();
      set => totalNfSkipRecs = value;
    }

    private Common totHistExistsRecs;
    private Common totAlertExistsRecs;
    private Common totHistRecsCreated;
    private Common totAlertRecsCreated;
    private Common alertGenerationSkipFl;
    private Common iwoGenerationSkipFl;
    private CsePerson csePerson;
    private FcrSvesPrison fcrSvesPrison;
    private FcrSvesTitleXvi fcrSvesTitleXvi;
    private FcrSvesTitleIi fcrSvesTitleIi;
    private FcrSvesTitleIiPend fcrSvesTitleIiPend;
    private Array<SvesAddressListGroup> svesAddressList;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private Common automaticGenerateIwo;
    private FcrSvesTitleIiPendingClaim nullFcrSvesTitleIiPendingClaim;
    private FcrSvesTitleIiPendingClaim fcrSvesTitleIiPendingClaim;
    private SsnWorkArea convertSsn;
    private TextWorkArea time;
    private TextWorkArea date;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private DateWorkArea max;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea datePlus3Years;
    private EabReportSend neededToWrite;
    private CsePersonsWorkSet kaecses;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private Infrastructure infrastructure;
    private ProgramProcessingInfo programProcessingInfo;
    private NarrativeDetail narrativeDetail;
    private Common narrativeDetailLineCnt;
    private SsnWorkArea convertMessage;
    private WorkArea message2;
    private WorkArea message1;
    private Common totalSvesInputRecords;
    private Common totalSvesSkipRecords;
    private Common totalSvesT2PendRecords;
    private Common totalSvesT2Records;
    private Common totalSvesT16Records;
    private Common totalSvesPrisonRecords;
    private Common totalSvesNotfoundRecord;
    private Common totalBadSsnRecords;
    private Common totalPersonNfRecods;
    private Common totalSvesGeninfoCreated;
    private Common totalSvesGeninfoUpdated;
    private Common totalT2AlertCreated;
    private Common totalT2AlertExists;
    private Common totalT2HistCreated;
    private Common totalT2HistExists;
    private Common totalT16Created;
    private Common totalT16Updated;
    private Common totalT16AlertCreated;
    private Common totalT16AlertExists;
    private Common totalT16HistCreated;
    private Common totalT16HistExists;
    private Common totalPrisonCreated;
    private Common totalPrisonUpdated;
    private Common totalPrisonAlertCreated;
    private Common totalPrisonAlertExists;
    private Common totalPrisonHistCreated;
    private Common totalPrisonHistExists;
    private Common totalNfCreated;
    private Common totalNfUpdated;
    private Common totalNfAlertCreated;
    private Common totalNfAlertExists;
    private Common totalNfHistCreated;
    private Common totalNfHistExists;
    private Common totalT2PendCreated;
    private Common totalT2PendUpdated;
    private Common totalT2PendAlertCreated;
    private Common totalT2PendHistCreated;
    private Common totalT2PendAlertExists;
    private Common totalT2PendHistExists;
    private Common totalT2PendArletterCrea;
    private Common totalT2PendIwoGenerated;
    private Common totalT2Created;
    private Common totalT2Updated;
    private Common totalResaddrCreated;
    private Common totalResaddrUpdated;
    private Common totalPeraddrCreated;
    private Common totalPeraddrUpdated;
    private Common totalDisaddrCreated;
    private Common totalDisaddrUpdated;
    private Common totalPayaddrCreated;
    private Common totalPayaddrUpdated;
    private Common totalPriaddrCreated;
    private Common totalPriaddrUpdated;
    private Common totalT2PendSkipRecs;
    private Common totalT2SkipRecs;
    private Common totalT16SkipRecs;
    private Common totalPrisonSkipRecs;
    private Common totalNfSkipRecs;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
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

    private InvalidSsn invalidSsn;
    private CsePerson csePerson;
  }
#endregion
}
