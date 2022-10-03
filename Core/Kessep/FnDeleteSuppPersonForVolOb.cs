// Program: FN_DELETE_SUPP_PERSON_FOR_VOL_OB, ID: 372100577, model: 746.
// Short name: SWE02161
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DELETE_SUPP_PERSON_FOR_VOL_OB.
/// </summary>
[Serializable]
public partial class FnDeleteSuppPersonForVolOb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_SUPP_PERSON_FOR_VOL_OB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteSuppPersonForVolOb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteSuppPersonForVolOb.
  /// </summary>
  public FnDeleteSuppPersonForVolOb(IContext context, Import import,
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
    // **************************************************
    // *** 09/01/98  Bud Adams	  deleted fn-hardcoded-debt-distribution	***
    // ***			imported values; imp timestamp  ***
    // ***
    // *** 3/27/99  bud adams  -  Read properties set
    // **************************************************
    if (!ReadObligation())
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    // =================================================
    // This Read may return many rows, it doesn't matter.
    // =================================================
    if (ReadCollection())
    {
      ExitState = "FN0000_SUPP_PERSON_HAS_COLL";

      return;
    }

    foreach(var item in ReadObligationTransaction2())
    {
      ++local.Common.Count;

      if (local.Common.Count > 1)
      {
        break;
      }
    }

    if (local.Common.Count == 0)
    {
      // ***---  Deleted test code left over from 1998 - 11/16/99 - b adams
    }
    else if (local.Common.Count == 1)
    {
      // : There is only one supported person and that supported person does not
      // have any collections. So go ahead and delete the obligation itself.
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.ReferenceDate = import.Current.Date;
      local.Infrastructure.CsePersonNumber = import.Obligor.Number;
      local.Infrastructure.EventId = 47;
      local.Infrastructure.UserId = "OVOL";
      local.Infrastructure.BusinessObjectCd = "OBL";
      local.Infrastructure.DenormNumeric12 =
        entities.ObligationTransaction.SystemGeneratedIdentifier;
      local.Infrastructure.DenormText12 = import.HcOtrnTDebt.Type1;
      local.ActivityType.Text4 = "DELE";
      UseFnCabRaiseEvent();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "FN0000_DELETE_SUCCESSFUL";
      }
      else
      {
        return;
      }

      // =================================================
      // 11/16/99 - bud adams  -  Debt_Details are going to be picked
      //   up on a cascade delete when Obligation_Transaction is
      //   deleted, and Obligation_Transaction is going to cascade
      //   when Obligation is deleted.
      // =================================================
      foreach(var item in ReadDebtDetail2())
      {
        DeleteDebtDetail();
      }

      DeleteObligation();
    }
    else
    {
      // : There is more than one supported person and the selected supported 
      // person does not have any collections. So delete the debt detail/
      // obligation transaction associated with the supported person.
      if (!ReadObligationTransaction1())
      {
        ExitState = "OBLIGATION_TRANSACTION_NF";

        return;
      }

      UpdateObligation();

      // ***---  select only; just one d-d per ob-tran for voluntaries
      if (ReadDebtDetail1())
      {
        DeleteDebtDetail();
      }
      else
      {
        ExitState = "FN0000_DEBT_DETAIL_NF_RB";
      }

      // =================================================
      // 11/16/99 - bud adams  -  Debt_Details are going to be picked
      //   up on a cascade delete when Obligation_Transaction is
      //   deleted, and all that checking is going to be done anyway.
      // =================================================
      DeleteObligationTransaction();
    }
  }

  private void UseFnCabRaiseEvent()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    useImport.ActivityType.Text4 = local.ActivityType.Text4;
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.Current.Timestamp = import.Current.Timestamp;

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
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
    var cpaType = entities.Obligation.CpaType;
    var cspNumber = entities.Obligation.CspNumber;
    bool exists;

    exists = Read("DeleteObligation#1",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
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
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
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
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
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
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#5",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#6",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#7",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#8",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#9",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#10",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#11",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#12",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#13",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#14",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#15",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#16",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#17",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#18",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#19",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#20",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#21",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#22",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#23",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#24",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#25",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      });

    Update("DeleteObligation#26",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNo", cspNumber);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
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

  private void DeleteObligationTransaction()
  {
    var obgGeneratedId = entities.ObligationTransaction.ObgGeneratedId;
    var cspNumber = entities.ObligationTransaction.CspNumber;
    var cpaType = entities.ObligationTransaction.CpaType;
    var otyType = entities.ObligationTransaction.OtyType;
    bool exists;

    Update("DeleteObligationTransaction#1",
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

    Update("DeleteObligationTransaction#2",
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

    exists = Read("DeleteObligationTransaction#3",
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
      Update("DeleteObligationTransaction#4",
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

    exists = Read("DeleteObligationTransaction#5",
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
      Update("DeleteObligationTransaction#6",
        (db, command) =>
        {
          db.SetInt32(command, "otyType2", otyType);
          db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
          db.SetString(command, "cspNumber", cspNumber);
          db.SetString(command, "cpaType", cpaType);
        });

      exists = Read("DeleteObligationTransaction#7",
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
        Update("DeleteObligationTransaction#8",
          (db, command) =>
          {
            db.SetString(command, "cpaType", cspNumber);
            db.SetString(command, "cspNumber", cpaType);
          });
      }
    }
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableString(command, "cspSupNumber", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrtType = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 2);
        entities.Collection.CrvId = db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 4);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.Collection.CspNumber = db.GetString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 8);
        entities.Collection.OtrType = db.GetString(reader, 9);
        entities.Collection.OtyId = db.GetInt32(reader, 10);
        entities.Collection.Populated = true;
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
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
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private IEnumerable<bool> ReadDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetString(command, "otrType", import.HcOtrnTDebt.Type1);
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
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
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
          
        db.SetInt32(
          command, "dtyGeneratedId",
          import.HcOtVoluntary.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadObligationTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction1",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
        db.SetString(command, "debtTyp", import.HcOtrnTVoluntary.DebtType);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableString(command, "cspSupNumber", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 5);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 8);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private IEnumerable<bool> ReadObligationTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransaction2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 5);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 8);
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

  private void UpdateObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.Current.Timestamp;

    entities.Obligation.Populated = false;
    Update("UpdateObligation",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.LastUpdatedBy = lastUpdatedBy;
    entities.Obligation.LastUpdateTmst = lastUpdateTmst;
    entities.Obligation.Populated = true;
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
    /// A value of HcOtVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtVoluntary")]
    public ObligationType HcOtVoluntary
    {
      get => hcOtVoluntary ??= new();
      set => hcOtVoluntary = value;
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
    /// A value of HcOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hcOtrnTDebt")]
    public ObligationTransaction HcOtrnTDebt
    {
      get => hcOtrnTDebt ??= new();
      set => hcOtrnTDebt = value;
    }

    /// <summary>
    /// A value of HcOtrnTVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtrnTVoluntary")]
    public ObligationTransaction HcOtrnTVoluntary
    {
      get => hcOtrnTVoluntary ??= new();
      set => hcOtrnTVoluntary = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private ObligationType hcOtVoluntary;
    private CsePersonAccount hcCpaObligor;
    private ObligationTransaction hcOtrnTDebt;
    private ObligationTransaction hcOtrnTVoluntary;
    private DateWorkArea current;
    private CsePerson supported;
    private Obligation obligation;
    private CsePerson obligor;
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
    /// A value of ActivityType.
    /// </summary>
    [JsonPropertyName("activityType")]
    public TextWorkArea ActivityType
    {
      get => activityType ??= new();
      set => activityType = value;
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
    /// A value of ZdelLocalHardcodeVoluntary.
    /// </summary>
    [JsonPropertyName("zdelLocalHardcodeVoluntary")]
    public ObligationTransaction ZdelLocalHardcodeVoluntary
    {
      get => zdelLocalHardcodeVoluntary ??= new();
      set => zdelLocalHardcodeVoluntary = value;
    }

    /// <summary>
    /// A value of ZdelLocalHardcodeDebtType.
    /// </summary>
    [JsonPropertyName("zdelLocalHardcodeDebtType")]
    public ObligationTransaction ZdelLocalHardcodeDebtType
    {
      get => zdelLocalHardcodeDebtType ??= new();
      set => zdelLocalHardcodeDebtType = value;
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
    /// A value of ZdelLocalHardcodeObligor.
    /// </summary>
    [JsonPropertyName("zdelLocalHardcodeObligor")]
    public CsePersonAccount ZdelLocalHardcodeObligor
    {
      get => zdelLocalHardcodeObligor ??= new();
      set => zdelLocalHardcodeObligor = value;
    }

    private TextWorkArea activityType;
    private Infrastructure infrastructure;
    private ObligationTransaction zdelLocalHardcodeVoluntary;
    private ObligationTransaction zdelLocalHardcodeDebtType;
    private Common common;
    private CsePersonAccount zdelLocalHardcodeObligor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private Collection collection;
    private CsePerson supportedCsePerson;
    private CsePersonAccount supportedCsePersonAccount;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount obligor;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePerson csePerson;
    private DebtDetail debtDetail;
  }
#endregion
}
