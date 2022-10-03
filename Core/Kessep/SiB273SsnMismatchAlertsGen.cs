// Program: SI_B273_SSN_MISMATCH_ALERTS_GEN, ID: 371415480, model: 746.
// Short name: SWE00024
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_B273_SSN_MISMATCH_ALERTS_GEN.
/// </para>
/// <para>
/// This action block generates the worker alerts where the SSN sent my NDNH is 
/// not match with CSE SSNs (Primary &amp; alertnate SSNs) for the selected
/// person.   Since the worker requires complete employer information (i.e.
/// Employer Name, SSN sent by NDNH and Complete employer address) and current
/// alert detail can't accomodate those details, this process will generate a
/// Narrative Details for the worker to look up the information on CSLN screen
/// and the same will be linked to the alert infrastructure record.  This way
/// the worker gets complete employer information to feed into CSE database.
/// </para>
/// </summary>
[Serializable]
public partial class SiB273SsnMismatchAlertsGen: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_SSN_MISMATCH_ALERTS_GEN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273SsnMismatchAlertsGen(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273SsnMismatchAlertsGen.
  /// </summary>
  public SiB273SsnMismatchAlertsGen(IContext context, Import import,
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
    // **************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // **************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  
    // ---------
    // ----------------------------------------*
    // * 02/02/2009  Raj S              CQ114       **** Initial Coding  ****
    // *
    // *
    // 
    // As part of FCR tracking service Request *
    // *
    // 
    // this AB will generate alerts for NDNH   *
    // *
    // 
    // record which are not matching with CSE  *
    // *
    // 
    // SSNs.                                   *
    // *
    // 
    // *
    // * 06/29/2009  Raj S              CQ11752     Modified to fix generate 
    // Alerts for     *
    // *
    // 
    // persons with 'AP', 'AR' & 'CH' roles.   *
    // *
    // 
    // This will fix duplicate generation of   *
    // *
    // 
    // infrastructure record the selected      *
    // *
    // 
    // person.                                 *
    // **************************************************************************************
    // **************************************************************************************
    // This action block will be used by all there(New Hire, Quarterly Wages, 
    // Unemployment
    // Insurance Benefit) NDNH processes.  This AB will be used whenever NDNH 
    // response SSN is
    // Not matching with CSE SSNs(Primary & Alternate SSNs) for the selected 
    // person then this
    // Action block will be used by respective NDNH processes.
    // This action block will generate History and Alert records and for 
    // employee and employee
    // Address details will be created in Narrative detail entity type, so that 
    // the worker
    // Will get complete information about the employer sent by NDNH.
    // **************************************************************************************
    local.FederalCaseRegistry.Assign(import.FederalCaseRegistry);
    local.Max.Date = import.Max.Date;
    local.Employer.Assign(import.Employer);
    local.EmployerAddress.Assign(import.EmployerAddress);
    local.Infrastructure.Assign(import.Infrastructure);

    // **************************************************************************************
    // Read all case & case role for the selected person and for each case 
    // selected establish
    // infrastructure record(Alert & History) record.  While selecting the case 
    // role record
    // only active records needs to be selected.
    // **************************************************************************************
    // **************************************************************************************
    // CQ11752(PR): 'MO' Role fix
    // 
    // START
    // Reach each statement modfied to select only 'AP', 'AR' & 'CH' roles while
    // slecting the
    // CSE persons.
    // **************************************************************************************
    foreach(var item in ReadCaseRoleCase())
    {
      // **************************************************************************************
      // CQ11752(PR): 'MO' Role fix
      // 
      // END
      // **************************************************************************************
      ExitState = "ACO_NN0000_ALL_OK";
      local.Infrastructure.CaseNumber = entities.Case1.Number;
      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // **************************************************************************************
      // Get the user id from Service Provider entity type for the selected via 
      // case assignment
      // and Officer Service Provider Entity Types.   This user id required to 
      // update in
      // Narrative detail entity type to display on CSLN screen.
      // **************************************************************************************
      local.NarrativeDetail.CreatedBy = global.UserId;

      if (ReadCaseAssignmentServiceProvider())
      {
        local.NarrativeDetail.CreatedBy = entities.ServiceProvider.UserId;
      }

      // **************************************************************************************
      // For each Infrastructure generated by the process, Narrative Detail 
      // information needs
      // to be created with multiple line containing below mentioned information
      // :
      // Line1:  CSE Person# & FCR SSN# received through NDNH file
      // Line2:  Employer Name
      // Line3:  Employer Address Street1 & Street2
      // Line4:  Employer Address Street3 & Street4 (If available)
      // Line5:  Employer Address City, State, Zip Code, Zip4 & Zip3 (If 
      // available)
      // Line6:  Postal Code, Province, County (If available)
      // Line7:  Employer source text will be passed by calling objects(
      // SWEIB273, B274 or B248)
      // **************************************************************************************
      local.NarrativeDetail.CaseNumber = entities.Case1.Number;
      local.NarrativeDetail.CreatedTimestamp =
        local.Infrastructure.CreatedTimestamp;
      local.NarrativeDetail.InfrastructureId =
        local.Infrastructure.SystemGeneratedIdentifier;
      local.NarrativeDetailLineCnt.Count = 0;
      local.NarrativeDetail.LineNumber = 0;

      do
      {
        ++local.NarrativeDetailLineCnt.Count;

        if (local.NarrativeDetailLineCnt.Count > 8)
        {
          break;
        }

        local.NarrativeDetail.NarrativeText = "";

        switch(local.NarrativeDetailLineCnt.Count)
        {
          case 1:
            ++local.NarrativeDetail.LineNumber;
            local.NarrativeDetail.NarrativeText = "CSE PERSON#:" + import
              .FederalCaseRegistry.Number + "       FCR SSN#:" + local
              .FederalCaseRegistry.Ssn;

            break;
          case 2:
            ++local.NarrativeDetail.LineNumber;
            local.NarrativeDetail.NarrativeText = "EMPLOYER: " + TrimEnd
              (local.Employer.Name);

            break;
          case 3:
            ++local.NarrativeDetail.LineNumber;
            local.NarrativeDetail.NarrativeText = "ADDRESS:  " + TrimEnd
              (local.EmployerAddress.Street1);

            if (!IsEmpty(local.EmployerAddress.Street2))
            {
              local.NarrativeDetail.NarrativeText =
                TrimEnd(local.NarrativeDetail.NarrativeText) + ", " + TrimEnd
                (local.EmployerAddress.Street2);
            }

            break;
          case 4:
            if (IsEmpty(local.EmployerAddress.Street3) && IsEmpty
              (local.EmployerAddress.Street4))
            {
              continue;
            }

            ++local.NarrativeDetail.LineNumber;
            local.NarrativeDetail.NarrativeText = "          " + TrimEnd
              (local.EmployerAddress.Street3);

            if (!IsEmpty(local.EmployerAddress.Street4))
            {
              local.NarrativeDetail.NarrativeText =
                TrimEnd(local.NarrativeDetail.NarrativeText) + ", " + TrimEnd
                (local.EmployerAddress.Street4);
            }

            break;
          case 5:
            if (IsEmpty(local.EmployerAddress.City) && IsEmpty
              (local.EmployerAddress.ZipCode) && IsEmpty
              (local.EmployerAddress.Zip4) && IsEmpty
              (local.EmployerAddress.Zip3))
            {
              continue;
            }

            ++local.NarrativeDetail.LineNumber;
            local.NarrativeDetail.NarrativeText = "          " + TrimEnd
              (local.EmployerAddress.City);

            if (!IsEmpty(local.EmployerAddress.State))
            {
              local.NarrativeDetail.NarrativeText =
                TrimEnd(local.NarrativeDetail.NarrativeText) + ", " + (
                  local.EmployerAddress.State ?? "");
              local.NarrativeDetail.NarrativeText =
                TrimEnd(local.NarrativeDetail.NarrativeText) + "-" + (
                  local.EmployerAddress.ZipCode ?? "") + "" + "";
            }

            if (!IsEmpty(local.EmployerAddress.Zip4) || !
              IsEmpty(local.EmployerAddress.Zip3))
            {
              local.NarrativeDetail.NarrativeText =
                TrimEnd(local.NarrativeDetail.NarrativeText) + ", " + (
                  local.EmployerAddress.Zip4 ?? "") + "" + ", ";
              local.NarrativeDetail.NarrativeText =
                (local.NarrativeDetail.NarrativeText ?? "") + TrimEnd
                (local.EmployerAddress.Zip3);
            }

            break;
          case 6:
            if (IsEmpty(local.EmployerAddress.Province) && IsEmpty
              (local.EmployerAddress.PostalCode) && IsEmpty
              (local.EmployerAddress.Country))
            {
              continue;
            }

            ++local.NarrativeDetail.LineNumber;
            local.NarrativeDetail.NarrativeText = "          " + TrimEnd
              (local.EmployerAddress.PostalCode);

            if (!IsEmpty(local.EmployerAddress.Province) || !
              IsEmpty(local.EmployerAddress.Country))
            {
              local.NarrativeDetail.NarrativeText =
                TrimEnd(local.NarrativeDetail.NarrativeText) + ", " + (
                  local.EmployerAddress.Province ?? "") + ", " + (
                  local.EmployerAddress.Country ?? "");
            }

            break;
          case 7:
            if (Equal(global.UserId, "SWEIB273"))
            {
              continue;
            }

            ++local.NarrativeDetail.LineNumber;
            local.NarrativeDetail.NarrativeText = "Wage Amount: $" + TrimEnd
              (NumberToString((long)import.WageAmount.AverageCurrency, 5, 11) +
              "." + NumberToString(0, 14, 2));

            break;
          case 8:
            ++local.NarrativeDetail.LineNumber;
            local.NarrativeDetail.NarrativeText =
              import.EmployerSourceTxt.NarrativeText ?? "";

            break;
          default:
            goto AfterCycle;
        }

        // **************************************************************************************
        // The following Common Action Block(CAB) will be used to store the 
        // narrative detail
        // information.
        // **************************************************************************************
        UseSpCabCreateNarrativeDetail();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
      while(local.NarrativeDetailLineCnt.Count <= 8);

AfterCycle:
      ;
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabCreateNarrativeDetail()
  {
    var useImport = new SpCabCreateNarrativeDetail.Import();
    var useExport = new SpCabCreateNarrativeDetail.Export();

    useImport.NarrativeDetail.Assign(local.NarrativeDetail);

    Call(SpCabCreateNarrativeDetail.Execute, useImport, useExport);
  }

  private bool ReadCaseAssignmentServiceProvider()
  {
    entities.CaseAssignment.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentServiceProvider",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(command, "effectiveDate", date);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
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
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 8);
        entities.ServiceProvider.UserId = db.GetString(reader, 9);
        entities.CaseAssignment.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", import.FederalCaseRegistry.Number);
        db.SetNullableDate(command, "startDate", date);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

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
    /// A value of FederalCaseRegistry.
    /// </summary>
    [JsonPropertyName("federalCaseRegistry")]
    public CsePersonsWorkSet FederalCaseRegistry
    {
      get => federalCaseRegistry ??= new();
      set => federalCaseRegistry = value;
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

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
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

    /// <summary>
    /// A value of EmployerSourceTxt.
    /// </summary>
    [JsonPropertyName("employerSourceTxt")]
    public NarrativeDetail EmployerSourceTxt
    {
      get => employerSourceTxt ??= new();
      set => employerSourceTxt = value;
    }

    /// <summary>
    /// A value of WageAmount.
    /// </summary>
    [JsonPropertyName("wageAmount")]
    public Common WageAmount
    {
      get => wageAmount ??= new();
      set => wageAmount = value;
    }

    private DateWorkArea max;
    private CsePersonsWorkSet federalCaseRegistry;
    private Employer employer;
    private EmployerAddress employerAddress;
    private Infrastructure infrastructure;
    private NarrativeDetail employerSourceTxt;
    private Common wageAmount;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of FederalCaseRegistry.
    /// </summary>
    [JsonPropertyName("federalCaseRegistry")]
    public CsePersonsWorkSet FederalCaseRegistry
    {
      get => federalCaseRegistry ??= new();
      set => federalCaseRegistry = value;
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

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
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

    /// <summary>
    /// A value of NarrativeDetailLineCnt.
    /// </summary>
    [JsonPropertyName("narrativeDetailLineCnt")]
    public Common NarrativeDetailLineCnt
    {
      get => narrativeDetailLineCnt ??= new();
      set => narrativeDetailLineCnt = value;
    }

    /// <summary>
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    private DateWorkArea max;
    private CsePersonsWorkSet federalCaseRegistry;
    private Employer employer;
    private EmployerAddress employerAddress;
    private Infrastructure infrastructure;
    private Common narrativeDetailLineCnt;
    private NarrativeDetail narrativeDetail;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private Case1 case1;
    private CaseAssignment caseAssignment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
