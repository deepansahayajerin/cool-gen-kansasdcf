// Program: OE_B462_AP_DEBT_DERIVATION_RPT, ID: 945103208, model: 746.
// Short name: SWEF462B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B462_AP_DEBT_DERIVATION_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB462ApDebtDerivationRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B462_AP_DEBT_DERIVATION_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB462ApDebtDerivationRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB462ApDebtDerivationRpt.
  /// </summary>
  public OeB462ApDebtDerivationRpt(IContext context, Import import,
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
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 07/25/2012      DDupree   	Initial Creation - CQ34499
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB462Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.StartCommon.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;
    local.IncludeArrearsOnly.Flag = "";

    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      do
      {
        local.Postion.Text1 =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.CurrentPosition.Count, 1);

        if (AsChar(local.Postion.Text1) == ',')
        {
          ++local.FieldNumber.Count;
          local.WorkArea.Text15 = "";

          switch(local.FieldNumber.Count)
          {
            case 1:
              if (local.Current.Count == 1)
              {
                ExitState = "ACO_NE0000_MISSING_PARAMETERS";

                return;
              }
              else
              {
                local.ArrearsParm.Text1 =
                  Substring(local.ProgramProcessingInfo.ParameterList,
                  local.StartCommon.Count, local.Current.Count - 1);
              }

              if (AsChar(local.ArrearsParm.Text1) != '1' && AsChar
                (local.ArrearsParm.Text1) != '2' && AsChar
                (local.ArrearsParm.Text1) != '3' && AsChar
                (local.ArrearsParm.Text1) != '4' && AsChar
                (local.ArrearsParm.Text1) != '5')
              {
                ExitState = "ACO_NE0000_PARAMETER_INVALID";

                return;
              }

              local.StartCommon.Count = local.CurrentPosition.Count + 1;
              local.Current.Count = 0;

              break;
            case 2:
              if (local.Current.Count == 1)
              {
                ExitState = "ACO_NE0000_MISSING_PARAMETERS";

                return;
              }
              else
              {
                local.CurrentParm.Text1 =
                  Substring(local.ProgramProcessingInfo.ParameterList,
                  local.StartCommon.Count, local.Current.Count - 1);
              }

              if (AsChar(local.CurrentParm.Text1) != 'N' && AsChar
                (local.CurrentParm.Text1) != 'Y')
              {
                ExitState = "ACO_NE0000_PARAMETER_INVALID";

                return;
              }

              goto Test1;

              local.StartCommon.Count = local.CurrentPosition.Count + 1;
              local.Current.Count = 0;

              break;
            default:
              goto Test1;
          }
        }
        else if (IsEmpty(local.Postion.Text1))
        {
          break;
        }

        ++local.CurrentPosition.Count;
        ++local.Current.Count;
      }
      while(!Equal(global.Command, "COMMAND"));
    }
    else
    {
      ExitState = "ACO_NE0000_MISSING_PARAMETERS";

      return;
    }

