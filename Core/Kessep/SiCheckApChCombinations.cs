// Program: SI_CHECK_AP_CH_COMBINATIONS, ID: 374387219, model: 746.
// Short name: SWE01493
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CHECK_AP_CH_COMBINATIONS.
/// </summary>
[Serializable]
public partial class SiCheckApChCombinations: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHECK_AP_CH_COMBINATIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCheckApChCombinations(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCheckApChCombinations.
  /// </summary>
  public SiCheckApChCombinations(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    //   Date   Developer	Description
    // -------- -------------- 
    // -------------------------------
    // 04/01/00 M.Lachowicz    Check if exist
    //                         other case with import child
    //                         and AP different than
    //                         import AP.
    // -----------------------------------------------------
    local.ErrOnAdabasUnavailable.Flag = "Y";

    foreach(var item in ReadCaseCsePerson())
    {
      local.Ap.Number = entities.ApCsePerson.Number;
      UseCabReadAdabasPerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (AsChar(local.Ap.Sex) == 'M')
      {
        export.ActiveOtherMaleAp.Flag = "Y";

        return;
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Ap.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.Ap);
    export.AbendData.Assign(useExport.AbendData);
  }

  private IEnumerable<bool> ReadCaseCsePerson()
  {
    entities.ApCsePerson.Populated = false;
    entities.Existing.Populated = false;

    return ReadEach("ReadCaseCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
        db.SetNullableDate(
          command, "endDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber1", import.Ap.Number);
        db.SetString(command, "cspNumber2", import.Ch.Number);
      },
      (db, reader) =>
      {
        entities.Existing.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCsePerson.Populated = true;
        entities.Existing.Populated = true;

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

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    private DateWorkArea current;
    private Case1 case1;
    private CsePerson ap;
    private CsePerson ch;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActiveOtherMaleAp.
    /// </summary>
    [JsonPropertyName("activeOtherMaleAp")]
    public Common ActiveOtherMaleAp
    {
      get => activeOtherMaleAp ??= new();
      set => activeOtherMaleAp = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private Common activeOtherMaleAp;
    private AbendData abendData;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    private CsePersonsWorkSet ap;
    private Common errOnAdabasUnavailable;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
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
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private CsePerson chCsePerson;
    private CsePerson apCsePerson;
    private CaseRole apCaseRole;
    private CaseRole chCaseRole;
    private Case1 existing;
  }
#endregion
}
