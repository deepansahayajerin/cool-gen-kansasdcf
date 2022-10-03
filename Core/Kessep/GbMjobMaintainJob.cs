// Program: GB_MJOB_MAINTAIN_JOB, ID: 373515697, model: 746.
// Short name: SWEMJOBP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: GB_MJOB_MAINTAIN_JOB.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class GbMjobMaintainJob: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_MJOB_MAINTAIN_JOB program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbMjobMaintainJob(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbMjobMaintainJob.
  /// </summary>
  public GbMjobMaintainJob(IContext context, Import import, Export export):
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
    export.Job.Assign(import.Job);
    export.PromptToLjob.SelectChar = import.PromptToLjob.SelectChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(import.Hidden, export.Hidden);

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
    if (Equal(global.Command, "DISPLAY") && Equal(global.Command, "ADD") && Equal
      (global.Command, "UPDATE") && Equal(global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (ReadJob1())
        {
          export.Job.Assign(entities.ExistingJob);
        }
        else
        {
          ExitState = "JOB_NF";

          return;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "ADD":
        if (IsEmpty(export.Job.Name))
        {
          var field1 = GetField(export.Job, "name");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.Job.TranId))
        {
          var field1 = GetField(export.Job, "tranId");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.Job.Description))
        {
          var field1 = GetField(export.Job, "description");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        try
        {
          CreateJob();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "JOB_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "JOB_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        ExitState = "ACO_NI0000_ADD_SUCCESSFUL";

        break;
      case "UPDATE":
        if (IsEmpty(export.Job.Name))
        {
          var field1 = GetField(export.Job, "name");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.Job.TranId))
        {
          var field1 = GetField(export.Job, "tranId");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.Job.Description))
        {
          var field1 = GetField(export.Job, "description");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (ReadJob2())
        {
          try
          {
            UpdateJob();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "JOB_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "JOB_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          ExitState = "JOB_NF";

          return;
        }

        ExitState = "FN0000_UPDATE_SUCCESSFUL";

        break;
      case "DELETE":
        if (ReadJob2())
        {
          if (ReadJclTemplate())
          {
            ExitState = "DEL_JCL_TEMPLATE_FIRST";

            return;
          }

          DeleteJob();
          export.Job.Assign(local.Null1);
        }
        else
        {
          ExitState = "JOB_NF";

          return;
        }

        ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";

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
      case "LJOB":
        ExitState = "ECO_LNK_TO_LJOB";

        break;
      case "LJRN":
        ExitState = "ECO_LNK_TO_LJRN";

        break;
      case "MJCL":
        ExitState = "GB0000_MTN_JCL";

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

  private void CreateJob()
  {
    var name = export.Job.Name;
    var description = export.Job.Description;
    var tranId = export.Job.TranId;

    entities.ExistingJob.Populated = false;
    Update("CreateJob",
      (db, command) =>
      {
        db.SetString(command, "name", name);
        db.SetNullableInt32(command, "zdelSequence", 0);
        db.SetNullableString(command, "zdelRecord", "");
        db.SetString(command, "description", description);
        db.SetString(command, "tranId", tranId);
      });

    entities.ExistingJob.Name = name;
    entities.ExistingJob.Description = description;
    entities.ExistingJob.TranId = tranId;
    entities.ExistingJob.Populated = true;
  }

  private void DeleteJob()
  {
    Update("DeleteJob",
      (db, command) =>
      {
        db.SetString(command, "name", entities.ExistingJob.Name);
      });
  }

  private bool ReadJclTemplate()
  {
    entities.ExistingJclTemplate.Populated = false;

    return Read("ReadJclTemplate",
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
      });
  }

  private bool ReadJob1()
  {
    entities.ExistingJob.Populated = false;

    return Read("ReadJob1",
      (db, command) =>
      {
        db.SetString(command, "name", export.Job.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Description = db.GetString(reader, 1);
        entities.ExistingJob.TranId = db.GetString(reader, 2);
        entities.ExistingJob.Populated = true;
      });
  }

  private bool ReadJob2()
  {
    entities.ExistingJob.Populated = false;

    return Read("ReadJob2",
      (db, command) =>
      {
        db.SetString(command, "name", export.Job.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Description = db.GetString(reader, 1);
        entities.ExistingJob.TranId = db.GetString(reader, 2);
        entities.ExistingJob.Populated = true;
      });
  }

  private void UpdateJob()
  {
    var description = export.Job.Description;
    var tranId = export.Job.TranId;

    entities.ExistingJob.Populated = false;
    Update("UpdateJob",
      (db, command) =>
      {
        db.SetString(command, "description", description);
        db.SetString(command, "tranId", tranId);
        db.SetString(command, "name", entities.ExistingJob.Name);
      });

    entities.ExistingJob.Description = description;
    entities.ExistingJob.TranId = tranId;
    entities.ExistingJob.Populated = true;
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
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Job Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private Job null1;
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
