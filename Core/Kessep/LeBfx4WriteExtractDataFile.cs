﻿// Program: LE_BFX4_WRITE_EXTRACT_DATA_FILE, ID: 371310567, model: 746.
// Short name: SWEXLE15
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX4_WRITE_EXTRACT_DATA_FILE.
/// </summary>
[Serializable]
public partial class LeBfx4WriteExtractDataFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX4_WRITE_EXTRACT_DATA_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx4WriteExtractDataFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx4WriteExtractDataFile.
  /// </summary>
  public LeBfx4WriteExtractDataFile(IContext context, Import import,
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
      "SWEXLE15", context, import, export, EabOptions.Hpvp);
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
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "Number", "FormattedName" })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "City",
      "State",
      "ZipCode",
      "Street2"
    })]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private EabFileHandling eabFileHandling;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonAddress csePersonAddress;
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
