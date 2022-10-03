// Program: FN_UPDATE_VOLUNTARY_OBLIGATION, ID: 372100574, model: 746.
// Short name: SWE00679
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_VOLUNTARY_OBLIGATION.
/// </summary>
[Serializable]
public partial class FnUpdateVoluntaryObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_VOLUNTARY_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateVoluntaryObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateVoluntaryObligation.
  /// </summary>
  public FnUpdateVoluntaryObligation(IContext context, Import import,
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
    // ***************************************************************
    // DATE        PROGRAMMER          COMMENT
    // 07/17/97    Paul R. Egger-MTW   Added logic to update the debt detail for
    // the effective and discontinue dates passed in the import views.
    // 09/29/97	A Samuels	Problem report 26135
    // ***************************************************************
    // *** 09/01/98  Bud Adams	  deleted fn-hardcoded-debt-distribution	***
    // ***			imported values; imp timestamp  ***
    // =================================================
    // 12/22/98 - B Adams  -  Changed many exit states to ones
    //   with Rollback attribute
    // =================================================
    // 12/22/98 - B Adams  -  Read of Obligation not fully qualified
    // =================================================
    // 12/23/98 - b adams  -  Read actions of Debt_Detail improperly
    //   coded.  See note below for details.
    // =================================================
    ExitState = "ACO_NN0000_ALL_OK";
    export.ActiveObligation.Flag = import.ActiveObligation.Flag;

    // =================================================
    // 1/21/99 - b adams  -  The FOR EACH import_grp here was not
    //   doing anything except moving import to export.  These
    //   views were not referenced anyplace else.  Un-hooked the
    //   view matching.
    // =================================================
    // =================================================
    // 12/22/98 - B Adams  -  This read was not fully qualified;
    //   obligation_type relationship was missing.
    // =================================================
    if (ReadObligation2())
    {
      // ================================================
      // Moved UPDATE inside IF construct so that there is only 1 update.  [The 
      // DISASSOCIATE also updates Obligation] and having it process separately
      // like it was is expensive.  B Adams   9-30-98
      // ================================================
      // =================================================
      // 12/23/98 - b adams  -  There is no alternate billing location
      //   for a voluntary.  No coupons sent...
      //   Code deleted 11/16/99
      // =================================================
      // =================================================
      // 10/20/1999, pr#77542, mbrown: Obligation update was commented out
      //   in the above structure - it should not have been.
      // 11/16/99 - bud adams  -  added IF construct; using a different
      //   view to avoid taking 'intent' locks when it's not necessary.
      // =================================================
      if (!Equal(entities.Obligation.Description, import.Obligation.Description))
        
      {
        ReadObligation1();

        try
        {
          UpdateObligation();

          // ** OK **
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // =================================================
      // 12/23/98 - b adams  -  There is no alternate billing location
      //   for a voluntary.  No coupons sent...
      //   Code deleted 11/16/99
      // =================================================
      // =================================================
      // PR# 78975: 11/16/99 - b adams  -  allocation percent was not
      //   being changed if the number of supported persons had
      //   changed and it should be.  The imported count value will be
      //   0 if the count remains the same (i.e., none added).
      // =================================================
      if (import.NumberOfSupportedPrsns.Count > 0)
      {
        local.NumberOfSupportedPersns.Percentage = 100 / import
          .NumberOfSupportedPrsns.Count;
        local.PercentageAllocated.Count = (int)(100 - (
          long)import.NumberOfSupportedPrsns.Count * local
          .NumberOfSupportedPersns.Percentage);
      }

      // =================================================
      // 12/23/98 - B Adams  -  This embedded Read Each had been coded
      //   as a single Read extension, generating both a cursor and
      //   Select.  The error was never caught, and only one Debt_Detail
      //   was ever updated.
      //   Coding this as one Read Each results in more SQL than this
      //   way, because an extra cursor is then required for the Update.
      // =================================================
      foreach(var item in ReadObligationTransaction())
      {
        // =================================================
        // PR# 78975: 11/16/99 - b adams  -  allocation percent was not
        //   being changed if the number of supported persons had
        //   changed and it should be.  The imported count value will be
        //   0 if the count remains the same (i.e., none added).
        // =================================================
        if (import.NumberOfSupportedPrsns.Count > 0)
        {
          if (local.PercentageAllocated.Count > 0)
          {
            local.PercentageAllocated.Percentage =
              local.NumberOfSupportedPersns.Percentage + 1;
            --local.PercentageAllocated.Count;
          }
          else
          {
            local.PercentageAllocated.Percentage =
              local.NumberOfSupportedPersns.Percentage;
          }

          try
          {
            UpdateObligationTransaction();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        // =================================================
        // 11/16/99 - b adams  -  Replaced Read Each debt_detail with
        //   this Read (select only).  There will only ever be one d_d
        //   per voluntary ob_tran.
        // =================================================
        if (ReadDebtDetail2())
        {
          // =================================================
          // 0921/99 - mfb, problem report #74567: Effective Date no
          // longer protected, so include it in the update.
          // 11/16/99 - b adams  -  Added IF construct this is inside.
          // =================================================
          if (!Equal(entities.DebtDetail.CoveredPrdEndDt,
            import.DebtDetail.CoveredPrdEndDt) || !
            Equal(entities.DebtDetail.CoveredPrdStartDt,
            import.DebtDetail.CoveredPrdStartDt))
          {
            // =================================================
            // 11/16/99 - bud adams  -  using a different view to avoid
            //   taking 'intent' locks when it's not necessary.
            // =================================================
            if (ReadDebtDetail1())
            {
              try
              {
                UpdateDebtDetail();

                // ** OK **
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0214_DEBT_DETAIL_NU_RB";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0218_DEBT_DETAIL_PV_RB";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
        }
        else
        {
          ExitState = "FN0000_DEBT_DETAIL_NF_RB";
        }

        local.UpdateCounter.Count = 2;
      }

      if (local.UpdateCounter.Count == 0)
      {
        ExitState = "FN0000_OBLIG_TRANS_NF_RB";
      }

      MoveCommon(local.PercentageAllocated, export.PercentageAllocated);
      export.NumberOfSupportedPrsns.Percentage =
        local.NumberOfSupportedPersns.Percentage;
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";
    }

    // =================================================
    // 12/22/98 - b adams  -  FOR EACH with embedded CASE OF
    //   constructs here had all actions diverted by Escapes.
    //   Deleted it.
    // =================================================
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.Percentage = source.Percentage;
  }

  private bool ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.UpdateOnlyDebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
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
        entities.UpdateOnlyDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.UpdateOnlyDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.UpdateOnlyDebtDetail.CpaType = db.GetString(reader, 2);
        entities.UpdateOnlyDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.UpdateOnlyDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.UpdateOnlyDebtDetail.OtrType = db.GetString(reader, 5);
        entities.UpdateOnlyDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 6);
        entities.UpdateOnlyDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 7);
        entities.UpdateOnlyDebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.UpdateOnlyDebtDetail.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.UpdateOnlyDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.UpdateOnlyDebtDetail.CpaType);
          
        CheckValid<DebtDetail>("OtrType", entities.UpdateOnlyDebtDetail.OtrType);
          
      });
  }

  private bool ReadDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail2",
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
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 6);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadObligation1()
  {
    entities.UpdateOnlyObligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.UpdateOnlyObligation.CpaType = db.GetString(reader, 0);
        entities.UpdateOnlyObligation.CspNumber = db.GetString(reader, 1);
        entities.UpdateOnlyObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.UpdateOnlyObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.UpdateOnlyObligation.Description =
          db.GetNullableString(reader, 4);
        entities.UpdateOnlyObligation.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.UpdateOnlyObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 6);
        entities.UpdateOnlyObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.UpdateOnlyObligation.CpaType);
          
      });
  }

  private bool ReadObligation2()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.HcOtVoluntary.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Description = db.GetNullableString(reader, 4);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 6);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private IEnumerable<bool> ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
        db.SetString(command, "debtTyp", import.HcOtrnDtVoluntary.DebtType);
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
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 8);
        entities.ObligationTransaction.VoluntaryPercentageAmount =
          db.GetNullableInt32(reader, 9);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 10);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 11);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 12);
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

  private void UpdateDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.UpdateOnlyDebtDetail.Populated);

    var coveredPrdStartDt = import.DebtDetail.CoveredPrdStartDt;
    var coveredPrdEndDt = import.DebtDetail.CoveredPrdEndDt;
    var lastUpdatedTmst = import.Current.Timestamp;
    var lastUpdatedBy = global.UserId;

    entities.UpdateOnlyDebtDetail.Populated = false;
    Update("UpdateDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(command, "cvrdPrdStartDt", coveredPrdStartDt);
        db.SetNullableDate(command, "cvdPrdEndDt", coveredPrdEndDt);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.UpdateOnlyDebtDetail.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.UpdateOnlyDebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.UpdateOnlyDebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.UpdateOnlyDebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.UpdateOnlyDebtDetail.OtyType);
        db.SetString(command, "otrType", entities.UpdateOnlyDebtDetail.OtrType);
      });

    entities.UpdateOnlyDebtDetail.CoveredPrdStartDt = coveredPrdStartDt;
    entities.UpdateOnlyDebtDetail.CoveredPrdEndDt = coveredPrdEndDt;
    entities.UpdateOnlyDebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.UpdateOnlyDebtDetail.LastUpdatedBy = lastUpdatedBy;
    entities.UpdateOnlyDebtDetail.Populated = true;
  }

  private void UpdateObligation()
  {
    System.Diagnostics.Debug.Assert(entities.UpdateOnlyObligation.Populated);

    var description = import.Obligation.Description ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.Current.Timestamp;

    entities.UpdateOnlyObligation.Populated = false;
    Update("UpdateObligation",
      (db, command) =>
      {
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.UpdateOnlyObligation.CpaType);
        db.SetString(
          command, "cspNumber", entities.UpdateOnlyObligation.CspNumber);
        db.SetInt32(
          command, "obId",
          entities.UpdateOnlyObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.UpdateOnlyObligation.DtyGeneratedId);
      });

    entities.UpdateOnlyObligation.Description = description;
    entities.UpdateOnlyObligation.LastUpdatedBy = lastUpdatedBy;
    entities.UpdateOnlyObligation.LastUpdateTmst = lastUpdateTmst;
    entities.UpdateOnlyObligation.Populated = true;
  }

  private void UpdateObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = import.Current.Timestamp;
    var voluntaryPercentageAmount = local.PercentageAllocated.Percentage;

    entities.ObligationTransaction.Populated = false;
    Update("UpdateObligationTransaction",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableInt32(command, "volPctAmt", voluntaryPercentageAmount);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetInt32(
          command, "obTrnId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
      });

    entities.ObligationTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.ObligationTransaction.LastUpdatedTmst = lastUpdatedTmst;
    entities.ObligationTransaction.VoluntaryPercentageAmount =
      voluntaryPercentageAmount;
    entities.ObligationTransaction.Populated = true;
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
    /// A value of NumberOfSupportedPrsns.
    /// </summary>
    [JsonPropertyName("numberOfSupportedPrsns")]
    public Common NumberOfSupportedPrsns
    {
      get => numberOfSupportedPrsns ??= new();
      set => numberOfSupportedPrsns = value;
    }

    /// <summary>
    /// A value of HardcodedObligorLap.
    /// </summary>
    [JsonPropertyName("hardcodedObligorLap")]
    public LegalActionPerson HardcodedObligorLap
    {
      get => hardcodedObligorLap ??= new();
      set => hardcodedObligorLap = value;
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
    /// A value of HcOtCVoluntaryClassif.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntaryClassif")]
    public ObligationType HcOtCVoluntaryClassif
    {
      get => hcOtCVoluntaryClassif ??= new();
      set => hcOtCVoluntaryClassif = value;
    }

    /// <summary>
    /// A value of HcOt718BUraJudgement.
    /// </summary>
    [JsonPropertyName("hcOt718BUraJudgement")]
    public ObligationType HcOt718BUraJudgement
    {
      get => hcOt718BUraJudgement ??= new();
      set => hcOt718BUraJudgement = value;
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
    /// A value of HcOtCFeesClassificati.
    /// </summary>
    [JsonPropertyName("hcOtCFeesClassificati")]
    public ObligationType HcOtCFeesClassificati
    {
      get => hcOtCFeesClassificati ??= new();
      set => hcOtCFeesClassificati = value;
    }

    /// <summary>
    /// A value of HcCpaSupportedPerson.
    /// </summary>
    [JsonPropertyName("hcCpaSupportedPerson")]
    public CsePersonAccount HcCpaSupportedPerson
    {
      get => hcCpaSupportedPerson ??= new();
      set => hcCpaSupportedPerson = value;
    }

    /// <summary>
    /// A value of HcDdshActiveStatus.
    /// </summary>
    [JsonPropertyName("hcDdshActiveStatus")]
    public DebtDetailStatusHistory HcDdshActiveStatus
    {
      get => hcDdshActiveStatus ??= new();
      set => hcDdshActiveStatus = value;
    }

    /// <summary>
    /// A value of HcOtrnDtAccrual.
    /// </summary>
    [JsonPropertyName("hcOtrnDtAccrual")]
    public ObligationTransaction HcOtrnDtAccrual
    {
      get => hcOtrnDtAccrual ??= new();
      set => hcOtrnDtAccrual = value;
    }

    /// <summary>
    /// A value of HcOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hcOtrnTDebt")]
    public ObligationTransaction HcOtrnTDebt
    {
      get => hcOtrnTDebt ??= new();
      set => hcOtrnTDebt = value;
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
    /// A value of HcOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hcOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HcOtrrConcurrentObliga
    {
      get => hcOtrrConcurrentObliga ??= new();
      set => hcOtrrConcurrentObliga = value;
    }

    /// <summary>
    /// A value of HcOtrnDtDebtDetail.
    /// </summary>
    [JsonPropertyName("hcOtrnDtDebtDetail")]
    public ObligationTransaction HcOtrnDtDebtDetail
    {
      get => hcOtrnDtDebtDetail ??= new();
      set => hcOtrnDtDebtDetail = value;
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
    /// A value of HcOtrnDtVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtrnDtVoluntary")]
    public ObligationTransaction HcOtrnDtVoluntary
    {
      get => hcOtrnDtVoluntary ??= new();
      set => hcOtrnDtVoluntary = value;
    }

    /// <summary>
    /// A value of HcOtVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtVoluntary")]
    public ObligationType HcOtVoluntary
    {
      get => hcOtVoluntary ??= new();
      set => hcOtVoluntary = value;
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
    /// A value of ActiveObligation.
    /// </summary>
    [JsonPropertyName("activeObligation")]
    public Common ActiveObligation
    {
      get => activeObligation ??= new();
      set => activeObligation = value;
    }

    private Common numberOfSupportedPrsns;
    private LegalActionPerson hardcodedObligorLap;
    private DateWorkArea max;
    private ObligationType hcOtCVoluntaryClassif;
    private ObligationType hcOt718BUraJudgement;
    private ObligationType hcOtCRecoverClassific;
    private ObligationType hcOtCFeesClassificati;
    private CsePersonAccount hcCpaSupportedPerson;
    private DebtDetailStatusHistory hcDdshActiveStatus;
    private ObligationTransaction hcOtrnDtAccrual;
    private ObligationTransaction hcOtrnTDebt;
    private CsePerson csePerson;
    private ObligationTransactionRlnRsn hcOtrrConcurrentObliga;
    private ObligationTransaction hcOtrnDtDebtDetail;
    private CsePersonAccount hcCpaObligor;
    private ObligationTransaction hcOtrnDtVoluntary;
    private ObligationType hcOtVoluntary;
    private DateWorkArea current;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private Common activeObligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NumberOfSupportedPrsns.
    /// </summary>
    [JsonPropertyName("numberOfSupportedPrsns")]
    public Common NumberOfSupportedPrsns
    {
      get => numberOfSupportedPrsns ??= new();
      set => numberOfSupportedPrsns = value;
    }

    /// <summary>
    /// A value of PercentageAllocated.
    /// </summary>
    [JsonPropertyName("percentageAllocated")]
    public Common PercentageAllocated
    {
      get => percentageAllocated ??= new();
      set => percentageAllocated = value;
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

    private Common numberOfSupportedPrsns;
    private Common percentageAllocated;
    private Common activeObligation;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of UpdateCounter.
    /// </summary>
    [JsonPropertyName("updateCounter")]
    public Common UpdateCounter
    {
      get => updateCounter ??= new();
      set => updateCounter = value;
    }

    /// <summary>
    /// A value of NumberOfSupportedPersns.
    /// </summary>
    [JsonPropertyName("numberOfSupportedPersns")]
    public Common NumberOfSupportedPersns
    {
      get => numberOfSupportedPersns ??= new();
      set => numberOfSupportedPersns = value;
    }

    /// <summary>
    /// A value of PercentageAllocated.
    /// </summary>
    [JsonPropertyName("percentageAllocated")]
    public Common PercentageAllocated
    {
      get => percentageAllocated ??= new();
      set => percentageAllocated = value;
    }

    private CsePerson obligor;
    private Common common;
    private Common updateCounter;
    private Common numberOfSupportedPersns;
    private Common percentageAllocated;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of UpdateOnlyDebtDetail.
    /// </summary>
    [JsonPropertyName("updateOnlyDebtDetail")]
    public DebtDetail UpdateOnlyDebtDetail
    {
      get => updateOnlyDebtDetail ??= new();
      set => updateOnlyDebtDetail = value;
    }

    /// <summary>
    /// A value of UpdateOnlyObligation.
    /// </summary>
    [JsonPropertyName("updateOnlyObligation")]
    public Obligation UpdateOnlyObligation
    {
      get => updateOnlyObligation ??= new();
      set => updateOnlyObligation = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePerson Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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

    private DebtDetail updateOnlyDebtDetail;
    private Obligation updateOnlyObligation;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePerson alternate;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
  }
#endregion
}
