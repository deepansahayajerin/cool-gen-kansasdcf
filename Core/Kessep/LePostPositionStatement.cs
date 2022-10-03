// Program: LE_POST_POSITION_STATEMENT, ID: 372603449, model: 746.
// Short name: SWEPOSTP
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
/// A program: LE_POST_POSITION_STATEMENT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This proc step maintains POSITION_STATEMENT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LePostPositionStatement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_POST_POSITION_STATEMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LePostPositionStatement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LePostPositionStatement.
  /// </summary>
  public LePostPositionStatement(IContext context, Import import, Export export):
    
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
    // Every initial development and change to that development needs to be 
    // documented.
    // ---------------------------------------------
    // *******************************************************************
    //   Date		Developer	Request #	Description
    // 05-21-95        S. Benton			Initial development
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // *********************************************
    // Move Imports to Exports
    // *********************************************
    MoveAdministrativeAction(import.AdministrativeAction,
      export.AdministrativeAction);
    export.AdministrativeAppeal.Assign(import.AdministrativeAppeal);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.PromptAdmAppeal.PromptField = import.PromptAdmAppeal.PromptField;
    export.SsnWorkArea.Assign(import.SsnWorkArea);

    if (export.SsnWorkArea.SsnNumPart1 > 0 || export.SsnWorkArea.SsnNumPart2 > 0
      || export.SsnWorkArea.SsnNumPart3 > 0)
    {
      export.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.CsePersonsWorkSet.Ssn = export.SsnWorkArea.SsnText9;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else if (!import.Import1.IsEmpty)
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

        export.Export1.Update.Common.SelectChar =
          import.Import1.Item.Common.SelectChar;
        MovePositionStatement(import.Import1.Item.PositionStatement,
          export.Export1.Update.PositionStatement);
        export.Export1.Next();
      }
    }

    // *********************************************
    // Move Hidden Imports to Hidden Exports.
    // *********************************************
    MoveAdministrativeAction(import.HiddenAdministrativeAction,
      export.HiddenAdministrativeAction);
    export.HiddenAdministrativeAppeal.Assign(import.HiddenAdministrativeAppeal);
    MoveCsePersonsWorkSet(import.HiddenCsePersonsWorkSet,
      export.HiddenCsePersonsWorkSet);

    export.Hidden.Index = 0;
    export.Hidden.Clear();

    for(import.Hidden.Index = 0; import.Hidden.Index < import.Hidden.Count; ++
      import.Hidden.Index)
    {
      if (export.Hidden.IsFull)
      {
        break;
      }

      export.Hidden.Update.HiddenCommon.SelectChar =
        import.Hidden.Item.HiddenCommon.SelectChar;
      MovePositionStatement(import.Hidden.Item.HiddenPositionStatement,
        export.Hidden.Update.HiddenPositionStatement);
      export.Hidden.Next();
    }

    if (!Equal(import.AdministrativeAppeal.Number,
      import.HiddenAdministrativeAppeal.Number))
    {
      export.AdministrativeAppeal.Identifier = 0;
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
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      var field = GetField(export.Standard, "nextTransaction");

      field.Error = true;

      return;
    }

    // PR139575 on 2-20-2002 changed the security call to include the "Add" 
    // command. L. Bachura
    if (Equal(global.Command, "CREATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "ADD"))
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // *********************************************
    // Verify that a display has been performed
    // before the UPDATE and DELETE can take place.
    // *********************************************
    if (Equal(global.Command, "UPDATE") && (
      import.AdministrativeAppeal.Identifier == 0 || import
      .AdministrativeAppeal.Identifier != import
      .HiddenAdministrativeAppeal.Identifier))
    {
      var field = GetField(export.AdministrativeAppeal, "number");

      field.Error = true;

      ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

      return;
    }

    if (Equal(global.Command, "DELETE") && (
      import.AdministrativeAppeal.Identifier == 0 || import
      .AdministrativeAppeal.Identifier != import
      .HiddenAdministrativeAppeal.Identifier))
    {
      var field = GetField(export.AdministrativeAppeal, "number");

      field.Error = true;

      ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

      return;
    }

    // *********************************************
    // Verify that admin appeal no is not changed
    // *********************************************
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      !
      Equal(import.AdministrativeAppeal.Number,
      import.HiddenAdministrativeAppeal.Number))
    {
      var field = GetField(export.AdministrativeAppeal, "number");

      field.Error = true;

      ExitState = "LE0000_ADM_APP_NO_CANT_BE_CHGD";

      return;
    }

    // *********************************************
    // Perform selection logic common for CREATEs,
    // UPDATEs, and DELETEs.
    // *********************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      local.Common.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            ++local.Common.Count;
          }
          else
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (local.Common.Count == 0)
      {
        // *********************************************
        // No Position Statement has been selected to
        // add, update, or delete.
        // *********************************************
        if (Equal(global.Command, "ADD"))
        {
          ExitState = "ACO_NI0000_NO_DATA_ADDED";
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        return;
      }
      else
      {
      }
    }

    // *********************************************
    // Perform validations common to both CREATEs
    // and UPDATEs.
    // *********************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(import.AdministrativeAppeal.Number))
      {
        var field = GetField(export.AdministrativeAppeal, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          if (IsEmpty(export.Export1.Item.PositionStatement.Explanation))
          {
            var field =
              GetField(export.Export1.Item.PositionStatement, "explanation");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }
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
      case "DISPLAY":
        // *********************************************
        // Required fields  EDIT LOGIC
        // *********************************************
        if (IsEmpty(export.CsePersonsWorkSet.Ssn) && IsEmpty
          (export.CsePersonsWorkSet.Number))
        {
          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          ExitState = "LE0000_CSE_NO_OR_SSN_REQD";

          return;
        }

        if (!IsEmpty(export.CsePersonsWorkSet.Ssn) && IsEmpty
          (export.CsePersonsWorkSet.Number))
        {
          UseFnReadCsePersonUsingSsnO();

          if (IsEmpty(local.AbendData.Type1))
          {
            export.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

            if (IsEmpty(export.CsePersonsWorkSet.Number))
            {
              var field1 = GetField(export.CsePersonsWorkSet, "number");

              field1.Error = true;

              var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

              field2.Error = true;

              var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

              field3.Error = true;

              var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

              field4.Error = true;

              ExitState = "LE0000_CSE_PERSON_NF_FOR_SSN";

              return;
            }
          }
          else
          {
            if (Equal(local.AbendData.AdabasFileAction, "SSN"))
            {
              var field1 = GetField(export.SsnWorkArea, "ssnNumPart1");

              field1.Error = true;

              var field2 = GetField(export.SsnWorkArea, "ssnNumPart2");

              field2.Error = true;

              var field3 = GetField(export.SsnWorkArea, "ssnNumPart3");

              field3.Error = true;

              ExitState = "ACO_ADABAS_NO_SSN_MATCH";
            }
            else
            {
              ExitState = "ACO_ADABAS_UNAVAILABLE";
            }

            return;
          }
        }

        UseLePostReadPositionStatement1();

        if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          export.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.Export1.IsEmpty)
          {
            ExitState = "LE0000_NO_POSITION_STMNT_ON_FILE";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
        else
        {
          if (IsExitState("CSE_PERSON_NF"))
          {
            var field1 = GetField(export.CsePersonsWorkSet, "number");

            field1.Error = true;

            export.CsePersonsWorkSet.FormattedName = "";
          }
          else if (IsExitState("LE0000_MULTIPLE_APPEALS_EXIT"))
          {
            export.HiddenSecurity1.LinkIndicator = "L";
            ExitState = "ECO_LNK_2_ADMN_APPEAL_BY_CSE_PER";
          }
          else
          {
            var field1 = GetField(export.AdministrativeAppeal, "number");

            field1.Error = true;
          }

          return;
        }

        break;
      case "EXIT":
        // ********************************************
        // Allows the user to flow back to the previous
        // screen.
        // ********************************************
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        if (!IsEmpty(export.PromptAdmAppeal.PromptField) && AsChar
          (export.PromptAdmAppeal.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.PromptAdmAppeal, "promptField");

          field1.Error = true;

          break;
        }

        if (AsChar(export.PromptAdmAppeal.PromptField) == 'S')
        {
          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_2_ADMN_APPEAL_BY_CSE_PER";

          return;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        var field = GetField(export.PromptAdmAppeal, "promptField");

        field.Error = true;

        break;
      case "ADD":
        // *********************************************
        // Read Administrative Appeal first to get the
        // Identifier to complete the CREATE.
        // *********************************************
        UseLePostReadPositionStatement3();

        // *********************************************
        // Insert the USE statement here to call the
        // CREATE action block.
        // *********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            UseDevelopPositionStatement();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Common.SelectChar = "";
            }
            else if (IsExitState("LE0000_ADMINISTRATIVE_APPEAL_NF"))
            {
              var field1 = GetField(export.AdministrativeAppeal, "number");

              field1.Error = true;

              return;
            }
            else
            {
              var field1 =
                GetField(export.Export1.Item.PositionStatement, "explanation");

              field1.Error = true;

              return;
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        // *********************************************
        // Insert the USE statement here to call the
        // UPDATE action block.
        // *********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            UseLePostUpdatePositionStatemnt();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Common.SelectChar = "";
            }
            else if (IsExitState("LE0000_ADMINISTRATIVE_APPEAL_NF"))
            {
              var field1 = GetField(export.AdministrativeAppeal, "number");

              field1.Error = true;

              return;
            }
            else
            {
              var field1 =
                GetField(export.Export1.Item.PositionStatement, "explanation");

              field1.Error = true;

              return;
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "DELETE":
        // *********************************************
        // Insert the USE statement here to call the
        // DELETE action block.
        // *********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            UseLePostDeletePositionStatemnt();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else if (IsExitState("LE0000_ADMINISTRATIVE_APPEAL_NF"))
            {
              var field1 = GetField(export.AdministrativeAppeal, "number");

              field1.Error = true;

              return;
            }
            else
            {
              var field1 =
                GetField(export.Export1.Item.PositionStatement, "explanation");

              field1.Error = true;

              return;
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseLePostReadPositionStatement2();
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      case "SIGNOFF":
        // ********************************************
        // Sign the user off the Kessep system
        // ********************************************
        // **** begin group F ****
        UseScCabSignoff();

        return;

        // **** end   group F ****
        break;
      case "AAPP":
        ExitState = "ECO_XFR_TO_ADMIN_APPEALS";

        break;
      case "AAAD":
        ExitState = "ECO_XFR_TO_ADMIN_APPEAL_ADDRESS";

        break;
      case "AHEA":
        ExitState = "ECO_XFR_TO_ADMIN_APPEAL_HEARING";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // *********************************************
    // If all processing completed successfully,
    // move all imports to previous exports .
    // *********************************************
    MoveAdministrativeAction(export.AdministrativeAction,
      export.HiddenAdministrativeAction);
    export.HiddenAdministrativeAppeal.Assign(export.AdministrativeAppeal);
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet,
      export.HiddenCsePersonsWorkSet);

    export.Hidden.Index = 0;
    export.Hidden.Clear();

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (export.Hidden.IsFull)
      {
        break;
      }

      export.Hidden.Update.HiddenCommon.SelectChar =
        export.Export1.Item.Common.SelectChar;
      MovePositionStatement(export.Export1.Item.PositionStatement,
        export.Hidden.Update.HiddenPositionStatement);
      export.Hidden.Next();
    }
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveAdministrativeAppeal(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1(LePostReadPositionStatement.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    MovePositionStatement(source.PositionStatement, target.PositionStatement);
  }

  private static void MovePositionStatement(PositionStatement source,
    PositionStatement target)
  {
    target.Number = source.Number;
    target.Explanation = source.Explanation;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.ConvertOption = source.ConvertOption;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(export.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = export.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseDevelopPositionStatement()
  {
    var useImport = new DevelopPositionStatement.Import();
    var useExport = new DevelopPositionStatement.Export();

    MovePositionStatement(export.Export1.Item.PositionStatement,
      useImport.PositionStatement);
    useImport.AdministrativeAppeal.Identifier =
      export.AdministrativeAppeal.Identifier;

    Call(DevelopPositionStatement.Execute, useImport, useExport);

    MovePositionStatement(useExport.PositionStatement,
      export.Export1.Update.PositionStatement);
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnReadCsePersonUsingSsnO()
  {
    var useImport = new FnReadCsePersonUsingSsnO.Import();
    var useExport = new FnReadCsePersonUsingSsnO.Export();

    useImport.CsePersonsWorkSet.Ssn = export.CsePersonsWorkSet.Ssn;
    useExport.AbendData.Assign(local.AbendData);
    useExport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(FnReadCsePersonUsingSsnO.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseLePostDeletePositionStatemnt()
  {
    var useImport = new LePostDeletePositionStatemnt.Import();
    var useExport = new LePostDeletePositionStatemnt.Export();

    useImport.PositionStatement.Number =
      export.Export1.Item.PositionStatement.Number;
    useImport.AdministrativeAppeal.Identifier =
      import.AdministrativeAppeal.Identifier;

    Call(LePostDeletePositionStatemnt.Execute, useImport, useExport);
  }

  private void UseLePostReadPositionStatement1()
  {
    var useImport = new LePostReadPositionStatement.Import();
    var useExport = new LePostReadPositionStatement.Export();

    MoveAdministrativeAppeal(import.AdministrativeAppeal,
      useImport.AdministrativeAppeal);
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

    Call(LePostReadPositionStatement.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
    export.AdministrativeAppeal.Assign(useExport.AdministrativeAppeal);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseLePostReadPositionStatement2()
  {
    var useImport = new LePostReadPositionStatement.Import();
    var useExport = new LePostReadPositionStatement.Export();

    MoveAdministrativeAppeal(import.AdministrativeAppeal,
      useImport.AdministrativeAppeal);
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

    Call(LePostReadPositionStatement.Execute, useImport, useExport);

    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
    export.AdministrativeAppeal.Assign(useExport.AdministrativeAppeal);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseLePostReadPositionStatement3()
  {
    var useImport = new LePostReadPositionStatement.Import();
    var useExport = new LePostReadPositionStatement.Export();

    MoveAdministrativeAppeal(import.AdministrativeAppeal,
      useImport.AdministrativeAppeal);
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

    Call(LePostReadPositionStatement.Execute, useImport, useExport);

    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
    export.AdministrativeAppeal.Assign(useExport.AdministrativeAppeal);
  }

  private void UseLePostUpdatePositionStatemnt()
  {
    var useImport = new LePostUpdatePositionStatemnt.Import();
    var useExport = new LePostUpdatePositionStatemnt.Export();

    useImport.AdministrativeAppeal.Identifier =
      import.AdministrativeAppeal.Identifier;
    MovePositionStatement(export.Export1.Item.PositionStatement,
      useImport.PositionStatement);

    Call(LePostUpdatePositionStatemnt.Execute, useImport, useExport);
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of PositionStatement.
      /// </summary>
      [JsonPropertyName("positionStatement")]
      public PositionStatement PositionStatement
      {
        get => positionStatement ??= new();
        set => positionStatement = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private PositionStatement positionStatement;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HiddenCommon.
      /// </summary>
      [JsonPropertyName("hiddenCommon")]
      public Common HiddenCommon
      {
        get => hiddenCommon ??= new();
        set => hiddenCommon = value;
      }

      /// <summary>
      /// A value of HiddenPositionStatement.
      /// </summary>
      [JsonPropertyName("hiddenPositionStatement")]
      public PositionStatement HiddenPositionStatement
      {
        get => hiddenPositionStatement ??= new();
        set => hiddenPositionStatement = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common hiddenCommon;
      private PositionStatement hiddenPositionStatement;
    }

    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
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
    /// A value of PromptAdmAppeal.
    /// </summary>
    [JsonPropertyName("promptAdmAppeal")]
    public Standard PromptAdmAppeal
    {
      get => promptAdmAppeal ??= new();
      set => promptAdmAppeal = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAction")]
    public AdministrativeAction HiddenAdministrativeAction
    {
      get => hiddenAdministrativeAction ??= new();
      set => hiddenAdministrativeAction = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// A value of HiddenAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAppeal")]
    public AdministrativeAppeal HiddenAdministrativeAppeal
    {
      get => hiddenAdministrativeAppeal ??= new();
      set => hiddenAdministrativeAppeal = value;
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
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
    }

    private SsnWorkArea ssnWorkArea;
    private Standard promptAdmAppeal;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAction administrativeAction;
    private Array<ImportGroup> import1;
    private AdministrativeAppeal administrativeAppeal;
    private Standard standard;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private AdministrativeAction hiddenAdministrativeAction;
    private Array<HiddenGroup> hidden;
    private AdministrativeAppeal hiddenAdministrativeAppeal;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of PositionStatement.
      /// </summary>
      [JsonPropertyName("positionStatement")]
      public PositionStatement PositionStatement
      {
        get => positionStatement ??= new();
        set => positionStatement = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private PositionStatement positionStatement;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HiddenCommon.
      /// </summary>
      [JsonPropertyName("hiddenCommon")]
      public Common HiddenCommon
      {
        get => hiddenCommon ??= new();
        set => hiddenCommon = value;
      }

      /// <summary>
      /// A value of HiddenPositionStatement.
      /// </summary>
      [JsonPropertyName("hiddenPositionStatement")]
      public PositionStatement HiddenPositionStatement
      {
        get => hiddenPositionStatement ??= new();
        set => hiddenPositionStatement = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common hiddenCommon;
      private PositionStatement hiddenPositionStatement;
    }

    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
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
    /// A value of PromptAdmAppeal.
    /// </summary>
    [JsonPropertyName("promptAdmAppeal")]
    public Standard PromptAdmAppeal
    {
      get => promptAdmAppeal ??= new();
      set => promptAdmAppeal = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAction")]
    public AdministrativeAction HiddenAdministrativeAction
    {
      get => hiddenAdministrativeAction ??= new();
      set => hiddenAdministrativeAction = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// A value of HiddenAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAppeal")]
    public AdministrativeAppeal HiddenAdministrativeAppeal
    {
      get => hiddenAdministrativeAppeal ??= new();
      set => hiddenAdministrativeAppeal = value;
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
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
    }

    private SsnWorkArea ssnWorkArea;
    private Standard promptAdmAppeal;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAction administrativeAction;
    private Array<ExportGroup> export1;
    private AdministrativeAppeal administrativeAppeal;
    private Standard standard;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private AdministrativeAction hiddenAdministrativeAction;
    private Array<HiddenGroup> hidden;
    private AdministrativeAppeal hiddenAdministrativeAppeal;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private TextWorkArea textWorkArea;
    private Common common;
    private NextTranInfo nextTranInfo;
  }
#endregion
}
