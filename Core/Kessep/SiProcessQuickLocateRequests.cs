// Program: SI_PROCESS_QUICK_LOCATE_REQUESTS, ID: 372424796, model: 746.
// Short name: SWE02285
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_PROCESS_QUICK_LOCATE_REQUESTS.
/// </summary>
[Serializable]
public partial class SiProcessQuickLocateRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PROCESS_QUICK_LOCATE_REQUESTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiProcessQuickLocateRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiProcessQuickLocateRequests.
  /// </summary>
  public SiProcessQuickLocateRequests(IContext context, Import import,
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
    // -----------------------------------------------------------------------------
    //         M A I N T E N A N C E   L O G
    // -----------------------------------------------------------------------------
    // Date		Developer	Request  Description
    // -----------------------------------------------------------------------------
    // 12/30/1998	Carl Ott		 Initial Dev.
    // 08/27/1999	C. Ott		1026	 Fixed a READ statement to remove
    //                                          
    // CURRENT case.
    // 09/24/1999	C. Ott		75274	 Fixed several READ statements to
    //                                          
    // remove CURRENT cse person.
    // 01/07/2000	C. Ott		84205	 Modified for subsequent requests
    //                                          
    // from the same state for the same SSN.
    // 04/13/2000	C. Scroggins	WR 162	 Modified for Family Violence to space
    //                                          
    // out address and phone number fields of
    //                                          
    // the AP ID Data Block if the individual
    //                                          
    // is tagged for Family Violence.
    // 01/31/2001	M Ramirez	106461	 Arrears only case's transactions err,
    //                                          
    // due to case type not being populated.
    // 04/06/2001      E Lyman         115484   Fixed logic that handled Alias.
    // 06/27/2001	M Ramirez	116637	Employer confirmed indicator missing
    // 06/27/2001	M Ramirez	none	Misc changes to improve performance and results
    // 04/23/2002	M Ashworth	142092	Mondatory Case Data Block missing.
    // -----------------------------------------------------------------------------
    if (Equal(import.BatchProcess.Date, local.Null1.Date))
    {
      local.Current.Date = Now().Date;
    }
    else
    {
      local.Current.Date = import.BatchProcess.Date;
      local.Batch.Flag = "Y";
    }

    local.VerifiedSince.Date = AddYears(local.Current.Date, -1);

    if (ReadInterstateApIdentification())
    {
      export.InterstateApIdentification.Ssn =
        entities.InterstateApIdentification.Ssn;

      if (!IsEmpty(entities.InterstateApIdentification.Ssn) && !
        Equal(entities.InterstateApIdentification.Ssn, "000000000"))
      {
      }
      else if (!IsEmpty(entities.InterstateApIdentification.AliasSsn1) && !
        Equal(entities.InterstateApIdentification.AliasSsn1, "000000000"))
      {
      }
      else if (!IsEmpty(entities.InterstateApIdentification.AliasSsn2) && !
        Equal(entities.InterstateApIdentification.AliasSsn2, "000000000"))
      {
      }
      else
      {
        local.InterstateCase.Assign(import.InterstateCase);
        local.InterstateApIdentification.Assign(
          entities.InterstateApIdentification);
        UseSiGenCsenetTransactSerialNo();
        local.InterstateCase.ActionReasonCode = "LUALL";
        local.InterstateCase.ActionCode = "P";
        local.InterstateCase.CaseDataInd = 1;
        local.InterstateCase.ApIdentificationInd = 1;
        local.InterstateCase.ApLocateDataInd = 0;
        local.InterstateCase.ParticipantDataInd = 0;
        local.InterstateCase.OrderDataInd = 0;
        local.InterstateCase.CollectionDataInd = 0;
        local.InterstateCase.InformationInd = 0;
        local.InterstateCase.AttachmentsInd = "N";

        goto Read;
      }

      local.InterstateCase.ActionCode = "P";
      local.InterstateCase.ActionReasonCode = "";
      local.InterstateCase.FunctionalTypeCode = "LO1";
      local.InterstateCase.InterstateCaseId =
        import.InterstateCase.InterstateCaseId ?? "";
      local.InterstateCase.OtherFipsState =
        import.InterstateCase.OtherFipsState;
      local.InterstateCase.OtherFipsCounty =
        import.InterstateCase.OtherFipsCounty.GetValueOrDefault();
      local.InterstateCase.OtherFipsLocation =
        import.InterstateCase.OtherFipsLocation.GetValueOrDefault();
      UseSiGetDataInterstateCaseDb();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (IsEmpty(local.CsePersonsWorkSet.Number))
      {
        // --------------------------------------------------------------
        // Person was not found searching by SSN and alias SSNs.
        // --------------------------------------------------------------
        if (AsChar(import.CsenetTransactionEnvelop.ProcessingStatusCode) == 'C')
        {
          export.SendToDhr.Flag = "Y";

          return;
        }

        local.InterstateCase.Assign(import.InterstateCase);
        local.InterstateApIdentification.Assign(
          entities.InterstateApIdentification);
        UseSiGenCsenetTransactSerialNo();
        local.InterstateCase.ActionCode = "P";
        local.InterstateCase.CaseDataInd = 1;
        local.InterstateCase.ApIdentificationInd = 1;
        local.InterstateCase.ParticipantDataInd = 0;
        local.InterstateCase.OrderDataInd = 0;
        local.InterstateCase.CollectionDataInd = 0;
        local.InterstateCase.InformationInd = 0;
        local.InterstateCase.AttachmentsInd = "N";

        // --------------------------------------------------------------------
        // Processing status = D (after DHR had their shot)
        // If this is encountered in SI_B280 then DHR didn't return any
        // information and the 30 days has elapsed.
        // If this is encountered in SI_B285 then DHR has responded.
        // There may be information regaridng employment to include in
        // the returning transaction
        // --------------------------------------------------------------------
        if (!IsEmpty(import.SiWageIncomeSourceRec.EmpName))
        {
          local.InterstateCase.ActionReasonCode = "LSEMP";
          local.InterstateCase.ApLocateDataInd = 1;
          local.InterstateApLocate.EmployerEin =
            (int?)StringToNumber(import.SiWageIncomeSourceRec.EmpId);
          local.InterstateApLocate.EmployerName =
            import.SiWageIncomeSourceRec.EmpName;
          local.InterstateApLocate.EmployerAddressLine1 =
            import.SiWageIncomeSourceRec.Street1;
          local.InterstateApLocate.EmployerCity =
            import.SiWageIncomeSourceRec.City;
          local.InterstateApLocate.EmployerState =
            import.SiWageIncomeSourceRec.State;
          local.InterstateApLocate.EmployerZipCode5 =
            import.SiWageIncomeSourceRec.ZipCode;
          local.InterstateApLocate.EmployerZipCode4 =
            import.SiWageIncomeSourceRec.Zip4;
          local.InterstateApLocate.EmployerEffectiveDate = local.Current.Date;
          local.InterstateApLocate.EmployerConfirmedInd = "Y";

          if (import.SiWageIncomeSourceRec.BwQtr > 0 && import
            .SiWageIncomeSourceRec.BwYr > 0 && import
            .SiWageIncomeSourceRec.WageOrUiAmt > 0)
          {
            local.InterstateApLocate.WageQtr =
              import.SiWageIncomeSourceRec.BwQtr;
            local.InterstateApLocate.WageYear =
              import.SiWageIncomeSourceRec.BwYr;
            local.InterstateApLocate.WageAmount =
              import.SiWageIncomeSourceRec.WageOrUiAmt;
          }
        }
        else
        {
          local.InterstateCase.ActionReasonCode = "LUALL";
          local.InterstateCase.ApLocateDataInd = 0;
        }

        goto Read;
      }

      // **********************************************************************
      // 04/23/2002	M Ashworth	142092	Mondatory Case Data Block missing. Added 
      // set case db ind to 1
      // **********************************************************************
      if (IsEmpty(local.InterstateCase.KsCaseId))
      {
        local.InterstateCase.CaseDataInd = 1;
        local.InterstateCase.KsCaseId = import.InterstateCase.KsCaseId ?? "";
        local.InterstateCase.CaseType = import.InterstateCase.CaseType;
        local.InterstateCase.CaseStatus = import.InterstateCase.CaseStatus;
      }

      UseSiGetDataInterstateApIdDb();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      UseSiGetDataInterstateApLocDb();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (AsChar(import.CsenetTransactionEnvelop.ProcessingStatusCode) == 'D'
        && !IsEmpty(import.SiWageIncomeSourceRec.EmpName))
      {
        if (local.InterstateCase.ApLocateDataInd.GetValueOrDefault() == 0)
        {
          local.InterstateCase.ApLocateDataInd = 1;
          local.InterstateApLocate.EmployerEin =
            (int?)StringToNumber(import.SiWageIncomeSourceRec.EmpId);
          local.InterstateApLocate.EmployerName =
            import.SiWageIncomeSourceRec.EmpName;
          local.InterstateApLocate.EmployerAddressLine1 =
            import.SiWageIncomeSourceRec.Street1;
          local.InterstateApLocate.EmployerCity =
            import.SiWageIncomeSourceRec.City;
          local.InterstateApLocate.EmployerState =
            import.SiWageIncomeSourceRec.State;
          local.InterstateApLocate.EmployerZipCode5 =
            import.SiWageIncomeSourceRec.ZipCode;
          local.InterstateApLocate.EmployerZipCode4 =
            import.SiWageIncomeSourceRec.Zip4;
          local.InterstateApLocate.EmployerEffectiveDate = local.Current.Date;
          local.InterstateApLocate.EmployerConfirmedInd = "Y";

          if (import.SiWageIncomeSourceRec.BwQtr > 0 && import
            .SiWageIncomeSourceRec.BwYr > 0 && import
            .SiWageIncomeSourceRec.WageOrUiAmt > 0)
          {
            local.InterstateApLocate.WageQtr =
              import.SiWageIncomeSourceRec.BwQtr;
            local.InterstateApLocate.WageYear =
              import.SiWageIncomeSourceRec.BwYr;
            local.InterstateApLocate.WageAmount =
              import.SiWageIncomeSourceRec.WageOrUiAmt;
          }
        }
        else if (IsEmpty(local.InterstateApLocate.EmployerName))
        {
          local.InterstateApLocate.EmployerEin =
            (int?)StringToNumber(import.SiWageIncomeSourceRec.EmpId);
          local.InterstateApLocate.EmployerName =
            import.SiWageIncomeSourceRec.EmpName;
          local.InterstateApLocate.EmployerAddressLine1 =
            import.SiWageIncomeSourceRec.Street1;
          local.InterstateApLocate.EmployerCity =
            import.SiWageIncomeSourceRec.City;
          local.InterstateApLocate.EmployerState =
            import.SiWageIncomeSourceRec.State;
          local.InterstateApLocate.EmployerZipCode5 =
            import.SiWageIncomeSourceRec.ZipCode;
          local.InterstateApLocate.EmployerZipCode4 =
            import.SiWageIncomeSourceRec.Zip4;
          local.InterstateApLocate.EmployerEffectiveDate = local.Current.Date;
          local.InterstateApLocate.EmployerConfirmedInd = "Y";

          if (import.SiWageIncomeSourceRec.BwQtr > 0 && import
            .SiWageIncomeSourceRec.BwYr > 0 && import
            .SiWageIncomeSourceRec.WageOrUiAmt > 0)
          {
            local.InterstateApLocate.WageQtr =
              import.SiWageIncomeSourceRec.BwQtr;
            local.InterstateApLocate.WageYear =
              import.SiWageIncomeSourceRec.BwYr;
            local.InterstateApLocate.WageAmount =
              import.SiWageIncomeSourceRec.WageOrUiAmt;
          }
        }
        else if (IsEmpty(local.InterstateApLocate.Employer2Name))
        {
          local.InterstateApLocate.Employer2Ein =
            (int?)StringToNumber(import.SiWageIncomeSourceRec.EmpId);
          local.InterstateApLocate.Employer2Name =
            import.SiWageIncomeSourceRec.EmpName;
          local.InterstateApLocate.Employer2AddressLine1 =
            import.SiWageIncomeSourceRec.Street1;
          local.InterstateApLocate.Employer2City =
            import.SiWageIncomeSourceRec.City;
          local.InterstateApLocate.Employer2State =
            import.SiWageIncomeSourceRec.State;
          local.InterstateApLocate.Employer2ZipCode5 =
            (int?)StringToNumber(import.SiWageIncomeSourceRec.ZipCode);
          local.InterstateApLocate.Employer2ZipCode4 =
            (int?)StringToNumber(import.SiWageIncomeSourceRec.Zip4);
          local.InterstateApLocate.Employer2EffectiveDate = local.Current.Date;
          local.InterstateApLocate.Employer2ConfirmedIndicator = "Y";

          if (import.SiWageIncomeSourceRec.BwQtr > 0 && import
            .SiWageIncomeSourceRec.BwYr > 0 && import
            .SiWageIncomeSourceRec.WageOrUiAmt > 0)
          {
            local.InterstateApLocate.Employer2WageQuarter =
              import.SiWageIncomeSourceRec.BwQtr;
            local.InterstateApLocate.Employer2WageYear =
              import.SiWageIncomeSourceRec.BwYr;
            local.InterstateApLocate.Employer2WageAmount =
              (long?)import.SiWageIncomeSourceRec.WageOrUiAmt;
          }
        }
        else
        {
          local.InterstateApLocate.Employer3Ein =
            (int?)StringToNumber(import.SiWageIncomeSourceRec.EmpId);
          local.InterstateApLocate.Employer3Name =
            import.SiWageIncomeSourceRec.EmpName;
          local.InterstateApLocate.Employer3AddressLine1 =
            import.SiWageIncomeSourceRec.Street1;
          local.InterstateApLocate.Employer3City =
            import.SiWageIncomeSourceRec.City;
          local.InterstateApLocate.Employer3State =
            import.SiWageIncomeSourceRec.State;
          local.InterstateApLocate.Employer3ZipCode5 =
            (int?)StringToNumber(import.SiWageIncomeSourceRec.ZipCode);
          local.InterstateApLocate.Employer3ZipCode4 =
            (int?)StringToNumber(import.SiWageIncomeSourceRec.Zip4);
          local.InterstateApLocate.Employer3EffectiveDate = local.Current.Date;
          local.InterstateApLocate.Employer3ConfirmedIndicator = "Y";

          if (import.SiWageIncomeSourceRec.BwQtr > 0 && import
            .SiWageIncomeSourceRec.BwYr > 0 && import
            .SiWageIncomeSourceRec.WageOrUiAmt > 0)
          {
            local.InterstateApLocate.Employer3WageQuarter =
              import.SiWageIncomeSourceRec.BwQtr;
            local.InterstateApLocate.Employer3WageYear =
              import.SiWageIncomeSourceRec.BwYr;
            local.InterstateApLocate.Employer3WageAmount =
              (long?)import.SiWageIncomeSourceRec.WageOrUiAmt;
          }
        }
      }

      if (IsEmpty(local.InterstateApLocate.MailingAddressConfirmedInd) && IsEmpty
        (local.InterstateApLocate.ResidentialAddressConfirmInd))
      {
        local.AddressFound.Flag = "N";
        local.InStateAddressConfirmed.Flag = "N";
        local.OutStateAddressConfirmd.Flag = "N";
      }
      else
      {
        local.AddressFound.Flag = "Y";

        if (AsChar(local.InterstateApLocate.ResidentialAddressConfirmInd) == 'Y'
          && Equal(local.InterstateApLocate.ResidentialState, "KS"))
        {
          local.InStateAddressConfirmed.Flag = "Y";
        }
        else if (AsChar(local.InterstateApLocate.MailingAddressConfirmedInd) ==
          'Y' && Equal(local.InterstateApLocate.MailingState, "KS"))
        {
          local.InStateAddressConfirmed.Flag = "Y";
        }
        else
        {
          local.InStateAddressConfirmed.Flag = "N";
        }

        if (AsChar(local.InterstateApLocate.ResidentialAddressConfirmInd) == 'Y'
          && !Equal(local.InterstateApLocate.ResidentialState, "KS"))
        {
          local.OutStateAddressConfirmd.Flag = "Y";
        }
        else if (AsChar(local.InterstateApLocate.MailingAddressConfirmedInd) ==
          'Y' && !Equal(local.InterstateApLocate.MailingState, "KS"))
        {
          local.OutStateAddressConfirmd.Flag = "Y";
        }
        else
        {
          local.OutStateAddressConfirmd.Flag = "N";
        }
      }

      if (IsEmpty(local.InterstateApLocate.EmployerConfirmedInd) && IsEmpty
        (local.InterstateApLocate.Employer2ConfirmedIndicator) && IsEmpty
        (local.InterstateApLocate.Employer3ConfirmedIndicator))
      {
        local.EmployerConfirmed.Flag = "N";
        local.EmployerFound.Flag = "N";
      }
      else
      {
        local.EmployerFound.Flag = "Y";

        if (AsChar(local.InterstateApLocate.EmployerConfirmedInd) == 'Y' || AsChar
          (local.InterstateApLocate.Employer2ConfirmedIndicator) == 'Y' || AsChar
          (local.InterstateApLocate.Employer3ConfirmedIndicator) == 'Y')
        {
          local.EmployerConfirmed.Flag = "Y";
        }
        else
        {
          local.EmployerConfirmed.Flag = "N";
        }
      }

      if (IsEmpty(local.InterstateApLocate.DriversLicenseNum) && IsEmpty
        (local.InterstateApLocate.ProfessionalLicenses) && IsEmpty
        (local.InterstateApLocate.InsurancePolicyNum))
      {
        local.OtherInformationFound.Flag = "N";
      }
      else
      {
        local.OtherInformationFound.Flag = "Y";
      }

      if (AsChar(local.ApIsDeceased.Flag) == 'Y')
      {
        local.InterstateCase.ActionReasonCode = "LUAPD";
      }
      else if (!IsEmpty(local.FamilyViolence.Flag))
      {
        local.InterstateCase.ActionReasonCode = "LUALL";
        local.InterstateCase.ApLocateDataInd = 0;
        local.InterstateCase.InformationInd = 1;
        local.InterstateMiscellaneous.StatusChangeCode = "C";
        local.InterstateMiscellaneous.InformationTextLine1 =
          "Protected for Family Violence";
      }
      else if (AsChar(local.InStateAddressConfirmed.Flag) == 'Y' && AsChar
        (local.EmployerConfirmed.Flag) == 'Y')
      {
        local.InterstateCase.ActionReasonCode = "LSALL";
      }
      else if (AsChar(local.InStateAddressConfirmed.Flag) == 'Y')
      {
        local.InterstateCase.ActionReasonCode = "LSADR";
      }
      else if (AsChar(local.OutStateAddressConfirmd.Flag) == 'Y')
      {
        local.InterstateCase.ActionReasonCode = "LSOUT";
      }
      else if (AsChar(local.EmployerConfirmed.Flag) == 'Y')
      {
        local.InterstateCase.ActionReasonCode = "LSEMP";
      }
      else if (AsChar(local.AddressFound.Flag) == 'Y')
      {
        local.InterstateCase.ActionReasonCode = "LICAD";
      }
      else if (AsChar(local.EmployerFound.Flag) == 'Y')
      {
        local.InterstateCase.ActionReasonCode = "LICEM";
      }
      else if (AsChar(local.OtherInformationFound.Flag) == 'Y')
      {
        local.InterstateCase.ActionReasonCode = "LSOTH";

        // --------------------------------------------------------------
        // LSOTH uses the AP Locate DB.  An LO1-P requires either
        // the Residential Address, Mailing Address or the Employer to
        // send the AP Locate DB.  However, if we had one of those
        // we would've sent a different reason code.  An AP Locate DB
        // can be sent on an MSC-P with no requirements, so send that
        // instead of LO1-P.
        // --------------------------------------------------------------
        local.InterstateCase.FunctionalTypeCode = "MSC";
        local.InterstateCase.ApLocateDataInd = 1;
        local.InterstateCase.InformationInd = 1;
        local.InterstateMiscellaneous.StatusChangeCode = "C";
        local.InterstateMiscellaneous.InformationTextLine1 =
          "This is a response to a Quick Locate Request";
      }
      else
      {
        if (AsChar(import.CsenetTransactionEnvelop.ProcessingStatusCode) == 'C')
        {
          // --------------------------------------------------------------
          // Person was found in SRS Adabas Person file but no other
          // information found.  Add to locate file to be sent to DHR.
          // --------------------------------------------------------------
          export.SendToDhr.Flag = "Y";

          return;
        }

        local.InterstateCase.ActionReasonCode = "LUALL";
      }
    }
    else
    {
      local.InterstateCase.Assign(import.InterstateCase);
      UseSiGenCsenetTransactSerialNo();
      local.InterstateCase.ActionReasonCode = "LUALL";
      local.InterstateCase.ActionCode = "P";
      local.InterstateCase.CaseDataInd = 1;
      local.InterstateCase.ApIdentificationInd = 0;
      local.InterstateCase.ApLocateDataInd = 0;
      local.InterstateCase.ParticipantDataInd = 0;
      local.InterstateCase.OrderDataInd = 0;
      local.InterstateCase.CollectionDataInd = 0;
      local.InterstateCase.InformationInd = 0;
      local.InterstateCase.AttachmentsInd = "N";
    }

Read:

    UseSiCreateInterstateCase();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    MoveInterstateCase2(local.InterstateCase, export.New1);
    UseSiCreateOgCsenetEnvelop();

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() == 1)
    {
      UseSiCreateInterstateApId();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (local.InterstateCase.ApLocateDataInd.GetValueOrDefault() == 1)
      {
        UseSiCreateInterstateApLocate();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

    if (local.InterstateCase.InformationInd.GetValueOrDefault() == 1)
    {
      UseSiCreateInterstateMisc();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    ++import.ExpCheckpointUpdates.Count;
    ++import.ExpInterstateCaseCreates.Count;

    if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() == 1)
    {
      ++import.ExpApIdentCreates.Count;
    }

    if (local.InterstateCase.ApLocateDataInd.GetValueOrDefault() == 1)
    {
      ++import.ExpApLocateCreates.Count;
    }

    if (local.InterstateCase.InformationInd.GetValueOrDefault() == 1)
    {
      ++import.ExpMiscDbCreates.Count;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveInterstateApIdentification(
    InterstateApIdentification source, InterstateApIdentification target)
  {
    target.Ssn = source.Ssn;
    target.AliasSsn1 = source.AliasSsn1;
    target.AliasSsn2 = source.AliasSsn2;
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.AttachmentsInd = source.AttachmentsInd;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateMiscellaneous(
    InterstateMiscellaneous source, InterstateMiscellaneous target)
  {
    target.StatusChangeCode = source.StatusChangeCode;
    target.InformationTextLine1 = source.InformationTextLine1;
  }

  private void UseSiCreateInterstateApId()
  {
    var useImport = new SiCreateInterstateApId.Import();
    var useExport = new SiCreateInterstateApId.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateApIdentification.
      Assign(local.InterstateApIdentification);

    Call(SiCreateInterstateApId.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateApLocate()
  {
    var useImport = new SiCreateInterstateApLocate.Import();
    var useExport = new SiCreateInterstateApLocate.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateApLocate.Assign(local.InterstateApLocate);

    Call(SiCreateInterstateApLocate.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateCase()
  {
    var useImport = new SiCreateInterstateCase.Import();
    var useExport = new SiCreateInterstateCase.Export();

    useImport.InterstateCase.Assign(local.InterstateCase);

    Call(SiCreateInterstateCase.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateMisc()
  {
    var useImport = new SiCreateInterstateMisc.Import();
    var useExport = new SiCreateInterstateMisc.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    MoveInterstateMiscellaneous(local.InterstateMiscellaneous,
      useImport.InterstateMiscellaneous);

    Call(SiCreateInterstateMisc.Execute, useImport, useExport);
  }

  private void UseSiCreateOgCsenetEnvelop()
  {
    var useImport = new SiCreateOgCsenetEnvelop.Import();
    var useExport = new SiCreateOgCsenetEnvelop.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetEnvelop.Execute, useImport, useExport);
  }

  private void UseSiGenCsenetTransactSerialNo()
  {
    var useImport = new SiGenCsenetTransactSerialNo.Import();
    var useExport = new SiGenCsenetTransactSerialNo.Export();

    Call(SiGenCsenetTransactSerialNo.Execute, useImport, useExport);

    MoveInterstateCase2(useExport.InterstateCase, local.InterstateCase);
  }

  private void UseSiGetDataInterstateApIdDb()
  {
    var useImport = new SiGetDataInterstateApIdDb.Import();
    var useExport = new SiGetDataInterstateApIdDb.Export();

    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Ap.Number = local.CsePersonsWorkSet.Number;

    Call(SiGetDataInterstateApIdDb.Execute, useImport, useExport);

    local.InterstateCase.ApIdentificationInd =
      useExport.InterstateCase.ApIdentificationInd;
    local.ApIsDeceased.Flag = useExport.ApIsDeceased.Flag;
    local.InterstateApIdentification.
      Assign(useExport.InterstateApIdentification);
  }

  private void UseSiGetDataInterstateApLocDb()
  {
    var useImport = new SiGetDataInterstateApLocDb.Import();
    var useExport = new SiGetDataInterstateApLocDb.Export();

    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Current.Date = local.Current.Date;
    useImport.VerifiedSince.Date = local.VerifiedSince.Date;
    useImport.Ap.Assign(local.CsePersonsWorkSet);

    Call(SiGetDataInterstateApLocDb.Execute, useImport, useExport);

    local.InterstateCase.ApLocateDataInd =
      useExport.InterstateCase.ApLocateDataInd;
    local.FamilyViolence.Flag = useExport.FamilyViolence.Flag;
    local.InterstateApLocate.Assign(useExport.InterstateApLocate);
  }

  private void UseSiGetDataInterstateCaseDb()
  {
    var useImport = new SiGetDataInterstateCaseDb.Import();
    var useExport = new SiGetDataInterstateCaseDb.Export();

    MoveInterstateApIdentification(entities.InterstateApIdentification,
      useImport.FindCaseFromApSsns);
    MoveInterstateCase1(local.InterstateCase, useImport.InterstateCase);
    useImport.Current.Date = local.Current.Date;

    Call(SiGetDataInterstateCaseDb.Execute, useImport, useExport);

    local.InterstateCase.Assign(useExport.InterstateCase);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private bool ReadInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;

    return Read("ReadInterstateApIdentification",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateApIdentification.AliasSsn2 =
          db.GetNullableString(reader, 2);
        entities.InterstateApIdentification.AliasSsn1 =
          db.GetNullableString(reader, 3);
        entities.InterstateApIdentification.OtherIdInfo =
          db.GetNullableString(reader, 4);
        entities.InterstateApIdentification.EyeColor =
          db.GetNullableString(reader, 5);
        entities.InterstateApIdentification.HairColor =
          db.GetNullableString(reader, 6);
        entities.InterstateApIdentification.Weight =
          db.GetNullableInt32(reader, 7);
        entities.InterstateApIdentification.HeightIn =
          db.GetNullableInt32(reader, 8);
        entities.InterstateApIdentification.HeightFt =
          db.GetNullableInt32(reader, 9);
        entities.InterstateApIdentification.PlaceOfBirth =
          db.GetNullableString(reader, 10);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 11);
        entities.InterstateApIdentification.Race =
          db.GetNullableString(reader, 12);
        entities.InterstateApIdentification.Sex =
          db.GetNullableString(reader, 13);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 14);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 15);
        entities.InterstateApIdentification.NameFirst =
          db.GetString(reader, 16);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 17);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 18);
        entities.InterstateApIdentification.PossiblyDangerous =
          db.GetNullableString(reader, 19);
        entities.InterstateApIdentification.MaidenName =
          db.GetNullableString(reader, 20);
        entities.InterstateApIdentification.MothersMaidenOrFathersName =
          db.GetNullableString(reader, 21);
        entities.InterstateApIdentification.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    /// <summary>
    /// A value of SiWageIncomeSourceRec.
    /// </summary>
    [JsonPropertyName("siWageIncomeSourceRec")]
    public SiWageIncomeSourceRec SiWageIncomeSourceRec
    {
      get => siWageIncomeSourceRec ??= new();
      set => siWageIncomeSourceRec = value;
    }

    /// <summary>
    /// A value of BatchProcess.
    /// </summary>
    [JsonPropertyName("batchProcess")]
    public DateWorkArea BatchProcess
    {
      get => batchProcess ??= new();
      set => batchProcess = value;
    }

    /// <summary>
    /// A value of ExpRecordsForDhrLocates.
    /// </summary>
    [JsonPropertyName("expRecordsForDhrLocates")]
    public Common ExpRecordsForDhrLocates
    {
      get => expRecordsForDhrLocates ??= new();
      set => expRecordsForDhrLocates = value;
    }

    /// <summary>
    /// A value of ExpInterstateCaseCreates.
    /// </summary>
    [JsonPropertyName("expInterstateCaseCreates")]
    public Common ExpInterstateCaseCreates
    {
      get => expInterstateCaseCreates ??= new();
      set => expInterstateCaseCreates = value;
    }

    /// <summary>
    /// A value of ExpCheckpointUpdates.
    /// </summary>
    [JsonPropertyName("expCheckpointUpdates")]
    public Common ExpCheckpointUpdates
    {
      get => expCheckpointUpdates ??= new();
      set => expCheckpointUpdates = value;
    }

    /// <summary>
    /// A value of ExpApIdentCreates.
    /// </summary>
    [JsonPropertyName("expApIdentCreates")]
    public Common ExpApIdentCreates
    {
      get => expApIdentCreates ??= new();
      set => expApIdentCreates = value;
    }

    /// <summary>
    /// A value of ExpApLocateCreates.
    /// </summary>
    [JsonPropertyName("expApLocateCreates")]
    public Common ExpApLocateCreates
    {
      get => expApLocateCreates ??= new();
      set => expApLocateCreates = value;
    }

    /// <summary>
    /// A value of ExpMiscDbCreates.
    /// </summary>
    [JsonPropertyName("expMiscDbCreates")]
    public Common ExpMiscDbCreates
    {
      get => expMiscDbCreates ??= new();
      set => expMiscDbCreates = value;
    }

    private InterstateCase interstateCase;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private SiWageIncomeSourceRec siWageIncomeSourceRec;
    private DateWorkArea batchProcess;
    private Common expRecordsForDhrLocates;
    private Common expInterstateCaseCreates;
    private Common expCheckpointUpdates;
    private Common expApIdentCreates;
    private Common expApLocateCreates;
    private Common expMiscDbCreates;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public InterstateCase New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of SendToDhr.
    /// </summary>
    [JsonPropertyName("sendToDhr")]
    public Common SendToDhr
    {
      get => sendToDhr ??= new();
      set => sendToDhr = value;
    }

    private InterstateCase new1;
    private InterstateApIdentification interstateApIdentification;
    private Common sendToDhr;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ZdelLocalGroupAlternateSsnGroup group.</summary>
    [Serializable]
    public class ZdelLocalGroupAlternateSsnGroup
    {
      /// <summary>
      /// A value of ZdelLocalGSsn.
      /// </summary>
      [JsonPropertyName("zdelLocalGSsn")]
      public CsePersonsWorkSet ZdelLocalGSsn
      {
        get => zdelLocalGSsn ??= new();
        set => zdelLocalGSsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet zdelLocalGSsn;
    }

    /// <summary>A ZdelLocalGroupNamesGroup group.</summary>
    [Serializable]
    public class ZdelLocalGroupNamesGroup
    {
      /// <summary>
      /// A value of ZdelLocalGNames.
      /// </summary>
      [JsonPropertyName("zdelLocalGNames")]
      public CsePersonsWorkSet ZdelLocalGNames
      {
        get => zdelLocalGNames ??= new();
        set => zdelLocalGNames = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet zdelLocalGNames;
    }

    /// <summary>A ZdelGroupAliasesGroup group.</summary>
    [Serializable]
    public class ZdelGroupAliasesGroup
    {
      /// <summary>
      /// A value of ZdelG.
      /// </summary>
      [JsonPropertyName("zdelG")]
      public CsePersonsWorkSet ZdelG
      {
        get => zdelG ??= new();
        set => zdelG = value;
      }

      /// <summary>
      /// A value of ZdelGLocalKscares.
      /// </summary>
      [JsonPropertyName("zdelGLocalKscares")]
      public Common ZdelGLocalKscares
      {
        get => zdelGLocalKscares ??= new();
        set => zdelGLocalKscares = value;
      }

      /// <summary>
      /// A value of ZdelGLocalKanpay.
      /// </summary>
      [JsonPropertyName("zdelGLocalKanpay")]
      public Common ZdelGLocalKanpay
      {
        get => zdelGLocalKanpay ??= new();
        set => zdelGLocalKanpay = value;
      }

      /// <summary>
      /// A value of ZdelGLocalCse.
      /// </summary>
      [JsonPropertyName("zdelGLocalCse")]
      public Common ZdelGLocalCse
      {
        get => zdelGLocalCse ??= new();
        set => zdelGLocalCse = value;
      }

      /// <summary>
      /// A value of ZdelGLocalAe.
      /// </summary>
      [JsonPropertyName("zdelGLocalAe")]
      public Common ZdelGLocalAe
      {
        get => zdelGLocalAe ??= new();
        set => zdelGLocalAe = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet zdelG;
      private Common zdelGLocalKscares;
      private Common zdelGLocalKanpay;
      private Common zdelGLocalCse;
      private Common zdelGLocalAe;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of OutStateAddressConfirmd.
    /// </summary>
    [JsonPropertyName("outStateAddressConfirmd")]
    public Common OutStateAddressConfirmd
    {
      get => outStateAddressConfirmd ??= new();
      set => outStateAddressConfirmd = value;
    }

    /// <summary>
    /// A value of OtherInformationFound.
    /// </summary>
    [JsonPropertyName("otherInformationFound")]
    public Common OtherInformationFound
    {
      get => otherInformationFound ??= new();
      set => otherInformationFound = value;
    }

    /// <summary>
    /// A value of EmployerFound.
    /// </summary>
    [JsonPropertyName("employerFound")]
    public Common EmployerFound
    {
      get => employerFound ??= new();
      set => employerFound = value;
    }

    /// <summary>
    /// A value of AddressFound.
    /// </summary>
    [JsonPropertyName("addressFound")]
    public Common AddressFound
    {
      get => addressFound ??= new();
      set => addressFound = value;
    }

    /// <summary>
    /// A value of EmployerConfirmed.
    /// </summary>
    [JsonPropertyName("employerConfirmed")]
    public Common EmployerConfirmed
    {
      get => employerConfirmed ??= new();
      set => employerConfirmed = value;
    }

    /// <summary>
    /// A value of InStateAddressConfirmed.
    /// </summary>
    [JsonPropertyName("inStateAddressConfirmed")]
    public Common InStateAddressConfirmed
    {
      get => inStateAddressConfirmed ??= new();
      set => inStateAddressConfirmed = value;
    }

    /// <summary>
    /// A value of FamilyViolence.
    /// </summary>
    [JsonPropertyName("familyViolence")]
    public Common FamilyViolence
    {
      get => familyViolence ??= new();
      set => familyViolence = value;
    }

    /// <summary>
    /// A value of ApIsDeceased.
    /// </summary>
    [JsonPropertyName("apIsDeceased")]
    public Common ApIsDeceased
    {
      get => apIsDeceased ??= new();
      set => apIsDeceased = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of VerifiedSince.
    /// </summary>
    [JsonPropertyName("verifiedSince")]
    public DateWorkArea VerifiedSince
    {
      get => verifiedSince ??= new();
      set => verifiedSince = value;
    }

    /// <summary>
    /// A value of TransactionSince.
    /// </summary>
    [JsonPropertyName("transactionSince")]
    public DateWorkArea TransactionSince
    {
      get => transactionSince ??= new();
      set => transactionSince = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
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
    /// Gets a value of ZdelLocalGroupAlternateSsn.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelLocalGroupAlternateSsnGroup> ZdelLocalGroupAlternateSsn =>
      zdelLocalGroupAlternateSsn ??= new(ZdelLocalGroupAlternateSsnGroup.
        Capacity, 0);

    /// <summary>
    /// Gets a value of ZdelLocalGroupAlternateSsn for json serialization.
    /// </summary>
    [JsonPropertyName("zdelLocalGroupAlternateSsn")]
    [Computed]
    public IList<ZdelLocalGroupAlternateSsnGroup>
      ZdelLocalGroupAlternateSsn_Json
    {
      get => zdelLocalGroupAlternateSsn;
      set => ZdelLocalGroupAlternateSsn.Assign(value);
    }

    /// <summary>
    /// Gets a value of ZdelLocalGroupNames.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelLocalGroupNamesGroup> ZdelLocalGroupNames =>
      zdelLocalGroupNames ??= new(ZdelLocalGroupNamesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ZdelLocalGroupNames for json serialization.
    /// </summary>
    [JsonPropertyName("zdelLocalGroupNames")]
    [Computed]
    public IList<ZdelLocalGroupNamesGroup> ZdelLocalGroupNames_Json
    {
      get => zdelLocalGroupNames;
      set => ZdelLocalGroupNames.Assign(value);
    }

    /// <summary>
    /// A value of ZdelProgram.
    /// </summary>
    [JsonPropertyName("zdelProgram")]
    public Program ZdelProgram
    {
      get => zdelProgram ??= new();
      set => zdelProgram = value;
    }

    /// <summary>
    /// A value of ZdelLocalCurrentEmployer.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrentEmployer")]
    public Common ZdelLocalCurrentEmployer
    {
      get => zdelLocalCurrentEmployer ??= new();
      set => zdelLocalCurrentEmployer = value;
    }

    /// <summary>
    /// A value of ZdelLocalCreateApLocateDb.
    /// </summary>
    [JsonPropertyName("zdelLocalCreateApLocateDb")]
    public Common ZdelLocalCreateApLocateDb
    {
      get => zdelLocalCreateApLocateDb ??= new();
      set => zdelLocalCreateApLocateDb = value;
    }

    /// <summary>
    /// A value of ZdelAbendData.
    /// </summary>
    [JsonPropertyName("zdelAbendData")]
    public AbendData ZdelAbendData
    {
      get => zdelAbendData ??= new();
      set => zdelAbendData = value;
    }

    /// <summary>
    /// A value of ZdelLocalResAddressFound.
    /// </summary>
    [JsonPropertyName("zdelLocalResAddressFound")]
    public Common ZdelLocalResAddressFound
    {
      get => zdelLocalResAddressFound ??= new();
      set => zdelLocalResAddressFound = value;
    }

    /// <summary>
    /// A value of ZdelLocalMailingAddressFound.
    /// </summary>
    [JsonPropertyName("zdelLocalMailingAddressFound")]
    public Common ZdelLocalMailingAddressFound
    {
      get => zdelLocalMailingAddressFound ??= new();
      set => zdelLocalMailingAddressFound = value;
    }

    /// <summary>
    /// A value of ZdelCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("zdelCsePersonsWorkSet")]
    public CsePersonsWorkSet ZdelCsePersonsWorkSet
    {
      get => zdelCsePersonsWorkSet ??= new();
      set => zdelCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ZdelCase.
    /// </summary>
    [JsonPropertyName("zdelCase")]
    public Case1 ZdelCase
    {
      get => zdelCase ??= new();
      set => zdelCase = value;
    }

    /// <summary>
    /// Gets a value of ZdelGroupAliases.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroupAliasesGroup> ZdelGroupAliases =>
      zdelGroupAliases ??= new(ZdelGroupAliasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ZdelGroupAliases for json serialization.
    /// </summary>
    [JsonPropertyName("zdelGroupAliases")]
    [Computed]
    public IList<ZdelGroupAliasesGroup> ZdelGroupAliases_Json
    {
      get => zdelGroupAliases;
      set => ZdelGroupAliases.Assign(value);
    }

    private InterstateCase interstateCase;
    private InterstateMiscellaneous interstateMiscellaneous;
    private Common outStateAddressConfirmd;
    private Common otherInformationFound;
    private Common employerFound;
    private Common addressFound;
    private Common employerConfirmed;
    private Common inStateAddressConfirmed;
    private Common familyViolence;
    private Common apIsDeceased;
    private Common batch;
    private DateWorkArea null1;
    private DateWorkArea current;
    private DateWorkArea maxDate;
    private DateWorkArea verifiedSince;
    private DateWorkArea transactionSince;
    private InterstateApLocate interstateApLocate;
    private InterstateApIdentification interstateApIdentification;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ZdelLocalGroupAlternateSsnGroup> zdelLocalGroupAlternateSsn;
    private Array<ZdelLocalGroupNamesGroup> zdelLocalGroupNames;
    private Program zdelProgram;
    private Common zdelLocalCurrentEmployer;
    private Common zdelLocalCreateApLocateDb;
    private AbendData zdelAbendData;
    private Common zdelLocalResAddressFound;
    private Common zdelLocalMailingAddressFound;
    private CsePersonsWorkSet zdelCsePersonsWorkSet;
    private Case1 zdelCase;
    private Array<ZdelGroupAliasesGroup> zdelGroupAliases;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of PreviousCsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("previousCsenetTransactionEnvelop")]
    public CsenetTransactionEnvelop PreviousCsenetTransactionEnvelop
    {
      get => previousCsenetTransactionEnvelop ??= new();
      set => previousCsenetTransactionEnvelop = value;
    }

    /// <summary>
    /// A value of PreviousInterstateCase.
    /// </summary>
    [JsonPropertyName("previousInterstateCase")]
    public InterstateCase PreviousInterstateCase
    {
      get => previousInterstateCase ??= new();
      set => previousInterstateCase = value;
    }

    /// <summary>
    /// A value of PreviousInterstateApIdentification.
    /// </summary>
    [JsonPropertyName("previousInterstateApIdentification")]
    public InterstateApIdentification PreviousInterstateApIdentification
    {
      get => previousInterstateApIdentification ??= new();
      set => previousInterstateApIdentification = value;
    }

    /// <summary>
    /// A value of ZdelOfficeAddress.
    /// </summary>
    [JsonPropertyName("zdelOfficeAddress")]
    public OfficeAddress ZdelOfficeAddress
    {
      get => zdelOfficeAddress ??= new();
      set => zdelOfficeAddress = value;
    }

    /// <summary>
    /// A value of ZdelServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("zdelServiceProviderAddress")]
    public ServiceProviderAddress ZdelServiceProviderAddress
    {
      get => zdelServiceProviderAddress ??= new();
      set => zdelServiceProviderAddress = value;
    }

    /// <summary>
    /// A value of ZdelCaseAssignment.
    /// </summary>
    [JsonPropertyName("zdelCaseAssignment")]
    public CaseAssignment ZdelCaseAssignment
    {
      get => zdelCaseAssignment ??= new();
      set => zdelCaseAssignment = value;
    }

    /// <summary>
    /// A value of ZdelCaseRole.
    /// </summary>
    [JsonPropertyName("zdelCaseRole")]
    public CaseRole ZdelCaseRole
    {
      get => zdelCaseRole ??= new();
      set => zdelCaseRole = value;
    }

    /// <summary>
    /// A value of ZdelCase.
    /// </summary>
    [JsonPropertyName("zdelCase")]
    public Case1 ZdelCase
    {
      get => zdelCase ??= new();
      set => zdelCase = value;
    }

    /// <summary>
    /// A value of ZdelOffice.
    /// </summary>
    [JsonPropertyName("zdelOffice")]
    public Office ZdelOffice
    {
      get => zdelOffice ??= new();
      set => zdelOffice = value;
    }

    /// <summary>
    /// A value of ZdelOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("zdelOfficeServiceProvider")]
    public OfficeServiceProvider ZdelOfficeServiceProvider
    {
      get => zdelOfficeServiceProvider ??= new();
      set => zdelOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ZdelServiceProvider.
    /// </summary>
    [JsonPropertyName("zdelServiceProvider")]
    public ServiceProvider ZdelServiceProvider
    {
      get => zdelServiceProvider ??= new();
      set => zdelServiceProvider = value;
    }

    /// <summary>
    /// A value of ZdelCsePersonLicense.
    /// </summary>
    [JsonPropertyName("zdelCsePersonLicense")]
    public CsePersonLicense ZdelCsePersonLicense
    {
      get => zdelCsePersonLicense ??= new();
      set => zdelCsePersonLicense = value;
    }

    /// <summary>
    /// A value of ZdelHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("zdelHealthInsuranceCoverage")]
    public HealthInsuranceCoverage ZdelHealthInsuranceCoverage
    {
      get => zdelHealthInsuranceCoverage ??= new();
      set => zdelHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of ZdelHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("zdelHealthInsuranceCompany")]
    public HealthInsuranceCompany ZdelHealthInsuranceCompany
    {
      get => zdelHealthInsuranceCompany ??= new();
      set => zdelHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of ZdelCsePerson.
    /// </summary>
    [JsonPropertyName("zdelCsePerson")]
    public CsePerson ZdelCsePerson
    {
      get => zdelCsePerson ??= new();
      set => zdelCsePerson = value;
    }

    /// <summary>
    /// A value of ZdelEmployerAddress.
    /// </summary>
    [JsonPropertyName("zdelEmployerAddress")]
    public EmployerAddress ZdelEmployerAddress
    {
      get => zdelEmployerAddress ??= new();
      set => zdelEmployerAddress = value;
    }

    /// <summary>
    /// A value of ZdelEmployer.
    /// </summary>
    [JsonPropertyName("zdelEmployer")]
    public Employer ZdelEmployer
    {
      get => zdelEmployer ??= new();
      set => zdelEmployer = value;
    }

    /// <summary>
    /// A value of ZdelIncomeSource.
    /// </summary>
    [JsonPropertyName("zdelIncomeSource")]
    public IncomeSource ZdelIncomeSource
    {
      get => zdelIncomeSource ??= new();
      set => zdelIncomeSource = value;
    }

    /// <summary>
    /// A value of ZdelCsePersonAddress.
    /// </summary>
    [JsonPropertyName("zdelCsePersonAddress")]
    public CsePersonAddress ZdelCsePersonAddress
    {
      get => zdelCsePersonAddress ??= new();
      set => zdelCsePersonAddress = value;
    }

    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private CsenetTransactionEnvelop previousCsenetTransactionEnvelop;
    private InterstateCase previousInterstateCase;
    private InterstateApIdentification previousInterstateApIdentification;
    private OfficeAddress zdelOfficeAddress;
    private ServiceProviderAddress zdelServiceProviderAddress;
    private CaseAssignment zdelCaseAssignment;
    private CaseRole zdelCaseRole;
    private Case1 zdelCase;
    private Office zdelOffice;
    private OfficeServiceProvider zdelOfficeServiceProvider;
    private ServiceProvider zdelServiceProvider;
    private CsePersonLicense zdelCsePersonLicense;
    private HealthInsuranceCoverage zdelHealthInsuranceCoverage;
    private HealthInsuranceCompany zdelHealthInsuranceCompany;
    private CsePerson zdelCsePerson;
    private EmployerAddress zdelEmployerAddress;
    private Employer zdelEmployer;
    private IncomeSource zdelIncomeSource;
    private CsePersonAddress zdelCsePersonAddress;
  }
#endregion
}
