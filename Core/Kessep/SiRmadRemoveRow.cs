// Program: SI_RMAD_REMOVE_ROW, ID: 373475612, model: 746.
// Short name: SWE00730
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_RMAD_REMOVE_ROW.
/// </summary>
[Serializable]
public partial class SiRmadRemoveRow: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RMAD_REMOVE_ROW program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRmadRemoveRow(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRmadRemoveRow.
  /// </summary>
  public SiRmadRemoveRow(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    for(import.Display.Index = 0; import.Display.Index < import.Display.Count; ++
      import.Display.Index)
    {
      if (!import.Display.CheckSize())
      {
        break;
      }

      if (Lt(local.Zero.Date, import.Display.Item.DisplayCaseRole.StartDate))
      {
        break;
      }
    }

    import.Display.CheckIndex();
    local.FirstDisplayRowNumber.Count =
      import.Display.Item.DisplayRowNumber.Count;

    for(import.Display.Index = import.Display.Count - 1; import
      .Display.Index >= 0; --import.Display.Index)
    {
      if (!import.Display.CheckSize())
      {
        break;
      }

      if (Lt(local.Zero.Date, import.Display.Item.DisplayCaseRole.StartDate))
      {
        break;
      }
    }

    import.Display.CheckIndex();
    local.LastDisplayRowNumber.Count =
      import.Display.Item.DisplayRowNumber.Count;
    export.Export1.Index = -1;
    import.Display.Index = -1;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      if (import.Import1.Item.RowNumber.Count < local
        .FirstDisplayRowNumber.Count || import.Import1.Item.RowNumber.Count > local
        .LastDisplayRowNumber.Count)
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        MoveCase1(import.Import1.Item.Case1, export.Export1.Update.Case1);
        export.Export1.Update.CaseRole.Assign(import.Import1.Item.CaseRole);
        MoveCsePersonsWorkSet(import.Import1.Item.CsePersonsWorkSet,
          export.Export1.Update.CsePersonsWorkSet);
        export.Export1.Update.Office.Name = import.Import1.Item.Office.Name;
        MoveServiceProvider(import.Import1.Item.ServiceProvider,
          export.Export1.Update.ServiceProvider);
        export.Export1.Update.RowOperation.OneChar =
          import.Import1.Item.RowOperation.OneChar;
        export.Export1.Update.RowNumber.Count = export.Export1.Index + 1;

        continue;
      }

      do
      {
        ++import.Display.Index;
        import.Display.CheckSize();

        if (import.Display.Item.DisplayRowNumber.Count == import
          .Import1.Item.RowNumber.Count)
        {
          if (AsChar(import.Display.Item.DisplayRowIndicator.SelectChar) == 'S')
          {
            goto Next;
          }
          else
          {
            ++export.Export1.Index;
            export.Export1.CheckSize();

            MoveCase1(import.Import1.Item.Case1, export.Export1.Update.Case1);
            export.Export1.Update.CaseRole.Assign(import.Import1.Item.CaseRole);
            MoveCsePersonsWorkSet(import.Import1.Item.CsePersonsWorkSet,
              export.Export1.Update.CsePersonsWorkSet);
            export.Export1.Update.Office.Name = import.Import1.Item.Office.Name;
            MoveServiceProvider(import.Import1.Item.ServiceProvider,
              export.Export1.Update.ServiceProvider);
            export.Export1.Update.RowOperation.OneChar =
              import.Import1.Item.RowOperation.OneChar;
            export.Export1.Update.RowNumber.Count = export.Export1.Index + 1;

            goto Next;
          }
        }
      }
      while(import.Display.Index + 1 < import.Display.Count);

Next:
      ;
    }

    import.Import1.CheckIndex();
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

    private Array<DisplayGroup> display;
    private Array<ImportGroup> import1;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of LastDisplayRowNumber.
    /// </summary>
    [JsonPropertyName("lastDisplayRowNumber")]
    public Common LastDisplayRowNumber
    {
      get => lastDisplayRowNumber ??= new();
      set => lastDisplayRowNumber = value;
    }

    /// <summary>
    /// A value of FirstDisplayRowNumber.
    /// </summary>
    [JsonPropertyName("firstDisplayRowNumber")]
    public Common FirstDisplayRowNumber
    {
      get => firstDisplayRowNumber ??= new();
      set => firstDisplayRowNumber = value;
    }

    private DateWorkArea zero;
    private Common lastDisplayRowNumber;
    private Common firstDisplayRowNumber;
  }
#endregion
}
