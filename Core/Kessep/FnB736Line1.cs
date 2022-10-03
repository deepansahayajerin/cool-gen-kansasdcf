// Program: FN_B736_LINE1, ID: 1902573552, model: 746.
// Short name: SWE03762
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B736_LINE1.
/// </summary>
[Serializable]
public partial class FnB736Line1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B736_LINE1 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB736Line1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB736Line1.
  /// </summary>
  public FnB736Line1(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.Create.YearMonth = import.StatsReport.YearMonth.GetValueOrDefault();
    local.Create.FirstRunNumber =
      import.StatsReport.FirstRunNumber.GetValueOrDefault();

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.RestartOffice.SystemGeneratedId =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 4, 4));
      local.RestartServiceProvider.SystemGeneratedId =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 8, 5));
      local.RestartCase.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 13, 10);
      local.OpenCases.Column1 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 23, 5));
      local.OpenCases.Column2 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 28, 5));
      local.OpenCases.Column3 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 33, 5));
      local.OpenCases.Column4 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 38, 5));
      local.OpenCases.Column5 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 43, 5));
      local.OpenCases.Column6 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 48, 5));
      local.OpenCases.Column7 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 53, 5));
      local.OpenCases.Column8 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 58, 5));
      local.OpenCases.Column9 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 63, 5));
      local.OpenCases.Column10 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 68, 5));
      local.OpenCases.Column11 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 73, 5));
      local.OpenCases.Column12 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 78, 5));
      local.OpenCases.Column13 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 83, 5));
      local.OpenCases.Column15 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 88, 6));
      local.ActiveChildren.Column1 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 94, 5));
      local.ActiveChildren.Column2 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 99, 5));
      local.ActiveChildren.Column3 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 104, 5));
      local.ActiveChildren.Column4 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 109, 5));
      local.ActiveChildren.Column5 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 114, 5));
      local.ActiveChildren.Column6 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 119, 5));
      local.ActiveChildren.Column7 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 124, 5));
      local.ActiveChildren.Column8 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 129, 5));
      local.ActiveChildren.Column9 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 134, 5));
      local.ActiveChildren.Column10 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 139, 5));
      local.ActiveChildren.Column11 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 144, 5));
      local.ActiveChildren.Column12 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 149, 5));
      local.ActiveChildren.Column13 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 154, 5));
      local.ActiveChildren.Column15 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 159, 6));
    }

    // ------------------------------------------
    // Read open cases as of report end date.
    // ------------------------------------------
    foreach(var item in ReadCaseCaseAssignmentOfficeOfficeServiceProvider())
    {
      if (Equal(entities.Case1.Number, local.PrevCase.Number))
      {
        continue;
      }

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() || entities
        .ServiceProvider.SystemGeneratedId != local
        .PrevServiceProvider.SystemGeneratedId)
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "01 " + NumberToString
          (local.PrevOffice.SystemGeneratedId, 12, 4);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.PrevServiceProvider.SystemGeneratedId, 11, 5);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + local
          .PrevCase.Number;
        local.StateColumn1.Text5 =
          NumberToString(local.OpenCases.Column1.GetValueOrDefault(), 11, 5);
        local.StateColumn2.Text5 =
          NumberToString(local.OpenCases.Column2.GetValueOrDefault(), 11, 5);
        local.StateColumn3.Text5 =
          NumberToString(local.OpenCases.Column3.GetValueOrDefault(), 11, 5);
        local.StateColumn4.Text5 =
          NumberToString(local.OpenCases.Column4.GetValueOrDefault(), 11, 5);
        local.StateColumn5.Text5 =
          NumberToString(local.OpenCases.Column5.GetValueOrDefault(), 11, 5);
        local.StateColumn6.Text5 =
          NumberToString(local.OpenCases.Column6.GetValueOrDefault(), 11, 5);
        local.StateColumn7.Text5 =
          NumberToString(local.OpenCases.Column7.GetValueOrDefault(), 11, 5);
        local.StateColumn8.Text5 =
          NumberToString(local.OpenCases.Column8.GetValueOrDefault(), 11, 5);
        local.StateColumn9.Text5 =
          NumberToString(local.OpenCases.Column9.GetValueOrDefault(), 11, 5);
        local.StateColumn10.Text5 =
          NumberToString(local.OpenCases.Column10.GetValueOrDefault(), 11, 5);
        local.StateColumn11.Text5 =
          NumberToString(local.OpenCases.Column11.GetValueOrDefault(), 11, 5);
        local.StateColumn12.Text5 =
          NumberToString(local.OpenCases.Column12.GetValueOrDefault(), 11, 5);
        local.StateColumn13.Text5 =
          NumberToString(local.OpenCases.Column13.GetValueOrDefault(), 11, 5);
        local.StateColumn15.Text6 =
          NumberToString(local.OpenCases.Column15.GetValueOrDefault(), 10, 6);
        local.State.Text80 = local.StateColumn1.Text5 + local
          .StateColumn2.Text5 + local.StateColumn3.Text5 + local
          .StateColumn4.Text5 + local.StateColumn5.Text5 + local
          .StateColumn6.Text5 + local.StateColumn7.Text5 + local
          .StateColumn8.Text5 + local.StateColumn9.Text5 + local
          .StateColumn10.Text5 + local.StateColumn11.Text5 + local
          .StateColumn12.Text5 + local.StateColumn13.Text5 + local
          .StateColumn15.Text6 + "";
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + local
          .State.Text80;
        local.ChildColumn1.Text5 =
          NumberToString(local.ActiveChildren.Column1.GetValueOrDefault(), 11, 5);
          
        local.ChildColumn2.Text5 =
          NumberToString(local.ActiveChildren.Column2.GetValueOrDefault(), 11, 5);
          
        local.ChildColumn3.Text5 =
          NumberToString(local.ActiveChildren.Column3.GetValueOrDefault(), 11, 5);
          
        local.ChildColumn4.Text5 =
          NumberToString(local.ActiveChildren.Column4.GetValueOrDefault(), 11, 5);
          
        local.ChildColumn5.Text5 =
          NumberToString(local.ActiveChildren.Column5.GetValueOrDefault(), 11, 5);
          
        local.ChildColumn6.Text5 =
          NumberToString(local.ActiveChildren.Column6.GetValueOrDefault(), 11, 5);
          
        local.ChildColumn7.Text5 =
          NumberToString(local.ActiveChildren.Column7.GetValueOrDefault(), 11, 5);
          
        local.ChildColumn8.Text5 =
          NumberToString(local.ActiveChildren.Column8.GetValueOrDefault(), 11, 5);
          
        local.ChildColumn9.Text5 =
          NumberToString(local.ActiveChildren.Column9.GetValueOrDefault(), 11, 5);
          
        local.ChildColumn10.Text5 =
          NumberToString(local.ActiveChildren.Column10.GetValueOrDefault(), 11,
          5);
        local.ChildColumn11.Text5 =
          NumberToString(local.ActiveChildren.Column11.GetValueOrDefault(), 11,
          5);
        local.ChildColumn12.Text5 =
          NumberToString(local.ActiveChildren.Column12.GetValueOrDefault(), 11,
          5);
        local.ChildColumn13.Text5 =
          NumberToString(local.ActiveChildren.Column13.GetValueOrDefault(), 11,
          5);
        local.ChildColumn15.Text6 =
          NumberToString(local.ActiveChildren.Column15.GetValueOrDefault(), 10,
          6);
        local.Child.Text80 = local.ChildColumn1.Text5 + local
          .ChildColumn2.Text5 + local.ChildColumn3.Text5 + local
          .ChildColumn4.Text5 + local.ChildColumn5.Text5 + local
          .ChildColumn6.Text5 + local.ChildColumn7.Text5 + local
          .ChildColumn8.Text5 + local.ChildColumn9.Text5 + local
          .ChildColumn10.Text5 + local.ChildColumn11.Text5 + local
          .ChildColumn12.Text5 + local.ChildColumn13.Text5 + local
          .ChildColumn15.Text6;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + local
          .Child.Text80;
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Error.LineNumber = 1;
          local.Error.CaseNumber = entities.Case1.Number;
          local.Error.OfficeId = entities.Office.SystemGeneratedId;
          UseFnB717WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }

      local.PrevCase.Number = entities.Case1.Number;
      local.PrevOffice.SystemGeneratedId = entities.Office.SystemGeneratedId;
      local.PrevServiceProvider.SystemGeneratedId =
        entities.ServiceProvider.SystemGeneratedId;
      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
        local.PrevOfficeServiceProvider);
      UseFnB717ProgHierarchy4Case();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.Create.LineNumber = 1;
        local.Create.CaseNumber = entities.Case1.Number;
        local.Create.CaseWrkRole = entities.OfficeServiceProvider.RoleCode;
        local.Create.ServicePrvdrId =
          entities.ServiceProvider.SystemGeneratedId;
        local.Create.OfficeId = entities.Office.SystemGeneratedId;
        local.Create.ProgramType = local.Program.Code;
        UseFnB717CreateStatsVerifi();
      }

      switch(TrimEnd(local.Program.Code))
      {
        case "AF":
          // colum 1
          local.OpenCases.Column1 =
            local.OpenCases.Column1.GetValueOrDefault() + 1;

          break;
        case "FC":
          // colum 5
          local.OpenCases.Column5 =
            local.OpenCases.Column5.GetValueOrDefault() + 1;

          break;
        case "NF":
          // colum 6
          local.OpenCases.Column6 =
            local.OpenCases.Column6.GetValueOrDefault() + 1;

          break;
        case "NC":
          // colum 7
          local.OpenCases.Column7 =
            local.OpenCases.Column7.GetValueOrDefault() + 1;

          break;
        case "NA":
          // colum 8
          local.OpenCases.Column8 =
            local.OpenCases.Column8.GetValueOrDefault() + 1;

          break;
        case "NAS":
          // colum 3
          local.OpenCases.Column3 =
            local.OpenCases.Column3.GetValueOrDefault() + 1;

          break;
        case "NAN":
          // colum 4
          local.OpenCases.Column4 =
            local.OpenCases.Column4.GetValueOrDefault() + 1;

          break;
        case "IKS":
          // colum 11
          local.OpenCases.Column11 =
            local.OpenCases.Column11.GetValueOrDefault() + 1;

          break;
        case "AFI":
          // colum 10
          local.OpenCases.Column10 =
            local.OpenCases.Column10.GetValueOrDefault() + 1;

          break;
        case "NAI":
          // colum 9
          local.OpenCases.Column9 =
            local.OpenCases.Column9.GetValueOrDefault() + 1;

          break;
        case "XA":
          // colum 2
          local.OpenCases.Column2 =
            local.OpenCases.Column2.GetValueOrDefault() + 1;

          break;
        case "PAF":
          // colum 13
          local.OpenCases.Column13 =
            local.OpenCases.Column13.GetValueOrDefault() + 1;

          break;
        case "PNA":
          // colum 12
          local.OpenCases.Column12 =
            local.OpenCases.Column12.GetValueOrDefault() + 1;

          break;
        default:
          break;
      }

      local.OpenCases.Column15 =
        local.OpenCases.Column15.GetValueOrDefault() + 1;

      // -------------------------------------
      // CH roles active during report period,
      // -------------------------------------
      local.PrevCsePerson.Number = "";

      foreach(var item1 in ReadCsePersonCaseRole())
      {
        // -------------------------------------
        // Count each child only once per case.
        // -------------------------------------
        if (Equal(entities.ChCsePerson.Number, local.PrevCsePerson.Number))
        {
          continue;
        }

        local.PrevCsePerson.Number = entities.ChCsePerson.Number;
        UseFnB717ProgHierarchy4Person();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 7;
          local.Create.ProgramType = local.Program.Code;
          local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
          local.Create.CaseRoleType = entities.ChCaseRole.Type1;
          UseFnB717CreateStatsVerifi();
        }

        switch(TrimEnd(local.Program.Code))
        {
          case "AF":
            // colum 1
            local.ActiveChildren.Column1 =
              local.ActiveChildren.Column1.GetValueOrDefault() + 1;

            break;
          case "FC":
            // colum 5
            local.ActiveChildren.Column5 =
              local.ActiveChildren.Column5.GetValueOrDefault() + 1;

            break;
          case "NF":
            // colum 6
            local.ActiveChildren.Column6 =
              local.ActiveChildren.Column6.GetValueOrDefault() + 1;

            break;
          case "NC":
            // colum 7
            local.ActiveChildren.Column7 =
              local.ActiveChildren.Column7.GetValueOrDefault() + 1;

            break;
          case "NA":
            // colum 8
            local.ActiveChildren.Column8 =
              local.ActiveChildren.Column8.GetValueOrDefault() + 1;

            break;
          case "NAS":
            // colum 3
            local.ActiveChildren.Column3 =
              local.ActiveChildren.Column3.GetValueOrDefault() + 1;

            break;
          case "NAN":
            // colum 4
            local.ActiveChildren.Column4 =
              local.ActiveChildren.Column4.GetValueOrDefault() + 1;

            break;
          case "IKS":
            // colum 11
            local.ActiveChildren.Column11 =
              local.ActiveChildren.Column11.GetValueOrDefault() + 1;

            break;
          case "AFI":
            // colum 10
            local.ActiveChildren.Column10 =
              local.ActiveChildren.Column10.GetValueOrDefault() + 1;

            break;
          case "NAI":
            // colum 9
            local.ActiveChildren.Column9 =
              local.ActiveChildren.Column9.GetValueOrDefault() + 1;

            break;
          case "XA":
            // colum 2
            local.ActiveChildren.Column2 =
              local.ActiveChildren.Column2.GetValueOrDefault() + 1;

            break;
          case "PAF":
            // colum 13
            local.ActiveChildren.Column13 =
              local.ActiveChildren.Column13.GetValueOrDefault() + 1;

            break;
          case "PNA":
            // colum 12
            local.ActiveChildren.Column12 =
              local.ActiveChildren.Column12.GetValueOrDefault() + 1;

            break;
          default:
            break;
        }

        local.ActiveChildren.Column15 =
          local.ActiveChildren.Column15.GetValueOrDefault() + 1;
      }

      ++local.CommitCnt.Count;
    }

    local.Blank.Text9 = "";
    local.Blank.Text13 = "";
    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column1.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn1.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn1.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column2.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn2.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn2.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column3.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn3.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn3.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column4.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn4.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn4.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column5.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn5.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn5.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column6.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn6.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn6.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column7.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn7.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn7.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column8.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn8.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn8.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column9.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn9.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn9.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column10.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn10.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn10.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column11.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn11.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn11.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column12.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn12.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn12.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column13.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn13.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.StateColumn13.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.OpenCases.Column15.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.StateColumn15.Text13 =
        Substring(local.Blank.Text13, WorkArea.Text13_MaxLength, 1, 12) + "0";
    }
    else
    {
      local.StateColumn15.Text13 =
        Substring(local.Blank.Text13, WorkArea.Text13_MaxLength, 1, 13 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Write.RptDetail = local.StateColumn15.Text13 + local
      .StateColumn1.Text9 + local.StateColumn2.Text9 + local
      .StateColumn3.Text9 + local.StateColumn4.Text9 + local
      .StateColumn5.Text9 + local.StateColumn6.Text9 + local
      .StateColumn7.Text9 + local.StateColumn8.Text9 + local
      .StateColumn9.Text9 + local.StateColumn10.Text9 + local
      .StateColumn11.Text9 + local.StateColumn12.Text9 + local
      .StateColumn13.Text9;
    local.EabFileHandling.Action = "WRITE";
    UseCabBusinessReport01();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing statewide report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail = "Active Childern";
    UseCabBusinessReport01();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing statewide report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column1.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn1.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn1.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column2.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn2.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn2.Text9 =
        Substring(local.Transfer.Text15, local.Verify.Count, 15 - local
        .Verify.Count + 1);
      local.ChildColumn2.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column3.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn3.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn3.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column4.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn4.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn4.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column5.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn5.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn5.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column6.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn6.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn6.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column7.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn7.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn7.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column8.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn8.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn8.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column9.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn9.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn9.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column10.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn10.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn10.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column11.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn11.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn11.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column12.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn12.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn12.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column13.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn13.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 8) + "0";
    }
    else
    {
      local.ChildColumn13.Text9 =
        Substring(local.Transfer.Text15, local.Verify.Count, 15 - local
        .Verify.Count + 1);
      local.ChildColumn13.Text9 =
        Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1, 9 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Transfer.Text15 =
      NumberToString(local.ActiveChildren.Column15.GetValueOrDefault(), 15);
    local.Verify.Count = Verify(local.Transfer.Text15, "0");

    if (local.Verify.Count <= 0)
    {
      local.ChildColumn15.Text13 = "0";
      local.ChildColumn15.Text13 =
        Substring(local.Blank.Text13, WorkArea.Text13_MaxLength, 1, 12) + "0";
    }
    else
    {
      local.ChildColumn15.Text13 =
        Substring(local.Transfer.Text15, local.Verify.Count, 15 - local
        .Verify.Count + 1);
      local.ChildColumn15.Text13 =
        Substring(local.Blank.Text13, WorkArea.Text13_MaxLength, 1, 13 - (15 - (
          local.Verify.Count - 1))) + Substring
        (local.Transfer.Text15, WorkArea.Text15_MaxLength, local.Verify.Count,
        15 - local.Verify.Count + 1);
    }

    local.Write.RptDetail = local.ChildColumn15.Text13 + local
      .ChildColumn1.Text9 + local.ChildColumn2.Text9 + local
      .ChildColumn3.Text9 + local.ChildColumn4.Text9 + local
      .ChildColumn5.Text9 + local.ChildColumn6.Text9 + local
      .ChildColumn7.Text9 + local.ChildColumn8.Text9 + local
      .ChildColumn9.Text9 + local.ChildColumn10.Text9 + local
      .ChildColumn11.Text9 + local.ChildColumn12.Text9 + local
      .ChildColumn13.Text9;
    local.EabFileHandling.Action = "WRITE";
    UseCabBusinessReport01();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing statewide report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveStatsVerifi(StatsVerifi source, StatsVerifi target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
    target.LineNumber = source.LineNumber;
    target.ProgramType = source.ProgramType;
    target.ServicePrvdrId = source.ServicePrvdrId;
    target.OfficeId = source.OfficeId;
    target.CaseWrkRole = source.CaseWrkRole;
    target.ParentId = source.ParentId;
    target.ChiefId = source.ChiefId;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.Dddd = source.Dddd;
    target.DebtDetailBaldue = source.DebtDetailBaldue;
    target.ObligationType = source.ObligationType;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.CollCreatedDate = source.CollCreatedDate;
    target.CaseRoleType = source.CaseRoleType;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.Open);
    useImport.NeededToWrite.RptDetail = local.Write.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB717CreateStatsVerifi()
  {
    var useImport = new FnB717CreateStatsVerifi.Import();
    var useExport = new FnB717CreateStatsVerifi.Export();

    MoveStatsVerifi(local.Create, useImport.StatsVerifi);

    Call(FnB717CreateStatsVerifi.Execute, useImport, useExport);
  }

  private void UseFnB717ProgHierarchy4Case()
  {
    var useImport = new FnB717ProgHierarchy4Case.Import();
    var useExport = new FnB717ProgHierarchy4Case.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717ProgHierarchy4Case.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseFnB717ProgHierarchy4Person()
  {
    var useImport = new FnB717ProgHierarchy4Person.Import();
    var useExport = new FnB717ProgHierarchy4Person.Export();

    useImport.CsePerson.Assign(entities.ChCsePerson);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717ProgHierarchy4Person.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseFnB717WriteError()
  {
    var useImport = new FnB717WriteError.Import();
    var useExport = new FnB717WriteError.Export();

    useImport.Error.Assign(local.Error);

    Call(FnB717WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCaseCaseAssignmentOfficeOfficeServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseCaseAssignmentOfficeOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetInt32(command, "officeId1", local.RestartOffice.SystemGeneratedId);
          
        db.SetInt32(
          command, "spdId", local.RestartServiceProvider.SystemGeneratedId);
        db.SetString(command, "casNo", local.RestartCase.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId2", import.From.OfficeId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId3", import.To.OfficeId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 5);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 8);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole()
  {
    entities.ChCsePerson.Populated = false;
    entities.ChCaseRole.Populated = false;

    return ReadEach("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ChCsePerson.Type1 = db.GetString(reader, 1);
        entities.ChCsePerson.BornOutOfWedlock = db.GetNullableString(reader, 2);
        entities.ChCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 3);
        entities.ChCaseRole.CasNumber = db.GetString(reader, 4);
        entities.ChCaseRole.Type1 = db.GetString(reader, 5);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 6);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 7);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 8);
        entities.ChCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 9);
        entities.ChCsePerson.Populated = true;
        entities.ChCaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);

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
    /// <summary>
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public StatsReport To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public StatsReport From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    private StatsReport statsReport;
    private StatsReport to;
    private StatsReport from;
    private DateWorkArea reportStartDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common displayInd;
    private DateWorkArea reportEndDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public WorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public Common Verify
    {
      get => verify ??= new();
      set => verify = value;
    }

    /// <summary>
    /// A value of Transfer.
    /// </summary>
    [JsonPropertyName("transfer")]
    public WorkArea Transfer
    {
      get => transfer ??= new();
      set => transfer = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public WorkArea Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public WorkArea State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of ChildColumn1.
    /// </summary>
    [JsonPropertyName("childColumn1")]
    public WorkArea ChildColumn1
    {
      get => childColumn1 ??= new();
      set => childColumn1 = value;
    }

    /// <summary>
    /// A value of ChildColumn2.
    /// </summary>
    [JsonPropertyName("childColumn2")]
    public WorkArea ChildColumn2
    {
      get => childColumn2 ??= new();
      set => childColumn2 = value;
    }

    /// <summary>
    /// A value of ChildColumn3.
    /// </summary>
    [JsonPropertyName("childColumn3")]
    public WorkArea ChildColumn3
    {
      get => childColumn3 ??= new();
      set => childColumn3 = value;
    }

    /// <summary>
    /// A value of ChildColumn4.
    /// </summary>
    [JsonPropertyName("childColumn4")]
    public WorkArea ChildColumn4
    {
      get => childColumn4 ??= new();
      set => childColumn4 = value;
    }

    /// <summary>
    /// A value of ChildColumn5.
    /// </summary>
    [JsonPropertyName("childColumn5")]
    public WorkArea ChildColumn5
    {
      get => childColumn5 ??= new();
      set => childColumn5 = value;
    }

    /// <summary>
    /// A value of ChildColumn6.
    /// </summary>
    [JsonPropertyName("childColumn6")]
    public WorkArea ChildColumn6
    {
      get => childColumn6 ??= new();
      set => childColumn6 = value;
    }

    /// <summary>
    /// A value of ChildColumn7.
    /// </summary>
    [JsonPropertyName("childColumn7")]
    public WorkArea ChildColumn7
    {
      get => childColumn7 ??= new();
      set => childColumn7 = value;
    }

    /// <summary>
    /// A value of ChildColumn8.
    /// </summary>
    [JsonPropertyName("childColumn8")]
    public WorkArea ChildColumn8
    {
      get => childColumn8 ??= new();
      set => childColumn8 = value;
    }

    /// <summary>
    /// A value of ChildColumn9.
    /// </summary>
    [JsonPropertyName("childColumn9")]
    public WorkArea ChildColumn9
    {
      get => childColumn9 ??= new();
      set => childColumn9 = value;
    }

    /// <summary>
    /// A value of ChildColumn10.
    /// </summary>
    [JsonPropertyName("childColumn10")]
    public WorkArea ChildColumn10
    {
      get => childColumn10 ??= new();
      set => childColumn10 = value;
    }

    /// <summary>
    /// A value of ChildColumn11.
    /// </summary>
    [JsonPropertyName("childColumn11")]
    public WorkArea ChildColumn11
    {
      get => childColumn11 ??= new();
      set => childColumn11 = value;
    }

    /// <summary>
    /// A value of ChildColumn12.
    /// </summary>
    [JsonPropertyName("childColumn12")]
    public WorkArea ChildColumn12
    {
      get => childColumn12 ??= new();
      set => childColumn12 = value;
    }

    /// <summary>
    /// A value of ChildColumn13.
    /// </summary>
    [JsonPropertyName("childColumn13")]
    public WorkArea ChildColumn13
    {
      get => childColumn13 ??= new();
      set => childColumn13 = value;
    }

    /// <summary>
    /// A value of ChildColumn15.
    /// </summary>
    [JsonPropertyName("childColumn15")]
    public WorkArea ChildColumn15
    {
      get => childColumn15 ??= new();
      set => childColumn15 = value;
    }

    /// <summary>
    /// A value of StateColumn1.
    /// </summary>
    [JsonPropertyName("stateColumn1")]
    public WorkArea StateColumn1
    {
      get => stateColumn1 ??= new();
      set => stateColumn1 = value;
    }

    /// <summary>
    /// A value of StateColumn2.
    /// </summary>
    [JsonPropertyName("stateColumn2")]
    public WorkArea StateColumn2
    {
      get => stateColumn2 ??= new();
      set => stateColumn2 = value;
    }

    /// <summary>
    /// A value of StateColumn3.
    /// </summary>
    [JsonPropertyName("stateColumn3")]
    public WorkArea StateColumn3
    {
      get => stateColumn3 ??= new();
      set => stateColumn3 = value;
    }

    /// <summary>
    /// A value of StateColumn4.
    /// </summary>
    [JsonPropertyName("stateColumn4")]
    public WorkArea StateColumn4
    {
      get => stateColumn4 ??= new();
      set => stateColumn4 = value;
    }

    /// <summary>
    /// A value of StateColumn5.
    /// </summary>
    [JsonPropertyName("stateColumn5")]
    public WorkArea StateColumn5
    {
      get => stateColumn5 ??= new();
      set => stateColumn5 = value;
    }

    /// <summary>
    /// A value of StateColumn6.
    /// </summary>
    [JsonPropertyName("stateColumn6")]
    public WorkArea StateColumn6
    {
      get => stateColumn6 ??= new();
      set => stateColumn6 = value;
    }

    /// <summary>
    /// A value of StateColumn7.
    /// </summary>
    [JsonPropertyName("stateColumn7")]
    public WorkArea StateColumn7
    {
      get => stateColumn7 ??= new();
      set => stateColumn7 = value;
    }

    /// <summary>
    /// A value of StateColumn8.
    /// </summary>
    [JsonPropertyName("stateColumn8")]
    public WorkArea StateColumn8
    {
      get => stateColumn8 ??= new();
      set => stateColumn8 = value;
    }

    /// <summary>
    /// A value of StateColumn9.
    /// </summary>
    [JsonPropertyName("stateColumn9")]
    public WorkArea StateColumn9
    {
      get => stateColumn9 ??= new();
      set => stateColumn9 = value;
    }

    /// <summary>
    /// A value of StateColumn10.
    /// </summary>
    [JsonPropertyName("stateColumn10")]
    public WorkArea StateColumn10
    {
      get => stateColumn10 ??= new();
      set => stateColumn10 = value;
    }

    /// <summary>
    /// A value of StateColumn11.
    /// </summary>
    [JsonPropertyName("stateColumn11")]
    public WorkArea StateColumn11
    {
      get => stateColumn11 ??= new();
      set => stateColumn11 = value;
    }

    /// <summary>
    /// A value of StateColumn12.
    /// </summary>
    [JsonPropertyName("stateColumn12")]
    public WorkArea StateColumn12
    {
      get => stateColumn12 ??= new();
      set => stateColumn12 = value;
    }

    /// <summary>
    /// A value of StateColumn13.
    /// </summary>
    [JsonPropertyName("stateColumn13")]
    public WorkArea StateColumn13
    {
      get => stateColumn13 ??= new();
      set => stateColumn13 = value;
    }

    /// <summary>
    /// A value of StateColumn15.
    /// </summary>
    [JsonPropertyName("stateColumn15")]
    public WorkArea StateColumn15
    {
      get => stateColumn15 ??= new();
      set => stateColumn15 = value;
    }

    /// <summary>
    /// A value of ActiveChildren.
    /// </summary>
    [JsonPropertyName("activeChildren")]
    public StatsReport ActiveChildren
    {
      get => activeChildren ??= new();
      set => activeChildren = value;
    }

    /// <summary>
    /// A value of OpenCases.
    /// </summary>
    [JsonPropertyName("openCases")]
    public StatsReport OpenCases
    {
      get => openCases ??= new();
      set => openCases = value;
    }

    /// <summary>
    /// A value of Sup.
    /// </summary>
    [JsonPropertyName("sup")]
    public ServiceProvider Sup
    {
      get => sup ??= new();
      set => sup = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public StatsVerifi Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public StatsVerifi Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of PrevOffice.
    /// </summary>
    [JsonPropertyName("prevOffice")]
    public Office PrevOffice
    {
      get => prevOffice ??= new();
      set => prevOffice = value;
    }

    /// <summary>
    /// A value of PrevOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("prevOfficeServiceProvider")]
    public OfficeServiceProvider PrevOfficeServiceProvider
    {
      get => prevOfficeServiceProvider ??= new();
      set => prevOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public StatsVerifi Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of PrevServiceProvider.
    /// </summary>
    [JsonPropertyName("prevServiceProvider")]
    public ServiceProvider PrevServiceProvider
    {
      get => prevServiceProvider ??= new();
      set => prevServiceProvider = value;
    }

    /// <summary>
    /// A value of RestartServiceProvider.
    /// </summary>
    [JsonPropertyName("restartServiceProvider")]
    public ServiceProvider RestartServiceProvider
    {
      get => restartServiceProvider ??= new();
      set => restartServiceProvider = value;
    }

    /// <summary>
    /// A value of RestartOffice.
    /// </summary>
    [JsonPropertyName("restartOffice")]
    public Office RestartOffice
    {
      get => restartOffice ??= new();
      set => restartOffice = value;
    }

    /// <summary>
    /// A value of RestartOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("restartOfficeServiceProvider")]
    public OfficeServiceProvider RestartOfficeServiceProvider
    {
      get => restartOfficeServiceProvider ??= new();
      set => restartOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of RestartCase.
    /// </summary>
    [JsonPropertyName("restartCase")]
    public Case1 RestartCase
    {
      get => restartCase ??= new();
      set => restartCase = value;
    }

    /// <summary>
    /// A value of PrevCase.
    /// </summary>
    [JsonPropertyName("prevCase")]
    public Case1 PrevCase
    {
      get => prevCase ??= new();
      set => prevCase = value;
    }

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    /// <summary>
    /// A value of PrevCsePerson.
    /// </summary>
    [JsonPropertyName("prevCsePerson")]
    public CsePerson PrevCsePerson
    {
      get => prevCsePerson ??= new();
      set => prevCsePerson = value;
    }

    /// <summary>
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabReportSend Write
    {
      get => write ??= new();
      set => write = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private WorkArea blank;
    private Common verify;
    private WorkArea transfer;
    private WorkArea child;
    private WorkArea state;
    private WorkArea childColumn1;
    private WorkArea childColumn2;
    private WorkArea childColumn3;
    private WorkArea childColumn4;
    private WorkArea childColumn5;
    private WorkArea childColumn6;
    private WorkArea childColumn7;
    private WorkArea childColumn8;
    private WorkArea childColumn9;
    private WorkArea childColumn10;
    private WorkArea childColumn11;
    private WorkArea childColumn12;
    private WorkArea childColumn13;
    private WorkArea childColumn15;
    private WorkArea stateColumn1;
    private WorkArea stateColumn2;
    private WorkArea stateColumn3;
    private WorkArea stateColumn4;
    private WorkArea stateColumn5;
    private WorkArea stateColumn6;
    private WorkArea stateColumn7;
    private WorkArea stateColumn8;
    private WorkArea stateColumn9;
    private WorkArea stateColumn10;
    private WorkArea stateColumn11;
    private WorkArea stateColumn12;
    private WorkArea stateColumn13;
    private WorkArea stateColumn15;
    private StatsReport activeChildren;
    private StatsReport openCases;
    private ServiceProvider sup;
    private StatsVerifi null1;
    private StatsVerifi error;
    private Office prevOffice;
    private OfficeServiceProvider prevOfficeServiceProvider;
    private StatsVerifi create;
    private ServiceProvider prevServiceProvider;
    private ServiceProvider restartServiceProvider;
    private Office restartOffice;
    private OfficeServiceProvider restartOfficeServiceProvider;
    private Program program;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restartCase;
    private Case1 prevCase;
    private Common commitCnt;
    private Common subscript;
    private CsePerson prevCsePerson;
    private Common firstTime;
    private EabFileHandling eabFileHandling;
    private EabReportSend open;
    private EabReportSend write;
    private EabReportSend eabReportSend;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private CsePerson chCsePerson;
    private CaseRole chCaseRole;
    private CsePerson ap;
    private CsePersonAccount supported;
  }
#endregion
}
