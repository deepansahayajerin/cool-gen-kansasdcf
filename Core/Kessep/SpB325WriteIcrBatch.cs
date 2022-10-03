// Program: SP_B325_WRITE_ICR_BATCH, ID: 371218538, model: 746.
// Short name: SWEP325B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B325_WRITE_ICR_BATCH.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB325WriteIcrBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B325_WRITE_ICR_BATCH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB325WriteIcrBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB325WriteIcrBatch.
  /// </summary>
  public SpB325WriteIcrBatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************
    // 
    // Date        Developer             Request #            Desc
    // 
    // __________  __________________   ___________
    // ___________________________________
    // 05/20/2004  AHockman              PR#0040139   Interstate Case 
    // Reconciliation Batch extract  SRRUN134
    // ****************************************************************
    // 
    // Check if ADABAS is available.
    // 
    // ****************************************************************
    UseCabReadAdabasPersonBatch1();

    if (Equal(local.AbendData.AdabasResponseCd, "0148"))
    {
      ExitState = "ADABAS_UNAVAILABLE_RB";

      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = "SWEPB325";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***  Get run parameters for this program
    UseReadProgramProcessingInfo();

    // ********************************************************************
    // 
    // Open the extract file.
    // ********************************************************************
    local.EabFileHandling.Action = "OPEN";
    UseEabWriteIcrExtract2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in opening extract file for ICR report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.MaxDate.Date = new DateTime(2099, 12, 31);
    local.ZeroDate.Date = new DateTime(1, 1, 1);

    foreach(var item in ReadInterstateRequest())
    {
      local.InterstateRequest.OtherStateCaseId =
        entities.EaInterstateRequest.OtherStateCaseId;
      local.InterstateRequest.KsCaseInd =
        entities.EaInterstateRequest.KsCaseInd;
      local.InterstateRequest.OtherStateCaseStatus =
        entities.EaInterstateRequest.OtherStateCaseStatus;
      local.InterstateRequest.OtherStateFips =
        entities.EaInterstateRequest.OtherStateFips;

      if (ReadCase())
      {
        local.CoName.FirstName = "";
        local.CoName.LastName = "";
        local.CsePerson.Number = "";
        local.CsePersonsWorkSet.FirstName = "";
        local.CsePersonsWorkSet.LastName = "";
        local.CsePersonsWorkSet.MiddleInitial = "";
        local.CsePersonsWorkSet.Number = "";
        local.CsePersonsWorkSet.Sex = "";
        local.Case1.Number = entities.EaCase.Number;

        // *** Find CASE ROLES
        foreach(var item1 in ReadCaseRole())
        {
          if (ReadCsePerson())
          {
            local.CsePersonsWorkSet.Number = entities.EaCsePerson.Number;
          }
          else
          {
            // ***********************************************************************
            // Write to
            // the error file and skip.
            // 
            // ***********************************************************************
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "No PERSON Found for  Case ROLE" + entities
              .EaCaseRole.Type1;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "CSE_PERSON_NF";
            }

            continue;
          }

          UseCabReadAdabasPersonBatch2();

          // ***************************************************
          // 
          // Modify our codes to what the Feds use
          // 
          // ***************************************************
          if (Equal(entities.EaCaseRole.Type1, "AP"))
          {
            local.CaseRole.Type1 = "NP";
          }

          if (Equal(entities.EaCaseRole.Type1, "AR"))
          {
            local.CaseRole.Type1 = "CP";
          }

          if (Equal(entities.EaCaseRole.Type1, "CH"))
          {
            local.CaseRole.Type1 = "CH";
          }

          if (AsChar(entities.EaInterstateRequest.KsCaseInd) == 'Y')
          {
            local.InterstateRequest.KsCaseInd = "I";
          }

          if (AsChar(entities.EaInterstateRequest.KsCaseInd) == 'N')
          {
            local.InterstateRequest.KsCaseInd = "R";
          }

          if (AsChar(entities.EaCase.Status) == 'O')
          {
            if (ReadOfficeOfficeServiceProviderServiceProvider())
            {
              local.CoName.FirstName = entities.EaCoServiceProvider.FirstName;
              local.CoName.LastName = entities.EaCoServiceProvider.LastName;
              local.Name.Name = entities.EaCoOffice.Name;
              local.OfficeServiceProvider.WorkPhoneNumber =
                entities.EaCoOfficeServiceProvider.WorkPhoneNumber;
              local.OfficeServiceProvider.WorkPhoneAreaCode =
                entities.EaCoOfficeServiceProvider.WorkPhoneAreaCode;
            }
            else
            {
              continue;
            }
          }

          // ***************************************************
          // Write
          // the person record to the extract file.
          // 
          // ***************************************************
          local.EabFileHandling.Action = "WRITE";
          UseEabWriteIcrExtract1();

          continue;
        }
      }
      else
      {
        // ***********************************************************************
        // Write to the
        // error file and skip.
        // 
        // ***********************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "No Case Found for Interstate Case" + entities
          .EaCase.Number;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          break;
        }

        continue;
      }
    }

    // ********************************************************************
    // 
    // Close the extract file.
    // 
    // ********************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseEabWriteIcrExtract3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in opening extract file for ICR report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writing to control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    local.Close.Number = "CLOSE";
    UseEabReadCsePersonBatch();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhoneNumber = source.WorkPhoneNumber;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

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

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabReadAdabasPersonBatch1()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabReadAdabasPersonBatch2()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Close.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseEabWriteIcrExtract1()
  {
    var useImport = new EabWriteIcrExtract.Import();
    var useExport = new EabWriteIcrExtract.Export();

    useImport.Case1.Number = local.Case1.Number;
    useImport.InterstateRequest.Assign(local.InterstateRequest);
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useImport.CaseRole.Type1 = local.CaseRole.Type1;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveServiceProvider(local.CoName, useImport.ServiceProvider);
    MoveOfficeServiceProvider(local.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(EabWriteIcrExtract.Execute, useImport, useExport);
  }

  private void UseEabWriteIcrExtract2()
  {
    var useImport = new EabWriteIcrExtract.Import();
    var useExport = new EabWriteIcrExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(EabWriteIcrExtract.Execute, useImport, useExport);
  }

  private void UseEabWriteIcrExtract3()
  {
    var useImport = new EabWriteIcrExtract.Import();
    var useExport = new EabWriteIcrExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteIcrExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.EaInterstateRequest.Populated);
    entities.EaCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.EaInterstateRequest.CasINumber ?? "");
      },
      (db, reader) =>
      {
        entities.EaCase.Number = db.GetString(reader, 0);
        entities.EaCase.Status = db.GetNullableString(reader, 1);
        entities.EaCase.StatusDate = db.GetNullableDate(reader, 2);
        entities.EaCase.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.EaCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.EaCaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.EaCase.Number);
      },
      (db, reader) =>
      {
        entities.EaCaseRole.CasNumber = db.GetString(reader, 0);
        entities.EaCaseRole.CspNumber = db.GetString(reader, 1);
        entities.EaCaseRole.Type1 = db.GetString(reader, 2);
        entities.EaCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.EaCaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.EaCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 5);
        entities.EaCaseRole.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.EaCaseRole.Populated);
    entities.EaCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.EaCaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.EaCsePerson.Number = db.GetString(reader, 0);
        entities.EaCsePerson.Type1 = db.GetString(reader, 1);
        entities.EaCsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.EaInterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      null,
      (db, reader) =>
      {
        entities.EaInterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.EaInterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.EaInterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.EaInterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.EaInterstateRequest.KsCaseInd = db.GetString(reader, 4);
        entities.EaInterstateRequest.CasINumber =
          db.GetNullableString(reader, 5);
        entities.EaInterstateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeOfficeServiceProviderServiceProvider()
  {
    entities.EaCoOffice.Populated = false;
    entities.EaCoOfficeServiceProvider.Populated = false;
    entities.EaCoServiceProvider.Populated = false;

    return Read("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "casNo", entities.EaCase.Number);
      },
      (db, reader) =>
      {
        entities.EaCoOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.EaCoOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 0);
        entities.EaCoOffice.Name = db.GetString(reader, 1);
        entities.EaCoOffice.OffOffice = db.GetNullableInt32(reader, 2);
        entities.EaCoOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 3);
        entities.EaCoServiceProvider.SystemGeneratedId = db.GetInt32(reader, 3);
        entities.EaCoOfficeServiceProvider.RoleCode = db.GetString(reader, 4);
        entities.EaCoOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 5);
        entities.EaCoOfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 6);
        entities.EaCoOfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 7);
        entities.EaCoServiceProvider.LastName = db.GetString(reader, 8);
        entities.EaCoServiceProvider.FirstName = db.GetString(reader, 9);
        entities.EaCoOffice.Populated = true;
        entities.EaCoOfficeServiceProvider.Populated = true;
        entities.EaCoServiceProvider.Populated = true;
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
    /// A value of TotalCases.
    /// </summary>
    [JsonPropertyName("totalCases")]
    public Common TotalCases
    {
      get => totalCases ??= new();
      set => totalCases = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of HoldCsePerson.
    /// </summary>
    [JsonPropertyName("holdCsePerson")]
    public CsePerson HoldCsePerson
    {
      get => holdCsePerson ??= new();
      set => holdCsePerson = value;
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
    /// A value of HoldEabReportSend.
    /// </summary>
    [JsonPropertyName("holdEabReportSend")]
    public EabReportSend HoldEabReportSend
    {
      get => holdEabReportSend ??= new();
      set => holdEabReportSend = value;
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
    /// A value of TotalErrors.
    /// </summary>
    [JsonPropertyName("totalErrors")]
    public Common TotalErrors
    {
      get => totalErrors ??= new();
      set => totalErrors = value;
    }

    /// <summary>
    /// A value of Area.
    /// </summary>
    [JsonPropertyName("area")]
    public Office Area
    {
      get => area ??= new();
      set => area = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public Office Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public ServiceProvider Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    /// <summary>
    /// A value of CoName.
    /// </summary>
    [JsonPropertyName("coName")]
    public ServiceProvider CoName
    {
      get => coName ??= new();
      set => coName = value;
    }

    /// <summary>
    /// A value of Type1.
    /// </summary>
    [JsonPropertyName("type1")]
    public Common Type1
    {
      get => type1 ??= new();
      set => type1 = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public CsePersonsWorkSet Close
    {
      get => close ??= new();
      set => close = value;
    }

    private Common totalCases;
    private InterstateRequest interstateRequest;
    private OfficeServiceProvider officeServiceProvider;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea maxDate;
    private DateWorkArea zeroDate;
    private CsePerson holdCsePerson;
    private CaseRole caseRole;
    private EabReportSend holdEabReportSend;
    private AbendData abendData;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common totalErrors;
    private Office area;
    private Office name;
    private ServiceProvider supervisor;
    private ServiceProvider coName;
    private Common type1;
    private Case1 case1;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common errOnAdabasUnavailable;
    private CsePersonsWorkSet close;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EaInterstateRequest.
    /// </summary>
    [JsonPropertyName("eaInterstateRequest")]
    public InterstateRequest EaInterstateRequest
    {
      get => eaInterstateRequest ??= new();
      set => eaInterstateRequest = value;
    }

    /// <summary>
    /// A value of EaCase.
    /// </summary>
    [JsonPropertyName("eaCase")]
    public Case1 EaCase
    {
      get => eaCase ??= new();
      set => eaCase = value;
    }

    /// <summary>
    /// A value of EaCaseRole.
    /// </summary>
    [JsonPropertyName("eaCaseRole")]
    public CaseRole EaCaseRole
    {
      get => eaCaseRole ??= new();
      set => eaCaseRole = value;
    }

    /// <summary>
    /// A value of EaCsePerson.
    /// </summary>
    [JsonPropertyName("eaCsePerson")]
    public CsePerson EaCsePerson
    {
      get => eaCsePerson ??= new();
      set => eaCsePerson = value;
    }

    /// <summary>
    /// A value of EaCoOffice.
    /// </summary>
    [JsonPropertyName("eaCoOffice")]
    public Office EaCoOffice
    {
      get => eaCoOffice ??= new();
      set => eaCoOffice = value;
    }

    /// <summary>
    /// A value of EaCoOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("eaCoOfficeServiceProvider")]
    public OfficeServiceProvider EaCoOfficeServiceProvider
    {
      get => eaCoOfficeServiceProvider ??= new();
      set => eaCoOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of EaCoServiceProvider.
    /// </summary>
    [JsonPropertyName("eaCoServiceProvider")]
    public ServiceProvider EaCoServiceProvider
    {
      get => eaCoServiceProvider ??= new();
      set => eaCoServiceProvider = value;
    }

    /// <summary>
    /// A value of EaCaseAssignment.
    /// </summary>
    [JsonPropertyName("eaCaseAssignment")]
    public CaseAssignment EaCaseAssignment
    {
      get => eaCaseAssignment ??= new();
      set => eaCaseAssignment = value;
    }

    /// <summary>
    /// A value of EaServiceProvider.
    /// </summary>
    [JsonPropertyName("eaServiceProvider")]
    public ServiceProvider EaServiceProvider
    {
      get => eaServiceProvider ??= new();
      set => eaServiceProvider = value;
    }

    /// <summary>
    /// A value of EaOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("eaOfficeServiceProvider")]
    public OfficeServiceProvider EaOfficeServiceProvider
    {
      get => eaOfficeServiceProvider ??= new();
      set => eaOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of EaOfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("eaOfficeServiceProvRelationship")]
    public OfficeServiceProvRelationship EaOfficeServiceProvRelationship
    {
      get => eaOfficeServiceProvRelationship ??= new();
      set => eaOfficeServiceProvRelationship = value;
    }

    /// <summary>
    /// A value of EaArea.
    /// </summary>
    [JsonPropertyName("eaArea")]
    public CseOrganization EaArea
    {
      get => eaArea ??= new();
      set => eaArea = value;
    }

    /// <summary>
    /// A value of EaCseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("eaCseOrganizationRelationship")]
    public CseOrganizationRelationship EaCseOrganizationRelationship
    {
      get => eaCseOrganizationRelationship ??= new();
      set => eaCseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of EaCoCseOrganization.
    /// </summary>
    [JsonPropertyName("eaCoCseOrganization")]
    public CseOrganization EaCoCseOrganization
    {
      get => eaCoCseOrganization ??= new();
      set => eaCoCseOrganization = value;
    }

    private InterstateRequest eaInterstateRequest;
    private Case1 eaCase;
    private CaseRole eaCaseRole;
    private CsePerson eaCsePerson;
    private Office eaCoOffice;
    private OfficeServiceProvider eaCoOfficeServiceProvider;
    private ServiceProvider eaCoServiceProvider;
    private CaseAssignment eaCaseAssignment;
    private ServiceProvider eaServiceProvider;
    private OfficeServiceProvider eaOfficeServiceProvider;
    private OfficeServiceProvRelationship eaOfficeServiceProvRelationship;
    private CseOrganization eaArea;
    private CseOrganizationRelationship eaCseOrganizationRelationship;
    private CseOrganization eaCoCseOrganization;
  }
#endregion
}
