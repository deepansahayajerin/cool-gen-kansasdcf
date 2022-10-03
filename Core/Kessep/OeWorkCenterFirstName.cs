// Program: OE_WORK_CENTER_FIRST_NAME, ID: 1902516443, model: 746.
// Short name: SWE00969
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_WORK_CENTER_FIRST_NAME.
/// </summary>
[Serializable]
public partial class OeWorkCenterFirstName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_WORK_CENTER_FIRST_NAME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeWorkCenterFirstName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeWorkCenterFirstName.
  /// </summary>
  public OeWorkCenterFirstName(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.CsePersonsWorkSet.FirstName = import.CsePersonsWorkSet.FirstName;
    local.FirstNameLength.Count =
      Length(TrimEnd(export.CsePersonsWorkSet.FirstName));

    if (local.FirstNameLength.Count < 8)
    {
      for(local.Common.Count = local.FirstNameLength.Count; local
        .Common.Count <= 8; local.Common.Count += 2)
      {
        export.CsePersonsWorkSet.FirstName = " " + TrimEnd
          (export.CsePersonsWorkSet.FirstName);
      }
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
    /// A value of FirstNameLength.
    /// </summary>
    [JsonPropertyName("firstNameLength")]
    public Common FirstNameLength
    {
      get => firstNameLength ??= new();
      set => firstNameLength = value;
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

    private Common firstNameLength;
    private Common common;
  }
#endregion
}
