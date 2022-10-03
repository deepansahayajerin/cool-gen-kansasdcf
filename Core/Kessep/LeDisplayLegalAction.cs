// Program: LE_DISPLAY_LEGAL_ACTION, ID: 371985167, model: 746.
// Short name: SWE00767
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DISPLAY_LEGAL_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action diagram will display information on the Legal Action screen.
/// </para>
/// </summary>
[Serializable]
public partial class LeDisplayLegalAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DISPLAY_LEGAL_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDisplayLegalAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDisplayLegalAction.
  /// </summary>
  public LeDisplayLegalAction(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------------
    // Date	  Developer    Request#	Description
    // 04/27/95  Dave Allen		Initial Code
    // 03/23/98  Siraj Konkader	ZDEL Cleanup
    // 12/02/98  R.Jean		Eliminated unused logic; currently USEd by
    // 				LE_LACT_LEGAL_ACTION and LE_DELA_DELETE_LEGAL_ACTIONS only.
    // 				These PRADs do not pass TRIBUNAL identifier thus much of
    // 				this CAB's TRIBUNAL logic can never be executed.
    // 02/05/99  P Sharp   		Removed read and view for entity 
    // conv_alt_bill_address.
    // 11/09/00  GVandy	PR92039	Read and export FIPS_Trib_Address for foreign 
    // tribunals.
    // 12/23/02  GVandy	WR10492 Read and export attribute system_gen_ind.
    // 10/28/10  GVandy	CQ109	Eliminate reads for EST legal action service 
    // provider.
    // -------------------------------------------------------------------------------------------
    // ------------------------------------------------------------
    // Move the import legal action to export so that court case no
    // and classification entered are not lost if legal action does not
    // exist
    // ------------------------------------------------------------
    MoveLegalAction(import.LegalAction, export.LegalAction);

    if (import.LegalAction.Identifier > 0)
    {
      if (ReadLegalAction2())
      {
        export.LegalAction.Assign(entities.LegalAction);
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";

        return;
      }
    }
    else if (ReadLegalAction1())
    {
      export.LegalAction.Assign(entities.LegalAction);
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // ------------------------------------------------------------
    // Tribunal.
    // ------------------------------------------------------------
    if (import.Tribunal.Identifier == 0)
    {
      if (ReadTribunalFips())
      {
        export.Tribunal.Assign(entities.Tribunal);
        export.Fips.Assign(entities.Fips);
      }
      else if (ReadTribunal())
      {
        export.Tribunal.Assign(entities.Tribunal);

        if (ReadFipsTribAddress())
        {
          MoveFipsTribAddress(entities.FipsTribAddress, export.FipsTribAddress);
        }
      }
    }

    if (ReadCsePerson())
    {
      export.AltBillingAddrLocn.Number = entities.ExistingAltBillingLocn.Number;
      UseSiReadCsePerson();
    }

    local.Search.ReasonCode = "RSP";
    UseSpCabDetOspAssgndToLact();

    // --- It is not an error if the assignments were not found. Reset any exit 
    // state set by the acblk sp_cab_det_osp_assgnd_to_lact
    ExitState = "ACO_NN0000_ALL_OK";
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveFipsTribAddress(FipsTribAddress source,
    FipsTribAddress target)
  {
    target.Identifier = source.Identifier;
    target.Country = source.Country;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.AltBillingAddrLocn.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.AltBillingAddrLocn);
      
  }

  private void UseSpCabDetOspAssgndToLact()
  {
    var useImport = new SpCabDetOspAssgndToLact.Import();
    var useExport = new SpCabDetOspAssgndToLact.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.Search.ReasonCode = local.Search.ReasonCode;

    Call(SpCabDetOspAssgndToLact.Execute, useImport, useExport);

    MoveOfficeServiceProvider(useExport.OfficeServiceProvider,
      export.OspEnforcingOfficeServiceProvider);
    export.OspEnforcingServiceProvider.Assign(useExport.ServiceProvider);
    MoveOffice(useExport.Office, export.OspEnforcingOffice);
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.ExistingAltBillingLocn.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.LegalAction.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingAltBillingLocn.Number = db.GetString(reader, 0);
        entities.ExistingAltBillingLocn.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetString(
          command, "classification", import.LegalAction.Classification);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.AttorneyApproval = db.GetNullableString(reader, 2);
        entities.LegalAction.ApprovalSentDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.PetitionerApproval =
          db.GetNullableString(reader, 4);
        entities.LegalAction.ApprovalReceivedDate =
          db.GetNullableDate(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.ActionTaken = db.GetString(reader, 7);
        entities.LegalAction.Type1 = db.GetString(reader, 8);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.ForeignOrderRegistrationDate =
          db.GetNullableDate(reader, 10);
        entities.LegalAction.UresaSentDate = db.GetNullableDate(reader, 11);
        entities.LegalAction.UresaAcknowledgedDate =
          db.GetNullableDate(reader, 12);
        entities.LegalAction.UifsaSentDate = db.GetNullableDate(reader, 13);
        entities.LegalAction.UifsaAcknowledgedDate =
          db.GetNullableDate(reader, 14);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 15);
        entities.LegalAction.InitiatingCounty =
          db.GetNullableString(reader, 16);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 17);
        entities.LegalAction.RespondingCounty =
          db.GetNullableString(reader, 18);
        entities.LegalAction.OrderAuthority = db.GetString(reader, 19);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 20);
        entities.LegalAction.RefileDate = db.GetNullableDate(reader, 21);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 22);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 23);
        entities.LegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 24);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 25);
        entities.LegalAction.LongArmStatuteIndicator =
          db.GetNullableString(reader, 26);
        entities.LegalAction.DismissalCode = db.GetNullableString(reader, 27);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 28);
        entities.LegalAction.ForeignFipsState = db.GetNullableInt32(reader, 29);
        entities.LegalAction.ForeignFipsCounty =
          db.GetNullableInt32(reader, 30);
        entities.LegalAction.ForeignFipsLocation =
          db.GetNullableInt32(reader, 31);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 32);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 33);
        entities.LegalAction.NonCsePetitioner =
          db.GetNullableString(reader, 34);
        entities.LegalAction.DateNonCpReqIwoBegin =
          db.GetNullableDate(reader, 35);
        entities.LegalAction.DateCpReqIwoBegin = db.GetNullableDate(reader, 36);
        entities.LegalAction.CtOrdAltBillingAddrInd =
          db.GetNullableString(reader, 37);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 38);
        entities.LegalAction.InitiatingCountry =
          db.GetNullableString(reader, 39);
        entities.LegalAction.RespondingCountry =
          db.GetNullableString(reader, 40);
        entities.LegalAction.KeyChangeDate = db.GetNullableDate(reader, 41);
        entities.LegalAction.SystemGenInd = db.GetNullableString(reader, 42);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.AttorneyApproval = db.GetNullableString(reader, 2);
        entities.LegalAction.ApprovalSentDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.PetitionerApproval =
          db.GetNullableString(reader, 4);
        entities.LegalAction.ApprovalReceivedDate =
          db.GetNullableDate(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.ActionTaken = db.GetString(reader, 7);
        entities.LegalAction.Type1 = db.GetString(reader, 8);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.ForeignOrderRegistrationDate =
          db.GetNullableDate(reader, 10);
        entities.LegalAction.UresaSentDate = db.GetNullableDate(reader, 11);
        entities.LegalAction.UresaAcknowledgedDate =
          db.GetNullableDate(reader, 12);
        entities.LegalAction.UifsaSentDate = db.GetNullableDate(reader, 13);
        entities.LegalAction.UifsaAcknowledgedDate =
          db.GetNullableDate(reader, 14);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 15);
        entities.LegalAction.InitiatingCounty =
          db.GetNullableString(reader, 16);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 17);
        entities.LegalAction.RespondingCounty =
          db.GetNullableString(reader, 18);
        entities.LegalAction.OrderAuthority = db.GetString(reader, 19);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 20);
        entities.LegalAction.RefileDate = db.GetNullableDate(reader, 21);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 22);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 23);
        entities.LegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 24);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 25);
        entities.LegalAction.LongArmStatuteIndicator =
          db.GetNullableString(reader, 26);
        entities.LegalAction.DismissalCode = db.GetNullableString(reader, 27);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 28);
        entities.LegalAction.ForeignFipsState = db.GetNullableInt32(reader, 29);
        entities.LegalAction.ForeignFipsCounty =
          db.GetNullableInt32(reader, 30);
        entities.LegalAction.ForeignFipsLocation =
          db.GetNullableInt32(reader, 31);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 32);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 33);
        entities.LegalAction.NonCsePetitioner =
          db.GetNullableString(reader, 34);
        entities.LegalAction.DateNonCpReqIwoBegin =
          db.GetNullableDate(reader, 35);
        entities.LegalAction.DateCpReqIwoBegin = db.GetNullableDate(reader, 36);
        entities.LegalAction.CtOrdAltBillingAddrInd =
          db.GetNullableString(reader, 37);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 38);
        entities.LegalAction.InitiatingCountry =
          db.GetNullableString(reader, 39);
        entities.LegalAction.RespondingCountry =
          db.GetNullableString(reader, 40);
        entities.LegalAction.KeyChangeDate = db.GetNullableDate(reader, 41);
        entities.LegalAction.SystemGenInd = db.GetNullableString(reader, 42);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunalFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadTribunalFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Fips.County = db.GetInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Fips.State = db.GetInt32(reader, 6);
        entities.Fips.StateDescription = db.GetNullableString(reader, 7);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 8);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 9);
        entities.Fips.StateAbbreviation = db.GetString(reader, 10);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 11);
        entities.Fips.Populated = true;
        entities.Tribunal.Populated = true;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private Tribunal tribunal;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of AltBillingAddrLocn.
    /// </summary>
    [JsonPropertyName("altBillingAddrLocn")]
    public CsePersonsWorkSet AltBillingAddrLocn
    {
      get => altBillingAddrLocn ??= new();
      set => altBillingAddrLocn = value;
    }

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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of OspEnforcingOffice.
    /// </summary>
    [JsonPropertyName("ospEnforcingOffice")]
    public Office OspEnforcingOffice
    {
      get => ospEnforcingOffice ??= new();
      set => ospEnforcingOffice = value;
    }

    /// <summary>
    /// A value of OspEnforcingServiceProvider.
    /// </summary>
    [JsonPropertyName("ospEnforcingServiceProvider")]
    public ServiceProvider OspEnforcingServiceProvider
    {
      get => ospEnforcingServiceProvider ??= new();
      set => ospEnforcingServiceProvider = value;
    }

    /// <summary>
    /// A value of OspEnforcingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("ospEnforcingOfficeServiceProvider")]
    public OfficeServiceProvider OspEnforcingOfficeServiceProvider
    {
      get => ospEnforcingOfficeServiceProvider ??= new();
      set => ospEnforcingOfficeServiceProvider = value;
    }

    private FipsTribAddress fipsTribAddress;
    private CsePersonsWorkSet altBillingAddrLocn;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private Fips fips;
    private Office ospEnforcingOffice;
    private ServiceProvider ospEnforcingServiceProvider;
    private OfficeServiceProvider ospEnforcingOfficeServiceProvider;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public LegalActionAssigment Search
    {
      get => search ??= new();
      set => search = value;
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

    private LegalActionAssigment search;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of ExistingAltBillingLocn.
    /// </summary>
    [JsonPropertyName("existingAltBillingLocn")]
    public CsePerson ExistingAltBillingLocn
    {
      get => existingAltBillingLocn ??= new();
      set => existingAltBillingLocn = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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

    private FipsTribAddress fipsTribAddress;
    private CsePerson existingAltBillingLocn;
    private CsePerson csePerson;
    private Fips fips;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private LegalActionPerson legalActionPerson;
    private CaseRole caseRole;
  }
#endregion
}
