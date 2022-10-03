// Program: FN_REOPEN_DEPOSIT, ID: 371768246, model: 746.
// Short name: SWE02221
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REOPEN_DEPOSIT.
/// </summary>
[Serializable]
public partial class FnReopenDeposit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REOPEN_DEPOSIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReopenDeposit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReopenDeposit.
  /// </summary>
  public FnReopenDeposit(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    UseHardcodedFundingInformation();

    if (!ReadFundTransaction())
    {
      ExitState = "FN0000_FUND_TRANS_NF";

      return;
    }

    foreach(var item in ReadFundTransactionStatusFundTransactionStatusHistory())
    {
      if (entities.FundTransactionStatus.SystemGeneratedIdentifier == local
        .HardcodedFtsClosed.SystemGeneratedIdentifier)
      {
        // ---------------------------------------------------------
        // Removed validation logic that mandated that a deposit
        // could only be reopened on the same day that it was closed.
        // JLK      10/30/98
        // ---------------------------------------------------------
        local.Common.Flag = "Y";

        break;
      }
      else
      {
        ExitState = "FN0000_DEPOSIT_ALREADY_OPEN";

        return;
      }
    }

    if (AsChar(local.Common.Flag) == 'Y')
    {
      export.FundTransactionStatusHistory.ReasonText =
        import.FundTransactionStatusHistory.ReasonText;
      export.FundTransactionStatusHistory.EffectiveTmst = Now();

      if (ReadFundTransactionStatus())
      {
        UseCreateFundTransactStatusHist();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
        }
      }
      else
      {
        ExitState = "FN0000_FUND_TRANS_STAT_NF";
      }
    }
    else
    {
      ExitState = "FN0000_FUND_TRANS_STAT_NF";
    }
  }

  private static void MoveFundTransactionStatusHistory1(
    FundTransactionStatusHistory source, FundTransactionStatusHistory target)
  {
    target.EffectiveTmst = source.EffectiveTmst;
    target.ReasonText = source.ReasonText;
  }

  private static void MoveFundTransactionStatusHistory2(
    FundTransactionStatusHistory source, FundTransactionStatusHistory target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCreateFundTransactStatusHist()
  {
    var useImport = new CreateFundTransactStatusHist.Import();
    var useExport = new CreateFundTransactStatusHist.Export();

    MoveFundTransactionStatusHistory1(export.FundTransactionStatusHistory,
      useImport.FundTransactionStatusHistory);
    useImport.FundTransaction.Assign(entities.FundTransaction);
    useImport.FundTransactionStatus.Assign(entities.MatchedToPersistent);

    Call(CreateFundTransactStatusHist.Execute, useImport, useExport);

    entities.FundTransaction.SystemGeneratedIdentifier =
      useImport.FundTransaction.SystemGeneratedIdentifier;
    entities.MatchedToPersistent.SystemGeneratedIdentifier =
      useImport.FundTransactionStatus.SystemGeneratedIdentifier;
    MoveFundTransactionStatusHistory2(useExport.FundTransactionStatusHistory,
      export.FundTransactionStatusHistory);
  }

  private void UseHardcodedFundingInformation()
  {
    var useImport = new HardcodedFundingInformation.Import();
    var useExport = new HardcodedFundingInformation.Export();

    Call(HardcodedFundingInformation.Execute, useImport, useExport);

    local.HardcodedFtsClosed.SystemGeneratedIdentifier =
      useExport.Closed.SystemGeneratedIdentifier;
    local.HardcodedFtsOpen.SystemGeneratedIdentifier =
      useExport.Open.SystemGeneratedIdentifier;
  }

  private bool ReadFundTransaction()
  {
    entities.FundTransaction.Populated = false;

    return Read("ReadFundTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "fundTransId",
          import.FundTransaction.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "fttIdentifier",
          import.FundTransactionType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "funIdentifier", import.Fund.SystemGeneratedIdentifier);
        db.SetString(command, "pcaCode", import.ProgramCostAccount.Code);
      },
      (db, reader) =>
      {
        entities.FundTransaction.FttIdentifier = db.GetInt32(reader, 0);
        entities.FundTransaction.PcaCode = db.GetString(reader, 1);
        entities.FundTransaction.PcaEffectiveDate = db.GetDate(reader, 2);
        entities.FundTransaction.FunIdentifier = db.GetInt32(reader, 3);
        entities.FundTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransaction.Populated = true;
      });
  }

  private bool ReadFundTransactionStatus()
  {
    entities.MatchedToPersistent.Populated = false;

    return Read("ReadFundTransactionStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "fundTransStatId",
          local.HardcodedFtsOpen.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MatchedToPersistent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MatchedToPersistent.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadFundTransactionStatusFundTransactionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);
    entities.FundTransactionStatusHistory.Populated = false;
    entities.FundTransactionStatus.Populated = false;

    return ReadEach("ReadFundTransactionStatusFundTransactionStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetString(command, "pcaCode", entities.FundTransaction.PcaCode);
      },
      (db, reader) =>
      {
        entities.FundTransactionStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatusHistory.FtsIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatusHistory.FtrIdentifier =
          db.GetInt32(reader, 1);
        entities.FundTransactionStatusHistory.FunIdentifier =
          db.GetInt32(reader, 2);
        entities.FundTransactionStatusHistory.PcaEffectiveDate =
          db.GetDate(reader, 3);
        entities.FundTransactionStatusHistory.PcaCode = db.GetString(reader, 4);
        entities.FundTransactionStatusHistory.FttIdentifier =
          db.GetInt32(reader, 5);
        entities.FundTransactionStatusHistory.EffectiveTmst =
          db.GetDateTime(reader, 6);
        entities.FundTransactionStatusHistory.CreatedBy =
          db.GetString(reader, 7);
        entities.FundTransactionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.FundTransactionStatusHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.FundTransactionStatusHistory.Populated = true;
        entities.FundTransactionStatus.Populated = true;

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
    /// A value of FundTransactionStatus.
    /// </summary>
    [JsonPropertyName("fundTransactionStatus")]
    public FundTransactionStatus FundTransactionStatus
    {
      get => fundTransactionStatus ??= new();
      set => fundTransactionStatus = value;
    }

    /// <summary>
    /// A value of FundTransactionType.
    /// </summary>
    [JsonPropertyName("fundTransactionType")]
    public FundTransactionType FundTransactionType
    {
      get => fundTransactionType ??= new();
      set => fundTransactionType = value;
    }

    /// <summary>
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    /// <summary>
    /// A value of ProgramCostAccount.
    /// </summary>
    [JsonPropertyName("programCostAccount")]
    public ProgramCostAccount ProgramCostAccount
    {
      get => programCostAccount ??= new();
      set => programCostAccount = value;
    }

    /// <summary>
    /// A value of Fund.
    /// </summary>
    [JsonPropertyName("fund")]
    public Fund Fund
    {
      get => fund ??= new();
      set => fund = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    private FundTransactionStatus fundTransactionStatus;
    private FundTransactionType fundTransactionType;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private ProgramCostAccount programCostAccount;
    private Fund fund;
    private FundTransaction fundTransaction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    private FundTransactionStatusHistory fundTransactionStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of HardcodedFtsClosed.
    /// </summary>
    [JsonPropertyName("hardcodedFtsClosed")]
    public FundTransactionStatus HardcodedFtsClosed
    {
      get => hardcodedFtsClosed ??= new();
      set => hardcodedFtsClosed = value;
    }

    /// <summary>
    /// A value of HardcodedFtsOpen.
    /// </summary>
    [JsonPropertyName("hardcodedFtsOpen")]
    public FundTransactionStatus HardcodedFtsOpen
    {
      get => hardcodedFtsOpen ??= new();
      set => hardcodedFtsOpen = value;
    }

    /// <summary>
    /// A value of HardcodeForReads.
    /// </summary>
    [JsonPropertyName("hardcodeForReads")]
    public FundTransactionType HardcodeForReads
    {
      get => hardcodeForReads ??= new();
      set => hardcodeForReads = value;
    }

    /// <summary>
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    private Common common;
    private FundTransactionStatus hardcodedFtsClosed;
    private FundTransactionStatus hardcodedFtsOpen;
    private FundTransactionType hardcodeForReads;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramCostAccount.
    /// </summary>
    [JsonPropertyName("programCostAccount")]
    public ProgramCostAccount ProgramCostAccount
    {
      get => programCostAccount ??= new();
      set => programCostAccount = value;
    }

    /// <summary>
    /// A value of PcaFundExplosionRule.
    /// </summary>
    [JsonPropertyName("pcaFundExplosionRule")]
    public PcaFundExplosionRule PcaFundExplosionRule
    {
      get => pcaFundExplosionRule ??= new();
      set => pcaFundExplosionRule = value;
    }

    /// <summary>
    /// A value of FundTransactionType.
    /// </summary>
    [JsonPropertyName("fundTransactionType")]
    public FundTransactionType FundTransactionType
    {
      get => fundTransactionType ??= new();
      set => fundTransactionType = value;
    }

    /// <summary>
    /// A value of Fund.
    /// </summary>
    [JsonPropertyName("fund")]
    public Fund Fund
    {
      get => fund ??= new();
      set => fund = value;
    }

    /// <summary>
    /// A value of MatchedToPersistent.
    /// </summary>
    [JsonPropertyName("matchedToPersistent")]
    public FundTransactionStatus MatchedToPersistent
    {
      get => matchedToPersistent ??= new();
      set => matchedToPersistent = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    /// <summary>
    /// A value of FundTransactionStatus.
    /// </summary>
    [JsonPropertyName("fundTransactionStatus")]
    public FundTransactionStatus FundTransactionStatus
    {
      get => fundTransactionStatus ??= new();
      set => fundTransactionStatus = value;
    }

    private ProgramCostAccount programCostAccount;
    private PcaFundExplosionRule pcaFundExplosionRule;
    private FundTransactionType fundTransactionType;
    private Fund fund;
    private FundTransactionStatus matchedToPersistent;
    private FundTransaction fundTransaction;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransactionStatus fundTransactionStatus;
  }
#endregion
}
