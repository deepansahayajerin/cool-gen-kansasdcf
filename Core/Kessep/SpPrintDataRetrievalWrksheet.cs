// Program: SP_PRINT_DATA_RETRIEVAL_WRKSHEET, ID: 372132892, model: 746.
// Short name: SWE02276
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_PRINT_DATA_RETRIEVAL_WRKSHEET.
/// </summary>
[Serializable]
public partial class SpPrintDataRetrievalWrksheet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DATA_RETRIEVAL_WRKSHEET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDataRetrievalWrksheet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDataRetrievalWrksheet.
  /// </summary>
  public SpPrintDataRetrievalWrksheet(IContext context, Import import,
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
    // ----------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------
    // 12/16/1998	M Ramirez	Initial Development
    // 07/14/1999	M Ramirez	Added row lock counts
    // 11/27/2007      M. Fan          WR318566(CQ297)- Added the parenting time
    // adjustment precent attribite to locals
    // 				for the child support worksheet entity. Also changed the specific 
    // child
    // 				 age group names to generic names for three local oblig work views
    // 				(00_06, 07_15 and 16_18 were change to agrp1, agrp2 and
    // 				agrp3 respectively.
    // 06/04/10	J. Huss		CQ# 18769 - Reduced group_local_worksheet size from 7 
    // to 6.
    // ----------------------------------------------------------------------
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 04/10/2012  Raj S              CQ33264     Modified to fix worksheet 
    // document print *
    // *
    // 
    // issue by adding CS guideline year        *
    // *
    // 
    // *
    // * 12/09/2015  GVandy             CQ50299     Add support for new fields 
    // WRKAFRSTNM    *
    // *
    // 
    // and WRKBFRSTNM                           *
    // ***************************************************************************************
    MoveFieldValue2(import.FieldValue, local.FieldValue);
    local.ChildSupportWorksheet.Identifier = import.SpDocKey.KeyWorksheet;
    UseOeWorkReadChildSupWorksheet();

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
    {
      ExitState = "ACO_NN0000_ALL_OK";

      // mjr---> Worksheet not found, but no message is given.
      return;
    }

    // mjr---> Call oe_work_calc_child_sup_worksheet for the calculated values
    UseOeWorkCalcChildSupWorksheet();

    if (!IsExitState("OE0000_CALCULATE_SUCCESSFUL"))
    {
      ExitState = "ACO_NN0000_ALL_OK";

      // mjr---> Some calculation error.  Worksheet information is not 
      // available.
      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    foreach(var item in ReadField())
    {
      if (Lt(entities.Field.SubroutineName, local.Previous.SubroutineName) || Equal
        (entities.Field.SubroutineName, local.Previous.SubroutineName) && !
        Lt(local.Previous.Name, entities.Field.Name))
      {
        continue;
      }

      local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
      local.ProcessGroup.Flag = "N";
      local.Current.Name = "";
      local.CurrentParentCommon.Flag = "";
      MoveField(entities.Field, local.Previous);

      switch(TrimEnd(entities.Field.SubroutineName))
      {
        case "PARENT":
          local.Current.Name = entities.Field.Name;
          local.Temp.Name = Substring(entities.Field.Name, 1, 3);

          if (Equal(local.Temp.Name, "WRK"))
          {
            local.Current.Name = "*" + Substring
              (entities.Field.Name, Field.Name_MaxLength, 5, 6);
            local.CurrentParentCommon.Flag =
              Substring(entities.Field.Name, 4, 1);
          }

          // mjr---> Set subscript based on field name.
          // 	(Each occurence in RGV has one adjustment.)
          switch(TrimEnd(local.Current.Name))
          {
            case "*TRAVEL":
              local.Worksheet.Index = 0;
              local.Worksheet.CheckSize();

              break;
            case "*VISIT":
              local.Worksheet.Index = 1;
              local.Worksheet.CheckSize();

              break;
            case "*TAXCR":
              if (local.ChildSupportWorksheet.CsGuidelineYear >= 2020)
              {
                local.Worksheet.Index = 1;
                local.Worksheet.CheckSize();
              }
              else
              {
                local.Worksheet.Index = 2;
                local.Worksheet.CheckSize();
              }

              break;
            case "*SPECL":
              if (local.ChildSupportWorksheet.CsGuidelineYear >= 2020)
              {
                local.Worksheet.Index = 2;
                local.Worksheet.CheckSize();
              }
              else
              {
                local.Worksheet.Index = 3;
                local.Worksheet.CheckSize();
              }

              break;
            case "*18PLUS":
              if (local.ChildSupportWorksheet.CsGuidelineYear >= 2020)
              {
                local.Worksheet.Index = 3;
                local.Worksheet.CheckSize();
              }
              else
              {
                local.Worksheet.Index = 4;
                local.Worksheet.CheckSize();
              }

              break;
            case "*OVRALL":
              if (local.ChildSupportWorksheet.CsGuidelineYear >= 2020)
              {
                local.Worksheet.Index = 4;
                local.Worksheet.CheckSize();
              }
              else
              {
                local.Worksheet.Index = 5;
                local.Worksheet.CheckSize();
              }

              break;
            default:
              // mjr---> Field is not in RGV  (Not an adjustment.)
              break;
          }

          switch(TrimEnd(local.Current.Name))
          {
            case "*ADJOBL":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAF3AdjCsOblig.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBF3AdjCsOblig.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*BASCS":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD13ParABasicChSup.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD13ParBBasicChSup.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }

              break;
            case "*CINSCC":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    InsuranceWorkRelatedCcCredit.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    InsuranceWorkRelatedCcCredit.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*COCSPD":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    CourtOrderedChildSupportPaid.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    CourtOrderedChildSupportPaid.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*COMTPD":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    CourtOrderedMaintenancePaid.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    CourtOrderedMaintenancePaid.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*COMTRC":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    CourtOrderedMaintenanceRecvd.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    CourtOrderedMaintenanceRecvd.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*CSOBL":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAD7CsOblig.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBD7CsOblig.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*DMSEGI":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAB3SeGrossInc.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBB3SeGrossInc.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*ENFFEE":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAEnfFee.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBEnfFee.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*EQPTOB":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    EqualParentingTimeObligation.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    EqualParentingTimeObligation.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*FNLOBL":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAF5NetCs.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBF5NetCs.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*FRSTNM":
              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.FieldValue.Value = UseOeWorkCenterFirstName2();
              }
              else
              {
                local.FieldValue.Value = UseOeWorkCenterFirstName1();
              }

              break;
            case "*FSUBT":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalF6BParAFinaSubtotal.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalF6BParBFinaSubtotal.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }

              break;
            case "*GI":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    WageEarnerGrossIncome.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    WageEarnerGrossIncome.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*INSCC":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAD8Adjustments.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBD8Adjustments.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*INSPRM":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    HealthAndDentalInsurancePrem.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    HealthAndDentalInsurancePrem.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*NAME":
              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.SpPrintWorkSet.FirstName =
                  local.ParentACsePersonsWorkSet.FirstName;
                local.SpPrintWorkSet.MidInitial =
                  local.ParentACsePersonsWorkSet.MiddleInitial;
                local.SpPrintWorkSet.LastName =
                  local.ParentACsePersonsWorkSet.LastName;
                local.FieldValue.Value = UseSpDocFormatName();
              }
              else
              {
                local.SpPrintWorkSet.FirstName =
                  local.ParentBCsePersonsWorkSet.FirstName;
                local.SpPrintWorkSet.MidInitial =
                  local.ParentBCsePersonsWorkSet.MiddleInitial;
                local.SpPrintWorkSet.LastName =
                  local.ParentBCsePersonsWorkSet.LastName;
                local.FieldValue.Value = UseSpDocFormatName();
              }

              break;
            case "*NETOBL":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAD9F1NetCs.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBD9F1NetCs.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*NCSOB":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalF8ParANetCsOblig.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalF8ParBNetCsOblig.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }

              break;
            case "*OVRALL":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentA.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentB.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }

              break;
            case "*PRCNTI":
              // mjr---> Calculated value
              local.Current.Name = "*PERCENT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAD2PercentInc.TotalCurrency *
                  10, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBD2PercentInc.TotalCurrency *
                  10, MidpointRounding.AwayFromZero);
              }

              break;
            case "*PSAPTA":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD6ParAPsAfterPat.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD6ParBPsAfterPat.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }

              break;
            case "*PSCSOB":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD11ParAPropShrCcob.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD11ParBPropShrCcob.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }

              break;
            case "*PSHIP":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD8ParAPropShrHip.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD8ParBPropShrHip.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }

              break;
            case "*PSWRCC":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD10ParAPropShrWrcc.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD10ParBPropShrWrcc.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }

              break;
            case "*PRSH":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD4ParAPropShare.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalD4ParBPropShare.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }

              break;
            case "*RBEXP":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    ReasonableBusinessExpense.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    ReasonableBusinessExpense.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*SEGI":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    SelfEmploymentGrossIncome.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    SelfEmploymentGrossIncome.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*SPECL":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentA.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentB.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }

              break;
            case "*SSDB":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    SocialSecDependentBenefit.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    SocialSecDependentBenefit.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*SUBT":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalF5BParASubtotal.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020Calculations.GlocalF5BParBSubtotal.
                    TotalCurrency, MidpointRounding.AwayFromZero);
              }

              break;
            case "*TAXCR":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentA.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentB.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }

              break;
            case "*TOTADJ":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";

              if (local.ChildSupportWorksheet.CsGuidelineYear >= 2020)
              {
                // --New for 2020 worksheet
                if (AsChar(local.CurrentParentCommon.Flag) == 'A')
                {
                  local.BatchConvertNumToText.Number15 =
                    (long)Math.Round(
                      local.ParentAF2TotalCsAdj.TotalCurrency,
                    MidpointRounding.AwayFromZero);
                }
                else
                {
                  local.BatchConvertNumToText.Number15 =
                    (long)Math.Round(
                      local.ParentBF2TotalCsAdj.TotalCurrency,
                    MidpointRounding.AwayFromZero);
                }
              }
              else if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAE7F2TotAdj.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBE7F2TotAdj.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*TOTCSI":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAC5D1TotCsInc.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBC5D1TotCsInc.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*TOTGI":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentAC1TotGrossInc.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBC1TotGrossInc.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*TRAVEL":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentA.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentB.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }

              break;
            case "*VISIT":
              local.Current.Name = "*AMOUNT";

              if (local.ChildSupportWorksheet.CsGuidelineYear >= 2020)
              {
                // --New for 2020 worksheet
                if (AsChar(local.CurrentParentCommon.Flag) == 'A')
                {
                  local.BatchConvertNumToText.Number15 =
                    (long)Math.Round(
                      local.Glocal2020Calculations.GlocalD5ParAParentTimAdj.
                      TotalCurrency, MidpointRounding.AwayFromZero);
                }
                else
                {
                  local.BatchConvertNumToText.Number15 =
                    (long)Math.Round(
                      local.Glocal2020Calculations.GlocalD5ParBParentTimAdj.
                      TotalCurrency, MidpointRounding.AwayFromZero);
                }
              }
              else if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentA.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentB.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }

              break;
            case "*WRCC":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentACsePersonSupportWorksheet.
                    WorkRelatedChildCareCosts.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.ParentBCsePersonSupportWorksheet.
                    WorkRelatedChildCareCosts.GetValueOrDefault(),
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "*18PLUS":
              local.Current.Name = "*AMOUNT";

              if (AsChar(local.CurrentParentCommon.Flag) == 'A')
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentA.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }
              else
              {
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Worksheet.Item.GlocalParentB.AdjustmentAmount.
                    GetValueOrDefault(), MidpointRounding.AwayFromZero);
              }

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "WRKSHEET":
          switch(TrimEnd(entities.Field.Name))
          {
            case "WRKABTOPY":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.Glocal2020Calculations.GlocalF5A3AbilityToPay.
                  TotalCurrency, MidpointRounding.AwayFromZero);

              break;
            case "WRKPADJPCT":
              // 11/27/2007  M. Fan WR318566(CQ297)- Case WRKPADJPCT Added for 
              // the new field of parenting time adjustment percent.
              if (local.ChildSupportWorksheet.ParentingTimeAdjPercent.
                GetValueOrDefault() > 0)
              {
                if (local.ChildSupportWorksheet.ParentingTimeAdjPercent.
                  GetValueOrDefault() > 99)
                {
                  local.FieldValue.Value =
                    NumberToString(local.ChildSupportWorksheet.
                      ParentingTimeAdjPercent.GetValueOrDefault(), 13, 3);
                }
                else if (local.ChildSupportWorksheet.ParentingTimeAdjPercent.
                  GetValueOrDefault() > 9)
                {
                  local.FieldValue.Value =
                    NumberToString(local.ChildSupportWorksheet.
                      ParentingTimeAdjPercent.GetValueOrDefault(), 14, 2);
                }
                else
                {
                  local.FieldValue.Value =
                    NumberToString(local.ChildSupportWorksheet.
                      ParentingTimeAdjPercent.GetValueOrDefault(), 15, 1);
                }
              }

              break;
            case "WRKCH0006":
              local.FieldValue.Value =
                NumberToString(local.ChildSupportWorksheet.
                  NoOfChildrenInAgeGrp1.GetValueOrDefault(), 15, 1);

              break;
            case "WRKCH0715":
              local.FieldValue.Value =
                NumberToString(local.ChildSupportWorksheet.
                  NoOfChildrenInAgeGrp2.GetValueOrDefault(), 15, 1);

              break;
            case "WRKCH1618":
              local.FieldValue.Value =
                NumberToString(local.ChildSupportWorksheet.
                  NoOfChildrenInAgeGrp3.GetValueOrDefault(), 15, 1);

              break;
            case "WRKCOLADJ":
              if (AsChar(local.ChildSupportWorksheet.CostOfLivingDiffAdjInd) ==
                'Y')
              {
                local.FieldValue.Value = "YES";
              }
              else
              {
                local.FieldValue.Value = "NO";
              }

              break;
            case "WRKCOLDAY":
              // --New for 2020 worksheet
              if (AsChar(local.ChildSupportWorksheet.CostOfLivingDiffAdjInd) ==
                'Y')
              {
                local.FieldValue.Value = "X";
              }
              else
              {
                local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              }

              break;
            case "WRKCOLDAN":
              // --New for 2020 worksheet
              if (AsChar(local.ChildSupportWorksheet.CostOfLivingDiffAdjInd) ==
                'N')
              {
                local.FieldValue.Value = "X";
              }
              else
              {
                local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              }

              break;
            case "WRKENFFLAT":
              // mjr---> Calculated value
              if (local.EnforcementFee.TotalCurrency > 0)
              {
                local.Current.Name = "*AMOUNT";
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.EnforcementFee.TotalCurrency,
                  MidpointRounding.AwayFromZero);
              }

              break;
            case "WRKENFPCNT":
              // mjr---> Calculated value
              if (local.EnforcementFee.Percentage > 0)
              {
                local.Current.Name = "*PERCENT";
                local.BatchConvertNumToText.Number15 =
                  local.EnforcementFee.Percentage * 10;
              }

              break;
            case "WRKIBCSSY":
              // --New for 2020 worksheet
              if (AsChar(local.IncomeBeyondSchedule.Flag) == 'Y')
              {
                local.FieldValue.Value = "X";
              }
              else
              {
                local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              }

              break;
            case "WRKIBCSSN":
              // --New for 2020 worksheet
              if (AsChar(local.IncomeBeyondSchedule.Flag) == 'Y')
              {
                local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              }
              else
              {
                local.FieldValue.Value = "X";
              }

              break;
            case "WRKMFAIY":
              // --New for 2020 worksheet
              if (AsChar(local.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y'
                )
              {
                local.FieldValue.Value = "X";
              }
              else
              {
                local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              }

              break;
            case "WRKMFAIN":
              // --New for 2020 worksheet
              if (AsChar(local.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'N'
                )
              {
                local.FieldValue.Value = "X";
              }
              else
              {
                local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              }

              break;
            case "WRKMULTADJ":
              if (AsChar(local.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y'
                )
              {
                local.FieldValue.Value = "YES";
              }
              else
              {
                local.FieldValue.Value = "NO";
              }

              break;
            case "WRKOBLTOT":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.CsObligTotalAmount.TotalCurrency,
                MidpointRounding.AwayFromZero);

              break;
            case "WRKOBL0006":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.CsObligAgrp1TotalAmt.TotalCurrency,
                MidpointRounding.AwayFromZero);

              break;
            case "WRKOBL0715":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.CsObligAgrp2TotalAmt.TotalCurrency,
                MidpointRounding.AwayFromZero);

              break;
            case "WRKOBL1618":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.CsObligAgrp3TotalAmt.TotalCurrency,
                MidpointRounding.AwayFromZero);

              break;
            case "WRKPARATOP":
              // --New for 2020 worksheet
              local.FieldValue.Value =
                local.Glocal2020EnterableFields.GlocalAbilityToPayParent.
                  SelectChar;

              break;
            case "WRKPARINC":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.Glocal2020Calculations.GlocalF5A1CsIncome.TotalCurrency,
                MidpointRounding.AwayFromZero);

              break;
            case "WRKPOVLVL":
              // --New for 2020 worksheet
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.Glocal2020Calculations.GlocalF5A2PovertyLevel.
                  TotalCurrency, MidpointRounding.AwayFromZero);

              break;
            case "WRKPTAY":
              // --New for 2020 worksheet
              if (IsEmpty(local.Glocal2020EnterableFields.GlocalParentingTime.
                SelectChar))
              {
                local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              }
              else
              {
                local.FieldValue.Value = "X";
              }

              break;
            case "WRKPTAN":
              // --New for 2020 worksheet
              if (IsEmpty(local.Glocal2020EnterableFields.GlocalParentingTime.
                SelectChar))
              {
                local.FieldValue.Value = "X";
              }
              else
              {
                local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              }

              break;
            case "WRKPTAP":
              // --New for 2020 worksheet
              if (local.Glocal2020EnterableFields.GlocalParentingTime.
                TotalCurrency > 0)
              {
                local.Current.Name = "*PERCENT";
                local.BatchConvertNumToText.Number15 =
                  (long)Math.Round(
                    local.Glocal2020EnterableFields.GlocalParentingTime.
                    TotalCurrency * 10, MidpointRounding.AwayFromZero);
              }

              break;
            case "WRKTINSPRM":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.TotalInsurancePrem.TotalCurrency,
                MidpointRounding.AwayFromZero);

              break;
            case "WRKTOTCSI":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.TotalD1CsInc.TotalCurrency,
                MidpointRounding.AwayFromZero);

              break;
            case "WRKTOTOBL":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.TotalD6ChildSuppOblig.TotalCurrency,
                MidpointRounding.AwayFromZero);

              break;
            case "WRKTOTWRCC":
              // mjr---> Calculated value
              local.Current.Name = "*AMOUNT";
              local.BatchConvertNumToText.Number15 =
                (long)Math.Round(
                  local.TotalChildCareCost.TotalCurrency,
                MidpointRounding.AwayFromZero);

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        default:
          export.ErrorDocumentField.ScreenPrompt = "Invalid Subroutine";
          export.ErrorFieldValue.Value = "Field:  " + TrimEnd
            (entities.Field.Name) + ",  Subroutine:  " + entities
            .Field.SubroutineName;
          ExitState = "FIELD_NF";

          break;
      }

      if (!IsEmpty(export.ErrorDocumentField.ScreenPrompt) || !
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsEmpty(export.ErrorDocumentField.ScreenPrompt))
        {
          export.ErrorDocumentField.ScreenPrompt = "Processing Error";
          export.ErrorFieldValue.Value = "Field:  " + TrimEnd
            (entities.Field.Name) + ",  Person:  " + local
            .Local1.Item.GcsePersonsWorkSet.Number;
        }

        return;
      }

      switch(TrimEnd(local.Current.Name))
      {
        case "*AMOUNT":
          if (local.BatchConvertNumToText.Number15 == 0)
          {
            local.FieldValue.Value = "0";
          }
          else
          {
            local.BatchConvertNumToText.TextNumber15 =
              NumberToString(local.BatchConvertNumToText.Number15, 15);
            local.Position.Count =
              Verify(local.BatchConvertNumToText.TextNumber15, "0");
            local.FieldValue.Value =
              Substring(local.BatchConvertNumToText.TextNumber15,
              BatchConvertNumToText.TextNumber15_MaxLength,
              local.Position.Count, 16 - local.Position.Count);

            if (local.BatchConvertNumToText.Number15 < 0)
            {
              local.FieldValue.Value = "-" + (local.FieldValue.Value ?? "");
            }
          }

          local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + ".00";

          break;
        case "*PERCENT":
          if (local.BatchConvertNumToText.Number15 == 0)
          {
            local.FieldValue.Value = "0.0";
          }
          else
          {
            local.BatchConvertNumToText.TextNumber15 =
              NumberToString(local.BatchConvertNumToText.Number15, 15);
            local.Position.Count =
              Verify(local.BatchConvertNumToText.TextNumber15, "0");
            local.FieldValue.Value =
              Substring(local.BatchConvertNumToText.TextNumber15,
              BatchConvertNumToText.TextNumber15_MaxLength,
              local.Position.Count, 15 - local.Position.Count) + "." + Substring
              (local.BatchConvertNumToText.TextNumber15,
              BatchConvertNumToText.TextNumber15_MaxLength, 15, 1);
          }

          break;
        default:
          // mjr---> Field is not an amount nor percent
          break;
      }

      if (AsChar(local.ProcessGroup.Flag) == 'A')
      {
        // mjr
        // ----------------------------------------------
        // Field is an address
        //    Process 1-5 of group_local
        // -------------------------------------------------
        local.Position.Count = Length(TrimEnd(entities.Field.Name));
        local.CurrentParentField.Name =
          Substring(entities.Field.Name, 1, local.Position.Count - 1);

        for(local.Address.Index = 0; local.Address.Index < local.Address.Count; ++
          local.Address.Index)
        {
          if (!local.Address.CheckSize())
          {
            break;
          }

          // mjr---> Increment Field Name
          local.Temp.Name = NumberToString(local.Address.Index + 1, 10);
          local.Position.Count = Verify(local.Temp.Name, "0");
          local.Temp.Name =
            Substring(local.Temp.Name, local.Position.Count, 16 -
            local.Position.Count);
          local.Current.Name = TrimEnd(local.CurrentParentField.Name) + local
            .Temp.Name;
          UseSpCabCreateUpdateFieldValue2();

          if (IsExitState("DOCUMENT_FIELD_NF_RB"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.ErrorDocumentField.ScreenPrompt = "Creation Error";
            export.ErrorFieldValue.Value = "Field:  " + local.Current.Name;

            return;
          }

          local.Address.Update.GlocalAddress.Value =
            Spaces(FieldValue.Value_MaxLength);
          ++import.ExpImpRowLockFieldValue.Count;
        }

        local.Address.CheckIndex();
      }
      else
      {
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
        UseSpCabCreateUpdateFieldValue1();

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
      }

      // mjr
      // -----------------------------------------------------------
      // set Previous Field to skip the rest of the group, if applicable.
      // --------------------------------------------------------------
      // mjr----> not applicable
    }
  }

  private static void MoveChildSupportWorksheet(ChildSupportWorksheet source,
    ChildSupportWorksheet target)
  {
    target.NoOfChildrenInAgeGrp3 = source.NoOfChildrenInAgeGrp3;
    target.NoOfChildrenInAgeGrp2 = source.NoOfChildrenInAgeGrp2;
    target.NoOfChildrenInAgeGrp1 = source.NoOfChildrenInAgeGrp1;
    target.AdditionalNoOfChildren = source.AdditionalNoOfChildren;
    target.Status = source.Status;
    target.CostOfLivingDiffAdjInd = source.CostOfLivingDiffAdjInd;
    target.Identifier = source.Identifier;
    target.MultipleFamilyAdjInd = source.MultipleFamilyAdjInd;
    target.CsGuidelineYear = source.CsGuidelineYear;
  }

  private static void MoveCsePersonSupportWorksheet1(
    CsePersonSupportWorksheet source, CsePersonSupportWorksheet target)
  {
    target.NoOfChildrenInDayCare = source.NoOfChildrenInDayCare;
    target.WorkRelatedChildCareCosts = source.WorkRelatedChildCareCosts;
    target.Identifer = source.Identifer;
    target.WageEarnerGrossIncome = source.WageEarnerGrossIncome;
    target.SelfEmploymentGrossIncome = source.SelfEmploymentGrossIncome;
    target.ReasonableBusinessExpense = source.ReasonableBusinessExpense;
    target.CourtOrderedChildSupportPaid = source.CourtOrderedChildSupportPaid;
    target.ChildSupprtPaidCourtOrderNo = source.ChildSupprtPaidCourtOrderNo;
    target.CourtOrderedMaintenancePaid = source.CourtOrderedMaintenancePaid;
    target.MaintenancePaidCourtOrderNo = source.MaintenancePaidCourtOrderNo;
    target.CourtOrderedMaintenanceRecvd = source.CourtOrderedMaintenanceRecvd;
    target.MaintenanceRecvdCourtOrderNo = source.MaintenanceRecvdCourtOrderNo;
    target.HealthAndDentalInsurancePrem = source.HealthAndDentalInsurancePrem;
    target.EligibleForFederalTaxCredit = source.EligibleForFederalTaxCredit;
    target.EligibleForKansasTaxCredit = source.EligibleForKansasTaxCredit;
    target.NetAdjParentalChildSuppAmt = source.NetAdjParentalChildSuppAmt;
    target.InsuranceWorkRelatedCcCredit = source.InsuranceWorkRelatedCcCredit;
    target.AbilityToPay = source.AbilityToPay;
    target.EqualParentingTimeObligation = source.EqualParentingTimeObligation;
    target.SocialSecDependentBenefit = source.SocialSecDependentBenefit;
  }

  private static void MoveCsePersonSupportWorksheet2(
    CsePersonSupportWorksheet source, CsePersonSupportWorksheet target)
  {
    target.NoOfChildrenInDayCare = source.NoOfChildrenInDayCare;
    target.WorkRelatedChildCareCosts = source.WorkRelatedChildCareCosts;
    target.Identifer = source.Identifer;
    target.WageEarnerGrossIncome = source.WageEarnerGrossIncome;
    target.SelfEmploymentGrossIncome = source.SelfEmploymentGrossIncome;
    target.ReasonableBusinessExpense = source.ReasonableBusinessExpense;
    target.CourtOrderedChildSupportPaid = source.CourtOrderedChildSupportPaid;
    target.ChildSupprtPaidCourtOrderNo = source.ChildSupprtPaidCourtOrderNo;
    target.CourtOrderedMaintenancePaid = source.CourtOrderedMaintenancePaid;
    target.MaintenancePaidCourtOrderNo = source.MaintenancePaidCourtOrderNo;
    target.CourtOrderedMaintenanceRecvd = source.CourtOrderedMaintenanceRecvd;
    target.MaintenanceRecvdCourtOrderNo = source.MaintenanceRecvdCourtOrderNo;
    target.HealthAndDentalInsurancePrem = source.HealthAndDentalInsurancePrem;
    target.EligibleForFederalTaxCredit = source.EligibleForFederalTaxCredit;
    target.EligibleForKansasTaxCredit = source.EligibleForKansasTaxCredit;
    target.NetAdjParentalChildSuppAmt = source.NetAdjParentalChildSuppAmt;
    target.EnforcementFeeAllowance = source.EnforcementFeeAllowance;
    target.InsuranceWorkRelatedCcCredit = source.InsuranceWorkRelatedCcCredit;
    target.AbilityToPay = source.AbilityToPay;
    target.EqualParentingTimeObligation = source.EqualParentingTimeObligation;
    target.SocialSecDependentBenefit = source.SocialSecDependentBenefit;
  }

  private static void MoveExport1ToWorksheet(OeWorkReadChildSupWorksheet.Export.
    ExportGroup source, Local.WorksheetGroup target)
  {
    target.GlocalWork.SelectChar = source.Work.SelectChar;
    target.GlocalParentB.AdjustmentAmount = source.ParentB.AdjustmentAmount;
    target.GlocalParentA.AdjustmentAmount = source.ParentA.AdjustmentAmount;
  }

  private static void MoveField(Field source, Field target)
  {
    target.Name = source.Name;
    target.SubroutineName = source.SubroutineName;
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

  private static void MoveGexport2020CalculationsToGlocal2020Calculations(
    OeWorkCalcChildSupWorksheet.Export.Gexport2020CalculationsGroup source,
    Local.Glocal2020CalculationsGroup target)
  {
    target.GlocalDParentTimeAdjFlag.SelectChar =
      source.GexportDParentTimeAdjFlag.SelectChar;
    target.GlocalD4ParAPropShare.TotalCurrency =
      source.GexportD4ParAPropShare.TotalCurrency;
    target.GlocalD4ParBPropShare.TotalCurrency =
      source.GexportD4ParBPropShare.TotalCurrency;
    target.GlocalD4TotalPropShare.TotalCurrency =
      source.GexportD4TotalPropShare.TotalCurrency;
    target.GlocalD5ParAParentTimAdj.TotalCurrency =
      source.GexportD5ParAParentTimAdj.TotalCurrency;
    target.GlocalD5ParBParentTimAdj.TotalCurrency =
      source.GexportD5ParBParentTimAdj.TotalCurrency;
    target.GlocalD5TotalParentTimAdj.TotalCurrency =
      source.GexportD5TotalParentTimAdj.TotalCurrency;
    target.GlocalD6ParAPsAfterPat.TotalCurrency =
      source.GexportD6ParAPsAfterPat.TotalCurrency;
    target.GlocalD6ParBPsAfterPat.TotalCurrency =
      source.GexportD6ParBPsAfterPat.TotalCurrency;
    target.GlocalD6TotalPsAfterPat.TotalCurrency =
      source.GexportD6TotalPsAfterPat.TotalCurrency;
    target.GlocalD8ParAPropShrHip.TotalCurrency =
      source.GexportD8ParAPropShrHip.TotalCurrency;
    target.GlocalD8ParBPropShrHip.TotalCurrency =
      source.GexportD8ParBPropShrHip.TotalCurrency;
    target.GlocalD8TotalPropShrHip.TotalCurrency =
      source.GexportD8TotalPropShrHip.TotalCurrency;
    target.GlocalD10ParAPropShrWrcc.TotalCurrency =
      source.GexportD10ParAPropShrWrcc.TotalCurrency;
    target.GlocalD10ParBPropShrWrcc.TotalCurrency =
      source.GexportD10ParBPropShrWrcc.TotalCurrency;
    target.GlocalD10TotalPropShrWrcc.TotalCurrency =
      source.GexportD10TotalPropShrWrcc.TotalCurrency;
    target.GlocalD11ParAPropShrCcob.TotalCurrency =
      source.GexportD11ParAPropShrCcob.TotalCurrency;
    target.GlocalD11ParBPropShrCcob.TotalCurrency =
      source.GexportD11ParBPropShrCcob.TotalCurrency;
    target.GlocalD11TotalPropShrCcob.TotalCurrency =
      source.GexportD11TotalPropShrCcob.TotalCurrency;
    target.GlocalD12TotalInsWrccPaid.TotalCurrency =
      source.GexportD12TotalInsWrccPaid.TotalCurrency;
    target.GlocalD13ParABasicChSup.TotalCurrency =
      source.GexportD13ParABasicChSup.TotalCurrency;
    target.GlocalD13ParBBasicChSup.TotalCurrency =
      source.GexportD13ParBBasicChSup.TotalCurrency;
    target.GlocalF3ParAAdjSubtotal.SelectChar =
      source.GexportF3ParAAdjSubtotal.SelectChar;
    target.GlocalF3ParBAdjSubtotal.SelectChar =
      source.GexportF3ParBAdjSubtotal.SelectChar;
    target.GlocalF5A0Parent.SelectChar = source.GexportF5A0Parent.SelectChar;
    target.GlocalF5A1CsIncome.TotalCurrency =
      source.GexportF5A1CsIncome.TotalCurrency;
    target.GlocalF5A2PovertyLevel.TotalCurrency =
      source.GexportF5A2PovertyLevel.TotalCurrency;
    target.GlocalF5A3AbilityToPay.TotalCurrency =
      source.GexportF5A3AbilityToPay.TotalCurrency;
    target.GlocalF5BParASubtotal.TotalCurrency =
      source.GexportF5BParASubtotal.TotalCurrency;
    target.GlocalF5BParBSubtotal.TotalCurrency =
      source.GexportF5BParBSubtotal.TotalCurrency;
    target.GlocalF6BParAFinaSubtotal.TotalCurrency =
      source.GexportF6BParAFinaSubtotal.TotalCurrency;
    target.GlocalF6BParBFinaSubtotal.TotalCurrency =
      source.GexportF6BParBFinaSubtotal.TotalCurrency;
    target.GlocalF8ParANetCsOblig.TotalCurrency =
      source.GexportF8ParANetCsOblig.TotalCurrency;
    target.GlocalF8ParBNetCsOblig.TotalCurrency =
      source.GexportF8ParBNetCsOblig.TotalCurrency;
  }

  private static void MoveWorksheetToImport1(Local.WorksheetGroup source,
    OeWorkCalcChildSupWorksheet.Import.ImportGroup target)
  {
    target.Work.SelectChar = source.GlocalWork.SelectChar;
    target.ParentB.AdjustmentAmount = source.GlocalParentB.AdjustmentAmount;
    target.ParentA.AdjustmentAmount = source.GlocalParentA.AdjustmentAmount;
  }

  private void UseOeWorkCalcChildSupWorksheet()
  {
    var useImport = new OeWorkCalcChildSupWorksheet.Import();
    var useExport = new OeWorkCalcChildSupWorksheet.Export();

    useImport.Common.Assign(local.EnforcementFee);
    MoveCsePersonSupportWorksheet2(local.ParentACsePersonSupportWorksheet,
      useImport.ParentA);
    MoveCsePersonSupportWorksheet2(local.ParentBCsePersonSupportWorksheet,
      useImport.ParentB);
    local.Worksheet.CopyTo(useImport.Import1, MoveWorksheetToImport1);
    MoveChildSupportWorksheet(local.ChildSupportWorksheet,
      useImport.ChildSupportWorksheet);
    useImport.Gimport2020EnterableFields.GimportParentingTime.Assign(
      local.Glocal2020EnterableFields.GlocalParentingTime);
    useImport.Gimport2020EnterableFields.GimportAbilityToPayParent.SelectChar =
      local.Glocal2020EnterableFields.GlocalAbilityToPayParent.SelectChar;

    Call(OeWorkCalcChildSupWorksheet.Execute, useImport, useExport);

    local.ParentBEnfFee.TotalCurrency = useExport.ParentBEnfFee.TotalCurrency;
    local.ParentAEnfFee.TotalCurrency = useExport.ParentAEnfFee.TotalCurrency;
    local.ParentBF5NetCs.TotalCurrency =
      useExport.ParentBD10F1NetCs.TotalCurrency;
    local.ParentAF5NetCs.TotalCurrency =
      useExport.ParentAD10F1NetCs.TotalCurrency;
    local.ParentAB3SeGrossInc.TotalCurrency =
      useExport.ParentAB3SeGrossInc.TotalCurrency;
    local.ParentBB3SeGrossInc.TotalCurrency =
      useExport.ParentBB3SeGrossInc.TotalCurrency;
    local.ParentAC1TotGrossInc.TotalCurrency =
      useExport.ParentAC1TotGrossInc.TotalCurrency;
    local.ParentBC1TotGrossInc.TotalCurrency =
      useExport.ParentBC1TotGrossInc.TotalCurrency;
    local.ParentAC5D1TotCsInc.TotalCurrency =
      useExport.ParentAC5D1TotCsInc.TotalCurrency;
    local.ParentBC5D1TotCsInc.TotalCurrency =
      useExport.ParentBC5D1TotCsInc.TotalCurrency;
    local.ParentAD2PercentInc.TotalCurrency =
      useExport.ParentAD2PercentInc.TotalCurrency;
    local.ParentBD2PercentInc.TotalCurrency =
      useExport.ParentBD2PercentInc.TotalCurrency;
    local.CsObligAgrp1TotalAmt.TotalCurrency =
      useExport.CsOblig06TotalAmt.TotalCurrency;
    local.CsObligAgrp3TotalAmt.TotalCurrency =
      useExport.CsOblig1618TotalAmt.TotalCurrency;
    local.CsObligAgrp2TotalAmt.TotalCurrency =
      useExport.CsOblig715TotalAmt.TotalCurrency;
    local.CsObligTotalAmount.TotalCurrency =
      useExport.CsObligTotalAmount.TotalCurrency;
    local.TotalInsurancePrem.TotalCurrency =
      useExport.TotalInsurancePrem.TotalCurrency;
    local.ParentATotalTaxCredit.TotalCurrency =
      useExport.ParentATotalTaxCredit.TotalCurrency;
    local.ParentBTotalTaxCredit.TotalCurrency =
      useExport.ParentBTotalTaxCredit.TotalCurrency;
    local.ParentBChildCareCost.TotalCurrency =
      useExport.ParentBChildCareCost.TotalCurrency;
    local.ParentAChildCareCost.TotalCurrency =
      useExport.ParentAChildCareCost.TotalCurrency;
    local.TotalChildCareCost.TotalCurrency =
      useExport.TotalChildCareCost.TotalCurrency;
    local.TotalD6ChildSuppOblig.TotalCurrency =
      useExport.D6otalChildSuppOblig.TotalCurrency;
    local.ParentAD7CsOblig.TotalCurrency =
      useExport.ParentAD7CsOblig.TotalCurrency;
    local.ParentBD7CsOblig.TotalCurrency =
      useExport.ParentBD7CsOblig.TotalCurrency;
    local.ParentAD8Adjustments.TotalCurrency =
      useExport.ParentAD8Adjustments.TotalCurrency;
    local.ParentBD8Adjustments.TotalCurrency =
      useExport.ParentBD8Adjustments.TotalCurrency;
    local.ParentBD9F1NetCs.TotalCurrency =
      useExport.ParentBD9F1NetCs.TotalCurrency;
    local.ParentAD9F1NetCs.TotalCurrency =
      useExport.ParentAD9F1NetCs.TotalCurrency;
    local.ParentAE7F2TotAdj.TotalCurrency =
      useExport.ParentAE7F2TotAdj.TotalCurrency;
    local.ParentAF3AdjCsOblig.TotalCurrency =
      useExport.ParentAF3AdjCsOblig.TotalCurrency;
    local.ParentBE7F2TotAdj.TotalCurrency =
      useExport.ParentBE7F2TotAdj.TotalCurrency;
    local.ParentBF3AdjCsOblig.TotalCurrency =
      useExport.ParentBF3AdjCsOblig.TotalCurrency;
    local.TotalD1CsInc.TotalCurrency = useExport.D1otalCsInc.TotalCurrency;
    local.ParentAF2TotalCsAdj.TotalCurrency =
      useExport.ParentAF2TotalCsAdj.TotalCurrency;
    local.ParentBF2TotalCsAdj.TotalCurrency =
      useExport.ParentBF2TotalCsAdj.TotalCurrency;
    local.ParentACsePersonSupportWorksheet.Assign(useExport.ParentA);
    local.ParentBCsePersonSupportWorksheet.Assign(useExport.ParentB);
    MoveChildSupportWorksheet(useExport.ChildSupportWorksheet,
      local.ChildSupportWorksheet);
    local.IncomeBeyondSchedule.Flag = useExport.IncomeBeyondSchedule.Flag;
    MoveGexport2020CalculationsToGlocal2020Calculations(useExport.
      Gexport2020Calculations, local.Glocal2020Calculations);
  }

  private string UseOeWorkCenterFirstName1()
  {
    var useImport = new OeWorkCenterFirstName.Import();
    var useExport = new OeWorkCenterFirstName.Export();

    useImport.CsePersonsWorkSet.FirstName =
      local.ParentBCsePersonsWorkSet.FirstName;

    Call(OeWorkCenterFirstName.Execute, useImport, useExport);

    return useExport.CsePersonsWorkSet.FirstName;
  }

  private string UseOeWorkCenterFirstName2()
  {
    var useImport = new OeWorkCenterFirstName.Import();
    var useExport = new OeWorkCenterFirstName.Export();

    useImport.CsePersonsWorkSet.FirstName =
      local.ParentACsePersonsWorkSet.FirstName;

    Call(OeWorkCenterFirstName.Execute, useImport, useExport);

    return useExport.CsePersonsWorkSet.FirstName;
  }

  private void UseOeWorkReadChildSupWorksheet()
  {
    var useImport = new OeWorkReadChildSupWorksheet.Import();
    var useExport = new OeWorkReadChildSupWorksheet.Export();

    useImport.ChildSupportWorksheet.Identifier =
      local.ChildSupportWorksheet.Identifier;

    Call(OeWorkReadChildSupWorksheet.Execute, useImport, useExport);

    local.EnforcementFee.Assign(useExport.Common);
    local.ParentBCsePersonsWorkSet.Assign(useExport.ParentBName);
    local.ParentACsePersonsWorkSet.Assign(useExport.ParentAName);
    MoveCsePersonSupportWorksheet1(useExport.ParentACsePersonSupportWorksheet,
      local.ParentACsePersonSupportWorksheet);
    MoveCsePersonSupportWorksheet1(useExport.ParentBCsePersonSupportWorksheet,
      local.ParentBCsePersonSupportWorksheet);
    useExport.Export1.CopyTo(local.Worksheet, MoveExport1ToWorksheet);
    local.ChildSupportWorksheet.Assign(useExport.ChildSupportWorksheet);
    local.Glocal2020EnterableFields.GlocalParentingTime.Assign(
      useExport.Gexport2020EnterableFields.GexportParentingTime);
    local.Glocal2020EnterableFields.GlocalAbilityToPayParent.SelectChar =
      useExport.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar;
      
  }

  private void UseSpCabCreateUpdateFieldValue1()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Field.Name = entities.Field.Name;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    MoveFieldValue1(local.FieldValue, useImport.FieldValue);

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue2()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    useImport.Field.Name = local.Current.Name;
    useImport.FieldValue.Value = local.Address.Item.GlocalAddress.Value;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private string UseSpDocFormatName()
  {
    var useImport = new SpDocFormatName.Import();
    var useExport = new SpDocFormatName.Export();

    useImport.SpPrintWorkSet.Assign(local.SpPrintWorkSet);

    Call(SpDocFormatName.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    private Infrastructure infrastructure;
    private Field field;
    private Document document;
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

    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A Glocal2020CalculationsGroup group.</summary>
    [Serializable]
    public class Glocal2020CalculationsGroup
    {
      /// <summary>
      /// A value of GlocalDParentTimeAdjFlag.
      /// </summary>
      [JsonPropertyName("glocalDParentTimeAdjFlag")]
      public Common GlocalDParentTimeAdjFlag
      {
        get => glocalDParentTimeAdjFlag ??= new();
        set => glocalDParentTimeAdjFlag = value;
      }

      /// <summary>
      /// A value of GlocalD4ParAPropShare.
      /// </summary>
      [JsonPropertyName("glocalD4ParAPropShare")]
      public Common GlocalD4ParAPropShare
      {
        get => glocalD4ParAPropShare ??= new();
        set => glocalD4ParAPropShare = value;
      }

      /// <summary>
      /// A value of GlocalD4ParBPropShare.
      /// </summary>
      [JsonPropertyName("glocalD4ParBPropShare")]
      public Common GlocalD4ParBPropShare
      {
        get => glocalD4ParBPropShare ??= new();
        set => glocalD4ParBPropShare = value;
      }

      /// <summary>
      /// A value of GlocalD4TotalPropShare.
      /// </summary>
      [JsonPropertyName("glocalD4TotalPropShare")]
      public Common GlocalD4TotalPropShare
      {
        get => glocalD4TotalPropShare ??= new();
        set => glocalD4TotalPropShare = value;
      }

      /// <summary>
      /// A value of GlocalD5ParAParentTimAdj.
      /// </summary>
      [JsonPropertyName("glocalD5ParAParentTimAdj")]
      public Common GlocalD5ParAParentTimAdj
      {
        get => glocalD5ParAParentTimAdj ??= new();
        set => glocalD5ParAParentTimAdj = value;
      }

      /// <summary>
      /// A value of GlocalD5ParBParentTimAdj.
      /// </summary>
      [JsonPropertyName("glocalD5ParBParentTimAdj")]
      public Common GlocalD5ParBParentTimAdj
      {
        get => glocalD5ParBParentTimAdj ??= new();
        set => glocalD5ParBParentTimAdj = value;
      }

      /// <summary>
      /// A value of GlocalD5TotalParentTimAdj.
      /// </summary>
      [JsonPropertyName("glocalD5TotalParentTimAdj")]
      public Common GlocalD5TotalParentTimAdj
      {
        get => glocalD5TotalParentTimAdj ??= new();
        set => glocalD5TotalParentTimAdj = value;
      }

      /// <summary>
      /// A value of GlocalD6ParAPsAfterPat.
      /// </summary>
      [JsonPropertyName("glocalD6ParAPsAfterPat")]
      public Common GlocalD6ParAPsAfterPat
      {
        get => glocalD6ParAPsAfterPat ??= new();
        set => glocalD6ParAPsAfterPat = value;
      }

      /// <summary>
      /// A value of GlocalD6ParBPsAfterPat.
      /// </summary>
      [JsonPropertyName("glocalD6ParBPsAfterPat")]
      public Common GlocalD6ParBPsAfterPat
      {
        get => glocalD6ParBPsAfterPat ??= new();
        set => glocalD6ParBPsAfterPat = value;
      }

      /// <summary>
      /// A value of GlocalD6TotalPsAfterPat.
      /// </summary>
      [JsonPropertyName("glocalD6TotalPsAfterPat")]
      public Common GlocalD6TotalPsAfterPat
      {
        get => glocalD6TotalPsAfterPat ??= new();
        set => glocalD6TotalPsAfterPat = value;
      }

      /// <summary>
      /// A value of GlocalD8ParAPropShrHip.
      /// </summary>
      [JsonPropertyName("glocalD8ParAPropShrHip")]
      public Common GlocalD8ParAPropShrHip
      {
        get => glocalD8ParAPropShrHip ??= new();
        set => glocalD8ParAPropShrHip = value;
      }

      /// <summary>
      /// A value of GlocalD8ParBPropShrHip.
      /// </summary>
      [JsonPropertyName("glocalD8ParBPropShrHip")]
      public Common GlocalD8ParBPropShrHip
      {
        get => glocalD8ParBPropShrHip ??= new();
        set => glocalD8ParBPropShrHip = value;
      }

      /// <summary>
      /// A value of GlocalD8TotalPropShrHip.
      /// </summary>
      [JsonPropertyName("glocalD8TotalPropShrHip")]
      public Common GlocalD8TotalPropShrHip
      {
        get => glocalD8TotalPropShrHip ??= new();
        set => glocalD8TotalPropShrHip = value;
      }

      /// <summary>
      /// A value of GlocalD10ParAPropShrWrcc.
      /// </summary>
      [JsonPropertyName("glocalD10ParAPropShrWrcc")]
      public Common GlocalD10ParAPropShrWrcc
      {
        get => glocalD10ParAPropShrWrcc ??= new();
        set => glocalD10ParAPropShrWrcc = value;
      }

      /// <summary>
      /// A value of GlocalD10ParBPropShrWrcc.
      /// </summary>
      [JsonPropertyName("glocalD10ParBPropShrWrcc")]
      public Common GlocalD10ParBPropShrWrcc
      {
        get => glocalD10ParBPropShrWrcc ??= new();
        set => glocalD10ParBPropShrWrcc = value;
      }

      /// <summary>
      /// A value of GlocalD10TotalPropShrWrcc.
      /// </summary>
      [JsonPropertyName("glocalD10TotalPropShrWrcc")]
      public Common GlocalD10TotalPropShrWrcc
      {
        get => glocalD10TotalPropShrWrcc ??= new();
        set => glocalD10TotalPropShrWrcc = value;
      }

      /// <summary>
      /// A value of GlocalD11ParAPropShrCcob.
      /// </summary>
      [JsonPropertyName("glocalD11ParAPropShrCcob")]
      public Common GlocalD11ParAPropShrCcob
      {
        get => glocalD11ParAPropShrCcob ??= new();
        set => glocalD11ParAPropShrCcob = value;
      }

      /// <summary>
      /// A value of GlocalD11ParBPropShrCcob.
      /// </summary>
      [JsonPropertyName("glocalD11ParBPropShrCcob")]
      public Common GlocalD11ParBPropShrCcob
      {
        get => glocalD11ParBPropShrCcob ??= new();
        set => glocalD11ParBPropShrCcob = value;
      }

      /// <summary>
      /// A value of GlocalD11TotalPropShrCcob.
      /// </summary>
      [JsonPropertyName("glocalD11TotalPropShrCcob")]
      public Common GlocalD11TotalPropShrCcob
      {
        get => glocalD11TotalPropShrCcob ??= new();
        set => glocalD11TotalPropShrCcob = value;
      }

      /// <summary>
      /// A value of GlocalD12TotalInsWrccPaid.
      /// </summary>
      [JsonPropertyName("glocalD12TotalInsWrccPaid")]
      public Common GlocalD12TotalInsWrccPaid
      {
        get => glocalD12TotalInsWrccPaid ??= new();
        set => glocalD12TotalInsWrccPaid = value;
      }

      /// <summary>
      /// A value of GlocalD13ParABasicChSup.
      /// </summary>
      [JsonPropertyName("glocalD13ParABasicChSup")]
      public Common GlocalD13ParABasicChSup
      {
        get => glocalD13ParABasicChSup ??= new();
        set => glocalD13ParABasicChSup = value;
      }

      /// <summary>
      /// A value of GlocalD13ParBBasicChSup.
      /// </summary>
      [JsonPropertyName("glocalD13ParBBasicChSup")]
      public Common GlocalD13ParBBasicChSup
      {
        get => glocalD13ParBBasicChSup ??= new();
        set => glocalD13ParBBasicChSup = value;
      }

      /// <summary>
      /// A value of GlocalF3ParAAdjSubtotal.
      /// </summary>
      [JsonPropertyName("glocalF3ParAAdjSubtotal")]
      public Common GlocalF3ParAAdjSubtotal
      {
        get => glocalF3ParAAdjSubtotal ??= new();
        set => glocalF3ParAAdjSubtotal = value;
      }

      /// <summary>
      /// A value of GlocalF3ParBAdjSubtotal.
      /// </summary>
      [JsonPropertyName("glocalF3ParBAdjSubtotal")]
      public Common GlocalF3ParBAdjSubtotal
      {
        get => glocalF3ParBAdjSubtotal ??= new();
        set => glocalF3ParBAdjSubtotal = value;
      }

      /// <summary>
      /// A value of GlocalF5A0Parent.
      /// </summary>
      [JsonPropertyName("glocalF5A0Parent")]
      public Common GlocalF5A0Parent
      {
        get => glocalF5A0Parent ??= new();
        set => glocalF5A0Parent = value;
      }

      /// <summary>
      /// A value of GlocalF5A1CsIncome.
      /// </summary>
      [JsonPropertyName("glocalF5A1CsIncome")]
      public Common GlocalF5A1CsIncome
      {
        get => glocalF5A1CsIncome ??= new();
        set => glocalF5A1CsIncome = value;
      }

      /// <summary>
      /// A value of GlocalF5A2PovertyLevel.
      /// </summary>
      [JsonPropertyName("glocalF5A2PovertyLevel")]
      public Common GlocalF5A2PovertyLevel
      {
        get => glocalF5A2PovertyLevel ??= new();
        set => glocalF5A2PovertyLevel = value;
      }

      /// <summary>
      /// A value of GlocalF5A3AbilityToPay.
      /// </summary>
      [JsonPropertyName("glocalF5A3AbilityToPay")]
      public Common GlocalF5A3AbilityToPay
      {
        get => glocalF5A3AbilityToPay ??= new();
        set => glocalF5A3AbilityToPay = value;
      }

      /// <summary>
      /// A value of GlocalF5BParASubtotal.
      /// </summary>
      [JsonPropertyName("glocalF5BParASubtotal")]
      public Common GlocalF5BParASubtotal
      {
        get => glocalF5BParASubtotal ??= new();
        set => glocalF5BParASubtotal = value;
      }

      /// <summary>
      /// A value of GlocalF5BParBSubtotal.
      /// </summary>
      [JsonPropertyName("glocalF5BParBSubtotal")]
      public Common GlocalF5BParBSubtotal
      {
        get => glocalF5BParBSubtotal ??= new();
        set => glocalF5BParBSubtotal = value;
      }

      /// <summary>
      /// A value of GlocalF6BParAFinaSubtotal.
      /// </summary>
      [JsonPropertyName("glocalF6BParAFinaSubtotal")]
      public Common GlocalF6BParAFinaSubtotal
      {
        get => glocalF6BParAFinaSubtotal ??= new();
        set => glocalF6BParAFinaSubtotal = value;
      }

      /// <summary>
      /// A value of GlocalF6BParBFinaSubtotal.
      /// </summary>
      [JsonPropertyName("glocalF6BParBFinaSubtotal")]
      public Common GlocalF6BParBFinaSubtotal
      {
        get => glocalF6BParBFinaSubtotal ??= new();
        set => glocalF6BParBFinaSubtotal = value;
      }

      /// <summary>
      /// A value of GlocalF8ParANetCsOblig.
      /// </summary>
      [JsonPropertyName("glocalF8ParANetCsOblig")]
      public Common GlocalF8ParANetCsOblig
      {
        get => glocalF8ParANetCsOblig ??= new();
        set => glocalF8ParANetCsOblig = value;
      }

      /// <summary>
      /// A value of GlocalF8ParBNetCsOblig.
      /// </summary>
      [JsonPropertyName("glocalF8ParBNetCsOblig")]
      public Common GlocalF8ParBNetCsOblig
      {
        get => glocalF8ParBNetCsOblig ??= new();
        set => glocalF8ParBNetCsOblig = value;
      }

      private Common glocalDParentTimeAdjFlag;
      private Common glocalD4ParAPropShare;
      private Common glocalD4ParBPropShare;
      private Common glocalD4TotalPropShare;
      private Common glocalD5ParAParentTimAdj;
      private Common glocalD5ParBParentTimAdj;
      private Common glocalD5TotalParentTimAdj;
      private Common glocalD6ParAPsAfterPat;
      private Common glocalD6ParBPsAfterPat;
      private Common glocalD6TotalPsAfterPat;
      private Common glocalD8ParAPropShrHip;
      private Common glocalD8ParBPropShrHip;
      private Common glocalD8TotalPropShrHip;
      private Common glocalD10ParAPropShrWrcc;
      private Common glocalD10ParBPropShrWrcc;
      private Common glocalD10TotalPropShrWrcc;
      private Common glocalD11ParAPropShrCcob;
      private Common glocalD11ParBPropShrCcob;
      private Common glocalD11TotalPropShrCcob;
      private Common glocalD12TotalInsWrccPaid;
      private Common glocalD13ParABasicChSup;
      private Common glocalD13ParBBasicChSup;
      private Common glocalF3ParAAdjSubtotal;
      private Common glocalF3ParBAdjSubtotal;
      private Common glocalF5A0Parent;
      private Common glocalF5A1CsIncome;
      private Common glocalF5A2PovertyLevel;
      private Common glocalF5A3AbilityToPay;
      private Common glocalF5BParASubtotal;
      private Common glocalF5BParBSubtotal;
      private Common glocalF6BParAFinaSubtotal;
      private Common glocalF6BParBFinaSubtotal;
      private Common glocalF8ParANetCsOblig;
      private Common glocalF8ParBNetCsOblig;
    }

    /// <summary>A Glocal2020EnterableFieldsGroup group.</summary>
    [Serializable]
    public class Glocal2020EnterableFieldsGroup
    {
      /// <summary>
      /// A value of GlocalParentingTime.
      /// </summary>
      [JsonPropertyName("glocalParentingTime")]
      public Common GlocalParentingTime
      {
        get => glocalParentingTime ??= new();
        set => glocalParentingTime = value;
      }

      /// <summary>
      /// A value of GlocalAbilityToPayParent.
      /// </summary>
      [JsonPropertyName("glocalAbilityToPayParent")]
      public Common GlocalAbilityToPayParent
      {
        get => glocalAbilityToPayParent ??= new();
        set => glocalAbilityToPayParent = value;
      }

      private Common glocalParentingTime;
      private Common glocalAbilityToPayParent;
    }

    /// <summary>A WorksheetGroup group.</summary>
    [Serializable]
    public class WorksheetGroup
    {
      /// <summary>
      /// A value of GlocalWork.
      /// </summary>
      [JsonPropertyName("glocalWork")]
      public Common GlocalWork
      {
        get => glocalWork ??= new();
        set => glocalWork = value;
      }

      /// <summary>
      /// A value of GlocalParentB.
      /// </summary>
      [JsonPropertyName("glocalParentB")]
      public CsePersonSupportAdjustment GlocalParentB
      {
        get => glocalParentB ??= new();
        set => glocalParentB = value;
      }

      /// <summary>
      /// A value of GlocalParentA.
      /// </summary>
      [JsonPropertyName("glocalParentA")]
      public CsePersonSupportAdjustment GlocalParentA
      {
        get => glocalParentA ??= new();
        set => glocalParentA = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common glocalWork;
      private CsePersonSupportAdjustment glocalParentB;
      private CsePersonSupportAdjustment glocalParentA;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GfieldValue.
      /// </summary>
      [JsonPropertyName("gfieldValue")]
      public FieldValue GfieldValue
      {
        get => gfieldValue ??= new();
        set => gfieldValue = value;
      }

      /// <summary>
      /// Gets a value of Subordinate.
      /// </summary>
      [JsonIgnore]
      public Array<SubordinateGroup> Subordinate => subordinate ??= new(
        SubordinateGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of Subordinate for json serialization.
      /// </summary>
      [JsonPropertyName("subordinate")]
      [Computed]
      public IList<SubordinateGroup> Subordinate_Json
      {
        get => subordinate;
        set => Subordinate.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet gcsePersonsWorkSet;
      private FieldValue gfieldValue;
      private Array<SubordinateGroup> subordinate;
    }

    /// <summary>A SubordinateGroup group.</summary>
    [Serializable]
    public class SubordinateGroup
    {
      /// <summary>
      /// A value of GlocalSubordinate.
      /// </summary>
      [JsonPropertyName("glocalSubordinate")]
      public FieldValue GlocalSubordinate
      {
        get => glocalSubordinate ??= new();
        set => glocalSubordinate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue glocalSubordinate;
    }

    /// <summary>A AddressGroup group.</summary>
    [Serializable]
    public class AddressGroup
    {
      /// <summary>
      /// A value of GlocalAddress.
      /// </summary>
      [JsonPropertyName("glocalAddress")]
      public FieldValue GlocalAddress
      {
        get => glocalAddress ??= new();
        set => glocalAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue glocalAddress;
    }

    /// <summary>
    /// A value of IncomeBeyondSchedule.
    /// </summary>
    [JsonPropertyName("incomeBeyondSchedule")]
    public Common IncomeBeyondSchedule
    {
      get => incomeBeyondSchedule ??= new();
      set => incomeBeyondSchedule = value;
    }

    /// <summary>
    /// Gets a value of Glocal2020Calculations.
    /// </summary>
    [JsonPropertyName("glocal2020Calculations")]
    public Glocal2020CalculationsGroup Glocal2020Calculations
    {
      get => glocal2020Calculations ?? (glocal2020Calculations = new());
      set => glocal2020Calculations = value;
    }

    /// <summary>
    /// Gets a value of Glocal2020EnterableFields.
    /// </summary>
    [JsonPropertyName("glocal2020EnterableFields")]
    public Glocal2020EnterableFieldsGroup Glocal2020EnterableFields
    {
      get => glocal2020EnterableFields ?? (glocal2020EnterableFields = new());
      set => glocal2020EnterableFields = value;
    }

    /// <summary>
    /// A value of ParentBEnfFee.
    /// </summary>
    [JsonPropertyName("parentBEnfFee")]
    public Common ParentBEnfFee
    {
      get => parentBEnfFee ??= new();
      set => parentBEnfFee = value;
    }

    /// <summary>
    /// A value of ParentAEnfFee.
    /// </summary>
    [JsonPropertyName("parentAEnfFee")]
    public Common ParentAEnfFee
    {
      get => parentAEnfFee ??= new();
      set => parentAEnfFee = value;
    }

    /// <summary>
    /// A value of ParentBF5NetCs.
    /// </summary>
    [JsonPropertyName("parentBF5NetCs")]
    public Common ParentBF5NetCs
    {
      get => parentBF5NetCs ??= new();
      set => parentBF5NetCs = value;
    }

    /// <summary>
    /// A value of ParentAF5NetCs.
    /// </summary>
    [JsonPropertyName("parentAF5NetCs")]
    public Common ParentAF5NetCs
    {
      get => parentAF5NetCs ??= new();
      set => parentAF5NetCs = value;
    }

    /// <summary>
    /// A value of ParentAB3SeGrossInc.
    /// </summary>
    [JsonPropertyName("parentAB3SeGrossInc")]
    public Common ParentAB3SeGrossInc
    {
      get => parentAB3SeGrossInc ??= new();
      set => parentAB3SeGrossInc = value;
    }

    /// <summary>
    /// A value of ParentBB3SeGrossInc.
    /// </summary>
    [JsonPropertyName("parentBB3SeGrossInc")]
    public Common ParentBB3SeGrossInc
    {
      get => parentBB3SeGrossInc ??= new();
      set => parentBB3SeGrossInc = value;
    }

    /// <summary>
    /// A value of ParentAC1TotGrossInc.
    /// </summary>
    [JsonPropertyName("parentAC1TotGrossInc")]
    public Common ParentAC1TotGrossInc
    {
      get => parentAC1TotGrossInc ??= new();
      set => parentAC1TotGrossInc = value;
    }

    /// <summary>
    /// A value of ParentBC1TotGrossInc.
    /// </summary>
    [JsonPropertyName("parentBC1TotGrossInc")]
    public Common ParentBC1TotGrossInc
    {
      get => parentBC1TotGrossInc ??= new();
      set => parentBC1TotGrossInc = value;
    }

    /// <summary>
    /// A value of ParentAC5D1TotCsInc.
    /// </summary>
    [JsonPropertyName("parentAC5D1TotCsInc")]
    public Common ParentAC5D1TotCsInc
    {
      get => parentAC5D1TotCsInc ??= new();
      set => parentAC5D1TotCsInc = value;
    }

    /// <summary>
    /// A value of ParentBC5D1TotCsInc.
    /// </summary>
    [JsonPropertyName("parentBC5D1TotCsInc")]
    public Common ParentBC5D1TotCsInc
    {
      get => parentBC5D1TotCsInc ??= new();
      set => parentBC5D1TotCsInc = value;
    }

    /// <summary>
    /// A value of ParentAD2PercentInc.
    /// </summary>
    [JsonPropertyName("parentAD2PercentInc")]
    public Common ParentAD2PercentInc
    {
      get => parentAD2PercentInc ??= new();
      set => parentAD2PercentInc = value;
    }

    /// <summary>
    /// A value of ParentBD2PercentInc.
    /// </summary>
    [JsonPropertyName("parentBD2PercentInc")]
    public Common ParentBD2PercentInc
    {
      get => parentBD2PercentInc ??= new();
      set => parentBD2PercentInc = value;
    }

    /// <summary>
    /// A value of CsObligAgrp1TotalAmt.
    /// </summary>
    [JsonPropertyName("csObligAgrp1TotalAmt")]
    public Common CsObligAgrp1TotalAmt
    {
      get => csObligAgrp1TotalAmt ??= new();
      set => csObligAgrp1TotalAmt = value;
    }

    /// <summary>
    /// A value of CsObligAgrp3TotalAmt.
    /// </summary>
    [JsonPropertyName("csObligAgrp3TotalAmt")]
    public Common CsObligAgrp3TotalAmt
    {
      get => csObligAgrp3TotalAmt ??= new();
      set => csObligAgrp3TotalAmt = value;
    }

    /// <summary>
    /// A value of CsObligAgrp2TotalAmt.
    /// </summary>
    [JsonPropertyName("csObligAgrp2TotalAmt")]
    public Common CsObligAgrp2TotalAmt
    {
      get => csObligAgrp2TotalAmt ??= new();
      set => csObligAgrp2TotalAmt = value;
    }

    /// <summary>
    /// A value of CsObligTotalAmount.
    /// </summary>
    [JsonPropertyName("csObligTotalAmount")]
    public Common CsObligTotalAmount
    {
      get => csObligTotalAmount ??= new();
      set => csObligTotalAmount = value;
    }

    /// <summary>
    /// A value of TotalInsurancePrem.
    /// </summary>
    [JsonPropertyName("totalInsurancePrem")]
    public Common TotalInsurancePrem
    {
      get => totalInsurancePrem ??= new();
      set => totalInsurancePrem = value;
    }

    /// <summary>
    /// A value of ParentATotalTaxCredit.
    /// </summary>
    [JsonPropertyName("parentATotalTaxCredit")]
    public Common ParentATotalTaxCredit
    {
      get => parentATotalTaxCredit ??= new();
      set => parentATotalTaxCredit = value;
    }

    /// <summary>
    /// A value of ParentBTotalTaxCredit.
    /// </summary>
    [JsonPropertyName("parentBTotalTaxCredit")]
    public Common ParentBTotalTaxCredit
    {
      get => parentBTotalTaxCredit ??= new();
      set => parentBTotalTaxCredit = value;
    }

    /// <summary>
    /// A value of ParentBChildCareCost.
    /// </summary>
    [JsonPropertyName("parentBChildCareCost")]
    public Common ParentBChildCareCost
    {
      get => parentBChildCareCost ??= new();
      set => parentBChildCareCost = value;
    }

    /// <summary>
    /// A value of ParentAChildCareCost.
    /// </summary>
    [JsonPropertyName("parentAChildCareCost")]
    public Common ParentAChildCareCost
    {
      get => parentAChildCareCost ??= new();
      set => parentAChildCareCost = value;
    }

    /// <summary>
    /// A value of TotalChildCareCost.
    /// </summary>
    [JsonPropertyName("totalChildCareCost")]
    public Common TotalChildCareCost
    {
      get => totalChildCareCost ??= new();
      set => totalChildCareCost = value;
    }

    /// <summary>
    /// A value of TotalD6ChildSuppOblig.
    /// </summary>
    [JsonPropertyName("totalD6ChildSuppOblig")]
    public Common TotalD6ChildSuppOblig
    {
      get => totalD6ChildSuppOblig ??= new();
      set => totalD6ChildSuppOblig = value;
    }

    /// <summary>
    /// A value of ParentAD7CsOblig.
    /// </summary>
    [JsonPropertyName("parentAD7CsOblig")]
    public Common ParentAD7CsOblig
    {
      get => parentAD7CsOblig ??= new();
      set => parentAD7CsOblig = value;
    }

    /// <summary>
    /// A value of ParentBD7CsOblig.
    /// </summary>
    [JsonPropertyName("parentBD7CsOblig")]
    public Common ParentBD7CsOblig
    {
      get => parentBD7CsOblig ??= new();
      set => parentBD7CsOblig = value;
    }

    /// <summary>
    /// A value of ParentAD8Adjustments.
    /// </summary>
    [JsonPropertyName("parentAD8Adjustments")]
    public Common ParentAD8Adjustments
    {
      get => parentAD8Adjustments ??= new();
      set => parentAD8Adjustments = value;
    }

    /// <summary>
    /// A value of ParentBD8Adjustments.
    /// </summary>
    [JsonPropertyName("parentBD8Adjustments")]
    public Common ParentBD8Adjustments
    {
      get => parentBD8Adjustments ??= new();
      set => parentBD8Adjustments = value;
    }

    /// <summary>
    /// A value of ParentBD9F1NetCs.
    /// </summary>
    [JsonPropertyName("parentBD9F1NetCs")]
    public Common ParentBD9F1NetCs
    {
      get => parentBD9F1NetCs ??= new();
      set => parentBD9F1NetCs = value;
    }

    /// <summary>
    /// A value of ParentAD9F1NetCs.
    /// </summary>
    [JsonPropertyName("parentAD9F1NetCs")]
    public Common ParentAD9F1NetCs
    {
      get => parentAD9F1NetCs ??= new();
      set => parentAD9F1NetCs = value;
    }

    /// <summary>
    /// A value of ParentAE7F2TotAdj.
    /// </summary>
    [JsonPropertyName("parentAE7F2TotAdj")]
    public Common ParentAE7F2TotAdj
    {
      get => parentAE7F2TotAdj ??= new();
      set => parentAE7F2TotAdj = value;
    }

    /// <summary>
    /// A value of ParentAF3AdjCsOblig.
    /// </summary>
    [JsonPropertyName("parentAF3AdjCsOblig")]
    public Common ParentAF3AdjCsOblig
    {
      get => parentAF3AdjCsOblig ??= new();
      set => parentAF3AdjCsOblig = value;
    }

    /// <summary>
    /// A value of ParentBE7F2TotAdj.
    /// </summary>
    [JsonPropertyName("parentBE7F2TotAdj")]
    public Common ParentBE7F2TotAdj
    {
      get => parentBE7F2TotAdj ??= new();
      set => parentBE7F2TotAdj = value;
    }

    /// <summary>
    /// A value of ParentBF3AdjCsOblig.
    /// </summary>
    [JsonPropertyName("parentBF3AdjCsOblig")]
    public Common ParentBF3AdjCsOblig
    {
      get => parentBF3AdjCsOblig ??= new();
      set => parentBF3AdjCsOblig = value;
    }

    /// <summary>
    /// A value of TotalD1CsInc.
    /// </summary>
    [JsonPropertyName("totalD1CsInc")]
    public Common TotalD1CsInc
    {
      get => totalD1CsInc ??= new();
      set => totalD1CsInc = value;
    }

    /// <summary>
    /// A value of ParentAF2TotalCsAdj.
    /// </summary>
    [JsonPropertyName("parentAF2TotalCsAdj")]
    public Common ParentAF2TotalCsAdj
    {
      get => parentAF2TotalCsAdj ??= new();
      set => parentAF2TotalCsAdj = value;
    }

    /// <summary>
    /// A value of ParentBF2TotalCsAdj.
    /// </summary>
    [JsonPropertyName("parentBF2TotalCsAdj")]
    public Common ParentBF2TotalCsAdj
    {
      get => parentBF2TotalCsAdj ??= new();
      set => parentBF2TotalCsAdj = value;
    }

    /// <summary>
    /// A value of EnforcementFee.
    /// </summary>
    [JsonPropertyName("enforcementFee")]
    public Common EnforcementFee
    {
      get => enforcementFee ??= new();
      set => enforcementFee = value;
    }

    /// <summary>
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
    }

    /// <summary>
    /// A value of ParentBCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("parentBCsePersonsWorkSet")]
    public CsePersonsWorkSet ParentBCsePersonsWorkSet
    {
      get => parentBCsePersonsWorkSet ??= new();
      set => parentBCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ParentACsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("parentACsePersonsWorkSet")]
    public CsePersonsWorkSet ParentACsePersonsWorkSet
    {
      get => parentACsePersonsWorkSet ??= new();
      set => parentACsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CurrentParentCommon.
    /// </summary>
    [JsonPropertyName("currentParentCommon")]
    public Common CurrentParentCommon
    {
      get => currentParentCommon ??= new();
      set => currentParentCommon = value;
    }

    /// <summary>
    /// A value of ParentACsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("parentACsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet ParentACsePersonSupportWorksheet
    {
      get => parentACsePersonSupportWorksheet ??= new();
      set => parentACsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of ParentBCsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("parentBCsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet ParentBCsePersonSupportWorksheet
    {
      get => parentBCsePersonSupportWorksheet ??= new();
      set => parentBCsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// Gets a value of Worksheet.
    /// </summary>
    [JsonIgnore]
    public Array<WorksheetGroup> Worksheet => worksheet ??= new(
      WorksheetGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Worksheet for json serialization.
    /// </summary>
    [JsonPropertyName("worksheet")]
    [Computed]
    public IList<WorksheetGroup> Worksheet_Json
    {
      get => worksheet;
      set => Worksheet.Assign(value);
    }

    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of CurrentParentField.
    /// </summary>
    [JsonPropertyName("currentParentField")]
    public Field CurrentParentField
    {
      get => currentParentField ??= new();
      set => currentParentField = value;
    }

    /// <summary>
    /// A value of ProcessGroup.
    /// </summary>
    [JsonPropertyName("processGroup")]
    public Common ProcessGroup
    {
      get => processGroup ??= new();
      set => processGroup = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Field Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// Gets a value of Address.
    /// </summary>
    [JsonIgnore]
    public Array<AddressGroup> Address => address ??= new(
      AddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Address for json serialization.
    /// </summary>
    [JsonPropertyName("address")]
    [Computed]
    public IList<AddressGroup> Address_Json
    {
      get => address;
      set => Address.Assign(value);
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Field Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Common incomeBeyondSchedule;
    private Glocal2020CalculationsGroup glocal2020Calculations;
    private Glocal2020EnterableFieldsGroup glocal2020EnterableFields;
    private Common parentBEnfFee;
    private Common parentAEnfFee;
    private Common parentBF5NetCs;
    private Common parentAF5NetCs;
    private Common parentAB3SeGrossInc;
    private Common parentBB3SeGrossInc;
    private Common parentAC1TotGrossInc;
    private Common parentBC1TotGrossInc;
    private Common parentAC5D1TotCsInc;
    private Common parentBC5D1TotCsInc;
    private Common parentAD2PercentInc;
    private Common parentBD2PercentInc;
    private Common csObligAgrp1TotalAmt;
    private Common csObligAgrp3TotalAmt;
    private Common csObligAgrp2TotalAmt;
    private Common csObligTotalAmount;
    private Common totalInsurancePrem;
    private Common parentATotalTaxCredit;
    private Common parentBTotalTaxCredit;
    private Common parentBChildCareCost;
    private Common parentAChildCareCost;
    private Common totalChildCareCost;
    private Common totalD6ChildSuppOblig;
    private Common parentAD7CsOblig;
    private Common parentBD7CsOblig;
    private Common parentAD8Adjustments;
    private Common parentBD8Adjustments;
    private Common parentBD9F1NetCs;
    private Common parentAD9F1NetCs;
    private Common parentAE7F2TotAdj;
    private Common parentAF3AdjCsOblig;
    private Common parentBE7F2TotAdj;
    private Common parentBF3AdjCsOblig;
    private Common totalD1CsInc;
    private Common parentAF2TotalCsAdj;
    private Common parentBF2TotalCsAdj;
    private Common enforcementFee;
    private SpPrintWorkSet spPrintWorkSet;
    private CsePersonsWorkSet parentBCsePersonsWorkSet;
    private CsePersonsWorkSet parentACsePersonsWorkSet;
    private Common currentParentCommon;
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private CsePersonSupportWorksheet parentBCsePersonSupportWorksheet;
    private Array<WorksheetGroup> worksheet;
    private ChildSupportWorksheet childSupportWorksheet;
    private SpDocLiteral spDocLiteral;
    private BatchConvertNumToText batchConvertNumToText;
    private Field currentParentField;
    private Common processGroup;
    private FieldValue fieldValue;
    private Field previous;
    private Array<LocalGroup> local1;
    private Array<AddressGroup> address;
    private Field temp;
    private Common position;
    private Field current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Field field;
    private DocumentField documentField;
    private Document document;
    private CsePerson csePerson;
  }
#endregion
}
