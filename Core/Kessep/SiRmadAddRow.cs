// Program: SI_RMAD_ADD_ROW, ID: 373475616, model: 746.
// Short name: SWE00655
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_RMAD_ADD_ROW.
/// </summary>
[Serializable]
public partial class SiRmadAddRow: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RMAD_ADD_ROW program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRmadAddRow(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRmadAddRow.
  /// </summary>
  public SiRmadAddRow(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.RowWasAdded.Flag = "N";
    export.Export1.Index = -1;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      ++export.Export1.Index;
      export.Export1.CheckSize();

      if (AsChar(local.RowWasAdded.Flag) == 'N')
      {
        if (Equal(import.NewCase.Number, import.Import1.Item.Case1.Number))
        {
          if (Lt(import.NewCsePersonsWorkSet.Number,
            import.Import1.Item.CsePersonsWorkSet.Number) || Equal
            (import.NewCsePersonsWorkSet.Number,
            import.Import1.Item.CsePersonsWorkSet.Number) && Lt
            (import.NewCaseRole.StartDate,
            import.Import1.Item.CaseRole.StartDate))
          {
            local.RowWasAdded.Flag = "Y";
            MoveCase1(import.NewCase, export.Export1.Update.Case1);
            export.Export1.Update.CaseRole.Assign(import.NewCaseRole);
            MoveCsePersonsWorkSet(import.NewCsePersonsWorkSet,
              export.Export1.Update.CsePersonsWorkSet);
            export.Export1.Update.Office.Name = import.NewOffice.Name;
            MoveServiceProvider(import.NewServiceProvider,
              export.Export1.Update.ServiceProvider);
            export.Export1.Update.RowOperation.OneChar = "A";
            export.Export1.Update.RowNumber.Count = export.Export1.Index + 1;

            ++export.Export1.Index;
            export.Export1.CheckSize();
          }
        }
        else if (Lt(import.NewCase.Number, import.Import1.Item.Case1.Number))
        {
          local.RowWasAdded.Flag = "Y";
          MoveCase1(import.NewCase, export.Export1.Update.Case1);
          export.Export1.Update.CaseRole.Assign(import.NewCaseRole);
          MoveCsePersonsWorkSet(import.NewCsePersonsWorkSet,
            export.Export1.Update.CsePersonsWorkSet);
          export.Export1.Update.Office.Name = import.NewOffice.Name;
          MoveServiceProvider(import.NewServiceProvider,
            export.Export1.Update.ServiceProvider);
          export.Export1.Update.RowOperation.OneChar = "A";
          export.Export1.Update.RowNumber.Count = export.Export1.Index + 1;

          ++export.Export1.Index;
          export.Export1.CheckSize();
        }
      }

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
    }

    import.Import1.CheckIndex();

    if (AsChar(local.RowWasAdded.Flag) == 'N')
    {
      ++export.Export1.Index;
      export.Export1.CheckSize();

      MoveCase1(import.NewCase, export.Export1.Update.Case1);
      export.Export1.Update.CaseRole.Assign(import.NewCaseRole);
      MoveCsePersonsWorkSet(import.NewCsePersonsWorkSet,
        export.Export1.Update.CsePersonsWorkSet);
      export.Export1.Update.Office.Name = import.NewOffice.Name;
      MoveServiceProvider(import.NewServiceProvider,
        export.Export1.Update.ServiceProvider);
      export.Export1.Update.RowOperation.OneChar = "A";
      export.Export1.Update.RowNumber.Count = export.Export1.Index + 1;
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
    /// A value of NewOffice.
    /// </summary>
    [JsonPropertyName("newOffice")]
    public Office NewOffice
    {
      get => newOffice ??= new();
      set => newOffice = value;
    }

    /// <summary>
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
    }

    /// <summary>
    /// A value of NewCaseRole.
    /// </summary>
    [JsonPropertyName("newCaseRole")]
    public CaseRole NewCaseRole
    {
      get => newCaseRole ??= new();
      set => newCaseRole = value;
    }

    /// <summary>
    /// A value of NewCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("newCsePersonsWorkSet")]
    public CsePersonsWorkSet NewCsePersonsWorkSet
    {
      get => newCsePersonsWorkSet ??= new();
      set => newCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NewCase.
    /// </summary>
    [JsonPropertyName("newCase")]
    public Case1 NewCase
    {
      get => newCase ??= new();
      set => newCase = value;
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

    private Office newOffice;
    private ServiceProvider newServiceProvider;
    private CaseRole newCaseRole;
    private CsePersonsWorkSet newCsePersonsWorkSet;
    private Case1 newCase;
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
    /// A value of RowWasAdded.
    /// </summary>
    [JsonPropertyName("rowWasAdded")]
    public Common RowWasAdded
    {
      get => rowWasAdded ??= new();
      set => rowWasAdded = value;
    }

    /// <summary>
    /// A value of LocaGroup.
    /// </summary>
    [JsonPropertyName("locaGroup")]
    public TextWorkArea LocaGroup
    {
      get => locaGroup ??= new();
      set => locaGroup = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public TextWorkArea New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Common rowWasAdded;
    private TextWorkArea locaGroup;
    private TextWorkArea new1;
  }
#endregion
}
