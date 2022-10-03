// Program: OE_VALIDATE_LICENSE_SUSPENSION, ID: 371080104, model: 746.
// Short name: SWE02001
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_VALIDATE_LICENSE_SUSPENSION.
/// </para>
/// <para>
/// Determines which CSE members qualify for their record to be submitted to the
/// cooperating government agency (currently KDHE 2/2001).
/// Parameters set forth in this program are having an FDSO certification of at 
/// least $10,000.
/// </para>
/// </summary>
[Serializable]
public partial class OeValidateLicenseSuspension: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_VALIDATE_LICENSE_SUSPENSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeValidateLicenseSuspension(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeValidateLicenseSuspension.
  /// </summary>
  public OeValidateLicenseSuspension(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // ----------------------------------------
    // 03/??/2001  SWSRPRM	WR # 291 	Initial Coding
    // 10/11/2005  GVandy	?????		Performance Improvements.
    // 03/05/2007  GVandy	PR261671	Re-written to improve performance.
    // -----------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------
    // The export_lic_susp_qualification flag is returned as
    //     Y - the person qualifies for license suspension
    //     1 - document has already been sent to person
    //     2 - the person is not FDSO certified
    //     3 - the person is decertified for FDSO
    //     4 - the FDSO balance due is less than the threshold amount
    //     5 - the person has made a payment within the last 90 days
    // -------------------------------------------------------------------------------------------
    export.LicSuspQualification.Flag = "";

    // --------------------------------------------------------------------------------------
    // Do not re-send person to KDHE if initial document advising of possibility
    // of intent to
    // suspend license has already been sent to the AP.  This is signified by 
    // license_suspension_ind = 'Y'
    // --------------------------------------------------------------------------------------
    local.LocateRequest.AgencyNumber = "00001";

    if (ReadLocateRequest())
    {
      if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'Y')
      {
        export.LicSuspQualification.Flag = "1";

        return;
      }
    }

    // --------------------------------------------------------------------------------------
    // To qualify for License Suspension the person must be FDSO certified.
    // --------------------------------------------------------------------------------------
    if (ReadAdministrativeActCertification())
    {
      // -- Person does not qualify for license suspension if they have been 
      // decertified from FDSO.
      if (entities.AdministrativeActCertification.DecertifiedDate != null || entities
        .AdministrativeActCertification.AmountOwed == 0)
      {
        export.LicSuspQualification.Flag = "3";

        return;
      }

      // -- Person does not qualify for license suspension if the current amount
      // owed is less than our threshold amount.
      if (entities.AdministrativeActCertification.AmountOwed < import
        .LicSuspThreshold.TotalCurrency)
      {
        export.LicSuspQualification.Flag = "4";

        return;
      }
    }

    if (!entities.AdministrativeActCertification.Populated)
    {
      export.LicSuspQualification.Flag = "2";

      return;
    }

    // ----------------------------------------------------------
    // Besides qualifying under FDSO parameters member has to
    // have had no payments w/in the last 90 days excluding the
    // following payment types:
    // F (collection type id=3)  =>  Federal offset
    // K (collection type id=10) =>  KPERS (KS retirement) offset
    // S (collection type id=4)  =>  State offset
    // T (collection type id=19) =>  Treasury offset - salary
    // Y (collection type id=25) =>  Treasury offset - retirement
    // Z (collection type id=26) =>  Treasury offset - vendor
    // U (collection type id=5)  =>  Unemployment offset
    // ----------------------------------------------------------
    local.Local90DaysAgo.ProcessDate =
      AddDays(import.ProgramProcessingInfo.ProcessDate, -90);

    // -----------------------------------------------------------
    // Looking for a payment (collection) made within last 90 days
    // -----------------------------------------------------------
    export.LicSuspQualification.Flag = "Y";

    if (ReadCashReceiptDetail())
    {
      export.LicSuspQualification.Flag = "5";
    }
  }

  private bool ReadAdministrativeActCertification()
  {
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 4);
        entities.AdministrativeActCertification.AmountOwed =
          db.GetInt32(reader, 5);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 6);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdministrativeActCertification.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetDate(
          command, "collectionDate",
          local.Local90DaysAgo.ProcessDate.GetValueOrDefault());
        db.SetString(
          command, "cpaType", entities.AdministrativeActCertification.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.AdministrativeActCertification.CspNumber);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 4);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadLocateRequest()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "csePersonNumber", import.CsePerson.Number);
        db.SetString(command, "agencyNumber", local.LocateRequest.AgencyNumber);
      },
      (db, reader) =>
      {
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 0);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 1);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 2);
        entities.LocateRequest.LicenseSuspensionInd =
          db.GetNullableString(reader, 3);
        entities.LocateRequest.Populated = true;
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
    /// A value of LicSuspThreshold.
    /// </summary>
    [JsonPropertyName("licSuspThreshold")]
    public Common LicSuspThreshold
    {
      get => licSuspThreshold ??= new();
      set => licSuspThreshold = value;
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

    private CsePerson csePerson;
    private Common licSuspThreshold;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LicSuspQualification.
    /// </summary>
    [JsonPropertyName("licSuspQualification")]
    public Common LicSuspQualification
    {
      get => licSuspQualification ??= new();
      set => licSuspQualification = value;
    }

    private Common licSuspQualification;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    /// <summary>
    /// A value of Local90DaysAgo.
    /// </summary>
    [JsonPropertyName("local90DaysAgo")]
    public ProgramProcessingInfo Local90DaysAgo
    {
      get => local90DaysAgo ??= new();
      set => local90DaysAgo = value;
    }

    private LocateRequest locateRequest;
    private ProgramProcessingInfo local90DaysAgo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private LocateRequest locateRequest;
    private AdministrativeActCertification administrativeActCertification;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CollectionType collectionType;
  }
#endregion
}
