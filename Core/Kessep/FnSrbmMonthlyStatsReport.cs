// Program: FN_SRBM_MONTHLY_STATS_REPORT, ID: 373366342, model: 746.
// Short name: SWESRBMP
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
/// A program: FN_SRBM_MONTHLY_STATS_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnSrbmMonthlyStatsReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SRBM_MONTHLY_STATS_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSrbmMonthlyStatsReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSrbmMonthlyStatsReport.
  /// </summary>
  public FnSrbmMonthlyStatsReport(IContext context, Import import, Export export)
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

    export.StatsVerifi.Assign(import.StatsVerifi);
    export.From.Assign(import.From);
    export.To.Assign(import.To);
    export.SortBy.SelectChar = import.SortBy.SelectChar;

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        break;
      case "DISPLAY":
        if (export.StatsVerifi.YearMonth.GetValueOrDefault() == 0)
        {
          var field = GetField(export.StatsVerifi, "yearMonth");

          field.Error = true;

          ExitState = "FN0000_MANDATORY_FIELDS";
        }

        if (IsEmpty(export.SortBy.SelectChar))
        {
          export.SortBy.SelectChar = "C";
        }

        if (AsChar(export.SortBy.SelectChar) != 'S' && AsChar
          (export.SortBy.SelectChar) != 'C')
        {
          var field = GetField(export.SortBy, "selectChar");

          field.Error = true;

          ExitState = "FN0000_ENTER_C_OR_S";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (export.StatsVerifi.FirstRunNumber.GetValueOrDefault() == 0)
        {
          export.StatsVerifi.FirstRunNumber = 1;
        }

        if (IsEmpty(export.From.SuppPersonNumber))
        {
          export.From.SuppPersonNumber = "0000000000";
        }

        if (IsEmpty(export.To.SuppPersonNumber))
        {
          export.To.SuppPersonNumber = "9999999999";
        }

        if (IsEmpty(export.From.ObligorPersonNbr))
        {
          export.From.ObligorPersonNbr = "0000000000";
        }

        if (IsEmpty(export.To.ObligorPersonNbr))
        {
          export.To.ObligorPersonNbr = "9999999999";
        }

        if (IsEmpty(export.From.CaseNumber))
        {
          export.From.CaseNumber = "0000000000";
        }

        if (IsEmpty(export.To.CaseNumber))
        {
          export.To.CaseNumber = "9999999999";
        }

        if (export.To.OfficeId.GetValueOrDefault() == 0)
        {
          export.To.OfficeId = 9999;
        }

        if (export.To.LineNumber.GetValueOrDefault() == 0)
        {
          export.To.LineNumber = 99;
        }

        if (export.To.ServicePrvdrId.GetValueOrDefault() == 0)
        {
          export.To.ServicePrvdrId = 99999;
        }

        if (export.To.ParentId.GetValueOrDefault() == 0)
        {
          export.To.ParentId = 99999;
        }

        local.TextWorkArea.Text10 = export.From.CaseNumber ?? Spaces(10);
        UseEabPadLeftWithZeros();
        export.From.CaseNumber = local.TextWorkArea.Text10;
        local.TextWorkArea.Text10 = export.To.CaseNumber ?? Spaces(10);
        UseEabPadLeftWithZeros();
        export.To.CaseNumber = local.TextWorkArea.Text10;
        local.TextWorkArea.Text10 = export.From.SuppPersonNumber ?? Spaces(10);
        UseEabPadLeftWithZeros();
        export.From.SuppPersonNumber = local.TextWorkArea.Text10;
        local.TextWorkArea.Text10 = export.To.SuppPersonNumber ?? Spaces(10);
        UseEabPadLeftWithZeros();
        export.To.SuppPersonNumber = local.TextWorkArea.Text10;
        local.TextWorkArea.Text10 = export.From.ObligorPersonNbr ?? Spaces(10);
        UseEabPadLeftWithZeros();
        export.From.ObligorPersonNbr = local.TextWorkArea.Text10;
        local.TextWorkArea.Text10 = export.To.ObligorPersonNbr ?? Spaces(10);
        UseEabPadLeftWithZeros();
        export.To.ObligorPersonNbr = local.TextWorkArea.Text10;

        switch(AsChar(export.SortBy.SelectChar))
        {
          case 'S':
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadStatsVerifi2())
            {
              MoveStatsVerifi(entities.StatsVerifi,
                export.Group.Update.StatsVerifi);
              export.Group.Next();
            }

            break;
          case 'C':
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadStatsVerifi1())
            {
              MoveStatsVerifi(entities.StatsVerifi,
                export.Group.Update.StatsVerifi);
              export.Group.Next();
            }

            break;
          default:
            break;
        }

        if (Equal(export.From.SuppPersonNumber, "0000000000"))
        {
          export.From.SuppPersonNumber = "";
        }

        if (Equal(export.To.SuppPersonNumber, "9999999999"))
        {
          export.To.SuppPersonNumber = "";
        }

        if (Equal(export.From.ObligorPersonNbr, "0000000000"))
        {
          export.From.ObligorPersonNbr = "";
        }

        if (Equal(export.To.ObligorPersonNbr, "9999999999"))
        {
          export.To.ObligorPersonNbr = "";
        }

        if (Equal(export.From.CaseNumber, "0000000000"))
        {
          export.From.CaseNumber = "";
        }

        if (Equal(export.To.CaseNumber, "9999999999"))
        {
          export.To.CaseNumber = "";
        }

        if (export.To.OfficeId.GetValueOrDefault() == 9999)
        {
          export.To.OfficeId = 0;
        }

        if (export.To.LineNumber.GetValueOrDefault() == 99)
        {
          export.To.LineNumber = 0;
        }

        if (export.To.ServicePrvdrId.GetValueOrDefault() == 99999)
        {
          export.To.ServicePrvdrId = 0;
        }

        if (export.To.ParentId.GetValueOrDefault() == 99999)
        {
          export.To.ParentId = 0;
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

  private static void MoveStatsVerifi(StatsVerifi source, StatsVerifi target)
  {
    target.LineNumber = source.LineNumber;
    target.ProgramType = source.ProgramType;
    target.ServicePrvdrId = source.ServicePrvdrId;
    target.OfficeId = source.OfficeId;
    target.ParentId = source.ParentId;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.TranAmount = source.TranAmount;
    target.DebtDetailBaldue = source.DebtDetailBaldue;
    target.ObligationType = source.ObligationType;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.CollCreatedDate = source.CollCreatedDate;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadStatsVerifi1()
  {
    return ReadEach("ReadStatsVerifi1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "yearMonth",
          export.StatsVerifi.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "firstRunNumber",
          export.StatsVerifi.FirstRunNumber.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lineNumber1", export.From.LineNumber.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lineNumber2", export.To.LineNumber.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId1", export.From.OfficeId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId2", export.To.OfficeId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "servicePrvdrId1",
          export.From.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "servicePrvdrId2",
          export.To.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "parentId1", export.From.ParentId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "parentId2", export.To.ParentId.GetValueOrDefault());
        db.SetNullableString(
          command, "obligorPersonNbr1", export.From.ObligorPersonNbr ?? "");
        db.SetNullableString(
          command, "obligorPersonNbr2", export.To.ObligorPersonNbr ?? "");
        db.SetNullableString(
          command, "caseNumber1", export.From.CaseNumber ?? "");
        db.
          SetNullableString(command, "caseNumber2", export.To.CaseNumber ?? "");
          
        db.SetNullableString(
          command, "suppPersonNumber1", export.From.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber2", export.To.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "programType", export.StatsVerifi.ProgramType ?? "");
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.StatsVerifi.YearMonth = db.GetNullableInt32(reader, 0);
        entities.StatsVerifi.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.StatsVerifi.LineNumber = db.GetNullableInt32(reader, 2);
        entities.StatsVerifi.ProgramType = db.GetNullableString(reader, 3);
        entities.StatsVerifi.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.StatsVerifi.ServicePrvdrId = db.GetNullableInt32(reader, 5);
        entities.StatsVerifi.OfficeId = db.GetNullableInt32(reader, 6);
        entities.StatsVerifi.CaseWrkRole = db.GetNullableString(reader, 7);
        entities.StatsVerifi.ParentId = db.GetNullableInt32(reader, 8);
        entities.StatsVerifi.CaseNumber = db.GetNullableString(reader, 9);
        entities.StatsVerifi.SuppPersonNumber =
          db.GetNullableString(reader, 10);
        entities.StatsVerifi.ObligorPersonNbr =
          db.GetNullableString(reader, 11);
        entities.StatsVerifi.DatePaternityEst = db.GetNullableDate(reader, 12);
        entities.StatsVerifi.CourtOrderNumber =
          db.GetNullableString(reader, 13);
        entities.StatsVerifi.TranAmount = db.GetNullableDecimal(reader, 14);
        entities.StatsVerifi.Dddd = db.GetNullableDate(reader, 15);
        entities.StatsVerifi.DebtDetailBaldue = db.GetDecimal(reader, 16);
        entities.StatsVerifi.ObligationType = db.GetNullableString(reader, 17);
        entities.StatsVerifi.CollectionAmount =
          db.GetNullableDecimal(reader, 18);
        entities.StatsVerifi.CollectionDate = db.GetNullableDate(reader, 19);
        entities.StatsVerifi.CollCreatedDate = db.GetNullableDate(reader, 20);
        entities.StatsVerifi.CaseRoleType = db.GetNullableString(reader, 21);
        entities.StatsVerifi.CaseAsinEffDte = db.GetNullableDate(reader, 22);
        entities.StatsVerifi.CaseAsinEndDte = db.GetNullableDate(reader, 23);
        entities.StatsVerifi.PersonProgCode = db.GetNullableString(reader, 24);
        entities.StatsVerifi.Comment = db.GetNullableString(reader, 25);
        entities.StatsVerifi.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadStatsVerifi2()
  {
    return ReadEach("ReadStatsVerifi2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "yearMonth",
          export.StatsVerifi.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "firstRunNumber",
          export.StatsVerifi.FirstRunNumber.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lineNumber1", export.From.LineNumber.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lineNumber2", export.To.LineNumber.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId1", export.From.OfficeId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId2", export.To.OfficeId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "servicePrvdrId1",
          export.From.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "servicePrvdrId2",
          export.To.ServicePrvdrId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "parentId1", export.From.ParentId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "parentId2", export.To.ParentId.GetValueOrDefault());
        db.SetNullableString(
          command, "obligorPersonNbr1", export.From.ObligorPersonNbr ?? "");
        db.SetNullableString(
          command, "obligorPersonNbr2", export.To.ObligorPersonNbr ?? "");
        db.SetNullableString(
          command, "caseNumber1", export.From.CaseNumber ?? "");
        db.
          SetNullableString(command, "caseNumber2", export.To.CaseNumber ?? "");
          
        db.SetNullableString(
          command, "suppPersonNumber1", export.From.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber2", export.To.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "programType", export.StatsVerifi.ProgramType ?? "");
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.StatsVerifi.YearMonth = db.GetNullableInt32(reader, 0);
        entities.StatsVerifi.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.StatsVerifi.LineNumber = db.GetNullableInt32(reader, 2);
        entities.StatsVerifi.ProgramType = db.GetNullableString(reader, 3);
        entities.StatsVerifi.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.StatsVerifi.ServicePrvdrId = db.GetNullableInt32(reader, 5);
        entities.StatsVerifi.OfficeId = db.GetNullableInt32(reader, 6);
        entities.StatsVerifi.CaseWrkRole = db.GetNullableString(reader, 7);
        entities.StatsVerifi.ParentId = db.GetNullableInt32(reader, 8);
        entities.StatsVerifi.CaseNumber = db.GetNullableString(reader, 9);
        entities.StatsVerifi.SuppPersonNumber =
          db.GetNullableString(reader, 10);
        entities.StatsVerifi.ObligorPersonNbr =
          db.GetNullableString(reader, 11);
        entities.StatsVerifi.DatePaternityEst = db.GetNullableDate(reader, 12);
        entities.StatsVerifi.CourtOrderNumber =
          db.GetNullableString(reader, 13);
        entities.StatsVerifi.TranAmount = db.GetNullableDecimal(reader, 14);
        entities.StatsVerifi.Dddd = db.GetNullableDate(reader, 15);
        entities.StatsVerifi.DebtDetailBaldue = db.GetDecimal(reader, 16);
        entities.StatsVerifi.ObligationType = db.GetNullableString(reader, 17);
        entities.StatsVerifi.CollectionAmount =
          db.GetNullableDecimal(reader, 18);
        entities.StatsVerifi.CollectionDate = db.GetNullableDate(reader, 19);
        entities.StatsVerifi.CollCreatedDate = db.GetNullableDate(reader, 20);
        entities.StatsVerifi.CaseRoleType = db.GetNullableString(reader, 21);
        entities.StatsVerifi.CaseAsinEffDte = db.GetNullableDate(reader, 22);
        entities.StatsVerifi.CaseAsinEndDte = db.GetNullableDate(reader, 23);
        entities.StatsVerifi.PersonProgCode = db.GetNullableString(reader, 24);
        entities.StatsVerifi.Comment = db.GetNullableString(reader, 25);
        entities.StatsVerifi.Populated = true;

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
      /// A value of StatsVerifi.
      /// </summary>
      [JsonPropertyName("statsVerifi")]
      public StatsVerifi StatsVerifi
      {
        get => statsVerifi ??= new();
        set => statsVerifi = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private Common common;
      private StatsVerifi statsVerifi;
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
    /// A value of StatsVerifi.
    /// </summary>
    [JsonPropertyName("statsVerifi")]
    public StatsVerifi StatsVerifi
    {
      get => statsVerifi ??= new();
      set => statsVerifi = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public StatsVerifi To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public StatsVerifi From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of DisplaySkippedRecords.
    /// </summary>
    [JsonPropertyName("displaySkippedRecords")]
    public Common DisplaySkippedRecords
    {
      get => displaySkippedRecords ??= new();
      set => displaySkippedRecords = value;
    }

    private Array<GroupGroup> group;
    private StatsVerifi statsVerifi;
    private StatsVerifi to;
    private StatsVerifi from;
    private Common sortBy;
    private Common displaySkippedRecords;
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
      /// A value of StatsVerifi.
      /// </summary>
      [JsonPropertyName("statsVerifi")]
      public StatsVerifi StatsVerifi
      {
        get => statsVerifi ??= new();
        set => statsVerifi = value;
      }

      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private StatsVerifi statsVerifi;
      private Common common;
    }

    /// <summary>
    /// A value of StatsVerifi.
    /// </summary>
    [JsonPropertyName("statsVerifi")]
    public StatsVerifi StatsVerifi
    {
      get => statsVerifi ??= new();
      set => statsVerifi = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public StatsVerifi To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public StatsVerifi From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of DisplaySkippedRecords.
    /// </summary>
    [JsonPropertyName("displaySkippedRecords")]
    public Common DisplaySkippedRecords
    {
      get => displaySkippedRecords ??= new();
      set => displaySkippedRecords = value;
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

    private StatsVerifi statsVerifi;
    private StatsVerifi to;
    private StatsVerifi from;
    private Common sortBy;
    private Common displaySkippedRecords;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of StatsVerifi.
    /// </summary>
    [JsonPropertyName("statsVerifi")]
    public StatsVerifi StatsVerifi
    {
      get => statsVerifi ??= new();
      set => statsVerifi = value;
    }

    private StatsVerifi statsVerifi;
  }
#endregion
}
