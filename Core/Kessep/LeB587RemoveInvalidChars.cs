// Program: LE_B587_REMOVE_INVALID_CHARS, ID: 1902480844, model: 746.
// Short name: SWE00844
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B587_REMOVE_INVALID_CHARS.
/// </summary>
[Serializable]
public partial class LeB587RemoveInvalidChars: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B587_REMOVE_INVALID_CHARS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB587RemoveInvalidChars(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB587RemoveInvalidChars.
  /// </summary>
  public LeB587RemoveInvalidChars(IContext context, Import import, Export export)
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
    // -- Create a view of the field value with only characters A-Z, period (.),
    // hyphen (-), apostophe ('), and embedded spaces.
    export.AllCharCleaned.Value = import.FieldValue.Value;
    local.TrimmedLength.Count = Length(TrimEnd(export.AllCharCleaned.Value));
    local.Common.Count = 1;

    for(var limit = local.TrimmedLength.Count; local.Common.Count <= limit; ++
      local.Common.Count)
    {
      if (Find("ABCDEFGHIJKLMNOPQRSTUVWXYZ.-' ",
        Substring(export.AllCharCleaned.Value, local.Common.Count, 1)) == 0)
      {
        if (local.Common.Count == 1)
        {
          export.AllCharCleaned.Value = " " + Substring
            (export.AllCharCleaned.Value, 2, local.TrimmedLength.Count - 1);
        }
        else
        {
          export.AllCharCleaned.Value =
            Substring(export.AllCharCleaned.Value, 1, local.Common.Count - 1) +
            " " + Substring
            (export.AllCharCleaned.Value, local.Common.Count +
            1, local.TrimmedLength.Count - local.Common.Count + 0);
        }
      }
    }

    export.AllCharCleaned.Value = UseLeB587TrimLeadingSpaces2();

    // -- Create a view of the field value where first character is characters A
    // -Z or number 0-9.
    export.FirstCharCleaned.Value = import.FieldValue.Value;
    local.TrimmedLength.Count = Length(TrimEnd(export.FirstCharCleaned.Value));
    local.Common.Count = 1;

    for(var limit = local.TrimmedLength.Count; local.Common.Count <= limit; ++
      local.Common.Count)
    {
      if (Find("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789",
        Substring(export.FirstCharCleaned.Value, local.Common.Count, 1)) == 0)
      {
        if (local.Common.Count == 1)
        {
          export.FirstCharCleaned.Value = " " + Substring
            (export.FirstCharCleaned.Value, 2, local.TrimmedLength.Count - 1);
        }
        else
        {
          export.FirstCharCleaned.Value =
            Substring(export.FirstCharCleaned.Value, 1, local.Common.Count - 1) +
            " " + Substring
            (export.FirstCharCleaned.Value, local.Common.Count +
            1, local.TrimmedLength.Count - local.Common.Count + 0);
        }
      }
      else
      {
        break;
      }
    }

    export.FirstCharCleaned.Value = UseLeB587TrimLeadingSpaces1();
  }

  private string UseLeB587TrimLeadingSpaces1()
  {
    var useImport = new LeB587TrimLeadingSpaces.Import();
    var useExport = new LeB587TrimLeadingSpaces.Export();

    useImport.FieldValue.Value = export.FirstCharCleaned.Value;

    Call(LeB587TrimLeadingSpaces.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private string UseLeB587TrimLeadingSpaces2()
  {
    var useImport = new LeB587TrimLeadingSpaces.Import();
    var useExport = new LeB587TrimLeadingSpaces.Export();

    useImport.FieldValue.Value = export.AllCharCleaned.Value;

    Call(LeB587TrimLeadingSpaces.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
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
    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    private FieldValue fieldValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FirstCharCleaned.
    /// </summary>
    [JsonPropertyName("firstCharCleaned")]
    public FieldValue FirstCharCleaned
    {
      get => firstCharCleaned ??= new();
      set => firstCharCleaned = value;
    }

    /// <summary>
    /// A value of AllCharCleaned.
    /// </summary>
    [JsonPropertyName("allCharCleaned")]
    public FieldValue AllCharCleaned
    {
      get => allCharCleaned ??= new();
      set => allCharCleaned = value;
    }

    private FieldValue firstCharCleaned;
    private FieldValue allCharCleaned;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of TrimmedLength.
    /// </summary>
    [JsonPropertyName("trimmedLength")]
    public Common TrimmedLength
    {
      get => trimmedLength ??= new();
      set => trimmedLength = value;
    }

    private Common common;
    private Common trimmedLength;
  }
#endregion
}
