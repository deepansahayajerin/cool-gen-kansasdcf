// Program: SP_CRIN_DISPLAY_CLOSED_CASE, ID: 372646309, model: 746.
// Short name: SWE02090
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CRIN_DISPLAY_CLOSED_CASE.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// This action block reads data for a closed case.  Case Role and Person 
/// participation is based upon the case composition at the time the AR was end
/// dated for case closure.
/// </para>
/// </summary>
[Serializable]
public partial class SpCrinDisplayClosedCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CRIN_DISPLAY_CLOSED_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCrinDisplayClosedCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCrinDisplayClosedCase.
  /// </summary>
  public SpCrinDisplayClosedCase(IContext context, Import import, Export export):
    
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
    //        M A I N T E N A N C E   L O G
    //  Date    Developer      Req #    	Description
    // 07/22/97 R. Grey	IDCR 356 	Initial Code
    // 04/22/99 N.Engoor                       Added code for displaying notes 
    // correctly.
    // 08/08/2000 SWSRCHF WR# 000170  Replace the read for Narrative by a read 
    // for
    //                                
    // Narrative Detail
    // 02/25/2002	M Ramirez	PR139864
    // Added 'last rvw resulted in mod' flag
    // 04/01/2011	T Pierce	CQ# 23212	Removed references to obsolete Narrative 
    // entity type.
    // ---------------------------------------------
    // 06/16/11  RMathews   CQ27977  Moved code so that a missing AP role 
    // wouldn't prevent processing.
    export.Case2.Number = import.Crme.Number;
    export.HiddenReviewType.ActionEntry = import.HiddenReviewType.ActionEntry;
    export.CommandPassedFromEnfor.Flag = import.CommandPassedFromEnfor.Flag;
    local.ExitStateFlag.ExitOfficeNf.Flag = "N";
    local.ExitStateFlag.ExitCaseAssignNf.Flag = "N";

    if (ReadCase())
    {
      export.Case2.Assign(entities.ClosedCase);
      local.CaseClosedDate.Date = entities.ClosedCase.StatusDate;
      local.NoOfApOnCase.Count = 0;

      // ************************************************
      // Review of a closed case will display a 'snapshot' of the case
      // construction on the day prior to case closure.
      // ************************************************
      foreach(var item in ReadCsePersonCaseRole())
      {
        ++local.NoOfApOnCase.Count;

        if (local.NoOfApOnCase.Count == 1)
        {
          local.Ap1.Number = entities.CsePerson.Number;
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          UseSiReadCsePerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            return;
          }

          MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, export.Ap1);
        }
        else if (local.NoOfApOnCase.Count > 1)
        {
          export.MoreApsMsg.Text30 = "More AP's exist for this case.";

          break;
        }
      }

      // CQ27977  Start of the block of code moved out of the 'Read Each' 
      // statement for AP
      if (local.NoOfApOnCase.Count == 0)
      {
        export.MoreApsMsg.Text30 = "Case AP identity is unknown.";
        export.Ap1.Number = "";
        export.Ap1.FormattedName = "";
      }

      if (ReadCsePerson())
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePerson();

        if (!IsEmpty(local.AbendData.Type1))
        {
          return;
        }

        MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, export.Ar);
      }

      local.ProgramCount.Count = 1;

      export.Program.Index = 0;
      export.Program.Clear();

      foreach(var item in ReadProgram())
      {
        export.Program.Update.Program1.Code = entities.ClosedCaseProgram.Code;

        switch(local.ProgramCount.Count)
        {
          case 1:
            export.Pgm1.Code = entities.ClosedCaseProgram.Code;

            break;
          case 2:
            export.Pgm2.Code = entities.ClosedCaseProgram.Code;

            break;
          case 3:
            export.Pgm3.Code = entities.ClosedCaseProgram.Code;

            break;
          case 4:
            export.Pgm4.Code = entities.ClosedCaseProgram.Code;

            break;
          case 5:
            export.Pgm5.Code = entities.ClosedCaseProgram.Code;

            break;
          case 6:
            export.Pgm6.Code = entities.ClosedCaseProgram.Code;

            break;
          case 7:
            export.Pgm7.Code = entities.ClosedCaseProgram.Code;

            break;
          case 8:
            export.Pgm8.Code = entities.ClosedCaseProgram.Code;
            export.Program.Next();

            goto ReadEach;
          default:
            export.Program.Next();

            goto ReadEach;
        }

        ++local.ProgramCount.Count;
        export.Program.Next();
      }

