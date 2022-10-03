// Program: CREATE_FUND_TRANSACTION, ID: 371725748, model: 746.
// Short name: SWE00144
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_FUND_TRANSACTION.
/// </para>
/// <para>
/// RESP: CASHMGMT
/// This action block will create a fund transaction, associate it to the 
/// appropriate pca fund explosion rule, and establish its fund transaction
/// status history.
/// </para>
/// </summary>
[Serializable]
public partial class CreateFundTransaction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_FUND_TRANSACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateFundTransaction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateFundTransaction.
  /// </summary>
  public CreateFundTransaction(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // AUTHOR		DATE		DESCRIPTION
    // ----------	--------	
    // -----------------------------------
    // J. Katz		06/04/99	Analyzed READ statements and set
    // 				read property to Select Only
    // 				where appropriate.
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ****   hardcode area  *****
    // *****  This should be the identifier of the 'active' transaction status.
    UseHardcodedFundingInformation();

    if (ReadPcaFundExplosionRule())
    {
      if (ReadFundTransactionType())
      {
        export.FundTransactionType.FundAffectInd =
          entities.FundTransactionType.FundAffectInd;

        do
        {
          try
          {
            CreateFundTransaction1();
            export.FundTransaction.Assign(entities.FundTransaction);

            if (ReadFundTransactionStatus())
            {
              export.FundTransactionStatus.Code =
                entities.FundTransactionStatus.Code;

              try
              {
                CreateFundTransactionStatusHistory();

                // ****  continue  ****
                break;
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_FUND_TRNS_STAT_HIST_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_FUND_TRNS_STAT_HIST_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
            else
            {
              ExitState = "FN0000_FUND_TRANS_STAT_NF_RB";

              return;
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ++local.Retry.Count;

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_FUND_TRANS_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        while(local.Retry.Count <= 5);

        if (local.Retry.Count > 5)
        {
          ExitState = "FN0000_FUND_TRANS_AE_RB";
        }
      }
      else
      {
        ExitState = "FN0000_FUND_TRANS_TYPE_NF_RB";
      }
    }
    else
    {
      ExitState = "FN0000_PCA_FUND_EXP_RULE_NF_RB";
    }
  }

  private int UseGenerateFundTransactionId()
  {
    var useImport = new GenerateFundTransactionId.Import();
    var useExport = new GenerateFundTransactionId.Export();

    Call(GenerateFundTransactionId.Execute, useImport, useExport);

    return useExport.FundTransaction.SystemGeneratedIdentifier;
  }

  private void UseHardcodedFundingInformation()
  {
    var useImport = new HardcodedFundingInformation.Import();
    var useExport = new HardcodedFundingInformation.Export();

    Call(HardcodedFundingInformation.Execute, useImport, useExport);

    local.Hardcoded.SystemGeneratedIdentifier =
      useExport.Open.SystemGeneratedIdentifier;
  }

  private void CreateFundTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.PcaFundExplosionRule.Populated);

    var fttIdentifier = entities.FundTransactionType.SystemGeneratedIdentifier;
    var pcaCode = entities.PcaFundExplosionRule.PcaCode;
    var pcaEffectiveDate = entities.PcaFundExplosionRule.PcaEffectiveDate;
    var funIdentifier = entities.PcaFundExplosionRule.FunIdentifier;
    var systemGeneratedIdentifier = UseGenerateFundTransactionId();
    var depositNumber =
      import.FundTransaction.DepositNumber.GetValueOrDefault();
    var amount = import.FundTransaction.Amount;
    var businessDate = import.FundTransaction.BusinessDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.FundTransaction.Populated = false;
    Update("CreateFundTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "fttIdentifier", fttIdentifier);
        db.SetString(command, "pcaCode", pcaCode);
        db.SetDate(command, "pcaEffectiveDate", pcaEffectiveDate);
        db.SetInt32(command, "funIdentifier", funIdentifier);
        db.SetInt32(command, "fundTransId", systemGeneratedIdentifier);
        db.SetNullableInt32(command, "depositNumber", depositNumber);
        db.SetDecimal(command, "amount", amount);
        db.SetDate(command, "businessDate", businessDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
      });

    entities.FundTransaction.FttIdentifier = fttIdentifier;
    entities.FundTransaction.PcaCode = pcaCode;
    entities.FundTransaction.PcaEffectiveDate = pcaEffectiveDate;
    entities.FundTransaction.FunIdentifier = funIdentifier;
    entities.FundTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.FundTransaction.DepositNumber = depositNumber;
    entities.FundTransaction.Amount = amount;
    entities.FundTransaction.BusinessDate = businessDate;
    entities.FundTransaction.CreatedBy = createdBy;
    entities.FundTransaction.CreatedTimestamp = createdTimestamp;
    entities.FundTransaction.Populated = true;
  }

  private void CreateFundTransactionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    var ftrIdentifier = entities.FundTransaction.SystemGeneratedIdentifier;
    var funIdentifier = entities.FundTransaction.FunIdentifier;
    var pcaEffectiveDate = entities.FundTransaction.PcaEffectiveDate;
    var pcaCode = entities.FundTransaction.PcaCode;
    var fttIdentifier = entities.FundTransaction.FttIdentifier;
    var ftsIdentifier =
      entities.FundTransactionStatus.SystemGeneratedIdentifier;
    var effectiveTmst = entities.FundTransaction.CreatedTimestamp;
    var createdBy = global.UserId;
    var reasonText = import.FundTransactionStatusHistory.ReasonText ?? "";

    entities.FundTransactionStatusHistory.Populated = false;
    Update("CreateFundTransactionStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "ftrIdentifier", ftrIdentifier);
        db.SetInt32(command, "funIdentifier", funIdentifier);
        db.SetDate(command, "pcaEffectiveDate", pcaEffectiveDate);
        db.SetString(command, "pcaCode", pcaCode);
        db.SetInt32(command, "fttIdentifier", fttIdentifier);
        db.SetInt32(command, "ftsIdentifier", ftsIdentifier);
        db.SetDateTime(command, "effectiveTmst", effectiveTmst);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", effectiveTmst);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.FundTransactionStatusHistory.FtrIdentifier = ftrIdentifier;
    entities.FundTransactionStatusHistory.FunIdentifier = funIdentifier;
    entities.FundTransactionStatusHistory.PcaEffectiveDate = pcaEffectiveDate;
    entities.FundTransactionStatusHistory.PcaCode = pcaCode;
    entities.FundTransactionStatusHistory.FttIdentifier = fttIdentifier;
    entities.FundTransactionStatusHistory.FtsIdentifier = ftsIdentifier;
    entities.FundTransactionStatusHistory.EffectiveTmst = effectiveTmst;
    entities.FundTransactionStatusHistory.CreatedBy = createdBy;
    entities.FundTransactionStatusHistory.CreatedTimestamp = effectiveTmst;
    entities.FundTransactionStatusHistory.ReasonText = reasonText;
    entities.FundTransactionStatusHistory.Populated = true;
  }

  private bool ReadFundTransactionStatus()
  {
    entities.FundTransactionStatus.Populated = false;

    return Read("ReadFundTransactionStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "fundTransStatId",
          local.Hardcoded.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.FundTransactionStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatus.Code = db.GetString(reader, 1);
        entities.FundTransactionStatus.Populated = true;
      });
  }

  private bool ReadFundTransactionType()
  {
    entities.FundTransactionType.Populated = false;

    return Read("ReadFundTransactionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "fundTransTypeId",
          import.FundTransactionType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.FundTransactionType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionType.FundAffectInd = db.GetInt32(reader, 1);
        entities.FundTransactionType.Populated = true;
      });
  }

  private bool ReadPcaFundExplosionRule()
  {
    entities.PcaFundExplosionRule.Populated = false;

    return Read("ReadPcaFundExplosionRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "funIdentifier", import.Fund.SystemGeneratedIdentifier);
        db.SetString(command, "code", import.ProgramCostAccount.Code);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramCostAccount.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PcaFundExplosionRule.FunIdentifier = db.GetInt32(reader, 0);
        entities.PcaFundExplosionRule.PcaEffectiveDate = db.GetDate(reader, 1);
        entities.PcaFundExplosionRule.PcaCode = db.GetString(reader, 2);
        entities.PcaFundExplosionRule.IndexNumber = db.GetInt32(reader, 3);
        entities.PcaFundExplosionRule.Populated = true;
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
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
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
    /// A value of FundTransactionType.
    /// </summary>
    [JsonPropertyName("fundTransactionType")]
    public FundTransactionType FundTransactionType
    {
      get => fundTransactionType ??= new();
      set => fundTransactionType = value;
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

    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransaction fundTransaction;
    private FundTransactionType fundTransactionType;
    private ProgramCostAccount programCostAccount;
    private Fund fund;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of FundTransactionStatus.
    /// </summary>
    [JsonPropertyName("fundTransactionStatus")]
    public FundTransactionStatus FundTransactionStatus
    {
      get => fundTransactionStatus ??= new();
      set => fundTransactionStatus = value;
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

    private FundTransactionType fundTransactionType;
    private FundTransactionStatus fundTransactionStatus;
    private FundTransaction fundTransaction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Retry.
    /// </summary>
    [JsonPropertyName("retry")]
    public Common Retry
    {
      get => retry ??= new();
      set => retry = value;
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
    /// A value of Hardcoded.
    /// </summary>
    [JsonPropertyName("hardcoded")]
    public FundTransactionStatus Hardcoded
    {
      get => hardcoded ??= new();
      set => hardcoded = value;
    }

    private Common retry;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransactionStatus hardcoded;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
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
    /// A value of PcaFundExplosionRule.
    /// </summary>
    [JsonPropertyName("pcaFundExplosionRule")]
    public PcaFundExplosionRule PcaFundExplosionRule
    {
      get => pcaFundExplosionRule ??= new();
      set => pcaFundExplosionRule = value;
    }

    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransactionStatus fundTransactionStatus;
    private FundTransaction fundTransaction;
    private FundTransactionType fundTransactionType;
    private ProgramCostAccount programCostAccount;
    private Fund fund;
    private PcaFundExplosionRule pcaFundExplosionRule;
  }
#endregion
}
