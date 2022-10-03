// Program: OE_B495_HINS_AVAILABILITY, ID: 372871836, model: 746.
// Short name: SWEE495B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B495_HINS_AVAILABILITY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB495HinsAvailability: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B495_HINS_AVAILABILITY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB495HinsAvailability(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB495HinsAvailability.
  /// </summary>
  public OeB495HinsAvailability(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.ReportNeeded.Flag = "Y";
    UseOeB495Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    foreach(var item in ReadHealthInsuranceAvailabilityCsePerson())
    {
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseEabReadCsePersonBatch2();

      switch(AsChar(local.AbendData.Type1))
      {
        case 'A':
          switch(TrimEnd(local.AbendData.AdabasResponseCd))
          {
            case "0113":
              ExitState = "FN0000_CSE_PERSON_UNKNOWN";
              local.NeededToWrite.RptDetail =
                "Adabas response code 113, person not found for " + entities
                .CsePerson.Number;

              break;
            case "0148":
              ExitState = "ADABAS_UNAVAILABLE_RB";
              local.NeededToWrite.RptDetail =
                "Adabas response code 148, unavailable fetching person " + entities
                .CsePerson.Number;

              break;
            default:
              ExitState = "ADABAS_READ_UNSUCCESSFUL";
              local.NeededToWrite.RptDetail = "Adabas error. Type = " + local
                .AbendData.Type1 + " File number = " + local
                .AbendData.AdabasFileNumber + " File action = " + local
                .AbendData.AdabasFileAction + " Response code = " + local
                .AbendData.AdabasResponseCd + " Person number = " + entities
                .CsePerson.Number;

              break;
          }

          break;
        case 'C':
          if (IsEmpty(local.AbendData.CicsResponseCd))
          {
          }
          else
          {
            ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
            local.NeededToWrite.RptDetail =
              "CICS error fetching person number  " + entities
              .CsePerson.Number;
          }

          break;
        case ' ':
          break;
        default:
          ExitState = "ADABAS_INVALID_RETURN_CODE";
          local.NeededToWrite.RptDetail =
            "Unknown error fetching person number  " + entities
            .CsePerson.Number;

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseCabErrorReport2();

        break;
      }

      local.CsePersonsWorkSet.FormattedName =
        TrimEnd(local.CsePersonsWorkSet.LastName) + ", " + TrimEnd
        (local.CsePersonsWorkSet.FirstName) + " " + local
        .CsePersonsWorkSet.MiddleInitial + "";
      ++local.Record.Count;
      UseEabWriteHinsAvailabilityChgs();

      if (AsChar(local.ReportNeeded.Flag) == 'Y')
      {
        // ************** beneficiary number *************************
        local.NeededToWrite.RptDetail = "BENEFICIARY NUMBER: " + entities
          .CsePerson.Number;
        UseCabBusinessReport01();

        // ************** beneficiary name *************************
        local.NeededToWrite.RptDetail = "BENEFICIARY NAME  : " + local
          .CsePersonsWorkSet.FormattedName;
        UseCabBusinessReport01();

        // ************** blank line 
        // *******************************
        local.NeededToWrite.RptDetail = "";
        UseCabBusinessReport01();

        // ************** policy number *************************
        local.NeededToWrite.RptDetail = "POLICY NUMBER     : " + entities
          .HealthInsuranceAvailability.InsurancePolicyNumber;
        UseCabBusinessReport01();

        // ************** group number *************************
        local.NeededToWrite.RptDetail = "GROUP NUMBER      : " + entities
          .HealthInsuranceAvailability.InsuranceGroupNumber;
        UseCabBusinessReport01();

        // ************** insurer name *************************
        local.NeededToWrite.RptDetail = "INSURER NAME      : " + entities
          .HealthInsuranceAvailability.InsuranceName;
        UseCabBusinessReport01();

        // ************** insurer address 1 *************************
        local.NeededToWrite.RptDetail = "        ADDRESS   : " + entities
          .HealthInsuranceAvailability.Street1;
        UseCabBusinessReport01();

        // ************** insurer address 2 *************************
        if (!IsEmpty(entities.HealthInsuranceAvailability.Street2))
        {
          local.NeededToWrite.RptDetail = "                  : " + entities
            .HealthInsuranceAvailability.Street2;
          UseCabBusinessReport01();
        }

        // ************** insurer city, st, zip *************************
        if (!IsEmpty(entities.HealthInsuranceAvailability.Zip4))
        {
          local.NeededToWrite.RptDetail = "     CITY, ST, ZIP: " + TrimEnd
            (entities.HealthInsuranceAvailability.City) + ", " + entities
            .HealthInsuranceAvailability.State + " " + entities
            .HealthInsuranceAvailability.Zip5 + "-" + entities
            .HealthInsuranceAvailability.Zip4;
        }
        else
        {
          local.NeededToWrite.RptDetail = "     CITY, ST, ZIP: " + TrimEnd
            (entities.HealthInsuranceAvailability.City) + ", " + entities
            .HealthInsuranceAvailability.State + " " + entities
            .HealthInsuranceAvailability.Zip5;
        }

        UseCabBusinessReport01();

        // ************** verified date **************************
        local.VerifiedDate.Text10 =
          NumberToString(
            Month(entities.HealthInsuranceAvailability.VerifiedDate), 14, 2) + "-"
          + NumberToString
          (Day(entities.HealthInsuranceAvailability.VerifiedDate), 14, 2) + "-"
          + NumberToString
          (Year(entities.HealthInsuranceAvailability.VerifiedDate), 12, 4);
        local.NeededToWrite.RptDetail = "VERIFIED DATE     : " + local
          .VerifiedDate.Text10;
        UseCabBusinessReport01();

        // ************** end date 
        // *******************************
        local.EndDate.Text10 =
          NumberToString(Month(entities.HealthInsuranceAvailability.EndDate),
          14, 2) + "-" + NumberToString
          (Day(entities.HealthInsuranceAvailability.EndDate), 14, 2) + "-" + NumberToString
          (Year(entities.HealthInsuranceAvailability.EndDate), 12, 4);
        local.NeededToWrite.RptDetail = "     END DATE     : " + local
          .EndDate.Text10;
        UseCabBusinessReport01();

        // ************** blank line 
        // *******************************
        local.NeededToWrite.RptDetail = "";
        UseCabBusinessReport01();

        // ************** employer name 
        // *******************************
        local.NeededToWrite.RptDetail = "EMPLOYER NAME     : " + entities
          .HealthInsuranceAvailability.EmployerName;
        UseCabBusinessReport01();

        // ************** employer address 1 *************************
        local.NeededToWrite.RptDetail = "         ADDRESS  : " + entities
          .HealthInsuranceAvailability.EmpStreet1;
        UseCabBusinessReport01();

        // ************** employer address 2 *************************
        if (!IsEmpty(entities.HealthInsuranceAvailability.EmpStreet2))
        {
          local.NeededToWrite.RptDetail = "                  : " + entities
            .HealthInsuranceAvailability.EmpStreet2;
          UseCabBusinessReport01();
        }

        // ************** employer city, st, zip *************************
        if (!IsEmpty(entities.HealthInsuranceAvailability.EmpZip4))
        {
          local.NeededToWrite.RptDetail = "     CITY, ST, ZIP: " + TrimEnd
            (entities.HealthInsuranceAvailability.EmpCity) + ", " + entities
            .HealthInsuranceAvailability.EmpState + " " + entities
            .HealthInsuranceAvailability.EmpZip5 + "-" + entities
            .HealthInsuranceAvailability.EmpZip4;
        }
        else
        {
          local.NeededToWrite.RptDetail = "     CITY, ST, ZIP: " + TrimEnd
            (entities.HealthInsuranceAvailability.EmpCity) + ", " + entities
            .HealthInsuranceAvailability.EmpState + " " + entities
            .HealthInsuranceAvailability.EmpZip5;
        }

        UseCabBusinessReport01();

        // ************** phone 
        // ***********************************
        local.NeededToWrite.RptDetail = "         PHONE    : " + "(" + NumberToString
          (entities.HealthInsuranceAvailability.EmpPhoneAreaCode, 13, 3) + ") " +
          NumberToString
          (entities.HealthInsuranceAvailability.EmpPhoneNo.GetValueOrDefault(),
          9, 3) + "-" + NumberToString
          (entities.HealthInsuranceAvailability.EmpPhoneNo.GetValueOrDefault(),
          12, 4);
        UseCabBusinessReport01();

        // ************** blank line 
        // *******************************
        local.NeededToWrite.RptDetail = "";
        UseCabBusinessReport01();

        // ************** blank line 
        // *******************************
        local.NeededToWrite.RptDetail = "";
        UseCabBusinessReport01();
      }
    }

    // **********************************************************
    // CLOSE ADABAS IS AVAILABLE
    // **********************************************************
    local.CsePersonsWorkSet.Number = "CLOSE";
    UseEabReadCsePersonBatch1();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB495Closing();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseOeB495Closing();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
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

  private static void MoveDateWorkArea1(DateWorkArea source, DateWorkArea target)
    
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveDateWorkArea2(DateWorkArea source, DateWorkArea target)
    
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);
  }

  private void UseEabReadCsePersonBatch1()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabReadCsePersonBatch2()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useExport.AbendData.Assign(local.AbendData);
    useExport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseEabWriteHinsAvailabilityChgs()
  {
    var useImport = new EabWriteHinsAvailabilityChgs.Import();
    var useExport = new EabWriteHinsAvailabilityChgs.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useImport.HealthInsuranceAvailability.Assign(
      entities.HealthInsuranceAvailability);
    MoveDateWorkArea1(local.Current, useImport.DateWorkArea);
    useImport.Record.Count = local.Record.Count;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteHinsAvailabilityChgs.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeB495Closing()
  {
    var useImport = new OeB495Closing.Import();
    var useExport = new OeB495Closing.Export();

    useImport.RecordCount.Count = local.Record.Count;

    Call(OeB495Closing.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeB495Housekeeping()
  {
    var useImport = new OeB495Housekeeping.Import();
    var useExport = new OeB495Housekeeping.Export();

    Call(OeB495Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    MoveDateWorkArea2(useExport.BeginPeriod, local.PeriodBeginDateWorkArea);
    local.PeriodBeginTextWorkArea.Text10 = useExport.PeriodBegin.Text10;
    MoveDateWorkArea2(useExport.EndPeriod, local.PeriodEndDateWorkArea);
    local.PeriodEndTextWorkArea.Text10 = useExport.PeriodEnd.Text10;
    local.ReportingMontyYear.Text30 = useExport.ReportingMonthYear.Text30;
  }

  private IEnumerable<bool> ReadHealthInsuranceAvailabilityCsePerson()
  {
    entities.HealthInsuranceAvailability.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadHealthInsuranceAvailabilityCsePerson",
      (db, command) =>
      {
        db.SetDate(
          command, "date1",
          local.PeriodBeginDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2",
          local.PeriodEndDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.HealthInsuranceAvailability.InsuranceId =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceAvailability.InsurancePolicyNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceAvailability.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceAvailability.InsuranceName =
          db.GetString(reader, 3);
        entities.HealthInsuranceAvailability.Street1 =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceAvailability.Street2 =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceAvailability.City =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceAvailability.State =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceAvailability.Zip5 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceAvailability.Zip4 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceAvailability.VerifiedDate =
          db.GetNullableDate(reader, 10);
        entities.HealthInsuranceAvailability.EndDate =
          db.GetNullableDate(reader, 11);
        entities.HealthInsuranceAvailability.EmployerName =
          db.GetString(reader, 12);
        entities.HealthInsuranceAvailability.EmpStreet1 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceAvailability.EmpStreet2 =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceAvailability.EmpCity =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceAvailability.EmpState =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceAvailability.EmpZip5 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceAvailability.EmpZip4 =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceAvailability.EmpPhoneAreaCode =
          db.GetInt32(reader, 19);
        entities.HealthInsuranceAvailability.EmpPhoneNo =
          db.GetNullableInt32(reader, 20);
        entities.HealthInsuranceAvailability.CspNumber =
          db.GetString(reader, 21);
        entities.CsePerson.Number = db.GetString(reader, 21);
        entities.HealthInsuranceAvailability.Populated = true;
        entities.CsePerson.Populated = true;

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
    /// A value of CompanyAddressFound.
    /// </summary>
    [JsonPropertyName("companyAddressFound")]
    public Common CompanyAddressFound
    {
      get => companyAddressFound ??= new();
      set => companyAddressFound = value;
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
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PeriodEndDateWorkArea.
    /// </summary>
    [JsonPropertyName("periodEndDateWorkArea")]
    public DateWorkArea PeriodEndDateWorkArea
    {
      get => periodEndDateWorkArea ??= new();
      set => periodEndDateWorkArea = value;
    }

    /// <summary>
    /// A value of PeriodBeginDateWorkArea.
    /// </summary>
    [JsonPropertyName("periodBeginDateWorkArea")]
    public DateWorkArea PeriodBeginDateWorkArea
    {
      get => periodBeginDateWorkArea ??= new();
      set => periodBeginDateWorkArea = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
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
    /// A value of ReportingMontyYear.
    /// </summary>
    [JsonPropertyName("reportingMontyYear")]
    public TextWorkArea ReportingMontyYear
    {
      get => reportingMontyYear ??= new();
      set => reportingMontyYear = value;
    }

    /// <summary>
    /// A value of PeriodEndTextWorkArea.
    /// </summary>
    [JsonPropertyName("periodEndTextWorkArea")]
    public TextWorkArea PeriodEndTextWorkArea
    {
      get => periodEndTextWorkArea ??= new();
      set => periodEndTextWorkArea = value;
    }

    /// <summary>
    /// A value of PeriodBeginTextWorkArea.
    /// </summary>
    [JsonPropertyName("periodBeginTextWorkArea")]
    public TextWorkArea PeriodBeginTextWorkArea
    {
      get => periodBeginTextWorkArea ??= new();
      set => periodBeginTextWorkArea = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of VerifiedDate.
    /// </summary>
    [JsonPropertyName("verifiedDate")]
    public TextWorkArea VerifiedDate
    {
      get => verifiedDate ??= new();
      set => verifiedDate = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public TextWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    private Common companyAddressFound;
    private AbendData abendData;
    private Common record;
    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea periodEndDateWorkArea;
    private DateWorkArea periodBeginDateWorkArea;
    private DateWorkArea process;
    private EabFileHandling eabFileHandling;
    private CsePerson csePerson;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Case1 case1;
    private Contact contact;
    private Common reportNeeded;
    private TextWorkArea reportingMontyYear;
    private TextWorkArea periodEndTextWorkArea;
    private TextWorkArea periodBeginTextWorkArea;
    private Common programFound;
    private EabReportSend neededToWrite;
    private TextWorkArea verifiedDate;
    private TextWorkArea endDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private HealthInsuranceAvailability healthInsuranceAvailability;
    private PersonProgram personProgram;
    private CsePerson csePerson;
    private Program program;
  }
#endregion
}
