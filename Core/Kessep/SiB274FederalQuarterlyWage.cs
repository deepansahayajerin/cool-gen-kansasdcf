// Program: SI_B274_FEDERAL_QUARTERLY_WAGE, ID: 371072505, model: 746.
// Short name: SWEI274B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B274_FEDERAL_QUARTERLY_WAGE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB274FederalQuarterlyWage: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B274_FEDERAL_QUARTERLY_WAGE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB274FederalQuarterlyWage(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB274FederalQuarterlyWage.
  /// </summary>
  public SiB274FederalQuarterlyWage(IContext context, Import import,
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
    // ***********************************************************************
    // *                   Maintenance Log
    // 
    // *
    // ***********************************************************************
    // *    DATE       NAME      REQUEST      DESCRIPTION                    *
    // * ----------  ---------  
    // ---------
    // --------------------------------
    // *
    // * 05/19/2001  Ed Lyman   WR10364     Initial Coding.                  *
    // *
    // 
    // *
    // * 03/25/2002  Ed Lyman   PR140565    Generate alert when employee     *
    // *
    // 
    // receives ten percent increase.   *
    // *
    // 
    // *
    // * 08/07/2003  Bonnie Lee  PR185104   Added a new value of 'P'         *
    // 
    // *                                    for military code and a value of *
    // *                                    'Pension/Retired' for note for   *
    // 
    // *                                    a military status of 'P'.        *
    // * 09/23/2003  Hans Woheel  PR187516  Change alerts from within 6      *
    // *				     months to within 1 year          *
    // * 02/02/2009  Raj S        CQ114     Modified to generate worker alert*
    // *
    // 
    // for NDNH response SSN & CSE SSNs *
    // *
    // 
    // mismatches.                      *
    // ***********************************************************************
    // 07/31/2009   DDupree     Added check when processing the returning ssn to
    // see
    //  if it is a invalid ssn and person number combination. Part of CQ7189.
    // __________________________________________________________________________________
    // 05/07/2013   LSS         CQ38148
    // Disabled code that was added for CQ33281 to generate IWOs when return 
    // code is SPACES
    // and changed the process to check for a prior IWO - if no IWO exists 
    // create an IWO;
    // if an IWO exists use MONTHS parameter on MPPI to determine if to create a
    // new IWO.
    // Changed process when employer/employment already exists with an end date 
    // to check for prior
    // IWO - if an IWO exists use DAYS parameter on the MPPI to determine if to 
    // create new IWO.
    // Also, if there is an an end-dated and an active employment record for 
    // same employer/employment
    // and NCP then just the wage information is updated and no IWO is 
    // initiated.
    // Added code for if active employer/employment exists with Return Date and 
    // Return Code of "O"
    // to check for prior IWO - if no IWO exists create an IWO; if an IWO exists
    // use the MONTHS parameter
    // on the MPPI to determine if to create a new IWO.
    // *********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.Max.Date = new DateTime(2099, 12, 31);

    local.AlternateSsn.Index = 0;
    local.AlternateSsn.CheckSize();

    local.Names.Index = 0;
    local.Names.CheckSize();

    UseSiB274Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.AutomaticGenerateIwo1.Flag = local.AutomaticGenerateIwo.Flag;
    local.CompareDateMonths.Date =
      AddMonths(local.Process.Date, -local.MonthsDiff.Count);
    local.CompareDateMonths.Time =
      StringToTime("00.00.00.00000000").GetValueOrDefault();
    UseFnBuildTimestampFrmDateTime();
    local.CompareDate.Date =
      AddDays(local.Process.Date, -local.NumberOfDays.Count);
    local.AddressSuitableForIwo.Flag = "N";

    // **********************************************************
    // Read each input record from the file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      local.Employment.Assign(local.Clear);
      UseEabReadFederalQuarterlyWage();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          break;
        case "EOF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING FEDERAL QUARTERLY WAGE INPUT FILE";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      ++local.RecordsRead.Count;
      local.Kaecses.Number = local.FederalCaseRegistry.Number;

      switch(AsChar(local.RecordType.Text1))
      {
        case '1':
          break;
        case '3':
          ++local.RecordsSkippedNotType1.Count;

          continue;
        default:
          ++local.RecordsSkippedUnknown.Count;

          continue;
      }

      if (ReadCsePerson())
      {
        // **********************************************************
        // 07/29/2009   DDupree   added this check as part of cq7189.
        // **********************************************************
        local.ConvertSsn.SsnNum9 =
          (int)StringToNumber(local.FederalCaseRegistry.Ssn);

        if (ReadInvalidSsn())
        {
          local.ConvertDateWorkArea.Text15 =
            NumberToString(local.Year.Year, 15);
          local.ConvertDateWorkArea.Text4 =
            Substring(local.ConvertDateWorkArea.Text15, 12, 4);

          switch(AsChar(local.Quarter.Text1))
          {
            case '1':
              local.ConvertDateWorkArea.Text6 = "1Q" + local
                .ConvertDateWorkArea.Text4;

              break;
            case '2':
              local.ConvertDateWorkArea.Text6 = "2Q" + local
                .ConvertDateWorkArea.Text4;

              break;
            case '3':
              local.ConvertDateWorkArea.Text6 = "3Q" + local
                .ConvertDateWorkArea.Text4;

              break;
            case '4':
              local.ConvertDateWorkArea.Text6 = "4Q" + local
                .ConvertDateWorkArea.Text4;

              break;
            default:
              ++local.RecordsSkippedUnknown.Count;

              continue;
          }

          local.ConvertMessage.SsnTextPart1 =
            Substring(local.FederalCaseRegistry.Ssn, 1, 3);
          local.ConvertMessage.SsnTextPart2 =
            Substring(local.FederalCaseRegistry.Ssn, 4, 2);
          local.ConvertMessage.SsnTextPart3 =
            Substring(local.FederalCaseRegistry.Ssn, 6, 4);
          local.Message2.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
            .ConvertMessage.SsnTextPart2 + "-" + local
            .ConvertMessage.SsnTextPart3;
          local.AmountConvert.Count = (int)(local.Wage.AverageCurrency * 100);
          local.ConvertDateWorkArea.Text15 =
            NumberToString(local.AmountConvert.Count, 15);
          local.Amount.Text2 =
            Substring(local.ConvertDateWorkArea.Text15, 14, 2);
          local.Amount.Text6 =
            Substring(local.ConvertDateWorkArea.Text15, 8, 6);
          local.Amount.Text9 = local.Amount.Text6 + "." + local.Amount.Text2;
          local.Message1.Text8 = "Bad SSN";
          local.Message1.Text6 = ", Per";
          local.Message1.Text16 = ": Rec not used ;";
          local.Message1.Text2 = ",";
          local.Message1.Text1 = "";
          local.Message1.Text80 = TrimEnd(local.ConvertDateWorkArea.Text6) + local
            .Message1.Text2 + TrimEnd(local.Employer.Name) + local
            .Message1.Text2 + TrimEnd(local.EmployerAddress.City) + local
            .Message1.Text2 + TrimEnd(local.EmployerAddress.State) + local
            .Message1.Text1 + TrimEnd(local.EmployerAddress.ZipCode) + local
            .Message1.Text2 + TrimEnd(local.EmployerAddress.Street1);
          local.NeededToWrite.RptDetail = local.Message1.Text8 + local
            .Message2.Text11 + local.Message1.Text6 + local
            .FederalCaseRegistry.Number + local.Message1.Text16 + local
            .Amount.Text9 + local.Message1.Text2 + local.Message1.Text80;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.RecordsPersonNotFound.Count;
          local.Message1.Text8 = "";
          local.Message1.Text6 = "";
          local.Message1.Text16 = "";
          local.Message1.Text2 = "";
          local.Message1.Text1 = "";
          local.Message1.Text80 = "";
          local.Message2.Text11 = "";
          local.Convert.TextDate = "";
          local.NeededToWrite.RptDetail = "";

          continue;
        }
        else
        {
          // this is fine, there is not invalid ssn record for this combination 
          // of cse person number and ssn number
        }

        UseSiCabReadAdabasBatch();

        if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
        {
          ++local.RecordsPersonNotFound.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("ACO_ADABAS_PERSON_NF_113"))
        {
          ++local.RecordsPersonNotFound.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("ADABAS_INVALID_SSN_W_RB"))
        {
          ++local.RecordsPersonNotFound.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          break;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          break;
        }
      }
      else
      {
        ++local.RecordsPersonNotFound.Count;
        local.NeededToWrite.RptDetail = "Person Not Found - AP # " + local
          .FederalCaseRegistry.Number + "  " + TrimEnd
          (local.FederalCaseRegistry.LastName) + ", " + TrimEnd
          (local.FederalCaseRegistry.FirstName) + " " + "" + local
          .FederalCaseRegistry.MiddleInitial;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }

        continue;
      }

      if (!Equal(local.FederalCaseRegistry.Ssn, local.Kaecses.Ssn))
      {
        local.Batch.Flag = "Y";
        UseCabRetrieveAliasesAndAltSsn();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(local.AlternateSsn.Index = 0; local.AlternateSsn.Index < local
          .AlternateSsn.Count; ++local.AlternateSsn.Index)
        {
          if (!local.AlternateSsn.CheckSize())
          {
            break;
          }

          if (Equal(local.FederalCaseRegistry.Ssn,
            local.AlternateSsn.Item.Gssn.Ssn))
          {
            local.Employment.Note = "Matched alternate ssn: " + Substring
              (local.AlternateSsn.Item.Gssn.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 1, 3) + "-" + Substring
              (local.AlternateSsn.Item.Gssn.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 4, 2) + "-" + Substring
              (local.AlternateSsn.Item.Gssn.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
            local.Employment.Note = TrimEnd(local.Employment.Note) + " name on W-2: " +
              TrimEnd(local.FederalCaseRegistry.LastName) + ", " + TrimEnd
              (local.FederalCaseRegistry.FirstName) + " " + local
              .FederalCaseRegistry.MiddleInitial;

            goto Test1;
          }
        }

        local.AlternateSsn.CheckIndex();
        ++local.RecordsSkippedSsn.Count;

        // ***************************************************************************
        // Write to report of ssn's don't match.  Skip to next person.
        // ***************************************************************************
        UseSiB273SsnMismatchReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }

        // **************************************************************************************
        // Generate worker alert for mismatched SSNs by using the Action Block
        // SI_B273_SSN_MISMATCH_ALERTS_GEN.   CQ114 Changes Start
        // **************************************************************************************
        local.Infrastructure.EventId = 10;
        local.Infrastructure.ReasonCode = "FCRNEWSSNNDNHQW";
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.ReferenceDate = local.Process.Date;
        local.Infrastructure.UserId = local.ProgramProcessingInfo.Name;
        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.BusinessObjectCd = "FCR";
        local.Infrastructure.CreatedBy = global.UserId;
        local.Infrastructure.LastUpdatedBy = "";
        local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
        local.Infrastructure.Detail = "SSN:" + TrimEnd
          (local.FederalCaseRegistry.Ssn) + ", PF16 to view Employer Name (expires in 1 Yr)";
          
        ExitState = "ACO_NN0000_ALL_OK";
        local.NarrativeDetail.NarrativeText =
          "**Employer Record from NDNH QW through SWEIB274 Batch Process**";
        UseSiB273SsnMismatchAlertsGen();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // **************************************************************************************
        // Generate worker alert for mismatched SSNs by using the Action Block
        // SI_B273_SSN_MISMATCH_ALERTS_GEN.   CQ114 Changes End
        // **************************************************************************************
        continue;
      }

