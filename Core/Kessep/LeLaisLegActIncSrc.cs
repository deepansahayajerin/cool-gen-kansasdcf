// Program: LE_LAIS_LEG_ACT_INC_SRC, ID: 1902468471, model: 746.
// Short name: SWELAISP
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
/// A program: LE_LAIS_LEG_ACT_INC_SRC.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLaisLegActIncSrc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LAIS_LEG_ACT_INC_SRC program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLaisLegActIncSrc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLaisLegActIncSrc.
  /// </summary>
  public LeLaisLegActIncSrc(IContext context, Import import, Export export):
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
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 06/09/15  GVandy	CQ22212		Initial Code.  Created from a copy of INCL.
    // 11/23/15  GVandy	CQ50406		Break read apart in RETINCL logic to support
    // 					income source where there is no associated
    // 					employer (type O and R).
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.LactActionTaken.Description = import.LactActionTaken.Description;
    export.Case1.Number = import.Case1.Number;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // -- Move import group to export group.  This is done after the CLEAR 
    // processing so
    //    that the group will be cleared when the user presses the CLEAR 
    // function key.
    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.Gcommon.SelectChar =
        import.Import1.Item.Gcommon.SelectChar;
      MoveIncomeSource(import.Import1.Item.GincomeSource,
        export.Export1.Update.GincomeSource);
      export.Export1.Update.GiwoAction.Assign(import.Import1.Item.GiwoAction);
      MoveIwoTransaction(import.Import1.Item.GiwoTransaction,
        export.Export1.Update.GiwoTransaction);
      export.Export1.Update.GexportEiwo.Flag =
        import.Import1.Item.GimportEiwo.Flag;
    }

    import.Import1.CheckIndex();

    // -- Establish eiwo aging cutoff date.
    if (ReadCodeValue())
    {
      local.EiwoAgingCutoffDate.Date =
        Now().Date.AddDays((int)(-StringToNumber(entities.CodeValue.Cdvalue)));
    }
    else
    {
      ExitState = "LE_EIWO_AGING_DAYS_CODE_TABLE_NF";

      return;
    }

    // -- Set display colors.  (The status of one or more group entries may have
    // changed).
    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      if (AsChar(export.Export1.Item.GiwoAction.SeverityClearedInd) == 'Y')
      {
        // -- The severity has been cleared on these IWO actions.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Protected = true;
      }
      else if (IsEmpty(export.Export1.Item.GiwoAction.ActionType))
      {
        // -- These are selections returned from INCL.  No action has been taken
        // yet.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Protected = true;
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'S')
      {
        // -- These are submitted eIWO actions.  Set color based on the aging 
        // cutoff date.
        if (!Lt(local.EiwoAgingCutoffDate.Date,
          export.Export1.Item.GiwoAction.StatusDate))
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Color = "red";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Color = "red";
          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Color = "red";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Color = "red";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Color = "red";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "red";
          field6.Protected = true;
        }
        else
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Protected = true;
        }
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'N' || AsChar
        (export.Export1.Item.GiwoAction.StatusCd) == 'R')
      {
        // -- These are sent and received eIWO actions.  Set color based on the 
        // aging cutoff date.
        if (!Lt(local.EiwoAgingCutoffDate.Date,
          export.Export1.Item.GiwoAction.StatusDate))
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Color = "red";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Color = "red";
          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Color = "red";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Color = "red";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Color = "red";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "red";
          field6.Protected = true;
        }
        else
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Color = "yellow";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Color = "yellow";
          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Color = "yellow";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Color = "yellow";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Color = "yellow";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "yellow";
          field6.Protected = true;
        }
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'E')
      {
        // -- These are errored eIWO actions.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Color = "red";
        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Color = "red";
        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Color = "red";
        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Color = "red";
        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Color = "red";
        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Color = "red";
        field6.Protected = true;
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'J')
      {
        // -- These are sent and rejected eIWO actions.  Set color based on the 
        // status reason code.
        if (Equal(export.Export1.Item.GiwoAction.StatusReasonCode, "N") || Equal
          (export.Export1.Item.GiwoAction.StatusReasonCode, "U"))
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Protected = true;
        }
        else
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Color = "red";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Color = "red";
          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Color = "red";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Color = "red";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Color = "red";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "red";
          field6.Protected = true;
        }
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'Q')
      {
        // -- These are queued print requests.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Color = "red";
        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Color = "red";
        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Color = "red";
        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Color = "red";
        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Color = "red";
        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Color = "red";
        field6.Protected = true;
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'P')
      {
        // -- These are printed print requests.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Protected = true;
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'C')
      {
        // -- These are cancelled print requests and eIWO actions.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Protected = true;
      }
    }

    export.Export1.CheckIndex();

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumber = export.CsePersonsWorkSet.Number;
      export.Hidden.LegalActionIdentifier = export.LegalAction.Identifier;
      export.Hidden.StandardCrtOrdNumber =
        export.LegalAction.StandardNumber ?? "";
      export.Hidden.CourtCaseNumber = export.LegalAction.CourtCaseNumber ?? "";
      UseScCabNextTranPut1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.CsePersonsWorkSet.Number = "";
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "EIWOSUB") || Equal(global.Command, "PRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    // ---------------------------------------------
    // Check how many selections have been made.
    // ---------------------------------------------
    local.Common.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Export1.Item.Gcommon.SelectChar))
      {
        case 'S':
          ++local.Common.Count;

          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
      }
    }

    export.Export1.CheckIndex();

    if (local.Common.Count == 0 && (Equal(global.Command, "EIWOSUB") || Equal
      (global.Command, "CANCEL") || Equal(global.Command, "EIWH")))
    {
      ExitState = "ACO_NE0000_NO_SELECTION_MADE";

      return;
    }

    if (local.Common.Count > 1 && Equal(global.Command, "EIWH"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
        {
          var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

          field1.Error = true;
        }
      }

      export.Export1.CheckIndex();
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        return;
      case "RETURN":
        // -- RETURN is done via a transfer dialog flow since a link dialog flow
        // is broken on the nextran for PRINTing.
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "DISPLAY":
        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        UseLeLaisDisplayIwoTrans();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (export.Export1.Index == -1)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "INCL":
        // -- Set flag for INCL so it knows to enable multiple selections.
        export.ToIncl.Flag = "Y";
        ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";

        return;
      case "RETINCL":
        export.Export1.Index = -1;
        export.Export1.Count = 0;

        // -- Put the returned selections at the top of the group view.
        for(import.SelectedFromIncl.Index = 0; import.SelectedFromIncl.Index < import
          .SelectedFromIncl.Count; ++import.SelectedFromIncl.Index)
        {
          if (!import.SelectedFromIncl.CheckSize())
          {
            break;
          }

          if (ReadIncomeSource())
          {
            ++export.Export1.Index;
            export.Export1.CheckSize();

            MoveIncomeSource(entities.IncomeSource,
              export.Export1.Update.GincomeSource);
            MoveIwoTransaction(local.NullIwoTransaction,
              export.Export1.Update.GiwoTransaction);
            export.Export1.Update.GiwoAction.Assign(local.NullIwoAction);
            export.Export1.Update.Gcommon.SelectChar = "S";

            if (ReadEmployer())
            {
              if (!Lt(Now().Date, entities.Employer.EiwoStartDate) && !
                Lt(entities.Employer.EiwoEndDate, Now().Date))
              {
                export.Export1.Update.GexportEiwo.Flag = "Y";
              }
              else
              {
                export.Export1.Update.GexportEiwo.Flag = "N";
              }
            }
            else
            {
              export.Export1.Update.GexportEiwo.Flag = "N";
            }
          }
        }

        import.SelectedFromIncl.CheckIndex();

        // -- Put all existing group view entries after the returned selections.
        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          MoveIncomeSource(import.Import1.Item.GincomeSource,
            export.Export1.Update.GincomeSource);
          export.Export1.Update.GiwoAction.
            Assign(import.Import1.Item.GiwoAction);
          MoveIwoTransaction(import.Import1.Item.GiwoTransaction,
            export.Export1.Update.GiwoTransaction);
          export.Export1.Update.GexportEiwo.Flag =
            import.Import1.Item.GimportEiwo.Flag;
        }

        import.Import1.CheckIndex();

        break;
      case "EIWOSUB":
        // -- Verify that the payor has an SSN.
        if (IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          var field1 = GetField(export.CsePersonsWorkSet, "ssn");

          field1.Error = true;

          ExitState = "LE0000_EIWO_NO_SSN";

          return;
        }

        // -- Cannot select a row with an action type.  Must select a freshly 
        // returned income source from INCL.
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            if (!IsEmpty(export.Export1.Item.GiwoAction.ActionType))
            {
              var field1 = GetField(export.Export1.Item.GincomeSource, "name");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

              field2.Error = true;

              var field3 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field3.Error = true;

              ExitState = "LE0000_MUST_PROMPT_TO_INCL";
            }
          }
        }

        export.Export1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -- For non Kansas tribunals, insure there is a SDU location 
        // description and address
        //    for the state where the tribunal resides.
        if (ReadFips1())
        {
          if (entities.Fips.State == 20)
          {
            goto Read;
          }

          if (ReadFips2())
          {
            if (IsEmpty(entities.Sdu.LocationDescription))
            {
              ExitState = "LE0000_EIWO_FIPS_LOC_DESC_NF";

              return;
            }

            if (!ReadFipsTribAddress())
            {
              ExitState = "LE0000_EIWO_SDU_FIPS_ADDRESS_NF";

              return;
            }
          }
          else
          {
            ExitState = "LE0000_SDU_FIPS_NOT_FOUND";

            return;
          }
        }
        else
        {
          ExitState = "LE0000_EIWO_NON_DOMESTIC_TRIBUNL";

          return;
        }

