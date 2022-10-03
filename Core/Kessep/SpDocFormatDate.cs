// Program: SP_DOC_FORMAT_DATE, ID: 372134031, model: 746.
// Short name: SWE01356
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DOC_FORMAT_DATE.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
public partial class SpDocFormatDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_FORMAT_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocFormatDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocFormatDate.
  /// </summary>
  public SpDocFormatDate(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------
    //               M A I N T E N A N C E   L O G
    // -------------------------------------------------------------------------
    // 04/08/2009	J Huss		CQ# 10302	Added placeholder option.
    // -------------------------------------------------------------------------
    export.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(import.DateWorkArea.Date, local.MaxDate.Date) || Equal
      (import.DateWorkArea.Date, local.NullDate.Date))
    {
      if (AsChar(import.PopulatePlaceholder.Flag) == 'Y')
      {
        export.FieldValue.Value = "UNKNOWN";
      }
    }
    else
    {
      local.DateWorkArea.TextDate =
        NumberToString(DateToInt(import.DateWorkArea.Date), 8, 8);
      export.FieldValue.Value =
        Substring(local.DateWorkArea.TextDate, DateWorkArea.TextDate_MaxLength,
        5, 2) + "/" + Substring
        (local.DateWorkArea.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2) + "/"
        + Substring
        (local.DateWorkArea.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4);
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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
    /// A value of PopulatePlaceholder.
    /// </summary>
    [JsonPropertyName("populatePlaceholder")]
    public Common PopulatePlaceholder
    {
      get => populatePlaceholder ??= new();
      set => populatePlaceholder = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private Common populatePlaceholder;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    private FieldValue fieldValue;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private DateWorkArea nullDate;
    private DateWorkArea dateWorkArea;
    private DateWorkArea maxDate;
  }
#endregion
}
