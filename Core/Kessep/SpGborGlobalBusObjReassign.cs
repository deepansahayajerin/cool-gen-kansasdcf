// Program: SP_GBOR_GLOBAL_BUS_OBJ_REASSIGN, ID: 372452871, model: 746.
// Short name: SWEGBORP
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
/// A program: SP_GBOR_GLOBAL_BUS_OBJ_REASSIGN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpGborGlobalBusObjReassign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_GBOR_GLOBAL_BUS_OBJ_REASSIGN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpGborGlobalBusObjReassign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpGborGlobalBusObjReassign.
  /// </summary>
  public SpGborGlobalBusObjReassign(IContext context, Import import,
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
    // ***************************************************************************
    // **   DATE   Developer   WR/PR    DESCRIPTION
    // ** 01/26/98 J. Rookard           Initial development
    // ** 02/11/99 A. Massey            modifications per screen assessment
    // ** 01/09/01 SWSRCHF   I00110892  Remove the Role Code restriction for
    // **
    // 
    // business object LEA
    // ** 11/08/18 R.Mathews   CQ61772  Change GBOR to be able to select a 
    // caseload
    // **
    // 
    // type (CAS, LEA, PAR) rather
    // than each
    // **
    // 
    // individual business object
    // type.  Default
    // **
    // 
    // assignment reason code to RSP.
    // ***************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    export.ExistingOffice.SystemGeneratedId =
      import.ExistingOffice.SystemGeneratedId;
    export.ExistingServiceProvider.Assign(import.ExistingServiceProvider);
    export.ListOsp.Flag = import.ListOsp.Flag;
    MoveOfficeServiceProvider(import.ExistingOfficeServiceProvider,
      export.ExistingOfficeServiceProvider);

    if (Equal(global.Command, "RETSVPO"))
    {
      if (AsChar(export.ListOsp.Flag) == 'S')
      {
        if (!IsEmpty(import.HiddenSelectionOfficeServiceProvider.RoleCode))
        {
          export.ExistingOffice.SystemGeneratedId =
            import.HiddenOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(import.HiddenSelectionOfficeServiceProvider,
            export.ExistingOfficeServiceProvider);
          export.ExistingServiceProvider.Assign(
            import.HiddenSelectionServiceProvider);
        }

        export.ListOsp.Flag = "";
        global.Command = "DISPLAY";
      }
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      // move imports to exports
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
        export.Export1.Update.GlobalReassignment.Assign(
          import.Import1.Item.GlobalReassignment);
        export.Export1.Update.ServiceProvider.Assign(
          import.Import1.Item.ServiceProvider);
        MoveOfficeServiceProvider(import.Import1.Item.OfficeServiceProvider,
          export.Export1.Update.OfficeServiceProvider);
        export.Export1.Update.Office.SystemGeneratedId =
          import.Import1.Item.Office.SystemGeneratedId;
        export.Export1.Update.ListOsp.Flag = import.Import1.Item.ListOsp.Flag;
        export.Export1.Update.ListBoCode.Flag =
          import.Import1.Item.ListBoCode.Flag;
        export.Export1.Update.ListReaCode.Flag =
          import.Import1.Item.ListReaCode.Flag;

        if (Equal(global.Command, "RETCDVL"))
        {
          // Returning on a link from CDVL.
          if (AsChar(export.Export1.Item.ListBoCode.Flag) == 'S')
          {
            if (!IsEmpty(import.HiddenSelectionCodeValue.Cdvalue))
            {
              export.Export1.Update.GlobalReassignment.BusinessObjectCode =
                import.HiddenSelectionCodeValue.Cdvalue;
            }

            var field = GetField(export.Export1.Item.ListBoCode, "flag");

            field.Protected = false;
            field.Focused = true;
          }

          if (AsChar(export.Export1.Item.ListReaCode.Flag) == 'S')
          {
            if (!IsEmpty(import.HiddenSelectionCodeValue.Cdvalue))
            {
              export.Export1.Update.GlobalReassignment.AssignmentReasonCode =
                import.HiddenSelectionCodeValue.Cdvalue;
            }

            var field = GetField(export.Export1.Item.ListReaCode, "flag");

            field.Protected = false;
            field.Focused = true;
          }

          export.Export1.Update.ListReaCode.Flag = "";
          export.Export1.Update.ListBoCode.Flag = "";
        }

        if (Equal(global.Command, "RETSVPO"))
        {
          // Returning on a link from SVPO to the export group.
          if (!IsEmpty(import.HiddenSelectionOfficeServiceProvider.RoleCode) &&
            AsChar(export.Export1.Item.ListOsp.Flag) == 'S')
          {
            MoveOfficeServiceProvider(import.
              HiddenSelectionOfficeServiceProvider,
              export.Export1.Update.OfficeServiceProvider);
            export.Export1.Update.ServiceProvider.Assign(
              import.HiddenSelectionServiceProvider);
            export.Export1.Update.Office.SystemGeneratedId =
              import.HiddenOffice.SystemGeneratedId;
          }

          export.Export1.Update.ListOsp.Flag = "";

          var field = GetField(export.Export1.Item.ListOsp, "flag");

          field.Protected = false;
          field.Focused = true;
        }

        export.Export1.Next();
      }
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        UseScCabNextTranPut();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }

        return;
      }

      ExitState = "ACO_NE0000_INVALID_COMMAND";
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // this is where you set your export value to the export hidden next tran 
      // values if the user is coming into this procedure on a next tran action.
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the properties for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    if (Equal(global.Command, "RETCDVL") || Equal(global.Command, "RETSVPO"))
    {
      // Returning on a link from either SVPO or CDVL.
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

    if (Equal(global.Command, "LIST"))
    {
      if (AsChar(export.ListOsp.Flag) == 'S')
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' || !
            IsEmpty(export.Export1.Item.ListBoCode.Flag) || !
            IsEmpty(export.Export1.Item.ListOsp.Flag) || !
            IsEmpty(export.Export1.Item.ListReaCode.Flag))
          {
            if (!IsEmpty(export.Export1.Item.Common.SelectChar))
            {
              var field1 = GetField(export.Export1.Item.Common, "selectChar");

              field1.Error = true;
            }

            if (!IsEmpty(export.Export1.Item.ListBoCode.Flag))
            {
              var field1 = GetField(export.Export1.Item.ListBoCode, "flag");

              field1.Error = true;
            }

            if (!IsEmpty(export.Export1.Item.ListOsp.Flag))
            {
              var field1 = GetField(export.Export1.Item.ListOsp, "flag");

              field1.Error = true;
            }

            if (!IsEmpty(export.Export1.Item.ListReaCode.Flag))
            {
              var field1 = GetField(export.Export1.Item.ListReaCode, "flag");

              field1.Error = true;
            }

            var field = GetField(export.ListOsp, "flag");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
          }
        }
      }

      if (AsChar(export.ListOsp.Flag) == 'S')
      {
        ExitState = "ECO_LNK_TO_OFFICE_SERVICE_PROVDR";

        return;
      }

      if (!IsEmpty(export.ListOsp.Flag))
      {
        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        var field = GetField(export.ListOsp, "flag");

        field.Error = true;

        return;
      }

      local.Count.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          ++local.Count.Count;

          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Protected = false;
          field.Focused = true;

          if (AsChar(export.ListOsp.Flag) == 'S')
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
          }

          if (AsChar(export.Export1.Item.ListBoCode.Flag) == 'S' && AsChar
            (export.Export1.Item.ListReaCode.Flag) == 'S')
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            var field1 = GetField(export.Export1.Item.ListBoCode, "flag");

            field1.Error = true;

            var field2 = GetField(export.Export1.Item.ListReaCode, "flag");

            field2.Error = true;

            if (AsChar(export.Export1.Item.ListOsp.Flag) == 'S')
            {
              var field3 = GetField(export.Export1.Item.ListOsp, "flag");

              field3.Error = true;
            }

            return;
          }

          if (AsChar(export.Export1.Item.ListReaCode.Flag) == 'S' && AsChar
            (export.Export1.Item.ListOsp.Flag) == 'S')
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            var field1 = GetField(export.Export1.Item.ListOsp, "flag");

            field1.Error = true;

            var field2 = GetField(export.Export1.Item.ListReaCode, "flag");

            field2.Error = true;

            if (AsChar(export.Export1.Item.ListBoCode.Flag) == 'S')
            {
              var field3 = GetField(export.Export1.Item.ListBoCode, "flag");

              field3.Error = true;
            }

            return;
          }

          if (AsChar(export.Export1.Item.ListBoCode.Flag) == 'S' && AsChar
            (export.Export1.Item.ListOsp.Flag) == 'S')
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            var field1 = GetField(export.Export1.Item.ListOsp, "flag");

            field1.Error = true;

            var field2 = GetField(export.Export1.Item.ListBoCode, "flag");

            field2.Error = true;

            if (AsChar(export.Export1.Item.ListReaCode.Flag) == 'S')
            {
              var field3 = GetField(export.Export1.Item.ListReaCode, "flag");

              field3.Error = true;
            }

            return;
          }
        }
      }

      if (local.Count.Count == 1)
      {
        goto Test1;
      }

      if (local.Count.Count < 1)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      if (local.Count.Count > 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }
    }

