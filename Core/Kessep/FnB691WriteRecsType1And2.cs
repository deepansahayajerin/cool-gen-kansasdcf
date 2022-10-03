// Program: FN_B691_WRITE_RECS_TYPE_1_AND_2, ID: 370998512, model: 746.
// Short name: SWE00369
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B691_WRITE_RECS_TYPE_1_AND_2.
/// </summary>
[Serializable]
public partial class FnB691WriteRecsType1And2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B691_WRITE_RECS_TYPE_1_AND_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB691WriteRecsType1And2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB691WriteRecsType1And2.
  /// </summary>
  public FnB691WriteRecsType1And2(IContext context, Import import, Export export)
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
    // --------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------------------------------------------------------------------------------------------------------------
    // 10/18/00  VMadhira			Initial Code
    // 01/23/03  GVandy	PR 162868	Correct logic setting old court order number, 
    // city, and county.
    // --------------------------------------------------------------------------------------------------------------
    local.FileNumber.Count = 1;
    export.TotalWritten.Count = import.TotalWritten.Count;

    switch(TrimEnd(import.HeaderRecord.ActionCode))
    {
      case "NCO":
        local.Send.Parm1 = "GR";

        // ******************************************************
        // Write Header Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.HeaderRecord.RecordType + import.HeaderRecord.ActionCode + import
          .HeaderRecord.TransactionDate + import.HeaderRecord.Userid + import
          .HeaderRecord.Timestamp + import.HeaderRecord.Filler;
        UseFnExtWriteInterfaceFile();
        ++export.TotalWritten.Count;

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Write Court Order Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.CourtOrderRecord.RecordType + import
          .CourtOrderRecord.CourtOrderNumber + import
          .CourtOrderRecord.CourtOrderType + import.CourtOrderRecord.FfpFlag + import
          .CourtOrderRecord.StartDate + import.CourtOrderRecord.EndDate + import
          .CourtOrderRecord.CountyId + import.CourtOrderRecord.CityIndicator + import
          .CourtOrderRecord.ModificationDate + import.CourtOrderRecord.Filler;
        UseFnExtWriteInterfaceFile();
        ++export.TotalWritten.Count;

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";
        }

        break;
      case "RCO":
        local.Send.Parm1 = "GR";

        // ******************************************************
        // Write Header Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.HeaderRecord.RecordType + import.HeaderRecord.ActionCode + import
          .HeaderRecord.TransactionDate + import.HeaderRecord.Userid + import
          .HeaderRecord.Timestamp + import.HeaderRecord.Filler;
        UseFnExtWriteInterfaceFile();
        ++export.TotalWritten.Count;

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Write  New Court Order Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.CourtOrderRecord.RecordType + import
          .CourtOrderRecord.CourtOrderNumber + import
          .CourtOrderRecord.CourtOrderType + import.CourtOrderRecord.FfpFlag + import
          .CourtOrderRecord.StartDate + import.CourtOrderRecord.EndDate + import
          .CourtOrderRecord.CountyId + import.CourtOrderRecord.CityIndicator + import
          .CourtOrderRecord.ModificationDate;
        UseFnExtWriteInterfaceFile();
        ++export.TotalWritten.Count;

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Write  Old  Court Order Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.CourtOrderRecord.RecordType + import.Old.CourtOrderNumber + import
          .CourtOrderRecord.CourtOrderType + import.CourtOrderRecord.FfpFlag + import
          .CourtOrderRecord.StartDate + import.CourtOrderRecord.EndDate + import
          .Old.CountyId + import.Old.CityIndicator + import
          .CourtOrderRecord.ModificationDate;
        UseFnExtWriteInterfaceFile();
        ++export.TotalWritten.Count;

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";
        }

        break;
      case "NDBT":
        local.Send.Parm1 = "GR";

        // ******************************************************
        // Write Header Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.HeaderRecord.RecordType + import.HeaderRecord.ActionCode + import
          .HeaderRecord.TransactionDate + import.HeaderRecord.Userid + import
          .HeaderRecord.Timestamp + import.HeaderRecord.Filler;
        UseFnExtWriteInterfaceFile();
        ++export.TotalWritten.Count;

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Write Court Order Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.CourtOrderRecord.RecordType + import
          .CourtOrderRecord.CourtOrderNumber + import
          .CourtOrderRecord.CourtOrderType + import.CourtOrderRecord.FfpFlag + import
          .CourtOrderRecord.StartDate + import.CourtOrderRecord.EndDate + import
          .CourtOrderRecord.CountyId + import.CourtOrderRecord.CityIndicator + import
          .CourtOrderRecord.ModificationDate + import.CourtOrderRecord.Filler;
        UseFnExtWriteInterfaceFile();
        ++export.TotalWritten.Count;

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";
        }

        break;
      default:
        break;
    }
  }

  private static void MoveKpcExternalParms(KpcExternalParms source,
    KpcExternalParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseFnExtWriteInterfaceFile()
  {
    var useImport = new FnExtWriteInterfaceFile.Import();
    var useExport = new FnExtWriteInterfaceFile.Export();

    MoveKpcExternalParms(local.Send, useImport.KpcExternalParms);
    useImport.PrintFileRecord.CourtOrderLine =
      local.PrintFileRecord.CourtOrderLine;
    useImport.FileCount.Count = local.FileNumber.Count;
    MoveKpcExternalParms(local.Return1, useExport.KpcExternalParms);

    Call(FnExtWriteInterfaceFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.Return1);
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
    /// <summary>
    /// A value of TotalWritten.
    /// </summary>
    [JsonPropertyName("totalWritten")]
    public Common TotalWritten
    {
      get => totalWritten ??= new();
      set => totalWritten = value;
    }

    /// <summary>
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecord HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
    }

    /// <summary>
    /// A value of CourtOrderRecord.
    /// </summary>
    [JsonPropertyName("courtOrderRecord")]
    public CourtOrderRecord CourtOrderRecord
    {
      get => courtOrderRecord ??= new();
      set => courtOrderRecord = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public CourtOrderRecord Old
    {
      get => old ??= new();
      set => old = value;
    }

    private Common totalWritten;
    private HeaderRecord headerRecord;
    private CourtOrderRecord courtOrderRecord;
    private CourtOrderRecord old;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TotalWritten.
    /// </summary>
    [JsonPropertyName("totalWritten")]
    public Common TotalWritten
    {
      get => totalWritten ??= new();
      set => totalWritten = value;
    }

    private Common totalWritten;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FileNumber.
    /// </summary>
    [JsonPropertyName("fileNumber")]
    public Common FileNumber
    {
      get => fileNumber ??= new();
      set => fileNumber = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public KpcExternalParms Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public KpcExternalParms Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    /// <summary>
    /// A value of PrintFileRecord.
    /// </summary>
    [JsonPropertyName("printFileRecord")]
    public PrintFileRecord PrintFileRecord
    {
      get => printFileRecord ??= new();
      set => printFileRecord = value;
    }

    private Common fileNumber;
    private KpcExternalParms send;
    private KpcExternalParms return1;
    private PrintFileRecord printFileRecord;
  }
#endregion
}
