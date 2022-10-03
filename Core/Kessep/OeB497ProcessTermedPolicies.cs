// Program: OE_B497_PROCESS_TERMED_POLICIES, ID: 371182456, model: 746.
// Short name: SWEE497B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B497_PROCESS_TERMED_POLICIES.
/// </para>
/// <para>
/// This program writes the CSE Person numbers of those who are APs or ARs, to a
/// sequential file, using external SWEXEE27.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB497ProcessTermedPolicies: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B497_PROCESS_TERMED_POLICIES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB497ProcessTermedPolicies(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB497ProcessTermedPolicies.
  /// </summary>
  public OeB497ProcessTermedPolicies(IContext context, Import import,
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
    // ***********************************************************************
    // *   Date      Name      Work Req  Description                         *
    // * ----------  --------  --------  
    // -----------------------------------
    // *
    // * 12/01/2002  Ed Lyman  WR020311  Initial Coding                      *
    // ***********************************************************************
    // ***********************************************************************
    // *   Process Health Insurance Policy Terminated from EDS               *
    // *       Validate File and Report Errors (Rpt01)                       *
    // *       Determine if there are differences                            *
    // *       Update Data Base if differences are found                     *
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseOeB497Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Status = "EOF";
    }

    while(!Equal(local.EabFileHandling.Status, "EOF"))
    {
      local.EabFileHandling.Action = "READ";
      UseEabReadTerminatedPolicyFile();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          ++local.RecordsRead.Count;
          local.PolicyTerminated.Date = local.FileHeaderRecord.Date;

          break;
        case "HEADER":
          local.CurrentFileHeader.Date = local.FileHeaderRecord.Date;

          if (ReadControlTable2())
          {
            local.CurrentFileHeader.TextDate =
              NumberToString(DateToInt(local.CurrentFileHeader.Date), 8, 8);
            local.LastFileDate.Date =
              IntToDate(entities.ControlTable.LastUsedNumber);
            local.EabFileHandling.Action = "WRITE";

            if (!Lt(local.LastFileDate.Date, local.CurrentFileHeader.Date))
            {
              local.NeededToWrite.RptDetail =
                "File header indicates file may have already been processed.  File date is: " +
                local.CurrentFileHeader.TextDate;
              UseCabErrorReport();
              local.NeededToWrite.RptDetail =
                "Date in the control table (showing header date of previous file) is: " +
                NumberToString(entities.ControlTable.LastUsedNumber, 8, 8);
              UseCabErrorReport();
              ExitState = "OE0000_ERROR_READING_EXT_FILE";

              goto AfterCycle;
            }
            else
            {
              local.NeededToWrite.RptDetail = "File Header date is:   " + local
                .CurrentFileHeader.TextDate;
              UseCabControlReport();
            }
          }
          else
          {
            try
            {
              CreateControlTable();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CONTROL_TABLE_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CONTROL_TABLE_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto AfterCycle;
          }

          continue;
        case "TRAILER":
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "File Trailer count is: " + NumberToString
            (local.TrailerCount.Count, 8, 8);
          UseCabControlReport();

          if (local.TrailerCount.Count != local.RecordsRead.Count + 2)
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Trailer Count does not equal records read.  " + "See Control Report.";
              
            UseCabErrorReport();
            ExitState = "OE0000_ERROR_READING_EXT_FILE";

            goto AfterCycle;
          }

          continue;
        case "EOF":
          if (local.RecordsRead.Count == 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail = "The input file is empty." + "";
            UseCabErrorReport();

            goto AfterCycle;
          }

          if (ReadControlTable1())
          {
            try
            {
              UpdateControlTable();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CONTROL_TABLE_VALUE_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CONTROL_TABLE_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            ExitState = "CONTROL_TABLE_ID_NF";
          }

          goto AfterCycle;
        default:
          local.NeededToWrite.RptDetail = "Error reading EDS file: " + local
            .EabFileHandling.Status;
          UseCabErrorReport();
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          goto AfterCycle;
      }

      UseOeB497PolicyTerminated();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      if (AsChar(local.FlagInfoValid.Flag) != 'Y')
      {
        ++local.RecordsInvalidInfo.Count;

        continue;
      }

      ++local.CommitCount.Count;

      if (local.CommitCount.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // *****************************************************************
        // This program will not do a checkpoint restart.  Simply
        // reprocess all the records.  Any previously processed
        // records will result in no change.
        // *****************************************************************
        UseExtToDoACommit();
        local.CommitCount.Count = 0;
      }
    }

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB497Close();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Abend: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB497Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

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

  private void UseEabReadTerminatedPolicyFile()
  {
    var useImport = new EabReadTerminatedPolicyFile.Import();
    var useExport = new EabReadTerminatedPolicyFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.HealthInsuranceCompany.CarrierCode =
      local.HealthInsuranceCompany.CarrierCode;
    useExport.HealthInsuranceCoverage.InsurancePolicyNumber =
      local.HealthInsuranceCoverage.InsurancePolicyNumber;
    useExport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useExport.FileHeader.Date = local.FileHeaderRecord.Date;
    useExport.FileTrailer.Count = local.TrailerCount.Count;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadTerminatedPolicyFile.Execute, useImport, useExport);

    local.HealthInsuranceCompany.CarrierCode =
      useExport.HealthInsuranceCompany.CarrierCode;
    local.HealthInsuranceCoverage.InsurancePolicyNumber =
      useExport.HealthInsuranceCoverage.InsurancePolicyNumber;
    local.CsePersonsWorkSet.Number = useExport.CsePersonsWorkSet.Number;
    local.FileHeaderRecord.Date = useExport.FileHeader.Date;
    local.TrailerCount.Count = useExport.FileTrailer.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    Call(ExtToDoACommit.Execute, useImport, useExport);
  }

  private void UseOeB497Close()
  {
    var useImport = new OeB497Close.Import();
    var useExport = new OeB497Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.PoliciesTerminated.Count = local.PoliciesTerminated.Count;
    useImport.PoliciesNotFound.Count = local.PoliciesNotFound.Count;
    useImport.AlreadyProcessed.Count = local.AlreadyProcessed.Count;
    useImport.RecordsInvalidInfo.Count = local.RecordsInvalidInfo.Count;

    Call(OeB497Close.Execute, useImport, useExport);
  }

  private void UseOeB497Housekeeping()
  {
    var useImport = new OeB497Housekeeping.Import();
    var useExport = new OeB497Housekeeping.Export();

    Call(OeB497Housekeeping.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseOeB497PolicyTerminated()
  {
    var useImport = new OeB497PolicyTerminated.Import();
    var useExport = new OeB497PolicyTerminated.Export();

    useImport.HealthInsuranceCompany.CarrierCode =
      local.HealthInsuranceCompany.CarrierCode;
    useImport.HealthInsuranceCoverage.InsurancePolicyNumber =
      local.HealthInsuranceCoverage.InsurancePolicyNumber;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.PolicyTerminated.Date = local.PolicyTerminated.Date;
    useImport.Max.Date = local.Max.Date;
    useImport.PoliciesTerminated.Count = local.PoliciesTerminated.Count;
    useImport.PoliciesNotFound.Count = local.PoliciesNotFound.Count;
    useImport.AlreadyProcessed.Count = local.AlreadyProcessed.Count;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(OeB497PolicyTerminated.Execute, useImport, useExport);

    local.FlagInfoValid.Flag = useExport.CompanyInfoValid.Flag;
    local.PoliciesTerminated.Count = useExport.PoliciesTerminated.Count;
    local.PoliciesNotFound.Count = useExport.PoliciesNotFound.Count;
    local.AlreadyProcessed.Count = useExport.AlreadyProcessed.Count;
  }

  private void CreateControlTable()
  {
    var identifier = "HINS POLICY TERMED FILE DATE";
    var lastUsedNumber = DateToInt(local.CurrentFileHeader.Date);

    entities.ControlTable.Populated = false;
    Update("CreateControlTable",
      (db, command) =>
      {
        db.SetString(command, "cntlTblId", identifier);
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetNullableString(command, "dummy1", "");
      });

    entities.ControlTable.Identifier = identifier;
    entities.ControlTable.LastUsedNumber = lastUsedNumber;
    entities.ControlTable.Populated = true;
  }

  private bool ReadControlTable1()
  {
    entities.ControlTable.Populated = false;

    return Read("ReadControlTable1",
      null,
      (db, reader) =>
      {
        entities.ControlTable.Identifier = db.GetString(reader, 0);
        entities.ControlTable.LastUsedNumber = db.GetInt32(reader, 1);
        entities.ControlTable.Populated = true;
      });
  }

  private bool ReadControlTable2()
  {
    entities.ControlTable.Populated = false;

    return Read("ReadControlTable2",
      null,
      (db, reader) =>
      {
        entities.ControlTable.Identifier = db.GetString(reader, 0);
        entities.ControlTable.LastUsedNumber = db.GetInt32(reader, 1);
        entities.ControlTable.Populated = true;
      });
  }

  private void UpdateControlTable()
  {
    var lastUsedNumber = DateToInt(local.CurrentFileHeader.Date);

    entities.ControlTable.Populated = false;
    Update("UpdateControlTable",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetString(command, "cntlTblId", entities.ControlTable.Identifier);
      });

    entities.ControlTable.LastUsedNumber = lastUsedNumber;
    entities.ControlTable.Populated = true;
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
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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
    /// A value of FileHeaderRecord.
    /// </summary>
    [JsonPropertyName("fileHeaderRecord")]
    public DateWorkArea FileHeaderRecord
    {
      get => fileHeaderRecord ??= new();
      set => fileHeaderRecord = value;
    }

    /// <summary>
    /// A value of PolicyTerminated.
    /// </summary>
    [JsonPropertyName("policyTerminated")]
    public DateWorkArea PolicyTerminated
    {
      get => policyTerminated ??= new();
      set => policyTerminated = value;
    }

    /// <summary>
    /// A value of TrailerCount.
    /// </summary>
    [JsonPropertyName("trailerCount")]
    public Common TrailerCount
    {
      get => trailerCount ??= new();
      set => trailerCount = value;
    }

    /// <summary>
    /// A value of CurrentFileHeader.
    /// </summary>
    [JsonPropertyName("currentFileHeader")]
    public DateWorkArea CurrentFileHeader
    {
      get => currentFileHeader ??= new();
      set => currentFileHeader = value;
    }

    /// <summary>
    /// A value of LastFileDate.
    /// </summary>
    [JsonPropertyName("lastFileDate")]
    public DateWorkArea LastFileDate
    {
      get => lastFileDate ??= new();
      set => lastFileDate = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of CommitCount.
    /// </summary>
    [JsonPropertyName("commitCount")]
    public Common CommitCount
    {
      get => commitCount ??= new();
      set => commitCount = value;
    }

    /// <summary>
    /// A value of FlagNoDifferences.
    /// </summary>
    [JsonPropertyName("flagNoDifferences")]
    public Common FlagNoDifferences
    {
      get => flagNoDifferences ??= new();
      set => flagNoDifferences = value;
    }

    /// <summary>
    /// A value of FlagInfoValid.
    /// </summary>
    [JsonPropertyName("flagInfoValid")]
    public Common FlagInfoValid
    {
      get => flagInfoValid ??= new();
      set => flagInfoValid = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of PoliciesTerminated.
    /// </summary>
    [JsonPropertyName("policiesTerminated")]
    public Common PoliciesTerminated
    {
      get => policiesTerminated ??= new();
      set => policiesTerminated = value;
    }

    /// <summary>
    /// A value of PoliciesNotFound.
    /// </summary>
    [JsonPropertyName("policiesNotFound")]
    public Common PoliciesNotFound
    {
      get => policiesNotFound ??= new();
      set => policiesNotFound = value;
    }

    /// <summary>
    /// A value of AlreadyProcessed.
    /// </summary>
    [JsonPropertyName("alreadyProcessed")]
    public Common AlreadyProcessed
    {
      get => alreadyProcessed ??= new();
      set => alreadyProcessed = value;
    }

    /// <summary>
    /// A value of RecordsInvalidInfo.
    /// </summary>
    [JsonPropertyName("recordsInvalidInfo")]
    public Common RecordsInvalidInfo
    {
      get => recordsInvalidInfo ??= new();
      set => recordsInvalidInfo = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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

    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea fileHeaderRecord;
    private DateWorkArea policyTerminated;
    private Common trailerCount;
    private DateWorkArea currentFileHeader;
    private DateWorkArea lastFileDate;
    private DateWorkArea max;
    private Common commitCount;
    private Common flagNoDifferences;
    private Common flagInfoValid;
    private Common recordsRead;
    private Common policiesTerminated;
    private Common policiesNotFound;
    private Common alreadyProcessed;
    private Common recordsInvalidInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend neededToWrite;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private ControlTable controlTable;
  }
#endregion
}
