// Program: FN_EST_ACCRUING_OBLIGATION, ID: 372084591, model: 746.
// Short name: SWE00454
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_EST_ACCRUING_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block manages the creation of an Accruing Obligation.
/// </para>
/// </summary>
[Serializable]
public partial class FnEstAccruingObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EST_ACCRUING_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEstAccruingObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEstAccruingObligation.
  /// </summary>
  public FnEstAccruingObligation(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";

    // ===============================================
    //  THIS ACTION BLOCK IS ONLY USED BY OACC
    // ===============================================
    // ********************************************************************
    //   RVW 3/5/97  This cab has been adjusted such that, when a concurrent
    //   obligation is created, instead of setting values of the primary/
    //   secondary indicator to P and S, it sets them to J, for Joint and
    //   several.  From this point on, P and S will only be used in PREL for
    //   Primary/Secondary obligations.
    // ***--- Sumanta - 04/30/97 - MTW
    //  Made the following changes :-
    //   *- Changed the DB2 current date to IEF current date
    //   *- Added code to associate the Obligation to alt. address
    //   *- Added code to associate obligation to interstate request
    // *******************************************************************
    // *********************
    // 9/8/98  Bud Adams	Missing SETs of mandatory attributes in
    // 			created Entity Type; Added import of
    // 			current: timestamp and date.
    // *********************
    // 5/6/99 - bud adams  -  Read properties set
    // *********************
    if (ReadObligationType())
    {
      if (AsChar(entities.ObligationType.Classification) != 'A')
      {
        ExitState = "FN0000_OBLIG_TYPE_CLASS_INVLD_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_OBLIG_TYPE_NF_RB";

      return;
    }

    // =================================================
    // 12/30/98 - b adams  -  deleted Read of Legal_Action_Detail.
    // =================================================
    MoveObligation3(import.Obligation, export.Obligation);

    if (!IsEmpty(import.ConcurrentObligor.Number))
    {
      export.Obligation.PrimarySecondaryCode = "J";
    }
    else
    {
      export.Obligation.PrimarySecondaryCode = "";
    }

    if (AsChar(import.HistoryIndicator.Flag) == 'Y')
    {
      export.Obligation.HistoryInd = "Y";
    }

    UseFnCreateObligation1();

    if (Equal(export.Obligation.OtherStateAbbr, "KS"))
    {
      export.Obligation.OtherStateAbbr = "";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***--- Sumanta - MTW - 04/30/97
    // ***--- Associate the obligation to the alternate add..
    // ***---
    if (!IsEmpty(import.AltAdd.Number))
    {
      if (ReadObligation2())
      {
        if (ReadCsePerson())
        {
          AssociateObligation();
        }
        else
        {
          ExitState = "CSE_PERSON_NF_RB";
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_NF_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // =================================================
    // 2/24/1999 - bud adams  -  Removed CRUD having to do with
    //   creating interstate obligations and put a CAB and logic into
    //   the PrAD.
    // =================================================
    // : If required, set up a concurrent obligation
    if (!IsEmpty(import.ConcurrentObligor.Number))
    {
      if (IsEmpty(export.Obligation.OtherStateAbbr))
      {
        export.Obligation.OtherStateAbbr = "KS";
      }

      export.Concurrent.Assign(export.Obligation);
      UseFnCreateObligation2();

      if (Equal(export.Obligation.OtherStateAbbr, "KS"))
      {
        export.Obligation.OtherStateAbbr = "";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (!IsEmpty(import.AltAdd.Number))
      {
        if (ReadObligation1())
        {
          if (ReadCsePerson())
          {
            AssociateObligation();
          }
          else
          {
            ExitState = "CSE_PERSON_NF_RB";
          }
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF_RB";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // =================================================
      // 2/24/1999 - bud adams  -  Removed CRUD having to do with
      //   creating interstate obligations and put a CAB and logic into
      //   the PrAD.
      // =================================================
      UseFnRelateTwoObligations();
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveObligation1(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
  }

  private static void MoveObligation2(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveObligation3(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private void UseFnCreateObligation1()
  {
    var useImport = new FnCreateObligation.Import();
    var useExport = new FnCreateObligation.Export();

    useImport.LegalActionDetail.Number = import.LegalActionDetail.Number;
    useImport.Max.Date = import.Max.Date;
    useImport.HcOtCVoluntaryClassif.Classification =
      import.HcOtCVoluntaryClassif.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      import.HcOtCRecoverClassific.Classification;
    useImport.HcOtCFeesClassificati.Classification =
      import.HcOtCFeesClassificati.Classification;
    useImport.HcOtCAccruingClassifi.Classification =
      import.HcOtCAccruingClassifi.Classification;
    useImport.HardcodeCpaObligor.Type1 = import.HcCpaObligor.Type1;
    MoveDateWorkArea(import.Current, useImport.Current);
    useImport.HistoryIndicator.Flag = import.HistoryIndicator.Flag;
    useImport.LegalAction.Identifier = import.LegalAction.Identifier;
    useImport.Obligation.Assign(export.Obligation);
    useImport.ObligationPaymentSchedule.
      Assign(import.ObligationPaymentSchedule);
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.CsePerson.Number = import.Obligor.Number;

    Call(FnCreateObligation.Execute, useImport, useExport);

    MoveObligation2(useExport.Obligation, export.Obligation);
  }

  private void UseFnCreateObligation2()
  {
    var useImport = new FnCreateObligation.Import();
    var useExport = new FnCreateObligation.Export();

    useImport.LegalActionDetail.Number = import.LegalActionDetail.Number;
    useImport.Max.Date = import.Max.Date;
    useImport.HcOtCVoluntaryClassif.Classification =
      import.HcOtCVoluntaryClassif.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      import.HcOtCRecoverClassific.Classification;
    useImport.HcOtCFeesClassificati.Classification =
      import.HcOtCFeesClassificati.Classification;
    useImport.HcOtCAccruingClassifi.Classification =
      import.HcOtCAccruingClassifi.Classification;
    useImport.HardcodeCpaObligor.Type1 = import.HcCpaObligor.Type1;
    MoveDateWorkArea(import.Current, useImport.Current);
    useImport.HistoryIndicator.Flag = import.HistoryIndicator.Flag;
    useImport.LegalAction.Identifier = import.LegalAction.Identifier;
    useImport.Obligation.Assign(export.Concurrent);
    useImport.ObligationPaymentSchedule.
      Assign(import.ObligationPaymentSchedule);
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.CsePerson.Number = import.ConcurrentObligor.Number;

    Call(FnCreateObligation.Execute, useImport, useExport);

    MoveObligation1(useExport.Obligation, export.Concurrent);
  }

  private void UseFnRelateTwoObligations()
  {
    var useImport = new FnRelateTwoObligations.Import();
    var useExport = new FnRelateTwoObligations.Export();

    useImport.HcObligJointSevConcur.PrimarySecondaryCode =
      import.HcObligJointSevConcur.PrimarySecondaryCode;
    useImport.HcCpaObligor.Type1 = import.HcCpaObligor.Type1;
    useImport.HcOrrJointSeveral.SequentialGeneratedIdentifier =
      import.HcOrrJointSeveral.SequentialGeneratedIdentifier;
    useImport.Current.Timestamp = import.Current.Timestamp;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.FirstCsePerson.Number = import.Obligor.Number;
    useImport.FirstObligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.SecondCsePerson.Number = import.ConcurrentObligor.Number;
    useImport.SecondObligation.SystemGeneratedIdentifier =
      export.Concurrent.SystemGeneratedIdentifier;

    Call(FnRelateTwoObligations.Execute, useImport, useExport);
  }

  private void AssociateObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var cspPNumber = entities.AltAdd.Number;

    entities.Obligation.Populated = false;
    Update("AssociateObligation",
      (db, command) =>
      {
        db.SetNullableString(command, "cspPNumber", cspPNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.CspPNumber = cspPNumber;
    entities.Obligation.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.AltAdd.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.AltAdd.Number);
      },
      (db, reader) =>
      {
        entities.AltAdd.Number = db.GetString(reader, 0);
        entities.AltAdd.Populated = true;
      });
  }

  private bool ReadObligation1()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.ConcurrentObligor.Number);
        db.
          SetInt32(command, "obId", export.Concurrent.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadObligation2()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of HcObligJointSevConcur.
    /// </summary>
    [JsonPropertyName("hcObligJointSevConcur")]
    public Obligation HcObligJointSevConcur
    {
      get => hcObligJointSevConcur ??= new();
      set => hcObligJointSevConcur = value;
    }

    /// <summary>
    /// A value of HcOrrJointSeveral.
    /// </summary>
    [JsonPropertyName("hcOrrJointSeveral")]
    public ObligationRlnRsn HcOrrJointSeveral
    {
      get => hcOrrJointSeveral ??= new();
      set => hcOrrJointSeveral = value;
    }

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
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

    /// <summary>
    /// A value of HcOtCAccruingClassifi.
    /// </summary>
    [JsonPropertyName("hcOtCAccruingClassifi")]
    public ObligationType HcOtCAccruingClassifi
    {
      get => hcOtCAccruingClassifi ??= new();
      set => hcOtCAccruingClassifi = value;
    }

    /// <summary>
    /// A value of HcOtCFeesClassificati.
    /// </summary>
    [JsonPropertyName("hcOtCFeesClassificati")]
    public ObligationType HcOtCFeesClassificati
    {
      get => hcOtCFeesClassificati ??= new();
      set => hcOtCFeesClassificati = value;
    }

    /// <summary>
    /// A value of HcOtCRecoverClassific.
    /// </summary>
    [JsonPropertyName("hcOtCRecoverClassific")]
    public ObligationType HcOtCRecoverClassific
    {
      get => hcOtCRecoverClassific ??= new();
      set => hcOtCRecoverClassific = value;
    }

    /// <summary>
    /// A value of HcOtCVoluntaryClassif.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntaryClassif")]
    public ObligationType HcOtCVoluntaryClassif
    {
      get => hcOtCVoluntaryClassif ??= new();
      set => hcOtCVoluntaryClassif = value;
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
    /// A value of HistoryIndicator.
    /// </summary>
    [JsonPropertyName("historyIndicator")]
    public Common HistoryIndicator
    {
      get => historyIndicator ??= new();
      set => historyIndicator = value;
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
    /// A value of AltAdd.
    /// </summary>
    [JsonPropertyName("altAdd")]
    public CsePersonsWorkSet AltAdd
    {
      get => altAdd ??= new();
      set => altAdd = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ConcurrentObligor.
    /// </summary>
    [JsonPropertyName("concurrentObligor")]
    public CsePerson ConcurrentObligor
    {
      get => concurrentObligor ??= new();
      set => concurrentObligor = value;
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
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public ObligationType P
    {
      get => p ??= new();
      set => p = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of Discontinue.
    /// </summary>
    [JsonPropertyName("discontinue")]
    public DateWorkArea Discontinue
    {
      get => discontinue ??= new();
      set => discontinue = value;
    }

    private Obligation hcObligJointSevConcur;
    private ObligationRlnRsn hcOrrJointSeveral;
    private DateWorkArea max;
    private CsePersonAccount hcCpaObligor;
    private ObligationType hcOtCAccruingClassifi;
    private ObligationType hcOtCFeesClassificati;
    private ObligationType hcOtCRecoverClassific;
    private ObligationType hcOtCVoluntaryClassif;
    private DateWorkArea current;
    private Common historyIndicator;
    private Case1 case1;
    private CsePersonsWorkSet altAdd;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private CsePerson obligor;
    private CsePerson concurrentObligor;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationType p;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private DateWorkArea discontinue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Concurrent.
    /// </summary>
    [JsonPropertyName("concurrent")]
    public Obligation Concurrent
    {
      get => concurrent ??= new();
      set => concurrent = value;
    }

    private Obligation obligation;
    private Obligation concurrent;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
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
    /// A value of CurrencyObtainedOnFips.
    /// </summary>
    [JsonPropertyName("currencyObtainedOnFips")]
    public Common CurrencyObtainedOnFips
    {
      get => currencyObtainedOnFips ??= new();
      set => currencyObtainedOnFips = value;
    }

    /// <summary>
    /// A value of Secondary.
    /// </summary>
    [JsonPropertyName("secondary")]
    public Obligation Secondary
    {
      get => secondary ??= new();
      set => secondary = value;
    }

    /// <summary>
    /// A value of Primary.
    /// </summary>
    [JsonPropertyName("primary")]
    public Obligation Primary
    {
      get => primary ??= new();
      set => primary = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      currencyObtainedOnFips = null;
      secondary = null;
      primary = null;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common currencyObtainedOnFips;
    private Obligation secondary;
    private Obligation primary;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of AltAdd.
    /// </summary>
    [JsonPropertyName("altAdd")]
    public CsePerson AltAdd
    {
      get => altAdd ??= new();
      set => altAdd = value;
    }

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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson altAdd;
    private CsePerson obligor;
    private ObligationType obligationType;
  }
#endregion
}
