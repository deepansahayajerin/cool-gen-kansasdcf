// Program: SP_B737_FOOD_ASSISTANCE_PROG_RPT, ID: 1902579359, model: 746.
// Short name: SWEB737P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B737_FOOD_ASSISTANCE_PROG_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB737FoodAssistanceProgRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B737_FOOD_ASSISTANCE_PROG_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB737FoodAssistanceProgRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB737FoodAssistanceProgRpt.
  /// </summary>
  public SpB737FoodAssistanceProgRpt(IContext context, Import import,
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
    // 12/19/2016      DDupree   	Initial Creation - CQ55806
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ***********************************************************************************************
    // SELECT CAS.NUMB, PP.CSP_NUMBER, PP.EFFECTIVE_DATE, AC.OFF_ID,
    // CAS.STATUS_DATE, SP.LAST_NAME, SP.FIRST_NAME
    // FROM SRCKT41.CKT_CASE CAS,
    //      SRCKT41.CKT_CASE_ROLE CR,
    //      SRCKT41.CKT_PERSON_PROGRAM PP,
    //      SRCKT41.CKT_ASSIGN_CASE AC,
    //      SRCKT41.CKT_OFFC_SRVC_PRVR OSP,
    //      SRCKT41.CKT_SERVICE_PRVDER SP
    // WHERE CAS.NUMB = CR.CAS_NUMBER
    //   AND CAS.NUMB = AC.CAS_NO
    //   AND AC.SPD_ID = OSP.SPD_GENERATED_ID
    //   AND AC.OFF_ID = OSP.OFF_GENERATED_ID
    // AND AC.OSP_CODE = OSP.ROLE_CODE
    // AND OSP.SPD_GENERATED_ID = SP.SERVICE_PRVDER_ID
    // AND AC.DISCONTINUE_DATE > CURRENT_DATE
    // AND OSP.DISCONTINUE_DATE > CURRENT_DATE
    // AND CR.CSP_NUMBER = PP.CSP_NUMBER
    // AND PP.PRG_GENERATED_ID = 4 --FOOD STAMPS
    // AND PP.EFFECTIVE_DATE < '2016-12-01'
    //   AND PP.DISCONTINUE_DATE > CURRENT DATE
    //   AND CAS.STATUS = 'O'
    //   AND CAS.STATUS_DATE < '2016-12-01'
    //   AND CR.TYPE = 'CH'
    // AND CR.END_DATE > CURRENT DATE
    // AND NOT EXISTS (SELECT 1 FROM SRCKT41.CKT_PERSON_PROGRAM P2
    //                      WHERE PP.CSP_NUMBER = P2.CSP_NUMBER
    //                        AND P2.PRG_GENERATED_ID IN
    // (2,5,15)
    // AND P2.DISCONTINUE_DATE > CURRENT DATE)
    //   ORDER BY AC.OFF_ID, OSP.SPD_GENERATED_ID, SP.LAST_NAME, SP.FIRST_NAME
    // WITH UR
    // ***********************************************************************************************
    UseSpB737Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.NumberOfRecordsRead.Count = 0;
    local.CurrentDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.BeginningOfPeriod.Date = AddDays(local.CurrentDate.Date, -45);
    UseCabFirstAndLastDateOfMonth();
    local.Count.Index = -1;
    local.Count.Count = 0;

    foreach(var item in ReadOffice())
    {
      ++local.Count.Index;
      local.Count.CheckSize();

      local.Count.Update.Office.SystemGeneratedId =
        entities.Office.SystemGeneratedId;
    }

    local.RecordsFound.Flag = "";

    foreach(var item in ReadCaseCaseRolePersonProgramOfficeServiceProvider())
    {
      if (Lt(entities.Case1.StatusDate, entities.PersonProgram.EffectiveDate))
      {
        continue;
      }

      if (Lt(entities.Case1.StatusDate, local.BeginningOfPeriod.Date))
      {
        continue;
      }

      if (ReadPersonProgramCsePerson())
      {
        continue;
      }
      else
      {
        // we do not want any ,so this is ok
      }

      UseFnB734DetermineJdFromCase();

      for(local.Count.Index = 0; local.Count.Index < local.Count.Count; ++
        local.Count.Index)
      {
        if (!local.Count.CheckSize())
        {
          break;
        }

        if (local.DashboardAuditData.Office.GetValueOrDefault() == local
          .Count.Item.Office.SystemGeneratedId)
        {
          local.Count.Update.Jd.Code =
            local.DashboardAuditData.JudicialDistrict ?? Spaces(2);

          break;
        }
      }

      local.Count.CheckIndex();

      if (ReadCseOrganization())
      {
        for(local.Count.Index = 0; local.Count.Index < local.Count.Count; ++
          local.Count.Index)
        {
          if (!local.Count.CheckSize())
          {
            break;
          }

          if (Equal(local.Count.Item.Jd.Code,
            local.DashboardAuditData.JudicialDistrict) && local
            .Count.Item.Office.SystemGeneratedId == local
            .DashboardAuditData.Office.GetValueOrDefault())
          {
            local.Count.Update.Contractor.Assign(entities.Contractor);
            local.Count.Update.Count1.Count = local.Count.Item.Count1.Count + 1;

            goto Read;
          }
        }

        local.Count.CheckIndex();
      }

Read:

      local.RecordsFound.Flag = "Y";

      if (Lt(local.ZeroDate.Date, entities.PersonProgram.EffectiveDate))
      {
        local.EffDateDateWorkArea.Year =
          Year(entities.PersonProgram.EffectiveDate);
        local.WorkArea.Text15 =
          NumberToString(local.EffDateDateWorkArea.Year, 15);
        local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
        local.EffDateDateWorkArea.Month =
          Month(entities.PersonProgram.EffectiveDate);
        local.WorkArea.Text15 =
          NumberToString(local.EffDateDateWorkArea.Month, 15);
        local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.EffDateDateWorkArea.Day =
          Day(entities.PersonProgram.EffectiveDate);
        local.WorkArea.Text15 =
          NumberToString(local.EffDateDateWorkArea.Day, 15);
        local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.EffDateDateWorkArea.TextDate = local.Year.Text4 + local
          .Month.Text2 + local.Day.Text2;
        local.EffDateWorkArea.Text10 = local.Month.Text2 + "/" + local
          .Day.Text2 + "/" + local.Year.Text4;
      }
      else
      {
        local.EffDateWorkArea.Text10 = "01/01/0001";
      }

      if (Lt(local.ZeroDate.Date, entities.Case1.StatusDate))
      {
        local.StatusDateDateWorkArea.Year = Year(entities.Case1.StatusDate);
        local.WorkArea.Text15 =
          NumberToString(local.StatusDateDateWorkArea.Year, 15);
        local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
        local.StatusDateDateWorkArea.Month = Month(entities.Case1.StatusDate);
        local.WorkArea.Text15 =
          NumberToString(local.StatusDateDateWorkArea.Month, 15);
        local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.StatusDateDateWorkArea.Day = Day(entities.Case1.StatusDate);
        local.WorkArea.Text15 =
          NumberToString(local.StatusDateDateWorkArea.Day, 15);
        local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.StatusDateDateWorkArea.TextDate = local.Year.Text4 + local
          .Month.Text2 + local.Day.Text2;
        local.StatusDateWorkArea.Text10 = local.Month.Text2 + "/" + local
          .Day.Text2 + "/" + local.Year.Text4;
      }
      else
      {
        local.StatusDateWorkArea.Text10 = "01/01/0001";
      }

      local.CasePersonNumber.Text37 = ";" + entities.Case1.Number + ";" + local
        .StatusDateWorkArea.Text10 + ";" + entities.CsePerson.Number;
      local.EffDateOffice.Text32 =
        NumberToString(entities.Office.SystemGeneratedId, 12, 4) + ";" + entities
        .Contractor.Name;
      local.EffDateOffice.Text32 = " " + local.EffDateOffice.Text32;
      local.Name.Text33 = entities.ServiceProvider.LastName + ";" + entities
        .ServiceProvider.FirstName;
      local.EabReportSend.RptDetail = local.EffDateOffice.Text32 + local
        .CasePersonNumber.Text37 + ";" + local.EffDateWorkArea.Text10 + ";" + local
        .Name.Text33;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriter1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      ++local.NumberOfRecordsRead.Count;
    }

    if (AsChar(local.RecordsFound.Flag) == 'Y')
    {
      // *************************************************************************
      // the following is sorting the summary table
      // most determine contractor totals, city total, grand total, then sorting
      // them in the
      // proper order
      // *************************************************************************
      local.Sorted.Index = -1;
      local.Sorted.Count = 0;

      for(local.Count.Index = 0; local.Count.Index < local.Count.Count; ++
        local.Count.Index)
      {
        if (!local.Count.CheckSize())
        {
          break;
        }

        // sort out blank lines
        if (!IsEmpty(local.Count.Item.Contractor.Name))
        {
        }
        else
        {
          continue;
        }

        ++local.Sorted.Index;
        local.Sorted.CheckSize();

        local.Sorted.Update.ContractorSorted.
          Assign(local.Count.Item.Contractor);
        local.Sorted.Update.Sorted1.SystemGeneratedId =
          local.Count.Item.Office.SystemGeneratedId;
        MoveCseOrganization(local.Count.Item.Jd, local.Sorted.Update.JdSorted);
        local.Sorted.Update.CountSorted.Count = local.Count.Item.Count1.Count;
      }

      local.Count.CheckIndex();
      local.Changed.Flag = "T";

      while(AsChar(local.Changed.Flag) == 'T')
      {
        // sort by contractor name, office
        local.Changed.Flag = "F";

        local.Sorted.Index = 0;
        local.Sorted.CheckSize();

        while(local.Sorted.Index + 1 < local.Sorted.Count)
        {
          local.Swap.Flag = "N";
          local.Temp1.SystemGeneratedId =
            local.Sorted.Item.Sorted1.SystemGeneratedId;
          MoveCseOrganization(local.Sorted.Item.JdSorted, local.JdTemp1);
          local.ContractorTemp1.Assign(local.Sorted.Item.ContractorSorted);
          local.CountTemp1.Count = local.Sorted.Item.CountSorted.Count;

          ++local.Sorted.Index;
          local.Sorted.CheckSize();

          local.Temp2.SystemGeneratedId =
            local.Sorted.Item.Sorted1.SystemGeneratedId;
          MoveCseOrganization(local.Sorted.Item.JdSorted, local.JdTemp2);
          local.ContrtactorTemp2.Assign(local.Sorted.Item.ContractorSorted);
          local.CountTemp2.Count = local.Sorted.Item.CountSorted.Count;

          if (Lt(local.ContrtactorTemp2.Name, local.ContractorTemp1.Name))
          {
            local.Swap.Flag = "Y";
          }
          else if (Equal(local.ContrtactorTemp2.Name, local.ContractorTemp1.Name)
            && local.Temp2.SystemGeneratedId < local.Temp1.SystemGeneratedId)
          {
            local.Swap.Flag = "Y";
          }

          if (AsChar(local.Swap.Flag) == 'Y')
          {
            --local.Sorted.Index;
            local.Sorted.CheckSize();

            local.Sorted.Update.Sorted1.SystemGeneratedId =
              local.Temp2.SystemGeneratedId;
            local.Sorted.Update.ContractorSorted.Assign(local.ContrtactorTemp2);
            MoveCseOrganization(local.JdTemp2, local.Sorted.Update.JdSorted);
            local.Sorted.Update.CountSorted.Count = local.CountTemp2.Count;

            ++local.Sorted.Index;
            local.Sorted.CheckSize();

            local.Sorted.Update.Sorted1.SystemGeneratedId =
              local.Temp1.SystemGeneratedId;
            local.Sorted.Update.ContractorSorted.Assign(local.ContractorTemp1);
            MoveCseOrganization(local.JdTemp1, local.Sorted.Update.JdSorted);
            local.Sorted.Update.CountSorted.Count = local.CountTemp1.Count;
            local.Changed.Flag = "T";
          }
        }
      }

      local.ContractorTemp1.Name = "";
      local.Final.Index = -1;
      local.Final.Count = 0;

      for(local.Sorted.Index = 0; local.Sorted.Index < local.Sorted.Count; ++
        local.Sorted.Index)
      {
        if (!local.Sorted.CheckSize())
        {
          break;
        }

        // trying to total up the contractors here
        local.Swap.Flag = "N";

        if (Equal(local.Sorted.Item.ContractorSorted.Name,
          local.ContractorTemp1.Name) && !IsEmpty(local.ContractorTemp1.Name))
        {
          local.Final.Update.CountFinal.Count =
            local.Final.Item.CountFinal.Count + local
            .Sorted.Item.CountSorted.Count;
        }
        else
        {
          local.Swap.Flag = "Y";

          ++local.Final.Index;
          local.Final.CheckSize();

          local.Final.Update.ContractorFinal.Assign(
            local.Sorted.Item.ContractorSorted);
          local.Final.Update.CountFinal.Count =
            local.Sorted.Item.CountSorted.Count;
          local.Final.Update.Final1.SystemGeneratedId = 0;
        }

        local.CountFinal.Count += local.Sorted.Item.CountSorted.Count;
        local.ContractorTemp1.Assign(local.Sorted.Item.ContractorSorted);
      }

      local.Sorted.CheckIndex();

      for(local.Sorted.Index = 0; local.Sorted.Index < local.Sorted.Count; ++
        local.Sorted.Index)
      {
        if (!local.Sorted.CheckSize())
        {
          break;
        }

        // added city totals back to file
        local.Final.Index = local.Final.Count;
        local.Final.CheckSize();

        local.Final.Update.ContractorFinal.Assign(
          local.Sorted.Item.ContractorSorted);
        local.Final.Update.Final1.SystemGeneratedId =
          local.Sorted.Item.Sorted1.SystemGeneratedId;
        MoveCseOrganization(local.Sorted.Item.JdSorted,
          local.Final.Update.JdFinal);
        local.Final.Update.CountFinal.Count =
          local.Sorted.Item.CountSorted.Count;
      }

      local.Sorted.CheckIndex();

      local.Final.Index = 0;
      local.Final.CheckSize();

      local.Changed.Flag = "T";

      while(AsChar(local.Changed.Flag) == 'T')
      {
        local.Changed.Flag = "F";

        local.Final.Index = 0;
        local.Final.CheckSize();

        while(local.Final.Index + 1 < local.Final.Count)
        {
          // final sort on contractors and cities summaries
          local.Swap.Flag = "N";
          local.Temp1.SystemGeneratedId =
            local.Final.Item.Final1.SystemGeneratedId;
          MoveCseOrganization(local.Final.Item.JdFinal, local.JdTemp1);
          local.ContractorTemp1.Assign(local.Final.Item.ContractorFinal);
          local.CountTemp1.Count = local.Final.Item.CountFinal.Count;

          ++local.Final.Index;
          local.Final.CheckSize();

          local.Temp2.SystemGeneratedId =
            local.Final.Item.Final1.SystemGeneratedId;
          MoveCseOrganization(local.Final.Item.JdFinal, local.JdTemp2);
          local.ContrtactorTemp2.Assign(local.Final.Item.ContractorFinal);
          local.CountTemp2.Count = local.Final.Item.CountFinal.Count;

          if (Lt(local.ContrtactorTemp2.Name, local.ContractorTemp1.Name))
          {
            local.Swap.Flag = "Y";
          }
          else if (Equal(local.ContrtactorTemp2.Name, local.ContractorTemp1.Name)
            && local.Temp2.SystemGeneratedId < local.Temp1.SystemGeneratedId)
          {
            local.Swap.Flag = "Y";
          }

          if (AsChar(local.Swap.Flag) == 'Y')
          {
            --local.Final.Index;
            local.Final.CheckSize();

            local.Final.Update.Final1.SystemGeneratedId =
              local.Temp2.SystemGeneratedId;
            local.Final.Update.ContractorFinal.Assign(local.ContrtactorTemp2);
            MoveCseOrganization(local.JdTemp2, local.Final.Update.JdFinal);
            local.Final.Update.CountFinal.Count = local.CountTemp2.Count;

            ++local.Final.Index;
            local.Final.CheckSize();

            local.Final.Update.Final1.SystemGeneratedId =
              local.Temp1.SystemGeneratedId;
            local.Final.Update.ContractorFinal.Assign(local.ContractorTemp1);
            MoveCseOrganization(local.JdTemp1, local.Final.Update.JdFinal);
            local.Final.Update.CountFinal.Count = local.CountTemp1.Count;
            local.Changed.Flag = "T";
          }
        }
      }

      local.Contractor.Text21 = "";
      local.Count1.Text10 = "";
      local.Write.TextLine80 = local.Contractor.Text21 + " " + local
        .Count1.Text10;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriterSmall();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }

      local.Contractor.Text21 = "";
      local.Count1.Text10 = "";
      local.Write.TextLine80 = local.Contractor.Text21 + " " + local
        .Count1.Text10;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriterSmall();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }

      local.Contractor.Text21 = " Contractor/Office";
      local.Count1.Text10 = "Total";
      local.Write.TextLine80 = local.Contractor.Text21 + ";" + local
        .Count1.Text10;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriterSmall();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }

      for(local.Final.Index = 0; local.Final.Index < local.Final.Count; ++
        local.Final.Index)
      {
        if (!local.Final.CheckSize())
        {
          break;
        }

        if (local.Final.Item.Final1.SystemGeneratedId <= 0)
        {
          local.Contractor.Text21 = local.Final.Item.ContractorFinal.Name;
        }
        else
        {
          local.Contractor.Text21 =
            NumberToString(local.Final.Item.Final1.SystemGeneratedId, 12, 4);
        }

        local.Count1.Text10 =
          NumberToString(local.Final.Item.CountFinal.Count, 6, 10);
        local.Contractor.Text21 = " " + local.Contractor.Text21;
        local.Start.Count = Verify(local.Count1.Text10, "123456789");
        local.Count1.Text10 =
          Substring(local.Count1.Text10, local.Start.Count, 10 - local
          .Start.Count + 1);
        local.Write.TextLine80 = local.Contractor.Text21 + ";" + local
          .Count1.Text10;
        local.EabFileHandling.Action = "WRITE";
        UseEabExternalReportWriterSmall();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          ExitState = "FILE_READ_ERROR_WITH_RB";
        }
      }

      local.Final.CheckIndex();
      local.Contractor.Text21 = " Grand Total";
      local.Count1.Text10 = NumberToString(local.CountFinal.Count, 6, 10);
      local.Start.Count = Verify(local.Count1.Text10, "123456789");
      local.Count1.Text10 =
        Substring(local.Count1.Text10, local.Start.Count, 10 - local
        .Start.Count + 1);
      local.Write.TextLine80 = local.Contractor.Text21 + ";" + local
        .Count1.Text10;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriterSmall();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }
    }
    else
    {
      local.EabReportSend.RptDetail = local.Contractor.Text21 + "  " + local
        .Count1.Text10;
      local.EabReportSend.RptDetail =
        "                                    ***********No records found********";
        
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriter1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseSpB737Close();
      local.EabFileHandling.Action = "CLOSE";
      UseEabExternalReportWriter2();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseEabExternalReportWriterSmall();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseSpB737Close();
      local.EabFileHandling.Action = "CLOSE";
      UseEabExternalReportWriter2();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseEabExternalReportWriterSmall();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.CurrentDate.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.Last.Date = useExport.Last.Date;
    local.BeginningOfTheMonth.Date = useExport.First.Date;
  }

  private void UseEabExternalReportWriter1()
  {
    var useImport = new EabExternalReportWriter.Import();
    var useExport = new EabExternalReportWriter.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.RptDetail = local.EabReportSend.RptDetail;
    MoveExternal(local.External, useExport.External);

    Call(EabExternalReportWriter.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseEabExternalReportWriter2()
  {
    var useImport = new EabExternalReportWriter.Import();
    var useExport = new EabExternalReportWriter.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveExternal(local.External, useExport.External);

    Call(EabExternalReportWriter.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseEabExternalReportWriterSmall()
  {
    var useImport = new EabExternalReportWriterSmall.Import();
    var useExport = new EabExternalReportWriterSmall.Export();

    useImport.External.TextLine80 = local.Write.TextLine80;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveExternal(local.External, useExport.External);

    Call(EabExternalReportWriterSmall.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnB734DetermineJdFromCase()
  {
    var useImport = new FnB734DetermineJdFromCase.Import();
    var useExport = new FnB734DetermineJdFromCase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.ReportEndDate.Date = local.Last.Date;

    Call(FnB734DetermineJdFromCase.Execute, useImport, useExport);

    local.DashboardAuditData.Assign(useExport.DashboardAuditData);
  }

  private void UseSpB737Close()
  {
    var useImport = new SpB737Close.Import();
    var useExport = new SpB737Close.Export();

    useImport.NumberOfRecordsRead.Count = local.NumberOfRecordsRead.Count;

    Call(SpB737Close.Execute, useImport, useExport);
  }

  private void UseSpB737Housekeeping()
  {
    var useImport = new SpB737Housekeeping.Import();
    var useExport = new SpB737Housekeeping.Export();

    Call(SpB737Housekeeping.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadCaseCaseRolePersonProgramOfficeServiceProvider()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseAssignment.Populated = false;
    entities.PersonProgram.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseCaseRolePersonProgramOfficeServiceProvider",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.CaseRole.CspNumber = db.GetString(reader, 3);
        entities.CsePerson.Number = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.PersonProgram.CspNumber = db.GetString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 9);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 10);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 11);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 12);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 13);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 13);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 13);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 14);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 14);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 14);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 15);
        entities.CaseAssignment.OspCode = db.GetString(reader, 15);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 16);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 16);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 17);
        entities.ServiceProvider.LastName = db.GetString(reader, 18);
        entities.ServiceProvider.FirstName = db.GetString(reader, 19);
        entities.Office.TypeCode = db.GetString(reader, 20);
        entities.Office.EffectiveDate = db.GetDate(reader, 21);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 22);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 23);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 24);
        entities.CaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 25);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 26);
        entities.CsePerson.Type1 = db.GetString(reader, 27);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = true;
        entities.PersonProgram.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCseOrganization()
  {
    entities.Contractor.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(
          command, "cogParentCode",
          local.DashboardAuditData.JudicialDistrict ?? "");
      },
      (db, reader) =>
      {
        entities.Contractor.Code = db.GetString(reader, 0);
        entities.Contractor.Type1 = db.GetString(reader, 1);
        entities.Contractor.Name = db.GetString(reader, 2);
        entities.Contractor.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOffice()
  {
    entities.Office.Populated = false;

    return ReadEach("ReadOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.BeginningOfTheMonth.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.EffectiveDate = db.GetDate(reader, 2);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 4);
        entities.Office.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgramCsePerson()
  {
    entities.N2dReadCsePerson.Populated = false;
    entities.N2dReadPersonProgram.Populated = false;

    return Read("ReadPersonProgramCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.N2dReadPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.N2dReadCsePerson.Number = db.GetString(reader, 0);
        entities.N2dReadPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.N2dReadPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.N2dReadPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.N2dReadPersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.N2dReadCsePerson.Type1 = db.GetString(reader, 5);
        entities.N2dReadCsePerson.Populated = true;
        entities.N2dReadPersonProgram.Populated = true;
        CheckValid<CsePerson>("Type1", entities.N2dReadCsePerson.Type1);
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
    /// <summary>A FinalGroup group.</summary>
    [Serializable]
    public class FinalGroup
    {
      /// <summary>
      /// A value of Final1.
      /// </summary>
      [JsonPropertyName("final1")]
      public Office Final1
      {
        get => final1 ??= new();
        set => final1 = value;
      }

      /// <summary>
      /// A value of JdFinal.
      /// </summary>
      [JsonPropertyName("jdFinal")]
      public CseOrganization JdFinal
      {
        get => jdFinal ??= new();
        set => jdFinal = value;
      }

      /// <summary>
      /// A value of ContractorFinal.
      /// </summary>
      [JsonPropertyName("contractorFinal")]
      public CseOrganization ContractorFinal
      {
        get => contractorFinal ??= new();
        set => contractorFinal = value;
      }

      /// <summary>
      /// A value of CountFinal.
      /// </summary>
      [JsonPropertyName("countFinal")]
      public Common CountFinal
      {
        get => countFinal ??= new();
        set => countFinal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Office final1;
      private CseOrganization jdFinal;
      private CseOrganization contractorFinal;
      private Common countFinal;
    }

    /// <summary>A SortedGroup group.</summary>
    [Serializable]
    public class SortedGroup
    {
      /// <summary>
      /// A value of Sorted1.
      /// </summary>
      [JsonPropertyName("sorted1")]
      public Office Sorted1
      {
        get => sorted1 ??= new();
        set => sorted1 = value;
      }

      /// <summary>
      /// A value of JdSorted.
      /// </summary>
      [JsonPropertyName("jdSorted")]
      public CseOrganization JdSorted
      {
        get => jdSorted ??= new();
        set => jdSorted = value;
      }

      /// <summary>
      /// A value of ContractorSorted.
      /// </summary>
      [JsonPropertyName("contractorSorted")]
      public CseOrganization ContractorSorted
      {
        get => contractorSorted ??= new();
        set => contractorSorted = value;
      }

      /// <summary>
      /// A value of CountSorted.
      /// </summary>
      [JsonPropertyName("countSorted")]
      public Common CountSorted
      {
        get => countSorted ??= new();
        set => countSorted = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Office sorted1;
      private CseOrganization jdSorted;
      private CseOrganization contractorSorted;
      private Common countSorted;
    }

    /// <summary>A CountGroup group.</summary>
    [Serializable]
    public class CountGroup
    {
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
      /// A value of Jd.
      /// </summary>
      [JsonPropertyName("jd")]
      public CseOrganization Jd
      {
        get => jd ??= new();
        set => jd = value;
      }

      /// <summary>
      /// A value of Contractor.
      /// </summary>
      [JsonPropertyName("contractor")]
      public CseOrganization Contractor
      {
        get => contractor ??= new();
        set => contractor = value;
      }

      /// <summary>
      /// A value of Count1.
      /// </summary>
      [JsonPropertyName("count1")]
      public Common Count1
      {
        get => count1 ??= new();
        set => count1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Office office;
      private CseOrganization jd;
      private CseOrganization contractor;
      private Common count1;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public External Write
    {
      get => write ??= new();
      set => write = value;
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
    /// A value of RecordsFound.
    /// </summary>
    [JsonPropertyName("recordsFound")]
    public Common RecordsFound
    {
      get => recordsFound ??= new();
      set => recordsFound = value;
    }

    /// <summary>
    /// A value of BeginningOfPeriod.
    /// </summary>
    [JsonPropertyName("beginningOfPeriod")]
    public DateWorkArea BeginningOfPeriod
    {
      get => beginningOfPeriod ??= new();
      set => beginningOfPeriod = value;
    }

    /// <summary>
    /// A value of Changed.
    /// </summary>
    [JsonPropertyName("changed")]
    public Common Changed
    {
      get => changed ??= new();
      set => changed = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Common Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Count1.
    /// </summary>
    [JsonPropertyName("count1")]
    public WorkArea Count1
    {
      get => count1 ??= new();
      set => count1 = value;
    }

    /// <summary>
    /// A value of Contractor.
    /// </summary>
    [JsonPropertyName("contractor")]
    public WorkArea Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
    }

    /// <summary>
    /// A value of GrpTotal.
    /// </summary>
    [JsonPropertyName("grpTotal")]
    public Common GrpTotal
    {
      get => grpTotal ??= new();
      set => grpTotal = value;
    }

    /// <summary>
    /// A value of CountFinal.
    /// </summary>
    [JsonPropertyName("countFinal")]
    public Common CountFinal
    {
      get => countFinal ??= new();
      set => countFinal = value;
    }

    /// <summary>
    /// Gets a value of Final.
    /// </summary>
    [JsonIgnore]
    public Array<FinalGroup> Final => final ??= new(FinalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Final for json serialization.
    /// </summary>
    [JsonPropertyName("final")]
    [Computed]
    public IList<FinalGroup> Final_Json
    {
      get => final;
      set => Final.Assign(value);
    }

    /// <summary>
    /// A value of CountTemp2.
    /// </summary>
    [JsonPropertyName("countTemp2")]
    public Common CountTemp2
    {
      get => countTemp2 ??= new();
      set => countTemp2 = value;
    }

    /// <summary>
    /// A value of ContrtactorTemp2.
    /// </summary>
    [JsonPropertyName("contrtactorTemp2")]
    public CseOrganization ContrtactorTemp2
    {
      get => contrtactorTemp2 ??= new();
      set => contrtactorTemp2 = value;
    }

    /// <summary>
    /// A value of JdTemp2.
    /// </summary>
    [JsonPropertyName("jdTemp2")]
    public CseOrganization JdTemp2
    {
      get => jdTemp2 ??= new();
      set => jdTemp2 = value;
    }

    /// <summary>
    /// A value of Temp2.
    /// </summary>
    [JsonPropertyName("temp2")]
    public Office Temp2
    {
      get => temp2 ??= new();
      set => temp2 = value;
    }

    /// <summary>
    /// A value of CountTemp1.
    /// </summary>
    [JsonPropertyName("countTemp1")]
    public Common CountTemp1
    {
      get => countTemp1 ??= new();
      set => countTemp1 = value;
    }

    /// <summary>
    /// A value of ContractorTemp1.
    /// </summary>
    [JsonPropertyName("contractorTemp1")]
    public CseOrganization ContractorTemp1
    {
      get => contractorTemp1 ??= new();
      set => contractorTemp1 = value;
    }

    /// <summary>
    /// A value of JdTemp1.
    /// </summary>
    [JsonPropertyName("jdTemp1")]
    public CseOrganization JdTemp1
    {
      get => jdTemp1 ??= new();
      set => jdTemp1 = value;
    }

    /// <summary>
    /// A value of Temp1.
    /// </summary>
    [JsonPropertyName("temp1")]
    public Office Temp1
    {
      get => temp1 ??= new();
      set => temp1 = value;
    }

    /// <summary>
    /// A value of Swap.
    /// </summary>
    [JsonPropertyName("swap")]
    public Common Swap
    {
      get => swap ??= new();
      set => swap = value;
    }

    /// <summary>
    /// Gets a value of Sorted.
    /// </summary>
    [JsonIgnore]
    public Array<SortedGroup> Sorted => sorted ??= new(SortedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Sorted for json serialization.
    /// </summary>
    [JsonPropertyName("sorted")]
    [Computed]
    public IList<SortedGroup> Sorted_Json
    {
      get => sorted;
      set => Sorted.Assign(value);
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public TextWorkArea Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of StatusDateWorkArea.
    /// </summary>
    [JsonPropertyName("statusDateWorkArea")]
    public WorkArea StatusDateWorkArea
    {
      get => statusDateWorkArea ??= new();
      set => statusDateWorkArea = value;
    }

    /// <summary>
    /// A value of EffDateWorkArea.
    /// </summary>
    [JsonPropertyName("effDateWorkArea")]
    public WorkArea EffDateWorkArea
    {
      get => effDateWorkArea ??= new();
      set => effDateWorkArea = value;
    }

    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public DateWorkArea Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// Gets a value of Count.
    /// </summary>
    [JsonIgnore]
    public Array<CountGroup> Count => count ??= new(CountGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Count for json serialization.
    /// </summary>
    [JsonPropertyName("count")]
    [Computed]
    public IList<CountGroup> Count_Json
    {
      get => count;
      set => Count.Assign(value);
    }

    /// <summary>
    /// A value of StatusDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("statusDateDateWorkArea")]
    public DateWorkArea StatusDateDateWorkArea
    {
      get => statusDateDateWorkArea ??= new();
      set => statusDateDateWorkArea = value;
    }

    /// <summary>
    /// A value of BeginningOfTheMonth.
    /// </summary>
    [JsonPropertyName("beginningOfTheMonth")]
    public DateWorkArea BeginningOfTheMonth
    {
      get => beginningOfTheMonth ??= new();
      set => beginningOfTheMonth = value;
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
    /// A value of EffDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("effDateDateWorkArea")]
    public DateWorkArea EffDateDateWorkArea
    {
      get => effDateDateWorkArea ??= new();
      set => effDateDateWorkArea = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public TextWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public TextWorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public TextWorkArea Day
    {
      get => day ??= new();
      set => day = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public WorkArea Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of CasePersonNumber.
    /// </summary>
    [JsonPropertyName("casePersonNumber")]
    public WorkArea CasePersonNumber
    {
      get => casePersonNumber ??= new();
      set => casePersonNumber = value;
    }

    /// <summary>
    /// A value of EffDateOffice.
    /// </summary>
    [JsonPropertyName("effDateOffice")]
    public WorkArea EffDateOffice
    {
      get => effDateOffice ??= new();
      set => effDateOffice = value;
    }

    private External write;
    private External external;
    private Common recordsFound;
    private DateWorkArea beginningOfPeriod;
    private Common changed;
    private Common start;
    private WorkArea count1;
    private WorkArea contractor;
    private Common grpTotal;
    private Common countFinal;
    private Array<FinalGroup> final;
    private Common countTemp2;
    private CseOrganization contrtactorTemp2;
    private CseOrganization jdTemp2;
    private Office temp2;
    private Common countTemp1;
    private CseOrganization contractorTemp1;
    private CseOrganization jdTemp1;
    private Office temp1;
    private Common swap;
    private Array<SortedGroup> sorted;
    private TextWorkArea case1;
    private WorkArea statusDateWorkArea;
    private WorkArea effDateWorkArea;
    private DashboardAuditData dashboardAuditData;
    private DateWorkArea last;
    private Array<CountGroup> count;
    private DateWorkArea statusDateDateWorkArea;
    private DateWorkArea beginningOfTheMonth;
    private DateWorkArea zeroDate;
    private DateWorkArea effDateDateWorkArea;
    private DateWorkArea currentDate;
    private WorkArea workArea;
    private AbendData abendData;
    private Common numberOfRecordsRead;
    private ExitStateWorkArea exitStateWorkArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private TextWorkArea year;
    private TextWorkArea month;
    private TextWorkArea day;
    private WorkArea name;
    private WorkArea casePersonNumber;
    private WorkArea effDateOffice;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of JudicalDistrict.
    /// </summary>
    [JsonPropertyName("judicalDistrict")]
    public CseOrganization JudicalDistrict
    {
      get => judicalDistrict ??= new();
      set => judicalDistrict = value;
    }

    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of Contractor.
    /// </summary>
    [JsonPropertyName("contractor")]
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
    }

    /// <summary>
    /// A value of N2dReadCsePerson.
    /// </summary>
    [JsonPropertyName("n2dReadCsePerson")]
    public CsePerson N2dReadCsePerson
    {
      get => n2dReadCsePerson ??= new();
      set => n2dReadCsePerson = value;
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
    /// A value of N2dReadPersonProgram.
    /// </summary>
    [JsonPropertyName("n2dReadPersonProgram")]
    public PersonProgram N2dReadPersonProgram
    {
      get => n2dReadPersonProgram ??= new();
      set => n2dReadPersonProgram = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CseOrganization judicalDistrict;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization contractor;
    private CsePerson n2dReadCsePerson;
    private Program program;
    private PersonProgram n2dReadPersonProgram;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private PersonProgram personProgram;
    private CaseRole caseRole;
    private CsePerson csePerson;
  }
#endregion
}
