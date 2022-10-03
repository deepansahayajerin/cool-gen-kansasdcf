// Program: FN_RETRIEVE_INTERSTATE_REQUEST, ID: 372084593, model: 746.
// Short name: SWE02249
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RETRIEVE_INTERSTATE_REQUEST.
/// </summary>
[Serializable]
public partial class FnRetrieveInterstateRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RETRIEVE_INTERSTATE_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRetrieveInterstateRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRetrieveInterstateRequest.
  /// </summary>
  public FnRetrieveInterstateRequest(IContext context, Import import,
    Export export):
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
    // =================================================
    // Created 10/29/98 - Bud Adams
    // Interstate obligations must have the proper Interstate Case
    // Number assigned to them.  Each procedure had embedded
    // CRUD actions to do this - and it was way wrong, allowing an
    // interstate obligation to be associated with any IC Number as
    // long as the state code matched.
    // This is involved in every obligation maintenance screen
    // and it makes sense to only develop, test, and fix this
    // function one place.
    // K.Price 4/19/2000 - Normally this module is called and
    // passed a Legal Action.  It is sometimes necessary to validate
    // if the interstate request even exists.  An IF statement was
    // added to check for a Legal Action.  If one exists the normal
    // logic flow is used, otherwise the Interstate Request was is
    // just read to see if it exists.
    // Also added logic to look up Interstate Request using Country
    // Code.  When present both Country Code and State Codes
    // are validated.
    // +++++++++++++++++++++++++++++++++++++++++++++++++
    // Required imports:
    // 	Interstate_Request Other_State_Case_ID (user entered)
    // 	Obligation Other_State_Abbr
    // 	Legal_Action Identifier
    // 	Legal_Action_Detail Number
    // Exports:
    // 	Interstate Request
    // Exit States:
    //    If "Interstate request not found", then light up Interstate_Request
    //       Other_State_Case_ID on the screen as being an ERROR.
    //    If "FIPS for the state not found", then light up Obligation
    //       Other_State_Abbr on the screen as being in ERROR
    //    Any other exit state indicates a database error.
    // =================================================
    // =================================================================================
    // 06/22/2006               GVandy              WR# 230751
    // Add capability to select tribal interstate request.
    // ===================================================================================
    local.InterstateRequest.OtherStateCaseStatus = "O";
    local.Ap.Type1 = "AP";
    local.Ar.Type1 = "AR";
    export.InterstateRequest.Assign(import.InterstateRequest);
    local.LegalAction.Identifier = import.LegalAction.Identifier;
    local.LegalActionDetail.Number = import.LegalActionDetail.Number;
    export.Country.Description = Spaces(CodeValue.Description_MaxLength);

    if (!IsEmpty(export.InterstateRequest.TribalAgency))
    {
      local.Code.CodeName = "TRIBAL IV-D AGENCIES";
      local.CodeValue.Cdvalue = export.InterstateRequest.TribalAgency ?? Spaces
        (10);
      UseCabValidateCodeValue2();

      if (AsChar(local.Error.Flag) != 'Y')
      {
        ExitState = "FN0000_INVALID_TRIBAL_AGENCY";

        return;
      }
    }

    if (!IsEmpty(export.InterstateRequest.Country))
    {
      local.Code.CodeName = "COUNTRY CODE";
      local.CodeValue.Cdvalue = export.InterstateRequest.Country ?? Spaces(10);
      UseCabValidateCodeValue2();

      if (AsChar(local.Error.Flag) != 'Y')
      {
        ExitState = "LE0000_INVALID_COUNTRY_CODE";

        return;
      }
    }
    else
    {
    }

    if (!IsEmpty(import.Obligor.OtherStateAbbr))
    {
      local.Code.CodeName = "STATE CODE";
      local.CodeValue.Cdvalue = import.Obligor.OtherStateAbbr ?? Spaces(10);
      UseCabValidateCodeValue1();

      if (AsChar(local.Error.Flag) != 'Y')
      {
        ExitState = "ACO_NE0000_INVALID_STATE_CODE";

        return;
      }

      if (ReadFips2())
      {
        local.Fips.Assign(entities.Fips);
      }
      else if (ReadFips1())
      {
        local.Fips.Assign(entities.Fips);
      }
      else
      {
        ExitState = "FN0000_FIPS_FOR_THE_STATE_NF";

        return;
      }
    }

    if (local.LegalAction.Identifier == 0)
    {
      if (!IsEmpty(import.InterstateRequest.OtherStateCaseId))
      {
        if (!IsEmpty(export.InterstateRequest.Country))
        {
          if (ReadInterstateRequest3())
          {
            export.InterstateRequest.Assign(entities.InterstateRequest);

            return;
          }

          ExitState = "FN0000_INVALID_COUNTRY_INTERSTAT";
        }
        else if (!IsEmpty(export.InterstateRequest.TribalAgency))
        {
          if (ReadInterstateRequest5())
          {
            export.InterstateRequest.Assign(entities.InterstateRequest);

            return;
          }

          ExitState = "FN0000_INVALID_TRIBAL_INTERSTAT";
        }
        else if (!IsEmpty(import.Obligor.OtherStateAbbr))
        {
          if (ReadInterstateRequest4())
          {
            export.InterstateRequest.Assign(entities.InterstateRequest);

            return;
          }

          ExitState = "FN0000_INVALID_STATE_INTERSTATE";
        }
      }
    }
    else
    {
      if (ReadLegalActionLegalActionDetailLegalActionPerson())
      {
        // =================================================
        // 4/23/99 - bud adams  -  added the start date / end date
        //   selection criteria for Case_Role
        // =================================================
        foreach(var item in ReadLaPersonLaCaseRoleLegalActionCaseRoleCaseRole())
        {
          // =================================================
          // 3/9/1999 - bud adams  -  If the transaction is OREC, then the
          //   obligor can be either the AP or the AR.  Otherwise, it can
          //   only be the AP.
          //   Removed the 'case_role type = AP' from the Read qualifier
          // =================================================
          if (Equal(global.TranCode, "SR34"))
          {
            if (Equal(entities.CaseRole.Type1, local.Ap.Type1) || Equal
              (entities.CaseRole.Type1, local.Ar.Type1))
            {
            }
            else
            {
              continue;
            }
          }
          else if (Equal(entities.CaseRole.Type1, local.Ap.Type1))
          {
          }
          else
          {
            continue;
          }

          // ---------------------------------------------------------------------------------------
          // PR# 158947  The  READ EACH 'interstate_request'  is  added since 
          // below READ 'case' is failing. Commneted the READ 'case' statement .
          // If this CAB is called in PSTEP while 'DISPLAY', do not qualify the 
          // READEACH with 'Interstate_Request'  Other_state_case_status. There'
          // s data in production, where the user closed the interstate_request
          // but the obligations associated to them are active.
          // In all other situations ( UPDATE),  check to make sure the '
          // Interstate_Request'  Other_state_case_status is 'O' (Open) so that
          // an obligation can only be linked to an active 'Interstate_Request'.
          // ---------------------------------------------------------------------------------------
          if (Equal(global.Command, "DISPLAY"))
          {
            if (ReadInterstateRequest2())
            {
              export.InterstateRequest.Assign(entities.InterstateRequest);

              goto Read;
            }
          }
          else if (ReadInterstateRequest1())
          {
            export.InterstateRequest.Assign(entities.InterstateRequest);

            goto Read;
          }
        }
      }
      else if (ReadLegalAction())
      {
        if (ReadLegalActionDetail())
        {
          ExitState = "LEGAL_ACTION_PERSON_NF";
        }
        else
        {
          ExitState = "LEGAL_ACTION_DETAIL_NF";
        }
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";
      }

Read:

      if (IsExitState("ACO_NN0000_ALL_OK") && export
        .InterstateRequest.IntHGeneratedId == 0)
      {
        ExitState = "FN0000_INTERSTATE_AP_MISMATCH";
      }
    }
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
    export.Country.Description = useExport.CodeValue.Description;
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", import.Obligor.OtherStateAbbr ?? "");
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.
          SetNullableString(command, "casINumber", entities.CaseRole.CasNumber);
          
        db.SetString(
          command, "othStCaseStatus",
          local.InterstateRequest.OtherStateCaseStatus);
        db.SetNullableString(
          command, "otherStateCasId",
          import.InterstateRequest.OtherStateCaseId ?? "");
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", export.InterstateRequest.TribalAgency ?? ""
          );
        db.SetInt32(command, "state", local.Fips.State);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 5);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.
          SetNullableString(command, "casINumber", entities.CaseRole.CasNumber);
          
        db.SetNullableString(
          command, "otherStateCasId",
          import.InterstateRequest.OtherStateCaseId ?? "");
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", export.InterstateRequest.TribalAgency ?? ""
          );
        db.SetInt32(command, "state", local.Fips.State);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 5);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest3()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "otherStateCasId",
          import.InterstateRequest.OtherStateCaseId ?? "");
        db.SetString(
          command, "othStCaseStatus",
          local.InterstateRequest.OtherStateCaseStatus);
        db.SetNullableString(
          command, "country", import.InterstateRequest.Country ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 5);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest4()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "otherStateCasId",
          import.InterstateRequest.OtherStateCaseId ?? "");
        db.SetString(
          command, "othStCaseStatus",
          local.InterstateRequest.OtherStateCaseStatus);
        db.SetInt32(command, "othrStateFipsCd", local.Fips.State);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 5);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest5()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "otherStateCasId",
          import.InterstateRequest.OtherStateCaseId ?? "");
        db.SetString(
          command, "othStCaseStatus",
          local.InterstateRequest.OtherStateCaseStatus);
        db.SetNullableString(
          command, "tribalAgency", import.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 5);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLaPersonLaCaseRoleLegalActionCaseRoleCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.LegalActionCaseRole.Populated = false;
    entities.LaPersonLaCaseRole.Populated = false;

    return ReadEach("ReadLaPersonLaCaseRoleLegalActionCaseRoleCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lapId", entities.LegalActionPerson.Identifier);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 0);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 1);
        entities.CaseRole.Identifier = db.GetInt32(reader, 1);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 3);
        entities.CaseRole.CspNumber = db.GetString(reader, 3);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 4);
        entities.CaseRole.CasNumber = db.GetString(reader, 4);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 7);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 9);
        entities.CaseRole.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        entities.LaPersonLaCaseRole.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "laDetailNo", local.LegalActionDetail.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.Populated = true;
      });
  }

  private bool ReadLegalActionLegalActionDetailLegalActionPerson()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;
    entities.LegalActionPerson.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadLegalActionLegalActionDetailLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
        db.SetNullableInt32(
          command, "ladRNumber", local.LegalActionDetail.Number);
        db.SetNullableString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 2);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 3);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 3);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 5);
        entities.CsePerson.Number = db.GetString(reader, 5);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
        entities.LegalActionPerson.Populated = true;
        entities.CsePerson.Populated = true;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public Obligation Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Obligation obligor;
    private InterstateRequest interstateRequest;
    private CsePerson csePerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
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

    private CodeValue country;
    private InterstateRequest interstateRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      legalActionDetail = null;
      legalAction = null;
      error = null;
      code = null;
      codeValue = null;
      fips = null;
    }

    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private Common error;
    private Code code;
    private CodeValue codeValue;
    private CaseRole ap;
    private InterstateRequest interstateRequest;
    private CaseRole ar;
    private Fips fips;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private Tribunal tribunal;
    private Fips fips;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private InterstateRequest interstateRequest;
    private Case1 case1;
    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
    private CsePerson csePerson;
    private Obligation obligation;
    private CsePersonAccount obligor;
  }
#endregion
}
