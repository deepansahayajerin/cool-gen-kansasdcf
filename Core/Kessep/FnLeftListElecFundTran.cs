// Program: FN_LEFT_LIST_ELEC_FUND_TRAN, ID: 372412714, model: 746.
// Short name: SWELEFTP
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
/// A program: FN_LEFT_LIST_ELEC_FUND_TRAN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnLeftListElecFundTran: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_LEFT_LIST_ELEC_FUND_TRAN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnLeftListElecFundTran(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnLeftListElecFundTran.
  /// </summary>
  public FnLeftListElecFundTran(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************
    // DATE       NAME			DESCRIPTION
    // 12/10/98   M Fangman 	        Initial Development	
    // ***************************************************************************
    switch(TrimEnd(global.Command))
    {
      case "CLEAR":
        MoveStandard(import.Standard, export.Standard);
        ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        break;
    }

    ExitState = "ACO_NN0000_ALL_OK";

    // *****
    // Move imports to exports
    // *****
    MoveStandard(import.Standard, export.Standard);
    MoveEftTransmissionFileInfo(import.Search, export.Search);
    export.SearchEnding.FileCreationDate = import.SearchEnding.FileCreationDate;

    if (Equal(global.Command, "EXIT"))
    {
      if (AsChar(export.Search.TransmissionType) == 'O')
      {
        ExitState = "ECO_XFR_TO_DISB_MGMNT_MENU";
      }
      else
      {
        ExitState = "ECO_XFR_TO_CASH_MGMNT_MENU";
      }

      return;
    }

    // *****
    // If the Transmission Type is blank then default it to "I".
    // *****
    if (IsEmpty(export.Search.TransmissionType))
    {
      export.Search.TransmissionType = "I";
    }

    // *****
    // If the Ending Date is blank then default it to the current date.
    // *****
    if (Lt(local.Initialized.FileCreationDate,
      export.SearchEnding.FileCreationDate))
    {
    }
    else
    {
      export.SearchEnding.FileCreationDate = Now().Date;
    }

    // *****
    // If the Begining Date is blank then default it to the End date less 1 
    // month.
    // *****
    if (Lt(local.Initialized.FileCreationDate, export.Search.FileCreationDate))
    {
    }
    else
    {
      export.Search.FileCreationDate =
        AddMonths(export.SearchEnding.FileCreationDate, -1);
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      local.NbrOfItemsSelected.Count = 0;

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
        export.Group.Update.EftTransmissionFileInfo.Assign(
          import.Group.Item.EftTransmissionFileInfo);

        // *****
        // Determine if a selection has been made
        // *****
        if (!IsEmpty(import.Group.Item.Common.SelectChar))
        {
          ++local.NbrOfItemsSelected.Count;

          if (AsChar(import.Group.Item.Common.SelectChar) == 'S')
          {
            export.Selection.TransmissionType = import.Search.TransmissionType;
            export.Selection.FileCreationDate =
              import.Group.Item.EftTransmissionFileInfo.FileCreationDate;
            export.Selection.FileCreationTime =
              import.Group.Item.EftTransmissionFileInfo.FileCreationTime;

            if (AsChar(export.Selection.TransmissionType) == 'I')
            {
              export.Selection.TransmissionStatusCode = "PENDED";
            }
            else
            {
              export.Selection.TransmissionStatusCode = "";
            }
          }
          else
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }

        export.Group.Next();
      }
    }

    if (local.NbrOfItemsSelected.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *****
    // Next Tran/Security logic
    // *****
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumber = export.CsePersonsWorkSet.Number;
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
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // **************************************
    // Validate search criteria.
    // **************************************
    if (Lt(Now().Date, export.Search.FileCreationDate))
    {
      var field = GetField(export.Search, "fileCreationDate");

      field.Error = true;

      ExitState = "FN0000_DATE_CANNOT_BE_IN_FUTURE";
    }

    if (Lt(local.Initialized.FileCreationDate,
      export.SearchEnding.FileCreationDate))
    {
      if (Lt(export.SearchEnding.FileCreationDate,
        export.Search.FileCreationDate))
      {
        var field = GetField(export.Search, "fileCreationDate");

        field.Error = true;

        ExitState = "FN0000_SEARCH_DATES_OVERLAP";
      }
    }

    switch(AsChar(export.Search.TransmissionType))
    {
      case 'I':
        break;
      case 'O':
        break;
      default:
        var field = GetField(export.Search, "transmissionType");

        field.Error = true;

        ExitState = "SP0000_INVALID_TYPE_CODE";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (Lt(local.Null1.Date, export.SearchEnding.FileCreationDate))
        {
          if (Lt(local.Null1.Date, export.Search.FileCreationDate))
          {
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadEftTransmissionFileInfo1())
            {
              export.Group.Update.EftTransmissionFileInfo.Assign(
                entities.EftTransmissionFileInfo);
              export.Group.Next();
            }
          }
          else
          {
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadEftTransmissionFileInfo3())
            {
              export.Group.Update.EftTransmissionFileInfo.Assign(
                entities.EftTransmissionFileInfo);
              export.Group.Next();
            }
          }
        }
        else if (Lt(local.Null1.Date, export.Search.FileCreationDate))
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadEftTransmissionFileInfo2())
          {
            export.Group.Update.EftTransmissionFileInfo.Assign(
              entities.EftTransmissionFileInfo);
            export.Group.Next();
          }
        }
        else
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadEftTransmissionFileInfo4())
          {
            export.Group.Update.EftTransmissionFileInfo.Assign(
              entities.EftTransmissionFileInfo);
            export.Group.Next();
          }
        }

        // *****
        // If group view is empty display message.  If group view is full 
        // display message.
        // *****
        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else if (export.Group.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "LTRN":
        if (local.NbrOfItemsSelected.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";
        }
        else if (local.NbrOfItemsSelected.Count == 1)
        {
          export.Selection.TransmissionStatusCode = "";
          ExitState = "ECO_LNK_LST_EFT_TRANSMISSION";
        }

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "RETURN":
        if (local.NbrOfItemsSelected.Count == 0)
        {
          export.Selection.TransmissionType = export.Search.TransmissionType;

          if (AsChar(export.Search.TransmissionType) == 'I')
          {
            export.Selection.TransmissionStatusCode = "PENDED";
          }
          else
          {
            export.Selection.TransmissionStatusCode = "";
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveEftTransmissionFileInfo(
    EftTransmissionFileInfo source, EftTransmissionFileInfo target)
  {
    target.TransmissionType = source.TransmissionType;
    target.FileCreationDate = source.FileCreationDate;
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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PromptField = source.PromptField;
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

  private IEnumerable<bool> ReadEftTransmissionFileInfo1()
  {
    return ReadEach("ReadEftTransmissionFileInfo1",
      (db, command) =>
      {
        db.
          SetString(command, "transmissionType", export.Search.TransmissionType);
          
        db.SetDate(
          command, "fileCreationDate1",
          export.Search.FileCreationDate.GetValueOrDefault());
        db.SetDate(
          command, "fileCreationDate2",
          export.SearchEnding.FileCreationDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.EftTransmissionFileInfo.TransmissionType =
          db.GetString(reader, 0);
        entities.EftTransmissionFileInfo.FileCreationDate =
          db.GetDate(reader, 1);
        entities.EftTransmissionFileInfo.FileCreationTime =
          db.GetTimeSpan(reader, 2);
        entities.EftTransmissionFileInfo.RecordCount = db.GetInt32(reader, 3);
        entities.EftTransmissionFileInfo.TotalAmount = db.GetDecimal(reader, 4);
        entities.EftTransmissionFileInfo.Populated = true;
        CheckValid<EftTransmissionFileInfo>("TransmissionType",
          entities.EftTransmissionFileInfo.TransmissionType);

        return true;
      });
  }

  private IEnumerable<bool> ReadEftTransmissionFileInfo2()
  {
    return ReadEach("ReadEftTransmissionFileInfo2",
      (db, command) =>
      {
        db.
          SetString(command, "transmissionType", export.Search.TransmissionType);
          
        db.SetDate(
          command, "fileCreationDate",
          export.Search.FileCreationDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.EftTransmissionFileInfo.TransmissionType =
          db.GetString(reader, 0);
        entities.EftTransmissionFileInfo.FileCreationDate =
          db.GetDate(reader, 1);
        entities.EftTransmissionFileInfo.FileCreationTime =
          db.GetTimeSpan(reader, 2);
        entities.EftTransmissionFileInfo.RecordCount = db.GetInt32(reader, 3);
        entities.EftTransmissionFileInfo.TotalAmount = db.GetDecimal(reader, 4);
        entities.EftTransmissionFileInfo.Populated = true;
        CheckValid<EftTransmissionFileInfo>("TransmissionType",
          entities.EftTransmissionFileInfo.TransmissionType);

        return true;
      });
  }

  private IEnumerable<bool> ReadEftTransmissionFileInfo3()
  {
    return ReadEach("ReadEftTransmissionFileInfo3",
      (db, command) =>
      {
        db.
          SetString(command, "transmissionType", export.Search.TransmissionType);
          
        db.SetDate(
          command, "fileCreationDate",
          export.SearchEnding.FileCreationDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.EftTransmissionFileInfo.TransmissionType =
          db.GetString(reader, 0);
        entities.EftTransmissionFileInfo.FileCreationDate =
          db.GetDate(reader, 1);
        entities.EftTransmissionFileInfo.FileCreationTime =
          db.GetTimeSpan(reader, 2);
        entities.EftTransmissionFileInfo.RecordCount = db.GetInt32(reader, 3);
        entities.EftTransmissionFileInfo.TotalAmount = db.GetDecimal(reader, 4);
        entities.EftTransmissionFileInfo.Populated = true;
        CheckValid<EftTransmissionFileInfo>("TransmissionType",
          entities.EftTransmissionFileInfo.TransmissionType);

        return true;
      });
  }

  private IEnumerable<bool> ReadEftTransmissionFileInfo4()
  {
    return ReadEach("ReadEftTransmissionFileInfo4",
      (db, command) =>
      {
        db.
          SetString(command, "transmissionType", export.Search.TransmissionType);
          
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.EftTransmissionFileInfo.TransmissionType =
          db.GetString(reader, 0);
        entities.EftTransmissionFileInfo.FileCreationDate =
          db.GetDate(reader, 1);
        entities.EftTransmissionFileInfo.FileCreationTime =
          db.GetTimeSpan(reader, 2);
        entities.EftTransmissionFileInfo.RecordCount = db.GetInt32(reader, 3);
        entities.EftTransmissionFileInfo.TotalAmount = db.GetDecimal(reader, 4);
        entities.EftTransmissionFileInfo.Populated = true;
        CheckValid<EftTransmissionFileInfo>("TransmissionType",
          entities.EftTransmissionFileInfo.TransmissionType);

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
      /// A value of EftTransmissionFileInfo.
      /// </summary>
      [JsonPropertyName("eftTransmissionFileInfo")]
      public EftTransmissionFileInfo EftTransmissionFileInfo
      {
        get => eftTransmissionFileInfo ??= new();
        set => eftTransmissionFileInfo = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common common;
      private EftTransmissionFileInfo eftTransmissionFileInfo;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public EftTransmissionFileInfo Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of SearchEnding.
    /// </summary>
    [JsonPropertyName("searchEnding")]
    public EftTransmissionFileInfo SearchEnding
    {
      get => searchEnding ??= new();
      set => searchEnding = value;
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

    private Standard standard;
    private EftTransmissionFileInfo search;
    private EftTransmissionFileInfo searchEnding;
    private Array<GroupGroup> group;
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
      /// A value of EftTransmissionFileInfo.
      /// </summary>
      [JsonPropertyName("eftTransmissionFileInfo")]
      public EftTransmissionFileInfo EftTransmissionFileInfo
      {
        get => eftTransmissionFileInfo ??= new();
        set => eftTransmissionFileInfo = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common common;
      private EftTransmissionFileInfo eftTransmissionFileInfo;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public EftTransmissionFileInfo Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of SearchEnding.
    /// </summary>
    [JsonPropertyName("searchEnding")]
    public EftTransmissionFileInfo SearchEnding
    {
      get => searchEnding ??= new();
      set => searchEnding = value;
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
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public ElectronicFundTransmission Selection
    {
      get => selection ??= new();
      set => selection = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Standard standard;
    private EftTransmissionFileInfo search;
    private EftTransmissionFileInfo searchEnding;
    private Array<GroupGroup> group;
    private ElectronicFundTransmission selection;
    private NextTranInfo hidden;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of NbrOfItemsSelected.
    /// </summary>
    [JsonPropertyName("nbrOfItemsSelected")]
    public Common NbrOfItemsSelected
    {
      get => nbrOfItemsSelected ??= new();
      set => nbrOfItemsSelected = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public EftTransmissionFileInfo Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private DateWorkArea null1;
    private Common nbrOfItemsSelected;
    private EftTransmissionFileInfo initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EftTransmissionFileInfo.
    /// </summary>
    [JsonPropertyName("eftTransmissionFileInfo")]
    public EftTransmissionFileInfo EftTransmissionFileInfo
    {
      get => eftTransmissionFileInfo ??= new();
      set => eftTransmissionFileInfo = value;
    }

    private EftTransmissionFileInfo eftTransmissionFileInfo;
  }
#endregion
}
