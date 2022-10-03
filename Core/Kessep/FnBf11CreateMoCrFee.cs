// Program: FN_BF11_CREATE_MO_CR_FEE, ID: 371040249, model: 746.
// Short name: SWE02721
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF11_CREATE_MO_CR_FEE.
/// </summary>
[Serializable]
public partial class FnBf11CreateMoCrFee: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF11_CREATE_MO_CR_FEE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf11CreateMoCrFee(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf11CreateMoCrFee.
  /// </summary>
  public FnBf11CreateMoCrFee(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // 2000-11-27  PR 108247  Fangman - New AB to create Monthly CR Fee rows for
    // Monthly CR Fee tbl Fix run.
    // ***************************************************
    if (ReadCsePersonAccount())
    {
      try
      {
        CreateMonthlyCourtOrderFee();

        if (AsChar(import.TestDisplayInd.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "    Created Mo tbl " + entities
            .Obligee.Number + "  " + entities
            .MonthlyCourtOrderFee.CourtOrderNumber + "  " + NumberToString
            (entities.MonthlyCourtOrderFee.YearMonth, 10, 6) + "  " + NumberToString
            ((long)(entities.MonthlyCourtOrderFee.Amount * 100), 10, 6);
          UseCabErrorReport();
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_MTHLY_COURT_ORDR_FEE_AERB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_MTHLY_COURT_ORDR_FEE_PVRB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGEE_CSE_PERSON_NF";
    }
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void CreateMonthlyCourtOrderFee()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var cpaType = entities.CsePersonAccount.Type1;
    var cspNumber = entities.CsePersonAccount.CspNumber;
    var courtOrderNumber = import.MonthlyCourtOrderFee.CourtOrderNumber;
    var yearMonth = import.MonthlyCourtOrderFee.YearMonth;
    var amount = import.MonthlyCourtOrderFee.Amount;
    var createdBy = "SWEFB651";
    var createdTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    CheckValid<MonthlyCourtOrderFee>("CpaType", cpaType);
    entities.MonthlyCourtOrderFee.Populated = false;
    Update("CreateMonthlyCourtOrderFee",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "courtOrderNumber", courtOrderNumber);
        db.SetInt32(command, "yearMonth", yearMonth);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.MonthlyCourtOrderFee.CpaType = cpaType;
    entities.MonthlyCourtOrderFee.CspNumber = cspNumber;
    entities.MonthlyCourtOrderFee.CourtOrderNumber = courtOrderNumber;
    entities.MonthlyCourtOrderFee.YearMonth = yearMonth;
    entities.MonthlyCourtOrderFee.Amount = amount;
    entities.MonthlyCourtOrderFee.CreatedBy = createdBy;
    entities.MonthlyCourtOrderFee.CreatedTimestamp = createdTimestamp;
    entities.MonthlyCourtOrderFee.LastUpdatedBy = lastUpdatedBy;
    entities.MonthlyCourtOrderFee.LastUpdatedTmst = createdTimestamp;
    entities.MonthlyCourtOrderFee.Populated = true;
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of MonthlyCourtOrderFee.
    /// </summary>
    [JsonPropertyName("monthlyCourtOrderFee")]
    public MonthlyCourtOrderFee MonthlyCourtOrderFee
    {
      get => monthlyCourtOrderFee ??= new();
      set => monthlyCourtOrderFee = value;
    }

    /// <summary>
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    private CsePerson obligee;
    private MonthlyCourtOrderFee monthlyCourtOrderFee;
    private Common testDisplayInd;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of MonthlyCourtOrderFee.
    /// </summary>
    [JsonPropertyName("monthlyCourtOrderFee")]
    public MonthlyCourtOrderFee MonthlyCourtOrderFee
    {
      get => monthlyCourtOrderFee ??= new();
      set => monthlyCourtOrderFee = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private MonthlyCourtOrderFee monthlyCourtOrderFee;
    private CsePerson obligee;
  }
#endregion
}
