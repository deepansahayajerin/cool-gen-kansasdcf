// Program: FN_NADR_MTN_NON_CASE_ADDR, ID: 372248431, model: 746.
// Short name: SWENADRQ
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
/// A program: FN_NADR_MTN_NON_CASE_ADDR.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure adds, updates and deletes
/// addresses for APs and ARs.  It also allows requests for postmaster letters 
/// and updates any information pertaining to these letters.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnNadrMtnNonCaseAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_NADR_MTN_NON_CASE_ADDR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnNadrMtnNonCaseAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnNadrMtnNonCaseAddr.
  /// </summary>
  public FnNadrMtnNonCaseAddr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //       M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // ------------------------------------------------------------
    // 	
    // 06/12/01  M. Lachowicz  Do not allow verified and end date greater
    //                         than 6 months from today.  WR 283 A.
    // -------------------------------------------------------------------------------------
    // 01/15/99 M. Brown  Copied from ADDR and changed according to non Case 
    // requirements.
    // 08/31/00 M. Brown  PR# 102285.  Added ability to copy the AE address.
    // 3/7/02 L. Bachura added edited checks for PF5 and PF6 to prevent adding 
    // or updating for organizations State of Kansas and JJA.
    // --------------------------------------------------------------------------------------
    // 06/20/2002         Vithal Madhira       PR# 144710
    // If the person is an AP/AR on any case, the top line must be protected.
    // --------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    // 06/12/01 M.L Start
    local.TodayPlusSixMonths.Date = AddMonths(local.Current.Date, 6);

    // 06/12/01 M.L End
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.HiddenCsePersonsWorkSet.Number =
      import.HiddenCsePersonsWorkSet.Number;

    // ---------------------------------------------
    // 12/17/98 W.Campbell - Added the following
    // IF - ELSE stmt so that on a DISPLAY command
    // the EXPORT_EMPTY_ADDR views will not be
    // populated from the import view.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.Line1Sel.SelectChar = import.Line1Sel.SelectChar;
      export.Line1Addr.Assign(import.Line1Addr);

      // : mfb Aug 2000, pr#102285
      export.AeAddr.Assign(import.AeAddr);
      export.Line1EnddtCdPrmt.Text1 = import.Line1EnddtCdPrmt.Text1;
      export.Line1SrcePrmt.Text1 = import.Line1SrcePrmt.Text1;
      export.Line1StatePrmt.Text1 = import.Line1StatePrmt.Text1;
      export.Line1CntyPrmt.Text1 = import.Line1CntyPrmt.Text1;
      export.Line1TypePrmt.Text1 = import.Line1TypePrmt.Text1;
      export.Fips.Flag = import.Fips.Flag;
      export.FipsMessage.Text50 = import.FipsMessage.Text50;
      export.CsePersonPrompt.SelectChar = import.CsePersonPrompt.SelectChar;
      export.Minus.Text1 = import.Minus.Text1;
      export.Plus.Text1 = import.Plus.Text1;
      export.SaveSubscript.Subscript = import.SaveSubscript.Subscript;
      MoveCsePersonAddress11(import.LastAddr, export.LastAddr);
      export.PromptCode.CodeName = import.PromptCode.CodeName;
      MoveCodeValue(import.PromptCodeValue, export.PromptCodeValue);
      export.ForeignAddress.AddressLine1 = import.ForeignAddress.AddressLine1;
      export.ForeignAddr.Text1 = import.ForeignAddr.Text1;
      MoveCsePerson(import.HiddenCsePerson, export.HiddenCsePerson);
      export.ProtectFields.Flag = import.ProtectFields.Flag;
      MoveStandard(import.Standard, export.Standard);

      if (!import.Group.IsEmpty)
      {
        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (!import.Group.CheckSize())
          {
            break;
          }

          export.Group.Index = import.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.Sel.SelectChar =
            import.Group.Item.Common.SelectChar;
          MoveCsePersonAddress2(import.Group.Item.CsePersonAddress,
            export.Group.Update.CsePersonAddress);
          export.Group.Update.EnddtCdPrmt.Text1 =
            import.Group.Item.EnddtCdPrmt.Text1;
          export.Group.Update.SrcePrmt.Text1 = import.Group.Item.SrcePrmt.Text1;
          export.Group.Update.StatePrmt.Text1 =
            import.Group.Item.StatePrmt.Text1;
          export.Group.Update.CntyPrmt.Text1 = import.Group.Item.CntyPrmt.Text1;
          export.Group.Update.TypePrmt.Text1 = import.Group.Item.TypePrmt.Text1;
          export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);
        }

        import.Group.CheckIndex();
      }

      if (!import.Pagenum.IsEmpty)
      {
        for(import.Pagenum.Index = 0; import.Pagenum.Index < import
          .Pagenum.Count; ++import.Pagenum.Index)
        {
          if (!import.Pagenum.CheckSize())
          {
            break;
          }

          export.Pagenum.Index = import.Pagenum.Index;
          export.Pagenum.CheckSize();

          MoveCsePersonAddress11(import.Pagenum.Item.LastAddr,
            export.Pagenum.Update.Pagenum1);
        }

        import.Pagenum.CheckIndex();
      }
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CsePersonNumber =
        export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }
      else
      {
        // : Flowing out.
        return;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "NADS") || Equal(global.Command, "FIPS"))
    {
      // : Flow to NADS or FIPS
    }
    else if (Equal(global.Command, "RTFRMLNK"))
    {
      // : Coming back from NADS or FIPS.  Need to execute field protection 
      // logic
      //   at the bottom of the prad.
    }
    else if (Equal(global.Command, "PRMPTRET"))
    {
      // : Returning from a prompt list screen.  Need to execute field 
      // protection logic at the bottom of the prad.
    }
    else if (Equal(global.Command, "RETNAME"))
    {
      // : Returning from link to NAME
      if (Equal(export.CsePersonsWorkSet.Number,
        export.HiddenCsePersonsWorkSet.Number))
      {
        // : No change in person number coming back from NAME.  No further
        //   processing required, except field protection at the end of the 
        // logic.
      }
      else
      {
        global.Command = "DISPLAY";
      }
    }
    else if (Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "COPY"))
    {
      if (!IsEmpty(import.CsePersonsWorkSet.Number))
      {
        local.TextWorkArea.Text10 = import.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;

        if (ReadCsePerson())
        {
          if (AsChar(entities.CsePerson.Type1) == 'O')
          {
            if (!Equal(global.Command, "COPY"))
            {
              // need to see the authorzation for orgz, since that is where the 
              // actaul add and update are done for an organization
              local.Transaction.Trancode = "SR2P";

              if (Equal(global.Command, "ADD"))
              {
                local.Command1.Value = "CREATE";
              }
              else
              {
                local.Command1.Value = "";
              }

              UseScCabTestSecurity2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }
            else
            {
              UseScCabTestSecurity();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }
          }
          else
          {
            // a non organization
            UseScCabTestSecurity();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }

        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }
    else
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
    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // : This next section checks selects and prompts.  It is long,
      //   because the same code had to be duplicated for the empty line
      //   at the top of the screen.
      switch(AsChar(export.Line1Sel.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.Select.Count;

          break;
        case '*':
          export.Line1Sel.SelectChar = "";

          break;
        default:
          var field = GetField(export.Line1Sel, "selectChar");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      switch(AsChar(export.Line1StatePrmt.Text1))
      {
        case 'S':
          ++local.Prmpt.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Line1StatePrmt, "text1");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      switch(AsChar(export.Line1CntyPrmt.Text1))
      {
        case 'S':
          ++local.Prmpt.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Line1CntyPrmt, "text1");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      switch(AsChar(export.Line1TypePrmt.Text1))
      {
        case 'S':
          ++local.Prmpt.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Line1TypePrmt, "text1");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      switch(AsChar(export.Line1SrcePrmt.Text1))
      {
        case 'S':
          ++local.Prmpt.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Line1SrcePrmt, "text1");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      switch(AsChar(export.Line1EnddtCdPrmt.Text1))
      {
        case 'S':
          ++local.Prmpt.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Line1EnddtCdPrmt, "text1");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      if (local.Error.Count > 0)
      {
        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        goto Test1;
      }

      // : Find prompts to MAKE ERROR for the more than one promt
      //   selected condition.
      if (local.Prmpt.Count > 1)
      {
        if (AsChar(export.Line1StatePrmt.Text1) == 'S')
        {
          var field = GetField(export.Line1StatePrmt, "text1");

          field.Error = true;
        }

        if (AsChar(export.Line1CntyPrmt.Text1) == 'S')
        {
          var field = GetField(export.Line1CntyPrmt, "text1");

          field.Error = true;
        }

        if (AsChar(export.Line1TypePrmt.Text1) == 'S')
        {
          var field = GetField(export.Line1TypePrmt, "text1");

          field.Error = true;
        }

        if (AsChar(export.Line1SrcePrmt.Text1) == 'S')
        {
          var field = GetField(export.Line1SrcePrmt, "text1");

          field.Error = true;
        }

        if (AsChar(export.Line1EnddtCdPrmt.Text1) == 'S')
        {
          var field = GetField(export.Line1EnddtCdPrmt, "text1");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        goto Test1;
      }

      // : Check the lines after line 1.
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Group.Item.Sel.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Select.Count;

            if (local.Select.Count > 1)
            {
              var field1 = GetField(export.Group.Item.Sel, "selectChar");

              field1.Error = true;
            }

            break;
          case '*':
            export.Group.Update.Sel.SelectChar = "";

            break;
          default:
            var field = GetField(export.Group.Item.Sel, "selectChar");

            field.Error = true;

            ++local.Error.Count;

            break;
        }

        local.Prmpt.Count = 0;

        switch(AsChar(export.Group.Item.StatePrmt.Text1))
        {
          case 'S':
            ++local.Prmpt.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Group.Item.StatePrmt, "text1");

            field.Error = true;

            ++local.Error.Count;

            break;
        }

        switch(AsChar(export.Group.Item.CntyPrmt.Text1))
        {
          case 'S':
            ++local.Prmpt.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Group.Item.CntyPrmt, "text1");

            field.Error = true;

            ++local.Error.Count;

            break;
        }

        switch(AsChar(export.Group.Item.TypePrmt.Text1))
        {
          case 'S':
            ++local.Prmpt.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Group.Item.TypePrmt, "text1");

            field.Error = true;

            ++local.Error.Count;

            break;
        }

        switch(AsChar(export.Group.Item.SrcePrmt.Text1))
        {
          case 'S':
            ++local.Prmpt.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Group.Item.SrcePrmt, "text1");

            field.Error = true;

            ++local.Error.Count;

            break;
        }

        switch(AsChar(export.Group.Item.EnddtCdPrmt.Text1))
        {
          case 'S':
            ++local.Prmpt.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Group.Item.EnddtCdPrmt, "text1");

            field.Error = true;

            ++local.Error.Count;

            break;
        }

        if (local.Prmpt.Count > 1)
        {
          if (AsChar(export.Group.Item.StatePrmt.Text1) == 'S')
          {
            var field = GetField(export.Group.Item.StatePrmt, "text1");

            field.Error = true;
          }

          if (AsChar(export.Group.Item.CntyPrmt.Text1) == 'S')
          {
            var field = GetField(export.Group.Item.CntyPrmt, "text1");

            field.Error = true;
          }

          if (AsChar(export.Group.Item.TypePrmt.Text1) == 'S')
          {
            var field = GetField(export.Group.Item.TypePrmt, "text1");

            field.Error = true;
          }

          if (AsChar(export.Group.Item.SrcePrmt.Text1) == 'S')
          {
            var field = GetField(export.Group.Item.SrcePrmt, "text1");

            field.Error = true;
          }

          if (AsChar(export.Group.Item.EnddtCdPrmt.Text1) == 'S')
          {
            var field = GetField(export.Group.Item.EnddtCdPrmt, "text1");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          goto Test1;
        }
      }

      export.Group.CheckIndex();

      if (local.Select.Count > 0 && (Equal(global.Command, "PREV") || Equal
        (global.Command, "NEXT")))
      {
        ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

        goto Test1;
      }

      if (local.Error.Count > 0)
      {
        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        goto Test1;
      }

      if (local.Select.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

        goto Test1;
      }

      if (!Equal(export.HiddenCsePersonsWorkSet.Number,
        export.CsePersonsWorkSet.Number))
      {
        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          export.CsePersonsWorkSet.Number =
            export.HiddenCsePersonsWorkSet.Number;
        }
        else if (!Equal(global.Command, "DISPLAY"))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }
      }
    }

