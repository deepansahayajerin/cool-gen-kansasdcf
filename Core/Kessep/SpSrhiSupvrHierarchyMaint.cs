// Program: SP_SRHI_SUPVR_HIERARCHY_MAINT, ID: 371783558, model: 746.
// Short name: SWESRHIP
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
/// A program: SP_SRHI_SUPVR_HIERARCHY_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpSrhiSupvrHierarchyMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_SRHI_SUPVR_HIERARCHY_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpSrhiSupvrHierarchyMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpSrhiSupvrHierarchyMaint.
  /// </summary>
  public SpSrhiSupvrHierarchyMaint(IContext context, Import import,
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
    // *********************************************
    // ** DATE      *  DESCRIPTION
    // ** 04/13/95     J. Kemp
    // ** 02/15/96     A. HACKLER     RETROFITS
    // ** 04/22/96     S. Konkader    Added edit for 'PF4 & no         selection
    // / multiple selection',  Field fill char + for
    //         detail prompts. Deleted all logic for RETURN.
    // ** 01/04/97	R. Marchman	Add new security/next tran.
    // ** 04/10/97     J. Rookard      Debug for handling multiple occurrences 
    // of OSP.
    // ** 04/30/97     J. Rookard      Current_Date retrofit to Local_Current 
    // Date_work_area date.
    // ** 11/13/98     A. Massey       screen assessment fixes
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    export.Search.LastName = import.Search.LastName;
    export.LeaderServiceProvider.Assign(import.LeaderServiceProvider);
    export.SubHidden.Assign(import.SubHidden);
    export.LeaderOfficeServiceProvider.
      Assign(import.LeaderOfficeServiceProvider);
    MoveCommon(import.ListLeadingSrvProvder, export.ListLeadingSrvProvder);
    MoveCommon(import.ListSub, export.ListSub);
    export.CodeValue.Description = import.CodeValue.Description;
    export.SubServiceProvider.Assign(import.SubServiceProvider);
    MoveOfficeServiceProvider(import.SubOfficeServiceProvider,
      export.SubOfficeServiceProvider);

    if (Equal(global.Command, "RTLIST"))
    {
      if (AsChar(import.ListSub.SelectChar) == 'S')
      {
        var field = GetField(export.ListSub, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }
      else if (AsChar(import.ListLeadingSrvProvder.SelectChar) == 'S')
      {
        var field = GetField(export.ListLeadingSrvProvder, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (import.HiddenSelection.SystemGeneratedId != 0 && AsChar
        (import.ListSub.SelectChar) == 'S')
      {
        export.SubServiceProvider.Assign(import.HiddenSelection);
        export.SubOfficeServiceProvider.RoleCode =
          import.HiddenSelection.RoleCode ?? Spaces(2);

        if (export.LeaderServiceProvider.SystemGeneratedId == 0 && IsEmpty
          (export.LeaderOfficeServiceProvider.RoleCode))
        {
          export.ListSub.SelectChar = "";
          global.Command = "DISPLAY";
        }
      }

      export.ListSub.SelectChar = "";

      if (import.HiddenSelection.SystemGeneratedId != 0 && AsChar
        (import.ListLeadingSrvProvder.SelectChar) == 'S')
      {
        export.LeaderServiceProvider.Assign(import.HiddenSelection);
        export.LeaderOfficeServiceProvider.RoleCode =
          import.HiddenSelection.RoleCode ?? Spaces(2);
        global.Command = "DISPLAY";
      }

      export.ListLeadingSrvProvder.SelectChar = "";
    }

    if (Equal(global.Command, "RTLIST"))
    {
      if (import.Selection.Count > 0)
      {
        local.RecCount.Count = 0;

        export.Export1.Index = 0;
        export.Export1.Clear();

        do
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          ++local.RecCount.Count;

          import.Selection.Index = local.RecCount.Count - 1;
          import.Selection.CheckSize();

          MoveServiceProvider3(import.Selection.Item.Selection1,
            export.Export1.Update.ServiceProvider);
          MoveServiceProvider2(import.Selection.Item.Selection1,
            export.Export1.Update.HiddenServiceProvider);
          export.Export1.Update.Common.SelectChar = "S";
          export.Export1.Update.ListSrvProvder.SelectChar = "";

          if (export.Export1.Item.ServiceProvider.SystemGeneratedId > 0)
          {
            var field =
              GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");
              

            field.Color = "cyan";
            field.Protected = true;
          }

          if (local.RecCount.Count == import.Selection.Count)
          {
            export.Export1.Next();

            goto Test1;
          }

          export.Export1.Next();
        }
        while(!export.Export1.IsFull);
      }
      else
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
          export.Export1.Update.ServiceProvider.Assign(
            import.Import1.Item.ServiceProvider);
          export.Export1.Update.OfficeServiceProvRelationship.Assign(
            import.Import1.Item.OfficeServiceProvRelationship);
          export.Export1.Update.OfficeServiceProvider.Assign(
            import.Import1.Item.OfficeServiceProvider);
          export.Export1.Update.HiddenServiceProvider.Assign(
            import.Import1.Item.HiddenServiceProvider);
          export.Export1.Update.HiddenOfficeServiceProvRelationship.Assign(
            import.Import1.Item.HiddenOfficeServiceProvRelationship);
          MoveCommon(import.Import1.Item.ListSrvProvder,
            export.Export1.Update.ListSrvProvder);
          export.Export1.Update.HiddenOfficeServiceProvider.Assign(
            import.Import1.Item.HiddenOfficeServiceProvider);

          if (export.Export1.Item.ServiceProvider.SystemGeneratedId > 0)
          {
            var field =
              GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");
              

            field.Color = "cyan";
            field.Protected = true;
          }

          export.Export1.Next();
        }
      }
    }
    else
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
        export.Export1.Update.ServiceProvider.Assign(
          import.Import1.Item.ServiceProvider);
        export.Export1.Update.OfficeServiceProvRelationship.Assign(
          import.Import1.Item.OfficeServiceProvRelationship);
        export.Export1.Update.OfficeServiceProvider.Assign(
          import.Import1.Item.OfficeServiceProvider);
        export.Export1.Update.HiddenServiceProvider.Assign(
          import.Import1.Item.HiddenServiceProvider);
        export.Export1.Update.HiddenOfficeServiceProvRelationship.Assign(
          import.Import1.Item.HiddenOfficeServiceProvRelationship);
        MoveCommon(import.Import1.Item.ListSrvProvder,
          export.Export1.Update.ListSrvProvder);
        export.Export1.Update.HiddenOfficeServiceProvider.Assign(
          import.Import1.Item.HiddenOfficeServiceProvider);

        if (export.Export1.Item.ServiceProvider.SystemGeneratedId > 0)
        {
          var field =
            GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");

          field.Color = "cyan";
          field.Protected = true;
        }

        export.Export1.Next();
      }
    }

