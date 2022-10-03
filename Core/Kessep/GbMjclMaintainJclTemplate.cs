// Program: GB_MJCL_MAINTAIN_JCL_TEMPLATE, ID: 371142997, model: 746.
// Short name: SWEMJCLP
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
/// A program: GB_MJCL_MAINTAIN_JCL_TEMPLATE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class GbMjclMaintainJclTemplate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_MJCL_MAINTAIN_JCL_TEMPLATE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbMjclMaintainJclTemplate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbMjclMaintainJclTemplate.
  /// </summary>
  public GbMjclMaintainJclTemplate(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Date		Developer	Request #	Description
    // --------------------------------------------------------------------
    // 10/02/01	K.Cole				New Development
    // 07/03/08	J. Huss		WR# 5380	Removed hard coded code value to validate 
    // against code_value table
    // 03/18/11        RMathews        CQ25663         Allow R-PRINTER as an 
    // output type although not in code value table
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

    if (Equal(global.Command, "FROMMJOB") || Equal(global.Command, "FROMLJOB"))
    {
      export.Job.Assign(import.Job);

      return;
    }

    if (Equal(global.Command, "RETLJOB"))
    {
      if (IsEmpty(import.JclTemplate.OutputType))
      {
        export.Job.Assign(import.Job);

        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      if (IsEmpty(import.Job.Name))
      {
        export.JclTemplate.OutputType = import.OutputType.Cdvalue;

        return;
      }
    }

    // *****************************************************************
    // Move Imports to Exports
    // *****************************************************************
    export.Job.Assign(import.Job);
    export.JclTemplate.OutputType = import.JclTemplate.OutputType;
    export.PromptToLjob.SelectChar = import.PromptToLjob.SelectChar;
    export.PromptToCdvl.SelectChar = import.PromptToCdvl.SelectChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(import.HiddenNextTranInfo, export.HiddenNextTranInfo);
    export.HiddenJob.Name = import.HiddenJob.Name;
    export.HiddenJclTemplate.OutputType = import.HiddenJclTemplate.OutputType;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.JclTemplate.RecordText =
        import.Group.Item.JclTemplate.RecordText;
      export.Group.Next();
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      export.JclTemplate.OutputType = import.OutputType.Cdvalue;
      global.Command = "DISPLAY";
    }

    // : Handle Next Tran...
    if (Equal(global.Command, "ENTER"))
    {
      if (IsEmpty(export.Standard.NextTransaction))
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
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // : Check for mandatory fields and valid output type
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE"))
    {
      if (IsEmpty(export.Job.Name))
      {
        var field = GetField(export.Job, "name");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      if (IsEmpty(export.JclTemplate.OutputType))
      {
        var field = GetField(export.JclTemplate, "outputType");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      if (Equal(export.JclTemplate.OutputType, "R-PRINTER"))
      {
        // 03/18/11  R-PRINTER is a valid output type for JCL maintenance, so 
        // allow it to process
      }
      else
      {
        // jmh 07/03/08  Removed hard coded code value to validate against 
        // code_value table
        local.OutputTypeCode.CodeName = "REPORT OUTPUT FORMAT";
        local.OutputTypeCodeValue.Cdvalue = export.JclTemplate.OutputType;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          var field = GetField(export.JclTemplate, "outputType");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE";

          return;
        }
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (!ReadJob2())
        {
          ExitState = "JOB_NF";

          return;
        }

        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadJclTemplate2())
        {
          export.Group.Update.JclTemplate.RecordText =
            entities.ExistingJclTemplate.RecordText;
          export.Group.Next();
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }
        else if (export.Group.IsFull)
        {
          ExitState = "ACO_NI0000_MORE_ROWS_EXIST";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        export.HiddenJob.Name = export.Job.Name;
        export.HiddenJclTemplate.OutputType = export.JclTemplate.OutputType;

        break;
      case "ADD":
        if (export.Group.IsEmpty)
        {
          ExitState = "GB_NO_JCL_TO_ADD";

          return;
        }

        if (!ReadJob1())
        {
          ExitState = "JOB_NF";

          return;
        }

        local.JclTemplate.SequenceNumber = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsEmpty(export.Group.Item.JclTemplate.RecordText))
          {
            continue;
          }

          ++local.JclTemplate.SequenceNumber;

          try
          {
            CreateJclTemplate();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "GB_JCL_ALREADY_EXISTS";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "GB_JCL_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadJclTemplate2())
        {
          export.Group.Update.JclTemplate.RecordText =
            entities.ExistingJclTemplate.RecordText;
          export.Group.Next();
        }

        export.HiddenJob.Name = export.Job.Name;
        export.HiddenJclTemplate.OutputType = export.JclTemplate.OutputType;
        ExitState = "ACO_NI0000_ADD_SUCCESSFUL";

        break;
      case "UPDATE":
        if (!Equal(export.Job.Name, export.HiddenJob.Name) || !
          Equal(export.JclTemplate.OutputType,
          export.HiddenJclTemplate.OutputType))
        {
          ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";

          return;
        }

        if (!ReadJob1())
        {
          ExitState = "JOB_NF";

          return;
        }

        foreach(var item in ReadJclTemplate1())
        {
          DeleteJclTemplate();
        }

        local.JclTemplate.SequenceNumber = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsEmpty(export.Group.Item.JclTemplate.RecordText))
          {
            continue;
          }

          ++local.JclTemplate.SequenceNumber;

          try
          {
            CreateJclTemplate();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "GB_JCL_ALREADY_EXISTS";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "GB_JCL_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadJclTemplate2())
        {
          export.Group.Update.JclTemplate.RecordText =
            entities.ExistingJclTemplate.RecordText;
          export.Group.Next();
        }

        ExitState = "FN0000_UPDATE_SUCCESSFUL";

        break;
      case "LIST":
        if (AsChar(export.PromptToLjob.SelectChar) == 'S' && AsChar
          (export.PromptToCdvl.SelectChar) == 'S')
        {
          ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";

          return;
        }

        if (AsChar(export.PromptToLjob.SelectChar) == 'S')
        {
          export.PromptToLjob.SelectChar = "";
          ExitState = "ECO_LNK_TO_LJOB";
        }
        else if (AsChar(export.PromptToCdvl.SelectChar) == 'S')
        {
          export.PromptToCdvl.SelectChar = "";
          export.FormatCode.CodeName = "REPORT OUTPUT FORMAT";
          ExitState = "ECO_LNK_TO_CODE_VALUES";
        }
        else
        {
          var field1 = GetField(export.PromptToLjob, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptToCdvl, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case "LJOB":
        ExitState = "ECO_LNK_TO_LJOB";

        break;
      case "RETURN":
        ExitState = "OE0000_RETURN_LNK_BLANK";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

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

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.OutputTypeCode.CodeName;
    useImport.CodeValue.Cdvalue = local.OutputTypeCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

  private void CreateJclTemplate()
  {
    var sequenceNumber = local.JclTemplate.SequenceNumber;
    var recordText = export.Group.Item.JclTemplate.RecordText;
    var jobName = entities.ExistingJob.Name;
    var outputType = export.JclTemplate.OutputType;

    entities.ExistingJclTemplate.Populated = false;
    Update("CreateJclTemplate",
      (db, command) =>
      {
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetString(command, "recordText", recordText);
        db.SetString(command, "jobName", jobName);
        db.SetString(command, "outputType", outputType);
      });

    entities.ExistingJclTemplate.SequenceNumber = sequenceNumber;
    entities.ExistingJclTemplate.RecordText = recordText;
    entities.ExistingJclTemplate.JobName = jobName;
    entities.ExistingJclTemplate.OutputType = outputType;
    entities.ExistingJclTemplate.Populated = true;
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

  private IEnumerable<bool> ReadJclTemplate1()
  {
    entities.ExistingJclTemplate.Populated = false;

    return ReadEach("ReadJclTemplate1",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJob.Name);
        db.SetString(command, "outputType", export.JclTemplate.OutputType);
      },
      (db, reader) =>
      {
        entities.ExistingJclTemplate.SequenceNumber = db.GetInt32(reader, 0);
        entities.ExistingJclTemplate.RecordText = db.GetString(reader, 1);
        entities.ExistingJclTemplate.JobName = db.GetString(reader, 2);
        entities.ExistingJclTemplate.OutputType = db.GetString(reader, 3);
        entities.ExistingJclTemplate.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadJclTemplate2()
  {
    return ReadEach("ReadJclTemplate2",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJob.Name);
        db.SetString(command, "outputType", export.JclTemplate.OutputType);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingJclTemplate.SequenceNumber = db.GetInt32(reader, 0);
        entities.ExistingJclTemplate.RecordText = db.GetString(reader, 1);
        entities.ExistingJclTemplate.JobName = db.GetString(reader, 2);
        entities.ExistingJclTemplate.OutputType = db.GetString(reader, 3);
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
      /// A value of JclTemplate.
      /// </summary>
      [JsonPropertyName("jclTemplate")]
      public JclTemplate JclTemplate
      {
        get => jclTemplate ??= new();
        set => jclTemplate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private JclTemplate jclTemplate;
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
    /// A value of JclTemplate.
    /// </summary>
    [JsonPropertyName("jclTemplate")]
    public JclTemplate JclTemplate
    {
      get => jclTemplate ??= new();
      set => jclTemplate = value;
    }

    /// <summary>
    /// A value of PromptToCdvl.
    /// </summary>
    [JsonPropertyName("promptToCdvl")]
    public Common PromptToCdvl
    {
      get => promptToCdvl ??= new();
      set => promptToCdvl = value;
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
    /// A value of OutputType.
    /// </summary>
    [JsonPropertyName("outputType")]
    public CodeValue OutputType
    {
      get => outputType ??= new();
      set => outputType = value;
    }

    /// <summary>
    /// A value of HiddenJob.
    /// </summary>
    [JsonPropertyName("hiddenJob")]
    public Job HiddenJob
    {
      get => hiddenJob ??= new();
      set => hiddenJob = value;
    }

    /// <summary>
    /// A value of HiddenJclTemplate.
    /// </summary>
    [JsonPropertyName("hiddenJclTemplate")]
    public JclTemplate HiddenJclTemplate
    {
      get => hiddenJclTemplate ??= new();
      set => hiddenJclTemplate = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private Standard standard;
    private Job job;
    private Common promptToLjob;
    private JclTemplate jclTemplate;
    private Common promptToCdvl;
    private Array<GroupGroup> group;
    private CodeValue outputType;
    private Job hiddenJob;
    private JclTemplate hiddenJclTemplate;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of JclTemplate.
      /// </summary>
      [JsonPropertyName("jclTemplate")]
      public JclTemplate JclTemplate
      {
        get => jclTemplate ??= new();
        set => jclTemplate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private JclTemplate jclTemplate;
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
    /// A value of JclTemplate.
    /// </summary>
    [JsonPropertyName("jclTemplate")]
    public JclTemplate JclTemplate
    {
      get => jclTemplate ??= new();
      set => jclTemplate = value;
    }

    /// <summary>
    /// A value of PromptToCdvl.
    /// </summary>
    [JsonPropertyName("promptToCdvl")]
    public Common PromptToCdvl
    {
      get => promptToCdvl ??= new();
      set => promptToCdvl = value;
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
    /// A value of FormatCode.
    /// </summary>
    [JsonPropertyName("formatCode")]
    public Code FormatCode
    {
      get => formatCode ??= new();
      set => formatCode = value;
    }

    /// <summary>
    /// A value of HiddenJob.
    /// </summary>
    [JsonPropertyName("hiddenJob")]
    public Job HiddenJob
    {
      get => hiddenJob ??= new();
      set => hiddenJob = value;
    }

    /// <summary>
    /// A value of HiddenJclTemplate.
    /// </summary>
    [JsonPropertyName("hiddenJclTemplate")]
    public JclTemplate HiddenJclTemplate
    {
      get => hiddenJclTemplate ??= new();
      set => hiddenJclTemplate = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private Standard standard;
    private Job job;
    private Common promptToLjob;
    private JclTemplate jclTemplate;
    private Common promptToCdvl;
    private Array<GroupGroup> group;
    private Code formatCode;
    private Job hiddenJob;
    private JclTemplate hiddenJclTemplate;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of OutputTypeCode.
    /// </summary>
    [JsonPropertyName("outputTypeCode")]
    public Code OutputTypeCode
    {
      get => outputTypeCode ??= new();
      set => outputTypeCode = value;
    }

    /// <summary>
    /// A value of OutputTypeCodeValue.
    /// </summary>
    [JsonPropertyName("outputTypeCodeValue")]
    public CodeValue OutputTypeCodeValue
    {
      get => outputTypeCodeValue ??= new();
      set => outputTypeCodeValue = value;
    }

    /// <summary>
    /// A value of JclTemplate.
    /// </summary>
    [JsonPropertyName("jclTemplate")]
    public JclTemplate JclTemplate
    {
      get => jclTemplate ??= new();
      set => jclTemplate = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Job Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private Common validCode;
    private Code outputTypeCode;
    private CodeValue outputTypeCodeValue;
    private JclTemplate jclTemplate;
    private Job null1;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingJclTemplate.
    /// </summary>
    [JsonPropertyName("existingJclTemplate")]
    public JclTemplate ExistingJclTemplate
    {
      get => existingJclTemplate ??= new();
      set => existingJclTemplate = value;
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

    private JclTemplate existingJclTemplate;
    private Job existingJob;
  }
#endregion
}
