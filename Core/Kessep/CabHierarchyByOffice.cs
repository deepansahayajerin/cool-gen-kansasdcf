// Program: CAB_HIERARCHY_BY_OFFICE, ID: 372970394, model: 746.
// Short name: SWEFG740
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_HIERARCHY_BY_OFFICE.
/// </summary>
[Serializable]
public partial class CabHierarchyByOffice: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_HIERARCHY_BY_OFFICE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabHierarchyByOffice(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabHierarchyByOffice.
  /// </summary>
  public CabHierarchyByOffice(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **************************************************************************
    // *
    // *                             M A I N T E N A N C E     L O G
    // *
    // **************************************************************************
    // *
    // *     Date Developer  Problem #  Description
    // * -------- ---------  ---------  -----------
    // * 11/08/99  SWSRCHF   H00077482  This is a new CAB to get the Office,
    // *
    // 
    // Supervisor and Collection
    // Officer names
    // **************************************************************************
    export.Extract.Assign(import.Extract);
    export.ErrorFound.Flag = "N";

    // ***
    // *** get each Case Assigmnent/Office Service Provider
    // *** (Collection Officer) for the imported Case number
    // ***
    foreach(var item in ReadCaseAssignmentOfficeServiceProvider())
    {
      // ***
      // *** get Service Provider for current Office Service Provider
      // ***
      if (ReadServiceProvider1())
      {
        // ***
        // *** get Collection Officer last name, first name, middle initial
        // ***
        export.Extract.CollectionOfficer =
          TrimEnd(entities.ServiceProvider.LastName) + ", " + TrimEnd
          (entities.ServiceProvider.FirstName) + " " + entities
          .ServiceProvider.MiddleInitial;

        // ***
        // *** get Section Supervisor last name, first name, middle initial
        // ***
        export.Extract.SectionSupervisor =
          TrimEnd(entities.ServiceProvider.LastName) + ", " + TrimEnd
          (entities.ServiceProvider.FirstName) + " " + entities
          .ServiceProvider.MiddleInitial;
      }
      else
      {
        export.ErrorFound.Flag = "Y";
        local.DateWorkArea.Year =
          Year(entities.CollectionOfficer.EffectiveDate);
        local.DateWorkArea.Month =
          Month(entities.CollectionOfficer.EffectiveDate);
        local.DateWorkArea.Day = Day(entities.CollectionOfficer.EffectiveDate);
        local.WorkDateWorkAttributes.TextDate10Char =
          NumberToString(local.DateWorkArea.Year, 15) + "-" + NumberToString
          (local.DateWorkArea.Month, 15) + "-" + NumberToString
          (local.DateWorkArea.Day, 15);
        export.NeededToWrite.RptDetail =
          "Service Provider not found for Office Service Provider with Role Type " +
          entities.CollectionOfficer.RoleCode + ", Effective Date " + local
          .WorkDateWorkAttributes.TextDate10Char;

        return;
      }

      if (ReadOffice())
      {
        // ***
        // *** get Office name
        // ***
        export.Extract.Office = entities.Office.Name;

        if (Equal(entities.CollectionOfficer.RoleCode, "SS"))
        {
          return;
        }
      }
      else
      {
        export.ErrorFound.Flag = "Y";
        local.DateWorkArea.Year =
          Year(entities.CollectionOfficer.EffectiveDate);
        local.DateWorkArea.Month =
          Month(entities.CollectionOfficer.EffectiveDate);
        local.DateWorkArea.Day = Day(entities.CollectionOfficer.EffectiveDate);
        local.WorkDateWorkAttributes.TextDate10Char =
          NumberToString(local.DateWorkArea.Year, 15) + "-" + NumberToString
          (local.DateWorkArea.Month, 15) + "-" + NumberToString
          (local.DateWorkArea.Day, 15);
        export.NeededToWrite.RptDetail =
          "Office not found for Office Service Provider with Role Type " + entities
          .CollectionOfficer.RoleCode + ", Effective Date " + local
          .WorkDateWorkAttributes.TextDate10Char;

        return;
      }

      foreach(var item1 in ReadOfficeServiceProvRelationship())
      {
        if (ReadOfficeServiceProvider())
        {
          if (ReadServiceProvider2())
          {
            // ***
            // *** get Section Supervisor last name, first name, middle initial
            // ***
            export.Extract.SectionSupervisor =
              TrimEnd(entities.ServiceProvider.LastName) + ", " + TrimEnd
              (entities.ServiceProvider.FirstName) + " " + entities
              .ServiceProvider.MiddleInitial;

            return;
          }
          else
          {
            export.ErrorFound.Flag = "Y";
            local.DateWorkArea.Year =
              Year(entities.CollectionOfficer.EffectiveDate);
            local.DateWorkArea.Month =
              Month(entities.CollectionOfficer.EffectiveDate);
            local.DateWorkArea.Day =
              Day(entities.CollectionOfficer.EffectiveDate);
            local.WorkDateWorkAttributes.TextDate10Char =
              NumberToString(local.DateWorkArea.Year, 15) + "-" + NumberToString
              (local.DateWorkArea.Month, 15) + "-" + NumberToString
              (local.DateWorkArea.Day, 15);
            export.NeededToWrite.RptDetail =
              "Service Provider not found for Office Service Provider with Role Type " +
              entities.CollectionOfficer.RoleCode + ", Effective Date " + local
              .WorkDateWorkAttributes.TextDate10Char;

            return;
          }
        }
        else
        {
          export.ErrorFound.Flag = "Y";
          local.DateWorkArea.Year =
            Year(entities.OfficeServiceProvRelationship.CreatedDtstamp);
          local.DateWorkArea.Month =
            Month(entities.OfficeServiceProvRelationship.CreatedDtstamp);
          local.DateWorkArea.Day =
            Day(entities.OfficeServiceProvRelationship.CreatedDtstamp);
          local.WorkTimeWorkAttributes.NumericalHours =
            Hour(entities.OfficeServiceProvRelationship.CreatedDtstamp);
          local.WorkTimeWorkAttributes.NumericalMinutes =
            Minute(entities.OfficeServiceProvRelationship.CreatedDtstamp);
          local.WorkTimeWorkAttributes.NumericalSeconds =
            Second(entities.OfficeServiceProvRelationship.CreatedDtstamp);
          local.WorkTimeWorkAttributes.NumericalMicroseconds =
            Microsecond(entities.OfficeServiceProvRelationship.CreatedDtstamp);
          export.NeededToWrite.RptDetail =
            "Office Service Provider not found for Office Service Provider Relationship with Reason Code " +
            entities.OfficeServiceProvRelationship.ReasonCode + ", Created Timestamp " +
            NumberToString(local.DateWorkArea.Year, 15) + "-" + NumberToString
            (local.DateWorkArea.Month, 15) + "-" + NumberToString
            (local.DateWorkArea.Day, 15) + "-" + NumberToString
            (local.WorkTimeWorkAttributes.NumericalHours, 15) + "." + NumberToString
            (local.WorkTimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
            (local.WorkTimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
            (local.WorkTimeWorkAttributes.NumericalMicroseconds, 15);

          return;
        }
      }
    }
  }

  private IEnumerable<bool> ReadCaseAssignmentOfficeServiceProvider()
  {
    entities.CollectionOfficer.Populated = false;
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignmentOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 2);
        entities.CollectionOfficer.SpdGeneratedId = db.GetInt32(reader, 2);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 3);
        entities.CollectionOfficer.OffGeneratedId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OspCode = db.GetString(reader, 4);
        entities.CollectionOfficer.RoleCode = db.GetString(reader, 4);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 5);
        entities.CollectionOfficer.EffectiveDate = db.GetDate(reader, 5);
        entities.CaseAssignment.CasNo = db.GetString(reader, 6);
        entities.CollectionOfficer.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.CollectionOfficer.Populated = true;
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CollectionOfficer.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.CollectionOfficer.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProvRelationship()
  {
    System.Diagnostics.Debug.Assert(entities.CollectionOfficer.Populated);
    entities.OfficeServiceProvRelationship.Populated = false;

    return ReadEach("ReadOfficeServiceProvRelationship",
      (db, command) =>
      {
        db.
          SetString(command, "ospRoleCode", entities.CollectionOfficer.RoleCode);
          
        db.SetDate(
          command, "ospEffectiveDate",
          entities.CollectionOfficer.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId", entities.CollectionOfficer.OffGeneratedId);
          
        db.SetInt32(
          command, "spdGeneratedId", entities.CollectionOfficer.SpdGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvRelationship.OspEffectiveDate =
          db.GetDate(reader, 0);
        entities.OfficeServiceProvRelationship.OspRoleCode =
          db.GetString(reader, 1);
        entities.OfficeServiceProvRelationship.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.OfficeServiceProvRelationship.SpdGeneratedId =
          db.GetInt32(reader, 3);
        entities.OfficeServiceProvRelationship.OspREffectiveDt =
          db.GetDate(reader, 4);
        entities.OfficeServiceProvRelationship.OspRRoleCode =
          db.GetString(reader, 5);
        entities.OfficeServiceProvRelationship.OffRGeneratedId =
          db.GetInt32(reader, 6);
        entities.OfficeServiceProvRelationship.SpdRGeneratedId =
          db.GetInt32(reader, 7);
        entities.OfficeServiceProvRelationship.ReasonCode =
          db.GetString(reader, 8);
        entities.OfficeServiceProvRelationship.CreatedDtstamp =
          db.GetDateTime(reader, 9);
        entities.OfficeServiceProvRelationship.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(
      entities.OfficeServiceProvRelationship.Populated);
    entities.Supervisor.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "roleCode",
          entities.OfficeServiceProvRelationship.OspRRoleCode);
        db.SetDate(
          command, "effectiveDate",
          entities.OfficeServiceProvRelationship.OspREffectiveDt.
            GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvRelationship.OffRGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvRelationship.SpdRGeneratedId);
      },
      (db, reader) =>
      {
        entities.Supervisor.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.Supervisor.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Supervisor.RoleCode = db.GetString(reader, 2);
        entities.Supervisor.EffectiveDate = db.GetDate(reader, 3);
        entities.Supervisor.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Supervisor.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    System.Diagnostics.Debug.Assert(entities.CollectionOfficer.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.CollectionOfficer.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.Supervisor.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", entities.Supervisor.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
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
    /// A value of Extract.
    /// </summary>
    [JsonPropertyName("extract")]
    public CollectionsExtract Extract
    {
      get => extract ??= new();
      set => extract = value;
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

    private CollectionsExtract extract;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Extract.
    /// </summary>
    [JsonPropertyName("extract")]
    public CollectionsExtract Extract
    {
      get => extract ??= new();
      set => extract = value;
    }

    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
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

    private CollectionsExtract extract;
    private Common errorFound;
    private EabReportSend neededToWrite;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of WorkDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("workDateWorkAttributes")]
    public DateWorkAttributes WorkDateWorkAttributes
    {
      get => workDateWorkAttributes ??= new();
      set => workDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of WorkTimeWorkAttributes.
    /// </summary>
    [JsonPropertyName("workTimeWorkAttributes")]
    public TimeWorkAttributes WorkTimeWorkAttributes
    {
      get => workTimeWorkAttributes ??= new();
      set => workTimeWorkAttributes = value;
    }

    private DateWorkArea dateWorkArea;
    private DateWorkAttributes workDateWorkAttributes;
    private TimeWorkAttributes workTimeWorkAttributes;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CollectionOfficer.
    /// </summary>
    [JsonPropertyName("collectionOfficer")]
    public OfficeServiceProvider CollectionOfficer
    {
      get => collectionOfficer ??= new();
      set => collectionOfficer = value;
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
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public OfficeServiceProvider Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider collectionOfficer;
    private Office office;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private OfficeServiceProvider supervisor;
    private CaseAssignment caseAssignment;
    private Case1 case1;
  }
#endregion
}
