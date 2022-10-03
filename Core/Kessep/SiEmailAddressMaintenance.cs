// Program: SI_EMAIL_ADDRESS_MAINTENANCE, ID: 1902521086, model: 746.
// Short name: SWEEMALP
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
/// A program: SI_EMAIL_ADDRESS_MAINTENANCE.
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
public partial class SiEmailAddressMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EMAIL_ADDRESS_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEmailAddressMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEmailAddressMaintenance.
  /// </summary>
  public SiEmailAddressMaintenance(IContext context, Import import,
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
    // Date		Developer	Request #	Description
    // ------------------------------------------------------------------------------------------------------
    // 01/21/2016	D Dupree        CQ 48843        Developed
    // 03/08/2018      JHarden         CQ61455         Add a flow to EMAL from 
    // APDS and allow PF9 to flow back to APDS.
    // 12/23/2020      GVandy          CQ68785         Email address cannot be 
    // end dated if cse_person customer
    //                                                 
    // service indicator is "E"
    // ------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Alrt.Assign(import.Alrt);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Next.Number = import.Next.Number;
    export.HiddenNext.Number = import.HiddenNext.Number;
    export.ApCommon.SelectChar = import.ApCommon.SelectChar;
    export.ArCommon.SelectChar = import.ArCommon.SelectChar;
    export.ApActive.Flag = import.ApActive.Flag;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.Case1.Number = import.Case1.Number;
    MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet, export.ArCsePersonsWorkSet);
      
    MoveCsePersonsWorkSet(import.ArFromCaseRole, export.ArFromCaseRole);
    export.ApCsePersonsWorkSet.Assign(import.ApCsePersonsWorkSet);
    export.HiddenAp.Number = import.HiddenAp.Number;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.EmptyAddrSelect.SelectChar = import.EmptyAddrSelect.SelectChar;
      export.Empty.Assign(import.Empty);
      export.EmptySourcePrmt.Text1 = import.EmptySourcePrmt.Text1;
    }

    export.Minus.Text1 = import.Minus.Text1;
    export.Plus.Text1 = import.Plus.Text1;
    export.SaveSubscript.Subscript = import.SaveSubscript.Subscript;
    MoveCsePersonEmailAddress1(import.LastAddr, export.LastAddr);
    export.Save.Number = import.Save.Number;
    export.PromptCode.CodeName = import.PromptCode.CodeName;
    MoveCodeValue(import.PromptCodeValue, export.PromptCodeValue);
    MoveStandard(import.Standard, export.Standard);
    export.ApList.Text1 = import.ApList.Text1;
    export.ArList.Text1 = import.ArList.Text1;
    MoveOffice(import.Office, export.Office);
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    if (!import.Email.IsEmpty)
    {
      for(import.Email.Index = 0; import.Email.Index < import.Email.Count; ++
        import.Email.Index)
      {
        if (!import.Email.CheckSize())
        {
          break;
        }

        export.Email.Index = import.Email.Index;
        export.Email.CheckSize();

        export.Email.Update.Select.SelectChar =
          import.Email.Item.Select.SelectChar;
        export.Email.Update.SourcePrompt.Text1 = import.Email.Item.Source.Text1;
        export.Email.Update.CsePersonEmailAddress.Assign(
          import.Email.Item.CsePersonEmailAddress);
        export.Email.Update.Hidden1.Assign(import.Email.Item.Hidden1);
        export.Email.Update.LastUpdated.Date =
          import.Email.Item.LastUpdated.Date;
      }

      import.Email.CheckIndex();
    }

    if (!import.Hidden.IsEmpty)
    {
      for(import.Hidden.Index = 0; import.Hidden.Index < import.Hidden.Count; ++
        import.Hidden.Index)
      {
        if (!import.Hidden.CheckSize())
        {
          break;
        }

        export.Hidden.Index = import.Hidden.Index;
        export.Hidden.CheckSize();

        export.Hidden.Update.HsendDate.EffectiveDate =
          import.Hidden.Item.HsendDate.EffectiveDate;
      }

      import.Hidden.CheckIndex();
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

        export.Pagenum.Update.Pagenum1.Assign(import.Pagenum.Item.Pagenum1);
      }

      import.Pagenum.CheckIndex();
    }

    UseCabZeroFillNumber();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      return;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;

      if (AsChar(export.ApCommon.SelectChar) == 'S')
      {
        export.HiddenNextTranInfo.CsePersonNumber =
          export.ApCsePersonsWorkSet.Number;
        export.HiddenNextTranInfo.CsePersonNumberAp =
          export.ApCsePersonsWorkSet.Number;
      }
      else if (AsChar(export.ArCommon.SelectChar) == 'S')
      {
        export.HiddenNextTranInfo.CsePersonNumber =
          export.ApCsePersonsWorkSet.Number;
      }

      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.ApCsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        UseCabZeroFillNumber();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }

      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPQ"))
      {
        export.Alrt.Assign(export.HiddenNextTranInfo);
      }

      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        if (export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault() == 0)
        {
          goto Test1;
        }

        local.LastTran.SystemGeneratedIdentifier =
          export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();

        if (ReadCaseRole())
        {
          switch(TrimEnd(entities.CaseRole.Type1))
          {
            case "AP":
              export.ApCsePersonsWorkSet.Number =
                local.LastTran.CsePersonNumber ?? Spaces(10);

              break;
            case "AR":
              export.ArCsePersonsWorkSet.Number =
                local.LastTran.CsePersonNumber ?? Spaces(10);

              break;
            default:
              break;
          }
        }

        export.Next.Number = local.LastTran.CaseNumber ?? Spaces(10);
      }

Test1:

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
        {
          if (Equal(global.Command, "DISPLAY"))
          {
            local.FvCheck.Flag = "Y";
            ExitState = "ACO_NN0000_ALL_OK";

            goto Test2;
          }
          else
          {
            if (AsChar(export.ArCommon.SelectChar) == 'S')
            {
              local.FvTest.Number = export.ArCsePersonsWorkSet.Number;
            }
            else
            {
              local.FvTest.Number = export.ApCsePersonsWorkSet.Number;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            UseScSecurityValidAuthForFv1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          goto Test2;
        }

        export.ApCsePersonsWorkSet.Number = "";
        export.ApCsePersonsWorkSet.FormattedName = "";
        export.ArCsePersonsWorkSet.Number = "";
        export.ArCsePersonsWorkSet.FormattedName = "";
        export.ArCommon.SelectChar = "";
        export.ApCommon.SelectChar = "";
        export.Email.Count = 0;

        return;
      }
    }

