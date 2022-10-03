// Program: SI_CADS_GET_GOOD_CAUSE_NON_COOP, ID: 371731800, model: 746.
// Short name: SWE01176
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
/// A program: SI_CADS_GET_GOOD_CAUSE_NON_COOP.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCadsGetGoodCauseNonCoop: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CADS_GET_GOOD_CAUSE_NON_COOP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCadsGetGoodCauseNonCoop(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCadsGetGoodCauseNonCoop.
  /// </summary>
  public SiCadsGetGoodCauseNonCoop(IContext context, Import import,
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
    // ---------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    //  Date	   Developer	Request #	Description
    // 12-20-95   Ken Evans	0		Initial Development
    // 02-02-96   P.ELIE	1		Developement
    // 05/08/96   G. Lofton			Rework
    // 04/29/97   JeHoward                     Current date fix.
    // 09/10/97   Sid				Rework DM changes.
    // ---------------------------------------------------------
    // 06/22/99 W.Campbell         Modified the properties
    //                             of 2 READ statements to
    //                             Select Only.   Also, added
    //                             case_role end date qualification
    //                             to one of the READ statements.
    // --------------------------------------------------------
    // 06/25/99 W.Campbell         Disabled a READ
    //                             statement as it was not
    //                             working correctly for a
    //                             closed case.
    // ---------------------------------------------------------
    // 06/25/99 W.Campbell         Added logic and
    //                             READ & READ EACH statements
    //                             to make READs work correctly
    //                             for either an OPEN or CLOSED
    //                             case.
    // ---------------------------------------------------------
    local.Current.Date = Now().Date;
    UseCabSetMaximumDiscontinueDate();
    export.Gc.Index = -1;
    export.Nc.Index = -1;

    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // ---------------------------------------------------------
    // 06/25/99 W.Campbell - Disabled the following
    // READ statement as it was not working
    // correctly for a closed case.
    // ---------------------------------------------------------
    // ---------------------------------------------------------
    // 06/25/99 W.Campbell - Added the following
    // logic and READ & READ EACH statements
    // to make READs work correctly for either
    // an OPEN or CLOSED case.
    // ---------------------------------------------------------
    // ---------------------------------------------------------
    // Check for an OPEN case status = "O".
    // ---------------------------------------------------------
    if (AsChar(entities.Case1.Status) == 'O')
    {
      // ---------------------------------------------------------
      // 06/25/99 W.Campbell - The properties
      // of the following READ statement
      // are set to Select Only.
      // ---------------------------------------------------------
      if (ReadCaseRole())
      {
        // ---------------------------------------------------------
        // Exit the READ EACH on the first one read.
        // ---------------------------------------------------------
      }

      if (!entities.Ar.Populated)
      {
        // ---------------------------------------------------------
        // If we didn't read any, then we have an error.
        // ---------------------------------------------------------
        ExitState = "AR_DB_ERROR_NF";

        return;
      }
    }
    else
    {
      // ---------------------------------------------------------
      // Closed case status = "C".
      // ---------------------------------------------------------
      if (ReadCaseRole())
      {
        // ---------------------------------------------------------
        // Exit the READ EACH on the first one read.
        // ---------------------------------------------------------
      }

      if (!entities.Ar.Populated)
      {
        // ---------------------------------------------------------
        // If we didn't read any, then we have an error.
        // ---------------------------------------------------------
        ExitState = "AR_DB_ERROR_NF";

        return;
      }
    }

    // ---------------------------------------------------------
    // 06/25/99 W.Campbell - End of logic
    // added to make READs work
    // correctly for either an OPEN
    // or CLOSED case.
    // ---------------------------------------------------------
    local.CompareDate.Date = local.Null1.Date;
    export.Gc.Index = -1;
    export.Gc.Count = 0;

    foreach(var item in ReadGoodCause())
    {
      if (Equal(entities.GoodCause.EffectiveDate, local.CompareDate.Date))
      {
        if (Equal(entities.GoodCause.Code, local.CompareGoodCause.Code))
        {
          continue;
        }
      }

      local.CompareDate.Date = entities.GoodCause.EffectiveDate;
      local.CompareGoodCause.Code = entities.GoodCause.Code;

      ++export.Gc.Index;
      export.Gc.CheckSize();

      MoveGoodCause(entities.GoodCause, export.Gc.Update.GcDetGoodCause);

      if (Equal(entities.GoodCause.Code, "CO"))
      {
        export.Gc.Update.GcDetGoodCause.Code = "NO";
      }

      if (ReadCaseRoleCsePerson1())
      {
        MoveCaseRole(entities.Ap, export.Gc.Update.GcDetApCaseRole);
        export.Gc.Update.GcDetApCsePersonsWorkSet.Number =
          entities.CsePerson.Number;
      }
    }

    export.Nc.Index = -1;
    export.Nc.Count = 0;
    local.CompareDate.Date = local.Null1.Date;

    foreach(var item in ReadNonCooperation())
    {
      if (Equal(entities.NonCooperation.EffectiveDate, local.CompareDate.Date))
      {
        if (AsChar(entities.NonCooperation.Code) == AsChar
          (local.CompareNonCooperation.Code))
        {
          continue;
        }
      }

      local.CompareDate.Date = entities.NonCooperation.EffectiveDate;
      local.CompareNonCooperation.Code = entities.NonCooperation.Code;

      ++export.Nc.Index;
      export.Nc.CheckSize();

      MoveNonCooperation(entities.NonCooperation,
        export.Nc.Update.NcDetNonCooperation);

      if (AsChar(entities.NonCooperation.Code) == 'Y')
      {
        export.Nc.Update.NcDetNonCooperation.Code = "N";
      }
      else if (AsChar(entities.NonCooperation.Code) == 'N')
      {
        export.Nc.Update.NcDetNonCooperation.Code = "C";
      }

      if (ReadCaseRoleCsePerson2())
      {
        MoveCaseRole(entities.Ap, export.Nc.Update.NcDetApCaseRole);
        export.Nc.Update.NcDetApCsePersonsWorkSet.Number =
          entities.CsePerson.Number;
      }
    }

    export.NoGcFnd.Count = export.Gc.Index + 1;
    export.NoNcFnd.Count = export.Nc.Index + 1;
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveGoodCause(GoodCause source, GoodCause target)
  {
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveNonCooperation(NonCooperation source,
    NonCooperation target)
  {
    target.Code = source.Code;
    target.Reason = source.Reason;
    target.EffectiveDate = source.EffectiveDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.Ar.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Ar.CasNumber = db.GetString(reader, 0);
        entities.Ar.CspNumber = db.GetString(reader, 1);
        entities.Ar.Type1 = db.GetString(reader, 2);
        entities.Ar.Identifier = db.GetInt32(reader, 3);
        entities.Ar.StartDate = db.GetNullableDate(reader, 4);
        entities.Ar.EndDate = db.GetNullableDate(reader, 5);
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ar.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.GoodCause.Populated);
    entities.CsePerson.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber1", entities.Case1.Number);
        db.
          SetString(command, "casNumber2", entities.GoodCause.CasNumber1 ?? "");
          
        db.SetInt32(
          command, "caseRoleId",
          entities.GoodCause.CroIdentifier1.GetValueOrDefault());
        db.SetString(command, "type", entities.GoodCause.CroType1 ?? "");
        db.SetString(command, "cspNumber", entities.GoodCause.CspNumber1 ?? "");
      },
      (db, reader) =>
      {
        entities.Ap.CasNumber = db.GetString(reader, 0);
        entities.Ap.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Ap.Type1 = db.GetString(reader, 2);
        entities.Ap.Identifier = db.GetInt32(reader, 3);
        entities.Ap.StartDate = db.GetNullableDate(reader, 4);
        entities.Ap.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ap.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.NonCooperation.Populated);
    entities.CsePerson.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber1", entities.Case1.Number);
        db.SetInt32(
          command, "caseRoleId",
          entities.NonCooperation.CroIdentifier1.GetValueOrDefault());
        db.SetString(command, "type", entities.NonCooperation.CroType1 ?? "");
        db.SetString(
          command, "cspNumber", entities.NonCooperation.CspNumber1 ?? "");
        db.SetString(
          command, "casNumber2", entities.NonCooperation.CasNumber1 ?? "");
      },
      (db, reader) =>
      {
        entities.Ap.CasNumber = db.GetString(reader, 0);
        entities.Ap.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Ap.Type1 = db.GetString(reader, 2);
        entities.Ap.Identifier = db.GetInt32(reader, 3);
        entities.Ap.StartDate = db.GetNullableDate(reader, 4);
        entities.Ap.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ap.Type1);
      });
  }

  private IEnumerable<bool> ReadGoodCause()
  {
    entities.GoodCause.Populated = false;

    return ReadEach("ReadGoodCause",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Ar.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedBy = db.GetNullableString(reader, 3);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.GoodCause.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.GoodCause.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.GoodCause.CasNumber = db.GetString(reader, 7);
        entities.GoodCause.CspNumber = db.GetString(reader, 8);
        entities.GoodCause.CroType = db.GetString(reader, 9);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 10);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 11);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 12);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 13);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 14);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);

        return true;
      });
  }

  private IEnumerable<bool> ReadNonCooperation()
  {
    entities.NonCooperation.Populated = false;

    return ReadEach("ReadNonCooperation",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Ar.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.NonCooperation.Code = db.GetNullableString(reader, 0);
        entities.NonCooperation.Reason = db.GetNullableString(reader, 1);
        entities.NonCooperation.EffectiveDate = db.GetNullableDate(reader, 2);
        entities.NonCooperation.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.NonCooperation.CreatedBy = db.GetString(reader, 4);
        entities.NonCooperation.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.NonCooperation.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.NonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.NonCooperation.CasNumber = db.GetString(reader, 8);
        entities.NonCooperation.CspNumber = db.GetString(reader, 9);
        entities.NonCooperation.CroType = db.GetString(reader, 10);
        entities.NonCooperation.CroIdentifier = db.GetInt32(reader, 11);
        entities.NonCooperation.CasNumber1 = db.GetNullableString(reader, 12);
        entities.NonCooperation.CspNumber1 = db.GetNullableString(reader, 13);
        entities.NonCooperation.CroType1 = db.GetNullableString(reader, 14);
        entities.NonCooperation.CroIdentifier1 =
          db.GetNullableInt32(reader, 15);
        entities.NonCooperation.Populated = true;
        CheckValid<NonCooperation>("CroType", entities.NonCooperation.CroType);
        CheckValid<NonCooperation>("CroType1", entities.NonCooperation.CroType1);
          

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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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

    private CsePersonsWorkSet ar;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GcGroup group.</summary>
    [Serializable]
    public class GcGroup
    {
      /// <summary>
      /// A value of GcDetGoodCause.
      /// </summary>
      [JsonPropertyName("gcDetGoodCause")]
      public GoodCause GcDetGoodCause
      {
        get => gcDetGoodCause ??= new();
        set => gcDetGoodCause = value;
      }

      /// <summary>
      /// A value of GcDetCommon.
      /// </summary>
      [JsonPropertyName("gcDetCommon")]
      public Common GcDetCommon
      {
        get => gcDetCommon ??= new();
        set => gcDetCommon = value;
      }

      /// <summary>
      /// A value of GcDetApCaseRole.
      /// </summary>
      [JsonPropertyName("gcDetApCaseRole")]
      public CaseRole GcDetApCaseRole
      {
        get => gcDetApCaseRole ??= new();
        set => gcDetApCaseRole = value;
      }

      /// <summary>
      /// A value of GcDetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcDetApCsePersonsWorkSet")]
      public CsePersonsWorkSet GcDetApCsePersonsWorkSet
      {
        get => gcDetApCsePersonsWorkSet ??= new();
        set => gcDetApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private GoodCause gcDetGoodCause;
      private Common gcDetCommon;
      private CaseRole gcDetApCaseRole;
      private CsePersonsWorkSet gcDetApCsePersonsWorkSet;
    }

    /// <summary>A NcGroup group.</summary>
    [Serializable]
    public class NcGroup
    {
      /// <summary>
      /// A value of NcDetNonCooperation.
      /// </summary>
      [JsonPropertyName("ncDetNonCooperation")]
      public NonCooperation NcDetNonCooperation
      {
        get => ncDetNonCooperation ??= new();
        set => ncDetNonCooperation = value;
      }

      /// <summary>
      /// A value of NcDetCommon.
      /// </summary>
      [JsonPropertyName("ncDetCommon")]
      public Common NcDetCommon
      {
        get => ncDetCommon ??= new();
        set => ncDetCommon = value;
      }

      /// <summary>
      /// A value of NcDetApCaseRole.
      /// </summary>
      [JsonPropertyName("ncDetApCaseRole")]
      public CaseRole NcDetApCaseRole
      {
        get => ncDetApCaseRole ??= new();
        set => ncDetApCaseRole = value;
      }

      /// <summary>
      /// A value of NcDetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("ncDetApCsePersonsWorkSet")]
      public CsePersonsWorkSet NcDetApCsePersonsWorkSet
      {
        get => ncDetApCsePersonsWorkSet ??= new();
        set => ncDetApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private NonCooperation ncDetNonCooperation;
      private Common ncDetCommon;
      private CaseRole ncDetApCaseRole;
      private CsePersonsWorkSet ncDetApCsePersonsWorkSet;
    }

    /// <summary>
    /// A value of NoGcFnd.
    /// </summary>
    [JsonPropertyName("noGcFnd")]
    public Common NoGcFnd
    {
      get => noGcFnd ??= new();
      set => noGcFnd = value;
    }

    /// <summary>
    /// A value of NoNcFnd.
    /// </summary>
    [JsonPropertyName("noNcFnd")]
    public Common NoNcFnd
    {
      get => noNcFnd ??= new();
      set => noNcFnd = value;
    }

    /// <summary>
    /// Gets a value of Gc.
    /// </summary>
    [JsonIgnore]
    public Array<GcGroup> Gc => gc ??= new(GcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Gc for json serialization.
    /// </summary>
    [JsonPropertyName("gc")]
    [Computed]
    public IList<GcGroup> Gc_Json
    {
      get => gc;
      set => Gc.Assign(value);
    }

    /// <summary>
    /// Gets a value of Nc.
    /// </summary>
    [JsonIgnore]
    public Array<NcGroup> Nc => nc ??= new(NcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Nc for json serialization.
    /// </summary>
    [JsonPropertyName("nc")]
    [Computed]
    public IList<NcGroup> Nc_Json
    {
      get => nc;
      set => Nc.Assign(value);
    }

    private Common noGcFnd;
    private Common noNcFnd;
    private Array<GcGroup> gc;
    private Array<NcGroup> nc;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CompareNonCooperation.
    /// </summary>
    [JsonPropertyName("compareNonCooperation")]
    public NonCooperation CompareNonCooperation
    {
      get => compareNonCooperation ??= new();
      set => compareNonCooperation = value;
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
    /// A value of CompareGoodCause.
    /// </summary>
    [JsonPropertyName("compareGoodCause")]
    public GoodCause CompareGoodCause
    {
      get => compareGoodCause ??= new();
      set => compareGoodCause = value;
    }

    /// <summary>
    /// A value of CompareDate.
    /// </summary>
    [JsonPropertyName("compareDate")]
    public DateWorkArea CompareDate
    {
      get => compareDate ??= new();
      set => compareDate = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private NonCooperation compareNonCooperation;
    private DateWorkArea null1;
    private GoodCause compareGoodCause;
    private DateWorkArea compareDate;
    private DateWorkArea current;
    private Common common;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private NonCooperation nonCooperation;
    private GoodCause goodCause;
    private CaseRole ar;
    private CsePerson csePerson;
    private CaseRole ap;
    private Case1 case1;
  }
#endregion
}
