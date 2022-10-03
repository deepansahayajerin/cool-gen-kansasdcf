// Program: FN_B586_EIWO_EMPLOYER_UPDATES, ID: 1902467122, model: 746.
// Short name: SWEB586P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B586_EIWO_EMPLOYER_UPDATES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB586EiwoEmployerUpdates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B586_EIWO_EMPLOYER_UPDATES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB586EiwoEmployerUpdates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB586EiwoEmployerUpdates.
  /// </summary>
  public FnB586EiwoEmployerUpdates(IContext context, Import import,
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
    // *                   Maintenance Log
    // 
    // *
    // ***********************************************************************
    // *    DATE       NAME      REQUEST      DESCRIPTION                    *
    // * ----------  ---------  
    // ---------
    // --------------------------------
    // *
    // * 06/05/2015  DDupree   CQ22212      Initial Coding.                  *
    // *
    // 
    // *
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseFnB586Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.RecordsRead.Count = 0;
    local.RecordsAdded.Count = 0;
    local.RecordsUpdated.Count = 0;
    local.RecordsDeleted.Count = 0;

    // **********************************************************
    // Read each input record from the file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      local.Employer.Assign(local.ClearEmployer);
      local.Change.Assign(local.ClearEmployer);
      local.EmployerAddress.Assign(local.ClearEmployerAddress);
      UseFnB586ReadFile();

      switch(TrimEnd(local.ReturnCode.TextReturnCode))
      {
        case "00":
          break;
        case "EF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING EIWO EMPLOYER INPUT FILE";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      ++local.RecordsRead.Count;

      if (AsChar(local.ActiveInd.Text1) == 'A')
      {
        if (ReadEmployer1())
        {
          foreach(var item in ReadEmployer2())
          {
            if (Lt(local.Process.Date, entities.Main.EiwoEndDate) && !
              Lt(local.Process.Date, entities.Main.EiwoStartDate))
            {
              continue;

              // already marked as eiwo, no need to update
            }
            else
            {
              local.Change.Assign(entities.Main);
              local.Change.EiwoStartDate = local.Process.Date;
              local.Change.EiwoEndDate = local.Max.Date;
              UseFnUpdateEmployerOnly();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "Error for EIN # " + (
                  local.Employer.Ein ?? "") + "  " + local
                  .ExitStateWorkArea.Message;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                continue;
              }

              ++local.RecordsUpdated.Count;
            }
          }
        }
        else
        {
          // now we create one
          local.ControlTable.Identifier = "EMPLOYER";
          UseAccessControlTable();
          local.Current.Timestamp = Now();
          local.Employer.Identifier = local.ControlTable.LastUsedNumber;
          local.Employer.EiwoStartDate = local.Process.Date;
          local.Employer.EiwoEndDate = local.Max.Date;
          UseSiCreateEmployer();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Error for EIN # " + (
              local.Employer.Ein ?? "") + "  " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            continue;
          }

          local.EmployerAddress.LocationType = "D";
          UseSiAddIncomeSourceAddress();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Error for EIN # " + (
              local.Employer.Ein ?? "") + "  " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            continue;
          }

          ++local.RecordsAdded.Count;
        }
      }
      else if (ReadEmployer1())
      {
        foreach(var item in ReadEmployer2())
        {
          if (ReadIncomeSource())
          {
            if (Lt(local.Process.Date, entities.Main.EiwoEndDate))
            {
              local.Change.Assign(entities.Main);
              local.Change.EiwoEndDate = local.Process.Date;
              UseFnUpdateEmployerOnly();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "Error for EIN # " + (
                  local.Employer.Ein ?? "") + "  " + local
                  .ExitStateWorkArea.Message;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                goto Next;
              }

              ++local.RecordsUpdated.Count;
            }
          }
          else
          {
            // not assoicated with any employment records so delete the record
            DeleteEmployer();
            ++local.RecordsDeleted.Count;
          }
        }
      }
      else
      {
        // do nighing, since we did not have the employer in the system
      }

      // *************** Check to see if commit is needed ********************
      ++local.Commit.Count;

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.Commit.Count = 0;
        local.EabFileHandling.Action = "WRITE";
        local.DateDel.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-" + NumberToString
          (Now().Date.Month, 14, 2) + "-" + NumberToString
          (Now().Date.Day, 14, 2);
        local.TimeDel.Text8 =
          NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
        local.NeededToWrite.RptDetail = "Commit taken after record number: " + NumberToString
          (local.RecordsRead.Count, 15) + "  Date: " + local.DateDel.Text10 + "  Time: " +
          local.TimeDel.Text8;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          break;
        }
      }

