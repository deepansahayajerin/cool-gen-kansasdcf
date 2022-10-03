// Program: SI_QLRQ_SEND_QUICK_LOCATE_RQSTS, ID: 372393250, model: 746.
// Short name: SWE01238
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
/// A program: SI_QLRQ_SEND_QUICK_LOCATE_RQSTS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB sets up the CSENet Quick Locate request to be send out.
/// </para>
/// </summary>
[Serializable]
public partial class SiQlrqSendQuickLocateRqsts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QLRQ_SEND_QUICK_LOCATE_RQSTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQlrqSendQuickLocateRqsts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQlrqSendQuickLocateRqsts.
  /// </summary>
  public SiQlrqSendQuickLocateRqsts(IContext context, Import import,
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
    // ****************************************************
    // A.Kinney	04/29/97	Changed Current_Date
    // ****************************************************
    // ***************************************************************
    // 03/14/00  C. Ott  PR # 85011  Case Program Type not determined correctly 
    // for outgoing interstate case.  Call new action block to retrieve arrears
    // programs.
    // ***************************************************************
    // 04/09/01 swsrchf I00117417  Set KS_CASE_IND to SPACE on the Send of an 
    // LO1.
    // ***************************************************************************
    local.Current.Date = Now().Date;

    if (ReadCase())
    {
      if (AsChar(entities.Case1.Status) == 'C')
      {
        ExitState = "SI0000_NO_CSENET_OUT_CLOSED_CASE";

        return;
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadCsePersonAbsentParent())
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    if (ReadFips())
    {
      local.InterstateRequest.OtherStateFips = entities.Fips.State;
      export.Common.State = entities.Fips.StateAbbreviation;
    }
    else
    {
      ExitState = "ACO_NE0000_INVALID_STATE_CODE";

      return;
    }

    // ***************************************************
    // Check that only 3 requests can be sent out per day.
    // ***************************************************
    if (import.Lo1SentTodayCount.Count != 0)
    {
      // THIS CHECK NEEDS TO BE DONE ONLY ONCE. SO THIS IS DONE THE FIRST TIME 
      // THIS CAB IS INVOKED.
      foreach(var item in ReadInterstateRequestHistoryInterstateRequest3())
      {
        ++local.Lo1SentTodayCount.Count;
      }

      if (import.Lo1SentTodayCount.Count + local.Lo1SentTodayCount.Count > 3)
      {
        ExitState = "SI0000_ONLY_3_LO1_ALLOWED";

        return;
      }
      else
      {
        export.Lo1SentTodayCount.Count = 0;
      }
    }

    // ***************************************************
    // If no response is received on a request, another LO1
    // can be send only after a month.
    // ***************************************************
    local.Lo1ForStateCount.Count = 0;

    if (ReadInterstateRequestHistoryInterstateRequest2())
    {
      local.Lo1ForStateCount.Count = 1;
    }

    if (local.Lo1ForStateCount.Count != 1)
    {
      // CONTINUE, NO PREVIOUS QUICK LOCATE HAS BEEN SENT.
    }
    else
    {
      // PREVIOUS QUICK LOCATE RECORD EXISTS. CHECK FOR RESPONSE RECEIVED.
      local.Lo1ForStateCount.Count = 0;

      if (ReadInterstateRequestHistoryInterstateRequest1())
      {
        local.Lo1ForStateCount.Count = 1;
      }

      if (local.Lo1ForStateCount.Count == 1)
      {
        // QUICK LOCATE RESPONSE EXISTS. CHECK FOR 3 MONTHS PAST BEFORE ANOTHER 
        // REQUEST CAN BE SEND.
        if (Lt(AddMonths(local.Current.Date, -3), entities.Send.TransactionDate))
          
        {
          ExitState = "SI0000_LO1R_REQUIRES_3_MNTH_WAIT";

          return;
        }
      }
      else
      {
        // QUICK LOCATE RESPONSE DOES NOT EXISTS. CHECK FOR 1 MONTH PAST BEFORE 
        // ANOTHER REQUEST CAN BE SEND.
        if (Lt(AddMonths(local.Current.Date, -1), entities.Send.TransactionDate))
          
        {
          ExitState = "SI0000_LO1R_REQUIRES_1_MNTH_WAIT";

          return;
        }
      }
    }

    local.InterstateRequest.KsCaseInd = "Y";
    export.Send.TransactionDate = local.Current.Date;
    export.Send.TransactionDirectionInd = "O";
    export.Send.ActionCode = "R";
    export.Send.FunctionalTypeCode = "LO1";
    export.Send.ActionReasonCode = "";
    export.Send.AttachmentIndicator = "N";
    export.Send.Note = Spaces(InterstateRequestHistory.Note_MaxLength);
    export.Send.ActionResolutionDate = null;

    // CHECK IF INTERSTATE REQUEST ALREADY EXISTS FOR THIS CASE TO THIS STATE 
    // FOR THIS AP.
    if (ReadInterstateRequest())
    {
      // INTERSTATE REQUEST ALREADY EXISTS. CREATE HISTORY.
      export.InterstateRequest.Assign(entities.InterstateRequest);
      UseSiCreateIsRequestHistory();

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        return;
      }
    }
    else
    {
      UseSiReadCaseProgramType();

      // ***************************************************************
      // 03/14/00  C. Ott  PR # 85011  Case Program Type not determined 
      // correctly for outgoing interstate case.  Call new action block to
      // retrieve arrears programs.
      // ***************************************************************
      if (IsEmpty(local.Program.Code) || !IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseSiReadArrOnlyCasePrgType();
        ExitState = "ACO_NN0000_ALL_OK";
      }

      // CREATE INTERSTATE REQUEST AND HISTORY.
      local.InterstateRequest.CaseType = local.Program.Code;
      local.InterstateRequest.OtherStateCaseStatus = "";
      local.InterstateRequest.OtherStateCaseClosureDate = local.Current.Date;
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      UseSiCreateIsRequest();

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        return;
      }

      local.InterstateRequestCreated.Flag = "Y";
      UseSiCreateIsRequestHistory();

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        return;
      }
    }

    // ******************************************************
    // CREATE THE CASE_DB, AP_ID_DB AND AP_LOCATE_DB HERE.
    // ******************************************************
    local.InterstateCase.AttachmentsInd = export.Send.AttachmentIndicator ?? Spaces
      (1);
    local.InterstateCase.CaseDataInd = 1;
    local.InterstateCase.ApIdentificationInd = 1;
    local.InterstateCase.ApLocateDataInd = 1;
    local.InterstateCase.ParticipantDataInd = 0;
    local.InterstateCase.OrderDataInd = 0;
    local.InterstateCase.CollectionDataInd = 0;
    local.InterstateCase.InformationInd = 0;
    UseSiCreateOgCsenetCaseDb();
    MoveInterstateCase2(local.InterstateCase, export.InterstateCase);

    // **************************************************************
    // If the Interstate Request was created for this Quick Locate Request, 
    // close the Interstate Request so that the case does not display as an
    // Interstate case.
    // **************************************************************
    if (AsChar(local.InterstateRequestCreated.Flag) == 'Y')
    {
      // *** Problem report I00117417
      // *** 04/09/01 swsrchf
      // *** start
      export.InterstateRequest.OtherStateCaseStatus = "";

      // *** end
      // *** 04/09/01 swsrchf
      // *** Problem report I00117417
      export.InterstateRequest.KsCaseInd = "";
      UseSiUpdateInterstateRqstStatus();

      if (IsExitState("ACO_NI0000_UPDATE_SUCCESSFUL"))
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
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
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.CaseType = source.CaseType;
    target.KsCaseInd = source.KsCaseInd;
    target.OtherStateCaseClosureReason = source.OtherStateCaseClosureReason;
    target.OtherStateCaseClosureDate = source.OtherStateCaseClosureDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
  }

  private void UseSiCreateIsRequest()
  {
    var useImport = new SiCreateIsRequest.Import();
    var useExport = new SiCreateIsRequest.Export();

    useImport.Ap.Number = entities.Ap.Number;
    useImport.Case1.Number = entities.Case1.Number;
    MoveInterstateRequest2(local.InterstateRequest, useImport.InterstateRequest);
      

    Call(SiCreateIsRequest.Execute, useImport, useExport);

    export.InterstateRequest.Assign(useExport.InterstateRequest);
  }

  private void UseSiCreateIsRequestHistory()
  {
    var useImport = new SiCreateIsRequestHistory.Import();
    var useExport = new SiCreateIsRequestHistory.Export();

    useImport.Ap.Number = entities.Ap.Number;
    useImport.Case1.Number = import.Case1.Number;
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      
    useImport.InterstateRequestHistory.Assign(export.Send);

    Call(SiCreateIsRequestHistory.Execute, useImport, useExport);

    export.Send.Assign(useExport.InterstateRequestHistory);
  }

  private void UseSiCreateOgCsenetCaseDb()
  {
    var useImport = new SiCreateOgCsenetCaseDb.Import();
    var useExport = new SiCreateOgCsenetCaseDb.Export();

    useImport.Ap.Number = entities.Ap.Number;
    MoveCase1(entities.Case1, useImport.Case1);
    MoveInterstateCase1(local.InterstateCase, useImport.InterstateCase);
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      
    MoveInterstateRequestHistory(export.Send, useImport.InterstateRequestHistory);
      

    Call(SiCreateOgCsenetCaseDb.Execute, useImport, useExport);

    MoveInterstateCase1(useExport.InterstateCase, local.InterstateCase);
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

  private void UseSiUpdateInterstateRqstStatus()
  {
    var useImport = new SiUpdateInterstateRqstStatus.Import();
    var useExport = new SiUpdateInterstateRqstStatus.Export();

    useImport.InterstateRequest.Assign(export.InterstateRequest);

    Call(SiUpdateInterstateRqstStatus.Execute, useImport, useExport);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePersonAbsentParent()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadCsePersonAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Ap.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.AbsentParent.CasNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetString(command, "stateAbbreviation", import.State.State);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
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
          command, "othrStateFipsCd", local.InterstateRequest.OtherStateFips);
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
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequestHistoryInterstateRequest1()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.Received.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequestHistoryInterstateRequest1",
      (db, command) =>
      {
        db.SetDate(
          command, "transactionDate",
          entities.Send.TransactionDate.GetValueOrDefault());
        db.SetInt32(
          command, "othrStateFipsCd", local.InterstateRequest.OtherStateFips);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
      },
      (db, reader) =>
      {
        entities.Received.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.Received.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Received.CreatedBy = db.GetNullableString(reader, 2);
        entities.Received.TransactionDirectionInd = db.GetString(reader, 3);
        entities.Received.TransactionSerialNum = db.GetInt64(reader, 4);
        entities.Received.ActionCode = db.GetString(reader, 5);
        entities.Received.FunctionalTypeCode = db.GetString(reader, 6);
        entities.Received.TransactionDate = db.GetDate(reader, 7);
        entities.Received.ActionReasonCode = db.GetNullableString(reader, 8);
        entities.Received.ActionResolutionDate = db.GetNullableDate(reader, 9);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 11);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 12);
        entities.InterstateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 14);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 16);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 17);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 18);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 19);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 20);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 21);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 22);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 23);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 24);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 25);
        entities.Received.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequestHistoryInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.Send.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequestHistoryInterstateRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "othrStateFipsCd", local.InterstateRequest.OtherStateFips);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
      },
      (db, reader) =>
      {
        entities.Send.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.Send.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Send.CreatedBy = db.GetNullableString(reader, 2);
        entities.Send.TransactionDirectionInd = db.GetString(reader, 3);
        entities.Send.TransactionSerialNum = db.GetInt64(reader, 4);
        entities.Send.ActionCode = db.GetString(reader, 5);
        entities.Send.FunctionalTypeCode = db.GetString(reader, 6);
        entities.Send.TransactionDate = db.GetDate(reader, 7);
        entities.Send.ActionReasonCode = db.GetNullableString(reader, 8);
        entities.Send.ActionResolutionDate = db.GetNullableDate(reader, 9);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 11);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 12);
        entities.InterstateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 14);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 16);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 17);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 18);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 19);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 20);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 21);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 22);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 23);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 24);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 25);
        entities.Send.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private IEnumerable<bool> ReadInterstateRequestHistoryInterstateRequest3()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.Send.Populated = false;
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequestHistoryInterstateRequest3",
      (db, command) =>
      {
        db.SetDate(
          command, "transactionDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
      },
      (db, reader) =>
      {
        entities.Send.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.Send.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Send.CreatedBy = db.GetNullableString(reader, 2);
        entities.Send.TransactionDirectionInd = db.GetString(reader, 3);
        entities.Send.TransactionSerialNum = db.GetInt64(reader, 4);
        entities.Send.ActionCode = db.GetString(reader, 5);
        entities.Send.FunctionalTypeCode = db.GetString(reader, 6);
        entities.Send.TransactionDate = db.GetDate(reader, 7);
        entities.Send.ActionReasonCode = db.GetNullableString(reader, 8);
        entities.Send.ActionResolutionDate = db.GetNullableDate(reader, 9);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 11);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 12);
        entities.InterstateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 14);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 16);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 17);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 18);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 19);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 20);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 21);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 22);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 23);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 24);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 25);
        entities.Send.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

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
    /// <summary>
    /// A value of Lo1SentTodayCount.
    /// </summary>
    [JsonPropertyName("lo1SentTodayCount")]
    public Common Lo1SentTodayCount
    {
      get => lo1SentTodayCount ??= new();
      set => lo1SentTodayCount = value;
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
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Common State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private Common lo1SentTodayCount;
    private Case1 case1;
    private Common state;
    private CsePersonsWorkSet ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of Lo1SentTodayCount.
    /// </summary>
    [JsonPropertyName("lo1SentTodayCount")]
    public Common Lo1SentTodayCount
    {
      get => lo1SentTodayCount ??= new();
      set => lo1SentTodayCount = value;
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
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public InterstateRequestHistory Send
    {
      get => send ??= new();
      set => send = value;
    }

    private Common common;
    private InterstateCase interstateCase;
    private Common lo1SentTodayCount;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory send;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of InterstateRequestCreated.
    /// </summary>
    [JsonPropertyName("interstateRequestCreated")]
    public Common InterstateRequestCreated
    {
      get => interstateRequestCreated ??= new();
      set => interstateRequestCreated = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of Lo1ForStateCount.
    /// </summary>
    [JsonPropertyName("lo1ForStateCount")]
    public Common Lo1ForStateCount
    {
      get => lo1ForStateCount ??= new();
      set => lo1ForStateCount = value;
    }

    /// <summary>
    /// A value of Lo1SentTodayCount.
    /// </summary>
    [JsonPropertyName("lo1SentTodayCount")]
    public Common Lo1SentTodayCount
    {
      get => lo1SentTodayCount ??= new();
      set => lo1SentTodayCount = value;
    }

    private Program program;
    private Common interstateRequestCreated;
    private DateWorkArea current;
    private InterstateCase interstateCase;
    private InterstateRequest interstateRequest;
    private Common lo1ForStateCount;
    private Common lo1SentTodayCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Received.
    /// </summary>
    [JsonPropertyName("received")]
    public InterstateRequestHistory Received
    {
      get => received ??= new();
      set => received = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public InterstateRequestHistory Send
    {
      get => send ??= new();
      set => send = value;
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

    private InterstateRequestHistory received;
    private CaseRole absentParent;
    private Fips fips;
    private InterstateRequestHistory send;
    private InterstateRequest interstateRequest;
    private CsePerson ap;
    private Case1 case1;
  }
#endregion
}
