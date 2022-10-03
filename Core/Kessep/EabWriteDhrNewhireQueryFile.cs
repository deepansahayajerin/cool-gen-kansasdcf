// Program: EAB_WRITE_DHR_NEWHIRE_QUERY_FILE, ID: 372745442, model: 746.
// Short name: SWEXIC01
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_WRITE_DHR_NEWHIRE_QUERY_FILE.
/// </summary>
[Serializable]
public partial class EabWriteDhrNewhireQueryFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_DHR_NEWHIRE_QUERY_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWriteDhrNewhireQueryFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWriteDhrNewhireQueryFile.
  /// </summary>
  public EabWriteDhrNewhireQueryFile(IContext context, Import import,
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
      "SWEXIC01", context, import, export, EabOptions.Hpvp);
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
    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      [Member(Index = 2, AccessFields = false, Members
        = new[] { "SystemGeneratedId" })]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      [Member(Index = 3, AccessFields = false, Members = new[]
      {
        "LastName",
        "FirstName",
        "MiddleInitial"
      })]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 case1;
      private Office office;
      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Number",
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 3)]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
    }

    private EabFileHandling eabFileHandling;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<GrpGroup> grp;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }
#endregion
}
