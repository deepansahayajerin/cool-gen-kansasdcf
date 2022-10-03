// Program: FN_VALIDATE_AP_PYR_STMT_CPN_DTS, ID: 371738140, model: 746.
// Short name: SWE01754
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_VALIDATE_AP_PYR_STMT_CPN_DTS.
/// </summary>
[Serializable]
public partial class FnValidateApPyrStmtCpnDts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_VALIDATE_AP_PYR_STMT_CPN_DTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnValidateApPyrStmtCpnDts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnValidateApPyrStmtCpnDts.
  /// </summary>
  public FnValidateApPyrStmtCpnDts(IContext context, Import import,
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
    // 07/25/96   G. Lofton - MTW			Initial code.
    // 9/2/98     E. Parker -  SRS			Changed logic to allow disc date = current 
    // date and effective date of new record = disc date of an old record.
    // ------------------------------------------------------------------
    if (!Lt(import.StmtCouponSuppStatusHist.EffectiveDate,
      import.StmtCouponSuppStatusHist.DiscontinueDate))
    {
      ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

      return;
    }

    if (Lt(import.StmtCouponSuppStatusHist.DiscontinueDate, Now().Date))
    {
      ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

      return;
    }

    if (!ReadCsePersonAccount())
    {
      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";

      return;
    }

    foreach(var item in ReadStmtCouponSuppStatusHist())
    {
      if (entities.StmtCouponSuppStatusHist.SystemGeneratedIdentifier == import
        .StmtCouponSuppStatusHist.SystemGeneratedIdentifier && Equal
        (import.Common.ActionEntry, "C") || !
        Lt(Now().Date, entities.StmtCouponSuppStatusHist.DiscontinueDate))
      {
        continue;
      }

      if (!Lt(entities.StmtCouponSuppStatusHist.EffectiveDate,
        import.StmtCouponSuppStatusHist.EffectiveDate) && !
        Lt(import.StmtCouponSuppStatusHist.DiscontinueDate,
        entities.StmtCouponSuppStatusHist.EffectiveDate) || Lt
        (import.StmtCouponSuppStatusHist.EffectiveDate,
        entities.StmtCouponSuppStatusHist.DiscontinueDate) && !
        Lt(import.StmtCouponSuppStatusHist.DiscontinueDate,
        entities.StmtCouponSuppStatusHist.DiscontinueDate))
      {
        ExitState = "OVERLAPPING_DATE_RANGE";

        return;
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

  private IEnumerable<bool> ReadStmtCouponSuppStatusHist()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.StmtCouponSuppStatusHist.Populated = false;

    return ReadEach("ReadStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.StmtCouponSuppStatusHist.CpaType = db.GetString(reader, 0);
        entities.StmtCouponSuppStatusHist.CspNumber = db.GetString(reader, 1);
        entities.StmtCouponSuppStatusHist.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.StmtCouponSuppStatusHist.Type1 = db.GetString(reader, 3);
        entities.StmtCouponSuppStatusHist.EffectiveDate = db.GetDate(reader, 4);
        entities.StmtCouponSuppStatusHist.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.StmtCouponSuppStatusHist.CreatedBy = db.GetString(reader, 6);
        entities.StmtCouponSuppStatusHist.DocTypeToSuppress =
          db.GetString(reader, 7);
        entities.StmtCouponSuppStatusHist.Populated = true;
        CheckValid<StmtCouponSuppStatusHist>("CpaType",
          entities.StmtCouponSuppStatusHist.CpaType);
        CheckValid<StmtCouponSuppStatusHist>("Type1",
          entities.StmtCouponSuppStatusHist.Type1);
        CheckValid<StmtCouponSuppStatusHist>("DocTypeToSuppress",
          entities.StmtCouponSuppStatusHist.DocTypeToSuppress);

        return true;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    /// <summary>
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
    }

    private Common common;
    private CsePerson csePerson;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
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
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
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

    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
