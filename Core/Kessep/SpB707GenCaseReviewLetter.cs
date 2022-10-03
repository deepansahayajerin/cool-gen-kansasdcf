// Program: SP_B707_GEN_CASE_REVIEW_LETTER, ID: 374482926, model: 746.
// Short name: SWEP707B
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
/// A program: SP_B707_GEN_CASE_REVIEW_LETTER.
/// </para>
/// <para>
/// Input : DB2
/// Output : DB2
/// External Commit.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB707GenCaseReviewLetter: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B707_GEN_CASE_REVIEW_LETTER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB707GenCaseReviewLetter(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB707GenCaseReviewLetter.
  /// </summary>
  public SpB707GenCaseReviewLetter(IContext context, Import import,
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
    // 07/14/2000	M Ramirez	WR# 177		Initial Development
    // --------------------------------------------------------------------------------------
    // 10/31/2008      Linda/Arun      CQ#7505        Added Run mode 
    // functionality
    // CQ31596 Update the business rules and generate multiple letters.
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
    // * 03/13/2012  Raj S              CQ31596     Phase-I Cahnges.
    // *
    // *
    // 
    // Exclude CSE cases with AP legal Rep.,    *
    // *
    // 
    // Interstate cases, print letter with      *
    // *
    // 
    // court case number.                       *
    // *
    // 
    // *
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB707Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.Document.Name = "PRMODLTR";
    local.Infrastructure.ReferenceDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
    local.Infrastructure.CreatedTimestamp = local.Current.Timestamp;

    // ***************************************************************************************
    // *CQ31596 Phase-I:  Temporaraly ignored the restart case value, in order 
    // to start the  *
    // *                  process starting from first case.  If you want to 
    // include restart  *
    // *                  logic again, remove the below statement.
    // *
    // ***************************************************************************************
    local.Restart.Number = "";

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

      ++local.CheckpointRead.Count;
      ++local.LcontrolRead.Count;
      local.EabReportSend.RptDetail = "";
      ExitState = "ACO_NN0000_ALL_OK";

      // ***************************************************************************************
      // *CQ31596 Phase-I:  Exclude CSE cases with interstate case involvement
      // *
      // ***************************************************************************************
      local.Found.Flag = "N";

      if (ReadInterstateRequest())
      {
        local.Found.Flag = "Y";
      }

      if (AsChar(local.Found.Flag) == 'Y')
      {
        ++local.LcontrolInterstateDup.Count;

        if (AsChar(local.DebugOn.Flag) == 'F')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       Interstate involvement found for Case";
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
      // -------------------------------------------------------------------
      // Must be in ENF Function
      // ----------------------------------------------------------------------
      UseSiCabReturnCaseFunction();

      if (!Equal(local.CaseFuncWorkSet.FuncText3, "ENF"))
      {
        ++local.LcontrolNotEnfFunction.Count;

        if (AsChar(local.DebugOn.Flag) == 'F')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       Case not in ENF Function";
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
      // -------------------------------------------------------------------
      // Must be an active AR
      // ----------------------------------------------------------------------
      if (!ReadCaseRoleCsePerson2())
      {
        ++local.LcontrolMissingAr.Count;

        if (AsChar(local.DebugOn.Flag) == 'F')
        {
          local.EabReportSend.RptDetail = "DEBUG:       AR not found for Case";
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
      // -------------------------------------------------------------------
      // There should only be one AP, but don't abend if there happen to be two
      // ----------------------------------------------------------------------
      if (ReadCaseRoleCsePerson1())
      {
        // ***************************************************************************************
        // *CQ31596 Phase-I:  Exclude CSE cases where AP has legal 
        // representation                *
        // ***************************************************************************************
        foreach(var item1 in ReadPersonPrivateAttorney())
        {
          ++local.LcontrolApLegalRep.Count;

          if (AsChar(local.DebugOn.Flag) == 'F')
          {
            local.EabReportSend.RptDetail =
              "DEBUG:          Active Legal Representation for AP; AP = " + entities
              .ApCsePerson.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
          }

          goto ReadEach3;
        }
      }
      else
      {
        ++local.LcontrolMissingAp.Count;

        if (AsChar(local.DebugOn.Flag) == 'F')
        {
          local.EabReportSend.RptDetail = "DEBUG:       AP not found for Case";
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
      // -----------------------------------------------------------------
      // Checking for the existence of active children
      // --------------------------------------------------------------------
      if (!ReadCaseRole())
      {
        ++local.LcontrolMissingCh.Count;

        if (AsChar(local.DebugOn.Flag) == 'F')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       Active CH not found for Case";
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

      // ***************************************************************************************
      // *CQ31596 :  AP must not have Good Cause.
      // 
      // *
      // ***************************************************************************************
      local.Found.Flag = "N";

      if (ReadGoodCause1())
      {
        if (Equal(entities.GoodCause.Code, "GC") || Equal
          (entities.GoodCause.Code, "PD"))
        {
          local.Found.Flag = "Y";
        }
      }

      if (AsChar(local.Found.Flag) == 'Y')
      {
        ++local.LcontrolArGoodCause.Count;

        if (AsChar(local.DebugOn.Flag) == 'F')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:                               AP has Good Cause for Case";
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

      // ***************************************************************************************
      // *CQ31596 :  AR must not have Good Cause.
      // 
      // *
      // ***************************************************************************************
      local.Found.Flag = "N";

      if (ReadGoodCause2())
      {
        if (Equal(entities.GoodCause.Code, "GC") || Equal
          (entities.GoodCause.Code, "PD"))
        {
          local.Found.Flag = "Y";
        }
      }

      if (AsChar(local.Found.Flag) == 'Y')
      {
        ++local.LcontrolArGoodCause.Count;

        if (AsChar(local.DebugOn.Flag) == 'F')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:                               AR has Good Cause for Case";
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

      // ***************************************************************************************
      // *CQ31596 :  Verify whether received modification review performed for 
      // the selected    *
      // *           Case, AP and Court Case number. If so, skip the selected 
      // Case            *
      // *
      // 
      // *
      // ***************************************************************************************
      local.NewestModificationReview.Timestamp = local.Null1.Timestamp;

      if (ReadInfrastructure1())
      {
        local.NewestModificationReview.Timestamp =
          entities.Infrastructure.CreatedTimestamp;
      }

      if (!Lt(local.NewestModificationReview.Timestamp,
        local.NewestReview.Timestamp))
      {
        ++local.LcontrolRecentModRev.Count;

        if (AsChar(local.DebugOn.Flag) == 'F')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       Recent Modification Review for Case and AP; AP = " + entities
            .ApCsePerson.Number;
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

      // ***************************************************************************************
      // *CQ31596 Phase-I:  Checking for an active CS Debt as well as selects 
      // only Primary     *
      // *                  Obligations (i.e. primary secondary code is 'P' or '
      // ')            *
      // ***************************************************************************************
      local.Found.Flag = "N";
      local.ObligationFound.Flag = "N";

      foreach(var item1 in ReadObligationObligationType2())
      {
        foreach(var item2 in ReadAccrualInstructions())
        {
          if (ReadObligationTransactionDebtDetail1())
          {
            local.ObligationFound.Flag = "Y";

            goto ReadEach1;
          }
        }
      }

ReadEach1:

      if (AsChar(local.ObligationFound.Flag) == 'N')
      {
        ++local.LcontrolNoActiveCsOblg.Count;

        if (AsChar(local.DebugOn.Flag) == 'F')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       Active CS Obligation not found for AP; AP = " + entities
            .ApCsePerson.Number;
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
      // -----------------------------------------------------------------
      // Check for a Legal Action Class 'J' for the AP
      // --------------------------------------------------------------------
      local.Found.Flag = "N";
      local.JclassLegalActionFound.Flag = "N";
      local.Previous.StandardNumber = "";
      local.Previous.CourtCaseNumber = "";

      foreach(var item1 in ReadLegalAction())
      {
        // ***************************************************************************************
        // *CQ31596 Phase-I:  Skip processing for duplicate court case number
        // *
        // ***************************************************************************************
        if (Equal(entities.LegalAction.CourtCaseNumber,
          local.Previous.CourtCaseNumber))
        {
          continue;
        }

        // ***************************************************************************************
        // *CQ31596 Phase-I:  Check the filed date with the review date and skip
        // if it is recent *
        // ***************************************************************************************
        if (!Lt(entities.LegalAction.FiledDate, local.NewestReview.Date))
        {
          ++local.LcontrolRecentJClass.Count;

          if (AsChar(local.DebugOn.Flag) == 'F')
          {
            local.EabReportSend.RptDetail =
              "DEBUG: Recent Journal Entry Filed for Court Case Number(Filed Dt) = " +
              entities.LegalAction.StandardNumber;
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

        // ***************************************************************************************
        // *CQ31596 Phase-I:  Checking for the existence of Select CSE Case 
        // active children  is  *
        // *                  part of the selected court order (Legal Action).
        // *
        // ***************************************************************************************
        // ***************************************************************************************
        // *CQ31596 Phase-I:  Checking for an active CS Debt as well as selects 
        // only Primary     *
        // *                  Obligations (i.e. primary secondary code is 'P' or
        // ' ')            *
        // ***************************************************************************************
        local.ObligationFound.Flag = "N";

        foreach(var item2 in ReadObligationObligationType1())
        {
          foreach(var item3 in ReadAccrualInstructions())
          {
            foreach(var item4 in ReadObligationTransactionDebtDetail2())
            {
              if (ReadLegalActionPersonCsePersonAccount())
              {
                local.ObligationFound.Flag = "Y";

                goto ReadEach2;
              }
            }
          }
        }

ReadEach2:

        if (AsChar(local.ObligationFound.Flag) == 'N')
        {
          ++local.LcontrolNoActiveCsOblg.Count;

          if (AsChar(local.DebugOn.Flag) == 'F')
          {
            local.EabReportSend.RptDetail =
              "DEBUG:Active CS Obligation not found for AP & CO; AP & CO = " + entities
              .ApCsePerson.Number + ", Std.#:" + entities
              .LegalAction.StandardNumber;
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

        local.Found.Flag = "Y";
        local.JclassLegalActionFound.Flag = "Y";
        local.Previous.CourtCaseNumber = entities.LegalAction.CourtCaseNumber;

        // ***************************************************************************************
        // *CQ31596 :  Process Docuemnt for selected 
        // AP
        // 
        // *
        // ***************************************************************************************
        if (IsEmpty(local.Ap.FamilyViolenceIndicator))
        {
          // ***************************************************************************************
          // *CQ31596 :  Check whether Docuemnt generated for AR for the 
          // selected CSE Case and     *
          // *           court order.
          // 
          // *
          // ***************************************************************************************
          local.SpDocKey.KeyCase = entities.Case1.Number;
          local.SpDocKey.KeyPerson = entities.ArCsePerson.Number;
          local.SpDocKey.KeyLegalAction = entities.LegalAction.Identifier;
          local.OutgoingDocument.PrintSucessfulIndicator = "";
          local.Infrastructure.SystemGeneratedIdentifier = 0;
          UseSpDocFindOutgoingDocument();

          if (!IsEmpty(local.OutgoingDocument.PrintSucessfulIndicator))
          {
            if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) == 'Y')
            {
              if (ReadInfrastructure2())
              {
                // mjr
                // --------------------------------------------------------------------
                // Document was printed recently enough that we don't need to 
                // send another
                // -----------------------------------------------------------------------
                ++local.LcontrolArRecentDoc.Count;
                ++local.LcontrolAllRecentDoc.Count;

                if (AsChar(local.DebugOn.Flag) == 'F')
                {
                  local.EabReportSend.RptDetail =
                    "DEBUG:       Recent document found for AR; AR = " + entities
                    .ArCsePerson.Number + ", Std.#:" + entities
                    .LegalAction.StandardNumber;
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
              else
              {
                // mjr
                // --------------------------------------------------------------------
                // Document was printed but not recently enough
                // -----------------------------------------------------------------------
                local.Infrastructure.SystemGeneratedIdentifier = 0;
              }
            }
            else if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) == 'N'
              )
            {
              // mjr
              // --------------------------------------------------------------------
              // Document for this AR was unsuccessfully printed.
              // Try it again using same outgoing_doc.
              // -----------------------------------------------------------------------
            }
            else
            {
              // mjr
              // --------------------------------------------------------------------
              // Document for this AR is already in queue.
              // -----------------------------------------------------------------------
              ++local.LcontrolArQueuedDoc.Count;
              ++local.LcontrolAllQueuedDoc.Count;

              if (AsChar(local.DebugOn.Flag) == 'F')
              {
                local.EabReportSend.RptDetail =
                  "DEBUG:       Document found in queue for AR; AR = " + entities
                  .ArCsePerson.Number + ", Std.#:" + entities
                  .LegalAction.StandardNumber;
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

          // ***************************************************************************************
          // *CQ31596 :  At least one valid address(i.e. open ended end date 
          // value)                *
          // *           If there is no valid address do not generate letter for
          // the selected AP   *
          // ***************************************************************************************
          if (!ReadCsePersonAddress1())
          {
            ++local.LcontrolApNoAddress.Count;
            ++local.LcontrolAllNoAddress.Count;

            if (AsChar(local.DebugOn.Flag) == 'F')
            {
              local.EabReportSend.RptDetail =
                "DEBUG:          No Valid/Active  address found for AP; AP = " +
                entities.ApCsePerson.Number + ", Std.#:" + entities
                .LegalAction.StandardNumber;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";
            }

            goto Test1;
          }

          // ***************************************************************************************
          // *CQ31596 :  A document should be generated for this AP. Check 
          // whether one was already *
          // *           generated.
          // 
          // *
          // ***************************************************************************************
          local.SpDocKey.KeyCase = entities.Case1.Number;
          local.SpDocKey.KeyPerson = entities.ApCsePerson.Number;
          local.SpDocKey.KeyLegalAction = entities.LegalAction.Identifier;
          local.OutgoingDocument.PrintSucessfulIndicator = "";
          local.Infrastructure.SystemGeneratedIdentifier = 0;
          UseSpDocFindOutgoingDocument();

          if (!IsEmpty(local.OutgoingDocument.PrintSucessfulIndicator))
          {
            if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) == 'Y')
            {
              if (ReadInfrastructure2())
              {
                // mjr
                // --------------------------------------------------------------------
                // Document was printed recently enough that we don't need to 
                // send another
                // -----------------------------------------------------------------------
                ++local.LcontrolApRecentDoc.Count;
                ++local.LcontrolAllRecentDoc.Count;

                if (AsChar(local.DebugOn.Flag) == 'F')
                {
                  local.EabReportSend.RptDetail =
                    "DEBUG:       Recent document found for AP; AP = " + entities
                    .ApCsePerson.Number + ", Std.#:" + entities
                    .LegalAction.StandardNumber;
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
              else
              {
                // mjr
                // --------------------------------------------------------------------
                // Document was printed but not recently enough
                // -----------------------------------------------------------------------
                local.Infrastructure.SystemGeneratedIdentifier = 0;
              }
            }
            else if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) == 'N'
              )
            {
              // mjr
              // --------------------------------------------------------------------
              // Document for this AP was unsuccessfully printed.
              // Try it again using same outgoing_doc.
              // -----------------------------------------------------------------------
            }
            else
            {
              // mjr
              // --------------------------------------------------------------------
              // Document for this AP is already in queue.
              // -----------------------------------------------------------------------
              ++local.LcontrolApQueuedDoc.Count;
              ++local.LcontrolAllQueuedDoc.Count;

              if (AsChar(local.DebugOn.Flag) == 'F')
              {
                local.EabReportSend.RptDetail =
                  "DEBUG:       Document found in queue for AP; AP = " + entities
                  .ApCsePerson.Number + ", Std.#:" + entities
                  .LegalAction.StandardNumber;
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

          // CQ7505 starts here
          if (AsChar(local.RunMode.Flag) == 'C')
          {
            // Moved the create action block to the if part
            // mjr
            // --------------------------------------------------------------
            // Create document_trigger
            // -----------------------------------------------------------------
            UseSpCreateDocumentInfrastruct();
          }
          else
          {
            if (AsChar(local.DebugOn.Flag) == 'F')
            {
              local.EabReportSend.RptDetail =
                "DEBUG:       Document will be created for AP; AP = " + entities
                .ApCsePerson.Number + ", Std.#:" + entities
                .LegalAction.StandardNumber;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }

          // CQ7505 ends here
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.LcontrolApDocError.Count;
            ++local.LcontrolAllDocError.Count;
            local.EabReportSend.RptDetail =
              "ERROR:  Error creating document trigger for AP;  = " + entities
              .ApCsePerson.Number + ", Std.#:" + entities
              .LegalAction.StandardNumber;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
            UseEabExtractExitStateMessage();

            if (local.Infrastructure.SystemGeneratedIdentifier <= 0)
            {
              // mjr
              // -----------------------------------------------------
              // Errors that occur before an infrastructure record is
              // created, create an ABEND.
              // --------------------------------------------------------
              local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
                .ExitStateWorkArea.Message;

              goto ReadEach4;
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
            ++local.LcontrolApDocCreated.Count;
            ++local.LcontrolAllDocCreated.Count;

            if (AsChar(local.DebugOn.Flag) != 'N')
            {
              local.EabReportSend.RptDetail =
                "DEBUG:       TRIGGER created for AP";
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

Test1:

        if (IsEmpty(local.Ar.FamilyViolenceIndicator))
        {
          // ***************************************************************************************
          // *CQ31596 :  At least one valid address(i.e. open ended end date 
          // value)                *
          // *           If there is no valid address do not generate letter for
          // the selected AR   *
          // ***************************************************************************************
          if (!ReadCsePersonAddress2())
          {
            ++local.LcontrolApNoAddress.Count;
            ++local.LcontrolAllNoAddress.Count;

            if (AsChar(local.DebugOn.Flag) == 'F')
            {
              local.EabReportSend.RptDetail =
                "DEBUG:          No Valid/Active  address found for AR; AR = " +
                entities.ArCsePerson.Number + ", Std.#:" + entities
                .LegalAction.StandardNumber;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";
            }

            goto Test2;
          }

          // ***************************************************************************************
          // *CQ31596 :  A document should be generated for this AR. Check 
          // whether one was already *
          // *           generated.
          // 
          // *
          // ***************************************************************************************
          local.SpDocKey.KeyCase = entities.Case1.Number;
          local.SpDocKey.KeyPerson = entities.ArCsePerson.Number;
          local.SpDocKey.KeyLegalAction = entities.LegalAction.Identifier;
          local.OutgoingDocument.PrintSucessfulIndicator = "";
          local.Infrastructure.SystemGeneratedIdentifier = 0;
          UseSpDocFindOutgoingDocument();

          if (!IsEmpty(local.OutgoingDocument.PrintSucessfulIndicator))
          {
            if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) == 'Y')
            {
              if (ReadInfrastructure2())
              {
                // mjr
                // --------------------------------------------------------------------
                // Document was printed recently enough that we don't need to 
                // send another
                // -----------------------------------------------------------------------
                ++local.LcontrolArRecentDoc.Count;
                ++local.LcontrolAllRecentDoc.Count;

                if (AsChar(local.DebugOn.Flag) == 'F')
                {
                  local.EabReportSend.RptDetail =
                    "DEBUG:       Recent document found for AR; AR = " + entities
                    .ArCsePerson.Number + ", Std.#:" + entities
                    .LegalAction.StandardNumber;
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  local.EabReportSend.RptDetail = "";
                }

                goto Test2;
              }
              else
              {
                // mjr
                // --------------------------------------------------------------------
                // Document was printed but not recently enough
                // -----------------------------------------------------------------------
                local.Infrastructure.SystemGeneratedIdentifier = 0;
              }
            }
            else if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) == 'N'
              )
            {
              // mjr
              // --------------------------------------------------------------------
              // Document for this AP was unsuccessfully printed.
              // Try it again using same outgoing_doc.
              // -----------------------------------------------------------------------
            }
            else
            {
              // mjr
              // --------------------------------------------------------------------
              // Document for this AP is already in queue.
              // -----------------------------------------------------------------------
              ++local.LcontrolArQueuedDoc.Count;
              ++local.LcontrolAllQueuedDoc.Count;

              if (AsChar(local.DebugOn.Flag) == 'F')
              {
                local.EabReportSend.RptDetail =
                  "DEBUG:       Document found in queue for AR; AR = " + entities
                  .ArCsePerson.Number + ", Std.#:" + entities
                  .LegalAction.StandardNumber;
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                local.EabReportSend.RptDetail = "";
              }

              goto Test2;
            }
          }

          // CQ7505 starts here
          if (AsChar(local.RunMode.Flag) == 'C')
          {
            // Moved the create action block to the if part
            // mjr
            // --------------------------------------------------------------
            // Create document_trigger
            // -----------------------------------------------------------------
            local.SpDocKey.KeyPerson = entities.ArCsePerson.Number;
            UseSpCreateDocumentInfrastruct();
          }
          else
          {
            if (AsChar(local.DebugOn.Flag) == 'F')
            {
              local.EabReportSend.RptDetail =
                "DEBUG:       Document will be created for AR; AR = " + entities
                .ArCsePerson.Number + ", Std.#:" + entities
                .LegalAction.StandardNumber;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }

          // CQ7505 ends here
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.LcontrolArDocError.Count;
            ++local.LcontrolAllDocError.Count;
            local.EabReportSend.RptDetail =
              "ERROR:  Error creating document trigger for AR;  = " + entities
              .ArCsePerson.Number + ", Std.#:" + entities
              .LegalAction.StandardNumber;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
            UseEabExtractExitStateMessage();

            if (local.Infrastructure.SystemGeneratedIdentifier <= 0)
            {
              // mjr
              // -----------------------------------------------------
              // Errors that occur before an infrastructure record is
              // created, create an ABEND.
              // --------------------------------------------------------
              local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
                .ExitStateWorkArea.Message;

              goto ReadEach4;
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
            ++local.LcontrolArDocCreated.Count;
            ++local.LcontrolAllDocCreated.Count;

            if (AsChar(local.DebugOn.Flag) != 'N')
            {
              local.EabReportSend.RptDetail =
                "DEBUG:       TRIGGER created for AR";
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

Test2:

        local.NewestClassJ.FiledDate = entities.LegalAction.FiledDate;
      }

      if (AsChar(local.JclassLegalActionFound.Flag) == 'N')
      {
        ++local.LcontrolNoLactJ.Count;

        if (AsChar(local.DebugOn.Flag) == 'F')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       Legal Action Class 'J' not found for Case and AP; AP = " +
            entities.ApCsePerson.Number;
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

      // -----------------------------------------------------------------------
      // Commit processing
      // -----------------------------------------------------------------------
      if (local.CheckpointRead.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .LcontrolAllDocCreated.Count >= local.MaximumTriggers.Count && local
        .MaximumTriggers.Count > 0)
      {
        // ----------------------------------------------------------------
        // Restart info in this program is Case Number.
        // ----------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInfo = "CASE:" + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
        local.ProgramCheckpointRestart.RestartInd = "Y";
        UseUpdatePgmCheckpointRestart2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabReportSend.RptDetail =
            "SYSTEM ERROR:  Unable to update program_checkpoint_restart.";

          return;
        }

        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabReportSend.RptDetail =
            "SYSTEM ERROR:  Unable to commit database.";

          return;
        }

        // -------------------------------------------------------------
        // Reset checkpoint counter
        // -------------------------------------------------------------
        local.CheckpointRead.Count = 0;
        local.Restart.Number = entities.Case1.Number;

        // -------------------------------------------------------------
        // If we created the maximum number of triggers for this run, then quit
        // -------------------------------------------------------------
        if (local.LcontrolAllDocCreated.Count >= local.MaximumTriggers.Count)
        {
          break;
        }
      }

      // *****End of READ EACH
ReadEach3:
      ;
    }

ReadEach4:

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
      if (local.LcontrolAllDocCreated.Count < local.MaximumTriggers.Count)
      {
        local.ProgramCheckpointRestart.RestartInfo = "";
        local.ProgramCheckpointRestart.RestartInd = "N";
        UseUpdatePgmCheckpointRestart1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }
      }
      else
      {
        local.EabReportSend.RptDetail =
          "DEBUG:  Maximum triggers created; Start next run on Case = " + local
          .Restart.Number;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
      }
    }

    // -----------------------------------------------------------
    // Write control totals and close reports
    // -----------------------------------------------------------
    UseSpB707WriteControlsAndClose();
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyCase = source.KeyCase;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyPerson = source.KeyPerson;
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

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiCabReturnCaseFunction()
  {
    var useImport = new SiCabReturnCaseFunction.Import();
    var useExport = new SiCabReturnCaseFunction.Export();

    useImport.Case1.Number = entities.Case1.Number;

    Call(SiCabReturnCaseFunction.Execute, useImport, useExport);

    local.CaseFuncWorkSet.FuncText3 = useExport.CaseFuncWorkSet.FuncText3;
  }

  private void UseSpB707Housekeeping()
  {
    var useImport = new SpB707Housekeeping.Import();
    var useExport = new SpB707Housekeeping.Export();

    Call(SpB707Housekeeping.Execute, useImport, useExport);

    local.RunMode.Flag = useExport.RunMode.Flag;
    local.MaximumTriggers.Count = useExport.MaximumTriggers.Count;
    MoveInfrastructure2(useExport.ModificationReview, local.ModificationReview);
    local.Current.Timestamp = useExport.Current.Timestamp;
    MoveDateWorkArea(useExport.NewestReview, local.NewestReview);
    local.Restart.Number = useExport.Restart.Number;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.DebugOn.Flag = useExport.DebugOn.Flag;
  }

  private void UseSpB707WriteControlsAndClose()
  {
    var useImport = new SpB707WriteControlsAndClose.Import();
    var useExport = new SpB707WriteControlsAndClose.Export();

    useImport.NotEnfFunc.Count = local.LcontrolNotEnfFunction.Count;
    useImport.AllFv.Count = local.LcontrolAllFv.Count;
    useImport.AllDocCreates.Count = local.LcontrolAllDocCreated.Count;
    useImport.AllDocErrors.Count = local.LcontrolAllDocError.Count;
    useImport.AllQueuedDoc.Count = local.LcontrolAllQueuedDoc.Count;
    useImport.AllRecentDoc.Count = local.LcontrolAllRecentDoc.Count;
    useImport.AllNoAddress.Count = local.LcontrolAllNoAddress.Count;
    useImport.RecentJClass.Count = local.LcontrolRecentJClass.Count;
    useImport.RecentModRev.Count = local.LcontrolRecentModRev.Count;
    useImport.InterstateInvolvement.Count = local.LcontrolInterstateDup.Count;
    useImport.CasesRead.Count = local.LcontrolRead.Count;
    useImport.ArMissing.Count = local.LcontrolMissingAr.Count;
    useImport.ApMissing.Count = local.LcontrolMissingAp.Count;
    useImport.ChildMissing.Count = local.LcontrolMissingCh.Count;
    useImport.NoLegalActionClassJ.Count = local.LcontrolNoLactJ.Count;
    useImport.NoActiveCsOblg.Count = local.LcontrolNoActiveCsOblg.Count;
    useImport.ArFv.Count = local.LcontrolArFv.Count;
    useImport.ArDocCreates.Count = local.LcontrolArDocCreated.Count;
    useImport.ArDocErrors.Count = local.LcontrolArDocError.Count;
    useImport.ArGoodCause.Count = local.LcontrolArGoodCause.Count;
    useImport.ArNoAddress.Count = local.LcontrolArNoAddress.Count;
    useImport.ArOrg.Count = local.LcontrolArOrganization.Count;
    useImport.ArQueuedDocs.Count = local.LcontrolArQueuedDoc.Count;
    useImport.ArRecentDoc.Count = local.LcontrolArRecentDoc.Count;
    useImport.ApFv.Count = local.LcontrolApFv.Count;
    useImport.ApDocCreates.Count = local.LcontrolApDocCreated.Count;
    useImport.ApDocErrors.Count = local.LcontrolApDocError.Count;
    useImport.ApRecentDoc.Count = local.LcontrolApRecentDoc.Count;
    useImport.ApQueuedDoc.Count = local.LcontrolApQueuedDoc.Count;
    useImport.ApNoAddress.Count = local.LcontrolApNoAddress.Count;

    Call(SpB707WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    MoveDocument(local.Document, useImport.Document);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpDocFindOutgoingDocument()
  {
    var useImport = new SpDocFindOutgoingDocument.Import();
    var useExport = new SpDocFindOutgoingDocument.Export();

    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    MoveDocument(local.Document, useImport.Document);

    Call(SpDocFindOutgoingDocument.Execute, useImport, useExport);

    local.OutgoingDocument.PrintSucessfulIndicator =
      useExport.OutgoingDocument.PrintSucessfulIndicator;
    local.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
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

  private IEnumerable<bool> ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableDate(
          command, "discontinueDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Restart.Number);
        db.SetNullableDate(
          command, "statusDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.ChildCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ChildCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChildCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChildCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChildCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChildCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChildCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChildCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChildCaseRole.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "dateOfDeath", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCsePerson.Type1 = db.GetString(reader, 6);
        entities.ApCsePerson.DateOfDeath = db.GetNullableDate(reader, 7);
        entities.ApCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 8);
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    entities.ArCaseRole.Populated = false;
    entities.ArCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
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
        entities.ArCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 7);
        entities.ArCaseRole.Populated = true;
        entities.ArCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ArCsePerson.Type1);
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "endDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.Populated = true;
      });
  }

  private bool ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
        db.SetNullableDate(
          command, "endDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.Populated = true;
      });
  }

  private bool ReadGoodCause1()
  {
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause1",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", entities.Case1.Number);
        db.
          SetNullableString(command, "cspNumber1", entities.ApCsePerson.Number);
          
        db.SetNullableDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 8);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 9);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 10);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 11);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);
      });
  }

  private bool ReadGoodCause2()
  {
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
        db.SetNullableDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 8);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 9);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 10);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 11);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);
      });
  }

  private bool ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetInt32(command, "eventId", local.ModificationReview.EventId);
        db.
          SetString(command, "reasonCode", local.ModificationReview.ReasonCode);
          
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetNullableString(
          command, "csePersonNum", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 2);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 3);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 4);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 5);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 8);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          local.Infrastructure.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "referenceDate",
          local.NewestReview.Date.GetValueOrDefault());
        db.SetInt32(command, "identifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 2);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 3);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 4);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 5);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 8);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "croId", entities.ApCaseRole.Identifier);
        db.SetString(command, "croType", entities.ApCaseRole.Type1);
        db.SetString(command, "cspNum", entities.ApCaseRole.CspNumber);
        db.SetString(command, "casNum", entities.ApCaseRole.CasNumber);
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
        db.SetNullableString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.Classification = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionPersonCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.LegalActionPerson.Populated = false;
    entities.CsePersonAccount.Populated = false;

    return Read("ReadLegalActionPersonCsePersonAccount",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspSupNumber",
          entities.ObligationTransaction.CspSupNumber ?? "");
        db.SetNullableString(
          command, "cpaSupType", entities.ObligationTransaction.CpaSupType ?? ""
          );
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 7);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 8);
        entities.LegalActionPerson.Populated = true;
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private IEnumerable<bool> ReadObligationObligationType1()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationObligationType1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetInt32(command, "croId", entities.ApCaseRole.Identifier);
        db.SetString(command, "croType", entities.ApCaseRole.Type1);
        db.SetString(command, "cspNum", entities.ApCaseRole.CspNumber);
        db.SetString(command, "casNum", entities.ApCaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.ObligationType.Code = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationObligationType2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetInt32(command, "croId", entities.ApCaseRole.Identifier);
        db.SetString(command, "croType", entities.ApCaseRole.Type1);
        db.SetString(command, "cspNum", entities.ApCaseRole.CspNumber);
        db.SetString(command, "casNum", entities.ApCaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.ObligationType.Code = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);

        return true;
      });
  }

  private bool ReadObligationTransactionDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransactionDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "dueDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 7);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 10);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 11);
        entities.DebtDetail.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private IEnumerable<bool> ReadObligationTransactionDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransactionDebtDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "dueDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 7);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 10);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 11);
        entities.DebtDetail.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonPrivateAttorney()
  {
    entities.ApPersonPrivateAttorney.Populated = false;

    return ReadEach("ReadPersonPrivateAttorney",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDate(
          command, "dateRetained",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApPersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.ApPersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.ApPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.ApPersonPrivateAttorney.DateRetained = db.GetDate(reader, 3);
        entities.ApPersonPrivateAttorney.DateDismissed = db.GetDate(reader, 4);
        entities.ApPersonPrivateAttorney.Populated = true;

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
    /// A value of JclassLegalActionFound.
    /// </summary>
    [JsonPropertyName("jclassLegalActionFound")]
    public Common JclassLegalActionFound
    {
      get => jclassLegalActionFound ??= new();
      set => jclassLegalActionFound = value;
    }

    /// <summary>
    /// A value of ObligationFound.
    /// </summary>
    [JsonPropertyName("obligationFound")]
    public Common ObligationFound
    {
      get => obligationFound ??= new();
      set => obligationFound = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LegalAction Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of LcontrolNotEnfFunction.
    /// </summary>
    [JsonPropertyName("lcontrolNotEnfFunction")]
    public Common LcontrolNotEnfFunction
    {
      get => lcontrolNotEnfFunction ??= new();
      set => lcontrolNotEnfFunction = value;
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
    /// A value of LcontrolAllFv.
    /// </summary>
    [JsonPropertyName("lcontrolAllFv")]
    public Common LcontrolAllFv
    {
      get => lcontrolAllFv ??= new();
      set => lcontrolAllFv = value;
    }

    /// <summary>
    /// A value of LcontrolAllDocCreated.
    /// </summary>
    [JsonPropertyName("lcontrolAllDocCreated")]
    public Common LcontrolAllDocCreated
    {
      get => lcontrolAllDocCreated ??= new();
      set => lcontrolAllDocCreated = value;
    }

    /// <summary>
    /// A value of LcontrolAllDocError.
    /// </summary>
    [JsonPropertyName("lcontrolAllDocError")]
    public Common LcontrolAllDocError
    {
      get => lcontrolAllDocError ??= new();
      set => lcontrolAllDocError = value;
    }

    /// <summary>
    /// A value of LcontrolAllQueuedDoc.
    /// </summary>
    [JsonPropertyName("lcontrolAllQueuedDoc")]
    public Common LcontrolAllQueuedDoc
    {
      get => lcontrolAllQueuedDoc ??= new();
      set => lcontrolAllQueuedDoc = value;
    }

    /// <summary>
    /// A value of LcontrolAllRecentDoc.
    /// </summary>
    [JsonPropertyName("lcontrolAllRecentDoc")]
    public Common LcontrolAllRecentDoc
    {
      get => lcontrolAllRecentDoc ??= new();
      set => lcontrolAllRecentDoc = value;
    }

    /// <summary>
    /// A value of LcontrolAllNoAddress.
    /// </summary>
    [JsonPropertyName("lcontrolAllNoAddress")]
    public Common LcontrolAllNoAddress
    {
      get => lcontrolAllNoAddress ??= new();
      set => lcontrolAllNoAddress = value;
    }

    /// <summary>
    /// A value of LcontrolRecentJClass.
    /// </summary>
    [JsonPropertyName("lcontrolRecentJClass")]
    public Common LcontrolRecentJClass
    {
      get => lcontrolRecentJClass ??= new();
      set => lcontrolRecentJClass = value;
    }

    /// <summary>
    /// A value of LcontrolRecentModRev.
    /// </summary>
    [JsonPropertyName("lcontrolRecentModRev")]
    public Common LcontrolRecentModRev
    {
      get => lcontrolRecentModRev ??= new();
      set => lcontrolRecentModRev = value;
    }

    /// <summary>
    /// A value of LcontrolInterstateDup.
    /// </summary>
    [JsonPropertyName("lcontrolInterstateDup")]
    public Common LcontrolInterstateDup
    {
      get => lcontrolInterstateDup ??= new();
      set => lcontrolInterstateDup = value;
    }

    /// <summary>
    /// A value of LcontrolRead.
    /// </summary>
    [JsonPropertyName("lcontrolRead")]
    public Common LcontrolRead
    {
      get => lcontrolRead ??= new();
      set => lcontrolRead = value;
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
    /// A value of LcontrolMissingAp.
    /// </summary>
    [JsonPropertyName("lcontrolMissingAp")]
    public Common LcontrolMissingAp
    {
      get => lcontrolMissingAp ??= new();
      set => lcontrolMissingAp = value;
    }

    /// <summary>
    /// A value of LcontrolApLegalRep.
    /// </summary>
    [JsonPropertyName("lcontrolApLegalRep")]
    public Common LcontrolApLegalRep
    {
      get => lcontrolApLegalRep ??= new();
      set => lcontrolApLegalRep = value;
    }

    /// <summary>
    /// A value of LcontrolMissingCh.
    /// </summary>
    [JsonPropertyName("lcontrolMissingCh")]
    public Common LcontrolMissingCh
    {
      get => lcontrolMissingCh ??= new();
      set => lcontrolMissingCh = value;
    }

    /// <summary>
    /// A value of LcontrolNoLactJ.
    /// </summary>
    [JsonPropertyName("lcontrolNoLactJ")]
    public Common LcontrolNoLactJ
    {
      get => lcontrolNoLactJ ??= new();
      set => lcontrolNoLactJ = value;
    }

    /// <summary>
    /// A value of LcontrolNoActiveCsOblg.
    /// </summary>
    [JsonPropertyName("lcontrolNoActiveCsOblg")]
    public Common LcontrolNoActiveCsOblg
    {
      get => lcontrolNoActiveCsOblg ??= new();
      set => lcontrolNoActiveCsOblg = value;
    }

    /// <summary>
    /// A value of LcontrolArFv.
    /// </summary>
    [JsonPropertyName("lcontrolArFv")]
    public Common LcontrolArFv
    {
      get => lcontrolArFv ??= new();
      set => lcontrolArFv = value;
    }

    /// <summary>
    /// A value of LcontrolArDocCreated.
    /// </summary>
    [JsonPropertyName("lcontrolArDocCreated")]
    public Common LcontrolArDocCreated
    {
      get => lcontrolArDocCreated ??= new();
      set => lcontrolArDocCreated = value;
    }

    /// <summary>
    /// A value of LcontrolArDocError.
    /// </summary>
    [JsonPropertyName("lcontrolArDocError")]
    public Common LcontrolArDocError
    {
      get => lcontrolArDocError ??= new();
      set => lcontrolArDocError = value;
    }

    /// <summary>
    /// A value of LcontrolArGoodCause.
    /// </summary>
    [JsonPropertyName("lcontrolArGoodCause")]
    public Common LcontrolArGoodCause
    {
      get => lcontrolArGoodCause ??= new();
      set => lcontrolArGoodCause = value;
    }

    /// <summary>
    /// A value of LcontrolArNoAddress.
    /// </summary>
    [JsonPropertyName("lcontrolArNoAddress")]
    public Common LcontrolArNoAddress
    {
      get => lcontrolArNoAddress ??= new();
      set => lcontrolArNoAddress = value;
    }

    /// <summary>
    /// A value of LcontrolArOrganization.
    /// </summary>
    [JsonPropertyName("lcontrolArOrganization")]
    public Common LcontrolArOrganization
    {
      get => lcontrolArOrganization ??= new();
      set => lcontrolArOrganization = value;
    }

    /// <summary>
    /// A value of LcontrolArQueuedDoc.
    /// </summary>
    [JsonPropertyName("lcontrolArQueuedDoc")]
    public Common LcontrolArQueuedDoc
    {
      get => lcontrolArQueuedDoc ??= new();
      set => lcontrolArQueuedDoc = value;
    }

    /// <summary>
    /// A value of LcontrolArRecentDoc.
    /// </summary>
    [JsonPropertyName("lcontrolArRecentDoc")]
    public Common LcontrolArRecentDoc
    {
      get => lcontrolArRecentDoc ??= new();
      set => lcontrolArRecentDoc = value;
    }

    /// <summary>
    /// A value of LcontrolApFv.
    /// </summary>
    [JsonPropertyName("lcontrolApFv")]
    public Common LcontrolApFv
    {
      get => lcontrolApFv ??= new();
      set => lcontrolApFv = value;
    }

    /// <summary>
    /// A value of LcontrolApDocCreated.
    /// </summary>
    [JsonPropertyName("lcontrolApDocCreated")]
    public Common LcontrolApDocCreated
    {
      get => lcontrolApDocCreated ??= new();
      set => lcontrolApDocCreated = value;
    }

    /// <summary>
    /// A value of LcontrolApDocError.
    /// </summary>
    [JsonPropertyName("lcontrolApDocError")]
    public Common LcontrolApDocError
    {
      get => lcontrolApDocError ??= new();
      set => lcontrolApDocError = value;
    }

    /// <summary>
    /// A value of LcontrolApRecentDoc.
    /// </summary>
    [JsonPropertyName("lcontrolApRecentDoc")]
    public Common LcontrolApRecentDoc
    {
      get => lcontrolApRecentDoc ??= new();
      set => lcontrolApRecentDoc = value;
    }

    /// <summary>
    /// A value of LcontrolApQueuedDoc.
    /// </summary>
    [JsonPropertyName("lcontrolApQueuedDoc")]
    public Common LcontrolApQueuedDoc
    {
      get => lcontrolApQueuedDoc ??= new();
      set => lcontrolApQueuedDoc = value;
    }

    /// <summary>
    /// A value of LcontrolApNoAddress.
    /// </summary>
    [JsonPropertyName("lcontrolApNoAddress")]
    public Common LcontrolApNoAddress
    {
      get => lcontrolApNoAddress ??= new();
      set => lcontrolApNoAddress = value;
    }

    /// <summary>
    /// A value of MaximumTriggers.
    /// </summary>
    [JsonPropertyName("maximumTriggers")]
    public Common MaximumTriggers
    {
      get => maximumTriggers ??= new();
      set => maximumTriggers = value;
    }

    /// <summary>
    /// A value of NewestModificationReview.
    /// </summary>
    [JsonPropertyName("newestModificationReview")]
    public DateWorkArea NewestModificationReview
    {
      get => newestModificationReview ??= new();
      set => newestModificationReview = value;
    }

    /// <summary>
    /// A value of NewestClassJ.
    /// </summary>
    [JsonPropertyName("newestClassJ")]
    public LegalAction NewestClassJ
    {
      get => newestClassJ ??= new();
      set => newestClassJ = value;
    }

    /// <summary>
    /// A value of ModificationReview.
    /// </summary>
    [JsonPropertyName("modificationReview")]
    public Infrastructure ModificationReview
    {
      get => modificationReview ??= new();
      set => modificationReview = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of NewestReview.
    /// </summary>
    [JsonPropertyName("newestReview")]
    public DateWorkArea NewestReview
    {
      get => newestReview ??= new();
      set => newestReview = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of RunMode.
    /// </summary>
    [JsonPropertyName("runMode")]
    public Common RunMode
    {
      get => runMode ??= new();
      set => runMode = value;
    }

    private Common jclassLegalActionFound;
    private Common obligationFound;
    private LegalAction previous;
    private CsePerson ap;
    private CsePerson ar;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common lcontrolNotEnfFunction;
    private Common checkpointRead;
    private Common lcontrolAllFv;
    private Common lcontrolAllDocCreated;
    private Common lcontrolAllDocError;
    private Common lcontrolAllQueuedDoc;
    private Common lcontrolAllRecentDoc;
    private Common lcontrolAllNoAddress;
    private Common lcontrolRecentJClass;
    private Common lcontrolRecentModRev;
    private Common lcontrolInterstateDup;
    private Common lcontrolRead;
    private Common lcontrolMissingAr;
    private Common lcontrolMissingAp;
    private Common lcontrolApLegalRep;
    private Common lcontrolMissingCh;
    private Common lcontrolNoLactJ;
    private Common lcontrolNoActiveCsOblg;
    private Common lcontrolArFv;
    private Common lcontrolArDocCreated;
    private Common lcontrolArDocError;
    private Common lcontrolArGoodCause;
    private Common lcontrolArNoAddress;
    private Common lcontrolArOrganization;
    private Common lcontrolArQueuedDoc;
    private Common lcontrolArRecentDoc;
    private Common lcontrolApFv;
    private Common lcontrolApDocCreated;
    private Common lcontrolApDocError;
    private Common lcontrolApRecentDoc;
    private Common lcontrolApQueuedDoc;
    private Common lcontrolApNoAddress;
    private Common maximumTriggers;
    private DateWorkArea newestModificationReview;
    private LegalAction newestClassJ;
    private Infrastructure modificationReview;
    private Common found;
    private OutgoingDocument outgoingDocument;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private SpDocKey spDocKey;
    private DateWorkArea newestReview;
    private Case1 restart;
    private DateWorkArea null1;
    private Document document;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common debugOn;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common runMode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("apPersonPrivateAttorney")]
    public PersonPrivateAttorney ApPersonPrivateAttorney
    {
      get => apPersonPrivateAttorney ??= new();
      set => apPersonPrivateAttorney = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CsePerson childCsePerson;
    private GoodCause goodCause;
    private Infrastructure infrastructure;
    private InterstateRequest interstateRequest;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
    private AccrualInstructions accrualInstructions;
    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private CsePersonAddress csePersonAddress;
    private CaseRole childCaseRole;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private PersonPrivateAttorney apPersonPrivateAttorney;
    private CaseRole arCaseRole;
    private CsePerson arCsePerson;
    private Case1 case1;
  }
#endregion
}
