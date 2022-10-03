// Program: SP_PRINT_DECODE_RETURN_CODE, ID: 371749361, model: 746.
// Short name: SWE01770
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_PRINT_DECODE_RETURN_CODE.
/// </para>
/// <para>
/// This CAB decodes the return code from the print process and returns the 
/// appropriate exitstate to the calling procedure.
/// Match export view if you want the return code removed from the work view.
/// ====================================
/// Used to be sp_ddoc_decode_title
/// This CAB decodes the title of a field in a document from a number into text.
/// </para>
/// </summary>
[Serializable]
public partial class SpPrintDecodeReturnCode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DECODE_RETURN_CODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDecodeReturnCode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDecodeReturnCode.
  /// </summary>
  public SpPrintDecodeReturnCode(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------
    // Date		Developer	Request #      Description
    // ----------------------------------------------------------------
    // 10/09/1998	M. Ramirez	Initial Development
    // ----------------------------------------------------------------
    UseSpDocSetLiterals();
    local.Position.Count =
      Find(String(import.WorkArea.Text50, WorkArea.Text50_MaxLength),
      TrimEnd(local.SpDocLiteral.IdDocument));

    if (local.Position.Count == 0)
    {
      // mjr---> Setting export
      export.WorkArea.Text50 = import.WorkArea.Text50;

      return;
    }

    local.WorkArea.Text50 =
      Substring(import.WorkArea.Text50, local.Position.Count +
      6, Length(TrimEnd(import.WorkArea.Text50)) - (local.Position.Count + 5));

    // mjr---> Setting export
    if (local.Position.Count > 1)
    {
      export.WorkArea.Text50 =
        Substring(import.WorkArea.Text50, 1, local.Position.Count - 1);
    }

    local.Position.Count = Find(local.WorkArea.Text50, ";");

    if (local.Position.Count > 0)
    {
      local.WorkArea.Text50 =
        Substring(local.WorkArea.Text50, 1, local.Position.Count - 1);

      // mjr---> Setting export
      export.WorkArea.Text50 = TrimEnd(export.WorkArea.Text50) + Substring
        (local.WorkArea.Text50, WorkArea.Text50_MaxLength,
        local.Position.Count + 1, Length(TrimEnd(local.WorkArea.Text50)) -
        local.Position.Count);
    }

    switch(TrimEnd(local.WorkArea.Text50))
    {
      case "CANCEL":
        ExitState = "SP0000_PRINT_CANCELED";

        break;
      case "RET0":
        ExitState = "SP0000_DOWNLOAD_SUCCESSFUL";

        break;
      case "RET1":
        ExitState = "SP0000_MULTIPLE_OBL_TYPE_FOR_DOC";

        break;
      case "RET2":
        ExitState = "SP0000_PRINT_REQUIRES_LEGAL_DTL";

        break;
      default:
        ExitState = "SP0000_DOWNLOAD_UNSUCCESSFUL";

        break;
    }
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    local.SpDocLiteral.IdDocument = useExport.SpDocLiteral.IdDocument;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public Common Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private WorkArea workArea;
    private Common zdel;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public WorkArea Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private WorkArea workArea;
    private WorkArea zdel;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    private SpDocLiteral spDocLiteral;
    private WorkArea workArea;
    private Common position;
  }
#endregion
}
