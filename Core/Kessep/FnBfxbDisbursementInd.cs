// Program: FN_BFXB_DISBURSEMENT_IND, ID: 373515863, model: 746.
// Short name: SWEFFXBB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXB_DISBURSEMENT_IND.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxbDisbursementInd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXB_DISBURSEMENT_IND program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxbDisbursementInd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxbDisbursementInd.
  /// </summary>
  public FnBfxbDisbursementInd(IContext context, Import import, Export export):
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
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = global.UserId;
    local.EabFileHandling.Action = "OPEN";

    // *****************************************************************
    // OPEN    ERROR   REPORT
    // *****************************************************************
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *****************************************************************
    // OPEN CONTROL REPORT
    // *****************************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "PARMS:";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *****************************************************************
    // GET PARM ROW
    // *****************************************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabReportSend.RptDetail =
        "Program processing info not found for " + local
        .ProgramProcessingInfo.Name;
      UseCabErrorReport4();

      return;
    }

    local.ProcessUpdates.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);
    local.EabReportSend.RptDetail = "Update Indicator:  " + local
      .ProcessUpdates.Flag;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // *****************************************************************
    // PRINT REPORT COLUMN HEADINGS
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "                 COLL         COLL         COLL          DISB         DISB         DISB              DISB          PAY        PAY";
      
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail =
      "PERSON           ID        CREATE DATE     AMT          COLL ID         ID         REF NBR           AMT          RQST ID  RQST TYP";
      
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail =
      "            PERSON        REFERENCE                                  COLL                      EFFECT         DISC";
      
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail =
      "TABLE       NUMBER        NUMBER         COLL ID         ID           DATE           AMT          DATE          DATE      STATUS";
      
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.AugDate.Timestamp = new DateTime(1999, 9, 1);
    local.UnitOfWorkCnt.Number3 = 0;
    local.ProgramCheckpointRestart.UpdateFrequencyCount = 1000;

    // *****************************************************************
    // MAIN PROCESS
    // *****************************************************************
    foreach(var item in ReadCollectionDisbCollectionCsePerson())
    {
      local.EabReportSend.RptDetail = "****  Collection Read Each  ****  " + entities
        .CsePerson.Number;
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

        return;
      }

      ++local.ReadCollCnt.Count;
      local.RptCollId.Text9 =
        NumberToString(entities.Collection.SystemGeneratedIdentifier, 7, 9);
      local.Month.NumericalMonth = Month(entities.Collection.CollectionDt);
      local.Day.NumericalDay = Day(entities.Collection.CollectionDt);
      local.Year.NumericalYear = Year(entities.Collection.CollectionDt);
      local.RptCollDate.TextDate10Char =
        NumberToString(local.Month.NumericalMonth, 14, 2) + "/" + NumberToString
        (local.Day.NumericalDay, 14, 2) + "/" + NumberToString
        (local.Year.NumericalYear, 12, 4);
      local.Month.NumericalMonth = Month(entities.Collection.CreatedTmst);
      local.Day.NumericalDay = Day(entities.Collection.CreatedTmst);
      local.Year.NumericalYear = Year(entities.Collection.CreatedTmst);
      local.RptCollCreateDate.TextDate10Char =
        NumberToString(local.Month.NumericalMonth, 14, 2) + "/" + NumberToString
        (local.Day.NumericalDay, 14, 2) + "/" + NumberToString
        (local.Year.NumericalYear, 12, 4);

      foreach(var item1 in ReadDisbCollectionDisbursementPaymentRequestDisbursementTransactionRln())
        
      {
        local.RptDisbId.Text9 =
          NumberToString(entities.ForReadOnlyDisbursement.
            SystemGeneratedIdentifier, 7, 9);
        local.RptDisbCollId.Text9 =
          NumberToString(entities.ForReadOnlyDisbCollection.
            SystemGeneratedIdentifier, 7, 9);
        local.RptDisbRefNbr.Text14 =
          entities.ForReadOnlyDisbursement.ReferenceNumber ?? Spaces(14);

        // *******************************************************************
        // Format Collection Amount
        // *******************************************************************
        local.EabConvertNumeric.SendNonSuppressPos = 0;

        if (entities.Collection.Amount < 0)
        {
          local.EabConvertNumeric.SendSign = "-";
        }
        else
        {
          local.EabConvertNumeric.SendSign = "";
        }

        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(entities.Collection.Amount * 100), 15);
        UseEabConvertNumeric1();

        if (AsChar(local.EabConvertNumeric.ReturnOkFlag) == 'Y')
        {
          local.RptCollAmt.Text10 =
            Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
        }
        else
        {
          local.EabReportSend.RptDetail = "Bad Return from eab_convert_numeric";
          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }

          goto ReadEach2;
        }

        // *******************************************************************
        // Format Disbursement Amount
        // *******************************************************************
        if (entities.Disbursement.Amount < 0)
        {
          local.EabConvertNumeric.SendSign = "-";
        }
        else
        {
          local.EabConvertNumeric.SendSign = "";
        }

        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(entities.ForReadOnlyDisbursement.Amount * 100),
          15);
        UseEabConvertNumeric1();

        if (AsChar(local.EabConvertNumeric.ReturnOkFlag) == 'Y')
        {
          local.RptDisbAmt.Text10 =
            Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
        }
        else
        {
          local.EabReportSend.RptDetail = "Bad Return from eab_convert_numeric";
          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }

          goto ReadEach2;
        }

        local.RptPayRqstId.Text9 =
          NumberToString(entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9);
          
        local.EabReportSend.RptDetail = entities.CsePerson.Number + "    " + local
          .RptCollId.Text9 + "    " + local.RptCollCreateDate.TextDate10Char + "    " +
          local.RptCollAmt.Text10 + "    " + local.RptDisbCollId.Text9 + "    " +
          local.RptDisbId.Text9 + "    " + local.RptDisbRefNbr.Text14 + "    " +
          local.RptDisbAmt.Text10 + "    " + local.RptPayRqstId.Text9 + "    " +
          entities.PaymentRequest.Type1;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++local.ErrorCnt.Count;

        goto ReadEach1;
      }

      // *******************************************************************
      // Print disbursement collection control row
      // *******************************************************************
      local.EabReportSend.RptDetail = "";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

        return;
      }

      local.EabConvertNumeric.SendNonSuppressPos = 0;

      if (entities.DisbCollection.Amount < 0)
      {
        local.EabConvertNumeric.SendSign = "-";
      }
      else
      {
        local.EabConvertNumeric.SendSign = "";
      }

      local.EabConvertNumeric.SendAmount =
        NumberToString((long)(entities.DisbCollection.Amount * 100), 15);
      UseEabConvertNumeric1();

      if (AsChar(local.EabConvertNumeric.ReturnOkFlag) == 'Y')
      {
        local.RptDisbCollAmt.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
      }
      else
      {
        local.EabReportSend.RptDetail = "Bad Return from eab_convert_numeric";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

          return;
        }

        break;
      }

      local.RptDisbCollId.Text9 =
        NumberToString(entities.DisbCollection.SystemGeneratedIdentifier, 7, 9);
        
      local.EabReportSend.RptDetail = "COLL  " + "    " + entities
        .CsePerson.Number + "    " + entities.DisbCollection.ReferenceNumber + "    " +
        local.RptCollId.Text9 + "    " + local.RptDisbCollId.Text9 + "    " + local
        .RptCollDate.TextDate10Char + "    " + local.RptDisbCollAmt.Text10;
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

        return;
      }

      foreach(var item1 in ReadDisbursementDisbursementTransactionRlnDisbursementType())
        
      {
        // *******************************************************************
        // Print disbursement control row
        // *******************************************************************
        local.EabConvertNumeric.SendNonSuppressPos = 0;

        if (entities.Disbursement.Amount < 0)
        {
          local.EabConvertNumeric.SendSign = "-";
        }
        else
        {
          local.EabConvertNumeric.SendSign = "";
        }

        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(entities.Disbursement.Amount * 100), 15);
        UseEabConvertNumeric1();

        if (AsChar(local.EabConvertNumeric.ReturnOkFlag) == 'Y')
        {
          local.RptDisbAmt.Text10 =
            Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
        }
        else
        {
          local.EabReportSend.RptDetail = "Bad Return from eab_convert_numeric";
          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }

          goto ReadEach2;
        }

        local.RptDisbId.Text9 =
          NumberToString(entities.Disbursement.SystemGeneratedIdentifier, 7, 9);
          
        local.EabReportSend.RptDetail = "DISB  " + "    " + entities
          .CsePerson.Number + "    " + entities.Disbursement.ReferenceNumber + "    " +
          local.RptCollId.Text9 + "    " + local.RptDisbId.Text9 + "    " + local
          .RptCollDate.TextDate10Char + "    " + local.RptDisbAmt.Text10 + "    " +
          entities.DisbursementType.Code;
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

          return;
        }

        foreach(var item2 in ReadDisbursementStatusHistoryDisbursementStatus())
        {
          // *******************************************************************
          // Bypass delete if disb. status suppressed.
          // *******************************************************************
          if (entities.DisbursementStatus.SystemGeneratedIdentifier == 3)
          {
            if (Lt(local.ProgramProcessingInfo.ProcessDate,
              entities.DisbursementStatusHistory.DiscontinueDate))
            {
              local.RptDisbId.Text9 =
                NumberToString(entities.Disbursement.SystemGeneratedIdentifier,
                7, 9);
              local.RptDisbCollId.Text9 =
                NumberToString(entities.DisbCollection.
                  SystemGeneratedIdentifier, 7, 9);
              local.RptDisbRefNbr.Text14 =
                entities.Disbursement.ReferenceNumber ?? Spaces(14);

              // *******************************************************************
              // Format Collection Amount
              // *******************************************************************
              local.EabConvertNumeric.SendNonSuppressPos = 0;

              if (entities.Collection.Amount < 0)
              {
                local.EabConvertNumeric.SendSign = "-";
              }
              else
              {
                local.EabConvertNumeric.SendSign = "";
              }

              local.EabConvertNumeric.SendAmount =
                NumberToString((long)(entities.Collection.Amount * 100), 15);
              UseEabConvertNumeric1();

              if (AsChar(local.EabConvertNumeric.ReturnOkFlag) == 'Y')
              {
                local.RptCollAmt.Text10 =
                  Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
                  
              }
              else
              {
                local.EabReportSend.RptDetail =
                  "Bad Return from eab_convert_numeric";
                UseCabControlReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

                  return;
                }

                goto ReadEach2;
              }

              // *******************************************************************
              // Format Disbursement Amount
              // *******************************************************************
              if (entities.Disbursement.Amount < 0)
              {
                local.EabConvertNumeric.SendSign = "-";
              }
              else
              {
                local.EabConvertNumeric.SendSign = "";
              }

              local.EabConvertNumeric.SendAmount =
                NumberToString((long)(entities.ForReadOnlyDisbursement.Amount *
                100), 15);
              UseEabConvertNumeric1();

              if (AsChar(local.EabConvertNumeric.ReturnOkFlag) == 'Y')
              {
                local.RptDisbAmt.Text10 =
                  Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
                  
              }
              else
              {
                local.EabReportSend.RptDetail =
                  "Bad Return from eab_convert_numeric";
                UseCabControlReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

                  return;
                }

                goto ReadEach2;
              }

              local.RptPayRqstId.Text9 =
                NumberToString(entities.PaymentRequest.
                  SystemGeneratedIdentifier, 7, 9);
              local.EabReportSend.RptDetail = entities.CsePerson.Number + "    " +
                local.RptCollId.Text9 + "    " + local
                .RptCollCreateDate.TextDate10Char + "    " + local
                .RptCollAmt.Text10 + "    " + local.RptDisbCollId.Text9 + "    " +
                local.RptDisbId.Text9 + "    " + local.RptDisbRefNbr.Text14 + "    " +
                local.RptDisbAmt.Text10 + "    " + local.RptPayRqstId.Text9 + "    " +
                entities.PaymentRequest.Type1 + "*";
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              ++local.ErrorCnt.Count;

              goto ReadEach1;
            }
          }

          // *******************************************************************
          // Print disbursement status history control row
          // *******************************************************************
          local.RptDisbStatusHstId.Text9 =
            NumberToString(entities.DisbursementStatusHistory.
              SystemGeneratedIdentifier, 7, 9);
          local.Month.NumericalMonth =
            Month(entities.DisbursementStatusHistory.EffectiveDate);
          local.Day.NumericalDay =
            Day(entities.DisbursementStatusHistory.EffectiveDate);
          local.Year.NumericalYear =
            Year(entities.DisbursementStatusHistory.EffectiveDate);
          local.RptEffectDate.TextDate10Char =
            NumberToString(local.Month.NumericalMonth, 14, 2) + "/" + NumberToString
            (local.Day.NumericalDay, 14, 2) + "/" + NumberToString
            (local.Year.NumericalYear, 12, 4);
          local.Month.NumericalMonth =
            Month(entities.DisbursementStatusHistory.DiscontinueDate);
          local.Day.NumericalDay =
            Day(entities.DisbursementStatusHistory.DiscontinueDate);
          local.Year.NumericalYear =
            Year(entities.DisbursementStatusHistory.DiscontinueDate);
          local.RptDiscDate.TextDate10Char =
            NumberToString(local.Month.NumericalMonth, 14, 2) + "/" + NumberToString
            (local.Day.NumericalDay, 14, 2) + "/" + NumberToString
            (local.Year.NumericalYear, 12, 4);
          local.EabReportSend.RptDetail = "STATUS" + "    " + entities
            .CsePerson.Number + "    " + entities
            .Disbursement.ReferenceNumber + "    " + local.RptCollId.Text9 + "    " +
            local.RptDisbStatusHstId.Text9 + "                               " +
            local.RptEffectDate.TextDate10Char + "    " + local
            .RptDiscDate.TextDate10Char + "    " + entities
            .DisbursementStatus.Code;
          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }

          if (AsChar(local.ProcessUpdates.Flag) == 'Y')
          {
            DeleteDisbursementStatusHistory();
          }

          ++local.DeleteDisbStatHstCnt.Count;
        }

        if (AsChar(local.ProcessUpdates.Flag) == 'Y')
        {
          DeleteDisbursement();
        }

        ++local.DeleteDisbCnt.Count;

        if (AsChar(local.ProcessUpdates.Flag) == 'Y')
        {
          DeleteDisbursementTransactionRln();
        }
      }

      if (AsChar(local.ProcessUpdates.Flag) == 'Y')
      {
        DeleteDisbCollection();
      }

      ++local.DeleteDisbCollCnt.Count;
      ++local.UnitOfWorkCnt.Number3;

      if (AsChar(local.ProcessUpdates.Flag) == 'Y')
      {
        if (local.UnitOfWorkCnt.Number3 == local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.EabReportSend.RptDetail =
            "****  Preparing to Commit for  ****   " + entities
            .CsePerson.Number;
          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }

          UseExtToDoACommit();
          local.UnitOfWorkCnt.Number3 = 0;
          local.EabReportSend.RptDetail =
            "****  Commit performed for  ****   " + entities.CsePerson.Number;
          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }
        }
      }

