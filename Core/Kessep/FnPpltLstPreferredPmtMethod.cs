// Program: FN_PPLT_LST_PREFERRED_PMT_METHOD, ID: 371880804, model: 746.
// Short name: SWEPPLTP
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
/// A program: FN_PPLT_LST_PREFERRED_PMT_METHOD.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPpltLstPreferredPmtMethod: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PPLT_LST_PREFERRED_PMT_METHOD program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPpltLstPreferredPmtMethod(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPpltLstPreferredPmtMethod.
  /// </summary>
  public FnPpltLstPreferredPmtMethod(IContext context, Import import,
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
    // **************************************************
    // DATE		WHO		DESCRIPTION
    // 05/07/97	A Samuels MTW	Development
    // 12/30/98        N Engoor        Maintenance
    // **************************************************
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";

    // Move all IMPORTs to EXPORTs.
    export.HiddenCsePersonsWorkSet.Number =
      import.HiddenCsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet(import.SearchCsePersonsWorkSet,
      export.SearchCsePersonsWorkSet);
    export.SearchPersonPreferredPaymentMethod.EffectiveDate =
      import.SearchPersonPreferredPaymentMethod.EffectiveDate;
    export.CsePerson.PromptField = import.CsePerson.PromptField;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.Common.Count = 0;
    local.Error.Count = 0;

    if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETNAME"))
    {
      if (IsEmpty(import.SearchCsePersonsWorkSet.Number))
      {
        export.SearchCsePersonsWorkSet.Number =
          import.HiddenCsePersonsWorkSet.Number;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY") && IsEmpty
      (export.SearchCsePersonsWorkSet.Number))
    {
      var field = GetField(export.SearchCsePersonsWorkSet, "number");

      field.Error = true;

      export.SearchCsePersonsWorkSet.FormattedName = "";
      ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

      return;
    }

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      MovePaymentMethodType(import.Group.Item.PaymentMethodType,
        export.Group.Update.PaymentMethodType);
      export.Group.Update.PersonPreferredPaymentMethod.Assign(
        import.Group.Item.PersonPreferredPaymentMethod);

      if (!IsEmpty(export.Group.Item.Common.SelectChar))
      {
        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          ++local.Common.Count;
          MovePaymentMethodType(export.Group.Item.PaymentMethodType,
            export.ToTranPaymentMethodType);
          export.ToTranPersonPreferredPaymentMethod.Assign(
            export.Group.Item.PersonPreferredPaymentMethod);
        }
        else
        {
          ++local.Error.Count;
        }
      }

      export.Group.Next();
    }

    if (local.Common.Count > 1)
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
      }
    }

    if (local.Error.Count > 0)
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.Common.SelectChar) != 'S' && !
          IsEmpty(export.Group.Item.Common.SelectChar))
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "PPMT"))
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

    // ****************************************************
    // Prompts are only valid with PF4 List.
    // ****************************************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // *****************************************************
        // Search fields must be populated.
        // *****************************************************
        if (IsEmpty(export.SearchCsePersonsWorkSet.Number))
        {
          export.SearchCsePersonsWorkSet.FormattedName = "";
          export.CsePerson.PromptField = "+";

          var field = GetField(export.SearchCsePersonsWorkSet, "number");

          field.Error = true;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.PaymentMethodType.Code = "";
            export.Group.Update.PersonPreferredPaymentMethod.DfiAccountNumber =
              "";
            export.Group.Update.PersonPreferredPaymentMethod.LastUpdateBy = "";
            export.Group.Update.PersonPreferredPaymentMethod.AbaRoutingNumber =
              0;
            export.Group.Update.PersonPreferredPaymentMethod.
              SystemGeneratedIdentifier = 0;
            export.Group.Update.PersonPreferredPaymentMethod.DiscontinueDate =
              null;
            export.Group.Update.PersonPreferredPaymentMethod.EffectiveDate =
              null;
          }

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        UseSiReadCsePerson();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCsePersonsWorkSet(local.ReturnFrom, export.SearchCsePersonsWorkSet);
            
          export.HiddenCsePersonsWorkSet.Number = local.ReturnFrom.Number;

          if (Equal(export.SearchPersonPreferredPaymentMethod.EffectiveDate,
            null))
          {
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadPersonPreferredPaymentMethodPaymentMethodType2())
              
            {
              export.Group.Update.PersonPreferredPaymentMethod.Assign(
                entities.PersonPreferredPaymentMethod);
              MovePaymentMethodType(entities.PaymentMethodType,
                export.Group.Update.PaymentMethodType);
              local.Routing.Text10 =
                NumberToString(export.Group.Item.PersonPreferredPaymentMethod.
                  AbaRoutingNumber.GetValueOrDefault(), 7, 9);
              export.Group.Update.PersonPreferredPaymentMethod.
                AbaRoutingNumber = StringToNumber(local.Routing.Text10);

              if (Equal(export.Group.Item.PersonPreferredPaymentMethod.
                DiscontinueDate, new DateTime(2099, 12, 31)))
              {
                export.Group.Update.PersonPreferredPaymentMethod.
                  DiscontinueDate = null;
              }

              export.Group.Next();
            }
          }
          else
          {
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadPersonPreferredPaymentMethodPaymentMethodType1())
              
            {
              export.Group.Update.PersonPreferredPaymentMethod.Assign(
                entities.PersonPreferredPaymentMethod);
              MovePaymentMethodType(entities.PaymentMethodType,
                export.Group.Update.PaymentMethodType);
              local.Routing.Text10 =
                NumberToString(export.Group.Item.PersonPreferredPaymentMethod.
                  AbaRoutingNumber.GetValueOrDefault(), 7, 9);
              export.Group.Update.PersonPreferredPaymentMethod.
                AbaRoutingNumber = StringToNumber(local.Routing.Text10);

              if (Equal(export.Group.Item.PersonPreferredPaymentMethod.
                DiscontinueDate, new DateTime(2099, 12, 31)))
              {
                export.Group.Update.PersonPreferredPaymentMethod.
                  DiscontinueDate = null;
              }

              export.Group.Next();
            }
          }

          if (export.Group.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
          }
          else if (export.Group.IsFull)
          {
            ExitState = "FN0000_GROUP_VIEW_OVERFLOW";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }
        else
        {
          var field = GetField(export.SearchCsePersonsWorkSet, "number");

          field.Error = true;
        }

        export.CsePerson.PromptField = "+";

        break;
      case "LIST":
        if (!IsEmpty(import.CsePerson.PromptField))
        {
          if (AsChar(import.CsePerson.PromptField) == 'S')
          {
            export.CsePerson.PromptField = "";
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }
          else
          {
            var field = GetField(export.CsePerson, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
          }
        }
        else
        {
          var field = GetField(export.CsePerson, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "RETURN":
        export.Common.Flag = "Y";
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "PPMT":
        if (local.Common.Count == 0 && local.Error.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else
        {
          export.Common.Flag = "Y";
          ExitState = "ECO_XFR_TO_MTN_PREF_PMNT_METHOD";
        }

        break;
      default:
        if (IsEmpty(global.Command))
        {
          return;
        }

        export.CsePerson.PromptField = "+";
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private static void MovePaymentMethodType(PaymentMethodType source,
    PaymentMethodType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.SearchCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadPersonPreferredPaymentMethodPaymentMethodType1()
  {
    return ReadEach("ReadPersonPreferredPaymentMethodPaymentMethodType1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.SearchPersonPreferredPaymentMethod.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "cspPNumber", export.SearchCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.AbaRoutingNumber =
          db.GetNullableInt64(reader, 2);
        entities.PersonPreferredPaymentMethod.DfiAccountNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 4);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonPreferredPaymentMethod.LastUpdateBy =
          db.GetNullableString(reader, 6);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 7);
        entities.PaymentMethodType.Code = db.GetString(reader, 8);
        entities.PaymentMethodType.Populated = true;
        entities.PersonPreferredPaymentMethod.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonPreferredPaymentMethodPaymentMethodType2()
  {
    return ReadEach("ReadPersonPreferredPaymentMethodPaymentMethodType2",
      (db, command) =>
      {
        db.SetString(
          command, "cspPNumber", export.SearchCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.AbaRoutingNumber =
          db.GetNullableInt64(reader, 2);
        entities.PersonPreferredPaymentMethod.DfiAccountNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 4);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonPreferredPaymentMethod.LastUpdateBy =
          db.GetNullableString(reader, 6);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 7);
        entities.PaymentMethodType.Code = db.GetString(reader, 8);
        entities.PaymentMethodType.Populated = true;
        entities.PersonPreferredPaymentMethod.Populated = true;

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of PaymentMethodType.
      /// </summary>
      [JsonPropertyName("paymentMethodType")]
      public PaymentMethodType PaymentMethodType
      {
        get => paymentMethodType ??= new();
        set => paymentMethodType = value;
      }

      /// <summary>
      /// A value of PersonPreferredPaymentMethod.
      /// </summary>
      [JsonPropertyName("personPreferredPaymentMethod")]
      public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
      {
        get => personPreferredPaymentMethod ??= new();
        set => personPreferredPaymentMethod = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 78;

      private Common common;
      private PaymentMethodType paymentMethodType;
      private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of SearchCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchCsePersonsWorkSet
    {
      get => searchCsePersonsWorkSet ??= new();
      set => searchCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchPersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("searchPersonPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod SearchPersonPreferredPaymentMethod
    {
      get => searchPersonPreferredPaymentMethod ??= new();
      set => searchPersonPreferredPaymentMethod = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Standard csePerson;
    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private PersonPreferredPaymentMethod searchPersonPreferredPaymentMethod;
    private Array<GroupGroup> group;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of PaymentMethodType.
      /// </summary>
      [JsonPropertyName("paymentMethodType")]
      public PaymentMethodType PaymentMethodType
      {
        get => paymentMethodType ??= new();
        set => paymentMethodType = value;
      }

      /// <summary>
      /// A value of PersonPreferredPaymentMethod.
      /// </summary>
      [JsonPropertyName("personPreferredPaymentMethod")]
      public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
      {
        get => personPreferredPaymentMethod ??= new();
        set => personPreferredPaymentMethod = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 78;

      private Common common;
      private PaymentMethodType paymentMethodType;
      private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ToTranPaymentMethodType.
    /// </summary>
    [JsonPropertyName("toTranPaymentMethodType")]
    public PaymentMethodType ToTranPaymentMethodType
    {
      get => toTranPaymentMethodType ??= new();
      set => toTranPaymentMethodType = value;
    }

    /// <summary>
    /// A value of ToTranPersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("toTranPersonPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod ToTranPersonPreferredPaymentMethod
    {
      get => toTranPersonPreferredPaymentMethod ??= new();
      set => toTranPersonPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of SearchCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchCsePersonsWorkSet
    {
      get => searchCsePersonsWorkSet ??= new();
      set => searchCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchPersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("searchPersonPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod SearchPersonPreferredPaymentMethod
    {
      get => searchPersonPreferredPaymentMethod ??= new();
      set => searchPersonPreferredPaymentMethod = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Common common;
    private Standard csePerson;
    private PaymentMethodType toTranPaymentMethodType;
    private PersonPreferredPaymentMethod toTranPersonPreferredPaymentMethod;
    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private PersonPreferredPaymentMethod searchPersonPreferredPaymentMethod;
    private Array<GroupGroup> group;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Routing.
    /// </summary>
    [JsonPropertyName("routing")]
    public TextWorkArea Routing
    {
      get => routing ??= new();
      set => routing = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
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
    /// A value of ReturnFrom.
    /// </summary>
    [JsonPropertyName("returnFrom")]
    public CsePersonsWorkSet ReturnFrom
    {
      get => returnFrom ??= new();
      set => returnFrom = value;
    }

    private TextWorkArea routing;
    private Common error;
    private Common common;
    private CsePersonsWorkSet returnFrom;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    private CsePerson csePerson;
    private PaymentMethodType paymentMethodType;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
  }
#endregion
}
