// Program: OE_VENL_LIST_VENDOR, ID: 371796767, model: 746.
// Short name: SWEVENLP
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
/// A program: OE_VENL_LIST_VENDOR.
/// </para>
/// <para>
/// This procedure lists vendor by alpha name order and it's located city and 
/// state up to 12 lines on a screen.  Scrolls implicitly.  Listing can start
/// from certain city or state.  Control will flow to Maintain-Vendor procedure
/// for selected vendor to update.
/// Processing:  If all parameter fields entered as blank, diagram assums to 
/// list by vendor name on ascending alpha order from beginning of the table.
/// If start vendor name entered, screen will display vendor from that vendor
/// name on alpha order.
/// If city and state parameter is specified, diagram will perform to list 
/// vendor in that city of the state by alpha Vendor Name order.
/// If state paremeter is specified, diagram will list vendor in that state by 
/// alpha order.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeVenlListVendor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_VENL_LIST_VENDOR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeVenlListVendor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeVenlListVendor.
  /// </summary>
  public OeVenlListVendor(IContext context, Import import, Export export):
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
    //      SYSTEM: KESSEP
    //      MODULE:
    //      MODULE TYPE:
    //      DESCRIPTION:
    //      This procedure lists vendor name and it's located city and state up 
    // to 12 lines on a screen.  Can start to list from certain city or from
    // certain state or certain vendor name.  Selected vendor will display
    // individual vendor detail listing for updating.
    // PROCESSING:
    // If starting vendor name entered, logic process to display starting from 
    // that vendor name.
    // If all starting vendor name, city, state parameters are spaces, logic 
    // process to display by vendor name on alpha order.
    // If start city and state are entered, logic process to display vendors in 
    // that city of that state. When city parameter is entered, state parameter
    // also must be entered.
    // If only starting state is entered, logic process to display vendor 
    // located in that state.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    //       VENDOR             -R-
    //       ADDRESS            -R-
    // MAINTENANCE LOG
    // AUTHOR         DATE   CHG REQ#    DESCRIPTION
    // Grace Kim      	1/23/95		Initial Code
    // Sherri Newman  	3/20/96         Retrofit
    // R. Marchman	11/19/96    	Add new security and next tran.
    // SHERAZ MALIK	4/29/97		CHANGE CURRENT_DATE
    // *********************************************
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Hidden.Assign(import.Hidden);

    export.Exp.Index = 0;
    export.Exp.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Exp.IsFull)
      {
        break;
      }

      export.Exp.Update.Vendor.Assign(import.Import1.Item.Vendor);
      MoveVendorAddress(import.Import1.Item.VendorAddress,
        export.Exp.Update.VendorAddress);
      export.Exp.Update.Selection1.OneChar =
        import.Import1.Item.Selection1.OneChar;
      export.Exp.Next();
    }

    MoveVendor2(import.StartVendor, export.StartVendor);
    MoveVendorAddress(import.StartVendorAddress, export.StartVendorAddress);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "STATE") || Equal(global.Command, "CITY") || Equal
      (global.Command, "NAME") || Equal(global.Command, "VEND"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        if (!IsEmpty(import.StartVendorAddress.State))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = import.StartVendorAddress.State ?? Spaces
            (10);
          local.ValidCode.Flag = "";
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            var field = GetField(export.StartVendorAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            return;
          }
        }

        // *********************************************
        // When no parameter entered, diagram assumes to list by vendor name on 
        // alpha order from the beginning of the table.
        // *********************************************
        if (!IsEmpty(import.StartVendorAddress.City))
        {
          local.Common.Command = "CITY";
        }
        else if (!IsEmpty(import.StartVendorAddress.State))
        {
          local.Common.Command = "STATE";
        }

        if (!IsEmpty(import.StartVendor.Name) || IsEmpty
          (import.StartVendor.Name) && IsEmpty
          (import.StartVendorAddress.City) && IsEmpty
          (import.StartVendorAddress.State))
        {
          local.Common.Command = "NAME";
        }

        switch(TrimEnd(local.Common.Command))
        {
          case "NAME":
            export.Exp.Index = 0;
            export.Exp.Clear();

            foreach(var item in ReadVendorVendorAddress3())
            {
              MoveVendorAddress(entities.ExistingVendorAddress,
                export.Exp.Update.VendorAddress);
              export.Exp.Update.Vendor.Assign(entities.ExistingVendor);
              export.Exp.Update.Selection1.OneChar = "";
              export.Exp.Next();
            }

            break;
          case "CITY":
            export.StartVendor.Name = "";

            if (!IsEmpty(import.StartVendorAddress.State))
            {
              export.Exp.Index = 0;
              export.Exp.Clear();

              foreach(var item in ReadVendorVendorAddress1())
              {
                MoveVendorAddress(entities.ExistingVendorAddress,
                  export.Exp.Update.VendorAddress);
                export.Exp.Update.Vendor.Assign(entities.ExistingVendor);
                export.Exp.Update.Selection1.OneChar = "";
                export.Exp.Next();
              }
            }
            else
            {
              export.Exp.Index = 0;
              export.Exp.Clear();

              foreach(var item in ReadVendorVendorAddress2())
              {
                MoveVendorAddress(entities.ExistingVendorAddress,
                  export.Exp.Update.VendorAddress);
                export.Exp.Update.Vendor.Assign(entities.ExistingVendor);
                export.Exp.Update.Selection1.OneChar = "";
                export.Exp.Next();
              }
            }

            break;
          case "STATE":
            export.Exp.Index = 0;
            export.Exp.Clear();

            foreach(var item in ReadVendorVendorAddress4())
            {
              MoveVendorAddress(entities.ExistingVendorAddress,
                export.Exp.Update.VendorAddress);
              export.Exp.Update.Vendor.Assign(entities.ExistingVendor);
              export.Exp.Update.Selection1.OneChar = "";
              export.Exp.Next();
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_COMMAND";

            break;
        }

        if (export.Exp.IsEmpty)
        {
          ExitState = "FN0000_VENDOR_NF";

          if (!IsEmpty(export.StartVendor.Name))
          {
            var field = GetField(export.StartVendor, "name");

            field.Error = true;
          }

          if (!IsEmpty(export.StartVendorAddress.City))
          {
            var field = GetField(export.StartVendorAddress, "city");

            field.Error = true;
          }

          if (!IsEmpty(export.StartVendorAddress.State))
          {
            var field = GetField(export.StartVendorAddress, "state");

            field.Error = true;
          }
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "PREV":
        ExitState = "OE0067_SCROLLED_BEY_FIRST_PAGE";

        break;
      case "NEXT":
        ExitState = "OE0068_SCROLLED_BEY_LAST_PAGE";

        break;
      case "RETURN":
        local.Selected.Count = 0;

        for(export.Exp.Index = 0; export.Exp.Index < export.Exp.Count; ++
          export.Exp.Index)
        {
          if (!IsEmpty(export.Exp.Item.Selection1.OneChar))
          {
            if (AsChar(export.Exp.Item.Selection1.OneChar) == 'S')
            {
              MoveVendor1(export.Exp.Item.Vendor, export.Selected);
            }
            else
            {
              var field = GetField(export.Exp.Item.Selection1, "oneChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            ++local.Selected.Count;
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (local.Selected.Count > 1)
          {
            ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";

            for(export.Exp.Index = 0; export.Exp.Index < export.Exp.Count; ++
              export.Exp.Index)
            {
              if (!IsEmpty(export.Exp.Item.Selection1.OneChar))
              {
                var field = GetField(export.Exp.Item.Selection1, "oneChar");

                field.Error = true;
              }
            }
          }
          else
          {
            ExitState = "ACO_NE0000_RETURN";
          }
        }

        break;
      case "VEND":
        local.Selected.Count = 0;

        for(export.Exp.Index = 0; export.Exp.Index < export.Exp.Count; ++
          export.Exp.Index)
        {
          if (!IsEmpty(export.Exp.Item.Selection1.OneChar))
          {
            if (AsChar(export.Exp.Item.Selection1.OneChar) == 'S')
            {
              MoveVendor1(export.Exp.Item.Vendor, export.Selected);
            }
            else
            {
              var field = GetField(export.Exp.Item.Selection1, "oneChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            ++local.Selected.Count;
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (local.Selected.Count == 0)
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            for(export.Exp.Index = 0; export.Exp.Index < export.Exp.Count; ++
              export.Exp.Index)
            {
              var field = GetField(export.Exp.Item.Selection1, "oneChar");

              field.Error = true;

              return;
            }
          }
          else if (local.Selected.Count > 1)
          {
            for(export.Exp.Index = 0; export.Exp.Index < export.Exp.Count; ++
              export.Exp.Index)
            {
              if (!IsEmpty(export.Exp.Item.Selection1.OneChar))
              {
                var field = GetField(export.Exp.Item.Selection1, "oneChar");

                field.Error = true;

                ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";
              }
            }
          }
          else
          {
            ExitState = "ECO_XFR_TO_MAINTAIN_VENDOR";
          }
        }

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

  private static void MoveVendor1(Vendor source, Vendor target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
    target.Number = source.Number;
    target.ServiceTypeCode = source.ServiceTypeCode;
  }

  private static void MoveVendor2(Vendor source, Vendor target)
  {
    target.Name = source.Name;
    target.Number = source.Number;
  }

  private static void MoveVendorAddress(VendorAddress source,
    VendorAddress target)
  {
    target.City = source.City;
    target.State = source.State;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
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

  private IEnumerable<bool> ReadVendorVendorAddress1()
  {
    return ReadEach("ReadVendorVendorAddress1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "city", import.StartVendorAddress.City ?? "");
        db.SetNullableString(
          command, "state", import.StartVendorAddress.State ?? "");
      },
      (db, reader) =>
      {
        if (export.Exp.IsFull)
        {
          return false;
        }

        entities.ExistingVendor.Identifier = db.GetInt32(reader, 0);
        entities.ExistingVendorAddress.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingVendor.Name = db.GetString(reader, 1);
        entities.ExistingVendor.Number = db.GetNullableString(reader, 2);
        entities.ExistingVendor.ServiceTypeCode =
          db.GetNullableString(reader, 3);
        entities.ExistingVendorAddress.EffectiveDate = db.GetDate(reader, 4);
        entities.ExistingVendorAddress.ExpiryDate = db.GetDate(reader, 5);
        entities.ExistingVendorAddress.City = db.GetNullableString(reader, 6);
        entities.ExistingVendorAddress.State = db.GetNullableString(reader, 7);
        entities.ExistingVendorAddress.Populated = true;
        entities.ExistingVendor.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadVendorVendorAddress2()
  {
    return ReadEach("ReadVendorVendorAddress2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "city", import.StartVendorAddress.City ?? "");
      },
      (db, reader) =>
      {
        if (export.Exp.IsFull)
        {
          return false;
        }

        entities.ExistingVendor.Identifier = db.GetInt32(reader, 0);
        entities.ExistingVendorAddress.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingVendor.Name = db.GetString(reader, 1);
        entities.ExistingVendor.Number = db.GetNullableString(reader, 2);
        entities.ExistingVendor.ServiceTypeCode =
          db.GetNullableString(reader, 3);
        entities.ExistingVendorAddress.EffectiveDate = db.GetDate(reader, 4);
        entities.ExistingVendorAddress.ExpiryDate = db.GetDate(reader, 5);
        entities.ExistingVendorAddress.City = db.GetNullableString(reader, 6);
        entities.ExistingVendorAddress.State = db.GetNullableString(reader, 7);
        entities.ExistingVendorAddress.Populated = true;
        entities.ExistingVendor.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadVendorVendorAddress3()
  {
    return ReadEach("ReadVendorVendorAddress3",
      (db, command) =>
      {
        db.SetString(command, "name", import.StartVendor.Name);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Exp.IsFull)
        {
          return false;
        }

        entities.ExistingVendor.Identifier = db.GetInt32(reader, 0);
        entities.ExistingVendorAddress.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingVendor.Name = db.GetString(reader, 1);
        entities.ExistingVendor.Number = db.GetNullableString(reader, 2);
        entities.ExistingVendor.ServiceTypeCode =
          db.GetNullableString(reader, 3);
        entities.ExistingVendorAddress.EffectiveDate = db.GetDate(reader, 4);
        entities.ExistingVendorAddress.ExpiryDate = db.GetDate(reader, 5);
        entities.ExistingVendorAddress.City = db.GetNullableString(reader, 6);
        entities.ExistingVendorAddress.State = db.GetNullableString(reader, 7);
        entities.ExistingVendorAddress.Populated = true;
        entities.ExistingVendor.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadVendorVendorAddress4()
  {
    return ReadEach("ReadVendorVendorAddress4",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "state", import.StartVendorAddress.State ?? "");
      },
      (db, reader) =>
      {
        if (export.Exp.IsFull)
        {
          return false;
        }

        entities.ExistingVendor.Identifier = db.GetInt32(reader, 0);
        entities.ExistingVendorAddress.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingVendor.Name = db.GetString(reader, 1);
        entities.ExistingVendor.Number = db.GetNullableString(reader, 2);
        entities.ExistingVendor.ServiceTypeCode =
          db.GetNullableString(reader, 3);
        entities.ExistingVendorAddress.EffectiveDate = db.GetDate(reader, 4);
        entities.ExistingVendorAddress.ExpiryDate = db.GetDate(reader, 5);
        entities.ExistingVendorAddress.City = db.GetNullableString(reader, 6);
        entities.ExistingVendorAddress.State = db.GetNullableString(reader, 7);
        entities.ExistingVendorAddress.Populated = true;
        entities.ExistingVendor.Populated = true;

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
      /// A value of Vendor.
      /// </summary>
      [JsonPropertyName("vendor")]
      public Vendor Vendor
      {
        get => vendor ??= new();
        set => vendor = value;
      }

      /// <summary>
      /// A value of VendorAddress.
      /// </summary>
      [JsonPropertyName("vendorAddress")]
      public VendorAddress VendorAddress
      {
        get => vendorAddress ??= new();
        set => vendorAddress = value;
      }

      /// <summary>
      /// A value of Selection1.
      /// </summary>
      [JsonPropertyName("selection1")]
      public Standard Selection1
      {
        get => selection1 ??= new();
        set => selection1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Vendor vendor;
      private VendorAddress vendorAddress;
      private Standard selection1;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Vendor Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of StartVendorAddress.
    /// </summary>
    [JsonPropertyName("startVendorAddress")]
    public VendorAddress StartVendorAddress
    {
      get => startVendorAddress ??= new();
      set => startVendorAddress = value;
    }

    /// <summary>
    /// A value of StartVendor.
    /// </summary>
    [JsonPropertyName("startVendor")]
    public Vendor StartVendor
    {
      get => startVendor ??= new();
      set => startVendor = value;
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

    private Standard standard;
    private Vendor selected;
    private VendorAddress startVendorAddress;
    private Vendor startVendor;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExpGroup group.</summary>
    [Serializable]
    public class ExpGroup
    {
      /// <summary>
      /// A value of VendorAddress.
      /// </summary>
      [JsonPropertyName("vendorAddress")]
      public VendorAddress VendorAddress
      {
        get => vendorAddress ??= new();
        set => vendorAddress = value;
      }

      /// <summary>
      /// A value of Vendor.
      /// </summary>
      [JsonPropertyName("vendor")]
      public Vendor Vendor
      {
        get => vendor ??= new();
        set => vendor = value;
      }

      /// <summary>
      /// A value of Selection1.
      /// </summary>
      [JsonPropertyName("selection1")]
      public Standard Selection1
      {
        get => selection1 ??= new();
        set => selection1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private VendorAddress vendorAddress;
      private Vendor vendor;
      private Standard selection1;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Vendor Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of StartVendorAddress.
    /// </summary>
    [JsonPropertyName("startVendorAddress")]
    public VendorAddress StartVendorAddress
    {
      get => startVendorAddress ??= new();
      set => startVendorAddress = value;
    }

    /// <summary>
    /// A value of StartVendor.
    /// </summary>
    [JsonPropertyName("startVendor")]
    public Vendor StartVendor
    {
      get => startVendor ??= new();
      set => startVendor = value;
    }

    /// <summary>
    /// Gets a value of Exp.
    /// </summary>
    [JsonIgnore]
    public Array<ExpGroup> Exp => exp ??= new(ExpGroup.Capacity);

    /// <summary>
    /// Gets a value of Exp for json serialization.
    /// </summary>
    [JsonPropertyName("exp")]
    [Computed]
    public IList<ExpGroup> Exp_Json
    {
      get => exp;
      set => Exp.Assign(value);
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

    private Standard standard;
    private Vendor selected;
    private VendorAddress startVendorAddress;
    private Vendor startVendor;
    private Array<ExpGroup> exp;
    private NextTranInfo hidden;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    private DateWorkArea current;
    private Common validCode;
    private Common common;
    private Common selected;
    private Code code;
    private CodeValue codeValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingVendorAddress.
    /// </summary>
    [JsonPropertyName("existingVendorAddress")]
    public VendorAddress ExistingVendorAddress
    {
      get => existingVendorAddress ??= new();
      set => existingVendorAddress = value;
    }

    /// <summary>
    /// A value of ExistingVendor.
    /// </summary>
    [JsonPropertyName("existingVendor")]
    public Vendor ExistingVendor
    {
      get => existingVendor ??= new();
      set => existingVendor = value;
    }

    private VendorAddress existingVendorAddress;
    private Vendor existingVendor;
  }
#endregion
}
