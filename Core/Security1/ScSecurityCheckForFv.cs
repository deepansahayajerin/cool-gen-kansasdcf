// Program: SC_SECURITY_CHECK_FOR_FV, ID: 374370269, model: 746.
// Short name: SWE00141
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Kessep;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_SECURITY_CHECK_FOR_FV.
/// </para>
/// <para>
/// This CAB performs the security validation checks as required by PRWORA for 
/// the KESSEP child support enforcement system.
/// </para>
/// </summary>
[Serializable]
public partial class ScSecurityCheckForFv: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_SECURITY_CHECK_FOR_FV program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScSecurityCheckForFv(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScSecurityCheckForFv.
  /// </summary>
  public ScSecurityCheckForFv(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // MAINTENANCE LOG:
    // 03/07/00	C. Scroggins		Initial Development
    // -------------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------
    // Move Imports to Exports
    // -------------------------------------------------------------------
    export.CsePerson.Number = import.CsePerson.Number;
    export.Case1.Number = import.Case1.Number;
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    // -------------------------------------------------------------------
    // Read transaction entity to get screen id
    // -------------------------------------------------------------------
    if (!ReadTransaction())
    {
      ExitState = "SC0000_TRANSACTION_TRANCODE_NF";

      return;
    }

    local.FamilyViolenceScreen.Flag = "";
    local.CodeValue.Cdvalue = entities.Transaction.ScreenId;
    local.Code.CodeName = "FAMILY VIOLENCE SCREENS";

    // -------------------------------------------------------------------
    // NOTE: Check current screen versus screens in code value tables here.
    // -------------------------------------------------------------------
    UseCabValidateCodeValue();

    // -------------------------------------------------------------------
    // NOTE: Check result of Code Value Search to see if screen is sensitive for
    // family violence.
    // -------------------------------------------------------------------
    if (AsChar(local.FamilyViolenceScreen.Flag) == 'Y')
    {
      // -------------------------------------------------------------------
      // NOTE: If screen is sensitive to family violence, call cab to authorize 
      // user.
      // -------------------------------------------------------------------
      UseScSecurityValidAuthForFv();
    }
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.FamilyViolenceScreen.Flag = useExport.ValidCode.Flag;
  }

  private void UseScSecurityValidAuthForFv()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private bool ReadTransaction()
  {
    entities.Transaction.Populated = false;

    return Read("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "trancode", global.TranCode);
      },
      (db, reader) =>
      {
        entities.Transaction.ScreenId = db.GetString(reader, 0);
        entities.Transaction.Trancode = db.GetString(reader, 1);
        entities.Transaction.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Case1 case1;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private Case1 case1;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Authorized.
    /// </summary>
    [JsonPropertyName("authorized")]
    public Common Authorized
    {
      get => authorized ??= new();
      set => authorized = value;
    }

    /// <summary>
    /// A value of FamilyViolenceScreen.
    /// </summary>
    [JsonPropertyName("familyViolenceScreen")]
    public Common FamilyViolenceScreen
    {
      get => familyViolenceScreen ??= new();
      set => familyViolenceScreen = value;
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
    /// A value of FvIndicator.
    /// </summary>
    [JsonPropertyName("fvIndicator")]
    public Common FvIndicator
    {
      get => fvIndicator ??= new();
      set => fvIndicator = value;
    }

    private Common authorized;
    private Common familyViolenceScreen;
    private Code code;
    private CodeValue codeValue;
    private Common fvIndicator;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
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

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Transaction transaction;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
  }
#endregion
}
