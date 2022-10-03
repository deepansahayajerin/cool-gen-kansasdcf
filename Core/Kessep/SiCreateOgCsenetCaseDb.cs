// Program: SI_CREATE_OG_CSENET_CASE_DB, ID: 372381424, model: 746.
// Short name: SWE01141
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
/// A program: SI_CREATE_OG_CSENET_CASE_DB.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the outgoing CSENet Case Data Block which contains the case
/// -related attributes and CSENet Header attributes of a referral.  This entity
/// type is the core record for all referral cases for all sources.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateOgCsenetCaseDb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_OG_CSENET_CASE_DB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateOgCsenetCaseDb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateOgCsenetCaseDb.
  /// </summary>
  public SiCreateOgCsenetCaseDb(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************************
    // 4/29/97		SHERAZ MALIK	CHANGE CURRENT_DATE
    // ********************************************************
    // ***************************************************************
    // 2/17/1999    Carl Ott    IDCR # 501, new attributes added.
    // **************************************************************
    // ***************************************************************
    // 5/14/1999    Carl Ott   Added counter for Participant DB.
    // **************************************************************
    // ***************************************************************
    // 6/23/1999    Carl Ott   Made a correction for assigning the Case Type 
    // attribute on Interstate Case.
    // **************************************************************
    // ***************************************************************
    // 8/4/1999    Carl Ott   Added unique rollback exit states for each CSENet 
    // data block create failure.
    // **************************************************************
    // **************************************************************
    // 8/5/99   Carl Ott   Payment Address must be populated if Payment Address 
    // flag is equal to 'Y'.  Set rollback exit state and exit to prevent
    // invalid transaction from being created.
    // **************************************************************
    // **************************************************************
    // 8/31/99   Carl Ott   Correction to Payment address conditional logic.
    // **************************************************************
    // ****************************************************************
    // 9/10/99   C. Ott   Modified for Problem # 73094
    // ****************************************************************
    // ***************************************************************
    // 03/14/00  C. Ott  PR # 85011  Case Program Type not determined correctly 
    // for outgoing interstate case.  Call new action block to retrieve arrears
    // programs.
    // ***************************************************************
    // ***************************************************************
    // 04/07/00  C. Scroggins Added check for family violence.
    // ***************************************************************
    // *************************************************************
    // 07/31/07  G. Pan   PR218020 Mapped two entities for calling
    //                    si_get_payment_mailing_address
    // *************************************************************
    local.Current.Date = Now().Date;
    local.TotalSupportOrder.Count = 0;
    MoveCase1(import.Case1, export.Case1);
    export.Ap.Number = import.Ap.Number;

    // ***   Set the CSENet transaction serial #.    ***
    UseSiGenCsenetTransactSerialNo();

    if (!ReadInterstateRequest())
    {
      ExitState = "INTERSTATE_REQUEST_NF";

      return;
    }

    if (ReadInterstateRequestHistory())
    {
      try
      {
        UpdateInterstateRequestHistory();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_INTERSTAT_REQ_HIST_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SI0000_INTERSTAT_REQ_HIST_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "INTERSTATE_REQUEST_HISTORY_NF";

      return;
    }

    // ***      Set KS as the Local FIPS.       ***
    local.InterstateCase.LocalFipsState = 20;
    local.InterstateCase.LocalFipsCounty = 0;
    local.InterstateCase.LocalFipsLocation = 0;

    // ***      Set the other State FIPS.        ***
    local.InterstateCase.OtherFipsState =
      entities.InterstateRequest.OtherStateFips;
    local.InterstateCase.OtherFipsCounty = 0;
    local.InterstateCase.OtherFipsLocation = 0;
    local.InterstateCase.ActionCode =
      entities.InterstateRequestHistory.ActionCode;
    local.InterstateCase.FunctionalTypeCode =
      entities.InterstateRequestHistory.FunctionalTypeCode;
    local.InterstateCase.ActionReasonCode =
      entities.InterstateRequestHistory.ActionReasonCode;
    local.InterstateCase.KsCaseId = import.Case1.Number;
    local.InterstateCase.InterstateCaseId =
      entities.InterstateRequest.OtherStateCaseId;
    local.InterstateCase.TransactionDate =
      entities.InterstateRequestHistory.TransactionDate;
    local.InterstateCase.ActionResolutionDate =
      entities.InterstateRequestHistory.ActionResolutionDate;

    // SET TO IMPORT VIEWS.
    local.InterstateCase.AttachmentsInd = import.InterstateCase.AttachmentsInd;
    local.InterstateCase.CaseDataInd =
      import.InterstateCase.CaseDataInd.GetValueOrDefault();
    local.InterstateCase.ApIdentificationInd =
      import.InterstateCase.ApIdentificationInd.GetValueOrDefault();
    local.InterstateCase.ApLocateDataInd =
      import.InterstateCase.ApLocateDataInd.GetValueOrDefault();
    local.InterstateCase.ParticipantDataInd =
      import.InterstateCase.ParticipantDataInd.GetValueOrDefault();
    local.InterstateCase.OrderDataInd =
      import.InterstateCase.OrderDataInd.GetValueOrDefault();
    local.InterstateCase.CollectionDataInd =
      import.InterstateCase.CollectionDataInd.GetValueOrDefault();
    local.InterstateCase.InformationInd =
      import.InterstateCase.InformationInd.GetValueOrDefault();

    // SET THE CASE TYPE HERE :
    UseSiReadCaseProgramType();

    // ***************************************************************
    // 03/14/00  C. Ott  PR # 85011  Case Program Type not determined correctly 
    // for outgoing interstate case.  Call new action block to retrieve arrears
    // programs.
    // ***************************************************************
    if (IsEmpty(local.Program.Code) || !
      IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      UseSiReadArrOnlyCasePrgType();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }

    local.InterstateCase.CaseType = local.Program.Code;
    local.InterstateCase.CaseStatus = import.Case1.Status ?? Spaces(1);

    // ***   Get CSENet_Case_Data_Block Payment_Address   ***
    if (AsChar(import.PaymentAddressReqd.Flag) == 'Y')
    {
      for(import.CourtOrder.Index = 0; import.CourtOrder.Index < import
        .CourtOrder.Count; ++import.CourtOrder.Index)
      {
        if (!IsEmpty(import.CourtOrder.Item.Details.CourtCaseNumber))
        {
          local.LegalAction.Identifier =
            import.CourtOrder.Item.Details.Identifier;

          break;
        }
      }

      UseSiGetPaymentMailingAddress();

      if (IsEmpty(local.PaymentAddress.PaymentMailingAddressLine1) || IsEmpty
        (local.PaymentAddress.PaymentCity) || IsEmpty
        (local.PaymentAddress.PaymentState) || IsEmpty
        (local.PaymentAddress.PaymentZipCode5))
      {
        // **************************************************************
        // 8/5/99   C. Ott   These fields must contain data if Payment Address 
        // flag is equal to 'Y'.  Set rollback exit state to prevent invalid
        // transaction from being created.
        // **************************************************************
        ExitState = "SI0000_PAYMENT_ADDRESS_NF_RB";

        return;
      }

      local.InterstateCase.PaymentMailingAddressLine1 =
        local.PaymentAddress.PaymentMailingAddressLine1 ?? "";
      local.InterstateCase.PaymentAddressLine2 =
        local.PaymentAddress.PaymentAddressLine2 ?? "";
      local.InterstateCase.PaymentCity = local.PaymentAddress.PaymentCity ?? "";
      local.InterstateCase.PaymentState = local.PaymentAddress.PaymentState ?? ""
        ;
      local.InterstateCase.PaymentZipCode5 =
        local.PaymentAddress.PaymentZipCode5 ?? "";
      local.InterstateCase.PaymentZipCode4 =
        local.PaymentAddress.PaymentZipCode4 ?? "";
    }

    // ***   Get CSENet_Case_Data_Block Contact Details   ***
    if (ReadCase())
    {
      if (!ReadCaseAssignment())
      {
        ExitState = "CASE_ASSIGNMENT_NF";

        return;
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadServiceProviderOfficeServiceProviderOffice())
    {
      local.ContactServiceProvider.Assign(entities.ServiceProvider);
      local.ContactOfficeServiceProvider.Assign(entities.OfficeServiceProvider);

      if (ReadServiceProviderAddress())
      {
        local.ContactOfficeAddress.Street1 =
          entities.ServiceProviderAddress.Street1;
        local.ContactOfficeAddress.Street2 =
          entities.ServiceProviderAddress.Street2;
        local.ContactOfficeAddress.City = entities.ServiceProviderAddress.City;
        local.ContactOfficeAddress.StateProvince =
          entities.ServiceProviderAddress.StateProvince;
        local.ContactOfficeAddress.Zip = entities.ServiceProviderAddress.Zip;
        local.ContactOfficeAddress.Zip4 = entities.ServiceProviderAddress.Zip4;
      }
      else if (ReadOfficeAddress())
      {
        local.ContactOfficeAddress.Assign(entities.OfficeAddress);
      }
    }

    local.InterstateCase.ContactNameLast =
      local.ContactServiceProvider.LastName;
    local.InterstateCase.ContactNameFirst =
      local.ContactServiceProvider.FirstName;
    local.InterstateCase.ContactNameMiddle =
      local.ContactServiceProvider.MiddleInitial;
    local.InterstateCase.ContactNameSuffix = "";
    local.InterstateCase.ContactAddressLine1 =
      local.ContactOfficeAddress.Street1;
    local.InterstateCase.ContactAddressLine2 =
      local.ContactOfficeAddress.Street2 ?? "";
    local.InterstateCase.ContactCity = local.ContactOfficeAddress.City;
    local.InterstateCase.ContactState =
      local.ContactOfficeAddress.StateProvince;
    local.InterstateCase.ContactZipCode5 = local.ContactOfficeAddress.Zip ?? "";
    local.InterstateCase.ContactZipCode4 = local.ContactOfficeAddress.Zip4 ?? ""
      ;
    local.InterstateCase.ContactPhoneNum =
      local.ContactOfficeServiceProvider.WorkPhoneNumber;
    local.InterstateCase.ContactAreaCode =
      local.ContactOfficeServiceProvider.WorkPhoneAreaCode;
    local.InterstateCase.ContactPhoneExtension =
      local.ContactOfficeServiceProvider.WorkPhoneExtension ?? "";

    if (local.ContactOfficeServiceProvider.WorkFaxNumber.GetValueOrDefault() > 0
      )
    {
      local.InterstateCase.ContactFaxAreaCode =
        local.ContactOfficeServiceProvider.WorkFaxAreaCode.GetValueOrDefault();
      local.InterstateCase.ContactFaxNumber =
        local.ContactOfficeServiceProvider.WorkFaxNumber.GetValueOrDefault();
    }
    else
    {
      local.InterstateCase.ContactFaxAreaCode = entities.Office.MainFaxAreaCode;
      local.InterstateCase.ContactFaxNumber =
        entities.Office.MainFaxPhoneNumber;
    }

    // ****************************************************************
    // 5/11/1999 Tim Hood confirmed today with the State Treasurer's office the 
    // following numbers for EFT:
    // ABA Routing number for UMB:  101000695
    // Sub account number at UMB:  9870969688
    // Carl Ott 5/11/1999
    // ***************************************************************
    local.InterstateCase.SendPaymentsRoutingCode = 101000695;
    local.InterstateCase.SendPaymentsBankAccount = "9870969688";

    try
    {
      CreateInterstateCase();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      export.InterstateCase.Assign(entities.InterstateCase);

      // ---------------------------------------------
      // Create the CSENet_Envelop and the other data
      // blocks depending on the indicator set.
      // ---------------------------------------------
      // ***   Call CAB to create CSENet Envelop   ***
      UseSiCreateOgCsenetEnvelop();

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        ExitState = "SI0000_CSENET_TRN_ENV_ERROR_RB";

        return;
      }

      if (import.InterstateCase.ApIdentificationInd.GetValueOrDefault() == 1)
      {
        // ***   Call CAB to create AP_Identification_DB   ***
        UseSiCreateOgCsenetApIdDb();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          if (import.InterstateCase.ApLocateDataInd.GetValueOrDefault() == 1)
          {
            // ***   Call CAB to create AP_Locate_DB   ***
            UseSiCreateOgCsenetApLocateDb();

            if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              ExitState = "SI0000_CSENET_AP_LOC_ERROR_RB";

              return;
            }
          }
        }
        else
        {
          ExitState = "SI0000_CSENET_AP_ID_ERROR_RB";

          return;
        }
      }

      if (import.InterstateCase.ParticipantDataInd.GetValueOrDefault() > 0 || !
        import.Participant.IsEmpty)
      {
        if (import.Participant.IsEmpty)
        {
          local.InterstateCase.ParticipantDataInd = 0;
        }
        else
        {
          // ***   Call CAB to create Participant_DB   ***
          UseSiCreateOgCsenetParticptnDb();

          if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
            ExitState = "SI0000_CSENET_PART_ERROR_RB";

            return;
          }
        }

        if (local.InterstateCase.ParticipantDataInd.GetValueOrDefault() != import
          .InterstateCase.ParticipantDataInd.GetValueOrDefault())
        {
          try
          {
            UpdateInterstateCase2();
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "SI0000_INTERSTATE_CASE_AE_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "SI0000_INTERSTATE_CASE_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      if (import.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
      {
        // ***   Call CAB to create Order_DB   ***
        // *****************************************************************
        // Add AR to the participant group so that Spousal Arrears may be 
        // included
        // ****************************************************************
        local.Participant.Index = -1;

        for(import.Participant.Index = 0; import.Participant.Index < import
          .Participant.Count; ++import.Participant.Index)
        {
          ++local.Participant.Index;
          local.Participant.CheckSize();

          local.Participant.Update.CsePersonsWorkSet.Number =
            import.Participant.Item.Person.Number;
          local.Participant.Update.Common.SelectChar =
            import.Participant.Item.Sel.SelectChar;
        }

        ++local.Participant.Index;
        local.Participant.CheckSize();

        local.Participant.Update.CsePersonsWorkSet.Number = import.Ar.Number;

        export.CourtOrder.Index = 0;
        export.CourtOrder.Clear();

        for(import.CourtOrder.Index = 0; import.CourtOrder.Index < import
          .CourtOrder.Count; ++import.CourtOrder.Index)
        {
          if (export.CourtOrder.IsFull)
          {
            break;
          }

          export.CourtOrder.Update.Details.
            Assign(import.CourtOrder.Item.Details);
          export.CourtOrder.Update.PromptLacs.SelectChar =
            import.CourtOrder.Item.PromptLacs.SelectChar;

          if (!IsEmpty(import.CourtOrder.Item.Details.CourtCaseNumber) && IsEmpty
            (local.SupportOrderError.Flag))
          {
            UseSiCreateOgCsenetOrderDb();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              local.TotalSupportOrder.Count += local.OrderDbCreated.Count;
            }
            else
            {
              // *****************************************************************
              // 11/18/99  C. Ott   PR # ?????
              // *****************************************************************
              export.CourtOrder.Update.PromptLacs.SelectChar = "E";
              local.SupportOrderError.Flag = "Y";
            }
          }

          export.CourtOrder.Next();
        }

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          return;
        }
      }

      if (IsEmpty(local.InterstateCase.CaseType))
      {
        if (IsEmpty(local.ArrearsOnly.CaseType))
        {
          local.InterstateCase.CaseType = local.Program.Code;
        }
        else
        {
          local.InterstateCase.CaseType = local.ArrearsOnly.CaseType;
        }

        try
        {
          UpdateInterstateCase1();
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SI0000_INTERSTATE_CASE_AE_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SI0000_INTERSTATE_CASE_AE_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // ***************************************************************
      // 5/14/1999  C. Ott
      // Removed call of SI_CREATE_OG_CSENET_COLLECTIONS because this data block
      // is created only by the FN_B676_OUTBOUND_CSENET_COLL batch procedure.
      // ***************************************************************
      if (import.InterstateCase.InformationInd.GetValueOrDefault() == 1)
      {
        // ***   Call CAB to create Information_DB   ***
        UseSiCreateOgCsenetMiscDb();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          ExitState = "SI0000_CSENET_MISC_ERROR_RB";

          return;
        }
      }

      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      UseSiUpdateReferralDeactStatus();

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        ExitState = "SI0000_CSENET_DEACT_STATUS_RB";
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SI0000_INTERSTATE_CASE_AE_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SI0000_INTERSTATE_CASE_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.TransactionDate = source.TransactionDate;
    target.KsCaseId = source.KsCaseId;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase3(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
    target.ActionReasonCode = source.ActionReasonCode;
  }

  private static void MoveInterstateCase4(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.AssnDeactInd = source.AssnDeactInd;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.TransactionSerialNum = source.TransactionSerialNum;
    target.Note = source.Note;
  }

  private static void MoveParticipant1(Import.ParticipantGroup source,
    SiCreateOgCsenetParticptnDb.Import.ParticipantGroup target)
  {
    target.Sel.SelectChar = source.Sel.SelectChar;
    target.Person.Number = source.Person.Number;
  }

  private static void MoveParticipant2(Local.ParticipantGroup source,
    SiCreateOgCsenetOrderDb.Import.ParticipantGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.CsePersonsWorkSet.Number = source.CsePersonsWorkSet.Number;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
  }

  private void UseSiCreateOgCsenetApIdDb()
  {
    var useImport = new SiCreateOgCsenetApIdDb.Import();
    var useExport = new SiCreateOgCsenetApIdDb.Export();

    useImport.Ap.Number = export.Ap.Number;
    MoveInterstateCase2(export.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetApIdDb.Execute, useImport, useExport);

    export.InterstateApIdentification.Assign(
      useExport.InterstateApIdentification);
  }

  private void UseSiCreateOgCsenetApLocateDb()
  {
    var useImport = new SiCreateOgCsenetApLocateDb.Import();
    var useExport = new SiCreateOgCsenetApLocateDb.Export();

    useImport.Ap.Number = export.Ap.Number;
    useImport.InterstateApIdentification.Assign(
      export.InterstateApIdentification);
    MoveInterstateCase3(export.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetApLocateDb.Execute, useImport, useExport);

    export.InterstateApLocate.Assign(useExport.InterstateApLocate);
  }

  private void UseSiCreateOgCsenetEnvelop()
  {
    var useImport = new SiCreateOgCsenetEnvelop.Import();
    var useExport = new SiCreateOgCsenetEnvelop.Export();

    MoveInterstateCase2(export.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetEnvelop.Execute, useImport, useExport);
  }

  private void UseSiCreateOgCsenetMiscDb()
  {
    var useImport = new SiCreateOgCsenetMiscDb.Import();
    var useExport = new SiCreateOgCsenetMiscDb.Export();

    MoveInterstateRequestHistory(entities.InterstateRequestHistory,
      useImport.InterstateRequestHistory);
    MoveCase1(export.Case1, useImport.Case1);
    useImport.InterstateRequest.IntHGeneratedId =
      entities.InterstateRequest.IntHGeneratedId;
    MoveInterstateCase2(entities.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetMiscDb.Execute, useImport, useExport);

    export.InterstateMiscellaneous.Assign(useExport.InterstateMiscellaneous);
  }

  private void UseSiCreateOgCsenetOrderDb()
  {
    var useImport = new SiCreateOgCsenetOrderDb.Import();
    var useExport = new SiCreateOgCsenetOrderDb.Export();

    useImport.LegalAction.Assign(import.CourtOrder.Item.Details);
    MoveInterstateCase1(export.InterstateCase, useImport.InterstateCase);
    local.Participant.CopyTo(useImport.Participant, MoveParticipant2);

    Call(SiCreateOgCsenetOrderDb.Execute, useImport, useExport);

    local.ArrearsOnly.CaseType = useExport.ArrearsOnly.CaseType;
    local.OrderDbCreated.Count = useExport.OrderDbCreated.Count;
  }

  private void UseSiCreateOgCsenetParticptnDb()
  {
    var useImport = new SiCreateOgCsenetParticptnDb.Import();
    var useExport = new SiCreateOgCsenetParticptnDb.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Ap.Number = export.Ap.Number;
    MoveInterstateCase2(export.InterstateCase, useImport.InterstateCase);
    import.Participant.CopyTo(useImport.Participant, MoveParticipant1);

    Call(SiCreateOgCsenetParticptnDb.Execute, useImport, useExport);

    local.InterstateCase.ParticipantDataInd =
      useExport.InterstateCase.ParticipantDataInd;
  }

  private void UseSiGenCsenetTransactSerialNo()
  {
    var useImport = new SiGenCsenetTransactSerialNo.Import();
    var useExport = new SiGenCsenetTransactSerialNo.Export();

    Call(SiGenCsenetTransactSerialNo.Execute, useImport, useExport);

    MoveInterstateCase2(useExport.InterstateCase, local.InterstateCase);
  }

  private void UseSiGetPaymentMailingAddress()
  {
    var useImport = new SiGetPaymentMailingAddress.Import();
    var useExport = new SiGetPaymentMailingAddress.Export();

    useImport.Ap.Number = export.Ap.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.LegalAction.Identifier = local.LegalAction.Identifier;

    Call(SiGetPaymentMailingAddress.Execute, useImport, useExport);

    local.PaymentAddress.Assign(useExport.InterstateCase);
  }

  private void UseSiReadArrOnlyCasePrgType()
  {
    var useImport = new SiReadArrOnlyCasePrgType.Import();
    var useExport = new SiReadArrOnlyCasePrgType.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadArrOnlyCasePrgType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseSiUpdateReferralDeactStatus()
  {
    var useImport = new SiUpdateReferralDeactStatus.Import();
    var useExport = new SiUpdateReferralDeactStatus.Export();

    MoveInterstateCase2(export.InterstateCase, useImport.InterstateCase);

    Call(SiUpdateReferralDeactStatus.Execute, useImport, useExport);

    MoveInterstateCase4(useExport.InterstateCase, export.InterstateCase);
  }

  private void CreateInterstateCase()
  {
    var localFipsState = local.InterstateCase.LocalFipsState;
    var localFipsCounty =
      local.InterstateCase.LocalFipsCounty.GetValueOrDefault();
    var localFipsLocation =
      local.InterstateCase.LocalFipsLocation.GetValueOrDefault();
    var otherFipsState = local.InterstateCase.OtherFipsState;
    var otherFipsCounty =
      local.InterstateCase.OtherFipsCounty.GetValueOrDefault();
    var otherFipsLocation =
      local.InterstateCase.OtherFipsLocation.GetValueOrDefault();
    var transSerialNumber = local.InterstateCase.TransSerialNumber;
    var actionCode = local.InterstateCase.ActionCode;
    var functionalTypeCode = local.InterstateCase.FunctionalTypeCode;
    var transactionDate = local.InterstateCase.TransactionDate;
    var ksCaseId = local.InterstateCase.KsCaseId ?? "";
    var interstateCaseId = local.InterstateCase.InterstateCaseId ?? "";
    var actionReasonCode = local.InterstateCase.ActionReasonCode ?? "";
    var actionResolutionDate = local.InterstateCase.ActionResolutionDate;
    var attachmentsInd = local.InterstateCase.AttachmentsInd;
    var caseDataInd = local.InterstateCase.CaseDataInd.GetValueOrDefault();
    var apIdentificationInd =
      local.InterstateCase.ApIdentificationInd.GetValueOrDefault();
    var apLocateDataInd =
      local.InterstateCase.ApLocateDataInd.GetValueOrDefault();
    var participantDataInd =
      local.InterstateCase.ParticipantDataInd.GetValueOrDefault();
    var orderDataInd = local.InterstateCase.OrderDataInd.GetValueOrDefault();
    var collectionDataInd =
      local.InterstateCase.CollectionDataInd.GetValueOrDefault();
    var informationInd =
      local.InterstateCase.InformationInd.GetValueOrDefault();
    var sentDate = local.InterstateCase.SentDate;
    var sentTime = local.InterstateCase.SentTime.GetValueOrDefault();
    var dueDate = local.InterstateCase.DueDate;
    var overdueInd = local.InterstateCase.OverdueInd.GetValueOrDefault();
    var dateReceived = local.InterstateCase.DateReceived;
    var timeReceived = local.InterstateCase.TimeReceived.GetValueOrDefault();
    var attachmentsDueDate = local.InterstateCase.AttachmentsDueDate;
    var interstateFormsPrinted =
      local.InterstateCase.InterstateFormsPrinted ?? "";
    var caseType = local.InterstateCase.CaseType;
    var caseStatus = local.InterstateCase.CaseStatus;
    var paymentMailingAddressLine1 =
      local.InterstateCase.PaymentMailingAddressLine1 ?? "";
    var paymentAddressLine2 = local.InterstateCase.PaymentAddressLine2 ?? "";
    var paymentCity = local.InterstateCase.PaymentCity ?? "";
    var paymentState = local.InterstateCase.PaymentState ?? "";
    var paymentZipCode5 = local.InterstateCase.PaymentZipCode5 ?? "";
    var paymentZipCode4 = local.InterstateCase.PaymentZipCode4 ?? "";
    var contactNameLast = local.InterstateCase.ContactNameLast ?? "";
    var contactNameFirst = local.InterstateCase.ContactNameFirst ?? "";
    var contactNameMiddle = local.InterstateCase.ContactNameMiddle ?? "";
    var contactNameSuffix = local.InterstateCase.ContactNameSuffix ?? "";
    var contactAddressLine1 = local.InterstateCase.ContactAddressLine1;
    var contactAddressLine2 = local.InterstateCase.ContactAddressLine2 ?? "";
    var contactCity = local.InterstateCase.ContactCity ?? "";
    var contactState = local.InterstateCase.ContactState ?? "";
    var contactZipCode5 = local.InterstateCase.ContactZipCode5 ?? "";
    var contactZipCode4 = local.InterstateCase.ContactZipCode4 ?? "";
    var contactPhoneNum =
      local.InterstateCase.ContactPhoneNum.GetValueOrDefault();
    var assnDeactDt = local.InterstateCase.AssnDeactDt;
    var assnDeactInd = local.InterstateCase.AssnDeactInd ?? "";
    var lastDeferDt = local.InterstateCase.LastDeferDt;
    var memo = local.InterstateCase.Memo ?? "";
    var contactPhoneExtension = local.InterstateCase.ContactPhoneExtension ?? ""
      ;
    var contactFaxNumber =
      local.InterstateCase.ContactFaxNumber.GetValueOrDefault();
    var contactFaxAreaCode =
      local.InterstateCase.ContactFaxAreaCode.GetValueOrDefault();
    var contactInternetAddress =
      local.InterstateCase.ContactInternetAddress ?? "";
    var initiatingDocketNumber =
      local.InterstateCase.InitiatingDocketNumber ?? "";
    var sendPaymentsBankAccount =
      local.InterstateCase.SendPaymentsBankAccount ?? "";
    var sendPaymentsRoutingCode =
      local.InterstateCase.SendPaymentsRoutingCode.GetValueOrDefault();
    var nondisclosureFinding = local.InterstateCase.NondisclosureFinding ?? "";
    var respondingDocketNumber =
      local.InterstateCase.RespondingDocketNumber ?? "";
    var stateWithCej = local.InterstateCase.StateWithCej ?? "";
    var paymentFipsCounty = local.InterstateCase.PaymentFipsCounty ?? "";
    var paymentFipsState = local.InterstateCase.PaymentFipsState ?? "";
    var paymentFipsLocation = local.InterstateCase.PaymentFipsLocation ?? "";
    var contactAreaCode =
      local.InterstateCase.ContactAreaCode.GetValueOrDefault();

    entities.InterstateCase.Populated = false;
    Update("CreateInterstateCase",
      (db, command) =>
      {
        db.SetInt32(command, "localFipsState", localFipsState);
        db.SetNullableInt32(command, "localFipsCounty", localFipsCounty);
        db.SetNullableInt32(command, "localFipsLocatio", localFipsLocation);
        db.SetInt32(command, "otherFipsState", otherFipsState);
        db.SetNullableInt32(command, "otherFipsCounty", otherFipsCounty);
        db.SetNullableInt32(command, "otherFipsLocatio", otherFipsLocation);
        db.SetInt64(command, "transSerialNbr", transSerialNumber);
        db.SetString(command, "actionCode", actionCode);
        db.SetString(command, "functionalTypeCo", functionalTypeCode);
        db.SetDate(command, "transactionDate", transactionDate);
        db.SetNullableString(command, "ksCaseId", ksCaseId);
        db.SetNullableString(command, "interstateCaseId", interstateCaseId);
        db.SetNullableString(command, "actionReasonCode", actionReasonCode);
        db.SetNullableDate(command, "actionResolution", actionResolutionDate);
        db.SetString(command, "attachmentsInd", attachmentsInd);
        db.SetNullableInt32(command, "caseDataInd", caseDataInd);
        db.SetNullableInt32(command, "apIdentification", apIdentificationInd);
        db.SetNullableInt32(command, "apLocateDataInd", apLocateDataInd);
        db.SetNullableInt32(command, "participantDataI", participantDataInd);
        db.SetNullableInt32(command, "orderDataInd", orderDataInd);
        db.SetNullableInt32(command, "collectionDataIn", collectionDataInd);
        db.SetNullableInt32(command, "informationInd", informationInd);
        db.SetNullableDate(command, "sentDate", sentDate);
        db.SetNullableTimeSpan(command, "sentTime", sentTime);
        db.SetNullableDate(command, "dueDate", dueDate);
        db.SetNullableInt32(command, "overdueInd", overdueInd);
        db.SetNullableDate(command, "dateReceived", dateReceived);
        db.SetNullableTimeSpan(command, "timeReceived", timeReceived);
        db.SetNullableDate(command, "attachmntsDueDte", attachmentsDueDate);
        db.
          SetNullableString(command, "interstateFormsP", interstateFormsPrinted);
          
        db.SetString(command, "caseType", caseType);
        db.SetString(command, "caseStatus", caseStatus);
        db.SetNullableString(
          command, "paymentMailingAd", paymentMailingAddressLine1);
        db.SetNullableString(command, "paymentAddressLi", paymentAddressLine2);
        db.SetNullableString(command, "paymentCity", paymentCity);
        db.SetNullableString(command, "paymentState", paymentState);
        db.SetNullableString(command, "paymentZipCode5", paymentZipCode5);
        db.SetNullableString(command, "paymentZipCode4", paymentZipCode4);
        db.SetNullableString(command, "zdelCpAddrLine1", "");
        db.SetNullableString(command, "zdelCpCity", "");
        db.SetNullableString(command, "zdelCpState", "");
        db.SetNullableString(command, "zdelCpZipCode5", "");
        db.SetNullableString(command, "zdelCpZipCode4", "");
        db.SetNullableString(command, "contactNameLast", contactNameLast);
        db.SetNullableString(command, "contactNameFirst", contactNameFirst);
        db.SetNullableString(command, "contactNameMiddl", contactNameMiddle);
        db.SetNullableString(command, "contactNameSuffi", contactNameSuffix);
        db.SetString(command, "contactAddrLine1", contactAddressLine1);
        db.SetNullableString(command, "contactAddrLine2", contactAddressLine2);
        db.SetNullableString(command, "contactCity", contactCity);
        db.SetNullableString(command, "contactState", contactState);
        db.SetNullableString(command, "contactZipCode5", contactZipCode5);
        db.SetNullableString(command, "contactZipCode4", contactZipCode4);
        db.SetNullableInt32(command, "contactPhoneNum", contactPhoneNum);
        db.SetNullableDate(command, "assnDeactDt", assnDeactDt);
        db.SetNullableString(command, "assnDeactInd", assnDeactInd);
        db.SetNullableDate(command, "lastDeferDt", lastDeferDt);
        db.SetNullableString(command, "memo", memo);
        db.SetNullableString(command, "contactPhoneExt", contactPhoneExtension);
        db.SetNullableInt32(command, "contactFaxNumber", contactFaxNumber);
        db.SetNullableInt32(command, "conFaxAreaCode", contactFaxAreaCode);
        db.
          SetNullableString(command, "conInternetAddr", contactInternetAddress);
          
        db.SetNullableString(command, "initDocketNum", initiatingDocketNumber);
        db.
          SetNullableString(command, "sendPaymBankAcc", sendPaymentsBankAccount);
          
        db.SetNullableInt64(command, "sendPaymRtCode", sendPaymentsRoutingCode);
        db.
          SetNullableString(command, "nondisclosureFind", nondisclosureFinding);
          
        db.SetNullableString(command, "respDocketNum", respondingDocketNumber);
        db.SetNullableString(command, "stateWithCej", stateWithCej);
        db.SetNullableString(command, "paymFipsCounty", paymentFipsCounty);
        db.SetNullableString(command, "paymentFipsState", paymentFipsState);
        db.SetNullableString(command, "paymFipsLocation", paymentFipsLocation);
        db.SetNullableInt32(command, "contactAreaCode", contactAreaCode);
      });

    entities.InterstateCase.LocalFipsState = localFipsState;
    entities.InterstateCase.LocalFipsCounty = localFipsCounty;
    entities.InterstateCase.LocalFipsLocation = localFipsLocation;
    entities.InterstateCase.OtherFipsState = otherFipsState;
    entities.InterstateCase.OtherFipsCounty = otherFipsCounty;
    entities.InterstateCase.OtherFipsLocation = otherFipsLocation;
    entities.InterstateCase.TransSerialNumber = transSerialNumber;
    entities.InterstateCase.ActionCode = actionCode;
    entities.InterstateCase.FunctionalTypeCode = functionalTypeCode;
    entities.InterstateCase.TransactionDate = transactionDate;
    entities.InterstateCase.KsCaseId = ksCaseId;
    entities.InterstateCase.InterstateCaseId = interstateCaseId;
    entities.InterstateCase.ActionReasonCode = actionReasonCode;
    entities.InterstateCase.ActionResolutionDate = actionResolutionDate;
    entities.InterstateCase.AttachmentsInd = attachmentsInd;
    entities.InterstateCase.CaseDataInd = caseDataInd;
    entities.InterstateCase.ApIdentificationInd = apIdentificationInd;
    entities.InterstateCase.ApLocateDataInd = apLocateDataInd;
    entities.InterstateCase.ParticipantDataInd = participantDataInd;
    entities.InterstateCase.OrderDataInd = orderDataInd;
    entities.InterstateCase.CollectionDataInd = collectionDataInd;
    entities.InterstateCase.InformationInd = informationInd;
    entities.InterstateCase.SentDate = sentDate;
    entities.InterstateCase.SentTime = sentTime;
    entities.InterstateCase.DueDate = dueDate;
    entities.InterstateCase.OverdueInd = overdueInd;
    entities.InterstateCase.DateReceived = dateReceived;
    entities.InterstateCase.TimeReceived = timeReceived;
    entities.InterstateCase.AttachmentsDueDate = attachmentsDueDate;
    entities.InterstateCase.InterstateFormsPrinted = interstateFormsPrinted;
    entities.InterstateCase.CaseType = caseType;
    entities.InterstateCase.CaseStatus = caseStatus;
    entities.InterstateCase.PaymentMailingAddressLine1 =
      paymentMailingAddressLine1;
    entities.InterstateCase.PaymentAddressLine2 = paymentAddressLine2;
    entities.InterstateCase.PaymentCity = paymentCity;
    entities.InterstateCase.PaymentState = paymentState;
    entities.InterstateCase.PaymentZipCode5 = paymentZipCode5;
    entities.InterstateCase.PaymentZipCode4 = paymentZipCode4;
    entities.InterstateCase.ContactNameLast = contactNameLast;
    entities.InterstateCase.ContactNameFirst = contactNameFirst;
    entities.InterstateCase.ContactNameMiddle = contactNameMiddle;
    entities.InterstateCase.ContactNameSuffix = contactNameSuffix;
    entities.InterstateCase.ContactAddressLine1 = contactAddressLine1;
    entities.InterstateCase.ContactAddressLine2 = contactAddressLine2;
    entities.InterstateCase.ContactCity = contactCity;
    entities.InterstateCase.ContactState = contactState;
    entities.InterstateCase.ContactZipCode5 = contactZipCode5;
    entities.InterstateCase.ContactZipCode4 = contactZipCode4;
    entities.InterstateCase.ContactPhoneNum = contactPhoneNum;
    entities.InterstateCase.AssnDeactDt = assnDeactDt;
    entities.InterstateCase.AssnDeactInd = assnDeactInd;
    entities.InterstateCase.LastDeferDt = lastDeferDt;
    entities.InterstateCase.Memo = memo;
    entities.InterstateCase.ContactPhoneExtension = contactPhoneExtension;
    entities.InterstateCase.ContactFaxNumber = contactFaxNumber;
    entities.InterstateCase.ContactFaxAreaCode = contactFaxAreaCode;
    entities.InterstateCase.ContactInternetAddress = contactInternetAddress;
    entities.InterstateCase.InitiatingDocketNumber = initiatingDocketNumber;
    entities.InterstateCase.SendPaymentsBankAccount = sendPaymentsBankAccount;
    entities.InterstateCase.SendPaymentsRoutingCode = sendPaymentsRoutingCode;
    entities.InterstateCase.NondisclosureFinding = nondisclosureFinding;
    entities.InterstateCase.RespondingDocketNumber = respondingDocketNumber;
    entities.InterstateCase.StateWithCej = stateWithCej;
    entities.InterstateCase.PaymentFipsCounty = paymentFipsCounty;
    entities.InterstateCase.PaymentFipsState = paymentFipsState;
    entities.InterstateCase.PaymentFipsLocation = paymentFipsLocation;
    entities.InterstateCase.ContactAreaCode = contactAreaCode;
    entities.InterstateCase.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequestHistory()
  {
    entities.InterstateRequestHistory.Populated = false;

    return Read("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          import.InterstateRequestHistory.CreatedTimestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 4);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 5);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 6);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 7);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 8);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private bool ReadOfficeAddress()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadServiceProviderAddress()
  {
    entities.ServiceProviderAddress.Populated = false;

    return Read("ReadServiceProviderAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProviderAddress.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProviderAddress.Type1 = db.GetString(reader, 1);
        entities.ServiceProviderAddress.Street1 = db.GetString(reader, 2);
        entities.ServiceProviderAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.ServiceProviderAddress.City = db.GetString(reader, 4);
        entities.ServiceProviderAddress.StateProvince = db.GetString(reader, 5);
        entities.ServiceProviderAddress.Zip = db.GetNullableString(reader, 6);
        entities.ServiceProviderAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.ServiceProviderAddress.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 8);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 9);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 11);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 12);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 13);
        entities.Office.Name = db.GetString(reader, 14);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 15);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 16);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private void UpdateInterstateCase1()
  {
    var caseType = local.InterstateCase.CaseType;

    entities.InterstateCase.Populated = false;
    Update("UpdateInterstateCase1",
      (db, command) =>
      {
        db.SetString(command, "caseType", caseType);
        db.SetInt64(
          command, "transSerialNbr", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "transactionDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
      });

    entities.InterstateCase.CaseType = caseType;
    entities.InterstateCase.Populated = true;
  }

  private void UpdateInterstateCase2()
  {
    var participantDataInd =
      local.InterstateCase.ParticipantDataInd.GetValueOrDefault();

    entities.InterstateCase.Populated = false;
    Update("UpdateInterstateCase2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "participantDataI", participantDataInd);
        db.SetInt64(
          command, "transSerialNbr", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "transactionDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
      });

    entities.InterstateCase.ParticipantDataInd = participantDataInd;
    entities.InterstateCase.Populated = true;
  }

  private void UpdateInterstateRequestHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateRequestHistory.Populated);

    var transactionSerialNum = local.InterstateCase.TransSerialNumber;

    entities.InterstateRequestHistory.Populated = false;
    Update("UpdateInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt64(command, "transactionSerial", transactionSerialNum);
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequestHistory.IntGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          entities.InterstateRequestHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.InterstateRequestHistory.TransactionSerialNum =
      transactionSerialNum;
    entities.InterstateRequestHistory.Populated = true;
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
    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of Person.
      /// </summary>
      [JsonPropertyName("person")]
      public CsePersonsWorkSet Person
      {
        get => person ??= new();
        set => person = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private Common sel;
      private CsePersonsWorkSet person;
    }

    /// <summary>A CourtOrderGroup group.</summary>
    [Serializable]
    public class CourtOrderGroup
    {
      /// <summary>
      /// A value of Details.
      /// </summary>
      [JsonPropertyName("details")]
      public LegalAction Details
      {
        get => details ??= new();
        set => details = value;
      }

      /// <summary>
      /// A value of PromptLacs.
      /// </summary>
      [JsonPropertyName("promptLacs")]
      public Common PromptLacs
      {
        get => promptLacs ??= new();
        set => promptLacs = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private LegalAction details;
      private Common promptLacs;
    }

    /// <summary>
    /// Gets a value of Participant.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantGroup> Participant => participant ??= new(
      ParticipantGroup.Capacity);

    /// <summary>
    /// Gets a value of Participant for json serialization.
    /// </summary>
    [JsonPropertyName("participant")]
    [Computed]
    public IList<ParticipantGroup> Participant_Json
    {
      get => participant;
      set => Participant.Assign(value);
    }

    /// <summary>
    /// A value of PaymentAddressReqd.
    /// </summary>
    [JsonPropertyName("paymentAddressReqd")]
    public Common PaymentAddressReqd
    {
      get => paymentAddressReqd ??= new();
      set => paymentAddressReqd = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// Gets a value of CourtOrder.
    /// </summary>
    [JsonIgnore]
    public Array<CourtOrderGroup> CourtOrder => courtOrder ??= new(
      CourtOrderGroup.Capacity);

    /// <summary>
    /// Gets a value of CourtOrder for json serialization.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    [Computed]
    public IList<CourtOrderGroup> CourtOrder_Json
    {
      get => courtOrder;
      set => CourtOrder.Assign(value);
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private Array<ParticipantGroup> participant;
    private Common paymentAddressReqd;
    private CsePerson ap;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private InterstateCase interstateCase;
    private InterstateRequestHistory interstateRequestHistory;
    private Array<CourtOrderGroup> courtOrder;
    private CsePersonsWorkSet ar;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CourtOrderGroup group.</summary>
    [Serializable]
    public class CourtOrderGroup
    {
      /// <summary>
      /// A value of Details.
      /// </summary>
      [JsonPropertyName("details")]
      public LegalAction Details
      {
        get => details ??= new();
        set => details = value;
      }

      /// <summary>
      /// A value of PromptLacs.
      /// </summary>
      [JsonPropertyName("promptLacs")]
      public Common PromptLacs
      {
        get => promptLacs ??= new();
        set => promptLacs = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private LegalAction details;
      private Common promptLacs;
    }

    /// <summary>
    /// Gets a value of CourtOrder.
    /// </summary>
    [JsonIgnore]
    public Array<CourtOrderGroup> CourtOrder => courtOrder ??= new(
      CourtOrderGroup.Capacity);

    /// <summary>
    /// Gets a value of CourtOrder for json serialization.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    [Computed]
    public IList<CourtOrderGroup> CourtOrder_Json
    {
      get => courtOrder;
      set => CourtOrder.Assign(value);
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private Array<CourtOrderGroup> courtOrder;
    private InterstateMiscellaneous interstateMiscellaneous;
    private InterstateApLocate interstateApLocate;
    private InterstateApIdentification interstateApIdentification;
    private CsePerson ap;
    private Case1 case1;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>
    /// A value of SupportOrderError.
    /// </summary>
    [JsonPropertyName("supportOrderError")]
    public Common SupportOrderError
    {
      get => supportOrderError ??= new();
      set => supportOrderError = value;
    }

    /// <summary>
    /// A value of FosterCareArrears.
    /// </summary>
    [JsonPropertyName("fosterCareArrears")]
    public Common FosterCareArrears
    {
      get => fosterCareArrears ??= new();
      set => fosterCareArrears = value;
    }

    /// <summary>
    /// A value of AfdcArrears.
    /// </summary>
    [JsonPropertyName("afdcArrears")]
    public Common AfdcArrears
    {
      get => afdcArrears ??= new();
      set => afdcArrears = value;
    }

    /// <summary>
    /// Gets a value of Participant.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantGroup> Participant => participant ??= new(
      ParticipantGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Participant for json serialization.
    /// </summary>
    [JsonPropertyName("participant")]
    [Computed]
    public IList<ParticipantGroup> Participant_Json
    {
      get => participant;
      set => Participant.Assign(value);
    }

    /// <summary>
    /// A value of ParticipantDbCreated.
    /// </summary>
    [JsonPropertyName("participantDbCreated")]
    public Common ParticipantDbCreated
    {
      get => participantDbCreated ??= new();
      set => participantDbCreated = value;
    }

    /// <summary>
    /// A value of ArrearsOnly.
    /// </summary>
    [JsonPropertyName("arrearsOnly")]
    public InterstateCase ArrearsOnly
    {
      get => arrearsOnly ??= new();
      set => arrearsOnly = value;
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
    /// A value of OrderDbCreated.
    /// </summary>
    [JsonPropertyName("orderDbCreated")]
    public Common OrderDbCreated
    {
      get => orderDbCreated ??= new();
      set => orderDbCreated = value;
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
    /// A value of Id.
    /// </summary>
    [JsonPropertyName("id")]
    public InterstateRequest Id
    {
      get => id ??= new();
      set => id = value;
    }

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
    /// A value of PaymentAddress.
    /// </summary>
    [JsonPropertyName("paymentAddress")]
    public InterstateCase PaymentAddress
    {
      get => paymentAddress ??= new();
      set => paymentAddress = value;
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
    /// A value of ContactOfficeAddress.
    /// </summary>
    [JsonPropertyName("contactOfficeAddress")]
    public OfficeAddress ContactOfficeAddress
    {
      get => contactOfficeAddress ??= new();
      set => contactOfficeAddress = value;
    }

    /// <summary>
    /// A value of ContactServiceProvider.
    /// </summary>
    [JsonPropertyName("contactServiceProvider")]
    public ServiceProvider ContactServiceProvider
    {
      get => contactServiceProvider ??= new();
      set => contactServiceProvider = value;
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
    /// A value of ContactOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("contactOfficeServiceProvider")]
    public OfficeServiceProvider ContactOfficeServiceProvider
    {
      get => contactOfficeServiceProvider ??= new();
      set => contactOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of TotalSupportOrder.
    /// </summary>
    [JsonPropertyName("totalSupportOrder")]
    public Common TotalSupportOrder
    {
      get => totalSupportOrder ??= new();
      set => totalSupportOrder = value;
    }

    private Common supportOrderError;
    private Common fosterCareArrears;
    private Common afdcArrears;
    private Array<ParticipantGroup> participant;
    private Common participantDbCreated;
    private InterstateCase arrearsOnly;
    private LegalAction legalAction;
    private Common orderDbCreated;
    private DateWorkArea current;
    private InterstateRequest id;
    private Program program;
    private InterstateCase paymentAddress;
    private InterstateCase interstateCase;
    private OfficeAddress contactOfficeAddress;
    private ServiceProvider contactServiceProvider;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private OfficeServiceProvider contactOfficeServiceProvider;
    private Common totalSupportOrder;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

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
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    private CaseRole caseRole;
    private CsePerson csePerson;
    private PersonProgram personProgram;
    private Program program;
    private ServiceProviderAddress serviceProviderAddress;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private Office office;
    private OfficeAddress officeAddress;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private InterstateRequest interstateRequest;
    private InterstateCase interstateCase;
    private InterstateRequestHistory interstateRequestHistory;
  }
#endregion
}
