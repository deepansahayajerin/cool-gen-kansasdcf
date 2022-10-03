// Program: LE_TRIB_VALIDATE_TRIBUNAL, ID: 372021821, model: 746.
// Short name: SWE00825
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
/// A program: LE_TRIB_VALIDATE_TRIBUNAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block validates Tribunal details
/// </para>
/// </summary>
[Serializable]
public partial class LeTribValidateTribunal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_TRIB_VALIDATE_TRIBUNAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeTribValidateTribunal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeTribValidateTribunal.
  /// </summary>
  public LeTribValidateTribunal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    //   Date		Developer	Request #	Description
    // 10/08/98	D. Jean     			Changed domain test for zip code, removed read 
    // for tribunal on update and delete
    // 09/15/2000	GVandy		PR 102557	Modified to validate tribunal document 
    // header attributes.
    // *******************************************************************
    MoveFips(import.Fips, export.Fips);
    export.ErrorCodes.Index = -1;

    if (Equal(import.UserAction.Command, "ADD") || Equal
      (import.UserAction.Command, "UPDATE"))
    {
      if (import.Fips.State == 0 && IsEmpty(import.FipsTribAddress.Country))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 1;
        export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;

        return;
      }

      if (import.Fips.State != 0 && !IsEmpty(import.FipsTribAddress.Country))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 4;
        export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;

        return;
      }

      if (import.Fips.State != 0 || import.Fips.County != 0 || import
        .Fips.Location != 0)
      {
        if (ReadFips())
        {
          export.Fips.Assign(entities.ExistingFips);
        }
        else
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 2;
          export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;

          return;
        }
      }
    }

    if (IsEmpty(import.Tribunal.Name))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 3;
      export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (Equal(import.UserAction.Command, "ADD") || Equal
      (import.UserAction.Command, "UPDATE"))
    {
      if (import.Fips.State == 20 && IsEmpty(import.Tribunal.DocumentHeader1))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 17;
        export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;

        return;
      }
    }

    if (Equal(global.Command, "ADD"))
    {
      if (import.Tribunal.Identifier > 0)
      {
        if (ReadTribunal())
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 5;
          export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;

          return;
        }
      }
    }

    if (Equal(import.UserAction.Command, "ADD") || Equal
      (import.UserAction.Command, "UPDATE"))
    {
      if (export.Fips.State != 0 || export.Fips.County != 0 || export
        .Fips.Location != 0)
      {
        // --- The tribunal has a fips assoc with it.
      }
      else
      {
        // --- For foreign tribunals the address details must be specified.
        if (import.Import1.IsEmpty)
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 7;
          export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;

          return;
        }

        local.ErrorEntryNo.Count = 0;

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          ++local.ErrorEntryNo.Count;

          if (IsEmpty(import.Import1.Item.DetailSelAddr.SelectChar))
          {
            continue;
          }

          if (import.Import1.Item.Detail.AreaCode.GetValueOrDefault() == 0 && IsEmpty
            (import.Import1.Item.Detail.City) && IsEmpty
            (import.Import1.Item.Detail.County) && import
            .Import1.Item.Detail.FaxAreaCode.GetValueOrDefault() == 0 && IsEmpty
            (import.Import1.Item.Detail.FaxExtension) && import
            .Import1.Item.Detail.FaxNumber.GetValueOrDefault() == 0 && IsEmpty
            (import.Import1.Item.Detail.PhoneExtension) && import
            .Import1.Item.Detail.PhoneNumber.GetValueOrDefault() == 0 && IsEmpty
            (import.Import1.Item.Detail.State) && IsEmpty
            (import.Import1.Item.Detail.Street1) && IsEmpty
            (import.Import1.Item.Detail.Street2) && IsEmpty
            (import.Import1.Item.Detail.Type1) && IsEmpty
            (import.Import1.Item.Detail.Zip3) && IsEmpty
            (import.Import1.Item.Detail.Zip4) && IsEmpty
            (import.Import1.Item.Detail.ZipCode))
          {
            // ---------------------------------------------
            // The entry is empty.
            // ---------------------------------------------
            continue;
          }

          local.Code.CodeName = "ADDRESS TYPE";
          local.CodeValue.Cdvalue = import.Import1.Item.Detail.Type1;
          UseCabValidateCodeValue1();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 8;
            export.ErrorCodes.Update.DetailErrEntryNo.Count =
              local.ErrorEntryNo.Count;
            export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;

            return;
          }

          if (IsEmpty(import.Import1.Item.Detail.Street1))
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 9;
            export.ErrorCodes.Update.DetailErrEntryNo.Count =
              local.ErrorEntryNo.Count;
            export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;

            return;
          }

          if (IsEmpty(import.Import1.Item.Detail.City))
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 10;
            export.ErrorCodes.Update.DetailErrEntryNo.Count =
              local.ErrorEntryNo.Count;
            export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;

            return;
          }
        }
      }

      if (!IsEmpty(import.FipsTribAddress.Country))
      {
        local.Code.CodeName = "COUNTRY CODE";
        local.CodeValue.Cdvalue = import.FipsTribAddress.Country ?? Spaces(10);
        UseCabValidateCodeValue2();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 16;
          export.ErrorCodes.Update.DetailErrEntryNo.Count =
            local.ErrorEntryNo.Count;
          export.LastErrorEntry.Count = export.ErrorCodes.Index + 1;
        }
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.State = source.State;
    target.County = source.County;
    target.Location = source.Location;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private bool ReadFips()
  {
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.Fips.State);
        db.SetInt32(command, "county", import.Fips.County);
        db.SetInt32(command, "location", import.Fips.Location);
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.StateDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 4);
        entities.ExistingFips.LocationDescription =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 6);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 7);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingTribunal.Name = db.GetString(reader, 1);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 2);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 3);
        entities.ExistingTribunal.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailSelAddr.
      /// </summary>
      [JsonPropertyName("detailSelAddr")]
      public Common DetailSelAddr
      {
        get => detailSelAddr ??= new();
        set => detailSelAddr = value;
      }

      /// <summary>
      /// A value of DetailListAddrTp.
      /// </summary>
      [JsonPropertyName("detailListAddrTp")]
      public Standard DetailListAddrTp
      {
        get => detailListAddrTp ??= new();
        set => detailListAddrTp = value;
      }

      /// <summary>
      /// A value of DetailListStates.
      /// </summary>
      [JsonPropertyName("detailListStates")]
      public Standard DetailListStates
      {
        get => detailListStates ??= new();
        set => detailListStates = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public FipsTribAddress Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detailSelAddr;
      private Standard detailListAddrTp;
      private Standard detailListStates;
      private FipsTribAddress detail;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private FipsTribAddress fipsTribAddress;
    private Common userAction;
    private Fips fips;
    private Tribunal tribunal;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of DetailErrEntryNo.
      /// </summary>
      [JsonPropertyName("detailErrEntryNo")]
      public Common DetailErrEntryNo
      {
        get => detailErrEntryNo ??= new();
        set => detailErrEntryNo = value;
      }

      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrEntryNo;
      private Common detailErrorCode;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LastErrorEntry.
    /// </summary>
    [JsonPropertyName("lastErrorEntry")]
    public Common LastErrorEntry
    {
      get => lastErrorEntry ??= new();
      set => lastErrorEntry = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    private Fips fips;
    private Common lastErrorEntry;
    private Array<ErrorCodesGroup> errorCodes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ErrorEntryNo.
    /// </summary>
    [JsonPropertyName("errorEntryNo")]
    public Common ErrorEntryNo
    {
      get => errorEntryNo ??= new();
      set => errorEntryNo = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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

    private Common errorEntryNo;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingFipsTribAddress")]
    public FipsTribAddress ExistingFipsTribAddress
    {
      get => existingFipsTribAddress ??= new();
      set => existingFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    private FipsTribAddress existingFipsTribAddress;
    private Tribunal existingTribunal;
    private Fips existingFips;
  }
#endregion
}
