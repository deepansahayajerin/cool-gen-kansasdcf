// Program: OE_B456_PROCESS_KDMV_RESPONSE, ID: 371367331, model: 746.
// Short name: SWE03604
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B456_PROCESS_KDMV_RESPONSE.
/// </summary>
[Serializable]
public partial class OeB456ProcessKdmvResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B456_PROCESS_KDMV_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB456ProcessKdmvResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB456ProcessKdmvResponse.
  /// </summary>
  public OeB456ProcessKdmvResponse(IContext context, Import import,
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
    export.TotalNumberOfRecords.Count = import.TotalNumberOfRecords.Count;
    export.CsePersonLicenseUpdate.Count = import.CsePersonLicenseUpdate.Count;
    export.KdmvDriverLicenseUpdat.Count = import.KdmvDriverLicenseUpdat.Count;
    export.OuputTotalErrorRecords.Count = import.TotalErrorRecords.Count;
    local.KsDriversLicense.KsDvrLicense = import.KdmvFile.DriverLicenseNumber;

    if (ReadCsePerson())
    {
      if (ReadCsePersonLicense())
      {
        local.CsePersonLicense.Assign(entities.CsePersonLicense);

        // Set the new  ks  driver's license number (attribute) to the incoming 
        // driver's license number from kdmv.
        if (!Equal(entities.CsePersonLicense.Number,
          import.KdmvFile.DriverLicenseNumber))
        {
          if (Equal(entities.CsePersonLicense.IssuingState, "KS") || IsEmpty
            (entities.CsePersonLicense.IssuingState))
          {
            local.CsePersonLicense.IssuingState = "KS";
            local.CsePersonLicense.Number = import.KdmvFile.DriverLicenseNumber;
            local.CsePersonLicense.Note =
              "Recieved driver's license update from DMV";
            local.CsePersonLicense.ExpirationDt =
              UseCabSetMaximumDiscontinueDate();
            UseSiUpdateCsePersonLicense();
            ++export.CsePersonLicenseUpdate.Count;
          }
        }
      }
      else
      {
        // If there is no record then we will  add a cse-person-license record
        local.CsePersonLicense.IssuingState = "KS";
        local.CsePersonLicense.Type1 = "D";
        local.CsePersonLicense.Number = import.KdmvFile.DriverLicenseNumber;
        local.CsePersonLicense.IssuingAgencyName = "DMV";
        local.CsePersonLicense.Note =
          "Recieved a driver's license number from DMV";
        local.CsePersonLicense.Description = "KS driver's license";
        local.CsePersonLicense.StartDt =
          import.ProgramProcessingInfo.ProcessDate;
        local.CsePersonLicense.ExpirationDt = UseCabSetMaximumDiscontinueDate();
        UseSiCreateCsePersonLicense();
        ++export.CsePersonLicenseUpdate.Count;
      }

      local.ProcessedCrtOrder.Index = -1;
      local.ProcessedCrtOrder.Count = 0;

      foreach(var item in ReadKsDriversLicense())
      {
        local.Previous.KsDvrLicense = entities.KsDriversLicense.KsDvrLicense;
        ReadLegalAction();
        local.DriversLicenseChanged.Flag = "";
        local.PreviousCtOrderFound.Flag = "";

        if (local.ProcessedCrtOrder.Index >= 0 && entities
          .LegalAction.Populated)
        {
          for(local.ProcessedCrtOrder.Index = 0; local
            .ProcessedCrtOrder.Index < local.ProcessedCrtOrder.Count; ++
            local.ProcessedCrtOrder.Index)
          {
            if (!local.ProcessedCrtOrder.CheckSize())
            {
              break;
            }

            if (local.ProcessedCrtOrder.Item.ProcessedCtOrder.Identifier == entities
              .LegalAction.Identifier)
            {
              local.PreviousCtOrderFound.Flag = "Y";

              goto Test;
            }
          }

          local.ProcessedCrtOrder.CheckIndex();

          local.ProcessedCrtOrder.Index = local.ProcessedCrtOrder.Count - 1;
          local.ProcessedCrtOrder.CheckSize();
        }

Test:

        if (AsChar(local.PreviousCtOrderFound.Flag) != 'Y')
        {
          // **********************************************************************************************
          // we only want to process the most current record for an ap and for a
          // accourt
          // order,  all other recourds are consider history reccords or closed
          // **********************************************************************************************
          local.KsDriversLicense.KsDvrLicense =
            entities.KsDriversLicense.KsDvrLicense;

          if (!Equal(entities.KsDriversLicense.KsDvrLicense,
            import.KdmvFile.DriverLicenseNumber))
          {
            local.KsDriversLicense.KsDvrLicense =
              import.KdmvFile.DriverLicenseNumber;
            local.DriversLicenseChanged.Flag = "Y";
            ++export.KdmvDriverLicenseUpdat.Count;
          }

          if (entities.LegalAction.Populated)
          {
            // **********************************************************************************************
            // IF THERE IS NO LEGAL ACTION THEN THERE IS NO REASON TO TRY
            // UPDATE THE GROUP VIEW, THIS RECORD IS ONLY A SHELL RECORD
            // **********************************************************************************************
            ++local.ProcessedCrtOrder.Index;
            local.ProcessedCrtOrder.CheckSize();

            local.ProcessedCrtOrder.Update.ProcessedCtOrder.Identifier =
              entities.LegalAction.Identifier;
          }

          try
          {
            UpdateKsDriversLicense();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ++export.OuputTotalErrorRecords.Count;
                ExitState = "KS_DRIVERS_LICENSE_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "KS_DRIVERS_LICENSE_PV";
                ++export.OuputTotalErrorRecords.Count;

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (AsChar(local.DriversLicenseChanged.Flag) == 'Y')
          {
            local.Infrastructure.SituationNumber = 0;
            local.Infrastructure.ReasonCode = "KDLVALIDSTATUS";
            local.Infrastructure.EventId = 1;
            local.Infrastructure.EventType = "ADMINACT";
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.UserId = "KDMV";
            local.Infrastructure.BusinessObjectCd = "ENF";
            local.Infrastructure.ReferenceDate = local.StartDate.Date;
            local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
            local.Infrastructure.EventDetailName = "KDL Valid License";
            local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
            local.Infrastructure.DenormNumeric12 =
              entities.LegalAction.Identifier;
            local.Infrastructure.DenormText12 =
              entities.LegalAction.CourtCaseNumber;
            local.Detail.Text7 = " Old #";
            local.Infrastructure.Detail =
              "KDMV DL updated, Driver's License # " + TrimEnd
              (import.KdmvFile.DriverLicenseNumber) + local.Detail.Text7 + (
                local.Previous.KsDvrLicense ?? "");

            if (entities.LegalAction.Populated)
            {
              if (ReadCaseCaseRoleCaseUnit())
              {
                local.Infrastructure.CaseNumber = entities.Case1.Number;
                local.Infrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
              }
              else
              {
                ExitState = "CASE_NF";
                ++export.OuputTotalErrorRecords.Count;

                return;
              }
            }
            else
            {
              local.Infrastructure.CaseNumber = "";
            }

            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ++export.OuputTotalErrorRecords.Count;

              return;
            }
          }
        }
      }

      if (IsEmpty(local.Previous.KsDvrLicense))
      {
        // this should be only the first time we have heard anything from kdor 
        // about driver's license infor for a AP
        ++export.TotalNumberOfRecords.Count;

        try
        {
          CreateKsDriversLicense();
          ++export.KdmvDriverLicenseUpdat.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "KS_DRIVERS_LICENSE_AE";
              ++export.OuputTotalErrorRecords.Count;

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "KS_DRIVERS_LICENSE_PV";
              ++export.OuputTotalErrorRecords.Count;

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.ReasonCode = "KDLVALIDSTATUS";
        local.Infrastructure.EventId = 1;
        local.Infrastructure.EventType = "ADMINACT";
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.UserId = "KDMV";
        local.Infrastructure.BusinessObjectCd = "ENF";
        local.Infrastructure.ReferenceDate = local.StartDate.Date;
        local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
        local.Infrastructure.EventDetailName = "KDL Valid License";
        local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
        local.Infrastructure.DenormNumeric12 = entities.LegalAction.Identifier;
        local.Infrastructure.DenormText12 =
          entities.LegalAction.CourtCaseNumber;
        local.Detail.Text7 = " Old #";
        local.Infrastructure.Detail = "KDMV DL added, Driver's License # " + TrimEnd
          (import.KdmvFile.DriverLicenseNumber);
        local.Infrastructure.CaseNumber = "";
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++export.OuputTotalErrorRecords.Count;
        }
      }

      // CDVALUE  	DESCRIPTION
      // ---+------	---+---------+---------+---------+---------+---------+
      // ---------+---
      // ADMIN    	ADMINISTRATIVE/EXEMPTIONS
      // ADMINACT 	ADMINISTRATIVE ACTION
      // ADMINAPL 	ADMINSTRATIVE APPEAL
      // APDTL    	ABSENT PARENT DETAILS
      // APSTMT   	AP STATEMENT
      // ARCHIVE  	ARCHIVE/RETRIEVAL (DOCUMENTS)
      // BKRP     	BANKRUPTCY ACTIVITIES
      // CASE     	CASE (AE ACTIVITIES, OPENINGS/CLOSINGS, APPOINTMENTS)
      // CASEROLE 	CASE ROLE (FAMILY VIOLENCE, ROLE CHANGES, GOOD CAUSE, SSN, 
      // PAT)
      // CASEUNIT 	CASE UNIT (LIFECYCLE, OBLIGATIONS PAID/ACTIVATED/REACTIVATED)
      // COMPLIANCE	OBLIGATIONS/FINANCE ACTIVITIES, PARENTAL RIGHTS/EMANCIPATION
      // CSENET   	CSENET, QUICK LOCATE
      // DATEMON  	DATE MONITOR (FUTURE DATES - EMANCIPATION/DISCHARGE/RELEASE)
      // DOCUMENT 	DOCUMENT (GENTEST, LOCATE, INSURANCE, MODIFICATION, JE)
      // EXTERNAL 	EXTERNAL (MANUALLY CREATED)
      // GENTEST   	GENETIC TEST ACTIVITIES
      // HEALTHINS 	HEALTH INSURANCE
      // LEACTION  	LEGAL ACTIONS, NOTICE OF HEARINGS
      // LEREFRL   	LEGAL REFERRALS
      // LOC       	LOCATE EVENTS (DHR, FPLS, 1099, ADDRESSES)
      // MODFN     	SUPPORT MODIFICATION REVIEW
      // PAT       	PERSON PATERNITY TYPE EVENT
      // PERSDTL   	CSE PERSON DETAIL TYPE EVENT Z
      // SERV      	SERVICE REQUESTED, COMPLETED, ANSWERS
    }
    else
    {
      ExitState = "CSE_PERSON_NF";
      ++export.OuputTotalErrorRecords.Count;
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSiCreateCsePersonLicense()
  {
    var useImport = new SiCreateCsePersonLicense.Import();
    var useExport = new SiCreateCsePersonLicense.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.CsePersonLicense.Assign(local.CsePersonLicense);

    Call(SiCreateCsePersonLicense.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePersonLicense()
  {
    var useImport = new SiUpdateCsePersonLicense.Import();
    var useExport = new SiUpdateCsePersonLicense.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.CsePersonLicense.Assign(local.CsePersonLicense);

    Call(SiUpdateCsePersonLicense.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void CreateKsDriversLicense()
  {
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var cspNum = entities.CsePerson.Number;
    var ksDvrLicense = local.KsDriversLicense.KsDvrLicense ?? "";
    var validationDate = import.ProgramProcessingInfo.ProcessDate;
    var sequenceCounter = export.TotalNumberOfRecords.Count;

    entities.KsDriversLicense.Populated = false;
    Update("CreateKsDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "cspNum", cspNum);
        db.SetNullableString(command, "ksDvrLicense", ksDvrLicense);
        db.SetNullableDate(command, "validationDate", validationDate);
        db.SetNullableDate(command, "courtesyLtrDate", default(DateTime));
        db.SetNullableString(command, "servCompleteInd", "");
        db.SetNullableString(command, "restrictionStatus", "");
        db.SetNullableDecimal(command, "amountOwed", 0M);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", createdTstamp);
        db.SetNullableString(command, "note1", "");
        db.SetInt32(command, "sequenceCounter", sequenceCounter);
      });

    entities.KsDriversLicense.CreatedBy = createdBy;
    entities.KsDriversLicense.CreatedTstamp = createdTstamp;
    entities.KsDriversLicense.CspNum = cspNum;
    entities.KsDriversLicense.LgaIdentifier = null;
    entities.KsDriversLicense.KsDvrLicense = ksDvrLicense;
    entities.KsDriversLicense.ValidationDate = validationDate;
    entities.KsDriversLicense.LastUpdatedBy = createdBy;
    entities.KsDriversLicense.LastUpdatedTstamp = createdTstamp;
    entities.KsDriversLicense.SequenceCounter = sequenceCounter;
    entities.KsDriversLicense.Populated = true;
  }

  private bool ReadCaseCaseRoleCaseUnit()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseCaseRoleCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 6);
        entities.CaseUnit.CasNo = db.GetString(reader, 7);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 8);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CaseUnit.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.KdmvFile.CsePersonNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return Read("ReadCsePersonLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonLicense.Identifier = db.GetInt32(reader, 0);
        entities.CsePersonLicense.CspNumber = db.GetString(reader, 1);
        entities.CsePersonLicense.IssuingState =
          db.GetNullableString(reader, 2);
        entities.CsePersonLicense.IssuingAgencyName =
          db.GetNullableString(reader, 3);
        entities.CsePersonLicense.Number = db.GetNullableString(reader, 4);
        entities.CsePersonLicense.ExpirationDt = db.GetNullableDate(reader, 5);
        entities.CsePersonLicense.StartDt = db.GetNullableDate(reader, 6);
        entities.CsePersonLicense.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonLicense.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CsePersonLicense.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.CsePersonLicense.CreatedTimestamp = db.GetDateTime(reader, 10);
        entities.CsePersonLicense.CreatedBy = db.GetString(reader, 11);
        entities.CsePersonLicense.Description =
          db.GetNullableString(reader, 12);
        entities.CsePersonLicense.Note = db.GetNullableString(reader, 13);
        entities.CsePersonLicense.Populated = true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense()
  {
    entities.KsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNum", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedBy = db.GetString(reader, 0);
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 1);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 2);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 4);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 8);
        entities.KsDriversLicense.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.KsDriversLicense.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private void UpdateKsDriversLicense()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);

    var ksDvrLicense = local.KsDriversLicense.KsDvrLicense ?? "";
    var validationDate = import.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense",
      (db, command) =>
      {
        db.SetNullableString(command, "ksDvrLicense", ksDvrLicense);
        db.SetNullableDate(command, "validationDate", validationDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.KsDriversLicense.CspNum);
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      });

    entities.KsDriversLicense.KsDvrLicense = ksDvrLicense;
    entities.KsDriversLicense.ValidationDate = validationDate;
    entities.KsDriversLicense.LastUpdatedBy = lastUpdatedBy;
    entities.KsDriversLicense.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.KsDriversLicense.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of KdmvFile.
    /// </summary>
    [JsonPropertyName("kdmvFile")]
    public KdmvFile KdmvFile
    {
      get => kdmvFile ??= new();
      set => kdmvFile = value;
    }

    /// <summary>
    /// A value of KdmvDriverLicenseUpdat.
    /// </summary>
    [JsonPropertyName("kdmvDriverLicenseUpdat")]
    public Common KdmvDriverLicenseUpdat
    {
      get => kdmvDriverLicenseUpdat ??= new();
      set => kdmvDriverLicenseUpdat = value;
    }

    /// <summary>
    /// A value of TotalErrorRecords.
    /// </summary>
    [JsonPropertyName("totalErrorRecords")]
    public Common TotalErrorRecords
    {
      get => totalErrorRecords ??= new();
      set => totalErrorRecords = value;
    }

    /// <summary>
    /// A value of CsePersonLicenseUpdate.
    /// </summary>
    [JsonPropertyName("csePersonLicenseUpdate")]
    public Common CsePersonLicenseUpdate
    {
      get => csePersonLicenseUpdate ??= new();
      set => csePersonLicenseUpdate = value;
    }

    /// <summary>
    /// A value of TotalNumberOfRecords.
    /// </summary>
    [JsonPropertyName("totalNumberOfRecords")]
    public Common TotalNumberOfRecords
    {
      get => totalNumberOfRecords ??= new();
      set => totalNumberOfRecords = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private KdmvFile kdmvFile;
    private Common kdmvDriverLicenseUpdat;
    private Common totalErrorRecords;
    private Common csePersonLicenseUpdate;
    private Common totalNumberOfRecords;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonLicenseUpdate.
    /// </summary>
    [JsonPropertyName("csePersonLicenseUpdate")]
    public Common CsePersonLicenseUpdate
    {
      get => csePersonLicenseUpdate ??= new();
      set => csePersonLicenseUpdate = value;
    }

    /// <summary>
    /// A value of OuputTotalErrorRecords.
    /// </summary>
    [JsonPropertyName("ouputTotalErrorRecords")]
    public Common OuputTotalErrorRecords
    {
      get => ouputTotalErrorRecords ??= new();
      set => ouputTotalErrorRecords = value;
    }

    /// <summary>
    /// A value of KdmvDriverLicenseUpdat.
    /// </summary>
    [JsonPropertyName("kdmvDriverLicenseUpdat")]
    public Common KdmvDriverLicenseUpdat
    {
      get => kdmvDriverLicenseUpdat ??= new();
      set => kdmvDriverLicenseUpdat = value;
    }

    /// <summary>
    /// A value of TotalNumberOfRecords.
    /// </summary>
    [JsonPropertyName("totalNumberOfRecords")]
    public Common TotalNumberOfRecords
    {
      get => totalNumberOfRecords ??= new();
      set => totalNumberOfRecords = value;
    }

    private Common csePersonLicenseUpdate;
    private Common ouputTotalErrorRecords;
    private Common kdmvDriverLicenseUpdat;
    private Common totalNumberOfRecords;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ProcessedCrtOrderGroup group.</summary>
    [Serializable]
    public class ProcessedCrtOrderGroup
    {
      /// <summary>
      /// A value of ProcessedCtOrder.
      /// </summary>
      [JsonPropertyName("processedCtOrder")]
      public LegalAction ProcessedCtOrder
      {
        get => processedCtOrder ??= new();
        set => processedCtOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private LegalAction processedCtOrder;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public KsDriversLicense Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of DriversLicenseChanged.
    /// </summary>
    [JsonPropertyName("driversLicenseChanged")]
    public Common DriversLicenseChanged
    {
      get => driversLicenseChanged ??= new();
      set => driversLicenseChanged = value;
    }

    /// <summary>
    /// A value of PreviousCtOrderFound.
    /// </summary>
    [JsonPropertyName("previousCtOrderFound")]
    public Common PreviousCtOrderFound
    {
      get => previousCtOrderFound ??= new();
      set => previousCtOrderFound = value;
    }

    /// <summary>
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public LegalAction Check
    {
      get => check ??= new();
      set => check = value;
    }

    /// <summary>
    /// Gets a value of ProcessedCrtOrder.
    /// </summary>
    [JsonIgnore]
    public Array<ProcessedCrtOrderGroup> ProcessedCrtOrder =>
      processedCrtOrder ??= new(ProcessedCrtOrderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ProcessedCrtOrder for json serialization.
    /// </summary>
    [JsonPropertyName("processedCrtOrder")]
    [Computed]
    public IList<ProcessedCrtOrderGroup> ProcessedCrtOrder_Json
    {
      get => processedCrtOrder;
      set => ProcessedCrtOrder.Assign(value);
    }

    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
    }

    /// <summary>
    /// A value of KdmvRecordFound.
    /// </summary>
    [JsonPropertyName("kdmvRecordFound")]
    public Common KdmvRecordFound
    {
      get => kdmvRecordFound ??= new();
      set => kdmvRecordFound = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
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
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
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
    /// A value of Detail.
    /// </summary>
    [JsonPropertyName("detail")]
    public WorkArea Detail
    {
      get => detail ??= new();
      set => detail = value;
    }

    /// <summary>
    /// A value of FinanceWorkAttributes.
    /// </summary>
    [JsonPropertyName("financeWorkAttributes")]
    public FinanceWorkAttributes FinanceWorkAttributes
    {
      get => financeWorkAttributes ??= new();
      set => financeWorkAttributes = value;
    }

    private KsDriversLicense previous;
    private Common driversLicenseChanged;
    private Common previousCtOrderFound;
    private LegalAction check;
    private Array<ProcessedCrtOrderGroup> processedCrtOrder;
    private KsDriversLicense ksDriversLicense;
    private Common kdmvRecordFound;
    private CsePersonLicense csePersonLicense;
    private Infrastructure infrastructure;
    private DateWorkArea startDate;
    private ProgramProcessingInfo programProcessingInfo;
    private WorkArea detail;
    private FinanceWorkAttributes financeWorkAttributes;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
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
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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

    private KsDriversLicense ksDriversLicense;
    private CsePerson csePerson;
    private CsePersonLicense csePersonLicense;
    private LegalAction legalAction;
    private Case1 case1;
    private CaseRole caseRole;
    private CaseUnit caseUnit;
    private LegalActionCaseRole legalActionCaseRole;
  }
#endregion
}
