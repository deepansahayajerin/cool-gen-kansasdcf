// Program: FN_BUILD_PROGRAM_VALUES, ID: 372279905, model: 746.
// Short name: SWE02338
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BUILD_PROGRAM_VALUES.
/// </summary>
[Serializable]
public partial class FnBuildProgramValues: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BUILD_PROGRAM_VALUES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBuildProgramValues(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBuildProgramValues.
  /// </summary>
  public FnBuildProgramValues(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    foreach(var item in ReadProgram())
    {
      switch(entities.Existing.SystemGeneratedIdentifier)
      {
        case 2:
          export.HardcodedAf.Assign(entities.Existing);

          break;
        case 14:
          export.HardcodedAfi.Assign(entities.Existing);

          break;
        case 15:
          export.HardcodedFc.Assign(entities.Existing);

          break;
        case 16:
          export.HardcodedFci.Assign(entities.Existing);

          break;
        case 12:
          export.HardcodedNa.Assign(entities.Existing);

          break;
        case 18:
          export.HardcodedNai.Assign(entities.Existing);

          break;
        case 13:
          export.HardcodedNc.Assign(entities.Existing);

          break;
        case 3:
          export.HardcodedNf.Assign(entities.Existing);

          break;
        case 17:
          export.HardcodedMai.Assign(entities.Existing);

          break;
        default:
          break;
      }
    }
  }

  private IEnumerable<bool> ReadProgram()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadProgram",
      null,
      (db, reader) =>
      {
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.Code = db.GetString(reader, 1);
        entities.Existing.InterstateIndicator = db.GetString(reader, 2);
        entities.Existing.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HardcodedAf.
    /// </summary>
    [JsonPropertyName("hardcodedAf")]
    public Program HardcodedAf
    {
      get => hardcodedAf ??= new();
      set => hardcodedAf = value;
    }

    /// <summary>
    /// A value of HardcodedAfi.
    /// </summary>
    [JsonPropertyName("hardcodedAfi")]
    public Program HardcodedAfi
    {
      get => hardcodedAfi ??= new();
      set => hardcodedAfi = value;
    }

    /// <summary>
    /// A value of HardcodedFc.
    /// </summary>
    [JsonPropertyName("hardcodedFc")]
    public Program HardcodedFc
    {
      get => hardcodedFc ??= new();
      set => hardcodedFc = value;
    }

    /// <summary>
    /// A value of HardcodedFci.
    /// </summary>
    [JsonPropertyName("hardcodedFci")]
    public Program HardcodedFci
    {
      get => hardcodedFci ??= new();
      set => hardcodedFci = value;
    }

    /// <summary>
    /// A value of HardcodedNa.
    /// </summary>
    [JsonPropertyName("hardcodedNa")]
    public Program HardcodedNa
    {
      get => hardcodedNa ??= new();
      set => hardcodedNa = value;
    }

    /// <summary>
    /// A value of HardcodedNai.
    /// </summary>
    [JsonPropertyName("hardcodedNai")]
    public Program HardcodedNai
    {
      get => hardcodedNai ??= new();
      set => hardcodedNai = value;
    }

    /// <summary>
    /// A value of HardcodedNc.
    /// </summary>
    [JsonPropertyName("hardcodedNc")]
    public Program HardcodedNc
    {
      get => hardcodedNc ??= new();
      set => hardcodedNc = value;
    }

    /// <summary>
    /// A value of HardcodedNf.
    /// </summary>
    [JsonPropertyName("hardcodedNf")]
    public Program HardcodedNf
    {
      get => hardcodedNf ??= new();
      set => hardcodedNf = value;
    }

    /// <summary>
    /// A value of HardcodedMai.
    /// </summary>
    [JsonPropertyName("hardcodedMai")]
    public Program HardcodedMai
    {
      get => hardcodedMai ??= new();
      set => hardcodedMai = value;
    }

    private Program hardcodedAf;
    private Program hardcodedAfi;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNa;
    private Program hardcodedNai;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Program Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Program existing;
  }
#endregion
}
