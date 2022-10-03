// Program: OE_B447_SVES_PRISON_RECORD_PRS, ID: 945066132, model: 746.
// Short name: SWE04475
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B447_SVES_PRISON_RECORD_PRS.
/// </para>
/// <para>
/// This SVES action block maintain the prison information received through FCR 
/// response.
/// </para>
/// </summary>
[Serializable]
public partial class OeB447SvesPrisonRecordPrs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B447_SVES_PRISON_RECORD_PRS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB447SvesPrisonRecordPrs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB447SvesPrisonRecordPrs.
  /// </summary>
  public OeB447SvesPrisonRecordPrs(IContext context, Import import,
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
    // ******************************************************************************************
    // * This Action Block received the SVES Prison information from the calling
    // object and     *
    // * process them by adding/upding to CSE database and create required 
    // worker alert,income  *
    // * source & document generation wherever 
    // required.
    // 
    // *
    // ******************************************************************************************
    // ******************************************************************************************
    // *                                  
    // Maintenance Log
    // 
    // *
    // ******************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------   
    // --------------------------------------------*
    // * 06/03/2011  Raj S              CQ5577      Initial Coding.
    // *
    // *
    // 
    // *
    // ******************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Infrastructure.Assign(import.Infrastructure);
    local.Process.Date = import.Infrastructure.ReferenceDate;

    // *******************************************************************************************
    // ** Check wheter received  Prison  record already exists in CSE database 
    // then update      **
    // ** the existing information otherwise create a new Prison response entry 
    // to CSE DB.      **
    // *******************************************************************************************
    if (ReadFcrSvesGenInfo())
    {
      if (ReadFcrSvesPrison())
      {
        try
        {
          UpdateFcrSvesPrison();
          ++import.TotPrisonUpdated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_SVES_PRISON_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_SVES_PRISON_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        try
        {
          CreateFcrSvesPrison();
          ++import.TotPrisonCreated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_SVES_PRISON_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_SVES_PRISON_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      ExitState = "FCR_SVES_GEN_INFO_NF";

      return;
    }

    // ******************************************************************************************
    // * Geenrate alerts, if the person plays a role AP or CH in any of CSE 
    // Cases, the person   *
    // * should be active as well as the case.
    // 
    // *
    // ******************************************************************************************
    UseOeB447SvesAlertNIwoGen();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      import.TotPrisonAlertCreated.Count += local.TotAlertRecsCreated.Count;
      import.TotPrisonHistCreated.Count += local.TotHistRecsCreated.Count;
      import.TotPrisonAlertExists.Count += local.TotAlertExistsRecs.Count;
      import.TotPrisonHistExists.Count += local.TotHistExistsRecs.Count;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }
  }

  private static void MoveFcrSvesGenInfo(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.MemberId = source.MemberId;
    target.LocateSourceResponseAgencyCo = source.LocateSourceResponseAgencyCo;
  }

  private void UseOeB447SvesAlertNIwoGen()
  {
    var useImport = new OeB447SvesAlertNIwoGen.Import();
    var useExport = new OeB447SvesAlertNIwoGen.Export();

    useImport.IwoGenerationSkipFl.Flag = import.IwoGenerationSkipFl.Flag;
    useImport.AlertGenerationSkipFl.Flag = import.AlertGenerationSkipFl.Flag;
    useImport.Max.Date = import.MaxDate.Date;
    MoveFcrSvesGenInfo(import.FcrSvesGenInfo, useImport.FcrSvesGenInfo);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.ProcessingDate.Date = local.Process.Date;

    Call(OeB447SvesAlertNIwoGen.Execute, useImport, useExport);

    local.TotAlertRecsCreated.Count = useExport.TotAlertRecsCreated.Count;
    local.TotHistRecsCreated.Count = useExport.TotHistRecsCreated.Count;
    local.TotAlertExistsRecs.Count = useExport.TotAlertExistsRecs.Count;
    local.TotHistExistsRecs.Count = useExport.TotHistExistsRecs.Count;
  }

  private void CreateFcrSvesPrison()
  {
    var fcgMemberId = entities.ExistingFcrSvesGenInfo.MemberId;
    var fcgLSRspAgy =
      entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo;
    var seqNo = import.FcrSvesPrison.SeqNo;
    var prisonFacilityType = import.FcrSvesPrison.PrisonFacilityType ?? "";
    var prisonFacilityPhone = import.FcrSvesPrison.PrisonFacilityPhone ?? "";
    var prisonFacilityFaxNum = import.FcrSvesPrison.PrisonFacilityFaxNum ?? "";
    var prisonerIdNumber = import.FcrSvesPrison.PrisonerIdNumber ?? "";
    var prisonReportedSsn = import.FcrSvesPrison.PrisonReportedSsn ?? "";
    var prisonReportedSuffix = import.FcrSvesPrison.PrisonReportedSuffix ?? "";
    var confinementDate = import.FcrSvesPrison.ConfinementDate;
    var releaseDate = import.FcrSvesPrison.ReleaseDate;
    var reportDate = import.FcrSvesPrison.ReportDate;
    var createdBy = import.FcrSvesPrison.CreatedBy;
    var createdTimestamp = import.FcrSvesPrison.CreatedTimestamp;
    var lastUpdatedBy = local.Null1.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.Null1.LastUpdatedTimestamp;
    var prisonFacilityName = import.FcrSvesPrison.PrisonFacilityName ?? "";
    var prisonFacilityContactName =
      import.FcrSvesPrison.PrisonFacilityContactName ?? "";
    var prisonerReporterName = import.FcrSvesPrison.PrisonerReporterName ?? "";

    entities.ExistingFcrSvesPrison.Populated = false;
    Update("CreateFcrSvesPrison",
      (db, command) =>
      {
        db.SetString(command, "fcgMemberId", fcgMemberId);
        db.SetString(command, "fcgLSRspAgy", fcgLSRspAgy);
        db.SetInt32(command, "seqNo", seqNo);
        db.SetNullableString(command, "facilityType", prisonFacilityType);
        db.SetNullableString(command, "facilityPhone", prisonFacilityPhone);
        db.SetNullableString(command, "facilityFaxNum", prisonFacilityFaxNum);
        db.SetNullableString(command, "prisonerIdNumber", prisonerIdNumber);
        db.SetNullableString(command, "prisonRptdSsn", prisonReportedSsn);
        db.SetNullableString(command, "prisonRptdSuffix", prisonReportedSuffix);
        db.SetNullableDate(command, "confinementDate", confinementDate);
        db.SetNullableDate(command, "releaseDate", releaseDate);
        db.SetNullableDate(command, "reportDate", reportDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "facilityName", prisonFacilityName);
        db.SetNullableString(
          command, "facilityContact", prisonFacilityContactName);
        db.SetNullableString(command, "reporterName", prisonerReporterName);
      });

    entities.ExistingFcrSvesPrison.FcgMemberId = fcgMemberId;
    entities.ExistingFcrSvesPrison.FcgLSRspAgy = fcgLSRspAgy;
    entities.ExistingFcrSvesPrison.SeqNo = seqNo;
    entities.ExistingFcrSvesPrison.PrisonFacilityType = prisonFacilityType;
    entities.ExistingFcrSvesPrison.PrisonFacilityPhone = prisonFacilityPhone;
    entities.ExistingFcrSvesPrison.PrisonFacilityFaxNum = prisonFacilityFaxNum;
    entities.ExistingFcrSvesPrison.PrisonerIdNumber = prisonerIdNumber;
    entities.ExistingFcrSvesPrison.PrisonReportedSsn = prisonReportedSsn;
    entities.ExistingFcrSvesPrison.PrisonReportedSuffix = prisonReportedSuffix;
    entities.ExistingFcrSvesPrison.ConfinementDate = confinementDate;
    entities.ExistingFcrSvesPrison.ReleaseDate = releaseDate;
    entities.ExistingFcrSvesPrison.ReportDate = reportDate;
    entities.ExistingFcrSvesPrison.CreatedBy = createdBy;
    entities.ExistingFcrSvesPrison.CreatedTimestamp = createdTimestamp;
    entities.ExistingFcrSvesPrison.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesPrison.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingFcrSvesPrison.PrisonFacilityName = prisonFacilityName;
    entities.ExistingFcrSvesPrison.PrisonFacilityContactName =
      prisonFacilityContactName;
    entities.ExistingFcrSvesPrison.PrisonerReporterName = prisonerReporterName;
    entities.ExistingFcrSvesPrison.Populated = true;
  }

  private bool ReadFcrSvesGenInfo()
  {
    entities.ExistingFcrSvesGenInfo.Populated = false;

    return Read("ReadFcrSvesGenInfo",
      (db, command) =>
      {
        db.SetString(command, "memberId", import.FcrSvesGenInfo.MemberId);
        db.SetString(
          command, "locSrcRspAgyCd",
          import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesGenInfo.MemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo =
          db.GetString(reader, 1);
        entities.ExistingFcrSvesGenInfo.Populated = true;
      });
  }

  private bool ReadFcrSvesPrison()
  {
    entities.ExistingFcrSvesPrison.Populated = false;

    return Read("ReadFcrSvesPrison",
      (db, command) =>
      {
        db.SetInt32(command, "seqNo", import.FcrSvesPrison.SeqNo);
        db.SetString(
          command, "fcgLSRspAgy",
          entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo);
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesGenInfo.MemberId);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesPrison.FcgMemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesPrison.FcgLSRspAgy = db.GetString(reader, 1);
        entities.ExistingFcrSvesPrison.SeqNo = db.GetInt32(reader, 2);
        entities.ExistingFcrSvesPrison.PrisonFacilityType =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesPrison.PrisonFacilityPhone =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesPrison.PrisonFacilityFaxNum =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesPrison.PrisonerIdNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesPrison.PrisonReportedSsn =
          db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesPrison.PrisonReportedSuffix =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesPrison.ConfinementDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingFcrSvesPrison.ReleaseDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingFcrSvesPrison.ReportDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingFcrSvesPrison.CreatedBy = db.GetString(reader, 12);
        entities.ExistingFcrSvesPrison.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.ExistingFcrSvesPrison.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingFcrSvesPrison.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.ExistingFcrSvesPrison.PrisonFacilityName =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrSvesPrison.PrisonFacilityContactName =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrSvesPrison.PrisonerReporterName =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrSvesPrison.Populated = true;
      });
  }

  private void UpdateFcrSvesPrison()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingFcrSvesPrison.Populated);

    var prisonFacilityType = import.FcrSvesPrison.PrisonFacilityType ?? "";
    var prisonFacilityPhone = import.FcrSvesPrison.PrisonFacilityPhone ?? "";
    var prisonFacilityFaxNum = import.FcrSvesPrison.PrisonFacilityFaxNum ?? "";
    var prisonerIdNumber = import.FcrSvesPrison.PrisonerIdNumber ?? "";
    var prisonReportedSsn = import.FcrSvesPrison.PrisonReportedSsn ?? "";
    var prisonReportedSuffix = import.FcrSvesPrison.PrisonReportedSuffix ?? "";
    var confinementDate = import.FcrSvesPrison.ConfinementDate;
    var releaseDate = import.FcrSvesPrison.ReleaseDate;
    var reportDate = import.FcrSvesPrison.ReportDate;
    var lastUpdatedBy = import.FcrSvesPrison.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = import.FcrSvesPrison.LastUpdatedTimestamp;
    var prisonFacilityName = import.FcrSvesPrison.PrisonFacilityName ?? "";
    var prisonFacilityContactName =
      import.FcrSvesPrison.PrisonFacilityContactName ?? "";
    var prisonerReporterName = import.FcrSvesPrison.PrisonerReporterName ?? "";

    entities.ExistingFcrSvesPrison.Populated = false;
    Update("UpdateFcrSvesPrison",
      (db, command) =>
      {
        db.SetNullableString(command, "facilityType", prisonFacilityType);
        db.SetNullableString(command, "facilityPhone", prisonFacilityPhone);
        db.SetNullableString(command, "facilityFaxNum", prisonFacilityFaxNum);
        db.SetNullableString(command, "prisonerIdNumber", prisonerIdNumber);
        db.SetNullableString(command, "prisonRptdSsn", prisonReportedSsn);
        db.SetNullableString(command, "prisonRptdSuffix", prisonReportedSuffix);
        db.SetNullableDate(command, "confinementDate", confinementDate);
        db.SetNullableDate(command, "releaseDate", releaseDate);
        db.SetNullableDate(command, "reportDate", reportDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "facilityName", prisonFacilityName);
        db.SetNullableString(
          command, "facilityContact", prisonFacilityContactName);
        db.SetNullableString(command, "reporterName", prisonerReporterName);
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesPrison.FcgMemberId);
        db.SetString(
          command, "fcgLSRspAgy", entities.ExistingFcrSvesPrison.FcgLSRspAgy);
        db.SetInt32(command, "seqNo", entities.ExistingFcrSvesPrison.SeqNo);
      });

    entities.ExistingFcrSvesPrison.PrisonFacilityType = prisonFacilityType;
    entities.ExistingFcrSvesPrison.PrisonFacilityPhone = prisonFacilityPhone;
    entities.ExistingFcrSvesPrison.PrisonFacilityFaxNum = prisonFacilityFaxNum;
    entities.ExistingFcrSvesPrison.PrisonerIdNumber = prisonerIdNumber;
    entities.ExistingFcrSvesPrison.PrisonReportedSsn = prisonReportedSsn;
    entities.ExistingFcrSvesPrison.PrisonReportedSuffix = prisonReportedSuffix;
    entities.ExistingFcrSvesPrison.ConfinementDate = confinementDate;
    entities.ExistingFcrSvesPrison.ReleaseDate = releaseDate;
    entities.ExistingFcrSvesPrison.ReportDate = reportDate;
    entities.ExistingFcrSvesPrison.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesPrison.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingFcrSvesPrison.PrisonFacilityName = prisonFacilityName;
    entities.ExistingFcrSvesPrison.PrisonFacilityContactName =
      prisonFacilityContactName;
    entities.ExistingFcrSvesPrison.PrisonerReporterName = prisonerReporterName;
    entities.ExistingFcrSvesPrison.Populated = true;
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
    /// A value of AlertGenerationSkipFl.
    /// </summary>
    [JsonPropertyName("alertGenerationSkipFl")]
    public Common AlertGenerationSkipFl
    {
      get => alertGenerationSkipFl ??= new();
      set => alertGenerationSkipFl = value;
    }

    /// <summary>
    /// A value of IwoGenerationSkipFl.
    /// </summary>
    [JsonPropertyName("iwoGenerationSkipFl")]
    public Common IwoGenerationSkipFl
    {
      get => iwoGenerationSkipFl ??= new();
      set => iwoGenerationSkipFl = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of FcrSvesPrison.
    /// </summary>
    [JsonPropertyName("fcrSvesPrison")]
    public FcrSvesPrison FcrSvesPrison
    {
      get => fcrSvesPrison ??= new();
      set => fcrSvesPrison = value;
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
    /// A value of TotPrisonCreated.
    /// </summary>
    [JsonPropertyName("totPrisonCreated")]
    public Common TotPrisonCreated
    {
      get => totPrisonCreated ??= new();
      set => totPrisonCreated = value;
    }

    /// <summary>
    /// A value of TotPrisonUpdated.
    /// </summary>
    [JsonPropertyName("totPrisonUpdated")]
    public Common TotPrisonUpdated
    {
      get => totPrisonUpdated ??= new();
      set => totPrisonUpdated = value;
    }

    /// <summary>
    /// A value of TotPrisonAlertCreated.
    /// </summary>
    [JsonPropertyName("totPrisonAlertCreated")]
    public Common TotPrisonAlertCreated
    {
      get => totPrisonAlertCreated ??= new();
      set => totPrisonAlertCreated = value;
    }

    /// <summary>
    /// A value of TotPrisonAlertExists.
    /// </summary>
    [JsonPropertyName("totPrisonAlertExists")]
    public Common TotPrisonAlertExists
    {
      get => totPrisonAlertExists ??= new();
      set => totPrisonAlertExists = value;
    }

    /// <summary>
    /// A value of TotPrisonHistCreated.
    /// </summary>
    [JsonPropertyName("totPrisonHistCreated")]
    public Common TotPrisonHistCreated
    {
      get => totPrisonHistCreated ??= new();
      set => totPrisonHistCreated = value;
    }

    /// <summary>
    /// A value of TotPrisonHistExists.
    /// </summary>
    [JsonPropertyName("totPrisonHistExists")]
    public Common TotPrisonHistExists
    {
      get => totPrisonHistExists ??= new();
      set => totPrisonHistExists = value;
    }

    private Common alertGenerationSkipFl;
    private Common iwoGenerationSkipFl;
    private DateWorkArea maxDate;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesPrison fcrSvesPrison;
    private Infrastructure infrastructure;
    private Common totPrisonCreated;
    private Common totPrisonUpdated;
    private Common totPrisonAlertCreated;
    private Common totPrisonAlertExists;
    private Common totPrisonHistCreated;
    private Common totPrisonHistExists;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public FcrSvesPrison Null1
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of TotHistExistsRecs.
    /// </summary>
    [JsonPropertyName("totHistExistsRecs")]
    public Common TotHistExistsRecs
    {
      get => totHistExistsRecs ??= new();
      set => totHistExistsRecs = value;
    }

    /// <summary>
    /// A value of TotAlertExistsRecs.
    /// </summary>
    [JsonPropertyName("totAlertExistsRecs")]
    public Common TotAlertExistsRecs
    {
      get => totAlertExistsRecs ??= new();
      set => totAlertExistsRecs = value;
    }

    /// <summary>
    /// A value of TotHistRecsCreated.
    /// </summary>
    [JsonPropertyName("totHistRecsCreated")]
    public Common TotHistRecsCreated
    {
      get => totHistRecsCreated ??= new();
      set => totHistRecsCreated = value;
    }

    /// <summary>
    /// A value of TotAlertRecsCreated.
    /// </summary>
    [JsonPropertyName("totAlertRecsCreated")]
    public Common TotAlertRecsCreated
    {
      get => totAlertRecsCreated ??= new();
      set => totAlertRecsCreated = value;
    }

    private FcrSvesPrison null1;
    private Infrastructure infrastructure;
    private DateWorkArea process;
    private Common totHistExistsRecs;
    private Common totAlertExistsRecs;
    private Common totHistRecsCreated;
    private Common totAlertRecsCreated;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("existingFcrSvesGenInfo")]
    public FcrSvesGenInfo ExistingFcrSvesGenInfo
    {
      get => existingFcrSvesGenInfo ??= new();
      set => existingFcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of ExistingFcrSvesPrison.
    /// </summary>
    [JsonPropertyName("existingFcrSvesPrison")]
    public FcrSvesPrison ExistingFcrSvesPrison
    {
      get => existingFcrSvesPrison ??= new();
      set => existingFcrSvesPrison = value;
    }

    private FcrSvesGenInfo existingFcrSvesGenInfo;
    private FcrSvesPrison existingFcrSvesPrison;
  }
#endregion
}
