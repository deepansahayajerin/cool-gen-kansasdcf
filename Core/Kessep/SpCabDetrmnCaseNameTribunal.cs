// Program: SP_CAB_DETRMN_CASE_NAME_TRIBUNAL, ID: 372572056, model: 746.
// Short name: SWE00020
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
/// A program: SP_CAB_DETRMN_CASE_NAME_TRIBUNAL.
/// </para>
/// <para>
/// This action blocks determines what cse_persons name to use for re-
/// distribution name matching.  It also determines what tribunal to use for
/// redistribution.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabDetrmnCaseNameTribunal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DETRMN_CASE_NAME_TRIBUNAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDetrmnCaseNameTribunal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDetrmnCaseNameTribunal.
  /// </summary>
  public SpCabDetrmnCaseNameTribunal(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	  Developer	Request		Description
    // ----------------------------------------------------------------------------
    // 10/07/99  SWSRCHF	H00073420       bypass CSE Person TYPEs= "O"
    // 					(State of Kansas)
    // 01/04/01  SWSRCHF	000265		Expand Last Name from 6 to 11
    // 					characters for OFCA and OFCD
    // 08/16/13  GVandy	CQ38147    	Replace assignments by county with
    // 					assignments by tribunal.
    // 09/24/13  GVandy	CQ41660    	Limit tribunals to only Kansas tribunals.
    // ----------------------------------------------------------------------------
    // ************************************************************
    // Determine last name to use for assignment
    // ************************************************************
    local.ApCnt.Count = 0;

    foreach(var item in ReadCaseRoleCsePerson3())
    {
      ++local.ApCnt.Count;

      if (local.ApCnt.Count > 1)
      {
        break;
      }

      local.CsePersonsWorkSet.Assign(local.Null1);
      local.CsePersonsWorkSet.Number = entities.ApCsePerson.Number;
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (Equal(local.CsePersonsWorkSet.LastName, "*ADABAS UNAVAIL*"))
      {
        ExitState = "ACO_ADABAS_UNAVAILABLE";

        return;
      }
    }

    if (local.ApCnt.Count == 1)
    {
    }
    else
    {
      // ************************************************************
      // No AP is known or multiple alleged APs are found,  use the
      // ARs last name for Alpha matching.
      // ************************************************************
      local.CsePersonsWorkSet.Assign(local.Null1);

      // The following statements were changed by Carl Galka on 10/25/1999. The 
      // Qualification of AR CSE_PERSON TYPE = C was added since we only want to
      // find the client AR
      if (ReadCaseRoleCsePerson1())
      {
        local.CsePersonsWorkSet.Number = entities.ArCsePerson.Number;
        local.CsePersonsWorkSet.FirstName = "";
        local.CsePersonsWorkSet.FormattedName = "";
        local.CsePersonsWorkSet.LastName = "";
        local.CsePersonsWorkSet.Sex = "";
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(local.CsePersonsWorkSet.LastName, "*ADABAS UNAVAIL*"))
        {
          ExitState = "ACO_ADABAS_UNAVAILABLE";

          return;
        }
      }
      else
      {
        // ************************************************************
        // The AR is an organization. Read for the Child CSE Person on the Case.
        // ************************************************************
        if (ReadCaseRoleCsePerson2())
        {
          // Currency established for the CH	Case role and the CSE Person 
          // playing the CH Case role. When this occurs, we have inferred that
          // the AR is an  Organization, such as the State of Kansas.  Use the
          // Child's last name for alpha matching.
          local.CsePersonsWorkSet.Number = entities.ChCsePerson.Number;
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (Equal(local.CsePersonsWorkSet.LastName, "*ADABAS UNAVAIL*"))
          {
            ExitState = "ACO_ADABAS_UNAVAILABLE";

            return;
          }
        }
      }
    }

    if (IsEmpty(local.CsePersonsWorkSet.LastName))
    {
      // ------------------------------------
      // 11/03/99 W.CAMPBELL - changed the
      // following exit state to one more
      // meaningful, in case it ever happens.
      // It should never happen.
      // Work done on PR# 00077898.
      // ------------------------------------
      ExitState = "NO_LAST_NAME_FOR_CASE_ASSIGNMENT";

      return;
    }

    if (IsEmpty(local.CsePersonsWorkSet.LastName))
    {
      // Either 0 or more than 1 AP on Case, AR is an organization, and the 
      // Child on the Case does not have a name.  This should never happen.  Set
      // the work set fields to the value of "a" as a final fall thru default
      // for alpha matching. Jack Rookard 8/13/97
      local.CsePersonsWorkSet.FirstName = "A";
      local.CsePersonsWorkSet.LastName = "A";
    }

    export.TextWorkArea.Text30 =
      Substring(local.CsePersonsWorkSet.LastName,
      CsePersonsWorkSet.LastName_MaxLength, 1, 11) + Substring
      (local.CsePersonsWorkSet.FirstName, CsePersonsWorkSet.FirstName_MaxLength,
      1, 1);

    // ************************************************************
    // Determine Tribunal to use for assignment
    // ************************************************************
    // 09/24/13  GVandy  CQ41660  Limit tribunals to only Kansas tribunals.
    // 08/16/13  GVandy  CQ38147  Replace assignments by county with assignments
    // by tribunal.
    local.Common.Count = 0;

    // -- The read each below is set to distinct.
    foreach(var item in ReadTribunal())
    {
      ++local.Common.Count;

      if (local.Common.Count == 1)
      {
        export.Tribunal.Identifier = entities.Tribunal.Identifier;
      }
      else
      {
        export.Tribunal.Identifier = 0;

        return;
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, local.CsePerson);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.ArCaseRole.Populated = false;
    entities.ArCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ArCsePerson.Number = db.GetString(reader, 1);
        entities.ArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ArCsePerson.Type1 = db.GetString(reader, 6);
        entities.ArCaseRole.Populated = true;
        entities.ArCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ArCsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    entities.ChCaseRole.Populated = false;
    entities.ChCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChCsePerson.Number = db.GetString(reader, 1);
        entities.ChCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCsePerson.Type1 = db.GetString(reader, 6);
        entities.ChCaseRole.Populated = true;
        entities.ChCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson3()
  {
    entities.ApCsePerson.Populated = false;
    entities.ApCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCsePerson.Type1 = db.GetString(reader, 6);
        entities.ApCsePerson.Populated = true;
        entities.ApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return ReadEach("ReadTribunal",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;

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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea current;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private TextWorkArea textWorkArea;
    private Tribunal tribunal;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CsePersonsWorkSet Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ApCnt.
    /// </summary>
    [JsonPropertyName("apCnt")]
    public Common ApCnt
    {
      get => apCnt ??= new();
      set => apCnt = value;
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
    /// A value of TbdLocalAr.
    /// </summary>
    [JsonPropertyName("tbdLocalAr")]
    public CsePersonAddress TbdLocalAr
    {
      get => tbdLocalAr ??= new();
      set => tbdLocalAr = value;
    }

    private CsePersonsWorkSet null1;
    private Common common;
    private CsePerson csePerson;
    private Common apCnt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonAddress tbdLocalAr;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    private Fips fips;
    private CaseRole caseRole;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private Tribunal tribunal;
    private Case1 case1;
    private CsePerson apCsePerson;
    private CaseRole apCaseRole;
    private CaseRole arCaseRole;
    private CsePerson arCsePerson;
    private CaseRole chCaseRole;
    private CsePerson chCsePerson;
  }
#endregion
}
