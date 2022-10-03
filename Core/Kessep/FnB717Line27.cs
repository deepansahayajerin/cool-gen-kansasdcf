// Program: FN_B717_LINE_27, ID: 373361307, model: 746.
// Short name: SWE03013
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_27.
/// </summary>
[Serializable]
public partial class FnB717Line27: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_27 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line27(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line27.
  /// </summary>
  public FnB717Line27(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.Create.YearMonth = import.StatsReport.YearMonth.GetValueOrDefault();
    local.Create.FirstRunNumber =
      import.StatsReport.FirstRunNumber.GetValueOrDefault();
    local.Create.LineNumber = 27;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "27 "))
    {
      local.Restart.SystemGeneratedIdentifier =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 4, 9));
      local.RestartAp.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 13, 10);
    }

    foreach(var item in ReadCollectionObligationTypeCsePersonCashReceiptType())
    {
      ++local.CommitCnt.Count;

      if (entities.Collection.SystemGeneratedIdentifier == local
        .Restart.SystemGeneratedIdentifier)
      {
        if (!Lt(local.RestartAp.Number, entities.ApCsePerson.Number))
        {
          continue;
        }
      }

      // -------------------------------------------------------------------
      // Exclude Recovery Ob Types.
      // -------------------------------------------------------------------
      if (entities.ObligationType.SystemGeneratedIdentifier == 4 || entities
        .ObligationType.SystemGeneratedIdentifier == 5 || entities
        .ObligationType.SystemGeneratedIdentifier == 6 || entities
        .ObligationType.SystemGeneratedIdentifier == 7 || entities
        .ObligationType.SystemGeneratedIdentifier == 8 || entities
        .ObligationType.SystemGeneratedIdentifier == 9)
      {
        continue;
      }

      // -------------------------------------------------------------------
      // Exclude REIP payments. Include CSE Net and Direct Pmnts.
      // -------------------------------------------------------------------
      if (entities.CashReceiptType.SystemGeneratedIdentifier == 2 || entities
        .CashReceiptType.SystemGeneratedIdentifier == 7)
      {
        if (!ReadCollectionType())
        {
          continue;
        }

        if (entities.CollectionType.SequentialIdentifier == 27 || entities
          .CollectionType.SequentialIdentifier == 28 || entities
          .CollectionType.SequentialIdentifier == 29 || entities
          .CollectionType.SequentialIdentifier == 14 || entities
          .CollectionType.SequentialIdentifier == 20 || entities
          .CollectionType.SequentialIdentifier == 21)
        {
        }
        else
        {
          continue;
        }
      }

      if (AsChar(entities.Collection.AdjustedInd) == 'Y' && Lt
        (entities.Collection.CreatedTmst, import.ReportStartDate.Timestamp))
      {
        local.DistributionDate.Date =
          entities.Collection.CollectionAdjustmentDt;
        UseFnBuildTimestampFrmDateTime();
      }
      else
      {
        local.DistributionDate.Timestamp = entities.Collection.CreatedTmst;
        local.DistributionDate.Date = Date(entities.Collection.CreatedTmst);
      }

      local.Create.SuppPersonNumber = "";
      local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
      local.Create.CollectionDate = entities.Collection.CollectionDt;

      if (Lt(entities.Collection.CreatedTmst, import.ReportStartDate.Timestamp))
      {
        local.Create.CollectionAmount = -entities.Collection.Amount;
      }
      else
      {
        local.Create.CollectionAmount = entities.Collection.Amount;
      }

      local.Create.CollCreatedDate = Date(entities.Collection.CreatedTmst);

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        ReadObligationType();
        local.Create.ObligationType = entities.ForDisplay.Code;
      }

      ReadCase5();

      if (local.NbrOfCases.Count == 0)
      {
        continue;
      }
      else if (local.NbrOfCases.Count == 1)
      {
        if (ReadCase4())
        {
          UseFnB717MaintainLine27();
        }
        else
        {
          continue;
        }
      }
      else if (entities.ObligationType.SystemGeneratedIdentifier == 15)
      {
        // -------------------------------------------------------------------
        // Get SP. Remember, Fees do not have a SP.
        // -------------------------------------------------------------------
        foreach(var item1 in ReadCase6())
        {
          UseFnB717MaintainLine27();
        }
      }
      else if (entities.ObligationType.SystemGeneratedIdentifier == 16)
      {
        local.AsOf.Date = Date(entities.Collection.CreatedTmst);

        if (!ReadDebtCsePerson())
        {
          continue;
        }

        local.Create.SuppPersonNumber = entities.Supp.Number;

        if (ReadCase2())
        {
          UseFnB717MaintainLine27();
        }
        else
        {
          continue;
        }
      }
      else
      {
        if (!ReadDebtCsePersonDebtDetail())
        {
          continue;
        }

        local.AsOf.Date = entities.DebtDetail.DueDt;
        local.Create.SuppPersonNumber = entities.Supp.Number;

        if (entities.ObligationType.SystemGeneratedIdentifier == 2 || entities
          .ObligationType.SystemGeneratedIdentifier == 7)
        {
          // --------------------------------------------
          // Spousal - AR is the supp person.
          // ---------------------------------------------
          if (ReadCase3())
          {
            UseFnB717MaintainLine27();
          }
          else
          {
            continue;
          }
        }
        else
        {
          // --------------------------------------------
          // CH is the supp person.
          // --------------------------------------------
          if (ReadCase1())
          {
            UseFnB717MaintainLine27();
          }
          else
          {
            // --------------------------------------------
            // Case nf as of due date. Go back in time.
            // ---------------------------------------------
            if (ReadCaseCaseRole2())
            {
              UseFnB717MaintainLine27();

              goto Test;
            }

            // --------------------------------------------
            // Case nf. Go forward in time.
            // ---------------------------------------------
            if (ReadCaseCaseRole1())
            {
              UseFnB717MaintainLine27();

              goto Test;
            }

            // --------------------------------------------
            // Case nf. Skip collection.
            // ---------------------------------------------
            continue;
          }
        }
      }

