// Program: SP_DOC_FORMAT_PHONE_NUMBER, ID: 372134148, model: 746.
// Short name: SWE01527
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DOC_FORMAT_PHONE_NUMBER.
/// </para>
/// <para>
/// Input is phone number area code, 7 digit and extension.
/// Output is formatted as (999) 9999999 xAAAAA
/// </para>
/// </summary>
[Serializable]
public partial class SpDocFormatPhoneNumber: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_FORMAT_PHONE_NUMBER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocFormatPhoneNumber(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocFormatPhoneNumber.
  /// </summary>
  public SpDocFormatPhoneNumber(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------
    // Date		Developer	Description
    // -----------------------------------------------------------------------
    // 06/04/1996	Siraj Konkader	Initial Development
    // 06/16/1999	M Ramirez	Removed use of sp_eab_concat
    // 06/16/1999	M Ramirez	Changed formatting to include '-'
    // 06/16/1999	M Ramirez	Added check for missing phone number
    // -----------------------------------------------------------------------
    // -----------------------------------------------------------------------
    // Input is phone number area code, 7 digit and extension.
    // Output is formatted as (999) 9999999 xAAAAA
    // -----------------------------------------------------------------------
    // mjr
    // ----------------------------------------------
    // 06/16/1999
    // Added check for missing phone number
    // -----------------------------------------------------------
    if (import.SpPrintWorkSet.Phone7Digit <= 0)
    {
      return;
    }

    if (import.SpPrintWorkSet.PhoneAreaCode > 0)
    {
      local.FieldValue.Value =
        NumberToString(import.SpPrintWorkSet.PhoneAreaCode, 13, 3);
      export.FieldValue.Value = "(" + (local.FieldValue.Value ?? "");
      local.FieldValue.Value = ")";
      export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
        local.FieldValue.Value ?? "");
    }

    // mjr
    // ----------------------------------------------
    // 06/16/1999
    // Changed formatting to include '-'
    // -----------------------------------------------------------
    local.FieldValue.Value =
      NumberToString(import.SpPrintWorkSet.Phone7Digit, 9, 3);

    if (import.SpPrintWorkSet.PhoneAreaCode > 0)
    {
      local.FieldValue.Value = " " + (local.FieldValue.Value ?? "");
      export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
        local.FieldValue.Value ?? "");
    }
    else
    {
      export.FieldValue.Value = local.FieldValue.Value ?? "";
    }

    local.FieldValue.Value =
      NumberToString(import.SpPrintWorkSet.Phone7Digit, 12, 4);
    local.FieldValue.Value = "-" + (local.FieldValue.Value ?? "");
    export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
      local.FieldValue.Value ?? "");

    if (!IsEmpty(import.SpPrintWorkSet.PhoneExt))
    {
      local.FieldValue.Value = " x" + import.SpPrintWorkSet.PhoneExt;
      export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
        local.FieldValue.Value ?? "");
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
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
    }

    private SpPrintWorkSet spPrintWorkSet;
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
#endregion
}
