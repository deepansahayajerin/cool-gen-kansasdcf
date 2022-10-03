// Program: CO_UPDATE_CODE_VALUE_DETAILS, ID: 371822730, model: 746.
// Short name: SWE00122
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
/// A program: CO_UPDATE_CODE_VALUE_DETAILS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block updates occurrences of entity CODE_VALUE
/// </para>
/// </summary>
[Serializable]
public partial class CoUpdateCodeValueDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_UPDATE_CODE_VALUE_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoUpdateCodeValueDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoUpdateCodeValueDetails.
  /// </summary>
  public CoUpdateCodeValueDetails(IContext context, Import import, Export export)
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
    // *********************************************
    // Date     Developer PR #      Reason
    // -----------------------------------
    // 02/03/00 SWSRCHF   H00085435 ADD and UPDATE function not working
    //                              correctly, the export group changed
    //                              from occurs 100 to occurs 180
    // 02/03/00                     Removed exit states prefixed with "ZD"
    // 01/22/09 Arun Mathias CQ#8604  Do not allow overlapping timeframes.
    // *********************************************
    // *** Problem report H00085435
    // *** 02/03/00 SWSRCHF
    // *** start
    // *** end
    // *** 02/03/00 SWSRCHF
    // *** Problem report H00085435
    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      if (IsEmpty(import.Import1.Item.Detail.Cdvalue) && IsEmpty
        (import.Import1.Item.Detail.Description) && IsEmpty
        (import.Import1.Item.DetailSelection.SelectChar))
      {
        export.Export1.Next();

        continue;
      }

      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
      export.Export1.Update.DetailSelection.SelectChar =
        import.Import1.Item.DetailSelection.SelectChar;

      if (!Lt(new DateTime(1, 1, 1), export.Export1.Item.Detail.EffectiveDate))
      {
        export.Export1.Update.Detail.EffectiveDate = Now().Date;
      }

      export.Export1.Next();
    }

    if (!ReadCode())
    {
      ExitState = "CO0000_PRIMARY_CODE_NF";

      return;
    }

    export.ErrorLineNo.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      ++export.ErrorLineNo.Count;

      if (!IsEmpty(export.Export1.Item.DetailSelection.SelectChar) && AsChar
        (export.Export1.Item.DetailSelection.SelectChar) != 'S')
      {
        ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

        return;
      }

      if (IsEmpty(export.Export1.Item.DetailSelection.SelectChar))
      {
        continue;
      }

      if (AsChar(export.Export1.Item.DetailSelection.SelectChar) == 'S')
      {
        if (IsEmpty(export.Export1.Item.Detail.Description))
        {
          ExitState = "CO0000_CODE_VALUE_DESC_BLANK";

          return;
        }

        if (Equal(export.Export1.Item.Detail.ExpirationDate, null))
        {
          export.Export1.Update.Detail.ExpirationDate =
            new DateTime(2099, 12, 31);
        }

        if (Lt(export.Export1.Item.Detail.ExpirationDate,
          export.Export1.Item.Detail.EffectiveDate))
        {
          ExitState = "CO0000_INVALID_EXP_EFF_DATE";

          return;
        }

        // *** CQ#8604 Changes Begin Here ***
        if (Equal(import.AddUpdateCommand.Command, "ADD"))
        {
          if (export.Export1.Item.Detail.Id > 0)
          {
            ExitState = "CANNOT_ADD_AN_EXISTING_OCCURRENC";

            return;
          }
        }

        foreach(var item in ReadCodeValue3())
        {
          if (Lt(entities.Overlap.ExpirationDate,
            export.Export1.Item.Detail.EffectiveDate) || Lt
            (export.Export1.Item.Detail.ExpirationDate,
            entities.Overlap.EffectiveDate))
          {
            // *** No overlapping timeframe
          }
          else
          {
            ExitState = "ACO_NE0000_DATE_OVERLAP";

            return;
          }
        }

        if (Lt(export.Export1.Item.Detail.EffectiveDate,
          entities.ExistingCode.EffectiveDate) || Lt
          (entities.ExistingCode.ExpirationDate,
          export.Export1.Item.Detail.ExpirationDate))
        {
          ExitState = "CD_VAL_TIMEFRAME_OUTSIDE_CD_NM";

          return;
        }

        // *** CQ#8604 Changes End   Here ***
        switch(TrimEnd(import.AddUpdateCommand.Command))
        {
          case "ADD":
            if (ReadCodeValue2())
            {
              local.Last.Id = entities.ExistingLast.Id;
            }

            try
            {
              CreateCodeValue();
              export.Export1.Update.DetailSelection.SelectChar = "";
              export.Export1.Update.Detail.Assign(entities.Required);
              local.DateWorkArea.Date =
                export.Export1.Item.Detail.ExpirationDate;
              UseCabSetMaximumDiscontinueDate();
              export.Export1.Update.Detail.ExpirationDate =
                local.DateWorkArea.Date;

              // *** Problem report H00085435
              // *** 02/03/00 SWSRCHF
              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  local.DateWorkArea.Date =
                    export.Export1.Item.Detail.ExpirationDate;
                  UseCabSetMaximumDiscontinueDate();
                  export.Export1.Update.Detail.ExpirationDate =
                    local.DateWorkArea.Date;
                  ExitState = "CODE_VALUE_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            break;
          case "UPDATE":
            if (!ReadCodeValue1())
            {
              ExitState = "CODE_VALUE_NF";

              return;
            }

            try
            {
              UpdateCodeValue();
              export.Export1.Update.Detail.Assign(entities.ExistingCodeValue);
              local.DateWorkArea.Date =
                export.Export1.Item.Detail.ExpirationDate;
              UseCabSetMaximumDiscontinueDate();
              export.Export1.Update.Detail.ExpirationDate =
                local.DateWorkArea.Date;
              export.Export1.Update.DetailSelection.SelectChar = "";

              // *** Problem report H00085435
              // *** 02/03/00 SWSRCHF
              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  local.DateWorkArea.Date =
                    export.Export1.Item.Detail.ExpirationDate;
                  UseCabSetMaximumDiscontinueDate();
                  export.Export1.Update.Detail.ExpirationDate =
                    local.DateWorkArea.Date;
                  ExitState = "CODE_VALUE_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            break;
          default:
            ExitState = "CO0000_INVALID_ACTION";

            return;
        }
      }
    }
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private void CreateCodeValue()
  {
    var id = local.Last.Id + 1;
    var codId = entities.ExistingCode.Id;
    var cdvalue = export.Export1.Item.Detail.Cdvalue;
    var effectiveDate = export.Export1.Item.Detail.EffectiveDate;
    var expirationDate = export.Export1.Item.Detail.ExpirationDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedTimestamp = local.ZeroInitialised.LastUpdatedTimestamp;
    var description = export.Export1.Item.Detail.Description;

    entities.Required.Populated = false;
    Update("CreateCodeValue",
      (db, command) =>
      {
        db.SetInt32(command, "covId", id);
        db.SetNullableInt32(command, "codId", codId);
        db.SetString(command, "cdvalue", cdvalue);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "description", description);
      });

    entities.Required.Id = id;
    entities.Required.CodId = codId;
    entities.Required.Cdvalue = cdvalue;
    entities.Required.EffectiveDate = effectiveDate;
    entities.Required.ExpirationDate = expirationDate;
    entities.Required.CreatedBy = createdBy;
    entities.Required.CreatedTimestamp = createdTimestamp;
    entities.Required.LastUpdatedBy = "";
    entities.Required.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Required.Description = description;
    entities.Required.Populated = true;
  }

  private bool ReadCode()
  {
    entities.ExistingCode.Populated = false;

    return Read("ReadCode",
      (db, command) =>
      {
        db.SetString(command, "codeName", import.Code.CodeName);
      },
      (db, reader) =>
      {
        entities.ExistingCode.Id = db.GetInt32(reader, 0);
        entities.ExistingCode.CodeName = db.GetString(reader, 1);
        entities.ExistingCode.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingCode.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingCode.CreatedBy = db.GetString(reader, 4);
        entities.ExistingCode.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.ExistingCode.LastUpdatedBy = db.GetString(reader, 6);
        entities.ExistingCode.LastUpdatedTimestamp = db.GetDateTime(reader, 7);
        entities.ExistingCode.DisplayTitle = db.GetString(reader, 8);
        entities.ExistingCode.Populated = true;
      });
  }

  private bool ReadCodeValue1()
  {
    entities.ExistingCodeValue.Populated = false;

    return Read("ReadCodeValue1",
      (db, command) =>
      {
        db.SetInt32(command, "covId", export.Export1.Item.Detail.Id);
      },
      (db, reader) =>
      {
        entities.ExistingCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingCodeValue.Cdvalue = db.GetString(reader, 1);
        entities.ExistingCodeValue.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingCodeValue.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingCodeValue.CreatedBy = db.GetString(reader, 4);
        entities.ExistingCodeValue.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.ExistingCodeValue.LastUpdatedBy = db.GetString(reader, 6);
        entities.ExistingCodeValue.LastUpdatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingCodeValue.Description = db.GetString(reader, 8);
        entities.ExistingCodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue2()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadCodeValue2",
      null,
      (db, reader) =>
      {
        entities.ExistingLast.Id = db.GetInt32(reader, 0);
        entities.ExistingLast.Cdvalue = db.GetString(reader, 1);
        entities.ExistingLast.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingLast.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingLast.Description = db.GetString(reader, 4);
        entities.ExistingLast.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCodeValue3()
  {
    entities.Overlap.Populated = false;

    return ReadEach("ReadCodeValue3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.ExistingCode.Id);
        db.SetString(command, "cdvalue", export.Export1.Item.Detail.Cdvalue);
        db.SetInt32(command, "covId", export.Export1.Item.Detail.Id);
      },
      (db, reader) =>
      {
        entities.Overlap.Id = db.GetInt32(reader, 0);
        entities.Overlap.CodId = db.GetNullableInt32(reader, 1);
        entities.Overlap.Cdvalue = db.GetString(reader, 2);
        entities.Overlap.EffectiveDate = db.GetDate(reader, 3);
        entities.Overlap.ExpirationDate = db.GetDate(reader, 4);
        entities.Overlap.Populated = true;

        return true;
      });
  }

  private void UpdateCodeValue()
  {
    var cdvalue = export.Export1.Item.Detail.Cdvalue;
    var effectiveDate = export.Export1.Item.Detail.EffectiveDate;
    var expirationDate = export.Export1.Item.Detail.ExpirationDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var description = export.Export1.Item.Detail.Description;

    entities.ExistingCodeValue.Populated = false;
    Update("UpdateCodeValue",
      (db, command) =>
      {
        db.SetString(command, "cdvalue", cdvalue);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "description", description);
        db.SetInt32(command, "covId", entities.ExistingCodeValue.Id);
      });

    entities.ExistingCodeValue.Cdvalue = cdvalue;
    entities.ExistingCodeValue.EffectiveDate = effectiveDate;
    entities.ExistingCodeValue.ExpirationDate = expirationDate;
    entities.ExistingCodeValue.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCodeValue.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCodeValue.Description = description;
    entities.ExistingCodeValue.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailSelection.
      /// </summary>
      [JsonPropertyName("detailSelection")]
      public Common DetailSelection
      {
        get => detailSelection ??= new();
        set => detailSelection = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CodeValue Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 180;

      private Common detailSelection;
      private CodeValue detail;
    }

    /// <summary>
    /// A value of AddUpdateCommand.
    /// </summary>
    [JsonPropertyName("addUpdateCommand")]
    public Common AddUpdateCommand
    {
      get => addUpdateCommand ??= new();
      set => addUpdateCommand = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private Common addUpdateCommand;
    private Code code;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailSelection.
      /// </summary>
      [JsonPropertyName("detailSelection")]
      public Common DetailSelection
      {
        get => detailSelection ??= new();
        set => detailSelection = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CodeValue Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 180;

      private Common detailSelection;
      private CodeValue detail;
    }

    /// <summary>
    /// A value of ErrorLineNo.
    /// </summary>
    [JsonPropertyName("errorLineNo")]
    public Common ErrorLineNo
    {
      get => errorLineNo ??= new();
      set => errorLineNo = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Common errorLineNo;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZeroInitialised.
    /// </summary>
    [JsonPropertyName("zeroInitialised")]
    public CodeValue ZeroInitialised
    {
      get => zeroInitialised ??= new();
      set => zeroInitialised = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public CodeValue Last
    {
      get => last ??= new();
      set => last = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public CodeValue Current
    {
      get => current ??= new();
      set => current = value;
    }

    private CodeValue zeroInitialised;
    private CodeValue last;
    private DateWorkArea dateWorkArea;
    private CodeValue current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCodeValue.
    /// </summary>
    [JsonPropertyName("existingCodeValue")]
    public CodeValue ExistingCodeValue
    {
      get => existingCodeValue ??= new();
      set => existingCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingCode.
    /// </summary>
    [JsonPropertyName("existingCode")]
    public Code ExistingCode
    {
      get => existingCode ??= new();
      set => existingCode = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public CodeValue Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public CodeValue ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of Overlap.
    /// </summary>
    [JsonPropertyName("overlap")]
    public CodeValue Overlap
    {
      get => overlap ??= new();
      set => overlap = value;
    }

    private CodeValue existingCodeValue;
    private Code existingCode;
    private CodeValue required;
    private CodeValue existingLast;
    private CodeValue overlap;
  }
#endregion
}
