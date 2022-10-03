// Program: LE_B503_KS_TO_OCSE_RECON, ID: 371286452, model: 746.
// Short name: SWEL503B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B503_KS_TO_OCSE_RECON.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB503KsToOcseRecon: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B503_KS_TO_OCSE_RECON program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB503KsToOcseRecon(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB503KsToOcseRecon.
  /// </summary>
  public LeB503KsToOcseRecon(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // -------------------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // ---------------------------------------------------------------------
    // 09/28/06  GVandy	PR289943	Initial Development
    // 05/23/07  GVandy	WR 289942	Add support for B (Name Change) and Z (Address
    // Changes) transaction types.
    // 07/25/2007  GVandy	PR313068	Set ttype_a_add_new_case attribute to A (ADC
    // ), N (Non-ADC), or B (Both).
    // -------------------------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------------
    // --  The purpose of this program is to implement a "Kansas to OCSE" 
    // reconciliation for FDSO.
    // --
    // -- We will create a new Federal_Debt_Setoff certification record for each
    // obligor whom we show as currently
    // -- FDSO certified.  Each of these Federal_Debt_Setoff records will be 
    // sent to OCSE as an Add transaction.
    // -- When received at OCSE they will compare the certifications to those in
    // their master file and adjust their
    // -- master file accordingly.  See Federal Offset Program User Guide 
    // section 2.4.2 State-to-OCSE Reconciliation
    // -- for additional information.
    // ---------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------------------------------------------------------------------
    // --  Get PPI Information.
    // ---------------------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // ---------------------------------------------------------------------------------------------------------
    // -- Get checkpoint restart information.
    // ---------------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "PROGRAM_CHECKPOINT_RESTART_NF_AB";

      return;
    }

    // ---------------------------------------------------------------------------------------------------------
    // -- Open Error Report
    // ---------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.TranCode;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // ---------------------------------------------------------------------------------------------------------
    // -- Open Control Report.
    // ---------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport4();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_CONTROL_RPT";

      return;
    }

    // ---------------------------------------------------------------------------------------------------------
    // -- Initialize process variables.
    // ---------------------------------------------------------------------------------------------------------
    local.NumberOfReads.Count = 0;
    local.TotalObligorsRead.Count = 0;
    local.TotalDecertifiedObligors.Count = 0;
    local.TotalReconcileRecsAdded.Count = 0;

    if (Find(local.ProgramProcessingInfo.ParameterList, "ADDRESS") != 0)
    {
      // -- For the first run after adding support for address changes we need 
      // to find the current address and
      //    store it on the new FDSO certification record.  Following the first 
      // run, we will simply report whatever
      //    address is on the most recent FDSO record.  For the first run only,
      // "ADDRESS" must be
      //    placed somewhere in the PPI parameter list.
      local.StoreCurrentAddress.Flag = "Y";

      for(local.Counter.Count = 1; local.Counter.Count <= 2; ++
        local.Counter.Count)
      {
        switch(local.Counter.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "ADDRESS parameter set on MPPI parameter list.  Obligors address will be retrieved and sent on the FDSO reconciliation record.";
              

            break;
          case 2:
            local.EabReportSend.RptDetail = "";

            break;
          default:
            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }
      }
    }
    else
    {
      local.StoreCurrentAddress.Flag = "N";
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.Starting.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Process restarting from person number -   " + local.Starting.Number;
      UseCabControlReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }
    else
    {
      local.Starting.Number = "";
    }

    if (!ReadAdministrativeAction())
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "FDSO Administraction_Action record not found.";
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ---------------------------------------------------------------------------------------------------------
    // -- Main Processing Loop
    // ---------------------------------------------------------------------------------------------------------
    // -- Find all obligors which have ever been FDSO certified.
    foreach(var item in ReadCsePersonObligor())
    {
      ++local.NumberOfReads.Count;
      ++local.TotalObligorsRead.Count;

      // -- Find the most recent FDSO certification for the obligor.
      if (ReadFederalDebtSetoff())
      {
        // -- If the most recent FDSO record was a Decertification then exclude 
        // the obligor from reconciliation.
        if (AsChar(entities.FederalDebtSetoff.TtypeDDeleteCertification) == 'D'
          || Equal(entities.FederalDebtSetoff.AdcAmount, 0) && Equal
          (entities.FederalDebtSetoff.NonAdcAmount, 0))
        {
          // -- Skip the obligor.  They are not currently FDSO certified.
          ++local.TotalDecertifiedObligors.Count;

          goto Read;
        }

        local.FederalDebtSetoff.Assign(local.Null1);

        if (AsChar(local.StoreCurrentAddress.Flag) == 'Y')
        {
          // -- For the first run after adding support for address changes we 
          // need to find the current address and
          //    store it on the new FDSO certification record.  Following the 
          // first run, we will simply report whatever
          //    address is on the most recent FDSO record.
          UseSiGetCsePersonMailingAddr();

          if (AsChar(local.CsePersonAddress.LocationType) == 'D')
          {
            // -- For a domestic address we send street_1, street_2, city, 
            // state, zip code, zip + 4.
            local.FederalDebtSetoff.AddressStreet1 =
              local.CsePersonAddress.Street1 ?? "";
            local.FederalDebtSetoff.AddressStreet2 =
              local.CsePersonAddress.Street2 ?? "";
            local.FederalDebtSetoff.AddressCity =
              local.CsePersonAddress.City ?? "";
            local.FederalDebtSetoff.AddressState =
              local.CsePersonAddress.State ?? "";
            local.FederalDebtSetoff.AddressZip =
              (local.CsePersonAddress.ZipCode ?? "") + (
                local.CsePersonAddress.Zip4 ?? "");
          }
          else if (AsChar(local.CsePersonAddress.LocationType) == 'F')
          {
            // -- For a foreign address we send street_1, street_2, city, and 
            // country in the place of state.
            //    No zip, postal code, or province is sent.
            //    This is per Brian Peeler at OCSE.
            local.FederalDebtSetoff.AddressStreet1 =
              local.CsePersonAddress.Street1 ?? "";
            local.FederalDebtSetoff.AddressStreet2 =
              local.CsePersonAddress.Street2 ?? "";
            local.FederalDebtSetoff.AddressCity =
              local.CsePersonAddress.City ?? "";
            local.FederalDebtSetoff.AddressState =
              local.CsePersonAddress.Country ?? "";
          }
        }
        else
        {
          local.FederalDebtSetoff.AddressStreet1 =
            entities.FederalDebtSetoff.AddressStreet1;
          local.FederalDebtSetoff.AddressStreet2 =
            entities.FederalDebtSetoff.AddressStreet2;
          local.FederalDebtSetoff.AddressCity =
            entities.FederalDebtSetoff.AddressCity;
          local.FederalDebtSetoff.AddressState =
            entities.FederalDebtSetoff.AddressState;
          local.FederalDebtSetoff.AddressZip =
            entities.FederalDebtSetoff.AddressZip;
        }

        if (Lt(0, entities.FederalDebtSetoff.AdcAmount) && Lt
          (0, entities.FederalDebtSetoff.NonAdcAmount))
        {
          // -- We need to send an Add transaction for both ADC and Non-ADC.  
          // Set the ttype_a_add_new_case to B (Both).
          local.FederalDebtSetoff.TtypeAAddNewCase = "B";
        }
        else if (Lt(0, entities.FederalDebtSetoff.AdcAmount))
        {
          // -- We need to send an Add transaction for ADC only.  Set the 
          // ttype_a_add_new_case to A (ADC).
          local.FederalDebtSetoff.TtypeAAddNewCase = "A";
        }
        else if (Lt(0, entities.FederalDebtSetoff.NonAdcAmount))
        {
          // -- We need to send an Add transaction for Non-ADC only.  Set the 
          // ttype_a_add_new_case to N (Non-ADC).
          local.FederalDebtSetoff.TtypeAAddNewCase = "N";
        }

        // --  Create the new FDSO certification record.
        try
        {
          CreateFederalDebtSetoff();
          ++local.TotalReconcileRecsAdded.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              local.EabReportSend.RptDetail =
                "Error creating Federal_Debt_Setoff.  (Already Exists)  CSP Number = " +
                entities.CsePerson.Number;

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              local.EabReportSend.RptDetail =
                "Error creating Federal_Debt_Setoff.  (Permitted Value)  CSP Number = " +
                entities.CsePerson.Number;

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

Read:

      // ---------------------------------------------------------------------------------------------------------
      // -- Take a checkpoint.
      // ---------------------------------------------------------------------------------------------------------
      if (local.NumberOfReads.Count > local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        local.NumberOfReads.Count = 0;
        local.ProgramCheckpointRestart.ProgramName = global.UserId;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = entities.CsePerson.Number;
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Extract the exit state message and write to the error report.
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Checkpoint Error... " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error in External Commit Routine.  Return Code = " + NumberToString
            (local.External.NumericReturnCode, 14, 2);
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------
    // -- Write totals to the control report.
    // ---------------------------------------------------------------------------------------------------------
    for(local.Counter.Count = 1; local.Counter.Count <= 5; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Number of Obligors Previously Certified . . . . . . . . . . . . . . . " +
            NumberToString(local.TotalObligorsRead.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Number of Obligors Previously Certified that are Now Decertified. . . " +
            NumberToString(local.TotalDecertifiedObligors.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "Number of Reconciliation Records Created. . . . . . . . . . . . . . . " +
            NumberToString(local.TotalReconcileRecsAdded.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error Writing Control Report... Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ---------------------------------------------------------------------------------------------------------
    // -- Take final checkpoint.
    // ---------------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Final Checkpoint Error... " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "(02) Error in External Commit Routine.  Return Code = " + NumberToString
        (local.External.NumericReturnCode, 14, 2);
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ---------------------------------------------------------------------------------------------------------
    // -- Close the Control Report
    // ---------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...   Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ---------------------------------------------------------------------------------------------------------
    // -- Close the Error Report
    // ---------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Country = source.Country;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport4()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseReadPgmChkpntRestart()
  {
    var useImport = new ReadPgmChkpntRestart.Import();
    var useExport = new ReadPgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmChkpntRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    MoveProgramCheckpointRestart(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void CreateFederalDebtSetoff()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);

    var cpaType = entities.Obligor.Type1;
    var cspNumber = entities.Obligor.CspNumber;
    var type1 = entities.FederalDebtSetoff.Type1;
    var takenDate = local.ProgramProcessingInfo.ProcessDate;
    var aatType = entities.AdministrativeAction.Type1;
    var originalAmount = entities.FederalDebtSetoff.OriginalAmount;
    var currentAmount = entities.FederalDebtSetoff.CurrentAmount;
    var currentAmountDate = entities.FederalDebtSetoff.CurrentAmountDate;
    var decertifiedDate = entities.FederalDebtSetoff.DecertifiedDate;
    var notificationDate = entities.FederalDebtSetoff.NotificationDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var adcAmount = entities.FederalDebtSetoff.AdcAmount;
    var nonAdcAmount = entities.FederalDebtSetoff.NonAdcAmount;
    var injuredSpouseDate = entities.FederalDebtSetoff.InjuredSpouseDate;
    var notifiedBy = entities.FederalDebtSetoff.NotifiedBy;
    var etypeAdministrativeOffset =
      entities.FederalDebtSetoff.EtypeAdministrativeOffset;
    var localCode = entities.FederalDebtSetoff.LocalCode;
    var ssn = entities.FederalDebtSetoff.Ssn;
    var caseNumber = entities.FederalDebtSetoff.CaseNumber;
    var lastName = entities.FederalDebtSetoff.LastName;
    var firstName = entities.FederalDebtSetoff.FirstName;
    var amountOwed = entities.FederalDebtSetoff.AmountOwed;
    var ttypeAAddNewCase = local.FederalDebtSetoff.TtypeAAddNewCase ?? "";
    var caseType = entities.FederalDebtSetoff.CaseType;
    var transferState = entities.FederalDebtSetoff.TransferState;
    var localForTransfer = entities.FederalDebtSetoff.LocalForTransfer;
    var processYear = Year(local.ProgramProcessingInfo.ProcessDate);
    var tanfCode = entities.FederalDebtSetoff.TanfCode;
    var etypeFederalRetirement =
      entities.FederalDebtSetoff.EtypeFederalRetirement;
    var etypeFederalSalary = entities.FederalDebtSetoff.EtypeFederalSalary;
    var etypeTaxRefund = entities.FederalDebtSetoff.EtypeTaxRefund;
    var etypeVendorPaymentOrMisc =
      entities.FederalDebtSetoff.EtypeVendorPaymentOrMisc;
    var etypePassportDenial = entities.FederalDebtSetoff.EtypePassportDenial;
    var etypeFinancialInstitution =
      entities.FederalDebtSetoff.EtypeFinancialInstitution;
    var etypeAdmBankrupt = entities.FederalDebtSetoff.EtypeAdmBankrupt;
    var decertificationReason =
      entities.FederalDebtSetoff.DecertificationReason;
    var addressStreet1 = local.FederalDebtSetoff.AddressStreet1 ?? "";
    var addressStreet2 = local.FederalDebtSetoff.AddressStreet2 ?? "";
    var addressCity = local.FederalDebtSetoff.AddressCity ?? "";
    var addressState = local.FederalDebtSetoff.AddressState ?? "";
    var addressZip = local.FederalDebtSetoff.AddressZip ?? "";

    CheckValid<AdministrativeActCertification>("CpaType", cpaType);
    CheckValid<AdministrativeActCertification>("Type1", type1);
    entities.New1.Populated = false;
    Update("CreateFederalDebtSetoff",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetDate(command, "takenDt", takenDate);
        db.SetNullableString(command, "aatType", aatType);
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableDecimal(command, "currentAmt", currentAmount);
        db.SetNullableDate(command, "currentAmtDt", currentAmountDate);
        db.SetNullableDate(command, "decertifiedDt", decertifiedDate);
        db.SetNullableDate(command, "notificationDt", notificationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", null);
        db.SetNullableDate(command, "referralDt", default(DateTime));
        db.SetNullableDecimal(command, "recoveryAmt", 0M);
        db.SetNullableDecimal(command, "adcAmt", adcAmount);
        db.SetNullableDecimal(command, "nonAdcAmt", nonAdcAmount);
        db.SetNullableDate(command, "injuredSpouseDt", injuredSpouseDate);
        db.SetString(command, "witness", "");
        db.SetNullableString(command, "notifiedBy", notifiedBy);
        db.SetNullableString(command, "reasonWithdraw", "");
        db.SetNullableString(command, "denialReason", "");
        db.SetNullableDate(command, "dateSent", null);
        db.SetNullableString(
          command, "etypeAdminOffset", etypeAdministrativeOffset);
        db.SetNullableString(command, "localCode", localCode);
        db.SetInt32(command, "ssn", ssn);
        db.SetString(command, "caseNumber", caseNumber);
        db.SetString(command, "lastName", lastName);
        db.SetString(command, "firstName", firstName);
        db.SetInt32(command, "amountOwed", amountOwed);
        db.SetNullableString(command, "ttypeAddNewCase", ttypeAAddNewCase);
        db.SetString(command, "caseType", caseType);
        db.SetNullableString(command, "transferState", transferState);
        db.SetNullableInt32(command, "localForTransfer", localForTransfer);
        db.SetNullableInt32(command, "processYear", processYear);
        db.SetString(command, "tanfCode", tanfCode);
        db.SetNullableString(command, "ttypeDeleteCert", "");
        db.SetNullableString(command, "ttypeChngSubSt", "");
        db.SetNullableString(command, "ttypeModifyAmnt", "");
        db.SetNullableString(command, "ttypeModifyExcl", "");
        db.SetNullableString(command, "ttypeStatePymnt", "");
        db.SetNullableString(command, "ttypeXferAdmRvw", "");
        db.SetNullableString(command, "ttypeNameChange", "");
        db.SetNullableString(command, "ttypeAddressChg", "");
        db.
          SetNullableString(command, "etypeFedRetrmnt", etypeFederalRetirement);
          
        db.SetNullableString(command, "etypeFedSalary", etypeFederalSalary);
        db.SetNullableString(command, "etypeTaxRefund", etypeTaxRefund);
        db.SetNullableString(
          command, "etypeVndrPymntM", etypeVendorPaymentOrMisc);
        db.SetNullableString(command, "etypePsprtDenial", etypePassportDenial);
        db.
          SetNullableString(command, "etypeFinInst", etypeFinancialInstitution);
          
        db.SetNullableString(command, "returnStatus", "");
        db.SetNullableDate(command, "returnStatusDate", null);
        db.SetNullableString(command, "changeSsnInd", "");
        db.SetNullableString(command, "etypeAdmBankrupt", etypeAdmBankrupt);
        db.SetNullableString(command, "decertifyReason", decertificationReason);
        db.SetNullableString(command, "addressStreet1", addressStreet1);
        db.SetNullableString(command, "addressStreet2", addressStreet2);
        db.SetNullableString(command, "addressCity", addressCity);
        db.SetNullableString(command, "addressState", addressState);
        db.SetNullableString(command, "addressZip", addressZip);
        db.SetNullableInt32(command, "numCourtOrders", 0);
      });

    entities.New1.CpaType = cpaType;
    entities.New1.CspNumber = cspNumber;
    entities.New1.Type1 = type1;
    entities.New1.TakenDate = takenDate;
    entities.New1.AatType = aatType;
    entities.New1.OriginalAmount = originalAmount;
    entities.New1.CurrentAmount = currentAmount;
    entities.New1.CurrentAmountDate = currentAmountDate;
    entities.New1.DecertifiedDate = decertifiedDate;
    entities.New1.NotificationDate = notificationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTstamp = null;
    entities.New1.AdcAmount = adcAmount;
    entities.New1.NonAdcAmount = nonAdcAmount;
    entities.New1.InjuredSpouseDate = injuredSpouseDate;
    entities.New1.NotifiedBy = notifiedBy;
    entities.New1.DateSent = null;
    entities.New1.EtypeAdministrativeOffset = etypeAdministrativeOffset;
    entities.New1.LocalCode = localCode;
    entities.New1.Ssn = ssn;
    entities.New1.CaseNumber = caseNumber;
    entities.New1.LastName = lastName;
    entities.New1.FirstName = firstName;
    entities.New1.AmountOwed = amountOwed;
    entities.New1.TtypeAAddNewCase = ttypeAAddNewCase;
    entities.New1.CaseType = caseType;
    entities.New1.TransferState = transferState;
    entities.New1.LocalForTransfer = localForTransfer;
    entities.New1.ProcessYear = processYear;
    entities.New1.TanfCode = tanfCode;
    entities.New1.TtypeDDeleteCertification = "";
    entities.New1.TtypeLChangeSubmittingState = "";
    entities.New1.TtypeMModifyAmount = "";
    entities.New1.TtypeRModifyExclusion = "";
    entities.New1.TtypeSStatePayment = "";
    entities.New1.TtypeTTransferAdminReview = "";
    entities.New1.TtypeBNameChange = "";
    entities.New1.TtypeZAddressChange = "";
    entities.New1.EtypeFederalRetirement = etypeFederalRetirement;
    entities.New1.EtypeFederalSalary = etypeFederalSalary;
    entities.New1.EtypeTaxRefund = etypeTaxRefund;
    entities.New1.EtypeVendorPaymentOrMisc = etypeVendorPaymentOrMisc;
    entities.New1.EtypePassportDenial = etypePassportDenial;
    entities.New1.EtypeFinancialInstitution = etypeFinancialInstitution;
    entities.New1.ReturnStatus = "";
    entities.New1.ReturnStatusDate = null;
    entities.New1.EtypeAdmBankrupt = etypeAdmBankrupt;
    entities.New1.DecertificationReason = decertificationReason;
    entities.New1.AddressStreet1 = addressStreet1;
    entities.New1.AddressStreet2 = addressStreet2;
    entities.New1.AddressCity = addressCity;
    entities.New1.AddressState = addressState;
    entities.New1.AddressZip = addressZip;
    entities.New1.Populated = true;
  }

  private bool ReadAdministrativeAction()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      null,
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonObligor()
  {
    entities.Obligor.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonObligor",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Starting.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 1);
        entities.Obligor.Type1 = db.GetString(reader, 2);
        entities.Obligor.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);

        return true;
      });
  }

  private bool ReadFederalDebtSetoff()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.FederalDebtSetoff.Populated = false;

    return Read("ReadFederalDebtSetoff",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
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
        entities.FederalDebtSetoff.TtypeSStatePayment =
          db.GetNullableString(reader, 35);
        entities.FederalDebtSetoff.TtypeTTransferAdminReview =
          db.GetNullableString(reader, 36);
        entities.FederalDebtSetoff.TtypeBNameChange =
          db.GetNullableString(reader, 37);
        entities.FederalDebtSetoff.TtypeZAddressChange =
          db.GetNullableString(reader, 38);
        entities.FederalDebtSetoff.EtypeFederalRetirement =
          db.GetNullableString(reader, 39);
        entities.FederalDebtSetoff.EtypeFederalSalary =
          db.GetNullableString(reader, 40);
        entities.FederalDebtSetoff.EtypeTaxRefund =
          db.GetNullableString(reader, 41);
        entities.FederalDebtSetoff.EtypeVendorPaymentOrMisc =
          db.GetNullableString(reader, 42);
        entities.FederalDebtSetoff.EtypePassportDenial =
          db.GetNullableString(reader, 43);
        entities.FederalDebtSetoff.EtypeFinancialInstitution =
          db.GetNullableString(reader, 44);
        entities.FederalDebtSetoff.ReturnStatus =
          db.GetNullableString(reader, 45);
        entities.FederalDebtSetoff.ReturnStatusDate =
          db.GetNullableDate(reader, 46);
        entities.FederalDebtSetoff.EtypeAdmBankrupt =
          db.GetNullableString(reader, 47);
        entities.FederalDebtSetoff.DecertificationReason =
          db.GetNullableString(reader, 48);
        entities.FederalDebtSetoff.AddressStreet1 =
          db.GetNullableString(reader, 49);
        entities.FederalDebtSetoff.AddressStreet2 =
          db.GetNullableString(reader, 50);
        entities.FederalDebtSetoff.AddressCity =
          db.GetNullableString(reader, 51);
        entities.FederalDebtSetoff.AddressState =
          db.GetNullableString(reader, 52);
        entities.FederalDebtSetoff.AddressZip =
          db.GetNullableString(reader, 53);
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public AdministrativeActCertification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of StoreCurrentAddress.
    /// </summary>
    [JsonPropertyName("storeCurrentAddress")]
    public Common StoreCurrentAddress
    {
      get => storeCurrentAddress ??= new();
      set => storeCurrentAddress = value;
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
    /// A value of FederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("federalDebtSetoff")]
    public AdministrativeActCertification FederalDebtSetoff
    {
      get => federalDebtSetoff ??= new();
      set => federalDebtSetoff = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of TotalDecertifiedObligors.
    /// </summary>
    [JsonPropertyName("totalDecertifiedObligors")]
    public Common TotalDecertifiedObligors
    {
      get => totalDecertifiedObligors ??= new();
      set => totalDecertifiedObligors = value;
    }

    /// <summary>
    /// A value of NumberOfReads.
    /// </summary>
    [JsonPropertyName("numberOfReads")]
    public Common NumberOfReads
    {
      get => numberOfReads ??= new();
      set => numberOfReads = value;
    }

    /// <summary>
    /// A value of TotalReconcileRecsAdded.
    /// </summary>
    [JsonPropertyName("totalReconcileRecsAdded")]
    public Common TotalReconcileRecsAdded
    {
      get => totalReconcileRecsAdded ??= new();
      set => totalReconcileRecsAdded = value;
    }

    /// <summary>
    /// A value of TotalObligorsRead.
    /// </summary>
    [JsonPropertyName("totalObligorsRead")]
    public Common TotalObligorsRead
    {
      get => totalObligorsRead ??= new();
      set => totalObligorsRead = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CsePerson Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private AdministrativeActCertification null1;
    private Common storeCurrentAddress;
    private CsePersonAddress csePersonAddress;
    private AdministrativeActCertification federalDebtSetoff;
    private Common counter;
    private ExitStateWorkArea exitStateWorkArea;
    private External external;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common totalDecertifiedObligors;
    private Common numberOfReads;
    private Common totalReconcileRecsAdded;
    private Common totalObligorsRead;
    private CsePerson starting;
    private EabReportSend neededToWrite;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private AdministrativeAction administrativeAction;
    private AdministrativeActCertification new1;
    private AdministrativeActCertification federalDebtSetoff;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
  }
#endregion
}
