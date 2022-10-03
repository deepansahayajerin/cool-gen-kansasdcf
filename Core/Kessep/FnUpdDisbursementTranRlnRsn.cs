// Program: FN_UPD_DISBURSEMENT_TRAN_RLN_RSN, ID: 371834963, model: 746.
// Short name: SWE00619
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPD_DISBURSEMENT_TRAN_RLN_RSN.
/// </para>
/// <para>
/// RESP: FINCLMNGMT
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdDisbursementTranRlnRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPD_DISBURSEMENT_TRAN_RLN_RSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdDisbursementTranRlnRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdDisbursementTranRlnRsn.
  /// </summary>
  public FnUpdDisbursementTranRlnRsn(IContext context, Import import,
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

    // -----------------------------------
    // If Discontinue date is blank, then default it to Maximum date
    // -----------------------------------
    if (Equal(import.DisbursementTranRlnRsn.DiscontinueDate, null))
    {
      local.DisbursementTranRlnRsn.DiscontinueDate =
        UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.DisbursementTranRlnRsn.DiscontinueDate =
        import.DisbursementTranRlnRsn.DiscontinueDate;
    }

    // ------------------------------------
    // Check for an existing record before you can update
    // ------------------------------------
    if (ReadDisbursementTranRlnRsn1())
    {
      local.Update.Flag = "Y";
    }

    ReadDisbursementTranRlnRsn3();
    ReadDisbursementTranRlnRsn4();

    if (!Lt(entities.Test1.DiscontinueDate,
      import.DisbursementTranRlnRsn.DiscontinueDate) || !
      Lt(import.DisbursementTranRlnRsn.EffectiveDate,
      entities.Test2.EffectiveDate))
    {
      local.Update.Flag = "Y";
    }
    else if (entities.Test1.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier && !
      Lt(entities.Test1.EffectiveDate,
      import.DisbursementTranRlnRsn.EffectiveDate) && !
      Lt(import.DisbursementTranRlnRsn.DiscontinueDate,
      entities.Test2.DiscontinueDate) || !
      Lt(import.DisbursementTranRlnRsn.DiscontinueDate,
      entities.Test2.DiscontinueDate) && import
      .DisbursementTranRlnRsn.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier || !
      Lt(entities.Test1.EffectiveDate,
      import.DisbursementTranRlnRsn.EffectiveDate) && import
      .DisbursementTranRlnRsn.SystemGeneratedIdentifier != entities
      .Test1.SystemGeneratedIdentifier)
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";

      return;
    }
    else
    {
      foreach(var item in ReadDisbursementTranRlnRsn5())
      {
        if (Lt(entities.DisbursementTranRlnRsn.EffectiveDate,
          import.DisbursementTranRlnRsn.DiscontinueDate) && Lt
          (import.DisbursementTranRlnRsn.DiscontinueDate,
          entities.DisbursementTranRlnRsn.DiscontinueDate) || Lt
          (entities.DisbursementTranRlnRsn.EffectiveDate,
          import.DisbursementTranRlnRsn.EffectiveDate) && Lt
          (import.DisbursementTranRlnRsn.EffectiveDate,
          entities.DisbursementTranRlnRsn.DiscontinueDate) || Equal
          (entities.DisbursementTranRlnRsn.DiscontinueDate,
          import.DisbursementTranRlnRsn.EffectiveDate) || Equal
          (entities.DisbursementTranRlnRsn.EffectiveDate,
          import.DisbursementTranRlnRsn.DiscontinueDate) || Equal
          (entities.DisbursementTranRlnRsn.EffectiveDate,
          import.DisbursementTranRlnRsn.EffectiveDate) || Equal
          (entities.DisbursementTranRlnRsn.DiscontinueDate,
          import.DisbursementTranRlnRsn.DiscontinueDate))
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
      // ***** All changes to a Type or Status entity must be audited.  
      // LOG_TYPE_OR_STATUS_AUDIT will perform this process for any of these
      // entities.
      export.TypeStatusAudit.TableName = "DISBURSEMENT TRAN RLN RSN";
      export.TypeStatusAudit.SystemGeneratedIdentifier =
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier;
      export.TypeStatusAudit.Code = entities.DisbursementTranRlnRsn.Code;
      export.TypeStatusAudit.Description =
        entities.DisbursementTranRlnRsn.Description;
      export.TypeStatusAudit.EffectiveDate =
        entities.DisbursementTranRlnRsn.EffectiveDate;
      export.TypeStatusAudit.DiscontinueDate =
        entities.DisbursementTranRlnRsn.DiscontinueDate;
      export.TypeStatusAudit.CreatedBy =
        entities.DisbursementTranRlnRsn.CreatedBy;
      export.TypeStatusAudit.CreatedTimestamp =
        entities.DisbursementTranRlnRsn.CreatedTimestamp;
      export.TypeStatusAudit.LastUpdatedBy =
        entities.DisbursementTranRlnRsn.LastUpdatedBy;
      export.TypeStatusAudit.LastUpdatedTmst =
        entities.DisbursementTranRlnRsn.LastUpdatedTmst;
      UseCabLogTypeOrStatusAudit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ********************
      //    UPDATE RECORD
      // ********************
      if (ReadDisbursementTranRlnRsn2())
      {
        try
        {
          UpdateDisbursementTranRlnRsn();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_TRANS_RLN_RSN_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_TRANS_RLN_RSN_PV";

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

  private bool ReadDisbursementTranRlnRsn1()
  {
    entities.DisbursementTranRlnRsn.Populated = false;

    return Read("ReadDisbursementTranRlnRsn1",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTrnRlnRsId",
          import.DisbursementTranRlnRsn.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          import.DisbursementTranRlnRsn.DiscontinueDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementTranRlnRsn.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Code = db.GetString(reader, 1);
        entities.DisbursementTranRlnRsn.Name = db.GetString(reader, 2);
        entities.DisbursementTranRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementTranRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTranRlnRsn.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementTranRlnRsn.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTranRlnRsn.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTranRlnRsn.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTranRlnRsn.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementTranRlnRsn.Populated = true;
      });
  }

  private bool ReadDisbursementTranRlnRsn2()
  {
    entities.DisbursementTranRlnRsn.Populated = false;

    return Read("ReadDisbursementTranRlnRsn2",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTrnRlnRsId",
          import.DisbursementTranRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Code = db.GetString(reader, 1);
        entities.DisbursementTranRlnRsn.Name = db.GetString(reader, 2);
        entities.DisbursementTranRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementTranRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTranRlnRsn.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementTranRlnRsn.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTranRlnRsn.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTranRlnRsn.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTranRlnRsn.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementTranRlnRsn.Populated = true;
      });
  }

  private bool ReadDisbursementTranRlnRsn3()
  {
    entities.Test1.Populated = false;

    return Read("ReadDisbursementTranRlnRsn3",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTranRlnRsn.Code);
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
        entities.Test1.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Test1.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Test1.Description = db.GetNullableString(reader, 9);
        entities.Test1.Populated = true;
      });
  }

  private bool ReadDisbursementTranRlnRsn4()
  {
    entities.Test2.Populated = false;

    return Read("ReadDisbursementTranRlnRsn4",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTranRlnRsn.Code);
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
        entities.Test2.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Test2.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Test2.Description = db.GetNullableString(reader, 9);
        entities.Test2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTranRlnRsn5()
  {
    entities.DisbursementTranRlnRsn.Populated = false;

    return ReadEach("ReadDisbursementTranRlnRsn5",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTranRlnRsn.Code);
        db.SetInt32(
          command, "disbTrnRlnRsId",
          import.DisbursementTranRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Code = db.GetString(reader, 1);
        entities.DisbursementTranRlnRsn.Name = db.GetString(reader, 2);
        entities.DisbursementTranRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementTranRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTranRlnRsn.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementTranRlnRsn.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTranRlnRsn.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTranRlnRsn.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTranRlnRsn.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementTranRlnRsn.Populated = true;

        return true;
      });
  }

  private void UpdateDisbursementTranRlnRsn()
  {
    var code = import.DisbursementTranRlnRsn.Code;
    var effectiveDate = import.DisbursementTranRlnRsn.EffectiveDate;
    var discontinueDate = import.DisbursementTranRlnRsn.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var description = import.DisbursementTranRlnRsn.Description ?? "";

    entities.DisbursementTranRlnRsn.Populated = false;
    Update("UpdateDisbursementTranRlnRsn",
      (db, command) =>
      {
        db.SetString(command, "code", code);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "disbTrnRlnRsId",
          entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier);
      });

    entities.DisbursementTranRlnRsn.Code = code;
    entities.DisbursementTranRlnRsn.EffectiveDate = effectiveDate;
    entities.DisbursementTranRlnRsn.DiscontinueDate = discontinueDate;
    entities.DisbursementTranRlnRsn.LastUpdatedBy = lastUpdatedBy;
    entities.DisbursementTranRlnRsn.LastUpdatedTmst = lastUpdatedTmst;
    entities.DisbursementTranRlnRsn.Description = description;
    entities.DisbursementTranRlnRsn.Populated = true;
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
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    private DisbursementTranRlnRsn disbursementTranRlnRsn;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    private Common update;
    private DisbursementTranRlnRsn disbursementTranRlnRsn;
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
    public DisbursementTranRlnRsn Test2
    {
      get => test2 ??= new();
      set => test2 = value;
    }

    /// <summary>
    /// A value of Test1.
    /// </summary>
    [JsonPropertyName("test1")]
    public DisbursementTranRlnRsn Test1
    {
      get => test1 ??= new();
      set => test1 = value;
    }

    /// <summary>
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    private DisbursementTranRlnRsn test2;
    private DisbursementTranRlnRsn test1;
    private DisbursementTranRlnRsn disbursementTranRlnRsn;
  }
#endregion
}
