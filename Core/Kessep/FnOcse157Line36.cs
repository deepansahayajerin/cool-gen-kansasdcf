// Program: FN_OCSE157_LINE_36, ID: 371284081, model: 746.
// Short name: SWE02978
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_36.
/// </summary>
[Serializable]
public partial class FnOcse157Line36: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_36 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line36(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line36.
  /// </summary>
  public FnOcse157Line36(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // -------------------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // ---------------------------------------------------------------------
    // 08/10/06  GVandy	WR00230751	Initial Development. (Patterned after Line 25
    // )
    // -------------------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Verification.LineNumber = "36";
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "36 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

      // --------------------------------------------------------------
      // Initialize counters on restart
      // ---------------------------------------------------------------
      local.Line36.Currency152 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 15)) / (
          decimal)100;
    }

    // -------------------------------------------------------------------
    // Read Each is sorted in Asc order of Supp Person #.
    // -------------------------------------------------------------------
    foreach(var item in ReadCsePersonSupported())
    {
      if (Equal(entities.Supp.Number, local.Prev.Number))
      {
        continue;
      }

      local.Prev.Number = entities.Supp.Number;
      ++local.CommitCnt.Count;

      // -------------------------------------------------------------------
      // Clear local views
      // -------------------------------------------------------------------
      MoveOcse157Verification3(local.Null1, local.ForCreateOcse157Verification);

      // -------------------------------------------------------------------
      // -Read MS and MC collections (ob type sys gen ids 3 and 19)
      // -Read colls 'created during' FY and un-adj at the end of FY
      // -Read colls 'adjusted during' FY but created in prev FYs
      // -Skip Concurrent colls
      // -Skip direct payments. (CRT= 2 or 7)
      // -------------------------------------------------------------------
      // -------------------------------------------------------------------
      // Comments on READ EACH.
      // -Generates 3 table join on collection, ob_trn and crd
      // -Redundant created_tmst qualification is to aid performance
      // -------------------------------------------------------------------
      foreach(var item1 in ReadCollectionCsePerson())
      {
        if (Equal(entities.Collection.ProgramAppliedTo, "NA") || CharAt
          (entities.Collection.ProgramAppliedTo, 3) == 'I')
        {
          // -- Skip collections applied to NA and Interstate programs.
          continue;
        }

        if (Lt(entities.Collection.CreatedTmst, import.ReportStartDate.Timestamp))
          
        {
          // -----------------------------------------------------------------
          // This must be an adjustment to a collection created in prev FY
          // -----------------------------------------------------------------
          local.ForCreateOcse157Verification.CollectionAmount =
            -entities.Collection.Amount;
          local.Line36.Currency152 -= entities.Collection.Amount;
        }
        else
        {
          local.ForCreateOcse157Verification.CollectionAmount =
            entities.Collection.Amount;
          local.Line36.Currency152 += entities.Collection.Amount;
        }

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CollectionSgi =
            entities.Collection.SystemGeneratedIdentifier;
          local.ForCreateOcse157Verification.CollectionDte =
            entities.Collection.CollectionDt;
        }

        local.ForCreateOcse157Verification.CollCreatedDte =
          Date(entities.Collection.CreatedTmst);
        local.ForCreateOcse157Verification.CollApplToCode =
          entities.Collection.AppliedToCode;
        local.ForCreateOcse157Verification.SuppPersonNumber =
          entities.Supp.Number;
        local.ForCreateOcse157Verification.ObligorPersonNbr =
          entities.Ap.Number;
        local.ForCreateOcse157Verification.CourtOrderNumber =
          entities.Collection.CourtOrderAppliedTo;
        UseFnCreateOcse157Verification();
      }

      // --------------------------------------------
      // Check commit counts.
      // --------------------------------------------
      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "36 " + entities
          .Supp.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          ((long)(local.Line36.Currency152 * 100), 15);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.SuppPersonNumber = entities.Supp.Number;
          local.ForError.LineNumber = "36";
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }
    }

    // --------------------------------------------------
    // Processing complete for this line.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "36";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number =
      (long?)Math.
        Round(local.Line36.Currency152, MidpointRounding.AwayFromZero);
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "37 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "36";
      local.ForError.SuppPersonNumber = "";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveOcse157Verification1(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.DtePaternityEst = source.DtePaternityEst;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.SocialSecurityNumber = source.SocialSecurityNumber;
    target.ObTranSgi = source.ObTranSgi;
    target.ObTranAmount = source.ObTranAmount;
    target.ObligationSgi = source.ObligationSgi;
    target.DebtAdjType = source.DebtAdjType;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.GoodCauseEffDte = source.GoodCauseEffDte;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
  }

  private static void MoveOcse157Verification3(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.DtePaternityEst = source.DtePaternityEst;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.SocialSecurityNumber = source.SocialSecurityNumber;
    target.ObTranSgi = source.ObTranSgi;
    target.ObTranAmount = source.ObTranAmount;
    target.ObligationSgi = source.ObligationSgi;
    target.DebtAdjType = source.DebtAdjType;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.GoodCauseEffDte = source.GoodCauseEffDte;
    target.Comment = source.Comment;
  }

  private void UseFnCreateOcse157Data()
  {
    var useImport = new FnCreateOcse157Data.Import();
    var useExport = new FnCreateOcse157Data.Export();

    useImport.Ocse157Data.Assign(local.ForCreateOcse157Data);

    Call(FnCreateOcse157Data.Execute, useImport, useExport);
  }

  private void UseFnCreateOcse157Verification()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification1(local.ForCreateOcse157Verification,
      useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    MoveOcse157Verification2(local.ForError, useImport.Ocse157Verification);

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCollectionCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.Collection.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadCollectionCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDateTime(
          command, "createdTmst1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date", import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Ap.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Collection.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonSupported()
  {
    entities.Supp.Populated = false;
    entities.Supported.Populated = false;

    return ReadEach("ReadCsePersonSupported",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.Restart.Number);
        db.SetNullableString(
          command, "suppPersonNumber1", import.From.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber2", import.To.SuppPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supp.Populated = true;
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);

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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public Ocse157Verification From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public Ocse157Verification To
    {
      get => to ??= new();
      set => to = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
    private DateWorkArea reportStartDate;
    private Common displayInd;
    private DateWorkArea reportEndDate;
    private Ocse157Verification from;
    private Ocse157Verification to;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ForCreateOcse157Verification.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Verification")]
    public Ocse157Verification ForCreateOcse157Verification
    {
      get => forCreateOcse157Verification ??= new();
      set => forCreateOcse157Verification = value;
    }

    /// <summary>
    /// A value of ForCreateOcse157Data.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Data")]
    public Ocse157Data ForCreateOcse157Data
    {
      get => forCreateOcse157Data ??= new();
      set => forCreateOcse157Data = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Line36.
    /// </summary>
    [JsonPropertyName("line36")]
    public ReportTotals Line36
    {
      get => line36 ??= new();
      set => line36 = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private CsePerson restart;
    private ReportTotals line36;
    private CsePerson prev;
    private Common commitCnt;
    private Ocse157Verification null1;
    private Ocse157Verification forError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    private ObligationType obligationType;
    private CsePerson supp;
    private Collection collection;
    private CsePerson ap;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private CsePersonAccount supported;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
  }
#endregion
}
