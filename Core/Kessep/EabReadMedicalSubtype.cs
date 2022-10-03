// Program: EAB_READ_MEDICAL_SUBTYPE, ID: 371277000, model: 746.
// Short name: SWEXGR35
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_MEDICAL_SUBTYPE.
/// </summary>
[Serializable]
public partial class EabReadMedicalSubtype: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_MEDICAL_SUBTYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadMedicalSubtype(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadMedicalSubtype.
  /// </summary>
  public EabReadMedicalSubtype(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXGR35", context, import, export, EabOptions.Hpvp);
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
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Month", "Year" })
      ]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Month", "Year" })
      ]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    private CsePerson csePerson;
    private DateWorkArea start;
    private DateWorkArea end;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GGroup group.</summary>
    [Serializable]
    public class GGroup
    {
      /// <summary>
      /// A value of G1.
      /// </summary>
      [JsonPropertyName("g1")]
      [Member(Index = 1, AccessFields = false, Members = new[] { "MedType" })]
      public PersonProgram G1
      {
        get => g1 ??= new();
        set => g1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private PersonProgram g1;
    }

    /// <summary>
    /// Gets a value of G.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 1)]
    public Array<GGroup> G => g ??= new(GGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of G for json serialization.
    /// </summary>
    [JsonPropertyName("g")]
    [Computed]
    public IList<GGroup> G_Json
    {
      get => g;
      set => G.Assign(value);
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Type1",
      "AdabasFileNumber",
      "AdabasFileAction",
      "AdabasResponseCd",
      "CicsResourceNm",
      "CicsFunctionCd",
      "CicsResponseCd"
    })]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private Array<GGroup> g;
    private AbendData abendData;
  }
#endregion
}
