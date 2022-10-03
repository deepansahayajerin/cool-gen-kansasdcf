// Program: SP_DELETE_DB_GOAL, ID: 945142411, model: 746.
// Short name: SWE03102
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_DB_GOAL.
/// </summary>
[Serializable]
public partial class SpDeleteDbGoal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_DB_GOAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteDbGoal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteDbGoal.
  /// </summary>
  public SpDeleteDbGoal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsEmpty(import.JdDashboardPerformanceMetrics.ReportLevelId))
    {
      if (ReadDashboardPerformanceMetrics())
      {
        DeleteDashboardPerformanceMetrics();
      }
      else
      {
        ExitState = "DASHBOARD_GOAL_NF";

        return;
      }

      if (ReadDashboardOutputMetrics1())
      {
        DeleteDashboardOutputMetrics1();
      }
      else
      {
        ExitState = "DASHBOARD_GOAL_NF";
      }
    }
    else if (ReadDashboardOutputMetrics2())
    {
      DeleteDashboardOutputMetrics2();
    }
    else
    {
      ExitState = "DASHBOARD_GOAL_NF";
    }
  }

  private void DeleteDashboardOutputMetrics1()
  {
    Update("DeleteDashboardOutputMetrics1",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth",
          entities.JdDashboardOutputMetrics.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.JdDashboardOutputMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.JdDashboardOutputMetrics.ReportLevelId);
        db.SetString(command, "type", entities.JdDashboardOutputMetrics.Type1);
      });
  }

  private void DeleteDashboardOutputMetrics2()
  {
    Update("DeleteDashboardOutputMetrics2",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", entities.Worker.ReportMonth);
        db.SetString(command, "reportLevel", entities.Worker.ReportLevel);
        db.SetString(command, "reportLevelId", entities.Worker.ReportLevelId);
        db.SetString(command, "type", entities.Worker.Type1);
      });
  }

  private void DeleteDashboardPerformanceMetrics()
  {
    Update("DeleteDashboardPerformanceMetrics",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth",
          entities.JdDashboardPerformanceMetrics.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.JdDashboardPerformanceMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.JdDashboardPerformanceMetrics.ReportLevelId);
        db.SetString(
          command, "type", entities.JdDashboardPerformanceMetrics.Type1);
      });
  }

  private bool ReadDashboardOutputMetrics1()
  {
    entities.JdDashboardOutputMetrics.Populated = false;

    return Read("ReadDashboardOutputMetrics1",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.JdDashboardOutputMetrics.ReportMonth);
        db.SetString(
          command, "reportLevel", import.JdDashboardOutputMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          import.JdDashboardOutputMetrics.ReportLevelId);
        db.SetString(command, "type", import.JdDashboardOutputMetrics.Type1);
      },
      (db, reader) =>
      {
        entities.JdDashboardOutputMetrics.ReportMonth = db.GetInt32(reader, 0);
        entities.JdDashboardOutputMetrics.ReportLevel = db.GetString(reader, 1);
        entities.JdDashboardOutputMetrics.ReportLevelId =
          db.GetString(reader, 2);
        entities.JdDashboardOutputMetrics.Type1 = db.GetString(reader, 3);
        entities.JdDashboardOutputMetrics.Populated = true;
      });
  }

  private bool ReadDashboardOutputMetrics2()
  {
    entities.Worker.Populated = false;

    return Read("ReadDashboardOutputMetrics2",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", import.Worker.ReportMonth);
        db.SetString(command, "reportLevel", import.Worker.ReportLevel);
        db.SetString(command, "reportLevelId", import.Worker.ReportLevelId);
        db.SetString(command, "type", import.Worker.Type1);
      },
      (db, reader) =>
      {
        entities.Worker.ReportMonth = db.GetInt32(reader, 0);
        entities.Worker.ReportLevel = db.GetString(reader, 1);
        entities.Worker.ReportLevelId = db.GetString(reader, 2);
        entities.Worker.Type1 = db.GetString(reader, 3);
        entities.Worker.Populated = true;
      });
  }

  private bool ReadDashboardPerformanceMetrics()
  {
    entities.JdDashboardPerformanceMetrics.Populated = false;

    return Read("ReadDashboardPerformanceMetrics",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth",
          import.JdDashboardPerformanceMetrics.ReportMonth);
        db.SetString(
          command, "reportLevel",
          import.JdDashboardPerformanceMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          import.JdDashboardPerformanceMetrics.ReportLevelId);
        db.
          SetString(command, "type", import.JdDashboardPerformanceMetrics.Type1);
          
      },
      (db, reader) =>
      {
        entities.JdDashboardPerformanceMetrics.ReportMonth =
          db.GetInt32(reader, 0);
        entities.JdDashboardPerformanceMetrics.ReportLevel =
          db.GetString(reader, 1);
        entities.JdDashboardPerformanceMetrics.ReportLevelId =
          db.GetString(reader, 2);
        entities.JdDashboardPerformanceMetrics.Type1 = db.GetString(reader, 3);
        entities.JdDashboardPerformanceMetrics.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of JdDashboardPerformanceMetrics.
    /// </summary>
    [JsonPropertyName("jdDashboardPerformanceMetrics")]
    public DashboardPerformanceMetrics JdDashboardPerformanceMetrics
    {
      get => jdDashboardPerformanceMetrics ??= new();
      set => jdDashboardPerformanceMetrics = value;
    }

    /// <summary>
    /// A value of JdDashboardOutputMetrics.
    /// </summary>
    [JsonPropertyName("jdDashboardOutputMetrics")]
    public DashboardOutputMetrics JdDashboardOutputMetrics
    {
      get => jdDashboardOutputMetrics ??= new();
      set => jdDashboardOutputMetrics = value;
    }

    /// <summary>
    /// A value of Worker.
    /// </summary>
    [JsonPropertyName("worker")]
    public DashboardOutputMetrics Worker
    {
      get => worker ??= new();
      set => worker = value;
    }

    private DashboardPerformanceMetrics jdDashboardPerformanceMetrics;
    private DashboardOutputMetrics jdDashboardOutputMetrics;
    private DashboardOutputMetrics worker;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Worker.
    /// </summary>
    [JsonPropertyName("worker")]
    public DashboardOutputMetrics Worker
    {
      get => worker ??= new();
      set => worker = value;
    }

    /// <summary>
    /// A value of JdDashboardPerformanceMetrics.
    /// </summary>
    [JsonPropertyName("jdDashboardPerformanceMetrics")]
    public DashboardPerformanceMetrics JdDashboardPerformanceMetrics
    {
      get => jdDashboardPerformanceMetrics ??= new();
      set => jdDashboardPerformanceMetrics = value;
    }

    /// <summary>
    /// A value of JdDashboardOutputMetrics.
    /// </summary>
    [JsonPropertyName("jdDashboardOutputMetrics")]
    public DashboardOutputMetrics JdDashboardOutputMetrics
    {
      get => jdDashboardOutputMetrics ??= new();
      set => jdDashboardOutputMetrics = value;
    }

    private DashboardOutputMetrics worker;
    private DashboardPerformanceMetrics jdDashboardPerformanceMetrics;
    private DashboardOutputMetrics jdDashboardOutputMetrics;
  }
#endregion
}
