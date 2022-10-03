// Program: FN_RPT1_MONEY_ON_CLOSED_CASES, ID: 371121869, model: 746.
// Short name: SWERPT1P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RPT1_MONEY_ON_CLOSED_CASES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnRpt1MoneyOnClosedCases: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RPT1_MONEY_ON_CLOSED_CASES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRpt1MoneyOnClosedCases(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRpt1MoneyOnClosedCases.
  /// </summary>
  public FnRpt1MoneyOnClosedCases(IContext context, Import import, Export export)
    :
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
    local.MaxLinesPerPage.Count = 50;
    local.ProgramProcessingInfo.ProcessDate = Now().Date;
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
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "" + "Case #    " + "  " + "AP #/Name                                  " +
      "  " + "CH #/Name                                  " + "  " + "OB ID" + "  " +
      "Court Order #";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NextPrintLine.Count = 1;

    foreach(var item in ReadCase2())
    {
      local.ApFnd.Flag = "N";
      local.ChFnd.Flag = "N";
      local.HoldAp.Number = "";
      local.HoldCh.Number = "";

      foreach(var item1 in ReadCsePersonAbsentParent())
      {
        local.ApFnd.Flag = "Y";

        if (Equal(entities.ExistingClosedAp.Number, local.HoldAp.Number))
        {
          continue;
        }

        local.HoldAp.Number = entities.ExistingClosedAp.Number;

        foreach(var item2 in ReadCsePersonChild())
        {
          local.ChFnd.Flag = "Y";

          if (Equal(entities.ExistingClosedCh.Number, local.HoldCh.Number))
          {
            continue;
          }

          local.HoldCh.Number = entities.ExistingClosedCh.Number;

          if (ReadCase1())
          {
            continue;
          }
          else
          {
            if (ReadObligationObligationTypeDebtDetail())
            {
              // : Balance Due Found on an Closed Case - Continue Processing.
            }
            else
            {
              continue;
            }

            if (ReadLegalAction())
            {
              local.LegalAction.StandardNumber =
                entities.ExistingLegalAction.StandardNumber;
            }
            else
            {
              local.LegalAction.StandardNumber = "";
            }

            ++local.CaseCnt.Count;

            if (local.NextPrintLine.Count > local.MaxLinesPerPage.Count)
            {
              local.EabReportSend.RptDetail = "" + "Case #    " + "  " + "AP #/Name                                  " +
                "  " + "CH #/Name                                  " + "  " + "OB ID" +
                "  " + "Court Order #";
              UseCabControlReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

                return;
              }

              local.NextPrintLine.Count = 1;
            }

            local.Ap.Number = entities.ExistingClosedAp.Number;
            UseSiReadCsePersonBatch1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
              local.Ap.FormattedName = "** AP Name Is Unavailable **";
            }

            local.Ch.Number = entities.ExistingClosedCh.Number;
            UseSiReadCsePersonBatch2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
              local.Ch.FormattedName = "** CH Name Is Unavailable **";
            }

            local.EabReportSend.RptDetail = "" + entities
              .ExistingClosedCase.Number + "  " + entities
              .ExistingClosedAp.Number + " " + local.Ap.FormattedName + " " + entities
              .ExistingClosedCh.Number + " " + local.Ch.FormattedName + "   " +
              NumberToString
              (entities.ExistingObligation.SystemGeneratedIdentifier, 13, 3) + "  " +
              (local.LegalAction.StandardNumber ?? "") + "" + "" + "" + "";
            UseCabControlReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

              return;
            }

            ++local.NextPrintLine.Count;
          }
        }

        if (AsChar(local.ChFnd.Flag) == 'N')
        {
          ++local.ErrorCnt.Count;
          local.EabReportSend.RptDetail = "CH IS MISSING ON CASE #: " + entities
            .ExistingClosedCase.Number;
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }
        }
      }

      if (AsChar(local.ApFnd.Flag) == 'N')
      {
        ++local.ErrorCnt.Count;
        local.EabReportSend.RptDetail = "AP IS MISSING ON CASE #: " + entities
          .ExistingClosedCase.Number;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

          return;
        }
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Case Count . . . . . . . . . . . . : " + NumberToString
      (local.CaseCnt.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Error Count. . . . . . . . . . . . : " + NumberToString
      (local.ErrorCnt.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";

    // **********************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
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

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiReadCsePersonBatch1()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ap.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.Ap);
  }

  private void UseSiReadCsePersonBatch2()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ch.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.Ch);
  }

  private bool ReadCase1()
  {
    entities.ExistingOpenCase.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ExistingClosedAp.Number);
        db.SetString(command, "cspNumber2", entities.ExistingClosedCh.Number);
      },
      (db, reader) =>
      {
        entities.ExistingOpenCase.Number = db.GetString(reader, 0);
        entities.ExistingOpenCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingOpenCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.ExistingClosedCase.Populated = false;

    return ReadEach("ReadCase2",
      null,
      (db, reader) =>
      {
        entities.ExistingClosedCase.Number = db.GetString(reader, 0);
        entities.ExistingClosedCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingClosedCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent()
  {
    entities.ExistingClosedAbsentParent.Populated = false;
    entities.ExistingClosedAp.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingClosedCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingClosedAp.Number = db.GetString(reader, 0);
        entities.ExistingClosedAbsentParent.CspNumber = db.GetString(reader, 0);
        entities.ExistingClosedAbsentParent.CasNumber = db.GetString(reader, 1);
        entities.ExistingClosedAbsentParent.Type1 = db.GetString(reader, 2);
        entities.ExistingClosedAbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.ExistingClosedAbsentParent.Populated = true;
        entities.ExistingClosedAp.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonChild()
  {
    entities.ExistingClosedChild.Populated = false;
    entities.ExistingClosedCh.Populated = false;

    return ReadEach("ReadCsePersonChild",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingClosedCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingClosedCh.Number = db.GetString(reader, 0);
        entities.ExistingClosedChild.CspNumber = db.GetString(reader, 0);
        entities.ExistingClosedChild.CasNumber = db.GetString(reader, 1);
        entities.ExistingClosedChild.Type1 = db.GetString(reader, 2);
        entities.ExistingClosedChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingClosedChild.Populated = true;
        entities.ExistingClosedCh.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadObligationObligationTypeDebtDetail()
  {
    entities.ExistingObligation.Populated = false;
    entities.ExistingObligationType.Populated = false;
    entities.ExistingDebtDetail.Populated = false;

    return Read("ReadObligationObligationTypeDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingClosedAp.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ExistingClosedCh.Number);
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
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
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 6);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 7);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 8);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 10);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 11);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 13);
        entities.ExistingObligation.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
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
    /// A value of HoldCh.
    /// </summary>
    [JsonPropertyName("holdCh")]
    public CsePerson HoldCh
    {
      get => holdCh ??= new();
      set => holdCh = value;
    }

    /// <summary>
    /// A value of HoldAp.
    /// </summary>
    [JsonPropertyName("holdAp")]
    public CsePerson HoldAp
    {
      get => holdAp ??= new();
      set => holdAp = value;
    }

    /// <summary>
    /// A value of ApFnd.
    /// </summary>
    [JsonPropertyName("apFnd")]
    public Common ApFnd
    {
      get => apFnd ??= new();
      set => apFnd = value;
    }

    /// <summary>
    /// A value of ChFnd.
    /// </summary>
    [JsonPropertyName("chFnd")]
    public Common ChFnd
    {
      get => chFnd ??= new();
      set => chFnd = value;
    }

    /// <summary>
    /// A value of SvcPrvdrName.
    /// </summary>
    [JsonPropertyName("svcPrvdrName")]
    public CsePersonsWorkSet SvcPrvdrName
    {
      get => svcPrvdrName ??= new();
      set => svcPrvdrName = value;
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
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public Common Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    /// <summary>
    /// A value of NextPrintLine.
    /// </summary>
    [JsonPropertyName("nextPrintLine")]
    public Common NextPrintLine
    {
      get => nextPrintLine ??= new();
      set => nextPrintLine = value;
    }

    /// <summary>
    /// A value of MaxLinesPerPage.
    /// </summary>
    [JsonPropertyName("maxLinesPerPage")]
    public Common MaxLinesPerPage
    {
      get => maxLinesPerPage ??= new();
      set => maxLinesPerPage = value;
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
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePersonsWorkSet Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of ErrorCnt.
    /// </summary>
    [JsonPropertyName("errorCnt")]
    public Common ErrorCnt
    {
      get => errorCnt ??= new();
      set => errorCnt = value;
    }

    /// <summary>
    /// A value of CaseCnt.
    /// </summary>
    [JsonPropertyName("caseCnt")]
    public Common CaseCnt
    {
      get => caseCnt ??= new();
      set => caseCnt = value;
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

    private CsePerson holdCh;
    private CsePerson holdAp;
    private Common apFnd;
    private Common chFnd;
    private CsePersonsWorkSet svcPrvdrName;
    private Case1 case1;
    private Common tmp;
    private Common nextPrintLine;
    private Common maxLinesPerPage;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ch;
    private Common lastNameLength;
    private Common firstNameLength;
    private Office office;
    private ServiceProvider serviceProvider;
    private Common errorCnt;
    private Common caseCnt;
    private LegalAction legalAction;
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
    /// A value of ExistingOpenAbsentParent.
    /// </summary>
    [JsonPropertyName("existingOpenAbsentParent")]
    public CaseRole ExistingOpenAbsentParent
    {
      get => existingOpenAbsentParent ??= new();
      set => existingOpenAbsentParent = value;
    }

    /// <summary>
    /// A value of ExistingOpenChild.
    /// </summary>
    [JsonPropertyName("existingOpenChild")]
    public CaseRole ExistingOpenChild
    {
      get => existingOpenChild ??= new();
      set => existingOpenChild = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    /// <summary>
    /// A value of ExistingClosedCase.
    /// </summary>
    [JsonPropertyName("existingClosedCase")]
    public Case1 ExistingClosedCase
    {
      get => existingClosedCase ??= new();
      set => existingClosedCase = value;
    }

    /// <summary>
    /// A value of ExistingOpenCase.
    /// </summary>
    [JsonPropertyName("existingOpenCase")]
    public Case1 ExistingOpenCase
    {
      get => existingOpenCase ??= new();
      set => existingOpenCase = value;
    }

    /// <summary>
    /// A value of ExistingClosedAbsentParent.
    /// </summary>
    [JsonPropertyName("existingClosedAbsentParent")]
    public CaseRole ExistingClosedAbsentParent
    {
      get => existingClosedAbsentParent ??= new();
      set => existingClosedAbsentParent = value;
    }

    /// <summary>
    /// A value of ExistingClosedChild.
    /// </summary>
    [JsonPropertyName("existingClosedChild")]
    public CaseRole ExistingClosedChild
    {
      get => existingClosedChild ??= new();
      set => existingClosedChild = value;
    }

    /// <summary>
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public LegalActionCaseRole ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
    }

    /// <summary>
    /// A value of ExistingCh.
    /// </summary>
    [JsonPropertyName("existingCh")]
    public LegalActionCaseRole ExistingCh
    {
      get => existingCh ??= new();
      set => existingCh = value;
    }

    /// <summary>
    /// A value of ExistingClosedAp.
    /// </summary>
    [JsonPropertyName("existingClosedAp")]
    public CsePerson ExistingClosedAp
    {
      get => existingClosedAp ??= new();
      set => existingClosedAp = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingClosedCh.
    /// </summary>
    [JsonPropertyName("existingClosedCh")]
    public CsePerson ExistingClosedCh
    {
      get => existingClosedCh ??= new();
      set => existingClosedCh = value;
    }

    /// <summary>
    /// A value of ExistingSupported.
    /// </summary>
    [JsonPropertyName("existingSupported")]
    public CsePersonAccount ExistingSupported
    {
      get => existingSupported ??= new();
      set => existingSupported = value;
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
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    private CaseRole existingOpenAbsentParent;
    private CaseRole existingOpenChild;
    private Office existingOffice;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private CaseAssignment existingCaseAssignment;
    private Case1 existingClosedCase;
    private Case1 existingOpenCase;
    private CaseRole existingClosedAbsentParent;
    private CaseRole existingClosedChild;
    private LegalActionCaseRole existingAp;
    private LegalActionCaseRole existingCh;
    private CsePerson existingClosedAp;
    private CsePersonAccount existingObligor;
    private CsePerson existingClosedCh;
    private CsePersonAccount existingSupported;
    private Obligation existingObligation;
    private ObligationType existingObligationType;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private LegalAction existingLegalAction;
  }
#endregion
}
