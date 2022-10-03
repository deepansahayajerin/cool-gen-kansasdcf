// Program: FN_UPDATE_DISBURSEMENT_TRAN_TYPE, ID: 371837538, model: 746.
// Short name: SWE00648
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
/// A program: FN_UPDATE_DISBURSEMENT_TRAN_TYPE.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateDisbursementTranType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_DISBURSEMENT_TRAN_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateDisbursementTranType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateDisbursementTranType.
  /// </summary>
  public FnUpdateDisbursementTranType(IContext context, Import import,
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
    export.DisbursementTransactionType.
      Assign(import.DisbursementTransactionType);
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate2();

    // ***** EDIT AREA *****
    // If Discontinue date is blank, then default it to Maximum date
    if (Equal(import.DisbursementTransactionType.DiscontinueDate, null))
    {
      local.DisbursementTransactionType.DiscontinueDate =
        UseCabSetMaximumDiscontinueDate1();
    }
    else
    {
      local.DisbursementTransactionType.DiscontinueDate =
        import.DisbursementTransactionType.DiscontinueDate;
    }

    // ----------------
    // Check for an existing record before you can update.
    // ----------------
    if (ReadDisbursementTransactionType2())
    {
      local.Update.Flag = "Y";
    }

    ReadDisbursementTransactionType3();
    ReadDisbursementTransactionType4();

    if (!Lt(entities.Test1.DiscontinueDate,
      import.DisbursementTransactionType.DiscontinueDate) || !
      Lt(import.DisbursementTransactionType.EffectiveDate,
      entities.Test2.EffectiveDate))
    {
      local.Update.Flag = "Y";
    }
    else if (entities.Test1.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier && (
        !Lt(entities.Test1.EffectiveDate,
      import.DisbursementTransactionType.EffectiveDate) && !
      Lt(import.DisbursementTransactionType.DiscontinueDate,
      entities.Test2.DiscontinueDate) || !
      Lt(entities.Test1.EffectiveDate,
      import.DisbursementTransactionType.EffectiveDate) && import
      .DisbursementTransactionType.SystemGeneratedIdentifier != entities
      .Test1.SystemGeneratedIdentifier || !
      Lt(import.DisbursementTransactionType.DiscontinueDate,
      entities.Test2.DiscontinueDate) && import
      .DisbursementTransactionType.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier))
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";

      return;
    }
    else
    {
      foreach(var item in ReadDisbursementTransactionType5())
      {
        if (Lt(entities.DisbursementTransactionType.EffectiveDate,
          import.DisbursementTransactionType.DiscontinueDate) && Lt
          (import.DisbursementTransactionType.DiscontinueDate,
          entities.DisbursementTransactionType.DiscontinueDate) || Lt
          (entities.DisbursementTransactionType.EffectiveDate,
          import.DisbursementTransactionType.EffectiveDate) && Lt
          (import.DisbursementTransactionType.EffectiveDate,
          entities.DisbursementTransactionType.DiscontinueDate) || Equal
          (entities.DisbursementTransactionType.DiscontinueDate,
          import.DisbursementTransactionType.DiscontinueDate) || Equal
          (entities.DisbursementTransactionType.EffectiveDate,
          import.DisbursementTransactionType.DiscontinueDate) || Equal
          (entities.DisbursementTransactionType.EffectiveDate,
          import.DisbursementTransactionType.EffectiveDate) || Equal
          (entities.DisbursementTransactionType.DiscontinueDate,
          import.DisbursementTransactionType.EffectiveDate))
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
      export.TypeStatusAudit.TableName = "disbursement tran type";
      export.TypeStatusAudit.SystemGeneratedIdentifier =
        entities.DisbursementTransactionType.SystemGeneratedIdentifier;
      export.TypeStatusAudit.Code = entities.DisbursementTransactionType.Code;
      export.TypeStatusAudit.Description =
        entities.DisbursementTransactionType.Description;
      export.TypeStatusAudit.EffectiveDate =
        entities.DisbursementTransactionType.EffectiveDate;
      export.TypeStatusAudit.DiscontinueDate =
        entities.DisbursementTransactionType.DiscontinueDate;
      export.TypeStatusAudit.CreatedBy =
        entities.DisbursementTransactionType.CreatedBy;
      export.TypeStatusAudit.CreatedTimestamp =
        entities.DisbursementTransactionType.CreatedTmst;
      export.TypeStatusAudit.LastUpdatedBy =
        entities.DisbursementTransactionType.LastUpdatedBy;
      export.TypeStatusAudit.LastUpdatedTmst =
        entities.DisbursementTransactionType.LastUpdatedTmst;
      UseCabLogTypeOrStatusAudit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (ReadDisbursementTransactionType1())
      {
        try
        {
          UpdateDisbursementTransactionType();
          export.DisbursementTransactionType.Assign(
            entities.DisbursementTransactionType);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_TRANS_TYP_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_TRANS_TYP_PV";

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

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadDisbursementTransactionType1()
  {
    entities.DisbursementTransactionType.Populated = false;

    return Read("ReadDisbursementTransactionType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranTypeId",
          import.DisbursementTransactionType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTransactionType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionType.Code = db.GetString(reader, 1);
        entities.DisbursementTransactionType.Name = db.GetString(reader, 2);
        entities.DisbursementTransactionType.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbursementTransactionType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTransactionType.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbursementTransactionType.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.DisbursementTransactionType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransactionType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTransactionType.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementTransactionType.Populated = true;
      });
  }

  private bool ReadDisbursementTransactionType2()
  {
    entities.DisbursementTransactionType.Populated = false;

    return Read("ReadDisbursementTransactionType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranTypeId",
          import.DisbursementTransactionType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementTransactionType.EffectiveDate.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "discontinueDate",
          import.DisbursementTransactionType.DiscontinueDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTransactionType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionType.Code = db.GetString(reader, 1);
        entities.DisbursementTransactionType.Name = db.GetString(reader, 2);
        entities.DisbursementTransactionType.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbursementTransactionType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTransactionType.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbursementTransactionType.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.DisbursementTransactionType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransactionType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTransactionType.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementTransactionType.Populated = true;
      });
  }

  private bool ReadDisbursementTransactionType3()
  {
    entities.Test1.Populated = false;

    return Read("ReadDisbursementTransactionType3",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTransactionType.Code);
      },
      (db, reader) =>
      {
        entities.Test1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Test1.Code = db.GetString(reader, 1);
        entities.Test1.Name = db.GetString(reader, 2);
        entities.Test1.EffectiveDate = db.GetDate(reader, 3);
        entities.Test1.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Test1.CreatedBy = db.GetString(reader, 5);
        entities.Test1.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Test1.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Test1.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Test1.Description = db.GetNullableString(reader, 9);
        entities.Test1.Populated = true;
      });
  }

  private bool ReadDisbursementTransactionType4()
  {
    entities.Test2.Populated = false;

    return Read("ReadDisbursementTransactionType4",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTransactionType.Code);
      },
      (db, reader) =>
      {
        entities.Test2.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Test2.Code = db.GetString(reader, 1);
        entities.Test2.Name = db.GetString(reader, 2);
        entities.Test2.EffectiveDate = db.GetDate(reader, 3);
        entities.Test2.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Test2.CreatedBy = db.GetString(reader, 5);
        entities.Test2.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Test2.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Test2.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Test2.Description = db.GetNullableString(reader, 9);
        entities.Test2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionType5()
  {
    entities.DisbursementTransactionType.Populated = false;

    return ReadEach("ReadDisbursementTransactionType5",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTransactionType.Code);
        db.SetInt32(
          command, "disbTranTypeId",
          import.DisbursementTransactionType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTransactionType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionType.Code = db.GetString(reader, 1);
        entities.DisbursementTransactionType.Name = db.GetString(reader, 2);
        entities.DisbursementTransactionType.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbursementTransactionType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTransactionType.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbursementTransactionType.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.DisbursementTransactionType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransactionType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTransactionType.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementTransactionType.Populated = true;

        return true;
      });
  }

  private void UpdateDisbursementTransactionType()
  {
    var code = import.DisbursementTransactionType.Code;
    var name = import.DisbursementTransactionType.Name;
    var effectiveDate = import.DisbursementTransactionType.EffectiveDate;
    var discontinueDate = import.DisbursementTransactionType.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var description = import.DisbursementTransactionType.Description ?? "";

    entities.DisbursementTransactionType.Populated = false;
    Update("UpdateDisbursementTransactionType",
      (db, command) =>
      {
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "disbTranTypeId",
          entities.DisbursementTransactionType.SystemGeneratedIdentifier);
      });

    entities.DisbursementTransactionType.Code = code;
    entities.DisbursementTransactionType.Name = name;
    entities.DisbursementTransactionType.EffectiveDate = effectiveDate;
    entities.DisbursementTransactionType.DiscontinueDate = discontinueDate;
    entities.DisbursementTransactionType.LastUpdatedBy = lastUpdatedBy;
    entities.DisbursementTransactionType.LastUpdatedTmst = lastUpdatedTmst;
    entities.DisbursementTransactionType.Description = description;
    entities.DisbursementTransactionType.Populated = true;
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
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private DisbursementTransactionType disbursementTransactionType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
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

    private DisbursementTransactionType disbursementTransactionType;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private Common update;
    private DateWorkArea dateWorkArea;
    private DateWorkArea maximum;
    private DateWorkArea zero;
    private DisbursementTransactionType disbursementTransactionType;
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
    public DisbursementTransactionType Test2
    {
      get => test2 ??= new();
      set => test2 = value;
    }

    /// <summary>
    /// A value of Test1.
    /// </summary>
    [JsonPropertyName("test1")]
    public DisbursementTransactionType Test1
    {
      get => test1 ??= new();
      set => test1 = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private DisbursementTransactionType test2;
    private DisbursementTransactionType test1;
    private DisbursementTransactionType disbursementTransactionType;
  }
#endregion
}
