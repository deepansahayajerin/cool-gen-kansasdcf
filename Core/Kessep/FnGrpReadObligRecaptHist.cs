// Program: FN_GRP_READ_OBLIG_RECAPT_HIST, ID: 372131724, model: 746.
// Short name: SWE00479
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_GRP_READ_OBLIG_RECAPT_HIST.
/// </summary>
[Serializable]
public partial class FnGrpReadObligRecaptHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GRP_READ_OBLIG_RECAPT_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGrpReadObligRecaptHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGrpReadObligRecaptHist.
  /// </summary>
  public FnGrpReadObligRecaptHist(IContext context, Import import, Export export)
    :
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
    // Read Oblig Recapture Hist
    // Date Created    Created by
    // 08/01/1995      Terry W. Cooley - MTW
    // ---------------------------------------------
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();

    if (!ReadObligor())
    {
      ExitState = "FN0000_OBLIGOR_NF";

      return;
    }

    // *** Read each obligation with a classification of RECOVERY
    export.Export1.Index = -1;

    foreach(var item in ReadObligationObligationType())
    {
      // *** If classification is not Recovery, skip to the next obligation.
      if (AsChar(entities.ObligationType.Classification) != 'R')
      {
        continue;
      }

      // --- For recovery obligations, there will be only one debt_detail 
      // record.
      if (ReadObligationTransactionDebtDetail())
      {
        // *** Move to export group view within the read each of the recapture 
        // inclusions.
        local.DebtDetailFound.Flag = "Y";
      }
      else
      {
        local.DebtDetailFound.Flag = "N";
      }

      if (ReadLegalAction())
      {
        // *** Move to export group view within the read each of the recapture 
        // inclusions.
        local.LegalActionFound.Flag = "Y";
      }
      else
      {
        local.LegalActionFound.Flag = "N";
      }

      foreach(var item1 in ReadRecaptureInclusion())
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.ObligationType.Code =
          entities.ObligationType.Code;
        export.Export1.Update.RecaptureInclusion.Assign(
          entities.RecaptureInclusion);

        if (Equal(export.Export1.Item.RecaptureInclusion.DiscontinueDate,
          local.MaxDate.Date))
        {
          export.Export1.Update.RecaptureInclusion.DiscontinueDate =
            local.InitialisedToZeros.Date;
        }

        if (AsChar(local.LegalActionFound.Flag) == 'Y')
        {
          MoveLegalAction(entities.LegalAction,
            export.Export1.Update.LegalAction);
        }

        if (AsChar(local.DebtDetailFound.Flag) == 'Y')
        {
          export.Export1.Update.DebtDetail.DueDt = entities.DebtDetail.DueDt;
          export.Export1.Update.ObligationTransaction.Amount =
            entities.ObligationTransaction.Amount;
        }

        export.Export1.Update.Obligation.SystemGeneratedIdentifier =
          entities.Obligation.SystemGeneratedIdentifier;
        export.Export1.Update.ObligationType.Code =
          entities.ObligationType.Code;
      }
    }
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

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
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ObligationType.Code = db.GetString(reader, 5);
        entities.ObligationType.Classification = db.GetString(reader, 6);
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

  private bool ReadObligor()
  {
    entities.Obligor.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private IEnumerable<bool> ReadRecaptureInclusion()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.RecaptureInclusion.Populated = false;

    return ReadEach("ReadRecaptureInclusion",
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
        entities.RecaptureInclusion.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.RecaptureInclusion.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.RecaptureInclusion.Populated = true;
        CheckValid<RecaptureInclusion>("CpaType",
          entities.RecaptureInclusion.CpaType);

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
      /// A value of RecaptureInclusion.
      /// </summary>
      [JsonPropertyName("recaptureInclusion")]
      public RecaptureInclusion RecaptureInclusion
      {
        get => recaptureInclusion ??= new();
        set => recaptureInclusion = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 180;

      private ObligationType obligationType;
      private Obligation obligation;
      private DebtDetail debtDetail;
      private ObligationTransaction obligationTransaction;
      private RecaptureInclusion recaptureInclusion;
      private LegalAction legalAction;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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
    /// A value of ZdelGrpExportSelect.
    /// </summary>
    [JsonPropertyName("zdelGrpExportSelect")]
    public Common ZdelGrpExportSelect
    {
      get => zdelGrpExportSelect ??= new();
      set => zdelGrpExportSelect = value;
    }

    private Array<ExportGroup> export1;
    private Common zdelGrpExportSelect;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DebtDetailFound.
    /// </summary>
    [JsonPropertyName("debtDetailFound")]
    public Common DebtDetailFound
    {
      get => debtDetailFound ??= new();
      set => debtDetailFound = value;
    }

    /// <summary>
    /// A value of LegalActionFound.
    /// </summary>
    [JsonPropertyName("legalActionFound")]
    public Common LegalActionFound
    {
      get => legalActionFound ??= new();
      set => legalActionFound = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public LegalAction Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of ZdelAlRecapInclusionFound.
    /// </summary>
    [JsonPropertyName("zdelAlRecapInclusionFound")]
    public TextWorkArea ZdelAlRecapInclusionFound
    {
      get => zdelAlRecapInclusionFound ??= new();
      set => zdelAlRecapInclusionFound = value;
    }

    private Common debtDetailFound;
    private Common legalActionFound;
    private DateWorkArea maxDate;
    private DateWorkArea initialisedToZeros;
    private LegalAction zdel;
    private TextWorkArea zdelAlRecapInclusionFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of RecaptureInclusion.
    /// </summary>
    [JsonPropertyName("recaptureInclusion")]
    public RecaptureInclusion RecaptureInclusion
    {
      get => recaptureInclusion ??= new();
      set => recaptureInclusion = value;
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public LegalActionDetail Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private ObligationTransaction obligationTransaction;
    private ObligationTransaction debt;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private CsePerson csePerson;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private RecaptureInclusion recaptureInclusion;
    private LegalAction legalAction;
    private LegalActionDetail zdel;
  }
#endregion
}
