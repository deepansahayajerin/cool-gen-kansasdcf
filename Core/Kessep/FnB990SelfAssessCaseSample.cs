// Program: FN_B990_SELF_ASSESS_CASE_SAMPLE, ID: 371033962, model: 746.
// Short name: SWEF990B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B990_SELF_ASSESS_CASE_SAMPLE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB990SelfAssessCaseSample: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B990_SELF_ASSESS_CASE_SAMPLE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB990SelfAssessCaseSample(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB990SelfAssessCaseSample.
  /// </summary>
  public FnB990SelfAssessCaseSample(IContext context, Import import,
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
    // ********************************************************************
    // MAINTENANCE LOG
    // PROGRAMMER   DATE      PR#     DESCRIPTION
    // ---------- ----------  ------  
    // ------------------------------------
    // Ed Lyman   03/12/2001  115278  Include cases where AP is not known.
    // ********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB990Housekeeping();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.CasesNotSelected.Count = 1;

    foreach(var item in ReadOfficeServiceProviderOfficeServiceProvider())
    {
      if (!Equal(entities.Office.Name, local.Last.Name))
      {
        if (!IsEmpty(local.Last.Name))
        {
          local.EabFileHandling.Action = "NEWPAGE";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "OFFICE:" + " " + entities.Office.Name;
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "";
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "WORKER #   CASE #       " + "AP #         " +
          "AP NAME";
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      local.Last.Name = entities.Office.Name;

      foreach(var item1 in ReadCase())
      {
        switch(AsChar(local.CaseStatus.Text1))
        {
          case 'B':
            break;
          case 'C':
            if (AsChar(entities.Case1.Status) != 'C')
            {
              continue;
            }

            break;
          case 'O':
            if (AsChar(entities.Case1.Status) != 'O')
            {
              continue;
            }

            break;
          default:
            break;
        }

        ++local.CasesRead.Count;

        if (local.CasesNotSelected.Count < local.SamplingFrequencyCommon.Count)
        {
          ++local.CasesNotSelected.Count;

          continue;
        }
        else
        {
          local.CasesNotSelected.Count = 1;
          ++local.CasesSampled.Count;

          foreach(var item2 in ReadCsePersonCaseUnit())
          {
            local.Ap.Number = entities.CsePerson.Number;
            UseSiReadCsePersonBatch();

            if (IsEmpty(local.Ap.FormattedName))
            {
              local.Ap.FormattedName = TrimEnd(local.Ap.LastName) + ", " + local
                .Ap.FirstName;
            }

            local.LastApNo.Text10 = entities.CsePerson.Number;
            local.LastCaseNo.Text10 = entities.Case1.Number;

            if (AsChar(entities.Case1.Status) == 'C')
            {
              local.EabReportSend.RptDetail =
                entities.ServiceProvider.UserId + "   " + entities
                .Case1.Number + "   " + entities.CsePerson.Number + "   " + local
                .Ap.FormattedName + "          " + "CLOSED";
            }
            else
            {
              local.EabReportSend.RptDetail =
                entities.ServiceProvider.UserId + "   " + entities
                .Case1.Number + "   " + entities.CsePerson.Number + "   " + local
                .Ap.FormattedName;
            }

            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            goto ReadEach;
          }

          local.LastApNo.Text10 = "";
          local.LastCaseNo.Text10 = entities.Case1.Number;

          if (AsChar(entities.Case1.Status) == 'C')
          {
            local.EabReportSend.RptDetail = entities.ServiceProvider.UserId + "   " +
              entities.Case1.Number + "   " + "          " + "   " + "No AP found.                     " +
              "          " + "CLOSED";
          }
          else
          {
            local.EabReportSend.RptDetail = entities.ServiceProvider.UserId + "   " +
              entities.Case1.Number + "   " + "          " + "   " + "No AP found.";
              
          }

          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.CasesWithNoAp.Count;
        }

ReadEach:
        ;
      }
    }

    UseFnB990Closing();
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB990Closing()
  {
    var useImport = new FnB990Closing.Import();
    var useExport = new FnB990Closing.Export();

    useImport.CasesWithNoAr.Count = local.CasesWithNoAp.Count;
    useImport.CasesSampled.Count = local.CasesSampled.Count;
    useImport.CasesRead.Count = local.CasesRead.Count;

    Call(FnB990Closing.Execute, useImport, useExport);
  }

  private void UseFnB990Housekeeping()
  {
    var useImport = new FnB990Housekeeping.Import();
    var useExport = new FnB990Housekeeping.Export();

    Call(FnB990Housekeeping.Execute, useImport, useExport);

    local.CaseStatus.Text1 = useExport.CaseStatus.Text1;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.SamplingFrequencyTextWorkArea.Text4 =
      useExport.SamplingFrequencyTextWorkArea.Text4;
    local.SamplingFrequencyCommon.Count =
      useExport.SamplingFrequencyCommon.Count;
    local.Max.Date = useExport.Max.Date;
    MoveEabFileHandling(useExport.FileHandling, local.EabFileHandling);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ap.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Ae.Flag = useExport.Ae.Flag;
    local.AbendData.Assign(useExport.AbendData);
    local.Ap.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseUnit()
  {
    entities.CsePerson.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCsePersonCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 0);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.CasNo = db.GetString(reader, 4);
        entities.CsePerson.Populated = true;
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider()
  {
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider",
      null,
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 3);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 3);
        entities.ServiceProvider.UserId = db.GetString(reader, 4);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 6);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;

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
    /// A value of CasesWithNoAp.
    /// </summary>
    [JsonPropertyName("casesWithNoAp")]
    public Common CasesWithNoAp
    {
      get => casesWithNoAp ??= new();
      set => casesWithNoAp = value;
    }

    /// <summary>
    /// A value of CaseStatus.
    /// </summary>
    [JsonPropertyName("caseStatus")]
    public TextWorkArea CaseStatus
    {
      get => caseStatus ??= new();
      set => caseStatus = value;
    }

    /// <summary>
    /// A value of CasesSampled.
    /// </summary>
    [JsonPropertyName("casesSampled")]
    public Common CasesSampled
    {
      get => casesSampled ??= new();
      set => casesSampled = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of SamplingFrequencyTextWorkArea.
    /// </summary>
    [JsonPropertyName("samplingFrequencyTextWorkArea")]
    public TextWorkArea SamplingFrequencyTextWorkArea
    {
      get => samplingFrequencyTextWorkArea ??= new();
      set => samplingFrequencyTextWorkArea = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Office Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of CasesNotSelected.
    /// </summary>
    [JsonPropertyName("casesNotSelected")]
    public Common CasesNotSelected
    {
      get => casesNotSelected ??= new();
      set => casesNotSelected = value;
    }

    /// <summary>
    /// A value of SamplingFrequencyCommon.
    /// </summary>
    [JsonPropertyName("samplingFrequencyCommon")]
    public Common SamplingFrequencyCommon
    {
      get => samplingFrequencyCommon ??= new();
      set => samplingFrequencyCommon = value;
    }

    /// <summary>
    /// A value of PriorityOne.
    /// </summary>
    [JsonPropertyName("priorityOne")]
    public Common PriorityOne
    {
      get => priorityOne ??= new();
      set => priorityOne = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
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
    /// A value of LastApNo.
    /// </summary>
    [JsonPropertyName("lastApNo")]
    public TextWorkArea LastApNo
    {
      get => lastApNo ??= new();
      set => lastApNo = value;
    }

    /// <summary>
    /// A value of LastCaseNo.
    /// </summary>
    [JsonPropertyName("lastCaseNo")]
    public TextWorkArea LastCaseNo
    {
      get => lastCaseNo ??= new();
      set => lastCaseNo = value;
    }

    /// <summary>
    /// A value of LastUserid.
    /// </summary>
    [JsonPropertyName("lastUserid")]
    public TextWorkArea LastUserid
    {
      get => lastUserid ??= new();
      set => lastUserid = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Begin.
    /// </summary>
    [JsonPropertyName("begin")]
    public DateWorkArea Begin
    {
      get => begin ??= new();
      set => begin = value;
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

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Common casesWithNoAp;
    private TextWorkArea caseStatus;
    private Common casesSampled;
    private Common casesRead;
    private ProgramProcessingInfo programProcessingInfo;
    private TextWorkArea samplingFrequencyTextWorkArea;
    private Office last;
    private Common casesNotSelected;
    private Common samplingFrequencyCommon;
    private Common priorityOne;
    private Common ae;
    private AbendData abendData;
    private TextWorkArea lastApNo;
    private TextWorkArea lastCaseNo;
    private TextWorkArea lastUserid;
    private DateWorkArea max;
    private CsePersonsWorkSet ap;
    private DateWorkArea begin;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private CsePerson csePerson;
    private Office office;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
