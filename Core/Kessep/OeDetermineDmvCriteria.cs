// Program: OE_DETERMINE_DMV_CRITERIA, ID: 371380962, model: 746.
// Short name: SWE03608
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_DETERMINE_DMV_CRITERIA.
/// </summary>
[Serializable]
public partial class OeDetermineDmvCriteria: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DETERMINE_DMV_CRITERIA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDetermineDmvCriteria(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDetermineDmvCriteria.
  /// </summary>
  public OeDetermineDmvCriteria(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************************************************
    // Initial Code       Dwayne Dupree        04/03/2008
    // This is checking the common compents for the driver's license restriction
    // process
    // **********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ZeroDate.Date = new DateTime(1, 1, 1);
    local.EndDate.Date = new DateTime(2099, 12, 31);

    // since it will be less than start date we will not be looking at a record 
    // that we could have just created.
    if (ReadKsDriversLicense())
    {
      // if the courtesy letter sent date is less than the incoming specified 
      // time frame for
      // letter then a new one should be created if all the other conditions are
      // meet
      // this will be checked by not only obligor but court order number also.
      // this will be a new table , what is here right now is only repersenting 
      // it
      export.CrtLetterSentDate.Date =
        entities.KsDriversLicense.CourtesyLetterSentDate;

      if (!Lt(entities.KsDriversLicense.CourtesyLetterSentDate,
        import.ValidPeriodCourtesyLtr.Date))
      {
        export.LtrWithinTimeFrame.Flag = "Y";

        goto Read;
      }

      // we are now checking the 30 day notice day to see if it is within the 
      // valid period and
      //  the record dose not have a record closure reason set
      if (Lt(entities.KsDriversLicense.CourtesyLetterSentDate,
        import.ValidPeriodCourtesyLtr.Date))
      {
        if (!Lt(entities.KsDriversLicense.Attribute30DayLetterCreatedDate,
          import.ValidPeriod30DayNotic.Date))
        {
          if (!IsEmpty(entities.KsDriversLicense.RecordClosureReason))
          {
          }
          else
          {
            export.LtrWithinTimeFrame.Flag = "Y";

            goto Read;
          }
        }
        else
        {
          goto Read;
        }
      }
      else
      {
        goto Read;
      }
    }

Read:

    foreach(var item in ReadIncarceration())
    {
      if (!Lt(local.ZeroDate.Date, entities.Incarceration.EndDate) || Lt
        (import.StartDate.Date, entities.Incarceration.EndDate))
      {
        export.NextPerson.Flag = "Y";
        export.FailureReturnCode.Flag = "B";

        return;
      }
    }

    if (ReadObligationAdmActionExemption())
    {
      export.GoToNextCourtOrder.Flag = "Y";
      export.FailureReturnCode.Flag = "C";

      // next legal action
      return;
    }

    foreach(var item in ReadCase())
    {
      if (ReadCaseRole())
      {
        if (ReadGoodCause())
        {
          // co=cooperting
          // gc=good cause
          // pd=pending
          if (Equal(entities.GoodCause.Code, "GC") || Equal
            (entities.GoodCause.Code, "PD"))
          {
            export.GoToNextCourtOrder.Flag = "Y";
            export.FailureReturnCode.Flag = "D";

            // next legal action
            return;
          }

          break;
        }

        break;
      }
    }

    UseCabGetNotEndedAddress();

    if (IsEmpty(export.CsePersonAddress.Street1))
    {
      export.NextPerson.Flag = "Y";
      export.FailureReturnCode.Flag = "E";
    }
  }

  private void UseCabGetNotEndedAddress()
  {
    var useImport = new CabGetNotEndedAddress.Import();
    var useExport = new CabGetNotEndedAddress.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(CabGetNotEndedAddress.Execute, useImport, useExport);

    export.CsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
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

  private bool ReadGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.
          SetNullableString(command, "casNumber1", entities.CaseRole.CasNumber);
          
        db.SetNullableInt32(
          command, "croIdentifier1", entities.CaseRole.Identifier);
        db.SetNullableString(command, "croType1", entities.CaseRole.Type1);
        db.
          SetNullableString(command, "cspNumber1", entities.CaseRole.CspNumber);
          
        db.SetNullableDate(
          command, "effectiveDate", import.StartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 8);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 9);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 10);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 11);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);
      });
  }

  private IEnumerable<bool> ReadIncarceration()
  {
    entities.Incarceration.Populated = false;

    return ReadEach("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate", local.ZeroDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 4);
        entities.Incarceration.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Incarceration.Incarcerated = db.GetNullableString(reader, 6);
        entities.Incarceration.Populated = true;

        return true;
      });
  }

  private bool ReadKsDriversLicense()
  {
    entities.KsDriversLicense.Populated = false;

    return Read("ReadKsDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNum", import.CsePerson.Number);
        db.SetNullableDate(
          command, "courtesyLtrDate",
          import.StartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 1);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 2);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 4);
        entities.KsDriversLicense.Populated = true;
      });
  }

  private bool ReadObligationAdmActionExemption()
  {
    entities.ObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
        db.SetString(command, "type", import.AdministrativeAction.Type1);
        db.SetDate(
          command, "effectiveDt", import.StartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);
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
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of ValidPeriodCourtesyLtr.
    /// </summary>
    [JsonPropertyName("validPeriodCourtesyLtr")]
    public DateWorkArea ValidPeriodCourtesyLtr
    {
      get => validPeriodCourtesyLtr ??= new();
      set => validPeriodCourtesyLtr = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ValidPeriod30DayNotic.
    /// </summary>
    [JsonPropertyName("validPeriod30DayNotic")]
    public DateWorkArea ValidPeriod30DayNotic
    {
      get => validPeriod30DayNotic ??= new();
      set => validPeriod30DayNotic = value;
    }

    private DateWorkArea startDate;
    private AdministrativeAction administrativeAction;
    private DateWorkArea validPeriodCourtesyLtr;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private DateWorkArea validPeriod30DayNotic;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FailureReturnCode.
    /// </summary>
    [JsonPropertyName("failureReturnCode")]
    public Common FailureReturnCode
    {
      get => failureReturnCode ??= new();
      set => failureReturnCode = value;
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

    /// <summary>
    /// A value of NextPerson.
    /// </summary>
    [JsonPropertyName("nextPerson")]
    public Common NextPerson
    {
      get => nextPerson ??= new();
      set => nextPerson = value;
    }

    /// <summary>
    /// A value of GoToNextCourtOrder.
    /// </summary>
    [JsonPropertyName("goToNextCourtOrder")]
    public Common GoToNextCourtOrder
    {
      get => goToNextCourtOrder ??= new();
      set => goToNextCourtOrder = value;
    }

    /// <summary>
    /// A value of LtrWithinTimeFrame.
    /// </summary>
    [JsonPropertyName("ltrWithinTimeFrame")]
    public Common LtrWithinTimeFrame
    {
      get => ltrWithinTimeFrame ??= new();
      set => ltrWithinTimeFrame = value;
    }

    /// <summary>
    /// A value of CrtLetterSentDate.
    /// </summary>
    [JsonPropertyName("crtLetterSentDate")]
    public DateWorkArea CrtLetterSentDate
    {
      get => crtLetterSentDate ??= new();
      set => crtLetterSentDate = value;
    }

    private Common failureReturnCode;
    private CsePersonAddress csePersonAddress;
    private Common nextPerson;
    private Common goToNextCourtOrder;
    private Common ltrWithinTimeFrame;
    private DateWorkArea crtLetterSentDate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    private DateWorkArea zeroDate;
    private DateWorkArea endDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
    }

    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of OthCaseRole.
    /// </summary>
    [JsonPropertyName("othCaseRole")]
    public CaseRole OthCaseRole
    {
      get => othCaseRole ??= new();
      set => othCaseRole = value;
    }

    /// <summary>
    /// A value of OthCsePerson.
    /// </summary>
    [JsonPropertyName("othCsePerson")]
    public CsePerson OthCsePerson
    {
      get => othCsePerson ??= new();
      set => othCsePerson = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

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
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
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

    private KsDriversLicense ksDriversLicense;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private AdministrativeAction administrativeAction;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private Obligation obligation;
    private LegalAction legalAction;
    private GoodCause goodCause;
    private CaseRole othCaseRole;
    private CsePerson othCsePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalActionCaseRole legalActionCaseRole;
    private Incarceration incarceration;
    private AdministrativeActCertification administrativeActCertification;
  }
#endregion
}
