// Program: FN_CREATE_AP_PYR_STMT_CPN_HIST, ID: 371738139, model: 746.
// Short name: SWE01571
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_AP_PYR_STMT_CPN_HIST.
/// </summary>
[Serializable]
public partial class FnCreateApPyrStmtCpnHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_AP_PYR_STMT_CPN_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateApPyrStmtCpnHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateApPyrStmtCpnHist.
  /// </summary>
  public FnCreateApPyrStmtCpnHist(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // 07/25/96   G. Lofton - MTW			Initial code.
    // 05/19/97  Sumanta - MTW
    //   - Added reason text attribute
    // ------------------------------------------------------------------
    MoveStmtCouponSuppStatusHist(import.StmtCouponSuppStatusHist,
      export.StmtCouponSuppStatusHist);

    if (ReadCsePersonAccount())
    {
      while(local.CreateAttempts.Count < 10)
      {
        try
        {
          CreateStmtCouponSuppStatusHist();
          export.StmtCouponSuppStatusHist.Assign(
            entities.StmtCouponSuppStatusHist);

          return;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ++local.CreateAttempts.Count;

              continue;
            case ErrorCode.PermittedValueViolation:
              // ***--- Sumanta - MTW - 05/19/97
              // ***--- Added Escape stmt to avoid the infinite loop.
              ExitState = "FN0000_STMT_CPN_SUPP_S_HST_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";
    }
  }

  private static void MoveStmtCouponSuppStatusHist(
    StmtCouponSuppStatusHist source, StmtCouponSuppStatusHist target)
  {
    target.DocTypeToSuppress = source.DocTypeToSuppress;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateStmtCouponSuppStatusHist()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var cpaType = entities.CsePersonAccount.Type1;
    var cspNumber = entities.CsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "R";
    var effectiveDate = import.StmtCouponSuppStatusHist.EffectiveDate;
    var discontinueDate = import.StmtCouponSuppStatusHist.DiscontinueDate;
    var reasonText = import.StmtCouponSuppStatusHist.ReasonText ?? "";
    var createdBy = global.UserId;
    var createdTmst = Now();
    var docTypeToSuppress = import.StmtCouponSuppStatusHist.DocTypeToSuppress;

    CheckValid<StmtCouponSuppStatusHist>("CpaType", cpaType);
    CheckValid<StmtCouponSuppStatusHist>("Type1", type1);
    CheckValid<StmtCouponSuppStatusHist>("DocTypeToSuppress", docTypeToSuppress);
      
    entities.StmtCouponSuppStatusHist.Populated = false;
    Update("CreateStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "collId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTmst);
        db.SetNullableString(command, "obligationFiller", "");
        db.SetNullableString(
          command, "cpaTypeOblig", GetImplicitValue<StmtCouponSuppStatusHist,
          string>("CpaTypeOblig"));
        db.SetString(command, "docTypeToSupp", docTypeToSuppress);
      });

    entities.StmtCouponSuppStatusHist.CpaType = cpaType;
    entities.StmtCouponSuppStatusHist.CspNumber = cspNumber;
    entities.StmtCouponSuppStatusHist.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.StmtCouponSuppStatusHist.Type1 = type1;
    entities.StmtCouponSuppStatusHist.EffectiveDate = effectiveDate;
    entities.StmtCouponSuppStatusHist.DiscontinueDate = discontinueDate;
    entities.StmtCouponSuppStatusHist.ReasonText = reasonText;
    entities.StmtCouponSuppStatusHist.CreatedBy = createdBy;
    entities.StmtCouponSuppStatusHist.CreatedTmst = createdTmst;
    entities.StmtCouponSuppStatusHist.LastUpdatedBy = createdBy;
    entities.StmtCouponSuppStatusHist.LastUpdatedTmst = createdTmst;
    entities.StmtCouponSuppStatusHist.DocTypeToSuppress = docTypeToSuppress;
    entities.StmtCouponSuppStatusHist.Populated = true;
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
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
    }

    private CsePerson csePerson;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CreateAttempts.
    /// </summary>
    [JsonPropertyName("createAttempts")]
    public Common CreateAttempts
    {
      get => createAttempts ??= new();
      set => createAttempts = value;
    }

    private Common createAttempts;
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
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
  }
#endregion
}
