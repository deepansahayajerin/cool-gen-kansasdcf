// Program: SP_PRINT_MANIPULATE_KEYS, ID: 372148586, model: 746.
// Short name: SWE02260
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_PRINT_MANIPULATE_KEYS.
/// </para>
/// <para>
/// This cab accepts a key value which is to be incorporated into a text listing
/// of key values.
/// The listing is in the form &quot;blah blah;&lt;KEY NAME>:&lt;KEY VALUE>;blah
/// blah&quot;
/// The new key value is in the form &quot;&lt;KEY NAME>:&lt;KEY VALUE>&quot;
/// </para>
/// </summary>
[Serializable]
public partial class SpPrintManipulateKeys: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_MANIPULATE_KEYS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintManipulateKeys(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintManipulateKeys.
  /// </summary>
  public SpPrintManipulateKeys(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------
    // Date      Developer           Request #         Description
    // ----------------------------------------------------------------------
    // 12/09/98  Michael Ramirez                       Initial Development
    // ----------------------------------------------------------------------
    // mjr
    // ------------------------------------------------------
    // Processing Notes
    // This cab accepts a key name and value which is to be incorporated
    // into a text listing of key values.
    // The listing is in the form
    //     "blah blah;<KEY NAME>:<KEY VALUE>;blah blah"
    // The new key value is in the form
    //     "<KEY NAME>:<KEY VALUE>"
    // If the listing is blank, the export listing is set to the
    //  new key value.
    // Else
    //   If the new key name not already found in the listing,
    //    then add the new key at the end of the listing.
    //   Else
    //     Replace the old key value in the listing with the new
    //      key value.
    // ---------------------------------------------------------
    // mjr
    // -----------------------------------------------
    // These literals are used on DKEY.
    // --------------------------------------------------
    UseSpDocSetLiterals();
    local.OldKeyListing.Text50 = import.KeyListing.Text50;

    if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.IdDocument))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdDocument;
    }
    else if (Equal(import.NewKeyName.Text33,
      local.SpDocLiteral.ScreenAdminAction))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdAdminAction;
    }
    else if (Equal(import.NewKeyName.Text33,
      local.SpDocLiteral.ScreenAppointment))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdAppointment;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenBankruptcy))
      
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdBankruptcy;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenChNumber))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdChNumber;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenContact))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdContact;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenGenetic))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdGenetic;
    }
    else if (Equal(import.NewKeyName.Text33,
      local.SpDocLiteral.ScreenIncomeSource))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdIncomeSource;
    }
    else if (Equal(import.NewKeyName.Text33,
      local.SpDocLiteral.ScreenInfoRequest))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdInfoRequest;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenJail))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdJail;
    }
    else if (Equal(import.NewKeyName.Text33,
      local.SpDocLiteral.ScreenLocateRequestAgency))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdLocateRequestAgency;
    }
    else if (Equal(import.NewKeyName.Text33,
      local.SpDocLiteral.ScreenLocateRequestSource))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdLocateRequestSource;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenMilitary))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdMilitary;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenObligation))
      
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdObligationType;
    }
    else if (Equal(import.NewKeyName.Text33,
      local.SpDocLiteral.ScreenObligationAdminAction))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdObligationAdminAction;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenPrNumber))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdPrNumber;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenResource))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdResource;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenTribunal))
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdTribunal;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenWorkerComp))
      
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdWorkerComp;
    }
    else if (Equal(import.NewKeyName.Text33, local.SpDocLiteral.ScreenWorksheet))
      
    {
      local.NewKey.Text10 = local.SpDocLiteral.IdWorksheet;
    }
    else
    {
      export.KeyListing.Text50 = import.KeyListing.Text50;

      return;
    }

    local.NewKey.Text50 = TrimEnd(local.NewKey.Text10) + import
      .NewKeyValue.Text33;

    if (IsEmpty(local.OldKeyListing.Text50))
    {
      export.KeyListing.Text50 = local.NewKey.Text50;

      return;
    }

    local.Position.Count =
      Find(String(local.OldKeyListing.Text50, WorkArea.Text50_MaxLength),
      TrimEnd(local.NewKey.Text10));

    if (local.Position.Count <= 0)
    {
      export.KeyListing.Text50 = TrimEnd(export.KeyListing.Text50) + ";" + local
        .NewKey.Text50;

      return;
    }

    export.KeyListing.Text50 =
      Substring(local.OldKeyListing.Text50, 1, local.Position.Count - 1);
    export.KeyListing.Text50 = TrimEnd(export.KeyListing.Text50) + local
      .NewKey.Text50;

    // mjr---> Determine if this key was the last key in the import
    local.OldKeyListing.Text50 =
      Substring(local.OldKeyListing.Text50, local.Position.Count,
      Length(TrimEnd(import.KeyListing.Text50)) - (local.Position.Count - 1));
    local.Position.Count = Find(local.OldKeyListing.Text50, ";");

    if (local.Position.Count > 0)
    {
      export.KeyListing.Text50 = TrimEnd(export.KeyListing.Text50) + Substring
        (local.OldKeyListing.Text50, WorkArea.Text50_MaxLength,
        local.Position.Count, Length(TrimEnd(local.OldKeyListing.Text50)) -
        (local.Position.Count - 1));
    }
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdAdminAction = source.IdAdminAction;
    target.IdAppointment = source.IdAppointment;
    target.IdBankruptcy = source.IdBankruptcy;
    target.IdContact = source.IdContact;
    target.IdChNumber = source.IdChNumber;
    target.IdDocument = source.IdDocument;
    target.IdGenetic = source.IdGenetic;
    target.IdIncomeSource = source.IdIncomeSource;
    target.IdInfoRequest = source.IdInfoRequest;
    target.IdJail = source.IdJail;
    target.IdLocateRequestAgency = source.IdLocateRequestAgency;
    target.IdLocateRequestSource = source.IdLocateRequestSource;
    target.IdMilitary = source.IdMilitary;
    target.IdObligationAdminAction = source.IdObligationAdminAction;
    target.IdObligationType = source.IdObligationType;
    target.IdPrNumber = source.IdPrNumber;
    target.IdResource = source.IdResource;
    target.IdTribunal = source.IdTribunal;
    target.IdWorkerComp = source.IdWorkerComp;
    target.IdWorksheet = source.IdWorksheet;
    target.ScreenAdminAction = source.ScreenAdminAction;
    target.ScreenAppointment = source.ScreenAppointment;
    target.ScreenBankruptcy = source.ScreenBankruptcy;
    target.ScreenContact = source.ScreenContact;
    target.ScreenChNumber = source.ScreenChNumber;
    target.ScreenGenetic = source.ScreenGenetic;
    target.ScreenIncomeSource = source.ScreenIncomeSource;
    target.ScreenInfoRequest = source.ScreenInfoRequest;
    target.ScreenJail = source.ScreenJail;
    target.ScreenLocateRequestAgency = source.ScreenLocateRequestAgency;
    target.ScreenLocateRequestSource = source.ScreenLocateRequestSource;
    target.ScreenMilitary = source.ScreenMilitary;
    target.ScreenObligation = source.ScreenObligation;
    target.ScreenObligationAdminAction = source.ScreenObligationAdminAction;
    target.ScreenPrNumber = source.ScreenPrNumber;
    target.ScreenResource = source.ScreenResource;
    target.ScreenTribunal = source.ScreenTribunal;
    target.ScreenWorkerComp = source.ScreenWorkerComp;
    target.ScreenWorksheet = source.ScreenWorksheet;
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
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
    /// A value of KeyListing.
    /// </summary>
    [JsonPropertyName("keyListing")]
    public WorkArea KeyListing
    {
      get => keyListing ??= new();
      set => keyListing = value;
    }

    /// <summary>
    /// A value of NewKeyName.
    /// </summary>
    [JsonPropertyName("newKeyName")]
    public WorkArea NewKeyName
    {
      get => newKeyName ??= new();
      set => newKeyName = value;
    }

    /// <summary>
    /// A value of NewKeyValue.
    /// </summary>
    [JsonPropertyName("newKeyValue")]
    public WorkArea NewKeyValue
    {
      get => newKeyValue ??= new();
      set => newKeyValue = value;
    }

    private WorkArea keyListing;
    private WorkArea newKeyName;
    private WorkArea newKeyValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of KeyListing.
    /// </summary>
    [JsonPropertyName("keyListing")]
    public WorkArea KeyListing
    {
      get => keyListing ??= new();
      set => keyListing = value;
    }

    private WorkArea keyListing;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of OldKeyListing.
    /// </summary>
    [JsonPropertyName("oldKeyListing")]
    public WorkArea OldKeyListing
    {
      get => oldKeyListing ??= new();
      set => oldKeyListing = value;
    }

    /// <summary>
    /// A value of ZdelLocalNewKeyLength.
    /// </summary>
    [JsonPropertyName("zdelLocalNewKeyLength")]
    public Common ZdelLocalNewKeyLength
    {
      get => zdelLocalNewKeyLength ??= new();
      set => zdelLocalNewKeyLength = value;
    }

    /// <summary>
    /// A value of NewKey.
    /// </summary>
    [JsonPropertyName("newKey")]
    public WorkArea NewKey
    {
      get => newKey ??= new();
      set => newKey = value;
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

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    private WorkArea oldKeyListing;
    private Common zdelLocalNewKeyLength;
    private WorkArea newKey;
    private Common position;
    private SpDocLiteral spDocLiteral;
  }
#endregion
}
