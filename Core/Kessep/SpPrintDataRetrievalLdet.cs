// Program: SP_PRINT_DATA_RETRIEVAL_LDET, ID: 372134810, model: 746.
// Short name: SWE02236
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_PRINT_DATA_RETRIEVAL_LDET.
/// </summary>
[Serializable]
public partial class SpPrintDataRetrievalLdet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DATA_RETRIEVAL_LDET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDataRetrievalLdet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDataRetrievalLdet.
  /// </summary>
  public SpPrintDataRetrievalLdet(IContext context, Import import, Export export)
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
    // -------------------------------------------------------------------------------------
    // Date        Developer	Request #	Description
    // ----------  ----------	---------	
    // ---------------------------------------------
    // 10/06/1998  M Ramirez			Initial Development
    // 07/14/1999  M Ramirez			Added row lock counts
    // 10/03/1999  M Ramirez	73527		Fixed fields when the LDET does not exist 
    // but
    // 					the preceding LDET does.
    // 10/25/1999  M Ramirez	74764		LA LDET J requires CS, added new field which
    // 					doesn't
    // 11/08/1999  M Ramirez			Fix Non-Financial LDETs
    // 10/24/2000  M Ramirez	105251		Zero amounts for WA and WC should be SPACES
    // 					instead of 0.00.  Also, LDETs should take CSE
    // 					Case Number into consideration, if available,
    // 					to avoid Multi-payor problems.
    // 03/08/2001  M Ramirez	WR 187 Seg F	WA and WC amounts should always be 
    // monthly
    // 03/08/2001  M Ramirez	WR 187 Seg F	Added LAWCWABW, LAWCWASM and LAWCWAW
    // 03/08/2001  M Ramirez	WR 187 Seg F	Added COCSAMT, COCSFRQ, COCSTERM, 
    // COARRBAL
    // 					and COARRBALDT
    // 05/14/2001  M Ramirez	120098		Arrears summed by Legal Action for each 
    // LDET,
    // 					instead of by LDET
    // 06/18/2001  M Ramirez	113159		LDETs could have future dates
    // 12/19/2001  K Cole	10508		Download maximum withholding percent
    // 05/23/2006  M.J. Quinn	272406		A 1061 abend is being caused by division 
    // by
    // 					zero. Check to ensure that local_textnum
    // 					total_currency is not zero.
    // 09/22/2008  J Huss	CQ 7056		Changed COCSX12WKS calculation.
    // 02/10/2009  J Huss	CQ 9182		Added field "LA LDET HI" to find J or O class
    // 					legal actions with HIC legal detail.
    // 01/20/2015  D Dupree	CQ45940		Added the set statement to fix the double
    // 					count issue when srrun195 has not been run
    // 					yet and the end of the month has past and a
    // 					2nd demand letter is being issued. This
    // 					change fixes the problem by account for the
    // 					time period correctly.
    // 02/17/2017  A Hockman	CQ47796		ticket is to allow modifications to zero 
    // for
    // 					cases where ap has iwo at full time employer
    // 					and iwo is sent via auto process to part time
    // 					employer.
    // 06/07/2018  GVandy	CQ62549		Don't require IWOTERM document to have a
    // 					legal detail.  This is required due to use of
    // 					new IWO_BODY include file that is common to
    // 					all the IWO documents.
    // -------------------------------------------------------------------------------------
    MoveFieldValue2(import.FieldValue, local.FieldValue);
    local.CurrentDateWorkArea.Date = import.Infrastructure.ReferenceDate;
    local.CurrentLegalAction.Identifier = import.SpDocKey.KeyLegalAction;
    local.LegalAction.Identifier = import.SpDocKey.KeyLegalAction;

    foreach(var item in ReadField())
    {
      if (!Lt(local.Previous.Name, entities.Field.Name))
      {
        continue;
      }

      local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
      local.TextnumWorkArea.Text15 = "";
      local.TextnumCommon.TotalCurrency = 0;
      local.ObligationType.Code = "";
      local.Previous.Name = entities.Field.Name;
      local.Temp.Name = Substring(entities.Field.Name, 1, 2);

      if (Equal(local.Temp.Name, "CO"))
      {
        local.Field.Name = entities.Field.Name;
        local.Temp.Name = Substring(local.Field.Name, 1, 5);

        if (Equal(local.Temp.Name, "COARR") || Equal
          (local.Field.Name, "COCSX12WKS"))
        {
          if (Equal(local.Field.Name, "COARRBALDT"))
          {
            goto Test1;
          }

          if (!IsEmpty(local.CoarrFieldsCalculated.Flag))
          {
            goto Test1;
          }

          local.CoarrFieldsCalculated.Flag = "Y";

          if (!ReadLegalAction1())
          {
            return;
          }

          if (!ReadTribunal())
          {
            return;
          }

          if (!IsEmpty(import.SpDocKey.KeyAp))
          {
            local.CsePerson.Number = import.SpDocKey.KeyAp;
          }
          else
          {
            foreach(var item1 in ReadCsePerson())
            {
              if (IsEmpty(local.CsePerson.Number))
              {
                local.CsePerson.Number = entities.CsePerson.Number;
              }
              else if (!Equal(local.CsePerson.Number, entities.CsePerson.Number))
                
              {
                local.CsePerson.Number = "";

                break;
              }
            }
          }

          if (IsEmpty(local.CsePerson.Number))
          {
            goto Test1;
          }

          if (!ReadCsePersonAccount())
          {
            local.CsePerson.Number = "";

            goto Test1;
          }

          local.LastMonthEnd.Date =
            AddMonths(local.CurrentDateWorkArea.Date, -1);
          local.LastMonthEnd.Year = Year(local.LastMonthEnd.Date);
          local.LastMonthEnd.Month = Month(local.LastMonthEnd.Date);
          local.LastMonthEnd.YearMonth = local.LastMonthEnd.Year * 100 + local
            .LastMonthEnd.Month;
          local.CoarrFieldsCalculated.TotalCurrency = 0;

          foreach(var item1 in ReadLegalAction3())
          {
            foreach(var item2 in ReadObligation())
            {
              if (ReadObligationType2())
              {
                if (AsChar(entities.ObligationType.Classification) != 'A' && AsChar
                  (entities.ObligationType.Classification) != 'M' && AsChar
                  (entities.ObligationType.Classification) != 'N')
                {
                  continue;
                }
              }
              else
              {
                goto Test1;
              }

              if (ReadMonthlyObligorSummary1())
              {
                local.CoarrFieldsCalculated.TotalCurrency += entities.
                  MonthlyObligorSummary.ForMthCurrBal.GetValueOrDefault();
              }
              else
              {
                if (ReadMonthlyObligorSummary2())
                {
                  local.CoarrFieldsCalculated.TotalCurrency += entities.
                    MonthlyObligorSummary.ForMthCurrBal.GetValueOrDefault();
                }

                // mjr
                // ------------------------------------------------------
                // Either there was no monthly obligor summary or the latest
                // one for that obligation was old.
                // Calculate the amount manually
                // ---------------------------------------------------------
                // mjr
                // ------------------------------------------------------------
                // set start timetamp
                // ---------------------------------------------------------------
                if (entities.MonthlyObligorSummary.YearMonth == 0)
                {
                  local.Start.Timestamp = local.NullDateWorkArea.Timestamp;
                }
                else
                {
                  local.TextYear.Text4 =
                    NumberToString(entities.MonthlyObligorSummary.YearMonth, 10,
                    4);
                  local.TextMonth.Text2 =
                    NumberToString(entities.MonthlyObligorSummary.YearMonth, 14,
                    2);
                  local.Start.Timestamp =
                    Timestamp(local.TextYear.Text4 + "-" + local
                    .TextMonth.Text2 + "-01-00.00.00.000000");

                  // cq45940 Added the set statement to fix the double count 
                  // issue when srrun195 has not
                  // been run yet and the end of the month has past and a 2nd 
                  // demand letter is being
                  // issued. This change fixes the problem by account for the 
                  // time period correctly.
                  local.Start.Timestamp = AddMonths(local.Start.Timestamp, 1);
                }

                // mjr
                // ------------------------------------------------------------
                // set start date
                // ---------------------------------------------------------------
                if (entities.MonthlyObligorSummary.YearMonth == 0)
                {
                  local.Start.Date = local.NullDateWorkArea.Date;
                }
                else
                {
                  local.Length.Count =
                    entities.MonthlyObligorSummary.YearMonth * 100 + 1;
                  local.Start.Date = IntToDate(local.Length.Count);

                  // cq45940 Added the set statement to fix the double count 
                  // issue when srrun195 has not
                  // been run yet and the end of the month has past and a 2nd 
                  // demand letter is being
                  // issued. This change fixes the problem by account for the 
                  // time period correctly.
                  local.Start.Date = AddMonths(local.Start.Date, 1);
                }

                // mjr
                // ------------------------------------------------------------
                // set end timetamp
                // ---------------------------------------------------------------
                switch(local.LastMonthEnd.Month)
                {
                  case 1:
                    local.LastMonthEnd.Day = 31;

                    break;
                  case 2:
                    // mjr
                    // ------------------------------------------------------
                    // If the current year is a leap year, December 31 will be
                    // the 366th day of the year
                    // ---------------------------------------------------------
                    local.DateWorkArea.Date =
                      IntToDate(local.LastMonthEnd.Year * 10000 + 1231);
                    local.Length.Count =
                      DateToJulianNumber(local.DateWorkArea.Date) - local
                      .LastMonthEnd.Year * 1000;

                    if (local.Length.Count == 365)
                    {
                      local.LastMonthEnd.Day = 28;
                    }
                    else
                    {
                      local.LastMonthEnd.Day = 29;
                    }

                    break;
                  case 3:
                    local.LastMonthEnd.Day = 31;

                    break;
                  case 4:
                    local.LastMonthEnd.Day = 30;

                    break;
                  case 5:
                    local.LastMonthEnd.Day = 31;

                    break;
                  case 6:
                    local.LastMonthEnd.Day = 30;

                    break;
                  case 7:
                    local.LastMonthEnd.Day = 31;

                    break;
                  case 8:
                    local.LastMonthEnd.Day = 31;

                    break;
                  case 9:
                    local.LastMonthEnd.Day = 30;

                    break;
                  case 10:
                    local.LastMonthEnd.Day = 31;

                    break;
                  case 11:
                    local.LastMonthEnd.Day = 30;

                    break;
                  case 12:
                    local.LastMonthEnd.Day = 31;

                    break;
                  default:
                    break;
                }

                local.TextYear.Text4 =
                  NumberToString(local.LastMonthEnd.Year, 12, 4);
                local.TextMonth.Text2 =
                  NumberToString(local.LastMonthEnd.Month, 14, 2);
                local.TextDay.Text2 =
                  NumberToString(local.LastMonthEnd.Day, 14, 2);
                local.LastMonthEnd.Timestamp =
                  Timestamp(local.TextYear.Text4 + "-" + local
                  .TextMonth.Text2 + "-" + local.TextDay.Text2 + "-23.59.59.999999"
                  );

                // mjr
                // ------------------------------------------------------------
                // set end date
                // ---------------------------------------------------------------
                local.Length.Count = local.LastMonthEnd.Year * 10000 + local
                  .LastMonthEnd.Month * 100 + local.LastMonthEnd.Day;
                local.LastMonthEnd.Date = IntToDate(local.Length.Count);

                // mjr
                // ------------------------------------------------------
                // Record new debts, and any debts that accrued
                // during the period
                // ---------------------------------------------------------
                foreach(var item3 in ReadObligationTransactionDebtDetail())
                {
                  if (Lt(local.LastMonthEnd.Date, entities.DebtDetail.DueDt))
                  {
                    continue;
                  }

                  local.CoarrFieldsCalculated.TotalCurrency += entities.
                    ObligationTransaction.Amount;
                }

                // mjr
                // ------------------------------------------------------
                // Record any payments
                // ---------------------------------------------------------
                foreach(var item3 in ReadCollection2())
                {
                  local.CoarrFieldsCalculated.TotalCurrency -= entities.
                    Collection.Amount;
                }

                // mjr
                // ------------------------------------------------------
                // Record any collection adjustments
                // ---------------------------------------------------------
                foreach(var item3 in ReadCollection1())
                {
                  local.CoarrFieldsCalculated.TotalCurrency += entities.
                    Collection.Amount;
                }

                // mjr
                // ------------------------------------------------------
                // Record any debt adjustments
                // ---------------------------------------------------------
                foreach(var item3 in ReadObligationTransaction())
                {
                  if (AsChar(entities.ObligationTransaction.DebtAdjustmentType) ==
                    'I')
                  {
                    local.CoarrFieldsCalculated.TotalCurrency += entities.
                      ObligationTransaction.Amount;
                  }
                  else
                  {
                    local.CoarrFieldsCalculated.TotalCurrency -= entities.
                      ObligationTransaction.Amount;
                  }
                }
              }
            }
          }
        }

Test1:

        local.Temp.Name = Substring(local.Field.Name, 1, 4);

        if (Equal(local.Temp.Name, "COCS"))
        {
          if (!IsEmpty(local.CocsFieldsCalculated.Flag))
          {
            goto Test2;
          }

          local.CocsFieldsCalculated.Flag = "Y";

          if (!ReadLegalAction1())
          {
            return;
          }

          if (!ReadTribunal())
          {
            return;
          }

          local.LegalActionDetail.Number = 99;

          foreach(var item1 in ReadLegalActionLegalActionDetailObligationType2())
            
          {
            switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
            {
              case "BW":
                local.LegalActionDetail.CurrentAmount =
                  entities.LegalActionDetail.CurrentAmount.GetValueOrDefault() *
                  26;

                if (local.LegalActionDetail.Number > 26)
                {
                  local.LegalActionDetail.Number = 26;
                }

                if (Lt(local.LegalActionDetail.DayOfWeek.GetValueOrDefault(),
                  entities.LegalActionDetail.DayOfWeek))
                {
                  local.LegalActionDetail.DayOfWeek =
                    entities.LegalActionDetail.DayOfWeek;
                }

                break;
              case "SM":
                local.LegalActionDetail.CurrentAmount =
                  entities.LegalActionDetail.CurrentAmount.GetValueOrDefault() *
                  24;

                if (local.LegalActionDetail.Number > 24)
                {
                  local.LegalActionDetail.Number = 24;
                }

                if (Lt(local.LegalActionDetail.DayOfMonth2.GetValueOrDefault(),
                  entities.LegalActionDetail.DayOfMonth2))
                {
                  local.LegalActionDetail.DayOfMonth2 =
                    entities.LegalActionDetail.DayOfMonth2;
                }

                break;
              case "W":
                local.LegalActionDetail.CurrentAmount =
                  entities.LegalActionDetail.CurrentAmount.GetValueOrDefault() *
                  52;

                if (local.LegalActionDetail.Number > 52)
                {
                  local.LegalActionDetail.Number = 52;
                }

                if (Lt(local.LegalActionDetail.DayOfWeek.GetValueOrDefault(),
                  entities.LegalActionDetail.DayOfWeek))
                {
                  local.LegalActionDetail.DayOfWeek =
                    entities.LegalActionDetail.DayOfWeek;
                }

                break;
              default:
                local.LegalActionDetail.CurrentAmount =
                  entities.LegalActionDetail.CurrentAmount.GetValueOrDefault() *
                  12;

                if (local.LegalActionDetail.Number > 12)
                {
                  local.LegalActionDetail.Number = 12;
                }

                if (Lt(local.LegalActionDetail.DayOfMonth1.GetValueOrDefault(),
                  entities.LegalActionDetail.DayOfMonth1))
                {
                  local.LegalActionDetail.DayOfMonth1 =
                    entities.LegalActionDetail.DayOfMonth1;
                }

                break;
            }

            local.CocsFieldsCalculated.TotalCurrency += local.LegalActionDetail.
              CurrentAmount.GetValueOrDefault();
          }
        }
      }
      else if (Equal(local.Temp.Name, "LA"))
      {
        // mjr
        // ----------------------------------------------------------------------
        // Determine the obligation type this field is supposed to retrieve.
        // Also, determine the generic name for this field.
        // The obligation type is the 2-4 characters between the "LA"
        // and the last 3-4 characters of the field.
        // The generic name is the original field name, with the
        // obligation type replaced by a '*', (ie LA*AMT).
        // LACRCHAMT   LACSAMT   LAFEEAMT   LAMJAMT   LAWAAMT   LAWCAMT   
        // LA718BAMT
        // LACRCHDOM1  LACSDOM1  LAFEEDOM1  LAMJDOM1  LAWADOM1  LAWCDOM1  
        // LA718BDOM1
        // LACRCHDOM2  LACSDOM2  LAFEEDOM2  LAMJDOM2  LAWADOM2  LAWCDOM2  
        // LA718BDOM2
        // LACRCHDOW   LACSDOW   LAFEEDOW   LAMJDOW   LAWADOW   LAWCDOW   
        // LA718BDOW
        // LACRCHEFDT  LACSEFDT  LAFEEEFDT  LAMJEFDT  LAWAEFDT  LAWCEFDT  
        // LA718BEFDT
        // LACRCHFREQ  LACSFREQ  LAFEEFREQ  LAMJFREQ  LAWAFREQ  LAWCFREQ  
        // LA718BFREQ
        //                                                      
        // LAWLAMT
        // -------------------------------------------------------------------------
        local.Length.Count = Length(TrimEnd(entities.Field.Name));
        local.Temp.Name =
          Substring(entities.Field.Name, local.Length.Count - 3, 4);

        if (Equal(local.Temp.Name, "DOM1") || Equal
          (local.Temp.Name, "DOM2") || Equal(local.Temp.Name, "EFDT") || Equal
          (local.Temp.Name, "FREQ"))
        {
          local.ObligationType.Code =
            Substring(entities.Field.Name, 3, local.Length.Count - 6);
        }
        else
        {
          local.Temp.Name =
            Substring(entities.Field.Name, local.Length.Count - 2, 3);

          if (Equal(local.Temp.Name, "AMT") || Equal(local.Temp.Name, "DOW"))
          {
            local.ObligationType.Code =
              Substring(entities.Field.Name, 3, local.Length.Count - 5);
          }
          else
          {
            local.Field.Name = entities.Field.Name;

            goto Test2;
          }
        }

        local.Field.Name = "LA*" + local.Temp.Name;

        if (!Equal(local.ObligationType.Code, local.CurrentObligationType.Code) &&
          !IsEmpty(local.ObligationType.Code))
        {
          // mjr
          // ----------------------------------------------------------
          // There is normally only one LDET for an obligation_type.  If
          // we find more than one, the user wants to know.  Also, don't
          // print out any values for that LDET.
          // If no LDETs are found for this document, don't allow the user
          // to print the document.
          // No effective date qualifier (print future dates)
          // -------------------------------------------------------------
          if (IsEmpty(local.FoundObligation.Flag))
          {
            local.FoundObligation.Flag = "N";
          }

          // mjr
          // ----------------------------------------------------
          // 05/05/1999
          // For WC and WA obligation types, always use the
          // current legal action.
          // For other obligation types, the user may decide to use
          // the last j class legal action.
          // ----------------------------------------------------------------
          if (Equal(local.ObligationType.Code, "WA") || Equal
            (local.ObligationType.Code, "WC") || Equal
            (local.ObligationType.Code, "WL"))
          {
            local.CurrentLegalAction.Identifier =
              import.SpDocKey.KeyLegalAction;
          }
          else
          {
            local.CurrentLegalAction.Identifier = local.LegalAction.Identifier;
          }

          if (ReadObligationType1())
          {
            local.CurrentObligationType.Code = entities.ObligationType.Code;

            // mjr
            // -----------------------------------------------------------
            // 11/08/1999
            // Financial LDET type
            // ------------------------------------------------------------------------
            local.MultipleObligations.Flag = "";
            local.LegalActionDetail.Assign(local.NullLegalActionDetail);

            if (!IsEmpty(import.SpDocKey.KeyAp))
            {
              // mjr
              // ----------------------------------------
              // 06/18/2001
              // PR# 113159 - Removed effective date check on
              // legal_action_person since it could be in the future
              // -----------------------------------------------------
              foreach(var item1 in ReadLegalActionDetail2())
              {
                local.FoundObligation.Flag = "Y";

                if (IsEmpty(local.MultipleObligations.Flag))
                {
                  local.MultipleObligations.Flag = "N";
                  local.LegalActionDetail.Assign(entities.LegalActionDetail);
                }
                else
                {
                  local.MultipleObligations.Flag = "Y";
                  local.LegalActionDetail.Assign(local.NullLegalActionDetail);
                  export.ErrorInd.Flag = "1";
                  local.Previous.Name = "LA" + local
                    .CurrentObligationType.Code + "Z";

                  goto ReadEach;
                }
              }
            }
            else
            {
              foreach(var item1 in ReadLegalActionDetail4())
              {
                local.FoundObligation.Flag = "Y";

                if (IsEmpty(local.MultipleObligations.Flag))
                {
                  local.MultipleObligations.Flag = "N";
                  local.LegalActionDetail.Assign(entities.LegalActionDetail);
                }
                else
                {
                  local.MultipleObligations.Flag = "Y";
                  local.LegalActionDetail.Assign(local.NullLegalActionDetail);
                  export.ErrorInd.Flag = "1";
                  local.Previous.Name = "LA" + local
                    .CurrentObligationType.Code + "Z";

                  goto ReadEach;
                }
              }
            }
          }
          else
          {
            local.CurrentObligationType.Code = local.ObligationType.Code;

            // mjr
            // -----------------------------------------------------------
            // 11/08/1999
            // Non-Financial LDET type
            // ------------------------------------------------------------------------
            local.MultipleObligations.Flag = "";
            local.LegalActionDetail.Assign(local.NullLegalActionDetail);

            if (!IsEmpty(import.SpDocKey.KeyAp))
            {
              // mjr
              // ----------------------------------------
              // 06/18/2001
              // PR# 113159 - Removed effective date check on
              // legal_action_person since it could be in the future
              // -----------------------------------------------------
              foreach(var item1 in ReadLegalActionDetail1())
              {
                local.FoundObligation.Flag = "Y";

                if (IsEmpty(local.MultipleObligations.Flag))
                {
                  local.MultipleObligations.Flag = "N";
                  local.LegalActionDetail.Assign(entities.LegalActionDetail);
                }
                else
                {
                  local.MultipleObligations.Flag = "Y";
                  local.LegalActionDetail.Assign(local.NullLegalActionDetail);
                  export.ErrorInd.Flag = "1";
                  local.Previous.Name = "LA" + local
                    .CurrentObligationType.Code + "Z";

                  goto ReadEach;
                }
              }
            }
            else
            {
              foreach(var item1 in ReadLegalActionDetail3())
              {
                local.FoundObligation.Flag = "Y";

                if (IsEmpty(local.MultipleObligations.Flag))
                {
                  local.MultipleObligations.Flag = "N";
                  local.LegalActionDetail.Assign(entities.LegalActionDetail);
                }
                else
                {
                  local.MultipleObligations.Flag = "Y";
                  local.LegalActionDetail.Assign(local.NullLegalActionDetail);
                  export.ErrorInd.Flag = "1";
                  local.Previous.Name = "LA" + local
                    .CurrentObligationType.Code + "Z";

                  goto ReadEach;
                }
              }
            }
          }
        }
      }
      else
      {
        local.Field.Name = entities.Field.Name;
      }

Test2:

      switch(TrimEnd(local.Field.Name))
      {
        case "COARRBAL":
          if (local.CoarrFieldsCalculated.TotalCurrency == 0)
          {
            if (!IsEmpty(local.CsePerson.Number))
            {
              local.FieldValue.Value = "0.00";
            }
          }
          else
          {
            local.TextnumWorkArea.Text15 =
              NumberToString((long)(local.CoarrFieldsCalculated.TotalCurrency *
              100), 15);
            local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
            local.FieldValue.Value =
              Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
              local.Verify.Count, 14 - local.Verify.Count) + "." + Substring
              (local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
          }

          break;
        case "COARRBALDT":
          local.LastMonthEnd.Date =
            AddMonths(local.CurrentDateWorkArea.Date, -1);
          local.LastMonthEnd.Year = Year(local.LastMonthEnd.Date);
          local.LastMonthEnd.Month = Month(local.LastMonthEnd.Date);

          switch(local.LastMonthEnd.Month)
          {
            case 1:
              local.LastMonthEnd.Day = 31;

              break;
            case 2:
              // mjr
              // ------------------------------------------------------
              // If the current year is a leap year, December 31 will be
              // the 366th day of the year
              // ---------------------------------------------------------
              local.DateWorkArea.Date = IntToDate(local.LastMonthEnd.Year * 10000
                + 1231);
              local.Length.Count =
                DateToJulianNumber(local.DateWorkArea.Date) - local
                .LastMonthEnd.Year * 1000;

              if (local.Length.Count == 365)
              {
                local.LastMonthEnd.Day = 28;
              }
              else
              {
                local.LastMonthEnd.Day = 29;
              }

              break;
            case 3:
              local.LastMonthEnd.Day = 31;

              break;
            case 4:
              local.LastMonthEnd.Day = 30;

              break;
            case 5:
              local.LastMonthEnd.Day = 31;

              break;
            case 6:
              local.LastMonthEnd.Day = 30;

              break;
            case 7:
              local.LastMonthEnd.Day = 31;

              break;
            case 8:
              local.LastMonthEnd.Day = 31;

              break;
            case 9:
              local.LastMonthEnd.Day = 30;

              break;
            case 10:
              local.LastMonthEnd.Day = 31;

              break;
            case 11:
              local.LastMonthEnd.Day = 30;

              break;
            case 12:
              local.LastMonthEnd.Day = 31;

              break;
            default:
              break;
          }

          local.Length.Count = local.LastMonthEnd.Year * 10000 + local
            .LastMonthEnd.Month * 100 + local.LastMonthEnd.Day;
          local.LastMonthEnd.Date = IntToDate(local.Length.Count);

          if (Lt(local.NullDateWorkArea.Date, local.LastMonthEnd.Date))
          {
            local.DateWorkArea.Date = local.LastMonthEnd.Date;
            local.FieldValue.Value = UseSpDocFormatDate();
          }

          break;
        case "COCSAMT":
          if (local.CocsFieldsCalculated.TotalCurrency == 0)
          {
            local.FieldValue.Value = "0.00";
          }
          else
          {
            switch(local.LegalActionDetail.Number)
            {
              case 12:
                break;
              case 24:
                break;
              case 26:
                break;
              case 52:
                break;
              default:
                local.LegalActionDetail.Number = 12;

                break;
            }

            local.CocsFieldsCalculated.TotalCurrency =
              Math.Round(
                local.CocsFieldsCalculated.TotalCurrency /
              local.LegalActionDetail.Number, 2, MidpointRounding.AwayFromZero);
              
            local.TextnumWorkArea.Text15 =
              NumberToString((long)(local.CocsFieldsCalculated.TotalCurrency * 100
              ), 15);
            local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
            local.FieldValue.Value =
              Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
              local.Verify.Count, 14 - local.Verify.Count) + "." + Substring
              (local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
          }

          break;
        case "COCSFRQ":
          switch(local.LegalActionDetail.Number)
          {
            case 12:
              local.LegalActionDetail.FreqPeriodCode = "M";

              break;
            case 24:
              local.LegalActionDetail.FreqPeriodCode = "SM";

              break;
            case 26:
              local.LegalActionDetail.FreqPeriodCode = "BW";

              break;
            case 52:
              local.LegalActionDetail.FreqPeriodCode = "W";

              break;
            default:
              local.LegalActionDetail.FreqPeriodCode = "";

              break;
          }

          if (!IsEmpty(local.LegalActionDetail.FreqPeriodCode))
          {
            local.ValidateCodeValue.Cdvalue =
              local.LegalActionDetail.FreqPeriodCode ?? Spaces(10);
            local.ValidateCode.CodeName = "LEGAL ACTION PAYMENT FREQNCY";
            UseCabGetCodeValueDescription();
            local.FieldValue.Value = local.ValidateCodeValue.Description;
          }

          break;
        case "COCSTERM":
          switch(local.LegalActionDetail.Number)
          {
            case 12:
              // mjr
              // ---------------------------------
              // Monthly
              // ------------------------------------
              local.TextnumWorkArea.Text15 =
                NumberToString(local.LegalActionDetail.DayOfMonth1.
                  GetValueOrDefault(), 15);
              local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
              local.FieldValue.Value =
                Substring(local.TextnumWorkArea.Text15,
                WorkArea.Text15_MaxLength, local.Verify.Count, 16 -
                local.Verify.Count);

              if (local.LegalActionDetail.DayOfMonth1.GetValueOrDefault() == 1
                || local.LegalActionDetail.DayOfMonth1.GetValueOrDefault() == 21
                || local.LegalActionDetail.DayOfMonth1.GetValueOrDefault() == 31
                )
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "st day of the month";
                  
              }
              else if (local.LegalActionDetail.DayOfMonth1.
                GetValueOrDefault() == 2 || local
                .LegalActionDetail.DayOfMonth1.GetValueOrDefault() == 22)
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "nd day of the month";
                  
              }
              else if (local.LegalActionDetail.DayOfMonth1.
                GetValueOrDefault() == 3 || local
                .LegalActionDetail.DayOfMonth1.GetValueOrDefault() == 23)
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "rd day of the month";
                  
              }
              else
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "th day of the month";
                  
              }

              break;
            case 24:
              // mjr
              // ---------------------------------
              // Semi-monthly
              // ------------------------------------
              if (local.LegalActionDetail.DayOfMonth1.GetValueOrDefault() > local
                .LegalActionDetail.DayOfMonth2.GetValueOrDefault())
              {
                local.LegalActionDetail.DayOfMonth2 =
                  local.LegalActionDetail.DayOfMonth1.GetValueOrDefault();
              }

              local.TextnumWorkArea.Text15 =
                NumberToString(local.LegalActionDetail.DayOfMonth2.
                  GetValueOrDefault(), 15);
              local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
              local.FieldValue.Value =
                Substring(local.TextnumWorkArea.Text15,
                WorkArea.Text15_MaxLength, local.Verify.Count, 16 -
                local.Verify.Count);

              if (local.LegalActionDetail.DayOfMonth2.GetValueOrDefault() == 1
                || local.LegalActionDetail.DayOfMonth2.GetValueOrDefault() == 21
                || local.LegalActionDetail.DayOfMonth2.GetValueOrDefault() == 31
                )
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "st day of the month";
                  
              }
              else if (local.LegalActionDetail.DayOfMonth2.
                GetValueOrDefault() == 2 || local
                .LegalActionDetail.DayOfMonth2.GetValueOrDefault() == 22)
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "nd day of the month";
                  
              }
              else if (local.LegalActionDetail.DayOfMonth2.
                GetValueOrDefault() == 3 || local
                .LegalActionDetail.DayOfMonth2.GetValueOrDefault() == 23)
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "rd day of the month";
                  
              }
              else
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "th day of the month";
                  
              }

              break;
            case 26:
              // mjr
              // ---------------------------------
              // Bi-weekly
              // ------------------------------------
              switch(local.LegalActionDetail.DayOfWeek.GetValueOrDefault())
              {
                case 1:
                  local.FieldValue.Value = "Sunday";

                  break;
                case 2:
                  local.FieldValue.Value = "Monday";

                  break;
                case 3:
                  local.FieldValue.Value = "Tuesday";

                  break;
                case 4:
                  local.FieldValue.Value = "Wednesday";

                  break;
                case 5:
                  local.FieldValue.Value = "Thursday";

                  break;
                case 7:
                  local.FieldValue.Value = "Saturday";

                  break;
                default:
                  local.FieldValue.Value = "Friday";

                  break;
              }

              local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + " of every other week";
                

              break;
            case 52:
              // mjr
              // ---------------------------------
              // Weekly
              // ------------------------------------
              switch(local.LegalActionDetail.DayOfWeek.GetValueOrDefault())
              {
                case 1:
                  local.FieldValue.Value = "Sunday";

                  break;
                case 2:
                  local.FieldValue.Value = "Monday";

                  break;
                case 3:
                  local.FieldValue.Value = "Tuesday";

                  break;
                case 4:
                  local.FieldValue.Value = "Wednesday";

                  break;
                case 5:
                  local.FieldValue.Value = "Thursday";

                  break;
                case 7:
                  local.FieldValue.Value = "Saturday";

                  break;
                default:
                  local.FieldValue.Value = "Friday";

                  break;
              }

              break;
            default:
              break;
          }

          break;
        case "COCSX12WKS":
          // 9/22/2008	J Huss	Changed calculation such that if an active "WA" 
          // Obligation Type exists, the value will be set to Y.
          if (ReadLegalActionLegalActionDetailObligationType1())
          {
            local.FieldValue.Value = "Y";
          }
          else
          {
            local.FieldValue.Value = "N";
          }

          break;
        case "LA LDET 1":
          // mjr----> Record the legal action identifier
          local.FieldValue.Value =
            NumberToString(local.CurrentLegalAction.Identifier, 7, 9);

          break;
        case "LA LDET 2":
          if (!ReadLegalAction2())
          {
            return;
          }

          if (!ReadTribunal())
          {
            return;
          }

          if (ReadLegalActionLegalActionDetail3())
          {
            local.LegalAction.Identifier = entities.LaLdetJ.Identifier;
          }

          // mjr----> Record the legal action identifier
          local.FieldValue.Value =
            NumberToString(local.LegalAction.Identifier, 7, 9);

          break;
        case "LA LDET HI":
          // 02/10/2009	J Huss		Added field "LA LDET HI" to find J or O class 
          // legal actions with HIC legal detail.
          if (!ReadLegalAction2())
          {
            return;
          }

          if (!ReadTribunal())
          {
            return;
          }

          if (ReadLegalActionLegalActionDetail2())
          {
            local.LegalAction.Identifier = entities.LaLdetJ.Identifier;
          }

          // mjr----> Record the legal action identifier
          local.FieldValue.Value =
            NumberToString(local.LegalAction.Identifier, 7, 9);

          break;
        case "LA LDET J":
          if (!ReadLegalAction2())
          {
            return;
          }

          if (!ReadTribunal())
          {
            return;
          }

          if (ReadLegalActionLegalActionDetail1())
          {
            local.LegalAction.Identifier = entities.LaLdetJ.Identifier;
          }

          // mjr----> Record the legal action identifier
          local.FieldValue.Value =
            NumberToString(local.LegalAction.Identifier, 7, 9);

          break;
        case "LA*AMT":
          switch(TrimEnd(local.CurrentObligationType.Code))
          {
            case "WL":
              if (local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() != 0
                )
              {
                local.TextnumCommon.TotalCurrency =
                  local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
              }

              break;
            case "CRCH":
              if (local.LegalActionDetail.JudgementAmount.GetValueOrDefault() !=
                0)
              {
                local.TextnumCommon.TotalCurrency =
                  local.LegalActionDetail.JudgementAmount.GetValueOrDefault();
              }

              break;
            case "CS":
              if (local.LegalActionDetail.CurrentAmount.GetValueOrDefault() != 0
                )
              {
                local.TextnumCommon.TotalCurrency =
                  local.LegalActionDetail.CurrentAmount.GetValueOrDefault();
              }

              break;
            case "FEE":
              if (local.LegalActionDetail.JudgementAmount.GetValueOrDefault() !=
                0)
              {
                local.TextnumCommon.TotalCurrency =
                  local.LegalActionDetail.JudgementAmount.GetValueOrDefault();
              }

              break;
            case "MJ":
              if (local.LegalActionDetail.JudgementAmount.GetValueOrDefault() !=
                0)
              {
                local.TextnumCommon.TotalCurrency =
                  local.LegalActionDetail.JudgementAmount.GetValueOrDefault();
              }

              break;
            case "WA":
              if (local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() != 0
                )
              {
                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(local.LegalActionDetail.FreqPeriodCode ?? ""))
                {
                  case "BW":
                    local.TextnumCommon.TotalCurrency =
                      Math.Round(
                        local.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 26
                      / 12, 2, MidpointRounding.AwayFromZero);

                    // mjr
                    // -----------------------------------------------
                    // Used for LAWCWASUM field
                    // --------------------------------------------------
                    local.LwcPlusWa.TotalCurrency =
                      local.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.TextnumCommon.TotalCurrency =
                      local.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 2;

                    // mjr
                    // -----------------------------------------------
                    // Used for LAWCWASUM field
                    // --------------------------------------------------
                    local.LwcPlusWa.TotalCurrency =
                      local.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.TextnumCommon.TotalCurrency =
                      Math.Round(
                        local.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 52
                      / 12, 2, MidpointRounding.AwayFromZero);

                    // mjr
                    // -----------------------------------------------
                    // Used for LAWCWASUM field
                    // --------------------------------------------------
                    local.LwcPlusWa.TotalCurrency =
                      local.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.TextnumCommon.TotalCurrency =
                      local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
                      

                    // mjr
                    // -----------------------------------------------
                    // Used for LAWCWASUM field
                    // --------------------------------------------------
                    local.LwcPlusWa.TotalCurrency =
                      local.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 12;

                    break;
                }
              }

              local.LwcPlusWa.Flag = "A";

              switch(local.LegalActionDetail.Limit.GetValueOrDefault())
              {
                case 50:
                  break;
                case 55:
                  break;
                case 60:
                  break;
                case 65:
                  break;
                default:
                  local.LegalActionDetail.Limit = 50;

                  break;
              }

              local.LwcPlusWa.Percentage =
                local.LegalActionDetail.Limit.GetValueOrDefault();

              break;
            case "WC":
              if (local.LegalActionDetail.CurrentAmount.GetValueOrDefault() != 0
                )
              {
                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(local.LegalActionDetail.FreqPeriodCode ?? ""))
                {
                  case "BW":
                    local.TextnumCommon.TotalCurrency =
                      Math.Round(
                        local.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 26
                      / 12, 2, MidpointRounding.AwayFromZero);

                    // mjr
                    // -----------------------------------------------
                    // Used for LAWCWASUM field
                    // --------------------------------------------------
                    local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                      CurrentAmount.GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.TextnumCommon.TotalCurrency =
                      local.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 2;

                    // mjr
                    // -----------------------------------------------
                    // Used for LAWCWASUM field
                    // --------------------------------------------------
                    local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                      CurrentAmount.GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.TextnumCommon.TotalCurrency =
                      Math.Round(
                        local.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 52
                      / 12, 2, MidpointRounding.AwayFromZero);

                    // mjr
                    // -----------------------------------------------
                    // Used for LAWCWASUM field
                    // --------------------------------------------------
                    local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                      CurrentAmount.GetValueOrDefault() * 52;

                    break;
                  default:
                    local.TextnumCommon.TotalCurrency =
                      local.LegalActionDetail.CurrentAmount.GetValueOrDefault();
                      

                    // mjr
                    // -----------------------------------------------
                    // Used for LAWCWASUM field
                    // --------------------------------------------------
                    local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                      CurrentAmount.GetValueOrDefault() * 12;

                    break;
                }
              }

              if (!IsEmpty(local.LwcPlusWa.Flag))
              {
                local.LwcPlusWa.Flag = "B";
              }
              else
              {
                local.LwcPlusWa.Flag = "C";
              }

              switch(local.LegalActionDetail.Limit.GetValueOrDefault())
              {
                case 50:
                  break;
                case 55:
                  break;
                case 60:
                  break;
                case 65:
                  break;
                default:
                  local.LegalActionDetail.Limit = 50;

                  break;
              }

              if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                .LwcPlusWa.Percentage)
              {
                local.LwcPlusWa.Percentage =
                  local.LegalActionDetail.Limit.GetValueOrDefault();
              }

              break;
            case "718B":
              if (local.LegalActionDetail.JudgementAmount.GetValueOrDefault() !=
                0)
              {
                local.TextnumCommon.TotalCurrency =
                  local.LegalActionDetail.JudgementAmount.GetValueOrDefault();
              }

              break;
            default:
              break;
          }

          if (local.TextnumCommon.TotalCurrency == 0)
          {
            // cq47796  commented out this code since we dont want anything set 
            // to spaces rather than zero
            //  based on the changes in this ticket to allow mod to zero, we 
            // need zeroes to show in the fields.
            local.FieldValue.Value = "0.00";
          }
          else
          {
            local.TextnumWorkArea.Text15 =
              NumberToString((long)(local.TextnumCommon.TotalCurrency * 100), 15);
              
            local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
            local.FieldValue.Value =
              Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
              local.Verify.Count, 14 - local.Verify.Count) + "." + Substring
              (local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
          }

          break;
        case "LA*DOM1":
          if (local.LegalActionDetail.DayOfMonth1.GetValueOrDefault() != 0)
          {
            local.TextnumWorkArea.Text15 =
              NumberToString(local.LegalActionDetail.DayOfMonth1.
                GetValueOrDefault(), 15);
            local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
            local.FieldValue.Value =
              Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
              local.Verify.Count, 16 - local.Verify.Count);
          }

          break;
        case "LA*DOM2":
          if (local.LegalActionDetail.DayOfMonth2.GetValueOrDefault() != 0)
          {
            local.TextnumWorkArea.Text15 =
              NumberToString(local.LegalActionDetail.DayOfMonth2.
                GetValueOrDefault(), 15);
            local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
            local.FieldValue.Value =
              Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
              local.Verify.Count, 16 - local.Verify.Count);
          }

          break;
        case "LA*DOW":
          if (local.LegalActionDetail.DayOfWeek.GetValueOrDefault() != 0)
          {
            local.TextnumWorkArea.Text15 =
              NumberToString(local.LegalActionDetail.DayOfWeek.
                GetValueOrDefault(), 15);
            local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
            local.FieldValue.Value =
              Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
              local.Verify.Count, 16 - local.Verify.Count);
          }

          break;
        case "LA*EFDT":
          if (Lt(local.NullDateWorkArea.Date,
            local.LegalActionDetail.EffectiveDate))
          {
            local.DateWorkArea.Date = local.LegalActionDetail.EffectiveDate;
            local.FieldValue.Value = UseSpDocFormatDate();
          }

          break;
        case "LA*FREQ":
          if (!IsEmpty(local.LegalActionDetail.FreqPeriodCode))
          {
            // mjr
            // ---------------------------------------------------
            // 03/08/2001
            // WR# 187 Segment F - WA and WC amounts should always be monthly
            // ----------------------------------------------------------------
            if (!Equal(local.CurrentObligationType.Code, "WA") && !
              Equal(local.CurrentObligationType.Code, "WC"))
            {
              local.ValidateCodeValue.Cdvalue =
                local.LegalActionDetail.FreqPeriodCode ?? Spaces(10);
            }
            else
            {
              local.ValidateCodeValue.Cdvalue = "M";
            }

            local.ValidateCode.CodeName = "LEGAL ACTION PAYMENT FREQNCY";
            UseCabGetCodeValueDescription();
            local.FieldValue.Value = local.ValidateCodeValue.Description;
          }

          break;
        case "LAFILEDT01":
          if (!ReadLegalAction2())
          {
            return;
          }

          if (Lt(local.NullDateWorkArea.Date, entities.LegalAction.FiledDate))
          {
            local.DateWorkArea.Date = entities.LegalAction.FiledDate;
            local.FieldValue.Value = UseSpDocFormatDate();
          }

          break;
        case "LAWCWABW":
          if (AsChar(local.MultipleObligations.Flag) == 'Y')
          {
            break;
          }

          // mjr
          // ------------------------------------------------
          // This value should be calculated already.  If it
          // is not, calculate it now.
          // ---------------------------------------------------
          if (AsChar(local.LwcPlusWa.Flag) != 'B')
          {
            if (IsEmpty(local.FoundObligation.Flag))
            {
              local.FoundObligation.Flag = "N";
            }

            local.LwcPlusWa.Flag = "B";

            if (!IsEmpty(import.SpDocKey.KeyAp))
            {
              // mjr
              // ----------------------------------------
              // 06/18/2001
              // PR# 113159 - Removed effective date check on
              // legal_action_person since it could be in the future
              // -----------------------------------------------------
              local.ObligationType.Code = "WA";

              if (ReadLegalActionDetailObligationType1())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency =
                  local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                local.LwcPlusWa.Percentage =
                  local.LegalActionDetail.Limit.GetValueOrDefault();
              }

              // mjr
              // ----------------------------------------
              // 06/18/2001
              // PR# 113159 - Removed effective date check on
              // legal_action_person since it could be in the future
              // -----------------------------------------------------
              local.ObligationType.Code = "WC";

              if (ReadLegalActionDetailObligationType1())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                  CurrentAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                  .LwcPlusWa.Percentage)
                {
                  local.LwcPlusWa.Percentage =
                    local.LegalActionDetail.Limit.GetValueOrDefault();
                }
              }
            }
            else
            {
              local.ObligationType.Code = "WA";

              if (ReadLegalActionDetailObligationType2())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency =
                  local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                local.LwcPlusWa.Percentage =
                  local.LegalActionDetail.Limit.GetValueOrDefault();
              }

              local.ObligationType.Code = "WC";

              if (ReadLegalActionDetailObligationType2())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                  CurrentAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                  .LwcPlusWa.Percentage)
                {
                  local.LwcPlusWa.Percentage =
                    local.LegalActionDetail.Limit.GetValueOrDefault();
                }
              }
            }
          }

          if (local.LwcPlusWa.TotalCurrency == 0)
          {
            // mjr
            // ---------------------------------------------------
            // 10/24/2000
            // PR# 105251 - WA and WC zero amounts should be SPACES
            // instead of 0.00
            // ----------------------------------------------------------------
            local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
          }
          else
          {
            // mjr
            // ------------------------------------------------
            // Amount is annual, convert it to bi-weekly
            // ---------------------------------------------------
            local.TextnumCommon.TotalCurrency =
              Math.Round(
                local.LwcPlusWa.TotalCurrency /
              26, 2, MidpointRounding.AwayFromZero);
            local.TextnumWorkArea.Text15 =
              NumberToString((long)(local.TextnumCommon.TotalCurrency * 100), 15);
              
            local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
            local.FieldValue.Value =
              Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
              local.Verify.Count, 14 - local.Verify.Count) + "." + Substring
              (local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
          }

          break;
        case "LAWCWASM":
          if (AsChar(local.MultipleObligations.Flag) == 'Y')
          {
            break;
          }

          // mjr
          // ------------------------------------------------
          // This value should be calculated already.  If it
          // is not, calculate it now.
          // ---------------------------------------------------
          if (AsChar(local.LwcPlusWa.Flag) != 'B')
          {
            if (IsEmpty(local.FoundObligation.Flag))
            {
              local.FoundObligation.Flag = "N";
            }

            local.LwcPlusWa.Flag = "B";

            if (!IsEmpty(import.SpDocKey.KeyAp))
            {
              // mjr
              // ----------------------------------------
              // 06/18/2001
              // PR# 113159 - Removed effective date check on
              // legal_action_person since it could be in the future
              // -----------------------------------------------------
              local.ObligationType.Code = "WA";

              if (ReadLegalActionDetailObligationType1())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency =
                  local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                local.LwcPlusWa.Percentage =
                  local.LegalActionDetail.Limit.GetValueOrDefault();
              }

              // mjr
              // ----------------------------------------
              // 06/18/2001
              // PR# 113159 - Removed effective date check on
              // legal_action_person since it could be in the future
              // -----------------------------------------------------
              local.ObligationType.Code = "WC";

              if (ReadLegalActionDetailObligationType1())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                  CurrentAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                  .LwcPlusWa.Percentage)
                {
                  local.LwcPlusWa.Percentage =
                    local.LegalActionDetail.Limit.GetValueOrDefault();
                }
              }
            }
            else
            {
              local.ObligationType.Code = "WA";

              if (ReadLegalActionDetailObligationType2())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency =
                  local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                local.LwcPlusWa.Percentage =
                  local.LegalActionDetail.Limit.GetValueOrDefault();
              }

              local.ObligationType.Code = "WC";

              if (ReadLegalActionDetailObligationType2())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                  CurrentAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                  .LwcPlusWa.Percentage)
                {
                  local.LwcPlusWa.Percentage =
                    local.LegalActionDetail.Limit.GetValueOrDefault();
                }
              }
            }
          }

          if (local.LwcPlusWa.TotalCurrency == 0)
          {
            // mjr
            // ---------------------------------------------------
            // 10/24/2000
            // PR# 105251 - WA and WC zero amounts should be SPACES
            // instead of 0.00
            // ----------------------------------------------------------------
            local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
          }
          else
          {
            // mjr
            // ------------------------------------------------
            // Amount is annual, convert it to semi-monthly
            // ---------------------------------------------------
            local.TextnumCommon.TotalCurrency =
              Math.Round(
                local.LwcPlusWa.TotalCurrency /
              24, 2, MidpointRounding.AwayFromZero);
            local.TextnumWorkArea.Text15 =
              NumberToString((long)(local.TextnumCommon.TotalCurrency * 100), 15);
              
            local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
            local.FieldValue.Value =
              Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
              local.Verify.Count, 14 - local.Verify.Count) + "." + Substring
              (local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
          }

          break;
        case "LAWCWASUM":
          if (AsChar(local.MultipleObligations.Flag) == 'Y')
          {
            break;
          }

          // mjr
          // ------------------------------------------------
          // This value should be calculated already.  If it
          // is not, calculate it now.
          // ---------------------------------------------------
          if (AsChar(local.LwcPlusWa.Flag) != 'B')
          {
            if (IsEmpty(local.FoundObligation.Flag))
            {
              local.FoundObligation.Flag = "N";
            }

            local.LwcPlusWa.Flag = "B";

            if (!IsEmpty(import.SpDocKey.KeyAp))
            {
              // mjr
              // ----------------------------------------
              // 06/18/2001
              // PR# 113159 - Removed effective date check on
              // legal_action_person since it could be in the future
              // -----------------------------------------------------
              local.ObligationType.Code = "WA";

              if (ReadLegalActionDetailObligationType1())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency =
                  local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                local.LwcPlusWa.Percentage =
                  local.LegalActionDetail.Limit.GetValueOrDefault();
              }

              // mjr
              // ----------------------------------------
              // 06/18/2001
              // PR# 113159 - Removed effective date check on
              // legal_action_person since it could be in the future
              // -----------------------------------------------------
              local.ObligationType.Code = "WC";

              if (ReadLegalActionDetailObligationType1())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                  CurrentAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                  .LwcPlusWa.Percentage)
                {
                  local.LwcPlusWa.Percentage =
                    local.LegalActionDetail.Limit.GetValueOrDefault();
                }
              }
            }
            else
            {
              local.ObligationType.Code = "WA";

              if (ReadLegalActionDetailObligationType2())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency =
                  local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                local.LwcPlusWa.Percentage =
                  local.LegalActionDetail.Limit.GetValueOrDefault();
              }

              local.ObligationType.Code = "WC";

              if (ReadLegalActionDetailObligationType2())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                  CurrentAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                  .LwcPlusWa.Percentage)
                {
                  local.LwcPlusWa.Percentage =
                    local.LegalActionDetail.Limit.GetValueOrDefault();
                }
              }
            }
          }

          if (local.LwcPlusWa.TotalCurrency == 0)
          {
            // mjr
            // ---------------------------------------------------
            // 10/24/2000
            // PR# 105251 - WA and WC zero amounts should be SPACES
            // instead of 0.00
            // ----------------------------------------------------------------
            local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
          }
          else
          {
            // mjr
            // ------------------------------------------------
            // Amount is annual, convert it to monthly
            // ---------------------------------------------------
            local.TextnumCommon.TotalCurrency =
              Math.Round(
                local.LwcPlusWa.TotalCurrency /
              12, 2, MidpointRounding.AwayFromZero);
            local.TextnumWorkArea.Text15 =
              NumberToString((long)(local.TextnumCommon.TotalCurrency * 100), 15);
              
            local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
            local.FieldValue.Value =
              Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
              local.Verify.Count, 14 - local.Verify.Count) + "." + Substring
              (local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
          }

          break;
        case "LAWCWAW":
          if (AsChar(local.MultipleObligations.Flag) == 'Y')
          {
            break;
          }

          // mjr
          // ------------------------------------------------
          // This value should be calculated already.  If it
          // is not, calculate it now.
          // ---------------------------------------------------
          if (AsChar(local.LwcPlusWa.Flag) != 'B')
          {
            if (IsEmpty(local.FoundObligation.Flag))
            {
              local.FoundObligation.Flag = "N";
            }

            local.LwcPlusWa.Flag = "B";

            if (!IsEmpty(import.SpDocKey.KeyAp))
            {
              // mjr
              // ----------------------------------------
              // 06/18/2001
              // PR# 113159 - Removed effective date check on
              // legal_action_person since it could be in the future
              // -----------------------------------------------------
              local.ObligationType.Code = "WA";

              if (ReadLegalActionDetailObligationType1())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency =
                  local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                local.LwcPlusWa.Percentage =
                  local.LegalActionDetail.Limit.GetValueOrDefault();
              }

              // mjr
              // ----------------------------------------
              // 06/18/2001
              // PR# 113159 - Removed effective date check on
              // legal_action_person since it could be in the future
              // -----------------------------------------------------
              local.ObligationType.Code = "WC";

              if (ReadLegalActionDetailObligationType1())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                  CurrentAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                  .LwcPlusWa.Percentage)
                {
                  local.LwcPlusWa.Percentage =
                    local.LegalActionDetail.Limit.GetValueOrDefault();
                }
              }
            }
            else
            {
              local.ObligationType.Code = "WA";

              if (ReadLegalActionDetailObligationType2())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.ArrearsAmount =
                      entities.LegalActionDetail.ArrearsAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency =
                  local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                local.LwcPlusWa.Percentage =
                  local.LegalActionDetail.Limit.GetValueOrDefault();
              }

              local.ObligationType.Code = "WC";

              if (ReadLegalActionDetailObligationType2())
              {
                local.FoundObligation.Flag = "Y";

                // mjr
                // ---------------------------------------------------
                // 03/08/2001
                // WR# 187 Segment F - WA and WC amounts should always be 
                // monthly
                // ----------------------------------------------------------------
                switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
                {
                  case "BW":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 26;

                    break;
                  case "SM":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 24;

                    break;
                  case "W":
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 52;

                    break;
                  default:
                    local.LegalActionDetail.CurrentAmount =
                      entities.LegalActionDetail.CurrentAmount.
                        GetValueOrDefault() * 12;

                    break;
                }

                local.LwcPlusWa.TotalCurrency += local.LegalActionDetail.
                  CurrentAmount.GetValueOrDefault();
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                  .LwcPlusWa.Percentage)
                {
                  local.LwcPlusWa.Percentage =
                    local.LegalActionDetail.Limit.GetValueOrDefault();
                }
              }
            }
          }

          if (local.LwcPlusWa.TotalCurrency == 0)
          {
            // mjr
            // ---------------------------------------------------
            // 10/24/2000
            // PR# 105251 - WA and WC zero amounts should be SPACES
            // instead of 0.00
            // ----------------------------------------------------------------
            local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
          }
          else
          {
            // mjr
            // ------------------------------------------------
            // Amount is annual, convert it to weekly
            // ---------------------------------------------------
            local.TextnumCommon.TotalCurrency =
              Math.Round(
                local.LwcPlusWa.TotalCurrency /
              52, 2, MidpointRounding.AwayFromZero);
            local.TextnumWorkArea.Text15 =
              NumberToString((long)(local.TextnumCommon.TotalCurrency * 100), 15);
              
            local.Verify.Count = Verify(local.TextnumWorkArea.Text15, "0");
            local.FieldValue.Value =
              Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
              local.Verify.Count, 14 - local.Verify.Count) + "." + Substring
              (local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
          }

          break;
        case "LAWITHPER":
          if (AsChar(local.MultipleObligations.Flag) == 'Y')
          {
            break;
          }

          if (AsChar(local.LwcPlusWa.Flag) != 'B')
          {
            if (IsEmpty(local.FoundObligation.Flag))
            {
              local.FoundObligation.Flag = "N";
            }

            local.LwcPlusWa.Flag = "B";

            if (!IsEmpty(import.SpDocKey.KeyAp))
            {
              foreach(var item1 in ReadLegalActionDetailObligationType3())
              {
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                  .LwcPlusWa.Percentage)
                {
                  local.LwcPlusWa.Percentage =
                    local.LegalActionDetail.Limit.GetValueOrDefault();
                }
              }
            }
            else
            {
              foreach(var item1 in ReadLegalActionDetailObligationType4())
              {
                local.LegalActionDetail.Limit =
                  entities.LegalActionDetail.Limit;

                switch(local.LegalActionDetail.Limit.GetValueOrDefault())
                {
                  case 50:
                    break;
                  case 55:
                    break;
                  case 60:
                    break;
                  case 65:
                    break;
                  default:
                    local.LegalActionDetail.Limit = 50;

                    break;
                }

                if (local.LegalActionDetail.Limit.GetValueOrDefault() > local
                  .LwcPlusWa.Percentage)
                {
                  local.LwcPlusWa.Percentage =
                    local.LegalActionDetail.Limit.GetValueOrDefault();
                }
              }
            }
          }

          if (local.LwcPlusWa.Percentage == 0)
          {
            local.LwcPlusWa.Percentage = 50;
          }

          local.TextnumWorkArea.Text15 =
            NumberToString(local.LwcPlusWa.Percentage, 15);
          local.FieldValue.Value =
            Substring(local.TextnumWorkArea.Text15, WorkArea.Text15_MaxLength,
            14, 2);

          break;
        default:
          export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
          export.ErrorFieldValue.Value = "Field:  " + TrimEnd
            (entities.Field.Name) + ",  Subroutine:  " + entities
            .Field.SubroutineName;

          break;
      }

      if (!IsEmpty(export.ErrorDocumentField.ScreenPrompt) || !
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // mjr
      // ----------------------------------------------
      // Field is a single value
      //    Process local field_value
      // -------------------------------------------------
      // --------------------------------------------------------
      // Add Field_value
      // Need to update PAD to include doc_field
      // (which means document and field need to be imported)
      // --------------------------------------------------------
      UseSpCabCreateUpdateFieldValue();

      if (IsExitState("DOCUMENT_FIELD_NF_RB"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.ErrorDocumentField.ScreenPrompt = "Creation Error";
        export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

        return;
      }

      ++import.ExpImpRowLockFieldValue.Count;

ReadEach:
      ;
    }

    if (AsChar(local.FoundObligation.Flag) == 'N' && IsEmpty
      (export.ErrorInd.Flag))
    {
      // --06/07/2018  GVandy  CQ62549 	Don't require IWOTERM document to have a
      // 				legal detail.  This is required due to use of
      // 				new IWO_BODY include file that is common to
      // 				all the IWO documents.
      if (Equal(import.Document.Name, "IWOTERM"))
      {
        return;
      }

      // mjr
      // ----------------------------------------------------------
      // If no LDETs are found for this document, don't allow the user
      // to print the document.
      // -------------------------------------------------------------
      export.ErrorInd.Flag = "2";

      // mjr
      // -----------------------------------------------
      // 12/18/1998
      // Allow the print process to finish normally, but display the
      // message after all fields are retrieved.
      // Setting the exitstate ends the print process prematurely.
      // ------------------------------------------------------------
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveFieldValue1(FieldValue source, FieldValue target)
  {
    target.Value = source.Value;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveFieldValue2(FieldValue source, FieldValue target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.ValidateCode.CodeName;
    useImport.CodeValue.Cdvalue = local.ValidateCodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.ValidateCodeValue);
  }

  private void UseSpCabCreateUpdateFieldValue()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Field.Name = entities.Field.Name;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    MoveFieldValue1(local.FieldValue, useImport.FieldValue);

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private string UseSpDocFormatDate()
  {
    var useImport = new SpDocFormatDate.Import();
    var useExport = new SpDocFormatDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(SpDocFormatDate.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private IEnumerable<bool> ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(command, "date1", local.Start.Date.GetValueOrDefault());
        db.
          SetDate(command, "date2", local.LastMonthEnd.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1", local.Start.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.LastMonthEnd.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private IEnumerable<bool> ReadField()
  {
    entities.Field.Populated = false;

    return ReadEach("ReadField",
      (db, command) =>
      {
        db.SetString(command, "docName", import.Document.Name);
        db.SetDate(
          command, "docEffectiveDte",
          import.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "dependancy", import.Field.Dependancy);
        db.SetString(command, "subroutineName", import.Field.SubroutineName);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.Field.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", local.CurrentLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.LaLdetJ.Populated = false;

    return ReadEach("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableDate(
          command, "filedDt1", local.NullDateWorkArea.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "filedDt2",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LaLdetJ.Identifier = db.GetInt32(reader, 0);
        entities.LaLdetJ.Classification = db.GetString(reader, 1);
        entities.LaLdetJ.ActionTaken = db.GetString(reader, 2);
        entities.LaLdetJ.FiledDate = db.GetNullableDate(reader, 3);
        entities.LaLdetJ.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LaLdetJ.EndDate = db.GetNullableDate(reader, 5);
        entities.LaLdetJ.TrbId = db.GetNullableInt32(reader, 6);
        entities.LaLdetJ.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail1()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail1",
      (db, command) =>
      {
        db.SetString(command, "code", local.ObligationType.Code);
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
        db.SetInt32(
          command, "lgaIdentifier", local.CurrentLegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", import.SpDocKey.KeyAp);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 9);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 10);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 14);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail2()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId", entities.ObligationType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
        db.SetInt32(
          command, "lgaIdentifier", local.CurrentLegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", import.SpDocKey.KeyAp);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 9);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 10);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 14);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail3()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail3",
      (db, command) =>
      {
        db.SetString(command, "code", local.ObligationType.Code);
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
        db.SetInt32(
          command, "lgaIdentifier", local.CurrentLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 9);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 10);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 14);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail4()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId", entities.ObligationType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
        db.SetInt32(
          command, "lgaIdentifier", local.CurrentLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 9);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 10);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 14);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadLegalActionDetailObligationType1()
  {
    entities.LegalActionDetail.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadLegalActionDetailObligationType1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
        db.SetInt32(command, "lgaIdentifier", import.SpDocKey.KeyLegalAction);
        db.SetNullableString(command, "cspNumber", import.SpDocKey.KeyAp);
        db.SetString(command, "debtTypCd", local.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 9);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 10);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.ObligationType.Code = db.GetString(reader, 15);
        entities.ObligationType.Classification = db.GetString(reader, 16);
        entities.LegalActionDetail.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadLegalActionDetailObligationType2()
  {
    entities.LegalActionDetail.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadLegalActionDetailObligationType2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
        db.SetString(command, "debtTypCd", local.ObligationType.Code);
        db.SetInt32(command, "lgaIdentifier", import.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 9);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 10);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.ObligationType.Code = db.GetString(reader, 15);
        entities.ObligationType.Classification = db.GetString(reader, 16);
        entities.LegalActionDetail.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailObligationType3()
  {
    entities.LegalActionDetail.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadLegalActionDetailObligationType3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
        db.SetInt32(command, "lgaIdentifier", import.SpDocKey.KeyLegalAction);
        db.SetNullableString(command, "cspNumber", import.SpDocKey.KeyAp);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 9);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 10);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.ObligationType.Code = db.GetString(reader, 15);
        entities.ObligationType.Classification = db.GetString(reader, 16);
        entities.LegalActionDetail.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailObligationType4()
  {
    entities.LegalActionDetail.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadLegalActionDetailObligationType4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
        db.SetInt32(command, "lgaIdentifier", import.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 9);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 10);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.ObligationType.Code = db.GetString(reader, 15);
        entities.ObligationType.Classification = db.GetString(reader, 16);
        entities.LegalActionDetail.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadLegalActionLegalActionDetail1()
  {
    entities.LegalActionDetail.Populated = false;
    entities.LaLdetJ.Populated = false;

    return Read("ReadLegalActionLegalActionDetail1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.LaLdetJ.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LaLdetJ.Classification = db.GetString(reader, 1);
        entities.LaLdetJ.ActionTaken = db.GetString(reader, 2);
        entities.LaLdetJ.FiledDate = db.GetNullableDate(reader, 3);
        entities.LaLdetJ.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LaLdetJ.EndDate = db.GetNullableDate(reader, 5);
        entities.LaLdetJ.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 7);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 8);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 14);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 15);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 16);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 18);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 19);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 20);
        entities.LegalActionDetail.Populated = true;
        entities.LaLdetJ.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionLegalActionDetail2()
  {
    entities.LegalActionDetail.Populated = false;
    entities.LaLdetJ.Populated = false;

    return Read("ReadLegalActionLegalActionDetail2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.LaLdetJ.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LaLdetJ.Classification = db.GetString(reader, 1);
        entities.LaLdetJ.ActionTaken = db.GetString(reader, 2);
        entities.LaLdetJ.FiledDate = db.GetNullableDate(reader, 3);
        entities.LaLdetJ.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LaLdetJ.EndDate = db.GetNullableDate(reader, 5);
        entities.LaLdetJ.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 7);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 8);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 14);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 15);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 16);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 18);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 19);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 20);
        entities.LegalActionDetail.Populated = true;
        entities.LaLdetJ.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionLegalActionDetail3()
  {
    entities.LegalActionDetail.Populated = false;
    entities.LaLdetJ.Populated = false;

    return Read("ReadLegalActionLegalActionDetail3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.LaLdetJ.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LaLdetJ.Classification = db.GetString(reader, 1);
        entities.LaLdetJ.ActionTaken = db.GetString(reader, 2);
        entities.LaLdetJ.FiledDate = db.GetNullableDate(reader, 3);
        entities.LaLdetJ.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LaLdetJ.EndDate = db.GetNullableDate(reader, 5);
        entities.LaLdetJ.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 7);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 8);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 14);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 15);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 16);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 18);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 19);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 20);
        entities.LegalActionDetail.Populated = true;
        entities.LaLdetJ.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionLegalActionDetailObligationType1()
  {
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadLegalActionLegalActionDetailObligationType1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.SpDocKey.KeyLegalAction);
        db.SetDate(
          command, "effectiveDt",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 6);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 7);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 8);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 13);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 14);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 15);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 16);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 18);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 19);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 19);
        entities.ObligationType.Code = db.GetString(reader, 20);
        entities.ObligationType.Classification = db.GetString(reader, 21);
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionDetailObligationType2()
  {
    entities.LegalActionDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.LaLdetJ.Populated = false;

    return ReadEach("ReadLegalActionLegalActionDetailObligationType2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableDate(
          command, "endDt", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "filedDt", local.NullDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LaLdetJ.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LaLdetJ.Classification = db.GetString(reader, 1);
        entities.LaLdetJ.ActionTaken = db.GetString(reader, 2);
        entities.LaLdetJ.FiledDate = db.GetNullableDate(reader, 3);
        entities.LaLdetJ.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LaLdetJ.EndDate = db.GetNullableDate(reader, 5);
        entities.LaLdetJ.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 7);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 8);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 13);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 14);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 15);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 16);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 18);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 19);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 20);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.Classification = db.GetString(reader, 22);
        entities.LegalActionDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.LaLdetJ.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadMonthlyObligorSummary1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.MonthlyObligorSummary.Populated = false;

    return Read("ReadMonthlyObligorSummary1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetNullableInt32(
          command, "obgSGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "cspSNumber", entities.Obligation.CspNumber);
        db.SetNullableString(command, "cpaSType", entities.Obligation.CpaType);
        db.SetInt32(command, "fnclMsumYrMth", local.LastMonthEnd.YearMonth);
      },
      (db, reader) =>
      {
        entities.MonthlyObligorSummary.Type1 = db.GetString(reader, 0);
        entities.MonthlyObligorSummary.YearMonth = db.GetInt32(reader, 1);
        entities.MonthlyObligorSummary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.MonthlyObligorSummary.CpaSType =
          db.GetNullableString(reader, 3);
        entities.MonthlyObligorSummary.CspSNumber =
          db.GetNullableString(reader, 4);
        entities.MonthlyObligorSummary.ObgSGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.MonthlyObligorSummary.OtyType = db.GetNullableInt32(reader, 6);
        entities.MonthlyObligorSummary.ForMthCurrBal =
          db.GetNullableDecimal(reader, 7);
        entities.MonthlyObligorSummary.Populated = true;
        CheckValid<MonthlyObligorSummary>("Type1",
          entities.MonthlyObligorSummary.Type1);
        CheckValid<MonthlyObligorSummary>("CpaSType",
          entities.MonthlyObligorSummary.CpaSType);
      });
  }

  private bool ReadMonthlyObligorSummary2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.MonthlyObligorSummary.Populated = false;

    return Read("ReadMonthlyObligorSummary2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetNullableInt32(
          command, "obgSGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "cspSNumber", entities.Obligation.CspNumber);
        db.SetNullableString(command, "cpaSType", entities.Obligation.CpaType);
        db.SetInt32(command, "fnclMsumYrMth", local.LastMonthEnd.YearMonth);
      },
      (db, reader) =>
      {
        entities.MonthlyObligorSummary.Type1 = db.GetString(reader, 0);
        entities.MonthlyObligorSummary.YearMonth = db.GetInt32(reader, 1);
        entities.MonthlyObligorSummary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.MonthlyObligorSummary.CpaSType =
          db.GetNullableString(reader, 3);
        entities.MonthlyObligorSummary.CspSNumber =
          db.GetNullableString(reader, 4);
        entities.MonthlyObligorSummary.ObgSGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.MonthlyObligorSummary.OtyType = db.GetNullableInt32(reader, 6);
        entities.MonthlyObligorSummary.ForMthCurrBal =
          db.GetNullableDecimal(reader, 7);
        entities.MonthlyObligorSummary.Populated = true;
        CheckValid<MonthlyObligorSummary>("Type1",
          entities.MonthlyObligorSummary.Type1);
        CheckValid<MonthlyObligorSummary>("CpaSType",
          entities.MonthlyObligorSummary.CpaSType);
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LaLdetJ.Identifier);
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1", local.Start.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.LastMonthEnd.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtAdjustmentType =
          db.GetString(reader, 6);
        entities.ObligationTransaction.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.ObligationTransaction.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 9);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 10);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 11);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 12);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.ObligationTransaction.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadObligationTransactionDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1", local.Start.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.LastMonthEnd.Timestamp.GetValueOrDefault());
        db.SetDate(command, "date1", local.Start.Date.GetValueOrDefault());
        db.
          SetDate(command, "date2", local.LastMonthEnd.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtAdjustmentType =
          db.GetString(reader, 6);
        entities.ObligationTransaction.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.ObligationTransaction.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 9);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 10);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 11);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 12);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 12);
        entities.DebtDetail.DueDt = db.GetDate(reader, 13);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 14);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 15);
        entities.ObligationTransaction.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.ObligationTransaction.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadObligationType1()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType1",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", local.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType2",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.Populated = true;
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
    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of ExpImpRowLockFieldValue.
    /// </summary>
    [JsonPropertyName("expImpRowLockFieldValue")]
    public Common ExpImpRowLockFieldValue
    {
      get => expImpRowLockFieldValue ??= new();
      set => expImpRowLockFieldValue = value;
    }

    private SpDocKey spDocKey;
    private FieldValue fieldValue;
    private Document document;
    private Field field;
    private Infrastructure infrastructure;
    private Common expImpRowLockFieldValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
    }

    /// <summary>
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
    }

    /// <summary>
    /// A value of ErrorInd.
    /// </summary>
    [JsonPropertyName("errorInd")]
    public Common ErrorInd
    {
      get => errorInd ??= new();
      set => errorInd = value;
    }

    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
    private Common errorInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Previous12Weeks.
    /// </summary>
    [JsonPropertyName("previous12Weeks")]
    public DateWorkArea Previous12Weeks
    {
      get => previous12Weeks ??= new();
      set => previous12Weeks = value;
    }

    /// <summary>
    /// A value of CoarrFieldsCalculated.
    /// </summary>
    [JsonPropertyName("coarrFieldsCalculated")]
    public Common CoarrFieldsCalculated
    {
      get => coarrFieldsCalculated ??= new();
      set => coarrFieldsCalculated = value;
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
    /// A value of CocsFieldsCalculated.
    /// </summary>
    [JsonPropertyName("cocsFieldsCalculated")]
    public Common CocsFieldsCalculated
    {
      get => cocsFieldsCalculated ??= new();
      set => cocsFieldsCalculated = value;
    }

    /// <summary>
    /// A value of TextDay.
    /// </summary>
    [JsonPropertyName("textDay")]
    public WorkArea TextDay
    {
      get => textDay ??= new();
      set => textDay = value;
    }

    /// <summary>
    /// A value of TextMonth.
    /// </summary>
    [JsonPropertyName("textMonth")]
    public WorkArea TextMonth
    {
      get => textMonth ??= new();
      set => textMonth = value;
    }

    /// <summary>
    /// A value of TextYear.
    /// </summary>
    [JsonPropertyName("textYear")]
    public WorkArea TextYear
    {
      get => textYear ??= new();
      set => textYear = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of ZdelLocalLatest.
    /// </summary>
    [JsonPropertyName("zdelLocalLatest")]
    public MonthlyObligorSummary ZdelLocalLatest
    {
      get => zdelLocalLatest ??= new();
      set => zdelLocalLatest = value;
    }

    /// <summary>
    /// A value of LastMonthEnd.
    /// </summary>
    [JsonPropertyName("lastMonthEnd")]
    public DateWorkArea LastMonthEnd
    {
      get => lastMonthEnd ??= new();
      set => lastMonthEnd = value;
    }

    /// <summary>
    /// A value of CurrentObligationType.
    /// </summary>
    [JsonPropertyName("currentObligationType")]
    public ObligationType CurrentObligationType
    {
      get => currentObligationType ??= new();
      set => currentObligationType = value;
    }

    /// <summary>
    /// A value of NullLegalActionDetail.
    /// </summary>
    [JsonPropertyName("nullLegalActionDetail")]
    public LegalActionDetail NullLegalActionDetail
    {
      get => nullLegalActionDetail ??= new();
      set => nullLegalActionDetail = value;
    }

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
    /// A value of Original.
    /// </summary>
    [JsonPropertyName("original")]
    public LegalAction Original
    {
      get => original ??= new();
      set => original = value;
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
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public Common Verify
    {
      get => verify ??= new();
      set => verify = value;
    }

    /// <summary>
    /// A value of CurrentLegalAction.
    /// </summary>
    [JsonPropertyName("currentLegalAction")]
    public LegalAction CurrentLegalAction
    {
      get => currentLegalAction ??= new();
      set => currentLegalAction = value;
    }

    /// <summary>
    /// A value of TextnumCommon.
    /// </summary>
    [JsonPropertyName("textnumCommon")]
    public Common TextnumCommon
    {
      get => textnumCommon ??= new();
      set => textnumCommon = value;
    }

    /// <summary>
    /// A value of FoundObligation.
    /// </summary>
    [JsonPropertyName("foundObligation")]
    public Common FoundObligation
    {
      get => foundObligation ??= new();
      set => foundObligation = value;
    }

    /// <summary>
    /// A value of MultipleObligations.
    /// </summary>
    [JsonPropertyName("multipleObligations")]
    public Common MultipleObligations
    {
      get => multipleObligations ??= new();
      set => multipleObligations = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Field Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Field Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of TextnumWorkArea.
    /// </summary>
    [JsonPropertyName("textnumWorkArea")]
    public WorkArea TextnumWorkArea
    {
      get => textnumWorkArea ??= new();
      set => textnumWorkArea = value;
    }

    /// <summary>
    /// A value of LwcPlusWa.
    /// </summary>
    [JsonPropertyName("lwcPlusWa")]
    public Common LwcPlusWa
    {
      get => lwcPlusWa ??= new();
      set => lwcPlusWa = value;
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
    /// A value of ValidateCode.
    /// </summary>
    [JsonPropertyName("validateCode")]
    public Code ValidateCode
    {
      get => validateCode ??= new();
      set => validateCode = value;
    }

    /// <summary>
    /// A value of ValidateCodeValue.
    /// </summary>
    [JsonPropertyName("validateCodeValue")]
    public CodeValue ValidateCodeValue
    {
      get => validateCodeValue ??= new();
      set => validateCodeValue = value;
    }

    private DateWorkArea previous12Weeks;
    private Common coarrFieldsCalculated;
    private CsePerson csePerson;
    private Common cocsFieldsCalculated;
    private WorkArea textDay;
    private WorkArea textMonth;
    private WorkArea textYear;
    private DateWorkArea start;
    private MonthlyObligorSummary zdelLocalLatest;
    private DateWorkArea lastMonthEnd;
    private ObligationType currentObligationType;
    private LegalActionDetail nullLegalActionDetail;
    private LegalActionDetail legalActionDetail;
    private LegalAction original;
    private LegalAction legalAction;
    private Common verify;
    private LegalAction currentLegalAction;
    private Common textnumCommon;
    private Common foundObligation;
    private Common multipleObligations;
    private Common length;
    private Field temp;
    private Field field;
    private FieldValue fieldValue;
    private DateWorkArea currentDateWorkArea;
    private Field previous;
    private DateWorkArea nullDateWorkArea;
    private ObligationType obligationType;
    private WorkArea textnumWorkArea;
    private Common lwcPlusWa;
    private DateWorkArea dateWorkArea;
    private Code validateCode;
    private CodeValue validateCodeValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CourtOrder.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    public LegalAction CourtOrder
    {
      get => courtOrder ??= new();
      set => courtOrder = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of MonthlyObligorSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligorSummary")]
    public MonthlyObligorSummary MonthlyObligorSummary
    {
      get => monthlyObligorSummary ??= new();
      set => monthlyObligorSummary = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of LaLdetJ.
    /// </summary>
    [JsonPropertyName("laLdetJ")]
    public LegalAction LaLdetJ
    {
      get => laLdetJ ??= new();
      set => laLdetJ = value;
    }

    private LegalAction courtOrder;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private CsePersonAccount csePersonAccount;
    private Collection collection;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private MonthlyObligorSummary monthlyObligorSummary;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private CaseRole caseRole;
    private Case1 case1;
    private Tribunal tribunal;
    private Field field;
    private DocumentField documentField;
    private Document document;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private ObligationType obligationType;
    private LegalAction laLdetJ;
  }
#endregion
}