Test1:

      if (IsEmpty(local.FederalCaseRegistry.Flag))
      {
        ++local.EmployeeNameMismatch.Count;

        if (IsEmpty(local.Employment.Note))
        {
          local.Employment.Note = TrimEnd("") + "Employee name on W-2 is: " + TrimEnd
            (local.FederalCaseRegistry.LastName) + ", " + TrimEnd
            (local.FederalCaseRegistry.FirstName) + " " + local
            .FederalCaseRegistry.MiddleInitial;
        }

        // ***************************************************************************
        // Write to report of names don't match.  Continue processing.
        // ***************************************************************************
        UseSiB273EmployeeNameMismatch();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }
      }

      UseSiB273ValidateAndNewhireRpt();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        break;
      }

      if (AsChar(local.Valid.Flag) == 'N')
      {
        ++local.RecordsSkippedNuhireRpt.Count;

        continue;
      }

      // ****************************************************************************
      // At this point, for the current record, no creates or updates have been 
      // done.
      // ****************************************************************************
      switch(AsChar(local.Quarter.Text1))
      {
        case '1':
          local.BeginQuarter.Date = IntToDate(local.Year.Year * 10000 + 101);

          break;
        case '2':
          local.BeginQuarter.Date = IntToDate(local.Year.Year * 10000 + 401);

          break;
        case '3':
          local.BeginQuarter.Date = IntToDate(local.Year.Year * 10000 + 701);

          break;
        case '4':
          local.BeginQuarter.Date = IntToDate(local.Year.Year * 10000 + 1001);

          break;
        default:
          ++local.RecordsSkippedUnknown.Count;

          continue;
      }

      // **************************************************************************
      // Military status of space or C indicates civilian employment.
      // Employment is a subtype of income source.  Does this person already 
      // have a
      // record of employment for this employer?  Valid return codes:
      //  Space = No verification from employer
      //      E = Employed
      //      F = Fired
      //      L = Laid Off
      //      N = Never Worked
      //      O = Other
      //      Q = Quit
      //      W = Receiving Workmen's Compensation
      // **************************************************************************
      local.TenPercentIncrease.Flag = "N";

      if (IsEmpty(local.MilitaryStatus.Text1) || AsChar
        (local.MilitaryStatus.Text1) == 'C')
      {
        foreach(var item in ReadEmployment1())
        {
          local.CurrentEmployment.Flag = "Y";
          local.Employment.Assign(local.Clear);

          if (Lt(entities.Employment.EndDt, local.BeginQuarter.Date))
          {
            // ***************************************************************
            // The employee may have been re-hired.  In this situation,
            // a new employment must be created.
            // ***************************************************************
            break;
          }

          if (!IsEmpty(entities.Employment.ReturnCd) && AsChar
            (entities.Employment.ReturnCd) != 'E')
          {
            if (AsChar(entities.Employment.ReturnCd) == 'O' && Equal
              (entities.Employment.EndDt, new DateTime(2099, 12, 31)))
            {
              foreach(var item1 in ReadLegalActionIncomeSource())
              {
                if (!Lt(entities.LegalActionIncomeSource.CreatedTstamp,
                  local.CompareDateMonths.Timestamp))
                {
                  // DO NOT create Income Source - DO NOT create auto IWO
                  // - update Income Source wages
                  goto Test2;
                }
                else
                {
                  // Create Income Source with Return Date, Return Code, Type
                  // - create auto IWO
                  goto ReadEach;
                }
              }

              // No existing IWO - create Income Source with Return Date,Return 
              // Code, Type
              // - create auto IWO
              break;
            }

            // ***************************************************************
            // The return code indicates the employee is not employed at this
            // employer as of the return date, regardless of whether the
            // employment has been end dated.  The employee may have been
            // re-hired.  In this situation, a new employment must be created.
            // ***************************************************************
            if (Lt(entities.Employment.ReturnDt, local.BeginQuarter.Date))
            {
              break;
            }
          }

Test2:

          // CQ38148 - Changed the process for no Return Code to check for a 
          // prior IWO
          if (IsEmpty(entities.Employment.ReturnCd))
          {
            foreach(var item1 in ReadLegalActionIncomeSource())
            {
              if (!Lt(entities.LegalActionIncomeSource.CreatedTstamp,
                local.CompareDateMonths.Timestamp))
              {
                // Update Income Source - Do NOT create auto IWO
                goto Test3;
              }
              else
              {
                // Update Income Source with Return Date and Return Code
                // - create auto IWO
                break;
              }
            }

            // No existing IWO - update Income Source Return Date and Return 
            // Code
            // - create auto IWO
            UseSiB273MaintainEmployer();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto AfterCycle;
            }

            MoveIncomeSource1(entities.Employment, local.Employment);

            switch(AsChar(local.Quarter.Text1))
            {
              case '1':
                local.Employment.LastQtr = local.Quarter.Text1;
                local.Employment.LastQtrIncome = local.Wage.AverageCurrency;
                local.Employment.LastQtrYr = local.Year.Year;

                if (!Lt(entities.Employment.LastQtrYr, local.Year.Year))
                {
                  ++local.RecordsAlreadyProcessed.Count;

                  if (Equal(local.Wage.AverageCurrency,
                    entities.Employment.LastQtrIncome) || Lt
                    (local.Year.Year, entities.Employment.LastQtrYr))
                  {
                  }
                  else
                  {
                    UseSiB274RevisedWageReport();
                  }
                }
                else
                {
                  MoveIncomeSource1(entities.Employment, local.Employment);
                  local.Employment.LastQtr = local.Quarter.Text1;
                  local.Employment.LastQtrYr = local.Year.Year;
                  local.Employment.LastQtrIncome = local.Wage.AverageCurrency;

                  if (Lt(2600, entities.Employment.Attribute4ThQtrIncome) && Lt
                    (2600, entities.Employment.Attribute3RdQtrIncome) && Equal
                    (entities.Employment.Attribute4ThQtrYr, local.Year.Year - 1) &&
                    Equal
                    (entities.Employment.Attribute3RdQtrYr, local.Year.Year - 1))
                    
                  {
                    if (local.Wage.AverageCurrency >= entities
                      .Employment.Attribute4ThQtrIncome.GetValueOrDefault() * 1.1M
                      && local.Wage.AverageCurrency >= entities
                      .Employment.Attribute3RdQtrIncome.GetValueOrDefault() * 1.1M
                      && Lt
                      (entities.Employment.SentDt, AddMonths(local.Process.Date,
                      -6)))
                    {
                      local.TenPercentIncrease.Flag = "Y";
                    }
                  }
                }

                break;
              case '2':
                local.Employment.Attribute2NdQtr = local.Quarter.Text1;
                local.Employment.Attribute2NdQtrIncome =
                  local.Wage.AverageCurrency;
                local.Employment.Attribute2NdQtrYr = local.Year.Year;

                if (!Lt(entities.Employment.Attribute2NdQtrYr, local.Year.Year))
                {
                  ++local.RecordsAlreadyProcessed.Count;

                  if (Equal(local.Wage.AverageCurrency,
                    entities.Employment.Attribute2NdQtrIncome) || Lt
                    (local.Year.Year, entities.Employment.Attribute2NdQtrYr))
                  {
                  }
                  else
                  {
                    UseSiB274RevisedWageReport();
                  }
                }
                else
                {
                  MoveIncomeSource1(entities.Employment, local.Employment);
                  local.Employment.Attribute2NdQtr = local.Quarter.Text1;
                  local.Employment.Attribute2NdQtrYr = local.Year.Year;
                  local.Employment.Attribute2NdQtrIncome =
                    local.Wage.AverageCurrency;

                  if (Lt(2600, entities.Employment.LastQtrIncome) && Lt
                    (2600, entities.Employment.Attribute4ThQtrIncome) && Equal
                    (entities.Employment.LastQtrYr, local.Year.Year) && Equal
                    (entities.Employment.Attribute4ThQtrYr, local.Year.Year - 1))
                    
                  {
                    if (local.Wage.AverageCurrency >= entities
                      .Employment.LastQtrIncome.GetValueOrDefault() * 1.1M && local
                      .Wage.AverageCurrency >= entities
                      .Employment.Attribute4ThQtrIncome.GetValueOrDefault() * 1.1M
                      && Lt
                      (entities.Employment.SentDt, AddMonths(local.Process.Date,
                      -6)))
                    {
                      local.TenPercentIncrease.Flag = "Y";
                    }
                  }
                }

                break;
              case '3':
                local.Employment.Attribute3RdQtr = local.Quarter.Text1;
                local.Employment.Attribute3RdQtrIncome =
                  local.Wage.AverageCurrency;
                local.Employment.Attribute3RdQtrYr = local.Year.Year;

                if (!Lt(entities.Employment.Attribute3RdQtrYr, local.Year.Year))
                {
                  ++local.RecordsAlreadyProcessed.Count;

                  if (Equal(local.Wage.AverageCurrency,
                    entities.Employment.Attribute3RdQtrIncome) || Lt
                    (local.Year.Year, entities.Employment.Attribute3RdQtrYr))
                  {
                  }
                  else
                  {
                    UseSiB274RevisedWageReport();
                  }
                }
                else
                {
                  MoveIncomeSource1(entities.Employment, local.Employment);
                  local.Employment.Attribute3RdQtr = local.Quarter.Text1;
                  local.Employment.Attribute3RdQtrYr = local.Year.Year;
                  local.Employment.Attribute3RdQtrIncome =
                    local.Wage.AverageCurrency;

                  if (Lt(2600, entities.Employment.Attribute2NdQtrIncome) && Lt
                    (2600, entities.Employment.LastQtrIncome) && Equal
                    (entities.Employment.Attribute2NdQtrYr, local.Year.Year) &&
                    Equal(entities.Employment.LastQtrYr, local.Year.Year))
                  {
                    if (local.Wage.AverageCurrency >= entities
                      .Employment.Attribute2NdQtrIncome.GetValueOrDefault() * 1.1M
                      && local.Wage.AverageCurrency >= entities
                      .Employment.LastQtrIncome.GetValueOrDefault() * 1.1M && Lt
                      (entities.Employment.SentDt, AddMonths(local.Process.Date,
                      -6)))
                    {
                      local.TenPercentIncrease.Flag = "Y";
                    }
                  }
                }

                break;
              case '4':
                local.Employment.Attribute4ThQtr = local.Quarter.Text1;
                local.Employment.Attribute4ThQtrIncome =
                  local.Wage.AverageCurrency;
                local.Employment.Attribute4ThQtrYr = local.Year.Year;

                if (!Lt(entities.Employment.Attribute4ThQtrYr, local.Year.Year))
                {
                  ++local.RecordsAlreadyProcessed.Count;

                  if (Equal(local.Wage.AverageCurrency,
                    entities.Employment.Attribute4ThQtrIncome) || Lt
                    (local.Year.Year, entities.Employment.Attribute4ThQtrYr))
                  {
                  }
                  else
                  {
                    UseSiB274RevisedWageReport();
                  }
                }
                else
                {
                  MoveIncomeSource1(entities.Employment, local.Employment);
                  local.Employment.Attribute4ThQtr = local.Quarter.Text1;
                  local.Employment.Attribute4ThQtrYr = local.Year.Year;
                  local.Employment.Attribute4ThQtrIncome =
                    local.Wage.AverageCurrency;

                  if (Lt(2600, entities.Employment.Attribute3RdQtrIncome) && Lt
                    (2600, entities.Employment.Attribute2NdQtrIncome) && Equal
                    (entities.Employment.Attribute3RdQtrYr, local.Year.Year) &&
                    Equal
                    (entities.Employment.Attribute2NdQtrYr, local.Year.Year))
                  {
                    if (local.Wage.AverageCurrency >= entities
                      .Employment.Attribute3RdQtrYr.GetValueOrDefault() * 1.1M
                      && local.Wage.AverageCurrency >= entities
                      .Employment.Attribute2NdQtrYr.GetValueOrDefault() * 1.1M
                      && Lt
                      (entities.Employment.SentDt, AddMonths(local.Process.Date,
                      -6)))
                    {
                      local.TenPercentIncrease.Flag = "Y";
                    }
                  }
                }

                break;
              default:
                break;
            }

            local.Employment.ReturnCd = "E";
            local.Employment.ReturnDt = local.Process.Date;

            if (!Lt(local.Null1.Date, local.Employment.StartDt))
            {
              local.Employment.StartDt = local.BeginQuarter.Date;
            }

            UseSiCabHireDateAlertIwo2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto AfterCycle;
            }

            if (Lt(AddYears(local.Process.Date, -1), local.BeginQuarter.Date) &&
              AsChar(local.TenPercentIncrease.Flag) == 'Y')
            {
              local.Infrastructure.ReasonCode = "TENQTRWAGE";
              local.Infrastructure.Detail =
                "Ten Percent or more Pay Increase from Employer: " + (
                  local.Employer.Name ?? "");
              UseSiB274SendAlertNewIncSrce();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto AfterCycle;
              }
            }

            local.CurrentEmployment.Flag = "N";

            goto Next;
          }

