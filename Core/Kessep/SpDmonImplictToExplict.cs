// Program: SP_DMON_IMPLICT_TO_EXPLICT, ID: 372981555, model: 746.
// Short name: SWE00287
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
/// A program: SP_DMON_IMPLICT_TO_EXPLICT.
/// </para>
/// <para>
/// This cab will remap the data from the implicit group view to an explicit 
/// group view so that the error can be seen immediately without having to
/// scroll through a great number of pages.
/// </para>
/// </summary>
[Serializable]
public partial class SpDmonImplictToExplict: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DMON_IMPLICT_TO_EXPLICT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDmonImplictToExplict(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDmonImplictToExplict.
  /// </summary>
  public SpDmonImplictToExplict(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Explicit1.Index = -1;

    for(import.Explicit1.Index = 0; import.Explicit1.Index < import
      .Explicit1.Count; ++import.Explicit1.Index)
    {
      if (!import.Explicit1.CheckSize())
      {
        break;
      }

      if (!IsEmpty(import.Explicit1.Item.Giommon.SelectChar) && IsEmpty
        (local.PromptedCharacter.Flag))
      {
        local.SelectedCharacter.Flag = "Y";

        ++export.Explicit1.Index;
        export.Explicit1.CheckSize();

        export.Explicit1.Update.GxsePersonsWorkSet.FormattedName =
          import.Explicit1.Item.GisePersonsWorkSet.FormattedName;
        MoveDocument(import.Explicit1.Item.Giocument,
          export.Explicit1.Update.Gxocument);
        export.Explicit1.Update.Gxnfrastructure.Assign(
          import.Explicit1.Item.Ginfrastructure);
        export.Explicit1.Update.GxegalAction.CourtCaseNumber =
          import.Explicit1.Item.GiegalAction.CourtCaseNumber;
        export.Explicit1.Update.GxonitoredDocument.Assign(
          import.Explicit1.Item.GionitoredDocument);
        export.Explicit1.Update.GxodeValue.SelectChar =
          import.Explicit1.Item.GiodeValue.SelectChar;
        export.Explicit1.Update.Gxrev.Assign(import.Explicit1.Item.Girev);
        export.Explicit1.Update.Gxommon.SelectChar =
          import.Explicit1.Item.Giommon.SelectChar;
      }
      else if (!IsEmpty(import.Explicit1.Item.GiodeValue.SelectChar) && IsEmpty
        (local.SelectedCharacter.Flag))
      {
        local.PromptedCharacter.Flag = "Y";

        ++export.Explicit1.Index;
        export.Explicit1.CheckSize();

        export.Explicit1.Update.GxsePersonsWorkSet.FormattedName =
          import.Explicit1.Item.GisePersonsWorkSet.FormattedName;
        MoveDocument(import.Explicit1.Item.Giocument,
          export.Explicit1.Update.Gxocument);
        export.Explicit1.Update.Gxnfrastructure.Assign(
          import.Explicit1.Item.Ginfrastructure);
        export.Explicit1.Update.GxegalAction.CourtCaseNumber =
          import.Explicit1.Item.GiegalAction.CourtCaseNumber;
        export.Explicit1.Update.GxonitoredDocument.Assign(
          import.Explicit1.Item.GionitoredDocument);
        export.Explicit1.Update.GxodeValue.SelectChar =
          import.Explicit1.Item.GiodeValue.SelectChar;
        export.Explicit1.Update.Gxrev.Assign(import.Explicit1.Item.Girev);
        export.Explicit1.Update.Gxommon.SelectChar =
          import.Explicit1.Item.Giommon.SelectChar;
      }
      else if (!IsEmpty(local.SelectedCharacter.Flag))
      {
        ++export.Explicit1.Index;
        export.Explicit1.CheckSize();

        export.Explicit1.Update.GxsePersonsWorkSet.FormattedName =
          import.Explicit1.Item.GisePersonsWorkSet.FormattedName;
        MoveDocument(import.Explicit1.Item.Giocument,
          export.Explicit1.Update.Gxocument);
        export.Explicit1.Update.Gxnfrastructure.Assign(
          import.Explicit1.Item.Ginfrastructure);
        export.Explicit1.Update.GxegalAction.CourtCaseNumber =
          import.Explicit1.Item.GiegalAction.CourtCaseNumber;
        export.Explicit1.Update.GxonitoredDocument.Assign(
          import.Explicit1.Item.GionitoredDocument);
        export.Explicit1.Update.GxodeValue.SelectChar =
          import.Explicit1.Item.GiodeValue.SelectChar;
        export.Explicit1.Update.Gxrev.Assign(import.Explicit1.Item.Girev);
        export.Explicit1.Update.Gxommon.SelectChar =
          import.Explicit1.Item.Giommon.SelectChar;
      }
      else if (!IsEmpty(local.PromptedCharacter.Flag))
      {
        ++export.Explicit1.Index;
        export.Explicit1.CheckSize();

        export.Explicit1.Update.GxsePersonsWorkSet.FormattedName =
          import.Explicit1.Item.GisePersonsWorkSet.FormattedName;
        MoveDocument(import.Explicit1.Item.Giocument,
          export.Explicit1.Update.Gxocument);
        export.Explicit1.Update.Gxnfrastructure.Assign(
          import.Explicit1.Item.Ginfrastructure);
        export.Explicit1.Update.GxegalAction.CourtCaseNumber =
          import.Explicit1.Item.GiegalAction.CourtCaseNumber;
        export.Explicit1.Update.GxonitoredDocument.Assign(
          import.Explicit1.Item.GionitoredDocument);
        export.Explicit1.Update.GxodeValue.SelectChar =
          import.Explicit1.Item.GiodeValue.SelectChar;
        export.Explicit1.Update.Gxrev.Assign(import.Explicit1.Item.Girev);
        export.Explicit1.Update.Gxommon.SelectChar =
          import.Explicit1.Item.Giommon.SelectChar;
      }
    }

    import.Explicit1.CheckIndex();

    if (export.Explicit1.Index == -1)
    {
      for(import.Explicit1.Index = 0; import.Explicit1.Index < import
        .Explicit1.Count; ++import.Explicit1.Index)
      {
        if (!import.Explicit1.CheckSize())
        {
          break;
        }

        ++export.Explicit1.Index;
        export.Explicit1.CheckSize();

        export.Explicit1.Update.GxsePersonsWorkSet.FormattedName =
          import.Explicit1.Item.GisePersonsWorkSet.FormattedName;
        MoveDocument(import.Explicit1.Item.Giocument,
          export.Explicit1.Update.Gxocument);
        export.Explicit1.Update.Gxnfrastructure.Assign(
          import.Explicit1.Item.Ginfrastructure);
        export.Explicit1.Update.GxegalAction.CourtCaseNumber =
          import.Explicit1.Item.GiegalAction.CourtCaseNumber;
        export.Explicit1.Update.GxonitoredDocument.Assign(
          import.Explicit1.Item.GionitoredDocument);
        export.Explicit1.Update.GxodeValue.SelectChar =
          import.Explicit1.Item.GiodeValue.SelectChar;
        export.Explicit1.Update.Gxrev.Assign(import.Explicit1.Item.Girev);
        export.Explicit1.Update.Gxommon.SelectChar =
          import.Explicit1.Item.Giommon.SelectChar;
      }

      import.Explicit1.CheckIndex();
    }
    else
    {
      export.ErrorForScrollDisplay.Flag = "Y";
    }
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.Description = source.Description;
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
    /// <summary>A ExplicitGroup group.</summary>
    [Serializable]
    public class ExplicitGroup
    {
      /// <summary>
      /// A value of Girev.
      /// </summary>
      [JsonPropertyName("girev")]
      public MonitoredDocument Girev
      {
        get => girev ??= new();
        set => girev = value;
      }

      /// <summary>
      /// A value of GiegalAction.
      /// </summary>
      [JsonPropertyName("giegalAction")]
      public LegalAction GiegalAction
      {
        get => giegalAction ??= new();
        set => giegalAction = value;
      }

      /// <summary>
      /// A value of GiodeValue.
      /// </summary>
      [JsonPropertyName("giodeValue")]
      public Common GiodeValue
      {
        get => giodeValue ??= new();
        set => giodeValue = value;
      }

      /// <summary>
      /// A value of GisePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gisePersonsWorkSet")]
      public CsePersonsWorkSet GisePersonsWorkSet
      {
        get => gisePersonsWorkSet ??= new();
        set => gisePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GionitoredDocument.
      /// </summary>
      [JsonPropertyName("gionitoredDocument")]
      public MonitoredDocument GionitoredDocument
      {
        get => gionitoredDocument ??= new();
        set => gionitoredDocument = value;
      }

      /// <summary>
      /// A value of Giommon.
      /// </summary>
      [JsonPropertyName("giommon")]
      public Common Giommon
      {
        get => giommon ??= new();
        set => giommon = value;
      }

      /// <summary>
      /// A value of Ginfrastructure.
      /// </summary>
      [JsonPropertyName("ginfrastructure")]
      public Infrastructure Ginfrastructure
      {
        get => ginfrastructure ??= new();
        set => ginfrastructure = value;
      }

      /// <summary>
      /// A value of Giocument.
      /// </summary>
      [JsonPropertyName("giocument")]
      public Document Giocument
      {
        get => giocument ??= new();
        set => giocument = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private MonitoredDocument girev;
      private LegalAction giegalAction;
      private Common giodeValue;
      private CsePersonsWorkSet gisePersonsWorkSet;
      private MonitoredDocument gionitoredDocument;
      private Common giommon;
      private Infrastructure ginfrastructure;
      private Document giocument;
    }

    /// <summary>
    /// Gets a value of Explicit1.
    /// </summary>
    [JsonIgnore]
    public Array<ExplicitGroup> Explicit1 => explicit1 ??= new(
      ExplicitGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Explicit1 for json serialization.
    /// </summary>
    [JsonPropertyName("explicit1")]
    [Computed]
    public IList<ExplicitGroup> Explicit1_Json
    {
      get => explicit1;
      set => Explicit1.Assign(value);
    }

    private Array<ExplicitGroup> explicit1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExplicitGroup group.</summary>
    [Serializable]
    public class ExplicitGroup
    {
      /// <summary>
      /// A value of Gxrev.
      /// </summary>
      [JsonPropertyName("gxrev")]
      public MonitoredDocument Gxrev
      {
        get => gxrev ??= new();
        set => gxrev = value;
      }

      /// <summary>
      /// A value of GxegalAction.
      /// </summary>
      [JsonPropertyName("gxegalAction")]
      public LegalAction GxegalAction
      {
        get => gxegalAction ??= new();
        set => gxegalAction = value;
      }

      /// <summary>
      /// A value of GxodeValue.
      /// </summary>
      [JsonPropertyName("gxodeValue")]
      public Common GxodeValue
      {
        get => gxodeValue ??= new();
        set => gxodeValue = value;
      }

      /// <summary>
      /// A value of GxsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gxsePersonsWorkSet")]
      public CsePersonsWorkSet GxsePersonsWorkSet
      {
        get => gxsePersonsWorkSet ??= new();
        set => gxsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GxonitoredDocument.
      /// </summary>
      [JsonPropertyName("gxonitoredDocument")]
      public MonitoredDocument GxonitoredDocument
      {
        get => gxonitoredDocument ??= new();
        set => gxonitoredDocument = value;
      }

      /// <summary>
      /// A value of Gxommon.
      /// </summary>
      [JsonPropertyName("gxommon")]
      public Common Gxommon
      {
        get => gxommon ??= new();
        set => gxommon = value;
      }

      /// <summary>
      /// A value of Gxnfrastructure.
      /// </summary>
      [JsonPropertyName("gxnfrastructure")]
      public Infrastructure Gxnfrastructure
      {
        get => gxnfrastructure ??= new();
        set => gxnfrastructure = value;
      }

      /// <summary>
      /// A value of Gxocument.
      /// </summary>
      [JsonPropertyName("gxocument")]
      public Document Gxocument
      {
        get => gxocument ??= new();
        set => gxocument = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private MonitoredDocument gxrev;
      private LegalAction gxegalAction;
      private Common gxodeValue;
      private CsePersonsWorkSet gxsePersonsWorkSet;
      private MonitoredDocument gxonitoredDocument;
      private Common gxommon;
      private Infrastructure gxnfrastructure;
      private Document gxocument;
    }

    /// <summary>
    /// A value of ErrorForScrollDisplay.
    /// </summary>
    [JsonPropertyName("errorForScrollDisplay")]
    public Common ErrorForScrollDisplay
    {
      get => errorForScrollDisplay ??= new();
      set => errorForScrollDisplay = value;
    }

    /// <summary>
    /// Gets a value of Explicit1.
    /// </summary>
    [JsonIgnore]
    public Array<ExplicitGroup> Explicit1 => explicit1 ??= new(
      ExplicitGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Explicit1 for json serialization.
    /// </summary>
    [JsonPropertyName("explicit1")]
    [Computed]
    public IList<ExplicitGroup> Explicit1_Json
    {
      get => explicit1;
      set => Explicit1.Assign(value);
    }

    private Common errorForScrollDisplay;
    private Array<ExplicitGroup> explicit1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PromptedCharacter.
    /// </summary>
    [JsonPropertyName("promptedCharacter")]
    public Common PromptedCharacter
    {
      get => promptedCharacter ??= new();
      set => promptedCharacter = value;
    }

    /// <summary>
    /// A value of SelectedCharacter.
    /// </summary>
    [JsonPropertyName("selectedCharacter")]
    public Common SelectedCharacter
    {
      get => selectedCharacter ??= new();
      set => selectedCharacter = value;
    }

    private Common promptedCharacter;
    private Common selectedCharacter;
  }
#endregion
}
