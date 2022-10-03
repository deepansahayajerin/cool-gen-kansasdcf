// Program: CO_PREQ_PRINT_REQUEST_STATUS, ID: 371140736, model: 746.
// Short name: SWEPREQP
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
/// A program: CO_PREQ_PRINT_REQUEST_STATUS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class CoPreqPrintRequestStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_PREQ_PRINT_REQUEST_STATUS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoPreqPrintRequestStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoPreqPrintRequestStatus.
  /// </summary>
  public CoPreqPrintRequestStatus(IContext context, Import import, Export export)
    :
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

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // : Check for Next Tran.
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // : User entered this screen from another screen
      global.Command = "DISPLAY";
      export.SelectedServiceProvider.UserId = global.UserId;
    }

    // : Handle the return from a prompt.
    switch(TrimEnd(global.Command))
    {
      case "RETSVPL":
        global.Command = "DISPLAY";

        break;
      case "RETLJOB":
        global.Command = "DISPLAY";

        break;
      default:
        break;
    }

    // *****************************************************************
    // Housekeeping
    // *****************************************************************
    local.Current.Date = Now().Date;

    // *****************************************************************
    // Move Imports to Exports
    // *****************************************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      global.Command = "DISPLAY";
      export.SelectedServiceProvider.UserId = global.UserId;
    }
    else
    {
      export.SelectedServiceProvider.Assign(import.SelectedServiceProvider);
      export.ServiceProvider.FormattedName =
        import.ServiceProvider.FormattedName;
      MoveJob(import.SelectedJob, export.SelectedJob);
      export.PromptToSpvl.SelectChar = import.PromptToSpvl.SelectChar;
      export.PromptToLjob.SelectChar = import.PromptToLjob.SelectChar;

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
        MoveJob(import.Group.Item.Job, export.Group.Update.Job);
        export.Group.Update.JobRun.Assign(import.Group.Item.JobRun);
        export.Group.Update.ParmInfo.Text80 = import.Group.Item.ParmInfo.Text80;
        export.Group.Update.RptDest.Text50 = import.Group.Item.RptDest.Text50;
        export.Group.Next();
      }
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

        ExitState = "ACO_NE0000_INVALID_ACTION";

        return;
      }
    }

    // : Verify the security for the User to be able to execute specific 
    // commands.
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "REPRINT") || Equal(global.Command, "DRPT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // : Verify Selection of one or more Items and handle DELETE's.
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "REPRINT") || Equal
      (global.Command, "DRPT"))
    {
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
                ExitState = "JOB_RUN_NF";
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

      if (Equal(global.Command, "DELETE"))
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          global.Command = "DISPLAY";
        }
      }
      else if (local.Common.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }
    }

    // : Main case of command.
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        // : Prompt Check
        if (!IsEmpty(export.PromptToLjob.SelectChar) && !
          IsEmpty(export.PromptToSpvl.SelectChar))
        {
          var field3 = GetField(export.PromptToSpvl, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.PromptToLjob, "selectChar");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        switch(AsChar(export.PromptToSpvl.SelectChar))
        {
          case 'S':
            export.PromptToSpvl.SelectChar = "";
            ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptToSpvl, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptToLjob.SelectChar))
        {
          case 'S':
            export.PromptToLjob.SelectChar = "";
            ExitState = "ECO_LNK_TO_LJOB";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptToLjob, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        // : PF4 - List pressed with no acceptable prompt entered.
        var field1 = GetField(export.PromptToSpvl, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.PromptToLjob, "selectChar");

        field2.Error = true;

        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        break;
      case "DISPLAY":
        // : As a default, Set the Service Provider to the Current User.
        if (IsEmpty(export.SelectedServiceProvider.UserId))
        {
          export.SelectedServiceProvider.UserId = global.UserId;
        }

        // : Read Service Provider for the current User.
        if (ReadServiceProvider())
        {
          MoveServiceProvider(entities.ExistingServiceProvider,
            export.SelectedServiceProvider);
          local.CsePersonsWorkSet.FirstName =
            entities.ExistingServiceProvider.FirstName;
          local.CsePersonsWorkSet.LastName =
            entities.ExistingServiceProvider.LastName;
          local.CsePersonsWorkSet.MiddleInitial =
            entities.ExistingServiceProvider.MiddleInitial;
          UseSiFormatCsePersonName();
          export.ServiceProvider.FormattedName =
            local.CsePersonsWorkSet.FormattedName;
        }
        else
        {
          var field3 =
            GetField(export.SelectedServiceProvider, "systemGeneratedId");

          field3.Error = true;

          var field4 = GetField(export.SelectedServiceProvider, "userId");

          field4.Error = true;

          var field5 = GetField(export.ServiceProvider, "formattedName");

          field5.Error = true;

          var field6 = GetField(export.PromptToSpvl, "selectChar");

          field6.Protected = false;
          field6.Focused = true;

          ExitState = "SERVICE_PROVIDER_NF";

          return;
        }

        if (!IsEmpty(export.SelectedJob.Name))
        {
          if (ReadJob())
          {
            MoveJob(entities.ExistingJob, export.SelectedJob);
          }
          else
          {
            var field = GetField(export.SelectedJob, "name");

            field.Error = true;

            export.SelectedJob.Description = "";
            ExitState = "JOB_NF";

            return;
          }
        }

        // : Load the Export Group View with Requested Reports.
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadJobJobRun())
        {
          if (!IsEmpty(export.SelectedJob.Name))
          {
            if (!Equal(entities.ExistingJob.Name, export.SelectedJob.Name))
            {
              export.Group.Next();

              continue;
            }
          }

          MoveJob(entities.ExistingJob, export.Group.Update.Job);
          export.Group.Update.JobRun.Assign(entities.ExistingJobRun);
          export.Group.Update.Common.SelectChar = "";
          export.Group.Update.ParmInfo.Text80 =
            entities.ExistingJobRun.ParmInfo ?? Spaces(80);

          switch(TrimEnd(entities.ExistingJobRun.OutputType))
          {
            case "PRINTER":
              export.Group.Update.RptDest.Text50 =
                entities.ExistingJobRun.PrinterId ?? Spaces(50);

              break;
            case "WORDPFCT-P":
              export.Group.Update.RptDest.Text50 =
                entities.ExistingJobRun.EmailAddress ?? Spaces(50);

              break;
            case "WORDPFCT-L":
              export.Group.Update.RptDest.Text50 =
                entities.ExistingJobRun.EmailAddress ?? Spaces(50);

              break;
            case "R-PRINTER":
              export.Group.Update.JobRun.OutputType = "PRINTER";
              export.Group.Update.RptDest.Text50 =
                entities.ExistingJobRun.PrinterId ?? Spaces(50);

              break;
            default:
              export.Group.Update.RptDest.Text50 = "";

              break;
          }

          export.Group.Next();
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else if (export.Group.IsFull)
        {
          ExitState = "ACO_NI0000_MORE_ROWS_EXIST";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "DELETE":
        // : DELETE has already been handled.
        break;
      case "DRPT":
        // : View a Single Report Request.
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) != 'S')
          {
            continue;
          }

          MoveJob(export.Group.Item.Job, export.DlgflwJob);
          MoveJobRun(export.Group.Item.JobRun, export.DlgflwJobRun);
          ExitState = "CO_LNK_TO_DRPT";

          return;
        }

        break;
      case "REPRINT":
        // : Reprint one or more Report Requests.
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) != 'S')
          {
            continue;
          }

          MoveJob(export.Group.Item.Job, export.DlgflwJob);
          MoveJobRun(export.Group.Item.JobRun, export.DlgflwJobRun);
          ExitState = "LNK_TO_RPRT";

          return;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveJob(Job source, Job target)
  {
    target.Name = source.Name;
    target.Description = source.Description;
  }

  private static void MoveJobRun(JobRun source, JobRun target)
  {
    target.StartTimestamp = source.StartTimestamp;
    target.Status = source.Status;
    target.OutputType = source.OutputType;
    target.ErrorMsg = source.ErrorMsg;
    target.SystemGenId = source.SystemGenId;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(export.Hidden);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

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
        db.SetString(command, "name", export.SelectedJob.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Description = db.GetString(reader, 1);
        entities.ExistingJob.Populated = true;
      });
  }

  private IEnumerable<bool> ReadJobJobRun()
  {
    return ReadEach("ReadJobJobRun",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdSrvcPrvderId",
          entities.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJobRun.JobName = db.GetString(reader, 0);
        entities.ExistingJob.Description = db.GetString(reader, 1);
        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 2);
        entities.ExistingJobRun.Status = db.GetString(reader, 3);
        entities.ExistingJobRun.SpdSrvcPrvderId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingJobRun.PrinterId = db.GetNullableString(reader, 5);
        entities.ExistingJobRun.OutputType = db.GetString(reader, 6);
        entities.ExistingJobRun.ErrorMsg = db.GetNullableString(reader, 7);
        entities.ExistingJobRun.EmailAddress = db.GetNullableString(reader, 8);
        entities.ExistingJobRun.ParmInfo = db.GetNullableString(reader, 9);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 10);
        entities.ExistingJob.Populated = true;
        entities.ExistingJobRun.Populated = true;

        return true;
      });
  }

  private bool ReadJobRun()
  {
    entities.ExistingJobRun.Populated = false;

    return Read("ReadJobRun",
      (db, command) =>
      {
        db.SetString(command, "jobName", export.Group.Item.Job.Name);
        db.
          SetInt32(command, "systemGenId", export.Group.Item.JobRun.SystemGenId);
          
      },
      (db, reader) =>
      {
        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 0);
        entities.ExistingJobRun.Status = db.GetString(reader, 1);
        entities.ExistingJobRun.SpdSrvcPrvderId =
          db.GetNullableInt32(reader, 2);
        entities.ExistingJobRun.PrinterId = db.GetNullableString(reader, 3);
        entities.ExistingJobRun.OutputType = db.GetString(reader, 4);
        entities.ExistingJobRun.ErrorMsg = db.GetNullableString(reader, 5);
        entities.ExistingJobRun.EmailAddress = db.GetNullableString(reader, 6);
        entities.ExistingJobRun.ParmInfo = db.GetNullableString(reader, 7);
        entities.ExistingJobRun.JobName = db.GetString(reader, 8);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 9);
        entities.ExistingJobRun.Populated = true;
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

  private bool ReadServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", export.SelectedServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.Populated = true;
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
      /// A value of Job.
      /// </summary>
      [JsonPropertyName("job")]
      public Job Job
      {
        get => job ??= new();
        set => job = value;
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
      /// A value of RptDest.
      /// </summary>
      [JsonPropertyName("rptDest")]
      public WorkArea RptDest
      {
        get => rptDest ??= new();
        set => rptDest = value;
      }

      /// <summary>
      /// A value of ParmInfo.
      /// </summary>
      [JsonPropertyName("parmInfo")]
      public WorkArea ParmInfo
      {
        get => parmInfo ??= new();
        set => parmInfo = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common common;
      private Job job;
      private JobRun jobRun;
      private WorkArea rptDest;
      private WorkArea parmInfo;
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
    /// A value of SelectedServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedServiceProvider")]
    public ServiceProvider SelectedServiceProvider
    {
      get => selectedServiceProvider ??= new();
      set => selectedServiceProvider = value;
    }

    /// <summary>
    /// A value of SelectedJob.
    /// </summary>
    [JsonPropertyName("selectedJob")]
    public Job SelectedJob
    {
      get => selectedJob ??= new();
      set => selectedJob = value;
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
    /// A value of PromptToSpvl.
    /// </summary>
    [JsonPropertyName("promptToSpvl")]
    public Common PromptToSpvl
    {
      get => promptToSpvl ??= new();
      set => promptToSpvl = value;
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

    private Standard standard;
    private ServiceProvider selectedServiceProvider;
    private Job selectedJob;
    private CsePersonsWorkSet serviceProvider;
    private Common promptToSpvl;
    private Common promptToLjob;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
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
      /// A value of Job.
      /// </summary>
      [JsonPropertyName("job")]
      public Job Job
      {
        get => job ??= new();
        set => job = value;
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
      /// A value of RptDest.
      /// </summary>
      [JsonPropertyName("rptDest")]
      public WorkArea RptDest
      {
        get => rptDest ??= new();
        set => rptDest = value;
      }

      /// <summary>
      /// A value of ParmInfo.
      /// </summary>
      [JsonPropertyName("parmInfo")]
      public WorkArea ParmInfo
      {
        get => parmInfo ??= new();
        set => parmInfo = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common common;
      private Job job;
      private JobRun jobRun;
      private WorkArea rptDest;
      private WorkArea parmInfo;
    }

    /// <summary>
    /// A value of SelectedServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedServiceProvider")]
    public ServiceProvider SelectedServiceProvider
    {
      get => selectedServiceProvider ??= new();
      set => selectedServiceProvider = value;
    }

    /// <summary>
    /// A value of SelectedJob.
    /// </summary>
    [JsonPropertyName("selectedJob")]
    public Job SelectedJob
    {
      get => selectedJob ??= new();
      set => selectedJob = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public CsePersonsWorkSet ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of PromptToSpvl.
    /// </summary>
    [JsonPropertyName("promptToSpvl")]
    public Common PromptToSpvl
    {
      get => promptToSpvl ??= new();
      set => promptToSpvl = value;
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
    /// A value of DlgflwJob.
    /// </summary>
    [JsonPropertyName("dlgflwJob")]
    public Job DlgflwJob
    {
      get => dlgflwJob ??= new();
      set => dlgflwJob = value;
    }

    /// <summary>
    /// A value of DlgflwJobRun.
    /// </summary>
    [JsonPropertyName("dlgflwJobRun")]
    public JobRun DlgflwJobRun
    {
      get => dlgflwJobRun ??= new();
      set => dlgflwJobRun = value;
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

    private ServiceProvider selectedServiceProvider;
    private Job selectedJob;
    private Standard standard;
    private CsePersonsWorkSet serviceProvider;
    private Common promptToSpvl;
    private Common promptToLjob;
    private Array<GroupGroup> group;
    private Job dlgflwJob;
    private JobRun dlgflwJobRun;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private DateWorkArea current;
    private Common common;
    private CsePersonsWorkSet csePersonsWorkSet;
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

    private ServiceProvider existingServiceProvider;
    private Job existingJob;
    private JobRun existingJobRun;
    private ReportData existingReportData;
  }
#endregion
}