Next:
      ;
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseFnB586Close();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ALL_OK";
      UseFnB586Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEmployer1(Employer source, Employer target)
  {
    target.Identifier = source.Identifier;
    target.Ein = source.Ein;
    target.KansasId = source.KansasId;
    target.Name = source.Name;
    target.PhoneNo = source.PhoneNo;
    target.AreaCode = source.AreaCode;
    target.EiwoEndDate = source.EiwoEndDate;
    target.EiwoStartDate = source.EiwoStartDate;
  }

  private static void MoveEmployer2(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
    target.PhoneNo = source.PhoneNo;
    target.AreaCode = source.AreaCode;
    target.EiwoEndDate = source.EiwoEndDate;
  }

  private static void MoveEmployerAddress1(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Identifier = source.Identifier;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveEmployerAddress2(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

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

    useExport.External.NumericReturnCode = local.ReturnCode.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ReturnCode.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB586Close()
  {
    var useImport = new FnB586Close.Import();
    var useExport = new FnB586Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.EmployersDeleted.Count = local.RecordsDeleted.Count;
    useImport.EmployersUpdated.Count = local.RecordsUpdated.Count;
    useImport.EmployersCreated.Count = local.RecordsAdded.Count;

    Call(FnB586Close.Execute, useImport, useExport);
  }

  private void UseFnB586Housekeeping()
  {
    var useImport = new FnB586Housekeeping.Import();
    var useExport = new FnB586Housekeeping.Export();

    Call(FnB586Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseFnB586ReadFile()
  {
    var useImport = new FnB586ReadFile.Import();
    var useExport = new FnB586ReadFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.Employer.Assign(local.Employer);
    useExport.EmployerAddress.Assign(local.EmployerAddress);
    useExport.ActiveInd.Text1 = local.ActiveInd.Text1;
    useExport.External.Assign(local.ReturnCode);

    Call(FnB586ReadFile.Execute, useImport, useExport);

    MoveEmployer2(useExport.Employer, local.Employer);
    MoveEmployerAddress2(useExport.EmployerAddress, local.EmployerAddress);
    local.ActiveInd.Text1 = useExport.ActiveInd.Text1;
    local.ReturnCode.Assign(useExport.External);
  }

  private void UseFnUpdateEmployerOnly()
  {
    var useImport = new FnUpdateEmployerOnly.Import();
    var useExport = new FnUpdateEmployerOnly.Export();

    useImport.Employer.Assign(local.Change);

    Call(FnUpdateEmployerOnly.Execute, useImport, useExport);
  }

  private void UseSiAddIncomeSourceAddress()
  {
    var useImport = new SiAddIncomeSourceAddress.Import();
    var useExport = new SiAddIncomeSourceAddress.Export();

    useImport.Employer.Identifier = local.Employer.Identifier;
    MoveEmployerAddress1(local.EmployerAddress, useImport.EmployerAddress);

    Call(SiAddIncomeSourceAddress.Execute, useImport, useExport);

    local.EmployerAddress.Identifier = useExport.EmployerAddress.Identifier;
  }

  private void UseSiCreateEmployer()
  {
    var useImport = new SiCreateEmployer.Import();
    var useExport = new SiCreateEmployer.Export();

    MoveEmployer1(local.Employer, useImport.Employer);

    Call(SiCreateEmployer.Execute, useImport, useExport);

    local.Employer.Assign(useExport.Employer);
  }

  private void DeleteEmployer()
  {
    Update("DeleteEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.Main.Identifier);
      });
  }

  private bool ReadEmployer1()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", local.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Populated = true;
      });
  }

  private IEnumerable<bool> ReadEmployer2()
  {
    entities.Main.Populated = false;

    return ReadEach("ReadEmployer2",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", local.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.Main.Identifier = db.GetInt32(reader, 0);
        entities.Main.Ein = db.GetNullableString(reader, 1);
        entities.Main.KansasId = db.GetNullableString(reader, 2);
        entities.Main.Name = db.GetNullableString(reader, 3);
        entities.Main.CreatedBy = db.GetString(reader, 4);
        entities.Main.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Main.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Main.LastUpdatedTstamp = db.GetNullableDateTime(reader, 7);
        entities.Main.PhoneNo = db.GetNullableString(reader, 8);
        entities.Main.AreaCode = db.GetNullableInt32(reader, 9);
        entities.Main.EiwoEndDate = db.GetNullableDate(reader, 10);
        entities.Main.EiwoStartDate = db.GetNullableDate(reader, 11);
        entities.Main.Populated = true;

        return true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetNullableInt32(command, "empId", entities.Main.Identifier);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.CspINumber = db.GetString(reader, 2);
        entities.IncomeSource.SelfEmployedInd = db.GetNullableString(reader, 3);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 5);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 6);
        entities.IncomeSource.Note2 = db.GetNullableString(reader, 7);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
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
    /// A value of Change.
    /// </summary>
    [JsonPropertyName("change")]
    public Employer Change
    {
      get => change ??= new();
      set => change = value;
    }

    /// <summary>
    /// A value of RecordsAdded.
    /// </summary>
    [JsonPropertyName("recordsAdded")]
    public Common RecordsAdded
    {
      get => recordsAdded ??= new();
      set => recordsAdded = value;
    }

    /// <summary>
    /// A value of ClearEmployer.
    /// </summary>
    [JsonPropertyName("clearEmployer")]
    public Employer ClearEmployer
    {
      get => clearEmployer ??= new();
      set => clearEmployer = value;
    }

    /// <summary>
    /// A value of ClearEmployerAddress.
    /// </summary>
    [JsonPropertyName("clearEmployerAddress")]
    public EmployerAddress ClearEmployerAddress
    {
      get => clearEmployerAddress ??= new();
      set => clearEmployerAddress = value;
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
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// A value of ActiveInd.
    /// </summary>
    [JsonPropertyName("activeInd")]
    public WorkArea ActiveInd
    {
      get => activeInd ??= new();
      set => activeInd = value;
    }

    /// <summary>
    /// A value of AutomaticGenerateIwo1De.
    /// </summary>
    [JsonPropertyName("automaticGenerateIwo1De")]
    public Common AutomaticGenerateIwo1De
    {
      get => automaticGenerateIwo1De ??= new();
      set => automaticGenerateIwo1De = value;
    }

    /// <summary>
    /// A value of PreviouslyCompared.
    /// </summary>
    [JsonPropertyName("previouslyCompared")]
    public Employer PreviouslyCompared
    {
      get => previouslyCompared ??= new();
      set => previouslyCompared = value;
    }

    /// <summary>
    /// A value of TimeDel.
    /// </summary>
    [JsonPropertyName("timeDel")]
    public TextWorkArea TimeDel
    {
      get => timeDel ??= new();
      set => timeDel = value;
    }

    /// <summary>
    /// A value of DateDel.
    /// </summary>
    [JsonPropertyName("dateDel")]
    public TextWorkArea DateDel
    {
      get => dateDel ??= new();
      set => dateDel = value;
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
    /// A value of RecordsUpdated.
    /// </summary>
    [JsonPropertyName("recordsUpdated")]
    public Common RecordsUpdated
    {
      get => recordsUpdated ??= new();
      set => recordsUpdated = value;
    }

    /// <summary>
    /// A value of RecordsDeleted.
    /// </summary>
    [JsonPropertyName("recordsDeleted")]
    public Common RecordsDeleted
    {
      get => recordsDeleted ??= new();
      set => recordsDeleted = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public External ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of BeginQuarter.
    /// </summary>
    [JsonPropertyName("beginQuarter")]
    public DateWorkArea BeginQuarter
    {
      get => beginQuarter ??= new();
      set => beginQuarter = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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

    private Employer change;
    private Common recordsAdded;
    private Employer clearEmployer;
    private EmployerAddress clearEmployerAddress;
    private DateWorkArea current;
    private ControlTable controlTable;
    private WorkArea activeInd;
    private Common automaticGenerateIwo1De;
    private Employer previouslyCompared;
    private TextWorkArea timeDel;
    private TextWorkArea dateDel;
    private Common recordsRead;
    private Common recordsUpdated;
    private Common recordsDeleted;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private DateWorkArea max;
    private DateWorkArea null1;
    private TextWorkArea recordType;
    private DateWorkArea beginQuarter;
    private Employer employer;
    private EmployerAddress employerAddress;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Main.
    /// </summary>
    [JsonPropertyName("main")]
    public Employer Main
    {
      get => main ??= new();
      set => main = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private IncomeSource incomeSource;
    private Employer main;
    private Employer employer;
    private EmployerAddress employerAddress;
  }
#endregion
}
