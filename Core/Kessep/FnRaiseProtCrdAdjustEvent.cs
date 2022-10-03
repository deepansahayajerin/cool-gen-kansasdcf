// Program: FN_RAISE_PROT_CRD_ADJUST_EVENT, ID: 373378807, model: 746.
// Short name: SWE01529
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RAISE_PROT_CRD_ADJUST_EVENT.
/// </summary>
[Serializable]
public partial class FnRaiseProtCrdAdjustEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RAISE_PROT_CRD_ADJUST_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRaiseProtCrdAdjustEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRaiseProtCrdAdjustEvent.
  /// </summary>
  public FnRaiseProtCrdAdjustEvent(IContext context, Import import,
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
    // ***************************************************
    // * * * *   INITIAL DEVELOPMENT  * * * *
    // 01/08/02        SWSRPDP         WR010504-A - Retro Processing
    // Create an EVENT if "P"rotected Collection is Adjusted
    // ***************************************************
    // * *   Verify we have an AP Number
    local.Selected.CourtOrderNumber = "";

    if (!IsEmpty(import.Collection.CourtOrderAppliedTo))
    {
      local.Selected.CourtOrderNumber =
        import.Collection.CourtOrderAppliedTo ?? "";
    }
    else if (!IsEmpty(import.CashReceiptDetail.CourtOrderNumber))
    {
      local.Selected.CourtOrderNumber =
        import.CashReceiptDetail.CourtOrderNumber ?? "";
    }

    // * *   Verify we have an AP Number
    if (!IsEmpty(import.CashReceiptDetail.ObligorPersonNumber))
    {
      local.CsePersonsWorkSet.Number =
        import.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
    }
    else
    {
      if (IsEmpty(import.CashReceiptDetail.ObligorSocialSecurityNumber))
      {
        // * *   NO AP Number - Do we have a Court Order Number?
        if (!IsEmpty(local.Selected.CourtOrderNumber))
        {
          goto Test;
        }

        // * * NO AP Information - Do Not Create EVENT
        ExitState = "CSE_PERSON_NF";

        return;
      }

      local.CsePersonsWorkSet.Ssn =
        import.CashReceiptDetail.ObligorSocialSecurityNumber ?? Spaces(9);
      UseEabReadCsePersonUsingSsn();

      if (!IsEmpty(local.CsePersonsWorkSet.Number))
      {
      }
      else
      {
        // * *   NO AP Number - Do we have a Court Order Number?
        if (!IsEmpty(local.Selected.CourtOrderNumber))
        {
          goto Test;
        }

        // * * NO AP Information - Do Not Create EVENT
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

Test:

    // * * * * * * * * * * * * * * * *
    // * * Set up the information to create the INFRASTRUCTURE Record
    // * * * * * * * * * * * * * * * *
    // local_pass infrastructure  function   ----    SET in CAB
    // local_pass infrastructure  system_generated_identifier   ----    SET in 
    // CAB
    local.Pass.SituationNumber = 0;
    local.Pass.ProcessStatus = "Q";
    local.Pass.EventId = 47;

    // local_pass infrastructure  event_type   ----    SET in CAB
    // local_pass infrastructure  event_detail_name   ----    SET in CAB
    local.Pass.ReasonCode = "COLLPROTADJ";
    local.Pass.BusinessObjectCd = "OBL";

    // denorm_numeric_12
    local.Pass.DenormText12 = "";

    // denorm_date
    // denorm_timestamp
    local.Pass.InitiatingStateCode = "KS";
    local.Pass.CsenetInOutCode = "";

    // CASE_NUMBER   SET BELOW IN READ_EACH
    local.Pass.CsePersonNumber = local.CsePersonsWorkSet.Number;
    local.Pass.CaseUnitNumber = 0;
    local.Pass.UserId = global.UserId;
    local.Pass.LastUpdatedBy = "";
    local.Pass.LastUpdatedBy = "";

    // last_updated_timestamp
    // reference_date
    // * * * * * * * * * * * * * * * *
    // * * CREATE an INFRASTRUCTURE record for EACH Open Case the person is an "
    // AP" on
    // * * * * * * * * * * * * * * * *
    // * * SORTED BY will be used to Prevent Duplicate Infrastructure Records
    // * *   IF Multiple case_unit's
    // * * * * * * * * * * * * * * * *
    ExitState = "ACO_NN0000_ALL_OK";
    local.Existing.Number = "";
    local.EventCreated.Flag = "N";

    if (!IsEmpty(import.CashReceiptDetail.CourtOrderNumber) && !
      IsEmpty(local.CsePersonsWorkSet.Number))
    {
      // * * * * * * * * * * * * * * * *
      // * *
      // * * We have BOTH the AP Number and the Court_Order_Number
      // * *
      // * * * * * * * * * * * * * * * *
      foreach(var item in ReadCaseCaseUnit1())
      {
        // * * * * * * * * * * * * * * * *
        // * * Only ONE Alert per Case
        // * * * * * * * * * * * * * * * *
        if (Equal(entities.ExistingCase.Number, local.Existing.Number))
        {
          continue;
        }

        local.Pass.CaseNumber = entities.ExistingCase.Number;
        UseSpCabCreateInfrastructure();

        // * * * * * * * * * * * * * * * *
        // * * If the CREATE was NOT succesfull - SET Error and EXIT
        // * * * * * * * * * * * * * * * *
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

          return;
        }

        local.Existing.Number = entities.ExistingCase.Number;
        local.EventCreated.Flag = "Y";
      }

      if (AsChar(local.EventCreated.Flag) == 'N')
      {
        ExitState = "CASE_NF";
      }
    }
    else if (!IsEmpty(local.Selected.CourtOrderNumber))
    {
      // * * * * * * * * * * * * * * * *
      // * *
      // * * We have the Court_Order_Number BUT NOT the AP Number
      // * *
      // * * * * * * * * * * * * * * * *
      foreach(var item in ReadCaseCaseUnit2())
      {
        // * * * * * * * * * * * * * * * *
        // * * Only ONE Alert per Case
        // * * * * * * * * * * * * * * * *
        if (Equal(entities.ExistingCase.Number, local.Existing.Number))
        {
          continue;
        }

        local.Pass.CaseNumber = entities.ExistingCase.Number;
        UseSpCabCreateInfrastructure();

        // * * * * * * * * * * * * * * * *
        // * * If the CREATE was NOT succesfull - SET Error and EXIT
        // * * * * * * * * * * * * * * * *
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

          return;
        }

        local.Existing.Number = entities.ExistingCase.Number;
        local.EventCreated.Flag = "Y";
      }

      if (AsChar(local.EventCreated.Flag) == 'N')
      {
        ExitState = "CASE_NF";
      }
    }
    else
    {
      // * * * * * * * * * * * * * * * *
      // * *
      // * * We have the AP Number BUT NOT the Court_Order_Number
      // * *
      // * * * * * * * * * * * * * * * *
      // * * * * * * * * * * * * * * * *
      // * * Find ALL Cases this Person is AP on
      // * * * * * * * * * * * * * * * *
      foreach(var item in ReadCaseCaseUnit3())
      {
        // * * * * * * * * * * * * * * * *
        // * * Only ONE Alert per Case
        // * * * * * * * * * * * * * * * *
        if (Equal(entities.ExistingCase.Number, local.Existing.Number))
        {
          continue;
        }

        // * * * * * * * * * * * * * * * *
        // * * Do we have AR Information?
        // * *  If NOT - BYPASS this IF - Create Infrastructure for EACH Case 
        // the AP is on
        // * * * * * * * * * * * * * * * *
        if (!Equal(import.Collection.ArNumber, "0000000000") && !
          IsEmpty(import.Collection.ArNumber))
        {
          // * * If AR Information is Availiable - Make sure AP and AR are on 
          // this Case
          if (ReadCaseRole())
          {
            // * * AR/AP/CASE Match Found - - CONTINUE
          }
          else
          {
            // * * AR Information is NOT for this Case - - Try the NEXT Case
            continue;
          }
        }

        local.Pass.CaseNumber = entities.ExistingCase.Number;

        // * * * * * * * * * * * * * * * *
        // * * CREATE the Infrastructure Record
        // * * * * * * * * * * * * * * * *
        UseSpCabCreateInfrastructure();

        // * * * * * * * * * * * * * * * *
        // * * If the CREATE was NOT succesfull - SET Error and EXIT
        // * * * * * * * * * * * * * * * *
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

          return;
        }

        local.EventCreated.Flag = "Y";
        local.Existing.Number = entities.ExistingCase.Number;
      }

      if (AsChar(local.EventCreated.Flag) == 'N')
      {
        ExitState = "CASE_NF";
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Pass);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCaseCaseUnit1()
  {
    entities.ExistingCase.Populated = false;
    entities.ExistingCaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit1",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNoAp", local.CsePersonsWorkSet.Number);
          
        db.SetNullableString(
          command, "standardNo", local.Selected.CourtOrderNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 3);
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
          command, "standardNo", local.Selected.CourtOrderNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.ExistingCase.Populated = true;
        entities.ExistingCaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseUnit3()
  {
    entities.ExistingCase.Populated = false;
    entities.ExistingCaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit3",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNoAp", local.CsePersonsWorkSet.Number);
          
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.ExistingCase.Populated = true;
        entities.ExistingCaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "cspNumber", import.Collection.ArNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.Populated = true;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

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

    private Collection collection;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CashReceiptDetail Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private CashReceiptDetail selected;
    private Common eventCreated;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Infrastructure pass;
    private Case1 existing;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of CaseAr.
    /// </summary>
    [JsonPropertyName("caseAr")]
    public CsePerson CaseAr
    {
      get => caseAr ??= new();
      set => caseAr = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePerson ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole existingAbsentParent;
    private CsePerson caseAr;
    private CaseRole caseRole;
    private LegalAction legalAction;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private Case1 existingCase;
    private CaseUnit existingCaseUnit;
    private CsePerson existingObligor;
  }
#endregion
}
