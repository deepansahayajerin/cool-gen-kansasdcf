// Program: SI_CHECK_CASE_TO_AP_AND_CHILD, ID: 371731794, model: 746.
// Short name: SWE01117
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CHECK_CASE_TO_AP_AND_CHILD.
/// </para>
/// <para>
/// RESP: SRVINIT		
/// This process reads the case.  If an AP is supplied it will check to 
/// determine if a relationship exists.  Also if a child is supplied it will
/// check to determine if a relationship exists.
/// </para>
/// </summary>
[Serializable]
public partial class SiCheckCaseToApAndChild: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHECK_CASE_TO_AP_AND_CHILD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCheckCaseToApAndChild(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCheckCaseToApAndChild.
  /// </summary>
  public SiCheckCaseToApAndChild(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // Date		Developer		Request #	Description
    // 04-23-96	G. LOFTON - MTW			0	Initial Dev
    // -------------------------------------------------------------------
    // 06/22/99 W.Campbell  Modified the properties
    //                      of 2 READ statements to
    //                      Select Only.
    // ---------------------------------------------------------
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // Check AP#
    // ---------------------------------------------
    if (!IsEmpty(import.Ap.Number))
    {
      if (Equal(import.Ap.Number, import.HiddenAp.Number))
      {
        // ---------------------------------------------
        // Read AP for the case
        // ---------------------------------------------
        // ---------------------------------------------------------
        // 06/22/99 W.Campbell - Modified the properties
        // of the following READ statement to Select Only.
        // ---------------------------------------------------------
        if (ReadCsePerson1())
        {
          export.Ap.Number = import.Ap.Number;
        }
        else
        {
          // ---------------------------------------------
          // AP number does not match to this case
          // ---------------------------------------------
          export.Ap.Number = "";
        }
      }
    }

    // ---------------------------------------------
    // Check Child #
    // ---------------------------------------------
    if (!IsEmpty(import.Child.Number))
    {
      if (Equal(import.Child.Number, import.HiddenChild.Number))
      {
        // ---------------------------------------------
        // Read Child for the case
        // ---------------------------------------------
        // ---------------------------------------------------------
        // 06/22/99 W.Campbell - Modified the properties
        // of the following READ statement to Select Only.
        // ---------------------------------------------------------
        if (ReadCsePerson2())
        {
          export.Child.Number = import.Child.Number;
        }
        else
        {
          // ---------------------------------------------
          // AP number does not match to this case
          // ---------------------------------------------
          export.Child.Number = "";
        }
      }
    }
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Child.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePersonsWorkSet Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of HiddenChild.
    /// </summary>
    [JsonPropertyName("hiddenChild")]
    public CsePersonsWorkSet HiddenChild
    {
      get => hiddenChild ??= new();
      set => hiddenChild = value;
    }

    private Case1 case1;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet hiddenAp;
    private CsePersonsWorkSet child;
    private CsePersonsWorkSet hiddenChild;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePersonsWorkSet Child
    {
      get => child ??= new();
      set => child = value;
    }

    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet child;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
