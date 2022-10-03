// Program: FN_B795_PROCESS_DATA_FORMAT_2, ID: 1902456222, model: 746.
// Short name: SWE03739
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B795_PROCESS_DATA_FORMAT_2.
/// </summary>
[Serializable]
public partial class FnB795ProcessDataFormat2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B795_PROCESS_DATA_FORMAT_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB795ProcessDataFormat2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB795ProcessDataFormat2.
  /// </summary>
  public FnB795ProcessDataFormat2(IContext context, Import import, Export export)
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
    export.FileType2Records.Count = import.FileType2Records.Count;

    switch(TrimEnd(import.EabFileHandling.Action))
    {
      case "OPEN":
        // -- Open the file and write header record.
        local.EabFileHandling.Action = "OPEN";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        // -- Set Header Record Info
        local.DateWorkArea.Date = import.ProgramProcessingInfo.ProcessDate;
        local.Date.Text10 = UseCabFormatDate();
        local.HeaderFooter.HeaderFooter = local.Date.Text10 + "  CASE UNIVERSE - DATA FORMAT 2 - SINGLE FILE, A RECORD (HEADER), B RECORD (CASE/NCP/COURT ORDER INFO), Z RECORD (FOOTER) - SR.SR24209.FORMAT2";
          
        local.RecordType.Text1 = "A";
        local.EabFileHandling.Action = "WRITE";
        UseFnB795EabDataFormat4();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        ++export.FileType2Records.Count;

        break;
      case "EXTEND":
        // -- Extend the file.
        local.EabFileHandling.Action = "EXTEND";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
        }

        break;
      case "WRITE":
        // -- Write the Case/NCP and Court Order info to file.
        if (import.Group.Count == 0)
        {
          local.RecordType.Text1 = "B";
          local.EabFileHandling.Action = "WRITE";
          UseFnB795EabDataFormat1();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            return;
          }

          ++export.FileType2Records.Count;
        }
        else
        {
          for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
            import.Group.Index)
          {
            if (!import.Group.CheckSize())
            {
              break;
            }

            local.RecordType.Text1 = "B";
            local.EabFileHandling.Action = "WRITE";
            UseFnB795EabDataFormat3();

            if (!Equal(export.EabFileHandling.Status, "OK"))
            {
              return;
            }

            ++export.FileType2Records.Count;
          }

          import.Group.CheckIndex();
        }

        break;
      case "COMMIT":
        // -- Commit the file records.
        local.EabFileHandling.Action = "COMMIT";
        UseFnB795EabDataFormat5();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
        }

        break;
      case "CLOSE":
        // -- Write the footer record and close the file.
        ++export.FileType2Records.Count;

        // -- Set Footer Record Info
        local.HeaderFooter.HeaderFooter = "";
        local.HeaderFooter.HeaderFooter =
          NumberToString(export.FileType2Records.Count, 15) + Substring
          (local.HeaderFooter.HeaderFooter,
          ContractorCaseUniverse.HeaderFooter_MaxLength, 16, 184);
        local.RecordType.Text1 = "Z";
        local.EabFileHandling.Action = "WRITE";
        UseFnB795EabDataFormat4();

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
    var useImport = new FnB795EabDataFormat2.Import();
    var useExport = new FnB795EabDataFormat2.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.RecordType.Text1 = local.RecordType.Text1;
    useImport.CaseNcp.Assign(import.ContractorCaseUniverse);
    useImport.CourtOrder.Assign(import.NullCourtOrder);
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(FnB795EabDataFormat2.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB795EabDataFormat3()
  {
    var useImport = new FnB795EabDataFormat2.Import();
    var useExport = new FnB795EabDataFormat2.Export();

    useImport.CourtOrder.Assign(import.Group.Item.G);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.RecordType.Text1 = local.RecordType.Text1;
    useImport.CaseNcp.Assign(import.ContractorCaseUniverse);
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(FnB795EabDataFormat2.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB795EabDataFormat4()
  {
    var useImport = new FnB795EabDataFormat2.Import();
    var useExport = new FnB795EabDataFormat2.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.RecordType.Text1 = local.RecordType.Text1;
    useImport.HeaderFooter.HeaderFooter = local.HeaderFooter.HeaderFooter;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(FnB795EabDataFormat2.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB795EabDataFormat5()
  {
    var useImport = new FnB795EabDataFormat2.Import();
    var useExport = new FnB795EabDataFormat2.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(FnB795EabDataFormat2.Execute, useImport, useExport);

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
    /// A value of FileType2Records.
    /// </summary>
    [JsonPropertyName("fileType2Records")]
    public Common FileType2Records
    {
      get => fileType2Records ??= new();
      set => fileType2Records = value;
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

    /// <summary>
    /// A value of NullCourtOrder.
    /// </summary>
    [JsonPropertyName("nullCourtOrder")]
    public ContractorCaseUniverse NullCourtOrder
    {
      get => nullCourtOrder ??= new();
      set => nullCourtOrder = value;
    }

    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private Common fileType2Records;
    private ContractorCaseUniverse contractorCaseUniverse;
    private Array<GroupGroup> group;
    private ContractorCaseUniverse nullCourtOrder;
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
    /// A value of FileType2Records.
    /// </summary>
    [JsonPropertyName("fileType2Records")]
    public Common FileType2Records
    {
      get => fileType2Records ??= new();
      set => fileType2Records = value;
    }

    private EabFileHandling eabFileHandling;
    private Common fileType2Records;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    private DateWorkArea dateWorkArea;
    private TextWorkArea date;
    private TextWorkArea recordType;
    private ContractorCaseUniverse headerFooter;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
