// Program: SI_RMAD_SAVE_DATA, ID: 373475617, model: 746.
// Short name: SWE00646
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_RMAD_SAVE_DATA.
/// </summary>
[Serializable]
public partial class SiRmadSaveData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RMAD_SAVE_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRmadSaveData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRmadSaveData.
  /// </summary>
  public SiRmadSaveData(IContext context, Import import, Export export):
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
    // 01/21/2007	Raj S		PR 299791 	Modified to fix the  VIEW OVERFLOW problem 
    // for group
    //                                                 
    // view GROUP_LOCAL_VALIDATE.  This change impacts
    // the
    //                                                 
    // calling action block
    // SI_RMAD_CREATE_UPDATE_CASE_UNIT.
    // ------------------------------------------------------------------------------------------------------
    local.Current.Timestamp = Now();
    UseCabSetMaximumDiscontinueDate();
    local.Common.Subscript = 1;

    for(var limit = import.Import1.Count; local.Common.Subscript <= limit; ++
      local.Common.Subscript)
    {
      import.Import1.Index = local.Common.Subscript - 1;
      import.Import1.CheckSize();

      // 11/15/2002 M. Lachowicz Start
      if (!Equal(import.Import1.Item.Case1.Number, local.New1.Number))
      {
        local.New1.Number = import.Import1.Item.Case1.Number;
        local.SubForFirstCaseOccur.Subscript = import.Import1.Index + 1;
      }

      // 11/15/2002 M. Lachowicz End
      if (AsChar(import.Import1.Item.RowOperation.OneChar) == 'A' || AsChar
        (import.Import1.Item.RowOperation.OneChar) == 'C')
      {
        if (!Equal(import.Import1.Item.Case1.Number, local.Prev.Number))
        {
          // 11/15/2002 M. Lachowicz Start
          // 11/15/2002 M. Lachowicz End
          local.Prev.Number = import.Import1.Item.Case1.Number;
          local.ValidatedCase.Number = local.Prev.Number;
          local.Validate.Count = 0;
          local.Validate.Index = -1;

          if (!ReadCase2())
          {
            ExitState = "CASE_NF";

            return;
          }
        }
        else
        {
          continue;
        }

        // Validate data from import array against data from DB2 table.
        foreach(var item in ReadCaseRoleCsePerson())
        {
          for(import.Import1.Index = local.SubForFirstCaseOccur.Subscript - 1; import
            .Import1.Index < import.Import1.Count; ++import.Import1.Index)
          {
            if (!import.Import1.CheckSize())
            {
              break;
            }

            if (!Equal(import.Import1.Item.Case1.Number, local.Prev.Number))
            {
              ++local.Validate.Index;
              local.Validate.CheckSize();

              MoveCaseRole1(entities.CaseRole,
                local.Validate.Update.AllCaseRoles);
              local.Validate.Update.AllPersons.Number =
                entities.CsePerson.Number;

              goto ReadEach1;
            }

            if (entities.CaseRole.Identifier == import
              .Import1.Item.CaseRole.Identifier)
            {
              ++local.Validate.Index;
              local.Validate.CheckSize();

              local.Validate.Update.AllCaseRoles.Assign(
                import.Import1.Item.CaseRole);
              local.Validate.Update.AllPersons.Number =
                import.Import1.Item.CsePersonsWorkSet.Number;
              local.Validate.Update.RowNumber.Count =
                import.Import1.Item.RowNumber.Count;
              local.Validate.Update.AllRowOperation.OneChar =
                import.Import1.Item.RowOperation.OneChar;

              goto ReadEach1;
            }
          }

          import.Import1.CheckIndex();

          if (!Equal(import.Import1.Item.Case1.Number, local.Prev.Number))
          {
            ++local.Validate.Index;
            local.Validate.CheckSize();

            MoveCaseRole1(entities.CaseRole, local.Validate.Update.AllCaseRoles);
              
            local.Validate.Update.AllPersons.Number = entities.CsePerson.Number;

            continue;
          }

ReadEach1:
          ;
        }

        for(import.Import1.Index = local.SubForFirstCaseOccur.Subscript - 1; import
          .Import1.Index < import.Import1.Count; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          if (!Equal(import.Import1.Item.Case1.Number, local.Prev.Number))
          {
            break;
          }

          if (import.Import1.Item.CaseRole.Identifier == 0)
          {
            ++local.Validate.Index;
            local.Validate.CheckSize();

            local.Validate.Update.AllCaseRoles.Assign(
              import.Import1.Item.CaseRole);
            local.Validate.Update.AllPersons.Number =
              import.Import1.Item.CsePersonsWorkSet.Number;
            local.Validate.Update.RowNumber.Count =
              import.Import1.Item.RowNumber.Count;
            local.Validate.Update.AllRowOperation.OneChar =
              import.Import1.Item.RowOperation.OneChar;
          }
        }

        import.Import1.CheckIndex();

        // Now We have all case roles in local repeating group view.
        local.SortSubscript.Flag = "Y";

        while(AsChar(local.SortSubscript.Flag) == 'Y')
        {
          local.SortSubscript.Flag = "N";
          local.SortSubscript.Subscript = 1;

          for(var limit1 = local.Validate.Count - 1; local
            .SortSubscript.Subscript <= limit1; ++
            local.SortSubscript.Subscript)
          {
            local.Validate.Index = local.SortSubscript.Subscript - 1;
            local.Validate.CheckSize();

            local.FirstCaseRole.Assign(local.Validate.Item.AllCaseRoles);
            local.FirstCsePersonsWorkSet.Number =
              local.Validate.Item.AllPersons.Number;
            local.FirstRowOperation.OneChar =
              local.Validate.Item.AllRowOperation.OneChar;
            local.FirstRowNumber.Count = local.Validate.Item.RowNumber.Count;

            local.Validate.Index = local.SortSubscript.Subscript;
            local.Validate.CheckSize();

            local.SecondCaseRole.Assign(local.Validate.Item.AllCaseRoles);
            local.SecondCsePersonsWorkSet.Number =
              local.Validate.Item.AllPersons.Number;
            local.SecondRowOperation.OneChar =
              local.Validate.Item.AllRowOperation.OneChar;
            local.SecondRowNumber.Count = local.Validate.Item.RowNumber.Count;

            if (Lt(local.SecondCsePersonsWorkSet.Number,
              local.FirstCsePersonsWorkSet.Number) || Equal
              (local.FirstCsePersonsWorkSet.Number,
              local.SecondCsePersonsWorkSet.Number) && Lt
              (local.SecondCaseRole.Type1, local.FirstCaseRole.Type1) || Equal
              (local.FirstCsePersonsWorkSet.Number,
              local.SecondCsePersonsWorkSet.Number) && Equal
              (local.SecondCaseRole.Type1, local.FirstCaseRole.Type1) && Lt
              (local.SecondCaseRole.StartDate, local.FirstCaseRole.StartDate))
            {
              local.Validate.Update.AllCaseRoles.Assign(local.FirstCaseRole);
              local.Validate.Update.AllPersons.Number =
                local.FirstCsePersonsWorkSet.Number;
              local.Validate.Update.RowNumber.Count =
                local.FirstRowNumber.Count;
              local.Validate.Update.AllRowOperation.OneChar =
                local.FirstRowOperation.OneChar;

              local.Validate.Index = local.SortSubscript.Subscript - 1;
              local.Validate.CheckSize();

              local.Validate.Update.AllCaseRoles.Assign(local.SecondCaseRole);
              local.Validate.Update.AllPersons.Number =
                local.SecondCsePersonsWorkSet.Number;
              local.Validate.Update.RowNumber.Count =
                local.SecondRowNumber.Count;
              local.Validate.Update.AllRowOperation.OneChar =
                local.SecondRowOperation.OneChar;
              local.SortSubscript.Flag = "Y";
            }
          }
        }

        // Sort is done, check first validation.
        local.SortSubscript.Subscript = 1;

        for(var limit1 = local.Validate.Count - 1; local
          .SortSubscript.Subscript <= limit1; ++local.SortSubscript.Subscript)
        {
          local.Validate.Index = local.SortSubscript.Subscript - 1;
          local.Validate.CheckSize();

          local.FirstCaseRole.Assign(local.Validate.Item.AllCaseRoles);
          local.FirstCsePersonsWorkSet.Number =
            local.Validate.Item.AllPersons.Number;
          local.FirstRowNumber.Count = local.Validate.Item.RowNumber.Count;
          local.FirstRowOperation.OneChar =
            local.Validate.Item.AllRowOperation.OneChar;

          local.Validate.Index = local.SortSubscript.Subscript;
          local.Validate.CheckSize();

          local.SecondCaseRole.Assign(local.Validate.Item.AllCaseRoles);
          local.SecondCsePersonsWorkSet.Number =
            local.Validate.Item.AllPersons.Number;
          local.SecondRowOperation.OneChar =
            local.Validate.Item.AllRowOperation.OneChar;
          local.SecondRowNumber.Count = local.Validate.Item.RowNumber.Count;

          if (Equal(local.SecondCsePersonsWorkSet.Number,
            local.FirstCsePersonsWorkSet.Number) && Equal
            (local.FirstCaseRole.Type1, local.SecondCaseRole.Type1) && !
            Lt(local.FirstCaseRole.EndDate, local.SecondCaseRole.StartDate))
          {
            // This is error.
            if (IsEmpty(local.FirstRowOperation.OneChar))
            {
              export.ErrorRowNumber.Count = local.SecondRowNumber.Count;
              export.ErrorCsePersonsWorkSet.Number =
                local.SecondCsePersonsWorkSet.Number;
            }
            else
            {
              export.ErrorRowNumber.Count = local.FirstRowNumber.Count;
              export.ErrorCsePersonsWorkSet.Number =
                local.FirstCsePersonsWorkSet.Number;
            }

            export.ErrorCase.Number = local.ValidatedCase.Number;
            export.ErrorNumber.Count = 1;

            return;
          }
        }

        // Sort for ascending sequence of roles, effective dates and person 
        // number.
        local.SortSubscript.Flag = "Y";

        while(AsChar(local.SortSubscript.Flag) == 'Y')
        {
          local.SortSubscript.Flag = "N";
          local.SortSubscript.Subscript = 1;

          for(var limit1 = local.Validate.Count - 1; local
            .SortSubscript.Subscript <= limit1; ++
            local.SortSubscript.Subscript)
          {
            local.Validate.Index = local.SortSubscript.Subscript - 1;
            local.Validate.CheckSize();

            local.FirstCaseRole.Assign(local.Validate.Item.AllCaseRoles);
            local.FirstCsePersonsWorkSet.Number =
              local.Validate.Item.AllPersons.Number;
            local.FirstRowOperation.OneChar =
              local.Validate.Item.AllRowOperation.OneChar;
            local.FirstRowNumber.Count = local.Validate.Item.RowNumber.Count;

            local.Validate.Index = local.SortSubscript.Subscript;
            local.Validate.CheckSize();

            local.SecondCaseRole.Assign(local.Validate.Item.AllCaseRoles);
            local.SecondCsePersonsWorkSet.Number =
              local.Validate.Item.AllPersons.Number;
            local.SecondRowOperation.OneChar =
              local.Validate.Item.AllRowOperation.OneChar;
            local.SecondRowNumber.Count = local.Validate.Item.RowNumber.Count;

            if (Lt(local.SecondCaseRole.Type1, local.FirstCaseRole.Type1) || Equal
              (local.SecondCaseRole.Type1, local.FirstCaseRole.Type1) && Lt
              (local.SecondCaseRole.StartDate, local.FirstCaseRole.StartDate) ||
              Equal(local.SecondCaseRole.Type1, local.FirstCaseRole.Type1) && Equal
              (local.SecondCaseRole.StartDate, local.FirstCaseRole.StartDate) &&
              Lt
              (local.SecondCsePersonsWorkSet.Number,
              local.FirstCsePersonsWorkSet.Number))
            {
              local.Validate.Update.AllCaseRoles.Assign(local.FirstCaseRole);
              local.Validate.Update.AllPersons.Number =
                local.FirstCsePersonsWorkSet.Number;
              local.Validate.Update.RowNumber.Count =
                local.FirstRowNumber.Count;
              local.Validate.Update.AllRowOperation.OneChar =
                local.FirstRowOperation.OneChar;

              local.Validate.Index = local.SortSubscript.Subscript - 1;
              local.Validate.CheckSize();

              local.Validate.Update.AllCaseRoles.Assign(local.SecondCaseRole);
              local.Validate.Update.AllPersons.Number =
                local.SecondCsePersonsWorkSet.Number;
              local.Validate.Update.RowNumber.Count =
                local.SecondRowNumber.Count;
              local.Validate.Update.AllRowOperation.OneChar =
                local.SecondRowOperation.OneChar;
              local.SortSubscript.Flag = "Y";
            }
          }
        }

        // ***********************************************
        // Check if there are no overlapping
        // time frames for AR.
        // ***********************************************
        for(local.Validate.Index = 0; local.Validate.Index < local
          .Validate.Count; ++local.Validate.Index)
        {
          if (!local.Validate.CheckSize())
          {
            break;
          }

          if (Equal(local.Validate.Item.AllCaseRoles.Type1, "AR"))
          {
            local.MinAr.Assign(local.Validate.Item.AllCaseRoles);
            local.MinSubscript.Subscript = local.Validate.Index + 1;
            export.ErrorRowNumber.Count = local.Validate.Item.RowNumber.Count;
            export.ErrorCase.Number = local.ValidatedCase.Number;

            // 11/15/2002 M.Lachowicz
            export.ErrorCsePersonsWorkSet.Number =
              local.Validate.Item.AllPersons.Number;

            // 11/15/2002 M.Lachowicz
            break;
          }
        }

        local.Validate.CheckIndex();

        // PR158462 M.L Start
        for(local.Validate.Index = 0; local.Validate.Index < local
          .Validate.Count; ++local.Validate.Index)
        {
          if (!local.Validate.CheckSize())
          {
            break;
          }

          if (Lt(local.Validate.Item.AllCaseRoles.StartDate,
            local.MinAr.StartDate))
          {
            export.ErrorNumber.Count = 6;
            export.ErrorRowNumber.Count = local.Validate.Item.RowNumber.Count;
            export.ErrorCase.Number = local.ValidatedCase.Number;
            export.ErrorCsePersonsWorkSet.Number =
              local.ValidatedCsePersonsWorkSet.Number;

            switch(TrimEnd(local.Validate.Item.AllCaseRoles.Type1))
            {
              case "CH":
                return;
              case "AP":
                return;
              default:
                break;
            }
          }
        }

        local.Validate.CheckIndex();

        // PR158462 M.L End
        for(local.Validate.Index = 0; local.Validate.Index < local
          .Validate.Count; ++local.Validate.Index)
        {
          if (!local.Validate.CheckSize())
          {
            break;
          }

          if (Lt(local.Validate.Item.AllCaseRoles.StartDate,
            local.MinAr.StartDate))
          {
            export.ErrorRowNumber.Count = local.Validate.Item.RowNumber.Count;
            export.ErrorCase.Number = local.ValidatedCase.Number;
            export.ErrorCsePersonsWorkSet.Number =
              local.ValidatedCsePersonsWorkSet.Number;
            export.ErrorNumber.Count = 3;

            return;
          }
        }

        local.Validate.CheckIndex();

        // PR157588 M.L Start
        local.ArChCombination.Flag = "N";

        for(local.Validate.Index = 0; local.Validate.Index < local
          .Validate.Count; ++local.Validate.Index)
        {
          if (!local.Validate.CheckSize())
          {
            break;
          }

          if (Equal(local.Validate.Item.AllCaseRoles.Type1, "CH"))
          {
            if (!Lt(local.MinAr.StartDate,
              local.Validate.Item.AllCaseRoles.StartDate))
            {
              local.ArChCombination.Flag = "Y";

              break;
            }
          }
        }

        local.Validate.CheckIndex();

        if (AsChar(local.ArChCombination.Flag) == 'N')
        {
          // 11/15/2002 M. Lachowicz
          local.Validate.Index = local.MinSubscript.Subscript - 1;
          local.Validate.CheckSize();

          export.ErrorRowNumber.Count = local.Validate.Item.RowNumber.Count;
          export.ErrorCase.Number = local.ValidatedCase.Number;
          export.ErrorCsePersonsWorkSet.Number =
            local.ValidatedCsePersonsWorkSet.Number;

          // 11/15/2002 M. Lachowicz
          export.ErrorNumber.Count = 5;

          return;
        }

        // PR157588 M.L End
        local.SortSubscript.Subscript = local.MinSubscript.Subscript;

        for(var limit1 = local.Validate.Count - 1; local
          .SortSubscript.Subscript <= limit1; ++local.SortSubscript.Subscript)
        {
          local.Validate.Index = local.SortSubscript.Subscript - 1;
          local.Validate.CheckSize();

          local.FirstCaseRole.Assign(local.Validate.Item.AllCaseRoles);
          local.FirstCsePersonsWorkSet.Number =
            local.Validate.Item.AllPersons.Number;
          local.FirstRowOperation.OneChar =
            local.Validate.Item.AllRowOperation.OneChar;
          local.FirstRowNumber.Count = local.Validate.Item.RowNumber.Count;

          local.Validate.Index = local.SortSubscript.Subscript;
          local.Validate.CheckSize();

          local.SecondCaseRole.Assign(local.Validate.Item.AllCaseRoles);
          local.SecondCsePersonsWorkSet.Number =
            local.Validate.Item.AllPersons.Number;
          local.SecondRowOperation.OneChar =
            local.Validate.Item.AllRowOperation.OneChar;
          local.SecondRowNumber.Count = local.Validate.Item.RowNumber.Count;

          if (!Equal(local.FirstCaseRole.Type1, "AR") || !
            Equal(local.SecondCaseRole.Type1, "AR"))
          {
            break;
          }

          if (!Lt(local.FirstCaseRole.EndDate, local.SecondCaseRole.StartDate))
          {
            if (IsEmpty(local.FirstRowOperation.OneChar))
            {
              export.ErrorRowNumber.Count = local.SecondRowNumber.Count;
              export.ErrorCsePersonsWorkSet.Number =
                local.SecondCsePersonsWorkSet.Number;
            }
            else
            {
              export.ErrorRowNumber.Count = local.FirstRowNumber.Count;
              export.ErrorCsePersonsWorkSet.Number =
                local.FirstCsePersonsWorkSet.Number;
            }

            export.ErrorCase.Number = local.ValidatedCase.Number;
            export.ErrorNumber.Count = 2;

            return;

            // This is error.
          }

          if (Lt(AddDays(local.FirstCaseRole.EndDate, 1),
            local.SecondCaseRole.StartDate))
          {
            if (Lt(import.ArGap.Number, local.ValidatedCase.Number) && AsChar
              (export.ArGapCommon.Flag) != 'Y')
            {
              export.ArGapCase.Number = local.ValidatedCase.Number;
              export.ArGapCommon.Flag = "Y";
            }

            for(local.Validate.Index = 0; local.Validate.Index < local
              .Validate.Count; ++local.Validate.Index)
            {
              if (!local.Validate.CheckSize())
              {
                break;
              }

              if (Lt(local.FirstCaseRole.EndDate,
                local.Validate.Item.AllCaseRoles.StartDate) && Lt
                (local.Validate.Item.AllCaseRoles.StartDate,
                local.SecondCaseRole.StartDate) || Lt
                (local.FirstCaseRole.EndDate,
                local.Validate.Item.AllCaseRoles.EndDate) && Lt
                (local.Validate.Item.AllCaseRoles.EndDate,
                local.SecondCaseRole.StartDate) || !
                Lt(local.FirstCaseRole.EndDate,
                local.Validate.Item.AllCaseRoles.StartDate) && Lt
                (local.FirstCaseRole.EndDate,
                local.Validate.Item.AllCaseRoles.EndDate))
              {
                export.ErrorRowNumber.Count =
                  local.Validate.Item.RowNumber.Count;
                export.ErrorCase.Number = local.ValidatedCase.Number;
                export.ErrorCsePersonsWorkSet.Number =
                  local.ValidatedCsePersonsWorkSet.Number;
                export.ErrorNumber.Count = 3;

                return;

                // This is error.
              }
            }

            local.Validate.CheckIndex();
          }
        }

        for(local.Validate.Index = local.Validate.Count - 1; local
          .Validate.Index >= 0; --local.Validate.Index)
        {
          if (!local.Validate.CheckSize())
          {
            break;
          }

          if (Equal(local.Validate.Item.AllCaseRoles.Type1, "AR"))
          {
            local.MaxAr.Assign(local.Validate.Item.AllCaseRoles);
            local.MaxAr.Identifier = local.Validate.Index + 1;

            break;
          }
        }

        local.Validate.CheckIndex();

        for(local.Validate.Index = 0; local.Validate.Index < local
          .Validate.Count; ++local.Validate.Index)
        {
          if (!local.Validate.CheckSize())
          {
            break;
          }

          if (Lt(local.MaxAr.EndDate, local.Validate.Item.AllCaseRoles.EndDate))
          {
            export.ErrorRowNumber.Count = local.Validate.Item.RowNumber.Count;
            export.ErrorCase.Number = local.ValidatedCase.Number;
            export.ErrorCsePersonsWorkSet.Number =
              local.ValidatedCsePersonsWorkSet.Number;
            export.ErrorNumber.Count = 3;

            return;
          }
        }

        local.Validate.CheckIndex();
      }
    }

    if (AsChar(export.ArGapCommon.Flag) == 'Y')
    {
      return;
    }

    local.Prev.Number = "";
    local.Common.Subscript = 1;

    for(var limit = import.Import1.Count; local.Common.Subscript <= limit; ++
      local.Common.Subscript)
    {
      import.Import1.Index = local.Common.Subscript - 1;
      import.Import1.CheckSize();

      if (!Equal(import.Import1.Item.Case1.Number, local.Prev.Number))
      {
        local.SubForFirstCaseOccur.Subscript = import.Import1.Index + 1;
        local.Prev.Number = import.Import1.Item.Case1.Number;

        if (ReadCase1())
        {
          local.Prev.CseOpenDate = entities.Case1.CseOpenDate;

          // 11/07/2002 M.Lachowicz Start
          local.CaseSaved.Flag = "N";

          // 11/07/2002 M.Lachowicz End
        }
        else
        {
          ExitState = "CASE_NF";

          return;
        }
      }

      // 11/07/2002 M.Lachowicz Start
      if (AsChar(local.CaseSaved.Flag) == 'Y')
      {
        continue;
      }

      // 11/07/2002 M.Lachowicz End
      if (AsChar(import.Import1.Item.RowOperation.OneChar) == 'A' || AsChar
        (import.Import1.Item.RowOperation.OneChar) == 'C')
      {
        // 11/07/2002 M.Lachowicz Start
        local.CaseSaved.Flag = "Y";

        // 11/07/2002 M.Lachowicz End
        local.Infrastructure.CsePersonNumber =
          import.Import1.Item.CsePersonsWorkSet.Number;
        local.Validate.Count = 0;
        local.Validate.Index = -1;

        foreach(var item in ReadCaseRoleCsePerson())
        {
          for(import.Import1.Index = local.SubForFirstCaseOccur.Subscript - 1; import
            .Import1.Index < import.Import1.Count; ++import.Import1.Index)
          {
            if (!import.Import1.CheckSize())
            {
              break;
            }

            if (!Equal(import.Import1.Item.Case1.Number, local.Prev.Number))
            {
              ++local.Validate.Index;
              local.Validate.CheckSize();

              MoveCaseRole1(entities.CaseRole,
                local.Validate.Update.AllCaseRoles);
              MoveCaseRole2(entities.CaseRole, local.Validate.Update.Original);
              local.Validate.Update.AllPersons.Number =
                entities.CsePerson.Number;

              goto ReadEach2;
            }

            if (entities.CaseRole.Identifier == import
              .Import1.Item.CaseRole.Identifier)
            {
              ++local.Validate.Index;
              local.Validate.CheckSize();

              local.Validate.Update.AllRowOperation.OneChar =
                import.Import1.Item.RowOperation.OneChar;
              local.Validate.Update.AllCaseRoles.Assign(
                import.Import1.Item.CaseRole);
              local.Validate.Update.AllPersons.Number =
                import.Import1.Item.CsePersonsWorkSet.Number;
              local.Validate.Update.Original.StartDate =
                entities.CaseRole.StartDate;
              local.Validate.Update.Original.EndDate =
                entities.CaseRole.EndDate;

              goto ReadEach2;
            }
          }

          import.Import1.CheckIndex();

          if (!Equal(import.Import1.Item.Case1.Number, local.Prev.Number))
          {
            ++local.Validate.Index;
            local.Validate.CheckSize();

            MoveCaseRole1(entities.CaseRole, local.Validate.Update.AllCaseRoles);
              
            local.Validate.Update.Original.StartDate =
              entities.CaseRole.StartDate;
            local.Validate.Update.Original.EndDate = entities.CaseRole.EndDate;
            local.Validate.Update.AllPersons.Number = entities.CsePerson.Number;

            continue;
          }

ReadEach2:
          ;
        }

        for(import.Import1.Index = local.SubForFirstCaseOccur.Subscript - 1; import
          .Import1.Index < import.Import1.Count; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          if (!Equal(import.Import1.Item.Case1.Number, local.Prev.Number))
          {
            break;
          }

          if (import.Import1.Item.CaseRole.Identifier == 0)
          {
            ++local.Validate.Index;
            local.Validate.CheckSize();

            local.Validate.Update.AllRowOperation.OneChar =
              import.Import1.Item.RowOperation.OneChar;
            MoveCaseRole2(import.Import1.Item.CaseRole,
              local.Validate.Update.Original);
            local.Validate.Update.AllCaseRoles.Assign(
              import.Import1.Item.CaseRole);
            local.Validate.Update.AllPersons.Number =
              import.Import1.Item.CsePersonsWorkSet.Number;
          }
        }

        import.Import1.CheckIndex();
        local.Validate.Index = 0;

        for(var limit1 = local.Validate.Count; local.Validate.Index < limit1; ++
          local.Validate.Index)
        {
          if (!local.Validate.CheckSize())
          {
            break;
          }

          switch(AsChar(local.Validate.Item.AllRowOperation.OneChar))
          {
            case 'A':
              if (Lt(local.Validate.Item.AllCaseRoles.StartDate,
                local.Prev.CseOpenDate))
              {
                local.Prev.CseOpenDate =
                  local.Validate.Item.AllCaseRoles.StartDate;
                local.UpdateCase.Flag = "Y";
              }

              UseSiRmadCreateCaseRole();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              break;
            case 'C':
              local.CsePerson.Number = local.Validate.Item.AllPersons.Number;

              if (ReadCaseRole())
              {
                local.Updated.Assign(entities.CaseRole);
                MoveCaseRole1(local.Validate.Item.AllCaseRoles, local.Updated);
              }
              else if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (Lt(local.Updated.StartDate, local.Prev.CseOpenDate))
              {
                local.Prev.CseOpenDate = local.Updated.StartDate;
                local.UpdateCase.Flag = "Y";
              }

              UseSiRmadUpdateCaseRole();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              // SI_UPDATE_CASE_ROLe
              break;
            default:
              break;
          }
        }

        local.Validate.CheckIndex();
        UseSiRmadCreateUpdateCaseUnit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.Infrastructure.CaseNumber = local.Prev.Number;
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.EventId = 5;
        local.Infrastructure.ReasonCode = "RMADCHANGE";
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.CaseUnitNumber = 0;
        local.Infrastructure.Detail =
          "Worker manually made the changes in the case :" + local.Prev.Number;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(local.UpdateCase.Flag) == 'Y')
        {
          local.UpdateCase.Flag = "N";

          if (ReadCase1())
          {
            try
            {
              UpdateCase();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  break;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        continue;
      }
    }
  }

  private static void MoveCaseRole1(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveCaseRole2(CaseRole source, CaseRole target)
  {
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveValidate(Local.ValidateGroup source,
    SiRmadCreateUpdateCaseUnit.Import.ValidateGroup target)
  {
    target.RowNumber.Count = source.RowNumber.Count;
    target.AllWorkOperation.OneChar = source.AllRowOperation.OneChar;
    target.AllPersons.Number = source.AllPersons.Number;
    target.CaseRoles.Assign(source.AllCaseRoles);
    MoveCaseRole2(source.Original, target.Original);
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiRmadCreateCaseRole()
  {
    var useImport = new SiRmadCreateCaseRole.Import();
    var useExport = new SiRmadCreateCaseRole.Export();

    useImport.CsePersonsWorkSet.Number = local.Validate.Item.AllPersons.Number;
    MoveCaseRole1(local.Validate.Item.AllCaseRoles, useImport.CaseRole);
    useImport.Case1.Number = local.Prev.Number;

    Call(SiRmadCreateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiRmadCreateUpdateCaseUnit()
  {
    var useImport = new SiRmadCreateUpdateCaseUnit.Import();
    var useExport = new SiRmadCreateUpdateCaseUnit.Export();

    local.Validate.CopyTo(useImport.Validate, MoveValidate);
    useImport.Case1.Number = local.Prev.Number;

    Call(SiRmadCreateUpdateCaseUnit.Execute, useImport, useExport);
  }

  private void UseSiRmadUpdateCaseRole()
  {
    var useImport = new SiRmadUpdateCaseRole.Import();
    var useExport = new SiRmadUpdateCaseRole.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.CaseRole.Assign(local.Updated);
    useImport.Case1.Number = local.Prev.Number;

    Call(SiRmadUpdateCaseRole.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Prev.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 4);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.ValidatedCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 4);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetInt32(
          command, "caseRoleId", local.Validate.Item.AllCaseRoles.Identifier);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 9);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 11);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 13);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.CaseRole.ParentType = db.GetNullableString(reader, 15);
        entities.CaseRole.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.CaseRole.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.CaseRole.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.CaseRole.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.CaseRole.ContactFirstName = db.GetNullableString(reader, 20);
        entities.CaseRole.ContactMiddleInitial =
          db.GetNullableString(reader, 21);
        entities.CaseRole.ContactPhone = db.GetNullableString(reader, 22);
        entities.CaseRole.ContactLastName = db.GetNullableString(reader, 23);
        entities.CaseRole.ChildCareExpenses = db.GetNullableDecimal(reader, 24);
        entities.CaseRole.AssignmentDate = db.GetNullableDate(reader, 25);
        entities.CaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 26);
        entities.CaseRole.AssignmentOfRights = db.GetNullableString(reader, 27);
        entities.CaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 28);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 29);
        entities.CaseRole.BirthCertFathersMi = db.GetNullableString(reader, 30);
        entities.CaseRole.BirthCertFathersFirstName =
          db.GetNullableString(reader, 31);
        entities.CaseRole.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 32);
        entities.CaseRole.BirthCertificateSignature =
          db.GetNullableString(reader, 33);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 34);
        entities.CaseRole.BirthCertFathersLastName =
          db.GetNullableString(reader, 35);
        entities.CaseRole.BornOutOfWedlock = db.GetNullableString(reader, 36);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 37);
        entities.CaseRole.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 38);
        entities.CaseRole.FcApNotified = db.GetNullableString(reader, 39);
        entities.CaseRole.FcCincInd = db.GetNullableString(reader, 40);
        entities.CaseRole.FcCostOfCare = db.GetNullableDecimal(reader, 41);
        entities.CaseRole.FcCostOfCareFreq = db.GetNullableString(reader, 42);
        entities.CaseRole.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 43);
        entities.CaseRole.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 44);
        entities.CaseRole.FcInHomeServiceInd = db.GetNullableString(reader, 45);
        entities.CaseRole.FcIvECaseNumber = db.GetNullableString(reader, 46);
        entities.CaseRole.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 47);
        entities.CaseRole.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 48);
        entities.CaseRole.FcLevelOfCare = db.GetNullableString(reader, 49);
        entities.CaseRole.FcNextJuvenileCtDt = db.GetNullableDate(reader, 50);
        entities.CaseRole.FcOrderEstBy = db.GetNullableString(reader, 51);
        entities.CaseRole.FcOtherBenefitInd = db.GetNullableString(reader, 52);
        entities.CaseRole.FcParentalRights = db.GetNullableString(reader, 53);
        entities.CaseRole.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 54);
        entities.CaseRole.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 55);
        entities.CaseRole.FcPlacementDate = db.GetNullableDate(reader, 56);
        entities.CaseRole.FcPlacementName = db.GetNullableString(reader, 57);
        entities.CaseRole.FcPlacementReason = db.GetNullableString(reader, 58);
        entities.CaseRole.FcPreviousPa = db.GetNullableString(reader, 59);
        entities.CaseRole.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 60);
        entities.CaseRole.FcSourceOfFunding = db.GetNullableString(reader, 61);
        entities.CaseRole.FcSrsPayee = db.GetNullableString(reader, 62);
        entities.CaseRole.FcSsa = db.GetNullableString(reader, 63);
        entities.CaseRole.FcSsi = db.GetNullableString(reader, 64);
        entities.CaseRole.FcVaInd = db.GetNullableString(reader, 65);
        entities.CaseRole.FcWardsAccount = db.GetNullableString(reader, 66);
        entities.CaseRole.FcZebInd = db.GetNullableString(reader, 67);
        entities.CaseRole.Over18AndInSchool = db.GetNullableString(reader, 68);
        entities.CaseRole.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 69);
        entities.CaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 70);
        entities.CaseRole.SpecialtyArea = db.GetNullableString(reader, 71);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 72);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 73);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 74);
        entities.CaseRole.CreatedBy = db.GetString(reader, 75);
        entities.CaseRole.ConfirmedType = db.GetNullableString(reader, 76);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 77);
        entities.CaseRole.ArChgProcReqInd = db.GetNullableString(reader, 78);
        entities.CaseRole.ArChgProcessedDate = db.GetNullableDate(reader, 79);
        entities.CaseRole.ArInvalidInd = db.GetNullableString(reader, 80);
        entities.CaseRole.Note = db.GetNullableString(reader, 81);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ParentType", entities.CaseRole.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.CaseRole.LivingWithArIndicator);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.CaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.CaseRole.SpecialtyArea);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 9);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 11);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 13);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.CaseRole.ParentType = db.GetNullableString(reader, 15);
        entities.CaseRole.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.CaseRole.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.CaseRole.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.CaseRole.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.CaseRole.ContactFirstName = db.GetNullableString(reader, 20);
        entities.CaseRole.ContactMiddleInitial =
          db.GetNullableString(reader, 21);
        entities.CaseRole.ContactPhone = db.GetNullableString(reader, 22);
        entities.CaseRole.ContactLastName = db.GetNullableString(reader, 23);
        entities.CaseRole.ChildCareExpenses = db.GetNullableDecimal(reader, 24);
        entities.CaseRole.AssignmentDate = db.GetNullableDate(reader, 25);
        entities.CaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 26);
        entities.CaseRole.AssignmentOfRights = db.GetNullableString(reader, 27);
        entities.CaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 28);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 29);
        entities.CaseRole.BirthCertFathersMi = db.GetNullableString(reader, 30);
        entities.CaseRole.BirthCertFathersFirstName =
          db.GetNullableString(reader, 31);
        entities.CaseRole.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 32);
        entities.CaseRole.BirthCertificateSignature =
          db.GetNullableString(reader, 33);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 34);
        entities.CaseRole.BirthCertFathersLastName =
          db.GetNullableString(reader, 35);
        entities.CaseRole.BornOutOfWedlock = db.GetNullableString(reader, 36);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 37);
        entities.CaseRole.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 38);
        entities.CaseRole.FcApNotified = db.GetNullableString(reader, 39);
        entities.CaseRole.FcCincInd = db.GetNullableString(reader, 40);
        entities.CaseRole.FcCostOfCare = db.GetNullableDecimal(reader, 41);
        entities.CaseRole.FcCostOfCareFreq = db.GetNullableString(reader, 42);
        entities.CaseRole.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 43);
        entities.CaseRole.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 44);
        entities.CaseRole.FcInHomeServiceInd = db.GetNullableString(reader, 45);
        entities.CaseRole.FcIvECaseNumber = db.GetNullableString(reader, 46);
        entities.CaseRole.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 47);
        entities.CaseRole.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 48);
        entities.CaseRole.FcLevelOfCare = db.GetNullableString(reader, 49);
        entities.CaseRole.FcNextJuvenileCtDt = db.GetNullableDate(reader, 50);
        entities.CaseRole.FcOrderEstBy = db.GetNullableString(reader, 51);
        entities.CaseRole.FcOtherBenefitInd = db.GetNullableString(reader, 52);
        entities.CaseRole.FcParentalRights = db.GetNullableString(reader, 53);
        entities.CaseRole.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 54);
        entities.CaseRole.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 55);
        entities.CaseRole.FcPlacementDate = db.GetNullableDate(reader, 56);
        entities.CaseRole.FcPlacementName = db.GetNullableString(reader, 57);
        entities.CaseRole.FcPlacementReason = db.GetNullableString(reader, 58);
        entities.CaseRole.FcPreviousPa = db.GetNullableString(reader, 59);
        entities.CaseRole.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 60);
        entities.CaseRole.FcSourceOfFunding = db.GetNullableString(reader, 61);
        entities.CaseRole.FcSrsPayee = db.GetNullableString(reader, 62);
        entities.CaseRole.FcSsa = db.GetNullableString(reader, 63);
        entities.CaseRole.FcSsi = db.GetNullableString(reader, 64);
        entities.CaseRole.FcVaInd = db.GetNullableString(reader, 65);
        entities.CaseRole.FcWardsAccount = db.GetNullableString(reader, 66);
        entities.CaseRole.FcZebInd = db.GetNullableString(reader, 67);
        entities.CaseRole.Over18AndInSchool = db.GetNullableString(reader, 68);
        entities.CaseRole.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 69);
        entities.CaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 70);
        entities.CaseRole.SpecialtyArea = db.GetNullableString(reader, 71);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 72);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 73);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 74);
        entities.CaseRole.CreatedBy = db.GetString(reader, 75);
        entities.CaseRole.ConfirmedType = db.GetNullableString(reader, 76);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 77);
        entities.CaseRole.ArChgProcReqInd = db.GetNullableString(reader, 78);
        entities.CaseRole.ArChgProcessedDate = db.GetNullableDate(reader, 79);
        entities.CaseRole.ArInvalidInd = db.GetNullableString(reader, 80);
        entities.CaseRole.Note = db.GetNullableString(reader, 81);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ParentType", entities.CaseRole.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.CaseRole.LivingWithArIndicator);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.CaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.CaseRole.SpecialtyArea);

        return true;
      });
  }

  private void UpdateCase()
  {
    var cseOpenDate = local.Prev.CseOpenDate;
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;

    entities.Case1.Populated = false;
    Update("UpdateCase",
      (db, command) =>
      {
        db.SetNullableDate(command, "cseOpenDate", cseOpenDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.CseOpenDate = cseOpenDate;
    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.Populated = true;
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
      /// A value of RowIndicator.
      /// </summary>
      [JsonPropertyName("rowIndicator")]
      public Common RowIndicator
      {
        get => rowIndicator ??= new();
        set => rowIndicator = value;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
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
      /// A value of RowNumber.
      /// </summary>
      [JsonPropertyName("rowNumber")]
      public Common RowNumber
      {
        get => rowNumber ??= new();
        set => rowNumber = value;
      }

      /// <summary>
      /// A value of RowOperation.
      /// </summary>
      [JsonPropertyName("rowOperation")]
      public Standard RowOperation
      {
        get => rowOperation ??= new();
        set => rowOperation = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common rowIndicator;
      private CaseRole caseRole;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Case1 case1;
      private Common rowNumber;
      private Standard rowOperation;
      private Office office;
      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// A value of ArGap.
    /// </summary>
    [JsonPropertyName("arGap")]
    public Case1 ArGap
    {
      get => arGap ??= new();
      set => arGap = value;
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Common PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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

    private Case1 arGap;
    private Common pageNumber;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ArGapCommon.
    /// </summary>
    [JsonPropertyName("arGapCommon")]
    public Common ArGapCommon
    {
      get => arGapCommon ??= new();
      set => arGapCommon = value;
    }

    /// <summary>
    /// A value of ArGapCase.
    /// </summary>
    [JsonPropertyName("arGapCase")]
    public Case1 ArGapCase
    {
      get => arGapCase ??= new();
      set => arGapCase = value;
    }

    /// <summary>
    /// A value of ErrorNumber.
    /// </summary>
    [JsonPropertyName("errorNumber")]
    public Common ErrorNumber
    {
      get => errorNumber ??= new();
      set => errorNumber = value;
    }

    /// <summary>
    /// A value of ErrorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("errorCsePersonsWorkSet")]
    public CsePersonsWorkSet ErrorCsePersonsWorkSet
    {
      get => errorCsePersonsWorkSet ??= new();
      set => errorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ErrorRowNumber.
    /// </summary>
    [JsonPropertyName("errorRowNumber")]
    public Common ErrorRowNumber
    {
      get => errorRowNumber ??= new();
      set => errorRowNumber = value;
    }

    /// <summary>
    /// A value of ErrorCase.
    /// </summary>
    [JsonPropertyName("errorCase")]
    public Case1 ErrorCase
    {
      get => errorCase ??= new();
      set => errorCase = value;
    }

    private Common arGapCommon;
    private Case1 arGapCase;
    private Common errorNumber;
    private CsePersonsWorkSet errorCsePersonsWorkSet;
    private Common errorRowNumber;
    private Case1 errorCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ValidateGroup group.</summary>
    [Serializable]
    public class ValidateGroup
    {
      /// <summary>
      /// A value of RowNumber.
      /// </summary>
      [JsonPropertyName("rowNumber")]
      public Common RowNumber
      {
        get => rowNumber ??= new();
        set => rowNumber = value;
      }

      /// <summary>
      /// A value of AllRowOperation.
      /// </summary>
      [JsonPropertyName("allRowOperation")]
      public Standard AllRowOperation
      {
        get => allRowOperation ??= new();
        set => allRowOperation = value;
      }

      /// <summary>
      /// A value of AllPersons.
      /// </summary>
      [JsonPropertyName("allPersons")]
      public CsePersonsWorkSet AllPersons
      {
        get => allPersons ??= new();
        set => allPersons = value;
      }

      /// <summary>
      /// A value of AllCaseRoles.
      /// </summary>
      [JsonPropertyName("allCaseRoles")]
      public CaseRole AllCaseRoles
      {
        get => allCaseRoles ??= new();
        set => allCaseRoles = value;
      }

      /// <summary>
      /// A value of Original.
      /// </summary>
      [JsonPropertyName("original")]
      public CaseRole Original
      {
        get => original ??= new();
        set => original = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common rowNumber;
      private Standard allRowOperation;
      private CsePersonsWorkSet allPersons;
      private CaseRole allCaseRoles;
      private CaseRole original;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Case1 New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of CaseSaved.
    /// </summary>
    [JsonPropertyName("caseSaved")]
    public Common CaseSaved
    {
      get => caseSaved ??= new();
      set => caseSaved = value;
    }

    /// <summary>
    /// A value of ArChCombination.
    /// </summary>
    [JsonPropertyName("arChCombination")]
    public Common ArChCombination
    {
      get => arChCombination ??= new();
      set => arChCombination = value;
    }

    /// <summary>
    /// A value of MaxAr.
    /// </summary>
    [JsonPropertyName("maxAr")]
    public CaseRole MaxAr
    {
      get => maxAr ??= new();
      set => maxAr = value;
    }

    /// <summary>
    /// A value of MinAr.
    /// </summary>
    [JsonPropertyName("minAr")]
    public CaseRole MinAr
    {
      get => minAr ??= new();
      set => minAr = value;
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
    /// A value of UpdateCase.
    /// </summary>
    [JsonPropertyName("updateCase")]
    public Common UpdateCase
    {
      get => updateCase ??= new();
      set => updateCase = value;
    }

    /// <summary>
    /// A value of SecondRowNumber.
    /// </summary>
    [JsonPropertyName("secondRowNumber")]
    public Common SecondRowNumber
    {
      get => secondRowNumber ??= new();
      set => secondRowNumber = value;
    }

    /// <summary>
    /// A value of FirstRowNumber.
    /// </summary>
    [JsonPropertyName("firstRowNumber")]
    public Common FirstRowNumber
    {
      get => firstRowNumber ??= new();
      set => firstRowNumber = value;
    }

    /// <summary>
    /// A value of SecondRowOperation.
    /// </summary>
    [JsonPropertyName("secondRowOperation")]
    public Standard SecondRowOperation
    {
      get => secondRowOperation ??= new();
      set => secondRowOperation = value;
    }

    /// <summary>
    /// A value of FirstRowOperation.
    /// </summary>
    [JsonPropertyName("firstRowOperation")]
    public Standard FirstRowOperation
    {
      get => firstRowOperation ??= new();
      set => firstRowOperation = value;
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
    /// A value of Updated.
    /// </summary>
    [JsonPropertyName("updated")]
    public CaseRole Updated
    {
      get => updated ??= new();
      set => updated = value;
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
    /// A value of FirstArCase.
    /// </summary>
    [JsonPropertyName("firstArCase")]
    public Common FirstArCase
    {
      get => firstArCase ??= new();
      set => firstArCase = value;
    }

    /// <summary>
    /// A value of SecondCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("secondCsePersonsWorkSet")]
    public CsePersonsWorkSet SecondCsePersonsWorkSet
    {
      get => secondCsePersonsWorkSet ??= new();
      set => secondCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SecondCaseRole.
    /// </summary>
    [JsonPropertyName("secondCaseRole")]
    public CaseRole SecondCaseRole
    {
      get => secondCaseRole ??= new();
      set => secondCaseRole = value;
    }

    /// <summary>
    /// A value of FirstCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("firstCsePersonsWorkSet")]
    public CsePersonsWorkSet FirstCsePersonsWorkSet
    {
      get => firstCsePersonsWorkSet ??= new();
      set => firstCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FirstCaseRole.
    /// </summary>
    [JsonPropertyName("firstCaseRole")]
    public CaseRole FirstCaseRole
    {
      get => firstCaseRole ??= new();
      set => firstCaseRole = value;
    }

    /// <summary>
    /// A value of SortSubscript.
    /// </summary>
    [JsonPropertyName("sortSubscript")]
    public Common SortSubscript
    {
      get => sortSubscript ??= new();
      set => sortSubscript = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of Ar1.
    /// </summary>
    [JsonPropertyName("ar1")]
    public CaseRole Ar1
    {
      get => ar1 ??= new();
      set => ar1 = value;
    }

    /// <summary>
    /// A value of Ar2.
    /// </summary>
    [JsonPropertyName("ar2")]
    public CaseRole Ar2
    {
      get => ar2 ??= new();
      set => ar2 = value;
    }

    /// <summary>
    /// Gets a value of Validate.
    /// </summary>
    [JsonIgnore]
    public Array<ValidateGroup> Validate => validate ??= new(
      ValidateGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Validate for json serialization.
    /// </summary>
    [JsonPropertyName("validate")]
    [Computed]
    public IList<ValidateGroup> Validate_Json
    {
      get => validate;
      set => Validate.Assign(value);
    }

    /// <summary>
    /// A value of SubForFirstCaseOccur.
    /// </summary>
    [JsonPropertyName("subForFirstCaseOccur")]
    public Common SubForFirstCaseOccur
    {
      get => subForFirstCaseOccur ??= new();
      set => subForFirstCaseOccur = value;
    }

    /// <summary>
    /// A value of RowOperation.
    /// </summary>
    [JsonPropertyName("rowOperation")]
    public Standard RowOperation
    {
      get => rowOperation ??= new();
      set => rowOperation = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of ValidatedCaseRole.
    /// </summary>
    [JsonPropertyName("validatedCaseRole")]
    public CaseRole ValidatedCaseRole
    {
      get => validatedCaseRole ??= new();
      set => validatedCaseRole = value;
    }

    /// <summary>
    /// A value of ValidatedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("validatedCsePersonsWorkSet")]
    public CsePersonsWorkSet ValidatedCsePersonsWorkSet
    {
      get => validatedCsePersonsWorkSet ??= new();
      set => validatedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ValidatedCase.
    /// </summary>
    [JsonPropertyName("validatedCase")]
    public Case1 ValidatedCase
    {
      get => validatedCase ??= new();
      set => validatedCase = value;
    }

    /// <summary>
    /// A value of Local1.
    /// </summary>
    [JsonPropertyName("local1")]
    public Common Local1
    {
      get => local1 ??= new();
      set => local1 = value;
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
    /// A value of MinSubscript.
    /// </summary>
    [JsonPropertyName("minSubscript")]
    public Common MinSubscript
    {
      get => minSubscript ??= new();
      set => minSubscript = value;
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

    private Case1 new1;
    private Common caseSaved;
    private Common arChCombination;
    private CaseRole maxAr;
    private CaseRole minAr;
    private DateWorkArea current;
    private Common updateCase;
    private Common secondRowNumber;
    private Common firstRowNumber;
    private Standard secondRowOperation;
    private Standard firstRowOperation;
    private DateWorkArea max;
    private CaseRole updated;
    private CsePerson csePerson;
    private Common firstArCase;
    private CsePersonsWorkSet secondCsePersonsWorkSet;
    private CaseRole secondCaseRole;
    private CsePersonsWorkSet firstCsePersonsWorkSet;
    private CaseRole firstCaseRole;
    private Common sortSubscript;
    private DateWorkArea zeroDate;
    private CaseRole ar1;
    private CaseRole ar2;
    private Array<ValidateGroup> validate;
    private Common subForFirstCaseOccur;
    private Standard rowOperation;
    private Case1 prev;
    private CaseRole validatedCaseRole;
    private CsePersonsWorkSet validatedCsePersonsWorkSet;
    private Case1 validatedCase;
    private Common local1;
    private Common common;
    private Common minSubscript;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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

    private CsePerson csePerson;
    private Case1 case1;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private CaseUnit caseUnit;
    private CaseRole caseRole;
  }
#endregion
}
