// Program: LE_LAPS_LST_LGL_ACTN_BY_CSE_PERS, ID: 372004317, model: 746.
// Short name: SWELAPSP
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
/// A program: LE_LAPS_LST_LGL_ACTN_BY_CSE_PERS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLapsLstLglActnByCsePers: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LAPS_LST_LGL_ACTN_BY_CSE_PERS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLapsLstLglActnByCsePers(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLapsLstLglActnByCsePers.
  /// </summary>
  public LeLapsLstLglActnByCsePers(IContext context, Import import,
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
    // ------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 08/14/95  S. Benton			Initial Code
    // 12/22/95  T.Redmond       		Rewrite
    // 02/01/96  H. Kennedy			Changed the Return Command Exitstate to Return.
    // 					Was Return to Caller
    // 01/13/98  P. Sharp			Made changes based on Phase II assessment sheets.
    // 01/11/01  GVandy	WR275		Add PF Key to flow to IWGL.
    // 12/10/01  GVandy	PR133994	Pass selected legal action id on nextran.
    // 04/02/02  GVandy	PR# 138221	Read for end dated code values when 
    // retrieving
    // 					action taken description.
    // 12/23/02  GVandy	WR10492		Display new attribute legal_action 
    // system_gen_ind.
    // ------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // *********************************************
    // Move Imports to Exports.
    // *********************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.SsnWorkArea.Assign(import.SsnWorkArea);
    export.RequiredClassification.Classification =
      import.RequiredClassification.Classification;
    export.PromptClassification.PromptField =
      import.PromptClassification.PromptField;

    if (import.SsnWorkArea.SsnNumPart1 > 0)
    {
      MoveSsnWorkArea2(import.SsnWorkArea, local.SsnWorkArea);
      local.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.CsePersonsWorkSet.Ssn = local.SsnWorkArea.SsnText9;
    }
    else
    {
      export.CsePersonsWorkSet.Ssn = "";
    }

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    export.ListByLrolOrLops.OneChar = import.ListByLrolOrLops.OneChar;
    export.ListEarlierThan.CreatedTstamp = import.ListEarlierThan.CreatedTstamp;
    local.NoOfRecordsSelected.Count = 0;
    local.GrpExpLegalActIndex.Count = 0;

    if (!import.LegalActions.IsEmpty)
    {
      for(import.LegalActions.Index = 0; import.LegalActions.Index < import
        .LegalActions.Count; ++import.LegalActions.Index)
      {
        ++local.GrpExpLegalActIndex.Count;

        export.LegalActions.Index = local.GrpExpLegalActIndex.Count - 1;
        export.LegalActions.CheckSize();

        export.LegalActions.Update.Common.SelectChar =
          import.LegalActions.Item.Common.SelectChar;
        export.LegalActions.Update.LegalAction.Assign(
          import.LegalActions.Item.LegalAction);
        export.LegalActions.Update.LaActTaken.Description =
          import.LegalActions.Item.DetailLaActTaken.Description;
        export.LegalActions.Update.LegalActionDetail.DetailType =
          import.LegalActions.Item.LegalActionDetail.DetailType;
        export.LegalActions.Update.ObligationType.Assign(
          import.LegalActions.Item.ObligationType);
        MoveLegalActionPerson(import.LegalActions.Item.LegalActionPerson,
          export.LegalActions.Update.LegalActionPerson);
        export.LegalActions.Update.DetailTribunal.Assign(
          import.LegalActions.Item.DetailTribunal);
        export.LegalActions.Update.DetailForeign.Country =
          import.LegalActions.Item.DetailForeign.Country;
        export.LegalActions.Update.DetailFips.Assign(
          import.LegalActions.Item.DetailFips);
        export.LegalActions.Update.DetailLaAppInd.Flag =
          import.LegalActions.Item.DetailLaAppInd.Flag;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
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
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      export.LegalActions.Index = -1;

      for(import.LegalActions.Index = 0; import.LegalActions.Index < import
        .LegalActions.Count; ++import.LegalActions.Index)
      {
        ++export.LegalActions.Index;
        export.LegalActions.CheckSize();

        if (!IsEmpty(export.LegalActions.Item.Common.SelectChar) && AsChar
          (export.LegalActions.Item.Common.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.LegalActions.Item.Common, "selectChar");

          field.Error = true;

          return;
        }

        if (AsChar(export.LegalActions.Item.Common.SelectChar) == 'S')
        {
          ++local.NoOfRecordsSelected.Count;

          if (local.NoOfRecordsSelected.Count > 1)
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            return;
          }

          export.Selected.Assign(export.LegalActions.Item.LegalAction);
        }
      }

      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      local.NextTranInfo.LegalActionIdentifier = export.Selected.Identifier;
      UseScCabNextTranPut();

      return;
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
      export.LegalActions.Count = 0;

      if (IsEmpty(export.CsePersonsWorkSet.Number) && IsEmpty
        (export.CsePersonsWorkSet.Ssn))
      {
        var field1 = GetField(export.CsePersonsWorkSet, "number");

        field1.Error = true;

        var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

        field2.Error = true;

        var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

        field3.Error = true;

        var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

        field4.Error = true;

        ExitState = "LE0000_CSE_NO_OR_SSN_REQD";

        return;
      }

      local.Saved.Number = export.CsePersonsWorkSet.Number;

      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.CsePersonsWorkSet.Number = local.Saved.Number;

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;
        }

        local.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
        UseCabSsnConvertTextToNum();
      }
      else if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
      {
        local.Search.Flag = "1";
        UseCabMatchCsePerson();

        for(local.CsePersFromSearch.Index = 0; local.CsePersFromSearch.Index < local
          .CsePersFromSearch.Count; ++local.CsePersFromSearch.Index)
        {
          ++local.ReturnFromSsn.Count;

          if (local.ReturnFromSsn.Count == 1)
          {
            MoveCsePersonsWorkSet1(local.CsePersFromSearch.Item.Detail,
              export.CsePersonsWorkSet);
          }
        }

        if (local.ReturnFromSsn.Count > 1)
        {
          var field1 = GetField(export.SsnWorkArea, "ssnNumPart1");

          field1.Error = true;

          var field2 = GetField(export.SsnWorkArea, "ssnNumPart2");

          field2.Error = true;

          var field3 = GetField(export.SsnWorkArea, "ssnNumPart3");

          field3.Error = true;

          ExitState = "LE0000_MULTIPLE_PERSONS_FOR_SSN";
        }

        if (local.ReturnFromSsn.Count == 0)
        {
          var field1 = GetField(export.SsnWorkArea, "ssnNumPart1");

          field1.Error = true;

          var field2 = GetField(export.SsnWorkArea, "ssnNumPart2");

          field2.Error = true;

          var field3 = GetField(export.SsnWorkArea, "ssnNumPart3");

          field3.Error = true;

          ExitState = "ACO_ADABAS_NO_SSN_MATCH";
        }
      }

      if (!IsEmpty(import.RequiredClassification.Classification))
      {
        local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        local.CodeValue.Cdvalue = import.RequiredClassification.Classification;
        UseCabValidateCodeValue();

        // **** PUT USE STATEMENT OF CAB_VALIDATE_CODE_VALUE HERE
        if (AsChar(local.ValidClass.Flag) != 'Y')
        {
          var field = GetField(export.RequiredClassification, "classification");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      if (IsEmpty(export.ListByLrolOrLops.OneChar))
      {
        export.ListByLrolOrLops.OneChar = "O";
      }

      if (AsChar(export.ListByLrolOrLops.OneChar) != 'O' && AsChar
        (export.ListByLrolOrLops.OneChar) != 'L')
      {
        var field = GetField(export.ListByLrolOrLops, "oneChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "LE0000_INVALID_LIST_BY_LROL_LOPS";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "RETURN") || Equal(global.Command, "LACT"))
    {
      export.LegalActions.Index = -1;

      for(import.LegalActions.Index = 0; import.LegalActions.Index < import
        .LegalActions.Count; ++import.LegalActions.Index)
      {
        ++export.LegalActions.Index;
        export.LegalActions.CheckSize();

        if (!IsEmpty(export.LegalActions.Item.Common.SelectChar) && AsChar
          (export.LegalActions.Item.Common.SelectChar) != 'S')
        {
          var field = GetField(export.LegalActions.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (AsChar(export.LegalActions.Item.Common.SelectChar) == 'S')
        {
          ++local.NoOfRecordsSelected.Count;

          if (local.NoOfRecordsSelected.Count > 1)
          {
            break;
          }

          export.Selected.Assign(export.LegalActions.Item.LegalAction);
        }
      }

      if (local.NoOfRecordsSelected.Count > 1)
      {
        export.LegalActions.Index = -1;

        for(import.LegalActions.Index = 0; import.LegalActions.Index < import
          .LegalActions.Count; ++import.LegalActions.Index)
        {
          ++export.LegalActions.Index;
          export.LegalActions.CheckSize();

          if (AsChar(export.LegalActions.Item.Common.SelectChar) == 'S')
          {
            var field = GetField(export.LegalActions.Item.Common, "selectChar");

            field.Error = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // *********************************************
    //        P F K E Y   P R O C E S S I N G
    // *********************************************
    switch(TrimEnd(global.Command))
    {
      case "IWGL":
        local.TotalSelected.Count = 0;

        for(export.LegalActions.Index = 0; export.LegalActions.Index < export
          .LegalActions.Count; ++export.LegalActions.Index)
        {
          if (!export.LegalActions.CheckSize())
          {
            break;
          }

          switch(AsChar(export.LegalActions.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.TotalSelected.Count;

              if (local.TotalSelected.Count > 1)
              {
                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                var field2 =
                  GetField(export.LegalActions.Item.Common, "selectChar");

                field2.Error = true;

                return;
              }

              MoveLegalAction(export.LegalActions.Item.LegalAction,
                export.DlgflwSelected);

              break;
            default:
              var field1 =
                GetField(export.LegalActions.Item.Common, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
          }
        }

        export.LegalActions.CheckIndex();

        if (local.TotalSelected.Count > 0)
        {
          if (AsChar(export.DlgflwSelected.Classification) == 'I' || AsChar
            (export.DlgflwSelected.Classification) == 'G')
          {
            export.DlgflwIwglType.Text1 = export.DlgflwSelected.Classification;
            ExitState = "ECO_LNK_TO_IWGL";
          }
          else
          {
            ExitState = "LE0000_I_OR_G_CLASS_FOR_IWGL";
          }
        }
        else
        {
          ExitState = "LE0000_LACT_MUST_BE_SELECTED";
        }

        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "LACT":
        ExitState = "ECO_LNK_TO_LEGAL_ACTION";

        break;
      case "LIST":
        switch(AsChar(export.PromptClassification.PromptField))
        {
          case ' ':
            break;
          case 'S':
            export.DlgflwRequired.CodeName = "LEGAL ACTION CLASSIFICATION";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field1 = GetField(export.PromptClassification, "promptField");

            field1.Error = true;

            return;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        var field = GetField(export.PromptClassification, "promptField");

        field.Error = true;

        break;
      case "RETCDVL":
        if (AsChar(export.PromptClassification.PromptField) == 'S')
        {
          export.PromptClassification.PromptField = "";

          if (!IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            export.RequiredClassification.Classification =
              import.DlgflwSelected.Cdvalue;

            var field1 = GetField(export.ListEarlierThan, "createdTstamp");

            field1.Protected = false;
            field1.Focused = true;
          }
          else
          {
            var field1 =
              GetField(export.RequiredClassification, "classification");

            field1.Protected = false;
            field1.Focused = true;
          }
        }

        break;
      case "DISPLAY":
        if (!ReadCsePerson())
        {
          ExitState = "CSE_PERSON_NF";

          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          return;
        }

        export.LegalActions.Index = -1;

        switch(AsChar(export.ListByLrolOrLops.OneChar))
        {
          case 'O':
            foreach(var item in ReadLegalActionPersonLegalActionDetailLegalAction())
              
            {
              if (IsEmpty(export.RequiredClassification.Classification))
              {
                // *** Continue on need all classifications.
              }
              else if (AsChar(entities.ExistingLegalAction.Classification) == AsChar
                (export.RequiredClassification.Classification))
              {
                // ***Only want classification specified.
              }
              else
              {
                continue;
              }

              if (Equal(export.ListEarlierThan.CreatedTstamp,
                local.Null1.Timestamp))
              {
                // *** Continue on need all dates
              }
              else if (Lt(entities.ExistingLegalAction.CreatedTstamp,
                export.ListEarlierThan.CreatedTstamp))
              {
                // *** Need only dates that are less than entered date.
              }
              else
              {
                continue;
              }

              if (AsChar(entities.ExistingLegalActionDetail.DetailType) == 'F')
              {
                if (ReadObligationType())
                {
                  local.ObligationType.Code =
                    entities.ExistingObligationType.Code;
                }
                else
                {
                  local.ObligationType.Assign(local.Initialised);
                }
              }
              else
              {
                local.ObligationType.Code =
                  entities.ExistingLegalActionDetail.NonFinOblgType ?? Spaces
                  (7);
              }

              if (entities.ExistingLegalAction.Identifier == local
                .PreviousLegalAction.Identifier && AsChar
                (entities.ExistingLegalActionPerson.AccountType) == AsChar
                (local.PreviousLegalActionPerson.AccountType) && Equal
                (local.ObligationType.Code, local.PreviousObligationType.Code))
              {
                continue;
              }

              if (export.LegalActions.Index + 1 >= Export
                .LegalActionsGroup.Capacity)
              {
                goto Test;
              }

              ++export.LegalActions.Index;
              export.LegalActions.CheckSize();

              export.LegalActions.Update.Common.SelectChar = "";
              export.LegalActions.Update.LegalAction.Assign(
                entities.ExistingLegalAction);
              local.PreviousLegalAction.Identifier =
                entities.ExistingLegalAction.Identifier;

              if (ReadLegalActionAppeal())
              {
                export.LegalActions.Update.DetailLaAppInd.Flag = "Y";
              }

              UseLeGetActionTakenDescription();
              MoveLegalActionPerson(entities.ExistingLegalActionPerson,
                export.LegalActions.Update.LegalActionPerson);
              local.PreviousLegalActionPerson.AccountType =
                entities.ExistingLegalActionPerson.AccountType;
              export.LegalActions.Update.LegalActionDetail.DetailType =
                entities.ExistingLegalActionDetail.DetailType;
              local.PreviousLegalActionDetail.Assign(
                entities.ExistingLegalActionDetail);
              export.LegalActions.Update.ObligationType.Code =
                local.ObligationType.Code;
              local.PreviousObligationType.Code = local.ObligationType.Code;

              if (ReadTribunal())
              {
                export.LegalActions.Update.DetailTribunal.Assign(
                  entities.ExistingTribunal);

                if (ReadFips())
                {
                  export.LegalActions.Update.DetailFips.Assign(
                    entities.ExistingFips);
                }
                else if (ReadFipsTribAddress())
                {
                  export.LegalActions.Update.DetailForeign.Country =
                    entities.ExistingForeign.Country;
                }
              }
            }

            break;
          case 'L':
            export.LegalActions.Index = -1;

            foreach(var item in ReadLegalActionPersonLegalAction())
            {
              if (IsEmpty(export.RequiredClassification.Classification))
              {
                // *** Continue on need all classifications.
              }
              else if (AsChar(entities.ExistingLegalAction.Classification) == AsChar
                (export.RequiredClassification.Classification))
              {
                // ***Only want classification specified.
              }
              else
              {
                continue;
              }

              if (Equal(export.ListEarlierThan.CreatedTstamp,
                local.Null1.Timestamp))
              {
                // *** Continue on need all dates
              }
              else if (Lt(entities.ExistingLegalAction.CreatedTstamp,
                export.ListEarlierThan.CreatedTstamp))
              {
                // *** Need only dates that are less than entered date.
              }
              else
              {
                continue;
              }

              if (export.LegalActions.Index + 1 >= Export
                .LegalActionsGroup.Capacity)
              {
                goto Test;
              }

              ++export.LegalActions.Index;
              export.LegalActions.CheckSize();

              export.LegalActions.Update.Common.SelectChar = "";
              local.PreviousLegalAction.Identifier =
                entities.ExistingLegalAction.Identifier;
              export.LegalActions.Update.LegalAction.Assign(
                entities.ExistingLegalAction);

              if (ReadLegalActionAppeal())
              {
                export.LegalActions.Update.DetailLaAppInd.Flag = "Y";
              }

              UseLeGetActionTakenDescription();
              MoveLegalActionPerson(entities.ExistingLegalActionPerson,
                export.LegalActions.Update.LegalActionPerson);

              if (ReadTribunal())
              {
                export.LegalActions.Update.DetailTribunal.Assign(
                  entities.ExistingTribunal);

                if (ReadFips())
                {
                  export.LegalActions.Update.DetailFips.Assign(
                    entities.ExistingFips);
                }
                else if (ReadFipsTribAddress())
                {
                  export.LegalActions.Update.DetailForeign.Country =
                    entities.ExistingForeign.Country;
                }
              }
            }

            break;
          default:
            break;
        }

Test:

        if (export.LegalActions.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }
        else if (export.LegalActions.IsEmpty)
        {
          ExitState = "LE0000_NO_LGL_ACTION_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveExport1ToCsePersFromSearch(CabMatchCsePerson.Export.
    ExportGroup source, Local.CsePersFromSearchGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.DetailAlt.Flag = source.Ae.Flag;
    target.Ae.Flag = source.Cse.Flag;
    target.Cse.Flag = source.Kanpay.Flag;
    target.Kanpay.Flag = source.Kscares.Flag;
    target.Kscares.Flag = source.Alt.Flag;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalActionPerson(LegalActionPerson source,
    LegalActionPerson target)
  {
    target.AccountType = source.AccountType;
    target.Role = source.Role;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNum9 = source.SsnNum9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabMatchCsePerson()
  {
    var useImport = new CabMatchCsePerson.Import();
    var useExport = new CabMatchCsePerson.Export();

    useImport.Search.Flag = local.Search.Flag;
    MoveCsePersonsWorkSet2(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

    Call(CabMatchCsePerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(
      local.CsePersFromSearch, MoveExport1ToCsePersFromSearch);
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    useImport.SsnWorkArea.Assign(local.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea1(useExport.SsnWorkArea, local.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    export.SsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidClass.Flag = useExport.ValidCode.Flag;
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

  private void UseLeGetActionTakenDescription()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken =
      entities.ExistingLegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    export.LegalActions.Update.LaActTaken.Description =
      useExport.CodeValue.Description;
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

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingTribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingTribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingTribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.ExistingForeign.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingForeign.Identifier = db.GetInt32(reader, 0);
        entities.ExistingForeign.Country = db.GetNullableString(reader, 1);
        entities.ExistingForeign.TrbId = db.GetNullableInt32(reader, 2);
        entities.ExistingForeign.Populated = true;
      });
  }

  private bool ReadLegalActionAppeal()
  {
    entities.ExistingLegalActionAppeal.Populated = false;

    return Read("ReadLegalActionAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.ExistingLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionAppeal.AplId = db.GetInt32(reader, 1);
        entities.ExistingLegalActionAppeal.LgaId = db.GetInt32(reader, 2);
        entities.ExistingLegalActionAppeal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonLegalAction()
  {
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalActionPersonLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 2);
        entities.ExistingLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingLegalActionPerson.Role = db.GetString(reader, 4);
        entities.ExistingLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 7);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 8);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 9);
        entities.ExistingLegalAction.ActionTaken = db.GetString(reader, 10);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 11);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingLegalAction.CreatedTstamp = db.GetDateTime(reader, 14);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 15);
        entities.ExistingLegalAction.SystemGenInd =
          db.GetNullableString(reader, 16);
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonLegalActionDetailLegalAction()
  {
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalActionDetail.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalActionPersonLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingLegalActionPerson.Role = db.GetString(reader, 4);
        entities.ExistingLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingLegalActionDetail.LgaIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 6);
        entities.ExistingLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 7);
        entities.ExistingLegalActionDetail.Number = db.GetInt32(reader, 7);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 8);
        entities.ExistingLegalActionDetail.EndDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingLegalActionDetail.EffectiveDate =
          db.GetDate(reader, 10);
        entities.ExistingLegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 11);
        entities.ExistingLegalActionDetail.DetailType =
          db.GetString(reader, 12);
        entities.ExistingLegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 13);
        entities.ExistingLegalActionDetail.OtyId =
          db.GetNullableInt32(reader, 14);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 15);
        entities.ExistingLegalAction.ActionTaken = db.GetString(reader, 16);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 17);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingLegalAction.CreatedTstamp = db.GetDateTime(reader, 20);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 21);
        entities.ExistingLegalAction.SystemGenInd =
          db.GetNullableString(reader, 22);
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalActionDetail.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.ExistingLegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingLegalActionDetail.Populated);
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.ExistingLegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Code = db.GetString(reader, 1);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 2);
        entities.ExistingObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ExistingObligationType.DiscontinueDt =
          db.GetNullableDate(reader, 4);
        entities.ExistingObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingLegalAction.Populated);
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingTribunal.Name = db.GetString(reader, 1);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 4);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.ExistingTribunal.Populated = true;
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
    /// <summary>A LegalActionsGroup group.</summary>
    [Serializable]
    public class LegalActionsGroup
    {
      /// <summary>
      /// A value of DetailLaAppInd.
      /// </summary>
      [JsonPropertyName("detailLaAppInd")]
      public Common DetailLaAppInd
      {
        get => detailLaAppInd ??= new();
        set => detailLaAppInd = value;
      }

      /// <summary>
      /// A value of DetailLaActTaken.
      /// </summary>
      [JsonPropertyName("detailLaActTaken")]
      public CodeValue DetailLaActTaken
      {
        get => detailLaActTaken ??= new();
        set => detailLaActTaken = value;
      }

      /// <summary>
      /// A value of DetailForeign.
      /// </summary>
      [JsonPropertyName("detailForeign")]
      public FipsTribAddress DetailForeign
      {
        get => detailForeign ??= new();
        set => detailForeign = value;
      }

      /// <summary>
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>
      /// A value of DetailTribunal.
      /// </summary>
      [JsonPropertyName("detailTribunal")]
      public Tribunal DetailTribunal
      {
        get => detailTribunal ??= new();
        set => detailTribunal = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>
      /// A value of LegalActionDetail.
      /// </summary>
      [JsonPropertyName("legalActionDetail")]
      public LegalActionDetail LegalActionDetail
      {
        get => legalActionDetail ??= new();
        set => legalActionDetail = value;
      }

      /// <summary>
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
      }

      /// <summary>
      /// A value of LegalActionPerson.
      /// </summary>
      [JsonPropertyName("legalActionPerson")]
      public LegalActionPerson LegalActionPerson
      {
        get => legalActionPerson ??= new();
        set => legalActionPerson = value;
      }

      /// <summary>
      /// A value of Prc.
      /// </summary>
      [JsonPropertyName("prc")]
      public LegalActionPerson Prc
      {
        get => prc ??= new();
        set => prc = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common detailLaAppInd;
      private CodeValue detailLaActTaken;
      private FipsTribAddress detailForeign;
      private Fips detailFips;
      private Tribunal detailTribunal;
      private Common common;
      private LegalAction legalAction;
      private LegalActionDetail legalActionDetail;
      private ObligationType obligationType;
      private LegalActionPerson legalActionPerson;
      private LegalActionPerson prc;
    }

    /// <summary>
    /// A value of RequiredClassification.
    /// </summary>
    [JsonPropertyName("requiredClassification")]
    public LegalAction RequiredClassification
    {
      get => requiredClassification ??= new();
      set => requiredClassification = value;
    }

    /// <summary>
    /// A value of ListEarlierThan.
    /// </summary>
    [JsonPropertyName("listEarlierThan")]
    public LegalAction ListEarlierThan
    {
      get => listEarlierThan ??= new();
      set => listEarlierThan = value;
    }

    /// <summary>
    /// A value of ListByLrolOrLops.
    /// </summary>
    [JsonPropertyName("listByLrolOrLops")]
    public Standard ListByLrolOrLops
    {
      get => listByLrolOrLops ??= new();
      set => listByLrolOrLops = value;
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
    /// Gets a value of LegalActions.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionsGroup> LegalActions => legalActions ??= new(
      LegalActionsGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalActions for json serialization.
    /// </summary>
    [JsonPropertyName("legalActions")]
    [Computed]
    public IList<LegalActionsGroup> LegalActions_Json
    {
      get => legalActions;
      set => LegalActions.Assign(value);
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
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
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of PromptClassification.
    /// </summary>
    [JsonPropertyName("promptClassification")]
    public Standard PromptClassification
    {
      get => promptClassification ??= new();
      set => promptClassification = value;
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public CodeValue DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    private LegalAction requiredClassification;
    private LegalAction listEarlierThan;
    private Standard listByLrolOrLops;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<LegalActionsGroup> legalActions;
    private Standard standard;
    private Security2 hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
    private SsnWorkArea ssnWorkArea;
    private Standard promptClassification;
    private CodeValue dlgflwSelected;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A LegalActionsGroup group.</summary>
    [Serializable]
    public class LegalActionsGroup
    {
      /// <summary>
      /// A value of DetailLaAppInd.
      /// </summary>
      [JsonPropertyName("detailLaAppInd")]
      public Common DetailLaAppInd
      {
        get => detailLaAppInd ??= new();
        set => detailLaAppInd = value;
      }

      /// <summary>
      /// A value of LaActTaken.
      /// </summary>
      [JsonPropertyName("laActTaken")]
      public CodeValue LaActTaken
      {
        get => laActTaken ??= new();
        set => laActTaken = value;
      }

      /// <summary>
      /// A value of DetailForeign.
      /// </summary>
      [JsonPropertyName("detailForeign")]
      public FipsTribAddress DetailForeign
      {
        get => detailForeign ??= new();
        set => detailForeign = value;
      }

      /// <summary>
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>
      /// A value of DetailTribunal.
      /// </summary>
      [JsonPropertyName("detailTribunal")]
      public Tribunal DetailTribunal
      {
        get => detailTribunal ??= new();
        set => detailTribunal = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>
      /// A value of LegalActionDetail.
      /// </summary>
      [JsonPropertyName("legalActionDetail")]
      public LegalActionDetail LegalActionDetail
      {
        get => legalActionDetail ??= new();
        set => legalActionDetail = value;
      }

      /// <summary>
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
      }

      /// <summary>
      /// A value of LegalActionPerson.
      /// </summary>
      [JsonPropertyName("legalActionPerson")]
      public LegalActionPerson LegalActionPerson
      {
        get => legalActionPerson ??= new();
        set => legalActionPerson = value;
      }

      /// <summary>
      /// A value of Prc.
      /// </summary>
      [JsonPropertyName("prc")]
      public LegalActionPerson Prc
      {
        get => prc ??= new();
        set => prc = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common detailLaAppInd;
      private CodeValue laActTaken;
      private FipsTribAddress detailForeign;
      private Fips detailFips;
      private Tribunal detailTribunal;
      private Common common;
      private LegalAction legalAction;
      private LegalActionDetail legalActionDetail;
      private ObligationType obligationType;
      private LegalActionPerson legalActionPerson;
      private LegalActionPerson prc;
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public LegalAction DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// A value of DlgflwIwglType.
    /// </summary>
    [JsonPropertyName("dlgflwIwglType")]
    public WorkArea DlgflwIwglType
    {
      get => dlgflwIwglType ??= new();
      set => dlgflwIwglType = value;
    }

    /// <summary>
    /// A value of RequiredClassification.
    /// </summary>
    [JsonPropertyName("requiredClassification")]
    public LegalAction RequiredClassification
    {
      get => requiredClassification ??= new();
      set => requiredClassification = value;
    }

    /// <summary>
    /// A value of ListEarlierThan.
    /// </summary>
    [JsonPropertyName("listEarlierThan")]
    public LegalAction ListEarlierThan
    {
      get => listEarlierThan ??= new();
      set => listEarlierThan = value;
    }

    /// <summary>
    /// A value of ListByLrolOrLops.
    /// </summary>
    [JsonPropertyName("listByLrolOrLops")]
    public Standard ListByLrolOrLops
    {
      get => listByLrolOrLops ??= new();
      set => listByLrolOrLops = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public LegalAction Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// Gets a value of LegalActions.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionsGroup> LegalActions => legalActions ??= new(
      LegalActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalActions for json serialization.
    /// </summary>
    [JsonPropertyName("legalActions")]
    [Computed]
    public IList<LegalActionsGroup> LegalActions_Json
    {
      get => legalActions;
      set => LegalActions.Assign(value);
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
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
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of PromptClassification.
    /// </summary>
    [JsonPropertyName("promptClassification")]
    public Standard PromptClassification
    {
      get => promptClassification ??= new();
      set => promptClassification = value;
    }

    /// <summary>
    /// A value of DlgflwRequired.
    /// </summary>
    [JsonPropertyName("dlgflwRequired")]
    public Code DlgflwRequired
    {
      get => dlgflwRequired ??= new();
      set => dlgflwRequired = value;
    }

    private LegalAction dlgflwSelected;
    private WorkArea dlgflwIwglType;
    private LegalAction requiredClassification;
    private LegalAction listEarlierThan;
    private Standard listByLrolOrLops;
    private LegalAction selected;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<LegalActionsGroup> legalActions;
    private Standard standard;
    private Security2 hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
    private SsnWorkArea ssnWorkArea;
    private Standard promptClassification;
    private Code dlgflwRequired;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CsePersFromSearchGroup group.</summary>
    [Serializable]
    public class CsePersFromSearchGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailAlt.
      /// </summary>
      [JsonPropertyName("detailAlt")]
      public Common DetailAlt
      {
        get => detailAlt ??= new();
        set => detailAlt = value;
      }

      /// <summary>
      /// A value of Ae.
      /// </summary>
      [JsonPropertyName("ae")]
      public Common Ae
      {
        get => ae ??= new();
        set => ae = value;
      }

      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>
      /// A value of Kanpay.
      /// </summary>
      [JsonPropertyName("kanpay")]
      public Common Kanpay
      {
        get => kanpay ??= new();
        set => kanpay = value;
      }

      /// <summary>
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet detail;
      private Common detailAlt;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
    }

    /// <summary>
    /// A value of TotalSelected.
    /// </summary>
    [JsonPropertyName("totalSelected")]
    public Common TotalSelected
    {
      get => totalSelected ??= new();
      set => totalSelected = value;
    }

    /// <summary>
    /// A value of ValidClass.
    /// </summary>
    [JsonPropertyName("validClass")]
    public Common ValidClass
    {
      get => validClass ??= new();
      set => validClass = value;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
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
    /// A value of PreviousObligationType.
    /// </summary>
    [JsonPropertyName("previousObligationType")]
    public ObligationType PreviousObligationType
    {
      get => previousObligationType ??= new();
      set => previousObligationType = value;
    }

    /// <summary>
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public ObligationType Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of PreviousLegalActionDetail.
    /// </summary>
    [JsonPropertyName("previousLegalActionDetail")]
    public LegalActionDetail PreviousLegalActionDetail
    {
      get => previousLegalActionDetail ??= new();
      set => previousLegalActionDetail = value;
    }

    /// <summary>
    /// A value of PreviousLegalActionPerson.
    /// </summary>
    [JsonPropertyName("previousLegalActionPerson")]
    public LegalActionPerson PreviousLegalActionPerson
    {
      get => previousLegalActionPerson ??= new();
      set => previousLegalActionPerson = value;
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
    /// A value of GrpExpLegalActIndex.
    /// </summary>
    [JsonPropertyName("grpExpLegalActIndex")]
    public Common GrpExpLegalActIndex
    {
      get => grpExpLegalActIndex ??= new();
      set => grpExpLegalActIndex = value;
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
    /// A value of NoOfRecordsSelected.
    /// </summary>
    [JsonPropertyName("noOfRecordsSelected")]
    public Common NoOfRecordsSelected
    {
      get => noOfRecordsSelected ??= new();
      set => noOfRecordsSelected = value;
    }

    /// <summary>
    /// A value of RowIsSelected.
    /// </summary>
    [JsonPropertyName("rowIsSelected")]
    public Common RowIsSelected
    {
      get => rowIsSelected ??= new();
      set => rowIsSelected = value;
    }

    /// <summary>
    /// A value of ReturnFromSsn.
    /// </summary>
    [JsonPropertyName("returnFromSsn")]
    public Common ReturnFromSsn
    {
      get => returnFromSsn ??= new();
      set => returnFromSsn = value;
    }

    /// <summary>
    /// Gets a value of CsePersFromSearch.
    /// </summary>
    [JsonIgnore]
    public Array<CsePersFromSearchGroup> CsePersFromSearch =>
      csePersFromSearch ??= new(CsePersFromSearchGroup.Capacity);

    /// <summary>
    /// Gets a value of CsePersFromSearch for json serialization.
    /// </summary>
    [JsonPropertyName("csePersFromSearch")]
    [Computed]
    public IList<CsePersFromSearchGroup> CsePersFromSearch_Json
    {
      get => csePersFromSearch;
      set => CsePersFromSearch.Assign(value);
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Common Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private Common totalSelected;
    private Common validClass;
    private SsnWorkArea ssnWorkArea;
    private TextWorkArea textWorkArea;
    private CsePersonsWorkSet saved;
    private ObligationType previousObligationType;
    private ObligationType initialised;
    private ObligationType obligationType;
    private LegalActionDetail previousLegalActionDetail;
    private LegalActionPerson previousLegalActionPerson;
    private LegalAction previousLegalAction;
    private Common grpExpLegalActIndex;
    private DateWorkArea null1;
    private Common noOfRecordsSelected;
    private Common rowIsSelected;
    private Common returnFromSsn;
    private Array<CsePersFromSearchGroup> csePersFromSearch;
    private Common search;
    private CodeValue codeValue;
    private Code code;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalActionAppeal.
    /// </summary>
    [JsonPropertyName("existingLegalActionAppeal")]
    public LegalActionAppeal ExistingLegalActionAppeal
    {
      get => existingLegalActionAppeal ??= new();
      set => existingLegalActionAppeal = value;
    }

    /// <summary>
    /// A value of ExistingForeign.
    /// </summary>
    [JsonPropertyName("existingForeign")]
    public FipsTribAddress ExistingForeign
    {
      get => existingForeign ??= new();
      set => existingForeign = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingLegalActionPerson")]
    public LegalActionPerson ExistingLegalActionPerson
    {
      get => existingLegalActionPerson ??= new();
      set => existingLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionDetail.
    /// </summary>
    [JsonPropertyName("existingLegalActionDetail")]
    public LegalActionDetail ExistingLegalActionDetail
    {
      get => existingLegalActionDetail ??= new();
      set => existingLegalActionDetail = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    private LegalActionAppeal existingLegalActionAppeal;
    private FipsTribAddress existingForeign;
    private Fips existingFips;
    private Tribunal existingTribunal;
    private LegalActionPerson existingLegalActionPerson;
    private LegalActionDetail existingLegalActionDetail;
    private CsePerson existingCsePerson;
    private LegalAction existingLegalAction;
    private ObligationType existingObligationType;
  }
#endregion
}
