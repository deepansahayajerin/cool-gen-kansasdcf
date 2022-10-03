// Program: SP_DOC_FORMAT_SSN, ID: 372134419, model: 746.
// Short name: SWE01684
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DOC_FORMAT_SSN.
/// </para>
/// <para>
/// This CAB formats an SSN to 999-99-9999.  Import View:  CSE_PERSONS_WORK_SET 
/// SSN.
/// </para>
/// </summary>
[Serializable]
public partial class SpDocFormatSsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_FORMAT_SSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocFormatSsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocFormatSsn.
  /// </summary>
  public SpDocFormatSsn(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------
    //               M A I N T E N A N C E   L O G
    // -------------------------------------------------------------------------
    // 				Initial Development
    // 05/06/1999	M Ramirez	Added check for zeroes
    // 03/04/2008	M Ramirez	Added masking SSN option
    // 04/06/2009	J Huss		Added placeholder for unknown SSN
    // -------------------------------------------------------------------------
    export.Ssn.Value = Spaces(FieldValue.Value_MaxLength);

    // J Huss	Organizations don't have SSN's.
    if (AsChar(import.CsePerson.Type1) == 'O')
    {
      return;
    }

    if (IsEmpty(import.CsePersonsWorkSet.Ssn) || Equal
      (import.CsePersonsWorkSet.Ssn, "000000000"))
    {
      // J Huss	Person exists but we don't have their SSN.  Populate a 
      // placeholder if requested.
      if (AsChar(import.PopulatePlaceholder.Flag) == 'Y')
      {
        export.Ssn.Value = "UNKNOWN";
      }
    }
    else
    {
      // J Huss	Person exists and we have their SSN.  Mask it if necessary.
      if (AsChar(import.MaskSsn.Flag) == 'Y')
      {
        export.Ssn.Value = "XXX-XX-" + Substring
          (import.CsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
          
      }
      else
      {
        export.Ssn.Value =
          Substring(import.CsePersonsWorkSet.Ssn,
          CsePersonsWorkSet.Ssn_MaxLength, 1, 3) + "-";
        export.Ssn.Value = (export.Ssn.Value ?? "") + Substring
          (import.CsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 4, 2);
          
        export.Ssn.Value = (export.Ssn.Value ?? "") + "-";
        export.Ssn.Value = (export.Ssn.Value ?? "") + Substring
          (import.CsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
          
      }
    }
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MaskSsn.
    /// </summary>
    [JsonPropertyName("maskSsn")]
    public Common MaskSsn
    {
      get => maskSsn ??= new();
      set => maskSsn = value;
    }

    /// <summary>
    /// A value of PopulatePlaceholder.
    /// </summary>
    [JsonPropertyName("populatePlaceholder")]
    public Common PopulatePlaceholder
    {
      get => populatePlaceholder ??= new();
      set => populatePlaceholder = value;
    }

    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common maskSsn;
    private Common populatePlaceholder;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ssn.
    /// </summary>
    [JsonPropertyName("ssn")]
    public FieldValue Ssn
    {
      get => ssn ??= new();
      set => ssn = value;
    }

    private FieldValue ssn;
  }
#endregion
}
