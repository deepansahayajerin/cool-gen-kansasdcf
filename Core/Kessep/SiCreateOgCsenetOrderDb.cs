// Program: SI_CREATE_OG_CSENET_ORDER_DB, ID: 372382221, model: 746.
// Short name: SWE02574
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_OG_CSENET_ORDER_DB.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the entity type that contains Interstate (CSENet) Order 
/// information.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateOgCsenetOrderDb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_OG_CSENET_ORDER_DB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateOgCsenetOrderDb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateOgCsenetOrderDb.
  /// </summary>
  public SiCreateOgCsenetOrderDb(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------
    // 04/29/97    JeHoward         Current date fix.
    // 05/10/99    C. Ott           Re-wrote action block to derive
    //                              IS Support Order from Debt Detail
    //                              rather than Legal Action Detail
    // ----------------------------------------------------------------
    // ****************************************************************
    // 8/5/99   C. Ott   Added validations for mandatory fields.
    // ****************************************************************
    // ****************************************************************
    // 9/10/99   C. Ott   Modified for Problem # 73094
    // ****************************************************************
    // ****************************************************************
    // 12/28/99   C. Ott   Modified for Problem # 82317.  Added rollback exit 
    // states.
    // ****************************************************************
    // ***************************************************************
    // 01/10/00  C. Ott   PR # 84465.  Added logic to get all NA programs.
    // **************************************************************
    // ****************************************************************
    // 03/14/00  R. Jean  PR89878 - Dropping filed date in logic.
    // ****************************************************************
    // 02/07/01 swsrchf I00112346  Exit the action block when the classification
    // is "U"
    // ---------------------------------------------------------------------------------------
    // 11/20/01 T.Bobb  Modified for PR00131962
    // **************************************************************
    local.Current.Date = Now().Date;

    if (!ReadInterstateCase())
    {
      ExitState = "SI0000_CSENET_CASE_NF_RB";

      return;
    }

    if (!ReadCase())
    {
      ExitState = "CASE_NF_RB";

      return;
    }

    if (ReadLegalAction())
    {
      if (AsChar(entities.LegalAction.Classification) != 'J')
      {
        // *** Problem report I00112346
        // *** 02/07/01 swsrchf
        // *** start
        if (AsChar(entities.LegalAction.Classification) == 'U')
        {
          return;
        }

        // *** end
        // *** 02/07/01 swsrchf
        // *** Problem report I00112346
        ExitState = "SI0000_LEG_ACT_MUST_BE_J_RB";

        return;
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF_RB";

      return;
    }

    if (ReadFips())
    {
      local.InterstateSupportOrder.FipsState =
        NumberToString(entities.Tribunal1.State, 2);
      local.InterstateSupportOrder.FipsCounty =
        NumberToString(entities.Tribunal1.County, 3);
      local.InterstateSupportOrder.FipsLocation =
        NumberToString(entities.Tribunal1.Location, 2);
    }
    else
    {
      ExitState = "SI0000_TRIB_FIPS_NF_RB";

      return;
    }

    local.InterstateSupportOrder.Number =
      entities.LegalAction.CourtCaseNumber ?? Spaces(17);
    local.InterstateSupportOrder.TribunalCaseNumber =
      entities.LegalAction.CourtCaseNumber;
    local.InterstateSupportOrder.OrderFilingDate =
      entities.LegalAction.FiledDate;
    local.InterstateSupportOrder.Type1 = entities.LegalAction.OrderAuthority;
    local.InterstateSupportOrder.CancelDate = null;
    local.CurrentMonth.YearMonth = UseCabGetYearMonthFromDate();
    local.CurrentMonth.Date = IntToDate(local.CurrentMonth.YearMonth * 100 + 1);

    foreach(var item in ReadLegalActionDetail())
    {
      local.InterstateSupportOrder.MedicalOrderedInd = "N";
      local.InterstateSupportOrder.PaymentFreq =
        entities.LegalActionDetail.FreqPeriodCode;
      local.InterstateSupportOrder.AmountOrdered =
        entities.LegalActionDetail.CurrentAmount;
      local.InterstateSupportOrder.EffectiveDate =
        entities.LegalActionDetail.EffectiveDate;

      if (Equal(entities.LegalActionDetail.NonFinOblgType, "HIC") && Lt
        (local.Current.Date, entities.LegalActionDetail.EndDate))
      {
        local.InterstateSupportOrder.MedicalOrderedInd = "Y";
      }

      ++local.LegalActionDetail.Count;
      local.InterstateSupportOrder.ArrearsFreqAmount =
        local.InterstateSupportOrder.ArrearsFreqAmount.GetValueOrDefault() + entities
        .LegalActionDetail.ArrearsAmount.GetValueOrDefault();

      if (local.InterstateSupportOrder.ArrearsFreqAmount.GetValueOrDefault() > 0
        )
      {
        local.InterstateSupportOrder.ArrearsFreq =
          entities.LegalActionDetail.FreqPeriodCode;
      }

      local.DebtExists.Flag = "";

      foreach(var item1 in ReadObligationObligationType())
      {
        ++local.Obligation.Count;

        if (ReadDebtCsePerson())
        {
          for(import.Participant.Index = 0; import.Participant.Index < import
            .Participant.Count; ++import.Participant.Index)
          {
            if (Equal(import.Participant.Item.CsePersonsWorkSet.Number,
              entities.CsePerson.Number))
            {
              local.DebtExists.Flag = "Y";
            }
          }

          if (AsChar(local.DebtExists.Flag) != 'Y')
          {
            continue;
          }

          ReadDebtDetail();
        }
        else
        {
          continue;
        }

        if (AsChar(entities.ObligationType.Classification) == 'A')
        {
          if (!Lt(entities.DebtDetail.DueDt, local.CurrentMonth.Date))
          {
            local.NonArrears.Flag = "Y";

            continue;
          }
        }

        if (Lt(local.InterstateSupportOrder.DateOfLastPayment,
          entities.Obligation.LastCollDt))
        {
          local.InterstateSupportOrder.DateOfLastPayment =
            entities.Obligation.LastCollDt;
        }

        if (Equal(entities.ObligationType.Code, "MS") && Lt
          (local.Current.Date, entities.LegalActionDetail.EndDate))
        {
          local.InterstateSupportOrder.MedicalOrderedInd = "Y";
        }

        local.InterstateSupportOrder.DebtType = entities.ObligationType.Code;

        if (AsChar(entities.ObligationType.Classification) == 'M' || entities
          .ObligationType.SystemGeneratedIdentifier == 3 || entities
          .ObligationType.SystemGeneratedIdentifier == 19)
        {
          if (AsChar(entities.ObligationType.Classification) == 'A')
          {
            if (ReadObligationPaymentSchedule())
            {
              local.InterstateSupportOrder.MedicalFromDate =
                entities.ObligationPaymentSchedule.StartDt;

              if (Equal(entities.ObligationPaymentSchedule.EndDt,
                new DateTime(2099, 12, 31)))
              {
                local.InterstateSupportOrder.MedicalThruDate = null;
              }
              else
              {
                local.InterstateSupportOrder.MedicalThruDate =
                  entities.ObligationPaymentSchedule.EndDt;
              }
            }
            else
            {
              // **************************************************************
              // Obligation Payment Schedule is not required, this is not an 
              // error.
              // ***************************************************************
            }
          }
          else
          {
            local.InterstateSupportOrder.MedicalFromDate =
              entities.DebtDetail.CoveredPrdStartDt;
            local.InterstateSupportOrder.MedicalThruDate =
              entities.DebtDetail.CoveredPrdEndDt;
          }

          local.InterstateSupportOrder.MedicalAmount =
            entities.DebtDetail.BalanceDueAmt + local
            .InterstateSupportOrder.MedicalAmount.GetValueOrDefault();
        }
        else
        {
          UseFnDeterminePgmForDebtDetail();

          if (local.Program.SystemGeneratedIdentifier == 2 || local
            .Program.SystemGeneratedIdentifier == 14)
          {
            local.InterstateSupportOrder.ArrearsAfdcAmount =
              local.InterstateSupportOrder.ArrearsAfdcAmount.
                GetValueOrDefault() + entities.DebtDetail.BalanceDueAmt;

            // ****************************************************************
            // AFDC
            // ****************************************************************
            if (ReadPersonProgram2())
            {
              if (local.InterstateSupportOrder.ArrearsAfdcFromDate != null)
              {
                if (Lt(entities.PersonProgram.EffectiveDate,
                  local.InterstateSupportOrder.ArrearsAfdcFromDate))
                {
                  local.InterstateSupportOrder.ArrearsAfdcFromDate =
                    entities.PersonProgram.EffectiveDate;
                }
                else
                {
                  // ****************************************************************
                  // Local date value is populated and is less than the date 
                  // value currently read, do not change value of local view.
                  // ****************************************************************
                }
              }
              else
              {
                local.InterstateSupportOrder.ArrearsAfdcFromDate =
                  entities.PersonProgram.EffectiveDate;
              }
            }

            if (ReadPersonProgram5())
            {
              if (local.InterstateSupportOrder.ArrearsAfdcThruDate != null)
              {
                if (Lt(local.InterstateSupportOrder.ArrearsAfdcThruDate,
                  entities.PersonProgram.DiscontinueDate))
                {
                  local.InterstateSupportOrder.ArrearsAfdcThruDate =
                    entities.PersonProgram.DiscontinueDate;
                }
                else
                {
                  // ****************************************************************
                  // Local date value is populated and is greater than the date 
                  // value currently read, do not change value of local view.
                  // ****************************************************************
                }
              }
              else
              {
                local.InterstateSupportOrder.ArrearsAfdcThruDate =
                  entities.PersonProgram.DiscontinueDate;
              }
            }
          }
          else if (local.Program.SystemGeneratedIdentifier == 15 || local
            .Program.SystemGeneratedIdentifier == 16 || local
            .Program.SystemGeneratedIdentifier == 3)
          {
            // ****************************************************************
            // AFDC FOSTER CARE
            // ****************************************************************
            local.InterstateSupportOrder.FosterCareAmount =
              local.InterstateSupportOrder.FosterCareAmount.
                GetValueOrDefault() + entities.DebtDetail.BalanceDueAmt;

            if (ReadPersonProgram3())
            {
              if (local.InterstateSupportOrder.FosterCareFromDate != null)
              {
                if (Lt(entities.PersonProgram.EffectiveDate,
                  local.InterstateSupportOrder.FosterCareFromDate))
                {
                  local.InterstateSupportOrder.FosterCareFromDate =
                    entities.PersonProgram.EffectiveDate;
                }
                else
                {
                  // ****************************************************************
                  // Local date value is populated and is less than the date 
                  // value currently read, do not change value of local view.
                  // ****************************************************************
                }
              }
              else
              {
                local.InterstateSupportOrder.FosterCareFromDate =
                  entities.PersonProgram.EffectiveDate;
              }
            }

            if (ReadPersonProgram6())
            {
              if (local.InterstateSupportOrder.FosterCareThruDate != null)
              {
                if (Lt(local.InterstateSupportOrder.FosterCareThruDate,
                  entities.PersonProgram.DiscontinueDate))
                {
                  local.InterstateSupportOrder.FosterCareThruDate =
                    entities.PersonProgram.DiscontinueDate;
                }
                else
                {
                  // ****************************************************************
                  // Local date value is populated and is greater than the date 
                  // value currently read, do not change value of local view.
                  // ****************************************************************
                }
              }
              else
              {
                local.InterstateSupportOrder.FosterCareThruDate =
                  entities.PersonProgram.DiscontinueDate;
              }
            }
          }
          else
          {
            // ****************************************************************
            // NON-AFDC
            // ****************************************************************
            // ***************************************************************
            // 01/10/00  C. Ott   PR # 84465.  Added logic to get all NA 
            // programs.  Any program other than AF or FC will be NA.
            // **************************************************************
            local.InterstateSupportOrder.ArrearsNonAfdcAmount =
              local.InterstateSupportOrder.ArrearsNonAfdcAmount.
                GetValueOrDefault() + entities.DebtDetail.BalanceDueAmt;

            if (ReadPersonProgram1())
            {
              if (local.InterstateSupportOrder.ArrearsNonAfdcFromDate != null)
              {
                if (Lt(entities.PersonProgram.EffectiveDate,
                  local.InterstateSupportOrder.ArrearsNonAfdcFromDate))
                {
                  local.InterstateSupportOrder.ArrearsNonAfdcFromDate =
                    entities.PersonProgram.EffectiveDate;
                }
                else
                {
                  // ****************************************************************
                  // Local date value is populated and is less than the date 
                  // value currently read, do not change value of local view.
                  // ****************************************************************
                }
              }
              else
              {
                local.InterstateSupportOrder.ArrearsNonAfdcFromDate =
                  entities.PersonProgram.EffectiveDate;
              }
            }

            if (ReadPersonProgram4())
            {
              if (local.InterstateSupportOrder.ArrearsNonAfdcThruDate != null)
              {
                if (Lt(local.InterstateSupportOrder.ArrearsNonAfdcThruDate,
                  entities.PersonProgram.DiscontinueDate))
                {
                  local.InterstateSupportOrder.ArrearsNonAfdcThruDate =
                    entities.PersonProgram.DiscontinueDate;
                }
                else
                {
                  // ****************************************************************
                  // Local date value is populated and is greater than the date 
                  // value currently read, do not change value of local view.
                  // ****************************************************************
                }
              }
              else
              {
                local.InterstateSupportOrder.ArrearsNonAfdcThruDate =
                  entities.PersonProgram.DiscontinueDate;
              }
            }
          }
        }
      }

      if (AsChar(local.DebtExists.Flag) == 'Y')
      {
        ReadInterstateSupportOrder();
        local.InterstateSupportOrder.SystemGeneratedSequenceNum =
          entities.InterstateSupportOrder.SystemGeneratedSequenceNum + 1;
        local.InterstateSupportOrder.ArrearsTotalAmount =
          local.InterstateSupportOrder.ArrearsAfdcAmount.GetValueOrDefault() + local
          .InterstateSupportOrder.ArrearsNonAfdcAmount.GetValueOrDefault() + local
          .InterstateSupportOrder.MedicalAmount.GetValueOrDefault() + local
          .InterstateSupportOrder.FosterCareAmount.GetValueOrDefault();

        // ****************************************************************
        // 8/5/99   C. Ott   Added validations for mandatory fields.
        // 12/28/99   Modified for rollback exit states.
        // ****************************************************************
        if (IsEmpty(local.InterstateSupportOrder.FipsState))
        {
          ExitState = "SI0000_TRIB_FIPS_STATE_CNTY_REQ";

          return;
        }

        if (IsEmpty(local.InterstateSupportOrder.FipsCounty))
        {
          ExitState = "SI0000_TRIB_FIPS_STATE_CNTY_REQ";

          return;
        }

        if (IsEmpty(local.InterstateSupportOrder.Number))
        {
          ExitState = "SI0000_CSENET_SUP_ORD_NUMB_REQ";

          return;
        }

        if (IsEmpty(local.InterstateSupportOrder.DebtType))
        {
          ExitState = "SI0000_CSENET_ORD_DEBT_TYPE_REQ";

          return;
        }

        if (Equal(local.InterstateSupportOrder.OrderFilingDate,
          local.Blank.OrderFilingDate))
        {
          ExitState = "SI0000_ORDER_FILING_DATE_REQ_RB";

          return;
        }

        // *****************************************************************
        // 11/18/99  C. Ott   PR # ?????
        // *****************************************************************
        if (local.InterstateSupportOrder.AmountOrdered.GetValueOrDefault() > 0)
        {
          if (IsEmpty(local.InterstateSupportOrder.PaymentFreq))
          {
            ExitState = "SI0000_CSENET_PAY_FREQ_REQ_RB";

            return;
          }
        }

        // ****************************************************************
        // 11/20/01 T.Bobb PR00131962 Added additional check
        // for arrears_freq_amount
        // ****************************************************************
        if (!IsEmpty(local.InterstateSupportOrder.PaymentFreq))
        {
          if (local.InterstateSupportOrder.AmountOrdered.GetValueOrDefault() ==
            0 && local
            .InterstateSupportOrder.ArrearsFreqAmount.GetValueOrDefault() == 0)
          {
            ExitState = "SI0000_CSENET_AMT_ORD_RB";

            return;
          }
        }

        if (Equal(import.InterstateCase.FunctionalTypeCode, "ENF") || (
          Equal(import.InterstateCase.FunctionalTypeCode, "EST") || Equal
          (import.InterstateCase.FunctionalTypeCode, "PAT")) && AsChar
          (import.InterstateCase.ActionCode) == 'P')
        {
          if (IsEmpty(local.InterstateSupportOrder.Type1))
          {
            ExitState = "SI0000_CSENET_ORD_TYPE_REQ_RB";

            return;
          }

          if (Equal(local.InterstateSupportOrder.EffectiveDate,
            local.Blank.EffectiveDate))
          {
            ExitState = "SI0000_CSENET_ORD_EFF_DATE_REQ";

            return;
          }

          if (local.InterstateSupportOrder.ArrearsFreqAmount.
            GetValueOrDefault() > 0)
          {
            if (IsEmpty(local.InterstateSupportOrder.ArrearsFreq))
            {
              ExitState = "SI0000_CSENET_ARREARS_FREQ_REQ";

              return;
            }
          }

          if (!IsEmpty(local.InterstateSupportOrder.ArrearsFreq))
          {
            if (local.InterstateSupportOrder.ArrearsFreqAmount.
              GetValueOrDefault() == 0)
            {
              ExitState = "SI0000_CSENET_ARR_FREQ_AMT_REQ";

              return;
            }
          }

          if (!IsEmpty(local.InterstateSupportOrder.ArrearsFreq) || local
            .InterstateSupportOrder.ArrearsFreqAmount.GetValueOrDefault() > 0)
          {
            if (local.InterstateSupportOrder.ArrearsTotalAmount.
              GetValueOrDefault() == 0)
            {
              ExitState = "SI0000_CSENET_ARRS_TOT_AMT_REQ";

              return;
            }
          }

          if (local.InterstateSupportOrder.ArrearsAfdcAmount.
            GetValueOrDefault() > 0)
          {
            if (Equal(local.InterstateSupportOrder.ArrearsAfdcFromDate,
              local.Blank.ArrearsAfdcFromDate))
            {
              ExitState = "SI0000_AF_ARRS_DATE_REQ_RB";

              return;
            }

            if (Equal(local.InterstateSupportOrder.ArrearsAfdcThruDate,
              local.Blank.ArrearsAfdcThruDate))
            {
              ExitState = "SI0000_AF_ARRS_THRU_DATE_REQ_RB";

              return;
            }
          }

          if (local.InterstateSupportOrder.ArrearsNonAfdcAmount.
            GetValueOrDefault() > 0)
          {
            if (Equal(local.InterstateSupportOrder.ArrearsNonAfdcFromDate,
              local.Blank.ArrearsNonAfdcFromDate))
            {
              ExitState = "SI0000_NA_ARRS_DATE_REQ_RB";

              return;
            }

            if (Equal(local.InterstateSupportOrder.ArrearsNonAfdcThruDate,
              local.Blank.ArrearsNonAfdcThruDate))
            {
              ExitState = "SI0000_NA_ARRS_THRU_DATE_REQ_RB";

              return;
            }
          }

          if (local.InterstateSupportOrder.FosterCareAmount.
            GetValueOrDefault() > 0)
          {
            if (Equal(local.InterstateSupportOrder.FosterCareFromDate,
              local.Blank.FosterCareFromDate))
            {
              ExitState = "SI0000_FC_ARRS_DATE_REQ_RB";

              return;
            }

            if (Equal(local.InterstateSupportOrder.FosterCareThruDate,
              local.Blank.FosterCareThruDate))
            {
              ExitState = "SI0000_FC_ARRS_THRU_DATE_REQ_RB";

              return;
            }
          }

          if (local.InterstateSupportOrder.MedicalAmount.GetValueOrDefault() > 0
            )
          {
            if (Equal(local.InterstateSupportOrder.MedicalFromDate,
              local.Blank.MedicalFromDate))
            {
              ExitState = "SI0000_MA_ARRS_DATE_REQ_RB";

              return;
            }

            if (Equal(local.InterstateSupportOrder.MedicalThruDate,
              local.Blank.MedicalThruDate))
            {
              ExitState = "SI0000_MA_ARRS_THRU_DATE_REQ_RB";

              return;
            }
          }
        }

        if ((Equal(import.InterstateCase.FunctionalTypeCode, "ENF") || Equal
          (import.InterstateCase.FunctionalTypeCode, "EST")) && AsChar
          (import.InterstateCase.ActionCode) == 'P')
        {
          if (IsEmpty(local.InterstateSupportOrder.MedicalOrderedInd))
          {
            ExitState = "SI0000_MED_ORDERED_IND_REQ_RB";

            return;
          }
        }

        if (Equal(import.InterstateCase.FunctionalTypeCode, "PAT") && AsChar
          (import.InterstateCase.ActionCode) == 'P' && AsChar
          (local.InterstateSupportOrder.Type1) != 'P')
        {
          if (IsEmpty(local.InterstateSupportOrder.MedicalOrderedInd))
          {
            ExitState = "SI0000_MED_ORDERED_IND_REQ_RB";

            return;
          }
        }

        try
        {
          CreateInterstateSupportOrder();
          ++export.OrderDbCreated.Count;
          MoveInterstateSupportOrder(local.Blank, local.InterstateSupportOrder);

          // ****************************************************************
          // 03/14/00  R. Jean  PR89878 - Dropping filed date in logic.
          // Reset filed date to legal action filed date.
          // ****************************************************************
          local.InterstateSupportOrder.OrderFilingDate =
            entities.LegalAction.FiledDate;
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INTERSTATE_SUPPORT_ORDER_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_SUPPORT_ORDER_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    UseSiReadCaseProgramType();

    if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

      if (AsChar(local.NonArrears.Flag) == 'Y')
      {
        export.ArrearsOnly.CaseType = "N";
      }
      else if (local.InterstateSupportOrder.ArrearsAfdcAmount.
        GetValueOrDefault() > 0 || local
        .InterstateSupportOrder.MedicalAmount.GetValueOrDefault() > 0)
      {
        export.ArrearsOnly.CaseType = "R";
      }
      else if (local.InterstateSupportOrder.FosterCareAmount.
        GetValueOrDefault() > 0)
      {
        export.ArrearsOnly.CaseType = "C";
      }
      else if (local.InterstateSupportOrder.ArrearsNonAfdcAmount.
        GetValueOrDefault() > 0)
      {
        export.ArrearsOnly.CaseType = "N";
      }
    }
    else if (Equal(local.Program.Code, "AF") || Equal
      (local.Program.Code, "AFI"))
    {
      export.ArrearsOnly.CaseType = "A";
    }
    else if (Equal(local.Program.Code, "FC") || Equal
      (local.Program.Code, "FCI") || Equal(local.Program.Code, "NF"))
    {
      export.ArrearsOnly.CaseType = "F";
    }
    else if (Equal(local.Program.Code, "NA") || Equal
      (local.Program.Code, "NAI") || Equal(local.Program.Code, "MAI"))
    {
      export.ArrearsOnly.CaseType = "N";
    }

    if (local.Obligation.Count == 0)
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";
    }

    if (local.LegalActionDetail.Count == 0)
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF_RB";
    }
  }

  private static void MoveInterstateSupportOrder(InterstateSupportOrder source,
    InterstateSupportOrder target)
  {
    target.SystemGeneratedSequenceNum = source.SystemGeneratedSequenceNum;
    target.OrderFilingDate = source.OrderFilingDate;
    target.DebtType = source.DebtType;
    target.PaymentFreq = source.PaymentFreq;
    target.AmountOrdered = source.AmountOrdered;
    target.EffectiveDate = source.EffectiveDate;
    target.EndDate = source.EndDate;
    target.ArrearsFreq = source.ArrearsFreq;
    target.ArrearsFreqAmount = source.ArrearsFreqAmount;
    target.ArrearsTotalAmount = source.ArrearsTotalAmount;
    target.ArrearsAfdcFromDate = source.ArrearsAfdcFromDate;
    target.ArrearsAfdcThruDate = source.ArrearsAfdcThruDate;
    target.ArrearsAfdcAmount = source.ArrearsAfdcAmount;
    target.ArrearsNonAfdcFromDate = source.ArrearsNonAfdcFromDate;
    target.ArrearsNonAfdcThruDate = source.ArrearsNonAfdcThruDate;
    target.ArrearsNonAfdcAmount = source.ArrearsNonAfdcAmount;
    target.FosterCareFromDate = source.FosterCareFromDate;
    target.FosterCareThruDate = source.FosterCareThruDate;
    target.FosterCareAmount = source.FosterCareAmount;
    target.MedicalFromDate = source.MedicalFromDate;
    target.MedicalThruDate = source.MedicalThruDate;
    target.MedicalAmount = source.MedicalAmount;
    target.MedicalOrderedInd = source.MedicalOrderedInd;
    target.DateOfLastPayment = source.DateOfLastPayment;
    target.ControllingOrderFlag = source.ControllingOrderFlag;
    target.NewOrderFlag = source.NewOrderFlag;
    target.DocketNumber = source.DocketNumber;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private int UseCabGetYearMonthFromDate()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    useImport.SupportedPerson.Number = entities.CsePerson.Number;
    useImport.DebtDetail.Assign(entities.DebtDetail);

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Assign(useExport.Program);
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void CreateInterstateSupportOrder()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var systemGeneratedSequenceNum =
      local.InterstateSupportOrder.SystemGeneratedSequenceNum;
    var ccaTranSerNum = entities.InterstateCase.TransSerialNumber;
    var fipsState = local.InterstateSupportOrder.FipsState;
    var fipsCounty = local.InterstateSupportOrder.FipsCounty ?? "";
    var fipsLocation = local.InterstateSupportOrder.FipsLocation ?? "";
    var number = local.InterstateSupportOrder.Number;
    var orderFilingDate = local.InterstateSupportOrder.OrderFilingDate;
    var type1 = local.InterstateSupportOrder.Type1 ?? "";
    var debtType = local.InterstateSupportOrder.DebtType;
    var paymentFreq = local.InterstateSupportOrder.PaymentFreq ?? "";
    var amountOrdered =
      local.InterstateSupportOrder.AmountOrdered.GetValueOrDefault();
    var effectiveDate = local.InterstateSupportOrder.EffectiveDate;
    var endDate = local.InterstateSupportOrder.EndDate;
    var cancelDate = local.InterstateSupportOrder.CancelDate;
    var arrearsFreq = local.InterstateSupportOrder.ArrearsFreq ?? "";
    var arrearsFreqAmount =
      local.InterstateSupportOrder.ArrearsFreqAmount.GetValueOrDefault();
    var arrearsTotalAmount =
      local.InterstateSupportOrder.ArrearsTotalAmount.GetValueOrDefault();
    var arrearsAfdcFromDate = local.InterstateSupportOrder.ArrearsAfdcFromDate;
    var arrearsAfdcThruDate = local.InterstateSupportOrder.ArrearsAfdcThruDate;
    var arrearsAfdcAmount =
      local.InterstateSupportOrder.ArrearsAfdcAmount.GetValueOrDefault();
    var arrearsNonAfdcFromDate =
      local.InterstateSupportOrder.ArrearsNonAfdcFromDate;
    var arrearsNonAfdcThruDate =
      local.InterstateSupportOrder.ArrearsNonAfdcThruDate;
    var arrearsNonAfdcAmount =
      local.InterstateSupportOrder.ArrearsNonAfdcAmount.GetValueOrDefault();
    var fosterCareFromDate = local.InterstateSupportOrder.FosterCareFromDate;
    var fosterCareThruDate = local.InterstateSupportOrder.FosterCareThruDate;
    var fosterCareAmount =
      local.InterstateSupportOrder.FosterCareAmount.GetValueOrDefault();
    var medicalFromDate = local.InterstateSupportOrder.MedicalFromDate;
    var medicalThruDate = local.InterstateSupportOrder.MedicalThruDate;
    var medicalAmount =
      local.InterstateSupportOrder.MedicalAmount.GetValueOrDefault();
    var medicalOrderedInd = local.InterstateSupportOrder.MedicalOrderedInd ?? ""
      ;
    var tribunalCaseNumber =
      local.InterstateSupportOrder.TribunalCaseNumber ?? "";
    var dateOfLastPayment = local.InterstateSupportOrder.DateOfLastPayment;
    var controllingOrderFlag =
      local.InterstateSupportOrder.ControllingOrderFlag ?? "";
    var newOrderFlag = local.InterstateSupportOrder.NewOrderFlag ?? "";
    var docketNumber = local.InterstateSupportOrder.DocketNumber ?? "";

    entities.InterstateSupportOrder.Populated = false;
    Update("CreateInterstateSupportOrder",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt32(command, "sysGeneratedId", systemGeneratedSequenceNum);
        db.SetInt64(command, "ccaTranSerNum", ccaTranSerNum);
        db.SetString(command, "fipsState", fipsState);
        db.SetNullableString(command, "fipsCounty", fipsCounty);
        db.SetNullableString(command, "fipsLocation", fipsLocation);
        db.SetString(command, "number", number);
        db.SetDate(command, "orderFilingDate", orderFilingDate);
        db.SetNullableString(command, "type", type1);
        db.SetString(command, "debtType", debtType);
        db.SetNullableString(command, "paymentFreq", paymentFreq);
        db.SetNullableDecimal(command, "amountOrdered", amountOrdered);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableDate(command, "cancelDate", cancelDate);
        db.SetNullableString(command, "arrearsFreq", arrearsFreq);
        db.SetNullableDecimal(command, "arrearsFrqAmt", arrearsFreqAmount);
        db.SetNullableDecimal(command, "arrearsTotalAmt", arrearsTotalAmount);
        db.SetNullableDate(command, "arrsAfdcFromDte", arrearsAfdcFromDate);
        db.SetNullableDate(command, "arrsAfdcThruDte", arrearsAfdcThruDate);
        db.SetNullableDecimal(command, "arrearsAfdcAmt", arrearsAfdcAmount);
        db.SetNullableDate(command, "arrNafdcFromDte", arrearsNonAfdcFromDate);
        db.SetNullableDate(command, "arrNafdcThruDte", arrearsNonAfdcThruDate);
        db.SetNullableDecimal(command, "arrNafdcAmt", arrearsNonAfdcAmount);
        db.SetNullableDate(command, "fostCareFromDte", fosterCareFromDate);
        db.SetNullableDate(command, "fostCareThruDte", fosterCareThruDate);
        db.SetNullableDecimal(command, "fosterCareAmount", fosterCareAmount);
        db.SetNullableDate(command, "medicalFromDate", medicalFromDate);
        db.SetNullableDate(command, "medicalThruDate", medicalThruDate);
        db.SetNullableDecimal(command, "medicalAmount", medicalAmount);
        db.SetNullableString(command, "medicalOrderedIn", medicalOrderedInd);
        db.SetNullableString(command, "tribunalCaseNum", tribunalCaseNumber);
        db.SetNullableDate(command, "dateOfLastPay", dateOfLastPayment);
        db.SetNullableString(command, "cntrlOrderFlag", controllingOrderFlag);
        db.SetNullableString(command, "newOrderFlag", newOrderFlag);
        db.SetNullableString(command, "docketNumber", docketNumber);
        db.SetNullableInt32(command, "legalActionId", 0);
      });

    entities.InterstateSupportOrder.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateSupportOrder.SystemGeneratedSequenceNum =
      systemGeneratedSequenceNum;
    entities.InterstateSupportOrder.CcaTranSerNum = ccaTranSerNum;
    entities.InterstateSupportOrder.FipsState = fipsState;
    entities.InterstateSupportOrder.FipsCounty = fipsCounty;
    entities.InterstateSupportOrder.FipsLocation = fipsLocation;
    entities.InterstateSupportOrder.Number = number;
    entities.InterstateSupportOrder.OrderFilingDate = orderFilingDate;
    entities.InterstateSupportOrder.Type1 = type1;
    entities.InterstateSupportOrder.DebtType = debtType;
    entities.InterstateSupportOrder.PaymentFreq = paymentFreq;
    entities.InterstateSupportOrder.AmountOrdered = amountOrdered;
    entities.InterstateSupportOrder.EffectiveDate = effectiveDate;
    entities.InterstateSupportOrder.EndDate = endDate;
    entities.InterstateSupportOrder.CancelDate = cancelDate;
    entities.InterstateSupportOrder.ArrearsFreq = arrearsFreq;
    entities.InterstateSupportOrder.ArrearsFreqAmount = arrearsFreqAmount;
    entities.InterstateSupportOrder.ArrearsTotalAmount = arrearsTotalAmount;
    entities.InterstateSupportOrder.ArrearsAfdcFromDate = arrearsAfdcFromDate;
    entities.InterstateSupportOrder.ArrearsAfdcThruDate = arrearsAfdcThruDate;
    entities.InterstateSupportOrder.ArrearsAfdcAmount = arrearsAfdcAmount;
    entities.InterstateSupportOrder.ArrearsNonAfdcFromDate =
      arrearsNonAfdcFromDate;
    entities.InterstateSupportOrder.ArrearsNonAfdcThruDate =
      arrearsNonAfdcThruDate;
    entities.InterstateSupportOrder.ArrearsNonAfdcAmount = arrearsNonAfdcAmount;
    entities.InterstateSupportOrder.FosterCareFromDate = fosterCareFromDate;
    entities.InterstateSupportOrder.FosterCareThruDate = fosterCareThruDate;
    entities.InterstateSupportOrder.FosterCareAmount = fosterCareAmount;
    entities.InterstateSupportOrder.MedicalFromDate = medicalFromDate;
    entities.InterstateSupportOrder.MedicalThruDate = medicalThruDate;
    entities.InterstateSupportOrder.MedicalAmount = medicalAmount;
    entities.InterstateSupportOrder.MedicalOrderedInd = medicalOrderedInd;
    entities.InterstateSupportOrder.TribunalCaseNumber = tribunalCaseNumber;
    entities.InterstateSupportOrder.DateOfLastPayment = dateOfLastPayment;
    entities.InterstateSupportOrder.ControllingOrderFlag = controllingOrderFlag;
    entities.InterstateSupportOrder.NewOrderFlag = newOrderFlag;
    entities.InterstateSupportOrder.DocketNumber = docketNumber;
    entities.InterstateSupportOrder.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ksCaseId", import.InterstateCase.KsCaseId ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadDebtCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Debt.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadDebtCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.CsePerson.Type1 = db.GetString(reader, 9);
        entities.Debt.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 10);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal1.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal1.State = db.GetInt32(reader, 0);
        entities.Tribunal1.County = db.GetInt32(reader, 1);
        entities.Tribunal1.Location = db.GetInt32(reader, 2);
        entities.Tribunal1.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.CaseType = db.GetString(reader, 2);
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadInterstateSupportOrder()
  {
    entities.InterstateSupportOrder.Populated = false;

    return Read("ReadInterstateSupportOrder",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTranSerNum", entities.InterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.InterstateSupportOrder.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateSupportOrder.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateSupportOrder.CcaTranSerNum = db.GetInt64(reader, 2);
        entities.InterstateSupportOrder.FipsState = db.GetString(reader, 3);
        entities.InterstateSupportOrder.FipsCounty =
          db.GetNullableString(reader, 4);
        entities.InterstateSupportOrder.FipsLocation =
          db.GetNullableString(reader, 5);
        entities.InterstateSupportOrder.Number = db.GetString(reader, 6);
        entities.InterstateSupportOrder.OrderFilingDate = db.GetDate(reader, 7);
        entities.InterstateSupportOrder.Type1 = db.GetNullableString(reader, 8);
        entities.InterstateSupportOrder.DebtType = db.GetString(reader, 9);
        entities.InterstateSupportOrder.PaymentFreq =
          db.GetNullableString(reader, 10);
        entities.InterstateSupportOrder.AmountOrdered =
          db.GetNullableDecimal(reader, 11);
        entities.InterstateSupportOrder.EffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.InterstateSupportOrder.EndDate =
          db.GetNullableDate(reader, 13);
        entities.InterstateSupportOrder.CancelDate =
          db.GetNullableDate(reader, 14);
        entities.InterstateSupportOrder.ArrearsFreq =
          db.GetNullableString(reader, 15);
        entities.InterstateSupportOrder.ArrearsFreqAmount =
          db.GetNullableDecimal(reader, 16);
        entities.InterstateSupportOrder.ArrearsTotalAmount =
          db.GetNullableDecimal(reader, 17);
        entities.InterstateSupportOrder.ArrearsAfdcFromDate =
          db.GetNullableDate(reader, 18);
        entities.InterstateSupportOrder.ArrearsAfdcThruDate =
          db.GetNullableDate(reader, 19);
        entities.InterstateSupportOrder.ArrearsAfdcAmount =
          db.GetNullableDecimal(reader, 20);
        entities.InterstateSupportOrder.ArrearsNonAfdcFromDate =
          db.GetNullableDate(reader, 21);
        entities.InterstateSupportOrder.ArrearsNonAfdcThruDate =
          db.GetNullableDate(reader, 22);
        entities.InterstateSupportOrder.ArrearsNonAfdcAmount =
          db.GetNullableDecimal(reader, 23);
        entities.InterstateSupportOrder.FosterCareFromDate =
          db.GetNullableDate(reader, 24);
        entities.InterstateSupportOrder.FosterCareThruDate =
          db.GetNullableDate(reader, 25);
        entities.InterstateSupportOrder.FosterCareAmount =
          db.GetNullableDecimal(reader, 26);
        entities.InterstateSupportOrder.MedicalFromDate =
          db.GetNullableDate(reader, 27);
        entities.InterstateSupportOrder.MedicalThruDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateSupportOrder.MedicalAmount =
          db.GetNullableDecimal(reader, 29);
        entities.InterstateSupportOrder.MedicalOrderedInd =
          db.GetNullableString(reader, 30);
        entities.InterstateSupportOrder.TribunalCaseNumber =
          db.GetNullableString(reader, 31);
        entities.InterstateSupportOrder.DateOfLastPayment =
          db.GetNullableDate(reader, 32);
        entities.InterstateSupportOrder.ControllingOrderFlag =
          db.GetNullableString(reader, 33);
        entities.InterstateSupportOrder.NewOrderFlag =
          db.GetNullableString(reader, 34);
        entities.InterstateSupportOrder.DocketNumber =
          db.GetNullableString(reader, 35);
        entities.InterstateSupportOrder.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.OrderAuthority = db.GetString(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.DismissalCode = db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
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
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 8);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 9);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.LastCollDt = db.GetNullableDate(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 7);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 8);
        entities.ObligationType.Code = db.GetString(reader, 9);
        entities.ObligationType.Classification = db.GetString(reader, 10);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
        db.SetDate(command, "startDt", date);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 5);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 6);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
      });
  }

  private bool ReadPersonProgram1()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram3()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram4()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram5()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram6()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
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
    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private CsePersonsWorkSet csePersonsWorkSet;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// Gets a value of Participant.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantGroup> Participant => participant ??= new(
      ParticipantGroup.Capacity);

    /// <summary>
    /// Gets a value of Participant for json serialization.
    /// </summary>
    [JsonPropertyName("participant")]
    [Computed]
    public IList<ParticipantGroup> Participant_Json
    {
      get => participant;
      set => Participant.Assign(value);
    }

    private LegalAction legalAction;
    private InterstateCase interstateCase;
    private Array<ParticipantGroup> participant;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ArrearsOnly.
    /// </summary>
    [JsonPropertyName("arrearsOnly")]
    public InterstateCase ArrearsOnly
    {
      get => arrearsOnly ??= new();
      set => arrearsOnly = value;
    }

    /// <summary>
    /// A value of OrderDbCreated.
    /// </summary>
    [JsonPropertyName("orderDbCreated")]
    public Common OrderDbCreated
    {
      get => orderDbCreated ??= new();
      set => orderDbCreated = value;
    }

    private InterstateCase arrearsOnly;
    private Common orderDbCreated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Common Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of DebtExists.
    /// </summary>
    [JsonPropertyName("debtExists")]
    public Common DebtExists
    {
      get => debtExists ??= new();
      set => debtExists = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public InterstateSupportOrder Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of CurrentMonth.
    /// </summary>
    [JsonPropertyName("currentMonth")]
    public DateWorkArea CurrentMonth
    {
      get => currentMonth ??= new();
      set => currentMonth = value;
    }

    /// <summary>
    /// A value of NonAfdcArrears.
    /// </summary>
    [JsonPropertyName("nonAfdcArrears")]
    public Common NonAfdcArrears
    {
      get => nonAfdcArrears ??= new();
      set => nonAfdcArrears = value;
    }

    /// <summary>
    /// A value of NonArrears.
    /// </summary>
    [JsonPropertyName("nonArrears")]
    public Common NonArrears
    {
      get => nonArrears ??= new();
      set => nonArrears = value;
    }

    /// <summary>
    /// A value of AfdcArrearsOnly.
    /// </summary>
    [JsonPropertyName("afdcArrearsOnly")]
    public Common AfdcArrearsOnly
    {
      get => afdcArrearsOnly ??= new();
      set => afdcArrearsOnly = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public Common LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    private Common obligation;
    private Common debtExists;
    private InterstateSupportOrder blank;
    private DateWorkArea currentMonth;
    private Common nonAfdcArrears;
    private Common nonArrears;
    private Common afdcArrearsOnly;
    private Program program;
    private Common legalActionDetail;
    private DateWorkArea current;
    private InterstateSupportOrder interstateSupportOrder;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    /// <summary>
    /// A value of Tribunal1.
    /// </summary>
    [JsonPropertyName("tribunal1")]
    public Fips Tribunal1
    {
      get => tribunal1 ??= new();
      set => tribunal1 = value;
    }

    /// <summary>
    /// A value of Tribunal2.
    /// </summary>
    [JsonPropertyName("tribunal2")]
    public Tribunal Tribunal2
    {
      get => tribunal2 ??= new();
      set => tribunal2 = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private Case1 case1;
    private Program program;
    private PersonProgram personProgram;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePersonAccount supported;
    private ObligationTransaction debt;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePerson csePerson;
    private DebtDetail debtDetail;
    private InterstateSupportOrder interstateSupportOrder;
    private Fips tribunal1;
    private Tribunal tribunal2;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private InterstateCase interstateCase;
  }
#endregion
}