Test1:

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE"))
    {
      local.Count.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          ++local.Count.Count;
        }
      }

      if (local.Count.Count < 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Color = "green";
          field.Protected = false;
          field.Focused = true;
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "LIST"))
    {
      // Check to see if a selection has been made.
      local.Count.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.Common.SelectChar) || !
          IsEmpty(export.Export1.Item.ListBoCode.Flag) || !
          IsEmpty(export.Export1.Item.ListReaCode.Flag) || !
          IsEmpty(export.Export1.Item.ListOsp.Flag))
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' || AsChar
            (export.Export1.Item.ListBoCode.Flag) == 'S' || AsChar
            (export.Export1.Item.ListReaCode.Flag) == 'S' || AsChar
            (export.Export1.Item.ListOsp.Flag) == 'S')
          {
            if (Equal(global.Command, "ADD"))
            {
              if (AsChar(export.Export1.Item.GlobalReassignment.StatusFlag) == 'P'
                )
              {
                var field = GetField(export.Export1.Item.Common, "selectChar");

                field.Color = "red";
                field.Protected = false;
                field.Focused = true;

                ExitState = "ACO_NE0000_INVALID_ACTION";

                return;
              }

              if (IsEmpty(export.ExistingOfficeServiceProvider.RoleCode))
              {
                var field1 =
                  GetField(export.ExistingOfficeServiceProvider, "roleCode");

                field1.Error = true;

                var field2 =
                  GetField(export.ExistingOfficeServiceProvider, "effectiveDate");
                  

                field2.Error = true;

                export.ListOsp.Flag = "S";

                var field3 = GetField(export.ListOsp, "flag");

                field3.Protected = false;
                field3.Focused = true;

                ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

                return;
              }

              if (IsEmpty(export.Export1.Item.GlobalReassignment.
                BusinessObjectCode))
              {
                var field =
                  GetField(export.Export1.Item.GlobalReassignment,
                  "businessObjectCode");

                field.Error = true;

                ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

                return;
              }
              else
              {
                // validate the business object code
                export.CodeValue.Cdvalue =
                  export.Export1.Item.GlobalReassignment.BusinessObjectCode;
                export.Code.CodeName = "GBOR REASSIGNABLE BUS OBJECTS";
                UseCabValidateCodeValue();

                switch(local.ReturnCode.Count)
                {
                  case 0:
                    // Input code value valid.
                    break;
                  case 1:
                    // Invalid code name.
                    ExitState = "CO0000_INVALID_CODE";

                    return;
                  case 2:
                    // Invalid code value
                    ExitState = "ACO_NE0000_INVALID_CODE";

                    var field1 =
                      GetField(export.Export1.Item.GlobalReassignment,
                      "businessObjectCode");

                    field1.Error = true;

                    export.Export1.Update.ListBoCode.Flag = "S";

                    var field2 =
                      GetField(export.Export1.Item.ListBoCode, "flag");

                    field2.Protected = false;
                    field2.Focused = true;

                    return;
                  default:
                    break;
                }
              }

              // CQ61772 disabled assignment reason code edit and default to '
              // RSP'
              // for all business objects
              export.Export1.Update.GlobalReassignment.AssignmentReasonCode =
                "RSP";

              if (IsEmpty(export.Export1.Item.GlobalReassignment.OverrideFlag) ||
                AsChar(export.Export1.Item.GlobalReassignment.OverrideFlag) == 'Y'
                || AsChar
                (export.Export1.Item.GlobalReassignment.OverrideFlag) == 'N')
              {
              }
              else
              {
                var field =
                  GetField(export.Export1.Item.GlobalReassignment,
                  "overrideFlag");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                return;
              }

              if (IsEmpty(export.Export1.Item.GlobalReassignment.OverrideFlag))
              {
                export.Export1.Update.GlobalReassignment.OverrideFlag = "N";
              }

              if (Equal(export.Export1.Item.GlobalReassignment.ProcessDate,
                local.InitializedDateWorkArea.Date))
              {
                export.Export1.Update.GlobalReassignment.ProcessDate =
                  local.Current.Date;
              }

              if (Lt(export.Export1.Item.GlobalReassignment.ProcessDate,
                local.Current.Date))
              {
                var field =
                  GetField(export.Export1.Item.GlobalReassignment, "processDate");
                  

                field.Color = "red";
                field.Protected = false;
                field.Focused = true;

                ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

                return;
              }

              if (IsEmpty(export.Export1.Item.OfficeServiceProvider.RoleCode))
              {
                var field = GetField(export.Export1.Item.ListOsp, "flag");

                field.Color = "red";
                field.Protected = false;
                field.Focused = true;

                ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

                return;
              }

              if (ReadServiceProvider3())
              {
                if (ReadOffice2())
                {
                  export.Export1.Update.Office.SystemGeneratedId =
                    entities.NewOffice.SystemGeneratedId;

                  if (ReadOfficeServiceProvider2())
                  {
                    if (!Lt(local.Current.Date,
                      entities.NewOfficeServiceProvider.EffectiveDate) && !
                      Lt(entities.NewOfficeServiceProvider.DiscontinueDate,
                      local.Current.Date))
                    {
                    }
                    else
                    {
                      ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

                      var field1 =
                        GetField(export.Export1.Item.Common, "selectChar");

                      field1.Error = true;

                      var field2 =
                        GetField(export.Export1.Item.OfficeServiceProvider,
                        "roleCode");

                      field2.Error = true;

                      return;
                    }

                    MoveOfficeServiceProvider(entities.NewOfficeServiceProvider,
                      export.Export1.Update.OfficeServiceProvider);
                  }
                  else
                  {
                    var field1 =
                      GetField(export.Export1.Item.OfficeServiceProvider,
                      "effectiveDate");

                    field1.Error = true;

                    var field2 =
                      GetField(export.Export1.Item.OfficeServiceProvider,
                      "roleCode");

                    field2.Error = true;

                    ExitState = "OFFICE_SERVICE_PROVIDER_NF";

                    return;
                  }
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.Office, "systemGeneratedId");

                  field.Error = true;

                  ExitState = "OFFICE_NF";

                  return;
                }
              }
              else
              {
                var field =
                  GetField(export.Export1.Item.ServiceProvider,
                  "systemGeneratedId");

                field.Error = true;

                ExitState = "SERVICE_PROVIDER_NF";

                return;
              }

              if (export.Export1.Item.Office.SystemGeneratedId == export
                .ExistingOffice.SystemGeneratedId && export
                .Export1.Item.ServiceProvider.SystemGeneratedId == export
                .ExistingServiceProvider.SystemGeneratedId && Equal
                (export.Export1.Item.OfficeServiceProvider.RoleCode,
                export.ExistingOfficeServiceProvider.RoleCode) && Equal
                (export.Export1.Item.OfficeServiceProvider.EffectiveDate,
                export.ExistingOfficeServiceProvider.EffectiveDate))
              {
                var field1 =
                  GetField(export.ExistingOfficeServiceProvider, "roleCode");

                field1.Error = true;

                var field2 =
                  GetField(export.ExistingServiceProvider, "systemGeneratedId");
                  

                field2.Error = true;

                var field3 =
                  GetField(export.ExistingOffice, "systemGeneratedId");

                field3.Error = true;

                var field4 =
                  GetField(export.Export1.Item.Office, "systemGeneratedId");

                field4.Error = true;

                var field5 =
                  GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");
                  

                field5.Error = true;

                var field6 =
                  GetField(export.Export1.Item.ServiceProvider,
                  "systemGeneratedId");

                field6.Error = true;

                export.Export1.Update.ListOsp.Flag = "S";

                var field7 = GetField(export.Export1.Item.ListOsp, "flag");

                field7.Protected = false;
                field7.Focused = true;

                ExitState = "SP0000_DUPLICATE_OSP_NOT_ALLOWED";

                return;
              }

              // *** Problem report I00110892
              // *** 01/09/01 swsrchf
              // ***
              // *** CQ61772 LRF requires attorney role, but is processed under
              // 'LEA'
              // *** business object now.  Following edit changed from 'LRF' to
              // 'LEA'.
              // *** start
              if (Equal(export.Export1.Item.GlobalReassignment.
                BusinessObjectCode, "LEA"))
              {
                // *** end
                // *** 01/09/01 swsrchf
                // *** Problem report I00110892
                // For Legal Referral or Legal Action reassignment, validate the
                // Office Service Provider Role Code.   Must be a valid
                // attorney role code to be assigned to a Legal Referral/Action.
                export.CodeValue.Cdvalue =
                  export.Export1.Item.OfficeServiceProvider.RoleCode;
                export.Code.CodeName = "ATTORNEY ROLE CODES";
                UseCabValidateCodeValue();

                switch(local.ReturnCode.Count)
                {
                  case 0:
                    // Input code value valid.
                    break;
                  case 1:
                    // Invalid code name.
                    ExitState = "CO0000_INVALID_CODE";

                    return;
                  case 2:
                    // Invalid code value
                    ExitState = "INVALID_COMBINATION";

                    var field1 =
                      GetField(export.Export1.Item.OfficeServiceProvider,
                      "roleCode");

                    field1.Error = true;

                    var field2 =
                      GetField(export.Export1.Item.GlobalReassignment,
                      "businessObjectCode");

                    field2.Error = true;

                    export.Export1.Update.ListOsp.Flag = "S";

                    var field3 = GetField(export.Export1.Item.ListOsp, "flag");

                    field3.Protected = false;
                    field3.Focused = true;

                    return;
                  default:
                    break;
                }
              }
            }

            switch(TrimEnd(global.Command))
            {
              case "LIST":
                if (AsChar(export.Export1.Item.ListBoCode.Flag) != 'S' && AsChar
                  (export.Export1.Item.ListReaCode.Flag) != 'S' && AsChar
                  (export.Export1.Item.ListOsp.Flag) != 'S')
                {
                  continue;
                }
                else
                {
                  if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' && AsChar
                    (export.Export1.Item.ListOsp.Flag) == 'S')
                  {
                    ++local.Count.Count;
                  }

                  if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' && AsChar
                    (export.Export1.Item.ListBoCode.Flag) == 'S')
                  {
                    ++local.Count.Count;
                  }

                  if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' && AsChar
                    (export.Export1.Item.ListReaCode.Flag) == 'S')
                  {
                    ++local.Count.Count;
                  }

                  if (local.Count.Count > 1)
                  {
                    if (!IsEmpty(export.Export1.Item.ListBoCode.Flag))
                    {
                      var field =
                        GetField(export.Export1.Item.ListBoCode, "flag");

                      field.Error = true;
                    }

                    if (!IsEmpty(export.Export1.Item.ListOsp.Flag))
                    {
                      var field = GetField(export.Export1.Item.ListOsp, "flag");

                      field.Error = true;
                    }

                    if (!IsEmpty(export.Export1.Item.ListReaCode.Flag))
                    {
                      var field =
                        GetField(export.Export1.Item.ListReaCode, "flag");

                      field.Error = true;
                    }

                    if (!IsEmpty(export.Export1.Item.Common.SelectChar))
                    {
                      var field =
                        GetField(export.Export1.Item.Common, "selectChar");

                      field.Error = true;
                    }

                    ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

                    return;
                  }

                  if (local.Count.Count == 0)
                  {
                    ExitState = "ACO_NE0000_NO_SELECTION_MADE";

                    return;
                  }

                  if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' && AsChar
                    (export.Export1.Item.ListReaCode.Flag) == 'S' && IsEmpty
                    (export.Export1.Item.GlobalReassignment.BusinessObjectCode))
                  {
                    var field1 =
                      GetField(export.Export1.Item.GlobalReassignment,
                      "businessObjectCode");

                    field1.Error = true;

                    var field2 =
                      GetField(export.Export1.Item.ListBoCode, "flag");

                    field2.Protected = false;
                    field2.Focused = true;

                    ExitState = "ACO_NE0000_INVALID_ACTION";

                    return;
                  }
                }

                if (AsChar(export.Export1.Item.ListOsp.Flag) == 'S')
                {
                  ExitState = "ECO_LNK_TO_OFFICE_SERVICE_PROVDR";

                  return;
                }
                else if (!IsEmpty(export.Export1.Item.ListOsp.Flag))
                {
                  var field = GetField(export.Export1.Item.ListOsp, "flag");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
                }

                if (AsChar(export.Export1.Item.ListBoCode.Flag) == 'S')
                {
                  export.Code.CodeName = "GBOR REASSIGNABLE BUS OBJECTS";
                  ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                  return;
                }
                else if (!IsEmpty(export.Export1.Item.ListBoCode.Flag))
                {
                  var field = GetField(export.Export1.Item.ListBoCode, "flag");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
                }

                if (AsChar(export.Export1.Item.ListReaCode.Flag) == 'S')
                {
                  if (Equal(export.Export1.Item.GlobalReassignment.
                    BusinessObjectCode, "LEA"))
                  {
                    export.Code.CodeName = "LEGAL ASSIGNMENT REASON CODE";
                  }
                  else
                  {
                    export.Code.CodeName = "ASSIGNMENT REASON CODE";
                  }

                  ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                  return;
                }
                else if (!IsEmpty(export.Export1.Item.ListReaCode.Flag))
                {
                  var field = GetField(export.Export1.Item.ListReaCode, "flag");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
                }

                break;
              case "ADD":
                if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
                {
                  ++local.Count.Count;
                }

                if (local.Count.Count == 0)
                {
                  ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                  return;
                }

                UseSpCreateGlobalReassignment();

                if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                  ("ZD_ACO_NI0000_SUCCESSFUL_ADD_2"))
                {
                  ExitState = "ZD_ACO_NI0000_SUCCESSFUL_ADD_2";
                }
                else
                {
                  return;
                }

                break;
              case "DELETE":
                if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
                {
                  ++local.Count.Count;
                }

                if (local.Count.Count == 0)
                {
                  ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                  return;
                }

                UseSpDeleteGlobalReassignment2();

                if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                  ("ZD_ACO_NI0000_SUCCESSFUL_DEL_2"))
                {
                  ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_2";
                }
                else
                {
                  return;
                }

                break;
              default:
                break;
            }

            if (!Equal(global.Command, "LIST"))
            {
              if (!IsExitState("ZD_ACO_NI0000_SUCCESSFUL_ADD_2") && !
                IsExitState("ZD_ACO_NI0000_SUCCESSFUL_DEL_2") && !
                IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field = GetField(export.Export1.Item.Common, "selectChar");

                field.Error = true;

                return;
              }
              else
              {
                export.Export1.Update.Common.SelectChar = "*";
              }
            }
          }
          else if (AsChar(export.Export1.Item.Common.SelectChar) == '*')
          {
            export.Export1.Update.Common.SelectChar = "";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;

            goto Test2;
          }
        }

        // ê...End of IF grp_export ief_supplied sel_char <> spaces....and so 
        // on....nest
      }

      // ê...End of FOR EACH group_export nest
    }