Test:

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "27 " + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + entities
          .ApCsePerson.Number;
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Error.LineNumber = 27;
          UseFnB717WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }
    }

    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "28" + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Error.LineNumber = 27;
      UseFnB717WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.Amount = source.Amount;
    target.AdjustedInd = source.AdjustedInd;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveStatsReport(StatsReport source, StatsReport target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
  }

  private void UseFnB717MaintainLine27()
  {
    var useImport = new FnB717MaintainLine27.Import();
    var useExport = new FnB717MaintainLine27.Export();

    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    useImport.Create.Assign(local.Create);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;
    MoveCollection(entities.Collection, useImport.Collection);
    MoveDateWorkArea(local.DistributionDate, useImport.DistributionDate);
    useImport.Case1.Assign(entities.Case1);
    MoveStatsReport(import.StatsReport, useImport.StatsReport);

    Call(FnB717MaintainLine27.Execute, useImport, useExport);
  }

  private void UseFnB717WriteError()
  {
    var useImport = new FnB717WriteError.Import();
    var useExport = new FnB717WriteError.Export();

    useImport.Error.Assign(local.Error);

    Call(FnB717WriteError.Execute, useImport, useExport);
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = local.DistributionDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, local.DistributionDate);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.AsOf.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber2", entities.Supp.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.Supp.Number);
        db.SetNullableDate(
          command, "startDate", local.AsOf.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase3()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.Supp.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase4()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase5()
  {
    return Read("ReadCase5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        local.NbrOfCases.Count = db.GetInt32(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCase6()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseCaseRole1()
  {
    entities.Case1.Populated = false;
    entities.ChCaseRole.Populated = false;

    return Read("ReadCaseCaseRole1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.AsOf.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber1", entities.Supp.Number);
        db.SetString(command, "cspNumber2", entities.ApCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ChCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCaseRole.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.ChCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.Case1.Populated = true;
        entities.ChCaseRole.Populated = true;
      });
  }

  private bool ReadCaseCaseRole2()
  {
    entities.Case1.Populated = false;
    entities.ChCaseRole.Populated = false;

    return Read("ReadCaseCaseRole2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.AsOf.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber1", entities.Supp.Number);
        db.SetString(command, "cspNumber2", entities.ApCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ChCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCaseRole.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.ChCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.Case1.Populated = true;
        entities.ChCaseRole.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCollectionObligationTypeCsePersonCashReceiptType()
  {
    entities.CashReceiptType.Populated = false;
    entities.Collection.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadCollectionObligationTypeCsePersonCashReceiptType",
      (db, command) =>
      {
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
        db.SetInt32(command, "collId", local.Restart.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.ApCsePerson.Number = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.CashReceiptType.Populated = true;
        entities.Collection.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ObligationType.Populated = true;

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadDebtCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Supp.Populated = false;
    entities.Debt.Populated = false;

    return Read("ReadDebtCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Supp.Number = db.GetString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.Supp.Populated = true;
        entities.Debt.Populated = true;
      });
  }

  private bool ReadDebtCsePersonDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.DebtDetail.Populated = false;
    entities.Supp.Populated = false;
    entities.Debt.Populated = false;

    return Read("ReadDebtCsePersonDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Supp.Number = db.GetString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.Populated = true;
        entities.Supp.Populated = true;
        entities.Debt.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ForDisplay.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ForDisplay.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ForDisplay.Code = db.GetString(reader, 1);
        entities.ForDisplay.Populated = true;
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
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

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
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
    }

    private Common displayInd;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private StatsReport statsReport;
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
    /// A value of RestartAp.
    /// </summary>
    [JsonPropertyName("restartAp")]
    public CsePerson RestartAp
    {
      get => restartAp ??= new();
      set => restartAp = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Collection Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of DistributionDate.
    /// </summary>
    [JsonPropertyName("distributionDate")]
    public DateWorkArea DistributionDate
    {
      get => distributionDate ??= new();
      set => distributionDate = value;
    }

    /// <summary>
    /// A value of NbrOfCases.
    /// </summary>
    [JsonPropertyName("nbrOfCases")]
    public Common NbrOfCases
    {
      get => nbrOfCases ??= new();
      set => nbrOfCases = value;
    }

    /// <summary>
    /// A value of AsOf.
    /// </summary>
    [JsonPropertyName("asOf")]
    public DateWorkArea AsOf
    {
      get => asOf ??= new();
      set => asOf = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public StatsVerifi Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public StatsVerifi Error
    {
      get => error ??= new();
      set => error = value;
    }

    private CsePerson restartAp;
    private Collection restart;
    private DateWorkArea distributionDate;
    private Common nbrOfCases;
    private DateWorkArea asOf;
    private StatsVerifi create;
    private Program program;
    private Common commitCnt;
    private ProgramCheckpointRestart programCheckpointRestart;
    private StatsVerifi error;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of ForDisplay.
    /// </summary>
    [JsonPropertyName("forDisplay")]
    public ObligationType ForDisplay
    {
      get => forDisplay ??= new();
      set => forDisplay = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    private CollectionType collectionType;
    private ObligationType forDisplay;
    private CaseAssignment caseAssignment;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private Case1 case1;
    private DebtDetail debtDetail;
    private Collection collection;
    private CsePerson apCsePerson;
    private CsePerson supp;
    private CaseRole apCaseRole;
    private CaseRole chCaseRole;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private ObligationType obligationType;
    private AccrualInstructions accrualInstructions;
    private CsePersonAccount supported;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePerson chCsePerson;
  }
#endregion
}
