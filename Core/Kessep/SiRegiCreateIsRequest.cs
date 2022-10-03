// Program: SI_REGI_CREATE_IS_REQUEST, ID: 373468013, model: 746.
// Short name: SWE02088
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_REGI_CREATE_IS_REQUEST.
/// </summary>
[Serializable]
public partial class SiRegiCreateIsRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_REGI_CREATE_IS_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRegiCreateIsRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRegiCreateIsRequest.
  /// </summary>
  public SiRegiCreateIsRequest(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------
    //   DATE   DEVELOPER         DESCRIPTION
    // ??/??/?? ??????????        Original Development.
    local.Current.Timestamp = Now();
    local.Current.Date = Date(local.Current.Timestamp);
    local.InterstateRequest.Assign(import.InterstateRequest);
    local.InterstateRequest.KsCaseInd = "N";
    local.InterstateRequest.OtherStateFips = import.Incoming.OtherFipsState;
    local.InterstateRequest.OtherStateCaseId =
      import.Incoming.InterstateCaseId ?? "";
    local.InterstateRequest.OtherStateCaseStatus = import.Incoming.CaseStatus;
    local.InterstateRequest.OtherStateCaseClosureReason = "";

    // *************************************************************
    // 03/20/00   C. Ott   PR # 85011.   Need to assure that Case Type attribute
    // is not empty for outgoing transaction.  If imported value is spaces,
    // call action blocks to derive active case type or arrears case type.
    // *************************************************************
    if (IsEmpty(import.Incoming.CaseType))
    {
      UseSiReadCaseProgramType();

      if (IsEmpty(local.Program.Code) || !IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
        UseSiReadArrOnlyCasePrgType();
      }

      local.InterstateRequest.CaseType = local.Program.Code;
    }
    else
    {
      local.InterstateRequest.CaseType = import.Incoming.CaseType;
    }

    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadAbsentParentCsePerson())
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    // ***************************************************************
    // 8/18/99   C. Ott   Added a check to prevent creation of a duplicate 
    // Interstate Request for the same state.
    // ***************************************************************
    if (local.InterstateRequest.OtherStateFips > 0)
    {
    }
    else if (import.InterstateRequest.OtherStateFips > 0)
    {
      local.InterstateRequest.OtherStateFips =
        import.InterstateRequest.OtherStateFips;
    }
    else if (import.Incoming.OtherFipsState > 0)
    {
      local.InterstateRequest.OtherStateFips = import.Incoming.OtherFipsState;
    }

    if (ReadInterstateRequest2())
    {
      export.InterstateRequest.Assign(entities.InterstateRequest);
      ExitState = "INTERSTATE_AE";

      return;
    }

    // >>
    // Check to see if a closed interstate request exists for this
    // state.
    if (ReadInterstateRequest3())
    {
      try
      {
        UpdateInterstateRequest();
        export.InterstateRequest.Assign(entities.InterstateRequest);

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // 06/23/99 M.L
    //              Change property of the READ EACH to OPTIMIZE FOR
    //              1 ROW
    ReadInterstateRequest1();
    local.InterstateRequest.IntHGeneratedId =
      entities.InterstateRequest.IntHGeneratedId + 1;

    // -----------------------------------------
    // 06/11/99 W.Campbell - Modified
    // set stmt for interstate_request
    // other_state_case_closure_date so that
    // it will be set from an import view rather
    // than a local view.
    // -----------------------------------------
    try
    {
      CreateInterstateRequest();
      export.InterstateRequest.Assign(entities.InterstateRequest);
      AssociateInterstateRequest1();
      AssociateInterstateRequest2();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "INTERSTATE_REQUEST_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "INTERSTATE_REQUEST_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
  }

  private void UseSiReadArrOnlyCasePrgType()
  {
    var useImport = new SiReadArrOnlyCasePrgType.Import();
    var useExport = new SiReadArrOnlyCasePrgType.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadArrOnlyCasePrgType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void AssociateInterstateRequest1()
  {
    var casINumber = entities.Case1.Number;

    entities.InterstateRequest.Populated = false;
    Update("AssociateInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", casINumber);
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.CasINumber = casINumber;
    entities.InterstateRequest.Populated = true;
  }

  private void AssociateInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);

    var casNumber = entities.AbsentParent.CasNumber;
    var cspNumber = entities.AbsentParent.CspNumber;
    var croType = entities.AbsentParent.Type1;
    var croId = entities.AbsentParent.Identifier;

    CheckValid<InterstateRequest>("CroType", croType);
    entities.InterstateRequest.Populated = false;
    Update("AssociateInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "croType", croType);
        db.SetNullableInt32(command, "croId", croId);
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.CasNumber = casNumber;
    entities.InterstateRequest.CspNumber = cspNumber;
    entities.InterstateRequest.CroType = croType;
    entities.InterstateRequest.CroId = croId;
    entities.InterstateRequest.Populated = true;
  }

  private void CreateInterstateRequest()
  {
    var intHGeneratedId = local.InterstateRequest.IntHGeneratedId;
    var otherStateCaseId = local.InterstateRequest.OtherStateCaseId ?? "";
    var otherStateFips = local.InterstateRequest.OtherStateFips;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var otherStateCaseStatus = local.InterstateRequest.OtherStateCaseStatus;
    var caseType = local.InterstateRequest.CaseType ?? "";
    var ksCaseInd = local.InterstateRequest.KsCaseInd;
    var otherStateCaseClosureReason =
      local.InterstateRequest.OtherStateCaseClosureReason ?? "";
    var otherStateCaseClosureDate =
      import.InterstateRequest.OtherStateCaseClosureDate;
    var country = import.InterstateRequest.Country ?? "";

    entities.InterstateRequest.Populated = false;
    Update("CreateInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", intHGeneratedId);
        db.SetNullableString(command, "otherStateCasId", otherStateCaseId);
        db.SetInt32(command, "othrStateFipsCd", otherStateFips);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "othStCaseStatus", otherStateCaseStatus);
        db.SetNullableString(command, "caseType", caseType);
        db.SetString(command, "ksCaseInd", ksCaseInd);
        db.SetNullableString(
          command, "othStateClsRes", otherStateCaseClosureReason);
        db.
          SetNullableDate(command, "othStateClsDte", otherStateCaseClosureDate);
          
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "tribalAgency", "");
      });

    entities.InterstateRequest.IntHGeneratedId = intHGeneratedId;
    entities.InterstateRequest.OtherStateCaseId = otherStateCaseId;
    entities.InterstateRequest.OtherStateFips = otherStateFips;
    entities.InterstateRequest.CreatedBy = createdBy;
    entities.InterstateRequest.CreatedTimestamp = createdTimestamp;
    entities.InterstateRequest.LastUpdatedBy = createdBy;
    entities.InterstateRequest.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstateRequest.OtherStateCaseStatus = otherStateCaseStatus;
    entities.InterstateRequest.CaseType = caseType;
    entities.InterstateRequest.KsCaseInd = ksCaseInd;
    entities.InterstateRequest.OtherStateCaseClosureReason =
      otherStateCaseClosureReason;
    entities.InterstateRequest.OtherStateCaseClosureDate =
      otherStateCaseClosureDate;
    entities.InterstateRequest.CasINumber = null;
    entities.InterstateRequest.CasNumber = null;
    entities.InterstateRequest.CspNumber = null;
    entities.InterstateRequest.CroType = null;
    entities.InterstateRequest.CroId = null;
    entities.InterstateRequest.Country = country;
    entities.InterstateRequest.Populated = true;
  }

  private bool ReadAbsentParentCsePerson()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadAbsentParentCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
      });
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
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      null,
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
        db.SetInt32(
          command, "othrStateFipsCd", local.InterstateRequest.OtherStateFips);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest3()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest3",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
        db.SetInt32(
          command, "othrStateFipsCd", local.InterstateRequest.OtherStateFips);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.Populated = true;
      });
  }

  private void UpdateInterstateRequest()
  {
    var otherStateCaseId = local.InterstateRequest.OtherStateCaseId ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var otherStateCaseStatus = local.InterstateRequest.OtherStateCaseStatus;
    var caseType = local.InterstateRequest.CaseType ?? "";
    var ksCaseInd = "N";
    var otherStateCaseClosureReason =
      local.InterstateRequest.OtherStateCaseClosureReason ?? "";
    var otherStateCaseClosureDate =
      import.InterstateRequest.OtherStateCaseClosureDate;
    var country = import.InterstateRequest.Country ?? "";

    entities.InterstateRequest.Populated = false;
    Update("UpdateInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "otherStateCasId", otherStateCaseId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "othStCaseStatus", otherStateCaseStatus);
        db.SetNullableString(command, "caseType", caseType);
        db.SetString(command, "ksCaseInd", ksCaseInd);
        db.SetNullableString(
          command, "othStateClsRes", otherStateCaseClosureReason);
        db.
          SetNullableDate(command, "othStateClsDte", otherStateCaseClosureDate);
          
        db.SetNullableString(command, "country", country);
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.OtherStateCaseId = otherStateCaseId;
    entities.InterstateRequest.CreatedBy = createdBy;
    entities.InterstateRequest.CreatedTimestamp = createdTimestamp;
    entities.InterstateRequest.LastUpdatedBy = createdBy;
    entities.InterstateRequest.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstateRequest.OtherStateCaseStatus = otherStateCaseStatus;
    entities.InterstateRequest.CaseType = caseType;
    entities.InterstateRequest.KsCaseInd = ksCaseInd;
    entities.InterstateRequest.OtherStateCaseClosureReason =
      otherStateCaseClosureReason;
    entities.InterstateRequest.OtherStateCaseClosureDate =
      otherStateCaseClosureDate;
    entities.InterstateRequest.Country = country;
    entities.InterstateRequest.Populated = true;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of Incoming.
    /// </summary>
    [JsonPropertyName("incoming")]
    public InterstateCase Incoming
    {
      get => incoming ??= new();
      set => incoming = value;
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

    private InterstateRequest interstateRequest;
    private InterstateCase incoming;
    private Case1 case1;
    private CsePerson ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateRequest interstateRequest;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private DateWorkArea current;
    private InterstateRequest interstateRequest;
    private Program program;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private Case1 case1;
    private CaseRole absentParent;
    private CsePerson ap;
    private InterstateRequest interstateRequest;
  }
#endregion
}
