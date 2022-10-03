// Program: SC_SECURITY_VALID_AUTH_FOR_FV, ID: 374370304, model: 746.
// Short name: SWE00301
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
/// A program: SC_SECURITY_VALID_AUTH_FOR_FV.
/// </para>
/// <para>
/// This CAB validates whether the user signed on is authorized for viewing or 
/// updating a case or individual involving family violence.
/// </para>
/// </summary>
[Serializable]
public partial class ScSecurityValidAuthForFv: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_SECURITY_VALID_AUTH_FOR_FV program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScSecurityValidAuthForFv(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScSecurityValidAuthForFv.
  /// </summary>
  public ScSecurityValidAuthForFv(IContext context, Import import, Export export)
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
    // -------------------------------------------------------------------
    // MAINTENANCE LOG:
    // 03/10/00	C. Scroggins		Initial Development
    // -------------------------------------------------------------------
    // ----------------------------------------------------------------------------------
    // 08/27/2001    Vithal Madhira        PR# 121249, 124583, 124584
    // Fixed the code for family violence indicator.  Changed code in SWE01082(
    // SC_CAB_TEST_SECURITY)  and SWE00301(SC_SECURITY_VALID_AUTH_FOR_FV) CABs .
    // ---------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------
    // 05/29/2002    Marek Lachowicz       PR# 147156
    // Fixed the code for family violence indicator in JAIL.
    // ---------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------
    // 08/17/2010    J Huss		    CQ# 21107
    // Added INCL (SRBO) to the list of tran codes that are skipped for case 
    // level checking.
    // ---------------------------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------
    // Check to make sure either a case number or a person number has been 
    // passed.
    // -------------------------------------------------------------------
    if (IsEmpty(import.Case1.Number) && IsEmpty(import.CsePerson.Number) && IsEmpty
      (import.CsePersonsWorkSet.Number))
    {
      return;
    }

    // -------------------------------------------------------------------
    // Move Imports to Exports
    // -------------------------------------------------------------------
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    // -------------------------------------------------------------------
    // Check to make see if the person number came through the work set view 
    // instead of the entity view. If so, move the work set export view to the
    // entity export view to use later.
    // -------------------------------------------------------------------
    if (!IsEmpty(export.CsePersonsWorkSet.Number) && IsEmpty
      (export.CsePerson.Number))
    {
      export.CsePerson.Number = export.CsePersonsWorkSet.Number;
    }

    // -------------------------------------------------------------------
    // Check for family violence by cse person number.
    // -------------------------------------------------------------------
    if (!IsEmpty(export.CsePerson.Number))
    {
      if (ReadCsePerson1())
      {
        // -------------------------------------------------------------------
        // If person is tagged for Family Violence, check to see if service 
        // Provider is approved for this person.
        // -------------------------------------------------------------------
        if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator) || Equal
          (global.TranCode, "SR1F"))
        {
          UseCoCabIsUserAssignedToCase2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (AsChar(local.Authorized.Flag) == 'N')
          {
            if (!Equal(global.TranCode, "SR1F"))
            {
              ExitState = "SC0000_DATA_NOT_DISPLAY_FOR_FV";
            }
            else
            {
              ExitState = "ACO_NE0000_USR_NOT_AUTH";
            }

            return;
          }
        }
      }
      else
      {
        ExitState = "CSE_PERSON_NF";
      }
    }

    // 05/29/2002 M.L Added SR6D to the following IF.
    if (Equal(global.TranCode, "SR1C") || Equal(global.TranCode, "SR1D") || Equal
      (global.TranCode, "SR1I") || Equal(global.TranCode, "SRQR") || Equal
      (global.TranCode, "SRQT") || Equal(global.TranCode, "SRQS") || Equal
      (global.TranCode, "SR1X") || Equal(global.TranCode, "SR1P") || Equal
      (global.TranCode, "SR5A") || Equal(global.TranCode, "SR6D") || Equal
      (global.TranCode, "SRBO"))
    {
      // -------------------------------------------------------------------------
      // Since the APDS(Trancode: SR1C),  ARDS(Trancode: SR1D), FCDS (Trancode: 
      // SR1I), QAPD(Trancode: SRQR), QARD(Trancode: SRQT), QINC(Trancode: SRQS
      // ), PADS(Trancode: SR1X), INCS(SR1P), RESO(SR5A)   are person driven
      // screens, there is no need to check the CASE level Family Violence. So
      // exit here.
      //                                                  
      // Vithal (08/28/2001) (PR# 121249)
      // --------------------------------------------------------------------------
      return;
    }

    // -------------------------------------------------------------------
    // Check for family violence by case number.
    // -------------------------------------------------------------------
    if (!IsEmpty(import.Case1.Number))
    {
      local.FvIndicator.Flag = "";

      foreach(var item in ReadCsePerson2())
      {
        if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
        {
          local.FvIndicator.Flag = "Y";

          break;
        }
      }

      // -------------------------------------------------------------------
      // If a participant on the case is tagged for Family Violence, check to 
      // see if service Provider is approved for this case.
      // -------------------------------------------------------------------
      if (!IsEmpty(local.FvIndicator.Flag) || Equal(global.TranCode, "SR1F"))
      {
        UseCoCabIsUserAssignedToCase1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -------------------------------------------------------------------
        // NOTE: If supervisor is not assigned, user cannot display.
        // -------------------------------------------------------------------
        if (AsChar(local.Authorized.Flag) == 'N')
        {
          if (!Equal(global.TranCode, "SR1F"))
          {
            ExitState = "SC0000_DATA_NOT_DISPLAY_FOR_FV";
          }
          else
          {
            ExitState = "ACO_NE0000_USR_NOT_AUTH";
          }
        }
      }
      else
      {
      }
    }
  }

  private void UseCoCabIsUserAssignedToCase1()
  {
    var useImport = new CoCabIsUserAssignedToCase.Import();
    var useExport = new CoCabIsUserAssignedToCase.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(CoCabIsUserAssignedToCase.Execute, useImport, useExport);

    local.Authorized.Flag = useExport.OnTheCase.Flag;
  }

  private void UseCoCabIsUserAssignedToCase2()
  {
    var useImport = new CoCabIsUserAssignedToCase.Import();
    var useExport = new CoCabIsUserAssignedToCase.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(CoCabIsUserAssignedToCase.Execute, useImport, useExport);

    local.Authorized.Flag = useExport.OnTheCase.Flag;
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePerson csePerson;
    private Case1 case1;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private Case1 case1;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Authorized.
    /// </summary>
    [JsonPropertyName("authorized")]
    public Common Authorized
    {
      get => authorized ??= new();
      set => authorized = value;
    }

    /// <summary>
    /// A value of FvIndicator.
    /// </summary>
    [JsonPropertyName("fvIndicator")]
    public Common FvIndicator
    {
      get => fvIndicator ??= new();
      set => fvIndicator = value;
    }

    private Common authorized;
    private Common fvIndicator;
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
