// Program: FN_REPORT_CASE_OBLIG_STATUS, ID: 372740997, model: 746.
// Short name: SWE02858
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REPORT_CASE_OBLIG_STATUS.
/// </summary>
[Serializable]
public partial class FnReportCaseObligStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REPORT_CASE_OBLIG_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReportCaseObligStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReportCaseObligStatus.
  /// </summary>
  public FnReportCaseObligStatus(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------
    // : IF ACTIVE
    //   THEN
    //     IF CASE LAST_EVENT_SENT = SPACES OR
    //        CASE LAST_EVENT_SENT = 'A'
    //       CONTINUE
    //     ELSE
    //       SEND 'REVERSED' EVENT
    //     END-IF
    //   ELSE (DEACTIVED)
    //     IF CASE LAST_EVENT_SENT = SPACES OR
    //        CASE LAST_EVENT_SENT = 'A'
    //       SEND 'PIF' EVENT
    //     ELSE
    //       CONTINUE
    //     END-IF
    //   END-IF
    // ----------------------------
    local.Vol.SystemGeneratedIdentifier = 16;

    if (ReadDebtDetail())
    {
      // ---------
      // Active.
      // ---------
      export.ActiveDeactive.Flag = "A";
    }
    else if (ReadAccrualInstructions())
    {
      // ---------
      // Active.
      // ---------
      export.ActiveDeactive.Flag = "A";
    }
    else
    {
      // --------------------
      // : Deactivated
      // --------------------
      export.ActiveDeactive.Flag = "D";
    }

    // -------------------------------------------------------------------
    // This READ is to find the legal_action associated to case. The 
    // Legal_action Standard_Number will be concatenated with appropriate
    // message and set to the Infrastructure  'DETAIL' field.
    // -----------------------------------------------------------------
    ReadLegalAction();

    if (AsChar(export.ActiveDeactive.Flag) == 'A')
    {
      if (AsChar(import.Case1.LastCaseEvent) == 'A')
      {
        local.NoChange.Flag = "Y";
      }
      else
      {
        // -----
        // Activated (Change in case status). Need to raise an event.
        // -----
        if (IsEmpty(import.Case1.LastCaseEvent))
        {
          local.NoChange.Flag = "N";
          export.ActivatedForFirstTime.Flag = "Y";
          local.Pass.ReasonCode = "FNALLOBGPA";
          local.Pass.Detail = "Obligation Activated for " + entities
            .LegalAction.StandardNumber;
        }
        else
        {
          local.NoChange.Flag = "N";
          local.Pass.ReasonCode = "FNALLOBGPDRCT";
          local.Pass.Detail =
            "Obligation reactivated due to adjustment for " + entities
            .LegalAction.StandardNumber;
        }
      }
    }
    else if (AsChar(import.Case1.LastCaseEvent) == 'D' || IsEmpty
      (import.Case1.LastCaseEvent))
    {
      local.NoChange.Flag = "Y";
    }
    else
    {
      // -------
      // Deactivated (Change in case status). Need to raise an event.
      // -------
      if (AsChar(import.FeeRecoveryObFound.Flag) == 'Y' && AsChar
        (import.ObligationFound.Flag) == 'N')
      {
        // -------------------------------------------------------------------------
        // Per WR# 243, if only Fee and Recovery type obligations associted to 
        // legal_action found, no need to raise the event when they are paid
        // off.But update the last_case_event to D.
        // -------------------------------------------------------------------------
        return;
      }

      local.NoChange.Flag = "N";
      local.Pass.ReasonCode = "FNALLOBGPD";
      local.Pass.Detail = "All Obligations for this CASE are satisfied for " + entities
        .LegalAction.StandardNumber;
    }

    if (AsChar(local.NoChange.Flag) == 'Y')
    {
    }
    else
    {
      // ------------------------------
      // Check if case is intertstate or not.
      // ------------------------------
      local.Pass.InitiatingStateCode = "KS";

      if (ReadInterstateRequest())
      {
        local.Pass.InitiatingStateCode = "OS";
      }

      // ------------------------------
      // Set the infrastucture parameters before calling the cab to create the 
      // infrastructure.
      // ------------------------------
      local.Pass.EventId = 7;
      local.Pass.BusinessObjectCd = "CAS";
      local.Pass.CaseNumber = import.Case1.Number;
      local.Pass.CsePersonNumber = import.Obligor.Number;
      local.Pass.UserId = "OPAY";
      local.Pass.ProcessStatus = "Q";
      local.Pass.ReferenceDate = import.ProgramProcessingInfo.ProcessDate;
      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        MoveInfrastructure2(local.Pass, export.Error);
      }
      else
      {
        export.EventCreated.Flag = "Y";
      }
    }
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure1(local.Pass, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private bool ReadAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadDebtDetail()
  {
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.SetInt32(
          command, "dtyGeneratedId", local.Vol.SystemGeneratedIdentifier);
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    /// <summary>
    /// A value of ObligationFound.
    /// </summary>
    [JsonPropertyName("obligationFound")]
    public Common ObligationFound
    {
      get => obligationFound ??= new();
      set => obligationFound = value;
    }

    /// <summary>
    /// A value of FeeRecoveryObFound.
    /// </summary>
    [JsonPropertyName("feeRecoveryObFound")]
    public Common FeeRecoveryObFound
    {
      get => feeRecoveryObFound ??= new();
      set => feeRecoveryObFound = value;
    }

    private CsePerson obligor;
    private Case1 case1;
    private ProgramProcessingInfo programProcessingInfo;
    private Common obligationFound;
    private Common feeRecoveryObFound;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Infrastructure Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of ActivatedForFirstTime.
    /// </summary>
    [JsonPropertyName("activatedForFirstTime")]
    public Common ActivatedForFirstTime
    {
      get => activatedForFirstTime ??= new();
      set => activatedForFirstTime = value;
    }

    /// <summary>
    /// A value of EventCreated.
    /// </summary>
    [JsonPropertyName("eventCreated")]
    public Common EventCreated
    {
      get => eventCreated ??= new();
      set => eventCreated = value;
    }

    /// <summary>
    /// A value of ActiveDeactive.
    /// </summary>
    [JsonPropertyName("activeDeactive")]
    public Common ActiveDeactive
    {
      get => activeDeactive ??= new();
      set => activeDeactive = value;
    }

    private Infrastructure error;
    private Common activatedForFirstTime;
    private Common eventCreated;
    private Common activeDeactive;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Vol.
    /// </summary>
    [JsonPropertyName("vol")]
    public ObligationType Vol
    {
      get => vol ??= new();
      set => vol = value;
    }

    /// <summary>
    /// A value of NoChange.
    /// </summary>
    [JsonPropertyName("noChange")]
    public Common NoChange
    {
      get => noChange ??= new();
      set => noChange = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Infrastructure Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of ActiveOrDeactive.
    /// </summary>
    [JsonPropertyName("activeOrDeactive")]
    public Common ActiveOrDeactive
    {
      get => activeOrDeactive ??= new();
      set => activeOrDeactive = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private ObligationType vol;
    private Common noChange;
    private Infrastructure pass;
    private Common activeOrDeactive;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private ObligationTransaction obligationTransaction;
    private CsePersonAccount csePersonAccount;
    private CaseRole caseRole;
    private InterstateRequest interstateRequest;
    private ObligationType obligationType;
    private Case1 case1;
    private CsePerson csePerson;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private AccrualInstructions accrualInstructions;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
