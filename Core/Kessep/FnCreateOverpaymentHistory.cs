// Program: FN_CREATE_OVERPAYMENT_HISTORY, ID: 372045338, model: 746.
// Short name: SWE01583
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_OVERPAYMENT_HISTORY.
/// </summary>
[Serializable]
public partial class FnCreateOverpaymentHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_OVERPAYMENT_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateOverpaymentHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateOverpaymentHistory.
  /// </summary>
  public FnCreateOverpaymentHistory(IContext context, Import import,
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
    MoveOverpaymentHistory(import.OverpaymentHistory, export.OverpaymentHistory);
      

    if (ReadCsePersonAccount())
    {
      try
      {
        CreateOverpaymentHistory();
        export.OverpaymentHistory.Assign(entities.OverpaymentHistory);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OVERPAYMENT_HISTORY_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OVERPAYMENT_HISTORY_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";
    }
  }

  private static void MoveOverpaymentHistory(OverpaymentHistory source,
    OverpaymentHistory target)
  {
    target.OverpaymentInd = source.OverpaymentInd;
    target.EffectiveDt = source.EffectiveDt;
  }

  private void CreateOverpaymentHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var cpaType = entities.CsePersonAccount.Type1;
    var cspNumber = entities.CsePersonAccount.CspNumber;
    var effectiveDt = import.OverpaymentHistory.EffectiveDt;
    var overpaymentInd = import.OverpaymentHistory.OverpaymentInd;
    var createdBy = global.UserId;
    var createdTmst = Now();

    CheckValid<OverpaymentHistory>("CpaType", cpaType);
    CheckValid<OverpaymentHistory>("OverpaymentInd", overpaymentInd);
    entities.OverpaymentHistory.Populated = false;
    Update("CreateOverpaymentHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetString(command, "overpaymentInd", overpaymentInd);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
      });

    entities.OverpaymentHistory.CpaType = cpaType;
    entities.OverpaymentHistory.CspNumber = cspNumber;
    entities.OverpaymentHistory.EffectiveDt = effectiveDt;
    entities.OverpaymentHistory.OverpaymentInd = overpaymentInd;
    entities.OverpaymentHistory.CreatedBy = createdBy;
    entities.OverpaymentHistory.CreatedTmst = createdTmst;
    entities.OverpaymentHistory.Populated = true;
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
    /// <summary>
    /// A value of OverpaymentHistory.
    /// </summary>
    [JsonPropertyName("overpaymentHistory")]
    public OverpaymentHistory OverpaymentHistory
    {
      get => overpaymentHistory ??= new();
      set => overpaymentHistory = value;
    }

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

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private OverpaymentHistory overpaymentHistory;
  }
#endregion
}