Test2:

    // ê...End of IF Command equal Add, Delete, List nest
    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "RETSVPO":
        break;
      case "RETCDVL":
        break;
      case "ADD":
        break;
      case "DELETE":
        break;
      case "LIST":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        // Perform user transparent table maintenance.  Delete all occurrences 
        // of Global Reasssignment that have processed successfully and are more
        // than 179 days old.
        foreach(var item in ReadGlobalReassignment1())
        {
          UseSpDeleteGlobalReassignment1();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            ExitState = "SP0000_GLOBAL_REASSIGN_DEL_ERR";

            return;
          }
        }

        // Initialize the group view.
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Update.GlobalReassignment.Assign(
            local.InitializedGlobalReassignment);
          MoveOfficeServiceProvider(local.InitializedOfficeServiceProvider,
            export.Export1.Update.OfficeServiceProvider);
          export.Export1.Update.ServiceProvider.SystemGeneratedId =
            local.InitializedServiceProvider.SystemGeneratedId;
          export.Export1.Next();
        }

        // If the export existing OSP role code is not populated, determine the 
        // logged on user
        // and set the export existing OSP info to that user.
        if (IsEmpty(export.ExistingOfficeServiceProvider.RoleCode))
        {
          if (ReadServiceProvider2())
          {
            export.ExistingServiceProvider.Assign(
              entities.ExistingServiceProvider);
            local.CsePersonsWorkSet.LastName =
              entities.ExistingServiceProvider.LastName;
            local.CsePersonsWorkSet.FirstName =
              entities.ExistingServiceProvider.FirstName;
            local.CsePersonsWorkSet.MiddleInitial =
              entities.ExistingServiceProvider.MiddleInitial;
            UseSiFormatCsePersonName();
            export.ExistingServiceProvider.LastName =
              local.CsePersonsWorkSet.FormattedName;
            local.CsePersonsWorkSet.FirstName = "";
            local.CsePersonsWorkSet.LastName = "";
            local.CsePersonsWorkSet.MiddleInitial = "";
            local.CsePersonsWorkSet.FormattedName = "";

            if (ReadOfficeOfficeServiceProvider())
            {
              export.ExistingOffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
                export.ExistingOfficeServiceProvider);
              ++local.OspCount.Count;
            }

            if (local.OspCount.Count != 1)
            {
              ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

              return;
            }
          }
          else
          {
            var field1 =
              GetField(export.ExistingServiceProvider, "systemGeneratedId");

            field1.Error = true;

            ExitState = "SERVICE_PROVIDER_NF";

            return;
          }
        }
        else
        {
          if (ReadOffice1())
          {
            // Currency on Office acquired.
            export.ExistingOffice.SystemGeneratedId =
              entities.ExistingOffice.SystemGeneratedId;
          }
          else
          {
            var field1 = GetField(export.ExistingOffice, "systemGeneratedId");

            field1.Error = true;

            ExitState = "OFFICE_NF";

            return;
          }

          if (ReadServiceProvider1())
          {
            // Currency on Service Provider acquired.
            export.ExistingServiceProvider.Assign(
              entities.ExistingServiceProvider);
            local.CsePersonsWorkSet.LastName =
              entities.ExistingServiceProvider.LastName;
            local.CsePersonsWorkSet.FirstName =
              entities.ExistingServiceProvider.FirstName;
            local.CsePersonsWorkSet.MiddleInitial =
              entities.ExistingServiceProvider.MiddleInitial;
            UseSiFormatCsePersonName();
            export.ExistingServiceProvider.LastName =
              local.CsePersonsWorkSet.FormattedName;
            local.CsePersonsWorkSet.FirstName = "";
            local.CsePersonsWorkSet.LastName = "";
            local.CsePersonsWorkSet.MiddleInitial = "";
            local.CsePersonsWorkSet.FormattedName = "";
          }
          else
          {
            var field1 =
              GetField(export.ExistingServiceProvider, "systemGeneratedId");

            field1.Error = true;

            ExitState = "SERVICE_PROVIDER_NF";

            return;
          }

          if (ReadOfficeServiceProvider1())
          {
            // Currency on Office Service Provider acquired.
            MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
              export.ExistingOfficeServiceProvider);
          }
          else
          {
            var field1 =
              GetField(export.ExistingOfficeServiceProvider, "effectiveDate");

            field1.Error = true;

            var field2 =
              GetField(export.ExistingOfficeServiceProvider, "roleCode");

            field2.Error = true;

            ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

            return;
          }
        }

        // Populate group view.
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadGlobalReassignment2())
        {
          MoveGlobalReassignment(entities.GlobalReassignment,
            export.Export1.Update.GlobalReassignment);

          if (ReadOfficeServiceProvider3())
          {
            MoveOfficeServiceProvider(entities.NewOfficeServiceProvider,
              export.Export1.Update.OfficeServiceProvider);
          }
          else
          {
            var field1 =
              GetField(export.ExistingOfficeServiceProvider, "roleCode");

            field1.Error = true;

            ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";
            export.Export1.Next();

            return;
          }

          if (ReadServiceProvider4())
          {
            export.Export1.Update.ServiceProvider.Assign(
              entities.NewServiceProvider);
            local.CsePersonsWorkSet.FirstName =
              entities.NewServiceProvider.FirstName;
            local.CsePersonsWorkSet.LastName =
              entities.NewServiceProvider.LastName;
            local.CsePersonsWorkSet.MiddleInitial =
              entities.NewServiceProvider.MiddleInitial;
            UseSiFormatCsePersonName();
            export.Export1.Update.ServiceProvider.LastName =
              local.CsePersonsWorkSet.FormattedName;
            local.CsePersonsWorkSet.FirstName = "";
            local.CsePersonsWorkSet.LastName = "";
            local.CsePersonsWorkSet.MiddleInitial = "";
            local.CsePersonsWorkSet.FormattedName = "";
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");
              

            field1.Error = true;

            ExitState = "SERVICE_PROVIDER_NF";
            export.Export1.Next();

            return;
          }

          if (ReadOffice3())
          {
            export.Export1.Update.Office.SystemGeneratedId =
              entities.NewOffice.SystemGeneratedId;
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.Office, "systemGeneratedId");

            field1.Error = true;

            ExitState = "OFFICE_NF";
            export.Export1.Next();

            return;
          }

          export.Export1.Update.Common.SelectChar = "";
          export.Export1.Update.ListBoCode.Flag = "";
          export.Export1.Update.ListOsp.Flag = "";
          export.Export1.Update.ListReaCode.Flag = "";
          export.Export1.Next();
        }

        var field = GetField(export.ListOsp, "flag");

        field.Protected = false;
        field.Focused = true;

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

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

  private static void MoveGlobalReassignment(GlobalReassignment source,
    GlobalReassignment target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ProcessDate = source.ProcessDate;
    target.StatusFlag = source.StatusFlag;
    target.OverrideFlag = source.OverrideFlag;
    target.BusinessObjectCode = source.BusinessObjectCode;
    target.AssignmentReasonCode = source.AssignmentReasonCode;
    target.BoCount = source.BoCount;
    target.MonCount = source.MonCount;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = export.Code.CodeName;
    useImport.CodeValue.Cdvalue = export.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
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

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSpCreateGlobalReassignment()
  {
    var useImport = new SpCreateGlobalReassignment.Import();
    var useExport = new SpCreateGlobalReassignment.Export();

    MoveOfficeServiceProvider(export.Export1.Item.OfficeServiceProvider,
      useImport.NewOfficeServiceProvider);
    useImport.NewServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;
    useImport.NewOffice.SystemGeneratedId =
      export.Export1.Item.Office.SystemGeneratedId;
    useImport.ExistingOffice.SystemGeneratedId =
      export.ExistingOffice.SystemGeneratedId;
    useImport.ExistingServiceProvider.SystemGeneratedId =
      export.ExistingServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(export.ExistingOfficeServiceProvider,
      useImport.ExistingOfficeServiceProvider);
    useImport.GlobalReassignment.Assign(export.Export1.Item.GlobalReassignment);

    Call(SpCreateGlobalReassignment.Execute, useImport, useExport);

    export.Export1.Update.Office.SystemGeneratedId =
      useExport.NewOffice.SystemGeneratedId;
    export.Export1.Update.ServiceProvider.SystemGeneratedId =
      useExport.NewServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(useExport.NewOfficeServiceProvider,
      export.Export1.Update.OfficeServiceProvider);
    export.Export1.Update.GlobalReassignment.
      Assign(useExport.GlobalReassignment);
  }

  private void UseSpDeleteGlobalReassignment1()
  {
    var useImport = new SpDeleteGlobalReassignment.Import();
    var useExport = new SpDeleteGlobalReassignment.Export();

    MoveGlobalReassignment(entities.GlobalReassignment,
      useImport.GlobalReassignment);

    Call(SpDeleteGlobalReassignment.Execute, useImport, useExport);
  }

  private void UseSpDeleteGlobalReassignment2()
  {
    var useImport = new SpDeleteGlobalReassignment.Import();
    var useExport = new SpDeleteGlobalReassignment.Export();

    useImport.GlobalReassignment.Assign(export.Export1.Item.GlobalReassignment);

    Call(SpDeleteGlobalReassignment.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadGlobalReassignment1()
  {
    entities.GlobalReassignment.Populated = false;

    return ReadEach("ReadGlobalReassignment1",
      (db, command) =>
      {
        db.SetDate(command, "date", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GlobalReassignment.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.GlobalReassignment.CreatedBy = db.GetString(reader, 1);
        entities.GlobalReassignment.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.GlobalReassignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.GlobalReassignment.ProcessDate = db.GetDate(reader, 4);
        entities.GlobalReassignment.StatusFlag = db.GetString(reader, 5);
        entities.GlobalReassignment.OverrideFlag = db.GetString(reader, 6);
        entities.GlobalReassignment.BusinessObjectCode =
          db.GetString(reader, 7);
        entities.GlobalReassignment.AssignmentReasonCode =
          db.GetString(reader, 8);
        entities.GlobalReassignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.GlobalReassignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.GlobalReassignment.OspRoleCode =
          db.GetNullableString(reader, 11);
        entities.GlobalReassignment.OspEffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.GlobalReassignment.SpdGeneratedId1 =
          db.GetNullableInt32(reader, 13);
        entities.GlobalReassignment.OffGeneratedId1 =
          db.GetNullableInt32(reader, 14);
        entities.GlobalReassignment.OspRoleCod =
          db.GetNullableString(reader, 15);
        entities.GlobalReassignment.OspEffectiveDat =
          db.GetNullableDate(reader, 16);
        entities.GlobalReassignment.BoCount = db.GetNullableInt32(reader, 17);
        entities.GlobalReassignment.MonCount = db.GetNullableInt32(reader, 18);
        entities.GlobalReassignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadGlobalReassignment2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    return ReadEach("ReadGlobalReassignment2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "ospEffectiveDat",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetNullableString(
          command, "ospRoleCod",
          entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetNullableInt32(
          command, "offGeneratedId1",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId1",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.GlobalReassignment.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.GlobalReassignment.CreatedBy = db.GetString(reader, 1);
        entities.GlobalReassignment.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.GlobalReassignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.GlobalReassignment.ProcessDate = db.GetDate(reader, 4);
        entities.GlobalReassignment.StatusFlag = db.GetString(reader, 5);
        entities.GlobalReassignment.OverrideFlag = db.GetString(reader, 6);
        entities.GlobalReassignment.BusinessObjectCode =
          db.GetString(reader, 7);
        entities.GlobalReassignment.AssignmentReasonCode =
          db.GetString(reader, 8);
        entities.GlobalReassignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.GlobalReassignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.GlobalReassignment.OspRoleCode =
          db.GetNullableString(reader, 11);
        entities.GlobalReassignment.OspEffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.GlobalReassignment.SpdGeneratedId1 =
          db.GetNullableInt32(reader, 13);
        entities.GlobalReassignment.OffGeneratedId1 =
          db.GetNullableInt32(reader, 14);
        entities.GlobalReassignment.OspRoleCod =
          db.GetNullableString(reader, 15);
        entities.GlobalReassignment.OspEffectiveDat =
          db.GetNullableDate(reader, 16);
        entities.GlobalReassignment.BoCount = db.GetNullableInt32(reader, 17);
        entities.GlobalReassignment.MonCount = db.GetNullableInt32(reader, 18);
        entities.GlobalReassignment.Populated = true;

        return true;
      });
  }

  private bool ReadOffice1()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.
          SetInt32(command, "officeId", export.ExistingOffice.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadOffice2()
  {
    entities.NewOffice.Populated = false;

    return Read("ReadOffice2",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", export.Export1.Item.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.NewOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.NewOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.NewOffice.Populated = true;
      });
  }

  private bool ReadOffice3()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    entities.NewOffice.Populated = false;

    return Read("ReadOffice3",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId",
          entities.NewOfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.NewOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.NewOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.NewOffice.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProvider()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadOfficeOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 2);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 3);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 4);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetString(
          command, "roleCode", export.ExistingOfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "effectiveDate",
          export.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider2()
  {
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.NewServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "offGeneratedId", entities.NewOffice.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          export.Export1.Item.OfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode",
          export.Export1.Item.OfficeServiceProvider.RoleCode);
      },
      (db, reader) =>
      {
        entities.NewOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.NewOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.NewOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.NewOfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.NewOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.NewOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider3()
  {
    System.Diagnostics.Debug.Assert(entities.GlobalReassignment.Populated);
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.GlobalReassignment.OspEffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.GlobalReassignment.OspRoleCode ?? "");
        db.SetInt32(
          command, "offGeneratedId",
          entities.GlobalReassignment.OffGeneratedId.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.GlobalReassignment.SpdGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NewOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.NewOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.NewOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.NewOfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.NewOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.NewOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          export.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider3()
  {
    entities.NewServiceProvider.Populated = false;

    return Read("ReadServiceProvider3",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          export.Export1.Item.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.NewServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.NewServiceProvider.UserId = db.GetString(reader, 1);
        entities.NewServiceProvider.LastName = db.GetString(reader, 2);
        entities.NewServiceProvider.FirstName = db.GetString(reader, 3);
        entities.NewServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.NewServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider4()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    entities.NewServiceProvider.Populated = false;

    return Read("ReadServiceProvider4",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.NewOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.NewServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.NewServiceProvider.UserId = db.GetString(reader, 1);
        entities.NewServiceProvider.LastName = db.GetString(reader, 2);
        entities.NewServiceProvider.FirstName = db.GetString(reader, 3);
        entities.NewServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.NewServiceProvider.Populated = true;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of GlobalReassignment.
      /// </summary>
      [JsonPropertyName("globalReassignment")]
      public GlobalReassignment GlobalReassignment
      {
        get => globalReassignment ??= new();
        set => globalReassignment = value;
      }

      /// <summary>
      /// A value of ListReaCode.
      /// </summary>
      [JsonPropertyName("listReaCode")]
      public Common ListReaCode
      {
        get => listReaCode ??= new();
        set => listReaCode = value;
      }

      /// <summary>
      /// A value of ListBoCode.
      /// </summary>
      [JsonPropertyName("listBoCode")]
      public Common ListBoCode
      {
        get => listBoCode ??= new();
        set => listBoCode = value;
      }

      /// <summary>
      /// A value of HiddenGlobalReassignment.
      /// </summary>
      [JsonPropertyName("hiddenGlobalReassignment")]
      public GlobalReassignment HiddenGlobalReassignment
      {
        get => hiddenGlobalReassignment ??= new();
        set => hiddenGlobalReassignment = value;
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
      /// A value of ListOsp.
      /// </summary>
      [JsonPropertyName("listOsp")]
      public Common ListOsp
      {
        get => listOsp ??= new();
        set => listOsp = value;
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
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

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
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 44;

      private Common common;
      private GlobalReassignment globalReassignment;
      private Common listReaCode;
      private Common listBoCode;
      private GlobalReassignment hiddenGlobalReassignment;
      private ServiceProvider serviceProvider;
      private Common listOsp;
      private ServiceProvider hiddenServiceProvider;
      private OfficeServiceProvider officeServiceProvider;
      private OfficeServiceProvider hiddenOfficeServiceProvider;
      private Office office;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenOffice.
    /// </summary>
    [JsonPropertyName("hiddenOffice")]
    public Office HiddenOffice
    {
      get => hiddenOffice ??= new();
      set => hiddenOffice = value;
    }

    /// <summary>
    /// A value of HiddenSelectionCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenSelectionCodeValue")]
    public CodeValue HiddenSelectionCodeValue
    {
      get => hiddenSelectionCodeValue ??= new();
      set => hiddenSelectionCodeValue = value;
    }

    /// <summary>
    /// A value of HiddenSelectionOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSelectionOfficeServiceProvider")]
    public OfficeServiceProvider HiddenSelectionOfficeServiceProvider
    {
      get => hiddenSelectionOfficeServiceProvider ??= new();
      set => hiddenSelectionOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenSelectionServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSelectionServiceProvider")]
    public ServiceProvider HiddenSelectionServiceProvider
    {
      get => hiddenSelectionServiceProvider ??= new();
      set => hiddenSelectionServiceProvider = value;
    }

    /// <summary>
    /// A value of ListOsp.
    /// </summary>
    [JsonPropertyName("listOsp")]
    public Common ListOsp
    {
      get => listOsp ??= new();
      set => listOsp = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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

    private OfficeServiceProvider existingOfficeServiceProvider;
    private Office hiddenOffice;
    private CodeValue hiddenSelectionCodeValue;
    private OfficeServiceProvider hiddenSelectionOfficeServiceProvider;
    private ServiceProvider hiddenSelectionServiceProvider;
    private Common listOsp;
    private ServiceProvider existingServiceProvider;
    private Office existingOffice;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of GlobalReassignment.
      /// </summary>
      [JsonPropertyName("globalReassignment")]
      public GlobalReassignment GlobalReassignment
      {
        get => globalReassignment ??= new();
        set => globalReassignment = value;
      }

      /// <summary>
      /// A value of HiddenGlobalReassignment.
      /// </summary>
      [JsonPropertyName("hiddenGlobalReassignment")]
      public GlobalReassignment HiddenGlobalReassignment
      {
        get => hiddenGlobalReassignment ??= new();
        set => hiddenGlobalReassignment = value;
      }

      /// <summary>
      /// A value of ListBoCode.
      /// </summary>
      [JsonPropertyName("listBoCode")]
      public Common ListBoCode
      {
        get => listBoCode ??= new();
        set => listBoCode = value;
      }

      /// <summary>
      /// A value of ListReaCode.
      /// </summary>
      [JsonPropertyName("listReaCode")]
      public Common ListReaCode
      {
        get => listReaCode ??= new();
        set => listReaCode = value;
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
      /// A value of HiddenServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenServiceProvider")]
      public ServiceProvider HiddenServiceProvider
      {
        get => hiddenServiceProvider ??= new();
        set => hiddenServiceProvider = value;
      }

      /// <summary>
      /// A value of ListOsp.
      /// </summary>
      [JsonPropertyName("listOsp")]
      public Common ListOsp
      {
        get => listOsp ??= new();
        set => listOsp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 44;

      private Common common;
      private GlobalReassignment globalReassignment;
      private GlobalReassignment hiddenGlobalReassignment;
      private Common listBoCode;
      private Common listReaCode;
      private Office office;
      private OfficeServiceProvider officeServiceProvider;
      private ServiceProvider serviceProvider;
      private ServiceProvider hiddenServiceProvider;
      private Common listOsp;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenOffice.
    /// </summary>
    [JsonPropertyName("hiddenOffice")]
    public Office HiddenOffice
    {
      get => hiddenOffice ??= new();
      set => hiddenOffice = value;
    }

    /// <summary>
    /// A value of HiddenSelectionCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenSelectionCodeValue")]
    public CodeValue HiddenSelectionCodeValue
    {
      get => hiddenSelectionCodeValue ??= new();
      set => hiddenSelectionCodeValue = value;
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
    /// A value of HiddenSelectionServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSelectionServiceProvider")]
    public ServiceProvider HiddenSelectionServiceProvider
    {
      get => hiddenSelectionServiceProvider ??= new();
      set => hiddenSelectionServiceProvider = value;
    }

    /// <summary>
    /// A value of ListOsp.
    /// </summary>
    [JsonPropertyName("listOsp")]
    public Common ListOsp
    {
      get => listOsp ??= new();
      set => listOsp = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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

    private OfficeServiceProvider existingOfficeServiceProvider;
    private Office hiddenOffice;
    private CodeValue hiddenSelectionCodeValue;
    private Code code;
    private CodeValue codeValue;
    private ServiceProvider hiddenSelectionServiceProvider;
    private Common listOsp;
    private ServiceProvider existingServiceProvider;
    private Office existingOffice;
    private Array<ExportGroup> export1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of OspCount.
    /// </summary>
    [JsonPropertyName("ospCount")]
    public Common OspCount
    {
      get => ospCount ??= new();
      set => ospCount = value;
    }

    /// <summary>
    /// A value of InitializedGlobalReassignment.
    /// </summary>
    [JsonPropertyName("initializedGlobalReassignment")]
    public GlobalReassignment InitializedGlobalReassignment
    {
      get => initializedGlobalReassignment ??= new();
      set => initializedGlobalReassignment = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
    }

    /// <summary>
    /// A value of InitializedOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("initializedOfficeServiceProvider")]
    public OfficeServiceProvider InitializedOfficeServiceProvider
    {
      get => initializedOfficeServiceProvider ??= new();
      set => initializedOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of InitializedServiceProvider.
    /// </summary>
    [JsonPropertyName("initializedServiceProvider")]
    public ServiceProvider InitializedServiceProvider
    {
      get => initializedServiceProvider ??= new();
      set => initializedServiceProvider = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common ospCount;
    private GlobalReassignment initializedGlobalReassignment;
    private DateWorkArea current;
    private Common returnCode;
    private DateWorkArea initializedDateWorkArea;
    private OfficeServiceProvider initializedOfficeServiceProvider;
    private ServiceProvider initializedServiceProvider;
    private Common count;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NewOffice.
    /// </summary>
    [JsonPropertyName("newOffice")]
    public Office NewOffice
    {
      get => newOffice ??= new();
      set => newOffice = value;
    }

    /// <summary>
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
    }

    /// <summary>
    /// A value of NewOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProvider")]
    public OfficeServiceProvider NewOfficeServiceProvider
    {
      get => newOfficeServiceProvider ??= new();
      set => newOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of GlobalReassignment.
    /// </summary>
    [JsonPropertyName("globalReassignment")]
    public GlobalReassignment GlobalReassignment
    {
      get => globalReassignment ??= new();
      set => globalReassignment = value;
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
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    private Office newOffice;
    private ServiceProvider newServiceProvider;
    private OfficeServiceProvider newOfficeServiceProvider;
    private GlobalReassignment globalReassignment;
    private Code code;
    private CodeValue codeValue;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private ServiceProvider existingServiceProvider;
    private Office existingOffice;
  }
#endregion
}
