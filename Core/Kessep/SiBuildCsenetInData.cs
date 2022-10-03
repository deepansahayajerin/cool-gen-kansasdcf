// Program: SI_BUILD_CSENET_IN_DATA, ID: 372621069, model: 746.
// Short name: SWE02329
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_BUILD_CSENET_IN_DATA.
/// </summary>
[Serializable]
public partial class SiBuildCsenetInData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BUILD_CSENET_IN_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBuildCsenetInData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBuildCsenetInData.
  /// </summary>
  public SiBuildCsenetInData(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    // ----------------------------------------------------------------------------
    // Date		Developer	Request
    // ----------------------------------------------------------------------------
    // 03/09/1999	Carl Ott
    // Initial Development
    // 06/04/1999	M Ramirez
    // Rewrite to reduce the occurence of rollbacks
    // 06/07/1999	M Ramirez
    // Changed READs for finding OSP
    // 07/02/1999	M Ramirez
    // Imported dates (current and max) instead of setting everytime
    // 07/27/1999	M Ramirez
    // On collection datablock, set posting_date to null_date, for fn_b617
    // 10/09/2000	C Scroggins	103717
    // Added code to prevent batch from updating code when the new information 
    // has an address from the State of Kansas.
    // 02/20/2001	M Ramirez	114212
    // Removed creates/updates for contact and payment addresses
    // 02/27/2001	M Ramirez	113334
    // Give KPC message to CSI-R txns with Case Type = V
    // Do not automatically reject CSI-P txns with Case Type = V
    // 02/27/2001	M Ramirez	113337
    // Do not send an alert for CSI-R txns
    // 02/27/2001	M Ramirez	114428
    // Do not automatically reject any LO1 txns with Case Type = V
    // 02/27/2001	M Ramirez	114429
    // CSI-R and LO1-R do not need case assignments, thus do not need KS Case Id
    // 02/27/2001	M Ramirez	114430
    // CSI-R do not need case assignments
    // 04/05/2001	M Ramirez	xxxxxx		LO1-R should not attempt to create 
    // infrastructure
    // 09/26/2001	M Ashworth	115337	Added code to fix Mona not being closed. Set
    // Denorm numeric12 to fips instead of serial#.  If AP data block is
    // populated, read ADABAS for ssn.
    // 03/7/2002	M Ashworth	138583	Added code to fix Mona not being closed for 
    // PAT, ENF, and EST Acknowledgements with reason codes of ANOAD or AADIN.
    // 03/7/2002	M Ashworth	133306 fix ability to access ALRT from ISTM.  A fix 
    // also needs to be added to the ALRT screen.
    // ---------------------------------------------------------------------------
    // 10/11/2010      R Mathews       CQ326 populate miscellaneous status 
    // change code for non IV-D cases
    // --------------------------------------------------------------------------------------------------
    if (import.InterstateCase.OtherFipsState == 99 && import
      .InterstateCase.OtherFipsCounty.GetValueOrDefault() == 999 && import
      .InterstateCase.OtherFipsLocation.GetValueOrDefault() == 99)
    {
      ++import.ExportPreviousRejection.Count;

      return;
    }

    // mjr
    // -----------------------------------------------
    // 06/04/1999
    // Check for a Non-IVD Case first
    // ------------------------------------------------------------
    if (Equal(import.InterstateCase.CaseType, "V"))
    {
      // *****************************************************************
      // This is a Non IV-D Case, which Kansas does not accept.  CSENet
      // transaction is created to send a rejection to the Initiating state.
      // *****************************************************************
      // mjr
      // -----------------------------------------------
      // 02/27/2001
      // PR# 114428 - Do not automatically reject any LO1 txns with Case Type = 
      // V
      // ------------------------------------------------------------
      if (Equal(import.InterstateCase.FunctionalTypeCode, "LO1"))
      {
        goto Test1;
      }

      // mjr
      // -----------------------------------------------
      // 02/27/2001
      // PR# 113334 - Do not automatically reject CSI-P txns with Case Type = V
      // ------------------------------------------------------------
      if (Equal(import.InterstateCase.FunctionalTypeCode, "CSI") && AsChar
        (import.InterstateCase.ActionCode) == 'P')
      {
        goto Test1;
      }

      // mjr
      // --------------------------------------------------
      // 06/04/1999
      // First, create an interstate_case and csenet_trans_envelope
      // to record the receipt of the transaction
      // ---------------------------------------------------------------
      try
      {
        CreateInterstateCase1();
        ++import.ExportCasesCreated.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            export.WriteError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate Case from CSENet data block";

            return;
          case ErrorCode.PermittedValueViolation:
            export.WriteError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate Case from CSENet data block";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      try
      {
        CreateCsenetTransactionEnvelop1();
        ++import.ExportUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create CSENet Transaction Envelope";

            return;
          case ErrorCode.PermittedValueViolation:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create CSENet Transaction Envelope";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // mjr
      // --------------------------------------------------
      // 06/04/1999
      // Now, create an interstate_case and csenet_trans_envelope
      // to send the rejection back to the other state
      // ---------------------------------------------------------------
      local.InterstateCase.Assign(entities.InterstateCase);
      local.InterstateCase.ActionCode = "P";

      switch(TrimEnd(import.InterstateCase.FunctionalTypeCode))
      {
        case "COL":
          local.InterstateCase.FunctionalTypeCode = "MSC";
          local.InterstateCase.ActionReasonCode = "";

          break;
        case "CSI":
          local.InterstateCase.FunctionalTypeCode = "CSI";
          local.InterstateCase.ActionReasonCode = "FUINF";

          break;
        case "ENF":
          local.InterstateCase.FunctionalTypeCode = "MSC";
          local.InterstateCase.ActionReasonCode = "";

          break;
        case "EST":
          local.InterstateCase.FunctionalTypeCode = "EST";
          local.InterstateCase.ActionReasonCode = "SUDEN";

          break;
        case "LO1":
          local.InterstateCase.FunctionalTypeCode = "LO1";
          local.InterstateCase.ActionReasonCode = "LUALL";

          break;
        case "MSC":
          local.InterstateCase.FunctionalTypeCode = "MSC";
          local.InterstateCase.ActionReasonCode = "";

          break;
        case "PAT":
          local.InterstateCase.FunctionalTypeCode = "PAT";
          local.InterstateCase.ActionReasonCode = "PUDEN";

          break;
        default:
          break;
      }

      // mjr
      // --------------------------------------------------
      // 02/27/2001
      // Don't send out an AP ID datablock, if they didn't provide one.
      // ---------------------------------------------------------------
      local.InterstateCase.ApLocateDataInd = 0;
      local.InterstateCase.CaseDataInd = 1;
      local.InterstateCase.InformationInd = 1;
      local.InterstateCase.CollectionDataInd = 0;
      local.InterstateCase.OrderDataInd = 0;
      local.InterstateCase.ParticipantDataInd = 0;
      UseSiGenCsenetTransactSerialNo();

      try
      {
        CreateInterstateCase2();
        ++import.ExportUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate Case for Non IV - D reject";

            return;
          case ErrorCode.PermittedValueViolation:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate Case for Non IV - D reject";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      try
      {
        CreateCsenetTransactionEnvelop2();
        ++import.ExportUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create CSENet Transaction Envelope";

            return;
          case ErrorCode.PermittedValueViolation:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create CSENet Transaction Envelope";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // mjr
      // --------------------------------------------------
      // 02/27/2001
      // Don't send out an AP ID datablock, if they didn't provide one.
      // ---------------------------------------------------------------
      if (import.InterstateCase.ApIdentificationInd.GetValueOrDefault() == 1)
      {
        try
        {
          CreateInterstateApIdentification();
          ++import.ExportUpdates.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              export.WriteError.Flag = "Y";
              export.RollbackError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "Unable to create Interstate AP Identification";

              return;
            case ErrorCode.PermittedValueViolation:
              export.WriteError.Flag = "Y";
              export.RollbackError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "Unable to create Interstate AP Identification";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // mjr
      // -----------------------------------------------
      // 02/27/2001
      // PR# 113334 - Give KPC message to CSI-R txns with Case Type = V
      // ------------------------------------------------------------
      if (Equal(import.InterstateCase.FunctionalTypeCode, "CSI") && AsChar
        (import.InterstateCase.ActionCode) == 'R')
      {
        local.InterstateMiscellaneous.InformationTextLine1 =
          "Non IV-D cases are not currently processed by State of Kansas Child Support";
          
        local.InterstateMiscellaneous.InformationTextLine2 =
          " Enforcement.  For CSI response on Non IV-D cases contact KANSAS PAY CENTER at";
          
        local.InterstateMiscellaneous.InformationTextLine3 =
          " PO Box 758599, Topeka, KS  66675-8599, or (877) 572-5722 for information on";
          
        local.InterstateMiscellaneous.InformationTextLine4 = " requested case.";
      }
      else
      {
        local.InterstateMiscellaneous.InformationTextLine1 =
          "Non IV-D cases are not currently processed by State of Kansas Child Support";
          
        local.InterstateMiscellaneous.InformationTextLine2 = " Enforcement.";
        local.InterstateMiscellaneous.InformationTextLine3 = "";
        local.InterstateMiscellaneous.InformationTextLine4 = "";
      }

      // -----------------------------------------------------------------------------
      // 10/11/2010   CQ326   Move case status to status change code instead of 
      // spaces
      // -----------------------------------------------------------------------------
      try
      {
        CreateInterstateMiscellaneous1();
        ++import.ExportUpdates.Count;
        ++import.ExportNonIvdRejected.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate Misc. DB for Non IV-D reject";

            return;
          case ErrorCode.PermittedValueViolation:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate Misc. DB for Non IV-D reject";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      return;
    }

Test1:

    // mjr
    // -----------------------------------------------
    // 02/27/2001
    // PR# 114429 - CSI-R and LO1-R do not need case assignments,
    // thus do not need KS Case Id
    // ------------------------------------------------------------
    if (Equal(import.InterstateCase.FunctionalTypeCode, "CSI") && AsChar
      (import.InterstateCase.ActionCode) == 'R')
    {
    }
    else if (Equal(import.InterstateCase.FunctionalTypeCode, "LO1") && AsChar
      (import.InterstateCase.ActionCode) != 'P' && AsChar
      (import.InterstateCase.ActionCode) != 'U')
    {
    }
    else if (Equal(import.InterstateCase.FunctionalTypeCode, "COL"))
    {
    }
    else
    {
      // mjr
      // -----------------------------------------------
      // 06/04/1999
      // Move validation of data before any Creates/Updates
      // ------------------------------------------------------------
      local.Case1.Number = Substring(import.InterstateCase.KsCaseId, 1, 10);

      if (ReadCase())
      {
        local.Case1.Number = entities.Case1.Number;

        if (ReadCaseAssignment())
        {
          if (ReadOfficeServiceProvider2())
          {
            local.OspFound.Flag = "Y";

            if (ReadOffice())
            {
              export.Office.SystemGeneratedId =
                entities.Office.SystemGeneratedId;

              if (entities.Office.SystemGeneratedId == 21)
              {
                local.Office21.Flag = "Y";
              }
            }
            else
            {
              // mjr
              // ------------------------------------------------
              // 06/07/1999
              // OSP is missing an Office.
              // -------------------------------------------------------------
              export.WriteError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "OSP assigned to Kansas Case is missing an Office";

              return;
            }
          }
          else
          {
            // mjr
            // ------------------------------------------------
            // 06/07/1999
            // CASE_ASSIGNMENT is missing an OSP.
            // -------------------------------------------------------------
            export.WriteError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Kansas Case assignment is missing OSP";

            return;
          }
        }
        else
        {
          // mjr
          // ------------------------------------------------
          // 06/07/1999
          // CASE is not assigned.
          // -------------------------------------------------------------
          export.WriteError.Flag = "Y";
          export.ErrorMessage.Text60 = "Kansas Case is unassigned";

          return;
        }
      }
      else
      {
        // --------------------------------------------------------------
        // Existing Kansas case was not found... continue as new referral
        // --------------------------------------------------------------
      }

      if (AsChar(local.OspFound.Flag) != 'Y')
      {
        if (import.InterstateCase.ApIdentificationInd.GetValueOrDefault() <= 0)
        {
          if (ReadOfficeCaseloadAssignment1())
          {
            local.Office21.Flag = "Y";
            export.Office.SystemGeneratedId = 21;
            local.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
              entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier;
          }
        }
        else
        {
          local.ApLastNameAlpha.Text6 =
            import.InterstateApIdentification.NameLast ?? Spaces(6);
          local.ApFirstNameAlpha.Text1 =
            import.InterstateApIdentification.NameFirst;

          foreach(var item in ReadOfficeCaseloadAssignment2())
          {
            if (Equal(entities.OfficeCaseloadAssignment.BeginingAlpha,
              local.ApLastNameAlpha.Text6) && AsChar
              (entities.OfficeCaseloadAssignment.BeginningFirstIntial) > AsChar
              (local.ApFirstNameAlpha.Text1))
            {
              continue;
            }

            if (Equal(entities.OfficeCaseloadAssignment.EndingAlpha,
              local.ApLastNameAlpha.Text6) && AsChar
              (entities.OfficeCaseloadAssignment.EndingFirstInitial) < AsChar
              (local.ApFirstNameAlpha.Text1))
            {
              continue;
            }

            local.Office21.Flag = "Y";
            export.Office.SystemGeneratedId = 21;
            local.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
              entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier;

            break;
          }
        }

        if (local.OfficeCaseloadAssignment.SystemGeneratedIdentifier <= 0)
        {
          export.WriteError.Flag = "Y";
          export.ErrorMessage.Text60 =
            "Office 21 is missing a default Referral (RE) Assignment";

          return;
        }

        if (ReadOfficeServiceProvider1())
        {
          local.OspFound.Flag = "Y";
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.ErrorMessage.Text60 =
            "OSP missing for Office Caseload Assignment";

          return;
        }
      }

      if (ReadServiceProvider())
      {
        export.ServiceProvider.UserId = entities.ServiceProvider.UserId;
      }
      else
      {
        export.WriteError.Flag = "Y";
        export.ErrorMessage.Text60 = "Service Provider missing for OSP";

        return;
      }
    }

    // mjr
    // -----------------------------------------------
    // 06/04/1999
    // Validation of data completed.  Now process transaction as normal.
    // ------------------------------------------------------------
    try
    {
      CreateInterstateCase1();
      ++import.ExportUpdates.Count;
      ++import.ExportCasesCreated.Count;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          export.WriteError.Flag = "Y";
          export.ErrorMessage.Text60 =
            "Unable to create Interstate Case from CSENet data block";

          return;
        case ErrorCode.PermittedValueViolation:
          export.WriteError.Flag = "Y";
          export.ErrorMessage.Text60 =
            "Unable to create Interstate Case from CSENet data block";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // mjr
    // ------------------------------------------------------
    // 06/04/1999
    // Every error that occurs after this point needs to cause a rollback.
    // -------------------------------------------------------------------
    try
    {
      CreateCsenetTransactionEnvelop3();
      ++import.ExportUpdates.Count;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          export.WriteError.Flag = "Y";
          export.RollbackError.Flag = "Y";
          export.ErrorMessage.Text60 =
            "Unable to create CSENet Transaction Envelope";

          return;
        case ErrorCode.PermittedValueViolation:
          export.WriteError.Flag = "Y";
          export.RollbackError.Flag = "Y";
          export.ErrorMessage.Text60 =
            "Unable to create CSENet Transaction Envelope";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // mjr
    // ------------------------------------------------------
    // 06/04/1999
    // Process AP ID datablock
    // -------------------------------------------------------------------
    if (import.InterstateCase.ApIdentificationInd.GetValueOrDefault() == 1)
    {
      try
      {
        CreateInterstateApIdentification();
        ++import.ExportUpdates.Count;
        ++import.ExportApIdentCreated.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate AP Identification";

            return;
          case ErrorCode.PermittedValueViolation:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate AP Identification";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // mjr
      // ------------------------------------------------------
      // 06/04/1999
      // Process Locate datablock (only valid with AP ID datablock)
      // -------------------------------------------------------------------
      if (import.InterstateCase.ApLocateDataInd.GetValueOrDefault() == 1)
      {
        try
        {
          CreateInterstateApLocate();
          ++import.ExportUpdates.Count;
          ++import.ExportApLocateCreated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              export.WriteError.Flag = "Y";
              export.RollbackError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "Unable to create Interstate AP Locate";

              return;
            case ErrorCode.PermittedValueViolation:
              export.WriteError.Flag = "Y";
              export.RollbackError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "Unable to create Interstate AP Locate";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    // mjr
    // ------------------------------------------------------
    // 06/04/1999
    // Process Participant datablock
    // -------------------------------------------------------------------
    if (import.InterstateCase.ParticipantDataInd.GetValueOrDefault() > 0)
    {
      for(import.Participant.Index = 0; import.Participant.Index < import
        .Participant.Count; ++import.Participant.Index)
      {
        if (local.InterstateParticipant.SystemGeneratedSequenceNum == 9)
        {
          goto Test2;
        }

        ++local.InterstateParticipant.SystemGeneratedSequenceNum;

        try
        {
          CreateInterstateParticipant();
          ++import.ExportUpdates.Count;
          ++import.ExportPartCreated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              export.WriteError.Flag = "Y";
              export.RollbackError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "Unable to create Interstate Participant";

              return;
            case ErrorCode.PermittedValueViolation:
              export.WriteError.Flag = "Y";
              export.RollbackError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "Unable to create Interstate Participant";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

Test2:

    // mjr
    // ------------------------------------------------------
    // 06/04/1999
    // Process Order datablock
    // -------------------------------------------------------------------
    if (import.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
    {
      for(import.Order.Index = 0; import.Order.Index < import.Order.Count; ++
        import.Order.Index)
      {
        if (local.InterstateSupportOrder.SystemGeneratedSequenceNum == 9)
        {
          goto Test3;
        }

        ++local.InterstateSupportOrder.SystemGeneratedSequenceNum;

        try
        {
          CreateInterstateSupportOrder();
          ++import.ExportUpdates.Count;
          ++import.ExportOrdersCreated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              export.WriteError.Flag = "Y";
              export.RollbackError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "Unable to create Interstate Support Order";

              return;
            case ErrorCode.PermittedValueViolation:
              export.WriteError.Flag = "Y";
              export.RollbackError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "Unable to create Interstate Support Order";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

Test3:

    // mjr
    // ------------------------------------------------------
    // 06/04/1999
    // Process Collection datablock
    // -------------------------------------------------------------------
    if (import.InterstateCase.CollectionDataInd.GetValueOrDefault() > 0)
    {
      for(import.Collection.Index = 0; import.Collection.Index < import
        .Collection.Count; ++import.Collection.Index)
      {
        if (local.InterstateCollection.SystemGeneratedSequenceNum == 9)
        {
          goto Test4;
        }

        ++local.InterstateCollection.SystemGeneratedSequenceNum;

        try
        {
          CreateInterstateCollection();
          ++import.ExportUpdates.Count;
          ++import.ExportCollectionCreated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              export.WriteError.Flag = "Y";
              export.RollbackError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "Unable to create Interstate Collection";

              return;
            case ErrorCode.PermittedValueViolation:
              export.WriteError.Flag = "Y";
              export.RollbackError.Flag = "Y";
              export.ErrorMessage.Text60 =
                "Unable to create Interstate Collection";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

Test4:

    // mjr
    // ------------------------------------------------------
    // 06/04/1999
    // Process Information datablock
    // -------------------------------------------------------------------
    if (import.InterstateCase.InformationInd.GetValueOrDefault() == 1)
    {
      try
      {
        CreateInterstateMiscellaneous2();
        ++import.ExportUpdates.Count;
        ++import.ExportMiscCreated.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate Miscellaneous";

            return;
          case ErrorCode.PermittedValueViolation:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate Miscellaneous";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // mjr
    // ------------------------------------------------------
    // 06/04/1999
    // Add Interstate Request History if an Interstate Request already exists
    // -------------------------------------------------------------------
    if (!IsEmpty(entities.Case1.Number))
    {
      // mca
      // ------------------------------------------------------
      // 09/26/2001
      // PR115337 Added code to fix Mona not being closed. Set Denorm numeric12 
      // to fips instead of serial#.  If AP data block is populated, read ADABAS
      // for ssn.
      // -------------------------------------------------------------------
      ReadAbsentParent();

      // mca
      // ------------------------------------------------------
      // 09/26/2001
      // PR115337 If there is more than one ap, we must qualify the ap by SSN.
      // -------------------------------------------------------------------
      if (local.CountAbsParentsPerCase.Count == 1)
      {
        if (ReadAbsentParentCsePerson1())
        {
          local.CsePerson.Number = entities.CsePerson.Number;
        }
        else
        {
          local.CsePerson.Number = "";
        }
      }
      else if (import.InterstateCase.ApIdentificationInd.GetValueOrDefault() ==
        1 && (!IsEmpty(import.InterstateApIdentification.Ssn) || !
        IsEmpty(import.InterstateApIdentification.AliasSsn1) || !
        IsEmpty(import.InterstateApIdentification.AliasSsn2)))
      {
        foreach(var item in ReadAbsentParentCsePerson2())
        {
          // ************************************************
          // *Call EAB to retrieve information about a CSE  *
          // *PERSON from the ADABAS system.                *
          // ************************************************
          local.CsePersonsWorkSet.Ssn = "";
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          UseCabReadAdabasPersonBatch();

          if (!IsExitState("ACO_NN0000_ALL_OK") || !
            IsEmpty(local.AbendData.Type1))
          {
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Problem reading ADABAS.  CSE Person #: " + entities
              .CsePerson.Number;

            return;
          }

          if (!IsEmpty(local.CsePersonsWorkSet.Ssn) && !
            Equal(local.CsePersonsWorkSet.Ssn, "000000000"))
          {
            if (!IsEmpty(import.InterstateApIdentification.Ssn) && !
              Equal(import.InterstateApIdentification.Ssn, "000000000"))
            {
              if (Equal(import.InterstateApIdentification.Ssn,
                local.CsePersonsWorkSet.Ssn))
              {
                local.CsePerson.Number = entities.CsePerson.Number;

                goto Test5;
              }
            }

            if (!IsEmpty(import.InterstateApIdentification.AliasSsn1) && !
              Equal(import.InterstateApIdentification.AliasSsn1, "000000000"))
            {
              if (Equal(import.InterstateApIdentification.AliasSsn1,
                local.CsePersonsWorkSet.Ssn))
              {
                local.CsePerson.Number = entities.CsePerson.Number;

                goto Test5;
              }
            }

            if (!IsEmpty(import.InterstateApIdentification.AliasSsn2) && !
              Equal(import.InterstateApIdentification.AliasSsn2, "000000000"))
            {
              if (Equal(import.InterstateApIdentification.AliasSsn2,
                local.CsePersonsWorkSet.Ssn))
              {
                local.CsePerson.Number = entities.CsePerson.Number;

                goto Test5;
              }
            }
          }
        }
      }

Test5:

      if (!IsEmpty(local.CsePerson.Number))
      {
        if (ReadInterstateRequest())
        {
          try
          {
            CreateInterstateRequestHistory();
            ++import.ExportUpdates.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                export.WriteError.Flag = "Y";
                export.RollbackError.Flag = "Y";
                export.ErrorMessage.Text60 =
                  "Unable to create Interstate Request History";

                return;
              case ErrorCode.PermittedValueViolation:
                export.WriteError.Flag = "Y";
                export.RollbackError.Flag = "Y";
                export.ErrorMessage.Text60 =
                  "Unable to create Interstate Request History";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // mjr
          // ------------------------------------------
          // 02/20/2001
          // PR# 114212 - Removed creates/updates to contact
          // and payment addresses
          // -------------------------------------------------------
        }
        else
        {
          // ****************************************************************
          // There is not an existing Interstate Request
          // ****************************************************************
        }
      }
    }

    // ****************************************************************
    // Create Interstate Case Assignment
    // ****************************************************************
    // mjr
    // ------------------------------------------------------
    // 02/27/2001
    // Changed format of IF statement
    // -------------------------------------------------------------------
    if (Equal(import.InterstateCase.FunctionalTypeCode, "CSI") && AsChar
      (import.InterstateCase.ActionCode) == 'R')
    {
    }
    else if (Equal(import.InterstateCase.FunctionalTypeCode, "LO1") && AsChar
      (import.InterstateCase.ActionCode) != 'P' && AsChar
      (import.InterstateCase.ActionCode) != 'U')
    {
    }
    else if (Equal(import.InterstateCase.FunctionalTypeCode, "COL"))
    {
    }
    else
    {
      try
      {
        CreateInterstateCaseAssignment();
        ++import.ExportUpdates.Count;
        ++import.ExportCaseAssigns.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate Case Assignment";

            return;
          case ErrorCode.PermittedValueViolation:
            export.WriteError.Flag = "Y";
            export.RollbackError.Flag = "Y";
            export.ErrorMessage.Text60 =
              "Unable to create Interstate Case Assignment";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ****************************************************************
    // Create Infrastructure to enable Alert to worker, except for 
    // Acknowledgement transactions and Interstate cases assigned to office 21.
    // ****************************************************************
    if (AsChar(local.Office21.Flag) != 'Y')
    {
      // mca
      // -----------------------------------------------
      // 03/07/2002
      // 03/07/2002	M Ashworth	138583	Added code to fix Mona not being closed 
      // for PAT, ENF, and EST Acknowledgements with reason codes of ANOAD or
      // AADIN.
      // Since these are the only acknowledgements, this code will be commented 
      // out Mike R. / Mark A.
      // ------------------------------------------------------------
      // mjr
      // -----------------------------------------------
      // 02/27/2001
      // PR# 113337 - Do not send an alert for CSI-R txns
      // ------------------------------------------------------------
      if (Equal(import.InterstateCase.FunctionalTypeCode, "CSI") && AsChar
        (import.InterstateCase.ActionCode) == 'R')
      {
        return;
      }

      // mjr
      // -----------------------------------------------
      // 04/05/2001
      // PR# XXXXXX - Do not send an alert for LO1-R txns
      // ------------------------------------------------------------
      if (Equal(import.InterstateCase.FunctionalTypeCode, "LO1") && AsChar
        (import.InterstateCase.ActionCode) != 'P' && AsChar
        (import.InterstateCase.ActionCode) != 'U')
      {
        return;
      }

      local.Infrastructure.Function = import.InterstateCase.FunctionalTypeCode;
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.ReferenceDate = import.Current.Date;
      local.Infrastructure.DenormDate = import.InterstateCase.TransactionDate;
      local.Infrastructure.CreatedBy = import.ProgramProcessingInfo.Name;
      local.Infrastructure.LastUpdatedBy = import.ProgramProcessingInfo.Name;
      local.Infrastructure.UserId = import.ProgramProcessingInfo.Name;
      local.Infrastructure.CreatedTimestamp = import.Current.Timestamp;
      local.Infrastructure.DenormTimestamp =
        local.Infrastructure.CreatedTimestamp;
      local.Infrastructure.LastUpdatedTimestamp =
        local.Infrastructure.CreatedTimestamp;
      local.Infrastructure.CsenetInOutCode = "I";
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.EventType = "CSENET";
      local.Infrastructure.ReasonCode =
        import.InterstateCase.ActionReasonCode ?? Spaces(15);

      if (IsEmpty(import.InterstateCase.KsCaseId))
      {
        local.Infrastructure.EventId = 16;
        local.Infrastructure.InitiatingStateCode =
          import.InterstateCase.ContactState ?? Spaces(2);
      }
      else
      {
        if (IsEmpty(import.InterstateCase.InterstateCaseId))
        {
          local.Infrastructure.EventId = 12;
        }
        else
        {
          local.Infrastructure.EventId = 15;
        }

        local.Infrastructure.CaseNumber = local.Case1.Number;
        local.Infrastructure.InitiatingStateCode = "KS";
      }

      local.Infrastructure.CsePersonNumber = local.CsePerson.Number;
      local.Infrastructure.DenormNumeric12 =
        import.InterstateCase.OtherFipsState;

      // mca
      // -----------------------------------------------
      // 03/07/2002
      // 03/7/2002	M Ashworth	133306 fix ability to access ALRT from ISTM.  A 
      // fix also needs to be added to the ALRT screen.
      // ------------------------------------------------------------
      local.Infrastructure.DenormText12 =
        NumberToString(import.InterstateCase.TransSerialNumber, 12);
      UseSpCabCreateInfrastructure();

      if (IsExitState("SP0000_EVENT_DETAIL_NF"))
      {
        export.WriteError.Flag = "Y";
        export.ErrorMessage.Text60 = "Detail not found for Event " + NumberToString
          (local.Infrastructure.EventId, 14, 2) + ", Reason " + local
          .Infrastructure.ReasonCode;
        ExitState = "ACO_NN0000_ALL_OK";

        return;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }

      ++import.ExportUpdates.Count;
    }
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.Ae.Flag = useExport.Ae.Flag;
    local.AbendData.Type1 = useExport.AbendData.Type1;
  }

  private void UseSiGenCsenetTransactSerialNo()
  {
    var useImport = new SiGenCsenetTransactSerialNo.Import();
    var useExport = new SiGenCsenetTransactSerialNo.Export();

    Call(SiGenCsenetTransactSerialNo.Execute, useImport, useExport);

    MoveInterstateCase(useExport.InterstateCase, local.InterstateCase);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private void CreateCsenetTransactionEnvelop1()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = import.Current.Timestamp;
    var directionInd = "I";
    var processingStatusCode = "P";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("CreateCsenetTransactionEnvelop1",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "directionInd", directionInd);
        db.SetString(command, "processingStatus", processingStatusCode);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTstamp", lastUpdatedTimestamp);
        db.SetNullableString(command, "errorCode", "");
      });

    entities.CsenetTransactionEnvelop.CcaTransactionDt = ccaTransactionDt;
    entities.CsenetTransactionEnvelop.CcaTransSerNum = ccaTransSerNum;
    entities.CsenetTransactionEnvelop.LastUpdatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.DirectionInd = directionInd;
    entities.CsenetTransactionEnvelop.ProcessingStatusCode =
      processingStatusCode;
    entities.CsenetTransactionEnvelop.CreatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.CreatedTstamp = lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.Populated = true;
  }

  private void CreateCsenetTransactionEnvelop2()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = import.Current.Timestamp;
    var directionInd = "O";
    var processingStatusCode = "S";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("CreateCsenetTransactionEnvelop2",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "directionInd", directionInd);
        db.SetString(command, "processingStatus", processingStatusCode);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTstamp", lastUpdatedTimestamp);
        db.SetNullableString(command, "errorCode", "");
      });

    entities.CsenetTransactionEnvelop.CcaTransactionDt = ccaTransactionDt;
    entities.CsenetTransactionEnvelop.CcaTransSerNum = ccaTransSerNum;
    entities.CsenetTransactionEnvelop.LastUpdatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.DirectionInd = directionInd;
    entities.CsenetTransactionEnvelop.ProcessingStatusCode =
      processingStatusCode;
    entities.CsenetTransactionEnvelop.CreatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.CreatedTstamp = lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.Populated = true;
  }

  private void CreateCsenetTransactionEnvelop3()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = import.Current.Timestamp;
    var directionInd = "I";
    var processingStatusCode = "C";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("CreateCsenetTransactionEnvelop3",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "directionInd", directionInd);
        db.SetString(command, "processingStatus", processingStatusCode);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTstamp", lastUpdatedTimestamp);
        db.SetNullableString(command, "errorCode", "");
      });

    entities.CsenetTransactionEnvelop.CcaTransactionDt = ccaTransactionDt;
    entities.CsenetTransactionEnvelop.CcaTransSerNum = ccaTransSerNum;
    entities.CsenetTransactionEnvelop.LastUpdatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.DirectionInd = directionInd;
    entities.CsenetTransactionEnvelop.ProcessingStatusCode =
      processingStatusCode;
    entities.CsenetTransactionEnvelop.CreatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.CreatedTstamp = lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.Populated = true;
  }

  private void CreateInterstateApIdentification()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var aliasSsn2 = import.InterstateApIdentification.AliasSsn2 ?? "";
    var aliasSsn1 = import.InterstateApIdentification.AliasSsn1 ?? "";
    var otherIdInfo = import.InterstateApIdentification.OtherIdInfo ?? "";
    var eyeColor = import.InterstateApIdentification.EyeColor ?? "";
    var hairColor = import.InterstateApIdentification.HairColor ?? "";
    var weight = import.InterstateApIdentification.Weight.GetValueOrDefault();
    var heightIn =
      import.InterstateApIdentification.HeightIn.GetValueOrDefault();
    var heightFt =
      import.InterstateApIdentification.HeightFt.GetValueOrDefault();
    var placeOfBirth = import.InterstateApIdentification.PlaceOfBirth ?? "";
    var ssn = import.InterstateApIdentification.Ssn ?? "";
    var race = import.InterstateApIdentification.Race ?? "";
    var sex = import.InterstateApIdentification.Sex ?? "";
    var dateOfBirth = import.InterstateApIdentification.DateOfBirth;
    var nameSuffix = import.InterstateApIdentification.NameSuffix ?? "";
    var nameFirst = import.InterstateApIdentification.NameFirst;
    var nameLast = import.InterstateApIdentification.NameLast ?? "";
    var middleName = import.InterstateApIdentification.MiddleName ?? "";
    var possiblyDangerous =
      import.InterstateApIdentification.PossiblyDangerous ?? "";
    var maidenName = import.InterstateApIdentification.MaidenName ?? "";
    var mothersMaidenOrFathersName =
      import.InterstateApIdentification.MothersMaidenOrFathersName ?? "";

    entities.InterstateApIdentification.Populated = false;
    Update("CreateInterstateApIdentification",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetNullableString(command, "altSsn2", aliasSsn2);
        db.SetNullableString(command, "altSsn1", aliasSsn1);
        db.SetNullableString(command, "otherIdInfo", otherIdInfo);
        db.SetNullableString(command, "eyeColor", eyeColor);
        db.SetNullableString(command, "hairColor", hairColor);
        db.SetNullableInt32(command, "weight", weight);
        db.SetNullableInt32(command, "heightIn", heightIn);
        db.SetNullableInt32(command, "heightFt", heightFt);
        db.SetNullableString(command, "birthPlaceCity", placeOfBirth);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "race", race);
        db.SetNullableString(command, "sex", sex);
        db.SetNullableDate(command, "birthDate", dateOfBirth);
        db.SetNullableString(command, "suffix", nameSuffix);
        db.SetString(command, "nameFirst", nameFirst);
        db.SetNullableString(command, "nameLast", nameLast);
        db.SetNullableString(command, "middleName", middleName);
        db.SetNullableString(command, "possiblyDangerous", possiblyDangerous);
        db.SetNullableString(command, "maidenName", maidenName);
        db.SetNullableString(
          command, "mthMaidOrFathN", mothersMaidenOrFathersName);
      });

    entities.InterstateApIdentification.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateApIdentification.CcaTransSerNum = ccaTransSerNum;
    entities.InterstateApIdentification.AliasSsn2 = aliasSsn2;
    entities.InterstateApIdentification.AliasSsn1 = aliasSsn1;
    entities.InterstateApIdentification.OtherIdInfo = otherIdInfo;
    entities.InterstateApIdentification.EyeColor = eyeColor;
    entities.InterstateApIdentification.HairColor = hairColor;
    entities.InterstateApIdentification.Weight = weight;
    entities.InterstateApIdentification.HeightIn = heightIn;
    entities.InterstateApIdentification.HeightFt = heightFt;
    entities.InterstateApIdentification.PlaceOfBirth = placeOfBirth;
    entities.InterstateApIdentification.Ssn = ssn;
    entities.InterstateApIdentification.Race = race;
    entities.InterstateApIdentification.Sex = sex;
    entities.InterstateApIdentification.DateOfBirth = dateOfBirth;
    entities.InterstateApIdentification.NameSuffix = nameSuffix;
    entities.InterstateApIdentification.NameFirst = nameFirst;
    entities.InterstateApIdentification.NameLast = nameLast;
    entities.InterstateApIdentification.MiddleName = middleName;
    entities.InterstateApIdentification.PossiblyDangerous = possiblyDangerous;
    entities.InterstateApIdentification.MaidenName = maidenName;
    entities.InterstateApIdentification.MothersMaidenOrFathersName =
      mothersMaidenOrFathersName;
    entities.InterstateApIdentification.Populated = true;
  }

  private void CreateInterstateApLocate()
  {
    System.Diagnostics.Debug.Assert(
      entities.InterstateApIdentification.Populated);

    var cncTransactionDt = entities.InterstateApIdentification.CcaTransactionDt;
    var cncTransSerlNbr = entities.InterstateApIdentification.CcaTransSerNum;
    var employerEin = import.InterstateApLocate.EmployerEin.GetValueOrDefault();
    var employerName = import.InterstateApLocate.EmployerName ?? "";
    var employerPhoneNum =
      import.InterstateApLocate.EmployerPhoneNum.GetValueOrDefault();
    var employerEffectiveDate = import.InterstateApLocate.EmployerEffectiveDate;
    var employerEndDate = import.InterstateApLocate.EmployerEndDate;
    var employerConfirmedInd =
      import.InterstateApLocate.EmployerConfirmedInd ?? "";
    var residentialAddressLine1 =
      import.InterstateApLocate.ResidentialAddressLine1 ?? "";
    var residentialAddressLine2 =
      import.InterstateApLocate.ResidentialAddressLine2 ?? "";
    var residentialCity = import.InterstateApLocate.ResidentialCity ?? "";
    var residentialState = import.InterstateApLocate.ResidentialState ?? "";
    var residentialZipCode5 = import.InterstateApLocate.ResidentialZipCode5 ?? ""
      ;
    var residentialZipCode4 = import.InterstateApLocate.ResidentialZipCode4 ?? ""
      ;
    var mailingAddressLine1 = import.InterstateApLocate.MailingAddressLine1 ?? ""
      ;
    var mailingAddressLine2 = import.InterstateApLocate.MailingAddressLine2 ?? ""
      ;
    var mailingCity = import.InterstateApLocate.MailingCity ?? "";
    var mailingState = import.InterstateApLocate.MailingState ?? "";
    var mailingZipCode5 = import.InterstateApLocate.MailingZipCode5 ?? "";
    var mailingZipCode4 = import.InterstateApLocate.MailingZipCode4 ?? "";
    var residentialAddressEffectvDate =
      import.InterstateApLocate.ResidentialAddressEffectvDate;
    var residentialAddressEndDate =
      import.InterstateApLocate.ResidentialAddressEndDate;
    var residentialAddressConfirmInd =
      import.InterstateApLocate.ResidentialAddressConfirmInd ?? "";
    var mailingAddressEffectiveDate =
      import.InterstateApLocate.MailingAddressEffectiveDate;
    var mailingAddressEndDate = import.InterstateApLocate.MailingAddressEndDate;
    var mailingAddressConfirmedInd =
      import.InterstateApLocate.MailingAddressConfirmedInd ?? "";
    var homePhoneNumber =
      import.InterstateApLocate.HomePhoneNumber.GetValueOrDefault();
    var workPhoneNumber =
      import.InterstateApLocate.WorkPhoneNumber.GetValueOrDefault();
    var driversLicState = import.InterstateApLocate.DriversLicState ?? "";
    var driversLicenseNum = import.InterstateApLocate.DriversLicenseNum ?? "";
    var alias1FirstName = import.InterstateApLocate.Alias1FirstName ?? "";
    var alias1MiddleName = import.InterstateApLocate.Alias1MiddleName ?? "";
    var alias1LastName = import.InterstateApLocate.Alias1LastName ?? "";
    var alias1Suffix = import.InterstateApLocate.Alias1Suffix ?? "";
    var alias2FirstName = import.InterstateApLocate.Alias2FirstName ?? "";
    var alias2MiddleName = import.InterstateApLocate.Alias2MiddleName ?? "";
    var alias2LastName = import.InterstateApLocate.Alias2LastName ?? "";
    var alias2Suffix = import.InterstateApLocate.Alias2Suffix ?? "";
    var alias3FirstName = import.InterstateApLocate.Alias3FirstName ?? "";
    var alias3MiddleName = import.InterstateApLocate.Alias3MiddleName ?? "";
    var alias3LastName = import.InterstateApLocate.Alias3LastName ?? "";
    var alias3Suffix = import.InterstateApLocate.Alias3Suffix ?? "";
    var currentSpouseFirstName =
      import.InterstateApLocate.CurrentSpouseFirstName ?? "";
    var currentSpouseMiddleName =
      import.InterstateApLocate.CurrentSpouseMiddleName ?? "";
    var currentSpouseLastName =
      import.InterstateApLocate.CurrentSpouseLastName ?? "";
    var currentSpouseSuffix = import.InterstateApLocate.CurrentSpouseSuffix ?? ""
      ;
    var occupation = import.InterstateApLocate.Occupation ?? "";
    var employerAddressLine1 =
      import.InterstateApLocate.EmployerAddressLine1 ?? "";
    var employerAddressLine2 =
      import.InterstateApLocate.EmployerAddressLine2 ?? "";
    var employerCity = import.InterstateApLocate.EmployerCity ?? "";
    var employerState = import.InterstateApLocate.EmployerState ?? "";
    var employerZipCode5 = import.InterstateApLocate.EmployerZipCode5 ?? "";
    var employerZipCode4 = import.InterstateApLocate.EmployerZipCode4 ?? "";
    var wageQtr = import.InterstateApLocate.WageQtr.GetValueOrDefault();
    var wageYear = import.InterstateApLocate.WageYear.GetValueOrDefault();
    var wageAmount = import.InterstateApLocate.WageAmount.GetValueOrDefault();
    var insuranceCarrierName =
      import.InterstateApLocate.InsuranceCarrierName ?? "";
    var insurancePolicyNum = import.InterstateApLocate.InsurancePolicyNum ?? "";
    var lastResAddressLine1 = import.InterstateApLocate.LastResAddressLine1 ?? ""
      ;
    var lastResAddressLine2 = import.InterstateApLocate.LastResAddressLine2 ?? ""
      ;
    var lastResCity = import.InterstateApLocate.LastResCity ?? "";
    var lastResState = import.InterstateApLocate.LastResState ?? "";
    var lastResZipCode5 = import.InterstateApLocate.LastResZipCode5 ?? "";
    var lastResZipCode4 = import.InterstateApLocate.LastResZipCode4 ?? "";
    var lastResAddressDate = import.InterstateApLocate.LastResAddressDate;
    var lastMailAddressLine1 =
      import.InterstateApLocate.LastMailAddressLine1 ?? "";
    var lastMailAddressLine2 =
      import.InterstateApLocate.LastMailAddressLine2 ?? "";
    var lastMailCity = import.InterstateApLocate.LastMailCity ?? "";
    var lastMailState = import.InterstateApLocate.LastMailState ?? "";
    var lastMailZipCode5 = import.InterstateApLocate.LastMailZipCode5 ?? "";
    var lastMailZipCode4 = import.InterstateApLocate.LastMailZipCode4 ?? "";
    var lastMailAddressDate = import.InterstateApLocate.LastMailAddressDate;
    var lastEmployerName = import.InterstateApLocate.LastEmployerName ?? "";
    var lastEmployerDate = import.InterstateApLocate.LastEmployerDate;
    var lastEmployerAddressLine1 =
      import.InterstateApLocate.LastEmployerAddressLine1 ?? "";
    var lastEmployerAddressLine2 =
      import.InterstateApLocate.LastEmployerAddressLine2 ?? "";
    var lastEmployerCity = import.InterstateApLocate.LastEmployerCity ?? "";
    var lastEmployerState = import.InterstateApLocate.LastEmployerState ?? "";
    var lastEmployerZipCode5 =
      import.InterstateApLocate.LastEmployerZipCode5 ?? "";
    var lastEmployerZipCode4 =
      import.InterstateApLocate.LastEmployerZipCode4 ?? "";
    var professionalLicenses =
      import.InterstateApLocate.ProfessionalLicenses ?? "";
    var workAreaCode =
      import.InterstateApLocate.WorkAreaCode.GetValueOrDefault();
    var homeAreaCode =
      import.InterstateApLocate.HomeAreaCode.GetValueOrDefault();
    var lastEmployerEndDate = import.InterstateApLocate.LastEmployerEndDate;
    var employerAreaCode =
      import.InterstateApLocate.EmployerAreaCode.GetValueOrDefault();
    var employer2Name = import.InterstateApLocate.Employer2Name ?? "";
    var employer2Ein =
      import.InterstateApLocate.Employer2Ein.GetValueOrDefault();
    var employer2PhoneNumber =
      import.InterstateApLocate.Employer2PhoneNumber ?? "";
    var employer2AreaCode = import.InterstateApLocate.Employer2AreaCode ?? "";
    var employer2AddressLine1 =
      import.InterstateApLocate.Employer2AddressLine1 ?? "";
    var employer2AddressLine2 =
      import.InterstateApLocate.Employer2AddressLine2 ?? "";
    var employer2City = import.InterstateApLocate.Employer2City ?? "";
    var employer2State = import.InterstateApLocate.Employer2State ?? "";
    var employer2ZipCode5 =
      import.InterstateApLocate.Employer2ZipCode5.GetValueOrDefault();
    var employer2ZipCode4 =
      import.InterstateApLocate.Employer2ZipCode4.GetValueOrDefault();
    var employer2ConfirmedIndicator =
      import.InterstateApLocate.Employer2ConfirmedIndicator ?? "";
    var employer2EffectiveDate =
      import.InterstateApLocate.Employer2EffectiveDate;
    var employer2EndDate = import.InterstateApLocate.Employer2EndDate;
    var employer2WageAmount =
      import.InterstateApLocate.Employer2WageAmount.GetValueOrDefault();
    var employer2WageQuarter =
      import.InterstateApLocate.Employer2WageQuarter.GetValueOrDefault();
    var employer2WageYear =
      import.InterstateApLocate.Employer2WageYear.GetValueOrDefault();
    var employer3Name = import.InterstateApLocate.Employer3Name ?? "";
    var employer3Ein =
      import.InterstateApLocate.Employer3Ein.GetValueOrDefault();
    var employer3PhoneNumber =
      import.InterstateApLocate.Employer3PhoneNumber ?? "";
    var employer3AreaCode = import.InterstateApLocate.Employer3AreaCode ?? "";
    var employer3AddressLine1 =
      import.InterstateApLocate.Employer3AddressLine1 ?? "";
    var employer3AddressLine2 =
      import.InterstateApLocate.Employer3AddressLine2 ?? "";
    var employer3City = import.InterstateApLocate.Employer3City ?? "";
    var employer3State = import.InterstateApLocate.Employer3State ?? "";
    var employer3ZipCode5 =
      import.InterstateApLocate.Employer3ZipCode5.GetValueOrDefault();
    var employer3ZipCode4 =
      import.InterstateApLocate.Employer3ZipCode4.GetValueOrDefault();
    var employer3ConfirmedIndicator =
      import.InterstateApLocate.Employer3ConfirmedIndicator ?? "";
    var employer3EffectiveDate =
      import.InterstateApLocate.Employer3EffectiveDate;
    var employer3EndDate = import.InterstateApLocate.Employer3EndDate;
    var employer3WageAmount =
      import.InterstateApLocate.Employer3WageAmount.GetValueOrDefault();
    var employer3WageQuarter =
      import.InterstateApLocate.Employer3WageQuarter.GetValueOrDefault();
    var employer3WageYear =
      import.InterstateApLocate.Employer3WageYear.GetValueOrDefault();

    entities.InterstateApLocate.Populated = false;
    Update("CreateInterstateApLocate",
      (db, command) =>
      {
        db.SetDate(command, "cncTransactionDt", cncTransactionDt);
        db.SetInt64(command, "cncTransSerlNbr", cncTransSerlNbr);
        db.SetNullableInt32(command, "employerEin", employerEin);
        db.SetNullableString(command, "employerName", employerName);
        db.SetNullableInt32(command, "employerPhoneNum", employerPhoneNum);
        db.SetNullableDate(command, "employerEffDate", employerEffectiveDate);
        db.SetNullableDate(command, "employerEndDate", employerEndDate);
        db.SetNullableString(command, "employerCfmdInd", employerConfirmedInd);
        db.SetNullableString(command, "resAddrLine1", residentialAddressLine1);
        db.SetNullableString(command, "resAddrLine2", residentialAddressLine2);
        db.SetNullableString(command, "residentialCity", residentialCity);
        db.SetNullableString(command, "residentialState", residentialState);
        db.SetNullableString(command, "residentialZip5", residentialZipCode5);
        db.SetNullableString(command, "residentialZip4", residentialZipCode4);
        db.SetNullableString(command, "mailingAddrLine1", mailingAddressLine1);
        db.SetNullableString(command, "mailingAddrLine2", mailingAddressLine2);
        db.SetNullableString(command, "mailingCity", mailingCity);
        db.SetNullableString(command, "mailingState", mailingState);
        db.SetNullableString(command, "mailingZip5", mailingZipCode5);
        db.SetNullableString(command, "mailingZip4", mailingZipCode4);
        db.SetNullableDate(
          command, "resAddrEffDate", residentialAddressEffectvDate);
        db.
          SetNullableDate(command, "resAddrEndDate", residentialAddressEndDate);
          
        db.SetNullableString(
          command, "resAddrConInd", residentialAddressConfirmInd);
        db.SetNullableDate(
          command, "mailAddrEffDte", mailingAddressEffectiveDate);
        db.SetNullableDate(command, "mailAddrEndDte", mailingAddressEndDate);
        db.SetNullableString(
          command, "mailAddrConfInd", mailingAddressConfirmedInd);
        db.SetNullableInt32(command, "homePhoneNumber", homePhoneNumber);
        db.SetNullableInt32(command, "workPhoneNumber", workPhoneNumber);
        db.SetNullableString(command, "driversLicState", driversLicState);
        db.SetNullableString(command, "driverLicenseNbr", driversLicenseNum);
        db.SetNullableString(command, "alias1FirstName", alias1FirstName);
        db.SetNullableString(command, "alias1MiddleNam", alias1MiddleName);
        db.SetNullableString(command, "alias1LastName", alias1LastName);
        db.SetNullableString(command, "alias1Suffix", alias1Suffix);
        db.SetNullableString(command, "alias2FirstName", alias2FirstName);
        db.SetNullableString(command, "alias2MiddleNam", alias2MiddleName);
        db.SetNullableString(command, "alias2LastName", alias2LastName);
        db.SetNullableString(command, "alias2Suffix", alias2Suffix);
        db.SetNullableString(command, "alias3FirstName", alias3FirstName);
        db.SetNullableString(command, "alias3MiddleNam", alias3MiddleName);
        db.SetNullableString(command, "alias3LastName", alias3LastName);
        db.SetNullableString(command, "alias3Suffix", alias3Suffix);
        db.
          SetNullableString(command, "currentSpouseFir", currentSpouseFirstName);
          
        db.SetNullableString(
          command, "currentSpouseMid", currentSpouseMiddleName);
        db.
          SetNullableString(command, "currentSpouseLas", currentSpouseLastName);
          
        db.SetNullableString(command, "currentSpouseSuf", currentSpouseSuffix);
        db.SetNullableString(command, "occupation", occupation);
        db.SetNullableString(command, "emplAddrLine1", employerAddressLine1);
        db.SetNullableString(command, "emplAddrLine2", employerAddressLine2);
        db.SetNullableString(command, "employerCity", employerCity);
        db.SetNullableString(command, "employerState", employerState);
        db.SetNullableString(command, "emplZipCode5", employerZipCode5);
        db.SetNullableString(command, "emplZipCode4", employerZipCode4);
        db.SetNullableInt32(command, "wageQtr", wageQtr);
        db.SetNullableInt32(command, "wageYear", wageYear);
        db.SetNullableDecimal(command, "wageAmount", wageAmount);
        db.SetNullableString(command, "insCarrierName", insuranceCarrierName);
        db.SetNullableString(command, "insPolicyNbr", insurancePolicyNum);
        db.SetNullableString(command, "lstResAddrLine1", lastResAddressLine1);
        db.SetNullableString(command, "lstResAddrLine2", lastResAddressLine2);
        db.SetNullableString(command, "lastResCity", lastResCity);
        db.SetNullableString(command, "lastResState", lastResState);
        db.SetNullableString(command, "lstResZipCode5", lastResZipCode5);
        db.SetNullableString(command, "lstResZipCode4", lastResZipCode4);
        db.SetNullableDate(command, "lastResAddrDte", lastResAddressDate);
        db.SetNullableString(command, "lstMailAddrLin1", lastMailAddressLine1);
        db.SetNullableString(command, "lstMailAddrLin2", lastMailAddressLine2);
        db.SetNullableString(command, "lastMailCity", lastMailCity);
        db.SetNullableString(command, "lastMailState", lastMailState);
        db.SetNullableString(command, "lstMailZipCode5", lastMailZipCode5);
        db.SetNullableString(command, "lstMailZipCode4", lastMailZipCode4);
        db.SetNullableDate(command, "lastMailAddrDte", lastMailAddressDate);
        db.SetNullableString(command, "lastEmployerName", lastEmployerName);
        db.SetNullableDate(command, "lastEmployerDate", lastEmployerDate);
        db.SetNullableString(
          command, "lstEmplAddrLin1", lastEmployerAddressLine1);
        db.SetNullableString(
          command, "lstEmplAddrLin2", lastEmployerAddressLine2);
        db.SetNullableString(command, "lastEmployerCity", lastEmployerCity);
        db.SetNullableString(command, "lastEmployerStat", lastEmployerState);
        db.SetNullableString(command, "lstEmplZipCode5", lastEmployerZipCode5);
        db.SetNullableString(command, "lstEmplZipCode4", lastEmployerZipCode4);
        db.SetNullableString(command, "professionalLics", professionalLicenses);
        db.SetNullableInt32(command, "workAreaCode", workAreaCode);
        db.SetNullableInt32(command, "homeAreaCode", homeAreaCode);
        db.SetNullableDate(command, "lastEmpEndDate", lastEmployerEndDate);
        db.SetNullableInt32(command, "employerAreaCode", employerAreaCode);
        db.SetNullableString(command, "employer2Name", employer2Name);
        db.SetNullableInt32(command, "employer2Ein", employer2Ein);
        db.SetNullableString(command, "emp2PhoneNumber", employer2PhoneNumber);
        db.SetNullableString(command, "empl2AreaCode", employer2AreaCode);
        db.SetNullableString(command, "emp2AddrLine1", employer2AddressLine1);
        db.SetNullableString(command, "emp2AddrLine2", employer2AddressLine2);
        db.SetNullableString(command, "employer2City", employer2City);
        db.SetNullableString(command, "employer2State", employer2State);
        db.SetNullableInt32(command, "emp2ZipCode5", employer2ZipCode5);
        db.SetNullableInt32(command, "emp2ZipCode4", employer2ZipCode4);
        db.SetNullableString(
          command, "emp2ConfirmedInd", employer2ConfirmedIndicator);
        db.SetNullableDate(command, "emp2EffectiveDt", employer2EffectiveDate);
        db.SetNullableDate(command, "employer2EndDate", employer2EndDate);
        db.SetNullableInt64(command, "emp2WageAmount", employer2WageAmount);
        db.SetNullableInt32(command, "emp2WageQuarter", employer2WageQuarter);
        db.SetNullableInt32(command, "emp2WageYear", employer2WageYear);
        db.SetNullableString(command, "employer3Name", employer3Name);
        db.SetNullableInt32(command, "employer3Ein", employer3Ein);
        db.SetNullableString(command, "emp3PhoneNumber", employer3PhoneNumber);
        db.SetNullableString(command, "emp3AreaCode", employer3AreaCode);
        db.SetNullableString(command, "emp3AddrLine1", employer3AddressLine1);
        db.SetNullableString(command, "emp3AddrLine2", employer3AddressLine2);
        db.SetNullableString(command, "employer3City", employer3City);
        db.SetNullableString(command, "employer3State", employer3State);
        db.SetNullableInt32(command, "emp3ZipCode5", employer3ZipCode5);
        db.SetNullableInt32(command, "emp3ZipCode4", employer3ZipCode4);
        db.SetNullableString(
          command, "emp3ConfirmedInd", employer3ConfirmedIndicator);
        db.SetNullableDate(command, "emp3EffectiveDt", employer3EffectiveDate);
        db.SetNullableDate(command, "employer3EndDate", employer3EndDate);
        db.SetNullableInt64(command, "emp3WageAmount", employer3WageAmount);
        db.SetNullableInt32(command, "emp3WageQuarter", employer3WageQuarter);
        db.SetNullableInt32(command, "emp3WageYear", employer3WageYear);
      });

    entities.InterstateApLocate.CncTransactionDt = cncTransactionDt;
    entities.InterstateApLocate.CncTransSerlNbr = cncTransSerlNbr;
    entities.InterstateApLocate.EmployerEin = employerEin;
    entities.InterstateApLocate.EmployerName = employerName;
    entities.InterstateApLocate.EmployerPhoneNum = employerPhoneNum;
    entities.InterstateApLocate.EmployerEffectiveDate = employerEffectiveDate;
    entities.InterstateApLocate.EmployerEndDate = employerEndDate;
    entities.InterstateApLocate.EmployerConfirmedInd = employerConfirmedInd;
    entities.InterstateApLocate.ResidentialAddressLine1 =
      residentialAddressLine1;
    entities.InterstateApLocate.ResidentialAddressLine2 =
      residentialAddressLine2;
    entities.InterstateApLocate.ResidentialCity = residentialCity;
    entities.InterstateApLocate.ResidentialState = residentialState;
    entities.InterstateApLocate.ResidentialZipCode5 = residentialZipCode5;
    entities.InterstateApLocate.ResidentialZipCode4 = residentialZipCode4;
    entities.InterstateApLocate.MailingAddressLine1 = mailingAddressLine1;
    entities.InterstateApLocate.MailingAddressLine2 = mailingAddressLine2;
    entities.InterstateApLocate.MailingCity = mailingCity;
    entities.InterstateApLocate.MailingState = mailingState;
    entities.InterstateApLocate.MailingZipCode5 = mailingZipCode5;
    entities.InterstateApLocate.MailingZipCode4 = mailingZipCode4;
    entities.InterstateApLocate.ResidentialAddressEffectvDate =
      residentialAddressEffectvDate;
    entities.InterstateApLocate.ResidentialAddressEndDate =
      residentialAddressEndDate;
    entities.InterstateApLocate.ResidentialAddressConfirmInd =
      residentialAddressConfirmInd;
    entities.InterstateApLocate.MailingAddressEffectiveDate =
      mailingAddressEffectiveDate;
    entities.InterstateApLocate.MailingAddressEndDate = mailingAddressEndDate;
    entities.InterstateApLocate.MailingAddressConfirmedInd =
      mailingAddressConfirmedInd;
    entities.InterstateApLocate.HomePhoneNumber = homePhoneNumber;
    entities.InterstateApLocate.WorkPhoneNumber = workPhoneNumber;
    entities.InterstateApLocate.DriversLicState = driversLicState;
    entities.InterstateApLocate.DriversLicenseNum = driversLicenseNum;
    entities.InterstateApLocate.Alias1FirstName = alias1FirstName;
    entities.InterstateApLocate.Alias1MiddleName = alias1MiddleName;
    entities.InterstateApLocate.Alias1LastName = alias1LastName;
    entities.InterstateApLocate.Alias1Suffix = alias1Suffix;
    entities.InterstateApLocate.Alias2FirstName = alias2FirstName;
    entities.InterstateApLocate.Alias2MiddleName = alias2MiddleName;
    entities.InterstateApLocate.Alias2LastName = alias2LastName;
    entities.InterstateApLocate.Alias2Suffix = alias2Suffix;
    entities.InterstateApLocate.Alias3FirstName = alias3FirstName;
    entities.InterstateApLocate.Alias3MiddleName = alias3MiddleName;
    entities.InterstateApLocate.Alias3LastName = alias3LastName;
    entities.InterstateApLocate.Alias3Suffix = alias3Suffix;
    entities.InterstateApLocate.CurrentSpouseFirstName = currentSpouseFirstName;
    entities.InterstateApLocate.CurrentSpouseMiddleName =
      currentSpouseMiddleName;
    entities.InterstateApLocate.CurrentSpouseLastName = currentSpouseLastName;
    entities.InterstateApLocate.CurrentSpouseSuffix = currentSpouseSuffix;
    entities.InterstateApLocate.Occupation = occupation;
    entities.InterstateApLocate.EmployerAddressLine1 = employerAddressLine1;
    entities.InterstateApLocate.EmployerAddressLine2 = employerAddressLine2;
    entities.InterstateApLocate.EmployerCity = employerCity;
    entities.InterstateApLocate.EmployerState = employerState;
    entities.InterstateApLocate.EmployerZipCode5 = employerZipCode5;
    entities.InterstateApLocate.EmployerZipCode4 = employerZipCode4;
    entities.InterstateApLocate.WageQtr = wageQtr;
    entities.InterstateApLocate.WageYear = wageYear;
    entities.InterstateApLocate.WageAmount = wageAmount;
    entities.InterstateApLocate.InsuranceCarrierName = insuranceCarrierName;
    entities.InterstateApLocate.InsurancePolicyNum = insurancePolicyNum;
    entities.InterstateApLocate.LastResAddressLine1 = lastResAddressLine1;
    entities.InterstateApLocate.LastResAddressLine2 = lastResAddressLine2;
    entities.InterstateApLocate.LastResCity = lastResCity;
    entities.InterstateApLocate.LastResState = lastResState;
    entities.InterstateApLocate.LastResZipCode5 = lastResZipCode5;
    entities.InterstateApLocate.LastResZipCode4 = lastResZipCode4;
    entities.InterstateApLocate.LastResAddressDate = lastResAddressDate;
    entities.InterstateApLocate.LastMailAddressLine1 = lastMailAddressLine1;
    entities.InterstateApLocate.LastMailAddressLine2 = lastMailAddressLine2;
    entities.InterstateApLocate.LastMailCity = lastMailCity;
    entities.InterstateApLocate.LastMailState = lastMailState;
    entities.InterstateApLocate.LastMailZipCode5 = lastMailZipCode5;
    entities.InterstateApLocate.LastMailZipCode4 = lastMailZipCode4;
    entities.InterstateApLocate.LastMailAddressDate = lastMailAddressDate;
    entities.InterstateApLocate.LastEmployerName = lastEmployerName;
    entities.InterstateApLocate.LastEmployerDate = lastEmployerDate;
    entities.InterstateApLocate.LastEmployerAddressLine1 =
      lastEmployerAddressLine1;
    entities.InterstateApLocate.LastEmployerAddressLine2 =
      lastEmployerAddressLine2;
    entities.InterstateApLocate.LastEmployerCity = lastEmployerCity;
    entities.InterstateApLocate.LastEmployerState = lastEmployerState;
    entities.InterstateApLocate.LastEmployerZipCode5 = lastEmployerZipCode5;
    entities.InterstateApLocate.LastEmployerZipCode4 = lastEmployerZipCode4;
    entities.InterstateApLocate.ProfessionalLicenses = professionalLicenses;
    entities.InterstateApLocate.WorkAreaCode = workAreaCode;
    entities.InterstateApLocate.HomeAreaCode = homeAreaCode;
    entities.InterstateApLocate.LastEmployerEndDate = lastEmployerEndDate;
    entities.InterstateApLocate.EmployerAreaCode = employerAreaCode;
    entities.InterstateApLocate.Employer2Name = employer2Name;
    entities.InterstateApLocate.Employer2Ein = employer2Ein;
    entities.InterstateApLocate.Employer2PhoneNumber = employer2PhoneNumber;
    entities.InterstateApLocate.Employer2AreaCode = employer2AreaCode;
    entities.InterstateApLocate.Employer2AddressLine1 = employer2AddressLine1;
    entities.InterstateApLocate.Employer2AddressLine2 = employer2AddressLine2;
    entities.InterstateApLocate.Employer2City = employer2City;
    entities.InterstateApLocate.Employer2State = employer2State;
    entities.InterstateApLocate.Employer2ZipCode5 = employer2ZipCode5;
    entities.InterstateApLocate.Employer2ZipCode4 = employer2ZipCode4;
    entities.InterstateApLocate.Employer2ConfirmedIndicator =
      employer2ConfirmedIndicator;
    entities.InterstateApLocate.Employer2EffectiveDate = employer2EffectiveDate;
    entities.InterstateApLocate.Employer2EndDate = employer2EndDate;
    entities.InterstateApLocate.Employer2WageAmount = employer2WageAmount;
    entities.InterstateApLocate.Employer2WageQuarter = employer2WageQuarter;
    entities.InterstateApLocate.Employer2WageYear = employer2WageYear;
    entities.InterstateApLocate.Employer3Name = employer3Name;
    entities.InterstateApLocate.Employer3Ein = employer3Ein;
    entities.InterstateApLocate.Employer3PhoneNumber = employer3PhoneNumber;
    entities.InterstateApLocate.Employer3AreaCode = employer3AreaCode;
    entities.InterstateApLocate.Employer3AddressLine1 = employer3AddressLine1;
    entities.InterstateApLocate.Employer3AddressLine2 = employer3AddressLine2;
    entities.InterstateApLocate.Employer3City = employer3City;
    entities.InterstateApLocate.Employer3State = employer3State;
    entities.InterstateApLocate.Employer3ZipCode5 = employer3ZipCode5;
    entities.InterstateApLocate.Employer3ZipCode4 = employer3ZipCode4;
    entities.InterstateApLocate.Employer3ConfirmedIndicator =
      employer3ConfirmedIndicator;
    entities.InterstateApLocate.Employer3EffectiveDate = employer3EffectiveDate;
    entities.InterstateApLocate.Employer3EndDate = employer3EndDate;
    entities.InterstateApLocate.Employer3WageAmount = employer3WageAmount;
    entities.InterstateApLocate.Employer3WageQuarter = employer3WageQuarter;
    entities.InterstateApLocate.Employer3WageYear = employer3WageYear;
    entities.InterstateApLocate.Populated = true;
  }

  private void CreateInterstateCase1()
  {
    var localFipsState = import.InterstateCase.LocalFipsState;
    var localFipsCounty =
      import.InterstateCase.LocalFipsCounty.GetValueOrDefault();
    var localFipsLocation =
      import.InterstateCase.LocalFipsLocation.GetValueOrDefault();
    var otherFipsState = import.InterstateCase.OtherFipsState;
    var otherFipsCounty =
      import.InterstateCase.OtherFipsCounty.GetValueOrDefault();
    var otherFipsLocation =
      import.InterstateCase.OtherFipsLocation.GetValueOrDefault();
    var transSerialNumber = import.InterstateCase.TransSerialNumber;
    var actionCode = import.InterstateCase.ActionCode;
    var functionalTypeCode = import.InterstateCase.FunctionalTypeCode;
    var transactionDate = import.InterstateCase.TransactionDate;
    var ksCaseId = import.InterstateCase.KsCaseId ?? "";
    var interstateCaseId = import.InterstateCase.InterstateCaseId ?? "";
    var actionReasonCode = import.InterstateCase.ActionReasonCode ?? "";
    var actionResolutionDate = import.InterstateCase.ActionResolutionDate;
    var attachmentsInd = import.InterstateCase.AttachmentsInd;
    var caseDataInd = import.InterstateCase.CaseDataInd.GetValueOrDefault();
    var apIdentificationInd =
      import.InterstateCase.ApIdentificationInd.GetValueOrDefault();
    var apLocateDataInd =
      import.InterstateCase.ApLocateDataInd.GetValueOrDefault();
    var participantDataInd =
      import.InterstateCase.ParticipantDataInd.GetValueOrDefault();
    var orderDataInd = import.InterstateCase.OrderDataInd.GetValueOrDefault();
    var collectionDataInd =
      import.InterstateCase.CollectionDataInd.GetValueOrDefault();
    var informationInd =
      import.InterstateCase.InformationInd.GetValueOrDefault();
    var sentDate = import.InterstateCase.SentDate;
    var sentTime = import.InterstateCase.SentTime.GetValueOrDefault();
    var dueDate = import.InterstateCase.DueDate;
    var overdueInd = import.InterstateCase.OverdueInd.GetValueOrDefault();
    var dateReceived = import.InterstateCase.DateReceived;
    var timeReceived = import.InterstateCase.TimeReceived.GetValueOrDefault();
    var attachmentsDueDate = import.InterstateCase.AttachmentsDueDate;
    var interstateFormsPrinted =
      import.InterstateCase.InterstateFormsPrinted ?? "";
    var caseType = import.InterstateCase.CaseType;
    var caseStatus = import.InterstateCase.CaseStatus;
    var paymentMailingAddressLine1 =
      import.InterstateCase.PaymentMailingAddressLine1 ?? "";
    var paymentAddressLine2 = import.InterstateCase.PaymentAddressLine2 ?? "";
    var paymentCity = import.InterstateCase.PaymentCity ?? "";
    var paymentState = import.InterstateCase.PaymentState ?? "";
    var paymentZipCode5 = import.InterstateCase.PaymentZipCode5 ?? "";
    var paymentZipCode4 = import.InterstateCase.PaymentZipCode4 ?? "";
    var contactNameLast = import.InterstateCase.ContactNameLast ?? "";
    var contactNameFirst = import.InterstateCase.ContactNameFirst ?? "";
    var contactNameMiddle = import.InterstateCase.ContactNameMiddle ?? "";
    var contactNameSuffix = import.InterstateCase.ContactNameSuffix ?? "";
    var contactAddressLine1 = import.InterstateCase.ContactAddressLine1;
    var contactAddressLine2 = import.InterstateCase.ContactAddressLine2 ?? "";
    var contactCity = import.InterstateCase.ContactCity ?? "";
    var contactState = import.InterstateCase.ContactState ?? "";
    var contactZipCode5 = import.InterstateCase.ContactZipCode5 ?? "";
    var contactZipCode4 = import.InterstateCase.ContactZipCode4 ?? "";
    var contactPhoneNum =
      import.InterstateCase.ContactPhoneNum.GetValueOrDefault();
    var assnDeactDt = import.InterstateCase.AssnDeactDt;
    var assnDeactInd = import.InterstateCase.AssnDeactInd ?? "";
    var lastDeferDt = import.InterstateCase.LastDeferDt;
    var memo = import.InterstateCase.Memo ?? "";
    var contactPhoneExtension = import.InterstateCase.ContactPhoneExtension ?? ""
      ;
    var contactFaxNumber =
      import.InterstateCase.ContactFaxNumber.GetValueOrDefault();
    var contactFaxAreaCode =
      import.InterstateCase.ContactFaxAreaCode.GetValueOrDefault();
    var contactInternetAddress =
      import.InterstateCase.ContactInternetAddress ?? "";
    var initiatingDocketNumber =
      import.InterstateCase.InitiatingDocketNumber ?? "";
    var sendPaymentsBankAccount =
      import.InterstateCase.SendPaymentsBankAccount ?? "";
    var sendPaymentsRoutingCode =
      import.InterstateCase.SendPaymentsRoutingCode.GetValueOrDefault();
    var nondisclosureFinding = import.InterstateCase.NondisclosureFinding ?? "";
    var respondingDocketNumber =
      import.InterstateCase.RespondingDocketNumber ?? "";
    var stateWithCej = import.InterstateCase.StateWithCej ?? "";
    var paymentFipsCounty = import.InterstateCase.PaymentFipsCounty ?? "";
    var paymentFipsState = import.InterstateCase.PaymentFipsState ?? "";
    var paymentFipsLocation = import.InterstateCase.PaymentFipsLocation ?? "";
    var contactAreaCode =
      import.InterstateCase.ContactAreaCode.GetValueOrDefault();

    entities.InterstateCase.Populated = false;
    Update("CreateInterstateCase1",
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

  private void CreateInterstateCase2()
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
    Update("CreateInterstateCase2",
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

  private void CreateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = import.Current.Date;
    var discontinueDate = import.Max.Date;
    var createdBy = import.ProgramProcessingInfo.Name;
    var createdTimestamp = import.Current.Timestamp;
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var icsDate = entities.InterstateCase.TransactionDate;
    var icsNo = entities.InterstateCase.TransSerialNumber;

    entities.InterstateCaseAssignment.Populated = false;
    Update("CreateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetDate(command, "icsDate", icsDate);
        db.SetInt64(command, "icsNo", icsNo);
      });

    entities.InterstateCaseAssignment.ReasonCode = reasonCode;
    entities.InterstateCaseAssignment.OverrideInd = overrideInd;
    entities.InterstateCaseAssignment.EffectiveDate = effectiveDate;
    entities.InterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.InterstateCaseAssignment.CreatedBy = createdBy;
    entities.InterstateCaseAssignment.CreatedTimestamp = createdTimestamp;
    entities.InterstateCaseAssignment.LastUpdatedBy = createdBy;
    entities.InterstateCaseAssignment.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstateCaseAssignment.SpdId = spdId;
    entities.InterstateCaseAssignment.OffId = offId;
    entities.InterstateCaseAssignment.OspCode = ospCode;
    entities.InterstateCaseAssignment.OspDate = ospDate;
    entities.InterstateCaseAssignment.IcsDate = icsDate;
    entities.InterstateCaseAssignment.IcsNo = icsNo;
    entities.InterstateCaseAssignment.Populated = true;
  }

  private void CreateInterstateCollection()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var systemGeneratedSequenceNum =
      local.InterstateCollection.SystemGeneratedSequenceNum;
    var dateOfCollection =
      import.Collection.Item.InterstateCollection.DateOfCollection;
    var dateOfPosting = local.Null1.Date;
    var paymentAmount =
      import.Collection.Item.InterstateCollection.PaymentAmount.
        GetValueOrDefault();
    var paymentSource =
      import.Collection.Item.InterstateCollection.PaymentSource ?? "";
    var interstatePaymentMethod =
      import.Collection.Item.InterstateCollection.InterstatePaymentMethod ?? ""
      ;
    var rdfiId = import.Collection.Item.InterstateCollection.RdfiId ?? "";
    var rdfiAccountNum =
      import.Collection.Item.InterstateCollection.RdfiAccountNum ?? "";

    entities.InterstateCollection.Populated = false;
    Update("CreateInterstateCollection",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetInt32(command, "sysGeneratedId", systemGeneratedSequenceNum);
        db.SetNullableDate(command, "dateOfCollection", dateOfCollection);
        db.SetNullableDate(command, "dateOfPosting", dateOfPosting);
        db.SetNullableDecimal(command, "paymentAmount", paymentAmount);
        db.SetNullableString(command, "paymentSource", paymentSource);
        db.SetNullableString(
          command, "interstatePayment", interstatePaymentMethod);
        db.SetNullableString(command, "rdfiId", rdfiId);
        db.SetNullableString(command, "rdfiAccountNum", rdfiAccountNum);
      });

    entities.InterstateCollection.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateCollection.CcaTransSerNum = ccaTransSerNum;
    entities.InterstateCollection.SystemGeneratedSequenceNum =
      systemGeneratedSequenceNum;
    entities.InterstateCollection.DateOfCollection = dateOfCollection;
    entities.InterstateCollection.DateOfPosting = dateOfPosting;
    entities.InterstateCollection.PaymentAmount = paymentAmount;
    entities.InterstateCollection.PaymentSource = paymentSource;
    entities.InterstateCollection.InterstatePaymentMethod =
      interstatePaymentMethod;
    entities.InterstateCollection.RdfiId = rdfiId;
    entities.InterstateCollection.RdfiAccountNum = rdfiAccountNum;
    entities.InterstateCollection.Populated = true;
  }

  private void CreateInterstateMiscellaneous1()
  {
    var statusChangeCode = local.InterstateCase.CaseStatus;
    var informationTextLine1 =
      local.InterstateMiscellaneous.InformationTextLine1 ?? "";
    var informationTextLine2 =
      local.InterstateMiscellaneous.InformationTextLine2 ?? "";
    var informationTextLine3 =
      local.InterstateMiscellaneous.InformationTextLine3 ?? "";
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var informationTextLine4 =
      local.InterstateMiscellaneous.InformationTextLine4 ?? "";

    entities.InterstateMiscellaneous.Populated = false;
    Update("CreateInterstateMiscellaneous1",
      (db, command) =>
      {
        db.SetString(command, "statusChangeCode", statusChangeCode);
        db.SetNullableString(command, "newCaseId", "");
        db.SetNullableString(command, "infoText1", informationTextLine1);
        db.SetNullableString(command, "infoText2", informationTextLine2);
        db.SetNullableString(command, "infoText3", informationTextLine3);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetNullableString(command, "infoTextLine4", informationTextLine4);
        db.SetNullableString(command, "infoTextLine5", "");
      });

    entities.InterstateMiscellaneous.StatusChangeCode = statusChangeCode;
    entities.InterstateMiscellaneous.NewCaseId = "";
    entities.InterstateMiscellaneous.InformationTextLine1 =
      informationTextLine1;
    entities.InterstateMiscellaneous.InformationTextLine2 =
      informationTextLine2;
    entities.InterstateMiscellaneous.InformationTextLine3 =
      informationTextLine3;
    entities.InterstateMiscellaneous.CcaTransSerNum = ccaTransSerNum;
    entities.InterstateMiscellaneous.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateMiscellaneous.InformationTextLine4 =
      informationTextLine4;
    entities.InterstateMiscellaneous.InformationTextLine5 = "";
    entities.InterstateMiscellaneous.Populated = true;
  }

  private void CreateInterstateMiscellaneous2()
  {
    var statusChangeCode = import.InterstateMiscellaneous.StatusChangeCode;
    var newCaseId = import.InterstateMiscellaneous.NewCaseId ?? "";
    var informationTextLine1 =
      import.InterstateMiscellaneous.InformationTextLine1 ?? "";
    var informationTextLine2 =
      import.InterstateMiscellaneous.InformationTextLine2 ?? "";
    var informationTextLine3 =
      import.InterstateMiscellaneous.InformationTextLine3 ?? "";
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var informationTextLine4 =
      import.InterstateMiscellaneous.InformationTextLine4 ?? "";
    var informationTextLine5 =
      import.InterstateMiscellaneous.InformationTextLine5 ?? "";

    entities.InterstateMiscellaneous.Populated = false;
    Update("CreateInterstateMiscellaneous2",
      (db, command) =>
      {
        db.SetString(command, "statusChangeCode", statusChangeCode);
        db.SetNullableString(command, "newCaseId", newCaseId);
        db.SetNullableString(command, "infoText1", informationTextLine1);
        db.SetNullableString(command, "infoText2", informationTextLine2);
        db.SetNullableString(command, "infoText3", informationTextLine3);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetNullableString(command, "infoTextLine4", informationTextLine4);
        db.SetNullableString(command, "infoTextLine5", informationTextLine5);
      });

    entities.InterstateMiscellaneous.StatusChangeCode = statusChangeCode;
    entities.InterstateMiscellaneous.NewCaseId = newCaseId;
    entities.InterstateMiscellaneous.InformationTextLine1 =
      informationTextLine1;
    entities.InterstateMiscellaneous.InformationTextLine2 =
      informationTextLine2;
    entities.InterstateMiscellaneous.InformationTextLine3 =
      informationTextLine3;
    entities.InterstateMiscellaneous.CcaTransSerNum = ccaTransSerNum;
    entities.InterstateMiscellaneous.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateMiscellaneous.InformationTextLine4 =
      informationTextLine4;
    entities.InterstateMiscellaneous.InformationTextLine5 =
      informationTextLine5;
    entities.InterstateMiscellaneous.Populated = true;
  }

  private void CreateInterstateParticipant()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var systemGeneratedSequenceNum =
      local.InterstateParticipant.SystemGeneratedSequenceNum;
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var nameLast = import.Participant.Item.InterstateParticipant.NameLast ?? "";
    var nameFirst = import.Participant.Item.InterstateParticipant.NameFirst ?? ""
      ;
    var nameMiddle =
      import.Participant.Item.InterstateParticipant.NameMiddle ?? "";
    var nameSuffix =
      import.Participant.Item.InterstateParticipant.NameSuffix ?? "";
    var dateOfBirth = import.Participant.Item.InterstateParticipant.DateOfBirth;
    var ssn = import.Participant.Item.InterstateParticipant.Ssn ?? "";
    var sex = import.Participant.Item.InterstateParticipant.Sex ?? "";
    var race = import.Participant.Item.InterstateParticipant.Race ?? "";
    var relationship =
      import.Participant.Item.InterstateParticipant.Relationship ?? "";
    var status = import.Participant.Item.InterstateParticipant.Status ?? "";
    var dependentRelationCp =
      import.Participant.Item.InterstateParticipant.DependentRelationCp ?? "";
    var addressLine1 =
      import.Participant.Item.InterstateParticipant.AddressLine1 ?? "";
    var addressLine2 =
      import.Participant.Item.InterstateParticipant.AddressLine2 ?? "";
    var city = import.Participant.Item.InterstateParticipant.City ?? "";
    var state = import.Participant.Item.InterstateParticipant.State ?? "";
    var zipCode5 = import.Participant.Item.InterstateParticipant.ZipCode5 ?? "";
    var zipCode4 = import.Participant.Item.InterstateParticipant.ZipCode4 ?? "";
    var employerAddressLine1 =
      import.Participant.Item.InterstateParticipant.EmployerAddressLine1 ?? "";
    var employerAddressLine2 =
      import.Participant.Item.InterstateParticipant.EmployerAddressLine2 ?? "";
    var employerCity =
      import.Participant.Item.InterstateParticipant.EmployerCity ?? "";
    var employerState =
      import.Participant.Item.InterstateParticipant.EmployerState ?? "";
    var employerZipCode5 =
      import.Participant.Item.InterstateParticipant.EmployerZipCode5 ?? "";
    var employerZipCode4 =
      import.Participant.Item.InterstateParticipant.EmployerZipCode4 ?? "";
    var employerName =
      import.Participant.Item.InterstateParticipant.EmployerName ?? "";
    var employerEin =
      import.Participant.Item.InterstateParticipant.EmployerEin.
        GetValueOrDefault();
    var addressVerifiedDate =
      import.Participant.Item.InterstateParticipant.AddressVerifiedDate;
    var employerVerifiedDate =
      import.Participant.Item.InterstateParticipant.EmployerVerifiedDate;
    var workPhone = import.Participant.Item.InterstateParticipant.WorkPhone ?? ""
      ;
    var workAreaCode =
      import.Participant.Item.InterstateParticipant.WorkAreaCode ?? "";
    var placeOfBirth =
      import.Participant.Item.InterstateParticipant.PlaceOfBirth ?? "";
    var childStateOfResidence =
      import.Participant.Item.InterstateParticipant.ChildStateOfResidence ?? ""
      ;
    var childPaternityStatus =
      import.Participant.Item.InterstateParticipant.ChildPaternityStatus ?? "";
    var employerConfirmedInd =
      import.Participant.Item.InterstateParticipant.EmployerConfirmedInd ?? "";
    var addressConfirmedInd =
      import.Participant.Item.InterstateParticipant.AddressConfirmedInd ?? "";

    entities.InterstateParticipant.Populated = false;
    Update("CreateInterstateParticipant",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt32(command, "sysGeneratedId", systemGeneratedSequenceNum);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetNullableString(command, "nameLast", nameLast);
        db.SetNullableString(command, "nameFirst", nameFirst);
        db.SetNullableString(command, "nameMiddle", nameMiddle);
        db.SetNullableString(command, "nameSuffix", nameSuffix);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "sex", sex);
        db.SetNullableString(command, "race", race);
        db.SetNullableString(command, "relationship", relationship);
        db.SetNullableString(command, "status", status);
        db.SetNullableString(command, "dependentRelation", dependentRelationCp);
        db.SetNullableString(command, "addressLine1", addressLine1);
        db.SetNullableString(command, "addressLine2", addressLine2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "empAddressLine1", employerAddressLine1);
        db.SetNullableString(command, "empAddressLine2", employerAddressLine2);
        db.SetNullableString(command, "employerCity", employerCity);
        db.SetNullableString(command, "employerState", employerState);
        db.SetNullableString(command, "empZipCode5", employerZipCode5);
        db.SetNullableString(command, "empZipCode4", employerZipCode4);
        db.SetNullableString(command, "employerName", employerName);
        db.SetNullableInt32(command, "employerEin", employerEin);
        db.SetNullableDate(command, "addrVerifiedDate", addressVerifiedDate);
        db.SetNullableDate(command, "empVerifiedDate", employerVerifiedDate);
        db.SetNullableString(command, "workPhone", workPhone);
        db.SetNullableString(command, "workAreaCode", workAreaCode);
        db.SetNullableString(command, "placeOfBirth", placeOfBirth);
        db.SetNullableString(command, "childStateOfRes", childStateOfResidence);
        db.SetNullableString(command, "childPaterStatus", childPaternityStatus);
        db.SetNullableString(command, "empConfirmedInd", employerConfirmedInd);
        db.SetNullableString(command, "addrConfirmedInd", addressConfirmedInd);
      });

    entities.InterstateParticipant.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateParticipant.SystemGeneratedSequenceNum =
      systemGeneratedSequenceNum;
    entities.InterstateParticipant.CcaTransSerNum = ccaTransSerNum;
    entities.InterstateParticipant.NameLast = nameLast;
    entities.InterstateParticipant.NameFirst = nameFirst;
    entities.InterstateParticipant.NameMiddle = nameMiddle;
    entities.InterstateParticipant.NameSuffix = nameSuffix;
    entities.InterstateParticipant.DateOfBirth = dateOfBirth;
    entities.InterstateParticipant.Ssn = ssn;
    entities.InterstateParticipant.Sex = sex;
    entities.InterstateParticipant.Race = race;
    entities.InterstateParticipant.Relationship = relationship;
    entities.InterstateParticipant.Status = status;
    entities.InterstateParticipant.DependentRelationCp = dependentRelationCp;
    entities.InterstateParticipant.AddressLine1 = addressLine1;
    entities.InterstateParticipant.AddressLine2 = addressLine2;
    entities.InterstateParticipant.City = city;
    entities.InterstateParticipant.State = state;
    entities.InterstateParticipant.ZipCode5 = zipCode5;
    entities.InterstateParticipant.ZipCode4 = zipCode4;
    entities.InterstateParticipant.EmployerAddressLine1 = employerAddressLine1;
    entities.InterstateParticipant.EmployerAddressLine2 = employerAddressLine2;
    entities.InterstateParticipant.EmployerCity = employerCity;
    entities.InterstateParticipant.EmployerState = employerState;
    entities.InterstateParticipant.EmployerZipCode5 = employerZipCode5;
    entities.InterstateParticipant.EmployerZipCode4 = employerZipCode4;
    entities.InterstateParticipant.EmployerName = employerName;
    entities.InterstateParticipant.EmployerEin = employerEin;
    entities.InterstateParticipant.AddressVerifiedDate = addressVerifiedDate;
    entities.InterstateParticipant.EmployerVerifiedDate = employerVerifiedDate;
    entities.InterstateParticipant.WorkPhone = workPhone;
    entities.InterstateParticipant.WorkAreaCode = workAreaCode;
    entities.InterstateParticipant.PlaceOfBirth = placeOfBirth;
    entities.InterstateParticipant.ChildStateOfResidence =
      childStateOfResidence;
    entities.InterstateParticipant.ChildPaternityStatus = childPaternityStatus;
    entities.InterstateParticipant.EmployerConfirmedInd = employerConfirmedInd;
    entities.InterstateParticipant.AddressConfirmedInd = addressConfirmedInd;
    entities.InterstateParticipant.Populated = true;
  }

  private void CreateInterstateRequestHistory()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var createdTimestamp = Now();
    var createdBy = import.ProgramProcessingInfo.Name;
    var transactionDirectionInd = "I";
    var transactionSerialNum = import.InterstateCase.TransSerialNumber;
    var actionCode = import.InterstateCase.ActionCode;
    var functionalTypeCode = import.InterstateCase.FunctionalTypeCode;
    var transactionDate = import.InterstateCase.TransactionDate;
    var actionReasonCode = import.InterstateCase.ActionReasonCode ?? "";
    var actionResolutionDate = import.InterstateCase.ActionResolutionDate;
    var attachmentIndicator = import.InterstateCase.AttachmentsInd;
    var note = import.InterstateCase.Memo ?? "";

    entities.InterstateRequestHistory.Populated = false;
    Update("CreateInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDateTime(command, "createdTstamp", createdTimestamp);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetString(command, "transactionDirect", transactionDirectionInd);
        db.SetInt64(command, "transactionSerial", transactionSerialNum);
        db.SetString(command, "actionCode", actionCode);
        db.SetString(command, "functionalTypeCo", functionalTypeCode);
        db.SetDate(command, "transactionDate", transactionDate);
        db.SetNullableString(command, "actionReasonCode", actionReasonCode);
        db.SetNullableDate(command, "actionResDte", actionResolutionDate);
        db.SetNullableString(command, "attachmentIndicat", attachmentIndicator);
        db.SetNullableString(command, "note", note);
      });

    entities.InterstateRequestHistory.IntGeneratedId = intGeneratedId;
    entities.InterstateRequestHistory.CreatedTimestamp = createdTimestamp;
    entities.InterstateRequestHistory.CreatedBy = createdBy;
    entities.InterstateRequestHistory.TransactionDirectionInd =
      transactionDirectionInd;
    entities.InterstateRequestHistory.TransactionSerialNum =
      transactionSerialNum;
    entities.InterstateRequestHistory.ActionCode = actionCode;
    entities.InterstateRequestHistory.FunctionalTypeCode = functionalTypeCode;
    entities.InterstateRequestHistory.TransactionDate = transactionDate;
    entities.InterstateRequestHistory.ActionReasonCode = actionReasonCode;
    entities.InterstateRequestHistory.ActionResolutionDate =
      actionResolutionDate;
    entities.InterstateRequestHistory.AttachmentIndicator = attachmentIndicator;
    entities.InterstateRequestHistory.Note = note;
    entities.InterstateRequestHistory.Populated = true;
  }

  private void CreateInterstateSupportOrder()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var systemGeneratedSequenceNum =
      local.InterstateSupportOrder.SystemGeneratedSequenceNum;
    var ccaTranSerNum = entities.InterstateCase.TransSerialNumber;
    var fipsState = import.Order.Item.InterstateSupportOrder.FipsState;
    var fipsCounty = import.Order.Item.InterstateSupportOrder.FipsCounty ?? "";
    var fipsLocation =
      import.Order.Item.InterstateSupportOrder.FipsLocation ?? "";
    var number = import.Order.Item.InterstateSupportOrder.Number;
    var orderFilingDate =
      import.Order.Item.InterstateSupportOrder.OrderFilingDate;
    var type1 = import.Order.Item.InterstateSupportOrder.Type1 ?? "";
    var debtType = import.Order.Item.InterstateSupportOrder.DebtType;
    var paymentFreq = import.Order.Item.InterstateSupportOrder.PaymentFreq ?? ""
      ;
    var amountOrdered =
      import.Order.Item.InterstateSupportOrder.AmountOrdered.
        GetValueOrDefault();
    var effectiveDate = import.Order.Item.InterstateSupportOrder.EffectiveDate;
    var endDate = import.Order.Item.InterstateSupportOrder.EndDate;
    var cancelDate = import.Order.Item.InterstateSupportOrder.CancelDate;
    var arrearsFreq = import.Order.Item.InterstateSupportOrder.ArrearsFreq ?? ""
      ;
    var arrearsFreqAmount =
      import.Order.Item.InterstateSupportOrder.ArrearsFreqAmount.
        GetValueOrDefault();
    var arrearsTotalAmount =
      import.Order.Item.InterstateSupportOrder.ArrearsTotalAmount.
        GetValueOrDefault();
    var arrearsAfdcFromDate =
      import.Order.Item.InterstateSupportOrder.ArrearsAfdcFromDate;
    var arrearsAfdcThruDate =
      import.Order.Item.InterstateSupportOrder.ArrearsAfdcThruDate;
    var arrearsAfdcAmount =
      import.Order.Item.InterstateSupportOrder.ArrearsAfdcAmount.
        GetValueOrDefault();
    var arrearsNonAfdcFromDate =
      import.Order.Item.InterstateSupportOrder.ArrearsNonAfdcFromDate;
    var arrearsNonAfdcThruDate =
      import.Order.Item.InterstateSupportOrder.ArrearsNonAfdcThruDate;
    var arrearsNonAfdcAmount =
      import.Order.Item.InterstateSupportOrder.ArrearsNonAfdcAmount.
        GetValueOrDefault();
    var fosterCareFromDate =
      import.Order.Item.InterstateSupportOrder.FosterCareFromDate;
    var fosterCareThruDate =
      import.Order.Item.InterstateSupportOrder.FosterCareThruDate;
    var fosterCareAmount =
      import.Order.Item.InterstateSupportOrder.FosterCareAmount.
        GetValueOrDefault();
    var medicalFromDate =
      import.Order.Item.InterstateSupportOrder.MedicalFromDate;
    var medicalThruDate =
      import.Order.Item.InterstateSupportOrder.MedicalThruDate;
    var medicalAmount =
      import.Order.Item.InterstateSupportOrder.MedicalAmount.
        GetValueOrDefault();
    var medicalOrderedInd =
      import.Order.Item.InterstateSupportOrder.MedicalOrderedInd ?? "";
    var tribunalCaseNumber =
      import.Order.Item.InterstateSupportOrder.TribunalCaseNumber ?? "";
    var dateOfLastPayment =
      import.Order.Item.InterstateSupportOrder.DateOfLastPayment;
    var controllingOrderFlag =
      import.Order.Item.InterstateSupportOrder.ControllingOrderFlag ?? "";
    var newOrderFlag =
      import.Order.Item.InterstateSupportOrder.NewOrderFlag ?? "";
    var docketNumber =
      import.Order.Item.InterstateSupportOrder.DocketNumber ?? "";

    entities.InterstateSupportOrder.Populated = false;
    Update("CreateInterstateSupportOrder",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt32(command, "sysGeneratedId", systemGeneratedSequenceNum);
        db.SetInt64(command, "ccaTranSerNum", ccaTranSerNum);
        db.SetString(command, "fipsState", fipsState);
        db.SetNullableString(command, "fipsCounty", fipsCounty);
        db.SetNullableString(command, "fipsLocation", fipsLocation);
        db.SetString(command, "number", number);
        db.SetDate(command, "orderFilingDate", orderFilingDate);
        db.SetNullableString(command, "type", type1);
        db.SetString(command, "debtType", debtType);
        db.SetNullableString(command, "paymentFreq", paymentFreq);
        db.SetNullableDecimal(command, "amountOrdered", amountOrdered);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableDate(command, "cancelDate", cancelDate);
        db.SetNullableString(command, "arrearsFreq", arrearsFreq);
        db.SetNullableDecimal(command, "arrearsFrqAmt", arrearsFreqAmount);
        db.SetNullableDecimal(command, "arrearsTotalAmt", arrearsTotalAmount);
        db.SetNullableDate(command, "arrsAfdcFromDte", arrearsAfdcFromDate);
        db.SetNullableDate(command, "arrsAfdcThruDte", arrearsAfdcThruDate);
        db.SetNullableDecimal(command, "arrearsAfdcAmt", arrearsAfdcAmount);
        db.SetNullableDate(command, "arrNafdcFromDte", arrearsNonAfdcFromDate);
        db.SetNullableDate(command, "arrNafdcThruDte", arrearsNonAfdcThruDate);
        db.SetNullableDecimal(command, "arrNafdcAmt", arrearsNonAfdcAmount);
        db.SetNullableDate(command, "fostCareFromDte", fosterCareFromDate);
        db.SetNullableDate(command, "fostCareThruDte", fosterCareThruDate);
        db.SetNullableDecimal(command, "fosterCareAmount", fosterCareAmount);
        db.SetNullableDate(command, "medicalFromDate", medicalFromDate);
        db.SetNullableDate(command, "medicalThruDate", medicalThruDate);
        db.SetNullableDecimal(command, "medicalAmount", medicalAmount);
        db.SetNullableString(command, "medicalOrderedIn", medicalOrderedInd);
        db.SetNullableString(command, "tribunalCaseNum", tribunalCaseNumber);
        db.SetNullableDate(command, "dateOfLastPay", dateOfLastPayment);
        db.SetNullableString(command, "cntrlOrderFlag", controllingOrderFlag);
        db.SetNullableString(command, "newOrderFlag", newOrderFlag);
        db.SetNullableString(command, "docketNumber", docketNumber);
        db.SetNullableInt32(command, "legalActionId", 0);
      });

    entities.InterstateSupportOrder.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateSupportOrder.SystemGeneratedSequenceNum =
      systemGeneratedSequenceNum;
    entities.InterstateSupportOrder.CcaTranSerNum = ccaTranSerNum;
    entities.InterstateSupportOrder.FipsState = fipsState;
    entities.InterstateSupportOrder.FipsCounty = fipsCounty;
    entities.InterstateSupportOrder.FipsLocation = fipsLocation;
    entities.InterstateSupportOrder.Number = number;
    entities.InterstateSupportOrder.OrderFilingDate = orderFilingDate;
    entities.InterstateSupportOrder.Type1 = type1;
    entities.InterstateSupportOrder.DebtType = debtType;
    entities.InterstateSupportOrder.PaymentFreq = paymentFreq;
    entities.InterstateSupportOrder.AmountOrdered = amountOrdered;
    entities.InterstateSupportOrder.EffectiveDate = effectiveDate;
    entities.InterstateSupportOrder.EndDate = endDate;
    entities.InterstateSupportOrder.CancelDate = cancelDate;
    entities.InterstateSupportOrder.ArrearsFreq = arrearsFreq;
    entities.InterstateSupportOrder.ArrearsFreqAmount = arrearsFreqAmount;
    entities.InterstateSupportOrder.ArrearsTotalAmount = arrearsTotalAmount;
    entities.InterstateSupportOrder.ArrearsAfdcFromDate = arrearsAfdcFromDate;
    entities.InterstateSupportOrder.ArrearsAfdcThruDate = arrearsAfdcThruDate;
    entities.InterstateSupportOrder.ArrearsAfdcAmount = arrearsAfdcAmount;
    entities.InterstateSupportOrder.ArrearsNonAfdcFromDate =
      arrearsNonAfdcFromDate;
    entities.InterstateSupportOrder.ArrearsNonAfdcThruDate =
      arrearsNonAfdcThruDate;
    entities.InterstateSupportOrder.ArrearsNonAfdcAmount = arrearsNonAfdcAmount;
    entities.InterstateSupportOrder.FosterCareFromDate = fosterCareFromDate;
    entities.InterstateSupportOrder.FosterCareThruDate = fosterCareThruDate;
    entities.InterstateSupportOrder.FosterCareAmount = fosterCareAmount;
    entities.InterstateSupportOrder.MedicalFromDate = medicalFromDate;
    entities.InterstateSupportOrder.MedicalThruDate = medicalThruDate;
    entities.InterstateSupportOrder.MedicalAmount = medicalAmount;
    entities.InterstateSupportOrder.MedicalOrderedInd = medicalOrderedInd;
    entities.InterstateSupportOrder.TribunalCaseNumber = tribunalCaseNumber;
    entities.InterstateSupportOrder.DateOfLastPayment = dateOfLastPayment;
    entities.InterstateSupportOrder.ControllingOrderFlag = controllingOrderFlag;
    entities.InterstateSupportOrder.NewOrderFlag = newOrderFlag;
    entities.InterstateSupportOrder.DocketNumber = docketNumber;
    entities.InterstateSupportOrder.Populated = true;
  }

  private bool ReadAbsentParent()
  {
    return Read("ReadAbsentParent",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        local.CountAbsParentsPerCase.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadAbsentParentCsePerson1()
  {
    entities.AbsentParent.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadAbsentParentCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 7);
        entities.AbsentParent.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadAbsentParentCsePerson2()
  {
    entities.AbsentParent.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadAbsentParentCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 7);
        entities.AbsentParent.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Case1.Number);
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
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
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
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
        db.SetInt32(
          command, "othrStateFipsCd", import.InterstateCase.OtherFipsState);
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
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeCaseloadAssignment1()
  {
    entities.OfficeCaseloadAssignment.Populated = false;

    return Read("ReadOfficeCaseloadAssignment1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 8);
        entities.OfficeCaseloadAssignment.EndingFirstInitial =
          db.GetNullableString(reader, 9);
        entities.OfficeCaseloadAssignment.BeginningFirstIntial =
          db.GetNullableString(reader, 10);
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 11);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 12);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 13);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 14);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 15);
        entities.OfficeCaseloadAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignment2()
  {
    entities.OfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignment2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "text6", local.ApLastNameAlpha.Text6);
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 8);
        entities.OfficeCaseloadAssignment.EndingFirstInitial =
          db.GetNullableString(reader, 9);
        entities.OfficeCaseloadAssignment.BeginningFirstIntial =
          db.GetNullableString(reader, 10);
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 11);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 12);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 13);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 14);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 15);
        entities.OfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "ofceCsldAssgnId",
          local.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
        db.SetDate(
          command, "effectiveDate2", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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
    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of InterstateParticipant.
      /// </summary>
      [JsonPropertyName("interstateParticipant")]
      public InterstateParticipant InterstateParticipant
      {
        get => interstateParticipant ??= new();
        set => interstateParticipant = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateParticipant interstateParticipant;
    }

    /// <summary>A OrderGroup group.</summary>
    [Serializable]
    public class OrderGroup
    {
      /// <summary>
      /// A value of InterstateSupportOrder.
      /// </summary>
      [JsonPropertyName("interstateSupportOrder")]
      public InterstateSupportOrder InterstateSupportOrder
      {
        get => interstateSupportOrder ??= new();
        set => interstateSupportOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder interstateSupportOrder;
    }

    /// <summary>A CollectionGroup group.</summary>
    [Serializable]
    public class CollectionGroup
    {
      /// <summary>
      /// A value of InterstateCollection.
      /// </summary>
      [JsonPropertyName("interstateCollection")]
      public InterstateCollection InterstateCollection
      {
        get => interstateCollection ??= new();
        set => interstateCollection = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateCollection interstateCollection;
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
    /// A value of ExportPayAddrUpdated.
    /// </summary>
    [JsonPropertyName("exportPayAddrUpdated")]
    public Common ExportPayAddrUpdated
    {
      get => exportPayAddrUpdated ??= new();
      set => exportPayAddrUpdated = value;
    }

    /// <summary>
    /// A value of ExportPayAddrCreated.
    /// </summary>
    [JsonPropertyName("exportPayAddrCreated")]
    public Common ExportPayAddrCreated
    {
      get => exportPayAddrCreated ??= new();
      set => exportPayAddrCreated = value;
    }

    /// <summary>
    /// A value of ExportContAddrUpdated.
    /// </summary>
    [JsonPropertyName("exportContAddrUpdated")]
    public Common ExportContAddrUpdated
    {
      get => exportContAddrUpdated ??= new();
      set => exportContAddrUpdated = value;
    }

    /// <summary>
    /// A value of ExportContAddrCreated.
    /// </summary>
    [JsonPropertyName("exportContAddrCreated")]
    public Common ExportContAddrCreated
    {
      get => exportContAddrCreated ??= new();
      set => exportContAddrCreated = value;
    }

    /// <summary>
    /// A value of ExportIntContactUpdate.
    /// </summary>
    [JsonPropertyName("exportIntContactUpdate")]
    public Common ExportIntContactUpdate
    {
      get => exportIntContactUpdate ??= new();
      set => exportIntContactUpdate = value;
    }

    /// <summary>
    /// A value of ExportIntContactCreate.
    /// </summary>
    [JsonPropertyName("exportIntContactCreate")]
    public Common ExportIntContactCreate
    {
      get => exportIntContactCreate ??= new();
      set => exportIntContactCreate = value;
    }

    /// <summary>
    /// A value of ExportUpdates.
    /// </summary>
    [JsonPropertyName("exportUpdates")]
    public Common ExportUpdates
    {
      get => exportUpdates ??= new();
      set => exportUpdates = value;
    }

    /// <summary>
    /// A value of ExportCaseAssigns.
    /// </summary>
    [JsonPropertyName("exportCaseAssigns")]
    public Common ExportCaseAssigns
    {
      get => exportCaseAssigns ??= new();
      set => exportCaseAssigns = value;
    }

    /// <summary>
    /// A value of ExportMiscCreated.
    /// </summary>
    [JsonPropertyName("exportMiscCreated")]
    public Common ExportMiscCreated
    {
      get => exportMiscCreated ??= new();
      set => exportMiscCreated = value;
    }

    /// <summary>
    /// A value of ExportCollectionCreated.
    /// </summary>
    [JsonPropertyName("exportCollectionCreated")]
    public Common ExportCollectionCreated
    {
      get => exportCollectionCreated ??= new();
      set => exportCollectionCreated = value;
    }

    /// <summary>
    /// A value of ExportOrdersCreated.
    /// </summary>
    [JsonPropertyName("exportOrdersCreated")]
    public Common ExportOrdersCreated
    {
      get => exportOrdersCreated ??= new();
      set => exportOrdersCreated = value;
    }

    /// <summary>
    /// A value of ExportPartCreated.
    /// </summary>
    [JsonPropertyName("exportPartCreated")]
    public Common ExportPartCreated
    {
      get => exportPartCreated ??= new();
      set => exportPartCreated = value;
    }

    /// <summary>
    /// A value of ExportApLocateCreated.
    /// </summary>
    [JsonPropertyName("exportApLocateCreated")]
    public Common ExportApLocateCreated
    {
      get => exportApLocateCreated ??= new();
      set => exportApLocateCreated = value;
    }

    /// <summary>
    /// A value of ExportApIdentCreated.
    /// </summary>
    [JsonPropertyName("exportApIdentCreated")]
    public Common ExportApIdentCreated
    {
      get => exportApIdentCreated ??= new();
      set => exportApIdentCreated = value;
    }

    /// <summary>
    /// A value of ExportNonIvdRejected.
    /// </summary>
    [JsonPropertyName("exportNonIvdRejected")]
    public Common ExportNonIvdRejected
    {
      get => exportNonIvdRejected ??= new();
      set => exportNonIvdRejected = value;
    }

    /// <summary>
    /// A value of ExportCasesCreated.
    /// </summary>
    [JsonPropertyName("exportCasesCreated")]
    public Common ExportCasesCreated
    {
      get => exportCasesCreated ??= new();
      set => exportCasesCreated = value;
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
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
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
    /// Gets a value of Order.
    /// </summary>
    [JsonIgnore]
    public Array<OrderGroup> Order => order ??= new(OrderGroup.Capacity);

    /// <summary>
    /// Gets a value of Order for json serialization.
    /// </summary>
    [JsonPropertyName("order")]
    [Computed]
    public IList<OrderGroup> Order_Json
    {
      get => order;
      set => Order.Assign(value);
    }

    /// <summary>
    /// Gets a value of Collection.
    /// </summary>
    [JsonIgnore]
    public Array<CollectionGroup> Collection => collection ??= new(
      CollectionGroup.Capacity);

    /// <summary>
    /// Gets a value of Collection for json serialization.
    /// </summary>
    [JsonPropertyName("collection")]
    [Computed]
    public IList<CollectionGroup> Collection_Json
    {
      get => collection;
      set => Collection.Assign(value);
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
    /// A value of ExportPreviousRejection.
    /// </summary>
    [JsonPropertyName("exportPreviousRejection")]
    public Common ExportPreviousRejection
    {
      get => exportPreviousRejection ??= new();
      set => exportPreviousRejection = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Common exportPayAddrUpdated;
    private Common exportPayAddrCreated;
    private Common exportContAddrUpdated;
    private Common exportContAddrCreated;
    private Common exportIntContactUpdate;
    private Common exportIntContactCreate;
    private Common exportUpdates;
    private Common exportCaseAssigns;
    private Common exportMiscCreated;
    private Common exportCollectionCreated;
    private Common exportOrdersCreated;
    private Common exportPartCreated;
    private Common exportApLocateCreated;
    private Common exportApIdentCreated;
    private Common exportNonIvdRejected;
    private Common exportCasesCreated;
    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private Array<ParticipantGroup> participant;
    private Array<OrderGroup> order;
    private Array<CollectionGroup> collection;
    private InterstateMiscellaneous interstateMiscellaneous;
    private Common exportPreviousRejection;
    private DateWorkArea max;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of RollbackError.
    /// </summary>
    [JsonPropertyName("rollbackError")]
    public Common RollbackError
    {
      get => rollbackError ??= new();
      set => rollbackError = value;
    }

    /// <summary>
    /// A value of WriteError.
    /// </summary>
    [JsonPropertyName("writeError")]
    public Common WriteError
    {
      get => writeError ??= new();
      set => writeError = value;
    }

    /// <summary>
    /// A value of ErrorMessage.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public WorkArea ErrorMessage
    {
      get => errorMessage ??= new();
      set => errorMessage = value;
    }

    /// <summary>
    /// A value of ZdelExportNeededToWrite.
    /// </summary>
    [JsonPropertyName("zdelExportNeededToWrite")]
    public EabReportSend ZdelExportNeededToWrite
    {
      get => zdelExportNeededToWrite ??= new();
      set => zdelExportNeededToWrite = value;
    }

    private ServiceProvider serviceProvider;
    private Office office;
    private Common rollbackError;
    private Common writeError;
    private WorkArea errorMessage;
    private EabReportSend zdelExportNeededToWrite;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of CountAbsParentsPerCase.
    /// </summary>
    [JsonPropertyName("countAbsParentsPerCase")]
    public Common CountAbsParentsPerCase
    {
      get => countAbsParentsPerCase ??= new();
      set => countAbsParentsPerCase = value;
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
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of Office21.
    /// </summary>
    [JsonPropertyName("office21")]
    public Common Office21
    {
      get => office21 ??= new();
      set => office21 = value;
    }

    /// <summary>
    /// A value of ZdelLocalContactExists.
    /// </summary>
    [JsonPropertyName("zdelLocalContactExists")]
    public Common ZdelLocalContactExists
    {
      get => zdelLocalContactExists ??= new();
      set => zdelLocalContactExists = value;
    }

    /// <summary>
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
    }

    /// <summary>
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    /// <summary>
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
    }

    /// <summary>
    /// A value of OspFound.
    /// </summary>
    [JsonPropertyName("ospFound")]
    public Common OspFound
    {
      get => ospFound ??= new();
      set => ospFound = value;
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
    /// A value of ApFirstNameAlpha.
    /// </summary>
    [JsonPropertyName("apFirstNameAlpha")]
    public WorkArea ApFirstNameAlpha
    {
      get => apFirstNameAlpha ??= new();
      set => apFirstNameAlpha = value;
    }

    /// <summary>
    /// A value of ZdelLocalMax.
    /// </summary>
    [JsonPropertyName("zdelLocalMax")]
    public DateWorkArea ZdelLocalMax
    {
      get => zdelLocalMax ??= new();
      set => zdelLocalMax = value;
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
    /// A value of ZdelLocalCurrent.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrent")]
    public DateWorkArea ZdelLocalCurrent
    {
      get => zdelLocalCurrent ??= new();
      set => zdelLocalCurrent = value;
    }

    /// <summary>
    /// A value of ApLastNameAlpha.
    /// </summary>
    [JsonPropertyName("apLastNameAlpha")]
    public WorkArea ApLastNameAlpha
    {
      get => apLastNameAlpha ??= new();
      set => apLastNameAlpha = value;
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
    /// A value of InterstateRequestFound.
    /// </summary>
    [JsonPropertyName("interstateRequestFound")]
    public Common InterstateRequestFound
    {
      get => interstateRequestFound ??= new();
      set => interstateRequestFound = value;
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
    /// A value of DepPart.
    /// </summary>
    [JsonPropertyName("depPart")]
    public Common DepPart
    {
      get => depPart ??= new();
      set => depPart = value;
    }

    /// <summary>
    /// A value of CpPart.
    /// </summary>
    [JsonPropertyName("cpPart")]
    public Common CpPart
    {
      get => cpPart ??= new();
      set => cpPart = value;
    }

    /// <summary>
    /// A value of CsenetCollectDataBlock.
    /// </summary>
    [JsonPropertyName("csenetCollectDataBlock")]
    public Common CsenetCollectDataBlock
    {
      get => csenetCollectDataBlock ??= new();
      set => csenetCollectDataBlock = value;
    }

    /// <summary>
    /// A value of CsenetOrderDataBlock.
    /// </summary>
    [JsonPropertyName("csenetOrderDataBlock")]
    public Common CsenetOrderDataBlock
    {
      get => csenetOrderDataBlock ??= new();
      set => csenetOrderDataBlock = value;
    }

    /// <summary>
    /// A value of CsenetPartDataBlock.
    /// </summary>
    [JsonPropertyName("csenetPartDataBlock")]
    public Common CsenetPartDataBlock
    {
      get => csenetPartDataBlock ??= new();
      set => csenetPartDataBlock = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ServiceProvider Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
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

    private CsePerson csePerson;
    private Common countAbsParentsPerCase;
    private InterstateMiscellaneous interstateMiscellaneous;
    private InterstatePaymentAddress interstatePaymentAddress;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private Common office21;
    private Common zdelLocalContactExists;
    private InterstateCollection interstateCollection;
    private InterstateSupportOrder interstateSupportOrder;
    private InterstateParticipant interstateParticipant;
    private Common ospFound;
    private Infrastructure infrastructure;
    private WorkArea apFirstNameAlpha;
    private DateWorkArea zdelLocalMax;
    private DateWorkArea null1;
    private DateWorkArea zdelLocalCurrent;
    private WorkArea apLastNameAlpha;
    private InterstateRequest interstateRequest;
    private InterstateCase interstateCase;
    private Common interstateRequestFound;
    private Case1 case1;
    private Common depPart;
    private Common cpPart;
    private Common csenetCollectDataBlock;
    private Common csenetOrderDataBlock;
    private Common csenetPartDataBlock;
    private ServiceProvider zdel;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common ae;
    private AbendData abendData;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ZdelInterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("zdelInterstatePaymentAddress")]
    public InterstatePaymentAddress ZdelInterstatePaymentAddress
    {
      get => zdelInterstatePaymentAddress ??= new();
      set => zdelInterstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of ZdelInterstateContactAddress.
    /// </summary>
    [JsonPropertyName("zdelInterstateContactAddress")]
    public InterstateContactAddress ZdelInterstateContactAddress
    {
      get => zdelInterstateContactAddress ??= new();
      set => zdelInterstateContactAddress = value;
    }

    /// <summary>
    /// A value of ZdelInterstateContact.
    /// </summary>
    [JsonPropertyName("zdelInterstateContact")]
    public InterstateContact ZdelInterstateContact
    {
      get => zdelInterstateContact ??= new();
      set => zdelInterstateContact = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
    }

    /// <summary>
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    /// <summary>
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
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

    private ServiceProvider serviceProvider;
    private InterstatePaymentAddress zdelInterstatePaymentAddress;
    private InterstateContactAddress zdelInterstateContactAddress;
    private InterstateContact zdelInterstateContact;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private InterstateCaseAssignment interstateCaseAssignment;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private CaseRole absentParent;
    private CsePerson csePerson;
    private Case1 case1;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateMiscellaneous interstateMiscellaneous;
    private InterstateCollection interstateCollection;
    private InterstateSupportOrder interstateSupportOrder;
    private InterstateParticipant interstateParticipant;
    private InterstateApLocate interstateApLocate;
    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
  }
#endregion
}
