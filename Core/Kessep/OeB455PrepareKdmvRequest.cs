// Program: OE_B455_PREPARE_KDMV_REQUEST, ID: 371365585, model: 746.
// Short name: SWEE455B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B455_PREPARE_KDMV_REQUEST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB455PrepareKdmvRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B455_PREPARE_KDMV_REQUEST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB455PrepareKdmvRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB455PrepareKdmvRequest.
  /// </summary>
  public OeB455PrepareKdmvRequest(IContext context, Import import, Export export)
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
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 02/19/2007      DDupree   	Initial Creation - WR280420
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB455Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.SufValues.Index = -1;

    foreach(var item in ReadCodeValue())
    {
      ++local.SufValues.Index;
      local.SufValues.CheckSize();

      if (local.SufValues.Index + 1 == Local.SufValuesGroup.Capacity)
      {
        ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

        return;
      }

      local.SufValues.Update.SufValues1.Cdvalue = entities.CodeValue.Cdvalue;
    }

    local.StartCommon.Count = 1;
    local.Current.Count = 1;
    local.FieldNumber.Count = 0;
    export.RecordProcessed.Count = import.RecordProcessed.Count + 1;
    local.StartDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.ZeroDate.Date = new DateTime(1, 1, 1);
    local.EndDate.Date = new DateTime(2099, 12, 31);
    local.TotalNumApThatQualfied.Count = 0;
    local.TotalNumberOfErrorsRec.Count = 0;
    local.NumberOfRecordsRead.Count = 0;
    local.CsePerson.Number = "";

    foreach(var item in ReadCsePerson())
    {
      ++local.NumberOfRecordsRead.Count;

      foreach(var item1 in ReadIncarceration())
      {
        if (!Lt(local.ZeroDate.Date, entities.Incarceration.EndDate) || Equal
          (entities.Incarceration.EndDate, local.EndDate.Date))
        {
          goto ReadEach;
        }
      }

      local.KdmvFile.DriverLicenseNumber = "";

      if (ReadKsDriversLicense())
      {
        local.KdmvFile.DriverLicenseNumber =
          entities.KsDriversLicense.KsDvrLicense ?? Spaces(9);
      }

      if (IsEmpty(local.KdmvFile.DriverLicenseNumber))
      {
        if (ReadCsePersonLicense())
        {
          local.KdmvFile.DriverLicenseNumber =
            entities.CsePersonLicense.Number ?? Spaces(9);
        }
        else
        {
          // this is ok if there is no ks driver's license number
        }
      }

      local.StartCsePersonsWorkSet.Number = entities.CsePerson.Number;
      local.CsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
      UseSiReadCsePersonBatch();

      if (IsEmpty(local.CsePersonsWorkSet.Ssn) || Equal
        (local.CsePersonsWorkSet.Ssn, "000000000") || !
        Lt(new DateTime(1, 1, 1), local.CsePersonsWorkSet.Dob))
      {
        continue;

        // next person
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // we will now try to scrape off any suffix the last name might have.
        UseOeScrubSuffixes();

        // we only want to write out one record per obligor
        ++local.TotalNumApThatQualfied.Count;
        local.DateWorkArea.Year = Year(local.CsePersonsWorkSet.Dob);
        local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Year, 15);
        local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
        local.DateWorkArea.Month = Month(local.CsePersonsWorkSet.Dob);
        local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Month, 15);
        local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.DateWorkArea.Day = Day(local.CsePersonsWorkSet.Dob);
        local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Day, 15);
        local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.DateWorkArea.TextDate = local.Year.Text4 + local.Month.Text2 + local
          .Day.Text2;
        local.KdmvFile.Dob = local.DateWorkArea.TextDate;
        local.KdmvFile.Ssn = local.CsePersonsWorkSet.Ssn;
        local.KdmvFile.CsePersonNumber = entities.CsePerson.Number;
        local.KdmvFile.LastName = local.CsePersonsWorkSet.LastName;
        local.KdmvFile.FirstName = local.CsePersonsWorkSet.FirstName;
        local.PassArea.FileInstruction = "WRITE";
        UseOeEabRequestToKdor1();

        if (!Equal(local.PassArea.TextReturnCode, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }
      }

      local.KdmvFile.Assign(local.ClearKdmvFile);

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Program abended because: " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.EabReportSend.RptDetail = "";
        ExitState = "ACO_NN0000_ALL_OK";
        ++local.TotalNumberOfErrorsRec.Count;
      }

