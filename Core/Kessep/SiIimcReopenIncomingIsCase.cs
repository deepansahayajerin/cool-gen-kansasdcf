// Program: SI_IIMC_REOPEN_INCOMING_IS_CASE, ID: 372855604, model: 746.
// Short name: SWE02594
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
/// A program: SI_IIMC_REOPEN_INCOMING_IS_CASE.
/// </para>
/// <para>
/// This CAB Reopens the Incoming Interstate case and sets up the Event and 
/// creates the Infrastructure record.
/// </para>
/// </summary>
[Serializable]
public partial class SiIimcReopenIncomingIsCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIMC_REOPEN_INCOMING_IS_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIimcReopenIncomingIsCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIimcReopenIncomingIsCase.
  /// </summary>
  public SiIimcReopenIncomingIsCase(IContext context, Import import,
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
    // 05/10/06 GVandy 	 WR230751	Add support for Tribal IV-D agencies.
    local.Current.Date = Now().Date;
    export.Case1.Number = import.Case1.Number;
    export.InterstateRequest.Assign(import.InterstateRequest);
    export.Ap.Number = import.Ap.Number;

    if (export.InterstateRequest.IntHGeneratedId == 0)
    {
      ExitState = "ACO_NE0000_DISPLAY_FIRST";
    }
    else
    {
      local.UpdateStatus.OtherStateCaseClosureReason = "";
      local.UpdateStatus.IntHGeneratedId =
        export.InterstateRequest.IntHGeneratedId;
      local.UpdateStatus.OtherStateCaseClosureDate = Now().Date;
      local.UpdateStatus.OtherStateCaseStatus = "O";
      local.UpdateStatus.KsCaseInd = "N";

      // ******************************************************************
      // WR - 010501 Check for open outgoing interstate request.
      // Cannot re-open if an open outgoing for the same state exists.
      // Tom Bobb 6/21/01
      // ******************************************************************
      if (ReadCaseInterstateRequest3())
      {
        ExitState = "ACO_NE0000_OPEN_OUT_INTER_REQ_AE";

        return;
      }
      else
      {
        // *************************************************************
        // Check if incoming is already opened
        // Tom Bobb 7/24/01
        // *************************************************************
        if (ReadCaseInterstateRequest1())
        {
          ExitState = "SI0000_INT_REQ_ALREADY_OPEN";

          return;
        }
      }

      // *************************************************************
      // Check if outgoing  is closed. If so, must use add or
      // reopen function.
      // Tom Bobb 7/24/01 PR00135692
      // *************************************************************
      if (ReadCaseInterstateRequest2())
      {
        // >>
        // If the incoming case has never been manually converted
        // user must use 'add'  function.
        if (!ReadInterstateRequestHistory())
        {
          ExitState = "SI00000_CLOSED_OG_INTER_AE";

          return;
        }
      }
      else
      {
        // >>
        // check to see if interstate request was initially created with an LO1
        if (ReadCaseInterstateRequest4())
        {
          ExitState = "CASE_NOT_IC_INTERSTATE";

          return;
        }
      }

      UseSiUpdateInterstateRqstStatus();

      if (IsExitState("ACO_NI0000_UPDATE_SUCCESSFUL"))
      {
        MoveInterstateRequest2(local.UpdateStatus, export.InterstateRequest);

        // ------------------------------------------------------------
        // Create the Interstate Request History for case reopen.
        // ------------------------------------------------------------
        if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'O')
        {
          local.InterstateRequestHistory.ActionReasonCode = "IICRO";
          local.InterstateRequestHistory.ActionCode = "";
          local.InterstateRequestHistory.FunctionalTypeCode = "";
          local.InterstateRequestHistory.TransactionDirectionInd = "";
          local.InterstateRequestHistory.AttachmentIndicator = "";
          local.InterstateRequestHistory.Note =
            Spaces(InterstateRequestHistory.Note_MaxLength);
          local.InterstateRequestHistory.TransactionSerialNum = 0;
          local.InterstateRequestHistory.ActionResolutionDate = local.Zero.Date;
          local.InterstateRequestHistory.TransactionDate = local.Current.Date;
          local.InterstateRequestHistory.CreatedBy = "SWEIIIMC";
          UseSiCreateIsRequestHistory();

          if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
            IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // ------------------------------------------------------------
          // Create the History for case reopen.
          // ------------------------------------------------------------
          local.Infrastructure.InitiatingStateCode = "KS";
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.EventId = 5;
          local.Infrastructure.ReasonCode = "REOPEN_INTSTATE";
          local.Infrastructure.BusinessObjectCd = "CAS";
          local.Infrastructure.CaseNumber = import.Case1.Number;
          local.Infrastructure.UserId = "IIMC";
          local.Infrastructure.ReferenceDate = local.Current.Date;

          if (!ReadCase())
          {
            ExitState = "CASE_NF";

            return;
          }

          foreach(var item in ReadCaseUnit())
          {
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

            if (!IsEmpty(import.OtherState.StateAbbreviation))
            {
              local.Infrastructure.Detail =
                "Incoming Interstate Case Reopened;" + " Initiating State :" + import
                .OtherState.StateAbbreviation;
            }
            else if (!IsEmpty(export.InterstateRequest.Country))
            {
              local.Infrastructure.Detail =
                "Incoming Interstate Case Reopened;" + " Initiating Country :" +
                (export.InterstateRequest.Country ?? "");
            }
            else if (!IsEmpty(export.InterstateRequest.TribalAgency))
            {
              local.Infrastructure.Detail =
                "Incoming Interstate Case Reopened;" + " Initiating Tribal Agency :" +
                (export.InterstateRequest.TribalAgency ?? "");
            }

            UseSpCabCreateInfrastructure();
          }

          if (entities.CaseUnit.CuNumber == 0)
          {
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            local.Infrastructure.ReasonCode = "";

            if (!IsEmpty(import.OtherState.StateAbbreviation))
            {
              local.Infrastructure.Detail =
                "Incoming Interstate Case (no Case Units) Reopened;" + " IN State :" +
                import.OtherState.StateAbbreviation;
            }
            else if (!IsEmpty(export.InterstateRequest.Country))
            {
              local.Infrastructure.Detail =
                "Incoming Interstate Case (no Case Units) Reopened;" + " IN Country :" +
                (export.InterstateRequest.Country ?? "");
            }
            else if (!IsEmpty(export.InterstateRequest.TribalAgency))
            {
              local.Infrastructure.Detail =
                "Incoming Interstate Case (no Case Units) Reopened;" + " IN Tribal Agency :" +
                (export.InterstateRequest.TribalAgency ?? "");
            }

            UseSpCabCreateInfrastructure();
          }

          if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
            IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        ExitState = "SI0000_INT_REQ_REOPEN_OK";
      }
      else
      {
        ExitState = "SI0000_INT_REQ_REOPEN_FAILED";
      }

      if (!ReadAbsentParentCsePerson())
      {
        ExitState = "CO0000_ABSENT_PARENT_NF";

        return;
      }

      // ------------------------------------------------------------
      // Update the Interstate Request for the Case and the AP.
      // ------------------------------------------------------------
      if (ReadInterstateRequest())
      {
        if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'C')
        {
          local.InterstateRequest.OtherStateCaseClosureDate =
            local.Current.Date;
        }
        else
        {
          local.InterstateRequest.OtherStateCaseClosureDate =
            export.InterstateRequest.OtherStateCaseClosureDate;
        }

        try
        {
          UpdateInterstateRequest();
          export.InterstateRequest.Assign(entities.InterstateRequest);
          export.IreqUpdated.Date =
            Date(entities.InterstateRequest.LastUpdatedTimestamp);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          if (ReadInterstateRequestHistory())
          {
            try
            {
              UpdateInterstateRequestHistory();
              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  break;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INTERSTATE_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "INTERSTATE_REQUEST_NF";
      }
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
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
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.KsCaseInd = source.KsCaseInd;
    target.OtherStateCaseClosureReason = source.OtherStateCaseClosureReason;
    target.OtherStateCaseClosureDate = source.OtherStateCaseClosureDate;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.CreatedBy = source.CreatedBy;
    target.TransactionDirectionInd = source.TransactionDirectionInd;
    target.TransactionSerialNum = source.TransactionSerialNum;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.TransactionDate = source.TransactionDate;
    target.ActionReasonCode = source.ActionReasonCode;
    target.ActionResolutionDate = source.ActionResolutionDate;
    target.AttachmentIndicator = source.AttachmentIndicator;
    target.Note = source.Note;
  }

  private void UseSiCreateIsRequestHistory()
  {
    var useImport = new SiCreateIsRequestHistory.Import();
    var useExport = new SiCreateIsRequestHistory.Export();

    useImport.InterstateCase.Assign(import.InterstateCase);
    MoveInterstateRequestHistory(local.InterstateRequestHistory,
      useImport.InterstateRequestHistory);
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      
    useImport.Ap.Number = export.Ap.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiCreateIsRequestHistory.Execute, useImport, useExport);
  }

  private void UseSiUpdateInterstateRqstStatus()
  {
    var useImport = new SiUpdateInterstateRqstStatus.Import();
    var useExport = new SiUpdateInterstateRqstStatus.Export();

    useImport.InterstateRequest.Assign(local.UpdateStatus);

    Call(SiUpdateInterstateRqstStatus.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private bool ReadAbsentParentCsePerson()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadAbsentParentCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
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
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 2);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 4);
        entities.Case1.IcTransactionDate = db.GetDate(reader, 5);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseInterstateRequest1()
  {
    entities.Case1.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadCaseInterstateRequest1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
        db.SetInt32(
          command, "otherStateFips", import.InterstateRequest.OtherStateFips);
        db.SetNullableString(
          command, "country", import.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", import.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 2);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 4);
        entities.Case1.IcTransactionDate = db.GetDate(reader, 5);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 6);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 7);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 8);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 9);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 10);
        entities.InterstateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 12);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 14);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 16);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 17);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 18);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 19);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 20);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 21);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 22);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 23);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 24);
        entities.Case1.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadCaseInterstateRequest2()
  {
    entities.Case1.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadCaseInterstateRequest2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
        db.SetInt32(
          command, "otherStateFips", import.InterstateRequest.OtherStateFips);
        db.SetNullableString(
          command, "country", import.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", import.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 2);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 4);
        entities.Case1.IcTransactionDate = db.GetDate(reader, 5);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 6);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 7);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 8);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 9);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 10);
        entities.InterstateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 12);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 14);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 16);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 17);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 18);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 19);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 20);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 21);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 22);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 23);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 24);
        entities.Case1.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadCaseInterstateRequest3()
  {
    entities.Case1.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadCaseInterstateRequest3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
        db.SetInt32(
          command, "otherStateFips", import.InterstateRequest.OtherStateFips);
        db.SetNullableString(
          command, "country", import.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", import.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 2);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 4);
        entities.Case1.IcTransactionDate = db.GetDate(reader, 5);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 6);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 7);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 8);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 9);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 10);
        entities.InterstateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 12);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 14);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 16);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 17);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 18);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 19);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 20);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 21);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 22);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 23);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 24);
        entities.Case1.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadCaseInterstateRequest4()
  {
    entities.Case1.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadCaseInterstateRequest4",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
        db.SetInt32(
          command, "otherStateFips", import.InterstateRequest.OtherStateFips);
        db.SetNullableString(
          command, "country", import.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", import.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 2);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 4);
        entities.Case1.IcTransactionDate = db.GetDate(reader, 5);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 6);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 7);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 8);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 9);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 10);
        entities.InterstateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 12);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 14);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 16);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 17);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 18);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 19);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 20);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 21);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 22);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 23);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 24);
        entities.Case1.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CasNo = db.GetString(reader, 5);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
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
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
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

  private void UpdateInterstateRequest()
  {
    var otherStateCaseId = export.InterstateRequest.OtherStateCaseId ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var otherStateCaseStatus = export.InterstateRequest.OtherStateCaseStatus;
    var caseType = export.InterstateRequest.CaseType ?? "";
    var otherStateCaseClosureReason =
      export.InterstateRequest.OtherStateCaseClosureReason ?? "";
    var otherStateCaseClosureDate = local.Current.Date;

    entities.InterstateRequest.Populated = false;
    Update("UpdateInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "otherStateCasId", otherStateCaseId);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "othStCaseStatus", otherStateCaseStatus);
        db.SetNullableString(command, "caseType", caseType);
        db.SetNullableString(
          command, "othStateClsRes", otherStateCaseClosureReason);
        db.
          SetNullableDate(command, "othStateClsDte", otherStateCaseClosureDate);
          
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.OtherStateCaseId = otherStateCaseId;
    entities.InterstateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterstateRequest.OtherStateCaseStatus = otherStateCaseStatus;
    entities.InterstateRequest.CaseType = caseType;
    entities.InterstateRequest.OtherStateCaseClosureReason =
      otherStateCaseClosureReason;
    entities.InterstateRequest.OtherStateCaseClosureDate =
      otherStateCaseClosureDate;
    entities.InterstateRequest.Populated = true;
  }

  private void UpdateInterstateRequestHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateRequestHistory.Populated);

    var note = export.InterstateRequestHistory.Note ?? "";

    entities.InterstateRequestHistory.Populated = false;
    Update("UpdateInterstateRequestHistory",
      (db, command) =>
      {
        db.SetNullableString(command, "note", note);
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequestHistory.IntGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          entities.InterstateRequestHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.InterstateRequestHistory.Note = note;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    private InterstateCase interstateCase;
    private CsePerson ap;
    private InterstateRequest interstateRequest;
    private Case1 case1;
    private Fips otherState;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    /// <summary>
    /// A value of IreqUpdated.
    /// </summary>
    [JsonPropertyName("ireqUpdated")]
    public DateWorkArea IreqUpdated
    {
      get => ireqUpdated ??= new();
      set => ireqUpdated = value;
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

    private InterstateRequest interstateRequest;
    private CsePerson ap;
    private Case1 case1;
    private DateWorkArea ireqUpdated;
    private InterstateRequestHistory interstateRequestHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of UpdateStatus.
    /// </summary>
    [JsonPropertyName("updateStatus")]
    public InterstateRequest UpdateStatus
    {
      get => updateStatus ??= new();
      set => updateStatus = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateRequest updateStatus;
    private InterstateRequestHistory interstateRequestHistory;
    private DateWorkArea zero;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private InterstateRequest interstateRequest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private Case1 case1;
    private CaseUnit caseUnit;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
    private CaseRole absentParent;
    private CsePerson ap;
  }
#endregion
}
