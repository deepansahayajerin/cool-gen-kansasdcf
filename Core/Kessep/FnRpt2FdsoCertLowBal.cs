// Program: FN_RPT2_FDSO_CERT_LOW_BAL, ID: 371112080, model: 746.
// Short name: SWERPT2P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RPT2_FDSO_CERT_LOW_BAL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnRpt2FdsoCertLowBal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RPT2_FDSO_CERT_LOW_BAL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRpt2FdsoCertLowBal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRpt2FdsoCertLowBal.
  /// </summary>
  public FnRpt2FdsoCertLowBal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodedAccruing.Classification = "A";
    local.Current.Date = Now().Date;
    local.FirstOfMonth.Date = Now().Date.AddDays(-Now().Date.Day);
    local.FirstOfMonth.Date = AddDays(local.FirstOfMonth.Date, 1);
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = global.UserId;
    local.EabFileHandling.Action = "OPEN";

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "TYPE OBLIGOR-#  FIRST-NAME      LAST-NAME                SSN         AF-CERT-AMT     NA-CERT-AMT      AF-AMT-DUE      NA-AMT-DUE";
      
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    local.Hold.Number = "";

    foreach(var item in ReadCsePersonObligorFederalDebtSetoff())
    {
      if (Equal(local.Hold.Number, entities.ExistingObligor1.Number))
      {
        continue;
      }

      local.Hold.Number = entities.ExistingObligor1.Number;

      if (AsChar(entities.ExistingFederalDebtSetoff.TtypeDDeleteCertification) ==
        'D')
      {
        continue;
      }

      local.Af.TotalCurrency = 0;
      local.Na.TotalCurrency = 0;

      foreach(var item1 in ReadObligationObligationTypeDebtDetailCsePerson())
      {
        if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == 'S')
        {
          continue;
        }

        UseFnDeterminePgmForDebtDetail();

        if (Equal(local.Program.Code, "AF") || Equal(local.Program.Code, "FC"))
        {
          local.Af.TotalCurrency += entities.ExistingDebtDetail.BalanceDueAmt;
        }
        else if (Equal(local.Program.Code, "NA"))
        {
          if (Equal(local.DprProgram.ProgramState, "CA"))
          {
            local.Af.TotalCurrency += entities.ExistingDebtDetail.BalanceDueAmt;
          }
          else
          {
            local.Na.TotalCurrency += entities.ExistingDebtDetail.BalanceDueAmt;
          }
        }
        else if (Equal(local.Program.Code, "NF") || Equal
          (local.Program.Code, "NC"))
        {
          local.Na.TotalCurrency += entities.ExistingDebtDetail.BalanceDueAmt;
        }
      }

      if (local.Af.TotalCurrency + local.Na.TotalCurrency > 25)
      {
        continue;
      }

      if ((!Lt(
        local.Af.TotalCurrency, entities.ExistingFederalDebtSetoff.AdcAmount) ||
        Equal(entities.ExistingFederalDebtSetoff.AdcAmount, 0)) && (
          !Lt(local.Na.TotalCurrency,
        entities.ExistingFederalDebtSetoff.NonAdcAmount) || Equal
        (entities.ExistingFederalDebtSetoff.NonAdcAmount, 0)))
      {
        continue;
      }

      // : Print this one!!!
      ++local.RecCnt.Count;
      local.EabReportSend.RptDetail = "  " + entities
        .ExistingFederalDebtSetoff.CaseType + "  " + entities
        .ExistingObligor1.Number + " " + entities
        .ExistingFederalDebtSetoff.FirstName + " " + entities
        .ExistingFederalDebtSetoff.LastName + " " + NumberToString
        (entities.ExistingFederalDebtSetoff.Ssn, 7, 3) + "-" + NumberToString
        (entities.ExistingFederalDebtSetoff.Ssn, 10, 2) + "-" + NumberToString
        (entities.ExistingFederalDebtSetoff.Ssn, 12, 4) + " " + NumberToString
        ((long)entities.ExistingFederalDebtSetoff.AdcAmount.GetValueOrDefault(),
        15) + " " + NumberToString
        ((long)entities.ExistingFederalDebtSetoff.NonAdcAmount.
          GetValueOrDefault(), 15) + " " + NumberToString
        ((long)local.Af.TotalCurrency, 15) + " " + NumberToString
        ((long)local.Na.TotalCurrency, 15);
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FILE_WRITE_ERROR_RB";

        return;
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    local.EabReportSend.RptDetail =
      "Record Count . . . . . . . . . . . . : " + NumberToString
      (local.RecCnt.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";

    // **********************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FILE_CLOSE_ERROR";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FILE_CLOSE_ERROR";
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.Obligation.OrderTypeCode =
      entities.ExistingObligation.OrderTypeCode;
    MoveObligationType(entities.ExistingObligationType, useImport.ObligationType);
      
    MoveDebtDetail(entities.ExistingDebtDetail, useImport.DebtDetail);
    useImport.SupportedPerson.Number = entities.ExistingSupported1.Number;
    useImport.HardcodedAccruing.Classification =
      local.HardcodedAccruing.Classification;
    useImport.Collection.Date = local.Current.Date;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.Program.Assign(useExport.Program);
  }

  private IEnumerable<bool> ReadCsePersonObligorFederalDebtSetoff()
  {
    entities.ExistingObligor1.Populated = false;
    entities.ExistingFederalDebtSetoff.Populated = false;
    entities.ExistingObligor2.Populated = false;

    return ReadEach("ReadCsePersonObligorFederalDebtSetoff",
      null,
      (db, reader) =>
      {
        entities.ExistingObligor1.Number = db.GetString(reader, 0);
        entities.ExistingObligor2.CspNumber = db.GetString(reader, 0);
        entities.ExistingFederalDebtSetoff.CspNumber = db.GetString(reader, 0);
        entities.ExistingFederalDebtSetoff.CspNumber = db.GetString(reader, 0);
        entities.ExistingObligor2.Type1 = db.GetString(reader, 1);
        entities.ExistingFederalDebtSetoff.CpaType = db.GetString(reader, 1);
        entities.ExistingFederalDebtSetoff.Type1 = db.GetString(reader, 2);
        entities.ExistingFederalDebtSetoff.TakenDate = db.GetDate(reader, 3);
        entities.ExistingFederalDebtSetoff.AdcAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingFederalDebtSetoff.NonAdcAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingFederalDebtSetoff.Ssn = db.GetInt32(reader, 6);
        entities.ExistingFederalDebtSetoff.CaseNumber = db.GetString(reader, 7);
        entities.ExistingFederalDebtSetoff.LastName = db.GetString(reader, 8);
        entities.ExistingFederalDebtSetoff.FirstName = db.GetString(reader, 9);
        entities.ExistingFederalDebtSetoff.CaseType = db.GetString(reader, 10);
        entities.ExistingFederalDebtSetoff.TanfCode = db.GetString(reader, 11);
        entities.ExistingFederalDebtSetoff.TtypeDDeleteCertification =
          db.GetNullableString(reader, 12);
        entities.ExistingObligor1.Populated = true;
        entities.ExistingFederalDebtSetoff.Populated = true;
        entities.ExistingObligor2.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationTypeDebtDetailCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligor2.Populated);
    entities.ExistingObligation.Populated = false;
    entities.ExistingObligationType.Populated = false;
    entities.ExistingDebtDetail.Populated = false;
    entities.ExistingSupported1.Populated = false;

    return ReadEach("ReadObligationObligationTypeDebtDetailCsePerson",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.ExistingObligor2.Type1);
        db.SetString(command, "cspNumber", entities.ExistingObligor2.CspNumber);
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.
          SetDate(command, "dueDt", local.FirstOfMonth.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.ExistingObligation.OrderTypeCode = db.GetString(reader, 5);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 6);
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 7);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 8);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 9);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 10);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 11);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 12);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 13);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 14);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 15);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 16);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 17);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 18);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 19);
        entities.ExistingSupported1.Number = db.GetString(reader, 20);
        entities.ExistingObligation.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
        entities.ExistingSupported1.Populated = true;

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
    /// A value of FirstOfMonth.
    /// </summary>
    [JsonPropertyName("firstOfMonth")]
    public DateWorkArea FirstOfMonth
    {
      get => firstOfMonth ??= new();
      set => firstOfMonth = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public CsePerson Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of HardcodedAccruing.
    /// </summary>
    [JsonPropertyName("hardcodedAccruing")]
    public ObligationType HardcodedAccruing
    {
      get => hardcodedAccruing ??= new();
      set => hardcodedAccruing = value;
    }

    /// <summary>
    /// A value of Na.
    /// </summary>
    [JsonPropertyName("na")]
    public Common Na
    {
      get => na ??= new();
      set => na = value;
    }

    /// <summary>
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public Common Af
    {
      get => af ??= new();
      set => af = value;
    }

    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
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
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public Common Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of LastNameLength.
    /// </summary>
    [JsonPropertyName("lastNameLength")]
    public Common LastNameLength
    {
      get => lastNameLength ??= new();
      set => lastNameLength = value;
    }

    /// <summary>
    /// A value of FirstNameLength.
    /// </summary>
    [JsonPropertyName("firstNameLength")]
    public Common FirstNameLength
    {
      get => firstNameLength ??= new();
      set => firstNameLength = value;
    }

    /// <summary>
    /// A value of TotCaseCnt.
    /// </summary>
    [JsonPropertyName("totCaseCnt")]
    public Common TotCaseCnt
    {
      get => totCaseCnt ??= new();
      set => totCaseCnt = value;
    }

    /// <summary>
    /// A value of RecCnt.
    /// </summary>
    [JsonPropertyName("recCnt")]
    public Common RecCnt
    {
      get => recCnt ??= new();
      set => recCnt = value;
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

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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

    private DateWorkArea firstOfMonth;
    private CsePerson hold;
    private ObligationType hardcodedAccruing;
    private Common na;
    private Common af;
    private DprProgram dprProgram;
    private Program program;
    private DateWorkArea current;
    private Common tmp;
    private CsePersonsWorkSet ap;
    private Common lastNameLength;
    private Common firstNameLength;
    private Common totCaseCnt;
    private Common recCnt;
    private DateWorkArea null1;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External external;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingFederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("existingFederalDebtSetoff")]
    public AdministrativeActCertification ExistingFederalDebtSetoff
    {
      get => existingFederalDebtSetoff ??= new();
      set => existingFederalDebtSetoff = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingSupported1.
    /// </summary>
    [JsonPropertyName("existingSupported1")]
    public CsePerson ExistingSupported1
    {
      get => existingSupported1 ??= new();
      set => existingSupported1 = value;
    }

    /// <summary>
    /// A value of ExistingSupported2.
    /// </summary>
    [JsonPropertyName("existingSupported2")]
    public CsePersonAccount ExistingSupported2
    {
      get => existingSupported2 ??= new();
      set => existingSupported2 = value;
    }

    private CsePerson existingObligor1;
    private AdministrativeActCertification existingFederalDebtSetoff;
    private CsePersonAccount existingObligor2;
    private Obligation existingObligation;
    private ObligationType existingObligationType;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private CsePerson existingSupported1;
    private CsePersonAccount existingSupported2;
  }
#endregion
}
