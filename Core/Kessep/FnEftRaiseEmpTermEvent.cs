// Program: FN_EFT_RAISE_EMP_TERM_EVENT, ID: 372405500, model: 746.
// Short name: SWE02381
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EFT_RAISE_EMP_TERM_EVENT.
/// </summary>
[Serializable]
public partial class FnEftRaiseEmpTermEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EFT_RAISE_EMP_TERM_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEftRaiseEmpTermEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEftRaiseEmpTermEvent.
  /// </summary>
  public FnEftRaiseEmpTermEvent(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // First search for a match between the EFT company number and the system's 
    // employer ID.
    local.EabFileHandling.Action = "WRITE";

    if (AsChar(import.TraceIndicator.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "    Start of Alert for Employee Termination.";
      UseCabControlReport();
    }

    local.Employer.Identifier =
      (int)StringToNumber(import.ElectronicFundTransmission.
        CompanyIdentificationNumber);

    if (AsChar(import.TraceIndicator.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "    Employer number is " + NumberToString
        (local.Employer.Identifier, 15);
      UseCabControlReport();
    }

    ++export.NbrOfReads.Count;

    if (ReadEmployer1())
    {
      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "    Employer number found";
        UseCabControlReport();
      }
    }
    else
    {
      ++export.NbrOfReads.Count;

      if (ReadEmployer2())
      {
        if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "    Kansas id number found";
          UseCabControlReport();
        }
      }
      else
      {
        if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "    Kansas id number NOT found";
          UseCabControlReport();
        }

        // A not found should not stop processing - per Tim 6/10/99.
      }
    }

    if (entities.Employer.Populated)
    {
      ++export.NbrOfReads.Count;

      if (ReadIncomeSource())
      {
        if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.DateWorkArea.Date = entities.IncomeSource.EndDt;
          UseCabFormatDate();
          local.EabReportSend.RptDetail =
            "    Income_Source found with type of" + entities
            .IncomeSource.Type1 + " and end date of " + local
            .FormattedDate.Text10;
          UseCabControlReport();
        }

        if (Equal(entities.IncomeSource.EndDt, local.Initialized.Date) || Equal
          (entities.IncomeSource.EndDt, import.MaximumDate.Date))
        {
          ++export.NbrOfUpdates.Count;

          try
          {
            UpdateIncomeSource();

            if (AsChar(import.TraceIndicator.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail = "    Income_Source updated.";
              UseCabControlReport();
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ++export.NbrOfNonPendingErrors.Count;
                local.EabReportSend.RptDetail = "ID " + NumberToString
                  (import.ElectronicFundTransmission.TransmissionIdentifier, 15) +
                  " Error: Not Unique error updating Income Source for Employer Number " +
                  (
                    import.ElectronicFundTransmission.
                    CompanyIdentificationNumber ?? "");
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  // ***** Any error dealing with file handling is  "critical" 
                  // and will result in an abend. *****
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
                }

                return;
              case ErrorCode.PermittedValueViolation:
                ++export.NbrOfNonPendingErrors.Count;
                local.EabReportSend.RptDetail = "ID " + NumberToString
                  (import.ElectronicFundTransmission.TransmissionIdentifier, 15) +
                  " Error: Permitted Value violation updating Income Source for Employer Number " +
                  (
                    import.ElectronicFundTransmission.
                    CompanyIdentificationNumber ?? "");
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  // ***** Any error dealing with file handling is  "critical" 
                  // and will result in an abend. *****
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
                }

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "    Income_Source end date not blank.";
          UseCabControlReport();
        }
      }
      else
      {
        if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "    Income_Source NOT found.";
          UseCabControlReport();
        }

        // A not found should not stop processing - per Tim 6/10/99.
      }
    }

    local.Current.Date = Now().Date;
    local.Infrastructure.EventId = 10;
    local.Infrastructure.ReasonCode = "INCSTEFT";
    local.Infrastructure.BusinessObjectCd = "ICS";
    local.Infrastructure.InitiatingStateCode = "";
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = global.UserId;
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.CreatedTimestamp = Now();
    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.CsePersonNumber = import.CsePersonsWorkSet.Number;
    local.Infrastructure.Detail =
      TrimEnd(import.CsePersonsWorkSet.FormattedName) + ", " + import
      .CsePersonsWorkSet.Ssn + ", " + TrimEnd
      (import.ElectronicFundTransmission.CompanyName) + ", Emp Termination Ind = Y";
      
    local.Obligor.Number = import.CsePersonsWorkSet.Number;

    if (AsChar(import.TraceIndicator.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "     About to start reading for cases.";
      UseCabControlReport();
    }

    foreach(var item in ReadCaseCaseUnit())
    {
      ++export.NbrOfReads.Count;

      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "     Obligor # " + import
          .CsePersonsWorkSet.Number + "  Case # " + entities.Case1.Number + "  Case Unit # " +
          NumberToString(entities.CaseUnit.CuNumber, 15);
        UseCabControlReport();
      }

      local.Infrastructure.CaseNumber = entities.Case1.Number;
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

      if (ReadInterstateRequest())
      {
        ++export.NbrOfReads.Count;

        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "OS";
        }
      }
      else
      {
        local.Infrastructure.InitiatingStateCode = "KS";
      }

      ++export.NbrOfUpdates.Count;

      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "    ----Call to infrastructure AB, CSP Person  " + (
            local.Infrastructure.CsePersonNumber ?? "") + " Reason Code " + local
          .Infrastructure.ReasonCode + " Event Type " + local
          .Infrastructure.EventType;
        UseCabControlReport();
      }

      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++export.NbrOfNonPendingErrors.Count;
        local.TextEftIdentifier.Text9 =
          NumberToString(import.ElectronicFundTransmission.
            TransmissionIdentifier, 7, 9);
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "ID " + local
          .TextEftIdentifier.Text9 + " Infrastructure error: " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // ***** Any error dealing with file handling is  "critical" and will 
          // result in an abend. *****
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";

        return;
      }
      else if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "Event ID =" + NumberToString
          (local.Infrastructure.EventId, 15);
        UseCabControlReport();
      }
    }

    if (AsChar(import.TraceIndicator.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "     Finished reading for cases.";
      UseCabControlReport();
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFormatDate()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedDate.Text10 = useExport.FormattedDate.Text10;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private IEnumerable<bool> ReadCaseCaseUnit()
  {
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNoAp", import.CsePersonsWorkSet.Number);
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadEmployer1()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ein",
          import.ElectronicFundTransmission.CompanyIdentificationNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployer2()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "companyIdentificationNumber",
          import.ElectronicFundTransmission.CompanyIdentificationNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetNullableInt32(command, "empId", entities.Employer.Identifier);
        db.SetString(command, "cspINumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.IncomeSource.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.IncomeSource.CspINumber = db.GetString(reader, 4);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 5);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 6);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private void UpdateIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var endDt = import.ElectronicFundTransmission.FileCreationDate;

    entities.IncomeSource.Populated = false;
    Update("UpdateIncomeSource",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetDateTime(
          command, "identifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
      });

    entities.IncomeSource.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IncomeSource.LastUpdatedBy = lastUpdatedBy;
    entities.IncomeSource.EndDt = endDt;
    entities.IncomeSource.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of MaximumDate.
    /// </summary>
    [JsonPropertyName("maximumDate")]
    public DateWorkArea MaximumDate
    {
      get => maximumDate ??= new();
      set => maximumDate = value;
    }

    /// <summary>
    /// A value of TraceIndicator.
    /// </summary>
    [JsonPropertyName("traceIndicator")]
    public Common TraceIndicator
    {
      get => traceIndicator ??= new();
      set => traceIndicator = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private ElectronicFundTransmission electronicFundTransmission;
    private DateWorkArea maximumDate;
    private Common traceIndicator;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NbrOfReads.
    /// </summary>
    [JsonPropertyName("nbrOfReads")]
    public Common NbrOfReads
    {
      get => nbrOfReads ??= new();
      set => nbrOfReads = value;
    }

    /// <summary>
    /// A value of NbrOfUpdates.
    /// </summary>
    [JsonPropertyName("nbrOfUpdates")]
    public Common NbrOfUpdates
    {
      get => nbrOfUpdates ??= new();
      set => nbrOfUpdates = value;
    }

    /// <summary>
    /// A value of NbrOfNonPendingErrors.
    /// </summary>
    [JsonPropertyName("nbrOfNonPendingErrors")]
    public Common NbrOfNonPendingErrors
    {
      get => nbrOfNonPendingErrors ??= new();
      set => nbrOfNonPendingErrors = value;
    }

    private Common nbrOfReads;
    private Common nbrOfUpdates;
    private Common nbrOfNonPendingErrors;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of TextEftIdentifier.
    /// </summary>
    [JsonPropertyName("textEftIdentifier")]
    public WorkArea TextEftIdentifier
    {
      get => textEftIdentifier ??= new();
      set => textEftIdentifier = value;
    }

    /// <summary>
    /// A value of ActiveDebtFoundInd.
    /// </summary>
    [JsonPropertyName("activeDebtFoundInd")]
    public Common ActiveDebtFoundInd
    {
      get => activeDebtFoundInd ??= new();
      set => activeDebtFoundInd = value;
    }

    private WorkArea formattedDate;
    private DateWorkArea dateWorkArea;
    private CsePerson obligor;
    private DateWorkArea initialized;
    private DateWorkArea current;
    private Employer employer;
    private Infrastructure infrastructure;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private WorkArea textEftIdentifier;
    private Common activeDebtFoundInd;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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

    private Employer employer;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CsePerson obligor;
    private CaseRole absentParent;
    private InterstateRequest interstateRequest;
    private CsePerson supported;
  }
#endregion
}
