// Program: FN_UPDATE_NON_CASE_ADABAS_PERSON, ID: 372254685, model: 746.
// Short name: SWE02292
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_NON_CASE_ADABAS_PERSON.
/// </para>
/// <para>
/// This CAB calls an external to update a non case person.  It is a copy of the
/// SI cab that updates CSE person.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateNonCaseAdabasPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_NON_CASE_ADABAS_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateNonCaseAdabasPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateNonCaseAdabasPerson.
  /// </summary>
  public FnUpdateNonCaseAdabasPerson(IContext context, Import import,
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
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer       Request #       Description
    // 0105/99  Maureen Brown                   Initial Development
    // ----------------------------------------------------------
    // ---------------------------------------------
    // Call the EAB to update details on the ADABAS files
    // ---------------------------------------------
    // : Dec 8, 1999, M Brown, PR#80435 - Added import flag to indicate to 
    // adabas that the system flag must be updated from 'Y' (case related) to '
    // N' (non case person).
    local.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (IsEmpty(local.CsePersonsWorkSet.Ssn))
    {
      local.CsePersonsWorkSet.Ssn = "000000000";
    }

    local.Current.Date = Now().Date;
    UseEabUpdateNonCasePerson();

    // ---------------------------------------------
    // Interpret the error code returned from ADABAS
    // and set the appropriate exit state.
    // ---------------------------------------------
    switch(AsChar(export.AbendData.Type1))
    {
      case ' ':
        // --------------------------------------------
        // Successful update of data
        // --------------------------------------------
        break;
      case 'A':
        // --------------------------------------------
        // ADABAS Update failed. A reason code should be
        // interpreted.
        // --------------------------------------------
        switch(TrimEnd(export.AbendData.AdabasFileNumber))
        {
          case "0000":
            switch(TrimEnd(export.AbendData.AdabasFileAction))
            {
              case "EAT":
                ExitState = "ADABAS_UNABLE_TO_END_TRANS_W_RB";

                break;
              case "INI":
                ExitState = "ACO_ADABAS_UNAVAILABLE";

                break;
              case "SSN":
                ExitState = "ADABAS_INVALID_SSN_W_RB";

                break;
              default:
                break;
            }

            break;
          case "0113":
            if (Equal(export.AbendData.AdabasFileAction, "KPO"))
            {
              ExitState = "ADABAS_PERSON_KNOWN_TO_KSCARES";
            }
            else
            {
            }

            break;
          case "0149":
            switch(TrimEnd(export.AbendData.AdabasFileAction))
            {
              case "ADS":
                ExitState = "ADABAS_DUPLICATE_SSN_W_RB";

                break;
              case "ARF":
                ExitState = "ACO_ADABAS_PERSON_NF_149";

                break;
              case "ARL":
                ExitState = "ACO_ADABAS_PERSON_NF_149";

                break;
              case "CHF":
                ExitState = "ADABAS_UNABLE_TO_UPDATE";

                break;
              case "CNF":
                ExitState = "ACO_ADABAS_PERSON_NF_149";

                break;
              case "CU":
                ExitState = "ADABAS_UNABLE_TO_UPDATE";

                break;
              default:
                break;
            }

            break;
          case "0152":
            switch(TrimEnd(export.AbendData.AdabasFileAction))
            {
              case "RHI":
                ExitState = "ACO_ADABAS_PERSON_NF_152";

                break;
              case "RI":
                ExitState = "ADABAS_UNABLE_TO_UPDATE";

                break;
              case "UI":
                ExitState = "ADABAS_UNABLE_TO_UPDATE";

                break;
              default:
                break;
            }

            break;
          case "0154":
            switch(TrimEnd(export.AbendData.AdabasFileAction))
            {
              case "RHI":
                ExitState = "ACO_ADABAS_PERSON_NF_154";

                break;
              case "RI":
                ExitState = "ADABAS_UNABLE_TO_UPDATE";

                break;
              case "UI":
                ExitState = "ADABAS_UNABLE_TO_UPDATE";

                break;
              default:
                break;
            }

            break;
          case "0161":
            if (Equal(export.AbendData.AdabasFileAction, "APO"))
            {
              ExitState = "ADABAS_PERSON_KNOWN_TO_AE";
            }
            else
            {
            }

            break;
          default:
            break;
        }

        break;
      case 'C':
        // --------------------------------------------
        // CICS action failed.  A reason code should be
        // interpreted.
        // --------------------------------------------
        ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

        break;
      default:
        break;
    }
  }

  private void UseEabUpdateNonCasePerson()
  {
    var useImport = new EabUpdateNonCasePerson.Import();
    var useExport = new EabUpdateNonCasePerson.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useImport.Converted.Flag = import.Converted.Flag;
    useExport.AbendData.Assign(export.AbendData);

    Call(EabUpdateNonCasePerson.Execute, useImport, useExport);

    export.AbendData.Assign(useExport.AbendData);
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
    /// A value of Converted.
    /// </summary>
    [JsonPropertyName("converted")]
    public Common Converted
    {
      get => converted ??= new();
      set => converted = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common converted;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private AbendData abendData;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
  }
#endregion
}
