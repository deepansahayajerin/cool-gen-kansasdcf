// Program: LE_PROCESS_FDSO_CERTIFICATION, ID: 372665300, model: 746.
// Short name: SWE02393
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_PROCESS_FDSO_CERTIFICATION.
/// </para>
/// <para/>
/// </summary>
[Serializable]
public partial class LeProcessFdsoCertification: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_PROCESS_FDSO_CERTIFICATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeProcessFdsoCertification(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeProcessFdsoCertification.
  /// </summary>
  public LeProcessFdsoCertification(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // -------------------------------------------------------------------------
    // 07/25/2007  GVandy	PR313068	Initial Development for this action block.
    // 					Original logic was re-written to correctly send transactions
    // 					per case type (i.e. adc verses non-adc).
    // 11/01/2007  GVandy	PR00001375	Send a Decertify transaction to the Feds 
    // when a previously certified
    // 					obligors SSN is removed or changed to an invalid SSN.
    // -----------------------------------------------------------------------------------------------------------------
    export.EabReportSend.RptDetail = "Obligor " + import.CsePerson.Number + " - ";
      

    // -----------------------------------------------------------------------------------------------------
    // Read the obligor, need to know if there is family violence.
    // -----------------------------------------------------------------------------------------------------
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";
      export.EabReportSend.RptDetail =
        TrimEnd(export.EabReportSend.RptDetail) + " CSE_PERSON Not Found.";
      export.Abort.Flag = "Y";

      return;
    }

    // -----------------------------------------------------------------------------------------------------
    // Read for previous FDSO certification record, if one exists.
    // -----------------------------------------------------------------------------------------------------
    if (ReadFederalDebtSetoff())
    {
      if (Equal(entities.FederalDebtSetoff.DateSent, local.Null1.Date))
      {
        // -- Latest one found but it was not sent to the Feds yet.  Skip this 
        // person.
        return;
      }

      local.Old.Assign(entities.FederalDebtSetoff);
    }

    // -----------------------------------------------------------------------------------------------------
    // Check for a valid SSN.  The feds store certification info by SSN and case
    // type so the obligor must have a valid SSN.
    // -----------------------------------------------------------------------------------------------------
    UseCabValidateSsn();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();

      if (IsExitState("ADABAS_UNAVAILABLE_RB") || IsExitState
        ("ADABAS_READ_UNSUCCESSFUL") || IsExitState
        ("ADABAS_INVALID_RETURN_CODE"))
      {
        if (IsExitState("ADABAS_UNAVAILABLE_RB"))
        {
          export.Abort.Flag = "Y";
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " Fatal error returned from cab_validate_ssn cab... " +
            local.ExitStateWorkArea.Message;
        }

        return;
      }
      else
      {
        // -- 11/01/2007 GVandy  PR00001375  Reset the exit state and allow 
        // processing to continue which will cause
        // the Decertify transaction to be created.
        ExitState = "ACO_NN0000_ALL_OK";
      }
    }

    // ------------------------------------------------------------------------------------
    // The prep_federal_debt_setoff cab will return...
    // 	1. A flag indicating if an active bankruptcy prior to 2005-10-17 exists 
    // (LOCAL_BANKRUPT_PRIOR_TO_20051017 ief_supplied flag)
    // 	2. A flag indicating if an active bankruptcy after 2005-10-17 exists (
    // LOCAL_BANKRUPT_AFTER_20051017 ief_supplied flag)
    // 	3. A flag indicating if an existing adm exclusion was found (local_new 
    // federal_debt_setoff etype_adm_bankruptcy)
    // 	4. The expected discharge date for an active bankruptcy  (needed to pass
    // to the created_adm_exclusion cab)
    // 	5. Exported aging views (tanf and nontanf less than and greater than 90 
    // days) are no longer used.
    // 	6. The local_new federal_debt_setoff FDSO view will contain the total 
    // ADC and non-ADC amounts owed.
    // ------------------------------------------------------------------------------------
    UseCabPrepFederalDebtSetoff();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      export.EabReportSend.RptDetail =
        TrimEnd(export.EabReportSend.RptDetail) + " Error returned from prep_federal_debt_setoff cab... " +
        local.ExitStateWorkArea.Message;

      return;
    }

    // --  The following IF statement is for control purposes only.  The IF 
    // provides a way to escape without performing the remaining logic inside
    // the IF.
    local.ControlOnly.Flag = "Y";

    if (AsChar(local.ControlOnly.Flag) == 'Y')
    {
      // -----------------------------------------------------------------------------------------------------
      // --
      // 
      // --
      // --                                 D E L E 
      // T E   C A S E
      // 
      // --
      // --
      // 
      // --
      // --                                 (
      // Transaction Type 'D')
      // 
      // --
      // --
      // 
      // --
      // -----------------------------------------------------------------------------------------------------
      // -----------------------------------------------------------------------------------------------------
      // -- Determine if we need to send any Delete transactions to the feds.
      // -- If any of the conditions below are true we may need to send one.
      // -----------------------------------------------------------------------------------------------------
      if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator) && AsChar
        (import.FviApplication.Flag) == 'Y' || AsChar(local.ValidSsn.Flag) == 'N'
        || AsChar(local.BankruptCh13.Flag) == 'Y' || AsChar
        (local.BankruptPriorTo20051017.Flag) == 'Y' || local
        .New1.AdcAmount.GetValueOrDefault() == 0 && local
        .New1.NonAdcAmount.GetValueOrDefault() == 0)
      {
        if (AsChar(import.IncludeBankruptcy.Flag) != 'Y')
        {
          goto Test1;
        }

        if (local.Old.AdcAmount.GetValueOrDefault() > 0 || local
          .Old.NonAdcAmount.GetValueOrDefault() > 0)
        {
          // --------------------------------------------------------------------------------------
          // -- The obligor is currently FDSO certified but needs to be 
          // decertified.
          // --------------------------------------------------------------------------------------
          // -- Set the SSN and name to the SSN and name on the existing record.
          //    If the SSN or name has been changed we need to send the Delete 
          // using the old values.
          local.New1.Ssn = local.Old.Ssn;
          local.New1.LastName = local.Old.LastName;
          local.New1.FirstName = local.Old.FirstName;

          if (local.Old.AdcAmount.GetValueOrDefault() > 0 && local
            .Old.NonAdcAmount.GetValueOrDefault() > 0)
          {
            // -- We need to send Decertify transactions for Both ADC and non-
            // ADC.  Set the ttype_d_delete_certification to B (Both).
            local.New1.TtypeDDeleteCertification = "B";
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " ADC and Non-ADC Cases ";
              
          }
          else if (local.Old.AdcAmount.GetValueOrDefault() > 0)
          {
            // -- We need to send a Decertify transaction for ADC only.  Set the
            // ttype_d_delete_certification to A (ADC).
            local.New1.TtypeDDeleteCertification = "A";
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " ADC Case ";
          }
          else if (local.Old.NonAdcAmount.GetValueOrDefault() > 0)
          {
            // -- We need to send a Decertify transaction for Non-ADC only.  Set
            // the ttype_d_delete_certification to N (Non-ADC).
            local.New1.TtypeDDeleteCertification = "N";
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Non-ADC Case ";
          }

          local.New1.AdcAmount = 0;
          local.New1.NonAdcAmount = 0;
          local.New1.DecertifiedDate = import.ProgramProcessingInfo.ProcessDate;

          if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator) && AsChar
            (import.FviApplication.Flag) == 'Y')
          {
            local.New1.DecertificationReason = "Family Violence";
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Decertified due to Family Violence.";
              
          }
          else if (AsChar(local.ValidSsn.Flag) == 'N')
          {
            local.New1.DecertificationReason = "Invalid SSN";
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Decertified due to Invalid SSN.";
              
          }
          else if (AsChar(local.BankruptCh13.Flag) == 'Y')
          {
            local.New1.DecertificationReason = "Bankruptcy Chapter 13";
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Decertified due to Bankruptcy Chaoter 13.";
              
          }
          else if (AsChar(local.BankruptPriorTo20051017.Flag) == 'Y')
          {
            local.New1.DecertificationReason = "Pre 2005-10-17 Bankruptcy";
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Decertified due to Bankruptcy Prior to 10-17-2005.";
              
          }
          else if (local.New1.AdcAmount.GetValueOrDefault() == 0 && local
            .New1.NonAdcAmount.GetValueOrDefault() == 0)
          {
            local.New1.DecertificationReason = "Arrears Paid";
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Decertified due to Arrears Paid in Full.";
              
          }

          if (AsChar(local.Old.EtypeAdmBankrupt) == 'Y')
          {
            // -- Log a message to the bankruptcy report.
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "CSE person# " + import
              .CsePerson.Number + "       " + local
              .CsePersonsWorkSet.FormattedName + " SSN: " + local
              .CsePersonsWorkSet.Ssn + " is bankrupt, but no longer FDSO certified.";
              
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error encountered writing to Bankruptcy Report.  Return Status = " +
                local.EabFileHandling.Status;
              UseCabErrorReport();
              export.Abort.Flag = "Y";

              return;
            }

            local.New1.EtypeAdmBankrupt = "";

            // -- Remove any ADM exclusions.
            local.TextWorkArea.Text10 = "ADM_DELETE";
            UseLeAdmExclusionUpdate();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Abort.Flag = "Y";

              return;
            }
          }

          // -- No additional transactions will be created.  The obligor is 
          // completely Decertified from FDSO.
          // We create the Decertify transaction only when BOTH ADC and Non-ADC 
          // certifications are ended.
          // This is done to facilitate the display of the "FDSO Decert" 
          // indicator on OBLO.
          // If one case type is ending but the other remains open we send a 
          // Modify to $0 instead for the case type that is ending.
          // The feds treat a Modify to $0 the same as a decertification (i.e. 
          // they end the certification for that case type).
          goto Test4;
        }
        else
        {
          // -- Obligor is not currently FDSO certified.  We're done.  Return 
          // back to the Pstep.
          if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator) && AsChar
            (import.FviApplication.Flag) == 'Y')
          {
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Not certifiable.  Family Violence.";
              
          }
          else if (AsChar(local.ValidSsn.Flag) == 'N')
          {
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Not certifiable.  Invalid SSN.";
              
          }
          else if (AsChar(local.BankruptCh13.Flag) == 'Y')
          {
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Not certifiable.  Bankruptcy Chapter 13.";
              
          }
          else if (AsChar(local.BankruptPriorTo20051017.Flag) == 'Y')
          {
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Not certifiable.  Bankruptcy Prior to 10-17-2005.";
              
          }
          else if (local.New1.AdcAmount.GetValueOrDefault() == 0 && local
            .New1.NonAdcAmount.GetValueOrDefault() == 0)
          {
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Not certifiable.";
          }

          return;
        }
      }

