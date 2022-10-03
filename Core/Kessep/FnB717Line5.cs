// Program: FN_B717_LINE_5, ID: 373364988, model: 746.
// Short name: SWE03031
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_5.
/// </summary>
[Serializable]
public partial class FnB717Line5: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_5 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line5(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line5.
  /// </summary>
  public FnB717Line5(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "05 "))
    {
      local.RestartOffice.SystemGeneratedId =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 4, 4));
      local.RestartServiceProvider.SystemGeneratedId =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 8, 5));
    }

    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.Create.YearMonth = import.StatsReport.YearMonth.GetValueOrDefault();
    local.Create.FirstRunNumber =
      import.StatsReport.FirstRunNumber.GetValueOrDefault();
    local.Create.LineNumber = 5;

    foreach(var item in ReadStatsReport3())
    {
      local.Create.ServicePrvdrId = entities.Ln35.ServicePrvdrId;
      local.Create.OfficeId = entities.Ln35.OfficeId;
      local.Create.ParentId = entities.Ln35.ParentId;
      local.Create.CaseWrkRole = entities.Ln35.CaseWrkRole;
      local.Create.CaseEffDate = entities.Ln35.CaseEffDate;

      if (!ReadStatsReport1())
      {
        // -------------
        // Ok
        // -------------
      }

      if (Lt(0, entities.Ln35.Column1))
      {
        local.Create.Column1 =
          (long?)Math.Round((
            entities.Ln4.Column1.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column1).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column1 = 0;
      }

      if (Lt(0, entities.Ln35.Column2))
      {
        local.Create.Column2 =
          (long?)Math.Round((
            entities.Ln4.Column2.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column2).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column2 = 0;
      }

      if (Lt(0, entities.Ln35.Column3))
      {
        local.Create.Column3 =
          (long?)Math.Round((
            entities.Ln4.Column3.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column3).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column3 = 0;
      }

      if (Lt(0, entities.Ln35.Column4))
      {
        local.Create.Column4 =
          (long?)Math.Round((
            entities.Ln4.Column4.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column4).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column4 = 0;
      }

      if (Lt(0, entities.Ln35.Column5))
      {
        local.Create.Column5 =
          (long?)Math.Round((
            entities.Ln4.Column5.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column5).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column5 = 0;
      }

      if (Lt(0, entities.Ln35.Column6))
      {
        local.Create.Column6 =
          (long?)Math.Round((
            entities.Ln4.Column6.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column6).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column6 = 0;
      }

      if (Lt(0, entities.Ln35.Column7))
      {
        local.Create.Column7 =
          (long?)Math.Round((
            entities.Ln4.Column7.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column7).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column7 = 0;
      }

      if (Lt(0, entities.Ln35.Column8))
      {
        local.Create.Column8 =
          (long?)Math.Round((
            entities.Ln4.Column8.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column8).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column8 = 0;
      }

      if (Lt(0, entities.Ln35.Column9))
      {
        local.Create.Column9 =
          (long?)Math.Round((
            entities.Ln4.Column9.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column9).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column9 = 0;
      }

      if (Lt(0, entities.Ln35.Column10))
      {
        local.Create.Column10 =
          (long?)Math.Round((
            entities.Ln4.Column10.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column10).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column10 = 0;
      }

      if (Lt(0, entities.Ln35.Column11))
      {
        local.Create.Column11 =
          (long?)Math.Round((
            entities.Ln4.Column11.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column11).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column11 = 0;
      }

      if (Lt(0, entities.Ln35.Column12))
      {
        local.Create.Column12 =
          (long?)Math.Round((
            entities.Ln4.Column12.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column12).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column12 = 0;
      }

      if (Lt(0, entities.Ln35.Column13))
      {
        local.Create.Column13 =
          (long?)Math.Round((
            entities.Ln4.Column13.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln35.Column13).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column13 = 0;
      }

      if (ReadStatsReport2())
      {
        try
        {
          UpdateStatsReport();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_STATS_REPORT_NU";
              export.Abort.Flag = "Y";

              return;
            case ErrorCode.PermittedValueViolation:
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
        do
        {
          try
          {
            CreateStatsReport();

            break;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ++local.Retry.Count;

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
        while(local.Retry.Count < 5);
      }

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "05 " + NumberToString
          (entities.Ln35.OfficeId.GetValueOrDefault(), 12, 4);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.Ln35.ServicePrvdrId.GetValueOrDefault(), 11, 5);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Error.LineNumber = 5;
          local.Error.OfficeId = entities.Ln35.OfficeId;
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
    local.ProgramCheckpointRestart.RestartInfo = "13 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Error.LineNumber = 5;
      UseFnB717WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
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

  private void CreateStatsReport()
  {
    var yearMonth = local.Create.YearMonth.GetValueOrDefault();
    var firstRunNumber = local.Create.FirstRunNumber.GetValueOrDefault();
    var lineNumber = local.Create.LineNumber.GetValueOrDefault();
    var createdTimestamp = Now();
    var servicePrvdrId = local.Create.ServicePrvdrId.GetValueOrDefault();
    var officeId = local.Create.OfficeId.GetValueOrDefault();
    var caseWrkRole = local.Create.CaseWrkRole ?? "";
    var caseEffDate = local.Create.CaseEffDate;
    var parentId = local.Create.ParentId.GetValueOrDefault();
    var column1 = local.Create.Column1.GetValueOrDefault();
    var column2 = local.Create.Column2.GetValueOrDefault();
    var column3 = local.Create.Column3.GetValueOrDefault();
    var column4 = local.Create.Column4.GetValueOrDefault();
    var column5 = local.Create.Column5.GetValueOrDefault();
    var column6 = local.Create.Column6.GetValueOrDefault();
    var column7 = local.Create.Column7.GetValueOrDefault();
    var column8 = local.Create.Column8.GetValueOrDefault();
    var column9 = local.Create.Column9.GetValueOrDefault();
    var column10 = local.Create.Column10.GetValueOrDefault();
    var column11 = local.Create.Column11.GetValueOrDefault();
    var column12 = local.Create.Column12.GetValueOrDefault();
    var column13 = local.Create.Column13.GetValueOrDefault();

    entities.Ln5.Populated = false;
    Update("CreateStatsReport",
      (db, command) =>
      {
        db.SetNullableInt32(command, "yearMonth", yearMonth);
        db.SetNullableInt32(command, "firstRunNumber", firstRunNumber);
        db.SetNullableInt32(command, "lineNumber", lineNumber);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "servicePrvdrId", servicePrvdrId);
        db.SetNullableInt32(command, "officeId", officeId);
        db.SetNullableString(command, "caseWrkRole", caseWrkRole);
        db.SetNullableDate(command, "caseEffDate", caseEffDate);
        db.SetNullableInt32(command, "parentId", parentId);
        db.SetNullableInt32(command, "chiefId", 0);
        db.SetNullableInt64(command, "column1", column1);
        db.SetNullableInt64(command, "column2", column2);
        db.SetNullableInt64(command, "column3", column3);
        db.SetNullableInt64(command, "column4", column4);
        db.SetNullableInt64(command, "column5", column5);
        db.SetNullableInt64(command, "column6", column6);
        db.SetNullableInt64(command, "column7", column7);
        db.SetNullableInt64(command, "column8", column8);
        db.SetNullableInt64(command, "column9", column9);
        db.SetNullableInt64(command, "column10", column10);
        db.SetNullableInt64(command, "column11", column11);
        db.SetNullableInt64(command, "column12", column12);
        db.SetNullableInt64(command, "column13", column13);
        db.SetNullableInt32(command, "column14", 0);
        db.SetNullableInt32(command, "column15", 0);
      });

    entities.Ln5.YearMonth = yearMonth;
    entities.Ln5.FirstRunNumber = firstRunNumber;
    entities.Ln5.LineNumber = lineNumber;
    entities.Ln5.CreatedTimestamp = createdTimestamp;
    entities.Ln5.ServicePrvdrId = servicePrvdrId;
    entities.Ln5.OfficeId = officeId;
    entities.Ln5.CaseWrkRole = caseWrkRole;
    entities.Ln5.CaseEffDate = caseEffDate;
    entities.Ln5.ParentId = parentId;
    entities.Ln5.ChiefId = 0;
    entities.Ln5.Column1 = column1;
    entities.Ln5.Column2 = column2;
    entities.Ln5.Column3 = column3;
    entities.Ln5.Column4 = column4;
    entities.Ln5.Column5 = column5;
    entities.Ln5.Column6 = column6;
    entities.Ln5.Column7 = column7;
    entities.Ln5.Column8 = column8;
    entities.Ln5.Column9 = column9;
    entities.Ln5.Column10 = column10;
    entities.Ln5.Column11 = column11;
    entities.Ln5.Column12 = column12;
    entities.Ln5.Column13 = column13;
    entities.Ln5.Column14 = 0;
    entities.Ln5.Column15 = 0;
    entities.Ln5.Populated = true;
  }

  private bool ReadStatsReport1()
  {
    entities.Ln4.Populated = false;

    return Read("ReadStatsReport1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "yearMonth",
          import.StatsReport.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "firstRunNumber",
          import.StatsReport.FirstRunNumber.GetValueOrDefault());
        db.SetNullableInt32(
          command, "servicePrvdrId",
          entities.Ln35.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId", entities.Ln35.OfficeId.GetValueOrDefault());
        db.SetNullableString(
          command, "caseWrkRole", entities.Ln35.CaseWrkRole ?? "");
        db.SetNullableDate(
          command, "caseEffDate",
          entities.Ln35.CaseEffDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ln4.YearMonth = db.GetNullableInt32(reader, 0);
        entities.Ln4.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.Ln4.LineNumber = db.GetNullableInt32(reader, 2);
        entities.Ln4.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ln4.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.Ln4.OfficeId = db.GetNullableInt32(reader, 5);
        entities.Ln4.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.Ln4.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.Ln4.ParentId = db.GetNullableInt32(reader, 8);
        entities.Ln4.ChiefId = db.GetNullableInt32(reader, 9);
        entities.Ln4.Column1 = db.GetNullableInt64(reader, 10);
        entities.Ln4.Column2 = db.GetNullableInt64(reader, 11);
        entities.Ln4.Column3 = db.GetNullableInt64(reader, 12);
        entities.Ln4.Column4 = db.GetNullableInt64(reader, 13);
        entities.Ln4.Column5 = db.GetNullableInt64(reader, 14);
        entities.Ln4.Column6 = db.GetNullableInt64(reader, 15);
        entities.Ln4.Column7 = db.GetNullableInt64(reader, 16);
        entities.Ln4.Column8 = db.GetNullableInt64(reader, 17);
        entities.Ln4.Column9 = db.GetNullableInt64(reader, 18);
        entities.Ln4.Column10 = db.GetNullableInt64(reader, 19);
        entities.Ln4.Column11 = db.GetNullableInt64(reader, 20);
        entities.Ln4.Column12 = db.GetNullableInt64(reader, 21);
        entities.Ln4.Column13 = db.GetNullableInt64(reader, 22);
        entities.Ln4.Column14 = db.GetNullableInt64(reader, 23);
        entities.Ln4.Column15 = db.GetNullableInt64(reader, 24);
        entities.Ln4.Populated = true;
      });
  }

  private bool ReadStatsReport2()
  {
    entities.Ln5.Populated = false;

    return Read("ReadStatsReport2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "yearMonth",
          import.StatsReport.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "firstRunNumber",
          import.StatsReport.FirstRunNumber.GetValueOrDefault());
        db.SetNullableInt32(
          command, "servicePrvdrId",
          entities.Ln35.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId", entities.Ln35.OfficeId.GetValueOrDefault());
        db.SetNullableString(
          command, "caseWrkRole", entities.Ln35.CaseWrkRole ?? "");
        db.SetNullableDate(
          command, "caseEffDate",
          entities.Ln35.CaseEffDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ln5.YearMonth = db.GetNullableInt32(reader, 0);
        entities.Ln5.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.Ln5.LineNumber = db.GetNullableInt32(reader, 2);
        entities.Ln5.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ln5.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.Ln5.OfficeId = db.GetNullableInt32(reader, 5);
        entities.Ln5.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.Ln5.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.Ln5.ParentId = db.GetNullableInt32(reader, 8);
        entities.Ln5.ChiefId = db.GetNullableInt32(reader, 9);
        entities.Ln5.Column1 = db.GetNullableInt64(reader, 10);
        entities.Ln5.Column2 = db.GetNullableInt64(reader, 11);
        entities.Ln5.Column3 = db.GetNullableInt64(reader, 12);
        entities.Ln5.Column4 = db.GetNullableInt64(reader, 13);
        entities.Ln5.Column5 = db.GetNullableInt64(reader, 14);
        entities.Ln5.Column6 = db.GetNullableInt64(reader, 15);
        entities.Ln5.Column7 = db.GetNullableInt64(reader, 16);
        entities.Ln5.Column8 = db.GetNullableInt64(reader, 17);
        entities.Ln5.Column9 = db.GetNullableInt64(reader, 18);
        entities.Ln5.Column10 = db.GetNullableInt64(reader, 19);
        entities.Ln5.Column11 = db.GetNullableInt64(reader, 20);
        entities.Ln5.Column12 = db.GetNullableInt64(reader, 21);
        entities.Ln5.Column13 = db.GetNullableInt64(reader, 22);
        entities.Ln5.Column14 = db.GetNullableInt64(reader, 23);
        entities.Ln5.Column15 = db.GetNullableInt64(reader, 24);
        entities.Ln5.Populated = true;
      });
  }

  private IEnumerable<bool> ReadStatsReport3()
  {
    entities.Ln35.Populated = false;

    return ReadEach("ReadStatsReport3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "yearMonth",
          import.StatsReport.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "firstRunNumber",
          import.StatsReport.FirstRunNumber.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId", local.RestartOffice.SystemGeneratedId);
        db.SetNullableInt32(
          command, "servicePrvdrId",
          local.RestartServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Ln35.YearMonth = db.GetNullableInt32(reader, 0);
        entities.Ln35.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.Ln35.LineNumber = db.GetNullableInt32(reader, 2);
        entities.Ln35.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ln35.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.Ln35.OfficeId = db.GetNullableInt32(reader, 5);
        entities.Ln35.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.Ln35.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.Ln35.ParentId = db.GetNullableInt32(reader, 8);
        entities.Ln35.ChiefId = db.GetNullableInt32(reader, 9);
        entities.Ln35.Column1 = db.GetNullableInt64(reader, 10);
        entities.Ln35.Column2 = db.GetNullableInt64(reader, 11);
        entities.Ln35.Column3 = db.GetNullableInt64(reader, 12);
        entities.Ln35.Column4 = db.GetNullableInt64(reader, 13);
        entities.Ln35.Column5 = db.GetNullableInt64(reader, 14);
        entities.Ln35.Column6 = db.GetNullableInt64(reader, 15);
        entities.Ln35.Column7 = db.GetNullableInt64(reader, 16);
        entities.Ln35.Column8 = db.GetNullableInt64(reader, 17);
        entities.Ln35.Column9 = db.GetNullableInt64(reader, 18);
        entities.Ln35.Column10 = db.GetNullableInt64(reader, 19);
        entities.Ln35.Column11 = db.GetNullableInt64(reader, 20);
        entities.Ln35.Column12 = db.GetNullableInt64(reader, 21);
        entities.Ln35.Column13 = db.GetNullableInt64(reader, 22);
        entities.Ln35.Column14 = db.GetNullableInt64(reader, 23);
        entities.Ln35.Column15 = db.GetNullableInt64(reader, 24);
        entities.Ln35.Populated = true;

        return true;
      });
  }

  private void UpdateStatsReport()
  {
    var column1 = local.Create.Column1.GetValueOrDefault();
    var column2 = local.Create.Column2.GetValueOrDefault();
    var column3 = local.Create.Column3.GetValueOrDefault();
    var column4 = local.Create.Column4.GetValueOrDefault();
    var column5 = local.Create.Column5.GetValueOrDefault();
    var column6 = local.Create.Column6.GetValueOrDefault();
    var column7 = local.Create.Column7.GetValueOrDefault();
    var column8 = local.Create.Column8.GetValueOrDefault();
    var column9 = local.Create.Column9.GetValueOrDefault();
    var column10 = local.Create.Column10.GetValueOrDefault();
    var column11 = local.Create.Column11.GetValueOrDefault();
    var column12 = local.Create.Column12.GetValueOrDefault();
    var column13 = local.Create.Column13.GetValueOrDefault();

    entities.Ln5.Populated = false;
    Update("UpdateStatsReport",
      (db, command) =>
      {
        db.SetNullableInt64(command, "column1", column1);
        db.SetNullableInt64(command, "column2", column2);
        db.SetNullableInt64(command, "column3", column3);
        db.SetNullableInt64(command, "column4", column4);
        db.SetNullableInt64(command, "column5", column5);
        db.SetNullableInt64(command, "column6", column6);
        db.SetNullableInt64(command, "column7", column7);
        db.SetNullableInt64(command, "column8", column8);
        db.SetNullableInt64(command, "column9", column9);
        db.SetNullableInt64(command, "column10", column10);
        db.SetNullableInt64(command, "column11", column11);
        db.SetNullableInt64(command, "column12", column12);
        db.SetNullableInt64(command, "column13", column13);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ln5.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ln5.Column1 = column1;
    entities.Ln5.Column2 = column2;
    entities.Ln5.Column3 = column3;
    entities.Ln5.Column4 = column4;
    entities.Ln5.Column5 = column5;
    entities.Ln5.Column6 = column6;
    entities.Ln5.Column7 = column7;
    entities.Ln5.Column8 = column8;
    entities.Ln5.Column9 = column9;
    entities.Ln5.Column10 = column10;
    entities.Ln5.Column11 = column11;
    entities.Ln5.Column12 = column12;
    entities.Ln5.Column13 = column13;
    entities.Ln5.Populated = true;
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
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public StatsReport Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of Retry.
    /// </summary>
    [JsonPropertyName("retry")]
    public Common Retry
    {
      get => retry ??= new();
      set => retry = value;
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
    /// A value of RestartOffice.
    /// </summary>
    [JsonPropertyName("restartOffice")]
    public Office RestartOffice
    {
      get => restartOffice ??= new();
      set => restartOffice = value;
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

    private StatsReport create;
    private Common retry;
    private ProgramCheckpointRestart programCheckpointRestart;
    private StatsVerifi error;
    private Common commitCnt;
    private Office restartOffice;
    private ServiceProvider restartServiceProvider;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ln5.
    /// </summary>
    [JsonPropertyName("ln5")]
    public StatsReport Ln5
    {
      get => ln5 ??= new();
      set => ln5 = value;
    }

    /// <summary>
    /// A value of Ln4.
    /// </summary>
    [JsonPropertyName("ln4")]
    public StatsReport Ln4
    {
      get => ln4 ??= new();
      set => ln4 = value;
    }

    /// <summary>
    /// A value of Ln35.
    /// </summary>
    [JsonPropertyName("ln35")]
    public StatsReport Ln35
    {
      get => ln35 ??= new();
      set => ln35 = value;
    }

    private StatsReport ln5;
    private StatsReport ln4;
    private StatsReport ln35;
  }
#endregion
}
