// Program: CAB_PROCESS_DATE_OF_DEATH, ID: 373344006, model: 746.
// Short name: SWE01969
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_PROCESS_DATE_OF_DEATH.
/// </summary>
[Serializable]
public partial class CabProcessDateOfDeath: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_PROCESS_DATE_OF_DEATH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabProcessDateOfDeath(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabProcessDateOfDeath.
  /// </summary>
  public CabProcessDateOfDeath(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************
    // Maintenance Log:
    //    Date     Request  Name   Description
    // ---------   -------  ----   
    // -------------------------------------------------------
    // ??/??/????   new     ?????   Original.
    // 03/26/2004  PR200614 GVandy Correct extraction of FCR submitted case id.
    // ***********************************************************************************
    // 3/13/2019  CQ65304  JHarden  End date CP address when date of death is 
    // entered.
    export.DodAlertsCreated.Count = import.DodAlertsCreated.Count;
    export.DodEventsCreated.Count = import.DodEventsCreated.Count;
    export.PersonsUpdated.Count = import.PersonsUpdated.Count;
    local.Infrastructure.ProcessStatus = "Q";

    if (ReadCsePerson())
    {
      if (Lt(local.Null1.Date, entities.CsePerson.DateOfDeath))
      {
        if (Year(entities.CsePerson.DateOfDeath) == Year
          (import.FcrPersonAckErrorRecord.DateOfDeath) && Month
          (entities.CsePerson.DateOfDeath) == Month
          (import.FcrPersonAckErrorRecord.DateOfDeath))
        {
          // ********  Same DOD, Event 10, Event Detail 295, No Alert 
          // ************
          local.Infrastructure.Detail = import.ConvertDateOfDeath.Text8;
          local.Infrastructure.CaseNumber =
            Substring(import.FcrPersonAckErrorRecord.CaseId, 1, 10);
          local.Infrastructure.CsePersonNumber = import.CsePerson.Number;

          if (Equal(import.ExternalFplsResponse.AgencyCode, "A03"))
          {
            local.Infrastructure.ReasonCode = "NSADODCONFIRMS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "SSADODCONFIRMS";
          }

          UseOeB412CreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
        else
        {
          // *****  Different DOD, Event 10, Event Detail 296, Alert 457 ******
          local.ExistingConvertDateDateWorkArea.Date =
            entities.CsePerson.DateOfDeath;
          UseCabConvertDate2String();
          local.Infrastructure.CaseNumber =
            Substring(import.FcrPersonAckErrorRecord.CaseId, 1, 10);
          local.Infrastructure.CsePersonNumber = import.CsePerson.Number;

          if (Equal(import.ExternalFplsResponse.AgencyCode, "A03"))
          {
            local.Infrastructure.ReasonCode = "NSADODCONFLICT";
            local.Infrastructure.Detail = "NSA Reports DOD " + import
              .ConvertDateOfDeath.Text8;
          }
          else
          {
            local.Infrastructure.ReasonCode = "SSADODCONFLICT";
            local.Infrastructure.Detail = "SSA Reports DOD " + import
              .ConvertDateOfDeath.Text8;
          }

          local.Infrastructure.ProcessStatus = "Q";
          UseOeB412CreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (export.DodEventsCreated.Count > import.DodEventsCreated.Count)
          {
            ++export.DodAlertsCreated.Count;
          }
        }
      }
      else
      {
        // ********  Update Person with Date of Death  ************
        try
        {
          UpdateCsePerson();
          ++export.PersonsUpdated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CSE_PERSON_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CSE_PERSON_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.CsePersonNumber = import.CsePerson.Number;
        local.Infrastructure.CaseNumber =
          Substring(import.FcrPersonAckErrorRecord.CaseId, 1, 10);
        local.Infrastructure.Detail = import.ConvertDateOfDeath.Text8;

        foreach(var item in ReadCaseUnit2())
        {
          // **************************************************************
          // If person is an AP,  create infrastructure for each case unit
          // because this is a lifecycle transforming event.
          // **************************************************************
          // ***** Add DOD for AP, Event 10, Event Detail 292, Alert 451 ******
          if (Equal(import.ExternalFplsResponse.AgencyCode, "A03"))
          {
            local.Infrastructure.ReasonCode = "NSADEATHDATE";
          }
          else
          {
            local.Infrastructure.ReasonCode = "SSADEATHDATE";
          }

          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
          UseOeB412CreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          ++export.DodAlertsCreated.Count;
        }

        if (ReadCaseUnit1())
        {
          // **************************************************************
          // If person is not an AP, create infrastructure once for the case.
          // **************************************************************
          // ***** Add DOD for AR/CH, Event 10, Event Detail 297, Alert 451 
          // ******
          if (Equal(import.ExternalFplsResponse.AgencyCode, "A03"))
          {
            local.Infrastructure.ReasonCode = "NSADODARCHILD";
          }
          else
          {
            local.Infrastructure.ReasonCode = "SSADODARCHILD";
          }

          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
          UseOeB412CreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          ++export.DodAlertsCreated.Count;
        }

        // CQ65304
        foreach(var item in ReadCsePersonAddress())
        {
          try
          {
            UpdateCsePersonAddress();
            export.PersonsUpdated.Count = export.AddressUpdated.Count + 1;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CSE_PERSON_ADDRESS_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CSE_PERSON_ADDRESS_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Person not found: " + import
        .CsePerson.Number + " date reported as: " + import
        .ConvertDateOfDeath.Text8;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // * Name and Last Known Residence Event 10, Event Detail 293, No Alert *
    if (Equal(import.FcrPersonAckErrorRecord.FirstName,
      import.FcrPersonAckErrorRecord.FcrPrimaryFirstName) && Equal
      (import.FcrPersonAckErrorRecord.MiddleName,
      import.FcrPersonAckErrorRecord.FcrPrimaryMiddleName) && Equal
      (import.FcrPersonAckErrorRecord.LastName,
      import.FcrPersonAckErrorRecord.FcrPrimaryLastName) || IsEmpty
      (import.FcrPersonAckErrorRecord.FcrPrimaryLastName))
    {
      if (IsEmpty(import.FcrPersonAckErrorRecord.SsaZipCodeOfLastResidence) && IsEmpty
        (import.SsaCityLastResidence.Text15) && IsEmpty
        (import.SsaStateLastResidence.Text2))
      {
        return;
      }
      else
      {
        // *** Last Known Residence Only ***
        local.Infrastructure.Detail =
          TrimEnd(import.SsaCityLastResidence.Text15) + " " + TrimEnd
          (import.SsaStateLastResidence.Text2) + " " + TrimEnd
          (import.FcrPersonAckErrorRecord.SsaZipCodeOfLastResidence);
      }
    }
    else if (IsEmpty(import.FcrPersonAckErrorRecord.SsaZipCodeOfLastResidence) &&
      IsEmpty(import.SsaCityLastResidence.Text15) && IsEmpty
      (import.SsaStateLastResidence.Text2))
    {
      // *** Name Only ***
      local.Infrastructure.Detail =
        TrimEnd(import.FcrPersonAckErrorRecord.FcrPrimaryLastName) + ", " + TrimEnd
        (import.FcrPersonAckErrorRecord.FcrPrimaryFirstName) + " " + TrimEnd
        (import.FcrPersonAckErrorRecord.FcrPrimaryMiddleName);

      if (Equal(import.ExternalFplsResponse.AgencyCode, "A03") && !
        IsEmpty(import.ExternalFplsResponse.ApNameReturned))
      {
        local.Infrastructure.Detail =
          import.ExternalFplsResponse.ApNameReturned;
      }
    }
    else
    {
      // *** Name and Last Known Residence ***
      local.Name.Text33 =
        TrimEnd(import.FcrPersonAckErrorRecord.FcrPrimaryLastName) + ", " + TrimEnd
        (import.FcrPersonAckErrorRecord.FcrPrimaryFirstName) + " " + TrimEnd
        (import.FcrPersonAckErrorRecord.FcrPrimaryMiddleName);
      local.Address.Text33 = TrimEnd(import.SsaCityLastResidence.Text15) + " " +
        TrimEnd(import.SsaStateLastResidence.Text2) + " " + TrimEnd
        (import.FcrPersonAckErrorRecord.SsaZipCodeOfLastResidence);
      local.Infrastructure.Detail = TrimEnd(local.Name.Text33) + "/" + local
        .Address.Text33;

      if (Equal(import.ExternalFplsResponse.AgencyCode, "A03") && !
        IsEmpty(import.ExternalFplsResponse.ApNameReturned))
      {
        local.Infrastructure.Detail = TrimEnd(local.Address.Text33) + "/" + import
          .ExternalFplsResponse.ApNameReturned;
      }
    }

    // * Name and Last Known Residence Event 10, Event Detail 293, No Alert *
    if (Equal(import.ExternalFplsResponse.AgencyCode, "A03"))
    {
      local.Infrastructure.ReasonCode = "NSADODNAMEADDR";
    }
    else
    {
      local.Infrastructure.ReasonCode = "SSADODNAMEADDR";
    }

    UseOeB412CreateInfrastructure();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.ExistingConvertDateDateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.ExistingConvertDateTextWorkArea.Text8 = useExport.TextWorkArea.Text8;
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

  private void UseOeB412CreateInfrastructure()
  {
    var useImport = new OeB412CreateInfrastructure.Import();
    var useExport = new OeB412CreateInfrastructure.Export();

    useImport.ItemsCreated.Count = export.DodEventsCreated.Count;
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(OeB412CreateInfrastructure.Execute, useImport, useExport);

    export.DodEventsCreated.Count = useExport.ItemsCreated.Count;
  }

  private bool ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAr", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "closureDate", import.Max.Date.GetValueOrDefault());
        db.SetString(command, "casNo", local.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 1);
        entities.CaseUnit.CasNo = db.GetString(reader, 2);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 5);
        entities.CaseUnit.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "closureDate", import.Max.Date.GetValueOrDefault());
        db.SetString(command, "casNo", local.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 1);
        entities.CaseUnit.CasNo = db.GetString(reader, 2);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 5);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(command, "endDate", date);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 9);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 14);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
      });
  }

  private void UpdateCsePerson()
  {
    var dateOfDeath = import.FcrPersonAckErrorRecord.DateOfDeath;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(command, "dateOfDeath", dateOfDeath);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.DateOfDeath = dateOfDeath;
    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.Populated = true;
  }

  private void UpdateCsePersonAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAddress.Populated);

    var endDate = Now().Date;
    var endCode = "DC";

    entities.CsePersonAddress.Populated = false;
    Update("UpdateCsePersonAddress",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "endCode", endCode);
        db.SetDateTime(
          command, "identifier",
          entities.CsePersonAddress.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePersonAddress.CspNumber);
      });

    entities.CsePersonAddress.EndDate = endDate;
    entities.CsePersonAddress.EndCode = endCode;
    entities.CsePersonAddress.Populated = true;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of PersonsUpdated.
    /// </summary>
    [JsonPropertyName("personsUpdated")]
    public Common PersonsUpdated
    {
      get => personsUpdated ??= new();
      set => personsUpdated = value;
    }

    /// <summary>
    /// A value of DodEventsCreated.
    /// </summary>
    [JsonPropertyName("dodEventsCreated")]
    public Common DodEventsCreated
    {
      get => dodEventsCreated ??= new();
      set => dodEventsCreated = value;
    }

    /// <summary>
    /// A value of DodAlertsCreated.
    /// </summary>
    [JsonPropertyName("dodAlertsCreated")]
    public Common DodAlertsCreated
    {
      get => dodAlertsCreated ??= new();
      set => dodAlertsCreated = value;
    }

    /// <summary>
    /// A value of ConvertDateOfDeath.
    /// </summary>
    [JsonPropertyName("convertDateOfDeath")]
    public TextWorkArea ConvertDateOfDeath
    {
      get => convertDateOfDeath ??= new();
      set => convertDateOfDeath = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of FcrPersonAckErrorRecord.
    /// </summary>
    [JsonPropertyName("fcrPersonAckErrorRecord")]
    public FcrPersonAckErrorRecord FcrPersonAckErrorRecord
    {
      get => fcrPersonAckErrorRecord ??= new();
      set => fcrPersonAckErrorRecord = value;
    }

    /// <summary>
    /// A value of SsaCityLastResidence.
    /// </summary>
    [JsonPropertyName("ssaCityLastResidence")]
    public WorkArea SsaCityLastResidence
    {
      get => ssaCityLastResidence ??= new();
      set => ssaCityLastResidence = value;
    }

    /// <summary>
    /// A value of SsaStateLastResidence.
    /// </summary>
    [JsonPropertyName("ssaStateLastResidence")]
    public WorkArea SsaStateLastResidence
    {
      get => ssaStateLastResidence ??= new();
      set => ssaStateLastResidence = value;
    }

    /// <summary>
    /// A value of ExternalFplsResponse.
    /// </summary>
    [JsonPropertyName("externalFplsResponse")]
    public ExternalFplsResponse ExternalFplsResponse
    {
      get => externalFplsResponse ??= new();
      set => externalFplsResponse = value;
    }

    private DateWorkArea max;
    private Common personsUpdated;
    private Common dodEventsCreated;
    private Common dodAlertsCreated;
    private TextWorkArea convertDateOfDeath;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson csePerson;
    private FcrPersonAckErrorRecord fcrPersonAckErrorRecord;
    private WorkArea ssaCityLastResidence;
    private WorkArea ssaStateLastResidence;
    private ExternalFplsResponse externalFplsResponse;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AddressUpdated.
    /// </summary>
    [JsonPropertyName("addressUpdated")]
    public Common AddressUpdated
    {
      get => addressUpdated ??= new();
      set => addressUpdated = value;
    }

    /// <summary>
    /// A value of PersonsUpdated.
    /// </summary>
    [JsonPropertyName("personsUpdated")]
    public Common PersonsUpdated
    {
      get => personsUpdated ??= new();
      set => personsUpdated = value;
    }

    /// <summary>
    /// A value of DodEventsCreated.
    /// </summary>
    [JsonPropertyName("dodEventsCreated")]
    public Common DodEventsCreated
    {
      get => dodEventsCreated ??= new();
      set => dodEventsCreated = value;
    }

    /// <summary>
    /// A value of DodAlertsCreated.
    /// </summary>
    [JsonPropertyName("dodAlertsCreated")]
    public Common DodAlertsCreated
    {
      get => dodAlertsCreated ??= new();
      set => dodAlertsCreated = value;
    }

    private Common addressUpdated;
    private Common personsUpdated;
    private Common dodEventsCreated;
    private Common dodAlertsCreated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public CsePersonAddress Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Address.
    /// </summary>
    [JsonPropertyName("address")]
    public WorkArea Address
    {
      get => address ??= new();
      set => address = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public WorkArea Name
    {
      get => name ??= new();
      set => name = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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

    /// <summary>
    /// A value of ExistingConvertDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("existingConvertDateDateWorkArea")]
    public DateWorkArea ExistingConvertDateDateWorkArea
    {
      get => existingConvertDateDateWorkArea ??= new();
      set => existingConvertDateDateWorkArea = value;
    }

    /// <summary>
    /// A value of ExistingConvertDateTextWorkArea.
    /// </summary>
    [JsonPropertyName("existingConvertDateTextWorkArea")]
    public TextWorkArea ExistingConvertDateTextWorkArea
    {
      get => existingConvertDateTextWorkArea ??= new();
      set => existingConvertDateTextWorkArea = value;
    }

    private CsePerson csePerson;
    private CsePersonAddress update;
    private WorkArea address;
    private WorkArea name;
    private DateWorkArea null1;
    private Infrastructure infrastructure;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea existingConvertDateDateWorkArea;
    private TextWorkArea existingConvertDateTextWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonAddress csePersonAddress;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CsePerson csePerson;
  }
#endregion
}
