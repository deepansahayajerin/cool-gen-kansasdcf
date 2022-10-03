// Program: OE_BXXX_CLOSE_CASE_ROLES, ID: 372881180, model: 746.
// Short name: SWEEXXXB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_BXXX_CLOSE_CASE_ROLES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeBxxxCloseCaseRoles: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_BXXX_CLOSE_CASE_ROLES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeBxxxCloseCaseRoles(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeBxxxCloseCaseRoles.
  /// </summary>
  public OeBxxxCloseCaseRoles(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.ReportNeeded.Flag = "Y";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseOeBxxxHousekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    foreach(var item in ReadCase())
    {
      local.CaseRoleFound.Flag = "N";

      if (ReadCaseRoleCsePerson())
      {
        local.CaseRoleFound.Flag = "Y";
      }

      if (AsChar(local.CaseRoleFound.Flag) == 'Y')
      {
        if (Equal(entities.CaseRole.EndDate, local.Max.Date))
        {
          continue;
        }
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "NO AR ON CASE NUMBER: " + entities
          .Case1.Number;
        UseCabErrorReport();

        continue;
      }

      ++local.CaseRolesRead.Count;
      local.StatusDate.Text10 =
        NumberToString(Month(entities.Case1.StatusDate), 14, 2) + "-" + NumberToString
        (Day(entities.Case1.StatusDate), 14, 2) + "-" + NumberToString
        (Year(entities.Case1.StatusDate), 12, 4);
      local.EndDate.Text10 =
        NumberToString(Month(entities.CaseRole.EndDate), 14, 2) + "-" + NumberToString
        (Day(entities.CaseRole.EndDate), 14, 2) + "-" + NumberToString
        (Year(entities.CaseRole.EndDate), 12, 4);

      if (AsChar(local.ReportNeeded.Flag) == 'Y')
      {
        if (AsChar(local.Update.Flag) == 'Y')
        {
          local.NeededToWrite.RptDetail = entities.Case1.Number + " " + local
            .StatusDate.Text10 + " " + entities.CsePerson.Number + "   " + entities
            .CaseRole.Type1 + "   " + local.EndDate.Text10 + "   " + "UPDATED";
        }
        else
        {
          local.NeededToWrite.RptDetail = entities.Case1.Number + " " + local
            .StatusDate.Text10 + " " + entities.CsePerson.Number + "   " + entities
            .CaseRole.Type1 + "   " + local.EndDate.Text10 + "   " + "";
        }

        UseCabBusinessReport01();
      }

      if (AsChar(local.Update.Flag) == 'Y')
      {
        try
        {
          UpdateCaseRole();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_ROLE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_ROLE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        ++local.CaseRolesUpdated.Count;
        ++local.CommitCount.Count;

        if (local.CommitCount.Count > 500)
        {
          local.CommitCount.Count = 0;
          UseExtToDoACommit();
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        break;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeBxxxClosing();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseOeBxxxClosing();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeBxxxClosing()
  {
    var useImport = new OeBxxxClosing.Import();
    var useExport = new OeBxxxClosing.Export();

    useImport.Cases.Count = local.CaseRolesRead.Count;
    useImport.MothersDeleted.Count = local.CaseRolesUpdated.Count;

    Call(OeBxxxClosing.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeBxxxHousekeeping()
  {
    var useImport = new OeBxxxHousekeeping.Import();
    var useExport = new OeBxxxHousekeeping.Export();

    Call(OeBxxxHousekeeping.Execute, useImport, useExport);

    local.Update.Flag = useExport.Delete.Flag;
    local.Current.Date = useExport.Process.Date;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      null,
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private void UpdateCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var endDate = local.Max.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = "SWEEBXXX";

    entities.CaseRole.Populated = false;
    Update("UpdateCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "type", entities.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CaseRole.Identifier);
      });

    entities.CaseRole.EndDate = endDate;
    entities.CaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CaseRole.Populated = true;
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
    /// A value of CaseRoleFound.
    /// </summary>
    [JsonPropertyName("caseRoleFound")]
    public Common CaseRoleFound
    {
      get => caseRoleFound ??= new();
      set => caseRoleFound = value;
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
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public TextWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of StatusDate.
    /// </summary>
    [JsonPropertyName("statusDate")]
    public TextWorkArea StatusDate
    {
      get => statusDate ??= new();
      set => statusDate = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of CaseRolesRead.
    /// </summary>
    [JsonPropertyName("caseRolesRead")]
    public Common CaseRolesRead
    {
      get => caseRolesRead ??= new();
      set => caseRolesRead = value;
    }

    /// <summary>
    /// A value of CaseRolesUpdated.
    /// </summary>
    [JsonPropertyName("caseRolesUpdated")]
    public Common CaseRolesUpdated
    {
      get => caseRolesUpdated ??= new();
      set => caseRolesUpdated = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
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
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private Common caseRoleFound;
    private DateWorkArea max;
    private TextWorkArea endDate;
    private TextWorkArea statusDate;
    private External external;
    private Common commitCount;
    private Common caseRolesRead;
    private Common caseRolesUpdated;
    private Common update;
    private ExitStateWorkArea exitStateWorkArea;
    private Common reportNeeded;
    private EabReportSend neededToWrite;
    private DateWorkArea current;
    private DateWorkArea process;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
  }
#endregion
}
