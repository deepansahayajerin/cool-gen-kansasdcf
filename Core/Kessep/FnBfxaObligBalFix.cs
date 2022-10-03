// Program: FN_BFXA_OBLIG_BAL_FIX, ID: 372983059, model: 746.
// Short name: SWEFFXAB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXA_OBLIG_BAL_FIX.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxaObligBalFix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXA_OBLIG_BAL_FIX program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxaObligBalFix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxaObligBalFix.
  /// </summary>
  public FnBfxaObligBalFix(IContext context, Import import, Export export):
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
    local.UserId.Text8 = global.UserId;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = local.UserId.Text8;
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    local.ProcessUpdatesInd.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

    if (IsEmpty(local.ProcessUpdatesInd.Flag))
    {
      local.ProcessUpdatesInd.Flag = "N";
    }

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
    local.EabReportSend.RptDetail = "PARMS:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Perform Updates . . . . . . . : " + local
      .ProcessUpdatesInd.Flag;
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

    local.EabReportSend.RptDetail =
      "                     PERSON      OBLIG  DUE-DATE   ORG-AMT-DUE   COLL/ADJ AMT    CALC AMT OWED  INC BAL OWED";
      
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **************************************************************
    foreach(var item in ReadDebtDebtDetailObligationObligationType())
    {
      ++local.ReadCnt.Count;

      // *****************************************************************
      // Bypass debts that were created via conversion and have a zero
      // amount.
      // *****************************************************************
      if (Equal(entities.ExistingDebt.CreatedBy, "CONVERSN") && entities
        .ExistingDebt.Amount == 0)
      {
        continue;
      }

      // *****************************************************************
      // Bypass voluntary obligations.
      // *****************************************************************
      if (entities.ExistingObligationType.SystemGeneratedIdentifier == 16)
      {
        continue;
      }

      local.NetAdj.TotalCurrency = 0;
      ReadCollection();

      foreach(var item1 in ReadDebtAdjustment())
      {
        switch(AsChar(entities.ExistingDebtAdjustment.DebtAdjustmentType))
        {
          case 'I':
            local.NetAdj.TotalCurrency -= entities.ExistingDebtAdjustment.
              Amount;

            break;
          case 'D':
            local.NetAdj.TotalCurrency += entities.ExistingDebtAdjustment.
              Amount;

            break;
          default:
            break;
        }
      }

      if (entities.ExistingDebt.Amount == entities
        .ExistingDebtDetail.BalanceDueAmt + local.NetAdj.TotalCurrency)
      {
        continue;
      }

      if (ReadObligorCsePerson())
      {
        // Continue
      }
      else
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.AlphaObligationId.Text3 =
        NumberToString(entities.ExistingObligation.SystemGeneratedIdentifier, 3);
        
      local.DateFormat.SelectChar = "C";
      local.DueDate.Date = entities.ExistingDebtDetail.DueDt;
      UseFnCabFormatDateText();
      local.AlphaDebtDate.Text8 =
        Substring(local.Text20Date.TextDate20Char, 1, 8);
      local.EabConvertNumeric.SendNonSuppressPos = 0;

      // ****  Format Debt Amount   ***
      if (entities.ExistingDebt.Amount < 0)
      {
        local.EabConvertNumeric.SendSign = "-";
      }
      else
      {
        local.EabConvertNumeric.SendSign = "";
      }

      local.EabConvertNumeric.SendAmount =
        NumberToString((long)(entities.ExistingDebt.Amount * 100), 15);
      UseEabConvertNumeric1();

      if (AsChar(local.EabConvertNumeric.ReturnOkFlag) == 'Y')
      {
        local.AlphaDebtAmt.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
      }
      else
      {
        return;
      }

      // ****  Format Net Adjustment   ***
      local.EabConvertNumeric.SendSign = "";
      local.EabConvertNumeric.SendAmount =
        NumberToString((long)(local.NetAdj.TotalCurrency * 100), 15);
      UseEabConvertNumeric1();

      if (AsChar(local.EabConvertNumeric.ReturnOkFlag) == 'Y')
      {
        local.AlphaNetAdj.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);

        if (local.NetAdj.TotalCurrency > 0)
        {
          local.AlphaNetAdj.Text10 =
            Substring(local.AlphaNetAdj.Text10, WorkArea.Text10_MaxLength, 1, 9) +
            "-";
        }
        else
        {
          local.AlphaNetAdj.Text10 =
            Substring(local.AlphaNetAdj.Text10, WorkArea.Text10_MaxLength, 1, 9) +
            " ";
        }
      }
      else
      {
        return;
      }

      local.CalcAmtWork.TotalCurrency = entities.ExistingDebt.Amount - local
        .NetAdj.TotalCurrency;

      if (local.CalcAmtWork.TotalCurrency > 0)
      {
        local.EabConvertNumeric.SendSign = "";
      }
      else
      {
        local.EabConvertNumeric.SendSign = "-";
      }

      local.EabConvertNumeric.SendAmount = NumberToString((long)(100 * local
        .CalcAmtWork.TotalCurrency), 15);
      UseEabConvertNumeric1();

      if (AsChar(local.EabConvertNumeric.ReturnOkFlag) == 'Y')
      {
        local.AlphaCalcAmtOwed.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
      }
      else
      {
        return;
      }

      // ****  Format Balance Due   ***
      if (entities.ExistingDebtDetail.BalanceDueAmt < 0)
      {
        local.EabConvertNumeric.SendSign = "-";
      }
      else
      {
        local.EabConvertNumeric.SendSign = "";
      }

      local.EabConvertNumeric.SendAmount =
        NumberToString((long)(entities.ExistingDebtDetail.BalanceDueAmt * 100),
        15);
      UseEabConvertNumeric1();

      if (AsChar(local.EabConvertNumeric.ReturnOkFlag) == 'Y')
      {
        local.AlphaBalDue.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
      }
      else
      {
        return;
      }

      local.EabReportSend.RptDetail = "**  OUT OF BAL  **" + "    " + entities
        .ExistingCsePerson.Number + "    " + local.AlphaObligationId.Text3 + "    " +
        local.AlphaDebtDate.Text8 + "    " + local.AlphaDebtAmt.Text10 + "    " +
        local.AlphaNetAdj.Text10 + "    " + local.AlphaCalcAmtOwed.Text10 + "    " +
        local.AlphaBalDue.Text10;
      ++local.ErrorCnt.Count;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    if (AsChar(local.ProcessUpdatesInd.Flag) == 'Y')
    {
      ++local.UpdateCnt.Count;
    }

    // : Print Summary Totals
    UseCabTextnum1();
    local.EabReportSend.RptDetail = "Read Count . . . . . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum2();
    local.EabReportSend.RptDetail = "Update Count . . . . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum3();
    local.EabReportSend.RptDetail = "Error Count. . . . . . . . . . . . : " + local
      .WorkArea.Text9;
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

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnCurrencySigned = source.ReturnCurrencySigned;
    target.ReturnOkFlag = source.ReturnOkFlag;
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

  private void UseCabTextnum1()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.ReadCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum2()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.UpdateCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum3()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.ErrorCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.EabConvertNumeric);
    MoveEabConvertNumeric2(local.EabConvertNumeric, useExport.EabConvertNumeric);
      

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric2(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }

  private void UseFnCabFormatDateText()
  {
    var useImport = new FnCabFormatDateText.Import();
    var useExport = new FnCabFormatDateText.Export();

    useImport.DateWorkArea.Date = local.DueDate.Date;
    useImport.DateTextStyle.SelectChar = local.DateFormat.SelectChar;

    Call(FnCabFormatDateText.Execute, useImport, useExport);

    local.Text20Date.TextDate20Char = useExport.DateText.TextDate20Char;
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;
    useExport.ProgramProcessingInfo.ParameterList =
      local.ProgramProcessingInfo.ParameterList;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
    local.ProgramProcessingInfo.ParameterList =
      useExport.ProgramProcessingInfo.ParameterList;
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.ExistingDebt.OtyType);
        db.SetString(command, "otrType", entities.ExistingDebt.Type1);
        db.SetInt32(
          command, "otrId", entities.ExistingDebt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.ExistingDebt.CpaType);
        db.SetString(command, "cspNumber", entities.ExistingDebt.CspNumber);
        db.SetInt32(command, "obgId", entities.ExistingDebt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        local.NetAdj.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private IEnumerable<bool> ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);
    entities.ExistingDebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.ExistingDebt.OtyType);
        db.SetString(command, "otrPType", entities.ExistingDebt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId",
          entities.ExistingDebt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.ExistingDebt.CpaType);
        db.SetString(command, "cspPNumber", entities.ExistingDebt.CspNumber);
        db.SetInt32(
          command, "obgPGeneratedId", entities.ExistingDebt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingDebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.ExistingDebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.ExistingDebtAdjustment.DebtAdjustmentType =
          db.GetString(reader, 6);
        entities.ExistingDebtAdjustment.CspSupNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingDebtAdjustment.CpaSupType =
          db.GetNullableString(reader, 8);
        entities.ExistingDebtAdjustment.OtyType = db.GetInt32(reader, 9);
        entities.ExistingDebtAdjustment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDebtDetailObligationObligationType()
  {
    entities.ExistingDebt.Populated = false;
    entities.ExistingDebtDetail.Populated = false;
    entities.ExistingObligation.Populated = false;
    entities.ExistingObligationType.Populated = false;

    return ReadEach("ReadDebtDebtDetailObligationObligationType",
      null,
      (db, reader) =>
      {
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebt.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingObligation.CpaType = db.GetString(reader, 2);
        entities.ExistingObligation.CpaType = db.GetString(reader, 2);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebt.Type1 = db.GetString(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 4);
        entities.ExistingDebt.Amount = db.GetDecimal(reader, 5);
        entities.ExistingDebt.CreatedBy = db.GetString(reader, 6);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 8);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 9);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 10);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 11);
        entities.ExistingDebt.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingObligationType.Populated = true;

        return true;
      });
  }

  private bool ReadObligorCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligor.Populated = false;
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadObligorCsePerson",
      (db, command) =>
      {
        db.SetString(command, "type", entities.ExistingObligation.CpaType);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.ExistingObligor.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingObligor.Type1 = db.GetString(reader, 1);
        entities.ExistingObligor.AsOfDtTotGiftColl =
          db.GetNullableDecimal(reader, 2);
        entities.ExistingObligor.Populated = true;
        entities.ExistingCsePerson.Populated = true;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of AlphaCalcAmtOwed.
    /// </summary>
    [JsonPropertyName("alphaCalcAmtOwed")]
    public WorkArea AlphaCalcAmtOwed
    {
      get => alphaCalcAmtOwed ??= new();
      set => alphaCalcAmtOwed = value;
    }

    /// <summary>
    /// A value of AlphaNetAdj.
    /// </summary>
    [JsonPropertyName("alphaNetAdj")]
    public WorkArea AlphaNetAdj
    {
      get => alphaNetAdj ??= new();
      set => alphaNetAdj = value;
    }

    /// <summary>
    /// A value of AlphaBalDue.
    /// </summary>
    [JsonPropertyName("alphaBalDue")]
    public WorkArea AlphaBalDue
    {
      get => alphaBalDue ??= new();
      set => alphaBalDue = value;
    }

    /// <summary>
    /// A value of AlphaDebtAmt.
    /// </summary>
    [JsonPropertyName("alphaDebtAmt")]
    public WorkArea AlphaDebtAmt
    {
      get => alphaDebtAmt ??= new();
      set => alphaDebtAmt = value;
    }

    /// <summary>
    /// A value of CalcAmtWork.
    /// </summary>
    [JsonPropertyName("calcAmtWork")]
    public Common CalcAmtWork
    {
      get => calcAmtWork ??= new();
      set => calcAmtWork = value;
    }

    /// <summary>
    /// A value of CalcAmtSign.
    /// </summary>
    [JsonPropertyName("calcAmtSign")]
    public WorkArea CalcAmtSign
    {
      get => calcAmtSign ??= new();
      set => calcAmtSign = value;
    }

    /// <summary>
    /// A value of NetAdjSign.
    /// </summary>
    [JsonPropertyName("netAdjSign")]
    public WorkArea NetAdjSign
    {
      get => netAdjSign ??= new();
      set => netAdjSign = value;
    }

    /// <summary>
    /// A value of Text20Date.
    /// </summary>
    [JsonPropertyName("text20Date")]
    public DateWorkAttributes Text20Date
    {
      get => text20Date ??= new();
      set => text20Date = value;
    }

    /// <summary>
    /// A value of DueDate.
    /// </summary>
    [JsonPropertyName("dueDate")]
    public DateWorkArea DueDate
    {
      get => dueDate ??= new();
      set => dueDate = value;
    }

    /// <summary>
    /// A value of DateFormat.
    /// </summary>
    [JsonPropertyName("dateFormat")]
    public Common DateFormat
    {
      get => dateFormat ??= new();
      set => dateFormat = value;
    }

    /// <summary>
    /// A value of AlphaDebtDate.
    /// </summary>
    [JsonPropertyName("alphaDebtDate")]
    public TextWorkArea AlphaDebtDate
    {
      get => alphaDebtDate ??= new();
      set => alphaDebtDate = value;
    }

    /// <summary>
    /// A value of AlphaObligationId.
    /// </summary>
    [JsonPropertyName("alphaObligationId")]
    public WorkArea AlphaObligationId
    {
      get => alphaObligationId ??= new();
      set => alphaObligationId = value;
    }

    /// <summary>
    /// A value of NetAdj.
    /// </summary>
    [JsonPropertyName("netAdj")]
    public Common NetAdj
    {
      get => netAdj ??= new();
      set => netAdj = value;
    }

    /// <summary>
    /// A value of AlphaDiscontinueDate.
    /// </summary>
    [JsonPropertyName("alphaDiscontinueDate")]
    public TextWorkArea AlphaDiscontinueDate
    {
      get => alphaDiscontinueDate ??= new();
      set => alphaDiscontinueDate = value;
    }

    /// <summary>
    /// A value of AlphaAssignDate.
    /// </summary>
    [JsonPropertyName("alphaAssignDate")]
    public TextWorkArea AlphaAssignDate
    {
      get => alphaAssignDate ??= new();
      set => alphaAssignDate = value;
    }

    /// <summary>
    /// A value of AlphaEffectDate.
    /// </summary>
    [JsonPropertyName("alphaEffectDate")]
    public TextWorkArea AlphaEffectDate
    {
      get => alphaEffectDate ??= new();
      set => alphaEffectDate = value;
    }

    /// <summary>
    /// A value of PersonProgramEffDate.
    /// </summary>
    [JsonPropertyName("personProgramEffDate")]
    public DateWorkArea PersonProgramEffDate
    {
      get => personProgramEffDate ??= new();
      set => personProgramEffDate = value;
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
    /// A value of ObId.
    /// </summary>
    [JsonPropertyName("obId")]
    public TextWorkArea ObId
    {
      get => obId ??= new();
      set => obId = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
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
    /// A value of ProcessUpdatesInd.
    /// </summary>
    [JsonPropertyName("processUpdatesInd")]
    public Common ProcessUpdatesInd
    {
      get => processUpdatesInd ??= new();
      set => processUpdatesInd = value;
    }

    /// <summary>
    /// A value of ReadCnt.
    /// </summary>
    [JsonPropertyName("readCnt")]
    public Common ReadCnt
    {
      get => readCnt ??= new();
      set => readCnt = value;
    }

    /// <summary>
    /// A value of UpdateCnt.
    /// </summary>
    [JsonPropertyName("updateCnt")]
    public Common UpdateCnt
    {
      get => updateCnt ??= new();
      set => updateCnt = value;
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
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
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

    private EabConvertNumeric2 eabConvertNumeric;
    private WorkArea alphaCalcAmtOwed;
    private WorkArea alphaNetAdj;
    private WorkArea alphaBalDue;
    private WorkArea alphaDebtAmt;
    private Common calcAmtWork;
    private WorkArea calcAmtSign;
    private WorkArea netAdjSign;
    private DateWorkAttributes text20Date;
    private DateWorkArea dueDate;
    private Common dateFormat;
    private TextWorkArea alphaDebtDate;
    private WorkArea alphaObligationId;
    private Common netAdj;
    private TextWorkArea alphaDiscontinueDate;
    private TextWorkArea alphaAssignDate;
    private TextWorkArea alphaEffectDate;
    private DateWorkArea personProgramEffDate;
    private Common tmp;
    private TextWorkArea obId;
    private WorkArea workArea;
    private DateWorkArea null1;
    private Common processUpdatesInd;
    private Common readCnt;
    private Common updateCnt;
    private Common errorCnt;
    private TextWorkArea userId;
    private DateWorkArea current;
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
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
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
    /// A value of ExistingDebtAdjustment.
    /// </summary>
    [JsonPropertyName("existingDebtAdjustment")]
    public ObligationTransaction ExistingDebtAdjustment
    {
      get => existingDebtAdjustment ??= new();
      set => existingDebtAdjustment = value;
    }

    /// <summary>
    /// A value of ExistingObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("existingObligationTransactionRln")]
    public ObligationTransactionRln ExistingObligationTransactionRln
    {
      get => existingObligationTransactionRln ??= new();
      set => existingObligationTransactionRln = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
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
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingPersonProgram.
    /// </summary>
    [JsonPropertyName("existingPersonProgram")]
    public PersonProgram ExistingPersonProgram
    {
      get => existingPersonProgram ??= new();
      set => existingPersonProgram = value;
    }

    private CsePersonAccount existingObligor;
    private ObligationTransaction existingDebt;
    private ObligationTransaction existingDebtAdjustment;
    private ObligationTransactionRln existingObligationTransactionRln;
    private Collection existingCollection;
    private DebtDetail existingDebtDetail;
    private Obligation existingObligation;
    private ObligationType existingObligationType;
    private CsePerson existingCsePerson;
    private PersonProgram existingPersonProgram;
  }
#endregion
}
