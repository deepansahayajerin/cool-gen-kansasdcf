// Program: OE_FPL2_PROCESS_FPLS, ID: 372354524, model: 746.
// Short name: SWEFPL2P
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
/// A program: OE_FPL2_PROCESS_FPLS.
/// </para>
/// <para>
/// Resp: OBLGEST
/// This Procedure Step is part of Procedure OE_PROCESS_FPLS TRANSACTIONS and 
/// functions to display a second page of data received on the FPLS_Response.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeFpl2ProcessFpls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FPL2_PROCESS_FPLS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFpl2ProcessFpls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFpl2ProcessFpls.
  /// </summary>
  public OeFpl2ProcessFpls(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------
    //  Date		Developer	Description
    // 01/22/96	T.O.Redmond	Initial Code
    // --------------------------------------------------------
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

    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.FplsLocateResponse.Assign(import.FplsLocateResponse);
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
      export.Hidden.CsePersonNumber = export.CsePersonsWorkSet.Number;
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

    if (!IsEmpty(import.FplsLocateResponse.VaSuspenseCode))
    {
      local.Code.CodeName = "VA SUSPENSE BENEFIT CODE";
      local.CodeValue.Cdvalue = import.FplsLocateResponse.VaSuspenseCode ?? Spaces
        (10);
      local.ValidCode.Flag = "";
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'N')
      {
        export.VaSuspense.Description = "Invalid Suspense Code";
      }
      else
      {
        export.VaSuspense.Description = local.CodeValue.Description;
      }
    }

    if (!IsEmpty(import.FplsLocateResponse.VaBenefitCode))
    {
      local.Code.CodeName = "VA BENEFIT CODE";
      local.CodeValue.Cdvalue = import.FplsLocateResponse.VaBenefitCode ?? Spaces
        (10);
      local.ValidCode.Flag = "";
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'N')
      {
        export.VaBenefit.Description = "Invalid Benefit Code";
      }
      else
      {
        export.VaBenefit.Description = local.CodeValue.Description;
      }
    }

    if (!IsEmpty(import.FplsLocateResponse.VaIncarcerationCode))
    {
      local.Code.CodeName = "VA INCARCERATION CODE";
      local.CodeValue.Cdvalue =
        import.FplsLocateResponse.VaIncarcerationCode ?? Spaces(10);
      local.ValidCode.Flag = "";
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'N')
      {
        export.VaIncarceration.Description = "Invalid Incarceration Code";
      }
      else
      {
        export.VaIncarceration.Description = local.CodeValue.Description;
      }
    }

    if (!IsEmpty(import.FplsLocateResponse.VaRetirementPayCode))
    {
      local.Code.CodeName = "VA RETIREMENT PAY CODE";
      local.CodeValue.Cdvalue =
        import.FplsLocateResponse.VaRetirementPayCode ?? Spaces(10);
      local.ValidCode.Flag = "";
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'N')
      {
        export.VaRetirement.Description = "Invalid Retirement Code";
      }
      else
      {
        export.VaRetirement.Description = local.CodeValue.Description;
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }
    else
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
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

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    /// <summary>
    /// A value of VaBenefit.
    /// </summary>
    [JsonPropertyName("vaBenefit")]
    public CodeValue VaBenefit
    {
      get => vaBenefit ??= new();
      set => vaBenefit = value;
    }

    /// <summary>
    /// A value of VaSuspense.
    /// </summary>
    [JsonPropertyName("vaSuspense")]
    public CodeValue VaSuspense
    {
      get => vaSuspense ??= new();
      set => vaSuspense = value;
    }

    /// <summary>
    /// A value of VaIncarceration.
    /// </summary>
    [JsonPropertyName("vaIncarceration")]
    public CodeValue VaIncarceration
    {
      get => vaIncarceration ??= new();
      set => vaIncarceration = value;
    }

    /// <summary>
    /// A value of VaRetirement.
    /// </summary>
    [JsonPropertyName("vaRetirement")]
    public CodeValue VaRetirement
    {
      get => vaRetirement ??= new();
      set => vaRetirement = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CodeValue vaBenefit;
    private CodeValue vaSuspense;
    private CodeValue vaIncarceration;
    private CodeValue vaRetirement;
    private FplsLocateResponse fplsLocateResponse;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of VaBenefit.
    /// </summary>
    [JsonPropertyName("vaBenefit")]
    public CodeValue VaBenefit
    {
      get => vaBenefit ??= new();
      set => vaBenefit = value;
    }

    /// <summary>
    /// A value of VaSuspense.
    /// </summary>
    [JsonPropertyName("vaSuspense")]
    public CodeValue VaSuspense
    {
      get => vaSuspense ??= new();
      set => vaSuspense = value;
    }

    /// <summary>
    /// A value of VaIncarceration.
    /// </summary>
    [JsonPropertyName("vaIncarceration")]
    public CodeValue VaIncarceration
    {
      get => vaIncarceration ??= new();
      set => vaIncarceration = value;
    }

    /// <summary>
    /// A value of VaRetirement.
    /// </summary>
    [JsonPropertyName("vaRetirement")]
    public CodeValue VaRetirement
    {
      get => vaRetirement ??= new();
      set => vaRetirement = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CodeValue vaBenefit;
    private CodeValue vaSuspense;
    private CodeValue vaIncarceration;
    private CodeValue vaRetirement;
    private FplsLocateResponse fplsLocateResponse;
    private CsePerson csePerson;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    private Code code;
    private CodeValue codeValue;
    private Common validCode;
  }
#endregion
}
