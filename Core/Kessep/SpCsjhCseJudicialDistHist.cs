// Program: SP_CSJH_CSE_JUDICIAL_DIST_HIST, ID: 1625341113, model: 746.
// Short name: SWECSJHP
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
/// A program: SP_CSJH_CSE_JUDICIAL_DIST_HIST.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCsjhCseJudicialDistHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CSJH_CSE_JUDICIAL_DIST_HIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCsjhCseJudicialDistHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCsjhCseJudicialDistHist.
  /// </summary>
  public SpCsjhCseJudicialDistHist(IContext context, Import import,
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
    // ***************************************************************************************
    // Date      Developer         Request #  	Description
    // --------  ----------------  ---------  	------------------------
    // 01/04/2019  D Dupree			CREATE TEMPLATE
    // ***************************************************************************************
    // ********************************************
    // * Set initial EXIT STATE.
    // *
    // * Move all IMPORTs to EXPORTs.
    // ********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      // *** per the user SME, Sana Beall, she wants to be able to signoff at 
      // any point in time.
      UseScCabSignoff();

      return;
    }

    local.Current.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);
    MoveStandard(import.Standard, export.Standard);
    export.Minus.Text1 = import.Minus.Text1;
    export.Plus.Text1 = import.Plus.Text1;
    export.Active.Flag = import.Active.Flag;

    if (IsEmpty(import.Active.Flag))
    {
      export.Active.Flag = "Y";
    }

    export.Contractor.Assign(import.Contractor);
    export.ContractorHidden.Assign(import.ContractorHidden);
    export.PromptContractorCd.Flag = import.PromptContractorCd.Flag;
    export.CseOrganizationRelationship.ReasonCode =
      import.CseOrganizationRelationship.ReasonCode;
    export.HiddenCseOrganizationRelationship.ReasonCode =
      import.HiddenCseOrganizationRelationship.ReasonCode;
    export.PromptJudicalDistrict.Flag = import.PromptJudicalDistrict.Flag;
    export.Hierarchy.Description = import.Hierarchy.Description;
    export.JudicalDistrict.Assign(import.JudicalDistrict);
    export.JudicalDistrictHidden.Assign(import.JudicalDistrictHidden);
    export.StartingFilter.Code = import.StartingFilter.Code;

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      export.Group.Index = import.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.EffDate.Date = import.Group.Item.EffDate.Date;
      export.Group.Update.EndDate.Date = import.Group.Item.EndDate.Date;
      export.Group.Update.Jd.Assign(import.Group.Item.Jd);
      export.Group.Update.Contractor.Assign(import.Group.Item.Contractor);
      export.Group.Update.ContractorHistory.Assign(
        import.Group.Item.ContractorHistory);
    }

    import.Group.CheckIndex();

    for(import.Page.Index = 0; import.Page.Index < import.Page.Count; ++
      import.Page.Index)
    {
      if (!import.Page.CheckSize())
      {
        break;
      }

      export.Page.Index = import.Page.Index;
      export.Page.CheckSize();

      export.Page.Update.Page1.Assign(import.Page.Item.Page1);
      export.Page.Update.ContractorPage.Code =
        import.Page.Item.ContractorPage.Code;
      export.Page.Update.JudicalDistrictPage.Code =
        import.Page.Item.JudicalDistrictPage.Code;
    }

    import.Page.CheckIndex();

    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        // ***if the next tran info is not equal to spaces, this implies the 
        // user requested a next tran action. now validate
        UseScCabNextTranPut();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }

        return;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ***  this is where you set your export value to the export hidden next 
      // tran values if the user is comming into this procedure on a next tran
      // action.
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // *** this is where you set your command to do whatever is necessary to 
      // do on a flow from the menu, maybe just escape....
      // *** You should get this information from the Dialog Flow Diagram.  It 
      // is the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    if (Equal(global.Command, "RTLIST"))
    {
      if (AsChar(export.PromptContractorCd.Flag) == 'S')
      {
        // ********************************************
        // * Return from selecting a Parent
        // * Organization and force a display.
        // ********************************************
        export.PromptContractorCd.Flag = "";
        export.StartingFilter.Code = "";

        var field = GetField(export.PromptContractorCd, "flag");

        field.Protected = false;
        field.Focused = true;

        if (!IsEmpty(import.FromCsor.Code))
        {
          export.Contractor.Type1 = import.FromCsor.Type1;
          export.Contractor.Code = import.FromCsor.Code;
          export.Contractor.Name = import.FromCsor.Name;
          global.Command = "DISPLAY";
        }
      }
      else
      {
        // ********************************************
        // * Return from selecting a Child Organization.
        // ********************************************
        if (AsChar(import.PromptJudicalDistrict.Flag) == 'S')
        {
          export.JudicalDistrict.Type1 = import.FromCsor.Type1;
          export.JudicalDistrict.Code = import.FromCsor.Code;
          export.JudicalDistrict.Name = import.FromCsor.Name;
          global.Command = "DISPLAY";
          export.PromptJudicalDistrict.Flag = "";

          var field = GetField(export.PromptJudicalDistrict, "flag");

          field.Protected = false;
          field.Focused = true;
        }
      }
    }

    // ********************************************
    // * Main Case of Command
    // ********************************************
    switch(TrimEnd(global.Command))
    {
      case "RTLIST":
        break;
      case "DISPLAY":
        // ********************************************
        // * Check for mandatory fields.
        // ********************************************
        if (!IsEmpty(export.Contractor.Type1))
        {
          // ********************************************
          // * Validate Parent Organization Type.
          // ********************************************
          local.Code.CodeName = "CSE ORGANIZATION TYPE";
          local.CodeValue.Cdvalue = export.Contractor.Type1;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'Y')
          {
          }
          else
          {
            var field = GetField(export.Contractor, "type1");

            field.Error = true;

            ExitState = "INVALID_TYPE_CODE";

            return;
          }
        }

        // ******************************************
        // * Validate Parent CSE Organization type
        // * against the relationship reason code.
        // ******************************************
        if (!IsEmpty(export.Contractor.Code))
        {
          if (ReadCseOrganization1())
          {
            export.Contractor.Assign(entities.Contractor);
            export.ContractorHidden.Assign(entities.Contractor);
          }
          else
          {
            var field = GetField(export.Contractor, "code");

            field.Error = true;

            ExitState = "CSE_ORGANIZATION_NF";

            return;
          }
        }

        if (!IsEmpty(export.JudicalDistrict.Code))
        {
          if (ReadCseOrganization2())
          {
            export.JudicalDistrict.Assign(entities.JudicalDistrict);
            export.JudicalDistrictHidden.Assign(entities.JudicalDistrict);
          }
          else
          {
            var field = GetField(export.JudicalDistrict, "code");

            field.Error = true;

            ExitState = "CSE_ORGANIZATION_NF";

            return;
          }
        }

        export.Group.Index = -1;
        export.Group.Count = 0;

        foreach(var item in ReadContractorHistoryCseOrganizationCseOrganization2())
          
        {
          if (Equal(entities.JudicalDistrict.Code, "32"))
          {
            continue;
          }

          if (AsChar(import.Active.Flag) == 'Y')
          {
            // active only records
            if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
            {
              if (!IsEmpty(export.Contractor.Code))
              {
                if (Equal(entities.Contractor.Code, export.Contractor.Code) && (
                  !IsEmpty(export.JudicalDistrict.Code) && Equal
                  (entities.JudicalDistrict.Code, export.JudicalDistrict.Code) ||
                  IsEmpty(export.JudicalDistrict.Code)))
                {
                  ++export.Group.Index;
                  export.Group.CheckSize();

                  export.Group.Update.ContractorHistory.Assign(
                    entities.ContractorHistory);
                  export.Group.Update.Contractor.Assign(entities.Contractor);
                  export.Group.Update.Jd.Assign(entities.JudicalDistrict);
                  export.Group.Update.EffDate.Date =
                    entities.ContractorHistory.EffectiveDate;
                  export.Group.Update.EndDate.Date =
                    local.NullDateWorkArea.Date;
                }
              }
              else if (!IsEmpty(export.JudicalDistrict.Code))
              {
                if (Equal(entities.JudicalDistrict.Code,
                  export.JudicalDistrict.Code) && (
                    !IsEmpty(export.Contractor.Code) && Equal
                  (entities.Contractor.Code, export.Contractor.Code) || IsEmpty
                  (export.Contractor.Code)))
                {
                  ++export.Group.Index;
                  export.Group.CheckSize();

                  export.Group.Update.ContractorHistory.Assign(
                    entities.ContractorHistory);
                  export.Group.Update.Jd.Assign(entities.JudicalDistrict);
                  export.Group.Update.Contractor.Assign(entities.Contractor);
                  export.Group.Update.EffDate.Date =
                    entities.ContractorHistory.EffectiveDate;
                  export.Group.Update.EndDate.Date =
                    local.NullDateWorkArea.Date;
                }
              }
              else if (IsEmpty(export.Contractor.Code) && IsEmpty
                (export.JudicalDistrict.Code))
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                export.Group.Update.ContractorHistory.Assign(
                  entities.ContractorHistory);
                export.Group.Update.Jd.Assign(entities.JudicalDistrict);
                export.Group.Update.Contractor.Assign(entities.Contractor);
                export.Group.Update.EffDate.Date =
                  entities.ContractorHistory.EffectiveDate;
                export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
              }
            }
            else
            {
              continue;
            }
          }
          else if (!IsEmpty(export.Contractor.Code))
          {
            if (Equal(entities.Contractor.Code, export.Contractor.Code) && (
              !IsEmpty(export.JudicalDistrict.Code) && Equal
              (entities.JudicalDistrict.Code, export.JudicalDistrict.Code) || IsEmpty
              (export.JudicalDistrict.Code)))
            {
              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ContractorHistory.Assign(
                entities.ContractorHistory);
              export.Group.Update.Contractor.Assign(entities.Contractor);
              export.Group.Update.Jd.Assign(entities.JudicalDistrict);
              export.Group.Update.EffDate.Date =
                entities.ContractorHistory.EffectiveDate;

              if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
              {
                export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
              }
              else
              {
                export.Group.Update.EndDate.Date =
                  entities.ContractorHistory.EndDate;
              }
            }
          }
          else if (!IsEmpty(export.JudicalDistrict.Code))
          {
            if (Equal(entities.JudicalDistrict.Code, export.JudicalDistrict.Code)
              && (!IsEmpty(export.Contractor.Code) && Equal
              (entities.Contractor.Code, export.Contractor.Code) || IsEmpty
              (export.Contractor.Code)))
            {
              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ContractorHistory.Assign(
                entities.ContractorHistory);
              export.Group.Update.Jd.Assign(entities.JudicalDistrict);
              export.Group.Update.Contractor.Assign(entities.Contractor);
              export.Group.Update.EffDate.Date =
                entities.ContractorHistory.EffectiveDate;

              if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
              {
                export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
              }
              else
              {
                export.Group.Update.EndDate.Date =
                  entities.ContractorHistory.EndDate;
              }
            }
          }
          else if (IsEmpty(export.Contractor.Code) && IsEmpty
            (export.JudicalDistrict.Code))
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.ContractorHistory.Assign(
              entities.ContractorHistory);
            export.Group.Update.Jd.Assign(entities.JudicalDistrict);
            export.Group.Update.Contractor.Assign(entities.Contractor);
            export.Group.Update.EffDate.Date =
              entities.ContractorHistory.EffectiveDate;

            if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
            {
              export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
            }
            else
            {
              export.Group.Update.EndDate.Date =
                entities.ContractorHistory.EndDate;
            }
          }

          if (export.Group.IsFull)
          {
            export.Plus.Text1 = "+";
            export.Standard.PageNumber = 1;

            export.Page.Index = 0;
            export.Page.CheckSize();

            export.Group.Index = 0;
            export.Group.CheckSize();

            export.Page.Update.ContractorPage.Code =
              export.Group.Item.Contractor.Code;
            export.Page.Update.JudicalDistrictPage.Code =
              export.Group.Item.Jd.Code;
            export.Page.Update.Page1.
              Assign(export.Group.Item.ContractorHistory);

            ++export.Page.Index;
            export.Page.CheckSize();

            export.Group.Index = Export.GroupGroup.Capacity - 1;
            export.Group.CheckSize();

            export.Page.Update.ContractorPage.Code =
              export.Group.Item.Contractor.Code;
            export.Page.Update.JudicalDistrictPage.Code =
              export.Group.Item.Jd.Code;
            export.Page.Update.Page1.
              Assign(export.Group.Item.ContractorHistory);

            break;
          }
          else
          {
            export.Plus.Text1 = "";
          }
        }

        if (export.Group.IsEmpty)
        {
          if (!IsEmpty(export.Contractor.Code) && !
            IsEmpty(export.JudicalDistrict.Code))
          {
            var field1 = GetField(export.Contractor, "code");

            field1.Error = true;

            var field2 = GetField(export.JudicalDistrict, "code");

            field2.Error = true;

            ExitState = "ACO_NE0000_CONTRACTOR_HISTORY_NF";
          }
          else if (!IsEmpty(export.Contractor.Code) && IsEmpty
            (export.JudicalDistrict.Code))
          {
            var field = GetField(export.Contractor, "code");

            field.Error = true;

            ExitState = "ACO_NE0000_CONTRACTOR_HISTORY_NF";
          }
          else if (IsEmpty(export.Contractor.Code) && !
            IsEmpty(export.JudicalDistrict.Code))
          {
            var field = GetField(export.JudicalDistrict, "code");

            field.Error = true;

            ExitState = "ACO_NE0000_CONTRACTOR_HISTORY_NF";
          }
          else
          {
            var field1 = GetField(export.Contractor, "code");

            field1.Error = true;

            var field2 = GetField(export.JudicalDistrict, "code");

            field2.Error = true;

            ExitState = "ACO_NE0000_CONTRACTOR_HISTORY_NF";
          }
        }

        break;
      case "LIST":
        // *******************************************
        // * Validate selection characters and insure
        // * only one prompt was selected.
        // *******************************************
        local.Common.Count = 0;

        switch(AsChar(export.PromptContractorCd.Flag))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;

            break;
          default:
            var field = GetField(export.PromptContractorCd, "flag");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(export.PromptJudicalDistrict.Flag))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;

            break;
          default:
            var field = GetField(export.PromptJudicalDistrict, "flag");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            if (AsChar(export.PromptContractorCd.Flag) == 'S')
            {
              // -- Prompt for parent organization.
              export.Contractor.Type1 = "X";
              export.ToCsor.Type1 = export.Contractor.Type1;
              ExitState = "ECO_LNK_TO_CSE_ORGANIZATION";

              return;
            }

            if (AsChar(export.PromptJudicalDistrict.Flag) == 'S')
            {
              export.JudicalDistrict.Type1 = "J";
              export.ToCsor.Type1 = export.JudicalDistrict.Type1;
              ExitState = "ECO_LNK_TO_CSE_ORGANIZATION";
            }

            break;
          default:
            if (AsChar(export.PromptJudicalDistrict.Flag) == 'S')
            {
              var field = GetField(export.PromptJudicalDistrict, "flag");

              field.Error = true;
            }

            if (AsChar(export.PromptContractorCd.Flag) == 'S')
            {
              var field = GetField(export.PromptContractorCd, "flag");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "PREV":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (IsEmpty(export.Minus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        if (!Equal(import.Contractor.Code, import.ContractorHidden.Code) || !
          Equal(import.JudicalDistrict.Code, import.JudicalDistrictHidden.Code))
        {
          var field1 = GetField(export.Contractor, "code");

          field1.Error = true;

          var field2 = GetField(export.JudicalDistrict, "code");

          field2.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        --export.Standard.PageNumber;

        export.Page.Index = export.Standard.PageNumber - 1;
        export.Page.CheckSize();

        local.Contractor.Code = export.Page.Item.ContractorPage.Code;
        local.ContractorHistory.Assign(export.Page.Item.Page1);
        local.JudicalDistrict.Code = export.Page.Item.JudicalDistrictPage.Code;
        export.Group.Index = -1;
        export.Group.Count = 0;

        foreach(var item in ReadContractorHistoryCseOrganizationCseOrganization1())
          
        {
          local.Contractor.Assign(entities.Contractor);
          local.ContractorHistory.Assign(entities.ContractorHistory);
          local.JudicalDistrict.Assign(entities.JudicalDistrict);

          if (Equal(entities.JudicalDistrict.Code, "32"))
          {
            continue;
          }

          if (AsChar(import.Active.Flag) == 'Y')
          {
            // active only records
            if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
            {
              if (!IsEmpty(import.Contractor.Code))
              {
                if (Equal(entities.Contractor.Code, import.Contractor.Code) && (
                  !IsEmpty(import.JudicalDistrict.Code) && Equal
                  (entities.JudicalDistrict.Code, import.JudicalDistrict.Code) ||
                  IsEmpty(import.JudicalDistrict.Code)))
                {
                  ++export.Group.Index;
                  export.Group.CheckSize();

                  export.Group.Update.ContractorHistory.Assign(
                    entities.ContractorHistory);
                  export.Group.Update.Contractor.Assign(entities.Contractor);
                  export.Group.Update.Jd.Assign(entities.JudicalDistrict);
                  export.Group.Update.EffDate.Date =
                    entities.ContractorHistory.EffectiveDate;
                  export.Group.Update.EndDate.Date =
                    local.NullDateWorkArea.Date;
                }
              }
              else if (!IsEmpty(import.JudicalDistrict.Code))
              {
                if (Equal(entities.JudicalDistrict.Code,
                  import.JudicalDistrict.Code) && (
                    !IsEmpty(import.Contractor.Code) && Equal
                  (entities.Contractor.Code, import.Contractor.Code) || IsEmpty
                  (import.Contractor.Code)))
                {
                  ++export.Group.Index;
                  export.Group.CheckSize();

                  export.Group.Update.ContractorHistory.Assign(
                    entities.ContractorHistory);
                  export.Group.Update.Jd.Assign(entities.JudicalDistrict);
                  export.Group.Update.Contractor.Assign(entities.Contractor);
                  export.Group.Update.EffDate.Date =
                    entities.ContractorHistory.EffectiveDate;
                  export.Group.Update.EndDate.Date =
                    local.NullDateWorkArea.Date;
                }
              }
              else if (IsEmpty(import.Contractor.Code) && IsEmpty
                (import.JudicalDistrict.Code))
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                export.Group.Update.ContractorHistory.Assign(
                  entities.ContractorHistory);
                export.Group.Update.Jd.Assign(entities.JudicalDistrict);
                export.Group.Update.Contractor.Assign(entities.Contractor);
                export.Group.Update.EffDate.Date =
                  entities.ContractorHistory.EffectiveDate;
                export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
              }
            }
            else
            {
              continue;
            }
          }
          else if (!IsEmpty(import.Contractor.Code))
          {
            if (Equal(entities.Contractor.Code, import.Contractor.Code) && (
              !IsEmpty(import.JudicalDistrict.Code) && Equal
              (entities.JudicalDistrict.Code, import.JudicalDistrict.Code) || IsEmpty
              (import.JudicalDistrict.Code)))
            {
              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ContractorHistory.Assign(
                entities.ContractorHistory);
              export.Group.Update.Contractor.Assign(entities.Contractor);
              export.Group.Update.Jd.Assign(entities.JudicalDistrict);
              export.Group.Update.EffDate.Date =
                entities.ContractorHistory.EffectiveDate;

              if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
              {
                export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
              }
              else
              {
                export.Group.Update.EndDate.Date =
                  entities.ContractorHistory.EndDate;
              }
            }
          }
          else if (!IsEmpty(import.JudicalDistrict.Code))
          {
            if (Equal(entities.JudicalDistrict.Code, import.JudicalDistrict.Code)
              && (!IsEmpty(import.Contractor.Code) && Equal
              (entities.Contractor.Code, import.Contractor.Code) || IsEmpty
              (import.Contractor.Code)))
            {
              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ContractorHistory.Assign(
                entities.ContractorHistory);
              export.Group.Update.Jd.Assign(entities.JudicalDistrict);
              export.Group.Update.Contractor.Assign(entities.Contractor);
              export.Group.Update.EffDate.Date =
                entities.ContractorHistory.EffectiveDate;

              if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
              {
                export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
              }
              else
              {
                export.Group.Update.EndDate.Date =
                  entities.ContractorHistory.EndDate;
              }
            }
          }
          else if (IsEmpty(import.Contractor.Code) && IsEmpty
            (import.JudicalDistrict.Code))
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.ContractorHistory.Assign(
              entities.ContractorHistory);
            export.Group.Update.Jd.Assign(entities.JudicalDistrict);
            export.Group.Update.Contractor.Assign(entities.Contractor);
            export.Group.Update.EffDate.Date =
              entities.ContractorHistory.EffectiveDate;

            if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
            {
              export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
            }
            else
            {
              export.Group.Update.EndDate.Date =
                entities.ContractorHistory.EndDate;
            }
          }

          if (export.Group.IsFull)
          {
            break;
          }
        }

        if (export.Standard.PageNumber > 1)
        {
          export.Minus.Text1 = "-";
        }
        else
        {
          export.Minus.Text1 = "";
        }

        export.Plus.Text1 = "+";

        break;
      case "NEXT":
        if (IsEmpty(export.Plus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        if (!Equal(import.Contractor.Code, import.ContractorHidden.Code) || !
          Equal(import.JudicalDistrict.Code, import.JudicalDistrictHidden.Code))
        {
          var field1 = GetField(export.Contractor, "code");

          field1.Error = true;

          var field2 = GetField(export.JudicalDistrict, "code");

          field2.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        ++export.Standard.PageNumber;

        export.Page.Index = export.Standard.PageNumber - 1;
        export.Page.CheckSize();

        local.Contractor.Code = export.Page.Item.ContractorPage.Code;
        local.JudicalDistrict.Code = export.Page.Item.JudicalDistrictPage.Code;
        local.ContractorHistory.Assign(export.Page.Item.Page1);
        export.Group.Index = -1;
        export.Group.Count = 0;

        foreach(var item in ReadContractorHistoryCseOrganizationCseOrganization1())
          
        {
          local.Contractor.Assign(entities.Contractor);
          local.ContractorHistory.Assign(entities.ContractorHistory);
          local.JudicalDistrict.Assign(entities.JudicalDistrict);

          if (Equal(entities.JudicalDistrict.Code, "32"))
          {
            continue;
          }

          if (AsChar(import.Active.Flag) == 'Y')
          {
            // active only records
            if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
            {
              if (!IsEmpty(import.Contractor.Code))
              {
                if (Equal(entities.Contractor.Code, import.Contractor.Code) && (
                  !IsEmpty(import.JudicalDistrict.Code) && Equal
                  (entities.JudicalDistrict.Code, import.JudicalDistrict.Code) ||
                  IsEmpty(import.JudicalDistrict.Code)))
                {
                  ++export.Group.Index;
                  export.Group.CheckSize();

                  export.Group.Update.ContractorHistory.Assign(
                    entities.ContractorHistory);
                  export.Group.Update.Contractor.Assign(entities.Contractor);
                  export.Group.Update.Jd.Assign(entities.JudicalDistrict);
                  export.Group.Update.EffDate.Date =
                    entities.ContractorHistory.EffectiveDate;
                  export.Group.Update.EndDate.Date =
                    local.NullDateWorkArea.Date;
                }
              }
              else if (!IsEmpty(import.JudicalDistrict.Code))
              {
                if (Equal(entities.JudicalDistrict.Code,
                  import.JudicalDistrict.Code) && (
                    !IsEmpty(import.Contractor.Code) && Equal
                  (entities.Contractor.Code, import.Contractor.Code) || IsEmpty
                  (import.Contractor.Code)))
                {
                  ++export.Group.Index;
                  export.Group.CheckSize();

                  export.Group.Update.ContractorHistory.Assign(
                    entities.ContractorHistory);
                  export.Group.Update.Jd.Assign(entities.JudicalDistrict);
                  export.Group.Update.Contractor.Assign(entities.Contractor);
                  export.Group.Update.EffDate.Date =
                    entities.ContractorHistory.EffectiveDate;
                  export.Group.Update.EndDate.Date =
                    local.NullDateWorkArea.Date;
                }
              }
              else if (IsEmpty(import.Contractor.Code) && IsEmpty
                (import.JudicalDistrict.Code))
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                export.Group.Update.ContractorHistory.Assign(
                  entities.ContractorHistory);
                export.Group.Update.Jd.Assign(entities.JudicalDistrict);
                export.Group.Update.Contractor.Assign(entities.Contractor);
                export.Group.Update.EffDate.Date =
                  entities.ContractorHistory.EffectiveDate;
                export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
              }
            }
            else
            {
              continue;
            }
          }
          else if (!IsEmpty(import.Contractor.Code))
          {
            if (Equal(entities.Contractor.Code, import.Contractor.Code) && (
              !IsEmpty(import.JudicalDistrict.Code) && Equal
              (entities.JudicalDistrict.Code, import.JudicalDistrict.Code) || IsEmpty
              (import.JudicalDistrict.Code)))
            {
              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ContractorHistory.Assign(
                entities.ContractorHistory);
              export.Group.Update.Contractor.Assign(entities.Contractor);
              export.Group.Update.Jd.Assign(entities.JudicalDistrict);
              export.Group.Update.EffDate.Date =
                entities.ContractorHistory.EffectiveDate;

              if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
              {
                export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
              }
              else
              {
                export.Group.Update.EndDate.Date =
                  entities.ContractorHistory.EndDate;
              }
            }
          }
          else if (!IsEmpty(import.JudicalDistrict.Code))
          {
            if (Equal(entities.JudicalDistrict.Code, import.JudicalDistrict.Code)
              && (!IsEmpty(import.Contractor.Code) && Equal
              (entities.Contractor.Code, import.Contractor.Code) || IsEmpty
              (import.Contractor.Code)))
            {
              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ContractorHistory.Assign(
                entities.ContractorHistory);
              export.Group.Update.Jd.Assign(entities.JudicalDistrict);
              export.Group.Update.Contractor.Assign(entities.Contractor);
              export.Group.Update.EffDate.Date =
                entities.ContractorHistory.EffectiveDate;

              if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
              {
                export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
              }
              else
              {
                export.Group.Update.EndDate.Date =
                  entities.ContractorHistory.EndDate;
              }
            }
          }
          else if (IsEmpty(import.Contractor.Code) && IsEmpty
            (import.JudicalDistrict.Code))
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.ContractorHistory.Assign(
              entities.ContractorHistory);
            export.Group.Update.Jd.Assign(entities.JudicalDistrict);
            export.Group.Update.Contractor.Assign(entities.Contractor);
            export.Group.Update.EffDate.Date =
              entities.ContractorHistory.EffectiveDate;

            if (Equal(entities.ContractorHistory.EndDate, local.Max.Date))
            {
              export.Group.Update.EndDate.Date = local.NullDateWorkArea.Date;
            }
            else
            {
              export.Group.Update.EndDate.Date =
                entities.ContractorHistory.EndDate;
            }
          }

          if (export.Group.IsFull)
          {
            export.Plus.Text1 = "+";

            ++export.Page.Index;
            export.Page.CheckSize();

            export.Group.Index = Export.GroupGroup.Capacity - 1;
            export.Group.CheckSize();

            export.Page.Update.ContractorPage.Code =
              export.Group.Item.Contractor.Code;
            export.Page.Update.JudicalDistrictPage.Code =
              export.Group.Item.Jd.Code;
            export.Page.Update.Page1.
              Assign(export.Group.Item.ContractorHistory);

            break;
          }
          else
          {
            export.Plus.Text1 = "";
          }
        }

        export.Minus.Text1 = "-";

        break;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private IEnumerable<bool>
    ReadContractorHistoryCseOrganizationCseOrganization1()
  {
    entities.JudicalDistrict.Populated = false;
    entities.Contractor.Populated = false;
    entities.ContractorHistory.Populated = false;

    return ReadEach("ReadContractorHistoryCseOrganizationCseOrganization1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate",
          local.ContractorHistory.EndDate.GetValueOrDefault());
        db.SetString(command, "organztnId1", local.JudicalDistrict.Code);
        db.SetString(command, "organztnId2", local.Contractor.Code);
      },
      (db, reader) =>
      {
        entities.ContractorHistory.Identifier = db.GetInt32(reader, 0);
        entities.ContractorHistory.EffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.ContractorHistory.EndDate = db.GetNullableDate(reader, 2);
        entities.ContractorHistory.FkCktCseOrgatypeCode =
          db.GetString(reader, 3);
        entities.Contractor.Type1 = db.GetString(reader, 3);
        entities.ContractorHistory.FkCktCseOrgaorganztnId =
          db.GetString(reader, 4);
        entities.Contractor.Code = db.GetString(reader, 4);
        entities.ContractorHistory.Fk0CktCseOrgatypeCode =
          db.GetString(reader, 5);
        entities.JudicalDistrict.Type1 = db.GetString(reader, 5);
        entities.ContractorHistory.Fk0CktCseOrgaorganztnId =
          db.GetString(reader, 6);
        entities.JudicalDistrict.Code = db.GetString(reader, 6);
        entities.JudicalDistrict.Name = db.GetString(reader, 7);
        entities.Contractor.Name = db.GetString(reader, 8);
        entities.JudicalDistrict.Populated = true;
        entities.Contractor.Populated = true;
        entities.ContractorHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadContractorHistoryCseOrganizationCseOrganization2()
  {
    entities.JudicalDistrict.Populated = false;
    entities.Contractor.Populated = false;
    entities.ContractorHistory.Populated = false;

    return ReadEach("ReadContractorHistoryCseOrganizationCseOrganization2",
      null,
      (db, reader) =>
      {
        entities.ContractorHistory.Identifier = db.GetInt32(reader, 0);
        entities.ContractorHistory.EffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.ContractorHistory.EndDate = db.GetNullableDate(reader, 2);
        entities.ContractorHistory.FkCktCseOrgatypeCode =
          db.GetString(reader, 3);
        entities.Contractor.Type1 = db.GetString(reader, 3);
        entities.ContractorHistory.FkCktCseOrgaorganztnId =
          db.GetString(reader, 4);
        entities.Contractor.Code = db.GetString(reader, 4);
        entities.ContractorHistory.Fk0CktCseOrgatypeCode =
          db.GetString(reader, 5);
        entities.JudicalDistrict.Type1 = db.GetString(reader, 5);
        entities.ContractorHistory.Fk0CktCseOrgaorganztnId =
          db.GetString(reader, 6);
        entities.JudicalDistrict.Code = db.GetString(reader, 6);
        entities.JudicalDistrict.Name = db.GetString(reader, 7);
        entities.Contractor.Name = db.GetString(reader, 8);
        entities.JudicalDistrict.Populated = true;
        entities.Contractor.Populated = true;
        entities.ContractorHistory.Populated = true;

        return true;
      });
  }

  private bool ReadCseOrganization1()
  {
    entities.Contractor.Populated = false;

    return Read("ReadCseOrganization1",
      (db, command) =>
      {
        db.SetString(command, "organztnId", export.Contractor.Code);
      },
      (db, reader) =>
      {
        entities.Contractor.Code = db.GetString(reader, 0);
        entities.Contractor.Type1 = db.GetString(reader, 1);
        entities.Contractor.Name = db.GetString(reader, 2);
        entities.Contractor.Populated = true;
      });
  }

  private bool ReadCseOrganization2()
  {
    entities.JudicalDistrict.Populated = false;

    return Read("ReadCseOrganization2",
      (db, command) =>
      {
        db.SetString(command, "organztnId", export.JudicalDistrict.Code);
      },
      (db, reader) =>
      {
        entities.JudicalDistrict.Code = db.GetString(reader, 0);
        entities.JudicalDistrict.Type1 = db.GetString(reader, 1);
        entities.JudicalDistrict.Name = db.GetString(reader, 2);
        entities.JudicalDistrict.Populated = true;
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
    /// <summary>A PageGroup group.</summary>
    [Serializable]
    public class PageGroup
    {
      /// <summary>
      /// A value of JudicalDistrictPage.
      /// </summary>
      [JsonPropertyName("judicalDistrictPage")]
      public CseOrganization JudicalDistrictPage
      {
        get => judicalDistrictPage ??= new();
        set => judicalDistrictPage = value;
      }

      /// <summary>
      /// A value of ContractorPage.
      /// </summary>
      [JsonPropertyName("contractorPage")]
      public CseOrganization ContractorPage
      {
        get => contractorPage ??= new();
        set => contractorPage = value;
      }

      /// <summary>
      /// A value of Page1.
      /// </summary>
      [JsonPropertyName("page1")]
      public ContractorHistory Page1
      {
        get => page1 ??= new();
        set => page1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CseOrganization judicalDistrictPage;
      private CseOrganization contractorPage;
      private ContractorHistory page1;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of EffDate.
      /// </summary>
      [JsonPropertyName("effDate")]
      public DateWorkArea EffDate
      {
        get => effDate ??= new();
        set => effDate = value;
      }

      /// <summary>
      /// A value of EndDate.
      /// </summary>
      [JsonPropertyName("endDate")]
      public DateWorkArea EndDate
      {
        get => endDate ??= new();
        set => endDate = value;
      }

      /// <summary>
      /// A value of ContractorHistory.
      /// </summary>
      [JsonPropertyName("contractorHistory")]
      public ContractorHistory ContractorHistory
      {
        get => contractorHistory ??= new();
        set => contractorHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private CseOrganization jd;
      private CseOrganization contractor;
      private DateWorkArea effDate;
      private DateWorkArea endDate;
      private ContractorHistory contractorHistory;
    }

    /// <summary>
    /// A value of FromCsor.
    /// </summary>
    [JsonPropertyName("fromCsor")]
    public CseOrganization FromCsor
    {
      get => fromCsor ??= new();
      set => fromCsor = value;
    }

    /// <summary>
    /// A value of JudicalDistrictHidden.
    /// </summary>
    [JsonPropertyName("judicalDistrictHidden")]
    public CseOrganization JudicalDistrictHidden
    {
      get => judicalDistrictHidden ??= new();
      set => judicalDistrictHidden = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public WorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// Gets a value of Page.
    /// </summary>
    [JsonIgnore]
    public Array<PageGroup> Page => page ??= new(PageGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Page for json serialization.
    /// </summary>
    [JsonPropertyName("page")]
    [Computed]
    public IList<PageGroup> Page_Json
    {
      get => page;
      set => Page.Assign(value);
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
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
    /// A value of Contractor.
    /// </summary>
    [JsonPropertyName("contractor")]
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
    }

    /// <summary>
    /// A value of ContractorHidden.
    /// </summary>
    [JsonPropertyName("contractorHidden")]
    public CseOrganization ContractorHidden
    {
      get => contractorHidden ??= new();
      set => contractorHidden = value;
    }

    /// <summary>
    /// A value of PromptContractorCd.
    /// </summary>
    [JsonPropertyName("promptContractorCd")]
    public Common PromptContractorCd
    {
      get => promptContractorCd ??= new();
      set => promptContractorCd = value;
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
    /// A value of HiddenCseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("hiddenCseOrganizationRelationship")]
    public CseOrganizationRelationship HiddenCseOrganizationRelationship
    {
      get => hiddenCseOrganizationRelationship ??= new();
      set => hiddenCseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of PromptJudicalDistrict.
    /// </summary>
    [JsonPropertyName("promptJudicalDistrict")]
    public Common PromptJudicalDistrict
    {
      get => promptJudicalDistrict ??= new();
      set => promptJudicalDistrict = value;
    }

    /// <summary>
    /// A value of Hierarchy.
    /// </summary>
    [JsonPropertyName("hierarchy")]
    public CodeValue Hierarchy
    {
      get => hierarchy ??= new();
      set => hierarchy = value;
    }

    /// <summary>
    /// A value of StartingFilter.
    /// </summary>
    [JsonPropertyName("startingFilter")]
    public CseOrganization StartingFilter
    {
      get => startingFilter ??= new();
      set => startingFilter = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private CseOrganization fromCsor;
    private CseOrganization judicalDistrictHidden;
    private WorkArea minus;
    private WorkArea plus;
    private Array<PageGroup> page;
    private Array<GroupGroup> group;
    private Common active;
    private Standard standard;
    private CseOrganization contractor;
    private CseOrganization contractorHidden;
    private Common promptContractorCd;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganizationRelationship hiddenCseOrganizationRelationship;
    private Common promptJudicalDistrict;
    private CodeValue hierarchy;
    private CseOrganization startingFilter;
    private CseOrganization judicalDistrict;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PageGroup group.</summary>
    [Serializable]
    public class PageGroup
    {
      /// <summary>
      /// A value of JudicalDistrictPage.
      /// </summary>
      [JsonPropertyName("judicalDistrictPage")]
      public CseOrganization JudicalDistrictPage
      {
        get => judicalDistrictPage ??= new();
        set => judicalDistrictPage = value;
      }

      /// <summary>
      /// A value of ContractorPage.
      /// </summary>
      [JsonPropertyName("contractorPage")]
      public CseOrganization ContractorPage
      {
        get => contractorPage ??= new();
        set => contractorPage = value;
      }

      /// <summary>
      /// A value of Page1.
      /// </summary>
      [JsonPropertyName("page1")]
      public ContractorHistory Page1
      {
        get => page1 ??= new();
        set => page1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CseOrganization judicalDistrictPage;
      private CseOrganization contractorPage;
      private ContractorHistory page1;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of EffDate.
      /// </summary>
      [JsonPropertyName("effDate")]
      public DateWorkArea EffDate
      {
        get => effDate ??= new();
        set => effDate = value;
      }

      /// <summary>
      /// A value of EndDate.
      /// </summary>
      [JsonPropertyName("endDate")]
      public DateWorkArea EndDate
      {
        get => endDate ??= new();
        set => endDate = value;
      }

      /// <summary>
      /// A value of ContractorHistory.
      /// </summary>
      [JsonPropertyName("contractorHistory")]
      public ContractorHistory ContractorHistory
      {
        get => contractorHistory ??= new();
        set => contractorHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private CseOrganization jd;
      private CseOrganization contractor;
      private DateWorkArea effDate;
      private DateWorkArea endDate;
      private ContractorHistory contractorHistory;
    }

    /// <summary>
    /// A value of ToCsor.
    /// </summary>
    [JsonPropertyName("toCsor")]
    public CseOrganization ToCsor
    {
      get => toCsor ??= new();
      set => toCsor = value;
    }

    /// <summary>
    /// A value of JudicalDistrictHidden.
    /// </summary>
    [JsonPropertyName("judicalDistrictHidden")]
    public CseOrganization JudicalDistrictHidden
    {
      get => judicalDistrictHidden ??= new();
      set => judicalDistrictHidden = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public WorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// Gets a value of Page.
    /// </summary>
    [JsonIgnore]
    public Array<PageGroup> Page => page ??= new(PageGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Page for json serialization.
    /// </summary>
    [JsonPropertyName("page")]
    [Computed]
    public IList<PageGroup> Page_Json
    {
      get => page;
      set => Page.Assign(value);
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
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
    /// A value of Contractor.
    /// </summary>
    [JsonPropertyName("contractor")]
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
    }

    /// <summary>
    /// A value of ContractorHidden.
    /// </summary>
    [JsonPropertyName("contractorHidden")]
    public CseOrganization ContractorHidden
    {
      get => contractorHidden ??= new();
      set => contractorHidden = value;
    }

    /// <summary>
    /// A value of PromptContractorCd.
    /// </summary>
    [JsonPropertyName("promptContractorCd")]
    public Common PromptContractorCd
    {
      get => promptContractorCd ??= new();
      set => promptContractorCd = value;
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
    /// A value of HiddenCseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("hiddenCseOrganizationRelationship")]
    public CseOrganizationRelationship HiddenCseOrganizationRelationship
    {
      get => hiddenCseOrganizationRelationship ??= new();
      set => hiddenCseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of PromptJudicalDistrict.
    /// </summary>
    [JsonPropertyName("promptJudicalDistrict")]
    public Common PromptJudicalDistrict
    {
      get => promptJudicalDistrict ??= new();
      set => promptJudicalDistrict = value;
    }

    /// <summary>
    /// A value of Hierarchy.
    /// </summary>
    [JsonPropertyName("hierarchy")]
    public CodeValue Hierarchy
    {
      get => hierarchy ??= new();
      set => hierarchy = value;
    }

    /// <summary>
    /// A value of StartingFilter.
    /// </summary>
    [JsonPropertyName("startingFilter")]
    public CseOrganization StartingFilter
    {
      get => startingFilter ??= new();
      set => startingFilter = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private CseOrganization toCsor;
    private CseOrganization judicalDistrictHidden;
    private WorkArea minus;
    private WorkArea plus;
    private Array<PageGroup> page;
    private Array<GroupGroup> group;
    private Common active;
    private Standard standard;
    private CseOrganization contractor;
    private CseOrganization contractorHidden;
    private Common promptContractorCd;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganizationRelationship hiddenCseOrganizationRelationship;
    private Common promptJudicalDistrict;
    private CodeValue hierarchy;
    private CseOrganization startingFilter;
    private CseOrganization judicalDistrict;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ContractorHistory.
    /// </summary>
    [JsonPropertyName("contractorHistory")]
    public ContractorHistory ContractorHistory
    {
      get => contractorHistory ??= new();
      set => contractorHistory = value;
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
    /// A value of Contractor.
    /// </summary>
    [JsonPropertyName("contractor")]
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of NullCommon.
    /// </summary>
    [JsonPropertyName("nullCommon")]
    public Common NullCommon
    {
      get => nullCommon ??= new();
      set => nullCommon = value;
    }

    /// <summary>
    /// A value of NullCseOrganization.
    /// </summary>
    [JsonPropertyName("nullCseOrganization")]
    public CseOrganization NullCseOrganization
    {
      get => nullCseOrganization ??= new();
      set => nullCseOrganization = value;
    }

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

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private ContractorHistory contractorHistory;
    private CseOrganization judicalDistrict;
    private CseOrganization contractor;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea max;
    private DateWorkArea current;
    private Common nullCommon;
    private CseOrganization nullCseOrganization;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of JudicalDistrict.
    /// </summary>
    [JsonPropertyName("judicalDistrict")]
    public CseOrganization JudicalDistrict
    {
      get => judicalDistrict ??= new();
      set => judicalDistrict = value;
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
    /// A value of ContractorHistory.
    /// </summary>
    [JsonPropertyName("contractorHistory")]
    public ContractorHistory ContractorHistory
    {
      get => contractorHistory ??= new();
      set => contractorHistory = value;
    }

    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization judicalDistrict;
    private CseOrganization contractor;
    private ContractorHistory contractorHistory;
  }
#endregion
}