ReadEach:

      UseSpDetOspAssgnToClosedCase();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("CASE_ASSIGNMENT_NF"))
        {
          local.ExitStateFlag.ExitCaseAssignNf.Flag = "Y";
        }

        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (IsEmpty(export.Ap1.Number))
      {
        export.FunctionDesc.Text4 = "LOC";
      }
      else
      {
        UseSpGetClosedCaseFunction();
      }

      if (ReadInfrastructure1())
      {
        export.HiddenOrigAndPass.SystemGeneratedIdentifier =
          entities.Infrastructure.SystemGeneratedIdentifier;
        export.LastReview.Date = Date(entities.Infrastructure.CreatedTimestamp);

        // *** Work request 000170
        // *** 08/08/2000 SWSRCHF
        // *** start
        local.Work2.Index = -1;

        foreach(var item in ReadNarrativeDetail7())
        {
          ++local.Work2.Index;
          local.Work2.CheckSize();

          if (local.Work2.Index == 0)
          {
            local.Work2.Update.Work.NarrativeText =
              entities.Existing.NarrativeText;
          }
          else
          {
            local.Work2.Update.Work.NarrativeText =
              Substring(entities.Existing.NarrativeText, 11, 58);
          }

          if (local.Work2.Index == 3)
          {
            break;
          }
        }

        local.WorkGroupMed.Index = -1;

        foreach(var item in ReadNarrativeDetail6())
        {
          ++local.WorkGroupMed.Index;
          local.WorkGroupMed.CheckSize();

          if (local.WorkGroupMed.Index == 0)
          {
            local.WorkGroupMed.Update.WorkGrpMed.NarrativeText =
              entities.Existing.NarrativeText;
          }
          else
          {
            local.WorkGroupMed.Update.WorkGrpMed.NarrativeText =
              Substring(entities.Existing.NarrativeText, 12, 57);
          }

          if (local.WorkGroupMed.Index == 3)
          {
            break;
          }
        }

        local.WorkGroupPat.Index = -1;

        foreach(var item in ReadNarrativeDetail1())
        {
          ++local.WorkGroupPat.Index;
          local.WorkGroupPat.CheckSize();

          if (local.WorkGroupPat.Index == 0)
          {
            local.WorkGroupPat.Update.WorkGrpPat.NarrativeText =
              entities.Existing.NarrativeText;
          }
          else
          {
            local.WorkGroupPat.Update.WorkGrpPat.NarrativeText =
              Substring(entities.Existing.NarrativeText, 14, 55);
          }

          if (local.WorkGroupPat.Index == 3)
          {
            break;
          }
        }

        local.WorkGroupEst.Index = -1;

        foreach(var item in ReadNarrativeDetail8())
        {
          ++local.WorkGroupEst.Index;
          local.WorkGroupEst.CheckSize();

          if (local.WorkGroupEst.Index == 0)
          {
            local.WorkGroupEst.Update.WorkGrpEst.NarrativeText =
              entities.Existing.NarrativeText;
          }
          else
          {
            local.WorkGroupEst.Update.WorkGrpEst.NarrativeText =
              Substring(entities.Existing.NarrativeText, 18, 51);
          }

          if (local.WorkGroupEst.Index == 3)
          {
            break;
          }
        }

        local.WorkGroupEnf.Index = -1;

        foreach(var item in ReadNarrativeDetail5())
        {
          ++local.WorkGroupEnf.Index;
          local.WorkGroupEnf.CheckSize();

          if (local.WorkGroupEnf.Index == 0)
          {
            local.WorkGroupEnf.Update.WorkGrpEnf.NarrativeText =
              entities.Existing.NarrativeText;
          }
          else
          {
            local.WorkGroupEnf.Update.WorkGrpEnf.NarrativeText =
              Substring(entities.Existing.NarrativeText, 16, 53);
          }

          if (local.WorkGroupEnf.Index == 3)
          {
            break;
          }
        }

        local.Work1.Index = 0;
        local.Work1.CheckSize();

        for(local.Work2.Index = 0; local.Work2.Index < local.Work2.Count; ++
          local.Work2.Index)
        {
          if (!local.Work2.CheckSize())
          {
            break;
          }

          local.Work1.Update.Work.Text = TrimEnd(local.Work1.Item.Work.Text) + (
            local.Work2.Item.Work.NarrativeText ?? "");
        }

        local.Work2.CheckIndex();

        local.Work1.Index = 1;
        local.Work1.CheckSize();

        for(local.WorkGroupMed.Index = 0; local.WorkGroupMed.Index < local
          .WorkGroupMed.Count; ++local.WorkGroupMed.Index)
        {
          if (!local.WorkGroupMed.CheckSize())
          {
            break;
          }

          // CMJ 160659  01/02/2004
          local.Work1.Update.Work.Text = TrimEnd(local.Work1.Item.Work.Text) + (
            local.WorkGroupMed.Item.WorkGrpMed.NarrativeText ?? "");
        }

        local.WorkGroupMed.CheckIndex();

        local.Work1.Index = 2;
        local.Work1.CheckSize();

        for(local.WorkGroupPat.Index = 0; local.WorkGroupPat.Index < local
          .WorkGroupPat.Count; ++local.WorkGroupPat.Index)
        {
          if (!local.WorkGroupPat.CheckSize())
          {
            break;
          }

          // CMJ 160659  01/02/2004
          local.Work1.Update.Work.Text = TrimEnd(local.Work1.Item.Work.Text) + (
            local.WorkGroupPat.Item.WorkGrpPat.NarrativeText ?? "");
        }

        local.WorkGroupPat.CheckIndex();

        local.Work1.Index = 3;
        local.Work1.CheckSize();

        for(local.WorkGroupEst.Index = 0; local.WorkGroupEst.Index < local
          .WorkGroupEst.Count; ++local.WorkGroupEst.Index)
        {
          if (!local.WorkGroupEst.CheckSize())
          {
            break;
          }

          // CMJ 160659  01/02/2004
          local.Work1.Update.Work.Text = TrimEnd(local.Work1.Item.Work.Text) + (
            local.WorkGroupEst.Item.WorkGrpEst.NarrativeText ?? "");
        }

        local.WorkGroupEst.CheckIndex();

        local.Work1.Index = 4;
        local.Work1.CheckSize();

        for(local.WorkGroupEnf.Index = 0; local.WorkGroupEnf.Index < local
          .WorkGroupEnf.Count; ++local.WorkGroupEnf.Index)
        {
          if (!local.WorkGroupEnf.CheckSize())
          {
            break;
          }

          // CMJ 160659  01/02/2004
          local.Work1.Update.Work.Text = TrimEnd(local.Work1.Item.Work.Text) + (
            local.WorkGroupEnf.Item.WorkGrpEnf.NarrativeText ?? "");
        }

        local.WorkGroupEnf.CheckIndex();

        for(local.Work1.Index = 0; local.Work1.Index < local.Work1.Count; ++
          local.Work1.Index)
        {
          if (!local.Work1.CheckSize())
          {
            break;
          }

          export.PassReviewNote.Index = local.Work1.Index;
          export.PassReviewNote.CheckSize();

          export.PassReviewNote.Update.GexportHidden.Text =
            local.Work1.Item.Work.Text;
        }

        local.Work1.CheckIndex();
        local.Work1.Index = -1;

        export.DisplayReviewNote.Index = 0;
        export.DisplayReviewNote.Clear();

        while(local.Work1.Index + 1 < local.Work1.Count)
        {
          if (export.DisplayReviewNote.IsFull)
          {
            break;
          }

          ++local.Work1.Index;
          local.Work1.CheckSize();

          export.DisplayReviewNote.Update.G.Text = local.Work1.Item.Work.Text;
          export.DisplayReviewNote.Next();
        }

        // *** end
        // *** 08/08/2000 SWSRCHF
        // *** Work request 000170
        if (ReadServiceProvider())
        {
          export.ServiceProvider.Assign(entities.ClosedCaseServiceProvider);
        }
        else
        {
          local.ExitStateFlag.ExitCaseAssignNf.Flag = "Y";
          export.ServiceProvider.UserId = entities.Infrastructure.CreatedBy;
        }
      }

      // CMJ PR 161926 - ADDED CHANGE TO READ FROM OR TO AND   01/09/2004
      if (ReadInfrastructure2())
      {
        export.HiddenOrigAndPass.SystemGeneratedIdentifier =
          entities.Infrastructure.SystemGeneratedIdentifier;
        export.LastReview1.Date =
          Date(entities.Infrastructure.CreatedTimestamp);

        // mjr
        // --------------------------------------
        // 02/25/2002
        // PR139864 - Added 'last rvw resulted in mod' flag
        // ---------------------------------------------------
        export.LastRvwResultedInMod.Flag =
          entities.Infrastructure.DenormText12 ?? Spaces(1);

        if (ReadServiceProvider())
        {
          MoveServiceProvider(entities.ClosedCaseServiceProvider, export.Export1);
            
        }
        else
        {
          local.ExitStateFlag.ExitCaseAssignNf.Flag = "Y";
          export.ServiceProvider.UserId = entities.Infrastructure.CreatedBy;
        }

        // CMJ PR 161926 IF NOTES ALREADY EXISTS FOR 'OSPRVWS' DO NOT READ FOR 
        // MODIFY NOTES
        // 01/20/2004
        if (!IsEmpty(local.Work1.Item.Work.Text))
        {
          goto Read;
        }

        // cmj commented out for displaying notes for modification pr161926
        // CMJ PR00161926 - ACCORDING TO SANA WANT TO SEE NARRATIVE NOTES
        local.Work2.Index = -1;

        foreach(var item in ReadNarrativeDetail2())
        {
          ++local.Work2.Index;
          local.Work2.CheckSize();

          if (local.Work2.Index == 0)
          {
            local.Work2.Update.Work.NarrativeText =
              entities.Existing.NarrativeText;
          }
          else
          {
            local.Work2.Update.Work.NarrativeText =
              Substring(entities.Existing.NarrativeText, 22, 47);
          }

          if (local.Work2.Index == 3)
          {
            break;
          }
        }

        local.WorkGroupMed.Index = -1;

        foreach(var item in ReadNarrativeDetail9())
        {
          ++local.WorkGroupMed.Index;
          local.WorkGroupMed.CheckSize();

          if (local.WorkGroupMed.Index == 0)
          {
            local.WorkGroupMed.Update.WorkGrpMed.NarrativeText =
              entities.Existing.NarrativeText;
          }
          else
          {
            local.WorkGroupMed.Update.WorkGrpMed.NarrativeText =
              Substring(entities.Existing.NarrativeText, 23, 46);
          }

          if (local.WorkGroupMed.Index == 3)
          {
            break;
          }
        }

        local.WorkGroupPat.Index = -1;

        foreach(var item in ReadNarrativeDetail3())
        {
          ++local.WorkGroupPat.Index;
          local.WorkGroupPat.CheckSize();

          if (local.WorkGroupPat.Index == 0)
          {
            local.WorkGroupPat.Update.WorkGrpPat.NarrativeText =
              entities.Existing.NarrativeText;
          }
          else
          {
            local.WorkGroupPat.Update.WorkGrpPat.NarrativeText =
              Substring(entities.Existing.NarrativeText, 25, 44);
          }

          if (local.WorkGroupPat.Index == 3)
          {
            break;
          }
        }

        local.WorkGroupEst.Index = -1;

        foreach(var item in ReadNarrativeDetail10())
        {
          ++local.WorkGroupEst.Index;
          local.WorkGroupEst.CheckSize();

          if (local.WorkGroupEst.Index == 0)
          {
            local.WorkGroupEst.Update.WorkGrpEst.NarrativeText =
              entities.Existing.NarrativeText;
          }
          else
          {
            local.WorkGroupEst.Update.WorkGrpEst.NarrativeText =
              Substring(entities.Existing.NarrativeText, 29, 40);
          }

          if (local.WorkGroupEst.Index == 3)
          {
            break;
          }
        }

        local.WorkGroupEnf.Index = -1;

        foreach(var item in ReadNarrativeDetail4())
        {
          ++local.WorkGroupEnf.Index;
          local.WorkGroupEnf.CheckSize();

          if (local.WorkGroupEnf.Index == 0)
          {
            local.WorkGroupEnf.Update.WorkGrpEnf.NarrativeText =
              entities.Existing.NarrativeText;
          }
          else
          {
            local.WorkGroupEnf.Update.WorkGrpEnf.NarrativeText =
              Substring(entities.Existing.NarrativeText, 27, 42);
          }

          if (local.WorkGroupEnf.Index == 3)
          {
            break;
          }
        }

        local.Work1.Index = 0;
        local.Work1.CheckSize();

        for(local.Work2.Index = 0; local.Work2.Index < local.Work2.Count; ++
          local.Work2.Index)
        {
          if (!local.Work2.CheckSize())
          {
            break;
          }

          local.Work1.Update.Work.Text = TrimEnd(local.Work1.Item.Work.Text) + (
            local.Work2.Item.Work.NarrativeText ?? "");
        }

        local.Work2.CheckIndex();

        local.Work1.Index = 1;
        local.Work1.CheckSize();

        for(local.WorkGroupMed.Index = 0; local.WorkGroupMed.Index < local
          .WorkGroupMed.Count; ++local.WorkGroupMed.Index)
        {
          if (!local.WorkGroupMed.CheckSize())
          {
            break;
          }

          local.Work1.Update.Work.Text = TrimEnd(local.Work1.Item.Work.Text) + (
            local.WorkGroupMed.Item.WorkGrpMed.NarrativeText ?? "");
        }

        local.WorkGroupMed.CheckIndex();

        local.Work1.Index = 2;
        local.Work1.CheckSize();

        for(local.WorkGroupPat.Index = 0; local.WorkGroupPat.Index < local
          .WorkGroupPat.Count; ++local.WorkGroupPat.Index)
        {
          if (!local.WorkGroupPat.CheckSize())
          {
            break;
          }

          local.Work1.Update.Work.Text = TrimEnd(local.Work1.Item.Work.Text) + (
            local.WorkGroupPat.Item.WorkGrpPat.NarrativeText ?? "");
        }

        local.WorkGroupPat.CheckIndex();

        local.Work1.Index = 3;
        local.Work1.CheckSize();

        for(local.WorkGroupEst.Index = 0; local.WorkGroupEst.Index < local
          .WorkGroupEst.Count; ++local.WorkGroupEst.Index)
        {
          if (!local.WorkGroupEst.CheckSize())
          {
            break;
          }

          local.Work1.Update.Work.Text = TrimEnd(local.Work1.Item.Work.Text) + (
            local.WorkGroupEst.Item.WorkGrpEst.NarrativeText ?? "");
        }

        local.WorkGroupEst.CheckIndex();

        local.Work1.Index = 4;
        local.Work1.CheckSize();

        for(local.WorkGroupEnf.Index = 0; local.WorkGroupEnf.Index < local
          .WorkGroupEnf.Count; ++local.WorkGroupEnf.Index)
        {
          if (!local.WorkGroupEnf.CheckSize())
          {
            break;
          }

          local.Work1.Update.Work.Text = TrimEnd(local.Work1.Item.Work.Text) + (
            local.WorkGroupEnf.Item.WorkGrpEnf.NarrativeText ?? "");
        }

        local.WorkGroupEnf.CheckIndex();

        for(local.Work1.Index = 0; local.Work1.Index < local.Work1.Count; ++
          local.Work1.Index)
        {
          if (!local.Work1.CheckSize())
          {
            break;
          }

          export.PassReviewNote.Index = local.Work1.Index;
          export.PassReviewNote.CheckSize();

          export.PassReviewNote.Update.GexportHidden.Text =
            local.Work1.Item.Work.Text;
        }

        local.Work1.CheckIndex();
        local.Work1.Index = -1;

        export.DisplayReviewNote.Index = 0;
        export.DisplayReviewNote.Clear();

        while(local.Work1.Index + 1 < local.Work1.Count)
        {
          if (export.DisplayReviewNote.IsFull)
          {
            break;
          }

          ++local.Work1.Index;
          local.Work1.CheckSize();

          export.DisplayReviewNote.Update.G.Text = local.Work1.Item.Work.Text;
          export.DisplayReviewNote.Next();
        }

        // cmj pr 161926 end of this section code
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

Read:

    // CQ27977  End of the block of code moved out of the 'Read Each' statement 
    // for AP
    // ************************************************
    // As this is a review of a closed case, it is possible that offices, 
    // service providers, ect are no longer valid or in the system.  However, a
    // closed case review will continue gathering data as far as possible.  The
    // following structure will raise Exit States in specified order for user
    // information while continuing display.
    // ************************************************
    if (AsChar(local.ExitStateFlag.ExitOfficeNf.Flag) == 'Y')
    {
      ExitState = "FN0000_OFFICE_NF";

      return;
    }

    if (AsChar(local.ExitStateFlag.ExitCaseAssignNf.Flag) == 'Y')
    {
      ExitState = "CASE_ASSIGNMENT_NF";

      return;
    }

    if (export.DisplayReviewNote.IsEmpty)
    {
      ExitState = "SP0000_FIRST_REVIEW_4_CLOSD_CASE";
    }
    else if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(export.CommandPassedFromEnfor.Flag) == 'A')
      {
        ExitState = "SP0000_CASE_REVIEW_COMPLETED";
      }
      else if (AsChar(export.CommandPassedFromEnfor.Flag) == 'D')
      {
        ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
      }
    }
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
      
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseSpDetOspAssgnToClosedCase()
  {
    var useImport = new SpDetOspAssgnToClosedCase.Import();
    var useExport = new SpDetOspAssgnToClosedCase.Export();

    useImport.Closed.Number = export.Case2.Number;

    Call(SpDetOspAssgnToClosedCase.Execute, useImport, useExport);

    export.CaseCoordinator.Assign(useExport.ServiceProvider);
    MoveOffice(useExport.Office, export.Case1);
  }

  private void UseSpGetClosedCaseFunction()
  {
    var useImport = new SpGetClosedCaseFunction.Import();
    var useExport = new SpGetClosedCaseFunction.Export();

    useImport.Closed.Number = export.Case2.Number;

    Call(SpGetClosedCaseFunction.Execute, useImport, useExport);

    export.FunctionDesc.Text4 = useExport.CauFunctionDescription.Text4;
  }

  private bool ReadCase()
  {
    entities.ClosedCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Crme.Number);
      },
      (db, reader) =>
      {
        entities.ClosedCase.ClosureReason = db.GetNullableString(reader, 0);
        entities.ClosedCase.Number = db.GetString(reader, 1);
        entities.ClosedCase.Status = db.GetNullableString(reader, 2);
        entities.ClosedCase.StatusDate = db.GetNullableDate(reader, 3);
        entities.ClosedCase.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ClosedCase.Number);
        db.SetNullableDate(
          command, "endDate", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole()
  {
    entities.CsePerson.Populated = false;
    entities.ClosedCaseRole.Populated = false;

    return ReadEach("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ClosedCase.Number);
        db.SetNullableDate(
          command, "endDate", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.ClosedCaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.ClosedCaseRole.CasNumber = db.GetString(reader, 2);
        entities.ClosedCaseRole.Type1 = db.GetString(reader, 3);
        entities.ClosedCaseRole.Identifier = db.GetInt32(reader, 4);
        entities.ClosedCaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.ClosedCaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.ClosedCaseRole.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CsePerson.Populated = true;
        entities.ClosedCaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.ClosedCaseRole.Type1);

        return true;
      });
  }

  private bool ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case2.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 4);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 5);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 6);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 7);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Infrastructure.Function = db.GetNullableString(reader, 9);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case2.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 4);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 5);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 6);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 7);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Infrastructure.Function = db.GetNullableString(reader, 9);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail1()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail10()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail10",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail2()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail3()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail3",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail4()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail4",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail5()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail5",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail6()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail6",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail7()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail7",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail8()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail8",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail9()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail9",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    return ReadEach("ReadProgram",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.CaseClosedDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.ClosedCase.Number);
      },
      (db, reader) =>
      {
        if (export.Program.IsFull)
        {
          return false;
        }

        entities.ClosedCaseProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ClosedCaseProgram.Code = db.GetString(reader, 1);
        entities.ClosedCaseProgram.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ClosedCaseServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", entities.Infrastructure.CreatedBy);
      },
      (db, reader) =>
      {
        entities.ClosedCaseServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ClosedCaseServiceProvider.UserId = db.GetString(reader, 1);
        entities.ClosedCaseServiceProvider.LastName = db.GetString(reader, 2);
        entities.ClosedCaseServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ClosedCaseServiceProvider.Populated = true;
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
    /// <summary>
    /// A value of Crme.
    /// </summary>
    [JsonPropertyName("crme")]
    public Case1 Crme
    {
      get => crme ??= new();
      set => crme = value;
    }

    /// <summary>
    /// A value of HiddenReviewType.
    /// </summary>
    [JsonPropertyName("hiddenReviewType")]
    public Common HiddenReviewType
    {
      get => hiddenReviewType ??= new();
      set => hiddenReviewType = value;
    }

    /// <summary>
    /// A value of CommandPassedFromEnfor.
    /// </summary>
    [JsonPropertyName("commandPassedFromEnfor")]
    public Common CommandPassedFromEnfor
    {
      get => commandPassedFromEnfor ??= new();
      set => commandPassedFromEnfor = value;
    }

    private Case1 crme;
    private Common hiddenReviewType;
    private Common commandPassedFromEnfor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ProgramGroup group.</summary>
    [Serializable]
    public class ProgramGroup
    {
      /// <summary>
      /// A value of Program1.
      /// </summary>
      [JsonPropertyName("program1")]
      public Program Program1
      {
        get => program1 ??= new();
        set => program1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private Program program1;
    }

    /// <summary>A PassReviewNoteGroup group.</summary>
    [Serializable]
    public class PassReviewNoteGroup
    {
      /// <summary>
      /// A value of GexportHidden.
      /// </summary>
      [JsonPropertyName("gexportHidden")]
      public NarrativeWork GexportHidden
      {
        get => gexportHidden ??= new();
        set => gexportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeWork gexportHidden;
    }

    /// <summary>A DisplayReviewNoteGroup group.</summary>
    [Serializable]
    public class DisplayReviewNoteGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public NarrativeWork G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeWork g;
    }

    /// <summary>
    /// A value of LastReview1.
    /// </summary>
    [JsonPropertyName("lastReview1")]
    public DateWorkArea LastReview1
    {
      get => lastReview1 ??= new();
      set => lastReview1 = value;
    }

    /// <summary>
    /// A value of Export1.
    /// </summary>
    [JsonPropertyName("export1")]
    public ServiceProvider Export1
    {
      get => export1 ??= new();
      set => export1 = value;
    }

    /// <summary>
    /// A value of CaseCoordinator.
    /// </summary>
    [JsonPropertyName("caseCoordinator")]
    public ServiceProvider CaseCoordinator
    {
      get => caseCoordinator ??= new();
      set => caseCoordinator = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Office Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap1.
    /// </summary>
    [JsonPropertyName("ap1")]
    public CsePersonsWorkSet Ap1
    {
      get => ap1 ??= new();
      set => ap1 = value;
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
    /// A value of Pgm1.
    /// </summary>
    [JsonPropertyName("pgm1")]
    public Program Pgm1
    {
      get => pgm1 ??= new();
      set => pgm1 = value;
    }

    /// <summary>
    /// A value of Pgm2.
    /// </summary>
    [JsonPropertyName("pgm2")]
    public Program Pgm2
    {
      get => pgm2 ??= new();
      set => pgm2 = value;
    }

    /// <summary>
    /// A value of Pgm3.
    /// </summary>
    [JsonPropertyName("pgm3")]
    public Program Pgm3
    {
      get => pgm3 ??= new();
      set => pgm3 = value;
    }

    /// <summary>
    /// A value of Pgm4.
    /// </summary>
    [JsonPropertyName("pgm4")]
    public Program Pgm4
    {
      get => pgm4 ??= new();
      set => pgm4 = value;
    }

    /// <summary>
    /// A value of Pgm5.
    /// </summary>
    [JsonPropertyName("pgm5")]
    public Program Pgm5
    {
      get => pgm5 ??= new();
      set => pgm5 = value;
    }

    /// <summary>
    /// A value of Pgm6.
    /// </summary>
    [JsonPropertyName("pgm6")]
    public Program Pgm6
    {
      get => pgm6 ??= new();
      set => pgm6 = value;
    }

    /// <summary>
    /// A value of Pgm7.
    /// </summary>
    [JsonPropertyName("pgm7")]
    public Program Pgm7
    {
      get => pgm7 ??= new();
      set => pgm7 = value;
    }

    /// <summary>
    /// A value of Pgm8.
    /// </summary>
    [JsonPropertyName("pgm8")]
    public Program Pgm8
    {
      get => pgm8 ??= new();
      set => pgm8 = value;
    }

    /// <summary>
    /// Gets a value of Program.
    /// </summary>
    [JsonIgnore]
    public Array<ProgramGroup> Program =>
      program ??= new(ProgramGroup.Capacity);

    /// <summary>
    /// Gets a value of Program for json serialization.
    /// </summary>
    [JsonPropertyName("program")]
    [Computed]
    public IList<ProgramGroup> Program_Json
    {
      get => program;
      set => Program.Assign(value);
    }

    /// <summary>
    /// A value of Case2.
    /// </summary>
    [JsonPropertyName("case2")]
    public Case1 Case2
    {
      get => case2 ??= new();
      set => case2 = value;
    }

    /// <summary>
    /// A value of MoreApsMsg.
    /// </summary>
    [JsonPropertyName("moreApsMsg")]
    public TextWorkArea MoreApsMsg
    {
      get => moreApsMsg ??= new();
      set => moreApsMsg = value;
    }

    /// <summary>
    /// A value of FunctionDesc.
    /// </summary>
    [JsonPropertyName("functionDesc")]
    public TextWorkArea FunctionDesc
    {
      get => functionDesc ??= new();
      set => functionDesc = value;
    }

    /// <summary>
    /// A value of HiddenOrigAndPass.
    /// </summary>
    [JsonPropertyName("hiddenOrigAndPass")]
    public Infrastructure HiddenOrigAndPass
    {
      get => hiddenOrigAndPass ??= new();
      set => hiddenOrigAndPass = value;
    }

    /// <summary>
    /// A value of LastReview.
    /// </summary>
    [JsonPropertyName("lastReview")]
    public DateWorkArea LastReview
    {
      get => lastReview ??= new();
      set => lastReview = value;
    }

    /// <summary>
    /// Gets a value of PassReviewNote.
    /// </summary>
    [JsonIgnore]
    public Array<PassReviewNoteGroup> PassReviewNote => passReviewNote ??= new(
      PassReviewNoteGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PassReviewNote for json serialization.
    /// </summary>
    [JsonPropertyName("passReviewNote")]
    [Computed]
    public IList<PassReviewNoteGroup> PassReviewNote_Json
    {
      get => passReviewNote;
      set => PassReviewNote.Assign(value);
    }

    /// <summary>
    /// Gets a value of DisplayReviewNote.
    /// </summary>
    [JsonIgnore]
    public Array<DisplayReviewNoteGroup> DisplayReviewNote =>
      displayReviewNote ??= new(DisplayReviewNoteGroup.Capacity);

    /// <summary>
    /// Gets a value of DisplayReviewNote for json serialization.
    /// </summary>
    [JsonPropertyName("displayReviewNote")]
    [Computed]
    public IList<DisplayReviewNoteGroup> DisplayReviewNote_Json
    {
      get => displayReviewNote;
      set => DisplayReviewNote.Assign(value);
    }

    /// <summary>
    /// A value of ClosedCaseIndicator.
    /// </summary>
    [JsonPropertyName("closedCaseIndicator")]
    public Common ClosedCaseIndicator
    {
      get => closedCaseIndicator ??= new();
      set => closedCaseIndicator = value;
    }

    /// <summary>
    /// A value of HiddenReviewType.
    /// </summary>
    [JsonPropertyName("hiddenReviewType")]
    public Common HiddenReviewType
    {
      get => hiddenReviewType ??= new();
      set => hiddenReviewType = value;
    }

    /// <summary>
    /// A value of CommandPassedFromEnfor.
    /// </summary>
    [JsonPropertyName("commandPassedFromEnfor")]
    public Common CommandPassedFromEnfor
    {
      get => commandPassedFromEnfor ??= new();
      set => commandPassedFromEnfor = value;
    }

    /// <summary>
    /// A value of LastRvwResultedInMod.
    /// </summary>
    [JsonPropertyName("lastRvwResultedInMod")]
    public Common LastRvwResultedInMod
    {
      get => lastRvwResultedInMod ??= new();
      set => lastRvwResultedInMod = value;
    }

    private DateWorkArea lastReview1;
    private ServiceProvider export1;
    private ServiceProvider caseCoordinator;
    private Office case1;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap1;
    private ServiceProvider serviceProvider;
    private Program pgm1;
    private Program pgm2;
    private Program pgm3;
    private Program pgm4;
    private Program pgm5;
    private Program pgm6;
    private Program pgm7;
    private Program pgm8;
    private Array<ProgramGroup> program;
    private Case1 case2;
    private TextWorkArea moreApsMsg;
    private TextWorkArea functionDesc;
    private Infrastructure hiddenOrigAndPass;
    private DateWorkArea lastReview;
    private Array<PassReviewNoteGroup> passReviewNote;
    private Array<DisplayReviewNoteGroup> displayReviewNote;
    private Common closedCaseIndicator;
    private Common hiddenReviewType;
    private Common commandPassedFromEnfor;
    private Common lastRvwResultedInMod;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A WorkGroup1 group.</summary>
    [Serializable]
    public class WorkGroup1
    {
      /// <summary>
      /// A value of Work.
      /// </summary>
      [JsonPropertyName("work")]
      public NarrativeWork Work
      {
        get => work ??= new();
        set => work = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeWork work;
    }

    /// <summary>A WorkGroup2 group.</summary>
    [Serializable]
    public class WorkGroup2
    {
      /// <summary>
      /// A value of Work.
      /// </summary>
      [JsonPropertyName("work")]
      public NarrativeDetail Work
      {
        get => work ??= new();
        set => work = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeDetail work;
    }

    /// <summary>A WorkGroupMedGroup group.</summary>
    [Serializable]
    public class WorkGroupMedGroup
    {
      /// <summary>
      /// A value of WorkGrpMed.
      /// </summary>
      [JsonPropertyName("workGrpMed")]
      public NarrativeDetail WorkGrpMed
      {
        get => workGrpMed ??= new();
        set => workGrpMed = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeDetail workGrpMed;
    }

    /// <summary>A WorkGroupPatGroup group.</summary>
    [Serializable]
    public class WorkGroupPatGroup
    {
      /// <summary>
      /// A value of WorkGrpPat.
      /// </summary>
      [JsonPropertyName("workGrpPat")]
      public NarrativeDetail WorkGrpPat
      {
        get => workGrpPat ??= new();
        set => workGrpPat = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeDetail workGrpPat;
    }

    /// <summary>A WorkGroupEstGroup group.</summary>
    [Serializable]
    public class WorkGroupEstGroup
    {
      /// <summary>
      /// A value of WorkGrpEst.
      /// </summary>
      [JsonPropertyName("workGrpEst")]
      public NarrativeDetail WorkGrpEst
      {
        get => workGrpEst ??= new();
        set => workGrpEst = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeDetail workGrpEst;
    }

    /// <summary>A WorkGroupEnfGroup group.</summary>
    [Serializable]
    public class WorkGroupEnfGroup
    {
      /// <summary>
      /// A value of WorkGrpEnf.
      /// </summary>
      [JsonPropertyName("workGrpEnf")]
      public NarrativeDetail WorkGrpEnf
      {
        get => workGrpEnf ??= new();
        set => workGrpEnf = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeDetail workGrpEnf;
    }

    /// <summary>A ExitStateFlagGroup group.</summary>
    [Serializable]
    public class ExitStateFlagGroup
    {
      /// <summary>
      /// A value of ExitCaseAssignNf.
      /// </summary>
      [JsonPropertyName("exitCaseAssignNf")]
      public Common ExitCaseAssignNf
      {
        get => exitCaseAssignNf ??= new();
        set => exitCaseAssignNf = value;
      }

      /// <summary>
      /// A value of ExitOfficeNf.
      /// </summary>
      [JsonPropertyName("exitOfficeNf")]
      public Common ExitOfficeNf
      {
        get => exitOfficeNf ??= new();
        set => exitOfficeNf = value;
      }

      private Common exitCaseAssignNf;
      private Common exitOfficeNf;
    }

    /// <summary>
    /// Gets a value of Work1.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroup1> Work1 => work1 ??= new(WorkGroup1.Capacity, 0);

    /// <summary>
    /// Gets a value of Work1 for json serialization.
    /// </summary>
    [JsonPropertyName("work1")]
    [Computed]
    public IList<WorkGroup1> Work1_Json
    {
      get => work1;
      set => Work1.Assign(value);
    }

    /// <summary>
    /// Gets a value of Work2.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroup2> Work2 => work2 ??= new(WorkGroup2.Capacity, 0);

    /// <summary>
    /// Gets a value of Work2 for json serialization.
    /// </summary>
    [JsonPropertyName("work2")]
    [Computed]
    public IList<WorkGroup2> Work2_Json
    {
      get => work2;
      set => Work2.Assign(value);
    }

    /// <summary>
    /// Gets a value of WorkGroupMed.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroupMedGroup> WorkGroupMed => workGroupMed ??= new(
      WorkGroupMedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of WorkGroupMed for json serialization.
    /// </summary>
    [JsonPropertyName("workGroupMed")]
    [Computed]
    public IList<WorkGroupMedGroup> WorkGroupMed_Json
    {
      get => workGroupMed;
      set => WorkGroupMed.Assign(value);
    }

    /// <summary>
    /// Gets a value of WorkGroupPat.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroupPatGroup> WorkGroupPat => workGroupPat ??= new(
      WorkGroupPatGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of WorkGroupPat for json serialization.
    /// </summary>
    [JsonPropertyName("workGroupPat")]
    [Computed]
    public IList<WorkGroupPatGroup> WorkGroupPat_Json
    {
      get => workGroupPat;
      set => WorkGroupPat.Assign(value);
    }

    /// <summary>
    /// Gets a value of WorkGroupEst.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroupEstGroup> WorkGroupEst => workGroupEst ??= new(
      WorkGroupEstGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of WorkGroupEst for json serialization.
    /// </summary>
    [JsonPropertyName("workGroupEst")]
    [Computed]
    public IList<WorkGroupEstGroup> WorkGroupEst_Json
    {
      get => workGroupEst;
      set => WorkGroupEst.Assign(value);
    }

    /// <summary>
    /// Gets a value of WorkGroupEnf.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroupEnfGroup> WorkGroupEnf => workGroupEnf ??= new(
      WorkGroupEnfGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of WorkGroupEnf for json serialization.
    /// </summary>
    [JsonPropertyName("workGroupEnf")]
    [Computed]
    public IList<WorkGroupEnfGroup> WorkGroupEnf_Json
    {
      get => workGroupEnf;
      set => WorkGroupEnf.Assign(value);
    }

    /// <summary>
    /// Gets a value of ExitStateFlag.
    /// </summary>
    [JsonPropertyName("exitStateFlag")]
    public ExitStateFlagGroup ExitStateFlag
    {
      get => exitStateFlag ?? (exitStateFlag = new());
      set => exitStateFlag = value;
    }

    /// <summary>
    /// A value of CaseClosedDate.
    /// </summary>
    [JsonPropertyName("caseClosedDate")]
    public DateWorkArea CaseClosedDate
    {
      get => caseClosedDate ??= new();
      set => caseClosedDate = value;
    }

    /// <summary>
    /// A value of NoOfApOnCase.
    /// </summary>
    [JsonPropertyName("noOfApOnCase")]
    public Common NoOfApOnCase
    {
      get => noOfApOnCase ??= new();
      set => noOfApOnCase = value;
    }

    /// <summary>
    /// A value of Ap1.
    /// </summary>
    [JsonPropertyName("ap1")]
    public CsePerson Ap1
    {
      get => ap1 ??= new();
      set => ap1 = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of ProgramCount.
    /// </summary>
    [JsonPropertyName("programCount")]
    public Common ProgramCount
    {
      get => programCount ??= new();
      set => programCount = value;
    }

    /// <summary>
    /// A value of ReviewType.
    /// </summary>
    [JsonPropertyName("reviewType")]
    public Infrastructure ReviewType
    {
      get => reviewType ??= new();
      set => reviewType = value;
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

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Array<WorkGroup1> work1;
    private Array<WorkGroup2> work2;
    private Array<WorkGroupMedGroup> workGroupMed;
    private Array<WorkGroupPatGroup> workGroupPat;
    private Array<WorkGroupEstGroup> workGroupEst;
    private Array<WorkGroupEnfGroup> workGroupEnf;
    private ExitStateFlagGroup exitStateFlag;
    private DateWorkArea caseClosedDate;
    private Common noOfApOnCase;
    private CsePerson ap1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private Common programCount;
    private Infrastructure reviewType;
    private Common count;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public NarrativeDetail Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ClosedCase.
    /// </summary>
    [JsonPropertyName("closedCase")]
    public Case1 ClosedCase
    {
      get => closedCase ??= new();
      set => closedCase = value;
    }

    /// <summary>
    /// A value of ClosedCaseAssigned.
    /// </summary>
    [JsonPropertyName("closedCaseAssigned")]
    public Office ClosedCaseAssigned
    {
      get => closedCaseAssigned ??= new();
      set => closedCaseAssigned = value;
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
    /// A value of ClosedCaseRole.
    /// </summary>
    [JsonPropertyName("closedCaseRole")]
    public CaseRole ClosedCaseRole
    {
      get => closedCaseRole ??= new();
      set => closedCaseRole = value;
    }

    /// <summary>
    /// A value of ClosedCaseProgram.
    /// </summary>
    [JsonPropertyName("closedCaseProgram")]
    public Program ClosedCaseProgram
    {
      get => closedCaseProgram ??= new();
      set => closedCaseProgram = value;
    }

    /// <summary>
    /// A value of ClosedCasePersonProgram.
    /// </summary>
    [JsonPropertyName("closedCasePersonProgram")]
    public PersonProgram ClosedCasePersonProgram
    {
      get => closedCasePersonProgram ??= new();
      set => closedCasePersonProgram = value;
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
    /// A value of ClosedCaseServiceProvider.
    /// </summary>
    [JsonPropertyName("closedCaseServiceProvider")]
    public ServiceProvider ClosedCaseServiceProvider
    {
      get => closedCaseServiceProvider ??= new();
      set => closedCaseServiceProvider = value;
    }

    /// <summary>
    /// A value of ClosedCaseAssignment.
    /// </summary>
    [JsonPropertyName("closedCaseAssignment")]
    public CaseAssignment ClosedCaseAssignment
    {
      get => closedCaseAssignment ??= new();
      set => closedCaseAssignment = value;
    }

    /// <summary>
    /// A value of ClosedCaseOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("closedCaseOfficeServiceProvider")]
    public OfficeServiceProvider ClosedCaseOfficeServiceProvider
    {
      get => closedCaseOfficeServiceProvider ??= new();
      set => closedCaseOfficeServiceProvider = value;
    }

    private NarrativeDetail existing;
    private Case1 closedCase;
    private Office closedCaseAssigned;
    private CsePerson csePerson;
    private CaseRole closedCaseRole;
    private Program closedCaseProgram;
    private PersonProgram closedCasePersonProgram;
    private Infrastructure infrastructure;
    private ServiceProvider closedCaseServiceProvider;
    private CaseAssignment closedCaseAssignment;
    private OfficeServiceProvider closedCaseOfficeServiceProvider;
  }
#endregion
}
