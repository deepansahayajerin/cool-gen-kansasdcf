// Program: SI_CREATE_EMPLOYER, ID: 371762204, model: 746.
// Short name: SWE01127
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_EMPLOYER.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateEmployer: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_EMPLOYER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateEmployer(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateEmployer.
  /// </summary>
  public SiCreateEmployer(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //     M A I N T E N A N C E    L O G
    //   Date   Developer   Description
    // 09-24-95 K Evans     Initial Development
    // ---------------------------------------------
    local.Employer.EiwoStartDate = import.Employer.EiwoStartDate;
    local.Employer.EiwoEndDate = import.Employer.EiwoEndDate;
    local.DateWorkArea.Date = new DateTime(1, 1, 1);

    if (Lt(new DateTime(1, 1, 1), import.Employer.EffectiveDate))
    {
      local.Employer.EffectiveDate = import.Employer.EffectiveDate;
    }
    else
    {
      local.Employer.EffectiveDate = Now().Date;
    }

    if (Lt(new DateTime(1, 1, 1), import.Employer.EndDate))
    {
      local.Employer.EndDate = import.Employer.EndDate;
    }
    else
    {
      UseCabSetMaximumDiscontinueDate();
      local.Employer.EndDate = local.Max.Date;
    }

    local.EmployerHistoryDetail.LineNumber = 0;
    local.EmployerHistoryDetail.CreatedBy = global.UserId;
    local.EmployerHistoryDetail.CreatedTimestamp = Now();
    local.EmployerHistoryDetail.LastUpdatedBy = global.UserId;
    local.EmployerHistoryDetail.LastUpdatedTimestamp = Now();
    local.EmployerHistory.CreatedTimestamp = Now();
    local.EmployerHistory.CreatedBy = global.UserId;
    local.EmployerHistory.ActionTaken = "ADDED";
    local.EmployerHistory.ActionDate = Now().Date;
    local.EmployerHistory.LastUpdatedBy = global.UserId;

    if (ReadEmployer())
    {
      if (Lt(local.DateWorkArea.Date, entities.Employer.EiwoStartDate))
      {
        local.Employer.EiwoStartDate = entities.Employer.EiwoStartDate;
        local.Employer.EiwoEndDate = entities.Employer.EiwoEndDate;
      }

      if (IsEmpty(import.Employer.EmailAddress))
      {
        local.Employer.EmailAddress = entities.Employer.EmailAddress;
      }
      else
      {
        local.Employer.EmailAddress = import.Employer.EmailAddress ?? "";
      }
    }
    else
    {
      local.Employer.EmailAddress = import.Employer.EmailAddress ?? "";
    }

    local.EmployerHistoryDetail.Change = "Employer record was created.";
    ++local.EmployerHistoryDetail.LineNumber;

    try
    {
      CreateEmployer();

      try
      {
        CreateEmployerHistory();

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e2)
        {
          switch(GetErrorCode(e2))
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
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
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

      export.Employer.Assign(entities.Employer);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "EMPLOYER_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "EMPLOYER_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    export.Employer.Assign(import.Employer);
    export.Employer.EiwoStartDate = local.Employer.EiwoStartDate;
    export.Employer.EiwoEndDate = local.Employer.EiwoEndDate;
    export.Employer.EffectiveDate = local.Employer.EffectiveDate;
    export.Employer.EndDate = local.Employer.EndDate;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void CreateEmployer()
  {
    var identifier = import.Employer.Identifier;
    var ein = import.Employer.Ein ?? "";
    var kansasId = import.Employer.KansasId ?? "";
    var name = Substring(ToUpper(import.Employer.Name), 1, 33);
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var phoneNo = import.Employer.PhoneNo ?? "";
    var areaCode = import.Employer.AreaCode.GetValueOrDefault();
    var eiwoEndDate = local.Employer.EiwoEndDate;
    var eiwoStartDate = local.Employer.EiwoStartDate;
    var faxAreaCode = import.Employer.FaxAreaCode.GetValueOrDefault();
    var faxPhoneNo = import.Employer.FaxPhoneNo ?? "";
    var emailAddress = local.Employer.EmailAddress ?? "";
    var effectiveDate = local.Employer.EffectiveDate;
    var endDate = local.Employer.EndDate;

    entities.Employer.Populated = false;
    Update("CreateEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "ein", ein);
        db.SetNullableString(command, "kansasId", kansasId);
        db.SetNullableString(command, "name", name);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetNullableString(command, "phoneNo", phoneNo);
        db.SetNullableInt32(command, "areaCode", areaCode);
        db.SetNullableDate(command, "eiwoEndDate", eiwoEndDate);
        db.SetNullableDate(command, "eiwoStartDate", eiwoStartDate);
        db.SetNullableInt32(command, "faxAreaCode", faxAreaCode);
        db.SetNullableString(command, "faxPhoneNo", faxPhoneNo);
        db.SetNullableString(command, "emailAddress", emailAddress);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
      });

    entities.Employer.Identifier = identifier;
    entities.Employer.Ein = ein;
    entities.Employer.KansasId = kansasId;
    entities.Employer.Name = name;
    entities.Employer.CreatedBy = createdBy;
    entities.Employer.CreatedTstamp = createdTstamp;
    entities.Employer.PhoneNo = phoneNo;
    entities.Employer.AreaCode = areaCode;
    entities.Employer.EiwoEndDate = eiwoEndDate;
    entities.Employer.EiwoStartDate = eiwoStartDate;
    entities.Employer.FaxAreaCode = faxAreaCode;
    entities.Employer.FaxPhoneNo = faxPhoneNo;
    entities.Employer.EmailAddress = emailAddress;
    entities.Employer.EffectiveDate = effectiveDate;
    entities.Employer.EndDate = endDate;
    entities.Employer.Populated = true;
  }

  private void CreateEmployerHistory()
  {
    var actionTaken = local.EmployerHistory.ActionTaken ?? "";
    var actionDate = local.EmployerHistory.ActionDate;
    var createdBy = local.EmployerHistory.CreatedBy;
    var createdTimestamp = local.EmployerHistory.CreatedTimestamp;
    var lastUpdatedBy = local.EmployerHistory.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = Now();
    var empId = entities.Employer.Identifier;

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

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", import.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.Employer.CreatedBy = db.GetString(reader, 4);
        entities.Employer.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 6);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 7);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 8);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 9);
        entities.Employer.FaxAreaCode = db.GetNullableInt32(reader, 10);
        entities.Employer.FaxPhoneNo = db.GetNullableString(reader, 11);
        entities.Employer.EmailAddress = db.GetNullableString(reader, 12);
        entities.Employer.EffectiveDate = db.GetNullableDate(reader, 13);
        entities.Employer.EndDate = db.GetNullableDate(reader, 14);
        entities.Employer.Populated = true;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private Employer employer;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private Employer employer;
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
    /// A value of EmployerHistoryDetail.
    /// </summary>
    [JsonPropertyName("employerHistoryDetail")]
    public EmployerHistoryDetail EmployerHistoryDetail
    {
      get => employerHistoryDetail ??= new();
      set => employerHistoryDetail = value;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
    }

    private DateWorkArea dateWorkArea;
    private EmployerHistoryDetail employerHistoryDetail;
    private EmployerHistory employerHistory;
    private DateWorkArea max;
    private Employer employer;
    private SystemGenerated systemGenerated;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    private Employer employer;
    private EmployerHistory employerHistory;
  }
#endregion
}
