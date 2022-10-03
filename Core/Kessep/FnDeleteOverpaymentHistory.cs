// Program: FN_DELETE_OVERPAYMENT_HISTORY, ID: 372045339, model: 746.
// Short name: SWE01585
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DELETE_OVERPAYMENT_HISTORY.
/// </summary>
[Serializable]
public partial class FnDeleteOverpaymentHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_OVERPAYMENT_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteOverpaymentHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteOverpaymentHistory.
  /// </summary>
  public FnDeleteOverpaymentHistory(IContext context, Import import,
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
    // ------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // 07/30/96   G. Lofton - MTW			Initial code.
    // ------------------------------------------------------------------
    if (ReadCsePersonAccount())
    {
      if (ReadOverpaymentHistory())
      {
        DeleteOverpaymentHistory();
      }
      else
      {
        ExitState = "FN0000_OVERPAYMENT_HISTORY_NF";
      }
    }
    else
    {
      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";
    }
  }

  private void DeleteOverpaymentHistory()
  {
    Update("DeleteOverpaymentHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.OverpaymentHistory.CpaType);
        db.
          SetString(command, "cspNumber", entities.OverpaymentHistory.CspNumber);
          
        db.SetDate(
          command, "effectiveDt",
          entities.OverpaymentHistory.EffectiveDt.GetValueOrDefault());
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadOverpaymentHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.OverpaymentHistory.Populated = false;

    return Read("ReadOverpaymentHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetDate(
          command, "effectiveDt",
          import.OverpaymentHistory.EffectiveDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OverpaymentHistory.CpaType = db.GetString(reader, 0);
        entities.OverpaymentHistory.CspNumber = db.GetString(reader, 1);
        entities.OverpaymentHistory.EffectiveDt = db.GetDate(reader, 2);
        entities.OverpaymentHistory.OverpaymentInd = db.GetString(reader, 3);
        entities.OverpaymentHistory.Populated = true;
        CheckValid<OverpaymentHistory>("CpaType",
          entities.OverpaymentHistory.CpaType);
        CheckValid<OverpaymentHistory>("OverpaymentInd",
          entities.OverpaymentHistory.OverpaymentInd);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of OverpaymentHistory.
    /// </summary>
    [JsonPropertyName("overpaymentHistory")]
    public OverpaymentHistory OverpaymentHistory
    {
      get => overpaymentHistory ??= new();
      set => overpaymentHistory = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private OverpaymentHistory overpaymentHistory;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OverpaymentHistory.
    /// </summary>
    [JsonPropertyName("overpaymentHistory")]
    public OverpaymentHistory OverpaymentHistory
    {
      get => overpaymentHistory ??= new();
      set => overpaymentHistory = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private OverpaymentHistory overpaymentHistory;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
