// Program: OE_B465_URA_AE_INTERFACE_BATCH, ID: 370981422, model: 746.
// Short name: SWEE465B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B465_URA_AE_INTERFACE_BATCH.
/// </para>
/// <para>
/// This program will be responsible for two functions: maintaining IM HOUSEHOLD
/// cases and applying grant payments to the IM HOUSEHOLD databases using data
/// received from external action blocks. This module will be design to
/// facilitate restart capability.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB465UraAeInterfaceBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B465_URA_AE_INTERFACE_BATCH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB465UraAeInterfaceBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB465UraAeInterfaceBatch.
  /// </summary>
  public OeB465UraAeInterfaceBatch(IContext context, Import import,
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
    // ***********************************************************************
    // Date          Developer	Description
    // ----------      ------------  
    // ---------------------------------------
    // 05/04/2000      D. Villareal  Initial Creation
    // 09/20/2000      E. Lyman   	Massive Rewrite
    // ***********************************************************************
    // *********************************************************************************
    // Revised restart logic has been added:
    // The input file is in AE Case number order.  Checkpoints are taken only 
    // when
    // the case number changes, in order to finish a logical unit of work.  This
    // program is restarted automatically via the checkpoint restart entity.  
    // The
    // input file is read and records skipped until the checkpointed case is 
    // read.
    // Normal processing then begins with the next case.
    // ****************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.Housekeeping.SelectChar = "O";
    UseOeB465Housekeeping1();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ****************************************************************************
    // Process each Benefit from the sequential file.
    // ****************************************************************************
    while(!Equal(local.EabFileHandling.Status, "EOF"))
    {
      local.EabFileHandling.Action = "READ";
      UseOeB465EabReadAeBenefitFile();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          if (Lt(local.ImHousehold.AeCaseNo, local.Previous.AeCaseNo))
          {
            ExitState = "ACO_RE0000_INPUT_FILE_NOT_SORTED";

            goto AfterCycle;
          }

          local.TotalGrantAmt.TotalCurrency += local.ImHouseholdMbrMnthlySum.
            GrantAmount.GetValueOrDefault();
          local.TotalGrantsRead.Value =
            local.TotalGrantsRead.Value.GetValueOrDefault() + 1;

          if (!Lt(local.Restart.AeCaseNo, local.ImHousehold.AeCaseNo))
          {
            continue;
          }

          local.TotalRestartGrantAmt.TotalCurrency += local.
            ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();
          local.TotalRestartGrantsRead.Value =
            local.TotalRestartGrantsRead.Value.GetValueOrDefault() + 1;
          local.TriggerDate.Date =
            IntToDate(local.ImHouseholdMbrMnthlySum.Year * 10000 + local
            .ImHouseholdMbrMnthlySum.Month * 100 + 1);

          break;
        case "EOF":
          goto AfterCycle;
        default:
          ExitState = "ERROR_READING_FILE_AB";
          local.EabReportSend.RptDetail =
            "ERROR: Unable to read benefits file.";

          goto AfterCycle;
      }

      // ****************************************************************************
      // Case number control break
      // ****************************************************************************
      if (!Equal(local.ImHousehold.AeCaseNo, local.Previous.AeCaseNo))
      {
        // ****************************************************************************
        // 
        // Determine if a commit is needed
        // ****************************************************************************
        if (local.CasesSinceLastChkpnt.Count > local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          // ****************************************************************************
          // Populate fields need to update the program checkpoint retart table.
          // Setting
          // the restart indicator to 'Y' indicates the module has begun  
          // Subsequent
          // executions of this module will read this flag and begin processing 
          // as a restart.
          // ****************************************************************************
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = local.Previous.AeCaseNo;
          local.ProgramCheckpointRestart.CheckpointCount =
            local.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault() +
            1;
          UseUpdatePgmCheckpointRestart2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabReportSend.RptDetail =
              "ERROR: Unable to Update Program Checkpoint Restart Table.";

            break;
          }

          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";
            local.EabReportSend.RptDetail =
              "ERROR: Unable to Update Program Checkpoint Restart Table.";

            break;
          }

          local.EabReportSend.RptDetail = "Commit after AE Case: " + local
            .ImHousehold.AeCaseNo + " Time: " + NumberToString
            (TimeToInt(Time(Now())), 15);
          local.EabFileHandling.Action = "WRITE";
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

            return;
          }

          local.CasesSinceLastChkpnt.Count = 0;
        }

        ++local.CasesSinceLastChkpnt.Count;
        local.Previous.AeCaseNo = local.ImHousehold.AeCaseNo;
      }

      if (!ReadImHousehold())
      {
        // ****************************************************************************
        // 
        // Create a new household record
        // 
        // ****************************************************************************
        try
        {
          CreateImHousehold();
          local.HhRecsAdded.Value =
            local.HhRecsAdded.Value.GetValueOrDefault() + 1;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Create IM Household (AE)";
                

              break;
            case ErrorCode.PermittedValueViolation:
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Create IM Household (PV)";
                

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!ReadCsePerson1())
      {
        try
        {
          CreateCsePerson();
          local.PersRecsAdded.Value =
            local.PersRecsAdded.Value.GetValueOrDefault() + 1;

          // ****************************************************************************
          // Must read CSE_PERSON for currency.
          // ****************************************************************************
          if (!ReadCsePerson2())
          {
            ExitState = "CSE_PERSON_ACCOUNT_NF";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Read CSE_PERSON Just Created";
              

            break;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CSE_PERSON_ACCOUNT_AE";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Create CSE_PERSON";
                

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CSE_PERSON_ACCOUNT_PV";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Create CSE_PERSON";
                

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // ****************************************************************************
      // 
      // Locate existing Year/Month grant entries
      // 
      // ****************************************************************************
      if (ReadImHouseholdMbrMnthlySum())
      {
        // ****************************************************************************
        // 
        // Update the exiting summary record by adding grant to existing
        // amounts.
        // ****************************************************************************
        local.Total.GrantAmount =
          entities.ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault() + local
          .ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();
        local.Total.UraAmount =
          entities.ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault() + local
          .ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();

        try
        {
          UpdateImHouseholdMbrMnthlySum();
          local.HhMbrMsumRecsUpdated.Value =
            local.HhMbrMsumRecsUpdated.Value.GetValueOrDefault() + 1;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "IM_HOUSEHOLD_MBR_MNTHLY_SUM_NU";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Update IM Household Member Monthly Summary (NU)";
                

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "IM_HOUSEHOLD_MBR_MNTHLY_SUM_PV";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Update IM Household Member Monthly Summary (PV)";
                

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
        try
        {
          CreateImHouseholdMbrMnthlySum();
          local.HhMbrMsumRecsAdded.Value =
            local.HhMbrMsumRecsAdded.Value.GetValueOrDefault() + 1;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "IM_HOUSEHOLD_MBR_MNTHLY_SUM_AE";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Create IM Household Member Monthly Summary (AE)";
                

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "IM_HOUSEHOLD_MBR_MNTHLY_SUM_PV";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Create IM Household Member Monthly Summary (PV)";
                

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // ****************************************************************************
      // 
      // Update CSE PERSON ACCOUNT trigger record
      // 
      // ****************************************************************************
      if (ReadCsePersonAccount())
      {
        if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
          local.Null1.Date) || Lt
          (local.TriggerDate.Date, entities.CsePersonAccount.PgmChgEffectiveDate))
          
        {
          try
          {
            UpdateCsePersonAccount();
            local.TriggerRecsSet.Value =
              local.TriggerRecsSet.Value.GetValueOrDefault() + 1;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_CSE_PERSON_ACCOUNT_NU";
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Update CSE Person Account (NU)";
                  

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + " ERROR: Unable to Update CSE Person Account (PV)";
                  

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else
      {
        // ****************************************************************************
        // No CSE_PERSON_ACCOUNT found - Countinue Processing.
        // ****************************************************************************
      }
    }

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ****************************************************************************
      // Call: update_pgm_checkpoint_restart.
      // This action block will update the program checkpoint restart table. 
      // Setting
      // the restart indicator to 'N' indicates the module successfully 
      // completed processing.
      // ****************************************************************************
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.CheckpointCount = 0;
      UseUpdatePgmCheckpointRestart1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabReportSend.RptDetail =
          "ERROR: Unable to update the Program Checkpoint Restart table.";
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ****************************************************************************
      // Write the summary control records.
      // ***************************************************************************
      UseOeB465WriteControlTotals();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Housekeeping.SelectChar = "C";
      UseOeB465Housekeeping2();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
      }
    }
    else
    {
      if (IsEmpty(local.EabReportSend.RptDetail))
      {
        local.EabReportSend.RptDetail =
          "Fatal Error has Occurred - PROGRAM ABORTED.";
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

        return;
      }

      // ****************************************************************************
      // Write the summary control records.
      // ***************************************************************************
      UseOeB465WriteControlTotals();
      local.Housekeeping.SelectChar = "C";
      UseOeB465Housekeeping2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeB465EabReadAeBenefitFile()
  {
    var useImport = new OeB465EabReadAeBenefitFile.Import();
    var useExport = new OeB465EabReadAeBenefitFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.IssueType.Text2 = local.IssuedType.Text2;
    useExport.ProgramType.Text2 = local.ProgType.Text2;
    useExport.CsePerson.Number = local.CsePerson.Number;
    useExport.ImHousehold.AeCaseNo = local.ImHousehold.AeCaseNo;
    useExport.ImHouseholdMbrMnthlySum.Assign(local.ImHouseholdMbrMnthlySum);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(OeB465EabReadAeBenefitFile.Execute, useImport, useExport);

    local.IssuedType.Text2 = useExport.IssueType.Text2;
    local.ProgType.Text2 = useExport.ProgramType.Text2;
    local.CsePerson.Number = useExport.CsePerson.Number;
    local.ImHousehold.AeCaseNo = useExport.ImHousehold.AeCaseNo;
    local.ImHouseholdMbrMnthlySum.Assign(useExport.ImHouseholdMbrMnthlySum);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeB465Housekeeping1()
  {
    var useImport = new OeB465Housekeeping.Import();
    var useExport = new OeB465Housekeeping.Export();

    useImport.Common.SelectChar = local.Housekeeping.SelectChar;

    Call(OeB465Housekeeping.Execute, useImport, useExport);

    local.Housekeeping.SelectChar = useImport.Common.SelectChar;
    local.Restart.AeCaseNo = useExport.Restart.AeCaseNo;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseOeB465Housekeeping2()
  {
    var useImport = new OeB465Housekeeping.Import();
    var useExport = new OeB465Housekeeping.Export();

    useImport.Common.SelectChar = local.Housekeeping.SelectChar;

    Call(OeB465Housekeeping.Execute, useImport, useExport);

    local.Housekeeping.SelectChar = useImport.Common.SelectChar;
  }

  private void UseOeB465WriteControlTotals()
  {
    var useImport = new OeB465WriteControlTotals.Import();
    var useExport = new OeB465WriteControlTotals.Export();

    useImport.TotalRestartGrantAmt.TotalCurrency =
      local.TotalRestartGrantAmt.TotalCurrency;
    useImport.TotalGrantAmt.TotalCurrency = local.TotalGrantAmt.TotalCurrency;
    useImport.TotalRestartGrantsRead.Value = local.TotalRestartGrantsRead.Value;
    useImport.TriggerRecsSet.Value = local.TriggerRecsSet.Value;
    useImport.HhMbrMsumRecsAdded.Value = local.HhMbrMsumRecsAdded.Value;
    useImport.HhMbrMsumRecsUpdated.Value = local.HhMbrMsumRecsUpdated.Value;
    useImport.PersRecsAdded.Value = local.PersRecsAdded.Value;
    useImport.HhRecsAdded.Value = local.HhRecsAdded.Value;
    useImport.TotalGrantsRead.Value = local.TotalGrantsRead.Value;

    Call(OeB465WriteControlTotals.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart1()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart2()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void CreateCsePerson()
  {
    var number = local.CsePerson.Number;
    var type1 = "C";
    var aeCaseNumber = local.ImHousehold.AeCaseNo;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var bornOutOfWedlock = "U";
    var paternityEstablishedIndicator = "N";

    CheckValid<CsePerson>("Type1", type1);
    entities.CsePerson.Populated = false;
    Update("CreateCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", number);
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "occupation", "");
        db.SetNullableString(command, "aeCaseNumber", aeCaseNumber);
        db.SetNullableDate(command, "dateOfDeath", default(DateTime));
        db.SetNullableString(command, "illegalAlienInd", "");
        db.SetNullableString(command, "currentSpouseMi", "");
        db.SetNullableString(command, "currSpouse1StNm", "");
        db.SetNullableInt32(command, "emergencyPhone", 0);
        db.SetNullableString(command, "nameMaiden", "");
        db.SetNullableString(command, "birthPlaceCity", "");
        db.SetNullableString(command, "taxId", "");
        db.SetNullableString(command, "organizationName", "");
        db.SetNullableInt32(command, "weight", 0);
        db.SetNullableInt32(command, "heightFt", 0);
        db.SetNullableInt32(command, "heightIn", 0);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableString(command, "kscaresNumber", "");
        db.SetNullableString(command, "workPhoneExt", "");
        db.SetNullableString(command, "otherIdInfo", "");
        db.SetNullableString(command, "outOfWedlock", bornOutOfWedlock);
        db.SetNullableString(command, "cseToEstPatr", bornOutOfWedlock);
        db.SetNullableString(
          command, "patEstabInd", paternityEstablishedIndicator);
      });

    entities.CsePerson.Number = number;
    entities.CsePerson.Type1 = type1;
    entities.CsePerson.AeCaseNumber = aeCaseNumber;
    entities.CsePerson.CreatedBy = createdBy;
    entities.CsePerson.CreatedTimestamp = createdTimestamp;
    entities.CsePerson.BornOutOfWedlock = bornOutOfWedlock;
    entities.CsePerson.CseToEstblPaternity = bornOutOfWedlock;
    entities.CsePerson.PaternityEstablishedIndicator =
      paternityEstablishedIndicator;
    entities.CsePerson.Populated = true;
  }

  private void CreateImHousehold()
  {
    var aeCaseNo = local.ImHousehold.AeCaseNo;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ImHousehold.Populated = false;
    Update("CreateImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", aeCaseNo);
        db.SetNullableInt32(command, "householdSize", 0);
        db.SetString(command, "caseStatus", "");
        db.SetDate(command, "statusDate", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTimes", default(DateTime));
        db.SetNullableString(command, "type", "");
      });

    entities.ImHousehold.AeCaseNo = aeCaseNo;
    entities.ImHousehold.CreatedBy = createdBy;
    entities.ImHousehold.CreatedTimestamp = createdTimestamp;
    entities.ImHousehold.Populated = true;
  }

  private void CreateImHouseholdMbrMnthlySum()
  {
    var year = local.ImHouseholdMbrMnthlySum.Year;
    var month = local.ImHouseholdMbrMnthlySum.Month;
    var relationship = local.ImHouseholdMbrMnthlySum.Relationship;
    var grantAmount =
      local.ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTmst = Now();
    var imhAeCaseNo = entities.ImHousehold.AeCaseNo;
    var cspNumber = entities.CsePerson.Number;

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("CreateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetInt32(command, "year0", year);
        db.SetInt32(command, "month0", month);
        db.SetString(command, "relationship", relationship);
        db.SetNullableDecimal(command, "grantAmt", grantAmount);
        db.SetNullableDecimal(command, "grantMedAmt", 0M);
        db.SetNullableDecimal(command, "uraAmount", grantAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetString(command, "imhAeCaseNo", imhAeCaseNo);
        db.SetString(command, "cspNumber", cspNumber);
      });

    entities.ImHouseholdMbrMnthlySum.Year = year;
    entities.ImHouseholdMbrMnthlySum.Month = month;
    entities.ImHouseholdMbrMnthlySum.Relationship = relationship;
    entities.ImHouseholdMbrMnthlySum.GrantAmount = grantAmount;
    entities.ImHouseholdMbrMnthlySum.UraAmount = grantAmount;
    entities.ImHouseholdMbrMnthlySum.CreatedBy = createdBy;
    entities.ImHouseholdMbrMnthlySum.CreatedTmst = createdTmst;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedBy = "";
    entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst = null;
    entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = imhAeCaseNo;
    entities.ImHouseholdMbrMnthlySum.CspNumber = cspNumber;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.CsePerson.CreatedBy = db.GetString(reader, 3);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 5);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 6);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 7);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.CsePerson.CreatedBy = db.GetString(reader, 3);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 5);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 6);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 7);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.CsePersonAccount.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 3);
        entities.CsePersonAccount.PgmChgEffectiveDate =
          db.GetNullableDate(reader, 4);
        entities.CsePersonAccount.TriggerType = db.GetNullableString(reader, 5);
        entities.CsePersonAccount.Populated = true;
      });
  }

  private bool ReadImHousehold()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", local.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.CreatedBy = db.GetString(reader, 1);
        entities.ImHousehold.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ImHousehold.Populated = true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetInt32(command, "year0", local.ImHouseholdMbrMnthlySum.Year);
        db.SetInt32(command, "month0", local.ImHouseholdMbrMnthlySum.Month);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlySum.GrantAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ImHouseholdMbrMnthlySum.CreatedBy = db.GetString(reader, 5);
        entities.ImHouseholdMbrMnthlySum.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 9);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 10);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private void UpdateCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = local.TriggerDate.Date;
    var triggerType = "U";

    entities.CsePersonAccount.Populated = false;
    Update("UpdateCsePersonAccount",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.SetNullableString(command, "triggerType", triggerType);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetString(command, "type", entities.CsePersonAccount.Type1);
      });

    entities.CsePersonAccount.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonAccount.LastUpdatedTmst = lastUpdatedTmst;
    entities.CsePersonAccount.PgmChgEffectiveDate = pgmChgEffectiveDate;
    entities.CsePersonAccount.TriggerType = triggerType;
    entities.CsePersonAccount.Populated = true;
  }

  private void UpdateImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var grantAmount = local.Total.GrantAmount.GetValueOrDefault();
    var uraAmount = local.Total.UraAmount.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("UpdateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "grantAmt", grantAmount);
        db.SetNullableDecimal(command, "uraAmount", uraAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(command, "year0", entities.ImHouseholdMbrMnthlySum.Year);
        db.SetInt32(command, "month0", entities.ImHouseholdMbrMnthlySum.Month);
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
        db.SetString(
          command, "cspNumber", entities.ImHouseholdMbrMnthlySum.CspNumber);
      });

    entities.ImHouseholdMbrMnthlySum.GrantAmount = grantAmount;
    entities.ImHouseholdMbrMnthlySum.UraAmount = uraAmount;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedBy = lastUpdatedBy;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst = lastUpdatedTmst;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
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
    /// A value of TotalRestartGrantAmt.
    /// </summary>
    [JsonPropertyName("totalRestartGrantAmt")]
    public Common TotalRestartGrantAmt
    {
      get => totalRestartGrantAmt ??= new();
      set => totalRestartGrantAmt = value;
    }

    /// <summary>
    /// A value of TotalGrantAmt.
    /// </summary>
    [JsonPropertyName("totalGrantAmt")]
    public Common TotalGrantAmt
    {
      get => totalGrantAmt ??= new();
      set => totalGrantAmt = value;
    }

    /// <summary>
    /// A value of TriggerDate.
    /// </summary>
    [JsonPropertyName("triggerDate")]
    public DateWorkArea TriggerDate
    {
      get => triggerDate ??= new();
      set => triggerDate = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public ImHousehold Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Issued.
    /// </summary>
    [JsonPropertyName("issued")]
    public DateWorkArea Issued
    {
      get => issued ??= new();
      set => issued = value;
    }

    /// <summary>
    /// A value of IssuedType.
    /// </summary>
    [JsonPropertyName("issuedType")]
    public WorkArea IssuedType
    {
      get => issuedType ??= new();
      set => issuedType = value;
    }

    /// <summary>
    /// A value of ProgSubtype.
    /// </summary>
    [JsonPropertyName("progSubtype")]
    public WorkArea ProgSubtype
    {
      get => progSubtype ??= new();
      set => progSubtype = value;
    }

    /// <summary>
    /// A value of ProgType.
    /// </summary>
    [JsonPropertyName("progType")]
    public WorkArea ProgType
    {
      get => progType ??= new();
      set => progType = value;
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public ImHousehold Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public ImHouseholdMbrMnthlySum Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of Housekeeping.
    /// </summary>
    [JsonPropertyName("housekeeping")]
    public Common Housekeeping
    {
      get => housekeeping ??= new();
      set => housekeeping = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of CasesSinceLastChkpnt.
    /// </summary>
    [JsonPropertyName("casesSinceLastChkpnt")]
    public Common CasesSinceLastChkpnt
    {
      get => casesSinceLastChkpnt ??= new();
      set => casesSinceLastChkpnt = value;
    }

    /// <summary>
    /// A value of TotalRestartGrantsRead.
    /// </summary>
    [JsonPropertyName("totalRestartGrantsRead")]
    public ProgramControlTotal TotalRestartGrantsRead
    {
      get => totalRestartGrantsRead ??= new();
      set => totalRestartGrantsRead = value;
    }

    /// <summary>
    /// A value of TriggerRecsSet.
    /// </summary>
    [JsonPropertyName("triggerRecsSet")]
    public ProgramControlTotal TriggerRecsSet
    {
      get => triggerRecsSet ??= new();
      set => triggerRecsSet = value;
    }

    /// <summary>
    /// A value of HhMbrMsumRecsAdded.
    /// </summary>
    [JsonPropertyName("hhMbrMsumRecsAdded")]
    public ProgramControlTotal HhMbrMsumRecsAdded
    {
      get => hhMbrMsumRecsAdded ??= new();
      set => hhMbrMsumRecsAdded = value;
    }

    /// <summary>
    /// A value of HhMbrMsumRecsUpdated.
    /// </summary>
    [JsonPropertyName("hhMbrMsumRecsUpdated")]
    public ProgramControlTotal HhMbrMsumRecsUpdated
    {
      get => hhMbrMsumRecsUpdated ??= new();
      set => hhMbrMsumRecsUpdated = value;
    }

    /// <summary>
    /// A value of PersRecsAdded.
    /// </summary>
    [JsonPropertyName("persRecsAdded")]
    public ProgramControlTotal PersRecsAdded
    {
      get => persRecsAdded ??= new();
      set => persRecsAdded = value;
    }

    /// <summary>
    /// A value of OverpaymentsRead.
    /// </summary>
    [JsonPropertyName("overpaymentsRead")]
    public ProgramControlTotal OverpaymentsRead
    {
      get => overpaymentsRead ??= new();
      set => overpaymentsRead = value;
    }

    /// <summary>
    /// A value of HhRecsAdded.
    /// </summary>
    [JsonPropertyName("hhRecsAdded")]
    public ProgramControlTotal HhRecsAdded
    {
      get => hhRecsAdded ??= new();
      set => hhRecsAdded = value;
    }

    /// <summary>
    /// A value of TotalGrantsRead.
    /// </summary>
    [JsonPropertyName("totalGrantsRead")]
    public ProgramControlTotal TotalGrantsRead
    {
      get => totalGrantsRead ??= new();
      set => totalGrantsRead = value;
    }

    private Common totalRestartGrantAmt;
    private Common totalGrantAmt;
    private DateWorkArea triggerDate;
    private ImHousehold restart;
    private DateWorkArea issued;
    private WorkArea issuedType;
    private WorkArea progSubtype;
    private WorkArea progType;
    private CsePerson csePerson;
    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private DateWorkArea null1;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ImHousehold previous;
    private ImHouseholdMbrMnthlySum total;
    private Common housekeeping;
    private External passArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common casesSinceLastChkpnt;
    private ProgramControlTotal totalRestartGrantsRead;
    private ProgramControlTotal triggerRecsSet;
    private ProgramControlTotal hhMbrMsumRecsAdded;
    private ProgramControlTotal hhMbrMsumRecsUpdated;
    private ProgramControlTotal persRecsAdded;
    private ProgramControlTotal overpaymentsRead;
    private ProgramControlTotal hhRecsAdded;
    private ProgramControlTotal totalGrantsRead;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
  }
#endregion
}
