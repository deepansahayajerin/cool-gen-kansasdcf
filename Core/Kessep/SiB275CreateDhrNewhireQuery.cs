// Program: SI_B275_CREATE_DHR_NEWHIRE_QUERY, ID: 372745390, model: 746.
// Short name: SWEI275B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B275_CREATE_DHR_NEWHIRE_QUERY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB275CreateDhrNewhireQuery: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B275_CREATE_DHR_NEWHIRE_QUERY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB275CreateDhrNewhireQuery(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB275CreateDhrNewhireQuery.
  /// </summary>
  public SiB275CreateDhrNewhireQuery(IContext context, Import import,
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
    // ***********************************************************************
    // *                   Maintenance Log
    // 
    // *
    // ***********************************************************************
    // *    DATE       NAME      REQUEST      DESCRIPTION                    *
    // * ----------  ---------  
    // ---------
    // --------------------------------
    // *
    // * 07/12/2001  E. Lyman   WR000290    Include AR's. Send SSN and person*
    // *
    // 
    // number only.                     *
    // *
    // 
    // *
    // *
    // 
    // *
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";

    local.Names.Index = 0;
    local.Names.CheckSize();

    local.AlternateSsn.Index = 0;
    local.AlternateSsn.CheckSize();

    UseSiB275Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    foreach(var item in ReadCsePerson())
    {
      if (Equal(entities.CsePerson.Number, local.CsePersonsWorkSet.Number))
      {
        continue;
      }

      foreach(var item1 in ReadCaseRole())
      {
        switch(TrimEnd(entities.CaseRole.Type1))
        {
          case "AP":
            ++local.CountOfApRead.Count;

            break;
          case "AR":
            ++local.CountOfArRead.Count;

            break;
          default:
            continue;
        }

        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiCabReadAdabasBatch();

        if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
        {
          ExitState = "ACO_NN0000_ALL_OK";

          goto ReadEach1;
        }
        else if (IsExitState("ACO_ADABAS_PERSON_NF_113"))
        {
          ExitState = "ACO_NN0000_ALL_OK";

          goto ReadEach1;
        }
        else if (IsExitState("ADABAS_INVALID_SSN_W_RB"))
        {
          ++local.ErrorInvalidSsn.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          goto ReadEach1;
        }
        else if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          goto ReadEach2;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          goto ReadEach2;
        }

        UseEabWriteDhrNewhireQueryFile();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          goto ReadEach2;
        }

        switch(TrimEnd(entities.CaseRole.Type1))
        {
          case "AP":
            ++local.ApQueriesCreated.Count;

            break;
          case "AR":
            ++local.ArQueriesCreated.Count;

            break;
          default:
            break;
        }

        local.Batch.Flag = "Y";
        UseCabRetrieveAliasesAndAltSsn();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto ReadEach2;
        }

        for(local.AlternateSsn.Index = 0; local.AlternateSsn.Index < local
          .AlternateSsn.Count; ++local.AlternateSsn.Index)
        {
          if (!local.AlternateSsn.CheckSize())
          {
            break;
          }

          local.CsePersonsWorkSet.Ssn = local.AlternateSsn.Item.Gssn.Ssn;
          UseEabWriteDhrNewhireQueryFile();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            goto ReadEach2;
          }

          switch(TrimEnd(entities.CaseRole.Type1))
          {
            case "AP":
              ++local.ApQueriesCreated.Count;

              break;
            case "AR":
              ++local.ArQueriesCreated.Count;

              break;
            default:
              break;
          }
        }

        local.AlternateSsn.CheckIndex();

        goto ReadEach1;
      }

ReadEach1:
      ;
    }

