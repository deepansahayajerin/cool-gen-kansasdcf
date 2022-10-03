// Program: OE_B413_LOAD_FCR_RESPONSES, ID: 373539251, model: 746.
// Short name: SWEE413B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B413_LOAD_FCR_RESPONSES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB413LoadFcrResponses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B413_LOAD_FCR_RESPONSES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB413LoadFcrResponses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB413LoadFcrResponses.
  /// </summary>
  public OeB413LoadFcrResponses(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *************************************************************************************
    // Maintenance Log:
    //    Date     Request  Name   Description
    // ---------   -------  ----   
    // ---------------------------------------------------------
    // 04/01/2000   new     Sree   Original.
    // 10/15/2001  WR20132  Ed     Added new fields in EAB and logic for date of
    // death.
    // 09/30/2002  WR00289  Ed     Create CSI-R when proactive match received.
    // 03/26/2004  PR200614 GVandy Correct extraction of FCR submitted case id.
    // 04/29/2005  PR243238 Ed     Fix problem that allowed an ap whose case 
    // role had ended
    //                             to slip through.  Prevent absent parent not 
    // found from
    //                             causing abend in the future. Just report it 
    // as an error.
    // *************************************************************************************
    // 06/05/209   DDupree     Added check when processing the returning ssn to 
    // see
    //  if it is a invalid ssn and person number combination. Part of CQ7189.
    // __________________________________________________________________________________
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseOeB413Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Eab.FileInstruction = "READ";

    do
    {
      UseOeEabReadFcrMatchResponses();

      if (Equal(local.Eab.TextReturnCode, "EF"))
      {
        break;
      }

      if (!IsEmpty(local.Eab.TextReturnCode))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      ++local.RecordsRead.Count;

      // **********************************************************
      // *   Skip records other than Proactive Match records      *
      // **********************************************************
      if (!Equal(local.Eab.TextLine8, "FT"))
      {
        ++local.RecordsSkipped.Count;

        continue;
      }

      if (Equal(local.FcrProactiveMatchResponse.MatchedCaseId, 1, 3, "KPC"))
      {
        ++local.RecordsSkipped.Count;

        continue;
      }

      if (Equal(local.FcrProactiveMatchResponse.StateMemberId, 1, 3, "KPC"))
      {
        ++local.RecordsSkipped.Count;

        continue;
      }

      local.Fcr.Number =
        Substring(local.FcrProactiveMatchResponse.StateMemberId, 6, 10);
      local.FcrProactiveMatchResponse.DateReceived =
        local.ProgramProcessingInfo.ProcessDate;

      if (ReadCsePerson())
      {
        if (AsChar(local.FcrProactiveMatchResponse.ActionTypeCode) == 'C' || AsChar
          (local.FcrProactiveMatchResponse.ActionTypeCode) == 'P')
        {
          ++local.PromatchRead.Count;

          // * C = Proactive match (case changed or deleted by other state)
          // * P = Proactive match (person added, changed, deleted by other 
          // state)
          // added this check as part of cq7189.
          local.Convert.SsnNum9 =
            (int)StringToNumber(local.FcrProactiveMatchResponse.
              SubmittedOrMatchedSsn);

          if (ReadInvalidSsn())
          {
            if (!IsEmpty(local.FcrProactiveMatchResponse.
              TransmitterStateOrTerrCode))
            {
              local.StateNumber.SsnNumPart2 =
                (int)StringToNumber(local.FcrProactiveMatchResponse.
                  TransmitterStateOrTerrCode);

              if (ReadFips())
              {
                local.FipsState.Text2 = entities.Fips.StateAbbreviation;
              }
              else
              {
                ExitState = "FIPS_NF";

                break;
              }
            }
            else
            {
              local.FipsState.Text2 = "";
            }

            local.Message1.Text8 = "Bad SSN";
            local.Message1.Text6 = ", Per";
            local.Message1.Text60 =
              ": Received a proactive match that will not be used from " + local
              .FipsState.Text2;
            local.EabReportSend.RptDetail = local.Message1.Text8 + (
              local.FcrProactiveMatchResponse.SubmittedOrMatchedSsn ?? "") + local
              .Message1.Text6 + entities.CsePerson.Number + local
              .Message1.Text60;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            local.Message1.Text8 = "";
            local.Message1.Text6 = "";
            local.Message1.Text60 = "";
            local.EabReportSend.RptDetail = "";

            continue;
          }
          else
          {
            // this is fine, there is not invalid ssn record for this 
            // combination of cse person number and ssn number
          }

          UseOeB413LoadProactiveMatch();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          if (AsChar(local.ProaCreatedSuccessfully.Flag) == 'Y')
          {
            foreach(var item in ReadCase())
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CaseUnitNumber = 0;
              local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
              local.Infrastructure.Detail =
                "FCR Proactive Match Response Received";
              local.Infrastructure.ProcessStatus = "Q";
              local.Infrastructure.ReasonCode = "FPLSRCV";
              local.Infrastructure.SituationNumber = 0;
              local.Infrastructure.UserId = "FCR";
              local.Infrastructure.EventId = 10;
              local.Infrastructure.BusinessObjectCd = "FPL";
              local.Infrastructure.ReferenceDate =
                local.ProgramProcessingInfo.ProcessDate;
              ++local.ProactiveEvents.Count;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto AfterCycle;
              }
            }

            if (Equal(entities.CsePerson.DateOfDeath, local.Null1.Date))
            {
              // ******************************************************
              // CSI-R should only be sent if person is alive.
              // ******************************************************
              UseOeB413IsCsiRNeeded();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }
            }
          }
        }
        else
        {
          switch(AsChar(local.FcrProactiveMatchResponse.ActionTypeCode))
          {
            case 'D':
              ++local.PromatchDodRead.Count;

              // **** SSA Date of Death File Update ****
              local.FcrPersonAckErrorRecord.CaseId =
                local.FcrProactiveMatchResponse.SubmittedCaseId ?? Spaces(15);
              local.FcrPersonAckErrorRecord.MemberId =
                local.FcrProactiveMatchResponse.MatchedMemberId ?? Spaces(15);
              local.FcrPersonAckErrorRecord.FirstName =
                local.FcrProactiveMatchResponse.FirstName ?? Spaces(16);
              local.FcrPersonAckErrorRecord.MiddleName =
                local.FcrProactiveMatchResponse.MiddleName ?? Spaces(16);
              local.FcrPersonAckErrorRecord.LastName =
                local.FcrProactiveMatchResponse.LastName ?? Spaces(30);
              local.FcrPersonAckErrorRecord.SsaZipCodeOfLastResidence =
                local.LastResidence.Text5;

              if (AsChar(local.DodIndicator.Flag) == 'A' || AsChar
                (local.DodIndicator.Flag) == 'C')
              {
                // * A = SSA reports date of death of this person
                // * C = SSA reports change to the date of death of this person
                local.FcrPersonAckErrorRecord.DateOfDeath =
                  local.FcrProactiveMatchResponse.MatchedPersonDod;
                local.ConvertDateOfDeathDateWorkArea.Date =
                  local.FcrPersonAckErrorRecord.DateOfDeath;
                UseCabConvertDate2String();

                // **  Primary last name is set to spaces because there is no
                //     comparison of names done in this program.
                local.FcrPersonAckErrorRecord.FcrPrimaryLastName = "";
                UseCabProcessDateOfDeath();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  break;
                }

                ++local.Commit.Count;
              }
              else if (AsChar(local.DodIndicator.Flag) == 'D')
              {
                // * D = SSA reports date of death should be removed for this 
                // person
                UseCabReverseDateOfDeath();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  break;
                }

                ++local.Commit.Count;
              }
              else if (AsChar(local.DodIndicator.Flag) == 'I')
              {
                // * I = SSA reported date of death but provided an invalid date
                local.ConvertDateOfDeathDateWorkArea.Date =
                  local.FcrProactiveMatchResponse.MatchedPersonDod;
                UseCabConvertDate2String();
                local.EabReportSend.RptDetail =
                  "Date of Death Invalid for person: " + local.Fcr.Number + " Date: " +
                  local.ConvertDateOfDeathTextWorkArea.Text8;
                local.EabFileHandling.Action = "WRITE";
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  break;
                }
              }
              else
              {
                local.EabReportSend.RptDetail =
                  "Date of Death Indicator Invalid for person: " + local
                  .Fcr.Number + " Indicator: " + local.DodIndicator.Flag;
                local.EabFileHandling.Action = "WRITE";
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  break;
                }
              }

              break;
            case 'F':
              // **** FCR Query Response - not used - Kansas does not send 
              // queries ****
              ++local.RecordsSkipped.Count;

              break;
            default:
              ++local.RecordsSkipped.Count;
              local.EabReportSend.RptDetail =
                "Proactive Match Action Code Invalid for person: " + local
                .Fcr.Number + " Action Code: " + (
                  local.FcrProactiveMatchResponse.ActionTypeCode ?? "");
              local.EabFileHandling.Action = "WRITE";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                break;
              }

              break;
          }
        }
      }
      else
      {
        local.EabReportSend.RptDetail = "CSE Person was not found  :" + local
          .Fcr.Number;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        ++local.CsePersonNotFound.Count;
      }

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
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
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.Commit.Count = 0;
      }
    }
    while(!Equal(local.Eab.TextReturnCode, "EF"));

