// Program: OE_CAL2_CALCULATE_WKSHEET_PAGE_2, ID: 371397346, model: 746.
// Short name: SWECAL2P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CAL2_CALCULATE_WKSHEET_PAGE_2.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeCal2CalculateWksheetPage2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CAL2_CALCULATE_WKSHEET_PAGE_2 program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCal2CalculateWksheetPage2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCal2CalculateWksheetPage2.
  /// </summary>
  public OeCal2CalculateWksheetPage2(IContext context, Import import,
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
    // Date      Author          Description
    // 03/03/95  Sid           Initial Creation
    // ---------------------------------------------
    // ---------------------------------------------
    // This is a 3 procedure step PRAD. The views
    // contain the details of all the 3 screens. The
    // PSADs are connected through two links and all
    // the data are passed through all the screens.
    // All the data not belonging to a particular
    // screen are kept in the hidden views.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------
    //        Move all IMPORTs to EXPORTs.
    // ---------------------------------------------
    export.Case1.Number = import.Case1.Number;
    export.ChildSupportWorksheet.Assign(import.ChildSupportWorksheet);
    export.LegalAction.CourtCaseNumber = import.LegalAction.CourtCaseNumber;
    export.County.TextLength16 = import.County.TextLength16;
    export.Tribunal.JudicialDistrict = import.Tribunal.JudicialDistrict;
    export.ParentACsePerson.Number = import.ParentACsePerson.Number;
    export.ParentBCsePerson.Number = import.ParentBCsePerson.Number;
    export.ParentBCsePersonSupportWorksheet.Assign(
      import.ParentBCsePersonSupportWorksheet);
    export.ParentACsePersonSupportWorksheet.Assign(
      import.ParentACsePersonSupportWorksheet);
    export.ParentAName.FormattedNameText = import.ParentAName.FormattedNameText;
    export.ParentBName.FormattedNameText = import.ParentBName.FormattedNameText;
    export.CsOblig06TotalAmt.TotalCurrency =
      import.CsOblig06TotalAmt.TotalCurrency;
    export.CsOblig1618TotalAmt.TotalCurrency =
      import.CsOblig1618TotalInc.TotalCurrency;
    export.CsOblig715TotalAmt.TotalCurrency =
      import.CsOblig715TotalAmt.TotalCurrency;
    export.CsObligTotalAmount.TotalCurrency =
      import.CsObligTotalAmount.TotalCurrency;
    export.D1otalCsInc.TotalCurrency = import.D1otalCsInc.TotalCurrency;
    export.D6otalChildSuppOblig.TotalCurrency =
      import.D6otalChildSuppOblig.TotalCurrency;
    export.ParentAB3SeGrossInc.TotalCurrency =
      import.ParentAB3SeGrossInc.TotalCurrency;
    export.ParentAC1TotGrossInc.TotalCurrency =
      import.ParentAC1TotGrossInc.TotalCurrency;
    export.ParentAC5D1TotCsInc.TotalCurrency =
      import.ParentAC5D1TotCsInc.TotalCurrency;
    export.ParentAChildCareCost.TotalCurrency =
      import.ParentAChildCareCost.TotalCurrency;
    export.ParentAD2PercentInc.TotalCurrency =
      import.ParentAD2PercentInc.TotalCurrency;
    export.ParentAD7CsOblig.TotalCurrency =
      import.ParentAD7CsOblig.TotalCurrency;
    export.ParentAD8Adjustments.TotalCurrency =
      import.ParentAD8Adjustments.TotalCurrency;
    export.ParentAD9F1NetCs.TotalCurrency =
      import.ParentAD9F1NetCs.TotalCurrency;
    export.ParentAE7F2TotAdj.TotalCurrency =
      import.ParentAE7F2TotAdjust.TotalCurrency;
    export.ParentAF2TotalCsAdj.TotalCurrency =
      import.ParentAF2TotalCsAdj.TotalCurrency;
    export.ParentAF3AdjCsOblig.TotalCurrency =
      import.ParentAF3AdjCsOblig.TotalCurrency;
    export.ParentATotalTaxCredit.TotalCurrency =
      import.ParentATotalTaxCredit.TotalCurrency;
    export.ParentBB3SeGrossInc.TotalCurrency =
      import.ParentBB3SeGrossInc.TotalCurrency;
    export.ParentBC1TotGrossInc.TotalCurrency =
      import.ParentBC1TotGrossInc.TotalCurrency;
    export.ParentBC5D1TotCsInc.TotalCurrency =
      import.ParentBC5D1TotCsInc.TotalCurrency;
    export.ParentBChildCareCost.TotalCurrency =
      import.ParentBChildCareCost.TotalCurrency;
    export.ParentBD2PercentInc.TotalCurrency =
      import.ParentBD2PercentInc.TotalCurrency;
    export.ParentBD7CsOblig.TotalCurrency =
      import.ParentBD7CsOblig.TotalCurrency;
    export.ParentBD8Adjustments.TotalCurrency =
      import.ParentBD8Adjustments.TotalCurrency;
    export.ParentBD9F1NetCs.TotalCurrency =
      import.ParentBD9F1NetCs.TotalCurrency;
    export.ParentBE7F2TotAdj.TotalCurrency =
      import.ParentBE7F2TotAdjust.TotalCurrency;
    export.ParentBF2TotalCsAdj.TotalCurrency =
      import.ParentBF2TotalCsAdj.TotalCurrency;
    export.ParentBF3AdjCsOblig.TotalCurrency =
      import.ParentBF3AdjCsOblig.TotalCurrency;
    export.ParentBTotalTaxCredit.TotalCurrency =
      import.ParentBTotalTaxCredit.TotalCurrency;
    export.TotalChildCareCost.TotalCurrency =
      import.TotalChildCareCost.TotalCurrency;
    export.TotalInsurancePrem.TotalCurrency =
      import.TotalInsurancePrem.TotalCurrency;

    // ---------------------------------------------
    //   Move all Group IMPORTs to Group EXPORTs.
    // ---------------------------------------------
    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.Work.SelectChar =
          import.Import1.Item.Work.SelectChar;
        export.Export1.Update.ParentA.AdjustmentAmount =
          import.Import1.Item.ParentA.AdjustmentAmount;
        export.Export1.Update.ParentB.AdjustmentAmount =
          import.Import1.Item.ParentB.AdjustmentAmount;
        export.Export1.Next();
      }
    }

    // ---------------------------------------------
    // When comming from one of the other pages,
    // move imports to exports and display the
    // screen.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPPAGE"))
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

      return;
    }

    // ---------------------------------------------
    //           N E X T   T R A N
    // Use the CAB to nexttran to another procedure.
    // ---------------------------------------------
    // ---------------------------------------------
    //   S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // ---------------------------------------------
    //     P F K E Y    P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "HELP":
        break;
      case "PREV":
        if (IsEmpty(import.ParentBCsePersonSupportWorksheet.
          EligibleForKansasTaxCredit))
        {
          if (import.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts.
            GetValueOrDefault() <= 0)
          {
            export.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit =
              "N";
          }
          else
          {
            export.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit =
              "Y";
          }
        }
        else if (AsChar(import.ParentBCsePersonSupportWorksheet.
          EligibleForKansasTaxCredit) != 'Y' && AsChar
          (import.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit) !=
            'N')
        {
          var field =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "eligibleForKansasTaxCredit");

          field.Error = true;

          ExitState = "INVALID_SWITCH";
        }

        if (IsEmpty(import.ParentBCsePersonSupportWorksheet.
          EligibleForFederalTaxCredit))
        {
          if (import.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts.
            GetValueOrDefault() <= 0)
          {
            export.ParentBCsePersonSupportWorksheet.
              EligibleForFederalTaxCredit = "N";
          }
          else
          {
            export.ParentBCsePersonSupportWorksheet.
              EligibleForFederalTaxCredit = "Y";
          }
        }
        else if (AsChar(import.ParentBCsePersonSupportWorksheet.
          EligibleForFederalTaxCredit) != 'Y' && AsChar
          (import.ParentBCsePersonSupportWorksheet.EligibleForFederalTaxCredit) !=
            'N')
        {
          var field =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "eligibleForFederalTaxCredit");

          field.Error = true;

          ExitState = "INVALID_SWITCH";
        }

        if (IsEmpty(import.ParentACsePersonSupportWorksheet.
          EligibleForKansasTaxCredit))
        {
          if (import.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts.
            GetValueOrDefault() <= 0)
          {
            export.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit =
              "N";
          }
          else
          {
            export.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit =
              "Y";
          }
        }
        else if (AsChar(import.ParentACsePersonSupportWorksheet.
          EligibleForKansasTaxCredit) != 'Y' && AsChar
          (import.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit) !=
            'N')
        {
          var field =
            GetField(export.ParentACsePersonSupportWorksheet,
            "eligibleForKansasTaxCredit");

          field.Error = true;

          ExitState = "INVALID_SWITCH";
        }

        if (IsEmpty(import.ParentACsePersonSupportWorksheet.
          EligibleForFederalTaxCredit))
        {
          if (import.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts.
            GetValueOrDefault() <= 0)
          {
            export.ParentACsePersonSupportWorksheet.
              EligibleForFederalTaxCredit = "N";
          }
          else
          {
            export.ParentACsePersonSupportWorksheet.
              EligibleForFederalTaxCredit = "Y";
          }
        }
        else if (AsChar(import.ParentACsePersonSupportWorksheet.
          EligibleForFederalTaxCredit) != 'Y' && AsChar
          (import.ParentACsePersonSupportWorksheet.EligibleForFederalTaxCredit) !=
            'N')
        {
          var field =
            GetField(export.ParentACsePersonSupportWorksheet,
            "eligibleForFederalTaxCredit");

          field.Error = true;

          ExitState = "INVALID_SWITCH";
        }

        if (import.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts.
          GetValueOrDefault() <= 0)
        {
          export.ParentACsePersonSupportWorksheet.EligibleForFederalTaxCredit =
            "";
          export.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit =
            "";

          if (export.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() > 0)
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "workRelatedChildCareCosts");

            field.Error = true;

            ExitState = "OE0000_DAYCARE_AMOUNT_REQUIRED";
          }
        }
        else
        {
          if (IsEmpty(export.ParentACsePersonSupportWorksheet.
            EligibleForFederalTaxCredit))
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "eligibleForFederalTaxCredit");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.ParentACsePersonSupportWorksheet.
            EligibleForKansasTaxCredit))
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "eligibleForKansasTaxCredit");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (import.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() <= 0)
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "noOfChildrenInDayCare");

            field.Error = true;

            ExitState = "OE0000_NO_OF_DAYCARE_CHLDRN_REQD";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts.
          GetValueOrDefault() <= 0)
        {
          export.ParentBCsePersonSupportWorksheet.EligibleForFederalTaxCredit =
            "";
          export.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit =
            "";

          if (export.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() > 0)
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "workRelatedChildCareCosts");

            field.Error = true;

            ExitState = "OE0000_DAYCARE_AMOUNT_REQUIRED";
          }
        }
        else
        {
          if (IsEmpty(export.ParentBCsePersonSupportWorksheet.
            EligibleForKansasTaxCredit))
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "eligibleForKansasTaxCredit");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.ParentBCsePersonSupportWorksheet.
            EligibleForFederalTaxCredit))
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "eligibleForFederalTaxCredit");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (import.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() <= 0)
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "noOfChildrenInDayCare");

            field.Error = true;

            ExitState = "OE0000_NO_OF_DAYCARE_CHLDRN_REQD";
          }
        }

        if (import.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare.
          GetValueOrDefault() > import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault())
        {
          var field =
            GetField(export.ParentACsePersonSupportWorksheet,
            "noOfChildrenInDayCare");

          field.Error = true;

          ExitState = "OE0000_INVALID_NO_OF_CHILDREN";
        }

        if (import.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare.
          GetValueOrDefault() > import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault())
        {
          var field =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "noOfChildrenInDayCare");

          field.Error = true;

          ExitState = "OE0000_INVALID_NO_OF_CHILDREN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ExitState = "ECO_LNK_TO_CS_WORKSHEET_1";
        global.Command = "DISPPAGE";

        break;
      case "NEXT":
        if (IsEmpty(import.ParentBCsePersonSupportWorksheet.
          EligibleForKansasTaxCredit))
        {
          if (import.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts.
            GetValueOrDefault() <= 0)
          {
            export.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit =
              "N";
          }
          else
          {
            export.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit =
              "Y";
          }
        }
        else if (AsChar(import.ParentBCsePersonSupportWorksheet.
          EligibleForKansasTaxCredit) != 'Y' && AsChar
          (import.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit) !=
            'N')
        {
          var field =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "eligibleForKansasTaxCredit");

          field.Error = true;

          ExitState = "INVALID_SWITCH";
        }

        if (IsEmpty(import.ParentBCsePersonSupportWorksheet.
          EligibleForFederalTaxCredit))
        {
          if (import.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts.
            GetValueOrDefault() <= 0)
          {
            export.ParentBCsePersonSupportWorksheet.
              EligibleForFederalTaxCredit = "N";
          }
          else
          {
            export.ParentBCsePersonSupportWorksheet.
              EligibleForFederalTaxCredit = "Y";
          }
        }
        else if (AsChar(import.ParentBCsePersonSupportWorksheet.
          EligibleForFederalTaxCredit) != 'Y' && AsChar
          (import.ParentBCsePersonSupportWorksheet.EligibleForFederalTaxCredit) !=
            'N')
        {
          var field =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "eligibleForFederalTaxCredit");

          field.Error = true;

          ExitState = "INVALID_SWITCH";
        }

        if (IsEmpty(import.ParentACsePersonSupportWorksheet.
          EligibleForKansasTaxCredit))
        {
          if (import.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts.
            GetValueOrDefault() <= 0)
          {
            export.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit =
              "N";
          }
          else
          {
            export.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit =
              "Y";
          }
        }
        else if (AsChar(import.ParentACsePersonSupportWorksheet.
          EligibleForKansasTaxCredit) != 'Y' && AsChar
          (import.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit) !=
            'N')
        {
          var field =
            GetField(export.ParentACsePersonSupportWorksheet,
            "eligibleForKansasTaxCredit");

          field.Error = true;

          ExitState = "INVALID_SWITCH";
        }

        if (IsEmpty(import.ParentACsePersonSupportWorksheet.
          EligibleForFederalTaxCredit))
        {
          if (import.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts.
            GetValueOrDefault() <= 0)
          {
            export.ParentACsePersonSupportWorksheet.
              EligibleForFederalTaxCredit = "N";
          }
          else
          {
            export.ParentACsePersonSupportWorksheet.
              EligibleForFederalTaxCredit = "Y";
          }
        }
        else if (AsChar(import.ParentACsePersonSupportWorksheet.
          EligibleForFederalTaxCredit) != 'Y' && AsChar
          (import.ParentACsePersonSupportWorksheet.EligibleForFederalTaxCredit) !=
            'N')
        {
          var field =
            GetField(export.ParentACsePersonSupportWorksheet,
            "eligibleForFederalTaxCredit");

          field.Error = true;

          ExitState = "INVALID_SWITCH";
        }

        if (import.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts.
          GetValueOrDefault() <= 0)
        {
          export.ParentACsePersonSupportWorksheet.EligibleForFederalTaxCredit =
            "";
          export.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit =
            "";

          if (export.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() > 0)
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "workRelatedChildCareCosts");

            field.Error = true;

            ExitState = "OE0000_DAYCARE_AMOUNT_REQUIRED";
          }
        }
        else
        {
          if (IsEmpty(export.ParentACsePersonSupportWorksheet.
            EligibleForFederalTaxCredit))
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "eligibleForFederalTaxCredit");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.ParentACsePersonSupportWorksheet.
            EligibleForKansasTaxCredit))
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "eligibleForKansasTaxCredit");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (import.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() <= 0)
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "noOfChildrenInDayCare");

            field.Error = true;

            ExitState = "OE0000_NO_OF_DAYCARE_CHLDRN_REQD";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts.
          GetValueOrDefault() <= 0)
        {
          export.ParentBCsePersonSupportWorksheet.EligibleForFederalTaxCredit =
            "";
          export.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit =
            "";

          if (export.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() > 0)
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "workRelatedChildCareCosts");

            field.Error = true;

            ExitState = "OE0000_DAYCARE_AMOUNT_REQUIRED";
          }
        }
        else
        {
          if (IsEmpty(export.ParentBCsePersonSupportWorksheet.
            EligibleForKansasTaxCredit))
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "eligibleForKansasTaxCredit");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.ParentBCsePersonSupportWorksheet.
            EligibleForFederalTaxCredit))
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "eligibleForFederalTaxCredit");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (import.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() <= 0)
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "noOfChildrenInDayCare");

            field.Error = true;

            ExitState = "OE0000_NO_OF_DAYCARE_CHLDRN_REQD";
          }
        }

        if (import.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare.
          GetValueOrDefault() > import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault())
        {
          var field =
            GetField(export.ParentACsePersonSupportWorksheet,
            "noOfChildrenInDayCare");

          field.Error = true;

          ExitState = "OE0000_INVALID_NO_OF_CHILDREN";
        }

        if (import.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare.
          GetValueOrDefault() > import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault())
        {
          var field =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "noOfChildrenInDayCare");

          field.Error = true;

          ExitState = "OE0000_INVALID_NO_OF_CHILDREN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ExitState = "ECO_LNK_TO_CS_WORKSHEET_3";
        global.Command = "DISPPAGE";

        break;
      case "CALCULAT":
        if (import.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts.
          GetValueOrDefault() <= 0)
        {
          export.ParentACsePersonSupportWorksheet.EligibleForFederalTaxCredit =
            "";
          export.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit =
            "";

          if (export.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() > 0)
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "workRelatedChildCareCosts");

            field.Error = true;

            ExitState = "OE0000_DAYCARE_AMOUNT_REQUIRED";
          }
        }
        else
        {
          if (IsEmpty(import.ParentACsePersonSupportWorksheet.
            EligibleForFederalTaxCredit))
          {
            export.ParentACsePersonSupportWorksheet.
              EligibleForFederalTaxCredit = "Y";
          }

          if (IsEmpty(import.ParentACsePersonSupportWorksheet.
            EligibleForKansasTaxCredit))
          {
            export.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit =
              "Y";
          }

          if (import.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() <= 0)
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "noOfChildrenInDayCare");

            field.Error = true;

            ExitState = "OE0000_NO_OF_DAYCARE_CHLDRN_REQD";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts.
          GetValueOrDefault() <= 0)
        {
          export.ParentBCsePersonSupportWorksheet.EligibleForFederalTaxCredit =
            "";
          export.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit =
            "";

          if (export.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() > 0)
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "workRelatedChildCareCosts");

            field.Error = true;

            ExitState = "OE0000_DAYCARE_AMOUNT_REQUIRED";
          }
        }
        else
        {
          if (IsEmpty(export.ParentBCsePersonSupportWorksheet.
            EligibleForKansasTaxCredit))
          {
            export.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit =
              "Y";
          }

          if (IsEmpty(export.ParentBCsePersonSupportWorksheet.
            EligibleForFederalTaxCredit))
          {
            export.ParentBCsePersonSupportWorksheet.
              EligibleForFederalTaxCredit = "Y";
          }

          if (import.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare.
            GetValueOrDefault() <= 0)
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "noOfChildrenInDayCare");

            field.Error = true;

            ExitState = "OE0000_NO_OF_DAYCARE_CHLDRN_REQD";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (import.ParentACsePersonSupportWorksheet.NoOfChildrenInDayCare.
          GetValueOrDefault() > import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault())
        {
          var field =
            GetField(export.ParentACsePersonSupportWorksheet,
            "noOfChildrenInDayCare");

          field.Error = true;

          ExitState = "OE0000_INVALID_NO_OF_CHILDREN";
        }

        if (import.ParentBCsePersonSupportWorksheet.NoOfChildrenInDayCare.
          GetValueOrDefault() > import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault())
        {
          var field =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "noOfChildrenInDayCare");

          field.Error = true;

          ExitState = "OE0000_INVALID_NO_OF_CHILDREN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (import.ParentBCsePersonSupportWorksheet.WorkRelatedChildCareCosts.
          GetValueOrDefault() > 0)
        {
          if (AsChar(export.ParentBCsePersonSupportWorksheet.
            EligibleForKansasTaxCredit) != 'Y' && AsChar
            (export.ParentBCsePersonSupportWorksheet.EligibleForKansasTaxCredit) !=
              'N')
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "eligibleForKansasTaxCredit");

            field.Error = true;

            ExitState = "INVALID_SWITCH";
          }

          if (AsChar(export.ParentBCsePersonSupportWorksheet.
            EligibleForFederalTaxCredit) != 'Y' && AsChar
            (export.ParentBCsePersonSupportWorksheet.EligibleForFederalTaxCredit)
            != 'N')
          {
            var field =
              GetField(export.ParentBCsePersonSupportWorksheet,
              "eligibleForFederalTaxCredit");

            field.Error = true;

            ExitState = "INVALID_SWITCH";
          }
        }

        if (import.ParentACsePersonSupportWorksheet.WorkRelatedChildCareCosts.
          GetValueOrDefault() > 0)
        {
          if (AsChar(export.ParentACsePersonSupportWorksheet.
            EligibleForKansasTaxCredit) != 'Y' && AsChar
            (export.ParentACsePersonSupportWorksheet.EligibleForKansasTaxCredit) !=
              'N')
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "eligibleForKansasTaxCredit");

            field.Error = true;

            ExitState = "INVALID_SWITCH";
          }

          if (AsChar(export.ParentACsePersonSupportWorksheet.
            EligibleForFederalTaxCredit) != 'Y' && AsChar
            (export.ParentACsePersonSupportWorksheet.EligibleForFederalTaxCredit)
            != 'N')
          {
            var field =
              GetField(export.ParentACsePersonSupportWorksheet,
              "eligibleForFederalTaxCredit");

            field.Error = true;

            ExitState = "INVALID_SWITCH";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeWorkCalcChildSupWorksheet();

        break;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY_2";

        break;
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
  }

  private static void MoveExport1ToImport1(Export.ExportGroup source,
    OeWorkCalcChildSupWorksheet.Import.ImportGroup target)
  {
    target.Work.SelectChar = source.Work.SelectChar;
    target.ParentB.AdjustmentAmount = source.ParentB.AdjustmentAmount;
    target.ParentA.AdjustmentAmount = source.ParentA.AdjustmentAmount;
  }

  private void UseOeWorkCalcChildSupWorksheet()
  {
    var useImport = new OeWorkCalcChildSupWorksheet.Import();
    var useExport = new OeWorkCalcChildSupWorksheet.Export();

    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport1);
    MoveChildSupportWorksheet(export.ChildSupportWorksheet,
      useImport.ChildSupportWorksheet);
    MoveCsePersonSupportWorksheet(export.ParentBCsePersonSupportWorksheet,
      useImport.ParentB);
    MoveCsePersonSupportWorksheet(export.ParentACsePersonSupportWorksheet,
      useImport.ParentA);

    Call(OeWorkCalcChildSupWorksheet.Execute, useImport, useExport);

    export.ParentBF2TotalCsAdj.TotalCurrency =
      useExport.ParentBF2TotalCsAdj.TotalCurrency;
    export.ParentAF2TotalCsAdj.TotalCurrency =
      useExport.ParentAF2TotalCsAdj.TotalCurrency;
    export.D1otalCsInc.TotalCurrency = useExport.D1otalCsInc.TotalCurrency;
    export.ParentBF3AdjCsOblig.TotalCurrency =
      useExport.ParentBF3AdjCsOblig.TotalCurrency;
    export.ParentBE7F2TotAdj.TotalCurrency =
      useExport.ParentBE7F2TotAdj.TotalCurrency;
    export.ParentAF3AdjCsOblig.TotalCurrency =
      useExport.ParentAF3AdjCsOblig.TotalCurrency;
    export.ParentAE7F2TotAdj.TotalCurrency =
      useExport.ParentAE7F2TotAdj.TotalCurrency;
    export.ParentAD9F1NetCs.TotalCurrency =
      useExport.ParentAD9F1NetCs.TotalCurrency;
    export.ParentBD9F1NetCs.TotalCurrency =
      useExport.ParentBD9F1NetCs.TotalCurrency;
    export.ParentBD8Adjustments.TotalCurrency =
      useExport.ParentBD8Adjustments.TotalCurrency;
    export.ParentAD8Adjustments.TotalCurrency =
      useExport.ParentAD8Adjustments.TotalCurrency;
    export.ParentBD7CsOblig.TotalCurrency =
      useExport.ParentBD7CsOblig.TotalCurrency;
    export.ParentAD7CsOblig.TotalCurrency =
      useExport.ParentAD7CsOblig.TotalCurrency;
    export.D6otalChildSuppOblig.TotalCurrency =
      useExport.D6otalChildSuppOblig.TotalCurrency;
    export.TotalChildCareCost.TotalCurrency =
      useExport.TotalChildCareCost.TotalCurrency;
    export.ParentAChildCareCost.TotalCurrency =
      useExport.ParentAChildCareCost.TotalCurrency;
    export.ParentBChildCareCost.TotalCurrency =
      useExport.ParentBChildCareCost.TotalCurrency;
    export.ParentBTotalTaxCredit.TotalCurrency =
      useExport.ParentBTotalTaxCredit.TotalCurrency;
    export.ParentATotalTaxCredit.TotalCurrency =
      useExport.ParentATotalTaxCredit.TotalCurrency;
    export.TotalInsurancePrem.TotalCurrency =
      useExport.TotalInsurancePrem.TotalCurrency;
    export.CsObligTotalAmount.TotalCurrency =
      useExport.CsObligTotalAmount.TotalCurrency;
    export.CsOblig1618TotalAmt.TotalCurrency =
      useExport.CsOblig1618TotalAmt.TotalCurrency;
    export.CsOblig715TotalAmt.TotalCurrency =
      useExport.CsOblig715TotalAmt.TotalCurrency;
    export.CsOblig06TotalAmt.TotalCurrency =
      useExport.CsOblig06TotalAmt.TotalCurrency;
    export.ParentBD2PercentInc.TotalCurrency =
      useExport.ParentBD2PercentInc.TotalCurrency;
    export.ParentAD2PercentInc.TotalCurrency =
      useExport.ParentAD2PercentInc.TotalCurrency;
    export.ParentBC5D1TotCsInc.TotalCurrency =
      useExport.ParentBC5D1TotCsInc.TotalCurrency;
    export.ParentAC5D1TotCsInc.TotalCurrency =
      useExport.ParentAC5D1TotCsInc.TotalCurrency;
    export.ParentBC1TotGrossInc.TotalCurrency =
      useExport.ParentBC1TotGrossInc.TotalCurrency;
    export.ParentAC1TotGrossInc.TotalCurrency =
      useExport.ParentAC1TotGrossInc.TotalCurrency;
    export.ParentBB3SeGrossInc.TotalCurrency =
      useExport.ParentBB3SeGrossInc.TotalCurrency;
    export.ParentAB3SeGrossInc.TotalCurrency =
      useExport.ParentAB3SeGrossInc.TotalCurrency;
    export.ChildSupportWorksheet.Assign(useExport.ChildSupportWorksheet);
    export.ParentBCsePersonSupportWorksheet.Assign(useExport.ParentB);
    export.ParentACsePersonSupportWorksheet.Assign(useExport.ParentA);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
      public const int Capacity = 7;

      private Common work;
      private CsePersonSupportAdjustment parentB;
      private CsePersonSupportAdjustment parentA;
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
    /// A value of ParentAF2TotalCsAdj.
    /// </summary>
    [JsonPropertyName("parentAF2TotalCsAdj")]
    public Common ParentAF2TotalCsAdj
    {
      get => parentAF2TotalCsAdj ??= new();
      set => parentAF2TotalCsAdj = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

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
    /// A value of ParentBF3AdjCsOblig.
    /// </summary>
    [JsonPropertyName("parentBF3AdjCsOblig")]
    public Common ParentBF3AdjCsOblig
    {
      get => parentBF3AdjCsOblig ??= new();
      set => parentBF3AdjCsOblig = value;
    }

    /// <summary>
    /// A value of ParentBE7F2TotAdjust.
    /// </summary>
    [JsonPropertyName("parentBE7F2TotAdjust")]
    public Common ParentBE7F2TotAdjust
    {
      get => parentBE7F2TotAdjust ??= new();
      set => parentBE7F2TotAdjust = value;
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
    /// A value of ParentAE7F2TotAdjust.
    /// </summary>
    [JsonPropertyName("parentAE7F2TotAdjust")]
    public Common ParentAE7F2TotAdjust
    {
      get => parentAE7F2TotAdjust ??= new();
      set => parentAE7F2TotAdjust = value;
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
    /// A value of ParentBD9F1NetCs.
    /// </summary>
    [JsonPropertyName("parentBD9F1NetCs")]
    public Common ParentBD9F1NetCs
    {
      get => parentBD9F1NetCs ??= new();
      set => parentBD9F1NetCs = value;
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
    /// A value of ParentAD8Adjustments.
    /// </summary>
    [JsonPropertyName("parentAD8Adjustments")]
    public Common ParentAD8Adjustments
    {
      get => parentAD8Adjustments ??= new();
      set => parentAD8Adjustments = value;
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
    /// A value of ParentAD7CsOblig.
    /// </summary>
    [JsonPropertyName("parentAD7CsOblig")]
    public Common ParentAD7CsOblig
    {
      get => parentAD7CsOblig ??= new();
      set => parentAD7CsOblig = value;
    }

    /// <summary>
    /// A value of D6otalChildSuppOblig.
    /// </summary>
    [JsonPropertyName("d6otalChildSuppOblig")]
    public Common D6otalChildSuppOblig
    {
      get => d6otalChildSuppOblig ??= new();
      set => d6otalChildSuppOblig = value;
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
    /// A value of ParentAChildCareCost.
    /// </summary>
    [JsonPropertyName("parentAChildCareCost")]
    public Common ParentAChildCareCost
    {
      get => parentAChildCareCost ??= new();
      set => parentAChildCareCost = value;
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
    /// A value of ParentBTotalTaxCredit.
    /// </summary>
    [JsonPropertyName("parentBTotalTaxCredit")]
    public Common ParentBTotalTaxCredit
    {
      get => parentBTotalTaxCredit ??= new();
      set => parentBTotalTaxCredit = value;
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
    /// A value of TotalInsurancePrem.
    /// </summary>
    [JsonPropertyName("totalInsurancePrem")]
    public Common TotalInsurancePrem
    {
      get => totalInsurancePrem ??= new();
      set => totalInsurancePrem = value;
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
    /// A value of CsOblig1618TotalInc.
    /// </summary>
    [JsonPropertyName("csOblig1618TotalInc")]
    public Common CsOblig1618TotalInc
    {
      get => csOblig1618TotalInc ??= new();
      set => csOblig1618TotalInc = value;
    }

    /// <summary>
    /// A value of CsOblig715TotalAmt.
    /// </summary>
    [JsonPropertyName("csOblig715TotalAmt")]
    public Common CsOblig715TotalAmt
    {
      get => csOblig715TotalAmt ??= new();
      set => csOblig715TotalAmt = value;
    }

    /// <summary>
    /// A value of CsOblig06TotalAmt.
    /// </summary>
    [JsonPropertyName("csOblig06TotalAmt")]
    public Common CsOblig06TotalAmt
    {
      get => csOblig06TotalAmt ??= new();
      set => csOblig06TotalAmt = value;
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
    /// A value of ParentAD2PercentInc.
    /// </summary>
    [JsonPropertyName("parentAD2PercentInc")]
    public Common ParentAD2PercentInc
    {
      get => parentAD2PercentInc ??= new();
      set => parentAD2PercentInc = value;
    }

    /// <summary>
    /// A value of D1otalCsInc.
    /// </summary>
    [JsonPropertyName("d1otalCsInc")]
    public Common D1otalCsInc
    {
      get => d1otalCsInc ??= new();
      set => d1otalCsInc = value;
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
    /// A value of ParentAC5D1TotCsInc.
    /// </summary>
    [JsonPropertyName("parentAC5D1TotCsInc")]
    public Common ParentAC5D1TotCsInc
    {
      get => parentAC5D1TotCsInc ??= new();
      set => parentAC5D1TotCsInc = value;
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
    /// A value of ParentAC1TotGrossInc.
    /// </summary>
    [JsonPropertyName("parentAC1TotGrossInc")]
    public Common ParentAC1TotGrossInc
    {
      get => parentAC1TotGrossInc ??= new();
      set => parentAC1TotGrossInc = value;
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
    /// A value of ParentAB3SeGrossInc.
    /// </summary>
    [JsonPropertyName("parentAB3SeGrossInc")]
    public Common ParentAB3SeGrossInc
    {
      get => parentAB3SeGrossInc ??= new();
      set => parentAB3SeGrossInc = value;
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
    public OeWorkGroup ParentAName
    {
      get => parentAName ??= new();
      set => parentAName = value;
    }

    /// <summary>
    /// A value of ParentBName.
    /// </summary>
    [JsonPropertyName("parentBName")]
    public OeWorkGroup ParentBName
    {
      get => parentBName ??= new();
      set => parentBName = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public AaWork County
    {
      get => county ??= new();
      set => county = value;
    }

    private Common parentBF2TotalCsAdj;
    private Common parentAF2TotalCsAdj;
    private Array<ImportGroup> import1;
    private Common parentBF3AdjCsOblig;
    private Common parentBE7F2TotAdjust;
    private Common parentAF3AdjCsOblig;
    private Common parentAE7F2TotAdjust;
    private Common parentAD9F1NetCs;
    private Common parentBD9F1NetCs;
    private Common parentBD8Adjustments;
    private Common parentAD8Adjustments;
    private Common parentBD7CsOblig;
    private Common parentAD7CsOblig;
    private Common d6otalChildSuppOblig;
    private Common totalChildCareCost;
    private Common parentAChildCareCost;
    private Common parentBChildCareCost;
    private Common parentBTotalTaxCredit;
    private Common parentATotalTaxCredit;
    private Common totalInsurancePrem;
    private Common csObligTotalAmount;
    private Common csOblig1618TotalInc;
    private Common csOblig715TotalAmt;
    private Common csOblig06TotalAmt;
    private Common parentBD2PercentInc;
    private Common parentAD2PercentInc;
    private Common d1otalCsInc;
    private Common parentBC5D1TotCsInc;
    private Common parentAC5D1TotCsInc;
    private Common parentBC1TotGrossInc;
    private Common parentAC1TotGrossInc;
    private Common parentBB3SeGrossInc;
    private Common parentAB3SeGrossInc;
    private ChildSupportWorksheet childSupportWorksheet;
    private CsePersonSupportWorksheet parentBCsePersonSupportWorksheet;
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private CsePerson parentBCsePerson;
    private CsePerson parentACsePerson;
    private Case1 case1;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private OeWorkGroup parentAName;
    private OeWorkGroup parentBName;
    private AaWork county;
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
      public const int Capacity = 7;

      private Common work;
      private CsePersonSupportAdjustment parentB;
      private CsePersonSupportAdjustment parentA;
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
    /// A value of ParentAF2TotalCsAdj.
    /// </summary>
    [JsonPropertyName("parentAF2TotalCsAdj")]
    public Common ParentAF2TotalCsAdj
    {
      get => parentAF2TotalCsAdj ??= new();
      set => parentAF2TotalCsAdj = value;
    }

    /// <summary>
    /// A value of D1otalCsInc.
    /// </summary>
    [JsonPropertyName("d1otalCsInc")]
    public Common D1otalCsInc
    {
      get => d1otalCsInc ??= new();
      set => d1otalCsInc = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

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
    /// A value of ParentBF3AdjCsOblig.
    /// </summary>
    [JsonPropertyName("parentBF3AdjCsOblig")]
    public Common ParentBF3AdjCsOblig
    {
      get => parentBF3AdjCsOblig ??= new();
      set => parentBF3AdjCsOblig = value;
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
    /// A value of ParentAF3AdjCsOblig.
    /// </summary>
    [JsonPropertyName("parentAF3AdjCsOblig")]
    public Common ParentAF3AdjCsOblig
    {
      get => parentAF3AdjCsOblig ??= new();
      set => parentAF3AdjCsOblig = value;
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
    /// A value of ParentAD9F1NetCs.
    /// </summary>
    [JsonPropertyName("parentAD9F1NetCs")]
    public Common ParentAD9F1NetCs
    {
      get => parentAD9F1NetCs ??= new();
      set => parentAD9F1NetCs = value;
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
    /// A value of ParentBD8Adjustments.
    /// </summary>
    [JsonPropertyName("parentBD8Adjustments")]
    public Common ParentBD8Adjustments
    {
      get => parentBD8Adjustments ??= new();
      set => parentBD8Adjustments = value;
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
    /// A value of ParentBD7CsOblig.
    /// </summary>
    [JsonPropertyName("parentBD7CsOblig")]
    public Common ParentBD7CsOblig
    {
      get => parentBD7CsOblig ??= new();
      set => parentBD7CsOblig = value;
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
    /// A value of D6otalChildSuppOblig.
    /// </summary>
    [JsonPropertyName("d6otalChildSuppOblig")]
    public Common D6otalChildSuppOblig
    {
      get => d6otalChildSuppOblig ??= new();
      set => d6otalChildSuppOblig = value;
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
    /// A value of ParentAChildCareCost.
    /// </summary>
    [JsonPropertyName("parentAChildCareCost")]
    public Common ParentAChildCareCost
    {
      get => parentAChildCareCost ??= new();
      set => parentAChildCareCost = value;
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
    /// A value of ParentBTotalTaxCredit.
    /// </summary>
    [JsonPropertyName("parentBTotalTaxCredit")]
    public Common ParentBTotalTaxCredit
    {
      get => parentBTotalTaxCredit ??= new();
      set => parentBTotalTaxCredit = value;
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
    /// A value of TotalInsurancePrem.
    /// </summary>
    [JsonPropertyName("totalInsurancePrem")]
    public Common TotalInsurancePrem
    {
      get => totalInsurancePrem ??= new();
      set => totalInsurancePrem = value;
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
    /// A value of CsOblig1618TotalAmt.
    /// </summary>
    [JsonPropertyName("csOblig1618TotalAmt")]
    public Common CsOblig1618TotalAmt
    {
      get => csOblig1618TotalAmt ??= new();
      set => csOblig1618TotalAmt = value;
    }

    /// <summary>
    /// A value of CsOblig715TotalAmt.
    /// </summary>
    [JsonPropertyName("csOblig715TotalAmt")]
    public Common CsOblig715TotalAmt
    {
      get => csOblig715TotalAmt ??= new();
      set => csOblig715TotalAmt = value;
    }

    /// <summary>
    /// A value of CsOblig06TotalAmt.
    /// </summary>
    [JsonPropertyName("csOblig06TotalAmt")]
    public Common CsOblig06TotalAmt
    {
      get => csOblig06TotalAmt ??= new();
      set => csOblig06TotalAmt = value;
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
    /// A value of ParentAD2PercentInc.
    /// </summary>
    [JsonPropertyName("parentAD2PercentInc")]
    public Common ParentAD2PercentInc
    {
      get => parentAD2PercentInc ??= new();
      set => parentAD2PercentInc = value;
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
    /// A value of ParentAC5D1TotCsInc.
    /// </summary>
    [JsonPropertyName("parentAC5D1TotCsInc")]
    public Common ParentAC5D1TotCsInc
    {
      get => parentAC5D1TotCsInc ??= new();
      set => parentAC5D1TotCsInc = value;
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
    /// A value of ParentAC1TotGrossInc.
    /// </summary>
    [JsonPropertyName("parentAC1TotGrossInc")]
    public Common ParentAC1TotGrossInc
    {
      get => parentAC1TotGrossInc ??= new();
      set => parentAC1TotGrossInc = value;
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
    /// A value of ParentAB3SeGrossInc.
    /// </summary>
    [JsonPropertyName("parentAB3SeGrossInc")]
    public Common ParentAB3SeGrossInc
    {
      get => parentAB3SeGrossInc ??= new();
      set => parentAB3SeGrossInc = value;
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
    public OeWorkGroup ParentAName
    {
      get => parentAName ??= new();
      set => parentAName = value;
    }

    /// <summary>
    /// A value of ParentBName.
    /// </summary>
    [JsonPropertyName("parentBName")]
    public OeWorkGroup ParentBName
    {
      get => parentBName ??= new();
      set => parentBName = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public AaWork County
    {
      get => county ??= new();
      set => county = value;
    }

    private Common parentBF2TotalCsAdj;
    private Common parentAF2TotalCsAdj;
    private Common d1otalCsInc;
    private Array<ExportGroup> export1;
    private Common parentBF3AdjCsOblig;
    private Common parentBE7F2TotAdj;
    private Common parentAF3AdjCsOblig;
    private Common parentAE7F2TotAdj;
    private Common parentAD9F1NetCs;
    private Common parentBD9F1NetCs;
    private Common parentBD8Adjustments;
    private Common parentAD8Adjustments;
    private Common parentBD7CsOblig;
    private Common parentAD7CsOblig;
    private Common d6otalChildSuppOblig;
    private Common totalChildCareCost;
    private Common parentAChildCareCost;
    private Common parentBChildCareCost;
    private Common parentBTotalTaxCredit;
    private Common parentATotalTaxCredit;
    private Common totalInsurancePrem;
    private Common csObligTotalAmount;
    private Common csOblig1618TotalAmt;
    private Common csOblig715TotalAmt;
    private Common csOblig06TotalAmt;
    private Common parentBD2PercentInc;
    private Common parentAD2PercentInc;
    private Common parentBC5D1TotCsInc;
    private Common parentAC5D1TotCsInc;
    private Common parentBC1TotGrossInc;
    private Common parentAC1TotGrossInc;
    private Common parentBB3SeGrossInc;
    private Common parentAB3SeGrossInc;
    private ChildSupportWorksheet childSupportWorksheet;
    private CsePersonSupportWorksheet parentBCsePersonSupportWorksheet;
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private CsePerson parentBCsePerson;
    private CsePerson parentACsePerson;
    private Case1 case1;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private OeWorkGroup parentAName;
    private OeWorkGroup parentBName;
    private AaWork county;
  }
#endregion
}