ReadEach1:
      ;
    }

ReadEach2:

    // *****************************************************************************
    // Print Error Count
    // *******************************************************************
    UseCabTextnum5();
    local.EabReportSend.RptDetail = "Error Count . . .. . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *******************************************************************
    // Print Control Counts
    // *******************************************************************
    local.EabReportSend.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // *******************************************************************
    UseCabTextnum1();
    local.EabReportSend.RptDetail = "Read Coll Count. . . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // *******************************************************************
    UseCabTextnum5();
    local.EabReportSend.RptDetail = "Error Count . . .. . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // *******************************************************************
    UseCabTextnum3();
    local.EabReportSend.RptDetail = "Delete Disb Coll Count. . . . . . .:" + local
      .WorkArea.Text9;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // *******************************************************************
    local.EabReportSend.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // *******************************************************************
    UseCabTextnum4();
    local.EabReportSend.RptDetail = "Delete Disb Count  . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // *******************************************************************
    UseCabTextnum2();
    local.EabReportSend.RptDetail = "Delete Disb Stat Hst Count.  . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport1();

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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport4()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabTextnum1()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.ReadCollCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum2()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.DeleteDisbStatHstCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum3()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.DeleteDisbCollCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum4()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.DeleteDisbCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum5()
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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void DeleteDisbCollection()
  {
    Update("DeleteDisbCollection",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.DisbCollection.CpaType);
        db.SetString(command, "cspNumber", entities.DisbCollection.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbCollection.SystemGeneratedIdentifier);
      });
  }

  private void DeleteDisbursement()
  {
    Update("DeleteDisbursement",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Disbursement.CpaType);
        db.SetString(command, "cspNumber", entities.Disbursement.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.Disbursement.SystemGeneratedIdentifier);
      });
  }

  private void DeleteDisbursementStatusHistory()
  {
    Update("DeleteDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbsGeneratedId",
          entities.DisbursementStatusHistory.DbsGeneratedId);
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementStatusHistory.DtrGeneratedId);
        db.SetString(
          command, "cspNumber", entities.DisbursementStatusHistory.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementStatusHistory.CpaType);
        db.SetInt32(
          command, "disbStatHistId",
          entities.DisbursementStatusHistory.SystemGeneratedIdentifier);
      });
  }

  private void DeleteDisbursementTransactionRln()
  {
    Update("DeleteDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranRlnId",
          entities.DisbursementTransactionRln.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dnrGeneratedId",
          entities.DisbursementTransactionRln.DnrGeneratedId);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransactionRln.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementTransactionRln.CpaType);
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementTransactionRln.DtrGeneratedId);
        db.SetString(
          command, "cspPNumber",
          entities.DisbursementTransactionRln.CspPNumber);
        db.SetString(
          command, "cpaPType", entities.DisbursementTransactionRln.CpaPType);
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.DisbursementTransactionRln.DtrPGeneratedId);
      });
  }

  private IEnumerable<bool> ReadCollectionDisbCollectionCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.Collection.Populated = false;
    entities.DisbCollection.Populated = false;

    return ReadEach("ReadCollectionDisbCollectionCsePerson",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst", local.AugDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DisbCollection.ColId = db.GetNullableInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.DisbCollection.CrtId = db.GetNullableInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.DisbCollection.CstId = db.GetNullableInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.DisbCollection.CrvId = db.GetNullableInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.DisbCollection.CrdId = db.GetNullableInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.DisbCollection.ObgId = db.GetNullableInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.DisbCollection.CspNumberDisb = db.GetNullableString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.DisbCollection.CpaTypeDisb = db.GetNullableString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.DisbCollection.OtrId = db.GetNullableInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.DisbCollection.OtrTypeDisb = db.GetNullableString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.DisbCollection.OtyId = db.GetNullableInt32(reader, 11);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 12);
        entities.Collection.Amount = db.GetDecimal(reader, 13);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 14);
        entities.DisbCollection.CpaType = db.GetString(reader, 15);
        entities.DisbCollection.CspNumber = db.GetString(reader, 16);
        entities.CsePerson.Number = db.GetString(reader, 16);
        entities.DisbCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 17);
        entities.DisbCollection.Type1 = db.GetString(reader, 18);
        entities.DisbCollection.Amount = db.GetDecimal(reader, 19);
        entities.DisbCollection.CreatedTimestamp = db.GetDateTime(reader, 20);
        entities.DisbCollection.ReferenceNumber =
          db.GetNullableString(reader, 21);
        entities.CsePerson.Populated = true;
        entities.Collection.Populated = true;
        entities.DisbCollection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadDisbCollectionDisbursementPaymentRequestDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ForReadOnlyDisbCollection.Populated = false;
    entities.ForReadOnlyDisbursement.Populated = false;
    entities.ForReadOnlyDisbursementTransactionRln.Populated = false;
    entities.PaymentRequest.Populated = false;

    return ReadEach(
      "ReadDisbCollectionDisbursementPaymentRequestDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId", entities.Collection.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "otyId", entities.Collection.OtyId);
        db.SetNullableInt32(command, "obgId", entities.Collection.ObgId);
        db.SetNullableString(
          command, "cspNumberDisb", entities.Collection.CspNumber);
        db.
          SetNullableString(command, "cpaTypeDisb", entities.Collection.CpaType);
          
        db.SetNullableInt32(command, "otrId", entities.Collection.OtrId);
        db.
          SetNullableString(command, "otrTypeDisb", entities.Collection.OtrType);
          
        db.SetNullableInt32(command, "crtId", entities.Collection.CrtType);
        db.SetNullableInt32(command, "cstId", entities.Collection.CstId);
        db.SetNullableInt32(command, "crvId", entities.Collection.CrvId);
        db.SetNullableInt32(command, "crdId", entities.Collection.CrdId);
      },
      (db, reader) =>
      {
        entities.ForReadOnlyDisbCollection.CpaType = db.GetString(reader, 0);
        entities.ForReadOnlyDisbursementTransactionRln.CpaPType =
          db.GetString(reader, 0);
        entities.ForReadOnlyDisbCollection.CspNumber = db.GetString(reader, 1);
        entities.ForReadOnlyDisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 1);
        entities.ForReadOnlyDisbCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ForReadOnlyDisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 2);
        entities.ForReadOnlyDisbCollection.OtyId =
          db.GetNullableInt32(reader, 3);
        entities.ForReadOnlyDisbCollection.OtrTypeDisb =
          db.GetNullableString(reader, 4);
        entities.ForReadOnlyDisbCollection.OtrId =
          db.GetNullableInt32(reader, 5);
        entities.ForReadOnlyDisbCollection.CpaTypeDisb =
          db.GetNullableString(reader, 6);
        entities.ForReadOnlyDisbCollection.CspNumberDisb =
          db.GetNullableString(reader, 7);
        entities.ForReadOnlyDisbCollection.ObgId =
          db.GetNullableInt32(reader, 8);
        entities.ForReadOnlyDisbCollection.CrdId =
          db.GetNullableInt32(reader, 9);
        entities.ForReadOnlyDisbCollection.CrvId =
          db.GetNullableInt32(reader, 10);
        entities.ForReadOnlyDisbCollection.CstId =
          db.GetNullableInt32(reader, 11);
        entities.ForReadOnlyDisbCollection.CrtId =
          db.GetNullableInt32(reader, 12);
        entities.ForReadOnlyDisbCollection.ColId =
          db.GetNullableInt32(reader, 13);
        entities.ForReadOnlyDisbursement.CpaType = db.GetString(reader, 14);
        entities.ForReadOnlyDisbursementTransactionRln.CpaType =
          db.GetString(reader, 14);
        entities.ForReadOnlyDisbursement.CspNumber = db.GetString(reader, 15);
        entities.ForReadOnlyDisbursementTransactionRln.CspNumber =
          db.GetString(reader, 15);
        entities.ForReadOnlyDisbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.ForReadOnlyDisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 16);
        entities.ForReadOnlyDisbursement.Amount = db.GetDecimal(reader, 17);
        entities.ForReadOnlyDisbursement.PrqGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.ForReadOnlyDisbursement.ReferenceNumber =
          db.GetNullableString(reader, 19);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 20);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 21);
        entities.PaymentRequest.Type1 = db.GetString(reader, 22);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 23);
        entities.ForReadOnlyDisbursementTransactionRln.
          SystemGeneratedIdentifier = db.GetInt32(reader, 24);
        entities.ForReadOnlyDisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 25);
        entities.ForReadOnlyDisbCollection.Populated = true;
        entities.ForReadOnlyDisbursement.Populated = true;
        entities.ForReadOnlyDisbursementTransactionRln.Populated = true;
        entities.PaymentRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadDisbursementDisbursementTransactionRlnDisbursementType()
  {
    System.Diagnostics.Debug.Assert(entities.DisbCollection.Populated);
    entities.DisbursementType.Populated = false;
    entities.Disbursement.Populated = false;
    entities.DisbursementTransactionRln.Populated = false;

    return ReadEach(
      "ReadDisbursementDisbursementTransactionRlnDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.DisbCollection.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.DisbCollection.CpaType);
        db.SetString(command, "cspPNumber", entities.DisbCollection.CspNumber);
      },
      (db, reader) =>
      {
        entities.Disbursement.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransactionRln.CpaType = db.GetString(reader, 0);
        entities.Disbursement.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransactionRln.CspNumber = db.GetString(reader, 1);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.Disbursement.Type1 = db.GetString(reader, 3);
        entities.Disbursement.Amount = db.GetDecimal(reader, 4);
        entities.Disbursement.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Disbursement.DbtGeneratedId = db.GetNullableInt32(reader, 6);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Disbursement.ReferenceNumber = db.GetNullableString(reader, 7);
        entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.DisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 9);
        entities.DisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 10);
        entities.DisbursementTransactionRln.CpaPType = db.GetString(reader, 11);
        entities.DisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 12);
        entities.DisbursementType.Code = db.GetString(reader, 13);
        entities.DisbursementType.Populated = true;
        entities.Disbursement.Populated = true;
        entities.DisbursementTransactionRln.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementStatusHistoryDisbursementStatus()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);
    entities.DisbursementStatusHistory.Populated = false;
    entities.DisbursementStatus.Populated = false;

    return ReadEach("ReadDisbursementStatusHistoryDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.Disbursement.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Disbursement.CspNumber);
        db.SetString(command, "cpaType", entities.Disbursement.CpaType);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatus.Code = db.GetString(reader, 7);
        entities.DisbursementStatusHistory.Populated = true;
        entities.DisbursementStatus.Populated = true;

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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of UnitOfWorkCnt.
    /// </summary>
    [JsonPropertyName("unitOfWorkCnt")]
    public NumericWorkSet UnitOfWorkCnt
    {
      get => unitOfWorkCnt ??= new();
      set => unitOfWorkCnt = value;
    }

    /// <summary>
    /// A value of RptCollCreateDate.
    /// </summary>
    [JsonPropertyName("rptCollCreateDate")]
    public DateWorkAttributes RptCollCreateDate
    {
      get => rptCollCreateDate ??= new();
      set => rptCollCreateDate = value;
    }

    /// <summary>
    /// A value of AugDate.
    /// </summary>
    [JsonPropertyName("augDate")]
    public DateWorkArea AugDate
    {
      get => augDate ??= new();
      set => augDate = value;
    }

    /// <summary>
    /// A value of RptDisbRefNbr.
    /// </summary>
    [JsonPropertyName("rptDisbRefNbr")]
    public WorkArea RptDisbRefNbr
    {
      get => rptDisbRefNbr ??= new();
      set => rptDisbRefNbr = value;
    }

    /// <summary>
    /// A value of RptDisbStatusHstId.
    /// </summary>
    [JsonPropertyName("rptDisbStatusHstId")]
    public WorkArea RptDisbStatusHstId
    {
      get => rptDisbStatusHstId ??= new();
      set => rptDisbStatusHstId = value;
    }

    /// <summary>
    /// A value of RptDisbCollAmt.
    /// </summary>
    [JsonPropertyName("rptDisbCollAmt")]
    public WorkArea RptDisbCollAmt
    {
      get => rptDisbCollAmt ??= new();
      set => rptDisbCollAmt = value;
    }

    /// <summary>
    /// A value of RptDiscDate.
    /// </summary>
    [JsonPropertyName("rptDiscDate")]
    public DateWorkAttributes RptDiscDate
    {
      get => rptDiscDate ??= new();
      set => rptDiscDate = value;
    }

    /// <summary>
    /// A value of RptEffectDate.
    /// </summary>
    [JsonPropertyName("rptEffectDate")]
    public DateWorkAttributes RptEffectDate
    {
      get => rptEffectDate ??= new();
      set => rptEffectDate = value;
    }

    /// <summary>
    /// A value of RptDateCreated.
    /// </summary>
    [JsonPropertyName("rptDateCreated")]
    public DateWorkAttributes RptDateCreated
    {
      get => rptDateCreated ??= new();
      set => rptDateCreated = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public DateWorkAttributes Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public DateWorkAttributes Day
    {
      get => day ??= new();
      set => day = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public DateWorkAttributes Year
    {
      get => year ??= new();
      set => year = value;
    }

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
    /// A value of RptPayRqstId.
    /// </summary>
    [JsonPropertyName("rptPayRqstId")]
    public WorkArea RptPayRqstId
    {
      get => rptPayRqstId ??= new();
      set => rptPayRqstId = value;
    }

    /// <summary>
    /// A value of RptDisbAmt.
    /// </summary>
    [JsonPropertyName("rptDisbAmt")]
    public WorkArea RptDisbAmt
    {
      get => rptDisbAmt ??= new();
      set => rptDisbAmt = value;
    }

    /// <summary>
    /// A value of RptDisbId.
    /// </summary>
    [JsonPropertyName("rptDisbId")]
    public WorkArea RptDisbId
    {
      get => rptDisbId ??= new();
      set => rptDisbId = value;
    }

    /// <summary>
    /// A value of RptDisbCollId.
    /// </summary>
    [JsonPropertyName("rptDisbCollId")]
    public WorkArea RptDisbCollId
    {
      get => rptDisbCollId ??= new();
      set => rptDisbCollId = value;
    }

    /// <summary>
    /// A value of RptCollAmt.
    /// </summary>
    [JsonPropertyName("rptCollAmt")]
    public WorkArea RptCollAmt
    {
      get => rptCollAmt ??= new();
      set => rptCollAmt = value;
    }

    /// <summary>
    /// A value of RptCollId.
    /// </summary>
    [JsonPropertyName("rptCollId")]
    public WorkArea RptCollId
    {
      get => rptCollId ??= new();
      set => rptCollId = value;
    }

    /// <summary>
    /// A value of RptCollDate.
    /// </summary>
    [JsonPropertyName("rptCollDate")]
    public DateWorkAttributes RptCollDate
    {
      get => rptCollDate ??= new();
      set => rptCollDate = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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

    /// <summary>
    /// A value of ProcessUpdates.
    /// </summary>
    [JsonPropertyName("processUpdates")]
    public Common ProcessUpdates
    {
      get => processUpdates ??= new();
      set => processUpdates = value;
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
    /// A value of ReadCollCnt.
    /// </summary>
    [JsonPropertyName("readCollCnt")]
    public Common ReadCollCnt
    {
      get => readCollCnt ??= new();
      set => readCollCnt = value;
    }

    /// <summary>
    /// A value of DeleteDisbStatHstCnt.
    /// </summary>
    [JsonPropertyName("deleteDisbStatHstCnt")]
    public Common DeleteDisbStatHstCnt
    {
      get => deleteDisbStatHstCnt ??= new();
      set => deleteDisbStatHstCnt = value;
    }

    /// <summary>
    /// A value of DeleteDisbCollCnt.
    /// </summary>
    [JsonPropertyName("deleteDisbCollCnt")]
    public Common DeleteDisbCollCnt
    {
      get => deleteDisbCollCnt ??= new();
      set => deleteDisbCollCnt = value;
    }

    /// <summary>
    /// A value of DeleteDisbCnt.
    /// </summary>
    [JsonPropertyName("deleteDisbCnt")]
    public Common DeleteDisbCnt
    {
      get => deleteDisbCnt ??= new();
      set => deleteDisbCnt = value;
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
    /// A value of ProgramName.
    /// </summary>
    [JsonPropertyName("programName")]
    public ProgramCheckpointRestart ProgramName
    {
      get => programName ??= new();
      set => programName = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private DateWorkArea maxDate;
    private External external;
    private NumericWorkSet unitOfWorkCnt;
    private DateWorkAttributes rptCollCreateDate;
    private DateWorkArea augDate;
    private WorkArea rptDisbRefNbr;
    private WorkArea rptDisbStatusHstId;
    private WorkArea rptDisbCollAmt;
    private DateWorkAttributes rptDiscDate;
    private DateWorkAttributes rptEffectDate;
    private DateWorkAttributes rptDateCreated;
    private DateWorkAttributes month;
    private DateWorkAttributes day;
    private DateWorkAttributes year;
    private EabConvertNumeric2 eabConvertNumeric;
    private WorkArea rptPayRqstId;
    private WorkArea rptDisbAmt;
    private WorkArea rptDisbId;
    private WorkArea rptDisbCollId;
    private WorkArea rptCollAmt;
    private WorkArea rptCollId;
    private DateWorkAttributes rptCollDate;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private Common processUpdates;
    private WorkArea workArea;
    private Common readCollCnt;
    private Common deleteDisbStatHstCnt;
    private Common deleteDisbCollCnt;
    private Common deleteDisbCnt;
    private Common errorCnt;
    private ProgramCheckpointRestart programName;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of ForReadOnlyDisbCollection.
    /// </summary>
    [JsonPropertyName("forReadOnlyDisbCollection")]
    public DisbursementTransaction ForReadOnlyDisbCollection
    {
      get => forReadOnlyDisbCollection ??= new();
      set => forReadOnlyDisbCollection = value;
    }

    /// <summary>
    /// A value of ForReadOnlyDisbursement.
    /// </summary>
    [JsonPropertyName("forReadOnlyDisbursement")]
    public DisbursementTransaction ForReadOnlyDisbursement
    {
      get => forReadOnlyDisbursement ??= new();
      set => forReadOnlyDisbursement = value;
    }

    /// <summary>
    /// A value of ForReadOnlyDisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("forReadOnlyDisbursementTransactionRln")]
    public DisbursementTransactionRln ForReadOnlyDisbursementTransactionRln
    {
      get => forReadOnlyDisbursementTransactionRln ??= new();
      set => forReadOnlyDisbursementTransactionRln = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    /// <summary>
    /// A value of DisbCollection.
    /// </summary>
    [JsonPropertyName("disbCollection")]
    public DisbursementTransaction DisbCollection
    {
      get => disbCollection ??= new();
      set => disbCollection = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private DisbursementType disbursementType;
    private DisbursementTransaction forReadOnlyDisbCollection;
    private DisbursementTransaction forReadOnlyDisbursement;
    private DisbursementTransactionRln forReadOnlyDisbursementTransactionRln;
    private CsePerson csePerson;
    private Collection collection;
    private PaymentRequest paymentRequest;
    private DisbursementTransaction disbursement;
    private DisbursementTransaction disbCollection;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus disbursementStatus;
    private DisbursementTransactionRln disbursementTransactionRln;
    private CsePersonAccount obligee;
  }
#endregion
}
