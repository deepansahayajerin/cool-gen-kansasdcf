// Program: FN_SRDH_RUN160_STATS_RPT_HARNESS, ID: 373367486, model: 746.
// Short name: SWESRDHP
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
/// A program: FN_SRDH_RUN160_STATS_RPT_HARNESS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnSrdhRun160StatsRptHarness: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SRDH_RUN160_STATS_RPT_HARNESS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSrdhRun160StatsRptHarness(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSrdhRun160StatsRptHarness.
  /// </summary>
  public FnSrdhRun160StatsRptHarness(IContext context, Import import,
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
    // --------------------------------------------
    // Initial version - 4/2002
    // --------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    switch(TrimEnd(global.Command))
    {
      case "CLEAR":
        ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();
        ExitState = "ECO_XFR_TO_SIGNOFF_PROCEDURE";

        return;
      case "RETLINK":
        return;
      default:
        break;
    }

    export.StatsReport.Assign(import.StatsReport);

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        break;
      case "DISPLAY":
        if (export.StatsReport.ServicePrvdrId.GetValueOrDefault() > 0 && export
          .StatsReport.ParentId.GetValueOrDefault() > 0)
        {
          var field1 = GetField(export.StatsReport, "servicePrvdrId");

          field1.Error = true;

          var field2 = GetField(export.StatsReport, "parentId");

          field2.Error = true;

          ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";
        }

        if (export.StatsReport.YearMonth.GetValueOrDefault() == 0)
        {
          var field = GetField(export.StatsReport, "yearMonth");

          field.Error = true;

          ExitState = "FN0000_MANDATORY_FIELDS";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (export.StatsReport.FirstRunNumber.GetValueOrDefault() == 0)
        {
          export.StatsReport.FirstRunNumber = 1;
        }

        if (export.StatsReport.ServicePrvdrId.GetValueOrDefault() > 0)
        {
          local.DisplayBy.Text1 = "S";
        }
        else if (export.StatsReport.OfficeId.GetValueOrDefault() > 0)
        {
          local.DisplayBy.Text1 = "O";
        }
        else if (export.StatsReport.ParentId.GetValueOrDefault() > 0)
        {
          local.DisplayBy.Text1 = "P";
        }
        else
        {
          // **** State wide totals ****
        }

        switch(AsChar(local.DisplayBy.Text1))
        {
          case 'S':
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadStatsReport1())
            {
              if (export.StatsReport.OfficeId.GetValueOrDefault() > 0 && !
                Equal(export.StatsReport.OfficeId.GetValueOrDefault(),
                entities.StatsReport.OfficeId))
              {
                export.Group.Next();

                continue;
              }

              MoveStatsReport(entities.StatsReport,
                export.Group.Update.StatsReport);
              export.Group.Next();
            }

            break;
          case 'O':
            if (export.StatsReport.ParentId.GetValueOrDefault() > 0)
            {
              local.From.ParentId =
                export.StatsReport.ParentId.GetValueOrDefault();
              local.To.ParentId =
                export.StatsReport.ParentId.GetValueOrDefault();
            }
            else
            {
              local.From.ParentId = 0;
              local.To.ParentId = 99999;
            }

            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadStatsReport2())
            {
              export.Group.Next();
            }

            break;
          case 'P':
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadStatsReport3())
            {
              export.Group.Next();
            }

            break;
          default:
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadStatsReport4())
            {
              export.Group.Next();
            }

            break;
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";

          return;
        }

        if (export.Group.IsFull)
        {
          ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

          return;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      default:
        break;
    }
  }

  private static void MoveStatsReport(StatsReport source, StatsReport target)
  {
    target.LineNumber = source.LineNumber;
    target.ServicePrvdrId = source.ServicePrvdrId;
    target.OfficeId = source.OfficeId;
    target.ParentId = source.ParentId;
    target.Column1 = source.Column1;
    target.Column2 = source.Column2;
    target.Column3 = source.Column3;
    target.Column4 = source.Column4;
    target.Column5 = source.Column5;
    target.Column6 = source.Column6;
    target.Column7 = source.Column7;
    target.Column8 = source.Column8;
    target.Column9 = source.Column9;
    target.Column10 = source.Column10;
    target.Column11 = source.Column11;
    target.Column12 = source.Column12;
    target.Column13 = source.Column13;
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadStatsReport1()
  {
    return ReadEach("ReadStatsReport1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "yearMonth",
          export.StatsReport.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "servicePrvdrId",
          export.StatsReport.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "firstRunNumber",
          export.StatsReport.FirstRunNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.StatsReport.YearMonth = db.GetNullableInt32(reader, 0);
        entities.StatsReport.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.StatsReport.LineNumber = db.GetNullableInt32(reader, 2);
        entities.StatsReport.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.StatsReport.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.StatsReport.OfficeId = db.GetNullableInt32(reader, 5);
        entities.StatsReport.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.StatsReport.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.StatsReport.ParentId = db.GetNullableInt32(reader, 8);
        entities.StatsReport.ChiefId = db.GetNullableInt32(reader, 9);
        entities.StatsReport.Column1 = db.GetNullableInt64(reader, 10);
        entities.StatsReport.Column2 = db.GetNullableInt64(reader, 11);
        entities.StatsReport.Column3 = db.GetNullableInt64(reader, 12);
        entities.StatsReport.Column4 = db.GetNullableInt64(reader, 13);
        entities.StatsReport.Column5 = db.GetNullableInt64(reader, 14);
        entities.StatsReport.Column6 = db.GetNullableInt64(reader, 15);
        entities.StatsReport.Column7 = db.GetNullableInt64(reader, 16);
        entities.StatsReport.Column8 = db.GetNullableInt64(reader, 17);
        entities.StatsReport.Column9 = db.GetNullableInt64(reader, 18);
        entities.StatsReport.Column10 = db.GetNullableInt64(reader, 19);
        entities.StatsReport.Column11 = db.GetNullableInt64(reader, 20);
        entities.StatsReport.Column12 = db.GetNullableInt64(reader, 21);
        entities.StatsReport.Column13 = db.GetNullableInt64(reader, 22);
        entities.StatsReport.Column14 = db.GetNullableInt64(reader, 23);
        entities.StatsReport.Column15 = db.GetNullableInt64(reader, 24);
        entities.StatsReport.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadStatsReport2()
  {
    return ReadEach("ReadStatsReport2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "yearMonth",
          export.StatsReport.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId", export.StatsReport.OfficeId.GetValueOrDefault());
          
        db.SetNullableInt32(
          command, "parentId1", local.From.ParentId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "parentId2", local.To.ParentId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "firstRunNumber",
          export.StatsReport.FirstRunNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        export.Group.Update.StatsReport.LineNumber =
          db.GetNullableInt32(reader, 0);
        export.Group.Update.StatsReport.Column8 =
          db.GetNullableInt64(reader, 1);
        export.Group.Update.StatsReport.Column9 =
          db.GetNullableInt64(reader, 2);
        export.Group.Update.StatsReport.Column10 =
          db.GetNullableInt64(reader, 3);
        export.Group.Update.StatsReport.Column11 =
          db.GetNullableInt64(reader, 4);
        export.Group.Update.StatsReport.Column12 =
          db.GetNullableInt64(reader, 5);
        export.Group.Update.StatsReport.Column13 =
          db.GetNullableInt64(reader, 6);
        export.Group.Update.StatsReport.Column1 =
          db.GetNullableInt64(reader, 7);
        export.Group.Update.StatsReport.Column2 =
          db.GetNullableInt64(reader, 8);
        export.Group.Update.StatsReport.Column3 =
          db.GetNullableInt64(reader, 9);
        export.Group.Update.StatsReport.Column4 =
          db.GetNullableInt64(reader, 10);
        export.Group.Update.StatsReport.Column5 =
          db.GetNullableInt64(reader, 11);
        export.Group.Update.StatsReport.Column6 =
          db.GetNullableInt64(reader, 12);
        export.Group.Update.StatsReport.Column7 =
          db.GetNullableInt64(reader, 13);
        export.Group.Item.StatsReport.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadStatsReport3()
  {
    return ReadEach("ReadStatsReport3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "yearMonth",
          export.StatsReport.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "parentId", export.StatsReport.ParentId.GetValueOrDefault());
          
        db.SetNullableInt32(
          command, "firstRunNumber",
          export.StatsReport.FirstRunNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        export.Group.Update.StatsReport.LineNumber =
          db.GetNullableInt32(reader, 0);
        export.Group.Update.StatsReport.Column8 =
          db.GetNullableInt64(reader, 1);
        export.Group.Update.StatsReport.Column9 =
          db.GetNullableInt64(reader, 2);
        export.Group.Update.StatsReport.Column10 =
          db.GetNullableInt64(reader, 3);
        export.Group.Update.StatsReport.Column11 =
          db.GetNullableInt64(reader, 4);
        export.Group.Update.StatsReport.Column12 =
          db.GetNullableInt64(reader, 5);
        export.Group.Update.StatsReport.Column13 =
          db.GetNullableInt64(reader, 6);
        export.Group.Update.StatsReport.Column1 =
          db.GetNullableInt64(reader, 7);
        export.Group.Update.StatsReport.Column2 =
          db.GetNullableInt64(reader, 8);
        export.Group.Update.StatsReport.Column3 =
          db.GetNullableInt64(reader, 9);
        export.Group.Update.StatsReport.Column4 =
          db.GetNullableInt64(reader, 10);
        export.Group.Update.StatsReport.Column5 =
          db.GetNullableInt64(reader, 11);
        export.Group.Update.StatsReport.Column6 =
          db.GetNullableInt64(reader, 12);
        export.Group.Update.StatsReport.Column7 =
          db.GetNullableInt64(reader, 13);
        export.Group.Item.StatsReport.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadStatsReport4()
  {
    return ReadEach("ReadStatsReport4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "yearMonth",
          export.StatsReport.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "firstRunNumber",
          export.StatsReport.FirstRunNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        export.Group.Update.StatsReport.LineNumber =
          db.GetNullableInt32(reader, 0);
        export.Group.Update.StatsReport.Column8 =
          db.GetNullableInt64(reader, 1);
        export.Group.Update.StatsReport.Column9 =
          db.GetNullableInt64(reader, 2);
        export.Group.Update.StatsReport.Column10 =
          db.GetNullableInt64(reader, 3);
        export.Group.Update.StatsReport.Column11 =
          db.GetNullableInt64(reader, 4);
        export.Group.Update.StatsReport.Column12 =
          db.GetNullableInt64(reader, 5);
        export.Group.Update.StatsReport.Column13 =
          db.GetNullableInt64(reader, 6);
        export.Group.Update.StatsReport.Column1 =
          db.GetNullableInt64(reader, 7);
        export.Group.Update.StatsReport.Column2 =
          db.GetNullableInt64(reader, 8);
        export.Group.Update.StatsReport.Column3 =
          db.GetNullableInt64(reader, 9);
        export.Group.Update.StatsReport.Column4 =
          db.GetNullableInt64(reader, 10);
        export.Group.Update.StatsReport.Column5 =
          db.GetNullableInt64(reader, 11);
        export.Group.Update.StatsReport.Column6 =
          db.GetNullableInt64(reader, 12);
        export.Group.Update.StatsReport.Column7 =
          db.GetNullableInt64(reader, 13);
        export.Group.Item.StatsReport.Populated = true;

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
      /// A value of StatsReport.
      /// </summary>
      [JsonPropertyName("statsReport")]
      public StatsReport StatsReport
      {
        get => statsReport ??= new();
        set => statsReport = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private StatsReport statsReport;
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
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
    }

    private Array<GroupGroup> group;
    private StatsReport statsReport;
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
      /// A value of StatsReport.
      /// </summary>
      [JsonPropertyName("statsReport")]
      public StatsReport StatsReport
      {
        get => statsReport ??= new();
        set => statsReport = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private StatsReport statsReport;
    }

    /// <summary>
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
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

    private StatsReport statsReport;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public StatsReport To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public StatsReport From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of DisplayBy.
    /// </summary>
    [JsonPropertyName("displayBy")]
    public TextWorkArea DisplayBy
    {
      get => displayBy ??= new();
      set => displayBy = value;
    }

    private StatsReport to;
    private StatsReport from;
    private TextWorkArea displayBy;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
    }

    private StatsReport statsReport;
  }
#endregion
}
