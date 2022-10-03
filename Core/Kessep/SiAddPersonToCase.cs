// Program: SI_ADD_PERSON_TO_CASE, ID: 371727800, model: 746.
// Short name: SWE01096
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_ADD_PERSON_TO_CASE.
/// </summary>
[Serializable]
public partial class SiAddPersonToCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ADD_PERSON_TO_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAddPersonToCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAddPersonToCase.
  /// </summary>
  public SiAddPersonToCase(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ++import.Import1.Count;

    import.Import1.Index = import.Import1.Count - 1;
    import.Import1.CheckSize();

    MoveCsePersonsWorkSet(import.Add,
      import.Import1.Update.DetailCsePersonsWorkSet);

    if (Equal(import.Import1.Item.DetailCsePersonsWorkSet.Ssn, "000000000"))
    {
      import.Import1.Update.DetailCsePersonsWorkSet.Ssn = "";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>
      /// A value of DetailFamily.
      /// </summary>
      [JsonPropertyName("detailFamily")]
      public CaseRole DetailFamily
      {
        get => detailFamily ??= new();
        set => detailFamily = value;
      }

      /// <summary>
      /// A value of DetailCaseConfrm.
      /// </summary>
      [JsonPropertyName("detailCaseConfrm")]
      public Common DetailCaseConfrm
      {
        get => detailCaseConfrm ??= new();
        set => detailCaseConfrm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
      private CaseRole detailFamily;
      private Common detailCaseConfrm;
    }

    /// <summary>
    /// A value of Add.
    /// </summary>
    [JsonPropertyName("add")]
    public CsePersonsWorkSet Add
    {
      get => add ??= new();
      set => add = value;
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

    private CsePersonsWorkSet add;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
