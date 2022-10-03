// Program: FN_GRP_READ_RECAPTURE_INCLUSIONS, ID: 372127106, model: 746.
// Short name: SWE00481
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_GRP_READ_RECAPTURE_INCLUSIONS.
/// </summary>
[Serializable]
public partial class FnGrpReadRecaptureInclusions: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GRP_READ_RECAPTURE_INCLUSIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGrpReadRecaptureInclusions(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGrpReadRecaptureInclusions.
  /// </summary>
  public FnGrpReadRecaptureInclusions(IContext context, Import import,
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
    // Group Read Recapture History
    // Date Created    Created by
    // 08/18/1995      Terry W. Cooley - MTW
    // Date Modified   Modified by
    // 09/11/1995      D.M. Nilsen
    // 03/22/1996      R.B.Mohapatra
    //   Log : * Replaced the Extended-Read which would have caused 3-table 
    // JOINS with embeded Reads
    // 05/01/97	A.Kinney	Changed Current_Date
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadObligationObligationType())
    {
      if (ReadObligationTransactionDebtDetail())
      {
        // *** continue ***
      }
      else
      {
        export.Export1.Next();

        continue;
      }

      if (ReadRecaptureInclusion())
      {
        if (!Lt(local.Current.Date, entities.RecaptureInclusion.EffectiveDate) &&
          Lt(local.Current.Date, entities.RecaptureInclusion.DiscontinueDate))
        {
          export.Export1.Update.DetailRecap.Flag = "Y";
          export.Export1.Update.DetailRecapPrev.Flag = "Y";
        }
        else
        {
          export.Export1.Update.DetailRecap.Flag = "N";
          export.Export1.Update.DetailRecapPrev.Flag = "N";
        }

        export.Export1.Update.DetailObligationTransaction.Amount =
          entities.ObligationTransaction.Amount;
        MoveDebtDetail(entities.DebtDetail,
          export.Export1.Update.DetailDebtDetail);
        export.Export1.Update.DetailRecaptureInclusion.SystemGeneratedId =
          entities.RecaptureInclusion.SystemGeneratedId;
        MoveObligationType(entities.ObligationType,
          export.Export1.Update.DetailObligationType);
        export.Export1.Update.DetailObligation.SystemGeneratedIdentifier =
          entities.Obligation.SystemGeneratedIdentifier;

        // **** read legal action associated to the CURRENT obligation ****
        if (ReadLegalAction())
        {
          MoveLegalAction(entities.LegalAction,
            export.Export1.Update.DetailLegalAction);
        }
        else
        {
          // OK
        }
      }

      export.Export1.Next();
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationType()
  {
    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 5);
        entities.Obligation.DormantInd = db.GetNullableString(reader, 6);
        entities.ObligationType.Code = db.GetString(reader, 7);
        entities.ObligationType.Classification = db.GetString(reader, 8);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadObligationTransactionDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;
    entities.DebtDetail.Populated = false;

    return Read("ReadObligationTransactionDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableDate(
          command, "retiredDt", local.Zero.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 10);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 11);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 13);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 14);
        entities.ObligationTransaction.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private bool ReadRecaptureInclusion()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.RecaptureInclusion.Populated = false;

    return Read("ReadRecaptureInclusion",
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
        entities.RecaptureInclusion.OtyType = db.GetInt32(reader, 0);
        entities.RecaptureInclusion.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.RecaptureInclusion.CspNumber = db.GetString(reader, 2);
        entities.RecaptureInclusion.CpaType = db.GetString(reader, 3);
        entities.RecaptureInclusion.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.RecaptureInclusion.DiscontinueDate = db.GetDate(reader, 5);
        entities.RecaptureInclusion.EffectiveDate = db.GetDate(reader, 6);
        entities.RecaptureInclusion.Populated = true;
        CheckValid<RecaptureInclusion>("CpaType",
          entities.RecaptureInclusion.CpaType);
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

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailSelect.
      /// </summary>
      [JsonPropertyName("detailSelect")]
      public Common DetailSelect
      {
        get => detailSelect ??= new();
        set => detailSelect = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailDebtDetail.
      /// </summary>
      [JsonPropertyName("detailDebtDetail")]
      public DebtDetail DetailDebtDetail
      {
        get => detailDebtDetail ??= new();
        set => detailDebtDetail = value;
      }

      /// <summary>
      /// A value of DetailObligationTransaction.
      /// </summary>
      [JsonPropertyName("detailObligationTransaction")]
      public ObligationTransaction DetailObligationTransaction
      {
        get => detailObligationTransaction ??= new();
        set => detailObligationTransaction = value;
      }

      /// <summary>
      /// A value of DetailRecap.
      /// </summary>
      [JsonPropertyName("detailRecap")]
      public Common DetailRecap
      {
        get => detailRecap ??= new();
        set => detailRecap = value;
      }

      /// <summary>
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>
      /// A value of DetailRecapPrev.
      /// </summary>
      [JsonPropertyName("detailRecapPrev")]
      public Common DetailRecapPrev
      {
        get => detailRecapPrev ??= new();
        set => detailRecapPrev = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailRecaptureInclusion.
      /// </summary>
      [JsonPropertyName("detailRecaptureInclusion")]
      public RecaptureInclusion DetailRecaptureInclusion
      {
        get => detailRecaptureInclusion ??= new();
        set => detailRecaptureInclusion = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private Common detailSelect;
      private ObligationType detailObligationType;
      private DebtDetail detailDebtDetail;
      private ObligationTransaction detailObligationTransaction;
      private Common detailRecap;
      private LegalAction detailLegalAction;
      private Common detailRecapPrev;
      private Obligation detailObligation;
      private RecaptureInclusion detailRecaptureInclusion;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of ZdelExportDetailPrev.
    /// </summary>
    [JsonPropertyName("zdelExportDetailPrev")]
    public DebtDetail ZdelExportDetailPrev
    {
      get => zdelExportDetailPrev ??= new();
      set => zdelExportDetailPrev = value;
    }

    /// <summary>
    /// A value of ZdelExportLast.
    /// </summary>
    [JsonPropertyName("zdelExportLast")]
    public RecaptureInclusion ZdelExportLast
    {
      get => zdelExportLast ??= new();
      set => zdelExportLast = value;
    }

    private Array<ExportGroup> export1;
    private DebtDetail zdelExportDetailPrev;
    private RecaptureInclusion zdelExportLast;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ActiveObligTransNf.
    /// </summary>
    [JsonPropertyName("activeObligTransNf")]
    public Common ActiveObligTransNf
    {
      get => activeObligTransNf ??= new();
      set => activeObligTransNf = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public LegalAction Work
    {
      get => work ??= new();
      set => work = value;
    }

    private DateWorkArea current;
    private Common activeObligTransNf;
    private DateWorkArea dateWorkArea;
    private DateWorkArea maximum;
    private DateWorkArea zero;
    private LegalAction work;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of RecaptureInclusion.
    /// </summary>
    [JsonPropertyName("recaptureInclusion")]
    public RecaptureInclusion RecaptureInclusion
    {
      get => recaptureInclusion ??= new();
      set => recaptureInclusion = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalActionDetail legalActionDetail;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private RecaptureInclusion recaptureInclusion;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private LegalAction legalAction;
  }
#endregion
}
