// Program: LE_CAB_GET_OSP_FOR_SDSO, ID: 372661479, model: 746.
// Short name: SWE00744
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CAB_GET_OSP_FOR_SDSO.
/// </para>
/// <para>
/// This cab will determine the office number, service provider id number, and 
/// how whether the case will be referenced to central office.
/// It replaces le_cab_get_osp_for_sdso which was reading a mulitude of uneeded 
/// entity types.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabGetOspForSdso: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_GET_OSP_FOR_SDSO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabGetOspForSdso(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabGetOspForSdso.
  /// </summary>
  public LeCabGetOspForSdso(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------
    // CHANGE LOG:
    // 04/14/99				PMcElderry
    // Original coding.
    // 03/12/02		PR137625	ESHIRK
    // Fixed read against case assignment to qualify on greater than or equal 
    // discontinued date.   Service providers are active through their
    // discontinued dates.
    // 03/22/2002		PR138512	ESHIRK
    // Altered process to no longer set abnormal exit states on a not found of 
    // case, case assignment, and office service provdier. This will allow the
    // calling routine to manage the exit states.
    // -----------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Case1.Number = "";
    export.Office.SystemGeneratedId = 0;
    export.ServiceProvider.SystemGeneratedId = 0;
    export.CentralDebtOffice.Flag = "";

    do
    {
      if (ReadCase())
      {
        local.Case1.Number = entities.Case1.Number;
        ++local.CaseCount.Count;

        if (local.CaseCount.Count > 1)
        {
          break;
        }

        if (ReadCaseAssignment())
        {
          if (ReadServiceProviderOffice())
          {
            export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
            export.ServiceProvider.SystemGeneratedId =
              entities.ServiceProvider.SystemGeneratedId;
          }
        }
      }
      else
      {
        local.Common.Command = "ENDLOOP";
      }
    }
    while(!Equal(local.Common.Command, "ENDLOOP"));

    // -------------------------------------------------------------
    // If the CSE person is on more than one case, it is referred to
    // office 0022 - central debt - regardless if there are any
    // service providers assigned.
    // -------------------------------------------------------------
    if (local.CaseCount.Count > 1)
    {
      export.CentralDebtOffice.Flag = "Y";
    }
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "numb", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(command, "spdId", entities.CaseAssignment.SpdId);
        db.SetInt32(command, "officeId", entities.CaseAssignment.OffId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CentralDebtOffice.
    /// </summary>
    [JsonPropertyName("centralDebtOffice")]
    public Common CentralDebtOffice
    {
      get => centralDebtOffice ??= new();
      set => centralDebtOffice = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private Common centralDebtOffice;
    private Office office;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of CaseCount.
    /// </summary>
    [JsonPropertyName("caseCount")]
    public Common CaseCount
    {
      get => caseCount ??= new();
      set => caseCount = value;
    }

    private Common common;
    private Case1 case1;
    private Common caseCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of StateDebtSetoff.
    /// </summary>
    [JsonPropertyName("stateDebtSetoff")]
    public AdministrativeActCertification StateDebtSetoff
    {
      get => stateDebtSetoff ??= new();
      set => stateDebtSetoff = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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

    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private CaseAssignment caseAssignment;
    private InterstateRequest interstateRequest;
    private AdministrativeActCertification stateDebtSetoff;
    private ObligationTransaction debt;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}
