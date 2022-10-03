// Program: CAB_LOG_TYPE_OR_STATUS_AUDIT, ID: 371725489, model: 746.
// Short name: SWE00065
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_LOG_TYPE_OR_STATUS_AUDIT.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will create an occurrence in the type status audit entity 
/// type as a before image of a type or status entity type.
/// </para>
/// </summary>
[Serializable]
public partial class CabLogTypeOrStatusAudit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_LOG_TYPE_OR_STATUS_AUDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabLogTypeOrStatusAudit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabLogTypeOrStatusAudit.
  /// </summary>
  public CabLogTypeOrStatusAudit(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    try
    {
      CreateTypeStatusAudit();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "TYPE_STATUS_AUDIT_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "TYPE_STATUS_AUDIT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateTypeStatusAudit()
  {
    var auditTimestamp = Now();
    var tableName = import.TypeStatusAudit.TableName;
    var systemGeneratedIdentifier =
      import.TypeStatusAudit.SystemGeneratedIdentifier;
    var stringOfOthers = import.TypeStatusAudit.StringOfOthers ?? "";
    var auditedBy = global.UserId;
    var code = import.TypeStatusAudit.Code ?? "";
    var name = import.TypeStatusAudit.Name;
    var effectiveDate = import.TypeStatusAudit.EffectiveDate;
    var discontinueDate = import.TypeStatusAudit.DiscontinueDate;
    var createdBy = import.TypeStatusAudit.CreatedBy ?? "";
    var createdTimestamp = import.TypeStatusAudit.CreatedTimestamp;
    var lastUpdatedBy = import.TypeStatusAudit.LastUpdatedBy ?? "";
    var lastUpdatedTmst = import.TypeStatusAudit.LastUpdatedTmst;
    var description = import.TypeStatusAudit.Description ?? "";

    entities.TypeStatusAudit.Populated = false;
    Update("CreateTypeStatusAudit",
      (db, command) =>
      {
        db.SetDateTime(command, "auditTimestamp", auditTimestamp);
        db.SetString(command, "tableName", tableName);
        db.SetInt32(command, "typeStatAudId", systemGeneratedIdentifier);
        db.SetNullableString(command, "stringOfOthers", stringOfOthers);
        db.SetString(command, "auditedBy", auditedBy);
        db.SetNullableString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "description", description);
      });

    entities.TypeStatusAudit.AuditTimestamp = auditTimestamp;
    entities.TypeStatusAudit.TableName = tableName;
    entities.TypeStatusAudit.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.TypeStatusAudit.StringOfOthers = stringOfOthers;
    entities.TypeStatusAudit.AuditedBy = auditedBy;
    entities.TypeStatusAudit.Code = code;
    entities.TypeStatusAudit.Name = name;
    entities.TypeStatusAudit.EffectiveDate = effectiveDate;
    entities.TypeStatusAudit.DiscontinueDate = discontinueDate;
    entities.TypeStatusAudit.CreatedBy = createdBy;
    entities.TypeStatusAudit.CreatedTimestamp = createdTimestamp;
    entities.TypeStatusAudit.LastUpdatedBy = lastUpdatedBy;
    entities.TypeStatusAudit.LastUpdatedTmst = lastUpdatedTmst;
    entities.TypeStatusAudit.Description = description;
    entities.TypeStatusAudit.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
    }

    private TypeStatusAudit typeStatusAudit;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
    }

    private TypeStatusAudit typeStatusAudit;
  }
#endregion
}
