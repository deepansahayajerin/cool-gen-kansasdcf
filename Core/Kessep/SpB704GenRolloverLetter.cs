// Program: SP_B704_GEN_ROLLOVER_LETTER, ID: 370987676, model: 746.
// Short name: SWEP704B
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
/// A program: SP_B704_GEN_ROLLOVER_LETTER.
/// </para>
/// <para>
/// Input : DB2
/// Output : DB2
/// External Commit.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB704GenRolloverLetter: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B704_GEN_ROLLOVER_LETTER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB704GenRolloverLetter(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB704GenRolloverLetter.
  /// </summary>
  public SpB704GenRolloverLetter(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------
    //  Date		Developer	Request #	Description
    // ------------------------------------------------------------------------
    // 09/06/2000	M Ramirez	96630 Seg B	Initial Development
    // 01/23/2001	M Ramirez	111614		Ignore Person_programs related
    // 						to emancipated children
    // 06/19/2001	M Ramirez	116405		Remove the check for the
    // 						AR's address
    // 07/25/2001	M Ramirez	WR 10559	Rework to accommodate new
    // 						MA subprogram CM
    // ------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB704Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Current.Date = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.RptDetail = "";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.Infrastructure.ReferenceDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
    local.Infrastructure.CreatedTimestamp = local.Current.Timestamp;

    // --------------------------------------------------------------------
    // Read all Cases that have a Person that has a person program that has
    // ended within the given timeframe.
    // READ EACH is set to Distinct so we'll process all person_programs
    // for any people on that case before doing a commit
    // --------------------------------------------------------------------
    foreach(var item in ReadCase())
    {
      if (AsChar(local.DebugOn.Flag) != 'N')
      {
        local.EabReportSend.RptDetail =
          "DEBUG:  READing Case; Case Number = " + entities.Case1.Number;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
      }

      // ----------------------------------------------------------------
      // Checkpoint is done against number of reads.
      // ----------------------------------------------------------------
      ++local.CheckpointRead.Count;
      ++local.LcontrolCasesRead.Count;
      local.EabReportSend.RptDetail = "";
      local.Infrastructure.SystemGeneratedIdentifier = 0;
      local.Document.Name = "";
      ExitState = "ACO_NN0000_ALL_OK";

      // mjr
      // -------------------------------------------------------
      // AR must not be an Organization and must have an address
      // ----------------------------------------------------------
      // mjr
      // ---------------------------------------------
      // 06/19/2001
      // PR# 116405 - Removed check for AR's address
      // ----------------------------------------------------------
      if (!ReadCaseRoleCsePerson())
      {
        ++local.LcontrolMissingAr.Count;

        if (AsChar(local.DebugOn.Flag) != 'N')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       No AR/AR is Org for Case.";
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
      // --------------------------------------
      // 07/25/2001
      // WR# 10559 - Rework to accommodate new MA subprogram CM
      // Removed AR programs from following READEACH
      // ---------------------------------------------------
      // mjr
      // --------------------------------------
      // 09/24/2002
      // WR# 10559 - Rework to accommodate new MA subprogram CM
      // Changed to use user defined list of compliance programs,
      // rather than hardcoded
      // ---------------------------------------------------
      local.Found.Flag = "N";
      local.Group.Index = -1;
      local.Group.Count = 0;

      foreach(var item1 in ReadPersonProgramProgramCsePerson())
      {
        if (AsChar(local.DebugOn.Flag) == 'V')
        {
          // mjr
          // ------------------------------------------------
          // Verbose Debug - print out each program
          // ---------------------------------------------------
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.Program.SystemGeneratedIdentifier, 15);
          UseEabConvertNumeric1();
          local.EabReportSend.RptDetail = "DEBUG:       Child = " + entities
            .CsePerson.Number + "; Program = " + TrimEnd
            (local.EabConvertNumeric.ReturnNoCommasInNonDecimal);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }

        // mjr
        // ------------------------------------------------
        // Ignore old programs
        // ---------------------------------------------------
        if (Lt(entities.PersonProgram.DiscontinueDate, local.Start.Date))
        {
          if (AsChar(local.DebugOn.Flag) == 'V')
          {
            local.EabReportSend.RptDetail = "DEBUG:            Old program";
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
        // ------------------------------------------------
        // Ignore future programs
        // ---------------------------------------------------
        if (Lt(local.ProgramProcessingInfo.ProcessDate,
          entities.PersonProgram.EffectiveDate))
        {
          if (AsChar(local.DebugOn.Flag) == 'V')
          {
            local.EabReportSend.RptDetail = "DEBUG:            Future program";
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
        // ------------------------------------------------
        // Ignore programs that are not open for a full day
        // ---------------------------------------------------
        if (Equal(entities.PersonProgram.EffectiveDate,
          entities.PersonProgram.DiscontinueDate))
        {
          if (AsChar(local.DebugOn.Flag) == 'V')
          {
            local.EabReportSend.RptDetail =
              "DEBUG:            Program not open for full day";
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
        // ------------------------------------------------
        // Ignore all programs except compliance programs
        // ---------------------------------------------------
        local.ComplianceProgramFound.Flag = "N";
        local.LiteralGroupCompliance.Index = 0;

        for(var limit = local.LiteralGroupCompliance.Count; local
          .LiteralGroupCompliance.Index < limit; ++
          local.LiteralGroupCompliance.Index)
        {
          if (!local.LiteralGroupCompliance.CheckSize())
          {
            break;
          }

          if (entities.Program.SystemGeneratedIdentifier == local
            .LiteralGroupCompliance.Item.GliteralCompliance.
              SystemGeneratedIdentifier)
          {
            if (local.LiteralGroupCompliance.Item.LiteralSubGroupCompliance.
              Count == 0)
            {
              // mjr
              // ------------------------------------------------
              // This is a compliance program and all subprograms
              // for that type of program are considered compliance
              // subprograms
              // ---------------------------------------------------
              local.ComplianceProgramFound.Flag = "Y";

              break;
            }
            else
            {
              // mjr
              // ------------------------------------------------
              // This is a compliance program.
              // Check whether the subprogram is a compliance subprogram
              // ---------------------------------------------------
              UseSiEabReadMaSubprogType2();

              for(local.LiteralGroupCompliance.Item.LiteralSubGroupCompliance.
                Index = 0; local
                .LiteralGroupCompliance.Item.LiteralSubGroupCompliance.Index < local
                .LiteralGroupCompliance.Item.LiteralSubGroupCompliance.Count; ++
                local.LiteralGroupCompliance.Item.LiteralSubGroupCompliance.
                  Index)
              {
                if (!local.LiteralGroupCompliance.Item.
                  LiteralSubGroupCompliance.CheckSize())
                {
                  break;
                }

                if (Equal(local.EndOfPeriod.MedType,
                  local.LiteralGroupCompliance.Item.LiteralSubGroupCompliance.
                    Item.GliteralComplianceSubprogram.MedType))
                {
                  // mjr
                  // ------------------------------------------------
                  // This is a compliance subprogram.
                  // ---------------------------------------------------
                  local.ComplianceProgramFound.Flag = "Y";

                  goto AfterCycle;
                }
              }

              local.LiteralGroupCompliance.Item.LiteralSubGroupCompliance.
                CheckIndex();

              // mjr
              // ------------------------------------------------
              // The subprogram is not a compliance subprogram.
              // Check whether last month was a compliance subprogram
              // ---------------------------------------------------
              UseSiEabReadMaSubprogType3();

              for(local.LiteralGroupCompliance.Item.LiteralSubGroupCompliance.
                Index = 0; local
                .LiteralGroupCompliance.Item.LiteralSubGroupCompliance.Index < local
                .LiteralGroupCompliance.Item.LiteralSubGroupCompliance.Count; ++
                local.LiteralGroupCompliance.Item.LiteralSubGroupCompliance.
                  Index)
              {
                if (!local.LiteralGroupCompliance.Item.
                  LiteralSubGroupCompliance.CheckSize())
                {
                  break;
                }

                if (Equal(local.StartOfPeriod.MedType,
                  local.LiteralGroupCompliance.Item.LiteralSubGroupCompliance.
                    Item.GliteralComplianceSubprogram.MedType))
                {
                  // mjr
                  // ------------------------------------------------
                  // Last month was a compliance subprogram.
                  // Special situation:  If the end of the period was an non-
                  // compliance subprogram (as opposed to a <blank>, which could
                  // represent a not-yet-activated compliance subprogram) and
                  // the program is still active, but the previous period was a
                  // compliance subprogram, then the compliance subprogram
                  // should be considered ended.  To accomplish this, set the
                  // discontinue date to the current date, which signifies an
                  // ending compliance subprogram.
                  // For example:  MA-CM is a compliance subprogram, and MA-WT 
                  // is not.  The program is MA, which has not ended.  The
                  // current subprogram is WT, which is a non-compliance
                  // subprogram.  The previous subprogram was a CM, which is a
                  // compliance subprogram.  This should mark the end of a
                  // compliance subprogram, but the discontinue date of the
                  // program shows still active (the MA is still active).  So
                  // add the program/subprogram combination to the group as an
                  // ended compliance program.
                  // ---------------------------------------------------
                  if (!IsEmpty(local.EndOfPeriod.MedType) && Lt
                    (local.Current.Date, entities.PersonProgram.DiscontinueDate))
                    
                  {
                    local.EndOfPeriod.DiscontinueDate = local.Current.Date;
                  }

                  local.ComplianceProgramFound.Flag = "Y";

                  goto AfterCycle;
                }
              }

              local.LiteralGroupCompliance.Item.LiteralSubGroupCompliance.
                CheckIndex();

              // mjr
              // ------------------------------------------------
              // The subprogram and the prior period subprogram are
              // not compliance subprograms.
              // Get the next program
              // ---------------------------------------------------
              if (AsChar(local.DebugOn.Flag) == 'V')
              {
                local.EabReportSend.RptDetail =
                  "DEBUG:            Compliance program, but current and previous subprograms are non-compliance; Current = " +
                  (local.EndOfPeriod.MedType ?? "") + "; Previous = " + (
                    local.StartOfPeriod.MedType ?? "");
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                local.EabReportSend.RptDetail = "";
              }

              goto ReadEach1;
            }
          }
          else
          {
            // mjr
            // ------------------------------------------------
            // The program does not match this compliance program, but
            // maybe it will match the next one.
            // ---------------------------------------------------
          }
        }

AfterCycle:

        local.LiteralGroupCompliance.CheckIndex();

        if (AsChar(local.ComplianceProgramFound.Flag) == 'N')
        {
          if (AsChar(local.DebugOn.Flag) == 'V')
          {
            local.EabReportSend.RptDetail =
              "DEBUG:            Program is non compliance";
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
        // ------------------------------------------------
        // 01/23/2001
        // PR# 111614 - Ignore Person_programs related to emancipated
        // children
        // -------------------------------------------------------------
        if (ReadCaseRole())
        {
          if (Lt(local.Null1.Date, entities.CaseRole.DateOfEmancipation) && Lt
            (entities.CaseRole.DateOfEmancipation, local.Start.Date))
          {
            if (AsChar(local.DebugOn.Flag) == 'V')
            {
              local.EabReportSend.RptDetail =
                "DEBUG:            Program related to emancipated child";
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
        }

        if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
        {
          local.EabReportSend.RptDetail =
            "ABEND:       Group view overflow for Case; Case = " + entities
            .Case1.Number;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          goto ReadEach2;
        }

        local.Found.Flag = "Y";

        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.Gprogram.SystemGeneratedIdentifier =
          entities.Program.SystemGeneratedIdentifier;
        local.Group.Update.GpersonProgram.Assign(entities.PersonProgram);

        // mjr
        // ------------------------------------------------
        // This is only set when the program is still active
        // but the compliance subprogram ended since
        // the last run.  It marks the end of a compliance
        // subprogram.
        // ---------------------------------------------------
        if (Lt(local.Null1.Date, local.EndOfPeriod.DiscontinueDate))
        {
          local.Group.Update.GpersonProgram.DiscontinueDate =
            local.EndOfPeriod.DiscontinueDate;
          local.EndOfPeriod.DiscontinueDate = local.Null1.Date;
        }

        if (AsChar(local.DebugOn.Flag) == 'V')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:            Program will factor into sending document";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }

ReadEach1:
        ;
      }

      if (AsChar(local.Found.Flag) == 'N')
      {
        // mjr
        // --------------------------------------------------------------
        // None of the programs factor into producing a document
        // -----------------------------------------------------------------
        ++local.LcontrolNoActiveCompliance.Count;

        if (AsChar(local.DebugOn.Flag) != 'N')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       No active compliance programs";
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
      // -------------------------------------------------------
      // If the AE Case closes with a reason code of DC then don't issue a 
      // letter
      // ----------------------------------------------------------
      local.Found.Flag = "N";

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        if (!Lt(local.Group.Item.GpersonProgram.DiscontinueDate,
          local.Start.Date) && !
          Lt(local.End.Date, local.Group.Item.GpersonProgram.DiscontinueDate) &&
          Equal(local.Group.Item.GpersonProgram.ClosureReason, "DC"))
        {
          local.Found.Flag = "Y";

          break;
        }
      }

      local.Group.CheckIndex();

      if (AsChar(local.Found.Flag) == 'Y')
      {
        // mjr
        // --------------------------------------------------------------
        // At least one person program closed DC.  Don't send a letter.
        // -----------------------------------------------------------------
        ++local.LcontrolClosedDc.Count;

        if (AsChar(local.DebugOn.Flag) != 'N')
        {
          local.EabReportSend.RptDetail = "DEBUG:       AE Case closed DC.";
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
      // --------------------------------------------------------------
      // Check for Case that closed MO during this month
      // -----------------------------------------------------------------
      local.Found.Flag = "N";

      if (ReadInfrastructure())
      {
        local.Found.Flag = "Y";
      }

      if (AsChar(local.Found.Flag) == 'Y')
      {
        ++local.LcontrolClosedMo.Count;

        if (AsChar(local.DebugOn.Flag) != 'N')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       CSE Case closed Medical Only.";
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
      // --------------------------------------------------------------
      // Check for any active programs
      // -----------------------------------------------------------------
      local.Found.Flag = "N";

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        if (Lt(local.ProgramProcessingInfo.ProcessDate,
          local.Group.Item.GpersonProgram.DiscontinueDate))
        {
          local.Found.Flag = "Y";

          break;
        }
      }

      local.Group.CheckIndex();

      if (AsChar(local.Found.Flag) == 'N')
      {
        // mjr
        // --------------------------------------------------------------
        // No active programs were found.  Send PAROLLO
        // -----------------------------------------------------------------
        local.Document.Name = "PAROLLO";
      }
      else
      {
        continue;
      }

      // mjr
      // --------------------------------------------------------------
      // Create document_trigger
      // -----------------------------------------------------------------
      local.SpDocKey.KeyCase = entities.Case1.Number;
      UseSpCreateDocumentInfrastruct();

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

        if (local.Infrastructure.SystemGeneratedIdentifier <= 0)
        {
          // mjr
          // -----------------------------------------------------
          // Errors that occur before an infrastructure record is
          // created, create an ABEND.
          // --------------------------------------------------------
          local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + TrimEnd
            (local.ExitStateWorkArea.Message) + ";  CSE Case = " + local
            .SpDocKey.KeyCase;

          break;
        }

        local.EabConvertNumeric.SendAmount =
          NumberToString(local.Infrastructure.SystemGeneratedIdentifier, 15);
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
          (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":  " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        ExitState = "ACO_NN0000_ALL_OK";
      }
      else
      {
        ++local.LcontrolDocsCreated.Count;

        if (Equal(local.Document.Name, "ADCROLLO"))
        {
          ++local.LcontrolCreatedAdcrollo.Count;
        }
        else
        {
          ++local.LcontrolCreatedParollo.Count;
        }

        if (AsChar(local.DebugOn.Flag) != 'N')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       TRIGGER created for Case; Document = " + local
            .Document.Name;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
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
        local.ProgramCheckpointRestart.RestartInfo = "LAST_PROCESS:" + local
          .LastProcess.TextDate + ";CASE:" + entities.Case1.Number;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        UseUpdatePgmCheckpointRestart2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabReportSend.RptDetail =
            "SYSTEM ERROR:  Unable to update program_checkpoint_restart.";

          break;
        }

        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          local.EabReportSend.RptDetail =
            "SYSTEM ERROR:  Unable to commit database.";

          break;
        }

        // -------------------------------------------------------------
        // Reset checkpoint counter
        // -------------------------------------------------------------
        local.CheckpointRead.Count = 0;

        // *****End of Control Total Processing
        if (AsChar(local.DebugOn.Flag) != 'N')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:  COMMIT performed after Case; Case Number = " + entities
            .Case1.Number;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }
      }

      // *****End of READ EACH
    }

ReadEach2:

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

      ExitState = "ACO_NN0000_ABEND_4_BATCH";
    }
    else
    {
      local.Close.Number = "CLOSE";
      UseSiEabReadMaSubprogType4();
      UseSiEabReadMaSubprogType1();

      // -----------------------------------------------------------
      // Successful end for this program
      // -----------------------------------------------------------
      local.ProgramCheckpointRestart.RestartInfo = "LAST_PROCESS:" + local
        .End.TextDate;
      local.ProgramCheckpointRestart.RestartInd = "N";
      UseUpdatePgmCheckpointRestart1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Unable to update Program_Checkpoint_Restart";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        goto Test;
      }

      UseExtToDoACommit();

      if (local.External.NumericReturnCode != 0)
      {
        local.EabReportSend.RptDetail = "ABEND:  Unable to commit database.";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        ExitState = "ACO_NN0000_ABEND_4_BATCH";
      }
    }

Test:

    // -----------------------------------------------------------
    // Write control totals and close reports
    // -----------------------------------------------------------
    UseSpB704WriteControlsAndClose();
  }

  private static void MoveCompliance(SpB704Housekeeping.Export.
    ComplianceGroup source, Local.LiteralGroupComplianceGroup target)
  {
    MoveProgram(source.GexportCompliance, target.GliteralCompliance);
    source.ComplianceSub.CopyTo(
      target.LiteralSubGroupCompliance,
      MoveComplianceSubToLiteralSubGroupCompliance);
  }

  private static void MoveComplianceSubToLiteralSubGroupCompliance(
    SpB704Housekeeping.Export.ComplianceSubGroup source,
    Local.LiteralSubGroupComplianceGroup target)
  {
    target.GliteralComplianceSubprogram.MedType =
      source.GexportComplianceSub.MedType;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiEabReadMaSubprogType1()
  {
    var useImport = new SiEabReadMaSubprogType.Import();
    var useExport = new SiEabReadMaSubprogType.Export();

    Call(SiEabReadMaSubprogType.Execute, useImport, useExport);
  }

  private void UseSiEabReadMaSubprogType2()
  {
    var useImport = new SiEabReadMaSubprogType.Import();
    var useExport = new SiEabReadMaSubprogType.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Program.Code =
      local.LiteralGroupCompliance.Item.GliteralCompliance.Code;
    useImport.Processing.Date = local.Current.Date;
    useExport.PersonProgram.MedType = local.EndOfPeriod.MedType;

    Call(SiEabReadMaSubprogType.Execute, useImport, useExport);

    local.EndOfPeriod.MedType = useExport.PersonProgram.MedType;
  }

  private void UseSiEabReadMaSubprogType3()
  {
    var useImport = new SiEabReadMaSubprogType.Import();
    var useExport = new SiEabReadMaSubprogType.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Program.Code =
      local.LiteralGroupCompliance.Item.GliteralCompliance.Code;
    useImport.Processing.Date = local.Start.Date;
    useExport.PersonProgram.MedType = local.StartOfPeriod.MedType;

    Call(SiEabReadMaSubprogType.Execute, useImport, useExport);

    local.StartOfPeriod.MedType = useExport.PersonProgram.MedType;
  }

  private void UseSiEabReadMaSubprogType4()
  {
    var useImport = new SiEabReadMaSubprogType.Import();
    var useExport = new SiEabReadMaSubprogType.Export();

    useImport.CsePerson.Number = local.Close.Number;

    Call(SiEabReadMaSubprogType.Execute, useImport, useExport);
  }

  private void UseSpB704Housekeeping()
  {
    var useImport = new SpB704Housekeeping.Import();
    var useExport = new SpB704Housekeeping.Export();

    Call(SpB704Housekeeping.Execute, useImport, useExport);

    local.LastProcess.TextDate = useExport.LastProcess.TextDate;
    local.Current.Timestamp = useExport.Current.Timestamp;
    local.End.Assign(useExport.End);
    MoveDateWorkArea(useExport.Start, local.Start);
    local.Restart.Number = useExport.Restart.Number;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    useExport.Compliance.CopyTo(local.LiteralGroupCompliance, MoveCompliance);
    local.LiteralAf.SystemGeneratedIdentifier =
      useExport.Af.SystemGeneratedIdentifier;
  }

  private void UseSpB704WriteControlsAndClose()
  {
    var useImport = new SpB704WriteControlsAndClose.Import();
    var useExport = new SpB704WriteControlsAndClose.Export();

    useImport.ClosedMo.Count = local.LcontrolClosedMo.Count;
    useImport.DocsError.Count = local.LcontrolDocsErred.Count;
    useImport.DocsCreated.Count = local.LcontrolDocsCreated.Count;
    useImport.CasesRead.Count = local.LcontrolCasesRead.Count;
    useImport.ClosedDc.Count = local.LcontrolClosedDc.Count;
    useImport.NoActiveCompliance.Count = local.LcontrolNoActiveCompliance.Count;
    useImport.MissingAr.Count = local.LcontrolMissingAr.Count;
    useImport.CreatedParollo.Count = local.LcontrolCreatedParollo.Count;
    useImport.CreatedAdcrollo.Count = local.LcontrolCreatedAdcrollo.Count;
    useImport.ActiveAfProg.Count = local.LcontrolActiveAfProg.Count;

    Call(SpB704WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.SpDocKey.KeyCase = local.SpDocKey.KeyCase;
    useImport.Document.Name = local.Document.Name;

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
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

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "statusDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "numb", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.PersonProgram.Populated);
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.PersonProgram.CspNumber);
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
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

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetDateTime(
          command, "timestamp1", local.Start.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.End.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 2);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 3);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgramCsePerson()
  {
    entities.Program.Populated = false;
    entities.CsePerson.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgramCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 7);
        entities.Program.Populated = true;
        entities.CsePerson.Populated = true;
        entities.PersonProgram.Populated = true;

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
    /// <summary>A LiteralGroupComplianceGroup group.</summary>
    [Serializable]
    public class LiteralGroupComplianceGroup
    {
      /// <summary>
      /// A value of GliteralCompliance.
      /// </summary>
      [JsonPropertyName("gliteralCompliance")]
      public Program GliteralCompliance
      {
        get => gliteralCompliance ??= new();
        set => gliteralCompliance = value;
      }

      /// <summary>
      /// Gets a value of LiteralSubGroupCompliance.
      /// </summary>
      [JsonIgnore]
      public Array<LiteralSubGroupComplianceGroup> LiteralSubGroupCompliance =>
        literalSubGroupCompliance ??= new(LiteralSubGroupComplianceGroup.
          Capacity, 0);

      /// <summary>
      /// Gets a value of LiteralSubGroupCompliance for json serialization.
      /// </summary>
      [JsonPropertyName("literalSubGroupCompliance")]
      [Computed]
      public IList<LiteralSubGroupComplianceGroup>
        LiteralSubGroupCompliance_Json
      {
        get => literalSubGroupCompliance;
        set => LiteralSubGroupCompliance.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program gliteralCompliance;
      private Array<LiteralSubGroupComplianceGroup> literalSubGroupCompliance;
    }

    /// <summary>A LiteralSubGroupComplianceGroup group.</summary>
    [Serializable]
    public class LiteralSubGroupComplianceGroup
    {
      /// <summary>
      /// A value of GliteralComplianceSubprogram.
      /// </summary>
      [JsonPropertyName("gliteralComplianceSubprogram")]
      public PersonProgram GliteralComplianceSubprogram
      {
        get => gliteralComplianceSubprogram ??= new();
        set => gliteralComplianceSubprogram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private PersonProgram gliteralComplianceSubprogram;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Gprogram.
      /// </summary>
      [JsonPropertyName("gprogram")]
      public Program Gprogram
      {
        get => gprogram ??= new();
        set => gprogram = value;
      }

      /// <summary>
      /// A value of GpersonProgram.
      /// </summary>
      [JsonPropertyName("gpersonProgram")]
      public PersonProgram GpersonProgram
      {
        get => gpersonProgram ??= new();
        set => gpersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private Program gprogram;
      private PersonProgram gpersonProgram;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public CsePerson Close
    {
      get => close ??= new();
      set => close = value;
    }

    /// <summary>
    /// A value of ComplianceProgramFound.
    /// </summary>
    [JsonPropertyName("complianceProgramFound")]
    public Common ComplianceProgramFound
    {
      get => complianceProgramFound ??= new();
      set => complianceProgramFound = value;
    }

    /// <summary>
    /// Gets a value of LiteralGroupCompliance.
    /// </summary>
    [JsonIgnore]
    public Array<LiteralGroupComplianceGroup> LiteralGroupCompliance =>
      literalGroupCompliance ??= new(LiteralGroupComplianceGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LiteralGroupCompliance for json serialization.
    /// </summary>
    [JsonPropertyName("literalGroupCompliance")]
    [Computed]
    public IList<LiteralGroupComplianceGroup> LiteralGroupCompliance_Json
    {
      get => literalGroupCompliance;
      set => LiteralGroupCompliance.Assign(value);
    }

    /// <summary>
    /// A value of EndOfPeriod.
    /// </summary>
    [JsonPropertyName("endOfPeriod")]
    public PersonProgram EndOfPeriod
    {
      get => endOfPeriod ??= new();
      set => endOfPeriod = value;
    }

    /// <summary>
    /// A value of StartOfPeriod.
    /// </summary>
    [JsonPropertyName("startOfPeriod")]
    public PersonProgram StartOfPeriod
    {
      get => startOfPeriod ??= new();
      set => startOfPeriod = value;
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
    /// A value of LiteralAf.
    /// </summary>
    [JsonPropertyName("literalAf")]
    public Program LiteralAf
    {
      get => literalAf ??= new();
      set => literalAf = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of LastProcess.
    /// </summary>
    [JsonPropertyName("lastProcess")]
    public DateWorkArea LastProcess
    {
      get => lastProcess ??= new();
      set => lastProcess = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of LcontrolCasesRead.
    /// </summary>
    [JsonPropertyName("lcontrolCasesRead")]
    public Common LcontrolCasesRead
    {
      get => lcontrolCasesRead ??= new();
      set => lcontrolCasesRead = value;
    }

    /// <summary>
    /// A value of LcontrolClosedDc.
    /// </summary>
    [JsonPropertyName("lcontrolClosedDc")]
    public Common LcontrolClosedDc
    {
      get => lcontrolClosedDc ??= new();
      set => lcontrolClosedDc = value;
    }

    /// <summary>
    /// A value of LcontrolClosedMo.
    /// </summary>
    [JsonPropertyName("lcontrolClosedMo")]
    public Common LcontrolClosedMo
    {
      get => lcontrolClosedMo ??= new();
      set => lcontrolClosedMo = value;
    }

    /// <summary>
    /// A value of LcontrolNoActiveCompliance.
    /// </summary>
    [JsonPropertyName("lcontrolNoActiveCompliance")]
    public Common LcontrolNoActiveCompliance
    {
      get => lcontrolNoActiveCompliance ??= new();
      set => lcontrolNoActiveCompliance = value;
    }

    /// <summary>
    /// A value of LcontrolMissingAr.
    /// </summary>
    [JsonPropertyName("lcontrolMissingAr")]
    public Common LcontrolMissingAr
    {
      get => lcontrolMissingAr ??= new();
      set => lcontrolMissingAr = value;
    }

    /// <summary>
    /// A value of LcontrolCreatedParollo.
    /// </summary>
    [JsonPropertyName("lcontrolCreatedParollo")]
    public Common LcontrolCreatedParollo
    {
      get => lcontrolCreatedParollo ??= new();
      set => lcontrolCreatedParollo = value;
    }

    /// <summary>
    /// A value of LcontrolNoActiveProg.
    /// </summary>
    [JsonPropertyName("lcontrolNoActiveProg")]
    public Common LcontrolNoActiveProg
    {
      get => lcontrolNoActiveProg ??= new();
      set => lcontrolNoActiveProg = value;
    }

    /// <summary>
    /// A value of LcontrolCreatedAdcrollo.
    /// </summary>
    [JsonPropertyName("lcontrolCreatedAdcrollo")]
    public Common LcontrolCreatedAdcrollo
    {
      get => lcontrolCreatedAdcrollo ??= new();
      set => lcontrolCreatedAdcrollo = value;
    }

    /// <summary>
    /// A value of LcontrolActiveAfProg.
    /// </summary>
    [JsonPropertyName("lcontrolActiveAfProg")]
    public Common LcontrolActiveAfProg
    {
      get => lcontrolActiveAfProg ??= new();
      set => lcontrolActiveAfProg = value;
    }

    private CsePerson close;
    private Common complianceProgramFound;
    private Array<LiteralGroupComplianceGroup> literalGroupCompliance;
    private PersonProgram endOfPeriod;
    private PersonProgram startOfPeriod;
    private DateWorkArea null1;
    private Program literalAf;
    private Array<GroupGroup> group;
    private DateWorkArea lastProcess;
    private Common found;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private SpDocKey spDocKey;
    private DateWorkArea end;
    private DateWorkArea start;
    private Case1 restart;
    private Document document;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External external;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common debugOn;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common checkpointRead;
    private Common lcontrolDocsErred;
    private Common lcontrolDocsCreated;
    private Common lcontrolCasesRead;
    private Common lcontrolClosedDc;
    private Common lcontrolClosedMo;
    private Common lcontrolNoActiveCompliance;
    private Common lcontrolMissingAr;
    private Common lcontrolCreatedParollo;
    private Common lcontrolNoActiveProg;
    private Common lcontrolCreatedAdcrollo;
    private Common lcontrolActiveAfProg;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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

    private Program program;
    private Infrastructure infrastructure;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private PersonProgram personProgram;
    private Case1 case1;
    private CaseRole arCaseRole;
    private CsePerson arCsePerson;
  }
#endregion
}
