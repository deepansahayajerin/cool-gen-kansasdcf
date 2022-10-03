// Program: FN_B717_LINE_18, ID: 373360297, model: 746.
// Short name: SWE03048
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_18.
/// </summary>
[Serializable]
public partial class FnB717Line18: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_18 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line18(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line18.
  /// </summary>
  public FnB717Line18(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.Create.YearMonth = import.StatsReport.YearMonth.GetValueOrDefault();
    local.Create.FirstRunNumber =
      import.StatsReport.FirstRunNumber.GetValueOrDefault();
    local.Create.LineNumber = 18;
    local.LineNumber.Count = 18;
    local.Filter.Timestamp = AddDays(import.ReportEndDate.Timestamp, -30);

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "18 "))
    {
      local.RestartCashReceiptSourceType.SystemGeneratedIdentifier =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 4, 3));
      local.RestartCashReceiptEvent.SystemGeneratedIdentifier =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 7, 9));
      local.RestartCashReceiptType.SystemGeneratedIdentifier =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 16, 3));
      local.RestartCashReceiptDetail.SequentialIdentifier =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 19, 4));
    }

    foreach(var item in ReadCashReceiptTypeCashReceiptEventCashReceiptSourceType())
      
    {
      ++local.CommitCnt.Count;

      if (entities.CashReceiptType.SystemGeneratedIdentifier == 2 || entities
        .CashReceiptType.SystemGeneratedIdentifier == 7 || entities
        .CashReceiptType.SystemGeneratedIdentifier == 8 || entities
        .CashReceiptType.SystemGeneratedIdentifier == 11 || entities
        .CashReceiptType.SystemGeneratedIdentifier == 13 || entities
        .CashReceiptType.SystemGeneratedIdentifier == 14)
      {
      }
      else
      {
        continue;
      }

      if (local.RestartCashReceiptEvent.SystemGeneratedIdentifier > 0 && entities
        .CashReceiptSourceType.SystemGeneratedIdentifier == local
        .RestartCashReceiptSourceType.SystemGeneratedIdentifier)
      {
        if (entities.CashReceiptEvent.SystemGeneratedIdentifier < local
          .RestartCashReceiptEvent.SystemGeneratedIdentifier)
        {
          continue;
        }
        else if (entities.CashReceiptEvent.SystemGeneratedIdentifier > local
          .RestartCashReceiptEvent.SystemGeneratedIdentifier)
        {
          goto Test;
        }

        if (entities.CashReceiptType.SystemGeneratedIdentifier < local
          .RestartCashReceiptType.SystemGeneratedIdentifier)
        {
          continue;
        }
        else if (entities.CashReceiptType.SystemGeneratedIdentifier > local
          .RestartCashReceiptType.SystemGeneratedIdentifier)
        {
          goto Test;
        }

        if (entities.Starved.SequentialIdentifier <= local
          .RestartCashReceiptDetail.SequentialIdentifier)
        {
          continue;
        }
      }

Test:

      if (!ReadCashReceiptDetail())
      {
        continue;
      }

      if (IsEmpty(entities.CashReceiptDetail.CourtOrderNumber) && IsEmpty
        (entities.CashReceiptDetail.ObligorPersonNumber))
      {
        continue;
      }

      if (AsChar(entities.CashReceiptDetail.AdjustmentInd) == 'Y')
      {
        continue;
      }

      local.CaseFoundForCrd.Flag = "N";

      // ---------------------------------------
      // First, look for case using court order.
      // ---------------------------------------
      if (!IsEmpty(entities.CashReceiptDetail.CourtOrderNumber))
      {
        foreach(var item1 in ReadLegalActionCsePersonCsePersonCase())
        {
          local.CaseFoundForCrd.Flag = "Y";

          if (Equal(entities.Case1.Number, local.Prev.Number))
          {
            continue;
          }

          local.Prev.Number = entities.Case1.Number;
          UseFnB717CheckIfCaseIsCounted();

          if (AsChar(local.CaseAlreadyCounted.Flag) == 'Y')
          {
            continue;
          }

          if (!ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider1())
          {
            continue;
          }

          if (AsChar(entities.Case1.Status) == 'C' && Lt
            (entities.CaseAssignment.DiscontinueDate, import.ReportEndDate.Date))
            
          {
            local.CaseClosureDate.Date =
              entities.CaseAssignment.DiscontinueDate;
            UseFnBuildTimestampFrmDateTime();
            local.CaseClosureDate.Timestamp =
              AddHours(AddDays(local.CaseClosureDate.Timestamp, 1), 7);
            UseFnB717ProgHierarchy4Case2();
          }
          else
          {
            UseFnB717ProgHierarchy4Case1();
          }

          UseFnB717CreateOrUpdtStatsRpt();
          local.Create.CaseNumber = entities.Case1.Number;
          local.Create.CaseWrkRole = entities.OfficeServiceProvider.RoleCode;
          local.Create.ServicePrvdrId =
            entities.ServiceProvider.SystemGeneratedId;
          local.Create.OfficeId = entities.Office.SystemGeneratedId;
          local.Create.ProgramType = local.Program.Code;
          local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
          local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
          local.Create.CourtOrderNumber = entities.LegalAction.StandardNumber;
          UseFnB717GetSupervisorSp();
          local.Create.ParentId = local.Sup.SystemGeneratedId;
          UseFnB717CreateStatsVerifi();
        }
      }

      // ---------------------------------------
      // Either court order is blank or invalid.
      // Look for case using Obligor #.
      // ---------------------------------------
      if (AsChar(local.CaseFoundForCrd.Flag) == 'N')
      {
        foreach(var item1 in ReadCsePersonCase())
        {
          if (Equal(entities.Case1.Number, local.Prev.Number))
          {
            continue;
          }

          local.Prev.Number = entities.Case1.Number;
          UseFnB717CheckIfCaseIsCounted();

          if (AsChar(local.CaseAlreadyCounted.Flag) == 'Y')
          {
            continue;
          }

          if (!ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider2())
          {
            continue;
          }

          if (AsChar(entities.Case1.Status) == 'C' && Lt
            (entities.CaseAssignment.DiscontinueDate, import.ReportEndDate.Date))
            
          {
            local.CaseClosureDate.Date =
              entities.CaseAssignment.DiscontinueDate;
            UseFnBuildTimestampFrmDateTime();
            local.CaseClosureDate.Timestamp =
              AddHours(AddDays(local.CaseClosureDate.Timestamp, 1), 7);
            UseFnB717ProgHierarchy4Case2();
          }
          else
          {
            UseFnB717ProgHierarchy4Case1();
          }

          UseFnB717CreateOrUpdtStatsRpt();
          local.Create.CaseNumber = entities.Case1.Number;
          local.Create.CaseWrkRole = entities.OfficeServiceProvider.RoleCode;
          local.Create.ServicePrvdrId =
            entities.ServiceProvider.SystemGeneratedId;
          local.Create.OfficeId = entities.Office.SystemGeneratedId;
          local.Create.ProgramType = local.Program.Code;
          local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
          local.Create.SuppPersonNumber = "";
          local.Create.CourtOrderNumber = "";
          UseFnB717GetSupervisorSp();
          local.Create.ParentId = local.Sup.SystemGeneratedId;
          UseFnB717CreateStatsVerifi();
        }
      }

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "18 " + NumberToString
          (entities.CashReceiptSourceType.SystemGeneratedIdentifier, 13, 3);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.CashReceiptEvent.SystemGeneratedIdentifier, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.CashReceiptType.SystemGeneratedIdentifier, 13, 3);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Error.LineNumber = 18;
          UseFnB717WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }
    }

    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "27 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Error.LineNumber = 18;
      UseFnB717WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveStatsReport(StatsReport source, StatsReport target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
  }

  private static void MoveStatsVerifi(StatsVerifi source, StatsVerifi target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
    target.LineNumber = source.LineNumber;
    target.ProgramType = source.ProgramType;
    target.ServicePrvdrId = source.ServicePrvdrId;
    target.OfficeId = source.OfficeId;
    target.CaseWrkRole = source.CaseWrkRole;
    target.ParentId = source.ParentId;
    target.ChiefId = source.ChiefId;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
  }

  private void UseFnB717CheckIfCaseIsCounted()
  {
    var useImport = new FnB717CheckIfCaseIsCounted.Import();
    var useExport = new FnB717CheckIfCaseIsCounted.Export();

    useImport.Case1.Number = entities.Case1.Number;
    MoveStatsReport(import.StatsReport, useImport.StatsReport);
    useImport.LineNumber.Count = local.LineNumber.Count;

    Call(FnB717CheckIfCaseIsCounted.Execute, useImport, useExport);

    local.CaseAlreadyCounted.Flag = useExport.CaseAlreadyCounted.Flag;
  }

  private void UseFnB717CreateOrUpdtStatsRpt()
  {
    var useImport = new FnB717CreateOrUpdtStatsRpt.Import();
    var useExport = new FnB717CreateOrUpdtStatsRpt.Export();

    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;
    MoveStatsReport(import.StatsReport, useImport.StatsReport);
    useImport.LineNumber.Count = local.LineNumber.Count;
    MoveProgram(local.Program, useImport.Program);

    Call(FnB717CreateOrUpdtStatsRpt.Execute, useImport, useExport);
  }

  private void UseFnB717CreateStatsVerifi()
  {
    var useImport = new FnB717CreateStatsVerifi.Import();
    var useExport = new FnB717CreateStatsVerifi.Export();

    MoveStatsVerifi(local.Create, useImport.StatsVerifi);

    Call(FnB717CreateStatsVerifi.Execute, useImport, useExport);
  }

  private void UseFnB717GetSupervisorSp()
  {
    var useImport = new FnB717GetSupervisorSp.Import();
    var useExport = new FnB717GetSupervisorSp.Export();

    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;

    Call(FnB717GetSupervisorSp.Execute, useImport, useExport);

    local.Sup.SystemGeneratedId = useExport.Sup.SystemGeneratedId;
  }

  private void UseFnB717ProgHierarchy4Case1()
  {
    var useImport = new FnB717ProgHierarchy4Case.Import();
    var useExport = new FnB717ProgHierarchy4Case.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717ProgHierarchy4Case.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseFnB717ProgHierarchy4Case2()
  {
    var useImport = new FnB717ProgHierarchy4Case.Import();
    var useExport = new FnB717ProgHierarchy4Case.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(local.CaseClosureDate, useImport.ReportEndDate);

    Call(FnB717ProgHierarchy4Case.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseFnB717WriteError()
  {
    var useImport = new FnB717WriteError.Import();
    var useExport = new FnB717WriteError.Export();

    useImport.Error.Assign(local.Error);

    Call(FnB717WriteError.Execute, useImport, useExport);
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = local.CaseClosureDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, local.CaseClosureDate);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider1()
  {
    entities.CaseAssignment.Populated = false;
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 10);
        entities.CaseAssignment.Populated = true;
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider2()
  {
    entities.CaseAssignment.Populated = false;
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 10);
        entities.CaseAssignment.Populated = true;
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(
      entities.CashReceiptDetailStatHistory.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.CashReceiptDetailStatHistory.CrdIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptDetailStatHistory.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptDetailStatHistory.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptDetailStatHistory.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptTypeCashReceiptEventCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptDetailStatHistory.Populated = false;
    entities.CashReceiptType.Populated = false;
    entities.Starved.Populated = false;

    return ReadEach("ReadCashReceiptTypeCashReceiptEventCashReceiptSourceType",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          local.Filter.Timestamp.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          local.RestartCashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.Starved.CstIdentifier = db.GetInt32(reader, 2);
        entities.Starved.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Starved.CrvIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 3);
        entities.Starved.CrtIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 4);
        entities.Starved.SequentialIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.Starved.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCase()
  {
    entities.Case1.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCsePersonCase",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          entities.CashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetNullableDate(
          command, "endDate", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.Populated = true;
        entities.ApCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionCsePersonCsePersonCase()
  {
    entities.Case1.Populated = false;
    entities.LegalAction.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadLegalActionCsePersonCsePersonCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo",
          entities.CashReceiptDetail.CourtOrderNumber ?? "");
        db.SetNullableDate(
          command, "endDate", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.ApCsePerson.Number = db.GetString(reader, 3);
        entities.ChCsePerson.Number = db.GetString(reader, 4);
        entities.Case1.Number = db.GetString(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.LegalAction.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChCsePerson.Populated = true;

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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
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

    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private StatsReport statsReport;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CaseFoundForCrd.
    /// </summary>
    [JsonPropertyName("caseFoundForCrd")]
    public Common CaseFoundForCrd
    {
      get => caseFoundForCrd ??= new();
      set => caseFoundForCrd = value;
    }

    /// <summary>
    /// A value of CaseAlreadyCounted.
    /// </summary>
    [JsonPropertyName("caseAlreadyCounted")]
    public Common CaseAlreadyCounted
    {
      get => caseAlreadyCounted ??= new();
      set => caseAlreadyCounted = value;
    }

    /// <summary>
    /// A value of LineNumber.
    /// </summary>
    [JsonPropertyName("lineNumber")]
    public Common LineNumber
    {
      get => lineNumber ??= new();
      set => lineNumber = value;
    }

    /// <summary>
    /// A value of RestartCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("restartCashReceiptDetail")]
    public CashReceiptDetail RestartCashReceiptDetail
    {
      get => restartCashReceiptDetail ??= new();
      set => restartCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of RestartCashReceiptType.
    /// </summary>
    [JsonPropertyName("restartCashReceiptType")]
    public CashReceiptType RestartCashReceiptType
    {
      get => restartCashReceiptType ??= new();
      set => restartCashReceiptType = value;
    }

    /// <summary>
    /// A value of RestartCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("restartCashReceiptSourceType")]
    public CashReceiptSourceType RestartCashReceiptSourceType
    {
      get => restartCashReceiptSourceType ??= new();
      set => restartCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of RestartCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("restartCashReceiptEvent")]
    public CashReceiptEvent RestartCashReceiptEvent
    {
      get => restartCashReceiptEvent ??= new();
      set => restartCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public DateWorkArea Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public StatsVerifi Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of Sup.
    /// </summary>
    [JsonPropertyName("sup")]
    public ServiceProvider Sup
    {
      get => sup ??= new();
      set => sup = value;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public StatsVerifi Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of CaseClosureDate.
    /// </summary>
    [JsonPropertyName("caseClosureDate")]
    public DateWorkArea CaseClosureDate
    {
      get => caseClosureDate ??= new();
      set => caseClosureDate = value;
    }

    private Common caseFoundForCrd;
    private Common caseAlreadyCounted;
    private Common lineNumber;
    private CashReceiptDetail restartCashReceiptDetail;
    private CashReceiptType restartCashReceiptType;
    private CashReceiptSourceType restartCashReceiptSourceType;
    private CashReceiptEvent restartCashReceiptEvent;
    private DateWorkArea filter;
    private Case1 prev;
    private Program program;
    private StatsVerifi create;
    private ServiceProvider sup;
    private ProgramCheckpointRestart programCheckpointRestart;
    private StatsVerifi error;
    private Common commitCnt;
    private DateWorkArea caseClosureDate;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
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
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("chLegalActionCaseRole")]
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Starved.
    /// </summary>
    [JsonPropertyName("starved")]
    public CashReceiptDetail Starved
    {
      get => starved ??= new();
      set => starved = value;
    }

    private Case1 case1;
    private LegalAction legalAction;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson apCsePerson;
    private CsePerson chCsePerson;
    private LegalActionCaseRole apLegalActionCaseRole;
    private CaseRole apCaseRole;
    private LegalActionCaseRole chLegalActionCaseRole;
    private CaseRole chCaseRole;
    private CaseAssignment caseAssignment;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private CashReceiptDetail starved;
  }
#endregion
}
