// Program: FN_B609_ATT_CNTR_COLL_DET_RPT, ID: 372957859, model: 746.
// Short name: SWEF609B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B609_ATT_CNTR_COLL_DET_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB609AttCntrCollDetRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B609_ATT_CNTR_COLL_DET_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB609AttCntrCollDetRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB609AttCntrCollDetRpt.
  /// </summary>
  public FnB609AttCntrCollDetRpt(IContext context, Import import, Export export):
    
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
    //           M A I N T E N A N C E   L O G
    // Date	    Developer		Description
    // 07-28-1999  SWSRKEH             Initial Development
    // END of   M A I N T E N A N C E   L O G
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramControlTotal.SystemGeneratedIdentifier = 0;

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    // <=========== Extract the run parameters ========>
    // <=========== Extract the type of report (Both/Detail/Summary) ========>
    local.PosCnt.Count = Find(local.ProgramProcessingInfo.ParameterList, "R=");
    local.PosCnt.Count += 2;

    // : Get the report type.  It will be 'S' for Summary, and 'D' for Detail.  
    // Default is 'D'.
    local.RptParm.Text1 =
      Substring(local.ProgramProcessingInfo.ParameterList, local.PosCnt.Count, 1);
      

    if (AsChar(local.RptParm.Text1) == 'S' || AsChar(local.RptParm.Text1) == 'D'
      )
    {
    }
    else
    {
      local.RptParm.Text1 = "D";
    }

    // <=========== Extract the SP's USERID if present) ========>
    local.PosCnt.Count = Find(local.ProgramProcessingInfo.ParameterList, "SP=");

    if (local.PosCnt.Count != 0)
    {
      local.PosCnt.Count += 3;
      local.SelectSp.UserId =
        Substring(local.ProgramProcessingInfo.ParameterList, local.PosCnt.Count,
        8);

      if (!IsEmpty(local.SelectSp.UserId))
      {
        if (!ReadServiceProvider())
        {
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }
      }
    }

    // <=========== GET REPORTING DATES ========>
    // To facilitate testing, the following logic sets the local current date 
    // work area date to either:
    // 1. If the processing info date is blank, set the BOM (Begining of Month) 
    // DATE
    // to the system current date month - 1 month and set day to the 1st, set 
    // EOM to last
    // day of the reporting month.
    // 2. If the processing info date is max date (2099-12-31), same as above
    // 3. Otherwise, use the program processing info date to calculate the BOM 
    // and EOM.
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Initialized.Date))
    {
      local.Current.Date = Now().Date;
    }
    else if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Max.Date))
    {
      local.Current.Date = Now().Date;
    }
    else
    {
      local.Current.Date = local.ProgramProcessingInfo.ProcessDate;
    }

    UseCabDetermineReportingDates();

    // <=========== end of calculating REPORTING DATES ========>
    // *****************************************************************
    // * Setup of batch error handling
    // 
    // *
    // *****************************************************************
    // *****************************************************************
    // * Open the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.Current.Date;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // : The following code gets a parameter for the report type from the JCL.  
    // Since this parameter is also input via the program processing table, this
    // code has been commented out.  It was not deleted, in the event of a
    // requirement that report type be provided through JCL.
    // <=========== END of Reading REPORTING Parameters from JCL ========>
    // **** OPEN sorted EXTRACT DSN (DDNAME = EXTT162S) ****
    local.Send.Parm1 = "OF";
    local.Send.Parm2 = "";
    UseEabB609FileReader3();

    if (!IsEmpty(local.Return1.Parm1))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the input file. Status =";
      local.NeededToWrite.RptDetail = TrimEnd(local.NeededToWrite.RptDetail) + " " +
        local.Return1.Parm1;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **** OPEN Report DSN (DDNAME = RPTSR162) ****
    local.Send.Parm1 = "OF";
    local.Send.Parm2 = "";
    UseEabB609ReportWriter2();

    if (!IsEmpty(local.Return1.Parm1))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Report file.  Status =";
      local.NeededToWrite.RptDetail = TrimEnd(local.NeededToWrite.RptDetail) + " " +
        local.Return1.Parm1;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **** END OF REPORT DSN OPEN PROCESS ****
    // <=============================================>
    // <======= Beginning Main Program Logic ========>
    // <=============================================>
    // **** Read the first record ****
    // **** Use EAB File Reader ****
    // **** Read sorted EXTRACT DSN (DDNAME = EXTT162S) ****
    // **** Set type of report to output ****
    local.Send.Parm1 = "GR";

    do
    {
      UseEabB609FileReader2();

      if (!IsEmpty(local.Return1.Parm1))
      {
        // *****************************************************************
        // * Write a line to the ERROR RPT.
        // 
        // *
        // *****************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered reading the input file. Status =";
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " " + local.Return1.Parm1;
        UseCabErrorReport2();

        if (Equal(local.Return1.Parm1, "EF"))
        {
          return;
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (IsEmpty(local.SelectSp.UserId))
      {
        break;
      }
    }
    while(!Equal(local.ServiceProvider.UserId, local.SelectSp.UserId));

    // **** Format Service Provider name ****
    local.Sp.Text30 = local.ServiceProvider.LastName + local
      .ServiceProvider.FirstName + local.ServiceProvider.MiddleInitial;
    local.PrevSp.Text30 = local.Sp.Text30;

    // **** Set '(' literals for negative (adjusted) amounts  ****
    switch(AsChar(local.Collection.AdjustedInd))
    {
      case 'N':
        local.DetLtPrn.Text1 = "";
        local.DetRtPrn.Text1 = "";
        local.TotPrgmGrpCollection.Amount = local.Collection.Amount;

        break;
      case 'Y':
        local.DetLtPrn.Text1 = "(";
        local.DetRtPrn.Text1 = ")";
        local.TotPrgmGrpCollection.Amount = 0 - local.Collection.Amount;

        break;
      default:
        break;
    }

    switch(AsChar(local.ProgGrp.Text1))
    {
      case 'A':
        local.BrkPrmGrp.Text10 = "TAF";

        break;
      case 'B':
        local.BrkPrmGrp.Text10 = "NonTAF";

        break;
      case 'C':
        local.BrkPrmGrp.Text10 = "State Only";

        break;
      default:
        break;
    }

    switch(AsChar(local.KorO.Text1))
    {
      case 'K':
        local.BrkKsOth.Text10 = "Kansas";

        break;
      case 'O':
        local.BrkKsOth.Text10 = "InterState";

        break;
      default:
        break;
    }

    local.PrevCase.Number = local.Case1.Number;

    // **** Roll current data to prev data ****
    local.PrevCase.Number = local.Case1.Number;
    local.PrevServiceProvider.UserId = local.ServiceProvider.UserId;
    local.BrkKsOthPrev.Text10 = local.BrkKsOth.Text10;
    local.BrkPrmGrpPrev.Text10 = local.BrkPrmGrp.Text10;
    local.PrgTotLtPrn.Text1 = local.DetLtPrn.Text1;
    local.PrgTotRtPrn.Text1 = local.DetRtPrn.Text1;
    local.TotPrgmGrpCommon.Count = 1;
    local.SpGtotCommon.Count = 1;
    local.SpGtotCollection.Amount = local.TotPrgmGrpCollection.Amount;
    local.SpGtotLtPrn.Text1 = local.DetLtPrn.Text1;
    local.SpGtotRtPrn.Text1 = local.DetRtPrn.Text1;

    switch(TrimEnd(local.BrkPrmGrp.Text10))
    {
      case "TAF":
        switch(TrimEnd(local.BrkKsOth.Text10))
        {
          case "Kansas":
            local.SpKsTaf.Amount = local.TotPrgmGrpCollection.Amount;
            local.SpKsTafCaseCnt.Count = 1;
            local.SpKsTafLtPrn.Text1 = local.DetLtPrn.Text1;
            local.SpKsTafRtPrn.Text1 = local.DetRtPrn.Text1;

            break;
          case "InterState":
            local.SpOthTaf.Amount = local.TotPrgmGrpCollection.Amount;
            local.SpOthTafCaseCnt.Count = 1;
            local.SpOthTafLtPrn.Text1 = local.DetLtPrn.Text1;
            local.SpOthTafRtPrn.Text1 = local.DetRtPrn.Text1;

            break;
          default:
            break;
        }

        break;
      case "NonTAF":
        switch(TrimEnd(local.BrkKsOth.Text10))
        {
          case "Kansas":
            local.SpKsNtaf.Amount = local.TotPrgmGrpCollection.Amount;
            local.SpKsNtafCaseCnt.Count = 1;
            local.SpKsNtafLtPrn.Text1 = local.DetLtPrn.Text1;
            local.SpKsNtafRtPrn.Text1 = local.DetRtPrn.Text1;

            break;
          case "InterState":
            local.SpOthNtaf.Amount = local.TotPrgmGrpCollection.Amount;
            local.SpOthNtafCaseCnt.Count = 1;
            local.SpOthNtafLtPrn.Text1 = local.DetLtPrn.Text1;
            local.SpOthNtafRtPrn.Text1 = local.DetRtPrn.Text1;

            break;
          default:
            break;
        }

        break;
      case "State Only":
        local.SpKsSo.Amount = local.TotPrgmGrpCollection.Amount;
        local.SpKsSoCaseCnt.Count = 1;
        local.SpKsSoLtPrn.Text1 = local.DetLtPrn.Text1;
        local.SpKsSoRtPrn.Text1 = local.DetRtPrn.Text1;

        break;
      default:
        break;
    }

    // <=============================================>
    // <===============  end of setup ===============>
    // <=============================================>
    do
    {
      // **** Use EAB Report Writer ****
      // : Parm2 will contain the report type ("D"etail or "S"ummary).  If "S",
      // Report Composer will generate the total lines, and suppress detail 
      // lines.
      local.Send.Parm2 = local.RptParm.Text1;
      UseEabB609ReportWriter1();

      if (!IsEmpty(local.Return1.Parm1))
      {
        // *****************************************************************
        // * Write a line to the ERROR RPT.
        // 
        // *
        // *****************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing to the report file.  Status =";
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " " + local.Return1.Parm1;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // **** Use EAB File Reader ****
      // **** Read sorted EXTRACT DSN (DDNAME = EXTT162S) ****
      local.Send.Parm1 = "GR";
      local.Send.Parm2 = "";
      UseEabB609FileReader2();

      if (!IsEmpty(local.Return1.Parm1))
      {
        if (Equal(local.Return1.Parm1, "EF"))
        {
          continue;
        }

        // *****************************************************************
        // * Write a line to the ERROR RPT.
        // 
        // *
        // *****************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered opening the output file.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (!IsEmpty(local.SelectSp.UserId) && !
        Equal(local.SelectSp.UserId, local.ServiceProvider.UserId))
      {
        break;
      }

      // **** Set Kansas / Interstate Control break ****
      switch(AsChar(local.KorO.Text1))
      {
        case 'K':
          local.BrkKsOth.Text10 = "Kansas";

          break;
        case 'O':
          local.BrkKsOth.Text10 = "InterState";

          break;
        default:
          continue;
      }

      // **** Build SP formatted name ****
      local.Sp.Text30 = local.ServiceProvider.LastName + local
        .ServiceProvider.FirstName + local.ServiceProvider.MiddleInitial;

      // **** Expands Codes for NonAdjusted / Adjusted  ****
      switch(AsChar(local.Collection.AdjustedInd))
      {
        case 'N':
          local.DetLtPrn.Text1 = "";
          local.DetRtPrn.Text1 = "";

          break;
        case 'Y':
          local.DetLtPrn.Text1 = "(";
          local.DetRtPrn.Text1 = ")";

          break;
        default:
          continue;
      }

      // **** Expands the Codes & Set program control breaks ****
      switch(AsChar(local.ProgGrp.Text1))
      {
        case 'A':
          local.BrkPrmGrp.Text10 = "TAF";

          break;
        case 'B':
          local.BrkPrmGrp.Text10 = "NonTAF";

          break;
        case 'C':
          local.BrkPrmGrp.Text10 = "State Only";

          break;
        default:
          continue;
      }

      // **** Check for control breaks and set totals ****
      if (!Equal(local.PrevServiceProvider.UserId, local.ServiceProvider.UserId) ||
        !Equal(local.Sp.Text30, local.PrevSp.Text30))
      {
        // ***** Total for Service Provider ****
        local.SpKsNtaf.Amount = 0;
        local.SpKsNtafCaseCnt.Count = 0;
        local.SpKsSo.Amount = 0;
        local.SpKsSoCaseCnt.Count = 0;
        local.SpKsTaf.Amount = 0;
        local.SpKsTafCaseCnt.Count = 0;
        local.SpOthNtaf.Amount = 0;
        local.SpOthNtafCaseCnt.Count = 0;
        local.SpOthTaf.Amount = 0;
        local.SpOthTafCaseCnt.Count = 0;
        local.PrgTotLtPrn.Text1 = "";
        local.PrgTotRtPrn.Text1 = "";
        local.SpKsTafLtPrn.Text1 = "";
        local.SpKsTafRtPrn.Text1 = "";
        local.SpOthTafLtPrn.Text1 = "";
        local.SpOthTafRtPrn.Text1 = "";
        local.SpKsNtafLtPrn.Text1 = "";
        local.SpKsNtafRtPrn.Text1 = "";
        local.SpOthNtafLtPrn.Text1 = "";
        local.SpOthNtafRtPrn.Text1 = "";
        local.SpKsSoLtPrn.Text1 = "";
        local.SpKsSoRtPrn.Text1 = "";
        local.SpGtotLtPrn.Text1 = "";
        local.SpGtotRtPrn.Text1 = "";
        local.PrevServiceProvider.UserId = local.ServiceProvider.UserId;
        local.BrkKsOthPrev.Text10 = "";
        local.PrevSp.Text30 = local.Sp.Text30;
      }

      if (!Equal(local.BrkKsOthPrev.Text10, local.BrkKsOth.Text10) || !
        Equal(local.BrkPrmGrpPrev.Text10, local.BrkPrmGrp.Text10))
      {
        local.TotPrgmGrpCollection.Amount = 0;
        local.TotPrgmGrpCommon.Count = 0;
        local.PrevCase.Number = "";
        local.BrkKsOthPrev.Text10 = local.BrkKsOth.Text10;
        local.BrkPrmGrpPrev.Text10 = local.BrkPrmGrp.Text10;
      }

      // **** Count the Unique Cases ****
      if (!Equal(local.Case1.Number, local.PrevCase.Number))
      {
        local.CaseCnt.Count = 1;
        local.PrevCase.Number = local.Case1.Number;
      }
      else
      {
        local.CaseCnt.Count = 0;
      }

      switch(AsChar(local.Collection.AdjustedInd))
      {
        case 'N':
          local.TotPrgmGrpCollection.Amount += local.Collection.Amount;

          break;
        case 'Y':
          local.TotPrgmGrpCollection.Amount -= local.Collection.Amount;

          break;
        default:
          break;
      }

      local.TotPrgmGrpCommon.Count += local.CaseCnt.Count;

      if (local.TotPrgmGrpCollection.Amount >= 0)
      {
        local.PrgTotLtPrn.Text1 = "";
        local.PrgTotRtPrn.Text1 = "";
      }
      else
      {
        local.PrgTotLtPrn.Text1 = "(";
        local.PrgTotRtPrn.Text1 = ")";
      }

      // **** ACCUM Report Totals ****
      switch(TrimEnd(local.BrkPrmGrp.Text10))
      {
        case "TAF":
          switch(TrimEnd(local.BrkKsOth.Text10))
          {
            case "Kansas":
              switch(AsChar(local.Collection.AdjustedInd))
              {
                case 'N':
                  local.SpKsTaf.Amount += local.Collection.Amount;

                  break;
                case 'Y':
                  local.SpKsTaf.Amount -= local.Collection.Amount;

                  break;
                default:
                  break;
              }

              local.SpKsTafCaseCnt.Count += local.CaseCnt.Count;

              if (local.SpKsTaf.Amount >= 0)
              {
                local.SpKsTafLtPrn.Text1 = "";
                local.SpKsTafRtPrn.Text1 = "";
              }
              else
              {
                local.SpKsTafLtPrn.Text1 = "(";
                local.SpKsTafRtPrn.Text1 = ")";
              }

              break;
            case "InterState":
              switch(AsChar(local.Collection.AdjustedInd))
              {
                case 'N':
                  local.SpOthTaf.Amount += local.Collection.Amount;

                  break;
                case 'Y':
                  local.SpOthTaf.Amount -= local.Collection.Amount;

                  break;
                default:
                  break;
              }

              local.SpOthTafCaseCnt.Count += local.CaseCnt.Count;

              if (local.SpOthTaf.Amount >= 0)
              {
                local.SpOthTafLtPrn.Text1 = "";
                local.SpOthTafRtPrn.Text1 = "";
              }
              else
              {
                local.SpOthTafLtPrn.Text1 = "(";
                local.SpOthTafRtPrn.Text1 = ")";
              }

              break;
            default:
              break;
          }

          break;
        case "NonTAF":
          switch(TrimEnd(local.BrkKsOth.Text10))
          {
            case "Kansas":
              switch(AsChar(local.Collection.AdjustedInd))
              {
                case 'N':
                  local.SpKsNtaf.Amount += local.Collection.Amount;

                  break;
                case 'Y':
                  local.SpKsNtaf.Amount -= local.Collection.Amount;

                  break;
                default:
                  break;
              }

              local.SpKsNtafCaseCnt.Count += local.CaseCnt.Count;

              if (local.SpKsNtaf.Amount >= 0)
              {
                local.SpKsNtafLtPrn.Text1 = "";
                local.SpKsNtafRtPrn.Text1 = "";
              }
              else
              {
                local.SpKsNtafLtPrn.Text1 = "(";
                local.SpKsNtafRtPrn.Text1 = ")";
              }

              break;
            case "InterState":
              switch(AsChar(local.Collection.AdjustedInd))
              {
                case 'N':
                  local.SpOthNtaf.Amount += local.Collection.Amount;

                  break;
                case 'Y':
                  local.SpOthNtaf.Amount -= local.Collection.Amount;

                  break;
                default:
                  break;
              }

              local.SpOthNtafCaseCnt.Count += local.CaseCnt.Count;

              if (local.SpOthNtaf.Amount >= 0)
              {
                local.SpOthNtafLtPrn.Text1 = "";
                local.SpOthNtafRtPrn.Text1 = "";
              }
              else
              {
                local.SpOthNtafLtPrn.Text1 = "(";
                local.SpOthNtafRtPrn.Text1 = ")";
              }

              break;
            default:
              break;
          }

          break;
        case "State Only":
          switch(AsChar(local.Collection.AdjustedInd))
          {
            case 'N':
              local.SpKsSo.Amount += local.Collection.Amount;

              break;
            case 'Y':
              local.SpKsSo.Amount -= local.Collection.Amount;

              break;
            default:
              break;
          }

          local.SpKsSoCaseCnt.Count += local.CaseCnt.Count;

          if (local.SpKsSo.Amount >= 0)
          {
            local.SpKsSoLtPrn.Text1 = "";
            local.SpKsSoRtPrn.Text1 = "";
          }
          else
          {
            local.SpKsSoLtPrn.Text1 = "(";
            local.SpKsSoRtPrn.Text1 = ")";
          }

          break;
        default:
          break;
      }

      local.SpGtotCollection.Amount = local.SpOthNtaf.Amount + local
        .SpOthTaf.Amount + local.SpKsNtaf.Amount + local.SpKsSo.Amount + local
        .SpKsTaf.Amount;
      local.SpGtotCommon.Count = local.SpOthNtafCaseCnt.Count + local
        .SpOthTafCaseCnt.Count + local.SpKsNtafCaseCnt.Count + local
        .SpKsSoCaseCnt.Count + local.SpKsTafCaseCnt.Count;

      if (local.SpGtotCollection.Amount >= 0)
      {
        local.SpGtotLtPrn.Text1 = "";
        local.SpGtotRtPrn.Text1 = "";
      }
      else
      {
        local.SpGtotLtPrn.Text1 = "(";
        local.SpGtotRtPrn.Text1 = ")";
      }
    }
    while(!Equal(local.Return1.Parm1, "EF"));

    // <=============================================>
    // <========= End of Main Program Logic =========>
    // <=============================================>
    // **** START OF REPORT DSN CLOSE PROCESS ****
    // **** Close Report DSN (DDNAME = RPTSR162) ****
    local.Send.Parm1 = "CF";
    local.Send.Parm2 = "";
    UseEabB609ReportWriter1();

    if (!IsEmpty(local.Return1.Parm1))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the report file.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // **** Close the sorted EXTRACT DSN (DDNAME = EXTT162S) ****
    // *****************************************************************
    local.Send.Parm1 = "CF";
    local.Send.Parm2 = "";
    UseEabB609FileReader1();

    if (!IsEmpty(local.Return1.Parm1))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the output file.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **** END OF REPORT DSN CLOSE PROCESS ****
    // *****************************************************************
    // * Close the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.Name = source.Name;
    target.JudicialDistrict = source.JudicialDistrict;
  }

  private void UseCabDetermineReportingDates()
  {
    var useImport = new CabDetermineReportingDates.Import();
    var useExport = new CabDetermineReportingDates.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabDetermineReportingDates.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.Bom, local.Bom);
    MoveDateWorkArea(useExport.Eom, local.Eom);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Initialized.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabB609FileReader1()
  {
    var useImport = new EabB609FileReader.Import();
    var useExport = new EabB609FileReader.Export();

    Call(EabB609FileReader.Execute, useImport, useExport);
  }

  private void UseEabB609FileReader2()
  {
    var useImport = new EabB609FileReader.Import();
    var useExport = new EabB609FileReader.Export();

    useImport.ReportParms.Assign(local.Send);
    useExport.ServiceProvider.Assign(local.ServiceProvider);
    useExport.Ko.Text1 = local.KorO.Text1;
    useExport.Collection.Assign(local.Collection);
    useExport.ProgGrp.Text1 = local.ProgGrp.Text1;
    MoveCsePersonsWorkSet(local.Ap, useExport.Ap);
    MoveCsePersonsWorkSet(local.Ar, useExport.Ar);
    useExport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useExport.Case1.Number = local.Case1.Number;
    useExport.CollectionType.Code = local.CollectionType.Code;
    useExport.LegalReferralAssignment.EffectiveDate =
      local.LegalReferralAssignment.EffectiveDate;
    MoveTribunal(local.Tribunal, useExport.Tribunal);
    useExport.Fips.CountyDescription = local.Fips.CountyDescription;
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabB609FileReader.Execute, useImport, useExport);

    local.ServiceProvider.Assign(useExport.ServiceProvider);
    local.KorO.Text1 = useExport.Ko.Text1;
    local.Collection.Assign(useExport.Collection);
    local.ProgGrp.Text1 = useExport.ProgGrp.Text1;
    MoveCsePersonsWorkSet(useExport.Ap, local.Ap);
    MoveCsePersonsWorkSet(useExport.Ar, local.Ar);
    local.LegalAction.StandardNumber = useExport.LegalAction.StandardNumber;
    local.Case1.Number = useExport.Case1.Number;
    local.CollectionType.Code = useExport.CollectionType.Code;
    local.LegalReferralAssignment.EffectiveDate =
      useExport.LegalReferralAssignment.EffectiveDate;
    MoveTribunal(useExport.Tribunal, local.Tribunal);
    local.Fips.CountyDescription = useExport.Fips.CountyDescription;
    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private void UseEabB609FileReader3()
  {
    var useImport = new EabB609FileReader.Import();
    var useExport = new EabB609FileReader.Export();

    useImport.ReportParms.Assign(local.Send);
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabB609FileReader.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private void UseEabB609ReportWriter1()
  {
    var useImport = new EabB609ReportWriter.Import();
    var useExport = new EabB609ReportWriter.Export();

    useImport.ReportParms.Assign(local.Send);
    useImport.Bom.Date = local.Bom.Date;
    useImport.Eom.Date = local.Eom.Date;
    useImport.ServiceProvider.Assign(local.ServiceProvider);
    MoveCsePersonsWorkSet(local.Ap, useImport.Ap);
    MoveCsePersonsWorkSet(local.Ar, useImport.Ar);
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useImport.Collection.Assign(local.Collection);
    useImport.CollectionType.Code = local.CollectionType.Code;
    useImport.LegalReferralAssignment.EffectiveDate =
      local.LegalReferralAssignment.EffectiveDate;
    useImport.Prev.UserId = local.PrevServiceProvider.UserId;
    useImport.SpGtotLtParn.Text1 = local.SpGtotLtPrn.Text1;
    useImport.SpGtotRtParn.Text1 = local.SpGtotRtPrn.Text1;
    useImport.SpOthTafLtParn.Text1 = local.SpOthTafLtPrn.Text1;
    useImport.SpOthTafRtParn.Text1 = local.SpOthTafRtPrn.Text1;
    useImport.SpOthNtafLtParn.Text1 = local.SpOthNtafLtPrn.Text1;
    useImport.SpOthNtafRtParn.Text1 = local.SpOthNtafRtPrn.Text1;
    useImport.SpKsSoLtParn.Text1 = local.SpKsSoLtPrn.Text1;
    useImport.SpKsSoRtParn.Text1 = local.SpKsSoRtPrn.Text1;
    useImport.SpKsTafLtParn.Text1 = local.SpKsTafLtPrn.Text1;
    useImport.SpKsTafRtParn.Text1 = local.SpKsTafRtPrn.Text1;
    useImport.SpKsNtafLtParn.Text1 = local.SpKsNtafLtPrn.Text1;
    useImport.SpKsNtafRtParn.Text1 = local.SpKsNtafRtPrn.Text1;
    useImport.PrgTotLtParn.Text1 = local.PrgTotLtPrn.Text1;
    useImport.PrgTotRtParn.Text1 = local.PrgTotRtPrn.Text1;
    useImport.DetLtParn.Text1 = local.DetLtPrn.Text1;
    useImport.DetRtParn.Text1 = local.DetRtPrn.Text1;
    useImport.SpKsSoCommon.Count = local.SpKsSoCaseCnt.Count;
    useImport.SpKsSoCollection.Amount = local.SpKsSo.Amount;
    useImport.SpKsNtafCommon.Count = local.SpKsNtafCaseCnt.Count;
    useImport.SpKsNtafCollection.Amount = local.SpKsNtaf.Amount;
    useImport.SpKsTafCommon.Count = local.SpKsTafCaseCnt.Count;
    useImport.SpKsTafCollection.Amount = local.SpKsTaf.Amount;
    useImport.SpGtotCommon.Count = local.SpGtotCommon.Count;
    useImport.SpGtotCollection.Amount = local.SpGtotCollection.Amount;
    useImport.SpOthNtafCommon.Count = local.SpOthNtafCaseCnt.Count;
    useImport.SpOthNtafCollection.Amount = local.SpOthNtaf.Amount;
    useImport.SpOthTafCommon.Count = local.SpOthTafCaseCnt.Count;
    useImport.SpOthTafCollection.Amount = local.SpOthTaf.Amount;
    useImport.PrgTotCommon.Count = local.TotPrgmGrpCommon.Count;
    useImport.PrgTotCollection.Amount = local.TotPrgmGrpCollection.Amount;
    useImport.CtlBrkKsOth.Text10 = local.BrkKsOth.Text10;
    useImport.CtlBrkSp.Text30 = local.Sp.Text30;
    useImport.CtlBrkPrgGrp.Text10 = local.BrkPrmGrp.Text10;
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabB609ReportWriter.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private void UseEabB609ReportWriter2()
  {
    var useImport = new EabB609ReportWriter.Import();
    var useExport = new EabB609ReportWriter.Export();

    useImport.ReportParms.Assign(local.Send);
    useImport.Bom.Date = local.Bom.Date;
    useImport.Eom.Date = local.Eom.Date;
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabB609ReportWriter.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", local.SelectSp.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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
    /// A value of ProgramControlTotal.
    /// </summary>
    [JsonPropertyName("programControlTotal")]
    public ProgramControlTotal ProgramControlTotal
    {
      get => programControlTotal ??= new();
      set => programControlTotal = value;
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
    /// A value of RptParm.
    /// </summary>
    [JsonPropertyName("rptParm")]
    public WorkArea RptParm
    {
      get => rptParm ??= new();
      set => rptParm = value;
    }

    /// <summary>
    /// A value of JclParms.
    /// </summary>
    [JsonPropertyName("jclParms")]
    public WorkArea JclParms
    {
      get => jclParms ??= new();
      set => jclParms = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of SelectSp.
    /// </summary>
    [JsonPropertyName("selectSp")]
    public ServiceProvider SelectSp
    {
      get => selectSp ??= new();
      set => selectSp = value;
    }

    /// <summary>
    /// A value of PrevServiceProvider.
    /// </summary>
    [JsonPropertyName("prevServiceProvider")]
    public ServiceProvider PrevServiceProvider
    {
      get => prevServiceProvider ??= new();
      set => prevServiceProvider = value;
    }

    /// <summary>
    /// A value of PrevSp.
    /// </summary>
    [JsonPropertyName("prevSp")]
    public TextWorkArea PrevSp
    {
      get => prevSp ??= new();
      set => prevSp = value;
    }

    /// <summary>
    /// A value of Sp.
    /// </summary>
    [JsonPropertyName("sp")]
    public TextWorkArea Sp
    {
      get => sp ??= new();
      set => sp = value;
    }

    /// <summary>
    /// A value of LprevProgGrp.
    /// </summary>
    [JsonPropertyName("lprevProgGrp")]
    public TextWorkArea LprevProgGrp
    {
      get => lprevProgGrp ??= new();
      set => lprevProgGrp = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of KorO.
    /// </summary>
    [JsonPropertyName("korO")]
    public TextWorkArea KorO
    {
      get => korO ??= new();
      set => korO = value;
    }

    /// <summary>
    /// A value of ProgGrp.
    /// </summary>
    [JsonPropertyName("progGrp")]
    public TextWorkArea ProgGrp
    {
      get => progGrp ??= new();
      set => progGrp = value;
    }

    /// <summary>
    /// A value of Parm.
    /// </summary>
    [JsonPropertyName("parm")]
    public TextWorkArea Parm
    {
      get => parm ??= new();
      set => parm = value;
    }

    /// <summary>
    /// A value of DetRtPrn.
    /// </summary>
    [JsonPropertyName("detRtPrn")]
    public WorkArea DetRtPrn
    {
      get => detRtPrn ??= new();
      set => detRtPrn = value;
    }

    /// <summary>
    /// A value of DetLtPrn.
    /// </summary>
    [JsonPropertyName("detLtPrn")]
    public WorkArea DetLtPrn
    {
      get => detLtPrn ??= new();
      set => detLtPrn = value;
    }

    /// <summary>
    /// A value of PrgTotRtPrn.
    /// </summary>
    [JsonPropertyName("prgTotRtPrn")]
    public WorkArea PrgTotRtPrn
    {
      get => prgTotRtPrn ??= new();
      set => prgTotRtPrn = value;
    }

    /// <summary>
    /// A value of PrgTotLtPrn.
    /// </summary>
    [JsonPropertyName("prgTotLtPrn")]
    public WorkArea PrgTotLtPrn
    {
      get => prgTotLtPrn ??= new();
      set => prgTotLtPrn = value;
    }

    /// <summary>
    /// A value of SpKsNtafRtPrn.
    /// </summary>
    [JsonPropertyName("spKsNtafRtPrn")]
    public WorkArea SpKsNtafRtPrn
    {
      get => spKsNtafRtPrn ??= new();
      set => spKsNtafRtPrn = value;
    }

    /// <summary>
    /// A value of SpKsNtafLtPrn.
    /// </summary>
    [JsonPropertyName("spKsNtafLtPrn")]
    public WorkArea SpKsNtafLtPrn
    {
      get => spKsNtafLtPrn ??= new();
      set => spKsNtafLtPrn = value;
    }

    /// <summary>
    /// A value of SpKsTafRtPrn.
    /// </summary>
    [JsonPropertyName("spKsTafRtPrn")]
    public WorkArea SpKsTafRtPrn
    {
      get => spKsTafRtPrn ??= new();
      set => spKsTafRtPrn = value;
    }

    /// <summary>
    /// A value of SpKsTafLtPrn.
    /// </summary>
    [JsonPropertyName("spKsTafLtPrn")]
    public WorkArea SpKsTafLtPrn
    {
      get => spKsTafLtPrn ??= new();
      set => spKsTafLtPrn = value;
    }

    /// <summary>
    /// A value of SpKsSoRtPrn.
    /// </summary>
    [JsonPropertyName("spKsSoRtPrn")]
    public WorkArea SpKsSoRtPrn
    {
      get => spKsSoRtPrn ??= new();
      set => spKsSoRtPrn = value;
    }

    /// <summary>
    /// A value of SpKsSoLtPrn.
    /// </summary>
    [JsonPropertyName("spKsSoLtPrn")]
    public WorkArea SpKsSoLtPrn
    {
      get => spKsSoLtPrn ??= new();
      set => spKsSoLtPrn = value;
    }

    /// <summary>
    /// A value of SpOthNtafRtPrn.
    /// </summary>
    [JsonPropertyName("spOthNtafRtPrn")]
    public WorkArea SpOthNtafRtPrn
    {
      get => spOthNtafRtPrn ??= new();
      set => spOthNtafRtPrn = value;
    }

    /// <summary>
    /// A value of SpOthNtafLtPrn.
    /// </summary>
    [JsonPropertyName("spOthNtafLtPrn")]
    public WorkArea SpOthNtafLtPrn
    {
      get => spOthNtafLtPrn ??= new();
      set => spOthNtafLtPrn = value;
    }

    /// <summary>
    /// A value of SpOthTafRtPrn.
    /// </summary>
    [JsonPropertyName("spOthTafRtPrn")]
    public WorkArea SpOthTafRtPrn
    {
      get => spOthTafRtPrn ??= new();
      set => spOthTafRtPrn = value;
    }

    /// <summary>
    /// A value of SpOthTafLtPrn.
    /// </summary>
    [JsonPropertyName("spOthTafLtPrn")]
    public WorkArea SpOthTafLtPrn
    {
      get => spOthTafLtPrn ??= new();
      set => spOthTafLtPrn = value;
    }

    /// <summary>
    /// A value of SpGtotRtPrn.
    /// </summary>
    [JsonPropertyName("spGtotRtPrn")]
    public WorkArea SpGtotRtPrn
    {
      get => spGtotRtPrn ??= new();
      set => spGtotRtPrn = value;
    }

    /// <summary>
    /// A value of SpGtotLtPrn.
    /// </summary>
    [JsonPropertyName("spGtotLtPrn")]
    public WorkArea SpGtotLtPrn
    {
      get => spGtotLtPrn ??= new();
      set => spGtotLtPrn = value;
    }

    /// <summary>
    /// A value of PosCnt.
    /// </summary>
    [JsonPropertyName("posCnt")]
    public Common PosCnt
    {
      get => posCnt ??= new();
      set => posCnt = value;
    }

    /// <summary>
    /// A value of SpOthTafCaseCnt.
    /// </summary>
    [JsonPropertyName("spOthTafCaseCnt")]
    public Common SpOthTafCaseCnt
    {
      get => spOthTafCaseCnt ??= new();
      set => spOthTafCaseCnt = value;
    }

    /// <summary>
    /// A value of SpOthNtafCaseCnt.
    /// </summary>
    [JsonPropertyName("spOthNtafCaseCnt")]
    public Common SpOthNtafCaseCnt
    {
      get => spOthNtafCaseCnt ??= new();
      set => spOthNtafCaseCnt = value;
    }

    /// <summary>
    /// A value of SpOthNtaf.
    /// </summary>
    [JsonPropertyName("spOthNtaf")]
    public Collection SpOthNtaf
    {
      get => spOthNtaf ??= new();
      set => spOthNtaf = value;
    }

    /// <summary>
    /// A value of SpOthTaf.
    /// </summary>
    [JsonPropertyName("spOthTaf")]
    public Collection SpOthTaf
    {
      get => spOthTaf ??= new();
      set => spOthTaf = value;
    }

    /// <summary>
    /// A value of SpGtotCollection.
    /// </summary>
    [JsonPropertyName("spGtotCollection")]
    public Collection SpGtotCollection
    {
      get => spGtotCollection ??= new();
      set => spGtotCollection = value;
    }

    /// <summary>
    /// A value of SpGtotCommon.
    /// </summary>
    [JsonPropertyName("spGtotCommon")]
    public Common SpGtotCommon
    {
      get => spGtotCommon ??= new();
      set => spGtotCommon = value;
    }

    /// <summary>
    /// A value of SpKsTafCaseCnt.
    /// </summary>
    [JsonPropertyName("spKsTafCaseCnt")]
    public Common SpKsTafCaseCnt
    {
      get => spKsTafCaseCnt ??= new();
      set => spKsTafCaseCnt = value;
    }

    /// <summary>
    /// A value of SpKsNtafCaseCnt.
    /// </summary>
    [JsonPropertyName("spKsNtafCaseCnt")]
    public Common SpKsNtafCaseCnt
    {
      get => spKsNtafCaseCnt ??= new();
      set => spKsNtafCaseCnt = value;
    }

    /// <summary>
    /// A value of SpKsSoCaseCnt.
    /// </summary>
    [JsonPropertyName("spKsSoCaseCnt")]
    public Common SpKsSoCaseCnt
    {
      get => spKsSoCaseCnt ??= new();
      set => spKsSoCaseCnt = value;
    }

    /// <summary>
    /// A value of SpAdjTafCaseCnt.
    /// </summary>
    [JsonPropertyName("spAdjTafCaseCnt")]
    public Common SpAdjTafCaseCnt
    {
      get => spAdjTafCaseCnt ??= new();
      set => spAdjTafCaseCnt = value;
    }

    /// <summary>
    /// A value of SpAdjNtafCaseCnt.
    /// </summary>
    [JsonPropertyName("spAdjNtafCaseCnt")]
    public Common SpAdjNtafCaseCnt
    {
      get => spAdjNtafCaseCnt ??= new();
      set => spAdjNtafCaseCnt = value;
    }

    /// <summary>
    /// A value of SpAdjSoCaseCnt.
    /// </summary>
    [JsonPropertyName("spAdjSoCaseCnt")]
    public Common SpAdjSoCaseCnt
    {
      get => spAdjSoCaseCnt ??= new();
      set => spAdjSoCaseCnt = value;
    }

    /// <summary>
    /// A value of SpAdjSo.
    /// </summary>
    [JsonPropertyName("spAdjSo")]
    public Collection SpAdjSo
    {
      get => spAdjSo ??= new();
      set => spAdjSo = value;
    }

    /// <summary>
    /// A value of SpAdjNtaf.
    /// </summary>
    [JsonPropertyName("spAdjNtaf")]
    public Collection SpAdjNtaf
    {
      get => spAdjNtaf ??= new();
      set => spAdjNtaf = value;
    }

    /// <summary>
    /// A value of SpAdjTaf.
    /// </summary>
    [JsonPropertyName("spAdjTaf")]
    public Collection SpAdjTaf
    {
      get => spAdjTaf ??= new();
      set => spAdjTaf = value;
    }

    /// <summary>
    /// A value of SpKsSo.
    /// </summary>
    [JsonPropertyName("spKsSo")]
    public Collection SpKsSo
    {
      get => spKsSo ??= new();
      set => spKsSo = value;
    }

    /// <summary>
    /// A value of SpKsNtaf.
    /// </summary>
    [JsonPropertyName("spKsNtaf")]
    public Collection SpKsNtaf
    {
      get => spKsNtaf ??= new();
      set => spKsNtaf = value;
    }

    /// <summary>
    /// A value of SpKsTaf.
    /// </summary>
    [JsonPropertyName("spKsTaf")]
    public Collection SpKsTaf
    {
      get => spKsTaf ??= new();
      set => spKsTaf = value;
    }

    /// <summary>
    /// A value of TotPrgmGrpCommon.
    /// </summary>
    [JsonPropertyName("totPrgmGrpCommon")]
    public Common TotPrgmGrpCommon
    {
      get => totPrgmGrpCommon ??= new();
      set => totPrgmGrpCommon = value;
    }

    /// <summary>
    /// A value of TotPrgmGrpCollection.
    /// </summary>
    [JsonPropertyName("totPrgmGrpCollection")]
    public Collection TotPrgmGrpCollection
    {
      get => totPrgmGrpCollection ??= new();
      set => totPrgmGrpCollection = value;
    }

    /// <summary>
    /// A value of So.
    /// </summary>
    [JsonPropertyName("so")]
    public Collection So
    {
      get => so ??= new();
      set => so = value;
    }

    /// <summary>
    /// A value of SoCaseCnt.
    /// </summary>
    [JsonPropertyName("soCaseCnt")]
    public Common SoCaseCnt
    {
      get => soCaseCnt ??= new();
      set => soCaseCnt = value;
    }

    /// <summary>
    /// A value of PrevCase.
    /// </summary>
    [JsonPropertyName("prevCase")]
    public Case1 PrevCase
    {
      get => prevCase ??= new();
      set => prevCase = value;
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
    /// A value of PrevLegalAction.
    /// </summary>
    [JsonPropertyName("prevLegalAction")]
    public LegalAction PrevLegalAction
    {
      get => prevLegalAction ??= new();
      set => prevLegalAction = value;
    }

    /// <summary>
    /// A value of BrkPrmGrpPrev.
    /// </summary>
    [JsonPropertyName("brkPrmGrpPrev")]
    public TextWorkArea BrkPrmGrpPrev
    {
      get => brkPrmGrpPrev ??= new();
      set => brkPrmGrpPrev = value;
    }

    /// <summary>
    /// A value of BrkKsOthPrev.
    /// </summary>
    [JsonPropertyName("brkKsOthPrev")]
    public TextWorkArea BrkKsOthPrev
    {
      get => brkKsOthPrev ??= new();
      set => brkKsOthPrev = value;
    }

    /// <summary>
    /// A value of BrkKsOth.
    /// </summary>
    [JsonPropertyName("brkKsOth")]
    public TextWorkArea BrkKsOth
    {
      get => brkKsOth ??= new();
      set => brkKsOth = value;
    }

    /// <summary>
    /// A value of BrkPrmGrp.
    /// </summary>
    [JsonPropertyName("brkPrmGrp")]
    public TextWorkArea BrkPrmGrp
    {
      get => brkPrmGrp ??= new();
      set => brkPrmGrp = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of Bom.
    /// </summary>
    [JsonPropertyName("bom")]
    public DateWorkArea Bom
    {
      get => bom ??= new();
      set => bom = value;
    }

    /// <summary>
    /// A value of Eom.
    /// </summary>
    [JsonPropertyName("eom")]
    public DateWorkArea Eom
    {
      get => eom ??= new();
      set => eom = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public ReportParms Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public ReportParms Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    private ProgramControlTotal programControlTotal;
    private ProgramProcessingInfo programProcessingInfo;
    private WorkArea rptParm;
    private WorkArea jclParms;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private LegalAction legalAction;
    private Case1 case1;
    private Collection collection;
    private ServiceProvider serviceProvider;
    private ServiceProvider selectSp;
    private ServiceProvider prevServiceProvider;
    private TextWorkArea prevSp;
    private TextWorkArea sp;
    private TextWorkArea lprevProgGrp;
    private LegalReferralAssignment legalReferralAssignment;
    private Tribunal tribunal;
    private Fips fips;
    private CollectionType collectionType;
    private TextWorkArea korO;
    private TextWorkArea progGrp;
    private TextWorkArea parm;
    private WorkArea detRtPrn;
    private WorkArea detLtPrn;
    private WorkArea prgTotRtPrn;
    private WorkArea prgTotLtPrn;
    private WorkArea spKsNtafRtPrn;
    private WorkArea spKsNtafLtPrn;
    private WorkArea spKsTafRtPrn;
    private WorkArea spKsTafLtPrn;
    private WorkArea spKsSoRtPrn;
    private WorkArea spKsSoLtPrn;
    private WorkArea spOthNtafRtPrn;
    private WorkArea spOthNtafLtPrn;
    private WorkArea spOthTafRtPrn;
    private WorkArea spOthTafLtPrn;
    private WorkArea spGtotRtPrn;
    private WorkArea spGtotLtPrn;
    private Common posCnt;
    private Common spOthTafCaseCnt;
    private Common spOthNtafCaseCnt;
    private Collection spOthNtaf;
    private Collection spOthTaf;
    private Collection spGtotCollection;
    private Common spGtotCommon;
    private Common spKsTafCaseCnt;
    private Common spKsNtafCaseCnt;
    private Common spKsSoCaseCnt;
    private Common spAdjTafCaseCnt;
    private Common spAdjNtafCaseCnt;
    private Common spAdjSoCaseCnt;
    private Collection spAdjSo;
    private Collection spAdjNtaf;
    private Collection spAdjTaf;
    private Collection spKsSo;
    private Collection spKsNtaf;
    private Collection spKsTaf;
    private Common totPrgmGrpCommon;
    private Collection totPrgmGrpCollection;
    private Collection so;
    private Common soCaseCnt;
    private Case1 prevCase;
    private Common caseCnt;
    private LegalAction prevLegalAction;
    private TextWorkArea brkPrmGrpPrev;
    private TextWorkArea brkKsOthPrev;
    private TextWorkArea brkKsOth;
    private TextWorkArea brkPrmGrp;
    private DateWorkArea max;
    private DateWorkArea initialized;
    private DateWorkArea current;
    private DateWorkArea bom;
    private DateWorkArea eom;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private ReportParms send;
    private EabReportSend neededToWrite;
    private ReportParms return1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private ServiceProvider serviceProvider;
  }
#endregion
}
