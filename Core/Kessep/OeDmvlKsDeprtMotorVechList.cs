// Program: OE_DMVL_KS_DEPRT_MOTOR_VECH_LIST, ID: 371369233, model: 746.
// Short name: SWEDMVLP
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
/// A program: OE_DMVL_KS_DEPRT_MOTOR_VECH_LIST.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This proc step action block lists the Obligor
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeDmvlKsDeprtMotorVechList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DMVL_KS_DEPRT_MOTOR_VECH_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDmvlKsDeprtMotorVechList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDmvlKsDeprtMotorVechList.
  /// </summary>
  public OeDmvlKsDeprtMotorVechList(IContext context, Import import,
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
    // Date			Developer	Request #
    // 11-03-2008		DDupree
    // Initial development
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      // transfer to ddmm screen (FN_DDMM_DEBT_DSTRBTN_MNGMNT_MENU)
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      // -------------
      // begin group F
      // -------------
      UseScCabSignoff();

      return;

      // -------------
      // end   group F
      // -------------
    }

    if (Equal(global.Command, "DONE"))
    {
      if (Equal(import.CsePersonsWorkSet.Number, import.Flow.Number) && !
        IsEmpty(import.CsePersonsWorkSet.Number))
      {
        export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
      }
      else if (!IsEmpty(import.Flow.Number))
      {
        export.CsePersonsWorkSet.Number = import.Flow.Number;
      }
      else
      {
        return;
      }

      global.Command = "DISPLAY";
    }
    else
    {
      export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    }

    export.MoreLessScroll.Text3 = import.MoreLessScroll.Text3;
    export.PageCount.Count = import.PageCount.Count;
    export.NumberOfCourtOrders.Count = import.NumberOfCourtOrders.Count;
    MoveCsePersonsWorkSet(import.HiddenPrev, export.HiddenPrev);
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.PromptCsePersonNumber.SelectChar =
      import.PromptCsePersonNumber.SelectChar;
    local.BlankStartDate.Date = new DateTime(1, 1, 1);
    local.Current.Date = Now().Date;

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      // this will be a group view based on the new table  - ks driver license
      if (!import.Group.IsEmpty)
      {
        export.Group.Index = -1;
        export.Group.Count = 0;

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (!import.Group.CheckSize())
          {
            break;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
          export.Group.Update.KsDriversLicense.Assign(
            import.Group.Item.KsDriversLicense);
          export.Group.Update.LegalAction.StandardNumber =
            import.Group.Item.LegalAction.StandardNumber;
          export.Group.Update.Name.Text23 = import.Group.Item.Name.Text23;
          export.Group.Update.Office.Text4 = import.Group.Item.Office.Text4;
          export.Group.Update.SelectOption.SelectChar =
            import.Group.Item.SelectOption.SelectChar;
        }

        import.Group.CheckIndex();
      }

      if (!import.Paging.IsEmpty)
      {
        export.Paging.Index = -1;
        export.Paging.Count = 0;

        for(import.Paging.Index = 0; import.Paging.Index < import.Paging.Count; ++
          import.Paging.Index)
        {
          if (!import.Paging.CheckSize())
          {
            break;
          }

          ++export.Paging.Index;
          export.Paging.CheckSize();

          export.Paging.Update.PageCase.Number =
            import.Paging.Item.PageCase.Number;
          MoveKsDriversLicense(import.Paging.Item.PageKsDriversLicense,
            export.Paging.Update.PageKsDriversLicense);
          export.Paging.Update.PageLegalAction.StandardNumber =
            import.Paging.Item.PageLegalAction.StandardNumber;
        }

        import.Paging.CheckIndex();
      }
    }
    else
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        export.Group.Update.Case1.Number = local.ClearCase.Number;
        export.Group.Update.KsDriversLicense.
          Assign(local.ClearKsDriversLicense);
        export.Group.Update.Name.Text23 = local.ClearName.Text23;
        export.Group.Update.Office.Text4 = local.ClearOffice.Text4;
        export.Group.Update.LegalAction.StandardNumber =
          local.ClearLegalAction.StandardNumber;
      }

      export.Group.CheckIndex();
      export.Group.Index = -1;
      export.Group.Count = 0;

      for(export.Paging.Index = 0; export.Paging.Index < export.Paging.Count; ++
        export.Paging.Index)
      {
        if (!export.Paging.CheckSize())
        {
          break;
        }

        export.Paging.Update.PageCase.Number = local.ClearCase.Number;
        MoveKsDriversLicense(local.ClearKsDriversLicense,
          export.Paging.Update.PageKsDriversLicense);
        export.Paging.Update.PageLegalAction.StandardNumber =
          local.ClearLegalAction.StandardNumber;
      }

      export.Paging.CheckIndex();
      export.Paging.Index = -1;
      export.Paging.Count = 0;
      export.PageCount.Count = 0;
      export.NumberOfCourtOrders.Count = 0;
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ----------------------------------------------------------
    // The following statements must be placed after MOVE imports
    // to exports
    // ----------------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------------------
      export.CsePersonsWorkSet.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
        (10);

      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
      }

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // -------------------------------------------------------------
      // Set up local next_tran_info for saving the current values for
      // the next screen
      // -------------------------------------------------------------
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        // either the cse person number needs to be entered or when the program 
        // is flowed
        // to from a another program then it needs to bring the cse person 
        // number with it
        // prompt for the cse person number
        ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        return;
      }
      else
      {
        UseSiReadCsePerson();
      }

      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
        ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        return;
      }

      local.ReadKsDriversLicense.CourtesyLetterSentDate = Now().Date;
      local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate = Now().Date;
      local.ReadLegalAction.StandardNumber = "";
      local.ReadCase.Number = "";

      ++export.Paging.Index;
      export.Paging.CheckSize();

      export.Paging.Update.PageCase.Number = local.ReadCase.Number;
      export.Paging.Update.PageLegalAction.StandardNumber =
        local.ReadLegalAction.StandardNumber ?? "";
      export.Paging.Update.PageKsDriversLicense.CourtesyLetterSentDate =
        local.ReadKsDriversLicense.CourtesyLetterSentDate;
      export.Paging.Update.PageKsDriversLicense.
        Attribute30DayLetterCreatedDate =
          local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate;
      MoveCsePersonsWorkSet(local.Initialised, export.HiddenPrev);
      export.MoreLessScroll.Text3 = "";

      if (ReadKsDriversLicense())
      {
        // ok proceed
      }
      else
      {
        if (ReadCsePerson())
        {
          ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";
        }
        else
        {
          ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";
        }

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        return;
      }
    }

    if (!Equal(global.Command, "LIST"))
    {
      export.PromptCsePersonNumber.SelectChar = "";
    }

    switch(TrimEnd(global.Command))
    {
      case "KDMV":
        if (!Equal(export.CsePersonsWorkSet.Number, export.HiddenPrev.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        local.CountExmp.Count = 0;

        // MAKE SURE WE HAVE THE CORRECT SELECT CHARACTER
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.SelectOption.SelectChar) != 'S' && !
            IsEmpty(export.Group.Item.SelectOption.SelectChar))
          {
            var field = GetField(export.Group.Item.SelectOption, "selectChar");

            field.Error = true;

            ++local.CountExmp.Count;
          }

          if (local.CountExmp.Count > 0)
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
          }
        }

        export.Group.CheckIndex();

        // MAKE SURE WE ONLY HAVE ONE SELCTION, NO MULTI SELECTION
        local.CountExmp.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.SelectOption.SelectChar) == 'S')
          {
            var field = GetField(export.Group.Item.SelectOption, "selectChar");

            field.Error = true;

            ++local.CountExmp.Count;
            export.KsDriversLicense.SequenceCounter =
              export.Group.Item.KsDriversLicense.SequenceCounter;

            // set export legal aciton id to grp legal action id
            // set export ks driver's license create timestamp to grp driver's 
            // license create timestamp
            // these will flow to the kdmv screen with cse person number.
          }
        }

        export.Group.CheckIndex();

        if (local.CountExmp.Count > 1)
        {
          ExitState = "SELECT_ONLY_ONE_RECORD";

          return;
        }

        if (export.Group.Count > 1)
        {
          if (import.NumberOfCourtOrders.Count > 1)
          {
            // there is more than one court order so one has to be selected 
            // before the program will flow to kdmv
            if (local.CountExmp.Count <= 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (!export.Group.CheckSize())
                {
                  break;
                }

                var field =
                  GetField(export.Group.Item.SelectOption, "selectChar");

                field.Error = true;
              }

              export.Group.CheckIndex();
              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }
          }
          else if (local.CountExmp.Count <= 0)
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              export.KsDriversLicense.SequenceCounter =
                export.Group.Item.KsDriversLicense.SequenceCounter;

              goto Test1;
            }

            export.Group.CheckIndex();
          }
        }
        else if (export.Group.Count == 1)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            export.KsDriversLicense.SequenceCounter =
              export.Group.Item.KsDriversLicense.SequenceCounter;

            goto Test1;
          }

          export.Group.CheckIndex();
        }

