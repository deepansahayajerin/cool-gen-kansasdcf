// Program: SRRUN156_COLLECTIONS_EXTRACT, ID: 372817140, model: 746.
// Short name: SWEF740B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SRRUN156_COLLECTIONS_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class Srrun156CollectionsExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SRRUN156_COLLECTIONS_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Srrun156CollectionsExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Srrun156CollectionsExtract.
  /// </summary>
  public Srrun156CollectionsExtract(IContext context, Import import,
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
    // *******************************************************************
    // **               M A I N T E N A N C E     L O G                 **
    // *******************************************************************
    // *
    // 
    // *
    // *   Date    Developer   PR#    Description                        *
    // *   ----    ---------   ---    -----------                        *
    // * 01/11/00  SWSRCHF  H00082616 Date selection parameter set up    *
    // *
    // 
    // incorrectly
    // *
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Found.Flag = "N";

    if (ReadProgramProcessingInfo())
    {
      // ** Problem report H00082616
      // ** 01/11/00 SWSRCHF
      // **
      // ** start
      if (Month(entities.ProgramProcessingInfo.ProcessDate) == 1)
      {
        local.ProcessYyyymm.Month = 12;
        local.ProcessYyyymm.Year =
          Year(AddYears(entities.ProgramProcessingInfo.ProcessDate, -1));
      }
      else
      {
        local.ProcessYyyymm.Month =
          Month(AddMonths(entities.ProgramProcessingInfo.ProcessDate, -1));
        local.ProcessYyyymm.Year =
          Year(entities.ProgramProcessingInfo.ProcessDate);
      }

      // ** end
      // **
      // ** 01/11/00 SWSRCHF
      // ** Problem report H00082616
      local.Found.Flag = "Y";
    }

    // ***
    // *** Open the Error Report
    // ***
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProcessDate = entities.ProgramProcessingInfo.ProcessDate;
    local.NeededToOpen.ProgramName = entities.ProgramProcessingInfo.Name;
    UseCabAccessErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    if (AsChar(local.Found.Flag) == 'N')
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF_AB";
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Program Processing Info not found";
      UseCabAccessErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // ***
      // *** Close the Error Report
      // ***
      local.EabFileHandling.Action = "CLOSE";
      UseCabAccessErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";
      }

      return;
    }

    UseCabGetMonthlyCollections();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      // ***
      // *** Close the Error Report
      // ***
      local.EabFileHandling.Action = "CLOSE";
      UseCabAccessErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";

        return;
      }

      return;
    }

    // ***
    // *** Close the Error Report
    // ***
    local.EabFileHandling.Action = "CLOSE";
    UseCabAccessErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Month = source.Month;
    target.Year = source.Year;
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabAccessErrorReport1()
  {
    var useImport = new CabAccessErrorReport.Import();
    var useExport = new CabAccessErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    MoveEabFileHandling(local.EabFileHandling, useImport.EabFileHandling);

    Call(CabAccessErrorReport.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseCabAccessErrorReport2()
  {
    var useImport = new CabAccessErrorReport.Import();
    var useExport = new CabAccessErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    MoveEabFileHandling(local.EabFileHandling, useImport.EabFileHandling);

    Call(CabAccessErrorReport.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseCabAccessErrorReport3()
  {
    var useImport = new CabAccessErrorReport.Import();
    var useExport = new CabAccessErrorReport.Export();

    MoveEabFileHandling(local.EabFileHandling, useImport.EabFileHandling);

    Call(CabAccessErrorReport.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseCabGetMonthlyCollections()
  {
    var useImport = new CabGetMonthlyCollections.Import();
    var useExport = new CabGetMonthlyCollections.Export();

    MoveDateWorkArea(local.ProcessYyyymm, useImport.Process);

    Call(CabGetMonthlyCollections.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      null,
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
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
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of ProcessYyyymm.
    /// </summary>
    [JsonPropertyName("processYyyymm")]
    public DateWorkArea ProcessYyyymm
    {
      get => processYyyymm ??= new();
      set => processYyyymm = value;
    }

    private Common errorFound;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private Common found;
    private DateWorkArea processYyyymm;
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
