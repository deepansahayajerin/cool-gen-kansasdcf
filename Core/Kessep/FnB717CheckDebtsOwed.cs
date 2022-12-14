// Program: FN_B717_CHECK_DEBTS_OWED, ID: 373348020, model: 746.
// Short name: SWE02994
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_CHECK_DEBTS_OWED.
/// </summary>
[Serializable]
public partial class FnB717CheckDebtsOwed: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_CHECK_DEBTS_OWED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717CheckDebtsOwed(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717CheckDebtsOwed.
  /// </summary>
  public FnB717CheckDebtsOwed(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.HardcodedAccr.Classification = "A";

    // -------------------------------------------------------
    // ???  Is timestamp check necessary?
    // -------------------------------------------------------
    foreach(var item in ReadDebtDebtDetailObligationObligationType())
    {
      // ------------------------------------------------------------
      // CAB defaults Coll date to Current date. Don't pass anything.
      // ------------------------------------------------------------
      UseFnDeterminePgmForDebtDetail();

      switch(TrimEnd(import.DebtSearch.Code))
      {
        case "AF":
          if (Equal(local.Program.Code, "AF") || Equal
            (local.Program.Code, "FC") || Equal(local.Program.Code, "NF") || Equal
            (local.Program.Code, "NC"))
          {
            export.DebtOwed.Flag = "Y";

            return;
          }

          break;
        case "NA":
          if (Equal(local.Program.Code, "NA"))
          {
            export.DebtOwed.Flag = "Y";

            return;
          }

          break;
        case "KS":
          if (Equal(local.Program.Code, "AF") || Equal
            (local.Program.Code, "FC") || Equal(local.Program.Code, "NF") || Equal
            (local.Program.Code, "NC") || Equal(local.Program.Code, "NA"))
          {
            export.DebtOwed.Flag = "Y";

            return;
          }

          break;
        default:
          break;
      }
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.SupportedPerson.Number = import.CsePerson.Number;
    useImport.HardcodedAccruing.Classification =
      local.HardcodedAccr.Classification;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private IEnumerable<bool> ReadDebtDebtDetailObligationObligationType()
  {
    entities.DebtDetail.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;

    return ReadEach("ReadDebtDebtDetailObligationObligationType",
      (db, command) =>
      {
        db.SetNullableString(command, "cspSupNumber", import.CsePerson.Number);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "dueDt", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 8);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 15);
        entities.ObligationType.Classification = db.GetString(reader, 16);
        entities.DebtDetail.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;

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
    /// A value of DebtSearch.
    /// </summary>
    [JsonPropertyName("debtSearch")]
    public Program DebtSearch
    {
      get => debtSearch ??= new();
      set => debtSearch = value;
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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    private Program debtSearch;
    private CsePerson csePerson;
    private DateWorkArea reportEndDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DebtOwed.
    /// </summary>
    [JsonPropertyName("debtOwed")]
    public Common DebtOwed
    {
      get => debtOwed ??= new();
      set => debtOwed = value;
    }

    private Common debtOwed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HardcodedAccr.
    /// </summary>
    [JsonPropertyName("hardcodedAccr")]
    public ObligationType HardcodedAccr
    {
      get => hardcodedAccr ??= new();
      set => hardcodedAccr = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private ObligationType hardcodedAccr;
    private Program program;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    private DebtDetail debtDetail;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private ObligationTransaction debt;
    private CsePersonAccount supported;
  }
#endregion
}
