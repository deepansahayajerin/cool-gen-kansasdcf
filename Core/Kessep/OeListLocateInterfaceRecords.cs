// Program: OE_LIST_LOCATE_INTERFACE_RECORDS, ID: 374424646, model: 746.
// Short name: SWE00955
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_LIST_LOCATE_INTERFACE_RECORDS.
/// </summary>
[Serializable]
public partial class OeListLocateInterfaceRecords: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LIST_LOCATE_INTERFACE_RECORDS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeListLocateInterfaceRecords(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeListLocateInterfaceRecords.
  /// </summary>
  public OeListLocateInterfaceRecords(IContext context, Import import,
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
    // --------------------------------------------------------------
    // CHANGE LOG:
    // 06/28/2000	PMcElderry, GVandy		Original coding
    // 08/12/2010	JHuss		CQ# 513		Allow any cases associated to person to 
    // display on LOCA.
    // 						Display the current user's cases first, then the rest.
    // --------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.CsePerson.Number = import.Loca1.Number;
    export.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    export.CsePerson.Number = import.Loca1.Number;
    export.ServiceProvider.SystemGeneratedId =
      import.ServiceProvider.SystemGeneratedId;
    local.Previous.Command = import.Previous.Command;
    export.LocateRequest.Assign(import.LocateRequest);
    export.NoCases.Flag = import.NoCases.Flag;

    // ---------------------------------------------------------
    // Determine which screen (LOCL or LOCA) is using the action
    // block
    // ---------------------------------------------------------
    if (Equal(global.TranCode, "SRDP"))
    {
      // ---------------------------------------------------
      // From LOCL - display only LOCATE REQUESTS which have
      // been returned from participating agencies
      // ---------------------------------------------------
      if (!ReadOfficeServiceProvider())
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";

        return;
      }

      if (IsEmpty(import.LoclFilterCsePerson.Number) && Equal
        (import.LoclFilterDateWorkArea.Date, local.Null1.Date))
      {
        switch(TrimEnd(global.Command))
        {
          case "DISPLAY":
            export.Locl.Index = -1;

            foreach(var item in ReadLocateRequest16())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index + 1 == Export.LoclGroup.Capacity)
              {
                export.ScrollIndicator.Text3 = "  +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                ++export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          case "PREV":
            export.Locl.Index = Export.LoclGroup.Capacity;
            export.Locl.CheckSize();

            export.ScrollIndicator.Text3 = "  +";

            // ------------------------------------------------------
            // Need the first value from the former group as a filter
            // ------------------------------------------------------
            foreach(var item in ReadLocateRequest8())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index == 0)
              {
                export.ScrollIndicator.Text3 = "- +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                --export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          case "NEXT":
            export.Locl.Index = -1;

            // -----------------------------------------------------
            // Need the last value from the former group as a filter
            // -----------------------------------------------------
            export.ScrollIndicator.Text3 = "-";

            foreach(var item in ReadLocateRequest14())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index + 1 == Export.LoclGroup.Capacity)
              {
                export.ScrollIndicator.Text3 = "- +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                ++export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_COMMAND";

            break;
        }
      }
      else if (!IsEmpty(import.LoclFilterCsePerson.Number) && Equal
        (import.LoclFilterDateWorkArea.Date, local.Null1.Date))
      {
        switch(TrimEnd(global.Command))
        {
          case "DISPLAY":
            export.Locl.Index = -1;

            foreach(var item in ReadLocateRequest13())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index + 1 == Export.LoclGroup.Capacity)
              {
                export.ScrollIndicator.Text3 = "  +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                ++export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          case "PREV":
            export.Locl.Index = Export.LoclGroup.Capacity;
            export.Locl.CheckSize();

            export.ScrollIndicator.Text3 = "  +";

            // ------------------------------------------------------
            // Need the first value from the former group as a filter
            // ------------------------------------------------------
            foreach(var item in ReadLocateRequest6())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index == 0)
              {
                export.ScrollIndicator.Text3 = "- +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                --export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          case "NEXT":
            export.Locl.Index = -1;

            // -----------------------------------------------------
            // Need the last value from the former group as a filter
            // -----------------------------------------------------
            export.ScrollIndicator.Text3 = "-";

            foreach(var item in ReadLocateRequest10())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index + 1 == Export.LoclGroup.Capacity)
              {
                export.ScrollIndicator.Text3 = "- +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                ++export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_COMMAND";

            break;
        }
      }
      else if (IsEmpty(import.LoclFilterCsePerson.Number) && !
        Equal(import.LoclFilterDateWorkArea.Date, local.Null1.Date))
      {
        switch(TrimEnd(global.Command))
        {
          case "DISPLAY":
            export.Locl.Index = -1;

            foreach(var item in ReadLocateRequest15())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index + 1 == Export.LoclGroup.Capacity)
              {
                export.ScrollIndicator.Text3 = "  +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                ++export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          case "PREV":
            export.Locl.Index = Export.LoclGroup.Capacity;
            export.Locl.CheckSize();

            export.ScrollIndicator.Text3 = "  +";

            // ------------------------------------------------------
            // Need the first value from the former group as a filter
            // ------------------------------------------------------
            foreach(var item in ReadLocateRequest7())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index == 0)
              {
                export.ScrollIndicator.Text3 = "- +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                --export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          case "NEXT":
            export.Locl.Index = -1;

            // -----------------------------------------------------
            // Need the last value from the former group as a filter
            // -----------------------------------------------------
            export.ScrollIndicator.Text3 = "-";

            foreach(var item in ReadLocateRequest12())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index + 1 == Export.LoclGroup.Capacity)
              {
                export.ScrollIndicator.Text3 = "- +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                ++export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_COMMAND";

            break;
        }
      }
      else
      {
        // --------------------------------------------------------
        // Both filters are being used.
        // --------------------------------------------------------
        switch(TrimEnd(global.Command))
        {
          case "DISPLAY":
            export.Locl.Index = -1;

            foreach(var item in ReadLocateRequest11())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index + 1 == Export.LoclGroup.Capacity)
              {
                export.ScrollIndicator.Text3 = "  +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                ++export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          case "PREV":
            export.Locl.Index = Export.LoclGroup.Capacity;
            export.Locl.CheckSize();

            export.ScrollIndicator.Text3 = "  +";

            // ------------------------------------------------------
            // Need the first value from the former group as a filter
            // ------------------------------------------------------
            foreach(var item in ReadLocateRequest5())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index == 0)
              {
                export.ScrollIndicator.Text3 = "- +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                --export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          case "NEXT":
            export.Locl.Index = -1;

            // -----------------------------------------------------
            // Need the last value from the former group as a filter
            // -----------------------------------------------------
            export.ScrollIndicator.Text3 = "-";

            foreach(var item in ReadLocateRequest9())
            {
              // ---------------
              // check scrolling
              // ---------------
              if (export.Locl.Index + 1 == Export.LoclGroup.Capacity)
              {
                export.ScrollIndicator.Text3 = "- +";

                return;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.CsePersonsWorkSet.Number =
                entities.LocateRequest.CsePersonNumber;

              // ---------------------------
              // Insert SI READ CSE PERSON
              // ---------------------------
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                // -------------------------------------------
                // Move the formatted name and LOCATE REQUEST
                // information into the group view
                // -------------------------------------------
                ++export.Locl.Index;
                export.Locl.CheckSize();

                MoveLocateRequest3(entities.LocateRequest,
                  export.Locl.Update.LoclLocateRequest);
                export.Locl.Update.LoclCsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_COMMAND";

            break;
        }
      }
    }
    else
    {
      // ----------------------------------------------------
      // From LOCA - display BOTH returned and non-returned
      // LOCATE REQUESTS
      // ----------------------------------------------------
      // -------------------------------------------------
      // Check if an unneccessary READ EACH can be skipped
      // -------------------------------------------------
      export.Loca.Index = -1;

      for(import.Loca.Index = 0; import.Loca.Index < import.Loca.Count; ++
        import.Loca.Index)
      {
        if (!import.Loca.CheckSize())
        {
          break;
        }

        ++export.Loca.Index;
        export.Loca.CheckSize();

        export.Loca.Update.LocaCommon.SelectChar =
          export.Loca.Item.LocaCommon.SelectChar;
        export.Loca.Update.LocaCase.Number = import.Loca.Item.LocaCase.Number;
      }

      import.Loca.CheckIndex();

      if (export.Loca.Index == -1)
      {
        export.NoCases.Flag = "Y";
      }

      // Find all cases that are associated to the current user.
      foreach(var item in ReadCase1())
      {
        export.Loca.Index = 1 + export.Loca.Index + 1 - 1;
        export.Loca.CheckSize();

        export.Loca.Update.LocaCase.Number = entities.Case1.Number;

        if (export.Loca.Index + 1 == Export.LocaGroup.Capacity)
        {
          break;
        }

        export.NoCases.Flag = "";
      }

      // Find all cases that are not associated to the current user, if there's 
      // still room in the group.
      if (export.Loca.Index + 1 < Export.LocaGroup.Capacity)
      {
        foreach(var item in ReadCase2())
        {
          export.Loca.Index = 1 + export.Loca.Index + 1 - 1;
          export.Loca.CheckSize();

          export.Loca.Update.LocaCase.Number = entities.Case1.Number;

          if (export.Loca.Index + 1 == Export.LocaGroup.Capacity)
          {
            break;
          }

          export.NoCases.Flag = "";
        }
      }

      if (AsChar(export.NoCases.Flag) == 'Y')
      {
        if (ReadCsePerson())
        {
          if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
          {
            export.CsePerson.Assign(entities.CsePerson);
            ExitState = "SC0000_DATA_NOT_DISPLAY_FOR_FV";

            return;
          }
        }
      }

      switch(TrimEnd(global.Command))
      {
        case "PREV":
          local.Find.Assign(import.LocaPrevDisplay);

          break;
        case "NEXT":
          local.Find.Assign(import.LocaNextDisplay);

          break;
        default:
          // -------
          // DISPLAY
          // -------
          if (Equal(local.Previous.Command, "LOCA"))
          {
            // -----------------------------------
            // information was brought in via LOCL
            // -----------------------------------
            MoveLocateRequest4(import.LocateRequest, local.Find);
          }
          else
          {
            // ------------------------------------------------
            // USER DISPLAYing from LOCA for a DISPLAY inquiry
            // ------------------------------------------------
            if (ReadLocateRequest3())
            {
              MoveLocateRequest4(entities.LocateRequest, local.Find);
            }

            if (!entities.LocateRequest.Populated)
            {
              ExitState = "OE0000_LOCATE_REQUEST_NF";

              return;
            }
          }

          break;
      }

      if (ReadLocateRequest4())
      {
        export.LocateRequest.Assign(entities.LocateRequest);
        MoveLocateRequest1(entities.LocateRequest, local.High);
        MoveLocateRequest1(entities.LocateRequest, local.Low);
      }
      else
      {
        ExitState = "OE0000_LOCATE_REQUEST_NF";

        return;
      }

      // ------------------------------------------------
      // Decode the agency number to show the agency name
      // ------------------------------------------------
      local.AgencyNumber.Id =
        (int)StringToNumber(export.LocateRequest.AgencyNumber);

      if (ReadCodeCodeValue())
      {
        export.LocateRequestSource.Text25 = entities.CodeValue.Description;
      }
      else
      {
        ExitState = "CODE_VALUE_NF";

        return;
      }

      // --7/11/2000 GVandy. NEW MORE INDICATOR LOGIC.
      // ---------------------------------------
      // Determine scrolling indicators for LOCA
      // ---------------------------------------
      export.ScrollIndicator.Text3 = "";

      // -------
      // LESSER
      // -------
      if (ReadLocateRequest1())
      {
        MoveLocateRequest4(entities.LocateRequest, export.LocaPrevDisplay);
        export.ScrollIndicator.Text3 = "-";
      }

      // -------
      // GREATER
      // -------
      if (ReadLocateRequest2())
      {
        MoveLocateRequest2(entities.LocateRequest, export.LocaNextDisplay);
        export.ScrollIndicator.Text3 =
          Substring(export.ScrollIndicator.Text3, WorkArea.Text3_MaxLength, 1, 2)
          + "+";
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveLocateRequest1(LocateRequest source,
    LocateRequest target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.RequestDate = source.RequestDate;
    target.ResponseDate = source.ResponseDate;
    target.AgencyNumber = source.AgencyNumber;
    target.SequenceNumber = source.SequenceNumber;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveLocateRequest2(LocateRequest source,
    LocateRequest target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.RequestDate = source.RequestDate;
    target.AgencyNumber = source.AgencyNumber;
    target.SequenceNumber = source.SequenceNumber;
  }

  private static void MoveLocateRequest3(LocateRequest source,
    LocateRequest target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.ResponseDate = source.ResponseDate;
    target.AgencyNumber = source.AgencyNumber;
    target.SequenceNumber = source.SequenceNumber;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveLocateRequest4(LocateRequest source,
    LocateRequest target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.AgencyNumber = source.AgencyNumber;
    target.SequenceNumber = source.SequenceNumber;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCase1()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCodeCodeValue()
  {
    entities.CodeValue.Populated = false;
    entities.Code.Populated = false;

    return Read("ReadCodeCodeValue",
      (db, command) =>
      {
        db.SetString(
          command, "agencyNumber", entities.LocateRequest.AgencyNumber);
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.CodeValue.Id = db.GetInt32(reader, 2);
        entities.CodeValue.Cdvalue = db.GetString(reader, 3);
        entities.CodeValue.Description = db.GetString(reader, 4);
        entities.CodeValue.Populated = true;
        entities.Code.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadLocateRequest1()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest1",
      (db, command) =>
      {
        db.SetString(command, "csePersonNumber", export.CsePerson.Number);
        db.SetNullableDate(
          command, "responseDate", local.Low.ResponseDate.GetValueOrDefault());
        db.SetString(command, "agencyNumber", local.Low.AgencyNumber);
        db.SetInt32(command, "sequenceNumber", local.Low.SequenceNumber);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest10()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest10",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate1", local.Null1.Date.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber1", import.LoclFilterCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate2",
          import.LoclScrollingValue.ResponseDate.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber2",
          import.LoclScrollingValue.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", import.LoclScrollingValue.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", import.LoclScrollingValue.SequenceNumber);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest11()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest11",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate1",
          import.LoclFilterDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate2", local.Null1.Date.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber", import.LoclFilterCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest12()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest12",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate1",
          import.LoclFilterDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate2", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate3",
          import.LoclScrollingValue.ResponseDate.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber",
          import.LoclScrollingValue.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", import.LoclScrollingValue.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", import.LoclScrollingValue.SequenceNumber);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest13()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest13",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate", local.Null1.Date.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber", import.LoclFilterCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest14()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest14",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate1", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate2",
          import.LoclScrollingValue.ResponseDate.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber",
          import.LoclScrollingValue.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", import.LoclScrollingValue.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", import.LoclScrollingValue.SequenceNumber);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest15()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest15",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate1",
          import.LoclFilterDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate2", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest16()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest16",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadLocateRequest2()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest2",
      (db, command) =>
      {
        db.SetString(command, "csePersonNumber", export.CsePerson.Number);
        db.SetNullableDate(
          command, "responseDate", local.High.ResponseDate.GetValueOrDefault());
          
        db.SetString(command, "agencyNumber", local.High.AgencyNumber);
        db.SetInt32(command, "sequenceNumber", local.High.SequenceNumber);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;
      });
  }

  private bool ReadLocateRequest3()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest3",
      (db, command) =>
      {
        db.SetString(command, "csePersonNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;
      });
  }

  private bool ReadLocateRequest4()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest4",
      (db, command) =>
      {
        db.SetString(command, "csePersonNumber", export.CsePerson.Number);
        db.SetString(command, "agencyNumber", local.Find.AgencyNumber);
        db.SetInt32(command, "sequenceNumber", local.Find.SequenceNumber);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest5()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest5",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate1",
          import.LoclFilterDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate2", local.Null1.Date.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber1", import.LoclFilterCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate3",
          import.LoclScrollingValue.ResponseDate.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber2",
          import.LoclScrollingValue.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", import.LoclScrollingValue.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", import.LoclScrollingValue.SequenceNumber);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest6()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest6",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate1", local.Null1.Date.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber1", import.LoclFilterCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate2",
          import.LoclScrollingValue.ResponseDate.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber2",
          import.LoclScrollingValue.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", import.LoclScrollingValue.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", import.LoclScrollingValue.SequenceNumber);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest7()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest7",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate1",
          import.LoclFilterDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate2", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate3",
          import.LoclScrollingValue.ResponseDate.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber",
          import.LoclScrollingValue.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", import.LoclScrollingValue.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", import.LoclScrollingValue.SequenceNumber);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest8()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest8",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate1", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate2",
          import.LoclScrollingValue.ResponseDate.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber",
          import.LoclScrollingValue.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", import.LoclScrollingValue.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", import.LoclScrollingValue.SequenceNumber);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest9()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest9",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "responseDate1",
          import.LoclFilterDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate2", local.Null1.Date.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber1", import.LoclFilterCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "responseDate3",
          import.LoclScrollingValue.ResponseDate.GetValueOrDefault());
        db.SetString(
          command, "csePersonNumber2",
          import.LoclScrollingValue.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", import.LoclScrollingValue.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", import.LoclScrollingValue.SequenceNumber);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
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
    /// <summary>A LocaGroup group.</summary>
    [Serializable]
    public class LocaGroup
    {
      /// <summary>
      /// A value of LocaCommon.
      /// </summary>
      [JsonPropertyName("locaCommon")]
      public Common LocaCommon
      {
        get => locaCommon ??= new();
        set => locaCommon = value;
      }

      /// <summary>
      /// A value of LocaCase.
      /// </summary>
      [JsonPropertyName("locaCase")]
      public Case1 LocaCase
      {
        get => locaCase ??= new();
        set => locaCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common locaCommon;
      private Case1 locaCase;
    }

    /// <summary>
    /// A value of NoCases.
    /// </summary>
    [JsonPropertyName("noCases")]
    public Common NoCases
    {
      get => noCases ??= new();
      set => noCases = value;
    }

    /// <summary>
    /// A value of LocaPrevDisplay.
    /// </summary>
    [JsonPropertyName("locaPrevDisplay")]
    public LocateRequest LocaPrevDisplay
    {
      get => locaPrevDisplay ??= new();
      set => locaPrevDisplay = value;
    }

    /// <summary>
    /// A value of LocaNextDisplay.
    /// </summary>
    [JsonPropertyName("locaNextDisplay")]
    public LocateRequest LocaNextDisplay
    {
      get => locaNextDisplay ??= new();
      set => locaNextDisplay = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// Gets a value of Loca.
    /// </summary>
    [JsonIgnore]
    public Array<LocaGroup> Loca => loca ??= new(LocaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Loca for json serialization.
    /// </summary>
    [JsonPropertyName("loca")]
    [Computed]
    public IList<LocaGroup> Loca_Json
    {
      get => loca;
      set => Loca.Assign(value);
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

    /// <summary>
    /// A value of LoclFilterDateWorkArea.
    /// </summary>
    [JsonPropertyName("loclFilterDateWorkArea")]
    public DateWorkArea LoclFilterDateWorkArea
    {
      get => loclFilterDateWorkArea ??= new();
      set => loclFilterDateWorkArea = value;
    }

    /// <summary>
    /// A value of LoclFilterCsePerson.
    /// </summary>
    [JsonPropertyName("loclFilterCsePerson")]
    public CsePerson LoclFilterCsePerson
    {
      get => loclFilterCsePerson ??= new();
      set => loclFilterCsePerson = value;
    }

    /// <summary>
    /// A value of Loca1.
    /// </summary>
    [JsonPropertyName("loca1")]
    public CsePerson Loca1
    {
      get => loca1 ??= new();
      set => loca1 = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of LoclScrollingValue.
    /// </summary>
    [JsonPropertyName("loclScrollingValue")]
    public LocateRequest LoclScrollingValue
    {
      get => loclScrollingValue ??= new();
      set => loclScrollingValue = value;
    }

    private Common noCases;
    private LocateRequest locaPrevDisplay;
    private LocateRequest locaNextDisplay;
    private Common previous;
    private Array<LocaGroup> loca;
    private LocateRequest locateRequest;
    private DateWorkArea loclFilterDateWorkArea;
    private CsePerson loclFilterCsePerson;
    private CsePerson loca1;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private LocateRequest loclScrollingValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A LocaGroup group.</summary>
    [Serializable]
    public class LocaGroup
    {
      /// <summary>
      /// A value of LocaCommon.
      /// </summary>
      [JsonPropertyName("locaCommon")]
      public Common LocaCommon
      {
        get => locaCommon ??= new();
        set => locaCommon = value;
      }

      /// <summary>
      /// A value of LocaCase.
      /// </summary>
      [JsonPropertyName("locaCase")]
      public Case1 LocaCase
      {
        get => locaCase ??= new();
        set => locaCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common locaCommon;
      private Case1 locaCase;
    }

    /// <summary>A LoclGroup group.</summary>
    [Serializable]
    public class LoclGroup
    {
      /// <summary>
      /// A value of LoclCommon.
      /// </summary>
      [JsonPropertyName("loclCommon")]
      public Common LoclCommon
      {
        get => loclCommon ??= new();
        set => loclCommon = value;
      }

      /// <summary>
      /// A value of LoclCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("loclCsePersonsWorkSet")]
      public CsePersonsWorkSet LoclCsePersonsWorkSet
      {
        get => loclCsePersonsWorkSet ??= new();
        set => loclCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of LoclLocateRequest.
      /// </summary>
      [JsonPropertyName("loclLocateRequest")]
      public LocateRequest LoclLocateRequest
      {
        get => loclLocateRequest ??= new();
        set => loclLocateRequest = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Common loclCommon;
      private CsePersonsWorkSet loclCsePersonsWorkSet;
      private LocateRequest loclLocateRequest;
    }

    /// <summary>
    /// A value of NoCases.
    /// </summary>
    [JsonPropertyName("noCases")]
    public Common NoCases
    {
      get => noCases ??= new();
      set => noCases = value;
    }

    /// <summary>
    /// A value of LocateRequestSource.
    /// </summary>
    [JsonPropertyName("locateRequestSource")]
    public WorkArea LocateRequestSource
    {
      get => locateRequestSource ??= new();
      set => locateRequestSource = value;
    }

    /// <summary>
    /// A value of LocaPrevDisplay.
    /// </summary>
    [JsonPropertyName("locaPrevDisplay")]
    public LocateRequest LocaPrevDisplay
    {
      get => locaPrevDisplay ??= new();
      set => locaPrevDisplay = value;
    }

    /// <summary>
    /// A value of LocaNextDisplay.
    /// </summary>
    [JsonPropertyName("locaNextDisplay")]
    public LocateRequest LocaNextDisplay
    {
      get => locaNextDisplay ??= new();
      set => locaNextDisplay = value;
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
    /// Gets a value of Loca.
    /// </summary>
    [JsonIgnore]
    public Array<LocaGroup> Loca => loca ??= new(LocaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Loca for json serialization.
    /// </summary>
    [JsonPropertyName("loca")]
    [Computed]
    public IList<LocaGroup> Loca_Json
    {
      get => loca;
      set => Loca.Assign(value);
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
    /// Gets a value of Locl.
    /// </summary>
    [JsonIgnore]
    public Array<LoclGroup> Locl => locl ??= new(LoclGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Locl for json serialization.
    /// </summary>
    [JsonPropertyName("locl")]
    [Computed]
    public IList<LoclGroup> Locl_Json
    {
      get => locl;
      set => Locl.Assign(value);
    }

    private Common noCases;
    private WorkArea locateRequestSource;
    private LocateRequest locaPrevDisplay;
    private LocateRequest locaNextDisplay;
    private WorkArea scrollIndicator;
    private Array<LocaGroup> loca;
    private LocateRequest locateRequest;
    private CsePerson csePerson;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<LoclGroup> locl;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Find.
    /// </summary>
    [JsonPropertyName("find")]
    public LocateRequest Find
    {
      get => find ??= new();
      set => find = value;
    }

    /// <summary>
    /// A value of WorkerForCase.
    /// </summary>
    [JsonPropertyName("workerForCase")]
    public Common WorkerForCase
    {
      get => workerForCase ??= new();
      set => workerForCase = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
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
    /// A value of High.
    /// </summary>
    [JsonPropertyName("high")]
    public LocateRequest High
    {
      get => high ??= new();
      set => high = value;
    }

    /// <summary>
    /// A value of Low.
    /// </summary>
    [JsonPropertyName("low")]
    public LocateRequest Low
    {
      get => low ??= new();
      set => low = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of LastDisplayed.
    /// </summary>
    [JsonPropertyName("lastDisplayed")]
    public LocateRequest LastDisplayed
    {
      get => lastDisplayed ??= new();
      set => lastDisplayed = value;
    }

    /// <summary>
    /// A value of AgencyNumber.
    /// </summary>
    [JsonPropertyName("agencyNumber")]
    public CodeValue AgencyNumber
    {
      get => agencyNumber ??= new();
      set => agencyNumber = value;
    }

    private LocateRequest find;
    private Common workerForCase;
    private Common previous;
    private Common errOnAdabasUnavailable;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LocateRequest high;
    private LocateRequest low;
    private DateWorkArea current;
    private DateWorkArea null1;
    private LocateRequest lastDisplayed;
    private CodeValue agencyNumber;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of SupervisorOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("supervisorOfficeServiceProvider")]
    public OfficeServiceProvider SupervisorOfficeServiceProvider
    {
      get => supervisorOfficeServiceProvider ??= new();
      set => supervisorOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SupervisorServiceProvider.
    /// </summary>
    [JsonPropertyName("supervisorServiceProvider")]
    public ServiceProvider SupervisorServiceProvider
    {
      get => supervisorServiceProvider ??= new();
      set => supervisorServiceProvider = value;
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
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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

    private OfficeServiceProvider supervisorOfficeServiceProvider;
    private ServiceProvider supervisorServiceProvider;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private CodeValue codeValue;
    private Code code;
    private LocateRequest locateRequest;
    private CsePerson csePerson;
    private OfficeServiceProvider officeServiceProvider;
    private CaseRole caseRole;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private Office office;
    private ServiceProvider serviceProvider;
  }
#endregion
}
