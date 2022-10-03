// Program: SI_B295_POST_CSENET_IN_TRANS, ID: 372621028, model: 746.
// Short name: SWEI295B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B295_POST_CSENET_IN_TRANS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB295PostCsenetInTrans: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B295_POST_CSENET_IN_TRANS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB295PostCsenetInTrans(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB295PostCsenetInTrans.
  /// </summary>
  public SiB295PostCsenetInTrans(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //         M A I N T E N A N C E   L O G
    // Date		Developer	Description
    // 03/05/1999	Carl Ott	Initial Dev.
    // 04/20/1999	M Ramirez	Made changes to PrAD to
    // 				incorporate the use of new EAB.
    // 05/19/1999	M Ramirez	Added local_received_date_time
    // 				interstate_case for the change to Version 3.1
    // 06/07/1999	M Ramirez	Fixed restart logic
    // 07/02/1999	M Ramirez	Rework to use housekeeping and closing cabs
    // 05/12/2009      J Harden  CQ7815  Close adabas at end of program
    // -------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiB295Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // mjr
    // ------------------------------------------------------------
    // 04/20/1999
    // Use of the EAB:
    // 	OPENI	- to open a file for reading
    // 	OPENO	- to open a file for writing
    // 	OPENX	- to open a file for extended writing
    // 	WRITE	- to write a record to a file
    // 	READ	- to read a record from a file
    // 	CLOSEI	- to close a file for reading
    // 	CLOSEO	- to close a file for writing
    // -------------------------------------------------------------------------
    local.External.FileInstruction = "OPENI";
    UseSiEabFormatCsenetOutTrans2();

    if (!IsEmpty(local.External.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the CSENet IN file.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (local.RestartRecord.Count > 0)
    {
      // mjr
      // ----------------------------------------------
      // 04/20/1999
      // Added repeating loop.
      // EAB will not handle repositioning for READs.
      // -----------------------------------------------------------
      local.RecordNumber.Count = 1;

      for(var limit = local.RestartRecord.Count; local.RecordNumber.Count <= limit
        ; ++local.RecordNumber.Count)
      {
        local.External.FileInstruction = "READ";
        UseSiEabFormatCsenetOutTrans2();

        if (!IsEmpty(local.External.TextReturnCode))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered positioning the CSENet IN file.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      local.RecordNumber.Count = local.RestartRecord.Count;
    }

    // mjr
    // -------------------------------------------
    // 05/19/1999
    // Added following 2 statements for the change to Version 3.1
    // NOTE:  I used CURRENT_DATE since we are also using the actual time.
    // --------------------------------------------------------
    local.ReceivedDateTime.DateReceived = Now().Date;
    local.ReceivedDateTime.TimeReceived = TimeOfDay(Now());

    do
    {
      local.RollbackRecord.Flag = "N";
      local.Commit.Flag = "N";
      local.External.FileInstruction = "READ";
      UseSiEabFormatCsenetOutTrans1();

      if (IsEmpty(local.External.TextReturnCode))
      {
        ++local.RecordNumber.Count;

        if (local.RollbackRecord.Count == local.RecordNumber.Count)
        {
          // *****************************************************************
          // If counters are equal, the current record caused the previous
          // rollback error.
          // Commit to the database and bypass the error record.
          // ****************************************************************
          // mjr--->  Don't erase the rollback record number
          local.Commit.Flag = "Y";

          goto Test;
        }

        ++local.CommitReads.Count;
        UseSiConvertCsenetToKessepVals();

        // mjr
        // -------------------------------------------
        // 05/19/1999
        // Added following 2 statements for the change to Version 3.1
        // --------------------------------------------------------
        local.InterstateCase.DateReceived = local.ReceivedDateTime.DateReceived;
        local.InterstateCase.TimeReceived =
          local.ReceivedDateTime.TimeReceived.GetValueOrDefault();
        UseSiBuildCsenetInData();

        if (AsChar(local.WriteError.Flag) == 'Y' && local
          .RecordNumber.Count >= local.RollbackRecord.Count)
        {
          // mjr
          // ------------------------------------------------
          // 06/07/1999
          // Write the error only if the error has not already been written.
          // It would have already been written if the record had been
          // processed before a rollback record had been encountered.
          // -------------------------------------------------------------
          ++local.ErrorRecords.Count;
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = local.ErrorMessage.Text60 + NumberToString
            (local.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
            (DateToInt(local.InterstateCase.TransactionDate), 8, 8) + "  " + Substring
            (local.InterstateCase.KsCaseId, 15, 1, 10) + "  " + NumberToString
            (local.Office.SystemGeneratedId, 12, 4) + "    " + local
            .ServiceProvider.UserId;

          if (AsChar(local.RollbackRecord.Flag) == 'Y')
          {
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + " - ROLLBACK";
          }

          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }
        }

        if (AsChar(local.RollbackRecord.Flag) == 'Y')
        {
          local.RollbackRecord.Count = local.RecordNumber.Count;
          UseEabRollbackSql();

          if (local.External.NumericReturnCode != 0)
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }

          // mjr
          // --------------------------------------------------
          // 06/07/1999
          // Since no other process will be updating this procedure's
          // restart_info, the READ can be replace with the use of local
          // variables.
          // ---------------------------------------------------------------
          // ***************************************************
          // *Close theCSENet IN File.
          // ***************************************************
          local.External.FileInstruction = "CLOSEI";
          UseSiEabFormatCsenetOutTrans2();

          if (!IsEmpty(local.External.TextReturnCode))
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered closing the CSENet IN file.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // mjr----> First action for READing a file is OPEN
          local.External.FileInstruction = "OPENI";
          UseSiEabFormatCsenetOutTrans2();

          if (!IsEmpty(local.External.TextReturnCode))
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered opening the CSENet IN file.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
          {
            // mjr
            // ----------------------------------------------
            // 04/20/1999
            // Added repeating loop.
            // EAB will not handle repositioning for READs.
            // -----------------------------------------------------------
            local.RecordNumber.Count = 1;

            for(var limit = local.RestartRecord.Count; local
              .RecordNumber.Count <= limit; ++local.RecordNumber.Count)
            {
              local.External.FileInstruction = "READ";
              UseSiEabFormatCsenetOutTrans2();

              if (!IsEmpty(local.External.TextReturnCode))
              {
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "Error encountered positioning the CSENet IN file.";
                UseCabErrorReport();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }
            }
          }

          local.RecordNumber.Count = local.RestartRecord.Count;
          local.CommitUpdates.Count = 0;
          local.CommitReads.Count = 0;
        }
      }
      else if (Equal(local.External.TextReturnCode, "EF"))
      {
        // mjr
        // -------------------------------------------------
        // 06/07/1999
        // Send message that the end of file was reached.
        // --------------------------------------------------------------
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "     End of file was reached successfully.";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }

        break;
      }
      else
      {
        ++local.ErrorRecords.Count;
        local.WriteError.Flag = "Y";
        local.NeededToWrite.RptDetail =
          "Error encountered reading the CSENet IN file.";
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

Test:

      if (local.CommitReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .CommitUpdates.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.Commit.Flag = "Y";
      }

      if (AsChar(local.Commit.Flag) == 'Y')
      {
        local.RestartRecord.Count = local.RecordNumber.Count;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          NumberToString(local.RestartRecord.Count, 250);
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered updating the Program Checkpoint Restart information.";
            
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }
        }

        UseExtToDoACommit();
        local.CommitUpdates.Count = 0;
        local.CommitReads.Count = 0;
      }
    }
    while(!Equal(local.External.TextReturnCode, "EF"));

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.CheckpointCount = 0;
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered updating the Program Checkpoint Restart information.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }
      }

      // mjr
      // -------------------------------------------
      // 06/07/1999
      // Added commit here, so errors with closing files will
      // not require the job to be re-run.
      // --------------------------------------------------------
      UseExtToDoACommit();
    }

    // ***************************************************
    // *Close the CSENet IN File.
    // ***************************************************
    local.External.FileInstruction = "CLOSEI";
    UseSiEabFormatCsenetOutTrans2();
    UseSiB295WriteControlsAndClose();
    UseSiCloseAdabas();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveCollection1(SiEabFormatCsenetOutTrans.Export.
    CollectionGroup source, Local.CollectionGroup target)
  {
    MoveInterstateCollection(source.InterstateCollection,
      target.InterstateCollection);
  }

  private static void MoveCollection2(Local.CollectionGroup source,
    SiEabFormatCsenetOutTrans.Export.CollectionGroup target)
  {
    target.InterstateCollection.Assign(source.InterstateCollection);
  }

  private static void MoveCollection3(Local.CollectionGroup source,
    SiBuildCsenetInData.Import.CollectionGroup target)
  {
    target.InterstateCollection.Assign(source.InterstateCollection);
  }

  private static void MoveCollection4(Local.CollectionGroup source,
    SiConvertCsenetToKessepVals.Import.CollectionGroup target)
  {
    target.InterstateCollection.Assign(source.InterstateCollection);
  }

  private static void MoveCollection5(SiConvertCsenetToKessepVals.Export.
    CollectionGroup source, Local.CollectionGroup target)
  {
    target.InterstateCollection.Assign(source.InterstateCollection);
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.LocalFipsState = source.LocalFipsState;
    target.LocalFipsCounty = source.LocalFipsCounty;
    target.LocalFipsLocation = source.LocalFipsLocation;
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.TransactionDate = source.TransactionDate;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.ActionResolutionDate = source.ActionResolutionDate;
    target.AttachmentsInd = source.AttachmentsInd;
    target.CaseDataInd = source.CaseDataInd;
    target.ApIdentificationInd = source.ApIdentificationInd;
    target.ApLocateDataInd = source.ApLocateDataInd;
    target.ParticipantDataInd = source.ParticipantDataInd;
    target.OrderDataInd = source.OrderDataInd;
    target.CollectionDataInd = source.CollectionDataInd;
    target.InformationInd = source.InformationInd;
    target.SentDate = source.SentDate;
    target.SentTime = source.SentTime;
    target.DueDate = source.DueDate;
    target.OverdueInd = source.OverdueInd;
    target.DateReceived = source.DateReceived;
    target.TimeReceived = source.TimeReceived;
    target.AttachmentsDueDate = source.AttachmentsDueDate;
    target.InterstateFormsPrinted = source.InterstateFormsPrinted;
    target.CaseType = source.CaseType;
    target.CaseStatus = source.CaseStatus;
    target.PaymentMailingAddressLine1 = source.PaymentMailingAddressLine1;
    target.PaymentAddressLine2 = source.PaymentAddressLine2;
    target.PaymentCity = source.PaymentCity;
    target.PaymentState = source.PaymentState;
    target.PaymentZipCode5 = source.PaymentZipCode5;
    target.PaymentZipCode4 = source.PaymentZipCode4;
    target.ContactNameLast = source.ContactNameLast;
    target.ContactNameFirst = source.ContactNameFirst;
    target.ContactNameMiddle = source.ContactNameMiddle;
    target.ContactNameSuffix = source.ContactNameSuffix;
    target.ContactAddressLine1 = source.ContactAddressLine1;
    target.ContactAddressLine2 = source.ContactAddressLine2;
    target.ContactCity = source.ContactCity;
    target.ContactState = source.ContactState;
    target.ContactZipCode5 = source.ContactZipCode5;
    target.ContactZipCode4 = source.ContactZipCode4;
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.AssnDeactDt = source.AssnDeactDt;
    target.AssnDeactInd = source.AssnDeactInd;
    target.LastDeferDt = source.LastDeferDt;
    target.Memo = source.Memo;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactFaxNumber = source.ContactFaxNumber;
    target.ContactFaxAreaCode = source.ContactFaxAreaCode;
    target.ContactInternetAddress = source.ContactInternetAddress;
    target.InitiatingDocketNumber = source.InitiatingDocketNumber;
    target.SendPaymentsBankAccount = source.SendPaymentsBankAccount;
    target.SendPaymentsRoutingCode = source.SendPaymentsRoutingCode;
    target.NondisclosureFinding = source.NondisclosureFinding;
    target.RespondingDocketNumber = source.RespondingDocketNumber;
    target.StateWithCej = source.StateWithCej;
    target.PaymentFipsCounty = source.PaymentFipsCounty;
    target.PaymentFipsState = source.PaymentFipsState;
    target.PaymentFipsLocation = source.PaymentFipsLocation;
    target.ContactAreaCode = source.ContactAreaCode;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.LocalFipsState = source.LocalFipsState;
    target.LocalFipsCounty = source.LocalFipsCounty;
    target.LocalFipsLocation = source.LocalFipsLocation;
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.TransactionDate = source.TransactionDate;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.ActionResolutionDate = source.ActionResolutionDate;
    target.AttachmentsInd = source.AttachmentsInd;
    target.CaseDataInd = source.CaseDataInd;
    target.ApIdentificationInd = source.ApIdentificationInd;
    target.ApLocateDataInd = source.ApLocateDataInd;
    target.ParticipantDataInd = source.ParticipantDataInd;
    target.OrderDataInd = source.OrderDataInd;
    target.CollectionDataInd = source.CollectionDataInd;
    target.InformationInd = source.InformationInd;
    target.SentDate = source.SentDate;
    target.SentTime = source.SentTime;
    target.DueDate = source.DueDate;
    target.OverdueInd = source.OverdueInd;
    target.DateReceived = source.DateReceived;
    target.TimeReceived = source.TimeReceived;
    target.AttachmentsDueDate = source.AttachmentsDueDate;
    target.InterstateFormsPrinted = source.InterstateFormsPrinted;
    target.CaseType = source.CaseType;
    target.CaseStatus = source.CaseStatus;
    target.PaymentMailingAddressLine1 = source.PaymentMailingAddressLine1;
    target.PaymentAddressLine2 = source.PaymentAddressLine2;
    target.PaymentCity = source.PaymentCity;
    target.PaymentState = source.PaymentState;
    target.PaymentZipCode5 = source.PaymentZipCode5;
    target.PaymentZipCode4 = source.PaymentZipCode4;
    target.ContactNameLast = source.ContactNameLast;
    target.ContactNameFirst = source.ContactNameFirst;
    target.ContactNameMiddle = source.ContactNameMiddle;
    target.ContactNameSuffix = source.ContactNameSuffix;
    target.ContactAddressLine1 = source.ContactAddressLine1;
    target.ContactAddressLine2 = source.ContactAddressLine2;
    target.ContactCity = source.ContactCity;
    target.ContactState = source.ContactState;
    target.ContactZipCode5 = source.ContactZipCode5;
    target.ContactZipCode4 = source.ContactZipCode4;
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactFaxNumber = source.ContactFaxNumber;
    target.ContactFaxAreaCode = source.ContactFaxAreaCode;
    target.ContactInternetAddress = source.ContactInternetAddress;
    target.InitiatingDocketNumber = source.InitiatingDocketNumber;
    target.SendPaymentsBankAccount = source.SendPaymentsBankAccount;
    target.SendPaymentsRoutingCode = source.SendPaymentsRoutingCode;
    target.NondisclosureFinding = source.NondisclosureFinding;
    target.RespondingDocketNumber = source.RespondingDocketNumber;
    target.StateWithCej = source.StateWithCej;
    target.PaymentFipsCounty = source.PaymentFipsCounty;
    target.PaymentFipsState = source.PaymentFipsState;
    target.PaymentFipsLocation = source.PaymentFipsLocation;
    target.ContactAreaCode = source.ContactAreaCode;
  }

  private static void MoveInterstateCollection(InterstateCollection source,
    InterstateCollection target)
  {
    target.DateOfCollection = source.DateOfCollection;
    target.DateOfPosting = source.DateOfPosting;
    target.PaymentAmount = source.PaymentAmount;
    target.PaymentSource = source.PaymentSource;
    target.InterstatePaymentMethod = source.InterstatePaymentMethod;
    target.RdfiId = source.RdfiId;
    target.RdfiAccountNum = source.RdfiAccountNum;
  }

  private static void MoveInterstateParticipant(InterstateParticipant source,
    InterstateParticipant target)
  {
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.NameMiddle = source.NameMiddle;
    target.NameSuffix = source.NameSuffix;
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.Sex = source.Sex;
    target.Race = source.Race;
    target.Relationship = source.Relationship;
    target.Status = source.Status;
    target.DependentRelationCp = source.DependentRelationCp;
    target.AddressLine1 = source.AddressLine1;
    target.AddressLine2 = source.AddressLine2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.EmployerAddressLine1 = source.EmployerAddressLine1;
    target.EmployerAddressLine2 = source.EmployerAddressLine2;
    target.EmployerCity = source.EmployerCity;
    target.EmployerState = source.EmployerState;
    target.EmployerZipCode5 = source.EmployerZipCode5;
    target.EmployerZipCode4 = source.EmployerZipCode4;
    target.EmployerName = source.EmployerName;
    target.EmployerEin = source.EmployerEin;
    target.AddressVerifiedDate = source.AddressVerifiedDate;
    target.EmployerVerifiedDate = source.EmployerVerifiedDate;
    target.WorkPhone = source.WorkPhone;
    target.WorkAreaCode = source.WorkAreaCode;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.ChildStateOfResidence = source.ChildStateOfResidence;
    target.ChildPaternityStatus = source.ChildPaternityStatus;
    target.EmployerConfirmedInd = source.EmployerConfirmedInd;
    target.AddressConfirmedInd = source.AddressConfirmedInd;
  }

  private static void MoveInterstateSupportOrder(InterstateSupportOrder source,
    InterstateSupportOrder target)
  {
    target.FipsState = source.FipsState;
    target.FipsCounty = source.FipsCounty;
    target.FipsLocation = source.FipsLocation;
    target.Number = source.Number;
    target.OrderFilingDate = source.OrderFilingDate;
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
    target.PaymentFreq = source.PaymentFreq;
    target.AmountOrdered = source.AmountOrdered;
    target.EffectiveDate = source.EffectiveDate;
    target.EndDate = source.EndDate;
    target.CancelDate = source.CancelDate;
    target.ArrearsFreq = source.ArrearsFreq;
    target.ArrearsFreqAmount = source.ArrearsFreqAmount;
    target.ArrearsTotalAmount = source.ArrearsTotalAmount;
    target.ArrearsAfdcFromDate = source.ArrearsAfdcFromDate;
    target.ArrearsAfdcThruDate = source.ArrearsAfdcThruDate;
    target.ArrearsAfdcAmount = source.ArrearsAfdcAmount;
    target.ArrearsNonAfdcFromDate = source.ArrearsNonAfdcFromDate;
    target.ArrearsNonAfdcThruDate = source.ArrearsNonAfdcThruDate;
    target.ArrearsNonAfdcAmount = source.ArrearsNonAfdcAmount;
    target.FosterCareFromDate = source.FosterCareFromDate;
    target.FosterCareThruDate = source.FosterCareThruDate;
    target.FosterCareAmount = source.FosterCareAmount;
    target.MedicalFromDate = source.MedicalFromDate;
    target.MedicalThruDate = source.MedicalThruDate;
    target.MedicalAmount = source.MedicalAmount;
    target.MedicalOrderedInd = source.MedicalOrderedInd;
    target.TribunalCaseNumber = source.TribunalCaseNumber;
    target.DateOfLastPayment = source.DateOfLastPayment;
    target.ControllingOrderFlag = source.ControllingOrderFlag;
    target.NewOrderFlag = source.NewOrderFlag;
    target.DocketNumber = source.DocketNumber;
  }

  private static void MoveOrder1(SiEabFormatCsenetOutTrans.Export.
    OrderGroup source, Local.OrderGroup target)
  {
    MoveInterstateSupportOrder(source.InterstateSupportOrder,
      target.InterstateSupportOrder);
  }

  private static void MoveOrder2(Local.OrderGroup source,
    SiEabFormatCsenetOutTrans.Export.OrderGroup target)
  {
    target.InterstateSupportOrder.Assign(source.InterstateSupportOrder);
  }

  private static void MoveOrder3(Local.OrderGroup source,
    SiBuildCsenetInData.Import.OrderGroup target)
  {
    target.InterstateSupportOrder.Assign(source.InterstateSupportOrder);
  }

  private static void MoveOrder4(Local.OrderGroup source,
    SiConvertCsenetToKessepVals.Import.OrderGroup target)
  {
    target.InterstateSupportOrder.Assign(source.InterstateSupportOrder);
  }

  private static void MoveOrder5(SiConvertCsenetToKessepVals.Export.
    OrderGroup source, Local.OrderGroup target)
  {
    target.InterstateSupportOrder.Assign(source.InterstateSupportOrder);
  }

  private static void MoveParticipant1(SiEabFormatCsenetOutTrans.Export.
    ParticipantGroup source, Local.ParticipantGroup target)
  {
    MoveInterstateParticipant(source.InterstateParticipant,
      target.InterstateParticipant);
  }

  private static void MoveParticipant2(Local.ParticipantGroup source,
    SiEabFormatCsenetOutTrans.Export.ParticipantGroup target)
  {
    target.InterstateParticipant.Assign(source.InterstateParticipant);
  }

  private static void MoveParticipant3(Local.ParticipantGroup source,
    SiBuildCsenetInData.Import.ParticipantGroup target)
  {
    target.InterstateParticipant.Assign(source.InterstateParticipant);
  }

  private static void MoveParticipant4(Local.ParticipantGroup source,
    SiConvertCsenetToKessepVals.Import.ParticipantGroup target)
  {
    target.InterstateParticipant.Assign(source.InterstateParticipant);
  }

  private static void MoveParticipant5(SiConvertCsenetToKessepVals.Export.
    ParticipantGroup source, Local.ParticipantGroup target)
  {
    target.InterstateParticipant.Assign(source.InterstateParticipant);
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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiB295Housekeeping()
  {
    var useImport = new SiB295Housekeeping.Import();
    var useExport = new SiB295Housekeeping.Export();

    Call(SiB295Housekeeping.Execute, useImport, useExport);

    local.Max.Date = useExport.Max.Date;
    MoveDateWorkArea(useExport.Current, local.Current);
    local.RestartRecord.Count = useExport.RestartRecord.Count;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseSiB295WriteControlsAndClose()
  {
    var useImport = new SiB295WriteControlsAndClose.Import();
    var useExport = new SiB295WriteControlsAndClose.Export();

    useImport.PreviousRejects.Count = local.PreviousRejection.Count;
    useImport.InterstContactCreates.Count = local.IntContactCreated.Count;
    useImport.InterstContactUpdates.Count = local.IntContactUpdated.Count;
    useImport.ContactAddrCreates.Count = local.ContactAddressCreated.Count;
    useImport.ContactAddrUpdates.Count = local.ContactAddressUpdated.Count;
    useImport.PaymentAddrCreates.Count = local.PayAddressCreated.Count;
    useImport.PaymentAddrUpdates.Count = local.PayAddressUpdated.Count;
    useImport.Errors.Count = local.ErrorRecords.Count;
    useImport.IntCaseAssgnmntCreates.Count = local.IsCaseAssignCreated.Count;
    useImport.MiscCreates.Count = local.MiscDbCreated.Count;
    useImport.CollectionCreates.Count = local.CollectionDbCreated.Count;
    useImport.OrderCreates.Count = local.SupportOrderDbCreated.Count;
    useImport.ParticipantCreates.Count = local.PartDbCreated.Count;
    useImport.ApLocateCreates.Count = local.ApLocateCreated.Count;
    useImport.ApIdentCreates.Count = local.ApIdentCreated.Count;
    useImport.NonIvdReject.Count = local.NonIvdRejected.Count;
    useImport.Read.Count = local.RecordNumber.Count;
    useImport.InterstateCaseCreates.Count = local.InterstateCaseCreates.Count;

    Call(SiB295WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSiBuildCsenetInData()
  {
    var useImport = new SiBuildCsenetInData.Import();
    var useExport = new SiBuildCsenetInData.Export();

    useImport.ExportIntContactCreate.Count = local.IntContactCreated.Count;
    useImport.ExportIntContactUpdate.Count = local.IntContactUpdated.Count;
    useImport.ExportContAddrCreated.Count = local.ContactAddressCreated.Count;
    useImport.ExportContAddrUpdated.Count = local.ContactAddressUpdated.Count;
    useImport.ExportPayAddrCreated.Count = local.PayAddressCreated.Count;
    useImport.ExportPayAddrUpdated.Count = local.PayAddressUpdated.Count;
    useImport.ExportCaseAssigns.Count = local.IsCaseAssignCreated.Count;
    useImport.ExportMiscCreated.Count = local.MiscDbCreated.Count;
    useImport.ExportCollectionCreated.Count = local.CollectionDbCreated.Count;
    useImport.ExportOrdersCreated.Count = local.SupportOrderDbCreated.Count;
    useImport.ExportPartCreated.Count = local.PartDbCreated.Count;
    useImport.ExportApLocateCreated.Count = local.ApLocateCreated.Count;
    useImport.ExportApIdentCreated.Count = local.ApIdentCreated.Count;
    useImport.ExportNonIvdRejected.Count = local.NonIvdRejected.Count;
    useImport.InterstateMiscellaneous.Assign(local.InterstateMiscellaneous);
    local.Collection.CopyTo(useImport.Collection, MoveCollection3);
    local.Order.CopyTo(useImport.Order, MoveOrder3);
    local.Participant.CopyTo(useImport.Participant, MoveParticipant3);
    useImport.ExportCasesCreated.Count = local.InterstateCaseCreates.Count;
    useImport.InterstateApLocate.Assign(local.InterstateApLocate);
    useImport.InterstateApIdentification.
      Assign(local.InterstateApIdentification);
    useImport.InterstateCase.Assign(local.InterstateCase);
    useImport.ExportUpdates.Count = local.CommitUpdates.Count;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ExportPreviousRejection.Count = local.PreviousRejection.Count;
    useImport.Max.Date = local.Max.Date;
    MoveDateWorkArea(local.Current, useImport.Current);

    Call(SiBuildCsenetInData.Execute, useImport, useExport);

    local.IntContactCreated.Count = useImport.ExportIntContactCreate.Count;
    local.IntContactUpdated.Count = useImport.ExportIntContactUpdate.Count;
    local.ContactAddressCreated.Count = useImport.ExportContAddrCreated.Count;
    local.ContactAddressUpdated.Count = useImport.ExportContAddrUpdated.Count;
    local.PayAddressCreated.Count = useImport.ExportPayAddrCreated.Count;
    local.PayAddressUpdated.Count = useImport.ExportPayAddrUpdated.Count;
    local.IsCaseAssignCreated.Count = useImport.ExportCaseAssigns.Count;
    local.MiscDbCreated.Count = useImport.ExportMiscCreated.Count;
    local.CollectionDbCreated.Count = useImport.ExportCollectionCreated.Count;
    local.SupportOrderDbCreated.Count = useImport.ExportOrdersCreated.Count;
    local.PartDbCreated.Count = useImport.ExportPartCreated.Count;
    local.ApLocateCreated.Count = useImport.ExportApLocateCreated.Count;
    local.ApIdentCreated.Count = useImport.ExportApIdentCreated.Count;
    local.NonIvdRejected.Count = useImport.ExportNonIvdRejected.Count;
    local.InterstateCaseCreates.Count = useImport.ExportCasesCreated.Count;
    local.CommitUpdates.Count = useImport.ExportUpdates.Count;
    local.PreviousRejection.Count = useImport.ExportPreviousRejection.Count;
    local.ServiceProvider.UserId = useExport.ServiceProvider.UserId;
    local.Office.SystemGeneratedId = useExport.Office.SystemGeneratedId;
    local.ErrorMessage.Text60 = useExport.ErrorMessage.Text60;
    local.RollbackRecord.Flag = useExport.RollbackError.Flag;
    local.WriteError.Flag = useExport.WriteError.Flag;
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void UseSiConvertCsenetToKessepVals()
  {
    var useImport = new SiConvertCsenetToKessepVals.Import();
    var useExport = new SiConvertCsenetToKessepVals.Export();

    local.Collection.CopyTo(useImport.Collection, MoveCollection4);
    local.Order.CopyTo(useImport.Order, MoveOrder4);
    local.Participant.CopyTo(useImport.Participant, MoveParticipant4);
    useImport.InterstateApLocate.Assign(local.InterstateApLocate);
    useImport.InterstateApIdentification.
      Assign(local.InterstateApIdentification);
    MoveInterstateCase1(local.InterstateCase, useImport.InterstateCase);

    Call(SiConvertCsenetToKessepVals.Execute, useImport, useExport);

    useExport.Collection.CopyTo(local.Collection, MoveCollection5);
    useExport.Order.CopyTo(local.Order, MoveOrder5);
    useExport.Participant.CopyTo(local.Participant, MoveParticipant5);
    local.InterstateApLocate.Assign(useExport.InterstateApLocate);
    local.InterstateApIdentification.
      Assign(useExport.InterstateApIdentification);
    local.InterstateCase.Assign(useExport.InterstateCase);
  }

  private void UseSiEabFormatCsenetOutTrans1()
  {
    var useImport = new SiEabFormatCsenetOutTrans.Import();
    var useExport = new SiEabFormatCsenetOutTrans.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useExport.InterstateMiscellaneous.Assign(local.InterstateMiscellaneous);
    local.Collection.CopyTo(useExport.Collection, MoveCollection2);
    local.Order.CopyTo(useExport.Order, MoveOrder2);
    local.Participant.CopyTo(useExport.Participant, MoveParticipant2);
    useExport.InterstateApLocate.Assign(local.InterstateApLocate);
    useExport.InterstateApIdentification.
      Assign(local.InterstateApIdentification);
    useExport.InterstateCase.Assign(local.InterstateCase);
    MoveExternal(local.External, useExport.External);

    Call(SiEabFormatCsenetOutTrans.Execute, useImport, useExport);

    local.InterstateMiscellaneous.Assign(useExport.InterstateMiscellaneous);
    useExport.Collection.CopyTo(local.Collection, MoveCollection1);
    useExport.Order.CopyTo(local.Order, MoveOrder1);
    useExport.Participant.CopyTo(local.Participant, MoveParticipant1);
    local.InterstateApLocate.Assign(useExport.InterstateApLocate);
    local.InterstateApIdentification.
      Assign(useExport.InterstateApIdentification);
    MoveInterstateCase2(useExport.InterstateCase, local.InterstateCase);
    MoveExternal(useExport.External, local.External);
  }

  private void UseSiEabFormatCsenetOutTrans2()
  {
    var useImport = new SiEabFormatCsenetOutTrans.Import();
    var useExport = new SiEabFormatCsenetOutTrans.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    MoveExternal(local.External, useExport.External);

    Call(SiEabFormatCsenetOutTrans.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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

    /// <summary>
    /// A value of PreviousRejection.
    /// </summary>
    [JsonPropertyName("previousRejection")]
    public Common PreviousRejection
    {
      get => previousRejection ??= new();
      set => previousRejection = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
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
    /// A value of ReceivedDateTime.
    /// </summary>
    [JsonPropertyName("receivedDateTime")]
    public InterstateCase ReceivedDateTime
    {
      get => receivedDateTime ??= new();
      set => receivedDateTime = value;
    }

    /// <summary>
    /// A value of IntContactCreated.
    /// </summary>
    [JsonPropertyName("intContactCreated")]
    public Common IntContactCreated
    {
      get => intContactCreated ??= new();
      set => intContactCreated = value;
    }

    /// <summary>
    /// A value of IntContactUpdated.
    /// </summary>
    [JsonPropertyName("intContactUpdated")]
    public Common IntContactUpdated
    {
      get => intContactUpdated ??= new();
      set => intContactUpdated = value;
    }

    /// <summary>
    /// A value of ContactAddressCreated.
    /// </summary>
    [JsonPropertyName("contactAddressCreated")]
    public Common ContactAddressCreated
    {
      get => contactAddressCreated ??= new();
      set => contactAddressCreated = value;
    }

    /// <summary>
    /// A value of ContactAddressUpdated.
    /// </summary>
    [JsonPropertyName("contactAddressUpdated")]
    public Common ContactAddressUpdated
    {
      get => contactAddressUpdated ??= new();
      set => contactAddressUpdated = value;
    }

    /// <summary>
    /// A value of PayAddressCreated.
    /// </summary>
    [JsonPropertyName("payAddressCreated")]
    public Common PayAddressCreated
    {
      get => payAddressCreated ??= new();
      set => payAddressCreated = value;
    }

    /// <summary>
    /// A value of PayAddressUpdated.
    /// </summary>
    [JsonPropertyName("payAddressUpdated")]
    public Common PayAddressUpdated
    {
      get => payAddressUpdated ??= new();
      set => payAddressUpdated = value;
    }

    /// <summary>
    /// A value of ErrorRecords.
    /// </summary>
    [JsonPropertyName("errorRecords")]
    public Common ErrorRecords
    {
      get => errorRecords ??= new();
      set => errorRecords = value;
    }

    /// <summary>
    /// A value of IsCaseAssignCreated.
    /// </summary>
    [JsonPropertyName("isCaseAssignCreated")]
    public Common IsCaseAssignCreated
    {
      get => isCaseAssignCreated ??= new();
      set => isCaseAssignCreated = value;
    }

    /// <summary>
    /// A value of MiscDbCreated.
    /// </summary>
    [JsonPropertyName("miscDbCreated")]
    public Common MiscDbCreated
    {
      get => miscDbCreated ??= new();
      set => miscDbCreated = value;
    }

    /// <summary>
    /// A value of CollectionDbCreated.
    /// </summary>
    [JsonPropertyName("collectionDbCreated")]
    public Common CollectionDbCreated
    {
      get => collectionDbCreated ??= new();
      set => collectionDbCreated = value;
    }

    /// <summary>
    /// A value of SupportOrderDbCreated.
    /// </summary>
    [JsonPropertyName("supportOrderDbCreated")]
    public Common SupportOrderDbCreated
    {
      get => supportOrderDbCreated ??= new();
      set => supportOrderDbCreated = value;
    }

    /// <summary>
    /// A value of PartDbCreated.
    /// </summary>
    [JsonPropertyName("partDbCreated")]
    public Common PartDbCreated
    {
      get => partDbCreated ??= new();
      set => partDbCreated = value;
    }

    /// <summary>
    /// A value of ApLocateCreated.
    /// </summary>
    [JsonPropertyName("apLocateCreated")]
    public Common ApLocateCreated
    {
      get => apLocateCreated ??= new();
      set => apLocateCreated = value;
    }

    /// <summary>
    /// A value of ApIdentCreated.
    /// </summary>
    [JsonPropertyName("apIdentCreated")]
    public Common ApIdentCreated
    {
      get => apIdentCreated ??= new();
      set => apIdentCreated = value;
    }

    /// <summary>
    /// A value of NonIvdRejected.
    /// </summary>
    [JsonPropertyName("nonIvdRejected")]
    public Common NonIvdRejected
    {
      get => nonIvdRejected ??= new();
      set => nonIvdRejected = value;
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
    /// A value of RecordNumber.
    /// </summary>
    [JsonPropertyName("recordNumber")]
    public Common RecordNumber
    {
      get => recordNumber ??= new();
      set => recordNumber = value;
    }

    /// <summary>
    /// A value of RollbackRecord.
    /// </summary>
    [JsonPropertyName("rollbackRecord")]
    public Common RollbackRecord
    {
      get => rollbackRecord ??= new();
      set => rollbackRecord = value;
    }

    /// <summary>
    /// A value of ZdelLocalIsCaseNotFound.
    /// </summary>
    [JsonPropertyName("zdelLocalIsCaseNotFound")]
    public Common ZdelLocalIsCaseNotFound
    {
      get => zdelLocalIsCaseNotFound ??= new();
      set => zdelLocalIsCaseNotFound = value;
    }

    /// <summary>
    /// A value of ZdelLocalNumberOfRecsWrittn.
    /// </summary>
    [JsonPropertyName("zdelLocalNumberOfRecsWrittn")]
    public Common ZdelLocalNumberOfRecsWrittn
    {
      get => zdelLocalNumberOfRecsWrittn ??= new();
      set => zdelLocalNumberOfRecsWrittn = value;
    }

    /// <summary>
    /// A value of RestartRecord.
    /// </summary>
    [JsonPropertyName("restartRecord")]
    public Common RestartRecord
    {
      get => restartRecord ??= new();
      set => restartRecord = value;
    }

    /// <summary>
    /// A value of ZdelLocalRestartOutputErr.
    /// </summary>
    [JsonPropertyName("zdelLocalRestartOutputErr")]
    public Common ZdelLocalRestartOutputErr
    {
      get => zdelLocalRestartOutputErr ??= new();
      set => zdelLocalRestartOutputErr = value;
    }

    /// <summary>
    /// A value of ZdelLocalApIdentCreates.
    /// </summary>
    [JsonPropertyName("zdelLocalApIdentCreates")]
    public Common ZdelLocalApIdentCreates
    {
      get => zdelLocalApIdentCreates ??= new();
      set => zdelLocalApIdentCreates = value;
    }

    /// <summary>
    /// A value of InterstateCaseCreates.
    /// </summary>
    [JsonPropertyName("interstateCaseCreates")]
    public Common InterstateCaseCreates
    {
      get => interstateCaseCreates ??= new();
      set => interstateCaseCreates = value;
    }

    /// <summary>
    /// A value of CreateApLocate.
    /// </summary>
    [JsonPropertyName("createApLocate")]
    public Common CreateApLocate
    {
      get => createApLocate ??= new();
      set => createApLocate = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of CommitUpdates.
    /// </summary>
    [JsonPropertyName("commitUpdates")]
    public Common CommitUpdates
    {
      get => commitUpdates ??= new();
      set => commitUpdates = value;
    }

    /// <summary>
    /// A value of CommitReads.
    /// </summary>
    [JsonPropertyName("commitReads")]
    public Common CommitReads
    {
      get => commitReads ??= new();
      set => commitReads = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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

    private DateWorkArea max;
    private DateWorkArea current;
    private Common previousRejection;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common commit;
    private WorkArea errorMessage;
    private InterstateCase receivedDateTime;
    private Common intContactCreated;
    private Common intContactUpdated;
    private Common contactAddressCreated;
    private Common contactAddressUpdated;
    private Common payAddressCreated;
    private Common payAddressUpdated;
    private Common errorRecords;
    private Common isCaseAssignCreated;
    private Common miscDbCreated;
    private Common collectionDbCreated;
    private Common supportOrderDbCreated;
    private Common partDbCreated;
    private Common apLocateCreated;
    private Common apIdentCreated;
    private Common nonIvdRejected;
    private InterstateMiscellaneous interstateMiscellaneous;
    private Array<CollectionGroup> collection;
    private Array<OrderGroup> order;
    private Array<ParticipantGroup> participant;
    private Common recordNumber;
    private Common rollbackRecord;
    private Common zdelLocalIsCaseNotFound;
    private Common zdelLocalNumberOfRecsWrittn;
    private Common restartRecord;
    private Common zdelLocalRestartOutputErr;
    private Common zdelLocalApIdentCreates;
    private Common interstateCaseCreates;
    private Common createApLocate;
    private InterstateApLocate interstateApLocate;
    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
    private Common commitUpdates;
    private Common commitReads;
    private External external;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common writeError;
  }
#endregion
}