Read:

        // -- IWO and IWOMODO must have a filed date.
        if ((Equal(export.LegalAction.ActionTaken, "IWO") || Equal
          (export.LegalAction.ActionTaken, "IWOMODO")) && Equal
          (export.LegalAction.FiledDate, local.NullDateWorkArea.Date))
        {
          var field1 = GetField(export.LegalAction, "filedDate");

          field1.Error = true;

          ExitState = "LE0000_EIWO_NO_FILED_DATE";

          return;
        }

        // -- Verify that selected employers are eIWO participants.
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            if (AsChar(export.Export1.Item.GexportEiwo.Flag) != 'Y')
            {
              var field1 = GetField(export.Export1.Item.GincomeSource, "name");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

              field2.Error = true;

              var field3 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field3.Error = true;

              ExitState = "LE0000_EIWO_EMP_NOT_EIWO";
            }
          }
        }

        export.Export1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -- There cannot be another non ORDIWOLS eIWO in submitted status.
        if (!Equal(export.LegalAction.ActionTaken, "ORDIWOLS"))
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
            {
              if (ReadIncomeSourceEmployer())
              {
                local.Employer.Ein = entities.Employer.Ein;
              }
              else
              {
                var field1 =
                  GetField(export.Export1.Item.GincomeSource, "name");

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field2.Error = true;

                ExitState = "INCOME_SOURCE_NF";

                return;
              }

              if (ReadLegalAction())
              {
                var field1 =
                  GetField(export.Export1.Item.GincomeSource, "name");

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field2.Error = true;

                ExitState = "LE0000_EIWO_ALREADY_IN_S_STATUS";

                return;
              }
              else
              {
                // -- Continue
              }
            }
          }

          export.Export1.CheckIndex();
        }

        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            // -- Create the IWGL record if one does not already exist.
            local.IwglInd.Text1 = "I";
            local.LegalActionIncomeSource.EffectiveDate = Now().Date;
            local.LegalActionIncomeSource.OrderType = "";
            local.LegalActionIncomeSource.WithholdingType = "W";
            UseEstabLegalActionIncSource();
            ExitState = "ACO_NN0000_ALL_OK";

            // -- Create the eIWO document trigger.
            local.SpDocKey.KeyAp = export.CsePersonsWorkSet.Number;
            local.SpDocKey.KeyCase = export.Case1.Number;
            local.SpDocKey.KeyIncomeSource =
              export.Export1.Item.GincomeSource.Identifier;
            local.SpDocKey.KeyLegalAction = export.LegalAction.Identifier;
            local.Document.Name = export.LegalAction.ActionTaken;
            UseSpCreateDocumentInfrastruct();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field7 = GetField(export.Export1.Item.GincomeSource, "name");

              field7.Error = true;

              var field8 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field8.Error = true;

              return;
            }

            // -- Set field indicating this document is being delivered 
            // electronically.
            local.Field.Name = "LAEIWO2EMP";
            local.FieldValue.Value = "Y";
            UseSpCabCreateUpdateFieldValue();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field7 = GetField(export.Export1.Item.GincomeSource, "name");

              field7.Error = true;

              var field8 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field8.Error = true;

              return;
            }

            // -- Create the eIWO.
            local.IwoTransaction.TransactionNumber = "";
            local.IwoAction.ActionType = "E-IWO";
            local.IwoAction.StatusCd = "S";
            local.CsePerson.Number = export.CsePersonsWorkSet.Number;
            local.OutgoingDocument.PrintSucessfulIndicator = "D";
            UseLeCreateIwoTransaction();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field7 = GetField(export.Export1.Item.GincomeSource, "name");

              field7.Error = true;

              var field8 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field8.Error = true;

              return;
            }

            export.Export1.Update.Gcommon.SelectChar = "";

            var field1 = GetField(export.Export1.Item.GincomeSource, "name");

            field1.Protected = true;

            var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

            field2.Protected = true;

            var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

            field3.Protected = true;

            var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");
              

            field5.Protected = true;

            var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

            field6.Protected = true;
          }
        }

        export.Export1.CheckIndex();
        ExitState = "LE0000_EIWO_SUBMITTED";

        break;
      case "CANCEL":
        // -- Verify that selected employers are eIWO participants.
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            if (AsChar(export.Export1.Item.GiwoAction.StatusCd) != 'S' && AsChar
              (export.Export1.Item.GiwoAction.StatusCd) != 'Q')
            {
              var field7 = GetField(export.Export1.Item.GiwoAction, "statusCd");

              field7.Error = true;

              ExitState = "LE0000_INVALID_STATUS_FOR_CANCEL";

              return;
            }

            // -- Cancel the eIWO.
            local.CsePerson.Number = export.CsePersonsWorkSet.Number;
            local.IwoAction.Identifier =
              export.Export1.Item.GiwoAction.Identifier;
            local.IwoAction.StatusCd = "C";
            UseLeUpdateIwoActionStatus();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            var field1 = GetField(export.Export1.Item.GincomeSource, "name");

            field1.Protected = true;

            var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

            field2.Protected = true;

            var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

            field3.Protected = true;

            var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");
              

            field5.Protected = true;

            var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

            field6.Protected = true;

            // -- Cancel the eIWO document trigger.
            if (ReadOutgoingDocumentInfrastructure())
            {
              local.OutgoingDocument.PrintSucessfulIndicator = "C";
              UseUpdateOutgoingDocument();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }
            else if (Equal(export.Export1.Item.GiwoAction.ActionType, "PRINT"))
            {
            }
            else
            {
              ExitState = "OUTGOING_DOCUMENT_NF";

              return;
            }

            export.Export1.Update.Gcommon.SelectChar = "";
          }
        }

        export.Export1.CheckIndex();
        ExitState = "LE0000_EIWO_CANCELLED";

        break;
      case "EIWH":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            if (!Equal(export.Export1.Item.GiwoAction.ActionType, "E-IWO") && !
              Equal(export.Export1.Item.GiwoAction.ActionType, "RESUB"))
            {
              var field1 =
                GetField(export.Export1.Item.GiwoAction, "actionType");

              field1.Error = true;

              ExitState = "LE0000_EIWO_INVALID_EIWH_ACTION";

              return;
            }

            MoveIwoTransaction(export.Export1.Item.GiwoTransaction,
              export.ToEiwhIwoTransaction);
            MoveIncomeSource(export.Export1.Item.GincomeSource,
              export.ToEiwhIncomeSource);
            export.Export1.Update.Gcommon.SelectChar = "";
            ExitState = "ECO_LNK_TO_EIWH";
          }
        }

        export.Export1.CheckIndex();

        break;
      case "RETEIWH":
        // -- Refresh the display in case the user cleared the severity.
        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        UseLeLaisDisplayIwoTrans();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }

        break;
      case "PRINT":
        // -- Print confirmation is required if the selected employer(s) are 
        // eIWO participants.
        if (AsChar(import.PrintConfirmation.Flag) != 'Y')
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
            {
              if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'Q')
              {
                continue;
              }

              if (AsChar(export.Export1.Item.GexportEiwo.Flag) == 'Y')
              {
                var field1 =
                  GetField(export.Export1.Item.GincomeSource, "name");

                field1.Error = true;

                var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

                field2.Error = true;

                var field3 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field3.Error = true;

                export.PrintConfirmation.Flag = "Y";
                ExitState = "LE0000_EMPLOYER_IS_EIWO_CONFIRM";
              }
            }
          }

          export.Export1.CheckIndex();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        if (local.Common.Count == 0)
        {
          export.Export1.Index = export.Export1.Count;
          export.Export1.CheckSize();

          // -- No employer was selected.  Create the Print action for a blank 
          // document.
          local.IwoTransaction.TransactionNumber = "";
          local.IwoAction.ActionType = "PRINT";
          local.IwoAction.StatusCd = "Q";
          local.CsePerson.Number = export.CsePersonsWorkSet.Number;
          local.OutgoingDocument.PrintSucessfulIndicator = "";
          UseLeCreateIwoTransaction();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field7 = GetField(export.Export1.Item.GincomeSource, "name");

            field7.Error = true;

            var field8 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field8.Error = true;

            return;
          }

          export.Export1.Update.Gcommon.SelectChar = "S";

          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Color = "red";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Color = "red";
          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Color = "red";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Color = "red";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Color = "red";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "red";
          field6.Protected = true;
        }
        else
        {
          // -- Create an IWO action for each selected income source and place 
          // in "Q" status.
          export.Export1.Index = 0;

          for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
            export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
            {
              if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'Q')
              {
                continue;
              }

              // -- Create the IWGL record if one does not already exist.
              local.IwglInd.Text1 = "I";
              local.LegalActionIncomeSource.EffectiveDate = Now().Date;
              local.LegalActionIncomeSource.OrderType = "";
              local.LegalActionIncomeSource.WithholdingType = "W";
              UseEstabLegalActionIncSource();
              ExitState = "ACO_NN0000_ALL_OK";

              // -- Create the Print action.
              local.IwoTransaction.TransactionNumber = "";
              local.IwoAction.ActionType = "PRINT";
              local.IwoAction.StatusCd = "Q";
              local.CsePerson.Number = export.CsePersonsWorkSet.Number;
              local.OutgoingDocument.PrintSucessfulIndicator = "";
              UseLeCreateIwoTransaction();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field7 =
                  GetField(export.Export1.Item.GincomeSource, "name");

                field7.Error = true;

                var field8 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field8.Error = true;

                return;
              }

              var field1 = GetField(export.Export1.Item.GincomeSource, "name");

              field1.Color = "red";
              field1.Protected = true;

              var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

              field2.Color = "red";
              field2.Protected = true;

              var field3 =
                GetField(export.Export1.Item.GiwoAction, "actionType");

              field3.Color = "red";
              field3.Protected = true;

              var field4 =
                GetField(export.Export1.Item.GiwoAction, "statusDate");

              field4.Color = "red";
              field4.Protected = true;

              var field5 =
                GetField(export.Export1.Item.GiwoTransaction,
                "transactionNumber");

              field5.Color = "red";
              field5.Protected = true;

              var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

              field6.Color = "red";
              field6.Protected = true;
            }
          }

          export.Export1.CheckIndex();
        }

        // -- Find the first Queued Print request and initiate the flow to DKEY/
        // DDOC.
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            export.Hidden.Assign(local.NullNextTranInfo);
            ReadDocument();

            if (!entities.Document.Populated)
            {
              ExitState = "SP0000_LACT_HAS_NO_DOCUMENT";

              return;
            }

            // -- Set the IWO action status to "P" signifying the document was 
            // printed.
            //    (If errors are encountered during printing the status will be 
            // changed to
            //     Error in the PrintRet section.)
            local.IwoAction.Assign(export.Export1.Item.GiwoAction);
            local.IwoAction.StatusCd = "P";
            local.CsePerson.Number = export.CsePersonsWorkSet.Number;
            UseLeUpdateIwoActionStatus();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            var field1 = GetField(export.Export1.Item.GincomeSource, "name");

            field1.Protected = true;

            var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

            field2.Protected = true;

            var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

            field3.Protected = true;

            var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");
              

            field5.Protected = true;

            var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

            field6.Protected = true;

            export.Standard.NextTransaction = "DKEY";
            export.Hidden.InfrastructureId = 0;
            export.Hidden.LegalActionIdentifier = export.LegalAction.Identifier;
            export.Hidden.CourtCaseNumber =
              export.LegalAction.CourtCaseNumber ?? "";
            export.Hidden.StandardCrtOrdNumber =
              export.LegalAction.StandardNumber ?? "";
            export.Hidden.CsePersonNumberAp = export.CsePersonsWorkSet.Number;
            export.Hidden.CaseNumber = export.Case1.Number;
            export.Hidden.MiscText2 = "PRINT:" + entities.Document.Name;

            if (!Equal(export.Export1.Item.GincomeSource.Identifier,
              local.NullDateWorkArea.Timestamp))
            {
              local.BatchTimestampWorkArea.IefTimestamp =
                export.Export1.Item.GincomeSource.Identifier;
              UseLeCabConvertTimestamp();
              export.Hidden.MiscText1 = "INCS:" + local
                .BatchTimestampWorkArea.TextTimestamp;
            }

            local.PrintProcess.Flag = "Y";
            local.PrintProcess.Command = "PRINT";
            UseScCabNextTranPut2();

            return;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "PRINTRET":
        UseScCabNextTranGet();
        export.LegalAction.Identifier =
          export.Hidden.LegalActionIdentifier.GetValueOrDefault();
        export.LegalAction.StandardNumber =
          export.Hidden.StandardCrtOrdNumber ?? "";
        export.CsePersonsWorkSet.Number = export.Hidden.CsePersonNumberAp ?? Spaces
          (10);
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
        local.Infrastructure.SystemGeneratedIdentifier =
          export.Hidden.InfrastructureId.GetValueOrDefault();

        // -- Tie the IWO Action to the Outgoing Document.
        if (local.Infrastructure.SystemGeneratedIdentifier > 0)
        {
          if (!ReadOutgoingDocument())
          {
            goto Test;
          }

          if (ReadIwoAction())
          {
            AssociateIwoAction();
          }
        }

