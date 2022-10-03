// Program: CO_UPDATE_CODE_VAL_COMBINATION, ID: 371822552, model: 746.
// Short name: SWE00121
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
/// A program: CO_UPDATE_CODE_VAL_COMBINATION.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block updates occurrences of entity 
/// CODE_VALUE_COMBINATION
/// </para>
/// </summary>
[Serializable]
public partial class CoUpdateCodeValCombination: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_UPDATE_CODE_VAL_COMBINATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoUpdateCodeValCombination(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoUpdateCodeValCombination.
  /// </summary>
  public CoUpdateCodeValCombination(IContext context, Import import,
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
    // ---------------------------------------------------------------------
    // Date		Developer	Description
    // ---------------------------------------------------------------------
    // 				Initial Development
    // 08/12/1999	M Ramirez	Changed READs to READEACHs to check
    // 				for multiples
    // 08/12/1999	M Ramirez	Minor clean-up
    // ---------------------------------------------------------------------
    export.PrimaryCode.Assign(import.PrimaryCode);
    export.PrimaryCodeValue.Assign(import.PrimaryCodeValue);
    export.Secondary.Assign(import.Secondary);

    if (!ReadCode1())
    {
      ExitState = "CO0000_PRIMARY_CODE_NF";

      return;
    }

    foreach(var item in ReadCodeValue3())
    {
      if (local.Primary.Id > 0)
      {
        ExitState = "CO0000_INVALID_CROSS_CODE_VAL";

        return;
      }
      else
      {
        local.Primary.Id = entities.ExistingPrimaryCodeValue.Id;
      }
    }

    // mjr
    // ----------------------------------------------------
    // 08/12/1999
    // For currency
    // -----------------------------------------------------------------
    if (!ReadCodeValue1())
    {
      ExitState = "CO0000_PRIMARY_CODE_VALUE_NF";

      return;
    }

    if (!ReadCode2())
    {
      ExitState = "CO0000_CROSS_VAL_CODE_NF";

      return;
    }

    export.ErrorLineNo.Count = 0;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailSelection.SelectChar =
        import.Import1.Item.DetailSelection.SelectChar;
      export.Export1.Update.DetailSecondary.Assign(
        import.Import1.Item.DetailSecondary);
      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
      ++export.ErrorLineNo.Count;

      if (!IsEmpty(export.Export1.Item.DetailSelection.SelectChar) && AsChar
        (export.Export1.Item.DetailSelection.SelectChar) != 'S')
      {
        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        export.Export1.Next();

        return;
      }

      export.Export1.Next();
    }

    if (!Equal(import.AddUpdateCommand.Command, "ADD") && !
      Equal(import.AddUpdateCommand.Command, "DELETE") && !
      Equal(import.AddUpdateCommand.Command, "UPDATE"))
    {
      ExitState = "CO0000_INVALID_ACTION";

      return;
    }

    export.ErrorLineNo.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      ++export.ErrorLineNo.Count;

      if (AsChar(export.Export1.Item.DetailSelection.SelectChar) != 'S')
      {
        continue;
      }

      local.Secondary.Id = 0;

      foreach(var item in ReadCodeValue4())
      {
        if (local.Secondary.Id > 0)
        {
          ExitState = "CO0000_INVALID_CROSS_CODE_VAL";

          return;
        }
        else
        {
          local.Secondary.Id = entities.ExistingSecondaryCodeValue.Id;
        }
      }

      // mjr
      // ----------------------------------------------------
      // 08/12/1999
      // For currency
      // -----------------------------------------------------------------
      if (!ReadCodeValue2())
      {
        ExitState = "CO0000_CROSS_VAL_CODE_VAL_NF";

        return;
      }

      if (ReadCodeValueCombination2())
      {
        if (Equal(import.AddUpdateCommand.Command, "ADD"))
        {
          ExitState = "CODE_VALUE_COMBINATION_AE";

          return;
        }
      }
      else if (Equal(import.AddUpdateCommand.Command, "UPDATE") || Equal
        (import.AddUpdateCommand.Command, "DELETE"))
      {
        ExitState = "CODE_VALUE_COMBINATION_NF";

        return;
      }

      if (!Lt(local.Null1.EffectiveDate,
        export.Export1.Item.Detail.EffectiveDate))
      {
        export.Export1.Update.Detail.EffectiveDate = Now().Date;
      }

      local.DateWorkArea.Date = export.Export1.Item.Detail.ExpirationDate;
      UseCabSetMaximumDiscontinueDate();
      export.Export1.Update.Detail.ExpirationDate = local.DateWorkArea.Date;

      if (Lt(export.Export1.Item.Detail.ExpirationDate,
        export.Export1.Item.Detail.EffectiveDate))
      {
        ExitState = "CO0000_INVALID_EXP_EFF_DATE";

        return;
      }

      switch(TrimEnd(import.AddUpdateCommand.Command))
      {
        case "ADD":
          if (ReadCodeValueCombination1())
          {
            local.Last.Id = entities.ExistingLast.Id;
          }

          try
          {
            CreateCodeValueCombination();
            export.Export1.Update.Detail.Assign(entities.New1);
            local.DateWorkArea.Date = export.Export1.Item.Detail.ExpirationDate;
            UseCabSetMaximumDiscontinueDate();
            export.Export1.Update.Detail.ExpirationDate =
              local.DateWorkArea.Date;
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
                ExitState = "CODE_VALUE_COMBINATION_AE";

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
          try
          {
            UpdateCodeValueCombination();
            export.Export1.Update.Detail.Assign(entities.Existing);
            local.DateWorkArea.Date = export.Export1.Item.Detail.ExpirationDate;
            UseCabSetMaximumDiscontinueDate();
            export.Export1.Update.Detail.ExpirationDate =
              local.DateWorkArea.Date;
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
                ExitState = "CODE_VALUE_COMBINATION_AE";

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
        case "DELETE":
          DeleteCodeValueCombination();
          export.Export1.Update.Detail.EffectiveDate =
            local.Null1.EffectiveDate;
          export.Export1.Update.Detail.ExpirationDate =
            local.Null1.EffectiveDate;

          break;
        default:
          break;
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

  private void CreateCodeValueCombination()
  {
    var id = local.Last.Id + 1;
    var covId = entities.ExistingPrimaryCodeValue.Id;
    var covSId = entities.ExistingSecondaryCodeValue.Id;
    var effectiveDate = export.Export1.Item.Detail.EffectiveDate;
    var expirationDate = export.Export1.Item.Detail.ExpirationDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedTimestamp = local.Null1.LastUpdatedTimestamp;

    entities.New1.Populated = false;
    Update("CreateCodeValueCombination",
      (db, command) =>
      {
        db.SetInt32(command, "cvcId", id);
        db.SetInt32(command, "covId", covId);
        db.SetInt32(command, "covSId", covSId);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
      });

    entities.New1.Id = id;
    entities.New1.CovId = covId;
    entities.New1.CovSId = covSId;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.ExpirationDate = expirationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.New1.Populated = true;
  }

  private void DeleteCodeValueCombination()
  {
    Update("DeleteCodeValueCombination",
      (db, command) =>
      {
        db.SetInt32(command, "cvcId", entities.Existing.Id);
      });
  }

  private bool ReadCode1()
  {
    entities.ExistingPrimaryCode.Populated = false;

    return Read("ReadCode1",
      (db, command) =>
      {
        db.SetString(command, "codeName", export.PrimaryCode.CodeName);
      },
      (db, reader) =>
      {
        entities.ExistingPrimaryCode.Id = db.GetInt32(reader, 0);
        entities.ExistingPrimaryCode.CodeName = db.GetString(reader, 1);
        entities.ExistingPrimaryCode.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingPrimaryCode.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingPrimaryCode.DisplayTitle = db.GetString(reader, 4);
        entities.ExistingPrimaryCode.Populated = true;
      });
  }

  private bool ReadCode2()
  {
    entities.ExistingSecondaryCode.Populated = false;

    return Read("ReadCode2",
      (db, command) =>
      {
        db.SetString(command, "codeName", export.Secondary.CodeName);
      },
      (db, reader) =>
      {
        entities.ExistingSecondaryCode.Id = db.GetInt32(reader, 0);
        entities.ExistingSecondaryCode.CodeName = db.GetString(reader, 1);
        entities.ExistingSecondaryCode.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingSecondaryCode.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingSecondaryCode.DisplayTitle = db.GetString(reader, 4);
        entities.ExistingSecondaryCode.Populated = true;
      });
  }

  private bool ReadCodeValue1()
  {
    entities.ExistingPrimaryCodeValue.Populated = false;

    return Read("ReadCodeValue1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.ExistingPrimaryCode.Id);
        db.SetInt32(command, "covId", local.Primary.Id);
      },
      (db, reader) =>
      {
        entities.ExistingPrimaryCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingPrimaryCodeValue.CodId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingPrimaryCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingPrimaryCodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingPrimaryCodeValue.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingPrimaryCodeValue.Description = db.GetString(reader, 5);
        entities.ExistingPrimaryCodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue2()
  {
    entities.ExistingSecondaryCodeValue.Populated = false;

    return Read("ReadCodeValue2",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "codId", entities.ExistingSecondaryCode.Id);
          
        db.SetInt32(command, "covId", local.Secondary.Id);
      },
      (db, reader) =>
      {
        entities.ExistingSecondaryCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingSecondaryCodeValue.CodId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingSecondaryCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingSecondaryCodeValue.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingSecondaryCodeValue.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingSecondaryCodeValue.Description =
          db.GetString(reader, 5);
        entities.ExistingSecondaryCodeValue.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCodeValue3()
  {
    entities.ExistingPrimaryCodeValue.Populated = false;

    return ReadEach("ReadCodeValue3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.ExistingPrimaryCode.Id);
        db.SetString(command, "cdvalue", export.PrimaryCodeValue.Cdvalue);
      },
      (db, reader) =>
      {
        entities.ExistingPrimaryCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingPrimaryCodeValue.CodId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingPrimaryCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingPrimaryCodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingPrimaryCodeValue.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingPrimaryCodeValue.Description = db.GetString(reader, 5);
        entities.ExistingPrimaryCodeValue.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCodeValue4()
  {
    entities.ExistingSecondaryCodeValue.Populated = false;

    return ReadEach("ReadCodeValue4",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "codId", entities.ExistingSecondaryCode.Id);
          
        db.SetString(
          command, "cdvalue", export.Export1.Item.DetailSecondary.Cdvalue);
      },
      (db, reader) =>
      {
        entities.ExistingSecondaryCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingSecondaryCodeValue.CodId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingSecondaryCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingSecondaryCodeValue.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingSecondaryCodeValue.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingSecondaryCodeValue.Description =
          db.GetString(reader, 5);
        entities.ExistingSecondaryCodeValue.Populated = true;

        return true;
      });
  }

  private bool ReadCodeValueCombination1()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadCodeValueCombination1",
      null,
      (db, reader) =>
      {
        entities.ExistingLast.Id = db.GetInt32(reader, 0);
        entities.ExistingLast.CovId = db.GetInt32(reader, 1);
        entities.ExistingLast.CovSId = db.GetInt32(reader, 2);
        entities.ExistingLast.Populated = true;
      });
  }

  private bool ReadCodeValueCombination2()
  {
    entities.Existing.Populated = false;

    return Read("ReadCodeValueCombination2",
      (db, command) =>
      {
        db.SetInt32(command, "covId", entities.ExistingPrimaryCodeValue.Id);
        db.SetInt32(command, "covSId", entities.ExistingSecondaryCodeValue.Id);
      },
      (db, reader) =>
      {
        entities.Existing.Id = db.GetInt32(reader, 0);
        entities.Existing.CovId = db.GetInt32(reader, 1);
        entities.Existing.CovSId = db.GetInt32(reader, 2);
        entities.Existing.EffectiveDate = db.GetDate(reader, 3);
        entities.Existing.ExpirationDate = db.GetDate(reader, 4);
        entities.Existing.CreatedBy = db.GetString(reader, 5);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Existing.LastUpdatedBy = db.GetString(reader, 7);
        entities.Existing.LastUpdatedTimestamp = db.GetDateTime(reader, 8);
        entities.Existing.Populated = true;
      });
  }

  private void UpdateCodeValueCombination()
  {
    var effectiveDate = export.Export1.Item.Detail.EffectiveDate;
    var expirationDate = export.Export1.Item.Detail.ExpirationDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.Existing.Populated = false;
    Update("UpdateCodeValueCombination",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(command, "cvcId", entities.Existing.Id);
      });

    entities.Existing.EffectiveDate = effectiveDate;
    entities.Existing.ExpirationDate = expirationDate;
    entities.Existing.LastUpdatedBy = lastUpdatedBy;
    entities.Existing.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Existing.Populated = true;
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
      public CodeValueCombination Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailSecondary.
      /// </summary>
      [JsonPropertyName("detailSecondary")]
      public CodeValue DetailSecondary
      {
        get => detailSecondary ??= new();
        set => detailSecondary = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelection;
      private CodeValueCombination detail;
      private CodeValue detailSecondary;
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
    /// A value of Secondary.
    /// </summary>
    [JsonPropertyName("secondary")]
    public Code Secondary
    {
      get => secondary ??= new();
      set => secondary = value;
    }

    /// <summary>
    /// A value of PrimaryCodeValue.
    /// </summary>
    [JsonPropertyName("primaryCodeValue")]
    public CodeValue PrimaryCodeValue
    {
      get => primaryCodeValue ??= new();
      set => primaryCodeValue = value;
    }

    /// <summary>
    /// A value of PrimaryCode.
    /// </summary>
    [JsonPropertyName("primaryCode")]
    public Code PrimaryCode
    {
      get => primaryCode ??= new();
      set => primaryCode = value;
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
    private Code secondary;
    private CodeValue primaryCodeValue;
    private Code primaryCode;
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
      public CodeValueCombination Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailSecondary.
      /// </summary>
      [JsonPropertyName("detailSecondary")]
      public CodeValue DetailSecondary
      {
        get => detailSecondary ??= new();
        set => detailSecondary = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelection;
      private CodeValueCombination detail;
      private CodeValue detailSecondary;
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
    /// A value of Secondary.
    /// </summary>
    [JsonPropertyName("secondary")]
    public Code Secondary
    {
      get => secondary ??= new();
      set => secondary = value;
    }

    /// <summary>
    /// A value of PrimaryCodeValue.
    /// </summary>
    [JsonPropertyName("primaryCodeValue")]
    public CodeValue PrimaryCodeValue
    {
      get => primaryCodeValue ??= new();
      set => primaryCodeValue = value;
    }

    /// <summary>
    /// A value of PrimaryCode.
    /// </summary>
    [JsonPropertyName("primaryCode")]
    public Code PrimaryCode
    {
      get => primaryCode ??= new();
      set => primaryCode = value;
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
    private Code secondary;
    private CodeValue primaryCodeValue;
    private Code primaryCode;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Secondary.
    /// </summary>
    [JsonPropertyName("secondary")]
    public CodeValue Secondary
    {
      get => secondary ??= new();
      set => secondary = value;
    }

    /// <summary>
    /// A value of Primary.
    /// </summary>
    [JsonPropertyName("primary")]
    public CodeValue Primary
    {
      get => primary ??= new();
      set => primary = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CodeValueCombination Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public CodeValueCombination Last
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

    private CodeValue secondary;
    private CodeValue primary;
    private CodeValueCombination null1;
    private CodeValueCombination last;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public CodeValueCombination ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CodeValueCombination New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CodeValueCombination Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ExistingSecondaryCodeValue.
    /// </summary>
    [JsonPropertyName("existingSecondaryCodeValue")]
    public CodeValue ExistingSecondaryCodeValue
    {
      get => existingSecondaryCodeValue ??= new();
      set => existingSecondaryCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingSecondaryCode.
    /// </summary>
    [JsonPropertyName("existingSecondaryCode")]
    public Code ExistingSecondaryCode
    {
      get => existingSecondaryCode ??= new();
      set => existingSecondaryCode = value;
    }

    /// <summary>
    /// A value of ExistingPrimaryCode.
    /// </summary>
    [JsonPropertyName("existingPrimaryCode")]
    public Code ExistingPrimaryCode
    {
      get => existingPrimaryCode ??= new();
      set => existingPrimaryCode = value;
    }

    /// <summary>
    /// A value of ExistingPrimaryCodeValue.
    /// </summary>
    [JsonPropertyName("existingPrimaryCodeValue")]
    public CodeValue ExistingPrimaryCodeValue
    {
      get => existingPrimaryCodeValue ??= new();
      set => existingPrimaryCodeValue = value;
    }

    private CodeValueCombination existingLast;
    private CodeValueCombination new1;
    private CodeValueCombination existing;
    private CodeValue existingSecondaryCodeValue;
    private Code existingSecondaryCode;
    private Code existingPrimaryCode;
    private CodeValue existingPrimaryCodeValue;
  }
#endregion
}
