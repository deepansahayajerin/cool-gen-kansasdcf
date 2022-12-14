// Program: OE_LURA_GET_ADJ_PERSON_CASE_DT, ID: 374459525, model: 746.
// Short name: SWE02538
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_LURA_GET_ADJ_PERSON_CASE_DT.
/// </para>
/// <para>
/// Populates the group view with adjustments for the given case
/// for the person and for the date range.
/// </para>
/// </summary>
[Serializable]
public partial class OeLuraGetAdjPersonCaseDt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LURA_GET_ADJ_PERSON_CASE_DT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLuraGetAdjPersonCaseDt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLuraGetAdjPersonCaseDt.
  /// </summary>
  public OeLuraGetAdjPersonCaseDt(IContext context, Import import, Export export)
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
    // ********************** MAINTENANCE LOG **********************
    //  AUTHOR         DATE         CHG REQ#       DESCRIPTION
    // Madhu Kumar   05/10/00                     Initial Code.
    // Fangman          08/09/00    Changed read to sort by desc YY & MM, moved 
    // call to adabas outside the loop.  Cleaned up some of the views.
    // *************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);

    if (!IsEmpty(import.CsePersonsWorkSet.Number))
    {
      if (ReadCsePerson())
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.CsePersonsWorkSet.FormattedName =
            "Name not available from Adabas.";
        }
        else
        {
          export.CsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;
        }
      }
      else
      {
        export.ImHouseholdMbrMnthlySum.Relationship = "";
        export.CsePersonsWorkSet.FormattedName = "";
        ExitState = "OE0000_CSE_PERSON_NF";

        return;
      }
    }
    else
    {
      export.ImHouseholdMbrMnthlySum.Relationship = "";
      export.CsePersonsWorkSet.FormattedName = "";

      return;
    }

    if (!IsEmpty(import.CsePersonsWorkSet.Number))
    {
      if (ReadCsePersonImHouseholdImHouseholdMbrMnthlySum())
      {
        export.ImHouseholdMbrMnthlySum.Relationship =
          entities.ImHouseholdMbrMnthlySum.Relationship;
      }
      else
      {
        export.ImHouseholdMbrMnthlySum.Relationship = "";
        ExitState = "OE0000_RELATIONSHIP_NF";

        return;
      }
    }
    else
    {
      export.ImHouseholdMbrMnthlySum.Relationship = "";
      export.CsePersonsWorkSet.FormattedName = "";
    }

    export.Group.Index = 0;
    export.Group.Clear();

    foreach(var item in ReadCsePersonImHouseholdImHouseholdMbrMnthlyAdj())
    {
      export.Group.Update.GimHouseholdMbrMnthlySum.Month =
        entities.ImHouseholdMbrMnthlySum.Month;
      export.Group.Update.GimHouseholdMbrMnthlySum.Year =
        entities.ImHouseholdMbrMnthlySum.Year;
      export.Group.Update.GadjustmentYrMnth.YearMonth =
        export.Group.Item.GimHouseholdMbrMnthlySum.Year * 100 + export
        .Group.Item.GimHouseholdMbrMnthlySum.Month;
      export.Group.Update.GimHouseholdMbrMnthlyAdj.AdjustmentAmount =
        entities.ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
      export.Group.Update.GimHouseholdMbrMnthlyAdj.Type1 =
        entities.ImHouseholdMbrMnthlyAdj.Type1;
      export.Group.Update.GimHouseholdMbrMnthlyAdj.LevelAppliedTo =
        entities.ImHouseholdMbrMnthlyAdj.LevelAppliedTo;
      export.Group.Update.GimHouseholdMbrMnthlyAdj.CreatedBy =
        entities.ImHouseholdMbrMnthlyAdj.CreatedBy;
      export.Group.Update.GimHouseholdMbrMnthlyAdj.CreatedTmst =
        entities.ImHouseholdMbrMnthlyAdj.CreatedTmst;
      export.Group.Update.GcsePersonsWorkSet.Number = entities.CsePerson.Number;
      export.Group.Update.GcsePersonsWorkSet.FormattedName =
        export.CsePersonsWorkSet.FormattedName;
      export.Group.Update.GimHouseholdMbrMnthlyAdj.AdjustmentReason =
        entities.ImHouseholdMbrMnthlyAdj.AdjustmentReason;
      export.Group.Next();
    }

    if (export.Group.IsEmpty)
    {
      ExitState = "OE0000_NO_URA_ADJUSTMENTS_FOUND";
    }

    if (export.Group.IsFull)
    {
      ExitState = "ACO_NI0000_LIST_IS_FULL";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonImHouseholdImHouseholdMbrMnthlyAdj()
  {
    return ReadEach("ReadCsePersonImHouseholdImHouseholdMbrMnthlyAdj",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetString(command, "imhAeCaseNo", import.ImHousehold.AeCaseNo);
        db.SetInt32(command, "yearMonth1", import.From.YearMonth);
        db.SetInt32(command, "yearMonth2", import.To.YearMonth);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.ImHouseholdMbrMnthlyAdj.CspNumber = db.GetString(reader, 0);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 0);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 0);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 1);
        entities.ImHouseholdMbrMnthlyAdj.ImhAeCaseNo = db.GetString(reader, 1);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 1);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 1);
        entities.ImHouseholdMbrMnthlyAdj.Type1 = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlyAdj.AdjustmentAmount =
          db.GetDecimal(reader, 3);
        entities.ImHouseholdMbrMnthlyAdj.LevelAppliedTo =
          db.GetString(reader, 4);
        entities.ImHouseholdMbrMnthlyAdj.CreatedBy = db.GetString(reader, 5);
        entities.ImHouseholdMbrMnthlyAdj.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.ImHouseholdMbrMnthlyAdj.ImsMonth = db.GetInt32(reader, 7);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 7);
        entities.ImHouseholdMbrMnthlyAdj.ImsYear = db.GetInt32(reader, 8);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 8);
        entities.ImHouseholdMbrMnthlyAdj.AdjustmentReason =
          db.GetString(reader, 9);
        entities.ImHouseholdMbrMnthlySum.Relationship =
          db.GetString(reader, 10);
        entities.ImHousehold.Populated = true;
        entities.CsePerson.Populated = true;
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.ImHouseholdMbrMnthlyAdj.Populated = true;

        return true;
      });
  }

  private bool ReadCsePersonImHouseholdImHouseholdMbrMnthlySum()
  {
    entities.ImHousehold.Populated = false;
    entities.CsePerson.Populated = false;
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadCsePersonImHouseholdImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", import.ImHousehold.AeCaseNo);
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 0);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 1);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 2);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 3);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 4);
        entities.ImHousehold.Populated = true;
        entities.CsePerson.Populated = true;
        entities.ImHouseholdMbrMnthlySum.Populated = true;
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
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
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

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    private DateWorkArea to;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ImHousehold imHousehold;
    private DateWorkArea from;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
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
      /// A value of GadjustmentYrMnth.
      /// </summary>
      [JsonPropertyName("gadjustmentYrMnth")]
      public DateWorkArea GadjustmentYrMnth
      {
        get => gadjustmentYrMnth ??= new();
        set => gadjustmentYrMnth = value;
      }

      /// <summary>
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GimHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("gimHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum GimHouseholdMbrMnthlySum
      {
        get => gimHouseholdMbrMnthlySum ??= new();
        set => gimHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of GimHouseholdMbrMnthlyAdj.
      /// </summary>
      [JsonPropertyName("gimHouseholdMbrMnthlyAdj")]
      public ImHouseholdMbrMnthlyAdj GimHouseholdMbrMnthlyAdj
      {
        get => gimHouseholdMbrMnthlyAdj ??= new();
        set => gimHouseholdMbrMnthlyAdj = value;
      }

      /// <summary>
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private DateWorkArea gadjustmentYrMnth;
      private Common gcommon;
      private ImHouseholdMbrMnthlySum gimHouseholdMbrMnthlySum;
      private ImHouseholdMbrMnthlyAdj gimHouseholdMbrMnthlyAdj;
      private CsePersonsWorkSet gcsePersonsWorkSet;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    private Array<GroupGroup> group;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
    }

    private ImHousehold imHousehold;
    private CsePerson csePerson;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
  }
#endregion
}
