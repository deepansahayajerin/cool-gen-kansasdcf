// Program: SP_SVPL_LIST_SERVICE_PROVIDER, ID: 371784288, model: 746.
// Short name: SWESVPLP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_SVPL_LIST_SERVICE_PROVIDER.
/// </para>
/// <para>
/// This procedure is used to add, update, delete and select service providers.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpSvplListServiceProvider: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_SVPL_LIST_SERVICE_PROVIDER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpSvplListServiceProvider(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpSvplListServiceProvider.
  /// </summary>
  public SpSvplListServiceProvider(IContext context, Import import,
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
    // *********************************************
    // ** DATE      *  DESCRIPTION
    // ** 05/11/95     a. Hackler        initial development
    // ** 02/02/96     a. hackler         retro fits
    // ** 01/04/97	R. Marchman	   Add new security/next tran.
    // ** 11/16/98     A. Massey       Removed all the convoluted sorting and 
    // filtering from this screen.  It was complicated far beyond the user's
    // needs.
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.HiddenFromSecurity.SelectChar = import.HiddenFromSecurity.SelectChar;
    local.Current.Date = Now().Date;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();
    }

    MoveServiceProvider(import.Starting, export.Starting);
    export.SortBy.SelectChar = import.SortBy.SelectChar;
    export.ActiveOnly.Flag = import.ActiveOnly.Flag;

    if (IsEmpty(export.ActiveOnly.Flag))
    {
      export.ActiveOnly.Flag = "Y";
    }

    if (AsChar(export.ActiveOnly.Flag) != 'Y' && AsChar
      (export.ActiveOnly.Flag) != 'N')
    {
      var field = GetField(export.ActiveOnly, "flag");

      field.Error = true;

      ExitState = "INVALID_VALUE";

      return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV") || Equal(global.Command, "RETURN"))
    {
    }
    else
    {
      // if the next tran info is not equal to spaces, this implies the user 
      // requested a next tran action. now validate
      export.Standard.NextTransaction = import.Standard.NextTransaction;

      if (IsEmpty(import.Standard.NextTransaction))
      {
      }
      else
      {
        UseScCabNextTranPut();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }

        return;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      global.Command = "DISPLAY";
    }

    if (import.Return1.Count > 0)
    {
      for(import.Return1.Index = 0; import.Return1.Index < import
        .Return1.Count; ++import.Return1.Index)
      {
        if (!import.Return1.CheckSize())
        {
          break;
        }

        export.Return1.Index = import.Return1.Index;
        export.Return1.CheckSize();

        export.Return1.Update.ServiceProvider.Assign(
          import.Return1.Item.ServiceProvider);
      }

      import.Return1.CheckIndex();
    }
    else
    {
      export.HiddenSelected.Index = -1;
      export.Return1.Index = -1;
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;
      export.Export1.Update.ServiceProvider.Assign(
        import.Import1.Item.ServiceProvider);
      export.Export1.Update.Active.Flag = import.Import1.Item.Active.Flag;

      if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
      {
        ++local.Count.Count;
        export.HiddenSelected1.Assign(export.Export1.Item.ServiceProvider);

        if (AsChar(export.HiddenFromSecurity.SelectChar) == 'M')
        {
          // if you linked here from the security system, allow multiple 
          // selections of service provider.
          if (export.HiddenSelected.IsFull)
          {
            goto Test;
          }

          ++export.HiddenSelected.Index;
          export.HiddenSelected.CheckSize();

          export.HiddenSelected.Update.HiddenSelected1.Assign(
            import.Import1.Item.ServiceProvider);
        }
        else if (AsChar(export.HiddenFromSecurity.SelectChar) == 'R')
        {
          if (export.Return1.Index + 1 == Export.ReturnGroup.Capacity)
          {
            if (Equal(global.Command, "RETURN"))
            {
            }
            else
            {
              ExitState = "ACO_NE0000_MAX_SELECTIONS_REACH";
            }

            export.Export1.Next();

            break;
          }

          ++export.Return1.Index;
          export.Return1.CheckSize();

          export.Return1.Update.ServiceProvider.Assign(
            import.Import1.Item.ServiceProvider);

          if (export.Return1.Index + 1 == Export.ReturnGroup.Capacity)
          {
            if (Equal(global.Command, "RETURN"))
            {
            }
            else
            {
              ExitState = "ACO_NE0000_MAX_SELECTIONS_REACH";
            }

            export.Export1.Next();

            break;
          }
        }
      }

Test:

      export.Export1.Next();
    }

    if (AsChar(export.HiddenFromSecurity.SelectChar) == 'M' || AsChar
      (export.HiddenFromSecurity.SelectChar) == '1')
    {
      // "M" = allowed to select more than 1 for security
      // "1" = allowed to select only 1 for security
      // no need to check security if comming from the security screens.
    }
    else
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (!IsEmpty(export.Starting.UserId) && !
        IsEmpty(export.Starting.LastName))
      {
        ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";

        var field1 = GetField(export.Starting, "userId");

        field1.Error = true;

        var field2 = GetField(export.Starting, "lastName");

        field2.Error = true;

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        if (!IsEmpty(export.Starting.UserId))
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadServiceProvider2())
          {
            if (AsChar(export.ActiveOnly.Flag) == 'Y')
            {
              if (Lt(entities.Existing.DiscontinueDate, local.Current.Date))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (IsEmpty(import.Starting.UserId))
            {
            }
            else if (!Lt(entities.Existing.UserId, import.Starting.UserId))
            {
            }
            else
            {
              export.Export1.Next();

              continue;
            }

            if (!Lt(entities.Existing.DiscontinueDate, local.Current.Date))
            {
              export.Export1.Update.Active.Flag = "Y";
            }
            else
            {
              export.Export1.Update.Active.Flag = "N";
            }

            export.Export1.Update.ServiceProvider.Assign(entities.Existing);
            export.Export1.Update.Common.SelectChar = "";
            export.Export1.Next();
          }
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadServiceProvider1())
          {
            if (AsChar(export.ActiveOnly.Flag) == 'Y')
            {
              if (Lt(entities.Existing.DiscontinueDate, local.Current.Date))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (IsEmpty(import.Starting.UserId))
            {
            }
            else if (!Lt(entities.Existing.UserId, import.Starting.UserId))
            {
            }
            else
            {
              export.Export1.Next();

              continue;
            }

            if (!Lt(entities.Existing.DiscontinueDate, local.Current.Date))
            {
              export.Export1.Update.Active.Flag = "Y";
            }
            else
            {
              export.Export1.Update.Active.Flag = "N";
            }

            export.Export1.Update.ServiceProvider.Assign(entities.Existing);
            export.Export1.Update.Common.SelectChar = "";
            export.Export1.Next();
          }
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "RETURN":
        if (AsChar(export.HiddenFromSecurity.SelectChar) == 'M')
        {
          // if you linked here from the security system, allow multiple 
          // selections of service provider.
        }
        else if (AsChar(export.HiddenFromSecurity.SelectChar) == 'R')
        {
          // can have up to 7 selecter to go back to srhi
        }
        else
        {
          // if you linked to this procedure from anywhere other than security, 
          // only allow the selection of 1 service provider
          if (local.Count.Count > 1)
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            return;
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.UserId = source.UserId;
    target.LastName = source.LastName;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadServiceProvider1()
  {
    return ReadEach("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "lastName", import.Starting.LastName);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Existing.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Existing.UserId = db.GetString(reader, 1);
        entities.Existing.LastName = db.GetString(reader, 2);
        entities.Existing.FirstName = db.GetString(reader, 3);
        entities.Existing.MiddleInitial = db.GetString(reader, 4);
        entities.Existing.RoleCode = db.GetNullableString(reader, 5);
        entities.Existing.EffectiveDate = db.GetNullableDate(reader, 6);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 7);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProvider2()
  {
    return ReadEach("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "userId", import.Starting.UserId);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Existing.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Existing.UserId = db.GetString(reader, 1);
        entities.Existing.LastName = db.GetString(reader, 2);
        entities.Existing.FirstName = db.GetString(reader, 3);
        entities.Existing.MiddleInitial = db.GetString(reader, 4);
        entities.Existing.RoleCode = db.GetNullableString(reader, 5);
        entities.Existing.EffectiveDate = db.GetNullableDate(reader, 6);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 7);
        entities.Existing.Populated = true;

        return true;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of Active.
      /// </summary>
      [JsonPropertyName("active")]
      public Common Active
      {
        get => active ??= new();
        set => active = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private Common common;
      private ServiceProvider serviceProvider;
      private Common active;
    }

    /// <summary>A ReturnGroup group.</summary>
    [Serializable]
    public class ReturnGroup
    {
      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// A value of HiddenSelected1.
    /// </summary>
    [JsonPropertyName("hiddenSelected1")]
    public ServiceProvider HiddenSelected1
    {
      get => hiddenSelected1 ??= new();
      set => hiddenSelected1 = value;
    }

    /// <summary>
    /// A value of ActiveOnly.
    /// </summary>
    [JsonPropertyName("activeOnly")]
    public Common ActiveOnly
    {
      get => activeOnly ??= new();
      set => activeOnly = value;
    }

    /// <summary>
    /// A value of HiddenFromSecurity.
    /// </summary>
    [JsonPropertyName("hiddenFromSecurity")]
    public Common HiddenFromSecurity
    {
      get => hiddenFromSecurity ??= new();
      set => hiddenFromSecurity = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ServiceProvider Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// Gets a value of Return1.
    /// </summary>
    [JsonIgnore]
    public Array<ReturnGroup> Return1 =>
      return1 ??= new(ReturnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Return1 for json serialization.
    /// </summary>
    [JsonPropertyName("return1")]
    [Computed]
    public IList<ReturnGroup> Return1_Json
    {
      get => return1;
      set => Return1.Assign(value);
    }

    private ServiceProvider hiddenSelected1;
    private Common activeOnly;
    private Common hiddenFromSecurity;
    private Common sortBy;
    private ServiceProvider starting;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private Array<ReturnGroup> return1;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of Active.
      /// </summary>
      [JsonPropertyName("active")]
      public Common Active
      {
        get => active ??= new();
        set => active = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private Common common;
      private ServiceProvider serviceProvider;
      private Common active;
    }

    /// <summary>A HiddenSelectedGroup group.</summary>
    [Serializable]
    public class HiddenSelectedGroup
    {
      /// <summary>
      /// A value of HiddenSelected1.
      /// </summary>
      [JsonPropertyName("hiddenSelected1")]
      public ServiceProvider HiddenSelected1
      {
        get => hiddenSelected1 ??= new();
        set => hiddenSelected1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private ServiceProvider hiddenSelected1;
    }

    /// <summary>A ReturnGroup group.</summary>
    [Serializable]
    public class ReturnGroup
    {
      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// A value of ActiveOnly.
    /// </summary>
    [JsonPropertyName("activeOnly")]
    public Common ActiveOnly
    {
      get => activeOnly ??= new();
      set => activeOnly = value;
    }

    /// <summary>
    /// A value of HiddenFromSecurity.
    /// </summary>
    [JsonPropertyName("hiddenFromSecurity")]
    public Common HiddenFromSecurity
    {
      get => hiddenFromSecurity ??= new();
      set => hiddenFromSecurity = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ServiceProvider Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of HiddenSelected1.
    /// </summary>
    [JsonPropertyName("hiddenSelected1")]
    public ServiceProvider HiddenSelected1
    {
      get => hiddenSelected1 ??= new();
      set => hiddenSelected1 = value;
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

    /// <summary>
    /// Gets a value of HiddenSelected.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSelectedGroup> HiddenSelected => hiddenSelected ??= new(
      HiddenSelectedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenSelected for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    [Computed]
    public IList<HiddenSelectedGroup> HiddenSelected_Json
    {
      get => hiddenSelected;
      set => HiddenSelected.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// Gets a value of Return1.
    /// </summary>
    [JsonIgnore]
    public Array<ReturnGroup> Return1 =>
      return1 ??= new(ReturnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Return1 for json serialization.
    /// </summary>
    [JsonPropertyName("return1")]
    [Computed]
    public IList<ReturnGroup> Return1_Json
    {
      get => return1;
      set => Return1.Assign(value);
    }

    private Common activeOnly;
    private Common hiddenFromSecurity;
    private Common sortBy;
    private ServiceProvider starting;
    private ServiceProvider hiddenSelected1;
    private Array<ExportGroup> export1;
    private Array<HiddenSelectedGroup> hiddenSelected;
    private NextTranInfo hidden;
    private Standard standard;
    private Array<ReturnGroup> return1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    private DateWorkArea current;
    private Common count;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public ServiceProvider Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private ServiceProvider existing;
  }
#endregion
}
