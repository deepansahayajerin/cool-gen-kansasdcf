// Program: FN_B603_KCP_FVI_EXTRACT, ID: 945057973, model: 746.
// Short name: SWEF603B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B603_KCP_FVI_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB603KcpFviExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B603_KCP_FVI_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB603KcpFviExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB603KcpFviExtract.
  /// </summary>
  public FnB603KcpFviExtract(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Date		Developer	Request		Desc
    // ----------------------------------------------------------------------------------------------------
    // 12/09/2010	J Huss		CQ# 9690	Initial Development
    // 01/24/2011	Raj S		CQ# 9690	Modified to prefix the record type(1 byte 
    // value)
    //                                                 
    // for each record. Below mentioned values will be
    // used
    //                                                 
    // based on the information.
    //                                                 
    // 1 -  Header Record
    //                                                 
    // 2 -  Detail Record
    //                                                 
    // 9 -  Footer Record
    // 03/18/2011	Raj S		CQ# 9690	Modified to remove the modified user id and 
    // FVI set date
    //                                                 
    // field values from  detial record layout.
    // ----------------------------------------------------------------------------------------------------
    // *********************************************************************
    // This program reads for any NCP/CP whose FVI has been updated since the
    // last run date and is known to a court order that has previously been
    // supplied to the KPC (via SRRUN251).  It writes those individuals to an
    // extract file that is sent to the KPC.
    // *********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB603Housekeeping();
    local.EabFileHandling.Action = "WRITE";
    local.Add.Count = 0;
    local.Remove.Count = 0;
    local.Total.Count = 0;

    // Find any NCP/CP whose FVI has been updated since the last run date
    // and is known to a legal action that has been sent to the KPC.
    foreach(var item in ReadCsePerson())
    {
      // If the current and previous person numbers are the same, this person 
      // has already been processed
      if (Equal(entities.CsePerson.Number, local.PreviousCsePerson.Number))
      {
        continue;
      }

      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseCabReadAdabasPersonBatch();

      if (IsExitState("ADABAS_UNAVAILABLE_RB"))
      {
        // ADABAS is unavailable.  Write a note to the error report and bail 
        // out.
        local.NeededToWrite.RptDetail =
          "ADABAS unavailable.  Response code:  " + local
          .AbendData.AdabasResponseCd;
        UseCabErrorReport();
        ExitState = "ACO_AE0000_BATCH_ABEND";

        return;
      }
      else if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
      {
        // The requested person was not found on ADABAS.  Write a note to the 
        // error report and continue.
        local.NeededToWrite.RptDetail = "CSE Person " + entities
          .CsePerson.Number + " was not found on ADABAS.  Response Code: " + local
          .AbendData.AdabasResponseCd;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
      {
        local.FvValue.Flag = "A";
        ++local.Add.Count;
      }
      else
      {
        local.FvValue.Flag = "R";
        ++local.Remove.Count;
      }

      // Write the extract report record
      // 	RAAAAAAAAABBBBBBBBBBBBBBBBBCCCCCCCCCCCCDEEEEEEEEEEF
      // 	Where
      //                 R = Record Type (1 Char - Valid Values 1,2 & 9)
      // 		A = SSN (9 chars)
      // 		B = Last Name (17 chars)
      // 		C = First Name (12 chars)
      // 		D = Middle Initial (1 char)
      // 		E = CSE Person Number (10 chars)
      // 		F = Family Violence Add or Remove Indicator (1 char)
      local.NeededToWrite.RptDetail = local.CsePersonsWorkSet.Ssn + local
        .CsePersonsWorkSet.LastName + local.CsePersonsWorkSet.FirstName + local
        .CsePersonsWorkSet.MiddleInitial + entities.CsePerson.Number + local
        .FvValue.Flag;

      // Since this is a Detail record Prefix Record Type "2" before the record 
      // before Write to the extract file
      local.NeededToWrite.RptDetail = "2" + TrimEnd
        (local.NeededToWrite.RptDetail);
      UseFnB603EabExtractFile();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        return;
      }

      ++local.Total.Count;
      local.PreviousCsePerson.Number = entities.CsePerson.Number;
    }

    UseFnB603WriteControlsAndClose();
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

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseFnB603EabExtractFile()
  {
    var useImport = new FnB603EabExtractFile.Import();
    var useExport = new FnB603EabExtractFile.Export();

    useImport.EabReportSend.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB603EabExtractFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB603Housekeeping()
  {
    var useImport = new FnB603Housekeeping.Import();
    var useExport = new FnB603Housekeeping.Export();

    Call(FnB603Housekeeping.Execute, useImport, useExport);

    local.LastRunDate.Date = useExport.LastRunDate.Date;
    local.ProcessingDate.Date = useExport.ProcessingDate.Date;
  }

  private void UseFnB603WriteControlsAndClose()
  {
    var useImport = new FnB603WriteControlsAndClose.Import();
    var useExport = new FnB603WriteControlsAndClose.Export();

    useImport.Total.Count = local.Total.Count;
    useImport.Add.Count = local.Add.Count;
    useImport.Remove.Count = local.Remove.Count;
    useImport.ProcessingDate.Date = local.ProcessingDate.Date;

    Call(FnB603WriteControlsAndClose.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "fviSetDate1", local.LastRunDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fviSetDate2",
          local.ProcessingDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 5);
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
    /// A value of Add.
    /// </summary>
    [JsonPropertyName("add")]
    public Common Add
    {
      get => add ??= new();
      set => add = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of FvValue.
    /// </summary>
    [JsonPropertyName("fvValue")]
    public Common FvValue
    {
      get => fvValue ??= new();
      set => fvValue = value;
    }

    /// <summary>
    /// A value of LastRunDate.
    /// </summary>
    [JsonPropertyName("lastRunDate")]
    public DateWorkArea LastRunDate
    {
      get => lastRunDate ??= new();
      set => lastRunDate = value;
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
    /// A value of PreviousCsePerson.
    /// </summary>
    [JsonPropertyName("previousCsePerson")]
    public CsePerson PreviousCsePerson
    {
      get => previousCsePerson ??= new();
      set => previousCsePerson = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
    }

    /// <summary>
    /// A value of ProcessingDate.
    /// </summary>
    [JsonPropertyName("processingDate")]
    public DateWorkArea ProcessingDate
    {
      get => processingDate ??= new();
      set => processingDate = value;
    }

    /// <summary>
    /// A value of Remove.
    /// </summary>
    [JsonPropertyName("remove")]
    public Common Remove
    {
      get => remove ??= new();
      set => remove = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Common Total
    {
      get => total ??= new();
      set => total = value;
    }

    private AbendData abendData;
    private Common add;
    private CsePersonsWorkSet csePersonsWorkSet;
    private EabFileHandling eabFileHandling;
    private Common fvValue;
    private DateWorkArea lastRunDate;
    private EabReportSend neededToWrite;
    private CsePerson previousCsePerson;
    private LegalAction previousLegalAction;
    private DateWorkArea processingDate;
    private Common remove;
    private Common total;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson legalActionPerson;
  }
#endregion
}
