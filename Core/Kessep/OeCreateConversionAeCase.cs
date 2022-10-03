// Program: OE_CREATE_CONVERSION_AE_CASE, ID: 371025649, model: 746.
// Short name: SWE02881
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_CREATE_CONVERSION_AE_CASE.
/// </summary>
[Serializable]
public partial class OeCreateConversionAeCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CREATE_CONVERSION_AE_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCreateConversionAeCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCreateConversionAeCase.
  /// </summary>
  public OeCreateConversionAeCase(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ReadImHousehold();
    local.MaxCaseNumeric.TotalInteger =
      StringToNumber(Substring(
        local.MaxCaseText.AeCaseNo, ImHousehold.AeCaseNo_MaxLength, 2, 7));
    ++local.MaxCaseNumeric.TotalInteger;
    export.ImHousehold.AeCaseNo = "C" + NumberToString
      (local.MaxCaseNumeric.TotalInteger, 9, 7);
    export.ImHousehold.FirstBenefitDate = import.ImHousehold.FirstBenefitDate;
    local.Action.ActionEntry = "AD";
    UseOeMaintainImHousehold();
  }

  private void UseOeMaintainImHousehold()
  {
    var useImport = new OeMaintainImHousehold.Import();
    var useExport = new OeMaintainImHousehold.Export();

    useImport.Action.ActionEntry = local.Action.ActionEntry;
    useImport.ImHousehold.Assign(export.ImHousehold);

    Call(OeMaintainImHousehold.Execute, useImport, useExport);
  }

  private bool ReadImHousehold()
  {
    local.MaxCaseText.Populated = false;

    return Read("ReadImHousehold",
      null,
      (db, reader) =>
      {
        local.MaxCaseText.AeCaseNo = db.GetString(reader, 0);
        local.MaxCaseText.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    private ImHousehold imHousehold;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    private ImHousehold imHousehold;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MaxCaseText.
    /// </summary>
    [JsonPropertyName("maxCaseText")]
    public ImHousehold MaxCaseText
    {
      get => maxCaseText ??= new();
      set => maxCaseText = value;
    }

    /// <summary>
    /// A value of MaxCaseNumeric.
    /// </summary>
    [JsonPropertyName("maxCaseNumeric")]
    public Common MaxCaseNumeric
    {
      get => maxCaseNumeric ??= new();
      set => maxCaseNumeric = value;
    }

    /// <summary>
    /// A value of Action.
    /// </summary>
    [JsonPropertyName("action")]
    public Common Action
    {
      get => action ??= new();
      set => action = value;
    }

    private ImHousehold maxCaseText;
    private Common maxCaseNumeric;
    private Common action;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    private ImHousehold imHousehold;
  }
#endregion
}
