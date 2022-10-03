// Program: GB_LJOB_LIST_JOBS, ID: 371148027, model: 746.
// Short name: SWELJOBP
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
/// A program: GB_LJOB_LIST_JOBS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class GbLjobListJobs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_LJOB_LIST_JOBS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbLjobListJobs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbLjobListJobs.
  /// </summary>
  public GbLjobListJobs(IContext context, Import import, Export export):
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
    // 04/30/96  Burrell - SRS          New Development
    // 12/12/96  R. Marchman            Add new security and next tran
    // 12/25/99  K. Price               Complete Re-write
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
      global.Command = "DISPLAY";
    }

    // *****************************************************************
    // Move Imports to Exports
    // *****************************************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(import.Hidden, export.Hidden);
    export.Starting.Name = import.Starting.Name;

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
      export.Group.Update.Job.Assign(import.Group.Item.Job);
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
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "MJOB") || Equal
      (global.Command, "LJRN") || Equal(global.Command, "MJCL"))
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
              if (ReadJob1())
              {
                foreach(var item in ReadJclTemplate())
                {
                  DeleteJclTemplate();
                }

                DeleteJob();
                export.Group.Update.Common.SelectChar = "*";
              }
              else
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;

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

      if (Equal(global.Command, "DELETE"))
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          global.Command = "DISPLAY";
        }

        goto Test;
      }

      if (local.Common.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }
    }

Test:

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
      }

      return;
    }

    // : Main case of command.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadJob2())
        {
          export.Group.Update.Common.SelectChar = "";
          export.Group.Update.Job.Assign(entities.ExistingJob);
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
      case "DELETE":
        // : DELETE has already been handled.
        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "MJOB":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            export.Group.Update.Common.SelectChar = "";
            export.Dlgflw.Name = export.Group.Item.Job.Name;
            ExitState = "ECO_XFR_TO_MTNX";

            return;
          }
        }

        break;
      case "LJRN":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            export.Group.Update.Common.SelectChar = "";
            export.Dlgflw.Name = export.Group.Item.Job.Name;
            ExitState = "ECO_XFR_TO_LST_JOB_RUNS";

            return;
          }
        }

        break;
      case "MJCL":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            export.Group.Update.Common.SelectChar = "";
            export.Dlgflw.Name = export.Group.Item.Job.Name;
            ExitState = "GB0000_MTN_JCL";

            return;
          }
        }

        break;
      case "RETURN":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            export.Dlgflw.Name = export.Group.Item.Job.Name;
          }
        }

        ExitState = "OE0000_RETURN_LNK_BLANK";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_ACTION";

        break;
    }
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

  private void DeleteJclTemplate()
  {
    Update("DeleteJclTemplate",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequenceNumber",
          entities.ExistingJclTemplate.SequenceNumber);
        db.SetString(command, "jobName", entities.ExistingJclTemplate.JobName);
        db.SetString(
          command, "outputType", entities.ExistingJclTemplate.OutputType);
      });
  }

  private void DeleteJob()
  {
    Update("DeleteJob",
      (db, command) =>
      {
        db.SetString(command, "name", entities.ExistingJob.Name);
      });
  }

  private IEnumerable<bool> ReadJclTemplate()
  {
    entities.ExistingJclTemplate.Populated = false;

    return ReadEach("ReadJclTemplate",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJob.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJclTemplate.SequenceNumber = db.GetInt32(reader, 0);
        entities.ExistingJclTemplate.JobName = db.GetString(reader, 1);
        entities.ExistingJclTemplate.OutputType = db.GetString(reader, 2);
        entities.ExistingJclTemplate.Populated = true;

        return true;
      });
  }

  private bool ReadJob1()
  {
    entities.ExistingJob.Populated = false;

    return Read("ReadJob1",
      (db, command) =>
      {
        db.SetString(command, "name", export.Group.Item.Job.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Description = db.GetString(reader, 1);
        entities.ExistingJob.TranId = db.GetString(reader, 2);
        entities.ExistingJob.Populated = true;
      });
  }

  private IEnumerable<bool> ReadJob2()
  {
    return ReadEach("ReadJob2",
      (db, command) =>
      {
        db.SetString(command, "name", export.Starting.Name);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Description = db.GetString(reader, 1);
        entities.ExistingJob.TranId = db.GetString(reader, 2);
        entities.ExistingJob.Populated = true;

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
      /// A value of Job.
      /// </summary>
      [JsonPropertyName("job")]
      public Job Job
      {
        get => job ??= new();
        set => job = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private Job job;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Job Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
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
    private Job starting;
    private ScrollingAttributes scrollingAttributes;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private Job job;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Job Starting
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
    public Job Dlgflw
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
    private Job starting;
    private Array<GroupGroup> group;
    private Job dlgflw;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
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

    private Common select;
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
    /// A value of ExistingJclTemplate.
    /// </summary>
    [JsonPropertyName("existingJclTemplate")]
    public JclTemplate ExistingJclTemplate
    {
      get => existingJclTemplate ??= new();
      set => existingJclTemplate = value;
    }

    private Job existingJob;
    private JclTemplate existingJclTemplate;
  }
#endregion
}
