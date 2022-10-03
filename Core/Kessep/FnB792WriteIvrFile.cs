// Program: FN_B792_WRITE_IVR_FILE, ID: 1902416811, model: 746.
// Short name: SWE03731
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B792_WRITE_IVR_FILE.
/// </summary>
[Serializable]
public partial class FnB792WriteIvrFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B792_WRITE_IVR_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB792WriteIvrFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB792WriteIvrFile.
  /// </summary>
  public FnB792WriteIvrFile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 10/01/2013  DDupree	CQ38344		Initial Development.
    // 10/22/2015  GVandy	CQ46101		Remove logic requiring CP to have assigned 
    // arrears.
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    // --  Create the IVR file using the data extracted by B791 which has been 
    // externally sorted/summed.
    // --
    // -------------------------------------------------------------------------------------------------------------------------
    // --  The export group is only used to view match to the local group in the
    // PrAD so that it is re-initialized after the AR statement is printed, it
    // is otherwise not referenced in this cab.
    // -- If the import group is empty then escape out.  This is used to re-
    // initialize the group in the PrAD when an AR statement exceeded the
    // maximum entries in the group view.
    if (import.Import1.IsEmpty)
    {
      return;
    }

    export.NumberOfRecordsWritten.Count = import.NumberOfRecordsWritten.Count;

    // -- Default return status to ERRORED.  We'll reset it to SKIPPED or 
    // PRINTED later.
    export.ArStatementStatus.Text8 = "ERRORED";
    local.LastOfImportGroup.Count = import.Import1.Count;

    // -- Define the number of lines to be printed on each page of the AR 
    // statement.
    local.NumberOfLinesPerPage.Count = 59;

    if (!ReadCsePerson())
    {
      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - CSE Person Not Found.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    // 05/02/05  GVandy  PR242550  Do not send statement if the AR is deceased.
    if (!Equal(entities.Ar.DateOfDeath, local.Null1.Date))
    {
      export.ArStatementStatus.Text8 = "DECEASED";

      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - AR is Deceased.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    // Do send record even if the only collection activity is for 718B 
    // judgements.So the following code is disabled
    // 10/22/2015  GVandy  CQ46101  Remove logic requiring CP to have assigned 
    // arrears.
    // -------------------------------------------------------------------------------------------------------------------------
    // --  Retrieve AR name from Adabas.
    // -------------------------------------------------------------------------------------------------------------------------
    local.Ar.Number = import.Ar.Number;
    UseEabReadCsePersonBatch2();

    if (IsEmpty(local.AbendData.Type1))
    {
      // -- Successful Adabas read occurred.
    }
    else
    {
      switch(AsChar(local.AbendData.Type1))
      {
        case 'A':
          // -- Unsuccessful Adabas read occurred.
          switch(TrimEnd(local.AbendData.AdabasResponseCd))
          {
            case "0113":
              local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas response code 113.  AR not found in Adabas.";
                

              break;
            case "0148":
              local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas response code 148.  Adabas unavailable.";
                

              break;
            default:
              local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas error, response code = " +
                local.AbendData.AdabasResponseCd + ", type = " + local
                .AbendData.Type1;

              break;
          }

          break;
        case 'C':
          // -- CICS action failed.
          local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - CICS error, response code = " +
            local.AbendData.CicsResponseCd;

          break;
        default:
          // -- Action failed.
          local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Unknown Adabas error, type = " +
            local.AbendData.Type1;

          break;
      }

      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      if (AsChar(local.AbendData.Type1) == 'A' && Equal
        (local.AbendData.AdabasResponseCd, "0113"))
      {
        // -- No need to abend if the AR is not found on Adabas, just log to the
        // error file.
      }
      else
      {
        // -- Any errors beside the AR not being found on Adabas should abend.
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // --  Do not send to the IVR if the AR has no SSN.  SSN is required by the 
    // IVR.
    if (!Lt("000000000", local.Ar.Ssn))
    {
      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - No SSN.";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    // --  Do not send to the IVR if the AR has no Date of Birth.  DoB is 
    // required by the IVR.
    if (!Lt(new DateTime(1, 1, 1), local.Ar.Dob))
    {
      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - No Date of Birth.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    // --  The following FOR goes to last of group_import + 1, this is needed so
    // that we write the total amount collected for the very last collection
    // date on the statement.
    import.Import1.Index = 0;

    for(var limit = import.Import1.Count + 1; import.Import1.Index < limit; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      if ((!Equal(
        import.Import1.Item.GimportObligor.Number,
        local.PreviousObligor.Number) || !
        Equal(import.Import1.Item.G.CourtOrderAppliedTo,
        local.Previous.CourtOrderAppliedTo) || !
        Equal(import.Import1.Item.G.CollectionDt, local.Previous.CollectionDt)) &&
        import.Import1.Index != 0)
      {
        local.RecordType.Text1 = "2";
        local.EabFileHandling.Action = "WRITE";
        UseFnB792EabWriteIvrFile();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // --  write to error file...
          local.EabReportSend.RptDetail =
            "(01) Error writing IVR record to the file...  Returned Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport();
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          return;
        }

        ++export.NumberOfRecordsWritten.Count;
        local.Collection.Amount = 0;
        local.TotalForwardedToFamily.Amount = 0;
        local.AppliedAsCurrent.Amount = 0;
        local.AppliedAsArrears.Amount = 0;
      }

      if (import.Import1.Index >= local.LastOfImportGroup.Count)
      {
        // -- We finished processing the import group.
        break;
      }

      local.Print.Update.GlocalReportDetailLine.RptDetail = "";

      // --  Line 29
      if (!Equal(import.Import1.Item.GimportObligor.Number,
        local.PreviousObligor.Number) || !
        Equal(import.Import1.Item.G.CourtOrderAppliedTo,
        local.Previous.CourtOrderAppliedTo))
      {
        if (!Equal(import.Import1.Item.GimportObligor.Number,
          local.PreviousObligor.Number))
        {
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Retrieve Obligor name.
          // -------------------------------------------------------------------------------------------------------------------------
          local.ApCsePersonsWorkSet.Number =
            import.Import1.Item.GimportObligor.Number;
          local.ApCsePerson.Number = import.Import1.Item.GimportObligor.Number;
          UseEabReadCsePersonBatch1();

          if (IsEmpty(local.AbendData.Type1))
          {
            // -- Successful Adabas read occurred.
          }
          else
          {
            switch(AsChar(local.AbendData.Type1))
            {
              case 'A':
                // -- Unsuccessful Adabas read occurred.
                switch(TrimEnd(local.AbendData.AdabasResponseCd))
                {
                  case "0113":
                    local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas response code 113.  Obligor " +
                      import.Import1.Item.GimportObligor.Number + " not found in Adabas.";
                      
                    local.EabReportSend.RptDetail =
                      "Adabas response code 113, Obligor cse person number " + import
                      .Import1.Item.GimportObligor.Number + " not found in Adabas.";
                      

                    break;
                  case "0148":
                    local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas response code 148.  Obligor " +
                      import.Import1.Item.GimportObligor.Number + ".  Adabas unavailable.";
                      
                    local.EabReportSend.RptDetail =
                      "Adabas response code 148, Adabas unavailable.  Obligor cse person number " +
                      import.Import1.Item.GimportObligor.Number + " ";

                    break;
                  default:
                    local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas error, response code = " +
                      local.AbendData.AdabasResponseCd + ", type = " + local
                      .AbendData.Type1 + "  Obligor " + import
                      .Import1.Item.GimportObligor.Number;
                    local.EabReportSend.RptDetail =
                      "Adabas error, response code = " + local
                      .AbendData.AdabasResponseCd + ", type = " + local
                      .AbendData.Type1 + ", Obligor cse person number = " + import
                      .Import1.Item.GimportObligor.Number;

                    break;
                }

                break;
              case 'C':
                // -- CICS action failed.
                local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - CICS error, response code = " +
                  local.AbendData.CicsResponseCd + "  Obligor " + import
                  .Import1.Item.GimportObligor.Number;
                local.EabReportSend.RptDetail =
                  "CICS error, response code = " + local
                  .AbendData.CicsResponseCd + ", for Obligor cse person number = " +
                  import.Import1.Item.GimportObligor.Number + " ";

                break;
              default:
                // -- Action failed.
                local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Unknown Adabas error, type = " +
                  local.AbendData.Type1 + "  Obligor " + import
                  .Import1.Item.GimportObligor.Number;
                local.EabReportSend.RptDetail =
                  "Unknown Adabas error, type = " + local.AbendData.Type1 + ", for Obligor cse person number = " +
                  import.Import1.Item.GimportObligor.Number + " ";

                break;
            }
          }
        }

        local.Collection.CourtOrderAppliedTo =
          import.Import1.Item.G.CourtOrderAppliedTo ?? "";
        local.LegalAction.StandardNumber =
          local.Collection.CourtOrderAppliedTo ?? "";
      }

      // -------------------------------------------------------------------------------------------------------------------------
      // -- Keep running totals for the amount collected on the collection date 
      // and the total amount sent to family.
      // -------------------------------------------------------------------------------------------------------------------------
      local.TotalForwardedToFamily.Amount += import.Import1.Item.
        GimportForwardedToFamily.Amount;

      if (AsChar(import.Import1.Item.G.AppliedToCode) == 'A')
      {
        local.AppliedAsArrears.Amount = local.AppliedAsArrears.Amount + import
          .Import1.Item.GimportRetained.Amount + import
          .Import1.Item.GimportForwardedToFamily.Amount;
      }
      else if (AsChar(import.Import1.Item.G.AppliedToCode) == 'C')
      {
        local.AppliedAsCurrent.Amount = local.AppliedAsCurrent.Amount + import
          .Import1.Item.GimportRetained.Amount + import
          .Import1.Item.GimportForwardedToFamily.Amount;
      }
      else
      {
        // -- Write to error file...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Unrecognized Applied to Code = " +
          import.Import1.Item.G.AppliedToCode;
        local.EabReportSend.RptDetail = "Unrecognized Applied to Code, " + import
          .Import1.Item.G.AppliedToCode + ", encounted on IVR file for AR " + import
          .Ar.Number + ".";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
        }

        return;
      }

      local.Collection.Amount = local.AppliedAsArrears.Amount + local
        .AppliedAsCurrent.Amount;
      local.Collection.CollectionDt = import.Import1.Item.G.CollectionDt;

      // -- Move current views to previous views.
      local.Previous.Assign(import.Import1.Item.G);
      local.PreviousObligor.Number = import.Import1.Item.GimportObligor.Number;
    }

    import.Import1.CheckIndex();

    if (local.Collection.Amount > 0)
    {
      local.RecordType.Text1 = "2";
      local.EabFileHandling.Action = "WRITE";
      UseFnB792EabWriteIvrFile();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // --  write to error file...
        local.EabReportSend.RptDetail =
          "(01) Error writing IVR record to the file...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport();
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        return;
      }

      ++export.NumberOfRecordsWritten.Count;
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.CollectionDt = source.CollectionDt;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
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

  private void UseEabReadCsePersonBatch1()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.ApCsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet(local.ApCsePersonsWorkSet, useExport.CsePersonsWorkSet);
      
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.ApCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabReadCsePersonBatch2()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ar.Number;
    useExport.AbendData.Assign(local.AbendData);
    MoveCsePersonsWorkSet(local.Ar, useExport.CsePersonsWorkSet);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.Ar);
  }

  private void UseFnB792EabWriteIvrFile()
  {
    var useImport = new FnB792EabWriteIvrFile.Import();
    var useExport = new FnB792EabWriteIvrFile.Export();

    useImport.RecordType.Text1 = local.RecordType.Text1;
    useImport.TotalForwardedToFamily.Amount =
      local.TotalForwardedToFamily.Amount;
    useImport.AppliedAsArrears.Amount = local.AppliedAsArrears.Amount;
    useImport.AppliedAsCurrent.Amount = local.AppliedAsCurrent.Amount;
    useImport.ApCsePerson.Number = local.ApCsePerson.Number;
    useImport.ApCsePersonsWorkSet.Assign(local.ApCsePersonsWorkSet);
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    MoveCollection(local.Collection, useImport.Collection);
    useImport.ArCsePersonsWorkSet.Assign(local.Ar);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ArCsePerson.Number = import.Ar.Number;
    useExport.External.Assign(local.External);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB792EabWriteIvrFile.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCsePerson()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
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
    /// <summary>A GimportExportStatementCountGroup group.</summary>
    [Serializable]
    public class GimportExportStatementCountGroup
    {
      /// <summary>
      /// A value of GimportExportCount.
      /// </summary>
      [JsonPropertyName("gimportExportCount")]
      public Common GimportExportCount
      {
        get => gimportExportCount ??= new();
        set => gimportExportCount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Common gimportExportCount;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of GimportObligor.
      /// </summary>
      [JsonPropertyName("gimportObligor")]
      public CsePerson GimportObligor
      {
        get => gimportObligor ??= new();
        set => gimportObligor = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Collection G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GimportRetained.
      /// </summary>
      [JsonPropertyName("gimportRetained")]
      public Collection GimportRetained
      {
        get => gimportRetained ??= new();
        set => gimportRetained = value;
      }

      /// <summary>
      /// A value of GimportForwardedToFamily.
      /// </summary>
      [JsonPropertyName("gimportForwardedToFamily")]
      public Collection GimportForwardedToFamily
      {
        get => gimportForwardedToFamily ??= new();
        set => gimportForwardedToFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private CsePerson gimportObligor;
      private Collection g;
      private Collection gimportRetained;
      private Collection gimportForwardedToFamily;
    }

    /// <summary>
    /// A value of NumberOfRecordsWritten.
    /// </summary>
    [JsonPropertyName("numberOfRecordsWritten")]
    public Common NumberOfRecordsWritten
    {
      get => numberOfRecordsWritten ??= new();
      set => numberOfRecordsWritten = value;
    }

    /// <summary>
    /// A value of Non718BCollection.
    /// </summary>
    [JsonPropertyName("non718BCollection")]
    public Common Non718BCollection
    {
      get => non718BCollection ??= new();
      set => non718BCollection = value;
    }

    /// <summary>
    /// A value of CreateEvents.
    /// </summary>
    [JsonPropertyName("createEvents")]
    public Common CreateEvents
    {
      get => createEvents ??= new();
      set => createEvents = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingDateWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingDateWorkArea")]
    public DateWorkArea ReportingPeriodEndingDateWorkArea
    {
      get => reportingPeriodEndingDateWorkArea ??= new();
      set => reportingPeriodEndingDateWorkArea = value;
    }

    /// <summary>
    /// Gets a value of GimportExportStatementCount.
    /// </summary>
    [JsonIgnore]
    public Array<GimportExportStatementCountGroup>
      GimportExportStatementCount => gimportExportStatementCount ??= new(
        GimportExportStatementCountGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of GimportExportStatementCount for json serialization.
    /// </summary>
    [JsonPropertyName("gimportExportStatementCount")]
    [Computed]
    public IList<GimportExportStatementCountGroup>
      GimportExportStatementCount_Json
    {
      get => gimportExportStatementCount;
      set => GimportExportStatementCount.Assign(value);
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStarting.
    /// </summary>
    [JsonPropertyName("reportingPeriodStarting")]
    public TextWorkArea ReportingPeriodStarting
    {
      get => reportingPeriodStarting ??= new();
      set => reportingPeriodStarting = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingTextWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingTextWorkArea")]
    public TextWorkArea ReportingPeriodEndingTextWorkArea
    {
      get => reportingPeriodEndingTextWorkArea ??= new();
      set => reportingPeriodEndingTextWorkArea = value;
    }

    /// <summary>
    /// A value of Voluntary.
    /// </summary>
    [JsonPropertyName("voluntary")]
    public ObligationType Voluntary
    {
      get => voluntary ??= new();
      set => voluntary = value;
    }

    /// <summary>
    /// A value of SpousalArrearsJudgement.
    /// </summary>
    [JsonPropertyName("spousalArrearsJudgement")]
    public ObligationType SpousalArrearsJudgement
    {
      get => spousalArrearsJudgement ??= new();
      set => spousalArrearsJudgement = value;
    }

    /// <summary>
    /// A value of SpousalSupport.
    /// </summary>
    [JsonPropertyName("spousalSupport")]
    public ObligationType SpousalSupport
    {
      get => spousalSupport ??= new();
      set => spousalSupport = value;
    }

    private Common numberOfRecordsWritten;
    private Common non718BCollection;
    private Common createEvents;
    private DateWorkArea reportingPeriodEndingDateWorkArea;
    private Array<GimportExportStatementCountGroup> gimportExportStatementCount;
    private CsePerson ar;
    private Array<ImportGroup> import1;
    private ProgramProcessingInfo programProcessingInfo;
    private TextWorkArea reportingPeriodStarting;
    private TextWorkArea reportingPeriodEndingTextWorkArea;
    private ObligationType voluntary;
    private ObligationType spousalArrearsJudgement;
    private ObligationType spousalSupport;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of GexportObligor.
      /// </summary>
      [JsonPropertyName("gexportObligor")]
      public CsePerson GexportObligor
      {
        get => gexportObligor ??= new();
        set => gexportObligor = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Collection G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GexportRetained.
      /// </summary>
      [JsonPropertyName("gexportRetained")]
      public Collection GexportRetained
      {
        get => gexportRetained ??= new();
        set => gexportRetained = value;
      }

      /// <summary>
      /// A value of GexportForwardedToFamily.
      /// </summary>
      [JsonPropertyName("gexportForwardedToFamily")]
      public Collection GexportForwardedToFamily
      {
        get => gexportForwardedToFamily ??= new();
        set => gexportForwardedToFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private CsePerson gexportObligor;
      private Collection g;
      private Collection gexportRetained;
      private Collection gexportForwardedToFamily;
    }

    /// <summary>
    /// A value of NumberOfRecordsWritten.
    /// </summary>
    [JsonPropertyName("numberOfRecordsWritten")]
    public Common NumberOfRecordsWritten
    {
      get => numberOfRecordsWritten ??= new();
      set => numberOfRecordsWritten = value;
    }

    /// <summary>
    /// A value of ArStatementStatus.
    /// </summary>
    [JsonPropertyName("arStatementStatus")]
    public TextWorkArea ArStatementStatus
    {
      get => arStatementStatus ??= new();
      set => arStatementStatus = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Common numberOfRecordsWritten;
    private TextWorkArea arStatementStatus;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A PrintGroup group.</summary>
    [Serializable]
    public class PrintGroup
    {
      /// <summary>
      /// A value of GlocalReportDetailLine.
      /// </summary>
      [JsonPropertyName("glocalReportDetailLine")]
      public EabReportSend GlocalReportDetailLine
      {
        get => glocalReportDetailLine ??= new();
        set => glocalReportDetailLine = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private EabReportSend glocalReportDetailLine;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public WorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of TotalForwardedToFamily.
    /// </summary>
    [JsonPropertyName("totalForwardedToFamily")]
    public Collection TotalForwardedToFamily
    {
      get => totalForwardedToFamily ??= new();
      set => totalForwardedToFamily = value;
    }

    /// <summary>
    /// A value of AppliedAsArrears.
    /// </summary>
    [JsonPropertyName("appliedAsArrears")]
    public Collection AppliedAsArrears
    {
      get => appliedAsArrears ??= new();
      set => appliedAsArrears = value;
    }

    /// <summary>
    /// A value of AppliedAsCurrent.
    /// </summary>
    [JsonPropertyName("appliedAsCurrent")]
    public Collection AppliedAsCurrent
    {
      get => appliedAsCurrent ??= new();
      set => appliedAsCurrent = value;
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
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of DerivedAr.
    /// </summary>
    [JsonPropertyName("derivedAr")]
    public CsePerson DerivedAr
    {
      get => derivedAr ??= new();
      set => derivedAr = value;
    }

    /// <summary>
    /// A value of DueDate.
    /// </summary>
    [JsonPropertyName("dueDate")]
    public DateWorkArea DueDate
    {
      get => dueDate ??= new();
      set => dueDate = value;
    }

    /// <summary>
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
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
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of LastOfImportGroup.
    /// </summary>
    [JsonPropertyName("lastOfImportGroup")]
    public Common LastOfImportGroup
    {
      get => lastOfImportGroup ??= new();
      set => lastOfImportGroup = value;
    }

    /// <summary>
    /// A value of NumberOfLinesPerPage.
    /// </summary>
    [JsonPropertyName("numberOfLinesPerPage")]
    public Common NumberOfLinesPerPage
    {
      get => numberOfLinesPerPage ??= new();
      set => numberOfLinesPerPage = value;
    }

    /// <summary>
    /// A value of TotalAmountToFamily.
    /// </summary>
    [JsonPropertyName("totalAmountToFamily")]
    public Common TotalAmountToFamily
    {
      get => totalAmountToFamily ??= new();
      set => totalAmountToFamily = value;
    }

    /// <summary>
    /// A value of AmountForCollDate.
    /// </summary>
    [JsonPropertyName("amountForCollDate")]
    public Common AmountForCollDate
    {
      get => amountForCollDate ??= new();
      set => amountForCollDate = value;
    }

    /// <summary>
    /// A value of AmountCollectedOffset.
    /// </summary>
    [JsonPropertyName("amountCollectedOffset")]
    public Common AmountCollectedOffset
    {
      get => amountCollectedOffset ??= new();
      set => amountCollectedOffset = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// Gets a value of Print.
    /// </summary>
    [JsonIgnore]
    public Array<PrintGroup> Print => print ??= new(PrintGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Print for json serialization.
    /// </summary>
    [JsonPropertyName("print")]
    [Computed]
    public IList<PrintGroup> Print_Json
    {
      get => print;
      set => Print.Assign(value);
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

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of PreviousAr.
    /// </summary>
    [JsonPropertyName("previousAr")]
    public CsePerson PreviousAr
    {
      get => previousAr ??= new();
      set => previousAr = value;
    }

    /// <summary>
    /// A value of PreviousObligor.
    /// </summary>
    [JsonPropertyName("previousObligor")]
    public CsePerson PreviousObligor
    {
      get => previousObligor ??= new();
      set => previousObligor = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Collection Previous
    {
      get => previous ??= new();
      set => previous = value;
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

    private External external;
    private WorkArea recordType;
    private Collection totalForwardedToFamily;
    private Collection appliedAsArrears;
    private Collection appliedAsCurrent;
    private CsePerson apCsePerson;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private LegalAction legalAction;
    private DateWorkArea null1;
    private CsePerson derivedAr;
    private DateWorkArea dueDate;
    private Common caseFound;
    private DebtDetail debtDetail;
    private DprProgram dprProgram;
    private Program program;
    private FieldValue fieldValue;
    private Common lastOfImportGroup;
    private Common numberOfLinesPerPage;
    private Common totalAmountToFamily;
    private Common amountForCollDate;
    private Common amountCollectedOffset;
    private Common common;
    private Collection collection;
    private TextWorkArea textWorkArea;
    private Array<PrintGroup> print;
    private AbendData abendData;
    private CsePersonsWorkSet ar;
    private CsePerson previousAr;
    private CsePerson previousObligor;
    private Collection previous;
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
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
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
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    private CsePersonAccount supported1;
    private ObligationType obligationType;
    private CsePerson supported2;
    private ObligationTransaction debt;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private Obligation obligation;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private CsePerson ar;
    private Case1 case1;
    private CaseRole caseRole;
    private Office office;
    private OfficeAddress officeAddress;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
  }
#endregion
}
