// Program: FN_PROCESS_VOLUNTARY_OBLIG_ADD, ID: 372100575, model: 746.
// Short name: SWE00527
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
/// A program: FN_PROCESS_VOLUNTARY_OBLIG_ADD.
/// </para>
/// <para>
/// This action block handles all processing associated with an Accruing 
/// Obligation add, including details
/// </para>
/// </summary>
[Serializable]
public partial class FnProcessVoluntaryObligAdd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_VOLUNTARY_OBLIG_ADD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessVoluntaryObligAdd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessVoluntaryObligAdd.
  /// </summary>
  public FnProcessVoluntaryObligAdd(IContext context, Import import,
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
    // =================================================
    // 09/01/98  Bud Adams	Deleted 'fn-hardcoded-debt-distribution
    // 			imported values
    // 			imported current-timestamp	
    // 11/6/98 - Bud Adams	Added exception logic to the CRUD
    // 			actions.
    // =================================================
    // 12/22/98 - Bud Adams  -  Added logic to capture the program
    //    code for screen display
    //    READ properties set
    // =================================================
    // 2/26/1999 - B Adams  -  Added logic to count the number of
    //   supported persons involved so the amount of money will
    //   be divided evenly among them.
    // =================================================
    ExitState = "ACO_NN0000_ALL_OK";
    MoveInfrastructure(import.Infrastructure, local.Infrastructure);

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      if (IsEmpty(import.Group.Item.SupportedCsePerson.Number))
      {
        export.Group.Next();

        continue;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      MoveCommon(import.Group.Item.Prompt, export.Group.Update.Prompt);
      MoveObligationTransaction1(import.Group.Item.ObligationTransaction,
        export.Group.Update.ObligationTransaction);
      export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
      export.Group.Update.SupportedCsePerson.Number =
        import.Group.Item.SupportedCsePerson.Number;
      MoveCsePersonsWorkSet(import.Group.Item.SupportedCsePersonsWorkSet,
        export.Group.Update.SupportedCsePersonsWorkSet);
      export.Group.Update.ServiceProvider.UserId =
        import.Group.Item.ServiceProvider.UserId;
      ++local.NumberOfSupportedPersns.Count;
      export.Group.Next();
    }

    if (ReadObligationType())
    {
      if (ReadCsePersonAccount())
      {
        // ** ALL OK **
      }
      else if (ReadCsePerson2())
      {
        // =================================================
        // 2/20/1999 - Bud Adams  -  This situation will be very rare and
        //   if we used 'obligor cse_person_account' in the Create action
        //   an 'update-intent' lock would be requested every time this
        //   CAB processed, because it's used in the above Read action.
        // =================================================
        try
        {
          CreateCsePersonAccount();

          // ** ALL OK **
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CSE_PERSON_ACCOUNT_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!ReadCsePersonAccount())
        {
          ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF_RB";
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGOR_NF";
      }
    }
    else
    {
      ExitState = "FN0000_OBLIG_TYPE_NF";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ****************************************************************
    // If an alternate address is indicated, relate it to the
    // obligation.
    // ****************************************************************
    // =================================================
    // 11/6/98 - B Adams  -  Moved CREATE action from above into
    //   both sections of the IF construct in order to avoid the optional
    //   ASSOCIATE action which would cause an UPDATE.
    // =================================================
    if (!IsEmpty(import.AltBillLoc.Number))
    {
      if (ReadCsePerson1())
      {
        try
        {
          CreateObligation1();
          MoveObligation(entities.Obligation, export.Obligation);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIGATION_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIGATION_PV";

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
        ExitState = "FN0000_ALTERNATE_ADD_NF";
      }
    }
    else
    {
      try
      {
        CreateObligation2();
        MoveObligation(entities.Obligation, export.Obligation);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGATION_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIGATION_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // Set up Obligation Transactions - 1 for each Supported Person
    local.NumberOfSupportedPersns.Percentage = 100 / local
      .NumberOfSupportedPersns.Count;
    local.PercentageAllocated.Count = (int)(100 - (
      long)local.NumberOfSupportedPersns.Count * local
      .NumberOfSupportedPersns.Percentage);

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (IsEmpty(export.Group.Item.SupportedCsePerson.Number))
      {
        continue;
      }

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

      UseFnCreateObligationTransaction();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ***--- Program code was not being captured for screen display  12/22/98
      // b adams
      export.Group.Update.Common.SelectChar = "";
      local.Infrastructure.CsePersonNumber = import.Obligor.Number;
      local.Infrastructure.EventId = 45;
      local.Infrastructure.ReasonCode = "OBLGVOLREC";
      local.Infrastructure.UserId = "OVOL";
      local.Infrastructure.BusinessObjectCd = "OBL";
      local.Infrastructure.DenormNumeric12 =
        export.Group.Item.ObligationTransaction.SystemGeneratedIdentifier;
      local.Infrastructure.DenormText12 = import.HcOtrnDebtType.Type1;
      UseFnCabRaiseEvent();
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveObligationTransaction1(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction2(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private int UseFnAssignObligationId()
  {
    var useImport = new FnAssignObligationId.Import();
    var useExport = new FnAssignObligationId.Export();

    useImport.CsePerson.Number = import.Obligor.Number;

    Call(FnAssignObligationId.Execute, useImport, useExport);

    return useExport.Obligation.SystemGeneratedIdentifier;
  }

  private void UseFnCabRaiseEvent()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.Supported.Number = export.Group.Item.SupportedCsePerson.Number;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      export.Group.Item.ObligationTransaction.SystemGeneratedIdentifier;
    useImport.Current.Timestamp = import.Current.Timestamp;

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseFnCreateObligationTransaction()
  {
    var useImport = new FnCreateObligationTransaction.Import();
    var useExport = new FnCreateObligationTransaction.Export();

    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.Obligor.Number = import.Obligor.Number;
    useImport.HardcodeObligorLap.AccountType =
      import.HardcodedObligorLap.AccountType;
    useImport.Max.Date = import.Max.Date;
    useImport.HcOtCVoluntaryClassif.Classification =
      import.HcOtCVoluntaryClassif.Classification;
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      import.HcOt718BUraJudgement.SystemGeneratedIdentifier;
    MoveObligationTransaction2(import.HcOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcOtCRecoverClassific.Classification =
      import.HcOtCRecoverClassific.Classification;
    useImport.HcOtCFeesClassificati.Classification =
      import.HcOtCFeesClassificati.Classification;
    useImport.HcCpaSupportedPerson.Type1 = import.HcCpaSupportedPerson.Type1;
    useImport.HcDdshActiveStatus.Code = import.HcDdshActiveStatus.Code;
    MoveObligationTransaction2(import.HcOtrnDtAccrual, useImport.HcOtrnDtAccrual);
      
    MoveObligationTransaction2(import.HcOtrnDtVoluntary,
      useImport.HcOtrnDtVoluntary);
    MoveObligationTransaction2(import.HcOtrnDtVoluntary, useImport.Hardcoded);
    useImport.HcOtrnTDebt.Type1 = import.HcOtrnDebtType.Type1;
    useImport.HcCpaObligor.Type1 = import.HcCpaObligor.Type1;
    MoveDateWorkArea(import.Current, useImport.Current);
    MoveDebtDetail(import.DebtDetail, useImport.DebtDetail);
    useImport.Supported.Number = export.Group.Item.SupportedCsePerson.Number;
    useImport.ObligationTransaction.Amount =
      export.Group.Item.ObligationTransaction.Amount;
    useImport.NumberOfSupportedPrsns.Percentage =
      local.PercentageAllocated.Percentage;

    Call(FnCreateObligationTransaction.Execute, useImport, useExport);

    export.Group.Update.ObligationTransaction.SystemGeneratedIdentifier =
      useExport.ObligationTransaction.SystemGeneratedIdentifier;
  }

  private void CreateCsePersonAccount()
  {
    var cspNumber = entities.ObligorCsePerson.Number;
    var type1 = import.HcCpaObligor.Type1;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;

    CheckValid<CsePersonAccount>("Type1", type1);
    entities.CreateObligor.Populated = false;
    Update("CreateCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableDate(command, "recompBalFromDt", default(DateTime));
        db.SetNullableDecimal(command, "stdTotGiftColl", 0M);
        db.SetNullableString(command, "triggerType", "");
      });

    entities.CreateObligor.CspNumber = cspNumber;
    entities.CreateObligor.Type1 = type1;
    entities.CreateObligor.CreatedBy = createdBy;
    entities.CreateObligor.CreatedTmst = createdTmst;
    entities.CreateObligor.Populated = true;
  }

  private void CreateObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);

    var cpaType = entities.ObligorCsePersonAccount.Type1;
    var cspNumber = entities.ObligorCsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseFnAssignObligationId();
    var dtyGeneratedId = entities.ObligationType.SystemGeneratedIdentifier;
    var cspPNumber = entities.AlternateBillLoc.Number;
    var otherStateAbbr = import.Obligation.OtherStateAbbr ?? "";
    var description = import.Obligation.Description ?? "";
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var orderTypeCode = import.Obligation.OrderTypeCode;

    CheckValid<Obligation>("CpaType", cpaType);
    CheckValid<Obligation>("PrimarySecondaryCode", "");
    CheckValid<Obligation>("OrderTypeCode", orderTypeCode);
    entities.Obligation.Populated = false;
    Update("CreateObligation1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obId", systemGeneratedIdentifier);
        db.SetInt32(command, "dtyGeneratedId", dtyGeneratedId);
        db.SetNullableString(command, "cspPNumber", cspPNumber);
        db.SetNullableString(command, "otherStateAbbr", otherStateAbbr);
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "historyInd", "");
        db.SetNullableString(command, "primSecCd", "");
        db.SetNullableInt32(command, "preConvDebtNo", 0);
        db.SetNullableString(command, "precnvrsnCaseNbr", "");
        db.SetNullableDecimal(command, "aodNadArrBal", 0M);
        db.SetNullableDate(command, "lastPymntDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetString(command, "ordTypCd", orderTypeCode);
      });

    entities.Obligation.CpaType = cpaType;
    entities.Obligation.CspNumber = cspNumber;
    entities.Obligation.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Obligation.DtyGeneratedId = dtyGeneratedId;
    entities.Obligation.CspPNumber = cspPNumber;
    entities.Obligation.OtherStateAbbr = otherStateAbbr;
    entities.Obligation.Description = description;
    entities.Obligation.PrimarySecondaryCode = "";
    entities.Obligation.CreatedBy = createdBy;
    entities.Obligation.CreatedTmst = createdTmst;
    entities.Obligation.OrderTypeCode = orderTypeCode;
    entities.Obligation.Populated = true;
  }

  private void CreateObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);

    var cpaType = entities.ObligorCsePersonAccount.Type1;
    var cspNumber = entities.ObligorCsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseFnAssignObligationId();
    var dtyGeneratedId = entities.ObligationType.SystemGeneratedIdentifier;
    var otherStateAbbr = import.Obligation.OtherStateAbbr ?? "";
    var description = import.Obligation.Description ?? "";
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var orderTypeCode = import.Obligation.OrderTypeCode;

    CheckValid<Obligation>("CpaType", cpaType);
    CheckValid<Obligation>("PrimarySecondaryCode", "");
    CheckValid<Obligation>("OrderTypeCode", orderTypeCode);
    entities.Obligation.Populated = false;
    Update("CreateObligation2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obId", systemGeneratedIdentifier);
        db.SetInt32(command, "dtyGeneratedId", dtyGeneratedId);
        db.SetNullableString(command, "otherStateAbbr", otherStateAbbr);
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "historyInd", "");
        db.SetNullableString(command, "primSecCd", "");
        db.SetNullableInt32(command, "preConvDebtNo", 0);
        db.SetNullableString(command, "precnvrsnCaseNbr", "");
        db.SetNullableDecimal(command, "aodNadArrBal", 0M);
        db.SetNullableDate(command, "lastPymntDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetString(command, "ordTypCd", orderTypeCode);
      });

    entities.Obligation.CpaType = cpaType;
    entities.Obligation.CspNumber = cspNumber;
    entities.Obligation.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Obligation.DtyGeneratedId = dtyGeneratedId;
    entities.Obligation.CspPNumber = null;
    entities.Obligation.OtherStateAbbr = otherStateAbbr;
    entities.Obligation.Description = description;
    entities.Obligation.PrimarySecondaryCode = "";
    entities.Obligation.CreatedBy = createdBy;
    entities.Obligation.CreatedTmst = createdTmst;
    entities.Obligation.OrderTypeCode = orderTypeCode;
    entities.Obligation.Populated = true;
  }

  private bool ReadCsePerson1()
  {
    entities.AlternateBillLoc.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.AltBillLoc.Number);
      },
      (db, reader) =>
      {
        entities.AlternateBillLoc.Number = db.GetString(reader, 0);
        entities.AlternateBillLoc.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ObligorCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ObligorCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.ObligorCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetString(command, "type", import.HcCpaObligor.Type1);
      },
      (db, reader) =>
      {
        entities.ObligorCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.ObligorCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePersonAccount.CreatedBy = db.GetString(reader, 2);
        entities.ObligorCsePersonAccount.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ObligorCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligorCsePersonAccount.Type1);
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId", import.HcOtVoluntary.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of HcOtrnDtDebtDetail.
    /// </summary>
    [JsonPropertyName("hcOtrnDtDebtDetail")]
    public ObligationTransaction HcOtrnDtDebtDetail
    {
      get => hcOtrnDtDebtDetail ??= new();
      set => hcOtrnDtDebtDetail = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of Discontinue.
    /// </summary>
    [JsonPropertyName("discontinue")]
    public DateWorkArea Discontinue
    {
      get => discontinue ??= new();
      set => discontinue = value;
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

    /// <summary>
    /// A value of AltBillLoc.
    /// </summary>
    [JsonPropertyName("altBillLoc")]
    public CsePersonsWorkSet AltBillLoc
    {
      get => altBillLoc ??= new();
      set => altBillLoc = value;
    }

    private CsePerson obligor;
    private LegalActionPerson hardcodedObligorLap;
    private DateWorkArea max;
    private ObligationType hcOtCVoluntaryClassif;
    private ObligationType hcOt718BUraJudgement;
    private ObligationTransaction hcOtrnDtDebtDetail;
    private ObligationType hcOtCRecoverClassific;
    private ObligationType hcOtCFeesClassificati;
    private CsePersonAccount hcCpaSupportedPerson;
    private DebtDetailStatusHistory hcDdshActiveStatus;
    private ObligationTransaction hcOtrnDtAccrual;
    private ObligationTransaction hcOtrnDtVoluntary;
    private ObligationType hcOtVoluntary;
    private ObligationTransaction hcOtrnDebtType;
    private CsePersonAccount hcCpaObligor;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private DateWorkArea effective;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private DateWorkArea discontinue;
    private Array<GroupGroup> group;
    private CsePersonsWorkSet altBillLoc;
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
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of HardcodeVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodeVoluntary")]
    public ObligationType HardcodeVoluntary
    {
      get => hardcodeVoluntary ??= new();
      set => hardcodeVoluntary = value;
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

    private Infrastructure infrastructure;
    private DateWorkArea max;
    private ObligationType hardcodeVoluntary;
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
    /// A value of AlternateBillLoc.
    /// </summary>
    [JsonPropertyName("alternateBillLoc")]
    public CsePerson AlternateBillLoc
    {
      get => alternateBillLoc ??= new();
      set => alternateBillLoc = value;
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
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
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
    /// A value of CreateObligor.
    /// </summary>
    [JsonPropertyName("createObligor")]
    public CsePersonAccount CreateObligor
    {
      get => createObligor ??= new();
      set => createObligor = value;
    }

    private CsePerson alternateBillLoc;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount obligorCsePersonAccount;
    private CsePerson obligorCsePerson;
    private CsePersonAccount createObligor;
  }
#endregion
}
