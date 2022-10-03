// Program: CAB_FCR_FORMAT_NAMES, ID: 371067094, model: 746.
// Short name: SWE01287
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_FCR_FORMAT_NAMES.
/// </summary>
[Serializable]
public partial class CabFcrFormatNames: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FCR_FORMAT_NAMES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFcrFormatNames(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFcrFormatNames.
  /// </summary>
  public CabFcrFormatNames(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (IsEmpty(export.CsePersonsWorkSet.FirstName) || IsEmpty
      (export.CsePersonsWorkSet.LastName))
    {
      export.CsePersonsWorkSet.FirstName = "";
      export.CsePersonsWorkSet.MiddleInitial = "";
      export.CsePersonsWorkSet.LastName = "";

      return;
    }

    // *****************  Process First Name *****************
    for(local.ForLoop.Count = 1; local.ForLoop.Count <= 12; ++
      local.ForLoop.Count)
    {
      local.OmitPosition.Count =
        Verify(export.CsePersonsWorkSet.FirstName, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        

      if (local.OmitPosition.Count < 1)
      {
        break;
      }

      if (local.OmitPosition.Count == 12)
      {
        export.CsePersonsWorkSet.FirstName =
          Substring(export.CsePersonsWorkSet.FirstName, 1, 11);
      }
      else if (local.OmitPosition.Count == 1)
      {
        export.CsePersonsWorkSet.FirstName =
          Substring(export.CsePersonsWorkSet.FirstName, 2, 11);
      }
      else
      {
        local.Next.Count = local.OmitPosition.Count + 1;
        local.Length.Count = 12 - local.OmitPosition.Count;
        export.CsePersonsWorkSet.FirstName =
          Substring(export.CsePersonsWorkSet.FirstName,
          CsePersonsWorkSet.FirstName_MaxLength, 1, local.OmitPosition.Count -
          1) + Substring
          (export.CsePersonsWorkSet.FirstName,
          CsePersonsWorkSet.FirstName_MaxLength, local.Next.Count,
          local.Length.Count);
      }
    }

    if (Equal(export.CsePersonsWorkSet.FirstName, 1, 2, "XX") || IsEmpty
      (export.CsePersonsWorkSet.FirstName))
    {
      export.CsePersonsWorkSet.FirstName = "";
      export.CsePersonsWorkSet.MiddleInitial = "";
      export.CsePersonsWorkSet.LastName = "";

      return;
    }

    // ***************  Process Middle Initial ***************
    local.OmitPosition.Count =
      Verify(export.CsePersonsWorkSet.MiddleInitial,
      "ABCDEFGHIJKLMNOPQRSTUVWXYZ");

    if (local.OmitPosition.Count == 1)
    {
      export.CsePersonsWorkSet.MiddleInitial = "";
    }

    // *****************  Process Last Name *****************
    for(local.ForLoop.Count = 1; local.ForLoop.Count <= 17; ++
      local.ForLoop.Count)
    {
      local.OmitPosition.Count =
        Verify(export.CsePersonsWorkSet.LastName, "-ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        

      if (local.OmitPosition.Count < 1)
      {
        break;
      }

      if (local.OmitPosition.Count == 17)
      {
        export.CsePersonsWorkSet.LastName =
          Substring(export.CsePersonsWorkSet.LastName, 1, 16);
      }
      else if (local.OmitPosition.Count == 1)
      {
        export.CsePersonsWorkSet.LastName =
          Substring(export.CsePersonsWorkSet.LastName, 2, 16);
      }
      else
      {
        local.Next.Count = local.OmitPosition.Count + 1;
        local.Length.Count = 17 - local.OmitPosition.Count;
        export.CsePersonsWorkSet.LastName =
          Substring(export.CsePersonsWorkSet.LastName,
          CsePersonsWorkSet.LastName_MaxLength, 1, local.OmitPosition.Count -
          1) + Substring
          (export.CsePersonsWorkSet.LastName,
          CsePersonsWorkSet.LastName_MaxLength, local.Next.Count,
          local.Length.Count);
      }
    }

    if (Equal(export.CsePersonsWorkSet.LastName, 1, 2, "XX") || IsEmpty
      (export.CsePersonsWorkSet.LastName))
    {
      export.CsePersonsWorkSet.FirstName = "";
      export.CsePersonsWorkSet.MiddleInitial = "";
      export.CsePersonsWorkSet.LastName = "";
    }
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of OmitPosition.
    /// </summary>
    [JsonPropertyName("omitPosition")]
    public Common OmitPosition
    {
      get => omitPosition ??= new();
      set => omitPosition = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Common Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of ForLoop.
    /// </summary>
    [JsonPropertyName("forLoop")]
    public Common ForLoop
    {
      get => forLoop ??= new();
      set => forLoop = value;
    }

    private Common omitPosition;
    private Common next;
    private Common length;
    private Common forLoop;
  }
#endregion
}
