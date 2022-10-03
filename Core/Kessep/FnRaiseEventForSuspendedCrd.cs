// Program: FN_RAISE_EVENT_FOR_SUSPENDED_CRD, ID: 372279917, model: 746.
// Short name: SWE02280
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RAISE_EVENT_FOR_SUSPENDED_CRD.
/// </summary>
[Serializable]
public partial class FnRaiseEventForSuspendedCrd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RAISE_EVENT_FOR_SUSPENDED_CRD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRaiseEventForSuspendedCrd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRaiseEventForSuspendedCrd.
  /// </summary>
  public FnRaiseEventForSuspendedCrd(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------
    //                        M A I N T E N A N C E        L O G
    // ---------------------------------------------------------------------------------------
    //   Date    Developer     WR #/PR #    Description
    // ---------------------------------------------------------------------------------------
    // 10/06/99  swsrgad                    Initial development
    // 03/22/01  swsrchf       I00115250    Added check for MAX date ('2099-12-
    // 31') to
    //                                      
    // SUMMARIZE statements.
    // 05/06/05  GVandy	PR243130     Raise event for FUTURE reason code.
    // 07/02/2010  GVandy	CQ20439	     Performance change required due to 
    // optimizer suddenly choosing new index path.
    // ---------------------------------------------------------------------------------------
    switch(TrimEnd(import.CashReceiptDetailStatHistory.ReasonCodeId ?? ""))
    {
      case "NODEBTPERS":
        break;
      case "DEBTPIF":
        break;
      case "NODEBTCTOR":
        break;
      case "NODEBTOPMT":
        break;
      case "NODEBTREIP":
        break;
      case "OPMTREFUND":
        break;
      case "OPMTERROR":
        break;
      case "FUTURE":
        break;
      default:
        return;
    }

    // *** Problem report I00115250
    // *** 03/22/01 swsrchf
    // *** Set MAX date to '2099-12-31'
    local.Max.Date = new DateTime(2099, 12, 31);
    UseFnAbConcatCrAndCrd();
    local.Infrastructure.Detail = "SUSP $$ CRD: " + local
      .CrdCrComboNo.CrdCrCombo + ", PRSN: " + (
        import.CashReceiptDetail.ObligorPersonNumber ?? "");
    local.Infrastructure.ReasonCode =
      import.CashReceiptDetailStatHistory.ReasonCodeId ?? Spaces(15);
    local.Infrastructure.InitiatingStateCode = "KS";
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = global.UserId;
    local.Infrastructure.BusinessObjectCd = "OBL";
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.CreatedTimestamp = Now();
    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.EventId = 9;

    if (IsEmpty(import.CashReceiptDetail.CourtOrderNumber))
    {
      // *** Problem report I00115250
      // *** 03/22/01 swsrchf
      // *** Added check for MAX date to SUMMARIZE statement
      ReadServiceProvider2();

      if (local.Common.Count > 1)
      {
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + ", M-OSP";
          
      }

      local.Common.Count = 0;

      foreach(var item in ReadCaseCaseUnit2())
      {
        local.Infrastructure.CsePersonNumber =
          import.CashReceiptDetail.ObligorPersonNumber ?? "";
        local.Infrastructure.CaseNumber = entities.ExistingCase.Number;
        local.Infrastructure.CaseUnitNumber =
          entities.ExistingCaseUnit.CuNumber;

        if (ReadInterstateRequest())
        {
          if (AsChar(entities.ExistingInterstateRequest.KsCaseInd) != 'Y')
          {
            local.Infrastructure.InitiatingStateCode = "OS";
          }
        }
        else
        {
          // : Continue Processing - It is a Kansas Case.
        }

        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

          return;
        }

        ++local.Common.Count;
      }
    }
    else
    {
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + ", CT OR: " +
        (import.CashReceiptDetail.CourtOrderNumber ?? "");

      // *** Problem report I00115250
      // *** 03/22/01 swsrchf
      // *** Added check for MAX date to SUMMARIZE statement
      // 07/02/2010  GVandy CQ20439  Performance change required due to 
      // optimizer suddenly choosing new index path.  Changed from
      // "AND THAT existing office_service_provider discontinue_date IS EQUAL TO
      // local_max date_work_area date"
      // to
      // "AND THAT existing office_service_provider discontinue_date IS GREATER 
      // OR EQUAL TO local_max date_work_area date
      //  AND THAT existing office_service_provider discontinue_date IS LESS OR 
      // EQUAL TO local_max date_work_area date"
      ReadServiceProvider1();

      if (local.Common.Count > 1)
      {
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + ", M-OSP";
          
      }

      local.Common.Count = 0;

      foreach(var item in ReadCaseCaseUnit1())
      {
        local.Infrastructure.CsePersonNumber =
          import.CashReceiptDetail.ObligorPersonNumber ?? "";
        local.Infrastructure.CaseNumber = entities.ExistingCase.Number;
        local.Infrastructure.CaseUnitNumber =
          entities.ExistingCaseUnit.CuNumber;

        if (ReadInterstateRequest())
        {
          if (AsChar(entities.ExistingInterstateRequest.KsCaseInd) != 'Y')
          {
            local.Infrastructure.InitiatingStateCode = "OS";
          }
        }
        else
        {
          // : Continue Processing - It is a Kansas Case.
        }

        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

          return;
        }

        ++local.Common.Count;
      }
    }

    if (local.Common.Count == 0)
    {
      ExitState = "CASE_UNIT_NF_RB";
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    local.CrdCrComboNo.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private IEnumerable<bool> ReadCaseCaseUnit1()
  {
    entities.ExistingCase.Populated = false;
    entities.ExistingCaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", import.CashReceiptDetail.ObligorPersonNumber ?? ""
          );
        db.SetNullableString(
          command, "standardNo", import.CashReceiptDetail.CourtOrderNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.ExistingCaseUnit.StartDate = db.GetDate(reader, 3);
        entities.ExistingCaseUnit.ClosureDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.ExistingCase.Populated = true;
        entities.ExistingCaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseUnit2()
  {
    entities.ExistingCase.Populated = false;
    entities.ExistingCaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", import.CashReceiptDetail.ObligorPersonNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.ExistingCaseUnit.StartDate = db.GetDate(reader, 3);
        entities.ExistingCaseUnit.ClosureDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.ExistingCase.Populated = true;
        entities.ExistingCaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.ExistingInterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.
          SetNullableString(command, "casINumber", entities.ExistingCase.Number);
          
      },
      (db, reader) =>
      {
        entities.ExistingInterstateRequest.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingInterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.ExistingInterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.ExistingInterstateRequest.CasINumber =
          db.GetNullableString(reader, 3);
        entities.ExistingInterstateRequest.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetDate(command, "date", local.Max.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          import.CashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetNullableString(
          command, "standardNo", import.CashReceiptDetail.CourtOrderNumber ?? ""
          );
      },
      (db, reader) =>
      {
        local.Common.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadServiceProvider2()
  {
    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          import.CashReceiptDetail.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        local.Common.Count = db.GetInt32(reader, 0);
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of DelMe1.
    /// </summary>
    [JsonPropertyName("delMe1")]
    public TextWorkArea DelMe1
    {
      get => delMe1 ??= new();
      set => delMe1 = value;
    }

    /// <summary>
    /// A value of DelMe2.
    /// </summary>
    [JsonPropertyName("delMe2")]
    public DateWorkArea DelMe2
    {
      get => delMe2 ??= new();
      set => delMe2 = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private TextWorkArea delMe1;
    private DateWorkArea delMe2;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
    }

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
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public DateWorkArea DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private DateWorkArea max;
    private Infrastructure infrastructure;
    private CrdCrComboNo crdCrComboNo;
    private Common common;
    private DateWorkArea delMe;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("existingLegalActionAssigment")]
    public LegalActionAssigment ExistingLegalActionAssigment
    {
      get => existingLegalActionAssigment ??= new();
      set => existingLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of ExistingAbsentParent.
    /// </summary>
    [JsonPropertyName("existingAbsentParent")]
    public CaseRole ExistingAbsentParent
    {
      get => existingAbsentParent ??= new();
      set => existingAbsentParent = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("existingLegalActionCaseRole")]
    public LegalActionCaseRole ExistingLegalActionCaseRole
    {
      get => existingLegalActionCaseRole ??= new();
      set => existingLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePerson ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCaseUnit.
    /// </summary>
    [JsonPropertyName("existingCaseUnit")]
    public CaseUnit ExistingCaseUnit
    {
      get => existingCaseUnit ??= new();
      set => existingCaseUnit = value;
    }

    /// <summary>
    /// A value of ExistingInterstateRequest.
    /// </summary>
    [JsonPropertyName("existingInterstateRequest")]
    public InterstateRequest ExistingInterstateRequest
    {
      get => existingInterstateRequest ??= new();
      set => existingInterstateRequest = value;
    }

    /// <summary>
    /// A value of DelMeAbsentParent.
    /// </summary>
    [JsonPropertyName("delMeAbsentParent")]
    public CaseRole DelMeAbsentParent
    {
      get => delMeAbsentParent ??= new();
      set => delMeAbsentParent = value;
    }

    /// <summary>
    /// A value of DelMeCaseRole.
    /// </summary>
    [JsonPropertyName("delMeCaseRole")]
    public CaseRole DelMeCaseRole
    {
      get => delMeCaseRole ??= new();
      set => delMeCaseRole = value;
    }

    /// <summary>
    /// A value of DelMeCsePerson.
    /// </summary>
    [JsonPropertyName("delMeCsePerson")]
    public CsePerson DelMeCsePerson
    {
      get => delMeCsePerson ??= new();
      set => delMeCsePerson = value;
    }

    private CaseAssignment existingCaseAssignment;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private LegalActionAssigment existingLegalActionAssigment;
    private CaseRole existingAbsentParent;
    private LegalActionCaseRole existingLegalActionCaseRole;
    private LegalAction existingLegalAction;
    private CsePerson existingObligor;
    private Case1 existingCase;
    private CaseUnit existingCaseUnit;
    private InterstateRequest existingInterstateRequest;
    private CaseRole delMeAbsentParent;
    private CaseRole delMeCaseRole;
    private CsePerson delMeCsePerson;
  }
#endregion
}
