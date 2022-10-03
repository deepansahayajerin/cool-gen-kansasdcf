// Program: OE_HOUSEHOLD_MULTI_CASE_UPDATE, ID: 372804949, model: 746.
// Short name: SWEEB66P
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_HOUSEHOLD_MULTI_CASE_UPDATE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeHouseholdMultiCaseUpdate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HOUSEHOLD_MULTI_CASE_UPDATE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHouseholdMultiCaseUpdate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHouseholdMultiCaseUpdate.
  /// </summary>
  public OeHouseholdMultiCaseUpdate(IContext context, Import import,
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
    // *************************************************************************
    // * David Lowry  July 15, 1999
    // * Initial creation of the prad and eab.  The purpose is to correctly 
    // update  * the multi case indicator on the IM_HOUSEHOLD entity.
    // **************************************************************************
    ExitState = "ACO_NI0000_ACTION_SUCCESSFUL";
    local.EndOfFile.Flag = "";
    local.EabFileHandling.Status = "INITIAL";
    local.ImHousehold.LastUpdatedBy = "BTCHLOAD";
    local.ImHousehold.LastUpdatedTimestamp = Now();
    local.NeededToOpen.ProgramName = "SWEEB666";
    local.NeededToOpen.ProcessDate = Now().Date;

    // *** A series of host language batch programs, external to cool:gen, has 
    // created a sequential file with the keys of the im_household records which
    // need to be updated.
    local.EabFileHandling.Action = "OPEN";
    UseOeEabReadImHholdMultiCase2();
    UseCabErrorReport1();

    if (Equal(local.EabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    do
    {
      // *** Read each im_household key, via the eab, and match into the local 
      // view.
      local.EabFileHandling.Action = "READ";
      UseOeEabReadImHholdMultiCase1();

      if (Equal(local.EabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "OE0000_ERROR_READING_EXT_FILE";

        return;
      }

      if (AsChar(local.EndOfFile.Flag) == 'Y')
      {
        return;
      }
      else
      {
      }

      if (ReadImHousehold())
      {
        try
        {
          UpdateImHousehold();

          // *** next iteration
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0000_IM_HOUSEHOLD_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "OE0000_IM_HOUSEHOLD_PV";

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
        ExitState = "OE0000_IM_HOUSEHOLD_NF";
      }

      if (IsExitState("ACO_NI0000_ACTION_SUCCESSFUL"))
      {
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "ERROR WITH IM_HOUSEHOLD AE_CASE_NO :" + local.ImHousehold.AeCaseNo;
        UseCabErrorReport2();

        return;
      }
    }
    while(AsChar(local.EndOfFile.Flag) != 'Y');
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeEabReadImHholdMultiCase1()
  {
    var useImport = new OeEabReadImHholdMultiCase.Import();
    var useExport = new OeEabReadImHholdMultiCase.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.ImHousehold.AeCaseNo = local.ImHousehold.AeCaseNo;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.LocalEndOfFile.Flag = local.EndOfFile.Flag;

    Call(OeEabReadImHholdMultiCase.Execute, useImport, useExport);

    local.ImHousehold.AeCaseNo = useExport.ImHousehold.AeCaseNo;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.EndOfFile.Flag = useExport.LocalEndOfFile.Flag;
  }

  private void UseOeEabReadImHholdMultiCase2()
  {
    var useImport = new OeEabReadImHholdMultiCase.Import();
    var useExport = new OeEabReadImHholdMultiCase.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(OeEabReadImHholdMultiCase.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadImHousehold()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", local.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.LastUpdatedBy = db.GetString(reader, 1);
        entities.ImHousehold.LastUpdatedTimestamp = db.GetDateTime(reader, 2);
        entities.ImHousehold.ZdelMultiCaseIndicator =
          db.GetNullableString(reader, 3);
        entities.ImHousehold.Populated = true;
      });
  }

  private void UpdateImHousehold()
  {
    var lastUpdatedBy = local.ImHousehold.LastUpdatedBy;
    var lastUpdatedTimestamp = local.ImHousehold.LastUpdatedTimestamp;
    var zdelMultiCaseIndicator = "Y";

    entities.ImHousehold.Populated = false;
    Update("UpdateImHousehold",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableString(command, "multiCaseInd", zdelMultiCaseIndicator);
        db.SetString(command, "aeCaseNo", entities.ImHousehold.AeCaseNo);
      });

    entities.ImHousehold.LastUpdatedBy = lastUpdatedBy;
    entities.ImHousehold.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ImHousehold.ZdelMultiCaseIndicator = zdelMultiCaseIndicator;
    entities.ImHousehold.Populated = true;
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
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
    /// A value of EndOfFile.
    /// </summary>
    [JsonPropertyName("endOfFile")]
    public Common EndOfFile
    {
      get => endOfFile ??= new();
      set => endOfFile = value;
    }

    private ImHousehold imHousehold;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private Common endOfFile;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    private ImHousehold imHousehold;
  }
#endregion
}
