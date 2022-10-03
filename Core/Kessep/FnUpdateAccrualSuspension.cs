// Program: FN_UPDATE_ACCRUAL_SUSPENSION, ID: 372082710, model: 746.
// Short name: SWE00513
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_ACCRUAL_SUSPENSION.
/// </summary>
[Serializable]
public partial class FnUpdateAccrualSuspension: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_ACCRUAL_SUSPENSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateAccrualSuspension(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateAccrualSuspension.
  /// </summary>
  public FnUpdateAccrualSuspension(IContext context, Import import,
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
    // *** 09/01/98  Bud Adams     Added import current timestamp ***
    // =================================================
    // 7/22/99 - bud adams  -  Read of Obligation_Transaction was
    //   not properly qualified.
    // =================================================
    // : Jan, 2002, M. Brown, Retro Processing, WO#010504.
    //   If an update will cause new debts to be created, and there are AF, FC, 
    // NF, or NC
    //   collections, issue a message asking if collection protection should be 
    // created for
    //   existing payments.  If the answer is yes, set up a collection 
    // protection timeframe
    //   with dates from earliest to most recent AF, FC, NF or NC collections, 
    // and set the
    //   distribution method on these collections to 'P'rotected.
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    MoveAccrualSuspension(import.AccrualSuspension, export.AccrualSuspension);

    // : Jan, 2002, M. Brown, Retro Processing, WO#010504.
    //   Added read of obligation, since obligation is used in retro processing 
    // code.
    if (!ReadObligationTransactionObligation())
    {
      ExitState = "FN0000_OBLIG_TRANS_NF";

      return;
    }

    // *****
    // Validate that dates do not overlap with another suspension.
    // *****
    // --------------------------------------------------------------------------------
    // PR# 159544: The above code is commented since the logic the check 
    // overlapping time frames is not working.
    //  Do not comment the code below. There is logic in using the Max_dates. 
    // Check before removing this logic.
    //                                                           
    // Vithal (10/11/2002)
    // ---------------------------------------------------------------------------------
    if (AsChar(import.DatesChanged.Flag) == 'Y')
    {
      local.Max.Date = import.AccrualSuspension.ResumeDt;
      local.Max.Date = UseCabSetMaximumDiscontinueDate();

      if (ReadAccrualSuspension())
      {
        ExitState = "ACO_NE0000_DATE_OVERLAP";

        return;
      }
    }

    if (ReadAccrualSuspensionAccrualInstructions())
    {
      MoveAccrualSuspension(entities.AccrualSuspension, export.AccrualSuspension);
        

      if (!Lt(import.AccrualSuspension.SuspendDt,
        entities.AccrualInstructions.DiscontinueDt))
      {
        ExitState = "FN0000_SUSP_DT_AFTER_DISCNT_DATE";

        return;
      }

      if (IsEmpty(import.CollProtAnswer.SelectChar))
      {
        // : Jan, 2002, M. Brown, Retro Processing, WO#010504.
        //   Check to see if this is an 'XA' (arrears-only obligation, with 
        // collections
        //   applied to AF, NF, FC or NC).  If so we will check with the user to
        // see if they
        //   want collections protected.
        if ((Lt(
          entities.AccrualSuspension.SuspendDt,
          import.AccrualSuspension.SuspendDt) || Lt
          (import.AccrualSuspension.ResumeDt,
          entities.AccrualSuspension.ResumeDt)) && Lt
          (export.AccrualSuspension.SuspendDt, import.Current.Date))
        {
          if (ReadCollection())
          {
            ExitState = "FN0000_Y_OR_N_THEN_PF15";

            return;
          }
          else
          {
            // : OK
          }
        }
      }
      else if (AsChar(import.CollProtAnswer.SelectChar) == 'Y' || AsChar
        (import.CollProtAnswer.SelectChar) == 'N')
      {
        // : OK, continue
      }
      else
      {
        ExitState = "FN0000_Y_OR_N_THEN_PF15";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_ACCRUAL_SUSPENSION_NF";

      return;
    }

    if (AsChar(import.CollProtAnswer.SelectChar) == 'Y')
    {
      // : Jan, 2002, M. Brown, Retro Processing, WO#010504.
      //    The user has confirmed that collections should be protected.
      local.ObligCollProtectionHist.ReasonText = "Accrual Suspension update.";

      if (IsEmpty(import.LegalAction.StandardNumber))
      {
        if (ReadLegalAction())
        {
          local.LegalAction.StandardNumber =
            entities.LegalAction.StandardNumber;
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF";

          return;
        }
      }
      else
      {
        local.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
      }

      if (Lt(entities.AccrualSuspension.SuspendDt,
        import.AccrualSuspension.SuspendDt) && Lt
        (entities.AccrualSuspension.SuspendDt, import.Current.Date))
      {
        local.ObligCollProtectionHist.CvrdCollStrtDt =
          entities.AccrualSuspension.SuspendDt;

        if (Lt(import.Current.Date, import.AccrualSuspension.SuspendDt))
        {
          local.ObligCollProtectionHist.CvrdCollEndDt = import.Current.Date;
        }
        else
        {
          local.ObligCollProtectionHist.CvrdCollEndDt =
            import.AccrualSuspension.SuspendDt;
        }

        foreach(var item in ReadObligation())
        {
          UseFnProtectCollectionsForOblig();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }

      if (Lt(import.AccrualSuspension.ResumeDt,
        entities.AccrualSuspension.ResumeDt) && Lt
        (import.AccrualSuspension.ResumeDt, import.Current.Date))
      {
        local.ObligCollProtectionHist.CvrdCollStrtDt =
          import.AccrualSuspension.ResumeDt;

        if (Lt(entities.AccrualSuspension.ResumeDt, import.Current.Date))
        {
          local.ObligCollProtectionHist.CvrdCollEndDt =
            entities.AccrualSuspension.ResumeDt;
        }
        else
        {
          local.ObligCollProtectionHist.CvrdCollEndDt = import.Current.Date;
        }

        foreach(var item in ReadObligation())
        {
          UseFnProtectCollectionsForOblig();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }
    }

    try
    {
      UpdateAccrualSuspension();
      MoveAccrualSuspension(entities.AccrualSuspension, export.AccrualSuspension);
        
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_ACCRUAL_SUSPENSION_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_ACCRUAL_SUSPENSION_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveAccrualSuspension(AccrualSuspension source,
    AccrualSuspension target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SuspendDt = source.SuspendDt;
    target.ResumeDt = source.ResumeDt;
    target.ReductionPercentage = source.ReductionPercentage;
    target.ReasonTxt = source.ReasonTxt;
    target.ReductionAmount = source.ReductionAmount;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnProtectCollectionsForOblig()
  {
    var useImport = new FnProtectCollectionsForOblig.Import();
    var useExport = new FnProtectCollectionsForOblig.Export();

    useImport.Persistent.Assign(entities.OtherView);
    useImport.CreateObCollProtHist.Flag = local.Common.Flag;
    useImport.ObligCollProtectionHist.Assign(local.ObligCollProtectionHist);

    Call(FnProtectCollectionsForOblig.Execute, useImport, useExport);

    entities.OtherView.Assign(useImport.Persistent);
    local.ObCollProtHistCreated.Flag = useExport.ObCollProtHistCreated.Flag;
    local.CollsFndToProtect.Flag = useExport.CollsFndToProtect.Flag;
  }

  private bool ReadAccrualSuspension()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.AccrualSuspension.Populated = false;

    return Read("ReadAccrualSuspension",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgId", entities.ObligationTransaction.ObgGeneratedId);
        db.SetDate(
          command, "suspendDt1",
          import.AccrualSuspension.SuspendDt.GetValueOrDefault());
        db.SetDate(command, "suspendDt2", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "frqSuspId",
          import.AccrualSuspension.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.ReductionPercentage =
          db.GetNullableDecimal(reader, 3);
        entities.AccrualSuspension.CreatedTmst = db.GetDateTime(reader, 4);
        entities.AccrualSuspension.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.AccrualSuspension.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 7);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 8);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 9);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 10);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 11);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 12);
        entities.AccrualSuspension.ReductionAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AccrualSuspension.ReasonTxt = db.GetNullableString(reader, 14);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);
      });
  }

  private bool ReadAccrualSuspensionAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.AccrualSuspension.Populated = false;
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualSuspensionAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(
          command, "frqSuspId",
          import.AccrualSuspension.SystemGeneratedIdentifier);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.ReductionPercentage =
          db.GetNullableDecimal(reader, 3);
        entities.AccrualSuspension.CreatedTmst = db.GetDateTime(reader, 4);
        entities.AccrualSuspension.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.AccrualSuspension.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 7);
        entities.AccrualInstructions.OtrType = db.GetString(reader, 7);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 8);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 8);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 9);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 9);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 10);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 10);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 11);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 11);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 12);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 12);
        entities.AccrualSuspension.ReductionAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AccrualSuspension.ReasonTxt = db.GetNullableString(reader, 14);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 15);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 16);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 17);
        entities.AccrualSuspension.Populated = true;
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetDate(
          command, "suspendDt",
          entities.AccrualSuspension.SuspendDt.GetValueOrDefault());
        db.SetNullableDate(
          command, "resumeDt",
          entities.AccrualSuspension.ResumeDt.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
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
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 16);
        entities.Collection.DistributionMethod = db.GetString(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
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
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.OtherView.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OtherView.CpaType = db.GetString(reader, 0);
        entities.OtherView.CspNumber = db.GetString(reader, 1);
        entities.OtherView.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.OtherView.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.OtherView.LgaId = db.GetNullableInt32(reader, 4);
        entities.OtherView.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.OtherView.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.OtherView.LastUpdateTmst = db.GetNullableDateTime(reader, 7);
        entities.OtherView.LastObligationEvent =
          db.GetNullableString(reader, 8);
        entities.OtherView.Populated = true;
        CheckValid<Obligation>("CpaType", entities.OtherView.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.OtherView.PrimarySecondaryCode);

        return true;
      });
  }

  private bool ReadObligationTransactionObligation()
  {
    entities.Obligation.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransactionObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnId",
          import.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 8);
        entities.Obligation.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private void UpdateAccrualSuspension()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualSuspension.Populated);

    var suspendDt = import.AccrualSuspension.SuspendDt;
    var resumeDt = import.AccrualSuspension.ResumeDt;
    var reductionPercentage =
      import.AccrualSuspension.ReductionPercentage.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = import.Current.Timestamp;
    var reductionAmount =
      import.AccrualSuspension.ReductionAmount.GetValueOrDefault();
    var reasonTxt = import.AccrualSuspension.ReasonTxt ?? "";

    entities.AccrualSuspension.Populated = false;
    Update("UpdateAccrualSuspension",
      (db, command) =>
      {
        db.SetDate(command, "suspendDt", suspendDt);
        db.SetNullableDate(command, "resumeDt", resumeDt);
        db.SetNullableDecimal(command, "redPct", reductionPercentage);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "reductionAmount", reductionAmount);
        db.SetNullableString(command, "frqSuspRsnTxt", reasonTxt);
        db.SetInt32(
          command, "frqSuspId",
          entities.AccrualSuspension.SystemGeneratedIdentifier);
        db.SetString(command, "otrType", entities.AccrualSuspension.OtrType);
        db.SetInt32(command, "otyId", entities.AccrualSuspension.OtyId);
        db.SetInt32(command, "obgId", entities.AccrualSuspension.ObgId);
        db.
          SetString(command, "cspNumber", entities.AccrualSuspension.CspNumber);
          
        db.SetString(command, "cpaType", entities.AccrualSuspension.CpaType);
        db.SetInt32(command, "otrId", entities.AccrualSuspension.OtrId);
      });

    entities.AccrualSuspension.SuspendDt = suspendDt;
    entities.AccrualSuspension.ResumeDt = resumeDt;
    entities.AccrualSuspension.ReductionPercentage = reductionPercentage;
    entities.AccrualSuspension.LastUpdatedBy = lastUpdatedBy;
    entities.AccrualSuspension.LastUpdatedTmst = lastUpdatedTmst;
    entities.AccrualSuspension.ReductionAmount = reductionAmount;
    entities.AccrualSuspension.ReasonTxt = reasonTxt;
    entities.AccrualSuspension.Populated = true;
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
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of CollProtAnswer.
    /// </summary>
    [JsonPropertyName("collProtAnswer")]
    public Common CollProtAnswer
    {
      get => collProtAnswer ??= new();
      set => collProtAnswer = value;
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
    /// A value of DatesChanged.
    /// </summary>
    [JsonPropertyName("datesChanged")]
    public Common DatesChanged
    {
      get => datesChanged ??= new();
      set => datesChanged = value;
    }

    private CsePersonAccount hcCpaObligor;
    private CsePerson obligor;
    private DateWorkArea current;
    private Obligation obligation;
    private AccrualSuspension accrualSuspension;
    private ObligationTransaction obligationTransaction;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common collProtAnswer;
    private LegalAction legalAction;
    private Common datesChanged;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    private AccrualSuspension accrualSuspension;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ObCollProtHistCreated.
    /// </summary>
    [JsonPropertyName("obCollProtHistCreated")]
    public Common ObCollProtHistCreated
    {
      get => obCollProtHistCreated ??= new();
      set => obCollProtHistCreated = value;
    }

    /// <summary>
    /// A value of CollsFndToProtect.
    /// </summary>
    [JsonPropertyName("collsFndToProtect")]
    public Common CollsFndToProtect
    {
      get => collsFndToProtect ??= new();
      set => collsFndToProtect = value;
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
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private Common obCollProtHistCreated;
    private Common collsFndToProtect;
    private Common common;
    private ObligCollProtectionHist obligCollProtectionHist;
    private DateWorkArea null1;
    private DateWorkArea max;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of OtherView.
    /// </summary>
    [JsonPropertyName("otherView")]
    public Obligation OtherView
    {
      get => otherView ??= new();
      set => otherView = value;
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

    private CsePersonAccount obligorCsePersonAccount;
    private CsePerson obligorCsePerson;
    private Obligation obligation;
    private AccrualSuspension accrualSuspension;
    private ObligationTransaction obligationTransaction;
    private AccrualInstructions accrualInstructions;
    private Collection collection;
    private ObligationTransaction debt;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Obligation otherView;
    private LegalAction legalAction;
  }
#endregion
}
