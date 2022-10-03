// Program: LE_AHEA_RAISE_INFRASTRUC_EVENTS, ID: 372582898, model: 746.
// Short name: SWE01706
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_AHEA_RAISE_INFRASTRUC_EVENTS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block raises the Infrastructure events from AHEA - Admin Appeal 
/// Hearing online procedure.
/// </para>
/// </summary>
[Serializable]
public partial class LeAheaRaiseInfrastrucEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_AHEA_RAISE_INFRASTRUC_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAheaRaiseInfrastrucEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAheaRaiseInfrastrucEvents.
  /// </summary>
  public LeAheaRaiseInfrastrucEvents(IContext context, Import import,
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
    // 12/15/97  R Grey	SP remove USE Next_Sit_No ADD Ext States
    // *********************************************
    if (!Lt(local.Initialised.ConductedDate, import.Hearing.ConductedDate))
    {
      // --- Hearing is not set. So no action.
      return;
    }

    local.Infrastructure.BusinessObjectCd = "ADA";
    local.Infrastructure.EventId = 3;
    local.Infrastructure.InitiatingStateCode = "KS";

    // ***********************************************************
    // There is no attribute in admin appeal that indicates whether the admin 
    // appeal hearing was requested by the other state. There should be some way
    // by which we can identify what type of INTERSTATE REQUEST record exists.
    // Until it is resolved properly, leave the Initiating State as "KS". The
    // main usage of this indicator is for using different time frames where
    // possible.
    // ***********************************************************
    if (Lt(local.InitialisedToZeros.Date, import.Hearing.OutcomeReceivedDate))
    {
      // --- hearing has been held
      local.Infrastructure.ReasonCode = "HEARNG";
    }
    else
    {
      // --- hearing date has been set.
      local.Infrastructure.ReasonCode = "HEARNGDT";
    }

    local.Infrastructure.CsePersonNumber = import.Appellant.Number;
    local.Infrastructure.DenormNumeric12 =
      import.AdministrativeAppeal.Identifier;
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.UserId = "AHEA";
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.CreatedTimestamp = Now();
    local.Infrastructure.LastUpdatedBy = "";
    local.Infrastructure.LastUpdatedTimestamp =
      local.InitialisedToZeros.Timestamp;
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.ReferenceDate = import.Hearing.ConductedDate;
    local.Infrastructure.SituationNumber = 0;
    UseSpCabCreateInfrastructure();
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of Appellant.
    /// </summary>
    [JsonPropertyName("appellant")]
    public CsePersonsWorkSet Appellant
    {
      get => appellant ??= new();
      set => appellant = value;
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    private AdministrativeAppeal administrativeAppeal;
    private CsePersonsWorkSet appellant;
    private Hearing hearing;
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public Hearing Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
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

    private DateWorkArea initialisedToZeros;
    private Hearing initialised;
    private Infrastructure infrastructure;
  }
#endregion
}