ReadEach2:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseSiB275Close();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseSiB275Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // **********************************************************
    // CLOSE ADABAS
    // **********************************************************
    local.CsePersonsWorkSet.Number = "CLOSE";
    UseEabReadCsePersonBatch();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveAlternateSsn(CabRetrieveAliasesAndAltSsn.Export.
    AlternateSsnGroup source, Local.AlternateSsnGroup target)
  {
    target.Gssn.Ssn = source.Gssn.Ssn;
  }

  private static void MoveGrp(Local.GrpGroup source,
    EabWriteDhrNewhireQueryFile.Import.GrpGroup target)
  {
    target.Case1.Number = source.Case1.Number;
    target.Office.SystemGeneratedId = source.Office.SystemGeneratedId;
    target.ServiceProvider.Assign(source.ServiceProvider);
  }

  private static void MoveNames(CabRetrieveAliasesAndAltSsn.Export.
    NamesGroup source, Local.NamesGroup target)
  {
    target.Gnames.Assign(source.Gnames);
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabRetrieveAliasesAndAltSsn()
  {
    var useImport = new CabRetrieveAliasesAndAltSsn.Import();
    var useExport = new CabRetrieveAliasesAndAltSsn.Export();

    useImport.Batch.Flag = local.Batch.Flag;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(CabRetrieveAliasesAndAltSsn.Execute, useImport, useExport);

    useExport.AlternateSsn.CopyTo(local.AlternateSsn, MoveAlternateSsn);
    useExport.Names.CopyTo(local.Names, MoveNames);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabWriteDhrNewhireQueryFile()
  {
    var useImport = new EabWriteDhrNewhireQueryFile.Import();
    var useExport = new EabWriteDhrNewhireQueryFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    local.Grp.CopyTo(useImport.Grp, MoveGrp);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteDhrNewhireQueryFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB275Close()
  {
    var useImport = new SiB275Close.Import();
    var useExport = new SiB275Close.Export();

    useImport.CountOfApRead.Count = local.CountOfApRead.Count;
    useImport.ApQueriesCreated.Count = local.ApQueriesCreated.Count;
    useImport.ErrorInvalidSsn.Count = local.ErrorInvalidSsn.Count;
    useImport.ArQueriesCreated.Count = local.ArQueriesCreated.Count;
    useImport.CountOfArRead.Count = local.CountOfArRead.Count;

    Call(SiB275Close.Execute, useImport, useExport);
  }

  private void UseSiB275Housekeeping()
  {
    var useImport = new SiB275Housekeeping.Import();
    var useExport = new SiB275Housekeeping.Export();

    Call(SiB275Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiCabReadAdabasBatch()
  {
    var useImport = new SiCabReadAdabasBatch.Import();
    var useExport = new SiCabReadAdabasBatch.Export();

    useImport.Obligor.Number = local.CsePersonsWorkSet.Number;

    Call(SiCabReadAdabasBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.Obligor);
    local.AbendData.Assign(useExport.AbendData);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      null,
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
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
    /// <summary>A AlternateSsnGroup group.</summary>
    [Serializable]
    public class AlternateSsnGroup
    {
      /// <summary>
      /// A value of Gssn.
      /// </summary>
      [JsonPropertyName("gssn")]
      public CsePersonsWorkSet Gssn
      {
        get => gssn ??= new();
        set => gssn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet gssn;
    }

    /// <summary>A NamesGroup group.</summary>
    [Serializable]
    public class NamesGroup
    {
      /// <summary>
      /// A value of Gnames.
      /// </summary>
      [JsonPropertyName("gnames")]
      public CsePersonsWorkSet Gnames
      {
        get => gnames ??= new();
        set => gnames = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet gnames;
    }

    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
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
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
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
      public const int Capacity = 10;

      private Case1 case1;
      private Office office;
      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// Gets a value of AlternateSsn.
    /// </summary>
    [JsonIgnore]
    public Array<AlternateSsnGroup> AlternateSsn => alternateSsn ??= new(
      AlternateSsnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AlternateSsn for json serialization.
    /// </summary>
    [JsonPropertyName("alternateSsn")]
    [Computed]
    public IList<AlternateSsnGroup> AlternateSsn_Json
    {
      get => alternateSsn;
      set => AlternateSsn.Assign(value);
    }

    /// <summary>
    /// Gets a value of Names.
    /// </summary>
    [JsonIgnore]
    public Array<NamesGroup> Names => names ??= new(NamesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Names for json serialization.
    /// </summary>
    [JsonPropertyName("names")]
    [Computed]
    public IList<NamesGroup> Names_Json
    {
      get => names;
      set => Names.Assign(value);
    }

    /// <summary>
    /// A value of ArQueriesCreated.
    /// </summary>
    [JsonPropertyName("arQueriesCreated")]
    public Common ArQueriesCreated
    {
      get => arQueriesCreated ??= new();
      set => arQueriesCreated = value;
    }

    /// <summary>
    /// A value of ApQueriesCreated.
    /// </summary>
    [JsonPropertyName("apQueriesCreated")]
    public Common ApQueriesCreated
    {
      get => apQueriesCreated ??= new();
      set => apQueriesCreated = value;
    }

    /// <summary>
    /// A value of SupportedNotAnAr.
    /// </summary>
    [JsonPropertyName("supportedNotAnAr")]
    public Common SupportedNotAnAr
    {
      get => supportedNotAnAr ??= new();
      set => supportedNotAnAr = value;
    }

    /// <summary>
    /// A value of ObligorNotAnAp.
    /// </summary>
    [JsonPropertyName("obligorNotAnAp")]
    public Common ObligorNotAnAp
    {
      get => obligorNotAnAp ??= new();
      set => obligorNotAnAp = value;
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
    /// A value of ErrorInvalidSsn.
    /// </summary>
    [JsonPropertyName("errorInvalidSsn")]
    public Common ErrorInvalidSsn
    {
      get => errorInvalidSsn ??= new();
      set => errorInvalidSsn = value;
    }

    /// <summary>
    /// A value of CasesRead.
    /// </summary>
    [JsonPropertyName("casesRead")]
    public Common CasesRead
    {
      get => casesRead ??= new();
      set => casesRead = value;
    }

    /// <summary>
    /// A value of CountOfApRead.
    /// </summary>
    [JsonPropertyName("countOfApRead")]
    public Common CountOfApRead
    {
      get => countOfApRead ??= new();
      set => countOfApRead = value;
    }

    /// <summary>
    /// A value of CountOfArRead.
    /// </summary>
    [JsonPropertyName("countOfArRead")]
    public Common CountOfArRead
    {
      get => countOfArRead ??= new();
      set => countOfArRead = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private Common batch;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend neededToWrite;
    private Array<AlternateSsnGroup> alternateSsn;
    private Array<NamesGroup> names;
    private Common arQueriesCreated;
    private Common apQueriesCreated;
    private Common supportedNotAnAr;
    private Common obligorNotAnAp;
    private CaseAssignment caseAssignment;
    private Common errorInvalidSsn;
    private Common casesRead;
    private Common countOfApRead;
    private Common countOfArRead;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<GrpGroup> grp;
    private DateWorkArea process;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private CaseRole caseRole;
  }
#endregion
}
