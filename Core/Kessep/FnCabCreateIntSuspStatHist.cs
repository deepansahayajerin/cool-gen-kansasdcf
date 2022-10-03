// Program: FN_CAB_CREATE_INT_SUSP_STAT_HIST, ID: 372164410, model: 746.
// Short name: SWE02096
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_CREATE_INT_SUSP_STAT_HIST.
/// </para>
/// <para>
/// This CAB will Create an occurence of Interest_Suspension_Status_Hist for a 
/// given Obligation
/// </para>
/// </summary>
[Serializable]
public partial class FnCabCreateIntSuspStatHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_CREATE_INT_SUSP_STAT_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabCreateIntSuspStatHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabCreateIntSuspStatHist.
  /// </summary>
  public FnCabCreateIntSuspStatHist(IContext context, Import import,
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
    // ****************************************************************
    // *** 9-15-98  B Adams    Imported current date and timestamp  ***
    // ***		        Combined Read actions; deleted an IF ***
    // ***
    // *** 3/25/99 b adams  READ properties set
    // ****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadObligationTypeObligation())
    {
      if (AsChar(entities.ObligationType.Classification) == AsChar
        (import.HcOtcRecovery.Classification))
      {
        local.InterestSuppStatusHistory.ReasonText =
          "At present Recovery Obligations will be set up with Interest Calculation Suppressed";
          
      }
    }
    else
    {
      ExitState = "FN0000_EXISTING_OB_TYPE_NF_RB";

      return;
    }

    // *****   Deleted IF construct surrounding this CREATE; it was always true
    // *****
    try
    {
      CreateInterestSuppStatusHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_INTEREST_SUPP_CREATE_PVRB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateInterestSuppStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var obgId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var otyId = entities.Obligation.DtyGeneratedId;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var effectiveDate = import.Current.Date;
    var discontinueDate = UseCabSetMaximumDiscontinueDate();
    var reasonText = local.InterestSuppStatusHistory.ReasonText ?? "";
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;

    CheckValid<InterestSuppStatusHistory>("CpaType", cpaType);
    entities.InterestSuppStatusHistory.Populated = false;
    Update("CreateInterestSuppStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otyId", otyId);
        db.SetInt32(command, "collId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
      });

    entities.InterestSuppStatusHistory.ObgId = obgId;
    entities.InterestSuppStatusHistory.CspNumber = cspNumber;
    entities.InterestSuppStatusHistory.CpaType = cpaType;
    entities.InterestSuppStatusHistory.OtyId = otyId;
    entities.InterestSuppStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.InterestSuppStatusHistory.EffectiveDate = effectiveDate;
    entities.InterestSuppStatusHistory.DiscontinueDate = discontinueDate;
    entities.InterestSuppStatusHistory.ReasonText = reasonText;
    entities.InterestSuppStatusHistory.CreatedBy = createdBy;
    entities.InterestSuppStatusHistory.CreatedTmst = createdTmst;
    entities.InterestSuppStatusHistory.LastUpdatedBy = "";
    entities.InterestSuppStatusHistory.LastUpdatedTmst = null;
    entities.InterestSuppStatusHistory.Populated = true;
  }

  private bool ReadObligationTypeObligation()
  {
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadObligationTypeObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 3);
        entities.Obligation.CspNumber = db.GetString(reader, 4);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
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
    /// A value of HcOtcRecovery.
    /// </summary>
    [JsonPropertyName("hcOtcRecovery")]
    public ObligationType HcOtcRecovery
    {
      get => hcOtcRecovery ??= new();
      set => hcOtcRecovery = value;
    }

    /// <summary>
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private ObligationType hcOtcRecovery;
    private CsePersonAccount hcCpaObligor;
    private DateWorkArea current;
    private CsePerson csePerson;
    private ObligationType obligationType;
    private Obligation obligation;
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
    /// A value of InterestSuppStatusHistory.
    /// </summary>
    [JsonPropertyName("interestSuppStatusHistory")]
    public InterestSuppStatusHistory InterestSuppStatusHistory
    {
      get => interestSuppStatusHistory ??= new();
      set => interestSuppStatusHistory = value;
    }

    private InterestSuppStatusHistory interestSuppStatusHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterestSuppStatusHistory.
    /// </summary>
    [JsonPropertyName("interestSuppStatusHistory")]
    public InterestSuppStatusHistory InterestSuppStatusHistory
    {
      get => interestSuppStatusHistory ??= new();
      set => interestSuppStatusHistory = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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

    private InterestSuppStatusHistory interestSuppStatusHistory;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
