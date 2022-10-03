// Program: CO_UPDATE_CODE_NAME, ID: 371822227, model: 746.
// Short name: SWE00120
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
/// A program: CO_UPDATE_CODE_NAME.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block updates CODE_NAME entity
/// </para>
/// </summary>
[Serializable]
public partial class CoUpdateCodeName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_UPDATE_CODE_NAME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoUpdateCodeName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoUpdateCodeName.
  /// </summary>
  public CoUpdateCodeName(IContext context, Import import, Export export):
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
    // 08/13/1999	M Ramirez	Changed READs to singleton selects
    // 01/27/2009      Arun Mathias    CQ#8604 Check the code value timeframes.
    // ---------------------------------------------------------------------
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

      if (IsEmpty(import.Import1.Item.Detail.CodeName) && IsEmpty
        (import.Import1.Item.Detail.DisplayTitle) && IsEmpty
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

      if (AsChar(export.Export1.Item.DetailSelection.SelectChar) == 'S')
      {
        if (IsEmpty(export.Export1.Item.Detail.CodeName))
        {
          ExitState = "CO0000_CODE_NAME_NOT_SPECIFD";

          return;
        }

        if (IsEmpty(export.Export1.Item.Detail.DisplayTitle))
        {
          ExitState = "CO0000_DISPLAY_NAME_BLANK";

          return;
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

        switch(TrimEnd(import.Common.Command))
        {
          case "ADD":
            if (ReadCode3())
            {
              ExitState = "CODE_AE";

              return;
            }

            if (ReadCode1())
            {
              local.Last.Id = entities.ExistingLast.Id;
            }

            try
            {
              CreateCode();
              export.Export1.Update.Detail.Assign(entities.New1);
              local.DateWorkArea.Date =
                export.Export1.Item.Detail.ExpirationDate;
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
                  ExitState = "CODE_AE";

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
            if (ReadCode2())
            {
              ExitState = "CODE_AE";

              return;
            }

            if (!ReadCode4())
            {
              ExitState = "CODE_NF";

              return;
            }

            // *** CQ#8604 Changes Begin Here ***
            foreach(var item in ReadCodeValue())
            {
              if (!Lt(entities.ExistingCodeValue.EffectiveDate,
                export.Export1.Item.Detail.EffectiveDate) && !
                Lt(export.Export1.Item.Detail.ExpirationDate,
                entities.ExistingCodeValue.ExpirationDate))
              {
                // Code value timeframe is within the code name timeframe.
              }
              else
              {
                ExitState = "CD_VAL_TIMEFRAME_OUTSIDE_CD_NM";

                return;
              }
            }

            // *** CQ#8604 Changes End   Here ***
            try
            {
              UpdateCode();
              export.Export1.Update.Detail.Assign(entities.ExistingCode);
              local.DateWorkArea.Date =
                export.Export1.Item.Detail.ExpirationDate;
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
                  ExitState = "CODE_AE";

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

  private void CreateCode()
  {
    var id = local.Last.Id + 1;
    var codeName = export.Export1.Item.Detail.CodeName;
    var effectiveDate = export.Export1.Item.Detail.EffectiveDate;
    var expirationDate = export.Export1.Item.Detail.ExpirationDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedTimestamp = local.ZeroInitialised.LastUpdatedTimestamp;
    var displayTitle = export.Export1.Item.Detail.DisplayTitle;

    entities.New1.Populated = false;
    Update("CreateCode",
      (db, command) =>
      {
        db.SetInt32(command, "codId", id);
        db.SetString(command, "codeName", codeName);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "displayTitle", displayTitle);
      });

    entities.New1.Id = id;
    entities.New1.CodeName = codeName;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.ExpirationDate = expirationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.New1.DisplayTitle = displayTitle;
    entities.New1.Populated = true;
  }

  private bool ReadCode1()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadCode1",
      null,
      (db, reader) =>
      {
        entities.ExistingLast.Id = db.GetInt32(reader, 0);
        entities.ExistingLast.CodeName = db.GetString(reader, 1);
        entities.ExistingLast.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingLast.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingLast.DisplayTitle = db.GetString(reader, 4);
        entities.ExistingLast.Populated = true;
      });
  }

  private bool ReadCode2()
  {
    entities.ExistingCode.Populated = false;

    return Read("ReadCode2",
      (db, command) =>
      {
        db.SetString(command, "codeName", export.Export1.Item.Detail.CodeName);
        db.SetInt32(command, "codId", export.Export1.Item.Detail.Id);
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

  private bool ReadCode3()
  {
    entities.ExistingCode.Populated = false;

    return Read("ReadCode3",
      (db, command) =>
      {
        db.SetString(command, "codeName", export.Export1.Item.Detail.CodeName);
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

  private bool ReadCode4()
  {
    entities.ExistingCode.Populated = false;

    return Read("ReadCode4",
      (db, command) =>
      {
        db.SetInt32(command, "codId", export.Export1.Item.Detail.Id);
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

  private IEnumerable<bool> ReadCodeValue()
  {
    entities.ExistingCodeValue.Populated = false;

    return ReadEach("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.ExistingCode.Id);
      },
      (db, reader) =>
      {
        entities.ExistingCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingCodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.ExistingCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingCodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingCodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.ExistingCodeValue.Populated = true;

        return true;
      });
  }

  private void UpdateCode()
  {
    var codeName = export.Export1.Item.Detail.CodeName;
    var effectiveDate = export.Export1.Item.Detail.EffectiveDate;
    var expirationDate = export.Export1.Item.Detail.ExpirationDate;
    var createdBy = entities.ExistingCode.CreatedBy;
    var createdTimestamp = entities.ExistingCode.CreatedTimestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var displayTitle = export.Export1.Item.Detail.DisplayTitle;

    entities.ExistingCode.Populated = false;
    Update("UpdateCode",
      (db, command) =>
      {
        db.SetString(command, "codeName", codeName);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "displayTitle", displayTitle);
        db.SetInt32(command, "codId", entities.ExistingCode.Id);
      });

    entities.ExistingCode.CodeName = codeName;
    entities.ExistingCode.EffectiveDate = effectiveDate;
    entities.ExistingCode.ExpirationDate = expirationDate;
    entities.ExistingCode.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCode.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCode.DisplayTitle = displayTitle;
    entities.ExistingCode.Populated = true;
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
      public Code Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelection;
      private Code detail;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private Common common;
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
      public Code Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelection;
      private Code detail;
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
    public Code ZeroInitialised
    {
      get => zeroInitialised ??= new();
      set => zeroInitialised = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Code Last
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

    private Code zeroInitialised;
    private Code last;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public Code ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Code New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingCodeValue.
    /// </summary>
    [JsonPropertyName("existingCodeValue")]
    public CodeValue ExistingCodeValue
    {
      get => existingCodeValue ??= new();
      set => existingCodeValue = value;
    }

    private Code existingCode;
    private Code existingLast;
    private Code new1;
    private CodeValue existingCodeValue;
  }
#endregion
}
