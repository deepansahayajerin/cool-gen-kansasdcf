// Program: FN_UPDATE_DISBURSEMENT_STATUS, ID: 371830094, model: 746.
// Short name: SWE00647
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_DISBURSEMENT_STATUS.
/// </summary>
[Serializable]
public partial class FnUpdateDisbursementStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_DISBURSEMENT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateDisbursementStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateDisbursementStatus.
  /// </summary>
  public FnUpdateDisbursementStatus(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    local.Update.Flag = "";
    export.DisbursementStatus.Assign(import.DisbursementStatus);

    // ***** EDIT AREA *****
    // ---------------------
    // If Discontinue date is blank, then default it to max date.
    // ---------------------
    if (Equal(import.DisbursementStatus.DiscontinueDate, null))
    {
      local.DisbursementStatus.DiscontinueDate =
        UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.DisbursementStatus.DiscontinueDate =
        import.DisbursementStatus.DiscontinueDate;
    }

    // --------------------------------
    // Check for an existing record before you can update.
    // --------------------------------
    if (ReadDisbursementStatus1())
    {
      local.Update.Flag = "Y";
    }

    // --------------------------------
    // Added logic to prevent updation if overlapping dates for the same code 
    // value.
    // --------------------------------
    ReadDisbursementStatus3();
    ReadDisbursementStatus4();

    if (!Lt(entities.Test1.DiscontinueDate,
      import.DisbursementStatus.DiscontinueDate) || !
      Lt(import.DisbursementStatus.EffectiveDate, entities.Test2.EffectiveDate))
    {
      local.Update.Flag = "Y";
    }
    else if (entities.Test1.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier && (
        !Lt(entities.Test1.EffectiveDate,
      import.DisbursementStatus.EffectiveDate) && !
      Lt(import.DisbursementStatus.DiscontinueDate,
      entities.Test2.DiscontinueDate) || !
      Lt(import.DisbursementStatus.DiscontinueDate,
      entities.Test2.DiscontinueDate) && import
      .DisbursementStatus.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier || !
      Lt(entities.Test1.EffectiveDate, import.DisbursementStatus.EffectiveDate) &&
      import.DisbursementStatus.SystemGeneratedIdentifier != entities
      .Test1.SystemGeneratedIdentifier))
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";

      return;
    }
    else
    {
      foreach(var item in ReadDisbursementStatus5())
      {
        if (Lt(entities.DisbursementStatus.EffectiveDate,
          import.DisbursementStatus.DiscontinueDate) && Lt
          (import.DisbursementStatus.DiscontinueDate,
          entities.DisbursementStatus.DiscontinueDate) || Lt
          (entities.DisbursementStatus.EffectiveDate,
          import.DisbursementStatus.EffectiveDate) && Lt
          (import.DisbursementStatus.EffectiveDate,
          entities.DisbursementStatus.DiscontinueDate) || Equal
          (entities.DisbursementStatus.DiscontinueDate,
          import.DisbursementStatus.DiscontinueDate) || Equal
          (entities.DisbursementStatus.EffectiveDate,
          import.DisbursementStatus.EffectiveDate) || Equal
          (entities.DisbursementStatus.DiscontinueDate,
          import.DisbursementStatus.EffectiveDate) || Equal
          (entities.DisbursementStatus.EffectiveDate,
          import.DisbursementStatus.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_OVERLAP";

          return;
        }
        else
        {
          continue;
        }
      }

      local.Update.Flag = "Y";
    }

    if (AsChar(local.Update.Flag) == 'Y')
    {
      // ***** MAIN-LINE AREA *****
      // ***** All changes to a Type or Status entity must be audited.  
      // LOG_TYPE_OR_STATUS_AUDIT will perform this process for any of these
      // entities.
      export.TypeStatusAudit.TableName = "DISBURSEMENT_STATUS";
      export.TypeStatusAudit.SystemGeneratedIdentifier =
        entities.DisbursementStatus.SystemGeneratedIdentifier;
      export.TypeStatusAudit.Code = entities.DisbursementStatus.Code;
      export.TypeStatusAudit.Description =
        entities.DisbursementStatus.Description;
      export.TypeStatusAudit.EffectiveDate =
        entities.DisbursementStatus.EffectiveDate;
      export.TypeStatusAudit.DiscontinueDate =
        entities.DisbursementStatus.DiscontinueDate;
      export.TypeStatusAudit.CreatedBy = entities.DisbursementStatus.CreatedBy;
      export.TypeStatusAudit.CreatedTimestamp =
        entities.DisbursementStatus.CreatedTimestamp;
      export.TypeStatusAudit.LastUpdatedBy =
        entities.DisbursementStatus.LastUpdateBy;
      export.TypeStatusAudit.LastUpdatedTmst =
        entities.DisbursementStatus.LastUpdateTmst;
      UseCabLogTypeOrStatusAudit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (ReadDisbursementStatus2())
      {
        try
        {
          UpdateDisbursementStatus();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_STAT_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_STAT_PV";

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

  private void UseCabLogTypeOrStatusAudit()
  {
    var useImport = new CabLogTypeOrStatusAudit.Import();
    var useExport = new CabLogTypeOrStatusAudit.Export();

    useImport.TypeStatusAudit.Assign(export.TypeStatusAudit);

    Call(CabLogTypeOrStatusAudit.Execute, useImport, useExport);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadDisbursementStatus1()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus1",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementStatus.Code);
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementStatus.EffectiveDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.DisbursementStatus.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Code = db.GetString(reader, 1);
        entities.DisbursementStatus.Name = db.GetString(reader, 2);
        entities.DisbursementStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementStatus.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementStatus.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementStatus.LastUpdateBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementStatus.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementStatus.Populated = true;
      });
  }

  private bool ReadDisbursementStatus2()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId",
          import.DisbursementStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Code = db.GetString(reader, 1);
        entities.DisbursementStatus.Name = db.GetString(reader, 2);
        entities.DisbursementStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementStatus.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementStatus.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementStatus.LastUpdateBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementStatus.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementStatus.Populated = true;
      });
  }

  private bool ReadDisbursementStatus3()
  {
    entities.Test1.Populated = false;

    return Read("ReadDisbursementStatus3",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementStatus.Code);
      },
      (db, reader) =>
      {
        entities.Test1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Test1.Code = db.GetString(reader, 1);
        entities.Test1.Name = db.GetString(reader, 2);
        entities.Test1.EffectiveDate = db.GetDate(reader, 3);
        entities.Test1.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Test1.CreatedBy = db.GetString(reader, 5);
        entities.Test1.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Test1.LastUpdateBy = db.GetNullableString(reader, 7);
        entities.Test1.LastUpdateTmst = db.GetNullableDateTime(reader, 8);
        entities.Test1.Description = db.GetNullableString(reader, 9);
        entities.Test1.Populated = true;
      });
  }

  private bool ReadDisbursementStatus4()
  {
    entities.Test2.Populated = false;

    return Read("ReadDisbursementStatus4",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementStatus.Code);
      },
      (db, reader) =>
      {
        entities.Test2.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Test2.Code = db.GetString(reader, 1);
        entities.Test2.Name = db.GetString(reader, 2);
        entities.Test2.EffectiveDate = db.GetDate(reader, 3);
        entities.Test2.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Test2.CreatedBy = db.GetString(reader, 5);
        entities.Test2.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Test2.LastUpdateBy = db.GetNullableString(reader, 7);
        entities.Test2.LastUpdateTmst = db.GetNullableDateTime(reader, 8);
        entities.Test2.Description = db.GetNullableString(reader, 9);
        entities.Test2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisbursementStatus5()
  {
    entities.DisbursementStatus.Populated = false;

    return ReadEach("ReadDisbursementStatus5",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementStatus.Code);
        db.SetInt32(
          command, "disbStatusId",
          import.DisbursementStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Code = db.GetString(reader, 1);
        entities.DisbursementStatus.Name = db.GetString(reader, 2);
        entities.DisbursementStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementStatus.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementStatus.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementStatus.LastUpdateBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementStatus.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementStatus.Populated = true;

        return true;
      });
  }

  private void UpdateDisbursementStatus()
  {
    var code = import.DisbursementStatus.Code;
    var name = import.DisbursementStatus.Name;
    var effectiveDate = import.DisbursementStatus.EffectiveDate;
    var discontinueDate = local.DisbursementStatus.DiscontinueDate;
    var lastUpdateBy = global.UserId;
    var lastUpdateTmst = Now();
    var description = import.DisbursementStatus.Description ?? "";

    entities.DisbursementStatus.Populated = false;
    Update("UpdateDisbursementStatus",
      (db, command) =>
      {
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdateBy", lastUpdateBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "disbStatusId",
          entities.DisbursementStatus.SystemGeneratedIdentifier);
      });

    entities.DisbursementStatus.Code = code;
    entities.DisbursementStatus.Name = name;
    entities.DisbursementStatus.EffectiveDate = effectiveDate;
    entities.DisbursementStatus.DiscontinueDate = discontinueDate;
    entities.DisbursementStatus.LastUpdateBy = lastUpdateBy;
    entities.DisbursementStatus.LastUpdateTmst = lastUpdateTmst;
    entities.DisbursementStatus.Description = description;
    entities.DisbursementStatus.Populated = true;
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
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private DisbursementStatus disbursementStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    /// <summary>
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
    }

    private DisbursementStatus disbursementStatus;
    private TypeStatusAudit typeStatusAudit;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private Common update;
    private DisbursementStatus disbursementStatus;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Test2.
    /// </summary>
    [JsonPropertyName("test2")]
    public DisbursementStatus Test2
    {
      get => test2 ??= new();
      set => test2 = value;
    }

    /// <summary>
    /// A value of Test1.
    /// </summary>
    [JsonPropertyName("test1")]
    public DisbursementStatus Test1
    {
      get => test1 ??= new();
      set => test1 = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private DisbursementStatus test2;
    private DisbursementStatus test1;
    private DisbursementStatus disbursementStatus;
  }
#endregion
}
