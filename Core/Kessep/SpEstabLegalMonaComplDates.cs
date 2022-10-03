// Program: SP_ESTAB_LEGAL_MONA_COMPL_DATES, ID: 372858502, model: 746.
// Short name: SWE00013
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_ESTAB_LEGAL_MONA_COMPL_DATES.
/// </para>
/// <para>
/// Use the Refernce Date from Infrastructure to establish the 4 compliance 
/// dates of the Monitored activity for Legal Bus Obj
/// </para>
/// </summary>
[Serializable]
public partial class SpEstabLegalMonaComplDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ESTAB_LEGAL_MONA_COMPL_DATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEstabLegalMonaComplDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEstabLegalMonaComplDates.
  /// </summary>
  public SpEstabLegalMonaComplDates(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Changes:
    // 08/22/2000	PMcElderry
    // PR # 100583
    // Event 41, Details 340, 342, 344, 345, 630, 640, and 642
    // need to have the monitored activities referenced by the
    // service date; all LEA object codes need to have the start
    // date referenced by the infrastructure reference date
    // (performed by calling AB)
    // ------------------------------------------------------------
    MoveMonitoredActivity(import.MonitoredActivity, export.MonitoredActivity);
    local.EventDetail.SystemGeneratedIdentifier =
      import.EventDetail.SystemGeneratedIdentifier;

    // ---------------
    // Beg PR # 100583
    // ---------------
    if (import.Infrastructure.EventId == 41)
    {
      if (local.EventDetail.SystemGeneratedIdentifier == 340 || local
        .EventDetail.SystemGeneratedIdentifier == 342 || local
        .EventDetail.SystemGeneratedIdentifier == 344 || local
        .EventDetail.SystemGeneratedIdentifier == 345 || local
        .EventDetail.SystemGeneratedIdentifier == 630 || local
        .EventDetail.SystemGeneratedIdentifier == 640 || local
        .EventDetail.SystemGeneratedIdentifier == 642)
      {
        if (ReadServiceProcess())
        {
          if (Equal(entities.ServiceProcess.ServiceDate, null))
          {
            export.MonitoredActivity.OtherNonComplianceDate = null;
          }
          else if (local.EventDetail.SystemGeneratedIdentifier == 340 || local
            .EventDetail.SystemGeneratedIdentifier == 342)
          {
            // ---------------------------------------------------------------
            // For 340, differentiate b/t activities 35 (135 days) and 41 (21
            // days)
            // For 342, differentiate b/t activities 38 (21 days) and 40 (365
            // days)
            // ---------------------------------------------------------------
            if (import.Activity.ControlNumber == 38 || import
              .Activity.ControlNumber == 41)
            {
              export.MonitoredActivity.OtherNonComplianceDate =
                AddDays(entities.ServiceProcess.ServiceDate, 21);
            }
            else if (import.Activity.ControlNumber == 40)
            {
              export.MonitoredActivity.OtherNonComplianceDate =
                AddDays(entities.ServiceProcess.ServiceDate, 365);
            }
            else
            {
              // -----------
              // BLOWUP here
              // -----------
              export.MonitoredActivity.OtherNonComplianceDate =
                AddDays(entities.ServiceProcess.ServiceDate, 135);
            }
          }
          else if (import.EventDetail.SystemGeneratedIdentifier == 344 || local
            .EventDetail.SystemGeneratedIdentifier == 345)
          {
            export.MonitoredActivity.OtherNonComplianceDate =
              AddDays(entities.ServiceProcess.ServiceDate, 10);
          }
          else if (local.EventDetail.SystemGeneratedIdentifier == 630)
          {
            // -------------------------------------------------------------
            // Differentiate b/t activities 42 (21 days) and 61 (135 days)
            // -------------------------------------------------------------
            if (import.Activity.ControlNumber == 42)
            {
              export.MonitoredActivity.OtherNonComplianceDate =
                AddDays(entities.ServiceProcess.ServiceDate, 21);
            }
            else
            {
              export.MonitoredActivity.OtherNonComplianceDate =
                AddDays(entities.ServiceProcess.ServiceDate, 135);
            }
          }
          else if (local.EventDetail.SystemGeneratedIdentifier == 640)
          {
            export.MonitoredActivity.OtherNonComplianceDate =
              AddDays(entities.ServiceProcess.ServiceDate, 45);
          }
          else if (local.EventDetail.SystemGeneratedIdentifier == 642)
          {
            export.MonitoredActivity.OtherNonComplianceDate =
              AddDays(entities.ServiceProcess.ServiceDate, 15);
          }
          else
          {
          }
        }
      }
      else
      {
        // ------------------------------
        // No special processing required
        // ------------------------------
      }
    }
    else
    {
      // ---------------
      // End PR # 100583
      // ---------------
    }

    if (!Equal(import.Infrastructure.ReferenceDate, local.Initialzed.Date))
    {
      switch(import.Infrastructure.EventId)
      {
        case 2:
          if (Equal(import.Infrastructure.ReasonCode, "FRENEWALA"))
          {
          }
          else
          {
            return;
          }

          break;
        case 29:
          switch(TrimEnd(import.Infrastructure.ReasonCode))
          {
            case "HEARAIDOEXMO":
              break;
            case "HEARCOMPENIM":
              break;
            case "HEARCOMTEMPT":
              break;
            case "HEARCONSOLDM":
              break;
            case "HEARCSMODM":
              break;
            case "HEARDEFJPATM":
              break;
            case "HEARDELTSUPM":
              break;
            case "HEARDISMISSM":
              break;
            case "HEARGALAMFM":
              break;
            case "HEARGARDADLM":
              break;
            case "HEARGENETICM":
              break;
            case "HEARJDAGEMPM":
              break;
            case "HEARJUDMEXPM":
              break;
            case "HEARMEDESTBM":
              break;
            case "HEARMODMSOM":
              break;
            case "HEARMOTIONKS":
              break;
            case "HEARMOTOSTAY":
              break;
            case "HEARREVIVORM":
              break;
            case "HEARSETARRSM":
              break;
            case "HEAR718BDEFM":
              break;
            default:
              return;
          }

          break;
        case 30:
          switch(TrimEnd(import.Infrastructure.ReasonCode))
          {
            case "FDEFJPATJ":
              break;
            case "FDFLTSUPJ":
              break;
            case "FEMPIWOJ":
              break;
            case "FEMPMWOJ":
              break;
            case "FJEFJ":
              break;
            case "FMEDEXPJ":
              break;
            case "FMODSUPPO":
              break;
            case "FPATERNJ":
              break;
            case "FSETARRSJ":
              break;
            case "FSUPPORTJ":
              break;
            case "FVOLPATTJ":
              break;
            case "FVOLSUPTJ":
              break;
            case "FVOL718BJ":
              break;
            case "F718BDEFJ":
              break;
            case "F718BJERJ":
              break;
            default:
              return;
          }

          break;
        case 31:
          if (Equal(import.Infrastructure.ReasonCode, "FGENWRITM"))
          {
          }
          else
          {
            return;
          }

          break;
        case 32:
          switch(TrimEnd(import.Infrastructure.ReasonCode))
          {
            case "FDISMISSO":
              break;
            case "FREVIVORJ":
              break;
            default:
              return;
          }

          break;
        case 38:
          switch(TrimEnd(import.Infrastructure.ReasonCode))
          {
            case "FIWOISTN":
              break;
            case "FNOIIWON":
              break;
            default:
              return;
          }

          break;
        case 41:
          switch(TrimEnd(import.Infrastructure.ReasonCode))
          {
            case "SC_DET1PATP":
              break;
            case "SC_DET2PATP":
              break;
            case "SC_RIMB718P":
              break;
            case "SC_SUPPORTP":
              break;
            case "SCCSONLYP":
              break;
            case "SCPATCSONP":
              break;
            default:
              return;
          }

          break;
        case 96:
          switch(TrimEnd(import.Infrastructure.ReasonCode))
          {
            case "FGARNRQNW":
              break;
            case "FGARNRQW":
              break;
            default:
              return;
          }

          break;
        case 99:
          switch(TrimEnd(import.Infrastructure.ReasonCode))
          {
            case "FGENETICO":
              break;
            case "FGENTEST":
              break;
            default:
              return;
          }

          break;
        default:
          return;
      }

      if (import.ActivityDetail.FedNearNonComplDays.GetValueOrDefault() != 0)
      {
        export.MonitoredActivity.FedNearNonComplDate =
          AddDays(import.Infrastructure.ReferenceDate,
          import.ActivityDetail.FedNearNonComplDays.GetValueOrDefault());
      }

      if (import.ActivityDetail.FedNonComplianceDays.GetValueOrDefault() != 0)
      {
        export.MonitoredActivity.FedNonComplianceDate =
          AddDays(import.Infrastructure.ReferenceDate,
          import.ActivityDetail.FedNonComplianceDays.GetValueOrDefault());
      }

      if (import.ActivityDetail.OtherNearNonComplDays.GetValueOrDefault() != 0)
      {
        export.MonitoredActivity.OtherNearNonComplDate =
          AddDays(import.Infrastructure.ReferenceDate,
          import.ActivityDetail.OtherNearNonComplDays.GetValueOrDefault());
      }

      // ---------------
      // Beg PR # 100583
      // ---------------
      if (entities.ServiceProcess.Identifier > 0)
      {
      }
      else
      {
        if (import.ActivityDetail.OtherNonComplianceDays.GetValueOrDefault() !=
          0)
        {
          export.MonitoredActivity.OtherNonComplianceDate =
            AddDays(import.Infrastructure.ReferenceDate,
            import.ActivityDetail.OtherNonComplianceDays.GetValueOrDefault());
        }

        // ---------------
        // End PR # 100583
        // ---------------
      }
    }
  }

  private static void MoveMonitoredActivity(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.FedNonComplianceDate = source.FedNonComplianceDate;
    target.FedNearNonComplDate = source.FedNearNonComplDate;
    target.OtherNonComplianceDate = source.OtherNonComplianceDate;
    target.OtherNearNonComplDate = source.OtherNearNonComplDate;
  }

  private bool ReadServiceProcess()
  {
    entities.ServiceProcess.Populated = false;

    return Read("ReadServiceProcess",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          import.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          import.Infrastructure.DenormTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ServiceProcess.ServiceRequestDate = db.GetDate(reader, 1);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 2);
        entities.ServiceProcess.CreatedTstamp = db.GetDateTime(reader, 3);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 4);
        entities.ServiceProcess.Populated = true;
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
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
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
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    private Activity activity;
    private EventDetail eventDetail;
    private ActivityDetail activityDetail;
    private Infrastructure infrastructure;
    private MonitoredActivity monitoredActivity;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    private MonitoredActivity monitoredActivity;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Initialzed.
    /// </summary>
    [JsonPropertyName("initialzed")]
    public DateWorkArea Initialzed
    {
      get => initialzed ??= new();
      set => initialzed = value;
    }

    private EventDetail eventDetail;
    private DateWorkArea initialzed;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
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

    private ServiceProcess serviceProcess;
    private LegalAction legalAction;
  }
#endregion
}