Test3:

          // CQ38148 END of Changed process
          // CQ38148 - disabled code
          // CQ38148 END - disabled code
          UseSiB274UpdateEmploymntIncSrc1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            goto AfterCycle;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto AfterCycle;
          }

          if (!Equal(local.Employer.Ein, local.PreviouslyCompared.Ein))
          {
            UseSiB273CompareEmployerInfo();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              goto AfterCycle;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto AfterCycle;
            }

            local.PreviouslyCompared.Ein = local.Employer.Ein ?? "";
          }

          if (Lt(AddYears(local.Process.Date, -1), local.BeginQuarter.Date) && AsChar
            (local.TenPercentIncrease.Flag) == 'Y')
          {
            local.Infrastructure.ReasonCode = "TENQTRWAGE";
            local.Infrastructure.Detail =
              "Ten Percent or more Pay Increase from Employer: " + (
                local.Employer.Name ?? "");
            UseSiB274SendAlertNewIncSrce();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto AfterCycle;
            }
          }

          local.CurrentEmployment.Flag = "N";

          goto Next;
        }

ReadEach:
        ;
      }
      else
      {
        // **************************************************************************
        // Military status of A = active duty, R = 
        // reserves and P = pension/retired.
        // 
        // Military is a subtype of income source. Does
        // this person already have a
        // record of military service?  Valid return codes:
        //  Space = No verication from the military
        //      A = Active
        //      I = Inactive
        //      N = None
        //      R = Retired
        //      U = Unknown
        // **************************************************************************
        foreach(var item in ReadMilitary())
        {
          if (Lt(entities.Military.EndDt, local.BeginQuarter.Date))
          {
            // **************************************************************************
            // The service man may have been re-enlisted.  In this situation,
            // a new military income source must be created.
            // **************************************************************************
            break;
          }

          if (AsChar(entities.Military.ReturnCd) != 'A' && AsChar
            (entities.Military.ReturnCd) != 'R' && !
            IsEmpty(entities.Military.ReturnCd))
          {
            local.Employment.LastQtrIncome = local.Wage.AverageCurrency;

            // ***************************************************************
            // The return code indicates the serviceman is not in the service
            // as of the return date, regardless of whether the military has
            // been end dated.  The serviceman may have reenlisted. In this
            // situation, a new military must be created.
            // ***************************************************************
            if (Lt(entities.Military.ReturnDt, local.BeginQuarter.Date))
            {
              break;
            }
          }

          UseSiB274UpdateMilitaryIncSrce();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            goto AfterCycle;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto AfterCycle;
          }

          if (!Equal(local.Employer.Ein, local.PreviouslyCompared.Ein))
          {
            UseSiB273CompareMilitaryInfo();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              goto AfterCycle;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto AfterCycle;
            }

            local.PreviouslyCompared.Ein = local.Employer.Ein ?? "";
          }

          if (Lt(AddYears(local.Process.Date, -1), local.BeginQuarter.Date) && AsChar
            (local.TenPercentIncrease.Flag) == 'Y')
          {
            local.Infrastructure.ReasonCode = "TENQTRWAGE";
            local.Infrastructure.Detail =
              "Ten Percent or more Pay Increase from Military: " + (
                local.Employer.Name ?? "");
            UseSiB274SendAlertNewIncSrce();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto AfterCycle;
            }
          }

          goto Next;
        }
      }

      UseSiB273MaintainEmployer();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      local.Employment.EndDt = local.Max.Date;
      local.Employment.StartDt = local.BeginQuarter.Date;
      local.Employment.Name = local.Employer.Name ?? "";

      if (IsEmpty(local.MilitaryStatus.Text1) || AsChar
        (local.MilitaryStatus.Text1) == 'C')
      {
        local.Employment.ReturnCd = "E";
        local.Employment.ReturnDt = local.Process.Date;
      }
      else
      {
        local.Employment.ReturnCd = "";
        local.Employment.ReturnDt = local.Null1.Date;
      }

      switch(AsChar(local.Quarter.Text1))
      {
        case '1':
          local.Employment.LastQtr = local.Quarter.Text1;
          local.Employment.LastQtrIncome = local.Wage.AverageCurrency;
          local.Employment.LastQtrYr = local.Year.Year;

          break;
        case '2':
          local.Employment.Attribute2NdQtr = local.Quarter.Text1;
          local.Employment.Attribute2NdQtrIncome = local.Wage.AverageCurrency;
          local.Employment.Attribute2NdQtrYr = local.Year.Year;

          break;
        case '3':
          local.Employment.Attribute3RdQtr = local.Quarter.Text1;
          local.Employment.Attribute3RdQtrIncome = local.Wage.AverageCurrency;
          local.Employment.Attribute3RdQtrYr = local.Year.Year;

          break;
        case '4':
          local.Employment.Attribute4ThQtr = local.Quarter.Text1;
          local.Employment.Attribute4ThQtrIncome = local.Wage.AverageCurrency;
          local.Employment.Attribute4ThQtrYr = local.Year.Year;

          break;
        default:
          break;
      }

      // *********************************************************************
      // CQ38141 - additional changes
      // If there is an active employment record with a return date and return
      // code of 'E' do not create an imcome source record just update the
      // wage information.
      // *********************************************************************
      if (AsChar(local.CurrentEmployment.Flag) == 'Y')
      {
        foreach(var item in ReadEmployment2())
        {
          UseSiB274UpdateEmploymntIncSrc2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            goto AfterCycle;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto AfterCycle;
          }

          if (!Equal(local.Employer.Ein, local.PreviouslyCompared.Ein))
          {
            UseSiB273CompareEmployerInfo();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              goto AfterCycle;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto AfterCycle;
            }

            local.PreviouslyCompared.Ein = local.Employer.Ein ?? "";
          }

          if (Lt(AddYears(local.Process.Date, -1), local.BeginQuarter.Date) && AsChar
            (local.TenPercentIncrease.Flag) == 'Y')
          {
            local.Infrastructure.ReasonCode = "TENQTRWAGE";
            local.Infrastructure.Detail =
              "Ten Percent or more Pay Increase from Employer: " + (
                local.Employer.Name ?? "");
            UseSiB274SendAlertNewIncSrce();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto AfterCycle;
            }
          }

          local.CurrentEmployment.Flag = "N";

          goto Next;
        }

        // ***************************************************************
        // CQ38141 - additional changes
        // If the employer/employment already exists with an end date less
        // than or equal to the process date minus the MPPI parameter
        // number of days (originally set at 150) DO NOT create an IWO.
        // If end date is greater than that number of days create an IWO.
        // ***************************************************************
        if (AsChar(local.AutomaticGenerateIwo.Flag) == 'Y')
        {
          if (Equal(entities.Employment.EndDt, new DateTime(2099, 12, 31)))
          {
          }
          else if (!Lt(entities.Employment.EndDt, local.CompareDate.Date))
          {
            local.AutomaticGenerateIwo1.Flag = "N";
          }
        }

        // *******************************************************************
        // CQ38141 END - additional changes
        // *******************************************************************
      }

      UseSiB273CreateEmpIncomeSource();

      if (AsChar(local.AutomaticGenerateIwo.Flag) == 'Y')
      {
        local.AutomaticGenerateIwo1.Flag = "Y";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      // *******************************************************************
      // ***  See if wage update is within 1 year before sending alert ***
      // *******************************************************************
      if (Lt(AddYears(local.Process.Date, -1), local.BeginQuarter.Date))
      {
        local.Infrastructure.ReasonCode = "FEDQTRWAGE";
        local.Infrastructure.Detail = "Employer: " + (
          local.Employer.Name ?? "");
        UseSiB274SendAlertNewIncSrce();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
      }

      if (!Equal(local.FederalCaseRegistry.Ssn, local.Kaecses.Ssn))
      {
        // ***************************************************************
        // Must have matched on alternate SSN, so send alert.
        // ***************************************************************
        local.Infrastructure.Detail = local.Employment.Note ?? "";
        UseSiB273SendAlertNewIncSrce();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      // *************** Check to see if commit is needed ********************
      ++local.Commit.Count;

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.Commit.Count = 0;
        local.EabFileHandling.Action = "WRITE";
        local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-" + NumberToString
          (Now().Date.Month, 14, 2) + "-" + NumberToString
          (Now().Date.Day, 14, 2);
        local.Time.Text8 = NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
        local.NeededToWrite.RptDetail = "Commit taken after record number: " + NumberToString
          (local.RecordsRead.Count, 15) + "  Date: " + local.Date.Text10 + "  Time: " +
          local.Time.Text8;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          break;
        }
      }

