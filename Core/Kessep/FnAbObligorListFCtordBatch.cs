// Program: FN_AB_OBLIGOR_LIST_F_CTORD_BATCH, ID: 372568195, model: 746.
// Short name: SWE02166
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
/// A program: FN_AB_OBLIGOR_LIST_F_CTORD_BATCH.
/// </para>
/// <para>
/// This AB lists all the Obligors for a given Court Order #
/// </para>
/// </summary>
[Serializable]
public partial class FnAbObligorListFCtordBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_OBLIGOR_LIST_F_CTORD_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbObligorListFCtordBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbObligorListFCtordBatch.
  /// </summary>
  public FnAbObligorListFCtordBatch(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    // This AB gives the list of obligor information for a given court order #
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Date	By	IDCR#	Description
    // -------------------------------------------------------------------------------------
    // 121197	govind		Batch version cloned from FN AB OBLIGOR LIST F CT ORDER
    // 012098	govind		Fixed to avoid comparing CRD court order number directly
    // 			against legal action standard number. Per Lee Lacey, it was
    // 			forcing DB2 to do full table scan
    // 021998	govind		Fixed bug: Sorted by Person number was missing
    // 030398	govind		Modified the check for adabas unavailable
    // 102018  GVandy	CQ62619	If multiple obligor records exist with the same 
    // SSN (i.e. a
    // 			duplicate person record was created) then insure that the
    // 			person number with active debts is put in the export group
    // 			view first.
    // -------------------------------------------------------------------------------------
    export.WorkNoOfObligors.Count = 0;
    export.ObligorList.Index = -1;
    local.ForComparison.StandardNumber =
      import.CashReceiptDetail.CourtOrderNumber ?? "";

    // -- Load obligor/court orders with active debts into the export group.
    foreach(var item in ReadCsePersonLegalActionObligation1())
    {
      if (export.ObligorList.Index == -1)
      {
      }
      else if (Equal(entities.CsePerson.Number,
        export.ObligorList.Item.Detail.Number))
      {
        // --- cse person already moved to group export
        continue;
      }

      if (export.ObligorList.Index + 1 >= Export.ObligorListGroup.Capacity)
      {
        return;
      }

      // --Check for an active debt.
      if (ReadDebtDetail())
      {
        // --Continue.
      }
      else
      {
        // --Skip the NCP/court order.  It does not have an active debt.
        continue;
      }

      ++export.ObligorList.Index;
      export.ObligorList.CheckSize();

      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePersonBatch();

      if (Equal(local.AbendData.AdabasResponseCd, "0148"))
      {
        ExitState = "ADABAS_UNAVAILABLE_RB";

        return;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ++export.WorkNoOfObligors.Count;
    }

    // -- Load obligor/court orders without active debts into the export group.
    foreach(var item in ReadCsePersonLegalActionObligation2())
    {
      if (export.ObligorList.Count == 0)
      {
      }
      else
      {
        for(export.ObligorList.Index = 0; export.ObligorList.Index < export
          .ObligorList.Count; ++export.ObligorList.Index)
        {
          if (!export.ObligorList.CheckSize())
          {
            break;
          }

          if (Equal(entities.CsePerson.Number,
            export.ObligorList.Item.Detail.Number))
          {
            // --- cse person already moved to group export
            goto ReadEach;
          }
        }

        export.ObligorList.CheckIndex();
      }

      if (export.ObligorList.Count >= Export.ObligorListGroup.Capacity)
      {
        return;
      }

      // --Check for an active debt.
      if (ReadDebtDetail())
      {
        // --Skip the NCP/court order.  It has an active debt.
        continue;
      }
      else
      {
        // --Continue.
      }

      export.ObligorList.Index = export.ObligorList.Count;
      export.ObligorList.CheckSize();

      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePersonBatch();

      if (Equal(local.AbendData.AdabasResponseCd, "0148"))
      {
        ExitState = "ADABAS_UNAVAILABLE_RB";

        return;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ++export.WorkNoOfObligors.Count;

ReadEach:
      ;
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.KscaresNumber = source.KscaresNumber;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.EmergencyAreaCode = source.EmergencyAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.UnemploymentInd = source.UnemploymentInd;
    target.FederalInd = source.FederalInd;
    target.TaxIdSuffix = source.TaxIdSuffix;
    target.TaxId = source.TaxId;
    target.OrganizationName = source.OrganizationName;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePerson(useExport.CsePerson, export.ObligorList.Update.DetailObligor);
    export.ObligorList.Update.Detail.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCsePersonLegalActionObligation1()
  {
    entities.Obligation.Populated = false;
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadCsePersonLegalActionObligation1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.ForComparison.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 1);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 1);
        entities.LegalAction.Classification = db.GetString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.Obligation.CpaType = db.GetString(reader, 4);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 6);
        entities.Obligation.Populated = true;
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonLegalActionObligation2()
  {
    entities.Obligation.Populated = false;
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadCsePersonLegalActionObligation2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.ForComparison.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 1);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 1);
        entities.LegalAction.Classification = db.GetString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.Obligation.CpaType = db.GetString(reader, 4);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 6);
        entities.Obligation.Populated = true;
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
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
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
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
    /// <summary>A ObligorListGroup group.</summary>
    [Serializable]
    public class ObligorListGroup
    {
      /// <summary>
      /// A value of DetailObligor.
      /// </summary>
      [JsonPropertyName("detailObligor")]
      public CsePerson DetailObligor
      {
        get => detailObligor ??= new();
        set => detailObligor = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePerson detailObligor;
      private CsePersonsWorkSet detail;
    }

    /// <summary>
    /// A value of WorkNoOfObligors.
    /// </summary>
    [JsonPropertyName("workNoOfObligors")]
    public Common WorkNoOfObligors
    {
      get => workNoOfObligors ??= new();
      set => workNoOfObligors = value;
    }

    /// <summary>
    /// Gets a value of ObligorList.
    /// </summary>
    [JsonIgnore]
    public Array<ObligorListGroup> ObligorList => obligorList ??= new(
      ObligorListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ObligorList for json serialization.
    /// </summary>
    [JsonPropertyName("obligorList")]
    [Computed]
    public IList<ObligorListGroup> ObligorList_Json
    {
      get => obligorList;
      set => ObligorList.Assign(value);
    }

    private Common workNoOfObligors;
    private Array<ObligorListGroup> obligorList;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of ForComparison.
    /// </summary>
    [JsonPropertyName("forComparison")]
    public LegalAction ForComparison
    {
      get => forComparison ??= new();
      set => forComparison = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public CsePerson Work
    {
      get => work ??= new();
      set => work = value;
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

    private AbendData abendData;
    private LegalAction forComparison;
    private CsePerson work;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }
#endregion
}
