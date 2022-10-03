// Program: FN_B634_PROT_COLL_FOR_AF_PGM_CHG, ID: 373386741, model: 746.
// Short name: SWE02422
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B634_PROT_COLL_FOR_AF_PGM_CHG.
/// </summary>
[Serializable]
public partial class FnB634ProtCollForAfPgmChg: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B634_PROT_COLL_FOR_AF_PGM_CHG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB634ProtCollForAfPgmChg(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB634ProtCollForAfPgmChg.
  /// </summary>
  public FnB634ProtCollForAfPgmChg(IContext context, Import import,
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
    // *********************************************************************
    // The PEPR and PEPRfix procedures are setting the effective date to the
    // previous programs end date, instead of the new program effective
    // date.  Code has been changed to check for both dates.
    // *********************************************************************
    local.PlusOne.PgmChgEffectiveDate =
      AddDays(import.Pers.PgmChgEffectiveDate, 1);

    if (ReadPersonProgramProgram())
    {
      if (!Equal(entities.Program.Code, "AF"))
      {
        return;
      }
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Person program not in effect for specified trigger date." + import
        .PersSupportedPersons.Number;
      UseCabErrorReport();

      return;
    }

    foreach(var item in ReadObligation())
    {
      if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'S')
      {
        continue;
      }

      local.ObligCollProtectionHist.CvrdCollStrtDt = local.Null1.Date;
      local.ObligCollProtectionHist.CvrdCollEndDt = local.Null1.Date;

      foreach(var item1 in ReadCollection())
      {
        local.ObligCollProtectionHist.CvrdCollEndDt =
          entities.Collection.CollectionDt;

        if (Equal(local.ObligCollProtectionHist.CvrdCollStrtDt, local.Null1.Date))
          
        {
          local.ObligCollProtectionHist.CvrdCollStrtDt =
            entities.Collection.CollectionDt;
        }
      }

      if (Equal(local.ObligCollProtectionHist.CvrdCollStrtDt, local.Null1.Date))
      {
        continue;
      }

      local.CreateObCollProtHist.Flag = "Y";
      local.ObligCollProtectionHist.ProtectionLevel = "C";
      local.ObligCollProtectionHist.ReasonText =
        "AUTO COLLECTION PROTECTION - RETRO AF PROGRAM CHANGE";
      UseFnProtectCollectionsForOblig();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnProtectCollectionsForOblig()
  {
    var useImport = new FnProtectCollectionsForOblig.Import();
    var useExport = new FnProtectCollectionsForOblig.Export();

    useImport.Persistent.Assign(entities.Obligation);
    useImport.ObligCollProtectionHist.Assign(local.ObligCollProtectionHist);
    useImport.CreateObCollProtHist.Flag = local.CreateObCollProtHist.Flag;

    Call(FnProtectCollectionsForOblig.Execute, useImport, useExport);

    entities.Obligation.Assign(useImport.Persistent);
    local.CollsFndToProtect.Flag = useExport.CollsFndToProtect.Flag;
    local.ObCollProtHistCreated.Flag = useExport.ObCollProtHistCreated.Flag;
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "collDt",
          entities.PersonProgram.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
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
        entities.Collection.DistributionMethod = db.GetString(reader, 14);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 15);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    System.Diagnostics.Debug.Assert(import.Pers.Populated);
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", import.Pers.Type1);
        db.SetNullableString(command, "cspSupNumber", import.Pers.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 6);
        entities.Obligation.LastObligationEvent =
          db.GetNullableString(reader, 7);
        entities.Obligation.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgramProgram()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.PersSupportedPersons.Number);
        db.SetNullableDate(
          command, "pgmChgEffectiveDate1",
          import.Pers.PgmChgEffectiveDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "pgmChgEffectiveDate2",
          local.PlusOne.PgmChgEffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 3);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.PersonProgram.CreatedBy = db.GetString(reader, 6);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 10);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 11);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 12);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 14);
        entities.Program.Code = db.GetString(reader, 15);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;
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
    /// A value of PersSupportedPersons.
    /// </summary>
    [JsonPropertyName("persSupportedPersons")]
    public CsePerson PersSupportedPersons
    {
      get => persSupportedPersons ??= new();
      set => persSupportedPersons = value;
    }

    /// <summary>
    /// A value of Pers.
    /// </summary>
    [JsonPropertyName("pers")]
    public CsePersonAccount Pers
    {
      get => pers ??= new();
      set => pers = value;
    }

    private CsePerson persSupportedPersons;
    private CsePersonAccount pers;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of PlusOne.
    /// </summary>
    [JsonPropertyName("plusOne")]
    public CsePersonAccount PlusOne
    {
      get => plusOne ??= new();
      set => plusOne = value;
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
    /// A value of ObCollProtHistCreated.
    /// </summary>
    [JsonPropertyName("obCollProtHistCreated")]
    public Common ObCollProtHistCreated
    {
      get => obCollProtHistCreated ??= new();
      set => obCollProtHistCreated = value;
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
    /// A value of CreateObCollProtHist.
    /// </summary>
    [JsonPropertyName("createObCollProtHist")]
    public Common CreateObCollProtHist
    {
      get => createObCollProtHist ??= new();
      set => createObCollProtHist = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private DateWorkArea null1;
    private CsePersonAccount plusOne;
    private Common collsFndToProtect;
    private Common obCollProtHistCreated;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Common createObCollProtHist;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
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

    private Collection collection;
    private Program program;
    private PersonProgram personProgram;
    private ObligationTransaction debt;
    private Obligation obligation;
  }
#endregion
}
