// Program: OE_B493_HINS_COVERAGE_ACTIVITY, ID: 372870835, model: 746.
// Short name: SWEE493B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B493_HINS_COVERAGE_ACTIVITY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB493HinsCoverageActivity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B493_HINS_COVERAGE_ACTIVITY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB493HinsCoverageActivity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB493HinsCoverageActivity.
  /// </summary>
  public OeB493HinsCoverageActivity(IContext context, Import import,
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
    // *******************************************************************
    // *   Date      Name            Request   Description               *
    // * ----------  --------------  --------  -----------------------   *
    // * 12/01/2002  Ed Lyman        WR 20311  Initial Coding            *
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.ReportNeeded.Flag = "Y";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseOeB493Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Process.Date = local.ProgramProcessingInfo.ProcessDate;

    // **********************************************************************
    // Read for each person covered by the health insurance policy.
    // **********************************************************************
    foreach(var item in ReadPersonalHealthInsuranceHealthInsuranceCoverage())
    {
      if (!Equal(entities.PersonalHealthInsurance.CoverageEndDate,
        local.Max.Date))
      {
        // ***********************************************************
        // Don't send closures that EDS requested to be closed.
        // ***********************************************************
        if (Equal(entities.PersonalHealthInsurance.LastUpdatedBy, "SWEEB497"))
        {
          continue;
        }
      }

      MoveHealthInsuranceCoverage1(entities.HealthInsuranceCoverage,
        local.HealthInsuranceCoverage);
      local.Beneficiary.Number = entities.Beneficiary.Number;
      UseOeB493GetPolicyInfo();

      if (AsChar(local.OneOrMoreCodesFound.Flag) == 'N')
      {
        local.NeededToWrite.RptDetail =
          "Skipped policy for beneficiary number: " + local
          .Beneficiary.Number + " because no coverage codes were designated." +
          "  ";
        UseCabErrorReport();
        ++local.RecordsSkippedNoCodes.Count;

        continue;
      }

      UseOeB493GetPolicyHolderInfo();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      if (IsEmpty(local.HealthInsuranceCoverage.InsurancePolicyNumber))
      {
        if (Lt("000000000", local.PolicyHolder.Ssn))
        {
          local.HealthInsuranceCoverage.InsurancePolicyNumber =
            local.PolicyHolder.Ssn;
        }
        else
        {
          local.NeededToWrite.RptDetail = "Skipped policy holder " + local
            .PolicyHolder.Number + "because no policy number was entered." + "  ";
            
          UseCabErrorReport();
          ++local.RecordsSkippedNoPolicy.Count;

          continue;
        }
      }

      ++local.RecordsWritten.Count;
      UseEabWriteHinsCoverageChanges();

      if (AsChar(local.ReportNeeded.Flag) == 'Y')
      {
        local.PolicyHolder.FormattedName =
          TrimEnd(local.PolicyHolder.LastName) + ", " + TrimEnd
          (local.PolicyHolder.FirstName) + " " + local
          .PolicyHolder.MiddleInitial + "";
        local.NeededToWrite.RptDetail = entities.Beneficiary.Number + " " + Substring
          (local.PolicyHolder.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 1, 3) + "-"
          + Substring
          (local.PolicyHolder.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 4, 2) + "-"
          + Substring
          (local.PolicyHolder.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 6, 4) + " " +
          Substring
          (local.PolicyHolder.FormattedName,
          CsePersonsWorkSet.FormattedName_MaxLength, 1, 27) + " " + entities
          .HealthInsuranceCompany.CarrierCode + " " + Substring
          (entities.HealthInsuranceCoverage.InsurancePolicyNumber,
          HealthInsuranceCoverage.InsurancePolicyNumber_MaxLength, 1, 12) + " " +
          Substring
          (entities.HealthInsuranceCoverage.InsuranceGroupNumber,
          HealthInsuranceCoverage.InsuranceGroupNumber_MaxLength, 1, 9);
        local.CoverageStart.Text10 =
          NumberToString(Month(
            entities.PersonalHealthInsurance.CoverageBeginDate), 14, 2) + "-"
          + NumberToString
          (Day(entities.PersonalHealthInsurance.CoverageBeginDate), 14, 2) + "-"
          + NumberToString
          (Year(entities.PersonalHealthInsurance.CoverageBeginDate), 12, 4);
        local.CoverageEnd.Text10 =
          NumberToString(
            Month(entities.PersonalHealthInsurance.CoverageEndDate), 14, 2) + "-"
          + NumberToString
          (Day(entities.PersonalHealthInsurance.CoverageEndDate), 14, 2) + "-"
          + NumberToString
          (Year(entities.PersonalHealthInsurance.CoverageEndDate), 12, 4);
        local.NeededToWrite.RptDetail =
          Substring(local.NeededToWrite.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 81) + " " + local
          .CoverageStart.Text10 + " " + local.CoverageEnd.Text10 + "" + "" + ""
          + "" + "" + "" + "" + "" + "" + "";
        local.NeededToWrite.RptDetail =
          Substring(local.NeededToWrite.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 103) + " " + entities
          .HealthInsuranceCoverage.CoverageCode1 + " " + entities
          .HealthInsuranceCoverage.CoverageCode2 + " " + entities
          .HealthInsuranceCoverage.CoverageCode3 + " " + entities
          .HealthInsuranceCoverage.CoverageCode4 + " " + entities
          .HealthInsuranceCoverage.CoverageCode5 + " " + entities
          .HealthInsuranceCoverage.CoverageCode6 + " " + entities
          .HealthInsuranceCoverage.CoverageCode7;
        UseCabBusinessReport01();
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB493Closing();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      local.BatchTimestampWorkArea.IefTimestamp = Now();
      UseLeCabConvertTimestamp();
      local.ProgramProcessingInfo.ParameterList = "Last Run=" + local
        .BatchTimestampWorkArea.TextTimestamp;
      UseUpdateProgramProcessingInfo();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseOeB493Closing();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private static void MoveHealthInsuranceCoverage1(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.PolicyPaidByCsePersonInd = source.PolicyPaidByCsePersonInd;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.CoverageCode1 = source.CoverageCode1;
    target.CoverageCode2 = source.CoverageCode2;
    target.CoverageCode3 = source.CoverageCode3;
    target.CoverageCode4 = source.CoverageCode4;
    target.CoverageCode5 = source.CoverageCode5;
    target.CoverageCode6 = source.CoverageCode6;
    target.CoverageCode7 = source.CoverageCode7;
  }

  private static void MoveHealthInsuranceCoverage2(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.CoverageCode1 = source.CoverageCode1;
    target.CoverageCode2 = source.CoverageCode2;
    target.CoverageCode3 = source.CoverageCode3;
    target.CoverageCode4 = source.CoverageCode4;
    target.CoverageCode5 = source.CoverageCode5;
    target.CoverageCode6 = source.CoverageCode6;
    target.CoverageCode7 = source.CoverageCode7;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabWriteHinsCoverageChanges()
  {
    var useImport = new EabWriteHinsCoverageChanges.Import();
    var useExport = new EabWriteHinsCoverageChanges.Export();

    useImport.HealthInsuranceCoverage.Assign(local.HealthInsuranceCoverage);
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    MoveEmployer(local.Employer, useImport.Employer);
    useImport.RelationToPolicyHolder.SelectChar =
      local.RelationToBeneficiary.SelectChar;
    useImport.CoveredPerson.Number = local.Beneficiary.Number;
    useImport.DateWorkArea.Date = local.Process.Date;
    useImport.HealthInsuranceCompany.CarrierCode =
      entities.HealthInsuranceCompany.CarrierCode;
    useImport.PolicyHolder.Assign(local.PolicyHolder);
    useImport.PersonalHealthInsurance.Assign(entities.PersonalHealthInsurance);
    useImport.Record.Count = local.RecordsWritten.Count;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteHinsCoverageChanges.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseOeB493Closing()
  {
    var useImport = new OeB493Closing.Import();
    var useExport = new OeB493Closing.Export();

    useImport.RecordsWritten.Count = local.RecordsWritten.Count;
    useImport.RecordsSkippedNoPolicy.Count = local.RecordsSkippedNoPolicy.Count;
    useImport.RecordsSkippedNoCodes.Count = local.RecordsSkippedNoCodes.Count;

    Call(OeB493Closing.Execute, useImport, useExport);
  }

  private void UseOeB493GetPolicyHolderInfo()
  {
    var useImport = new OeB493GetPolicyHolderInfo.Import();
    var useExport = new OeB493GetPolicyHolderInfo.Export();

    useImport.PersistentBeneficiary.Assign(entities.Beneficiary);
    useImport.Persistent.Assign(entities.HealthInsuranceCoverage);
    useImport.EdsRelationshipTable.Id = local.EdsRelationshipTable.Id;
    useImport.ProcessDate.Date = local.Process.Date;

    Call(OeB493GetPolicyHolderInfo.Execute, useImport, useExport);

    local.EmployerAddress.Assign(useExport.EmployerAddress);
    MoveEmployer(useExport.Employer, local.Employer);
    local.RelationToBeneficiary.SelectChar =
      useExport.RelationToBeneficiary.SelectChar;
    MoveCsePersonsWorkSet(useExport.PolicyHolder, local.PolicyHolder);
  }

  private void UseOeB493GetPolicyInfo()
  {
    var useImport = new OeB493GetPolicyInfo.Import();
    var useExport = new OeB493GetPolicyInfo.Export();

    useImport.HealthInsuranceCoverage.Assign(entities.HealthInsuranceCoverage);
    useImport.Coverages.Id = local.EdsCoverageCodes.Id;
    useImport.ProcessDate.Date = local.Process.Date;

    Call(OeB493GetPolicyInfo.Execute, useImport, useExport);

    local.OneOrMoreCodesFound.Flag = useExport.OneOrMoreCodesFound.Flag;
    MoveHealthInsuranceCoverage2(useExport.HealthInsuranceCoverage,
      local.HealthInsuranceCoverage);
  }

  private void UseOeB493Housekeeping()
  {
    var useImport = new OeB493Housekeeping.Import();
    var useExport = new OeB493Housekeeping.Export();

    Call(OeB493Housekeeping.Execute, useImport, useExport);

    local.EdsCoverageCodes.Id = useExport.EdsCoverageCode.Id;
    local.EdsRelationshipTable.Id = useExport.EdsRelationshipCode.Id;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.ProcessLastRun.Timestamp = useExport.LastRun.Timestamp;
  }

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadPersonalHealthInsuranceHealthInsuranceCoverage()
  {
    entities.Beneficiary.Populated = false;
    entities.PersonalHealthInsurance.Populated = false;
    entities.HealthInsuranceCoverage.Populated = false;
    entities.HealthInsuranceCompany.Populated = false;

    return ReadEach("ReadPersonalHealthInsuranceHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetDateTime(
          command, "lastUpdatedTmst",
          local.ProcessLastRun.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.Beneficiary.Number = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageVerifiedDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 3);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 4);
        entities.PersonalHealthInsurance.LastUpdatedBy =
          db.GetString(reader, 5);
        entities.PersonalHealthInsurance.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
          db.GetString(reader, 7);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 18);
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 18);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 19);
        entities.Beneficiary.Populated = true;
        entities.PersonalHealthInsurance.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of OneOrMoreCodesFound.
    /// </summary>
    [JsonPropertyName("oneOrMoreCodesFound")]
    public Common OneOrMoreCodesFound
    {
      get => oneOrMoreCodesFound ??= new();
      set => oneOrMoreCodesFound = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of EdsCoverageCodes.
    /// </summary>
    [JsonPropertyName("edsCoverageCodes")]
    public Code EdsCoverageCodes
    {
      get => edsCoverageCodes ??= new();
      set => edsCoverageCodes = value;
    }

    /// <summary>
    /// A value of EdsRelationshipTable.
    /// </summary>
    [JsonPropertyName("edsRelationshipTable")]
    public Code EdsRelationshipTable
    {
      get => edsRelationshipTable ??= new();
      set => edsRelationshipTable = value;
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of RelationToBeneficiary.
    /// </summary>
    [JsonPropertyName("relationToBeneficiary")]
    public Common RelationToBeneficiary
    {
      get => relationToBeneficiary ??= new();
      set => relationToBeneficiary = value;
    }

    /// <summary>
    /// A value of Beneficiary.
    /// </summary>
    [JsonPropertyName("beneficiary")]
    public CsePersonsWorkSet Beneficiary
    {
      get => beneficiary ??= new();
      set => beneficiary = value;
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
    /// A value of CoverageStart.
    /// </summary>
    [JsonPropertyName("coverageStart")]
    public TextWorkArea CoverageStart
    {
      get => coverageStart ??= new();
      set => coverageStart = value;
    }

    /// <summary>
    /// A value of CoverageEnd.
    /// </summary>
    [JsonPropertyName("coverageEnd")]
    public TextWorkArea CoverageEnd
    {
      get => coverageEnd ??= new();
      set => coverageEnd = value;
    }

    /// <summary>
    /// A value of ReportingMontyYear.
    /// </summary>
    [JsonPropertyName("reportingMontyYear")]
    public TextWorkArea ReportingMontyYear
    {
      get => reportingMontyYear ??= new();
      set => reportingMontyYear = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
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
    /// A value of ProgramFound.
    /// </summary>
    [JsonPropertyName("programFound")]
    public Common ProgramFound
    {
      get => programFound ??= new();
      set => programFound = value;
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
    /// A value of RecordsWritten.
    /// </summary>
    [JsonPropertyName("recordsWritten")]
    public Common RecordsWritten
    {
      get => recordsWritten ??= new();
      set => recordsWritten = value;
    }

    /// <summary>
    /// A value of RecordsSkippedNoPolicy.
    /// </summary>
    [JsonPropertyName("recordsSkippedNoPolicy")]
    public Common RecordsSkippedNoPolicy
    {
      get => recordsSkippedNoPolicy ??= new();
      set => recordsSkippedNoPolicy = value;
    }

    /// <summary>
    /// A value of RecordsSkippedNoCodes.
    /// </summary>
    [JsonPropertyName("recordsSkippedNoCodes")]
    public Common RecordsSkippedNoCodes
    {
      get => recordsSkippedNoCodes ??= new();
      set => recordsSkippedNoCodes = value;
    }

    /// <summary>
    /// A value of PolicyHolder.
    /// </summary>
    [JsonPropertyName("policyHolder")]
    public CsePersonsWorkSet PolicyHolder
    {
      get => policyHolder ??= new();
      set => policyHolder = value;
    }

    /// <summary>
    /// A value of PeriodEnd.
    /// </summary>
    [JsonPropertyName("periodEnd")]
    public DateWorkArea PeriodEnd
    {
      get => periodEnd ??= new();
      set => periodEnd = value;
    }

    /// <summary>
    /// A value of PeriodBegin.
    /// </summary>
    [JsonPropertyName("periodBegin")]
    public DateWorkArea PeriodBegin
    {
      get => periodBegin ??= new();
      set => periodBegin = value;
    }

    /// <summary>
    /// A value of ProcessLastRun.
    /// </summary>
    [JsonPropertyName("processLastRun")]
    public DateWorkArea ProcessLastRun
    {
      get => processLastRun ??= new();
      set => processLastRun = value;
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

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private DateWorkArea max;
    private Common oneOrMoreCodesFound;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Code edsCoverageCodes;
    private Code edsRelationshipTable;
    private ProgramProcessingInfo programProcessingInfo;
    private EmployerAddress employerAddress;
    private Employer employer;
    private Common relationToBeneficiary;
    private CsePersonsWorkSet beneficiary;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea coverageStart;
    private TextWorkArea coverageEnd;
    private TextWorkArea reportingMontyYear;
    private Common reportNeeded;
    private EabReportSend neededToWrite;
    private Common programFound;
    private AbendData abendData;
    private Common recordsWritten;
    private Common recordsSkippedNoPolicy;
    private Common recordsSkippedNoCodes;
    private CsePersonsWorkSet policyHolder;
    private DateWorkArea periodEnd;
    private DateWorkArea periodBegin;
    private DateWorkArea processLastRun;
    private DateWorkArea process;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Beneficiary.
    /// </summary>
    [JsonPropertyName("beneficiary")]
    public CsePerson Beneficiary
    {
      get => beneficiary ??= new();
      set => beneficiary = value;
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    private CsePerson beneficiary;
    private Program program;
    private PersonProgram personProgram;
    private PersonalHealthInsurance personalHealthInsurance;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
  }
#endregion
}
