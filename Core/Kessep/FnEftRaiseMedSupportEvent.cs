// Program: FN_EFT_RAISE_MED_SUPPORT_EVENT, ID: 372405507, model: 746.
// Short name: SWE02418
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EFT_RAISE_MED_SUPPORT_EVENT.
/// </summary>
[Serializable]
public partial class FnEftRaiseMedSupportEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EFT_RAISE_MED_SUPPORT_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEftRaiseMedSupportEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEftRaiseMedSupportEvent.
  /// </summary>
  public FnEftRaiseMedSupportEvent(IContext context, Import import,
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
    local.EabFileHandling.Action = "WRITE";

    if (ReadPersonalHealthInsurance())
    {
      ++export.NbrOfReads.Count;

      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "*Insurance found - escape from AB.  " + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
        UseCabControlReport();
      }

      return;
    }

    local.Current.Date = Now().Date;
    local.Infrastructure.EventId = 5;
    local.Infrastructure.ReasonCode = "MEDSUPPEREFT";
    local.Infrastructure.BusinessObjectCd = "HIN";
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
      (import.ElectronicFundTransmission.CompanyName) + ".";
    local.Obligor.Number = import.CsePersonsWorkSet.Number;

    if (AsChar(import.TraceIndicator.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "     About to start reading for cases.";
      UseCabControlReport();
    }

    foreach(var item in ReadCaseCaseUnit())
    {
      ++export.NbrOfReads.Count;
      local.Infrastructure.CaseNumber = entities.Case1.Number;
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "     Obligor # " + import
          .CsePersonsWorkSet.Number + "  Case # " + entities.Case1.Number + "  Case Unit # " +
          NumberToString(entities.CaseUnit.CuNumber, 15);
        UseCabControlReport();
      }

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

      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "    About to call infrastructure cab";
        UseCabControlReport();
      }

      ++export.NbrOfUpdates.Count;
      UseSpCabCreateInfrastructure();

      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "Back from call to infratructure cab";
        UseCabControlReport();
      }

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
    }

    if (AsChar(import.TraceIndicator.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "     Finished reading for cases.";
      UseCabControlReport();
    }

    if (!IsEmpty(local.Infrastructure.CaseNumber))
    {
      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "         At least one infrastructure record created.";
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // ***** Any error dealing with file handling is  "critical" and will 
          // result in an abend. *****
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }
      }
    }
    else if (AsChar(import.TraceIndicator.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "           No infrastructure records created.";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // ***** Any error dealing with file handling is  "critical" and will 
        // result in an abend. *****
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
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
    entities.CaseUnit.Populated = false;
    entities.Case1.Populated = false;

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
        entities.CaseUnit.Populated = true;
        entities.Case1.Populated = true;

        return true;
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

  private bool ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "coverBeginDate",
          import.ElectronicFundTransmission.FileCreationDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 3);
        entities.PersonalHealthInsurance.Populated = true;
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
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
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
    /// A value of TraceIndicator.
    /// </summary>
    [JsonPropertyName("traceIndicator")]
    public Common TraceIndicator
    {
      get => traceIndicator ??= new();
      set => traceIndicator = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
    private CsePersonsWorkSet csePersonsWorkSet;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private CsePerson obligor;
    private Common activeDebtFoundInd;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private WorkArea textEftIdentifier;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson obligor;
    private CsePerson supported;
    private CaseUnit caseUnit;
    private CaseRole absentParent;
    private InterstateRequest interstateRequest;
    private Case1 case1;
    private ElectronicFundTransmission electronicFundTransmission;
  }
#endregion
}
