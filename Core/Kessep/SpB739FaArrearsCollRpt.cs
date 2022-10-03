// Program: SP_B739_FA_ARREARS_COLL_RPT, ID: 1625373422, model: 746.
// Short name: SWEB739P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B739_FA_ARREARS_COLL_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB739FaArrearsCollRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B739_FA_ARREARS_COLL_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB739FaArrearsCollRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB739FaArrearsCollRpt.
  /// </summary>
  public SpB739FaArrearsCollRpt(IContext context, Import import, Export export):
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
    // 10/22/2019      DDupree   	Initial Creation - CQ65984
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ***********************************************************************************************
    // this is a copy of the sp b737 food assistance report
    // ***********************************************************************************************
    UseSpB739HouseKeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.Start.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;
    local.IncludeArrearsOnly.Flag = "";
    local.ProgramMinium.Date = new DateTime(2015, 7, 1);

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
              local.FiscalYear.Text1 = "S";
            }
            else
            {
              local.FiscalYear.Text1 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 2:
            if (local.Current.Count == 1)
            {
              local.StartDate.Date = Now().Date;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
              local.StartDate.Date = StringToDate(local.WorkArea.Text15);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          default:
            break;
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

    if (AsChar(local.FiscalYear.Text1) == 'S')
    {
      if (Lt(local.StartDate.Date, new DateTime(2015, 7, 1)))
      {
        ExitState = "DATE_MUST_BE_AFTER_SFY";

        return;
      }
    }
    else if (Lt(local.StartDate.Date, new DateTime(2015, 7, 1)))
    {
      ExitState = "DATE_MUST_BE_AFTER_FFY";

      return;
    }

    local.SetUp.FiscalInd = local.FiscalYear.Text1;

    if (AsChar(local.FiscalYear.Text1) == 'S')
    {
      // 7/1 to 6/30
      local.DateWorkArea.Year = Year(local.StartDate.Date);
      local.DateWorkArea.Month = Month(local.StartDate.Date);

      if (local.DateWorkArea.Month < 7)
      {
      }
      else
      {
        ++local.DateWorkArea.Year;
      }

      local.Date.Text24 = "State Fiscal Year " + NumberToString
        (local.DateWorkArea.Year, 12, 4);
      local.DateWorkArea.Year = Year(local.StartDate.Date);
      local.DateWorkArea.Month = Month(local.StartDate.Date);
      local.DateWorkAttributes.TextMonth = "07";
      local.DateWorkAttributes.TextDay = "01";
      local.SetUp.FiscalYear = NumberToString(local.DateWorkArea.Year, 12, 4);

      if (local.DateWorkArea.Month < 7)
      {
        --local.DateWorkArea.Year;
      }
      else
      {
      }

      local.DateWorkAttributes.TextYear =
        NumberToString(local.DateWorkArea.Year, 12, 4);
      local.Begin.Date = StringToDate(local.DateWorkAttributes.TextMonth + "/"
        + local.DateWorkAttributes.TextDay + "/"
        + local.DateWorkAttributes.TextYear);
      local.Begin.Timestamp = Timestamp(local.DateWorkAttributes.TextYear + "-"
        + local.DateWorkAttributes.TextMonth + "-" + local
        .DateWorkAttributes.TextDay + "-" + ".01.01.01.000000");
      local.DateWorkArea.Year = Year(local.StartDate.Date);
      local.DateWorkAttributes.TextMonth = "06";
      local.DateWorkAttributes.TextDay = "30";

      if (local.DateWorkArea.Month < 7)
      {
      }
      else
      {
        ++local.DateWorkArea.Year;
      }

      local.DateWorkAttributes.TextYear =
        NumberToString(local.DateWorkArea.Year, 12, 4);
      local.End.Date = StringToDate(local.DateWorkAttributes.TextMonth + "/" + local
        .DateWorkAttributes.TextDay + "/" + local.DateWorkAttributes.TextYear);
      local.End.Timestamp = Timestamp(local.DateWorkAttributes.TextYear + "-"
        + local.DateWorkAttributes.TextMonth + "-" + local
        .DateWorkAttributes.TextDay + "-" + ".23.59.59.999999");
    }
    else if (!Lt(local.StartDate.Date, new DateTime(2015, 7, 1)) && !
      Lt(new DateTime(2015, 9, 30), local.StartDate.Date))
    {
      // special 1st time fed only 07/01/2015 - 09/30/2015
      local.DateWorkArea.Year = Year(local.StartDate.Date);
      local.DateWorkAttributes.TextMonth = "07";
      local.DateWorkAttributes.TextDay = "01";
      local.DateWorkAttributes.TextYear =
        NumberToString(local.DateWorkArea.Year, 12, 4);
      local.Begin.Date = StringToDate(local.DateWorkAttributes.TextMonth + "/"
        + local.DateWorkAttributes.TextDay + "/"
        + local.DateWorkAttributes.TextYear);
      local.Begin.Timestamp = Timestamp(local.DateWorkAttributes.TextYear + "-"
        + local.DateWorkAttributes.TextMonth + "-" + local
        .DateWorkAttributes.TextDay + "-" + ".01.01.01.000000");
      local.DateWorkArea.Year = Year(local.StartDate.Date);
      local.DateWorkArea.Year = Year(local.StartDate.Date);
      local.DateWorkAttributes.TextMonth = "09";
      local.DateWorkAttributes.TextDay = "30";
      local.Date.Text24 = "Federal Fiscal Year " + NumberToString
        (local.DateWorkArea.Year, 12, 4);
      local.DateWorkAttributes.TextYear =
        NumberToString(local.DateWorkArea.Year, 12, 4);
      local.End.Date = StringToDate(local.DateWorkAttributes.TextMonth + "/" + local
        .DateWorkAttributes.TextDay + "/" + local.DateWorkAttributes.TextYear);
      local.End.Timestamp = Timestamp(local.DateWorkAttributes.TextYear + "-"
        + local.DateWorkAttributes.TextMonth + "-" + local
        .DateWorkAttributes.TextDay + "-" + ".23.59.59.999999");
    }
    else
    {
      // 10/1 to 9/30
      local.DateWorkArea.Year = Year(local.StartDate.Date);
      local.DateWorkArea.Month = Month(local.StartDate.Date);
      local.DateWorkAttributes.TextMonth = "10";
      local.DateWorkAttributes.TextDay = "01";
      local.SetUp.FiscalYear = NumberToString(local.DateWorkArea.Year, 12, 4);

      if (local.DateWorkArea.Month < 10)
      {
        --local.DateWorkArea.Year;
      }
      else
      {
      }

      local.DateWorkAttributes.TextYear =
        NumberToString(local.DateWorkArea.Year, 12, 4);
      local.Begin.Date = StringToDate(local.DateWorkAttributes.TextMonth + "/"
        + local.DateWorkAttributes.TextDay + "/"
        + local.DateWorkAttributes.TextYear);
      local.Begin.Timestamp = Timestamp(local.DateWorkAttributes.TextYear + "-"
        + local.DateWorkAttributes.TextMonth + "-" + local
        .DateWorkAttributes.TextDay + "-" + ".01.01.01.000000");
      local.DateWorkArea.Year = Year(local.StartDate.Date);
      local.DateWorkAttributes.TextMonth = "09";
      local.DateWorkAttributes.TextDay = "30";

      if (local.DateWorkArea.Month < 10)
      {
      }
      else
      {
        ++local.DateWorkArea.Year;
      }

      local.Date.Text24 = "Federal Fiscal Year " + NumberToString
        (local.DateWorkArea.Year, 12, 4);
      local.DateWorkAttributes.TextYear =
        NumberToString(local.DateWorkArea.Year, 12, 4);
      local.End.Date = StringToDate(local.DateWorkAttributes.TextMonth + "/" + local
        .DateWorkAttributes.TextDay + "/" + local.DateWorkAttributes.TextYear);
      local.End.Timestamp = Timestamp(local.DateWorkAttributes.TextYear + "-"
        + local.DateWorkAttributes.TextMonth + "-" + local
        .DateWorkAttributes.TextDay + "-" + ".23.59.59.999999");
    }

    // DETAIL HEADING
    local.OpenEabReportSend.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.OpenEabReportSend.BlankLineAfterHeading = "Y";
    local.OpenEabReportSend.ProgramName = "SWEPB739";
    local.OpenEabReportSend.NumberOfColHeadings = 0;
    local.RtpDetail.Text200 =
      " Annual Food Assistance Detail Arrears Rpt - " + local.Date.Text24;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriterLarge2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.RtpDetail.Text200 = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriterLarge2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Header.Text40 = "Closure   ;Closure ;Non-Coop;Non-Coop;";
    local.EabReportSend.RptDetail =
      " Office   ;Contractor      ; Case #   ;Case Open   ;Child     ;Standard   ; Est ;Program   ;Worker Last  ;Worker First  ;Arrears ;";
      
    local.RtpDetail.Text200 = local.EabReportSend.RptDetail + local
      .Header.Text40;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriterLarge2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Header.Text40 = "Date     ;Reason;FA time;End date;";
    local.EabReportSend.RptDetail =
      "          ;                ;          ; Date       ; Person # ;Number     ; By  ;Eff Date  ;Name        ;Name          ;Amt      ;";
      
    local.RtpDetail.Text200 = local.EabReportSend.RptDetail + local
      .Header.Text40;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriterLarge2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // SUMMARY HEADING
    local.Report.TextLine80 =
      " Summary of the Annual Food Assistance Arrears Rpt - " + local
      .Date.Text24;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriterSmall2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 2nd report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Report.TextLine80 = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriterSmall2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 2nd report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.CurrentDate.Date = local.Begin.Date;
    local.NumberOfRecordsRead.Count = 0;
    local.CurrentDate1.Date = local.ProgramProcessingInfo.ProcessDate;
    local.BeginningOfPeriod.Date = AddDays(local.CurrentDate1.Date, -45);
    UseCabFirstAndLastDateOfMonth();
    local.SetUp.RunNumber = 0;

    if (ReadFaArrearsCollections2())
    {
      local.SetUp.RunNumber = entities.FaArrearsCollections.RunNumber + 1;
    }

    if (local.SetUp.RunNumber <= 0)
    {
      local.SetUp.RunNumber = 1;
    }

    local.Count.Index = -1;
    local.Count.Count = 0;

    foreach(var item in ReadOffice2())
    {
      ++local.Count.Index;
      local.Count.CheckSize();

      local.Count.Update.Office.SystemGeneratedId =
        entities.Office.SystemGeneratedId;
    }

    local.RecordsFound.Flag = "";

    foreach(var item in ReadCaseCaseRolePersonProgramOfficeServiceProvider())
    {
      local.Local2NdCaseReadFound.Flag = "";
      local.OpenCase.CseOpenDate = local.ZeroDate.Date;
      MoveInfrastructure(local.ClearInfrastructure, local.Infrastructure);

      if (ReadInfrastructure2())
      {
        MoveInfrastructure(entities.Infrastructure, local.Infrastructure);
      }

      if (!IsEmpty(local.Infrastructure.Detail))
      {
        if (ReadCase())
        {
          local.OpenCase.CseOpenDate = entities.Infrastructure.ReferenceDate;
          local.Local2NdCaseReadFound.Flag = "Y";
        }
      }

      if (IsEmpty(local.Local2NdCaseReadFound.Flag))
      {
        continue;
      }

      if (Lt(entities.CaseRole.EndDate, local.OpenCase.CseOpenDate))
      {
        continue;
      }

      ReadPersonProgramCsePerson();

      if (!Lt(local.ZeroDate.Date, entities.N2dReadPersonProgram.EffectiveDate))
      {
        continue;
      }

      if (Lt(local.OpenCase.CseOpenDate,
        entities.N2dReadPersonProgram.EffectiveDate))
      {
        continue;
      }

      if (Lt(entities.N2dReadPersonProgram.EffectiveDate,
        local.ProgramMinium.Date))
      {
        // we do not want any programs that were opened before 07/01/2015
        continue;
      }

      if (Equal(entities.PersonProgram.EffectiveDate,
        entities.N2dReadPersonProgram.EffectiveDate) && Equal
        (entities.PersonProgram.DiscontinueDate,
        entities.N2dReadPersonProgram.DiscontinueDate))
      {
      }
      else
      {
        // to stop mutiple open and close fs programs for the same case/ch
        continue;
      }

      if (ReadCsePersonPersonProgram())
      {
        if (!Lt(entities.N2dReadPersonProgram.EffectiveDate,
          entities.N3dReadPersonProgram.EffectiveDate))
        {
          continue;
        }

        if (!Lt(local.OpenCase.CseOpenDate,
          entities.N3dReadPersonProgram.EffectiveDate) && !
          Lt(entities.N3dReadPersonProgram.DiscontinueDate,
          local.OpenCase.CseOpenDate))
        {
          continue;
        }
      }
      else
      {
        // we do not want any ,so this is ok
      }

      UseCabDate2TextWithHyphens2();
      local.AmtOwed.TotalCurrency = 0;

      foreach(var item1 in ReadLegalActionObligationDebtObligor())
      {
        if (!Lt(entities.LegalAction.FiledDate,
          entities.N2dReadPersonProgram.EffectiveDate))
        {
          continue;
        }

        if (Equal(entities.LegalAction.EstablishmentCode, "CT") || Equal
          (entities.LegalAction.EstablishmentCode, "CS") || Equal
          (entities.LegalAction.EstablishmentCode, "PR") || Equal
          (entities.LegalAction.EstablishmentCode, "OS"))
        {
        }
        else
        {
          continue;
        }

        local.ObligationCreate.Date = Date(entities.Obligation.CreatedTmst);
        local.CaseMatch.Flag = "";

        foreach(var item2 in ReadCaseCaseRoleCsePersonLegalActionCaseRole())
        {
          if (Equal(entities.ApCase.Number, entities.Case1.Number))
          {
            local.CaseMatch.Flag = "Y";

            break;
          }
        }

        if (AsChar(local.CaseMatch.Flag) == 'Y')
        {
          local.AmtOwed.TotalCurrency += entities.Debt.Amount;
        }
      }

      if (local.AmtOwed.TotalCurrency <= 0)
      {
        continue;

        // no money owed so skipp
      }

      local.FaArrearsCollections.Assign(local.ClearFaArrearsCollections);
      MoveFaArrearsCollections(local.SetUp, local.FaArrearsCollections);
      local.FaArrearsCollections.ArrearsAmountDue = local.AmtOwed.TotalCurrency;
      local.FaArrearsCollections.CaseNumber = entities.N2dReadCase.Number;
      local.FaArrearsCollections.CaseOpenDate = local.OpenCase.CseOpenDate;
      local.FaArrearsCollections.ChildPersonNumber = entities.CsePerson.Number;
      local.FaArrearsCollections.CourtOrderEstBy =
        entities.LegalAction.EstablishmentCode;
      local.FaArrearsCollections.StandardNumber =
        entities.LegalAction.StandardNumber;
      local.FaArrearsCollections.FsStartDate =
        entities.N2dReadPersonProgram.EffectiveDate;
      local.FaArrearsCollections.Office = entities.Office.Name;
      local.FaArrearsCollections.CaseworkerLastName =
        entities.ServiceProvider.LastName;
      local.FaArrearsCollections.CaseworkerFirstName =
        entities.ServiceProvider.FirstName;
      local.FaArrearsCollections.CreatedBy = global.UserId;
      local.FaArrearsCollections.CreatedTstamp = Now();
      local.DashboardAuditData.JudicialDistrict = "";
      UseFnB734DetermineJdFromCase();

      if (IsEmpty(local.DashboardAuditData.JudicialDistrict))
      {
        if (ReadCseOrganizationOffice())
        {
          local.DashboardAuditData.JudicialDistrict =
            entities.JudicialDistrict.Code;
          local.DashboardAuditData.Office = entities.Office.SystemGeneratedId;
        }
        else
        {
          // --  Write to error report...
          UseCabDate2TextWithHyphens1();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "FN_B734_DETERMINE_JD_FROM_CASE - Error finding office/judicial district for case " +
            entities.N2dReadCase.Number + ".  Rpt End Date = " + local
            .TextWorkArea.Text10;
          UseCabErrorReport1();

          return;
        }
      }

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

          if (ReadOffice1())
          {
            local.Count.Update.Jd.Name = entities.N2dReadOffice.Name;
          }

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
            local.Count.Update.Count1.TotalCurrency =
              local.Count.Item.Count1.TotalCurrency + local
              .FaArrearsCollections.ArrearsAmountDue.GetValueOrDefault();
            local.FaArrearsCollections.Contractor = entities.Contractor.Name;

            goto Read;
          }
        }

        local.Count.CheckIndex();
      }

