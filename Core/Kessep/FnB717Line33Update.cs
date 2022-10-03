// Program: FN_B717_LINE_33_UPDATE, ID: 373364649, model: 746.
// Short name: SWE03041
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_33_UPDATE.
/// </summary>
[Serializable]
public partial class FnB717Line33Update: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_33_UPDATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line33Update(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line33Update.
  /// </summary>
  public FnB717Line33Update(IContext context, Import import, Export export):
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
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "33 "))
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

    foreach(var item in ReadStatsReport())
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

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "33 " + NumberToString
          (entities.Ln33.OfficeId.GetValueOrDefault(), 12, 4);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.Ln33.ServicePrvdrId.GetValueOrDefault(), 11, 5);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Error.LineNumber = 33;
          local.Error.OfficeId = entities.Ln33.OfficeId;
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
    local.ProgramCheckpointRestart.RestartInfo = "35 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Error.LineNumber = 33;
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

  private IEnumerable<bool> ReadStatsReport()
  {
    entities.Ln33.Populated = false;

    return ReadEach("ReadStatsReport",
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
        entities.Ln33.YearMonth = db.GetNullableInt32(reader, 0);
        entities.Ln33.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.Ln33.LineNumber = db.GetNullableInt32(reader, 2);
        entities.Ln33.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ln33.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.Ln33.OfficeId = db.GetNullableInt32(reader, 5);
        entities.Ln33.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.Ln33.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.Ln33.ParentId = db.GetNullableInt32(reader, 8);
        entities.Ln33.ChiefId = db.GetNullableInt32(reader, 9);
        entities.Ln33.Column1 = db.GetNullableInt64(reader, 10);
        entities.Ln33.Column2 = db.GetNullableInt64(reader, 11);
        entities.Ln33.Column3 = db.GetNullableInt64(reader, 12);
        entities.Ln33.Column4 = db.GetNullableInt64(reader, 13);
        entities.Ln33.Column5 = db.GetNullableInt64(reader, 14);
        entities.Ln33.Column6 = db.GetNullableInt64(reader, 15);
        entities.Ln33.Column7 = db.GetNullableInt64(reader, 16);
        entities.Ln33.Column8 = db.GetNullableInt64(reader, 17);
        entities.Ln33.Column9 = db.GetNullableInt64(reader, 18);
        entities.Ln33.Column10 = db.GetNullableInt64(reader, 19);
        entities.Ln33.Column11 = db.GetNullableInt64(reader, 20);
        entities.Ln33.Column12 = db.GetNullableInt64(reader, 21);
        entities.Ln33.Column13 = db.GetNullableInt64(reader, 22);
        entities.Ln33.Column14 = db.GetNullableInt64(reader, 23);
        entities.Ln33.Column15 = db.GetNullableInt64(reader, 24);
        entities.Ln33.Populated = true;

        return true;
      });
  }

  private void UpdateStatsReport()
  {
    var column1 = entities.Ln33.Column1.GetValueOrDefault() / 100;
    var column2 = entities.Ln33.Column2.GetValueOrDefault() / 100;
    var column3 = entities.Ln33.Column3.GetValueOrDefault() / 100;
    var column4 = entities.Ln33.Column4.GetValueOrDefault() / 100;
    var column5 = entities.Ln33.Column5.GetValueOrDefault() / 100;
    var column6 = entities.Ln33.Column6.GetValueOrDefault() / 100;
    var column7 = entities.Ln33.Column7.GetValueOrDefault() / 100;
    var column8 = entities.Ln33.Column8.GetValueOrDefault() / 100;
    var column9 = entities.Ln33.Column9.GetValueOrDefault() / 100;
    var column10 = entities.Ln33.Column10.GetValueOrDefault() / 100;
    var column11 = entities.Ln33.Column11.GetValueOrDefault() / 100;
    var column12 = entities.Ln33.Column12.GetValueOrDefault() / 100;
    var column13 = entities.Ln33.Column13.GetValueOrDefault() / 100;
    var column14 = entities.Ln33.Column14.GetValueOrDefault() / 100;
    var column15 = entities.Ln33.Column15.GetValueOrDefault() / 100;

    entities.Ln33.Populated = false;
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
        db.SetNullableInt64(command, "column14", column14);
        db.SetNullableInt64(command, "column15", column15);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ln33.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ln33.Column1 = column1;
    entities.Ln33.Column2 = column2;
    entities.Ln33.Column3 = column3;
    entities.Ln33.Column4 = column4;
    entities.Ln33.Column5 = column5;
    entities.Ln33.Column6 = column6;
    entities.Ln33.Column7 = column7;
    entities.Ln33.Column8 = column8;
    entities.Ln33.Column9 = column9;
    entities.Ln33.Column10 = column10;
    entities.Ln33.Column11 = column11;
    entities.Ln33.Column12 = column12;
    entities.Ln33.Column13 = column13;
    entities.Ln33.Column14 = column14;
    entities.Ln33.Column15 = column15;
    entities.Ln33.Populated = true;
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

    private Office restartOffice;
    private ServiceProvider restartServiceProvider;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common commitCnt;
    private StatsVerifi error;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ln33.
    /// </summary>
    [JsonPropertyName("ln33")]
    public StatsReport Ln33
    {
      get => ln33 ??= new();
      set => ln33 = value;
    }

    private StatsReport ln33;
  }
#endregion
}