Test1:

    if (IsEmpty(local.ArrearsParm.Text1) || IsEmpty(local.CurrentParm.Text1))
    {
      ExitState = "ACO_NE0000_MISSING_PARAMETERS";

      return;
    }

    local.Process.Date = Now().Date;
    UseCabFirstAndLastDateOfMonth();
    local.ZeroDate.Date = new DateTime(1, 1, 1);
    local.EndDate.Date = new DateTime(2099, 12, 31);
    local.TotalNumApThatQualfied.Count = 0;
    local.TotalNumberOfErrorsRec.Count = 0;
    local.NumberOfRecordsRead.Count = 0;
    local.CsePerson.Number = "";

    foreach(var item in ReadObligationCsePersonLegalAction())
    {
      // DEBT_TYP_ID  DEBT_TYP_CD  
      // DEBT_TYP_NM
      // 
      // DEBT_TYP_CLA
      // ---------+---------+---------+---------+---------+---------+---------+
      // ---------+
      //           1  CS           CHILD SUPPORT                             A
      //           2  SP           SPOUSAL SUPPORT                           A
      //           3  MS           MEDICAL SUPPORT                           A
      //           4  IVD RC       IV-D RECOVERY                             R
      //           5  IRS NEG      IRS NEGATIVE RECOVERY                     R
      //           6  MIS AR       AR MISDIRECTED PAYMENT                    R
      //           7  MIS AP       AP MISDIRECTED PAYMENT                    R
      //           8  MIS NON      NON-CASE PERSON MISDIRECTED PAYMENT       R
      //           9  BDCK RC      BAD 
      // CHECK
      // 
      // R
      //          10  MJ           MEDICAL JUDGEMENT                         M
      //          11  %UME         PERCENT UNINSURED MEDICAL EXP JUDGEMENT   M
      //          12  IJ           INTEREST JUDGEMENT                        N
      //          13  AJ           ARREARS JUDGEMENT                         N
      //          14  CRCH         COST OF RAISING CHILD                     N
      //          15  FEE          GENETIC FEE TEST                          F
      //          16  VOL          
      // VOLUNTARY
      // 
      // V
      //          17  SAJ          SPOUSAL ARREARS JUDGEMENT                 N
      //          18  718B         718B JUDGEMENT                            N
      //          19  MC           MEDICAL COSTS                             A
      //          20  WA           WITHHOLDING ARREARS                       N
      //          21  WC           WITHHOLDING CURRENT                       A
      //          22  GIFT         GIFT
      // 
      // V
      ExitState = "ACO_NN0000_ALL_OK";

      if (Equal(local.ApAlreadyProcessed.Number, entities.CsePerson.Number) && Equal
        (local.AlreadyCheckedCtNum.StandardNumber,
        entities.LegalAction.StandardNumber))
      {
        // we have already checked and processed this combination, need to go to
        // the next combination
        continue;
      }

      if (!Equal(local.CsePerson.Number, entities.CsePerson.Number))
      {
        // we have switched court order numbers so we have to start over, clear 
        // flags
        local.ActiveAccrual.Flag = "";
        local.StateArrears.Flag = "";
        local.ApAlreadyProcessed.Number = "";
        local.AlreadyCheckedCtNum.StandardNumber = "";
      }

      if (!Equal(entities.LegalAction.StandardNumber,
        local.LegalAction.StandardNumber))
      {
        // we have switched court order numbers so we have to start over, clear 
        // flags
        local.ActiveAccrual.Flag = "";
        local.StateArrears.Flag = "";
        local.ApAlreadyProcessed.Number = "";
        local.AlreadyCheckedCtNum.StandardNumber = "";
      }

      local.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
      local.CsePerson.Number = entities.CsePerson.Number;

      if (AsChar(local.ProvideLevelForMessage.Text1) != 'Z')
      {
        // this if statement is just so the program can write out error messages
        // without stopping the program.
        local.ApAlreadyProcessed.Number = local.CsePerson.Number;
        local.AlreadyCheckedCtNum.StandardNumber =
          entities.LegalAction.StandardNumber;

        // set the flags her so we will only check the ap/court order number 
        // once.
        if (AsChar(local.CurrentParm.Text1) == 'Y')
        {
          if (ReadAccrualInstructions())
          {
            if (Lt(entities.AccrualInstructions.LastAccrualDt, local.From.Date))
            {
              // this is a arrears only accural
              continue;
            }
            else
            {
              // this is a current accural
              local.ActiveAccrual.Flag = "Y";
            }
          }
          else
          {
            continue;
          }
        }
        else
        {
          local.ActiveAccrual.Flag = "Y";
        }

        local.ScreenOwedAmountsDtl.Assign(local.ClearScreenOwedAmountsDtl);
        UseFnComputeSummaryTotalsDtl();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test2;
        }

        local.Total.TotalArrearsOwed = 0;

        switch(AsChar(local.ArrearsParm.Text1))
        {
          case '1':
            local.Total.TotalArrearsOwed =
              local.ScreenOwedAmountsDtl.AfPaArrearsOwed + local
              .ScreenOwedAmountsDtl.AfTaArrearsOwed + local
              .ScreenOwedAmountsDtl.FcPaArrearsOwed + local
              .ScreenOwedAmountsDtl.NcArrearsOwed + local
              .ScreenOwedAmountsDtl.NfArrearsOwed;

            break;
          case '2':
            local.Total.TotalArrearsOwed =
              local.ScreenOwedAmountsDtl.NaNaArrearsOwed + local
              .ScreenOwedAmountsDtl.NaCaArrearsOwed;

            break;
          case '3':
            local.Total.TotalArrearsOwed =
              local.ScreenOwedAmountsDtl.AfPaArrearsOwed + local
              .ScreenOwedAmountsDtl.AfTaArrearsOwed + local
              .ScreenOwedAmountsDtl.FcPaArrearsOwed + local
              .ScreenOwedAmountsDtl.NcArrearsOwed + local
              .ScreenOwedAmountsDtl.NfArrearsOwed + local
              .ScreenOwedAmountsDtl.NaCaArrearsOwed + local
              .ScreenOwedAmountsDtl.NaNaArrearsOwed;

            break;
          case '4':
            local.Total.TotalArrearsOwed =
              local.ScreenOwedAmountsDtl.AfiArrearsOwed + local
              .ScreenOwedAmountsDtl.NaiArrearsOwed + local
              .ScreenOwedAmountsDtl.FciArrearsOwed;

            break;
          case '5':
            local.Total.TotalArrearsOwed =
              local.ScreenOwedAmountsDtl.AfPaArrearsOwed + local
              .ScreenOwedAmountsDtl.AfTaArrearsOwed + local
              .ScreenOwedAmountsDtl.FcPaArrearsOwed + local
              .ScreenOwedAmountsDtl.NcArrearsOwed + local
              .ScreenOwedAmountsDtl.NfArrearsOwed + local
              .ScreenOwedAmountsDtl.NaCaArrearsOwed + local
              .ScreenOwedAmountsDtl.NaNaArrearsOwed + local
              .ScreenOwedAmountsDtl.NaiArrearsOwed + local
              .ScreenOwedAmountsDtl.FciArrearsOwed + local
              .ScreenOwedAmountsDtl.AfiArrearsOwed;

            break;
          default:
            break;
        }

        if (local.Total.TotalArrearsOwed <= 0)
        {
          continue;
        }
        else
        {
          local.StateArrears.Flag = "Y";
        }

        if (AsChar(local.ActiveAccrual.Flag) == 'Y' && AsChar
          (local.StateArrears.Flag) == 'Y')
        {
          local.Local1.Number = "";
          local.Local2.Number = "";
          local.Local3.Number = "";
          local.Local4.Number = "";
          local.Local5.Number = "";

          foreach(var item1 in ReadCase())
          {
            if (!Equal(entities.Case1.Number, local.Local1.Number) && IsEmpty
              (local.Local1.Number))
            {
              local.Local1.Number = entities.Case1.Number;
            }
            else if (!Equal(entities.Case1.Number, local.Local2.Number) && IsEmpty
              (local.Local2.Number) && !
              Equal(entities.Case1.Number, local.Local1.Number))
            {
              local.Local2.Number = entities.Case1.Number;
            }
            else if (!Equal(entities.Case1.Number, local.Local3.Number) && IsEmpty
              (local.Local3.Number) && !
              Equal(entities.Case1.Number, local.Local1.Number) && !
              Equal(entities.Case1.Number, local.Local2.Number))
            {
              local.Local3.Number = entities.Case1.Number;
            }
            else if (!Equal(entities.Case1.Number, local.Local4.Number) && IsEmpty
              (local.Local4.Number) && !
              Equal(entities.Case1.Number, local.Local1.Number) && !
              Equal(entities.Case1.Number, local.Local2.Number) && !
              Equal(entities.Case1.Number, local.Local3.Number))
            {
              local.Local4.Number = entities.Case1.Number;
            }
            else if (!Equal(entities.Case1.Number, local.Local5.Number) && IsEmpty
              (local.Local5.Number) && !
              Equal(entities.Case1.Number, local.Local1.Number) && !
              Equal(entities.Case1.Number, local.Local2.Number) && !
              Equal(entities.Case1.Number, local.Local3.Number) && !
              Equal(entities.Case1.Number, local.Local4.Number))
            {
              local.Local5.Number = entities.Case1.Number;

              break;
            }
          }

          local.CasesFound.Text60 = local.Local1.Number + " " + local
            .Local2.Number + " " + local.Local3.Number + " " + local
            .Local4.Number + " " + local.Local5.Number;
          local.Search.Flag = "3";
          local.Phonetic.Percentage = 100;
          local.StartCsePersonsWorkSet.Number = entities.CsePerson.Number;
          MoveCsePersonsWorkSet1(local.ClearCsePersonsWorkSet,
            local.CsePersonsWorkSet);
          UseSiReadCsePersonBatch();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test2;
          }

          UseFnGetActiveCsePersonAddress();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test2;
          }

          local.ConvertAmount.Text15 =
            NumberToString((long)(local.Total.TotalArrearsOwed * 100), 15);
          local.ConvertAmount.Text2 =
            Substring(local.ConvertAmount.Text15, 14, 2);
          local.PostionCount.Count = Verify(local.ConvertAmount.Text15, "0");
          local.TotalArrears.Text11 = "$" + Substring
            (local.ConvertAmount.Text15, WorkArea.Text15_MaxLength,
            local.PostionCount.Count, 13 - local.PostionCount.Count + 1) + "."
            + local.ConvertAmount.Text2;
          local.PassArea.FileInstruction = "WRITE";
          UseOeEabWriteApDebtDerivation1();

          if (!Equal(local.PassArea.TextReturnCode, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.PersonSend.Flag = "";

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Program abended because: " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            ++local.TotalNumberOfErrorsRec.Count;
          }

          continue;
        }
      }

