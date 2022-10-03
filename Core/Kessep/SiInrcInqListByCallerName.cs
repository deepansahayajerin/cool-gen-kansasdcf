// Program: SI_INRC_INQ_LIST_BY_CALLER_NAME, ID: 371426418, model: 746.
// Short name: SWEINRCP
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
/// A program: SI_INRC_INQ_LIST_BY_CALLER_NAME.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiInrcInqListByCallerName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INRC_INQ_LIST_BY_CALLER_NAME program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiInrcInqListByCallerName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiInrcInqListByCallerName.
  /// </summary>
  public SiInrcInqListByCallerName(IContext context, Import import,
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
    // ------------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 07-10-95  Ken Evans		Initial Development
    // 01-30-96  Bruce Moore		Retrofit
    // 11/05/96  G. Lofton		Add new security and removed
    // 				old.
    // ------------------------------------------------------------
    // 05/26/99 W.Campbell             Replaced zd exit states.
    // ---------------------------------------------------
    // *********************************************
    // This PRAD will provide a list of all requests
    // for a specified caller name.
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.InformationRequest.Assign(import.InformationRequest);
    export.HiddenInformationRequest.Assign(import.HiddenInformationRequest);

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Gcommon.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.GinformationRequest.Assign(
        import.Group.Item.InformationRequest);
      export.Group.Update.GdateWorkArea.Date =
        import.Group.Item.DateWorkArea.Date;
      export.Group.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      global.Command = "DISPLAY";
    }

    // to validate action level security
    if (Equal(global.Command, "INRD"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(import.InformationRequest.CallerFirstName) || IsEmpty
          (import.InformationRequest.CallerLastName))
        {
          var field1 = GetField(export.InformationRequest, "callerFirstName");

          field1.Error = true;

          var field2 = GetField(export.InformationRequest, "callerLastName");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        UseSiListInquiryByCallerName();
        export.HiddenInformationRequest.Assign(import.InformationRequest);

        break;
      case "INRD":
        local.SelectChar.Count = 0;

        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.Gcommon.SelectChar =
            import.Group.Item.Common.SelectChar;
          export.Group.Update.GinformationRequest.Assign(
            import.Group.Item.InformationRequest);

          switch(AsChar(import.Group.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              export.HiddenInformationRequest.Assign(
                export.Group.Item.GinformationRequest);
              ++local.SelectChar.Count;

              break;
            default:
              var field = GetField(export.Group.Item.Gcommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
              export.Group.Next();

              return;
          }

          export.Group.Next();
        }

        switch(local.SelectChar.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_INQUIRY_MAINTENANCE";

            break;
          default:
            // --------------------------------------
            // 05/26/99 W.Campbell - Replaced zd exit states.
            // --------------------------------------
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "EXIT":
        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (AsChar(import.Group.Item.Common.SelectChar) == 'S')
          {
            export.Menu.ApplicantFirstName =
              import.Group.Item.InformationRequest.CallerFirstName;
            export.Menu.ApplicantLastName =
              import.Group.Item.InformationRequest.CallerLastName ?? "";
            export.Menu.ApplicantMiddleInitial =
              import.Group.Item.InformationRequest.CallerMiddleInitial;
            export.Menu.Number = import.Group.Item.InformationRequest.Number;

            break;
          }
        }

        if (export.Menu.Number == 0)
        {
          export.Menu.ApplicantFirstName =
            export.InformationRequest.CallerFirstName;
          export.Menu.ApplicantLastName =
            export.InformationRequest.CallerLastName ?? "";
          export.Menu.ApplicantMiddleInitial =
            export.InformationRequest.CallerMiddleInitial;
        }

        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "NEXT":
        // *********************************************
        // The following exit state is used because of
        // the wrap to the first page feature (when at
        // last page) of implicit scrolling.
        // *********************************************
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveGroup(SiListInquiryByCallerName.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.Gcommon.SelectChar = source.Gcommon.SelectChar;
    MoveInformationRequest(source.GinformationRequest,
      target.GinformationRequest);
    target.GdateWorkArea.Date = source.GdateWorkArea.Date;
  }

  private static void MoveInformationRequest(InformationRequest source,
    InformationRequest target)
  {
    target.Number = source.Number;
    target.CallerLastName = source.CallerLastName;
    target.CallerFirstName = source.CallerFirstName;
    target.CallerMiddleInitial = source.CallerMiddleInitial;
    target.Type1 = source.Type1;
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

  private void UseSiListInquiryByCallerName()
  {
    var useImport = new SiListInquiryByCallerName.Import();
    var useExport = new SiListInquiryByCallerName.Export();

    useImport.InformationRequest.Assign(export.InformationRequest);

    Call(SiListInquiryByCallerName.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
      /// A value of InformationRequest.
      /// </summary>
      [JsonPropertyName("informationRequest")]
      public InformationRequest InformationRequest
      {
        get => informationRequest ??= new();
        set => informationRequest = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private InformationRequest informationRequest;
      private DateWorkArea dateWorkArea;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of HiddenInformationRequest.
    /// </summary>
    [JsonPropertyName("hiddenInformationRequest")]
    public InformationRequest HiddenInformationRequest
    {
      get => hiddenInformationRequest ??= new();
      set => hiddenInformationRequest = value;
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

    private InformationRequest informationRequest;
    private InformationRequest hiddenInformationRequest;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GinformationRequest.
      /// </summary>
      [JsonPropertyName("ginformationRequest")]
      public InformationRequest GinformationRequest
      {
        get => ginformationRequest ??= new();
        set => ginformationRequest = value;
      }

      /// <summary>
      /// A value of GdateWorkArea.
      /// </summary>
      [JsonPropertyName("gdateWorkArea")]
      public DateWorkArea GdateWorkArea
      {
        get => gdateWorkArea ??= new();
        set => gdateWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common gcommon;
      private InformationRequest ginformationRequest;
      private DateWorkArea gdateWorkArea;
    }

    /// <summary>
    /// A value of Menu.
    /// </summary>
    [JsonPropertyName("menu")]
    public InformationRequest Menu
    {
      get => menu ??= new();
      set => menu = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of HiddenInformationRequest.
    /// </summary>
    [JsonPropertyName("hiddenInformationRequest")]
    public InformationRequest HiddenInformationRequest
    {
      get => hiddenInformationRequest ??= new();
      set => hiddenInformationRequest = value;
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

    private InformationRequest menu;
    private InformationRequest informationRequest;
    private InformationRequest hiddenInformationRequest;
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
    /// A value of SelectChar.
    /// </summary>
    [JsonPropertyName("selectChar")]
    public Common SelectChar
    {
      get => selectChar ??= new();
      set => selectChar = value;
    }

    private Common selectChar;
  }
#endregion
}
