// Program: SP_CREATE_CASE_UNIT, ID: 371728367, model: 746.
// Short name: SWE02044
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CREATE_CASE_UNIT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SpCreateCaseUnit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_CASE_UNIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateCaseUnit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateCaseUnit.
  /// </summary>
  public SpCreateCaseUnit(IContext context, Import import, Export export):
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
    // Date	  Developer		Description
    // 11-12-96  Ken Evans		Initial Dev
    // 05/96     Jack Rookard          This cab is a copy of SI_Create_Case_Unit
    // with necessary mods to allow Case Unit creation from both ROLE and REGI.
    // ------------------------------------------------------------
    // 06/22/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // ------------------------------------------------------------
    // The Case Unit is created in this action block.  The Case
    // Unit Function Assignment occurs in another action block.
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCase())
    {
      ExitState = "CASE_NF_RB";

      return;
    }

    // ------------------------------------------------------------
    // Calculate the new CU number
    // ------------------------------------------------------------
    // 06/22/99 M.L
    //              Change property of the following READ EACH to OPTIMIZE
    //              FOR 1 ROW
    if (ReadCaseUnit())
    {
      local.LastCase.CuNumber = entities.CaseUnit.CuNumber;
    }

    ++local.LastCase.CuNumber;

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCsePerson2())
    {
      ExitState = "AR_NF_RB";

      return;
    }

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCsePerson3())
    {
      ExitState = "CHILD_NF_RB";

      return;
    }

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCsePerson1())
    {
      ExitState = "AP_NF_RB";

      return;
    }

    UseCabSetMaximumDiscontinueDate();

    if (Equal(import.CaseUnit.StartDate, local.Initialized.Date))
    {
      local.CaseUnit.StartDate = local.Current.Date;
    }
    else
    {
      local.CaseUnit.StartDate = import.CaseUnit.StartDate;
    }

    try
    {
      CreateCaseUnit();
      export.CaseUnit.Assign(entities.CaseUnit);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CASE_UNIT_AE_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CASE_UNIT_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void CreateCaseUnit()
  {
    var cuNumber = local.LastCase.CuNumber;
    var state = import.CaseUnit.State;
    var startDate = local.CaseUnit.StartDate;
    var closureDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var casNo = entities.Case1.Number;
    var cspNoAr = entities.Ar.Number;
    var cspNoAp = entities.Ap.Number;
    var cspNoChild = entities.Child.Number;

    entities.CaseUnit.Populated = false;
    Update("CreateCaseUnit",
      (db, command) =>
      {
        db.SetInt32(command, "cuNumber", cuNumber);
        db.SetString(command, "state", state);
        db.SetDate(command, "startDate", startDate);
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "casNo", casNo);
        db.SetNullableString(command, "cspNoAr", cspNoAr);
        db.SetNullableString(command, "cspNoAp", cspNoAp);
        db.SetNullableString(command, "cspNoChild", cspNoChild);
      });

    entities.CaseUnit.CuNumber = cuNumber;
    entities.CaseUnit.State = state;
    entities.CaseUnit.StartDate = startDate;
    entities.CaseUnit.ClosureDate = closureDate;
    entities.CaseUnit.ClosureReasonCode = "";
    entities.CaseUnit.CreatedBy = createdBy;
    entities.CaseUnit.CreatedTimestamp = createdTimestamp;
    entities.CaseUnit.LastUpdatedBy = createdBy;
    entities.CaseUnit.LastUpdatedTimestamp = createdTimestamp;
    entities.CaseUnit.CasNo = casNo;
    entities.CaseUnit.CspNoAr = cspNoAr;
    entities.CaseUnit.CspNoAp = cspNoAp;
    entities.CaseUnit.CspNoChild = cspNoChild;
    entities.CaseUnit.Populated = true;
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
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CreatedBy = db.GetString(reader, 5);
        entities.CaseUnit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CaseUnit.CasNo = db.GetString(reader, 9);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 10);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 11);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 12);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Ap.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.Child.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Child.Number);
      },
      (db, reader) =>
      {
        entities.Child.Number = db.GetString(reader, 0);
        entities.Child.Populated = true;
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

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private CaseUnit caseUnit;
    private Case1 case1;
    private CsePerson child;
    private CsePerson ap;
    private CsePerson ar;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private CaseUnit caseUnit;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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

    /// <summary>
    /// A value of LastCase.
    /// </summary>
    [JsonPropertyName("lastCase")]
    public CaseUnit LastCase
    {
      get => lastCase ??= new();
      set => lastCase = value;
    }

    private DateWorkArea current;
    private CaseUnit caseUnit;
    private DateWorkArea initialized;
    private DateWorkArea max;
    private CaseUnit lastCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ControlTable Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private ControlTable zdel;
    private CaseUnit caseUnit;
    private Case1 case1;
    private CsePerson child;
    private CsePerson ap;
    private CsePerson ar;
  }
#endregion
}
