// Program: FN_DETERMINE_PROGRAM_STATE, ID: 374422481, model: 746.
// Short name: SWE01615
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_PROGRAM_STATE.
/// </summary>
[Serializable]
public partial class FnDetermineProgramState: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_PROGRAM_STATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineProgramState(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineProgramState.
  /// </summary>
  public FnDetermineProgramState(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.HardcodedAf.SystemGeneratedIdentifier = 2;
    local.HardcodedAfi.SystemGeneratedIdentifier = 14;
    local.HardcodedFc.SystemGeneratedIdentifier = 15;
    local.HardcodedFci.SystemGeneratedIdentifier = 16;
    local.HardcodedNa.SystemGeneratedIdentifier = 12;
    local.HardcodedNai.SystemGeneratedIdentifier = 18;
    local.HardcodedNc.SystemGeneratedIdentifier = 13;
    local.HardcodedNf.SystemGeneratedIdentifier = 3;
    local.HardcodedMai.SystemGeneratedIdentifier = 17;

    // *****************************************************
    //     AF    :  AF-PA
    //     FC    :  FC-PA
    //     NA    :  NA-NA
    //     NF    :  NF
    //     NC    :  NC
    //     AFI   :  AFI
    //     FCI   :  FCI
    //     NAI   :  NAI
    // *****************************************************
    if (import.KeyOnly.SystemGeneratedIdentifier == local
      .HardcodedAf.SystemGeneratedIdentifier)
    {
      export.DprProgram.ProgramState = "PA";
    }
    else if (import.KeyOnly.SystemGeneratedIdentifier == local
      .HardcodedFc.SystemGeneratedIdentifier)
    {
      export.DprProgram.ProgramState = "PA";
    }
    else if (import.KeyOnly.SystemGeneratedIdentifier == local
      .HardcodedNa.SystemGeneratedIdentifier)
    {
      export.DprProgram.ProgramState = "NA";
    }
    else
    {
      export.DprProgram.ProgramState = "";
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
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public Program KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    private Program keyOnly;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    private DprProgram dprProgram;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
#endregion
}
