// Program: SP_CAB_CSLN_GET_NARR_DETAILS, ID: 370955999, model: 746.
// Short name: SWE00354
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CSLN_GET_NARR_DETAILS.
/// </summary>
[Serializable]
public partial class SpCabCslnGetNarrDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CSLN_GET_NARR_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCslnGetNarrDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCslnGetNarrDetails.
  /// </summary>
  public SpCabCslnGetNarrDetails(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************
    // Date: 06-17-2000.
    //  By : Sree Veettil.
    // 09/20/00  SWSRCHF H00104020 Set the READ attribute to 'Select Only' for 
    // singleton reads
    //                             Set the READ EACH attribute to 'Uncommitted/
    // Browse'
    // 11/08/00  SWSRCHF I00107257 Incorrect worker location shown on screen. 
    // Added check for
    //                             End Date equal to '2099-12-31' to READ's 
    // using
    //                             Office_Service_Provider
    // 03/10/04 SWDPABC  PR#165871 If the 'Show All' indicator on the CSLN 
    // screen is set to 'Y'
    //                             show only HIST records with a Status of 'H' 
    // or 'Q'.
    // *** Problem report I00107257
    // *** 11/08/00 SWSRCHF
    // 05/20/2008   LSS    PR320860 CQ1168  &  PR295785 CQ550
    //              Added OR condition to if statement so that the Event Title
    //              will display when it is at the beginning of a new page.
    //              (added local Entity View -  local_null_timestamp 
    // narrative_detail)
    //              Modified MOVE statement to move the local_prev_narr 
    // narrative detail
    //              instead of the local_inner_grp narrative detail so that when
    // the userid/office
    //              for a detail line is the first line of the next page it will
    // display.
    //              (The userid/office gets overridden by the next detail line 
    // in the local_inner_grp.)
    // 04/12/13 GVandy   CQ32829 Display infrastructure records in "E" (Error) 
    // status if the Event
    // 				Log to Diary = "Y".
    // 10/18/2016  JHarden  CQ50345  CSLN screen filtering by all event types.
    // 07/20/18  JHarden  CQ61838  Not all narratives are showing when using the
    // 'Y' filter on CSLN
    // ****************************************************
    local.Max.Date = new DateTime(2099, 12, 31);

    for(import.HiddenKeys.Index = 0; import.HiddenKeys.Index < import
      .HiddenKeys.Count; ++import.HiddenKeys.Index)
    {
      if (!import.HiddenKeys.CheckSize())
      {
        break;
      }

      export.HiddenKeys.Index = import.HiddenKeys.Index;
      export.HiddenKeys.CheckSize();

      export.HiddenKeys.Update.HiddenKeyInfrastructure.Assign(
        import.HiddenKeys.Item.GkeyInfrastructure);
      export.HiddenKeys.Update.HiddenKeyNarrativeDetail.Assign(
        import.HiddenKeys.Item.GkeyNarrativeDetail);
    }

    import.HiddenKeys.CheckIndex();

    export.HiddenKeys.Index = import.Hidden.PageNumber - 1;
    export.HiddenKeys.CheckSize();

    if (import.Hidden.PageNumber == 1)
    {
      export.HiddenKeys.Index = 0;
      export.HiddenKeys.CheckSize();

      export.HiddenKeys.Update.HiddenKeyInfrastructure.CreatedTimestamp =
        import.StartingDate.Timestamp;
    }
    else
    {
      export.HiddenKeys.Index = import.Hidden.PageNumber - 1;
      export.HiddenKeys.CheckSize();
    }

    local.Outer.Index = -1;
    export.Group.Index = -1;

    foreach(var item in ReadInfrastructure())
    {
      // 10/18/2016  JHarden  CQ50345  CSLN screen filtering by all event types.
      if (import.ExternalEvent.EventId > 0)
      {
        if (entities.ExistingInfrastructure.EventId != import
          .ExternalEvent.EventId)
        {
          continue;
        }
      }

      // 04/12/13 GVandy CQ32829 Display infrastructure records in "E" (Error) 
      // status if
      // the Event Log to Diary = "Y".
      if (!ReadEventDetail())
      {
        continue;
      }

      // CQ61838
      if (AsChar(entities.EventDetail.LogToDiaryInd) != 'Y')
      {
        continue;
      }

      if (AsChar(import.ShowAll.Flag) == 'Y')
      {
        if (AsChar(entities.ExistingInfrastructure.ProcessStatus) == 'E')
        {
        }
        else if (AsChar(entities.ExistingInfrastructure.ProcessStatus) != 'H'
          && AsChar(entities.ExistingInfrastructure.ProcessStatus) != 'Q')
        {
          // PR#165871 - 03/10/04 - Andrew Convery - IF statement below added to
          // code
          continue;
        }

        ++local.Outer.Index;
        local.Outer.CheckSize();

        if (local.Outer.Index >= Local.OuterGroup.Capacity)
        {
          break;
        }

        local.Outer.Item.Inner.Index = -1;
        local.Outer.Update.Outer1.Assign(entities.ExistingInfrastructure);
      }
      else if (AsChar(import.ShowAll.Flag) == 'N')
      {
        if (ReadNarrativeDetail1())
        {
          ++local.Outer.Index;
          local.Outer.CheckSize();

          if (local.Outer.Index >= Local.OuterGroup.Capacity)
          {
            break;
          }

          local.Outer.Item.Inner.Index = -1;
          local.Outer.Update.Outer1.Assign(entities.ExistingInfrastructure);
        }
        else
        {
          continue;
        }
      }

      local.Enf.Flag = "N";
      local.Est.Flag = "N";
      local.Common.Flag = "N";
      local.Med.Flag = "N";
      local.Pat.Flag = "N";
      local.ModRevEnf.Flag = "N";
      local.ModRevEst.Flag = "N";
      local.ModRev.Flag = "N";
      local.ModRevMed.Flag = "N";
      local.ModRevPat.Flag = "N";

      foreach(var item1 in ReadNarrativeDetail2())
      {
        if (Equal(entities.ExistingInfrastructure.CreatedTimestamp,
          export.HiddenKeys.Item.HiddenKeyInfrastructure.CreatedTimestamp))
        {
          if (Lt(entities.ExistingNarrativeDetail.CreatedTimestamp,
            export.HiddenKeys.Item.HiddenKeyNarrativeDetail.CreatedTimestamp))
          {
            continue;
          }
          else if (Equal(entities.ExistingNarrativeDetail.CreatedTimestamp,
            export.HiddenKeys.Item.HiddenKeyNarrativeDetail.CreatedTimestamp))
          {
            if (entities.ExistingNarrativeDetail.LineNumber < export
              .HiddenKeys.Item.HiddenKeyNarrativeDetail.LineNumber)
            {
              continue;
            }
          }
        }

        ++local.Outer.Item.Inner.Index;
        local.Outer.Item.Inner.CheckSize();

        if (local.Outer.Item.Inner.Index >= Local.InnerGroup.Capacity)
        {
          goto ReadEach;
        }

        local.Outer.Update.Inner.Update.Inner1.Assign(
          entities.ExistingNarrativeDetail);

        if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 10,
          "LOCATE -- "))
        {
          if (AsChar(local.Common.Flag) == 'N')
          {
            local.Common.Flag = "Y";
          }
          else
          {
            local.Outer.Update.Inner.Update.Inner1.NarrativeText =
              Substring(entities.ExistingNarrativeDetail.NarrativeText, 11, 58);
              
          }

          continue;
        }

        if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 11,
          "MEDICAL -- "))
        {
          if (AsChar(local.Med.Flag) == 'N')
          {
            local.Med.Flag = "Y";
          }
          else
          {
            local.Outer.Update.Inner.Update.Inner1.NarrativeText =
              Substring(entities.ExistingNarrativeDetail.NarrativeText, 12, 57);
              
          }

          continue;
        }

        if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 13,
          "PATERNITY -- "))
        {
          if (AsChar(local.Pat.Flag) == 'N')
          {
            local.Pat.Flag = "Y";
          }
          else
          {
            local.Outer.Update.Inner.Update.Inner1.NarrativeText =
              Substring(entities.ExistingNarrativeDetail.NarrativeText, 14, 55);
              
          }

          continue;
        }

        if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 17,
          "ESTABLISHMENT -- "))
        {
          if (AsChar(local.Est.Flag) == 'N')
          {
            local.Est.Flag = "Y";
          }
          else
          {
            local.Outer.Update.Inner.Update.Inner1.NarrativeText =
              Substring(entities.ExistingNarrativeDetail.NarrativeText, 18, 51);
              
          }

          continue;
        }

        if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 15,
          "ENFORCEMENT -- "))
        {
          if (AsChar(local.Enf.Flag) == 'N')
          {
            local.Enf.Flag = "Y";
          }
          else
          {
            local.Outer.Update.Inner.Update.Inner1.NarrativeText =
              Substring(entities.ExistingNarrativeDetail.NarrativeText, 16, 53);
              
          }

          continue;
        }

        if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 21,
          "MOD REVIEW LOCATE -- "))
        {
          if (AsChar(local.ModRev.Flag) == 'N')
          {
            local.ModRev.Flag = "Y";
          }
          else
          {
            local.Outer.Update.Inner.Update.Inner1.NarrativeText =
              Substring(entities.ExistingNarrativeDetail.NarrativeText, 22, 47);
              
          }

          continue;
        }

        if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 22,
          "MOD REVIEW MEDICAL -- "))
        {
          if (AsChar(local.ModRevMed.Flag) == 'N')
          {
            local.ModRevMed.Flag = "Y";
          }
          else
          {
            local.Outer.Update.Inner.Update.Inner1.NarrativeText =
              Substring(entities.ExistingNarrativeDetail.NarrativeText, 23, 46);
              
          }

          continue;
        }

        if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 24,
          "MOD REVIEW PATERNITY -- "))
        {
          if (AsChar(local.ModRevPat.Flag) == 'N')
          {
            local.ModRevPat.Flag = "Y";
          }
          else
          {
            local.Outer.Update.Inner.Update.Inner1.NarrativeText =
              Substring(entities.ExistingNarrativeDetail.NarrativeText, 25, 44);
              
          }

          continue;
        }

        if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 28,
          "MOD REVIEW ESTABLISHMENT -- "))
        {
          if (AsChar(local.ModRevEst.Flag) == 'N')
          {
            local.ModRevEst.Flag = "Y";
          }
          else
          {
            local.Outer.Update.Inner.Update.Inner1.NarrativeText =
              Substring(entities.ExistingNarrativeDetail.NarrativeText, 29, 40);
              
          }

          continue;
        }

        if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 26,
          "MOD REVIEW ENFORCEMENT -- "))
        {
          if (AsChar(local.ModRevEnf.Flag) == 'N')
          {
            local.ModRevEnf.Flag = "Y";
          }
          else
          {
            local.Outer.Update.Inner.Update.Inner1.NarrativeText =
              Substring(entities.ExistingNarrativeDetail.NarrativeText, 27, 42);
              
          }

          continue;
        }
      }
    }

