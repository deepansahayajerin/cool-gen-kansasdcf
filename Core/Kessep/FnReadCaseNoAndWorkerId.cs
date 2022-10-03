// Program: FN_READ_CASE_NO_AND_WORKER_ID, ID: 371741377, model: 746.
// Short name: SWE00529
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_CASE_NO_AND_WORKER_ID.
/// </para>
/// <para>
/// RESP: FINANCE
/// This will return the case number and worker ID for a specific obligor and 
/// supported person.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadCaseNoAndWorkerId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_CASE_NO_AND_WORKER_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCaseNoAndWorkerId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCaseNoAndWorkerId.
  /// </summary>
  public FnReadCaseNoAndWorkerId(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***--- Sumanta - MTW - 05/01/97
    // ***--- Changed the use of DB2 current date to IEF current date..
    // ***---
    // =================================================
    // 12/1/98 - B Adams  -  Added export views 'Service_Provider'
    //   and 'Office' so 'FN_Create_Obligation' could take care of
    //   creating Obligation_Assignment for Fees.
    // 3/23/1999 - b adams  -  Read properties set; Read of Case
    //   where Case is related to Case_Role is reset to the default
    //   since ONAC doesn't care if the supported person is active
    //   or not.  In that case, a low date (1900-01-01) will be imported.
    // =================================================
    // ***---  read action properties set
    // ***---  read action properties re-set
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    if (Lt(local.Discontinue.Date, import.SearchDiscontinue.Date))
    {
      local.Discontinue.Date = import.SearchDiscontinue.Date;
    }
    else
    {
      local.Discontinue.Date = local.Current.Date;
    }

    // ***** HARDCODE AREA *****
    local.HardcodeAp.Type1 = "AP";
    local.HardcodeChild.Type1 = "CH";
    local.HardcodeSpouse.Type1 = "AR";

    // =================================================
    // 3/3/1999 - bud adams  -  Persistent view of an entity type
    //   that is not going to be processed is senseless; and it began
    //   to act irregularly.  I couldn't get rid of the imported view but
    //   I did get rid of the Read and all references to it.
    //   Finally abandoned the other persistent view, too.
    // =================================================
    if (!import.Pobligor.Populated)
    {
      local.Obligor.Number = import.Obligor.Number;
    }
    else
    {
      local.Obligor.Number = import.Pobligor.Number;
    }

    if (!import.Psupported.Populated)
    {
      if (IsEmpty(import.Supported.Number))
      {
        // ***---  the person may be an AP on > 1 Case.  It doesn't matter, 
        // though.
        // ***---
        if (ReadCase3())
        {
          MoveCase1(entities.Case1, export.Case1);
        }
        else
        {
          ExitState = "CASE_NF";
        }
      }
      else
      {
        local.Supported.Number = import.Supported.Number;
      }
    }
    else
    {
      local.Supported.Number = import.Psupported.Number;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***** MAIN-LINE AREA *****
    // =================================================
    // 2/16/1999 - Bud Adams  -  Processing the persistent view
    //   statements was blowing up, so this was re-done to avoid
    //   those problems
    // =================================================
    if (IsEmpty(export.Case1.Number))
    {
      if (ReadCase2())
      {
        MoveCase1(entities.Case1, export.Case1);
      }
      else if (ReadCase1())
      {
        MoveCase1(entities.Case1, export.Case1);
      }
      else
      {
        ExitState = "NO_CASE_RL_FOUND_FOR_SUPP_PERSON";
      }
    }

    // *** This logic is no longer needed as this action block is not used to 
    // retrieve worker id - E. Parker 11/05/99
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.OfficeIdentifier = source.OfficeIdentifier;
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Discontinue.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber1", local.Obligor.Number);
        db.SetString(command, "type", local.HardcodeSpouse.Type1);
        db.SetString(command, "cspNumber2", local.Supported.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Discontinue.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber1", local.Obligor.Number);
        db.SetString(command, "type", local.HardcodeChild.Type1);
        db.SetString(command, "cspNumber2", local.Supported.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase3()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Discontinue.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 1);
        entities.Case1.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Pobligor.
    /// </summary>
    [JsonPropertyName("pobligor")]
    public CsePerson Pobligor
    {
      get => pobligor ??= new();
      set => pobligor = value;
    }

    /// <summary>
    /// A value of Psupported.
    /// </summary>
    [JsonPropertyName("psupported")]
    public CsePerson Psupported
    {
      get => psupported ??= new();
      set => psupported = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ObligationType Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of GetCaseNumberOnly.
    /// </summary>
    [JsonPropertyName("getCaseNumberOnly")]
    public Common GetCaseNumberOnly
    {
      get => getCaseNumberOnly ??= new();
      set => getCaseNumberOnly = value;
    }

    /// <summary>
    /// A value of SearchDiscontinue.
    /// </summary>
    [JsonPropertyName("searchDiscontinue")]
    public DateWorkArea SearchDiscontinue
    {
      get => searchDiscontinue ??= new();
      set => searchDiscontinue = value;
    }

    private CsePerson supported;
    private CsePerson obligor;
    private CsePerson pobligor;
    private CsePerson psupported;
    private ObligationType zdel;
    private Common getCaseNumberOnly;
    private DateWorkArea searchDiscontinue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Assign1.
    /// </summary>
    [JsonPropertyName("assign1")]
    public CsePersonsWorkSet Assign1
    {
      get => assign1 ??= new();
      set => assign1 = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private CsePersonsWorkSet assign1;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private Case1 case1;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of Discontinue.
    /// </summary>
    [JsonPropertyName("discontinue")]
    public DateWorkArea Discontinue
    {
      get => discontinue ??= new();
      set => discontinue = value;
    }

    /// <summary>
    /// A value of SpousalAj.
    /// </summary>
    [JsonPropertyName("spousalAj")]
    public ObligationType SpousalAj
    {
      get => spousalAj ??= new();
      set => spousalAj = value;
    }

    /// <summary>
    /// A value of Spousal.
    /// </summary>
    [JsonPropertyName("spousal")]
    public ObligationType Spousal
    {
      get => spousal ??= new();
      set => spousal = value;
    }

    /// <summary>
    /// A value of HardcodeAp.
    /// </summary>
    [JsonPropertyName("hardcodeAp")]
    public CaseRole HardcodeAp
    {
      get => hardcodeAp ??= new();
      set => hardcodeAp = value;
    }

    /// <summary>
    /// A value of HardcodeChild.
    /// </summary>
    [JsonPropertyName("hardcodeChild")]
    public CaseRole HardcodeChild
    {
      get => hardcodeChild ??= new();
      set => hardcodeChild = value;
    }

    /// <summary>
    /// A value of HardcodeSpouse.
    /// </summary>
    [JsonPropertyName("hardcodeSpouse")]
    public CaseRole HardcodeSpouse
    {
      get => hardcodeSpouse ??= new();
      set => hardcodeSpouse = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      discontinue = null;
      spousalAj = null;
      spousal = null;
      hardcodeAp = null;
      hardcodeChild = null;
      hardcodeSpouse = null;
      current = null;
      supported = null;
    }

    private DateWorkArea discontinue;
    private ObligationType spousalAj;
    private ObligationType spousal;
    private CaseRole hardcodeAp;
    private CaseRole hardcodeChild;
    private CaseRole hardcodeSpouse;
    private DateWorkArea current;
    private CsePerson obligor;
    private CsePerson supported;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ApType.
    /// </summary>
    [JsonPropertyName("apType")]
    public CaseRole ApType
    {
      get => apType ??= new();
      set => apType = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    private Office office;
    private Case1 case1;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CaseRole apType;
    private CaseRole child;
    private CaseRole ar;
    private CsePerson obligor;
    private CsePerson supported;
  }
#endregion
}
