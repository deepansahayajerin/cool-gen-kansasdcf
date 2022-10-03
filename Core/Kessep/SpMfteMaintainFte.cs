// Program: SP_MFTE_MAINTAIN_FTE, ID: 945145059, model: 746.
// Short name: SWEMFTEP
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
/// A program: SP_MFTE_MAINTAIN_FTE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpMfteMaintainFte: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_MFTE_MAINTAIN_FTE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpMfteMaintainFte(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpMfteMaintainFte.
  /// </summary>
  public SpMfteMaintainFte(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------
    // ---
    // 
    // ---
    // ---
    // 
    // Maintain Full Time Equivalent
    // --
    // -
    // ---
    // 
    // ---
    // ---------------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // 06/10/13  GVandy	CQ36547		Initial Development.  Copied from ASLM.
    // 			Segment B	
    // ---------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      // ---------------------------------------------
      // Clear scrolling group if command=clear.
      // ---------------------------------------------
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    MoveWorkArea(import.YearMonth, export.YearMonth);
    MoveWorkArea(import.HiddenImportYearMonth, export.HiddenExportYearMonth);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ---------------------------------------------
    // Move group views if command <> display.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else if (Equal(global.Command, "OFFICES"))
    {
    }
    else
    {
      local.Prompt.Count = 0;
      local.Select.Count = 0;

      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        MoveOffice(import.Group.Item.Office1, export.Group.Update.Office1);
        MoveOffice(import.Group.Item.HiddenOffice,
          export.Group.Update.HiddenOffice);
        MoveOfficeStaffing(import.Group.Item.OfficeStaffing,
          export.Group.Update.OfficeStaffing);
        MoveOfficeStaffing(import.Group.Item.HiddenOfficeStaffing,
          export.Group.Update.HiddenOfficeStaffing);
        export.Group.Update.Office2.SelectChar =
          import.Group.Item.Office2.SelectChar;
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;

        if (AsChar(export.Group.Item.Office2.SelectChar) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          ++local.Select.Count;
        }

        export.Group.Next();
      }
    }

    // **** All valid commands for this AB is validated in the following CASE OF
    // ****
    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          local.NextTranInfo.Assign(import.Hidden);

          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          UseScCabNextTranPut();

          return;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        return;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "OFFICES":
        local.Offices.Flag = "Y";
        global.Command = "DISPLAY";

        break;
      case "ADD":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "DELETE":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "LIST":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "UPDATE":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "XXNEXTXX":
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();

        // ---------------------------------------------
        // Populate export views from local next_tran_info view read from the 
        // data base
        // Set command to initial command required or ESCAPE
        // ---------------------------------------------
        export.Hidden.Assign(local.NextTranInfo);
        global.Command = "DISPLAY";

        break;
      case "RETOFCL":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Office2.SelectChar) == 'S')
          {
            export.Group.Update.Office2.SelectChar = "";

            if (import.FromOfcl.SystemGeneratedId > 0)
            {
              MoveOffice(import.FromOfcl, export.Group.Update.Office1);
            }

            var field =
              GetField(export.Group.Item.Office1, "systemGeneratedId");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }
        }

        return;
      case "SELALL":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (export.Group.Item.OfficeStaffing.YearMonth > 0)
          {
            export.Group.Update.Common.SelectChar = "S";
          }
        }

        return;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
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

    // ---------------------------------------------
    // Move group views if command <> display.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      local.Prompt.Count = 0;
      local.Select.Count = 0;

      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        MoveOffice(import.Group.Item.Office1, export.Group.Update.Office1);
        MoveOffice(import.Group.Item.HiddenOffice,
          export.Group.Update.HiddenOffice);
        MoveOfficeStaffing(import.Group.Item.OfficeStaffing,
          export.Group.Update.OfficeStaffing);
        MoveOfficeStaffing(import.Group.Item.HiddenOfficeStaffing,
          export.Group.Update.HiddenOfficeStaffing);
        export.Group.Update.Office2.SelectChar =
          import.Group.Item.Office2.SelectChar;
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;

        if (AsChar(export.Group.Item.Office2.SelectChar) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          ++local.Select.Count;
        }

        export.Group.Next();
      }
    }

    // ---------------------------------------------
    // Must display before maintenance.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
      {
        if (Equal(import.YearMonth.Text2, import.HiddenImportYearMonth.Text2) &&
          Equal(import.YearMonth.Text4, import.HiddenImportYearMonth.Text4))
        {
        }
        else
        {
          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }
      }

      switch(local.Select.Count)
      {
        case 0:
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        case 1:
          break;
        default:
          break;
      }
    }

    // ---------------------------------------------
    // Prompt is only valid on PF4 List.
    // ---------------------------------------------
    if (Equal(global.Command, "LIST"))
    {
      switch(local.Prompt.Count)
      {
        case 0:
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        case 1:
          break;
        default:
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.Office2.SelectChar) == 'S')
            {
              var field = GetField(export.Group.Item.Office2, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "ADD":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              local.StaffingMonth.Year =
                (int)StringToNumber(export.YearMonth.Text4);
              local.StaffingMonth.Month =
                (int)StringToNumber(export.YearMonth.Text2);
              local.StaffingMonth.Day = 1;
              local.StaffingMonth.Date = IntToDate(local.StaffingMonth.Year * 10000
                + local.StaffingMonth.Month * 100 + local.StaffingMonth.Day);
              local.StaffingMonth.YearMonth = local.StaffingMonth.Year * 100 + local
                .StaffingMonth.Month;

              // ---------------------------------------------
              // An add must be on a previously blank row.
              // ---------------------------------------------
              if (export.Group.Item.HiddenOffice.SystemGeneratedId > 0 && export
                .Group.Item.HiddenOfficeStaffing.YearMonth > 0 && export
                .Group.Item.HiddenOfficeStaffing.YearMonth == local
                .StaffingMonth.YearMonth)
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

                return;
              }

              // ---------------------------------------------
              // Prompt is not valid on add.
              // ---------------------------------------------
              if (AsChar(export.Group.Item.Office2.SelectChar) == 'S')
              {
                var field = GetField(export.Group.Item.Office2, "selectChar");

                field.Error = true;

                ExitState = "SP0000_PROMPT_NOT_ALLOWED";

                return;
              }

              export.Group.Update.OfficeStaffing.YearMonth =
                local.StaffingMonth.YearMonth;
              UseSpCreateOfficeStaffing();

              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("ACO_NI0000_ADD_SUCCESSFUL"))
              {
                ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                if (IsExitState("OFFICE_NF"))
                {
                  export.Group.Update.Office2.SelectChar = "S";

                  var field =
                    GetField(export.Group.Item.Office1, "systemGeneratedId");

                  field.Error = true;

                  ExitState = "ZD_ACO_NE0_INVALID_CODE_PRES_PF4";
                }
                else
                {
                  export.Group.Update.Office2.SelectChar = "S";

                  var field1 = GetField(export.Group.Item.Common, "selectChar");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Group.Item.Office1, "systemGeneratedId");

                  field2.Error = true;

                  var field3 =
                    GetField(export.Group.Item.OfficeStaffing,
                    "fullTimeEquivalent");

                  field3.Error = true;
                }

                return;
              }

              MoveOffice(export.Group.Item.Office1,
                export.Group.Update.HiddenOffice);
              MoveOfficeStaffing(export.Group.Item.OfficeStaffing,
                export.Group.Update.HiddenOfficeStaffing);
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_PROMPT_VAL";
            }
          }
          else
          {
            continue;
          }
        }

        break;
      case "UPDATE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // An update must be performed on a populated
              // row.
              // ---------------------------------------------
              if (export.Group.Item.HiddenOffice.SystemGeneratedId == 0 && export
                .Group.Item.HiddenOfficeStaffing.YearMonth == 0)
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

                return;
              }

              // ---------------------------------------------
              // Prompt is not valid on update.
              // ---------------------------------------------
              if (AsChar(export.Group.Item.Office2.SelectChar) == 'S')
              {
                var field = GetField(export.Group.Item.Office2, "selectChar");

                field.Error = true;

                ExitState = "SP0000_PROMPT_NOT_ALLOWED";

                return;
              }

              // ---------------------------------------------
              // Effective date or disc date must have
              // changed.
              // ---------------------------------------------
              if (export.Group.Item.OfficeStaffing.FullTimeEquivalent.
                GetValueOrDefault() == export
                .Group.Item.HiddenOfficeStaffing.FullTimeEquivalent.
                  GetValueOrDefault())
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_DATA_NOT_CHANGED";

                return;
              }

              // ---------------------------------------------
              // Perform data validation for update request.
              // ---------------------------------------------
              if (export.Group.Item.Office1.SystemGeneratedId != export
                .Group.Item.HiddenOffice.SystemGeneratedId)
              {
                var field =
                  GetField(export.Group.Item.Office1, "systemGeneratedId");

                field.Error = true;

                export.Group.Update.Office1.SystemGeneratedId =
                  export.Group.Item.HiddenOffice.SystemGeneratedId;
                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";

                return;
              }
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_PROMPT_VAL";
            }
          }
          else
          {
            continue;
          }

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
          }
          else
          {
            return;
          }

          // ---------------------------------------------
          // Data has passed validation. Update.
          // ---------------------------------------------
          UseSpUpdateOfficeStaffing();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
            ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
          }
          else
          {
            export.Group.Update.Office2.SelectChar = "S";

            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Error = true;

            var field2 =
              GetField(export.Group.Item.Office1, "systemGeneratedId");

            field2.Error = true;

            var field3 =
              GetField(export.Group.Item.OfficeStaffing, "fullTimeEquivalent");

            field3.Error = true;

            return;
          }

          MoveOffice(export.Group.Item.Office1, export.Group.Update.HiddenOffice);
            
          MoveOfficeStaffing(export.Group.Item.OfficeStaffing,
            export.Group.Update.HiddenOfficeStaffing);
        }

        break;
      case "DELETE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // A delete must be performed on a populated
              // row.
              // ---------------------------------------------
              if (export.Group.Item.HiddenOfficeStaffing.YearMonth == 0)
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_DELETE_ON_EMPTY_ROW";

                return;
              }

              UseSpDeleteOfficeStaffing();

              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("ACO_NI0000_DELETE_SUCCESSFUL"))
              {
                ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
                MoveOfficeStaffing(local.NullOfficeStaffing,
                  export.Group.Update.OfficeStaffing);
                MoveOfficeStaffing(local.NullOfficeStaffing,
                  export.Group.Update.HiddenOfficeStaffing);
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                export.Group.Update.Office2.SelectChar = "S";

                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.Office1, "systemGeneratedId");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.OfficeStaffing,
                  "fullTimeEquivalent");

                field3.Error = true;

                return;
              }
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_PROMPT_VAL";

              return;
            }
          }
        }

        break;
      case "LIST":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Office2.SelectChar))
          {
            if (AsChar(export.Group.Item.Office2.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_LIST_OFFICE";
              ++local.Common.Count;
            }
            else
            {
              var field = GetField(export.Group.Item.Office2, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_PROMPT_VAL";

              return;
            }
          }
        }

        if (local.Common.Count == 0)
        {
          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";
        }
        else if (local.Common.Count > 1)
        {
          ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S";
        }

        break;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.YearMonth.Text2))
      {
        // -- Default to current month.
        export.YearMonth.Text2 = NumberToString(Now().Date.Month, 14, 2);
      }

      if (Verify(export.YearMonth.Text2, " 0123456789") > 0)
      {
        var field = GetField(export.YearMonth, "text2");

        field.Error = true;

        ExitState = "INVALID_MONTH_ENTERED";
      }
      else
      {
        local.StaffingMonth.Month = (int)StringToNumber(export.YearMonth.Text2);

        if (local.StaffingMonth.Month < 1 || local.StaffingMonth.Month > 12)
        {
          var field = GetField(export.YearMonth, "text2");

          field.Error = true;

          ExitState = "INVALID_MONTH_ENTERED";
        }
      }

      if (IsEmpty(export.YearMonth.Text4))
      {
        // -- Default to current year.
        export.YearMonth.Text4 = NumberToString(Now().Date.Year, 12, 4);
      }

      if (Lt(export.YearMonth.Text4, "0000") || Lt
        ("9999", export.YearMonth.Text4) || Verify
        (export.YearMonth.Text4, "0123456789") > 0)
      {
        var field = GetField(export.YearMonth, "text4");

        field.Error = true;

        ExitState = "OE0000_INVALID_YEAR";
      }
      else
      {
        local.StaffingMonth.Year = (int)StringToNumber(export.YearMonth.Text4);
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.StaffingMonth.Day = 1;
      local.StaffingMonth.Date = IntToDate(local.StaffingMonth.Year * 10000 + local
        .StaffingMonth.Month * 100 + local.StaffingMonth.Day);
      local.StaffingMonth.YearMonth = local.StaffingMonth.Year * 100 + local
        .StaffingMonth.Month;

      if (AsChar(local.Offices.Flag) == 'Y')
      {
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadOffice())
        {
          MoveOffice(entities.Office, export.Group.Update.Office1);
          MoveOffice(entities.Office, export.Group.Update.HiddenOffice);

          if (ReadOfficeStaffing())
          {
            MoveOfficeStaffing(entities.OfficeStaffing,
              export.Group.Update.OfficeStaffing);
            MoveOfficeStaffing(entities.OfficeStaffing,
              export.Group.Update.HiddenOfficeStaffing);
          }

          export.Group.Next();
        }
      }
      else
      {
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadOfficeOfficeStaffing())
        {
          MoveOffice(entities.Office, export.Group.Update.Office1);
          MoveOffice(entities.Office, export.Group.Update.HiddenOffice);
          MoveOfficeStaffing(entities.OfficeStaffing,
            export.Group.Update.OfficeStaffing);
          MoveOfficeStaffing(entities.OfficeStaffing,
            export.Group.Update.HiddenOfficeStaffing);
          export.Group.Next();
        }
      }

      MoveWorkArea(export.YearMonth, export.HiddenExportYearMonth);

      if (export.Group.IsEmpty)
      {
        ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
    }
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveOfficeStaffing(OfficeStaffing source,
    OfficeStaffing target)
  {
    target.YearMonth = source.YearMonth;
    target.FullTimeEquivalent = source.FullTimeEquivalent;
  }

  private static void MoveWorkArea(WorkArea source, WorkArea target)
  {
    target.Text2 = source.Text2;
    target.Text4 = source.Text4;
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

  private void UseSpCreateOfficeStaffing()
  {
    var useImport = new SpCreateOfficeStaffing.Import();
    var useExport = new SpCreateOfficeStaffing.Export();

    MoveOfficeStaffing(export.Group.Item.OfficeStaffing,
      useImport.OfficeStaffing);
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office1.SystemGeneratedId;

    Call(SpCreateOfficeStaffing.Execute, useImport, useExport);
  }

  private void UseSpDeleteOfficeStaffing()
  {
    var useImport = new SpDeleteOfficeStaffing.Import();
    var useExport = new SpDeleteOfficeStaffing.Export();

    useImport.OfficeStaffing.YearMonth =
      export.Group.Item.OfficeStaffing.YearMonth;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office1.SystemGeneratedId;

    Call(SpDeleteOfficeStaffing.Execute, useImport, useExport);
  }

  private void UseSpUpdateOfficeStaffing()
  {
    var useImport = new SpUpdateOfficeStaffing.Import();
    var useExport = new SpUpdateOfficeStaffing.Export();

    MoveOfficeStaffing(export.Group.Item.OfficeStaffing,
      useImport.OfficeStaffing);
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office1.SystemGeneratedId;

    Call(SpUpdateOfficeStaffing.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadOffice()
  {
    return ReadEach("ReadOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.StaffingMonth.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.EffectiveDate = db.GetDate(reader, 2);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 4);
        entities.Office.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeOfficeStaffing()
  {
    return ReadEach("ReadOfficeOfficeStaffing",
      (db, command) =>
      {
        db.SetInt32(command, "yearMonth", local.StaffingMonth.YearMonth);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeStaffing.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.EffectiveDate = db.GetDate(reader, 2);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 4);
        entities.OfficeStaffing.YearMonth = db.GetInt32(reader, 5);
        entities.OfficeStaffing.FullTimeEquivalent =
          db.GetNullableDecimal(reader, 6);
        entities.OfficeStaffing.Populated = true;
        entities.Office.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeStaffing()
  {
    entities.OfficeStaffing.Populated = false;

    return Read("ReadOfficeStaffing",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetInt32(command, "yearMonth", local.StaffingMonth.YearMonth);
      },
      (db, reader) =>
      {
        entities.OfficeStaffing.YearMonth = db.GetInt32(reader, 0);
        entities.OfficeStaffing.FullTimeEquivalent =
          db.GetNullableDecimal(reader, 1);
        entities.OfficeStaffing.OffGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeStaffing.Populated = true;
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
      /// A value of HiddenOffice.
      /// </summary>
      [JsonPropertyName("hiddenOffice")]
      public Office HiddenOffice
      {
        get => hiddenOffice ??= new();
        set => hiddenOffice = value;
      }

      /// <summary>
      /// A value of Office1.
      /// </summary>
      [JsonPropertyName("office1")]
      public Office Office1
      {
        get => office1 ??= new();
        set => office1 = value;
      }

      /// <summary>
      /// A value of HiddenOfficeStaffing.
      /// </summary>
      [JsonPropertyName("hiddenOfficeStaffing")]
      public OfficeStaffing HiddenOfficeStaffing
      {
        get => hiddenOfficeStaffing ??= new();
        set => hiddenOfficeStaffing = value;
      }

      /// <summary>
      /// A value of OfficeStaffing.
      /// </summary>
      [JsonPropertyName("officeStaffing")]
      public OfficeStaffing OfficeStaffing
      {
        get => officeStaffing ??= new();
        set => officeStaffing = value;
      }

      /// <summary>
      /// A value of Office2.
      /// </summary>
      [JsonPropertyName("office2")]
      public Common Office2
      {
        get => office2 ??= new();
        set => office2 = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 165;

      private Office hiddenOffice;
      private Office office1;
      private OfficeStaffing hiddenOfficeStaffing;
      private OfficeStaffing officeStaffing;
      private Common office2;
      private Common common;
    }

    /// <summary>
    /// A value of FromOfcl.
    /// </summary>
    [JsonPropertyName("fromOfcl")]
    public Office FromOfcl
    {
      get => fromOfcl ??= new();
      set => fromOfcl = value;
    }

    /// <summary>
    /// A value of HiddenImportYearMonth.
    /// </summary>
    [JsonPropertyName("hiddenImportYearMonth")]
    public WorkArea HiddenImportYearMonth
    {
      get => hiddenImportYearMonth ??= new();
      set => hiddenImportYearMonth = value;
    }

    /// <summary>
    /// A value of YearMonth.
    /// </summary>
    [JsonPropertyName("yearMonth")]
    public WorkArea YearMonth
    {
      get => yearMonth ??= new();
      set => yearMonth = value;
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
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Office fromOfcl;
    private WorkArea hiddenImportYearMonth;
    private WorkArea yearMonth;
    private Standard standard;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
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
      /// A value of HiddenOffice.
      /// </summary>
      [JsonPropertyName("hiddenOffice")]
      public Office HiddenOffice
      {
        get => hiddenOffice ??= new();
        set => hiddenOffice = value;
      }

      /// <summary>
      /// A value of Office1.
      /// </summary>
      [JsonPropertyName("office1")]
      public Office Office1
      {
        get => office1 ??= new();
        set => office1 = value;
      }

      /// <summary>
      /// A value of HiddenOfficeStaffing.
      /// </summary>
      [JsonPropertyName("hiddenOfficeStaffing")]
      public OfficeStaffing HiddenOfficeStaffing
      {
        get => hiddenOfficeStaffing ??= new();
        set => hiddenOfficeStaffing = value;
      }

      /// <summary>
      /// A value of OfficeStaffing.
      /// </summary>
      [JsonPropertyName("officeStaffing")]
      public OfficeStaffing OfficeStaffing
      {
        get => officeStaffing ??= new();
        set => officeStaffing = value;
      }

      /// <summary>
      /// A value of Office2.
      /// </summary>
      [JsonPropertyName("office2")]
      public Common Office2
      {
        get => office2 ??= new();
        set => office2 = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 165;

      private Office hiddenOffice;
      private Office office1;
      private OfficeStaffing hiddenOfficeStaffing;
      private OfficeStaffing officeStaffing;
      private Common office2;
      private Common common;
    }

    /// <summary>
    /// A value of HiddenExportYearMonth.
    /// </summary>
    [JsonPropertyName("hiddenExportYearMonth")]
    public WorkArea HiddenExportYearMonth
    {
      get => hiddenExportYearMonth ??= new();
      set => hiddenExportYearMonth = value;
    }

    /// <summary>
    /// A value of YearMonth.
    /// </summary>
    [JsonPropertyName("yearMonth")]
    public WorkArea YearMonth
    {
      get => yearMonth ??= new();
      set => yearMonth = value;
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
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private WorkArea hiddenExportYearMonth;
    private WorkArea yearMonth;
    private Standard standard;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Offices.
    /// </summary>
    [JsonPropertyName("offices")]
    public Common Offices
    {
      get => offices ??= new();
      set => offices = value;
    }

    /// <summary>
    /// A value of NullOfficeStaffing.
    /// </summary>
    [JsonPropertyName("nullOfficeStaffing")]
    public OfficeStaffing NullOfficeStaffing
    {
      get => nullOfficeStaffing ??= new();
      set => nullOfficeStaffing = value;
    }

    /// <summary>
    /// A value of NullOffice.
    /// </summary>
    [JsonPropertyName("nullOffice")]
    public Office NullOffice
    {
      get => nullOffice ??= new();
      set => nullOffice = value;
    }

    /// <summary>
    /// A value of StaffingMonth.
    /// </summary>
    [JsonPropertyName("staffingMonth")]
    public DateWorkArea StaffingMonth
    {
      get => staffingMonth ??= new();
      set => staffingMonth = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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

    private Common offices;
    private OfficeStaffing nullOfficeStaffing;
    private Office nullOffice;
    private DateWorkArea staffingMonth;
    private Common select;
    private Common prompt;
    private Common common;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OfficeStaffing.
    /// </summary>
    [JsonPropertyName("officeStaffing")]
    public OfficeStaffing OfficeStaffing
    {
      get => officeStaffing ??= new();
      set => officeStaffing = value;
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

    private OfficeStaffing officeStaffing;
    private Office office;
  }
#endregion
}
