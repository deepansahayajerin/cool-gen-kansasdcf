// Program: CAB_VALIDATE_SSN, ID: 372662875, model: 746.
// Short name: SWE02032
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_VALIDATE_SSN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block validates SSN.
/// </para>
/// </summary>
[Serializable]
public partial class CabValidateSsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_VALIDATE_SSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabValidateSsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabValidateSsn.
  /// </summary>
  public CabValidateSsn(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------
    // CHANGE LOG:
    // *** PR#93622:  UNKNOWN
    // SSN first 3-digit must be between 001
    // and 765, inclusive.
    // Changed to include Florida 766 to 772 inclusive as per SSA e-flash.
    // 08/02/2000	PMcElderry
    // PR # 100333-A.  SSN upper limit now 785.
    // NOTE - this will be a temporary fix as the upper paramter will
    // be increasing next quarter.
    // 05/03/11  RMathews  CQ22253
    // Standardize SSN edits for Social Security Administration randomization
    // project.  SSN upper limit now 899.
    // ----------------------------------------------------------------
    // *** Added export cse_persons_work_set.
    // For possible reuse in subsequent ABs, to reducing ADABAS I/O calls.
    export.ValidSsn.Flag = "N";

    if (!IsEmpty(import.CsePersonsWorkSet.Number))
    {
      local.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    }
    else
    {
      local.CsePersonsWorkSet.Number = import.CsePerson.Number;
    }

    export.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    UseSiReadCsePersonBatch();

    if (IsExitState("ADABAS_UNAVAILABLE_RB") || IsExitState
      ("ADABAS_READ_UNSUCCESSFUL") || IsExitState
      ("ADABAS_INVALID_RETURN_CODE"))
    {
      return;
    }

    if (IsEmpty(export.CsePersonsWorkSet.Ssn))
    {
      ExitState = "LE0000_SSN_IS_BLANK";

      return;
    }

    if (Length(TrimEnd(export.CsePersonsWorkSet.Ssn)) < 9)
    {
      ExitState = "LE0000_SSN_HAS_LT_9_CHARS";

      return;
    }

    if (Verify(export.CsePersonsWorkSet.Ssn, "0123456789") != 0)
    {
      ExitState = "LE0000_SSN_CONTAINS_NONNUM";

      return;
    }

    local.Ssn1St3.Text3 = export.CsePersonsWorkSet.Ssn;
    local.SsnTextPart2.Text2 = Substring(export.CsePersonsWorkSet.Ssn, 4, 2);
    local.SsnTextPart3.Text4 = Substring(export.CsePersonsWorkSet.Ssn, 6, 4);

    // CQ22253 - Modified SSN edits.  Upper limit for first 3 digits is now '
    // 899' excluding '666'.
    //  No '00' in positions 4-5, and no '0000' in positions 6-9.
    if (!Lt(local.Ssn1St3.Text3, "001") && !Lt("665", local.Ssn1St3.Text3) || !
      Lt(local.Ssn1St3.Text3, "667") && !Lt("899", local.Ssn1St3.Text3))
    {
      if (Lt(local.SsnTextPart2.Text2, "01") || Lt
        ("99", local.SsnTextPart2.Text2))
      {
        ExitState = "ACO_NE0000_INVALID_SSN2";

        return;
      }
      else if (Lt(local.SsnTextPart3.Text4, "0001") || Lt
        ("9999", local.SsnTextPart3.Text4))
      {
        ExitState = "ACO_NE0000_INVALID_SSN4";

        return;
      }

      export.ValidSsn.Flag = "Y";
    }
    else
    {
      ExitState = "LE0000_SSN_1ST_3_INVALID_CHAR";
    }
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ValidSsn.
    /// </summary>
    [JsonPropertyName("validSsn")]
    public Common ValidSsn
    {
      get => validSsn ??= new();
      set => validSsn = value;
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

    private Common validSsn;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SsnTextPart3.
    /// </summary>
    [JsonPropertyName("ssnTextPart3")]
    public WorkArea SsnTextPart3
    {
      get => ssnTextPart3 ??= new();
      set => ssnTextPart3 = value;
    }

    /// <summary>
    /// A value of SsnTextPart2.
    /// </summary>
    [JsonPropertyName("ssnTextPart2")]
    public WorkArea SsnTextPart2
    {
      get => ssnTextPart2 ??= new();
      set => ssnTextPart2 = value;
    }

    /// <summary>
    /// A value of Ssn1St3.
    /// </summary>
    [JsonPropertyName("ssn1St3")]
    public WorkArea Ssn1St3
    {
      get => ssn1St3 ??= new();
      set => ssn1St3 = value;
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

    private WorkArea ssnTextPart3;
    private WorkArea ssnTextPart2;
    private WorkArea ssn1St3;
    private CsePersonsWorkSet csePersonsWorkSet;
  }
#endregion
}
