// Program: FN_RPT3_MED_CASES_WO_ENF, ID: 371123584, model: 746.
// Short name: SWERPT3P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RPT3_MED_CASES_WO_ENF.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnRpt3MedCasesWoEnf: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RPT3_MED_CASES_WO_ENF program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRpt3MedCasesWoEnf(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRpt3MedCasesWoEnf.
  /// </summary>
  public FnRpt3MedCasesWoEnf(IContext context, Import import, Export export):
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
    local.MaxLinesPerPage.Count = 50;
    local.ProgramProcessingInfo.ProcessDate = Now().Date;
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = global.UserId;
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
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.HoldOffice.SystemGeneratedId = 0;
    local.HoldServiceProvider.SystemGeneratedId = 0;
    local.NextPrintLine.Count = 0;

    foreach(var item in ReadOfficeServiceProviderOfficeServiceProvider())
    {
      local.Case1.Number = "";

      foreach(var item1 in ReadCaseCaseAssignment())
      {
        if (Equal(entities.ExistingCase.Number, local.Case1.Number))
        {
          continue;
        }

        local.Case1.Number = entities.ExistingCase.Number;

        // : Skip all cases with a Case Unit Function of ENF.
        foreach(var item2 in ReadCaseUnit())
        {
          if (CharAt(entities.ExistingCaseUnit.State, 1) == 'E')
          {
            goto ReadEach;
          }
        }

        // : Skip all cases with an active CC for any related child.
        if (ReadPersonProgram1())
        {
          continue;
        }
        else
        {
          // : No active CC program found. - Continue Processing.
        }

        // : Skip all cases where AF exists for any related child.
        if (ReadPersonProgram3())
        {
          continue;
        }
        else
        {
          // : No AF program found. - Continue Processing.
        }

        // : Include case if SI, CI, MP, MI, MA or MS exists for any of the 
        // children on the case.
        if (!ReadPersonProgram2())
        {
          continue;
        }

        ++local.CaseCnt.Count;

        if (entities.ExistingOffice.SystemGeneratedId != local
          .HoldOffice.SystemGeneratedId || entities
          .ExistingServiceProvider.SystemGeneratedId != local
          .HoldServiceProvider.SystemGeneratedId || local
          .NextPrintLine.Count >= local.MaxLinesPerPage.Count)
        {
          if (local.NextPrintLine.Count != 0)
          {
            local.EabReportSend.RptDetail = "";
            local.Tmp.Count = local.NextPrintLine.Count;

            for(var limit = local.MaxLinesPerPage.Count; local.Tmp.Count <= limit
              ; ++local.Tmp.Count)
            {
              UseCabControlReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

                return;
              }
            }
          }

          local.EabReportSend.RptDetail = "Office";
          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }

          local.EabReportSend.RptDetail = "     Office Service Provider";
          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }

          local.EabReportSend.RptDetail = "          " + "Case #    " + "  " + "AP #/Name";
            
          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }

          local.HoldOffice.SystemGeneratedId =
            entities.ExistingOffice.SystemGeneratedId;
          local.HoldServiceProvider.SystemGeneratedId = 0;
          local.EabReportSend.RptDetail = entities.ExistingOffice.Name;
          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }

          local.HoldServiceProvider.SystemGeneratedId =
            entities.ExistingServiceProvider.SystemGeneratedId;
          local.FirstNameLength.Count =
            Find(String(
              entities.ExistingServiceProvider.FirstName,
            ServiceProvider.FirstName_MaxLength), " ") - 1;
          local.LastNameLength.Count =
            Find(String(
              entities.ExistingServiceProvider.LastName,
            ServiceProvider.LastName_MaxLength), " ") - 1;
          local.EabReportSend.RptDetail = "     " + Substring
            (entities.ExistingServiceProvider.LastName,
            ServiceProvider.LastName_MaxLength, 1, local.LastNameLength.Count) +
            ", " + Substring
            (entities.ExistingServiceProvider.FirstName,
            ServiceProvider.FirstName_MaxLength, 1,
            local.FirstNameLength.Count) + entities
            .ExistingServiceProvider.MiddleInitial;
          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }

          local.NextPrintLine.Count = 5;
        }

        if (ReadCsePersonAbsentParent())
        {
          local.Ap.Number = entities.ExistingAp.Number;
          UseSiReadCsePersonBatch();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
            local.Ap.FormattedName = "** AP Name Is Unavailable **";
          }

          local.Ap.Number = entities.ExistingAp.Number;
        }
        else
        {
          local.Ap.Number = "";
          local.Ap.FormattedName = "** AP Not Found **";
        }

        local.EabReportSend.RptDetail = "          " + entities
          .ExistingCase.Number + "  " + local.Ap.Number + " " + local
          .Ap.FormattedName;
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

          return;
        }

        ++local.NextPrintLine.Count;

