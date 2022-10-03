// Program: SI_EAB_RETRIEVE_ADABAS_PRSN_PGMS, ID: 371728407, model: 746.
// Short name: SWEXIR50
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_EAB_RETRIEVE_ADABAS_PRSN_PGMS.
/// </summary>
[Serializable]
public partial class SiEabRetrieveAdabasPrsnPgms: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EAB_RETRIEVE_ADABAS_PRSN_PGMS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEabRetrieveAdabasPrsnPgms(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEabRetrieveAdabasPrsnPgms.
  /// </summary>
  public SiEabRetrieveAdabasPrsnPgms(IContext context, Import import,
    Export export):
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
      "SWEXIR50", context, import, export, EabOptions.NoIefParams);
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    [Member(Index = 1, Members = new[] { "Date" })]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 2, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private DateWorkArea current;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Det.
      /// </summary>
      [JsonPropertyName("det")]
      [Member(Index = 1, Members = new[]
      {
        "SourceOfFunds",
        "ProgramCode",
        "ProgEffectiveDate",
        "ProgramEndDate",
        "AeProgramSubtype"
      })]
      public InterfacePersonProgram Det
      {
        get => det ??= new();
        set => det = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private InterfacePersonProgram det;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 1)]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    [Member(Index = 2, Members = new[]
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

    private Array<GroupGroup> group;
    private AbendData abendData;
  }
#endregion
}
