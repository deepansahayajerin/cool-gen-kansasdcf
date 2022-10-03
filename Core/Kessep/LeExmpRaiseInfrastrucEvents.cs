// Program: LE_EXMP_RAISE_INFRASTRUC_EVENTS, ID: 372590680, model: 746.
// Short name: SWE01707
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
/// A program: LE_EXMP_RAISE_INFRASTRUC_EVENTS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block raises the Infrastructure events from AHEA - Admin Appeal 
/// Hearing online procedure.
/// </para>
/// </summary>
[Serializable]
public partial class LeExmpRaiseInfrastrucEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EXMP_RAISE_INFRASTRUC_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeExmpRaiseInfrastrucEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeExmpRaiseInfrastrucEvents.
  /// </summary>
  public LeExmpRaiseInfrastrucEvents(IContext context, Import import,
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
    // *********************************************
    // Date	  By	  IDCR #	Description
    // 01-01-96  govind		Initial coding
    // *********************************************
    local.MaxDate.EndDate = UseCabSetMaximumDiscontinueDate();

    if (Lt(local.Initialised.EndDate,
      import.ObligationAdmActionExemption.EndDate) && !
      Equal(import.ObligationAdmActionExemption.EndDate, local.MaxDate.EndDate))
    {
      // --- Exemption has been ended. Events have not been identified for this.
      return;
    }

    // *********************************************
    // Check the value for business object cd
    // *********************************************
    local.Infrastructure.BusinessObjectCd = "OBL";
    local.Infrastructure.CsePersonNumber = import.Obligor.Number;
    local.Infrastructure.DenormNumeric12 =
      import.Obligation.SystemGeneratedIdentifier;
    local.Infrastructure.DenormText12 =
      NumberToString(import.ObligationType.SystemGeneratedIdentifier, 12);
    local.Infrastructure.DenormDate =
      import.ObligationAdmActionExemption.EffectiveDate;
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.UserId = "EXMP";
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.CreatedTimestamp = Now();
    local.Infrastructure.LastUpdatedBy = "";
    local.Infrastructure.LastUpdatedTimestamp = local.Null1.Timestamp;
    local.Infrastructure.Detail = import.AdministrativeAction.Type1 + " exempted for obligation " +
      import.ObligationType.Code + ".";
    local.Infrastructure.SituationNumber =
      import.Infrastructure.SituationNumber;

    // ********************************************************
    // Need to resolve how to identify Intiating State Code. There is no 
    // attribute in Obligation Admin Action exemption that indicates the
    // initiating state code.
    // ********************************************************
    local.Infrastructure.InitiatingStateCode = "KS";
    local.Infrastructure.EventId = 1;

    switch(TrimEnd(import.AdministrativeAction.Type1))
    {
      case "FDSO":
        local.Infrastructure.ReasonCode = "FDSOEXMP";

        break;
      case "SDSO":
        local.Infrastructure.ReasonCode = "SDSOEXMP";

        break;
      case "CRED":
        local.Infrastructure.ReasonCode = "CREDEXMP";

        break;
      case "COAG":
        local.Infrastructure.ReasonCode = "COAGEXMP";

        break;
      case "IRS":
        local.Infrastructure.ReasonCode = "IRSEXMP";

        break;
      case "KSMW":
        local.Infrastructure.ReasonCode = "KSMWEXMP";

        break;
      case "KDWP":
        local.Infrastructure.ReasonCode = "KDWPEXMP";

        break;
      case "KDMV":
        local.Infrastructure.ReasonCode = "KDMVPEXMP";

        break;
      default:
        return;
    }

    UseSpCabCreateInfrastructure();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.Infrastructure.SituationNumber =
        local.Infrastructure.SituationNumber;
    }
    else
    {
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private Infrastructure infrastructure;
    private CsePerson obligor;
    private AdministrativeAction administrativeAction;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private Obligation obligation;
    private ObligationType obligationType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of DetailOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("detailOfficeServiceProvider")]
      public OfficeServiceProvider DetailOfficeServiceProvider
      {
        get => detailOfficeServiceProvider ??= new();
        set => detailOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailOffice.
      /// </summary>
      [JsonPropertyName("detailOffice")]
      public Office DetailOffice
      {
        get => detailOffice ??= new();
        set => detailOffice = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private OfficeServiceProvider detailOfficeServiceProvider;
      private ServiceProvider detailServiceProvider;
      private Office detailOffice;
      private Case1 detailCase;
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
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public ObligationAdmActionExemption Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public ObligationAdmActionExemption MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
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

    private DateWorkArea null1;
    private ObligationAdmActionExemption initialised;
    private ObligationAdmActionExemption maxDate;
    private Array<LocalGroup> local1;
    private Infrastructure infrastructure;
  }
#endregion
}
