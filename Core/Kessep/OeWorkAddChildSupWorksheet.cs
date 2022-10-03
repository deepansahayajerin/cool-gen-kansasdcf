// Program: OE_WORK_ADD_CHILD_SUP_WORKSHEET, ID: 371897919, model: 746.
// Short name: SWE00975
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
/// A program: OE_WORK_ADD_CHILD_SUP_WORKSHEET.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeWorkAddChildSupWorksheet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_WORK_ADD_CHILD_SUP_WORKSHEET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeWorkAddChildSupWorksheet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeWorkAddChildSupWorksheet.
  /// </summary>
  public OeWorkAddChildSupWorksheet(IContext context, Import import,
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
    // ---------------------------------------------
    // Date      Author	Reason
    // 03/06/95  Sid		Completion
    // 03/06/95  Srini Ganji	PR #706
    // 11/19/07   M. Fan       WR318566(CQ297)- Added the parenting time 
    // adjustment percent to imports, exports and
    // 			entity actions for child support worksheet entity view.
    // 06/04/10  J. Huss	CQ# 18769 - Modified action block to only maintain CSE 
    // person adjustment
    // 			records that have valid values.  Also removed references to Child 
    // Support Adjustment
    // 			Deviation Reason to allow for normalization of tables.
    // 03/29/12   A Hockman    Added a set for cs guideline year
    // 11/06/19  GVandy        CQ66067     2020 Worksheet Changes
    // ---------------------------------------------
    local.Current.Timestamp = Now();

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!IsEmpty(import.ParentACsePerson.Number))
    {
      if (!ReadCaseRole1())
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    if (!IsEmpty(import.ParentBCsePerson.Number))
    {
      if (!ReadCaseRole2())
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    ReadChildSupportWorksheet();
    local.Work.Count = (int)(entities.ChildSupportWorksheet.Identifier + 1);

    // 11/19/07  M. Fan   WR318566(CQ297)- Added a SET statement for 
    // parenting_time_adj_precent.
    try
    {
      CreateChildSupportWorksheet();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

      if (!IsEmpty(import.LegalAction.CourtCaseNumber))
      {
        if (ReadLegalAction())
        {
          AssociateLegalAction();
        }
      }

      export.ChildSupportWorksheet.Assign(entities.ChildSupportWorksheet);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CHILD_SUPPORT_WORKSHEET_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CHILD_SUPPORT_WORKSHEET_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ---------------------------------------------
    // Create Parent A CSE Person Support Worksheet.
    // ---------------------------------------------
    if (entities.ParentACaseRole.Populated)
    {
      if (AsChar(import.Gimport2020EnterableFields.GimportAbilityToPayParent.
        SelectChar) == '1')
      {
        local.CsePersonSupportWorksheet.AbilityToPay = "Y";
      }
      else
      {
        local.CsePersonSupportWorksheet.AbilityToPay = "N";
      }

      try
      {
        CreateCsePersonSupportWorksheet1();
        MoveCsePersonSupportWorksheet(entities.ParentACsePersonSupportWorksheet,
          export.ParentA);

        // ---------------------------------------------
        // Create the Adjustment  Amount.
        // ---------------------------------------------
        // 06/04/10  J. Huss	CQ# 18769	Modified logic to not write zero value 
        // records to the database.
        foreach(var item in ReadChildSupportAdjustment())
        {
          if (entities.ChildSupportWorksheet.CsGuidelineYear >= 2020)
          {
            // --Parenting Time Adjustment was moved out of the "child support 
            // adjustments" section on the 2020 worksheet.
            switch(entities.ChildSupportAdjustment.Number)
            {
              case 1:
                import.Import1.Index =
                  entities.ChildSupportAdjustment.Number - 1;
                import.Import1.CheckSize();

                local.CsePersonSupportAdjustment.AdjustmentAmount =
                  import.Import1.Item.ParentA.AdjustmentAmount.
                    GetValueOrDefault();

                break;
              case 2:
                if (AsChar(import.Gimport2020EnterableFields.
                  GimportParentingTime.SelectChar) == '1')
                {
                  local.CsePersonSupportAdjustment.AdjustmentAmount =
                    import.Gimport2020EnterableFields.GimportParentingTime.
                      TotalCurrency;
                }
                else
                {
                  local.CsePersonSupportAdjustment.AdjustmentAmount = 0;
                }

                break;
              default:
                import.Import1.Index =
                  entities.ChildSupportAdjustment.Number - 2;
                import.Import1.CheckSize();

                local.CsePersonSupportAdjustment.AdjustmentAmount =
                  import.Import1.Item.ParentA.AdjustmentAmount.
                    GetValueOrDefault();

                break;
            }
          }
          else
          {
            import.Import1.Index = entities.ChildSupportAdjustment.Number - 1;
            import.Import1.CheckSize();

            local.CsePersonSupportAdjustment.AdjustmentAmount =
              import.Import1.Item.ParentA.AdjustmentAmount.GetValueOrDefault();
          }

          if (local.CsePersonSupportAdjustment.AdjustmentAmount.
            GetValueOrDefault() != 0)
          {
            try
            {
              CreateCsePersonSupportAdjustment1();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CSE_PERSON_SUPPORT_ADJUSTMENT_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CSE_PERSON_SUPPORT_ADJUSTMENT_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CSE_PERSON_SUPPORT_WORKSHEET_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CSE_PERSON_SUPPORT_WORKSHEET_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ---------------------------------------------
    // Create Parent B CSE Person Support Worksheet.
    // ---------------------------------------------
    if (entities.ParentBCaseRole.Populated)
    {
      if (AsChar(import.Gimport2020EnterableFields.GimportAbilityToPayParent.
        SelectChar) == '2')
      {
        local.CsePersonSupportWorksheet.AbilityToPay = "Y";
      }
      else
      {
        local.CsePersonSupportWorksheet.AbilityToPay = "N";
      }

      try
      {
        CreateCsePersonSupportWorksheet2();
        MoveCsePersonSupportWorksheet(entities.ParentBCsePersonSupportWorksheet,
          export.ParentB);

        // ---------------------------------------------
        // Create the Adjustment  Amount.
        // ---------------------------------------------
        // 06/04/10  J. Huss	CQ# 18769	Modified logic to not write zero value 
        // records to the database.
        foreach(var item in ReadChildSupportAdjustment())
        {
          if (entities.ChildSupportWorksheet.CsGuidelineYear >= 2020)
          {
            // --Parenting Time Adjustment was moved out of the "child support 
            // adjustments" section on the 2020 worksheet.
            switch(entities.ChildSupportAdjustment.Number)
            {
              case 1:
                import.Import1.Index =
                  entities.ChildSupportAdjustment.Number - 1;
                import.Import1.CheckSize();

                local.CsePersonSupportAdjustment.AdjustmentAmount =
                  import.Import1.Item.ParentB.AdjustmentAmount.
                    GetValueOrDefault();

                break;
              case 2:
                if (AsChar(import.Gimport2020EnterableFields.
                  GimportParentingTime.SelectChar) == '2')
                {
                  local.CsePersonSupportAdjustment.AdjustmentAmount =
                    import.Gimport2020EnterableFields.GimportParentingTime.
                      TotalCurrency;
                }
                else
                {
                  local.CsePersonSupportAdjustment.AdjustmentAmount = 0;
                }

                break;
              default:
                import.Import1.Index =
                  entities.ChildSupportAdjustment.Number - 2;
                import.Import1.CheckSize();

                local.CsePersonSupportAdjustment.AdjustmentAmount =
                  import.Import1.Item.ParentB.AdjustmentAmount.
                    GetValueOrDefault();

                break;
            }
          }
          else
          {
            import.Import1.Index = entities.ChildSupportAdjustment.Number - 1;
            import.Import1.CheckSize();

            local.CsePersonSupportAdjustment.AdjustmentAmount =
              import.Import1.Item.ParentB.AdjustmentAmount.GetValueOrDefault();
          }

          if (local.CsePersonSupportAdjustment.AdjustmentAmount.
            GetValueOrDefault() != 0)
          {
            try
            {
              CreateCsePersonSupportAdjustment2();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CSE_PERSON_SUPPORT_ADJUSTMENT_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CSE_PERSON_SUPPORT_ADJUSTMENT_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CSE_PERSON_SUPPORT_WORKSHEET_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CSE_PERSON_SUPPORT_WORKSHEET_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveCsePersonSupportWorksheet(
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
    target.EnforcementFeeType = source.EnforcementFeeType;
    target.EnforcementFeeAllowance = source.EnforcementFeeAllowance;
    target.InsuranceWorkRelatedCcCredit = source.InsuranceWorkRelatedCcCredit;
    target.AbilityToPay = source.AbilityToPay;
    target.EqualParentingTimeObligation = source.EqualParentingTimeObligation;
    target.SocialSecDependentBenefit = source.SocialSecDependentBenefit;
  }

  private void AssociateLegalAction()
  {
    var lgaIdentifier = entities.LegalAction.Identifier;

    entities.ChildSupportWorksheet.Populated = false;
    Update("AssociateLegalAction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetInt64(
          command, "identifier", entities.ChildSupportWorksheet.Identifier);
        db.SetInt32(
          command, "csGuidelineYear",
          entities.ChildSupportWorksheet.CsGuidelineYear);
      });

    entities.ChildSupportWorksheet.LgaIdentifier = lgaIdentifier;
    entities.ChildSupportWorksheet.Populated = true;
  }

  private void CreateChildSupportWorksheet()
  {
    var identifier = local.Work.Count;
    var noOfChildrenInAgeGrp3 =
      import.ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault();
    var noOfChildrenInAgeGrp2 =
      import.ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault();
    var noOfChildrenInAgeGrp1 =
      import.ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault();
    var additionalNoOfChildren =
      import.ChildSupportWorksheet.AdditionalNoOfChildren.GetValueOrDefault();
    var status = import.ChildSupportWorksheet.Status;
    var costOfLivingDiffAdjInd =
      import.ChildSupportWorksheet.CostOfLivingDiffAdjInd ?? "";
    var multipleFamilyAdjInd =
      import.ChildSupportWorksheet.MultipleFamilyAdjInd ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var authorizingAuthority =
      import.ChildSupportWorksheet.AuthorizingAuthority ?? "";
    var parentingTimeAdjPercent =
      import.ChildSupportWorksheet.ParentingTimeAdjPercent.GetValueOrDefault();
    var csGuidelineYear = import.ChildSupportWorksheet.CsGuidelineYear;

    entities.ChildSupportWorksheet.Populated = false;
    Update("CreateChildSupportWorksheet",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableInt32(command, "noOfChGrp3", noOfChildrenInAgeGrp3);
        db.SetNullableInt32(command, "noOfChGrp2", noOfChildrenInAgeGrp2);
        db.SetNullableInt32(command, "noOfChGrp1", noOfChildrenInAgeGrp1);
        db.SetNullableInt32(command, "addlNoChildren", additionalNoOfChildren);
        db.SetString(command, "status", status);
        db.SetNullableString(command, "colDiffAdjInd", costOfLivingDiffAdjInd);
        db.SetNullableString(command, "multFamAdjInd", multipleFamilyAdjInd);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.
          SetNullableString(command, "authorizingAthrty", authorizingAuthority);
          
        db.
          SetNullableInt32(command, "parntTimeAdjPct", parentingTimeAdjPercent);
          
        db.SetInt32(command, "csGuidelineYear", csGuidelineYear);
      });

    entities.ChildSupportWorksheet.Identifier = identifier;
    entities.ChildSupportWorksheet.LgaIdentifier = null;
    entities.ChildSupportWorksheet.NoOfChildrenInAgeGrp3 =
      noOfChildrenInAgeGrp3;
    entities.ChildSupportWorksheet.NoOfChildrenInAgeGrp2 =
      noOfChildrenInAgeGrp2;
    entities.ChildSupportWorksheet.NoOfChildrenInAgeGrp1 =
      noOfChildrenInAgeGrp1;
    entities.ChildSupportWorksheet.AdditionalNoOfChildren =
      additionalNoOfChildren;
    entities.ChildSupportWorksheet.Status = status;
    entities.ChildSupportWorksheet.CostOfLivingDiffAdjInd =
      costOfLivingDiffAdjInd;
    entities.ChildSupportWorksheet.MultipleFamilyAdjInd = multipleFamilyAdjInd;
    entities.ChildSupportWorksheet.CreatedBy = createdBy;
    entities.ChildSupportWorksheet.CreatedTimestamp = createdTimestamp;
    entities.ChildSupportWorksheet.LastUpdatedBy = createdBy;
    entities.ChildSupportWorksheet.LastUpdatedTimestamp = createdTimestamp;
    entities.ChildSupportWorksheet.AuthorizingAuthority = authorizingAuthority;
    entities.ChildSupportWorksheet.ParentingTimeAdjPercent =
      parentingTimeAdjPercent;
    entities.ChildSupportWorksheet.CsGuidelineYear = csGuidelineYear;
    entities.ChildSupportWorksheet.Populated = true;
  }

  private void CreateCsePersonSupportAdjustment1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ParentACsePersonSupportWorksheet.Populated);

    var croIdentifier = entities.ParentACsePersonSupportWorksheet.CroIdentifier;
    var adjustmentAmount =
      local.CsePersonSupportAdjustment.AdjustmentAmount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var csdNumber = entities.ChildSupportAdjustment.Number;
    var cpsIdentifier = entities.ParentACsePersonSupportWorksheet.Identifer;
    var casNumber = entities.ParentACsePersonSupportWorksheet.CasNumber;
    var cspNumber = entities.ParentACsePersonSupportWorksheet.CspNumber;
    var croType = entities.ParentACsePersonSupportWorksheet.CroType;
    var cswIdentifier = entities.ParentACsePersonSupportWorksheet.CswIdentifier;
    var cssGuidelineYr =
      entities.ParentACsePersonSupportWorksheet.CssGuidelineYr;

    CheckValid<CsePersonSupportAdjustment>("CroType", croType);
    entities.ParentACsePersonSupportAdjustment.Populated = false;
    Update("CreateCsePersonSupportAdjustment1",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", croIdentifier);
        db.SetNullableDecimal(command, "adjustmentAmount", adjustmentAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "csdNumber", csdNumber);
        db.SetInt32(command, "cpsIdentifier", cpsIdentifier);
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt64(command, "cswIdentifier", cswIdentifier);
        db.SetInt32(command, "cssGuidelineYr", cssGuidelineYr);
      });

    entities.ParentACsePersonSupportAdjustment.CroIdentifier = croIdentifier;
    entities.ParentACsePersonSupportAdjustment.AdjustmentAmount =
      adjustmentAmount;
    entities.ParentACsePersonSupportAdjustment.CreatedBy = createdBy;
    entities.ParentACsePersonSupportAdjustment.CreatedTimestamp =
      createdTimestamp;
    entities.ParentACsePersonSupportAdjustment.LastUpdatedBy = createdBy;
    entities.ParentACsePersonSupportAdjustment.LastUpdatedTimestamp =
      createdTimestamp;
    entities.ParentACsePersonSupportAdjustment.CsdNumber = csdNumber;
    entities.ParentACsePersonSupportAdjustment.CpsIdentifier = cpsIdentifier;
    entities.ParentACsePersonSupportAdjustment.CasNumber = casNumber;
    entities.ParentACsePersonSupportAdjustment.CspNumber = cspNumber;
    entities.ParentACsePersonSupportAdjustment.CroType = croType;
    entities.ParentACsePersonSupportAdjustment.CswIdentifier = cswIdentifier;
    entities.ParentACsePersonSupportAdjustment.CssGuidelineYr = cssGuidelineYr;
    entities.ParentACsePersonSupportAdjustment.Populated = true;
  }

  private void CreateCsePersonSupportAdjustment2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ParentBCsePersonSupportWorksheet.Populated);

    var croIdentifier = entities.ParentBCsePersonSupportWorksheet.CroIdentifier;
    var adjustmentAmount =
      local.CsePersonSupportAdjustment.AdjustmentAmount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var csdNumber = entities.ChildSupportAdjustment.Number;
    var cpsIdentifier = entities.ParentBCsePersonSupportWorksheet.Identifer;
    var casNumber = entities.ParentBCsePersonSupportWorksheet.CasNumber;
    var cspNumber = entities.ParentBCsePersonSupportWorksheet.CspNumber;
    var croType = entities.ParentBCsePersonSupportWorksheet.CroType;
    var cswIdentifier = entities.ParentBCsePersonSupportWorksheet.CswIdentifier;
    var cssGuidelineYr =
      entities.ParentBCsePersonSupportWorksheet.CssGuidelineYr;

    CheckValid<CsePersonSupportAdjustment>("CroType", croType);
    entities.ParentBCsePersonSupportAdjustment.Populated = false;
    Update("CreateCsePersonSupportAdjustment2",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", croIdentifier);
        db.SetNullableDecimal(command, "adjustmentAmount", adjustmentAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "csdNumber", csdNumber);
        db.SetInt32(command, "cpsIdentifier", cpsIdentifier);
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt64(command, "cswIdentifier", cswIdentifier);
        db.SetInt32(command, "cssGuidelineYr", cssGuidelineYr);
      });

    entities.ParentBCsePersonSupportAdjustment.CroIdentifier = croIdentifier;
    entities.ParentBCsePersonSupportAdjustment.AdjustmentAmount =
      adjustmentAmount;
    entities.ParentBCsePersonSupportAdjustment.CreatedBy = createdBy;
    entities.ParentBCsePersonSupportAdjustment.CreatedTimestamp =
      createdTimestamp;
    entities.ParentBCsePersonSupportAdjustment.LastUpdatedBy = createdBy;
    entities.ParentBCsePersonSupportAdjustment.LastUpdatedTimestamp =
      createdTimestamp;
    entities.ParentBCsePersonSupportAdjustment.CsdNumber = csdNumber;
    entities.ParentBCsePersonSupportAdjustment.CpsIdentifier = cpsIdentifier;
    entities.ParentBCsePersonSupportAdjustment.CasNumber = casNumber;
    entities.ParentBCsePersonSupportAdjustment.CspNumber = cspNumber;
    entities.ParentBCsePersonSupportAdjustment.CroType = croType;
    entities.ParentBCsePersonSupportAdjustment.CswIdentifier = cswIdentifier;
    entities.ParentBCsePersonSupportAdjustment.CssGuidelineYr = cssGuidelineYr;
    entities.ParentBCsePersonSupportAdjustment.Populated = true;
  }

  private void CreateCsePersonSupportWorksheet1()
  {
    System.Diagnostics.Debug.Assert(entities.ParentACaseRole.Populated);

    var croIdentifier = entities.ParentACaseRole.Identifier;
    var identifer = 1;
    var noOfChildrenInDayCare =
      import.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare.
        GetValueOrDefault();
    var workRelatedChildCareCosts =
      import.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts.
        GetValueOrDefault();
    var wageEarnerGrossIncome =
      import.ParentACsePersonSupportWorksheet.WageEarnerGrossIncome.
        GetValueOrDefault();
    var selfEmploymentGrossIncome =
      import.ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
        GetValueOrDefault();
    var reasonableBusinessExpense =
      import.ParentACsePersonSupportWorksheet.ReasonableBusinessExpense.
        GetValueOrDefault();
    var courtOrderedChildSupportPaid =
      import.ParentACsePersonSupportWorksheet.CourtOrderedChildSupportPaid.
        GetValueOrDefault();
    var childSupprtPaidCourtOrderNo =
      import.ParentACsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo ?? ""
      ;
    var courtOrderedMaintenancePaid =
      import.ParentACsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
        GetValueOrDefault();
    var maintenancePaidCourtOrderNo =
      import.ParentACsePersonSupportWorksheet.MaintenancePaidCourtOrderNo ?? ""
      ;
    var courtOrderedMaintenanceRecvd =
      import.ParentACsePersonSupportWorksheet.CourtOrderedMaintenanceRecvd.
        GetValueOrDefault();
    var maintenanceRecvdCourtOrderNo =
      import.ParentACsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo ?? ""
      ;
    var healthAndDentalInsurancePrem =
      import.ParentACsePersonSupportWorksheet.HealthAndDentalInsurancePrem.
        GetValueOrDefault();
    var eligibleForFederalTaxCredit =
      import.ParentACsePersonSupportWorksheet.EligibleForFederalTaxCredit ?? ""
      ;
    var eligibleForKansasTaxCredit =
      import.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit ?? "";
    var netAdjParentalChildSuppAmt =
      import.ParentACsePersonSupportWorksheet.NetAdjParentalChildSuppAmt.
        GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var enforcementFeeType =
      import.ParentACsePersonSupportWorksheet.EnforcementFeeType ?? "";
    var enforcementFeeAllowance =
      import.ParentACsePersonSupportWorksheet.EnforcementFeeAllowance.
        GetValueOrDefault();
    var cswIdentifier = entities.ChildSupportWorksheet.Identifier;
    var cspNumber = entities.ParentACaseRole.CspNumber;
    var casNumber = entities.ParentACaseRole.CasNumber;
    var croType = entities.ParentACaseRole.Type1;
    var cssGuidelineYr = entities.ChildSupportWorksheet.CsGuidelineYear;
    var insuranceWorkRelatedCcCredit =
      import.ParentACsePersonSupportWorksheet.InsuranceWorkRelatedCcCredit.
        GetValueOrDefault();
    var abilityToPay = local.CsePersonSupportWorksheet.AbilityToPay ?? "";
    var equalParentingTimeObligation =
      import.ParentACsePersonSupportWorksheet.EqualParentingTimeObligation.
        GetValueOrDefault();
    var socialSecDependentBenefit =
      import.ParentACsePersonSupportWorksheet.SocialSecDependentBenefit.
        GetValueOrDefault();

    CheckValid<CsePersonSupportWorksheet>("CroType", croType);
    entities.ParentACsePersonSupportWorksheet.Populated = false;
    Update("CreateCsePersonSupportWorksheet1",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", croIdentifier);
        db.SetInt32(command, "identifer", identifer);
        db.SetNullableInt32(command, "noChInDayCare", noOfChildrenInDayCare);
        db.SetNullableDecimal(
          command, "wrkRelChcarecost", workRelatedChildCareCosts);
        db.
          SetNullableDecimal(command, "wageEarnerGrInc", wageEarnerGrossIncome);
          
        db.SetNullableDecimal(
          command, "selfEmpGrossInc", selfEmploymentGrossIncome);
        db.SetNullableDecimal(
          command, "reasonableBusexp", reasonableBusinessExpense);
        db.SetNullableDecimal(
          command, "ctordChsuppPaid", courtOrderedChildSupportPaid);
        db.SetNullableString(
          command, "csPaidCtOrdNo", childSupprtPaidCourtOrderNo);
        db.SetNullableDecimal(
          command, "ctordMaintPaid", courtOrderedMaintenancePaid);
        db.SetNullableString(
          command, "maintPdCtordNo", maintenancePaidCourtOrderNo);
        db.SetNullableDecimal(
          command, "ctordMaintRcvd", courtOrderedMaintenanceRecvd);
        db.SetNullableString(
          command, "maintRdCtordNo", maintenanceRecvdCourtOrderNo);
        db.SetNullableDecimal(
          command, "hlthDntlInsPrem", healthAndDentalInsurancePrem);
        db.SetNullableString(
          command, "eligFedTaxCr", eligibleForFederalTaxCredit);
        db.
          SetNullableString(command, "eligKsTaxCr", eligibleForKansasTaxCredit);
          
        db.SetNullableDecimal(
          command, "netAdjPrtlCsamt", netAdjParentalChildSuppAmt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "enfrcmtFeeTyp", enforcementFeeType);
        db.
          SetNullableInt32(command, "enfrcmtFeeAllwn", enforcementFeeAllowance);
          
        db.SetInt64(command, "cswIdentifier", cswIdentifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "cssGuidelineYr", cssGuidelineYr);
        db.SetNullableDecimal(
          command, "insWrCcCredit", insuranceWorkRelatedCcCredit);
        db.SetNullableString(command, "abilityToPay", abilityToPay);
        db.SetNullableDecimal(
          command, "eqParentTimeObg", equalParentingTimeObligation);
        db.SetNullableDecimal(
          command, "ssDepndntBenefit", socialSecDependentBenefit);
      });

    entities.ParentACsePersonSupportWorksheet.CroIdentifier = croIdentifier;
    entities.ParentACsePersonSupportWorksheet.Identifer = identifer;
    entities.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare =
      noOfChildrenInDayCare;
    entities.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts =
      workRelatedChildCareCosts;
    entities.ParentACsePersonSupportWorksheet.WageEarnerGrossIncome =
      wageEarnerGrossIncome;
    entities.ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome =
      selfEmploymentGrossIncome;
    entities.ParentACsePersonSupportWorksheet.ReasonableBusinessExpense =
      reasonableBusinessExpense;
    entities.ParentACsePersonSupportWorksheet.CourtOrderedChildSupportPaid =
      courtOrderedChildSupportPaid;
    entities.ParentACsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo =
      childSupprtPaidCourtOrderNo;
    entities.ParentACsePersonSupportWorksheet.CourtOrderedMaintenancePaid =
      courtOrderedMaintenancePaid;
    entities.ParentACsePersonSupportWorksheet.MaintenancePaidCourtOrderNo =
      maintenancePaidCourtOrderNo;
    entities.ParentACsePersonSupportWorksheet.CourtOrderedMaintenanceRecvd =
      courtOrderedMaintenanceRecvd;
    entities.ParentACsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo =
      maintenanceRecvdCourtOrderNo;
    entities.ParentACsePersonSupportWorksheet.HealthAndDentalInsurancePrem =
      healthAndDentalInsurancePrem;
    entities.ParentACsePersonSupportWorksheet.EligibleForFederalTaxCredit =
      eligibleForFederalTaxCredit;
    entities.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit =
      eligibleForKansasTaxCredit;
    entities.ParentACsePersonSupportWorksheet.NetAdjParentalChildSuppAmt =
      netAdjParentalChildSuppAmt;
    entities.ParentACsePersonSupportWorksheet.CreatedBy = createdBy;
    entities.ParentACsePersonSupportWorksheet.CreatedTimestamp =
      createdTimestamp;
    entities.ParentACsePersonSupportWorksheet.LastUpdatedBy = createdBy;
    entities.ParentACsePersonSupportWorksheet.LastUpdatedTimestamp =
      createdTimestamp;
    entities.ParentACsePersonSupportWorksheet.EnforcementFeeType =
      enforcementFeeType;
    entities.ParentACsePersonSupportWorksheet.EnforcementFeeAllowance =
      enforcementFeeAllowance;
    entities.ParentACsePersonSupportWorksheet.CswIdentifier = cswIdentifier;
    entities.ParentACsePersonSupportWorksheet.CspNumber = cspNumber;
    entities.ParentACsePersonSupportWorksheet.CasNumber = casNumber;
    entities.ParentACsePersonSupportWorksheet.CroType = croType;
    entities.ParentACsePersonSupportWorksheet.CssGuidelineYr = cssGuidelineYr;
    entities.ParentACsePersonSupportWorksheet.InsuranceWorkRelatedCcCredit =
      insuranceWorkRelatedCcCredit;
    entities.ParentACsePersonSupportWorksheet.AbilityToPay = abilityToPay;
    entities.ParentACsePersonSupportWorksheet.EqualParentingTimeObligation =
      equalParentingTimeObligation;
    entities.ParentACsePersonSupportWorksheet.SocialSecDependentBenefit =
      socialSecDependentBenefit;
    entities.ParentACsePersonSupportWorksheet.Populated = true;
  }

  private void CreateCsePersonSupportWorksheet2()
  {
    System.Diagnostics.Debug.Assert(entities.ParentBCaseRole.Populated);

    var croIdentifier = entities.ParentBCaseRole.Identifier;
    var identifer = 2;
    var noOfChildrenInDayCare =
      import.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare.
        GetValueOrDefault();
    var workRelatedChildCareCosts =
      import.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts.
        GetValueOrDefault();
    var wageEarnerGrossIncome =
      import.ParentBCsePersonSupportWorksheet.WageEarnerGrossIncome.
        GetValueOrDefault();
    var selfEmploymentGrossIncome =
      import.ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
        GetValueOrDefault();
    var reasonableBusinessExpense =
      import.ParentBCsePersonSupportWorksheet.ReasonableBusinessExpense.
        GetValueOrDefault();
    var courtOrderedChildSupportPaid =
      import.ParentBCsePersonSupportWorksheet.CourtOrderedChildSupportPaid.
        GetValueOrDefault();
    var childSupprtPaidCourtOrderNo =
      import.ParentBCsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo ?? ""
      ;
    var courtOrderedMaintenancePaid =
      import.ParentBCsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
        GetValueOrDefault();
    var maintenancePaidCourtOrderNo =
      import.ParentBCsePersonSupportWorksheet.MaintenancePaidCourtOrderNo ?? ""
      ;
    var courtOrderedMaintenanceRecvd =
      import.ParentBCsePersonSupportWorksheet.CourtOrderedMaintenanceRecvd.
        GetValueOrDefault();
    var maintenanceRecvdCourtOrderNo =
      import.ParentBCsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo ?? ""
      ;
    var healthAndDentalInsurancePrem =
      import.ParentBCsePersonSupportWorksheet.HealthAndDentalInsurancePrem.
        GetValueOrDefault();
    var eligibleForFederalTaxCredit =
      import.ParentBCsePersonSupportWorksheet.EligibleForFederalTaxCredit ?? ""
      ;
    var eligibleForKansasTaxCredit =
      import.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit ?? "";
    var netAdjParentalChildSuppAmt =
      import.ParentBCsePersonSupportWorksheet.NetAdjParentalChildSuppAmt.
        GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var enforcementFeeType =
      import.ParentBCsePersonSupportWorksheet.EnforcementFeeType ?? "";
    var enforcementFeeAllowance =
      import.ParentBCsePersonSupportWorksheet.EnforcementFeeAllowance.
        GetValueOrDefault();
    var cswIdentifier = entities.ChildSupportWorksheet.Identifier;
    var cspNumber = entities.ParentBCaseRole.CspNumber;
    var casNumber = entities.ParentBCaseRole.CasNumber;
    var croType = entities.ParentBCaseRole.Type1;
    var cssGuidelineYr = entities.ChildSupportWorksheet.CsGuidelineYear;
    var insuranceWorkRelatedCcCredit =
      import.ParentBCsePersonSupportWorksheet.InsuranceWorkRelatedCcCredit.
        GetValueOrDefault();
    var abilityToPay = local.CsePersonSupportWorksheet.AbilityToPay ?? "";
    var equalParentingTimeObligation =
      import.ParentBCsePersonSupportWorksheet.EqualParentingTimeObligation.
        GetValueOrDefault();
    var socialSecDependentBenefit =
      import.ParentBCsePersonSupportWorksheet.SocialSecDependentBenefit.
        GetValueOrDefault();

    CheckValid<CsePersonSupportWorksheet>("CroType", croType);
    entities.ParentBCsePersonSupportWorksheet.Populated = false;
    Update("CreateCsePersonSupportWorksheet2",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", croIdentifier);
        db.SetInt32(command, "identifer", identifer);
        db.SetNullableInt32(command, "noChInDayCare", noOfChildrenInDayCare);
        db.SetNullableDecimal(
          command, "wrkRelChcarecost", workRelatedChildCareCosts);
        db.
          SetNullableDecimal(command, "wageEarnerGrInc", wageEarnerGrossIncome);
          
        db.SetNullableDecimal(
          command, "selfEmpGrossInc", selfEmploymentGrossIncome);
        db.SetNullableDecimal(
          command, "reasonableBusexp", reasonableBusinessExpense);
        db.SetNullableDecimal(
          command, "ctordChsuppPaid", courtOrderedChildSupportPaid);
        db.SetNullableString(
          command, "csPaidCtOrdNo", childSupprtPaidCourtOrderNo);
        db.SetNullableDecimal(
          command, "ctordMaintPaid", courtOrderedMaintenancePaid);
        db.SetNullableString(
          command, "maintPdCtordNo", maintenancePaidCourtOrderNo);
        db.SetNullableDecimal(
          command, "ctordMaintRcvd", courtOrderedMaintenanceRecvd);
        db.SetNullableString(
          command, "maintRdCtordNo", maintenanceRecvdCourtOrderNo);
        db.SetNullableDecimal(
          command, "hlthDntlInsPrem", healthAndDentalInsurancePrem);
        db.SetNullableString(
          command, "eligFedTaxCr", eligibleForFederalTaxCredit);
        db.
          SetNullableString(command, "eligKsTaxCr", eligibleForKansasTaxCredit);
          
        db.SetNullableDecimal(
          command, "netAdjPrtlCsamt", netAdjParentalChildSuppAmt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "enfrcmtFeeTyp", enforcementFeeType);
        db.
          SetNullableInt32(command, "enfrcmtFeeAllwn", enforcementFeeAllowance);
          
        db.SetInt64(command, "cswIdentifier", cswIdentifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "cssGuidelineYr", cssGuidelineYr);
        db.SetNullableDecimal(
          command, "insWrCcCredit", insuranceWorkRelatedCcCredit);
        db.SetNullableString(command, "abilityToPay", abilityToPay);
        db.SetNullableDecimal(
          command, "eqParentTimeObg", equalParentingTimeObligation);
        db.SetNullableDecimal(
          command, "ssDepndntBenefit", socialSecDependentBenefit);
      });

    entities.ParentBCsePersonSupportWorksheet.CroIdentifier = croIdentifier;
    entities.ParentBCsePersonSupportWorksheet.Identifer = identifer;
    entities.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare =
      noOfChildrenInDayCare;
    entities.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts =
      workRelatedChildCareCosts;
    entities.ParentBCsePersonSupportWorksheet.WageEarnerGrossIncome =
      wageEarnerGrossIncome;
    entities.ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome =
      selfEmploymentGrossIncome;
    entities.ParentBCsePersonSupportWorksheet.ReasonableBusinessExpense =
      reasonableBusinessExpense;
    entities.ParentBCsePersonSupportWorksheet.CourtOrderedChildSupportPaid =
      courtOrderedChildSupportPaid;
    entities.ParentBCsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo =
      childSupprtPaidCourtOrderNo;
    entities.ParentBCsePersonSupportWorksheet.CourtOrderedMaintenancePaid =
      courtOrderedMaintenancePaid;
    entities.ParentBCsePersonSupportWorksheet.MaintenancePaidCourtOrderNo =
      maintenancePaidCourtOrderNo;
    entities.ParentBCsePersonSupportWorksheet.CourtOrderedMaintenanceRecvd =
      courtOrderedMaintenanceRecvd;
    entities.ParentBCsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo =
      maintenanceRecvdCourtOrderNo;
    entities.ParentBCsePersonSupportWorksheet.HealthAndDentalInsurancePrem =
      healthAndDentalInsurancePrem;
    entities.ParentBCsePersonSupportWorksheet.EligibleForFederalTaxCredit =
      eligibleForFederalTaxCredit;
    entities.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit =
      eligibleForKansasTaxCredit;
    entities.ParentBCsePersonSupportWorksheet.NetAdjParentalChildSuppAmt =
      netAdjParentalChildSuppAmt;
    entities.ParentBCsePersonSupportWorksheet.CreatedBy = createdBy;
    entities.ParentBCsePersonSupportWorksheet.CreatedTimestamp =
      createdTimestamp;
    entities.ParentBCsePersonSupportWorksheet.LastUpdatedBy = createdBy;
    entities.ParentBCsePersonSupportWorksheet.LastUpdatedTimestamp =
      createdTimestamp;
    entities.ParentBCsePersonSupportWorksheet.EnforcementFeeType =
      enforcementFeeType;
    entities.ParentBCsePersonSupportWorksheet.EnforcementFeeAllowance =
      enforcementFeeAllowance;
    entities.ParentBCsePersonSupportWorksheet.CswIdentifier = cswIdentifier;
    entities.ParentBCsePersonSupportWorksheet.CspNumber = cspNumber;
    entities.ParentBCsePersonSupportWorksheet.CasNumber = casNumber;
    entities.ParentBCsePersonSupportWorksheet.CroType = croType;
    entities.ParentBCsePersonSupportWorksheet.CssGuidelineYr = cssGuidelineYr;
    entities.ParentBCsePersonSupportWorksheet.InsuranceWorkRelatedCcCredit =
      insuranceWorkRelatedCcCredit;
    entities.ParentBCsePersonSupportWorksheet.AbilityToPay = abilityToPay;
    entities.ParentBCsePersonSupportWorksheet.EqualParentingTimeObligation =
      equalParentingTimeObligation;
    entities.ParentBCsePersonSupportWorksheet.SocialSecDependentBenefit =
      socialSecDependentBenefit;
    entities.ParentBCsePersonSupportWorksheet.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.ParentACaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", import.ParentACsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ParentACaseRole.CasNumber = db.GetString(reader, 0);
        entities.ParentACaseRole.CspNumber = db.GetString(reader, 1);
        entities.ParentACaseRole.Type1 = db.GetString(reader, 2);
        entities.ParentACaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ParentACaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ParentACaseRole.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.ParentBCaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", import.ParentBCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ParentBCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ParentBCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ParentBCaseRole.Type1 = db.GetString(reader, 2);
        entities.ParentBCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ParentBCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ParentBCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadChildSupportAdjustment()
  {
    entities.ChildSupportAdjustment.Populated = false;

    return ReadEach("ReadChildSupportAdjustment",
      null,
      (db, reader) =>
      {
        entities.ChildSupportAdjustment.Number = db.GetInt32(reader, 0);
        entities.ChildSupportAdjustment.AdjustmentType =
          db.GetString(reader, 1);
        entities.ChildSupportAdjustment.Description =
          db.GetNullableString(reader, 2);
        entities.ChildSupportAdjustment.Populated = true;

        return true;
      });
  }

  private bool ReadChildSupportWorksheet()
  {
    entities.ChildSupportWorksheet.Populated = false;

    return Read("ReadChildSupportWorksheet",
      null,
      (db, reader) =>
      {
        entities.ChildSupportWorksheet.Identifier = db.GetInt64(reader, 0);
        entities.ChildSupportWorksheet.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.ChildSupportWorksheet.NoOfChildrenInAgeGrp3 =
          db.GetNullableInt32(reader, 2);
        entities.ChildSupportWorksheet.NoOfChildrenInAgeGrp2 =
          db.GetNullableInt32(reader, 3);
        entities.ChildSupportWorksheet.NoOfChildrenInAgeGrp1 =
          db.GetNullableInt32(reader, 4);
        entities.ChildSupportWorksheet.AdditionalNoOfChildren =
          db.GetNullableInt32(reader, 5);
        entities.ChildSupportWorksheet.Status = db.GetString(reader, 6);
        entities.ChildSupportWorksheet.CostOfLivingDiffAdjInd =
          db.GetNullableString(reader, 7);
        entities.ChildSupportWorksheet.MultipleFamilyAdjInd =
          db.GetNullableString(reader, 8);
        entities.ChildSupportWorksheet.CreatedBy = db.GetString(reader, 9);
        entities.ChildSupportWorksheet.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.ChildSupportWorksheet.LastUpdatedBy = db.GetString(reader, 11);
        entities.ChildSupportWorksheet.LastUpdatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ChildSupportWorksheet.AuthorizingAuthority =
          db.GetNullableString(reader, 13);
        entities.ChildSupportWorksheet.ParentingTimeAdjPercent =
          db.GetNullableInt32(reader, 14);
        entities.ChildSupportWorksheet.CsGuidelineYear =
          db.GetInt32(reader, 15);
        entities.ChildSupportWorksheet.Populated = true;
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
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 2);
        entities.LegalAction.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of ParentB.
      /// </summary>
      [JsonPropertyName("parentB")]
      public CsePersonSupportAdjustment ParentB
      {
        get => parentB ??= new();
        set => parentB = value;
      }

      /// <summary>
      /// A value of ParentA.
      /// </summary>
      [JsonPropertyName("parentA")]
      public CsePersonSupportAdjustment ParentA
      {
        get => parentA ??= new();
        set => parentA = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonSupportAdjustment parentB;
      private CsePersonSupportAdjustment parentA;
    }

    /// <summary>A Gimport2020EnterableFieldsGroup group.</summary>
    [Serializable]
    public class Gimport2020EnterableFieldsGroup
    {
      /// <summary>
      /// A value of GimportParentingTime.
      /// </summary>
      [JsonPropertyName("gimportParentingTime")]
      public Common GimportParentingTime
      {
        get => gimportParentingTime ??= new();
        set => gimportParentingTime = value;
      }

      /// <summary>
      /// A value of GimportAbilityToPayParent.
      /// </summary>
      [JsonPropertyName("gimportAbilityToPayParent")]
      public Common GimportAbilityToPayParent
      {
        get => gimportAbilityToPayParent ??= new();
        set => gimportAbilityToPayParent = value;
      }

      private Common gimportParentingTime;
      private Common gimportAbilityToPayParent;
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
    /// A value of ParentBCsePerson.
    /// </summary>
    [JsonPropertyName("parentBCsePerson")]
    public CsePerson ParentBCsePerson
    {
      get => parentBCsePerson ??= new();
      set => parentBCsePerson = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ParentACsePerson.
    /// </summary>
    [JsonPropertyName("parentACsePerson")]
    public CsePerson ParentACsePerson
    {
      get => parentACsePerson ??= new();
      set => parentACsePerson = value;
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
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// Gets a value of Gimport2020EnterableFields.
    /// </summary>
    [JsonPropertyName("gimport2020EnterableFields")]
    public Gimport2020EnterableFieldsGroup Gimport2020EnterableFields
    {
      get => gimport2020EnterableFields ?? (gimport2020EnterableFields = new());
      set => gimport2020EnterableFields = value;
    }

    private LegalAction legalAction;
    private CsePerson parentBCsePerson;
    private CsePersonSupportWorksheet parentBCsePersonSupportWorksheet;
    private Case1 case1;
    private CsePerson parentACsePerson;
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private ChildSupportWorksheet childSupportWorksheet;
    private Array<ImportGroup> import1;
    private Gimport2020EnterableFieldsGroup gimport2020EnterableFields;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ParentB.
    /// </summary>
    [JsonPropertyName("parentB")]
    public CsePersonSupportWorksheet ParentB
    {
      get => parentB ??= new();
      set => parentB = value;
    }

    /// <summary>
    /// A value of ParentA.
    /// </summary>
    [JsonPropertyName("parentA")]
    public CsePersonSupportWorksheet ParentA
    {
      get => parentA ??= new();
      set => parentA = value;
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

    private CsePersonSupportWorksheet parentB;
    private CsePersonSupportWorksheet parentA;
    private ChildSupportWorksheet childSupportWorksheet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("csePersonSupportWorksheet")]
    public CsePersonSupportWorksheet CsePersonSupportWorksheet
    {
      get => csePersonSupportWorksheet ??= new();
      set => csePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of CsePersonSupportAdjustment.
    /// </summary>
    [JsonPropertyName("csePersonSupportAdjustment")]
    public CsePersonSupportAdjustment CsePersonSupportAdjustment
    {
      get => csePersonSupportAdjustment ??= new();
      set => csePersonSupportAdjustment = value;
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    private CsePersonSupportWorksheet csePersonSupportWorksheet;
    private CsePersonSupportAdjustment csePersonSupportAdjustment;
    private DateWorkArea current;
    private Common work;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ChildSupportAdjustment.
    /// </summary>
    [JsonPropertyName("childSupportAdjustment")]
    public ChildSupportAdjustment ChildSupportAdjustment
    {
      get => childSupportAdjustment ??= new();
      set => childSupportAdjustment = value;
    }

    /// <summary>
    /// A value of ParentBCsePersonSupportAdjustment.
    /// </summary>
    [JsonPropertyName("parentBCsePersonSupportAdjustment")]
    public CsePersonSupportAdjustment ParentBCsePersonSupportAdjustment
    {
      get => parentBCsePersonSupportAdjustment ??= new();
      set => parentBCsePersonSupportAdjustment = value;
    }

    /// <summary>
    /// A value of ParentACsePersonSupportAdjustment.
    /// </summary>
    [JsonPropertyName("parentACsePersonSupportAdjustment")]
    public CsePersonSupportAdjustment ParentACsePersonSupportAdjustment
    {
      get => parentACsePersonSupportAdjustment ??= new();
      set => parentACsePersonSupportAdjustment = value;
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
    /// A value of ParentACsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("parentACsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet ParentACsePersonSupportWorksheet
    {
      get => parentACsePersonSupportWorksheet ??= new();
      set => parentACsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of ParentBCsePerson.
    /// </summary>
    [JsonPropertyName("parentBCsePerson")]
    public CsePerson ParentBCsePerson
    {
      get => parentBCsePerson ??= new();
      set => parentBCsePerson = value;
    }

    /// <summary>
    /// A value of ParentACsePerson.
    /// </summary>
    [JsonPropertyName("parentACsePerson")]
    public CsePerson ParentACsePerson
    {
      get => parentACsePerson ??= new();
      set => parentACsePerson = value;
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
    /// A value of ParentACaseRole.
    /// </summary>
    [JsonPropertyName("parentACaseRole")]
    public CaseRole ParentACaseRole
    {
      get => parentACaseRole ??= new();
      set => parentACaseRole = value;
    }

    /// <summary>
    /// A value of ParentBCaseRole.
    /// </summary>
    [JsonPropertyName("parentBCaseRole")]
    public CaseRole ParentBCaseRole
    {
      get => parentBCaseRole ??= new();
      set => parentBCaseRole = value;
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

    private LegalAction legalAction;
    private ChildSupportAdjustment childSupportAdjustment;
    private CsePersonSupportAdjustment parentBCsePersonSupportAdjustment;
    private CsePersonSupportAdjustment parentACsePersonSupportAdjustment;
    private CsePersonSupportWorksheet parentBCsePersonSupportWorksheet;
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private CsePerson parentBCsePerson;
    private CsePerson parentACsePerson;
    private Case1 case1;
    private CaseRole parentACaseRole;
    private CaseRole parentBCaseRole;
    private ChildSupportWorksheet childSupportWorksheet;
  }
#endregion
}
