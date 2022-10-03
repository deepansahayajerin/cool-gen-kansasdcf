// Program: FN_OCSE157_FIND_CASE_FOR_AUDIT, ID: 371356309, model: 746.
// Short name: SWE02979
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_FIND_CASE_FOR_AUDIT.
/// </summary>
[Serializable]
public partial class FnOcse157FindCaseForAudit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_FIND_CASE_FOR_AUDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157FindCaseForAudit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157FindCaseForAudit.
  /// </summary>
  public FnOcse157FindCaseForAudit(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // -------------------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // ---------------------------------------------------------------------
    // 04/14/08  GVandy  	CQ#2461		Initial Development.  Per federal data 
    // reliability audit, provide
    // 					case number in audit data.
    // -------------------------------------------------------------------------------------------------------------
    // -- Find case where supported is active on the import date and the AP has 
    // a role on the
    //    case where the AP effective date range overlaps the supported role 
    // date range.
    foreach(var item in ReadCaseRoleCase1())
    {
      if (ReadCaseRole())
      {
        export.Ocse157Verification.CaseNumber = entities.Case1.Number;

        return;
      }
    }

    // -- Find case where supported role ended prior to the import date and the 
    // AP has a role on the
    //    case where the AP effective date range overlaps the supported role 
    // date range.
    foreach(var item in ReadCaseRoleCase3())
    {
      if (ReadCaseRole())
      {
        export.Ocse157Verification.CaseNumber = entities.Case1.Number;

        return;
      }
    }

    // -- Find case where supported role started after the import date and the 
    // AP has a role on the
    //    case where the AP effective date range overlaps the supported role 
    // date range.
    foreach(var item in ReadCaseRoleCase2())
    {
      if (ReadCaseRole())
      {
        export.Ocse157Verification.CaseNumber = entities.Case1.Number;

        return;
      }
    }

    // -- We did not find a case to report in the audit data.  Set the cae 
    // number to "NOT FOUND".
    export.Ocse157Verification.CaseNumber = "NOT FOUND";
  }

  private bool ReadCaseRole()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          entities.SupportedCaseRole.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          entities.SupportedCaseRole.StartDate.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase1()
  {
    entities.Case1.Populated = false;
    entities.SupportedCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCase1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          import.DebtOrCollection.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.SupportedCaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.SupportedCaseRole.CspNumber = db.GetString(reader, 1);
        entities.SupportedCaseRole.Type1 = db.GetString(reader, 2);
        entities.SupportedCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.SupportedCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.SupportedCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Populated = true;
        entities.SupportedCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.SupportedCaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase2()
  {
    entities.Case1.Populated = false;
    entities.SupportedCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCase2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          import.DebtOrCollection.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.SupportedCaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.SupportedCaseRole.CspNumber = db.GetString(reader, 1);
        entities.SupportedCaseRole.Type1 = db.GetString(reader, 2);
        entities.SupportedCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.SupportedCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.SupportedCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Populated = true;
        entities.SupportedCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.SupportedCaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase3()
  {
    entities.Case1.Populated = false;
    entities.SupportedCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCase3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", import.DebtOrCollection.Date.GetValueOrDefault());
          
        db.SetString(command, "cspNumber", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.SupportedCaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.SupportedCaseRole.CspNumber = db.GetString(reader, 1);
        entities.SupportedCaseRole.Type1 = db.GetString(reader, 2);
        entities.SupportedCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.SupportedCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.SupportedCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Populated = true;
        entities.SupportedCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.SupportedCaseRole.Type1);

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of DebtOrCollection.
    /// </summary>
    [JsonPropertyName("debtOrCollection")]
    public DateWorkArea DebtOrCollection
    {
      get => debtOrCollection ??= new();
      set => debtOrCollection = value;
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

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private DateWorkArea debtOrCollection;
    private CsePerson supported;
    private CsePerson ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    private Ocse157Verification ocse157Verification;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of SupportedCaseRole.
    /// </summary>
    [JsonPropertyName("supportedCaseRole")]
    public CaseRole SupportedCaseRole
    {
      get => supportedCaseRole ??= new();
      set => supportedCaseRole = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
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

    private Case1 case1;
    private CaseRole apCaseRole;
    private CaseRole supportedCaseRole;
    private CsePerson supportedCsePerson;
    private CsePerson apCsePerson;
  }
#endregion
}
