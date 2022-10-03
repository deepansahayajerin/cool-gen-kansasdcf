// Program: FN_DELETE_OBLIGATION, ID: 372084590, model: 746.
// Short name: SWE00394
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
/// A program: FN_DELETE_OBLIGATION.
/// </para>
/// <para>
/// This action block will delete an obligation.
/// The delete will occur only if the obligation is not active. An obligation is
/// considered active if the create date is not the current date, or if there
/// are any debt adjustments or collections that have been applied to any of the
/// obligation transactions for the obligation.
/// </para>
/// </summary>
[Serializable]
public partial class FnDeleteObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteObligation.
  /// </summary>
  public FnDeleteObligation(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ==================================================
    // ==						==
    // ==         This is used by OACC Only		==
    // ==						==
    // ==================================================
    // ****************************************************
    // ***  9-3-98  B Adams     Deleted fn-hardcoded-debt-distribution;
    // ***			 imported values
    // ***		 	Changed use of obligation-rln-rsn value of
    // ***			'concurrent' to 'joint / several'
    // ***
    // ***  3/27/99  B Adams  -  Read properties set
    // ***
    // ***  6/8/99  B Adams  -  Moved FN_Raise_Event from the end
    // ***  to within the R/E Ob_Trans actions and added CSE_Person
    // ***  to it.  Prior to this, deleted Events were being created for
    // ***  every person with a Case_Role on the Case.  We only
    // ***  want Events for the supported persons on the debt being
    // ***  deleted.
    // ***
    // ***  8/31/99 - b adams  -  removed read of obligation_type
    // ****************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadObligation1())
    {
      // *** The Obligation is created on the same day; so DELETE is permitted. 
      // No other restrictions apply ***
      if (Equal(Date(entities.Obligation.CreatedTmst), import.Current.Date))
      {
      }
      else
      {
        UseFnCheckObligationForActivity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(local.ObligationActive.Flag) == 'Y')
        {
          ExitState = "FN0000_OBLIG_ACT_DEL_NOT_ALLOWED";

          return;
        }
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    // *****************************************************************
    // The hardcode value that WAS being used here was the one
    // defined as 'concurrent'; but that is an obsolete, invalid
    // value for obligation-rln-rsn.  Changed to 'joint / several'
    // 9-3-98  Bud Adams
    // *****************************************************************
    if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
    {
      if (!ReadObligation3())
      {
        if (!ReadObligation2())
        {
          ExitState = "FN0000_CONCURRENT_OBLIGOR_NF";

          return;
        }
      }

      local.ConcurrentObligorPresent.Flag = "Y";
    }
    else if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'P' || AsChar
      (entities.Obligation.PrimarySecondaryCode) == 'S')
    {
      // =================================================
      // 1/3/00 - bud adams  -  PR# 83291: If the obligaiton being
      //   deleted is either a Primary or Secondary, disassociate
      //   the other obligation and change its Primary_Secondary_
      //   Code to SPACES.
      //   These Reads are not done as a join because primary_
      //   secondary obligation is going to be retrieved with a cursor
      //   - because that's how IEF does Updates.
      //   NOTE: The Obligation_Rln record will be deleted by the RI
      //   trigger as soon as the Obligation is deleted.
      // =================================================
      local.HcOrrPrimarySecondary.SequentialGeneratedIdentifier = 1;

      if (ReadObligationRln1())
      {
        if (!ReadObligation5())
        {
          ExitState = "FN0000_PRIMARY_SECONDRY_OBLIG_NF";
        }
      }
      else if (ReadObligationRln2())
      {
        if (!ReadObligation4())
        {
          ExitState = "FN0000_PRIMARY_SECONDRY_OBLIG_NF";
        }
      }
      else
      {
        ExitState = "FN0000_OBLIG_RLN_NF_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      try
      {
        UpdateObligation();

        // =================================================
        // 1/3/00 - b adams  -  NOTE: The Obligation_Rln record will be
        //   deleted by the RI trigger when the related Obligation is
        //   deleted.  When the Obligation_Rln record is deleted, the
        //   second Obligation will be disassociated from it.  (Happy
        //   Y2K New Year!)
        // =================================================
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGATION_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      local.ConcurrentObligorPresent.Flag = "N";

      // ***---  end of Primary / Secondary processing
    }
    else
    {
      // : No related obligations - Continue Processing
      local.ConcurrentObligorPresent.Flag = "N";
    }

    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.ReferenceDate = import.Current.Date;
    local.Infrastructure.CsePersonNumber = import.Obligor.Number;
    local.Infrastructure.EventId = 47;
    local.Infrastructure.BusinessObjectCd = "OBL";
    local.Infrastructure.DenormNumeric12 =
      import.Obligation.SystemGeneratedIdentifier;
    local.Infrastructure.DenormText12 = import.ObligationType.Code;
    local.Infrastructure.UserId = "OACC";
    local.ActivityType.Text4 = "DELE";

    // =================================================
    // 6/8/99 - Bud Adams  -  The supported person is necessary
    //   to generate the proper Events.
    // =================================================
    // <<< DELETE THE DEBT_DETAILS AND THEN THE DEBTS AND OBLIGATION>>>
    foreach(var item in ReadObligationTransactionCsePerson1())
    {
      UseFnCabRaiseEvent2();

      if (ReadDebtDetail1())
      {
        DeleteDebtDetail();
      }
      else if (ReadAccrualInstructions1())
      {
        // =================================================
        // 2/3/1999 - B Adams  -  Explicit delete of Accrual_Suspension
        //   added because DB2 has this relationship defined with a
        //   DELETE RESTRICT rule, over-riding the CASCADE DELETE
        //   rule defined in the data model.
        // =================================================
        foreach(var item1 in ReadAccrualSuspension())
        {
          DeleteAccrualSuspension();
        }

        DeleteAccrualInstructions();
      }

      DeleteObligationTransaction1();
    }

    // =================================================
    // PR# 241: 9/1/99 - bud adams  -  Obligation is deleted when
    //   the last Obligation_Transaction is deleted due to the IEF
    //   delete rules.  This is always going to be the case here; all
    //   Ob_Trans will be deleted.  No sense incurring the overhead
    //   of executing the delete trigger.
    // =================================================
    if (AsChar(local.ConcurrentObligorPresent.Flag) == 'Y')
    {
      foreach(var item in ReadObligationTransactionCsePerson2())
      {
        UseFnCabRaiseEvent1();

        if (ReadDebtDetail2())
        {
          DeleteDebtDetail();
        }
        else if (ReadAccrualInstructions2())
        {
          // =================================================
          // 2/3/1999 - B Adams  -  Explicit delete of Accrual_Suspension
          //   added because DB2 has this relationship defined with a
          //   DELETE RESTRICT rule, over-riding the CASCADE DELETE
          //   rule defined in the data model.
          // =================================================
          foreach(var item1 in ReadAccrualSuspension())
          {
            DeleteAccrualSuspension();
          }

          DeleteAccrualInstructions();
        }

        DeleteObligationTransaction2();
      }

      DeleteObligation();
    }
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private void UseFnCabRaiseEvent1()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    useImport.Supported.Number = entities.Supported.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.ConcurrentObligation.SystemGeneratedIdentifier;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      entities.ConcurrentObligationTransaction.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(import.ObligationType);
    useImport.Current.Timestamp = import.Current.Timestamp;
    useImport.ActivityType.Text4 = local.ActivityType.Text4;
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseFnCabRaiseEvent2()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    useImport.ActivityType.Text4 = local.ActivityType.Text4;
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.Current.Timestamp = import.Current.Timestamp;
    useImport.ObligationType.Assign(import.ObligationType);
    useImport.Supported.Number = entities.Supported.Number;
    MoveObligationTransaction(entities.ObligationTransaction,
      useImport.ObligationTransaction);
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseFnCheckObligationForActivity()
  {
    var useImport = new FnCheckObligationForActivity.Import();
    var useExport = new FnCheckObligationForActivity.Export();

    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      import.HcOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.HcCpaObligor.Type1 = import.HcCpaObligor.Type1;
    useImport.P.Assign(entities.Obligation);

    Call(FnCheckObligationForActivity.Execute, useImport, useExport);

    entities.Obligation.Assign(useImport.P);
    local.ObligationActive.Flag = useExport.ActiveObligation.Flag;
  }

  private void DeleteAccrualInstructions()
  {
    Update("DeleteAccrualInstructions",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.AccrualInstructions.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.AccrualInstructions.OtrGeneratedId);
      });
  }

  private void DeleteAccrualSuspension()
  {
    Update("DeleteAccrualSuspension",
      (db, command) =>
      {
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
  }

  private void DeleteDebtDetail()
  {
    Update("DeleteDebtDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId", entities.DebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
      });
  }

  private void DeleteObligation()
  {
    var cpaType = entities.ConcurrentObligation.CpaType;
    var cspNumber = entities.ConcurrentObligation.CspNumber;
    bool exists;

    exists = Read("DeleteObligation#1",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_OBG\".", "50001");
        
    }

    exists = Read("DeleteObligation#2",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ADMIN_APPEAL\".",
        "50001");
    }

    exists = Read("DeleteObligation#3",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_OBG_AA\".",
        "50001");
    }

    Update("DeleteObligation#4",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#5",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#6",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#7",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#8",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#9",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#10",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#11",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#12",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#13",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#14",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#15",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#16",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#17",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#18",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#19",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#20",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#21",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#22",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#23",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#24",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#25",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    Update("DeleteObligation#26",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
      });

    exists = Read("DeleteObligation#27",
      (db, command) =>
      {
        db.SetString(command, "cpaType2", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
      },
      null);

    if (!exists)
    {
      Update("DeleteObligation#28",
        (db, command) =>
        {
          db.SetString(command, "cpaType2", cpaType);
          db.SetString(command, "cspNumber", cspNumber);
        });
    }
  }

  private void DeleteObligationTransaction1()
  {
    var obgGeneratedId = entities.ObligationTransaction.ObgGeneratedId;
    var cspNumber = entities.ObligationTransaction.CspNumber;
    var cpaType = entities.ObligationTransaction.CpaType;
    var otyType = entities.ObligationTransaction.OtyType;
    bool exists;

    Update("DeleteObligationTransaction1#1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "obgGeneratedId1", obgGeneratedId);
        db.SetNullableString(command, "cspRNumber", cspNumber);
        db.SetNullableString(command, "cpaRType", cpaType);
        db.SetNullableInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "otrType", entities.ObligationTransaction.Type1);
        db.SetNullableInt32(command, "otyType1", otyType);
      });

    Update("DeleteObligationTransaction1#2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "obgGeneratedId1", obgGeneratedId);
        db.SetNullableString(command, "cspRNumber", cspNumber);
        db.SetNullableString(command, "cpaRType", cpaType);
        db.SetNullableInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "otrType", entities.ObligationTransaction.Type1);
        db.SetNullableInt32(command, "otyType1", otyType);
      });

    exists = Read("DeleteObligationTransaction1#3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cpaSupType", entities.ObligationTransaction.CpaSupType ?? ""
          );
        db.SetNullableString(
          command, "cspSupNumber",
          entities.ObligationTransaction.CspSupNumber ?? "");
      },
      null);

    if (!exists)
    {
      Update("DeleteObligationTransaction1#4",
        (db, command) =>
        {
          db.SetNullableString(
            command, "cpaSupType",
            entities.ObligationTransaction.CpaSupType ?? "");
          db.SetNullableString(
            command, "cspSupNumber",
            entities.ObligationTransaction.CspSupNumber ?? "");
        });
    }

    exists = Read("DeleteObligationTransaction1#5",
      (db, command) =>
      {
        db.SetInt32(command, "otyType2", otyType);
        db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
      },
      null);

    if (!exists)
    {
      Update("DeleteObligationTransaction1#6",
        (db, command) =>
        {
          db.SetInt32(command, "otyType2", otyType);
          db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
          db.SetString(command, "cspNumber", cspNumber);
          db.SetString(command, "cpaType", cpaType);
        });

      exists = Read("DeleteObligationTransaction1#7",
        (db, command) =>
        {
          db.SetInt32(command, "otyType2", otyType);
          db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
          db.SetString(command, "cspNumber", cspNumber);
          db.SetString(command, "cpaType", cpaType);
        },
        null);

      if (!exists)
      {
        Update("DeleteObligationTransaction1#8",
          (db, command) =>
          {
            db.SetString(command, "cpaType", cspNumber);
            db.SetString(command, "cspNumber", cpaType);
          });
      }
    }
  }

  private void DeleteObligationTransaction2()
  {
    var obgGeneratedId =
      entities.ConcurrentObligationTransaction.ObgGeneratedId;
    var cspNumber = entities.ConcurrentObligationTransaction.CspNumber;
    var cpaType = entities.ConcurrentObligationTransaction.CpaType;
    var otyType = entities.ConcurrentObligationTransaction.OtyType;
    bool exists;

    Update("DeleteObligationTransaction2#1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "obgGeneratedId1", obgGeneratedId);
        db.SetNullableString(command, "cspRNumber", cspNumber);
        db.SetNullableString(command, "cpaRType", cpaType);
        db.SetNullableInt32(
          command, "otrGeneratedId",
          entities.ConcurrentObligationTransaction.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "otrType", entities.ConcurrentObligationTransaction.Type1);
        db.SetNullableInt32(command, "otyType1", otyType);
      });

    Update("DeleteObligationTransaction2#2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "obgGeneratedId1", obgGeneratedId);
        db.SetNullableString(command, "cspRNumber", cspNumber);
        db.SetNullableString(command, "cpaRType", cpaType);
        db.SetNullableInt32(
          command, "otrGeneratedId",
          entities.ConcurrentObligationTransaction.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "otrType", entities.ConcurrentObligationTransaction.Type1);
        db.SetNullableInt32(command, "otyType1", otyType);
      });

    exists = Read("DeleteObligationTransaction2#3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cpaSupType",
          entities.ConcurrentObligationTransaction.CpaSupType ?? "");
        db.SetNullableString(
          command, "cspSupNumber",
          entities.ConcurrentObligationTransaction.CspSupNumber ?? "");
      },
      null);

    if (!exists)
    {
      Update("DeleteObligationTransaction2#4",
        (db, command) =>
        {
          db.SetNullableString(
            command, "cpaSupType",
            entities.ConcurrentObligationTransaction.CpaSupType ?? "");
          db.SetNullableString(
            command, "cspSupNumber",
            entities.ConcurrentObligationTransaction.CspSupNumber ?? "");
        });
    }

    exists = Read("DeleteObligationTransaction2#5",
      (db, command) =>
      {
        db.SetInt32(command, "otyType2", otyType);
        db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
      },
      null);

    if (!exists)
    {
      Update("DeleteObligationTransaction2#6",
        (db, command) =>
        {
          db.SetInt32(command, "otyType2", otyType);
          db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
          db.SetString(command, "cspNumber", cspNumber);
          db.SetString(command, "cpaType", cpaType);
        });

      exists = Read("DeleteObligationTransaction2#7",
        (db, command) =>
        {
          db.SetInt32(command, "otyType2", otyType);
          db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
          db.SetString(command, "cspNumber", cspNumber);
          db.SetString(command, "cpaType", cpaType);
        },
        null);

      if (!exists)
      {
        Update("DeleteObligationTransaction2#8",
          (db, command) =>
          {
            db.SetString(command, "cpaType", cspNumber);
            db.SetString(command, "cspNumber", cpaType);
          });
      }
    }
  }

  private bool ReadAccrualInstructions1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions1",
      (db, command) =>
      {
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
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadAccrualInstructions2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ConcurrentObligationTransaction.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions2",
      (db, command) =>
      {
        db.SetString(
          command, "otrType", entities.ConcurrentObligationTransaction.Type1);
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ConcurrentObligationTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cpaType", entities.ConcurrentObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber",
          entities.ConcurrentObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ConcurrentObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private IEnumerable<bool> ReadAccrualSuspension()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.AccrualSuspension.Populated = false;

    return ReadEach("ReadAccrualSuspension",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgId", entities.AccrualInstructions.ObgGeneratedId);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 1);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 2);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 3);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 4);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 5);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 6);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);

        return true;
      });
  }

  private bool ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

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
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ConcurrentObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ConcurrentObligationTransaction.OtyType);
          
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ConcurrentObligationTransaction.ObgGeneratedId);
        db.SetString(
          command, "otrType", entities.ConcurrentObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ConcurrentObligationTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cpaType", entities.ConcurrentObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber",
          entities.ConcurrentObligationTransaction.CspNumber);
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
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadObligation1()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Description = db.GetNullableString(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
      });
  }

  private bool ReadObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ConcurrentObligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "orrGeneratedId",
          import.HcOrrJointAndSeveral.SequentialGeneratedIdentifier);
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ConcurrentObligation.CpaType = db.GetString(reader, 0);
        entities.ConcurrentObligation.CspNumber = db.GetString(reader, 1);
        entities.ConcurrentObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ConcurrentObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ConcurrentObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ConcurrentObligation.CpaType);
          
      });
  }

  private bool ReadObligation3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ConcurrentObligation.Populated = false;

    return Read("ReadObligation3",
      (db, command) =>
      {
        db.SetInt32(
          command, "orrGeneratedId",
          import.HcOrrJointAndSeveral.SequentialGeneratedIdentifier);
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ConcurrentObligation.CpaType = db.GetString(reader, 0);
        entities.ConcurrentObligation.CspNumber = db.GetString(reader, 1);
        entities.ConcurrentObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ConcurrentObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ConcurrentObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ConcurrentObligation.CpaType);
          
      });
  }

  private bool ReadObligation4()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationRln.Populated);
    entities.PrimarySecondary.Populated = false;

    return Read("ReadObligation4",
      (db, command) =>
      {
        db.
          SetInt32(command, "dtyGeneratedId", entities.ObligationRln.OtyFirstId);
          
        db.SetInt32(command, "obId", entities.ObligationRln.ObgFGeneratedId);
        db.SetString(command, "cspNumber", entities.ObligationRln.CspFNumber);
        db.SetString(command, "cpaType", entities.ObligationRln.CpaFType);
      },
      (db, reader) =>
      {
        entities.PrimarySecondary.CpaType = db.GetString(reader, 0);
        entities.PrimarySecondary.CspNumber = db.GetString(reader, 1);
        entities.PrimarySecondary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PrimarySecondary.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.PrimarySecondary.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.PrimarySecondary.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.PrimarySecondary.LastUpdateTmst =
          db.GetNullableDateTime(reader, 6);
        entities.PrimarySecondary.Populated = true;
        CheckValid<Obligation>("CpaType", entities.PrimarySecondary.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.PrimarySecondary.PrimarySecondaryCode);
      });
  }

  private bool ReadObligation5()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationRln.Populated);
    entities.PrimarySecondary.Populated = false;

    return Read("ReadObligation5",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId", entities.ObligationRln.OtySecondId);
        db.SetInt32(command, "obId", entities.ObligationRln.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.ObligationRln.CspNumber);
        db.SetString(command, "cpaType", entities.ObligationRln.CpaType);
      },
      (db, reader) =>
      {
        entities.PrimarySecondary.CpaType = db.GetString(reader, 0);
        entities.PrimarySecondary.CspNumber = db.GetString(reader, 1);
        entities.PrimarySecondary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PrimarySecondary.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.PrimarySecondary.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.PrimarySecondary.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.PrimarySecondary.LastUpdateTmst =
          db.GetNullableDateTime(reader, 6);
        entities.PrimarySecondary.Populated = true;
        CheckValid<Obligation>("CpaType", entities.PrimarySecondary.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.PrimarySecondary.PrimarySecondaryCode);
      });
  }

  private bool ReadObligationRln1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln1",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "orrGeneratedId",
          local.HcOrrPrimarySecondary.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.CreatedBy = db.GetString(reader, 7);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 8);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 9);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private bool ReadObligationRln2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln2",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "orrGeneratedId",
          local.HcOrrPrimarySecondary.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.CreatedBy = db.GetString(reader, 7);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 8);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 9);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private IEnumerable<bool> ReadObligationTransactionCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Supported.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransactionCsePerson1",
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
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.Supported.Number = db.GetString(reader, 8);
        entities.Supported.Type1 = db.GetString(reader, 9);
        entities.Supported.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<CsePerson>("Type1", entities.Supported.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);
    entities.Supported.Populated = false;
    entities.ConcurrentObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransactionCsePerson2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ConcurrentObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ConcurrentObligation.CspNumber);
        db.SetString(command, "cpaType", entities.ConcurrentObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ConcurrentObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 0);
        entities.ConcurrentObligationTransaction.CspNumber =
          db.GetString(reader, 1);
        entities.ConcurrentObligationTransaction.CpaType =
          db.GetString(reader, 2);
        entities.ConcurrentObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ConcurrentObligationTransaction.Type1 =
          db.GetString(reader, 4);
        entities.ConcurrentObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ConcurrentObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ConcurrentObligationTransaction.OtyType =
          db.GetInt32(reader, 7);
        entities.Supported.Number = db.GetString(reader, 8);
        entities.Supported.Type1 = db.GetString(reader, 9);
        entities.Supported.Populated = true;
        entities.ConcurrentObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ConcurrentObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ConcurrentObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ConcurrentObligationTransaction.CpaSupType);
        CheckValid<CsePerson>("Type1", entities.Supported.Type1);

        return true;
      });
  }

  private void UpdateObligation()
  {
    System.Diagnostics.Debug.Assert(entities.PrimarySecondary.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.Current.Timestamp;

    CheckValid<Obligation>("PrimarySecondaryCode", "");
    entities.PrimarySecondary.Populated = false;
    Update("UpdateObligation",
      (db, command) =>
      {
        db.SetNullableString(command, "primSecCd", "");
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.PrimarySecondary.CpaType);
        db.SetString(command, "cspNumber", entities.PrimarySecondary.CspNumber);
        db.SetInt32(
          command, "obId", entities.PrimarySecondary.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId", entities.PrimarySecondary.DtyGeneratedId);
      });

    entities.PrimarySecondary.PrimarySecondaryCode = "";
    entities.PrimarySecondary.LastUpdatedBy = lastUpdatedBy;
    entities.PrimarySecondary.LastUpdateTmst = lastUpdateTmst;
    entities.PrimarySecondary.Populated = true;
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
    /// A value of HcOrrJointAndSeveral.
    /// </summary>
    [JsonPropertyName("hcOrrJointAndSeveral")]
    public ObligationRlnRsn HcOrrJointAndSeveral
    {
      get => hcOrrJointAndSeveral ??= new();
      set => hcOrrJointAndSeveral = value;
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
    /// A value of HcOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hcOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HcOtrrConcurrentObliga
    {
      get => hcOtrrConcurrentObliga ??= new();
      set => hcOtrrConcurrentObliga = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private ObligationRlnRsn hcOrrJointAndSeveral;
    private CsePersonAccount hcCpaObligor;
    private ObligationTransactionRlnRsn hcOtrrConcurrentObliga;
    private CsePerson obligor;
    private ObligationType obligationType;
    private Obligation obligation;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HcOrrPrimarySecondary.
    /// </summary>
    [JsonPropertyName("hcOrrPrimarySecondary")]
    public ObligationRlnRsn HcOrrPrimarySecondary
    {
      get => hcOrrPrimarySecondary ??= new();
      set => hcOrrPrimarySecondary = value;
    }

    /// <summary>
    /// A value of ActivityType.
    /// </summary>
    [JsonPropertyName("activityType")]
    public TextWorkArea ActivityType
    {
      get => activityType ??= new();
      set => activityType = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorPresent.
    /// </summary>
    [JsonPropertyName("concurrentObligorPresent")]
    public Common ConcurrentObligorPresent
    {
      get => concurrentObligorPresent ??= new();
      set => concurrentObligorPresent = value;
    }

    /// <summary>
    /// A value of ObligationActive.
    /// </summary>
    [JsonPropertyName("obligationActive")]
    public Common ObligationActive
    {
      get => obligationActive ??= new();
      set => obligationActive = value;
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
    /// A value of HardcodeDebt.
    /// </summary>
    [JsonPropertyName("hardcodeDebt")]
    public ObligationTransaction HardcodeDebt
    {
      get => hardcodeDebt ??= new();
      set => hardcodeDebt = value;
    }

    private ObligationRlnRsn hcOrrPrimarySecondary;
    private TextWorkArea activityType;
    private Common concurrentObligorPresent;
    private Common obligationActive;
    private Infrastructure infrastructure;
    private ObligationTransaction hardcodeDebt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PrimarySecondary.
    /// </summary>
    [JsonPropertyName("primarySecondary")]
    public Obligation PrimarySecondary
    {
      get => primarySecondary ??= new();
      set => primarySecondary = value;
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
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
    }

    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
    }

    /// <summary>
    /// A value of ObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationRlnRsn")]
    public ObligationRlnRsn ObligationRlnRsn
    {
      get => obligationRlnRsn ??= new();
      set => obligationRlnRsn = value;
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
    /// A value of ConcurrentObligationTransaction.
    /// </summary>
    [JsonPropertyName("concurrentObligationTransaction")]
    public ObligationTransaction ConcurrentObligationTransaction
    {
      get => concurrentObligationTransaction ??= new();
      set => concurrentObligationTransaction = value;
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

    private Obligation primarySecondary;
    private AccrualInstructions accrualInstructions;
    private AccrualSuspension accrualSuspension;
    private CsePerson obligor;
    private CsePerson supported;
    private CsePersonAccount csePersonAccount;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private Obligation concurrentObligation;
    private ObligationRln obligationRln;
    private ObligationRlnRsn obligationRlnRsn;
    private ObligationTransaction obligationTransaction;
    private ObligationTransaction concurrentObligationTransaction;
    private ObligationType obligationType;
  }
#endregion
}