AfterCycle:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB413Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseOeB413Close();
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.ConvertDateOfDeathDateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.ConvertDateOfDeathTextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabProcessDateOfDeath()
  {
    var useImport = new CabProcessDateOfDeath.Import();
    var useExport = new CabProcessDateOfDeath.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Max.Date = local.Max.Date;
    useImport.ConvertDateOfDeath.Text8 =
      local.ConvertDateOfDeathTextWorkArea.Text8;
    useImport.PersonsUpdated.Count = local.PersonsUpdated.Count;
    useImport.DodEventsCreated.Count = local.DodEventsCreated.Count;
    useImport.DodAlertsCreated.Count = local.DodAlertsCreated.Count;
    useImport.FcrPersonAckErrorRecord.Assign(local.FcrPersonAckErrorRecord);
    useImport.SsaStateLastResidence.Text2 = local.LastResidence.Text2;
    useImport.SsaCityLastResidence.Text15 = local.LastResidence.Text15;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useExport.DodAlertsCreated.Count = local.DodAlertsCreated.Count;

    Call(CabProcessDateOfDeath.Execute, useImport, useExport);

    local.PersonsUpdated.Count = useExport.PersonsUpdated.Count;
    local.DodEventsCreated.Count = useExport.DodEventsCreated.Count;
    local.DodAlertsCreated.Count = useExport.DodAlertsCreated.Count;
  }

  private void UseCabReverseDateOfDeath()
  {
    var useImport = new CabReverseDateOfDeath.Import();
    var useExport = new CabReverseDateOfDeath.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Max.Date = local.Max.Date;
    useImport.PersonsUpdated.Count = local.PersonsUpdated.Count;
    useImport.DodEventsCreated.Count = local.DodEventsCreated.Count;
    useImport.DodAlertsCreated.Count = local.DodAlertsCreated.Count;
    useImport.FcrPersonAckErrorRecord.Assign(local.FcrPersonAckErrorRecord);
    useImport.SsaStateLastResidence.Text2 = local.LastResidence.Text2;
    useImport.SsaCityLastResidence.Text15 = local.LastResidence.Text15;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(CabReverseDateOfDeath.Execute, useImport, useExport);

    local.PersonsUpdated.Count = useExport.PersonsUpdated.Count;
    local.DodEventsCreated.Count = useExport.DodEventsCreated.Count;
    local.DodAlertsCreated.Count = useExport.DodAlertsCreated.Count;
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

  private void UseOeB413Close()
  {
    var useImport = new OeB413Close.Import();
    var useExport = new OeB413Close.Export();

    useImport.DodPersonsUpdated.Count = local.PersonsUpdated.Count;
    useImport.DodEventsCreated.Count = local.DodEventsCreated.Count;
    useImport.DodAlertsCreated.Count = local.DodAlertsCreated.Count;
    useImport.PromatchCreated.Count = local.PromatchCreated.Count;
    useImport.InterstateCaseExists.Count = local.InterstateCaseExists.Count;
    useImport.PromatchAlreadyReceived.Count =
      local.PromatchAlreadyReceived.Count;
    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.RecordsSkipped.Count = local.RecordsSkipped.Count;
    useImport.PromatchRead.Count = local.PromatchRead.Count;
    useImport.PromatchDodRead.Count = local.PromatchDodRead.Count;
    useImport.CsiRequestsCreated.Count = local.CsiRequestsCreated.Count;
    useImport.PromatchNonCsenet.Count = local.PromatchNonCsenet.Count;
    useImport.ProactiveMatchErrors.Count = local.CsePersonNotFound.Count;

    Call(OeB413Close.Execute, useImport, useExport);
  }

  private void UseOeB413Housekeeping()
  {
    var useImport = new OeB413Housekeeping.Import();
    var useExport = new OeB413Housekeeping.Export();

    Call(OeB413Housekeeping.Execute, useImport, useExport);

    MoveCode(useExport.Csi, local.Csi);
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseOeB413IsCsiRNeeded()
  {
    var useImport = new OeB413IsCsiRNeeded.Import();
    var useExport = new OeB413IsCsiRNeeded.Export();

    useImport.CsePersonNotFound.Count = local.CsePersonNotFound.Count;
    useImport.MaxDate.Date = local.Max.Date;
    useImport.Commit.Count = local.Commit.Count;
    useImport.InterstateCaseExists.Count = local.InterstateCaseExists.Count;
    useImport.PromatchNonCsenet.Count = local.PromatchNonCsenet.Count;
    useImport.PromatchAlreadyReceived.Count =
      local.PromatchAlreadyReceived.Count;
    useImport.PromatchCreated.Count = local.PromatchCreated.Count;
    useImport.CsenetCsiRequest.Count = local.CsiRequestsCreated.Count;
    useImport.FcrProactiveMatchResponse.Assign(local.FcrProactiveMatchResponse);
    useImport.CsePerson.Number = entities.CsePerson.Number;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    MoveCode(local.Csi, useImport.Csi);

    Call(OeB413IsCsiRNeeded.Execute, useImport, useExport);

    local.CsePersonNotFound.Count = useExport.CsePersonNotFound.Count;
    local.Commit.Count = useExport.Commit.Count;
    local.InterstateCaseExists.Count = useExport.InterstateCaseExists.Count;
    local.PromatchNonCsenet.Count = useExport.PromatchNonCsenet.Count;
    local.PromatchAlreadyReceived.Count =
      useExport.PromatchAlreadyReceived.Count;
    local.PromatchCreated.Count = useExport.PromatchCreated.Count;
    local.CsiRequestsCreated.Count = useExport.CsiRequest.Count;
  }

  private void UseOeB413LoadProactiveMatch()
  {
    var useImport = new OeB413LoadProactiveMatch.Import();
    var useExport = new OeB413LoadProactiveMatch.Export();

    useImport.PromatchCreated.Count = local.PromatchCreated.Count;
    useImport.PromatchAlreadyReceived.Count =
      local.PromatchAlreadyReceived.Count;
    useImport.Commit.Count = local.Commit.Count;
    useImport.FcrProactiveMatchResponse.Assign(local.FcrProactiveMatchResponse);
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(OeB413LoadProactiveMatch.Execute, useImport, useExport);

    local.ProaCreatedSuccessfully.Flag = useExport.CreatedSuccessfully.Flag;
    local.PromatchCreated.Count = useExport.PromatchCreated.Count;
    local.PromatchAlreadyReceived.Count =
      useExport.PromatchAlreadyReceived.Count;
    local.Commit.Count = useExport.Commit.Count;
  }

  private void UseOeEabReadFcrMatchResponses()
  {
    var useImport = new OeEabReadFcrMatchResponses.Import();
    var useExport = new OeEabReadFcrMatchResponses.Export();

    useImport.External.Assign(local.Eab);
    useExport.CaseChangeType.Flag = local.CaseChangeType.Flag;
    useExport.PreviousCaseId.Text15 = local.PreviousCaseId.Text15;
    useExport.PersonDeleteIndicator.Flag = local.PersonDeleteIndicator.Flag;
    useExport.LumpSum.Assign(local.LumpSum);
    useExport.LastResidence.Assign(local.LastResidence);
    useExport.DodIndicator.Flag = local.DodIndicator.Flag;
    useExport.External.Assign(local.Eab);
    useExport.FcrProactiveMatchResponse.Assign(local.FcrProactiveMatchResponse);

    Call(OeEabReadFcrMatchResponses.Execute, useImport, useExport);

    local.CaseChangeType.Flag = useExport.CaseChangeType.Flag;
    local.PreviousCaseId.Text15 = useExport.PreviousCaseId.Text15;
    local.PersonDeleteIndicator.Flag = useExport.PersonDeleteIndicator.Flag;
    local.LumpSum.Assign(useExport.LumpSum);
    local.LastResidence.Assign(useExport.LastResidence);
    local.DodIndicator.Flag = useExport.DodIndicator.Flag;
    local.Eab.Assign(useExport.External);
    local.FcrProactiveMatchResponse.Assign(useExport.FcrProactiveMatchResponse);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
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
        db.SetString(command, "numb", local.Fcr.Number);
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

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", local.StateNumber.SsnNumPart2);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "ssn", local.Convert.SsnNum9);
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
    /// <summary>
    /// A value of FipsState.
    /// </summary>
    [JsonPropertyName("fipsState")]
    public WorkArea FipsState
    {
      get => fipsState ??= new();
      set => fipsState = value;
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
    /// A value of StateNumber.
    /// </summary>
    [JsonPropertyName("stateNumber")]
    public SsnWorkArea StateNumber
    {
      get => stateNumber ??= new();
      set => stateNumber = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public SsnWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
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
    /// A value of ProactiveEvents.
    /// </summary>
    [JsonPropertyName("proactiveEvents")]
    public Common ProactiveEvents
    {
      get => proactiveEvents ??= new();
      set => proactiveEvents = value;
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
    /// A value of ProaCreatedSuccessfully.
    /// </summary>
    [JsonPropertyName("proaCreatedSuccessfully")]
    public Common ProaCreatedSuccessfully
    {
      get => proaCreatedSuccessfully ??= new();
      set => proaCreatedSuccessfully = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of ConvertDateOfDeathTextWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateOfDeathTextWorkArea")]
    public TextWorkArea ConvertDateOfDeathTextWorkArea
    {
      get => convertDateOfDeathTextWorkArea ??= new();
      set => convertDateOfDeathTextWorkArea = value;
    }

    /// <summary>
    /// A value of ConvertDateOfDeathDateWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateOfDeathDateWorkArea")]
    public DateWorkArea ConvertDateOfDeathDateWorkArea
    {
      get => convertDateOfDeathDateWorkArea ??= new();
      set => convertDateOfDeathDateWorkArea = value;
    }

    /// <summary>
    /// A value of PersonsUpdated.
    /// </summary>
    [JsonPropertyName("personsUpdated")]
    public Common PersonsUpdated
    {
      get => personsUpdated ??= new();
      set => personsUpdated = value;
    }

    /// <summary>
    /// A value of DodEventsCreated.
    /// </summary>
    [JsonPropertyName("dodEventsCreated")]
    public Common DodEventsCreated
    {
      get => dodEventsCreated ??= new();
      set => dodEventsCreated = value;
    }

    /// <summary>
    /// A value of DodAlertsCreated.
    /// </summary>
    [JsonPropertyName("dodAlertsCreated")]
    public Common DodAlertsCreated
    {
      get => dodAlertsCreated ??= new();
      set => dodAlertsCreated = value;
    }

    /// <summary>
    /// A value of FcrPersonAckErrorRecord.
    /// </summary>
    [JsonPropertyName("fcrPersonAckErrorRecord")]
    public FcrPersonAckErrorRecord FcrPersonAckErrorRecord
    {
      get => fcrPersonAckErrorRecord ??= new();
      set => fcrPersonAckErrorRecord = value;
    }

    /// <summary>
    /// A value of CaseChangeType.
    /// </summary>
    [JsonPropertyName("caseChangeType")]
    public Common CaseChangeType
    {
      get => caseChangeType ??= new();
      set => caseChangeType = value;
    }

    /// <summary>
    /// A value of PreviousCaseId.
    /// </summary>
    [JsonPropertyName("previousCaseId")]
    public WorkArea PreviousCaseId
    {
      get => previousCaseId ??= new();
      set => previousCaseId = value;
    }

    /// <summary>
    /// A value of PersonDeleteIndicator.
    /// </summary>
    [JsonPropertyName("personDeleteIndicator")]
    public Common PersonDeleteIndicator
    {
      get => personDeleteIndicator ??= new();
      set => personDeleteIndicator = value;
    }

    /// <summary>
    /// A value of LumpSum.
    /// </summary>
    [JsonPropertyName("lumpSum")]
    public WorkArea LumpSum
    {
      get => lumpSum ??= new();
      set => lumpSum = value;
    }

    /// <summary>
    /// A value of LastResidence.
    /// </summary>
    [JsonPropertyName("lastResidence")]
    public WorkArea LastResidence
    {
      get => lastResidence ??= new();
      set => lastResidence = value;
    }

    /// <summary>
    /// A value of DodIndicator.
    /// </summary>
    [JsonPropertyName("dodIndicator")]
    public Common DodIndicator
    {
      get => dodIndicator ??= new();
      set => dodIndicator = value;
    }

    /// <summary>
    /// A value of PromatchCreated.
    /// </summary>
    [JsonPropertyName("promatchCreated")]
    public Common PromatchCreated
    {
      get => promatchCreated ??= new();
      set => promatchCreated = value;
    }

    /// <summary>
    /// A value of InterstateCaseExists.
    /// </summary>
    [JsonPropertyName("interstateCaseExists")]
    public Common InterstateCaseExists
    {
      get => interstateCaseExists ??= new();
      set => interstateCaseExists = value;
    }

    /// <summary>
    /// A value of PromatchAlreadyReceived.
    /// </summary>
    [JsonPropertyName("promatchAlreadyReceived")]
    public Common PromatchAlreadyReceived
    {
      get => promatchAlreadyReceived ??= new();
      set => promatchAlreadyReceived = value;
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
    /// A value of RecordsSkipped.
    /// </summary>
    [JsonPropertyName("recordsSkipped")]
    public Common RecordsSkipped
    {
      get => recordsSkipped ??= new();
      set => recordsSkipped = value;
    }

    /// <summary>
    /// A value of PromatchRead.
    /// </summary>
    [JsonPropertyName("promatchRead")]
    public Common PromatchRead
    {
      get => promatchRead ??= new();
      set => promatchRead = value;
    }

    /// <summary>
    /// A value of PromatchDodRead.
    /// </summary>
    [JsonPropertyName("promatchDodRead")]
    public Common PromatchDodRead
    {
      get => promatchDodRead ??= new();
      set => promatchDodRead = value;
    }

    /// <summary>
    /// A value of CsiRequestsCreated.
    /// </summary>
    [JsonPropertyName("csiRequestsCreated")]
    public Common CsiRequestsCreated
    {
      get => csiRequestsCreated ??= new();
      set => csiRequestsCreated = value;
    }

    /// <summary>
    /// A value of PromatchNonCsenet.
    /// </summary>
    [JsonPropertyName("promatchNonCsenet")]
    public Common PromatchNonCsenet
    {
      get => promatchNonCsenet ??= new();
      set => promatchNonCsenet = value;
    }

    /// <summary>
    /// A value of CsePersonNotFound.
    /// </summary>
    [JsonPropertyName("csePersonNotFound")]
    public Common CsePersonNotFound
    {
      get => csePersonNotFound ??= new();
      set => csePersonNotFound = value;
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
    /// A value of Csi.
    /// </summary>
    [JsonPropertyName("csi")]
    public Code Csi
    {
      get => csi ??= new();
      set => csi = value;
    }

    /// <summary>
    /// A value of Fcr.
    /// </summary>
    [JsonPropertyName("fcr")]
    public CsePerson Fcr
    {
      get => fcr ??= new();
      set => fcr = value;
    }

    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
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
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public External Eab
    {
      get => eab ??= new();
      set => eab = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ZdelLocalFcr.
    /// </summary>
    [JsonPropertyName("zdelLocalFcr")]
    public Case1 ZdelLocalFcr
    {
      get => zdelLocalFcr ??= new();
      set => zdelLocalFcr = value;
    }

    private WorkArea fipsState;
    private WorkArea message1;
    private SsnWorkArea stateNumber;
    private SsnWorkArea convert;
    private DateWorkArea null1;
    private Common proactiveEvents;
    private Infrastructure infrastructure;
    private Common proaCreatedSuccessfully;
    private DateWorkArea max;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea convertDateOfDeathTextWorkArea;
    private DateWorkArea convertDateOfDeathDateWorkArea;
    private Common personsUpdated;
    private Common dodEventsCreated;
    private Common dodAlertsCreated;
    private FcrPersonAckErrorRecord fcrPersonAckErrorRecord;
    private Common caseChangeType;
    private WorkArea previousCaseId;
    private Common personDeleteIndicator;
    private WorkArea lumpSum;
    private WorkArea lastResidence;
    private Common dodIndicator;
    private Common promatchCreated;
    private Common interstateCaseExists;
    private Common promatchAlreadyReceived;
    private Common recordsRead;
    private Common recordsSkipped;
    private Common promatchRead;
    private Common promatchDodRead;
    private Common csiRequestsCreated;
    private Common promatchNonCsenet;
    private Common csePersonNotFound;
    private Common commit;
    private Code csi;
    private CsePerson fcr;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External eab;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private Case1 zdelLocalFcr;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Fips fips;
    private InvalidSsn invalidSsn;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
  }
#endregion
}