ReadEach:
        ;
      }
    }

    local.EabReportSend.RptDetail = "";
    local.Tmp.Count = local.NextPrintLine.Count;

    for(var limit = local.MaxLinesPerPage.Count; local.Tmp.Count <= limit; ++
      local.Tmp.Count)
    {
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

        return;
      }
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Case Count . . . . . . . . . . . . : " + NumberToString
      (local.CaseCnt.Count, 15);
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
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ap.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.Ap);
  }

  private IEnumerable<bool> ReadCaseCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingCaseAssignment.Populated = false;
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadCaseCaseAssignment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCase.OffGeneratedId = db.GetNullableInt32(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 1);
        entities.ExistingCaseAssignment.CasNo = db.GetString(reader, 1);
        entities.ExistingCase.Status = db.GetNullableString(reader, 2);
        entities.ExistingCaseAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingCaseAssignment.SpdId = db.GetInt32(reader, 6);
        entities.ExistingCaseAssignment.OffId = db.GetInt32(reader, 7);
        entities.ExistingCaseAssignment.OspCode = db.GetString(reader, 8);
        entities.ExistingCaseAssignment.OspDate = db.GetDate(reader, 9);
        entities.ExistingCaseAssignment.Populated = true;
        entities.ExistingCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.ExistingCaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.ExistingCaseUnit.State = db.GetString(reader, 1);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 2);
        entities.ExistingCaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePersonAbsentParent()
  {
    entities.ExistingAp.Populated = false;
    entities.ExistingAbsentParent.Populated = false;

    return Read("ReadCsePersonAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAp.Number = db.GetString(reader, 0);
        entities.ExistingAbsentParent.CspNumber = db.GetString(reader, 0);
        entities.ExistingAbsentParent.CasNumber = db.GetString(reader, 1);
        entities.ExistingAbsentParent.Type1 = db.GetString(reader, 2);
        entities.ExistingAbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAp.Populated = true;
        entities.ExistingAbsentParent.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider()
  {
    entities.ExistingOffice.Populated = false;
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOffice.Name = db.GetString(reader, 1);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 2);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 4);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 5);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 6);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 7);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 8);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 9);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingOffice.Populated = true;
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgram1()
  {
    entities.ExistingPersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.ExistingPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingPersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingPersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.ExistingPersonProgram.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.ExistingPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingPersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingPersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram3()
  {
    entities.ExistingPersonProgram.Populated = false;

    return Read("ReadPersonProgram3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.ExistingPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingPersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingPersonProgram.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of NextPrintLine.
    /// </summary>
    [JsonPropertyName("nextPrintLine")]
    public Common NextPrintLine
    {
      get => nextPrintLine ??= new();
      set => nextPrintLine = value;
    }

    /// <summary>
    /// A value of MaxLinesPerPage.
    /// </summary>
    [JsonPropertyName("maxLinesPerPage")]
    public Common MaxLinesPerPage
    {
      get => maxLinesPerPage ??= new();
      set => maxLinesPerPage = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of LastNameLength.
    /// </summary>
    [JsonPropertyName("lastNameLength")]
    public Common LastNameLength
    {
      get => lastNameLength ??= new();
      set => lastNameLength = value;
    }

    /// <summary>
    /// A value of FirstNameLength.
    /// </summary>
    [JsonPropertyName("firstNameLength")]
    public Common FirstNameLength
    {
      get => firstNameLength ??= new();
      set => firstNameLength = value;
    }

    /// <summary>
    /// A value of HoldOffice.
    /// </summary>
    [JsonPropertyName("holdOffice")]
    public Office HoldOffice
    {
      get => holdOffice ??= new();
      set => holdOffice = value;
    }

    /// <summary>
    /// A value of HoldServiceProvider.
    /// </summary>
    [JsonPropertyName("holdServiceProvider")]
    public ServiceProvider HoldServiceProvider
    {
      get => holdServiceProvider ??= new();
      set => holdServiceProvider = value;
    }

    /// <summary>
    /// A value of CaseCnt.
    /// </summary>
    [JsonPropertyName("caseCnt")]
    public Common CaseCnt
    {
      get => caseCnt ??= new();
      set => caseCnt = value;
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

    private Case1 case1;
    private Common tmp;
    private Common nextPrintLine;
    private Common maxLinesPerPage;
    private CsePersonsWorkSet ap;
    private Common lastNameLength;
    private Common firstNameLength;
    private Office holdOffice;
    private ServiceProvider holdServiceProvider;
    private Common caseCnt;
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
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
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
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public CsePerson ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
    }

    /// <summary>
    /// A value of ExistingAbsentParent.
    /// </summary>
    [JsonPropertyName("existingAbsentParent")]
    public CaseRole ExistingAbsentParent
    {
      get => existingAbsentParent ??= new();
      set => existingAbsentParent = value;
    }

    /// <summary>
    /// A value of ExistingCh.
    /// </summary>
    [JsonPropertyName("existingCh")]
    public CsePerson ExistingCh
    {
      get => existingCh ??= new();
      set => existingCh = value;
    }

    /// <summary>
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CaseRole ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
    }

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
    /// A value of ExistingCaseUnit.
    /// </summary>
    [JsonPropertyName("existingCaseUnit")]
    public CaseUnit ExistingCaseUnit
    {
      get => existingCaseUnit ??= new();
      set => existingCaseUnit = value;
    }

    private Office existingOffice;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private CaseAssignment existingCaseAssignment;
    private Case1 existingCase;
    private CsePerson existingAp;
    private CaseRole existingAbsentParent;
    private CsePerson existingCh;
    private CaseRole existingChild;
    private Program existingProgram;
    private PersonProgram existingPersonProgram;
    private CaseUnit existingCaseUnit;
  }
#endregion
}
