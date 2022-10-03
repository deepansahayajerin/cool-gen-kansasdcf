// Program: SI_EMPH_EMPLOYER_CHANGE_HIST, ID: 1902589420, model: 746.
// Short name: SWEEMPHP
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
/// A program: SI_EMPH_EMPLOYER_CHANGE_HIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiEmphEmployerChangeHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EMPH_EMPLOYER_CHANGE_HIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEmphEmployerChangeHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEmphEmployerChangeHist.
  /// </summary>
  public SiEmphEmployerChangeHist(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 09/21/16  DDupree	CQ46988		Initial Code.  Created from a copy of EIWH
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    MoveStandard(import.Standard, export.Standard);
    export.Employer1.Assign(import.Employer1);
    export.EmployerAddress.Assign(import.EmployerAddress);
    export.Minus.OneChar = import.Minus.OneChar;
    export.Plus.OneChar = import.Plus.OneChar;

    // -- Set display colors.
    local.Severity.Text8 = "";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // -- Move import group to export group.  This is done after the CLEAR 
    // processing so
    //    that the group will be cleared when the user presses the CLEAR 
    // function key.
    for(import.Employer.Index = 0; import.Employer.Index < import
      .Employer.Count; ++import.Employer.Index)
    {
      if (!import.Employer.CheckSize())
      {
        break;
      }

      export.Employer.Index = import.Employer.Index;
      export.Employer.CheckSize();

      export.Employer.Update.EmployerHistory.Assign(
        import.Employer.Item.EmployerHistory);
      export.Employer.Update.EmployerHistoryDetail.LineNumber =
        import.Employer.Item.EmployerHistoryDetail.LineNumber;
      export.Employer.Update.Sel.SelectChar =
        import.Employer.Item.Sel.SelectChar;
      export.Employer.Update.StartStop.Flag =
        import.Employer.Item.StartAndStop.Flag;
      export.Employer.Update.Change.Text80 = import.Employer.Item.Change.Text80;
    }

    import.Employer.CheckIndex();

    for(import.Paging.Index = 0; import.Paging.Index < import.Paging.Count; ++
      import.Paging.Index)
    {
      if (!import.Paging.CheckSize())
      {
        break;
      }

      export.Paging.Index = import.Paging.Index;
      export.Paging.CheckSize();

      export.Paging.Update.PagingEmployerHistory.CreatedTimestamp =
        import.Paging.Item.PagingEmployerHistory.CreatedTimestamp;
      export.Paging.Update.PagingEmployerHistoryDetail.LineNumber =
        import.Paging.Item.PagingEmployerHistoryDetail.LineNumber;
    }

    import.Paging.CheckIndex();

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    local.CurrentDate.Date = Now().Date;

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.UpdateOk.Flag = "Y";
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        return;
      case "LIST":
        if (AsChar(import.PromptEmployer.Flag) == 'S')
        {
          ExitState = "ECO_LNK_TO_EMPLOYER";

          return;
        }

        break;
      case "RETURN":
        ExitState = "ECO_XFR_TO_EMPLOYER_MAINTENANCE";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "DISPLAY":
        if (export.Employer1.Identifier <= 0)
        {
          var field1 = GetField(export.Employer1, "name");

          field1.Error = true;

          var field2 = GetField(export.Employer1, "ein");

          field2.Error = true;

          ExitState = "MUST_SELECT_EMPLOYER_FROM_EMPL";

          break;
        }

        local.Employer1.Identifier = import.Employer1.Identifier;
        local.ScrollEmployerHistory.CreatedTimestamp = Now();
        local.ScrollEmployerHistoryDetail.LineNumber = 0;
        UseSiReadEmployerHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.Employer1, "name");

          field1.Error = true;

          var field2 = GetField(export.Employer1, "ein");

          field2.Error = true;

          break;
        }

        if (export.Employer.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        export.Standard.PageNumber = 1;

        if (export.Employer.IsFull)
        {
          export.Plus.OneChar = "+";

          export.Paging.Index = 0;
          export.Paging.CheckSize();

          export.Employer.Index = 0;
          export.Employer.CheckSize();

          export.Paging.Update.PagingEmployerHistory.CreatedTimestamp =
            local.ScrollEmployerHistory.CreatedTimestamp;
          export.Paging.Update.PagingEmployerHistoryDetail.LineNumber =
            local.ScrollEmployerHistoryDetail.LineNumber;

          ++export.Paging.Index;
          export.Paging.CheckSize();

          export.Employer.Index = Export.EmployerGroup.Capacity - 1;
          export.Employer.CheckSize();

          export.Paging.Update.PagingEmployerHistory.CreatedTimestamp =
            export.Employer.Item.EmployerHistory.CreatedTimestamp;

          for(export.Employer.Index = export.Employer.Count - 1; export
            .Employer.Index >= 0; --export.Employer.Index)
          {
            if (!export.Employer.CheckSize())
            {
              break;
            }

            if (!Equal(export.Paging.Item.PagingEmployerHistory.
              CreatedTimestamp,
              export.Employer.Item.EmployerHistory.CreatedTimestamp))
            {
              export.Employer.Count = export.Employer.Index + 1;

              goto Test1;
            }
          }

          export.Employer.CheckIndex();
        }
        else
        {
          export.Plus.OneChar = "";
        }

Test1:

        if (export.Employer.Index == -1)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        for(export.Employer.Index = 0; export.Employer.Index < export
          .Employer.Count; ++export.Employer.Index)
        {
          if (!export.Employer.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Employer.Item.StartStop.Flag))
          {
            if (AsChar(export.Employer.Item.StartStop.Flag) == 'Y')
            {
              var field1 = GetField(export.Employer.Item.Sel, "selectChar");

              field1.Color = "green";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
            }
            else if (AsChar(export.Employer.Item.StartStop.Flag) == 'N')
            {
              var field1 = GetField(export.Employer.Item.Change, "text80");

              field1.Color = "green";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
            }
          }
        }

        export.Employer.CheckIndex();

        break;
      case "UPDATE":
        for(export.Employer.Index = 0; export.Employer.Index < export
          .Employer.Count; ++export.Employer.Index)
        {
          if (!export.Employer.CheckSize())
          {
            break;
          }

          if (AsChar(export.Employer.Item.Sel.SelectChar) == 'S')
          {
            ++local.GroupCount.Count;
            MoveEmployerHistory(export.Employer.Item.EmployerHistory, local.Last);
              
          }

          if (AsChar(export.Employer.Item.StartStop.Flag) == 'N' && Equal
            (export.Employer.Item.EmployerHistory.CreatedTimestamp,
            local.Last.CreatedTimestamp))
          {
            if (Equal(export.Employer.Item.Change.Text80,
              export.Employer.Item.EmployerHistory.Note))
            {
              var field1 = GetField(export.Employer.Item.Change, "text80");

              field1.Error = true;

              ExitState = "NO_CHANGES_WERE_MADE";

              goto Test3;
            }

            local.Last.Note = export.Employer.Item.Change.Text80;
          }
        }

        export.Employer.CheckIndex();

        switch(local.GroupCount.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            goto Test3;
          case 1:
            break;
          default:
            for(export.Employer.Index = 0; export.Employer.Index < export
              .Employer.Count; ++export.Employer.Index)
            {
              if (!export.Employer.CheckSize())
              {
                break;
              }

              if (AsChar(export.Employer.Item.Sel.SelectChar) == 'S')
              {
                var field1 = GetField(export.Employer.Item.Sel, "selectChar");

                field1.Error = true;
              }
              else
              {
              }
            }

            export.Employer.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            goto Test3;
        }

        UseSiUpdateEmployerHistory();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        export.Employer.Index = 0;

        for(var limit = export.Employer.Count; export.Employer.Index < limit; ++
          export.Employer.Index)
        {
          if (!export.Employer.CheckSize())
          {
            break;
          }

          if (AsChar(export.Employer.Item.Sel.SelectChar) == 'S')
          {
            export.Employer.Update.Sel.SelectChar = "*";

            if (AsChar(export.Employer.Item.StartStop.Flag) == 'Y')
            {
              export.Employer.Update.EmployerHistory.Note = local.Last.Note ?? ""
                ;
              local.Action.Date =
                export.Employer.Item.EmployerHistory.ActionDate;
              UseCabDate2TextWithHyphens();
              export.Employer.Update.Change.Text80 =
                (export.Employer.Item.EmployerHistory.ActionTaken ?? "") + "  " +
                local.EffectDate.Text10 + "  " + (
                  local.Last.LastUpdatedBy ?? "");
              export.Employer.Update.EmployerHistory.LastUpdatedTimestamp =
                local.Last.LastUpdatedTimestamp;
            }
          }
        }

        export.Employer.CheckIndex();

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

        local.ScrollEmployerHistory.CreatedTimestamp =
          export.Paging.Item.PagingEmployerHistory.CreatedTimestamp;
        local.ScrollEmployerHistoryDetail.LineNumber =
          export.Paging.Item.PagingEmployerHistoryDetail.LineNumber;
        local.Employer1.Identifier = export.Employer1.Identifier;
        UseSiReadEmployerHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (export.Standard.PageNumber > 1)
        {
          export.Minus.OneChar = "-";
        }
        else
        {
          export.Minus.OneChar = "";
        }

        export.Plus.OneChar = "+";

        export.Employer.Index = Export.EmployerGroup.Capacity - 1;
        export.Employer.CheckSize();

        local.ScrollEmployerHistory.CreatedTimestamp =
          export.Employer.Item.EmployerHistory.CreatedTimestamp;

        for(export.Employer.Index = export.Employer.Count - 1; export
          .Employer.Index >= 0; --export.Employer.Index)
        {
          if (!export.Employer.CheckSize())
          {
            break;
          }

          if (!Equal(local.ScrollEmployerHistory.CreatedTimestamp,
            export.Employer.Item.EmployerHistory.CreatedTimestamp))
          {
            export.Employer.Count = export.Employer.Index + 1;

            break;
          }
        }

        export.Employer.CheckIndex();

        for(export.Employer.Index = 0; export.Employer.Index < export
          .Employer.Count; ++export.Employer.Index)
        {
          if (!export.Employer.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Employer.Item.StartStop.Flag))
          {
            if (AsChar(export.Employer.Item.StartStop.Flag) == 'Y')
            {
              var field1 = GetField(export.Employer.Item.Sel, "selectChar");

              field1.Color = "green";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
            }
            else if (AsChar(export.Employer.Item.StartStop.Flag) == 'N')
            {
              var field1 = GetField(export.Employer.Item.Change, "text80");

              field1.Color = "green";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
            }
          }
        }

        export.Employer.CheckIndex();

        break;
      case "NEXT":
        if (export.Standard.PageNumber == Export.PagingGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          break;
        }

        if (IsEmpty(import.Plus.OneChar))
        {
          local.GroupCount.Count = 0;

          for(export.Employer.Index = 0; export.Employer.Index < export
            .Employer.Count; ++export.Employer.Index)
          {
            if (!export.Employer.CheckSize())
            {
              break;
            }

            if (Lt(local.NullDateWorkArea.Timestamp,
              export.Employer.Item.EmployerHistory.CreatedTimestamp))
            {
              ++local.GroupCount.Count;
              local.ScrollEmployerHistory.CreatedTimestamp =
                export.Employer.Item.EmployerHistory.CreatedTimestamp;
              local.ScrollEmployerHistoryDetail.LineNumber =
                export.Employer.Item.EmployerHistoryDetail.LineNumber;
            }
          }

          export.Employer.CheckIndex();

          if (local.GroupCount.Count < Export.EmployerGroup.Capacity)
          {
            ExitState = "ACO_NE0000_INVALID_FORWARD";

            break;
          }
        }

        ++export.Standard.PageNumber;

        export.Paging.Index = export.Standard.PageNumber - 1;
        export.Paging.CheckSize();

        if (Lt(local.NullDateWorkArea.Timestamp,
          export.Paging.Item.PagingEmployerHistory.CreatedTimestamp))
        {
          local.ScrollEmployerHistory.CreatedTimestamp =
            export.Paging.Item.PagingEmployerHistory.CreatedTimestamp;
        }

        local.Employer1.Identifier = export.Employer1.Identifier;
        UseSiReadEmployerHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        export.Minus.OneChar = "-";

        if (export.Employer.IsFull)
        {
          export.Plus.OneChar = "+";

          if (export.Paging.Index + 1 == Export.PagingGroup.Capacity)
          {
            ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

            break;
          }

          ++export.Paging.Index;
          export.Paging.CheckSize();

          export.Employer.Index = Export.EmployerGroup.Capacity - 1;
          export.Employer.CheckSize();

          export.Paging.Update.PagingEmployerHistory.CreatedTimestamp =
            export.Employer.Item.EmployerHistory.CreatedTimestamp;

          for(export.Employer.Index = export.Employer.Count - 1; export
            .Employer.Index >= 0; --export.Employer.Index)
          {
            if (!export.Employer.CheckSize())
            {
              break;
            }

            if (!Equal(export.Paging.Item.PagingEmployerHistory.
              CreatedTimestamp,
              export.Employer.Item.EmployerHistory.CreatedTimestamp))
            {
              export.Employer.Count = export.Employer.Index + 1;

              goto Test2;
            }
          }

          export.Employer.CheckIndex();
        }
        else
        {
          export.Plus.OneChar = "";
        }

