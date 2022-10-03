// Program: SI_RMAD_DISPLAY_PAGE, ID: 373475613, model: 746.
// Short name: SWE00729
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_RMAD_DISPLAY_PAGE.
/// </summary>
[Serializable]
public partial class SiRmadDisplayPage: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RMAD_DISPLAY_PAGE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRmadDisplayPage(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRmadDisplayPage.
  /// </summary>
  public SiRmadDisplayPage(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    UseCabSetMaximumDiscontinueDate();
    local.PageNumber.TotalInteger = 1;

    import.Import1.Index = 0;
    import.Import1.CheckSize();

    export.Display.Index = -1;
    MoveCase1(import.Import1.Item.Case1, local.PrevCase);
    local.PrevOffice.Name = import.Import1.Item.Office.Name;
    MoveServiceProvider(import.Import1.Item.ServiceProvider,
      local.PrevServiceProvider);
    MoveCase1(import.Import1.Item.Case1, local.BackupCase);
    local.BackupOffice.Name = import.Import1.Item.Office.Name;
    MoveServiceProvider(import.Import1.Item.ServiceProvider,
      local.BackupServiceProvider);

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      if (local.PageNumber.TotalInteger < import.Standard.PageNumber)
      {
        if (Equal(import.Import1.Item.Case1.Number, local.PrevCase.Number))
        {
          ++local.RowCounter.Count;
        }
        else
        {
          local.RowCounter.Count += 2;

          // 11/14/2002 M.L
          MoveCase1(local.PrevCase, local.BackupCase);
          local.BackupOffice.Name = local.PrevOffice.Name;
          MoveServiceProvider(local.PrevServiceProvider,
            local.BackupServiceProvider);

          // 11/14/2002 M.L
          MoveCase1(import.Import1.Item.Case1, local.PrevCase);
          local.PrevOffice.Name = import.Import1.Item.Office.Name;
          MoveServiceProvider(import.Import1.Item.ServiceProvider,
            local.PrevServiceProvider);
        }

        if (local.RowCounter.Count > Export.DisplayGroup.Capacity)
        {
          ++local.PageNumber.TotalInteger;
          local.RowCounter.Count -= Export.DisplayGroup.Capacity;

          if (local.PageNumber.TotalInteger == import.Standard.PageNumber)
          {
          }
          else
          {
            MoveCase1(local.PrevCase, local.BackupCase);
            local.BackupOffice.Name = local.PrevOffice.Name;
            MoveServiceProvider(local.PrevServiceProvider,
              local.BackupServiceProvider);
          }
        }
      }

      if (local.PageNumber.TotalInteger == import.Standard.PageNumber)
      {
        if (local.RowCounter.Count == 2)
        {
          ++export.Display.Index;
          export.Display.CheckSize();

          if (export.Display.Index >= Export.DisplayGroup.Capacity)
          {
            if (import.Standard.PageNumber == 1)
            {
              export.Standard.ScrollingMessage = "MORE +";
            }
            else
            {
              export.Standard.ScrollingMessage = "MORE - +";
            }

            return;
          }

          // 11/14/2002 M.L
          export.Display.Update.DisplayCase.Status =
            local.BackupCase.Status ?? "";

          // 11/14/2002 M.L
          export.Display.Update.DisplayCaseRole.EndDate = local.Zero.Date;
          export.Display.Update.DisplayCaseRole.StartDate = local.Zero.Date;
          export.Display.Update.DisplayCsePersonsWorkSet.Number = "";
          export.Display.Update.DisplayCase.Number = local.BackupCase.Number;
          export.Display.Update.DisplayOffice.Name = local.BackupOffice.Name;
          export.Display.Update.DisplayServiceProvider.LastName =
            local.BackupOffice.Name;
          local.WorkArea.Text3 = Substring(local.BackupOffice.Name, 1, 3);

          for(local.StringCounter.Count = 1; local.StringCounter.Count <= 10; ++
            local.StringCounter.Count)
          {
            if (CharAt(local.BackupCase.Number, local.StringCounter.Count) != '0'
              )
            {
              break;
            }
          }

          local.LengthOfCaseNumber.Count = 11 - local.StringCounter.Count;
          local.WorkArea.Text10 =
            Substring(local.BackupCase.Number, local.StringCounter.Count,
            local.LengthOfCaseNumber.Count);
          local.LengthOfName.Count =
            Length(TrimEnd(local.BackupServiceProvider.LastName));
          local.StartingPosition.Count = 35 - (
            local.LengthOfCaseNumber.Count + local.LengthOfName.Count + 7) + 1;
          local.WorkArea.Text35 = local.WorkArea.Text3 + " " + TrimEnd
            (local.BackupServiceProvider.LastName) + " " + TrimEnd
            (local.WorkArea.Text10);
          export.Display.Update.DisplayCsePersonsWorkSet.FormattedName =
            Substring(export.Display.Item.DisplayCsePersonsWorkSet.
              FormattedName, CsePersonsWorkSet.FormattedName_MaxLength, 1,
            local.StartingPosition.Count - 1) + TrimEnd(local.WorkArea.Text35);
          local.RowCounter.Count = 0;
        }

        if (!Equal(import.Import1.Item.Case1.Number, local.PrevCase.Number))
        {
          ++export.Display.Index;
          export.Display.CheckSize();

          if (export.Display.Index >= Export.DisplayGroup.Capacity)
          {
            if (import.Standard.PageNumber == 1)
            {
              export.Standard.ScrollingMessage = "MORE +";
            }
            else
            {
              export.Standard.ScrollingMessage = "MORE - +";
            }

            return;
          }

          MoveCase1(local.PrevCase, local.BackupCase);
          local.BackupOffice.Name = local.PrevOffice.Name;
          MoveServiceProvider(local.PrevServiceProvider,
            local.BackupServiceProvider);
          MoveCase1(import.Import1.Item.Case1, local.PrevCase);
          export.Display.Update.DisplayCaseRole.EndDate = local.Zero.Date;
          export.Display.Update.DisplayCaseRole.StartDate = local.Zero.Date;
          export.Display.Update.DisplayCsePersonsWorkSet.Number = "";
          export.Display.Update.DisplayCase.Number = local.BackupCase.Number;
          export.Display.Update.DisplayCase.Status =
            local.BackupCase.Status ?? "";
          export.Display.Update.DisplayOffice.Name = local.BackupOffice.Name;
          export.Display.Update.DisplayServiceProvider.LastName =
            local.BackupOffice.Name;
          local.WorkArea.Text3 = Substring(local.BackupOffice.Name, 1, 3);

          for(local.StringCounter.Count = 1; local.StringCounter.Count <= 10; ++
            local.StringCounter.Count)
          {
            if (CharAt(local.BackupCase.Number, local.StringCounter.Count) != '0'
              )
            {
              break;
            }
          }

          local.LengthOfCaseNumber.Count = 11 - local.StringCounter.Count;
          local.WorkArea.Text10 =
            Substring(local.BackupCase.Number, local.StringCounter.Count,
            local.LengthOfCaseNumber.Count);
          local.LengthOfName.Count =
            Length(TrimEnd(local.BackupServiceProvider.LastName));
          local.StartingPosition.Count = 35 - (
            local.LengthOfCaseNumber.Count + local.LengthOfName.Count + 7) + 1;
          local.WorkArea.Text35 = local.WorkArea.Text3 + " " + TrimEnd
            (local.BackupServiceProvider.LastName) + " " + TrimEnd
            (local.WorkArea.Text10);
          export.Display.Update.DisplayCsePersonsWorkSet.FormattedName =
            Substring(export.Display.Item.DisplayCsePersonsWorkSet.
              FormattedName, CsePersonsWorkSet.FormattedName_MaxLength, 1,
            local.StartingPosition.Count - 1) + TrimEnd(local.WorkArea.Text35);

          if (export.Display.Index + 1 >= Export.DisplayGroup.Capacity)
          {
            if (import.Standard.PageNumber == 1)
            {
              export.Standard.ScrollingMessage = "MORE +";
            }
            else
            {
              export.Standard.ScrollingMessage = "MORE - +";
            }

            return;
          }
        }

        ++export.Display.Index;
        export.Display.CheckSize();

        if (export.Display.Index >= Export.DisplayGroup.Capacity)
        {
          if (import.Standard.PageNumber == 1)
          {
            export.Standard.ScrollingMessage = "MORE +";
          }
          else
          {
            export.Standard.ScrollingMessage = "MORE - +";
          }

          return;
        }

        MoveCase1(import.Import1.Item.Case1, export.Display.Update.DisplayCase);
        export.Display.Update.DisplayCaseRole.Assign(
          import.Import1.Item.CaseRole);

        // ** CQ# 2109 1/25/2008 Comment the below Case status being set to 
        // spaces **
        if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
          local.MaxDate.Date))
        {
          export.Display.Update.DisplayCaseRole.EndDate = local.Zero.Date;
        }

        MoveCsePersonsWorkSet(import.Import1.Item.CsePersonsWorkSet,
          export.Display.Update.DisplayCsePersonsWorkSet);
        export.Display.Update.DisplayOffice.Name =
          import.Import1.Item.Office.Name;
        MoveServiceProvider(import.Import1.Item.ServiceProvider,
          export.Display.Update.DisplayServiceProvider);
        export.Display.Update.DisplayRowOperation.OneChar =
          import.Import1.Item.RowOperation.OneChar;
        export.Display.Update.DisplayRowNumber.Count =
          import.Import1.Item.RowNumber.Count;
      }

      if (local.PageNumber.TotalInteger > import.Standard.PageNumber)
      {
        if (import.Standard.PageNumber == 1)
        {
          export.Standard.ScrollingMessage = "MORE +";
        }
        else
        {
          export.Standard.ScrollingMessage = "MORE - +";
        }

        return;
      }

      if (import.Standard.PageNumber == 1)
      {
        export.Standard.ScrollingMessage = "";
      }
      else
      {
        export.Standard.ScrollingMessage = "MORE -";
      }
    }

    import.Import1.CheckIndex();

    // 08/28/2002 M.Lachowicz Start
    if (import.Import1.Count <= import.Import1.Index + 1 && export
      .Display.Count >= Export.DisplayGroup.Capacity)
    {
      if (import.Standard.PageNumber == 1)
      {
        export.Standard.ScrollingMessage = "MORE +";
      }
      else
      {
        export.Standard.ScrollingMessage = "MORE - +";
      }

      return;
    }

    // 08/28/2002 M.Lachowicz End
    local.ImportLast.Count = import.Import1.Count;
    local.ImportSubscript.Count = import.Import1.Index + 1;
    local.ExportSubscript.Count = export.Display.Index + 1;

    if (import.Import1.Count <= import.Import1.Index + 1 && export
      .Display.Count < Export.DisplayGroup.Capacity)
    {
      import.Import1.Index = import.Import1.Count - 1;
      import.Import1.CheckSize();

      ++export.Display.Index;
      export.Display.CheckSize();

      MoveCase1(import.Import1.Item.Case1, local.BackupCase);
      local.BackupOffice.Name = import.Import1.Item.Office.Name;
      MoveServiceProvider(import.Import1.Item.ServiceProvider,
        local.BackupServiceProvider);
      export.Display.Update.DisplayCaseRole.EndDate = local.Zero.Date;
      export.Display.Update.DisplayCaseRole.StartDate = local.Zero.Date;
      export.Display.Update.DisplayCsePersonsWorkSet.Number = "";
      export.Display.Update.DisplayCase.Number = local.BackupCase.Number;
      export.Display.Update.DisplayCase.Status = local.BackupCase.Status ?? "";
      export.Display.Update.DisplayOffice.Name = local.BackupOffice.Name;
      export.Display.Update.DisplayServiceProvider.LastName =
        local.BackupOffice.Name;
      local.WorkArea.Text3 = Substring(local.BackupOffice.Name, 1, 3);

      for(local.StringCounter.Count = 1; local.StringCounter.Count <= 10; ++
        local.StringCounter.Count)
      {
        if (CharAt(local.BackupCase.Number, local.StringCounter.Count) != '0')
        {
          break;
        }
      }

      local.LengthOfCaseNumber.Count = 11 - local.StringCounter.Count;
      local.WorkArea.Text10 =
        Substring(local.BackupCase.Number, local.StringCounter.Count,
        local.LengthOfCaseNumber.Count);
      local.LengthOfName.Count =
        Length(TrimEnd(local.BackupServiceProvider.LastName));
      local.StartingPosition.Count = 35 - (local.LengthOfCaseNumber.Count + local
        .LengthOfName.Count + 7) + 1;
      local.WorkArea.Text35 = local.WorkArea.Text3 + " " + TrimEnd
        (local.BackupServiceProvider.LastName) + " " + TrimEnd
        (local.WorkArea.Text10);
      export.Display.Update.DisplayCsePersonsWorkSet.FormattedName =
        Substring(export.Display.Item.DisplayCsePersonsWorkSet.FormattedName,
        CsePersonsWorkSet.FormattedName_MaxLength, 1,
        local.StartingPosition.Count - 1) + TrimEnd(local.WorkArea.Text35);
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.UserId = source.UserId;
    target.LastName = source.LastName;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.DateWorkArea.Date;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    private Standard standard;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DisplayGroup group.</summary>
    [Serializable]
    public class DisplayGroup
    {
      /// <summary>
      /// A value of DisplayRowIndicator.
      /// </summary>
      [JsonPropertyName("displayRowIndicator")]
      public Common DisplayRowIndicator
      {
        get => displayRowIndicator ??= new();
        set => displayRowIndicator = value;
      }

      /// <summary>
      /// A value of DisplayCaseRole.
      /// </summary>
      [JsonPropertyName("displayCaseRole")]
      public CaseRole DisplayCaseRole
      {
        get => displayCaseRole ??= new();
        set => displayCaseRole = value;
      }

      /// <summary>
      /// A value of DisplayCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("displayCsePersonsWorkSet")]
      public CsePersonsWorkSet DisplayCsePersonsWorkSet
      {
        get => displayCsePersonsWorkSet ??= new();
        set => displayCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DisplayCase.
      /// </summary>
      [JsonPropertyName("displayCase")]
      public Case1 DisplayCase
      {
        get => displayCase ??= new();
        set => displayCase = value;
      }

      /// <summary>
      /// A value of DisplayRowNumber.
      /// </summary>
      [JsonPropertyName("displayRowNumber")]
      public Common DisplayRowNumber
      {
        get => displayRowNumber ??= new();
        set => displayRowNumber = value;
      }

      /// <summary>
      /// A value of DisplayRowOperation.
      /// </summary>
      [JsonPropertyName("displayRowOperation")]
      public Standard DisplayRowOperation
      {
        get => displayRowOperation ??= new();
        set => displayRowOperation = value;
      }

      /// <summary>
      /// A value of DisplayOffice.
      /// </summary>
      [JsonPropertyName("displayOffice")]
      public Office DisplayOffice
      {
        get => displayOffice ??= new();
        set => displayOffice = value;
      }

      /// <summary>
      /// A value of DisplayServiceProvider.
      /// </summary>
      [JsonPropertyName("displayServiceProvider")]
      public ServiceProvider DisplayServiceProvider
      {
        get => displayServiceProvider ??= new();
        set => displayServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common displayRowIndicator;
      private CaseRole displayCaseRole;
      private CsePersonsWorkSet displayCsePersonsWorkSet;
      private Case1 displayCase;
      private Common displayRowNumber;
      private Standard displayRowOperation;
      private Office displayOffice;
      private ServiceProvider displayServiceProvider;
    }

    /// <summary>
    /// Gets a value of Display.
    /// </summary>
    [JsonIgnore]
    public Array<DisplayGroup> Display => display ??= new(
      DisplayGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Display for json serialization.
    /// </summary>
    [JsonPropertyName("display")]
    [Computed]
    public IList<DisplayGroup> Display_Json
    {
      get => display;
      set => Display.Assign(value);
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

    private Array<DisplayGroup> display;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ImportLast.
    /// </summary>
    [JsonPropertyName("importLast")]
    public Common ImportLast
    {
      get => importLast ??= new();
      set => importLast = value;
    }

    /// <summary>
    /// A value of ExportSubscript.
    /// </summary>
    [JsonPropertyName("exportSubscript")]
    public Common ExportSubscript
    {
      get => exportSubscript ??= new();
      set => exportSubscript = value;
    }

    /// <summary>
    /// A value of ImportSubscript.
    /// </summary>
    [JsonPropertyName("importSubscript")]
    public Common ImportSubscript
    {
      get => importSubscript ??= new();
      set => importSubscript = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of StartingPosition.
    /// </summary>
    [JsonPropertyName("startingPosition")]
    public Common StartingPosition
    {
      get => startingPosition ??= new();
      set => startingPosition = value;
    }

    /// <summary>
    /// A value of LengthOfName.
    /// </summary>
    [JsonPropertyName("lengthOfName")]
    public Common LengthOfName
    {
      get => lengthOfName ??= new();
      set => lengthOfName = value;
    }

    /// <summary>
    /// A value of LengthOfCaseNumber.
    /// </summary>
    [JsonPropertyName("lengthOfCaseNumber")]
    public Common LengthOfCaseNumber
    {
      get => lengthOfCaseNumber ??= new();
      set => lengthOfCaseNumber = value;
    }

    /// <summary>
    /// A value of StringCounter.
    /// </summary>
    [JsonPropertyName("stringCounter")]
    public Common StringCounter
    {
      get => stringCounter ??= new();
      set => stringCounter = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of BackupServiceProvider.
    /// </summary>
    [JsonPropertyName("backupServiceProvider")]
    public ServiceProvider BackupServiceProvider
    {
      get => backupServiceProvider ??= new();
      set => backupServiceProvider = value;
    }

    /// <summary>
    /// A value of BackupOffice.
    /// </summary>
    [JsonPropertyName("backupOffice")]
    public Office BackupOffice
    {
      get => backupOffice ??= new();
      set => backupOffice = value;
    }

    /// <summary>
    /// A value of BackupCase.
    /// </summary>
    [JsonPropertyName("backupCase")]
    public Case1 BackupCase
    {
      get => backupCase ??= new();
      set => backupCase = value;
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
    /// A value of PrevOffice.
    /// </summary>
    [JsonPropertyName("prevOffice")]
    public Office PrevOffice
    {
      get => prevOffice ??= new();
      set => prevOffice = value;
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
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Common PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    /// <summary>
    /// A value of RowCounter.
    /// </summary>
    [JsonPropertyName("rowCounter")]
    public Common RowCounter
    {
      get => rowCounter ??= new();
      set => rowCounter = value;
    }

    private Common importLast;
    private Common exportSubscript;
    private Common importSubscript;
    private DateWorkArea maxDate;
    private Common startingPosition;
    private Common lengthOfName;
    private Common lengthOfCaseNumber;
    private Common stringCounter;
    private WorkArea workArea;
    private DateWorkArea zero;
    private ServiceProvider backupServiceProvider;
    private Office backupOffice;
    private Case1 backupCase;
    private ServiceProvider prevServiceProvider;
    private Office prevOffice;
    private Case1 prevCase;
    private Common pageNumber;
    private Common rowCounter;
  }
#endregion
}
