// Program: FN_B710_HOUSEKEEPING, ID: 374439588, model: 746.
// Short name: SWE02550
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B710_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB710Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B710_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB710Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB710Housekeeping.
  /// </summary>
  public FnB710Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // --------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEFB710";
    export.Current.Timestamp = Now();
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      export.ProgramProcessingInfo);

    // mjr---> If ppi process_date is null, set it to current date
    if (!Lt(local.Null1.Date, export.ProgramProcessingInfo.ProcessDate))
    {
      export.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    // --------------------------------------------------------------------
    // SET RUNTIME PARAMETERS TO DEFAULTS
    // --------------------------------------------------------------------
    export.DebugOn.Flag = "N";
    local.BatchTimestampWorkArea.TextDate = "00010101";
    export.CreateHistory.Flag = "N";

    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG");

      if (local.Position.Count > 0)
      {
        export.DebugOn.Flag = "Y";
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "START:");

      if (local.Position.Count > 0)
      {
        // mjr---> Retrieve parameter into local views
        local.BatchTimestampWorkArea.TextDateMm =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 6, 2);
        local.BatchTimestampWorkArea.TestDateDd =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 8, 2);
        local.BatchTimestampWorkArea.TextDateYyyy =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 10, 4);

        // mjr---> Validate local views
        if (Lt(local.BatchTimestampWorkArea.TextDateYyyy, "0001") || Lt
          ("2099", local.BatchTimestampWorkArea.TextDateYyyy))
        {
          goto Test;
        }

        if (Lt(local.BatchTimestampWorkArea.TextDateMm, "01") || Lt
          ("12", local.BatchTimestampWorkArea.TextDateMm))
        {
          goto Test;
        }

        if (Lt(local.BatchTimestampWorkArea.TestDateDd, "01") || Lt
          ("31", local.BatchTimestampWorkArea.TestDateDd))
        {
          goto Test;
        }

        if (Lt("30", local.BatchTimestampWorkArea.TestDateDd) && (
          Equal(local.BatchTimestampWorkArea.TextDateMm, "04") || Equal
          (local.BatchTimestampWorkArea.TextDateMm, "06") || Equal
          (local.BatchTimestampWorkArea.TextDateMm, "09") || Equal
          (local.BatchTimestampWorkArea.TextDateMm, "11")))
        {
          goto Test;
        }

        // mjr--->  LEAP YEAR check
        if (Lt("28", local.BatchTimestampWorkArea.TestDateDd) && Equal
          (local.BatchTimestampWorkArea.TextDateMm, "02"))
        {
          local.Calc.Count =
            (int)StringToNumber(local.BatchTimestampWorkArea.TextDateYyyy);
          local.Calc.TotalReal = (decimal)local.Calc.Count / 4;
          local.Calc.TotalInteger = local.Calc.Count / 4;

          if (local.Calc.TotalInteger != local.Calc.TotalReal)
          {
            goto Test;
          }

          local.Calc.TotalReal = (decimal)local.Calc.Count / 100;
          local.Calc.TotalInteger = local.Calc.Count / 100;

          if (local.Calc.TotalInteger == local.Calc.TotalReal)
          {
            local.Calc.TotalReal = (decimal)local.Calc.Count / 400;
            local.Calc.TotalInteger = local.Calc.Count / 400;

            if (local.Calc.TotalInteger != local.Calc.TotalReal)
            {
              goto Test;
            }
          }
        }

        // mjr---> Construct text date
        local.BatchTimestampWorkArea.TextDate =
          local.BatchTimestampWorkArea.TextDateYyyy + local
          .BatchTimestampWorkArea.TextDateMm + local
          .BatchTimestampWorkArea.TestDateDd;

        // mjr---> Construct ief date
        export.CollectionStart.Date =
          IntToDate((int)StringToNumber(local.BatchTimestampWorkArea.TextDate));
          
      }

