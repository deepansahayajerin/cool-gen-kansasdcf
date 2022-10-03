// Program: OE_GTSC_GET_CSE_NUMBER, ID: 371797163, model: 746.
// Short name: SWE01896
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
/// A program: OE_GTSC_GET_CSE_NUMBER.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This BSD common action block reads and populates genetic test details for a 
/// father-mother-child combination for display.
/// </para>
/// </summary>
[Serializable]
public partial class OeGtscGetCseNumber: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_GET_CSE_NUMBER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscGetCseNumber(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscGetCseNumber.
  /// </summary>
  public OeGtscGetCseNumber(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR		DATE	CHGREQ	DESCRIPTION
    // raju		1-21-97	Initial coding
    // 			1430 hrs CST
    // A.Kinney	4-30-97	Changed Current_Date
    // *********************************************
    // ******** SPECIAL MAINTENANCE ********************
    // AUTHOR  DATE  		DESCRIPTION
    // R. Jean	07/09/99	Singleton reads changed to select only
    // ******* END MAINTENANCE LOG ****************
    // PR136851 on 2-5-2002 by LBachura. Added start and end checks for case 3. 
    // This eliminated the abend when going from HIST to GTSC when the child was
    // on the case with more than one role entry, ie one open and one closed.
    // PR340828 CQ6222  9/5/2008   LSS
    // On CASE 3  -  added qualifer for case and for case_role to eliminate the 
    // abend
    // when flow from HIST to GTSC when child is on more than one case with same
    // role.
    local.Current.Date = Now().Date;

    if (ReadCaseUnit())
    {
      for(export.Export1.Index = 0; export.Export1.Index < 3; ++
        export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        switch(export.Export1.Index + 1)
        {
          case 1:
            if (ReadCsePerson1())
            {
              export.Export1.Update.DetailCsePerson.Number =
                entities.CsePerson.Number;

              if (ReadCaseRole1())
              {
                export.Export1.Update.DetailCaseRole.Assign(entities.CaseRole);
              }
            }

            break;
          case 2:
            if (ReadCaseRoleCsePerson())
            {
              export.Export1.Update.DetailCsePerson.Number =
                entities.CsePerson.Number;
              export.Export1.Update.DetailCaseRole.Assign(entities.CaseRole);
            }

            break;
          case 3:
            if (ReadCsePerson2())
            {
              export.Export1.Update.DetailCsePerson.Number =
                entities.CsePerson.Number;

              // PR340828 CQ6222  9/5/2008   LSS
              // Added qualifer for case and for case_role to get just one row 
              // when child is on more than one case
              // with same role and eliminate the abend when flow from HIST to 
              // GTSC  (with selection of a GENTEST
              // record on HISTand then PF15 DETAILS).
              if (ReadCaseRole2())
              {
                export.Export1.Update.DetailCaseRole.Assign(entities.CaseRole);
              }
            }

            break;
          default:
            break;
        }
      }

      export.Export1.CheckIndex();
    }
  }

  private bool ReadCaseRole1()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetInt32(command, "cuNumber", import.CaseUnit.CuNumber);
        db.SetString(command, "casNo", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 3);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoAp ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoChild ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CaseUnit caseUnit;
    private Case1 case1;
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
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
      }

      /// <summary>
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private CsePerson detailCsePerson;
      private CaseRole detailCaseRole;
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

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CaseRole caseRole;
    private CsePerson csePerson;
    private CaseUnit caseUnit;
    private Case1 case1;
  }
#endregion
}