Test2:

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (AsChar(export.ApCommon.SelectChar) == 'S' && AsChar
      (export.ArCommon.SelectChar) == 'S')
    {
      var field1 = GetField(export.ApCommon, "selectChar");

      field1.Error = true;

      var field2 = GetField(export.ArCommon, "selectChar");

      field2.Error = true;

      ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";

      return;
    }

    for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
      export.Email.Index)
    {
      if (!export.Email.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Email.Item.Select.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.Select.Count;

          break;
        case '*':
          export.Email.Update.Select.SelectChar = "";

          break;
        default:
          var field = GetField(export.Email.Item.Select, "selectChar");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      switch(AsChar(export.Email.Item.SourcePrompt.Text1))
      {
        case 'S':
          ++local.Prmpt.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Email.Item.SourcePrompt, "text1");

          field.Error = true;

          ++local.Error.Count;

          break;
      }
    }

    export.Email.CheckIndex();

    if (local.Error.Count > 0)
    {
      ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

      return;
    }

    if (local.Prmpt.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

      return;
    }

    if (local.Select.Count > 0 && (Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT")))
    {
      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

      return;
    }

    if (local.Select.Count > 1 && (Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE")))
    {
      for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
        export.Email.Index)
      {
        if (!export.Email.CheckSize())
        {
          break;
        }

        if (AsChar(export.Email.Item.Select.SelectChar) == 'S')
        {
          var field = GetField(export.Email.Item.Select, "selectChar");

          field.Error = true;
        }
      }

      export.Email.CheckIndex();
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "RETURN"))
    {
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPU") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPQ"))
      {
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPQ"))
        {
          export.HiddenNextTranInfo.Assign(export.Alrt);
        }

        global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
          ;

        return;
      }
      else
      {
        ExitState = "ACO_NE0000_RETURN";

        return;
      }
    }

    if (!Equal(export.HiddenNext.Number, export.Next.Number))
    {
      if (IsEmpty(export.Next.Number))
      {
        export.Next.Number = import.HiddenNext.Number;
      }
      else if (!Equal(global.Command, "DISPLAY"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }
      else if (Equal(export.ApCsePersonsWorkSet.Number, export.HiddenAp.Number))
      {
        export.ApCsePersonsWorkSet.Number = "";
        export.HiddenAp.Number = "";
      }
    }

    if (!Equal(export.HiddenAp.Number, export.ApCsePersonsWorkSet.Number))
    {
      if (IsEmpty(export.ApCsePersonsWorkSet.Number))
      {
        export.ApCsePersonsWorkSet.Number = export.HiddenAp.Number;
      }
      else if (!Equal(global.Command, "DISPLAY"))
      {
        var field = GetField(export.ApCsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        export.Case1.Number = export.Next.Number;
        UseSiReadCaseHeaderInformation();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("NO_APS_ON_A_CASE"))
          {
            if (AsChar(export.ArCommon.SelectChar) != 'S')
            {
              export.ApCommon.SelectChar = "";
              export.ArCommon.SelectChar = "S";
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            return;
          }
        }
        else if (!Equal(export.HiddenNext.Number, export.Next.Number))
        {
          // --------------------------------
          // The user wants a new case displayed
          // therefore reset for the AP to be
          // selected for display by default,
          // if the new case has an AP.
          // --------------------------------
          if (!IsEmpty(export.ApCsePersonsWorkSet.Number))
          {
            export.ApCommon.SelectChar = "S";
            export.ArCommon.SelectChar = "";
          }
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y' && IsEmpty
          (export.ApCsePersonsWorkSet.Number))
        {
          export.HiddenNextTranInfo.MiscText1 = "SELECTION NEEDED";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!IsEmpty(export.ArFromCaseRole.Number))
        {
          if (Equal(export.Next.Number, export.HiddenNext.Number))
          {
            UseSiReadArInformation();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            export.ArFromCaseRole.Number = "";
          }
        }

        export.ApList.Text1 = "";
        export.ArList.Text1 = "";
        export.HiddenNext.Number = export.Next.Number;
        export.HiddenAp.Number = export.ApCsePersonsWorkSet.Number;

        switch(AsChar(export.ArCommon.SelectChar))
        {
          case 'S':
            break;
          case ' ':
            break;
          default:
            var field = GetField(export.ArCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(export.ApCommon.SelectChar))
        {
          case 'S':
            break;
          case ' ':
            break;
          default:
            var field = GetField(export.ApCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
            export.Email.Index)
          {
            if (!export.Email.CheckSize())
            {
              break;
            }

            export.Email.Update.Hidden1.EndDate = local.Zero.Date;
          }

          export.Email.CheckIndex();

          return;
        }
        else
        {
          local.BuildAddressList.Flag = "Y";
          export.Minus.Text1 = "";

          for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
            export.Email.Index)
          {
            if (!export.Email.CheckSize())
            {
              break;
            }

            export.Email.Update.Hidden1.Assign(
              export.Email.Item.CsePersonEmailAddress);
            export.Email.Update.Select.SelectChar = "";
          }

          export.Email.CheckIndex();
        }

        break;
      case "NEXT":
        if (IsEmpty(import.Plus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.Standard.PageNumber;

        export.Pagenum.Index = export.Standard.PageNumber - 1;
        export.Pagenum.CheckSize();

        export.LastAddr.Assign(export.Pagenum.Item.Pagenum1);
        local.Command.Command = global.Command;
        export.Email.Count = 0;
        export.Email.Index = -1;

        foreach(var item in ReadCsePersonEmailAddress())
        {
          ++export.Email.Index;
          export.Email.CheckSize();

          export.Email.Update.CsePersonEmailAddress.Assign(
            entities.CsePersonEmailAddress);
          export.Email.Update.LastUpdated.Date =
            Date(export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp);

          if (export.Email.IsFull)
          {
            break;
          }
        }

        if (export.Email.IsFull)
        {
          export.Plus.Text1 = "+";

          ++export.Pagenum.Index;
          export.Pagenum.CheckSize();

          export.Email.Index = Export.EmailGroup.Capacity - 2;
          export.Email.CheckSize();

          export.Pagenum.Update.Pagenum1.LastUpdatedTimestamp =
            export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp;
          export.Pagenum.Update.Pagenum1.EndDate =
            export.Email.Item.CsePersonEmailAddress.EndDate;
        }
        else
        {
          export.Plus.Text1 = "";
        }

        export.Minus.Text1 = "-";

        for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
          export.Email.Index)
        {
          if (!export.Email.CheckSize())
          {
            break;
          }

          export.Hidden.Index = export.Email.Index;
          export.Hidden.CheckSize();

          export.Email.Update.Hidden1.Assign(
            export.Email.Item.CsePersonEmailAddress);
          export.Hidden.Update.HsendDate.EffectiveDate =
            export.Email.Item.CsePersonEmailAddress.EffectiveDate;

          if (Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
            local.Maximum.Date))
          {
            export.Email.Update.CsePersonEmailAddress.EndDate = local.Zero.Date;
          }
          else if (!Lt(local.Zero.Date,
            export.Email.Item.CsePersonEmailAddress.EndDate))
          {
            export.Email.Update.CsePersonEmailAddress.EndDate = local.Zero.Date;
          }
          else
          {
            var field1 =
              GetField(export.Email.Item.CsePersonEmailAddress, "emailAddress");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Email.Item.CsePersonEmailAddress, "emailSource");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Email.Item.CsePersonEmailAddress, "effectiveDate");
              

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Email.Item.SourcePrompt, "text1");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        export.Email.CheckIndex();

        break;
      case "PREV":
        if (IsEmpty(import.Minus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.Standard.PageNumber;

        export.Pagenum.Index = export.Standard.PageNumber - 1;
        export.Pagenum.CheckSize();

        export.LastAddr.Assign(export.Pagenum.Item.Pagenum1);
        local.Command.Command = global.Command;
        export.Email.Index = -1;
        export.Email.Count = 0;

        foreach(var item in ReadCsePersonEmailAddress())
        {
          ++export.Email.Index;
          export.Email.CheckSize();

          export.Email.Update.CsePersonEmailAddress.Assign(
            entities.CsePersonEmailAddress);
          export.Email.Update.LastUpdated.Date =
            Date(export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp);

          if (export.Email.IsFull)
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

        for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
          export.Email.Index)
        {
          if (!export.Email.CheckSize())
          {
            break;
          }

          export.Hidden.Index = export.Email.Index;
          export.Hidden.CheckSize();

          export.Hidden.Update.HsendDate.EffectiveDate =
            export.Email.Item.CsePersonEmailAddress.EffectiveDate;
          export.Email.Update.Hidden1.Assign(
            export.Email.Item.CsePersonEmailAddress);

          if (Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
            local.Maximum.Date))
          {
            export.Email.Update.CsePersonEmailAddress.EndDate = local.Zero.Date;
          }
          else if (!Lt(local.Zero.Date,
            export.Email.Item.CsePersonEmailAddress.EndDate))
          {
            export.Email.Update.CsePersonEmailAddress.EndDate = local.Zero.Date;
          }
          else
          {
            var field1 =
              GetField(export.Email.Item.CsePersonEmailAddress, "emailAddress");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Email.Item.CsePersonEmailAddress, "emailSource");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Email.Item.CsePersonEmailAddress, "effectiveDate");
              

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Email.Item.SourcePrompt, "text1");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        export.Email.CheckIndex();

        break;
      case "ADD":
        for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
          export.Email.Index)
        {
          if (!export.Email.CheckSize())
          {
            break;
          }

          export.Email.Update.Hidden1.EmailAddress =
            Spaces(CsePersonEmailAddress.EmailAddress_MaxLength);
          export.Email.Update.Hidden1.EffectiveDate = local.Zero.Date;
          export.Email.Update.Hidden1.EndDate = local.Zero.Date;
        }

        export.Email.CheckIndex();

        if (AsChar(export.ArCommon.SelectChar) == 'S' && CharAt
          (export.ArCsePersonsWorkSet.Number, 10) == 'O')
        {
          var field = GetField(export.ArCommon, "selectChar");

          field.Error = true;

          ExitState = "SI0000_DO_NOT_CHANGE_ORGZ_ADDR";

          return;
        }

        for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
          export.Email.Index)
        {
          if (!export.Email.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Email.Item.Select.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              var field = GetField(export.Email.Item.Select, "selectChar");

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

        export.Email.CheckIndex();

        if (AsChar(export.EmptyAddrSelect.SelectChar) != 'S')
        {
          var field = GetField(export.EmptyAddrSelect, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        if (IsEmpty(export.ApCommon.SelectChar) && IsEmpty
          (export.ArCommon.SelectChar))
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
        else if (!IsEmpty(export.ApCommon.SelectChar) && !
          IsEmpty(export.ArCommon.SelectChar))
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        // *********************************************
        // Field validations
        // *********************************************
        if (IsEmpty(export.Empty.EmailSource))
        {
        }
        else
        {
          local.Code.CodeName = "EMAIL SOURCE";
          local.CodeValue.Cdvalue = import.Empty.EmailSource ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field = GetField(export.Empty, "emailSource");

            field.Error = true;

            ExitState = "INVALID_SOURCE";

            return;
          }
        }

        if (IsEmpty(export.Empty.EmailAddress))
        {
          ExitState = "ADDRESS_INCOMPLETE";

          var field = GetField(export.Empty, "emailAddress");

          field.Error = true;

          return;
        }
        else
        {
          local.EmailVerify.Count = 0;

          do
          {
            if (local.CurrentPosition.Count >= 70)
            {
              break;
            }

            local.Postion.Text1 =
              Substring(export.Empty.EmailAddress, local.CurrentPosition.Count,
              1);

            if (AsChar(local.Postion.Text1) == '@')
            {
              if (local.CurrentPosition.Count <= 1)
              {
                var field = GetField(export.Empty, "emailAddress");

                field.Error = true;

                ExitState = "INVALID_EMAIL_ADDRESS";

                return;
              }

              local.EmailVerify.Count = local.CurrentPosition.Count + 5;

              if (IsEmpty(Substring(
                export.Empty.EmailAddress, local.EmailVerify.Count, 1)))
              {
                var field = GetField(export.Empty, "emailAddress");

                field.Error = true;

                ExitState = "INVALID_EMAIL_ADDRESS";

                return;
              }

              break;
            }

            ++local.CurrentPosition.Count;
          }
          while(!Equal(global.Command, "COMMAND"));

          if (local.EmailVerify.Count <= 0)
          {
            var field = GetField(export.Empty, "emailAddress");

            field.Error = true;

            ExitState = "INVALID_EMAIL_ADDRESS";

            return;
          }
        }

        if (!Lt(local.Zero.Date, export.Empty.EffectiveDate))
        {
          export.Empty.EffectiveDate = Now().Date;
        }

        if (!Equal(export.Empty.EffectiveDate, local.Current.Date))
        {
          var field = GetField(export.Empty, "effectiveDate");

          field.Error = true;

          ExitState = "EFFECTIVE_DT_MUST_BE_CURRENT_DT";

          return;
        }

        if (!Lt(local.Zero.Date, export.Empty.EndDate))
        {
          export.Empty.EndDate = local.Maximum.Date;
        }
        else if (Equal(export.Empty.EndDate, local.Maximum.Date))
        {
        }
        else
        {
          var field = GetField(export.Empty, "endDate");

          field.Error = true;

          ExitState = "END_DATE_NOT_CURR_DATE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.SendDateUpdated.Flag = "N";

        if (AsChar(export.ApCommon.SelectChar) == 'S')
        {
          local.CsePerson.Number = import.ApCsePersonsWorkSet.Number;
          export.ArCommon.SelectChar = "";
        }
        else
        {
          local.CsePerson.Number = import.ArCsePersonsWorkSet.Number;
        }

        export.HiddenCsePerson.Number = local.CsePerson.Number;
        UseSiCreateCsePersonEmailAddr();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCsePersonEmailAddress2(export.Empty, local.Asterisk);
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          export.EmptyAddrSelect.SelectChar = local.BlankCommon.SelectChar;
          export.Empty.Assign(local.BlankCsePersonEmailAddress);
          local.BuildAddressList.Flag = "Y";
        }

        break;
      case "UPDATE":
        if (AsChar(export.EmptyAddrSelect.SelectChar) == 'S')
        {
          var field = GetField(export.EmptyAddrSelect, "selectChar");

          field.Error = true;

          ExitState = "INVALID_UPDATE_THIS_ENTRY";

          return;
        }

        if (AsChar(export.ArCommon.SelectChar) == 'S' && CharAt
          (export.ArCsePersonsWorkSet.Number, 10) == 'O')
        {
          var field = GetField(export.ArCommon, "selectChar");

          field.Error = true;

          ExitState = "SI0000_DO_NOT_CHANGE_ORGZ_ADDR";

          return;
        }

        export.Email.Index = 0;

        for(var limit = export.Email.Count; export.Email.Index < limit; ++
          export.Email.Index)
        {
          if (!export.Email.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Email.Item.Select.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              local.SaveUpdates.Index = export.Email.Index;
              local.SaveUpdates.CheckSize();

              local.SaveUpdates.Update.AddrId.Assign(
                export.Email.Item.CsePersonEmailAddress);
              local.SaveUpdates.Update.AddrId.LastUpdatedTimestamp =
                export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp;
              ++local.Select.Count;

              if (IsEmpty(export.ApCommon.SelectChar) && IsEmpty
                (export.ArCommon.SelectChar))
              {
                var field1 = GetField(export.ApCommon, "selectChar");

                field1.Error = true;

                var field2 = GetField(export.ArCommon, "selectChar");

                field2.Error = true;

                ExitState = "ACO_NE0000_NO_SELECTION_MADE";

                return;
              }
              else if (!IsEmpty(export.ApCommon.SelectChar) && !
                IsEmpty(export.ArCommon.SelectChar))
              {
                var field1 = GetField(export.ApCommon, "selectChar");

                field1.Error = true;

                var field2 = GetField(export.ArCommon, "selectChar");

                field2.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                return;
              }

              export.Hidden.Index = export.Email.Index;
              export.Hidden.CheckSize();

              if (Equal(export.Email.Item.Hidden1.EndDate, local.Maximum.Date))
              {
                local.ActiveRecord.Flag = "Y";
              }
              else
              {
                local.ActiveRecord.Flag = "N";
              }

              // *********************************************
              // Field validations
              // *********************************************
              if (IsEmpty(export.Email.Item.CsePersonEmailAddress.EmailAddress))
              {
                var field =
                  GetField(export.Email.Item.CsePersonEmailAddress,
                  "emailAddress");

                field.Error = true;

                ExitState = "NO_EMAIL_ADDRESS";

                return;
              }
              else if (!Equal(export.Email.Item.CsePersonEmailAddress.
                EmailAddress, export.Email.Item.Hidden1.EmailAddress))
              {
                local.EmailVerify.Count = 0;

                do
                {
                  if (local.CurrentPosition.Count >= 70)
                  {
                    break;
                  }

                  local.Postion.Text1 =
                    Substring(export.Email.Item.CsePersonEmailAddress.
                      EmailAddress, local.CurrentPosition.Count, 1);

                  if (AsChar(local.Postion.Text1) == '@')
                  {
                    if (local.CurrentPosition.Count <= 1)
                    {
                      var field =
                        GetField(export.Email.Item.CsePersonEmailAddress,
                        "emailAddress");

                      field.Error = true;

                      ExitState = "INVALID_EMAIL_ADDRESS";

                      return;
                    }

                    local.EmailVerify.Count = local.CurrentPosition.Count + 5;

                    if (IsEmpty(Substring(
                      export.Email.Item.CsePersonEmailAddress.EmailAddress,
                      local.EmailVerify.Count, 1)))
                    {
                      var field =
                        GetField(export.Email.Item.CsePersonEmailAddress,
                        "emailAddress");

                      field.Error = true;

                      ExitState = "INVALID_EMAIL_ADDRESS";

                      return;
                    }

                    break;
                  }

                  ++local.CurrentPosition.Count;
                }
                while(!Equal(global.Command, "COMMAND"));

                if (local.EmailVerify.Count <= 0)
                {
                  var field =
                    GetField(export.Email.Item.CsePersonEmailAddress,
                    "emailAddress");

                  field.Error = true;

                  ExitState = "INVALID_EMAIL_ADDRESS";

                  return;
                }

                if (AsChar(local.ActiveRecord.Flag) == 'Y')
                {
                  export.Email.Update.CsePersonEmailAddress.EffectiveDate =
                    Now().Date;
                }
              }

              if (IsEmpty(export.Email.Item.CsePersonEmailAddress.EmailSource))
              {
              }
              else
              {
                local.Code.CodeName = "EMAIL SOURCE";
                local.CodeValue.Cdvalue =
                  export.Email.Item.CsePersonEmailAddress.EmailSource ?? Spaces
                  (10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnCode.Flag) == 'N')
                {
                  var field = GetField(export.Empty, "emailSource");

                  field.Error = true;

                  ExitState = "INVALID_SOURCE";

                  return;
                }

                if (!Equal(export.Email.Item.CsePersonEmailAddress.EmailSource,
                  export.Email.Item.Hidden1.EmailSource) && AsChar
                  (local.ActiveRecord.Flag) == 'Y')
                {
                  export.Email.Update.CsePersonEmailAddress.EffectiveDate =
                    Now().Date;
                }
              }

              if (!Lt(local.Zero.Date,
                export.Email.Item.CsePersonEmailAddress.EffectiveDate))
              {
                export.Email.Update.CsePersonEmailAddress.EffectiveDate =
                  Now().Date;
              }

              if (!Equal(export.Email.Item.CsePersonEmailAddress.EffectiveDate,
                export.Email.Item.Hidden1.EffectiveDate))
              {
                if (!Equal(export.Email.Item.CsePersonEmailAddress.
                  EffectiveDate, local.Current.Date))
                {
                  var field =
                    GetField(export.Email.Item.CsePersonEmailAddress,
                    "effectiveDate");

                  field.Error = true;

                  ExitState = "EFFECTIVE_DT_MUST_BE_CURRENT_DT";

                  return;
                }
              }

              if (Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
                local.Zero.Date))
              {
                export.Email.Update.CsePersonEmailAddress.EndDate =
                  local.Maximum.Date;
              }
              else if (!Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
                export.Email.Item.Hidden1.EndDate))
              {
                if (!Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
                  Now().Date))
                {
                  var field =
                    GetField(export.Email.Item.CsePersonEmailAddress, "endDate");
                    

                  field.Error = true;

                  ExitState = "END_DATE_NOT_CURR_DATE";

                  return;
                }

                // 12/23/2020  GVandy  CQ68785  Email address cannot be end 
                // dated if cse_person customer service indicator is "E"
                if (AsChar(export.ApCommon.SelectChar) == 'S')
                {
                  if (ReadCsePerson1())
                  {
                    var field =
                      GetField(export.Email.Item.CsePersonEmailAddress,
                      "endDate");

                    field.Error = true;

                    ExitState = "SI0000_CHANGE_CUSTOMER_SERVICE";

                    return;
                  }
                  else
                  {
                    // --Continue
                  }
                }
                else if (ReadCsePerson2())
                {
                  var field =
                    GetField(export.Email.Item.CsePersonEmailAddress, "endDate");
                    

                  field.Error = true;

                  ExitState = "SI0000_CHANGE_CUSTOMER_SERVICE";

                  return;
                }
                else
                {
                  // --Continue
                }
              }

              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
              }
              else
              {
                return;
              }

              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
                if (AsChar(export.ApCommon.SelectChar) == 'S')
                {
                  local.CsePerson.Number = import.ApCsePersonsWorkSet.Number;
                }
                else
                {
                  local.CsePerson.Number = import.ArCsePersonsWorkSet.Number;
                }

                export.HiddenCsePerson.Number = local.CsePerson.Number;
                export.Email.Update.CsePersonEmailAddress.LastUpdatedBy =
                  global.UserId;
                UseSiUpdateCsePersonEmailAddr();

                if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                  ("ACO_NI0000_SUCCESSFUL_UPDATE"))
                {
                  export.Email.Update.Select.SelectChar = "*";
                  export.Email.Update.LastUpdated.Date =
                    Date(export.Email.Item.CsePersonEmailAddress.
                      LastUpdatedTimestamp);
                  MoveCsePersonEmailAddress2(export.Email.Item.
                    CsePersonEmailAddress, local.AsteriskGroup);
                  ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
                }
              }
              else
              {
                return;
              }

              break;
            default:
              break;
          }
        }

        export.Email.CheckIndex();

        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "LIST":
        if (AsChar(import.ArList.Text1) == 'S' && AsChar
          (import.ApList.Text1) == 'S')
        {
          var field1 = GetField(export.ApList, "text1");

          field1.Error = true;

          var field2 = GetField(export.ArList, "text1");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(import.ApList.Text1) == 'S')
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }
        else if (AsChar(import.ArList.Text1) == 'S')
        {
          ExitState = "ECO_LNK_TO_CASE_ROLE_MAINTENANCE";

          return;
        }
        else
        {
          if (!IsEmpty(import.ApList.Text1))
          {
            var field = GetField(export.ApList, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }

          if (!IsEmpty(import.ArList.Text1))
          {
            var field = GetField(export.ArList, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }

          if (IsEmpty(import.ArList.Text1) && IsEmpty(import.ApList.Text1))
          {
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (AsChar(export.EmptyAddrSelect.SelectChar) == 'S')
        {
          if (AsChar(export.EmptySourcePrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "EMAIL SOURCE";
          }
          else
          {
            var field = GetField(export.EmptyAddrSelect, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            return;
          }

          ExitState = "ECO_LNK_TO_CODE_TABLES";

          return;
        }

        for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
          export.Email.Index)
        {
          if (!export.Email.CheckSize())
          {
            break;
          }

          if (AsChar(export.Email.Item.Select.SelectChar) == 'S')
          {
            if (AsChar(export.Email.Item.SourcePrompt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "EMAIL SOURCE";
            }
            else
            {
              var field = GetField(export.Email.Item.Select, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            ExitState = "ECO_LNK_TO_CODE_TABLES";

            return;
          }
        }

        export.Email.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "RTLIST":
        if (AsChar(export.EmptyAddrSelect.SelectChar) == 'S')
        {
          if (AsChar(export.EmptySourcePrmt.Text1) == 'S')
          {
            export.EmptySourcePrmt.Text1 = "";

            var field = GetField(export.EmptySourcePrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.Empty.EmailSource = import.PromptCodeValue.Cdvalue;

              return;
            }
          }
        }

        for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
          export.Email.Index)
        {
          if (!export.Email.CheckSize())
          {
            break;
          }

          if (AsChar(export.Email.Item.Select.SelectChar) == 'S')
          {
            if (AsChar(export.Email.Item.SourcePrompt.Text1) == 'S')
            {
              export.Email.Update.SourcePrompt.Text1 = "";

              var field = GetField(export.Email.Item.SourcePrompt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Email.Update.CsePersonEmailAddress.EmailSource =
                  import.PromptCodeValue.Cdvalue;

                return;
              }
            }
          }
          else
          {
          }
        }

        export.Email.CheckIndex();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // --------------------------------
    // Display Address List as required
    // by above functions.
    // --------------------------------
    if (AsChar(local.BuildAddressList.Flag) == 'Y')
    {
      if (AsChar(export.ApCommon.SelectChar) == 'S' && AsChar
        (export.ArCommon.SelectChar) == 'S')
      {
        var field1 = GetField(export.ApCommon, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.ArCommon, "selectChar");

        field2.Error = true;

        ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";

        return;
      }
      else if (AsChar(export.ArCommon.SelectChar) == 'S')
      {
        export.Save.Number = export.ArCsePersonsWorkSet.Number;
      }
      else
      {
        export.ApCommon.SelectChar = "S";
        export.Save.Number = export.ApCsePersonsWorkSet.Number;
      }

      export.Email.Count = 0;
      export.Hidden.Count = 0;
      export.Pagenum.Count = 0;

      if (AsChar(local.FvCheck.Flag) == 'Y')
      {
        UseScSecurityValidAuthForFv2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenNext.Number = export.Case1.Number;

          return;
        }
      }

      export.LastAddr.LastUpdatedTimestamp = Now().AddDays(1);
      export.LastAddr.EndDate = UseCabSetMaximumDiscontinueDate();
      local.Command.Command = global.Command;
      export.Email.Index = -1;

      foreach(var item in ReadCsePersonEmailAddress())
      {
        ++export.Email.Index;
        export.Email.CheckSize();

        export.Email.Update.CsePersonEmailAddress.Assign(
          entities.CsePersonEmailAddress);
        export.Email.Update.LastUpdated.Date =
          Date(export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp);

        if (export.Email.IsFull)
        {
          break;
        }
      }

      export.Standard.PageNumber = 1;

      // *********************************************
      // Set up group pagenum keys with the first
      // and the 4th occurrence of the address
      // identifiers
      // *********************************************
      if (export.Email.IsFull)
      {
        export.Plus.Text1 = "+";

        export.Pagenum.Index = 0;
        export.Pagenum.CheckSize();

        export.Email.Index = 0;
        export.Email.CheckSize();

        export.Pagenum.Update.Pagenum1.LastUpdatedTimestamp = Now().AddDays(1);
        export.Pagenum.Update.Pagenum1.EndDate =
          export.Email.Item.CsePersonEmailAddress.EndDate;

        ++export.Pagenum.Index;
        export.Pagenum.CheckSize();

        export.Email.Index = Export.EmailGroup.Capacity - 2;
        export.Email.CheckSize();

        export.Pagenum.Update.Pagenum1.LastUpdatedTimestamp =
          export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp;
        export.Pagenum.Update.Pagenum1.EndDate =
          export.Email.Item.CsePersonEmailAddress.EndDate;
      }
      else
      {
        export.Plus.Text1 = "";
      }

      export.Minus.Text1 = "";

      for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
        export.Email.Index)
      {
        if (!export.Email.CheckSize())
        {
          break;
        }

        export.Hidden.Index = export.Email.Index;
        export.Hidden.CheckSize();

        export.Hidden.Update.HsendDate.EffectiveDate =
          export.Email.Item.CsePersonEmailAddress.EffectiveDate;
        export.Email.Update.Hidden1.Assign(
          export.Email.Item.CsePersonEmailAddress);

        if (Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
          local.Maximum.Date))
        {
          export.Email.Update.CsePersonEmailAddress.EndDate = local.Zero.Date;
        }
        else if (!Lt(local.Zero.Date,
          export.Email.Item.CsePersonEmailAddress.EndDate))
        {
        }
        else
        {
          var field1 =
            GetField(export.Email.Item.CsePersonEmailAddress, "emailAddress");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Email.Item.CsePersonEmailAddress, "emailSource");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Email.Item.CsePersonEmailAddress, "effectiveDate");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Email.Item.SourcePrompt, "text1");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        if (Equal(export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp,
          local.Asterisk.CreatedTimestamp))
        {
          export.Email.Update.LastUpdated.Date =
            Date(export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp);
          export.Email.Update.Select.SelectChar = "*";
        }
      }

      export.Email.CheckIndex();
    }

    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      // --------------------------------------------
      // The below code may be redundant, however it
      //   is to ensure that either ap/ar values are
      //    selected and not both.
      // If you remove it, pls test for it under
      //   add/update conditions that only one
      //   ap/ar is always selected. I am not risking
      //   anything in this complex prad.
      // --------------------------------------------
      if (AsChar(export.ApCommon.SelectChar) == 'S' && AsChar
        (export.ArCommon.SelectChar) == 'S')
      {
        var field1 = GetField(export.ApCommon, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.ArCommon, "selectChar");

        field2.Error = true;

        ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";
        UseEabRollbackCics();

        return;
      }

      local.Infrastructure.UserId = "EMAL";
      local.Infrastructure.EventId = 10;
      local.Infrastructure.BusinessObjectCd = "CAU";
      local.Infrastructure.Function = "LOC";
      local.Infrastructure.SituationNumber = 0;
      local.DetailText1.Text1 = ",";

      if (AsChar(export.ApCommon.SelectChar) == 'S')
      {
        local.AparSelection.Text1 = "P";
        local.CsePerson.Number = export.ApCsePersonsWorkSet.Number;
      }

      if (AsChar(export.ArCommon.SelectChar) == 'S')
      {
        local.AparSelection.Text1 = "R";
        local.CsePerson.Number = export.ArCsePersonsWorkSet.Number;
      }

      if (Lt(local.BlankCsePersonEmailAddress.CreatedTimestamp,
        local.Asterisk.CreatedTimestamp))
      {
        switch(AsChar(local.AparSelection.Text1))
        {
          case 'P':
            local.Infrastructure.ReasonCode = "EMAILADDAP";
            local.DetalText.Text50 =
              "AP email addr record was added. Source code:";

            break;
          case 'R':
            local.Infrastructure.ReasonCode = "EMAILADDAR";
            local.DetalText.Text50 =
              "AR email addr record was added. Source code:";

            break;
          default:
            break;
        }

        // the program added a record
        local.RaiseEventFlag.Text1 = "Y";
        local.Infrastructure.Detail = TrimEnd(local.DetalText.Text50) + TrimEnd
          (local.Asterisk.EmailSource);
        local.Infrastructure.DenormTimestamp = local.Asterisk.CreatedTimestamp;
        export.HiddenCsePerson.Number = local.CsePerson.Number;
        UseSiAddrRaiseEvent();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
          ("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
        }
        else
        {
          UseEabRollbackCics();
          export.Empty.Assign(import.Empty);

          var field1 = GetField(export.Empty, "emailAddress");

          field1.Error = true;

          var field2 = GetField(export.Empty, "emailSource");

          field2.Error = true;

          var field3 = GetField(export.Empty, "effectiveDate");

          field3.Error = true;

          var field4 = GetField(export.Empty, "endDate");

          field4.Error = true;

          var field5 = GetField(export.EmptySourcePrmt, "text1");

          field5.Error = true;

          var field6 = GetField(export.EmptyAddrSelect, "selectChar");

          field6.Error = true;

          return;
        }

        if (AsChar(export.ApCommon.SelectChar) == 'S')
        {
          local.AparSelection.Text1 = "R";
        }

        if (AsChar(export.ArCommon.SelectChar) == 'S')
        {
          local.AparSelection.Text1 = "P";
        }

        local.Infrastructure.ReasonCode = "";
        local.DetalText.Text50 = "";

        switch(AsChar(local.AparSelection.Text1))
        {
          case 'P':
            local.Infrastructure.ReasonCode = "EMAILADDAP";
            local.DetalText.Text50 =
              "AP email addr record was added. Source code:";

            break;
          case 'R':
            local.Infrastructure.ReasonCode = "EMAILADDAR";
            local.DetalText.Text50 =
              "AR email addr record was added. Source code:";

            break;
          default:
            break;
        }

        // the program added a record
        local.RaiseEventFlag.Text1 = "Y";
        local.Infrastructure.Detail = TrimEnd(local.DetalText.Text50) + TrimEnd
          (local.Asterisk.EmailSource);
        local.Infrastructure.DenormTimestamp = local.Asterisk.CreatedTimestamp;
        export.HiddenCsePerson.Number = local.CsePerson.Number;
        UseSiAddrRaiseEvent();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
          ("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
        }
        else
        {
          UseEabRollbackCics();
          export.Empty.Assign(import.Empty);

          var field1 = GetField(export.Empty, "emailAddress");

          field1.Error = true;

          var field2 = GetField(export.Empty, "emailSource");

          field2.Error = true;

          var field3 = GetField(export.Empty, "effectiveDate");

          field3.Error = true;

          var field4 = GetField(export.Empty, "endDate");

          field4.Error = true;

          var field5 = GetField(export.EmptySourcePrmt, "text1");

          field5.Error = true;

          var field6 = GetField(export.EmptyAddrSelect, "selectChar");

          field6.Error = true;

          return;
        }
      }
      else
      {
        // the program updated a record
        export.Email.Index = 0;

        for(var limit = export.Email.Count; export.Email.Index < limit; ++
          export.Email.Index)
        {
          if (!export.Email.CheckSize())
          {
            break;
          }

          if (AsChar(export.Email.Item.Select.SelectChar) == '*')
          {
            local.Infrastructure.DenormTimestamp =
              export.Email.Item.CsePersonEmailAddress.CreatedTimestamp;
            local.RaiseEventFlag.Text1 = "N";
            local.Infrastructure.Detail =
              Spaces(Infrastructure.Detail_MaxLength);

            if (Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
              Now().Date) && !
              Equal(export.Email.Item.Hidden1.EndDate, Now().Date))
            {
              // Check to see if the email address has been inactivated.
              switch(AsChar(local.AparSelection.Text1))
              {
                case 'P':
                  local.Infrastructure.ReasonCode = "EMAILENDAP";
                  local.DetalText.Text50 =
                    "AP's email address was inactivated. Source code:";

                  break;
                case 'R':
                  local.Infrastructure.ReasonCode = "EMAILENDAR";
                  local.DetalText.Text50 =
                    "AR's email address was inactivated. Source code:";

                  break;
                default:
                  break;
              }

              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.Detail = TrimEnd(local.DetalText.Text50) + TrimEnd
                (export.Email.Item.CsePersonEmailAddress.EmailSource);
              export.HiddenCsePerson.Number = local.CsePerson.Number;
            }
            else if (Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
              local.Maximum.Date) && !
              Equal(export.Email.Item.Hidden1.EndDate, local.Maximum.Date))
            {
              switch(AsChar(local.AparSelection.Text1))
              {
                case 'P':
                  local.Infrastructure.ReasonCode = "EMAILACTAP";
                  local.DetalText.Text50 =
                    "AP's email address was activated. Source code:";

                  break;
                case 'R':
                  local.Infrastructure.ReasonCode = "EMAILACTAR";
                  local.DetalText.Text50 =
                    "AR's email address was activated. Source code:";

                  break;
                default:
                  break;
              }

              // Check to see if the email address has been activated.
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.Detail = TrimEnd(local.DetalText.Text50) + TrimEnd
                (export.Email.Item.CsePersonEmailAddress.EmailSource);
              export.HiddenCsePerson.Number = local.CsePerson.Number;
            }
            else if (Equal(export.Email.Item.CsePersonEmailAddress.
              EffectiveDate, Now().Date) && Equal
              (export.Email.Item.Hidden1.EndDate, local.Maximum.Date))
            {
              switch(AsChar(local.AparSelection.Text1))
              {
                case 'P':
                  local.Infrastructure.ReasonCode = "EMAILUPDATEDAP";
                  local.DetalText.Text50 =
                    "AP's email addr record was updated. Source code:";

                  break;
                case 'R':
                  local.Infrastructure.ReasonCode = "EMAILUPDATEDAR";
                  local.DetalText.Text50 =
                    "AR's email addr record was updated. Source code:";

                  break;
                default:
                  break;
              }

              // Check to see if the email address record has been updated.
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.Detail = TrimEnd(local.DetalText.Text50) + TrimEnd
                (export.Email.Item.CsePersonEmailAddress.EmailSource);
              export.HiddenCsePerson.Number = local.CsePerson.Number;
            }

            if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
            {
              UseSiAddrRaiseEvent();

              if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
                ("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
              }
              else
              {
                UseEabRollbackCics();

                var field1 =
                  GetField(export.Email.Item.CsePersonEmailAddress,
                  "emailAddress");

                field1.Error = true;

                var field2 =
                  GetField(export.Email.Item.CsePersonEmailAddress,
                  "emailSource");

                field2.Error = true;

                var field3 =
                  GetField(export.Email.Item.CsePersonEmailAddress,
                  "effectiveDate");

                field3.Error = true;

                var field4 =
                  GetField(export.Email.Item.CsePersonEmailAddress, "endDate");

                field4.Error = true;

                var field5 =
                  GetField(export.Email.Item.CsePersonEmailAddress,
                  "lastUpdatedBy");

                field5.Error = true;

                var field6 = GetField(export.Email.Item.Select, "selectChar");

                field6.Error = true;

                var field7 = GetField(export.Email.Item.SourcePrompt, "text1");

                field7.Error = true;

                var field8 = GetField(export.Email.Item.LastUpdated, "date");

                field8.Error = true;

                return;
              }

              // --------------------------------------------
              // Locate is person specific event. So the event
              // has to be raised for all Case Units that the
              // Located Person participates as an AP and an AR.
              // --------------------------------------------
              // Reset the field values for the next loop.
              if (AsChar(export.ApCommon.SelectChar) == 'S')
              {
                local.AparSelection.Text1 = "R";
              }

              if (AsChar(export.ArCommon.SelectChar) == 'S')
              {
                local.AparSelection.Text1 = "P";
              }

              local.Infrastructure.ReasonCode = "";
              local.DetalText.Text50 = "";

              if (Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
                Now().Date) && !
                Equal(export.Email.Item.Hidden1.EndDate, Now().Date))
              {
                // Check to see if the email address has been inactivated.
                switch(AsChar(local.AparSelection.Text1))
                {
                  case 'P':
                    local.Infrastructure.ReasonCode = "EMAILENDAP";
                    local.DetalText.Text50 =
                      "AP's email address was inactivated. Source code:";

                    break;
                  case 'R':
                    local.Infrastructure.ReasonCode = "EMAILENDAR";
                    local.DetalText.Text50 =
                      "AR's email address was inactivated. Source code:";

                    break;
                  default:
                    break;
                }

                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.Detail =
                  TrimEnd(local.DetalText.Text50) + TrimEnd
                  (export.Email.Item.CsePersonEmailAddress.EmailSource);
                export.HiddenCsePerson.Number = local.CsePerson.Number;
              }
              else if (Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
                local.Maximum.Date) && !
                Equal(export.Email.Item.Hidden1.EndDate, local.Maximum.Date))
              {
                switch(AsChar(local.AparSelection.Text1))
                {
                  case 'P':
                    local.Infrastructure.ReasonCode = "EMAILACTAP";
                    local.DetalText.Text50 =
                      "AP's email address was activated. Source code:";

                    break;
                  case 'R':
                    local.Infrastructure.ReasonCode = "EMAILACTAR";
                    local.DetalText.Text50 =
                      "AR's email address was activated. Source code:";

                    break;
                  default:
                    break;
                }

                // Check to see if the email address has been activated.
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.Detail =
                  TrimEnd(local.DetalText.Text50) + TrimEnd
                  (export.Email.Item.CsePersonEmailAddress.EmailSource);
                export.HiddenCsePerson.Number = local.CsePerson.Number;
              }
              else if (Equal(export.Email.Item.CsePersonEmailAddress.
                EffectiveDate, Now().Date) && Equal
                (export.Email.Item.Hidden1.EndDate, local.Maximum.Date))
              {
                switch(AsChar(local.AparSelection.Text1))
                {
                  case 'P':
                    local.Infrastructure.ReasonCode = "EMAILUPDATEDAP";
                    local.DetalText.Text50 =
                      "AP's email addr record was updated. Source code:";

                    break;
                  case 'R':
                    local.Infrastructure.ReasonCode = "EMAILUPDATEDAR";
                    local.DetalText.Text50 =
                      "AR's email addr record was updated. Source code:";

                    break;
                  default:
                    break;
                }

                // Check to see if the email address record has been updated.
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.Detail =
                  TrimEnd(local.DetalText.Text50) + TrimEnd
                  (export.Email.Item.CsePersonEmailAddress.EmailSource);
                export.HiddenCsePerson.Number = local.CsePerson.Number;
              }

              UseSiAddrRaiseEvent();

              if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
                ("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
              }
              else
              {
                UseEabRollbackCics();

                var field1 =
                  GetField(export.Email.Item.CsePersonEmailAddress,
                  "emailAddress");

                field1.Error = true;

                var field2 =
                  GetField(export.Email.Item.CsePersonEmailAddress,
                  "emailSource");

                field2.Error = true;

                var field3 =
                  GetField(export.Email.Item.CsePersonEmailAddress,
                  "effectiveDate");

                field3.Error = true;

                var field4 =
                  GetField(export.Email.Item.CsePersonEmailAddress, "endDate");

                field4.Error = true;

                var field5 =
                  GetField(export.Email.Item.CsePersonEmailAddress,
                  "lastUpdatedBy");

                field5.Error = true;

                var field6 = GetField(export.Email.Item.Select, "selectChar");

                field6.Error = true;

                var field7 = GetField(export.Email.Item.SourcePrompt, "text1");

                field7.Error = true;

                var field8 = GetField(export.Email.Item.LastUpdated, "date");

                field8.Error = true;

                return;
              }

              local.Infrastructure.ReasonCode = "";
              local.DetalText.Text50 = "";
            }
          }
          else
          {
          }
        }

        export.Email.CheckIndex();
      }

      if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        return;
      }

      for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
        export.Email.Index)
      {
        if (!export.Email.CheckSize())
        {
          break;
        }

        export.Hidden.Index = export.Email.Index;
        export.Hidden.CheckSize();

        export.Email.Update.Hidden1.CreatedBy =
          export.Email.Item.CsePersonEmailAddress.CreatedBy;
        export.Email.Update.Hidden1.CreatedTimestamp =
          export.Email.Item.CsePersonEmailAddress.CreatedTimestamp;
        export.Email.Update.Hidden1.EffectiveDate =
          export.Email.Item.CsePersonEmailAddress.EffectiveDate;
        export.Email.Update.Hidden1.EmailAddress =
          export.Email.Item.CsePersonEmailAddress.EmailAddress ?? "";
        export.Email.Update.Hidden1.EmailSource =
          export.Email.Item.CsePersonEmailAddress.EmailSource ?? "";
        export.Email.Update.Hidden1.LastUpdatedBy =
          export.Email.Item.CsePersonEmailAddress.LastUpdatedBy ?? "";
        export.Email.Update.Hidden1.LastUpdatedTimestamp =
          export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp;

        if (Lt(local.Zero.Date, export.Email.Item.CsePersonEmailAddress.EndDate))
          
        {
          export.Email.Update.Hidden1.EndDate =
            export.Email.Item.CsePersonEmailAddress.EndDate;
        }

        export.Hidden.Update.HsendDate.EffectiveDate =
          export.Email.Item.CsePersonEmailAddress.EffectiveDate;
      }

      export.Email.CheckIndex();
      MoveCsePersonEmailAddress2(local.AsteriskGroup, local.Asterisk);

      for(export.Email.Index = 0; export.Email.Index < export.Email.Count; ++
        export.Email.Index)
      {
        if (!export.Email.CheckSize())
        {
          break;
        }

        if (Equal(export.Email.Item.CsePersonEmailAddress.EndDate,
          local.Maximum.Date))
        {
          export.Email.Update.CsePersonEmailAddress.EndDate = local.Zero.Date;
        }
        else if (!Lt(local.Zero.Date,
          export.Email.Item.CsePersonEmailAddress.EndDate))
        {
        }
        else
        {
          var field1 =
            GetField(export.Email.Item.CsePersonEmailAddress, "emailAddress");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Email.Item.CsePersonEmailAddress, "emailSource");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Email.Item.CsePersonEmailAddress, "effectiveDate");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Email.Item.SourcePrompt, "text1");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        if (Equal(export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp,
          local.Asterisk.CreatedTimestamp))
        {
          export.Email.Update.LastUpdated.Date =
            Date(export.Email.Item.CsePersonEmailAddress.LastUpdatedTimestamp);
          export.Email.Update.Select.SelectChar = "*";
        }
      }

      export.Email.CheckIndex();
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePersonEmailAddress1(CsePersonEmailAddress source,
    CsePersonEmailAddress target)
  {
    target.EndDate = source.EndDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveCsePersonEmailAddress2(CsePersonEmailAddress source,
    CsePersonEmailAddress target)
  {
    target.EmailSource = source.EmailSource;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveCsePersonEmailAddress3(CsePersonEmailAddress source,
    CsePersonEmailAddress target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PageNumber = source.PageNumber;
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

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCabReadInfrastructure()
  {
    var useImport = new OeCabReadInfrastructure.Import();
    var useExport = new OeCabReadInfrastructure.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.LastTran.SystemGeneratedIdentifier;

    Call(OeCabReadInfrastructure.Execute, useImport, useExport);

    local.LastTran.Assign(useExport.Infrastructure);
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

    useImport.CsePersonsWorkSet.Number = import.ApCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv1()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.CsePersonsWorkSet.Number = local.FvTest.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv2()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.Save.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseSiAddrRaiseEvent()
  {
    var useImport = new SiAddrRaiseEvent.Import();
    var useExport = new SiAddrRaiseEvent.Export();

    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.AparSelection.Text1 = local.AparSelection.Text1;

    Call(SiAddrRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSiCreateCsePersonEmailAddr()
  {
    var useImport = new SiCreateCsePersonEmailAddr.Import();
    var useExport = new SiCreateCsePersonEmailAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.CsePersonEmailAddress.Assign(export.Empty);

    Call(SiCreateCsePersonEmailAddr.Execute, useImport, useExport);

    export.Empty.Assign(useExport.CsePersonEmailAddress);
  }

  private void UseSiReadArInformation()
  {
    var useImport = new SiReadArInformation.Import();
    var useExport = new SiReadArInformation.Export();

    useImport.Ar.Number = export.ArFromCaseRole.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadArInformation.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    export.ApActive.Flag = useExport.ApActive.Flag;
    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
    export.ApCsePersonsWorkSet.Assign(useExport.Ap);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private void UseSiUpdateCsePersonEmailAddr()
  {
    var useImport = new SiUpdateCsePersonEmailAddr.Import();
    var useExport = new SiUpdateCsePersonEmailAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.CsePersonEmailAddress.Assign(
      export.Email.Item.CsePersonEmailAddress);

    Call(SiUpdateCsePersonEmailAddr.Execute, useImport, useExport);

    MoveCsePersonEmailAddress3(useExport.CsePersonEmailAddress,
      export.Email.Update.CsePersonEmailAddress);
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", local.LastTran.CsePersonNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.CustomerServiceCode =
          db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.CustomerServiceCode =
          db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonEmailAddress()
  {
    entities.CsePersonEmailAddress.Populated = false;

    return ReadEach("ReadCsePersonEmailAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Save.Number);
        db.SetNullableDate(
          command, "endDate", export.LastAddr.EndDate.GetValueOrDefault());
        db.SetNullableDateTime(
          command, "lastUpdatedTmst",
          export.LastAddr.LastUpdatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonEmailAddress.CspNumber = db.GetString(reader, 0);
        entities.CsePersonEmailAddress.EffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.CsePersonEmailAddress.EndDate = db.GetNullableDate(reader, 2);
        entities.CsePersonEmailAddress.EmailSource =
          db.GetNullableString(reader, 3);
        entities.CsePersonEmailAddress.CreatedBy = db.GetString(reader, 4);
        entities.CsePersonEmailAddress.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CsePersonEmailAddress.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.CsePersonEmailAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CsePersonEmailAddress.EmailAddress =
          db.GetNullableString(reader, 8);
        entities.CsePersonEmailAddress.Populated = true;

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
    /// <summary>A EmailGroup group.</summary>
    [Serializable]
    public class EmailGroup
    {
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
      /// A value of CsePersonEmailAddress.
      /// </summary>
      [JsonPropertyName("csePersonEmailAddress")]
      public CsePersonEmailAddress CsePersonEmailAddress
      {
        get => csePersonEmailAddress ??= new();
        set => csePersonEmailAddress = value;
      }

      /// <summary>
      /// A value of Hidden1.
      /// </summary>
      [JsonPropertyName("hidden1")]
      public CsePersonEmailAddress Hidden1
      {
        get => hidden1 ??= new();
        set => hidden1 = value;
      }

      /// <summary>
      /// A value of Source.
      /// </summary>
      [JsonPropertyName("source")]
      public WorkArea Source
      {
        get => source ??= new();
        set => source = value;
      }

      /// <summary>
      /// A value of LastUpdated.
      /// </summary>
      [JsonPropertyName("lastUpdated")]
      public DateWorkArea LastUpdated
      {
        get => lastUpdated ??= new();
        set => lastUpdated = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common select;
      private CsePersonEmailAddress csePersonEmailAddress;
      private CsePersonEmailAddress hidden1;
      private WorkArea source;
      private DateWorkArea lastUpdated;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HsendDate.
      /// </summary>
      [JsonPropertyName("hsendDate")]
      public CsePersonEmailAddress HsendDate
      {
        get => hsendDate ??= new();
        set => hsendDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonEmailAddress hsendDate;
    }

    /// <summary>A PagenumGroup group.</summary>
    [Serializable]
    public class PagenumGroup
    {
      /// <summary>
      /// A value of Pagenum1.
      /// </summary>
      [JsonPropertyName("pagenum1")]
      public CsePersonEmailAddress Pagenum1
      {
        get => pagenum1 ??= new();
        set => pagenum1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonEmailAddress pagenum1;
    }

    /// <summary>
    /// Gets a value of Email.
    /// </summary>
    [JsonIgnore]
    public Array<EmailGroup> Email => email ??= new(EmailGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Email for json serialization.
    /// </summary>
    [JsonPropertyName("email")]
    [Computed]
    public IList<EmailGroup> Email_Json
    {
      get => email;
      set => Email.Assign(value);
    }

    /// <summary>
    /// A value of PrevCommandPerson.
    /// </summary>
    [JsonPropertyName("prevCommandPerson")]
    public WorkArea PrevCommandPerson
    {
      get => prevCommandPerson ??= new();
      set => prevCommandPerson = value;
    }

    /// <summary>
    /// A value of PrevCommandCase.
    /// </summary>
    [JsonPropertyName("prevCommandCase")]
    public WorkArea PrevCommandCase
    {
      get => prevCommandCase ??= new();
      set => prevCommandCase = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of ApActive.
    /// </summary>
    [JsonPropertyName("apActive")]
    public Common ApActive
    {
      get => apActive ??= new();
      set => apActive = value;
    }

    /// <summary>
    /// A value of ArFromCaseRole.
    /// </summary>
    [JsonPropertyName("arFromCaseRole")]
    public CsePersonsWorkSet ArFromCaseRole
    {
      get => arFromCaseRole ??= new();
      set => arFromCaseRole = value;
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
    /// A value of EmptySourcePrmt.
    /// </summary>
    [JsonPropertyName("emptySourcePrmt")]
    public WorkArea EmptySourcePrmt
    {
      get => emptySourcePrmt ??= new();
      set => emptySourcePrmt = value;
    }

    /// <summary>
    /// A value of ApList.
    /// </summary>
    [JsonPropertyName("apList")]
    public WorkArea ApList
    {
      get => apList ??= new();
      set => apList = value;
    }

    /// <summary>
    /// A value of ArList.
    /// </summary>
    [JsonPropertyName("arList")]
    public WorkArea ArList
    {
      get => arList ??= new();
      set => arList = value;
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
    public CsePersonEmailAddress LastAddr
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
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of ApCommon.
    /// </summary>
    [JsonPropertyName("apCommon")]
    public Common ApCommon
    {
      get => apCommon ??= new();
      set => apCommon = value;
    }

    /// <summary>
    /// A value of ArCommon.
    /// </summary>
    [JsonPropertyName("arCommon")]
    public Common ArCommon
    {
      get => arCommon ??= new();
      set => arCommon = value;
    }

    /// <summary>
    /// A value of EmptyAddrSelect.
    /// </summary>
    [JsonPropertyName("emptyAddrSelect")]
    public Common EmptyAddrSelect
    {
      get => emptyAddrSelect ??= new();
      set => emptyAddrSelect = value;
    }

    /// <summary>
    /// A value of Empty.
    /// </summary>
    [JsonPropertyName("empty")]
    public CsePersonEmailAddress Empty
    {
      get => empty ??= new();
      set => empty = value;
    }

    /// <summary>
    /// A value of HiddenNext.
    /// </summary>
    [JsonPropertyName("hiddenNext")]
    public Case1 HiddenNext
    {
      get => hiddenNext ??= new();
      set => hiddenNext = value;
    }

    /// <summary>
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
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
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity, 0);

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
    /// A value of Alrt.
    /// </summary>
    [JsonPropertyName("alrt")]
    public NextTranInfo Alrt
    {
      get => alrt ??= new();
      set => alrt = value;
    }

    private Array<EmailGroup> email;
    private WorkArea prevCommandPerson;
    private WorkArea prevCommandCase;
    private WorkArea headerLine;
    private CsePerson hiddenCsePerson;
    private Common caseOpen;
    private Common apActive;
    private CsePersonsWorkSet arFromCaseRole;
    private Code promptCode;
    private CodeValue promptCodeValue;
    private WorkArea emptySourcePrmt;
    private WorkArea apList;
    private WorkArea arList;
    private CsePersonsWorkSet save;
    private Common saveSubscript;
    private CsePersonEmailAddress lastAddr;
    private WorkArea plus;
    private WorkArea minus;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Case1 next;
    private Case1 case1;
    private Common apCommon;
    private Common arCommon;
    private Common emptyAddrSelect;
    private CsePersonEmailAddress empty;
    private Case1 hiddenNext;
    private CsePersonsWorkSet hiddenAp;
    private Standard standard;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<HiddenGroup> hidden;
    private Array<PagenumGroup> pagenum;
    private NextTranInfo hiddenNextTranInfo;
    private NextTranInfo alrt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A EmailGroup group.</summary>
    [Serializable]
    public class EmailGroup
    {
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
      /// A value of CsePersonEmailAddress.
      /// </summary>
      [JsonPropertyName("csePersonEmailAddress")]
      public CsePersonEmailAddress CsePersonEmailAddress
      {
        get => csePersonEmailAddress ??= new();
        set => csePersonEmailAddress = value;
      }

      /// <summary>
      /// A value of Hidden1.
      /// </summary>
      [JsonPropertyName("hidden1")]
      public CsePersonEmailAddress Hidden1
      {
        get => hidden1 ??= new();
        set => hidden1 = value;
      }

      /// <summary>
      /// A value of SourcePrompt.
      /// </summary>
      [JsonPropertyName("sourcePrompt")]
      public WorkArea SourcePrompt
      {
        get => sourcePrompt ??= new();
        set => sourcePrompt = value;
      }

      /// <summary>
      /// A value of LastUpdated.
      /// </summary>
      [JsonPropertyName("lastUpdated")]
      public DateWorkArea LastUpdated
      {
        get => lastUpdated ??= new();
        set => lastUpdated = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common select;
      private CsePersonEmailAddress csePersonEmailAddress;
      private CsePersonEmailAddress hidden1;
      private WorkArea sourcePrompt;
      private DateWorkArea lastUpdated;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HsendDate.
      /// </summary>
      [JsonPropertyName("hsendDate")]
      public CsePersonEmailAddress HsendDate
      {
        get => hsendDate ??= new();
        set => hsendDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonEmailAddress hsendDate;
    }

    /// <summary>A PagenumGroup group.</summary>
    [Serializable]
    public class PagenumGroup
    {
      /// <summary>
      /// A value of Pagenum1.
      /// </summary>
      [JsonPropertyName("pagenum1")]
      public CsePersonEmailAddress Pagenum1
      {
        get => pagenum1 ??= new();
        set => pagenum1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonEmailAddress pagenum1;
    }

    /// <summary>
    /// Gets a value of Email.
    /// </summary>
    [JsonIgnore]
    public Array<EmailGroup> Email => email ??= new(EmailGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Email for json serialization.
    /// </summary>
    [JsonPropertyName("email")]
    [Computed]
    public IList<EmailGroup> Email_Json
    {
      get => email;
      set => Email.Assign(value);
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of ApActive.
    /// </summary>
    [JsonPropertyName("apActive")]
    public Common ApActive
    {
      get => apActive ??= new();
      set => apActive = value;
    }

    /// <summary>
    /// A value of MtCntyPrmt.
    /// </summary>
    [JsonPropertyName("mtCntyPrmt")]
    public WorkArea MtCntyPrmt
    {
      get => mtCntyPrmt ??= new();
      set => mtCntyPrmt = value;
    }

    /// <summary>
    /// A value of ArFromCaseRole.
    /// </summary>
    [JsonPropertyName("arFromCaseRole")]
    public CsePersonsWorkSet ArFromCaseRole
    {
      get => arFromCaseRole ??= new();
      set => arFromCaseRole = value;
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
    /// A value of EmptySourcePrmt.
    /// </summary>
    [JsonPropertyName("emptySourcePrmt")]
    public WorkArea EmptySourcePrmt
    {
      get => emptySourcePrmt ??= new();
      set => emptySourcePrmt = value;
    }

    /// <summary>
    /// A value of ApList.
    /// </summary>
    [JsonPropertyName("apList")]
    public WorkArea ApList
    {
      get => apList ??= new();
      set => apList = value;
    }

    /// <summary>
    /// A value of ArList.
    /// </summary>
    [JsonPropertyName("arList")]
    public WorkArea ArList
    {
      get => arList ??= new();
      set => arList = value;
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
    public CsePersonEmailAddress LastAddr
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
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of ApCommon.
    /// </summary>
    [JsonPropertyName("apCommon")]
    public Common ApCommon
    {
      get => apCommon ??= new();
      set => apCommon = value;
    }

    /// <summary>
    /// A value of ArCommon.
    /// </summary>
    [JsonPropertyName("arCommon")]
    public Common ArCommon
    {
      get => arCommon ??= new();
      set => arCommon = value;
    }

    /// <summary>
    /// A value of EmptyAddrSelect.
    /// </summary>
    [JsonPropertyName("emptyAddrSelect")]
    public Common EmptyAddrSelect
    {
      get => emptyAddrSelect ??= new();
      set => emptyAddrSelect = value;
    }

    /// <summary>
    /// A value of Empty.
    /// </summary>
    [JsonPropertyName("empty")]
    public CsePersonEmailAddress Empty
    {
      get => empty ??= new();
      set => empty = value;
    }

    /// <summary>
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
    }

    /// <summary>
    /// A value of HiddenNext.
    /// </summary>
    [JsonPropertyName("hiddenNext")]
    public Case1 HiddenNext
    {
      get => hiddenNext ??= new();
      set => hiddenNext = value;
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
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity, 0);

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
    /// A value of Alrt.
    /// </summary>
    [JsonPropertyName("alrt")]
    public NextTranInfo Alrt
    {
      get => alrt ??= new();
      set => alrt = value;
    }

    private Array<EmailGroup> email;
    private WorkArea headerLine;
    private CsePerson hiddenCsePerson;
    private Common caseOpen;
    private Common apActive;
    private WorkArea mtCntyPrmt;
    private CsePersonsWorkSet arFromCaseRole;
    private CodeValue promptCodeValue;
    private Code promptCode;
    private WorkArea emptySourcePrmt;
    private WorkArea apList;
    private WorkArea arList;
    private CsePersonsWorkSet save;
    private Common saveSubscript;
    private CsePersonEmailAddress lastAddr;
    private WorkArea plus;
    private WorkArea minus;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Case1 next;
    private Case1 case1;
    private Common apCommon;
    private Common arCommon;
    private Common emptyAddrSelect;
    private CsePersonEmailAddress empty;
    private CsePersonsWorkSet hiddenAp;
    private Case1 hiddenNext;
    private Standard standard;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<HiddenGroup> hidden;
    private Array<PagenumGroup> pagenum;
    private NextTranInfo hiddenNextTranInfo;
    private NextTranInfo alrt;
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
      public CsePersonEmailAddress AddrId
      {
        get => addrId ??= new();
        set => addrId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePersonEmailAddress addrId;
    }

    /// <summary>
    /// A value of AsteriskGroup.
    /// </summary>
    [JsonPropertyName("asteriskGroup")]
    public CsePersonEmailAddress AsteriskGroup
    {
      get => asteriskGroup ??= new();
      set => asteriskGroup = value;
    }

    /// <summary>
    /// A value of DetalText.
    /// </summary>
    [JsonPropertyName("detalText")]
    public WorkArea DetalText
    {
      get => detalText ??= new();
      set => detalText = value;
    }

    /// <summary>
    /// A value of ActiveRecord.
    /// </summary>
    [JsonPropertyName("activeRecord")]
    public Common ActiveRecord
    {
      get => activeRecord ??= new();
      set => activeRecord = value;
    }

    /// <summary>
    /// A value of EmailVerify.
    /// </summary>
    [JsonPropertyName("emailVerify")]
    public Common EmailVerify
    {
      get => emailVerify ??= new();
      set => emailVerify = value;
    }

    /// <summary>
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
    }

    /// <summary>
    /// A value of Stored.
    /// </summary>
    [JsonPropertyName("stored")]
    public NextTranInfo Stored
    {
      get => stored ??= new();
      set => stored = value;
    }

    /// <summary>
    /// A value of PrevCommandPerson.
    /// </summary>
    [JsonPropertyName("prevCommandPerson")]
    public WorkArea PrevCommandPerson
    {
      get => prevCommandPerson ??= new();
      set => prevCommandPerson = value;
    }

    /// <summary>
    /// A value of PrevCommandCase.
    /// </summary>
    [JsonPropertyName("prevCommandCase")]
    public WorkArea PrevCommandCase
    {
      get => prevCommandCase ??= new();
      set => prevCommandCase = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public NextTranInfo Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public Common Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Asterisk.
    /// </summary>
    [JsonPropertyName("asterisk")]
    public CsePersonEmailAddress Asterisk
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
    /// A value of BlankCsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("blankCsePersonEmailAddress")]
    public CsePersonEmailAddress BlankCsePersonEmailAddress
    {
      get => blankCsePersonEmailAddress ??= new();
      set => blankCsePersonEmailAddress = value;
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
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Common Command
    {
      get => command ??= new();
      set => command = value;
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
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
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
    /// A value of SendDateUpdated.
    /// </summary>
    [JsonPropertyName("sendDateUpdated")]
    public Common SendDateUpdated
    {
      get => sendDateUpdated ??= new();
      set => sendDateUpdated = value;
    }

    /// <summary>
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
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
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    /// <summary>
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
    }

    /// <summary>
    /// A value of Miscellaneous.
    /// </summary>
    [JsonPropertyName("miscellaneous")]
    public DateWorkArea Miscellaneous
    {
      get => miscellaneous ??= new();
      set => miscellaneous = value;
    }

    /// <summary>
    /// A value of FvCheck.
    /// </summary>
    [JsonPropertyName("fvCheck")]
    public Common FvCheck
    {
      get => fvCheck ??= new();
      set => fvCheck = value;
    }

    /// <summary>
    /// A value of FvTest.
    /// </summary>
    [JsonPropertyName("fvTest")]
    public CsePersonsWorkSet FvTest
    {
      get => fvTest ??= new();
      set => fvTest = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private CsePersonEmailAddress asteriskGroup;
    private WorkArea detalText;
    private Common activeRecord;
    private Common emailVerify;
    private CsePersonEmailAddress csePersonEmailAddress;
    private NextTranInfo stored;
    private WorkArea prevCommandPerson;
    private WorkArea prevCommandCase;
    private NextTranInfo null1;
    private Common position;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common length;
    private Common save;
    private Common screenIdentification;
    private DateWorkArea maximum;
    private DateWorkArea zero;
    private DateWorkArea current;
    private Array<SaveUpdatesGroup> saveUpdates;
    private Infrastructure infrastructure;
    private CsePersonEmailAddress asterisk;
    private Common blankCommon;
    private CsePersonEmailAddress blankCsePersonEmailAddress;
    private DateWorkArea dateWorkArea;
    private Common buildAddressList;
    private Common command;
    private Common prmpt;
    private CsePerson csePerson;
    private Common returnCode;
    private Code code;
    private CodeValue codeValue;
    private Common multipleAps;
    private Common error;
    private Common select;
    private Common sendDateUpdated;
    private WorkArea raiseEventFlag;
    private DateWorkArea date;
    private WorkArea aparSelection;
    private WorkArea detailText1;
    private InterfaceAlert interfaceAlert;
    private Infrastructure lastTran;
    private DateWorkArea miscellaneous;
    private Common fvCheck;
    private CsePersonsWorkSet fvTest;
    private TextWorkArea postion;
    private Common currentPosition;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private CsePersonEmailAddress csePersonEmailAddress;
    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
