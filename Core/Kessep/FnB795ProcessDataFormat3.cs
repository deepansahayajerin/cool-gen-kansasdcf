// Program: FN_B795_PROCESS_DATA_FORMAT_3, ID: 1902456223, model: 746.
// Short name: SWE03740
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B795_PROCESS_DATA_FORMAT_3.
/// </summary>
[Serializable]
public partial class FnB795ProcessDataFormat3: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B795_PROCESS_DATA_FORMAT_3 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB795ProcessDataFormat3(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB795ProcessDataFormat3.
  /// </summary>
  public FnB795ProcessDataFormat3(IContext context, Import import, Export export)
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
    export.FileType3ARecords.Count = import.FileType3ARecords.Count;
    export.FileType3BRecords.Count = import.FileType3BRecords.Count;

    switch(TrimEnd(import.EabFileHandling.Action))
    {
      case "OPEN":
        // -- Open the first file and write header record.
        local.FileType.Text1 = "1";
        local.EabFileHandling.Action = "OPEN";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        // -- Set Header Record Info
        local.DateWorkArea.Date = import.ProgramProcessingInfo.ProcessDate;
        local.Date.Text10 = UseCabFormatDate();
        local.HeaderFooter.HeaderFooter = local.Date.Text10 + "  CASE UNIVERSE - DATA FORMAT 3 - FILE 1 - A RECORD (HEADER), B RECORD (CASE/NCP INFO), Z RECORD (FOOTER) - SR.SR24209.FORMAT3A";
          
        local.RecordType.Text1 = "A";
        local.EabFileHandling.Action = "WRITE";
        UseFnB795EabDataFormat2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        ++export.FileType3ARecords.Count;

        // -- Open the second file and write header record.
        local.FileType.Text1 = "2";
        local.EabFileHandling.Action = "OPEN";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        // -- Set Header Record Info
        local.DateWorkArea.Date = import.ProgramProcessingInfo.ProcessDate;
        local.Date.Text10 = UseCabFormatDate();
        local.HeaderFooter.HeaderFooter = local.Date.Text10 + "  CASE UNIVERSE - DATA FORMAT 3 - FILE 2 - A RECORD (HEADER), C RECORD (COURT ORDER INFO), Z RECORD (FOOTER) - SR.SR04209.FORMAT3B";
          
        local.RecordType.Text1 = "A";
        local.EabFileHandling.Action = "WRITE";
        UseFnB795EabDataFormat2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        export.FileType3BRecords.Count = import.FileType3BRecords.Count + 1;

        break;
      case "EXTEND":
        // -- Extend the files.
        local.FileType.Text1 = "1";
        local.EabFileHandling.Action = "EXTEND";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        local.FileType.Text1 = "2";
        local.EabFileHandling.Action = "EXTEND";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
        }

        break;
      case "WRITE":
        // -- Write the Case/NCP and Court Order info to file.
        local.FileType.Text1 = "1";
        local.RecordType.Text1 = "B";
        local.EabFileHandling.Action = "WRITE";
        UseFnB795EabDataFormat1();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        ++export.FileType3ARecords.Count;

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (!import.Group.CheckSize())
          {
            break;
          }

          local.FileType.Text1 = "2";
          local.RecordType.Text1 = "C";
          local.EabFileHandling.Action = "WRITE";
          UseFnB795EabDataFormat4();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            return;
          }

          ++export.FileType3BRecords.Count;
        }

        import.Group.CheckIndex();

        break;
      case "COMMIT":
        // -- Commit the file records.
        local.FileType.Text1 = "1";
        local.EabFileHandling.Action = "COMMIT";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        local.FileType.Text1 = "2";
        local.EabFileHandling.Action = "COMMIT";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
        }

        break;
      case "CLOSE":
        // -- Write the footer record and close the first file.
        ++export.FileType3ARecords.Count;

        // -- Set Footer Record Info
        local.HeaderFooter.HeaderFooter = "";
        local.HeaderFooter.HeaderFooter =
          NumberToString(export.FileType3ARecords.Count, 15) + Substring
          (local.HeaderFooter.HeaderFooter,
          ContractorCaseUniverse.HeaderFooter_MaxLength, 16, 184);
        local.FileType.Text1 = "1";
        local.RecordType.Text1 = "Z";
        local.EabFileHandling.Action = "WRITE";
        UseFnB795EabDataFormat2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        // -- Commit the file records.
        local.EabFileHandling.Action = "COMMIT";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        // -- Close the file.
        local.EabFileHandling.Action = "CLOSE";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        // -- Write the footer record and close the second file.
        ++export.FileType3BRecords.Count;

        // -- Set Footer Record Info
        local.HeaderFooter.HeaderFooter = "";
        local.HeaderFooter.HeaderFooter =
          NumberToString(export.FileType3BRecords.Count, 15) + Substring
          (local.HeaderFooter.HeaderFooter,
          ContractorCaseUniverse.HeaderFooter_MaxLength, 16, 184);
        local.FileType.Text1 = "2";
        local.RecordType.Text1 = "Z";
        local.EabFileHandling.Action = "WRITE";
        UseFnB795EabDataFormat2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        // -- Commit the file records.
        local.EabFileHandling.Action = "COMMIT";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        // -- Close the file.
        local.EabFileHandling.Action = "CLOSE";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
        }

        break;
      default:
        export.EabFileHandling.Status = "BI";

        break;
    }
  }

  private string UseCabFormatDate()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    return useExport.FormattedDate.Text10;
  }

  private void UseFnB795EabDataFormat1()
  {
    var useImport = new FnB795EabDataFormat3.Import();
    var useExport = new FnB795EabDataFormat3.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType.Text1 = local.FileType.Text1;
    useImport.RecordType.Text1 = local.RecordType.Text1;
    useImport.CaseNcp.Assign(import.ContractorCaseUniverse);
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(FnB795EabDataFormat3.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB795EabDataFormat2()
  {
    var useImport = new FnB795EabDataFormat3.Import();
    var useExport = new FnB795EabDataFormat3.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType.Text1 = local.FileType.Text1;
    useImport.RecordType.Text1 = local.RecordType.Text1;
    useImport.HeaderFooter.HeaderFooter = local.HeaderFooter.HeaderFooter;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(FnB795EabDataFormat3.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB795EabDataFormat4()
  {
    var useImport = new FnB795EabDataFormat3.Import();
    var useExport = new FnB795EabDataFormat3.Export();

    useImport.CourtOrder.Assign(import.Group.Item.G);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType.Text1 = local.FileType.Text1;
    useImport.RecordType.Text1 = local.RecordType.Text1;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(FnB795EabDataFormat3.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB795EabDataFormat5()
  {
    var useImport = new FnB795EabDataFormat3.Import();
    var useExport = new FnB795EabDataFormat3.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType.Text1 = local.FileType.Text1;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(FnB795EabDataFormat3.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public ContractorCaseUniverse G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 75;

      private ContractorCaseUniverse g;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of FileType3ARecords.
    /// </summary>
    [JsonPropertyName("fileType3ARecords")]
    public Common FileType3ARecords
    {
      get => fileType3ARecords ??= new();
      set => fileType3ARecords = value;
    }

    /// <summary>
    /// A value of FileType3BRecords.
    /// </summary>
    [JsonPropertyName("fileType3BRecords")]
    public Common FileType3BRecords
    {
      get => fileType3BRecords ??= new();
      set => fileType3BRecords = value;
    }

    /// <summary>
    /// A value of ContractorCaseUniverse.
    /// </summary>
    [JsonPropertyName("contractorCaseUniverse")]
    public ContractorCaseUniverse ContractorCaseUniverse
    {
      get => contractorCaseUniverse ??= new();
      set => contractorCaseUniverse = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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

    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private Common fileType3ARecords;
    private Common fileType3BRecords;
    private ContractorCaseUniverse contractorCaseUniverse;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of FileType3ARecords.
    /// </summary>
    [JsonPropertyName("fileType3ARecords")]
    public Common FileType3ARecords
    {
      get => fileType3ARecords ??= new();
      set => fileType3ARecords = value;
    }

    /// <summary>
    /// A value of FileType3BRecords.
    /// </summary>
    [JsonPropertyName("fileType3BRecords")]
    public Common FileType3BRecords
    {
      get => fileType3BRecords ??= new();
      set => fileType3BRecords = value;
    }

    private EabFileHandling eabFileHandling;
    private Common fileType3ARecords;
    private Common fileType3BRecords;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FileType.
    /// </summary>
    [JsonPropertyName("fileType")]
    public TextWorkArea FileType
    {
      get => fileType ??= new();
      set => fileType = value;
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

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of HeaderFooter.
    /// </summary>
    [JsonPropertyName("headerFooter")]
    public ContractorCaseUniverse HeaderFooter
    {
      get => headerFooter ??= new();
      set => headerFooter = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private TextWorkArea fileType;
    private DateWorkArea dateWorkArea;
    private TextWorkArea date;
    private TextWorkArea recordType;
    private ContractorCaseUniverse headerFooter;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
