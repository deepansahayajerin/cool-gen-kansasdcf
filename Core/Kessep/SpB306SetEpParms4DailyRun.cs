// Program: SP_B306_SET_EP_PARMS_4_DAILY_RUN, ID: 372236582, model: 746.
// Short name: SWEP306B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B306_SET_EP_PARMS_4_DAILY_RUN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB306SetEpParms4DailyRun: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B306_SET_EP_PARMS_4_DAILY_RUN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB306SetEpParms4DailyRun(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB306SetEpParms4DailyRun.
  /// </summary>
  public SpB306SetEpParms4DailyRun(IContext context, Import import,
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
    // ***********************************************************************************************
    // * Date          Developer       Request #      Description
    // ***********************************************************************************************
    // * 07 Mar 99     John C Crook    -              Intital Dev
    // *
    // * 19 Oct 10     GVandy		CQ966 		Modify format of event processor parms
    // ***********************************************************************************************
    // *****************************************************************
    // Purpose of this procedure is to set the run parm of the
    // Program_Processing_Info Table to control the Event Processor
    // behavior during the DAY-TIME hours
    // ********************************************
    // Crook  07 Mar 99 ***
    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    // Get the run parameter for Event Processor - SWEPB306
    // ********************************************
    // Crook  07 Mar 99 ***
    local.ProgramProcessingInfo.Name = "SWEPB306";
    UseReadProgramProcessingInfo();
    local.Specified.ParameterList = "FULL   ";

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Specified.ParameterList = "FULL   ";
      ExitState = "ACO_NN0000_ALL_OK";
    }
    else if (Equal(local.ProgramProcessingInfo.ParameterList, 1, 6, "ALERTS"))
    {
      local.Specified.ParameterList = "ALERTS ";
    }
    else if (Equal(local.ProgramProcessingInfo.ParameterList, 1, 4, "FULL"))
    {
      local.Specified.ParameterList = "FULL   ";
    }
    else if (Equal(local.ProgramProcessingInfo.ParameterList, 1, 3, "END"))
    {
      local.Specified.ParameterList = "END    ";
    }
    else
    {
      local.Specified.ParameterList = "FULL   ";
    }

    // *****************************************************************
    // Get the run parameters for Event Processor - SWEPB301
    // ********************************************
    // Crook  07 Mar 99 ***
    local.ProgramProcessingInfo.Name = "SWEPB301";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *****************************************************************
    // Program_Processing_Information (Parameter_List)
    // Refer to Event Processor
    // ********************************************
    // Crook  17 Mar 99 ***
    local.ProgramProcessingInfo.ParameterList =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 33) + Substring
      (local.Specified.ParameterList, 1, 7) + Substring
      (local.ProgramProcessingInfo.ParameterList, 41, 199);

    // *****************************************************************
    // Update the run parameters for Event Processor - SWEPB301
    // ********************************************
    // Crook  07 Mar 99 ***
    UseUpdateProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // This is here for no other reason that to get this procedure to take a 
    // commit
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
    }
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Specified.
    /// </summary>
    [JsonPropertyName("specified")]
    public ProgramProcessingInfo Specified
    {
      get => specified ??= new();
      set => specified = value;
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
    /// A value of TbdLocalWork.
    /// </summary>
    [JsonPropertyName("tbdLocalWork")]
    public ProgramProcessingInfo TbdLocalWork
    {
      get => tbdLocalWork ??= new();
      set => tbdLocalWork = value;
    }

    /// <summary>
    /// A value of TbdLocalWaitSeconds.
    /// </summary>
    [JsonPropertyName("tbdLocalWaitSeconds")]
    public TextWorkArea TbdLocalWaitSeconds
    {
      get => tbdLocalWaitSeconds ??= new();
      set => tbdLocalWaitSeconds = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramProcessingInfo specified;
    private External passArea;
    private ProgramProcessingInfo tbdLocalWork;
    private TextWorkArea tbdLocalWaitSeconds;
  }
#endregion
}
