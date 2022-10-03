// Program: FN_AB_VALIDATE_SDSO_FILE, ID: 372428113, model: 746.
// Short name: SWE01673
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_AB_VALIDATE_SDSO_FILE.
/// </summary>
[Serializable]
public partial class FnAbValidateSdsoFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_VALIDATE_SDSO_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbValidateSdsoFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbValidateSdsoFile.
  /// </summary>
  public FnAbValidateSdsoFile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // Every initial development and change to that
    // development needs to be documented.
    // ***************************************************
    // *****************************************************************************************
    // Date      Developers Name         Request #  Description
    // --------  ----------------------  ---------
    // 
    // --------------------------------------------
    // 09/24/96  Holly Kennedy - MTW                Initial
    // 04/28/97  SHERAZ MALIK				CHANGE CURRENT DATE
    // *****************************************************************************************
    // *****
    // Hardcode Area.
    // *****
    local.Current.Date = Now().Date;
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeRead.FileInstruction = "READ";
    local.HardcodeClose.FileInstruction = "CLOSE";

    // *****
    // Validate the SDSO Source Type
    // *****
    if (ReadCashReceiptSourceType())
    {
      export.Sdso.Code = entities.Sdso.Code;
    }
    else
    {
      ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

      return;
    }

    // *****
    // Call external to open the driver file.
    // *****
    local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
    UseEabSdsoInterfaceDrvr2();

    if (!Equal(local.PassArea.TextReturnCode, "00"))
    {
      ExitState = "ZD_FILE_OPEN_ERROR_WITH_AB";

      return;
    }

    // *****
    // Set the previous Record Type to '3'.  This will prevent the "Input File 
    // not sorted" error on the first record when validating the file sort.
    // *****
    local.PreviousRecordType.Count = 3;

    do
    {
      // *****
      // Call external to read the driver file.
      // *****
      local.PassArea.FileInstruction = local.HardcodeRead.FileInstruction;
      UseEabSdsoInterfaceDrvr1();

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "00":
          ++local.TotalRecords.Count;

          break;
        case "EF":
          if (local.TotalRecords.Count == 0)
          {
            ExitState = "ACO_RE0000_INPUT_FILE_EMPTY_RB";

            return;
          }

          if (local.PreviousRecordType.Count != 3)
          {
            ExitState = "ACO_RE0000_INPUT_FILE_NOT_SORTED";

            return;
          }

          return;
        default:
          ExitState = "FILE_READ_ERROR_WITH_RB";

          return;
      }

      // *****
      // Make sure the input file is not out of sequence.
      // *****
      switch(local.RecordType.Count)
      {
        case 1:
          if (local.PreviousRecordType.Count == 2)
          {
            ExitState = "ACO_RE0000_INPUT_FILE_NOT_SORTED";

            break;
          }

          local.PreviousRecordType.Count = 1;

          break;
        case 2:
          if (local.PreviousRecordType.Count != 1 && local
            .PreviousRecordType.Count != 2)
          {
            ExitState = "ACO_RE0000_INPUT_FILE_NOT_SORTED";

            break;
          }

          local.PreviousRecordType.Count = 2;

          break;
        case 3:
          if (local.PreviousRecordType.Count != 2)
          {
            ExitState = "ACO_RE0000_INPUT_FILE_NOT_SORTED";

            break;
          }

          local.PreviousRecordType.Count = 3;

          break;
        default:
          ExitState = "ACO_RE0000_INPUT_RECORD_TYPE_INV";

          break;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.PreviousRecordType.Count = local.RecordType.Count;
      }
      else
      {
        return;
      }
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmount = source.CollectionAmount;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.TotalNonCashTransactionCount = source.TotalNonCashTransactionCount;
    target.TotalNonCashAmt = source.TotalNonCashAmt;
  }

  private void UseEabSdsoInterfaceDrvr1()
  {
    var useImport = new EabSdsoInterfaceDrvr.Import();
    var useExport = new EabSdsoInterfaceDrvr.Export();

    useImport.External.Assign(local.PassArea);
    MoveCashReceiptEvent(local.TotalRecord.DetailTotal,
      useExport.TotalRecord.DetailTotal);
    useExport.DetailRecord.CollectionType.SelectChar =
      local.DetailRecord.CollectionType.SelectChar;
    MoveCashReceiptDetail(local.DetailRecord.DetailDetail,
      useExport.DetailRecord.Detail);
    useExport.HeaderRecord.Detail.SourceCreationDate =
      local.HeaderRecord.HeaderDetail.SourceCreationDate;
    useExport.RecordType.Count = local.RecordType.Count;
    useExport.External.Assign(local.PassArea);

    Call(EabSdsoInterfaceDrvr.Execute, useImport, useExport);

    local.PassArea.Assign(useImport.External);
    MoveCashReceiptEvent(useExport.TotalRecord.DetailTotal,
      local.TotalRecord.DetailTotal);
    local.DetailRecord.CollectionType.SelectChar =
      useExport.DetailRecord.CollectionType.SelectChar;
    MoveCashReceiptDetail(useExport.DetailRecord.Detail,
      local.DetailRecord.DetailDetail);
    local.HeaderRecord.HeaderDetail.SourceCreationDate =
      useExport.HeaderRecord.Detail.SourceCreationDate;
    local.RecordType.Count = useExport.RecordType.Count;
    local.PassArea.Assign(useExport.External);
  }

  private void UseEabSdsoInterfaceDrvr2()
  {
    var useImport = new EabSdsoInterfaceDrvr.Import();
    var useExport = new EabSdsoInterfaceDrvr.Export();

    useImport.External.Assign(local.PassArea);
    useExport.External.Assign(local.PassArea);

    Call(EabSdsoInterfaceDrvr.Execute, useImport, useExport);

    local.PassArea.Assign(useImport.External);
    local.PassArea.Assign(useExport.External);
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.Sdso.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Sdso.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Sdso.InterfaceIndicator = db.GetString(reader, 1);
        entities.Sdso.Code = db.GetString(reader, 2);
        entities.Sdso.EffectiveDate = db.GetDate(reader, 3);
        entities.Sdso.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Sdso.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.Sdso.InterfaceIndicator);
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Sdso.
    /// </summary>
    [JsonPropertyName("sdso")]
    public CashReceiptSourceType Sdso
    {
      get => sdso ??= new();
      set => sdso = value;
    }

    private CashReceiptSourceType sdso;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A TotalRecordGroup group.</summary>
    [Serializable]
    public class TotalRecordGroup
    {
      /// <summary>
      /// A value of DetailTotal.
      /// </summary>
      [JsonPropertyName("detailTotal")]
      public CashReceiptEvent DetailTotal
      {
        get => detailTotal ??= new();
        set => detailTotal = value;
      }

      private CashReceiptEvent detailTotal;
    }

    /// <summary>A DetailRecordGroup group.</summary>
    [Serializable]
    public class DetailRecordGroup
    {
      /// <summary>
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      public Common CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>
      /// A value of DetailDetail.
      /// </summary>
      [JsonPropertyName("detailDetail")]
      public CashReceiptDetail DetailDetail
      {
        get => detailDetail ??= new();
        set => detailDetail = value;
      }

      private Common collectionType;
      private CashReceiptDetail detailDetail;
    }

    /// <summary>A HeaderRecordGroup group.</summary>
    [Serializable]
    public class HeaderRecordGroup
    {
      /// <summary>
      /// A value of HeaderDetail.
      /// </summary>
      [JsonPropertyName("headerDetail")]
      public CashReceiptEvent HeaderDetail
      {
        get => headerDetail ??= new();
        set => headerDetail = value;
      }

      private CashReceiptEvent headerDetail;
    }

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
    /// Gets a value of TotalRecord.
    /// </summary>
    [JsonPropertyName("totalRecord")]
    public TotalRecordGroup TotalRecord
    {
      get => totalRecord ?? (totalRecord = new());
      set => totalRecord = value;
    }

    /// <summary>
    /// Gets a value of DetailRecord.
    /// </summary>
    [JsonPropertyName("detailRecord")]
    public DetailRecordGroup DetailRecord
    {
      get => detailRecord ?? (detailRecord = new());
      set => detailRecord = value;
    }

    /// <summary>
    /// Gets a value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecordGroup HeaderRecord
    {
      get => headerRecord ?? (headerRecord = new());
      set => headerRecord = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of HardcodeOpen.
    /// </summary>
    [JsonPropertyName("hardcodeOpen")]
    public External HardcodeOpen
    {
      get => hardcodeOpen ??= new();
      set => hardcodeOpen = value;
    }

    /// <summary>
    /// A value of HardcodeRead.
    /// </summary>
    [JsonPropertyName("hardcodeRead")]
    public External HardcodeRead
    {
      get => hardcodeRead ??= new();
      set => hardcodeRead = value;
    }

    /// <summary>
    /// A value of HardcodeClose.
    /// </summary>
    [JsonPropertyName("hardcodeClose")]
    public External HardcodeClose
    {
      get => hardcodeClose ??= new();
      set => hardcodeClose = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of PreviousRecordType.
    /// </summary>
    [JsonPropertyName("previousRecordType")]
    public Common PreviousRecordType
    {
      get => previousRecordType ??= new();
      set => previousRecordType = value;
    }

    /// <summary>
    /// A value of TotalRecords.
    /// </summary>
    [JsonPropertyName("totalRecords")]
    public Common TotalRecords
    {
      get => totalRecords ??= new();
      set => totalRecords = value;
    }

    private DateWorkArea current;
    private TotalRecordGroup totalRecord;
    private DetailRecordGroup detailRecord;
    private HeaderRecordGroup headerRecord;
    private Common recordType;
    private External hardcodeOpen;
    private External hardcodeRead;
    private External hardcodeClose;
    private External passArea;
    private Common previousRecordType;
    private Common totalRecords;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Sdso.
    /// </summary>
    [JsonPropertyName("sdso")]
    public CashReceiptSourceType Sdso
    {
      get => sdso ??= new();
      set => sdso = value;
    }

    private CashReceiptSourceType sdso;
  }
#endregion
}