Test:

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "HISTORY");

      if (local.Position.Count > 0)
      {
        export.CreateHistory.Flag = "Y";
      }
    }

    // --------------------------------------------------------------------
    // DETERMINE IF THIS IS A RESTART SITUATION
    // --------------------------------------------------------------------
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // --------------------------------------------------------------------
      // EXTRACT RESTART PARAMETERS FROM RESTART_INFO
      // --------------------------------------------------------------------
    }

    // -------------------------------------------------------
    // OPEN OUTPUT ERROR REPORT 99
    // -------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ------------------------------------------------------------
    // OPEN OUTPUT CONTROL REPORT 98
    // ------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO ERROR REPORT 99
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO CONTROL REPORT 98
    // -----------------------------------------------------------
    local.EabReportSend.RptDetail =
      "Collections created date must be greater or equal to " + local
      .BatchTimestampWorkArea.TextDate;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabReportSend.RptDetail = "Log to History:  " + export
      .CreateHistory.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // GET LITERALS
    // -----------------------------------------------------------
    if (AsChar(export.CreateHistory.Flag) == 'Y')
    {
      if (!ReadEvent())
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Event not found; Event ID = 34";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      if (!ReadEventDetail())
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Event Detail not found; Reason Code  = KPCNOTICESENT";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      export.Infrastructure.EventId = entities.Event1.ControlNumber;
      export.Infrastructure.ReasonCode = entities.EventDetail.ReasonCode;
      export.Infrastructure.ProcessStatus = "Q";
      export.Infrastructure.ReferenceDate =
        export.ProgramProcessingInfo.ProcessDate;
      export.Infrastructure.CsenetInOutCode =
        entities.EventDetail.CsenetInOutCode;
      export.Infrastructure.InitiatingStateCode =
        entities.EventDetail.InitiatingStateCode;
      export.Infrastructure.BusinessObjectCd =
        entities.Event1.BusinessObjectCode;
    }

    export.Trigger.Type1 = "KPC";
    export.Trigger.Action = "";
    export.Trigger.Status = "";
    export.Trigger.CreatedBy = export.ProgramProcessingInfo.Name;
    export.Trigger.CreatedTimestamp = export.Current.Timestamp;
    export.Trigger.LastUpdatedBy = export.ProgramProcessingInfo.Name;
    local.BatchTimestampWorkArea.TextDate =
      NumberToString(DateToInt(export.ProgramProcessingInfo.ProcessDate), 8);
    local.BatchTimestampWorkArea.TextDateYyyy =
      Substring(local.BatchTimestampWorkArea.TextDate, 1, 4);
    local.BatchTimestampWorkArea.TextDateMm =
      Substring(local.BatchTimestampWorkArea.TextDate, 5, 2);
    local.BatchTimestampWorkArea.TestDateDd =
      Substring(local.BatchTimestampWorkArea.TextDate, 7, 2);
    export.Trigger.UpdatedTimestamp =
      Timestamp(local.BatchTimestampWorkArea.TextDateYyyy + "-" + local
      .BatchTimestampWorkArea.TextDateMm + "-" + local
      .BatchTimestampWorkArea.TestDateDd + "-00.00.00.000000");
    local.Code.CodeName = "TRIGGER ACTION";
    local.CodeValue.Cdvalue = "BLANK";
    UseCabValidateCodeValue1();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Invalid TRIGGER ACTION; Code  = BLANK";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.Code.CodeName = "TRIGGER STATUS";
    local.CodeValue.Cdvalue = "BLANK";
    UseCabValidateCodeValue1();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Invalid TRIGGER STATUS; Code  = BLANK";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.Code.CodeName = "TRIGGER TYPE";
    local.CodeValue.Cdvalue = export.Trigger.Type1;
    UseCabValidateCodeValue1();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Invalid TRIGGER TYPE; Code  = " + export.Trigger.Type1;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.Code.CodeName = "TRIGGER TYPE";
    local.CodeValue.Cdvalue = export.Trigger.Type1;
    local.CrossValidationCode.CodeName = "TRIGGER ACTION";
    local.CrossValidationCodeValue.Cdvalue = "BLANK";
    UseCabValidateCodeValue2();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Invalid TRIGGER TYPE - TRIGGER ACTION Combination; TRIGGER TYPE Code  = " +
        TrimEnd(export.Trigger.Type1) + ", TRIGGER ACTION Code = BLANK";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.Code.CodeName = "TRIGGER TYPE";
    local.CodeValue.Cdvalue = export.Trigger.Type1;
    local.CrossValidationCode.CodeName = "TRIGGER STATUS";
    local.CrossValidationCodeValue.Cdvalue = "BLANK";
    UseCabValidateCodeValue2();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Invalid TRIGGER TYPE - TRIGGER STATUS Combination; TRIGGER TYPE Code  = " +
        TrimEnd(export.Trigger.Type1) + ", TRIGGER STATUS Code = BLANK";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.CrossValidationCode.CodeName = local.CrossValidationCode.CodeName;
    useImport.CrossValidationCodeValue.Cdvalue =
      local.CrossValidationCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadEvent()
  {
    entities.Event1.Populated = false;

    return Read("ReadEvent",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.Event1.BusinessObjectCode = db.GetString(reader, 1);
        entities.Event1.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.Event1.ControlNumber);
        db.SetDate(
          command, "effectiveDate",
          export.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 1);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 2);
        entities.EventDetail.ReasonCode = db.GetString(reader, 3);
        entities.EventDetail.EffectiveDate = db.GetDate(reader, 4);
        entities.EventDetail.DiscontinueDate = db.GetDate(reader, 5);
        entities.EventDetail.EveNo = db.GetInt32(reader, 6);
        entities.EventDetail.Populated = true;
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
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of CollectionStart.
    /// </summary>
    [JsonPropertyName("collectionStart")]
    public DateWorkArea CollectionStart
    {
      get => collectionStart ??= new();
      set => collectionStart = value;
    }

    /// <summary>
    /// A value of CreateHistory.
    /// </summary>
    [JsonPropertyName("createHistory")]
    public Common CreateHistory
    {
      get => createHistory ??= new();
      set => createHistory = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Common debugOn;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea collectionStart;
    private Common createHistory;
    private Infrastructure infrastructure;
    private Trigger trigger;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Calc.
    /// </summary>
    [JsonPropertyName("calc")]
    public Common Calc
    {
      get => calc ??= new();
      set => calc = value;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of CrossValidationCode.
    /// </summary>
    [JsonPropertyName("crossValidationCode")]
    public Code CrossValidationCode
    {
      get => crossValidationCode ??= new();
      set => crossValidationCode = value;
    }

    /// <summary>
    /// A value of CrossValidationCodeValue.
    /// </summary>
    [JsonPropertyName("crossValidationCodeValue")]
    public CodeValue CrossValidationCodeValue
    {
      get => crossValidationCodeValue ??= new();
      set => crossValidationCodeValue = value;
    }

    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common calc;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Code crossValidationCode;
    private CodeValue crossValidationCodeValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ProgramProcessingInfo Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private EventDetail eventDetail;
    private Event1 event1;
    private ProgramProcessingInfo zdel;
  }
#endregion
}
