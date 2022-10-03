// Program: FN_B700_OCSE34_STEP_5-1356350890, ID: 373315422, model: 746.
// Short name: SWE02990
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_OCSE34_STEP_5-1356350890.
/// </summary>
[Serializable]
public partial class FnB700Ocse34Step51356350890: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_OCSE34_STEP_5-1356350890 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700Ocse34Step51356350890(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700Ocse34Step51356350890.
  /// </summary>
  public FnB700Ocse34Step51356350890(IContext context, Import import,
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
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 2, "05"))
    {
      UseFnB700BuildGvForRestart();
    }

    foreach(var item in ReadDisbursementTransaction1())
    {
      local.Found.Flag = "N";

      foreach(var item1 in ReadDisbursementTransaction2())
      {
        if (ReadUraExcessCollection())
        {
          local.Found.Flag = "Y";

          break;
        }
      }

      if (AsChar(local.Found.Flag) == 'N')
      {
        continue;
      }

      // ***
      // *** get Collection for current Disbursement Transaction
      // ***
      foreach(var item1 in ReadCollection())
      {
        ReadObligationType();

        if (!ReadCsePersonCsePersonAccountObligationTransaction())
        {
          ExitState = "CSE_PERSON_NF";
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "CSE Person not found for CSE Person Account " + entities
            .CsePersonAccount.Type1 + " Obligation Transaction " + NumberToString
            (entities.ObligationTransaction.SystemGeneratedIdentifier, 15) + " Type " +
            entities.ObligationTransaction.Type1;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            export.Abort.Flag = "Y";

            return;
          }
        }

        local.Work.EffectiveDate = Date(entities.Collection.CreatedTmst);

        // ***Active AF or FC??
        foreach(var item2 in ReadPersonProgramProgram())
        {
          // *** Medical?
          if (entities.ObligationType.SystemGeneratedIdentifier == 3 || entities
            .ObligationType.SystemGeneratedIdentifier == 10 || entities
            .ObligationType.SystemGeneratedIdentifier == 19)
          {
            if (entities.Program.SystemGeneratedIdentifier == 2)
            {
              import.Group.Index = 17;
              import.Group.CheckSize();

              import.Group.Update.Common.TotalCurrency =
                import.Group.Item.Common.TotalCurrency + entities
                .DisbursementTransaction.Amount;

              import.Group.Index = 13;
              import.Group.CheckSize();

              import.Group.Update.Common.TotalCurrency =
                import.Group.Item.Common.TotalCurrency - entities
                .DisbursementTransaction.Amount;
            }
            else
            {
              import.Group.Index = 18;
              import.Group.CheckSize();

              import.Group.Update.Common.TotalCurrency =
                import.Group.Item.Common.TotalCurrency + entities
                .DisbursementTransaction.Amount;

              import.Group.Index = 14;
              import.Group.CheckSize();

              import.Group.Update.Common.TotalCurrency =
                import.Group.Item.Common.TotalCurrency - entities
                .DisbursementTransaction.Amount;
            }
          }
          else if (entities.Program.SystemGeneratedIdentifier == 2)
          {
            import.Group.Index = 17;
            import.Group.CheckSize();

            import.Group.Update.Common.TotalCurrency =
              import.Group.Item.Common.TotalCurrency + entities
              .DisbursementTransaction.Amount;

            import.Group.Index = 10;
            import.Group.CheckSize();

            import.Group.Update.Common.TotalCurrency =
              import.Group.Item.Common.TotalCurrency - entities
              .DisbursementTransaction.Amount;
          }
          else
          {
            import.Group.Index = 18;
            import.Group.CheckSize();

            import.Group.Update.Common.TotalCurrency =
              import.Group.Item.Common.TotalCurrency + entities
              .DisbursementTransaction.Amount;

            import.Group.Index = 11;
            import.Group.CheckSize();

            import.Group.Update.Common.TotalCurrency =
              import.Group.Item.Common.TotalCurrency - entities
              .DisbursementTransaction.Amount;
          }

          // *** Process next Disbursement Transaction
          goto ReadEach;
        }

        // *** No AF or FC found.
        // *** Medical?
        if (entities.ObligationType.SystemGeneratedIdentifier == 3 || entities
          .ObligationType.SystemGeneratedIdentifier == 10 || entities
          .ObligationType.SystemGeneratedIdentifier == 19)
        {
          import.Group.Index = 19;
          import.Group.CheckSize();

          import.Group.Update.Common.TotalCurrency =
            import.Group.Item.Common.TotalCurrency + entities
            .DisbursementTransaction.Amount;

          import.Group.Index = 15;
          import.Group.CheckSize();

          import.Group.Update.Common.TotalCurrency =
            import.Group.Item.Common.TotalCurrency - entities
            .DisbursementTransaction.Amount;
        }
        else
        {
          import.Group.Index = 19;
          import.Group.CheckSize();

          import.Group.Update.Common.TotalCurrency =
            import.Group.Item.Common.TotalCurrency + entities
            .DisbursementTransaction.Amount;

          import.Group.Index = 12;
          import.Group.CheckSize();

          import.Group.Update.Common.TotalCurrency =
            import.Group.Item.Common.TotalCurrency - entities
            .DisbursementTransaction.Amount;
        }

        // *** Process next Disbursement Transaction
        goto ReadEach;
      }

