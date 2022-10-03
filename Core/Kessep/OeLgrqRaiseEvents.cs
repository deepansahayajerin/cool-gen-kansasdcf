// Program: OE_LGRQ_RAISE_EVENTS, ID: 371913184, model: 746.
// Short name: SWE01893
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_LGRQ_RAISE_EVENTS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// </para>
/// </summary>
[Serializable]
public partial class OeLgrqRaiseEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LGRQ_RAISE_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLgrqRaiseEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLgrqRaiseEvents.
  /// </summary>
  public OeLgrqRaiseEvents(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Change log
    // date		author	remarks
    // 12/17/96	Sid 	initial creation
    // 01/30/97	Raju	modifications
    // --------------------------------------------
    MoveInfrastructure(import.Infrastructure, local.Infrastructure);

    // --------------------------------------------
    // Assigning global infrastructure attribute
    //   values
    // --------------------------------------------
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.LastUpdatedBy = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.BusinessObjectCd = "LRF";
    local.Infrastructure.UserId = "LGRQ";

    if (ReadCase())
    {
      local.Infrastructure.CaseNumber = entities.ExistingCase.Number;

      if (ReadInterstateRequest())
      {
        if (AsChar(entities.ExistingInterstateRequest.KsCaseInd) == 'Y')
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "OS";
        }
      }
      else
      {
        local.Infrastructure.InitiatingStateCode = "KS";
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!IsEmpty(import.Ap.Number))
    {
      if (ReadCsePerson2())
      {
        local.Infrastructure.CsePersonNumber = entities.ExistingAp.Number;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }
    else
    {
      // These statements were added by Carl Galka on 12/22/1999. This returns 
      // the AP when the AP is not passed to the routing. It is not passed to
      // the routine when we come from ASIN.
      local.LegalReferral.Identifier =
        (int)import.Infrastructure.DenormNumeric12.GetValueOrDefault();

      if (ReadCsePerson1())
      {
        local.Infrastructure.CsePersonNumber = entities.ExistingAp.Number;
      }
      else
      {
        // These statements were added by Carl Galka on 12/22/1999.
        // 
        // There is not an AP associated to this Legal Referral.
      }
    }

    UseSpCabCreateInfrastructure();
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingAp.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", local.LegalReferral.Identifier);
        db.SetString(command, "casNumberRole", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAp.Number = db.GetString(reader, 0);
        entities.ExistingAp.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingAp.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAp.Number = db.GetString(reader, 0);
        entities.ExistingAp.Populated = true;
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
        entities.ExistingInterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.ExistingInterstateRequest.CasINumber =
          db.GetNullableString(reader, 2);
        entities.ExistingInterstateRequest.Populated = true;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private CsePersonsWorkSet ap;
    private Case1 case1;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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

    private LegalReferral legalReferral;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalReferral.
    /// </summary>
    [JsonPropertyName("existingLegalReferral")]
    public LegalReferral ExistingLegalReferral
    {
      get => existingLegalReferral ??= new();
      set => existingLegalReferral = value;
    }

    /// <summary>
    /// A value of ExistingLegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("existingLegalReferralCaseRole")]
    public LegalReferralCaseRole ExistingLegalReferralCaseRole
    {
      get => existingLegalReferralCaseRole ??= new();
      set => existingLegalReferralCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public CsePerson ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
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
    /// A value of ExistingInterstateRequest.
    /// </summary>
    [JsonPropertyName("existingInterstateRequest")]
    public InterstateRequest ExistingInterstateRequest
    {
      get => existingInterstateRequest ??= new();
      set => existingInterstateRequest = value;
    }

    private LegalReferral existingLegalReferral;
    private LegalReferralCaseRole existingLegalReferralCaseRole;
    private CaseRole existingCaseRole;
    private CsePerson existingAp;
    private Case1 existingCase;
    private InterstateRequest existingInterstateRequest;
  }
#endregion
}
