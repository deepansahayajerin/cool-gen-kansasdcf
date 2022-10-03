// Program: SI_READ_PARENT_AND_AR_DETAILS, ID: 371734864, model: 746.
// Short name: SWE01231
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_PARENT_AND_AR_DETAILS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block reads the basic details of the mother, father and current 
/// AR of the specified Case.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadParentAndArDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_PARENT_AND_AR_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadParentAndArDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadParentAndArDetails.
  /// </summary>
  public SiReadParentAndArDetails(IContext context, Import import, Export export)
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
    // ---------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    //  Date	   Developer	Request #	Description
    // 3-6-95	Helen Sharland	        0	Initial Development
    // 4-30-97 JF.Caillouet			Change Current Date
    // 6-5-97  Sid		Cleanup and Fixes.
    // ---------------------------------------------------------
    // 07/01/99 M.Lachowicz      Change property of READ
    //                           (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // --------------------------------------------------------------------------
    // 11/15/2002    V.Madhira    PR# 160658       Display FA & MO roles on COMP
    // for closed cases.
    // --------------------------------------------------------------------------
    local.SuppressZeroDob.Flag = "Y";
    local.Current.Date = Now().Date;

    // --------------------------------------------
    // Read Mother details
    // --------------------------------------------
    // 07/01/99 M.L         Change property of READ to generate
    //                      Select Only
    // ------------------------------------------------------------
    if (ReadCsePerson3())
    {
      // ---------------------------------------------
      // Call EAB to retrieve information from the
      // ADABAS files about a CSE Person
      // ---------------------------------------------
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      local.ErrOnAdabasUnavailable.Flag = "Y";
      UseSiReadCsePerson2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      // ------------------------------------------------------------------------------------
      // Per PR# 160658, if the case is closed, display the latest end_dated '
      // MO' role info on COMP (per SME Pam Bishop).
      // ------------------------------------------------------------------------------------
      if (AsChar(import.CaseOpen.Flag) == 'N')
      {
        if (ReadCsePersonCaseRole2())
        {
          // ---------------------------------------------
          // Call EAB to retrieve information from the
          // ADABAS files about a CSE Person
          // ---------------------------------------------
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          local.ErrOnAdabasUnavailable.Flag = "Y";
          UseSiReadCsePerson2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }

      // --------------------------------------------
      // A mother has not been established for this case.  Continue with 
      // processing
      // --------------------------------------------
    }

    // --------------------------------------------
    // Read Father details
    // --------------------------------------------
    // 07/01/99 M.L         Change property of READ to generate
    //                      Select Only
    // ------------------------------------------------------------
    if (ReadCsePerson2())
    {
      // ---------------------------------------------
      // Call EAB to retrieve information from the
      // ADABAS files about a CSE Person
      // ---------------------------------------------
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      local.ErrOnAdabasUnavailable.Flag = "Y";
      UseSiReadCsePerson1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      // ------------------------------------------------------------------------------------
      // Per PR# 160658, if the case is closed, display the latest end_dated '
      // FA' role info on COMP (per SME Pam Bishop).
      // ------------------------------------------------------------------------------------
      if (AsChar(import.CaseOpen.Flag) == 'N')
      {
        if (ReadCsePersonCaseRole1())
        {
          // ---------------------------------------------
          // Call EAB to retrieve information from the
          // ADABAS files about a CSE Person
          // ---------------------------------------------
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          local.ErrOnAdabasUnavailable.Flag = "Y";
          UseSiReadCsePerson1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }

      // --------------------------------------------
      // A father has not been established for this case.  Continue with 
      // processing
      // --------------------------------------------
    }

    // --------------------------------------------
    // Read AR details
    // --------------------------------------------
    // 07/01/99 M.L         Change property of READ to generate
    //                      Select Only
    // ------------------------------------------------------------
    if (ReadCsePerson1())
    {
      if (CharAt(entities.CsePerson.Number, 10) == 'O')
      {
        export.Ar.FormattedName = entities.CsePerson.OrganizationName ?? Spaces
          (33);
        export.Ar.Number = entities.CsePerson.Number;
      }
      else
      {
        // ---------------------------------------------
        // Call EAB to retrieve information from the
        // ADABAS files about a CSE Person
        // ---------------------------------------------
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        local.ErrOnAdabasUnavailable.Flag = "Y";
        UseSiReadCsePerson3();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
      }
    }
    else
    {
      if (AsChar(import.CaseOpen.Flag) == 'N')
      {
        // --------------------------------------------
        // Read last AR details for a CLOSED CASE.
        // --------------------------------------------
        if (ReadCaseRoleCsePerson())
        {
          if (CharAt(entities.CsePerson.Number, 10) == 'O')
          {
            export.Ar.FormattedName = entities.CsePerson.OrganizationName ?? Spaces
              (33);
            export.Ar.Number = entities.CsePerson.Number;
          }
          else
          {
            // ---------------------------------------------
            // Call EAB to retrieve information from the
            // ADABAS files about a CSE Person
            // ---------------------------------------------
            local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            local.ErrOnAdabasUnavailable.Flag = "Y";
            UseSiReadCsePerson3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          return;
        }
      }

      ExitState = "AR_DB_ERROR_NF";
    }
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.SuppressZeroDob.Flag = local.SuppressZeroDob.Flag;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.AbendData.Assign(useExport.AbendData);
    export.Father.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.SuppressZeroDob.Flag = local.SuppressZeroDob.Flag;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.AbendData.Assign(useExport.AbendData);
    export.Mother.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.SuppressZeroDob.Flag = local.SuppressZeroDob.Flag;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.AbendData.Assign(useExport.AbendData);
    export.Ar.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
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
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 7);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonCaseRole1()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CaseRole.CasNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePersonCaseRole2()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CaseRole.CasNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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

    private Common caseOpen;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of Father.
    /// </summary>
    [JsonPropertyName("father")]
    public CsePersonsWorkSet Father
    {
      get => father ??= new();
      set => father = value;
    }

    /// <summary>
    /// A value of Mother.
    /// </summary>
    [JsonPropertyName("mother")]
    public CsePersonsWorkSet Mother
    {
      get => mother ??= new();
      set => mother = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private AbendData abendData;
    private CsePersonsWorkSet father;
    private CsePersonsWorkSet mother;
    private CsePersonsWorkSet ar;
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

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public Common Test
    {
      get => test ??= new();
      set => test = value;
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
    /// A value of SuppressZeroDob.
    /// </summary>
    [JsonPropertyName("suppressZeroDob")]
    public Common SuppressZeroDob
    {
      get => suppressZeroDob ??= new();
      set => suppressZeroDob = value;
    }

    private DateWorkArea current;
    private Common errOnAdabasUnavailable;
    private Common test;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common suppressZeroDob;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
  }
#endregion
}
