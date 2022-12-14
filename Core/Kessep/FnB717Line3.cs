// Program: FN_B717_LINE_3, ID: 373363034, model: 746.
// Short name: SWE03030
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_3.
/// </summary>
[Serializable]
public partial class FnB717Line3: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_3 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line3(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line3.
  /// </summary>
  public FnB717Line3(IContext context, Import import, Export export):
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
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "03 "))
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
    local.Create.LineNumber = 3;

    foreach(var item in ReadStatsReport3())
    {
      local.Create.ServicePrvdrId = entities.Ln1.ServicePrvdrId;
      local.Create.OfficeId = entities.Ln1.OfficeId;
      local.Create.ParentId = entities.Ln1.ParentId;
      local.Create.CaseWrkRole = entities.Ln1.CaseWrkRole;
      local.Create.CaseEffDate = entities.Ln1.CaseEffDate;

      if (!ReadStatsReport2())
      {
        // -------------
        // Ok
        // -------------
      }

      if (Lt(0, entities.Ln1.Column1))
      {
        local.Create.Column1 =
          (long?)Math.Round((
            entities.Ln2.Column1.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column1).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column1 = 0;
      }

      if (Lt(0, entities.Ln1.Column2))
      {
        local.Create.Column2 =
          (long?)Math.Round((
            entities.Ln2.Column2.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column2).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column2 = 0;
      }

      if (Lt(0, entities.Ln1.Column3))
      {
        local.Create.Column3 =
          (long?)Math.Round((
            entities.Ln2.Column3.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column3).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column3 = 0;
      }

      if (Lt(0, entities.Ln1.Column4))
      {
        local.Create.Column4 =
          (long?)Math.Round((
            entities.Ln2.Column4.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column4).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column4 = 0;
      }

      if (Lt(0, entities.Ln1.Column5))
      {
        local.Create.Column5 =
          (long?)Math.Round((
            entities.Ln2.Column5.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column5).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column5 = 0;
      }

      if (Lt(0, entities.Ln1.Column6))
      {
        local.Create.Column6 =
          (long?)Math.Round((
            entities.Ln2.Column6.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column6).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column6 = 0;
      }

      if (Lt(0, entities.Ln1.Column7))
      {
        local.Create.Column7 =
          (long?)Math.Round((
            entities.Ln2.Column7.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column7).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column7 = 0;
      }

      if (Lt(0, entities.Ln1.Column8))
      {
        local.Create.Column8 =
          (long?)Math.Round((
            entities.Ln2.Column8.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column8).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column8 = 0;
      }

      if (Lt(0, entities.Ln1.Column9))
      {
        local.Create.Column9 =
          (long?)Math.Round((
            entities.Ln2.Column9.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column9).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column9 = 0;
      }

      if (Lt(0, entities.Ln1.Column10))
      {
        local.Create.Column10 =
          (long?)Math.Round((
            entities.Ln2.Column10.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column10).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column10 = 0;
      }

      if (Lt(0, entities.Ln1.Column11))
      {
        local.Create.Column11 =
          (long?)Math.Round((
            entities.Ln2.Column11.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column11).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column11 = 0;
      }

      if (Lt(0, entities.Ln1.Column12))
      {
        local.Create.Column12 =
          (long?)Math.Round((
            entities.Ln2.Column12.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column12).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column12 = 0;
      }

      if (Lt(0, entities.Ln1.Column13))
      {
        local.Create.Column13 =
          (long?)Math.Round((
            entities.Ln2.Column13.GetValueOrDefault() * 100 / (
            decimal?)entities.Ln1.Column13).GetValueOrDefault(),
          MidpointRounding.AwayFromZero);
      }
      else
      {
        local.Create.Column13 = 0;
      }

      if (ReadStatsReport1())
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

            goto Read;
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

Read:

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "03 " + NumberToString
          (entities.Ln1.OfficeId.GetValueOrDefault(), 12, 4);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.Ln1.ServicePrvdrId.GetValueOrDefault(), 11, 5);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Error.LineNumber = 3;
          local.Error.OfficeId = entities.Ln1.OfficeId;
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
    local.ProgramCheckpointRestart.RestartInfo = "05 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Error.LineNumber = 3;
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

    entities.Ln3.Populated = false;
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

    entities.Ln3.YearMonth = yearMonth;
    entities.Ln3.FirstRunNumber = firstRunNumber;
    entities.Ln3.LineNumber = lineNumber;
    entities.Ln3.CreatedTimestamp = createdTimestamp;
    entities.Ln3.ServicePrvdrId = servicePrvdrId;
    entities.Ln3.OfficeId = officeId;
    entities.Ln3.CaseWrkRole = caseWrkRole;
    entities.Ln3.CaseEffDate = caseEffDate;
    entities.Ln3.ParentId = parentId;
    entities.Ln3.ChiefId = 0;
    entities.Ln3.Column1 = column1;
    entities.Ln3.Column2 = column2;
    entities.Ln3.Column3 = column3;
    entities.Ln3.Column4 = column4;
    entities.Ln3.Column5 = column5;
    entities.Ln3.Column6 = column6;
    entities.Ln3.Column7 = column7;
    entities.Ln3.Column8 = column8;
    entities.Ln3.Column9 = column9;
    entities.Ln3.Column10 = column10;
    entities.Ln3.Column11 = column11;
    entities.Ln3.Column12 = column12;
    entities.Ln3.Column13 = column13;
    entities.Ln3.Column14 = 0;
    entities.Ln3.Column15 = 0;
    entities.Ln3.Populated = true;
  }

  private bool ReadStatsReport1()
  {
    entities.Ln3.Populated = false;

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
          entities.Ln1.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId", entities.Ln1.OfficeId.GetValueOrDefault());
        db.SetNullableString(
          command, "caseWrkRole", entities.Ln1.CaseWrkRole ?? "");
        db.SetNullableDate(
          command, "caseEffDate", entities.Ln1.CaseEffDate.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Ln3.YearMonth = db.GetNullableInt32(reader, 0);
        entities.Ln3.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.Ln3.LineNumber = db.GetNullableInt32(reader, 2);
        entities.Ln3.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ln3.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.Ln3.OfficeId = db.GetNullableInt32(reader, 5);
        entities.Ln3.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.Ln3.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.Ln3.ParentId = db.GetNullableInt32(reader, 8);
        entities.Ln3.ChiefId = db.GetNullableInt32(reader, 9);
        entities.Ln3.Column1 = db.GetNullableInt64(reader, 10);
        entities.Ln3.Column2 = db.GetNullableInt64(reader, 11);
        entities.Ln3.Column3 = db.GetNullableInt64(reader, 12);
        entities.Ln3.Column4 = db.GetNullableInt64(reader, 13);
        entities.Ln3.Column5 = db.GetNullableInt64(reader, 14);
        entities.Ln3.Column6 = db.GetNullableInt64(reader, 15);
        entities.Ln3.Column7 = db.GetNullableInt64(reader, 16);
        entities.Ln3.Column8 = db.GetNullableInt64(reader, 17);
        entities.Ln3.Column9 = db.GetNullableInt64(reader, 18);
        entities.Ln3.Column10 = db.GetNullableInt64(reader, 19);
        entities.Ln3.Column11 = db.GetNullableInt64(reader, 20);
        entities.Ln3.Column12 = db.GetNullableInt64(reader, 21);
        entities.Ln3.Column13 = db.GetNullableInt64(reader, 22);
        entities.Ln3.Column14 = db.GetNullableInt64(reader, 23);
        entities.Ln3.Column15 = db.GetNullableInt64(reader, 24);
        entities.Ln3.Populated = true;
      });
  }

  private bool ReadStatsReport2()
  {
    entities.Ln2.Populated = false;

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
          entities.Ln1.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId", entities.Ln1.OfficeId.GetValueOrDefault());
        db.SetNullableString(
          command, "caseWrkRole", entities.Ln1.CaseWrkRole ?? "");
        db.SetNullableDate(
          command, "caseEffDate", entities.Ln1.CaseEffDate.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Ln2.YearMonth = db.GetNullableInt32(reader, 0);
        entities.Ln2.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.Ln2.LineNumber = db.GetNullableInt32(reader, 2);
        entities.Ln2.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ln2.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.Ln2.OfficeId = db.GetNullableInt32(reader, 5);
        entities.Ln2.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.Ln2.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.Ln2.ParentId = db.GetNullableInt32(reader, 8);
        entities.Ln2.ChiefId = db.GetNullableInt32(reader, 9);
        entities.Ln2.Column1 = db.GetNullableInt64(reader, 10);
        entities.Ln2.Column2 = db.GetNullableInt64(reader, 11);
        entities.Ln2.Column3 = db.GetNullableInt64(reader, 12);
        entities.Ln2.Column4 = db.GetNullableInt64(reader, 13);
        entities.Ln2.Column5 = db.GetNullableInt64(reader, 14);
        entities.Ln2.Column6 = db.GetNullableInt64(reader, 15);
        entities.Ln2.Column7 = db.GetNullableInt64(reader, 16);
        entities.Ln2.Column8 = db.GetNullableInt64(reader, 17);
        entities.Ln2.Column9 = db.GetNullableInt64(reader, 18);
        entities.Ln2.Column10 = db.GetNullableInt64(reader, 19);
        entities.Ln2.Column11 = db.GetNullableInt64(reader, 20);
        entities.Ln2.Column12 = db.GetNullableInt64(reader, 21);
        entities.Ln2.Column13 = db.GetNullableInt64(reader, 22);
        entities.Ln2.Column14 = db.GetNullableInt64(reader, 23);
        entities.Ln2.Column15 = db.GetNullableInt64(reader, 24);
        entities.Ln2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadStatsReport3()
  {
    entities.Ln1.Populated = false;

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
        entities.Ln1.YearMonth = db.GetNullableInt32(reader, 0);
        entities.Ln1.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.Ln1.LineNumber = db.GetNullableInt32(reader, 2);
        entities.Ln1.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ln1.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.Ln1.OfficeId = db.GetNullableInt32(reader, 5);
        entities.Ln1.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.Ln1.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.Ln1.ParentId = db.GetNullableInt32(reader, 8);
        entities.Ln1.ChiefId = db.GetNullableInt32(reader, 9);
        entities.Ln1.Column1 = db.GetNullableInt64(reader, 10);
        entities.Ln1.Column2 = db.GetNullableInt64(reader, 11);
        entities.Ln1.Column3 = db.GetNullableInt64(reader, 12);
        entities.Ln1.Column4 = db.GetNullableInt64(reader, 13);
        entities.Ln1.Column5 = db.GetNullableInt64(reader, 14);
        entities.Ln1.Column6 = db.GetNullableInt64(reader, 15);
        entities.Ln1.Column7 = db.GetNullableInt64(reader, 16);
        entities.Ln1.Column8 = db.GetNullableInt64(reader, 17);
        entities.Ln1.Column9 = db.GetNullableInt64(reader, 18);
        entities.Ln1.Column10 = db.GetNullableInt64(reader, 19);
        entities.Ln1.Column11 = db.GetNullableInt64(reader, 20);
        entities.Ln1.Column12 = db.GetNullableInt64(reader, 21);
        entities.Ln1.Column13 = db.GetNullableInt64(reader, 22);
        entities.Ln1.Column14 = db.GetNullableInt64(reader, 23);
        entities.Ln1.Column15 = db.GetNullableInt64(reader, 24);
        entities.Ln1.Populated = true;

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

    entities.Ln3.Populated = false;
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
          entities.Ln3.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ln3.Column1 = column1;
    entities.Ln3.Column2 = column2;
    entities.Ln3.Column3 = column3;
    entities.Ln3.Column4 = column4;
    entities.Ln3.Column5 = column5;
    entities.Ln3.Column6 = column6;
    entities.Ln3.Column7 = column7;
    entities.Ln3.Column8 = column8;
    entities.Ln3.Column9 = column9;
    entities.Ln3.Column10 = column10;
    entities.Ln3.Column11 = column11;
    entities.Ln3.Column12 = column12;
    entities.Ln3.Column13 = column13;
    entities.Ln3.Populated = true;
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
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    private Common commitCnt;
    private External forCommit;
    private StatsVerifi error;
    private ProgramCheckpointRestart programCheckpointRestart;
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
    /// A value of Ln3.
    /// </summary>
    [JsonPropertyName("ln3")]
    public StatsReport Ln3
    {
      get => ln3 ??= new();
      set => ln3 = value;
    }

    /// <summary>
    /// A value of Ln2.
    /// </summary>
    [JsonPropertyName("ln2")]
    public StatsReport Ln2
    {
      get => ln2 ??= new();
      set => ln2 = value;
    }

    /// <summary>
    /// A value of Ln1.
    /// </summary>
    [JsonPropertyName("ln1")]
    public StatsReport Ln1
    {
      get => ln1 ??= new();
      set => ln1 = value;
    }

    private StatsReport ln3;
    private StatsReport ln2;
    private StatsReport ln1;
  }
#endregion
}