Test1:

      // -----------------------------------------------------------------------------------------------------
      // -- If we reach this point we know that there is an amount currently 
      // owed.
      // -----------------------------------------------------------------------------------------------------
      // -----------------------------------------------------------------------------------------------------
      // --
      // 
      // --
      // --                                    A D D
      // C A S E
      // 
      // --
      // --
      // 
      // --
      // --                                 (
      // Transaction Type 'A')
      // 
      // --
      // --
      // 
      // --
      // -----------------------------------------------------------------------------------------------------
      // -----------------------------------------------------------------------------------------------------
      // -- Determine if we need to send any Add transactions to the feds.
      // -- If any of the conditions below are true we may need to send one.
      // -----------------------------------------------------------------------------------------------------
      if (local.Old.Ssn != local.New1.Ssn && entities
        .FederalDebtSetoff.Populated)
      {
        // -- Setting the change_ssn_ind will cause SWELB520 to also create a 
        // delete transaction for the old SSN.
        local.New1.ChangeSsnInd = "Y";
      }

      if (local.Old.AdcAmount.GetValueOrDefault() == 0 || local
        .Old.NonAdcAmount.GetValueOrDefault() == 0 || AsChar
        (local.New1.ChangeSsnInd) == 'Y')
      {
        if (local.Old.AdcAmount.GetValueOrDefault() == 0)
        {
          // -- The obligor will be newly certified for ADC if they meet the 
          // threshold amount.
          if (local.New1.AdcAmount.GetValueOrDefault() < 150)
          {
            // -- The obligor did not exceed the $150 threshold for ADC 
            // certification.  Set the adc amount back to $0.
            local.New1.AdcAmount = 0;
          }
          else
          {
            // -- The obligor can be certified for ADC.  We need to send an Add 
            // transaction for ADC.  Set the ttype_a_add_new_case to A (ADC).
            local.New1.TtypeAAddNewCase = "A";
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " ADC Case Added.";
          }
        }
        else if (AsChar(local.New1.ChangeSsnInd) == 'Y')
        {
          // -- The obligor is currently certified for ADC.  We need to send an 
          // Add transaction for ADC for the new SSN.
          //    Set the ttype_a_add_new_case to A (ADC).
          local.New1.TtypeAAddNewCase = "A";
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " ADC Case Added Due to SSN Change.";
            
        }

        if (local.Old.NonAdcAmount.GetValueOrDefault() == 0)
        {
          // -- The obligor will be newly certified for Non-ADC if they meet the
          // threshold amount.
          if (local.New1.NonAdcAmount.GetValueOrDefault() < 500)
          {
            // -- The obligor did not exceed the $500 threshold for non ADC 
            // certification.  Set the non adc amount back to $0.
            local.New1.NonAdcAmount = 0;
          }
          else
          {
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Non-ADC Case Added.";

            if (IsEmpty(local.New1.TtypeAAddNewCase))
            {
              // -- We need to send an Add transaction for Non-ADC only.  Set 
              // the ttype_a_add_new_case to N (Non-ADC).
              local.New1.TtypeAAddNewCase = "N";
            }
            else
            {
              // -- We need to send Add transactions for Both ADC and non-ADC.  
              // Set the ttype_a_add_new_case to B (Both).
              // (i.e. we already set the ttype_a_add_new_case to "A" above to 
              // certify for ADC, now we want to also certify
              // for Non ADC so we need to set the ttype_a_add_new_case 
              // attribute to "B")
              local.New1.TtypeAAddNewCase = "B";
            }
          }
        }
        else if (AsChar(local.New1.ChangeSsnInd) == 'Y')
        {
          // -- The obligor is currently certified for Non-ADC.  We need to send
          // an Add transaction for the new SSN.
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " Non-ADC Case Added Due to SSN Change.";
            

          if (IsEmpty(local.New1.TtypeAAddNewCase))
          {
            // -- We need to send an Add transaction for Non-ADC only.  Set the 
            // ttype_a_add_new_case to N (Non-ADC).
            local.New1.TtypeAAddNewCase = "N";
          }
          else
          {
            // -- We need to send Add transactions for Both ADC and non-ADC.  
            // Set the ttype_a_add_new_case to B (Both).
            // (i.e. we already set the ttype_a_add_new_case to "A" above to 
            // certify for ADC, now we want to also certify
            // for Non ADC so we need to set the ttype_a_add_new_case attribute 
            // to "B")
            local.New1.TtypeAAddNewCase = "B";
          }
        }

        if (IsEmpty(local.New1.TtypeAAddNewCase))
        {
          // -- Not adding any new certifications for this obligor.
          if (local.Old.AdcAmount.GetValueOrDefault() == 0 && local
            .Old.NonAdcAmount.GetValueOrDefault() == 0)
          {
            // -- The obligor is not currently certified so we don't need to 
            // check for any changes to an existing certification.
            //    We are done.  Return back to the PStep.
            export.EabReportSend.RptDetail =
              TrimEnd(export.EabReportSend.RptDetail) + " Obligor cannot be FDSO certified.  Arrears do not meet the threshold amounts.";
              

            return;
          }
          else
          {
            // -- Still need to check if any Modification transactions need to 
            // be sent.
            goto Test2;
          }
        }

        if (AsChar(local.BankruptAfter20051017.Flag) == 'Y' && AsChar
          (local.New1.EtypeAdmBankrupt) != 'Y' || AsChar
          (local.BankruptCh13.Flag) == 'Y' && AsChar
          (import.IncludeBankruptcy.Flag) != 'Y')
        {
          local.New1.EtypeAdmBankrupt = "Y";

          // --  Add ADM exclusion(s)
          local.TextWorkArea.Text10 = "ADM_ADD";
          UseLeAdmExclusionUpdate();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
        else if (AsChar(local.BankruptAfter20051017.Flag) != 'Y' && AsChar
          (local.New1.EtypeAdmBankrupt) == 'Y')
        {
          local.New1.EtypeAdmBankrupt = "";

          // --  Remove the ADM exclusion(s)
          local.TextWorkArea.Text10 = "ADM_DELETE";
          UseLeAdmExclusionUpdate();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }

        if (AsChar(local.New1.ChangeSsnInd) == 'Y')
        {
          // -- We are adding new certification(s) due to a SSN change.  No need
          // to continue looking for changes to the existing certifications.
          // -- A Delete transaction for the old certifications will be 
          // generated in SWELB520 as a result of the changed_ssn_ind being set.
          goto Test4;
        }
      }

Test2:

      // -----------------------------------------------------------------------------------------------------
      // --
      // 
      // --
      // --                                  N A M 
      // E   C H A N G E
      // 
      // --
      // --
      // 
      // --
      // --                                 (
      // Transaction Type 'B')
      // 
      // --
      // --
      // 
      // --
      // -----------------------------------------------------------------------------------------------------
      if (!Equal(local.Old.FirstName, local.New1.FirstName) || !
        Equal(local.Old.LastName, local.New1.LastName))
      {
        if (!IsEmpty(local.New1.TtypeAAddNewCase))
        {
          // -- The new name will be sent on the add transaction and will 
          // replace the name on any other case types.
          //    No need to send the name change transaction.
          goto Test3;
        }

        export.EabReportSend.RptDetail =
          TrimEnd(export.EabReportSend.RptDetail) + " Name Change.";

        // -- Only need to send one name change transaction to the feds.  They 
        // will update the name on all case types.
        if (local.Old.AdcAmount.GetValueOrDefault() > 0)
        {
          local.New1.TtypeBNameChange = "A";
        }
        else if (local.Old.NonAdcAmount.GetValueOrDefault() > 0)
        {
          local.New1.TtypeBNameChange = "N";
        }
      }

Test3:

      // -----------------------------------------------------------------------------------------------------
      // --
      // 
      // --
      // --                           L O C A 
      // L   C O D E   C H A N G E
      // 
      // --
      // --
      // 
      // --
      // --                                 (
      // Transaction Type 'L')
      // 
      // --
      // --
      // 
      // --
      // -----------------------------------------------------------------------------------------------------
      if (!Equal(local.Old.LocalCode, local.New1.LocalCode))
      {
        // -- Send a Local Code Change transaction for each applicable case 
        // type.
        if (local.Old.AdcAmount.GetValueOrDefault() > 0 && local
          .Old.NonAdcAmount.GetValueOrDefault() > 0)
        {
          local.New1.TtypeLChangeSubmittingState = "B";
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " ADC and Non-ADC Local Code Changes.";
            
        }
        else if (local.Old.AdcAmount.GetValueOrDefault() > 0)
        {
          local.New1.TtypeLChangeSubmittingState = "A";
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " ADC Local Code Change.";
            
        }
        else if (local.Old.NonAdcAmount.GetValueOrDefault() > 0)
        {
          local.New1.TtypeLChangeSubmittingState = "N";
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " Non-ADC Local Code Change.";
            
        }
      }

      // -----------------------------------------------------------------------------------------------------
      // --
      // 
      // --
      // --                      M O D 
      // I F Y   A R R E A R A G E   A
      // M O U N T
      // 
      // --
      // --
      // 
      // --
      // --                                 (
      // Transaction Type 'M')
      // 
      // --
      // --
      // 
      // --
      // -----------------------------------------------------------------------------------------------------
      // -- Send a Modify Arrearage Amount transaction for each applicable case 
      // type.
      if (local.New1.AdcAmount.GetValueOrDefault() > 0 && local
        .New1.AdcAmount.GetValueOrDefault() < 25)
      {
        // -- Always certify for a minimum of $25.
        local.New1.AdcAmount = 25;
      }

      if (local.Old.AdcAmount.GetValueOrDefault() > 0 && local
        .Old.AdcAmount.GetValueOrDefault() != local
        .New1.AdcAmount.GetValueOrDefault())
      {
        // -- We need to send a Modify transaction for ADC.  Set the 
        // ttype_m_modify_amount to A (ADC).
        local.New1.TtypeMModifyAmount = "A";
        export.EabReportSend.RptDetail =
          TrimEnd(export.EabReportSend.RptDetail) + " ADC";
      }

      if (local.New1.NonAdcAmount.GetValueOrDefault() > 0 && local
        .New1.NonAdcAmount.GetValueOrDefault() < 25)
      {
        // -- Always certify for a minimum of $25.
        local.New1.NonAdcAmount = 25;
      }

      if (local.Old.NonAdcAmount.GetValueOrDefault() > 0 && local
        .Old.NonAdcAmount.GetValueOrDefault() != local
        .New1.NonAdcAmount.GetValueOrDefault())
      {
        if (IsEmpty(local.New1.TtypeMModifyAmount))
        {
          // -- We need to send a Modify transaction for Non-ADC only.  Set the 
          // ttype_m_modify_amount to N (Non-ADC).
          local.New1.TtypeMModifyAmount = "N";
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " Non-ADC";
        }
        else
        {
          // -- We need to send Modify transactions for Both ADC and non-ADC.  
          // Set the ttype_m_modify_amount to B (Both).
          // (i.e. we already set the ttype_m_modify_amount to "A" above to 
          // certify for ADC, now we want to also certify
          // for Non ADC so we need to set the ttype_m_modify_amount attribute 
          // to "B")
          local.New1.TtypeMModifyAmount = "B";
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " and Non-ADC";
        }
      }

      if (!IsEmpty(local.New1.TtypeMModifyAmount))
      {
        export.EabReportSend.RptDetail =
          TrimEnd(export.EabReportSend.RptDetail) + " Arrearage Modification.";
      }

      // -----------------------------------------------------------------------------------------------------
      // --
      // 
      // --
      // --                 R E P L A C E   E X C L U S I O N   I N D I C A T O 
      // R S                         --
      // --
      // 
      // --
      // --                                 (
      // Transaction Type 'R')
      // 
      // --
      // --
      // 
      // --
      // -----------------------------------------------------------------------------------------------------
      if (AsChar(local.Old.EtypeAdmBankrupt) != AsChar
        (local.New1.EtypeAdmBankrupt) || AsChar
        (local.Old.EtypeAdministrativeOffset) != AsChar
        (local.New1.EtypeAdministrativeOffset) || AsChar
        (local.Old.EtypeFederalRetirement) != AsChar
        (local.New1.EtypeFederalRetirement) || AsChar
        (local.Old.EtypeFederalSalary) != AsChar
        (local.New1.EtypeFederalSalary) || AsChar
        (local.Old.EtypeFinancialInstitution) != AsChar
        (local.New1.EtypeFinancialInstitution) || AsChar
        (local.Old.EtypePassportDenial) != AsChar
        (local.New1.EtypePassportDenial) || AsChar
        (local.Old.EtypeTaxRefund) != AsChar(local.New1.EtypeTaxRefund) || AsChar
        (local.Old.EtypeVendorPaymentOrMisc) != AsChar
        (local.New1.EtypeVendorPaymentOrMisc) || AsChar
        (local.New1.EtypeAdmBankrupt) != 'Y' && AsChar
        (local.BankruptAfter20051017.Flag) == 'Y' || AsChar
        (local.New1.EtypeAdmBankrupt) == 'Y' && AsChar
        (local.BankruptAfter20051017.Flag) != 'Y' || AsChar
        (local.BankruptCh13.Flag) == 'Y' && AsChar
        (import.IncludeBankruptcy.Flag) != 'Y')
      {
        // -- Send a Replace Exclusion Indicator transaction for each applicable
        // case type.
        if (local.Old.AdcAmount.GetValueOrDefault() > 0 && local
          .Old.NonAdcAmount.GetValueOrDefault() > 0)
        {
          local.New1.TtypeRModifyExclusion = "B";
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " ADC and Non-ADC Exclusion Indicator Changes.";
            
        }
        else if (local.Old.AdcAmount.GetValueOrDefault() > 0)
        {
          local.New1.TtypeRModifyExclusion = "A";
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " ADC Exclusion Indicator Change.";
            
        }
        else if (local.Old.NonAdcAmount.GetValueOrDefault() > 0)
        {
          local.New1.TtypeRModifyExclusion = "N";
          export.EabReportSend.RptDetail =
            TrimEnd(export.EabReportSend.RptDetail) + " Non-ADC Exclusion Indicator Change.";
            
        }

        if (AsChar(local.BankruptAfter20051017.Flag) == 'Y' && AsChar
          (local.New1.EtypeAdmBankrupt) != 'Y' || AsChar
          (import.IncludeBankruptcy.Flag) != 'Y' && AsChar
          (local.BankruptCh13.Flag) == 'Y')
        {
          local.New1.EtypeAdmBankrupt = "Y";

          // --  Add ADM exclusion(s)
          local.TextWorkArea.Text10 = "ADM_ADD";
          UseLeAdmExclusionUpdate();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
        else if (AsChar(local.BankruptAfter20051017.Flag) != 'Y' && AsChar
          (local.New1.EtypeAdmBankrupt) == 'Y')
        {
          local.New1.EtypeAdmBankrupt = "";

          // --  Remove the ADM exclusion(s)
          local.TextWorkArea.Text10 = "ADM_DELETE";
          UseLeAdmExclusionUpdate();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }

      // -----------------------------------------------------------------------------------------------------
      // --
      // 
      // --
      // --                               S T A T
      // E    P A Y M E N T
      // 
      // --
      // --
      // 
      // --
      // --                                 (
      // Transaction Type 'S')
      // 
      // --
      // --
      // 
      // --
      // -----------------------------------------------------------------------------------------------------
      // --  This transaction type is handled by SWEFB670.
      // -----------------------------------------------------------------------------------------------------
      // --
      // 
      // --
      // --             T R A N S F E R   F O R   A D M I N I S T R A T I V E   
      // R E V I E W                 --
      // --
      // 
      // --
      // --                                 (
      // Transaction Type 'T')
      // 
      // --
      // --
      // 
      // --
      // -----------------------------------------------------------------------------------------------------
      // --  KESSEP does not support this transaction type.
      // -----------------------------------------------------------------------------------------------------
      // --
      // 
      // --
      // --                              A D D R
      // E S S    C H A N G E
      // 
      // --
      // --
      // 
      // --
      // --                                 (
      // Transaction Type 'Z')
      // 
      // --
      // --
      // 
      // --
      // -----------------------------------------------------------------------------------------------------
      if (!Equal(local.Old.AddressStreet1, local.New1.AddressStreet1) || !
        Equal(local.Old.AddressStreet2, local.New1.AddressStreet2) || !
        Equal(local.Old.AddressCity, local.New1.AddressCity) || !
        Equal(local.Old.AddressState, local.New1.AddressState) || !
        Equal(local.Old.AddressZip, local.New1.AddressZip))
      {
        if (!IsEmpty(local.New1.TtypeAAddNewCase))
        {
          // -- The new address will be sent on the add transaction and will 
          // replace the address on any other case types.
          //    No need to send the address change transaction.
          goto Test4;
        }

        export.EabReportSend.RptDetail =
          TrimEnd(export.EabReportSend.RptDetail) + " Address Change.";

        // -- Only need to send one address change transaction to the feds.  
        // They will update the address on all case types.
        if (local.Old.AdcAmount.GetValueOrDefault() > 0)
        {
          local.New1.TtypeZAddressChange = "A";
        }
        else if (local.Old.NonAdcAmount.GetValueOrDefault() > 0)
        {
          local.New1.TtypeZAddressChange = "N";
        }
      }
    }

Test4:

    if (IsEmpty(local.New1.TtypeAAddNewCase) && IsEmpty
      (local.New1.TtypeBNameChange) && IsEmpty
      (local.New1.TtypeDDeleteCertification) && IsEmpty
      (local.New1.TtypeLChangeSubmittingState) && IsEmpty
      (local.New1.TtypeMModifyAmount) && IsEmpty
      (local.New1.TtypeRModifyExclusion) && IsEmpty
      (local.New1.TtypeZAddressChange))
    {
      // -- No transactions are to be sent to the feds.  Return to the PStep.
      export.EabReportSend.RptDetail =
        TrimEnd(export.EabReportSend.RptDetail) + " No changes to existing certification.";
        

      return;
    }

    // -- The attributes below are set for consistency with the prior version of
    // FDSO certification.
    //    The attributes don't serve any purpose and could probably be removed 
    // from the datamodel.
    local.New1.CurrentAmount = local.New1.AdcAmount.GetValueOrDefault() + local
      .New1.NonAdcAmount.GetValueOrDefault();
    local.New1.CurrentAmountDate = import.ProgramProcessingInfo.ProcessDate;
    local.New1.AmountOwed = (int)local.New1.CurrentAmount.GetValueOrDefault();

    if (IsEmpty(local.New1.TtypeAAddNewCase))
    {
      local.New1.OriginalAmount = local.Old.OriginalAmount.GetValueOrDefault();
      local.New1.CaseType = local.Old.CaseType;
    }
    else
    {
      local.New1.OriginalAmount = local.New1.CurrentAmount.GetValueOrDefault();

      if (AsChar(local.New1.TtypeAAddNewCase) == 'B')
      {
        local.New1.CaseType = "A";
      }
      else
      {
        local.New1.CaseType = local.New1.TtypeAAddNewCase ?? Spaces(1);
      }
    }

    local.New1.TanfCode = local.New1.CaseType;

    // --  Create the FDSO record.
    UseCreateFederalDebtSetoff();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- The export_num_fdso_recs_created view is set to import also (i.e. it
      // is passed by reference)
      ++export.NumFdsoRecsCreated.Count;
    }
    else
    {
      export.EabReportSend.RptDetail =
        TrimEnd(export.EabReportSend.RptDetail) + "  Error creating a new federal debt setoff record.";
        
      export.Abort.Flag = "Y";
    }
  }

  private static void MoveAdministrativeActCertification1(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.TakenDate = source.TakenDate;
    target.EtypeAdmBankrupt = source.EtypeAdmBankrupt;
    target.AdcAmount = source.AdcAmount;
    target.NonAdcAmount = source.NonAdcAmount;
    target.LocalCode = source.LocalCode;
    target.Ssn = source.Ssn;
    target.CaseNumber = source.CaseNumber;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.ProcessYear = source.ProcessYear;
    target.EtypeAdministrativeOffset = source.EtypeAdministrativeOffset;
    target.EtypeFederalRetirement = source.EtypeFederalRetirement;
    target.EtypeFederalSalary = source.EtypeFederalSalary;
    target.EtypeTaxRefund = source.EtypeTaxRefund;
    target.EtypeVendorPaymentOrMisc = source.EtypeVendorPaymentOrMisc;
    target.EtypePassportDenial = source.EtypePassportDenial;
    target.EtypeFinancialInstitution = source.EtypeFinancialInstitution;
    target.AddressStreet1 = source.AddressStreet1;
    target.AddressStreet2 = source.AddressStreet2;
    target.AddressCity = source.AddressCity;
    target.AddressState = source.AddressState;
    target.AddressZip = source.AddressZip;
  }

  private static void MoveAdministrativeActCertification2(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.TanfCode = source.TanfCode;
    target.TakenDate = source.TakenDate;
    target.OriginalAmount = source.OriginalAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.CurrentAmountDate = source.CurrentAmountDate;
    target.DecertifiedDate = source.DecertifiedDate;
    target.NotificationDate = source.NotificationDate;
    target.NotifiedBy = source.NotifiedBy;
    target.ChangeSsnInd = source.ChangeSsnInd;
    target.EtypeAdmBankrupt = source.EtypeAdmBankrupt;
    target.DecertificationReason = source.DecertificationReason;
    target.AdcAmount = source.AdcAmount;
    target.NonAdcAmount = source.NonAdcAmount;
    target.InjuredSpouseDate = source.InjuredSpouseDate;
    target.LocalCode = source.LocalCode;
    target.Ssn = source.Ssn;
    target.CaseNumber = source.CaseNumber;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.AmountOwed = source.AmountOwed;
    target.CaseType = source.CaseType;
    target.TransferState = source.TransferState;
    target.LocalForTransfer = source.LocalForTransfer;
    target.ProcessYear = source.ProcessYear;
    target.TtypeAAddNewCase = source.TtypeAAddNewCase;
    target.TtypeDDeleteCertification = source.TtypeDDeleteCertification;
    target.TtypeLChangeSubmittingState = source.TtypeLChangeSubmittingState;
    target.TtypeMModifyAmount = source.TtypeMModifyAmount;
    target.TtypeRModifyExclusion = source.TtypeRModifyExclusion;
    target.EtypeAdministrativeOffset = source.EtypeAdministrativeOffset;
    target.EtypeFederalRetirement = source.EtypeFederalRetirement;
    target.EtypeFederalSalary = source.EtypeFederalSalary;
    target.EtypeTaxRefund = source.EtypeTaxRefund;
    target.EtypeVendorPaymentOrMisc = source.EtypeVendorPaymentOrMisc;
    target.EtypePassportDenial = source.EtypePassportDenial;
    target.EtypeFinancialInstitution = source.EtypeFinancialInstitution;
    target.TtypeBNameChange = source.TtypeBNameChange;
    target.TtypeZAddressChange = source.TtypeZAddressChange;
    target.AddressStreet1 = source.AddressStreet1;
    target.AddressStreet2 = source.AddressStreet2;
    target.AddressCity = source.AddressCity;
    target.AddressState = source.AddressState;
    target.AddressZip = source.AddressZip;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabPrepFederalDebtSetoff()
  {
    var useImport = new CabPrepFederalDebtSetoff.Import();
    var useExport = new CabPrepFederalDebtSetoff.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Obligor.Assign(local.CsePersonsWorkSet);

    Call(CabPrepFederalDebtSetoff.Execute, useImport, useExport);

    local.BankruptAfter20051017.Flag = useExport.BankrptAfter20051017.Flag;
    local.BankruptPriorTo20051017.Flag = useExport.BankrptPriorTo20051017.Flag;
    local.Bankruptcy.ExpectedBkrpDischargeDate =
      useExport.Bankruptcy.ExpectedBkrpDischargeDate;
    MoveAdministrativeActCertification1(useExport.FederalDebtSetoff, local.New1);
      
    local.BankruptCh13.Flag = useExport.BankruptCh13.Flag;
  }

  private void UseCabValidateSsn()
  {
    var useImport = new CabValidateSsn.Import();
    var useExport = new CabValidateSsn.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(CabValidateSsn.Execute, useImport, useExport);

    local.ValidSsn.Flag = useExport.ValidSsn.Flag;
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCreateFederalDebtSetoff()
  {
    var useImport = new CreateFederalDebtSetoff.Import();
    var useExport = new CreateFederalDebtSetoff.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveAdministrativeActCertification2(local.New1, useImport.Export1);

    Call(CreateFederalDebtSetoff.Execute, useImport, useExport);

    local.New1.Assign(useImport.Export1);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseLeAdmExclusionUpdate()
  {
    var useImport = new LeAdmExclusionUpdate.Import();
    var useExport = new LeAdmExclusionUpdate.Export();

    useImport.Obligor.Assign(local.CsePersonsWorkSet);
    useImport.Bankruptcy.ExpectedBkrpDischargeDate =
      local.Bankruptcy.ExpectedBkrpDischargeDate;
    useImport.AdmBankruptyStatus.Text10 = local.TextWorkArea.Text10;

    Call(LeAdmExclusionUpdate.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadFederalDebtSetoff()
  {
    entities.FederalDebtSetoff.Populated = false;

    return Read("ReadFederalDebtSetoff",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.FederalDebtSetoff.CpaType = db.GetString(reader, 0);
        entities.FederalDebtSetoff.CspNumber = db.GetString(reader, 1);
        entities.FederalDebtSetoff.Type1 = db.GetString(reader, 2);
        entities.FederalDebtSetoff.TakenDate = db.GetDate(reader, 3);
        entities.FederalDebtSetoff.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.FederalDebtSetoff.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.FederalDebtSetoff.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.FederalDebtSetoff.DecertifiedDate =
          db.GetNullableDate(reader, 7);
        entities.FederalDebtSetoff.NotificationDate =
          db.GetNullableDate(reader, 8);
        entities.FederalDebtSetoff.CreatedBy = db.GetString(reader, 9);
        entities.FederalDebtSetoff.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.FederalDebtSetoff.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.FederalDebtSetoff.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 12);
        entities.FederalDebtSetoff.AdcAmount =
          db.GetNullableDecimal(reader, 13);
        entities.FederalDebtSetoff.NonAdcAmount =
          db.GetNullableDecimal(reader, 14);
        entities.FederalDebtSetoff.InjuredSpouseDate =
          db.GetNullableDate(reader, 15);
        entities.FederalDebtSetoff.NotifiedBy =
          db.GetNullableString(reader, 16);
        entities.FederalDebtSetoff.DateSent = db.GetNullableDate(reader, 17);
        entities.FederalDebtSetoff.EtypeAdministrativeOffset =
          db.GetNullableString(reader, 18);
        entities.FederalDebtSetoff.LocalCode = db.GetNullableString(reader, 19);
        entities.FederalDebtSetoff.Ssn = db.GetInt32(reader, 20);
        entities.FederalDebtSetoff.CaseNumber = db.GetString(reader, 21);
        entities.FederalDebtSetoff.LastName = db.GetString(reader, 22);
        entities.FederalDebtSetoff.FirstName = db.GetString(reader, 23);
        entities.FederalDebtSetoff.AmountOwed = db.GetInt32(reader, 24);
        entities.FederalDebtSetoff.TtypeAAddNewCase =
          db.GetNullableString(reader, 25);
        entities.FederalDebtSetoff.CaseType = db.GetString(reader, 26);
        entities.FederalDebtSetoff.TransferState =
          db.GetNullableString(reader, 27);
        entities.FederalDebtSetoff.LocalForTransfer =
          db.GetNullableInt32(reader, 28);
        entities.FederalDebtSetoff.ProcessYear =
          db.GetNullableInt32(reader, 29);
        entities.FederalDebtSetoff.TanfCode = db.GetString(reader, 30);
        entities.FederalDebtSetoff.TtypeDDeleteCertification =
          db.GetNullableString(reader, 31);
        entities.FederalDebtSetoff.TtypeLChangeSubmittingState =
          db.GetNullableString(reader, 32);
        entities.FederalDebtSetoff.TtypeMModifyAmount =
          db.GetNullableString(reader, 33);
        entities.FederalDebtSetoff.TtypeRModifyExclusion =
          db.GetNullableString(reader, 34);
        entities.FederalDebtSetoff.TtypeBNameChange =
          db.GetNullableString(reader, 35);
        entities.FederalDebtSetoff.TtypeZAddressChange =
          db.GetNullableString(reader, 36);
        entities.FederalDebtSetoff.EtypeFederalRetirement =
          db.GetNullableString(reader, 37);
        entities.FederalDebtSetoff.EtypeFederalSalary =
          db.GetNullableString(reader, 38);
        entities.FederalDebtSetoff.EtypeTaxRefund =
          db.GetNullableString(reader, 39);
        entities.FederalDebtSetoff.EtypeVendorPaymentOrMisc =
          db.GetNullableString(reader, 40);
        entities.FederalDebtSetoff.EtypePassportDenial =
          db.GetNullableString(reader, 41);
        entities.FederalDebtSetoff.EtypeFinancialInstitution =
          db.GetNullableString(reader, 42);
        entities.FederalDebtSetoff.ReturnStatus =
          db.GetNullableString(reader, 43);
        entities.FederalDebtSetoff.ReturnStatusDate =
          db.GetNullableDate(reader, 44);
        entities.FederalDebtSetoff.ChangeSsnInd =
          db.GetNullableString(reader, 45);
        entities.FederalDebtSetoff.EtypeAdmBankrupt =
          db.GetNullableString(reader, 46);
        entities.FederalDebtSetoff.DecertificationReason =
          db.GetNullableString(reader, 47);
        entities.FederalDebtSetoff.AddressStreet1 =
          db.GetNullableString(reader, 48);
        entities.FederalDebtSetoff.AddressStreet2 =
          db.GetNullableString(reader, 49);
        entities.FederalDebtSetoff.AddressCity =
          db.GetNullableString(reader, 50);
        entities.FederalDebtSetoff.AddressState =
          db.GetNullableString(reader, 51);
        entities.FederalDebtSetoff.AddressZip =
          db.GetNullableString(reader, 52);
        entities.FederalDebtSetoff.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.FederalDebtSetoff.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.FederalDebtSetoff.Type1);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of IncludeBankruptcy.
    /// </summary>
    [JsonPropertyName("includeBankruptcy")]
    public Common IncludeBankruptcy
    {
      get => includeBankruptcy ??= new();
      set => includeBankruptcy = value;
    }

    /// <summary>
    /// A value of FviApplication.
    /// </summary>
    [JsonPropertyName("fviApplication")]
    public Common FviApplication
    {
      get => fviApplication ??= new();
      set => fviApplication = value;
    }

    private CsePerson csePerson;
    private ProgramProcessingInfo programProcessingInfo;
    private Common includeBankruptcy;
    private Common fviApplication;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    /// <summary>
    /// A value of NumFdsoRecsCreated.
    /// </summary>
    [JsonPropertyName("numFdsoRecsCreated")]
    public Common NumFdsoRecsCreated
    {
      get => numFdsoRecsCreated ??= new();
      set => numFdsoRecsCreated = value;
    }

    private EabReportSend eabReportSend;
    private Common abort;
    private Common numFdsoRecsCreated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BankruptCh13.
    /// </summary>
    [JsonPropertyName("bankruptCh13")]
    public Common BankruptCh13
    {
      get => bankruptCh13 ??= new();
      set => bankruptCh13 = value;
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
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public AdministrativeActCertification Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of ValidSsn.
    /// </summary>
    [JsonPropertyName("validSsn")]
    public Common ValidSsn
    {
      get => validSsn ??= new();
      set => validSsn = value;
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
    /// A value of BankruptAfter20051017.
    /// </summary>
    [JsonPropertyName("bankruptAfter20051017")]
    public Common BankruptAfter20051017
    {
      get => bankruptAfter20051017 ??= new();
      set => bankruptAfter20051017 = value;
    }

    /// <summary>
    /// A value of BankruptPriorTo20051017.
    /// </summary>
    [JsonPropertyName("bankruptPriorTo20051017")]
    public Common BankruptPriorTo20051017
    {
      get => bankruptPriorTo20051017 ??= new();
      set => bankruptPriorTo20051017 = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public AdministrativeActCertification New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of ControlOnly.
    /// </summary>
    [JsonPropertyName("controlOnly")]
    public Common ControlOnly
    {
      get => controlOnly ??= new();
      set => controlOnly = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private Common bankruptCh13;
    private DateWorkArea null1;
    private AdministrativeActCertification old;
    private Common validSsn;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common bankruptAfter20051017;
    private Common bankruptPriorTo20051017;
    private Bankruptcy bankruptcy;
    private AdministrativeActCertification new1;
    private ExitStateWorkArea exitStateWorkArea;
    private Common controlOnly;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private TextWorkArea textWorkArea;
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
    /// A value of FederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("federalDebtSetoff")]
    public AdministrativeActCertification FederalDebtSetoff
    {
      get => federalDebtSetoff ??= new();
      set => federalDebtSetoff = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private CsePerson csePerson;
    private AdministrativeActCertification federalDebtSetoff;
    private CsePersonAccount obligor;
  }
#endregion
}
