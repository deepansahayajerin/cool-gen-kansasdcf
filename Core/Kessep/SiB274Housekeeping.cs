// Program: SI_B274_HOUSEKEEPING, ID: 371072739, model: 746.
// Short name: SWE01284
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B274_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SiB274Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B274_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB274Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB274Housekeeping.
  /// </summary>
  public SiB274Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // **********************************************************
    local.ProgramProcessingInfo.Name = "SWEIB274";
    local.Start.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;

    if (ReadProgramProcessingInfo())
    {
      local.ProgramProcessingInfo.Assign(entities.ProgramProcessingInfo);
      export.Process.Date = entities.ProgramProcessingInfo.ProcessDate;
      local.PpiFound.Flag = "Y";

      // CQ38148 - added second and third parameter
      do
      {
        local.Position.Text1 =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.CurrentPosition.Count, 1);

        if (AsChar(local.Position.Text1) == ',')
        {
          ++local.FieldNumber.Count;
          local.WorkArea.Text15 = "";

          switch(local.FieldNumber.Count)
          {
            case 1:
              if (local.Current.Count == 1)
              {
                export.AutomaticGenerateIwo.Flag = "Y";
              }
              else
              {
                local.WorkArea.Text15 =
                  Substring(local.ProgramProcessingInfo.ParameterList,
                  local.Start.Count, local.Current.Count - 1);
                export.AutomaticGenerateIwo.Flag = local.WorkArea.Text15;
              }

              local.Start.Count = local.CurrentPosition.Count + 1;
              local.Current.Count = 0;

              break;
            case 2:
              if (local.Current.Count == 1)
              {
                export.NumberOfDays.Count = 150;
              }
              else
              {
                local.WorkArea.Text15 =
                  Substring(local.ProgramProcessingInfo.ParameterList,
                  local.Start.Count, local.Current.Count - 1);
                export.NumberOfDays.Count =
                  (int)StringToNumber(local.WorkArea.Text15);
              }

              local.Start.Count = local.CurrentPosition.Count + 1;
              local.Current.Count = 0;

              break;
            case 3:
              if (local.Current.Count == 1)
              {
                export.MonthsDiff.Count = 12;
              }
              else
              {
                local.WorkArea.Text15 =
                  Substring(local.ProgramProcessingInfo.ParameterList,
                  local.Start.Count, local.Current.Count - 1);
                export.MonthsDiff.Count =
                  (int)StringToNumber(local.WorkArea.Text15);
              }

              local.Start.Count = local.CurrentPosition.Count + 1;
              local.Current.Count = 0;

              break;
            default:
              break;
          }
        }
        else if (IsEmpty(local.Position.Text1))
        {
          break;
        }

        ++local.CurrentPosition.Count;
        ++local.Current.Count;
      }
      while(!Equal(global.Command, "COMMAND"));

      // CQ38148 end
    }

    if (IsEmpty(export.AutomaticGenerateIwo.Flag))
    {
      export.AutomaticGenerateIwo.Flag = "Y";
    }

    if (export.NumberOfDays.Count <= 0)
    {
      export.NumberOfDays.Count = 150;
    }

    if (export.MonthsDiff.Count <= 0)
    {
      export.MonthsDiff.Count = 12;
    }

    if (AsChar(local.PpiFound.Flag) != 'Y')
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.Process.Date;
    local.EabReportSend.ProgramName = entities.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = export.Process.Date;
    local.NeededToOpen.ProgramName = entities.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT REPORT 01  -  NEW HIRE REPORT - SAR
    // **********************************************************
    local.NeededToOpen.RptHeading3 =
      "                     FEDERAL QUARTERLY WAGE REPORT";
    UseCabBusinessReport01();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT REPORT 02 - SSN MISMATCH
    // **********************************************************
    local.NeededToOpen.RptHeading3 =
      "                      FEDERAL QUARTERLY WAGE SSN MISMATCH";
    local.NeededToOpen.NumberOfColHeadings = 2;
    local.NeededToOpen.ColHeading1 =
      "Federal Case Registry Information FCR SSN     KAECSES SSN KAECSES Information";
      
    local.NeededToOpen.ColHeading2 =
      "--------------------------------- ----------- ----------- -------------------";
      
    UseCabBusinessReport02();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT REPORT 03 - EMPLOYEE NAME MISMATCH
    // **********************************************************
    local.NeededToOpen.RptHeading3 =
      "                  FEDERAL QUARTERLY WAGE NAME MISMATCH";
    local.NeededToOpen.NumberOfColHeadings = 2;
    local.NeededToOpen.ColHeading1 =
      "Person Num  Federal Case Registry                 KAECSES";
    local.NeededToOpen.ColHeading2 =
      "----------  ---------------------                 -------------------";
    UseCabBusinessReport03();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT REPORT 04 - EMPLOYER NAME OR ADDRESS MISMATCH
    // **********************************************************
    local.NeededToOpen.RptHeading3 =
      "FEDERAL QUARTERLY WAGE EMPLOYER NAME OR ADDRESS MISMATCH";
    local.NeededToOpen.NumberOfColHeadings = 2;
    local.NeededToOpen.ColHeading1 =
      " Field   Federal Case Registry Information    KAECSES Information";
    local.NeededToOpen.ColHeading2 =
      "-------- ---------------------------------    -------------------";
    UseCabBusinessReport04();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT REPORT 05 - REVISED EARNINGS
    // **********************************************************
    local.NeededToOpen.RptHeading3 =
      "        FEDERAL QUARTERLY WAGE - REVISED EARNINGS";
    local.NeededToOpen.NumberOfColHeadings = 3;
    local.NeededToOpen.ColHeading1 =
      "  PERSON   EMPLOYER                                     PREVIOUS   REVISED";
      
    local.NeededToOpen.ColHeading2 =
      "  NUMBER   NAME                              YEAR QTR   EARNINGS   EARNINGS";
      
    local.NeededToOpen.ColHeading3 =
      "---------- --------------------------------- ---- --- ---------- ----------";
      
    UseCabBusinessReport05();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN INPUT NEW HIRE FILE RECEIVED FROM DHR
    // **********************************************************
    UseEabReadFederalQuarterlyWage();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // GET THE COMMIT COUNT
    // **********************************************************
    export.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    // **********************************************************
    // REPORT THE SETTING OF AUTOMATIC GENERATE IWO SWITCH
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";

    if (AsChar(export.AutomaticGenerateIwo.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "The automatic generation of Income Withholding Orders has been requested.";
        
    }
    else
    {
      local.EabReportSend.RptDetail =
        "The automatic generation of Income Withholding Orders has been suppressed.";
        
    }

    UseCabControlReport1();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport02()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.NeededToOpen);

    Call(CabBusinessReport02.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport03()
  {
    var useImport = new CabBusinessReport03.Import();
    var useExport = new CabBusinessReport03.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.NeededToOpen);

    Call(CabBusinessReport03.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport04()
  {
    var useImport = new CabBusinessReport04.Import();
    var useExport = new CabBusinessReport04.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.NeededToOpen);

    Call(CabBusinessReport04.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport05()
  {
    var useImport = new CabBusinessReport05.Import();
    var useExport = new CabBusinessReport05.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.NeededToOpen);

    Call(CabBusinessReport05.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadFederalQuarterlyWage()
  {
    var useImport = new EabReadFederalQuarterlyWage.Import();
    var useExport = new EabReadFederalQuarterlyWage.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabReadFederalQuarterlyWage.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    export.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", local.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 3);
        entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of MonthsDiff.
    /// </summary>
    [JsonPropertyName("monthsDiff")]
    public Common MonthsDiff
    {
      get => monthsDiff ??= new();
      set => monthsDiff = value;
    }

    /// <summary>
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of AutomaticGenerateIwo.
    /// </summary>
    [JsonPropertyName("automaticGenerateIwo")]
    public Common AutomaticGenerateIwo
    {
      get => automaticGenerateIwo ??= new();
      set => automaticGenerateIwo = value;
    }

    private Common monthsDiff;
    private Common numberOfDays;
    private DateWorkArea process;
    private EabFileHandling eabFileHandling;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common automaticGenerateIwo;
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
    /// A value of PpiFound.
    /// </summary>
    [JsonPropertyName("ppiFound")]
    public Common PpiFound
    {
      get => ppiFound ??= new();
      set => ppiFound = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public TextWorkArea Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
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

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Common Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of MiniumPayment.
    /// </summary>
    [JsonPropertyName("miniumPayment")]
    public Common MiniumPayment
    {
      get => miniumPayment ??= new();
      set => miniumPayment = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Common ppiFound;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabReportSend neededToOpen;
    private TextWorkArea position;
    private Common currentPosition;
    private Common fieldNumber;
    private WorkArea workArea;
    private Common current;
    private Common start;
    private DateWorkArea startDate;
    private Common miniumPayment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
