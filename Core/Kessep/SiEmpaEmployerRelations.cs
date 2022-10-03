// Program: SI_EMPA_EMPLOYER_RELATIONS, ID: 1902591355, model: 746.
// Short name: SWEEMPAP
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
/// A program: SI_EMPA_EMPLOYER_RELATIONS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure adds, updates and deletes details about a CSE Persons' 
/// employer.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiEmpaEmployerRelations: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EMPA_EMPLOYER_RELATIONS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEmpaEmployerRelations(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEmpaEmployerRelations.
  /// </summary>
  public SiEmpaEmployerRelations(IContext context, Import import, Export export):
    
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
    //     M A I N T E N A N C E    L O G
    // Date	   Developer		Description
    // 03-13-2017 D. Dupree		Initial Development
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Next.Number = import.Next.Number;
    MoveStandard(import.Standard, export.Standard);
    export.Minus.OneChar = import.Minus.OneChar;
    export.Plus.OneChar = import.Plus.OneChar;
    MoveCommon(import.ConfirmCreateRelationsh, export.ConfirmCreateRelationsh);
    export.ScreenSelect.Text1 = import.ScreenSelect.Text1;
    MoveEmployer(import.ServiceProviderEmployer, export.ServiceProviderEmployer);
      
    export.ServiceProviderEmployerAddress.Assign(
      import.ServiceProviderEmployerAddress);
    export.EmployerPrompt.Flag = import.EmployerPrompt.Flag;
    export.WsEmployer.Assign(import.WsEmployer);
    export.WsEmployerAddress.Assign(import.WsEmployerAddress);
    export.WsEmployerRelation.Type1 = import.WsEmployerRelation.Type1;
    export.TypePrompt.Flag = import.TypePrompt.Flag;
    MoveEmployerRelation1(import.EmployerRelation, export.EmployerRelation);
    export.Code.CodeName = import.Code.CodeName;
    export.CodeValue.Cdvalue = import.CodeValue.Cdvalue;

    if (AsChar(import.ScreenSelect.Text1) == 'X')
    {
      export.ScreenTitle.Text40 = "CSE - Employer Reverse Relations - EMPX";
    }
    else
    {
      export.ScreenTitle.Text40 = "     CSE - Employer Relations - EMPA";
    }

    if (!import.Empa.IsEmpty)
    {
      for(import.Empa.Index = 0; import.Empa.Index < import.Empa.Count; ++
        import.Empa.Index)
      {
        if (!import.Empa.CheckSize())
        {
          break;
        }

        export.Empa.Index = import.Empa.Index;
        export.Empa.CheckSize();

        export.Empa.Update.SelectEmpa.SelectChar =
          import.Empa.Item.SelectEmpa.SelectChar;
        export.Empa.Update.EmpaEmployer.Assign(import.Empa.Item.EmpaEmployer);
        export.Empa.Update.EmpaEmployerAddress.Assign(
          import.Empa.Item.EmpaEmployerAddress);
        export.Empa.Update.EmpaEmployerRelation.Assign(
          import.Empa.Item.EmpaEmployerRelation);
        export.Empa.Update.EmpEff.Date = import.Empa.Item.EmpaEff.Date;
        export.Empa.Update.EmpaEnd.Date = import.Empa.Item.EmpaEnd.Date;
        export.Empa.Update.PrmtEmplEmpa.Flag =
          import.Empa.Item.PrmtEmplEmpa.Flag;
        export.Empa.Update.PrmtTypeEmpa.Flag =
          import.Empa.Item.PrmtTypeEmpa.Flag;
        export.Empa.Update.OrginalEmployerRelation.Type1 =
          import.Empa.Item.OrginalEmployerRelation.Type1;
        export.Empa.Update.OrginalEmployer.Identifier =
          import.Empa.Item.OrginalEmployer.Identifier;

        switch(AsChar(import.Empa.Item.SelectEmpa.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Select.Count;

            break;
          case '*':
            export.Empa.Update.SelectEmpa.SelectChar = "";

            break;
          default:
            var field = GetField(export.Empa.Item.SelectEmpa, "selectChar");

            field.Error = true;

            ++local.Select.Count;

            break;
        }
      }

      import.Empa.CheckIndex();
    }

    local.CurrentDate.Date = Now().Date;
    UseCabSetMaximumDiscontinueDate1();

    if (!import.Paging.IsEmpty)
    {
      for(import.Paging.Index = 0; import.Paging.Index < import.Paging.Count; ++
        import.Paging.Index)
      {
        if (!import.Paging.CheckSize())
        {
          break;
        }

        export.Paging.Index = import.Paging.Index;
        export.Paging.CheckSize();

        MoveEmployerRelation3(import.Paging.Item.Scroll,
          export.Paging.Update.Scroll);
      }

      import.Paging.CheckIndex();
    }

    if (export.ConfirmCreateRelationsh.Count >= 1 && !
      Equal(global.Command, "ADD"))
    {
      export.ConfirmCreateRelationsh.Count = 0;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // Use the CAB to nexttran to another procedure.
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      export.Hidden.CaseNumber = export.Next.Number;
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
      export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RTLIST"))
    {
      if (AsChar(export.EmployerPrompt.Flag) == 'S')
      {
        MoveEmployer(import.RtnEmployer, export.ServiceProviderEmployer);
        export.ServiceProviderEmployerAddress.Assign(import.RtnEmployerAddress);
        export.EmployerPrompt.Flag = "";
        ExitState = "ADD_NECESSARY_TO_ESTAB_RELATIONS";

        return;
      }
      else
      {
      }

      if (AsChar(export.TypePrompt.Flag) == 'S')
      {
        export.WsEmployerRelation.Type1 = import.CodeValue.Cdvalue;
        export.TypePrompt.Flag = "";
        global.Command = "DISPLAY";
      }
      else
      {
      }

      for(export.Empa.Index = 0; export.Empa.Index < export.Empa.Count; ++
        export.Empa.Index)
      {
        if (!export.Empa.CheckSize())
        {
          break;
        }

        if (AsChar(export.Empa.Item.PrmtEmplEmpa.Flag) == 'S')
        {
          export.Empa.Update.EmpaEmployer.Assign(import.RtnEmployer);
          export.Empa.Update.EmpaEmployerAddress.Assign(
            import.RtnEmployerAddress);
          export.Empa.Update.PrmtEmplEmpa.Flag = "";

          return;
        }
        else
        {
        }
      }

      export.Empa.CheckIndex();

      for(export.Empa.Index = 0; export.Empa.Index < export.Empa.Count; ++
        export.Empa.Index)
      {
        if (!export.Empa.CheckSize())
        {
          break;
        }

        if (AsChar(export.Empa.Item.PrmtTypeEmpa.Flag) == 'S')
        {
          export.Empa.Update.EmpaEmployerRelation.Type1 =
            export.CodeValue.Cdvalue;
          export.Empa.Update.PrmtTypeEmpa.Flag = "";

          return;
        }
        else
        {
        }
      }

      export.Empa.CheckIndex();
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (!Equal(global.Command, "EMPL"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.UpdateOk.Flag = "Y";
    }

    UseCabSetMaximumDiscontinueDate2();

    if (!Equal(global.Command, "EMPL"))
    {
      if (export.WsEmployer.Identifier == 0)
      {
        var field1 = GetField(export.TypePrompt, "flag");

        field1.Protected = false;
        field1.Focused = true;

        var field2 = GetField(export.TypePrompt, "flag");

        field2.Error = true;

        var field3 = GetField(export.WsEmployer, "name");

        field3.Error = true;

        var field4 = GetField(export.WsEmployer, "ein");

        field4.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "MUST_SELECT_EMPLOYER_FROM_EMPL";

          return;
        }
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        ExitState = "ECO_XFR_TO_EMPLOYER_MAINTENANCE";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        // --------------------------------------------
        // Allows the user to flow back to the previous
        // screen.
        // --------------------------------------------
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        switch(AsChar(export.TypePrompt.Flag))
        {
          case 'S':
            ++local.Common.Count;
            export.Code.CodeName = "EMPLOYER RELATIONSHIP TYPE";
            ExitState = "ECO_LNK_TO_CODE_TABLES";

            return;
          case ' ':
            break;
          default:
            ++local.Common.Count;

            var field = GetField(export.TypePrompt, "flag");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // for the cases where you link from 1 procedure to another procedure, 
        // you must set the export_hidden security link_indicator to "L".
        // this will tell the called procedure that we are on a link and not a 
        // transfer.  Don't forget to do the view matching on the dialog design
        // screen.
        // ****
        // ****
        local.Common.Count = 0;

        for(export.Empa.Index = 0; export.Empa.Index < export.Empa.Count; ++
          export.Empa.Index)
        {
          if (!export.Empa.CheckSize())
          {
            break;
          }

          if (AsChar(export.Empa.Item.PrmtEmplEmpa.Flag) == 'S')
          {
            ++local.Common.Count;
          }
          else
          {
          }
        }

        export.Empa.CheckIndex();

        switch(local.Common.Count)
        {
          case 0:
            break;
          case 1:
            export.ServiceProviderEmployer.Identifier = 0;
            ExitState = "ECO_LNK_TO_EMPLOYER";

            return;
          default:
            for(export.Empa.Index = 0; export.Empa.Index < export.Empa.Count; ++
              export.Empa.Index)
            {
              if (!export.Empa.CheckSize())
              {
                break;
              }

              if (AsChar(export.Empa.Item.PrmtEmplEmpa.Flag) == 'S')
              {
                var field = GetField(export.Empa.Item.PrmtEmplEmpa, "flag");

                field.Error = true;
              }
              else
              {
              }
            }

            export.Empa.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.Common.Count = 0;

        for(export.Empa.Index = 0; export.Empa.Index < export.Empa.Count; ++
          export.Empa.Index)
        {
          if (!export.Empa.CheckSize())
          {
            break;
          }

          if (AsChar(export.Empa.Item.PrmtTypeEmpa.Flag) == 'S')
          {
            ++local.Common.Count;
          }
          else
          {
          }
        }

        export.Empa.CheckIndex();

        switch(local.Common.Count)
        {
          case 0:
            break;
          case 1:
            export.Code.CodeName = "EMPLOYER RELATIONSHIP TYPE";
            ExitState = "ECO_LNK_TO_CODE_TABLES";

            return;
          default:
            for(export.Empa.Index = 0; export.Empa.Index < export.Empa.Count; ++
              export.Empa.Index)
            {
              if (!export.Empa.CheckSize())
              {
                break;
              }

              if (AsChar(export.Empa.Item.PrmtTypeEmpa.Flag) == 'S')
              {
                var field = GetField(export.Empa.Item.PrmtTypeEmpa, "flag");

                field.Error = true;
              }
              else
              {
              }
            }

            export.Empa.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }

        break;
      case "ADD":
        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Empa.Index = 0;

        for(var limit = export.Empa.Count; export.Empa.Index < limit; ++
          export.Empa.Index)
        {
          if (!export.Empa.CheckSize())
          {
            break;
          }

          if (AsChar(export.Empa.Item.SelectEmpa.SelectChar) == 'S')
          {
            if (export.Empa.Item.EmpaEmployer.Identifier == export
              .WsEmployer.Identifier)
            {
              var field1 = GetField(export.Empa.Item.EmpaEmployer, "name");

              field1.Error = true;

              var field2 = GetField(export.Empa.Item.EmpaEmployer, "ein");

              field2.Error = true;

              var field3 = GetField(export.WsEmployer, "name");

              field3.Error = true;

              var field4 = GetField(export.WsEmployer, "ein");

              field4.Error = true;

              ExitState = "CANT_HAVE_A_RELATIONSHIP_EMPLOYE";

              return;
            }

            if (IsEmpty(export.Empa.Item.EmpaEmployerRelation.Type1))
            {
              var field =
                GetField(export.Empa.Item.EmpaEmployerRelation, "type1");

              field.Error = true;

              ExitState = "MUST_HAVE_A_RELATIONSHIP_TYPE";

              return;
            }
            else
            {
              local.Code.CodeName = "EMPLOYER RELATIONSHIP TYPE";
              local.CodeValue.Cdvalue =
                export.Empa.Item.EmpaEmployerRelation.Type1;
              UseCabValidateCodeValue();

              if (AsChar(local.Rtn.Flag) != 'Y')
              {
                var field =
                  GetField(export.Empa.Item.EmpaEmployerRelation, "type1");

                field.Error = true;

                ExitState = "ACO_NE0000_INVLD_EMP_RELSHIP_CD";

                return;
              }
            }

            if (export.ConfirmCreateRelationsh.Count >= 1)
            {
              export.ConfirmCreateRelationsh.Flag = "Y";
            }
            else
            {
              export.ConfirmCreateRelationsh.Count = 1;
            }

            if (AsChar(export.ScreenSelect.Text1) == 'X')
            {
              if (ReadEmployerRelationEmployer1())
              {
                // the worksite has an active relationship for the type with 
                // some employer that is not current main employer
                if (!Equal(entities.EmployerRelation.Type1, "INS"))
                {
                  if (!Lt(entities.EmployerRelation.EndDate,
                    export.Empa.Item.EmpEff.Date))
                  {
                    var field1 =
                      GetField(export.Empa.Item.SelectEmpa, "selectChar");

                    field1.Error = true;

                    var field2 =
                      GetField(export.Empa.Item.EmpaEmployer, "name");

                    field2.Error = true;

                    var field3 = GetField(export.Empa.Item.EmpaEmployer, "ein");

                    field3.Error = true;

                    var field4 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "street1");
                      

                    field4.Error = true;

                    var field5 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "street2");
                      

                    field5.Error = true;

                    var field6 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "city");

                    field6.Error = true;

                    var field7 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "state");

                    field7.Error = true;

                    var field8 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "zipCode");
                      

                    field8.Error = true;

                    var field9 = GetField(export.Empa.Item.EmpEff, "date");

                    field9.Error = true;

                    var field10 = GetField(export.Empa.Item.EmpaEnd, "date");

                    field10.Error = true;

                    ExitState = "EMPLOYER_RELATIO_TYPE_AE";

                    return;
                  }
                }
              }

              if (ReadEmployerRelationEmployer3())
              {
                // the worksite has an active relationship for the type with 
                // current main employer
                if (!Equal(entities.EmployerRelation.Type1, "INS"))
                {
                  var field1 =
                    GetField(export.Empa.Item.SelectEmpa, "selectChar");

                  field1.Error = true;

                  var field2 = GetField(export.Empa.Item.EmpaEmployer, "name");

                  field2.Error = true;

                  var field3 = GetField(export.Empa.Item.EmpaEmployer, "ein");

                  field3.Error = true;

                  var field4 =
                    GetField(export.Empa.Item.EmpaEmployerAddress, "street1");

                  field4.Error = true;

                  var field5 =
                    GetField(export.Empa.Item.EmpaEmployerAddress, "street2");

                  field5.Error = true;

                  var field6 =
                    GetField(export.Empa.Item.EmpaEmployerAddress, "city");

                  field6.Error = true;

                  var field7 =
                    GetField(export.Empa.Item.EmpaEmployerAddress, "state");

                  field7.Error = true;

                  var field8 =
                    GetField(export.Empa.Item.EmpaEmployerAddress, "zipCode");

                  field8.Error = true;

                  var field9 = GetField(export.Empa.Item.EmpEff, "date");

                  field9.Error = true;

                  var field10 = GetField(export.Empa.Item.EmpaEnd, "date");

                  field10.Error = true;

                  if (!Lt(local.CurrentDate.Date,
                    entities.EmployerRelation.EffectiveDate) && !
                    Lt(entities.EmployerRelation.EndDate, local.CurrentDate.Date))
                    
                  {
                    ExitState = "EMPLOYER_RELATIONSHIP_ALREADY_AT";
                  }
                  else
                  {
                    ExitState = "EMPLOYER_RELATIONSHIP_ALREADY_NA";
                  }

                  return;
                }
              }
            }
            else
            {
              if (ReadEmployerRelationEmployer4())
              {
                if (!Equal(entities.EmployerRelation.Type1, "INS"))
                {
                  var field1 =
                    GetField(export.Empa.Item.SelectEmpa, "selectChar");

                  field1.Error = true;

                  var field2 = GetField(export.Empa.Item.EmpaEmployer, "name");

                  field2.Error = true;

                  var field3 = GetField(export.Empa.Item.EmpaEmployer, "ein");

                  field3.Error = true;

                  var field4 =
                    GetField(export.Empa.Item.EmpaEmployerAddress, "street1");

                  field4.Error = true;

                  var field5 =
                    GetField(export.Empa.Item.EmpaEmployerAddress, "street2");

                  field5.Error = true;

                  var field6 =
                    GetField(export.Empa.Item.EmpaEmployerAddress, "city");

                  field6.Error = true;

                  var field7 =
                    GetField(export.Empa.Item.EmpaEmployerAddress, "state");

                  field7.Error = true;

                  var field8 =
                    GetField(export.Empa.Item.EmpaEmployerAddress, "zipCode");

                  field8.Error = true;

                  var field9 = GetField(export.Empa.Item.EmpEff, "date");

                  field9.Error = true;

                  var field10 = GetField(export.Empa.Item.EmpaEnd, "date");

                  field10.Error = true;

                  if (!Lt(local.CurrentDate.Date,
                    entities.EmployerRelation.EffectiveDate) && !
                    Lt(entities.EmployerRelation.EndDate, local.CurrentDate.Date))
                    
                  {
                    ExitState = "EMPLOYER_RELATIONSHIP_ALREADY_AT";
                  }
                  else
                  {
                    ExitState = "EMPLOYER_RELATIONSHIP_ALREADY_NA";
                  }

                  return;
                }
              }

              if (ReadEmployerRelationEmployer5())
              {
                // looking for relationships with any other employers other than
                // the current main employer
                if (!Lt(local.CurrentDate.Date,
                  entities.EmployerRelation.EffectiveDate) && !
                  Lt(entities.EmployerRelation.EndDate, local.CurrentDate.Date))
                {
                  if (Equal(entities.EmployerRelation.Type1, "INS"))
                  {
                  }
                  else if (!Lt(export.Empa.Item.EmpEff.Date,
                    local.CurrentDate.Date))
                  {
                    if (!Lt(export.Empa.Item.EmpEff.Date,
                      entities.EmployerRelation.EffectiveDate) && !
                      Lt(entities.EmployerRelation.EndDate,
                      export.Empa.Item.EmpEff.Date))
                    {
                      var field1 =
                        GetField(export.Empa.Item.SelectEmpa, "selectChar");

                      field1.Error = true;

                      var field2 =
                        GetField(export.Empa.Item.EmpaEmployer, "name");

                      field2.Error = true;

                      var field3 =
                        GetField(export.Empa.Item.EmpaEmployer, "ein");

                      field3.Error = true;

                      var field4 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "street1");
                        

                      field4.Error = true;

                      var field5 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "street2");
                        

                      field5.Error = true;

                      var field6 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "city");

                      field6.Error = true;

                      var field7 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "state");
                        

                      field7.Error = true;

                      var field8 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "zipCode");
                        

                      field8.Error = true;

                      var field9 =
                        GetField(export.Empa.Item.EmpaEmployerRelation, "type1");
                        

                      field9.Error = true;

                      var field10 = GetField(export.Empa.Item.EmpEff, "date");

                      field10.Error = true;

                      var field11 = GetField(export.Empa.Item.EmpaEnd, "date");

                      field11.Error = true;

                      ExitState = "ACTIVE_EMPLOYER_RELATIONSHIP_AE";

                      return;
                    }
                  }
                  else
                  {
                    var field1 =
                      GetField(export.Empa.Item.SelectEmpa, "selectChar");

                    field1.Error = true;

                    var field2 =
                      GetField(export.Empa.Item.EmpaEmployer, "name");

                    field2.Error = true;

                    var field3 = GetField(export.Empa.Item.EmpaEmployer, "ein");

                    field3.Error = true;

                    var field4 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "street1");
                      

                    field4.Error = true;

                    var field5 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "street2");
                      

                    field5.Error = true;

                    var field6 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "city");

                    field6.Error = true;

                    var field7 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "state");

                    field7.Error = true;

                    var field8 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "zipCode");
                      

                    field8.Error = true;

                    var field9 =
                      GetField(export.Empa.Item.EmpaEmployerRelation, "type1");

                    field9.Error = true;

                    var field10 = GetField(export.Empa.Item.EmpEff, "date");

                    field10.Error = true;

                    var field11 = GetField(export.Empa.Item.EmpaEnd, "date");

                    field11.Error = true;

                    ExitState = "ACTIVE_EMPLOYER_RELATIONSHIP_AE";

                    return;
                  }
                }
              }
            }

            if (AsChar(export.ConfirmCreateRelationsh.Flag) != 'Y')
            {
              for(export.Empa.Index = 0; export.Empa.Index < export.Empa.Count; ++
                export.Empa.Index)
              {
                if (!export.Empa.CheckSize())
                {
                  break;
                }

                var field11 =
                  GetField(export.Empa.Item.SelectEmpa, "selectChar");

                field11.Color = "";
                field11.Protected = true;

                var field12 = GetField(export.Empa.Item.PrmtEmplEmpa, "flag");

                field12.Protected = true;

                var field13 = GetField(export.Empa.Item.PrmtTypeEmpa, "flag");

                field13.Protected = true;

                var field14 = GetField(export.Empa.Item.EmpaEmployer, "name");

                field14.Protected = true;

                var field15 = GetField(export.Empa.Item.EmpaEmployer, "ein");

                field15.Protected = true;

                var field16 =
                  GetField(export.Empa.Item.EmpaEmployerAddress, "street1");

                field16.Protected = true;

                var field17 =
                  GetField(export.Empa.Item.EmpaEmployerAddress, "street2");

                field17.Protected = true;

                var field18 =
                  GetField(export.Empa.Item.EmpaEmployerAddress, "city");

                field18.Protected = true;

                var field19 =
                  GetField(export.Empa.Item.EmpaEmployerAddress, "state");

                field19.Protected = true;

                var field20 =
                  GetField(export.Empa.Item.EmpaEmployerAddress, "zipCode");

                field20.Protected = true;

                var field21 =
                  GetField(export.Empa.Item.EmpaEmployerRelation, "type1");

                field21.Protected = true;

                var field22 =
                  GetField(export.Empa.Item.EmpaEmployerRelation, "note");

                field22.Protected = true;

                var field23 = GetField(export.Empa.Item.EmpEff, "date");

                field23.Protected = true;

                var field24 = GetField(export.Empa.Item.EmpaEnd, "date");

                field24.Protected = true;

                var field25 = GetField(export.Empa.Item.EmpaEnd, "date");

                field25.Protected = true;
              }

              export.Empa.CheckIndex();

              var field1 = GetField(export.ConfirmCreateRelationsh, "flag");

              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.WsEmployer, "name");

              field2.Protected = true;

              var field3 = GetField(export.WsEmployer, "ein");

              field3.Protected = true;

              var field4 = GetField(export.WsEmployerAddress, "city");

              field4.Protected = true;

              var field5 = GetField(export.WsEmployerAddress, "state");

              field5.Protected = true;

              var field6 = GetField(export.WsEmployerAddress, "zipCode");

              field6.Protected = true;

              var field7 = GetField(export.WsEmployerAddress, "street1");

              field7.Protected = true;

              var field8 = GetField(export.WsEmployerAddress, "street2");

              field8.Protected = true;

              var field9 = GetField(export.WsEmployerRelation, "type1");

              field9.Protected = true;

              var field10 = GetField(export.TypePrompt, "flag");

              field10.Protected = true;

              export.ConfirmCreateRelationsh.Flag = "";
              ExitState = "CONFIRM_CREATING_EMPY_RELSHIP";

              return;
            }

            if (Lt(new DateTime(1, 1, 1), export.Empa.Item.EmpEff.Date))
            {
              if (Lt(export.Empa.Item.EmpEff.Date, local.CurrentDate.Date))
              {
                var field = GetField(export.Empa.Item.EmpEff, "date");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

                return;
              }

              export.Empa.Update.EmpaEmployerRelation.EffectiveDate =
                export.Empa.Item.EmpEff.Date;
            }

            if (Lt(new DateTime(1, 1, 1), export.Empa.Item.EmpaEnd.Date))
            {
              if (Lt(export.Empa.Item.EmpaEnd.Date, local.CurrentDate.Date) && !
                Equal(export.Empa.Item.EmpaEnd.Date,
                export.Empa.Item.EmpaEmployerRelation.EndDate))
              {
                var field = GetField(export.Empa.Item.EmpaEnd, "date");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

                return;
              }

              export.Empa.Update.EmpaEmployerRelation.EndDate =
                export.Empa.Item.EmpaEnd.Date;
            }

            local.ChgEmployer.Identifier =
              export.Empa.Item.EmpaEmployer.Identifier;
            MoveEmployerRelation2(export.Empa.Item.EmpaEmployerRelation,
              local.ChgEmployerRelation);
            UseSiCreateEmployerRelation();
            export.ConfirmCreateRelationsh.Flag = "";
            export.ConfirmCreateRelationsh.Count = 0;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
              export.Empa.Update.SelectEmpa.SelectChar = "*";
              export.Empa.Update.EmpEff.Date =
                export.Empa.Item.EmpaEmployerRelation.EffectiveDate;
              export.Empa.Update.OrginalEmployer.Identifier =
                export.Empa.Item.EmpaEmployer.Identifier;
              export.Empa.Update.OrginalEmployerRelation.Type1 =
                export.Empa.Item.EmpaEmployerRelation.Type1;

              if (Equal(export.Empa.Item.EmpaEmployerRelation.EndDate,
                local.DiscountineDate.Date))
              {
                export.Empa.Update.EmpaEnd.Date = local.Null1.Date;
              }
              else
              {
                export.Empa.Update.EmpaEnd.Date =
                  export.Empa.Item.EmpaEmployerRelation.EndDate;
              }
            }
            else
            {
              var field1 = GetField(export.Empa.Item.SelectEmpa, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.Empa.Item.EmpaEmployerRelation, "type1");

              field2.Error = true;

              var field3 = GetField(export.Empa.Item.EmpEff, "date");

              field3.Error = true;

              var field4 = GetField(export.Empa.Item.EmpaEnd, "date");

              field4.Error = true;

              var field5 = GetField(export.Empa.Item.EmpaEmployer, "name");

              field5.Error = true;
            }
          }
        }

        export.Empa.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }

        break;
      case "UPDATE":
        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Empa.Index = 0;

        for(var limit = export.Empa.Count; export.Empa.Index < limit; ++
          export.Empa.Index)
        {
          if (!export.Empa.CheckSize())
          {
            break;
          }

          if (AsChar(export.Empa.Item.SelectEmpa.SelectChar) == 'S')
          {
            if (!Equal(export.Empa.Item.OrginalEmployerRelation.Type1,
              export.Empa.Item.EmpaEmployerRelation.Type1))
            {
              var field1 =
                GetField(export.Empa.Item.EmpaEmployerRelation, "type1");

              field1.Error = true;

              var field2 = GetField(export.Empa.Item.SelectEmpa, "selectChar");

              field2.Error = true;

              ExitState = "CANT_CHG_EMPER_RELATIONSHIP_TYPE";

              return;
            }

            if (export.Empa.Item.EmpaEmployer.Identifier != export
              .Empa.Item.OrginalEmployer.Identifier)
            {
              var field1 = GetField(export.Empa.Item.EmpaEmployer, "name");

              field1.Error = true;

              var field2 = GetField(export.Empa.Item.EmpaEmployer, "ein");

              field2.Error = true;

              var field3 = GetField(export.Empa.Item.SelectEmpa, "selectChar");

              field3.Error = true;

              ExitState = "CAN_NOT_CHANGE_EMPLOYER_IN_A_UPD";

              return;
            }

            if (Lt(new DateTime(1, 1, 1), export.Empa.Item.EmpaEnd.Date) && Lt
              (export.Empa.Item.EmpaEnd.Date, export.Empa.Item.EmpEff.Date))
            {
              var field1 = GetField(export.Empa.Item.EmpaEnd, "date");

              field1.Error = true;

              var field2 = GetField(export.Empa.Item.SelectEmpa, "selectChar");

              field2.Error = true;

              ExitState = "INVALID_EFF_END_DATE_COMBINATION";

              return;
            }

            if (Lt(new DateTime(1, 1, 1), export.Empa.Item.EmpaEnd.Date) && Lt
              (export.Empa.Item.EmpaEnd.Date, local.CurrentDate.Date))
            {
              var field1 = GetField(export.Empa.Item.EmpaEnd, "date");

              field1.Error = true;

              var field2 = GetField(export.Empa.Item.SelectEmpa, "selectChar");

              field2.Error = true;

              ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

              return;
            }

            if (Equal(export.Empa.Item.EmpaEnd.Date, local.DiscountineDate.Date) &&
              !
              Equal(export.Empa.Item.EmpaEmployerRelation.EndDate,
              local.Max.Date) && Lt
              (local.Null1.Date, export.Empa.Item.EmpaEmployerRelation.EndDate) ||
              !Lt(local.Null1.Date, export.Empa.Item.EmpaEnd.Date) && Lt
              (export.Empa.Item.EmpaEmployerRelation.EndDate,
              local.DiscountineDate.Date))
            {
              if (AsChar(export.ScreenSelect.Text1) == 'X')
              {
                if (ReadEmployerRelationEmployer3())
                {
                  // there is a relationship for the type between the employers
                  if (!Lt(entities.EmployerRelation.EndDate,
                    local.CurrentDate.Date))
                  {
                    ExitState = "EMPLOYER_RELATIONSHIP_ALREADY_AT";

                    var field1 =
                      GetField(export.Empa.Item.SelectEmpa, "selectChar");

                    field1.Error = true;

                    var field2 =
                      GetField(export.Empa.Item.EmpaEmployer, "name");

                    field2.Error = true;

                    var field3 = GetField(export.Empa.Item.EmpaEmployer, "ein");

                    field3.Error = true;

                    var field4 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "street1");
                      

                    field4.Error = true;

                    var field5 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "street2");
                      

                    field5.Error = true;

                    var field6 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "city");

                    field6.Error = true;

                    var field7 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "state");

                    field7.Error = true;

                    var field8 =
                      GetField(export.Empa.Item.EmpaEmployerAddress, "zipCode");
                      

                    field8.Error = true;

                    var field9 = GetField(export.Empa.Item.EmpEff, "date");

                    field9.Error = true;

                    var field10 = GetField(export.Empa.Item.EmpaEnd, "date");

                    field10.Error = true;

                    return;
                  }
                  else
                  {
                  }
                }

                if (ReadEmployerRelationEmployer2())
                {
                  // the worksite employer already has an active relationship 
                  // for this type with someone
                  if (!Equal(entities.EmployerRelation.Type1, "INS"))
                  {
                    if (!Lt(entities.EmployerRelation.EndDate,
                      export.Empa.Item.EmpEff.Date))
                    {
                      var field1 =
                        GetField(export.Empa.Item.SelectEmpa, "selectChar");

                      field1.Error = true;

                      var field2 =
                        GetField(export.Empa.Item.EmpaEmployer, "name");

                      field2.Error = true;

                      var field3 =
                        GetField(export.Empa.Item.EmpaEmployer, "ein");

                      field3.Error = true;

                      var field4 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "street1");
                        

                      field4.Error = true;

                      var field5 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "street2");
                        

                      field5.Error = true;

                      var field6 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "city");

                      field6.Error = true;

                      var field7 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "state");
                        

                      field7.Error = true;

                      var field8 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "zipCode");
                        

                      field8.Error = true;

                      var field9 = GetField(export.Empa.Item.EmpEff, "date");

                      field9.Error = true;

                      var field10 = GetField(export.Empa.Item.EmpaEnd, "date");

                      field10.Error = true;

                      ExitState = "EMPLOYER_RELATIO_TYPE_AE";

                      return;
                    }
                  }
                }
              }
              else
              {
                if (ReadEmployerRelationEmployer4())
                {
                  if (!Equal(entities.EmployerRelation.Type1, "INS"))
                  {
                    if (!Lt(entities.EmployerRelation.EndDate,
                      local.CurrentDate.Date))
                    {
                      ExitState = "EMPLOYER_RELATIONSHIP_ALREADY_AT";

                      var field1 =
                        GetField(export.Empa.Item.SelectEmpa, "selectChar");

                      field1.Error = true;

                      var field2 =
                        GetField(export.Empa.Item.EmpaEmployer, "name");

                      field2.Error = true;

                      var field3 =
                        GetField(export.Empa.Item.EmpaEmployer, "ein");

                      field3.Error = true;

                      var field4 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "street1");
                        

                      field4.Error = true;

                      var field5 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "street2");
                        

                      field5.Error = true;

                      var field6 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "city");

                      field6.Error = true;

                      var field7 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "state");
                        

                      field7.Error = true;

                      var field8 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "zipCode");
                        

                      field8.Error = true;

                      var field9 = GetField(export.Empa.Item.EmpEff, "date");

                      field9.Error = true;

                      var field10 = GetField(export.Empa.Item.EmpaEnd, "date");

                      field10.Error = true;

                      return;
                    }
                    else
                    {
                    }
                  }
                }

                if (ReadEmployerRelationEmployer6())
                {
                  if (!Lt(entities.EmployerRelation.EndDate,
                    local.CurrentDate.Date))
                  {
                    if (Equal(entities.EmployerRelation.Type1, "INS"))
                    {
                    }
                    else if (!Lt(export.Empa.Item.EmpEff.Date,
                      local.CurrentDate.Date))
                    {
                      if (!Lt(entities.EmployerRelation.EndDate,
                        export.Empa.Item.EmpEff.Date))
                      {
                        var field1 =
                          GetField(export.Empa.Item.SelectEmpa, "selectChar");

                        field1.Error = true;

                        var field2 =
                          GetField(export.Empa.Item.EmpaEmployer, "name");

                        field2.Error = true;

                        var field3 =
                          GetField(export.Empa.Item.EmpaEmployer, "ein");

                        field3.Error = true;

                        var field4 =
                          GetField(export.Empa.Item.EmpaEmployerAddress,
                          "street1");

                        field4.Error = true;

                        var field5 =
                          GetField(export.Empa.Item.EmpaEmployerAddress,
                          "street2");

                        field5.Error = true;

                        var field6 =
                          GetField(export.Empa.Item.EmpaEmployerAddress, "city");
                          

                        field6.Error = true;

                        var field7 =
                          GetField(export.Empa.Item.EmpaEmployerAddress, "state");
                          

                        field7.Error = true;

                        var field8 =
                          GetField(export.Empa.Item.EmpaEmployerAddress,
                          "zipCode");

                        field8.Error = true;

                        var field9 =
                          GetField(export.Empa.Item.EmpaEmployerRelation,
                          "type1");

                        field9.Error = true;

                        var field10 = GetField(export.Empa.Item.EmpEff, "date");

                        field10.Error = true;

                        var field11 =
                          GetField(export.Empa.Item.EmpaEnd, "date");

                        field11.Error = true;

                        ExitState = "ACTIVE_EMPLOYER_RELATIONSHIP_AE";

                        return;
                      }
                    }
                    else
                    {
                      var field1 =
                        GetField(export.Empa.Item.SelectEmpa, "selectChar");

                      field1.Error = true;

                      var field2 =
                        GetField(export.Empa.Item.EmpaEmployer, "name");

                      field2.Error = true;

                      var field3 =
                        GetField(export.Empa.Item.EmpaEmployer, "ein");

                      field3.Error = true;

                      var field4 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "street1");
                        

                      field4.Error = true;

                      var field5 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "street2");
                        

                      field5.Error = true;

                      var field6 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "city");

                      field6.Error = true;

                      var field7 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "state");
                        

                      field7.Error = true;

                      var field8 =
                        GetField(export.Empa.Item.EmpaEmployerAddress, "zipCode");
                        

                      field8.Error = true;

                      var field9 =
                        GetField(export.Empa.Item.EmpaEmployerRelation, "type1");
                        

                      field9.Error = true;

                      var field10 = GetField(export.Empa.Item.EmpEff, "date");

                      field10.Error = true;

                      var field11 = GetField(export.Empa.Item.EmpaEnd, "date");

                      field11.Error = true;

                      ExitState = "ACTIVE_EMPLOYER_RELATIONSHIP_AE";

                      return;
                    }
                  }
                }
              }
            }

            if (!Lt(new DateTime(1, 1, 1), export.Empa.Item.EmpEff.Date))
            {
              export.Empa.Update.EmpaEmployerRelation.EffectiveDate =
                local.CurrentDate.Date;
            }

            if (!Lt(new DateTime(1, 1, 1), export.Empa.Item.EmpaEnd.Date) && !
              Lt(new DateTime(1, 1, 1),
              export.Empa.Item.EmpaEmployerRelation.EndDate))
            {
              export.Empa.Update.EmpaEmployerRelation.EndDate =
                local.DiscountineDate.Date;
            }

            if (!Lt(new DateTime(1, 1, 1), export.Empa.Item.EmpaEnd.Date) && Lt
              (export.Empa.Item.EmpaEmployerRelation.EndDate, new DateTime(2099,
              12, 31)))
            {
              // reactivating relationship
              export.Empa.Update.EmpaEmployerRelation.EndDate =
                local.DiscountineDate.Date;

              if (Lt(local.CurrentDate.Date, export.Empa.Item.EmpEff.Date))
              {
                export.Empa.Update.EmpaEmployerRelation.EffectiveDate =
                  export.Empa.Item.EmpEff.Date;
              }
              else
              {
                export.Empa.Update.EmpaEmployerRelation.EffectiveDate =
                  local.CurrentDate.Date;
              }
            }

            if (!Equal(export.Empa.Item.EmpEff.Date,
              export.Empa.Item.EmpaEmployerRelation.EffectiveDate))
            {
              if (!Lt(export.Empa.Item.EmpEff.Date, local.CurrentDate.Date))
              {
                export.Empa.Update.EmpaEmployerRelation.EffectiveDate =
                  export.Empa.Item.EmpEff.Date;
              }
              else if (Lt(export.Empa.Item.EmpEff.Date, local.CurrentDate.Date))
              {
              }
            }

            if (Lt(new DateTime(1, 1, 1), export.Empa.Item.EmpaEnd.Date) && !
              Equal(export.Empa.Item.EmpaEnd.Date,
              export.Empa.Item.EmpaEmployerRelation.EndDate))
            {
              export.Empa.Update.EmpaEmployerRelation.EndDate =
                export.Empa.Item.EmpaEnd.Date;
            }

            local.ChgEmployer.Identifier =
              export.Empa.Item.EmpaEmployer.Identifier;
            MoveEmployerRelation2(export.Empa.Item.EmpaEmployerRelation,
              local.ChgEmployerRelation);

            if (!Lt(export.Empa.Item.EmpaEmployerRelation.EffectiveDate,
              local.CurrentDate.Date) && !
              Lt(new DateTime(1, 1, 1), export.Empa.Item.EmpaEnd.Date))
            {
              local.ChgEmployerRelation.EndDate = local.DiscountineDate.Date;
            }

            UseSiUpdateEmployerRelation();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
              export.Empa.Update.SelectEmpa.SelectChar = "*";
              MoveEmployerRelation2(local.ChgEmployerRelation,
                export.Empa.Update.EmpaEmployerRelation);
              export.Empa.Update.EmpEff.Date =
                export.Empa.Item.EmpaEmployerRelation.EffectiveDate;

              if (Equal(export.Empa.Item.EmpaEmployerRelation.EndDate,
                local.DiscountineDate.Date))
              {
                export.Empa.Update.EmpaEnd.Date = local.Null1.Date;
              }
              else
              {
                export.Empa.Update.EmpaEnd.Date =
                  export.Empa.Item.EmpaEmployerRelation.EndDate;
              }
            }
          }
        }

        export.Empa.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Verify that all mandatory fields for a
        // display have been entered.
        // ---------------------------------------------
        if (export.WsEmployer.Identifier == 0)
        {
          var field1 = GetField(export.TypePrompt, "flag");

          field1.Protected = false;
          field1.Focused = true;

          var field2 = GetField(export.TypePrompt, "flag");

          field2.Error = true;

          var field3 = GetField(export.WsEmployer, "name");

          field3.Error = true;

          var field4 = GetField(export.WsEmployer, "ein");

          field4.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "MUST_SELECT_EMPLOYER_FROM_EMPL";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // --------------------------------------------
        // 10/27/98 W. Campbell  Removed an IF stmt
        // "IF export_hq employer identifier IS EQUAL TO 0"
        // which was around the following USE stmt
        // to give a positive response to a DISPLAY
        // command.
        // --------------------------------------------
        local.Scroll.CreatedTimestamp = Now();
        local.Scroll.EndDate = local.Max.Date;
        UseSiReadEmployerEmpa();

        // --------------------------------------------
        // 10/26/98 W. Campbell  Added the following
        // IF stmt to give a positive response to a DISPLAY
        // command.
        // --------------------------------------------
        if (export.Empa.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        export.Paging.Count = 0;
        export.Standard.PageNumber = 1;

        if (export.Empa.IsFull)
        {
          export.Plus.OneChar = "+";

          export.Paging.Index = 0;
          export.Paging.CheckSize();

          export.Empa.Index = 0;
          export.Empa.CheckSize();

          export.Paging.Update.Scroll.CreatedTimestamp =
            local.Scroll.CreatedTimestamp;
          export.Paging.Update.Scroll.EndDate = local.Scroll.EndDate;

          ++export.Paging.Index;
          export.Paging.CheckSize();

          export.Empa.Index = Export.EmpaGroup.Capacity - 1;
          export.Empa.CheckSize();

          export.Paging.Update.Scroll.CreatedTimestamp =
            export.Empa.Item.EmpaEmployerRelation.CreatedTimestamp;
          export.Paging.Update.Scroll.EndDate =
            export.Empa.Item.EmpaEmployerRelation.EndDate;
        }
        else
        {
          export.Plus.OneChar = "";

          export.Paging.Index = 0;
          export.Paging.CheckSize();

          export.Paging.Update.Scroll.CreatedTimestamp =
            local.Scroll.CreatedTimestamp;
          export.Paging.Update.Scroll.EndDate =
            export.Empa.Item.EmpaEmployerRelation.EndDate;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "NEXT":
        if (export.Standard.PageNumber == Export.PagingGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        if (IsEmpty(import.Plus.OneChar))
        {
          local.GroupCount.Count = 0;

          for(export.Empa.Index = 0; export.Empa.Index < export.Empa.Count; ++
            export.Empa.Index)
          {
            if (!export.Empa.CheckSize())
            {
              break;
            }

            if (export.Empa.Item.EmpaEmployer.Identifier > 0)
            {
              ++local.GroupCount.Count;
              local.Scroll.CreatedTimestamp =
                export.Empa.Item.EmpaEmployerRelation.CreatedTimestamp;
              local.Scroll.EndDate =
                export.Empa.Item.EmpaEmployerRelation.EndDate;
            }
          }

          export.Empa.CheckIndex();

          if (local.GroupCount.Count > 1)
          {
            if (Lt(local.Null1.Timestamp, local.Scroll.CreatedTimestamp))
            {
              local.Scroll.CreatedTimestamp =
                AddMicroseconds(local.Scroll.CreatedTimestamp, -1);
            }
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_FORWARD";

            return;
          }
        }

        ++export.Standard.PageNumber;

        export.Paging.Index = export.Standard.PageNumber - 1;
        export.Paging.CheckSize();

        if (Lt(local.Null1.Timestamp, export.Paging.Item.Scroll.CreatedTimestamp))
          
        {
          local.Scroll.CreatedTimestamp =
            export.Paging.Item.Scroll.CreatedTimestamp;
          local.Scroll.EndDate = export.Paging.Item.Scroll.EndDate;
        }

        UseSiReadEmployerEmpa();
        export.Minus.OneChar = "-";

        if (export.Empa.IsFull)
        {
          export.Plus.OneChar = "+";

          if (export.Paging.Index + 1 == Export.PagingGroup.Capacity)
          {
            ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

            return;
          }

          ++export.Paging.Index;
          export.Paging.CheckSize();

          export.Empa.Index = Export.EmpaGroup.Capacity - 1;
          export.Empa.CheckSize();

          export.Paging.Update.Scroll.CreatedTimestamp =
            export.Empa.Item.EmpaEmployerRelation.CreatedTimestamp;
          export.Paging.Update.Scroll.EndDate =
            export.Empa.Item.EmpaEmployerRelation.EndDate;
        }
        else
        {
          export.Plus.OneChar = "";
        }

        break;
      case "PREV":
        if (IsEmpty(import.Minus.OneChar))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.Standard.PageNumber;

        export.Paging.Index = export.Standard.PageNumber - 1;
        export.Paging.CheckSize();

        local.Scroll.CreatedTimestamp =
          export.Paging.Item.Scroll.CreatedTimestamp;
        local.Scroll.EndDate = export.Paging.Item.Scroll.EndDate;
        UseSiReadEmployerEmpa();

        if (export.Standard.PageNumber > 1)
        {
          export.Minus.OneChar = "-";
        }
        else
        {
          export.Minus.OneChar = "";
        }

        export.Plus.OneChar = "+";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "EMPL":
        ExitState = "ECO_XFR_TO_EMPLOYER_MAINTENANCE";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.Flag = source.Flag;
  }

  private static void MoveEmpa(SiReadEmployerEmpa.Export.EmpaGroup source,
    Export.EmpaGroup target)
  {
    target.SelectEmpa.SelectChar = source.Select.SelectChar;
    target.EmpaEmployer.Assign(source.Employer);
    target.PrmtEmplEmpa.Flag = source.PrmpEmpl.Flag;
    target.PrmtTypeEmpa.Flag = source.PrmpType.Flag;
    target.EmpaEmployerRelation.Assign(source.EmployerRelation);
    MoveEmployerAddress2(source.EmployerAddress, target.EmpaEmployerAddress);
    target.EmpEff.Date = source.EffDate.Date;
    target.EmpaEnd.Date = source.EndDate.Date;
    target.OrginalEmployerRelation.Type1 = source.OrginalEmployerRelation.Type1;
    target.OrginalEmployer.Identifier = source.OrginalEmployer.Identifier;
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveEmployerAddress1(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveEmployerAddress2(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveEmployerRelation1(EmployerRelation source,
    EmployerRelation target)
  {
    target.Note = source.Note;
    target.Identifier = source.Identifier;
  }

  private static void MoveEmployerRelation2(EmployerRelation source,
    EmployerRelation target)
  {
    target.Note = source.Note;
    target.Identifier = source.Identifier;
    target.EffectiveDate = source.EffectiveDate;
    target.EndDate = source.EndDate;
    target.Type1 = source.Type1;
  }

  private static void MoveEmployerRelation3(EmployerRelation source,
    EmployerRelation target)
  {
    target.EndDate = source.EndDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DiscountineDate.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Rtn.Flag = useExport.ValidCode.Flag;
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

    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateEmployerRelation()
  {
    var useImport = new SiCreateEmployerRelation.Import();
    var useExport = new SiCreateEmployerRelation.Export();

    useImport.EmployerRelation.Assign(local.ChgEmployerRelation);
    useImport.ServiceProvider.Identifier = local.ChgEmployer.Identifier;
    useImport.SelectScreen.Text1 = export.ScreenSelect.Text1;
    useImport.Ws.Identifier = export.WsEmployer.Identifier;

    Call(SiCreateEmployerRelation.Execute, useImport, useExport);

    export.Empa.Update.EmpaEmployerRelation.Assign(useExport.EmployerRelation);
  }

  private void UseSiReadEmployerEmpa()
  {
    var useImport = new SiReadEmployerEmpa.Import();
    var useExport = new SiReadEmployerEmpa.Export();

    useImport.Scroll.Assign(local.Scroll);
    useImport.ScreenSelect.Text1 = export.ScreenSelect.Text1;
    useImport.EmployerRelation.Type1 = export.WsEmployerRelation.Type1;
    useImport.Ws.Identifier = export.WsEmployer.Identifier;

    Call(SiReadEmployerEmpa.Execute, useImport, useExport);

    useExport.Empa.CopyTo(export.Empa, MoveEmpa);
    export.WsEmployer.Assign(useExport.WorksiteEmployer);
    MoveEmployerAddress1(useExport.WorksiteEmployerAddress,
      export.WsEmployerAddress);
  }

  private void UseSiUpdateEmployerRelation()
  {
    var useImport = new SiUpdateEmployerRelation.Import();
    var useExport = new SiUpdateEmployerRelation.Export();

    useImport.EmployerRelation.Assign(local.ChgEmployerRelation);
    useImport.ServiceProvider.Identifier = local.ChgEmployer.Identifier;
    useImport.ScreenSelect.Text1 = export.ScreenSelect.Text1;
    useImport.Ws.Identifier = export.WsEmployer.Identifier;

    Call(SiUpdateEmployerRelation.Execute, useImport, useExport);
  }

  private bool ReadEmployerRelationEmployer1()
  {
    entities.N2d.Populated = false;
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelationEmployer1",
      (db, command) =>
      {
        db.SetString(
          command, "type", export.Empa.Item.EmpaEmployerRelation.Type1);
        db.SetInt32(
          command, "identifier", export.Empa.Item.EmpaEmployer.Identifier);
        db.SetNullableDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
        db.SetInt32(command, "empHqId", export.WsEmployer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 3);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 4);
        entities.N2d.Identifier = db.GetInt32(reader, 4);
        entities.EmployerRelation.Type1 = db.GetString(reader, 5);
        entities.N2d.Ein = db.GetNullableString(reader, 6);
        entities.N2d.Name = db.GetNullableString(reader, 7);
        entities.N2d.Populated = true;
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadEmployerRelationEmployer2()
  {
    entities.N2d.Populated = false;
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelationEmployer2",
      (db, command) =>
      {
        db.SetString(
          command, "type", export.Empa.Item.EmpaEmployerRelation.Type1);
        db.SetInt32(
          command, "identifier", export.Empa.Item.EmpaEmployer.Identifier);
        db.SetNullableDate(
          command, "endDate", local.CurrentDate.Date.GetValueOrDefault());
        db.SetInt32(command, "empHqId", export.WsEmployer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 3);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 4);
        entities.N2d.Identifier = db.GetInt32(reader, 4);
        entities.EmployerRelation.Type1 = db.GetString(reader, 5);
        entities.N2d.Ein = db.GetNullableString(reader, 6);
        entities.N2d.Name = db.GetNullableString(reader, 7);
        entities.N2d.Populated = true;
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadEmployerRelationEmployer3()
  {
    entities.N2d.Populated = false;
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelationEmployer3",
      (db, command) =>
      {
        db.SetInt32(command, "empHqId", export.WsEmployer.Identifier);
        db.SetInt32(
          command, "identifier", export.Empa.Item.EmpaEmployer.Identifier);
        db.SetString(
          command, "type", export.Empa.Item.EmpaEmployerRelation.Type1);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 3);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 4);
        entities.N2d.Identifier = db.GetInt32(reader, 4);
        entities.EmployerRelation.Type1 = db.GetString(reader, 5);
        entities.N2d.Ein = db.GetNullableString(reader, 6);
        entities.N2d.Name = db.GetNullableString(reader, 7);
        entities.N2d.Populated = true;
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadEmployerRelationEmployer4()
  {
    entities.N2d.Populated = false;
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelationEmployer4",
      (db, command) =>
      {
        db.SetInt32(command, "empLocId", export.WsEmployer.Identifier);
        db.SetInt32(
          command, "identifier", export.Empa.Item.EmpaEmployer.Identifier);
        db.SetString(
          command, "type", export.Empa.Item.EmpaEmployerRelation.Type1);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 3);
        entities.N2d.Identifier = db.GetInt32(reader, 3);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 4);
        entities.EmployerRelation.Type1 = db.GetString(reader, 5);
        entities.N2d.Ein = db.GetNullableString(reader, 6);
        entities.N2d.Name = db.GetNullableString(reader, 7);
        entities.N2d.Populated = true;
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadEmployerRelationEmployer5()
  {
    entities.N2d.Populated = false;
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelationEmployer5",
      (db, command) =>
      {
        db.SetInt32(command, "empLocId", export.WsEmployer.Identifier);
        db.SetString(
          command, "type", export.Empa.Item.EmpaEmployerRelation.Type1);
        db.SetInt32(
          command, "identifier", export.Empa.Item.EmpaEmployer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 3);
        entities.N2d.Identifier = db.GetInt32(reader, 3);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 4);
        entities.EmployerRelation.Type1 = db.GetString(reader, 5);
        entities.N2d.Ein = db.GetNullableString(reader, 6);
        entities.N2d.Name = db.GetNullableString(reader, 7);
        entities.N2d.Populated = true;
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadEmployerRelationEmployer6()
  {
    entities.N2d.Populated = false;
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelationEmployer6",
      (db, command) =>
      {
        db.SetInt32(command, "empLocId", export.WsEmployer.Identifier);
        db.SetInt32(
          command, "identifier", export.Empa.Item.EmpaEmployer.Identifier);
        db.SetString(
          command, "type", export.Empa.Item.EmpaEmployerRelation.Type1);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 3);
        entities.N2d.Identifier = db.GetInt32(reader, 3);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 4);
        entities.EmployerRelation.Type1 = db.GetString(reader, 5);
        entities.N2d.Ein = db.GetNullableString(reader, 6);
        entities.N2d.Name = db.GetNullableString(reader, 7);
        entities.N2d.Populated = true;
        entities.EmployerRelation.Populated = true;
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
    /// <summary>A EmpaGroup group.</summary>
    [Serializable]
    public class EmpaGroup
    {
      /// <summary>
      /// A value of SelectEmpa.
      /// </summary>
      [JsonPropertyName("selectEmpa")]
      public Common SelectEmpa
      {
        get => selectEmpa ??= new();
        set => selectEmpa = value;
      }

      /// <summary>
      /// A value of EmpaEmployer.
      /// </summary>
      [JsonPropertyName("empaEmployer")]
      public Employer EmpaEmployer
      {
        get => empaEmployer ??= new();
        set => empaEmployer = value;
      }

      /// <summary>
      /// A value of EmpaEmployerRelation.
      /// </summary>
      [JsonPropertyName("empaEmployerRelation")]
      public EmployerRelation EmpaEmployerRelation
      {
        get => empaEmployerRelation ??= new();
        set => empaEmployerRelation = value;
      }

      /// <summary>
      /// A value of PrmtEmplEmpa.
      /// </summary>
      [JsonPropertyName("prmtEmplEmpa")]
      public Common PrmtEmplEmpa
      {
        get => prmtEmplEmpa ??= new();
        set => prmtEmplEmpa = value;
      }

      /// <summary>
      /// A value of PrmtTypeEmpa.
      /// </summary>
      [JsonPropertyName("prmtTypeEmpa")]
      public Common PrmtTypeEmpa
      {
        get => prmtTypeEmpa ??= new();
        set => prmtTypeEmpa = value;
      }

      /// <summary>
      /// A value of EmpaEmployerAddress.
      /// </summary>
      [JsonPropertyName("empaEmployerAddress")]
      public EmployerAddress EmpaEmployerAddress
      {
        get => empaEmployerAddress ??= new();
        set => empaEmployerAddress = value;
      }

      /// <summary>
      /// A value of EmpaEff.
      /// </summary>
      [JsonPropertyName("empaEff")]
      public DateWorkArea EmpaEff
      {
        get => empaEff ??= new();
        set => empaEff = value;
      }

      /// <summary>
      /// A value of EmpaEnd.
      /// </summary>
      [JsonPropertyName("empaEnd")]
      public DateWorkArea EmpaEnd
      {
        get => empaEnd ??= new();
        set => empaEnd = value;
      }

      /// <summary>
      /// A value of OrginalEmployerRelation.
      /// </summary>
      [JsonPropertyName("orginalEmployerRelation")]
      public EmployerRelation OrginalEmployerRelation
      {
        get => orginalEmployerRelation ??= new();
        set => orginalEmployerRelation = value;
      }

      /// <summary>
      /// A value of OrginalEmployer.
      /// </summary>
      [JsonPropertyName("orginalEmployer")]
      public Employer OrginalEmployer
      {
        get => orginalEmployer ??= new();
        set => orginalEmployer = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common selectEmpa;
      private Employer empaEmployer;
      private EmployerRelation empaEmployerRelation;
      private Common prmtEmplEmpa;
      private Common prmtTypeEmpa;
      private EmployerAddress empaEmployerAddress;
      private DateWorkArea empaEff;
      private DateWorkArea empaEnd;
      private EmployerRelation orginalEmployerRelation;
      private Employer orginalEmployer;
    }

    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of Scroll.
      /// </summary>
      [JsonPropertyName("scroll")]
      public EmployerRelation Scroll
      {
        get => scroll ??= new();
        set => scroll = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private EmployerRelation scroll;
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
    /// A value of ScreenSelect.
    /// </summary>
    [JsonPropertyName("screenSelect")]
    public WorkArea ScreenSelect
    {
      get => screenSelect ??= new();
      set => screenSelect = value;
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
    /// A value of WsEmployerRelation.
    /// </summary>
    [JsonPropertyName("wsEmployerRelation")]
    public EmployerRelation WsEmployerRelation
    {
      get => wsEmployerRelation ??= new();
      set => wsEmployerRelation = value;
    }

    /// <summary>
    /// A value of ScreenTitle.
    /// </summary>
    [JsonPropertyName("screenTitle")]
    public WorkArea ScreenTitle
    {
      get => screenTitle ??= new();
      set => screenTitle = value;
    }

    /// <summary>
    /// A value of ConfirmCreateRelationsh.
    /// </summary>
    [JsonPropertyName("confirmCreateRelationsh")]
    public Common ConfirmCreateRelationsh
    {
      get => confirmCreateRelationsh ??= new();
      set => confirmCreateRelationsh = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Standard Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public Standard Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// Gets a value of Empa.
    /// </summary>
    [JsonIgnore]
    public Array<EmpaGroup> Empa => empa ??= new(EmpaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Empa for json serialization.
    /// </summary>
    [JsonPropertyName("empa")]
    [Computed]
    public IList<EmpaGroup> Empa_Json
    {
      get => empa;
      set => Empa.Assign(value);
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
    /// A value of SelectEmployerAddress.
    /// </summary>
    [JsonPropertyName("selectEmployerAddress")]
    public EmployerAddress SelectEmployerAddress
    {
      get => selectEmployerAddress ??= new();
      set => selectEmployerAddress = value;
    }

    /// <summary>
    /// A value of SelectEmployer.
    /// </summary>
    [JsonPropertyName("selectEmployer")]
    public Employer SelectEmployer
    {
      get => selectEmployer ??= new();
      set => selectEmployer = value;
    }

    /// <summary>
    /// A value of AllRelationships.
    /// </summary>
    [JsonPropertyName("allRelationships")]
    public WorkArea AllRelationships
    {
      get => allRelationships ??= new();
      set => allRelationships = value;
    }

    /// <summary>
    /// A value of ImportselectRelationship.
    /// </summary>
    [JsonPropertyName("importselectRelationship")]
    public WorkArea ImportselectRelationship
    {
      get => importselectRelationship ??= new();
      set => importselectRelationship = value;
    }

    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    /// <summary>
    /// A value of RtnEmployerAddress.
    /// </summary>
    [JsonPropertyName("rtnEmployerAddress")]
    public EmployerAddress RtnEmployerAddress
    {
      get => rtnEmployerAddress ??= new();
      set => rtnEmployerAddress = value;
    }

    /// <summary>
    /// A value of RtnEmployer.
    /// </summary>
    [JsonPropertyName("rtnEmployer")]
    public Employer RtnEmployer
    {
      get => rtnEmployer ??= new();
      set => rtnEmployer = value;
    }

    /// <summary>
    /// A value of WsEmployer.
    /// </summary>
    [JsonPropertyName("wsEmployer")]
    public Employer WsEmployer
    {
      get => wsEmployer ??= new();
      set => wsEmployer = value;
    }

    /// <summary>
    /// A value of WsEmployerAddress.
    /// </summary>
    [JsonPropertyName("wsEmployerAddress")]
    public EmployerAddress WsEmployerAddress
    {
      get => wsEmployerAddress ??= new();
      set => wsEmployerAddress = value;
    }

    /// <summary>
    /// A value of TypePrompt.
    /// </summary>
    [JsonPropertyName("typePrompt")]
    public Common TypePrompt
    {
      get => typePrompt ??= new();
      set => typePrompt = value;
    }

    /// <summary>
    /// A value of EmployerPrompt.
    /// </summary>
    [JsonPropertyName("employerPrompt")]
    public Common EmployerPrompt
    {
      get => employerPrompt ??= new();
      set => employerPrompt = value;
    }

    /// <summary>
    /// A value of ServiceProviderEmployer.
    /// </summary>
    [JsonPropertyName("serviceProviderEmployer")]
    public Employer ServiceProviderEmployer
    {
      get => serviceProviderEmployer ??= new();
      set => serviceProviderEmployer = value;
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
    /// A value of ServiceProviderEmployerAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderEmployerAddress")]
    public EmployerAddress ServiceProviderEmployerAddress
    {
      get => serviceProviderEmployerAddress ??= new();
      set => serviceProviderEmployerAddress = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private CodeValue codeValue;
    private WorkArea screenSelect;
    private Code code;
    private EmployerRelation wsEmployerRelation;
    private WorkArea screenTitle;
    private Common confirmCreateRelationsh;
    private Standard minus;
    private Standard plus;
    private Array<EmpaGroup> empa;
    private Array<PagingGroup> paging;
    private EmployerAddress selectEmployerAddress;
    private Employer selectEmployer;
    private WorkArea allRelationships;
    private WorkArea importselectRelationship;
    private EmployerRelation employerRelation;
    private EmployerAddress rtnEmployerAddress;
    private Employer rtnEmployer;
    private Employer wsEmployer;
    private EmployerAddress wsEmployerAddress;
    private Common typePrompt;
    private Common employerPrompt;
    private Employer serviceProviderEmployer;
    private Case1 next;
    private EmployerAddress serviceProviderEmployerAddress;
    private Standard standard;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A EmpaGroup group.</summary>
    [Serializable]
    public class EmpaGroup
    {
      /// <summary>
      /// A value of SelectEmpa.
      /// </summary>
      [JsonPropertyName("selectEmpa")]
      public Common SelectEmpa
      {
        get => selectEmpa ??= new();
        set => selectEmpa = value;
      }

      /// <summary>
      /// A value of EmpaEmployer.
      /// </summary>
      [JsonPropertyName("empaEmployer")]
      public Employer EmpaEmployer
      {
        get => empaEmployer ??= new();
        set => empaEmployer = value;
      }

      /// <summary>
      /// A value of PrmtEmplEmpa.
      /// </summary>
      [JsonPropertyName("prmtEmplEmpa")]
      public Common PrmtEmplEmpa
      {
        get => prmtEmplEmpa ??= new();
        set => prmtEmplEmpa = value;
      }

      /// <summary>
      /// A value of PrmtTypeEmpa.
      /// </summary>
      [JsonPropertyName("prmtTypeEmpa")]
      public Common PrmtTypeEmpa
      {
        get => prmtTypeEmpa ??= new();
        set => prmtTypeEmpa = value;
      }

      /// <summary>
      /// A value of EmpaEmployerRelation.
      /// </summary>
      [JsonPropertyName("empaEmployerRelation")]
      public EmployerRelation EmpaEmployerRelation
      {
        get => empaEmployerRelation ??= new();
        set => empaEmployerRelation = value;
      }

      /// <summary>
      /// A value of EmpaEmployerAddress.
      /// </summary>
      [JsonPropertyName("empaEmployerAddress")]
      public EmployerAddress EmpaEmployerAddress
      {
        get => empaEmployerAddress ??= new();
        set => empaEmployerAddress = value;
      }

      /// <summary>
      /// A value of EmpEff.
      /// </summary>
      [JsonPropertyName("empEff")]
      public DateWorkArea EmpEff
      {
        get => empEff ??= new();
        set => empEff = value;
      }

      /// <summary>
      /// A value of EmpaEnd.
      /// </summary>
      [JsonPropertyName("empaEnd")]
      public DateWorkArea EmpaEnd
      {
        get => empaEnd ??= new();
        set => empaEnd = value;
      }

      /// <summary>
      /// A value of OrginalEmployerRelation.
      /// </summary>
      [JsonPropertyName("orginalEmployerRelation")]
      public EmployerRelation OrginalEmployerRelation
      {
        get => orginalEmployerRelation ??= new();
        set => orginalEmployerRelation = value;
      }

      /// <summary>
      /// A value of OrginalEmployer.
      /// </summary>
      [JsonPropertyName("orginalEmployer")]
      public Employer OrginalEmployer
      {
        get => orginalEmployer ??= new();
        set => orginalEmployer = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common selectEmpa;
      private Employer empaEmployer;
      private Common prmtEmplEmpa;
      private Common prmtTypeEmpa;
      private EmployerRelation empaEmployerRelation;
      private EmployerAddress empaEmployerAddress;
      private DateWorkArea empEff;
      private DateWorkArea empaEnd;
      private EmployerRelation orginalEmployerRelation;
      private Employer orginalEmployer;
    }

    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of Scroll.
      /// </summary>
      [JsonPropertyName("scroll")]
      public EmployerRelation Scroll
      {
        get => scroll ??= new();
        set => scroll = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private EmployerRelation scroll;
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
    /// A value of ScreenSelect.
    /// </summary>
    [JsonPropertyName("screenSelect")]
    public WorkArea ScreenSelect
    {
      get => screenSelect ??= new();
      set => screenSelect = value;
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
    /// A value of WsEmployerRelation.
    /// </summary>
    [JsonPropertyName("wsEmployerRelation")]
    public EmployerRelation WsEmployerRelation
    {
      get => wsEmployerRelation ??= new();
      set => wsEmployerRelation = value;
    }

    /// <summary>
    /// A value of ScreenTitle.
    /// </summary>
    [JsonPropertyName("screenTitle")]
    public WorkArea ScreenTitle
    {
      get => screenTitle ??= new();
      set => screenTitle = value;
    }

    /// <summary>
    /// A value of ConfirmCreateRelationsh.
    /// </summary>
    [JsonPropertyName("confirmCreateRelationsh")]
    public Common ConfirmCreateRelationsh
    {
      get => confirmCreateRelationsh ??= new();
      set => confirmCreateRelationsh = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Standard Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public Standard Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// Gets a value of Empa.
    /// </summary>
    [JsonIgnore]
    public Array<EmpaGroup> Empa => empa ??= new(EmpaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Empa for json serialization.
    /// </summary>
    [JsonPropertyName("empa")]
    [Computed]
    public IList<EmpaGroup> Empa_Json
    {
      get => empa;
      set => Empa.Assign(value);
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
    /// A value of AllRelationships.
    /// </summary>
    [JsonPropertyName("allRelationships")]
    public WorkArea AllRelationships
    {
      get => allRelationships ??= new();
      set => allRelationships = value;
    }

    /// <summary>
    /// A value of SelectRelationship.
    /// </summary>
    [JsonPropertyName("selectRelationship")]
    public WorkArea SelectRelationship
    {
      get => selectRelationship ??= new();
      set => selectRelationship = value;
    }

    /// <summary>
    /// A value of SelectEmployerAddress.
    /// </summary>
    [JsonPropertyName("selectEmployerAddress")]
    public EmployerAddress SelectEmployerAddress
    {
      get => selectEmployerAddress ??= new();
      set => selectEmployerAddress = value;
    }

    /// <summary>
    /// A value of SelectEmployer.
    /// </summary>
    [JsonPropertyName("selectEmployer")]
    public Employer SelectEmployer
    {
      get => selectEmployer ??= new();
      set => selectEmployer = value;
    }

    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    /// <summary>
    /// A value of WsEmployer.
    /// </summary>
    [JsonPropertyName("wsEmployer")]
    public Employer WsEmployer
    {
      get => wsEmployer ??= new();
      set => wsEmployer = value;
    }

    /// <summary>
    /// A value of WsEmployerAddress.
    /// </summary>
    [JsonPropertyName("wsEmployerAddress")]
    public EmployerAddress WsEmployerAddress
    {
      get => wsEmployerAddress ??= new();
      set => wsEmployerAddress = value;
    }

    /// <summary>
    /// A value of TypePrompt.
    /// </summary>
    [JsonPropertyName("typePrompt")]
    public Common TypePrompt
    {
      get => typePrompt ??= new();
      set => typePrompt = value;
    }

    /// <summary>
    /// A value of EmployerPrompt.
    /// </summary>
    [JsonPropertyName("employerPrompt")]
    public Common EmployerPrompt
    {
      get => employerPrompt ??= new();
      set => employerPrompt = value;
    }

    /// <summary>
    /// A value of ServiceProviderEmployer.
    /// </summary>
    [JsonPropertyName("serviceProviderEmployer")]
    public Employer ServiceProviderEmployer
    {
      get => serviceProviderEmployer ??= new();
      set => serviceProviderEmployer = value;
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
    /// A value of ServiceProviderEmployerAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderEmployerAddress")]
    public EmployerAddress ServiceProviderEmployerAddress
    {
      get => serviceProviderEmployerAddress ??= new();
      set => serviceProviderEmployerAddress = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private CodeValue codeValue;
    private WorkArea screenSelect;
    private Code code;
    private EmployerRelation wsEmployerRelation;
    private WorkArea screenTitle;
    private Common confirmCreateRelationsh;
    private Standard minus;
    private Standard plus;
    private Array<EmpaGroup> empa;
    private Array<PagingGroup> paging;
    private WorkArea allRelationships;
    private WorkArea selectRelationship;
    private EmployerAddress selectEmployerAddress;
    private Employer selectEmployer;
    private EmployerRelation employerRelation;
    private Employer wsEmployer;
    private EmployerAddress wsEmployerAddress;
    private Common typePrompt;
    private Common employerPrompt;
    private Employer serviceProviderEmployer;
    private Case1 next;
    private EmployerAddress serviceProviderEmployerAddress;
    private Standard standard;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of GroupCount.
    /// </summary>
    [JsonPropertyName("groupCount")]
    public Common GroupCount
    {
      get => groupCount ??= new();
      set => groupCount = value;
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
    /// A value of UpdateOk.
    /// </summary>
    [JsonPropertyName("updateOk")]
    public Common UpdateOk
    {
      get => updateOk ??= new();
      set => updateOk = value;
    }

    /// <summary>
    /// A value of DiscountineDate.
    /// </summary>
    [JsonPropertyName("discountineDate")]
    public DateWorkArea DiscountineDate
    {
      get => discountineDate ??= new();
      set => discountineDate = value;
    }

    /// <summary>
    /// A value of ChgEmployerRelation.
    /// </summary>
    [JsonPropertyName("chgEmployerRelation")]
    public EmployerRelation ChgEmployerRelation
    {
      get => chgEmployerRelation ??= new();
      set => chgEmployerRelation = value;
    }

    /// <summary>
    /// A value of ChgEmployer.
    /// </summary>
    [JsonPropertyName("chgEmployer")]
    public Employer ChgEmployer
    {
      get => chgEmployer ??= new();
      set => chgEmployer = value;
    }

    /// <summary>
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public EmployerRelation Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
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
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of Rtn.
    /// </summary>
    [JsonPropertyName("rtn")]
    public Common Rtn
    {
      get => rtn ??= new();
      set => rtn = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    private Common groupCount;
    private DateWorkArea null1;
    private Common updateOk;
    private DateWorkArea discountineDate;
    private EmployerRelation chgEmployerRelation;
    private Employer chgEmployer;
    private EmployerRelation scroll;
    private Common common;
    private Common update;
    private DateWorkArea currentDate;
    private Common select;
    private Code code;
    private Common rtn;
    private CodeValue codeValue;
    private DateWorkArea max;
    private Common command;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of N2d.
    /// </summary>
    [JsonPropertyName("n2d")]
    public Employer N2d
    {
      get => n2d ??= new();
      set => n2d = value;
    }

    /// <summary>
    /// A value of Ws.
    /// </summary>
    [JsonPropertyName("ws")]
    public Employer Ws
    {
      get => ws ??= new();
      set => ws = value;
    }

    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
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
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
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

    private Employer n2d;
    private Employer ws;
    private EmployerRelation employerRelation;
    private Profile profile;
    private ServiceProviderProfile serviceProviderProfile;
    private ServiceProvider serviceProvider;
  }
#endregion
}
