// Program: FN_CAB_CHECK_MANUAL_DIST_FOR_CRD, ID: 371770026, model: 746.
// Short name: SWE01950
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
/// A program: FN_CAB_CHECK_MANUAL_DIST_FOR_CRD.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block checks if any obligation that may possibly get satisfied 
/// by a given cash receipt detail.
/// </para>
/// </summary>
[Serializable]
public partial class FnCabCheckManualDistForCrd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_CHECK_MANUAL_DIST_FOR_CRD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabCheckManualDistForCrd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabCheckManualDistForCrd.
  /// </summary>
  public FnCabCheckManualDistForCrd(IContext context, Import import,
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
    // ---------------------------------------------
    // Date	By	IDCR #	Description
    // 031397	govind		Initial creation
    // 042897	govind		Added logic to skip those obligations that have been 
    // manually distributed from this CRD.
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    export.ManualDistribInd.Flag = "N";

    if (!IsEmpty(import.CashReceiptDetail.ObligorPersonNumber))
    {
      // --- Current Date is used specifically instead of the processing date
      foreach(var item in ReadManualDistributionAuditObligation2())
      {
        if (!ReadDebtDetail())
        {
          // --- No distributable debt found. So ignore any manual distribution 
          // instruction that exists.
          continue;
        }

        if (!IsEmpty(import.CashReceiptDetail.CourtOrderNumber))
        {
          if (ReadLegalAction())
          {
            if (!Equal(import.CashReceiptDetail.CourtOrderNumber,
              entities.LegalAction.StandardNumber))
            {
              // ---------------------------------------------
              // The CRD belongs to one court order but the obligation belongs 
              // to another court order. So the manual instructions don't apply
              // to this cash receipt detail.
              // ---------------------------------------------
              continue;
            }
          }
          else
          {
            continue;
          }
        }

        // --- Either no court order is specified in CRD OR the court order # is
        // the same in CRD and the obligation. And the obligation has manual
        // distribution Instructions specified.
        // So now check if any collection exists (zero or non zero collection 
        // amount - MCOL may create a collection with zero amount) against that
        // CRD
        if (ReadCollection())
        {
          // --- The obligation set to manual dist has been distributed money 
          // from this cash receipt detail with method = 'manual'. So assume
          // that the user has seen this CRD and taken care of the obligation.
          // Try the next obligation.
          continue;
        }
        else
        {
          // --- At least one obligation found with manual dist ind set and no 
          // money has been distributed yet manually to that obligation from
          // this CRD.
          export.ManualDistribInd.Flag = "Y";

          return;
        }
      }
    }
    else if (!IsEmpty(import.CashReceiptDetail.ObligorSocialSecurityNumber))
    {
      local.CsePersonMatch.Flag = "1";
      local.CsePersonsWorkSet.Ssn =
        import.CashReceiptDetail.ObligorSocialSecurityNumber ?? Spaces(9);
      UseEabMatchCsePersons();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (local.MatchedCsePers.Count > 1)
        {
          ExitState = "LE0000_MULTIPLE_PERSONS_FOR_SSN";

          return;
        }

        if (local.MatchedCsePers.Count < 1)
        {
          ExitState = "FN0000_CRD_OBLIGOR_SSN_INVALID";

          return;
        }

        local.MatchedCsePers.Index = 0;
        local.MatchedCsePers.CheckSize();

        foreach(var item in ReadManualDistributionAuditObligation1())
        {
          if (!ReadDebtDetail())
          {
            // --- No distributable debt found. So ignore any manual 
            // distribution instruction that exists.
            continue;
          }

          if (!IsEmpty(import.CashReceiptDetail.CourtOrderNumber))
          {
            if (ReadLegalAction())
            {
              if (!Equal(import.CashReceiptDetail.CourtOrderNumber,
                entities.LegalAction.StandardNumber))
              {
                // ---------------------------------------------
                // The CRD belongs to one court order but the obligation belongs
                // to another court order. So the manual instructions don't
                // apply to this cash receipt detail.
                // ---------------------------------------------
                continue;
              }
            }
            else
            {
              continue;
            }
          }

          // --- Either no court order is specified in CRD OR the court order # 
          // is the same in CRD and the obligation. And the obligation has
          // manual distribution Instructions specified.
          // So now check if any collection exists (zero or non zero collection 
          // amount - MCOL may create a collection with zero amount) against
          // that CRD
          if (ReadCollection())
          {
            // --- The obligation set to manual dist has been distributed money 
            // from this cash receipt detail with method = 'manual'. So assume
            // that the user has seen this CRD and taken care of the obligation.
            continue;
          }
          else
          {
            // --- At least one obligation found with manual dist ind set and no
            // money has been distributed yet manually to that obligation from
            // this CRD.
            export.ManualDistribInd.Flag = "Y";

            return;
          }
        }
      }
      else if (IsExitState("ACO_ADABAS_NO_SSN_MATCH"))
      {
        ExitState = "FN0000_CRD_OBLIGOR_SSN_INVALID";
      }
      else
      {
      }
    }
    else if (!IsEmpty(import.CashReceiptDetail.CourtOrderNumber))
    {
      // --- There is no need to check for legal action classification "J" and 
      // nonzero Filed Date.
      foreach(var item in ReadManualDistributionAuditObligation3())
      {
        if (!ReadDebtDetail())
        {
          // --- No distributable debt found. So ignore any manual distribution 
          // instruction that exists.
          continue;
        }

        if (ReadCollection())
        {
          // --- The obligation set to manual dist has been distributed money 
          // from this cash receipt detail with method = 'manual'. So assume
          // that the user has seen this CRD and taken care of the obligation.
          continue;
        }
        else
        {
          // --- At least one obligation found with manual dist ind set and no 
          // money has been distributed yet manually to that obligation from
          // this CRD.
          export.ManualDistribInd.Flag = "Y";

          return;
        }
      }
    }
    else
    {
      // ---------------------------------------------
      // Neither the court order nor obligor has been specified. So it cannot be
      // checked.
      // ---------------------------------------------
    }
  }

  private static void MoveExport1ToMatchedCsePers(EabMatchCsePersons.Export.
    ExportGroup source, Local.MatchedCsePersGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.Ae.Flag = source.Ae.Flag;
    target.Cse.Flag = source.Cse.Flag;
    target.Kanpay.Flag = source.Kanpay.Flag;
    target.Kscares.Flag = source.Kscares.Flag;
    target.Alt.Flag = source.Alt.Flag;
  }

  private static void MoveMatchedCsePersToExport1(Local.
    MatchedCsePersGroup source, EabMatchCsePersons.Export.ExportGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.Ae.Flag = source.Ae.Flag;
    target.Cse.Flag = source.Cse.Flag;
    target.Kanpay.Flag = source.Kanpay.Flag;
    target.Kscares.Flag = source.Kscares.Flag;
    target.Alt.Flag = source.Alt.Flag;
  }

  private void UseEabMatchCsePersons()
  {
    var useImport = new EabMatchCsePersons.Import();
    var useExport = new EabMatchCsePersons.Export();

    useImport.Search.Flag = local.CsePersonMatch.Flag;
    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    local.MatchedCsePers.CopyTo(useExport.Export1, MoveMatchedCsePersToExport1);

    Call(EabMatchCsePersons.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.MatchedCsePers, MoveExport1ToMatchedCsePers);
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(import.CashReceiptDetail.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", import.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", import.CashReceiptDetail.CstIdentifier);
        db.SetInt32(command, "crtType", import.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.DistributionMethod = db.GetString(reader, 12);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadManualDistributionAuditObligation1()
  {
    entities.ManualDistributionAudit.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadManualDistributionAuditObligation1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.MatchedCsePers.Item.Detail.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ManualDistributionAudit.OtyType = db.GetInt32(reader, 0);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.ManualDistributionAudit.CspNumber = db.GetString(reader, 2);
        entities.Obligation.CspNumber = db.GetString(reader, 2);
        entities.ManualDistributionAudit.CpaType = db.GetString(reader, 3);
        entities.Obligation.CpaType = db.GetString(reader, 3);
        entities.ManualDistributionAudit.EffectiveDt = db.GetDate(reader, 4);
        entities.ManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 6);
        entities.ManualDistributionAudit.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ManualDistributionAudit.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadManualDistributionAuditObligation2()
  {
    entities.ManualDistributionAudit.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadManualDistributionAuditObligation2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          import.CashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ManualDistributionAudit.OtyType = db.GetInt32(reader, 0);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.ManualDistributionAudit.CspNumber = db.GetString(reader, 2);
        entities.Obligation.CspNumber = db.GetString(reader, 2);
        entities.ManualDistributionAudit.CpaType = db.GetString(reader, 3);
        entities.Obligation.CpaType = db.GetString(reader, 3);
        entities.ManualDistributionAudit.EffectiveDt = db.GetDate(reader, 4);
        entities.ManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 6);
        entities.ManualDistributionAudit.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ManualDistributionAudit.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadManualDistributionAuditObligation3()
  {
    entities.ManualDistributionAudit.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadManualDistributionAuditObligation3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", import.CashReceiptDetail.CourtOrderNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ManualDistributionAudit.OtyType = db.GetInt32(reader, 0);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.ManualDistributionAudit.CspNumber = db.GetString(reader, 2);
        entities.Obligation.CspNumber = db.GetString(reader, 2);
        entities.ManualDistributionAudit.CpaType = db.GetString(reader, 3);
        entities.Obligation.CpaType = db.GetString(reader, 3);
        entities.ManualDistributionAudit.EffectiveDt = db.GetDate(reader, 4);
        entities.ManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 6);
        entities.ManualDistributionAudit.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ManualDistributionAudit.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ManualDistribInd.
    /// </summary>
    [JsonPropertyName("manualDistribInd")]
    public Common ManualDistribInd
    {
      get => manualDistribInd ??= new();
      set => manualDistribInd = value;
    }

    private Common manualDistribInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A MatchedCsePersGroup group.</summary>
    [Serializable]
    public class MatchedCsePersGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of Ae.
      /// </summary>
      [JsonPropertyName("ae")]
      public Common Ae
      {
        get => ae ??= new();
        set => ae = value;
      }

      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>
      /// A value of Kanpay.
      /// </summary>
      [JsonPropertyName("kanpay")]
      public Common Kanpay
      {
        get => kanpay ??= new();
        set => kanpay = value;
      }

      /// <summary>
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>
      /// A value of Alt.
      /// </summary>
      [JsonPropertyName("alt")]
      public Common Alt
      {
        get => alt ??= new();
        set => alt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet detail;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
      private Common alt;
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// Gets a value of MatchedCsePers.
    /// </summary>
    [JsonIgnore]
    public Array<MatchedCsePersGroup> MatchedCsePers => matchedCsePers ??= new(
      MatchedCsePersGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of MatchedCsePers for json serialization.
    /// </summary>
    [JsonPropertyName("matchedCsePers")]
    [Computed]
    public IList<MatchedCsePersGroup> MatchedCsePers_Json
    {
      get => matchedCsePers;
      set => MatchedCsePers.Assign(value);
    }

    /// <summary>
    /// A value of CsePersonMatch.
    /// </summary>
    [JsonPropertyName("csePersonMatch")]
    public Common CsePersonMatch
    {
      get => csePersonMatch ??= new();
      set => csePersonMatch = value;
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

    private DateWorkArea current;
    private DateWorkArea initialisedToZeros;
    private Array<MatchedCsePersGroup> matchedCsePers;
    private Common csePersonMatch;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private Collection collection;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private CsePersonAccount obligor;
    private ManualDistributionAudit manualDistributionAudit;
    private Obligation obligation;
  }
#endregion
}
