// Program: OE_CSSC_CHILD_SUPPORT_SCHEDULE, ID: 1902516517, model: 746.
// Short name: SWECSSCP
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
/// A program: OE_CSSC_CHILD_SUPPORT_SCHEDULE.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeCsscChildSupportSchedule: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CSSC_CHILD_SUPPORT_SCHEDULE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCsscChildSupportSchedule(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCsscChildSupportSchedule.
  /// </summary>
  public OeCsscChildSupportSchedule(IContext context, Import import,
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
    // Date          Author           Reason
    // 03/27/95       	Sid           	Initial Creation
    // 02/06/96      	A. HACKLER      RETRO FITS
    // 11/14/96	R. Marchman	Add new security and next tran.
    // 06/18/97        M. D. Wheaton   Removed datenum
    // 11/16/07        M. Fan          WR318566(CQ297)- Required age groups 
    // changed.
    // 12/14/15	GVandy		CQ50299 - Changes to add CS_Guideline_Year to
    // 				Child_Support_Schedule identifier.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      export.ChildSupportSchedule.Assign(import.ChildSupportSchedule);
      MoveAgeGroupSupportSchedule(import.AgeGrp1, export.AgeGrp1);
      MoveAgeGroupSupportSchedule(import.AgeGrp3, export.AgeGrp3);
      MoveAgeGroupSupportSchedule(import.AgeGrp2, export.AgeGrp2);

      // 11/14/07 MFan WR318566(CQ297)  Added to retain age group descriptions. 
      // ---- start ----
      MoveWorkArea2(import.HeaderAgeGrp1, export.HeaderAgeGrp1);
      MoveWorkArea2(import.HeaderAgeGrp2, export.HeaderAgeGrp2);
      MoveWorkArea1(import.DtlAgeGrp1, export.DtlAgeGrp1);
      MoveWorkArea1(import.DtlAgeGrp2, export.DtlAgeGrp2);
      MoveWorkArea1(import.DtlAgeGrp3, export.DtlAgeGrp3);

      // 11/14/07 MFan WR318566(CQ297)  Added to retain age group descriptions. 
      // ---- end ----
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.ChildSupportSchedule.Assign(import.ChildSupportSchedule);
    MoveAgeGroupSupportSchedule(import.AgeGrp1, export.AgeGrp1);
    MoveAgeGroupSupportSchedule(import.AgeGrp3, export.AgeGrp3);
    MoveAgeGroupSupportSchedule(import.AgeGrp2, export.AgeGrp2);
    MoveChildSupportSchedule(import.PrevH, export.PrevH);
    export.GlYearPrompt.SelectChar = import.GlYearPrompt.SelectChar;

    // 11/16/07 MFan WR318566(CQ297)  Added to get descriptions for each age 
    // group. ---- start ----
    MoveWorkArea1(import.DtlAgeGrp1, export.DtlAgeGrp1);
    MoveWorkArea1(import.DtlAgeGrp2, export.DtlAgeGrp2);
    MoveWorkArea1(import.DtlAgeGrp3, export.DtlAgeGrp3);
    MoveWorkArea2(import.HeaderAgeGrp1, export.HeaderAgeGrp1);
    MoveWorkArea2(import.HeaderAgeGrp2, export.HeaderAgeGrp2);

    if (IsEmpty(import.HeaderAgeGrp1.Text25))
    {
      export.HeaderAgeGrp1.Text25 = "Age Group Factor (0-5)";
      export.HeaderAgeGrp2.Text25 = "Age Group Factor (6-11)";
      export.DtlAgeGrp1.Text10 = "Age 0-5";
      export.DtlAgeGrp2.Text10 = "Age 6-11";
      export.DtlAgeGrp3.Text10 = "Age 12-18";

      // 11/16/07 MFan WR318566(CQ297)  Commented out following lines, not going
      // to use code values yet.
    }

    // 11/16/07 MFan WR318566(CQ297)  Added to get description for each age 
    // group. ---- end ----
    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.AgeGrp1.PerChildSupportAmount =
        import.Import1.Item.AgeGrp1.PerChildSupportAmount;
      export.Export1.Update.AgeGrp3.PerChildSupportAmount =
        import.Import1.Item.AgeGrp3.PerChildSupportAmount;
      export.Export1.Update.AgeGrp2.PerChildSupportAmount =
        import.Import1.Item.AgeGrp2.PerChildSupportAmount;
      export.Export1.Update.Detail.CombinedGrossMnthlyIncomeAmt =
        import.Import1.Item.Detail.CombinedGrossMnthlyIncomeAmt;
      export.Export1.Update.PrevH.CombinedGrossMnthlyIncomeAmt =
        import.Import1.Item.PrevH.CombinedGrossMnthlyIncomeAmt;
      export.Export1.Update.Work.SelectChar =
        import.Import1.Item.Work.SelectChar;

      if (!IsEmpty(import.Import1.Item.Work.SelectChar) && AsChar
        (import.Import1.Item.Work.SelectChar) != 'S' && AsChar
        (import.Import1.Item.Work.SelectChar) != '*')
      {
        var field = GetField(export.Export1.Item.Work, "selectChar");

        field.Error = true;

        ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";
      }
      else if (AsChar(import.Import1.Item.Work.SelectChar) == 'S')
      {
        ++local.WorkSelect.Count;
      }

      export.Export1.Next();
    }

    switch(AsChar(export.GlYearPrompt.SelectChar))
    {
      case ' ':
        if (Equal(global.Command, "LIST"))
        {
          var field1 = GetField(export.GlYearPrompt, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case '+':
        if (Equal(global.Command, "LIST"))
        {
          var field1 = GetField(export.GlYearPrompt, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case 'S':
        if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCDVL"))
        {
          var field1 = GetField(export.GlYearPrompt, "selectChar");

          field1.Error = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";
        }

        break;
      default:
        var field = GetField(export.GlYearPrompt, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // to validate action level security
    if (Equal(global.Command, "RETCDVL"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DELETE":
        if (export.PrevH.CsGuidelineYear <= 0)
        {
          var field = GetField(export.ChildSupportSchedule, "csGuidelineYear");

          field.Error = true;

          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";

          return;
        }

        if (export.PrevH.NumberOfChildrenInFamily <= 0)
        {
          var field =
            GetField(export.ChildSupportSchedule, "numberOfChildrenInFamily");

          field.Error = true;

          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";

          return;
        }

        if (export.PrevH.CsGuidelineYear != export
          .ChildSupportSchedule.CsGuidelineYear)
        {
          var field = GetField(export.ChildSupportSchedule, "csGuidelineYear");

          field.Error = true;

          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";

          return;
        }

        if (export.PrevH.NumberOfChildrenInFamily != export
          .ChildSupportSchedule.NumberOfChildrenInFamily)
        {
          var field =
            GetField(export.ChildSupportSchedule, "numberOfChildrenInFamily");

          field.Error = true;

          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";

          return;
        }

        local.Common.Flag = "";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            local.Common.Flag = "Y";

            if (export.Export1.Item.Detail.CombinedGrossMnthlyIncomeAmt != export
              .Export1.Item.PrevH.CombinedGrossMnthlyIncomeAmt)
            {
              var field =
                GetField(export.Export1.Item.Detail,
                "combinedGrossMnthlyIncomeAmt");

              field.Error = true;

              ExitState = "OE0000_CANT_CHANGE_REC_BEFOR_DEL";
            }
          }
        }

        if (IsEmpty(local.Common.Flag) && !export.Export1.IsEmpty)
        {
          ExitState = "OE0176_CHILD_SUPPORT_DEL_NO_SEL";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            UseOeCsscDelChSuppScheduleDet();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
            {
              export.Export1.Update.Work.SelectChar = "*";
            }
          }
        }

        if (import.Import1.IsEmpty && IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseOeCsscDelChSuppScheduleHdr();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_SUCCESSFUL_DELETE"))
          {
          }
          else
          {
            var field =
              GetField(export.ChildSupportSchedule, "numberOfChildrenInFamily");
              

            field.Error = true;
          }
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "DISPLAY":
        if (import.ChildSupportSchedule.NumberOfChildrenInFamily <= 0)
        {
          var field =
            GetField(export.ChildSupportSchedule, "numberOfChildrenInFamily");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        UseOeCsscListChSuppSchedule();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY") || IsExitState
          ("ACO_NI0000_NO_DATA_TO_DISPLAY"))
        {
          MoveChildSupportSchedule(export.ChildSupportSchedule, export.PrevH);
        }
        else if (IsExitState("CHILD_SUPPORT_SCHEDULE_NF"))
        {
          export.ChildSupportSchedule.Assign(local.Initialized);
          export.ChildSupportSchedule.NumberOfChildrenInFamily =
            import.ChildSupportSchedule.NumberOfChildrenInFamily;
          export.ChildSupportSchedule.CsGuidelineYear =
            import.ChildSupportSchedule.CsGuidelineYear;

          var field1 =
            GetField(export.ChildSupportSchedule, "numberOfChildrenInFamily");

          field1.Error = true;

          var field2 = GetField(export.ChildSupportSchedule, "csGuidelineYear");

          field2.Error = true;
        }

        break;
      case "CREATE":
        if (export.AgeGrp2.AgeGroupFactor <= 0)
        {
          var field = GetField(export.AgeGrp2, "ageGroupFactor");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (export.AgeGrp1.AgeGroupFactor <= 0)
        {
          var field = GetField(export.AgeGrp1, "ageGroupFactor");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (export.ChildSupportSchedule.MonthlyIncomePovertyLevelInd <= 0)
        {
          var field =
            GetField(export.ChildSupportSchedule, "monthlyIncomePovertyLevelInd");
            

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (export.ChildSupportSchedule.IncomeExponent <= 0)
        {
          var field = GetField(export.ChildSupportSchedule, "incomeExponent");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (export.ChildSupportSchedule.IncomeMultiplier <= 0)
        {
          var field = GetField(export.ChildSupportSchedule, "incomeMultiplier");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (import.ChildSupportSchedule.NumberOfChildrenInFamily <= 0)
        {
          var field =
            GetField(export.ChildSupportSchedule, "numberOfChildrenInFamily");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (export.ChildSupportSchedule.CsGuidelineYear <= 0)
        {
          var field = GetField(export.ChildSupportSchedule, "csGuidelineYear");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!Equal(import.ChildSupportSchedule.ExpirationDate, local.Zero.Date))
        {
          if (!Lt(import.ChildSupportSchedule.EffectiveDate,
            import.ChildSupportSchedule.ExpirationDate))
          {
            var field = GetField(export.ChildSupportSchedule, "expirationDate");

            field.Error = true;

            ExitState = "EXPIRE_DATE_PRIOR_TO_EFFECTIVE";
          }

          if (Lt(import.ChildSupportSchedule.ExpirationDate, Now().Date))
          {
            var field = GetField(export.ChildSupportSchedule, "expirationDate");

            field.Error = true;

            ExitState = "EXPIRATION_DATE_PRIOR_TO_CURRENT";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -- Validate the guideline year against the code table
        local.Code.CodeName = "CS VALID GUIDELINE YEARS";
        local.CodeValue.Cdvalue =
          NumberToString(export.ChildSupportSchedule.CsGuidelineYear, 12, 4);

        if (!ReadCodeValue())
        {
          var field = GetField(export.ChildSupportSchedule, "csGuidelineYear");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            if (export.Export1.Item.AgeGrp3.PerChildSupportAmount <= 0)
            {
              var field =
                GetField(export.Export1.Item.AgeGrp3, "perChildSupportAmount");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (export.Export1.Item.AgeGrp2.PerChildSupportAmount <= 0)
            {
              var field =
                GetField(export.Export1.Item.AgeGrp2, "perChildSupportAmount");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (export.Export1.Item.AgeGrp1.PerChildSupportAmount <= 0)
            {
              var field =
                GetField(export.Export1.Item.AgeGrp1, "perChildSupportAmount");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (export.Export1.Item.Detail.CombinedGrossMnthlyIncomeAmt <= 0)
            {
              var field =
                GetField(export.Export1.Item.Detail,
                "combinedGrossMnthlyIncomeAmt");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeCsscAddChSuppScheduleHdr();

        if (IsExitState("OE0000_CHILD_SUPP_SCHED_AE_RB"))
        {
          if (local.WorkSelect.Count <= 0)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (export.Export1.Item.PrevH.CombinedGrossMnthlyIncomeAmt == 0
                && export.Export1.Item.Detail.CombinedGrossMnthlyIncomeAmt > 0)
              {
                var field = GetField(export.Export1.Item.Work, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
              }
            }

            if (IsExitState("OE0000_CHILD_SUPP_SCHED_AE_RB"))
            {
              var field1 =
                GetField(export.ChildSupportSchedule, "csGuidelineYear");

              field1.Error = true;

              var field2 =
                GetField(export.ChildSupportSchedule, "numberOfChildrenInFamily");
                

              field2.Error = true;
            }

            return;
          }
        }
        else if (IsExitState("OE0000_CHILD_SUPP_SCHED_OVERLAP"))
        {
          var field1 = GetField(export.ChildSupportSchedule, "expirationDate");

          field1.Error = true;

          var field2 = GetField(export.ChildSupportSchedule, "effectiveDate");

          field2.Error = true;
        }
        else if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            UseOeCsscAddChSuppScheduleDet();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              export.Export1.Update.Work.SelectChar = "*";
              export.Export1.Update.PrevH.CombinedGrossMnthlyIncomeAmt =
                export.Export1.Item.Detail.CombinedGrossMnthlyIncomeAmt;
            }
            else
            {
              var field =
                GetField(export.Export1.Item.Detail,
                "combinedGrossMnthlyIncomeAmt");

              field.Error = true;
            }
          }
        }

        break;
      case "UPDATE":
        if (export.PrevH.CsGuidelineYear <= 0)
        {
          var field = GetField(export.ChildSupportSchedule, "csGuidelineYear");

          field.Error = true;

          ExitState = "OE0013_DISP_REC_BEFORE_UPD";

          return;
        }

        if (export.PrevH.NumberOfChildrenInFamily <= 0)
        {
          var field =
            GetField(export.ChildSupportSchedule, "numberOfChildrenInFamily");

          field.Error = true;

          ExitState = "OE0013_DISP_REC_BEFORE_UPD";

          return;
        }

        if (export.PrevH.CsGuidelineYear != export
          .ChildSupportSchedule.CsGuidelineYear)
        {
          var field = GetField(export.ChildSupportSchedule, "csGuidelineYear");

          field.Error = true;

          ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";

          return;
        }

        if (export.PrevH.NumberOfChildrenInFamily != export
          .ChildSupportSchedule.NumberOfChildrenInFamily)
        {
          var field =
            GetField(export.ChildSupportSchedule, "numberOfChildrenInFamily");

          field.Error = true;

          ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";

          return;
        }

        if (export.AgeGrp2.AgeGroupFactor <= 0)
        {
          var field = GetField(export.AgeGrp2, "ageGroupFactor");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (export.AgeGrp1.AgeGroupFactor <= 0)
        {
          var field = GetField(export.AgeGrp1, "ageGroupFactor");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (export.ChildSupportSchedule.MonthlyIncomePovertyLevelInd <= 0)
        {
          var field =
            GetField(export.ChildSupportSchedule, "monthlyIncomePovertyLevelInd");
            

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (export.ChildSupportSchedule.IncomeExponent <= 0)
        {
          var field = GetField(export.ChildSupportSchedule, "incomeExponent");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (export.ChildSupportSchedule.IncomeMultiplier <= 0)
        {
          var field = GetField(export.ChildSupportSchedule, "incomeMultiplier");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!Equal(import.ChildSupportSchedule.ExpirationDate, local.Zero.Date))
        {
          if (!Lt(import.ChildSupportSchedule.EffectiveDate,
            import.ChildSupportSchedule.ExpirationDate))
          {
            var field = GetField(export.ChildSupportSchedule, "expirationDate");

            field.Error = true;

            ExitState = "EXPIRE_DATE_PRIOR_TO_EFFECTIVE";
          }

          if (Lt(import.ChildSupportSchedule.ExpirationDate, Now().Date))
          {
            var field = GetField(export.ChildSupportSchedule, "expirationDate");

            field.Error = true;

            ExitState = "EXPIRATION_DATE_PRIOR_TO_CURRENT";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            if (export.Export1.Item.AgeGrp3.PerChildSupportAmount <= 0)
            {
              var field =
                GetField(export.Export1.Item.AgeGrp3, "perChildSupportAmount");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (export.Export1.Item.AgeGrp2.PerChildSupportAmount <= 0)
            {
              var field =
                GetField(export.Export1.Item.AgeGrp2, "perChildSupportAmount");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (export.Export1.Item.AgeGrp1.PerChildSupportAmount <= 0)
            {
              var field =
                GetField(export.Export1.Item.AgeGrp1, "perChildSupportAmount");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (export.Export1.Item.Detail.CombinedGrossMnthlyIncomeAmt != export
              .Export1.Item.PrevH.CombinedGrossMnthlyIncomeAmt)
            {
              var field =
                GetField(export.Export1.Item.Detail,
                "combinedGrossMnthlyIncomeAmt");

              field.Error = true;

              ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeCsscUpdChSuppScheduleHdr();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            UseOeCsscUpdChSuppScheduleDet();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
              export.Export1.Update.Work.SelectChar = "*";
              export.Export1.Update.PrevH.CombinedGrossMnthlyIncomeAmt =
                export.Export1.Item.Detail.CombinedGrossMnthlyIncomeAmt;
            }
          }
        }

        break;
      case "PREV":
        ExitState = "ZD_ACO_NE0000_INVALID_BACKWARD_2";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "LIST":
        export.ToCdvlActiveCodesOnly.Flag = "N";
        export.ToCdvl.CodeName = "CS VALID GUIDELINE YEARS";
        ExitState = "ECO_LNK_TO_CODE_VALUES";

        return;
      case "RETCDVL":
        if (IsEmpty(import.FromCdvl.Cdvalue))
        {
        }
        else
        {
          export.ChildSupportSchedule.CsGuidelineYear =
            (int)StringToNumber(import.FromCdvl.Cdvalue);
          export.GlYearPrompt.SelectChar = "";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    global.Command = "";
  }

  private static void MoveAgeGroupSupportSchedule(
    AgeGroupSupportSchedule source, AgeGroupSupportSchedule target)
  {
    target.MaximumAgeInARange = source.MaximumAgeInARange;
    target.AgeGroupFactor = source.AgeGroupFactor;
  }

  private static void MoveChildSupportSchedule(ChildSupportSchedule source,
    ChildSupportSchedule target)
  {
    target.NumberOfChildrenInFamily = source.NumberOfChildrenInFamily;
    target.CsGuidelineYear = source.CsGuidelineYear;
  }

  private static void MoveExport1(OeCsscListChSuppSchedule.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.PrevH.CombinedGrossMnthlyIncomeAmt =
      source.PrevH.CombinedGrossMnthlyIncomeAmt;
    target.Work.SelectChar = source.Work.SelectChar;
    target.Detail.CombinedGrossMnthlyIncomeAmt =
      source.Detail.CombinedGrossMnthlyIncomeAmt;
    target.AgeGrp1.PerChildSupportAmount =
      source.Export06.PerChildSupportAmount;
    target.AgeGrp2.PerChildSupportAmount =
      source.Export715.PerChildSupportAmount;
    target.AgeGrp3.PerChildSupportAmount =
      source.Export1618.PerChildSupportAmount;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveWorkArea1(WorkArea source, WorkArea target)
  {
    target.Text5 = source.Text5;
    target.Text10 = source.Text10;
  }

  private static void MoveWorkArea2(WorkArea source, WorkArea target)
  {
    target.Text5 = source.Text5;
    target.Text25 = source.Text25;
  }

  private void UseOeCsscAddChSuppScheduleDet()
  {
    var useImport = new OeCsscAddChSuppScheduleDet.Import();
    var useExport = new OeCsscAddChSuppScheduleDet.Export();

    useImport.ChildSupportSchedule.Assign(import.ChildSupportSchedule);
    MoveAgeGroupSupportSchedule(export.AgeGrp3,
      useImport.Import1618AgeGroupSupportSchedule);
    MoveAgeGroupSupportSchedule(export.AgeGrp2,
      useImport.Import715AgeGroupSupportSchedule);
    MoveAgeGroupSupportSchedule(export.AgeGrp1,
      useImport.Import06AgeGroupSupportSchedule);
    useImport.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
      export.Export1.Item.Detail.CombinedGrossMnthlyIncomeAmt;
    useImport.Import06CsGrossMonthlyIncSched.PerChildSupportAmount =
      export.Export1.Item.AgeGrp1.PerChildSupportAmount;
    useImport.Import715CsGrossMonthlyIncSched.PerChildSupportAmount =
      export.Export1.Item.AgeGrp2.PerChildSupportAmount;
    useImport.Import1618CsGrossMonthlyIncSched.PerChildSupportAmount =
      export.Export1.Item.AgeGrp3.PerChildSupportAmount;

    Call(OeCsscAddChSuppScheduleDet.Execute, useImport, useExport);

    export.ChildSupportSchedule.Assign(useExport.ChildSupportSchedule);
  }

  private void UseOeCsscAddChSuppScheduleHdr()
  {
    var useImport = new OeCsscAddChSuppScheduleHdr.Import();
    var useExport = new OeCsscAddChSuppScheduleHdr.Export();

    useImport.ChildSupportSchedule.Assign(import.ChildSupportSchedule);
    MoveAgeGroupSupportSchedule(import.AgeGrp3, useImport.Import1618);
    MoveAgeGroupSupportSchedule(import.AgeGrp2, useImport.Import715);
    MoveAgeGroupSupportSchedule(import.AgeGrp1, useImport.Import06);

    Call(OeCsscAddChSuppScheduleHdr.Execute, useImport, useExport);

    export.ChildSupportSchedule.Assign(useExport.ChildSupportSchedule);
  }

  private void UseOeCsscDelChSuppScheduleDet()
  {
    var useImport = new OeCsscDelChSuppScheduleDet.Import();
    var useExport = new OeCsscDelChSuppScheduleDet.Export();

    useImport.ChildSupportSchedule.Assign(import.ChildSupportSchedule);
    MoveAgeGroupSupportSchedule(import.AgeGrp3,
      useImport.Import1618AgeGroupSupportSchedule);
    MoveAgeGroupSupportSchedule(import.AgeGrp2,
      useImport.Import715AgeGroupSupportSchedule);
    MoveAgeGroupSupportSchedule(import.AgeGrp1,
      useImport.Import06AgeGroupSupportSchedule);
    useImport.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
      export.Export1.Item.Detail.CombinedGrossMnthlyIncomeAmt;
    useImport.Import06CsGrossMonthlyIncSched.PerChildSupportAmount =
      export.Export1.Item.AgeGrp1.PerChildSupportAmount;
    useImport.Import715CsGrossMonthlyIncSched.PerChildSupportAmount =
      export.Export1.Item.AgeGrp2.PerChildSupportAmount;
    useImport.Import1618CsGrossMonthlyIncSched.PerChildSupportAmount =
      export.Export1.Item.AgeGrp3.PerChildSupportAmount;

    Call(OeCsscDelChSuppScheduleDet.Execute, useImport, useExport);

    export.ChildSupportSchedule.Assign(useExport.ChildSupportSchedule);
  }

  private void UseOeCsscDelChSuppScheduleHdr()
  {
    var useImport = new OeCsscDelChSuppScheduleHdr.Import();
    var useExport = new OeCsscDelChSuppScheduleHdr.Export();

    useImport.ChildSupportSchedule.Assign(import.ChildSupportSchedule);

    Call(OeCsscDelChSuppScheduleHdr.Execute, useImport, useExport);
  }

  private void UseOeCsscListChSuppSchedule()
  {
    var useImport = new OeCsscListChSuppSchedule.Import();
    var useExport = new OeCsscListChSuppSchedule.Export();

    useImport.ChildSupportSchedule.Assign(import.ChildSupportSchedule);

    Call(OeCsscListChSuppSchedule.Execute, useImport, useExport);

    export.ChildSupportSchedule.Assign(useExport.ChildSupportSchedule);
    MoveAgeGroupSupportSchedule(useExport.Export1618, export.AgeGrp3);
    MoveAgeGroupSupportSchedule(useExport.Export715, export.AgeGrp2);
    MoveAgeGroupSupportSchedule(useExport.Export06, export.AgeGrp1);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseOeCsscUpdChSuppScheduleDet()
  {
    var useImport = new OeCsscUpdChSuppScheduleDet.Import();
    var useExport = new OeCsscUpdChSuppScheduleDet.Export();

    useImport.ChildSupportSchedule.Assign(import.ChildSupportSchedule);
    MoveAgeGroupSupportSchedule(import.AgeGrp3,
      useImport.Import1618AgeGroupSupportSchedule);
    MoveAgeGroupSupportSchedule(import.AgeGrp2,
      useImport.Import715AgeGroupSupportSchedule);
    MoveAgeGroupSupportSchedule(import.AgeGrp1,
      useImport.Import06AgeGroupSupportSchedule);
    useImport.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
      export.Export1.Item.Detail.CombinedGrossMnthlyIncomeAmt;
    useImport.Import06CsGrossMonthlyIncSched.PerChildSupportAmount =
      export.Export1.Item.AgeGrp1.PerChildSupportAmount;
    useImport.Import715CsGrossMonthlyIncSched.PerChildSupportAmount =
      export.Export1.Item.AgeGrp2.PerChildSupportAmount;
    useImport.Import1618CsGrossMonthlyIncSched.PerChildSupportAmount =
      export.Export1.Item.AgeGrp3.PerChildSupportAmount;

    Call(OeCsscUpdChSuppScheduleDet.Execute, useImport, useExport);

    export.ChildSupportSchedule.Assign(useExport.ChildSupportSchedule);
  }

  private void UseOeCsscUpdChSuppScheduleHdr()
  {
    var useImport = new OeCsscUpdChSuppScheduleHdr.Import();
    var useExport = new OeCsscUpdChSuppScheduleHdr.Export();

    useImport.ChildSupportSchedule.Assign(import.ChildSupportSchedule);
    MoveAgeGroupSupportSchedule(import.AgeGrp3, useImport.Import1618);
    MoveAgeGroupSupportSchedule(import.AgeGrp2, useImport.Import715);
    MoveAgeGroupSupportSchedule(import.AgeGrp1, useImport.Import06);

    Call(OeCsscUpdChSuppScheduleHdr.Execute, useImport, useExport);

    export.ChildSupportSchedule.Assign(useExport.ChildSupportSchedule);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetString(command, "codeName", local.Code.CodeName);
        db.SetString(command, "cdvalue", local.CodeValue.Cdvalue);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Populated = true;
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
      /// A value of PrevH.
      /// </summary>
      [JsonPropertyName("prevH")]
      public CsGrossMonthlyIncSched PrevH
      {
        get => prevH ??= new();
        set => prevH = value;
      }

      /// <summary>
      /// A value of AgeGrp1.
      /// </summary>
      [JsonPropertyName("ageGrp1")]
      public CsGrossMonthlyIncSched AgeGrp1
      {
        get => ageGrp1 ??= new();
        set => ageGrp1 = value;
      }

      /// <summary>
      /// A value of AgeGrp2.
      /// </summary>
      [JsonPropertyName("ageGrp2")]
      public CsGrossMonthlyIncSched AgeGrp2
      {
        get => ageGrp2 ??= new();
        set => ageGrp2 = value;
      }

      /// <summary>
      /// A value of AgeGrp3.
      /// </summary>
      [JsonPropertyName("ageGrp3")]
      public CsGrossMonthlyIncSched AgeGrp3
      {
        get => ageGrp3 ??= new();
        set => ageGrp3 = value;
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

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsGrossMonthlyIncSched Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 180;

      private CsGrossMonthlyIncSched prevH;
      private CsGrossMonthlyIncSched ageGrp1;
      private CsGrossMonthlyIncSched ageGrp2;
      private CsGrossMonthlyIncSched ageGrp3;
      private Common work;
      private CsGrossMonthlyIncSched detail;
    }

    /// <summary>
    /// A value of PrevH.
    /// </summary>
    [JsonPropertyName("prevH")]
    public ChildSupportSchedule PrevH
    {
      get => prevH ??= new();
      set => prevH = value;
    }

    /// <summary>
    /// A value of ChildSupportSchedule.
    /// </summary>
    [JsonPropertyName("childSupportSchedule")]
    public ChildSupportSchedule ChildSupportSchedule
    {
      get => childSupportSchedule ??= new();
      set => childSupportSchedule = value;
    }

    /// <summary>
    /// A value of AgeGrp3.
    /// </summary>
    [JsonPropertyName("ageGrp3")]
    public AgeGroupSupportSchedule AgeGrp3
    {
      get => ageGrp3 ??= new();
      set => ageGrp3 = value;
    }

    /// <summary>
    /// A value of AgeGrp2.
    /// </summary>
    [JsonPropertyName("ageGrp2")]
    public AgeGroupSupportSchedule AgeGrp2
    {
      get => ageGrp2 ??= new();
      set => ageGrp2 = value;
    }

    /// <summary>
    /// A value of AgeGrp1.
    /// </summary>
    [JsonPropertyName("ageGrp1")]
    public AgeGroupSupportSchedule AgeGrp1
    {
      get => ageGrp1 ??= new();
      set => ageGrp1 = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of HeaderAgeGrp1.
    /// </summary>
    [JsonPropertyName("headerAgeGrp1")]
    public WorkArea HeaderAgeGrp1
    {
      get => headerAgeGrp1 ??= new();
      set => headerAgeGrp1 = value;
    }

    /// <summary>
    /// A value of HeaderAgeGrp2.
    /// </summary>
    [JsonPropertyName("headerAgeGrp2")]
    public WorkArea HeaderAgeGrp2
    {
      get => headerAgeGrp2 ??= new();
      set => headerAgeGrp2 = value;
    }

    /// <summary>
    /// A value of DtlAgeGrp1.
    /// </summary>
    [JsonPropertyName("dtlAgeGrp1")]
    public WorkArea DtlAgeGrp1
    {
      get => dtlAgeGrp1 ??= new();
      set => dtlAgeGrp1 = value;
    }

    /// <summary>
    /// A value of DtlAgeGrp2.
    /// </summary>
    [JsonPropertyName("dtlAgeGrp2")]
    public WorkArea DtlAgeGrp2
    {
      get => dtlAgeGrp2 ??= new();
      set => dtlAgeGrp2 = value;
    }

    /// <summary>
    /// A value of DtlAgeGrp3.
    /// </summary>
    [JsonPropertyName("dtlAgeGrp3")]
    public WorkArea DtlAgeGrp3
    {
      get => dtlAgeGrp3 ??= new();
      set => dtlAgeGrp3 = value;
    }

    /// <summary>
    /// A value of FromCdvl.
    /// </summary>
    [JsonPropertyName("fromCdvl")]
    public CodeValue FromCdvl
    {
      get => fromCdvl ??= new();
      set => fromCdvl = value;
    }

    /// <summary>
    /// A value of GlYearPrompt.
    /// </summary>
    [JsonPropertyName("glYearPrompt")]
    public Common GlYearPrompt
    {
      get => glYearPrompt ??= new();
      set => glYearPrompt = value;
    }

    private ChildSupportSchedule prevH;
    private ChildSupportSchedule childSupportSchedule;
    private AgeGroupSupportSchedule ageGrp3;
    private AgeGroupSupportSchedule ageGrp2;
    private AgeGroupSupportSchedule ageGrp1;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea headerAgeGrp1;
    private WorkArea headerAgeGrp2;
    private WorkArea dtlAgeGrp1;
    private WorkArea dtlAgeGrp2;
    private WorkArea dtlAgeGrp3;
    private CodeValue fromCdvl;
    private Common glYearPrompt;
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
      /// A value of PrevH.
      /// </summary>
      [JsonPropertyName("prevH")]
      public CsGrossMonthlyIncSched PrevH
      {
        get => prevH ??= new();
        set => prevH = value;
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

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsGrossMonthlyIncSched Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of AgeGrp1.
      /// </summary>
      [JsonPropertyName("ageGrp1")]
      public CsGrossMonthlyIncSched AgeGrp1
      {
        get => ageGrp1 ??= new();
        set => ageGrp1 = value;
      }

      /// <summary>
      /// A value of AgeGrp2.
      /// </summary>
      [JsonPropertyName("ageGrp2")]
      public CsGrossMonthlyIncSched AgeGrp2
      {
        get => ageGrp2 ??= new();
        set => ageGrp2 = value;
      }

      /// <summary>
      /// A value of AgeGrp3.
      /// </summary>
      [JsonPropertyName("ageGrp3")]
      public CsGrossMonthlyIncSched AgeGrp3
      {
        get => ageGrp3 ??= new();
        set => ageGrp3 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 180;

      private CsGrossMonthlyIncSched prevH;
      private Common work;
      private CsGrossMonthlyIncSched detail;
      private CsGrossMonthlyIncSched ageGrp1;
      private CsGrossMonthlyIncSched ageGrp2;
      private CsGrossMonthlyIncSched ageGrp3;
    }

    /// <summary>
    /// A value of ToCdvlActiveCodesOnly.
    /// </summary>
    [JsonPropertyName("toCdvlActiveCodesOnly")]
    public Common ToCdvlActiveCodesOnly
    {
      get => toCdvlActiveCodesOnly ??= new();
      set => toCdvlActiveCodesOnly = value;
    }

    /// <summary>
    /// A value of PrevH.
    /// </summary>
    [JsonPropertyName("prevH")]
    public ChildSupportSchedule PrevH
    {
      get => prevH ??= new();
      set => prevH = value;
    }

    /// <summary>
    /// A value of ChildSupportSchedule.
    /// </summary>
    [JsonPropertyName("childSupportSchedule")]
    public ChildSupportSchedule ChildSupportSchedule
    {
      get => childSupportSchedule ??= new();
      set => childSupportSchedule = value;
    }

    /// <summary>
    /// A value of AgeGrp3.
    /// </summary>
    [JsonPropertyName("ageGrp3")]
    public AgeGroupSupportSchedule AgeGrp3
    {
      get => ageGrp3 ??= new();
      set => ageGrp3 = value;
    }

    /// <summary>
    /// A value of AgeGrp2.
    /// </summary>
    [JsonPropertyName("ageGrp2")]
    public AgeGroupSupportSchedule AgeGrp2
    {
      get => ageGrp2 ??= new();
      set => ageGrp2 = value;
    }

    /// <summary>
    /// A value of AgeGrp1.
    /// </summary>
    [JsonPropertyName("ageGrp1")]
    public AgeGroupSupportSchedule AgeGrp1
    {
      get => ageGrp1 ??= new();
      set => ageGrp1 = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of HeaderAgeGrp1.
    /// </summary>
    [JsonPropertyName("headerAgeGrp1")]
    public WorkArea HeaderAgeGrp1
    {
      get => headerAgeGrp1 ??= new();
      set => headerAgeGrp1 = value;
    }

    /// <summary>
    /// A value of HeaderAgeGrp2.
    /// </summary>
    [JsonPropertyName("headerAgeGrp2")]
    public WorkArea HeaderAgeGrp2
    {
      get => headerAgeGrp2 ??= new();
      set => headerAgeGrp2 = value;
    }

    /// <summary>
    /// A value of DtlAgeGrp1.
    /// </summary>
    [JsonPropertyName("dtlAgeGrp1")]
    public WorkArea DtlAgeGrp1
    {
      get => dtlAgeGrp1 ??= new();
      set => dtlAgeGrp1 = value;
    }

    /// <summary>
    /// A value of DtlAgeGrp2.
    /// </summary>
    [JsonPropertyName("dtlAgeGrp2")]
    public WorkArea DtlAgeGrp2
    {
      get => dtlAgeGrp2 ??= new();
      set => dtlAgeGrp2 = value;
    }

    /// <summary>
    /// A value of DtlAgeGrp3.
    /// </summary>
    [JsonPropertyName("dtlAgeGrp3")]
    public WorkArea DtlAgeGrp3
    {
      get => dtlAgeGrp3 ??= new();
      set => dtlAgeGrp3 = value;
    }

    /// <summary>
    /// A value of ToCdvl.
    /// </summary>
    [JsonPropertyName("toCdvl")]
    public Code ToCdvl
    {
      get => toCdvl ??= new();
      set => toCdvl = value;
    }

    /// <summary>
    /// A value of GlYearPrompt.
    /// </summary>
    [JsonPropertyName("glYearPrompt")]
    public Common GlYearPrompt
    {
      get => glYearPrompt ??= new();
      set => glYearPrompt = value;
    }

    private Common toCdvlActiveCodesOnly;
    private ChildSupportSchedule prevH;
    private ChildSupportSchedule childSupportSchedule;
    private AgeGroupSupportSchedule ageGrp3;
    private AgeGroupSupportSchedule ageGrp2;
    private AgeGroupSupportSchedule ageGrp1;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea headerAgeGrp1;
    private WorkArea headerAgeGrp2;
    private WorkArea dtlAgeGrp1;
    private WorkArea dtlAgeGrp2;
    private WorkArea dtlAgeGrp3;
    private Code toCdvl;
    private Common glYearPrompt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of WorkSelect.
    /// </summary>
    [JsonPropertyName("workSelect")]
    public Common WorkSelect
    {
      get => workSelect ??= new();
      set => workSelect = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public ChildSupportSchedule Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    private Common validCode;
    private Common subscript;
    private Common common;
    private DateWorkArea zero;
    private Common workSelect;
    private ChildSupportSchedule initialized;
    private Code code;
    private CodeValue codeValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    private CodeValue codeValue;
    private Code code;
  }
#endregion
}
