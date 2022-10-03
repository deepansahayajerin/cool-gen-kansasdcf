// Program: LE_EIWH_EIWO_HISTORY, ID: 1902506115, model: 746.
// Short name: SWEEIWHP
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
/// A program: LE_EIWH_EIWO_HISTORY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeEiwhEiwoHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EIWH_EIWO_HISTORY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEiwhEiwoHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEiwhEiwoHistory.
  /// </summary>
  public LeEiwhEiwoHistory(IContext context, Import import, Export export):
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
    // 08/20/15  GVandy	CQ22212		Initial Code.  Created from a copy of LAIS.
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.LegalAction.Assign(import.LegalAction);
    MoveIncomeSource(import.IncomeSource, export.IncomeSource);
    export.IwoTransaction.Assign(import.IwoTransaction);
    export.Case1.Number = import.Case1.Number;
    export.HiddenIwoTransaction.Note = import.HiddenIwoTransaction.Note;
    export.HiddenMostRecent.Assign(import.HiddenMostRecent);

    // -- Set display colors.
    local.Severity.Text8 = "";

    if (AsChar(export.HiddenMostRecent.SeverityClearedInd) == 'Y')
    {
      // -- The severity has been cleared.
      var field1 = GetField(export.IwoTransaction, "transactionNumber");

      field1.Protected = true;
    }
    else
    {
      switch(AsChar(export.HiddenMostRecent.StatusCd))
      {
        case 'S':
          // -- These are submitted eIWO transactions.  Set color based on the 
          // aging cutoff date.
          if (!Lt(local.EiwoAgingCutoffDate.Date,
            export.HiddenMostRecent.StatusDate))
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "red";
            field2.Protected = true;

            local.Severity.Text8 = "RED";
          }
          else
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Protected = true;
          }

          break;
        case 'N':
          // -- These are sent eIWO transactions.  Set color based on the aging 
          // cutoff date.
          if (!Lt(local.EiwoAgingCutoffDate.Date,
            export.HiddenMostRecent.StatusDate))
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "red";
            field2.Protected = true;

            local.Severity.Text8 = "RED";
          }
          else
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "yellow";
            field2.Protected = true;

            local.Severity.Text8 = "YELLOW";
          }

          break;
        case 'R':
          // -- These are receipted eIWO transactions.  Set color based on the 
          // aging cutoff date.
          if (!Lt(local.EiwoAgingCutoffDate.Date,
            export.HiddenMostRecent.StatusDate))
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "red";
            field2.Protected = true;

            local.Severity.Text8 = "RED";
          }
          else
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "yellow";
            field2.Protected = true;

            local.Severity.Text8 = "YELLOW";
          }

          break;
        case 'E':
          // -- These are errored eIWO transactions.
          var field1 = GetField(export.IwoTransaction, "transactionNumber");

          field1.Color = "red";
          field1.Protected = true;

          local.Severity.Text8 = "RED";

          break;
        case 'J':
          // -- These are rejected eIWO transactions.
          if (Equal(export.HiddenMostRecent.StatusReasonCode, "N") || Equal
            (export.HiddenMostRecent.StatusReasonCode, "U"))
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Protected = true;
          }
          else
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "red";
            field2.Protected = true;

            local.Severity.Text8 = "RED";
          }

          break;
        default:
          break;
      }
    }

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

      MoveIwoAction3(import.Import1.Item.GiwoAction,
        export.Export1.Update.GiwoAction);
      MoveIwoActionHistory(import.Import1.Item.GiwoActionHistory,
        export.Export1.Update.GiwoActionHistory);
      export.Export1.Update.GworkArea.Text50 =
        import.Import1.Item.GworkArea.Text50;
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

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CsePersonNumber =
        export.CsePersonsWorkSet.Number;
      export.HiddenNextTranInfo.LegalActionIdentifier =
        export.LegalAction.Identifier;
      export.HiddenNextTranInfo.StandardCrtOrdNumber =
        export.LegalAction.StandardNumber ?? "";
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
      export.CsePersonsWorkSet.Number = "";
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "RESUBMIT") || Equal(global.Command, "CLEARSEV"))
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
    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "DISPLAY":
        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        UseLeEiwhDisplayEiwoHistory();

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
      case "UPDATE":
        if (Equal(export.HiddenIwoTransaction.Note,
          export.IwoTransaction.TransactionNumber))
        {
          var field1 = GetField(export.IwoTransaction, "note");

          field1.Error = true;

          ExitState = "FN0000_NO_UPDATES_MADE";

          break;
        }

        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        UseLeUpdateIwoTransactionNote();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "CLEARSEV":
        if (Equal(local.Severity.Text8, "RED") || Equal
          (local.Severity.Text8, "YELLOW"))
        {
        }
        else
        {
          var field1 = GetField(export.IwoTransaction, "transactionNumber");

          field1.Error = true;

          ExitState = "LE_EIWO_CANNOT_CLEAR_SEVERITY";

          return;
        }

        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        UseLeClearEiwoSeverity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ExitState = "LE0000_EIWO_SEVERITY_CLEARED";
        local.Command.Command = "REDISP";

        break;
      case "RESUBMIT":
        // -- Transaction must be in Error or Rejection status.
        if (AsChar(export.IwoTransaction.CurrentStatus) != 'J' && AsChar
          (export.IwoTransaction.CurrentStatus) != 'E')
        {
          ExitState = "LE0000_INVALID_RESUBMIT_STATUS";

          return;
        }

        // -- Do not allow portal errors to be re-submitted.
        if (ReadIwoTransaction())
        {
          foreach(var item in ReadIwoAction())
          {
            if (ReadIwoActionHistory())
            {
              if (Equal(entities.IwoActionHistory.CreatedBy, "SWELB588"))
              {
                ExitState = "LE0000_CANNOT_RESUB_PORTAL_ERROR";

                return;
              }

              break;
            }
          }
        }
        else
        {
          ExitState = "IWO_TRANSACTION_NF";

          return;
        }

        // -- Verify that the payor has an SSN.
        if (IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          var field1 = GetField(export.CsePersonsWorkSet, "ssn");

          field1.Error = true;

          ExitState = "LE0000_EIWO_NO_SSN";

          return;
        }

        if (IsEmpty(export.Case1.Number))
        {
          if (ReadOutgoingDocument())
          {
            if (ReadFieldValue())
            {
              export.Case1.Number = Substring(entities.FieldValue.Value, 1, 10);
            }
            else
            {
              ExitState = "LE0000_RESUB_THRU_LACT_LAIS";

              return;
            }
          }
          else
          {
            ExitState = "LE0000_RESUB_THRU_LACT_LAIS";

            return;
          }
        }

        // -- Verify that LACT edits are still satisfied.
        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        local.CaseRole.Type1 = "AP";
        UseLeValidateForEiwo();

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
          ExitState = "LE0000_EIWO_NO_FILED_DATE";

          return;
        }

        // -- Verify that selected employers are eIWO participants.
        if (ReadEmployer())
        {
          if (!Lt(Now().Date, entities.Employer.EiwoStartDate) && !
            Lt(entities.Employer.EiwoEndDate, Now().Date))
          {
          }
          else
          {
            ExitState = "LE0000_EIWO_EMP_NOT_EIWO";
          }
        }
        else
        {
          ExitState = "EMPLOYER_NF";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -- There cannot be another non ORDIWOLS eIWO in submitted status.
        if (!Equal(export.LegalAction.ActionTaken, "ORDIWOLS"))
        {
          if (ReadIncomeSourceEmployer())
          {
            local.Employer.Ein = entities.Employer.Ein;
          }
          else
          {
            var field1 = GetField(export.IncomeSource, "name");

            field1.Error = true;

            ExitState = "INCOME_SOURCE_NF";

            return;
          }

          if (ReadLegalAction())
          {
            var field1 = GetField(export.IncomeSource, "name");

            field1.Error = true;

            ExitState = "LE0000_EIWO_ALREADY_IN_S_STATUS";

            return;
          }
          else
          {
            // -- Continue
          }
        }

        // -- Update Note if it was changed.
        if (!Equal(export.IwoTransaction.Note, export.HiddenIwoTransaction.Note))
          
        {
          local.CsePerson.Number = export.CsePersonsWorkSet.Number;
          UseLeUpdateIwoTransactionNote();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field1 = GetField(export.IwoTransaction, "transactionNumber");

            field1.Error = true;

            return;
          }
        }

        // -- Create the eIWO document trigger.
        local.SpDocKey.KeyAp = export.CsePersonsWorkSet.Number;
        local.SpDocKey.KeyCase = export.Case1.Number;
        local.SpDocKey.KeyIncomeSource = export.IncomeSource.Identifier;
        local.SpDocKey.KeyLegalAction = export.LegalAction.Identifier;
        local.Document.Name = export.LegalAction.ActionTaken;
        UseSpCreateDocumentInfrastruct();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.IncomeSource, "name");

          field1.Error = true;

          return;
        }

        // -- Set field indicating this document is being delivered 
        // electronically.
        local.Field.Name = "LAEIWO2EMP";
        local.FieldValue.Value = "Y";
        UseSpCabCreateUpdateFieldValue();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.IncomeSource, "name");

          field1.Error = true;

          return;
        }

        // -- Create the eIWO.
        local.IwoAction.ActionType = "RESUB";
        local.IwoAction.StatusCd = "S";
        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        local.OutgoingDocument.PrintSucessfulIndicator = "D";
        UseLeCreateIwoTransaction();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.IncomeSource, "name");

          field1.Error = true;

          return;
        }

        local.Command.Command = "REDISP";
        ExitState = "LE0000_EIWO_RESUBMITTED";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (Equal(local.Command.Command, "REDISP"))
    {
      local.CsePerson.Number = export.CsePersonsWorkSet.Number;
      UseLeEiwhDisplayEiwoHistory();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
    }

    // -- Set display colors.
    if (AsChar(export.HiddenMostRecent.SeverityClearedInd) == 'Y')
    {
      // -- The severity has been cleared.
      var field1 = GetField(export.IwoTransaction, "transactionNumber");

      field1.Protected = true;
    }
    else
    {
      switch(AsChar(export.HiddenMostRecent.StatusCd))
      {
        case 'S':
          // -- These are submitted eIWO transactions.  Set color based on the 
          // aging cutoff date.
          if (!Lt(local.EiwoAgingCutoffDate.Date,
            export.HiddenMostRecent.StatusDate))
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "red";
            field2.Protected = true;
          }
          else
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Protected = true;
          }

          break;
        case 'N':
          // -- These are sent eIWO transactions.  Set color based on the aging 
          // cutoff date.
          if (!Lt(local.EiwoAgingCutoffDate.Date,
            export.HiddenMostRecent.StatusDate))
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "red";
            field2.Protected = true;
          }
          else
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "yellow";
            field2.Protected = true;
          }

          break;
        case 'R':
          // -- These are receipted eIWO transactions.  Set color based on the 
          // aging cutoff date.
          if (!Lt(local.EiwoAgingCutoffDate.Date,
            export.HiddenMostRecent.StatusDate))
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "red";
            field2.Protected = true;
          }
          else
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "yellow";
            field2.Protected = true;
          }

          break;
        case 'E':
          // -- These are errored eIWO transactions.
          var field1 = GetField(export.IwoTransaction, "transactionNumber");

          field1.Color = "red";
          field1.Protected = true;

          break;
        case 'J':
          // -- These are rejected eIWO transactions.
          if (Equal(export.HiddenMostRecent.StatusReasonCode, "N") || Equal
            (export.HiddenMostRecent.StatusReasonCode, "U"))
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Protected = true;
          }
          else
          {
            var field2 = GetField(export.IwoTransaction, "transactionNumber");

            field2.Color = "red";
            field2.Protected = true;
          }

          break;
        default:
          break;
      }
    }
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveExport1(LeEiwhDisplayEiwoHistory.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveIwoAction3(source.GiwoAction, target.GiwoAction);
    target.GworkArea.Text50 = source.GworkArea.Text50;
    MoveIwoActionHistory(source.GiwoActionHistory, target.GiwoActionHistory);
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
    target.ActionType = source.ActionType;
    target.StatusCd = source.StatusCd;
  }

  private static void MoveIwoAction3(IwoAction source, IwoAction target)
  {
    target.DocumentTrackingIdentifier = source.DocumentTrackingIdentifier;
    target.SeverityClearedInd = source.SeverityClearedInd;
  }

  private static void MoveIwoActionHistory(IwoActionHistory source,
    IwoActionHistory target)
  {
    target.ActionDate = source.ActionDate;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveIwoTransaction(IwoTransaction source,
    IwoTransaction target)
  {
    target.Identifier = source.Identifier;
    target.Note = source.Note;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.ActionTaken = source.ActionTaken;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAp = source.KeyAp;
    target.KeyCase = source.KeyCase;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyLegalAction = source.KeyLegalAction;
  }

  private void UseLeClearEiwoSeverity()
  {
    var useImport = new LeClearEiwoSeverity.Import();
    var useExport = new LeClearEiwoSeverity.Export();

    useImport.IwoAction.Identifier = export.HiddenMostRecent.Identifier;
    MoveIwoTransaction(export.IwoTransaction, useImport.IwoTransaction);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(LeClearEiwoSeverity.Execute, useImport, useExport);
  }

  private void UseLeCreateIwoTransaction()
  {
    var useImport = new LeCreateIwoTransaction.Import();
    var useExport = new LeCreateIwoTransaction.Export();

    MoveIwoAction2(local.IwoAction, useImport.IwoAction);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    useImport.OutgoingDocument.PrintSucessfulIndicator =
      local.OutgoingDocument.PrintSucessfulIndicator;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.IwoTransaction.TransactionNumber =
      export.IwoTransaction.TransactionNumber;
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeCreateIwoTransaction.Execute, useImport, useExport);

    MoveIwoAction1(useExport.IwoAction, export.HiddenMostRecent);
  }

  private void UseLeEiwhDisplayEiwoHistory()
  {
    var useImport = new LeEiwhDisplayEiwoHistory.Import();
    var useExport = new LeEiwhDisplayEiwoHistory.Export();

    useImport.IwoTransaction.Identifier = export.IwoTransaction.Identifier;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(LeEiwhDisplayEiwoHistory.Execute, useImport, useExport);

    export.HiddenMostRecent.Assign(useExport.MostRecent);
    export.IwoTransaction.Assign(useExport.IwoTransaction);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseLeUpdateIwoTransactionNote()
  {
    var useImport = new LeUpdateIwoTransactionNote.Import();
    var useExport = new LeUpdateIwoTransactionNote.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveIwoTransaction(export.IwoTransaction, useImport.IwoTransaction);

    Call(LeUpdateIwoTransactionNote.Execute, useImport, useExport);

    export.IwoTransaction.Assign(useExport.IwoTransaction);
  }

  private void UseLeValidateForEiwo()
  {
    var useImport = new LeValidateForEiwo.Import();
    var useExport = new LeValidateForEiwo.Export();

    MoveCaseRole(local.CaseRole, useImport.CaseRole);
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;
    MoveLegalAction1(export.LegalAction, useImport.LegalAction);

    Call(LeValidateForEiwo.Execute, useImport, useExport);
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
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

    MoveLegalAction2(export.LegalAction, useImport.LegalAction);
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

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          export.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", export.CsePersonsWorkSet.Number);
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

  private bool ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
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

  private bool ReadIncomeSourceEmployer()
  {
    entities.IncomeSource.Populated = false;
    entities.Employer.Populated = false;

    return Read("ReadIncomeSourceEmployer",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          export.IncomeSource.Identifier.GetValueOrDefault());
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

  private IEnumerable<bool> ReadIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.IwoAction.Populated = false;

    return ReadEach("ReadIwoAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.
          SetInt32(command, "iwtIdentifier", entities.IwoTransaction.Identifier);
          
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

        return true;
      });
  }

  private bool ReadIwoActionHistory()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);
    entities.IwoActionHistory.Populated = false;

    return Read("ReadIwoActionHistory",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
        db.SetInt32(command, "iwaIdentifier", entities.IwoAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IwoActionHistory.Identifier = db.GetInt32(reader, 0);
        entities.IwoActionHistory.CreatedBy = db.GetString(reader, 1);
        entities.IwoActionHistory.CspNumber = db.GetString(reader, 2);
        entities.IwoActionHistory.LgaIdentifier = db.GetInt32(reader, 3);
        entities.IwoActionHistory.IwtIdentifier = db.GetInt32(reader, 4);
        entities.IwoActionHistory.IwaIdentifier = db.GetInt32(reader, 5);
        entities.IwoActionHistory.Populated = true;
      });
  }

  private bool ReadIwoTransaction()
  {
    entities.IwoTransaction.Populated = false;

    return Read("ReadIwoTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", export.IwoTransaction.Identifier);
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 1);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 2);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 3);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 4);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 5);
        entities.IwoTransaction.Populated = true;
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
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", entities.IwoAction.InfId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 1);
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
      /// A value of GiwoAction.
      /// </summary>
      [JsonPropertyName("giwoAction")]
      public IwoAction GiwoAction
      {
        get => giwoAction ??= new();
        set => giwoAction = value;
      }

      /// <summary>
      /// A value of GworkArea.
      /// </summary>
      [JsonPropertyName("gworkArea")]
      public WorkArea GworkArea
      {
        get => gworkArea ??= new();
        set => gworkArea = value;
      }

      /// <summary>
      /// A value of GiwoActionHistory.
      /// </summary>
      [JsonPropertyName("giwoActionHistory")]
      public IwoActionHistory GiwoActionHistory
      {
        get => giwoActionHistory ??= new();
        set => giwoActionHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 96;

      private IwoAction giwoAction;
      private WorkArea gworkArea;
      private IwoActionHistory giwoActionHistory;
    }

    /// <summary>
    /// A value of HiddenMostRecent.
    /// </summary>
    [JsonPropertyName("hiddenMostRecent")]
    public IwoAction HiddenMostRecent
    {
      get => hiddenMostRecent ??= new();
      set => hiddenMostRecent = value;
    }

    /// <summary>
    /// A value of HiddenIwoTransaction.
    /// </summary>
    [JsonPropertyName("hiddenIwoTransaction")]
    public IwoTransaction HiddenIwoTransaction
    {
      get => hiddenIwoTransaction ??= new();
      set => hiddenIwoTransaction = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private IwoAction hiddenMostRecent;
    private IwoTransaction hiddenIwoTransaction;
    private Array<ImportGroup> import1;
    private IwoTransaction iwoTransaction;
    private IncomeSource incomeSource;
    private Case1 case1;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of GiwoAction.
      /// </summary>
      [JsonPropertyName("giwoAction")]
      public IwoAction GiwoAction
      {
        get => giwoAction ??= new();
        set => giwoAction = value;
      }

      /// <summary>
      /// A value of GworkArea.
      /// </summary>
      [JsonPropertyName("gworkArea")]
      public WorkArea GworkArea
      {
        get => gworkArea ??= new();
        set => gworkArea = value;
      }

      /// <summary>
      /// A value of GiwoActionHistory.
      /// </summary>
      [JsonPropertyName("giwoActionHistory")]
      public IwoActionHistory GiwoActionHistory
      {
        get => giwoActionHistory ??= new();
        set => giwoActionHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 96;

      private IwoAction giwoAction;
      private WorkArea gworkArea;
      private IwoActionHistory giwoActionHistory;
    }

    /// <summary>
    /// A value of HiddenMostRecent.
    /// </summary>
    [JsonPropertyName("hiddenMostRecent")]
    public IwoAction HiddenMostRecent
    {
      get => hiddenMostRecent ??= new();
      set => hiddenMostRecent = value;
    }

    /// <summary>
    /// A value of HiddenIwoTransaction.
    /// </summary>
    [JsonPropertyName("hiddenIwoTransaction")]
    public IwoTransaction HiddenIwoTransaction
    {
      get => hiddenIwoTransaction ??= new();
      set => hiddenIwoTransaction = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private IwoAction hiddenMostRecent;
    private IwoTransaction hiddenIwoTransaction;
    private Array<ExportGroup> export1;
    private IwoTransaction iwoTransaction;
    private IncomeSource incomeSource;
    private Case1 case1;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Severity.
    /// </summary>
    [JsonPropertyName("severity")]
    public TextWorkArea Severity
    {
      get => severity ??= new();
      set => severity = value;
    }

    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Common Command
    {
      get => command ??= new();
      set => command = value;
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

    private CaseRole caseRole;
    private TextWorkArea severity;
    private Common command;
    private FieldValue fieldValue;
    private Field field;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private NextTranInfo nullNextTranInfo;
    private IwoTransaction nullIwoTransaction;
    private IwoAction nullIwoAction;
    private Document document;
    private SpDocKey spDocKey;
    private Employer employer;
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
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of IwoActionHistory.
    /// </summary>
    [JsonPropertyName("iwoActionHistory")]
    public IwoActionHistory IwoActionHistory
    {
      get => iwoActionHistory ??= new();
      set => iwoActionHistory = value;
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

    private DocumentField documentField;
    private FieldValue fieldValue;
    private Field field;
    private IwoActionHistory iwoActionHistory;
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
