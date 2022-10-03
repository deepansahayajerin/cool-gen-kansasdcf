// Program: CO_DRPT_DISPLAY_REPORT, ID: 371138088, model: 746.
// Short name: SWEDRPTP
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
/// A program: CO_DRPT_DISPLAY_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class CoDrptDisplayReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_DRPT_DISPLAY_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoDrptDisplayReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoDrptDisplayReport.
  /// </summary>
  public CoDrptDisplayReport(IContext context, Import import, Export export):
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
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 01/05/2010  Raj S              CQ602,9076  Modified to increate page 
    // group view size*
    // *
    // 
    // from 500 to 1050. Added new Exist State  *
    // *
    // 
    // to prompt the user about Maximum Rows.   *
    // *
    // 
    // *
    // ***************************************************************************************
    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // : No Next Tran to this screen is allowed.
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // : User entered this screen from another screen and this is NOT allowed.
      ExitState = "LE0000_CANT_NEXTTRAN_INTO";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    // Move Imports to Exports
    // *****************************************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Hidden.Assign(import.Hidden);
    MoveJob(import.Job, export.Job);
    export.JobRun.Assign(import.JobRun);
    export.FirstTimeThru.Flag = import.FirstTimeThru.Flag;
    export.DisplaySide.Text1 = import.DisplaySide.Text1;
    export.MaxPages.Count = import.MaxPages.Count;
    export.DisplayPage.Count = import.DisplayPage.Count;
    export.ScrollInd.Text10 = import.ScrollInd.Text10;

    if (!import.Page.IsEmpty)
    {
      for(import.Page.Index = 0; import.Page.Index < import.Page.Count; ++
        import.Page.Index)
      {
        if (!import.Page.CheckSize())
        {
          break;
        }

        export.Page.Index = import.Page.Index;
        export.Page.CheckSize();

        MoveReportData(import.Page.Item.Page1, export.Page.Update.Page1);
      }

      import.Page.CheckIndex();
    }

    if (!import.Group.IsEmpty)
    {
      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.ReportData.Assign(import.Group.Item.ReportData);
        export.Group.Update.WorkArea.Text80 = import.Group.Item.WorkArea.Text80;
      }

      import.Group.CheckIndex();
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
      }

      return;
    }

    // : Verify the security for the User to be able to execute specific 
    // commands.
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "REPRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // : Main case of command.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (AsChar(export.FirstTimeThru.Flag) == 'N')
        {
          if (export.DisplayPage.Count > export.MaxPages.Count)
          {
            var field = GetField(export.DisplayPage, "count");

            field.Error = true;

            ExitState = "CO0000_DSPLY_LN_GRTR_MAX_RPT_LN";

            return;
          }

          break;
        }

        // : Execute ONLY on the first time thru.
        export.FirstTimeThru.Flag = "N";
        export.DisplaySide.Text1 = "L";
        export.DisplayPage.Count = 1;
        export.ScrollInd.Text10 = "More:";
        local.Tmp.Count = 0;

        foreach(var item in ReadReportData4())
        {
          ++local.Tmp.Count;

          if (export.Page.IsEmpty)
          {
            export.Page.Index = 0;
            export.Page.CheckSize();

            MoveReportData(entities.ExistingSumm, export.Page.Update.Page1);

            continue;
          }

          if (local.Tmp.Count == 14)
          {
            ++export.Page.Index;
            export.Page.CheckSize();

            MoveReportData(entities.ExistingSumm, export.Page.Update.Page1);

            if (export.Page.Index + 1 == Export.PageGroup.Capacity)
            {
              break;
            }

            local.Tmp.Count = 0;

            continue;
          }

          // : Check the Line Control
          if (Verify(entities.ExistingSumm.LineControl, "0123456789") == 0)
          {
            local.BlankLine.Count =
              (int)StringToNumber(entities.ExistingSumm.LineControl);

            if (local.Tmp.Count + local.BlankLine.Count >= 14)
            {
              ++export.Page.Index;
              export.Page.CheckSize();

              MoveReportData(entities.ExistingSumm, export.Page.Update.Page1);

              if (export.Page.Index + 1 == Export.PageGroup.Capacity)
              {
                break;
              }

              local.Tmp.Count = 0;

              continue;
            }

            local.Tmp.Count += local.BlankLine.Count;
          }
        }

        export.MaxPages.Count = export.Page.Count;

        break;
      case "DELETE":
        // : Delete one or more Report Requests.
        if (ReadJobRun())
        {
          foreach(var item in ReadReportData1())
          {
            DeleteReportData();
          }

          DeleteJobRun();
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

          return;
        }
        else
        {
          ExitState = "JOB_RUN_NF";

          return;
        }

        break;
      case "REPRINT":
        ExitState = "LNK_TO_RPRT";

        return;
      case "RETURN":
        ExitState = "OE0000_RETURN_LNK_BLANK";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "PREV":
        if (export.DisplayPage.Count <= 1)
        {
          export.DisplayPage.Count = 1;
          local.DisplaySeqNo.Count = 1;
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          break;
        }

        --export.DisplayPage.Count;

        break;
      case "NEXT":
        if (export.DisplayPage.Count >= export.MaxPages.Count)
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          return;
        }

        ++export.DisplayPage.Count;

        break;
      case "LEFT":
        // : Display the left side of the report.
        if (AsChar(export.DisplaySide.Text1) == 'L')
        {
          ExitState = "CO0000_REPORT_EDGE_REACHED";

          return;
        }

        export.DisplaySide.Text1 = "L";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          export.Group.Update.WorkArea.Text80 =
            Substring(export.Group.Item.ReportData.LineText, 1, 79);
        }

        export.Group.CheckIndex();

        return;
      case "RIGHT":
        // : Display the right side of the report.
        if (AsChar(export.DisplaySide.Text1) == 'R')
        {
          ExitState = "CO0000_REPORT_EDGE_REACHED";

          return;
        }

        export.DisplaySide.Text1 = "R";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          export.Group.Update.WorkArea.Text80 =
            Substring(export.Group.Item.ReportData.LineText, 54, 79);
        }

        export.Group.CheckIndex();

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (export.Page.IsEmpty)
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

      return;
    }

    // *****************************************************************
    // Display Processing Main Line
    // *****************************************************************
    for(export.Group.Index = 0; export.Group.Index < Export
      .GroupGroup.Capacity; ++export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      export.Group.Update.ReportData.Assign(local.NullReportData);
      export.Group.Update.WorkArea.Text80 = local.NullWorkArea.Text80;
    }

    export.Group.CheckIndex();
    export.Group.Index = -1;
    export.Group.Count = 0;

    export.Page.Index = export.DisplayPage.Count - 1;
    export.Page.CheckSize();

    if (AsChar(export.Page.Item.Page1.Type1) == 'H')
    {
      foreach(var item in ReadReportData3())
      {
        if (AsChar(entities.ExistingReportData.Type1) == 'H')
        {
          if (entities.ExistingReportData.SequenceNumber < export
            .Page.Item.Page1.SequenceNumber)
          {
            continue;
          }
        }

        // : Check the Line Control
        if (export.Group.Index >= 0)
        {
          if (Verify(entities.ExistingReportData.LineControl, "0123456789") == 0
            )
          {
            local.BlankLine.Count =
              (int)StringToNumber(entities.ExistingReportData.LineControl);
            local.Tmp.Count = 1;

            for(var limit = local.BlankLine.Count; local.Tmp.Count <= limit; ++
              local.Tmp.Count)
            {
              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ReportData.Assign(local.NullReportData);
              export.Group.Update.WorkArea.Text80 = local.NullWorkArea.Text80;

              if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
              {
                goto ReadEach1;
              }
            }
          }
        }

        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.ReportData.Assign(entities.ExistingReportData);

        if (AsChar(export.DisplaySide.Text1) == 'L')
        {
          export.Group.Update.WorkArea.Text80 =
            Substring(entities.ExistingReportData.LineText, 1, 79);
        }
        else
        {
          export.Group.Update.WorkArea.Text80 =
            Substring(entities.ExistingReportData.LineText, 54, 79);
        }

        if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
        {
          break;
        }
      }

ReadEach1:
      ;
    }
    else
    {
      foreach(var item in ReadReportData2())
      {
        // : Check the Line Control
        if (export.Group.Index >= 0)
        {
          if (Verify(entities.ExistingReportData.LineControl, "0123456789") == 0
            )
          {
            local.BlankLine.Count =
              (int)StringToNumber(entities.ExistingReportData.LineControl);
            local.Tmp.Count = 1;

            for(var limit = local.BlankLine.Count; local.Tmp.Count <= limit; ++
              local.Tmp.Count)
            {
              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ReportData.Assign(local.NullReportData);
              export.Group.Update.WorkArea.Text80 = local.NullWorkArea.Text80;

              if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
              {
                goto ReadEach2;
              }
            }
          }
        }

        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.ReportData.Assign(entities.ExistingReportData);

        if (AsChar(export.DisplaySide.Text1) == 'L')
        {
          export.Group.Update.WorkArea.Text80 =
            Substring(entities.ExistingReportData.LineText, 1, 79);
        }
        else
        {
          export.Group.Update.WorkArea.Text80 =
            Substring(entities.ExistingReportData.LineText, 54, 79);
        }

        if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
        {
          break;
        }
      }

ReadEach2:
      ;
    }

    if (export.DisplayPage.Count == 1)
    {
      if (export.MaxPages.Count > 1)
      {
        export.ScrollInd.Text10 = "More:   +";
      }
      else
      {
        export.ScrollInd.Text10 = "More:";
      }
    }
    else if (export.DisplayPage.Count == export.MaxPages.Count)
    {
      export.ScrollInd.Text10 = "More: -";
    }
    else
    {
      export.ScrollInd.Text10 = "More: - +";
    }

    if (IsExitState("ACO_NI0000_TOP_OF_LIST"))
    {
      return;
    }

    ExitState = "ACO_DRPT_DISPLAY_SUCCESSFUL";
  }

  private static void MoveJob(Job source, Job target)
  {
    target.Name = source.Name;
    target.Description = source.Description;
  }

  private static void MoveReportData(ReportData source, ReportData target)
  {
    target.Type1 = source.Type1;
    target.SequenceNumber = source.SequenceNumber;
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

  private bool ReadJobRun()
  {
    entities.ExistingJobRun.Populated = false;

    return Read("ReadJobRun",
      (db, command) =>
      {
        db.SetString(command, "jobName", export.Job.Name);
        db.SetInt32(command, "systemGenId", export.JobRun.SystemGenId);
      },
      (db, reader) =>
      {
        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 0);
        entities.ExistingJobRun.Status = db.GetString(reader, 1);
        entities.ExistingJobRun.JobName = db.GetString(reader, 2);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 3);
        entities.ExistingJobRun.Populated = true;
      });
  }

  private IEnumerable<bool> ReadReportData1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);
    entities.ExistingReportData.Populated = false;

    return ReadEach("ReadReportData1",
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

  private IEnumerable<bool> ReadReportData2()
  {
    entities.ExistingReportData.Populated = false;

    return ReadEach("ReadReportData2",
      (db, command) =>
      {
        db.SetInt32(command, "jruSystemGenId", export.JobRun.SystemGenId);
        db.SetString(command, "jobName", export.Job.Name);
        db.SetInt32(
          command, "sequenceNumber", export.Page.Item.Page1.SequenceNumber);
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

  private IEnumerable<bool> ReadReportData3()
  {
    entities.ExistingReportData.Populated = false;

    return ReadEach("ReadReportData3",
      (db, command) =>
      {
        db.SetInt32(command, "jruSystemGenId", export.JobRun.SystemGenId);
        db.SetString(command, "jobName", export.Job.Name);
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

  private IEnumerable<bool> ReadReportData4()
  {
    entities.ExistingSumm.Populated = false;

    return ReadEach("ReadReportData4",
      (db, command) =>
      {
        db.SetInt32(command, "jruSystemGenId", export.JobRun.SystemGenId);
        db.SetString(command, "jobName", export.Job.Name);
      },
      (db, reader) =>
      {
        entities.ExistingSumm.Type1 = db.GetString(reader, 0);
        entities.ExistingSumm.SequenceNumber = db.GetInt32(reader, 1);
        entities.ExistingSumm.LineControl = db.GetString(reader, 2);
        entities.ExistingSumm.JobName = db.GetString(reader, 3);
        entities.ExistingSumm.JruSystemGenId = db.GetInt32(reader, 4);
        entities.ExistingSumm.Populated = true;

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
    /// <summary>A PageGroup group.</summary>
    [Serializable]
    public class PageGroup
    {
      /// <summary>
      /// A value of Page1.
      /// </summary>
      [JsonPropertyName("page1")]
      public ReportData Page1
      {
        get => page1 ??= new();
        set => page1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1050;

      private ReportData page1;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of ReportData.
      /// </summary>
      [JsonPropertyName("reportData")]
      public ReportData ReportData
      {
        get => reportData ??= new();
        set => reportData = value;
      }

      /// <summary>
      /// A value of WorkArea.
      /// </summary>
      [JsonPropertyName("workArea")]
      public WorkArea WorkArea
      {
        get => workArea ??= new();
        set => workArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private ReportData reportData;
      private WorkArea workArea;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of DisplaySide.
    /// </summary>
    [JsonPropertyName("displaySide")]
    public TextWorkArea DisplaySide
    {
      get => displaySide ??= new();
      set => displaySide = value;
    }

    /// <summary>
    /// A value of DisplayPage.
    /// </summary>
    [JsonPropertyName("displayPage")]
    public Common DisplayPage
    {
      get => displayPage ??= new();
      set => displayPage = value;
    }

    /// <summary>
    /// A value of CurrPageEndLine.
    /// </summary>
    [JsonPropertyName("currPageEndLine")]
    public Common CurrPageEndLine
    {
      get => currPageEndLine ??= new();
      set => currPageEndLine = value;
    }

    /// <summary>
    /// A value of MaxHeaderLines.
    /// </summary>
    [JsonPropertyName("maxHeaderLines")]
    public Common MaxHeaderLines
    {
      get => maxHeaderLines ??= new();
      set => maxHeaderLines = value;
    }

    /// <summary>
    /// A value of MaxPages.
    /// </summary>
    [JsonPropertyName("maxPages")]
    public Common MaxPages
    {
      get => maxPages ??= new();
      set => maxPages = value;
    }

    /// <summary>
    /// A value of FirstTimeThru.
    /// </summary>
    [JsonPropertyName("firstTimeThru")]
    public Common FirstTimeThru
    {
      get => firstTimeThru ??= new();
      set => firstTimeThru = value;
    }

    /// <summary>
    /// Gets a value of Page.
    /// </summary>
    [JsonIgnore]
    public Array<PageGroup> Page => page ??= new(PageGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Page for json serialization.
    /// </summary>
    [JsonPropertyName("page")]
    [Computed]
    public IList<PageGroup> Page_Json
    {
      get => page;
      set => Page.Assign(value);
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

    /// <summary>
    /// A value of ScrollInd.
    /// </summary>
    [JsonPropertyName("scrollInd")]
    public TextWorkArea ScrollInd
    {
      get => scrollInd ??= new();
      set => scrollInd = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public Common DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private Standard standard;
    private NextTranInfo hidden;
    private Job job;
    private JobRun jobRun;
    private TextWorkArea displaySide;
    private Common displayPage;
    private Common currPageEndLine;
    private Common maxHeaderLines;
    private Common maxPages;
    private Common firstTimeThru;
    private Array<PageGroup> page;
    private Array<GroupGroup> group;
    private TextWorkArea scrollInd;
    private Common delMe;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PageGroup group.</summary>
    [Serializable]
    public class PageGroup
    {
      /// <summary>
      /// A value of Page1.
      /// </summary>
      [JsonPropertyName("page1")]
      public ReportData Page1
      {
        get => page1 ??= new();
        set => page1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1050;

      private ReportData page1;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of ReportData.
      /// </summary>
      [JsonPropertyName("reportData")]
      public ReportData ReportData
      {
        get => reportData ??= new();
        set => reportData = value;
      }

      /// <summary>
      /// A value of WorkArea.
      /// </summary>
      [JsonPropertyName("workArea")]
      public WorkArea WorkArea
      {
        get => workArea ??= new();
        set => workArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private ReportData reportData;
      private WorkArea workArea;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of DisplaySide.
    /// </summary>
    [JsonPropertyName("displaySide")]
    public TextWorkArea DisplaySide
    {
      get => displaySide ??= new();
      set => displaySide = value;
    }

    /// <summary>
    /// A value of DisplayPage.
    /// </summary>
    [JsonPropertyName("displayPage")]
    public Common DisplayPage
    {
      get => displayPage ??= new();
      set => displayPage = value;
    }

    /// <summary>
    /// A value of CurrPageEndLine.
    /// </summary>
    [JsonPropertyName("currPageEndLine")]
    public Common CurrPageEndLine
    {
      get => currPageEndLine ??= new();
      set => currPageEndLine = value;
    }

    /// <summary>
    /// A value of MaxHeaderLines.
    /// </summary>
    [JsonPropertyName("maxHeaderLines")]
    public Common MaxHeaderLines
    {
      get => maxHeaderLines ??= new();
      set => maxHeaderLines = value;
    }

    /// <summary>
    /// A value of MaxPages.
    /// </summary>
    [JsonPropertyName("maxPages")]
    public Common MaxPages
    {
      get => maxPages ??= new();
      set => maxPages = value;
    }

    /// <summary>
    /// A value of FirstTimeThru.
    /// </summary>
    [JsonPropertyName("firstTimeThru")]
    public Common FirstTimeThru
    {
      get => firstTimeThru ??= new();
      set => firstTimeThru = value;
    }

    /// <summary>
    /// Gets a value of Page.
    /// </summary>
    [JsonIgnore]
    public Array<PageGroup> Page => page ??= new(PageGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Page for json serialization.
    /// </summary>
    [JsonPropertyName("page")]
    [Computed]
    public IList<PageGroup> Page_Json
    {
      get => page;
      set => Page.Assign(value);
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

    /// <summary>
    /// A value of ScrollInd.
    /// </summary>
    [JsonPropertyName("scrollInd")]
    public TextWorkArea ScrollInd
    {
      get => scrollInd ??= new();
      set => scrollInd = value;
    }

    private Standard standard;
    private NextTranInfo hidden;
    private Job job;
    private JobRun jobRun;
    private TextWorkArea displaySide;
    private Common displayPage;
    private Common currPageEndLine;
    private Common maxHeaderLines;
    private Common maxPages;
    private Common firstTimeThru;
    private Array<PageGroup> page;
    private Array<GroupGroup> group;
    private TextWorkArea scrollInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A HeaderGroup group.</summary>
    [Serializable]
    public class HeaderGroup
    {
      /// <summary>
      /// A value of Header1.
      /// </summary>
      [JsonPropertyName("header1")]
      public ReportData Header1
      {
        get => header1 ??= new();
        set => header1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private ReportData header1;
    }

    /// <summary>
    /// A value of DisplaySeqNo.
    /// </summary>
    [JsonPropertyName("displaySeqNo")]
    public Common DisplaySeqNo
    {
      get => displaySeqNo ??= new();
      set => displaySeqNo = value;
    }

    /// <summary>
    /// A value of NullReportData.
    /// </summary>
    [JsonPropertyName("nullReportData")]
    public ReportData NullReportData
    {
      get => nullReportData ??= new();
      set => nullReportData = value;
    }

    /// <summary>
    /// A value of NullWorkArea.
    /// </summary>
    [JsonPropertyName("nullWorkArea")]
    public WorkArea NullWorkArea
    {
      get => nullWorkArea ??= new();
      set => nullWorkArea = value;
    }

    /// <summary>
    /// A value of PageNo.
    /// </summary>
    [JsonPropertyName("pageNo")]
    public Common PageNo
    {
      get => pageNo ??= new();
      set => pageNo = value;
    }

    /// <summary>
    /// Gets a value of Header.
    /// </summary>
    [JsonIgnore]
    public Array<HeaderGroup> Header => header ??= new(HeaderGroup.Capacity);

    /// <summary>
    /// Gets a value of Header for json serialization.
    /// </summary>
    [JsonPropertyName("header")]
    [Computed]
    public IList<HeaderGroup> Header_Json
    {
      get => header;
      set => Header.Assign(value);
    }

    /// <summary>
    /// A value of BlankLine.
    /// </summary>
    [JsonPropertyName("blankLine")]
    public Common BlankLine
    {
      get => blankLine ??= new();
      set => blankLine = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public Common Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    private Common displaySeqNo;
    private ReportData nullReportData;
    private WorkArea nullWorkArea;
    private Common pageNo;
    private Array<HeaderGroup> header;
    private Common blankLine;
    private Common tmp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingSumm.
    /// </summary>
    [JsonPropertyName("existingSumm")]
    public ReportData ExistingSumm
    {
      get => existingSumm ??= new();
      set => existingSumm = value;
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
    /// A value of ExistingJobRun.
    /// </summary>
    [JsonPropertyName("existingJobRun")]
    public JobRun ExistingJobRun
    {
      get => existingJobRun ??= new();
      set => existingJobRun = value;
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

    private ReportData existingSumm;
    private ReportData existingReportData;
    private JobRun existingJobRun;
    private Job existingJob;
  }
#endregion
}
