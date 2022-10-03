// Program: FN_REMP_RSRCH_COL_USING_EMPLYR, ID: 371774970, model: 746.
// Short name: SWEREMPP
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
/// A program: FN_REMP_RSRCH_COL_USING_EMPLYR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRempRsrchColUsingEmplyr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REMP_RSRCH_COL_USING_EMPLYR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRempRsrchColUsingEmplyr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRempRsrchColUsingEmplyr.
  /// </summary>
  public FnRempRsrchColUsingEmplyr(IContext context, Import import,
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
    // *******************************************************************
    // 01/03/97	R. Marchman	Add new security/next tran.
    // 02/25/98	A Samuels	PR #39262 - new PF keys
    // 11/24/98	Sunya Sharp	Add changes per screen assessment.
    // 03/20/99	Sunya Sharp	The entity type cse_person_employer was maked for 
    // deletion by IDCR 508.  Need to remove logic that uses this entity type.
    // The relationship now goes from cse person to income source with a type of
    // "E" for employment to employer.  Making changes to the display logic to
    // support this change.
    // 03/22/99	Sunya Sharp	Add logic to sort group view in alpha order by name 
    // and add a starting filter by payor name.
    // 03/27/99	Sunya Sharp	Correcting problems that are occurring when 
    // information is not contained on adabas.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }
    else if (Equal(global.Command, "FORWARD"))
    {
      ExitState = "ECO_LNK_TO_EMPLOYER";

      return;
    }

    // *** Add logic to hide the state from the employer's address.  This is 
    // need when flowing back to EMPL for successful display.  The user
    // requested so that if there was an error and the correct business location
    // was selected the user did not have to retype the information again.
    // Sunya Sharp 11/24/1998 ***
    export.Employer.Assign(import.Employer);
    export.Starting.FormattedName = import.Starting.FormattedName;
    MoveEmployerAddress(import.HiddenEmployerAddress,
      export.HiddenEmployerAddress);
    export.PromptEmployer.PromptField = import.PromptEmployer.PromptField;
    local.SelectCount.Count = 0;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.CsePersonsWorkSet.Assign(
        import.Group.Item.CsePersonsWorkSet);
      export.Group.Update.Payor.Number = import.Group.Item.Payor.Number;
      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;

      switch(AsChar(import.Group.Item.Common.SelectChar))
      {
        case 'S':
          ++local.SelectCount.Count;
          local.SelectedChar.SelectChar = export.Group.Item.Common.SelectChar;
          export.ApCsePerson.Number = import.Group.Item.Payor.Number;

          break;
        case ' ':
          break;
        default:
          ++local.SelectCount.Count;

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          break;
      }

      export.Group.Next();
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    // *** Set export hidden next tran fields.  Sunya Sharp 11/24/1998 ***
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.HiddenNextTranInfo.CsePersonNumber = export.ApCsePerson.Number;
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

    // *** Next tran to this screen is not allowed at this time.  Sunya Sharp 11
    // /24/1998 ***
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "RETEMPL"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // **** end   group C ****
    // *** Add logic when returning from EMPL.  If nothing is selected loses the
    // information that was originally there.  Sunya Sharp 11/24/1998 ***
    if (Equal(global.Command, "RETEMPL"))
    {
      if (!IsEmpty(import.FromEmplEmployer.Name))
      {
        export.Employer.Assign(import.FromEmplEmployer);
        MoveEmployerAddress(import.FromEmplEmployerAddress,
          export.HiddenEmployerAddress);
      }

      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "NEXT":
        // *** Changed exit state to be more user friendly.  Sunya Sharp 11/24/
        // 1998 ***
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "DISPLAY":
        // *** Add logic if there is no employer, return a message to tell the 
        // user to prompt to EMPL and select an employer.  Sunya Sharp 11/24/
        // 1998 ***
        // *** Changed logic to do the following.  If there is an EIN number the
        // complete the read each using this number.  If there is no EIN, then
        // do the read each with the employer identifier.  Sunya Sharp 12/3/1998
        // ***
        // *** Changed logic to no longer use cse_person_employer.  Sunya Sharp 
        // 3/20/1999 ***
        if (!IsEmpty(export.Employer.Ein))
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadCsePerson1())
          {
            local.CsePersonsWorkSet.Number = entities.ExistingPayor.Number;
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Group.Update.CsePersonsWorkSet.Assign(
                local.CsePersonsWorkSet);
              export.Group.Update.CsePersonsWorkSet.FormattedName =
                "**** ADABAS UNAVAILABLE ****";
              export.Group.Update.Payor.Number = entities.ExistingPayor.Number;
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else if (!Lt(local.CsePersonsWorkSet.FormattedName,
              export.Starting.FormattedName))
            {
              export.Group.Update.Payor.Number = entities.ExistingPayor.Number;
              export.Group.Update.CsePersonsWorkSet.Assign(
                local.CsePersonsWorkSet);
            }
            else
            {
              export.Group.Next();

              continue;
            }

            export.Group.Next();
          }

          // *** Added logic for successful display and if no information was 
          // found.  Also if the group view is full let the user know that there
          // is more information the what is viewed.  Sunya Sharp 11/24/1998 **
          // *
          // *** Added logic to sort by payor name.  This was requested by the 
          // user.  Would have liked to do an EAB but due to time constrants was
          // unable to.  The logic below uses explicit subscripting to sort the
          // group view that was ready to be passed back to the screen.  Sunya
          // Sharp 3/23/1999 ***
          if (export.Group.IsEmpty)
          {
            ExitState = "FN0000_NO_INFO_FOUND";
          }
          else
          {
            local.StartingSort.Index = -1;
            local.EndSort.Index = -1;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (IsEmpty(export.Group.Item.CsePersonsWorkSet.Number))
              {
                continue;
              }

              ++local.StartingSort.Index;
              local.StartingSort.CheckSize();

              local.StartingSort.Update.StartingCommon.SelectChar =
                export.Group.Item.Common.SelectChar;
              local.StartingSort.Update.StartingLocalPayor.Number =
                export.Group.Item.Payor.Number;
              local.StartingSort.Update.StartingCsePersonsWorkSet.Assign(
                export.Group.Item.CsePersonsWorkSet);
            }

            for(local.StartingSort.Index = 0; local.StartingSort.Index < local
              .StartingSort.Count; ++local.StartingSort.Index)
            {
              if (!local.StartingSort.CheckSize())
              {
                break;
              }

              if (local.StartingSort.Index == 0)
              {
                local.SortHold.FormattedName =
                  local.StartingSort.Item.StartingCsePersonsWorkSet.
                    FormattedName;
                local.SubscriptHold.Count = local.StartingSort.Index + 1;
              }
              else if (Lt(local.StartingSort.Item.StartingCsePersonsWorkSet.
                FormattedName, local.SortHold.FormattedName))
              {
                local.SortHold.FormattedName =
                  local.StartingSort.Item.StartingCsePersonsWorkSet.
                    FormattedName;
                local.SubscriptHold.Count = local.StartingSort.Index + 1;
              }
            }

            local.StartingSort.CheckIndex();

            local.StartingSort.Index = local.SubscriptHold.Count - 1;
            local.StartingSort.CheckSize();

            local.EndSort.Index = 0;
            local.EndSort.CheckSize();

            local.EndSort.Update.EndCommon.SelectChar =
              local.StartingSort.Item.StartingCommon.SelectChar;
            local.EndSort.Update.EndLocalPayor.Number =
              local.StartingSort.Item.StartingLocalPayor.Number;
            local.EndSort.Update.EndCsePersonsWorkSet.Assign(
              local.StartingSort.Item.StartingCsePersonsWorkSet);
            local.StartingSort.Update.StartingCsePersonsWorkSet.FormattedName =
              "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            local.SortHold.FormattedName = "";

            do
            {
              for(local.StartingSort.Index = 0; local.StartingSort.Index < local
                .StartingSort.Count; ++local.StartingSort.Index)
              {
                if (!local.StartingSort.CheckSize())
                {
                  break;
                }

                if (Equal(local.StartingSort.Item.StartingCsePersonsWorkSet.
                  FormattedName, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"))
                {
                  continue;
                }
                else if (IsEmpty(local.SortHold.FormattedName))
                {
                  local.SortHold.FormattedName =
                    local.StartingSort.Item.StartingCsePersonsWorkSet.
                      FormattedName;
                  local.SubscriptHold.Count = local.StartingSort.Index + 1;
                }
                else if (Lt(local.StartingSort.Item.StartingCsePersonsWorkSet.
                  FormattedName, local.SortHold.FormattedName))
                {
                  local.SortHold.FormattedName =
                    local.StartingSort.Item.StartingCsePersonsWorkSet.
                      FormattedName;
                  local.SubscriptHold.Count = local.StartingSort.Index + 1;
                }
              }

              local.StartingSort.CheckIndex();

              if (local.SubscriptHold.Count == 0)
              {
                local.GetOut.Flag = "Y";
              }
              else
              {
                local.StartingSort.Index = local.SubscriptHold.Count - 1;
                local.StartingSort.CheckSize();

                ++local.EndSort.Index;
                local.EndSort.CheckSize();

                local.EndSort.Update.EndCommon.SelectChar =
                  local.StartingSort.Item.StartingCommon.SelectChar;
                local.EndSort.Update.EndLocalPayor.Number =
                  local.StartingSort.Item.StartingLocalPayor.Number;
                local.EndSort.Update.EndCsePersonsWorkSet.Assign(
                  local.StartingSort.Item.StartingCsePersonsWorkSet);
                local.StartingSort.Update.StartingCsePersonsWorkSet.
                  FormattedName = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
                local.SortHold.FormattedName = "";
                local.SubscriptHold.Count = 0;
              }
            }
            while(AsChar(local.GetOut.Flag) != 'Y');

            local.EndSort.Index = -1;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              ++local.EndSort.Index;
              local.EndSort.CheckSize();

              export.Group.Update.Common.SelectChar =
                local.EndSort.Item.EndCommon.SelectChar;
              export.Group.Update.Payor.Number =
                local.EndSort.Item.EndLocalPayor.Number;
              export.Group.Update.CsePersonsWorkSet.Assign(
                local.EndSort.Item.EndCsePersonsWorkSet);
            }

            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }

          if (export.Group.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
        }
        else if (export.Employer.Identifier > 0)
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadCsePerson2())
          {
            local.CsePersonsWorkSet.Number = entities.ExistingPayor.Number;
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Group.Update.CsePersonsWorkSet.Assign(
                local.CsePersonsWorkSet);
              export.Group.Update.CsePersonsWorkSet.FormattedName =
                "**** ADABAS UNAVAILABLE ****";
              export.Group.Update.Payor.Number = entities.ExistingPayor.Number;
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else if (!Lt(local.CsePersonsWorkSet.FormattedName,
              export.Starting.FormattedName))
            {
              export.Group.Update.Payor.Number = entities.ExistingPayor.Number;
              export.Group.Update.CsePersonsWorkSet.Assign(
                local.CsePersonsWorkSet);
            }
            else
            {
              export.Group.Next();

              continue;
            }

            export.Group.Next();
          }

          // *** Added logic for successful display and if no information was 
          // found.  Also if the group view is full let the user know that there
          // is more information the what is viewed.  Sunya Sharp 11/24/1998 **
          // *
          // *** Added logic to sort by payor name.  This was requested by the 
          // user.  Would have liked to do an EAB but due to time constrants was
          // unable to.  The logic below uses explicit subscripting to sort the
          // group view that was ready to be passed back to the screen.  Sunya
          // Sharp 3/23/1999 ***
          if (export.Group.IsEmpty)
          {
            ExitState = "FN0000_NO_INFO_FOUND";
          }
          else
          {
            local.StartingSort.Index = -1;
            local.EndSort.Index = -1;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (IsEmpty(export.Group.Item.CsePersonsWorkSet.Number))
              {
                continue;
              }

              ++local.StartingSort.Index;
              local.StartingSort.CheckSize();

              local.StartingSort.Update.StartingCommon.SelectChar =
                export.Group.Item.Common.SelectChar;
              local.StartingSort.Update.StartingLocalPayor.Number =
                export.Group.Item.Payor.Number;
              local.StartingSort.Update.StartingCsePersonsWorkSet.Assign(
                export.Group.Item.CsePersonsWorkSet);
            }

            for(local.StartingSort.Index = 0; local.StartingSort.Index < local
              .StartingSort.Count; ++local.StartingSort.Index)
            {
              if (!local.StartingSort.CheckSize())
              {
                break;
              }

              if (local.StartingSort.Index == 0)
              {
                local.SortHold.FormattedName =
                  local.StartingSort.Item.StartingCsePersonsWorkSet.
                    FormattedName;
                local.SubscriptHold.Count = local.StartingSort.Index + 1;
              }
              else if (Lt(local.StartingSort.Item.StartingCsePersonsWorkSet.
                FormattedName, local.SortHold.FormattedName))
              {
                local.SortHold.FormattedName =
                  local.StartingSort.Item.StartingCsePersonsWorkSet.
                    FormattedName;
                local.SubscriptHold.Count = local.StartingSort.Index + 1;
              }
            }

            local.StartingSort.CheckIndex();

            local.StartingSort.Index = local.SubscriptHold.Count - 1;
            local.StartingSort.CheckSize();

            local.EndSort.Index = 0;
            local.EndSort.CheckSize();

            local.EndSort.Update.EndCommon.SelectChar =
              local.StartingSort.Item.StartingCommon.SelectChar;
            local.EndSort.Update.EndLocalPayor.Number =
              local.StartingSort.Item.StartingLocalPayor.Number;
            local.EndSort.Update.EndCsePersonsWorkSet.Assign(
              local.StartingSort.Item.StartingCsePersonsWorkSet);
            local.StartingSort.Update.StartingCsePersonsWorkSet.FormattedName =
              "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            local.SortHold.FormattedName = "";

            do
            {
              for(local.StartingSort.Index = 0; local.StartingSort.Index < local
                .StartingSort.Count; ++local.StartingSort.Index)
              {
                if (!local.StartingSort.CheckSize())
                {
                  break;
                }

                if (Equal(local.StartingSort.Item.StartingCsePersonsWorkSet.
                  FormattedName, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"))
                {
                  continue;
                }
                else if (IsEmpty(local.SortHold.FormattedName))
                {
                  local.SortHold.FormattedName =
                    local.StartingSort.Item.StartingCsePersonsWorkSet.
                      FormattedName;
                  local.SubscriptHold.Count = local.StartingSort.Index + 1;
                }
                else if (Lt(local.StartingSort.Item.StartingCsePersonsWorkSet.
                  FormattedName, local.SortHold.FormattedName))
                {
                  local.SortHold.FormattedName =
                    local.StartingSort.Item.StartingCsePersonsWorkSet.
                      FormattedName;
                  local.SubscriptHold.Count = local.StartingSort.Index + 1;
                }
              }

              local.StartingSort.CheckIndex();

              if (local.SubscriptHold.Count == 0)
              {
                local.GetOut.Flag = "Y";
              }
              else
              {
                local.StartingSort.Index = local.SubscriptHold.Count - 1;
                local.StartingSort.CheckSize();

                ++local.EndSort.Index;
                local.EndSort.CheckSize();

                local.EndSort.Update.EndCommon.SelectChar =
                  local.StartingSort.Item.StartingCommon.SelectChar;
                local.EndSort.Update.EndLocalPayor.Number =
                  local.StartingSort.Item.StartingLocalPayor.Number;
                local.EndSort.Update.EndCsePersonsWorkSet.Assign(
                  local.StartingSort.Item.StartingCsePersonsWorkSet);
                local.StartingSort.Update.StartingCsePersonsWorkSet.
                  FormattedName = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
                local.SortHold.FormattedName = "";
                local.SubscriptHold.Count = 0;
              }
            }
            while(AsChar(local.GetOut.Flag) != 'Y');

            local.EndSort.Index = -1;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              ++local.EndSort.Index;
              local.EndSort.CheckSize();

              export.Group.Update.Common.SelectChar =
                local.EndSort.Item.EndCommon.SelectChar;
              export.Group.Update.Payor.Number =
                local.EndSort.Item.EndLocalPayor.Number;
              export.Group.Update.CsePersonsWorkSet.Assign(
                local.EndSort.Item.EndCsePersonsWorkSet);
            }

            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }

          if (export.Group.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
        }
        else
        {
          ExitState = "FN0000_SELECT_EMPLOYER_FROM_EMPL";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        // *** Add logic to make selection character error when multiple rows 
        // are selected.  Sunya Sharp 11/24/1998 ***
        export.RempSelection.Flag = "";

        switch(local.SelectCount.Count)
        {
          case 0:
            break;
          case 1:
            if (AsChar(local.SelectedChar.SelectChar) == 'S')
            {
              export.RempSelection.Flag = "N";
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
            }

            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!IsEmpty(export.Group.Item.Common.SelectChar))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST":
        // *** Add logic to reset prompt field when flowing to EMPL.  Add logic 
        // for when no "S" or an invalid code is placed in prompt field.  Sunya
        // Sharp 11/24/1998 ***
        if (AsChar(export.PromptEmployer.PromptField) == 'S')
        {
          export.PromptEmployer.PromptField = "";
          ExitState = "ECO_LNK_TO_EMPLOYER";
        }
        else if (IsEmpty(export.PromptEmployer.PromptField))
        {
          var field = GetField(export.PromptEmployer, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
        else
        {
          var field = GetField(export.PromptEmployer, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveEmployerAddress(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.State = source.State;
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

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePerson.Number = import.Group.Item.Payor.Number;
    useImport.CsePersonsWorkSet.Number =
      import.Group.Item.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCsePerson1()
  {
    return ReadEach("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", export.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingPayor.Number = db.GetString(reader, 0);
        entities.ExistingPayor.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "empId", export.Employer.Identifier);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingPayor.Number = db.GetString(reader, 0);
        entities.ExistingPayor.Populated = true;

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Payor.
      /// </summary>
      [JsonPropertyName("payor")]
      public CsePerson Payor
      {
        get => payor ??= new();
        set => payor = value;
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
      public const int Capacity = 150;

      private Common common;
      private CsePerson payor;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CsePersonsWorkSet Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of FromEmplEmployerAddress.
    /// </summary>
    [JsonPropertyName("fromEmplEmployerAddress")]
    public EmployerAddress FromEmplEmployerAddress
    {
      get => fromEmplEmployerAddress ??= new();
      set => fromEmplEmployerAddress = value;
    }

    /// <summary>
    /// A value of FromEmplEmployer.
    /// </summary>
    [JsonPropertyName("fromEmplEmployer")]
    public Employer FromEmplEmployer
    {
      get => fromEmplEmployer ??= new();
      set => fromEmplEmployer = value;
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
    /// A value of HiddenEmployerAddress.
    /// </summary>
    [JsonPropertyName("hiddenEmployerAddress")]
    public EmployerAddress HiddenEmployerAddress
    {
      get => hiddenEmployerAddress ??= new();
      set => hiddenEmployerAddress = value;
    }

    /// <summary>
    /// A value of PromptEmployer.
    /// </summary>
    [JsonPropertyName("promptEmployer")]
    public Standard PromptEmployer
    {
      get => promptEmployer ??= new();
      set => promptEmployer = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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

    private CsePersonsWorkSet starting;
    private EmployerAddress fromEmplEmployerAddress;
    private Employer fromEmplEmployer;
    private Employer employer;
    private EmployerAddress hiddenEmployerAddress;
    private Standard promptEmployer;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Payor.
      /// </summary>
      [JsonPropertyName("payor")]
      public CsePerson Payor
      {
        get => payor ??= new();
        set => payor = value;
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
      public const int Capacity = 150;

      private Common common;
      private CsePerson payor;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CsePersonsWorkSet Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of HiddenEmployerAddress.
    /// </summary>
    [JsonPropertyName("hiddenEmployerAddress")]
    public EmployerAddress HiddenEmployerAddress
    {
      get => hiddenEmployerAddress ??= new();
      set => hiddenEmployerAddress = value;
    }

    /// <summary>
    /// A value of PromptEmployer.
    /// </summary>
    [JsonPropertyName("promptEmployer")]
    public Standard PromptEmployer
    {
      get => promptEmployer ??= new();
      set => promptEmployer = value;
    }

    /// <summary>
    /// A value of RempSelection.
    /// </summary>
    [JsonPropertyName("rempSelection")]
    public Common RempSelection
    {
      get => rempSelection ??= new();
      set => rempSelection = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet starting;
    private EmployerAddress hiddenEmployerAddress;
    private Standard promptEmployer;
    private Common rempSelection;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePerson apCsePerson;
    private Employer employer;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A EndSortGroup group.</summary>
    [Serializable]
    public class EndSortGroup
    {
      /// <summary>
      /// A value of EndCommon.
      /// </summary>
      [JsonPropertyName("endCommon")]
      public Common EndCommon
      {
        get => endCommon ??= new();
        set => endCommon = value;
      }

      /// <summary>
      /// A value of EndLocalPayor.
      /// </summary>
      [JsonPropertyName("endLocalPayor")]
      public CsePerson EndLocalPayor
      {
        get => endLocalPayor ??= new();
        set => endLocalPayor = value;
      }

      /// <summary>
      /// A value of EndCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("endCsePersonsWorkSet")]
      public CsePersonsWorkSet EndCsePersonsWorkSet
      {
        get => endCsePersonsWorkSet ??= new();
        set => endCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common endCommon;
      private CsePerson endLocalPayor;
      private CsePersonsWorkSet endCsePersonsWorkSet;
    }

    /// <summary>A StartingSortGroup group.</summary>
    [Serializable]
    public class StartingSortGroup
    {
      /// <summary>
      /// A value of StartingCommon.
      /// </summary>
      [JsonPropertyName("startingCommon")]
      public Common StartingCommon
      {
        get => startingCommon ??= new();
        set => startingCommon = value;
      }

      /// <summary>
      /// A value of StartingLocalPayor.
      /// </summary>
      [JsonPropertyName("startingLocalPayor")]
      public CsePerson StartingLocalPayor
      {
        get => startingLocalPayor ??= new();
        set => startingLocalPayor = value;
      }

      /// <summary>
      /// A value of StartingCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("startingCsePersonsWorkSet")]
      public CsePersonsWorkSet StartingCsePersonsWorkSet
      {
        get => startingCsePersonsWorkSet ??= new();
        set => startingCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common startingCommon;
      private CsePerson startingLocalPayor;
      private CsePersonsWorkSet startingCsePersonsWorkSet;
    }

    /// <summary>
    /// A value of GetOut.
    /// </summary>
    [JsonPropertyName("getOut")]
    public Common GetOut
    {
      get => getOut ??= new();
      set => getOut = value;
    }

    /// <summary>
    /// A value of EndHold.
    /// </summary>
    [JsonPropertyName("endHold")]
    public CsePersonsWorkSet EndHold
    {
      get => endHold ??= new();
      set => endHold = value;
    }

    /// <summary>
    /// A value of SubscriptHold.
    /// </summary>
    [JsonPropertyName("subscriptHold")]
    public Common SubscriptHold
    {
      get => subscriptHold ??= new();
      set => subscriptHold = value;
    }

    /// <summary>
    /// Gets a value of EndSort.
    /// </summary>
    [JsonIgnore]
    public Array<EndSortGroup> EndSort => endSort ??= new(
      EndSortGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of EndSort for json serialization.
    /// </summary>
    [JsonPropertyName("endSort")]
    [Computed]
    public IList<EndSortGroup> EndSort_Json
    {
      get => endSort;
      set => EndSort.Assign(value);
    }

    /// <summary>
    /// A value of SortHold.
    /// </summary>
    [JsonPropertyName("sortHold")]
    public CsePersonsWorkSet SortHold
    {
      get => sortHold ??= new();
      set => sortHold = value;
    }

    /// <summary>
    /// Gets a value of StartingSort.
    /// </summary>
    [JsonIgnore]
    public Array<StartingSortGroup> StartingSort => startingSort ??= new(
      StartingSortGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of StartingSort for json serialization.
    /// </summary>
    [JsonPropertyName("startingSort")]
    [Computed]
    public IList<StartingSortGroup> StartingSort_Json
    {
      get => startingSort;
      set => StartingSort.Assign(value);
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
    /// A value of SelectedChar.
    /// </summary>
    [JsonPropertyName("selectedChar")]
    public Common SelectedChar
    {
      get => selectedChar ??= new();
      set => selectedChar = value;
    }

    /// <summary>
    /// A value of SelectCount.
    /// </summary>
    [JsonPropertyName("selectCount")]
    public Common SelectCount
    {
      get => selectCount ??= new();
      set => selectCount = value;
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

    private Common getOut;
    private CsePersonsWorkSet endHold;
    private Common subscriptHold;
    private Array<EndSortGroup> endSort;
    private CsePersonsWorkSet sortHold;
    private Array<StartingSortGroup> startingSort;
    private DateWorkArea current;
    private Common selectedChar;
    private Common selectCount;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Employer Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ExistingPayor.
    /// </summary>
    [JsonPropertyName("existingPayor")]
    public CsePerson ExistingPayor
    {
      get => existingPayor ??= new();
      set => existingPayor = value;
    }

    private IncomeSource incomeSource;
    private Employer existing;
    private CsePerson existingPayor;
  }
#endregion
}
