// Program: FN_CAB_UPDATE_DEBT_DETAIL, ID: 372266806, model: 746.
// Short name: SWE02348
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_UPDATE_DEBT_DETAIL.
/// </summary>
[Serializable]
public partial class FnCabUpdateDebtDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_UPDATE_DEBT_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabUpdateDebtDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabUpdateDebtDetail.
  /// </summary>
  public FnCabUpdateDebtDetail(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    try
    {
      UpdateDebtDetail();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0213_DEBT_DETAIL_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0217_DEBT_DETAIL_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UpdateDebtDetail()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var dueDt = import.DebtDetail.DueDt;
    var balanceDueAmt = import.DebtDetail.BalanceDueAmt;
    var interestBalanceDueAmt =
      import.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
    var adcDt = import.DebtDetail.AdcDt;
    var retiredDt = import.DebtDetail.RetiredDt;
    var coveredPrdStartDt = import.DebtDetail.CoveredPrdStartDt;
    var coveredPrdEndDt = import.DebtDetail.CoveredPrdEndDt;
    var preconversionProgramCode =
      import.DebtDetail.PreconversionProgramCode ?? "";
    var lastUpdatedTmst = import.DebtDetail.LastUpdatedTmst;
    var lastUpdatedBy = import.DebtDetail.LastUpdatedBy ?? "";

    import.Persistent.Populated = false;
    Update("UpdateDebtDetail",
      (db, command) =>
      {
        db.SetDate(command, "dueDt", dueDt);
        db.SetDecimal(command, "balDueAmt", balanceDueAmt);
        db.SetNullableDecimal(command, "intBalDueAmt", interestBalanceDueAmt);
        db.SetNullableDate(command, "adcDt", adcDt);
        db.SetNullableDate(command, "retiredDt", retiredDt);
        db.SetNullableDate(command, "cvrdPrdStartDt", coveredPrdStartDt);
        db.SetNullableDate(command, "cvdPrdEndDt", coveredPrdEndDt);
        db.
          SetNullableString(command, "precnvrsnPgmCd", preconversionProgramCode);
          
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetInt32(command, "obgGeneratedId", import.Persistent.ObgGeneratedId);
          
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.
          SetInt32(command, "otrGeneratedId", import.Persistent.OtrGeneratedId);
          
        db.SetInt32(command, "otyType", import.Persistent.OtyType);
        db.SetString(command, "otrType", import.Persistent.OtrType);
      });

    import.Persistent.DueDt = dueDt;
    import.Persistent.BalanceDueAmt = balanceDueAmt;
    import.Persistent.InterestBalanceDueAmt = interestBalanceDueAmt;
    import.Persistent.AdcDt = adcDt;
    import.Persistent.RetiredDt = retiredDt;
    import.Persistent.CoveredPrdStartDt = coveredPrdStartDt;
    import.Persistent.CoveredPrdEndDt = coveredPrdEndDt;
    import.Persistent.PreconversionProgramCode = preconversionProgramCode;
    import.Persistent.LastUpdatedTmst = lastUpdatedTmst;
    import.Persistent.LastUpdatedBy = lastUpdatedBy;
    import.Persistent.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public DebtDetail Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    private DebtDetail persistent;
    private DebtDetail debtDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
