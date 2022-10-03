// Program: CO_B800_PRINT_HEADING, ID: 371136616, model: 746.
// Short name: SWE02725
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CO_B800_PRINT_HEADING.
/// </summary>
[Serializable]
public partial class CoB800PrintHeading: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_B800_PRINT_HEADING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoB800PrintHeading(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoB800PrintHeading.
  /// </summary>
  public CoB800PrintHeading(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.EabFileHandling.Action = "WRITE";

    if (import.PageNo.Count == 0)
    {
      // : Do not skip any lines - First page being printed.
    }
    else if (import.NextLineNo.Count > import.MaxLinesPerPage.Count)
    {
      // : Do not skip any lines - Page is already full.
    }
    else
    {
      local.TmpCommon.Count = import.NextLineNo.Count;

      for(var limit = import.MaxLinesPerPage.Count; local.TmpCommon.Count <= limit
        ; ++local.TmpCommon.Count)
      {
        UseCoB800HardcopyReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";

          return;
        }
      }
    }

    export.NextLineNo.Count = 1;
    export.PageNo.Count = import.PageNo.Count + 1;

    if (import.Header.IsEmpty)
    {
      return;
    }

    for(import.Header.Index = 0; import.Header.Index < import.Header.Count; ++
      import.Header.Index)
    {
      if (export.PageNo.Count > 1)
      {
        if (AsChar(import.Header.Item.Header1.FirstPageOnlyInd) == 'Y')
        {
          continue;
        }
      }

      local.TmpReportData.Assign(import.Header.Item.Header1);

      if (export.NextLineNo.Count == 1)
      {
        local.TmpReportData.LineText =
          Substring(local.TmpReportData.LineText, ReportData.LineText_MaxLength,
          1, 127) + NumberToString(export.PageNo.Count, 11, 15);
      }
      else if (Equal(local.TmpReportData.LineControl, "PG"))
      {
        // : "PG" is not supported while printing the page heading - Just print 
        // the line.
      }
      else if (!IsEmpty(local.TmpReportData.LineControl))
      {
        if (Verify(local.TmpReportData.LineControl, "0123456789") == 0)
        {
          local.BlankLine.Count =
            (int)StringToNumber(local.TmpReportData.LineControl);
          local.TmpCommon.Count = 1;

          for(var limit = local.BlankLine.Count; local.TmpCommon.Count <= limit
            ; ++local.TmpCommon.Count)
          {
            UseCoB800HardcopyReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ERROR_WRITING_TO_REPORT_AB";

              return;
            }

            ++export.NextLineNo.Count;
          }
        }
      }

      UseCoB800HardcopyReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      ++export.NextLineNo.Count;
    }
  }

  private void UseCoB800HardcopyReport1()
  {
    var useImport = new CoB800HardcopyReport.Import();
    var useExport = new CoB800HardcopyReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ReportData.LineText = local.Null1.LineText;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(CoB800HardcopyReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCoB800HardcopyReport2()
  {
    var useImport = new CoB800HardcopyReport.Import();
    var useExport = new CoB800HardcopyReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ReportData.LineText = local.TmpReportData.LineText;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(CoB800HardcopyReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// <summary>A HeaderGroup group.</summary>
    [Serializable]
    public class HeaderGroup
    {
      /// <summary>
      /// A value of Header1.
      /// </summary>
      [JsonPropertyName("header1")]
      public ReportData Header1
      {
        get => header1 ??= new();
        set => header1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private ReportData header1;
    }

    /// <summary>
    /// A value of PageNo.
    /// </summary>
    [JsonPropertyName("pageNo")]
    public Common PageNo
    {
      get => pageNo ??= new();
      set => pageNo = value;
    }

    /// <summary>
    /// A value of NextLineNo.
    /// </summary>
    [JsonPropertyName("nextLineNo")]
    public Common NextLineNo
    {
      get => nextLineNo ??= new();
      set => nextLineNo = value;
    }

    /// <summary>
    /// A value of MaxLinesPerPage.
    /// </summary>
    [JsonPropertyName("maxLinesPerPage")]
    public Common MaxLinesPerPage
    {
      get => maxLinesPerPage ??= new();
      set => maxLinesPerPage = value;
    }

    /// <summary>
    /// Gets a value of Header.
    /// </summary>
    [JsonIgnore]
    public Array<HeaderGroup> Header => header ??= new(HeaderGroup.Capacity);

    /// <summary>
    /// Gets a value of Header for json serialization.
    /// </summary>
    [JsonPropertyName("header")]
    [Computed]
    public IList<HeaderGroup> Header_Json
    {
      get => header;
      set => Header.Assign(value);
    }

    private Common pageNo;
    private Common nextLineNo;
    private Common maxLinesPerPage;
    private Array<HeaderGroup> header;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PageNo.
    /// </summary>
    [JsonPropertyName("pageNo")]
    public Common PageNo
    {
      get => pageNo ??= new();
      set => pageNo = value;
    }

    /// <summary>
    /// A value of NextLineNo.
    /// </summary>
    [JsonPropertyName("nextLineNo")]
    public Common NextLineNo
    {
      get => nextLineNo ??= new();
      set => nextLineNo = value;
    }

    private Common pageNo;
    private Common nextLineNo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BlankLine.
    /// </summary>
    [JsonPropertyName("blankLine")]
    public Common BlankLine
    {
      get => blankLine ??= new();
      set => blankLine = value;
    }

    /// <summary>
    /// A value of TmpCommon.
    /// </summary>
    [JsonPropertyName("tmpCommon")]
    public Common TmpCommon
    {
      get => tmpCommon ??= new();
      set => tmpCommon = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public ReportData Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of TmpReportData.
    /// </summary>
    [JsonPropertyName("tmpReportData")]
    public ReportData TmpReportData
    {
      get => tmpReportData ??= new();
      set => tmpReportData = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public External DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private Common blankLine;
    private Common tmpCommon;
    private EabFileHandling eabFileHandling;
    private ReportData null1;
    private ReportData tmpReportData;
    private External delMe;
  }
#endregion
}
