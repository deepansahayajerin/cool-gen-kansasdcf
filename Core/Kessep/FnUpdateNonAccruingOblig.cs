// Program: FN_UPDATE_NON_ACCRUING_OBLIG, ID: 372095446, model: 746.
// Short name: SWE00660
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
/// A program: FN_UPDATE_NON_ACCRUING_OBLIG.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block updates the Description and Contingency Reason on Non-
/// Accruing Obligation.  It is used by the Maintain Non-Accruing Obligation
/// screen.  It also updates Debt Detail entities, as the Due Date and Covered
/// Period date range are on the screen header.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateNonAccruingOblig: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_NON_ACCRUING_OBLIG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateNonAccruingOblig(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateNonAccruingOblig.
  /// </summary>
  public FnUpdateNonAccruingOblig(IContext context, Import import, Export export)
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
    // ************************************************
    // * Date	Developer	Description	       *
    // *061997	T.O.Redmond	Removed Logic that was *
    // *erroneously changing the Payment Schedule End *
    // *Date to the Covereed Period End Date.         *
    // ************************************************
    // ********************************
    // 9-3-98  B Adams	Deleted fn-hardcode-debt-detail; not used!
    // *******************************
    // =================================================
    // 12/18/98 - b adams  -  Since interstate information now is going
    //   to come from this transaction and NOT from legal, we need
    //   to be able to update it, at least on the same day that the
    //   obligation was created.  Added attributes.
    // =================================================
    // : Jan 2002, M. Brown, WO# 020144, Pre-conversion code is now updateable.
    export.ActiveObligation.Flag = import.ActiveObligation.Flag;

    if (ReadObligation())
    {
      if (AsChar(import.ActiveObligation.Flag) == 'N')
      {
        UseFnCheckObligationForActivity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      if (AsChar(export.ActiveObligation.Flag) == 'N')
      {
        try
        {
          UpdateObligation2();
          MoveObligation(entities.Obligation, export.Obligation);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIGATION_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIGATION_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        // =================================================
        // 2/13/1999 - b adams  -  The only thing that can be updated if
        //   the obligation is Active is the description.
        // =================================================
        try
        {
          UpdateObligation1();
          MoveObligation(entities.Obligation, export.Obligation);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIGATION_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIGATION_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
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

    // : Jan 2002, M. Brown, WO# 020144, Pre-conversion code is now updateable.
    // This used to read every debt detail for the obligation, and update it to 
    // new values that were entered in the header portion of the screen.
    // But the preconversion code is on the list portion of the screen, so the 
    // read was changed to use the debt detail group view, updating each
    // occurrence to the same header values as before, in addition to the pre-
    // conversion code entered on the detail lines.
    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (ReadDebtDetailObligationTransaction())
      {
        if (!Equal(entities.DebtDetail.PreconversionProgramCode,
          import.Import1.Item.DebtDetail.PreconversionProgramCode))
        {
          local.SetTrigger.Flag = "Y";
        }

        if (AsChar(import.ActiveObligation.Flag) == 'Y')
        {
          // : Jan 2002, M. Brown, WO# 020144, only preconversion program code 
          // may change.
          if (!Equal(entities.DebtDetail.PreconversionProgramCode,
            import.Import1.Item.DebtDetail.PreconversionProgramCode))
          {
            try
            {
              UpdateDebtDetail2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0214_DEBT_DETAIL_NU_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0218_DEBT_DETAIL_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
        else
        {
          try
          {
            UpdateDebtDetail1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0214_DEBT_DETAIL_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0218_DEBT_DETAIL_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else
      {
        ExitState = "FN0000_DEBT_DETAIL_NF_RB";

        return;
      }

      // : Jan 2002, M. Brown, WO# 020144
      //   If the preconversion program code was updated, set the trigger to the
      //   earliest collection for the debt.
      if (AsChar(local.SetTrigger.Flag) == 'Y')
      {
        ReadCollection();

        if (Equal(local.Supported2.PgmChgEffectiveDate, local.Null1.Date))
        {
          goto Test;
        }

        if (ReadSupported())
        {
          try
          {
            UpdateSupported();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CSE_PERSON_ACCOUNT_NU_RB";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CSE_PERSON_ACCOUNT_PV_RB";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF_RB";

          return;
        }
      }

Test:
      ;
    }
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
  }

  private void UseFnCheckObligationForActivity()
  {
    var useImport = new FnCheckObligationForActivity.Import();
    var useExport = new FnCheckObligationForActivity.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      import.HcOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.Obligor.Number = import.CsePerson.Number;
    useImport.HcCpaObligor.Type1 = import.HardcodedObligor.Type1;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;

    Call(FnCheckObligationForActivity.Execute, useImport, useExport);

    export.ActiveObligation.Flag = useExport.ActiveObligation.Flag;
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    local.Supported2.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgId", entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        local.Supported2.PgmChgEffectiveDate = db.GetNullableDate(reader, 0);
        local.Supported2.Populated = true;
      });
  }

  private bool ReadDebtDetailObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetailObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnId",
          import.Import1.Item.ObligationTransaction.SystemGeneratedIdentifier);
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
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 9);
        entities.DebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 10);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 12);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.ObligationTransaction.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.HardcodedObligor.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 4);
        entities.Obligation.Description = db.GetNullableString(reader, 5);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 6);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 7);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 8);
        entities.Obligation.CreatedBy = db.GetString(reader, 9);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 10);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 12);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 13);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadSupported()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Supported.Populated = false;

    return Read("ReadSupported",
      (db, command) =>
      {
        db.SetString(
          command, "type", entities.ObligationTransaction.CpaSupType ?? "");
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspSupNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Supported.LastUpdatedTmst = db.GetNullableDateTime(reader, 3);
        entities.Supported.PgmChgEffectiveDate = db.GetNullableDate(reader, 4);
        entities.Supported.TriggerType = db.GetNullableString(reader, 5);
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);
      });
  }

  private void UpdateDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    var coveredPrdStartDt = import.Header.CoveredPrdStartDt;
    var coveredPrdEndDt = import.Header.CoveredPrdEndDt;
    var preconversionProgramCode =
      import.Import1.Item.DebtDetail.PreconversionProgramCode ?? "";
    var lastUpdatedTmst = import.Current.Timestamp;
    var lastUpdatedBy = global.UserId;

    entities.DebtDetail.Populated = false;
    Update("UpdateDebtDetail1",
      (db, command) =>
      {
        db.SetNullableDate(command, "cvrdPrdStartDt", coveredPrdStartDt);
        db.SetNullableDate(command, "cvdPrdEndDt", coveredPrdEndDt);
        db.
          SetNullableString(command, "precnvrsnPgmCd", preconversionProgramCode);
          
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId", entities.DebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
      });

    entities.DebtDetail.CoveredPrdStartDt = coveredPrdStartDt;
    entities.DebtDetail.CoveredPrdEndDt = coveredPrdEndDt;
    entities.DebtDetail.PreconversionProgramCode = preconversionProgramCode;
    entities.DebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.DebtDetail.LastUpdatedBy = lastUpdatedBy;
    entities.DebtDetail.Populated = true;
  }

  private void UpdateDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    var preconversionProgramCode =
      import.Import1.Item.DebtDetail.PreconversionProgramCode ?? "";
    var lastUpdatedTmst = import.Current.Timestamp;
    var lastUpdatedBy = global.UserId;

    entities.DebtDetail.Populated = false;
    Update("UpdateDebtDetail2",
      (db, command) =>
      {
        db.
          SetNullableString(command, "precnvrsnPgmCd", preconversionProgramCode);
          
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId", entities.DebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
      });

    entities.DebtDetail.PreconversionProgramCode = preconversionProgramCode;
    entities.DebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.DebtDetail.LastUpdatedBy = lastUpdatedBy;
    entities.DebtDetail.Populated = true;
  }

  private void UpdateObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var description = import.Obligation.Description ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.Current.Timestamp;

    entities.Obligation.Populated = false;
    Update("UpdateObligation1",
      (db, command) =>
      {
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.Description = description;
    entities.Obligation.LastUpdatedBy = lastUpdatedBy;
    entities.Obligation.LastUpdateTmst = lastUpdateTmst;
    entities.Obligation.Populated = true;
  }

  private void UpdateObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otherStateAbbr = import.Obligation.OtherStateAbbr ?? "";
    var description = import.Obligation.Description ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.Current.Timestamp;
    var orderTypeCode = import.Obligation.OrderTypeCode;

    CheckValid<Obligation>("OrderTypeCode", orderTypeCode);
    entities.Obligation.Populated = false;
    Update("UpdateObligation2",
      (db, command) =>
      {
        db.SetNullableString(command, "otherStateAbbr", otherStateAbbr);
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "ordTypCd", orderTypeCode);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.OtherStateAbbr = otherStateAbbr;
    entities.Obligation.Description = description;
    entities.Obligation.LastUpdatedBy = lastUpdatedBy;
    entities.Obligation.LastUpdateTmst = lastUpdateTmst;
    entities.Obligation.OrderTypeCode = orderTypeCode;
    entities.Obligation.Populated = true;
  }

  private void UpdateSupported()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = import.Current.Timestamp;
    var pgmChgEffectiveDate = local.Supported2.PgmChgEffectiveDate;

    entities.Supported.Populated = false;
    Update("UpdateSupported",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.SetNullableString(command, "triggerType", "");
        db.SetString(command, "cspNumber", entities.Supported.CspNumber);
        db.SetString(command, "type", entities.Supported.Type1);
      });

    entities.Supported.LastUpdatedBy = lastUpdatedBy;
    entities.Supported.LastUpdatedTmst = lastUpdatedTmst;
    entities.Supported.PgmChgEffectiveDate = pgmChgEffectiveDate;
    entities.Supported.TriggerType = "";
    entities.Supported.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of ZdelImportGrpDetail.
      /// </summary>
      [JsonPropertyName("zdelImportGrpDetail")]
      public Program ZdelImportGrpDetail
      {
        get => zdelImportGrpDetail ??= new();
        set => zdelImportGrpDetail = value;
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
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
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
      /// A value of Concurrent.
      /// </summary>
      [JsonPropertyName("concurrent")]
      public ObligationTransaction Concurrent
      {
        get => concurrent ??= new();
        set => concurrent = value;
      }

      /// <summary>
      /// A value of Prev.
      /// </summary>
      [JsonPropertyName("prev")]
      public ObligationTransaction Prev
      {
        get => prev ??= new();
        set => prev = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public DebtDetail Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program zdelImportGrpDetail;
      private CsePerson supportedCsePerson;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private ObligationTransaction obligationTransaction;
      private DebtDetail debtDetail;
      private ServiceProvider serviceProvider;
      private ObligationPaymentSchedule obligationPaymentSchedule;
      private ObligationTransaction concurrent;
      private ObligationTransaction prev;
      private Common common;
      private DebtDetail hidden;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Concurrent.
    /// </summary>
    [JsonPropertyName("concurrent")]
    public Common Concurrent
    {
      get => concurrent ??= new();
      set => concurrent = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public DebtDetail Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public DebtDetail Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of ActiveObligation.
    /// </summary>
    [JsonPropertyName("activeObligation")]
    public Common ActiveObligation
    {
      get => activeObligation ??= new();
      set => activeObligation = value;
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
    /// A value of ZdelImportP.
    /// </summary>
    [JsonPropertyName("zdelImportP")]
    public CsePerson ZdelImportP
    {
      get => zdelImportP ??= new();
      set => zdelImportP = value;
    }

    /// <summary>
    /// A value of HardcodedObligor.
    /// </summary>
    [JsonPropertyName("hardcodedObligor")]
    public CsePersonAccount HardcodedObligor
    {
      get => hardcodedObligor ??= new();
      set => hardcodedObligor = value;
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
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private ObligationType obligationType;
    private DateWorkArea current;
    private ObligationTransactionRlnRsn hcOtrrConcurrentObliga;
    private Common concurrent;
    private DebtDetail header;
    private DebtDetail prev;
    private Common activeObligation;
    private CsePerson csePerson;
    private CsePerson zdelImportP;
    private CsePersonAccount hardcodedObligor;
    private Obligation obligation;
    private Array<ImportGroup> import1;
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
    /// A value of ActiveObligation.
    /// </summary>
    [JsonPropertyName("activeObligation")]
    public Common ActiveObligation
    {
      get => activeObligation ??= new();
      set => activeObligation = value;
    }

    private Obligation obligation;
    private Common activeObligation;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Action.
    /// </summary>
    [JsonPropertyName("action")]
    public Common Action
    {
      get => action ??= new();
      set => action = value;
    }

    /// <summary>
    /// A value of SetTrigger.
    /// </summary>
    [JsonPropertyName("setTrigger")]
    public Common SetTrigger
    {
      get => setTrigger ??= new();
      set => setTrigger = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public DebtDetail Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private CsePerson supported1;
    private Common action;
    private Common setTrigger;
    private DebtDetail zdel;
    private CsePersonAccount supported2;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ObligationType Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private CsePersonAccount supported;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePerson csePerson;
    private ObligationType zdel;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private Collection collection;
  }
#endregion
}
