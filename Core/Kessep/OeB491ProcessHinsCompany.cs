// Program: OE_B491_PROCESS_HINS_COMPANY, ID: 372868970, model: 746.
// Short name: SWEE491B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B491_PROCESS_HINS_COMPANY.
/// </para>
/// <para>
/// This program writes the CSE Person numbers of those who are APs or ARs, to a
/// sequential file, using external SWEXEE27.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB491ProcessHinsCompany: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B491_PROCESS_HINS_COMPANY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB491ProcessHinsCompany(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB491ProcessHinsCompany.
  /// </summary>
  public OeB491ProcessHinsCompany(IContext context, Import import, Export export)
    :
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
    // *   Process Health Insurance Company file from EDS                    *
    // *       Validate File and Report Errors (Rpt01)                       *
    // *       Determine if there are differences                            *
    // *       Update Data Base if differences are found                     *
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseOeB491Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Status = "EOF";
    }

    while(!Equal(local.EabFileHandling.Status, "EOF"))
    {
      local.EabFileHandling.Action = "READ";
      UseEabReadHinsCoFile();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          ++local.RecordsRead.Count;

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

      UseOeB491ValidateHinsCompany();

      if (AsChar(local.FlagInfoValid.Flag) != 'Y')
      {
        ++local.CompaniesInvalidInfo.Count;

        continue;
      }

      UseOeB491FindHinsCoDifferences();

      if (AsChar(local.FlagNoDifferences.Flag) == 'Y')
      {
        ++local.CompaniesNoChange.Count;

        continue;
      }

      UseOeB491MaintainHealthInsCo();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
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
      UseOeB491Close();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Abend: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB491Close();
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

  private void UseEabReadHinsCoFile()
  {
    var useImport = new EabReadHinsCoFile.Import();
    var useExport = new EabReadHinsCoFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.FileHeader.Date = local.FileHeaderRecord.Date;
    useExport.TotalRecords.Count = local.TrailerCount.Count;
    useExport.HealthInsuranceCompanyAddress.Assign(
      local.HealthInsuranceCompanyAddress);
    useExport.HealthInsuranceCompany.Assign(local.HealthInsuranceCompany);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadHinsCoFile.Execute, useImport, useExport);

    local.FileHeaderRecord.Date = useExport.FileHeader.Date;
    local.TrailerCount.Count = useExport.TotalRecords.Count;
    local.HealthInsuranceCompanyAddress.Assign(
      useExport.HealthInsuranceCompanyAddress);
    local.HealthInsuranceCompany.Assign(useExport.HealthInsuranceCompany);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    Call(ExtToDoACommit.Execute, useImport, useExport);
  }

  private void UseOeB491Close()
  {
    var useImport = new OeB491Close.Import();
    var useExport = new OeB491Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.CompaniesAdded.Count = local.CompaniesAdded.Count;
    useImport.CompaniesUpdated.Count = local.CompaniesUpdated.Count;
    useImport.CompaniesNoChange.Count = local.CompaniesNoChange.Count;
    useImport.CompaniesInvalidInfo.Count = local.CompaniesInvalidInfo.Count;

    Call(OeB491Close.Execute, useImport, useExport);
  }

  private void UseOeB491FindHinsCoDifferences()
  {
    var useImport = new OeB491FindHinsCoDifferences.Import();
    var useExport = new OeB491FindHinsCoDifferences.Export();

    useImport.HealthInsuranceCompanyAddress.Assign(
      local.HealthInsuranceCompanyAddress);
    useImport.HealthInsuranceCompany.Assign(local.HealthInsuranceCompany);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(OeB491FindHinsCoDifferences.Execute, useImport, useExport);

    local.FlagNoDifferences.Flag = useExport.FlagNoDifferences.Flag;
  }

  private void UseOeB491Housekeeping()
  {
    var useImport = new OeB491Housekeeping.Import();
    var useExport = new OeB491Housekeeping.Export();

    Call(OeB491Housekeeping.Execute, useImport, useExport);

    local.State.Id = useExport.State.Id;
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseOeB491MaintainHealthInsCo()
  {
    var useImport = new OeB491MaintainHealthInsCo.Import();
    var useExport = new OeB491MaintainHealthInsCo.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.CompaniesAdded.Count = local.CompaniesAdded.Count;
    useImport.CompaniesUpdated.Count = local.CompaniesUpdated.Count;
    useImport.HealthInsuranceCompanyAddress.Assign(
      local.HealthInsuranceCompanyAddress);
    useImport.HealthInsuranceCompany.Assign(local.HealthInsuranceCompany);
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(OeB491MaintainHealthInsCo.Execute, useImport, useExport);

    local.CompaniesAdded.Count = useExport.CompaniesAdded.Count;
    local.CompaniesUpdated.Count = useExport.CompaniesUpdated.Count;
  }

  private void UseOeB491ValidateHinsCompany()
  {
    var useImport = new OeB491ValidateHinsCompany.Import();
    var useExport = new OeB491ValidateHinsCompany.Export();

    useImport.State.Id = local.State.Id;
    useImport.HealthInsuranceCompanyAddress.Assign(
      local.HealthInsuranceCompanyAddress);
    useImport.HealthInsuranceCompany.Assign(local.HealthInsuranceCompany);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(OeB491ValidateHinsCompany.Execute, useImport, useExport);

    local.FlagInfoValid.Flag = useExport.CompanyInfoValid.Flag;
  }

  private void CreateControlTable()
  {
    var identifier = "HINS POLICY FILE DATE";
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
    /// A value of FileHeaderRecord.
    /// </summary>
    [JsonPropertyName("fileHeaderRecord")]
    public DateWorkArea FileHeaderRecord
    {
      get => fileHeaderRecord ??= new();
      set => fileHeaderRecord = value;
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
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
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
    /// A value of CompaniesAdded.
    /// </summary>
    [JsonPropertyName("companiesAdded")]
    public Common CompaniesAdded
    {
      get => companiesAdded ??= new();
      set => companiesAdded = value;
    }

    /// <summary>
    /// A value of CompaniesUpdated.
    /// </summary>
    [JsonPropertyName("companiesUpdated")]
    public Common CompaniesUpdated
    {
      get => companiesUpdated ??= new();
      set => companiesUpdated = value;
    }

    /// <summary>
    /// A value of CompaniesNoChange.
    /// </summary>
    [JsonPropertyName("companiesNoChange")]
    public Common CompaniesNoChange
    {
      get => companiesNoChange ??= new();
      set => companiesNoChange = value;
    }

    /// <summary>
    /// A value of CompaniesInvalidInfo.
    /// </summary>
    [JsonPropertyName("companiesInvalidInfo")]
    public Common CompaniesInvalidInfo
    {
      get => companiesInvalidInfo ??= new();
      set => companiesInvalidInfo = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

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

    private DateWorkArea fileHeaderRecord;
    private Common trailerCount;
    private DateWorkArea currentFileHeader;
    private DateWorkArea lastFileDate;
    private DateWorkArea max;
    private Code state;
    private Common commitCount;
    private Common flagNoDifferences;
    private Common flagInfoValid;
    private Common recordsRead;
    private Common companiesAdded;
    private Common companiesUpdated;
    private Common companiesNoChange;
    private Common companiesInvalidInfo;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompany healthInsuranceCompany;
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