Test2:

        for(export.Employer.Index = 0; export.Employer.Index < export
          .Employer.Count; ++export.Employer.Index)
        {
          if (!export.Employer.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Employer.Item.StartStop.Flag))
          {
            if (AsChar(export.Employer.Item.StartStop.Flag) == 'Y')
            {
              var field1 = GetField(export.Employer.Item.Sel, "selectChar");

              field1.Color = "green";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
            }
            else if (AsChar(export.Employer.Item.StartStop.Flag) == 'N')
            {
              var field1 = GetField(export.Employer.Item.Change, "text80");

              field1.Color = "green";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
            }
          }
        }

        export.Employer.CheckIndex();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test3:

    if (!export.Employer.IsEmpty)
    {
      for(export.Employer.Index = 0; export.Employer.Index < export
        .Employer.Count; ++export.Employer.Index)
      {
        if (!export.Employer.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Employer.Item.StartStop.Flag))
        {
          if (AsChar(export.Employer.Item.StartStop.Flag) == 'Y')
          {
            var field1 = GetField(export.Employer.Item.Sel, "selectChar");

            field1.Color = "green";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;
          }
          else if (AsChar(export.Employer.Item.StartStop.Flag) == 'N')
          {
            var field1 = GetField(export.Employer.Item.Change, "text80");

            field1.Color = "green";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;
          }
        }
      }

      export.Employer.CheckIndex();
    }
  }

  private static void MoveEmployerHistory(EmployerHistory source,
    EmployerHistory target)
  {
    target.Note = source.Note;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveHistToEmployer(SiReadEmployerHistory.Export.
    HistGroup source, Export.EmployerGroup target)
  {
    target.Sel.SelectChar = source.Select.SelectChar;
    target.Change.Text80 = source.Data.Text80;
    target.EmployerHistory.Assign(source.EmployerHistory);
    target.EmployerHistoryDetail.LineNumber =
      source.EmployerHistoryDetail.LineNumber;
    target.StartStop.Flag = source.StartStop.Flag;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.Action.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.EffectDate.Text10 = useExport.TextWorkArea.Text10;
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

  private void UseSiReadEmployerHistory()
  {
    var useImport = new SiReadEmployerHistory.Import();
    var useExport = new SiReadEmployerHistory.Export();

    useImport.Scroll.CreatedTimestamp =
      local.ScrollEmployerHistory.CreatedTimestamp;
    useImport.Ws.Identifier = local.Employer1.Identifier;

    Call(SiReadEmployerHistory.Execute, useImport, useExport);

    export.EmployerAddress.Assign(useExport.WorksiteEmployerAddress);
    export.Employer1.Assign(useExport.WorksiteEmployer);
    useExport.Hist.CopyTo(export.Employer, MoveHistToEmployer);
  }

  private void UseSiUpdateEmployerHistory()
  {
    var useImport = new SiUpdateEmployerHistory.Import();
    var useExport = new SiUpdateEmployerHistory.Export();

    useImport.EmployerHistory.Assign(local.Last);
    useImport.Employer.Identifier = export.Employer1.Identifier;

    Call(SiUpdateEmployerHistory.Execute, useImport, useExport);

    local.Last.Assign(useExport.EmployerHistory);
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
    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of PagingEmployerHistoryDetail.
      /// </summary>
      [JsonPropertyName("pagingEmployerHistoryDetail")]
      public EmployerHistoryDetail PagingEmployerHistoryDetail
      {
        get => pagingEmployerHistoryDetail ??= new();
        set => pagingEmployerHistoryDetail = value;
      }

      /// <summary>
      /// A value of PagingEmployerHistory.
      /// </summary>
      [JsonPropertyName("pagingEmployerHistory")]
      public EmployerHistory PagingEmployerHistory
      {
        get => pagingEmployerHistory ??= new();
        set => pagingEmployerHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private EmployerHistoryDetail pagingEmployerHistoryDetail;
      private EmployerHistory pagingEmployerHistory;
    }

    /// <summary>A EmployerGroup group.</summary>
    [Serializable]
    public class EmployerGroup
    {
      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of Change.
      /// </summary>
      [JsonPropertyName("change")]
      public WorkArea Change
      {
        get => change ??= new();
        set => change = value;
      }

      /// <summary>
      /// A value of EmployerHistory.
      /// </summary>
      [JsonPropertyName("employerHistory")]
      public EmployerHistory EmployerHistory
      {
        get => employerHistory ??= new();
        set => employerHistory = value;
      }

      /// <summary>
      /// A value of EmployerHistoryDetail.
      /// </summary>
      [JsonPropertyName("employerHistoryDetail")]
      public EmployerHistoryDetail EmployerHistoryDetail
      {
        get => employerHistoryDetail ??= new();
        set => employerHistoryDetail = value;
      }

      /// <summary>
      /// A value of StartAndStop.
      /// </summary>
      [JsonPropertyName("startAndStop")]
      public Common StartAndStop
      {
        get => startAndStop ??= new();
        set => startAndStop = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common sel;
      private WorkArea change;
      private EmployerHistory employerHistory;
      private EmployerHistoryDetail employerHistoryDetail;
      private Common startAndStop;
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
    /// Gets a value of Employer.
    /// </summary>
    [JsonIgnore]
    public Array<EmployerGroup> Employer => employer ??= new(
      EmployerGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Employer for json serialization.
    /// </summary>
    [JsonPropertyName("employer")]
    [Computed]
    public IList<EmployerGroup> Employer_Json
    {
      get => employer;
      set => Employer.Assign(value);
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of PromptEmployer.
    /// </summary>
    [JsonPropertyName("promptEmployer")]
    public Common PromptEmployer
    {
      get => promptEmployer ??= new();
      set => promptEmployer = value;
    }

    /// <summary>
    /// A value of Employer1.
    /// </summary>
    [JsonPropertyName("employer1")]
    public Employer Employer1
    {
      get => employer1 ??= new();
      set => employer1 = value;
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
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Standard Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    private Array<PagingGroup> paging;
    private Array<EmployerGroup> employer;
    private EmployerAddress employerAddress;
    private Common promptEmployer;
    private Employer employer1;
    private Standard standard;
    private NextTranInfo hidden;
    private Standard plus;
    private Standard minus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of PagingEmployerHistoryDetail.
      /// </summary>
      [JsonPropertyName("pagingEmployerHistoryDetail")]
      public EmployerHistoryDetail PagingEmployerHistoryDetail
      {
        get => pagingEmployerHistoryDetail ??= new();
        set => pagingEmployerHistoryDetail = value;
      }

      /// <summary>
      /// A value of PagingEmployerHistory.
      /// </summary>
      [JsonPropertyName("pagingEmployerHistory")]
      public EmployerHistory PagingEmployerHistory
      {
        get => pagingEmployerHistory ??= new();
        set => pagingEmployerHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private EmployerHistoryDetail pagingEmployerHistoryDetail;
      private EmployerHistory pagingEmployerHistory;
    }

    /// <summary>A EmployerGroup group.</summary>
    [Serializable]
    public class EmployerGroup
    {
      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of Change.
      /// </summary>
      [JsonPropertyName("change")]
      public WorkArea Change
      {
        get => change ??= new();
        set => change = value;
      }

      /// <summary>
      /// A value of EmployerHistory.
      /// </summary>
      [JsonPropertyName("employerHistory")]
      public EmployerHistory EmployerHistory
      {
        get => employerHistory ??= new();
        set => employerHistory = value;
      }

      /// <summary>
      /// A value of EmployerHistoryDetail.
      /// </summary>
      [JsonPropertyName("employerHistoryDetail")]
      public EmployerHistoryDetail EmployerHistoryDetail
      {
        get => employerHistoryDetail ??= new();
        set => employerHistoryDetail = value;
      }

      /// <summary>
      /// A value of StartStop.
      /// </summary>
      [JsonPropertyName("startStop")]
      public Common StartStop
      {
        get => startStop ??= new();
        set => startStop = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common sel;
      private WorkArea change;
      private EmployerHistory employerHistory;
      private EmployerHistoryDetail employerHistoryDetail;
      private Common startStop;
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of PromptEmployer.
    /// </summary>
    [JsonPropertyName("promptEmployer")]
    public Common PromptEmployer
    {
      get => promptEmployer ??= new();
      set => promptEmployer = value;
    }

    /// <summary>
    /// A value of Employer1.
    /// </summary>
    [JsonPropertyName("employer1")]
    public Employer Employer1
    {
      get => employer1 ??= new();
      set => employer1 = value;
    }

    /// <summary>
    /// Gets a value of Employer.
    /// </summary>
    [JsonIgnore]
    public Array<EmployerGroup> Employer => employer ??= new(
      EmployerGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Employer for json serialization.
    /// </summary>
    [JsonPropertyName("employer")]
    [Computed]
    public IList<EmployerGroup> Employer_Json
    {
      get => employer;
      set => Employer.Assign(value);
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
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Standard Minus
    {
      get => minus ??= new();
      set => minus = value;
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

    private Array<PagingGroup> paging;
    private EmployerAddress employerAddress;
    private Common promptEmployer;
    private Employer employer1;
    private Array<EmployerGroup> employer;
    private Standard standard;
    private NextTranInfo hidden;
    private Standard plus;
    private Standard minus;
    private Case1 next;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public EmployerHistory Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public EmployerHistory Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of ScrollEmployerHistoryDetail.
    /// </summary>
    [JsonPropertyName("scrollEmployerHistoryDetail")]
    public EmployerHistoryDetail ScrollEmployerHistoryDetail
    {
      get => scrollEmployerHistoryDetail ??= new();
      set => scrollEmployerHistoryDetail = value;
    }

    /// <summary>
    /// A value of ScrollEmployerHistory.
    /// </summary>
    [JsonPropertyName("scrollEmployerHistory")]
    public EmployerHistory ScrollEmployerHistory
    {
      get => scrollEmployerHistory ??= new();
      set => scrollEmployerHistory = value;
    }

    /// <summary>
    /// A value of Severity.
    /// </summary>
    [JsonPropertyName("severity")]
    public TextWorkArea Severity
    {
      get => severity ??= new();
      set => severity = value;
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

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of NullNextTranInfo.
    /// </summary>
    [JsonPropertyName("nullNextTranInfo")]
    public NextTranInfo NullNextTranInfo
    {
      get => nullNextTranInfo ??= new();
      set => nullNextTranInfo = value;
    }

    /// <summary>
    /// A value of Employer1.
    /// </summary>
    [JsonPropertyName("employer1")]
    public Employer Employer1
    {
      get => employer1 ??= new();
      set => employer1 = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

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
    /// A value of UpdateOk.
    /// </summary>
    [JsonPropertyName("updateOk")]
    public Common UpdateOk
    {
      get => updateOk ??= new();
      set => updateOk = value;
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
    /// A value of EffectDate.
    /// </summary>
    [JsonPropertyName("effectDate")]
    public TextWorkArea EffectDate
    {
      get => effectDate ??= new();
      set => effectDate = value;
    }

    /// <summary>
    /// A value of Action.
    /// </summary>
    [JsonPropertyName("action")]
    public DateWorkArea Action
    {
      get => action ??= new();
      set => action = value;
    }

    private EmployerHistory update;
    private EmployerHistory last;
    private EmployerHistoryDetail scrollEmployerHistoryDetail;
    private EmployerHistory scrollEmployerHistory;
    private TextWorkArea severity;
    private Common command;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private NextTranInfo nullNextTranInfo;
    private Employer employer1;
    private DateWorkArea nullDateWorkArea;
    private Common common;
    private AbendData abendData;
    private Common groupCount;
    private Common updateOk;
    private DateWorkArea currentDate;
    private TextWorkArea effectDate;
    private DateWorkArea action;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of IwoActionHistory.
    /// </summary>
    [JsonPropertyName("iwoActionHistory")]
    public IwoActionHistory IwoActionHistory
    {
      get => iwoActionHistory ??= new();
      set => iwoActionHistory = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Employer1.
    /// </summary>
    [JsonPropertyName("employer1")]
    public Employer Employer1
    {
      get => employer1 ??= new();
      set => employer1 = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Sdu.
    /// </summary>
    [JsonPropertyName("sdu")]
    public Fips Sdu
    {
      get => sdu ??= new();
      set => sdu = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
    }

    private DocumentField documentField;
    private FieldValue fieldValue;
    private Field field;
    private IwoActionHistory iwoActionHistory;
    private Document document;
    private IwoAction iwoAction;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private LegalActionIncomeSource legalActionIncomeSource;
    private IwoTransaction iwoTransaction;
    private IncomeSource incomeSource;
    private Employer employer1;
    private CsePerson csePerson;
    private FipsTribAddress fipsTribAddress;
    private Fips sdu;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private CodeValue codeValue;
    private Code code;
    private Profile profile;
    private ServiceProviderProfile serviceProviderProfile;
    private ServiceProvider serviceProvider;
    private EmployerHistory employerHistory;
  }
#endregion
}
