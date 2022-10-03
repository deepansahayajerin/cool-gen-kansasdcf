// Program: OE_B447_SVES_ALERT_N_IWO_GEN, ID: 945066112, model: 746.
// Short name: SWE04476
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
/// A program: OE_B447_SVES_ALERT_N_IWO_GEN.
/// </para>
/// <para>
/// This action block generates the alerts and History records for SVES 
/// responses received from FCR.  In addtion to this, this action generate
/// Income Source, Automatic IWOs and SVES notics to AR.
/// </para>
/// </summary>
[Serializable]
public partial class OeB447SvesAlertNIwoGen: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B447_SVES_ALERT_N_IWO_GEN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB447SvesAlertNIwoGen(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB447SvesAlertNIwoGen.
  /// </summary>
  public OeB447SvesAlertNIwoGen(IContext context, Import import, Export export):
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
    // *                               
    // Maintenance Log
    // 
    // *
    // **************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  
    // ---------
    // ----------------------------------------*
    // * 06/21/2011  Raj S              CQ5577      **** Initial Coding  ****
    // *
    // *
    // 
    // This Action Block generates the alerts  *
    // *
    // 
    // and History records for SVES responses  *
    // *
    // 
    // received from FCR.                      *
    // *
    // 
    // *
    // *
    // 
    // *
    // * 06/26/2011  Raj S              CQ34791     Modified to fix duplicate 
    // IWO generation*
    // *
    // 
    // for the duplicate SVES Responses.       *
    // *
    // 
    // The process will check for SSA income   *
    // *
    // 
    // source record for the selected person   *
    // *
    // 
    // and if exists will not generate Auto IWO*
    // *
    // 
    // *
    // *
    // 
    // *
    // * 08/02/2018  GVandy             CQ61457     Update SVES and 'O' type 
    // employer to    *
    // *
    // 
    // work with eIWO for SSA.                 *
    // *
    // 
    // *
    // *
    // 
    // *
    // **************************************************************************************
    // **************************************************************************************
    // This action block will be used by SVES Response batch procedure in order 
    // to generate
    // worker alerts and Narrative Details for manual IWO Generation.
    // **************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *************************************************************************************
    // Skip Alert & IWO generation processes when job skip parameter set to 'Y'.
    // *************************************************************************************
    if (AsChar(import.AlertGenerationSkipFl.Flag) == 'Y')
    {
      return;
    }

    local.Max.Date = import.Max.Date;
    local.Infrastructure.Assign(import.Infrastructure);
    local.Document1.Assign(import.Infrastructure);
    local.IncsExists.Flag = "";
    local.IwoGenerated.Flag = "";
    local.IwoGenerated.Flag = "";
    local.ProgramProcessingInfo.ProcessDate = import.ProcessingDate.Date;
    local.CsePersonsWorkSet.Number = import.Infrastructure.CsePersonNumber ?? Spaces
      (10);

    // **************************************************************************************
    // Read all case & case role for the selected person and for each case 
    // selected establish
    // infrastructure record(Alert & History) record.  While selecting the case 
    // role record
    // only active records needs to be selected.
    // **************************************************************************************
    foreach(var item in ReadCaseRoleCaseCsePerson())
    {
      // ******************************************************************************************
      // * Check for Active person in order generate alerts, IWOs and Document/
      // Notices.           *
      // ******************************************************************************************
      local.SvesPersonActiveFlag.Flag = "";
      local.Infrastructure.ProcessStatus = "Q";
      MoveCsePerson(entities.ExistingCsePerson, local.CsePerson);

      if (!Lt(import.ProcessingDate.Date, entities.ExistingCaseRole.StartDate) &&
        !Lt(entities.ExistingCaseRole.EndDate, import.ProcessingDate.Date))
      {
        local.SvesPersonActiveFlag.Flag = "Y";
        local.Infrastructure.ProcessStatus = "Q";
      }
      else
      {
        local.Infrastructure.ProcessStatus = "H";
      }

      local.Infrastructure.CaseNumber = entities.ExistingCase.Number;

      // ****************************************************************************************
      // ** Assign the respective reason codes based on the role played by the 
      // person for the  **
      // ** selected case.
      // 
      // **
      // **
      // 
      // **
      // ** Person Role Type           		History     Alert
      // **
      // **
      // 
      // **
      // ** AP
      // 
      // X	      X
      // *
      // *
      // ** AR
      // 
      // X	      X
      // *
      // *
      // ** CH
      // 
      // X	      X
      // *
      // *
      // ****************************************************************************************
      local.Infrastructure.ReasonCode = "";

      // ******************************************************************************************
      // * Common Processing for Title-II Pending and Title-II SVES response 
      // records.             *
      // * 1. Generate Income Source
      // 
      // *
      // * 2. Generate Automatic IWOs (Only for Title-II Pending Response)
      // *
      // * 3. Generate AR Notice on Title II pending claim at SSA (Only for 
      // Title-II Pending)     *
      // ******************************************************************************************
      if ((Equal(import.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E04") || Equal
        (import.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E05") && Equal
        (import.FcrSvesTitleIi.TitleIiSuspendTerminateDt, new DateTime(1, 1, 1)))
        && AsChar(local.SvesPersonActiveFlag.Flag) == 'Y')
      {
        // *************************************************************************************
        // Skip IWO generation processes when job IWO skip parameter set to 'Y'.
        // *************************************************************************************
        if (AsChar(import.AlertGenerationSkipFl.Flag) == 'Y')
        {
          return;
        }

        // ******************************************************************************************
        // * Check whether SSA Income Source record already exists for the 
        // selected person, if not, *
        // * create a new income source for the
        // selected person.
        // 
        // *
        // ******************************************************************************************
        if (ReadIncomeSource())
        {
          // ******************************************************************************************
          // * SSA Income Source record already exists for the person not need 
          // to create a new one.   *
          // ******************************************************************************************
          if (IsEmpty(local.IncsExists.Flag))
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "SSA Income source record already exists, IWO will NOT be generated " +
              TrimEnd(" ");
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + TrimEnd
              (entities.ExistingCsePerson.Number +
              import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
            UseCabErrorReport2();
            local.IncsExists.Flag = "Y";
            local.IwoGenerated.Flag = "Y";
            MoveIncomeSource1(entities.ExistingIncomeSource, local.IncomeSource);
              
          }
        }
        else
        {
          // ******************************************************************************************
          // * SSA Income Source record  for AP/AR Roles and IWO generation AP 
          // Role Only              *
          // ******************************************************************************************
          if (Equal(entities.ExistingCaseRole.Type1, "AP"))
          {
            local.IncomeSource.Type1 = "O";
            local.IncomeSource.Code = "SA";

            if (Equal(import.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E04"))
              
            {
              local.IncomeSource.Note =
                "Generated based on FCR SVES Title-II Pending response record.";
                
            }
            else
            {
              local.IncomeSource.Note =
                "Generated based on FCR SVES Title-II  response record.";
            }

            local.IncomeSource.WorkerId = import.Infrastructure.CreatedBy;
            local.IncomeSource.StartDt = import.ProcessingDate.Date;
            local.IncomeSource.EndDt = import.Max.Date;
            local.IncomeSource.CreatedBy = local.Infrastructure.CreatedBy;
            local.IncomeSource.Identifier = Now();

            // --08/02/2018 GVandy CQ61457  Update SVES and 'O' type employer to
            // work with eIWO for SSA.
            local.IncomeSource.ReturnCd = "V";
            local.IncomeSource.ReturnDt = import.ProcessingDate.Date;

            // --Read the main employer record for the Social Security 
            // Administration, EIN 070000000.
            if (ReadEmployer())
            {
              local.IncomeSource.Name = entities.Employer.Name;
            }
            else
            {
              local.NeededToWrite.RptDetail =
                TrimEnd("While Reading SSA Employer record with EIN 070000000: ")
                + TrimEnd
                (local.CsePerson.Number +
                import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ALL_OK";

              goto Test;
            }

            // ******************************************************************************************
            // * SSA Income Source record  for AP/AR Roles and IWO generation AP
            // Role Only              *
            // ******************************************************************************************
            UseSiIncsCreateIncomeSource();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.IncsExists.Flag = "Y";
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              UseEabExtractExitStateMessage();
              local.NeededToWrite.RptDetail = "Program abended because: " + TrimEnd
                (local.ExitStateWorkArea.Message);
              local.NeededToWrite.RptDetail += TrimEnd(
                local.CsePerson.Number +
                import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ALL_OK";

              goto Test;
            }

            // ******************************************************************************************
            // * SSA Income Source non employer address record creation for IWO 
            // generation              *
            // ******************************************************************************************
            // --08/02/2018 GVandy CQ61457  Update SVES and 'O' type employer to
            // work with eIWO for SSA.
            // (Pass SSA Employer to si_incs_assoc_income_source_addr)
            UseSiIncsAssocIncomeSourceAddr();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              UseEabExtractExitStateMessage();
              local.NeededToWrite.RptDetail =
                "While Creating INCS Address: " + TrimEnd
                (local.ExitStateWorkArea.Message);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + TrimEnd
                (local.CsePerson.Number +
                import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ALL_OK";

              goto Test;
            }

            // --08/02/2018 GVandy CQ61457  Update SVES and 'O' type employer to
            // work with eIWO for SSA.
            // (Original code commented out below)
            // ******************************************************************************************
            // * SSA Income Source contact information build. Eventhough, FCR 
            // SVES response did not have*
            // * this information, we need to create the income source contact 
            // record with type "HP"    *
            // * because, INCS screen has a business rule stating all the income
            // source records should  *
            // * be associated with HP contact record.   The below mentioned AB 
            // will create the contact *
            // * record with telephone number as zero.
            // 
            // *
            // ******************************************************************************************
            local.IncomeSourceContact.AreaCode = 0;
            local.IncomeSourceContact.Number = 0;
            local.IncomeSourceContact.Type1 = "HP";
            UseSiIncsCreateIncomeSrcContct();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              UseEabExtractExitStateMessage();
              local.NeededToWrite.RptDetail =
                "Income source Contact type HP Creation:" + TrimEnd
                (local.ExitStateWorkArea.Message);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + TrimEnd
                (local.CsePerson.Number +
                import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ALL_OK";

              goto Test;
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.IncsExists.Flag = "Y";
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              UseEabExtractExitStateMessage();
              local.NeededToWrite.RptDetail =
                "After Reading & creating INCS Recs: " + TrimEnd
                (local.ExitStateWorkArea.Message);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + TrimEnd
                (import.FcrSvesGenInfo.MemberId +
                import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ALL_OK";

              goto Test;
            }
          }
        }

        if (Equal(entities.ExistingCaseRole.Type1, "AP") && IsEmpty
          (local.IwoGenerated.Flag))
        {
          // ******************************************************************************************
          // * Geenrate Income Withholding Order for the AP selected when valid 
          // incomde source record *
          // * exists for the person.
          // 
          // *
          // ******************************************************************************************
          if (AsChar(local.IncsExists.Flag) == 'Y' && IsEmpty
            (local.IwoGenerated.Flag))
          {
            local.CsePersonsWorkSet.Number =
              import.Infrastructure.CsePersonNumber ?? Spaces(10);
            UseLeAutomaticIwoGeneration();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.IwoGenerated.Flag = "Y";
              ++export.TotIwoRecs.Count;
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              UseEabExtractExitStateMessage();
              local.BatchTimestampWorkArea.IefTimestamp =
                local.IncomeSource.Identifier;
              UseLeCabConvertTimestamp();
              local.NeededToWrite.RptDetail = "During Auto IWO Generation: " + TrimEnd
                (local.ExitStateWorkArea.Message);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + TrimEnd
                (entities.ExistingCsePerson.Number +
                local.BatchTimestampWorkArea.TextTimestamp);
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ALL_OK";

              goto Test;
            }
          }

          // ******************************************************************************************
          // * Generate Notice SVESNOTC regarding SVES Title II pending/Title II
          // claim to AR to follow*
          // * with Nearest SSA office on SSA Claim.
          // 
          // *
          // ******************************************************************************************
          if (Equal(import.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E04"))
          {
            // ******************************************************************************************
            // * Check for current case interstate 
            // involvement
            // 
            // *
            // ******************************************************************************************
            if (ReadInterstateRequest())
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "Interstate Request Case Found:" + TrimEnd(" ");
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + TrimEnd
                (entities.ExistingCase.Number +
                import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
              UseCabErrorReport2();

              goto Test;
            }

            // ******************************************************************************************
            // * The slected case should be associated with Enforcement/
            // Obligation Funcitons (i.e. ENF  *
            // * or OBG), The below mentioned CAB will determine the function of
            // a case based on the    *
            // * mhighest level of state within the case
            // units.
            // 
            // *
            // ******************************************************************************************
            UseSiCabReturnCaseFunction();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (Equal(local.CaseFuncWorkSet.FuncText3, "ENF") || Equal
                (local.CaseFuncWorkSet.FuncText3, "OBG"))
              {
                // ******************************************************************************************
                // * The selected case is associated with the valid case 
                // fucntion process to generate the   *
                // * the AR notice.
                // 
                // *
                // ******************************************************************************************
              }
              else
              {
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail = "No ENF case unit found" + TrimEnd
                  (" ");
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + TrimEnd
                  (entities.ExistingCase.Number +
                  import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
                UseCabErrorReport2();

                goto Test;
              }
            }
            else
            {
              UseEabExtractExitStateMessage();
              local.NeededToWrite.RptDetail =
                "While Determining Case Function:" + TrimEnd
                (local.ExitStateWorkArea.Message);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + TrimEnd
                (import.FcrSvesGenInfo.MemberId +
                import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ALL_OK";

              goto Test;
            }

            // ******************************************************************************************
            // * The AR associated with the selected case should have type 'C'
            // lient not Organization    *
            // ******************************************************************************************
            if (ReadCaseRoleCsePerson1())
            {
              // ******************************************************************************************
              // * Selected AR must have a current 
              // verified address
              // 
              // *
              // ******************************************************************************************
              if (!ReadCsePersonAddress())
              {
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "Verified Address Not Found for AR: " + entities
                  .ExistingCase.Number + entities.ArCsePerson.Number;
                UseCabErrorReport2();

                goto Test;
              }

              // ******************************************************************************************
              // * Selected AR must NOT associated 
              // with any Good Cause.
              // 
              // *
              // ******************************************************************************************
              // **********************************************************
              // we are now looking the most current good cause record for the 
              // current ar and current
              // case combination, not the most current ar case role
              // **********************************************************
              if (ReadGoodCause())
              {
                local.EabFileHandling.Action = "WRITE";

                if (Equal(entities.GoodCause.Code, "GC") || Equal
                  (entities.GoodCause.Code, "PD"))
                {
                  // **********************************************************
                  // cq61087
                  // changed this to only count the CG and PD since this good 
                  // cause. If it is a CO then
                  // that means they are off good cause. Also added the sort so 
                  // it only looks at the
                  // current record not just any record.
                  // **********************************************************
                  local.NeededToWrite.RptDetail =
                    "Good Cause Found for AR: " + entities
                    .ExistingCase.Number + entities.ArCsePerson.Number;
                  UseCabErrorReport2();

                  goto Test;
                }
              }
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "AR Not Found to generate Doc: " + entities
                .ExistingCase.Number + entities.ArCsePerson.Number;
              UseCabErrorReport2();

              goto Test;
            }

            // ******************************************************************************************
            // * Checking for the existence of Active and Valid Children in the 
            // selected case           *
            // ******************************************************************************************
            local.ValidChildFlag.Flag = "";

            foreach(var item1 in ReadCaseRoleCsePerson2())
            {
              // ******************************************************************************************
              // * Check for Emancipation date. If it is less than processing 
              // date, that means child is   *
              // * emancipated before 18 years.
              // 
              // *
              // ******************************************************************************************
              if (Lt(local.Null1.Date, entities.ChildCaseRole.DateOfEmancipation)
                && Lt
                (entities.ChildCaseRole.DateOfEmancipation,
                import.ProcessingDate.Date))
              {
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail = "Child is Emancipated: " + TrimEnd
                  (NumberToString(
                    DateToInt(AddYears(
                    entities.ChildCaseRole.DateOfEmancipation, 0)), 15));
                UseCabErrorReport2();

                continue;
              }

              local.ValidChildFlag.Flag = "Y";

              break;
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              UseEabExtractExitStateMessage();
              local.NeededToWrite.RptDetail = "Program abended because: " + TrimEnd
                (local.ExitStateWorkArea.Message);
              local.NeededToWrite.RptDetail += TrimEnd(
                import.FcrSvesGenInfo.MemberId +
                import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ALL_OK";
            }

            if (IsEmpty(local.ValidChildFlag.Flag))
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "No Valid Child Found to generate doc" + entities
                .ExistingCase.Number + entities.ArCsePerson.Number;
              UseCabErrorReport2();

              goto Test;
            }

            // ******************************************************************************************
            // * A document/letter SVESNOTC needs to be generated for the 
            // selected AR.                  *
            // * Check wheter one was alreay generated for the selected AR.
            // *
            // ******************************************************************************************
            local.SpDocKey.KeyCase = entities.ExistingCase.Number;
            local.SpDocKey.KeyPerson = entities.ArCsePerson.Number;
            local.SpDocKey.KeyAr = entities.ArCsePerson.Number;
            local.OutgoingDocument.PrintSucessfulIndicator = "";
            local.Document1.SystemGeneratedIdentifier = 0;
            local.Document2.Name = "SVESNOTC";
            UseSpDocFindOutgoingDocument();

            if (!IsEmpty(local.OutgoingDocument.PrintSucessfulIndicator))
            {
              if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) == 'Y')
              {
                // ******************************************************************************************
                // * A SVESNOTC generated recently to the selected AR, No need 
                // to send another one.         *
                // ******************************************************************************************
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "SVESNOTC outgoing Document print Successful: " + entities
                  .ExistingCase.Number + entities.ArCsePerson.Number;
                UseCabErrorReport2();

                goto Test;
              }
              else if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) ==
                'N')
              {
                // ******************************************************************************************
                // * Previous Docment SVESNOTC generation for this AR was NOT 
                // Successful, Try it again      *
                // * using same outgong document.
                // 
                // *
                // ******************************************************************************************
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "SVESNOTC outgoing Document print NOT Successful: " + entities
                  .ExistingCase.Number + entities.ArCsePerson.Number;
                UseCabErrorReport2();
              }
              else
              {
                // ******************************************************************************************
                // * Docment SVESNOTC generation is already in queue for this 
                // AR, skip the document trigger *
                // * generation for the selected PR.
                // 
                // *
                // ******************************************************************************************
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "SVESNOTC outgoing Document already in Queue: " + entities
                  .ExistingCase.Number + entities.ArCsePerson.Number;
                UseCabErrorReport2();

                goto Test;
              }
            }
            else if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              UseEabExtractExitStateMessage();
              local.NeededToWrite.RptDetail =
                "Outgoing document retrieval problem: " + TrimEnd
                (local.ExitStateWorkArea.Message);
              local.NeededToWrite.RptDetail += TrimEnd(
                import.FcrSvesGenInfo.MemberId +
                import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ALL_OK";

              goto Test;
            }

            // ******************************************************************************************
            // * Generate SVESNOTC Document Trigger.
            // 
            // *
            // ******************************************************************************************
            UseSpCreateDocumentInfrastruct();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "ERROR:  Error creating document trigger for AR; Case = " + entities
                .ExistingCase.Number + "; AR = " + entities.ArCsePerson.Number;
              UseCabErrorReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";
              UseEabExtractExitStateMessage();

              if (local.Document1.SystemGeneratedIdentifier <= 0)
              {
                // mjr
                // -----------------------------------------------------
                // Errors that occur before an infrastructure record is
                // created, create an ABEND.
                // --------------------------------------------------------
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
                  .ExitStateWorkArea.Message;
                UseCabErrorReport1();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabConvertNumeric.SendAmount =
                NumberToString(local.Document1.SystemGeneratedIdentifier, 15);
              UseEabConvertNumeric1();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
                (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":  " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport1();

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
              ++export.TotArLetterRecs.Count;
            }
          }
        }
      }

Test:

      // **************************************************************************************
      // *
      // 
      // *
      // * If the SVES response is for Titl-II pending claim then the process 
      // will follow this*
      // * path to set Infrastructure record 
      // values.
      // 
      // *
      // *
      // 
      // *
      // **************************************************************************************
      if (Equal(import.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E04"))
      {
        // **************************************************************************************
        // *
        // 
        // *
        // * If the SVES response is for Titl-II pending claim then the process 
        // will follow this*
        // * path to set Infrastructure record 
        // values.
        // 
        // *
        // *
        // 
        // *
        // **************************************************************************************
        if (Equal(entities.ExistingCaseRole.Type1, "AP"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESPENDAP";
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "AR"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESPENDAR";
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "CH"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESPENDCH";
        }
      }
      else if (Equal(import.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E05"))
      {
        // **************************************************************************************
        // * SVES Title-II response suspension/termination date value is 
        // populate then the      *
        // * generate the alert record stating Title-II SVES response is 
        // terminatd/suspended.   *
        // **************************************************************************************
        if (!Equal(import.FcrSvesTitleIi.TitleIiSuspendTerminateDt,
          new DateTime(1, 1, 1)))
        {
          if (Equal(entities.ExistingCaseRole.Type1, "AP"))
          {
            local.Infrastructure.ReasonCode = "FCRSVEST2TERAP";
          }
          else if (Equal(entities.ExistingCaseRole.Type1, "AR"))
          {
            local.Infrastructure.ReasonCode = "FCRSVEST2TERAR";
          }
          else if (Equal(entities.ExistingCaseRole.Type1, "CH"))
          {
            local.Infrastructure.ReasonCode = "FCRSVEST2TERCH";
          }
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "AP"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST2AP";
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "AR"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST2AR";
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "CH"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST2CH";
        }
      }
      else if (Equal(import.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E06"))
      {
        // **************************************************************************************
        // *
        // 
        // *
        // * If the SVES response is for Titl-XVI(E06) then the process will 
        // follow this path   *
        // * to set Infrastructure record values.
        // 
        // *
        // *
        // 
        // *
        // **************************************************************************************
        if (Equal(entities.ExistingCaseRole.Type1, "AP"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST16AP";
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "AR"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST16AR";
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "CH"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST16CH";
        }
      }
      else if (Equal(import.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E07"))
      {
        // **************************************************************************************
        // *
        // 
        // *
        // * If the SVES response is from prison then the process will generate 
        // required alerts *
        // * and History records.
        // 
        // *
        // *
        // 
        // *
        // **************************************************************************************
        if (Equal(entities.ExistingCaseRole.Type1, "AP"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESPRISONAP";
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "AR"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESPRISONAR";
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "CH"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESPRISONCH";
        }
      }
      else if (Equal(import.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E10"))
      {
        if (Equal(entities.ExistingCaseRole.Type1, "AP"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESNFAP";
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "AR"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESNFAR";
        }
        else if (Equal(entities.ExistingCaseRole.Type1, "CH"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESNFCH";
        }

        local.Infrastructure.ProcessStatus = "H";
      }
      else
      {
        // **************************************************************************************
        // *
        // 
        // *
        // * The SVES Response record currently read is NOT having valid locate 
        // agency response *
        // * code, skip from processing.
        // 
        // *
        // *
        // 
        // *
        // **************************************************************************************
        return;
      }

      if (IsEmpty(local.Infrastructure.ReasonCode))
      {
        continue;
      }

      // **************************************************************************************
      // The following check ensures that process is not creating a duplicate 
      // alert/narrative
      // details for the same person and same claim type.  FCR/SVES can send the
      // same info.
      // again when there is a change to the case, in those cases the following 
      // check will not
      // generate any alerts/narrative details.
      // **************************************************************************************
      if (ReadInfrastructure())
      {
        if (AsChar(local.Infrastructure.ProcessStatus) == 'H' || Equal
          (import.FcrSvesGenInfo.LocateSourceResponseAgencyCo, "E10"))
        {
          ++export.TotHistExistsRecs.Count;
        }
        else
        {
          ++export.TotAlertExistsRecs.Count;
        }

        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          TrimEnd("Infrastructure Record Already Exists For: ") + TrimEnd
          (import.FcrSvesGenInfo.MemberId +
          import.FcrSvesGenInfo.LocateSourceResponseAgencyCo + local
          .Infrastructure.ReasonCode);
        UseCabErrorReport2();

        continue;
      }

      // **************************************************************************************
      // No infrastructure records available for the selected Case, Person, 
      // Reason code & detail,
      // the process can generate the alert/History record for the selected 
      // perosn.
      // **************************************************************************************
      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        UseEabExtractExitStateMessage();
        local.NeededToWrite.RptDetail = local.Infrastructure.ReasonCode + "-  Generating Alert/History Records: " +
          TrimEnd(local.ExitStateWorkArea.Message);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + TrimEnd
          (import.FcrSvesGenInfo.MemberId +
          import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      if (AsChar(local.Infrastructure.ProcessStatus) == 'H')
      {
        ++export.TotHistRecsCreated.Count;
      }
      else
      {
        ++export.TotAlertRecsCreated.Count;
      }
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.FederalInd = source.FederalInd;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveIncomeSource1(IncomeSource source, IncomeSource target)
    
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.Note = source.Note;
    target.Name = source.Name;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
    target.Code = source.Code;
  }

  private static void MoveIncomeSource2(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveIncomeSource3(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.WorkerId = source.WorkerId;
    target.Note = source.Note;
    target.Name = source.Name;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
    target.Code = source.Code;
  }

  private static void MoveIncomeSource4(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.ReturnCd = source.ReturnCd;
    target.StartDt = source.StartDt;
  }

  private static void MoveIncomeSourceContact(IncomeSourceContact source,
    IncomeSourceContact target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
    target.ExtensionNo = source.ExtensionNo;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.AreaCode = source.AreaCode;
  }

  private static void MoveInfrastructure1(Infrastructure source,
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

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveNonEmployIncomeSourceAddress(
    NonEmployIncomeSourceAddress source, NonEmployIncomeSourceAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAr = source.KeyAr;
    target.KeyCase = source.KeyCase;
    target.KeyPerson = source.KeyPerson;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseLeAutomaticIwoGeneration()
  {
    var useImport = new LeAutomaticIwoGeneration.Import();
    var useExport = new LeAutomaticIwoGeneration.Export();

    useImport.CsePerson.Number = entities.ExistingCsePerson.Number;
    MoveIncomeSource4(local.IncomeSource, useImport.IncomeSource);

    Call(LeAutomaticIwoGeneration.Execute, useImport, useExport);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseSiCabReturnCaseFunction()
  {
    var useImport = new SiCabReturnCaseFunction.Import();
    var useExport = new SiCabReturnCaseFunction.Export();

    useImport.Case1.Number = entities.ExistingCase.Number;

    Call(SiCabReturnCaseFunction.Execute, useImport, useExport);

    local.CaseFuncWorkSet.FuncText3 = useExport.CaseFuncWorkSet.FuncText3;
  }

  private void UseSiIncsAssocIncomeSourceAddr()
  {
    var useImport = new SiIncsAssocIncomeSourceAddr.Import();
    var useExport = new SiIncsAssocIncomeSourceAddr.Export();

    useImport.Employer.Identifier = entities.Employer.Identifier;
    MoveIncomeSource2(local.IncomeSource, useImport.IncomeSource);
    useImport.NonEmployIncomeSourceAddress.Assign(
      local.NonEmployIncomeSourceAddress);
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiIncsAssocIncomeSourceAddr.Execute, useImport, useExport);

    MoveNonEmployIncomeSourceAddress(useImport.NonEmployIncomeSourceAddress,
      local.NonEmployIncomeSourceAddress);
    local.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseSiIncsCreateIncomeSource()
  {
    var useImport = new SiIncsCreateIncomeSource.Import();
    var useExport = new SiIncsCreateIncomeSource.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    MoveIncomeSource3(local.IncomeSource, useImport.IncomeSource);
    useImport.CsePerson.Assign(local.CsePerson);

    Call(SiIncsCreateIncomeSource.Execute, useImport, useExport);

    MoveIncomeSource3(useImport.IncomeSource, local.IncomeSource);
    local.CsePerson.Assign(useImport.CsePerson);
  }

  private void UseSiIncsCreateIncomeSrcContct()
  {
    var useImport = new SiIncsCreateIncomeSrcContct.Import();
    var useExport = new SiIncsCreateIncomeSrcContct.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.IncomeSource.Identifier = local.IncomeSource.Identifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveIncomeSourceContact(local.IncomeSourceContact,
      useImport.IncomeSourceContact);

    Call(SiIncsCreateIncomeSrcContct.Execute, useImport, useExport);

    local.CsePerson.Number = useImport.CsePerson.Number;
    local.IncomeSourceContact.Assign(useImport.IncomeSourceContact);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    MoveInfrastructure2(local.Document1, useImport.Infrastructure);
    useImport.Document.Name = local.Document2.Name;

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Document1);
  }

  private void UseSpDocFindOutgoingDocument()
  {
    var useImport = new SpDocFindOutgoingDocument.Import();
    var useExport = new SpDocFindOutgoingDocument.Export();

    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    useImport.Document.Name = local.Document2.Name;

    Call(SpDocFindOutgoingDocument.Execute, useImport, useExport);

    local.Document1.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
    local.OutgoingDocument.PrintSucessfulIndicator =
      useExport.OutgoingDocument.PrintSucessfulIndicator;
  }

  private IEnumerable<bool> ReadCaseRoleCaseCsePerson()
  {
    entities.ExistingCaseRole.Populated = false;
    entities.ExistingCase.Populated = false;
    entities.ExistingCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCaseCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", import.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCase.Status = db.GetNullableString(reader, 6);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 7);
        entities.ExistingCsePerson.FederalInd = db.GetNullableString(reader, 8);
        entities.ExistingCaseRole.Populated = true;
        entities.ExistingCase.Populated = true;
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);

        return true;
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.ArCaseRole.Populated = false;
    entities.ArCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
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

  private IEnumerable<bool> ReadCaseRoleCsePerson2()
  {
    entities.ChildCaseRole.Populated = false;
    entities.ChildCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "startDate", import.ProcessingDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ChildCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChildCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChildCsePerson.Number = db.GetString(reader, 1);
        entities.ChildCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChildCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChildCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChildCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChildCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 6);
        entities.ChildCsePerson.Type1 = db.GetString(reader, 7);
        entities.ChildCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 8);
        entities.ChildCaseRole.Populated = true;
        entities.ChildCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChildCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ChildCsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate",
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

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      null,
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadGoodCause()
  {
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
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
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
      });
  }

  private bool ReadIncomeSource()
  {
    entities.ExistingIncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDt", import.ProcessingDate.Date.GetValueOrDefault());
        db.
          SetNullableDate(command, "endDt", import.Max.Date.GetValueOrDefault());
          
        db.SetString(command, "cspINumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingIncomeSource.Type1 = db.GetString(reader, 1);
        entities.ExistingIncomeSource.Name = db.GetNullableString(reader, 2);
        entities.ExistingIncomeSource.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.ExistingIncomeSource.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingIncomeSource.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingIncomeSource.CreatedBy = db.GetString(reader, 6);
        entities.ExistingIncomeSource.Code = db.GetNullableString(reader, 7);
        entities.ExistingIncomeSource.CspINumber = db.GetString(reader, 8);
        entities.ExistingIncomeSource.StartDt = db.GetNullableDate(reader, 9);
        entities.ExistingIncomeSource.EndDt = db.GetNullableDate(reader, 10);
        entities.ExistingIncomeSource.Note = db.GetNullableString(reader, 11);
        entities.ExistingIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.ExistingIncomeSource.Type1);
      });
  }

  private bool ReadInfrastructure()
  {
    entities.ExistingInfrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", local.Infrastructure.CaseNumber ?? "");
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "reasonCode", local.Infrastructure.ReasonCode);
        db.SetNullableString(
          command, "detail", local.Infrastructure.Detail ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 1);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingInfrastructure.Detail =
          db.GetNullableString(reader, 4);
        entities.ExistingInfrastructure.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.
          SetNullableString(command, "casINumber", entities.ExistingCase.Number);
          
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
    /// A value of AlertGenerationSkipFl.
    /// </summary>
    [JsonPropertyName("alertGenerationSkipFl")]
    public Common AlertGenerationSkipFl
    {
      get => alertGenerationSkipFl ??= new();
      set => alertGenerationSkipFl = value;
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
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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

    private Common iwoGenerationSkipFl;
    private Common alertGenerationSkipFl;
    private DateWorkArea max;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesTitleIi fcrSvesTitleIi;
    private Infrastructure infrastructure;
    private DateWorkArea processingDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of TotHistRecsCreated.
    /// </summary>
    [JsonPropertyName("totHistRecsCreated")]
    public Common TotHistRecsCreated
    {
      get => totHistRecsCreated ??= new();
      set => totHistRecsCreated = value;
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
    /// A value of TotHistExistsRecs.
    /// </summary>
    [JsonPropertyName("totHistExistsRecs")]
    public Common TotHistExistsRecs
    {
      get => totHistExistsRecs ??= new();
      set => totHistExistsRecs = value;
    }

    /// <summary>
    /// A value of TotArLetterRecs.
    /// </summary>
    [JsonPropertyName("totArLetterRecs")]
    public Common TotArLetterRecs
    {
      get => totArLetterRecs ??= new();
      set => totArLetterRecs = value;
    }

    /// <summary>
    /// A value of TotIwoRecs.
    /// </summary>
    [JsonPropertyName("totIwoRecs")]
    public Common TotIwoRecs
    {
      get => totIwoRecs ??= new();
      set => totIwoRecs = value;
    }

    private Common totAlertRecsCreated;
    private Common totHistRecsCreated;
    private Common totAlertExistsRecs;
    private Common totHistExistsRecs;
    private Common totArLetterRecs;
    private Common totIwoRecs;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of SvesPersonActiveFlag.
    /// </summary>
    [JsonPropertyName("svesPersonActiveFlag")]
    public Common SvesPersonActiveFlag
    {
      get => svesPersonActiveFlag ??= new();
      set => svesPersonActiveFlag = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of IwoGenerated.
    /// </summary>
    [JsonPropertyName("iwoGenerated")]
    public Common IwoGenerated
    {
      get => iwoGenerated ??= new();
      set => iwoGenerated = value;
    }

    /// <summary>
    /// A value of ValidChildFlag.
    /// </summary>
    [JsonPropertyName("validChildFlag")]
    public Common ValidChildFlag
    {
      get => validChildFlag ??= new();
      set => validChildFlag = value;
    }

    /// <summary>
    /// A value of IncsExists.
    /// </summary>
    [JsonPropertyName("incsExists")]
    public Common IncsExists
    {
      get => incsExists ??= new();
      set => incsExists = value;
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
    /// A value of NonEmployIncomeSourceAddress.
    /// </summary>
    [JsonPropertyName("nonEmployIncomeSourceAddress")]
    public NonEmployIncomeSourceAddress NonEmployIncomeSourceAddress
    {
      get => nonEmployIncomeSourceAddress ??= new();
      set => nonEmployIncomeSourceAddress = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of Document1.
    /// </summary>
    [JsonPropertyName("document1")]
    public Infrastructure Document1
    {
      get => document1 ??= new();
      set => document1 = value;
    }

    /// <summary>
    /// A value of PersonPlaysArRoleFlag.
    /// </summary>
    [JsonPropertyName("personPlaysArRoleFlag")]
    public Common PersonPlaysArRoleFlag
    {
      get => personPlaysArRoleFlag ??= new();
      set => personPlaysArRoleFlag = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of Document2.
    /// </summary>
    [JsonPropertyName("document2")]
    public Document Document2
    {
      get => document2 ??= new();
      set => document2 = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of IncomeSourceContact.
    /// </summary>
    [JsonPropertyName("incomeSourceContact")]
    public IncomeSourceContact IncomeSourceContact
    {
      get => incomeSourceContact ??= new();
      set => incomeSourceContact = value;
    }

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common svesPersonActiveFlag;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common iwoGenerated;
    private Common validChildFlag;
    private Common incsExists;
    private SpDocKey spDocKey;
    private NonEmployIncomeSourceAddress nonEmployIncomeSourceAddress;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private DateWorkArea null1;
    private DateWorkArea max;
    private Infrastructure infrastructure;
    private Infrastructure document1;
    private Common personPlaysArRoleFlag;
    private CaseFuncWorkSet caseFuncWorkSet;
    private ProgramProcessingInfo programProcessingInfo;
    private Common lcontrolMissingCh;
    private OutgoingDocument outgoingDocument;
    private Document document2;
    private DateWorkArea newestReview;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private EabConvertNumeric2 eabConvertNumeric;
    private EabReportSend neededToWrite;
    private IncomeSourceContact incomeSourceContact;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingFcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("existingFcrSvesGenInfo")]
    public FcrSvesGenInfo ExistingFcrSvesGenInfo
    {
      get => existingFcrSvesGenInfo ??= new();
      set => existingFcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of ExistingFcrSvesAddress.
    /// </summary>
    [JsonPropertyName("existingFcrSvesAddress")]
    public FcrSvesAddress ExistingFcrSvesAddress
    {
      get => existingFcrSvesAddress ??= new();
      set => existingFcrSvesAddress = value;
    }

    /// <summary>
    /// A value of ExistingIncomeSource.
    /// </summary>
    [JsonPropertyName("existingIncomeSource")]
    public IncomeSource ExistingIncomeSource
    {
      get => existingIncomeSource ??= new();
      set => existingIncomeSource = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
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
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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

    private Employer employer;
    private FcrSvesGenInfo existingFcrSvesGenInfo;
    private FcrSvesAddress existingFcrSvesAddress;
    private IncomeSource existingIncomeSource;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private CsePerson existingCsePerson;
    private Infrastructure existingInfrastructure;
    private CaseRole arCaseRole;
    private CsePerson arCsePerson;
    private CaseRole childCaseRole;
    private CsePerson childCsePerson;
    private InterstateRequest interstateRequest;
    private CsePersonAddress csePersonAddress;
    private GoodCause goodCause;
    private Infrastructure infrastructure;
  }
#endregion
}
