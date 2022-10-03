// Program: OE_FPLS_SESA_STATE_SUBSTRING, ID: 372356130, model: 746.
// Short name: SWE00910
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
/// A program: OE_FPLS_SESA_STATE_SUBSTRING.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block imports FPLS-LOCATE-REQUEST Send-Request attribute and 
/// substrings SESA states dropping 'B' delimeters between the states.
/// </para>
/// </summary>
[Serializable]
public partial class OeFplsSesaStateSubstring: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FPLS_SESA_STATE_SUBSTRING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFplsSesaStateSubstring(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFplsSesaStateSubstring.
  /// </summary>
  public OeFplsSesaStateSubstring(IContext context, Import import, Export export)
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
    local.SubstringPointer.Count = 2;
    export.Sesa.Index = -1;

    for(export.Sesa.Index = 0; export.Sesa.Index < 6; ++export.Sesa.Index)
    {
      if (!export.Sesa.CheckSize())
      {
        break;
      }

      if (Length(TrimEnd(import.FplsLocateRequest.SendRequestTo)) > local
        .SubstringPointer.Count)
      {
        export.Sesa.Update.Sesa1.State =
          Substring(import.FplsLocateRequest.SendRequestTo,
          local.SubstringPointer.Count, 2);
        local.SubstringPointer.Count =
          (int)((long)local.SubstringPointer.Count + 3);
      }
      else
      {
        return;
      }
    }

    export.Sesa.CheckIndex();
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
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    private FplsLocateRequest fplsLocateRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A SesaGroup group.</summary>
    [Serializable]
    public class SesaGroup
    {
      /// <summary>
      /// A value of Sesa1.
      /// </summary>
      [JsonPropertyName("sesa1")]
      public OblgWork Sesa1
      {
        get => sesa1 ??= new();
        set => sesa1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private OblgWork sesa1;
    }

    /// <summary>
    /// Gets a value of Sesa.
    /// </summary>
    [JsonIgnore]
    public Array<SesaGroup> Sesa => sesa ??= new(SesaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Sesa for json serialization.
    /// </summary>
    [JsonPropertyName("sesa")]
    [Computed]
    public IList<SesaGroup> Sesa_Json
    {
      get => sesa;
      set => Sesa.Assign(value);
    }

    private Array<SesaGroup> sesa;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SubstringPointer.
    /// </summary>
    [JsonPropertyName("substringPointer")]
    public Common SubstringPointer
    {
      get => substringPointer ??= new();
      set => substringPointer = value;
    }

    private Common substringPointer;
  }
#endregion
}
