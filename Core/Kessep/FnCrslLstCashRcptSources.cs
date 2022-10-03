// Program: FN_CRSL_LST_CASH_RCPT_SOURCES, ID: 371785004, model: 746.
// Short name: SWECRSLP
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
/// A program: FN_CRSL_LST_CASH_RCPT_SOURCES.
/// </para>
/// <para>
/// RESP: CASHMGMT
/// This procedure will list CASH RECEIPT SOURCE TYPES for selection.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrslLstCashRcptSources: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRSL_LST_CASH_RCPT_SOURCES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrslLstCashRcptSources(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrslLstCashRcptSources.
  /// </summary>
  public FnCrslLstCashRcptSources(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------------
    // Date 	  	Developer Name	Description
    // 02/03/96	Holly Kennedy	Retrofits
    // 02/21/96	Holly Kennedy	Flow returning to Cash Management.Menu cannot be 
    // an auto flow.  A flow also exists to the Cash Management Admin Menu.  A
    // flag is passed to this screen from the Admin Menu and is evaluated when
    // PF3 is pressed to determine which menu to flow to.
    // 12/13/96        R. Marchman	Add new security/next tran
    // 02/07/97        R. Welborn      Numerous Fixes
    // 03/20/97	JF Caillouet	Display/Flow fixes
    // 04/29/97        J. Howard       CURRENT_DATE fix
    // 06/15/97	T.O.Redmond	Add Court/Interface Indicator
    // 07/01/97	JF.Caillouet    Protect/blank out Disc.Date
    // 
    // 10/13/98        S. Newman       Increased group viewsize from 150 to 208.
    // Added Successful display message.  Revised selection character logic to
    // only accept a 'S'.  Revised multiple selection logic, reset counter,
    // added Make statements.  Also, removed '*' from selection field in case of
    // display.
    // 
    // 2/1/99          S. Newman       Added FIPS Code to the screen & Export
    // Out views
    // ------------------------------------------------------------------
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // Move all IMPORTs to EXPORTs.
    export.SearchCashReceiptSourceType.Code =
      import.SearchCashReceiptSourceType.Code;
    export.ShowHistory.Flag = import.ShowHistory.Flag;
    export.SearchServiceProvider.UserId = import.SearchServiceProvider.UserId;
    export.FromAdminMenu.Flag = import.FromAdminMenu.Flag;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // *****
    // Next Tran Logic
    // *****
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // No valid values to set
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

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // to validate action level security
    if (Equal(global.Command, "CRSM") || Equal(global.Command, "RTFRMLNK") || Equal
      (global.Command, "ENTER"))
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

    // Selection test
    // Check to see if a selection has been made.  Only one selection will be 
    // processed at a time.  If more than one cash receipt source is selected,
    // an error will occur.
    local.SelectionCounter.Count = 0;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.CashReceiptSourceType.Assign(
        import.Import1.Item.CashReceiptSourceType);

      if (Equal(global.Command, "RTFRMLNK") && AsChar
        (import.Import1.Item.Common.SelectChar) == 'S')
      {
        export.Export1.Update.Common.SelectChar = "*";
      }
      else
      {
        export.Export1.Update.Common.SelectChar =
          import.Import1.Item.Common.SelectChar;

        if (AsChar(import.Import1.Item.Common.SelectChar) == 'S')
        {
          MoveCashReceiptSourceType(import.Import1.Item.CashReceiptSourceType,
            export.FlowSelection);
          export.HiddenSelection.Assign(
            import.Import1.Item.CashReceiptSourceType);
          ++local.SelectionCounter.Count;
        }
        else if (IsEmpty(import.Import1.Item.Common.SelectChar) || AsChar
          (import.Import1.Item.Common.SelectChar) == '*')
        {
          // Do nothing
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }
      }

      export.Export1.Next();
    }

    if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE1"))
    {
      return;
    }

    if (local.SelectionCounter.Count > 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }
      }

      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == '*')
        {
          export.Export1.Update.Common.SelectChar = "";
        }
      }
    }

    // *************************
    // Main CASE OF COMMAND.
    // *************************
    switch(TrimEnd(global.Command))
    {
      case "CRSM":
        ExitState = "ECO_XFR_TO_MTN_CASH_RECPT_SRC";

        break;
      case "DISPLAY":
        if (local.SelectionCounter.Count > 0)
        {
          ExitState = "NO_SELECT_WITH_DISPLAY_OPTION";

          return;
        }

        // READ EACH for selection list.
        if (!IsEmpty(import.SearchServiceProvider.UserId))
        {
          if (!ReadServiceProvider())
          {
            var field = GetField(export.SearchServiceProvider, "userId");

            field.Error = true;

            ExitState = "SERVICE_PROVIDER_NF";

            return;
          }

          if (AsChar(import.ShowHistory.Flag) == 'Y')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadCashReceiptSourceTypeReceiptResearchAssignment2())
              
            {
              export.Export1.Update.CashReceiptSourceType.Assign(
                entities.CashReceiptSourceType);
              local.CashSourceTypeDisc.Date =
                export.Export1.Item.CashReceiptSourceType.DiscontinueDate;
              UseCabSetMaximumDiscontinueDate();
              export.Export1.Update.CashReceiptSourceType.DiscontinueDate =
                local.CashSourceTypeDisc.Date;
              export.Export1.Next();
            }

            if (export.Export1.IsEmpty)
            {
              ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

              return;
            }

            if (export.Export1.IsFull)
            {
              ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

              return;
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
          else if (AsChar(import.ShowHistory.Flag) != 'Y')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadCashReceiptSourceTypeReceiptResearchAssignment1())
              
            {
              export.Export1.Update.CashReceiptSourceType.Assign(
                entities.CashReceiptSourceType);
              local.CashSourceTypeDisc.Date =
                export.Export1.Item.CashReceiptSourceType.DiscontinueDate;
              UseCabSetMaximumDiscontinueDate();
              export.Export1.Update.CashReceiptSourceType.DiscontinueDate =
                local.CashSourceTypeDisc.Date;
              export.Export1.Next();
            }

            if (export.Export1.IsEmpty)
            {
              ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

              return;
            }

            if (export.Export1.IsFull)
            {
              ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

              return;
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }

        if (IsEmpty(import.SearchServiceProvider.UserId))
        {
          if (AsChar(import.ShowHistory.Flag) == 'Y')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadCashReceiptSourceType2())
            {
              export.Export1.Update.CashReceiptSourceType.Assign(
                entities.CashReceiptSourceType);
              local.CashSourceTypeDisc.Date =
                export.Export1.Item.CashReceiptSourceType.DiscontinueDate;
              UseCabSetMaximumDiscontinueDate();
              export.Export1.Update.CashReceiptSourceType.DiscontinueDate =
                local.CashSourceTypeDisc.Date;
              export.Export1.Next();
            }

            if (export.Export1.IsEmpty)
            {
              ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

              return;
            }

            if (export.Export1.IsFull)
            {
              ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

              return;
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
          else if (AsChar(import.ShowHistory.Flag) != 'Y')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadCashReceiptSourceType1())
            {
              export.Export1.Update.CashReceiptSourceType.Assign(
                entities.CashReceiptSourceType);
              local.CashSourceTypeDisc.Date =
                export.Export1.Item.CashReceiptSourceType.DiscontinueDate;
              UseCabSetMaximumDiscontinueDate();
              export.Export1.Update.CashReceiptSourceType.DiscontinueDate =
                local.CashSourceTypeDisc.Date;
              export.Export1.Next();
            }

            if (export.Export1.IsEmpty)
            {
              ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

              return;
            }

            if (export.Export1.IsFull)
            {
              ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

              return;
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }

        break;
      case "RETURN":
        if (local.SelectionCounter.Count <= 1)
        {
          ExitState = "ACO_NE0000_RETURN";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RTFRMLNK":
        break;
      case "EXIT":
        ExitState = "ECO_XFR_TO_CASH_MNGMNT_MENU";

        break;
      default:
        if (Equal(global.Command, "ENTER"))
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";
        }

        break;
    }

    // Add any common logic that must occur at
    // the end of every pass.
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.InterfaceIndicator = source.InterfaceIndicator;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CourtInd = source.CourtInd;
    target.State = source.State;
    target.County = source.County;
    target.Location = source.Location;
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

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.CashSourceTypeDisc.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.CashSourceTypeDisc.Date = useExport.DateWorkArea.Date;
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

  private IEnumerable<bool> ReadCashReceiptSourceType1()
  {
    return ReadEach("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "code", import.SearchCashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 3);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 4);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 6);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 7);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 8);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 9);
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptSourceType2()
  {
    return ReadEach("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetString(command, "code", import.SearchCashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 3);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 4);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 6);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 7);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 8);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 9);
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptSourceTypeReceiptResearchAssignment1()
  {
    return ReadEach("ReadCashReceiptSourceTypeReceiptResearchAssignment1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdIdentifier", entities.ServiceProvider.SystemGeneratedId);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "code", import.SearchCashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 3);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 4);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 6);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 7);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 8);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 9);
        entities.ReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 10);
        entities.ReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 11);
        entities.ReceiptResearchAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.ReceiptResearchAssignment.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptSourceTypeReceiptResearchAssignment2()
  {
    return ReadEach("ReadCashReceiptSourceTypeReceiptResearchAssignment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdIdentifier", entities.ServiceProvider.SystemGeneratedId);
          
        db.SetString(command, "code", import.SearchCashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 3);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 4);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 6);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 7);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 8);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 9);
        entities.ReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 10);
        entities.ReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 11);
        entities.ReceiptResearchAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.ReceiptResearchAssignment.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);

        return true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", import.SearchServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 195;

      private Common common;
      private CashReceiptSourceType cashReceiptSourceType;
    }

    /// <summary>
    /// A value of FromAdminMenu.
    /// </summary>
    [JsonPropertyName("fromAdminMenu")]
    public Common FromAdminMenu
    {
      get => fromAdminMenu ??= new();
      set => fromAdminMenu = value;
    }

    /// <summary>
    /// A value of SearchServiceProvider.
    /// </summary>
    [JsonPropertyName("searchServiceProvider")]
    public ServiceProvider SearchServiceProvider
    {
      get => searchServiceProvider ??= new();
      set => searchServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("searchCashReceiptSourceType")]
    public CashReceiptSourceType SearchCashReceiptSourceType
    {
      get => searchCashReceiptSourceType ??= new();
      set => searchCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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

    private Common fromAdminMenu;
    private ServiceProvider searchServiceProvider;
    private CashReceiptSourceType searchCashReceiptSourceType;
    private Common showHistory;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 195;

      private Common common;
      private CashReceiptSourceType cashReceiptSourceType;
    }

    /// <summary>
    /// A value of FromAdminMenu.
    /// </summary>
    [JsonPropertyName("fromAdminMenu")]
    public Common FromAdminMenu
    {
      get => fromAdminMenu ??= new();
      set => fromAdminMenu = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public DateWorkArea CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of SearchServiceProvider.
    /// </summary>
    [JsonPropertyName("searchServiceProvider")]
    public ServiceProvider SearchServiceProvider
    {
      get => searchServiceProvider ??= new();
      set => searchServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("searchCashReceiptSourceType")]
    public CashReceiptSourceType SearchCashReceiptSourceType
    {
      get => searchCashReceiptSourceType ??= new();
      set => searchCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public CashReceiptSourceType HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
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
    /// A value of FlowSelection.
    /// </summary>
    [JsonPropertyName("flowSelection")]
    public CashReceiptSourceType FlowSelection
    {
      get => flowSelection ??= new();
      set => flowSelection = value;
    }

    private Common fromAdminMenu;
    private DateWorkArea cashReceiptSourceType;
    private ServiceProvider searchServiceProvider;
    private CashReceiptSourceType searchCashReceiptSourceType;
    private Common showHistory;
    private Array<ExportGroup> export1;
    private CashReceiptSourceType hiddenSelection;
    private NextTranInfo hidden;
    private Standard standard;
    private CashReceiptSourceType flowSelection;
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
    /// A value of CashSourceTypeDisc.
    /// </summary>
    [JsonPropertyName("cashSourceTypeDisc")]
    public DateWorkArea CashSourceTypeDisc
    {
      get => cashSourceTypeDisc ??= new();
      set => cashSourceTypeDisc = value;
    }

    /// <summary>
    /// A value of SelectionCounter.
    /// </summary>
    [JsonPropertyName("selectionCounter")]
    public Common SelectionCounter
    {
      get => selectionCounter ??= new();
      set => selectionCounter = value;
    }

    private DateWorkArea current;
    private DateWorkArea cashSourceTypeDisc;
    private Common selectionCounter;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReceiptResearchAssignment.
    /// </summary>
    [JsonPropertyName("receiptResearchAssignment")]
    public ReceiptResearchAssignment ReceiptResearchAssignment
    {
      get => receiptResearchAssignment ??= new();
      set => receiptResearchAssignment = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    private ReceiptResearchAssignment receiptResearchAssignment;
    private ServiceProvider serviceProvider;
    private CashReceiptSourceType cashReceiptSourceType;
  }
#endregion
}
