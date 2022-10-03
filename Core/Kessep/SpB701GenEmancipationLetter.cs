// Program: SP_B701_GEN_EMANCIPATION_LETTER, ID: 372446762, model: 746.
// Short name: SWEP701B
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
/// A program: SP_B701_GEN_EMANCIPATION_LETTER.
/// </para>
/// <para>
/// Input : DB2
/// Output : DB2
/// External Commit.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB701GenEmancipationLetter: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B701_GEN_EMANCIPATION_LETTER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB701GenEmancipationLetter(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB701GenEmancipationLetter.
  /// </summary>
  public SpB701GenEmancipationLetter(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------
    //  Date		Developer	Request #      Description
    // --------------------------------------------------------------------------------------
    // 07/03/1996	Siraj Konkader			Initial Dev
    // 01/26/1999	M Ramirez			Re-work to remove DB2 error report
    // 01/26/1999	M Ramirez			Re-work to create document_trigger,
    // 						instead of creating document
    // 01/26/1999	M Ramirez			Re-work to use housekeeping and close cabs.
    // 08/02/1999	M Ramirez			Added Full/Partial runs:
    // 						F(ull)    - updates children, and
    // 							  - generates document triggers
    // 						D(ocs)    - generates document triggers
    // 						U(pdates) - updates children
    // 08/02/1999	M Ramirez			Added check for AR Address to aid
    // 						processing efficiency
    // 09/07/1999	M Ramirez	523		Change to check for NULL DATE before
    // 						updating Date of Emancipation
    // 05/08/2000	M Ramirez	91601		Update end dated children as well.
    // 06/12/2000	M Ramirez	none		While testing for 91601, found a
    // 						restart error, which I corrected.
    // 07/28/2000	M Ramirez	100373		Don't generate the letter for an XA child
    // 12/11/2001	M Ashworth	133483          Changed to call CAB which considers
    // interstate involvement to determine
    //                                                 
    // whether to send the EMANCIPA or an interstate
    // equivalent to the other state.
    // 07/07/2020	Raj	        CQ66150         Modified to revise Job(SRRUN111) 
    // scedule from Monthly to Daily.
    // 						Update the Case Role Emancipation Date based on date of birth value
    // 						from KSD_CLIENT_BASIC.
    // --------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB701Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (AsChar(local.RunType.Flag) != 'D')
    {
      do
      {
        local.Loop.Flag = "N";

        // mjr
        // -------------------------------------------------
        // 08/02/1999
        // Update all children without a date_of_emancipation
        // --------------------------------------------------------------
        // mjr
        // ----------------------------------------------
        // 05/08/2000
        // PR# 91601 - Update end dated children as well.
        // Removed qualification
        // "AND DESIRED CHILD CASE_ROLE END_DATE IS GREATER
        // OR EQUAL TO LOCAL PROGRAM_PROCESSING_INFO PROCESS_DATE"
        // from the following READ EACH
        // ------------------------------------------------------------
        foreach(var item in ReadCaseRoleCsePersonCase2())
        {
          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "DEBUG:  Updating Child; Child Number = " + entities
              .ChildCsePerson.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
          }

          local.EabReportSend.RptDetail = "";
          local.Read.Type1 = local.NullAbendData.Type1;
          ExitState = "ACO_NN0000_ALL_OK";
          ++local.CheckpointRead.Count;
          ++local.LcontrolChildRead.Count;

          // ----------------------------------------------------------------------------------------------------------------
          // Commented out the code which updates the empancipation date with 
          // child's date of death value where avaialble.
          // CQ66150:  Business team advised to remove the DOD value update with
          // Emancipation Date.
          // ----------------------------------------------------------------------------------------------------------------
          local.CsePersonsWorkSet.Number = entities.ChildCsePerson.Number;
          UseSiReadCsePersonBatch();

          if (!IsEmpty(local.Read.Type1) && Equal
            (local.Read.AdabasResponseCd, "0148"))
          {
            // mjr
            // -----------------------------------------------------------
            // Because a resource (normally ADABAS) is unavailable,
            // there is no need to continue the batch.
            // --------------------------------------------------------------
            local.EabReportSend.RptDetail =
              "ABEND:  Resource Unavailable  (usually ADABAS).";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            }

            return;
          }

          if (!IsEmpty(local.Read.Type1) || !IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabReportSend.RptDetail =
              "DATA ERROR:  ADABAS error for child - " + entities
              .ChildCsePerson.Number;
          }
          else
          {
            // Populate the emancipation date with the date of death if the 
            // child is dead. kcole
            if (!Equal(local.CsePersonsWorkSet.Dob, local.NullDateWorkArea.Date))
              
            {
              local.DateChildEmancipates.Date =
                AddYears(local.CsePersonsWorkSet.Dob, 18);

              if (AsChar(entities.ChildCaseRole.Over18AndInSchool) != 'Y' && !
                Equal(entities.ChildCaseRole.DateOfEmancipation,
                local.DateChildEmancipates.Date))
              {
                try
                {
                  UpdateCaseRole();
                  ++local.LcontrolChildUpdated.Count;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "CASE_ROLE_NU";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "CASE_ROLE_PV";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabReportSend.RptDetail = "DATA ERROR:" + TrimEnd
                (local.ExitStateWorkArea.Message) + ";  Child Number - " + entities
                .ChildCsePerson.Number;
            }
          }

          if (!IsEmpty(local.EabReportSend.RptDetail))
          {
            ++local.LcontrolChildErred.Count;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          // -----------------------------------------------------------------------
          // Check for commit
          // -----------------------------------------------------------------------
          if (local.CheckpointRead.Count >= local
            .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
          {
            local.Loop.Flag = "Y";

            break;
          }
        }

        // -----------------------------------------------------------------------
        // Commit processing
        // -----------------------------------------------------------------------
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabReportSend.RptDetail =
            "SYSTEM ERROR:  Unable to commit database.";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          }

          return;
        }

        // -------------------------------------------------------------
        // Reset checkpoint counter
        // -------------------------------------------------------------
        local.CheckpointRead.Count = 0;
        local.LastCommittedCase.Number = entities.ChildCase.Number;
        local.LastCommittedCsePerson.Number = entities.ChildCsePerson.Number;
        local.LastCommittedCaseRole.Identifier =
          entities.ChildCaseRole.Identifier;
      }
      while(AsChar(local.Loop.Flag) != 'N');

      // ---------------------------------------------------------------
      // CLOSE ADABAS IF AVAILABLE
      // ---------------------------------------------------------------
      local.CsePersonsWorkSet.Number = "CLOSE";
      UseEabReadCsePersonBatch();
    }

    local.Document.Name = "EMANCIPA";
    local.EabReportSend.RptDetail = "";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.Infrastructure.ReferenceDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
    local.Infrastructure.CreatedTimestamp = local.Current.Timestamp;
    ExitState = "ACO_NN0000_ALL_OK";

    if (AsChar(local.RunType.Flag) != 'U')
    {
      // --------------------------------------------------------------------
      // Read all cases which are Active. Get the active AR for the
      // case (excluding Foster Care Cases). Then get the child on
      // the case, if he/she is active.
      // --------------------------------------------------------------------
      // mjr
      // ---------------------------------------------
      // 08/02/1999
      // Added check for AR Address
      // Added check for specific ranges for date_of_emancipation
      // Added check for over_18_and_in_school = "Y"
      // ----------------------------------------------------------
      foreach(var item in ReadCaseRoleCsePersonCase1())
      {
        if (AsChar(local.DebugOn.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:  READing Child; Child Number = " + entities
            .ChildCsePerson.Number;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }

        // Dont set document trigger if child is dead. KCole
        if (ReadCsePerson())
        {
          if (Lt(local.NullDateWorkArea.Date, entities.ChildDod.DateOfDeath))
          {
            continue;
          }
        }
        else
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Resource Unavailable for cse person.";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          }

          return;
        }

        // ----------------------------------------------------------------
        // Checkpoint is done against number of reads.
        // ----------------------------------------------------------------
        ++local.CheckpointRead.Count;
        ++local.LcontrolDocsRead.Count;
        local.OutgoingDocument.PrintSucessfulIndicator = "";
        local.EabReportSend.RptDetail = "";
        local.Infrastructure.SystemGeneratedIdentifier = 0;
        ExitState = "ACO_NN0000_ALL_OK";

        if (!ReadCase())
        {
          ++local.LcontrolDocsClosedCase.Count;

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail = "DEBUG:       Case closed";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
          }

          continue;
        }

        if (!ReadCaseRoleCsePerson())
        {
          ++local.LcontrolDocsMissingAr.Count;

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail = "DEBUG:       No AR / AR is Org";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
          }

          continue;
        }

        // mjr
        // ----------------------------------------------------
        // 01/26/1999
        // Check if this child needs a letter generated for it.
        // -----------------------------------------------------------------
        local.CsePersonsWorkSet.Number = entities.ChildCsePerson.Number;

        // mjr
        // ----------------------------------------------------
        // 07/28/2000
        // PR# 100373 - Don't generate the letter for an XA child
        //     (No active person_programs)
        // -----------------------------------------------------------------
        local.Found.Flag = "N";

        if (ReadPersonProgram())
        {
          local.Found.Flag = "Y";
        }

        if (AsChar(local.Found.Flag) == 'N')
        {
          ++local.LcontrolDocsXaChild.Count;

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "DEBUG:       No active programs for CH";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
          }

          continue;
        }

        // mca
        // ----------------------------------------------------
        // 12/11/2001
        //  PR 133483 Changed to call CAB which considers interstate
        //  involvement to determine whether to send the EMANCIPA
        //  or an interstate equivalent to the other state.
        // -----------------------------------------------------------------
        local.FindDoc.Flag = "Y";
        local.SpDocKey.KeyChild = entities.ChildCsePerson.Number;
        local.SpDocKey.KeyCase = entities.Case1.Number;
        UseSpCabDetermineInterstateDoc();

        // ----------------------------------------------------------
        // For critical errors that need to abend the program, set
        // an abort exit state and escape.
        // For non-critical errors you may write an error record to
        // the program error entity type.
        // ----------------------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++local.LcontrolDocsErred.Count;
          UseEabExtractExitStateMessage();

          // mca
          // -----------------------------------------------------
          // Changed if local infrastructure sys gen id is <= 0 to - if exit 
          // state is not = office not found.  With the above change, inf id is
          // not sent back. Office not found is not a reason to abend. This will
          // allow all other exit states to abend.
          // --------------------------------------------------------
          if (!IsExitState("OFFICE_NF"))
          {
            local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
              .ExitStateWorkArea.Message;

            break;
          }

          // mca
          // -----------------------------------------------------
          // Changed message to child number instead of infrastructure ID.
          // --------------------------------------------------------
          local.EabReportSend.RptDetail = "Child Number = " + entities
            .ChildCsePerson.Number + ":  " + local.ExitStateWorkArea.Message;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }
        else if (local.LcabDocsPrevPrints.Count == 0 && local
          .LcabIcDocsCreated.Count == 0 && local.LcabKsDocsCreated.Count == 0)
        {
          ++local.LcontrolDocsForeignOrder.Count;

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "DEBUG:       Only Foreign orders for CH";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
          }
        }
        else
        {
          local.LcontrolDocsPrevPrints.Count += local.LcabDocsPrevPrints.Count;
          local.LcontrolIcDocsCreated.Count += local.LcabIcDocsCreated.Count;
          local.LcontrolKsDocsCreated.Count += local.LcabKsDocsCreated.Count;

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            if (local.LcabDocsPrevPrints.Count > 0)
            {
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.LcabDocsPrevPrints.Count, 15);
              UseEabConvertNumeric1();
              local.EabReportSend.RptDetail = "DEBUG:       " + local
                .EabConvertNumeric.ReturnNoCommasInNonDecimal + " previous documents for Child";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";
            }

            if (local.LcabIcDocsCreated.Count > 0)
            {
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.LcabIcDocsCreated.Count, 15);
              UseEabConvertNumeric1();
              local.EabReportSend.RptDetail = "DEBUG:       " + local
                .EabConvertNumeric.ReturnNoCommasInNonDecimal + " IS TRIGGERs created for Child";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";
            }

            if (local.LcabKsDocsCreated.Count > 0)
            {
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.LcabKsDocsCreated.Count, 15);
              UseEabConvertNumeric1();
              local.EabReportSend.RptDetail = "DEBUG:       " + local
                .EabConvertNumeric.ReturnNoCommasInNonDecimal + " KS TRIGGERs created for Child";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";
            }
          }
        }

        // -----------------------------------------------------------------------
        // Commit processing
        // -----------------------------------------------------------------------
        if (local.CheckpointRead.Count >= local
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
        {
          // ----------------------------------------------------------------
          // Restart info in this program is Case Number and Child
          // number.
          // ----------------------------------------------------------------
          local.ProgramCheckpointRestart.RestartInfo = "CASE:" + entities
            .ChildCase.Number;
          local.WorkArea.Text16 = "CHILD:" + entities.ChildCsePerson.Number;
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + ";" + local
            .WorkArea.Text16;
          local.WorkArea.Text16 = "ROLE:" + NumberToString
            (entities.ChildCaseRole.Identifier, 13, 3);
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + ";" + local
            .WorkArea.Text16;
          local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabReportSend.RptDetail =
              "SYSTEM ERROR:  Unable to update program_checkpoint_restart.";

            break;
          }

          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.EabReportSend.RptDetail =
              "SYSTEM ERROR:  Unable to commit database.";

            break;
          }

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail = "DEBUG:       COMMIT performed";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
          }

          // -------------------------------------------------------------
          // Reset checkpoint counter
          // -------------------------------------------------------------
          local.CheckpointRead.Count = 0;
          local.RestartCase.Number = entities.ChildCase.Number;
          local.RestartCsePerson.Number = entities.ChildCsePerson.Number;
          local.RestartCaseRole.Identifier = entities.ChildCaseRole.Identifier;

          // *****End of Control Total Processing
        }

        // *****End of READ EACH
      }
    }

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      // -----------------------------------------------------------
      // Ending as an ABEND
      // -----------------------------------------------------------
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      // -----------------------------------------------------------
      // Successful end for this program
      // -----------------------------------------------------------
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      UseUpdatePgmCheckpointRestart1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        goto Test;
      }

      // ----------------------------------------------------------------------------------------------------------------
      // CQ66150:  Update Processing date with Last Processing date in Program 
      // Processing Information table for next Run.
      // ----------------------------------------------------------------------------------------------------------------
      if (ReadProgramProcessingInfo())
      {
        try
        {
          UpdateProgramProcessingInfo();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "PROGRAM_PROCESSING_INFO_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "PROGRAM_PROCESSING_INFO_PV";

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
        ExitState = "PROGRAM_PROCESSING_INFO_NU";
      }
    }

