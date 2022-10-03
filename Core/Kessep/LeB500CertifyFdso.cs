// Program: LE_B500_CERTIFY_FDSO, ID: 372665268, model: 746.
// Short name: SWEL500B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_B500_CERTIFY_FDSO.
/// </para>
/// <para>
/// Read thru database for obligor's that meet FDSO reporting guidelines. 
/// Distinction is made between insert or update and ADC or NON-ADC.
/// Objective is to build the federal_debt_setoff subtype, one instance for ADC 
/// and another for NON-ADC.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB500CertifyFdso: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B500_CERTIFY_FDSO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB500CertifyFdso(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB500CertifyFdso.
  /// </summary>
  public LeB500CertifyFdso(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // ---------------------------------------------------------
    // ??/??/????  ????????	?????		Initial Coding
    // 01/09/2001  EShirk	PR131995	Fully enable restart functionality
    // 01/21/2004  CMJohnson	PR00162643	FDSO certified obligors newly marked for
    // family
    // 					violence should be decertified.  Do not re-certify
    // 					the obligor until family violence is removed.
    // 01/13/2006  M.J.Quinn	WR258945	Allow for FDSO certification of bankruptcy
    // 07/25/2007  GVandy	PR313068	Program was  re-structured/re-written to 
    // correctly
    // 					send transactions per case type (i.e. adc verses non-adc).
    // 					Also added lots of formatting, comments, cleanup,
    // 					and general error checking.
    // 08/09/2019  GVandy	CQ66204		Correct Checkpoint logic.
    // -------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -----------------------------------------------------------------------------------------------
    // Retrieve the PPI info.
    // -----------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = global.TranCode;
    local.NeededToOpen.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Control Report
    // -----------------------------------------------------------------------------------------------
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Bankruptcy Report
    // -----------------------------------------------------------------------------------------------
    local.NeededToOpen.BlankLineAfterHeading = "Y";
    local.NeededToOpen.RptHeading3 =
      "          Bankruptcy Exclusion Report for FDSO Obligors";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening Bankruptcy report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Get the DB2 commit frequency counts and determine if we are restarting.
    // -----------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -- Extract the restart person number.
      local.Starting.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);

      // -- Write the restart info to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.NeededToWrite.RptDetail =
              "Process restarting from person number -   " + local
              .Starting.Number;

            break;
          case 2:
            local.NeededToWrite.RptDetail = "";

            break;
          default:
            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing Restart Person Number to control report.";
            
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Extract parameters from the PPI record.
    // -----------------------------------------------------------------------------------------------
    if (Find(local.ProgramProcessingInfo.ParameterList, "DISPLAY") == 0)
    {
    }
    else
    {
      local.Display.Flag = "Y";

      // -- Write Month End to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.NeededToWrite.RptDetail =
              "DISPLAY (Display Certification Detail) was specified on the PPI record.";
              

            break;
          case 2:
            local.NeededToWrite.RptDetail = "";

            break;
          default:
            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing Month End info to control report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Retrieve family violence application indicator.
    // -----------------------------------------------------------------------------------------------
    if (ReadCodeValue())
    {
      local.FviApply.Flag = Substring(entities.CodeValue.Description, 1, 1);
    }
    else
    {
      local.FviApply.Flag = "N";
    }

    local.IncludeBankruptcy.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 9, 1);

    // -----------------------------------------------------------------------------------------------
    // Read each obligor and process against FDSO certification requirements.
    // -----------------------------------------------------------------------------------------------
    foreach(var item in ReadCsePerson())
    {
      ++local.NumberOfReads.Count;
      UseLeProcessFdsoCertification();

      if (AsChar(local.Display.Flag) == 'Y' || AsChar(local.Abort.Flag) == 'Y'
        || !IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (Length(TrimEnd(local.NeededToWrite.RptDetail)) > 20)
        {
          // -- Write whatever report information was returned from the fdso 
          // certification cab to the error report.
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        if (AsChar(local.Abort.Flag) == 'Y' || !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.NeededToWrite.RptDetail = "Obligor " + entities
            .CsePerson.Number + " - " + local.ExitStateWorkArea.Message;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        if (AsChar(local.Abort.Flag) == 'Y')
        {
          if (IsExitState("FN0000_ERROR_WRITING_ERROR_RPT"))
          {
            // -- Don't reset the exit state. No message was written to the 
            // error report and
            //    this exit state will provide a more descriptive abend message.
          }
          else
          {
            ExitState = "ACO_AE0000_BATCH_ABEND";
          }

          // -- Flush the buffer before abending...
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport3();

          return;
        }
      }

      ExitState = "ACO_NN0000_ALL_OK";

      // --  Check for commit point.
      if (local.NumberOfReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // --  Update program checkpoint restart with current commit position.
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = entities.CsePerson.Number;
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Update of checkpoint restart failed at person number -   " + entities
            .CsePerson.Number;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Commit Failed at person number -   " + entities.CsePerson.Number;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.NumberOfReads.Count = 0;

        // --Log the number of checkpoints to the error report.
        local.EabFileHandling.Action = "WRITE";
        ++local.NumberOfCommits.Count;
        local.NeededToWrite.RptDetail = "Number of commits performed : " + NumberToString
          (local.NumberOfCommits.Count, 15) + " At Person Number : " + entities
          .CsePerson.Number;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Write number of fdso records created to the control report.
    // -----------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.NeededToWrite.RptDetail =
            "Number of FDSO records created - " + NumberToString
            (local.NumFdsoRecsCreated.Count, 6, 10);

          break;
        case 2:
          local.NeededToWrite.RptDetail = "";

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing Totals to control report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Take a final checkpoint.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.CheckpointCount = -1;
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Final update of checkpoint restart table failed.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Bankruptcy Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered while closing Bankruptcy report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Control Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered while closing control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    MoveEabReportSend1(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend2(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend2(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeProcessFdsoCertification()
  {
    var useImport = new LeProcessFdsoCertification.Import();
    var useExport = new LeProcessFdsoCertification.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.IncludeBankruptcy.Flag = local.IncludeBankruptcy.Flag;
    useImport.FviApplication.Flag = local.FviApply.Flag;
    useExport.NumFdsoRecsCreated.Count = local.NumFdsoRecsCreated.Count;

    Call(LeProcessFdsoCertification.Execute, useImport, useExport);

    local.NeededToWrite.RptDetail = useExport.EabReportSend.RptDetail;
    local.NumFdsoRecsCreated.Count = useExport.NumFdsoRecsCreated.Count;
    local.Abort.Flag = useExport.Abort.Flag;
  }

  private void UseReadPgmChkpntRestart()
  {
    var useImport = new ReadPgmChkpntRestart.Import();
    var useExport = new ReadPgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmChkpntRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      null,
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Starting.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

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
    /// A value of IncludeBankruptcy.
    /// </summary>
    [JsonPropertyName("includeBankruptcy")]
    public Common IncludeBankruptcy
    {
      get => includeBankruptcy ??= new();
      set => includeBankruptcy = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of NumberOfReads.
    /// </summary>
    [JsonPropertyName("numberOfReads")]
    public Common NumberOfReads
    {
      get => numberOfReads ??= new();
      set => numberOfReads = value;
    }

    /// <summary>
    /// A value of NumFdsoRecsCreated.
    /// </summary>
    [JsonPropertyName("numFdsoRecsCreated")]
    public Common NumFdsoRecsCreated
    {
      get => numFdsoRecsCreated ??= new();
      set => numFdsoRecsCreated = value;
    }

    /// <summary>
    /// A value of NumberOfCommits.
    /// </summary>
    [JsonPropertyName("numberOfCommits")]
    public Common NumberOfCommits
    {
      get => numberOfCommits ??= new();
      set => numberOfCommits = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public Common Display
    {
      get => display ??= new();
      set => display = value;
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
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CsePerson Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of FviApply.
    /// </summary>
    [JsonPropertyName("fviApply")]
    public Common FviApply
    {
      get => fviApply ??= new();
      set => fviApply = value;
    }

    private Common includeBankruptcy;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common numberOfReads;
    private Common numFdsoRecsCreated;
    private Common numberOfCommits;
    private ExitStateWorkArea exitStateWorkArea;
    private Common display;
    private Common common;
    private Common abort;
    private External passArea;
    private CsePerson starting;
    private Common fviApply;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private CodeValue codeValue;
    private Code code;
  }
#endregion
}
