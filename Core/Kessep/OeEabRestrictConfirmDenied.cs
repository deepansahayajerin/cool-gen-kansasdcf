﻿// Program: OE_EAB_RESTRICT_CONFIRM_DENIED, ID: 371393491, model: 746.
// Short name: SWEXER12
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_RESTRICT_CONFIRM_DENIED.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This EXTERNAL procedure carries view that will be used for receiving the 
/// FPLS responses from the Federal Parent Locator Service in reaction to
/// FPLS_LOCATE_REQUEST's.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabRestrictConfirmDenied: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_RESTRICT_CONFIRM_DENIED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabRestrictConfirmDenied(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabRestrictConfirmDenied.
  /// </summary>
  public OeEabRestrictConfirmDenied(IContext context, Import import,
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
      "SWEXER12", context, import, export, EabOptions.Hpvp);
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

    private External external;
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

    /// <summary>
    /// A value of KdmvFile.
    /// </summary>
    [JsonPropertyName("kdmvFile")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "CsePersonNumber",
      "FileType",
      "LastName",
      "FirstName",
      "Ssn",
      "Dob",
      "DriverLicenseNumber",
      "DmvProblemText",
      "ProcessDate"
    })]
    public KdmvFile KdmvFile
    {
      get => kdmvFile ??= new();
      set => kdmvFile = value;
    }

    private External external;
    private KdmvFile kdmvFile;
  }
#endregion
}