Test1:

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "LIST") || Equal
      (global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "PREV") || Equal(global.Command, "DELETE"))
    {
    }
    else
    {
      // **** begin group B ****
      export.Standard.NextTransaction = import.Standard.NextTransaction;

      // if the next tran info is not equal to spaces, this implies the user 
      // requested a next tran action. now validate
      if (IsEmpty(import.Standard.NextTransaction))
      {
      }
      else
      {
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
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is coming into this procedure on a next tran action.
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** end   group B ****
    if (Equal(global.Command, "RTLIST"))
    {
    }
    else
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "RTLIST"))
    {
      return;
    }

    if (Equal(global.Command, "LIST"))
    {
      if (AsChar(import.ListSub.SelectChar) == 'S' && AsChar
        (import.ListLeadingSrvProvder.SelectChar) == 'S')
      {
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        var field1 = GetField(export.ListSub, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.ListLeadingSrvProvder, "selectChar");

        field2.Error = true;

        return;
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.ListSrvProvder.SelectChar) == 'S')
        {
          if (AsChar(export.ListLeadingSrvProvder.SelectChar) == 'S' || AsChar
            (export.ListSub.SelectChar) == 'S')
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
          }
        }
      }

      if (AsChar(import.ListSub.SelectChar) == 'S')
      {
        MoveServiceProvider1(import.SubServiceProvider, export.HiddenSelectSub);
        ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";
        ++local.Count.Count;

        return;
      }

      if (AsChar(import.ListLeadingSrvProvder.SelectChar) == 'S')
      {
        MoveServiceProvider1(import.LeaderServiceProvider,
          export.HiddenSelectLeader);
        ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";
        ++local.Count.Count;

        return;
      }

      // Begin*****************siraj 4/22/1996*********************
      local.Dtl.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.ListSrvProvder.SelectChar) == 'S')
        {
          ++local.Dtl.Count;
        }
      }

      if (local.Dtl.Count == 0)
      {
        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        return;
      }
      else if (local.Dtl.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.ListSrvProvder.SelectChar) == 'S')
          {
            var field =
              GetField(export.Export1.Item.ListSrvProvder, "selectChar");

            field.Error = true;
          }
        }

        return;
      }

      // End*****************siraj 4/22/1996*********************
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "LIST"))
    {
      if (!Equal(global.Command, "LIST"))
      {
        if (import.LeaderServiceProvider.SystemGeneratedId == 0)
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field =
            GetField(export.LeaderServiceProvider, "systemGeneratedId");

          field.Error = true;
        }

        if (IsEmpty(export.LeaderOfficeServiceProvider.RoleCode))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.LeaderOfficeServiceProvider, "roleCode");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      if (Equal(global.Command, "DELETE"))
      {
        if (!IsEmpty(export.SubServiceProvider.UserId))
        {
          ExitState = "CAN_NOT_DELETE_RECORD";

          var field = GetField(export.SubServiceProvider, "userId");

          field.Error = true;

          return;
        }
      }

      // Check to see if a selection has been made.
      local.Count.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.Common.SelectChar) && AsChar
          (export.Export1.Item.Common.SelectChar) != '*' || AsChar
          (export.Export1.Item.ListSrvProvder.SelectChar) != '+' && !
          IsEmpty(export.Export1.Item.ListSrvProvder.SelectChar))
        {
          ++local.Count.Count;

          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' || AsChar
            (export.Export1.Item.ListSrvProvder.SelectChar) == 'S')
          {
            if (IsExitState("ACO_NI0000_ADD_SUCCESSFUL") || IsExitState
              ("ACO_NI0000_DELETE_SUCCESSFUL") || IsExitState
              ("ACO_NI0000_UPDATE_SUCCESSFUL"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            if (!Equal(global.Command, "LIST"))
            {
              if (export.Export1.Item.ServiceProvider.SystemGeneratedId == 0)
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                var field =
                  GetField(export.Export1.Item.ServiceProvider,
                  "systemGeneratedId");

                field.Error = true;
              }

              if (IsEmpty(export.Export1.Item.ServiceProvider.RoleCode))
              {
                if (ReadServiceProvider())
                {
                  export.Export1.Update.ServiceProvider.Assign(
                    entities.ServiceProvider);
                }
                else
                {
                  ExitState = "SERVICE_PROVIDER_NF";

                  var field =
                    GetField(export.Export1.Item.ServiceProvider,
                    "systemGeneratedId");

                  field.Error = true;
                }
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }
            }

            switch(TrimEnd(global.Command))
            {
              case "LIST":
                if (AsChar(export.Export1.Item.ListSrvProvder.SelectChar) == 'S'
                  )
                {
                  export.HiddenSelectLeader.Assign(
                    export.Export1.Item.ServiceProvider);
                  export.MultipleSelect.SelectChar = "R";
                  ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

                  return;
                }

                break;
              case "ADD":
                if (Equal(import.LeaderOfficeServiceProvider.RoleCode, "AT") ||
                  Equal(import.LeaderOfficeServiceProvider.RoleCode, "SS") || Equal
                  (import.LeaderOfficeServiceProvider.RoleCode, "PM") || Equal
                  (import.LeaderOfficeServiceProvider.RoleCode, "TR"))
                {
                }
                else
                {
                  ExitState = "NO_ACTIVE_ROLE_CODE";

                  var field1 = GetField(export.LeaderServiceProvider, "userId");

                  field1.Error = true;

                  var field2 =
                    GetField(export.LeaderOfficeServiceProvider, "roleCode");

                  field2.Error = true;

                  return;
                }

                local.RecordFound.Flag = "";

                // find all the offices that the workers are actively in
                foreach(var item in ReadOfficeServiceProviderServiceProviderOffice())
                  
                {
                  local.RecordFound.Flag = "Y";

                  // now find the active supervisor they had for the current 
                  // office
                  foreach(var item1 in ReadServiceProviderOfficeServiceProviderOfficeServiceProvRelationship())
                    
                  {
                    // worker already had a different supervisor so we need to 
                    // delete that relationship
                    UseSpDeleteServicePrvdrRelation1();

                    if (!IsExitState("ACO_NI0000_DELETE_SUCCESSFUL"))
                    {
                      goto Test2;
                    }
                  }

                  // checking to see if the supervisor is a active supervisor in
                  // the office, if they are
                  // not then need to create a office service provider 
                  // supervisor record for them for the
                  // office
                  if (ReadServiceProviderOfficeServiceProvider1())
                  {
                    // good to create relationship with worker
                    local.New1.Assign(entities.LeaderOfficeServiceProvider);
                  }
                  else
                  {
                    // not a supervisor in a office so we need to create a 
                    // office service provider record for this service provider
                    local.New1.WorkFaxAreaCode =
                      import.LeaderOfficeServiceProvider.WorkFaxAreaCode.
                        GetValueOrDefault();
                    local.New1.WorkFaxNumber =
                      import.LeaderOfficeServiceProvider.WorkFaxNumber.
                        GetValueOrDefault();
                    local.New1.WorkPhoneAreaCode =
                      import.LeaderOfficeServiceProvider.WorkPhoneAreaCode;
                    local.New1.WorkPhoneNumber =
                      import.LeaderOfficeServiceProvider.WorkPhoneNumber;
                    local.New1.WorkPhoneExtension =
                      import.LeaderOfficeServiceProvider.WorkPhoneExtension ?? ""
                      ;
                    local.New1.LocalContactCodeForIrs =
                      import.LeaderOfficeServiceProvider.LocalContactCodeForIrs.
                        GetValueOrDefault();
                    local.New1.RoleCode =
                      import.LeaderOfficeServiceProvider.RoleCode;
                    local.New1.EffectiveDate = local.Current.Date;
                    local.New1.DiscontinueDate =
                      UseCabSetMaximumDiscontinueDate();
                    ExitState = "ACO_NN0000_ALL_OK";
                    UseSpCreateOfficeServicePrvdr();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      goto Test2;
                    }
                  }

                  UseSpCreateServicePrvdrRelation();

                  if (!IsExitState("ACO_NI0000_ADD_SUCCESSFUL"))
                  {
                    goto Test2;
                  }
                }

                if (AsChar(local.RecordFound.Flag) != 'Y')
                {
                  ExitState = "OFFICE_SERVICE_PROVIDER_NF";

                  var field1 =
                    GetField(export.Export1.Item.ServiceProvider,
                    "systemGeneratedId");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Export1.Item.ServiceProvider, "roleCode");

                  field2.Error = true;
                }

                break;
              case "UPDATE":
                // since reason code was taken off the screen, there is no need 
                // to keep the update code
                // since the only thing it was updating was the reason code
                break;
              case "DELETE":
                // need to delete every relatonship between the supervisor and 
                // the service person
                foreach(var item in ReadOfficeServiceProviderOfficeServiceProvider())
                  
                {
                  UseSpDeleteServicePrvdrRelation2();

                  if (!IsExitState("ACO_NI0000_DELETE_SUCCESSFUL"))
                  {
                    goto Test2;
                  }
                }

                break;
              default:
                break;
            }

