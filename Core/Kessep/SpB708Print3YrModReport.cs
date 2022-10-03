// Program: SP_B708_PRINT_3YR_MOD_REPORT, ID: 1902444778, model: 746.
// Short name: SWEB708P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B708_PRINT_3YR_MOD_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB708Print3YrModReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B708_PRINT_3YR_MOD_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB708Print3YrModReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB708Print3YrModReport.
  /// </summary>
  public SpB708Print3YrModReport(IContext context, Import import, Export export):
    
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
    // 10/14/2014      DDupree   	Initial Creation - CR45789
    // ***********************************************************************************************
    // DATE		Developer	Description
    //  DATE		Developer	Description
    // 01/20/2016      LSS     	CQ50054 - Add Office Service Provider (SP name) 
    // to Report
    //                                         
    // - Remove CC and MA programs from the
    // report
    //                                         
    // - Include only open TANF(AF) programs as
    // of report run date
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB708Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.ProcessDate.Date = local.ProgramProcessingInfo.ProcessDate;
    UseFnBuildTimestampFrmDateTime();
    local.Spaces.Text1 = ";";

    foreach(var item in ReadInfrastructure())
    {
      local.EabFileHandling.Status = "";
      ExitState = "ACO_NN0000_ALL_OK";
      local.CsePersonsWorkSet.Assign(local.Clear);

      if (ReadLegalAction())
      {
        local.CsePersonsWorkSet.Number =
          entities.Infrastructure.CsePersonNumber ?? Spaces(10);
        UseSiReadCsePersonBatch();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "CSE Person number: " + entities
            .Infrastructure.CsePersonNumber + local.ExitStateWorkArea.Message;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          continue;
        }

        local.CsePersonsWorkSet.FormattedName =
          local.CsePersonsWorkSet.FirstName + " " + local
          .CsePersonsWorkSet.MiddleInitial + " " + local
          .CsePersonsWorkSet.LastName;
        local.OfficeId.Text4 = "";

        // CQ50054 01/20/16 Added Service Provider to READ
        local.SpName.Text32 = "";

        if (ReadCaseAssignmentOfficeServiceProvider())
        {
          local.OfficeId.Text4 =
            NumberToString(entities.Office.SystemGeneratedId, 12, 4);

          if (!IsEmpty(local.OfficeId.Text4))
          {
            do
            {
              local.Position.Count =
                Verify(Substring(
                  local.OfficeId.Text4, TextWorkArea.Text4_MaxLength, 1, 1),
                " 123456789");

              if (local.Position.Count > 0)
              {
                local.OfficeId.Text4 =
                  Substring(local.OfficeId.Text4, local.Position.Count + 1, 4 -
                  local.Position.Count);
              }
              else
              {
                goto Test;
              }
            }
            while(AsChar(local.Verify.Flag) != 'Y');
          }

Test:

          local.SpName.Text32 = entities.ServiceProvider.LastName + " " + entities
            .ServiceProvider.FirstName;
        }

        // CQ50054 01/20/16 MA and CC programs no longer included on report
        // END CQ50054 01/20/16
        local.CpFound.Flag = "";
        local.ProgramFound.Flag = "";
        local.EffectiveDate.Text10 = "";

        // CQ50054 - Added cse_person to read to get current person for case
        if (ReadCaseRoleCsePerson1())
        {
          local.CpFound.Flag = "Y";
        }

        local.ProgramFound.Flag = "";

        if (AsChar(local.CpFound.Flag) == 'Y')
        {
          // CQ50054 01/20/16 - added qualifier to retrieve only open TANF (AF) 
          // programs
          if (ReadPersonProgramProgram())
          {
            local.ProgramFound.Flag = "Y";
          }

          // END CQ50054 01/20/16
          if (AsChar(local.ProgramFound.Flag) == 'Y')
          {
            local.Convert.Date = entities.PersonProgram.EffectiveDate;
            UseCabDate2TextWithHyphens();
          }

          if (ReadCaseRoleCsePerson2())
          {
            local.EffectiveDate.Text10 = "";
          }
        }

        local.External.FileInstruction = "WRITE";

        // CQ50054 01/20/16  added Office Service Provider; removed MA and CC 
        // columns from report
        // CQ50054 01/20/16 - put report data inside IF statement since only 
        // want the cases that CP has an open TANF (AF) program to be included
        // on the report.
        if (AsChar(local.ProgramFound.Flag) == 'Y')
        {
          local.Local3YrLine.Text166 = entities.Infrastructure.CaseNumber + " ; " +
            entities.Infrastructure.CsePersonNumber + " ; " + local
            .CsePersonsWorkSet.FormattedName + " ; " + entities
            .LegalAction.StandardNumber + " ; " + local.OfficeId.Text4 + " ; " +
            local.SpName.Text32 + " ; " + local.EffectiveDate.Text10;
          UseSpEabWrite3YrModRpt1();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        // END CQ50054 01/20/16
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Program abended because: " + local
          .ExitStateWorkArea.Message;
        local.EabReportSend.RptDetail = "Case number: " + entities
          .Infrastructure.CaseNumber + local.ExitStateWorkArea.Message;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.External.FileInstruction = "CLOSE";
      UseSpEabWrite3YrModRpt2();

      if (!Equal(local.External.TextReturnCode, "OK"))
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

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveExternal1(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveExternal2(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextLine80 = source.TextLine80;
  }

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.Convert.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.EffectiveDate.Text10 = useExport.TextWorkArea.Text10;
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

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = local.ProcessDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, local.ProcessDate);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.LoacalAe.Flag = useExport.Ae.Flag;
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSpB708Housekeeping()
  {
    var useImport = new SpB708Housekeeping.Import();
    var useExport = new SpB708Housekeeping.Export();

    Call(SpB708Housekeeping.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSpEabWrite3YrModRpt1()
  {
    var useImport = new SpEabWrite3YrModRpt.Import();
    var useExport = new SpEabWrite3YrModRpt.Export();

    MoveExternal2(local.External, useImport.External);
    useImport.Import3YrLine.Text166 = local.Local3YrLine.Text166;
    MoveExternal1(local.External, useExport.External);

    Call(SpEabWrite3YrModRpt.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseSpEabWrite3YrModRpt2()
  {
    var useImport = new SpEabWrite3YrModRpt.Import();
    var useExport = new SpEabWrite3YrModRpt.Export();

    MoveExternal2(local.External, useImport.External);
    MoveExternal1(local.External, useExport.External);

    Call(SpEabWrite3YrModRpt.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private bool ReadCaseAssignmentOfficeServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignmentOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetString(command, "casNo", entities.Infrastructure.CaseNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 10);
        entities.ServiceProvider.LastName = db.GetString(reader, 11);
        entities.ServiceProvider.FirstName = db.GetString(reader, 12);
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber", entities.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    entities.N2d.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber", entities.Infrastructure.CaseNumber ?? "");
        db.SetString(
          command, "cspNumber", entities.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.N2d.CasNumber = db.GetString(reader, 0);
        entities.N2d.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.N2d.Type1 = db.GetString(reader, 2);
        entities.N2d.Identifier = db.GetInt32(reader, 3);
        entities.N2d.StartDate = db.GetNullableDate(reader, 4);
        entities.N2d.EndDate = db.GetNullableDate(reader, 5);
        entities.N2d.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.N2d.Type1);
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.ProcessDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 1);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 2);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 3);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 4);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 7);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          entities.Infrastructure.DenormNumeric12.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadPersonProgramProgram()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;
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
    /// A value of SpName.
    /// </summary>
    [JsonPropertyName("spName")]
    public WorkArea SpName
    {
      get => spName ??= new();
      set => spName = value;
    }

    /// <summary>
    /// A value of Cc.
    /// </summary>
    [JsonPropertyName("cc")]
    public Common Cc
    {
      get => cc ??= new();
      set => cc = value;
    }

    /// <summary>
    /// A value of Ma.
    /// </summary>
    [JsonPropertyName("ma")]
    public Common Ma
    {
      get => ma ??= new();
      set => ma = value;
    }

    /// <summary>
    /// A value of Local3YrLine.
    /// </summary>
    [JsonPropertyName("local3YrLine")]
    public WorkArea Local3YrLine
    {
      get => local3YrLine ??= new();
      set => local3YrLine = value;
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
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public CsePersonsWorkSet Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of LoacalAe.
    /// </summary>
    [JsonPropertyName("loacalAe")]
    public Common LoacalAe
    {
      get => loacalAe ??= new();
      set => loacalAe = value;
    }

    /// <summary>
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public Common Verify
    {
      get => verify ??= new();
      set => verify = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of OfficeId.
    /// </summary>
    [JsonPropertyName("officeId")]
    public TextWorkArea OfficeId
    {
      get => officeId ??= new();
      set => officeId = value;
    }

    /// <summary>
    /// A value of Spaces.
    /// </summary>
    [JsonPropertyName("spaces")]
    public TextWorkArea Spaces
    {
      get => spaces ??= new();
      set => spaces = value;
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
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public DateWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of EffectiveDate.
    /// </summary>
    [JsonPropertyName("effectiveDate")]
    public TextWorkArea EffectiveDate
    {
      get => effectiveDate ??= new();
      set => effectiveDate = value;
    }

    /// <summary>
    /// A value of ProgramFound.
    /// </summary>
    [JsonPropertyName("programFound")]
    public Common ProgramFound
    {
      get => programFound ??= new();
      set => programFound = value;
    }

    /// <summary>
    /// A value of Report.
    /// </summary>
    [JsonPropertyName("report")]
    public EabReportSend Report
    {
      get => report ??= new();
      set => report = value;
    }

    /// <summary>
    /// A value of CpFound.
    /// </summary>
    [JsonPropertyName("cpFound")]
    public Common CpFound
    {
      get => cpFound ??= new();
      set => cpFound = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
    }

    private WorkArea spName;
    private Common cc;
    private Common ma;
    private WorkArea local3YrLine;
    private External external;
    private CsePersonsWorkSet clear;
    private Common loacalAe;
    private Common verify;
    private Common position;
    private TextWorkArea officeId;
    private TextWorkArea spaces;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea convert;
    private TextWorkArea effectiveDate;
    private Common programFound;
    private EabReportSend report;
    private Common cpFound;
    private DateWorkArea processDate;
    private ExitStateWorkArea exitStateWorkArea;
    private External passArea;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabReportSend open;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of N2d.
    /// </summary>
    [JsonPropertyName("n2d")]
    public CaseRole N2d
    {
      get => n2d ??= new();
      set => n2d = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private CaseRole n2d;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private CaseAssignment caseAssignment;
    private CsePerson csePerson;
    private Case1 case1;
    private CaseRole caseRole;
    private Program program;
    private PersonProgram personProgram;
    private LegalAction legalAction;
    private Infrastructure infrastructure;
  }
#endregion
}
