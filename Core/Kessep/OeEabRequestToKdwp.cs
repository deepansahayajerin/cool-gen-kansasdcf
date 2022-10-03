﻿// Program: OE_EAB_REQUEST_TO_KDWP, ID: 371320724, model: 746.
// Short name: SWEXEW05
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_REQUEST_TO_KDWP.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This EXTERNAL procedure carries view that will be used for receiving the 
/// FPLS responses from the Federal Parent Locator Service in reaction to
/// FPLS_LOCATE_REQUEST's.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabRequestToKdwp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_REQUEST_TO_KDWP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabRequestToKdwp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabRequestToKdwp.
  /// </summary>
  public OeEabRequestToKdwp(IContext context, Import import, Export export):
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
      "SWEXEW05", context, import, export, EabOptions.Hpvp);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of KdwpOutput.
    /// </summary>
    [JsonPropertyName("kdwpOutput")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "CsePersonNumber",
      "LastName",
      "FirstName",
      "MiddleName",
      "Ssn",
      "Dob",
      "AliasInd"
    })]
    public KdwpOutput KdwpOutput
    {
      get => kdwpOutput ??= new();
      set => kdwpOutput = value;
    }

    private External external;
    private KdwpOutput kdwpOutput;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }
#endregion
}