Test:

        UseLeLaisDisplayIwoTrans();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        local.Position.Count = Find(export.Hidden.MiscText2, "PRINT:");

        if (local.Position.Count <= 0)
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else
        {
          // ----------------------------------------------------------
          // mjr---> Determines the appropriate exitstate for the Print
          // process
          // ------------------------------------------------------------
          local.PrintRetCode.Text50 = export.Hidden.MiscText2 ?? Spaces(50);
          UseSpPrintDecodeReturnCode();
          export.Hidden.MiscText2 = local.PrintRetCode.Text50;

          if (!IsExitState("SP0000_DOWNLOAD_SUCCESSFUL"))
          {
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'Q')
          {
            export.Export1.Update.Gcommon.SelectChar = "S";
          }
        }

        export.Export1.CheckIndex();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // -- Set display colors.  (The status of one or more group entries may have
    // changed).
    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      if (AsChar(export.Export1.Item.GiwoAction.SeverityClearedInd) == 'Y')
      {
        // -- The severity has been cleared on these IWO actions.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Protected = true;
      }
      else if (IsEmpty(export.Export1.Item.GiwoAction.ActionType))
      {
        // -- These are selections returned from INCL.  No action has been taken
        // yet.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Protected = true;
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'S')
      {
        // -- These are submitted eIWO actions.  Set color based on the aging 
        // cutoff date.
        if (!Lt(local.EiwoAgingCutoffDate.Date,
          export.Export1.Item.GiwoAction.StatusDate))
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Color = "red";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Color = "red";
          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Color = "red";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Color = "red";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Color = "red";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "red";
          field6.Protected = true;
        }
        else
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Protected = true;
        }
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'N' || AsChar
        (export.Export1.Item.GiwoAction.StatusCd) == 'R')
      {
        // -- These are sent and received eIWO actions.  Set color based on the 
        // aging cutoff date.
        if (!Lt(local.EiwoAgingCutoffDate.Date,
          export.Export1.Item.GiwoAction.StatusDate))
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Color = "red";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Color = "red";
          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Color = "red";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Color = "red";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Color = "red";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "red";
          field6.Protected = true;
        }
        else
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Color = "yellow";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Color = "yellow";
          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Color = "yellow";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Color = "yellow";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Color = "yellow";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "yellow";
          field6.Protected = true;
        }
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'E')
      {
        // -- These are errored eIWO actions.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Color = "red";
        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Color = "red";
        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Color = "red";
        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Color = "red";
        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Color = "red";
        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Color = "red";
        field6.Protected = true;
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'J')
      {
        // -- These are sent and rejected eIWO actions.  Set color based on the 
        // status reason code.
        if (Equal(export.Export1.Item.GiwoAction.StatusReasonCode, "N") || Equal
          (export.Export1.Item.GiwoAction.StatusReasonCode, "U"))
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Protected = true;
        }
        else
        {
          var field1 = GetField(export.Export1.Item.GincomeSource, "name");

          field1.Color = "red";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

          field2.Color = "red";
          field2.Protected = true;

          var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

          field3.Color = "red";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field4.Color = "red";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field5.Color = "red";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "red";
          field6.Protected = true;
        }
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'Q')
      {
        // -- These are queued print requests.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Color = "red";
        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Color = "red";
        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Color = "red";
        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Color = "red";
        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Color = "red";
        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Color = "red";
        field6.Protected = true;
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'P')
      {
        // -- These are printed print requests.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Protected = true;
      }
      else if (AsChar(export.Export1.Item.GiwoAction.StatusCd) == 'C')
      {
        // -- These are cancelled print requests and eIWO actions.
        var field1 = GetField(export.Export1.Item.GincomeSource, "name");

        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.GexportEiwo, "flag");

        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.GiwoAction, "actionType");

        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.GiwoAction, "statusDate");

        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

        field6.Protected = true;
      }
    }

    export.Export1.CheckIndex();
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveExport1(LeLaisDisplayIwoTrans.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Gcommon.SelectChar = source.Gcommon.SelectChar;
    MoveIncomeSource(source.GincomeSource, target.GincomeSource);
    target.GexportEiwo.Flag = source.GexportEiwo.Flag;
    MoveIwoTransaction(source.GiwoTransaction, target.GiwoTransaction);
    target.GiwoAction.Assign(source.GiwoAction);
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveIwoAction1(IwoAction source, IwoAction target)
  {
    target.Identifier = source.Identifier;
    target.ActionType = source.ActionType;
    target.StatusCd = source.StatusCd;
    target.StatusDate = source.StatusDate;
    target.StatusReasonCode = source.StatusReasonCode;
    target.SeverityClearedInd = source.SeverityClearedInd;
  }

  private static void MoveIwoAction2(IwoAction source, IwoAction target)
  {
    target.Identifier = source.Identifier;
    target.StatusCd = source.StatusCd;
  }

  private static void MoveIwoAction3(IwoAction source, IwoAction target)
  {
    target.Identifier = source.Identifier;
    target.StatusCd = source.StatusCd;
    target.StatusDate = source.StatusDate;
    target.StatusReasonCode = source.StatusReasonCode;
  }

  private static void MoveIwoAction4(IwoAction source, IwoAction target)
  {
    target.ActionType = source.ActionType;
    target.StatusCd = source.StatusCd;
  }

  private static void MoveIwoTransaction(IwoTransaction source,
    IwoTransaction target)
  {
    target.Identifier = source.Identifier;
    target.TransactionNumber = source.TransactionNumber;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAp = source.KeyAp;
    target.KeyCase = source.KeyCase;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyLegalAction = source.KeyLegalAction;
  }

  private void UseEstabLegalActionIncSource()
  {
    var useImport = new EstabLegalActionIncSource.Import();
    var useExport = new EstabLegalActionIncSource.Export();

    useImport.IwglInd.Text1 = local.IwglInd.Text1;
    useImport.LegalActionIncomeSource.Assign(local.LegalActionIncomeSource);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.IncomeSource.Identifier =
      export.Export1.Item.GincomeSource.Identifier;

    Call(EstabLegalActionIncSource.Execute, useImport, useExport);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseLeCreateIwoTransaction()
  {
    var useImport = new LeCreateIwoTransaction.Import();
    var useExport = new LeCreateIwoTransaction.Export();

    useImport.OutgoingDocument.PrintSucessfulIndicator =
      local.OutgoingDocument.PrintSucessfulIndicator;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    useImport.IncomeSource.Identifier =
      export.Export1.Item.GincomeSource.Identifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveIwoAction4(local.IwoAction, useImport.IwoAction);
    useImport.IwoTransaction.TransactionNumber =
      local.IwoTransaction.TransactionNumber;

    Call(LeCreateIwoTransaction.Execute, useImport, useExport);

    MoveIwoAction1(useExport.IwoAction, export.Export1.Update.GiwoAction);
    MoveIwoTransaction(useExport.IwoTransaction,
      export.Export1.Update.GiwoTransaction);
  }

  private void UseLeLaisDisplayIwoTrans()
  {
    var useImport = new LeLaisDisplayIwoTrans.Import();
    var useExport = new LeLaisDisplayIwoTrans.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeLaisDisplayIwoTrans.Execute, useImport, useExport);

    export.LactActionTaken.Description = useExport.LactActionTaken.Description;
    export.LegalAction.Assign(useExport.LegalAction);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseLeUpdateIwoActionStatus()
  {
    var useImport = new LeUpdateIwoActionStatus.Import();
    var useExport = new LeUpdateIwoActionStatus.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveIwoAction2(local.IwoAction, useImport.IwoAction);
    useImport.IwoTransaction.Identifier =
      export.Export1.Item.GiwoTransaction.Identifier;

    Call(LeUpdateIwoActionStatus.Execute, useImport, useExport);

    export.Export1.Update.GiwoTransaction.Identifier =
      useExport.IwoTransaction.Identifier;
    MoveIwoAction3(useExport.IwoAction, export.Export1.Update.GiwoAction);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveCommon(local.PrintProcess, useImport.PrintProcess);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

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

    useImport.LegalAction.Assign(export.LegalAction);
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.FieldValue.Assign(local.FieldValue);
    useImport.Field.Name = local.Field.Name;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Name = local.Document.Name;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);

    local.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.PrintRetCode.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.PrintRetCode.Text50 = useExport.WorkArea.Text50;
  }

  private void UseUpdateOutgoingDocument()
  {
    var useImport = new UpdateOutgoingDocument.Import();
    var useExport = new UpdateOutgoingDocument.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      entities.Infrastructure.SystemGeneratedIdentifier;
    useImport.OutgoingDocument.PrintSucessfulIndicator =
      local.OutgoingDocument.PrintSucessfulIndicator;

    Call(UpdateOutgoingDocument.Execute, useImport, useExport);
  }

  private void AssociateIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    var infId = entities.OutgoingDocument.InfId;

    entities.IwoAction.Populated = false;
    Update("AssociateIwoAction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "infId", infId);
        db.SetInt32(command, "identifier", entities.IwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
      });

    entities.IwoAction.InfId = infId;
    entities.IwoAction.Populated = true;
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "actionTaken", export.LegalAction.ActionTaken);
        db.SetDate(command, "expirationDate", new DateTime(2099, 12, 31));
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.EffectiveDate = db.GetDate(reader, 1);
        entities.Document.ExpirationDate = db.GetDate(reader, 2);
        entities.Document.Populated = true;
      });
  }

  private bool ReadEmployer()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.IncomeSource.EmpId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 2);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 3);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Sdu.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(command, "state", entities.Fips.State);
      },
      (db, reader) =>
      {
        entities.Sdu.State = db.GetInt32(reader, 0);
        entities.Sdu.County = db.GetInt32(reader, 1);
        entities.Sdu.Location = db.GetInt32(reader, 2);
        entities.Sdu.LocationDescription = db.GetNullableString(reader, 3);
        entities.Sdu.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Sdu.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Sdu.County);
        db.SetNullableInt32(command, "fipState", entities.Sdu.State);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 5);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", export.CsePersonsWorkSet.Number);
        db.SetDateTime(
          command, "identifier",
          import.SelectedFromIncl.Item.SelectedFromIncl1.Identifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Name = db.GetNullableString(reader, 1);
        entities.IncomeSource.CspINumber = db.GetString(reader, 2);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 3);
        entities.IncomeSource.Populated = true;
      });
  }

  private bool ReadIncomeSourceEmployer()
  {
    entities.IncomeSource.Populated = false;
    entities.Employer.Populated = false;

    return Read("ReadIncomeSourceEmployer",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          export.Export1.Item.GincomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Name = db.GetNullableString(reader, 1);
        entities.IncomeSource.CspINumber = db.GetString(reader, 2);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 3);
        entities.Employer.Identifier = db.GetInt32(reader, 3);
        entities.Employer.Ein = db.GetNullableString(reader, 4);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 5);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 6);
        entities.IncomeSource.Populated = true;
        entities.Employer.Populated = true;
      });
  }

  private bool ReadIwoAction()
  {
    entities.IwoAction.Populated = false;

    return Read("ReadIwoAction",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", global.UserId);
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 2);
        entities.IwoAction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoAction.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.IwoAction.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.IwoAction.CspNumber = db.GetString(reader, 6);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 7);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 8);
        entities.IwoAction.InfId = db.GetNullableInt32(reader, 9);
        entities.IwoAction.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
        db.SetString(command, "cspINumber", export.CsePersonsWorkSet.Number);
        db.SetNullableString(command, "ein", local.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.ActionTaken = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", local.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 1);
        entities.OutgoingDocument.Populated = true;
      });
  }

  private bool ReadOutgoingDocumentInfrastructure()
  {
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocumentInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", export.Export1.Item.GiwoAction.Identifier);
        db.SetInt32(
          command, "iwtIdentifier",
          export.Export1.Item.GiwoTransaction.Identifier);
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 1);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GincomeSource.
      /// </summary>
      [JsonPropertyName("gincomeSource")]
      public IncomeSource GincomeSource
      {
        get => gincomeSource ??= new();
        set => gincomeSource = value;
      }

      /// <summary>
      /// A value of GimportEiwo.
      /// </summary>
      [JsonPropertyName("gimportEiwo")]
      public Common GimportEiwo
      {
        get => gimportEiwo ??= new();
        set => gimportEiwo = value;
      }

      /// <summary>
      /// A value of GiwoTransaction.
      /// </summary>
      [JsonPropertyName("giwoTransaction")]
      public IwoTransaction GiwoTransaction
      {
        get => giwoTransaction ??= new();
        set => giwoTransaction = value;
      }

      /// <summary>
      /// A value of GiwoAction.
      /// </summary>
      [JsonPropertyName("giwoAction")]
      public IwoAction GiwoAction
      {
        get => giwoAction ??= new();
        set => giwoAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 66;

      private Common gcommon;
      private IncomeSource gincomeSource;
      private Common gimportEiwo;
      private IwoTransaction giwoTransaction;
      private IwoAction giwoAction;
    }

    /// <summary>A SelectedFromInclGroup group.</summary>
    [Serializable]
    public class SelectedFromInclGroup
    {
      /// <summary>
      /// A value of SelectedFromIncl1.
      /// </summary>
      [JsonPropertyName("selectedFromIncl1")]
      public IncomeSource SelectedFromIncl1
      {
        get => selectedFromIncl1 ??= new();
        set => selectedFromIncl1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private IncomeSource selectedFromIncl1;
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
    /// A value of LactActionTaken.
    /// </summary>
    [JsonPropertyName("lactActionTaken")]
    public CodeValue LactActionTaken
    {
      get => lactActionTaken ??= new();
      set => lactActionTaken = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// Gets a value of SelectedFromIncl.
    /// </summary>
    [JsonIgnore]
    public Array<SelectedFromInclGroup> SelectedFromIncl =>
      selectedFromIncl ??= new(SelectedFromInclGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SelectedFromIncl for json serialization.
    /// </summary>
    [JsonPropertyName("selectedFromIncl")]
    [Computed]
    public IList<SelectedFromInclGroup> SelectedFromIncl_Json
    {
      get => selectedFromIncl;
      set => SelectedFromIncl.Assign(value);
    }

    /// <summary>
    /// A value of PrintConfirmation.
    /// </summary>
    [JsonPropertyName("printConfirmation")]
    public Common PrintConfirmation
    {
      get => printConfirmation ??= new();
      set => printConfirmation = value;
    }

    private Case1 case1;
    private CodeValue lactActionTaken;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard standard;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Array<SelectedFromInclGroup> selectedFromIncl;
    private Common printConfirmation;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GincomeSource.
      /// </summary>
      [JsonPropertyName("gincomeSource")]
      public IncomeSource GincomeSource
      {
        get => gincomeSource ??= new();
        set => gincomeSource = value;
      }

      /// <summary>
      /// A value of GexportEiwo.
      /// </summary>
      [JsonPropertyName("gexportEiwo")]
      public Common GexportEiwo
      {
        get => gexportEiwo ??= new();
        set => gexportEiwo = value;
      }

      /// <summary>
      /// A value of GiwoTransaction.
      /// </summary>
      [JsonPropertyName("giwoTransaction")]
      public IwoTransaction GiwoTransaction
      {
        get => giwoTransaction ??= new();
        set => giwoTransaction = value;
      }

      /// <summary>
      /// A value of GiwoAction.
      /// </summary>
      [JsonPropertyName("giwoAction")]
      public IwoAction GiwoAction
      {
        get => giwoAction ??= new();
        set => giwoAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 66;

      private Common gcommon;
      private IncomeSource gincomeSource;
      private Common gexportEiwo;
      private IwoTransaction giwoTransaction;
      private IwoAction giwoAction;
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
    /// A value of ToIncl.
    /// </summary>
    [JsonPropertyName("toIncl")]
    public Common ToIncl
    {
      get => toIncl ??= new();
      set => toIncl = value;
    }

    /// <summary>
    /// A value of LactActionTaken.
    /// </summary>
    [JsonPropertyName("lactActionTaken")]
    public CodeValue LactActionTaken
    {
      get => lactActionTaken ??= new();
      set => lactActionTaken = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of ToEiwhIwoTransaction.
    /// </summary>
    [JsonPropertyName("toEiwhIwoTransaction")]
    public IwoTransaction ToEiwhIwoTransaction
    {
      get => toEiwhIwoTransaction ??= new();
      set => toEiwhIwoTransaction = value;
    }

    /// <summary>
    /// A value of PrintConfirmation.
    /// </summary>
    [JsonPropertyName("printConfirmation")]
    public Common PrintConfirmation
    {
      get => printConfirmation ??= new();
      set => printConfirmation = value;
    }

    /// <summary>
    /// A value of ToEiwhIncomeSource.
    /// </summary>
    [JsonPropertyName("toEiwhIncomeSource")]
    public IncomeSource ToEiwhIncomeSource
    {
      get => toEiwhIncomeSource ??= new();
      set => toEiwhIncomeSource = value;
    }

    private Case1 case1;
    private Common toIncl;
    private CodeValue lactActionTaken;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard standard;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private IwoTransaction toEiwhIwoTransaction;
    private Common printConfirmation;
    private IncomeSource toEiwhIncomeSource;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of NullNextTranInfo.
    /// </summary>
    [JsonPropertyName("nullNextTranInfo")]
    public NextTranInfo NullNextTranInfo
    {
      get => nullNextTranInfo ??= new();
      set => nullNextTranInfo = value;
    }

    /// <summary>
    /// A value of NullIwoTransaction.
    /// </summary>
    [JsonPropertyName("nullIwoTransaction")]
    public IwoTransaction NullIwoTransaction
    {
      get => nullIwoTransaction ??= new();
      set => nullIwoTransaction = value;
    }

    /// <summary>
    /// A value of NullIwoAction.
    /// </summary>
    [JsonPropertyName("nullIwoAction")]
    public IwoAction NullIwoAction
    {
      get => nullIwoAction ??= new();
      set => nullIwoAction = value;
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
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of IwglInd.
    /// </summary>
    [JsonPropertyName("iwglInd")]
    public WorkArea IwglInd
    {
      get => iwglInd ??= new();
      set => iwglInd = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
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
    /// A value of EiwoAgingCutoffDate.
    /// </summary>
    [JsonPropertyName("eiwoAgingCutoffDate")]
    public DateWorkArea EiwoAgingCutoffDate
    {
      get => eiwoAgingCutoffDate ??= new();
      set => eiwoAgingCutoffDate = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of PrintProcess.
    /// </summary>
    [JsonPropertyName("printProcess")]
    public Common PrintProcess
    {
      get => printProcess ??= new();
      set => printProcess = value;
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
    /// A value of PrintRetCode.
    /// </summary>
    [JsonPropertyName("printRetCode")]
    public WorkArea PrintRetCode
    {
      get => printRetCode ??= new();
      set => printRetCode = value;
    }

    private FieldValue fieldValue;
    private Field field;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private NextTranInfo nullNextTranInfo;
    private IwoTransaction nullIwoTransaction;
    private IwoAction nullIwoAction;
    private Document document;
    private SpDocKey spDocKey;
    private Employer employer;
    private IwoTransaction iwoTransaction;
    private IwoAction iwoAction;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private WorkArea iwglInd;
    private LegalActionIncomeSource legalActionIncomeSource;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea eiwoAgingCutoffDate;
    private CsePerson csePerson;
    private Common common;
    private AbendData abendData;
    private Common printProcess;
    private Common position;
    private WorkArea printRetCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Sdu.
    /// </summary>
    [JsonPropertyName("sdu")]
    public Fips Sdu
    {
      get => sdu ??= new();
      set => sdu = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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

    private Document document;
    private IwoAction iwoAction;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private LegalActionIncomeSource legalActionIncomeSource;
    private IwoTransaction iwoTransaction;
    private IncomeSource incomeSource;
    private Employer employer;
    private CsePerson csePerson;
    private FipsTribAddress fipsTribAddress;
    private Fips sdu;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private CodeValue codeValue;
    private Code code;
  }
#endregion
}