ReadEach:

    // *******************************************************************
    // Move all the elements from a two dimentional group view to one big group 
    // view. Set  show all fla to Y, whenever we have narrative detail for the
    // color manipulation.
    // *******************************************************************
    for(local.Outer.Index = 0; local.Outer.Index < local.Outer.Count; ++
      local.Outer.Index)
    {
      if (!local.Outer.CheckSize())
      {
        break;
      }

      ++export.Group.Index;
      export.Group.CheckSize();

      if (export.Group.Index >= Export.GroupGroup.Capacity)
      {
        if (export.HiddenKeys.Index + 1 < Export.HiddenKeysGroup.Capacity)
        {
          ++export.HiddenKeys.Index;
          export.HiddenKeys.CheckSize();

          export.HiddenKeys.Update.HiddenKeyInfrastructure.Assign(
            local.Outer.Item.Outer1);
        }
        else
        {
          ExitState = "SP0000_LIST_IS_FULL";
        }

        return;
      }

      export.Group.Update.DateWorkArea.Timestamp =
        local.Outer.Item.Outer1.CreatedTimestamp;

      if (local.Outer.Item.Outer1.SystemGeneratedIdentifier != export
        .HiddenKeys.Item.HiddenKeyInfrastructure.SystemGeneratedIdentifier)
      {
        export.Group.Update.Infrastructure.Assign(local.Outer.Item.Outer1);
        export.Group.Update.NarInd.Flag = "N";
        export.Group.Update.Display.NarrativeText =
          local.Outer.Item.Outer1.EventDetailName;
        export.Group.Update.Common.SelectChar = "_";
      }
      else if (local.Outer.Index == 0)
      {
        // -----------------------------------------------------------------------------------------------------
        // LSS   05/20/2008   PR320860 CQ1168  &  PR295785 CQ550
        //       Added OR condition to if statement so that the Event Title
        //       will display when it is at the beginning of a new page.
        // -----------------------------------------------------------------------------------------------------
        if (local.Outer.Index + 1 == local.Outer.Count || export
          .HiddenKeys.Index >= 1 && Equal
          (export.HiddenKeys.Item.HiddenKeyNarrativeDetail.CreatedTimestamp,
          local.NullTimestamp.CreatedTimestamp))
        {
          export.Group.Update.Infrastructure.Assign(local.Outer.Item.Outer1);
          export.Group.Update.NarInd.Flag = "N";
          export.Group.Update.Display.NarrativeText =
            local.Outer.Item.Outer1.EventDetailName;
          export.Group.Update.Common.SelectChar = "_";
        }
        else
        {
          export.Group.Index = -1;
        }
      }

      // ****************************************************
      // Date: 06-17-2000.
      // Developer: Sree Veettil
      // Description :  The indicator is set because on the screen user wants 
      // history records in
      // blue and Narrative details in blue color. If indicator is Y , occurance
      // in the group
      // view is from Narrative detail. In the main procedure the color is 
      // manipulated.
      // ****************************************************
      for(local.Outer.Item.Inner.Index = 0; local.Outer.Item.Inner.Index < local
        .Outer.Item.Inner.Count; ++local.Outer.Item.Inner.Index)
      {
        if (!local.Outer.Item.Inner.CheckSize())
        {
          break;
        }

        if (Equal(local.PrevNarr.CreatedTimestamp,
          local.Outer.Item.Inner.Item.Inner1.CreatedTimestamp) && local
          .PrevNarr.LineNumber != local
          .Outer.Item.Inner.Item.Inner1.LineNumber)
        {
          ++export.Group.Index;
          export.Group.CheckSize();

          if (export.Group.Index >= Export.GroupGroup.Capacity)
          {
            if (export.HiddenKeys.Index + 1 < Export.HiddenKeysGroup.Capacity)
            {
              ++export.HiddenKeys.Index;
              export.HiddenKeys.CheckSize();

              export.HiddenKeys.Update.HiddenKeyInfrastructure.Assign(
                local.Outer.Item.Outer1);
              export.HiddenKeys.Update.HiddenKeyNarrativeDetail.Assign(
                local.Outer.Item.Inner.Item.Inner1);
            }
            else
            {
              ExitState = "SP0000_LIST_IS_FULL";
            }

            return;
          }

          export.Group.Update.Display.NarrativeText =
            local.Outer.Item.Inner.Item.Inner1.NarrativeText ?? "";
          export.Group.Update.NarInd.Flag = "Y";
          export.Group.Update.Common.SelectChar = "";
          export.Group.Update.Infrastructure.Assign(local.Outer.Item.Outer1);
          export.Group.Update.NarrativeDetail.Assign(
            local.Outer.Item.Inner.Item.Inner1);
          local.PrevNarr.Assign(local.Outer.Item.Inner.Item.Inner1);
        }
        else if (!Equal(local.PrevNarr.CreatedTimestamp,
          local.Outer.Item.Inner.Item.Inner1.CreatedTimestamp))
        {
          if (local.Outer.Item.Inner.Index != 0)
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (export.Group.Index >= Export.GroupGroup.Capacity)
            {
              if (export.HiddenKeys.Index + 1 < Export.HiddenKeysGroup.Capacity)
              {
                ++export.HiddenKeys.Index;
                export.HiddenKeys.CheckSize();

                export.HiddenKeys.Update.HiddenKeyInfrastructure.Assign(
                  local.Outer.Item.Outer1);

                // ---------------------------------------------------------------------------------------------------------------------
                // LSS   05/20/2008   PR320860 CQ1168  &  PR295785 CQ550
                //       Modified MOVE statement to move the local_prev_narr 
                // narrative detail
                //       instead of the local_inner_grp narrative detail so that
                // when the userid/office
                //       for a detail line is the first line of the next page it
                // will display.
                //       (The userid/office gets overridden by the next detail 
                // line in the local_inner_grp.)
                // ----------------------------------------------------------------------------------------------------------------------
                export.HiddenKeys.Update.HiddenKeyNarrativeDetail.Assign(
                  local.PrevNarr);
              }
              else
              {
                ExitState = "SP0000_LIST_IS_FULL";
              }

              return;
            }

            // ******************************************************************
            // Read the office and Service provider and populate it to group 
            // view to display on the screen.
            // ******************************************************************
            if (ReadServiceProvider())
            {
              // *** Problem report I00107257
              // *** 11/08/00 SWSRCHF
              if (ReadOffice())
              {
                local.OfficeName.Text30 = entities.ExistingOffice.Name;
              }
              else
              {
                local.OfficeName.Text30 = "Office not found";
              }

              local.Name.Text30 =
                TrimEnd(entities.ExistingServiceProvider.FirstName) + " " + entities
                .ExistingServiceProvider.MiddleInitial;
              local.Name.Text30 = TrimEnd(local.Name.Text30) + " " + entities
                .ExistingServiceProvider.LastName;
              export.Group.Update.Display.NarrativeText =
                "                                -" + TrimEnd
                (local.Name.Text30);
              export.Group.Update.Display.NarrativeText =
                TrimEnd(export.Group.Item.Display.NarrativeText) + " , ";
              export.Group.Update.Display.NarrativeText =
                TrimEnd(export.Group.Item.Display.NarrativeText) + TrimEnd
                (local.OfficeName.Text30);
            }
            else
            {
              local.Name.Text30 = (local.PrevNarr.CreatedBy ?? "") + "not found";
                
              export.Group.Update.Display.NarrativeText =
                "                                -" + TrimEnd
                (local.Name.Text30);
            }

            export.Group.Update.NarInd.Flag = "Y";
            export.Group.Update.Infrastructure.Assign(local.Outer.Item.Outer1);
            export.Group.Update.NarrativeDetail.Assign(
              local.Outer.Item.Inner.Item.Inner1);
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          if (export.Group.Index >= Export.GroupGroup.Capacity)
          {
            if (export.HiddenKeys.Index + 1 < Export.HiddenKeysGroup.Capacity)
            {
              ++export.HiddenKeys.Index;
              export.HiddenKeys.CheckSize();

              export.HiddenKeys.Update.HiddenKeyInfrastructure.Assign(
                local.Outer.Item.Outer1);
              export.HiddenKeys.Update.HiddenKeyNarrativeDetail.Assign(
                local.Outer.Item.Inner.Item.Inner1);
            }
            else
            {
              ExitState = "SP0000_LIST_IS_FULL";
            }

            return;
          }

          export.Group.Update.DateWorkArea.Timestamp =
            local.Outer.Item.Inner.Item.Inner1.CreatedTimestamp;
          export.Group.Update.Display.NarrativeText =
            local.Outer.Item.Inner.Item.Inner1.NarrativeText ?? "";
          export.Group.Update.NarInd.Flag = "Y";
          export.Group.Update.Common.SelectChar = "";
          export.Group.Update.NarrativeDetail.Assign(
            local.Outer.Item.Inner.Item.Inner1);
          export.Group.Update.Infrastructure.Assign(local.Outer.Item.Outer1);
          local.PrevNarr.Assign(local.Outer.Item.Inner.Item.Inner1);
        }

        if (local.Outer.Item.Inner.Index + 1 == local.Outer.Item.Inner.Count)
        {
          ++export.Group.Index;
          export.Group.CheckSize();

          if (export.Group.Index >= Export.GroupGroup.Capacity)
          {
            if (export.HiddenKeys.Index + 1 < Export.HiddenKeysGroup.Capacity)
            {
              ++export.HiddenKeys.Index;
              export.HiddenKeys.CheckSize();

              export.HiddenKeys.Update.HiddenKeyInfrastructure.Assign(
                local.Outer.Item.Outer1);
              export.HiddenKeys.Update.HiddenKeyNarrativeDetail.Assign(
                local.Outer.Item.Inner.Item.Inner1);
            }
            else
            {
              ExitState = "SP0000_LIST_IS_FULL";
            }

            return;
          }

          // ******************************************************************
          // Read the office and Service provider and populate it to group view 
          // to display on the screen.
          // ******************************************************************
          if (ReadServiceProvider())
          {
            // *** Problem report I00107257
            // *** 11/08/00 SWSRCHF
            if (ReadOffice())
            {
              local.OfficeName.Text30 = entities.ExistingOffice.Name;
            }
            else
            {
              local.OfficeName.Text30 = "Office not found";
            }

            local.Name.Text30 =
              TrimEnd(entities.ExistingServiceProvider.FirstName) + " " + entities
              .ExistingServiceProvider.MiddleInitial;
            local.Name.Text30 = TrimEnd(local.Name.Text30) + " " + entities
              .ExistingServiceProvider.LastName;
            export.Group.Update.Display.NarrativeText =
              "                                -" + TrimEnd(local.Name.Text30);
            export.Group.Update.Display.NarrativeText =
              TrimEnd(export.Group.Item.Display.NarrativeText) + " , ";
            export.Group.Update.Display.NarrativeText =
              TrimEnd(export.Group.Item.Display.NarrativeText) + TrimEnd
              (local.OfficeName.Text30);
          }
          else
          {
            local.Name.Text30 = (local.PrevNarr.CreatedBy ?? "") + "not found";
            export.Group.Update.Display.NarrativeText =
              "                                -" + TrimEnd(local.Name.Text30);
          }

          export.Group.Update.Infrastructure.Assign(local.Outer.Item.Outer1);
          export.Group.Update.NarrativeDetail.Assign(
            local.Outer.Item.Inner.Item.Inner1);
          export.Group.Update.NarInd.Flag = "Y";
        }
      }

      local.Outer.Item.Inner.CheckIndex();
    }

    local.Outer.CheckIndex();
  }

  private bool ReadEventDetail()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.ExistingInfrastructure.EventId);
        db.SetString(
          command, "reasonCode", entities.ExistingInfrastructure.ReasonCode);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.ReasonCode = db.GetString(reader, 1);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 2);
        entities.EventDetail.EveNo = db.GetInt32(reader, 3);
        entities.EventDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.ExistingInfrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
        db.SetDateTime(
          command, "createdTimestamp",
          export.HiddenKeys.Item.HiddenKeyInfrastructure.CreatedTimestamp.
            GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp", import.LastDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.ExistingInfrastructure.EventId = db.GetInt32(reader, 2);
        entities.ExistingInfrastructure.EventType = db.GetString(reader, 3);
        entities.ExistingInfrastructure.EventDetailName =
          db.GetString(reader, 4);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 5);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 8);
        entities.ExistingInfrastructure.CreatedBy = db.GetString(reader, 9);
        entities.ExistingInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.ExistingInfrastructure.Function =
          db.GetNullableString(reader, 11);
        entities.ExistingInfrastructure.Populated = true;

        return true;
      });
  }

  private bool ReadNarrativeDetail1()
  {
    entities.ExistingNarrativeDetail.Populated = false;

    return Read("ReadNarrativeDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingNarrativeDetail.InfrastructureId =
          db.GetInt32(reader, 0);
        entities.ExistingNarrativeDetail.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingNarrativeDetail.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingNarrativeDetail.CaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingNarrativeDetail.NarrativeText =
          db.GetNullableString(reader, 4);
        entities.ExistingNarrativeDetail.LineNumber = db.GetInt32(reader, 5);
        entities.ExistingNarrativeDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail2()
  {
    entities.ExistingNarrativeDetail.Populated = false;

    return ReadEach("ReadNarrativeDetail2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", entities.ExistingInfrastructure.CaseNumber ?? ""
          );
        db.SetInt32(
          command, "infrastructureId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingNarrativeDetail.InfrastructureId =
          db.GetInt32(reader, 0);
        entities.ExistingNarrativeDetail.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingNarrativeDetail.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingNarrativeDetail.CaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingNarrativeDetail.NarrativeText =
          db.GetNullableString(reader, 4);
        entities.ExistingNarrativeDetail.LineNumber = db.GetInt32(reader, 5);
        entities.ExistingNarrativeDetail.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.Name = db.GetString(reader, 1);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 2);
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", local.PrevNarr.CreatedBy ?? "");
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
    /// <summary>A HiddenKeysGroup group.</summary>
    [Serializable]
    public class HiddenKeysGroup
    {
      /// <summary>
      /// A value of GkeyInfrastructure.
      /// </summary>
      [JsonPropertyName("gkeyInfrastructure")]
      public Infrastructure GkeyInfrastructure
      {
        get => gkeyInfrastructure ??= new();
        set => gkeyInfrastructure = value;
      }

      /// <summary>
      /// A value of GkeyNarrativeDetail.
      /// </summary>
      [JsonPropertyName("gkeyNarrativeDetail")]
      public NarrativeDetail GkeyNarrativeDetail
      {
        get => gkeyNarrativeDetail ??= new();
        set => gkeyNarrativeDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Infrastructure gkeyInfrastructure;
      private NarrativeDetail gkeyNarrativeDetail;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public Standard Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of LastDate.
    /// </summary>
    [JsonPropertyName("lastDate")]
    public DateWorkArea LastDate
    {
      get => lastDate ??= new();
      set => lastDate = value;
    }

    /// <summary>
    /// A value of StartingDate.
    /// </summary>
    [JsonPropertyName("startingDate")]
    public DateWorkArea StartingDate
    {
      get => startingDate ??= new();
      set => startingDate = value;
    }

    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
    }

    /// <summary>
    /// A value of ExternalEvent.
    /// </summary>
    [JsonPropertyName("externalEvent")]
    public Infrastructure ExternalEvent
    {
      get => externalEvent ??= new();
      set => externalEvent = value;
    }

    /// <summary>
    /// Gets a value of HiddenKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenKeysGroup> HiddenKeys => hiddenKeys ??= new(
      HiddenKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenKeys")]
    [Computed]
    public IList<HiddenKeysGroup> HiddenKeys_Json
    {
      get => hiddenKeys;
      set => HiddenKeys.Assign(value);
    }

    private Standard hidden;
    private Case1 case1;
    private DateWorkArea lastDate;
    private DateWorkArea startingDate;
    private Common showAll;
    private Infrastructure externalEvent;
    private Array<HiddenKeysGroup> hiddenKeys;
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
      /// A value of Infrastructure.
      /// </summary>
      [JsonPropertyName("infrastructure")]
      public Infrastructure Infrastructure
      {
        get => infrastructure ??= new();
        set => infrastructure = value;
      }

      /// <summary>
      /// A value of NarrativeDetail.
      /// </summary>
      [JsonPropertyName("narrativeDetail")]
      public NarrativeDetail NarrativeDetail
      {
        get => narrativeDetail ??= new();
        set => narrativeDetail = value;
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
      /// A value of Display.
      /// </summary>
      [JsonPropertyName("display")]
      public NarrativeDetail Display
      {
        get => display ??= new();
        set => display = value;
      }

      /// <summary>
      /// A value of NarInd.
      /// </summary>
      [JsonPropertyName("narInd")]
      public Common NarInd
      {
        get => narInd ??= new();
        set => narInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private Common common;
      private Infrastructure infrastructure;
      private NarrativeDetail narrativeDetail;
      private DateWorkArea dateWorkArea;
      private NarrativeDetail display;
      private Common narInd;
    }

    /// <summary>A HiddenKeysGroup group.</summary>
    [Serializable]
    public class HiddenKeysGroup
    {
      /// <summary>
      /// A value of HiddenKeyInfrastructure.
      /// </summary>
      [JsonPropertyName("hiddenKeyInfrastructure")]
      public Infrastructure HiddenKeyInfrastructure
      {
        get => hiddenKeyInfrastructure ??= new();
        set => hiddenKeyInfrastructure = value;
      }

      /// <summary>
      /// A value of HiddenKeyNarrativeDetail.
      /// </summary>
      [JsonPropertyName("hiddenKeyNarrativeDetail")]
      public NarrativeDetail HiddenKeyNarrativeDetail
      {
        get => hiddenKeyNarrativeDetail ??= new();
        set => hiddenKeyNarrativeDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Infrastructure hiddenKeyInfrastructure;
      private NarrativeDetail hiddenKeyNarrativeDetail;
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
    /// Gets a value of HiddenKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenKeysGroup> HiddenKeys => hiddenKeys ??= new(
      HiddenKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenKeys")]
    [Computed]
    public IList<HiddenKeysGroup> HiddenKeys_Json
    {
      get => hiddenKeys;
      set => HiddenKeys.Assign(value);
    }

    private Array<GroupGroup> group;
    private Array<HiddenKeysGroup> hiddenKeys;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A OuterGroup group.</summary>
    [Serializable]
    public class OuterGroup
    {
      /// <summary>
      /// A value of Outer1.
      /// </summary>
      [JsonPropertyName("outer1")]
      public Infrastructure Outer1
      {
        get => outer1 ??= new();
        set => outer1 = value;
      }

      /// <summary>
      /// Gets a value of Inner.
      /// </summary>
      [JsonIgnore]
      public Array<InnerGroup> Inner => inner ??= new(InnerGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of Inner for json serialization.
      /// </summary>
      [JsonPropertyName("inner")]
      [Computed]
      public IList<InnerGroup> Inner_Json
      {
        get => inner;
        set => Inner.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Infrastructure outer1;
      private Array<InnerGroup> inner;
    }

    /// <summary>A InnerGroup group.</summary>
    [Serializable]
    public class InnerGroup
    {
      /// <summary>
      /// A value of Inner1.
      /// </summary>
      [JsonPropertyName("inner1")]
      public NarrativeDetail Inner1
      {
        get => inner1 ??= new();
        set => inner1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private NarrativeDetail inner1;
    }

    /// <summary>
    /// A value of NullTimestamp.
    /// </summary>
    [JsonPropertyName("nullTimestamp")]
    public NarrativeDetail NullTimestamp
    {
      get => nullTimestamp ??= new();
      set => nullTimestamp = value;
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
    /// A value of OfficeName.
    /// </summary>
    [JsonPropertyName("officeName")]
    public TextWorkArea OfficeName
    {
      get => officeName ??= new();
      set => officeName = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public TextWorkArea Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of PrevNarr.
    /// </summary>
    [JsonPropertyName("prevNarr")]
    public NarrativeDetail PrevNarr
    {
      get => prevNarr ??= new();
      set => prevNarr = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// Gets a value of Outer.
    /// </summary>
    [JsonIgnore]
    public Array<OuterGroup> Outer => outer ??= new(OuterGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Outer for json serialization.
    /// </summary>
    [JsonPropertyName("outer")]
    [Computed]
    public IList<OuterGroup> Outer_Json
    {
      get => outer;
      set => Outer.Assign(value);
    }

    /// <summary>
    /// A value of Enf.
    /// </summary>
    [JsonPropertyName("enf")]
    public Common Enf
    {
      get => enf ??= new();
      set => enf = value;
    }

    /// <summary>
    /// A value of Est.
    /// </summary>
    [JsonPropertyName("est")]
    public Common Est
    {
      get => est ??= new();
      set => est = value;
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
    /// A value of Med.
    /// </summary>
    [JsonPropertyName("med")]
    public Common Med
    {
      get => med ??= new();
      set => med = value;
    }

    /// <summary>
    /// A value of Pat.
    /// </summary>
    [JsonPropertyName("pat")]
    public Common Pat
    {
      get => pat ??= new();
      set => pat = value;
    }

    /// <summary>
    /// A value of ModRevEnf.
    /// </summary>
    [JsonPropertyName("modRevEnf")]
    public Common ModRevEnf
    {
      get => modRevEnf ??= new();
      set => modRevEnf = value;
    }

    /// <summary>
    /// A value of ModRevEst.
    /// </summary>
    [JsonPropertyName("modRevEst")]
    public Common ModRevEst
    {
      get => modRevEst ??= new();
      set => modRevEst = value;
    }

    /// <summary>
    /// A value of ModRev.
    /// </summary>
    [JsonPropertyName("modRev")]
    public Common ModRev
    {
      get => modRev ??= new();
      set => modRev = value;
    }

    /// <summary>
    /// A value of ModRevMed.
    /// </summary>
    [JsonPropertyName("modRevMed")]
    public Common ModRevMed
    {
      get => modRevMed ??= new();
      set => modRevMed = value;
    }

    /// <summary>
    /// A value of ModRevPat.
    /// </summary>
    [JsonPropertyName("modRevPat")]
    public Common ModRevPat
    {
      get => modRevPat ??= new();
      set => modRevPat = value;
    }

    private NarrativeDetail nullTimestamp;
    private DateWorkArea max;
    private TextWorkArea officeName;
    private TextWorkArea name;
    private NarrativeDetail prevNarr;
    private DateWorkArea start;
    private Array<OuterGroup> outer;
    private Common enf;
    private Common est;
    private Common common;
    private Common med;
    private Common pat;
    private Common modRevEnf;
    private Common modRevEst;
    private Common modRev;
    private Common modRevMed;
    private Common modRevPat;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
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

    /// <summary>
    /// A value of ExistingNarrativeDetail.
    /// </summary>
    [JsonPropertyName("existingNarrativeDetail")]
    public NarrativeDetail ExistingNarrativeDetail
    {
      get => existingNarrativeDetail ??= new();
      set => existingNarrativeDetail = value;
    }

    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    private Event1 event1;
    private EventDetail eventDetail;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private ServiceProvider existingServiceProvider;
    private Office existingOffice;
    private NarrativeDetail existingNarrativeDetail;
    private Infrastructure existingInfrastructure;
  }
#endregion
}
