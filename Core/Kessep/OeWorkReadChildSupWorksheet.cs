// Program: OE_WORK_READ_CHILD_SUP_WORKSHEET, ID: 371897942, model: 746.
// Short name: SWE00978
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
/// A program: OE_WORK_READ_CHILD_SUP_WORKSHEET.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeWorkReadChildSupWorksheet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_WORK_READ_CHILD_SUP_WORKSHEET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeWorkReadChildSupWorksheet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeWorkReadChildSupWorksheet.
  /// </summary>
  public OeWorkReadChildSupWorksheet(IContext context, Import import,
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
    // Date		Author		Description
    // 03/03/95	Sid		Initial Creation
    // 10/29/99	Srini Ganji	PR#H00078367 & 78428
    // If there is only Parent B Support Worksheet Details and Program trying to
    // read CASE with Current Parent A Case role, resulted Program abend with "
    // DB LAST STATUS = 'DU' ", Code changed to avoid such abends
    // 05/03/04	Andrew Convery	PR#207205 - Changed loading of data elements from
    // "A" to "M"
    // 				and "B" to "F".  This was causing confusion inasmuch as PR#198090/
    // 201861 changed the
    // 				entries in OE_WORK_CS_WORKSHEET_PAGE_3
    // 11/19/07	M. Fan		WR318566(CQ297)- Added the parenting time adjustment 
    // percent to exports and entity
    // 				actions for child support worksheet entity view.
    // 06/04/10	J. Huss		CQ# 18769 - Modified logic to handle non-existent Child
    // Support Adjustment records, rather
    // 				than expecting records with zero values.  Also removed references to 
    // Child Support Adjustment
    // 				Deviation Reason to allow for normalization of tables.
    // ---------------------------------------------
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
    // * 12/09/2015  GVandy             CQ50299     Change Enforcement Fee 
    // values from M/F   *
    // *
    // 
    // to 1/2.                                  *
    // *
    // 
    // *
    // * 11/06/2019  GVandy             CQ66067     2020 Worksheet Changes
    // *
    // *
    // 
    // *
    // ***************************************************************************************
    // ---------------------------------------------
    // This CAb is used to display the Child Support
    // Worksheet details.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------
    //        Move all IMPORTs to EXPORTs.
    // ---------------------------------------------
    export.ChildSupportWorksheet.Identifier =
      import.ChildSupportWorksheet.Identifier;

    // ---------------------------------------------
    // The required Child Support Worksheet has
    // already been selected from the List screen.
    // So the Child Support Worksheet Identifier is
    // already known.
    // ---------------------------------------------
    if (ReadChildSupportWorksheet())
    {
      export.ChildSupportWorksheet.Assign(entities.ChildSupportWorksheet);
    }
    else
    {
      ExitState = "CHILD_SUPPORT_WORKSHEET_NF";

      return;
    }

    // ---------------------------------------------
    // If the Legal Action Court Case Number is
    // present, READ the legal action details.
    // ---------------------------------------------
    if (ReadLegalAction())
    {
      MoveLegalAction(entities.LegalAction, export.LegalAction);
      UseOeWorkReadCourtOrderDetails();
    }

    // ---------------------------------------------
    // READ the Parent A Support Worksheet Details.
    // ---------------------------------------------
    // ---------------------------------------------
    // 10/29/99 Srini Ganji PR#H00078367
    // Statement added
    // ---------------------------------------------
    local.ParentA.Identifier = 0;

    if (ReadCsePersonSupportWorksheet1())
    {
      MoveCsePersonSupportWorksheet(entities.ParentACsePersonSupportWorksheet,
        export.ParentACsePersonSupportWorksheet);

      if (export.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts.
        GetValueOrDefault() == 0)
      {
        export.ParentACsePersonSupportWorksheet.EligibleForFederalTaxCredit =
          "";
        export.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit = "";
      }

      if (AsChar(entities.ParentACsePersonSupportWorksheet.EnforcementFeeType) ==
        'P')
      {
        // -- Change to Parent 1 from (M)other.  CQ50299
        export.Common.SelectChar = "1";
        export.Common.Percentage =
          entities.ParentACsePersonSupportWorksheet.EnforcementFeeAllowance.
            GetValueOrDefault();
      }
      else if (AsChar(entities.ParentACsePersonSupportWorksheet.
        EnforcementFeeType) == 'F')
      {
        // -- Change to Parent 1 from (M)other.  CQ50299
        export.Common.SelectChar = "1";
        export.Common.TotalCurrency =
          entities.ParentACsePersonSupportWorksheet.EnforcementFeeAllowance.
            GetValueOrDefault();
      }

      if (AsChar(entities.ParentACsePersonSupportWorksheet.AbilityToPay) == 'Y')
      {
        export.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar =
          "1";
      }

      if (ReadCsePersonCaseRole1())
      {
        // ---------------------------------------------
        // 10/29/99 Srini Ganji PR#H00078367
        // Statement added
        // ---------------------------------------------
        local.ParentA.Identifier = entities.ParentACaseRole.Identifier;
        export.ParentACsePerson.Number = entities.ParentACsePerson.Number;
        local.CsePersonsWorkSet.Number = export.ParentACsePerson.Number;
        UseSiReadCsePerson();
        export.ParentAName.Assign(local.CsePersonsWorkSet);
      }
    }

    // ---------------------------------------------
    // READ the Parent B Support Worksheet Details.
    // ---------------------------------------------
    // ---------------------------------------------
    // 10/29/99 Srini Ganji PR#H00078367
    // Statement added
    // ---------------------------------------------
    local.ParentB.Identifier = 0;

    if (ReadCsePersonSupportWorksheet2())
    {
      MoveCsePersonSupportWorksheet(entities.ParentBCsePersonSupportWorksheet,
        export.ParentBCsePersonSupportWorksheet);

      if (export.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts.
        GetValueOrDefault() == 0)
      {
        export.ParentBCsePersonSupportWorksheet.EligibleForFederalTaxCredit =
          "";
        export.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit = "";
      }

      if (AsChar(entities.ParentBCsePersonSupportWorksheet.EnforcementFeeType) ==
        'P')
      {
        // -- Change to Parent 2 from (F)ather.  CQ50299
        export.Common.SelectChar = "2";
        export.Common.Percentage =
          entities.ParentBCsePersonSupportWorksheet.EnforcementFeeAllowance.
            GetValueOrDefault();
      }
      else if (AsChar(entities.ParentBCsePersonSupportWorksheet.
        EnforcementFeeType) == 'F')
      {
        // -- Change to Parent 2 from (F)ather.  CQ50299
        export.Common.SelectChar = "2";
        export.Common.TotalCurrency =
          entities.ParentBCsePersonSupportWorksheet.EnforcementFeeAllowance.
            GetValueOrDefault();
      }

      if (AsChar(entities.ParentBCsePersonSupportWorksheet.AbilityToPay) == 'Y')
      {
        export.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar =
          "2";
      }

      if (ReadCsePersonCaseRole2())
      {
        // ---------------------------------------------
        // 10/29/99 Srini Ganji PR#H00078367
        // Statement added
        // ---------------------------------------------
        local.ParentB.Identifier = entities.ParentBCaseRole.Identifier;
        export.ParentBCsePerson.Number = entities.ParentBCsePerson.Number;
        local.CsePersonsWorkSet.Number = export.ParentBCsePerson.Number;
        UseSiReadCsePerson();
        export.ParentBName.Assign(local.CsePersonsWorkSet);
      }
    }

    // ---------------------------------------------
    // 10/29/1999 Srini Ganji PR#H00078367
    // IF statement block added
    // ---------------------------------------------
    if (local.ParentA.Identifier > 0)
    {
      if (ReadCase1())
      {
        export.Case1.Number = entities.Case1.Number;
      }
      else if (local.ParentB.Identifier > 0)
      {
        if (ReadCase2())
        {
          export.Case1.Number = entities.Case1.Number;
        }
        else
        {
          ExitState = "CASE_NF";

          return;
        }
      }
      else
      {
        ExitState = "CASE_NF";

        return;
      }
    }
    else if (local.ParentB.Identifier > 0)
    {
      if (ReadCase2())
      {
        export.Case1.Number = entities.Case1.Number;
      }
      else
      {
        ExitState = "CASE_NF";

        return;
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // Initialize group values.
    for(export.Export1.Index = 0; export.Export1.Index < Export
      .ExportGroup.Capacity; ++export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      export.Export1.Update.ParentA.AdjustmentAmount = 0;
      export.Export1.Update.ParentB.AdjustmentAmount = 0;
      export.Export1.Update.Work.SelectChar = "";
    }

    export.Export1.CheckIndex();
    export.Gexport2020EnterableFields.GexportParentingTime.ActionEntry = "N";
    export.Gexport2020EnterableFields.GexportParentingTime.SelectChar = "";
    export.Gexport2020EnterableFields.GexportParentingTime.TotalCurrency = 0;

    // ---------------------------------------------
    //  READ the Child Support Adjustment Amount.
    // ---------------------------------------------
    // 06/04/10  J. Huss	CQ# 18769	Modified logic to handle non-existent Child 
    // Support Adjustment records, rather
    // 					than expecting records with zero values.
    if (entities.ParentACsePerson.Populated)
    {
      foreach(var item in ReadChildSupportAdjustmentCsePersonSupportAdjustment1())
        
      {
        if (entities.ChildSupportWorksheet.CsGuidelineYear >= 2020)
        {
          // --Parenting Time Adjustment was moved out of the "child support 
          // adjustments" section on the 2020 worksheet.
          switch(entities.ChildSupportAdjustment.Number)
          {
            case 1:
              export.Export1.Index = entities.ChildSupportAdjustment.Number - 1;
              export.Export1.CheckSize();

              break;
            case 2:
              // --Set the parenting time Y/N and percentage on screen 1 and the
              // parent to which the PTA applies on screen 3.
              export.Gexport2020EnterableFields.GexportParentingTime.
                ActionEntry = "Y";
              export.Gexport2020EnterableFields.GexportParentingTime.
                SelectChar = "1";
              export.Gexport2020EnterableFields.GexportParentingTime.
                TotalCurrency =
                  entities.ParentACsePersonSupportAdjustment.AdjustmentAmount.
                  GetValueOrDefault();

              continue;
            default:
              export.Export1.Index = entities.ChildSupportAdjustment.Number - 2;
              export.Export1.CheckSize();

              break;
          }
        }
        else
        {
          export.Export1.Index = entities.ChildSupportAdjustment.Number - 1;
          export.Export1.CheckSize();
        }

        export.Export1.Update.ParentA.AdjustmentAmount =
          entities.ParentACsePersonSupportAdjustment.AdjustmentAmount;

        if (!Equal(entities.ParentACsePersonSupportAdjustment.AdjustmentAmount,
          0))
        {
          export.Export1.Update.Work.SelectChar = "Y";
        }
        else
        {
          export.Export1.Update.Work.SelectChar = "";
        }
      }
    }

    if (entities.ParentBCsePerson.Populated)
    {
      foreach(var item in ReadChildSupportAdjustmentCsePersonSupportAdjustment2())
        
      {
        if (entities.ChildSupportWorksheet.CsGuidelineYear >= 2020)
        {
          // @@@ set the parenting time flag and % on screen 1 in the workset 
          // group...
          // --Parenting Time Adjustment was moved out of the "child support 
          // adjustments" section on the 2020 worksheet.
          switch(entities.ChildSupportAdjustment.Number)
          {
            case 1:
              export.Export1.Index = entities.ChildSupportAdjustment.Number - 1;
              export.Export1.CheckSize();

              break;
            case 2:
              // --Set the parenting time Y/N and percentage on screen 1 and the
              // parent to which the PTA applies on screen 3.
              export.Gexport2020EnterableFields.GexportParentingTime.
                ActionEntry = "Y";
              export.Gexport2020EnterableFields.GexportParentingTime.
                SelectChar = "2";
              export.Gexport2020EnterableFields.GexportParentingTime.
                TotalCurrency =
                  entities.ParentBCsePersonSupportAdjustment.AdjustmentAmount.
                  GetValueOrDefault();

              continue;
            default:
              export.Export1.Index = entities.ChildSupportAdjustment.Number - 2;
              export.Export1.CheckSize();

              break;
          }
        }
        else
        {
          export.Export1.Index = entities.ChildSupportAdjustment.Number - 1;
          export.Export1.CheckSize();
        }

        export.Export1.Update.ParentB.AdjustmentAmount =
          entities.ParentBCsePersonSupportAdjustment.AdjustmentAmount;

        if (!Equal(entities.ParentBCsePersonSupportAdjustment.AdjustmentAmount,
          0))
        {
          export.Export1.Update.Work.SelectChar = "Y";
        }
        else
        {
          export.Export1.Update.Work.SelectChar = "";
        }
      }
    }

    if (entities.ParentACsePersonSupportWorksheet.Populated || entities
      .ParentBCsePersonSupportWorksheet.Populated)
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }
    else
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
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
    target.InsuranceWorkRelatedCcCredit = source.InsuranceWorkRelatedCcCredit;
    target.AbilityToPay = source.AbilityToPay;
    target.EqualParentingTimeObligation = source.EqualParentingTimeObligation;
    target.SocialSecDependentBenefit = source.SocialSecDependentBenefit;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private void UseOeWorkReadCourtOrderDetails()
  {
    var useImport = new OeWorkReadCourtOrderDetails.Import();
    var useExport = new OeWorkReadCourtOrderDetails.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(OeWorkReadCourtOrderDetails.Execute, useImport, useExport);

    export.FipsTribAddress.County = useExport.FipsTribAddress.County;
    export.County.Description = useExport.County.Description;
    export.Tribunal.JudicialDistrict = useExport.Tribunal.JudicialDistrict;
    MoveLegalAction(useExport.LegalAction, export.LegalAction);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCase1()
  {
    System.Diagnostics.Debug.Assert(entities.ParentACaseRole.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ParentACaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    System.Diagnostics.Debug.Assert(entities.ParentBCaseRole.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ParentBCaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadChildSupportAdjustmentCsePersonSupportAdjustment1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ParentACsePersonSupportWorksheet.Populated);
    entities.ChildSupportAdjustment.Populated = false;
    entities.ParentACsePersonSupportAdjustment.Populated = false;

    return ReadEach("ReadChildSupportAdjustmentCsePersonSupportAdjustment1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cpsIdentifier",
          entities.ParentACsePersonSupportWorksheet.Identifer);
        db.SetString(
          command, "cspNumber",
          entities.ParentACsePersonSupportWorksheet.CspNumber);
        db.SetString(
          command, "casNumber",
          entities.ParentACsePersonSupportWorksheet.CasNumber);
        db.SetString(
          command, "croType",
          entities.ParentACsePersonSupportWorksheet.CroType);
        db.SetInt32(
          command, "croIdentifier",
          entities.ParentACsePersonSupportWorksheet.CroIdentifier);
        db.SetInt64(
          command, "cswIdentifier",
          entities.ParentACsePersonSupportWorksheet.CswIdentifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ParentACsePersonSupportWorksheet.CssGuidelineYr);
      },
      (db, reader) =>
      {
        entities.ChildSupportAdjustment.Number = db.GetInt32(reader, 0);
        entities.ParentACsePersonSupportAdjustment.CsdNumber =
          db.GetInt32(reader, 0);
        entities.ChildSupportAdjustment.AdjustmentType =
          db.GetString(reader, 1);
        entities.ChildSupportAdjustment.Description =
          db.GetNullableString(reader, 2);
        entities.ParentACsePersonSupportAdjustment.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.ParentACsePersonSupportAdjustment.AdjustmentAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ParentACsePersonSupportAdjustment.CpsIdentifier =
          db.GetInt32(reader, 5);
        entities.ParentACsePersonSupportAdjustment.CasNumber =
          db.GetString(reader, 6);
        entities.ParentACsePersonSupportAdjustment.CspNumber =
          db.GetString(reader, 7);
        entities.ParentACsePersonSupportAdjustment.CroType =
          db.GetString(reader, 8);
        entities.ParentACsePersonSupportAdjustment.CswIdentifier =
          db.GetInt64(reader, 9);
        entities.ParentACsePersonSupportAdjustment.CssGuidelineYr =
          db.GetInt32(reader, 10);
        entities.ChildSupportAdjustment.Populated = true;
        entities.ParentACsePersonSupportAdjustment.Populated = true;
        CheckValid<CsePersonSupportAdjustment>("CroType",
          entities.ParentACsePersonSupportAdjustment.CroType);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadChildSupportAdjustmentCsePersonSupportAdjustment2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ParentBCsePersonSupportWorksheet.Populated);
    entities.ChildSupportAdjustment.Populated = false;
    entities.ParentBCsePersonSupportAdjustment.Populated = false;

    return ReadEach("ReadChildSupportAdjustmentCsePersonSupportAdjustment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cpsIdentifier",
          entities.ParentBCsePersonSupportWorksheet.Identifer);
        db.SetString(
          command, "cspNumber",
          entities.ParentBCsePersonSupportWorksheet.CspNumber);
        db.SetString(
          command, "casNumber",
          entities.ParentBCsePersonSupportWorksheet.CasNumber);
        db.SetString(
          command, "croType",
          entities.ParentBCsePersonSupportWorksheet.CroType);
        db.SetInt32(
          command, "croIdentifier",
          entities.ParentBCsePersonSupportWorksheet.CroIdentifier);
        db.SetInt64(
          command, "cswIdentifier",
          entities.ParentBCsePersonSupportWorksheet.CswIdentifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ParentBCsePersonSupportWorksheet.CssGuidelineYr);
      },
      (db, reader) =>
      {
        entities.ChildSupportAdjustment.Number = db.GetInt32(reader, 0);
        entities.ParentBCsePersonSupportAdjustment.CsdNumber =
          db.GetInt32(reader, 0);
        entities.ChildSupportAdjustment.AdjustmentType =
          db.GetString(reader, 1);
        entities.ChildSupportAdjustment.Description =
          db.GetNullableString(reader, 2);
        entities.ParentBCsePersonSupportAdjustment.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.ParentBCsePersonSupportAdjustment.AdjustmentAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ParentBCsePersonSupportAdjustment.CpsIdentifier =
          db.GetInt32(reader, 5);
        entities.ParentBCsePersonSupportAdjustment.CasNumber =
          db.GetString(reader, 6);
        entities.ParentBCsePersonSupportAdjustment.CspNumber =
          db.GetString(reader, 7);
        entities.ParentBCsePersonSupportAdjustment.CroType =
          db.GetString(reader, 8);
        entities.ParentBCsePersonSupportAdjustment.CswIdentifier =
          db.GetInt64(reader, 9);
        entities.ParentBCsePersonSupportAdjustment.CssGuidelineYr =
          db.GetInt32(reader, 10);
        entities.ChildSupportAdjustment.Populated = true;
        entities.ParentBCsePersonSupportAdjustment.Populated = true;
        CheckValid<CsePersonSupportAdjustment>("CroType",
          entities.ParentBCsePersonSupportAdjustment.CroType);

        return true;
      });
  }

  private bool ReadChildSupportWorksheet()
  {
    entities.ChildSupportWorksheet.Populated = false;

    return Read("ReadChildSupportWorksheet",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier", import.ChildSupportWorksheet.Identifier);
      },
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
        entities.ChildSupportWorksheet.AuthorizingAuthority =
          db.GetNullableString(reader, 9);
        entities.ChildSupportWorksheet.ParentingTimeAdjPercent =
          db.GetNullableInt32(reader, 10);
        entities.ChildSupportWorksheet.CsGuidelineYear =
          db.GetInt32(reader, 11);
        entities.ChildSupportWorksheet.Populated = true;
      });
  }

  private bool ReadCsePersonCaseRole1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ParentACsePersonSupportWorksheet.Populated);
    entities.ParentACsePerson.Populated = false;
    entities.ParentACaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole1",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber",
          entities.ParentACsePersonSupportWorksheet.CasNumber);
        db.SetString(
          command, "cspNumber",
          entities.ParentACsePersonSupportWorksheet.CspNumber);
        db.SetString(
          command, "croType",
          entities.ParentACsePersonSupportWorksheet.CroType);
        db.SetInt32(
          command, "croIdentifier",
          entities.ParentACsePersonSupportWorksheet.CroIdentifier);
      },
      (db, reader) =>
      {
        entities.ParentACsePerson.Number = db.GetString(reader, 0);
        entities.ParentACaseRole.CasNumber = db.GetString(reader, 1);
        entities.ParentACaseRole.CspNumber = db.GetString(reader, 2);
        entities.ParentACaseRole.Type1 = db.GetString(reader, 3);
        entities.ParentACaseRole.Identifier = db.GetInt32(reader, 4);
        entities.ParentACsePerson.Populated = true;
        entities.ParentACaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ParentACaseRole.Type1);
      });
  }

  private bool ReadCsePersonCaseRole2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ParentBCsePersonSupportWorksheet.Populated);
    entities.ParentBCsePerson.Populated = false;
    entities.ParentBCaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole2",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber",
          entities.ParentBCsePersonSupportWorksheet.CasNumber);
        db.SetString(
          command, "cspNumber",
          entities.ParentBCsePersonSupportWorksheet.CspNumber);
        db.SetString(
          command, "croType",
          entities.ParentBCsePersonSupportWorksheet.CroType);
        db.SetInt32(
          command, "croIdentifier",
          entities.ParentBCsePersonSupportWorksheet.CroIdentifier);
      },
      (db, reader) =>
      {
        entities.ParentBCsePerson.Number = db.GetString(reader, 0);
        entities.ParentBCaseRole.CasNumber = db.GetString(reader, 1);
        entities.ParentBCaseRole.CspNumber = db.GetString(reader, 2);
        entities.ParentBCaseRole.Type1 = db.GetString(reader, 3);
        entities.ParentBCaseRole.Identifier = db.GetInt32(reader, 4);
        entities.ParentBCsePerson.Populated = true;
        entities.ParentBCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ParentBCaseRole.Type1);
      });
  }

  private bool ReadCsePersonSupportWorksheet1()
  {
    entities.ParentACsePersonSupportWorksheet.Populated = false;

    return Read("ReadCsePersonSupportWorksheet1",
      (db, command) =>
      {
        db.SetInt64(
          command, "cswIdentifier", entities.ChildSupportWorksheet.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportWorksheet.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.ParentACsePersonSupportWorksheet.CroIdentifier =
          db.GetInt32(reader, 0);
        entities.ParentACsePersonSupportWorksheet.Identifer =
          db.GetInt32(reader, 1);
        entities.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare =
          db.GetNullableInt32(reader, 2);
        entities.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts =
          db.GetNullableDecimal(reader, 3);
        entities.ParentACsePersonSupportWorksheet.WageEarnerGrossIncome =
          db.GetNullableDecimal(reader, 4);
        entities.ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome =
          db.GetNullableDecimal(reader, 5);
        entities.ParentACsePersonSupportWorksheet.ReasonableBusinessExpense =
          db.GetNullableDecimal(reader, 6);
        entities.ParentACsePersonSupportWorksheet.CourtOrderedChildSupportPaid =
          db.GetNullableDecimal(reader, 7);
        entities.ParentACsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo =
          db.GetNullableString(reader, 8);
        entities.ParentACsePersonSupportWorksheet.CourtOrderedMaintenancePaid =
          db.GetNullableDecimal(reader, 9);
        entities.ParentACsePersonSupportWorksheet.MaintenancePaidCourtOrderNo =
          db.GetNullableString(reader, 10);
        entities.ParentACsePersonSupportWorksheet.CourtOrderedMaintenanceRecvd =
          db.GetNullableDecimal(reader, 11);
        entities.ParentACsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo =
          db.GetNullableString(reader, 12);
        entities.ParentACsePersonSupportWorksheet.HealthAndDentalInsurancePrem =
          db.GetNullableDecimal(reader, 13);
        entities.ParentACsePersonSupportWorksheet.EligibleForFederalTaxCredit =
          db.GetNullableString(reader, 14);
        entities.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit =
          db.GetNullableString(reader, 15);
        entities.ParentACsePersonSupportWorksheet.NetAdjParentalChildSuppAmt =
          db.GetNullableDecimal(reader, 16);
        entities.ParentACsePersonSupportWorksheet.EnforcementFeeType =
          db.GetNullableString(reader, 17);
        entities.ParentACsePersonSupportWorksheet.EnforcementFeeAllowance =
          db.GetNullableInt32(reader, 18);
        entities.ParentACsePersonSupportWorksheet.CswIdentifier =
          db.GetInt64(reader, 19);
        entities.ParentACsePersonSupportWorksheet.CspNumber =
          db.GetString(reader, 20);
        entities.ParentACsePersonSupportWorksheet.CasNumber =
          db.GetString(reader, 21);
        entities.ParentACsePersonSupportWorksheet.CroType =
          db.GetString(reader, 22);
        entities.ParentACsePersonSupportWorksheet.CssGuidelineYr =
          db.GetInt32(reader, 23);
        entities.ParentACsePersonSupportWorksheet.InsuranceWorkRelatedCcCredit =
          db.GetNullableDecimal(reader, 24);
        entities.ParentACsePersonSupportWorksheet.AbilityToPay =
          db.GetNullableString(reader, 25);
        entities.ParentACsePersonSupportWorksheet.EqualParentingTimeObligation =
          db.GetNullableDecimal(reader, 26);
        entities.ParentACsePersonSupportWorksheet.SocialSecDependentBenefit =
          db.GetNullableDecimal(reader, 27);
        entities.ParentACsePersonSupportWorksheet.Populated = true;
        CheckValid<CsePersonSupportWorksheet>("CroType",
          entities.ParentACsePersonSupportWorksheet.CroType);
      });
  }

  private bool ReadCsePersonSupportWorksheet2()
  {
    entities.ParentBCsePersonSupportWorksheet.Populated = false;

    return Read("ReadCsePersonSupportWorksheet2",
      (db, command) =>
      {
        db.SetInt64(
          command, "cswIdentifier", entities.ChildSupportWorksheet.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportWorksheet.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.ParentBCsePersonSupportWorksheet.CroIdentifier =
          db.GetInt32(reader, 0);
        entities.ParentBCsePersonSupportWorksheet.Identifer =
          db.GetInt32(reader, 1);
        entities.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare =
          db.GetNullableInt32(reader, 2);
        entities.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts =
          db.GetNullableDecimal(reader, 3);
        entities.ParentBCsePersonSupportWorksheet.WageEarnerGrossIncome =
          db.GetNullableDecimal(reader, 4);
        entities.ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome =
          db.GetNullableDecimal(reader, 5);
        entities.ParentBCsePersonSupportWorksheet.ReasonableBusinessExpense =
          db.GetNullableDecimal(reader, 6);
        entities.ParentBCsePersonSupportWorksheet.CourtOrderedChildSupportPaid =
          db.GetNullableDecimal(reader, 7);
        entities.ParentBCsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo =
          db.GetNullableString(reader, 8);
        entities.ParentBCsePersonSupportWorksheet.CourtOrderedMaintenancePaid =
          db.GetNullableDecimal(reader, 9);
        entities.ParentBCsePersonSupportWorksheet.MaintenancePaidCourtOrderNo =
          db.GetNullableString(reader, 10);
        entities.ParentBCsePersonSupportWorksheet.CourtOrderedMaintenanceRecvd =
          db.GetNullableDecimal(reader, 11);
        entities.ParentBCsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo =
          db.GetNullableString(reader, 12);
        entities.ParentBCsePersonSupportWorksheet.HealthAndDentalInsurancePrem =
          db.GetNullableDecimal(reader, 13);
        entities.ParentBCsePersonSupportWorksheet.EligibleForFederalTaxCredit =
          db.GetNullableString(reader, 14);
        entities.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit =
          db.GetNullableString(reader, 15);
        entities.ParentBCsePersonSupportWorksheet.NetAdjParentalChildSuppAmt =
          db.GetNullableDecimal(reader, 16);
        entities.ParentBCsePersonSupportWorksheet.EnforcementFeeType =
          db.GetNullableString(reader, 17);
        entities.ParentBCsePersonSupportWorksheet.EnforcementFeeAllowance =
          db.GetNullableInt32(reader, 18);
        entities.ParentBCsePersonSupportWorksheet.CswIdentifier =
          db.GetInt64(reader, 19);
        entities.ParentBCsePersonSupportWorksheet.CspNumber =
          db.GetString(reader, 20);
        entities.ParentBCsePersonSupportWorksheet.CasNumber =
          db.GetString(reader, 21);
        entities.ParentBCsePersonSupportWorksheet.CroType =
          db.GetString(reader, 22);
        entities.ParentBCsePersonSupportWorksheet.CssGuidelineYr =
          db.GetInt32(reader, 23);
        entities.ParentBCsePersonSupportWorksheet.InsuranceWorkRelatedCcCredit =
          db.GetNullableDecimal(reader, 24);
        entities.ParentBCsePersonSupportWorksheet.AbilityToPay =
          db.GetNullableString(reader, 25);
        entities.ParentBCsePersonSupportWorksheet.EqualParentingTimeObligation =
          db.GetNullableDecimal(reader, 26);
        entities.ParentBCsePersonSupportWorksheet.SocialSecDependentBenefit =
          db.GetNullableDecimal(reader, 27);
        entities.ParentBCsePersonSupportWorksheet.Populated = true;
        CheckValid<CsePersonSupportWorksheet>("CroType",
          entities.ParentBCsePersonSupportWorksheet.CroType);
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ChildSupportWorksheet.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ChildSupportWorksheet.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
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
    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    private ChildSupportWorksheet childSupportWorksheet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Work.
      /// </summary>
      [JsonPropertyName("work")]
      public Common Work
      {
        get => work ??= new();
        set => work = value;
      }

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

      private Common work;
      private CsePersonSupportAdjustment parentB;
      private CsePersonSupportAdjustment parentA;
    }

    /// <summary>A Gexport2020EnterableFieldsGroup group.</summary>
    [Serializable]
    public class Gexport2020EnterableFieldsGroup
    {
      /// <summary>
      /// A value of GexportParentingTime.
      /// </summary>
      [JsonPropertyName("gexportParentingTime")]
      public Common GexportParentingTime
      {
        get => gexportParentingTime ??= new();
        set => gexportParentingTime = value;
      }

      /// <summary>
      /// A value of GexportAbilityToPayParent.
      /// </summary>
      [JsonPropertyName("gexportAbilityToPayParent")]
      public Common GexportAbilityToPayParent
      {
        get => gexportAbilityToPayParent ??= new();
        set => gexportAbilityToPayParent = value;
      }

      private Common gexportParentingTime;
      private Common gexportAbilityToPayParent;
    }

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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public CodeValue County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
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
    /// A value of PrevParentB.
    /// </summary>
    [JsonPropertyName("prevParentB")]
    public CsePerson PrevParentB
    {
      get => prevParentB ??= new();
      set => prevParentB = value;
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
    /// A value of PrevParentA.
    /// </summary>
    [JsonPropertyName("prevParentA")]
    public CsePerson PrevParentA
    {
      get => prevParentA ??= new();
      set => prevParentA = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ParentAName.
    /// </summary>
    [JsonPropertyName("parentAName")]
    public CsePersonsWorkSet ParentAName
    {
      get => parentAName ??= new();
      set => parentAName = value;
    }

    /// <summary>
    /// A value of ParentBName.
    /// </summary>
    [JsonPropertyName("parentBName")]
    public CsePersonsWorkSet ParentBName
    {
      get => parentBName ??= new();
      set => parentBName = value;
    }

    /// <summary>
    /// Gets a value of Gexport2020EnterableFields.
    /// </summary>
    [JsonPropertyName("gexport2020EnterableFields")]
    public Gexport2020EnterableFieldsGroup Gexport2020EnterableFields
    {
      get => gexport2020EnterableFields ?? (gexport2020EnterableFields = new());
      set => gexport2020EnterableFields = value;
    }

    private Common common;
    private FipsTribAddress fipsTribAddress;
    private CodeValue county;
    private Array<ExportGroup> export1;
    private ChildSupportWorksheet childSupportWorksheet;
    private CsePersonSupportWorksheet parentBCsePersonSupportWorksheet;
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private CsePerson parentBCsePerson;
    private CsePerson prevParentB;
    private CsePerson parentACsePerson;
    private CsePerson prevParentA;
    private Case1 case1;
    private Case1 prev;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private CsePersonsWorkSet parentAName;
    private CsePersonsWorkSet parentBName;
    private Gexport2020EnterableFieldsGroup gexport2020EnterableFields;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ParentB.
    /// </summary>
    [JsonPropertyName("parentB")]
    public CaseRole ParentB
    {
      get => parentB ??= new();
      set => parentB = value;
    }

    /// <summary>
    /// A value of ParentA.
    /// </summary>
    [JsonPropertyName("parentA")]
    public CaseRole ParentA
    {
      get => parentA ??= new();
      set => parentA = value;
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
    /// A value of FirstNameLength.
    /// </summary>
    [JsonPropertyName("firstNameLength")]
    public Common FirstNameLength
    {
      get => firstNameLength ??= new();
      set => firstNameLength = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private CaseRole parentB;
    private CaseRole parentA;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common firstNameLength;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ParentBCsePerson.
    /// </summary>
    [JsonPropertyName("parentBCsePerson")]
    public CsePerson ParentBCsePerson
    {
      get => parentBCsePerson ??= new();
      set => parentBCsePerson = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of ParentBCaseRole.
    /// </summary>
    [JsonPropertyName("parentBCaseRole")]
    public CaseRole ParentBCaseRole
    {
      get => parentBCaseRole ??= new();
      set => parentBCaseRole = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    private CsePerson parentACsePerson;
    private CsePerson parentBCsePerson;
    private ChildSupportAdjustment childSupportAdjustment;
    private CsePersonSupportAdjustment parentBCsePersonSupportAdjustment;
    private CsePersonSupportAdjustment parentACsePersonSupportAdjustment;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private CaseRole parentBCaseRole;
    private CaseRole parentACaseRole;
    private Case1 case1;
    private CsePersonSupportWorksheet parentBCsePersonSupportWorksheet;
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private ChildSupportWorksheet childSupportWorksheet;
  }
#endregion
}
