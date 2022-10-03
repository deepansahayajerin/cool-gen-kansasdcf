// Program: OE_LOCL_LOCATE_INTERFACE_LIST, ID: 374435254, model: 746.
// Short name: SWELOCLP
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
/// A program: OE_LOCL_LOCATE_INTERFACE_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeLoclLocateInterfaceList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LOCL_LOCATE_INTERFACE_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLoclLocateInterfaceList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLoclLocateInterfaceList.
  /// </summary>
  public OeLoclLocateInterfaceList(IContext context, Import import,
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
    // *****************************************************
    // Every initial development and change to that
    // development needs to be documented.
    // *****************************************************
    // ************************************************************************************
    // Date     Developers Name         Request #  Description
    // -------- ----------------------  
    // ---------
    // ----------------------------------------
    // 06/28/00 G Vandy                            Initial Development
    // SWSRKXD PR149011 08/16/2002
    // - Fix screen Help Id.
    // ***************************************************************
    local.CurrentDate.Date = Now().Date;

    if (IsEmpty(export.PromptServiceProvider.PromptField))
    {
      export.PromptServiceProvider.PromptField = "+";
    }

    export.Hidden.Assign(import.Hidden);
    ExitState = "ACO_NN0000_ALL_OK";

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

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.CsePerson.Number = import.CsePerson.Number;
    MoveDateWorkArea(import.DateWorkArea, export.DateWorkArea);
    export.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    MoveServiceProvider(import.ServiceProvider, export.ServiceProvider);
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      export.OfficeServiceProvider);
    export.PromptServiceProvider.PromptField =
      import.PromptServiceProvider.PromptField;
    export.ScrollIndicator.Text3 = import.ScrollIndicator.Text3;
    export.PrevHCsePerson.Number = import.PrevHCsePerson.Number;
    MoveDateWorkArea(import.PrevHDateWorkArea, export.PrevHDateWorkArea);
    export.PrevHOffice.SystemGeneratedId = import.PrevHOffice.SystemGeneratedId;
    MoveServiceProvider(import.PrevHServiceProvider, export.PrevHServiceProvider);
      

    if (IsEmpty(export.PromptServiceProvider.PromptField))
    {
      export.PromptServiceProvider.PromptField = "+";
    }

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      export.Group.Index = import.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.CsePersonsWorkSet.FormattedName =
        import.Group.Item.CsePersonsWorkSet.FormattedName;
      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.LocateRequest.Assign(import.Group.Item.LocateRequest);
    }

    import.Group.CheckIndex();

    if (!IsEmpty(export.CsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();

      // -- Verify that the person number is numeric.
      if (Verify(local.TextWorkArea.Text10, "0123456789") != 0)
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "INVALID_NUMERIC_VALUE";

        return;
      }

      export.CsePerson.Number = local.TextWorkArea.Text10;
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        // ----------------------------------------------------------
        // set the local next_tran_info attributes to the import view
        // to pass data to the next transaction
        // ----------------------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (Equal(export.Standard.NextTransaction, "LOCA"))
            {
              export.Hidden.MiscText1 =
                export.Group.Item.LocateRequest.CsePersonNumber;
              export.Hidden.MiscText2 =
                export.Group.Item.LocateRequest.AgencyNumber;
              export.Hidden.MiscNum1 =
                export.Group.Item.LocateRequest.SequenceNumber;
            }

            break;
          }
        }

        export.Group.CheckIndex();
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

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        // this is where you set your export value to the export hidden next 
        // tran values if the user is coming into this procedure on a next tran
        // action.
        UseScCabNextTranGet();

        // -- Default values when entering via nextran.
        export.ServiceProvider.UserId = global.UserId;

        if (ReadOfficeOfficeServiceProviderServiceProvider())
        {
          export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
          MoveServiceProvider(entities.ServiceProvider, export.ServiceProvider);
          MoveOfficeServiceProvider(entities.OfficeServiceProvider,
            export.OfficeServiceProvider);
        }

        global.Command = "DISPLAY";

        // --- Don't want to escape.  We want to execute the list cab with the 
        // defaulted service provider id and date.
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        // You should get this information from the Dialog Flow Diagram.  It is 
        // the SEND CMD on the propertis for a Transfer from one  procedure to
        // another.
        // -- Default values when entering from the menu.
        export.ServiceProvider.UserId = global.UserId;

        if (ReadOfficeOfficeServiceProviderServiceProvider())
        {
          export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
          MoveServiceProvider(entities.ServiceProvider, export.ServiceProvider);
          MoveOfficeServiceProvider(entities.OfficeServiceProvider,
            export.OfficeServiceProvider);
        }

        global.Command = "DISPLAY";

        // --- Don't want to escape.  We want to execute the list cab with the 
        // defaulted service provider id and date.
        // *** if the dialog flow property was display first, just add an escape
        // completely out of the procedure  ****
      }
    }

    if (Equal(global.Command, "RETLINK"))
    {
      export.PromptServiceProvider.PromptField = "+";

      if (Equal(import.PrevHCommon.Command, "SVPO"))
      {
        if (import.FromSvpoServiceProvider.SystemGeneratedId == 0)
        {
          return;
        }
        else
        {
          export.Office.SystemGeneratedId =
            import.FromSvpoOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(import.FromSvpoOfficeServiceProvider,
            export.OfficeServiceProvider);
          MoveServiceProvider(import.FromSvpoServiceProvider,
            export.ServiceProvider);
          global.Command = "DISPLAY";
        }
      }
      else if (Equal(import.PrevHCommon.Command, "LOCA"))
      {
        return;
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "LOCA"))
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "PREV") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "LOCA"))
    {
      if (!Equal(export.PrevHDateWorkArea.Date, export.DateWorkArea.Date))
      {
        var field = GetField(export.DateWorkArea, "date");

        field.Error = true;

        if (Equal(global.Command, "LOCA"))
        {
          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SCROL_IF_CRIT_CHG";
        }
      }

      if (!Equal(export.PrevHCsePerson.Number, export.CsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        if (Equal(global.Command, "LOCA"))
        {
          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SCROL_IF_CRIT_CHG";
        }
      }

      if (export.PrevHOffice.SystemGeneratedId != export
        .Office.SystemGeneratedId)
      {
        var field = GetField(export.Office, "systemGeneratedId");

        field.Error = true;

        if (Equal(global.Command, "LOCA"))
        {
          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SCROL_IF_CRIT_CHG";
        }
      }

      if (!Equal(export.PrevHServiceProvider.UserId,
        export.ServiceProvider.UserId))
      {
        var field = GetField(export.ServiceProvider, "userId");

        field.Error = true;

        if (Equal(global.Command, "LOCA"))
        {
          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SCROL_IF_CRIT_CHG";
        }
      }
    }

    if (!Equal(global.Command, "SVPO") && !Equal(global.Command, "EXIT"))
    {
      if (AsChar(export.PromptServiceProvider.PromptField) != '+')
      {
        var field = GetField(export.PromptServiceProvider, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.PromptServiceProvider.PromptField = "+";
        export.ScrollIndicator.Text3 = "";

        // -- Clear the export group view.
        for(export.Group.Index = 0; export.Group.Index < Export
          .GroupGroup.Capacity; ++export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          export.Group.Update.CsePersonsWorkSet.FormattedName =
            local.NullCsePersonsWorkSet.FormattedName;
          export.Group.Update.Common.SelectChar = local.NullCommon.SelectChar;
          export.Group.Update.LocateRequest.Assign(local.NullLocateRequest);
        }

        export.Group.CheckIndex();
        export.Group.Count = 0;

        if (export.Office.SystemGeneratedId == 0)
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;
        }
        else if (ReadOffice())
        {
          export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
        }
        else
        {
          ExitState = "OFFICE_NF";

          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;
        }

        if (IsEmpty(export.ServiceProvider.UserId))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.ServiceProvider, "userId");

          field.Error = true;
        }
        else if (ReadServiceProvider())
        {
          MoveServiceProvider(entities.ServiceProvider, export.ServiceProvider);
        }
        else
        {
          ExitState = "SERVICE_PROVIDER_NF";

          var field = GetField(export.ServiceProvider, "userId");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(import.PrevHCommon.Command, "SVPO"))
        {
          if (ReadOfficeServiceProvider1())
          {
            export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
            MoveServiceProvider(entities.ServiceProvider, export.ServiceProvider);
              
            MoveOfficeServiceProvider(entities.OfficeServiceProvider,
              export.OfficeServiceProvider);
          }
        }
        else if (ReadOfficeServiceProvider2())
        {
          export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
          MoveServiceProvider(entities.ServiceProvider, export.ServiceProvider);
          MoveOfficeServiceProvider(entities.OfficeServiceProvider,
            export.OfficeServiceProvider);
        }

        if (!entities.OfficeServiceProvider.Populated)
        {
          ExitState = "OFFICE_SERVICE_PROVIDER_NF";

          var field1 = GetField(export.ServiceProvider, "userId");

          field1.Error = true;

          var field2 = GetField(export.Office, "systemGeneratedId");

          field2.Error = true;

          return;
        }

        UseOeListLocateInterfaceRecords1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.PrevHCsePerson.Number = export.CsePerson.Number;
          MoveDateWorkArea(export.DateWorkArea, export.PrevHDateWorkArea);
          export.PrevHOffice.SystemGeneratedId =
            export.Office.SystemGeneratedId;
          MoveServiceProvider(export.ServiceProvider,
            export.PrevHServiceProvider);

          if (export.Group.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SVPO":
        if (AsChar(export.PromptServiceProvider.PromptField) == 'S')
        {
          // --- Link to SVPO to select the service provider.
          ExitState = "ECO_LNK_TO_SVPO";
          export.PrevHCommon.Command = global.Command;
        }
        else
        {
          // --- An invalid prompt value was entered.
          var field = GetField(export.PromptServiceProvider, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case "PREV":
        if (Find(export.ScrollIndicator.Text3, "-") == 0)
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }

        for(export.Group.Index = export.Group.Count - 1; export.Group.Index >= 0
          ; --export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SCROLL_SELECTIONS";
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NE0000_NO_SCROLL_SELECTIONS"))
        {
          return;
        }

        export.Group.Index = 0;
        export.Group.CheckSize();

        UseOeListLocateInterfaceRecords2();

        break;
      case "NEXT":
        if (Find(export.ScrollIndicator.Text3, "+") == 0)
        {
          ExitState = "NO_MORE_ITEMS_TO_SCROLL";

          return;
        }

        for(export.Group.Index = export.Group.Count - 1; export.Group.Index >= 0
          ; --export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SCROLL_SELECTIONS";
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NE0000_NO_SCROLL_SELECTIONS"))
        {
          return;
        }

        export.Group.Index = export.Group.Count - 1;
        export.Group.CheckSize();

        UseOeListLocateInterfaceRecords2();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LOCA":
        // ---  Verify that no invalid selection characters have been entered 
        // and that only one row is selected.
        local.Common.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Group.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.Common.Count;

              // -- Move the selected row to the export selected view to pass to
              // LOCA.
              export.Selected.Assign(export.Group.Item.LocateRequest);

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              break;
          }
        }

        export.Group.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(local.Common.Count)
        {
          case 0:
            // --- No selection was made
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            // --- Only one selection was made.  Link to LOCA.
            ExitState = "ECO_LNK_TO_LOCA";
            export.LastKeyToLoca.Assign(export.Selected);
            export.PrevHCommon.Command = global.Command;

            break;
          default:
            // --- Multiple selections were made.
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Group.Item.Common.SelectChar))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;
              }
            }

            export.Group.CheckIndex();

            break;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.TextDate = source.TextDate;
    target.Date = source.Date;
  }

  private static void MoveLoclToGroup(OeListLocateInterfaceRecords.Export.
    LoclGroup source, Export.GroupGroup target)
  {
    target.Common.SelectChar = source.LoclCommon.SelectChar;
    target.CsePersonsWorkSet.FormattedName =
      source.LoclCsePersonsWorkSet.FormattedName;
    target.LocateRequest.Assign(source.LoclLocateRequest);
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
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

  private void UseOeListLocateInterfaceRecords1()
  {
    var useImport = new OeListLocateInterfaceRecords.Import();
    var useExport = new OeListLocateInterfaceRecords.Export();

    MoveOfficeServiceProvider(export.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = export.Office.SystemGeneratedId;
    useImport.LoclFilterCsePerson.Number = export.CsePerson.Number;
    useImport.LoclFilterDateWorkArea.Date = export.DateWorkArea.Date;

    Call(OeListLocateInterfaceRecords.Execute, useImport, useExport);

    useExport.Locl.CopyTo(export.Group, MoveLoclToGroup);
    export.ScrollIndicator.Text3 = useExport.ScrollIndicator.Text3;
  }

  private void UseOeListLocateInterfaceRecords2()
  {
    var useImport = new OeListLocateInterfaceRecords.Import();
    var useExport = new OeListLocateInterfaceRecords.Export();

    MoveOfficeServiceProvider(export.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = export.Office.SystemGeneratedId;
    useImport.LoclFilterCsePerson.Number = export.CsePerson.Number;
    useImport.LoclFilterDateWorkArea.Date = export.DateWorkArea.Date;
    useImport.LoclScrollingValue.Assign(export.Group.Item.LocateRequest);

    Call(OeListLocateInterfaceRecords.Execute, useImport, useExport);

    useExport.Locl.CopyTo(export.Group, MoveLoclToGroup);
    export.ScrollIndicator.Text3 = useExport.ScrollIndicator.Text3;
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

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProviderServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
        db.SetString(command, "userId", export.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 2);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 3);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetDate(
          command, "effectiveDate",
          import.FromSvpoOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode", import.FromSvpoOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
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

  private bool ReadOfficeServiceProvider2()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
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
        db.SetString(command, "userId", export.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of LocateRequest.
      /// </summary>
      [JsonPropertyName("locateRequest")]
      public LocateRequest LocateRequest
      {
        get => locateRequest ??= new();
        set => locateRequest = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Common common;
      private CsePersonsWorkSet csePersonsWorkSet;
      private LocateRequest locateRequest;
    }

    /// <summary>
    /// A value of LastKeyToLoca.
    /// </summary>
    [JsonPropertyName("lastKeyToLoca")]
    public LocateRequest LastKeyToLoca
    {
      get => lastKeyToLoca ??= new();
      set => lastKeyToLoca = value;
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
    /// A value of PrevHCommon.
    /// </summary>
    [JsonPropertyName("prevHCommon")]
    public Common PrevHCommon
    {
      get => prevHCommon ??= new();
      set => prevHCommon = value;
    }

    /// <summary>
    /// A value of FromSvpoOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("fromSvpoOfficeServiceProvider")]
    public OfficeServiceProvider FromSvpoOfficeServiceProvider
    {
      get => fromSvpoOfficeServiceProvider ??= new();
      set => fromSvpoOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of FromSvpoServiceProvider.
    /// </summary>
    [JsonPropertyName("fromSvpoServiceProvider")]
    public ServiceProvider FromSvpoServiceProvider
    {
      get => fromSvpoServiceProvider ??= new();
      set => fromSvpoServiceProvider = value;
    }

    /// <summary>
    /// A value of FromSvpoOffice.
    /// </summary>
    [JsonPropertyName("fromSvpoOffice")]
    public Office FromSvpoOffice
    {
      get => fromSvpoOffice ??= new();
      set => fromSvpoOffice = value;
    }

    /// <summary>
    /// A value of PrevHServiceProvider.
    /// </summary>
    [JsonPropertyName("prevHServiceProvider")]
    public ServiceProvider PrevHServiceProvider
    {
      get => prevHServiceProvider ??= new();
      set => prevHServiceProvider = value;
    }

    /// <summary>
    /// A value of PrevHOffice.
    /// </summary>
    [JsonPropertyName("prevHOffice")]
    public Office PrevHOffice
    {
      get => prevHOffice ??= new();
      set => prevHOffice = value;
    }

    /// <summary>
    /// A value of PrevHCsePerson.
    /// </summary>
    [JsonPropertyName("prevHCsePerson")]
    public CsePerson PrevHCsePerson
    {
      get => prevHCsePerson ??= new();
      set => prevHCsePerson = value;
    }

    /// <summary>
    /// A value of PrevHDateWorkArea.
    /// </summary>
    [JsonPropertyName("prevHDateWorkArea")]
    public DateWorkArea PrevHDateWorkArea
    {
      get => prevHDateWorkArea ??= new();
      set => prevHDateWorkArea = value;
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public WorkArea ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
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
    /// A value of PromptServiceProvider.
    /// </summary>
    [JsonPropertyName("promptServiceProvider")]
    public Standard PromptServiceProvider
    {
      get => promptServiceProvider ??= new();
      set => promptServiceProvider = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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

    private LocateRequest lastKeyToLoca;
    private NextTranInfo hidden;
    private Common prevHCommon;
    private OfficeServiceProvider fromSvpoOfficeServiceProvider;
    private ServiceProvider fromSvpoServiceProvider;
    private Office fromSvpoOffice;
    private ServiceProvider prevHServiceProvider;
    private Office prevHOffice;
    private CsePerson prevHCsePerson;
    private DateWorkArea prevHDateWorkArea;
    private WorkArea scrollIndicator;
    private OfficeServiceProvider officeServiceProvider;
    private Array<GroupGroup> group;
    private Standard promptServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePerson csePerson;
    private DateWorkArea dateWorkArea;
    private Standard standard;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of LocateRequest.
      /// </summary>
      [JsonPropertyName("locateRequest")]
      public LocateRequest LocateRequest
      {
        get => locateRequest ??= new();
        set => locateRequest = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Common common;
      private CsePersonsWorkSet csePersonsWorkSet;
      private LocateRequest locateRequest;
    }

    /// <summary>
    /// A value of LastKeyToLoca.
    /// </summary>
    [JsonPropertyName("lastKeyToLoca")]
    public LocateRequest LastKeyToLoca
    {
      get => lastKeyToLoca ??= new();
      set => lastKeyToLoca = value;
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
    /// A value of PrevHCommon.
    /// </summary>
    [JsonPropertyName("prevHCommon")]
    public Common PrevHCommon
    {
      get => prevHCommon ??= new();
      set => prevHCommon = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public LocateRequest Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of PrevHServiceProvider.
    /// </summary>
    [JsonPropertyName("prevHServiceProvider")]
    public ServiceProvider PrevHServiceProvider
    {
      get => prevHServiceProvider ??= new();
      set => prevHServiceProvider = value;
    }

    /// <summary>
    /// A value of PrevHOffice.
    /// </summary>
    [JsonPropertyName("prevHOffice")]
    public Office PrevHOffice
    {
      get => prevHOffice ??= new();
      set => prevHOffice = value;
    }

    /// <summary>
    /// A value of PrevHCsePerson.
    /// </summary>
    [JsonPropertyName("prevHCsePerson")]
    public CsePerson PrevHCsePerson
    {
      get => prevHCsePerson ??= new();
      set => prevHCsePerson = value;
    }

    /// <summary>
    /// A value of PrevHDateWorkArea.
    /// </summary>
    [JsonPropertyName("prevHDateWorkArea")]
    public DateWorkArea PrevHDateWorkArea
    {
      get => prevHDateWorkArea ??= new();
      set => prevHDateWorkArea = value;
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
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public WorkArea ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// A value of PromptServiceProvider.
    /// </summary>
    [JsonPropertyName("promptServiceProvider")]
    public Standard PromptServiceProvider
    {
      get => promptServiceProvider ??= new();
      set => promptServiceProvider = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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

    private LocateRequest lastKeyToLoca;
    private NextTranInfo hidden;
    private Common prevHCommon;
    private LocateRequest selected;
    private ServiceProvider prevHServiceProvider;
    private Office prevHOffice;
    private CsePerson prevHCsePerson;
    private DateWorkArea prevHDateWorkArea;
    private OfficeServiceProvider officeServiceProvider;
    private Array<GroupGroup> group;
    private WorkArea scrollIndicator;
    private Standard promptServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePerson csePerson;
    private DateWorkArea dateWorkArea;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of NullCommon.
    /// </summary>
    [JsonPropertyName("nullCommon")]
    public Common NullCommon
    {
      get => nullCommon ??= new();
      set => nullCommon = value;
    }

    /// <summary>
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NullLocateRequest.
    /// </summary>
    [JsonPropertyName("nullLocateRequest")]
    public LocateRequest NullLocateRequest
    {
      get => nullLocateRequest ??= new();
      set => nullLocateRequest = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private TextWorkArea textWorkArea;
    private Common nullCommon;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private LocateRequest nullLocateRequest;
    private DateWorkArea currentDate;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }
#endregion
}
