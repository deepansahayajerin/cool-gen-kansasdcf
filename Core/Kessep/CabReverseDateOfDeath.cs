// Program: CAB_REVERSE_DATE_OF_DEATH, ID: 373344772, model: 746.
// Short name: SWE01970
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_REVERSE_DATE_OF_DEATH.
/// </summary>
[Serializable]
public partial class CabReverseDateOfDeath: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_REVERSE_DATE_OF_DEATH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabReverseDateOfDeath(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabReverseDateOfDeath.
  /// </summary>
  public CabReverseDateOfDeath(IContext context, Import import, Export export):
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
    export.DodAlertsCreated.Count = import.DodAlertsCreated.Count;
    export.DodEventsCreated.Count = import.DodEventsCreated.Count;
    export.PersonsUpdated.Count = import.PersonsUpdated.Count;
    local.Infrastructure.ProcessStatus = "Q";

    if (ReadCsePerson())
    {
      if (!Equal(entities.CsePerson.DateOfDeath, local.Null1.Date))
      {
        // ********  Update Person removing Date of Death  ************
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
      }

      // *** DOD Deleted, Event 10, Event Detail 294, Alert 452 ***
      local.Infrastructure.Detail = "99999999 Reported by SSA";
      local.Infrastructure.CaseNumber =
        Substring(import.FcrPersonAckErrorRecord.CaseId, 1, 10);
      local.Infrastructure.CsePersonNumber = import.CsePerson.Number;
      local.Infrastructure.ReasonCode = "SSADODDELETE";

      foreach(var item in ReadCaseUnit2())
      {
        // **************************************************************
        // If person is an AP,  create infrastructure for each case unit
        // because this is a lifecycle transforming event.
        // **************************************************************
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
        // If person is not an AP,  create infrastructure only once.
        // **************************************************************
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        UseOeB412CreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.DodAlertsCreated.Count;
      }
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Person not found: " + import
        .CsePerson.Number + " date reported as: " + "99999999";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
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

    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.ItemsCreated.Count = export.DodEventsCreated.Count;

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
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 20);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 21);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 22);
        entities.CsePerson.CreatedBy = db.GetString(reader, 23);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 24);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 25);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 26);
        entities.CsePerson.KscaresNumber = db.GetNullableString(reader, 27);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 28);
        entities.CsePerson.EmergencyAreaCode = db.GetNullableInt32(reader, 29);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 30);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 31);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 32);
        entities.CsePerson.WorkPhoneExt = db.GetNullableString(reader, 33);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 34);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 35);
        entities.CsePerson.FederalInd = db.GetNullableString(reader, 36);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 37);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 38);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 39);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 40);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 41);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 42);
        entities.CsePerson.BirthCertFathersLastName =
          db.GetNullableString(reader, 43);
        entities.CsePerson.BirthCertFathersFirstName =
          db.GetNullableString(reader, 44);
        entities.CsePerson.BirthCertFathersMi =
          db.GetNullableString(reader, 45);
        entities.CsePerson.BirthCertificateSignature =
          db.GetNullableString(reader, 46);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 47);
        entities.CsePerson.Populated = true;
      });
  }

  private void UpdateCsePerson()
  {
    var dateOfDeath = local.Null1.Date;
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
    /// A value of PersonsUpdated.
    /// </summary>
    [JsonPropertyName("personsUpdated")]
    public Common PersonsUpdated
    {
      get => personsUpdated ??= new();
      set => personsUpdated = value;
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
    /// A value of DodAlertsCreated.
    /// </summary>
    [JsonPropertyName("dodAlertsCreated")]
    public Common DodAlertsCreated
    {
      get => dodAlertsCreated ??= new();
      set => dodAlertsCreated = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    private Common personsUpdated;
    private CsePerson csePerson;
    private Common dodAlertsCreated;
    private Common dodEventsCreated;
    private FcrPersonAckErrorRecord fcrPersonAckErrorRecord;
    private WorkArea ssaCityLastResidence;
    private WorkArea ssaStateLastResidence;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of DodEventsCreated.
    /// </summary>
    [JsonPropertyName("dodEventsCreated")]
    public Common DodEventsCreated
    {
      get => dodEventsCreated ??= new();
      set => dodEventsCreated = value;
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

    private Common dodAlertsCreated;
    private Common dodEventsCreated;
    private Common personsUpdated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public WorkArea Name
    {
      get => name ??= new();
      set => name = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private Infrastructure infrastructure;
    private WorkArea name;
    private WorkArea address;
    private DateWorkArea null1;
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

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CaseUnit caseUnit;
    private CsePerson csePerson;
    private Case1 case1;
  }
#endregion
}
