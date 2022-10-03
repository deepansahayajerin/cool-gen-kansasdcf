// Program: CO_POPT_PRINT_OPTION_SELECTION, ID: 371139280, model: 746.
// Short name: SWEPOPTP
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
/// A program: CO_POPT_PRINT_OPTION_SELECTION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class CoPoptPrintOptionSelection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_POPT_PRINT_OPTION_SELECTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoPoptPrintOptionSelection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoPoptPrintOptionSelection.
  /// </summary>
  public CoPoptPrintOptionSelection(IContext context, Import import,
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
    // Date      Developer         Request #      Description
    // --------------------------------------------------------------------
    // 12/11/00  Alan Doty                        Initial Development
    // 07/03/08  Jonathan Huss     WR# 5380       Update WORDPFCT code values to
    // WORDPROC (more generic)
    // 08/13/13  GVandy            CQ# 38147      Default Output Format to 
    // WORDPROC-L instead of PRINTER.
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

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

    // *****************************************************************
    // Housekeeping
    // *****************************************************************
    local.Current.Date = Now().Date;

    // *****************************************************************
    // Move Imports to Exports
    // *****************************************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Hidden.Assign(import.Hidden);
    export.Job.TranId = import.Job.TranId;
    MoveJobRun(import.JobRun, export.JobRun);

    if (Equal(global.Command, "CLEAR"))
    {
      global.Command = "DISPLAY";
    }
    else
    {
      MoveOffice(import.SelectedOffice, export.SelectedOffice);
      export.SelectedServiceProvider.Assign(import.SelectedServiceProvider);
      export.SelectedPrinterOutputDestination.VtamPrinterId =
        import.PrinterOutputDestination.VtamPrinterId;
      export.FormatHidden.Cdvalue = import.OutputFormatHidden.Cdvalue;
      export.ServiceProvider.FormattedName =
        import.ServiceProvider.FormattedName;
      export.PromptToCdvl.SelectChar = import.PromptToCdvl.SelectChar;
      export.PromptToOfcl.SelectChar = import.PromptToOcfl.SelectChar;
      export.PromptToSpvl.SelectChar = import.PromptToSpvl.SelectChar;

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
        export.Group.Next();
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // : Set Default Values...
      if (IsEmpty(export.JobRun.OutputType))
      {
        export.JobRun.OutputType = "WORDPROC-L";
      }

      if (IsEmpty(export.SelectedServiceProvider.UserId))
      {
        export.SelectedServiceProvider.UserId = global.UserId;
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
      }

      return;
    }

    // : Handle the return from a prompt.
    switch(TrimEnd(global.Command))
    {
      case "RETCDVL":
        export.JobRun.OutputType = export.FormatHidden.Cdvalue;
        global.Command = "DISPLAY";

        break;
      case "RETOFCL":
        global.Command = "DISPLAY";

        break;
      case "RETSVPL":
        global.Command = "DISPLAY";

        break;
      default:
        break;
    }

    // : Verify the security for the User to be able to execute specific 
    // commands.
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "SUBMIT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "SUBMIT"))
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

      if (local.Common.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }

      if (local.Common.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }
    }

    // : Main case of command.
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        // : Prompt Check
        local.Common.Count = 0;

        if (AsChar(export.PromptToCdvl.SelectChar) == 'S')
        {
          ++local.Common.Count;
        }

        if (AsChar(export.PromptToOfcl.SelectChar) == 'S')
        {
          ++local.Common.Count;
        }

        if (AsChar(export.PromptToSpvl.SelectChar) == 'S')
        {
          ++local.Common.Count;
        }

        switch(local.Common.Count)
        {
          case 0:
            // : PF4 - List pressed with no acceptable prompt entered.
            var field1 = GetField(export.PromptToCdvl, "selectChar");

            field1.Error = true;

            var field2 = GetField(export.PromptToOfcl, "selectChar");

            field2.Error = true;

            var field3 = GetField(export.PromptToSpvl, "selectChar");

            field3.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            return;
          case 1:
            break;
          default:
            var field4 = GetField(export.PromptToCdvl, "selectChar");

            field4.Error = true;

            var field5 = GetField(export.PromptToOfcl, "selectChar");

            field5.Error = true;

            var field6 = GetField(export.PromptToSpvl, "selectChar");

            field6.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
        }

        switch(AsChar(export.PromptToCdvl.SelectChar))
        {
          case 'S':
            export.PromptToCdvl.SelectChar = "";
            export.FormatCode.CodeName = "REPORT OUTPUT FORMAT";
            ExitState = "ECO_LNK_TO_CODE_VALUES";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptToCdvl, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptToOfcl.SelectChar))
        {
          case 'S':
            export.PromptToOfcl.SelectChar = "";
            ExitState = "ECO_LNK_TO_LIST_OFFICE";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptToOfcl, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptToSpvl.SelectChar))
        {
          case 'S':
            export.PromptToSpvl.SelectChar = "";
            ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptToSpvl, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        break;
      case "DISPLAY":
        // : Read Service Provider for the current User.
        if (export.SelectedServiceProvider.SystemGeneratedId == 0)
        {
          export.SelectedServiceProvider.UserId = global.UserId;
        }

        if (ReadServiceProvider3())
        {
          export.SelectedServiceProvider.
            Assign(entities.ExistingServiceProvider);
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
          export.ServiceProvider.FormattedName =
            "** Service Provider not found **";
        }

        // : Read Office for the current User.
        if (export.SelectedOffice.SystemGeneratedId == 0)
        {
          if (ReadOffice1())
          {
            MoveOffice(entities.ExistingOffice, export.SelectedOffice);

            if (ReadPrinterOutputDestination())
            {
              export.SelectedPrinterOutputDestination.VtamPrinterId =
                entities.ExistingPrinterOutputDestination.VtamPrinterId;
            }
            else
            {
              // : No Default Value - Continue Processing.
            }
          }
          else
          {
            export.SelectedOffice.Name = "** Office not found **";
          }
        }
        else if (ReadOffice2())
        {
          MoveOffice(entities.ExistingOffice, export.SelectedOffice);

          if (ReadPrinterOutputDestination())
          {
            export.SelectedPrinterOutputDestination.VtamPrinterId =
              entities.ExistingPrinterOutputDestination.VtamPrinterId;
          }
          else
          {
            // : No Default Value - Continue Processing.
          }
        }
        else
        {
          export.SelectedOffice.Name = "** Office not found **";
        }

        // : Load the Export Group View with Report Names.
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadJob2())
        {
          MoveJob(entities.ExistingJob, export.Group.Update.Job);
          export.Group.Next();
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
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
      case "SUBMIT":
        // : Only one Destination is Allowed.
        switch(TrimEnd(export.JobRun.OutputType))
        {
          case "ONLINE":
            // : No Specific Edits - Continue Processing.
            break;
          case "PRINTER":
            if (IsEmpty(export.SelectedPrinterOutputDestination.VtamPrinterId))
            {
              var field1 =
                GetField(export.SelectedPrinterOutputDestination,
                "vtamPrinterId");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              return;
            }

            local.JobRun.PrinterId =
              export.SelectedPrinterOutputDestination.VtamPrinterId ?? "";

            // jmh 07/03/08  Changed WORDPFCT-P to WORDPROC-P
            break;
          case "WORDPROC-P":
            if (IsEmpty(export.SelectedServiceProvider.EmailAddress))
            {
              var field1 =
                GetField(export.SelectedServiceProvider, "emailAddress");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              return;
            }

            local.JobRun.EmailAddress =
              export.SelectedServiceProvider.EmailAddress ?? "";

            // jmh 07/03/08  Changed WORDPFCT-L to WORDPROC-L
            break;
          case "WORDPROC-L":
            if (IsEmpty(export.SelectedServiceProvider.EmailAddress))
            {
              var field1 =
                GetField(export.SelectedServiceProvider, "emailAddress");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              return;
            }

            local.JobRun.EmailAddress =
              export.SelectedServiceProvider.EmailAddress ?? "";

            break;
          default:
            var field = GetField(export.JobRun, "outputType");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";

            return;
        }

        // jmh 07/03/08  Changed WORDPFCT-L to WORDPROC-L and WORDPFCT-P to 
        // WORDPROC-P
        if (Equal(export.JobRun.OutputType, "WORDPROC-L") || Equal
          (export.JobRun.OutputType, "WORDPROC-P"))
        {
          if (!ReadServiceProvider1())
          {
            ExitState = "GB0000_INVALID_EMAIL_ADDRESS";

            return;
          }
        }

        local.Group.Index = 0;
        local.Group.Clear();

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (local.Group.IsFull)
          {
            break;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if ((Equal(export.Group.Item.Job.Name, "SRRUN250") || Equal
              (export.Group.Item.Job.Name, "SRRUN280") || Equal
              (export.Group.Item.Job.Name, "SRRUN281")) && IsEmpty
              (Substring(export.JobRun.ParmInfo, 12, 12)))
            {
              ExitState = "FN0000_COURT_ORDER_REQ_FOR_FUNC";
              local.Group.Next();

              return;
            }

            local.Job.Name = export.Group.Item.Job.Name;
            local.Group.Next();

            break;
          }

          local.Group.Next();
        }

        if (!ReadServiceProvider2())
        {
          ExitState = "SERVICE_PROVIDER_NF";

          return;
        }

        if (ReadJob1())
        {
          for(local.Loop.Count = 1; local.Loop.Count <= 10; ++local.Loop.Count)
          {
            try
            {
              CreateJobRun();

              goto Read;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  continue;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "JOB_RUN_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          ExitState = "JOB_RUN_AE";

          return;
        }
        else
        {
          ExitState = "JOB_NF";

          return;
        }

Read:

        // : Build JCL and Submit Job.
        local.BatchTimestampWorkArea.IefTimestamp =
          entities.ExistingJobRun.StartTimestamp;
        UseLeCabConvertTimestamp();

        // **** QUALIFY BY OUTPUT_TYPE - NEW ATTRIBUTE ON JCL_TEMPLATE ****
        local.Group.Index = 0;
        local.Group.Clear();

        foreach(var item in ReadJclTemplate())
        {
          local.Common.Count =
            Find(entities.ExistingJclTemplate.RecordText, "&USERID");

          if (local.Common.Count > 0)
          {
            local.Group.Update.JclTemplate.RecordText =
              Substring(entities.ExistingJclTemplate.RecordText,
              JclTemplate.RecordText_MaxLength, 1, local.Common.Count - 1) + TrimEnd
              (global.UserId) + Substring
              (entities.ExistingJclTemplate.RecordText,
              JclTemplate.RecordText_MaxLength, local.Common.Count + 7, 80 - local
              .Common.Count - 7);
            local.Group.Next();

            continue;
          }

          local.Common.Count =
            Find(entities.ExistingJclTemplate.RecordText, "&JOBNAME");

          if (local.Common.Count > 0)
          {
            local.Group.Update.JclTemplate.RecordText =
              Substring(entities.ExistingJclTemplate.RecordText,
              JclTemplate.RecordText_MaxLength, 1, local.Common.Count - 1) + TrimEnd
              (entities.ExistingJob.Name) + Substring
              (entities.ExistingJclTemplate.RecordText,
              JclTemplate.RecordText_MaxLength, local.Common.Count + 8, 80 - local
              .Common.Count - 8);
            local.Group.Next();

            continue;
          }

          local.Common.Count =
            Find(entities.ExistingJclTemplate.RecordText, "&SYSGENID");

          if (local.Common.Count > 0)
          {
            local.Group.Update.JclTemplate.RecordText =
              Substring(entities.ExistingJclTemplate.RecordText,
              JclTemplate.RecordText_MaxLength, 1, local.Common.Count - 1) + NumberToString
              (entities.ExistingJobRun.SystemGenId, 7, 9) + Substring
              (entities.ExistingJclTemplate.RecordText,
              JclTemplate.RecordText_MaxLength, local.Common.Count + 9, 80 - local
              .Common.Count - 9);
            local.Group.Next();

            continue;
          }

          if (Equal(export.JobRun.OutputType, "PRINTER"))
          {
            local.Common.Count =
              Find(entities.ExistingJclTemplate.RecordText, "&PRINTER");

            if (local.Common.Count > 0)
            {
              local.Group.Update.JclTemplate.RecordText =
                Substring(entities.ExistingJclTemplate.RecordText,
                JclTemplate.RecordText_MaxLength, 1, local.Common.Count - 1) + TrimEnd
                (entities.ExistingJobRun.PrinterId) + Substring
                (entities.ExistingJclTemplate.RecordText,
                JclTemplate.RecordText_MaxLength, local.Common.Count + 8, 80 - local
                .Common.Count - 8);
              local.Group.Next();

              continue;
            }
          }

          local.Group.Update.JclTemplate.RecordText =
            entities.ExistingJclTemplate.RecordText;
          local.Group.Next();
        }

        if (local.Group.IsEmpty)
        {
          ExitState = "CO0000_UNABLE_TO_SUBMIT_JOB_R";

          return;
        }

        UseCabSubmitJobToJes();

        switch(local.External.NumericReturnCode)
        {
          case 0:
            ExitState = "CO0000_JOB_SUBMITTED";

            break;
          case 1:
            ExitState = "CO0000_CICS_IO_ERROR";

            break;
          case 2:
            ExitState = "CO0000_CICS_INVALID_REQUEST";

            break;
          case 3:
            ExitState = "CO0000_TRANSID_ERROR";

            break;
          case 4:
            ExitState = "CO0000_CICS_LENGTH_ERROR";

            break;
          case 5:
            ExitState = "CO0000_CICS_NOT_OPEN_ERROR";

            break;
          case 6:
            ExitState = "CO0000_CICS_QUEID_ERROR_R";

            break;
          default:
            ExitState = "CO0000_UNKNOWN_CICS_ERROR_CODE_R";

            break;
        }

        break;
      case "PREQ":
        ExitState = "CO_LNK_TO_PREQ";

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

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveGroup(Local.GroupGroup source,
    CabSubmitJobToJes.Import.GroupGroup target)
  {
    target.JclTemplate.RecordText = source.JclTemplate.RecordText;
  }

  private static void MoveJob(Job source, Job target)
  {
    target.Name = source.Name;
    target.Description = source.Description;
  }

  private static void MoveJobRun(JobRun source, JobRun target)
  {
    target.OutputType = source.OutputType;
    target.ParmInfo = source.ParmInfo;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private void UseCabSubmitJobToJes()
  {
    var useImport = new CabSubmitJobToJes.Import();
    var useExport = new CabSubmitJobToJes.Export();

    local.Group.CopyTo(useImport.Group, MoveGroup);
    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(CabSubmitJobToJes.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
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

  private void CreateJobRun()
  {
    var startTimestamp = Now();
    var status = "REQUESTED";
    var spdSrvcPrvderId = entities.ExistingServiceProvider.SystemGeneratedId;
    var printerId = local.JobRun.PrinterId ?? "";
    var outputType = export.JobRun.OutputType;
    var emailAddress = local.JobRun.EmailAddress ?? "";
    var parmInfo = export.JobRun.ParmInfo ?? "";
    var jobName = entities.ExistingJob.Name;
    var systemGenId = UseGenerate9DigitRandomNumber();

    entities.ExistingJobRun.Populated = false;
    Update("CreateJobRun",
      (db, command) =>
      {
        db.SetDateTime(command, "startTimestamp", startTimestamp);
        db.SetNullableDateTime(command, "endTimestamp", null);
        db.SetNullableString(command, "zdelUserId", "");
        db.SetNullableString(command, "zdelPersonNumber", "");
        db.SetNullableInt32(command, "zdelLegActionId", 0);
        db.SetString(command, "status", status);
        db.SetNullableInt32(command, "spdSrvcPrvderId", spdSrvcPrvderId);
        db.SetNullableString(command, "printerId", printerId);
        db.SetString(command, "outputType", outputType);
        db.SetNullableString(command, "errorMsg", "");
        db.SetNullableString(command, "emailAddress", emailAddress);
        db.SetNullableString(command, "parmInfo", parmInfo);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "systemGenId", systemGenId);
      });

    entities.ExistingJobRun.StartTimestamp = startTimestamp;
    entities.ExistingJobRun.EndTimestamp = null;
    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.SpdSrvcPrvderId = spdSrvcPrvderId;
    entities.ExistingJobRun.PrinterId = printerId;
    entities.ExistingJobRun.OutputType = outputType;
    entities.ExistingJobRun.EmailAddress = emailAddress;
    entities.ExistingJobRun.ParmInfo = parmInfo;
    entities.ExistingJobRun.JobName = jobName;
    entities.ExistingJobRun.SystemGenId = systemGenId;
    entities.ExistingJobRun.Populated = true;
  }

  private IEnumerable<bool> ReadJclTemplate()
  {
    return ReadEach("ReadJclTemplate",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJob.Name);
        db.SetString(command, "outputType", entities.ExistingJobRun.OutputType);
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
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
        db.SetString(command, "name", local.Job.Name);
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
        db.SetString(command, "tranId", export.Job.TranId);
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

  private bool ReadOffice1()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.Name = db.GetString(reader, 1);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 2);
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadOffice2()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice2",
      (db, command) =>
      {
        db.
          SetInt32(command, "officeId", export.SelectedOffice.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.Name = db.GetString(reader, 1);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 2);
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadPrinterOutputDestination()
  {
    entities.ExistingPrinterOutputDestination.Populated = false;

    return Read("ReadPrinterOutputDestination",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGenerated", entities.ExistingOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingPrinterOutputDestination.PrinterId =
          db.GetString(reader, 0);
        entities.ExistingPrinterOutputDestination.OffGenerated =
          db.GetNullableInt32(reader, 1);
        entities.ExistingPrinterOutputDestination.DefaultInd =
          db.GetNullableString(reader, 2);
        entities.ExistingPrinterOutputDestination.VtamPrinterId =
          db.GetNullableString(reader, 3);
        entities.ExistingPrinterOutputDestination.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "emailAddress",
          export.SelectedServiceProvider.EmailAddress ?? "");
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
        entities.ExistingServiceProvider.EmailAddress =
          db.GetNullableString(reader, 5);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
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
        entities.ExistingServiceProvider.EmailAddress =
          db.GetNullableString(reader, 5);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider3()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider3",
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
        entities.ExistingServiceProvider.EmailAddress =
          db.GetNullableString(reader, 5);
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
    /// A value of PromptToCdvl.
    /// </summary>
    [JsonPropertyName("promptToCdvl")]
    public Common PromptToCdvl
    {
      get => promptToCdvl ??= new();
      set => promptToCdvl = value;
    }

    /// <summary>
    /// A value of SelectedOffice.
    /// </summary>
    [JsonPropertyName("selectedOffice")]
    public Office SelectedOffice
    {
      get => selectedOffice ??= new();
      set => selectedOffice = value;
    }

    /// <summary>
    /// A value of PrinterOutputDestination.
    /// </summary>
    [JsonPropertyName("printerOutputDestination")]
    public PrinterOutputDestination PrinterOutputDestination
    {
      get => printerOutputDestination ??= new();
      set => printerOutputDestination = value;
    }

    /// <summary>
    /// A value of PromptToOcfl.
    /// </summary>
    [JsonPropertyName("promptToOcfl")]
    public Common PromptToOcfl
    {
      get => promptToOcfl ??= new();
      set => promptToOcfl = value;
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
    /// A value of OutputFormatHidden.
    /// </summary>
    [JsonPropertyName("outputFormatHidden")]
    public CodeValue OutputFormatHidden
    {
      get => outputFormatHidden ??= new();
      set => outputFormatHidden = value;
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
    private JobRun jobRun;
    private Array<GroupGroup> group;
    private Common promptToCdvl;
    private Office selectedOffice;
    private PrinterOutputDestination printerOutputDestination;
    private Common promptToOcfl;
    private ServiceProvider selectedServiceProvider;
    private CsePersonsWorkSet serviceProvider;
    private Common promptToSpvl;
    private CodeValue outputFormatHidden;
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
    /// A value of PromptToCdvl.
    /// </summary>
    [JsonPropertyName("promptToCdvl")]
    public Common PromptToCdvl
    {
      get => promptToCdvl ??= new();
      set => promptToCdvl = value;
    }

    /// <summary>
    /// A value of SelectedOffice.
    /// </summary>
    [JsonPropertyName("selectedOffice")]
    public Office SelectedOffice
    {
      get => selectedOffice ??= new();
      set => selectedOffice = value;
    }

    /// <summary>
    /// A value of SelectedPrinterOutputDestination.
    /// </summary>
    [JsonPropertyName("selectedPrinterOutputDestination")]
    public PrinterOutputDestination SelectedPrinterOutputDestination
    {
      get => selectedPrinterOutputDestination ??= new();
      set => selectedPrinterOutputDestination = value;
    }

    /// <summary>
    /// A value of PromptToOfcl.
    /// </summary>
    [JsonPropertyName("promptToOfcl")]
    public Common PromptToOfcl
    {
      get => promptToOfcl ??= new();
      set => promptToOfcl = value;
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
    /// A value of SelectedServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedServiceProvider")]
    public ServiceProvider SelectedServiceProvider
    {
      get => selectedServiceProvider ??= new();
      set => selectedServiceProvider = value;
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
    /// A value of FormatCode.
    /// </summary>
    [JsonPropertyName("formatCode")]
    public Code FormatCode
    {
      get => formatCode ??= new();
      set => formatCode = value;
    }

    /// <summary>
    /// A value of FormatHidden.
    /// </summary>
    [JsonPropertyName("formatHidden")]
    public CodeValue FormatHidden
    {
      get => formatHidden ??= new();
      set => formatHidden = value;
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
    private JobRun jobRun;
    private Array<GroupGroup> group;
    private Common promptToCdvl;
    private Office selectedOffice;
    private PrinterOutputDestination selectedPrinterOutputDestination;
    private Common promptToOfcl;
    private Common promptToSpvl;
    private ServiceProvider selectedServiceProvider;
    private CsePersonsWorkSet serviceProvider;
    private Code formatCode;
    private CodeValue formatHidden;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of Loop.
    /// </summary>
    [JsonPropertyName("loop")]
    public Common Loop
    {
      get => loop ??= new();
      set => loop = value;
    }

    private External external;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Job job;
    private JobRun jobRun;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common common;
    private DateWorkArea current;
    private Array<GroupGroup> group;
    private Common loop;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingPrinterOutputDestination.
    /// </summary>
    [JsonPropertyName("existingPrinterOutputDestination")]
    public PrinterOutputDestination ExistingPrinterOutputDestination
    {
      get => existingPrinterOutputDestination ??= new();
      set => existingPrinterOutputDestination = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
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
    /// A value of ExistingJclTemplate.
    /// </summary>
    [JsonPropertyName("existingJclTemplate")]
    public JclTemplate ExistingJclTemplate
    {
      get => existingJclTemplate ??= new();
      set => existingJclTemplate = value;
    }

    private Office existingOffice;
    private PrinterOutputDestination existingPrinterOutputDestination;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private ServiceProvider existingServiceProvider;
    private Job existingJob;
    private JobRun existingJobRun;
    private JclTemplate existingJclTemplate;
  }
#endregion
}
