// Program: FN_READ_VOLUNTARY_OBLIGATION, ID: 372100573, model: 746.
// Short name: SWE00588
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_VOLUNTARY_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This CAB will read the CSE Person, Obligation, Obligation Transaction (&amp;
/// related entities) and all of the support CSE Person's associated to the
/// Obligation.
/// Required Import Views:
/// 	CSE Person Number
/// 	Obligation Sys Gen Id
/// 	Obligation Transaction Sys Gen ID
/// </para>
/// </summary>
[Serializable]
public partial class FnReadVoluntaryObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_VOLUNTARY_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadVoluntaryObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadVoluntaryObligation.
  /// </summary>
  public FnReadVoluntaryObligation(IContext context, Import import,
    Export export):
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
    // *******************************************************************
    // AUTHOR		DATE			DESCRIPTION
    // SHERAZ MALIK	04/28/97		Change Current date
    // Paul R. Egger   07/10/97
    //   Added a section of code which determines if there is only one 
    // OBLIGATION for this OBLIGOR.  If so, it will return that one OBLIGATION.
    // 09/29/97	A Samuels	Problem Report 26135
    // *******************************************************************
    // *** 09/01/98  Bud Adams     Added import current-date  ***
    // ***		Deleted 'use fn-hardcoded-debt-distribution; 	***
    // ***		imported those values' ***
    // =================================================
    // 12/22/98 - B Adams  -  Program type was not being deduced
    //   properly.  Replaced Read of Program with Use
    // =================================================
    // =================================================
    // 10/20/99 - M Brown  -  pr#s 77437, 77622, 76960:
    // Removed Case and Worker from obligation screens.
    // 11/16/99 - B Adams  -  cleaned up views; removed obsolete
    //   code.
    // =================================================
    // *** Set hardcoded values ***
    local.HardcodeChildType.Type1 = "CH";
    local.HardcodeApType.Type1 = "AP";

    // ***  Begin Pre Code  ***
    if (import.Obligation.SystemGeneratedIdentifier == 0)
    {
      foreach(var item in ReadObligation2())
      {
        // : Aug 4, 1999, mfb - fixed 'obligation type nf' problem - import view
        //   to cab was not populated.  Populate a local view, since there's 
        // only
        //   one obligation type we're concerned with.
        local.ObligationType.Classification =
          import.HcOtCVoluntary.Classification;
        local.ObligationType.Code = "VOL";
        local.ObligationType.SystemGeneratedIdentifier =
          import.HcVolSysGenId.SystemGeneratedIdentifier;
        UseFnGetObligationStatus();

        // =================================================
        // 6/15/99 - Bud Adams  -  ONLY count those that Active.  If
        //   they're Deactive, disregard them.
        // =================================================
        if (AsChar(local.ScreenObligationStatus.ObligationStatus) == 'D')
        {
        }
        else
        {
          ++local.CountObligations.Count;

          // ------------------------------------------------------------------------------
          // The following SET statement is added to store the active Obligation
          // 'system_generated_identifier'. Sometimes only one active
          // obligation may exist along with many deactivated obligations. In
          // this situation if we READ a deactivated obligation after active
          // obligation, the SET statement in the IF condition below will not
          // store the active Obligation 'system_generated_identifier' .
          //                                            
          // -----  Vithal Madhira ( 07/06/2000)
          // -------------------------------------------------------------------------------------
          local.Active.SystemGeneratedIdentifier =
            entities.Count.SystemGeneratedIdentifier;
        }

        if (local.CountObligations.Count > 1)
        {
          ExitState = "FN0000_MULT_OBLIGATIONS_FOUND";

          return;
        }
      }

      if (local.CountObligations.Count == 1)
      {
        // ------------------------------------------------------------------------------
        // The SET statement will not store the active obligation. Comment it.
        //                                               
        // ---- Vithal Madhira (07/06/2000)
        // -------------------------------------------------------------------------------------
        local.Obligation.SystemGeneratedIdentifier =
          local.Active.SystemGeneratedIdentifier;
      }
      else
      {
        // : Aug 4, 1999, mfb - Added logic to escape if no obligations found.
        //   Removed set of oblig sys gen id to import sys gen id.
        ExitState = "FN0000_NO_OBLIGATIONS_FOUND";

        return;
      }
    }
    else
    {
      local.Obligation.SystemGeneratedIdentifier =
        import.Obligation.SystemGeneratedIdentifier;
    }

    // ***  End Pre Code  ***
    if (ReadObligation1())
    {
      export.Obligation.Assign(entities.Obligation);

      // =================================================
      // 11/16/99 - bud adams  -  Alternate billing location is out of
      //   context for voluntary obligations.  No coupons are sent.
      //   Code related to that concept deleted.
      // =================================================
      if (AsChar(entities.Obligation.OrderTypeCode) == 'I')
      {
        export.Obligation.OrderTypeCode = "Y";
      }
      else
      {
        export.Obligation.OrderTypeCode = "N";
      }

      // =================================================
      // 11/16/99 - b adams  -  Removed Read oblgation_type.  Not
      //   needed and not used.
      // =================================================
      // Call action block to check obligation for activity
      UseFnCheckObligationForActivity();

      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadObligationTransaction())
      {
        MoveObligationTransaction(entities.ObligationTransaction,
          export.Group.Update.ObligationTransaction);

        if (ReadDebtDetailDebtDetailStatusHistory())
        {
          MoveDebtDetail(entities.DebtDetail, export.DebtDetail);
          export.Disc.Date = entities.DebtDetail.CoveredPrdEndDt;
        }
        else
        {
          ExitState = "FN0211_DEBT_DETAIL_NF";
          export.Group.Next();

          return;
        }

        if (ReadCsePerson())
        {
          export.Group.Update.SupportedCsePerson.Number =
            entities.Supported.Number;
          local.Supported.Number = entities.Supported.Number;
          UseSiReadCsePerson1();
          MoveCsePersonsWorkSet(local.Supported,
            export.Group.Update.SupportedCsePersonsWorkSet);

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Group.Next();

            return;
          }

          // =================================================
          // 12/22/1998 - bud adams  -  designated person is no longer
          //   part of the requirements for this screen.  for now...
          // =================================================
          // ***--- Replaced Read with this Use - b adams - 12/22/98
          // ***--- Replaced the used action block with this new one.
          // =================================================
          // 3/31/99 - bud adams  -  fn_read_program_for_supp_person
          //   deleted.  Program code for supported person does not add
          //   any value to this screen and only leads to confusion.
          // =================================================
          // =================================================
          // 10/20/99 - M Brown  -  pr#s 77437, 77622, 76960:
          // Removed Case and Worker from obligation screens.
          // =================================================
        }
        else
        {
          ExitState = "FN0000_SUPP_PERSON_NF";
          export.Group.Next();

          return;
        }

        export.Group.Next();
      }

      if (export.Group.IsEmpty)
      {
        ExitState = "FN0000_NO_SUPPORTED_PERS_FOUND";

        return;
      }

      // =================================================
      // 11-17-98 - B Adams  -  Qualification of Effective_Dt and
      //   Discontinue_Dt below were using the wrong operators.
      // =================================================
      // Set the Manual Distribution flag
      if (ReadManualDistributionAudit())
      {
        export.ManualDistributionInd.Flag = "Y";
      }
      else
      {
        export.ManualDistributionInd.Flag = "N";
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    // **** Call AB to get a formatted Obligor name ****
    local.Obligor.Number = import.Obligor.Number;
    UseSiReadCsePerson2();
    export.Obligor.Number = local.Obligor.Number;

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedTmst = source.CreatedTmst;
    target.CpaType = source.CpaType;
    target.CspNumber = source.CspNumber;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveScreenObligationStatus(ScreenObligationStatus source,
    ScreenObligationStatus target)
  {
    target.ObligationStatusTxt = source.ObligationStatusTxt;
    target.ObligationStatus = source.ObligationStatus;
  }

  private void UseFnCheckObligationForActivity()
  {
    var useImport = new FnCheckObligationForActivity.Import();
    var useExport = new FnCheckObligationForActivity.Export();

    useImport.P.Assign(entities.Obligation);
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      import.HcOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.HcCpaObligor.Type1 = import.HcCpaObligor.Type1;

    Call(FnCheckObligationForActivity.Execute, useImport, useExport);

    MoveObligation(useImport.P, entities.Obligation);
    export.ObligationInEffectInd.Flag = useExport.ActiveObligation.Flag;
  }

  private void UseFnGetObligationStatus()
  {
    var useImport = new FnGetObligationStatus.Import();
    var useExport = new FnGetObligationStatus.Export();

    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Count.SystemGeneratedIdentifier;
    useImport.HcOtCVoluntary.Classification =
      import.HcOtCVoluntary.Classification;
    useImport.HcOtCAccruing.Classification =
      import.HcOtCAccruing.Classification;
    useImport.CsePerson.Number = import.Obligor.Number;
    useImport.CsePersonAccount.Type1 = import.HcCpaObligor.Type1;
    useImport.Current.Date = import.Current.Date;
    useImport.ObligationType.Assign(local.ObligationType);

    Call(FnGetObligationStatus.Execute, useImport, useExport);

    MoveScreenObligationStatus(useExport.ScreenObligationStatus,
      local.ScreenObligationStatus);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Supported.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.Supported);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Obligor.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Obligor.Number = useExport.CsePersonsWorkSet.Number;
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Supported.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ObligationTransaction.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported.Number = db.GetString(reader, 0);
        entities.Supported.Populated = true;
      });
  }

  private bool ReadDebtDetailDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetailStatusHistory.Populated = false;
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetailDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 8);
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 10);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 11);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 12);
        entities.DebtDetailStatusHistory.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.DebtDetailStatusHistory.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.DebtDetailStatusHistory.OtrType);
      });
  }

  private bool ReadManualDistributionAudit()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ManualDistributionAudit.Populated = false;

    return Read("ReadManualDistributionAudit",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ManualDistributionAudit.OtyType = db.GetInt32(reader, 0);
        entities.ManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ManualDistributionAudit.CspNumber = db.GetString(reader, 2);
        entities.ManualDistributionAudit.CpaType = db.GetString(reader, 3);
        entities.ManualDistributionAudit.EffectiveDt = db.GetDate(reader, 4);
        entities.ManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ManualDistributionAudit.CpaType);
      });
  }

  private bool ReadObligation1()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", local.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Description = db.GetNullableString(reader, 4);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 5);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 6);
        entities.Obligation.CreatedBy = db.GetString(reader, 7);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 8);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 10);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 11);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private IEnumerable<bool> ReadObligation2()
  {
    entities.Count.Populated = false;

    return ReadEach("ReadObligation2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.HcVolSysGenId.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Count.CpaType = db.GetString(reader, 0);
        entities.Count.CspNumber = db.GetString(reader, 1);
        entities.Count.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Count.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Count.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Count.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    return ReadEach("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", import.HcOtrnDebtType.Type1);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "debtTyp", import.HcOtrnVoluntary.DebtType);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 6);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 9);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
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
    /// A value of HcVolSysGenId.
    /// </summary>
    [JsonPropertyName("hcVolSysGenId")]
    public ObligationType HcVolSysGenId
    {
      get => hcVolSysGenId ??= new();
      set => hcVolSysGenId = value;
    }

    /// <summary>
    /// A value of HcOtCVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntary")]
    public ObligationType HcOtCVoluntary
    {
      get => hcOtCVoluntary ??= new();
      set => hcOtCVoluntary = value;
    }

    /// <summary>
    /// A value of HcOtCAccruing.
    /// </summary>
    [JsonPropertyName("hcOtCAccruing")]
    public ObligationType HcOtCAccruing
    {
      get => hcOtCAccruing ??= new();
      set => hcOtCAccruing = value;
    }

    /// <summary>
    /// A value of HcOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hcOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HcOtrrConcurrentObliga
    {
      get => hcOtrrConcurrentObliga ??= new();
      set => hcOtrrConcurrentObliga = value;
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
    /// A value of HcOtrnVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtrnVoluntary")]
    public ObligationTransaction HcOtrnVoluntary
    {
      get => hcOtrnVoluntary ??= new();
      set => hcOtrnVoluntary = value;
    }

    /// <summary>
    /// A value of HcOtrnDebtType.
    /// </summary>
    [JsonPropertyName("hcOtrnDebtType")]
    public ObligationTransaction HcOtrnDebtType
    {
      get => hcOtrnDebtType ??= new();
      set => hcOtrnDebtType = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private ObligationType hcVolSysGenId;
    private ObligationType hcOtCVoluntary;
    private ObligationType hcOtCAccruing;
    private ObligationTransactionRlnRsn hcOtrrConcurrentObliga;
    private CsePerson obligor;
    private ObligationTransaction hcOtrnVoluntary;
    private ObligationTransaction hcOtrnDebtType;
    private CsePersonAccount hcCpaObligor;
    private DateWorkArea current;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Prompt.
      /// </summary>
      [JsonPropertyName("prompt")]
      public Common Prompt
      {
        get => prompt ??= new();
        set => prompt = value;
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
      /// A value of SupportedCsePerson.
      /// </summary>
      [JsonPropertyName("supportedCsePerson")]
      public CsePerson SupportedCsePerson
      {
        get => supportedCsePerson ??= new();
        set => supportedCsePerson = value;
      }

      /// <summary>
      /// A value of SupportedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedCsePersonsWorkSet
      {
        get => supportedCsePersonsWorkSet ??= new();
        set => supportedCsePersonsWorkSet = value;
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
      /// A value of ObligationTransaction.
      /// </summary>
      [JsonPropertyName("obligationTransaction")]
      public ObligationTransaction ObligationTransaction
      {
        get => obligationTransaction ??= new();
        set => obligationTransaction = value;
      }

      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common prompt;
      private Common common;
      private CsePerson supportedCsePerson;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private ObligationTransaction obligationTransaction;
      private ServiceProvider serviceProvider;
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
    /// A value of Disc.
    /// </summary>
    [JsonPropertyName("disc")]
    public DateWorkArea Disc
    {
      get => disc ??= new();
      set => disc = value;
    }

    /// <summary>
    /// A value of Effective.
    /// </summary>
    [JsonPropertyName("effective")]
    public DateWorkArea Effective
    {
      get => effective ??= new();
      set => effective = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of ObligationInEffectInd.
    /// </summary>
    [JsonPropertyName("obligationInEffectInd")]
    public Common ObligationInEffectInd
    {
      get => obligationInEffectInd ??= new();
      set => obligationInEffectInd = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Obligation obligation;
    private DebtDetail debtDetail;
    private DateWorkArea disc;
    private DateWorkArea effective;
    private CsePersonsWorkSet obligor;
    private Common manualDistributionInd;
    private Common obligationInEffectInd;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
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
    /// A value of ScreenObligationStatus.
    /// </summary>
    [JsonPropertyName("screenObligationStatus")]
    public ScreenObligationStatus ScreenObligationStatus
    {
      get => screenObligationStatus ??= new();
      set => screenObligationStatus = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
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
    /// A value of EabReturn.
    /// </summary>
    [JsonPropertyName("eabReturn")]
    public AbendData EabReturn
    {
      get => eabReturn ??= new();
      set => eabReturn = value;
    }

    /// <summary>
    /// A value of HardcodeApType.
    /// </summary>
    [JsonPropertyName("hardcodeApType")]
    public CaseRole HardcodeApType
    {
      get => hardcodeApType ??= new();
      set => hardcodeApType = value;
    }

    /// <summary>
    /// A value of HardcodeChildType.
    /// </summary>
    [JsonPropertyName("hardcodeChildType")]
    public CaseRole HardcodeChildType
    {
      get => hardcodeChildType ??= new();
      set => hardcodeChildType = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonsWorkSet Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CountObligations.
    /// </summary>
    [JsonPropertyName("countObligations")]
    public Common CountObligations
    {
      get => countObligations ??= new();
      set => countObligations = value;
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
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Obligation Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      obligationType = null;
      screenObligationStatus = null;
      alternate = null;
      current = null;
      eabReturn = null;
      supported = null;
      obligor = null;
      countObligations = null;
      active = null;
    }

    private ObligationType obligationType;
    private ScreenObligationStatus screenObligationStatus;
    private CsePersonsWorkSet alternate;
    private DateWorkArea current;
    private AbendData eabReturn;
    private CaseRole hardcodeApType;
    private CaseRole hardcodeChildType;
    private CsePersonsWorkSet supported;
    private CsePersonsWorkSet obligor;
    private Common countObligations;
    private Obligation obligation;
    private Obligation active;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
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
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

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
    /// A value of ManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("manualDistributionAudit")]
    public ManualDistributionAudit ManualDistributionAudit
    {
      get => manualDistributionAudit ??= new();
      set => manualDistributionAudit = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Obligation Count
    {
      get => count ??= new();
      set => count = value;
    }

    private Obligation obligation;
    private CsePerson obligorCsePerson;
    private ObligationType obligationType;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private DebtDetail debtDetail;
    private CsePersonAccount obligorCsePersonAccount;
    private CsePerson supported;
    private ObligationTransaction obligationTransaction;
    private ManualDistributionAudit manualDistributionAudit;
    private Obligation count;
  }
#endregion
}