Test1:

        ExitState = "ECO_LNK_KDMV_MOTOR_VECHIL_DETAIL";

        // THIS IS A  NEW EXITSTATE TO FLOW TO KDMV SCREEN
        break;
      case "LIST":
        // the only prompt will be for the ap person number - goes to the NAME 
        // screen.
        if (IsEmpty(export.PromptCsePersonNumber.SelectChar))
        {
          var field = GetField(export.PromptCsePersonNumber, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          return;
        }

        if (!IsEmpty(export.PromptCsePersonNumber.SelectChar) && AsChar
          (export.PromptCsePersonNumber.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptCsePersonNumber, "selectChar");

          field.Error = true;

          break;
        }

        // prompt for ap person number - send to NAME screen
        if (AsChar(export.PromptCsePersonNumber.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_SELECT_PERSON";

          return;
        }

        break;
      case "DISPLAY":
        MoveCsePersonsWorkSet(export.CsePersonsWorkSet, local.Saved);

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          // this is an error, must have a ap person number
          // this should have errored out before but for some reason if it gets 
          // to this point and
          // there is no cse person number then we will error it now
          return;
        }
        else
        {
          // read each ks driv er's license
          //                  legal action
          //   sorted by desending courtesy letter sent date
          //   sorted by desending 30 day notice create date
          //   sorted by ascending legal action standard number
          //         Where cse person number = import cse person
          //             and courtesy lettter sent date > null date
          //             and courtesy letter date => local couresty letter date
          //             and 30 day notice create date => local 30 day notice 
          // create date
          //             and ks driver's license had a legal action
          //             and legal action standard number =< local legal action
          // If there is no legal action id on the table, this means that is a 
          // shell record and
          // there have not been any other records created or they are historic 
          // records. so
          // we would only do the following reads if there is a legal action id
          foreach(var item in ReadKsDriversLicenseLegalAction())
          {
            // now we need to get the case number
            foreach(var item1 in ReadCase())
            {
              ++export.Group.Index;
              export.Group.CheckSize();

              if (!Equal(entities.LegalAction.StandardNumber,
                local.ReadLegalAction.StandardNumber))
              {
                ++export.NumberOfCourtOrders.Count;
              }

              if (!Equal(entities.LegalAction.StandardNumber,
                local.ReadLegalAction.StandardNumber) || Equal
                (entities.LegalAction.StandardNumber,
                local.ReadLegalAction.StandardNumber) && !
                Equal(entities.KsDriversLicense.CourtesyLetterSentDate,
                local.ReadKsDriversLicense.CourtesyLetterSentDate) || Equal
                (entities.LegalAction.StandardNumber,
                local.ReadLegalAction.StandardNumber) && Equal
                (entities.KsDriversLicense.CourtesyLetterSentDate,
                local.ReadKsDriversLicense.CourtesyLetterSentDate) && !
                Equal(entities.KsDriversLicense.Attribute30DayLetterCreatedDate,
                local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate) || Equal
                (entities.LegalAction.StandardNumber,
                local.ReadLegalAction.StandardNumber) && Equal
                (entities.KsDriversLicense.CourtesyLetterSentDate,
                local.ReadKsDriversLicense.CourtesyLetterSentDate) && Equal
                (entities.KsDriversLicense.Attribute30DayLetterCreatedDate,
                local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate) && !
                Equal(entities.Case1.Number, local.ReadCase.Number))
              {
                var field =
                  GetField(export.Group.Item.SelectOption, "selectChar");

                field.Highlighting = Highlighting.Underscore;
                field.Protected = false;
              }
              else
              {
                var field =
                  GetField(export.Group.Item.SelectOption, "selectChar");

                field.Protected = true;
              }

              // now we need to get the case worker id or the office
              local.ReadCase.Number = entities.Case1.Number;
              UseSpCabDetOspAssgndToCsecase();
              local.ReadKsDriversLicense.CourtesyLetterSentDate =
                entities.KsDriversLicense.CourtesyLetterSentDate;
              local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate =
                entities.KsDriversLicense.Attribute30DayLetterCreatedDate;
              local.ReadLegalAction.StandardNumber =
                entities.LegalAction.StandardNumber;
              local.Name.Text13 =
                Substring(local.ServiceProvider.LastName, 1, 13);
              local.Name.Text8 =
                Substring(local.ServiceProvider.FirstName, 1, 8);
              export.Group.Update.Name.Text23 = TrimEnd(local.Name.Text13) + ", " +
                local.Name.Text8;
              export.Group.Update.Office.Text4 =
                Substring(local.Office2.Name, 1, 4);
              export.Group.Update.Case1.Number = entities.Case1.Number;

              if (Equal(local.PreviousLegalAction.StandardNumber,
                local.ReadLegalAction.StandardNumber) && Equal
                (local.PreviousKsDriversLicense.CourtesyLetterSentDate,
                local.ReadKsDriversLicense.CourtesyLetterSentDate) && Equal
                (local.PreviousKsDriversLicense.Attribute30DayLetterCreatedDate,
                local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate))
              {
                export.Group.Update.LegalAction.StandardNumber = "";
              }
              else
              {
                export.Group.Update.LegalAction.StandardNumber =
                  entities.LegalAction.StandardNumber;
              }

              export.Group.Update.KsDriversLicense.CourtesyLetterSentDate =
                entities.KsDriversLicense.CourtesyLetterSentDate;
              export.Group.Update.KsDriversLicense.
                Attribute30DayLetterCreatedDate =
                  entities.KsDriversLicense.Attribute30DayLetterCreatedDate;
              export.Group.Update.KsDriversLicense.SequenceCounter =
                entities.KsDriversLicense.SequenceCounter;

              if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
              {
                export.MoreLessScroll.Text3 = "+";
                ++export.PageCount.Count;

                ++export.Paging.Index;
                export.Paging.CheckSize();

                export.Paging.Update.PageCase.Number = entities.Case1.Number;
                export.Paging.Update.PageLegalAction.StandardNumber =
                  entities.LegalAction.StandardNumber;
                export.Paging.Update.PageKsDriversLicense.
                  CourtesyLetterSentDate =
                    entities.KsDriversLicense.CourtesyLetterSentDate;
                export.Paging.Update.PageKsDriversLicense.
                  Attribute30DayLetterCreatedDate =
                    entities.KsDriversLicense.Attribute30DayLetterCreatedDate;

                goto ReadEach1;
              }

              local.Case1.Number = entities.Case1.Number;
              MoveKsDriversLicense(local.ReadKsDriversLicense,
                local.PreviousKsDriversLicense);
              local.PreviousLegalAction.StandardNumber =
                entities.LegalAction.StandardNumber;
            }

            local.ReadCase.Number = "";
          }

ReadEach1:
          ;
        }

        if (export.Group.IsEmpty)
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";
        }
        else
        {
          MoveCsePersonsWorkSet(export.CsePersonsWorkSet, export.HiddenPrev);
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "RETURN":
        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
        }
        else
        {
          export.FlowToKdmv.Number = export.HiddenPrev.Number;

          if (!Equal(export.CsePersonsWorkSet.Number, export.HiddenPrev.Number))
          {
            // since there are more than one court orders for the last 
            // successfully displayed ap,
            // then the worker will have to redisplay the new entered ap number 
            // before they
            // are allowed to return the the screen they came from
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            export.HiddenDisplayPerformed.Flag = "N";
            ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

            return;
          }

          local.CountExmp.Count = 0;

          // MAKE SURE WE HAVE THE CORRECT SELECT CHARACTER
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.SelectOption.SelectChar) != 'S' && !
              IsEmpty(export.Group.Item.SelectOption.SelectChar))
            {
              var field =
                GetField(export.Group.Item.SelectOption, "selectChar");

              field.Error = true;

              ++local.CountExmp.Count;
            }

            if (local.CountExmp.Count > 0)
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
            }
          }

          export.Group.CheckIndex();

          // MAKE SURE WE ONLY HAVE ONE SELCTION, NO MULTI SELECTION
          local.CountExmp.Count = 0;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.SelectOption.SelectChar) == 'S')
            {
              var field =
                GetField(export.Group.Item.SelectOption, "selectChar");

              field.Error = true;

              ++local.CountExmp.Count;
              export.Flow.SequenceCounter =
                export.Group.Item.KsDriversLicense.SequenceCounter;

              // set export legal aciton id to grp legal action id
              // set export ks driver's license create timestamp to grp driver's
              // license create timestamp
              // these will flow to the kdmv screen with cse person number.
            }
          }

          export.Group.CheckIndex();

          if (local.CountExmp.Count > 1)
          {
            ExitState = "SELECT_ONLY_ONE_RECORD";

            return;
          }

          if (export.Group.Count > 1)
          {
            if (import.NumberOfCourtOrders.Count > 1)
            {
              // there is more than one court order so one has to be selected 
              // before the program will flow to kdmv
              if (local.CountExmp.Count <= 0)
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (!export.Group.CheckSize())
                  {
                    break;
                  }

                  var field =
                    GetField(export.Group.Item.SelectOption, "selectChar");

                  field.Error = true;
                }

                export.Group.CheckIndex();
                ExitState = "ACO_NE0000_NO_SELECTION_MADE";

                return;
              }
            }
            else if (local.CountExmp.Count <= 0)
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (!export.Group.CheckSize())
                {
                  break;
                }

                export.Flow.SequenceCounter =
                  export.Group.Item.KsDriversLicense.SequenceCounter;

                goto Test2;
              }

              export.Group.CheckIndex();
            }
          }
          else if (export.Group.Count == 1)
          {
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              export.Flow.SequenceCounter =
                export.Group.Item.KsDriversLicense.SequenceCounter;

              goto Test2;
            }

            export.Group.CheckIndex();
          }
        }

