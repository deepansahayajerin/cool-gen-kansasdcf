// Program: SI_CAB_CREATE_IS_REQUEST, ID: 373465505, model: 746.
// Short name: SWE02794
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_CREATE_IS_REQUEST.
/// </summary>
[Serializable]
public partial class SiCabCreateIsRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_CREATE_IS_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabCreateIsRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabCreateIsRequest.
  /// </summary>
  public SiCabCreateIsRequest(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (Equal(import.InterstateRequest.CreatedTimestamp,
      local.InterstateRequest.CreatedTimestamp))
    {
      local.InterstateRequest.CreatedTimestamp = Now();
    }
    else
    {
      local.InterstateRequest.CreatedTimestamp =
        import.InterstateRequest.CreatedTimestamp;
    }

    if (IsEmpty(import.InterstateRequest.CreatedBy))
    {
      local.InterstateRequest.CreatedBy = global.UserId;
    }
    else
    {
      local.InterstateRequest.CreatedBy = import.InterstateRequest.CreatedBy;
    }

    local.InterstateRequest.LastUpdatedBy = local.InterstateRequest.CreatedBy;
    local.InterstateRequest.LastUpdatedTimestamp =
      local.InterstateRequest.CreatedTimestamp;
    local.Current.Date = Date(local.InterstateRequest.CreatedTimestamp);

    if (!IsEmpty(import.Case1.Number))
    {
      local.Case1.Number = import.Case1.Number;
    }
    else if (!IsEmpty(import.Incoming.KsCaseId))
    {
      local.Case1.Number = import.Incoming.KsCaseId ?? Spaces(10);
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadCaseRole())
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    // @@@
    local.InterstateRequest.OtherStateFips =
      import.InterstateRequest.OtherStateFips;
    local.InterstateRequest.Country = import.InterstateRequest.Country ?? "";
    local.InterstateRequest.TribalAgency =
      import.InterstateRequest.TribalAgency ?? "";

    // -----------------------------------------------------------
    // Confirm there is not existing interstate involvement
    // for that state, whether active or inactive, incoming or outgoing.
    // If there is existing interstate involvement, that one needs to
    // be updated, instead of adding a new one.
    // -----------------------------------------------------------
    foreach(var item in ReadInterstateRequest2())
    {
      if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'O')
      {
        ExitState = "INTERSTATE_CASE_AE_FOR_THE_COMBN";

        return;
      }
      else
      {
        local.InterstateRequest.IntHGeneratedId =
          entities.InterstateRequest.IntHGeneratedId;

        break;
      }
    }

    // ------------------------------------------
    // Validate Ks_Case_Ind, Other Case Status and
    // Closure Reason
    // Valid values for Ks_Case_Ind are Y (Outgoing),
    // N (Incoming) and <spaces> (LO1 and CSI)
    // ------------------------------------------
    switch(AsChar(import.InterstateRequest.KsCaseInd))
    {
      case 'Y':
        local.InterstateRequest.KsCaseInd = "Y";
        local.InterstateRequest.OtherStateCaseStatus = "O";
        local.InterstateRequest.OtherStateCaseClosureReason = "";
        local.InterstateRequest.CaseType =
          import.InterstateRequest.CaseType ?? "";

        break;
      case 'N':
        local.InterstateRequest.KsCaseInd = "N";

        if (!IsEmpty(import.InterstateRequest.OtherStateCaseStatus))
        {
          local.InterstateRequest.OtherStateCaseStatus =
            import.InterstateRequest.OtherStateCaseStatus;
        }
        else if (!IsEmpty(import.Incoming.CaseStatus))
        {
          local.InterstateRequest.OtherStateCaseStatus =
            import.Incoming.CaseStatus;
        }

        switch(AsChar(local.InterstateRequest.OtherStateCaseStatus))
        {
          case 'O':
            local.InterstateRequest.OtherStateCaseClosureReason = "";

            break;
          case 'C':
            local.InterstateRequest.OtherStateCaseClosureReason =
              import.InterstateRequest.OtherStateCaseClosureReason ?? "";

            break;
          default:
            local.InterstateRequest.OtherStateCaseStatus = "O";
            local.InterstateRequest.OtherStateCaseClosureReason = "";

            break;
        }

        if (!IsEmpty(import.InterstateRequest.CaseType))
        {
          local.InterstateRequest.CaseType =
            import.InterstateRequest.CaseType ?? "";
        }
        else if (!IsEmpty(import.Incoming.CaseType))
        {
          local.InterstateRequest.CaseType = import.Incoming.CaseType;
        }
        else
        {
          UseSiReadCaseProgramType();

          if (IsEmpty(local.Program.Code))
          {
            ExitState = "ACO_NN0000_ALL_OK";
            UseSiReadArrOnlyCasePrgType();
          }

          local.InterstateRequest.CaseType = local.Program.Code;
        }

        break;
      case ' ':
        // -----------------------------------------------------------
        // This is only for LO1, CSI or incoming transactions
        // Because it is not possible to send CSENet transactions to
        // other countries, this cannot be used for LO1 nor CSI
        // -----------------------------------------------------------
        if (!IsEmpty(local.InterstateRequest.Country))
        {
          ExitState = "INTERSTATE_REQUEST_PV";

          return;
        }

        if (!IsEmpty(import.Incoming.InterstateCaseId))
        {
          local.InterstateRequest.KsCaseInd = "N";
          local.InterstateRequest.OtherStateCaseStatus =
            import.Incoming.CaseStatus;
          local.InterstateRequest.OtherStateCaseClosureReason = "";

          if (!IsEmpty(import.Incoming.CaseType))
          {
            local.InterstateRequest.CaseType = import.Incoming.CaseType;
          }
          else
          {
            UseSiReadCaseProgramType();

            if (IsEmpty(local.Program.Code))
            {
              ExitState = "ACO_NN0000_ALL_OK";
              UseSiReadArrOnlyCasePrgType();
            }

            local.InterstateRequest.CaseType = local.Program.Code;
          }
        }

        break;
      default:
        ExitState = "INTERSTATE_REQUEST_PV";

        return;
    }

    if (IsEmpty(import.Previous.Command))
    {
      export.Previous.Command = global.Command;

      return;
    }
    else
    {
      export.Previous.Command = "";
    }

    if (!IsEmpty(import.InterstateRequest.OtherStateCaseId))
    {
      local.InterstateRequest.OtherStateCaseId =
        import.InterstateRequest.OtherStateCaseId ?? "";
    }
    else if (!IsEmpty(import.Incoming.InterstateCaseId))
    {
      local.InterstateRequest.OtherStateCaseId =
        import.Incoming.InterstateCaseId ?? "";
    }

    // -----------------------------------------------------------
    // Other State Closure Date is used as the interstate
    // status date, i.e. when we last set the Other State
    // Case Status
    // -----------------------------------------------------------
    local.InterstateRequest.OtherStateCaseClosureDate = local.Current.Date;

    if (local.InterstateRequest.IntHGeneratedId == 0)
    {
      if (ReadInterstateRequest1())
      {
        local.InterstateRequest.IntHGeneratedId =
          entities.KeyOnly.IntHGeneratedId + 1;
      }

      local.Common.Count = 0;

      while(local.Common.Count < 25)
      {
        try
        {
          CreateInterstateRequest();
          export.InterstateRequest.IntHGeneratedId =
            entities.InterstateRequest.IntHGeneratedId;

          return;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ++local.Common.Count;
              ++local.InterstateRequest.IntHGeneratedId;

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_REQUEST_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      ExitState = "INTERSTATE_REQUEST_AE";
    }
    else
    {
      UseSiCabUpdateIsRequest();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      export.InterstateRequest.IntHGeneratedId =
        local.InterstateRequest.IntHGeneratedId;
    }
  }

  private void UseSiCabUpdateIsRequest()
  {
    var useImport = new SiCabUpdateIsRequest.Import();
    var useExport = new SiCabUpdateIsRequest.Export();

    useImport.InterstateRequest.Assign(local.InterstateRequest);

    Call(SiCabUpdateIsRequest.Execute, useImport, useExport);
  }

  private void UseSiReadArrOnlyCasePrgType()
  {
    var useImport = new SiReadArrOnlyCasePrgType.Import();
    var useExport = new SiReadArrOnlyCasePrgType.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadArrOnlyCasePrgType.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void CreateInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var intHGeneratedId = local.InterstateRequest.IntHGeneratedId;
    var otherStateCaseId = local.InterstateRequest.OtherStateCaseId ?? "";
    var otherStateFips = local.InterstateRequest.OtherStateFips;
    var createdBy = local.InterstateRequest.CreatedBy;
    var createdTimestamp = local.InterstateRequest.CreatedTimestamp;
    var lastUpdatedBy = local.InterstateRequest.LastUpdatedBy;
    var lastUpdatedTimestamp = local.InterstateRequest.LastUpdatedTimestamp;
    var otherStateCaseStatus = local.InterstateRequest.OtherStateCaseStatus;
    var caseType = local.InterstateRequest.CaseType ?? "";
    var ksCaseInd = local.InterstateRequest.KsCaseInd;
    var otherStateCaseClosureReason =
      local.InterstateRequest.OtherStateCaseClosureReason ?? "";
    var otherStateCaseClosureDate =
      local.InterstateRequest.OtherStateCaseClosureDate;
    var casINumber = entities.Case1.Number;
    var casNumber = entities.CaseRole.CasNumber;
    var cspNumber = entities.CaseRole.CspNumber;
    var croType = entities.CaseRole.Type1;
    var croId = entities.CaseRole.Identifier;
    var country = local.InterstateRequest.Country ?? "";
    var tribalAgency = local.InterstateRequest.TribalAgency ?? "";

    CheckValid<InterstateRequest>("CroType", croType);
    entities.InterstateRequest.Populated = false;
    Update("CreateInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", intHGeneratedId);
        db.SetNullableString(command, "otherStateCasId", otherStateCaseId);
        db.SetInt32(command, "othrStateFipsCd", otherStateFips);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "othStCaseStatus", otherStateCaseStatus);
        db.SetNullableString(command, "caseType", caseType);
        db.SetString(command, "ksCaseInd", ksCaseInd);
        db.SetNullableString(
          command, "othStateClsRes", otherStateCaseClosureReason);
        db.
          SetNullableDate(command, "othStateClsDte", otherStateCaseClosureDate);
          
        db.SetNullableString(command, "casINumber", casINumber);
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "croType", croType);
        db.SetNullableInt32(command, "croId", croId);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "tribalAgency", tribalAgency);
      });

    entities.InterstateRequest.IntHGeneratedId = intHGeneratedId;
    entities.InterstateRequest.OtherStateCaseId = otherStateCaseId;
    entities.InterstateRequest.OtherStateFips = otherStateFips;
    entities.InterstateRequest.CreatedBy = createdBy;
    entities.InterstateRequest.CreatedTimestamp = createdTimestamp;
    entities.InterstateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterstateRequest.OtherStateCaseStatus = otherStateCaseStatus;
    entities.InterstateRequest.CaseType = caseType;
    entities.InterstateRequest.KsCaseInd = ksCaseInd;
    entities.InterstateRequest.OtherStateCaseClosureReason =
      otherStateCaseClosureReason;
    entities.InterstateRequest.OtherStateCaseClosureDate =
      otherStateCaseClosureDate;
    entities.InterstateRequest.CasINumber = casINumber;
    entities.InterstateRequest.CasNumber = casNumber;
    entities.InterstateRequest.CspNumber = cspNumber;
    entities.InterstateRequest.CroType = croType;
    entities.InterstateRequest.CroId = croId;
    entities.InterstateRequest.Country = country;
    entities.InterstateRequest.TribalAgency = tribalAgency;
    entities.InterstateRequest.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.KeyOnly.Populated = false;

    return Read("ReadInterstateRequest1",
      null,
      (db, reader) =>
      {
        entities.KeyOnly.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.KeyOnly.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.CaseRole.Identifier);
        db.SetNullableString(command, "croType", entities.CaseRole.Type1);
        db.SetNullableString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetNullableString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetInt32(
          command, "otherStateFips", local.InterstateRequest.OtherStateFips);
        db.SetNullableString(
          command, "country", local.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", local.InterstateRequest.TribalAgency ?? "");
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
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
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

    /// <summary>
    /// A value of Incoming.
    /// </summary>
    [JsonPropertyName("incoming")]
    public InterstateCase Incoming
    {
      get => incoming ??= new();
      set => incoming = value;
    }

    private Common previous;
    private InterstateRequest interstateRequest;
    private CsePerson ap;
    private Case1 case1;
    private InterstateCase incoming;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
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

    private Common previous;
    private InterstateRequest interstateRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private Case1 case1;
    private Common common;
    private InterstateRequest interstateRequest;
    private DateWorkArea current;
    private Program program;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public InterstateRequest KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
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

    private InterstateRequest keyOnly;
    private InterstateRequest interstateRequest;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
