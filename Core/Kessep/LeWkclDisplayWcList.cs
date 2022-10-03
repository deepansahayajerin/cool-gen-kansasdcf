// Program: LE_WKCL_DISPLAY_WC_LIST, ID: 1625339662, model: 746.
// Short name: SWE00847
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_WKCL_DISPLAY_WC_LIST.
/// </summary>
[Serializable]
public partial class LeWkclDisplayWcList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_WKCL_DISPLAY_WC_LIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeWkclDisplayWcList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeWkclDisplayWcList.
  /// </summary>
  public LeWkclDisplayWcList(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 12/01/16  GVandy	CQ51923		Initial Code.
    // 6/17/19   AHockman      CQ51923 part II  changing display to descending/ 
    // adding filter
    // -------------------------------------------------------------------------------------
    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.CsePerson.Number;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
    UseSiReadCsePerson();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.Export1.Index = -1;

    foreach(var item in ReadWorkersCompClaim())
    {
      switch(AsChar(import.DocketFilter.Text1))
      {
        case 'D':
          if (IsEmpty(entities.WorkersCompClaim.DocketNumber))
          {
            continue;
          }

          break;
        case 'U':
          if (!IsEmpty(entities.WorkersCompClaim.DocketNumber))
          {
            continue;
          }

          break;
        default:
          // fall through if neither D or U are chosen BLANK gets ALL docket 
          // items & accounts for filter being left blank
          break;
      }

      if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
      {
        MoveWorkersCompClaim(entities.WorkersCompClaim, export.NextPage);

        return;
      }
      else
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.GworkersCompClaim.
          Assign(entities.WorkersCompClaim);
      }

      if (!IsEmpty(export.Export1.Item.GworkersCompClaim.DocketNumber))
      {
        export.Export1.Update.GexportDocket.Text1 = "D";
      }
      else
      {
        export.Export1.Update.GexportDocket.Text1 = "U";
      }
    }
  }

  private static void MoveWorkersCompClaim(WorkersCompClaim source,
    WorkersCompClaim target)
  {
    target.DateOfLoss = source.DateOfLoss;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadWorkersCompClaim()
  {
    entities.WorkersCompClaim.Populated = false;

    return ReadEach("ReadWorkersCompClaim",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "lossDate", import.Starting.DateOfLoss.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.Starting.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.WorkersCompClaim.CspNumber = db.GetString(reader, 0);
        entities.WorkersCompClaim.Identifier = db.GetInt32(reader, 1);
        entities.WorkersCompClaim.DocketNumber =
          db.GetNullableString(reader, 2);
        entities.WorkersCompClaim.InsurerName = db.GetNullableString(reader, 3);
        entities.WorkersCompClaim.DateOfLoss = db.GetNullableDate(reader, 4);
        entities.WorkersCompClaim.AdministrativeClaimNo =
          db.GetNullableString(reader, 5);
        entities.WorkersCompClaim.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.WorkersCompClaim.Populated = true;

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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public WorkersCompClaim Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of DocketFilter.
    /// </summary>
    [JsonPropertyName("docketFilter")]
    public WorkArea DocketFilter
    {
      get => docketFilter ??= new();
      set => docketFilter = value;
    }

    private WorkersCompClaim starting;
    private CsePerson csePerson;
    private WorkArea docketFilter;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
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
      /// A value of GworkersCompClaim.
      /// </summary>
      [JsonPropertyName("gworkersCompClaim")]
      public WorkersCompClaim GworkersCompClaim
      {
        get => gworkersCompClaim ??= new();
        set => gworkersCompClaim = value;
      }

      /// <summary>
      /// A value of GexportDocket.
      /// </summary>
      [JsonPropertyName("gexportDocket")]
      public WorkArea GexportDocket
      {
        get => gexportDocket ??= new();
        set => gexportDocket = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Common gcommon;
      private WorkersCompClaim gworkersCompClaim;
      private WorkArea gexportDocket;
    }

    /// <summary>
    /// A value of DocketFilter.
    /// </summary>
    [JsonPropertyName("docketFilter")]
    public WorkArea DocketFilter
    {
      get => docketFilter ??= new();
      set => docketFilter = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of NextPage.
    /// </summary>
    [JsonPropertyName("nextPage")]
    public WorkersCompClaim NextPage
    {
      get => nextPage ??= new();
      set => nextPage = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private WorkArea docketFilter;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private WorkersCompClaim nextPage;
    private Array<ExportGroup> export1;
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
    /// A value of WorkersCompClaim.
    /// </summary>
    [JsonPropertyName("workersCompClaim")]
    public WorkersCompClaim WorkersCompClaim
    {
      get => workersCompClaim ??= new();
      set => workersCompClaim = value;
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

    private WorkersCompClaim workersCompClaim;
    private CsePerson csePerson;
  }
#endregion
}
