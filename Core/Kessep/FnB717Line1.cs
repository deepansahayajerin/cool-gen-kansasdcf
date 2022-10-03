// Program: FN_B717_LINE_1, ID: 373353966, model: 746.
// Short name: SWE03018
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_1.
/// </summary>
[Serializable]
public partial class FnB717Line1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_1 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line1.
  /// </summary>
  public FnB717Line1(IContext context, Import import, Export export):
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

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.RestartOffice.SystemGeneratedId =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 4, 4));
      local.RestartServiceProvider.SystemGeneratedId =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 8, 5));
      local.RestartCase.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 13, 10);
    }

    // ------------------------------------------
    // Read open cases as of report end date.
    // ------------------------------------------
    foreach(var item in ReadCaseCaseAssignmentOfficeOfficeServiceProvider())
    {
      if (Equal(entities.Case1.Number, local.PrevCase.Number))
      {
        continue;
      }

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() || entities
        .ServiceProvider.SystemGeneratedId != local
        .PrevServiceProvider.SystemGeneratedId)
      {
        UseFnB717UpdateCounts();
        UseFnB717ClearViews();
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "01 " + NumberToString
          (local.PrevOffice.SystemGeneratedId, 12, 4);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.PrevServiceProvider.SystemGeneratedId, 11, 5);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + local
          .PrevCase.Number;
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Error.LineNumber = 1;
          local.Error.CaseNumber = entities.Case1.Number;
          local.Error.OfficeId = entities.Office.SystemGeneratedId;
          UseFnB717WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }

      local.PrevCase.Number = entities.Case1.Number;
      local.PrevOffice.SystemGeneratedId = entities.Office.SystemGeneratedId;
      local.PrevServiceProvider.SystemGeneratedId =
        entities.ServiceProvider.SystemGeneratedId;
      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
        local.PrevOfficeServiceProvider);
      UseFnB717ProgHierarchy4Case();
      local.Subscript.Count = 1;
      UseFnB717InflateGv();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.Create.LineNumber = 1;
        local.Create.CaseNumber = entities.Case1.Number;
        local.Create.CaseWrkRole = entities.OfficeServiceProvider.RoleCode;
        local.Create.ServicePrvdrId =
          entities.ServiceProvider.SystemGeneratedId;
        local.Create.OfficeId = entities.Office.SystemGeneratedId;
        local.Create.ProgramType = local.Program.Code;
        UseFnB717GetSupervisorSp();
        local.Create.ParentId = local.Sup.SystemGeneratedId;
        UseFnB717CreateStatsVerifi();
      }

      UseFnB717Line791014();
      UseFnB717Line24835();

      if (AsChar(local.AccrInstrOpen.Flag) != 'Y')
      {
        UseFnB717Line4();
      }

      UseFnB717Line111214();
      UseFnB717Line16();
      UseFnB717Line1920();
      UseFnB717Line212223();
      UseFnB717Line2425();
      UseFnB717Line2();
      UseFnB717Line28();
      UseFnB717Line2931();
      UseFnB717Line3334();
      ++local.CommitCnt.Count;
    }

    UseFnB717UpdateCounts();
    UseFnB717ClearViews();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "02 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Error.LineNumber = 1;
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

  private static void MoveGroup1(FnB717ClearViews.Export.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup2(FnB717Line2.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup3(FnB717Line28.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup4(FnB717Line2931.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup5(FnB717Line3334.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup6(FnB717InflateGv.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup7(Import.GroupGroup source,
    FnB717Line2.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup8(Import.GroupGroup source,
    FnB717UpdateCounts.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup9(Import.GroupGroup source,
    FnB717Line28.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup10(Import.GroupGroup source,
    FnB717Line2931.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup11(Import.GroupGroup source,
    FnB717Line3334.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup12(Import.GroupGroup source,
    FnB717InflateGv.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup13(Import.GroupGroup source,
    FnB717Line791014.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup14(Import.GroupGroup source,
    FnB717Line111214.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup15(Import.GroupGroup source,
    FnB717Line4.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup16(Import.GroupGroup source,
    FnB717Line24835.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup17(Import.GroupGroup source,
    FnB717Line1920.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup18(Import.GroupGroup source,
    FnB717Line212223.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup19(Import.GroupGroup source,
    FnB717Line16.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup20(Import.GroupGroup source,
    FnB717Line2425.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup21(FnB717Line791014.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup22(FnB717Line111214.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup23(FnB717Line4.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup24(FnB717Line24835.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup25(FnB717Line1920.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup26(FnB717Line212223.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup27(FnB717Line16.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup28(FnB717Line2425.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
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
  }

  private void UseFnB717ClearViews()
  {
    var useImport = new FnB717ClearViews.Import();
    var useExport = new FnB717ClearViews.Export();

    Call(FnB717ClearViews.Execute, useImport, useExport);

    useExport.Group.CopyTo(import.Group, MoveGroup1);
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

    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(FnB717GetSupervisorSp.Execute, useImport, useExport);

    local.Sup.SystemGeneratedId = useExport.Sup.SystemGeneratedId;
  }

  private void UseFnB717InflateGv()
  {
    var useImport = new FnB717InflateGv.Import();
    var useExport = new FnB717InflateGv.Export();

    import.Group.CopyTo(useImport.Group, MoveGroup12);
    MoveProgram(local.Program, useImport.Program);
    useImport.Subscript.Count = local.Subscript.Count;

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup6);
  }

  private void UseFnB717Line111214()
  {
    var useImport = new FnB717Line111214.Import();
    var useExport = new FnB717Line111214.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    MoveProgram(local.Program, useImport.Program);
    import.Group.CopyTo(useImport.Group, MoveGroup14);
    useImport.Create.Assign(local.Create);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;

    Call(FnB717Line111214.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup22);
  }

  private void UseFnB717Line16()
  {
    var useImport = new FnB717Line16.Import();
    var useExport = new FnB717Line16.Export();

    MoveProgram(local.Program, useImport.Program);
    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    import.Group.CopyTo(useImport.Group, MoveGroup19);
    useImport.Create.Assign(local.Create);

    Call(FnB717Line16.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup27);
  }

  private void UseFnB717Line1920()
  {
    var useImport = new FnB717Line1920.Import();
    var useExport = new FnB717Line1920.Export();

    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.ReportStartDate.Date = import.ReportStartDate.Date;
    useImport.Case1.Assign(entities.Case1);
    MoveProgram(local.Program, useImport.Program);
    import.Group.CopyTo(useImport.Group, MoveGroup17);
    useImport.Create.Assign(local.Create);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;

    Call(FnB717Line1920.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup25);
  }

  private void UseFnB717Line2()
  {
    var useImport = new FnB717Line2.Import();
    var useExport = new FnB717Line2.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    import.Group.CopyTo(useImport.Group, MoveGroup7);
    useImport.Create.Assign(local.Create);

    Call(FnB717Line2.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup2);
  }

  private void UseFnB717Line212223()
  {
    var useImport = new FnB717Line212223.Import();
    var useExport = new FnB717Line212223.Export();

    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.ReportStartDate.Date = import.ReportStartDate.Date;
    useImport.Case1.Assign(entities.Case1);
    MoveProgram(local.Program, useImport.Program);
    import.Group.CopyTo(useImport.Group, MoveGroup18);
    useImport.Create.Assign(local.Create);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;

    Call(FnB717Line212223.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup26);
  }

  private void UseFnB717Line2425()
  {
    var useImport = new FnB717Line2425.Import();
    var useExport = new FnB717Line2425.Export();

    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    useImport.Case1.Assign(entities.Case1);
    MoveProgram(local.Program, useImport.Program);
    import.Group.CopyTo(useImport.Group, MoveGroup20);
    useImport.Create.Assign(local.Create);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;

    Call(FnB717Line2425.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup28);
  }

  private void UseFnB717Line24835()
  {
    var useImport = new FnB717Line24835.Import();
    var useExport = new FnB717Line24835.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    MoveProgram(local.Program, useImport.Program);
    import.Group.CopyTo(useImport.Group, MoveGroup16);
    useImport.Create.Assign(local.Create);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;

    Call(FnB717Line24835.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup24);
    local.AccrInstrOpen.Flag = useExport.AccrInstOpen.Flag;
  }

  private void UseFnB717Line28()
  {
    var useImport = new FnB717Line28.Import();
    var useExport = new FnB717Line28.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    import.Group.CopyTo(useImport.Group, MoveGroup9);
    useImport.Create.Assign(local.Create);
    MoveProgram(local.Program, useImport.Program);

    Call(FnB717Line28.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup3);
  }

  private void UseFnB717Line2931()
  {
    var useImport = new FnB717Line2931.Import();
    var useExport = new FnB717Line2931.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    import.Group.CopyTo(useImport.Group, MoveGroup10);
    useImport.Create.Assign(local.Create);
    MoveProgram(local.Program, useImport.Program);

    Call(FnB717Line2931.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup4);
  }

  private void UseFnB717Line3334()
  {
    var useImport = new FnB717Line3334.Import();
    var useExport = new FnB717Line3334.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    import.Group.CopyTo(useImport.Group, MoveGroup11);
    useImport.Create.Assign(local.Create);
    MoveProgram(local.Program, useImport.Program);

    Call(FnB717Line3334.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup5);
  }

  private void UseFnB717Line4()
  {
    var useImport = new FnB717Line4.Import();
    var useExport = new FnB717Line4.Export();

    import.Group.CopyTo(useImport.Group, MoveGroup15);
    MoveProgram(local.Program, useImport.Program);
    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    useImport.Create.Assign(local.Create);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;

    Call(FnB717Line4.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup23);
  }

  private void UseFnB717Line791014()
  {
    var useImport = new FnB717Line791014.Import();
    var useExport = new FnB717Line791014.Export();

    useImport.Create.Assign(local.Create);
    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    import.Group.CopyTo(useImport.Group, MoveGroup13);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;

    Call(FnB717Line791014.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup21);
  }

  private void UseFnB717ProgHierarchy4Case()
  {
    var useImport = new FnB717ProgHierarchy4Case.Import();
    var useExport = new FnB717ProgHierarchy4Case.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717ProgHierarchy4Case.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseFnB717UpdateCounts()
  {
    var useImport = new FnB717UpdateCounts.Import();
    var useExport = new FnB717UpdateCounts.Export();

    MoveStatsReport(import.StatsReport, useImport.StatsReport);
    import.Group.CopyTo(useImport.Group, MoveGroup8);
    useImport.Office.SystemGeneratedId = local.PrevOffice.SystemGeneratedId;
    MoveOfficeServiceProvider(local.PrevOfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      local.PrevServiceProvider.SystemGeneratedId;

    Call(FnB717UpdateCounts.Execute, useImport, useExport);
  }

  private void UseFnB717WriteError()
  {
    var useImport = new FnB717WriteError.Import();
    var useExport = new FnB717WriteError.Export();

    useImport.Error.Assign(local.Error);

    Call(FnB717WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCaseCaseAssignmentOfficeOfficeServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseCaseAssignmentOfficeOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetInt32(command, "officeId1", local.RestartOffice.SystemGeneratedId);
          
        db.SetInt32(
          command, "spdId", local.RestartServiceProvider.SystemGeneratedId);
        db.SetString(command, "casNo", local.RestartCase.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId2", import.From.OfficeId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId3", import.To.OfficeId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 5);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 8);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = true;

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of StatsReport.
      /// </summary>
      [JsonPropertyName("statsReport")]
      public StatsReport StatsReport
      {
        get => statsReport ??= new();
        set => statsReport = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 35;

      private StatsReport statsReport;
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
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public StatsReport To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public StatsReport From
    {
      get => from ??= new();
      set => from = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private StatsReport statsReport;
    private StatsReport to;
    private StatsReport from;
    private DateWorkArea reportStartDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common displayInd;
    private DateWorkArea reportEndDate;
    private Array<GroupGroup> group;
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
    /// A value of Sup.
    /// </summary>
    [JsonPropertyName("sup")]
    public ServiceProvider Sup
    {
      get => sup ??= new();
      set => sup = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public StatsVerifi Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of PrevOffice.
    /// </summary>
    [JsonPropertyName("prevOffice")]
    public Office PrevOffice
    {
      get => prevOffice ??= new();
      set => prevOffice = value;
    }

    /// <summary>
    /// A value of PrevOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("prevOfficeServiceProvider")]
    public OfficeServiceProvider PrevOfficeServiceProvider
    {
      get => prevOfficeServiceProvider ??= new();
      set => prevOfficeServiceProvider = value;
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
    /// A value of PrevServiceProvider.
    /// </summary>
    [JsonPropertyName("prevServiceProvider")]
    public ServiceProvider PrevServiceProvider
    {
      get => prevServiceProvider ??= new();
      set => prevServiceProvider = value;
    }

    /// <summary>
    /// A value of Line11.
    /// </summary>
    [JsonPropertyName("line11")]
    public Common Line11
    {
      get => line11 ??= new();
      set => line11 = value;
    }

    /// <summary>
    /// A value of EmanCh.
    /// </summary>
    [JsonPropertyName("emanCh")]
    public Common EmanCh
    {
      get => emanCh ??= new();
      set => emanCh = value;
    }

    /// <summary>
    /// A value of Line10.
    /// </summary>
    [JsonPropertyName("line10")]
    public Common Line10
    {
      get => line10 ??= new();
      set => line10 = value;
    }

    /// <summary>
    /// A value of Line9.
    /// </summary>
    [JsonPropertyName("line9")]
    public Common Line9
    {
      get => line9 ??= new();
      set => line9 = value;
    }

    /// <summary>
    /// A value of Line7.
    /// </summary>
    [JsonPropertyName("line7")]
    public Common Line7
    {
      get => line7 ??= new();
      set => line7 = value;
    }

    /// <summary>
    /// A value of Line8.
    /// </summary>
    [JsonPropertyName("line8")]
    public Common Line8
    {
      get => line8 ??= new();
      set => line8 = value;
    }

    /// <summary>
    /// A value of Line6.
    /// </summary>
    [JsonPropertyName("line6")]
    public Common Line6
    {
      get => line6 ??= new();
      set => line6 = value;
    }

    /// <summary>
    /// A value of AccrInstrOpen.
    /// </summary>
    [JsonPropertyName("accrInstrOpen")]
    public Common AccrInstrOpen
    {
      get => accrInstrOpen ??= new();
      set => accrInstrOpen = value;
    }

    /// <summary>
    /// A value of RestartServiceProvider.
    /// </summary>
    [JsonPropertyName("restartServiceProvider")]
    public ServiceProvider RestartServiceProvider
    {
      get => restartServiceProvider ??= new();
      set => restartServiceProvider = value;
    }

    /// <summary>
    /// A value of RestartOffice.
    /// </summary>
    [JsonPropertyName("restartOffice")]
    public Office RestartOffice
    {
      get => restartOffice ??= new();
      set => restartOffice = value;
    }

    /// <summary>
    /// A value of RestartOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("restartOfficeServiceProvider")]
    public OfficeServiceProvider RestartOfficeServiceProvider
    {
      get => restartOfficeServiceProvider ??= new();
      set => restartOfficeServiceProvider = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of RestartCase.
    /// </summary>
    [JsonPropertyName("restartCase")]
    public Case1 RestartCase
    {
      get => restartCase ??= new();
      set => restartCase = value;
    }

    /// <summary>
    /// A value of PrevCase.
    /// </summary>
    [JsonPropertyName("prevCase")]
    public Case1 PrevCase
    {
      get => prevCase ??= new();
      set => prevCase = value;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    private ServiceProvider sup;
    private StatsVerifi null1;
    private StatsVerifi error;
    private Office prevOffice;
    private OfficeServiceProvider prevOfficeServiceProvider;
    private StatsVerifi create;
    private ServiceProvider prevServiceProvider;
    private Common line11;
    private Common emanCh;
    private Common line10;
    private Common line9;
    private Common line7;
    private Common line8;
    private Common line6;
    private Common accrInstrOpen;
    private ServiceProvider restartServiceProvider;
    private Office restartOffice;
    private OfficeServiceProvider restartOfficeServiceProvider;
    private Program program;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restartCase;
    private Case1 prevCase;
    private Common commitCnt;
    private Common subscript;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private Case1 case1;
    private CaseAssignment caseAssignment;
  }
#endregion
}