Test2:

            if (!IsExitState("ACO_NI0000_ADD_SUCCESSFUL") && !
              IsExitState("ACO_NI0000_DELETE_SUCCESSFUL") && !
              IsExitState("ACO_NI0000_UPDATE_SUCCESSFUL"))
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              export.Export1.Update.HiddenServiceProvider.Assign(
                export.Export1.Item.ServiceProvider);
              export.Export1.Update.HiddenOfficeServiceProvider.Assign(
                export.Export1.Item.OfficeServiceProvider);

              return;
            }
            else
            {
              export.Export1.Update.Common.SelectChar = "*";
            }
          }
          else if (AsChar(export.Export1.Item.Common.SelectChar) == '*')
          {
            export.Export1.Update.Common.SelectChar = "";
          }
          else
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }
      }

      if (Equal(global.Command, "LIST"))
      {
        if (local.Count.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
      }
      else if (local.Count.Count == 0)
      {
        if (Equal(global.Command, "ADD"))
        {
          ExitState = "NO_SUBORDINATE_SELECTED";
        }
        else if (Equal(global.Command, "UPDATE") || Equal
          (global.Command, "DELETE"))
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";
        }
      }
    }
    else
    {
      // Main CASE OF COMMAND.
      switch(TrimEnd(global.Command))
      {
        case "SIGNOFF":
          UseScCabSignoff();

          break;
        case "DISPLAY":
          if ((import.SubServiceProvider.SystemGeneratedId > 0 || export
            .SubServiceProvider.SystemGeneratedId > 0) && import
            .LeaderServiceProvider.SystemGeneratedId > 0)
          {
            var field1 = GetField(export.LeaderServiceProvider, "lastName");

            field1.Error = true;

            var field2 = GetField(export.SubServiceProvider, "lastName");

            field2.Error = true;

            ExitState = "ACO_NI0000_CLEAR_SCREEN_TO_DISP";

            return;
          }

          if (export.LeaderServiceProvider.SystemGeneratedId == 0 && IsEmpty
            (export.LeaderServiceProvider.UserId) && IsEmpty
            (export.SubServiceProvider.UserId) && IsEmpty
            (export.SubServiceProvider.UserId))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            var field = GetField(export.LeaderServiceProvider, "userId");

            field.Error = true;

            export.ListLeadingSrvProvder.SelectChar = "S";

            return;
          }

          if (!IsEmpty(export.SubServiceProvider.UserId) && IsEmpty
            (import.LeaderServiceProvider.UserId))
          {
            if (ReadServiceProviderServiceProviderOfficeServiceProvider())
            {
              export.LeaderOfficeServiceProvider.Assign(
                entities.LeaderOfficeServiceProvider);
              export.LeaderServiceProvider.
                Assign(entities.LeaderServiceProvider);
              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.SubOfficeServiceProvider);
              MoveServiceProvider1(entities.ServiceProvider,
                export.SubServiceProvider);

              return;
            }
            else
            {
              ExitState = "ACO_NE0000_NO_ACTIVE_SUPERVISOR";

              var field = GetField(export.SubServiceProvider, "userId");

              field.Error = true;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          export.LeaderHidden.SystemGeneratedId = 0;

          if (ReadServiceProviderOfficeServiceProvider2())
          {
            if (export.LeaderHidden.SystemGeneratedId > 0)
            {
              goto Read;
            }

            export.LeaderServiceProvider.Assign(entities.LeaderServiceProvider);
            export.LeaderHidden.Assign(entities.LeaderServiceProvider);
            export.LeaderOfficeServiceProvider.Assign(
              entities.LeaderOfficeServiceProvider);
            local.CompareRead.SystemGeneratedId = 0;

            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadServiceProviderOfficeServiceProvRelationship())
              
            {
              if (entities.ServiceProvider.SystemGeneratedId == local
                .CompareRead.SystemGeneratedId)
              {
                export.Export1.Next();

                continue;
              }

              local.CompareRead.SystemGeneratedId =
                entities.ServiceProvider.SystemGeneratedId;
              export.Export1.Update.ServiceProvider.Assign(
                entities.ServiceProvider);
              MoveOfficeServiceProvRelationship(entities.
                OfficeServiceProvRelationship,
                export.Export1.Update.OfficeServiceProvRelationship);
              export.Export1.Update.ListSrvProvder.SelectChar = "+";
              export.Export1.Update.Common.SelectChar = "";

              var field =
                GetField(export.Export1.Item.ServiceProvider,
                "systemGeneratedId");

              field.Color = "cyan";
              field.Protected = true;

              export.Export1.Next();
            }

            if (export.Export1.IsEmpty)
            {
              ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
            }
            else
            {
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }
          }

Read:

          if (export.LeaderHidden.SystemGeneratedId <= 0)
          {
            ExitState = "NO_ACTIVE_ROLE_CODE";

            var field1 = GetField(export.LeaderServiceProvider, "userId");

            field1.Error = true;

            var field2 =
              GetField(export.LeaderOfficeServiceProvider, "roleCode");

            field2.Error = true;
          }

          break;
        case "NEXT":
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          break;
        case "PREV":
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          break;
        default:
          ExitState = "ACO_NE0000_INVALID_COMMAND";

          break;
      }
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
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

  private static void MoveOfficeServiceProvRelationship(
    OfficeServiceProvRelationship source, OfficeServiceProvRelationship target)
  {
    target.ReasonCode = source.ReasonCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedDtstamp = source.CreatedDtstamp;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveServiceProvider1(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private static void MoveServiceProvider2(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
  }

  private static void MoveServiceProvider3(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCreateOfficeServicePrvdr()
  {
    var useImport = new SpCreateOfficeServicePrvdr.Import();
    var useExport = new SpCreateOfficeServicePrvdr.Export();

    useImport.Office.SystemGeneratedId =
      entities.LeaderOffice.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      import.LeaderServiceProvider.SystemGeneratedId;
    useImport.OfficeServiceProvider.Assign(local.New1);

    Call(SpCreateOfficeServicePrvdr.Execute, useImport, useExport);
  }

  private void UseSpCreateServicePrvdrRelation()
  {
    var useImport = new SpCreateServicePrvdrRelation.Import();
    var useExport = new SpCreateServicePrvdrRelation.Export();

    useImport.Office.SystemGeneratedId =
      entities.LeaderOffice.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;
    useImport.LeadingServiceProvider.SystemGeneratedId =
      import.LeaderServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(local.New1, useImport.LeadingOfficeServiceProvider);
      
    useImport.OfficeServiceProvRelationship.ReasonCode =
      export.Export1.Item.OfficeServiceProvRelationship.ReasonCode;

    Call(SpCreateServicePrvdrRelation.Execute, useImport, useExport);

    export.Export1.Update.OfficeServiceProvRelationship.Assign(
      useExport.OfficeServiceProvRelationship);
  }

  private void UseSpDeleteServicePrvdrRelation1()
  {
    var useImport = new SpDeleteServicePrvdrRelation.Import();
    var useExport = new SpDeleteServicePrvdrRelation.Export();

    useImport.LeaderServiceProvider.SystemGeneratedId =
      entities.PrevLeaderServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.PrevLeaderOfficeServiceProvider,
      useImport.LeaderOfficeServiceProvider);
    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;

    Call(SpDeleteServicePrvdrRelation.Execute, useImport, useExport);

    export.Export1.Update.OfficeServiceProvRelationship.Assign(
      useExport.OfficeServiceProvRelationship);
  }

  private void UseSpDeleteServicePrvdrRelation2()
  {
    var useImport = new SpDeleteServicePrvdrRelation.Import();
    var useExport = new SpDeleteServicePrvdrRelation.Export();

    useImport.LeaderServiceProvider.SystemGeneratedId =
      import.LeaderServiceProvider.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.LeaderOfficeServiceProvider,
      useImport.LeaderOfficeServiceProvider);
    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;

    Call(SpDeleteServicePrvdrRelation.Execute, useImport, useExport);

    export.Export1.Update.OfficeServiceProvRelationship.Assign(
      useExport.OfficeServiceProvRelationship);
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider()
  {
    entities.LeaderOfficeServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.OfficeServiceProvRelationship.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId1",
          import.LeaderServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId2",
          export.Export1.Item.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.LeaderOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProvRelationship.SpdRGeneratedId =
          db.GetInt32(reader, 0);
        entities.LeaderOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.OfficeServiceProvRelationship.OffRGeneratedId =
          db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.LeaderOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvRelationship.OspRRoleCode =
          db.GetString(reader, 2);
        entities.LeaderOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.OfficeServiceProvRelationship.OspREffectiveDt =
          db.GetDate(reader, 3);
        entities.LeaderOfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 4);
        entities.LeaderOfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.LeaderOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LeaderOfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.LeaderOfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.LeaderOfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.LeaderOfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 11);
        entities.OfficeServiceProvRelationship.SpdGeneratedId =
          db.GetInt32(reader, 11);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 12);
        entities.OfficeServiceProvRelationship.OffGeneratedId =
          db.GetInt32(reader, 12);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 12);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 13);
        entities.OfficeServiceProvRelationship.OspRoleCode =
          db.GetString(reader, 13);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 14);
        entities.OfficeServiceProvRelationship.OspEffectiveDate =
          db.GetDate(reader, 14);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.OfficeServiceProvRelationship.ReasonCode =
          db.GetString(reader, 16);
        entities.OfficeServiceProvRelationship.CreatedBy =
          db.GetString(reader, 17);
        entities.OfficeServiceProvRelationship.CreatedDtstamp =
          db.GetDateTime(reader, 18);
        entities.Office.TypeCode = db.GetString(reader, 19);
        entities.Office.Name = db.GetString(reader, 20);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 21);
        entities.LeaderOfficeServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.OfficeServiceProvRelationship.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderServiceProviderOffice()
  {
    entities.LeaderOffice.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          export.Export1.Item.ServiceProvider.SystemGeneratedId);
        db.SetString(
          command, "roleCode", export.Export1.Item.ServiceProvider.RoleCode ?? ""
          );
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.LeaderOffice.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 5);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.ServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.ServiceProvider.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.ServiceProvider.UserId = db.GetString(reader, 9);
        entities.ServiceProvider.LastName = db.GetString(reader, 10);
        entities.ServiceProvider.FirstName = db.GetString(reader, 11);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 12);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 13);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 14);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.LeaderOffice.TypeCode = db.GetString(reader, 16);
        entities.LeaderOffice.Name = db.GetString(reader, 17);
        entities.LeaderOffice.OffOffice = db.GetNullableInt32(reader, 18);
        entities.LeaderOffice.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          export.Export1.Item.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ServiceProvider.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.LastName = db.GetString(reader, 6);
        entities.ServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 8);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 9);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 10);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderOfficeServiceProvRelationship()
  {
    return ReadEach("ReadServiceProviderOfficeServiceProvRelationship",
      (db, command) =>
      {
        db.SetString(command, "lastName", export.Search.LastName);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.LeaderServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ServiceProvider.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.LastName = db.GetString(reader, 6);
        entities.ServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 8);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 9);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 10);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.OfficeServiceProvRelationship.OspEffectiveDate =
          db.GetDate(reader, 12);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 12);
        entities.OfficeServiceProvRelationship.OspRoleCode =
          db.GetString(reader, 13);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 13);
        entities.OfficeServiceProvRelationship.OffGeneratedId =
          db.GetInt32(reader, 14);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 14);
        entities.OfficeServiceProvRelationship.SpdGeneratedId =
          db.GetInt32(reader, 15);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 15);
        entities.OfficeServiceProvRelationship.OspREffectiveDt =
          db.GetDate(reader, 16);
        entities.OfficeServiceProvRelationship.OspRRoleCode =
          db.GetString(reader, 17);
        entities.OfficeServiceProvRelationship.OffRGeneratedId =
          db.GetInt32(reader, 18);
        entities.OfficeServiceProvRelationship.SpdRGeneratedId =
          db.GetInt32(reader, 19);
        entities.OfficeServiceProvRelationship.ReasonCode =
          db.GetString(reader, 20);
        entities.OfficeServiceProvRelationship.CreatedBy =
          db.GetString(reader, 21);
        entities.OfficeServiceProvRelationship.CreatedDtstamp =
          db.GetDateTime(reader, 22);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 23);
        entities.OfficeServiceProvider.Populated = true;
        entities.OfficeServiceProvRelationship.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvider1()
  {
    entities.LeaderOfficeServiceProvider.Populated = false;
    entities.LeaderServiceProvider.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          import.LeaderServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "offGeneratedId", entities.LeaderOffice.SystemGeneratedId);
        db.SetString(
          command, "roleCode", import.LeaderOfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LeaderServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.LeaderOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.LeaderServiceProvider.UserId = db.GetString(reader, 1);
        entities.LeaderServiceProvider.LastName = db.GetString(reader, 2);
        entities.LeaderServiceProvider.FirstName = db.GetString(reader, 3);
        entities.LeaderOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 4);
        entities.LeaderOfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.LeaderOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 6);
        entities.LeaderOfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 7);
        entities.LeaderOfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 8);
        entities.LeaderOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.LeaderOfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.LeaderOfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 11);
        entities.LeaderOfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 12);
        entities.LeaderOfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 13);
        entities.LeaderOfficeServiceProvider.Populated = true;
        entities.LeaderServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvider2()
  {
    entities.LeaderOfficeServiceProvider.Populated = false;
    entities.LeaderServiceProvider.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "userId", export.LeaderServiceProvider.UserId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LeaderServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.LeaderOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.LeaderServiceProvider.UserId = db.GetString(reader, 1);
        entities.LeaderServiceProvider.LastName = db.GetString(reader, 2);
        entities.LeaderServiceProvider.FirstName = db.GetString(reader, 3);
        entities.LeaderOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 4);
        entities.LeaderOfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.LeaderOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 6);
        entities.LeaderOfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 7);
        entities.LeaderOfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 8);
        entities.LeaderOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.LeaderOfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.LeaderOfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 11);
        entities.LeaderOfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 12);
        entities.LeaderOfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 13);
        entities.LeaderOfficeServiceProvider.Populated = true;
        entities.LeaderServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadServiceProviderOfficeServiceProviderOfficeServiceProvRelationship()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Prev.Populated = false;
    entities.PrevLeaderServiceProvider.Populated = false;
    entities.PrevLeaderOfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return ReadEach(
      "ReadServiceProviderOfficeServiceProviderOfficeServiceProvRelationship",
      (db, command) =>
      {
        db.SetString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetInt32(
          command, "servicePrvderId",
          import.LeaderServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.PrevLeaderServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.PrevLeaderOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.Prev.SpdRGeneratedId = db.GetInt32(reader, 0);
        entities.PrevLeaderServiceProvider.UserId = db.GetString(reader, 1);
        entities.PrevLeaderServiceProvider.LastName = db.GetString(reader, 2);
        entities.PrevLeaderServiceProvider.FirstName = db.GetString(reader, 3);
        entities.PrevLeaderOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 4);
        entities.Prev.OffRGeneratedId = db.GetInt32(reader, 4);
        entities.PrevLeaderOfficeServiceProvider.RoleCode =
          db.GetString(reader, 5);
        entities.Prev.OspRRoleCode = db.GetString(reader, 5);
        entities.PrevLeaderOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 6);
        entities.Prev.OspREffectiveDt = db.GetDate(reader, 6);
        entities.PrevLeaderOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.Prev.OspEffectiveDate = db.GetDate(reader, 8);
        entities.Prev.OspRoleCode = db.GetString(reader, 9);
        entities.Prev.OffGeneratedId = db.GetInt32(reader, 10);
        entities.Prev.SpdGeneratedId = db.GetInt32(reader, 11);
        entities.Prev.ReasonCode = db.GetString(reader, 12);
        entities.Prev.CreatedBy = db.GetString(reader, 13);
        entities.Prev.CreatedDtstamp = db.GetDateTime(reader, 14);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 15);
        entities.Office.TypeCode = db.GetString(reader, 16);
        entities.Office.Name = db.GetString(reader, 17);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 18);
        entities.Prev.Populated = true;
        entities.PrevLeaderServiceProvider.Populated = true;
        entities.PrevLeaderOfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProviderServiceProviderOfficeServiceProvider()
  {
    entities.LeaderOfficeServiceProvider.Populated = false;
    entities.LeaderServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProviderServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", export.SubServiceProvider.UserId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 1);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.ServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ServiceProvider.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.LastName = db.GetString(reader, 6);
        entities.ServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 8);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 9);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 10);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.LeaderServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 12);
        entities.LeaderOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 12);
        entities.LeaderServiceProvider.UserId = db.GetString(reader, 13);
        entities.LeaderServiceProvider.LastName = db.GetString(reader, 14);
        entities.LeaderServiceProvider.FirstName = db.GetString(reader, 15);
        entities.LeaderOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 16);
        entities.LeaderOfficeServiceProvider.RoleCode =
          db.GetString(reader, 17);
        entities.LeaderOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 18);
        entities.LeaderOfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 19);
        entities.LeaderOfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 20);
        entities.LeaderOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 21);
        entities.LeaderOfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 22);
        entities.LeaderOfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 23);
        entities.LeaderOfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 24);
        entities.LeaderOfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 25);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 26);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 27);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 28);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 29);
        entities.LeaderOfficeServiceProvider.Populated = true;
        entities.LeaderServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
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
    /// <summary>A SelectionGroup group.</summary>
    [Serializable]
    public class SelectionGroup
    {
      /// <summary>
      /// A value of Selection1.
      /// </summary>
      [JsonPropertyName("selection1")]
      public ServiceProvider Selection1
      {
        get => selection1 ??= new();
        set => selection1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private ServiceProvider selection1;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of HiddenOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenOfficeServiceProvider")]
      public OfficeServiceProvider HiddenOfficeServiceProvider
      {
        get => hiddenOfficeServiceProvider ??= new();
        set => hiddenOfficeServiceProvider = value;
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
      /// A value of HiddenOfficeServiceProvRelationship.
      /// </summary>
      [JsonPropertyName("hiddenOfficeServiceProvRelationship")]
      public OfficeServiceProvRelationship HiddenOfficeServiceProvRelationship
      {
        get => hiddenOfficeServiceProvRelationship ??= new();
        set => hiddenOfficeServiceProvRelationship = value;
      }

      /// <summary>
      /// A value of HiddenServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenServiceProvider")]
      public ServiceProvider HiddenServiceProvider
      {
        get => hiddenServiceProvider ??= new();
        set => hiddenServiceProvider = value;
      }

      /// <summary>
      /// A value of OfficeServiceProvRelationship.
      /// </summary>
      [JsonPropertyName("officeServiceProvRelationship")]
      public OfficeServiceProvRelationship OfficeServiceProvRelationship
      {
        get => officeServiceProvRelationship ??= new();
        set => officeServiceProvRelationship = value;
      }

      /// <summary>
      /// A value of ListSrvProvder.
      /// </summary>
      [JsonPropertyName("listSrvProvder")]
      public Common ListSrvProvder
      {
        get => listSrvProvder ??= new();
        set => listSrvProvder = value;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private OfficeServiceProvider hiddenOfficeServiceProvider;
      private OfficeServiceProvider officeServiceProvider;
      private OfficeServiceProvRelationship hiddenOfficeServiceProvRelationship;
      private ServiceProvider hiddenServiceProvider;
      private OfficeServiceProvRelationship officeServiceProvRelationship;
      private Common listSrvProvder;
      private Common common;
      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// Gets a value of Selection.
    /// </summary>
    [JsonIgnore]
    public Array<SelectionGroup> Selection => selection ??= new(
      SelectionGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Selection for json serialization.
    /// </summary>
    [JsonPropertyName("selection")]
    [Computed]
    public IList<SelectionGroup> Selection_Json
    {
      get => selection;
      set => Selection.Assign(value);
    }

    /// <summary>
    /// A value of SubHidden.
    /// </summary>
    [JsonPropertyName("subHidden")]
    public ServiceProvider SubHidden
    {
      get => subHidden ??= new();
      set => subHidden = value;
    }

    /// <summary>
    /// A value of SubOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("subOfficeServiceProvider")]
    public OfficeServiceProvider SubOfficeServiceProvider
    {
      get => subOfficeServiceProvider ??= new();
      set => subOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SubServiceProvider.
    /// </summary>
    [JsonPropertyName("subServiceProvider")]
    public ServiceProvider SubServiceProvider
    {
      get => subServiceProvider ??= new();
      set => subServiceProvider = value;
    }

    /// <summary>
    /// A value of LeaderOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderOfficeServiceProvider")]
    public OfficeServiceProvider LeaderOfficeServiceProvider
    {
      get => leaderOfficeServiceProvider ??= new();
      set => leaderOfficeServiceProvider = value;
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
    /// A value of HiddenSelectionDelete.
    /// </summary>
    [JsonPropertyName("hiddenSelectionDelete")]
    public Office HiddenSelectionDelete
    {
      get => hiddenSelectionDelete ??= new();
      set => hiddenSelectionDelete = value;
    }

    /// <summary>
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public ServiceProvider HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
    }

    /// <summary>
    /// A value of ListLeadingSrvProvder.
    /// </summary>
    [JsonPropertyName("listLeadingSrvProvder")]
    public Common ListLeadingSrvProvder
    {
      get => listLeadingSrvProvder ??= new();
      set => listLeadingSrvProvder = value;
    }

    /// <summary>
    /// A value of ListSub.
    /// </summary>
    [JsonPropertyName("listSub")]
    public Common ListSub
    {
      get => listSub ??= new();
      set => listSub = value;
    }

    /// <summary>
    /// A value of LeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderServiceProvider")]
    public ServiceProvider LeaderServiceProvider
    {
      get => leaderServiceProvider ??= new();
      set => leaderServiceProvider = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ServiceProvider Search
    {
      get => search ??= new();
      set => search = value;
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

    private Array<SelectionGroup> selection;
    private ServiceProvider subHidden;
    private OfficeServiceProvider subOfficeServiceProvider;
    private ServiceProvider subServiceProvider;
    private OfficeServiceProvider leaderOfficeServiceProvider;
    private CodeValue codeValue;
    private Office hiddenSelectionDelete;
    private ServiceProvider hiddenSelection;
    private Common listLeadingSrvProvder;
    private Common listSub;
    private ServiceProvider leaderServiceProvider;
    private ServiceProvider search;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of HiddenOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenOfficeServiceProvider")]
      public OfficeServiceProvider HiddenOfficeServiceProvider
      {
        get => hiddenOfficeServiceProvider ??= new();
        set => hiddenOfficeServiceProvider = value;
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
      /// A value of HiddenOfficeServiceProvRelationship.
      /// </summary>
      [JsonPropertyName("hiddenOfficeServiceProvRelationship")]
      public OfficeServiceProvRelationship HiddenOfficeServiceProvRelationship
      {
        get => hiddenOfficeServiceProvRelationship ??= new();
        set => hiddenOfficeServiceProvRelationship = value;
      }

      /// <summary>
      /// A value of HiddenServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenServiceProvider")]
      public ServiceProvider HiddenServiceProvider
      {
        get => hiddenServiceProvider ??= new();
        set => hiddenServiceProvider = value;
      }

      /// <summary>
      /// A value of OfficeServiceProvRelationship.
      /// </summary>
      [JsonPropertyName("officeServiceProvRelationship")]
      public OfficeServiceProvRelationship OfficeServiceProvRelationship
      {
        get => officeServiceProvRelationship ??= new();
        set => officeServiceProvRelationship = value;
      }

      /// <summary>
      /// A value of ListSrvProvder.
      /// </summary>
      [JsonPropertyName("listSrvProvder")]
      public Common ListSrvProvder
      {
        get => listSrvProvder ??= new();
        set => listSrvProvder = value;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private OfficeServiceProvider hiddenOfficeServiceProvider;
      private OfficeServiceProvider officeServiceProvider;
      private OfficeServiceProvRelationship hiddenOfficeServiceProvRelationship;
      private ServiceProvider hiddenServiceProvider;
      private OfficeServiceProvRelationship officeServiceProvRelationship;
      private Common listSrvProvder;
      private Common common;
      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// A value of MultipleSelect.
    /// </summary>
    [JsonPropertyName("multipleSelect")]
    public Common MultipleSelect
    {
      get => multipleSelect ??= new();
      set => multipleSelect = value;
    }

    /// <summary>
    /// A value of HiddenSelectSub.
    /// </summary>
    [JsonPropertyName("hiddenSelectSub")]
    public ServiceProvider HiddenSelectSub
    {
      get => hiddenSelectSub ??= new();
      set => hiddenSelectSub = value;
    }

    /// <summary>
    /// A value of SubHidden.
    /// </summary>
    [JsonPropertyName("subHidden")]
    public ServiceProvider SubHidden
    {
      get => subHidden ??= new();
      set => subHidden = value;
    }

    /// <summary>
    /// A value of SubOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("subOfficeServiceProvider")]
    public OfficeServiceProvider SubOfficeServiceProvider
    {
      get => subOfficeServiceProvider ??= new();
      set => subOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SubServiceProvider.
    /// </summary>
    [JsonPropertyName("subServiceProvider")]
    public ServiceProvider SubServiceProvider
    {
      get => subServiceProvider ??= new();
      set => subServiceProvider = value;
    }

    /// <summary>
    /// A value of LeaderOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderOfficeServiceProvider")]
    public OfficeServiceProvider LeaderOfficeServiceProvider
    {
      get => leaderOfficeServiceProvider ??= new();
      set => leaderOfficeServiceProvider = value;
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
    /// A value of HiddenSelectionDelet.
    /// </summary>
    [JsonPropertyName("hiddenSelectionDelet")]
    public Office HiddenSelectionDelet
    {
      get => hiddenSelectionDelet ??= new();
      set => hiddenSelectionDelet = value;
    }

    /// <summary>
    /// A value of LeaderHidden.
    /// </summary>
    [JsonPropertyName("leaderHidden")]
    public ServiceProvider LeaderHidden
    {
      get => leaderHidden ??= new();
      set => leaderHidden = value;
    }

    /// <summary>
    /// A value of ListLeadingSrvProvder.
    /// </summary>
    [JsonPropertyName("listLeadingSrvProvder")]
    public Common ListLeadingSrvProvder
    {
      get => listLeadingSrvProvder ??= new();
      set => listLeadingSrvProvder = value;
    }

    /// <summary>
    /// A value of ListSub.
    /// </summary>
    [JsonPropertyName("listSub")]
    public Common ListSub
    {
      get => listSub ??= new();
      set => listSub = value;
    }

    /// <summary>
    /// A value of LeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderServiceProvider")]
    public ServiceProvider LeaderServiceProvider
    {
      get => leaderServiceProvider ??= new();
      set => leaderServiceProvider = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ServiceProvider Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of HiddenSelectLeader.
    /// </summary>
    [JsonPropertyName("hiddenSelectLeader")]
    public ServiceProvider HiddenSelectLeader
    {
      get => hiddenSelectLeader ??= new();
      set => hiddenSelectLeader = value;
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

    private Common multipleSelect;
    private ServiceProvider hiddenSelectSub;
    private ServiceProvider subHidden;
    private OfficeServiceProvider subOfficeServiceProvider;
    private ServiceProvider subServiceProvider;
    private OfficeServiceProvider leaderOfficeServiceProvider;
    private CodeValue codeValue;
    private Office hiddenSelectionDelet;
    private ServiceProvider leaderHidden;
    private Common listLeadingSrvProvder;
    private Common listSub;
    private ServiceProvider leaderServiceProvider;
    private ServiceProvider search;
    private Array<ExportGroup> export1;
    private ServiceProvider hiddenSelectLeader;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RecordFound.
    /// </summary>
    [JsonPropertyName("recordFound")]
    public Common RecordFound
    {
      get => recordFound ??= new();
      set => recordFound = value;
    }

    /// <summary>
    /// A value of CompareRead.
    /// </summary>
    [JsonPropertyName("compareRead")]
    public ServiceProvider CompareRead
    {
      get => compareRead ??= new();
      set => compareRead = value;
    }

    /// <summary>
    /// A value of RecCount.
    /// </summary>
    [JsonPropertyName("recCount")]
    public Common RecCount
    {
      get => recCount ??= new();
      set => recCount = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public OfficeServiceProvider New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of Dtl.
    /// </summary>
    [JsonPropertyName("dtl")]
    public Common Dtl
    {
      get => dtl ??= new();
      set => dtl = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    private Common recordFound;
    private ServiceProvider compareRead;
    private Common recCount;
    private OfficeServiceProvider new1;
    private DateWorkArea current;
    private Common dtl;
    private Common count;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Leader2NdRead.
    /// </summary>
    [JsonPropertyName("leader2NdRead")]
    public OfficeServiceProvider Leader2NdRead
    {
      get => leader2NdRead ??= new();
      set => leader2NdRead = value;
    }

    /// <summary>
    /// A value of LeaderOffice.
    /// </summary>
    [JsonPropertyName("leaderOffice")]
    public Office LeaderOffice
    {
      get => leaderOffice ??= new();
      set => leaderOffice = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public OfficeServiceProvRelationship Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of PrevLeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("prevLeaderServiceProvider")]
    public ServiceProvider PrevLeaderServiceProvider
    {
      get => prevLeaderServiceProvider ??= new();
      set => prevLeaderServiceProvider = value;
    }

    /// <summary>
    /// A value of PrevLeaderOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("prevLeaderOfficeServiceProvider")]
    public OfficeServiceProvider PrevLeaderOfficeServiceProvider
    {
      get => prevLeaderOfficeServiceProvider ??= new();
      set => prevLeaderOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of LeaderOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderOfficeServiceProvider")]
    public OfficeServiceProvider LeaderOfficeServiceProvider
    {
      get => leaderOfficeServiceProvider ??= new();
      set => leaderOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of LeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderServiceProvider")]
    public ServiceProvider LeaderServiceProvider
    {
      get => leaderServiceProvider ??= new();
      set => leaderServiceProvider = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
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

    private OfficeServiceProvider leader2NdRead;
    private Office leaderOffice;
    private OfficeServiceProvRelationship prev;
    private ServiceProvider prevLeaderServiceProvider;
    private OfficeServiceProvider prevLeaderOfficeServiceProvider;
    private OfficeServiceProvider leaderOfficeServiceProvider;
    private ServiceProvider leaderServiceProvider;
    private Code code;
    private CodeValue codeValue;
    private OfficeServiceProvider officeServiceProvider;
    private OfficeAddress officeAddress;
    private Office office;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private ServiceProvider serviceProvider;
  }
#endregion
}