ReadEach:
      ;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB455Close();
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabRequestToKdor2();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseOeB455Close();
      local.PassArea.FileInstruction = "CLOSE";
      local.Convert.Text15 =
        NumberToString(local.TotalNumApThatQualfied.Count, 15);
      local.PassArea.TextLine80 = "T   TOTAL NUMBER OF RECORDS: " + local
        .Convert.Text15;
      UseOeEabRequestToKdor2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.LastName = source.LastName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveKdmvFile(KdmvFile source, KdmvFile target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.Ssn = source.Ssn;
    target.Dob = source.Dob;
    target.DriverLicenseNumber = source.DriverLicenseNumber;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private static void MoveSufValuesToGroup(Local.SufValuesGroup source,
    OeScrubSuffixes.Import.GroupGroup target)
  {
    target.CodeValue.Cdvalue = source.SufValues1.Cdvalue;
  }

  private void UseCabErrorReport()
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

  private void UseOeB455Close()
  {
    var useImport = new OeB455Close.Import();
    var useExport = new OeB455Close.Export();

    useImport.NumberOfRecordsRead.Count = local.NumberOfRecordsRead.Count;
    useImport.TotalNumApThatQuailfy.Count = local.TotalNumApThatQualfied.Count;

    Call(OeB455Close.Execute, useImport, useExport);
  }

  private void UseOeB455Housekeeping()
  {
    var useImport = new OeB455Housekeeping.Import();
    var useExport = new OeB455Housekeeping.Export();

    Call(OeB455Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseOeEabRequestToKdor1()
  {
    var useImport = new OeEabRequestToKdor.Import();
    var useExport = new OeEabRequestToKdor.Export();

    MoveKdmvFile(local.KdmvFile, useImport.KdmvFile);
    MoveExternal(local.PassArea, useImport.External);
    useExport.External.Assign(local.PassArea);

    Call(OeEabRequestToKdor.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabRequestToKdor2()
  {
    var useImport = new OeEabRequestToKdor.Import();
    var useExport = new OeEabRequestToKdor.Export();

    MoveExternal(local.PassArea, useImport.External);
    useExport.External.Assign(local.PassArea);

    Call(OeEabRequestToKdor.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeScrubSuffixes()
  {
    var useImport = new OeScrubSuffixes.Import();
    var useExport = new OeScrubSuffixes.Export();

    local.SufValues.CopyTo(useImport.Group, MoveSufValuesToGroup);
    useImport.CsePersonsWorkSet.LastName = local.CsePersonsWorkSet.LastName;

    Call(OeScrubSuffixes.Execute, useImport, useExport);

    local.CsePersonsWorkSet.LastName = useExport.CsePersonsWorkSet.LastName;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.StartCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return ReadEach("ReadCodeValue",
      null,
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
        db.SetNullableDate(
          command, "dateOfDeath", local.ZeroDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return Read("ReadCsePersonLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonLicense.Identifier = db.GetInt32(reader, 0);
        entities.CsePersonLicense.CspNumber = db.GetString(reader, 1);
        entities.CsePersonLicense.IssuingState =
          db.GetNullableString(reader, 2);
        entities.CsePersonLicense.Number = db.GetNullableString(reader, 3);
        entities.CsePersonLicense.Type1 = db.GetNullableString(reader, 4);
        entities.CsePersonLicense.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIncarceration()
  {
    entities.Incarceration.Populated = false;

    return ReadEach("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate", local.ZeroDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 4);
        entities.Incarceration.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Incarceration.Incarcerated = db.GetNullableString(reader, 6);
        entities.Incarceration.Populated = true;

        return true;
      });
  }

  private bool ReadKsDriversLicense()
  {
    entities.KsDriversLicense.Populated = false;

    return Read("ReadKsDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNum", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 1);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 2);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 3);
        entities.KsDriversLicense.Populated = true;
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
    /// <summary>
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
    }

    private Common recordProcessed;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
    }

    private Common recordProcessed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A SufValuesGroup group.</summary>
    [Serializable]
    public class SufValuesGroup
    {
      /// <summary>
      /// A value of SufValues1.
      /// </summary>
      [JsonPropertyName("sufValues1")]
      public CodeValue SufValues1
      {
        get => sufValues1 ??= new();
        set => sufValues1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 250;

      private CodeValue sufValues1;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public WorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

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
    /// A value of ClearCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("clearCsePersonsWorkSet")]
    public CsePersonsWorkSet ClearCsePersonsWorkSet
    {
      get => clearCsePersonsWorkSet ??= new();
      set => clearCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ClearKdmvFile.
    /// </summary>
    [JsonPropertyName("clearKdmvFile")]
    public KdmvFile ClearKdmvFile
    {
      get => clearKdmvFile ??= new();
      set => clearKdmvFile = value;
    }

    /// <summary>
    /// A value of KdmvFile.
    /// </summary>
    [JsonPropertyName("kdmvFile")]
    public KdmvFile KdmvFile
    {
      get => kdmvFile ??= new();
      set => kdmvFile = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Common Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of StartCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("startCsePersonsWorkSet")]
    public CsePersonsWorkSet StartCsePersonsWorkSet
    {
      get => startCsePersonsWorkSet ??= new();
      set => startCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of TotalNumberOfErrorsRec.
    /// </summary>
    [JsonPropertyName("totalNumberOfErrorsRec")]
    public Common TotalNumberOfErrorsRec
    {
      get => totalNumberOfErrorsRec ??= new();
      set => totalNumberOfErrorsRec = value;
    }

    /// <summary>
    /// A value of NumberOfRecordsRead.
    /// </summary>
    [JsonPropertyName("numberOfRecordsRead")]
    public Common NumberOfRecordsRead
    {
      get => numberOfRecordsRead ??= new();
      set => numberOfRecordsRead = value;
    }

    /// <summary>
    /// A value of TotalNumberRecordsFound.
    /// </summary>
    [JsonPropertyName("totalNumberRecordsFound")]
    public Common TotalNumberRecordsFound
    {
      get => totalNumberRecordsFound ??= new();
      set => totalNumberRecordsFound = value;
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
    /// A value of TotalNumApThatQualfied.
    /// </summary>
    [JsonPropertyName("totalNumApThatQualfied")]
    public Common TotalNumApThatQualfied
    {
      get => totalNumApThatQualfied ??= new();
      set => totalNumApThatQualfied = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of StartCommon.
    /// </summary>
    [JsonPropertyName("startCommon")]
    public Common StartCommon
    {
      get => startCommon ??= new();
      set => startCommon = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public TextWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public TextWorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public TextWorkArea Day
    {
      get => day ??= new();
      set => day = value;
    }

    /// <summary>
    /// Gets a value of SufValues.
    /// </summary>
    [JsonIgnore]
    public Array<SufValuesGroup> SufValues => sufValues ??= new(
      SufValuesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SufValues for json serialization.
    /// </summary>
    [JsonPropertyName("sufValues")]
    [Computed]
    public IList<SufValuesGroup> SufValues_Json
    {
      get => sufValues;
      set => SufValues.Assign(value);
    }

    private WorkArea convert;
    private CsePerson csePerson;
    private CsePersonsWorkSet clearCsePersonsWorkSet;
    private KdmvFile clearKdmvFile;
    private KdmvFile kdmvFile;
    private DateWorkArea endDate;
    private DateWorkArea zeroDate;
    private DateWorkArea dateWorkArea;
    private DateWorkArea startDate;
    private WorkArea workArea;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common search;
    private Common phonetic;
    private CsePersonsWorkSet startCsePersonsWorkSet;
    private Common totalNumberOfErrorsRec;
    private Common numberOfRecordsRead;
    private Common totalNumberRecordsFound;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalNumApThatQualfied;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common fieldNumber;
    private Common current;
    private Common startCommon;
    private TextWorkArea year;
    private TextWorkArea month;
    private TextWorkArea day;
    private Array<SufValuesGroup> sufValues;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
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

    private CsePersonLicense csePersonLicense;
    private KsDriversLicense ksDriversLicense;
    private Incarceration incarceration;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private CodeValue codeValue;
    private Code code;
  }
#endregion
}
