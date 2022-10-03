// Program: FN_B692_NOA_ENTRIES_INTERFACE, ID: 374399079, model: 746.
// Short name: SWEF692B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B692_NOA_ENTRIES_INTERFACE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB692NoaEntriesInterface: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B692_NOA_ENTRIES_INTERFACE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB692NoaEntriesInterface(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB692NoaEntriesInterface.
  /// </summary>
  public FnB692NoaEntriesInterface(IContext context, Import import,
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
    // ****************************************************************************
    //    Date     Programmer   Request      Description
    // ----------  -----------  -----------  
    // --------------------------------------
    // 03/23/2000  Rick Moody                Initial Version
    // 11/16/2000  Ed Lyman     WR172 SEG-Q  Store process date, instead of 
    // current
    //                                       
    // date in ppi last rund date.
    //                                       
    // Read Legal Action Descending
    // Timestamp.
    //                                       
    // Timestamp greater than (instead of
    //                                       
    // greater than or equal to).
    // 05/22/2001   Vithal Madhira  PR# 120078  - KPC NOA Interface  should 
    // default to KPC Max   Date for all End Dates.
    // 06/01/01  Tom Bobb PR# 00121082 - Changed  logic in
    // read to only compare the date portion of the timestamp
    // to eliminated extracting previous days NOA's.
    // *********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB692Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.FileCount.Count = 1;
    local.SrsHighDate.Date = new DateTime(2099, 12, 31);
    local.KpcHighDate.Date = new DateTime(9999, 12, 31);
    local.BatchTimestampWorkArea.TextDateYyyy =
      NumberToString(Year(local.LastRun.Date), 12, 4);
    local.BatchTimestampWorkArea.TextDateMm =
      NumberToString(Month(local.LastRun.Date), 14, 2);
    local.BatchTimestampWorkArea.TestDateDd =
      NumberToString(Day(local.LastRun.Date), 14, 2);
    local.BatchTimestampWorkArea.IefTimestamp =
      Timestamp(local.BatchTimestampWorkArea.TextDateYyyy + "-" + local
      .BatchTimestampWorkArea.TextDateMm + "-" + local
      .BatchTimestampWorkArea.TestDateDd);
    local.PartialTermKansas.ActionTaken = "TERMPALN";
    local.PartialTermNonKansas.ActionTaken = "TERMPARN";
    local.TermKansas.ActionTaken = "TERMASSN";
    local.TermNonKansas.ActionTaken = "TERMASLN";
    local.NoaKansas.ActionTaken = "ASSIGNLN";
    local.NoaNonKansas.ActionTaken = "ASSIGNN";

    // >>
    // Problem report # 00121082 Corrected problem of extracting
    // previous days NOA's. Changed time stamp compare to
    // compare the date portion of the created_tstamp.
    // Tom Bobb 6/1/01
    foreach(var item in ReadLegalAction2())
    {
      if (!IsEmpty(local.Prev.StandardNumber) && !
        Equal(entities.LegalAction.StandardNumber, local.Prev.StandardNumber))
      {
        // *******************************
        // Set header record attributes
        // *******************************
        local.HeaderRecord.ActionCode = "UCO";
        local.HeaderRecord.RecordType = "1";
        local.HeaderRecord.TransactionDate =
          NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8,
          8);
        local.HeaderRecord.Userid = "SWEFB692";
        local.HeaderRecord.Timestamp =
          local.BatchTimestampWorkArea.TextDateYyyy + "-" + local
          .BatchTimestampWorkArea.TextDateMm + "-" + local
          .BatchTimestampWorkArea.TestDateDd + "-" + local.Header.TestTimeHh + "."
          + local.Header.TextTimeMm + "." + local.Header.TextTimeSs;
        UseCabProcessCourtOrderInfo();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseFnB692PrintErrorLine();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }
        else
        {
          ++local.NoasProcessed.Count;
        }
      }
      else if (Equal(entities.LegalAction.StandardNumber,
        local.Prev.StandardNumber))
      {
        continue;
      }

      MoveLegalAction(entities.LegalAction, local.Prev);
      ++local.NoasRead.Count;

      // ******************************
      // Get Court Order information
      // ******************************
      if (ReadLegalAction1())
      {
        // ***********************************
        // Check state court order is from
        // ***********************************
        ReadTribunalFips2();
        local.CourtOrderRecord.ModificationDate =
          NumberToString(
            DateToInt(entities.Original.LastModificationReviewDate), 8, 8);
        local.CourtOrderRecord.StartDate =
          NumberToString(DateToInt(entities.Original.FiledDate), 8, 8);

        // ----------------------------------------------------------------------------
        // Per PR# 120078, KPC NOA Interface should default to KPC Max Date for 
        // all End Dates.
        //                                                    
        // Vithal Madhira (05/22/2001)
        // ------------------------------------------------------------------------
        local.CourtOrderRecord.EndDate =
          NumberToString(DateToInt(local.KpcHighDate.Date), 8, 8);

        if (Equal(entities.Fips.StateAbbreviation, "KS"))
        {
          local.CourtOrderRecord.CountyId =
            Substring(entities.Original.StandardNumber, 1, 2);
          local.Find.Count = Find(entities.Original.StandardNumber, "*");

          if (local.Find.Count == 0)
          {
            local.CourtOrderRecord.CourtOrderNumber =
              Substring(entities.Original.StandardNumber, 3, 10);
          }
          else
          {
            local.CourtOrderRecord.CourtOrderNumber =
              Substring(entities.Original.StandardNumber,
              LegalAction.StandardNumber_MaxLength, 3, local.Find.Count - 3) + " " +
              Substring
              (entities.Original.StandardNumber,
              LegalAction.StandardNumber_MaxLength, local.Find.Count + 1, 12 -
              local.Find.Count);
          }

          // *******************
          // Get City Indicator
          // *******************
          if (Equal(local.CourtOrderRecord.CountyId, "CL") || Equal
            (local.CourtOrderRecord.CountyId, "LB") || Equal
            (local.CourtOrderRecord.CountyId, "MG"))
          {
            local.Length.Count =
              Length(TrimEnd(entities.Original.CourtCaseNumber));
            local.Temp.Flag =
              Substring(entities.Original.CourtCaseNumber, local.Length.Count, 1);
              

            if (AsChar(local.Temp.Flag) >= 'A' && AsChar(local.Temp.Flag) <= 'Z'
              )
            {
              local.CourtOrderRecord.CityIndicator = local.Temp.Flag;
            }
            else
            {
              local.CourtOrderRecord.CityIndicator = "";
            }
          }
          else
          {
            local.CourtOrderRecord.CityIndicator = "";
          }
        }
        else
        {
          local.CourtOrderRecord.CountyId = "IN";
          local.CourtOrderRecord.CourtOrderNumber =
            entities.Original.StandardNumber ?? Spaces(12);
        }
      }
      else
      {
        ReadTribunalFips1();
        local.CourtOrderRecord.ModificationDate =
          NumberToString(DateToInt(
            entities.LegalAction.LastModificationReviewDate), 8, 8);
        local.CourtOrderRecord.StartDate =
          NumberToString(DateToInt(entities.LegalAction.FiledDate), 8, 8);

        // ----------------------------------------------------------------------------
        // Per PR# 120078, KPC NOA Interface should default to KPC Max Date for 
        // all End Dates.
        //                                                    
        // Vithal Madhira (05/22/2001)
        // ------------------------------------------------------------------------
        local.CourtOrderRecord.EndDate =
          NumberToString(DateToInt(local.KpcHighDate.Date), 8, 8);

        if (Equal(entities.Fips.StateAbbreviation, "KS"))
        {
          local.CourtOrderRecord.CountyId =
            Substring(entities.LegalAction.StandardNumber, 1, 2);
          local.Find.Count = Find(entities.LegalAction.StandardNumber, "*");

          if (local.Find.Count == 0)
          {
            local.CourtOrderRecord.CourtOrderNumber =
              Substring(entities.LegalAction.StandardNumber, 3, 10);
          }
          else
          {
            local.CourtOrderRecord.CourtOrderNumber =
              Substring(entities.LegalAction.StandardNumber,
              LegalAction.StandardNumber_MaxLength, 3, local.Find.Count - 3) + " " +
              Substring
              (entities.LegalAction.StandardNumber,
              LegalAction.StandardNumber_MaxLength, local.Find.Count + 1, 12 -
              local.Find.Count);
          }

          // *******************
          // Get City Indicator
          // *******************
          if (Equal(local.CourtOrderRecord.CountyId, "CL") || Equal
            (local.CourtOrderRecord.CountyId, "LB") || Equal
            (local.CourtOrderRecord.CountyId, "MG"))
          {
            local.Length.Count =
              Length(TrimEnd(entities.LegalAction.CourtCaseNumber));
            local.Temp.Flag =
              Substring(entities.LegalAction.CourtCaseNumber,
              local.Length.Count, 1);

            if (AsChar(local.Temp.Flag) >= 'A' && AsChar(local.Temp.Flag) <= 'Z'
              )
            {
              local.CourtOrderRecord.CityIndicator = local.Temp.Flag;
            }
            else
            {
              local.CourtOrderRecord.CityIndicator = "";
            }
          }
          else
          {
            local.CourtOrderRecord.CityIndicator = "";
          }
        }
        else
        {
          local.CourtOrderRecord.CountyId = "IN";
          local.CourtOrderRecord.CourtOrderNumber =
            entities.LegalAction.StandardNumber ?? Spaces(12);
        }
      }

      local.CourtOrderRecord.RecordType = "2";

      if (Equal(entities.LegalAction.ActionTaken,
        local.PartialTermKansas.ActionTaken) || Equal
        (entities.LegalAction.ActionTaken,
        local.PartialTermNonKansas.ActionTaken))
      {
        local.CourtOrderRecord.CourtOrderType = "NIVA";
      }
      else if (Equal(entities.LegalAction.ActionTaken,
        local.NoaKansas.ActionTaken) || Equal
        (entities.LegalAction.ActionTaken, local.NoaNonKansas.ActionTaken))
      {
        local.CourtOrderRecord.CourtOrderType = "IVD";
      }
      else
      {
        local.CourtOrderRecord.CourtOrderType = "NIVD";
      }
    }

    if (local.NoasRead.Count > 0)
    {
      // *******************************
      // Set header record attributes
      // *******************************
      local.HeaderRecord.ActionCode = "UCO";
      local.HeaderRecord.RecordType = "1";
      local.HeaderRecord.TransactionDate =
        NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8, 8);
        
      local.HeaderRecord.Userid = "SWEFB692";
      local.HeaderRecord.Timestamp =
        local.BatchTimestampWorkArea.TextDateYyyy + "-" + local
        .BatchTimestampWorkArea.TextDateMm + "-" + local
        .BatchTimestampWorkArea.TestDateDd + "-" + local.Header.TestTimeHh + "."
        + local.Header.TextTimeMm + "." + local.Header.TextTimeSs;
      UseCabProcessCourtOrderInfo();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseFnB692PrintErrorLine();
      }
      else
      {
        ++local.NoasProcessed.Count;
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ****************************
      // UPDATE PROCESSING DATA
      // ****************************
      local.ProgramProcessingInfo.ParameterList = "N" + NumberToString
        (DateToInt(local.ProgramProcessingInfo.ProcessDate), 8, 8);
      UseUpdatePgmProcessingInfo();
    }

    local.CloseInd.Flag = "Y";

    // ****************************
    // CLOSE OUTPUT FILE
    // ****************************
    local.KpcExternalParms.Parm1 = "CF";
    UseFnExtWriteInterfaceFile();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // : Close the Error Report.
      UseFnB692PrintErrorLine();
      UseFnB692PrintControlTotals();
    }
    else
    {
      // : Report the error that occurred and close the Error Report.
      //   ABEND the procedure once reporting is complete.
      UseFnB692PrintErrorLine();
      UseFnB692PrintControlTotals();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveKpcExternalParms(KpcExternalParms source,
    KpcExternalParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private void UseCabProcessCourtOrderInfo()
  {
    var useImport = new CabProcessCourtOrderInfo.Import();
    var useExport = new CabProcessCourtOrderInfo.Export();

    useImport.FileCount.Count = local.FileCount.Count;
    useImport.HeaderRecord.Assign(local.HeaderRecord);
    useImport.CourtOrderRecord.Assign(local.CourtOrderRecord);

    Call(CabProcessCourtOrderInfo.Execute, useImport, useExport);
  }

  private void UseFnB692Housekeeping()
  {
    var useImport = new FnB692Housekeeping.Import();
    var useExport = new FnB692Housekeeping.Export();

    Call(FnB692Housekeeping.Execute, useImport, useExport);

    local.ZdelCommon.Flag = useExport.ZdelCommon.Flag;
    local.LastRun.Date = useExport.LastRun.Date;
    local.ZdelDateWorkArea.Date = useExport.ZdelDateWorkArea.Date;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseFnB692PrintControlTotals()
  {
    var useImport = new FnB692PrintControlTotals.Import();
    var useExport = new FnB692PrintControlTotals.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;
    useImport.CourtOrdersRead.Count = local.NoasRead.Count;
    useImport.CourtOrdersProcessed.Count = local.NoasProcessed.Count;

    Call(FnB692PrintControlTotals.Execute, useImport, useExport);
  }

  private void UseFnB692PrintErrorLine()
  {
    var useImport = new FnB692PrintErrorLine.Import();
    var useExport = new FnB692PrintErrorLine.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;

    Call(FnB692PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnExtWriteInterfaceFile()
  {
    var useImport = new FnExtWriteInterfaceFile.Import();
    var useExport = new FnExtWriteInterfaceFile.Export();

    MoveKpcExternalParms(local.KpcExternalParms, useImport.KpcExternalParms);
    MoveKpcExternalParms(local.KpcExternalParms, useExport.KpcExternalParms);

    Call(FnExtWriteInterfaceFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.KpcExternalParms);
  }

  private void UseUpdatePgmProcessingInfo()
  {
    var useImport = new UpdatePgmProcessingInfo.Import();
    var useExport = new UpdatePgmProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    Call(UpdatePgmProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadLegalAction1()
  {
    entities.Original.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Original.Identifier = db.GetInt32(reader, 0);
        entities.Original.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.Original.Classification = db.GetString(reader, 2);
        entities.Original.ActionTaken = db.GetString(reader, 3);
        entities.Original.FiledDate = db.GetNullableDate(reader, 4);
        entities.Original.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.Original.EndDate = db.GetNullableDate(reader, 6);
        entities.Original.StandardNumber = db.GetNullableString(reader, 7);
        entities.Original.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.Original.TrbId = db.GetNullableInt32(reader, 9);
        entities.Original.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetDate(command, "date", local.LastRun.Date.GetValueOrDefault());
        db.SetString(command, "actionTaken1", local.TermKansas.ActionTaken);
        db.SetString(command, "actionTaken2", local.TermNonKansas.ActionTaken);
        db.SetString(
          command, "actionTaken3", local.PartialTermKansas.ActionTaken);
        db.SetString(
          command, "actionTaken4", local.PartialTermNonKansas.ActionTaken);
        db.SetString(command, "actionTaken5", local.NoaKansas.ActionTaken);
        db.SetString(command, "actionTaken6", local.NoaNonKansas.ActionTaken);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.Classification = db.GetString(reader, 2);
        entities.LegalAction.ActionTaken = db.GetString(reader, 3);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 10);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadTribunalFips1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;

    return Read("ReadTribunalFips1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Fips.Location = db.GetInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Fips.County = db.GetInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Fips.State = db.GetInt32(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;
      });
  }

  private bool ReadTribunalFips2()
  {
    System.Diagnostics.Debug.Assert(entities.Original.Populated);
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;

    return Read("ReadTribunalFips2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.Original.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Fips.Location = db.GetInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Fips.County = db.GetInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Fips.State = db.GetInt32(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;
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
    /// A value of FileCount.
    /// </summary>
    [JsonPropertyName("fileCount")]
    public Common FileCount
    {
      get => fileCount ??= new();
      set => fileCount = value;
    }

    /// <summary>
    /// A value of KpcHighDate.
    /// </summary>
    [JsonPropertyName("kpcHighDate")]
    public DateWorkArea KpcHighDate
    {
      get => kpcHighDate ??= new();
      set => kpcHighDate = value;
    }

    /// <summary>
    /// A value of SrsHighDate.
    /// </summary>
    [JsonPropertyName("srsHighDate")]
    public DateWorkArea SrsHighDate
    {
      get => srsHighDate ??= new();
      set => srsHighDate = value;
    }

    /// <summary>
    /// A value of Find.
    /// </summary>
    [JsonPropertyName("find")]
    public Common Find
    {
      get => find ??= new();
      set => find = value;
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
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public BatchTimestampWorkArea Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of ZdelCommon.
    /// </summary>
    [JsonPropertyName("zdelCommon")]
    public Common ZdelCommon
    {
      get => zdelCommon ??= new();
      set => zdelCommon = value;
    }

    /// <summary>
    /// A value of LastRun.
    /// </summary>
    [JsonPropertyName("lastRun")]
    public DateWorkArea LastRun
    {
      get => lastRun ??= new();
      set => lastRun = value;
    }

    /// <summary>
    /// A value of ZdelDateWorkArea.
    /// </summary>
    [JsonPropertyName("zdelDateWorkArea")]
    public DateWorkArea ZdelDateWorkArea
    {
      get => zdelDateWorkArea ??= new();
      set => zdelDateWorkArea = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
    }

    /// <summary>
    /// A value of NoasRead.
    /// </summary>
    [JsonPropertyName("noasRead")]
    public Common NoasRead
    {
      get => noasRead ??= new();
      set => noasRead = value;
    }

    /// <summary>
    /// A value of NoasProcessed.
    /// </summary>
    [JsonPropertyName("noasProcessed")]
    public Common NoasProcessed
    {
      get => noasProcessed ??= new();
      set => noasProcessed = value;
    }

    /// <summary>
    /// A value of KpcExternalParms.
    /// </summary>
    [JsonPropertyName("kpcExternalParms")]
    public KpcExternalParms KpcExternalParms
    {
      get => kpcExternalParms ??= new();
      set => kpcExternalParms = value;
    }

    /// <summary>
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecord HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
    }

    /// <summary>
    /// A value of CourtOrderRecord.
    /// </summary>
    [JsonPropertyName("courtOrderRecord")]
    public CourtOrderRecord CourtOrderRecord
    {
      get => courtOrderRecord ??= new();
      set => courtOrderRecord = value;
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
    /// A value of TermNonKansas.
    /// </summary>
    [JsonPropertyName("termNonKansas")]
    public LegalAction TermNonKansas
    {
      get => termNonKansas ??= new();
      set => termNonKansas = value;
    }

    /// <summary>
    /// A value of TermKansas.
    /// </summary>
    [JsonPropertyName("termKansas")]
    public LegalAction TermKansas
    {
      get => termKansas ??= new();
      set => termKansas = value;
    }

    /// <summary>
    /// A value of PartialTermNonKansas.
    /// </summary>
    [JsonPropertyName("partialTermNonKansas")]
    public LegalAction PartialTermNonKansas
    {
      get => partialTermNonKansas ??= new();
      set => partialTermNonKansas = value;
    }

    /// <summary>
    /// A value of PartialTermKansas.
    /// </summary>
    [JsonPropertyName("partialTermKansas")]
    public LegalAction PartialTermKansas
    {
      get => partialTermKansas ??= new();
      set => partialTermKansas = value;
    }

    /// <summary>
    /// A value of NoaNonKansas.
    /// </summary>
    [JsonPropertyName("noaNonKansas")]
    public LegalAction NoaNonKansas
    {
      get => noaNonKansas ??= new();
      set => noaNonKansas = value;
    }

    /// <summary>
    /// A value of NoaKansas.
    /// </summary>
    [JsonPropertyName("noaKansas")]
    public LegalAction NoaKansas
    {
      get => noaKansas ??= new();
      set => noaKansas = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public LegalAction Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    private Common fileCount;
    private DateWorkArea kpcHighDate;
    private DateWorkArea srsHighDate;
    private Common find;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private BatchTimestampWorkArea header;
    private Common zdelCommon;
    private DateWorkArea lastRun;
    private DateWorkArea zdelDateWorkArea;
    private Common temp;
    private Common length;
    private Common closeInd;
    private Common noasRead;
    private Common noasProcessed;
    private KpcExternalParms kpcExternalParms;
    private HeaderRecord headerRecord;
    private CourtOrderRecord courtOrderRecord;
    private ProgramProcessingInfo programProcessingInfo;
    private LegalAction termNonKansas;
    private LegalAction termKansas;
    private LegalAction partialTermNonKansas;
    private LegalAction partialTermKansas;
    private LegalAction noaNonKansas;
    private LegalAction noaKansas;
    private LegalAction prev;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Original.
    /// </summary>
    [JsonPropertyName("original")]
    public LegalAction Original
    {
      get => original ??= new();
      set => original = value;
    }

    private Tribunal tribunal;
    private Fips fips;
    private LegalAction legalAction;
    private LegalAction original;
  }
#endregion
}
