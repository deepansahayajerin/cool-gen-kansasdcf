// Program: GB_LJRN_LIST_JOB_RUNS, ID: 371147303, model: 746.
// Short name: SWELJRNP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: GB_LJRN_LIST_JOB_RUNS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class GbLjrnListJobRuns: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_LJRN_LIST_JOB_RUNS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbLjrnListJobRuns(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbLjrnListJobRuns.
  /// </summary>
  public GbLjrnListJobRuns(IContext context, Import import, Export export):
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
    // 12/25/99  K.Price                New Development
    // 12/11/00  Alan Doty              Complete Rewrite
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // : Check for Next Tran.
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // : User entered this screen from another screen
      return;
    }

    // *****************************************************************
    // Move Imports to Exports
    // *****************************************************************
    export.Job.Name = import.Job.Name;
    export.Starting.StartTimestamp = import.Starting.StartTimestamp;
    export.PromptToLjob.SelectChar = import.PromptToLjob.SelectChar;
    MoveNextTranInfo(import.Hidden, export.Hidden);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.JobRun.Assign(import.Group.Item.JobRun);
      MoveServiceProvider(import.Group.Item.ServiceProvider,
        export.Group.Update.ServiceProvider);
      export.Group.Update.CsePersonsWorkSet.FormattedName =
        import.Group.Item.CsePersonsWorkSet.FormattedName;
      export.Group.Next();
    }

    // : Handle Next Tran...
    if (Equal(global.Command, "ENTER"))
    {
      if (IsEmpty(import.Standard.NextTransaction))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        ExitState = "SP0000_REQUIRED_FIELD_MISSING";

        return;
      }

      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        ExitState = "SC0052_NEXT_TRAN_PROHIBITED";

        return;
      }
    }

    // : Verify the security for the User to be able to execute specific 
    // commands.
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // : Check selection and process DELETE is required.
    if (Equal(global.Command, "DELETE"))
    {
      // : Verify that one and only one Report has been selected.
      local.Common.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Common.Count;

            if (Equal(global.Command, "DELETE"))
            {
              if (ReadJobRun())
              {
                foreach(var item in ReadReportData())
                {
                  DeleteReportData();
                }

                DeleteJobRun();
                export.Group.Update.Common.SelectChar = "*";
              }
              else
              {
                ExitState = "JOB_NF";
              }
            }

            break;
          case ' ':
            break;
          case '*':
            break;
          default:
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Color = "red";
            field.Protected = false;
            field.Focused = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }
      }

      if (local.Common.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      if (Equal(global.Command, "DRPT"))
      {
        if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "DISPLAY";
      }
    }

    // : Main case of command.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (!ReadJob())
        {
          ExitState = "JOB_NF";

          return;
        }

        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadJobRunServiceProvider())
        {
          export.Group.Update.Common.SelectChar = "";
          MoveJobRun2(entities.ExistingJobRun, export.Group.Update.JobRun);
          MoveServiceProvider(entities.ExistingServiceProvider,
            export.Group.Update.ServiceProvider);
          local.CsePersonsWorkSet.FirstName =
            entities.ExistingServiceProvider.FirstName;
          local.CsePersonsWorkSet.LastName =
            entities.ExistingServiceProvider.LastName;
          local.CsePersonsWorkSet.MiddleInitial =
            entities.ExistingServiceProvider.MiddleInitial;
          UseSiFormatCsePersonName();
          export.Group.Update.CsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;
          export.Group.Next();
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else if (export.Group.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "DRPT":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            export.Group.Update.Common.SelectChar = "";
            MoveJobRun1(export.Group.Item.JobRun, export.Dlgflw);
            ExitState = "CO_LNK_TO_DRPT";

            return;
          }
        }

        break;
      case "DELETE":
        // : DELETE has already been handled.
        break;
      case "LIST":
        if (AsChar(export.PromptToLjob.SelectChar) == 'S')
        {
          export.PromptToLjob.SelectChar = "";
          ExitState = "ECO_LNK_TO_LIST1";

          return;
        }

        var field = GetField(export.PromptToLjob, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "RETURN":
        ExitState = "OE0000_RETURN_LNK_BLANK";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveJobRun1(JobRun source, JobRun target)
  {
    target.StartTimestamp = source.StartTimestamp;
    target.SystemGenId = source.SystemGenId;
  }

  private static void MoveJobRun2(JobRun source, JobRun target)
  {
    target.StartTimestamp = source.StartTimestamp;
    target.EndTimestamp = source.EndTimestamp;
    target.Status = source.Status;
    target.SystemGenId = source.SystemGenId;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
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

  private bool ReadJob()
  {
    entities.ExistingJob.Populated = false;

    return Read("ReadJob",
      (db, command) =>
      {
        db.SetString(command, "name", export.Job.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Populated = true;
      });
  }

  private bool ReadJobRun()
  {
    entities.ExistingJobRun.Populated = false;

    return Read("ReadJobRun",
      (db, command) =>
      {
        db.SetString(command, "jobName", export.Job.Name);
        db.
          SetInt32(command, "systemGenId", export.Group.Item.JobRun.SystemGenId);
          
      },
      (db, reader) =>
      {
        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 0);
        entities.ExistingJobRun.EndTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ExistingJobRun.Status = db.GetString(reader, 2);
        entities.ExistingJobRun.SpdSrvcPrvderId =
          db.GetNullableInt32(reader, 3);
        entities.ExistingJobRun.OutputType = db.GetString(reader, 4);
        entities.ExistingJobRun.JobName = db.GetString(reader, 5);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 6);
        entities.ExistingJobRun.Populated = true;
      });
  }

  private IEnumerable<bool> ReadJobRunServiceProvider()
  {
    return ReadEach("ReadJobRunServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJob.Name);
        db.SetDateTime(
          command, "startTimestamp",
          export.Starting.StartTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 0);
        entities.ExistingJobRun.EndTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ExistingJobRun.Status = db.GetString(reader, 2);
        entities.ExistingJobRun.SpdSrvcPrvderId =
          db.GetNullableInt32(reader, 3);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingJobRun.OutputType = db.GetString(reader, 4);
        entities.ExistingJobRun.JobName = db.GetString(reader, 5);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 6);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 7);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 8);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 9);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 10);
        entities.ExistingJobRun.Populated = true;
        entities.ExistingServiceProvider.Populated = true;

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
        entities.ExistingReportData.JobName = db.GetString(reader, 2);
        entities.ExistingReportData.JruSystemGenId = db.GetInt32(reader, 3);
        entities.ExistingReportData.Populated = true;

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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of JobRun.
      /// </summary>
      [JsonPropertyName("jobRun")]
      public JobRun JobRun
      {
        get => jobRun ??= new();
        set => jobRun = value;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common common;
      private JobRun jobRun;
      private ServiceProvider serviceProvider;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Job.
    /// </summary>
    [JsonPropertyName("job")]
    public Job Job
    {
      get => job ??= new();
      set => job = value;
    }

    /// <summary>
    /// A value of PromptToLjob.
    /// </summary>
    [JsonPropertyName("promptToLjob")]
    public Common PromptToLjob
    {
      get => promptToLjob ??= new();
      set => promptToLjob = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public JobRun Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public Standard DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private Standard standard;
    private Job job;
    private Common promptToLjob;
    private JobRun starting;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard delMe;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of JobRun.
      /// </summary>
      [JsonPropertyName("jobRun")]
      public JobRun JobRun
      {
        get => jobRun ??= new();
        set => jobRun = value;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common common;
      private JobRun jobRun;
      private ServiceProvider serviceProvider;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Job.
    /// </summary>
    [JsonPropertyName("job")]
    public Job Job
    {
      get => job ??= new();
      set => job = value;
    }

    /// <summary>
    /// A value of PromptToLjob.
    /// </summary>
    [JsonPropertyName("promptToLjob")]
    public Common PromptToLjob
    {
      get => promptToLjob ??= new();
      set => promptToLjob = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public JobRun Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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

    /// <summary>
    /// A value of Dlgflw.
    /// </summary>
    [JsonPropertyName("dlgflw")]
    public JobRun Dlgflw
    {
      get => dlgflw ??= new();
      set => dlgflw = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Standard standard;
    private Job job;
    private Common promptToLjob;
    private JobRun starting;
    private Array<GroupGroup> group;
    private JobRun dlgflw;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingJob.
    /// </summary>
    [JsonPropertyName("existingJob")]
    public Job ExistingJob
    {
      get => existingJob ??= new();
      set => existingJob = value;
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

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    private Job existingJob;
    private JobRun existingJobRun;
    private ReportData existingReportData;
    private ServiceProvider existingServiceProvider;
  }
#endregion
}
