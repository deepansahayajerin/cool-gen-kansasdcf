// Program: OE_1099_DETAILS, ID: 372359199, model: 746.
// Short name: SWE1099P
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_1099_DETAILS.
/// </para>
/// <para>
/// Rsp: OBLGEST
/// This procedure handles the on-line display and deletion of 1099 Responses 
/// received from the IRS.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class Oe1099Details: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_1099_DETAILS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Oe1099Details(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Oe1099Details.
  /// </summary>
  public Oe1099Details(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // unknown   	MM/DD/YY	Initial Code
    // T.O.Redmond	02/06/96	Retrofit
    // T.O.Redmond	04/17/96	Remove Delete Key (Retain Delete Logic) - Add logic 
    // to only allow  a manual request to be created for the same person within
    // a 100 day (Three Month) time span.
    // 				Add Create logic and dissalow creation if a prior request exists 
    // within the past 100 days or if there is no Social Security Number for the
    // CSE Person Requested.
    // R. Marchman	11/19/96	Add new security and next tran.
    // 07/05/97        H. Kennedy      Added exception logic for when not
    //                                 
    // found in all CABS.
    // MK		9/98
    // 1. Added Make cse person number ERROR.
    // 2. Added
    // MAKE export list cse persons standard prompt field ERROR
    // following EXIT STATE Must Select for Prompt.
    // 3. Added validation for cse number.
    // 4. Modified Make statement to MAKE cse person workset SSN ERROR.
    // 5. Modified CREATE CAB to return Name and SSN to display on screen.
    // 6. Modified Security call.
    // E. Lyman	02/02/99	Added new security action block.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";

    // ************************************************
    // *Move import security views to export.         *
    // ************************************************
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    export.StartingCase.Number = import.StartingCase.Number;

    // ************************************************
    // *Put leading zeroes on Case number             *
    // ************************************************
    if (IsEmpty(import.StartingCase.Number))
    {
      export.StartingCase.Number = import.StartingCase.Number;
    }
    else
    {
      local.TextWorkArea.Text10 = import.StartingCase.Number;
      UseEabPadLeftWithZeros();
      export.StartingCase.Number = local.TextWorkArea.Text10;
    }

    // ************************************************
    // *Put leading zeroes on CSE Person              *
    // ************************************************
    if (IsEmpty(import.StartingCsePerson.Number))
    {
      export.PersonName.FormattedName = "";
      export.StartingCsePerson.Number = "";
    }
    else
    {
      local.TextWorkArea.Text10 = import.StartingCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.StartingCsePerson.Number = local.TextWorkArea.Text10;
    }

    export.StartingCsePersonsWorkSet.Assign(import.StartingCsePersonsWorkSet);
    export.ListCsePersons.PromptField = import.Prompt.PromptField;

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    // ---------------------------------------------
    // Move imports to exports
    // ---------------------------------------------
    export.Data1099LocateRequest.Assign(import.Data1099LocateRequest);
    export.Data1099LocateResponse.Assign(import.Data1099LocateResponse);
    export.ListCsePersons.PromptField = import.Prompt.PromptField;
    export.AmountInd1.Description = import.AmountInd1.Description;
    export.AmountInd2.Description = import.AmountInd2.Description;
    export.AmountInd3.Description = import.AmountInd3.Description;
    export.AmountInd4.Description = import.AmountInd4.Description;
    export.AmountInd5.Description = import.AmountInd5.Description;
    export.AmountInd6.Description = import.AmountInd6.Description;
    export.AmountInd7.Description = import.AmountInd7.Description;
    export.AmountInd8.Description = import.AmountInd8.Description;
    export.AmountInd9.Description = import.AmountInd9.Description;
    export.AmountInd10.Description = import.AmountInd10.Description;
    export.AmountInd11.Description = import.AmountInd11.Description;
    export.AmountInd12.Description = import.AmountInd12.Description;
    export.Delete1099Info.Flag = import.Delete1099Info.Flag;
    export.DocumentCode.Description = import.DocumentCode.Description;
    export.ResultCode.Description = import.ResultCode.Description;
    export.SendGarnishment.Flag = import.SendGarnishment.Flag;
    export.SendIwo.Flag = import.SendIwo.Flag;
    export.SendPostmasterLetter.Flag = import.SendPostmasterLetter.Flag;
    export.SendVerificationLetter.Flag = import.SendVerificationLetter.Flag;
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);
    export.Data1099LocateRequest.Assign(import.Data1099LocateRequest);
    export.Data1099LocateResponse.Assign(import.Data1099LocateResponse);

    // ************************************************
    // *Move import hidden views to export.           *
    // ************************************************
    if (Equal(export.StartingCsePerson.Number, import.HiddenCsePerson.Number) &&
      !IsEmpty(import.HiddenCsePerson.Number))
    {
      export.HiddenPrevdata1099LocateRequest.Identifier =
        import.HiddenPrevdata1099LocateRequest.Identifier;
      export.HiddenPrevdata1099LocateResponse.Identifier =
        import.HiddenPrevdata1099LocateResponse.Identifier;
      export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *If the next tran info is not equal to spaces, *
      // *this implies the user requested a next tran   *
      // *action. Now validate that action.             *
      // ************************************************
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.HiddenNextTranInfo.CsePersonNumber =
        export.StartingCsePerson.Number;
      export.HiddenNextTranInfo.CaseNumber = export.StartingCase.Number;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      if (!IsEmpty(export.HiddenNextTranInfo.CaseNumber))
      {
        export.StartingCase.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
          (10);
        export.StartingCsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
      }

      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCOMP") || Equal
      (global.Command, "RETADDR") || Equal(global.Command, "RETNAME"))
    {
      global.Command = "DISPLAY";

      if (IsEmpty(export.StartingCsePersonsWorkSet.Number))
      {
        export.PersonName.FormattedName = "";
        export.StartingCsePerson.Number = "";
      }
      else
      {
        export.StartingCsePerson.Number =
          export.StartingCsePersonsWorkSet.Number;
      }

      export.ListCsePersons.PromptField = "";
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      if (!IsEmpty(export.StartingCsePerson.Number))
      {
        // *** New security action block added 2/2/99 by E. Lyman ***
        UseCoCabIsUserAssignedToCase();

        if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.StartingCsePerson, "number");

          field.Error = true;

          export.Data1099LocateRequest.Identifier = 0;
          export.Data1099LocateResponse.Identifier = 0;

          return;
        }
        else
        {
        }

        if (AsChar(local.OnTheCase.Flag) != 'Y')
        {
          var field = GetField(export.StartingCsePerson, "number");

          field.Error = true;

          export.Data1099LocateRequest.Identifier = 0;
          export.Data1099LocateResponse.Identifier = 0;
          ExitState = "SC0001_USER_NOT_AUTH_COMMAND";

          return;
        }
      }
    }

    local.UserAction.Command = global.Command;

    // ---------------------------------------------
    // Command DISPLAY displays given request and its response
    // Command PREV displays previous response/ request.
    // Command NEXT displays next response/request.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      export.Delete1099Info.Flag = "";
      export.SendGarnishment.Flag = "";
      export.SendIwo.Flag = "";
      export.SendPostmasterLetter.Flag = "";
      export.SendVerificationLetter.Flag = "";

      if (IsEmpty(export.StartingCsePerson.Number))
      {
        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

        var field = GetField(export.StartingCsePerson, "number");

        field.Error = true;

        return;
      }

      UseRead1099LocateResponse();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("CSE_PERSON_NF"))
        {
          // ************************************************
          // Added Make cse person number ERROR.
          // MK 9/98
          // ************************************************
          var field = GetField(export.StartingCsePerson, "number");

          field.Error = true;

          export.Data1099LocateRequest.Identifier = 0;
          export.Data1099LocateResponse.Identifier = 0;
        }
        else if (IsExitState("1099_LOCATE_REQUEST_NF"))
        {
          var field = GetField(export.StartingCsePerson, "number");

          field.Error = true;

          export.Data1099LocateRequest.Identifier = 0;
          export.Data1099LocateResponse.Identifier = 0;
        }
        else if (IsExitState("1099_LOCATE_RESPONSE_NF"))
        {
          var field = GetField(export.Data1099LocateResponse, "identifier");

          field.Error = true;

          export.Data1099LocateResponse.Identifier = 0;
        }
        else
        {
        }
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        export.HiddenCsePerson.Number = export.StartingCsePerson.Number;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        return;
      case "PREV":
        return;
      case "NEXT":
        return;
      case "SIGNOFF":
        return;
      case "ADDR":
        ExitState = "ECO_LNK_TO_ADDRESS_MAINTENANCE";

        return;
      case "INCS":
        ExitState = "ECO_LNK_TO_INCOME_SOURCE_DETAILS";

        return;
      case "RESL":
        ExitState = "ECO_LNK_TO_RESOURCE_LIST";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "LIST":
        if (!IsEmpty(import.Prompt.PromptField) && AsChar
          (import.Prompt.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.ListCsePersons, "promptField");

          field1.Error = true;

          break;
        }

        if (AsChar(export.ListCsePersons.PromptField) == 'S')
        {
          if (!IsEmpty(export.StartingCase.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }

          break;
        }

        // **********************************
        // Added
        // MAKE export list cse persons standard
        // prompt field ERROR
        // following EXIT STATE 'Must Select for Prompt'
        // MK 9/98
        // **********************************
        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        var field = GetField(export.ListCsePersons, "promptField");

        field.Error = true;

        break;
      case "ADD":
        // ************************************************
        // Added validation for cse person number.
        // MK 9/98
        // ************************************************
        if (IsEmpty(export.StartingCsePerson.Number))
        {
          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          var field1 = GetField(export.StartingCsePerson, "number");

          field1.Error = true;

          return;
        }

        // ************************************************
        // Modified CREATE CAB to return Name and SSN
        // to display on screen.
        // MK 9/98
        // ************************************************
        UseCreate1099Locate();

        // ************************************************
        // SSN MUST BE PROVIDED WHEN REQUESTING
        // INFORMATION FROM THE IRS
        // ************************************************
        // ************************************************
        // Modified Make statement to MAKE cse person
        // workset SSN ERROR.
        // MK 9/98
        // ************************************************
        if (IsExitState("ACO_NE0000_REQUIRED_FIELD_MISSIN"))
        {
          export.Data1099LocateRequest.Assign(import.Data1099LocateRequest);
          ExitState = "OE0000_SSN_REQUIRED_FOR_1099";

          var field1 = GetField(export.StartingCsePersonsWorkSet, "ssn");

          field1.Error = true;

          return;
        }

        // ************************************************
        // *A request exists that has not yet been sent   *
        // *to the IRS - Cannot add twice.                *
        // ************************************************
        if (IsExitState("CANNOT_ADD_AN_EXISTING_OCCURRENC"))
        {
          export.Data1099LocateRequest.Assign(import.Data1099LocateRequest);

          var field1 =
            GetField(export.Data1099LocateRequest, "requestSentDate");

          field1.Error = true;

          return;
        }

        // ************************************************
        // *Cannot send a new request if a previous       *
        // *request has been sent in the past three months*
        // ************************************************
        if (IsExitState("OE0000_REQUEST_IS_WITHIN_3_MNTHS"))
        {
          export.Data1099LocateRequest.Assign(import.Data1099LocateRequest);

          return;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          var field1 = GetField(export.StartingCsePerson, "number");

          field1.Error = true;

          return;
        }
        else
        {
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_ADD_2";
        }

        break;
      case "UPDATE":
        break;
      case "DELETE":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // ---------------------------------------------
    // Save the current views and command in hidden views for checking in the 
    // next pass.
    // ---------------------------------------------
    export.HiddenPrevdata1099LocateRequest.Identifier =
      export.Data1099LocateRequest.Identifier;
    export.HiddenPrevdata1099LocateResponse.Identifier =
      export.Data1099LocateResponse.Identifier;
    export.HiddenCsePerson.Number = export.StartingCsePerson.Number;
    export.HiddenPrevUserAction.Command = global.Command;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private void UseCoCabIsUserAssignedToCase()
  {
    var useImport = new CoCabIsUserAssignedToCase.Import();
    var useExport = new CoCabIsUserAssignedToCase.Export();

    useImport.CsePerson.Number = export.StartingCsePerson.Number;

    Call(CoCabIsUserAssignedToCase.Execute, useImport, useExport);

    local.OnTheCase.Flag = useExport.OnTheCase.Flag;
  }

  private void UseCreate1099Locate()
  {
    var useImport = new Create1099Locate.Import();
    var useExport = new Create1099Locate.Export();

    useImport.Data1099LocateRequest.Assign(import.Data1099LocateRequest);
    useImport.CsePerson.Number = export.StartingCsePerson.Number;

    Call(Create1099Locate.Execute, useImport, useExport);

    export.StartingCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.Data1099LocateRequest.Assign(useExport.Data1099LocateRequest);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseRead1099LocateResponse()
  {
    var useImport = new Read1099LocateResponse.Import();
    var useExport = new Read1099LocateResponse.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    useImport.CsePerson.Number = export.StartingCsePerson.Number;
    useImport.Data1099LocateRequest.Identifier =
      export.Data1099LocateRequest.Identifier;
    useImport.Data1099LocateResponse.Identifier =
      export.Data1099LocateResponse.Identifier;

    Call(Read1099LocateResponse.Execute, useImport, useExport);

    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
    export.StartingCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.ResultCode.Description = useExport.Export1099ResultCode.Description;
    export.DocumentCode.Description =
      useExport.Export1099DocumentCode.Description;
    export.AmountInd1.Description = useExport.AmountInd1.Description;
    export.AmountInd2.Description = useExport.AmountInd2.Description;
    export.AmountInd3.Description = useExport.AmountInd3.Description;
    export.AmountInd4.Description = useExport.AmountInd4.Description;
    export.AmountInd5.Description = useExport.AmountInd5.Description;
    export.AmountInd6.Description = useExport.AmountInd6.Description;
    export.AmountInd7.Description = useExport.AmountInd7.Description;
    export.AmountInd8.Description = useExport.AmountInd8.Description;
    export.AmountInd9.Description = useExport.AmountInd9.Description;
    export.AmountInd10.Description = useExport.AmountInd10.Description;
    export.AmountInd11.Description = useExport.AmountInd11.Description;
    export.AmountInd12.Description = useExport.AmountInd12.Description;
    export.StartingCsePerson.Number = useExport.CsePerson.Number;
    export.Data1099LocateResponse.Assign(useExport.Data1099LocateResponse);
    export.Data1099LocateRequest.Assign(useExport.Data1099LocateRequest);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.Case1.Number = import.StartingCase.Number;
    useImport.CsePerson.Number = import.StartingCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Hiddendata1099LocateRequest.
    /// </summary>
    [JsonPropertyName("hiddendata1099LocateRequest")]
    public Data1099LocateRequest Hiddendata1099LocateRequest
    {
      get => hiddendata1099LocateRequest ??= new();
      set => hiddendata1099LocateRequest = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of StartingCase.
    /// </summary>
    [JsonPropertyName("startingCase")]
    public Case1 StartingCase
    {
      get => startingCase ??= new();
      set => startingCase = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Standard Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of StartingCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("startingCsePersonsWorkSet")]
    public CsePersonsWorkSet StartingCsePersonsWorkSet
    {
      get => startingCsePersonsWorkSet ??= new();
      set => startingCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevdata1099LocateResponse.
    /// </summary>
    [JsonPropertyName("hiddenPrevdata1099LocateResponse")]
    public Data1099LocateResponse HiddenPrevdata1099LocateResponse
    {
      get => hiddenPrevdata1099LocateResponse ??= new();
      set => hiddenPrevdata1099LocateResponse = value;
    }

    /// <summary>
    /// A value of HiddenPrevdata1099LocateRequest.
    /// </summary>
    [JsonPropertyName("hiddenPrevdata1099LocateRequest")]
    public Data1099LocateRequest HiddenPrevdata1099LocateRequest
    {
      get => hiddenPrevdata1099LocateRequest ??= new();
      set => hiddenPrevdata1099LocateRequest = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePerson")]
    public CsePerson HiddenPrevCsePerson
    {
      get => hiddenPrevCsePerson ??= new();
      set => hiddenPrevCsePerson = value;
    }

    /// <summary>
    /// A value of ResultCode.
    /// </summary>
    [JsonPropertyName("resultCode")]
    public CodeValue ResultCode
    {
      get => resultCode ??= new();
      set => resultCode = value;
    }

    /// <summary>
    /// A value of SendVerificationLetter.
    /// </summary>
    [JsonPropertyName("sendVerificationLetter")]
    public Common SendVerificationLetter
    {
      get => sendVerificationLetter ??= new();
      set => sendVerificationLetter = value;
    }

    /// <summary>
    /// A value of SendIwo.
    /// </summary>
    [JsonPropertyName("sendIwo")]
    public Common SendIwo
    {
      get => sendIwo ??= new();
      set => sendIwo = value;
    }

    /// <summary>
    /// A value of SendGarnishment.
    /// </summary>
    [JsonPropertyName("sendGarnishment")]
    public Common SendGarnishment
    {
      get => sendGarnishment ??= new();
      set => sendGarnishment = value;
    }

    /// <summary>
    /// A value of SendPostmasterLetter.
    /// </summary>
    [JsonPropertyName("sendPostmasterLetter")]
    public Common SendPostmasterLetter
    {
      get => sendPostmasterLetter ??= new();
      set => sendPostmasterLetter = value;
    }

    /// <summary>
    /// A value of Delete1099Info.
    /// </summary>
    [JsonPropertyName("delete1099Info")]
    public Common Delete1099Info
    {
      get => delete1099Info ??= new();
      set => delete1099Info = value;
    }

    /// <summary>
    /// A value of AmountInd1.
    /// </summary>
    [JsonPropertyName("amountInd1")]
    public CodeValue AmountInd1
    {
      get => amountInd1 ??= new();
      set => amountInd1 = value;
    }

    /// <summary>
    /// A value of AmountInd2.
    /// </summary>
    [JsonPropertyName("amountInd2")]
    public CodeValue AmountInd2
    {
      get => amountInd2 ??= new();
      set => amountInd2 = value;
    }

    /// <summary>
    /// A value of AmountInd3.
    /// </summary>
    [JsonPropertyName("amountInd3")]
    public CodeValue AmountInd3
    {
      get => amountInd3 ??= new();
      set => amountInd3 = value;
    }

    /// <summary>
    /// A value of AmountInd4.
    /// </summary>
    [JsonPropertyName("amountInd4")]
    public CodeValue AmountInd4
    {
      get => amountInd4 ??= new();
      set => amountInd4 = value;
    }

    /// <summary>
    /// A value of AmountInd5.
    /// </summary>
    [JsonPropertyName("amountInd5")]
    public CodeValue AmountInd5
    {
      get => amountInd5 ??= new();
      set => amountInd5 = value;
    }

    /// <summary>
    /// A value of AmountInd6.
    /// </summary>
    [JsonPropertyName("amountInd6")]
    public CodeValue AmountInd6
    {
      get => amountInd6 ??= new();
      set => amountInd6 = value;
    }

    /// <summary>
    /// A value of AmountInd7.
    /// </summary>
    [JsonPropertyName("amountInd7")]
    public CodeValue AmountInd7
    {
      get => amountInd7 ??= new();
      set => amountInd7 = value;
    }

    /// <summary>
    /// A value of AmountInd8.
    /// </summary>
    [JsonPropertyName("amountInd8")]
    public CodeValue AmountInd8
    {
      get => amountInd8 ??= new();
      set => amountInd8 = value;
    }

    /// <summary>
    /// A value of AmountInd9.
    /// </summary>
    [JsonPropertyName("amountInd9")]
    public CodeValue AmountInd9
    {
      get => amountInd9 ??= new();
      set => amountInd9 = value;
    }

    /// <summary>
    /// A value of AmountInd10.
    /// </summary>
    [JsonPropertyName("amountInd10")]
    public CodeValue AmountInd10
    {
      get => amountInd10 ??= new();
      set => amountInd10 = value;
    }

    /// <summary>
    /// A value of AmountInd11.
    /// </summary>
    [JsonPropertyName("amountInd11")]
    public CodeValue AmountInd11
    {
      get => amountInd11 ??= new();
      set => amountInd11 = value;
    }

    /// <summary>
    /// A value of AmountInd12.
    /// </summary>
    [JsonPropertyName("amountInd12")]
    public CodeValue AmountInd12
    {
      get => amountInd12 ??= new();
      set => amountInd12 = value;
    }

    /// <summary>
    /// A value of DocumentCode.
    /// </summary>
    [JsonPropertyName("documentCode")]
    public CodeValue DocumentCode
    {
      get => documentCode ??= new();
      set => documentCode = value;
    }

    /// <summary>
    /// A value of StartingCsePerson.
    /// </summary>
    [JsonPropertyName("startingCsePerson")]
    public CsePerson StartingCsePerson
    {
      get => startingCsePerson ??= new();
      set => startingCsePerson = value;
    }

    /// <summary>
    /// A value of Data1099LocateRequest.
    /// </summary>
    [JsonPropertyName("data1099LocateRequest")]
    public Data1099LocateRequest Data1099LocateRequest
    {
      get => data1099LocateRequest ??= new();
      set => data1099LocateRequest = value;
    }

    /// <summary>
    /// A value of Data1099LocateResponse.
    /// </summary>
    [JsonPropertyName("data1099LocateResponse")]
    public Data1099LocateResponse Data1099LocateResponse
    {
      get => data1099LocateResponse ??= new();
      set => data1099LocateResponse = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Case1 Start
    {
      get => start ??= new();
      set => start = value;
    }

    private Data1099LocateRequest hiddendata1099LocateRequest;
    private CsePerson hiddenCsePerson;
    private ScrollingAttributes scrollingAttributes;
    private Case1 startingCase;
    private Standard prompt;
    private CsePersonsWorkSet startingCsePersonsWorkSet;
    private Common hiddenPrevUserAction;
    private Data1099LocateResponse hiddenPrevdata1099LocateResponse;
    private Data1099LocateRequest hiddenPrevdata1099LocateRequest;
    private CsePerson hiddenPrevCsePerson;
    private CodeValue resultCode;
    private Common sendVerificationLetter;
    private Common sendIwo;
    private Common sendGarnishment;
    private Common sendPostmasterLetter;
    private Common delete1099Info;
    private CodeValue amountInd1;
    private CodeValue amountInd2;
    private CodeValue amountInd3;
    private CodeValue amountInd4;
    private CodeValue amountInd5;
    private CodeValue amountInd6;
    private CodeValue amountInd7;
    private CodeValue amountInd8;
    private CodeValue amountInd9;
    private CodeValue amountInd10;
    private CodeValue amountInd11;
    private CodeValue amountInd12;
    private CodeValue documentCode;
    private CsePerson startingCsePerson;
    private Data1099LocateRequest data1099LocateRequest;
    private Data1099LocateResponse data1099LocateResponse;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Case1 start;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Hiddendata1099LocateRequest.
    /// </summary>
    [JsonPropertyName("hiddendata1099LocateRequest")]
    public Data1099LocateRequest Hiddendata1099LocateRequest
    {
      get => hiddendata1099LocateRequest ??= new();
      set => hiddendata1099LocateRequest = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of StartingCase.
    /// </summary>
    [JsonPropertyName("startingCase")]
    public Case1 StartingCase
    {
      get => startingCase ??= new();
      set => startingCase = value;
    }

    /// <summary>
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
    }

    /// <summary>
    /// A value of StartingCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("startingCsePersonsWorkSet")]
    public CsePersonsWorkSet StartingCsePersonsWorkSet
    {
      get => startingCsePersonsWorkSet ??= new();
      set => startingCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevdata1099LocateResponse.
    /// </summary>
    [JsonPropertyName("hiddenPrevdata1099LocateResponse")]
    public Data1099LocateResponse HiddenPrevdata1099LocateResponse
    {
      get => hiddenPrevdata1099LocateResponse ??= new();
      set => hiddenPrevdata1099LocateResponse = value;
    }

    /// <summary>
    /// A value of HiddenPrevdata1099LocateRequest.
    /// </summary>
    [JsonPropertyName("hiddenPrevdata1099LocateRequest")]
    public Data1099LocateRequest HiddenPrevdata1099LocateRequest
    {
      get => hiddenPrevdata1099LocateRequest ??= new();
      set => hiddenPrevdata1099LocateRequest = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePerson")]
    public CsePerson HiddenPrevCsePerson
    {
      get => hiddenPrevCsePerson ??= new();
      set => hiddenPrevCsePerson = value;
    }

    /// <summary>
    /// A value of ResultCode.
    /// </summary>
    [JsonPropertyName("resultCode")]
    public CodeValue ResultCode
    {
      get => resultCode ??= new();
      set => resultCode = value;
    }

    /// <summary>
    /// A value of SendVerificationLetter.
    /// </summary>
    [JsonPropertyName("sendVerificationLetter")]
    public Common SendVerificationLetter
    {
      get => sendVerificationLetter ??= new();
      set => sendVerificationLetter = value;
    }

    /// <summary>
    /// A value of SendIwo.
    /// </summary>
    [JsonPropertyName("sendIwo")]
    public Common SendIwo
    {
      get => sendIwo ??= new();
      set => sendIwo = value;
    }

    /// <summary>
    /// A value of SendGarnishment.
    /// </summary>
    [JsonPropertyName("sendGarnishment")]
    public Common SendGarnishment
    {
      get => sendGarnishment ??= new();
      set => sendGarnishment = value;
    }

    /// <summary>
    /// A value of SendPostmasterLetter.
    /// </summary>
    [JsonPropertyName("sendPostmasterLetter")]
    public Common SendPostmasterLetter
    {
      get => sendPostmasterLetter ??= new();
      set => sendPostmasterLetter = value;
    }

    /// <summary>
    /// A value of Delete1099Info.
    /// </summary>
    [JsonPropertyName("delete1099Info")]
    public Common Delete1099Info
    {
      get => delete1099Info ??= new();
      set => delete1099Info = value;
    }

    /// <summary>
    /// A value of AmountInd1.
    /// </summary>
    [JsonPropertyName("amountInd1")]
    public CodeValue AmountInd1
    {
      get => amountInd1 ??= new();
      set => amountInd1 = value;
    }

    /// <summary>
    /// A value of AmountInd2.
    /// </summary>
    [JsonPropertyName("amountInd2")]
    public CodeValue AmountInd2
    {
      get => amountInd2 ??= new();
      set => amountInd2 = value;
    }

    /// <summary>
    /// A value of AmountInd3.
    /// </summary>
    [JsonPropertyName("amountInd3")]
    public CodeValue AmountInd3
    {
      get => amountInd3 ??= new();
      set => amountInd3 = value;
    }

    /// <summary>
    /// A value of AmountInd4.
    /// </summary>
    [JsonPropertyName("amountInd4")]
    public CodeValue AmountInd4
    {
      get => amountInd4 ??= new();
      set => amountInd4 = value;
    }

    /// <summary>
    /// A value of AmountInd5.
    /// </summary>
    [JsonPropertyName("amountInd5")]
    public CodeValue AmountInd5
    {
      get => amountInd5 ??= new();
      set => amountInd5 = value;
    }

    /// <summary>
    /// A value of AmountInd6.
    /// </summary>
    [JsonPropertyName("amountInd6")]
    public CodeValue AmountInd6
    {
      get => amountInd6 ??= new();
      set => amountInd6 = value;
    }

    /// <summary>
    /// A value of AmountInd7.
    /// </summary>
    [JsonPropertyName("amountInd7")]
    public CodeValue AmountInd7
    {
      get => amountInd7 ??= new();
      set => amountInd7 = value;
    }

    /// <summary>
    /// A value of AmountInd8.
    /// </summary>
    [JsonPropertyName("amountInd8")]
    public CodeValue AmountInd8
    {
      get => amountInd8 ??= new();
      set => amountInd8 = value;
    }

    /// <summary>
    /// A value of AmountInd9.
    /// </summary>
    [JsonPropertyName("amountInd9")]
    public CodeValue AmountInd9
    {
      get => amountInd9 ??= new();
      set => amountInd9 = value;
    }

    /// <summary>
    /// A value of AmountInd10.
    /// </summary>
    [JsonPropertyName("amountInd10")]
    public CodeValue AmountInd10
    {
      get => amountInd10 ??= new();
      set => amountInd10 = value;
    }

    /// <summary>
    /// A value of AmountInd11.
    /// </summary>
    [JsonPropertyName("amountInd11")]
    public CodeValue AmountInd11
    {
      get => amountInd11 ??= new();
      set => amountInd11 = value;
    }

    /// <summary>
    /// A value of AmountInd12.
    /// </summary>
    [JsonPropertyName("amountInd12")]
    public CodeValue AmountInd12
    {
      get => amountInd12 ??= new();
      set => amountInd12 = value;
    }

    /// <summary>
    /// A value of DocumentCode.
    /// </summary>
    [JsonPropertyName("documentCode")]
    public CodeValue DocumentCode
    {
      get => documentCode ??= new();
      set => documentCode = value;
    }

    /// <summary>
    /// A value of StartingCsePerson.
    /// </summary>
    [JsonPropertyName("startingCsePerson")]
    public CsePerson StartingCsePerson
    {
      get => startingCsePerson ??= new();
      set => startingCsePerson = value;
    }

    /// <summary>
    /// A value of Data1099LocateResponse.
    /// </summary>
    [JsonPropertyName("data1099LocateResponse")]
    public Data1099LocateResponse Data1099LocateResponse
    {
      get => data1099LocateResponse ??= new();
      set => data1099LocateResponse = value;
    }

    /// <summary>
    /// A value of Data1099LocateRequest.
    /// </summary>
    [JsonPropertyName("data1099LocateRequest")]
    public Data1099LocateRequest Data1099LocateRequest
    {
      get => data1099LocateRequest ??= new();
      set => data1099LocateRequest = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of PersonName.
    /// </summary>
    [JsonPropertyName("personName")]
    public CsePersonsWorkSet PersonName
    {
      get => personName ??= new();
      set => personName = value;
    }

    private Data1099LocateRequest hiddendata1099LocateRequest;
    private CsePerson hiddenCsePerson;
    private ScrollingAttributes scrollingAttributes;
    private Case1 startingCase;
    private Standard listCsePersons;
    private CsePersonsWorkSet startingCsePersonsWorkSet;
    private Common hiddenPrevUserAction;
    private Data1099LocateResponse hiddenPrevdata1099LocateResponse;
    private Data1099LocateRequest hiddenPrevdata1099LocateRequest;
    private CsePerson hiddenPrevCsePerson;
    private CodeValue resultCode;
    private Common sendVerificationLetter;
    private Common sendIwo;
    private Common sendGarnishment;
    private Common sendPostmasterLetter;
    private Common delete1099Info;
    private CodeValue amountInd1;
    private CodeValue amountInd2;
    private CodeValue amountInd3;
    private CodeValue amountInd4;
    private CodeValue amountInd5;
    private CodeValue amountInd6;
    private CodeValue amountInd7;
    private CodeValue amountInd8;
    private CodeValue amountInd9;
    private CodeValue amountInd10;
    private CodeValue amountInd11;
    private CodeValue amountInd12;
    private CodeValue documentCode;
    private CsePerson startingCsePerson;
    private Data1099LocateResponse data1099LocateResponse;
    private Data1099LocateRequest data1099LocateRequest;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CsePersonsWorkSet personName;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of OnTheCase.
    /// </summary>
    [JsonPropertyName("onTheCase")]
    public Common OnTheCase
    {
      get => onTheCase ??= new();
      set => onTheCase = value;
    }

    /// <summary>
    /// A value of OkToDelete1099Request.
    /// </summary>
    [JsonPropertyName("okToDelete1099Request")]
    public Common OkToDelete1099Request
    {
      get => okToDelete1099Request ??= new();
      set => okToDelete1099Request = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of ImportStartCase.
    /// </summary>
    [JsonPropertyName("importStartCase")]
    public TextWorkArea ImportStartCase
    {
      get => importStartCase ??= new();
      set => importStartCase = value;
    }

    /// <summary>
    /// A value of StartCsePersonWithZero.
    /// </summary>
    [JsonPropertyName("startCsePersonWithZero")]
    public TextWorkArea StartCsePersonWithZero
    {
      get => startCsePersonWithZero ??= new();
      set => startCsePersonWithZero = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private Common onTheCase;
    private Common okToDelete1099Request;
    private Common userAction;
    private TextWorkArea importStartCase;
    private TextWorkArea startCsePersonWithZero;
    private TextWorkArea textWorkArea;
  }
#endregion
}
