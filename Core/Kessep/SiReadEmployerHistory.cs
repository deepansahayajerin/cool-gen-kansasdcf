// Program: SI_READ_EMPLOYER_HISTORY, ID: 1902583918, model: 746.
// Short name: SWE03770
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_READ_EMPLOYER_HISTORY.
/// </summary>
[Serializable]
public partial class SiReadEmployerHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_EMPLOYER_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadEmployerHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadEmployerHistory.
  /// </summary>
  public SiReadEmployerHistory(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //     M A I N T E N A N C E    L O G
    //   Date     Developer   Description
    // 02-07-2017 D Dupree    Initial Development
    // ---------------------------------------------
    local.CurrentDateWorkArea.Date = Now().Date;
    UseCabSetMaximumDiscontinueDate();

    if (ReadEmployer())
    {
      export.WorksiteEmployer.Assign(entities.Ws);

      if (ReadEmployerAddress())
      {
        export.WorksiteEmployerAddress.Assign(entities.EmployerAddress);
      }
      else
      {
        ExitState = "EMPLOYER_ADDRESS_NF";

        return;
      }
    }
    else
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    export.Hist.Index = -1;
    export.Hist.Count = 0;

    foreach(var item in ReadEmployerHistoryEmployerHistoryDetail())
    {
      ++export.Hist.Index;
      export.Hist.CheckSize();

      export.Hist.Update.EmployerHistory.Assign(entities.EmployerHistory);
      export.Hist.Update.EmployerHistoryDetail.LineNumber =
        entities.EmployerHistoryDetail.LineNumber;

      if (Equal(entities.EmployerHistory.CreatedTimestamp,
        local.CurrentEmployerHistory.CreatedTimestamp))
      {
        export.Hist.Update.Data.Text80 =
          entities.EmployerHistoryDetail.Change ?? Spaces(80);
      }
      else
      {
        if (Lt(local.Null1.Timestamp,
          local.CurrentEmployerHistory.CreatedTimestamp))
        {
          export.Hist.Update.StartStop.Flag = "N";
          export.Hist.Update.Data.Text80 =
            local.CurrentEmployerHistory.Note ?? Spaces(80);
          export.Hist.Update.EmployerHistory.
            Assign(local.CurrentEmployerHistory);

          if (export.Hist.IsFull)
          {
            return;
          }

          ++export.Hist.Index;
          export.Hist.CheckSize();

          export.Hist.Update.EmployerHistory.Assign(entities.EmployerHistory);
          export.Hist.Update.EmployerHistoryDetail.LineNumber =
            entities.EmployerHistoryDetail.LineNumber;
        }

        export.Hist.Update.StartStop.Flag = "Y";
        local.CurrentDateWorkArea.Date = entities.EmployerHistory.ActionDate;
        UseCabDate2TextWithHyphens();
        export.Hist.Update.Data.Text80 =
          entities.EmployerHistory.ActionTaken + "  " + local
          .EffectDate.Text10 + "  " + entities.EmployerHistory.LastUpdatedBy;

        if (export.Hist.IsFull)
        {
          return;
        }

        ++export.Hist.Index;
        export.Hist.CheckSize();

        export.Hist.Update.Data.Text80 =
          entities.EmployerHistoryDetail.Change ?? Spaces(80);
        export.Hist.Update.EmployerHistory.Assign(entities.EmployerHistory);
        export.Hist.Update.EmployerHistoryDetail.LineNumber =
          entities.EmployerHistoryDetail.LineNumber;
      }

      local.CurrentEmployerHistory.Assign(entities.EmployerHistory);

      if (export.Hist.IsFull)
      {
        break;
      }
    }

    if (!export.Hist.IsEmpty && !export.Hist.IsFull)
    {
      ++export.Hist.Index;
      export.Hist.CheckSize();

      export.Hist.Update.EmployerHistory.Assign(entities.EmployerHistory);
      export.Hist.Update.EmployerHistoryDetail.LineNumber =
        entities.EmployerHistoryDetail.LineNumber;
      export.Hist.Update.StartStop.Flag = "N";
      export.Hist.Update.Data.Text80 = local.CurrentEmployerHistory.Note ?? Spaces
        (80);
    }

    if (export.Hist.IsEmpty)
    {
      ExitState = "EMPLOYER_RELATION_NF";
    }
  }

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.CurrentDateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.EffectDate.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private bool ReadEmployer()
  {
    entities.Ws.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Ws.Identifier);
      },
      (db, reader) =>
      {
        entities.Ws.Identifier = db.GetInt32(reader, 0);
        entities.Ws.Ein = db.GetNullableString(reader, 1);
        entities.Ws.Name = db.GetNullableString(reader, 2);
        entities.Ws.Populated = true;
      });
  }

  private bool ReadEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Ws.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 1);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 2);
        entities.EmployerAddress.City = db.GetNullableString(reader, 3);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 4);
        entities.EmployerAddress.State = db.GetNullableString(reader, 5);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 8);
        entities.EmployerAddress.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadEmployerHistoryEmployerHistoryDetail()
  {
    entities.EmployerHistoryDetail.Populated = false;
    entities.EmployerHistory.Populated = false;

    return ReadEach("ReadEmployerHistoryEmployerHistoryDetail",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Ws.Identifier);
        db.SetDateTime(
          command, "createdTmst",
          import.Scroll.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EmployerHistory.ActionTaken = db.GetNullableString(reader, 0);
        entities.EmployerHistory.ActionDate = db.GetDate(reader, 1);
        entities.EmployerHistory.Note = db.GetNullableString(reader, 2);
        entities.EmployerHistory.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.EmployerHistoryDetail.EhxCreatedTmst =
          db.GetDateTime(reader, 3);
        entities.EmployerHistory.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.EmployerHistory.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.EmployerHistory.EmpId = db.GetInt32(reader, 6);
        entities.EmployerHistoryDetail.EmpId = db.GetInt32(reader, 6);
        entities.EmployerHistoryDetail.LineNumber = db.GetInt32(reader, 7);
        entities.EmployerHistoryDetail.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.EmployerHistoryDetail.Change = db.GetNullableString(reader, 9);
        entities.EmployerHistoryDetail.Populated = true;
        entities.EmployerHistory.Populated = true;

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
    /// <summary>
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public EmployerHistory Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
    }

    /// <summary>
    /// A value of Ws.
    /// </summary>
    [JsonPropertyName("ws")]
    public Employer Ws
    {
      get => ws ??= new();
      set => ws = value;
    }

    private EmployerHistory scroll;
    private Employer ws;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HistGroup group.</summary>
    [Serializable]
    public class HistGroup
    {
      /// <summary>
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of Data.
      /// </summary>
      [JsonPropertyName("data")]
      public WorkArea Data
      {
        get => data ??= new();
        set => data = value;
      }

      /// <summary>
      /// A value of EmployerHistory.
      /// </summary>
      [JsonPropertyName("employerHistory")]
      public EmployerHistory EmployerHistory
      {
        get => employerHistory ??= new();
        set => employerHistory = value;
      }

      /// <summary>
      /// A value of EmployerHistoryDetail.
      /// </summary>
      [JsonPropertyName("employerHistoryDetail")]
      public EmployerHistoryDetail EmployerHistoryDetail
      {
        get => employerHistoryDetail ??= new();
        set => employerHistoryDetail = value;
      }

      /// <summary>
      /// A value of StartStop.
      /// </summary>
      [JsonPropertyName("startStop")]
      public Common StartStop
      {
        get => startStop ??= new();
        set => startStop = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common select;
      private WorkArea data;
      private EmployerHistory employerHistory;
      private EmployerHistoryDetail employerHistoryDetail;
      private Common startStop;
    }

    /// <summary>
    /// A value of WorksiteEmployerAddress.
    /// </summary>
    [JsonPropertyName("worksiteEmployerAddress")]
    public EmployerAddress WorksiteEmployerAddress
    {
      get => worksiteEmployerAddress ??= new();
      set => worksiteEmployerAddress = value;
    }

    /// <summary>
    /// A value of WorksiteEmployer.
    /// </summary>
    [JsonPropertyName("worksiteEmployer")]
    public Employer WorksiteEmployer
    {
      get => worksiteEmployer ??= new();
      set => worksiteEmployer = value;
    }

    /// <summary>
    /// Gets a value of Hist.
    /// </summary>
    [JsonIgnore]
    public Array<HistGroup> Hist => hist ??= new(HistGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Hist for json serialization.
    /// </summary>
    [JsonPropertyName("hist")]
    [Computed]
    public IList<HistGroup> Hist_Json
    {
      get => hist;
      set => Hist.Assign(value);
    }

    private EmployerAddress worksiteEmployerAddress;
    private Employer worksiteEmployer;
    private Array<HistGroup> hist;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EmployerHistoryDetail.
    /// </summary>
    [JsonPropertyName("employerHistoryDetail")]
    public EmployerHistoryDetail EmployerHistoryDetail
    {
      get => employerHistoryDetail ??= new();
      set => employerHistoryDetail = value;
    }

    /// <summary>
    /// A value of CurrentEmployerHistory.
    /// </summary>
    [JsonPropertyName("currentEmployerHistory")]
    public EmployerHistory CurrentEmployerHistory
    {
      get => currentEmployerHistory ??= new();
      set => currentEmployerHistory = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

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
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of EffectDate.
    /// </summary>
    [JsonPropertyName("effectDate")]
    public TextWorkArea EffectDate
    {
      get => effectDate ??= new();
      set => effectDate = value;
    }

    private EmployerHistoryDetail employerHistoryDetail;
    private EmployerHistory currentEmployerHistory;
    private DateWorkArea max;
    private DateWorkArea null1;
    private DateWorkArea currentDateWorkArea;
    private TextWorkArea effectDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EmployerHistoryDetail.
    /// </summary>
    [JsonPropertyName("employerHistoryDetail")]
    public EmployerHistoryDetail EmployerHistoryDetail
    {
      get => employerHistoryDetail ??= new();
      set => employerHistoryDetail = value;
    }

    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Ws.
    /// </summary>
    [JsonPropertyName("ws")]
    public Employer Ws
    {
      get => ws ??= new();
      set => ws = value;
    }

    private EmployerHistoryDetail employerHistoryDetail;
    private EmployerHistory employerHistory;
    private EmployerAddress employerAddress;
    private Employer ws;
  }
#endregion
}