Test1:

    switch(TrimEnd(global.Command))
    {
      case "RETNAME":
        break;
      case "RTFRMLNK":
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.ForeignAddress.AddressLine1 = "";
        export.ForeignAddr.Text1 = "N";
        export.Fips.Flag = "N";
        export.FipsMessage.Text50 = "";
        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;

        // : Read CSE person and check for active foreign addresses.
        if (ReadCsePerson())
        {
          MoveCsePerson(entities.CsePerson, export.HiddenCsePerson);

          if (AsChar(entities.CsePerson.Type1) == 'O')
          {
            export.CsePersonsWorkSet.FormattedName =
              entities.CsePerson.OrganizationName ?? Spaces(33);

            if (ReadFips2())
            {
              // : If we are displaying a court, all fields must be protected.
              //   Addresses can be maintained on NADR for any other type of 
              // organization.
              export.FipsMessage.Text50 =
                "FIPS org, use FIPS to maintain addresses.";
              export.ProtectFields.Flag = "Y";
              export.Fips.Flag = "Y";
            }
            else
            {
              export.ProtectFields.Flag = "N";
            }
          }
          else
          {
            UseCabReadAdabasPerson();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // Per PR# 144710, if the person is an AP/AR on any case, the top 
              // line must be protected.  ( BR# 53)
              export.ProtectFields.Flag = "";

              if (ReadCaseRole1())
              {
                export.ProtectFields.Flag = "Y";
              }

              UseSiFormatCsePersonName();
            }
            else
            {
              var field = GetField(export.CsePersonsWorkSet, "number");

              field.Error = true;
            }
          }
        }
        else
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";
        }

        export.HiddenCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.BuildAddressList.Flag = "Y";
          export.Minus.Text1 = "";
        }
        else
        {
          // : Error in CSE person retrieval.
          return;
        }

        break;
      case "NEXT":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (IsEmpty(import.Plus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        ++export.Standard.PageNumber;

        export.Pagenum.Index = export.Standard.PageNumber - 1;
        export.Pagenum.CheckSize();

        MoveCsePersonAddress11(export.Pagenum.Item.Pagenum1, export.LastAddr);
        export.Group.Index = -1;
        export.Group.Count = 0;

        foreach(var item in ReadCsePersonAddress1())
        {
          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.CsePersonAddress.
            Assign(entities.CsePersonAddress);

          if (export.Group.IsFull)
          {
            break;
          }
        }

        if (export.Group.IsFull)
        {
          export.Plus.Text1 = "+";

          ++export.Pagenum.Index;
          export.Pagenum.CheckSize();

          export.Group.Index = Export.GroupGroup.Capacity - 2;
          export.Group.CheckSize();

          export.Pagenum.Update.Pagenum1.Identifier =
            export.Group.Item.CsePersonAddress.Identifier;
          export.Pagenum.Update.Pagenum1.EndDate =
            export.Group.Item.CsePersonAddress.EndDate;
        }
        else
        {
          export.Plus.Text1 = "";
        }

        export.Minus.Text1 = "-";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (Equal(export.Group.Item.CsePersonAddress.EndDate,
            local.Maximum.Date))
          {
            export.Group.Update.CsePersonAddress.EndDate = local.Zero.Date;
          }
        }

        export.Group.CheckIndex();

        break;
      case "PREV":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (IsEmpty(import.Minus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.Standard.PageNumber;

        export.Pagenum.Index = export.Standard.PageNumber - 1;
        export.Pagenum.CheckSize();

        MoveCsePersonAddress11(export.Pagenum.Item.Pagenum1, export.LastAddr);
        local.Command2.Command = global.Command;
        export.Group.Index = -1;
        export.Group.Count = 0;

        foreach(var item in ReadCsePersonAddress1())
        {
          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.CsePersonAddress.
            Assign(entities.CsePersonAddress);

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

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (Equal(export.Group.Item.CsePersonAddress.EndDate,
            local.Maximum.Date))
          {
            export.Group.Update.CsePersonAddress.EndDate = local.Zero.Date;
          }
        }

        export.Group.CheckIndex();

        break;
      case "ADD":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (Equal(export.CsePersonsWorkSet.Number, "000000017O") || Equal
          (export.CsePersonsWorkSet.Number, "000004029O"))
        {
          ExitState = "USE_ORGZ";

          return;
        }

        if (AsChar(export.ProtectFields.Flag) == 'Y')
        {
          if (AsChar(export.Fips.Flag) == 'Y')
          {
            // : Don't display a message if FIPS, even though the protection 
            // flag is on.
            //   There is a message line for a FIPS indicator literal.
          }
          else
          {
            ExitState = "FN0000_PERSON_IS_CASE_RELATED";
          }

          // : Escape to field protection logic.
          break;
        }

        // ***************************************************
        // Add action invalid unless select is on first line.
        // ***************************************************
        local.Select.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Group.Item.Sel.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              var field = GetField(export.Group.Item.Sel, "selectChar");

              field.Error = true;

              ++local.Select.Count;

              break;
            default:
              break;
          }

          if (local.Select.Count > 0)
          {
            ExitState = "ACO_NE0000_INVALID_ACTION";

            goto Test2;
          }
        }

        export.Group.CheckIndex();

        if (AsChar(export.Line1Sel.SelectChar) != 'S')
        {
          var field = GetField(export.Line1Sel, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        // *********************************************
        // Field validations
        // *********************************************
        if (IsEmpty(export.Line1Addr.Street1))
        {
          var field = GetField(export.Line1Addr, "street1");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }

        if (IsEmpty(export.Line1Addr.City))
        {
          var field = GetField(export.Line1Addr, "city");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }

        if (IsEmpty(export.Line1Addr.State))
        {
          var field = GetField(export.Line1Addr, "state");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }

        if (IsEmpty(export.Line1Addr.ZipCode))
        {
          var field = GetField(export.Line1Addr, "zipCode");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }

        if (IsEmpty(export.Line1Addr.Type1))
        {
          var field = GetField(export.Line1Addr, "type1");

          field.Error = true;

          if (IsExitState("ADDRESS_INCOMPLETE"))
          {
          }
          else
          {
            ExitState = "SI0000_ADDRESS_TYPE_REQUIRED";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (!IsEmpty(export.Line1Addr.State))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = import.Line1Addr.State ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field = GetField(export.Line1Addr, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            break;
          }
        }

        if (IsEmpty(export.Line1Addr.County))
        {
          if (Equal(import.Line1Addr.State, "KS"))
          {
            UseEabReturnKsCountyByZip2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }
        }
        else
        {
          local.Code.CodeName = "COUNTY CODE";
          local.CodeValue.Cdvalue = import.Line1Addr.County ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field = GetField(export.Line1Addr, "county");

            field.Error = true;

            ExitState = "INVALID_COUNTY";

            break;
          }
        }

        if (Length(TrimEnd(export.Line1Addr.ZipCode)) < 5)
        {
          var field = GetField(export.Line1Addr, "zipCode");

          field.Error = true;

          ExitState = "ACO_NE0000_ZIP_CODE_INCOMPLETE";

          break;
        }

        local.CheckZip.ZipCode = export.Line1Addr.ZipCode ?? "";
        UseSiCheckZipIsNumeric2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Line1Addr, "zipCode");

          field.Error = true;

          break;
        }

        // :lss 05/11/2007 PR# 00209846  The following statements are added to 
        // verify the zip4 field.
        if (Length(TrimEnd(export.Line1Addr.ZipCode)) > 0 && Length
          (TrimEnd(export.Line1Addr.Zip4)) > 0)
        {
          if (Length(TrimEnd(export.Line1Addr.Zip4)) < 4)
          {
            var field = GetField(export.Line1Addr, "zip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

            break;
          }
          else if (Verify(export.Line1Addr.Zip4, "0123456789") != 0)
          {
            var field = GetField(export.Line1Addr, "zip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            break;
          }
        }

        // : Edit address type.
        if (AsChar(export.Line1Addr.Type1) != 'R' && AsChar
          (export.Line1Addr.Type1) != 'M')
        {
          var field = GetField(export.Line1Addr, "type1");

          field.Error = true;

          ExitState = "INVALID_ADDRESS_TYPE";

          break;
        }

        local.Code.CodeName = "ADDRESS TYPE";
        local.CodeValue.Cdvalue = export.Line1Addr.Type1 ?? Spaces(10);
        UseCabValidateCodeValue();

        if (AsChar(local.ReturnCode.Flag) == 'N')
        {
          var field = GetField(export.Line1Addr, "type1");

          field.Error = true;

          ExitState = "INVALID_TYPE_CODE";

          break;
        }

        if (!IsEmpty(export.Line1Addr.Source))
        {
          local.Code.CodeName = "ADDRESS SOURCE";
          local.CodeValue.Cdvalue = import.Line1Addr.Source ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field = GetField(export.Line1Addr, "source");

            field.Error = true;

            ExitState = "INVALID_SOURCE";

            break;
          }
        }

        // : Edit VERIFIED DATE
        if (Equal(export.Line1Addr.VerifiedDate, local.Zero.Date))
        {
          export.Line1Addr.VerifiedDate = local.Current.Date;
        }
        else if (Lt(export.Line1Addr.VerifiedDate, local.Current.Date))
        {
          var field = GetField(export.Line1Addr, "verifiedDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

          break;
        }

        // 06/12/01 M.L Start
        if (Lt(local.TodayPlusSixMonths.Date, export.Line1Addr.VerifiedDate))
        {
          var field = GetField(export.Line1Addr, "verifiedDate");

          field.Error = true;

          ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

          break;
        }

        // 06/12/01 M.L End
        if (!IsEmpty(export.Line1Addr.EndCode))
        {
          if (Equal(export.Line1Addr.EndDate, local.Zero.Date))
          {
            // : For add, the end date must be entered with the end code.
            //   (for update, we default end date to current date if end code 
            // entered).
            var field = GetField(export.Line1Addr, "endDate");

            field.Error = true;

            ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

            break;
          }

          local.Code.CodeName = "ADDRESS END";
          local.CodeValue.Cdvalue = import.Line1Addr.EndCode ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field = GetField(export.Line1Addr, "endCode");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";

            break;
          }
        }
        else
        {
          // ---------------------------------------------
          // 01/04/99 W.Campbell - Logic added to make
          // sure END CODE is entered with END DATE.
          // Work done on IDCR454.
          // ---------------------------------------------
          if (!Equal(export.Line1Addr.EndDate, local.Zero.Date))
          {
            var field1 = GetField(export.Line1Addr, "endCode");

            field1.Error = true;

            var field2 = GetField(export.Line1Addr, "endDate");

            field2.Error = true;

            ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

            break;
          }
        }

        if (Equal(export.Line1Addr.EndDate, local.Zero.Date))
        {
          export.Line1Addr.EndDate = local.Maximum.Date;
        }
        else if (!Lt(Now().Date, export.Line1Addr.EndDate))
        {
          // : End date must be greater than current date on an add.
          var field = GetField(export.Line1Addr, "endDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_MUST_BE_FUTURE";

          break;
        }

        // 06/12/01 M.L Start
        if (Lt(local.TodayPlusSixMonths.Date, export.Line1Addr.EndDate) && Lt
          (export.Line1Addr.EndDate, local.Maximum.Date))
        {
          var field = GetField(export.Line1Addr, "endDate");

          field.Error = true;

          ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

          break;
        }

        // 06/12/01 M.L End
        local.CsePerson.Number = import.CsePersonsWorkSet.Number;
        export.Line1Addr.LocationType = "D";
        export.Line1Addr.WorkerId = global.UserId;
        UseSiCreateCsePersonAddress2();

        if (Equal(export.Line1Addr.EndDate, local.Maximum.Date))
        {
          export.Line1Addr.EndDate = local.Zero.Date;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : CSENET action block call.
          local.CsePerson.Number = export.CsePersonsWorkSet.Number;
          local.ScreenId.Command = "NADR";
          UseSiCreateAutoCsenetTrans();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          export.Line1Sel.SelectChar = local.BlankCommon.SelectChar;
          export.Line1Addr.Assign(local.BlankCsePersonAddress);
          local.BuildAddressList.Flag = "Y";
        }
        else
        {
        }

        break;
      case "UPDATE":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (Equal(export.CsePersonsWorkSet.Number, "000000017O") || Equal
          (export.CsePersonsWorkSet.Number, "000004029O"))
        {
          ExitState = "USE_ORGZ";

          return;
        }

        if (AsChar(export.ProtectFields.Flag) == 'Y')
        {
          if (AsChar(export.Fips.Flag) == 'Y')
          {
            // : Don't display a message if FIPS, even though the protection 
            // flag is on.
            //   There is a message line for a FIPS indicator literal.
          }
          else
          {
            ExitState = "FN0000_PERSON_IS_CASE_RELATED";
          }

          // : Escape to field protection logic.
          break;
        }

        if (AsChar(export.Line1Sel.SelectChar) == 'S')
        {
          var field = GetField(export.Line1Sel, "selectChar");

          field.Error = true;

          ExitState = "INVALID_UPDATE_THIS_ENTRY";

          break;
        }

        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Group.Item.Sel.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              local.SaveUpdates.Index = export.Group.Index;
              local.SaveUpdates.CheckSize();

              local.SaveUpdates.Update.AddrId.Identifier =
                export.Group.Item.CsePersonAddress.Identifier;

              // *********************************************
              // Field validations
              // *********************************************
              if (IsEmpty(export.Group.Item.CsePersonAddress.Street1))
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "street1");

                field.Error = true;

                ExitState = "ADDRESS_INCOMPLETE";
              }

              if (IsEmpty(export.Group.Item.CsePersonAddress.City))
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "city");

                field.Error = true;

                ExitState = "ADDRESS_INCOMPLETE";
              }

              if (IsEmpty(export.Group.Item.CsePersonAddress.State))
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "state");

                field.Error = true;

                ExitState = "ADDRESS_INCOMPLETE";
              }

              if (IsEmpty(export.Group.Item.CsePersonAddress.ZipCode))
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "zipCode");

                field.Error = true;

                ExitState = "ADDRESS_INCOMPLETE";
              }

              if (IsEmpty(export.Group.Item.CsePersonAddress.Type1))
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "type1");

                field.Error = true;

                if (IsExitState("ADDRESS_INCOMPLETE"))
                {
                }
                else
                {
                  ExitState = "SI0000_ADDRESS_TYPE_REQUIRED";
                }
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test2;
              }

              if (!IsEmpty(export.Group.Item.CsePersonAddress.State))
              {
                local.Code.CodeName = "STATE CODE";
                local.CodeValue.Cdvalue =
                  export.Group.Item.CsePersonAddress.State ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnCode.Flag) == 'N')
                {
                  var field =
                    GetField(export.Group.Item.CsePersonAddress, "state");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_STATE_CODE";

                  goto Test2;
                }
              }

              if (IsEmpty(export.Group.Item.CsePersonAddress.County))
              {
                if (Equal(export.Group.Item.CsePersonAddress.State, "KS"))
                {
                  UseEabReturnKsCountyByZip3();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    goto Test2;
                  }
                }
              }
              else
              {
                local.Code.CodeName = "COUNTY CODE";
                local.CodeValue.Cdvalue =
                  export.Group.Item.CsePersonAddress.County ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnCode.Flag) == 'N')
                {
                  var field =
                    GetField(export.Group.Item.CsePersonAddress, "county");

                  field.Error = true;

                  ExitState = "INVALID_COUNTY";

                  goto Test2;
                }
              }

              if (Length(TrimEnd(export.Group.Item.CsePersonAddress.ZipCode)) < 5
                )
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "zipCode");

                field.Error = true;

                ExitState = "ACO_NE0000_ZIP_CODE_INCOMPLETE";

                goto Test2;
              }

              local.CheckZip.ZipCode =
                export.Group.Item.CsePersonAddress.ZipCode ?? "";
              UseSiCheckZipIsNumeric2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "zipCode");

                field.Error = true;

                goto Test2;
              }

              // :lss 05/11/2007 PR# 00209846  The following statements are 
              // added to verify the zip4 field.
              if (Length(TrimEnd(export.Group.Item.CsePersonAddress.ZipCode)) >
                0 && Length
                (TrimEnd(export.Group.Item.CsePersonAddress.Zip4)) > 0)
              {
                if (Length(TrimEnd(export.Group.Item.CsePersonAddress.Zip4)) < 4
                  )
                {
                  var field =
                    GetField(export.Group.Item.CsePersonAddress, "zip4");

                  field.Error = true;

                  ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

                  goto Test2;
                }
                else if (Verify(export.Group.Item.CsePersonAddress.Zip4,
                  "0123456789") != 0)
                {
                  var field =
                    GetField(export.Group.Item.CsePersonAddress, "zip4");

                  field.Error = true;

                  ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

                  goto Test2;
                }
              }

              // : Edit Address Type
              if (AsChar(export.Group.Item.CsePersonAddress.Type1) != 'R' && AsChar
                (export.Group.Item.CsePersonAddress.Type1) != 'M')
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "type1");

                field.Error = true;

                ExitState = "INVALID_ADDRESS_TYPE";

                goto Test2;
              }

              local.Code.CodeName = "ADDRESS TYPE";
              local.CodeValue.Cdvalue =
                export.Group.Item.CsePersonAddress.Type1 ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnCode.Flag) == 'N')
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "type1");

                field.Error = true;

                ExitState = "LE0000_INVALID_ADDRESS_TYPE";

                goto Test2;
              }

              if (!IsEmpty(export.Group.Item.CsePersonAddress.Source))
              {
                local.Code.CodeName = "ADDRESS SOURCE";
                local.CodeValue.Cdvalue =
                  export.Group.Item.CsePersonAddress.Source ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnCode.Flag) == 'N')
                {
                  var field =
                    GetField(export.Group.Item.CsePersonAddress, "source");

                  field.Error = true;

                  ExitState = "INVALID_SOURCE";

                  goto Test2;
                }
              }

              // : If Verify date is not entered, set it to current date.
              if (Equal(export.Group.Item.CsePersonAddress.VerifiedDate,
                local.Zero.Date))
              {
                export.Group.Update.CsePersonAddress.VerifiedDate =
                  local.Current.Date;
              }
              else if (Lt(export.Group.Item.CsePersonAddress.VerifiedDate,
                Now().Date) && !
                Equal(export.Group.Item.CsePersonAddress.VerifiedDate,
                export.Group.Item.Hidden.VerifiedDate))
              {
                // : Verified date must be greater or equal to current date.
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "verifiedDate");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

                goto Test2;
              }

              // 06/12/01 M.L Start
              if (Lt(local.TodayPlusSixMonths.Date,
                export.Group.Item.CsePersonAddress.VerifiedDate))
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "verifiedDate");

                field.Error = true;

                ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

                goto Test2;
              }

              // 06/12/01 M.L End
              if (IsEmpty(export.Group.Item.CsePersonAddress.EndCode))
              {
                // : If end date is entered, end code must also be entered.
                if (!Equal(export.Group.Item.CsePersonAddress.EndDate,
                  local.Zero.Date))
                {
                  var field1 =
                    GetField(export.Group.Item.CsePersonAddress, "endCode");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Group.Item.CsePersonAddress, "endDate");

                  field2.Error = true;

                  ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

                  goto Test2;
                }
              }
              else
              {
                local.Code.CodeName = "ADDRESS END";
                local.CodeValue.Cdvalue =
                  export.Group.Item.CsePersonAddress.EndCode ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnCode.Flag) == 'N')
                {
                  var field =
                    GetField(export.Group.Item.CsePersonAddress, "endCode");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_CODE";

                  goto Test2;
                }

                // : If end date is blank, and end code is entered, set it to 
                // current date.
                if (Equal(export.Group.Item.CsePersonAddress.EndDate,
                  local.Zero.Date))
                {
                  export.Group.Update.CsePersonAddress.EndDate =
                    local.Current.Date;
                }
              }

              // : If end date is blank, set it to high date.
              if (Equal(export.Group.Item.CsePersonAddress.EndDate,
                local.Zero.Date))
              {
                export.Group.Update.CsePersonAddress.EndDate =
                  local.Maximum.Date;
              }

              if (Lt(export.Group.Item.CsePersonAddress.EndDate, Now().Date) &&
                !
                Equal(export.Group.Item.Hidden.EndDate,
                export.Group.Item.CsePersonAddress.EndDate))
              {
                // : End date must be greater than or equal to current date.
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "endDate");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

                goto Test2;
              }

              // 06/12/01 M.L Start
              if (Lt(local.TodayPlusSixMonths.Date,
                export.Group.Item.CsePersonAddress.EndDate) && Lt
                (export.Group.Item.CsePersonAddress.EndDate, local.Maximum.Date))
                
              {
                var field =
                  GetField(export.Group.Item.CsePersonAddress, "endDate");

                field.Error = true;

                ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

                goto Test2;
              }

              // 06/12/01 M.L End
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test2;
              }

              // : All edits have been passed.  Perform update logic.
              local.CsePerson.Number = export.CsePersonsWorkSet.Number;
              export.Group.Update.CsePersonAddress.WorkerId = global.UserId;
              UseSiUpdateCsePersonAddress();

              if (Equal(export.Group.Item.CsePersonAddress.EndDate,
                local.Maximum.Date))
              {
                export.Group.Update.CsePersonAddress.EndDate = local.Zero.Date;
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                // : CSENET action block call.
                local.CsePerson.Number = export.CsePersonsWorkSet.Number;
                local.ScreenId.Command = "NADR";
                UseSiCreateAutoCsenetTrans();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // : Rollback
                  return;
                }

                export.Group.Update.Sel.SelectChar = "*";
                MoveCsePersonAddress7(export.Group.Item.CsePersonAddress,
                  export.Group.Update.Hidden);
                ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
              }

              goto Test2;
            default:
              break;
          }
        }

        export.Group.CheckIndex();

        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "NADS":
        ExitState = "ECO_LNK_TO_NADS";

        return;
      case "LIST":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (!IsEmpty(export.CsePersonPrompt.SelectChar))
        {
          if (AsChar(export.CsePersonPrompt.SelectChar) == 'S')
          {
            export.CsePersonPrompt.SelectChar = "";
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          }
          else if (AsChar(export.CsePersonPrompt.SelectChar) == '+')
          {
          }
          else
          {
            var field = GetField(export.CsePersonPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (AsChar(export.Line1Sel.SelectChar) == 'S')
        {
          if (AsChar(export.Line1EnddtCdPrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "ADDRESS END";
          }
          else if (AsChar(export.Line1SrcePrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "ADDRESS SOURCE";
          }
          else if (AsChar(export.Line1StatePrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "STATE CODE";
          }
          else if (AsChar(export.Line1CntyPrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "COUNTY CODE";
          }
          else if (AsChar(export.Line1TypePrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "ADDRESS TYPE";
          }
          else
          {
            var field1 = GetField(export.Line1StatePrmt, "text1");

            field1.Error = true;

            var field2 = GetField(export.Line1CntyPrmt, "text1");

            field2.Error = true;

            var field3 = GetField(export.Line1SrcePrmt, "text1");

            field3.Error = true;

            var field4 = GetField(export.Line1TypePrmt, "text1");

            field4.Error = true;

            var field5 = GetField(export.Line1EnddtCdPrmt, "text1");

            field5.Error = true;

            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          }

          ExitState = "ECO_LNK_TO_CODE_TABLES";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.Sel.SelectChar) == 'S')
          {
            if (AsChar(export.Group.Item.EnddtCdPrmt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "ADDRESS END";
            }
            else if (AsChar(export.Group.Item.SrcePrmt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "ADDRESS SOURCE";
            }
            else if (AsChar(export.Group.Item.StatePrmt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "STATE CODE";
            }
            else if (AsChar(export.Group.Item.CntyPrmt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "COUNTY CODE";
            }
            else if (AsChar(export.Group.Item.TypePrmt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "ADDRESS TYPE";
            }
            else
            {
              var field1 = GetField(export.Group.Item.StatePrmt, "text1");

              field1.Error = true;

              var field2 = GetField(export.Group.Item.CntyPrmt, "text1");

              field2.Error = true;

              var field3 = GetField(export.Group.Item.TypePrmt, "text1");

              field3.Error = true;

              var field4 = GetField(export.Group.Item.SrcePrmt, "text1");

              field4.Error = true;

              var field5 = GetField(export.Group.Item.EnddtCdPrmt, "text1");

              field5.Error = true;

              ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

              goto Test2;
            }

            ExitState = "ECO_LNK_TO_CODE_TABLES";

            return;
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "PRMPTRET":
        // ---------------------------------------------
        // When the control is returned from a promting
        // screen, populate the data appropriately.
        // ---------------------------------------------
        if (AsChar(export.Line1Sel.SelectChar) == 'S')
        {
          if (AsChar(export.Line1EnddtCdPrmt.Text1) == 'S')
          {
            export.Line1EnddtCdPrmt.Text1 = "";

            var field = GetField(export.Line1EnddtCdPrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.Line1Addr.EndCode = import.PromptCodeValue.Cdvalue;

              break;
            }
          }
          else if (AsChar(export.Line1SrcePrmt.Text1) == 'S')
          {
            export.Line1SrcePrmt.Text1 = "";

            var field = GetField(export.Line1SrcePrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.Line1Addr.Source = import.PromptCodeValue.Cdvalue;

              break;
            }
          }
          else if (AsChar(export.Line1StatePrmt.Text1) == 'S')
          {
            export.Line1StatePrmt.Text1 = "";

            var field = GetField(export.Line1StatePrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.Line1Addr.State = import.PromptCodeValue.Cdvalue;

              break;
            }
          }
          else if (AsChar(export.Line1CntyPrmt.Text1) == 'S')
          {
            export.Line1CntyPrmt.Text1 = "";

            var field = GetField(export.Line1CntyPrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.Line1Addr.County = import.PromptCodeValue.Cdvalue;

              break;
            }
          }
          else if (AsChar(export.Line1TypePrmt.Text1) == 'S')
          {
            export.Line1TypePrmt.Text1 = "";

            var field = GetField(export.Line1TypePrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.Line1Addr.Type1 = import.PromptCodeValue.Cdvalue;

              if (AsChar(export.Line1Addr.Type1) == 'S')
              {
                var field1 = GetField(export.Line1Addr, "type1");

                field1.Error = true;

                ExitState = "INVALID_ADDRESS_TYPE";
              }

              break;
            }
          }
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.Sel.SelectChar) == 'S')
          {
            if (AsChar(export.Group.Item.EnddtCdPrmt.Text1) == 'S')
            {
              export.Group.Update.EnddtCdPrmt.Text1 = "";

              var field = GetField(export.Group.Item.EnddtCdPrmt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Group.Update.CsePersonAddress.EndCode =
                  import.PromptCodeValue.Cdvalue;

                goto Test2;
              }
            }
            else if (AsChar(export.Group.Item.SrcePrmt.Text1) == 'S')
            {
              export.Group.Update.SrcePrmt.Text1 = "";

              var field = GetField(export.Group.Item.SrcePrmt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Group.Update.CsePersonAddress.Source =
                  import.PromptCodeValue.Cdvalue;

                goto Test2;
              }
            }
            else if (AsChar(export.Group.Item.StatePrmt.Text1) == 'S')
            {
              export.Group.Update.StatePrmt.Text1 = "";

              var field = GetField(export.Group.Item.StatePrmt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Group.Update.CsePersonAddress.State =
                  import.PromptCodeValue.Cdvalue;

                goto Test2;
              }
            }
            else if (AsChar(export.Group.Item.CntyPrmt.Text1) == 'S')
            {
              export.Group.Update.CntyPrmt.Text1 = "";

              var field = GetField(export.Group.Item.CntyPrmt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Group.Update.CsePersonAddress.County =
                  import.PromptCodeValue.Cdvalue;

                goto Test2;
              }
            }
            else if (AsChar(export.Group.Item.TypePrmt.Text1) == 'S')
            {
              export.Group.Update.TypePrmt.Text1 = "";

              var field = GetField(export.Group.Item.TypePrmt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Group.Update.CsePersonAddress.Type1 =
                  import.PromptCodeValue.Cdvalue;

                if (AsChar(export.Group.Item.CsePersonAddress.Type1) == 'S')
                {
                  var field1 =
                    GetField(export.Group.Item.CsePersonAddress, "type1");

                  field1.Error = true;

                  ExitState = "INVALID_ADDRESS_TYPE";
                }

                goto Test2;
              }
            }
          }
          else
          {
          }
        }

        export.Group.CheckIndex();

        break;
      case "FIPS":
        if (ReadFips1())
        {
          export.PassToFips.Assign(entities.Fips);
          ExitState = "ECO_LNK_TO_FIPS";

          return;
        }
        else
        {
          ExitState = "FN0000_FIPS_FLOW_INVALID";
        }

        break;
      case "COPY":
        // : mfb, Aug. 2000, PR# 102285
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // -------------------------------------------------------------------------------------
        // Per PR# 141525, the following code is added. This code will let the 
        // user add the AE address if the cse_person's family role is 'FA' or '
        // MO'  but not playing AP/AR role on any case whether active or
        // inactive.
        //                                                      
        // Vithal Madhira (04/16/2002)
        // --------------------------------------------------------------------------------------
        if (ReadCaseRole2())
        {
          if (ReadCaseRole1())
          {
            // -------------------------------------------------------------------------------------
            // The person is playing 'AP' or 'AR' role on some case. Add the 
            // address on ADDR or FADS.
            //                                                      
            // Vithal Madhira (04/16/2002)
            // --------------------------------------------------------------------------------------
            ExitState = "FN0000_PERSON_IS_CASE_RELATED";

            break;
          }

          export.AeAddr.Type1 = "M";

          if (!IsEmpty(export.AeAddr.ZipCode) && IsEmpty(export.AeAddr.County))
          {
            UseEabReturnKsCountyByZip1();
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        // *********************************************
        // Copy action invalid on existing addresses
        // *********************************************
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Group.Item.Sel.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              var field = GetField(export.Group.Item.Sel, "selectChar");

              field.Error = true;

              ++local.Select.Count;

              break;
            default:
              break;
          }

          if (local.Select.Count > 0)
          {
            ExitState = "ACO_NE0000_INVALID_ACTION";

            return;
          }
        }

        export.Group.CheckIndex();

        // *********************************************
        // Field validations
        // *********************************************
        if (IsEmpty(export.AeAddr.Street1))
        {
          ExitState = "ADDRESS_AE_INVALID";

          break;
        }

        if (IsEmpty(export.AeAddr.City))
        {
          ExitState = "ADDRESS_AE_INVALID";

          break;
        }

        if (IsEmpty(export.AeAddr.State))
        {
          ExitState = "ADDRESS_AE_INVALID";

          break;
        }

        if (IsEmpty(export.AeAddr.ZipCode))
        {
          ExitState = "ADDRESS_AE_INVALID";

          break;
        }
        else
        {
          local.CheckZip.ZipCode = export.AeAddr.ZipCode ?? "";
          UseSiCheckZipIsNumeric1();

          if (AsChar(local.NumericZip.Flag) == 'N')
          {
            ExitState = "ADDRESS_AE_INVALID";

            break;
          }

          // :lss 05/11/2007 PR# 00209846  The following statements are added to
          // verify the zip4 field.
          if (Length(TrimEnd(export.AeAddr.ZipCode)) > 0 && Length
            (TrimEnd(export.AeAddr.Zip4)) > 0)
          {
            if (Length(TrimEnd(export.AeAddr.Zip4)) < 4)
            {
              ExitState = "ADDRESS_AE_INVALID";

              break;
            }
            else if (Verify(export.AeAddr.Zip4, "0123456789") != 0)
            {
              ExitState = "ADDRESS_AE_INVALID";

              break;
            }
          }
        }

        if (!IsEmpty(export.AeAddr.State))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = import.AeAddr.State ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            ExitState = "ADDRESS_AE_INVALID";

            break;
          }
        }

        if (IsEmpty(export.AeAddr.Type1))
        {
          export.AeAddr.Type1 = "R";
        }

        export.AeAddr.LocationType = "D";
        export.AeAddr.Source = "AE";
        export.AeAddr.VerifiedDate = Now().Date;
        UseSiCreateCsePersonAddress1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.BuildAddressList.Flag = "Y";
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test2:

    // --------------------------------
    // Display Address List as required
    // by above functions.
    // --------------------------------
    if (AsChar(local.BuildAddressList.Flag) == 'Y')
    {
      export.Group.Count = 0;
      export.Pagenum.Count = 0;
      export.LastAddr.Identifier = Now().AddDays(1);
      export.LastAddr.EndDate = UseCabSetMaximumDiscontinueDate();
      local.Command2.Command = global.Command;

      // : mfb Aug, 2000, pr# 102285
      if (IsEmpty(export.AeAddr.WorkerId))
      {
        // : Only need to get the ae address if it isn't already populated.
      }

      local.AddressType.Flag = "R";
      UseCabReadAdabasAddress();

      if (AsChar(export.Fips.Flag) == 'Y')
      {
        // : Display addresses from fips tribunal address entity.
        export.Group.Index = -1;

        foreach(var item in ReadFipsTribAddress())
        {
          if (!Equal(entities.FipsTribAddress.Country, "US") && !
            IsEmpty(entities.FipsTribAddress.Country) && IsEmpty
            (entities.FipsTribAddress.State))
          {
            continue;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.CsePersonAddress.Street1 =
            entities.FipsTribAddress.Street1;
          export.Group.Update.CsePersonAddress.Street2 =
            entities.FipsTribAddress.Street2;
          export.Group.Update.CsePersonAddress.City =
            entities.FipsTribAddress.City;
          export.Group.Update.CsePersonAddress.State =
            entities.FipsTribAddress.State;
          export.Group.Update.CsePersonAddress.County =
            entities.FipsTribAddress.County;
          export.Group.Update.CsePersonAddress.Type1 =
            entities.FipsTribAddress.Type1;
          export.Group.Update.CsePersonAddress.ZipCode =
            entities.FipsTribAddress.ZipCode;
          export.Group.Update.CsePersonAddress.Zip4 =
            entities.FipsTribAddress.Zip4;
          export.Group.Update.CsePersonAddress.LastUpdatedBy =
            entities.FipsTribAddress.LastUpdatedBy;
          export.Group.Update.CsePersonAddress.Identifier =
            entities.FipsTribAddress.CreatedTstamp;
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "FN0000_NO_FIPS_ADDRESS_FOUND";

          goto Test3;
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
      }
      else
      {
        // : No FIPS found - use regular address build.
        export.Group.Index = -1;
        export.ForeignAddress.AddressLine1 = "";

        foreach(var item in ReadCsePersonAddress2())
        {
          if (AsChar(entities.CsePersonAddress.LocationType) == 'D')
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.CsePersonAddress.Assign(
              entities.CsePersonAddress);
            MoveCsePersonAddress7(entities.CsePersonAddress,
              export.Group.Update.Hidden);
          }
          else if (AsChar(entities.CsePersonAddress.LocationType) == 'F')
          {
            // -------------------------------------------------------------
            // Display "Foreign Address" on NADR when the address is active.
            // The Address end date must be checked to determine if active.
            // These addressess are not displayed on NADR.
            // -----------------------------------------------------------
            if (Lt(Now().Date, entities.CsePersonAddress.EndDate))
            {
              export.ForeignAddress.AddressLine1 = "Foreign Address";
              export.ForeignAddr.Text1 = "Y";
            }
          }

          if (export.Group.IsFull)
          {
            break;
          }
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          goto Test3;
        }
        else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
          ("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
      }

      export.Standard.PageNumber = 1;

      // *********************************************
      // Set up group pagenum keys with the first
      // and the 4th occurrence of the address
      // identifiers
      // *********************************************
      if (export.Group.IsFull)
      {
        export.Plus.Text1 = "+";

        export.Pagenum.Index = 0;
        export.Pagenum.CheckSize();

        export.Group.Index = 0;
        export.Group.CheckSize();

        export.Pagenum.Update.Pagenum1.Identifier = Now().AddDays(1);
        export.Pagenum.Update.Pagenum1.EndDate =
          export.Group.Item.CsePersonAddress.EndDate;

        ++export.Pagenum.Index;
        export.Pagenum.CheckSize();

        export.Group.Index = Export.GroupGroup.Capacity - 2;
        export.Group.CheckSize();

        export.Pagenum.Update.Pagenum1.Identifier =
          export.Group.Item.CsePersonAddress.Identifier;
        export.Pagenum.Update.Pagenum1.EndDate =
          export.Group.Item.CsePersonAddress.EndDate;
      }
      else
      {
        export.Plus.Text1 = "";
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (Equal(export.Group.Item.CsePersonAddress.EndDate, local.Maximum.Date))
          
        {
          export.Group.Update.CsePersonAddress.EndDate = local.Zero.Date;
          export.Group.Update.Hidden.EndDate = local.Zero.Date;
        }

        if (Equal(export.Group.Item.CsePersonAddress.Identifier,
          local.Asterisk.Identifier))
        {
          export.Group.Update.Sel.SelectChar = "*";
        }
      }

      export.Group.CheckIndex();
    }

Test3:

    // *****     Field Protection     *****
    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      // : Don't protect lines that have been selected.
      if (AsChar(export.Group.Item.Sel.SelectChar) == 'S')
      {
        continue;
      }

      if (AsChar(export.ProtectFields.Flag) == 'Y' || !
        Lt(Now().Date, export.Group.Item.CsePersonAddress.EndDate) && !
        Equal(export.Group.Item.CsePersonAddress.EndDate, local.Zero.Date))
      {
        var field1 = GetField(export.Group.Item.CntyPrmt, "text1");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Group.Item.EnddtCdPrmt, "text1");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Group.Item.StatePrmt, "text1");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.Group.Item.TypePrmt, "text1");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Group.Item.SrcePrmt, "text1");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Group.Item.CsePersonAddress, "city");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Group.Item.CsePersonAddress, "county");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.Group.Item.CsePersonAddress, "endCode");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.Group.Item.CsePersonAddress, "endDate");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 =
          GetField(export.Group.Item.CsePersonAddress, "lastUpdatedTimestamp");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.Group.Item.CsePersonAddress, "source");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.Group.Item.CsePersonAddress, "state");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.Group.Item.CsePersonAddress, "street1");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.Group.Item.CsePersonAddress, "street2");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.Group.Item.CsePersonAddress, "type1");

        field15.Color = "cyan";
        field15.Protected = true;

        var field16 =
          GetField(export.Group.Item.CsePersonAddress, "verifiedDate");

        field16.Color = "cyan";
        field16.Protected = true;

        var field17 = GetField(export.Group.Item.CsePersonAddress, "workerId");

        field17.Color = "cyan";
        field17.Protected = true;

        var field18 = GetField(export.Group.Item.CsePersonAddress, "zipCode");

        field18.Color = "cyan";
        field18.Protected = true;

        var field19 = GetField(export.Group.Item.CsePersonAddress, "zip4");

        field19.Color = "cyan";
        field19.Protected = true;

        var field20 = GetField(export.Group.Item.Sel, "selectChar");

        field20.Color = "cyan";
        field20.Protected = true;
      }
    }

    export.Group.CheckIndex();

    // : If first line is selected, don't protect, because we don't want to 
    // protect for 'end date less than current date' edit error.
    if (AsChar(export.Line1Sel.SelectChar) == 'S')
    {
      return;
    }

    if (AsChar(export.ProtectFields.Flag) == 'Y' || !
      Lt(Now().Date, export.Line1Addr.EndDate) && !
      Equal(export.Line1Addr.EndDate, local.Zero.Date))
    {
      var field1 = GetField(export.Line1Addr, "city");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Line1Addr, "county");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.Line1Addr, "endCode");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.Line1Addr, "endDate");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.Line1Addr, "lastUpdatedTimestamp");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.Line1Addr, "source");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.Line1Addr, "state");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.Line1Addr, "street1");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.Line1Addr, "street2");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.Line1Addr, "type1");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.Line1Addr, "verifiedDate");

      field11.Color = "cyan";
      field11.Protected = true;

      var field12 = GetField(export.Line1Addr, "workerId");

      field12.Color = "cyan";
      field12.Protected = true;

      var field13 = GetField(export.Line1Addr, "zipCode");

      field13.Color = "cyan";
      field13.Protected = true;

      var field14 = GetField(export.Line1Addr, "zip4");

      field14.Color = "cyan";
      field14.Protected = true;

      var field15 = GetField(export.Line1CntyPrmt, "text1");

      field15.Color = "cyan";
      field15.Protected = true;

      var field16 = GetField(export.Line1EnddtCdPrmt, "text1");

      field16.Color = "cyan";
      field16.Protected = true;

      var field17 = GetField(export.Line1SrcePrmt, "text1");

      field17.Color = "cyan";
      field17.Protected = true;

      var field18 = GetField(export.Line1StatePrmt, "text1");

      field18.Color = "cyan";
      field18.Protected = true;

      var field19 = GetField(export.Line1TypePrmt, "text1");

      field19.Color = "cyan";
      field19.Protected = true;

      var field20 = GetField(export.Line1Sel, "selectChar");

      field20.Color = "cyan";
      field20.Protected = true;
    }

    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress3(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress4(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress5(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress6(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress7(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.State = source.State;
  }

  private static void MoveCsePersonAddress8(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.County = source.County;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonAddress9(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonAddress10(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private static void MoveCsePersonAddress11(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.Identifier = source.Identifier;
    target.EndDate = source.EndDate;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabReadAdabasAddress()
  {
    var useImport = new CabReadAdabasAddress.Import();
    var useExport = new CabReadAdabasAddress.Export();

    useImport.AddressType.Flag = local.AddressType.Flag;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabReadAdabasAddress.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonAddress6(useExport.Ae, export.AeAddr);
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CseFlag.Flag = useExport.Cse.Flag;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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

    local.ReturnCode.Flag = useExport.ValidCode.Flag;
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

  private void UseEabReturnKsCountyByZip1()
  {
    var useImport = new EabReturnKsCountyByZip.Import();
    var useExport = new EabReturnKsCountyByZip.Export();

    MoveCsePersonAddress9(export.AeAddr, useImport.CsePersonAddress);
    MoveCsePersonAddress8(export.AeAddr, useExport.CsePersonAddress);

    Call(EabReturnKsCountyByZip.Execute, useImport, useExport);

    MoveCsePersonAddress8(useExport.CsePersonAddress, export.AeAddr);
  }

  private void UseEabReturnKsCountyByZip2()
  {
    var useImport = new EabReturnKsCountyByZip.Import();
    var useExport = new EabReturnKsCountyByZip.Export();

    MoveCsePersonAddress9(export.Line1Addr, useImport.CsePersonAddress);
    MoveCsePersonAddress8(export.Line1Addr, useExport.CsePersonAddress);

    Call(EabReturnKsCountyByZip.Execute, useImport, useExport);

    MoveCsePersonAddress8(useExport.CsePersonAddress, export.Line1Addr);
  }

  private void UseEabReturnKsCountyByZip3()
  {
    var useImport = new EabReturnKsCountyByZip.Import();
    var useExport = new EabReturnKsCountyByZip.Export();

    MoveCsePersonAddress9(export.Group.Item.CsePersonAddress,
      useImport.CsePersonAddress);
    MoveCsePersonAddress8(export.Group.Item.CsePersonAddress,
      useExport.CsePersonAddress);

    Call(EabReturnKsCountyByZip.Execute, useImport, useExport);

    MoveCsePersonAddress8(useExport.CsePersonAddress,
      export.Group.Update.CsePersonAddress);
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

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity2()
  {
    var useImport = new ScCabTestSecurity2.Import();
    var useExport = new ScCabTestSecurity2.Export();

    useImport.Command.Value = local.Command1.Value;
    useImport.Transaction.Trancode = local.Transaction.Trancode;
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity2.Execute, useImport, useExport);
  }

  private void UseSiCheckZipIsNumeric1()
  {
    var useImport = new SiCheckZipIsNumeric.Import();
    var useExport = new SiCheckZipIsNumeric.Export();

    MoveCsePersonAddress9(local.CheckZip, useImport.CsePersonAddress);

    Call(SiCheckZipIsNumeric.Execute, useImport, useExport);

    local.NumericZip.Flag = useExport.NumericZip.Flag;
  }

  private void UseSiCheckZipIsNumeric2()
  {
    var useImport = new SiCheckZipIsNumeric.Import();
    var useExport = new SiCheckZipIsNumeric.Export();

    MoveCsePersonAddress9(local.CheckZip, useImport.CsePersonAddress);

    Call(SiCheckZipIsNumeric.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.ScreenIdentification.Command = local.ScreenId.Command;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePersonAddress1()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    useImport.CsePerson.Number = export.HiddenCsePerson.Number;
    MoveCsePersonAddress4(export.AeAddr, useImport.CsePersonAddress);

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress, export.AeAddr);
  }

  private void UseSiCreateCsePersonAddress2()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveCsePersonAddress5(export.Line1Addr, useImport.CsePersonAddress);

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress2(useExport.CsePersonAddress, export.Line1Addr);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiUpdateCsePersonAddress()
  {
    var useImport = new SiUpdateCsePersonAddress.Import();
    var useExport = new SiUpdateCsePersonAddress.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveCsePersonAddress3(export.Group.Item.CsePersonAddress,
      useImport.CsePersonAddress);

    Call(SiUpdateCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress10(useExport.CsePersonAddress,
      export.Group.Update.CsePersonAddress);
  }

  private bool ReadCaseRole1()
  {
    entities.ApAr.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ApAr.CasNumber = db.GetString(reader, 0);
        entities.ApAr.CspNumber = db.GetString(reader, 1);
        entities.ApAr.Type1 = db.GetString(reader, 2);
        entities.ApAr.Identifier = db.GetInt32(reader, 3);
        entities.ApAr.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApAr.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.FaMo.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.FaMo.CasNumber = db.GetString(reader, 0);
        entities.FaMo.CspNumber = db.GetString(reader, 1);
        entities.FaMo.Type1 = db.GetString(reader, 2);
        entities.FaMo.Identifier = db.GetInt32(reader, 3);
        entities.FaMo.Populated = true;
        CheckValid<CaseRole>("Type1", entities.FaMo.Type1);
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
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 20);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 21);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 22);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 23);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 24);
        entities.CsePerson.CreatedBy = db.GetString(reader, 25);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 26);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 27);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 28);
        entities.CsePerson.KscaresNumber = db.GetNullableString(reader, 29);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 30);
        entities.CsePerson.EmergencyAreaCode = db.GetNullableInt32(reader, 31);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 32);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 33);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 34);
        entities.CsePerson.WorkPhoneExt = db.GetNullableString(reader, 35);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 36);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 37);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", export.LastAddr.EndDate.GetValueOrDefault());
        db.SetDateTime(
          command, "identifier",
          export.LastAddr.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 9);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 17);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", export.LastAddr.EndDate.GetValueOrDefault());
        db.SetDateTime(
          command, "identifier",
          export.LastAddr.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 9);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 17);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
      });
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CspNumber = db.GetNullableString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CspNumber = db.GetNullableString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return ReadEach("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", export.CsePersonsWorkSet.Number);
        db.SetDateTime(
          command, "createdTstamp",
          export.LastAddr.Identifier.GetValueOrDefault());
        db.SetDateTime(
          command, "identifier",
          local.BlankCsePersonAddress.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.FaxExtension = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FaxAreaCode = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.FipsTribAddress.AreaCode = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.City = db.GetString(reader, 8);
        entities.FipsTribAddress.State = db.GetString(reader, 9);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 12);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 13);
        entities.FipsTribAddress.Street3 = db.GetNullableString(reader, 14);
        entities.FipsTribAddress.Street4 = db.GetNullableString(reader, 15);
        entities.FipsTribAddress.Province = db.GetNullableString(reader, 16);
        entities.FipsTribAddress.PostalCode = db.GetNullableString(reader, 17);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 18);
        entities.FipsTribAddress.PhoneNumber = db.GetNullableInt32(reader, 19);
        entities.FipsTribAddress.FaxNumber = db.GetNullableInt32(reader, 20);
        entities.FipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.FipsTribAddress.CreatedTstamp = db.GetDateTime(reader, 22);
        entities.FipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.FipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 25);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 26);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 27);
        entities.FipsTribAddress.Populated = true;

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
      /// A value of CsePersonAddress.
      /// </summary>
      [JsonPropertyName("csePersonAddress")]
      public CsePersonAddress CsePersonAddress
      {
        get => csePersonAddress ??= new();
        set => csePersonAddress = value;
      }

      /// <summary>
      /// A value of StatePrmt.
      /// </summary>
      [JsonPropertyName("statePrmt")]
      public WorkArea StatePrmt
      {
        get => statePrmt ??= new();
        set => statePrmt = value;
      }

      /// <summary>
      /// A value of EnddtCdPrmt.
      /// </summary>
      [JsonPropertyName("enddtCdPrmt")]
      public WorkArea EnddtCdPrmt
      {
        get => enddtCdPrmt ??= new();
        set => enddtCdPrmt = value;
      }

      /// <summary>
      /// A value of SrcePrmt.
      /// </summary>
      [JsonPropertyName("srcePrmt")]
      public WorkArea SrcePrmt
      {
        get => srcePrmt ??= new();
        set => srcePrmt = value;
      }

      /// <summary>
      /// A value of TypePrmt.
      /// </summary>
      [JsonPropertyName("typePrmt")]
      public WorkArea TypePrmt
      {
        get => typePrmt ??= new();
        set => typePrmt = value;
      }

      /// <summary>
      /// A value of CntyPrmt.
      /// </summary>
      [JsonPropertyName("cntyPrmt")]
      public WorkArea CntyPrmt
      {
        get => cntyPrmt ??= new();
        set => cntyPrmt = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CsePersonAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common common;
      private CsePersonAddress csePersonAddress;
      private WorkArea statePrmt;
      private WorkArea enddtCdPrmt;
      private WorkArea srcePrmt;
      private WorkArea typePrmt;
      private WorkArea cntyPrmt;
      private CsePersonAddress hidden;
    }

    /// <summary>A PagenumGroup group.</summary>
    [Serializable]
    public class PagenumGroup
    {
      /// <summary>
      /// A value of LastAddr.
      /// </summary>
      [JsonPropertyName("lastAddr")]
      public CsePersonAddress LastAddr
      {
        get => lastAddr ??= new();
        set => lastAddr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private CsePersonAddress lastAddr;
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
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of ForeignAddress.
    /// </summary>
    [JsonPropertyName("foreignAddress")]
    public LocalWorkAddr ForeignAddress
    {
      get => foreignAddress ??= new();
      set => foreignAddress = value;
    }

    /// <summary>
    /// A value of ForeignAddr.
    /// </summary>
    [JsonPropertyName("foreignAddr")]
    public WorkArea ForeignAddr
    {
      get => foreignAddr ??= new();
      set => foreignAddr = value;
    }

    /// <summary>
    /// A value of PromptCode.
    /// </summary>
    [JsonPropertyName("promptCode")]
    public Code PromptCode
    {
      get => promptCode ??= new();
      set => promptCode = value;
    }

    /// <summary>
    /// A value of PromptCodeValue.
    /// </summary>
    [JsonPropertyName("promptCodeValue")]
    public CodeValue PromptCodeValue
    {
      get => promptCodeValue ??= new();
      set => promptCodeValue = value;
    }

    /// <summary>
    /// A value of SaveSubscript.
    /// </summary>
    [JsonPropertyName("saveSubscript")]
    public Common SaveSubscript
    {
      get => saveSubscript ??= new();
      set => saveSubscript = value;
    }

    /// <summary>
    /// A value of LastAddr.
    /// </summary>
    [JsonPropertyName("lastAddr")]
    public CsePersonAddress LastAddr
    {
      get => lastAddr ??= new();
      set => lastAddr = value;
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
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Line1Sel.
    /// </summary>
    [JsonPropertyName("line1Sel")]
    public Common Line1Sel
    {
      get => line1Sel ??= new();
      set => line1Sel = value;
    }

    /// <summary>
    /// A value of Line1StatePrmt.
    /// </summary>
    [JsonPropertyName("line1StatePrmt")]
    public WorkArea Line1StatePrmt
    {
      get => line1StatePrmt ??= new();
      set => line1StatePrmt = value;
    }

    /// <summary>
    /// A value of Line1TypePrmt.
    /// </summary>
    [JsonPropertyName("line1TypePrmt")]
    public WorkArea Line1TypePrmt
    {
      get => line1TypePrmt ??= new();
      set => line1TypePrmt = value;
    }

    /// <summary>
    /// A value of Line1EnddtCdPrmt.
    /// </summary>
    [JsonPropertyName("line1EnddtCdPrmt")]
    public WorkArea Line1EnddtCdPrmt
    {
      get => line1EnddtCdPrmt ??= new();
      set => line1EnddtCdPrmt = value;
    }

    /// <summary>
    /// A value of Line1SrcePrmt.
    /// </summary>
    [JsonPropertyName("line1SrcePrmt")]
    public WorkArea Line1SrcePrmt
    {
      get => line1SrcePrmt ??= new();
      set => line1SrcePrmt = value;
    }

    /// <summary>
    /// A value of Line1CntyPrmt.
    /// </summary>
    [JsonPropertyName("line1CntyPrmt")]
    public WorkArea Line1CntyPrmt
    {
      get => line1CntyPrmt ??= new();
      set => line1CntyPrmt = value;
    }

    /// <summary>
    /// A value of Line1Addr.
    /// </summary>
    [JsonPropertyName("line1Addr")]
    public CsePersonAddress Line1Addr
    {
      get => line1Addr ??= new();
      set => line1Addr = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of ProtectFields.
    /// </summary>
    [JsonPropertyName("protectFields")]
    public Common ProtectFields
    {
      get => protectFields ??= new();
      set => protectFields = value;
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
    /// Gets a value of Pagenum.
    /// </summary>
    [JsonIgnore]
    public Array<PagenumGroup> Pagenum => pagenum ??= new(
      PagenumGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Pagenum for json serialization.
    /// </summary>
    [JsonPropertyName("pagenum")]
    [Computed]
    public IList<PagenumGroup> Pagenum_Json
    {
      get => pagenum;
      set => Pagenum.Assign(value);
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CodeValue Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of FipsMessage.
    /// </summary>
    [JsonPropertyName("fipsMessage")]
    public WorkArea FipsMessage
    {
      get => fipsMessage ??= new();
      set => fipsMessage = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Common Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of AeAddr.
    /// </summary>
    [JsonPropertyName("aeAddr")]
    public CsePersonAddress AeAddr
    {
      get => aeAddr ??= new();
      set => aeAddr = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common csePersonPrompt;
    private CsePerson hiddenCsePerson;
    private LocalWorkAddr foreignAddress;
    private WorkArea foreignAddr;
    private Code promptCode;
    private CodeValue promptCodeValue;
    private Common saveSubscript;
    private CsePersonAddress lastAddr;
    private WorkArea plus;
    private WorkArea minus;
    private Common line1Sel;
    private WorkArea line1StatePrmt;
    private WorkArea line1TypePrmt;
    private WorkArea line1EnddtCdPrmt;
    private WorkArea line1SrcePrmt;
    private WorkArea line1CntyPrmt;
    private CsePersonAddress line1Addr;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Standard standard;
    private Common protectFields;
    private Array<GroupGroup> group;
    private Array<PagenumGroup> pagenum;
    private NextTranInfo hiddenNextTranInfo;
    private CodeValue selected;
    private WorkArea fipsMessage;
    private Common fips;
    private CsePersonAddress aeAddr;
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
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of CsePersonAddress.
      /// </summary>
      [JsonPropertyName("csePersonAddress")]
      public CsePersonAddress CsePersonAddress
      {
        get => csePersonAddress ??= new();
        set => csePersonAddress = value;
      }

      /// <summary>
      /// A value of StatePrmt.
      /// </summary>
      [JsonPropertyName("statePrmt")]
      public WorkArea StatePrmt
      {
        get => statePrmt ??= new();
        set => statePrmt = value;
      }

      /// <summary>
      /// A value of EnddtCdPrmt.
      /// </summary>
      [JsonPropertyName("enddtCdPrmt")]
      public WorkArea EnddtCdPrmt
      {
        get => enddtCdPrmt ??= new();
        set => enddtCdPrmt = value;
      }

      /// <summary>
      /// A value of SrcePrmt.
      /// </summary>
      [JsonPropertyName("srcePrmt")]
      public WorkArea SrcePrmt
      {
        get => srcePrmt ??= new();
        set => srcePrmt = value;
      }

      /// <summary>
      /// A value of TypePrmt.
      /// </summary>
      [JsonPropertyName("typePrmt")]
      public WorkArea TypePrmt
      {
        get => typePrmt ??= new();
        set => typePrmt = value;
      }

      /// <summary>
      /// A value of CntyPrmt.
      /// </summary>
      [JsonPropertyName("cntyPrmt")]
      public WorkArea CntyPrmt
      {
        get => cntyPrmt ??= new();
        set => cntyPrmt = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CsePersonAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common sel;
      private CsePersonAddress csePersonAddress;
      private WorkArea statePrmt;
      private WorkArea enddtCdPrmt;
      private WorkArea srcePrmt;
      private WorkArea typePrmt;
      private WorkArea cntyPrmt;
      private CsePersonAddress hidden;
    }

    /// <summary>A PagenumGroup group.</summary>
    [Serializable]
    public class PagenumGroup
    {
      /// <summary>
      /// A value of Pagenum1.
      /// </summary>
      [JsonPropertyName("pagenum1")]
      public CsePersonAddress Pagenum1
      {
        get => pagenum1 ??= new();
        set => pagenum1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private CsePersonAddress pagenum1;
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
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public CsePersonsWorkSet Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
    }

    /// <summary>
    /// A value of ProtectFields.
    /// </summary>
    [JsonPropertyName("protectFields")]
    public Common ProtectFields
    {
      get => protectFields ??= new();
      set => protectFields = value;
    }

    /// <summary>
    /// A value of ForeignAddress.
    /// </summary>
    [JsonPropertyName("foreignAddress")]
    public LocalWorkAddr ForeignAddress
    {
      get => foreignAddress ??= new();
      set => foreignAddress = value;
    }

    /// <summary>
    /// A value of ForeignAddr.
    /// </summary>
    [JsonPropertyName("foreignAddr")]
    public WorkArea ForeignAddr
    {
      get => foreignAddr ??= new();
      set => foreignAddr = value;
    }

    /// <summary>
    /// A value of PromptCodeValue.
    /// </summary>
    [JsonPropertyName("promptCodeValue")]
    public CodeValue PromptCodeValue
    {
      get => promptCodeValue ??= new();
      set => promptCodeValue = value;
    }

    /// <summary>
    /// A value of PromptCode.
    /// </summary>
    [JsonPropertyName("promptCode")]
    public Code PromptCode
    {
      get => promptCode ??= new();
      set => promptCode = value;
    }

    /// <summary>
    /// A value of SaveSubscript.
    /// </summary>
    [JsonPropertyName("saveSubscript")]
    public Common SaveSubscript
    {
      get => saveSubscript ??= new();
      set => saveSubscript = value;
    }

    /// <summary>
    /// A value of LastAddr.
    /// </summary>
    [JsonPropertyName("lastAddr")]
    public CsePersonAddress LastAddr
    {
      get => lastAddr ??= new();
      set => lastAddr = value;
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
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Line1Sel.
    /// </summary>
    [JsonPropertyName("line1Sel")]
    public Common Line1Sel
    {
      get => line1Sel ??= new();
      set => line1Sel = value;
    }

    /// <summary>
    /// A value of Line1CntyPrmt.
    /// </summary>
    [JsonPropertyName("line1CntyPrmt")]
    public WorkArea Line1CntyPrmt
    {
      get => line1CntyPrmt ??= new();
      set => line1CntyPrmt = value;
    }

    /// <summary>
    /// A value of Line1StatePrmt.
    /// </summary>
    [JsonPropertyName("line1StatePrmt")]
    public WorkArea Line1StatePrmt
    {
      get => line1StatePrmt ??= new();
      set => line1StatePrmt = value;
    }

    /// <summary>
    /// A value of Line1EnddtCdPrmt.
    /// </summary>
    [JsonPropertyName("line1EnddtCdPrmt")]
    public WorkArea Line1EnddtCdPrmt
    {
      get => line1EnddtCdPrmt ??= new();
      set => line1EnddtCdPrmt = value;
    }

    /// <summary>
    /// A value of Line1SrcePrmt.
    /// </summary>
    [JsonPropertyName("line1SrcePrmt")]
    public WorkArea Line1SrcePrmt
    {
      get => line1SrcePrmt ??= new();
      set => line1SrcePrmt = value;
    }

    /// <summary>
    /// A value of Line1TypePrmt.
    /// </summary>
    [JsonPropertyName("line1TypePrmt")]
    public WorkArea Line1TypePrmt
    {
      get => line1TypePrmt ??= new();
      set => line1TypePrmt = value;
    }

    /// <summary>
    /// A value of Line1Addr.
    /// </summary>
    [JsonPropertyName("line1Addr")]
    public CsePersonAddress Line1Addr
    {
      get => line1Addr ??= new();
      set => line1Addr = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// Gets a value of Pagenum.
    /// </summary>
    [JsonIgnore]
    public Array<PagenumGroup> Pagenum => pagenum ??= new(
      PagenumGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Pagenum for json serialization.
    /// </summary>
    [JsonPropertyName("pagenum")]
    [Computed]
    public IList<PagenumGroup> Pagenum_Json
    {
      get => pagenum;
      set => Pagenum.Assign(value);
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
    /// A value of FipsMessage.
    /// </summary>
    [JsonPropertyName("fipsMessage")]
    public WorkArea FipsMessage
    {
      get => fipsMessage ??= new();
      set => fipsMessage = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Common Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of PassToFips.
    /// </summary>
    [JsonPropertyName("passToFips")]
    public Fips PassToFips
    {
      get => passToFips ??= new();
      set => passToFips = value;
    }

    /// <summary>
    /// A value of AeAddr.
    /// </summary>
    [JsonPropertyName("aeAddr")]
    public CsePersonAddress AeAddr
    {
      get => aeAddr ??= new();
      set => aeAddr = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet save;
    private CsePerson hiddenCsePerson;
    private Common csePersonPrompt;
    private Common protectFields;
    private LocalWorkAddr foreignAddress;
    private WorkArea foreignAddr;
    private CodeValue promptCodeValue;
    private Code promptCode;
    private Common saveSubscript;
    private CsePersonAddress lastAddr;
    private WorkArea plus;
    private WorkArea minus;
    private Common line1Sel;
    private WorkArea line1CntyPrmt;
    private WorkArea line1StatePrmt;
    private WorkArea line1EnddtCdPrmt;
    private WorkArea line1SrcePrmt;
    private WorkArea line1TypePrmt;
    private CsePersonAddress line1Addr;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Standard standard;
    private Array<GroupGroup> group;
    private Array<PagenumGroup> pagenum;
    private NextTranInfo hiddenNextTranInfo;
    private WorkArea fipsMessage;
    private Common fips;
    private Fips passToFips;
    private CsePersonAddress aeAddr;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A SaveUpdatesGroup group.</summary>
    [Serializable]
    public class SaveUpdatesGroup
    {
      /// <summary>
      /// A value of AddrId.
      /// </summary>
      [JsonPropertyName("addrId")]
      public CsePersonAddress AddrId
      {
        get => addrId ??= new();
        set => addrId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePersonAddress addrId;
    }

    /// <summary>
    /// A value of Command1.
    /// </summary>
    [JsonPropertyName("command1")]
    public Command Command1
    {
      get => command1 ??= new();
      set => command1 = value;
    }

    /// <summary>
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
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
    /// A value of AddressType.
    /// </summary>
    [JsonPropertyName("addressType")]
    public Common AddressType
    {
      get => addressType ??= new();
      set => addressType = value;
    }

    /// <summary>
    /// A value of ScreenId.
    /// </summary>
    [JsonPropertyName("screenId")]
    public Common ScreenId
    {
      get => screenId ??= new();
      set => screenId = value;
    }

    /// <summary>
    /// A value of NumericZip.
    /// </summary>
    [JsonPropertyName("numericZip")]
    public Common NumericZip
    {
      get => numericZip ??= new();
      set => numericZip = value;
    }

    /// <summary>
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public CsePersonAddress CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// Gets a value of SaveUpdates.
    /// </summary>
    [JsonIgnore]
    public Array<SaveUpdatesGroup> SaveUpdates => saveUpdates ??= new(
      SaveUpdatesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SaveUpdates for json serialization.
    /// </summary>
    [JsonPropertyName("saveUpdates")]
    [Computed]
    public IList<SaveUpdatesGroup> SaveUpdates_Json
    {
      get => saveUpdates;
      set => SaveUpdates.Assign(value);
    }

    /// <summary>
    /// A value of Asterisk.
    /// </summary>
    [JsonPropertyName("asterisk")]
    public CsePersonAddress Asterisk
    {
      get => asterisk ??= new();
      set => asterisk = value;
    }

    /// <summary>
    /// A value of BlankCommon.
    /// </summary>
    [JsonPropertyName("blankCommon")]
    public Common BlankCommon
    {
      get => blankCommon ??= new();
      set => blankCommon = value;
    }

    /// <summary>
    /// A value of BlankCsePersonAddress.
    /// </summary>
    [JsonPropertyName("blankCsePersonAddress")]
    public CsePersonAddress BlankCsePersonAddress
    {
      get => blankCsePersonAddress ??= new();
      set => blankCsePersonAddress = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of BuildAddressList.
    /// </summary>
    [JsonPropertyName("buildAddressList")]
    public Common BuildAddressList
    {
      get => buildAddressList ??= new();
      set => buildAddressList = value;
    }

    /// <summary>
    /// A value of Command2.
    /// </summary>
    [JsonPropertyName("command2")]
    public Common Command2
    {
      get => command2 ??= new();
      set => command2 = value;
    }

    /// <summary>
    /// A value of Prmpt.
    /// </summary>
    [JsonPropertyName("prmpt")]
    public Common Prmpt
    {
      get => prmpt ??= new();
      set => prmpt = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    /// <summary>
    /// A value of DetailText30.
    /// </summary>
    [JsonPropertyName("detailText30")]
    public TextWorkArea DetailText30
    {
      get => detailText30 ??= new();
      set => detailText30 = value;
    }

    /// <summary>
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of AparSelection.
    /// </summary>
    [JsonPropertyName("aparSelection")]
    public WorkArea AparSelection
    {
      get => aparSelection ??= new();
      set => aparSelection = value;
    }

    /// <summary>
    /// A value of DetailText1.
    /// </summary>
    [JsonPropertyName("detailText1")]
    public WorkArea DetailText1
    {
      get => detailText1 ??= new();
      set => detailText1 = value;
    }

    /// <summary>
    /// A value of CseFlag.
    /// </summary>
    [JsonPropertyName("cseFlag")]
    public Common CseFlag
    {
      get => cseFlag ??= new();
      set => cseFlag = value;
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
    /// A value of TodayPlusSixMonths.
    /// </summary>
    [JsonPropertyName("todayPlusSixMonths")]
    public DateWorkArea TodayPlusSixMonths
    {
      get => todayPlusSixMonths ??= new();
      set => todayPlusSixMonths = value;
    }

    /// <summary>
    /// A value of ProtectFields.
    /// </summary>
    [JsonPropertyName("protectFields")]
    public Common ProtectFields
    {
      get => protectFields ??= new();
      set => protectFields = value;
    }

    private Command command1;
    private Transaction transaction;
    private AbendData abendData;
    private Common addressType;
    private Common screenId;
    private Common numericZip;
    private CsePersonAddress checkZip;
    private DateWorkArea maximum;
    private DateWorkArea zero;
    private DateWorkArea current;
    private Array<SaveUpdatesGroup> saveUpdates;
    private CsePersonAddress asterisk;
    private Common blankCommon;
    private CsePersonAddress blankCsePersonAddress;
    private DateWorkArea dateWorkArea;
    private Common buildAddressList;
    private Common command2;
    private Common prmpt;
    private CsePerson csePerson;
    private Common returnCode;
    private Code code;
    private CodeValue codeValue;
    private Common error;
    private Common select;
    private TextWorkArea detailText30;
    private TextWorkArea detailText10;
    private DateWorkArea date;
    private WorkArea aparSelection;
    private WorkArea detailText1;
    private Common cseFlag;
    private TextWorkArea textWorkArea;
    private DateWorkArea todayPlusSixMonths;
    private Common protectFields;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of FaMo.
    /// </summary>
    [JsonPropertyName("faMo")]
    public CaseRole FaMo
    {
      get => faMo ??= new();
      set => faMo = value;
    }

    /// <summary>
    /// A value of ApAr.
    /// </summary>
    [JsonPropertyName("apAr")]
    public CaseRole ApAr
    {
      get => apAr ??= new();
      set => apAr = value;
    }

    private ServiceProviderProfile serviceProviderProfile;
    private Profile profile;
    private ServiceProvider serviceProvider;
    private Fips fips;
    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
    private FipsTribAddress fipsTribAddress;
    private CaseRole faMo;
    private CaseRole apAr;
  }
#endregion
}
