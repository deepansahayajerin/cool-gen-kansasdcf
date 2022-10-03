// Program: FN_BFX9_PERSON_PROGRAM_FIX, ID: 372961116, model: 746.
// Short name: SWEBFX9P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFX9_PERSON_PROGRAM_FIX.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfx9PersonProgramFix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFX9_PERSON_PROGRAM_FIX program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfx9PersonProgramFix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfx9PersonProgramFix.
  /// </summary>
  public FnBfx9PersonProgramFix(IContext context, Import import, Export export):
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
    local.UserId.Text8 = global.UserId;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = local.UserId.Text8;
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    local.ProcessUpdatesInd.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

    if (IsEmpty(local.ProcessUpdatesInd.Flag))
    {
      local.ProcessUpdatesInd.Flag = "N";
    }

    local.EabFileHandling.Action = "OPEN";

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "PARMS:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Perform Updates . . . . . . . : " + local
      .ProcessUpdatesInd.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail =
      "            PERSON      EFFECT-DATE   ASSIGNED-DATE    DISCONTINUE-DATE";
      
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    foreach(var item in ReadPersonProgramCsePerson())
    {
      ++local.ReadCnt.Count;
      local.PersonProgramEffDate.Date =
        entities.ExistingPersonProgram.EffectiveDate;

      if (ReadCaseCaseRole1())
      {
        // Continue
      }
      else
      {
        continue;
      }

      if (ReadCaseCaseRole2())
      {
        // Continue
      }
      else
      {
        continue;
      }

      ++local.ErrorCnt.Count;
      local.AlphaEffectDate.Text10 =
        NumberToString(Month(entities.ExistingPersonProgram.EffectiveDate), 14,
        2) + "-" + NumberToString
        (Day(entities.ExistingPersonProgram.EffectiveDate), 14, 2) + "-" + NumberToString
        (Year(entities.ExistingPersonProgram.EffectiveDate), 12, 4);
      local.AlphaAssignDate.Text10 =
        NumberToString(Month(entities.ExistingPersonProgram.AssignedDate), 14, 2)
        + "-" + NumberToString
        (Day(entities.ExistingPersonProgram.AssignedDate), 14, 2) + "-" + NumberToString
        (Year(entities.ExistingPersonProgram.AssignedDate), 12, 4);
      local.AlphaDiscontinueDate.Text10 =
        NumberToString(Month(entities.ExistingPersonProgram.DiscontinueDate),
        14, 2) + "-" + NumberToString
        (Day(entities.ExistingPersonProgram.DiscontinueDate), 14, 2) + "-" + NumberToString
        (Year(entities.ExistingPersonProgram.DiscontinueDate), 12, 4);
      local.EabReportSend.RptDetail = "**ERROR***" + "   " + entities
        .ExistingCsePerson.Number + "   " + local.AlphaEffectDate.Text10 + "   " +
        local.AlphaAssignDate.Text10 + "   " + local
        .AlphaDiscontinueDate.Text10;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // ********************************************************************
      // Remove the person program row.....
      // *****************************************************************
      if (AsChar(local.ProcessUpdatesInd.Flag) == 'Y')
      {
        DeletePersonProgram();
        ++local.UpdateCnt.Count;
      }

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // : Print Summary Totals
    UseCabTextnum1();
    local.EabReportSend.RptDetail = "Read Count . . . . . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum2();
    local.EabReportSend.RptDetail = "Update Count . . . . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum3();
    local.EabReportSend.RptDetail = "Error Count. . . . . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";

    // **********************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabTextnum1()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.ReadCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum2()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.UpdateCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum3()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.ErrorCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;
    useExport.ProgramProcessingInfo.ParameterList =
      local.ProgramProcessingInfo.ParameterList;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
    local.ProgramProcessingInfo.ParameterList =
      useExport.ProgramProcessingInfo.ParameterList;
  }

  private void DeletePersonProgram()
  {
    Update("DeletePersonProgram",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingPersonProgram.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingPersonProgram.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId",
          entities.ExistingPersonProgram.PrgGeneratedId);
      });
  }

  private bool ReadCaseCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingPersonProgram.Populated);
    entities.ExistingCase.Populated = false;
    entities.ExistingCaseRole.Populated = false;

    return Read("ReadCaseCaseRole1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingPersonProgram.CspNumber);
        db.SetDate(
          command, "date", local.PersonProgramEffDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 2);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 3);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 4);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.ExistingCase.Populated = true;
        entities.ExistingCaseRole.Populated = true;
      });
  }

  private bool ReadCaseCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingPersonProgram.Populated);
    entities.ExistingCase.Populated = false;
    entities.ExistingCaseRole.Populated = false;

    return Read("ReadCaseCaseRole2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingPersonProgram.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 2);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 3);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 4);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.ExistingCase.Populated = true;
        entities.ExistingCaseRole.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramCsePerson()
  {
    entities.ExistingPersonProgram.Populated = false;
    entities.ExistingCsePerson.Populated = false;

    return ReadEach("ReadPersonProgramCsePerson",
      null,
      (db, reader) =>
      {
        entities.ExistingPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingPersonProgram.AssignedDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingPersonProgram.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingPersonProgram.Populated = true;
        entities.ExistingCsePerson.Populated = true;

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
    /// A value of AlphaDiscontinueDate.
    /// </summary>
    [JsonPropertyName("alphaDiscontinueDate")]
    public TextWorkArea AlphaDiscontinueDate
    {
      get => alphaDiscontinueDate ??= new();
      set => alphaDiscontinueDate = value;
    }

    /// <summary>
    /// A value of AlphaAssignDate.
    /// </summary>
    [JsonPropertyName("alphaAssignDate")]
    public TextWorkArea AlphaAssignDate
    {
      get => alphaAssignDate ??= new();
      set => alphaAssignDate = value;
    }

    /// <summary>
    /// A value of AlphaEffectDate.
    /// </summary>
    [JsonPropertyName("alphaEffectDate")]
    public TextWorkArea AlphaEffectDate
    {
      get => alphaEffectDate ??= new();
      set => alphaEffectDate = value;
    }

    /// <summary>
    /// A value of PersonProgramEffDate.
    /// </summary>
    [JsonPropertyName("personProgramEffDate")]
    public DateWorkArea PersonProgramEffDate
    {
      get => personProgramEffDate ??= new();
      set => personProgramEffDate = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public Common Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    /// <summary>
    /// A value of ObId.
    /// </summary>
    [JsonPropertyName("obId")]
    public TextWorkArea ObId
    {
      get => obId ??= new();
      set => obId = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ProcessUpdatesInd.
    /// </summary>
    [JsonPropertyName("processUpdatesInd")]
    public Common ProcessUpdatesInd
    {
      get => processUpdatesInd ??= new();
      set => processUpdatesInd = value;
    }

    /// <summary>
    /// A value of ReadCnt.
    /// </summary>
    [JsonPropertyName("readCnt")]
    public Common ReadCnt
    {
      get => readCnt ??= new();
      set => readCnt = value;
    }

    /// <summary>
    /// A value of UpdateCnt.
    /// </summary>
    [JsonPropertyName("updateCnt")]
    public Common UpdateCnt
    {
      get => updateCnt ??= new();
      set => updateCnt = value;
    }

    /// <summary>
    /// A value of ErrorCnt.
    /// </summary>
    [JsonPropertyName("errorCnt")]
    public Common ErrorCnt
    {
      get => errorCnt ??= new();
      set => errorCnt = value;
    }

    /// <summary>
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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

    private TextWorkArea alphaDiscontinueDate;
    private TextWorkArea alphaAssignDate;
    private TextWorkArea alphaEffectDate;
    private DateWorkArea personProgramEffDate;
    private Common tmp;
    private TextWorkArea obId;
    private WorkArea workArea;
    private DateWorkArea null1;
    private Common processUpdatesInd;
    private Common readCnt;
    private Common updateCnt;
    private Common errorCnt;
    private TextWorkArea userId;
    private DateWorkArea current;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External external;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingProgram.
    /// </summary>
    [JsonPropertyName("existingProgram")]
    public Program ExistingProgram
    {
      get => existingProgram ??= new();
      set => existingProgram = value;
    }

    /// <summary>
    /// A value of ExistingPersonProgram.
    /// </summary>
    [JsonPropertyName("existingPersonProgram")]
    public PersonProgram ExistingPersonProgram
    {
      get => existingPersonProgram ??= new();
      set => existingPersonProgram = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    private Program existingProgram;
    private PersonProgram existingPersonProgram;
    private CsePerson existingCsePerson;
    private Case1 existingCase;
    private CaseRole existingCaseRole;
  }
#endregion
}
