// Program: FN_REIP_MIGRATE_PMT_HIST_REQUEST, ID: 372418910, model: 746.
// Short name: SWE02433
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REIP_MIGRATE_PMT_HIST_REQUEST.
/// </summary>
[Serializable]
public partial class FnReipMigratePmtHistRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REIP_MIGRATE_PMT_HIST_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReipMigratePmtHistRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReipMigratePmtHistRequest.
  /// </summary>
  public FnReipMigratePmtHistRequest(IContext context, Import import,
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
    // ---------------------------------------------------------------------
    //                              Change Log
    // ---------------------------------------------------------------------
    // Date		Developer	Description
    // ---------------------------------------------------------------------
    // 06/08/99	J. Katz		Analyzed READ statements and changed
    // 				read property to Select Only where
    // 				appropriate.
    // ---------------------------------------------------------------------
    // ----------------------------------------------------------------
    // Set up local views.
    // ----------------------------------------------------------------
    local.Current.Date = Now().Date;

    // ----------------------------------------------------------------
    // Determine if payment history for the specified CSE Person
    // and Court Order has already been requested or migrated.
    // ----------------------------------------------------------------
    if (ReadInterfacePaymentHistoryReques())
    {
      if (!Lt(entities.InterfacePaymentHistoryReques.DateProcessed,
        entities.InterfacePaymentHistoryReques.DateRequested))
      {
        // ----------------------------------------------------------------
        // Payment history request was processed.
        // ----------------------------------------------------------------
        switch(AsChar(entities.InterfacePaymentHistoryReques.
          SuccessfullyConverted))
        {
          case 'Y':
            // ----------------------------------------------------------------
            // Payment history data has already been migrated.
            // ----------------------------------------------------------------
            ExitState = "FN0000_PMT_HIST_DATA_ALREADY_MIG";

            break;
          case 'N':
            // ----------------------------------------------------------------
            // Payment history data migration request was not successfully
            // processed.  OK to try again.
            // ----------------------------------------------------------------
            try
            {
              UpdateInterfacePaymentHistoryReques();
              ExitState = "FN0000_INTF_PMT_HIST_REQ_SUCCESS";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_INTF_PMT_HIST_REQ_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_INTF_PMT_HIST_REQ_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            break;
          default:
            // ----------------------------------------------------------------
            // Processing error.  The Successfully converted flag should
            // have be set to Y or N after the request was processed.
            // ----------------------------------------------------------------
            ExitState = "FN0000_DATA_CONVERSION_PGM_ERR";

            break;
        }
      }
      else if (Equal(entities.InterfacePaymentHistoryReques.DateProcessed,
        local.Null1.Date))
      {
        // ----------------------------------------------------------------
        // Payment history data migration from KAECSES has already
        // been requested.
        // ----------------------------------------------------------------
        ExitState = "FN0000_PMT_HIST_MIG_ALREADY_REQ";
      }
    }
    else
    {
      // ------------------------------------------------------------------
      // Payment history data migration for specified CSE Person and Court
      // Order was not previously requested.  OK to create new request.
      // ------------------------------------------------------------------
      try
      {
        CreateInterfacePaymentHistoryReques();

        // ------------------------------------------------------------------
        // Payment history data migration request successfully recorded.
        // ------------------------------------------------------------------
        ExitState = "FN0000_INTF_PMT_HIST_REQ_SUCCESS";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_INTF_PMT_HIST_REQ_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_INTF_PMT_HIST_REQ_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void CreateInterfacePaymentHistoryReques()
  {
    var obligorCsePersonNumber =
      import.InterfacePaymentHistoryReques.ObligorCsePersonNumber;
    var kaecsesCourtOrderNumber =
      import.InterfacePaymentHistoryReques.KaecsesCourtOrderNumber;
    var requestingUserId = global.UserId;
    var dateRequested = local.Current.Date;

    entities.InterfacePaymentHistoryReques.Populated = false;
    Update("CreateInterfacePaymentHistoryReques",
      (db, command) =>
      {
        db.SetString(command, "obligorCseperNum", obligorCsePersonNumber);
        db.SetString(command, "kaecsesCrtordNum", kaecsesCourtOrderNumber);
        db.SetString(command, "requestingUserId", requestingUserId);
        db.SetDate(command, "dateRequested", dateRequested);
        db.SetNullableDate(command, "dateProcessed", null);
        db.SetNullableString(command, "successfullyConvt", "");
      });

    entities.InterfacePaymentHistoryReques.ObligorCsePersonNumber =
      obligorCsePersonNumber;
    entities.InterfacePaymentHistoryReques.KaecsesCourtOrderNumber =
      kaecsesCourtOrderNumber;
    entities.InterfacePaymentHistoryReques.RequestingUserId = requestingUserId;
    entities.InterfacePaymentHistoryReques.DateRequested = dateRequested;
    entities.InterfacePaymentHistoryReques.DateProcessed = null;
    entities.InterfacePaymentHistoryReques.SuccessfullyConverted = "";
    entities.InterfacePaymentHistoryReques.Populated = true;
  }

  private bool ReadInterfacePaymentHistoryReques()
  {
    entities.InterfacePaymentHistoryReques.Populated = false;

    return Read("ReadInterfacePaymentHistoryReques",
      (db, command) =>
      {
        db.SetString(
          command, "obligorCseperNum",
          import.InterfacePaymentHistoryReques.ObligorCsePersonNumber);
        db.SetString(
          command, "kaecsesCrtordNum",
          import.InterfacePaymentHistoryReques.KaecsesCourtOrderNumber);
      },
      (db, reader) =>
      {
        entities.InterfacePaymentHistoryReques.ObligorCsePersonNumber =
          db.GetString(reader, 0);
        entities.InterfacePaymentHistoryReques.KaecsesCourtOrderNumber =
          db.GetString(reader, 1);
        entities.InterfacePaymentHistoryReques.RequestingUserId =
          db.GetString(reader, 2);
        entities.InterfacePaymentHistoryReques.DateRequested =
          db.GetDate(reader, 3);
        entities.InterfacePaymentHistoryReques.DateProcessed =
          db.GetNullableDate(reader, 4);
        entities.InterfacePaymentHistoryReques.SuccessfullyConverted =
          db.GetNullableString(reader, 5);
        entities.InterfacePaymentHistoryReques.Populated = true;
      });
  }

  private void UpdateInterfacePaymentHistoryReques()
  {
    var requestingUserId = global.UserId;
    var dateRequested = local.Current.Date;
    var dateProcessed = local.Null1.Date;

    entities.InterfacePaymentHistoryReques.Populated = false;
    Update("UpdateInterfacePaymentHistoryReques",
      (db, command) =>
      {
        db.SetString(command, "requestingUserId", requestingUserId);
        db.SetDate(command, "dateRequested", dateRequested);
        db.SetNullableDate(command, "dateProcessed", dateProcessed);
        db.SetNullableString(command, "successfullyConvt", "");
        db.SetString(
          command, "obligorCseperNum",
          entities.InterfacePaymentHistoryReques.ObligorCsePersonNumber);
        db.SetString(
          command, "kaecsesCrtordNum",
          entities.InterfacePaymentHistoryReques.KaecsesCourtOrderNumber);
      });

    entities.InterfacePaymentHistoryReques.RequestingUserId = requestingUserId;
    entities.InterfacePaymentHistoryReques.DateRequested = dateRequested;
    entities.InterfacePaymentHistoryReques.DateProcessed = dateProcessed;
    entities.InterfacePaymentHistoryReques.SuccessfullyConverted = "";
    entities.InterfacePaymentHistoryReques.Populated = true;
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
    /// A value of InterfacePaymentHistoryReques.
    /// </summary>
    [JsonPropertyName("interfacePaymentHistoryReques")]
    public InterfacePaymentHistoryReques InterfacePaymentHistoryReques
    {
      get => interfacePaymentHistoryReques ??= new();
      set => interfacePaymentHistoryReques = value;
    }

    private InterfacePaymentHistoryReques interfacePaymentHistoryReques;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea current;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterfacePaymentHistoryReques.
    /// </summary>
    [JsonPropertyName("interfacePaymentHistoryReques")]
    public InterfacePaymentHistoryReques InterfacePaymentHistoryReques
    {
      get => interfacePaymentHistoryReques ??= new();
      set => interfacePaymentHistoryReques = value;
    }

    private InterfacePaymentHistoryReques interfacePaymentHistoryReques;
  }
#endregion
}
