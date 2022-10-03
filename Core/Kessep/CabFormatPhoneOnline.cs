// Program: CAB_FORMAT_PHONE_ONLINE, ID: 945075630, model: 746.
// Short name: SWE03702
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_FORMAT_PHONE_ONLINE.
/// </summary>
[Serializable]
public partial class CabFormatPhoneOnline: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FORMAT_PHONE_ONLINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFormatPhoneOnline(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFormatPhoneOnline.
  /// </summary>
  public CabFormatPhoneOnline(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.FormattedPhone.Text12 = "";

    if (IsEmpty(import.Phone.Text10))
    {
      export.FormattedPhone.Text12 = "";
    }
    else
    {
      export.FormattedPhone.Text12 =
        Substring(import.Phone.Text10, TextWorkArea.Text10_MaxLength, 1, 3) + "-"
        ;
      export.FormattedPhone.Text12 = TrimEnd(export.FormattedPhone.Text12) + Substring
        (import.Phone.Text10, TextWorkArea.Text10_MaxLength, 4, 3);
      export.FormattedPhone.Text12 = TrimEnd(export.FormattedPhone.Text12) + "-"
        ;
      export.FormattedPhone.Text12 = TrimEnd(export.FormattedPhone.Text12) + Substring
        (import.Phone.Text10, TextWorkArea.Text10_MaxLength, 7, 4);
    }
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
    /// A value of Phone.
    /// </summary>
    [JsonPropertyName("phone")]
    public TextWorkArea Phone
    {
      get => phone ??= new();
      set => phone = value;
    }

    private TextWorkArea phone;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FormattedPhone.
    /// </summary>
    [JsonPropertyName("formattedPhone")]
    public TextWorkArea FormattedPhone
    {
      get => formattedPhone ??= new();
      set => formattedPhone = value;
    }

    private TextWorkArea formattedPhone;
  }
#endregion
}