ReadEach:
      ;
    }

    UseFnB700ApplyUpdates();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "06" + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "05";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveGroup1(Import.GroupGroup source,
    FnB700ApplyUpdates.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup2(FnB700BuildGvForRestart.Export.
    GroupGroup source, Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveOcse157Verification(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB700ApplyUpdates()
  {
    var useImport = new FnB700ApplyUpdates.Import();
    var useExport = new FnB700ApplyUpdates.Export();

    useImport.Ocse34.Period = import.Ocse34.Period;
    import.Group.CopyTo(useImport.Group, MoveGroup1);

    Call(FnB700ApplyUpdates.Execute, useImport, useExport);
  }

  private void UseFnB700BuildGvForRestart()
  {
    var useImport = new FnB700BuildGvForRestart.Import();
    var useExport = new FnB700BuildGvForRestart.Export();

    useImport.Ocse34.Period = import.Ocse34.Period;

    Call(FnB700BuildGvForRestart.Execute, useImport, useExport);

    useExport.Group.CopyTo(import.Group, MoveGroup2);
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    MoveOcse157Verification(local.ForError, useImport.Ocse157Verification);

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCollection()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId", entities.UraExcessCollection.InitiatingCollection);
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 18);
        entities.Collection.Ocse34ReportingPeriod =
          db.GetNullableDate(reader, 19);
        entities.Collection.AppliedToFuture = db.GetString(reader, 20);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);

        return true;
      });
  }

  private bool ReadCsePersonCsePersonAccountObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ObligationTransaction.Populated = false;
    entities.Supp.Populated = false;
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonCsePersonAccountObligationTransaction",
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
        entities.Supp.Number = db.GetString(reader, 0);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 0);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 1);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 3);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 4);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.ObligationTransaction.Populated = true;
        entities.Supp.Populated = true;
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction1()
  {
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStart.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEnd.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.DisbursementTransaction.UraExcessCollSeqNbr =
          db.GetInt32(reader, 7);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cpaPType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspPNumber", entities.DisbursementTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.DisbursementTransaction.UraExcessCollSeqNbr =
          db.GetInt32(reader, 7);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Collection.OtyId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Supp.Number);
        db.SetDate(
          command, "effectiveDate",
          local.Work.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private bool ReadUraExcessCollection()
  {
    entities.UraExcessCollection.Populated = false;

    return Read("ReadUraExcessCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "seqNumber",
          entities.DisbursementTransaction.UraExcessCollSeqNbr);
      },
      (db, reader) =>
      {
        entities.UraExcessCollection.SequenceNumber = db.GetInt32(reader, 0);
        entities.UraExcessCollection.InitiatingCollection =
          db.GetInt32(reader, 1);
        entities.UraExcessCollection.Populated = true;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 74;

      private Common common;
    }

    /// <summary>
    /// A value of ReportEnd.
    /// </summary>
    [JsonPropertyName("reportEnd")]
    public DateWorkArea ReportEnd
    {
      get => reportEnd ??= new();
      set => reportEnd = value;
    }

    /// <summary>
    /// A value of ReportStart.
    /// </summary>
    [JsonPropertyName("reportStart")]
    public DateWorkArea ReportStart
    {
      get => reportStart ??= new();
      set => reportStart = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private DateWorkArea reportEnd;
    private DateWorkArea reportStart;
    private Array<GroupGroup> group;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse34 ocse34;
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
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public PersonProgram Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    private Common found;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private PersonProgram work;
    private ProgramCheckpointRestart programCheckpointRestart;
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
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    private ObligationType obligationType;
    private UraExcessCollection uraExcessCollection;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementTransactionRln disbursementTransactionRln;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private Collection collection;
    private CsePerson supp;
    private CsePersonAccount csePersonAccount;
    private Program program;
    private PersonProgram personProgram;
    private DisbursementType disbursementType;
  }
#endregion
}