Test:

    // -----------------------------------------------------------
    // Write control totals and close reports
    // -----------------------------------------------------------
    UseSpB701WriteControlsAndClose();
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyCase = source.KeyCase;
    target.KeyChild = source.KeyChild;
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

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric2(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
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
    useExport.AbendData.Type1 = local.NullAbendData.Type1;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.NullAbendData.Type1 = useExport.AbendData.Type1;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Read.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSpB701Housekeeping()
  {
    var useImport = new SpB701Housekeeping.Import();
    var useExport = new SpB701Housekeeping.Export();

    Call(SpB701Housekeeping.Execute, useImport, useExport);

    local.RestartCase.Number = useExport.RestartCase.Number;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.Current.Timestamp = useExport.Current.Timestamp;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.RunType.Flag = useExport.RunType.Flag;
    local.RestartCaseRole.Identifier = useExport.RestartCaseRole.Identifier;
    local.RestartCsePerson.Number = useExport.RestartCsePerson.Number;
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    local.End.Date = useExport.EndMonth.Date;
    local.Start.Date = useExport.StartMonth.Date;
  }

  private void UseSpB701WriteControlsAndClose()
  {
    var useImport = new SpB701WriteControlsAndClose.Import();
    var useExport = new SpB701WriteControlsAndClose.Export();

    useImport.DocsRead.Count = local.LcontrolDocsRead.Count;
    useImport.DocsError.Count = local.LcontrolDocsErred.Count;
    useImport.DocsPrevPrints.Count = local.LcontrolDocsPrevPrints.Count;
    useImport.ChildError.Count = local.LcontrolChildErred.Count;
    useImport.ChildUpdated.Count = local.LcontrolChildUpdated.Count;
    useImport.ChildRead.Count = local.LcontrolChildRead.Count;
    useImport.DocsMissingAr.Count = local.LcontrolDocsMissingAr.Count;
    useImport.DocsClosedCases.Count = local.LcontrolDocsClosedCase.Count;
    useImport.DocsForeignOrder.Count = local.LcontrolDocsForeignOrder.Count;
    useImport.IcDocsCreated.Count = local.LcontrolIcDocsCreated.Count;
    useImport.KsDocsCreated.Count = local.LcontrolKsDocsCreated.Count;
    useImport.DocsXaChild.Count = local.LcontrolDocsXaChild.Count;

    Call(SpB701WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSpCabDetermineInterstateDoc()
  {
    var useImport = new SpCabDetermineInterstateDoc.Import();
    var useExport = new SpCabDetermineInterstateDoc.Export();

    useImport.FindDoc.Flag = local.FindDoc.Flag;
    useImport.Infrastructure.ReferenceDate = local.Infrastructure.ReferenceDate;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    useImport.Document.Name = local.Document.Name;

    Call(SpCabDetermineInterstateDoc.Execute, useImport, useExport);

    local.LcabDocsPrevPrints.Count = useExport.ControlDocsPrevPrints.Count;
    local.LcabKsDocsCreated.Count = useExport.KansasDocsCreated.Count;
    local.LcabIcDocsCreated.Count = useExport.InterstateDocsCreated.Count;
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

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ChildCase.Number);
        db.SetNullableDate(
          command, "statusDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 3);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Case1.CreatedBy = db.GetString(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.ArCaseRole.Populated = false;
    entities.ArCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ArCsePerson.Number = db.GetString(reader, 1);
        entities.ArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ArCsePerson.Type1 = db.GetString(reader, 6);
        entities.ArCaseRole.Populated = true;
        entities.ArCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ArCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCase1()
  {
    entities.ChildCase.Populated = false;
    entities.ChildCsePerson.Populated = false;
    entities.ChildCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCase1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetDate(command, "date1", local.Start.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.End.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", local.RestartCase.Number);
        db.SetString(command, "cspNumber", local.RestartCsePerson.Number);
        db.SetInt32(command, "caseRoleId", local.RestartCaseRole.Identifier);
      },
      (db, reader) =>
      {
        entities.ChildCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChildCase.Number = db.GetString(reader, 0);
        entities.ChildCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChildCsePerson.Number = db.GetString(reader, 1);
        entities.ChildCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChildCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChildCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChildCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChildCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 6);
        entities.ChildCaseRole.Over18AndInSchool =
          db.GetNullableString(reader, 7);
        entities.ChildCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.ChildCaseRole.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.ChildCase.Populated = true;
        entities.ChildCsePerson.Populated = true;
        entities.ChildCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChildCaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCase2()
  {
    entities.ChildCase.Populated = false;
    entities.ChildCsePerson.Populated = false;
    entities.ChildCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCase2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.LastCommittedCase.Number);
        db.SetString(command, "cspNumber", local.LastCommittedCsePerson.Number);
        db.SetInt32(
          command, "caseRoleId", local.LastCommittedCaseRole.Identifier);
      },
      (db, reader) =>
      {
        entities.ChildCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChildCase.Number = db.GetString(reader, 0);
        entities.ChildCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChildCsePerson.Number = db.GetString(reader, 1);
        entities.ChildCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChildCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChildCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChildCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChildCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 6);
        entities.ChildCaseRole.Over18AndInSchool =
          db.GetNullableString(reader, 7);
        entities.ChildCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.ChildCaseRole.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.ChildCase.Populated = true;
        entities.ChildCsePerson.Populated = true;
        entities.ChildCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChildCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ChildDod.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ChildDod.Number = db.GetString(reader, 0);
        entities.ChildDod.Type1 = db.GetString(reader, 1);
        entities.ChildDod.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.ChildDod.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChildDod.Type1);
      });
  }

  private bool ReadPersonProgram()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ChildCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", local.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          local.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private void UpdateCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.ChildCaseRole.Populated);

    var dateOfEmancipation = local.DateChildEmancipates.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.ChildCaseRole.Populated = false;
    Update("UpdateCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(command, "emancipationDt", dateOfEmancipation);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "casNumber", entities.ChildCaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.ChildCaseRole.CspNumber);
        db.SetString(command, "type", entities.ChildCaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.ChildCaseRole.Identifier);
      });

    entities.ChildCaseRole.DateOfEmancipation = dateOfEmancipation;
    entities.ChildCaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ChildCaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.ChildCaseRole.Populated = true;
  }

  private void UpdateProgramProcessingInfo()
  {
    var parameterList = local.ProgramProcessingInfo.ParameterList ?? "";

    entities.ProgramProcessingInfo.Populated = false;
    Update("UpdateProgramProcessingInfo",
      (db, command) =>
      {
        db.SetNullableString(command, "parameterList", parameterList);
        db.SetString(command, "name", entities.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ProgramProcessingInfo.ParameterList = parameterList;
    entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of LcontrolDocsForeignOrder.
    /// </summary>
    [JsonPropertyName("lcontrolDocsForeignOrder")]
    public Common LcontrolDocsForeignOrder
    {
      get => lcontrolDocsForeignOrder ??= new();
      set => lcontrolDocsForeignOrder = value;
    }

    /// <summary>
    /// A value of FindDoc.
    /// </summary>
    [JsonPropertyName("findDoc")]
    public Common FindDoc
    {
      get => findDoc ??= new();
      set => findDoc = value;
    }

    /// <summary>
    /// A value of LcontrolIcDocsCreated.
    /// </summary>
    [JsonPropertyName("lcontrolIcDocsCreated")]
    public Common LcontrolIcDocsCreated
    {
      get => lcontrolIcDocsCreated ??= new();
      set => lcontrolIcDocsCreated = value;
    }

    /// <summary>
    /// A value of LcontrolKsDocsCreated.
    /// </summary>
    [JsonPropertyName("lcontrolKsDocsCreated")]
    public Common LcontrolKsDocsCreated
    {
      get => lcontrolKsDocsCreated ??= new();
      set => lcontrolKsDocsCreated = value;
    }

    /// <summary>
    /// A value of LcabDocsPrevPrints.
    /// </summary>
    [JsonPropertyName("lcabDocsPrevPrints")]
    public Common LcabDocsPrevPrints
    {
      get => lcabDocsPrevPrints ??= new();
      set => lcabDocsPrevPrints = value;
    }

    /// <summary>
    /// A value of LcabKsDocsCreated.
    /// </summary>
    [JsonPropertyName("lcabKsDocsCreated")]
    public Common LcabKsDocsCreated
    {
      get => lcabKsDocsCreated ??= new();
      set => lcabKsDocsCreated = value;
    }

    /// <summary>
    /// A value of LcabIcDocsCreated.
    /// </summary>
    [JsonPropertyName("lcabIcDocsCreated")]
    public Common LcabIcDocsCreated
    {
      get => lcabIcDocsCreated ??= new();
      set => lcabIcDocsCreated = value;
    }

    /// <summary>
    /// A value of LcontrolDocsXaChild.
    /// </summary>
    [JsonPropertyName("lcontrolDocsXaChild")]
    public Common LcontrolDocsXaChild
    {
      get => lcontrolDocsXaChild ??= new();
      set => lcontrolDocsXaChild = value;
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
    /// A value of LastCommittedCaseRole.
    /// </summary>
    [JsonPropertyName("lastCommittedCaseRole")]
    public CaseRole LastCommittedCaseRole
    {
      get => lastCommittedCaseRole ??= new();
      set => lastCommittedCaseRole = value;
    }

    /// <summary>
    /// A value of RestartCaseRole.
    /// </summary>
    [JsonPropertyName("restartCaseRole")]
    public CaseRole RestartCaseRole
    {
      get => restartCaseRole ??= new();
      set => restartCaseRole = value;
    }

    /// <summary>
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
    }

    /// <summary>
    /// A value of LastCommittedCsePerson.
    /// </summary>
    [JsonPropertyName("lastCommittedCsePerson")]
    public CsePerson LastCommittedCsePerson
    {
      get => lastCommittedCsePerson ??= new();
      set => lastCommittedCsePerson = value;
    }

    /// <summary>
    /// A value of LcontrolDocsMissingAr.
    /// </summary>
    [JsonPropertyName("lcontrolDocsMissingAr")]
    public Common LcontrolDocsMissingAr
    {
      get => lcontrolDocsMissingAr ??= new();
      set => lcontrolDocsMissingAr = value;
    }

    /// <summary>
    /// A value of LcontrolDocsClosedCase.
    /// </summary>
    [JsonPropertyName("lcontrolDocsClosedCase")]
    public Common LcontrolDocsClosedCase
    {
      get => lcontrolDocsClosedCase ??= new();
      set => lcontrolDocsClosedCase = value;
    }

    /// <summary>
    /// A value of LastCommittedCase.
    /// </summary>
    [JsonPropertyName("lastCommittedCase")]
    public Case1 LastCommittedCase
    {
      get => lastCommittedCase ??= new();
      set => lastCommittedCase = value;
    }

    /// <summary>
    /// A value of Loop.
    /// </summary>
    [JsonPropertyName("loop")]
    public Common Loop
    {
      get => loop ??= new();
      set => loop = value;
    }

    /// <summary>
    /// A value of LcontrolDocsPrevPrints.
    /// </summary>
    [JsonPropertyName("lcontrolDocsPrevPrints")]
    public Common LcontrolDocsPrevPrints
    {
      get => lcontrolDocsPrevPrints ??= new();
      set => lcontrolDocsPrevPrints = value;
    }

    /// <summary>
    /// A value of LcontrolChildErred.
    /// </summary>
    [JsonPropertyName("lcontrolChildErred")]
    public Common LcontrolChildErred
    {
      get => lcontrolChildErred ??= new();
      set => lcontrolChildErred = value;
    }

    /// <summary>
    /// A value of LcontrolChildUpdated.
    /// </summary>
    [JsonPropertyName("lcontrolChildUpdated")]
    public Common LcontrolChildUpdated
    {
      get => lcontrolChildUpdated ??= new();
      set => lcontrolChildUpdated = value;
    }

    /// <summary>
    /// A value of LcontrolChildRead.
    /// </summary>
    [JsonPropertyName("lcontrolChildRead")]
    public Common LcontrolChildRead
    {
      get => lcontrolChildRead ??= new();
      set => lcontrolChildRead = value;
    }

    /// <summary>
    /// A value of RunType.
    /// </summary>
    [JsonPropertyName("runType")]
    public Common RunType
    {
      get => runType ??= new();
      set => runType = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of DateChildEmancipates.
    /// </summary>
    [JsonPropertyName("dateChildEmancipates")]
    public DateWorkArea DateChildEmancipates
    {
      get => dateChildEmancipates ??= new();
      set => dateChildEmancipates = value;
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
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public AbendData Read
    {
      get => read ??= new();
      set => read = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of RestartCase.
    /// </summary>
    [JsonPropertyName("restartCase")]
    public Case1 RestartCase
    {
      get => restartCase ??= new();
      set => restartCase = value;
    }

    /// <summary>
    /// A value of LcontrolDocsErred.
    /// </summary>
    [JsonPropertyName("lcontrolDocsErred")]
    public Common LcontrolDocsErred
    {
      get => lcontrolDocsErred ??= new();
      set => lcontrolDocsErred = value;
    }

    /// <summary>
    /// A value of LcontrolDocsCreated.
    /// </summary>
    [JsonPropertyName("lcontrolDocsCreated")]
    public Common LcontrolDocsCreated
    {
      get => lcontrolDocsCreated ??= new();
      set => lcontrolDocsCreated = value;
    }

    /// <summary>
    /// A value of LcontrolDocsRead.
    /// </summary>
    [JsonPropertyName("lcontrolDocsRead")]
    public Common LcontrolDocsRead
    {
      get => lcontrolDocsRead ??= new();
      set => lcontrolDocsRead = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of CheckpointRead.
    /// </summary>
    [JsonPropertyName("checkpointRead")]
    public Common CheckpointRead
    {
      get => checkpointRead ??= new();
      set => checkpointRead = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NullAbendData.
    /// </summary>
    [JsonPropertyName("nullAbendData")]
    public AbendData NullAbendData
    {
      get => nullAbendData ??= new();
      set => nullAbendData = value;
    }

    /// <summary>
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    private Common lcontrolDocsForeignOrder;
    private Common findDoc;
    private Common lcontrolIcDocsCreated;
    private Common lcontrolKsDocsCreated;
    private Common lcabDocsPrevPrints;
    private Common lcabKsDocsCreated;
    private Common lcabIcDocsCreated;
    private Common lcontrolDocsXaChild;
    private Common found;
    private CaseRole lastCommittedCaseRole;
    private CaseRole restartCaseRole;
    private CsePerson restartCsePerson;
    private CsePerson lastCommittedCsePerson;
    private Common lcontrolDocsMissingAr;
    private Common lcontrolDocsClosedCase;
    private Case1 lastCommittedCase;
    private Common loop;
    private Common lcontrolDocsPrevPrints;
    private Common lcontrolChildErred;
    private Common lcontrolChildUpdated;
    private Common lcontrolChildRead;
    private Common runType;
    private OutgoingDocument outgoingDocument;
    private WorkArea workArea;
    private DateWorkArea dateChildEmancipates;
    private DateWorkArea current;
    private AbendData read;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Infrastructure infrastructure;
    private SpDocKey spDocKey;
    private DateWorkArea end;
    private DateWorkArea start;
    private Case1 restartCase;
    private Common lcontrolDocsErred;
    private Common lcontrolDocsCreated;
    private Common lcontrolDocsRead;
    private DateWorkArea nullDateWorkArea;
    private Document document;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private Common checkpointRead;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private AbendData nullAbendData;
    private Common debugOn;
    private EabConvertNumeric2 eabConvertNumeric;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChildDod.
    /// </summary>
    [JsonPropertyName("childDod")]
    public CsePerson ChildDod
    {
      get => childDod ??= new();
      set => childDod = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of ChildCase.
    /// </summary>
    [JsonPropertyName("childCase")]
    public Case1 ChildCase
    {
      get => childCase ??= new();
      set => childCase = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private CsePerson childDod;
    private PersonProgram personProgram;
    private Case1 childCase;
    private CaseRole arCaseRole;
    private CsePerson arCsePerson;
    private CsePerson childCsePerson;
    private CaseRole childCaseRole;
    private CsePersonAddress csePersonAddress;
    private Case1 case1;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
