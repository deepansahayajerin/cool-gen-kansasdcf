// Program: SI_CREATE_EMPLOYER_RELATION, ID: 371766120, model: 746.
// Short name: SWE01128
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_EMPLOYER_RELATION.
/// </para>
/// <para>
/// This AB links a  registered agent to an employer
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateEmployerRelation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_EMPLOYER_RELATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateEmployerRelation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateEmployerRelation.
  /// </summary>
  public SiCreateEmployerRelation(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //     M A I N T E N A N C E    L O G
    // Date	  Developer	  Description
    // 10-21-95  H. Sharland     Initial Development
    // 4/29/97	  SHERAZ MALIK	  CHANGE CURRENT_DATE
    // 10/23/98  W. Campbell     Code added to prevent
    //                           multiple HQWS from being created
    //                           for a given work site.
    // ------------------------------------------------------------
    // 07/13/99  Marek Lachowicz   Change property of READ (Select Only)
    local.Current.Date = Now().Date;
    local.StartDate.Timestamp = Now();

    if (Lt(local.Null1.Date, import.EmployerRelation.EffectiveDate))
    {
      if (Lt(import.EmployerRelation.EffectiveDate, local.StartDate.Date))
      {
        local.StartDate.Date = local.Current.Date;
      }
      else
      {
        local.StartDate.Date = import.EmployerRelation.EffectiveDate;
      }
    }
    else
    {
      local.StartDate.Date = local.Current.Date;
    }

    if (Lt(local.Null1.Date, import.EmployerRelation.EndDate))
    {
      if (Lt(local.StartDate.Date, import.EmployerRelation.EffectiveDate))
      {
        local.End.Date = import.EmployerRelation.EndDate;
      }
      else
      {
        local.StartDate.Date = import.EmployerRelation.EffectiveDate;
        UseCabSetMaximumDiscontinueDate();
      }
    }
    else
    {
      UseCabSetMaximumDiscontinueDate();
    }

    // 07/13/99  M.L 	Change property of READ (Select Only)
    if (!ReadEmployer2())
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    // 07/13/99  M.L 	Change property of READ (Select Only)
    if (!ReadEmployer1())
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    // If there active/inactive already existing relationship between two 
    // employers for this type of relationship then can not add the same
    // relationship again
    if (AsChar(import.SelectScreen.Text1) == 'X')
    {
      if (ReadEmployerRelation1())
      {
        ExitState = "EMPLOYER_RELATION_AE";

        return;
      }
    }
    else if (ReadEmployerRelation2())
    {
      ExitState = "EMPLOYER_RELATION_AE";

      return;
    }

    local.EmployerHistoryDetail.LineNumber = 0;
    local.EmployerHistoryDetail.CreatedBy = global.UserId;
    local.EmployerHistoryDetail.CreatedTimestamp = Now();
    local.EmployerHistoryDetail.LastUpdatedTimestamp = Now();
    local.EmployerHistoryDetail.LastUpdatedBy = global.UserId;
    local.EmployerHistory.CreatedTimestamp = Now();
    local.EmployerHistory.CreatedBy = global.UserId;
    local.EmployerHistory.ActionTaken = "ADDED";
    local.EmployerHistory.ActionDate = Now().Date;
    local.EmployerHistory.LastUpdatedBy = global.UserId;
    local.EmployerHistoryDetail.Change = "Created a " + import
      .EmployerRelation.Type1 + " relationship between ";
    ++local.EmployerHistoryDetail.LineNumber;

    try
    {
      CreateEmployerHistory();

      try
      {
        CreateEmployerHistoryDetail();
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "EMPLOYER_HISTORY_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "EMPLOYER_HISTORY_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    local.EmployerHistoryDetail.Change = TrimEnd(entities.Ws.Name) + " and " + TrimEnd
      (entities.ServiceProvider.Name);
    ++local.EmployerHistoryDetail.LineNumber;

    try
    {
      CreateEmployerHistoryDetail();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ------------------------------------------------
    // 10/23/98 W. Campbell  End of code added to
    // prevent multiple HQWS from being created
    // for a given work site.
    // ------------------------------------------------
    local.ControlTable.Identifier = "EMPLOYER RELATION";
    UseAccessControlTable();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(import.SelectScreen.Text1) == 'X')
    {
      try
      {
        CreateEmployerRelation2();
        export.EmployerRelation.EffectiveDate = local.StartDate.Date;
        export.EmployerRelation.EndDate = local.End.Date;
        export.EmployerRelation.Identifier = local.ControlTable.LastUsedNumber;
        export.EmployerRelation.Note = import.EmployerRelation.Note ?? "";
        export.EmployerRelation.Type1 = import.EmployerRelation.Type1;
        export.EmployerRelation.UpdatedTimestamp = local.StartDate.Timestamp;
        export.EmployerRelation.CreatedTimestamp = local.StartDate.Timestamp;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "EMPLOYER_RELATION_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "EMPLOYER_RELATION_PV";

            break;
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
        CreateEmployerRelation1();
        export.EmployerRelation.EffectiveDate = local.StartDate.Date;
        export.EmployerRelation.EndDate = local.End.Date;
        export.EmployerRelation.Identifier = local.ControlTable.LastUsedNumber;
        export.EmployerRelation.Note = import.EmployerRelation.Note ?? "";
        export.EmployerRelation.Type1 = import.EmployerRelation.Type1;
        export.EmployerRelation.UpdatedTimestamp = local.StartDate.Timestamp;
        export.EmployerRelation.CreatedTimestamp = local.StartDate.Timestamp;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "EMPLOYER_RELATION_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "EMPLOYER_RELATION_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.End.Date = useExport.DateWorkArea.Date;
  }

  private void CreateEmployerHistory()
  {
    var actionTaken = local.EmployerHistory.ActionTaken ?? "";
    var actionDate = local.EmployerHistory.ActionDate;
    var createdBy = local.EmployerHistory.CreatedBy;
    var createdTimestamp = local.EmployerHistory.CreatedTimestamp;
    var lastUpdatedBy = local.EmployerHistory.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = Now();
    var empId = entities.Ws.Identifier;

    entities.EmployerHistory.Populated = false;
    Update("CreateEmployerHistory",
      (db, command) =>
      {
        db.SetNullableString(command, "actionTaken", actionTaken);
        db.SetDate(command, "actionDate", actionDate);
        db.SetNullableString(command, "note", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "empId", empId);
      });

    entities.EmployerHistory.ActionTaken = actionTaken;
    entities.EmployerHistory.ActionDate = actionDate;
    entities.EmployerHistory.Note = "";
    entities.EmployerHistory.CreatedBy = createdBy;
    entities.EmployerHistory.CreatedTimestamp = createdTimestamp;
    entities.EmployerHistory.LastUpdatedBy = lastUpdatedBy;
    entities.EmployerHistory.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.EmployerHistory.EmpId = empId;
    entities.EmployerHistory.Populated = true;
  }

  private void CreateEmployerHistoryDetail()
  {
    System.Diagnostics.Debug.Assert(entities.EmployerHistory.Populated);

    var empId = entities.EmployerHistory.EmpId;
    var ehxCreatedTmst = entities.EmployerHistory.CreatedTimestamp;
    var lineNumber = local.EmployerHistoryDetail.LineNumber;
    var createdBy = local.EmployerHistoryDetail.CreatedBy;
    var createdTimestamp = local.EmployerHistoryDetail.CreatedTimestamp;
    var lastUpdatedBy = local.EmployerHistoryDetail.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.EmployerHistoryDetail.LastUpdatedTimestamp;
    var change = local.EmployerHistoryDetail.Change ?? "";

    entities.EmployerHistoryDetail.Populated = false;
    Update("CreateEmployerHistoryDetail",
      (db, command) =>
      {
        db.SetInt32(command, "empId", empId);
        db.SetDateTime(command, "ehxCreatedTmst", ehxCreatedTmst);
        db.SetInt32(command, "lineNumber", lineNumber);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "change", change);
      });

    entities.EmployerHistoryDetail.EmpId = empId;
    entities.EmployerHistoryDetail.EhxCreatedTmst = ehxCreatedTmst;
    entities.EmployerHistoryDetail.LineNumber = lineNumber;
    entities.EmployerHistoryDetail.CreatedBy = createdBy;
    entities.EmployerHistoryDetail.CreatedTimestamp = createdTimestamp;
    entities.EmployerHistoryDetail.LastUpdatedBy = lastUpdatedBy;
    entities.EmployerHistoryDetail.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.EmployerHistoryDetail.Change = change;
    entities.EmployerHistoryDetail.Populated = true;
  }

  private void CreateEmployerRelation1()
  {
    var identifier = local.ControlTable.LastUsedNumber;
    var effectiveDate = local.StartDate.Date;
    var endDate = local.End.Date;
    var createdTimestamp = local.StartDate.Timestamp;
    var createdBy = global.UserId;
    var empHqId = entities.ServiceProvider.Identifier;
    var empLocId = entities.Ws.Identifier;
    var note = import.EmployerRelation.Note ?? "";
    var type1 = import.EmployerRelation.Type1;

    entities.EmployerRelation.Populated = false;
    Update("CreateEmployerRelation1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableDate(command, "verifiedDate", null);
        db.SetNullableString(command, "verifiedBy", "");
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetInt32(command, "empHqId", empHqId);
        db.SetInt32(command, "empLocId", empLocId);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "type", type1);
      });

    entities.EmployerRelation.Identifier = identifier;
    entities.EmployerRelation.VerifiedDate = null;
    entities.EmployerRelation.VerifiedBy = "";
    entities.EmployerRelation.EffectiveDate = effectiveDate;
    entities.EmployerRelation.EndDate = endDate;
    entities.EmployerRelation.CreatedTimestamp = createdTimestamp;
    entities.EmployerRelation.UpdatedTimestamp = createdTimestamp;
    entities.EmployerRelation.CreatedBy = createdBy;
    entities.EmployerRelation.UpdatedBy = createdBy;
    entities.EmployerRelation.EmpHqId = empHqId;
    entities.EmployerRelation.EmpLocId = empLocId;
    entities.EmployerRelation.Note = note;
    entities.EmployerRelation.Type1 = type1;
    entities.EmployerRelation.Populated = true;
  }

  private void CreateEmployerRelation2()
  {
    var identifier = local.ControlTable.LastUsedNumber;
    var effectiveDate = local.StartDate.Date;
    var endDate = local.End.Date;
    var createdTimestamp = local.StartDate.Timestamp;
    var createdBy = global.UserId;
    var empHqId = entities.Ws.Identifier;
    var empLocId = entities.ServiceProvider.Identifier;
    var note = import.EmployerRelation.Note ?? "";
    var type1 = import.EmployerRelation.Type1;

    entities.EmployerRelation.Populated = false;
    Update("CreateEmployerRelation2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableDate(command, "verifiedDate", null);
        db.SetNullableString(command, "verifiedBy", "");
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetInt32(command, "empHqId", empHqId);
        db.SetInt32(command, "empLocId", empLocId);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "type", type1);
      });

    entities.EmployerRelation.Identifier = identifier;
    entities.EmployerRelation.VerifiedDate = null;
    entities.EmployerRelation.VerifiedBy = "";
    entities.EmployerRelation.EffectiveDate = effectiveDate;
    entities.EmployerRelation.EndDate = endDate;
    entities.EmployerRelation.CreatedTimestamp = createdTimestamp;
    entities.EmployerRelation.UpdatedTimestamp = createdTimestamp;
    entities.EmployerRelation.CreatedBy = createdBy;
    entities.EmployerRelation.UpdatedBy = createdBy;
    entities.EmployerRelation.EmpHqId = empHqId;
    entities.EmployerRelation.EmpLocId = empLocId;
    entities.EmployerRelation.Note = note;
    entities.EmployerRelation.Type1 = type1;
    entities.EmployerRelation.Populated = true;
  }

  private bool ReadEmployer1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.ServiceProvider.Identifier);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.Identifier = db.GetInt32(reader, 0);
        entities.ServiceProvider.Name = db.GetNullableString(reader, 1);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadEmployer2()
  {
    entities.Ws.Populated = false;

    return Read("ReadEmployer2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Ws.Identifier);
      },
      (db, reader) =>
      {
        entities.Ws.Identifier = db.GetInt32(reader, 0);
        entities.Ws.Name = db.GetNullableString(reader, 1);
        entities.Ws.Populated = true;
      });
  }

  private bool ReadEmployerRelation1()
  {
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelation1",
      (db, command) =>
      {
        db.SetString(command, "type", import.EmployerRelation.Type1);
        db.SetInt32(command, "empHqId", entities.Ws.Identifier);
        db.SetInt32(command, "empLocId", entities.ServiceProvider.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.VerifiedDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.VerifiedBy = db.GetNullableString(reader, 2);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 3);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 4);
        entities.EmployerRelation.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.EmployerRelation.UpdatedTimestamp = db.GetDateTime(reader, 6);
        entities.EmployerRelation.CreatedBy = db.GetString(reader, 7);
        entities.EmployerRelation.UpdatedBy = db.GetString(reader, 8);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 9);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 10);
        entities.EmployerRelation.Note = db.GetNullableString(reader, 11);
        entities.EmployerRelation.Type1 = db.GetString(reader, 12);
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadEmployerRelation2()
  {
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelation2",
      (db, command) =>
      {
        db.SetString(command, "type", import.EmployerRelation.Type1);
        db.SetInt32(command, "empLocId", entities.Ws.Identifier);
        db.SetInt32(command, "empHqId", entities.ServiceProvider.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.VerifiedDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.VerifiedBy = db.GetNullableString(reader, 2);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 3);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 4);
        entities.EmployerRelation.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.EmployerRelation.UpdatedTimestamp = db.GetDateTime(reader, 6);
        entities.EmployerRelation.CreatedBy = db.GetString(reader, 7);
        entities.EmployerRelation.UpdatedBy = db.GetString(reader, 8);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 9);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 10);
        entities.EmployerRelation.Note = db.GetNullableString(reader, 11);
        entities.EmployerRelation.Type1 = db.GetString(reader, 12);
        entities.EmployerRelation.Populated = true;
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
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    /// <summary>
    /// A value of Ws.
    /// </summary>
    [JsonPropertyName("ws")]
    public Employer Ws
    {
      get => ws ??= new();
      set => ws = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public Employer ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of SelectScreen.
    /// </summary>
    [JsonPropertyName("selectScreen")]
    public WorkArea SelectScreen
    {
      get => selectScreen ??= new();
      set => selectScreen = value;
    }

    private EmployerRelation employerRelation;
    private Employer ws;
    private Employer serviceProvider;
    private WorkArea selectScreen;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    private EmployerRelation employerRelation;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EmployerHistoryDetail.
    /// </summary>
    [JsonPropertyName("employerHistoryDetail")]
    public EmployerHistoryDetail EmployerHistoryDetail
    {
      get => employerHistoryDetail ??= new();
      set => employerHistoryDetail = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
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
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
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
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
    }

    private EmployerHistoryDetail employerHistoryDetail;
    private DateWorkArea end;
    private DateWorkArea null1;
    private DateWorkArea startDate;
    private DateWorkArea current;
    private ControlTable controlTable;
    private EmployerHistory employerHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EmployerHistoryDetail.
    /// </summary>
    [JsonPropertyName("employerHistoryDetail")]
    public EmployerHistoryDetail EmployerHistoryDetail
    {
      get => employerHistoryDetail ??= new();
      set => employerHistoryDetail = value;
    }

    /// <summary>
    /// A value of OtherHq.
    /// </summary>
    [JsonPropertyName("otherHq")]
    public Employer OtherHq
    {
      get => otherHq ??= new();
      set => otherHq = value;
    }

    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public Employer ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Ws.
    /// </summary>
    [JsonPropertyName("ws")]
    public Employer Ws
    {
      get => ws ??= new();
      set => ws = value;
    }

    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
    }

    private EmployerHistoryDetail employerHistoryDetail;
    private Employer otherHq;
    private EmployerRelation employerRelation;
    private Employer serviceProvider;
    private Employer ws;
    private EmployerHistory employerHistory;
  }
#endregion
}
