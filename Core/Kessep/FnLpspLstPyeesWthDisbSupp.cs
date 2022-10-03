// Program: FN_LPSP_LST_PYEES_WTH_DISB_SUPP, ID: 371754761, model: 746.
// Short name: SWELPSPP
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
/// A program: FN_LPSP_LST_PYEES_WTH_DISB_SUPP.
/// </para>
/// <para>
/// RESP: FINANCE
/// This will list all of the payees that currently have
/// disbursements suppressed.  It may also show all payees that
/// have ever had disbursements suppressed.  This list may be
/// limited to just payees assigned to a specific worker.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnLpspLstPyeesWthDisbSupp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_LPSP_LST_PYEES_WTH_DISB_SUPP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnLpspLstPyeesWthDisbSupp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnLpspLstPyeesWthDisbSupp.
  /// </summary>
  public FnLpspLstPyeesWthDisbSupp(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ------------	-----------	
    // ---------------------------------------------
    // ??/??/??  ?????????			Initial Development
    // 12/11/96  R. Marchman			Add new security and next tran
    // 04/30/97  SHERAZ MALIK			CHANGE CURRENT_DATE
    // 03/23/98  Siraj Konkader		ZDEL cleanup
    // 11/03/98  RK				Made the following changes as part of Phase II.
    // 					1. Suppressions for AP's should be viewable
    // 					   as well.
    // 					2. An invalid Collection Type should should
    // 					   be errored as such.
    // 					3. If the number of returning records is
    // 					   above the max size of the group view(150
    // 					   now) then tell the users more data is out
    // 					   there than can be seen by this screen.
    // 					4. Make sure that the error messages for the
    // 					   prompts are correct for each situation.
    // 					5. When flowing to NAME make sure that a
    // 					   command is sent to make the 35% appear on
    // 					   that screen.
    // 					6. Change Caseload Worker to Case Coordinator
    // 					   on screen.
    // 					7. Make one of the 4 major fields(Case
    // 					   Coordinator, Suppressing Worker, Payee
    // 					   Number or Collection Type) is entered for
    // 					   a display.
    // 					8. Change Col Type to Sup Type on screen.
    // 06/03/99  RK				Integration test fixes:
    // 					1. No flow or NEXT function should be allowed
    // 					   with an invalid CSE Person number.
    // 					2. After a display is done, place the cursor
    // 					   on the first select field.
    // 02/03/00  SWSRKXD	PR#81986	1. Add Date filter
    // 					2. Change sort criteria
    // 					3. Increase GV limits
    // 09/18/00  Fangman	PRWORA
    // 			SEG ID A4	1. Added suppression rule type to screen.
    // 					2. Change Sup Typ column heading to Coll Typ
    // 					   column heading.
    // 					3. Added code for specific read when payee #
    // 					   entered.
    // 09/26/00  Fangman	PR 98039	Added logic to show Payees with rules created
    // 					by "Suppressing Worker" SWEFB651 which
    // 					creates the X ura rule and the Duplicate
    // 					Payment rule.
    // 01/13/05  Fangman	WR 040796	Added changes for suppression by court order
    // 					number.
    // 07/02/19  GVandy	CQ65423		Add Suppression Type filter.
    // -------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    local.Common.Count = 0;
    ExitState = "ACO_NN0000_ALL_OK";
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "RETCSENO"))
    {
      global.Command = "DISPLAY";
    }

    // Move all IMPORTs to EXPORTs.
    export.ShowHistory.Flag = import.ShowHistory.Flag;
    export.FilterCollectionType.Code = import.FilterCollectionType.Code;
    export.WorkerServiceProvider.UserId = import.WorkerServiceProvider.UserId;
    export.SuppressingDisbSuppressionStatusHistory.CreatedBy =
      import.SuppressingDisbSuppressionStatusHistory.CreatedBy;
    MoveCsePersonsWorkSet(import.SearchPayee, export.SearchPayee);

    // ****************************************************************
    // 2/21/2000 PR#81986 SWSRKXD
    //  Add Date filter
    // ****************************************************************
    export.FromDate.EffectiveDate = import.FromDate.EffectiveDate;
    export.ToDate.DiscontinueDate = import.ToDate.DiscontinueDate;
    export.PayeeNumberPrompt.Text1 = import.PayeeNumberPrompt.Text1;
    export.CollectionTypePrompt.Text1 = import.CollectionTypePrompt.Text1;
    export.FilterDisbSuppressionStatusHistory.Type1 =
      import.FilterDisbSuppressionStatusHistory.Type1;
    export.SuppressionTypePrompt.Text1 = import.SuppressionTypePrompt.Text1;
    export.SuppressionType.Description = import.SuppressionType.Description;

    if (Equal(global.Command, "RETCDVL"))
    {
      if (!IsEmpty(import.FromCdvl.Cdvalue))
      {
        export.FilterDisbSuppressionStatusHistory.Type1 =
          Substring(import.FromCdvl.Cdvalue, 1, 1);
        export.SuppressionType.Description = import.FromCdvl.Description;
        export.SuppressionTypePrompt.Text1 = "";
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      export.SearchPayee.FormattedName = "";
      local.LeftPadding.Text10 = export.SearchPayee.Number;
      UseEabPadLeftWithZeros();
      export.SearchPayee.Number = local.LeftPadding.Text10;
    }
    else
    {
      export.Export1.Index = -1;
      export.SuppressingCsePersonsWorkSet.FormattedName =
        import.SuppressingCsePersonsWorkSet.FormattedName;
      export.WorkerCsePersonsWorkSet.FormattedName =
        import.WorkerCsePersonsWorkSet.FormattedName;

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (!import.Import1.CheckSize())
        {
          break;
        }

        export.Export1.Index = import.Import1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        MoveCsePersonsWorkSet(import.Import1.Item.DetailPayee,
          export.Export1.Update.DetailPayee);
        export.Export1.Update.DetailDisbSuppressionStatusHistory.Assign(
          import.Import1.Item.DetailDisbSuppressionStatusHistory);
        export.Export1.Update.DetailCollectionType.Code =
          import.Import1.Item.DetailCollectionType.Code;

        // *****  changes for WR 040796
        export.Export1.Update.DetailLegalAction.StandardNumber =
          import.Import1.Item.DetailLegalAction.StandardNumber;

        // *****  changes for WR 040796
      }

      import.Import1.CheckIndex();
    }

    local.Common.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          export.SelectedDisbSuppressionStatusHistory.Assign(
            export.Export1.Item.DetailDisbSuppressionStatusHistory);
          MoveCsePersonsWorkSet(export.Export1.Item.DetailPayee,
            export.SelectedPayee);
          export.SelectedCollectionType.Code =
            export.Export1.Item.DetailCollectionType.Code;
          export.SelectedCsePerson.Number = export.SelectedPayee.Number;

          // *****  changes for WR 040796
          export.SelectedLegalAction.StandardNumber =
            export.Export1.Item.DetailLegalAction.StandardNumber;

          // *****  changes for WR 040796
          ++local.Common.Count;
          local.Common.SelectChar = export.Export1.Item.DetailCommon.SelectChar;

          break;
        default:
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          return;
      }
    }

    export.Export1.CheckIndex();

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.Hidden.CsePersonNumberObligee = export.SelectedPayee.Number;
      export.Hidden.CsePersonNumber = export.SelectedPayee.Number;
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
      UseScCabNextTranGet();

      if (!IsEmpty(export.Hidden.CsePersonNumberObligee))
      {
        export.SearchPayee.Number = export.Hidden.CsePersonNumberObligee ?? Spaces
          (10);
      }
      else
      {
        export.SearchPayee.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "PSUP") || Equal(global.Command, "CSUP") || Equal
      (global.Command, "LDSP"))
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

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PSUP") || Equal
      (global.Command, "CSUP") || Equal(global.Command, "LDSP"))
    {
      if (Equal(global.Command, "DISPLAY"))
      {
        // ****************************************************************
        // One of the 4 main fields must have been entered. RK 11/2/98.
        // ****************************************************************
        if (IsEmpty(export.WorkerServiceProvider.UserId) && IsEmpty
          (export.SuppressingDisbSuppressionStatusHistory.CreatedBy) && IsEmpty
          (export.SearchPayee.Number) && IsEmpty
          (export.FilterCollectionType.Code))
        {
          var field1 = GetField(export.WorkerServiceProvider, "userId");

          field1.Error = true;

          var field2 =
            GetField(export.SuppressingDisbSuppressionStatusHistory, "createdBy");
            

          field2.Error = true;

          var field3 = GetField(export.SearchPayee, "number");

          field3.Error = true;

          var field4 = GetField(export.FilterCollectionType, "code");

          field4.Error = true;

          ExitState = "FN0000_ENTER_ONE_OF_THESE_FIELDS";

          return;
        }
      }

      // **********************
      // Suppressing worker
      // *********************
      if (!IsEmpty(export.SuppressingDisbSuppressionStatusHistory.CreatedBy))
      {
        // ****************************************************************
        // If the Suppression was an Automatic one then say that the Batch 650 
        // job did it.
        // ****************************************************************
        switch(TrimEnd(export.SuppressingDisbSuppressionStatusHistory.CreatedBy))
          
        {
          case "SWEFB650":
            export.SuppressingCsePersonsWorkSet.FormattedName =
              "Disbursement, Batch-Credit C";

            break;
          case "SWEFB651":
            export.SuppressingCsePersonsWorkSet.FormattedName =
              "Disbursement, Batch-Debit D";

            break;
          default:
            if (ReadServiceProvider1())
            {
              local.CsePersonsWorkSet.FirstName =
                entities.ServiceProvider.FirstName;
              local.CsePersonsWorkSet.LastName =
                entities.ServiceProvider.LastName;
              local.CsePersonsWorkSet.MiddleInitial =
                entities.ServiceProvider.MiddleInitial;
              UseSiFormatCsePersonName3();
            }
            else
            {
              if (Equal(global.Command, "DISPLAY"))
              {
                ExitState = "ZD_SERVICE_PROVIDER_NF_1";
              }
              else
              {
                ExitState = "FN0000_NO_FLOW_ALLWD_INV_PERSON";
              }

              var field =
                GetField(export.SuppressingDisbSuppressionStatusHistory,
                "createdBy");

              field.Error = true;

              return;
            }

            break;
        }
      }

      // ********************
      // Case Coordinator
      // *******************
      if (!IsEmpty(export.WorkerServiceProvider.UserId))
      {
        if (ReadServiceProvider2())
        {
          local.CsePersonsWorkSet.FirstName =
            entities.ServiceProvider.FirstName;
          local.CsePersonsWorkSet.LastName = entities.ServiceProvider.LastName;
          local.CsePersonsWorkSet.MiddleInitial =
            entities.ServiceProvider.MiddleInitial;
          UseSiFormatCsePersonName2();
        }
        else
        {
          if (Equal(global.Command, "DISPLAY"))
          {
            ExitState = "ZD_SERVICE_PROVIDER_NF_1";
          }
          else
          {
            ExitState = "FN0000_NO_FLOW_ALLWD_INV_PERSON";
          }

          var field = GetField(export.WorkerServiceProvider, "userId");

          field.Error = true;

          return;
        }
      }

      // ********
      // Payee
      // ********
      if (!IsEmpty(export.SearchPayee.Number))
      {
        if (ReadCsePerson2())
        {
          if (AsChar(entities.StartingPayee.Type1) == 'O')
          {
            export.SearchPayee.FormattedName =
              entities.StartingPayee.OrganizationName ?? Spaces(33);
          }
          else
          {
            UseCabReadAdabasPerson1();
            UseSiFormatCsePersonName1();
            export.SearchPayee.FormattedName = local.Starting.FormattedName;
          }
        }
        else
        {
          if (Equal(global.Command, "DISPLAY"))
          {
            ExitState = "FN0000_PAYEE_NOT_FOUND";
          }
          else
          {
            ExitState = "FN0000_NO_FLOW_ALLWD_INV_PERSON";
          }

          var field = GetField(export.SearchPayee, "number");

          field.Error = true;

          return;
        }
      }
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ****************************************************************
        // 2/21/2000 PR#81986 SWSRKXD
        // If Payee # has been entered and history is not required, override 
        // date filters and display all records!
        // ****************************************************************
        if (!IsEmpty(export.SearchPayee.Number) && AsChar
          (export.ShowHistory.Flag) != 'Y')
        {
          local.FromDate.EffectiveDate = new DateTime(1901, 1, 1);
          local.ToDate.DiscontinueDate = new DateTime(2099, 12, 31);
          export.FromDate.EffectiveDate = null;
          export.ToDate.DiscontinueDate = null;
        }
        else
        {
          // ****************************************************************
          // Default filter to 1 month when dates are left blank.
          // ****************************************************************
          if (Equal(export.FromDate.EffectiveDate, null))
          {
            export.FromDate.EffectiveDate = Now().Date.AddMonths(-1);
          }

          if (Equal(export.ToDate.DiscontinueDate, null))
          {
            export.ToDate.DiscontinueDate = Now().Date;
          }

          // *************************
          // Ensure dates are valid.
          // *************************
          if (Lt(export.ToDate.DiscontinueDate, export.FromDate.EffectiveDate))
          {
            ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

            var field = GetField(export.FromDate, "effectiveDate");

            field.Error = true;
          }

          if (Lt(Now().Date, export.ToDate.DiscontinueDate))
          {
            ExitState = "TO_DATE_GREATER_CURRENT_DATE";

            var field = GetField(export.ToDate, "discontinueDate");

            field.Error = true;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          local.FromDate.EffectiveDate = export.FromDate.EffectiveDate;
          local.ToDate.DiscontinueDate = export.ToDate.DiscontinueDate;
        }

        if (!IsEmpty(export.FilterCollectionType.Code))
        {
          if (ReadCollectionType1())
          {
            // Continue on
          }
          else
          {
            ExitState = "FN0000_INV_COLL_TYPE";

            var field = GetField(export.FilterCollectionType, "code");

            field.Error = true;

            return;
          }
        }

        // 07/02/19 GVandy  CQ65423  Add Suppression Type filter.
        // --Validate the suppression type filter.
        if (IsEmpty(export.FilterDisbSuppressionStatusHistory.Type1))
        {
          export.SuppressionType.Description =
            Spaces(CodeValue.Description_MaxLength);
        }
        else
        {
          local.Code.CodeName = "SUPPRESSION TYPE";
          local.CodeValue.Cdvalue =
            export.FilterDisbSuppressionStatusHistory.Type1;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'Y')
          {
            export.SuppressionType.Description = local.CodeValue.Description;
          }
          else
          {
            var field = GetField(export.SuppressionType, "description");

            field.Error = true;

            export.SuppressionType.Description =
              Spaces(CodeValue.Description_MaxLength);
            ExitState = "FN0000_INVALID_SUPPRESSION_TYPE";

            return;
          }
        }

        export.Export1.Index = -1;

        // *******************************************************
        // PR#81986 - 2/21/2000
        // Change sort to desc Effective_date.
        // NB - Export list is sorted by Asc Alpha Name and then Desc
        // Effective_date. Pstep calls a CAB further down to sort by Name.
        // *******************************************************
        if (!IsEmpty(export.SearchPayee.Number))
        {
          // Read only for one Payee.
          foreach(var item in ReadDisbSuppressionStatusHistoryCsePerson2())
          {
            if (!IsEmpty(export.SearchPayee.Number))
            {
              if (!Equal(entities.Obligee2.Number, export.SearchPayee.Number))
              {
                continue;
              }
            }

            // Now check to see if show history is set
            if (AsChar(export.ShowHistory.Flag) != 'Y')
            {
              // History is required skip inactive suppressions
              if (Lt(local.Current.Date,
                entities.DisbSuppressionStatusHistory.EffectiveDate) || Lt
                (entities.DisbSuppressionStatusHistory.DiscontinueDate,
                local.Current.Date))
              {
                continue;
              }
            }

            // now check to see if suppressing worker was entered
            if (!IsEmpty(export.SuppressingDisbSuppressionStatusHistory.
              CreatedBy))
            {
              // check the suppressing worker
              if (!Equal(entities.DisbSuppressionStatusHistory.CreatedBy,
                import.SuppressingDisbSuppressionStatusHistory.CreatedBy))
              {
                continue;
              }
            }

            // now check to see if service provider was entered
            // ****************************************************************
            // Should read for records tied to AP as well. RK 11/2/98
            // Read for an currently active case role taken out. Just need one 
            // in the past. rk 7/18/99
            // ****************************************************************
            if (!IsEmpty(export.WorkerServiceProvider.UserId))
            {
              if (!ReadCaseRoleCase())
              {
                continue;
              }

              UseSpCabDetOspAssgndToCsecase();

              if (!Equal(local.Assigned.UserId,
                export.WorkerServiceProvider.UserId))
              {
                continue;
              }
            }

            // -----------------------------------------------------------
            // Sometimes there is no collection type. If a suppression is 
            // done by person the collection type will be blank. In this
            // case the read will not find a valid collection type in the
            // collection type table. When the not found condition is met
            // space is moved to a local collection type view. This view
            // will be moved later to the group view to  display blanks, or
            // the correct collection type.
            // ------------------------------------------------------------
            // *****  changes for WR 040796
            local.LegalAction.StandardNumber = "";

            if (AsChar(entities.DisbSuppressionStatusHistory.Type1) == 'O')
            {
              if (ReadLegalAction())
              {
                local.LegalAction.StandardNumber =
                  entities.LegalAction.StandardNumber;
              }
              else
              {
                ExitState = "LEGAL_ACTION_NF_RB";

                return;
              }
            }

            // *****  changes for WR 040796
            // *****  changes for WR 040796
            if (AsChar(entities.DisbSuppressionStatusHistory.Type1) == 'C' || AsChar
              (entities.DisbSuppressionStatusHistory.Type1) == 'O')
            {
              // *****  changes for WR 040796
              if (ReadCollectionType2())
              {
                local.CollectionType.Text1 = entities.CollectionType.Code;
              }
              else
              {
                if (AsChar(entities.DisbSuppressionStatusHistory.Type1) == 'C')
                {
                  ExitState = "FN0000_COLLECTION_TYPE_NF_RB";

                  return;
                }

                // *****  changes for WR 040796
                local.CollectionType.Text1 = "";

                // *****  changes for WR 040796
              }
            }
            else
            {
              local.CollectionType.Text1 = "";
            }

            // now check to see if a filter collection type was entered.
            if (!IsEmpty(export.FilterCollectionType.Code))
            {
              if (Equal(local.CollectionType.Text1,
                export.FilterCollectionType.Code))
              {
              }
              else
              {
                continue;
              }
            }

            // 07/02/19 GVandy  CQ65423  Add Suppression Type filter.
            // -- Check for Suppression Type filter.
            if (!IsEmpty(export.FilterDisbSuppressionStatusHistory.Type1))
            {
              if (AsChar(entities.DisbSuppressionStatusHistory.Type1) != AsChar
                (export.FilterDisbSuppressionStatusHistory.Type1))
              {
                continue;
              }
            }

            // if we get here we have a value that needs to be displayed
            if (export.Export1.Index + 1 < Export.ExportGroup.Capacity)
            {
              ++export.Export1.Index;
              export.Export1.CheckSize();

              MoveCsePersonsWorkSet(export.SearchPayee,
                export.Export1.Update.DetailPayee);
              export.Export1.Update.DetailDisbSuppressionStatusHistory.Assign(
                entities.DisbSuppressionStatusHistory);

              // *****  changes for WR 040796
              export.Export1.Update.DetailLegalAction.StandardNumber =
                local.LegalAction.StandardNumber;

              // *****  changes for WR 040796
              // The discontinue date for X URA suppression rule's discontinue 
              // date is the effective date + the number of suppression days set
              // in a control table.  This is needed because the number of
              // suppression days can be changed at any time.
              if (AsChar(entities.DisbSuppressionStatusHistory.Type1) == 'X')
              {
                if (!entities.UraSuppressionLength.Populated)
                {
                  if (ReadControlTable())
                  {
                    // Continue
                  }
                  else
                  {
                    local.EabReportSend.RptDetail =
                      "Error: Not found reading Control table w/ ID of 'URA SUPPRESSION LENGTH'.";
                      
                  }
                }

                export.Export1.Update.DetailDisbSuppressionStatusHistory.
                  DiscontinueDate =
                    AddDays(entities.DisbSuppressionStatusHistory.EffectiveDate,
                  entities.UraSuppressionLength.LastUsedNumber);
              }

              if (Equal(export.Export1.Item.DetailDisbSuppressionStatusHistory.
                DiscontinueDate, local.MaxDate.Date))
              {
                export.Export1.Update.DetailDisbSuppressionStatusHistory.
                  DiscontinueDate = local.InitialisedToZeros.Date;
              }

              export.Export1.Update.DetailCollectionType.Code =
                local.CollectionType.Text1;
            }
            else
            {
              break;
            }

            // end of read each disb suppression status history
          }
        }
        else
        {
          // Read for multiple Payees.
          foreach(var item in ReadDisbSuppressionStatusHistoryCsePerson1())
          {
            if (!IsEmpty(export.SearchPayee.Number))
            {
              if (!Equal(entities.Obligee2.Number, export.SearchPayee.Number))
              {
                continue;
              }
            }

            // Now check to see if show history is set
            if (AsChar(export.ShowHistory.Flag) != 'Y')
            {
              // History is required skip inactive suppressions
              if (Lt(local.Current.Date,
                entities.DisbSuppressionStatusHistory.EffectiveDate) || Lt
                (entities.DisbSuppressionStatusHistory.DiscontinueDate,
                local.Current.Date))
              {
                continue;
              }
            }

            // now check to see if suppressing worker was entered
            if (!IsEmpty(export.SuppressingDisbSuppressionStatusHistory.
              CreatedBy))
            {
              // check the suppressing worker
              if (!Equal(entities.DisbSuppressionStatusHistory.CreatedBy,
                import.SuppressingDisbSuppressionStatusHistory.CreatedBy))
              {
                continue;
              }
            }

            // now check to see if service provider was entered
            // ****************************************************************
            // Should read for records tied to AP as well. RK 11/2/98
            // Read for an currently active case role taken out. Just need one 
            // in the past. rk 7/18/99
            // ****************************************************************
            if (!IsEmpty(export.WorkerServiceProvider.UserId))
            {
              if (!ReadCaseRoleCase())
              {
                continue;
              }

              UseSpCabDetOspAssgndToCsecase();

              if (!Equal(local.Assigned.UserId,
                export.WorkerServiceProvider.UserId))
              {
                continue;
              }
            }

            // -----------------------------------------------------------
            // Sometimes there is no collection type. If a suppression is 
            // done by person the collection type will be blank. In this
            // case the read will not find a valid collection type in the
            // collection type table. When the not found condition is met
            // space is moved to a local collection type view. This view
            // will be moved later to the group view to  display blanks, or
            // the correct collection type.
            // ------------------------------------------------------------
            // *****  changes for WR 040796
            local.LegalAction.StandardNumber = "";

            if (AsChar(entities.DisbSuppressionStatusHistory.Type1) == 'O')
            {
              if (ReadLegalAction())
              {
                local.LegalAction.StandardNumber =
                  entities.LegalAction.StandardNumber;
              }
              else
              {
                ExitState = "LEGAL_ACTION_NF_RB";

                return;
              }
            }

            // *****  changes for WR 040796
            // *****  changes for WR 040796
            if (AsChar(entities.DisbSuppressionStatusHistory.Type1) == 'C' || AsChar
              (entities.DisbSuppressionStatusHistory.Type1) == 'O')
            {
              // *****  changes for WR 040796
              if (ReadCollectionType2())
              {
                local.CollectionType.Text1 = entities.CollectionType.Code;
              }
              else
              {
                if (AsChar(entities.DisbSuppressionStatusHistory.Type1) == 'C')
                {
                  ExitState = "FN0000_COLLECTION_TYPE_NF_RB";

                  return;
                }

                // *****  changes for WR 040796
                local.CollectionType.Text1 = "";

                // *****  changes for WR 040796
              }
            }
            else
            {
              local.CollectionType.Text1 = "";
            }

            // now check to see if a filter collection type was entered.
            if (!IsEmpty(export.FilterCollectionType.Code))
            {
              if (Equal(local.CollectionType.Text1,
                export.FilterCollectionType.Code))
              {
              }
              else
              {
                continue;
              }
            }

            // 07/02/19 GVandy  CQ65423  Add Suppression Type filter.
            // -- Check for Suppression Type filter.
            if (!IsEmpty(export.FilterDisbSuppressionStatusHistory.Type1))
            {
              if (AsChar(entities.DisbSuppressionStatusHistory.Type1) != AsChar
                (export.FilterDisbSuppressionStatusHistory.Type1))
              {
                continue;
              }
            }

            // if we get here we have a value that needs to be displayed
            if (export.Export1.Index + 1 < Export.ExportGroup.Capacity)
            {
              ++export.Export1.Index;
              export.Export1.CheckSize();
            }
            else
            {
              break;
            }

            export.Export1.Update.DetailPayee.Number = entities.Obligee2.Number;
            export.Export1.Update.DetailDisbSuppressionStatusHistory.Assign(
              entities.DisbSuppressionStatusHistory);

            // *****  changes for WR 040796
            export.Export1.Update.DetailLegalAction.StandardNumber =
              local.LegalAction.StandardNumber;

            // *****  changes for WR 040796
            // The discontinue date for X URA suppression rule's discontinue 
            // date is the effective date + the number of suppression days set
            // in a control table.  This is needed because the number of
            // suppression days can be changed at any time.
            if (AsChar(entities.DisbSuppressionStatusHistory.Type1) == 'X')
            {
              if (!entities.UraSuppressionLength.Populated)
              {
                if (ReadControlTable())
                {
                  // Continue
                }
                else
                {
                  local.EabReportSend.RptDetail =
                    "Error: Not found reading Control table w/ ID of 'URA SUPPRESSION LENGTH'.";
                    
                }
              }

              export.Export1.Update.DetailDisbSuppressionStatusHistory.
                DiscontinueDate =
                  AddDays(entities.DisbSuppressionStatusHistory.EffectiveDate,
                entities.UraSuppressionLength.LastUsedNumber);
            }

            if (Equal(export.Export1.Item.DetailDisbSuppressionStatusHistory.
              DiscontinueDate, local.MaxDate.Date))
            {
              export.Export1.Update.DetailDisbSuppressionStatusHistory.
                DiscontinueDate = local.InitialisedToZeros.Date;
            }

            export.Export1.Update.DetailCollectionType.Code =
              local.CollectionType.Text1;

            // if the read below fails the database has a problem and the read 
            // will abort
            if (ReadCsePerson1())
            {
              if (AsChar(entities.Display.Type1) == 'O')
              {
                export.Export1.Update.DetailPayee.FormattedName =
                  entities.Display.OrganizationName ?? Spaces(33);
              }
              else if (Equal(entities.Display.Number, local.HoldCsePerson.Number))
                
              {
                export.Export1.Update.DetailPayee.FormattedName =
                  local.HoldCsePersonsWorkSet.FormattedName;
              }
              else
              {
                UseCabReadAdabasPerson2();
                UseSiFormatCsePersonName4();
                local.HoldCsePerson.Number = entities.Display.Number;
                local.HoldCsePersonsWorkSet.FormattedName =
                  export.Export1.Item.DetailPayee.FormattedName;
              }
            }

            // end of read each disb suppression status history
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsEmpty(export.SearchPayee.Number))
          {
            // *******************************************************
            // PR#81986 - 2/21/2000
            // Call new CAB to sort by Name.
            // *******************************************************
            UseCabLpspSortGroupView();
          }

          if (export.Export1.IsFull)
          {
            ExitState = "FN0000_GROUP_VIEW_OVERFLOW";
          }
          else if (export.Export1.IsEmpty)
          {
            ExitState = "FN0000_NO_SUPPRESSIONS_FOUND";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

            export.Export1.Index = 0;
            export.Export1.CheckSize();

            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }
        }

        break;
      case "RETURN":
        if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "LIST":
        local.PromptCount.Count = 0;

        switch(AsChar(export.SuppressionTypePrompt.Text1))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.SuppressionTypePrompt, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.CollectionTypePrompt.Text1))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.CollectionTypePrompt, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.PayeeNumberPrompt.Text1))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PayeeNumberPrompt, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(local.PromptCount.Count)
        {
          case 0:
            var field1 = GetField(export.SuppressionTypePrompt, "text1");

            field1.Error = true;

            var field2 = GetField(export.CollectionTypePrompt, "text1");

            field2.Error = true;

            var field3 = GetField(export.PayeeNumberPrompt, "text1");

            field3.Error = true;

            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            return;
          case 1:
            break;
          default:
            if (!IsEmpty(import.SuppressionTypePrompt.Text1))
            {
              var field = GetField(export.SuppressionTypePrompt, "text1");

              field.Error = true;
            }

            if (!IsEmpty(import.CollectionTypePrompt.Text1))
            {
              var field = GetField(export.CollectionTypePrompt, "text1");

              field.Error = true;
            }

            if (!IsEmpty(import.PayeeNumberPrompt.Text1))
            {
              var field = GetField(export.PayeeNumberPrompt, "text1");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
        }

        if (!IsEmpty(import.PayeeNumberPrompt.Text1))
        {
          export.PayeeNumberPrompt.Text1 = "";
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
        }
        else if (!IsEmpty(import.CollectionTypePrompt.Text1))
        {
          export.CollectionTypePrompt.Text1 = "";
          ExitState = "ECO_LNK_LST_COLLECTION_TYPES";
        }
        else if (!IsEmpty(export.SuppressionTypePrompt.Text1))
        {
          export.SuppressionTypePrompt.Text1 = "";
          export.ToCdvl.CodeName = "SUPPRESSION TYPE";
          ExitState = "ECO_LNK_TO_CDVL";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "PSUP":
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsEmpty(export.SelectedCollectionType.Code))
          {
            ExitState = "ECO_XFR_TO_MTN_PERSON_DISB_SUPP";
          }
          else
          {
            ExitState = "FN0000_INVALID_FLOW_SUPPRESSION";
          }
        }

        break;
      case "CSUP":
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsEmpty(export.WorkerServiceProvider.UserId) && IsEmpty
            (export.SuppressingDisbSuppressionStatusHistory.CreatedBy) && IsEmpty
            (export.SearchPayee.Number) && IsEmpty
            (export.FilterCollectionType.Code))
          {
            ExitState = "ECO_XFR_TO_MTN_COLL_DISB_SUPPR";
          }
          else if (IsEmpty(export.SelectedCollectionType.Code))
          {
            ExitState = "FN0000_INVALID_FLOW_SUPPRESSION";
          }
          else
          {
            ExitState = "ECO_XFR_TO_MTN_COLL_DISB_SUPPR";
          }
        }

        break;
      case "LDSP":
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ECO_XFR_TO_LST_MTN_DISB_SUPP";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1ToImport1(Export.ExportGroup source,
    CabLpspSortGroupView.Import.ImportGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly but only head.");
      
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    MoveCsePersonsWorkSet(source.DetailPayee, target.DetailPayee);
    target.DetailDisbSuppressionStatusHistory.Assign(
      source.DetailDisbSuppressionStatusHistory);
    target.DetailCollectionType.Code = source.DetailCollectionType.Code;
  }

  private static void MoveImport1ToExport1(CabLpspSortGroupView.Import.
    ImportGroup source, Export.ExportGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    MoveCsePersonsWorkSet(source.DetailPayee, target.DetailPayee);
    target.DetailDisbSuppressionStatusHistory.Assign(
      source.DetailDisbSuppressionStatusHistory);
    target.DetailCollectionType.Code = source.DetailCollectionType.Code;
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

  private void UseCabLpspSortGroupView()
  {
    var useImport = new CabLpspSortGroupView.Import();
    var useExport = new CabLpspSortGroupView.Export();

    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport1);

    Call(CabLpspSortGroupView.Execute, useImport, useExport);

    System.Diagnostics.Trace.TraceWarning(
      "INFO: source has greater capacity than target.");
    useImport.Import1.CopyTo(export.Export1, MoveImport1ToExport1);
  }

  private void UseCabReadAdabasPerson1()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = export.SearchPayee.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabReadAdabasPerson2()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Export1.Item.DetailPayee.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.LeftPadding.Text10;
    useExport.TextWorkArea.Text10 = local.LeftPadding.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.LeftPadding.Text10 = useExport.TextWorkArea.Text10;
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

    useImport.CsePersonsWorkSet.Number = import.SearchPayee.Number;
    useImport.CsePerson.Number = export.SelectedCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName1()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.Starting.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName2()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.WorkerCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName3()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.SuppressingCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName4()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Export1.Update.DetailPayee.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSpCabDetOspAssgndToCsecase()
  {
    var useImport = new SpCabDetOspAssgndToCsecase.Import();
    var useExport = new SpCabDetOspAssgndToCsecase.Export();

    useImport.Case1.Number = entities.Case1.Number;

    Call(SpCabDetOspAssgndToCsecase.Execute, useImport, useExport);

    local.AssignedToOfficeServiceProvider.
      Assign(useExport.OfficeServiceProvider);
    local.Assigned.Assign(useExport.ServiceProvider);
    local.AssignedToOffice.Assign(useExport.Office);
  }

  private bool ReadCaseRoleCase()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligee2.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCollectionType1()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.FilterCollectionType.Code);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCollectionType2()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.DisbSuppressionStatusHistory.CltSequentialId.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadControlTable()
  {
    entities.UraSuppressionLength.Populated = false;

    return Read("ReadControlTable",
      null,
      (db, reader) =>
      {
        entities.UraSuppressionLength.Identifier = db.GetString(reader, 0);
        entities.UraSuppressionLength.LastUsedNumber = db.GetInt32(reader, 1);
        entities.UraSuppressionLength.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Display.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Obligee2.Number);
      },
      (db, reader) =>
      {
        entities.Display.Number = db.GetString(reader, 0);
        entities.Display.Type1 = db.GetString(reader, 1);
        entities.Display.OrganizationName = db.GetNullableString(reader, 2);
        entities.Display.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Display.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.StartingPayee.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SearchPayee.Number);
      },
      (db, reader) =>
      {
        entities.StartingPayee.Number = db.GetString(reader, 0);
        entities.StartingPayee.Type1 = db.GetString(reader, 1);
        entities.StartingPayee.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.StartingPayee.Populated = true;
        CheckValid<CsePerson>("Type1", entities.StartingPayee.Type1);
      });
  }

  private IEnumerable<bool> ReadDisbSuppressionStatusHistoryCsePerson1()
  {
    entities.Obligee2.Populated = false;
    entities.DisbSuppressionStatusHistory.Populated = false;

    return ReadEach("ReadDisbSuppressionStatusHistoryCsePerson1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.FromDate.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "discontinueDate",
          local.ToDate.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.Obligee2.Number = db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.Obligee2.Populated = true;
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbSuppressionStatusHistoryCsePerson2()
  {
    entities.Obligee2.Populated = false;
    entities.DisbSuppressionStatusHistory.Populated = false;

    return ReadEach("ReadDisbSuppressionStatusHistoryCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.StartingPayee.Number);
        db.SetDate(
          command, "effectiveDate",
          local.FromDate.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "discontinueDate",
          local.ToDate.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.Obligee2.Number = db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.Obligee2.Populated = true;
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.DisbSuppressionStatusHistory.LgaIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetString(
          command, "userId",
          export.SuppressingDisbSuppressionStatusHistory.CreatedBy);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "userId", export.WorkerServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailPayee.
      /// </summary>
      [JsonPropertyName("detailPayee")]
      public CsePersonsWorkSet DetailPayee
      {
        get => detailPayee ??= new();
        set => detailPayee = value;
      }

      /// <summary>
      /// A value of DetailDisbSuppressionStatusHistory.
      /// </summary>
      [JsonPropertyName("detailDisbSuppressionStatusHistory")]
      public DisbSuppressionStatusHistory DetailDisbSuppressionStatusHistory
      {
        get => detailDisbSuppressionStatusHistory ??= new();
        set => detailDisbSuppressionStatusHistory = value;
      }

      /// <summary>
      /// A value of DetailCollectionType.
      /// </summary>
      [JsonPropertyName("detailCollectionType")]
      public CollectionType DetailCollectionType
      {
        get => detailCollectionType ??= new();
        set => detailCollectionType = value;
      }

      /// <summary>
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 170;

      private Common detailCommon;
      private CsePersonsWorkSet detailPayee;
      private DisbSuppressionStatusHistory detailDisbSuppressionStatusHistory;
      private CollectionType detailCollectionType;
      private LegalAction detailLegalAction;
    }

    /// <summary>
    /// A value of ToDate.
    /// </summary>
    [JsonPropertyName("toDate")]
    public DisbSuppressionStatusHistory ToDate
    {
      get => toDate ??= new();
      set => toDate = value;
    }

    /// <summary>
    /// A value of FromDate.
    /// </summary>
    [JsonPropertyName("fromDate")]
    public DisbSuppressionStatusHistory FromDate
    {
      get => fromDate ??= new();
      set => fromDate = value;
    }

    /// <summary>
    /// A value of CollectionTypePrompt.
    /// </summary>
    [JsonPropertyName("collectionTypePrompt")]
    public TextWorkArea CollectionTypePrompt
    {
      get => collectionTypePrompt ??= new();
      set => collectionTypePrompt = value;
    }

    /// <summary>
    /// A value of PayeeNumberPrompt.
    /// </summary>
    [JsonPropertyName("payeeNumberPrompt")]
    public TextWorkArea PayeeNumberPrompt
    {
      get => payeeNumberPrompt ??= new();
      set => payeeNumberPrompt = value;
    }

    /// <summary>
    /// A value of FilterCollectionType.
    /// </summary>
    [JsonPropertyName("filterCollectionType")]
    public CollectionType FilterCollectionType
    {
      get => filterCollectionType ??= new();
      set => filterCollectionType = value;
    }

    /// <summary>
    /// A value of WorkerServiceProvider.
    /// </summary>
    [JsonPropertyName("workerServiceProvider")]
    public ServiceProvider WorkerServiceProvider
    {
      get => workerServiceProvider ??= new();
      set => workerServiceProvider = value;
    }

    /// <summary>
    /// A value of WorkerCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("workerCsePersonsWorkSet")]
    public CsePersonsWorkSet WorkerCsePersonsWorkSet
    {
      get => workerCsePersonsWorkSet ??= new();
      set => workerCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SuppressingDisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("suppressingDisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory SuppressingDisbSuppressionStatusHistory
    {
      get => suppressingDisbSuppressionStatusHistory ??= new();
      set => suppressingDisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of SuppressingCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("suppressingCsePersonsWorkSet")]
    public CsePersonsWorkSet SuppressingCsePersonsWorkSet
    {
      get => suppressingCsePersonsWorkSet ??= new();
      set => suppressingCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchPayee.
    /// </summary>
    [JsonPropertyName("searchPayee")]
    public CsePersonsWorkSet SearchPayee
    {
      get => searchPayee ??= new();
      set => searchPayee = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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
    /// A value of FilterDisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("filterDisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory FilterDisbSuppressionStatusHistory
    {
      get => filterDisbSuppressionStatusHistory ??= new();
      set => filterDisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of SuppressionTypePrompt.
    /// </summary>
    [JsonPropertyName("suppressionTypePrompt")]
    public TextWorkArea SuppressionTypePrompt
    {
      get => suppressionTypePrompt ??= new();
      set => suppressionTypePrompt = value;
    }

    /// <summary>
    /// A value of SuppressionType.
    /// </summary>
    [JsonPropertyName("suppressionType")]
    public CodeValue SuppressionType
    {
      get => suppressionType ??= new();
      set => suppressionType = value;
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

    private DisbSuppressionStatusHistory toDate;
    private DisbSuppressionStatusHistory fromDate;
    private TextWorkArea collectionTypePrompt;
    private TextWorkArea payeeNumberPrompt;
    private CollectionType filterCollectionType;
    private ServiceProvider workerServiceProvider;
    private CsePersonsWorkSet workerCsePersonsWorkSet;
    private DisbSuppressionStatusHistory suppressingDisbSuppressionStatusHistory;
      
    private CsePersonsWorkSet suppressingCsePersonsWorkSet;
    private CsePersonsWorkSet searchPayee;
    private Common showHistory;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private DisbSuppressionStatusHistory filterDisbSuppressionStatusHistory;
    private TextWorkArea suppressionTypePrompt;
    private CodeValue suppressionType;
    private CodeValue fromCdvl;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailPayee.
      /// </summary>
      [JsonPropertyName("detailPayee")]
      public CsePersonsWorkSet DetailPayee
      {
        get => detailPayee ??= new();
        set => detailPayee = value;
      }

      /// <summary>
      /// A value of DetailDisbSuppressionStatusHistory.
      /// </summary>
      [JsonPropertyName("detailDisbSuppressionStatusHistory")]
      public DisbSuppressionStatusHistory DetailDisbSuppressionStatusHistory
      {
        get => detailDisbSuppressionStatusHistory ??= new();
        set => detailDisbSuppressionStatusHistory = value;
      }

      /// <summary>
      /// A value of DetailCollectionType.
      /// </summary>
      [JsonPropertyName("detailCollectionType")]
      public CollectionType DetailCollectionType
      {
        get => detailCollectionType ??= new();
        set => detailCollectionType = value;
      }

      /// <summary>
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 170;

      private Common detailCommon;
      private CsePersonsWorkSet detailPayee;
      private DisbSuppressionStatusHistory detailDisbSuppressionStatusHistory;
      private CollectionType detailCollectionType;
      private LegalAction detailLegalAction;
    }

    /// <summary>
    /// A value of ToDate.
    /// </summary>
    [JsonPropertyName("toDate")]
    public DisbSuppressionStatusHistory ToDate
    {
      get => toDate ??= new();
      set => toDate = value;
    }

    /// <summary>
    /// A value of FromDate.
    /// </summary>
    [JsonPropertyName("fromDate")]
    public DisbSuppressionStatusHistory FromDate
    {
      get => fromDate ??= new();
      set => fromDate = value;
    }

    /// <summary>
    /// A value of CollectionTypePrompt.
    /// </summary>
    [JsonPropertyName("collectionTypePrompt")]
    public TextWorkArea CollectionTypePrompt
    {
      get => collectionTypePrompt ??= new();
      set => collectionTypePrompt = value;
    }

    /// <summary>
    /// A value of PayeeNumberPrompt.
    /// </summary>
    [JsonPropertyName("payeeNumberPrompt")]
    public TextWorkArea PayeeNumberPrompt
    {
      get => payeeNumberPrompt ??= new();
      set => payeeNumberPrompt = value;
    }

    /// <summary>
    /// A value of FilterCollectionType.
    /// </summary>
    [JsonPropertyName("filterCollectionType")]
    public CollectionType FilterCollectionType
    {
      get => filterCollectionType ??= new();
      set => filterCollectionType = value;
    }

    /// <summary>
    /// A value of WorkerServiceProvider.
    /// </summary>
    [JsonPropertyName("workerServiceProvider")]
    public ServiceProvider WorkerServiceProvider
    {
      get => workerServiceProvider ??= new();
      set => workerServiceProvider = value;
    }

    /// <summary>
    /// A value of WorkerCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("workerCsePersonsWorkSet")]
    public CsePersonsWorkSet WorkerCsePersonsWorkSet
    {
      get => workerCsePersonsWorkSet ??= new();
      set => workerCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SuppressingDisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("suppressingDisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory SuppressingDisbSuppressionStatusHistory
    {
      get => suppressingDisbSuppressionStatusHistory ??= new();
      set => suppressingDisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of SuppressingCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("suppressingCsePersonsWorkSet")]
    public CsePersonsWorkSet SuppressingCsePersonsWorkSet
    {
      get => suppressingCsePersonsWorkSet ??= new();
      set => suppressingCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchPayee.
    /// </summary>
    [JsonPropertyName("searchPayee")]
    public CsePersonsWorkSet SearchPayee
    {
      get => searchPayee ??= new();
      set => searchPayee = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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
    /// A value of SelectedDisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("selectedDisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory SelectedDisbSuppressionStatusHistory
    {
      get => selectedDisbSuppressionStatusHistory ??= new();
      set => selectedDisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of SelectedCollectionType.
    /// </summary>
    [JsonPropertyName("selectedCollectionType")]
    public CollectionType SelectedCollectionType
    {
      get => selectedCollectionType ??= new();
      set => selectedCollectionType = value;
    }

    /// <summary>
    /// A value of SelectedPayee.
    /// </summary>
    [JsonPropertyName("selectedPayee")]
    public CsePersonsWorkSet SelectedPayee
    {
      get => selectedPayee ??= new();
      set => selectedPayee = value;
    }

    /// <summary>
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
    }

    /// <summary>
    /// A value of SelectedLegalAction.
    /// </summary>
    [JsonPropertyName("selectedLegalAction")]
    public LegalAction SelectedLegalAction
    {
      get => selectedLegalAction ??= new();
      set => selectedLegalAction = value;
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
    /// A value of FilterDisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("filterDisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory FilterDisbSuppressionStatusHistory
    {
      get => filterDisbSuppressionStatusHistory ??= new();
      set => filterDisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of SuppressionTypePrompt.
    /// </summary>
    [JsonPropertyName("suppressionTypePrompt")]
    public TextWorkArea SuppressionTypePrompt
    {
      get => suppressionTypePrompt ??= new();
      set => suppressionTypePrompt = value;
    }

    /// <summary>
    /// A value of SuppressionType.
    /// </summary>
    [JsonPropertyName("suppressionType")]
    public CodeValue SuppressionType
    {
      get => suppressionType ??= new();
      set => suppressionType = value;
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

    private DisbSuppressionStatusHistory toDate;
    private DisbSuppressionStatusHistory fromDate;
    private TextWorkArea collectionTypePrompt;
    private TextWorkArea payeeNumberPrompt;
    private CollectionType filterCollectionType;
    private ServiceProvider workerServiceProvider;
    private CsePersonsWorkSet workerCsePersonsWorkSet;
    private DisbSuppressionStatusHistory suppressingDisbSuppressionStatusHistory;
      
    private CsePersonsWorkSet suppressingCsePersonsWorkSet;
    private CsePersonsWorkSet searchPayee;
    private Common showHistory;
    private Array<ExportGroup> export1;
    private DisbSuppressionStatusHistory selectedDisbSuppressionStatusHistory;
    private CollectionType selectedCollectionType;
    private CsePersonsWorkSet selectedPayee;
    private CsePerson selectedCsePerson;
    private LegalAction selectedLegalAction;
    private NextTranInfo hidden;
    private Standard standard;
    private DisbSuppressionStatusHistory filterDisbSuppressionStatusHistory;
    private TextWorkArea suppressionTypePrompt;
    private CodeValue suppressionType;
    private Code toCdvl;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
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
    /// A value of HoldCsePerson.
    /// </summary>
    [JsonPropertyName("holdCsePerson")]
    public CsePerson HoldCsePerson
    {
      get => holdCsePerson ??= new();
      set => holdCsePerson = value;
    }

    /// <summary>
    /// A value of HoldCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("holdCsePersonsWorkSet")]
    public CsePersonsWorkSet HoldCsePersonsWorkSet
    {
      get => holdCsePersonsWorkSet ??= new();
      set => holdCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ToDate.
    /// </summary>
    [JsonPropertyName("toDate")]
    public DisbSuppressionStatusHistory ToDate
    {
      get => toDate ??= new();
      set => toDate = value;
    }

    /// <summary>
    /// A value of FromDate.
    /// </summary>
    [JsonPropertyName("fromDate")]
    public DisbSuppressionStatusHistory FromDate
    {
      get => fromDate ??= new();
      set => fromDate = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public TextWorkArea CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CsePersonsWorkSet Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of NumRecordsSelected.
    /// </summary>
    [JsonPropertyName("numRecordsSelected")]
    public Common NumRecordsSelected
    {
      get => numRecordsSelected ??= new();
      set => numRecordsSelected = value;
    }

    /// <summary>
    /// A value of AssignedToOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("assignedToOfficeServiceProvider")]
    public OfficeServiceProvider AssignedToOfficeServiceProvider
    {
      get => assignedToOfficeServiceProvider ??= new();
      set => assignedToOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of Assigned.
    /// </summary>
    [JsonPropertyName("assigned")]
    public ServiceProvider Assigned
    {
      get => assigned ??= new();
      set => assigned = value;
    }

    /// <summary>
    /// A value of AssignedToOffice.
    /// </summary>
    [JsonPropertyName("assignedToOffice")]
    public Office AssignedToOffice
    {
      get => assignedToOffice ??= new();
      set => assignedToOffice = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private Common promptCount;
    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private LegalAction legalAction;
    private CsePerson holdCsePerson;
    private CsePersonsWorkSet holdCsePersonsWorkSet;
    private DisbSuppressionStatusHistory toDate;
    private DisbSuppressionStatusHistory fromDate;
    private TextWorkArea collectionType;
    private DateWorkArea current;
    private CsePersonsWorkSet starting;
    private Common numRecordsSelected;
    private OfficeServiceProvider assignedToOfficeServiceProvider;
    private ServiceProvider assigned;
    private Office assignedToOffice;
    private DateWorkArea initialisedToZeros;
    private DateWorkArea maxDate;
    private TextWorkArea leftPadding;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common common;
    private EabReportSend eabReportSend;
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
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public CsePerson Display
    {
      get => display ??= new();
      set => display = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of StartingPayee.
    /// </summary>
    [JsonPropertyName("startingPayee")]
    public CsePerson StartingPayee
    {
      get => startingPayee ??= new();
      set => startingPayee = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of UraSuppressionLength.
    /// </summary>
    [JsonPropertyName("uraSuppressionLength")]
    public ControlTable UraSuppressionLength
    {
      get => uraSuppressionLength ??= new();
      set => uraSuppressionLength = value;
    }

    private LegalAction legalAction;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson display;
    private CollectionType collectionType;
    private CsePerson startingPayee;
    private CsePersonAccount csePersonAccount;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private CsePersonAccount obligee1;
    private CsePerson obligee2;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private ControlTable uraSuppressionLength;
  }
#endregion
}
