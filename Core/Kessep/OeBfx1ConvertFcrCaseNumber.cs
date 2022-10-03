// Program: OE_BFX1_CONVERT_FCR_CASE_NUMBER, ID: 371212946, model: 746.
// Short name: SWEEFX1B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_BFX1_CONVERT_FCR_CASE_NUMBER.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeBfx1ConvertFcrCaseNumber: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_BFX1_CONVERT_FCR_CASE_NUMBER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeBfx1ConvertFcrCaseNumber(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeBfx1ConvertFcrCaseNumber.
  /// </summary>
  public OeBfx1ConvertFcrCaseNumber(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";

    // ----------------------------------------------------------------------------------
    // -- Open Control Report.
    // ----------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_OPENING_CTL_RPT_FILE";

      return;
    }

    // ----------------------------------------------------------------------------------
    // -- Open Error Report.
    // ----------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

      return;
    }

    local.TotalRead.Count = 0;
    local.TotalUpdate.Count = 0;
    local.Common.Count = 0;

    // ----------------------------------------------------------------------------------
    // -- Read each FCR_Proactive_Match_Response record.
    // ----------------------------------------------------------------------------------
    foreach(var item in ReadFcrProactiveMatchResponse())
    {
      ++local.TotalRead.Count;

      // ----------------------------------------------------------------------------------
      // -- If this is a IV-D case then update the submitted case id.
      // ----------------------------------------------------------------------------------
      if (Equal(entities.FcrProactiveMatchResponse.SubmittedCaseId, 1, 3, "000") &&
        !
        IsEmpty(Substring(
          entities.FcrProactiveMatchResponse.SubmittedCaseId, 11, 5)))
      {
        // ----------------------------------------------------------------------------------
        // -- Remove leading zeros and add 5 trailing spaces.
        // ----------------------------------------------------------------------------------
        local.FcrProactiveMatchResponse.SubmittedCaseId =
          Substring(entities.FcrProactiveMatchResponse.SubmittedCaseId,
          FcrProactiveMatchResponse.SubmittedCaseId_MaxLength, 6, 10) + "     ";
          

        try
        {
          UpdateFcrProactiveMatchResponse();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_PROACTIVE_MATCH_RESPONSE_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_PROACTIVE_MATCH_RESPONSE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        ++local.TotalUpdate.Count;
        ++local.Common.Count;

        if (local.Common.Count >= 500)
        {
          // ----------------------------------------------------------------------------------
          // -- Commit changes.
          // ----------------------------------------------------------------------------------
          UseExtToDoACommit();

          if (local.External.NumericReturnCode != 0)
          {
            ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

            break;
          }

          local.Common.Count = 0;
        }
      }
    }

    // ----------------------------------------------------------------------------------
    // -- Do a final Commit.
    // ----------------------------------------------------------------------------------
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";
    }

    // ----------------------------------------------------------------------------------
    // -- Write read & update counts to the control report.
    // ----------------------------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "FCR Proactive Match Responses -- Read: " + NumberToString
      (local.TotalRead.Count, 15) + "  Updated: " + NumberToString
      (local.TotalUpdate.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";
    }

    // ----------------------------------------------------------------------------------
    // -- Write completion status to the control report.
    // ----------------------------------------------------------------------------------
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabReportSend.RptDetail = "Updates successfully completed.";
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Updates NOT successfully completed.  See error report for details.";
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ----------------------------------------------------------------------------------
      // -- Write error to the error report.
      // ----------------------------------------------------------------------------------
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // ----------------------------------------------------------------------------------
      // -- Set an abort exit state...
      // ----------------------------------------------------------------------------------
      ExitState = "ACO_NN0000_ABEND_4_BATCH";
    }

    // ----------------------------------------------------------------------------------
    // -- Close Control Report.
    // ----------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      return;
    }

    // ----------------------------------------------------------------------------------
    // -- Close Error Report.
    // ----------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";
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

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
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

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private IEnumerable<bool> ReadFcrProactiveMatchResponse()
  {
    entities.FcrProactiveMatchResponse.Populated = false;

    return ReadEach("ReadFcrProactiveMatchResponse",
      null,
      (db, reader) =>
      {
        entities.FcrProactiveMatchResponse.SubmittedCaseId =
          db.GetNullableString(reader, 0);
        entities.FcrProactiveMatchResponse.Identifier = db.GetInt32(reader, 1);
        entities.FcrProactiveMatchResponse.Populated = true;

        return true;
      });
  }

  private void UpdateFcrProactiveMatchResponse()
  {
    var submittedCaseId = local.FcrProactiveMatchResponse.SubmittedCaseId ?? "";

    entities.FcrProactiveMatchResponse.Populated = false;
    Update("UpdateFcrProactiveMatchResponse",
      (db, command) =>
      {
        db.SetNullableString(command, "submittedCaseId", submittedCaseId);
        db.SetInt32(
          command, "identifier", entities.FcrProactiveMatchResponse.Identifier);
          
      });

    entities.FcrProactiveMatchResponse.SubmittedCaseId = submittedCaseId;
    entities.FcrProactiveMatchResponse.Populated = true;
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
    /// A value of TotalRead.
    /// </summary>
    [JsonPropertyName("totalRead")]
    public Common TotalRead
    {
      get => totalRead ??= new();
      set => totalRead = value;
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
    /// A value of TotalUpdate.
    /// </summary>
    [JsonPropertyName("totalUpdate")]
    public Common TotalUpdate
    {
      get => totalUpdate ??= new();
      set => totalUpdate = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    private Common totalRead;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalUpdate;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common common;
    private External external;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
  }
#endregion
}