Read:

      MoveInfrastructure(local.ClearInfrastructure, local.Infrastructure);

      if (ReadInfrastructure1())
      {
        MoveInfrastructure(entities.Infrastructure, local.Infrastructure);
      }

      if (!IsEmpty(local.Infrastructure.Detail))
      {
        local.FaArrearsCollections.CaseClosedDate =
          local.Infrastructure.ReferenceDate;
        local.FaArrearsCollections.CaseClosureReason =
          Substring(local.Infrastructure.Detail, 41, 2);
      }

      if (Lt(new DateTime(1, 1, 1), local.FaArrearsCollections.CaseClosedDate))
      {
        local.ClosedDateWorkArea.Year =
          Year(local.FaArrearsCollections.CaseClosedDate);
        local.ClosedDateWorkArea.Month =
          Month(local.FaArrearsCollections.CaseClosedDate);
        local.ClosedDateWorkArea.Day =
          Day(local.FaArrearsCollections.CaseClosedDate);
        local.ClosedDateWorkAttributes.TextDay =
          NumberToString(local.ClosedDateWorkArea.Day, 14, 2);
        local.ClosedDateWorkAttributes.TextMonth =
          NumberToString(local.ClosedDateWorkArea.Month, 14, 2);
        local.ClosedDateWorkAttributes.TextYear =
          NumberToString(local.ClosedDateWorkArea.Year, 12, 4);
        local.ClosedDateWorkAttributes.TextDate10Char =
          local.ClosedDateWorkAttributes.TextMonth + "/" + local
          .ClosedDateWorkAttributes.TextDay + "/" + local
          .ClosedDateWorkAttributes.TextYear;
      }
      else
      {
        local.ClosedDateWorkAttributes.TextDate10Char = "";
      }

      local.NonCooperation.Code = local.ClearNonCooperation.Code;

      if (Lt(new DateTime(1, 1, 1), local.FaArrearsCollections.CaseClosedDate))
      {
        if (ReadNonCooperation1())
        {
          local.NonCooperation.Code = entities.NonCooperation.Code;
        }

        if (AsChar(local.NonCooperation.Code) == 'Y')
        {
          local.FaArrearsCollections.NonCoopCd = "N";
        }
        else if (!IsEmpty(local.NonCooperation.Code))
        {
          local.FaArrearsCollections.NonCoopCd = "C";
        }
      }

      local.NonCooperation.Code = local.ClearNonCooperation.Code;

      if (ReadNonCooperation2())
      {
        local.NonCooperation.Code = entities.NonCooperation.Code;
      }

      if (!IsEmpty(local.NonCooperation.Code))
      {
        if (AsChar(local.NonCooperation.Code) == 'Y')
        {
          local.FaArrearsCollections.CurrentNonCoopCd = "N";
        }
        else if (!IsEmpty(local.NonCooperation.Code))
        {
          local.FaArrearsCollections.CurrentNonCoopCd = "C";
        }
      }

      local.ClosedCoop.Text20 = "";
      local.ClosedCoop.Text20 =
        local.ClosedDateWorkAttributes.TextDate10Char + ";" + (
          local.FaArrearsCollections.CaseClosureReason ?? "") + " ;" + (
          local.FaArrearsCollections.NonCoopCd ?? "") + " ;" + (
          local.FaArrearsCollections.CurrentNonCoopCd ?? "") + ";";

      if (ReadFaArrearsCollections1())
      {
        // already have the record
        continue;
      }
      else
      {
        try
        {
          CreateFaArrearsCollections();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FA_ARREARS_COLLECTION_AE";

              continue;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FA_ARREARS_COLLECTION_PV";

              continue;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      local.RecordsFound.Flag = "Y";

      if (Lt(local.ZeroDate.Date, entities.N2dReadPersonProgram.EffectiveDate))
      {
        local.EffDateDateWorkArea.Year =
          Year(entities.N2dReadPersonProgram.EffectiveDate);
        local.WorkArea.Text15 =
          NumberToString(local.EffDateDateWorkArea.Year, 15);
        local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
        local.EffDateDateWorkArea.Month =
          Month(entities.N2dReadPersonProgram.EffectiveDate);
        local.WorkArea.Text15 =
          NumberToString(local.EffDateDateWorkArea.Month, 15);
        local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.EffDateDateWorkArea.Day =
          Day(entities.N2dReadPersonProgram.EffectiveDate);
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

      if (Lt(local.ZeroDate.Date, local.FaArrearsCollections.CaseOpenDate))
      {
        local.StatusDateDateWorkArea.Year =
          Year(local.FaArrearsCollections.CaseOpenDate);
        local.WorkArea.Text15 =
          NumberToString(local.StatusDateDateWorkArea.Year, 15);
        local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
        local.StatusDateDateWorkArea.Month =
          Month(local.FaArrearsCollections.CaseOpenDate);
        local.WorkArea.Text15 =
          NumberToString(local.StatusDateDateWorkArea.Month, 15);
        local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.StatusDateDateWorkArea.Day =
          Day(local.FaArrearsCollections.CaseOpenDate);
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

      local.CasePersonNumber.Text33 = ";" + entities.Case1.Number + ";" + local
        .StatusDateWorkArea.Text10 + ";" + entities.CsePerson.Number;
      local.EffDateOffice.Text25 =
        NumberToString(entities.Office.SystemGeneratedId, 12, 4) + ";" + entities
        .Contractor.Name;
      local.NameTextWorkArea.Text30 = entities.ServiceProvider.LastName + ";"
        + entities.ServiceProvider.FirstName;
      local.StandarNumber.Text25 = entities.LegalAction.StandardNumber + ";" + entities
        .LegalAction.EstablishmentCode + ";";
      local.RtpDetail.Text200 = local.EffDateOffice.Text25 + local
        .CasePersonNumber.Text33 + ";" + local.StandarNumber.Text25 + " " + local
        .EffDateWorkArea.Text10 + ";" + local.NameTextWorkArea.Text30;

      if (local.FaArrearsCollections.ArrearsAmountDue.GetValueOrDefault() > 0)
      {
        local.Arrears.Count =
          (int)(local.FaArrearsCollections.ArrearsAmountDue.
            GetValueOrDefault() * 100);
        local.Count1.Text15 = NumberToString(local.Arrears.Count, 15);
        local.Count1.Text2 = Substring(local.Count1.Text15, 14, 2);
        local.Count1.Text9 =
          Substring(local.Count1.Text15, WorkArea.Text15_MaxLength, 8, 6) + "."
          + local.Count1.Text2;
      }
      else
      {
        local.Count1.Text9 = "0";
      }

      local.RtpDetail.Text200 = TrimEnd(local.RtpDetail.Text200) + ";" + local
        .Count1.Text9;
      local.RtpDetail.Text200 = TrimEnd(local.RtpDetail.Text200) + ";" + local
        .ClosedCoop.Text20;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriterLarge1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      ++local.NumberOfRecordsRead.Count;
    }

    if (AsChar(local.RecordsFound.Flag) == 'Y')
    {
      UseExtToDoACommit();

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
        if (local.Count.Item.Count1.Count <= 0 || local
          .Count.Item.Count1.TotalCurrency <= 0)
        {
          continue;
        }

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
        MoveCommon(local.Count.Item.Count1, local.Sorted.Update.CountSorted);
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
          MoveCommon(local.Sorted.Item.CountSorted, local.CountTemp1);

          ++local.Sorted.Index;
          local.Sorted.CheckSize();

          local.Temp2.SystemGeneratedId =
            local.Sorted.Item.Sorted1.SystemGeneratedId;
          MoveCseOrganization(local.Sorted.Item.JdSorted, local.JdTemp2);
          local.ContrtactorTemp2.Assign(local.Sorted.Item.ContractorSorted);
          MoveCommon(local.Sorted.Item.CountSorted, local.CountTemp2);

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
            MoveCommon(local.CountTemp2, local.Sorted.Update.CountSorted);

            ++local.Sorted.Index;
            local.Sorted.CheckSize();

            local.Sorted.Update.Sorted1.SystemGeneratedId =
              local.Temp1.SystemGeneratedId;
            local.Sorted.Update.ContractorSorted.Assign(local.ContractorTemp1);
            MoveCseOrganization(local.JdTemp1, local.Sorted.Update.JdSorted);
            MoveCommon(local.CountTemp1, local.Sorted.Update.CountSorted);
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
          local.Final.Update.CountFinal.TotalCurrency =
            local.Final.Item.CountFinal.TotalCurrency + local
            .Sorted.Item.CountSorted.TotalCurrency;
        }
        else
        {
          local.Swap.Flag = "Y";

          ++local.Final.Index;
          local.Final.CheckSize();

          local.Final.Update.ContractorFinal.Assign(
            local.Sorted.Item.ContractorSorted);
          MoveCommon(local.Sorted.Item.CountSorted,
            local.Final.Update.CountFinal);
          local.Final.Update.Final1.SystemGeneratedId = 0;
        }

        local.CountFinal.Count += local.Sorted.Item.CountSorted.Count;
        local.CountFinal.TotalCurrency += local.Sorted.Item.CountSorted.
          TotalCurrency;
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
        MoveCommon(local.Sorted.Item.CountSorted, local.Final.Update.CountFinal);
          
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
          MoveCommon(local.Final.Item.CountFinal, local.CountTemp1);

          ++local.Final.Index;
          local.Final.CheckSize();

          local.Temp2.SystemGeneratedId =
            local.Final.Item.Final1.SystemGeneratedId;
          MoveCseOrganization(local.Final.Item.JdFinal, local.JdTemp2);
          local.ContrtactorTemp2.Assign(local.Final.Item.ContractorFinal);
          MoveCommon(local.Final.Item.CountFinal, local.CountTemp2);

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
            MoveCommon(local.CountTemp2, local.Final.Update.CountFinal);

            ++local.Final.Index;
            local.Final.CheckSize();

            local.Final.Update.Final1.SystemGeneratedId =
              local.Temp1.SystemGeneratedId;
            local.Final.Update.ContractorFinal.Assign(local.ContractorTemp1);
            MoveCseOrganization(local.JdTemp1, local.Final.Update.JdFinal);
            MoveCommon(local.CountTemp1, local.Final.Update.CountFinal);
            local.Changed.Flag = "T";
          }
        }
      }

      local.Contractor.Text21 = "";
      local.Count1.Text10 = "";
      local.Count1.Text11 = "";
      local.Count1.Text2 = "";
      local.Write.TextLine80 = local.Contractor.Text21 + " " + local
        .Count1.Text10 + " " + local.Count1.Text11;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriterSmall1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }

      local.Contractor.Text21 = "";
      local.Count1.Text10 = "";
      local.Count1.Text11 = "";
      local.Count1.Text2 = "";
      local.Write.TextLine80 = local.Contractor.Text21 + " " + local
        .Count1.Text10 + " " + local.Count1.Text11;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriterSmall1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }

      local.Contractor.Text21 = " Contractor/Office";
      local.Count1.Text10 = "Total";
      local.Count1.Text11 = "Arrears";
      local.Write.TextLine80 = local.Contractor.Text21 + ";" + local
        .Count1.Text10 + ";" + local.Count1.Text11;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriterSmall1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }

      local.Final2.Index = -1;
      local.Final2.Count = 0;

      for(local.Final.Index = 0; local.Final.Index < local.Final.Count; ++
        local.Final.Index)
      {
        if (!local.Final.CheckSize())
        {
          break;
        }

        local.Final2.Index = local.Final.Index;
        local.Final2.CheckSize();

        local.Final2.Update.ContractorFinal2.Assign(
          local.Final.Item.ContractorFinal);
        MoveCseOrganization(local.Final.Item.JdFinal,
          local.Final2.Update.JdFinal2);
        local.Final2.Update.Final1.SystemGeneratedId =
          local.Final.Item.Final1.SystemGeneratedId;
      }

      local.Final.CheckIndex();

      foreach(var item in ReadFaArrearsCollections3())
      {
        for(local.Final2.Index = 0; local.Final2.Index < local.Final2.Count; ++
          local.Final2.Index)
        {
          if (!local.Final2.CheckSize())
          {
            break;
          }

          if (Equal(entities.FaArrearsCollections.Contractor,
            local.Final2.Item.ContractorFinal2.Name))
          {
            if (Equal(entities.FaArrearsCollections.Office,
              local.Final2.Item.JdFinal2.Name))
            {
              if (Equal(entities.FaArrearsCollections.CaseNumber,
                local.AlreadyProcessedCase.Number) && Equal
                (entities.FaArrearsCollections.ChildPersonNumber,
                local.AlreadyProcessedCsePerson.Number))
              {
                goto ReadEach;
              }

              local.Final2.Update.CountFinal2.Count =
                (int)((long)local.Final2.Item.CountFinal2.Count + 1);
              local.Final2.Update.CountFinal2.TotalCurrency =
                local.Final2.Item.CountFinal2.TotalCurrency + entities
                .FaArrearsCollections.ArrearsAmountDue.GetValueOrDefault();
              local.AlreadyProcessedCase.Number =
                entities.FaArrearsCollections.CaseNumber;
              local.AlreadyProcessedCsePerson.Number =
                entities.FaArrearsCollections.ChildPersonNumber;
            }
          }
        }

        local.Final2.CheckIndex();

ReadEach:
        ;
      }

      for(local.Final.Index = 0; local.Final.Index < local.Final.Count; ++
        local.Final.Index)
      {
        if (!local.Final.CheckSize())
        {
          break;
        }

        local.Final.Update.CountFinal.Count = 0;
        local.Final.Update.CountFinal.TotalCurrency = 0;
      }

      local.Final.CheckIndex();

      for(local.Final2.Index = 0; local.Final2.Index < local.Final2.Count; ++
        local.Final2.Index)
      {
        if (!local.Final2.CheckSize())
        {
          break;
        }

        for(local.Final.Index = 0; local.Final.Index < local.Final.Count; ++
          local.Final.Index)
        {
          if (!local.Final.CheckSize())
          {
            break;
          }

          if (Equal(local.Final2.Item.ContractorFinal2.Name,
            local.Final.Item.ContractorFinal.Name) && Equal
            (local.Final2.Item.JdFinal2.Name, local.Final.Item.JdFinal.Name))
          {
            MoveCommon(local.Final2.Item.CountFinal2,
              local.Final.Update.CountFinal);
          }
        }

        local.Final.CheckIndex();
      }

      local.Final2.CheckIndex();

      for(local.Final2.Index = 0; local.Final2.Index < local.Final2.Count; ++
        local.Final2.Index)
      {
        if (!local.Final2.CheckSize())
        {
          break;
        }

        // total the contractor
        if (Equal(local.Final2.Item.ContractorFinal2.Name,
          local.CurrentContactor.Name) && local
          .Final2.Item.Final1.SystemGeneratedId > 0)
        {
          local.ContractorTotal.Count += local.Final2.Item.CountFinal2.Count;
          local.ContractorTotal.TotalCurrency += local.Final2.Item.CountFinal2.
            TotalCurrency;
          local.GrandTotal.Count += local.Final2.Item.CountFinal2.Count;
          local.GrandTotal.TotalCurrency += local.Final2.Item.CountFinal2.
            TotalCurrency;

          if (local.Final2.Index + 1 == local.Final2.Count)
          {
            for(local.Final.Index = 0; local.Final.Index < local.Final.Count; ++
              local.Final.Index)
            {
              if (!local.Final.CheckSize())
              {
                break;
              }

              if (Equal(local.CurrentContactor.Name,
                local.Final.Item.ContractorFinal.Name) && IsEmpty
                (local.Final.Item.JdFinal.Name))
              {
                local.Final.Update.CountFinal.Count =
                  local.ContractorTotal.Count;
                local.Final.Update.CountFinal.TotalCurrency =
                  local.ContractorTotal.TotalCurrency;
                local.ContractorTotal.Count = 0;
                local.ContractorTotal.TotalCurrency = 0;
              }
            }

            local.Final.CheckIndex();
          }
        }
        else if (!Equal(local.Final2.Item.ContractorFinal2.Name,
          local.CurrentContactor.Name))
        {
          if (!IsEmpty(local.CurrentContactor.Name))
          {
            for(local.Final.Index = 0; local.Final.Index < local.Final.Count; ++
              local.Final.Index)
            {
              if (!local.Final.CheckSize())
              {
                break;
              }

              if (Equal(local.CurrentContactor.Name,
                local.Final.Item.ContractorFinal.Name) && IsEmpty
                (local.Final.Item.JdFinal.Name))
              {
                local.Final.Update.CountFinal.Count =
                  local.ContractorTotal.Count;
                local.Final.Update.CountFinal.TotalCurrency =
                  local.ContractorTotal.TotalCurrency;
                local.ContractorTotal.Count = 0;
                local.ContractorTotal.TotalCurrency = 0;
                local.CurrentContactor.Name =
                  local.Final2.Item.ContractorFinal2.Name;
              }
            }

            local.Final.CheckIndex();
          }
          else
          {
            // first time
            local.ContractorTotal.Count += local.Final2.Item.CountFinal2.Count;
            local.ContractorTotal.TotalCurrency += local.Final2.Item.
              CountFinal2.TotalCurrency;
            local.GrandTotal.Count += local.Final2.Item.CountFinal2.Count;
            local.GrandTotal.TotalCurrency += local.Final2.Item.CountFinal2.
              TotalCurrency;
            local.CurrentContactor.Name =
              local.Final2.Item.ContractorFinal2.Name;
          }
        }
      }

      local.Final2.CheckIndex();

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

        if (local.Final.Item.CountFinal.TotalCurrency > 0)
        {
          local.Arrears.Count =
            (int)(local.Final.Item.CountFinal.TotalCurrency * 100);
          local.Count1.Text15 = NumberToString(local.Arrears.Count, 15);
          local.Count1.Text2 = Substring(local.Count1.Text15, 14, 2);
          local.Count1.Text11 =
            Substring(local.Count1.Text15, WorkArea.Text15_MaxLength, 7, 7) + "."
            + local.Count1.Text2;
        }
        else
        {
          local.Count1.Text11 = "0";

          continue;
        }

        local.Write.TextLine80 = local.Contractor.Text21 + ";" + local
          .Count1.Text10 + ";" + local.Count1.Text11;
        local.EabFileHandling.Action = "WRITE";
        UseEabExternalReportWriterSmall1();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          ExitState = "FILE_READ_ERROR_WITH_RB";
        }
      }

      local.Final.CheckIndex();
      local.Contractor.Text21 = " Grand Total";
      local.Count1.Text10 = NumberToString(local.GrandTotal.Count, 6, 10);
      local.Start.Count = Verify(local.Count1.Text10, "123456789");
      local.Count1.Text10 =
        Substring(local.Count1.Text10, local.Start.Count, 10 - local
        .Start.Count + 1);

      if (local.CountFinal.TotalCurrency > 0)
      {
        local.Arrears.Count = (int)(local.GrandTotal.TotalCurrency * 100);
        local.Count1.Text15 = NumberToString(local.Arrears.Count, 15);
        local.Count1.Text2 = Substring(local.Count1.Text15, 14, 2);
        local.Count1.Text11 =
          Substring(local.Count1.Text15, WorkArea.Text15_MaxLength, 7, 7) + "."
          + local.Count1.Text2;
      }
      else
      {
        local.Count1.Text11 = "0";
      }

      local.Write.TextLine80 = local.Contractor.Text21 + ";" + local
        .Count1.Text10 + ";" + local.Count1.Text11;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriterSmall1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }
    }
    else
    {
      local.Write.TextLine80 = local.Contractor.Text21 + " " + local
        .Count1.Text10 + " " + local.Count1.Text11;
      local.RtpDetail.Text200 =
        "                                    ***********No records found********";
        
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriterLarge1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseSpB737Close();
      local.EabFileHandling.Action = "CLOSE";
      UseEabExternalReportWriterLarge3();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseEabExternalReportWriterSmall1();

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
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseSpB737Close();
      local.EabFileHandling.Action = "CLOSE";
      UseEabExternalReportWriterLarge3();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseEabExternalReportWriterSmall1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
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

  private static void MoveFaArrearsCollections(FaArrearsCollections source,
    FaArrearsCollections target)
  {
    target.FiscalInd = source.FiscalInd;
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.End.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.StatusDateDateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.Case1.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
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

  private void UseEabExternalReportWriterLarge1()
  {
    var useImport = new EabExternalReportWriterLarge.Import();
    var useExport = new EabExternalReportWriterLarge.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.RtpDetail.Text200 = local.RtpDetail.Text200;
    MoveExternal(local.External, useExport.External);

    Call(EabExternalReportWriterLarge.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseEabExternalReportWriterLarge2()
  {
    var useImport = new EabExternalReportWriterLarge.Import();
    var useExport = new EabExternalReportWriterLarge.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.RtpDetail.Text200 = local.RtpDetail.Text200;
    MoveExternal(local.PassArea, useExport.External);

    Call(EabExternalReportWriterLarge.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseEabExternalReportWriterLarge3()
  {
    var useImport = new EabExternalReportWriterLarge.Import();
    var useExport = new EabExternalReportWriterLarge.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveExternal(local.External, useExport.External);

    Call(EabExternalReportWriterLarge.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseEabExternalReportWriterSmall1()
  {
    var useImport = new EabExternalReportWriterSmall.Import();
    var useExport = new EabExternalReportWriterSmall.Export();

    useImport.External.TextLine80 = local.Write.TextLine80;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveExternal(local.External, useExport.External);

    Call(EabExternalReportWriterSmall.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseEabExternalReportWriterSmall2()
  {
    var useImport = new EabExternalReportWriterSmall.Import();
    var useExport = new EabExternalReportWriterSmall.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.External.TextLine80 = local.Report.TextLine80;
    MoveExternal(local.PassArea, useExport.External);

    Call(EabExternalReportWriterSmall.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    Call(ExtToDoACommit.Execute, useImport, useExport);
  }

  private void UseFnB734DetermineJdFromCase()
  {
    var useImport = new FnB734DetermineJdFromCase.Import();
    var useExport = new FnB734DetermineJdFromCase.Export();

    useImport.Case1.Number = entities.N2dReadCase.Number;
    useImport.ReportEndDate.Date = local.End.Date;

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

  private void UseSpB739HouseKeeping()
  {
    var useImport = new SpB739HouseKeeping.Import();
    var useExport = new SpB739HouseKeeping.Export();

    Call(SpB739HouseKeeping.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void CreateFaArrearsCollections()
  {
    var office1 = local.FaArrearsCollections.Office ?? "";
    var createdBy = local.FaArrearsCollections.CreatedBy ?? "";
    var createdTstamp = local.FaArrearsCollections.CreatedTstamp;
    var contractor = local.FaArrearsCollections.Contractor ?? "";
    var caseNumber = local.FaArrearsCollections.CaseNumber;
    var caseOpenDate = local.FaArrearsCollections.CaseOpenDate;
    var fiscalInd = local.FaArrearsCollections.FiscalInd;
    var fiscalYear = local.FaArrearsCollections.FiscalYear;
    var runNumber = local.FaArrearsCollections.RunNumber;
    var childPersonNumber = local.FaArrearsCollections.ChildPersonNumber;
    var fsStartDate = local.FaArrearsCollections.FsStartDate;
    var standardNumber = local.FaArrearsCollections.StandardNumber ?? "";
    var courtOrderEstBy = local.FaArrearsCollections.CourtOrderEstBy ?? "";
    var arrearsAmountDue =
      local.FaArrearsCollections.ArrearsAmountDue.GetValueOrDefault();
    var totalCollectionsAmount =
      local.FaArrearsCollections.TotalCollectionsAmount.GetValueOrDefault();
    var caseworkerLastName = local.FaArrearsCollections.CaseworkerLastName ?? ""
      ;
    var caseworkerFirstName =
      local.FaArrearsCollections.CaseworkerFirstName ?? "";
    var caseClosedDate = local.FaArrearsCollections.CaseClosedDate;
    var caseClosureReason = local.FaArrearsCollections.CaseClosureReason ?? "";
    var nonCoopCd = local.FaArrearsCollections.NonCoopCd ?? "";
    var currentNonCoopCd = local.FaArrearsCollections.CurrentNonCoopCd ?? "";

    entities.FaArrearsCollections.Populated = false;
    Update("CreateFaArrearsCollections",
      (db, command) =>
      {
        db.SetNullableString(command, "office", office1);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "contractor", contractor);
        db.SetString(command, "cseCaseNum", caseNumber);
        db.SetDate(command, "caseOpenDate", caseOpenDate);
        db.SetString(command, "fiscalInd", fiscalInd);
        db.SetString(command, "fiscalYear", fiscalYear);
        db.SetInt32(command, "runNumber", runNumber);
        db.SetString(command, "childPersonNumber", childPersonNumber);
        db.SetNullableDate(command, "fsStartDate", fsStartDate);
        db.SetNullableString(command, "standardNumber", standardNumber);
        db.SetNullableString(command, "courtOrderEstBy", courtOrderEstBy);
        db.SetNullableDecimal(command, "arrearsAmountDue", arrearsAmountDue);
        db.
          SetNullableDecimal(command, "totalCollectAmt", totalCollectionsAmount);
          
        db.SetNullableString(command, "caseworkerLname", caseworkerLastName);
        db.SetNullableString(command, "caseworkerFname", caseworkerFirstName);
        db.SetNullableDate(command, "caseClosedDate", caseClosedDate);
        db.SetNullableString(command, "caseCloserReason", caseClosureReason);
        db.SetNullableString(command, "nonCoopCd", nonCoopCd);
        db.SetNullableString(command, "currNonCoopCd", currentNonCoopCd);
      });

    entities.FaArrearsCollections.Office = office1;
    entities.FaArrearsCollections.CreatedBy = createdBy;
    entities.FaArrearsCollections.CreatedTstamp = createdTstamp;
    entities.FaArrearsCollections.Contractor = contractor;
    entities.FaArrearsCollections.CaseNumber = caseNumber;
    entities.FaArrearsCollections.CaseOpenDate = caseOpenDate;
    entities.FaArrearsCollections.FiscalInd = fiscalInd;
    entities.FaArrearsCollections.FiscalYear = fiscalYear;
    entities.FaArrearsCollections.RunNumber = runNumber;
    entities.FaArrearsCollections.ChildPersonNumber = childPersonNumber;
    entities.FaArrearsCollections.FsStartDate = fsStartDate;
    entities.FaArrearsCollections.StandardNumber = standardNumber;
    entities.FaArrearsCollections.CourtOrderEstBy = courtOrderEstBy;
    entities.FaArrearsCollections.ArrearsAmountDue = arrearsAmountDue;
    entities.FaArrearsCollections.TotalCollectionsAmount =
      totalCollectionsAmount;
    entities.FaArrearsCollections.CaseworkerLastName = caseworkerLastName;
    entities.FaArrearsCollections.CaseworkerFirstName = caseworkerFirstName;
    entities.FaArrearsCollections.CaseClosedDate = caseClosedDate;
    entities.FaArrearsCollections.CaseClosureReason = caseClosureReason;
    entities.FaArrearsCollections.NonCoopCd = nonCoopCd;
    entities.FaArrearsCollections.CurrentNonCoopCd = currentNonCoopCd;
    entities.FaArrearsCollections.Populated = true;
  }

  private bool ReadCase()
  {
    entities.N2dReadCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.N2dReadCase.Number = db.GetString(reader, 0);
        entities.N2dReadCase.Status = db.GetNullableString(reader, 1);
        entities.N2dReadCase.StatusDate = db.GetNullableDate(reader, 2);
        entities.N2dReadCase.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.N2dReadCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRoleCsePersonLegalActionCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.ApCase.Populated = false;
    entities.ApLegalActionCaseRole.Populated = false;
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCaseCaseRoleCsePersonLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "endDate", local.CurrentDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetDate(
          command, "effectiveDt",
          local.ObligationCreate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCase.Number = db.GetString(reader, 0);
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApLegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApLegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApLegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApLegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApLegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApLegalActionCaseRole.LgaId = db.GetInt32(reader, 6);
        entities.ApLegalActionCaseRole.CreatedTstamp =
          db.GetDateTime(reader, 7);
        entities.ApCase.Populated = true;
        entities.ApLegalActionCaseRole.Populated = true;
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.ApLegalActionCaseRole.CroType);

        return true;
      });
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
          command, "endDate", local.Begin.Date.GetValueOrDefault());
        db.
          SetDate(command, "effectiveDate", local.End.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.CaseAssignment.CasNo = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.CspNumber = db.GetString(reader, 5);
        entities.CsePerson.Number = db.GetString(reader, 5);
        entities.CaseRole.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Identifier = db.GetInt32(reader, 7);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.CspNumber = db.GetString(reader, 10);
        entities.CsePerson.Number = db.GetString(reader, 10);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 11);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 12);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 13);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 14);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 15);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 15);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 15);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 16);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 16);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 16);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 17);
        entities.CaseAssignment.OspCode = db.GetString(reader, 17);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 18);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 18);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 19);
        entities.ServiceProvider.LastName = db.GetString(reader, 20);
        entities.ServiceProvider.FirstName = db.GetString(reader, 21);
        entities.Office.TypeCode = db.GetString(reader, 22);
        entities.Office.Name = db.GetString(reader, 23);
        entities.Office.EffectiveDate = db.GetDate(reader, 24);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 25);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 26);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 27);
        entities.CaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 28);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 29);
        entities.CsePerson.Type1 = db.GetString(reader, 30);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 31);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 32);
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

  private bool ReadCseOrganizationOffice()
  {
    entities.N3dRead.Populated = false;
    entities.JudicialDistrict.Populated = false;

    return Read("ReadCseOrganizationOffice",
      (db, command) =>
      {
        db.
          SetDate(command, "effectiveDate", local.End.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "discontinueDate", local.Begin.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.N2dReadCase.Number);
      },
      (db, reader) =>
      {
        entities.JudicialDistrict.Code = db.GetString(reader, 0);
        entities.JudicialDistrict.Type1 = db.GetString(reader, 1);
        entities.N3dRead.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.N3dRead.TypeCode = db.GetString(reader, 3);
        entities.N3dRead.Name = db.GetString(reader, 4);
        entities.N3dRead.CogTypeCode = db.GetNullableString(reader, 5);
        entities.N3dRead.CogCode = db.GetNullableString(reader, 6);
        entities.N3dRead.EffectiveDate = db.GetDate(reader, 7);
        entities.N3dRead.DiscontinueDate = db.GetNullableDate(reader, 8);
        entities.N3dRead.OffOffice = db.GetNullableInt32(reader, 9);
        entities.N3dRead.Populated = true;
        entities.JudicialDistrict.Populated = true;
      });
  }

  private bool ReadCsePersonPersonProgram()
  {
    entities.N3dReadCsePerson.Populated = false;
    entities.N3dReadPersonProgram.Populated = false;

    return Read("ReadCsePersonPersonProgram",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          entities.N2dReadPersonProgram.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          entities.N2dReadPersonProgram.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.N3dReadCsePerson.Number = db.GetString(reader, 0);
        entities.N3dReadPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.N3dReadCsePerson.Type1 = db.GetString(reader, 1);
        entities.N3dReadPersonProgram.EffectiveDate = db.GetDate(reader, 2);
        entities.N3dReadPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.N3dReadPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.N3dReadPersonProgram.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.N3dReadCsePerson.Populated = true;
        entities.N3dReadPersonProgram.Populated = true;
        CheckValid<CsePerson>("Type1", entities.N3dReadCsePerson.Type1);
      });
  }

  private bool ReadFaArrearsCollections1()
  {
    entities.FaArrearsCollections.Populated = false;

    return Read("ReadFaArrearsCollections1",
      (db, command) =>
      {
        db.SetString(
          command, "fiscalYear", local.FaArrearsCollections.FiscalYear);
        db.
          SetString(command, "fiscalInd", local.FaArrearsCollections.FiscalInd);
          
        db.SetString(
          command, "cseCaseNum", local.FaArrearsCollections.CaseNumber);
        db.SetString(
          command, "childPersonNumber",
          local.FaArrearsCollections.ChildPersonNumber);
        db.SetInt32(command, "runNumber", local.FaArrearsCollections.RunNumber);
      },
      (db, reader) =>
      {
        entities.FaArrearsCollections.Office = db.GetNullableString(reader, 0);
        entities.FaArrearsCollections.CreatedBy =
          db.GetNullableString(reader, 1);
        entities.FaArrearsCollections.CreatedTstamp =
          db.GetNullableDateTime(reader, 2);
        entities.FaArrearsCollections.Contractor =
          db.GetNullableString(reader, 3);
        entities.FaArrearsCollections.CaseNumber = db.GetString(reader, 4);
        entities.FaArrearsCollections.CaseOpenDate = db.GetDate(reader, 5);
        entities.FaArrearsCollections.FiscalInd = db.GetString(reader, 6);
        entities.FaArrearsCollections.FiscalYear = db.GetString(reader, 7);
        entities.FaArrearsCollections.RunNumber = db.GetInt32(reader, 8);
        entities.FaArrearsCollections.ChildPersonNumber =
          db.GetString(reader, 9);
        entities.FaArrearsCollections.FsStartDate =
          db.GetNullableDate(reader, 10);
        entities.FaArrearsCollections.StandardNumber =
          db.GetNullableString(reader, 11);
        entities.FaArrearsCollections.CourtOrderEstBy =
          db.GetNullableString(reader, 12);
        entities.FaArrearsCollections.ArrearsAmountDue =
          db.GetNullableDecimal(reader, 13);
        entities.FaArrearsCollections.TotalCollectionsAmount =
          db.GetNullableDecimal(reader, 14);
        entities.FaArrearsCollections.CaseworkerLastName =
          db.GetNullableString(reader, 15);
        entities.FaArrearsCollections.CaseworkerFirstName =
          db.GetNullableString(reader, 16);
        entities.FaArrearsCollections.CaseClosedDate =
          db.GetNullableDate(reader, 17);
        entities.FaArrearsCollections.CaseClosureReason =
          db.GetNullableString(reader, 18);
        entities.FaArrearsCollections.NonCoopCd =
          db.GetNullableString(reader, 19);
        entities.FaArrearsCollections.CurrentNonCoopCd =
          db.GetNullableString(reader, 20);
        entities.FaArrearsCollections.Populated = true;
      });
  }

  private bool ReadFaArrearsCollections2()
  {
    entities.FaArrearsCollections.Populated = false;

    return Read("ReadFaArrearsCollections2",
      (db, command) =>
      {
        db.SetString(command, "fiscalInd", local.SetUp.FiscalInd);
        db.SetString(command, "fiscalYear", local.SetUp.FiscalYear);
      },
      (db, reader) =>
      {
        entities.FaArrearsCollections.Office = db.GetNullableString(reader, 0);
        entities.FaArrearsCollections.CreatedBy =
          db.GetNullableString(reader, 1);
        entities.FaArrearsCollections.CreatedTstamp =
          db.GetNullableDateTime(reader, 2);
        entities.FaArrearsCollections.Contractor =
          db.GetNullableString(reader, 3);
        entities.FaArrearsCollections.CaseNumber = db.GetString(reader, 4);
        entities.FaArrearsCollections.CaseOpenDate = db.GetDate(reader, 5);
        entities.FaArrearsCollections.FiscalInd = db.GetString(reader, 6);
        entities.FaArrearsCollections.FiscalYear = db.GetString(reader, 7);
        entities.FaArrearsCollections.RunNumber = db.GetInt32(reader, 8);
        entities.FaArrearsCollections.ChildPersonNumber =
          db.GetString(reader, 9);
        entities.FaArrearsCollections.FsStartDate =
          db.GetNullableDate(reader, 10);
        entities.FaArrearsCollections.StandardNumber =
          db.GetNullableString(reader, 11);
        entities.FaArrearsCollections.CourtOrderEstBy =
          db.GetNullableString(reader, 12);
        entities.FaArrearsCollections.ArrearsAmountDue =
          db.GetNullableDecimal(reader, 13);
        entities.FaArrearsCollections.TotalCollectionsAmount =
          db.GetNullableDecimal(reader, 14);
        entities.FaArrearsCollections.CaseworkerLastName =
          db.GetNullableString(reader, 15);
        entities.FaArrearsCollections.CaseworkerFirstName =
          db.GetNullableString(reader, 16);
        entities.FaArrearsCollections.CaseClosedDate =
          db.GetNullableDate(reader, 17);
        entities.FaArrearsCollections.CaseClosureReason =
          db.GetNullableString(reader, 18);
        entities.FaArrearsCollections.NonCoopCd =
          db.GetNullableString(reader, 19);
        entities.FaArrearsCollections.CurrentNonCoopCd =
          db.GetNullableString(reader, 20);
        entities.FaArrearsCollections.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFaArrearsCollections3()
  {
    entities.FaArrearsCollections.Populated = false;

    return ReadEach("ReadFaArrearsCollections3",
      (db, command) =>
      {
        db.SetInt32(command, "runNumber", local.SetUp.RunNumber);
        db.SetString(command, "fiscalInd", local.SetUp.FiscalInd);
        db.SetString(command, "fiscalYear", local.SetUp.FiscalYear);
      },
      (db, reader) =>
      {
        entities.FaArrearsCollections.Office = db.GetNullableString(reader, 0);
        entities.FaArrearsCollections.CreatedBy =
          db.GetNullableString(reader, 1);
        entities.FaArrearsCollections.CreatedTstamp =
          db.GetNullableDateTime(reader, 2);
        entities.FaArrearsCollections.Contractor =
          db.GetNullableString(reader, 3);
        entities.FaArrearsCollections.CaseNumber = db.GetString(reader, 4);
        entities.FaArrearsCollections.CaseOpenDate = db.GetDate(reader, 5);
        entities.FaArrearsCollections.FiscalInd = db.GetString(reader, 6);
        entities.FaArrearsCollections.FiscalYear = db.GetString(reader, 7);
        entities.FaArrearsCollections.RunNumber = db.GetInt32(reader, 8);
        entities.FaArrearsCollections.ChildPersonNumber =
          db.GetString(reader, 9);
        entities.FaArrearsCollections.FsStartDate =
          db.GetNullableDate(reader, 10);
        entities.FaArrearsCollections.StandardNumber =
          db.GetNullableString(reader, 11);
        entities.FaArrearsCollections.CourtOrderEstBy =
          db.GetNullableString(reader, 12);
        entities.FaArrearsCollections.ArrearsAmountDue =
          db.GetNullableDecimal(reader, 13);
        entities.FaArrearsCollections.TotalCollectionsAmount =
          db.GetNullableDecimal(reader, 14);
        entities.FaArrearsCollections.CaseworkerLastName =
          db.GetNullableString(reader, 15);
        entities.FaArrearsCollections.CaseworkerFirstName =
          db.GetNullableString(reader, 16);
        entities.FaArrearsCollections.CaseClosedDate =
          db.GetNullableDate(reader, 17);
        entities.FaArrearsCollections.CaseClosureReason =
          db.GetNullableString(reader, 18);
        entities.FaArrearsCollections.NonCoopCd =
          db.GetNullableString(reader, 19);
        entities.FaArrearsCollections.CurrentNonCoopCd =
          db.GetNullableString(reader, 20);
        entities.FaArrearsCollections.Populated = true;

        return true;
      });
  }

  private bool ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetDate(command, "date1", local.Begin.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.End.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 1);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 2);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 3);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 4);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetDate(command, "date1", local.Begin.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.End.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 1);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 2);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 3);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 4);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionObligationDebtObligor()
  {
    entities.Debt.Populated = false;
    entities.LegalAction.Populated = false;
    entities.Obligation.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadLegalActionObligationDebtObligor",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspSupNumber", entities.CsePerson.Number);
          
        db.SetNullableDate(
          command, "retiredDt1", local.ZeroDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt2",
          local.OpenCase.CseOpenDate.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp1", local.Begin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.End.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.CpaType = db.GetString(reader, 6);
        entities.Debt.CpaType = db.GetString(reader, 6);
        entities.Obligor.Type1 = db.GetString(reader, 6);
        entities.Obligor.Type1 = db.GetString(reader, 6);
        entities.Obligation.CspNumber = db.GetString(reader, 7);
        entities.Debt.CspNumber = db.GetString(reader, 7);
        entities.Obligor.CspNumber = db.GetString(reader, 7);
        entities.Obligor.CspNumber = db.GetString(reader, 7);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 8);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 10);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.Debt.Type1 = db.GetString(reader, 12);
        entities.Debt.Amount = db.GetDecimal(reader, 13);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Debt.DebtType = db.GetString(reader, 15);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 16);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 17);
        entities.Debt.Populated = true;
        entities.LegalAction.Populated = true;
        entities.Obligation.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadNonCooperation1()
  {
    entities.NonCooperation.Populated = false;

    return Read("ReadNonCooperation1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.End.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Begin.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "effectiveDate",
          entities.PersonProgram.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NonCooperation.Code = db.GetNullableString(reader, 0);
        entities.NonCooperation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.NonCooperation.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.NonCooperation.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.NonCooperation.CasNumber = db.GetString(reader, 4);
        entities.NonCooperation.CspNumber = db.GetString(reader, 5);
        entities.NonCooperation.CroType = db.GetString(reader, 6);
        entities.NonCooperation.CroIdentifier = db.GetInt32(reader, 7);
        entities.NonCooperation.Populated = true;
        CheckValid<NonCooperation>("CroType", entities.NonCooperation.CroType);
      });
  }

  private bool ReadNonCooperation2()
  {
    entities.NonCooperation.Populated = false;

    return Read("ReadNonCooperation2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.End.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Begin.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.NonCooperation.Code = db.GetNullableString(reader, 0);
        entities.NonCooperation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.NonCooperation.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.NonCooperation.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.NonCooperation.CasNumber = db.GetString(reader, 4);
        entities.NonCooperation.CspNumber = db.GetString(reader, 5);
        entities.NonCooperation.CroType = db.GetString(reader, 6);
        entities.NonCooperation.CroIdentifier = db.GetInt32(reader, 7);
        entities.NonCooperation.Populated = true;
        CheckValid<NonCooperation>("CroType", entities.NonCooperation.CroType);
      });
  }

  private bool ReadOffice1()
  {
    entities.N2dReadOffice.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId",
          local.DashboardAuditData.Office.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.N2dReadOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.N2dReadOffice.TypeCode = db.GetString(reader, 1);
        entities.N2dReadOffice.Name = db.GetString(reader, 2);
        entities.N2dReadOffice.EffectiveDate = db.GetDate(reader, 3);
        entities.N2dReadOffice.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.N2dReadOffice.OffOffice = db.GetNullableInt32(reader, 5);
        entities.N2dReadOffice.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOffice2()
  {
    entities.Office.Populated = false;

    return ReadEach("ReadOffice2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Begin.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.EffectiveDate = db.GetDate(reader, 3);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
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
        db.
          SetDate(command, "effectiveDate", local.End.Date.GetValueOrDefault());
          
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
    /// <summary>A Final2Group group.</summary>
    [Serializable]
    public class Final2Group
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
      /// A value of JdFinal2.
      /// </summary>
      [JsonPropertyName("jdFinal2")]
      public CseOrganization JdFinal2
      {
        get => jdFinal2 ??= new();
        set => jdFinal2 = value;
      }

      /// <summary>
      /// A value of ContractorFinal2.
      /// </summary>
      [JsonPropertyName("contractorFinal2")]
      public CseOrganization ContractorFinal2
      {
        get => contractorFinal2 ??= new();
        set => contractorFinal2 = value;
      }

      /// <summary>
      /// A value of CountFinal2.
      /// </summary>
      [JsonPropertyName("countFinal2")]
      public Common CountFinal2
      {
        get => countFinal2 ??= new();
        set => countFinal2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Office final1;
      private CseOrganization jdFinal2;
      private CseOrganization contractorFinal2;
      private Common countFinal2;
    }

    /// <summary>A FiscalYearsGroup group.</summary>
    [Serializable]
    public class FiscalYearsGroup
    {
      /// <summary>
      /// A value of Begin.
      /// </summary>
      [JsonPropertyName("begin")]
      public DateWorkArea Begin
      {
        get => begin ??= new();
        set => begin = value;
      }

      /// <summary>
      /// A value of End.
      /// </summary>
      [JsonPropertyName("end")]
      public DateWorkArea End
      {
        get => end ??= new();
        set => end = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private DateWorkArea begin;
      private DateWorkArea end;
    }

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
    /// A value of OpenCase.
    /// </summary>
    [JsonPropertyName("openCase")]
    public Case1 OpenCase
    {
      get => openCase ??= new();
      set => openCase = value;
    }

    /// <summary>
    /// A value of ClearNonCooperation.
    /// </summary>
    [JsonPropertyName("clearNonCooperation")]
    public NonCooperation ClearNonCooperation
    {
      get => clearNonCooperation ??= new();
      set => clearNonCooperation = value;
    }

    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of ClearInfrastructure.
    /// </summary>
    [JsonPropertyName("clearInfrastructure")]
    public Infrastructure ClearInfrastructure
    {
      get => clearInfrastructure ??= new();
      set => clearInfrastructure = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public WorkArea Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of ClosedCoop.
    /// </summary>
    [JsonPropertyName("closedCoop")]
    public WorkArea ClosedCoop
    {
      get => closedCoop ??= new();
      set => closedCoop = value;
    }

    /// <summary>
    /// A value of ClosedDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("closedDateWorkAttributes")]
    public DateWorkAttributes ClosedDateWorkAttributes
    {
      get => closedDateWorkAttributes ??= new();
      set => closedDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of ClosedDateWorkArea.
    /// </summary>
    [JsonPropertyName("closedDateWorkArea")]
    public DateWorkArea ClosedDateWorkArea
    {
      get => closedDateWorkArea ??= new();
      set => closedDateWorkArea = value;
    }

    /// <summary>
    /// A value of RtpDetail.
    /// </summary>
    [JsonPropertyName("rtpDetail")]
    public WorkArea RtpDetail
    {
      get => rtpDetail ??= new();
      set => rtpDetail = value;
    }

    /// <summary>
    /// A value of ProgramMinium.
    /// </summary>
    [JsonPropertyName("programMinium")]
    public DateWorkArea ProgramMinium
    {
      get => programMinium ??= new();
      set => programMinium = value;
    }

    /// <summary>
    /// A value of NameTextWorkArea.
    /// </summary>
    [JsonPropertyName("nameTextWorkArea")]
    public TextWorkArea NameTextWorkArea
    {
      get => nameTextWorkArea ??= new();
      set => nameTextWorkArea = value;
    }

    /// <summary>
    /// A value of StandarNumber.
    /// </summary>
    [JsonPropertyName("standarNumber")]
    public WorkArea StandarNumber
    {
      get => standarNumber ??= new();
      set => standarNumber = value;
    }

    /// <summary>
    /// A value of AlreadyProcessedCsePerson.
    /// </summary>
    [JsonPropertyName("alreadyProcessedCsePerson")]
    public CsePerson AlreadyProcessedCsePerson
    {
      get => alreadyProcessedCsePerson ??= new();
      set => alreadyProcessedCsePerson = value;
    }

    /// <summary>
    /// A value of DebtCreated.
    /// </summary>
    [JsonPropertyName("debtCreated")]
    public DateWorkArea DebtCreated
    {
      get => debtCreated ??= new();
      set => debtCreated = value;
    }

    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public Case1 Test
    {
      get => test ??= new();
      set => test = value;
    }

    /// <summary>
    /// A value of FinalSubscriptCount.
    /// </summary>
    [JsonPropertyName("finalSubscriptCount")]
    public Common FinalSubscriptCount
    {
      get => finalSubscriptCount ??= new();
      set => finalSubscriptCount = value;
    }

    /// <summary>
    /// A value of CurrentContactor.
    /// </summary>
    [JsonPropertyName("currentContactor")]
    public CseOrganization CurrentContactor
    {
      get => currentContactor ??= new();
      set => currentContactor = value;
    }

    /// <summary>
    /// A value of ContractorTotal.
    /// </summary>
    [JsonPropertyName("contractorTotal")]
    public Common ContractorTotal
    {
      get => contractorTotal ??= new();
      set => contractorTotal = value;
    }

    /// <summary>
    /// A value of GrandTotal.
    /// </summary>
    [JsonPropertyName("grandTotal")]
    public Common GrandTotal
    {
      get => grandTotal ??= new();
      set => grandTotal = value;
    }

    /// <summary>
    /// Gets a value of Final2.
    /// </summary>
    [JsonIgnore]
    public Array<Final2Group> Final2 => final2 ??= new(Final2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Final2 for json serialization.
    /// </summary>
    [JsonPropertyName("final2")]
    [Computed]
    public IList<Final2Group> Final2_Json
    {
      get => final2;
      set => Final2.Assign(value);
    }

    /// <summary>
    /// A value of AlreadyProcessedCase.
    /// </summary>
    [JsonPropertyName("alreadyProcessedCase")]
    public Case1 AlreadyProcessedCase
    {
      get => alreadyProcessedCase ??= new();
      set => alreadyProcessedCase = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public DateWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of ConvertDate.
    /// </summary>
    [JsonPropertyName("convertDate")]
    public DateWorkAttributes ConvertDate
    {
      get => convertDate ??= new();
      set => convertDate = value;
    }

    /// <summary>
    /// A value of ObligationCreate.
    /// </summary>
    [JsonPropertyName("obligationCreate")]
    public DateWorkArea ObligationCreate
    {
      get => obligationCreate ??= new();
      set => obligationCreate = value;
    }

    /// <summary>
    /// A value of CaseMatch.
    /// </summary>
    [JsonPropertyName("caseMatch")]
    public Common CaseMatch
    {
      get => caseMatch ??= new();
      set => caseMatch = value;
    }

    /// <summary>
    /// A value of Local2NdCaseReadFound.
    /// </summary>
    [JsonPropertyName("local2NdCaseReadFound")]
    public Common Local2NdCaseReadFound
    {
      get => local2NdCaseReadFound ??= new();
      set => local2NdCaseReadFound = value;
    }

    /// <summary>
    /// A value of Arrears.
    /// </summary>
    [JsonPropertyName("arrears")]
    public Common Arrears
    {
      get => arrears ??= new();
      set => arrears = value;
    }

    /// <summary>
    /// A value of ClearFaArrearsCollections.
    /// </summary>
    [JsonPropertyName("clearFaArrearsCollections")]
    public FaArrearsCollections ClearFaArrearsCollections
    {
      get => clearFaArrearsCollections ??= new();
      set => clearFaArrearsCollections = value;
    }

    /// <summary>
    /// A value of SetUp.
    /// </summary>
    [JsonPropertyName("setUp")]
    public FaArrearsCollections SetUp
    {
      get => setUp ??= new();
      set => setUp = value;
    }

    /// <summary>
    /// A value of FaArrearsCollections.
    /// </summary>
    [JsonPropertyName("faArrearsCollections")]
    public FaArrearsCollections FaArrearsCollections
    {
      get => faArrearsCollections ??= new();
      set => faArrearsCollections = value;
    }

    /// <summary>
    /// A value of AmtOwed.
    /// </summary>
    [JsonPropertyName("amtOwed")]
    public Common AmtOwed
    {
      get => amtOwed ??= new();
      set => amtOwed = value;
    }

    /// <summary>
    /// A value of CurrentDate1.
    /// </summary>
    [JsonPropertyName("currentDate1")]
    public DateWorkArea CurrentDate1
    {
      get => currentDate1 ??= new();
      set => currentDate1 = value;
    }

    /// <summary>
    /// A value of FiscalEnd.
    /// </summary>
    [JsonPropertyName("fiscalEnd")]
    public DateWorkArea FiscalEnd
    {
      get => fiscalEnd ??= new();
      set => fiscalEnd = value;
    }

    /// <summary>
    /// A value of FiscalBegin.
    /// </summary>
    [JsonPropertyName("fiscalBegin")]
    public DateWorkArea FiscalBegin
    {
      get => fiscalBegin ??= new();
      set => fiscalBegin = value;
    }

    /// <summary>
    /// Gets a value of FiscalYears.
    /// </summary>
    [JsonIgnore]
    public Array<FiscalYearsGroup> FiscalYears => fiscalYears ??= new(
      FiscalYearsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FiscalYears for json serialization.
    /// </summary>
    [JsonPropertyName("fiscalYears")]
    [Computed]
    public IList<FiscalYearsGroup> FiscalYears_Json
    {
      get => fiscalYears;
      set => FiscalYears.Assign(value);
    }

    /// <summary>
    /// A value of DateWorkAttributes.
    /// </summary>
    [JsonPropertyName("dateWorkAttributes")]
    public DateWorkAttributes DateWorkAttributes
    {
      get => dateWorkAttributes ??= new();
      set => dateWorkAttributes = value;
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
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Begin.
    /// </summary>
    [JsonPropertyName("begin")]
    public DateWorkArea Begin
    {
      get => begin ??= new();
      set => begin = value;
    }

    /// <summary>
    /// A value of FiscalYear.
    /// </summary>
    [JsonPropertyName("fiscalYear")]
    public WorkArea FiscalYear
    {
      get => fiscalYear ??= new();
      set => fiscalYear = value;
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
    /// A value of NameWorkArea.
    /// </summary>
    [JsonPropertyName("nameWorkArea")]
    public WorkArea NameWorkArea
    {
      get => nameWorkArea ??= new();
      set => nameWorkArea = value;
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
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public WorkArea Date
    {
      get => date ??= new();
      set => date = value;
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
    /// A value of OpenEabReportSend.
    /// </summary>
    [JsonPropertyName("openEabReportSend")]
    public EabReportSend OpenEabReportSend
    {
      get => openEabReportSend ??= new();
      set => openEabReportSend = value;
    }

    /// <summary>
    /// A value of Report.
    /// </summary>
    [JsonPropertyName("report")]
    public External Report
    {
      get => report ??= new();
      set => report = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private Case1 openCase;
    private NonCooperation clearNonCooperation;
    private NonCooperation nonCooperation;
    private Infrastructure clearInfrastructure;
    private Infrastructure infrastructure;
    private WorkArea header;
    private WorkArea closedCoop;
    private DateWorkAttributes closedDateWorkAttributes;
    private DateWorkArea closedDateWorkArea;
    private WorkArea rtpDetail;
    private DateWorkArea programMinium;
    private TextWorkArea nameTextWorkArea;
    private WorkArea standarNumber;
    private CsePerson alreadyProcessedCsePerson;
    private DateWorkArea debtCreated;
    private Case1 test;
    private Common finalSubscriptCount;
    private CseOrganization currentContactor;
    private Common contractorTotal;
    private Common grandTotal;
    private Array<Final2Group> final2;
    private Case1 alreadyProcessedCase;
    private DateWorkArea convert;
    private DateWorkAttributes convertDate;
    private DateWorkArea obligationCreate;
    private Common caseMatch;
    private Common local2NdCaseReadFound;
    private Common arrears;
    private FaArrearsCollections clearFaArrearsCollections;
    private FaArrearsCollections setUp;
    private FaArrearsCollections faArrearsCollections;
    private Common amtOwed;
    private DateWorkArea currentDate1;
    private DateWorkArea fiscalEnd;
    private DateWorkArea fiscalBegin;
    private Array<FiscalYearsGroup> fiscalYears;
    private DateWorkAttributes dateWorkAttributes;
    private DateWorkArea dateWorkArea;
    private DateWorkArea end;
    private DateWorkArea begin;
    private WorkArea fiscalYear;
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
    private WorkArea nameWorkArea;
    private WorkArea casePersonNumber;
    private WorkArea effDateOffice;
    private Common current;
    private Common currentPosition;
    private Common fieldNumber;
    private Common includeArrearsOnly;
    private TextWorkArea postion;
    private DateWorkArea startDate;
    private WorkArea date;
    private External passArea;
    private EabReportSend openEabReportSend;
    private External report;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of N3dRead.
    /// </summary>
    [JsonPropertyName("n3dRead")]
    public Office N3dRead
    {
      get => n3dRead ??= new();
      set => n3dRead = value;
    }

    /// <summary>
    /// A value of N2dReadCaseAssignment.
    /// </summary>
    [JsonPropertyName("n2dReadCaseAssignment")]
    public CaseAssignment N2dReadCaseAssignment
    {
      get => n2dReadCaseAssignment ??= new();
      set => n2dReadCaseAssignment = value;
    }

    /// <summary>
    /// A value of N2dReadOffice.
    /// </summary>
    [JsonPropertyName("n2dReadOffice")]
    public Office N2dReadOffice
    {
      get => n2dReadOffice ??= new();
      set => n2dReadOffice = value;
    }

    /// <summary>
    /// A value of N3dReadCsePerson.
    /// </summary>
    [JsonPropertyName("n3dReadCsePerson")]
    public CsePerson N3dReadCsePerson
    {
      get => n3dReadCsePerson ??= new();
      set => n3dReadCsePerson = value;
    }

    /// <summary>
    /// A value of N3dReadPersonProgram.
    /// </summary>
    [JsonPropertyName("n3dReadPersonProgram")]
    public PersonProgram N3dReadPersonProgram
    {
      get => n3dReadPersonProgram ??= new();
      set => n3dReadPersonProgram = value;
    }

    /// <summary>
    /// A value of ApCase.
    /// </summary>
    [JsonPropertyName("apCase")]
    public Case1 ApCase
    {
      get => apCase ??= new();
      set => apCase = value;
    }

    /// <summary>
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ApLaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("apLaPersonLaCaseRole")]
    public LaPersonLaCaseRole ApLaPersonLaCaseRole
    {
      get => apLaPersonLaCaseRole ??= new();
      set => apLaPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of ApLegalActionPerson.
    /// </summary>
    [JsonPropertyName("apLegalActionPerson")]
    public LegalActionPerson ApLegalActionPerson
    {
      get => apLegalActionPerson ??= new();
      set => apLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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

    /// <summary>
    /// A value of FaArrearsCollections.
    /// </summary>
    [JsonPropertyName("faArrearsCollections")]
    public FaArrearsCollections FaArrearsCollections
    {
      get => faArrearsCollections ??= new();
      set => faArrearsCollections = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of N2dReadCase.
    /// </summary>
    [JsonPropertyName("n2dReadCase")]
    public Case1 N2dReadCase
    {
      get => n2dReadCase ??= new();
      set => n2dReadCase = value;
    }

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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of JudicialDistrict.
    /// </summary>
    [JsonPropertyName("judicialDistrict")]
    public CseOrganization JudicialDistrict
    {
      get => judicialDistrict ??= new();
      set => judicialDistrict = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public CseOrganization County
    {
      get => county ??= new();
      set => county = value;
    }

    private NonCooperation nonCooperation;
    private CaseRole arCaseRole;
    private CsePerson arCsePerson;
    private Infrastructure infrastructure;
    private Office n3dRead;
    private CaseAssignment n2dReadCaseAssignment;
    private Office n2dReadOffice;
    private CsePerson n3dReadCsePerson;
    private PersonProgram n3dReadPersonProgram;
    private Case1 apCase;
    private LegalActionCaseRole apLegalActionCaseRole;
    private LaPersonLaCaseRole apLaPersonLaCaseRole;
    private LegalActionPerson apLegalActionPerson;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CsePersonAccount supported;
    private FaArrearsCollections faArrearsCollections;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
    private Case1 n2dReadCase;
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
    private LegalAction legalAction;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount obligor;
    private CseOrganization judicialDistrict;
    private CseOrganization county;
  }
#endregion
}
