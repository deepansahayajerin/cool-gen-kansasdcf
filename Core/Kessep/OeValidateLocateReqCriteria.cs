// Program: OE_VALIDATE_LOCATE_REQ_CRITERIA, ID: 374417857, model: 746.
// Short name: SWE02614
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_VALIDATE_LOCATE_REQ_CRITERIA.
/// </para>
/// <para>
/// Create locate requests.
/// </para>
/// </summary>
[Serializable]
public partial class OeValidateLocateReqCriteria: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_VALIDATE_LOCATE_REQ_CRITERIA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeValidateLocateReqCriteria(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeValidateLocateReqCriteria.
  /// </summary>
  public OeValidateLocateReqCriteria(IContext context, Import import,
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
    // 07/??/2000  SWSCDRS	????? 		Initial Coding
    // 10/11/2005  GVandy	?????		Performance Improvements.
    // 03/05/2007  GVandy	PR261671	Re-written to improve performance.
    // -----------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------
    // The export_loc_req_qualification flag is returned as
    //     Y - the person qualifies for locate request
    //     1 - the persons is incarcerated
    //     2 - the person does not have a debt more than 60 days old
    //     3 - the person has made a payment in the last 60 days
    //     4 - the person is not an AP
    //     5 - the person is already located
    // -------------------------------------------------------------------------------------------
    export.LocateReqQualification.Flag = "N";

    // ----------------------------------------------------------------------------------------------
    // Check if the person is incarcerated.
    // ----------------------------------------------------------------------------------------------
    if (ReadIncarceration())
    {
      // -- Person is incarcerated.  They do not qualify under the locate rules.
      // Skip this person.
      export.LocateReqQualification.Flag = "1";

      return;
    }
    else
    {
      // -- Continue
    }

    // ----------------------------------------------------------------------------------------------
    // Determine if the person owes a debt more than 60 days old.
    // ----------------------------------------------------------------------------------------------
    export.LocateReqQualification.Flag = "2";
    local.Local60DaysAgo.ProcessDate =
      AddDays(import.ProgramProcessingInfo.ProcessDate, -60);

    if (ReadDebtDetail())
    {
      // ----------------------------------------------------------
      // Besides owing a debt more than 60 days old, person has to
      // have had no payments w/in the last 60 days.
      // ----------------------------------------------------------
      if (ReadCashReceiptDetail())
      {
        export.LocateReqQualification.Flag = "3";

        goto Read;
      }

      // -- A debt exists more than 60 days old, no payment was found within the
      // last 60 days, and the person is not incarcerated.
      // -- The person qualifies under the locate rules.
      export.LocateReqQualification.Flag = "Y";

      return;
    }

Read:

    // ----------------------------------------------------------------------------------------------
    // If the person is an AP and are not located then they also qualify under 
    // the locate rules.
    // ----------------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------
    // Determine if person is an AP.
    // ----------------------------------------------------------------------------------------------
    if (ReadCaseRole())
    {
      // -- Continue
    }
    else
    {
      // -- The person is not an AP.
      export.LocateReqQualification.Flag = "4";

      return;
    }

    // ----------------------------------------------------------------------------------------------
    // Check address to determine if AP is located.
    // ----------------------------------------------------------------------------------------------
    UseSiGetCsePersonMailingAddr();

    if (Lt(local.Null1.Date, local.CsePersonAddress.VerifiedDate) && !
      Lt(import.ProgramProcessingInfo.ProcessDate,
      local.CsePersonAddress.VerifiedDate) && !
      Lt(local.CsePersonAddress.EndDate,
      import.ProgramProcessingInfo.ProcessDate))
    {
      // -- AP is already located.  They do not qualify under the locate rules.
      // Skip this person.
      export.LocateReqQualification.Flag = "5";
    }
    else
    {
      // -- AP is not located.  This person qualifies under the locate rules.
      export.LocateReqQualification.Flag = "Y";
    }
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetDate(
          command, "collectionDate",
          local.Local60DaysAgo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 4);
        entities.CashReceiptDetail.Populated = true;
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
        db.SetDate(
          command, "dueDt",
          local.Local60DaysAgo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadIncarceration()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LocateReqQualification.
    /// </summary>
    [JsonPropertyName("locateReqQualification")]
    public Common LocateReqQualification
    {
      get => locateReqQualification ??= new();
      set => locateReqQualification = value;
    }

    private Common locateReqQualification;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Local60DaysAgo.
    /// </summary>
    [JsonPropertyName("local60DaysAgo")]
    public ProgramProcessingInfo Local60DaysAgo
    {
      get => local60DaysAgo ??= new();
      set => local60DaysAgo = value;
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

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private ProgramProcessingInfo local60DaysAgo;
    private DateWorkArea null1;
    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private Incarceration incarceration;
    private CsePerson csePerson;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private DebtDetail debtDetail;
    private CaseRole caseRole;
  }
#endregion
}
