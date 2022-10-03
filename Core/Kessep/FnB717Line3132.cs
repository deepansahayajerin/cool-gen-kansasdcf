// Program: FN_B717_LINE_31_32, ID: 373363796, model: 746.
// Short name: SWE03040
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_31_32.
/// </summary>
[Serializable]
public partial class FnB717Line3132: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_31_32 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line3132(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line3132.
  /// </summary>
  public FnB717Line3132(IContext context, Import import, Export export):
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
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "31 "))
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
    local.Create.LineNumber = 32;

    foreach(var item in ReadStatsReport3())
    {
      local.Create.ServicePrvdrId = entities.Ln31.ServicePrvdrId;
      local.Create.OfficeId = entities.Ln31.OfficeId;
      local.Create.ParentId = entities.Ln31.ParentId;
      local.Create.CaseWrkRole = entities.Ln31.CaseWrkRole;
      local.Create.CaseEffDate = entities.Ln31.CaseEffDate;

      if (!ReadStatsReport2())
      {
        // -------------
        // Ok
        // -------------
      }

      if (Lt(0, entities.Ln29.Column1))
      {
        local.Create.Column1 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column1 / (
            entities.Ln29.Column1.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column1 = 0;
      }

      if (Lt(0, entities.Ln29.Column2))
      {
        local.Create.Column2 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column2 / (
            entities.Ln29.Column2.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column2 = 0;
      }

      if (Lt(0, entities.Ln29.Column3))
      {
        local.Create.Column3 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column3 / (
            entities.Ln29.Column3.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column3 = 0;
      }

      if (Lt(0, entities.Ln29.Column4))
      {
        local.Create.Column4 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column4 / (
            entities.Ln29.Column4.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column4 = 0;
      }

      if (Lt(0, entities.Ln29.Column5))
      {
        local.Create.Column5 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column5 / (
            entities.Ln29.Column5.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column5 = 0;
      }

      if (Lt(0, entities.Ln29.Column6))
      {
        local.Create.Column6 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column6 / (
            entities.Ln29.Column6.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column6 = 0;
      }

      if (Lt(0, entities.Ln29.Column7))
      {
        local.Create.Column7 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column7 / (
            entities.Ln29.Column7.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column7 = 0;
      }

      if (Lt(0, entities.Ln29.Column8))
      {
        local.Create.Column8 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column8 / (
            entities.Ln29.Column8.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column8 = 0;
      }

      if (Lt(0, entities.Ln29.Column9))
      {
        local.Create.Column9 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column9 / (
            entities.Ln29.Column9.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column9 = 0;
      }

      if (Lt(0, entities.Ln29.Column10))
      {
        local.Create.Column10 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column10 / (
            entities.Ln29.Column10.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column10 = 0;
      }

      if (Lt(0, entities.Ln29.Column11))
      {
        local.Create.Column11 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column11 / (
            entities.Ln29.Column11.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column11 = 0;
      }

      if (Lt(0, entities.Ln29.Column12))
      {
        local.Create.Column12 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column12 / (
            entities.Ln29.Column12.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column12 = 0;
      }

      if (Lt(0, entities.Ln29.Column13))
      {
        local.Create.Column13 =
          (long?)Math.Round(((decimal?)entities.Ln31.Column13 / (
            entities.Ln29.Column13.GetValueOrDefault() * 100
          )).GetValueOrDefault(), MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column13 = 0;
      }

      if (ReadStatsReport1())
      {
        try
        {
          UpdateStatsReport2();
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

      try
      {
        UpdateStatsReport1();
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

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "31 " + NumberToString
          (entities.Ln31.OfficeId.GetValueOrDefault(), 12, 4);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.Ln31.ServicePrvdrId.GetValueOrDefault(), 11, 5);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Error.LineNumber = 31;
          local.Error.OfficeId = entities.Ln31.OfficeId;
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
    local.ProgramCheckpointRestart.RestartInfo = "33 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Error.LineNumber = 31;
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

    entities.Ln32.Populated = false;
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

    entities.Ln32.YearMonth = yearMonth;
    entities.Ln32.FirstRunNumber = firstRunNumber;
    entities.Ln32.LineNumber = lineNumber;
    entities.Ln32.CreatedTimestamp = createdTimestamp;
    entities.Ln32.ServicePrvdrId = servicePrvdrId;
    entities.Ln32.OfficeId = officeId;
    entities.Ln32.CaseWrkRole = caseWrkRole;
    entities.Ln32.CaseEffDate = caseEffDate;
    entities.Ln32.ParentId = parentId;
    entities.Ln32.ChiefId = 0;
    entities.Ln32.Column1 = column1;
    entities.Ln32.Column2 = column2;
    entities.Ln32.Column3 = column3;
    entities.Ln32.Column4 = column4;
    entities.Ln32.Column5 = column5;
    entities.Ln32.Column6 = column6;
    entities.Ln32.Column7 = column7;
    entities.Ln32.Column8 = column8;
    entities.Ln32.Column9 = column9;
    entities.Ln32.Column10 = column10;
    entities.Ln32.Column11 = column11;
    entities.Ln32.Column12 = column12;
    entities.Ln32.Column13 = column13;
    entities.Ln32.Column14 = 0;
    entities.Ln32.Column15 = 0;
    entities.Ln32.Populated = true;
  }

  private bool ReadStatsReport1()
  {
    entities.Ln32.Populated = false;

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
          entities.Ln31.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId", entities.Ln31.OfficeId.GetValueOrDefault());
        db.SetNullableString(
          command, "caseWrkRole", entities.Ln31.CaseWrkRole ?? "");
        db.SetNullableDate(
          command, "caseEffDate",
          entities.Ln31.CaseEffDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ln32.YearMonth = db.GetNullableInt32(reader, 0);
        entities.Ln32.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.Ln32.LineNumber = db.GetNullableInt32(reader, 2);
        entities.Ln32.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ln32.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.Ln32.OfficeId = db.GetNullableInt32(reader, 5);
        entities.Ln32.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.Ln32.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.Ln32.ParentId = db.GetNullableInt32(reader, 8);
        entities.Ln32.ChiefId = db.GetNullableInt32(reader, 9);
        entities.Ln32.Column1 = db.GetNullableInt64(reader, 10);
        entities.Ln32.Column2 = db.GetNullableInt64(reader, 11);
        entities.Ln32.Column3 = db.GetNullableInt64(reader, 12);
        entities.Ln32.Column4 = db.GetNullableInt64(reader, 13);
        entities.Ln32.Column5 = db.GetNullableInt64(reader, 14);
        entities.Ln32.Column6 = db.GetNullableInt64(reader, 15);
        entities.Ln32.Column7 = db.GetNullableInt64(reader, 16);
        entities.Ln32.Column8 = db.GetNullableInt64(reader, 17);
        entities.Ln32.Column9 = db.GetNullableInt64(reader, 18);
        entities.Ln32.Column10 = db.GetNullableInt64(reader, 19);
        entities.Ln32.Column11 = db.GetNullableInt64(reader, 20);
        entities.Ln32.Column12 = db.GetNullableInt64(reader, 21);
        entities.Ln32.Column13 = db.GetNullableInt64(reader, 22);
        entities.Ln32.Column14 = db.GetNullableInt64(reader, 23);
        entities.Ln32.Column15 = db.GetNullableInt64(reader, 24);
        entities.Ln32.Populated = true;
      });
  }

  private bool ReadStatsReport2()
  {
    entities.Ln29.Populated = false;

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
          entities.Ln31.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId", entities.Ln31.OfficeId.GetValueOrDefault());
        db.SetNullableString(
          command, "caseWrkRole", entities.Ln31.CaseWrkRole ?? "");
        db.SetNullableDate(
          command, "caseEffDate",
          entities.Ln31.CaseEffDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ln29.YearMonth = db.GetNullableInt32(reader, 0);
        entities.Ln29.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.Ln29.LineNumber = db.GetNullableInt32(reader, 2);
        entities.Ln29.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ln29.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.Ln29.OfficeId = db.GetNullableInt32(reader, 5);
        entities.Ln29.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.Ln29.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.Ln29.ParentId = db.GetNullableInt32(reader, 8);
        entities.Ln29.ChiefId = db.GetNullableInt32(reader, 9);
        entities.Ln29.Column1 = db.GetNullableInt64(reader, 10);
        entities.Ln29.Column2 = db.GetNullableInt64(reader, 11);
        entities.Ln29.Column3 = db.GetNullableInt64(reader, 12);
        entities.Ln29.Column4 = db.GetNullableInt64(reader, 13);
        entities.Ln29.Column5 = db.GetNullableInt64(reader, 14);
        entities.Ln29.Column6 = db.GetNullableInt64(reader, 15);
        entities.Ln29.Column7 = db.GetNullableInt64(reader, 16);
        entities.Ln29.Column8 = db.GetNullableInt64(reader, 17);
        entities.Ln29.Column9 = db.GetNullableInt64(reader, 18);
        entities.Ln29.Column10 = db.GetNullableInt64(reader, 19);
        entities.Ln29.Column11 = db.GetNullableInt64(reader, 20);
        entities.Ln29.Column12 = db.GetNullableInt64(reader, 21);
        entities.Ln29.Column13 = db.GetNullableInt64(reader, 22);
        entities.Ln29.Column14 = db.GetNullableInt64(reader, 23);
        entities.Ln29.Column15 = db.GetNullableInt64(reader, 24);
        entities.Ln29.Populated = true;
      });
  }

  private IEnumerable<bool> ReadStatsReport3()
  {
    entities.Ln31.Populated = false;

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
        entities.Ln31.YearMonth = db.GetNullableInt32(reader, 0);
        entities.Ln31.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.Ln31.LineNumber = db.GetNullableInt32(reader, 2);
        entities.Ln31.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ln31.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.Ln31.OfficeId = db.GetNullableInt32(reader, 5);
        entities.Ln31.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.Ln31.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.Ln31.ParentId = db.GetNullableInt32(reader, 8);
        entities.Ln31.ChiefId = db.GetNullableInt32(reader, 9);
        entities.Ln31.Column1 = db.GetNullableInt64(reader, 10);
        entities.Ln31.Column2 = db.GetNullableInt64(reader, 11);
        entities.Ln31.Column3 = db.GetNullableInt64(reader, 12);
        entities.Ln31.Column4 = db.GetNullableInt64(reader, 13);
        entities.Ln31.Column5 = db.GetNullableInt64(reader, 14);
        entities.Ln31.Column6 = db.GetNullableInt64(reader, 15);
        entities.Ln31.Column7 = db.GetNullableInt64(reader, 16);
        entities.Ln31.Column8 = db.GetNullableInt64(reader, 17);
        entities.Ln31.Column9 = db.GetNullableInt64(reader, 18);
        entities.Ln31.Column10 = db.GetNullableInt64(reader, 19);
        entities.Ln31.Column11 = db.GetNullableInt64(reader, 20);
        entities.Ln31.Column12 = db.GetNullableInt64(reader, 21);
        entities.Ln31.Column13 = db.GetNullableInt64(reader, 22);
        entities.Ln31.Column14 = db.GetNullableInt64(reader, 23);
        entities.Ln31.Column15 = db.GetNullableInt64(reader, 24);
        entities.Ln31.Populated = true;

        return true;
      });
  }

  private void UpdateStatsReport1()
  {
    var column1 = entities.Ln31.Column1.GetValueOrDefault() / 100;
    var column2 = entities.Ln31.Column2.GetValueOrDefault() / 100;
    var column3 = entities.Ln31.Column3.GetValueOrDefault() / 100;
    var column4 = entities.Ln31.Column4.GetValueOrDefault() / 100;
    var column5 = entities.Ln31.Column5.GetValueOrDefault() / 100;
    var column6 = entities.Ln31.Column6.GetValueOrDefault() / 100;
    var column7 = entities.Ln31.Column7.GetValueOrDefault() / 100;
    var column8 = entities.Ln31.Column8.GetValueOrDefault() / 100;
    var column9 = entities.Ln31.Column9.GetValueOrDefault() / 100;
    var column10 = entities.Ln31.Column10.GetValueOrDefault() / 100;
    var column11 = entities.Ln31.Column11.GetValueOrDefault() / 100;
    var column12 = entities.Ln31.Column12.GetValueOrDefault() / 100;
    var column13 = entities.Ln31.Column13.GetValueOrDefault() / 100;
    var column14 = entities.Ln31.Column14.GetValueOrDefault() / 100;
    var column15 = entities.Ln31.Column15.GetValueOrDefault() / 100;

    entities.Ln31.Populated = false;
    Update("UpdateStatsReport1",
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
        db.SetNullableInt64(command, "column14", column14);
        db.SetNullableInt64(command, "column15", column15);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ln31.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ln31.Column1 = column1;
    entities.Ln31.Column2 = column2;
    entities.Ln31.Column3 = column3;
    entities.Ln31.Column4 = column4;
    entities.Ln31.Column5 = column5;
    entities.Ln31.Column6 = column6;
    entities.Ln31.Column7 = column7;
    entities.Ln31.Column8 = column8;
    entities.Ln31.Column9 = column9;
    entities.Ln31.Column10 = column10;
    entities.Ln31.Column11 = column11;
    entities.Ln31.Column12 = column12;
    entities.Ln31.Column13 = column13;
    entities.Ln31.Column14 = column14;
    entities.Ln31.Column15 = column15;
    entities.Ln31.Populated = true;
  }

  private void UpdateStatsReport2()
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

    entities.Ln32.Populated = false;
    Update("UpdateStatsReport2",
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
          entities.Ln32.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ln32.Column1 = column1;
    entities.Ln32.Column2 = column2;
    entities.Ln32.Column3 = column3;
    entities.Ln32.Column4 = column4;
    entities.Ln32.Column5 = column5;
    entities.Ln32.Column6 = column6;
    entities.Ln32.Column7 = column7;
    entities.Ln32.Column8 = column8;
    entities.Ln32.Column9 = column9;
    entities.Ln32.Column10 = column10;
    entities.Ln32.Column11 = column11;
    entities.Ln32.Column12 = column12;
    entities.Ln32.Column13 = column13;
    entities.Ln32.Populated = true;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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

    private ProgramCheckpointRestart programCheckpointRestart;
    private StatsReport statsReport;
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
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
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

    private Office restartOffice;
    private ServiceProvider restartServiceProvider;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common commitCnt;
    private StatsVerifi error;
    private StatsReport create;
    private Common retry;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ln31.
    /// </summary>
    [JsonPropertyName("ln31")]
    public StatsReport Ln31
    {
      get => ln31 ??= new();
      set => ln31 = value;
    }

    /// <summary>
    /// A value of Ln29.
    /// </summary>
    [JsonPropertyName("ln29")]
    public StatsReport Ln29
    {
      get => ln29 ??= new();
      set => ln29 = value;
    }

    /// <summary>
    /// A value of Ln32.
    /// </summary>
    [JsonPropertyName("ln32")]
    public StatsReport Ln32
    {
      get => ln32 ??= new();
      set => ln32 = value;
    }

    private StatsReport ln31;
    private StatsReport ln29;
    private StatsReport ln32;
  }
#endregion
}
