// Program: SI_B270_EXTRACT_PERSON_INFO, ID: 371790652, model: 746.
// Short name: SWEI270B
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
/// A program: SI_B270_EXTRACT_PERSON_INFO.
/// </para>
/// <para>
/// This Procedure is for extracting Person numbers and SSN's of all active AR's
/// and AP's
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB270ExtractPersonInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B270_EXTRACT_PERSON_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB270ExtractPersonInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB270ExtractPersonInfo.
  /// </summary>
  public SiB270ExtractPersonInfo(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------
    //     Date          Developer     PR/WO#      Description
    //   6/10/96       Rao Mulpuri                 Initial Creation
    //   1/5/98        Siraj Konkader
    //   2/1/02        K Cole         PR#137114    Removed restart logic and
    // restructured/streamlined program
    // ------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiB270Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***************************************************
    // * Process all the selected records
    // ***************************************************
    foreach(var item in ReadCsePerson())
    {
      if (ReadCaseRole())
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        ExitState = "ACO_NN0000_ALL_OK";
        UseCabReadAdabasPersonBatch();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ok, continue processing
        }
        else
        {
          if (AsChar(local.AbendData.Type1) == 'A' && Equal
            (local.AbendData.AdabasResponseCd, "0113"))
          {
            // ***************************************************
            // *This is the ADABAS not found condition.
            // ***************************************************
            ExitState = "ACO_NN0000_ALL_OK";

            // ***************************************************
            // *Write a line to the ERROR RPT.
            // ***************************************************
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Person Not Found in ADABAS (type 113) for :  " + entities
              .CsePerson.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

              // ***************************************************************
              // 11/04/98  C. OTT  -   All 'Hard' errors will cause an Abort 
              // exit state to be set.  'Hard' errors are database errors and
              // file handling errors.
              // ****************************************************************
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
            else
            {
              continue;
            }
          }
          else if (AsChar(local.AbendData.Type1) == 'A' && Equal
            (local.AbendData.AdabasResponseCd, "0000") && Equal
            (local.AbendData.AdabasFileAction, " NF"))
          {
            // ***************************************************
            // *This is the ADABAS not found condition.
            // ***************************************************
            ExitState = "ACO_NN0000_ALL_OK";

            // ***************************************************
            // *Write a line to the ERROR RPT.
            // ***************************************************
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Person Not Found in ADABAS database for :  " + entities
              .CsePerson.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

              // ***************************************************************
              // 11/04/98  C. OTT  -   All 'Hard' errors will cause an Abort 
              // exit state to be set.  'Hard' errors are database errors and
              // file handling errors.
              // ****************************************************************
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
            else
            {
              continue;
            }
          }
          else
          {
            // **************************************************************
            // An unknown error response has been returned from adabas.
            // **************************************************************
            // ***************************************************
            // *Write a line to the ERROR RPT.
            // ***************************************************
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Fatal error in ADABAS for person number :  " + entities
              .CsePerson.Number;
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + ". Abend Type code: " + local
              .AbendData.Type1 + "; Response code: " + local
              .AbendData.AdabasResponseCd + "; File number: " + local
              .AbendData.AdabasFileNumber + "; File action: " + local
              .AbendData.AdabasFileAction;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

              // ***************************************************************
              // 11/04/98  C. OTT  -   All 'Hard' errors will cause an Abort 
              // exit state to be set.  'Hard' errors are database errors and
              // file handling errors.
              // ****************************************************************
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
            else
            {
              return;
            }
          }
        }

        if (IsEmpty(local.CsePersonsWorkSet.Ssn) || Equal
          (local.CsePersonsWorkSet.Ssn, "000000000"))
        {
          continue;
        }

        local.SiWageIncomeSourceRec.CseIndicator = "C";
        local.SiWageIncomeSourceRec.PersonNumber = entities.CsePerson.Number;
        local.SiWageIncomeSourceRec.PersonSsn = local.CsePersonsWorkSet.Ssn;
        local.ExtractPersonInfo.FileInstruction = "WRITE";
        UseSiEabWritePersonInfo();

        if (!IsEmpty(local.ExtractPersonInfo.TextReturnCode))
        {
          // ***************************************************
          // *Write a line to the ERROR RPT.
          // ***************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing the Person Information extract file.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++local.NoOfExtFileRecsWrittn.Count;
      }
    }

    UseSiB270Close();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSiB270Close()
  {
    var useImport = new SiB270Close.Import();
    var useExport = new SiB270Close.Export();

    useImport.NumOfRecsWritten.Count = local.NoOfExtFileRecsWrittn.Count;

    Call(SiB270Close.Execute, useImport, useExport);
  }

  private void UseSiB270Housekeeping()
  {
    var useImport = new SiB270Housekeeping.Import();
    var useExport = new SiB270Housekeeping.Export();

    Call(SiB270Housekeeping.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseSiEabWritePersonInfo()
  {
    var useImport = new SiEabWritePersonInfo.Import();
    var useExport = new SiEabWritePersonInfo.Export();

    useImport.SiWageIncomeSourceRec.Assign(local.SiWageIncomeSourceRec);
    useImport.External.Assign(local.ExtractPersonInfo);
    useExport.External.Assign(local.ExtractPersonInfo);

    Call(SiEabWritePersonInfo.Execute, useImport, useExport);

    local.ExtractPersonInfo.Assign(useExport.External);
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      null,
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CaseRole Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of RestartCommon.
    /// </summary>
    [JsonPropertyName("restartCommon")]
    public Common RestartCommon
    {
      get => restartCommon ??= new();
      set => restartCommon = value;
    }

    /// <summary>
    /// A value of RestartCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("restartCsePersonsWorkSet")]
    public CsePersonsWorkSet RestartCsePersonsWorkSet
    {
      get => restartCsePersonsWorkSet ??= new();
      set => restartCsePersonsWorkSet = value;
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
    /// A value of SiWageIncomeSourceRec.
    /// </summary>
    [JsonPropertyName("siWageIncomeSourceRec")]
    public SiWageIncomeSourceRec SiWageIncomeSourceRec
    {
      get => siWageIncomeSourceRec ??= new();
      set => siWageIncomeSourceRec = value;
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
    /// A value of ExtractPersonInfo.
    /// </summary>
    [JsonPropertyName("extractPersonInfo")]
    public External ExtractPersonInfo
    {
      get => extractPersonInfo ??= new();
      set => extractPersonInfo = value;
    }

    /// <summary>
    /// A value of NoOfExtFileRecsWrittn.
    /// </summary>
    [JsonPropertyName("noOfExtFileRecsWrittn")]
    public Common NoOfExtFileRecsWrittn
    {
      get => noOfExtFileRecsWrittn ??= new();
      set => noOfExtFileRecsWrittn = value;
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

    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private CaseRole blank;
    private AbendData abendData;
    private Common restartCommon;
    private CsePersonsWorkSet restartCsePersonsWorkSet;
    private ProgramCheckpointRestart programCheckpointRestart;
    private SiWageIncomeSourceRec siWageIncomeSourceRec;
    private CsePersonsWorkSet csePersonsWorkSet;
    private External extractPersonInfo;
    private Common noOfExtFileRecsWrittn;
    private ProgramProcessingInfo programProcessingInfo;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
