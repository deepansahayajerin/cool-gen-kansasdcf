// Program: LE_B596_IWO_DELINQUENCY_EXTRACT, ID: 373498367, model: 746.
// Short name: SWEL596B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B596_IWO_DELINQUENCY_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB596IwoDelinquencyExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B596_IWO_DELINQUENCY_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB596IwoDelinquencyExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB596IwoDelinquencyExtract.
  /// </summary>
  public LeB596IwoDelinquencyExtract(IContext context, Import import,
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
    // *****************************************************************************************
    // Date      Developer         Request #  Description
    // --------  ----------------  ---------  ------------------------
    // 06/17/01  GVandy            WR10358    Initial Development
    // 08/26/11  GVandy            CQ29124    Add reason_code = 'RC' when 
    // reading for regional office.
    // 11/27/13  DDupree           CQ41960    Change the period when the IWO 
    // that were added
    // to be evulated by changing the hard coded value with a parameter pass 
    // into the program.
    // *****************************************************************************************
    // ****************************
    // CHECK IF ADABAS IS AVAILABLE
    // ****************************
    UseCabReadAdabasPersonBatch2();

    if (Equal(local.AbendData.AdabasResponseCd, "0148"))
    {
      ExitState = "ADABAS_UNAVAILABLE_RB";

      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ************************************************
    // *Call External to Open the Flat File.          *
    // ************************************************
    local.EabFileHandling.Action = "OPEN";
    UseLeEabWriteIwo45DayInfo2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in file open for 'le_iwo_45_day_delinquency_info'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.FileOpened.Flag = "Y";
    local.StartCommon.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;

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
              local.NumberOfWeeksBack.Count = 3;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.NumberOfWeeksBack.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 2:
            if (local.Current.Count == 1)
            {
              local.RptPeriodNumOfWeeks.Count = 1;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.RptPeriodNumOfWeeks.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
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

    if (local.NumberOfWeeksBack.Count <= 0)
    {
      local.NumberOfWeeksBack.Count = 3;
    }

    local.NumberOfDays.Count =
      (int)((long)local.NumberOfWeeksBack.Count * 7 - 3);

    // -----------------------------------------------------------------------------------------------
    // Determine the reporting period start and end dates..
    // The end date is set to the program processing date (which will be a 
    // Friday) minus 39 days.  This will be 12:00am on a Monday.  The start date
    // will then be set to the end date minus 7 days.  This will also be 12:
    // 00am on a Monday.  Since the reporting period we want is a Monday through
    // Sunday, we will then look at all IWGL records with a created timestamp
    // greater or equal to the start date and less than the end date.
    // -----------------------------------------------------------------------------------------------
    local.End.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate, -local.NumberOfDays.Count);
      
    UseCabDate2TextWithHyphens1();
    local.End.Timestamp = Timestamp(local.TextWorkArea.Text10);
    local.NumberOfDaysInPeriod.Count =
      (int)((long)local.RptPeriodNumOfWeeks.Count * 7);
    local.StartDateWorkArea.Date =
      AddDays(local.End.Date, -local.NumberOfDaysInPeriod.Count);
    UseCabDate2TextWithHyphens2();
    local.StartDateWorkArea.Timestamp = Timestamp(local.TextWorkArea.Text10);

    foreach(var item in ReadLegalActionIncomeSource())
    {
      if (Lt(entities.LegalActionIncomeSource.EndDate,
        local.ProgramProcessingInfo.ProcessDate) && !
        Equal(entities.LegalActionIncomeSource.EndDate,
        local.NullDateWorkArea.Date))
      {
        continue;
      }

      if (!ReadIncomeSourceCsePerson())
      {
        // Write to the error file and skip the record...
        local.BatchTimestampWorkArea.TextTimestamp = "";
        local.BatchTimestampWorkArea.IefTimestamp =
          entities.LegalActionIncomeSource.CreatedTstamp;
        UseLeCabConvertTimestamp();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reading Income Source associated to Legal Action Income Source id with created timestamp : " +
          local.BatchTimestampWorkArea.TextTimestamp;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;

        continue;
      }

      local.BatchTimestampWorkArea.TextTimestamp = "";
      local.BatchTimestampWorkArea.IefTimestamp =
        entities.IncomeSource.Identifier;
      UseLeCabConvertTimestamp();

      if (!ReadLegalActionTribunal())
      {
        // Write to the error file and skip the record...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reading Legal Action associated to Income Source id : " + local
          .BatchTimestampWorkArea.TextTimestamp;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;

        continue;
      }

      switch(TrimEnd(entities.LegalAction.ActionTaken))
      {
        case "IWO":
          break;
        case "IWOMODO":
          break;
        case "IWONOTKM":
          break;
        case "IWONOTKS":
          break;
        case "ORDIWO2":
          break;
        default:
          // -- Skip the IWGL record if the associated legal action is not one 
          // of IWO,
          // IWOMODO, IWONOTKM, IWONOTKS,  or ORDIWO2.
          continue;
      }

      local.IwglCreatedDate.Date =
        Date(entities.LegalActionIncomeSource.CreatedTstamp);

      // -- Determine if any payments have been received since the IWO was 
      // created.
      ReadCashReceiptDetail();

      if (local.NumberOfPayments.Count > 0)
      {
        // -- A payment has been received since the IWO was created.  Skip this 
        // legal_action_income_source.  The IWO is not delinquent.
        continue;
      }

      local.ServiceProvider1.FirstName = "";
      local.ServiceProvider1.LastName = "";
      local.ServiceProvider1.MiddleInitial = "";
      local.ServiceProvider1.FormattedName = "";

      if (ReadCase())
      {
        if (ReadServiceProvider1())
        {
          local.ServiceProvider1.FirstName = entities.ServiceProvider.FirstName;
          local.ServiceProvider1.LastName = entities.ServiceProvider.LastName;
          local.ServiceProvider1.MiddleInitial =
            entities.ServiceProvider.MiddleInitial;
          UseSiFormatCsePersonName1();
        }
        else
        {
          // Write to the error file.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error trying to find caseworker for case number : " + entities
            .Case1.Number;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.TotalErrors.Count;
          ExitState = "ACO_NN0000_ALL_OK";
          local.ServiceProvider1.FormattedName =
            "Caseworker not found for case";
        }
      }
      else
      {
        local.Convert.Text15 =
          NumberToString(entities.LegalAction.Identifier, 15);
        local.LegalActionId.Text9 = Substring(local.Convert.Text15, 7, 9);

        // Write to the error file.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error trying to find case using LOPS for legal action : " + local
          .LegalActionId.Text9;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;
        ExitState = "ACO_NN0000_ALL_OK";
        local.ServiceProvider1.FormattedName = "Case not found using LOPS";
      }

      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseCabReadAdabasPersonBatch1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseSiFormatCsePersonName2();
      }
      else
      {
        if (AsChar(local.AbendData.Type1) == 'A' && Equal
          (local.AbendData.AdabasResponseCd, "0113"))
        {
          // -------------------------------------------------------
          //   Adabas not found..
          // -------------------------------------------------------
        }
        else
        {
          // -------------------------------------------------------
          //   Unknown error response returned from adabas.
          // -------------------------------------------------------
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Fatal error in Adabas for person number : " + entities
            .CsePerson.Number;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + ", Abend Type Code=";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + local.AbendData.Type1;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + ", Response Code=";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + local
            .AbendData.AdabasResponseCd;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + ", File Number=";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + local
            .AbendData.AdabasFileNumber;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + ", File Action=";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + local
            .AbendData.AdabasFileAction;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.TotalErrors.Count;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        local.CsePersonsWorkSet.FormattedName =
          "*** Name not found in ADABAS ***";
      }

      if (AsChar(entities.IncomeSource.Type1) == 'O')
      {
        if (ReadNonEmployIncomeSourceAddress())
        {
          local.Employer.Name = entities.IncomeSource.Name;
          local.EmployerAddress.City =
            entities.NonEmployIncomeSourceAddress.City;
          local.EmployerAddress.State =
            entities.NonEmployIncomeSourceAddress.State;
        }
        else
        {
          // Write to the error file and skip the record.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading non employment income source address for income source id : " +
            local.BatchTimestampWorkArea.TextTimestamp;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.TotalErrors.Count;

          continue;
        }
      }
      else if (ReadEmployerEmployerAddress())
      {
        local.Employer.Name = entities.Employer.Name;
        local.EmployerAddress.Assign(entities.EmployerAddress);
      }
      else
      {
        // Write to the error file and skip the record.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reading employer/employer address for income source id : " + local
          .BatchTimestampWorkArea.TextTimestamp;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;

        continue;
      }

      local.ServiceProvider2.Assign(local.NullServiceProvider);
      local.Office.Name = "";

      // -- The following IF statement is simply for control purposes.  Inside 
      // the IF we will determine the office and service provider under which
      // this legal action income source will be reported.   Once the
      // determination is made we will escape out of the IF statement.
      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Determine the service provider and office under which the 
        // delinquent IWO should be reported.
        // -- Check for a legal service provider assigned to the IWO legal 
        // action.
        if (ReadOfficeServiceProvider1())
        {
          goto Test;
        }
        else
        {
          // -- There is no legal service provider assigned to the legal action.
          // Continue...
        }

        // -- Check for a legal service provider assigned to the Enforcement 
        // Legal Referral.
        foreach(var item1 in ReadLegalReferral())
        {
          if (Equal(entities.LegalReferral.ReferralReason1, "ENF") || Equal
            (entities.LegalReferral.ReferralReason2, "ENF") || Equal
            (entities.LegalReferral.ReferralReason3, "ENF") || Equal
            (entities.LegalReferral.ReferralReason4, "ENF") || Equal
            (entities.LegalReferral.ReferralReason5, "ENF"))
          {
            if (ReadOfficeServiceProvider2())
            {
              goto Test;
            }
            else
            {
              // -- There is no legal service provider assigned to the 
              // Enforcement legal referral.  Continue...
            }
          }
        }

        // -- Check for a legal service provider assigned to a non Enforcement 
        // Legal Referral.
        foreach(var item1 in ReadLegalReferral())
        {
          if (Equal(entities.LegalReferral.ReferralReason1, "ENF") || Equal
            (entities.LegalReferral.ReferralReason2, "ENF") || Equal
            (entities.LegalReferral.ReferralReason3, "ENF") || Equal
            (entities.LegalReferral.ReferralReason4, "ENF") || Equal
            (entities.LegalReferral.ReferralReason5, "ENF"))
          {
            continue;
          }
          else if (ReadOfficeServiceProvider2())
          {
            goto Test;
          }
          else
          {
            // -- There is no legal service provider assigned to the non 
            // Enforcement legal referral.  Continue...
          }
        }

        // -- Check for an end dated legal service provider assigned to the IWO 
        // legal action.
        ReadLegalActionAssigmentOfficeServiceProvider();
      }

Test:

      if (entities.OfficeServiceProvider.Populated)
      {
        // -- We found an office service provider who is 'responsible' for this 
        // IWGL record.  We will report the delinquent IWO under the office and
        // service provider name associated to this office service provider.
        if (ReadOffice3())
        {
          local.Office.Name = entities.Office.Name;
        }
        else
        {
          // Write to the error file and skip the record...
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading Office for income source id : " + local
            .BatchTimestampWorkArea.TextTimestamp;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.TotalErrors.Count;

          continue;
        }

        if (ReadServiceProvider2())
        {
          local.ServiceProvider2.Assign(entities.ServiceProvider);
        }
        else
        {
          // Write to the error file and skip the record...
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading Service Provider for income source id : " + local
            .BatchTimestampWorkArea.TextTimestamp;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.TotalErrors.Count;

          continue;
        }
      }
      else
      {
        // -- We did NOT find an office service provider who is 'responsible' 
        // for this IWGL record.  We will report the delinquent IWO under the
        // office that provides the Enforcement function for the county
        // associated to the tribunal that issued the Legal Action.
        if (ReadFips())
        {
          if (Equal(entities.Fips.StateAbbreviation, "KS"))
          {
            // -- Find the office that provides the Enforcement function for the
            // county associated to the tribunal that issued the legal action.
            if (ReadOffice1())
            {
              local.Office.Name = entities.Office.Name;
            }
            else
            {
              // Write to the error file and skip the record...
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error reading enforcement Office for income source id : " + local
                .BatchTimestampWorkArea.TextTimestamp;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ++local.TotalErrors.Count;

              continue;
            }
          }
          else
          {
            // -- Report under Office 21 (Interstate) for non Kansas tribunals.
            if (ReadOffice2())
            {
              local.Office.Name = entities.Office.Name;
            }
            else
            {
              // Write to the error file and skip the record...
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error reading interstate Office for income source id : " + local
                .BatchTimestampWorkArea.TextTimestamp;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ++local.TotalErrors.Count;

              continue;
            }
          }
        }
        else if (ReadFipsTribAddress())
        {
          // -- The tribunal is a foreign tribunal.  Use Office 21 (Interstate) 
          // for non Kansas tribunals.
          if (ReadOffice2())
          {
            local.Office.Name = entities.Office.Name;
          }
          else
          {
            // Write to the error file and skip the record...
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error reading interstate Office for income source id : " + local
              .BatchTimestampWorkArea.TextTimestamp;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.TotalErrors.Count;

            continue;
          }
        }
        else
        {
          // Write to the error file and skip the record...
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading legal action tribunal for income source id : " + local
            .BatchTimestampWorkArea.TextTimestamp;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.TotalErrors.Count;

          continue;
        }
      }

      if (entities.Office.Populated)
      {
        switch(AsChar(entities.Office.TypeCode))
        {
          case 'F':
            local.Area.TypeCode = "1";

            // 08/26/11  GVandy CQ29124  Add reason_code = 'RC' when reading for
            // regional office.
            if (ReadCseOrganization())
            {
              local.Area.Name = entities.AreaCseOrganization.Name;
            }
            else
            {
              local.Area.Name = "<Office Not Found>";
            }

            break;
          case 'I':
            local.Area.TypeCode = "2";
            local.Area.Name = "INTERSTATE";

            break;
          case 'E':
            local.Area.TypeCode = "3";
            local.Area.Name = "CONTRACT OFFICE";

            break;
          default:
            local.Area.TypeCode = "0";
            local.Area.Name = "<Unknown>";

            break;
        }
      }
      else
      {
        local.Area.TypeCode = "0";
        local.Area.Name = "<Not Applicable>";
      }

      local.EabFileHandling.Action = "WRITE";
      UseLeEabWriteIwo45DayInfo1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing record to external file for 'le_iwo_45_day_delinquency_info'.";
          
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ++local.Record.Count;
    }

    local.EabReportSend.RptDetail =
      "Total Number Of Delinquent IWO Records Written  :  " + NumberToString
      (local.Record.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Delinquent IWO Records Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Total Number of Errors Written  :  " + NumberToString
      (local.TotalErrors.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Errors Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.FileOpened.Flag) == 'Y')
    {
      // ******************************************
      // *  Close IWO Delinquency File            *
      // ******************************************
      local.EabFileHandling.Action = "CLOSE";
      UseLeEabWriteIwo45DayInfo2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error in closing external file  for 'le_write_iwo_45_day_info'.";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_CLOSE_ERROR";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while closing control report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    local.Close.Ssn = "close";
    UseEabReadCsePersonUsingSsn();

    // Do NOT need to verify on Return
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

    useImport.DateWorkArea.Date = local.StartDateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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

  private void UseCabReadAdabasPersonBatch1()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabReadAdabasPersonBatch2()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.Close.Ssn;

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseLeEabWriteIwo45DayInfo1()
  {
    var useImport = new LeEabWriteIwo45DayInfo.Import();
    var useExport = new LeEabWriteIwo45DayInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ReportingPeriodStart.Date = local.StartDateWorkArea.Date;
    useImport.IwglCreatedDate.Date = local.IwglCreatedDate.Date;
    useImport.CsePersonsWorkSet.FormattedName =
      local.CsePersonsWorkSet.FormattedName;
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    useImport.Employer.Name = local.Employer.Name;
    useImport.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ServiceProvider.Assign(local.ServiceProvider2);
    useImport.Office.Name = local.Office.Name;
    MoveOffice(local.Area, useImport.Area);
    useImport.Caseworker.FormattedName = local.ServiceProvider1.FormattedName;
    MoveEabFileHandling(local.EabFileHandling, useExport.EabFileHandling);

    Call(LeEabWriteIwo45DayInfo.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseLeEabWriteIwo45DayInfo2()
  {
    var useImport = new LeEabWriteIwo45DayInfo.Import();
    var useExport = new LeEabWriteIwo45DayInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabFileHandling(local.EabFileHandling, useExport.EabFileHandling);

    Call(LeEabWriteIwo45DayInfo.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiFormatCsePersonName1()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.ServiceProvider1);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.ServiceProvider1.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName2()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtOrderNumber", entities.LegalAction.StandardNumber ?? ""
          );
        db.SetDate(
          command, "collectionDate",
          local.IwglCreatedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.NumberOfPayments.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCseOrganization()
  {
    System.Diagnostics.Debug.Assert(entities.Office.Populated);
    entities.AreaCseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.
          SetString(command, "cogParentType", entities.Office.CogTypeCode ?? "");
          
        db.SetString(command, "cogParentCode", entities.Office.CogCode ?? "");
      },
      (db, reader) =>
      {
        entities.AreaCseOrganization.Code = db.GetString(reader, 0);
        entities.AreaCseOrganization.Type1 = db.GetString(reader, 1);
        entities.AreaCseOrganization.Name = db.GetString(reader, 2);
        entities.AreaCseOrganization.Populated = true;
      });
  }

  private bool ReadEmployerEmployerAddress()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.EmployerAddress.Populated = false;
    entities.Employer.Populated = false;

    return Read("ReadEmployerEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.IncomeSource.EmpId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.Employer.Name = db.GetNullableString(reader, 1);
        entities.EmployerAddress.LocationType = db.GetString(reader, 2);
        entities.EmployerAddress.City = db.GetNullableString(reader, 3);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 4);
        entities.EmployerAddress.State = db.GetNullableString(reader, 5);
        entities.EmployerAddress.Populated = true;
        entities.Employer.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.State = db.GetString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadIncomeSourceCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionIncomeSource.Populated);
    entities.IncomeSource.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadIncomeSourceCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "cspINumber", entities.LegalActionIncomeSource.CspNumber);
        db.SetDateTime(
          command, "identifier",
          entities.LegalActionIncomeSource.IsrIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.Name = db.GetNullableString(reader, 2);
        entities.IncomeSource.CspINumber = db.GetString(reader, 3);
        entities.CsePerson.Number = db.GetString(reader, 3);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
      });
  }

  private bool ReadLegalActionAssigmentOfficeServiceProvider()
  {
    entities.LegalActionAssigment.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadLegalActionAssigmentOfficeServiceProvider",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.LegalActionAssigment.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionIncomeSource()
  {
    entities.LegalActionIncomeSource.Populated = false;

    return ReadEach("ReadLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp1",
          local.StartDateWorkArea.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp2", local.End.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 5);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 7);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 8);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 9);
        entities.LegalActionIncomeSource.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionIncomeSource.Populated);
    entities.Tribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.LegalActionIncomeSource.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.ActionTaken = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 6);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 7);
        entities.Tribunal.Populated = true;
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 3);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 4);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 8);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 9);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 10);
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private bool ReadNonEmployIncomeSourceAddress()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.NonEmployIncomeSourceAddress.Populated = false;

    return Read("ReadNonEmployIncomeSourceAddress",
      (db, command) =>
      {
        db.SetString(command, "csePerson", entities.IncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NonEmployIncomeSourceAddress.IsrIdentifier =
          db.GetDateTime(reader, 0);
        entities.NonEmployIncomeSourceAddress.City =
          db.GetNullableString(reader, 1);
        entities.NonEmployIncomeSourceAddress.State =
          db.GetNullableString(reader, 2);
        entities.NonEmployIncomeSourceAddress.LocationType =
          db.GetString(reader, 3);
        entities.NonEmployIncomeSourceAddress.CsePerson =
          db.GetString(reader, 4);
        entities.NonEmployIncomeSourceAddress.Populated = true;
        CheckValid<NonEmployIncomeSourceAddress>("LocationType",
          entities.NonEmployIncomeSourceAddress.LocationType);
      });
  }

  private bool ReadOffice1()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cogCode", entities.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 3);
        entities.Office.CogCode = db.GetNullableString(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOffice2()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice2",
      null,
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 3);
        entities.Office.CogCode = db.GetNullableString(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOffice3()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice3",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 3);
        entities.Office.CogCode = db.GetNullableString(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 3);
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
    /// A value of NumberOfDaysInPeriod.
    /// </summary>
    [JsonPropertyName("numberOfDaysInPeriod")]
    public Common NumberOfDaysInPeriod
    {
      get => numberOfDaysInPeriod ??= new();
      set => numberOfDaysInPeriod = value;
    }

    /// <summary>
    /// A value of RptPeriodNumOfWeeks.
    /// </summary>
    [JsonPropertyName("rptPeriodNumOfWeeks")]
    public Common RptPeriodNumOfWeeks
    {
      get => rptPeriodNumOfWeeks ??= new();
      set => rptPeriodNumOfWeeks = value;
    }

    /// <summary>
    /// A value of LegalActionId.
    /// </summary>
    [JsonPropertyName("legalActionId")]
    public WorkArea LegalActionId
    {
      get => legalActionId ??= new();
      set => legalActionId = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public WorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of ServiceProvider1.
    /// </summary>
    [JsonPropertyName("serviceProvider1")]
    public CsePersonsWorkSet ServiceProvider1
    {
      get => serviceProvider1 ??= new();
      set => serviceProvider1 = value;
    }

    /// <summary>
    /// A value of NumberOfWeeksBack.
    /// </summary>
    [JsonPropertyName("numberOfWeeksBack")]
    public Common NumberOfWeeksBack
    {
      get => numberOfWeeksBack ??= new();
      set => numberOfWeeksBack = value;
    }

    /// <summary>
    /// A value of Area.
    /// </summary>
    [JsonPropertyName("area")]
    public Office Area
    {
      get => area ??= new();
      set => area = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of NullServiceProvider.
    /// </summary>
    [JsonPropertyName("nullServiceProvider")]
    public ServiceProvider NullServiceProvider
    {
      get => nullServiceProvider ??= new();
      set => nullServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider2.
    /// </summary>
    [JsonPropertyName("serviceProvider2")]
    public ServiceProvider ServiceProvider2
    {
      get => serviceProvider2 ??= new();
      set => serviceProvider2 = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
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
    /// A value of IwglCreatedDate.
    /// </summary>
    [JsonPropertyName("iwglCreatedDate")]
    public DateWorkArea IwglCreatedDate
    {
      get => iwglCreatedDate ??= new();
      set => iwglCreatedDate = value;
    }

    /// <summary>
    /// A value of NumberOfPayments.
    /// </summary>
    [JsonPropertyName("numberOfPayments")]
    public Common NumberOfPayments
    {
      get => numberOfPayments ??= new();
      set => numberOfPayments = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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
    /// A value of StartDateWorkArea.
    /// </summary>
    [JsonPropertyName("startDateWorkArea")]
    public DateWorkArea StartDateWorkArea
    {
      get => startDateWorkArea ??= new();
      set => startDateWorkArea = value;
    }

    /// <summary>
    /// A value of TotalErrors.
    /// </summary>
    [JsonPropertyName("totalErrors")]
    public Common TotalErrors
    {
      get => totalErrors ??= new();
      set => totalErrors = value;
    }

    /// <summary>
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
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
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public CsePersonsWorkSet Close
    {
      get => close ??= new();
      set => close = value;
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
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
    }

    private Common numberOfDaysInPeriod;
    private Common rptPeriodNumOfWeeks;
    private WorkArea legalActionId;
    private WorkArea convert;
    private CsePersonsWorkSet serviceProvider1;
    private Common numberOfWeeksBack;
    private Office area;
    private Employer employer;
    private EmployerAddress employerAddress;
    private Office office;
    private TextWorkArea textWorkArea;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private ServiceProvider nullServiceProvider;
    private ServiceProvider serviceProvider2;
    private Common errOnAdabasUnavailable;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea iwglCreatedDate;
    private Common numberOfPayments;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea end;
    private DateWorkArea startDateWorkArea;
    private Common totalErrors;
    private Common record;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common fileOpened;
    private AbendData abendData;
    private CsePersonsWorkSet close;
    private Common startCommon;
    private Common current;
    private Common currentPosition;
    private Common fieldNumber;
    private TextWorkArea postion;
    private WorkArea workArea;
    private Common numberOfDays;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of AreaCseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("areaCseOrganizationRelationship")]
    public CseOrganizationRelationship AreaCseOrganizationRelationship
    {
      get => areaCseOrganizationRelationship ??= new();
      set => areaCseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of AreaCseOrganization.
    /// </summary>
    [JsonPropertyName("areaCseOrganization")]
    public CseOrganization AreaCseOrganization
    {
      get => areaCseOrganization ??= new();
      set => areaCseOrganization = value;
    }

    /// <summary>
    /// A value of AreaOffice.
    /// </summary>
    [JsonPropertyName("areaOffice")]
    public Office AreaOffice
    {
      get => areaOffice ??= new();
      set => areaOffice = value;
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
    /// A value of NonEmployIncomeSourceAddress.
    /// </summary>
    [JsonPropertyName("nonEmployIncomeSourceAddress")]
    public NonEmployIncomeSourceAddress NonEmployIncomeSourceAddress
    {
      get => nonEmployIncomeSourceAddress ??= new();
      set => nonEmployIncomeSourceAddress = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of CountyService.
    /// </summary>
    [JsonPropertyName("countyService")]
    public CountyService CountyService
    {
      get => countyService ??= new();
      set => countyService = value;
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
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
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

    private LegalActionDetail legalActionDetail;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
    private CaseAssignment caseAssignment;
    private CseOrganizationRelationship areaCseOrganizationRelationship;
    private CseOrganization areaCseOrganization;
    private Office areaOffice;
    private CollectionType collectionType;
    private NonEmployIncomeSourceAddress nonEmployIncomeSourceAddress;
    private FipsTribAddress fipsTribAddress;
    private CountyService countyService;
    private Fips fips;
    private CseOrganization cseOrganization;
    private Tribunal tribunal;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferral legalReferral;
    private LegalActionAssigment legalActionAssigment;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private IncomeSource incomeSource;
    private EmployerAddress employerAddress;
    private Employer employer;
    private CashReceiptDetail cashReceiptDetail;
    private LegalAction legalAction;
    private LegalActionIncomeSource legalActionIncomeSource;
    private CsePerson csePerson;
  }
#endregion
}