Test2:

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error occured: " + TrimEnd
          (local.ExitStateWorkArea.Message) + " for AP # " + local
          .CsePerson.Number;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        ++local.TotalNumberOfErrorsRec.Count;
      }
    }

    local.PassArea.FileInstruction = "CLOSE";
    UseOeEabWriteApDebtDerivation2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.ReplicationIndicator = source.ReplicationIndicator;
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private static void MoveScreenOwedAmountsDtl(ScreenOwedAmountsDtl source,
    ScreenOwedAmountsDtl target)
  {
    target.AfiArrearsOwed = source.AfiArrearsOwed;
    target.FciArrearsOwed = source.FciArrearsOwed;
    target.NaiArrearsOwed = source.NaiArrearsOwed;
    target.NfArrearsOwed = source.NfArrearsOwed;
    target.NcArrearsOwed = source.NcArrearsOwed;
    target.NaNaArrearsOwed = source.NaNaArrearsOwed;
    target.NaCaArrearsOwed = source.NaCaArrearsOwed;
    target.AfPaArrearsOwed = source.AfPaArrearsOwed;
    target.AfTaArrearsOwed = source.AfTaArrearsOwed;
    target.FcPaArrearsOwed = source.FcPaArrearsOwed;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

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

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Process.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.To.Date = useExport.Last.Date;
    local.From.Date = useExport.First.Date;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnComputeSummaryTotalsDtl()
  {
    var useImport = new FnComputeSummaryTotalsDtl.Import();
    var useExport = new FnComputeSummaryTotalsDtl.Export();

    useImport.Obligor.Number = local.CsePerson.Number;
    useImport.FilterByStdNo.StandardNumber = local.LegalAction.StandardNumber;

    Call(FnComputeSummaryTotalsDtl.Execute, useImport, useExport);

    MoveScreenOwedAmountsDtl(useExport.ScreenOwedAmountsDtl,
      local.ScreenOwedAmountsDtl);
  }

  private void UseFnGetActiveCsePersonAddress()
  {
    var useImport = new FnGetActiveCsePersonAddress.Import();
    var useExport = new FnGetActiveCsePersonAddress.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(FnGetActiveCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseOeB462Housekeeping()
  {
    var useImport = new OeB462Housekeeping.Import();
    var useExport = new OeB462Housekeeping.Export();

    Call(OeB462Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseOeEabWriteApDebtDerivation1()
  {
    var useImport = new OeEabWriteApDebtDerivation.Import();
    var useExport = new OeEabWriteApDebtDerivation.Export();

    useImport.SummedAmount.Text11 = local.TotalArrears.Text11;
    useImport.Cases.Text60 = local.CasesFound.Text60;
    useImport.CsePersonAddress.Assign(local.CsePersonAddress);
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    MoveExternal(local.PassArea, useImport.External);
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteApDebtDerivation.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteApDebtDerivation2()
  {
    var useImport = new OeEabWriteApDebtDerivation.Import();
    var useExport = new OeEabWriteApDebtDerivation.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useImport.CsePersonAddress.Assign(local.CsePersonAddress);
    MoveExternal(local.PassArea, useImport.External);
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteApDebtDerivation.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.StartCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
      
  }

  private bool ReadAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "asOfDt", date);
        db.SetString(command, "cspNumber", local.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationCsePersonLegalAction()
  {
    entities.LegalAction.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationCsePersonLegalAction",
      null,
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 7);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.CsePerson.Type1 = db.GetString(reader, 9);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 10);
        entities.LegalAction.Classification = db.GetString(reader, 11);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 12);
        entities.LegalAction.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

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
    /// A value of ClearScreenOwedAmountsDtl.
    /// </summary>
    [JsonPropertyName("clearScreenOwedAmountsDtl")]
    public ScreenOwedAmountsDtl ClearScreenOwedAmountsDtl
    {
      get => clearScreenOwedAmountsDtl ??= new();
      set => clearScreenOwedAmountsDtl = value;
    }

    /// <summary>
    /// A value of CurrentParm.
    /// </summary>
    [JsonPropertyName("currentParm")]
    public WorkArea CurrentParm
    {
      get => currentParm ??= new();
      set => currentParm = value;
    }

    /// <summary>
    /// A value of ProvideLevelForMessage.
    /// </summary>
    [JsonPropertyName("provideLevelForMessage")]
    public WorkArea ProvideLevelForMessage
    {
      get => provideLevelForMessage ??= new();
      set => provideLevelForMessage = value;
    }

    /// <summary>
    /// A value of ArrearsParm.
    /// </summary>
    [JsonPropertyName("arrearsParm")]
    public WorkArea ArrearsParm
    {
      get => arrearsParm ??= new();
      set => arrearsParm = value;
    }

    /// <summary>
    /// A value of PostionCount.
    /// </summary>
    [JsonPropertyName("postionCount")]
    public Common PostionCount
    {
      get => postionCount ??= new();
      set => postionCount = value;
    }

    /// <summary>
    /// A value of ConvertAmount.
    /// </summary>
    [JsonPropertyName("convertAmount")]
    public WorkArea ConvertAmount
    {
      get => convertAmount ??= new();
      set => convertAmount = value;
    }

    /// <summary>
    /// A value of TotalArrears.
    /// </summary>
    [JsonPropertyName("totalArrears")]
    public WorkArea TotalArrears
    {
      get => totalArrears ??= new();
      set => totalArrears = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public ScreenOwedAmountsDtl Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of Local5.
    /// </summary>
    [JsonPropertyName("local5")]
    public Case1 Local5
    {
      get => local5 ??= new();
      set => local5 = value;
    }

    /// <summary>
    /// A value of Local4.
    /// </summary>
    [JsonPropertyName("local4")]
    public Case1 Local4
    {
      get => local4 ??= new();
      set => local4 = value;
    }

    /// <summary>
    /// A value of Local3.
    /// </summary>
    [JsonPropertyName("local3")]
    public Case1 Local3
    {
      get => local3 ??= new();
      set => local3 = value;
    }

    /// <summary>
    /// A value of Local2.
    /// </summary>
    [JsonPropertyName("local2")]
    public Case1 Local2
    {
      get => local2 ??= new();
      set => local2 = value;
    }

    /// <summary>
    /// A value of Local1.
    /// </summary>
    [JsonPropertyName("local1")]
    public Case1 Local1
    {
      get => local1 ??= new();
      set => local1 = value;
    }

    /// <summary>
    /// A value of CasesFound.
    /// </summary>
    [JsonPropertyName("casesFound")]
    public WorkArea CasesFound
    {
      get => casesFound ??= new();
      set => casesFound = value;
    }

    /// <summary>
    /// A value of MultiJoint.
    /// </summary>
    [JsonPropertyName("multiJoint")]
    public TextWorkArea MultiJoint
    {
      get => multiJoint ??= new();
      set => multiJoint = value;
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
    /// A value of StateArrears.
    /// </summary>
    [JsonPropertyName("stateArrears")]
    public Common StateArrears
    {
      get => stateArrears ??= new();
      set => stateArrears = value;
    }

    /// <summary>
    /// A value of ActiveAccrual.
    /// </summary>
    [JsonPropertyName("activeAccrual")]
    public Common ActiveAccrual
    {
      get => activeAccrual ??= new();
      set => activeAccrual = value;
    }

    /// <summary>
    /// A value of ProgramScreenAttributes.
    /// </summary>
    [JsonPropertyName("programScreenAttributes")]
    public ProgramScreenAttributes ProgramScreenAttributes
    {
      get => programScreenAttributes ??= new();
      set => programScreenAttributes = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of Amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public WorkArea Amount
    {
      get => amount ??= new();
      set => amount = value;
    }

    /// <summary>
    /// A value of AlreadyCheckedCtNum.
    /// </summary>
    [JsonPropertyName("alreadyCheckedCtNum")]
    public LegalAction AlreadyCheckedCtNum
    {
      get => alreadyCheckedCtNum ??= new();
      set => alreadyCheckedCtNum = value;
    }

    /// <summary>
    /// A value of NumberOfProcessedRecord.
    /// </summary>
    [JsonPropertyName("numberOfProcessedRecord")]
    public Common NumberOfProcessedRecord
    {
      get => numberOfProcessedRecord ??= new();
      set => numberOfProcessedRecord = value;
    }

    /// <summary>
    /// A value of AmountOwed.
    /// </summary>
    [JsonPropertyName("amountOwed")]
    public Common AmountOwed
    {
      get => amountOwed ??= new();
      set => amountOwed = value;
    }

    /// <summary>
    /// A value of ContinueProcess.
    /// </summary>
    [JsonPropertyName("continueProcess")]
    public Common ContinueProcess
    {
      get => continueProcess ??= new();
      set => continueProcess = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public LegalAction Hold
    {
      get => hold ??= new();
      set => hold = value;
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
    /// A value of ClearCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("clearCsePersonsWorkSet")]
    public CsePersonsWorkSet ClearCsePersonsWorkSet
    {
      get => clearCsePersonsWorkSet ??= new();
      set => clearCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PersonSend.
    /// </summary>
    [JsonPropertyName("personSend")]
    public Common PersonSend
    {
      get => personSend ??= new();
      set => personSend = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
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
    /// A value of ApAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("apAlreadyProcessed")]
    public CsePerson ApAlreadyProcessed
    {
      get => apAlreadyProcessed ??= new();
      set => apAlreadyProcessed = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmountsDtl.
    /// </summary>
    [JsonPropertyName("screenOwedAmountsDtl")]
    public ScreenOwedAmountsDtl ScreenOwedAmountsDtl
    {
      get => screenOwedAmountsDtl ??= new();
      set => screenOwedAmountsDtl = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of StartCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("startCsePersonsWorkSet")]
    public CsePersonsWorkSet StartCsePersonsWorkSet
    {
      get => startCsePersonsWorkSet ??= new();
      set => startCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of TotalNumberOfErrorsRec.
    /// </summary>
    [JsonPropertyName("totalNumberOfErrorsRec")]
    public Common TotalNumberOfErrorsRec
    {
      get => totalNumberOfErrorsRec ??= new();
      set => totalNumberOfErrorsRec = value;
    }

    /// <summary>
    /// A value of NumberOfRecordsRead.
    /// </summary>
    [JsonPropertyName("numberOfRecordsRead")]
    public Common NumberOfRecordsRead
    {
      get => numberOfRecordsRead ??= new();
      set => numberOfRecordsRead = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of TotalNumApThatQualfied.
    /// </summary>
    [JsonPropertyName("totalNumApThatQualfied")]
    public Common TotalNumApThatQualfied
    {
      get => totalNumApThatQualfied ??= new();
      set => totalNumApThatQualfied = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of ProcessMonthEnd.
    /// </summary>
    [JsonPropertyName("processMonthEnd")]
    public DateWorkArea ProcessMonthEnd
    {
      get => processMonthEnd ??= new();
      set => processMonthEnd = value;
    }

    /// <summary>
    /// A value of CurrentMonthAccurals.
    /// </summary>
    [JsonPropertyName("currentMonthAccurals")]
    public DateWorkArea CurrentMonthAccurals
    {
      get => currentMonthAccurals ??= new();
      set => currentMonthAccurals = value;
    }

    /// <summary>
    /// A value of OneFutureMonthAccurals.
    /// </summary>
    [JsonPropertyName("oneFutureMonthAccurals")]
    public DateWorkArea OneFutureMonthAccurals
    {
      get => oneFutureMonthAccurals ??= new();
      set => oneFutureMonthAccurals = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Common Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of StartCommon.
    /// </summary>
    [JsonPropertyName("startCommon")]
    public Common StartCommon
    {
      get => startCommon ??= new();
      set => startCommon = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
    }

    /// <summary>
    /// A value of IncludeArrearsOnly.
    /// </summary>
    [JsonPropertyName("includeArrearsOnly")]
    public Common IncludeArrearsOnly
    {
      get => includeArrearsOnly ??= new();
      set => includeArrearsOnly = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
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
    /// A value of MinimumAmountOwed.
    /// </summary>
    [JsonPropertyName("minimumAmountOwed")]
    public Common MinimumAmountOwed
    {
      get => minimumAmountOwed ??= new();
      set => minimumAmountOwed = value;
    }

    /// <summary>
    /// A value of ArrearsOnly.
    /// </summary>
    [JsonPropertyName("arrearsOnly")]
    public Common ArrearsOnly
    {
      get => arrearsOnly ??= new();
      set => arrearsOnly = value;
    }

    private ScreenOwedAmountsDtl clearScreenOwedAmountsDtl;
    private WorkArea currentParm;
    private WorkArea provideLevelForMessage;
    private WorkArea arrearsParm;
    private Common postionCount;
    private WorkArea convertAmount;
    private WorkArea totalArrears;
    private ScreenOwedAmountsDtl total;
    private Case1 local5;
    private Case1 local4;
    private Case1 local3;
    private Case1 local2;
    private Case1 local1;
    private WorkArea casesFound;
    private TextWorkArea multiJoint;
    private Case1 case1;
    private Common stateArrears;
    private Common activeAccrual;
    private ProgramScreenAttributes programScreenAttributes;
    private DprProgram dprProgram;
    private Program program;
    private CsePersonAddress csePersonAddress;
    private WorkArea amount;
    private LegalAction alreadyCheckedCtNum;
    private Common numberOfProcessedRecord;
    private Common amountOwed;
    private Common continueProcess;
    private LegalAction hold;
    private CsePerson csePerson;
    private CsePersonsWorkSet clearCsePersonsWorkSet;
    private Common personSend;
    private DateWorkArea endDate;
    private LegalAction legalAction;
    private CsePerson apAlreadyProcessed;
    private DateWorkArea zeroDate;
    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private DateWorkArea dateWorkArea;
    private DateWorkArea startDate;
    private Common numberOfDays;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet startCsePersonsWorkSet;
    private DateWorkArea to;
    private DateWorkArea from;
    private Common totalNumberOfErrorsRec;
    private Common numberOfRecordsRead;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalNumApThatQualfied;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea process;
    private DateWorkArea processMonthEnd;
    private DateWorkArea currentMonthAccurals;
    private DateWorkArea oneFutureMonthAccurals;
    private Common search;
    private Common phonetic;
    private Common startCommon;
    private Common current;
    private Common currentPosition;
    private Common fieldNumber;
    private Common includeArrearsOnly;
    private TextWorkArea postion;
    private WorkArea workArea;
    private Common minimumAmountOwed;
    private Common arrearsOnly;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of SupportedPersonCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedPersonCsePersonAccount")]
    public CsePersonAccount SupportedPersonCsePersonAccount
    {
      get => supportedPersonCsePersonAccount ??= new();
      set => supportedPersonCsePersonAccount = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of N2dView.
    /// </summary>
    [JsonPropertyName("n2dView")]
    public LegalAction N2dView
    {
      get => n2dView ??= new();
      set => n2dView = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of Oth.
    /// </summary>
    [JsonPropertyName("oth")]
    public CsePersonAccount Oth
    {
      get => oth ??= new();
      set => oth = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of SupportedPersonCsePerson.
    /// </summary>
    [JsonPropertyName("supportedPersonCsePerson")]
    public CsePerson SupportedPersonCsePerson
    {
      get => supportedPersonCsePerson ??= new();
      set => supportedPersonCsePerson = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    private CsePersonAccount supportedPersonCsePersonAccount;
    private AccrualInstructions accrualInstructions;
    private LegalAction n2dView;
    private CsePersonAccount csePersonAccount;
    private CsePersonAddress csePersonAddress;
    private LegalActionCaseRole legalActionCaseRole;
    private CaseUnit caseUnit;
    private DisbursementTransaction disbCollection;
    private ObligationTransaction obligationTransaction;
    private LegalAction legalAction;
    private CsePersonAccount oth;
    private LegalActionPerson legalActionPerson;
    private CollectionType collectionType;
    private ObligationType obligationType;
    private CsePerson supportedPersonCsePerson;
    private Case1 case1;
    private CaseRole caseRole;
    private ObligationTransaction debt;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private Obligation obligation;
    private Collection collection;
    private DebtDetail debtDetail;
    private DebtDetail existingDebtDetail;
    private ObligationTransaction existingDebt;
  }
#endregion
}
