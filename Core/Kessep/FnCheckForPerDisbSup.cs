// Program: FN_CHECK_FOR_PER_DISB_SUP, ID: 372544595, model: 746.
// Short name: SWE00318
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CHECK_FOR_PER_DISB_SUP.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will determine if disbursement suppression is turned on at
/// the person level.
/// </para>
/// </summary>
[Serializable]
public partial class FnCheckForPerDisbSup: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_FOR_PER_DISB_SUP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckForPerDisbSup(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckForPerDisbSup.
  /// </summary>
  public FnCheckForPerDisbSup(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************
    // A.Kinney  05/01/97	Changed Current_Date
    // Fangman  03/21/00  PR 86768  Defaulted suppression date to initialized 
    // values.
    // Fangman  04/10/00  PN 000164  Changed Read for discontinue date > instead
    // of >= so that disbursements would not be suppressed on the "date of
    // discontinuance".
    // Fangman  09/14/00  PR 103323  Removed unneeded views.
    // ****************************************************
    // ****  Check to see if disbursement suppression is on at the person level.
    if (ReadDisbSuppressionStatusHistoryCsePersonAccount())
    {
      export.DisbSuppressionStatusHistory.DiscontinueDate =
        entities.DisbSuppressionStatusHistory.DiscontinueDate;
    }
    else
    {
      export.DisbSuppressionStatusHistory.DiscontinueDate =
        local.Initialized.DiscontinueDate;
    }
  }

  private bool ReadDisbSuppressionStatusHistoryCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistoryCsePersonAccount",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 5);
        entities.CsePersonAccount.Populated = true;
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private CsePerson csePerson;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DisbSuppressionStatusHistory Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Currentzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("currentzzzzzzzzzzzzzzzzzzz")]
    public DateWorkArea Currentzzzzzzzzzzzzzzzzzzz
    {
      get => currentzzzzzzzzzzzzzzzzzzz ??= new();
      set => currentzzzzzzzzzzzzzzzzzzz = value;
    }

    private DisbSuppressionStatusHistory initialized;
    private DateWorkArea currentzzzzzzzzzzzzzzzzzzz;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }
#endregion
}
