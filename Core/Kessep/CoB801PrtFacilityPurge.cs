// Program: CO_B801_PRT_FACILITY_PURGE, ID: 371137515, model: 746.
// Short name: SWEE801B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CO_B801_PRT_FACILITY_PURGE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class CoB801PrtFacilityPurge: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_B801_PRT_FACILITY_PURGE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoB801PrtFacilityPurge(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoB801PrtFacilityPurge.
  /// </summary>
  public CoB801PrtFacilityPurge(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Date    Developer    Request #   Description
    // --------------------------------------------------------------------
    // 12/11/00  Alan Doty              Initial Development
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ReqFnd.Flag = "N";

    // *****************************************************************
    // Get Program Processing Info
    // *****************************************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Null1.Date))
    {
      local.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    local.PurgeDays.Count =
      (int)StringToNumber(Substring(
        local.ProgramProcessingInfo.ParameterList, 1, 3));

    if (local.PurgeDays.Count == 0)
    {
      local.PurgeDays.Count = 14;
    }

    local.PurgeTo.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate, -local.PurgeDays.Count);

    // *****************************************************************
    // Open Report Files
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = global.UserId;
    local.NeededToOpen.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    // *****************************************************************
    // Mainline
    // *****************************************************************
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail =
      "Service-Provider                     Print-Reqs";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    // : Produce a report on activity.
    foreach(var item in ReadServiceProviderJobRun())
    {
      if (entities.ExistingServiceProvider.SystemGeneratedId != local
        .Hold.SystemGeneratedId)
      {
        if (local.Hold.SystemGeneratedId == 0)
        {
          local.ReqFnd.Flag = "Y";
        }
        else
        {
          local.NeededToWrite.RptDetail =
            NumberToString(local.Hold.SystemGeneratedId, 11, 5) + "  " + local
            .ServiceProvider.FormattedName + "  " + NumberToString
            (local.JobRunReq.Count, 11, 5);
          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

            return;
          }
        }

        local.Hold.SystemGeneratedId =
          entities.ExistingServiceProvider.SystemGeneratedId;
        local.JobRunReq.Count = 1;
        local.ServiceProvider.FirstName =
          entities.ExistingServiceProvider.FirstName;
        local.ServiceProvider.LastName =
          entities.ExistingServiceProvider.LastName;
        local.ServiceProvider.MiddleInitial =
          entities.ExistingServiceProvider.MiddleInitial;
        UseSiFormatCsePersonName();
      }

      ++local.JobRunReq.Count;
    }

    if (AsChar(local.ReqFnd.Flag) == 'Y')
    {
      local.NeededToWrite.RptDetail =
        NumberToString(entities.ExistingServiceProvider.SystemGeneratedId, 11, 5)
        + "  " + local.ServiceProvider.FormattedName + "  " + NumberToString
        (local.JobRunReq.Count, 11, 5);
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

        return;
      }
    }

    // : Purge all Reports older than the parm value (i.e. 14 days)
    foreach(var item in ReadJobRun())
    {
      foreach(var item1 in ReadReportData())
      {
        DeleteReportData();
        ++local.ReportDataRecsDeleted.Count;
      }

      DeleteJobRun();
      ++local.JobRunRecsDeleted.Count;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Number of Reports Deleted . . . . . . : " + NumberToString
      (local.JobRunRecsDeleted.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Number of Report Lines Deleted. . . . : " + NumberToString
      (local.ReportDataRecsDeleted.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    // *****************************************************************
    // Close Report Files
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.ServiceProvider);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.ServiceProvider.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void DeleteJobRun()
  {
    Update("DeleteJobRun",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });
  }

  private void DeleteReportData()
  {
    Update("DeleteReportData",
      (db, command) =>
      {
        db.SetString(command, "type", entities.ExistingReportData.Type1);
        db.SetInt32(
          command, "sequenceNumber",
          entities.ExistingReportData.SequenceNumber);
        db.SetString(command, "jobName", entities.ExistingReportData.JobName);
        db.SetInt32(
          command, "jruSystemGenId",
          entities.ExistingReportData.JruSystemGenId);
      });
  }

  private IEnumerable<bool> ReadJobRun()
  {
    entities.ExistingJobRun.Populated = false;

    return ReadEach("ReadJobRun",
      (db, command) =>
      {
        db.SetDate(command, "date", local.PurgeTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 0);
        entities.ExistingJobRun.EndTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ExistingJobRun.Status = db.GetString(reader, 2);
        entities.ExistingJobRun.SpdSrvcPrvderId =
          db.GetNullableInt32(reader, 3);
        entities.ExistingJobRun.PrinterId = db.GetNullableString(reader, 4);
        entities.ExistingJobRun.OutputType = db.GetString(reader, 5);
        entities.ExistingJobRun.ErrorMsg = db.GetNullableString(reader, 6);
        entities.ExistingJobRun.EmailAddress = db.GetNullableString(reader, 7);
        entities.ExistingJobRun.ParmInfo = db.GetNullableString(reader, 8);
        entities.ExistingJobRun.JobName = db.GetString(reader, 9);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 10);
        entities.ExistingJobRun.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadReportData()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);
    entities.ExistingReportData.Populated = false;

    return ReadEach("ReadReportData",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.SetInt32(
          command, "jruSystemGenId", entities.ExistingJobRun.SystemGenId);
      },
      (db, reader) =>
      {
        entities.ExistingReportData.Type1 = db.GetString(reader, 0);
        entities.ExistingReportData.SequenceNumber = db.GetInt32(reader, 1);
        entities.ExistingReportData.FirstPageOnlyInd =
          db.GetNullableString(reader, 2);
        entities.ExistingReportData.LineControl = db.GetString(reader, 3);
        entities.ExistingReportData.LineText = db.GetString(reader, 4);
        entities.ExistingReportData.JobName = db.GetString(reader, 5);
        entities.ExistingReportData.JruSystemGenId = db.GetInt32(reader, 6);
        entities.ExistingReportData.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderJobRun()
  {
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingJobRun.Populated = false;

    return ReadEach("ReadServiceProviderJobRun",
      null,
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingJobRun.SpdSrvcPrvderId =
          db.GetNullableInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 5);
        entities.ExistingJobRun.EndTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.ExistingJobRun.Status = db.GetString(reader, 7);
        entities.ExistingJobRun.PrinterId = db.GetNullableString(reader, 8);
        entities.ExistingJobRun.OutputType = db.GetString(reader, 9);
        entities.ExistingJobRun.ErrorMsg = db.GetNullableString(reader, 10);
        entities.ExistingJobRun.EmailAddress = db.GetNullableString(reader, 11);
        entities.ExistingJobRun.ParmInfo = db.GetNullableString(reader, 12);
        entities.ExistingJobRun.JobName = db.GetString(reader, 13);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 14);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingJobRun.Populated = true;

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
    /// A value of ReqFnd.
    /// </summary>
    [JsonPropertyName("reqFnd")]
    public Common ReqFnd
    {
      get => reqFnd ??= new();
      set => reqFnd = value;
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
    /// A value of PurgeDays.
    /// </summary>
    [JsonPropertyName("purgeDays")]
    public Common PurgeDays
    {
      get => purgeDays ??= new();
      set => purgeDays = value;
    }

    /// <summary>
    /// A value of PurgeTo.
    /// </summary>
    [JsonPropertyName("purgeTo")]
    public DateWorkArea PurgeTo
    {
      get => purgeTo ??= new();
      set => purgeTo = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public CsePersonsWorkSet ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of JobRunReq.
    /// </summary>
    [JsonPropertyName("jobRunReq")]
    public Common JobRunReq
    {
      get => jobRunReq ??= new();
      set => jobRunReq = value;
    }

    /// <summary>
    /// A value of JobRunRecsDeleted.
    /// </summary>
    [JsonPropertyName("jobRunRecsDeleted")]
    public Common JobRunRecsDeleted
    {
      get => jobRunRecsDeleted ??= new();
      set => jobRunRecsDeleted = value;
    }

    /// <summary>
    /// A value of ReportDataRecsDeleted.
    /// </summary>
    [JsonPropertyName("reportDataRecsDeleted")]
    public Common ReportDataRecsDeleted
    {
      get => reportDataRecsDeleted ??= new();
      set => reportDataRecsDeleted = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public ServiceProvider Hold
    {
      get => hold ??= new();
      set => hold = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private Common reqFnd;
    private ProgramProcessingInfo programProcessingInfo;
    private Common purgeDays;
    private DateWorkArea purgeTo;
    private CsePersonsWorkSet serviceProvider;
    private Common jobRunReq;
    private Common jobRunRecsDeleted;
    private Common reportDataRecsDeleted;
    private ServiceProvider hold;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingJobRun.
    /// </summary>
    [JsonPropertyName("existingJobRun")]
    public JobRun ExistingJobRun
    {
      get => existingJobRun ??= new();
      set => existingJobRun = value;
    }

    /// <summary>
    /// A value of ExistingReportData.
    /// </summary>
    [JsonPropertyName("existingReportData")]
    public ReportData ExistingReportData
    {
      get => existingReportData ??= new();
      set => existingReportData = value;
    }

    private ServiceProvider existingServiceProvider;
    private JobRun existingJobRun;
    private ReportData existingReportData;
  }
#endregion
}