Next:
      ;
    }
    while(!Equal(local.EabFileHandling.Status, "EOF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseSiB274Close();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseSiB274Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveAlternateSsn(CabRetrieveAliasesAndAltSsn.Export.
    AlternateSsnGroup source, Local.AlternateSsnGroup target)
  {
    target.Gssn.Ssn = source.Gssn.Ssn;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private static void MoveIncomeSource1(IncomeSource source, IncomeSource target)
    
  {
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.LastQtrIncome = source.LastQtrIncome;
    target.LastQtr = source.LastQtr;
    target.LastQtrYr = source.LastQtrYr;
    target.Attribute2NdQtrIncome = source.Attribute2NdQtrIncome;
    target.Attribute2NdQtr = source.Attribute2NdQtr;
    target.Attribute2NdQtrYr = source.Attribute2NdQtrYr;
    target.Attribute3RdQtrIncome = source.Attribute3RdQtrIncome;
    target.Attribute3RdQtr = source.Attribute3RdQtr;
    target.Attribute3RdQtrYr = source.Attribute3RdQtrYr;
    target.Attribute4ThQtrIncome = source.Attribute4ThQtrIncome;
    target.Attribute4ThQtr = source.Attribute4ThQtr;
    target.Attribute4ThQtrYr = source.Attribute4ThQtrYr;
    target.SentDt = source.SentDt;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.WorkerId = source.WorkerId;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
  }

  private static void MoveIncomeSource2(IncomeSource source, IncomeSource target)
    
  {
    target.LastQtrIncome = source.LastQtrIncome;
    target.LastQtr = source.LastQtr;
    target.LastQtrYr = source.LastQtrYr;
    target.Attribute2NdQtrIncome = source.Attribute2NdQtrIncome;
    target.Attribute2NdQtr = source.Attribute2NdQtr;
    target.Attribute2NdQtrYr = source.Attribute2NdQtrYr;
    target.Attribute3RdQtrIncome = source.Attribute3RdQtrIncome;
    target.Attribute3RdQtr = source.Attribute3RdQtr;
    target.Attribute3RdQtrYr = source.Attribute3RdQtrYr;
    target.Attribute4ThQtrIncome = source.Attribute4ThQtrIncome;
    target.Attribute4ThQtr = source.Attribute4ThQtr;
    target.Attribute4ThQtrYr = source.Attribute4ThQtrYr;
  }

  private static void MoveIncomeSource3(IncomeSource source, IncomeSource target)
    
  {
    target.LastQtrIncome = source.LastQtrIncome;
    target.LastQtr = source.LastQtr;
    target.LastQtrYr = source.LastQtrYr;
    target.Attribute2NdQtrIncome = source.Attribute2NdQtrIncome;
    target.Attribute2NdQtr = source.Attribute2NdQtr;
    target.Attribute2NdQtrYr = source.Attribute2NdQtrYr;
    target.Attribute3RdQtrIncome = source.Attribute3RdQtrIncome;
    target.Attribute3RdQtr = source.Attribute3RdQtr;
    target.Attribute3RdQtrYr = source.Attribute3RdQtrYr;
    target.Attribute4ThQtrIncome = source.Attribute4ThQtrIncome;
    target.Attribute4ThQtr = source.Attribute4ThQtr;
    target.Attribute4ThQtrYr = source.Attribute4ThQtrYr;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.Note = source.Note;
    target.StartDt = source.StartDt;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Detail = source.Detail;
  }

  private static void MoveNames(CabRetrieveAliasesAndAltSsn.Export.
    NamesGroup source, Local.NamesGroup target)
  {
    target.Gname.Assign(source.Gnames);
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabRetrieveAliasesAndAltSsn()
  {
    var useImport = new CabRetrieveAliasesAndAltSsn.Import();
    var useExport = new CabRetrieveAliasesAndAltSsn.Export();

    useImport.Batch.Flag = local.Batch.Flag;
    useImport.CsePersonsWorkSet.Assign(local.Kaecses);

    Call(CabRetrieveAliasesAndAltSsn.Execute, useImport, useExport);

    useExport.Names.CopyTo(local.Names, MoveNames);
    useExport.AlternateSsn.CopyTo(local.AlternateSsn, MoveAlternateSsn);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadFederalQuarterlyWage()
  {
    var useImport = new EabReadFederalQuarterlyWage.Import();
    var useExport = new EabReadFederalQuarterlyWage.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.CsePersonsWorkSet.Assign(local.FederalCaseRegistry);
    useExport.Employer.Assign(local.Employer);
    useExport.EmployerAddress.Assign(local.EmployerAddress);
    useExport.RecordType.Text1 = local.RecordType.Text1;
    useExport.Year.Year = local.Year.Year;
    useExport.Quarter.Text1 = local.Quarter.Text1;
    useExport.Wage.AverageCurrency = local.Wage.AverageCurrency;
    useExport.MilitaryStatus.Text1 = local.MilitaryStatus.Text1;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadFederalQuarterlyWage.Execute, useImport, useExport);

    local.FederalCaseRegistry.Assign(useExport.CsePersonsWorkSet);
    local.Employer.Assign(useExport.Employer);
    local.EmployerAddress.Assign(useExport.EmployerAddress);
    local.RecordType.Text1 = useExport.RecordType.Text1;
    local.Year.Year = useExport.Year.Year;
    local.Quarter.Text1 = useExport.Quarter.Text1;
    local.Wage.AverageCurrency = useExport.Wage.AverageCurrency;
    local.MilitaryStatus.Text1 = useExport.MilitaryStatus.Text1;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ReturnCode.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ReturnCode.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea(local.CompareDateMonths, useImport.DateWorkArea);

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    local.CompareDateMonths.Assign(useExport.DateWorkArea);
  }

  private void UseSiB273CompareEmployerInfo()
  {
    var useImport = new SiB273CompareEmployerInfo.Import();
    var useExport = new SiB273CompareEmployerInfo.Export();

    useImport.Employment.Identifier = entities.Employment.Identifier;
    useImport.EmployerInfoMismatch.Count = local.EmployerInfoMismatch.Count;
    useImport.FcrEmployer.Assign(local.Employer);
    useImport.FcrEmployerAddress.Assign(local.EmployerAddress);

    Call(SiB273CompareEmployerInfo.Execute, useImport, useExport);

    local.EmployerInfoMismatch.Count = useExport.EmployerInfoMismatch.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273CompareMilitaryInfo()
  {
    var useImport = new SiB273CompareMilitaryInfo.Import();
    var useExport = new SiB273CompareMilitaryInfo.Export();

    useImport.Military.Identifier = entities.Military.Identifier;
    useImport.EmployerInfoMismatch.Count = local.EmployerInfoMismatch.Count;
    useImport.FcrEmployer.Assign(local.Employer);
    useImport.FcrEmployerAddress.Assign(local.EmployerAddress);

    Call(SiB273CompareMilitaryInfo.Execute, useImport, useExport);

    local.EmployerInfoMismatch.Count = useExport.EmployerInfoMismatch.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273CreateEmpIncomeSource()
  {
    var useImport = new SiB273CreateEmpIncomeSource.Import();
    var useExport = new SiB273CreateEmpIncomeSource.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.MilitaryStatus.Text1 = local.MilitaryStatus.Text1;
    useImport.IncomeSourceCreated.Count = local.IncomeSourcesCreated.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Employer.Identifier = local.Employer.Identifier;
    useImport.Employment.Assign(local.Employment);
    useImport.AddressSuitableForIwo.Flag = local.AddressSuitableForIwo.Flag;
    useImport.AutomaticGenerateIwo.Flag = local.AutomaticGenerateIwo1.Flag;

    Call(SiB273CreateEmpIncomeSource.Execute, useImport, useExport);

    local.IncomeSourcesCreated.Count = useExport.IncomeSourceCreated.Count;
  }

  private void UseSiB273EmployeeNameMismatch()
  {
    var useImport = new SiB273EmployeeNameMismatch.Import();
    var useExport = new SiB273EmployeeNameMismatch.Export();

    useImport.Kaecses.Assign(local.Kaecses);
    useImport.FederalCaseRegistry.Assign(local.FederalCaseRegistry);

    Call(SiB273EmployeeNameMismatch.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273MaintainEmployer()
  {
    var useImport = new SiB273MaintainEmployer.Import();
    var useExport = new SiB273MaintainEmployer.Export();

    useImport.PreviouslyCompared.Ein = local.PreviouslyCompared.Ein;
    useImport.EmployerMismatch.Count = local.EmployerInfoMismatch.Count;
    useImport.EmployersCreated.Count = local.EmployersCreated.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Employer.Assign(local.Employer);
    useImport.EmployerAddress.Assign(local.EmployerAddress);

    Call(SiB273MaintainEmployer.Execute, useImport, useExport);

    local.AddressSuitableForIwo.Flag = useExport.AddressSuitableForIwo.Flag;
    local.PreviouslyCompared.Ein = useExport.PreviouslyCompared.Ein;
    local.EmployerInfoMismatch.Count = useExport.EmployerMismatch.Count;
    local.EmployersCreated.Count = useExport.EmployersCreated.Count;
    local.Employer.Assign(useExport.Employer);
    local.EmployerAddress.Identifier = useExport.EmployerAddress.Identifier;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273SendAlertNewIncSrce()
  {
    var useImport = new SiB273SendAlertNewIncSrce.Import();
    var useExport = new SiB273SendAlertNewIncSrce.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = local.Process.Date;
    useImport.Employment.Identifier = local.Employment.Identifier;
    useImport.Infrastructure.Detail = local.Infrastructure.Detail;

    Call(SiB273SendAlertNewIncSrce.Execute, useImport, useExport);
  }

  private void UseSiB273SsnMismatchAlertsGen()
  {
    var useImport = new SiB273SsnMismatchAlertsGen.Import();
    var useExport = new SiB273SsnMismatchAlertsGen.Export();

    useImport.WageAmount.AverageCurrency = local.Wage.AverageCurrency;
    useImport.Max.Date = local.Max.Date;
    useImport.Employer.Assign(local.Employer);
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    useImport.FederalCaseRegistry.Assign(local.FederalCaseRegistry);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.EmployerSourceTxt.NarrativeText =
      local.NarrativeDetail.NarrativeText;

    Call(SiB273SsnMismatchAlertsGen.Execute, useImport, useExport);
  }

  private void UseSiB273SsnMismatchReport()
  {
    var useImport = new SiB273SsnMismatchReport.Import();
    var useExport = new SiB273SsnMismatchReport.Export();

    useImport.Kaecses.Assign(local.Kaecses);
    useImport.FederalCaseRegistry.Assign(local.FederalCaseRegistry);

    Call(SiB273SsnMismatchReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273ValidateAndNewhireRpt()
  {
    var useImport = new SiB273ValidateAndNewhireRpt.Import();
    var useExport = new SiB273ValidateAndNewhireRpt.Export();

    useImport.Process.Date = local.Process.Date;
    useImport.Employer.Assign(local.Employer);
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    useImport.CsePersonsWorkSet.Assign(local.FederalCaseRegistry);

    Call(SiB273ValidateAndNewhireRpt.Execute, useImport, useExport);

    local.Valid.Flag = useExport.Valid.Flag;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB274Close()
  {
    var useImport = new SiB274Close.Import();
    var useExport = new SiB274Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.IncomeSourcesCreated.Count = local.IncomeSourcesCreated.Count;
    useImport.NumberIncSrcUpdates.Count = local.NumberIncSrcUpdates.Count;
    useImport.RecordsSkippedNuhireRp.Count =
      local.RecordsSkippedNuhireRpt.Count;
    useImport.RecordsSkippedDateHire.Count = local.RecordsSkippedUnknown.Count;
    useImport.RecordsSkippedSsn.Count = local.RecordsSkippedSsn.Count;
    useImport.RecordsSkippedNotOne.Count = local.RecordsSkippedNotType1.Count;
    useImport.RecordsPersonNotFound.Count = local.RecordsPersonNotFound.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.EmployersCreated.Count = local.EmployersCreated.Count;
    useImport.EmployerInfoMismatch.Count = local.EmployerInfoMismatch.Count;
    useImport.EmployeeNameMismatch.Count = local.EmployeeNameMismatch.Count;

    Call(SiB274Close.Execute, useImport, useExport);
  }

  private void UseSiB274Housekeeping()
  {
    var useImport = new SiB274Housekeeping.Import();
    var useExport = new SiB274Housekeeping.Export();

    Call(SiB274Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.AutomaticGenerateIwo.Flag = useExport.AutomaticGenerateIwo.Flag;
    local.MonthsDiff.Count = useExport.MonthsDiff.Count;
    local.NumberOfDays.Count = useExport.NumberOfDays.Count;
  }

  private void UseSiB274RevisedWageReport()
  {
    var useImport = new SiB274RevisedWageReport.Import();
    var useExport = new SiB274RevisedWageReport.Export();

    MoveIncomeSource2(entities.Employment, useImport.Existing);
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Year.Year = local.Year.Year;
    useImport.Wage.AverageCurrency = local.Wage.AverageCurrency;
    useImport.Quarter.Text1 = local.Quarter.Text1;
    useImport.Employer.Name = local.Employer.Name;

    Call(SiB274RevisedWageReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB274SendAlertNewIncSrce()
  {
    var useImport = new SiB274SendAlertNewIncSrce.Import();
    var useExport = new SiB274SendAlertNewIncSrce.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = local.Process.Date;
    useImport.Employment.Identifier = local.Employment.Identifier;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SiB274SendAlertNewIncSrce.Execute, useImport, useExport);
  }

  private void UseSiB274UpdateEmploymntIncSrc1()
  {
    var useImport = new SiB274UpdateEmploymntIncSrc.Import();
    var useExport = new SiB274UpdateEmploymntIncSrc.Export();

    useImport.Employment.Identifier = entities.Employment.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Year.Year = local.Year.Year;
    useImport.Wage.AverageCurrency = local.Wage.AverageCurrency;
    useImport.Quarter.Text1 = local.Quarter.Text1;
    useImport.NumberOfIncSrcUpdates.Count = local.NumberIncSrcUpdates.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = local.Process.Date;
    useImport.Employer.Name = local.Employer.Name;

    Call(SiB274UpdateEmploymntIncSrc.Execute, useImport, useExport);

    local.TenPercentIncrease.Flag = useExport.TenPercentIncrease.Flag;
    local.NumberIncSrcUpdates.Count = useExport.NumberOfIncSrcUpdates.Count;
    local.RecordsAlreadyProcessed.Count =
      useExport.RecordsAlreadyProcessed.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB274UpdateEmploymntIncSrc2()
  {
    var useImport = new SiB274UpdateEmploymntIncSrc.Import();
    var useExport = new SiB274UpdateEmploymntIncSrc.Export();

    useImport.Employment.Identifier = entities.SecondRead.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Year.Year = local.Year.Year;
    useImport.Wage.AverageCurrency = local.Wage.AverageCurrency;
    useImport.Quarter.Text1 = local.Quarter.Text1;
    useImport.NumberOfIncSrcUpdates.Count = local.NumberIncSrcUpdates.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = local.Process.Date;
    useImport.Employer.Name = local.Employer.Name;

    Call(SiB274UpdateEmploymntIncSrc.Execute, useImport, useExport);

    local.TenPercentIncrease.Flag = useExport.TenPercentIncrease.Flag;
    local.NumberIncSrcUpdates.Count = useExport.NumberOfIncSrcUpdates.Count;
    local.RecordsAlreadyProcessed.Count =
      useExport.RecordsAlreadyProcessed.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB274UpdateMilitaryIncSrce()
  {
    var useImport = new SiB274UpdateMilitaryIncSrce.Import();
    var useExport = new SiB274UpdateMilitaryIncSrce.Export();

    useImport.Military.Identifier = entities.Military.Identifier;
    useImport.NumberOfIncSrcUpdates.Count = local.NumberIncSrcUpdates.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.Wage.AverageCurrency = local.Wage.AverageCurrency;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Employer.Name = local.Employer.Name;
    useImport.Quarter.Text1 = local.Quarter.Text1;
    useImport.Year.Year = local.Year.Year;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.MilitaryStatus.Text1 = local.MilitaryStatus.Text1;
    useImport.Process.Date = local.Process.Date;

    Call(SiB274UpdateMilitaryIncSrce.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.NumberIncSrcUpdates.Count = useExport.NumberOfIncSrcUpdates.Count;
    local.RecordsAlreadyProcessed.Count =
      useExport.RecordsAlreadyProcessed.Count;
    local.TenPercentIncrease.Flag = useExport.TenPercentIncrease.Flag;
  }

  private void UseSiCabHireDateAlertIwo2()
  {
    var useImport = new SiCabHireDateAlertIwo2.Import();
    var useExport = new SiCabHireDateAlertIwo2.Export();

    useImport.Employment.Identifier = entities.Employment.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.AutomaticGenerateIwo.Flag = local.AutomaticGenerateIwo1.Flag;
    useImport.AddressSuitableForIwo.Flag = local.AddressSuitableForIwo.Flag;
    useImport.NumberIncSrcUpdates.Count = local.NumberIncSrcUpdates.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = local.Process.Date;
    MoveEmployer(local.Employer, useImport.Employer);
    MoveIncomeSource3(local.Employment, useImport.NewInfo);

    Call(SiCabHireDateAlertIwo2.Execute, useImport, useExport);

    local.NumberIncSrcUpdates.Count = useExport.NumberIncSrcUpdates.Count;
    local.RecordsAlreadyProcessed.Count =
      useExport.RecordsAlreadyProcessed.Count;
  }

  private void UseSiCabReadAdabasBatch()
  {
    var useImport = new SiCabReadAdabasBatch.Import();
    var useExport = new SiCabReadAdabasBatch.Export();

    useImport.Obligor.Number = local.Kaecses.Number;

    Call(SiCabReadAdabasBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.Obligor, local.Kaecses);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.AbendData.Assign(useExport.AbendData);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.FederalCaseRegistry.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadEmployment1()
  {
    entities.Employment.Populated = false;

    return ReadEach("ReadEmployment1",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetNullableString(command, "ein", local.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.Employment.Identifier = db.GetDateTime(reader, 0);
        entities.Employment.Type1 = db.GetString(reader, 1);
        entities.Employment.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.Employment.LastQtr = db.GetNullableString(reader, 3);
        entities.Employment.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.Employment.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.Employment.Attribute2NdQtr = db.GetNullableString(reader, 6);
        entities.Employment.Attribute2NdQtrYr = db.GetNullableInt32(reader, 7);
        entities.Employment.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 8);
        entities.Employment.Attribute3RdQtr = db.GetNullableString(reader, 9);
        entities.Employment.Attribute3RdQtrYr = db.GetNullableInt32(reader, 10);
        entities.Employment.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 11);
        entities.Employment.Attribute4ThQtr = db.GetNullableString(reader, 12);
        entities.Employment.Attribute4ThQtrYr = db.GetNullableInt32(reader, 13);
        entities.Employment.SentDt = db.GetNullableDate(reader, 14);
        entities.Employment.ReturnDt = db.GetNullableDate(reader, 15);
        entities.Employment.ReturnCd = db.GetNullableString(reader, 16);
        entities.Employment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 17);
        entities.Employment.LastUpdatedBy = db.GetNullableString(reader, 18);
        entities.Employment.CspINumber = db.GetString(reader, 19);
        entities.Employment.EmpId = db.GetNullableInt32(reader, 20);
        entities.Employment.WorkerId = db.GetNullableString(reader, 21);
        entities.Employment.StartDt = db.GetNullableDate(reader, 22);
        entities.Employment.EndDt = db.GetNullableDate(reader, 23);
        entities.Employment.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.Employment.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadEmployment2()
  {
    entities.SecondRead.Populated = false;

    return ReadEach("ReadEmployment2",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetNullableString(command, "ein", local.Employer.Ein ?? "");
        db.SetNullableDate(command, "endDt", new DateTime(1, 1, 1));
      },
      (db, reader) =>
      {
        entities.SecondRead.Identifier = db.GetDateTime(reader, 0);
        entities.SecondRead.Type1 = db.GetString(reader, 1);
        entities.SecondRead.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.SecondRead.LastQtr = db.GetNullableString(reader, 3);
        entities.SecondRead.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.SecondRead.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.SecondRead.Attribute2NdQtr = db.GetNullableString(reader, 6);
        entities.SecondRead.Attribute2NdQtrYr = db.GetNullableInt32(reader, 7);
        entities.SecondRead.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 8);
        entities.SecondRead.Attribute3RdQtr = db.GetNullableString(reader, 9);
        entities.SecondRead.Attribute3RdQtrYr = db.GetNullableInt32(reader, 10);
        entities.SecondRead.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 11);
        entities.SecondRead.Attribute4ThQtr = db.GetNullableString(reader, 12);
        entities.SecondRead.Attribute4ThQtrYr = db.GetNullableInt32(reader, 13);
        entities.SecondRead.SentDt = db.GetNullableDate(reader, 14);
        entities.SecondRead.ReturnDt = db.GetNullableDate(reader, 15);
        entities.SecondRead.ReturnCd = db.GetNullableString(reader, 16);
        entities.SecondRead.Name = db.GetNullableString(reader, 17);
        entities.SecondRead.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.SecondRead.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.SecondRead.CreatedTimestamp = db.GetDateTime(reader, 20);
        entities.SecondRead.CreatedBy = db.GetString(reader, 21);
        entities.SecondRead.CspINumber = db.GetString(reader, 22);
        entities.SecondRead.SelfEmployedInd = db.GetNullableString(reader, 23);
        entities.SecondRead.EmpId = db.GetNullableInt32(reader, 24);
        entities.SecondRead.SendTo = db.GetNullableString(reader, 25);
        entities.SecondRead.WorkerId = db.GetNullableString(reader, 26);
        entities.SecondRead.StartDt = db.GetNullableDate(reader, 27);
        entities.SecondRead.EndDt = db.GetNullableDate(reader, 28);
        entities.SecondRead.Note = db.GetNullableString(reader, 29);
        entities.SecondRead.Note2 = db.GetNullableString(reader, 30);
        entities.SecondRead.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.SecondRead.Type1);
        CheckValid<IncomeSource>("SendTo", entities.SecondRead.SendTo);

        return true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "ssn", local.ConvertSsn.SsnNum9);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.Employment.Populated);
    entities.LegalActionIncomeSource.Populated = false;

    return ReadEach("ReadLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Employment.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.Employment.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.CreatedBy = db.GetString(reader, 4);
        entities.LegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 5);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 6);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 7);
        entities.LegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 8);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 9);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 10);
        entities.LegalActionIncomeSource.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMilitary()
  {
    entities.Military.Populated = false;

    return ReadEach("ReadMilitary",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetNullableString(command, "ein", local.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.Military.Identifier = db.GetDateTime(reader, 0);
        entities.Military.Type1 = db.GetString(reader, 1);
        entities.Military.ReturnDt = db.GetNullableDate(reader, 2);
        entities.Military.ReturnCd = db.GetNullableString(reader, 3);
        entities.Military.CspINumber = db.GetString(reader, 4);
        entities.Military.EmpId = db.GetNullableInt32(reader, 5);
        entities.Military.StartDt = db.GetNullableDate(reader, 6);
        entities.Military.EndDt = db.GetNullableDate(reader, 7);
        entities.Military.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.Military.Type1);

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
    /// <summary>A NamesGroup group.</summary>
    [Serializable]
    public class NamesGroup
    {
      /// <summary>
      /// A value of Gname.
      /// </summary>
      [JsonPropertyName("gname")]
      public CsePersonsWorkSet Gname
      {
        get => gname ??= new();
        set => gname = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet gname;
    }

    /// <summary>A AlternateSsnGroup group.</summary>
    [Serializable]
    public class AlternateSsnGroup
    {
      /// <summary>
      /// A value of Gssn.
      /// </summary>
      [JsonPropertyName("gssn")]
      public CsePersonsWorkSet Gssn
      {
        get => gssn ??= new();
        set => gssn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet gssn;
    }

    /// <summary>
    /// A value of CurrentEmployment.
    /// </summary>
    [JsonPropertyName("currentEmployment")]
    public Common CurrentEmployment
    {
      get => currentEmployment ??= new();
      set => currentEmployment = value;
    }

    /// <summary>
    /// A value of AutomaticGenerateIwo1.
    /// </summary>
    [JsonPropertyName("automaticGenerateIwo1")]
    public Common AutomaticGenerateIwo1
    {
      get => automaticGenerateIwo1 ??= new();
      set => automaticGenerateIwo1 = value;
    }

    /// <summary>
    /// A value of CompareIwoCreatedDate.
    /// </summary>
    [JsonPropertyName("compareIwoCreatedDate")]
    public DateWorkArea CompareIwoCreatedDate
    {
      get => compareIwoCreatedDate ??= new();
      set => compareIwoCreatedDate = value;
    }

    /// <summary>
    /// A value of CompareDateMonths.
    /// </summary>
    [JsonPropertyName("compareDateMonths")]
    public DateWorkArea CompareDateMonths
    {
      get => compareDateMonths ??= new();
      set => compareDateMonths = value;
    }

    /// <summary>
    /// A value of MonthsDiff.
    /// </summary>
    [JsonPropertyName("monthsDiff")]
    public Common MonthsDiff
    {
      get => monthsDiff ??= new();
      set => monthsDiff = value;
    }

    /// <summary>
    /// A value of LaisTmstp.
    /// </summary>
    [JsonPropertyName("laisTmstp")]
    public WorkArea LaisTmstp
    {
      get => laisTmstp ??= new();
      set => laisTmstp = value;
    }

    /// <summary>
    /// A value of IwoCreatedDate.
    /// </summary>
    [JsonPropertyName("iwoCreatedDate")]
    public DateWorkArea IwoCreatedDate
    {
      get => iwoCreatedDate ??= new();
      set => iwoCreatedDate = value;
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
    /// A value of CompareDate.
    /// </summary>
    [JsonPropertyName("compareDate")]
    public DateWorkArea CompareDate
    {
      get => compareDate ??= new();
      set => compareDate = value;
    }

    /// <summary>
    /// A value of AmountConvert.
    /// </summary>
    [JsonPropertyName("amountConvert")]
    public Common AmountConvert
    {
      get => amountConvert ??= new();
      set => amountConvert = value;
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
    /// A value of AddressSuitableForIwo.
    /// </summary>
    [JsonPropertyName("addressSuitableForIwo")]
    public Common AddressSuitableForIwo
    {
      get => addressSuitableForIwo ??= new();
      set => addressSuitableForIwo = value;
    }

    /// <summary>
    /// A value of AutomaticGenerateIwo.
    /// </summary>
    [JsonPropertyName("automaticGenerateIwo")]
    public Common AutomaticGenerateIwo
    {
      get => automaticGenerateIwo ??= new();
      set => automaticGenerateIwo = value;
    }

    /// <summary>
    /// A value of TenPercentIncrease.
    /// </summary>
    [JsonPropertyName("tenPercentIncrease")]
    public Common TenPercentIncrease
    {
      get => tenPercentIncrease ??= new();
      set => tenPercentIncrease = value;
    }

    /// <summary>
    /// A value of PreviouslyCompared.
    /// </summary>
    [JsonPropertyName("previouslyCompared")]
    public Employer PreviouslyCompared
    {
      get => previouslyCompared ??= new();
      set => previouslyCompared = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public DateWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of MilitaryStatus.
    /// </summary>
    [JsonPropertyName("militaryStatus")]
    public TextWorkArea MilitaryStatus
    {
      get => militaryStatus ??= new();
      set => militaryStatus = value;
    }

    /// <summary>
    /// A value of Wage.
    /// </summary>
    [JsonPropertyName("wage")]
    public Common Wage
    {
      get => wage ??= new();
      set => wage = value;
    }

    /// <summary>
    /// A value of Quarter.
    /// </summary>
    [JsonPropertyName("quarter")]
    public TextWorkArea Quarter
    {
      get => quarter ??= new();
      set => quarter = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// Gets a value of Names.
    /// </summary>
    [JsonIgnore]
    public Array<NamesGroup> Names => names ??= new(NamesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Names for json serialization.
    /// </summary>
    [JsonPropertyName("names")]
    [Computed]
    public IList<NamesGroup> Names_Json
    {
      get => names;
      set => Names.Assign(value);
    }

    /// <summary>
    /// Gets a value of AlternateSsn.
    /// </summary>
    [JsonIgnore]
    public Array<AlternateSsnGroup> AlternateSsn => alternateSsn ??= new(
      AlternateSsnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AlternateSsn for json serialization.
    /// </summary>
    [JsonPropertyName("alternateSsn")]
    [Computed]
    public IList<AlternateSsnGroup> AlternateSsn_Json
    {
      get => alternateSsn;
      set => AlternateSsn.Assign(value);
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of EmployerInfoMismatch.
    /// </summary>
    [JsonPropertyName("employerInfoMismatch")]
    public Common EmployerInfoMismatch
    {
      get => employerInfoMismatch ??= new();
      set => employerInfoMismatch = value;
    }

    /// <summary>
    /// A value of EmployersCreated.
    /// </summary>
    [JsonPropertyName("employersCreated")]
    public Common EmployersCreated
    {
      get => employersCreated ??= new();
      set => employersCreated = value;
    }

    /// <summary>
    /// A value of IncomeSourcesCreated.
    /// </summary>
    [JsonPropertyName("incomeSourcesCreated")]
    public Common IncomeSourcesCreated
    {
      get => incomeSourcesCreated ??= new();
      set => incomeSourcesCreated = value;
    }

    /// <summary>
    /// A value of Valid.
    /// </summary>
    [JsonPropertyName("valid")]
    public Common Valid
    {
      get => valid ??= new();
      set => valid = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of RecordsSkippedNuhireRpt.
    /// </summary>
    [JsonPropertyName("recordsSkippedNuhireRpt")]
    public Common RecordsSkippedNuhireRpt
    {
      get => recordsSkippedNuhireRpt ??= new();
      set => recordsSkippedNuhireRpt = value;
    }

    /// <summary>
    /// A value of RecordsSkippedUnknown.
    /// </summary>
    [JsonPropertyName("recordsSkippedUnknown")]
    public Common RecordsSkippedUnknown
    {
      get => recordsSkippedUnknown ??= new();
      set => recordsSkippedUnknown = value;
    }

    /// <summary>
    /// A value of NumberIncSrcUpdates.
    /// </summary>
    [JsonPropertyName("numberIncSrcUpdates")]
    public Common NumberIncSrcUpdates
    {
      get => numberIncSrcUpdates ??= new();
      set => numberIncSrcUpdates = value;
    }

    /// <summary>
    /// A value of EmployeeNameMismatch.
    /// </summary>
    [JsonPropertyName("employeeNameMismatch")]
    public Common EmployeeNameMismatch
    {
      get => employeeNameMismatch ??= new();
      set => employeeNameMismatch = value;
    }

    /// <summary>
    /// A value of RecordsSkippedSsn.
    /// </summary>
    [JsonPropertyName("recordsSkippedSsn")]
    public Common RecordsSkippedSsn
    {
      get => recordsSkippedSsn ??= new();
      set => recordsSkippedSsn = value;
    }

    /// <summary>
    /// A value of RecordsSkippedNotType1.
    /// </summary>
    [JsonPropertyName("recordsSkippedNotType1")]
    public Common RecordsSkippedNotType1
    {
      get => recordsSkippedNotType1 ??= new();
      set => recordsSkippedNotType1 = value;
    }

    /// <summary>
    /// A value of RecordsPersonNotFound.
    /// </summary>
    [JsonPropertyName("recordsPersonNotFound")]
    public Common RecordsPersonNotFound
    {
      get => recordsPersonNotFound ??= new();
      set => recordsPersonNotFound = value;
    }

    /// <summary>
    /// A value of RecordsAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("recordsAlreadyProcessed")]
    public Common RecordsAlreadyProcessed
    {
      get => recordsAlreadyProcessed ??= new();
      set => recordsAlreadyProcessed = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public External ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of BeginQuarter.
    /// </summary>
    [JsonPropertyName("beginQuarter")]
    public DateWorkArea BeginQuarter
    {
      get => beginQuarter ??= new();
      set => beginQuarter = value;
    }

    /// <summary>
    /// A value of DateOfHireTextWorkArea.
    /// </summary>
    [JsonPropertyName("dateOfHireTextWorkArea")]
    public TextWorkArea DateOfHireTextWorkArea
    {
      get => dateOfHireTextWorkArea ??= new();
      set => dateOfHireTextWorkArea = value;
    }

    /// <summary>
    /// A value of StateHiredInCsePersonAddress.
    /// </summary>
    [JsonPropertyName("stateHiredInCsePersonAddress")]
    public CsePersonAddress StateHiredInCsePersonAddress
    {
      get => stateHiredInCsePersonAddress ??= new();
      set => stateHiredInCsePersonAddress = value;
    }

    /// <summary>
    /// A value of StateHiredInTextWorkArea.
    /// </summary>
    [JsonPropertyName("stateHiredInTextWorkArea")]
    public TextWorkArea StateHiredInTextWorkArea
    {
      get => stateHiredInTextWorkArea ??= new();
      set => stateHiredInTextWorkArea = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of Kaecses.
    /// </summary>
    [JsonPropertyName("kaecses")]
    public CsePersonsWorkSet Kaecses
    {
      get => kaecses ??= new();
      set => kaecses = value;
    }

    /// <summary>
    /// A value of FederalCaseRegistry.
    /// </summary>
    [JsonPropertyName("federalCaseRegistry")]
    public CsePersonsWorkSet FederalCaseRegistry
    {
      get => federalCaseRegistry ??= new();
      set => federalCaseRegistry = value;
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
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
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
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public IncomeSource Clear
    {
      get => clear ??= new();
      set => clear = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    /// <summary>
    /// A value of ConvertSsn.
    /// </summary>
    [JsonPropertyName("convertSsn")]
    public SsnWorkArea ConvertSsn
    {
      get => convertSsn ??= new();
      set => convertSsn = value;
    }

    /// <summary>
    /// A value of ConvertDateCommon.
    /// </summary>
    [JsonPropertyName("convertDateCommon")]
    public Common ConvertDateCommon
    {
      get => convertDateCommon ??= new();
      set => convertDateCommon = value;
    }

    /// <summary>
    /// A value of DateOfHireDateWorkArea.
    /// </summary>
    [JsonPropertyName("dateOfHireDateWorkArea")]
    public DateWorkArea DateOfHireDateWorkArea
    {
      get => dateOfHireDateWorkArea ??= new();
      set => dateOfHireDateWorkArea = value;
    }

    /// <summary>
    /// A value of ConvertDateWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateWorkArea")]
    public WorkArea ConvertDateWorkArea
    {
      get => convertDateWorkArea ??= new();
      set => convertDateWorkArea = value;
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
    /// A value of ConvertMessage.
    /// </summary>
    [JsonPropertyName("convertMessage")]
    public SsnWorkArea ConvertMessage
    {
      get => convertMessage ??= new();
      set => convertMessage = value;
    }

    /// <summary>
    /// A value of Message2.
    /// </summary>
    [JsonPropertyName("message2")]
    public WorkArea Message2
    {
      get => message2 ??= new();
      set => message2 = value;
    }

    /// <summary>
    /// A value of Message1.
    /// </summary>
    [JsonPropertyName("message1")]
    public WorkArea Message1
    {
      get => message1 ??= new();
      set => message1 = value;
    }

    private Common currentEmployment;
    private Common automaticGenerateIwo1;
    private DateWorkArea compareIwoCreatedDate;
    private DateWorkArea compareDateMonths;
    private Common monthsDiff;
    private WorkArea laisTmstp;
    private DateWorkArea iwoCreatedDate;
    private Common numberOfDays;
    private DateWorkArea compareDate;
    private Common amountConvert;
    private WorkArea amount;
    private Common addressSuitableForIwo;
    private Common automaticGenerateIwo;
    private Common tenPercentIncrease;
    private Employer previouslyCompared;
    private DateWorkArea year;
    private TextWorkArea militaryStatus;
    private Common wage;
    private TextWorkArea quarter;
    private Common batch;
    private Array<NamesGroup> names;
    private Array<AlternateSsnGroup> alternateSsn;
    private TextWorkArea time;
    private TextWorkArea date;
    private Common employerInfoMismatch;
    private Common employersCreated;
    private Common incomeSourcesCreated;
    private Common valid;
    private Common recordsRead;
    private Common recordsSkippedNuhireRpt;
    private Common recordsSkippedUnknown;
    private Common numberIncSrcUpdates;
    private Common employeeNameMismatch;
    private Common recordsSkippedSsn;
    private Common recordsSkippedNotType1;
    private Common recordsPersonNotFound;
    private Common recordsAlreadyProcessed;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private DateWorkArea max;
    private DateWorkArea null1;
    private TextWorkArea recordType;
    private DateWorkArea beginQuarter;
    private TextWorkArea dateOfHireTextWorkArea;
    private CsePersonAddress stateHiredInCsePersonAddress;
    private TextWorkArea stateHiredInTextWorkArea;
    private Employer employer;
    private EmployerAddress employerAddress;
    private EabReportSend neededToWrite;
    private CsePersonsWorkSet kaecses;
    private CsePersonsWorkSet federalCaseRegistry;
    private EabFileHandling eabFileHandling;
    private IncomeSource employment;
    private AbendData abendData;
    private IncomeSource clear;
    private Infrastructure infrastructure;
    private ProgramProcessingInfo programProcessingInfo;
    private NarrativeDetail narrativeDetail;
    private SsnWorkArea convertSsn;
    private Common convertDateCommon;
    private DateWorkArea dateOfHireDateWorkArea;
    private WorkArea convertDateWorkArea;
    private DateWorkArea convert;
    private SsnWorkArea convertMessage;
    private WorkArea message2;
    private WorkArea message1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
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
    /// A value of SecondRead.
    /// </summary>
    [JsonPropertyName("secondRead")]
    public IncomeSource SecondRead
    {
      get => secondRead ??= new();
      set => secondRead = value;
    }

    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public IncomeSource Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
    }

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson legalActionPerson;
    private LegalActionIncomeSource legalActionIncomeSource;
    private LegalAction legalAction;
    private IncomeSource secondRead;
    private IncomeSource military;
    private IncomeSource employment;
    private Employer employer;
    private CsePerson csePerson;
    private InvalidSsn invalidSsn;
  }
#endregion
}
