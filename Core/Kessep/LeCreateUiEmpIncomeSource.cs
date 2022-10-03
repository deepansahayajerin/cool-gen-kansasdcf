// Program: LE_CREATE_UI_EMP_INCOME_SOURCE, ID: 945097424, model: 746.
// Short name: SWE03072
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CREATE_UI_EMP_INCOME_SOURCE.
/// </summary>
[Serializable]
public partial class LeCreateUiEmpIncomeSource: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CREATE_UI_EMP_INCOME_SOURCE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCreateUiEmpIncomeSource(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCreateUiEmpIncomeSource.
  /// </summary>
  public LeCreateUiEmpIncomeSource(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    // Date      Developer         Request #  Description
    // --------  ----------------  ---------  ------------------------
    // 05/14/12  GVandy            CQ33628    Initial Development
    // -------------------------------------------------------------------------------------
    // -- This cab was cloned from SI_B273_create_emp_income_source and then 
    // modified for this purpose.
    local.DateWorkArea.Timestamp = Now();

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadEmployer())
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    if (entities.Employer.Populated)
    {
      if (Equal(global.UserId, "SWELB578"))
      {
        // -- DOL weekly withholding batch
        // -- When processing the weekly withholding file we don't want to end 
        // date and create
        //    a new income source.  We'll just use the income source with the 
        // most recent
        //    start date.  If one doesn't exist we'll create a new one.
        if (ReadIncomeSource1())
        {
          goto Test;
        }
      }
      else if (Equal(global.UserId, "SWELB579"))
      {
        // -- DOL Annual notice batch
        // -- End date any existing income source records to DOL for UI 
        // withholding.
        foreach(var item in ReadIncomeSource2())
        {
          try
          {
            UpdateIncomeSource();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "INCOME_SOURCE_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "INCOME_SOURCE_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      if (Equal(global.UserId, "SWELB578"))
      {
        // -- DOL weekly withholding batch
        local.IncomeSource.Note = "Created by weekly DOL UI withholding batch.";
      }
      else if (Equal(global.UserId, "SWELB579"))
      {
        // -- DOL Annual notice batch
        local.IncomeSource.Note = "Created by annual DOL UI Notice batch.";
      }

      // -- Create a new income source record to DOL for UI withholding.
      try
      {
        CreateIncomeSource();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INCOME_SOURCE_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INCOME_SOURCE_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // ***************************************************************
      // This note was retained from the original 
      // SI_B273_create_emp_income_source cab.
      // 11/20/1998   C. Ott
      // This batch program creates data that may be maintained online by the 
      // INCS screen.  Because that screen requires the presence of an 'HP' and
      // an 'HF' Income Source Contact type, they must be created here to ensure
      // functioning of the INCS screen.  This is an issue that may be re-
      // consider in the future but must take into account the requirements of
      // both this batch procedure and the INCS screen.
      // ****************************************************************
      local.IncomeSourceContact.AreaCode = entities.Employer.AreaCode;
      local.IncomeSourceContact.Number =
        (int?)StringToNumber(entities.Employer.PhoneNo);
      local.IncomeSourceContact.Type1 = "HP";
      local.CsePerson.Number = import.CsePerson.Number;
      UseSiIncsCreateIncomeSrcContct();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error creating HP income source contact for cse person = " + local
          .CsePerson.Number + ", Employer FEIN = " + entities.Employer.Ein;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.IncomeSourceContact.AreaCode = 0;
      local.IncomeSourceContact.Number = 0;
      local.IncomeSourceContact.Type1 = "HF";
      UseSiIncsCreateIncomeSrcContct();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error creating HF income source contact for cse person = " + local
          .CsePerson.Number + ", Employer FEIN = " + entities.Employer.Ein;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

Test:

    // -- Call auto IWO cab to trigger ORDIWO2B documents for appropriate cases.
    UseLeAutomaticIwoGeneration();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Unable to create IWO for person:  " + local
        .CsePerson.Number + " Error: " + local.ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
    }
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.ReturnCd = source.ReturnCd;
    target.StartDt = source.StartDt;
  }

  private static void MoveIncomeSourceContact(IncomeSourceContact source,
    IncomeSourceContact target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
    target.ExtensionNo = source.ExtensionNo;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.AreaCode = source.AreaCode;
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

  private void UseLeAutomaticIwoGeneration()
  {
    var useImport = new LeAutomaticIwoGeneration.Import();
    var useExport = new LeAutomaticIwoGeneration.Export();

    MoveIncomeSource(entities.IncomeSource, useImport.IncomeSource);
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Filter.StandardNumber = import.Filter.StandardNumber;
    useImport.InterstateCaseloadOfficeServiceProvider.RoleCode =
      import.InterstateCaseloadOfficeServiceProvider.RoleCode;
    useImport.InterstateCaseloadServiceProvider.SystemGeneratedId =
      import.InterstateCaseloadServiceProvider.SystemGeneratedId;
    useImport.InterstateCaseloadOffice.SystemGeneratedId =
      import.InterstateCaseloadOffice.SystemGeneratedId;
    useImport.CentralOffDefaultAttyOfficeServiceProvider.RoleCode =
      import.CentralOffDefaultAttyOfficeServiceProvider.RoleCode;
    useImport.CentralOffDefaultAttyOffice.SystemGeneratedId =
      import.CentralOffDefaultAttyOffice.SystemGeneratedId;
    useImport.CentralOffDefaultAttyServiceProvider.SystemGeneratedId =
      import.CentralOffDefaultAttyServiceProvider.SystemGeneratedId;

    Call(LeAutomaticIwoGeneration.Execute, useImport, useExport);
  }

  private void UseSiIncsCreateIncomeSrcContct()
  {
    var useImport = new SiIncsCreateIncomeSrcContct.Import();
    var useExport = new SiIncsCreateIncomeSrcContct.Export();

    useImport.IncomeSource.Identifier = entities.IncomeSource.Identifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveIncomeSourceContact(local.IncomeSourceContact,
      useImport.IncomeSourceContact);

    Call(SiIncsCreateIncomeSrcContct.Execute, useImport, useExport);

    local.CsePerson.Number = useImport.CsePerson.Number;
    local.IncomeSourceContact.Assign(useImport.IncomeSourceContact);
  }

  private void CreateIncomeSource()
  {
    var identifier = local.DateWorkArea.Timestamp;
    var type1 = "E";
    var returnDt = import.ProgramProcessingInfo.ProcessDate;
    var returnCd = "U";
    var name = entities.Employer.Name;
    var lastUpdatedBy = global.UserId;
    var cspINumber = entities.CsePerson.Number;
    var empId = entities.Employer.Identifier;
    var endDt = new DateTime(2099, 12, 31);
    var note = local.IncomeSource.Note ?? "";

    CheckValid<IncomeSource>("Type1", type1);
    CheckValid<IncomeSource>("SendTo", "");
    entities.IncomeSource.Populated = false;
    Update("CreateIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(command, "identifier", identifier);
        db.SetString(command, "type", type1);
        db.SetNullableDecimal(command, "lastQtrIncome", 0M);
        db.SetNullableString(command, "lastQtr", "");
        db.SetNullableInt32(command, "lastQtrYr", 0);
        db.SetNullableDate(command, "sentDt", null);
        db.SetNullableDate(command, "returnDt", returnDt);
        db.SetNullableString(command, "returnCd", returnCd);
        db.SetNullableString(command, "name", name);
        db.SetNullableDateTime(command, "lastUpdatedTmst", identifier);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTimestamp", identifier);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetNullableString(command, "code", "");
        db.SetString(command, "cspINumber", cspINumber);
        db.SetNullableInt32(command, "empId", empId);
        db.SetNullableString(command, "sendTo", "");
        db.SetNullableString(command, "workerId", lastUpdatedBy);
        db.SetNullableDate(command, "startDt", returnDt);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "note2", "");
      });

    entities.IncomeSource.Identifier = identifier;
    entities.IncomeSource.Type1 = type1;
    entities.IncomeSource.SentDt = null;
    entities.IncomeSource.ReturnDt = returnDt;
    entities.IncomeSource.ReturnCd = returnCd;
    entities.IncomeSource.Name = name;
    entities.IncomeSource.LastUpdatedTimestamp = identifier;
    entities.IncomeSource.LastUpdatedBy = lastUpdatedBy;
    entities.IncomeSource.CreatedTimestamp = identifier;
    entities.IncomeSource.CreatedBy = lastUpdatedBy;
    entities.IncomeSource.Code = "";
    entities.IncomeSource.CspINumber = cspINumber;
    entities.IncomeSource.EmpId = empId;
    entities.IncomeSource.SendTo = "";
    entities.IncomeSource.WorkerId = lastUpdatedBy;
    entities.IncomeSource.StartDt = returnDt;
    entities.IncomeSource.EndDt = endDt;
    entities.IncomeSource.Note = note;
    entities.IncomeSource.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 3);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 4);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadIncomeSource1()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource1",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetNullableInt32(command, "empId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.SentDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 3);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 4);
        entities.IncomeSource.Name = db.GetNullableString(reader, 5);
        entities.IncomeSource.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.IncomeSource.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.IncomeSource.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.IncomeSource.CreatedBy = db.GetString(reader, 9);
        entities.IncomeSource.Code = db.GetNullableString(reader, 10);
        entities.IncomeSource.CspINumber = db.GetString(reader, 11);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 12);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 13);
        entities.IncomeSource.WorkerId = db.GetNullableString(reader, 14);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 15);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 16);
        entities.IncomeSource.Note = db.GetNullableString(reader, 17);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
      });
  }

  private IEnumerable<bool> ReadIncomeSource2()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource2",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetNullableInt32(command, "empId", entities.Employer.Identifier);
        db.SetNullableDate(
          command, "endDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.SentDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 3);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 4);
        entities.IncomeSource.Name = db.GetNullableString(reader, 5);
        entities.IncomeSource.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.IncomeSource.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.IncomeSource.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.IncomeSource.CreatedBy = db.GetString(reader, 9);
        entities.IncomeSource.Code = db.GetNullableString(reader, 10);
        entities.IncomeSource.CspINumber = db.GetString(reader, 11);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 12);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 13);
        entities.IncomeSource.WorkerId = db.GetNullableString(reader, 14);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 15);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 16);
        entities.IncomeSource.Note = db.GetNullableString(reader, 17);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);

        return true;
      });
  }

  private void UpdateIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var lastUpdatedTimestamp = local.DateWorkArea.Timestamp;
    var lastUpdatedBy = global.UserId;
    var endDt = AddDays(import.ProgramProcessingInfo.ProcessDate, -1);

    entities.IncomeSource.Populated = false;
    Update("UpdateIncomeSource",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetDateTime(
          command, "identifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
      });

    entities.IncomeSource.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IncomeSource.LastUpdatedBy = lastUpdatedBy;
    entities.IncomeSource.EndDt = endDt;
    entities.IncomeSource.Populated = true;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public LegalAction Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseloadOfficeServiceProvider")]
    public OfficeServiceProvider InterstateCaseloadOfficeServiceProvider
    {
      get => interstateCaseloadOfficeServiceProvider ??= new();
      set => interstateCaseloadOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseloadServiceProvider")]
    public ServiceProvider InterstateCaseloadServiceProvider
    {
      get => interstateCaseloadServiceProvider ??= new();
      set => interstateCaseloadServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadOffice.
    /// </summary>
    [JsonPropertyName("interstateCaseloadOffice")]
    public Office InterstateCaseloadOffice
    {
      get => interstateCaseloadOffice ??= new();
      set => interstateCaseloadOffice = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyOfficeServiceProvider")]
    public OfficeServiceProvider CentralOffDefaultAttyOfficeServiceProvider
    {
      get => centralOffDefaultAttyOfficeServiceProvider ??= new();
      set => centralOffDefaultAttyOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyOffice.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyOffice")]
    public Office CentralOffDefaultAttyOffice
    {
      get => centralOffDefaultAttyOffice ??= new();
      set => centralOffDefaultAttyOffice = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyServiceProvider.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyServiceProvider")]
    public ServiceProvider CentralOffDefaultAttyServiceProvider
    {
      get => centralOffDefaultAttyServiceProvider ??= new();
      set => centralOffDefaultAttyServiceProvider = value;
    }

    private Employer employer;
    private CsePerson csePerson;
    private ProgramProcessingInfo programProcessingInfo;
    private LegalAction filter;
    private OfficeServiceProvider interstateCaseloadOfficeServiceProvider;
    private ServiceProvider interstateCaseloadServiceProvider;
    private Office interstateCaseloadOffice;
    private OfficeServiceProvider centralOffDefaultAttyOfficeServiceProvider;
    private Office centralOffDefaultAttyOffice;
    private ServiceProvider centralOffDefaultAttyServiceProvider;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of IncomeSourceContact.
    /// </summary>
    [JsonPropertyName("incomeSourceContact")]
    public IncomeSourceContact IncomeSourceContact
    {
      get => incomeSourceContact ??= new();
      set => incomeSourceContact = value;
    }

    private IncomeSource incomeSource;
    private DateWorkArea dateWorkArea;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private CsePerson csePerson;
    private IncomeSourceContact incomeSourceContact;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private Employer employer;
  }
#endregion
}
