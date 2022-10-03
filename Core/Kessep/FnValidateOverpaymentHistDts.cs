// Program: FN_VALIDATE_OVERPAYMENT_HIST_DTS, ID: 372045337, model: 746.
// Short name: SWE01584
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_VALIDATE_OVERPAYMENT_HIST_DTS.
/// </summary>
[Serializable]
public partial class FnValidateOverpaymentHistDts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_VALIDATE_OVERPAYMENT_HIST_DTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnValidateOverpaymentHistDts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnValidateOverpaymentHistDts.
  /// </summary>
  public FnValidateOverpaymentHistDts(IContext context, Import import,
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
    // 05/01/97   A.Kinney				Changed Current_Date
    // ------------------------------------------------------------------
    local.Current.Date = Now().Date;

    if (!ReadCsePersonAccount())
    {
      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";

      return;
    }

    if (ReadOverpaymentHistory1())
    {
      if (AsChar(entities.OverpaymentHistory.OverpaymentInd) == AsChar
        (import.OverpaymentHistory.OverpaymentInd))
      {
        foreach(var item in ReadOverpaymentHistory2())
        {
          if (AsChar(entities.NextEff.OverpaymentInd) != AsChar
            (import.OverpaymentHistory.OverpaymentInd) && Lt
            (entities.NextEff.EffectiveDt, import.OverpaymentHistory.EffectiveDt))
            
          {
            return;
          }
        }

        ExitState = "FN0000_OVRPYMNT_INT_EXISTS_ALRDY";
      }
    }
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

  private bool ReadOverpaymentHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.OverpaymentHistory.Populated = false;

    return Read("ReadOverpaymentHistory1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
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

  private IEnumerable<bool> ReadOverpaymentHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.NextEff.Populated = false;

    return ReadEach("ReadOverpaymentHistory2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NextEff.CpaType = db.GetString(reader, 0);
        entities.NextEff.CspNumber = db.GetString(reader, 1);
        entities.NextEff.EffectiveDt = db.GetDate(reader, 2);
        entities.NextEff.OverpaymentInd = db.GetString(reader, 3);
        entities.NextEff.Populated = true;
        CheckValid<OverpaymentHistory>("CpaType", entities.NextEff.CpaType);
        CheckValid<OverpaymentHistory>("OverpaymentInd",
          entities.NextEff.OverpaymentInd);

        return true;
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
    /// A value of OverpaymentHistory.
    /// </summary>
    [JsonPropertyName("overpaymentHistory")]
    public OverpaymentHistory OverpaymentHistory
    {
      get => overpaymentHistory ??= new();
      set => overpaymentHistory = value;
    }

    private CsePerson csePerson;
    private OverpaymentHistory overpaymentHistory;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of OverpaymentHistory.
    /// </summary>
    [JsonPropertyName("overpaymentHistory")]
    public OverpaymentHistory OverpaymentHistory
    {
      get => overpaymentHistory ??= new();
      set => overpaymentHistory = value;
    }

    private DateWorkArea current;
    private OverpaymentHistory overpaymentHistory;
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
    /// A value of OverpaymentHistory.
    /// </summary>
    [JsonPropertyName("overpaymentHistory")]
    public OverpaymentHistory OverpaymentHistory
    {
      get => overpaymentHistory ??= new();
      set => overpaymentHistory = value;
    }

    /// <summary>
    /// A value of NextEff.
    /// </summary>
    [JsonPropertyName("nextEff")]
    public OverpaymentHistory NextEff
    {
      get => nextEff ??= new();
      set => nextEff = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private OverpaymentHistory overpaymentHistory;
    private OverpaymentHistory nextEff;
  }
#endregion
}
