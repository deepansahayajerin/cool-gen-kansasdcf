// Program: SI_VALIDATE_CSENET_TRANS_TYPE, ID: 372853348, model: 746.
// Short name: SWE02861
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_VALIDATE_CSENET_TRANS_TYPE.
/// </para>
/// <para>
/// This action block validates the Functional type code, Action reason code, 
/// Action code and the combination of all three.
/// Example:  ENF is a valid Functional type code, A is a valid Action code, 
/// AADIN is a valid Action reason code, ENF A is a valid combination of
/// Functional type code and Action code, and ENF A AADIN is a valid combination
/// of Functional type code, Action code, and Action reason code.
/// </para>
/// </summary>
[Serializable]
public partial class SiValidateCsenetTransType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_VALIDATE_CSENET_TRANS_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiValidateCsenetTransType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiValidateCsenetTransType.
  /// </summary>
  public SiValidateCsenetTransType(IContext context, Import import,
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
    // ------------------------------------------------------------------
    // Date		Developer	Description
    // ------------------------------------------------------------------
    // 08/05/1999	M Ramirez	Initial development
    // 08/10/1999	M Ramirez	Added BLANK for action reason code
    // 08/10/1999	M Ramirez	Added flags for incoming/outgoing
    // 				case and automatic/manual transactions
    // 01/31/2007     A Hockman   Removed BLANK as a valid option since it was
    //                           never valid, it should have been blanks or 
    // spaces,
    //                           not the word BLANK.
    // 4/16/08            A Hockman      cq 972  287643 was the pr number
    //                                     
    // changed the  code to allow the
    // blanks
    //                                    
    // to go out in the batch job
    // srrun063.
    // 08/06/08       A Hockman  cq5898 changed validation to only check action 
    // reason
    //                       code if greater than spaces.  We added the word 
    // BLANK back in
    //                       so we know that the wrkr has purposely made that 
    // choice.
    //                      and then we immediately convert it to spaces in the 
    // code.
    //                       However we don't want spaces in the table at all 
    // because we
    //                        can't hide it from workers so we just won't 
    // validate if
    //                         it's spaces  because we  know that is good.
    // ------------------------------------------------------------------
    UseOeCabSetMnemonics();

    if (AsChar(import.Automatic.Flag) == 'Y')
    {
      if (AsChar(import.IncomingCase.Flag) == 'Y')
      {
        local.ActionReason.CodeName = "INTERSTATE AUTO INCOMING REASON";
      }
      else
      {
        local.ActionReason.CodeName = "INTERSTATE AUTO OUTGOING REASON";
      }
    }
    else if (AsChar(import.Automatic.Flag) == 'N')
    {
      if (AsChar(import.IncomingCase.Flag) == 'Y')
      {
        local.ActionReason.CodeName = local.LiteralCsenetOgTranReason.CodeName;
      }
      else
      {
        local.ActionReason.CodeName = local.LiteralCsenetOgCaseReason.CodeName;
      }
    }
    else
    {
      local.ActionReason.CodeName = local.LiteralCsenetActionReason.CodeName;
    }

    // ------------------------------------------------------------------
    // Validate Functional Type Code
    // ------------------------------------------------------------------
    local.Code.CodeName = local.LiteralCsenetFunctionalType.CodeName;
    local.CodeValue.Cdvalue = import.InterstateCase.FunctionalTypeCode;
    UseCabValidateCodeValue2();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      export.Error.Text10 = "FUNCTIONAL";

      return;
    }

    // ------------------------------------------------------------------
    // Validate Action Code
    // ------------------------------------------------------------------
    local.Code.CodeName = local.LiteralCsenetActionType.CodeName;
    local.CodeValue.Cdvalue = import.InterstateCase.ActionCode;
    UseCabValidateCodeValue2();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      export.Error.Text10 = "ACTION";

      return;
    }

    // ------------------------------------------------------------------
    // Validate Action Reason Code
    // ------------------------------------------------------------------
    // put this validation below  inside an IF statement because we do want to 
    // allow blank action reason code to go through in cases where it is valid.
    // We don't have blank (spaces) in the table because we don't want staff
    // to choose it, we want them to choose the word BLANK which is in the
    // table, that way we know they made a choice and just didn't leave the
    // field blank by accident.     If they enter something we want it checked
    // to make sure the code is a good one.   cq5898   changed 8/6/08    Anita
    // Hockman
    local.Code.CodeName = local.ActionReason.CodeName;
    local.CodeValue.Cdvalue = import.InterstateCase.ActionReasonCode ?? Spaces
      (10);

    if (!IsEmpty(import.InterstateCase.ActionReasonCode))
    {
      UseCabValidateCodeValue2();

      if (AsChar(local.ValidCode.Flag) != 'Y')
      {
        export.Error.Text10 = "ACTION RSN";

        return;
      }
    }

    // ------------------------------------------------------------------
    // Validate Combination of Function - Action - Action Reason Code
    // ------------------------------------------------------------------
    local.Code.CodeName = local.ActionReason.CodeName;
    local.CodeValue.Cdvalue = import.InterstateCase.ActionReasonCode ?? Spaces
      (10);

    // *** AHockman removed set from if statement below and put it above for 4/
    // 16/08 fix
    local.CrossValidationCode.CodeName = "INTERSTATE TRANS TYPE";
    local.CrossValidationCodeValue.Cdvalue =
      import.InterstateCase.FunctionalTypeCode + " " + import
      .InterstateCase.ActionCode;

    if (!IsEmpty(import.InterstateCase.ActionReasonCode))
    {
      UseCabValidateCodeValue1();

      if (AsChar(local.ValidCode.Flag) != 'Y')
      {
        export.Error.Text10 = "COMBO";
      }
    }
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CrossValidationCode.CodeName = local.CrossValidationCode.CodeName;
    useImport.CrossValidationCodeValue.Cdvalue =
      local.CrossValidationCodeValue.Cdvalue;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.LiteralCsenetFunctionalType.CodeName =
      useExport.CsenetFunctionalType.CodeName;
    local.LiteralCsenetActionType.CodeName =
      useExport.CsenetActionType.CodeName;
    local.LiteralCsenetOgCaseReason.CodeName =
      useExport.CsenetOgCaseReason.CodeName;
    local.LiteralCsenetActionReason.CodeName =
      useExport.CsenetActionReason.CodeName;
    local.LiteralCsenetOgTranReason.CodeName =
      useExport.CsenetOgTranReason.CodeName;
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
    /// A value of IncomingCase.
    /// </summary>
    [JsonPropertyName("incomingCase")]
    public Common IncomingCase
    {
      get => incomingCase ??= new();
      set => incomingCase = value;
    }

    /// <summary>
    /// A value of Automatic.
    /// </summary>
    [JsonPropertyName("automatic")]
    public Common Automatic
    {
      get => automatic ??= new();
      set => automatic = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private Common incomingCase;
    private Common automatic;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public WorkArea Error
    {
      get => error ??= new();
      set => error = value;
    }

    private WorkArea error;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LiteralCsenetActionReason.
    /// </summary>
    [JsonPropertyName("literalCsenetActionReason")]
    public Code LiteralCsenetActionReason
    {
      get => literalCsenetActionReason ??= new();
      set => literalCsenetActionReason = value;
    }

    /// <summary>
    /// A value of ActionReason.
    /// </summary>
    [JsonPropertyName("actionReason")]
    public Code ActionReason
    {
      get => actionReason ??= new();
      set => actionReason = value;
    }

    /// <summary>
    /// A value of LiteralCsenetOgTranReason.
    /// </summary>
    [JsonPropertyName("literalCsenetOgTranReason")]
    public Code LiteralCsenetOgTranReason
    {
      get => literalCsenetOgTranReason ??= new();
      set => literalCsenetOgTranReason = value;
    }

    /// <summary>
    /// A value of CrossValidationCode.
    /// </summary>
    [JsonPropertyName("crossValidationCode")]
    public Code CrossValidationCode
    {
      get => crossValidationCode ??= new();
      set => crossValidationCode = value;
    }

    /// <summary>
    /// A value of CrossValidationCodeValue.
    /// </summary>
    [JsonPropertyName("crossValidationCodeValue")]
    public CodeValue CrossValidationCodeValue
    {
      get => crossValidationCodeValue ??= new();
      set => crossValidationCodeValue = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of LiteralCsenetFunctionalType.
    /// </summary>
    [JsonPropertyName("literalCsenetFunctionalType")]
    public Code LiteralCsenetFunctionalType
    {
      get => literalCsenetFunctionalType ??= new();
      set => literalCsenetFunctionalType = value;
    }

    /// <summary>
    /// A value of LiteralCsenetActionType.
    /// </summary>
    [JsonPropertyName("literalCsenetActionType")]
    public Code LiteralCsenetActionType
    {
      get => literalCsenetActionType ??= new();
      set => literalCsenetActionType = value;
    }

    /// <summary>
    /// A value of LiteralCsenetOgCaseReason.
    /// </summary>
    [JsonPropertyName("literalCsenetOgCaseReason")]
    public Code LiteralCsenetOgCaseReason
    {
      get => literalCsenetOgCaseReason ??= new();
      set => literalCsenetOgCaseReason = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    private Code literalCsenetActionReason;
    private Code actionReason;
    private Code literalCsenetOgTranReason;
    private Code crossValidationCode;
    private CodeValue crossValidationCodeValue;
    private Common validCode;
    private Code literalCsenetFunctionalType;
    private Code literalCsenetActionType;
    private Code literalCsenetOgCaseReason;
    private CodeValue codeValue;
    private Code code;
  }
#endregion
}
