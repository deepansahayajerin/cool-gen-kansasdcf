// Program: LE_EIWL_EIWO_LOG, ID: 1902508645, model: 746.
// Short name: SWEEIWLP
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
/// A program: LE_EIWL_EIWO_LOG.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeEiwlEiwoLog: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EIWL_EIWO_LOG program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEiwlEiwoLog(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEiwlEiwoLog.
  /// </summary>
  public LeEiwlEiwoLog(IContext context, Import import, Export export):
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
    // 11/17/15  GVandy	CQ50342		When a user returns from EIWH display the
    // 					same screen of data as when they went to
    // 					EIWH.  No longer do a full display or
    // 					calculate the severity counts.
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveCseOrganization(import.SearchContractor, export.SearchContractor);
    MoveOffice(import.SearchOffice, export.SearchOffice);
    export.SearchServiceProvider.UserId = import.SearchServiceProvider.UserId;
    export.SearchWorker.FormattedName = import.SearchWorker.FormattedName;
    export.SearchCsePerson.Number = import.SearchCsePerson.Number;
    MoveIwoTransaction2(import.SearchIwoTransaction, export.SearchIwoTransaction);
      
    export.SearchStatusCode.Description = import.SearchStatusCode.Description;
    export.SearchSeverity.Text7 = import.SearchSeverity.Text7;
    export.SearchSeverityCode.Description =
      import.SearchSeverityCode.Description;
    export.HiddenSearchCsePerson.Number = import.HiddenSearchCsePerson.Number;
    MoveIwoTransaction2(import.HiddenSearchIwoTransaction,
      export.HiddenSearchIwoTransaction);
    export.HiddenSearchOffice.SystemGeneratedId =
      import.HiddenSearchOffice.SystemGeneratedId;
    export.HiddenSearchServiceProvider.UserId =
      import.HiddenSearchServiceProvider.UserId;
    export.HiddenSearchContractor.Code = import.HiddenSearchContractor.Code;
    export.HiddenSearchSeverity.Text7 = import.HiddenSearchSeverity.Text7;
    export.SeverityRed.Count = import.SeverityRed.Count;
    export.SeverityYellow.Count = import.SeverityYellow.Count;
    export.SeverityDefault.Count = import.SeverityDefault.Count;
    export.SeverityTotal.Count = import.SeverityTotal.Count;
    export.PromptContractor.SelectChar = import.PromptContractor.SelectChar;
    export.PromptOffice.SelectChar = import.PromptOffice.SelectChar;
    export.PromptWorker.SelectChar = import.PromptWorker.SelectChar;
    export.PromptPerson.SelectChar = import.PromptPerson.SelectChar;
    export.PromptStatus.SelectChar = import.PromptStatus.SelectChar;
    export.PromptSeverity.SelectChar = import.PromptSeverity.SelectChar;
    export.MoreIndicator.Text9 = import.MoreIndicator.Text9;
    export.PageNumber.Count = import.PageNumber.Count;

    for(import.Paging.Index = 0; import.Paging.Index < import.Paging.Count; ++
      import.Paging.Index)
    {
      if (!import.Paging.CheckSize())
      {
        break;
      }

      export.Paging.Index = import.Paging.Index;
      export.Paging.CheckSize();

      MoveIwoTransaction3(import.Paging.Item.GimportPaging,
        export.Paging.Update.GexportPaging);
    }

    import.Paging.CheckIndex();

    // -- If any of the search fields are spaces then space out the associated 
    // descriptions.
    if (IsEmpty(export.SearchContractor.Code))
    {
      export.SearchContractor.Name = "";
    }

    if (export.SearchOffice.SystemGeneratedId == 0)
    {
      export.SearchOffice.Name = "";
    }

    if (IsEmpty(export.SearchServiceProvider.UserId))
    {
      export.SearchWorker.FormattedName = "";
    }

    if (IsEmpty(export.SearchIwoTransaction.CurrentStatus))
    {
      export.SearchStatusCode.Description =
        Spaces(CodeValue.Description_MaxLength);
    }

    if (IsEmpty(export.SearchSeverity.Text7))
    {
      export.SearchSeverityCode.Description =
        Spaces(CodeValue.Description_MaxLength);
    }

    if (!IsEmpty(export.SearchCsePerson.Number))
    {
      UseCabZeroFillNumber();
    }

    if (!IsEmpty(export.SearchIwoTransaction.TransactionNumber))
    {
      if (Verify(export.SearchIwoTransaction.TransactionNumber, "0123456789 ") ==
        0)
      {
        export.SearchIwoTransaction.TransactionNumber =
          NumberToString(StringToNumber(
            export.SearchIwoTransaction.TransactionNumber), 4, 12);
      }
    }

    // -- Move import group to export group.
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
      export.Export1.Update.Goffice.SystemGeneratedId =
        import.Import1.Item.Goffice.SystemGeneratedId;
      export.Export1.Update.GserviceProvider.UserId =
        import.Import1.Item.GserviceProvider.UserId;
      MoveIwoTransaction1(import.Import1.Item.GiwoTransaction,
        export.Export1.Update.GiwoTransaction);
      MoveIncomeSource(import.Import1.Item.GincomeSource,
        export.Export1.Update.GincomeSource);
      export.Export1.Update.GlegalAction.
        Assign(import.Import1.Item.GlegalAction);
      export.Export1.Update.GiwoAction.Assign(import.Import1.Item.GiwoAction);
      export.Export1.Update.GexportSeverity.Text7 =
        import.Import1.Item.GimportSeverity.Text7;
      export.Export1.Update.GcsePerson.Number =
        import.Import1.Item.GcsePerson.Number;
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

      switch(TrimEnd(export.Export1.Item.GexportSeverity.Text7))
      {
        case "RED":
          var field1 =
            GetField(export.Export1.Item.Goffice, "systemGeneratedId");

          field1.Color = "red";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GserviceProvider, "userId");

          field2.Color = "red";
          field2.Protected = true;

          var field3 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field3.Color = "red";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GincomeSource, "name");

          field4.Color = "red";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GlegalAction, "actionTaken");

          field5.Color = "red";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "red";
          field6.Protected = true;

          var field7 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field7.Color = "red";
          field7.Protected = true;

          break;
        case "YELLOW":
          var field8 =
            GetField(export.Export1.Item.Goffice, "systemGeneratedId");

          field8.Color = "yellow";
          field8.Protected = true;

          var field9 = GetField(export.Export1.Item.GserviceProvider, "userId");

          field9.Color = "yellow";
          field9.Protected = true;

          var field10 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field10.Color = "yellow";
          field10.Protected = true;

          var field11 = GetField(export.Export1.Item.GincomeSource, "name");

          field11.Color = "yellow";
          field11.Protected = true;

          var field12 =
            GetField(export.Export1.Item.GlegalAction, "actionTaken");

          field12.Color = "yellow";
          field12.Protected = true;

          var field13 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field13.Color = "yellow";
          field13.Protected = true;

          var field14 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field14.Color = "yellow";
          field14.Protected = true;

          break;
        default:
          var field15 =
            GetField(export.Export1.Item.Goffice, "systemGeneratedId");

          field15.Protected = true;

          var field16 =
            GetField(export.Export1.Item.GserviceProvider, "userId");

          field16.Protected = true;

          var field17 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field17.Protected = true;

          var field18 = GetField(export.Export1.Item.GincomeSource, "name");

          field18.Protected = true;

          var field19 =
            GetField(export.Export1.Item.GlegalAction, "actionTaken");

          field19.Protected = true;

          var field20 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field20.Protected = true;

          var field21 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field21.Protected = true;

          break;
      }
    }

    export.Export1.CheckIndex();

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETCSOR":
        export.PromptContractor.SelectChar = "";

        switch(AsChar(import.FromCsor.Type1))
        {
          case ' ':
            var field1 = GetField(export.SearchContractor, "code");

            field1.Error = true;

            ExitState = "ACO_NE0000_NO_PROMPT_SELECTION";

            return;
          case 'X':
            MoveCseOrganization(import.FromCsor, export.SearchContractor);

            break;
          default:
            MoveCseOrganization(import.FromCsor, export.SearchContractor);

            var field2 = GetField(export.SearchContractor, "code");

            field2.Error = true;

            ExitState = "LE_NE0000_INVALID_ORG_TYPE";

            return;
        }

        global.Command = "DISPLAY";

        break;
      case "RETOFCL":
        export.PromptOffice.SelectChar = "";

        if (IsEmpty(import.FromOfclAndSvpo.Name))
        {
          var field1 = GetField(export.SearchOffice, "systemGeneratedId");

          field1.Error = true;

          ExitState = "ACO_NE0000_NO_PROMPT_SELECTION";

          return;
        }
        else
        {
          MoveOffice(import.FromOfclAndSvpo, export.SearchOffice);
        }

        global.Command = "DISPLAY";

        break;
      case "RETSVPL":
        export.PromptWorker.SelectChar = "";

        if (IsEmpty(import.FromSvplAndSvpo.UserId))
        {
          var field1 = GetField(export.SearchServiceProvider, "userId");

          field1.Error = true;

          ExitState = "ACO_NE0000_NO_PROMPT_SELECTION";

          return;
        }
        else
        {
          export.SearchServiceProvider.UserId = import.FromSvplAndSvpo.UserId;
          export.SearchWorker.FormattedName =
            TrimEnd(import.FromSvplAndSvpo.LastName) + ", " + TrimEnd
            (import.FromSvplAndSvpo.FirstName) + " " + import
            .FromSvplAndSvpo.MiddleInitial;
        }

        global.Command = "DISPLAY";

        break;
      case "RETSVPO":
        export.PromptWorker.SelectChar = "";

        if (IsEmpty(import.FromSvplAndSvpo.UserId))
        {
          var field1 = GetField(export.SearchServiceProvider, "userId");

          field1.Error = true;

          ExitState = "ACO_NE0000_NO_PROMPT_SELECTION";

          return;
        }
        else
        {
          export.SearchServiceProvider.UserId = import.FromSvplAndSvpo.UserId;
          MoveOffice(import.FromOfclAndSvpo, export.SearchOffice);
          export.SearchWorker.FormattedName =
            TrimEnd(import.FromSvplAndSvpo.LastName) + ", " + TrimEnd
            (import.FromSvplAndSvpo.FirstName) + " " + import
            .FromSvplAndSvpo.MiddleInitial;
        }

        global.Command = "DISPLAY";

        break;
      case "RETNAME":
        export.PromptPerson.SelectChar = "";

        if (IsEmpty(import.FromName.Number))
        {
          var field1 = GetField(export.SearchCsePerson, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_NO_PROMPT_SELECTION";

          return;
        }
        else
        {
          export.SearchCsePerson.Number = import.FromName.Number;
        }

        global.Command = "DISPLAY";

        break;
      case "RETCDVL":
        if (AsChar(export.PromptStatus.SelectChar) == 'S')
        {
          export.PromptStatus.SelectChar = "";

          if (IsEmpty(import.FromCdvl.Cdvalue))
          {
            var field1 = GetField(export.SearchIwoTransaction, "currentStatus");

            field1.Error = true;

            ExitState = "ACO_NE0000_NO_PROMPT_SELECTION";

            return;
          }
          else
          {
            export.SearchIwoTransaction.CurrentStatus = import.FromCdvl.Cdvalue;
            export.SearchStatusCode.Description = import.FromCdvl.Description;
          }
        }
        else if (AsChar(export.PromptSeverity.SelectChar) == 'S')
        {
          export.PromptSeverity.SelectChar = "";

          if (IsEmpty(import.FromCdvl.Cdvalue))
          {
            var field1 = GetField(export.SearchSeverity, "text7");

            field1.Error = true;

            ExitState = "ACO_NE0000_NO_PROMPT_SELECTION";

            return;
          }
          else
          {
            export.SearchSeverity.Text7 = import.FromCdvl.Cdvalue;
            export.SearchSeverityCode.Description = import.FromCdvl.Description;
          }
        }

        global.Command = "DISPLAY";

        break;
      case "RETEIWH":
        // -- Redisplay the same page of data as when the user went to EIWH.
        ++export.PageNumber.Count;
        global.Command = "PREV";

        break;
      default:
        // -- Continue
        break;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumber = export.SearchCsePerson.Number;
      UseScCabNextTranPut();

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
      export.SearchCsePerson.Number = "";
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "EIWH"))
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
    // -- Prompt field validation.
    // ---------------------------------------------
    if (Equal(global.Command, "LIST"))
    {
      local.PromptCount.Count = 0;

      switch(AsChar(export.PromptSeverity.SelectChar))
      {
        case 'S':
          ++local.PromptCount.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptSeverity, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      switch(AsChar(export.PromptStatus.SelectChar))
      {
        case 'S':
          ++local.PromptCount.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptStatus, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      switch(AsChar(export.PromptPerson.SelectChar))
      {
        case 'S':
          ++local.PromptCount.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptPerson, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      switch(AsChar(export.PromptWorker.SelectChar))
      {
        case 'S':
          ++local.PromptCount.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptWorker, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      switch(AsChar(export.PromptOffice.SelectChar))
      {
        case 'S':
          ++local.PromptCount.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptOffice, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      switch(AsChar(export.PromptContractor.SelectChar))
      {
        case 'S':
          ++local.PromptCount.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptContractor, "selectChar");

          field1.Error = true;

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
          var field1 = GetField(export.PromptSeverity, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptStatus, "selectChar");

          field2.Error = true;

          var field3 = GetField(export.PromptPerson, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.PromptWorker, "selectChar");

          field4.Error = true;

          var field5 = GetField(export.PromptOffice, "selectChar");

          field5.Error = true;

          var field6 = GetField(export.PromptContractor, "selectChar");

          field6.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          break;
        case 1:
          break;
        default:
          if (AsChar(export.PromptSeverity.SelectChar) == 'S')
          {
            var field7 = GetField(export.PromptSeverity, "selectChar");

            field7.Error = true;
          }
          else
          {
          }

          if (AsChar(export.PromptStatus.SelectChar) == 'S')
          {
            var field7 = GetField(export.PromptStatus, "selectChar");

            field7.Error = true;
          }
          else
          {
          }

          if (AsChar(export.PromptPerson.SelectChar) == 'S')
          {
            var field7 = GetField(export.PromptPerson, "selectChar");

            field7.Error = true;
          }
          else
          {
          }

          if (AsChar(export.PromptWorker.SelectChar) == 'S')
          {
            var field7 = GetField(export.PromptWorker, "selectChar");

            field7.Error = true;
          }
          else
          {
          }

          if (AsChar(export.PromptOffice.SelectChar) == 'S')
          {
            var field7 = GetField(export.PromptOffice, "selectChar");

            field7.Error = true;
          }
          else
          {
          }

          if (AsChar(export.PromptContractor.SelectChar) == 'S')
          {
            var field7 = GetField(export.PromptContractor, "selectChar");

            field7.Error = true;
          }
          else
          {
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      switch(AsChar(export.PromptSeverity.SelectChar))
      {
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptSeverity, "selectChar");

          field1.Error = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
      }

      switch(AsChar(export.PromptStatus.SelectChar))
      {
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptStatus, "selectChar");

          field1.Error = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
      }

      switch(AsChar(export.PromptPerson.SelectChar))
      {
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptPerson, "selectChar");

          field1.Error = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
      }

      switch(AsChar(export.PromptWorker.SelectChar))
      {
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptWorker, "selectChar");

          field1.Error = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
      }

      switch(AsChar(export.PromptOffice.SelectChar))
      {
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptOffice, "selectChar");

          field1.Error = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
      }

      switch(AsChar(export.PromptContractor.SelectChar))
      {
        case '+':
          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.PromptContractor, "selectChar");

          field1.Error = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

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

    if (local.Common.Count == 0 && Equal(global.Command, "EIWH"))
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
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "PREV":
        if (!Equal(export.SearchCsePerson.Number,
          export.HiddenSearchCsePerson.Number) || !
          Equal(export.SearchIwoTransaction.TransactionNumber,
          export.HiddenSearchIwoTransaction.TransactionNumber) || AsChar
          (export.SearchIwoTransaction.CurrentStatus) != AsChar
          (export.HiddenSearchIwoTransaction.CurrentStatus) || export
          .SearchOffice.SystemGeneratedId != export
          .HiddenSearchOffice.SystemGeneratedId || !
          Equal(export.SearchServiceProvider.UserId,
          export.HiddenSearchServiceProvider.UserId) || !
          Equal(export.SearchContractor.Code, export.HiddenSearchContractor.Code)
          || !
          Equal(export.SearchSeverity.Text7, export.HiddenSearchSeverity.Text7))
        {
          ExitState = "ACO_PREV_INVALID_WITH_KEY_CHANGE";

          return;
        }

        if (export.PageNumber.Count <= 1)
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }

        --export.PageNumber.Count;

        export.Paging.Index = export.PageNumber.Count - 1;
        export.Paging.CheckSize();

        UseLeEiwlDisplayLog2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (export.PageNumber.Count == 1)
        {
          export.MoreIndicator.Text9 = "MORE:   +";

          export.Paging.Index = export.PageNumber.Count;
          export.Paging.CheckSize();

          MoveIwoTransaction3(local.NextPage, export.Paging.Update.GexportPaging);
            
        }
        else
        {
          export.Paging.Index = export.PageNumber.Count;
          export.Paging.CheckSize();

          MoveIwoTransaction3(local.NextPage, export.Paging.Update.GexportPaging);
            
          export.MoreIndicator.Text9 = "MORE: - +";
        }

        export.HiddenSearchCsePerson.Number = export.SearchCsePerson.Number;
        export.HiddenSearchContractor.Code = export.SearchContractor.Code;
        MoveIwoTransaction2(export.SearchIwoTransaction,
          export.HiddenSearchIwoTransaction);
        export.HiddenSearchOffice.SystemGeneratedId =
          export.SearchOffice.SystemGeneratedId;
        export.HiddenSearchServiceProvider.UserId =
          export.SearchServiceProvider.UserId;
        export.HiddenSearchSeverity.Text7 = export.SearchSeverity.Text7;

        if (export.Export1.Index == -1)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "NEXT":
        if (!Equal(export.SearchCsePerson.Number,
          export.HiddenSearchCsePerson.Number) || !
          Equal(export.SearchIwoTransaction.TransactionNumber,
          export.HiddenSearchIwoTransaction.TransactionNumber) || AsChar
          (export.SearchIwoTransaction.CurrentStatus) != AsChar
          (export.HiddenSearchIwoTransaction.CurrentStatus) || export
          .SearchOffice.SystemGeneratedId != export
          .HiddenSearchOffice.SystemGeneratedId || !
          Equal(export.SearchServiceProvider.UserId,
          export.HiddenSearchServiceProvider.UserId) || !
          Equal(export.SearchContractor.Code, export.HiddenSearchContractor.Code)
          || !
          Equal(export.SearchSeverity.Text7, export.HiddenSearchSeverity.Text7))
        {
          ExitState = "ACO_PREV_INVALID_WITH_KEY_CHANGE";

          return;
        }

        if (export.PageNumber.Count == Export.PagingGroup.Capacity)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";

          return;
        }

        if (export.PageNumber.Count == 0)
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        if (export.PageNumber.Count == export.Paging.Count)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          return;
        }

        ++export.PageNumber.Count;

        export.Paging.Index = export.PageNumber.Count - 1;
        export.Paging.CheckSize();

        UseLeEiwlDisplayLog2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(local.NextPage.StatusDate, local.Null1.Date))
        {
          export.MoreIndicator.Text9 = "MORE: -";
        }
        else
        {
          export.Paging.Index = export.PageNumber.Count;
          export.Paging.CheckSize();

          MoveIwoTransaction3(local.NextPage, export.Paging.Update.GexportPaging);
            
          export.MoreIndicator.Text9 = "MORE: - +";
        }

        export.HiddenSearchCsePerson.Number = export.SearchCsePerson.Number;
        export.HiddenSearchContractor.Code = export.SearchContractor.Code;
        MoveIwoTransaction2(export.SearchIwoTransaction,
          export.HiddenSearchIwoTransaction);
        export.HiddenSearchOffice.SystemGeneratedId =
          export.SearchOffice.SystemGeneratedId;
        export.HiddenSearchServiceProvider.UserId =
          export.SearchServiceProvider.UserId;
        export.HiddenSearchSeverity.Text7 = export.SearchSeverity.Text7;

        if (export.Export1.Index == -1)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "DISPLAY":
        export.PageNumber.Count = 1;

        export.Paging.Index = export.PageNumber.Count - 1;
        export.Paging.CheckSize();

        export.Paging.Update.GexportPaging.StatusDate =
          new DateTime(2099, 12, 31);
        export.Paging.Count = 1;
        export.Export1.Count = 0;
        export.MoreIndicator.Text9 = "MORE:";

        // -- Default the display to the current worker id.
        if (IsEmpty(export.SearchContractor.Code) && export
          .SearchOffice.SystemGeneratedId == 0 && IsEmpty
          (export.SearchServiceProvider.UserId) && IsEmpty
          (export.SearchCsePerson.Number) && IsEmpty
          (export.SearchIwoTransaction.TransactionNumber))
        {
          export.SearchServiceProvider.UserId = global.UserId;
        }

        if (IsEmpty(export.SearchSeverity.Text7))
        {
        }
        else
        {
          local.Code.CodeName = "IWO SEVERITY";
          local.CodeValue.Cdvalue = export.SearchSeverity.Text7;
          UseCabValidateCodeValue2();

          if (AsChar(local.ValidCode.Flag) == 'Y')
          {
            export.SearchSeverityCode.Description = local.CodeValue.Description;
          }
          else
          {
            export.SearchSeverityCode.Description =
              Spaces(CodeValue.Description_MaxLength);

            var field1 = GetField(export.SearchSeverity, "text7");

            field1.Error = true;

            ExitState = "LE0000_INVALID_SEVERITY";
          }
        }

        if (IsEmpty(export.SearchIwoTransaction.CurrentStatus))
        {
        }
        else
        {
          local.Code.CodeName = "IWO STATUS";
          local.CodeValue.Cdvalue =
            export.SearchIwoTransaction.CurrentStatus ?? Spaces(10);
          UseCabValidateCodeValue2();

          if (AsChar(local.ValidCode.Flag) == 'Y')
          {
            export.SearchStatusCode.Description = local.CodeValue.Description;
          }
          else
          {
            export.SearchStatusCode.Description =
              Spaces(CodeValue.Description_MaxLength);

            var field1 = GetField(export.SearchIwoTransaction, "currentStatus");

            field1.Error = true;

            ExitState = "LE0000_INVALID_STATUS";
          }
        }

        if (!IsEmpty(export.SearchIwoTransaction.CurrentStatus) && !
          IsEmpty(export.SearchSeverity.Text7) && IsExitState
          ("ACO_NN0000_ALL_OK"))
        {
          local.CrossValidationCode.CodeName = "IWO STATUS";
          local.CrossValidationCodeValue.Cdvalue =
            export.SearchIwoTransaction.CurrentStatus ?? Spaces(10);
          local.Code.CodeName = "IWO SEVERITY";
          local.CodeValue.Cdvalue = export.SearchSeverity.Text7;
          UseCabValidateCodeValue1();

          if (AsChar(local.ValidCode.Flag) == 'Y')
          {
          }
          else
          {
            var field1 = GetField(export.SearchSeverity, "text7");

            field1.Error = true;

            ExitState = "LE0000_INVALID_SEVERITY_4_STATUS";
          }
        }

        if (IsEmpty(export.SearchCsePerson.Number))
        {
        }
        else
        {
          if (!ReadCsePerson())
          {
            var field1 = GetField(export.SearchCsePerson, "number");

            field1.Error = true;

            ExitState = "CSE_PERSON_NF";
          }
        }

        if (IsEmpty(export.SearchServiceProvider.UserId))
        {
        }
        else
        {
          if (ReadServiceProvider())
          {
            export.SearchWorker.FormattedName =
              TrimEnd(entities.ServiceProvider.LastName) + ", " + TrimEnd
              (entities.ServiceProvider.FirstName) + " " + entities
              .ServiceProvider.MiddleInitial;
          }
          else
          {
            export.SearchWorker.FormattedName = "";

            var field1 = GetField(export.SearchServiceProvider, "userId");

            field1.Error = true;

            ExitState = "SERVICE_PROVIDER_NF";
          }
        }

        if (export.SearchOffice.SystemGeneratedId == 0)
        {
        }
        else
        {
          if (ReadOffice())
          {
            MoveOffice(entities.Office, export.SearchOffice);
          }
          else
          {
            export.SearchOffice.Name = "";

            var field1 = GetField(export.SearchOffice, "systemGeneratedId");

            field1.Error = true;

            ExitState = "OFFICE_NF";
          }
        }

        if (entities.ServiceProvider.Populated && entities.Office.Populated)
        {
          if (!ReadOfficeServiceProvider())
          {
            var field1 = GetField(export.SearchOffice, "systemGeneratedId");

            field1.Error = true;

            var field2 = GetField(export.SearchServiceProvider, "userId");

            field2.Error = true;

            ExitState = "LE0000_OSP_NOT_ATIVE";
          }
        }

        if (IsEmpty(export.SearchContractor.Code))
        {
        }
        else
        {
          if (ReadCseOrganization())
          {
            MoveCseOrganization(entities.CseOrganization,
              export.SearchContractor);
          }
          else
          {
            export.SearchContractor.Name = "";

            var field1 = GetField(export.SearchContractor, "code");

            field1.Error = true;

            ExitState = "LE0000_CONTRACTOR_NOT_FOUND";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!IsEmpty(export.SearchContractor.Code))
        {
          if (!IsEmpty(export.SearchIwoTransaction.TransactionNumber))
          {
            var field1 =
              GetField(export.SearchIwoTransaction, "transactionNumber");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_1";
          }

          if (!IsEmpty(export.SearchCsePerson.Number))
          {
            var field1 = GetField(export.SearchCsePerson, "number");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_1";
          }

          if (!IsEmpty(export.SearchServiceProvider.UserId))
          {
            var field1 = GetField(export.SearchServiceProvider, "userId");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_1";
          }

          if (export.SearchOffice.SystemGeneratedId != 0)
          {
            var field1 = GetField(export.SearchOffice, "systemGeneratedId");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_1";
          }
        }
        else if (export.SearchOffice.SystemGeneratedId != 0)
        {
          if (!IsEmpty(export.SearchIwoTransaction.TransactionNumber))
          {
            var field1 =
              GetField(export.SearchIwoTransaction, "transactionNumber");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_2";
          }

          if (!IsEmpty(export.SearchCsePerson.Number))
          {
            var field1 = GetField(export.SearchCsePerson, "number");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_2";
          }

          if (!IsEmpty(export.SearchContractor.Code))
          {
            var field1 = GetField(export.SearchContractor, "code");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_2";
          }
        }
        else if (!IsEmpty(export.SearchServiceProvider.UserId))
        {
          if (!IsEmpty(export.SearchIwoTransaction.TransactionNumber))
          {
            var field1 =
              GetField(export.SearchIwoTransaction, "transactionNumber");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_3";
          }

          if (!IsEmpty(export.SearchCsePerson.Number))
          {
            var field1 = GetField(export.SearchCsePerson, "number");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_3";
          }

          if (!IsEmpty(export.SearchContractor.Code))
          {
            var field1 = GetField(export.SearchContractor, "code");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_3";
          }
        }
        else if (!IsEmpty(export.SearchCsePerson.Number))
        {
          if (!IsEmpty(export.SearchIwoTransaction.TransactionNumber))
          {
            var field1 =
              GetField(export.SearchIwoTransaction, "transactionNumber");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_4";
          }

          if (!IsEmpty(export.SearchServiceProvider.UserId))
          {
            var field1 = GetField(export.SearchServiceProvider, "userId");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_4";
          }

          if (export.SearchOffice.SystemGeneratedId != 0)
          {
            var field1 = GetField(export.SearchOffice, "systemGeneratedId");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_4";
          }

          if (!IsEmpty(export.SearchContractor.Code))
          {
            var field1 = GetField(export.SearchContractor, "code");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_4";
          }
        }
        else if (!IsEmpty(export.SearchIwoTransaction.TransactionNumber))
        {
          if (!IsEmpty(export.SearchCsePerson.Number))
          {
            var field1 = GetField(export.SearchCsePerson, "number");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_5";
          }

          if (!IsEmpty(export.SearchServiceProvider.UserId))
          {
            var field1 = GetField(export.SearchServiceProvider, "userId");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_5";
          }

          if (export.SearchOffice.SystemGeneratedId != 0)
          {
            var field1 = GetField(export.SearchOffice, "systemGeneratedId");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_5";
          }

          if (!IsEmpty(export.SearchContractor.Code))
          {
            var field1 = GetField(export.SearchContractor, "code");

            field1.Error = true;

            ExitState = "LE0000_EIWL_INVALID_SEARCH_5";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseLeEiwlDisplayLog1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (Equal(local.NextPage.StatusDate, local.Null1.Date))
        {
          export.MoreIndicator.Text9 = "MORE:";
        }
        else
        {
          export.Paging.Index = export.PageNumber.Count;
          export.Paging.CheckSize();

          MoveIwoTransaction3(local.NextPage, export.Paging.Update.GexportPaging);
            
          export.MoreIndicator.Text9 = "MORE:   +";
        }

        export.HiddenSearchCsePerson.Number = export.SearchCsePerson.Number;
        export.HiddenSearchContractor.Code = export.SearchContractor.Code;
        MoveIwoTransaction2(export.SearchIwoTransaction,
          export.HiddenSearchIwoTransaction);
        export.HiddenSearchOffice.SystemGeneratedId =
          export.SearchOffice.SystemGeneratedId;
        export.HiddenSearchServiceProvider.UserId =
          export.SearchServiceProvider.UserId;
        export.HiddenSearchSeverity.Text7 = export.SearchSeverity.Text7;

        if (export.Export1.Count == 0)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "LIST":
        if (AsChar(export.PromptSeverity.SelectChar) == 'S')
        {
          export.ToCdvl.CodeName = "IWO SEVERITY";

          if (!IsEmpty(export.SearchIwoTransaction.CurrentStatus))
          {
            export.ToCdvlCombinationCode.CodeName = "IWO STATUS";
            export.ToCdvlCombinationCodeValue.Cdvalue =
              export.SearchIwoTransaction.CurrentStatus ?? Spaces(10);
          }

          ExitState = "ECO_LNK_TO_CDVL";
        }
        else
        {
        }

        if (AsChar(export.PromptStatus.SelectChar) == 'S')
        {
          export.ToCdvl.CodeName = "IWO STATUS";
          ExitState = "ECO_LNK_TO_CDVL";
        }
        else
        {
        }

        if (AsChar(export.PromptPerson.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_NAME";
        }
        else
        {
        }

        if (AsChar(export.PromptWorker.SelectChar) == 'S')
        {
          if (export.SearchOffice.SystemGeneratedId == 0)
          {
            ExitState = "ECO_LNK_TO_SVPL";
          }
          else
          {
            ExitState = "ECO_LNK_TO_SVPO";
          }
        }
        else
        {
        }

        if (AsChar(export.PromptOffice.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_OFCL";
        }
        else
        {
        }

        if (AsChar(export.PromptContractor.SelectChar) == 'S')
        {
          export.ToCsor.Type1 = "X";
          ExitState = "ECO_LNK_TO_CSOR";
        }
        else
        {
        }

        return;
      case "EIWH":
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
            if (!Equal(export.Export1.Item.GiwoAction.ActionType, "E-IWO") && !
              Equal(export.Export1.Item.GiwoAction.ActionType, "RESUB"))
            {
              var field1 =
                GetField(export.Export1.Item.GiwoAction, "actionType");

              field1.Error = true;

              ExitState = "LE0000_EIWO_INVALID_EIWH_ACTION";

              return;
            }

            if (Equal(export.Export1.Item.GincomeSource.Name,
              "** FAMILY VIOLENCE SET **"))
            {
              var field1 =
                GetField(export.Export1.Item.GiwoAction, "actionType");

              field1.Error = true;

              ExitState = "LE0000_NO_EIWH_DUE_TO_FV";

              return;
            }

            if (ReadFieldValue())
            {
              export.ToEiwhCase.Number = entities.FieldValue.Value ?? Spaces
                (10);
            }

            export.ToEiwhCsePersonsWorkSet.Number =
              export.Export1.Item.GcsePerson.Number;
            UseSiReadCsePerson();
            MoveIwoTransaction1(export.Export1.Item.GiwoTransaction,
              export.ToEiwhIwoTransaction);
            MoveIncomeSource(export.Export1.Item.GincomeSource,
              export.ToEiwhIncomeSource);
            export.ToEiwhLegalAction.Assign(export.Export1.Item.GlegalAction);
            export.Export1.Update.Gcommon.SelectChar = "";
            ExitState = "ECO_LNK_TO_EIWH";
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

      switch(TrimEnd(export.Export1.Item.GexportSeverity.Text7))
      {
        case "RED":
          var field1 =
            GetField(export.Export1.Item.Goffice, "systemGeneratedId");

          field1.Color = "red";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.GserviceProvider, "userId");

          field2.Color = "red";
          field2.Protected = true;

          var field3 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field3.Color = "red";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.GincomeSource, "name");

          field4.Color = "red";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.GlegalAction, "actionTaken");

          field5.Color = "red";
          field5.Protected = true;

          var field6 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field6.Color = "red";
          field6.Protected = true;

          var field7 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field7.Color = "red";
          field7.Protected = true;

          break;
        case "YELLOW":
          var field8 =
            GetField(export.Export1.Item.Goffice, "systemGeneratedId");

          field8.Color = "yellow";
          field8.Protected = true;

          var field9 = GetField(export.Export1.Item.GserviceProvider, "userId");

          field9.Color = "yellow";
          field9.Protected = true;

          var field10 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field10.Color = "yellow";
          field10.Protected = true;

          var field11 = GetField(export.Export1.Item.GincomeSource, "name");

          field11.Color = "yellow";
          field11.Protected = true;

          var field12 =
            GetField(export.Export1.Item.GlegalAction, "actionTaken");

          field12.Color = "yellow";
          field12.Protected = true;

          var field13 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field13.Color = "yellow";
          field13.Protected = true;

          var field14 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field14.Color = "yellow";
          field14.Protected = true;

          break;
        default:
          var field15 =
            GetField(export.Export1.Item.Goffice, "systemGeneratedId");

          field15.Protected = true;

          var field16 =
            GetField(export.Export1.Item.GserviceProvider, "userId");

          field16.Protected = true;

          var field17 =
            GetField(export.Export1.Item.GiwoTransaction, "transactionNumber");

          field17.Protected = true;

          var field18 = GetField(export.Export1.Item.GincomeSource, "name");

          field18.Protected = true;

          var field19 =
            GetField(export.Export1.Item.GlegalAction, "actionTaken");

          field19.Protected = true;

          var field20 = GetField(export.Export1.Item.GiwoAction, "statusCd");

          field20.Protected = true;

          var field21 = GetField(export.Export1.Item.GiwoAction, "statusDate");

          field21.Protected = true;

          break;
      }
    }

    export.Export1.CheckIndex();
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveExport1(LeEiwlDisplayLog.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.Gcommon.SelectChar = source.Gcommon.SelectChar;
    target.Goffice.SystemGeneratedId = source.Goffice.SystemGeneratedId;
    target.GserviceProvider.UserId = source.GserviceProvider.UserId;
    MoveIwoTransaction1(source.GiwoTransaction, target.GiwoTransaction);
    MoveIncomeSource(source.GincomeSource, target.GincomeSource);
    target.GlegalAction.Assign(source.GlegalAction);
    target.GiwoAction.Assign(source.GiwoAction);
    target.GexportSeverity.Text7 = source.GexportSeverity.Text7;
    target.GcsePerson.Number = source.GcsePerson.Number;
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveIwoTransaction1(IwoTransaction source,
    IwoTransaction target)
  {
    target.Identifier = source.Identifier;
    target.TransactionNumber = source.TransactionNumber;
  }

  private static void MoveIwoTransaction2(IwoTransaction source,
    IwoTransaction target)
  {
    target.TransactionNumber = source.TransactionNumber;
    target.CurrentStatus = source.CurrentStatus;
  }

  private static void MoveIwoTransaction3(IwoTransaction source,
    IwoTransaction target)
  {
    target.StatusDate = source.StatusDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CrossValidationCodeValue.Cdvalue =
      local.CrossValidationCodeValue.Cdvalue;
    useImport.CrossValidationCode.CodeName = local.CrossValidationCode.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    local.CodeValue.Assign(useExport.CodeValue);
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    local.CodeValue.Assign(useExport.CodeValue);
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = export.SearchCsePerson.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.SearchCsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseLeEiwlDisplayLog1()
  {
    var useImport = new LeEiwlDisplayLog.Import();
    var useExport = new LeEiwlDisplayLog.Export();

    MoveIwoTransaction3(export.Paging.Item.GexportPaging, useImport.Paging);
    useImport.EiwoAgingCutoffDate.Date = local.EiwoAgingCutoffDate.Date;
    useImport.SearchContractor.Code = export.SearchContractor.Code;
    useImport.SearchOffice.SystemGeneratedId =
      export.SearchOffice.SystemGeneratedId;
    useImport.SearchServiceProvider.UserId =
      export.SearchServiceProvider.UserId;
    useImport.SearchCsePerson.Number = export.SearchCsePerson.Number;
    MoveIwoTransaction2(export.SearchIwoTransaction,
      useImport.SearchIwoTransaction);
    useImport.SearchSeverity.Text7 = export.SearchSeverity.Text7;

    Call(LeEiwlDisplayLog.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.SeverityRed.Count = useExport.SeverityRed.Count;
    export.SeverityYellow.Count = useExport.SeverityYellow.Count;
    export.SeverityDefault.Count = useExport.SeverityDefault.Count;
    export.SeverityTotal.Count = useExport.SeverityTotal.Count;
    MoveIwoTransaction3(useExport.NextPage, local.NextPage);
  }

  private void UseLeEiwlDisplayLog2()
  {
    var useImport = new LeEiwlDisplayLog.Import();
    var useExport = new LeEiwlDisplayLog.Export();

    MoveIwoTransaction3(export.Paging.Item.GexportPaging, useImport.Paging);
    useImport.EiwoAgingCutoffDate.Date = local.EiwoAgingCutoffDate.Date;
    useImport.SearchContractor.Code = export.SearchContractor.Code;
    useImport.SearchOffice.SystemGeneratedId =
      export.SearchOffice.SystemGeneratedId;
    useImport.SearchServiceProvider.UserId =
      export.SearchServiceProvider.UserId;
    useImport.SearchCsePerson.Number = export.SearchCsePerson.Number;
    MoveIwoTransaction2(export.SearchIwoTransaction,
      useImport.SearchIwoTransaction);
    useImport.SearchSeverity.Text7 = export.SearchSeverity.Text7;

    Call(LeEiwlDisplayLog.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    MoveIwoTransaction3(useExport.NextPage, local.NextPage);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ToEiwhCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.ToEiwhCsePersonsWorkSet);
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

  private bool ReadCseOrganization()
  {
    entities.CseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "organztnId", export.SearchContractor.Code);
      },
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Name = db.GetString(reader, 2);
        entities.CseOrganization.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SearchCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadFieldValue()
  {
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", export.Export1.Item.GiwoAction.Identifier);
        db.SetInt32(
          command, "iwtIdentifier",
          export.Export1.Item.GiwoTransaction.Identifier);
        db.
          SetString(command, "cspNumber", export.Export1.Item.GcsePerson.Number);
          
        db.SetInt32(
          command, "lgaIdentifier",
          export.Export1.Item.GlegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.SearchOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", export.SearchServiceProvider.UserId);
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
      /// A value of GcsePerson.
      /// </summary>
      [JsonPropertyName("gcsePerson")]
      public CsePerson GcsePerson
      {
        get => gcsePerson ??= new();
        set => gcsePerson = value;
      }

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
      /// A value of Goffice.
      /// </summary>
      [JsonPropertyName("goffice")]
      public Office Goffice
      {
        get => goffice ??= new();
        set => goffice = value;
      }

      /// <summary>
      /// A value of GserviceProvider.
      /// </summary>
      [JsonPropertyName("gserviceProvider")]
      public ServiceProvider GserviceProvider
      {
        get => gserviceProvider ??= new();
        set => gserviceProvider = value;
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
      /// A value of GincomeSource.
      /// </summary>
      [JsonPropertyName("gincomeSource")]
      public IncomeSource GincomeSource
      {
        get => gincomeSource ??= new();
        set => gincomeSource = value;
      }

      /// <summary>
      /// A value of GlegalAction.
      /// </summary>
      [JsonPropertyName("glegalAction")]
      public LegalAction GlegalAction
      {
        get => glegalAction ??= new();
        set => glegalAction = value;
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

      /// <summary>
      /// A value of GimportSeverity.
      /// </summary>
      [JsonPropertyName("gimportSeverity")]
      public WorkArea GimportSeverity
      {
        get => gimportSeverity ??= new();
        set => gimportSeverity = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePerson gcsePerson;
      private Common gcommon;
      private Office goffice;
      private ServiceProvider gserviceProvider;
      private IwoTransaction giwoTransaction;
      private IncomeSource gincomeSource;
      private LegalAction glegalAction;
      private IwoAction giwoAction;
      private WorkArea gimportSeverity;
    }

    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of GimportPaging.
      /// </summary>
      [JsonPropertyName("gimportPaging")]
      public IwoTransaction GimportPaging
      {
        get => gimportPaging ??= new();
        set => gimportPaging = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private IwoTransaction gimportPaging;
    }

    /// <summary>
    /// A value of MoreIndicator.
    /// </summary>
    [JsonPropertyName("moreIndicator")]
    public WorkArea MoreIndicator
    {
      get => moreIndicator ??= new();
      set => moreIndicator = value;
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
    /// A value of SearchContractor.
    /// </summary>
    [JsonPropertyName("searchContractor")]
    public CseOrganization SearchContractor
    {
      get => searchContractor ??= new();
      set => searchContractor = value;
    }

    /// <summary>
    /// A value of SearchOffice.
    /// </summary>
    [JsonPropertyName("searchOffice")]
    public Office SearchOffice
    {
      get => searchOffice ??= new();
      set => searchOffice = value;
    }

    /// <summary>
    /// A value of SearchServiceProvider.
    /// </summary>
    [JsonPropertyName("searchServiceProvider")]
    public ServiceProvider SearchServiceProvider
    {
      get => searchServiceProvider ??= new();
      set => searchServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchWorker.
    /// </summary>
    [JsonPropertyName("searchWorker")]
    public CsePersonsWorkSet SearchWorker
    {
      get => searchWorker ??= new();
      set => searchWorker = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchIwoTransaction.
    /// </summary>
    [JsonPropertyName("searchIwoTransaction")]
    public IwoTransaction SearchIwoTransaction
    {
      get => searchIwoTransaction ??= new();
      set => searchIwoTransaction = value;
    }

    /// <summary>
    /// A value of SearchSeverity.
    /// </summary>
    [JsonPropertyName("searchSeverity")]
    public WorkArea SearchSeverity
    {
      get => searchSeverity ??= new();
      set => searchSeverity = value;
    }

    /// <summary>
    /// A value of SearchSeverityCode.
    /// </summary>
    [JsonPropertyName("searchSeverityCode")]
    public CodeValue SearchSeverityCode
    {
      get => searchSeverityCode ??= new();
      set => searchSeverityCode = value;
    }

    /// <summary>
    /// A value of SearchStatusCode.
    /// </summary>
    [JsonPropertyName("searchStatusCode")]
    public CodeValue SearchStatusCode
    {
      get => searchStatusCode ??= new();
      set => searchStatusCode = value;
    }

    /// <summary>
    /// A value of PromptSeverity.
    /// </summary>
    [JsonPropertyName("promptSeverity")]
    public Common PromptSeverity
    {
      get => promptSeverity ??= new();
      set => promptSeverity = value;
    }

    /// <summary>
    /// A value of PromptStatus.
    /// </summary>
    [JsonPropertyName("promptStatus")]
    public Common PromptStatus
    {
      get => promptStatus ??= new();
      set => promptStatus = value;
    }

    /// <summary>
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of PromptWorker.
    /// </summary>
    [JsonPropertyName("promptWorker")]
    public Common PromptWorker
    {
      get => promptWorker ??= new();
      set => promptWorker = value;
    }

    /// <summary>
    /// A value of PromptOffice.
    /// </summary>
    [JsonPropertyName("promptOffice")]
    public Common PromptOffice
    {
      get => promptOffice ??= new();
      set => promptOffice = value;
    }

    /// <summary>
    /// A value of PromptContractor.
    /// </summary>
    [JsonPropertyName("promptContractor")]
    public Common PromptContractor
    {
      get => promptContractor ??= new();
      set => promptContractor = value;
    }

    /// <summary>
    /// A value of SeverityTotal.
    /// </summary>
    [JsonPropertyName("severityTotal")]
    public Common SeverityTotal
    {
      get => severityTotal ??= new();
      set => severityTotal = value;
    }

    /// <summary>
    /// A value of SeverityDefault.
    /// </summary>
    [JsonPropertyName("severityDefault")]
    public Common SeverityDefault
    {
      get => severityDefault ??= new();
      set => severityDefault = value;
    }

    /// <summary>
    /// A value of SeverityYellow.
    /// </summary>
    [JsonPropertyName("severityYellow")]
    public Common SeverityYellow
    {
      get => severityYellow ??= new();
      set => severityYellow = value;
    }

    /// <summary>
    /// A value of SeverityRed.
    /// </summary>
    [JsonPropertyName("severityRed")]
    public Common SeverityRed
    {
      get => severityRed ??= new();
      set => severityRed = value;
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
    /// A value of HiddenSearchContractor.
    /// </summary>
    [JsonPropertyName("hiddenSearchContractor")]
    public CseOrganization HiddenSearchContractor
    {
      get => hiddenSearchContractor ??= new();
      set => hiddenSearchContractor = value;
    }

    /// <summary>
    /// A value of HiddenSearchOffice.
    /// </summary>
    [JsonPropertyName("hiddenSearchOffice")]
    public Office HiddenSearchOffice
    {
      get => hiddenSearchOffice ??= new();
      set => hiddenSearchOffice = value;
    }

    /// <summary>
    /// A value of HiddenSearchServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSearchServiceProvider")]
    public ServiceProvider HiddenSearchServiceProvider
    {
      get => hiddenSearchServiceProvider ??= new();
      set => hiddenSearchServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenSearchCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenSearchCsePerson")]
    public CsePerson HiddenSearchCsePerson
    {
      get => hiddenSearchCsePerson ??= new();
      set => hiddenSearchCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenSearchIwoTransaction.
    /// </summary>
    [JsonPropertyName("hiddenSearchIwoTransaction")]
    public IwoTransaction HiddenSearchIwoTransaction
    {
      get => hiddenSearchIwoTransaction ??= new();
      set => hiddenSearchIwoTransaction = value;
    }

    /// <summary>
    /// A value of HiddenSearchSeverity.
    /// </summary>
    [JsonPropertyName("hiddenSearchSeverity")]
    public WorkArea HiddenSearchSeverity
    {
      get => hiddenSearchSeverity ??= new();
      set => hiddenSearchSeverity = value;
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
    /// A value of FromOfclAndSvpo.
    /// </summary>
    [JsonPropertyName("fromOfclAndSvpo")]
    public Office FromOfclAndSvpo
    {
      get => fromOfclAndSvpo ??= new();
      set => fromOfclAndSvpo = value;
    }

    /// <summary>
    /// A value of FromSvplAndSvpo.
    /// </summary>
    [JsonPropertyName("fromSvplAndSvpo")]
    public ServiceProvider FromSvplAndSvpo
    {
      get => fromSvplAndSvpo ??= new();
      set => fromSvplAndSvpo = value;
    }

    /// <summary>
    /// A value of FromName.
    /// </summary>
    [JsonPropertyName("fromName")]
    public CsePersonsWorkSet FromName
    {
      get => fromName ??= new();
      set => fromName = value;
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
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Common PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    private WorkArea moreIndicator;
    private Standard standard;
    private CseOrganization searchContractor;
    private Office searchOffice;
    private ServiceProvider searchServiceProvider;
    private CsePersonsWorkSet searchWorker;
    private CsePerson searchCsePerson;
    private IwoTransaction searchIwoTransaction;
    private WorkArea searchSeverity;
    private CodeValue searchSeverityCode;
    private CodeValue searchStatusCode;
    private Common promptSeverity;
    private Common promptStatus;
    private Common promptPerson;
    private Common promptWorker;
    private Common promptOffice;
    private Common promptContractor;
    private Common severityTotal;
    private Common severityDefault;
    private Common severityYellow;
    private Common severityRed;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private CseOrganization hiddenSearchContractor;
    private Office hiddenSearchOffice;
    private ServiceProvider hiddenSearchServiceProvider;
    private CsePerson hiddenSearchCsePerson;
    private IwoTransaction hiddenSearchIwoTransaction;
    private WorkArea hiddenSearchSeverity;
    private CseOrganization fromCsor;
    private Office fromOfclAndSvpo;
    private ServiceProvider fromSvplAndSvpo;
    private CsePersonsWorkSet fromName;
    private CodeValue fromCdvl;
    private Array<PagingGroup> paging;
    private Common pageNumber;
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
      /// A value of Goffice.
      /// </summary>
      [JsonPropertyName("goffice")]
      public Office Goffice
      {
        get => goffice ??= new();
        set => goffice = value;
      }

      /// <summary>
      /// A value of GserviceProvider.
      /// </summary>
      [JsonPropertyName("gserviceProvider")]
      public ServiceProvider GserviceProvider
      {
        get => gserviceProvider ??= new();
        set => gserviceProvider = value;
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
      /// A value of GincomeSource.
      /// </summary>
      [JsonPropertyName("gincomeSource")]
      public IncomeSource GincomeSource
      {
        get => gincomeSource ??= new();
        set => gincomeSource = value;
      }

      /// <summary>
      /// A value of GlegalAction.
      /// </summary>
      [JsonPropertyName("glegalAction")]
      public LegalAction GlegalAction
      {
        get => glegalAction ??= new();
        set => glegalAction = value;
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

      /// <summary>
      /// A value of GexportSeverity.
      /// </summary>
      [JsonPropertyName("gexportSeverity")]
      public WorkArea GexportSeverity
      {
        get => gexportSeverity ??= new();
        set => gexportSeverity = value;
      }

      /// <summary>
      /// A value of GcsePerson.
      /// </summary>
      [JsonPropertyName("gcsePerson")]
      public CsePerson GcsePerson
      {
        get => gcsePerson ??= new();
        set => gcsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common gcommon;
      private Office goffice;
      private ServiceProvider gserviceProvider;
      private IwoTransaction giwoTransaction;
      private IncomeSource gincomeSource;
      private LegalAction glegalAction;
      private IwoAction giwoAction;
      private WorkArea gexportSeverity;
      private CsePerson gcsePerson;
    }

    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of GexportPaging.
      /// </summary>
      [JsonPropertyName("gexportPaging")]
      public IwoTransaction GexportPaging
      {
        get => gexportPaging ??= new();
        set => gexportPaging = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private IwoTransaction gexportPaging;
    }

    /// <summary>
    /// A value of ToEiwhCase.
    /// </summary>
    [JsonPropertyName("toEiwhCase")]
    public Case1 ToEiwhCase
    {
      get => toEiwhCase ??= new();
      set => toEiwhCase = value;
    }

    /// <summary>
    /// A value of ToEiwhCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("toEiwhCsePersonsWorkSet")]
    public CsePersonsWorkSet ToEiwhCsePersonsWorkSet
    {
      get => toEiwhCsePersonsWorkSet ??= new();
      set => toEiwhCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ToEiwhLegalAction.
    /// </summary>
    [JsonPropertyName("toEiwhLegalAction")]
    public LegalAction ToEiwhLegalAction
    {
      get => toEiwhLegalAction ??= new();
      set => toEiwhLegalAction = value;
    }

    /// <summary>
    /// A value of MoreIndicator.
    /// </summary>
    [JsonPropertyName("moreIndicator")]
    public WorkArea MoreIndicator
    {
      get => moreIndicator ??= new();
      set => moreIndicator = value;
    }

    /// <summary>
    /// A value of ToCdvlCombinationCode.
    /// </summary>
    [JsonPropertyName("toCdvlCombinationCode")]
    public Code ToCdvlCombinationCode
    {
      get => toCdvlCombinationCode ??= new();
      set => toCdvlCombinationCode = value;
    }

    /// <summary>
    /// A value of ToCdvlCombinationCodeValue.
    /// </summary>
    [JsonPropertyName("toCdvlCombinationCodeValue")]
    public CodeValue ToCdvlCombinationCodeValue
    {
      get => toCdvlCombinationCodeValue ??= new();
      set => toCdvlCombinationCodeValue = value;
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
    /// A value of SearchContractor.
    /// </summary>
    [JsonPropertyName("searchContractor")]
    public CseOrganization SearchContractor
    {
      get => searchContractor ??= new();
      set => searchContractor = value;
    }

    /// <summary>
    /// A value of SearchOffice.
    /// </summary>
    [JsonPropertyName("searchOffice")]
    public Office SearchOffice
    {
      get => searchOffice ??= new();
      set => searchOffice = value;
    }

    /// <summary>
    /// A value of SearchServiceProvider.
    /// </summary>
    [JsonPropertyName("searchServiceProvider")]
    public ServiceProvider SearchServiceProvider
    {
      get => searchServiceProvider ??= new();
      set => searchServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchWorker.
    /// </summary>
    [JsonPropertyName("searchWorker")]
    public CsePersonsWorkSet SearchWorker
    {
      get => searchWorker ??= new();
      set => searchWorker = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchIwoTransaction.
    /// </summary>
    [JsonPropertyName("searchIwoTransaction")]
    public IwoTransaction SearchIwoTransaction
    {
      get => searchIwoTransaction ??= new();
      set => searchIwoTransaction = value;
    }

    /// <summary>
    /// A value of SearchSeverity.
    /// </summary>
    [JsonPropertyName("searchSeverity")]
    public WorkArea SearchSeverity
    {
      get => searchSeverity ??= new();
      set => searchSeverity = value;
    }

    /// <summary>
    /// A value of SearchSeverityCode.
    /// </summary>
    [JsonPropertyName("searchSeverityCode")]
    public CodeValue SearchSeverityCode
    {
      get => searchSeverityCode ??= new();
      set => searchSeverityCode = value;
    }

    /// <summary>
    /// A value of SearchStatusCode.
    /// </summary>
    [JsonPropertyName("searchStatusCode")]
    public CodeValue SearchStatusCode
    {
      get => searchStatusCode ??= new();
      set => searchStatusCode = value;
    }

    /// <summary>
    /// A value of PromptSeverity.
    /// </summary>
    [JsonPropertyName("promptSeverity")]
    public Common PromptSeverity
    {
      get => promptSeverity ??= new();
      set => promptSeverity = value;
    }

    /// <summary>
    /// A value of PromptStatus.
    /// </summary>
    [JsonPropertyName("promptStatus")]
    public Common PromptStatus
    {
      get => promptStatus ??= new();
      set => promptStatus = value;
    }

    /// <summary>
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of PromptWorker.
    /// </summary>
    [JsonPropertyName("promptWorker")]
    public Common PromptWorker
    {
      get => promptWorker ??= new();
      set => promptWorker = value;
    }

    /// <summary>
    /// A value of PromptOffice.
    /// </summary>
    [JsonPropertyName("promptOffice")]
    public Common PromptOffice
    {
      get => promptOffice ??= new();
      set => promptOffice = value;
    }

    /// <summary>
    /// A value of PromptContractor.
    /// </summary>
    [JsonPropertyName("promptContractor")]
    public Common PromptContractor
    {
      get => promptContractor ??= new();
      set => promptContractor = value;
    }

    /// <summary>
    /// A value of SeverityTotal.
    /// </summary>
    [JsonPropertyName("severityTotal")]
    public Common SeverityTotal
    {
      get => severityTotal ??= new();
      set => severityTotal = value;
    }

    /// <summary>
    /// A value of SeverityDefault.
    /// </summary>
    [JsonPropertyName("severityDefault")]
    public Common SeverityDefault
    {
      get => severityDefault ??= new();
      set => severityDefault = value;
    }

    /// <summary>
    /// A value of SeverityYellow.
    /// </summary>
    [JsonPropertyName("severityYellow")]
    public Common SeverityYellow
    {
      get => severityYellow ??= new();
      set => severityYellow = value;
    }

    /// <summary>
    /// A value of SeverityRed.
    /// </summary>
    [JsonPropertyName("severityRed")]
    public Common SeverityRed
    {
      get => severityRed ??= new();
      set => severityRed = value;
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
    /// A value of ToEiwhIncomeSource.
    /// </summary>
    [JsonPropertyName("toEiwhIncomeSource")]
    public IncomeSource ToEiwhIncomeSource
    {
      get => toEiwhIncomeSource ??= new();
      set => toEiwhIncomeSource = value;
    }

    /// <summary>
    /// A value of HiddenSearchContractor.
    /// </summary>
    [JsonPropertyName("hiddenSearchContractor")]
    public CseOrganization HiddenSearchContractor
    {
      get => hiddenSearchContractor ??= new();
      set => hiddenSearchContractor = value;
    }

    /// <summary>
    /// A value of HiddenSearchOffice.
    /// </summary>
    [JsonPropertyName("hiddenSearchOffice")]
    public Office HiddenSearchOffice
    {
      get => hiddenSearchOffice ??= new();
      set => hiddenSearchOffice = value;
    }

    /// <summary>
    /// A value of HiddenSearchServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSearchServiceProvider")]
    public ServiceProvider HiddenSearchServiceProvider
    {
      get => hiddenSearchServiceProvider ??= new();
      set => hiddenSearchServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenSearchCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenSearchCsePerson")]
    public CsePerson HiddenSearchCsePerson
    {
      get => hiddenSearchCsePerson ??= new();
      set => hiddenSearchCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenSearchIwoTransaction.
    /// </summary>
    [JsonPropertyName("hiddenSearchIwoTransaction")]
    public IwoTransaction HiddenSearchIwoTransaction
    {
      get => hiddenSearchIwoTransaction ??= new();
      set => hiddenSearchIwoTransaction = value;
    }

    /// <summary>
    /// A value of HiddenSearchSeverity.
    /// </summary>
    [JsonPropertyName("hiddenSearchSeverity")]
    public WorkArea HiddenSearchSeverity
    {
      get => hiddenSearchSeverity ??= new();
      set => hiddenSearchSeverity = value;
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
    /// A value of ToCsor.
    /// </summary>
    [JsonPropertyName("toCsor")]
    public CseOrganization ToCsor
    {
      get => toCsor ??= new();
      set => toCsor = value;
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Common PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
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

    private Case1 toEiwhCase;
    private CsePersonsWorkSet toEiwhCsePersonsWorkSet;
    private LegalAction toEiwhLegalAction;
    private WorkArea moreIndicator;
    private Code toCdvlCombinationCode;
    private CodeValue toCdvlCombinationCodeValue;
    private Standard standard;
    private CseOrganization searchContractor;
    private Office searchOffice;
    private ServiceProvider searchServiceProvider;
    private CsePersonsWorkSet searchWorker;
    private CsePerson searchCsePerson;
    private IwoTransaction searchIwoTransaction;
    private WorkArea searchSeverity;
    private CodeValue searchSeverityCode;
    private CodeValue searchStatusCode;
    private Common promptSeverity;
    private Common promptStatus;
    private Common promptPerson;
    private Common promptWorker;
    private Common promptOffice;
    private Common promptContractor;
    private Common severityTotal;
    private Common severityDefault;
    private Common severityYellow;
    private Common severityRed;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private IwoTransaction toEiwhIwoTransaction;
    private IncomeSource toEiwhIncomeSource;
    private CseOrganization hiddenSearchContractor;
    private Office hiddenSearchOffice;
    private ServiceProvider hiddenSearchServiceProvider;
    private CsePerson hiddenSearchCsePerson;
    private IwoTransaction hiddenSearchIwoTransaction;
    private WorkArea hiddenSearchSeverity;
    private Code toCdvl;
    private CseOrganization toCsor;
    private Common pageNumber;
    private Array<PagingGroup> paging;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public Common Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of NextPage.
    /// </summary>
    [JsonPropertyName("nextPage")]
    public IwoTransaction NextPage
    {
      get => nextPage ??= new();
      set => nextPage = value;
    }

    /// <summary>
    /// A value of CrossValidationCodeValue.
    /// </summary>
    [JsonPropertyName("crossValidationCodeValue")]
    public CodeValue CrossValidationCodeValue
    {
      get => crossValidationCodeValue ??= new();
      set => crossValidationCodeValue = value;
    }

    /// <summary>
    /// A value of CrossValidationCode.
    /// </summary>
    [JsonPropertyName("crossValidationCode")]
    public Code CrossValidationCode
    {
      get => crossValidationCode ??= new();
      set => crossValidationCode = value;
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
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common convert;
    private IwoTransaction nextPage;
    private CodeValue crossValidationCodeValue;
    private Code crossValidationCode;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Common promptCount;
    private DateWorkArea null1;
    private DateWorkArea eiwoAgingCutoffDate;
    private Common common;
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
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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

    private LegalAction legalAction;
    private IwoTransaction iwoTransaction;
    private IwoAction iwoAction;
    private DocumentField documentField;
    private OutgoingDocument outgoingDocument;
    private Field field;
    private FieldValue fieldValue;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private CseOrganization cseOrganization;
    private CsePerson csePerson;
    private CodeValue codeValue;
    private Code code;
  }
#endregion
}