Test2:

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PREV":
        if (!Equal(export.CsePersonsWorkSet.Number, export.HiddenPrev.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (import.Paging.Count <= 1)
        {
          ExitState = "THERE_NO_PREVIOUS_RECORD_DISPLAY";

          return;
        }
        else
        {
          local.ReadKsDriversLicense.CourtesyLetterSentDate = Now().Date;
          local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate =
            Now().Date;
          local.ReadLegalAction.StandardNumber = "";
          local.ReadCase.Number = "";
          local.ReadPreviousCase.Number = local.ReadCase.Number;
          MoveKsDriversLicense(local.ReadKsDriversLicense,
            local.ReadPreviousKsDriversLicense);
          local.ReadPreviousLegalAction.StandardNumber =
            local.ReadLegalAction.StandardNumber;

          if (export.PageCount.Count == 1)
          {
            ExitState = "THERE_NO_PREVIOUS_RECORD_DISPLAY";

            return;
          }

          --export.PageCount.Count;

          for(import.Paging.Index = 0; import.Paging.Index < import
            .Paging.Count; ++import.Paging.Index)
          {
            if (!import.Paging.CheckSize())
            {
              break;
            }

            if (export.PageCount.Count == import.Paging.Index + 1)
            {
              local.ReadCase.Number = import.Paging.Item.PageCase.Number;
              MoveKsDriversLicense(import.Paging.Item.PageKsDriversLicense,
                local.ReadKsDriversLicense);
              local.ReadLegalAction.StandardNumber =
                import.Paging.Item.PageLegalAction.StandardNumber;

              break;
            }
          }

          import.Paging.CheckIndex();
        }

        if (export.PageCount.Count == 1)
        {
          export.MoreLessScroll.Text3 = "+";
        }
        else if (export.PageCount.Count < import.Paging.Count && export
          .PageCount.Count > 1)
        {
          export.MoreLessScroll.Text3 = "+ -";
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          export.Group.Update.Case1.Number = local.ClearCase.Number;
          export.Group.Update.KsDriversLicense.Assign(
            local.ClearKsDriversLicense);
          export.Group.Update.Name.Text23 = local.ClearName.Text23;
          export.Group.Update.Office.Text4 = local.ClearOffice.Text4;
          export.Group.Update.LegalAction.StandardNumber =
            local.ClearLegalAction.StandardNumber;
        }

        export.Group.CheckIndex();
        export.Group.Index = -1;
        export.Group.Count = 0;

        foreach(var item in ReadKsDriversLicenseLegalAction())
        {
          // now we need to get the case number
          foreach(var item1 in ReadCase())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (!Equal(entities.LegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber) || Equal
              (entities.LegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber) && !
              Equal(entities.KsDriversLicense.CourtesyLetterSentDate,
              local.ReadKsDriversLicense.CourtesyLetterSentDate) || Equal
              (entities.LegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber) && Equal
              (entities.KsDriversLicense.CourtesyLetterSentDate,
              local.ReadKsDriversLicense.CourtesyLetterSentDate) && !
              Equal(entities.KsDriversLicense.Attribute30DayLetterCreatedDate,
              local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate) || Equal
              (entities.LegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber) && Equal
              (entities.KsDriversLicense.CourtesyLetterSentDate,
              local.ReadKsDriversLicense.CourtesyLetterSentDate) && Equal
              (entities.KsDriversLicense.Attribute30DayLetterCreatedDate,
              local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate) && !
              Equal(entities.Case1.Number, local.ReadCase.Number))
            {
              var field =
                GetField(export.Group.Item.SelectOption, "selectChar");

              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
            }
            else
            {
              var field =
                GetField(export.Group.Item.SelectOption, "selectChar");

              field.Protected = true;
            }

            // now we need to get the case worker id or the office
            local.ReadCase.Number = entities.Case1.Number;
            UseSpCabDetOspAssgndToCsecase();
            local.ReadKsDriversLicense.CourtesyLetterSentDate =
              entities.KsDriversLicense.CourtesyLetterSentDate;
            local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate =
              entities.KsDriversLicense.Attribute30DayLetterCreatedDate;
            local.ReadLegalAction.StandardNumber =
              entities.LegalAction.StandardNumber;
            local.Name.Text13 =
              Substring(local.ServiceProvider.LastName, 1, 13);
            local.Name.Text8 = Substring(local.ServiceProvider.FirstName, 1, 8);
            export.Group.Update.Name.Text23 = TrimEnd(local.Name.Text13) + ", " +
              local.Name.Text8;
            export.Group.Update.Office.Text4 =
              Substring(local.Office2.Name, 1, 4);
            export.Group.Update.Case1.Number = entities.Case1.Number;

            if (Equal(local.PreviousLegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber) && Equal
              (local.PreviousKsDriversLicense.CourtesyLetterSentDate,
              local.ReadKsDriversLicense.CourtesyLetterSentDate) && Equal
              (local.PreviousKsDriversLicense.Attribute30DayLetterCreatedDate,
              local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate))
            {
              export.Group.Update.LegalAction.StandardNumber = "";
            }
            else
            {
              export.Group.Update.LegalAction.StandardNumber =
                entities.LegalAction.StandardNumber;
            }

            export.Group.Update.KsDriversLicense.CourtesyLetterSentDate =
              entities.KsDriversLicense.CourtesyLetterSentDate;
            export.Group.Update.KsDriversLicense.
              Attribute30DayLetterCreatedDate =
                entities.KsDriversLicense.Attribute30DayLetterCreatedDate;
            export.Group.Update.KsDriversLicense.SequenceCounter =
              entities.KsDriversLicense.SequenceCounter;

            if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
            {
              goto ReadEach2;
            }

            local.Case1.Number = entities.Case1.Number;
            MoveKsDriversLicense(local.ReadKsDriversLicense,
              local.PreviousKsDriversLicense);
            local.PreviousLegalAction.StandardNumber =
              entities.LegalAction.StandardNumber;
          }

          local.ReadCase.Number = "";
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

ReadEach2:

        break;
      case "NEXT":
        if (!Equal(export.CsePersonsWorkSet.Number, export.HiddenPrev.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (import.Paging.Count <= 1)
        {
          ExitState = "THERE_NO_MORE_RECORDS_TO_DISPLAY";

          return;
        }
        else
        {
          if (import.PageCount.Count == import.Paging.Count)
          {
            ExitState = "THERE_NO_MORE_RECORDS_TO_DISPLAY";

            return;
          }

          ++export.PageCount.Count;

          for(import.Paging.Index = 0; import.Paging.Index < import
            .Paging.Count; ++import.Paging.Index)
          {
            if (!import.Paging.CheckSize())
            {
              break;
            }

            if (export.PageCount.Count == import.Paging.Index + 1)
            {
              local.ReadCase.Number = import.Paging.Item.PageCase.Number;
              MoveKsDriversLicense(import.Paging.Item.PageKsDriversLicense,
                local.ReadKsDriversLicense);
              local.ReadLegalAction.StandardNumber =
                import.Paging.Item.PageLegalAction.StandardNumber;

              break;
            }
          }

          import.Paging.CheckIndex();
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          export.Group.Update.Case1.Number = local.ClearCase.Number;
          export.Group.Update.KsDriversLicense.Assign(
            local.ClearKsDriversLicense);
          export.Group.Update.Name.Text23 = local.ClearName.Text23;
          export.Group.Update.Office.Text4 = local.ClearOffice.Text4;
          export.Group.Update.LegalAction.StandardNumber =
            local.ClearLegalAction.StandardNumber;
        }

        export.Group.CheckIndex();
        export.Group.Index = -1;
        export.Group.Count = 0;

        foreach(var item in ReadKsDriversLicenseLegalAction())
        {
          // now we need to get the case number
          foreach(var item1 in ReadCase())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (!Equal(entities.LegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber))
            {
              ++export.NumberOfCourtOrders.Count;
            }

            if (!Equal(entities.LegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber) || Equal
              (entities.LegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber) && !
              Equal(entities.KsDriversLicense.CourtesyLetterSentDate,
              local.ReadKsDriversLicense.CourtesyLetterSentDate) || Equal
              (entities.LegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber) && Equal
              (entities.KsDriversLicense.CourtesyLetterSentDate,
              local.ReadKsDriversLicense.CourtesyLetterSentDate) && !
              Equal(entities.KsDriversLicense.Attribute30DayLetterCreatedDate,
              local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate) || Equal
              (entities.LegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber) && Equal
              (entities.KsDriversLicense.CourtesyLetterSentDate,
              local.ReadKsDriversLicense.CourtesyLetterSentDate) && Equal
              (entities.KsDriversLicense.Attribute30DayLetterCreatedDate,
              local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate) && !
              Equal(entities.Case1.Number, local.ReadCase.Number))
            {
              var field =
                GetField(export.Group.Item.SelectOption, "selectChar");

              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
            }
            else
            {
              var field =
                GetField(export.Group.Item.SelectOption, "selectChar");

              field.Protected = true;
            }

            // now we need to get the case worker id or the office
            local.ReadCase.Number = entities.Case1.Number;
            UseSpCabDetOspAssgndToCsecase();
            local.ReadKsDriversLicense.CourtesyLetterSentDate =
              entities.KsDriversLicense.CourtesyLetterSentDate;
            local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate =
              entities.KsDriversLicense.Attribute30DayLetterCreatedDate;
            local.ReadLegalAction.StandardNumber =
              entities.LegalAction.StandardNumber;
            local.Name.Text13 =
              Substring(local.ServiceProvider.LastName, 1, 13);
            local.Name.Text8 = Substring(local.ServiceProvider.FirstName, 1, 8);
            export.Group.Update.Name.Text23 = TrimEnd(local.Name.Text13) + ", " +
              local.Name.Text8;
            export.Group.Update.Office.Text4 =
              Substring(local.Office2.Name, 1, 4);
            export.Group.Update.Case1.Number = entities.Case1.Number;

            if (Equal(local.PreviousLegalAction.StandardNumber,
              local.ReadLegalAction.StandardNumber) && Equal
              (local.PreviousKsDriversLicense.CourtesyLetterSentDate,
              local.ReadKsDriversLicense.CourtesyLetterSentDate) && Equal
              (local.PreviousKsDriversLicense.Attribute30DayLetterCreatedDate,
              local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate))
            {
              export.Group.Update.LegalAction.StandardNumber = "";
            }
            else
            {
              export.Group.Update.LegalAction.StandardNumber =
                entities.LegalAction.StandardNumber;
            }

            export.Group.Update.KsDriversLicense.CourtesyLetterSentDate =
              entities.KsDriversLicense.CourtesyLetterSentDate;
            export.Group.Update.KsDriversLicense.
              Attribute30DayLetterCreatedDate =
                entities.KsDriversLicense.Attribute30DayLetterCreatedDate;
            export.Group.Update.KsDriversLicense.SequenceCounter =
              entities.KsDriversLicense.SequenceCounter;

            if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
            {
              export.MoreLessScroll.Text3 = "+ -";

              if (export.PageCount.Count == export.Paging.Count)
              {
                ++export.Paging.Index;
                export.Paging.CheckSize();

                export.Paging.Update.PageCase.Number = entities.Case1.Number;
                export.Paging.Update.PageLegalAction.StandardNumber =
                  entities.LegalAction.StandardNumber;
                export.Paging.Update.PageKsDriversLicense.
                  CourtesyLetterSentDate =
                    entities.KsDriversLicense.CourtesyLetterSentDate;
                export.Paging.Update.PageKsDriversLicense.
                  Attribute30DayLetterCreatedDate =
                    entities.KsDriversLicense.Attribute30DayLetterCreatedDate;
              }

              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

              goto ReadEach3;
            }

            local.Case1.Number = entities.Case1.Number;
            MoveKsDriversLicense(local.ReadKsDriversLicense,
              local.PreviousKsDriversLicense);
            local.PreviousLegalAction.StandardNumber =
              entities.LegalAction.StandardNumber;
          }

          local.ReadCase.Number = "";
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

ReadEach3:

        if (export.PageCount.Count == 1 && export.Paging.Count > 1)
        {
          export.MoreLessScroll.Text3 = "+";
        }
        else if (export.PageCount.Count < export.Paging.Count && export
          .PageCount.Count > 1)
        {
          export.MoreLessScroll.Text3 = "+ -";
        }
        else if (export.PageCount.Count == export.Paging.Count && export
          .PageCount.Count > 1)
        {
          export.MoreLessScroll.Text3 = "  -";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    export.HiddenPrevUserAction.Command = global.Command;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveKsDriversLicense(KsDriversLicense source,
    KsDriversLicense target)
  {
    target.CourtesyLetterSentDate = source.CourtesyLetterSentDate;
    target.Attribute30DayLetterCreatedDate =
      source.Attribute30DayLetterCreatedDate;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSpCabDetOspAssgndToCsecase()
  {
    var useImport = new SpCabDetOspAssgndToCsecase.Import();
    var useExport = new SpCabDetOspAssgndToCsecase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.AsOfDate.Date = local.Current.Date;

    Call(SpCabDetOspAssgndToCsecase.Execute, useImport, useExport);

    local.Office2.Name = useExport.Office.Name;
    MoveServiceProvider(useExport.ServiceProvider, local.ServiceProvider);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.ReadCase.Number);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.
          SetNullableString(command, "cspNoAp", export.CsePersonsWorkSet.Number);
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadKsDriversLicense()
  {
    entities.KsDriversLicense.Populated = false;

    return Read("ReadKsDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "courtesyLtrDate",
          local.BlankStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 2);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 3);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 4);
        entities.KsDriversLicense.Populated = true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicenseLegalAction()
  {
    entities.KsDriversLicense.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadKsDriversLicenseLegalAction",
      (db, command) =>
      {
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "courtesyLtrDate1",
          local.BlankStartDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "courtesyLtrDate2",
          local.ReadKsDriversLicense.CourtesyLetterSentDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "ltr30DayDate",
          local.ReadKsDriversLicense.Attribute30DayLetterCreatedDate.
            GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", local.ReadLegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.LegalAction.Identifier = db.GetInt32(reader, 1);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 2);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 3);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 4);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.KsDriversLicense.Populated = true;
        entities.LegalAction.Populated = true;

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
    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of PageKsDriversLicense.
      /// </summary>
      [JsonPropertyName("pageKsDriversLicense")]
      public KsDriversLicense PageKsDriversLicense
      {
        get => pageKsDriversLicense ??= new();
        set => pageKsDriversLicense = value;
      }

      /// <summary>
      /// A value of PageLegalAction.
      /// </summary>
      [JsonPropertyName("pageLegalAction")]
      public LegalAction PageLegalAction
      {
        get => pageLegalAction ??= new();
        set => pageLegalAction = value;
      }

      /// <summary>
      /// A value of PageCase.
      /// </summary>
      [JsonPropertyName("pageCase")]
      public Case1 PageCase
      {
        get => pageCase ??= new();
        set => pageCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private KsDriversLicense pageKsDriversLicense;
      private LegalAction pageLegalAction;
      private Case1 pageCase;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of SelectOption.
      /// </summary>
      [JsonPropertyName("selectOption")]
      public Common SelectOption
      {
        get => selectOption ??= new();
        set => selectOption = value;
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
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public WorkArea Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>
      /// A value of Name.
      /// </summary>
      [JsonPropertyName("name")]
      public WorkArea Name
      {
        get => name ??= new();
        set => name = value;
      }

      /// <summary>
      /// A value of KsDriversLicense.
      /// </summary>
      [JsonPropertyName("ksDriversLicense")]
      public KsDriversLicense KsDriversLicense
      {
        get => ksDriversLicense ??= new();
        set => ksDriversLicense = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Common selectOption;
      private LegalAction legalAction;
      private Case1 case1;
      private WorkArea office;
      private WorkArea name;
      private KsDriversLicense ksDriversLicense;
    }

    /// <summary>
    /// A value of NumberOfCourtOrders.
    /// </summary>
    [JsonPropertyName("numberOfCourtOrders")]
    public Common NumberOfCourtOrders
    {
      get => numberOfCourtOrders ??= new();
      set => numberOfCourtOrders = value;
    }

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public CsePersonsWorkSet Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    /// <summary>
    /// A value of PageCount.
    /// </summary>
    [JsonPropertyName("pageCount")]
    public Common PageCount
    {
      get => pageCount ??= new();
      set => pageCount = value;
    }

    /// <summary>
    /// Gets a value of Paging.
    /// </summary>
    [JsonIgnore]
    public Array<PagingGroup> Paging => paging ??= new(PagingGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Paging for json serialization.
    /// </summary>
    [JsonPropertyName("paging")]
    [Computed]
    public IList<PagingGroup> Paging_Json
    {
      get => paging;
      set => Paging.Assign(value);
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
    /// A value of PromptCsePersonNumber.
    /// </summary>
    [JsonPropertyName("promptCsePersonNumber")]
    public Common PromptCsePersonNumber
    {
      get => promptCsePersonNumber ??= new();
      set => promptCsePersonNumber = value;
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

    /// <summary>
    /// A value of MoreLessScroll.
    /// </summary>
    [JsonPropertyName("moreLessScroll")]
    public WorkArea MoreLessScroll
    {
      get => moreLessScroll ??= new();
      set => moreLessScroll = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePersonsWorkSet HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    private Common numberOfCourtOrders;
    private CsePersonsWorkSet flow;
    private Common pageCount;
    private Array<PagingGroup> paging;
    private Array<GroupGroup> group;
    private Common promptCsePersonNumber;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea moreLessScroll;
    private Common hiddenPrevUserAction;
    private CsePersonsWorkSet hiddenPrev;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of PageKsDriversLicense.
      /// </summary>
      [JsonPropertyName("pageKsDriversLicense")]
      public KsDriversLicense PageKsDriversLicense
      {
        get => pageKsDriversLicense ??= new();
        set => pageKsDriversLicense = value;
      }

      /// <summary>
      /// A value of PageLegalAction.
      /// </summary>
      [JsonPropertyName("pageLegalAction")]
      public LegalAction PageLegalAction
      {
        get => pageLegalAction ??= new();
        set => pageLegalAction = value;
      }

      /// <summary>
      /// A value of PageCase.
      /// </summary>
      [JsonPropertyName("pageCase")]
      public Case1 PageCase
      {
        get => pageCase ??= new();
        set => pageCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private KsDriversLicense pageKsDriversLicense;
      private LegalAction pageLegalAction;
      private Case1 pageCase;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of SelectOption.
      /// </summary>
      [JsonPropertyName("selectOption")]
      public Common SelectOption
      {
        get => selectOption ??= new();
        set => selectOption = value;
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
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public WorkArea Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>
      /// A value of Name.
      /// </summary>
      [JsonPropertyName("name")]
      public WorkArea Name
      {
        get => name ??= new();
        set => name = value;
      }

      /// <summary>
      /// A value of KsDriversLicense.
      /// </summary>
      [JsonPropertyName("ksDriversLicense")]
      public KsDriversLicense KsDriversLicense
      {
        get => ksDriversLicense ??= new();
        set => ksDriversLicense = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Common selectOption;
      private LegalAction legalAction;
      private Case1 case1;
      private WorkArea office;
      private WorkArea name;
      private KsDriversLicense ksDriversLicense;
    }

    /// <summary>
    /// A value of NumberOfCourtOrders.
    /// </summary>
    [JsonPropertyName("numberOfCourtOrders")]
    public Common NumberOfCourtOrders
    {
      get => numberOfCourtOrders ??= new();
      set => numberOfCourtOrders = value;
    }

    /// <summary>
    /// A value of PageCount.
    /// </summary>
    [JsonPropertyName("pageCount")]
    public Common PageCount
    {
      get => pageCount ??= new();
      set => pageCount = value;
    }

    /// <summary>
    /// Gets a value of Paging.
    /// </summary>
    [JsonIgnore]
    public Array<PagingGroup> Paging => paging ??= new(PagingGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Paging for json serialization.
    /// </summary>
    [JsonPropertyName("paging")]
    [Computed]
    public IList<PagingGroup> Paging_Json
    {
      get => paging;
      set => Paging.Assign(value);
    }

    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
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
    /// A value of PromptAdminActType.
    /// </summary>
    [JsonPropertyName("promptAdminActType")]
    public Standard PromptAdminActType
    {
      get => promptAdminActType ??= new();
      set => promptAdminActType = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePerson Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of PromptCsePersonNumber.
    /// </summary>
    [JsonPropertyName("promptCsePersonNumber")]
    public Common PromptCsePersonNumber
    {
      get => promptCsePersonNumber ??= new();
      set => promptCsePersonNumber = value;
    }

    /// <summary>
    /// A value of FlowToKdmv.
    /// </summary>
    [JsonPropertyName("flowToKdmv")]
    public CsePersonsWorkSet FlowToKdmv
    {
      get => flowToKdmv ??= new();
      set => flowToKdmv = value;
    }

    /// <summary>
    /// A value of MoreLessScroll.
    /// </summary>
    [JsonPropertyName("moreLessScroll")]
    public WorkArea MoreLessScroll
    {
      get => moreLessScroll ??= new();
      set => moreLessScroll = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePersonsWorkSet HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public KsDriversLicense Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    private Common numberOfCourtOrders;
    private Common pageCount;
    private Array<PagingGroup> paging;
    private KsDriversLicense ksDriversLicense;
    private Standard standard;
    private Standard promptAdminActType;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson selected;
    private Array<GroupGroup> group;
    private Common promptCsePersonNumber;
    private CsePersonsWorkSet flowToKdmv;
    private WorkArea moreLessScroll;
    private Common hiddenDisplayPerformed;
    private Common hiddenPrevUserAction;
    private CsePersonsWorkSet hiddenPrev;
    private KsDriversLicense flow;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ClearKsDriversLicense.
    /// </summary>
    [JsonPropertyName("clearKsDriversLicense")]
    public KsDriversLicense ClearKsDriversLicense
    {
      get => clearKsDriversLicense ??= new();
      set => clearKsDriversLicense = value;
    }

    /// <summary>
    /// A value of ClearName.
    /// </summary>
    [JsonPropertyName("clearName")]
    public WorkArea ClearName
    {
      get => clearName ??= new();
      set => clearName = value;
    }

    /// <summary>
    /// A value of ClearOffice.
    /// </summary>
    [JsonPropertyName("clearOffice")]
    public WorkArea ClearOffice
    {
      get => clearOffice ??= new();
      set => clearOffice = value;
    }

    /// <summary>
    /// A value of ClearCase.
    /// </summary>
    [JsonPropertyName("clearCase")]
    public Case1 ClearCase
    {
      get => clearCase ??= new();
      set => clearCase = value;
    }

    /// <summary>
    /// A value of ClearLegalAction.
    /// </summary>
    [JsonPropertyName("clearLegalAction")]
    public LegalAction ClearLegalAction
    {
      get => clearLegalAction ??= new();
      set => clearLegalAction = value;
    }

    /// <summary>
    /// A value of ReadPreviousLegalAction.
    /// </summary>
    [JsonPropertyName("readPreviousLegalAction")]
    public LegalAction ReadPreviousLegalAction
    {
      get => readPreviousLegalAction ??= new();
      set => readPreviousLegalAction = value;
    }

    /// <summary>
    /// A value of ReadPreviousKsDriversLicense.
    /// </summary>
    [JsonPropertyName("readPreviousKsDriversLicense")]
    public KsDriversLicense ReadPreviousKsDriversLicense
    {
      get => readPreviousKsDriversLicense ??= new();
      set => readPreviousKsDriversLicense = value;
    }

    /// <summary>
    /// A value of ReadPreviousCase.
    /// </summary>
    [JsonPropertyName("readPreviousCase")]
    public Case1 ReadPreviousCase
    {
      get => readPreviousCase ??= new();
      set => readPreviousCase = value;
    }

    /// <summary>
    /// A value of PageCounter.
    /// </summary>
    [JsonPropertyName("pageCounter")]
    public Common PageCounter
    {
      get => pageCounter ??= new();
      set => pageCounter = value;
    }

    /// <summary>
    /// A value of PreviousKsDriversLicense.
    /// </summary>
    [JsonPropertyName("previousKsDriversLicense")]
    public KsDriversLicense PreviousKsDriversLicense
    {
      get => previousKsDriversLicense ??= new();
      set => previousKsDriversLicense = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
    }

    /// <summary>
    /// A value of Office1.
    /// </summary>
    [JsonPropertyName("office1")]
    public WorkArea Office1
    {
      get => office1 ??= new();
      set => office1 = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public WorkArea Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of ReadLegalAction.
    /// </summary>
    [JsonPropertyName("readLegalAction")]
    public LegalAction ReadLegalAction
    {
      get => readLegalAction ??= new();
      set => readLegalAction = value;
    }

    /// <summary>
    /// A value of ReadKsDriversLicense.
    /// </summary>
    [JsonPropertyName("readKsDriversLicense")]
    public KsDriversLicense ReadKsDriversLicense
    {
      get => readKsDriversLicense ??= new();
      set => readKsDriversLicense = value;
    }

    /// <summary>
    /// A value of ReadCase.
    /// </summary>
    [JsonPropertyName("readCase")]
    public Case1 ReadCase
    {
      get => readCase ??= new();
      set => readCase = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Office2.
    /// </summary>
    [JsonPropertyName("office2")]
    public Office Office2
    {
      get => office2 ??= new();
      set => office2 = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of BlankStartDate.
    /// </summary>
    [JsonPropertyName("blankStartDate")]
    public DateWorkArea BlankStartDate
    {
      get => blankStartDate ??= new();
      set => blankStartDate = value;
    }

    /// <summary>
    /// A value of CountExmp.
    /// </summary>
    [JsonPropertyName("countExmp")]
    public Common CountExmp
    {
      get => countExmp ??= new();
      set => countExmp = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public CsePersonsWorkSet Saved
    {
      get => saved ??= new();
      set => saved = value;
    }

    /// <summary>
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public CsePersonsWorkSet InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public CsePersonsWorkSet Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
    }

    private KsDriversLicense clearKsDriversLicense;
    private WorkArea clearName;
    private WorkArea clearOffice;
    private Case1 clearCase;
    private LegalAction clearLegalAction;
    private LegalAction readPreviousLegalAction;
    private KsDriversLicense readPreviousKsDriversLicense;
    private Case1 readPreviousCase;
    private Common pageCounter;
    private KsDriversLicense previousKsDriversLicense;
    private LegalAction previousLegalAction;
    private WorkArea office1;
    private WorkArea name;
    private LegalAction readLegalAction;
    private KsDriversLicense readKsDriversLicense;
    private Case1 readCase;
    private Case1 case1;
    private DateWorkArea current;
    private Office office2;
    private ServiceProvider serviceProvider;
    private LegalAction legalAction;
    private DateWorkArea blankStartDate;
    private Common countExmp;
    private TextWorkArea textWorkArea;
    private CsePersonsWorkSet saved;
    private CsePersonsWorkSet initialisedToSpaces;
    private CsePerson csePerson;
    private Common selected;
    private NextTranInfo nextTranInfo;
    private CsePersonsWorkSet initialised;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    private KsDriversLicense ksDriversLicense;
    private LegalAction legalAction;
    private Case1 case1;
    private CaseRole caseRole;
    private CaseUnit caseUnit;
    private CsePerson csePerson;
    private LegalActionCaseRole legalActionCaseRole;
  }
#endregion
}
