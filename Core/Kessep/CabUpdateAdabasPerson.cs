// Program: CAB_UPDATE_ADABAS_PERSON, ID: 371755708, model: 746.
// Short name: SWE00098
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_UPDATE_ADABAS_PERSON.
/// </summary>
[Serializable]
public partial class CabUpdateAdabasPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_UPDATE_ADABAS_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabUpdateAdabasPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabUpdateAdabasPerson.
  /// </summary>
  public CabUpdateAdabasPerson(IContext context, Import import, Export export):
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
    // 02/23/95  Helen Sharland                   Initial Development
    // 06/18/97  Sid Chowdhary		Change blank SSN to all zeros.
    // ----------------------------------------------------------
    // 01/23/99 W.Campbell    Inserted an EXIT STATE IS
    //                        adabas_invalid_return_code,
    //                        into all the otherwise statements
    //                        within the CASE OF statement to
    //                        check the return from the EAB,
    //                        eab_update_cse_person.
    // ---------------------------------------------
    // 09/30/2004   Michael Quinn   Added a new Exit
    // State that is set when a record
    // 
    // in the Client-DBF already matches the
    // proposed new input data.       CASE = DUP
    // PR 208957
    // ---------------------------------------------
    // Call the EAB to update details on the ADABAS files
    // ---------------------------------------------
    local.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (IsEmpty(local.CsePersonsWorkSet.Ssn))
    {
      local.CsePersonsWorkSet.Ssn = "000000000";
    }

    local.Current.Date = Now().Date;
    UseEabUpdateCsePerson();

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
                // ---------------------------------------------
                // 01/23/99 W.Campbell - Inserted an EXIT STATE IS
                // adabas_invalid_return_code, into all the otherwise
                // statements within the CASE OF statement to
                // check the return from the EAB,
                // eab_update_cse_person.
                // ---------------------------------------------
                ExitState = "ADABAS_INVALID_RETURN_CODE";

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
              // ---------------------------------------------
              // 01/23/99 W.Campbell - Inserted an EXIT STATE IS
              // adabas_invalid_return_code, into all the otherwise
              // statements within the CASE OF statement to
              // check the return from the EAB,
              // eab_update_cse_person.
              // ---------------------------------------------
              ExitState = "ADABAS_INVALID_RETURN_CODE";
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
              case "DUP":
                ExitState = "ADABAS_DUPLICATE_ALIAS_EXISTS";

                break;
              default:
                // ---------------------------------------------
                // 01/23/99 W.Campbell - Inserted an EXIT STATE IS
                // adabas_invalid_return_code, into all the otherwise
                // statements within the CASE OF statement to
                // check the return from the EAB,
                // eab_update_cse_person.
                // ---------------------------------------------
                ExitState = "ADABAS_INVALID_RETURN_CODE";

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
                // ---------------------------------------------
                // 01/23/99 W.Campbell - Inserted an EXIT STATE IS
                // adabas_invalid_return_code, into all the otherwise
                // statements within the CASE OF statement to
                // check the return from the EAB,
                // eab_update_cse_person.
                // ---------------------------------------------
                ExitState = "ADABAS_INVALID_RETURN_CODE";

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
                // ---------------------------------------------
                // 01/23/99 W.Campbell - Inserted an EXIT STATE IS
                // adabas_invalid_return_code, into all the otherwise
                // statements within the CASE OF statement to
                // check the return from the EAB,
                // eab_update_cse_person.
                // ---------------------------------------------
                ExitState = "ADABAS_INVALID_RETURN_CODE";

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
              // ---------------------------------------------
              // 01/23/99 W.Campbell - Inserted an EXIT STATE IS
              // adabas_invalid_return_code, into all the otherwise
              // statements within the CASE OF statement to
              // check the return from the EAB,
              // eab_update_cse_person.
              // ---------------------------------------------
              ExitState = "ADABAS_INVALID_RETURN_CODE";
            }

            break;
          default:
            // ---------------------------------------------
            // 01/23/99 W.Campbell - Inserted an EXIT STATE IS
            // adabas_invalid_return_code, into all the otherwise
            // statements within the CASE OF statement to
            // check the return from the EAB,
            // eab_update_cse_person.
            // ---------------------------------------------
            ExitState = "ADABAS_INVALID_RETURN_CODE";

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
        // ---------------------------------------------
        // 01/23/99 W.Campbell - Inserted an EXIT STATE IS
        // adabas_invalid_return_code, into all the otherwise
        // statements within the CASE OF statement to
        // check the return from the EAB,
        // eab_update_cse_person.
        // ---------------------------------------------
        ExitState = "ADABAS_INVALID_RETURN_CODE";

        break;
    }
  }

  private void UseEabUpdateCsePerson()
  {
    var useImport = new EabUpdateCsePerson.Import();
    var useExport = new EabUpdateCsePerson.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useExport.AbendData.Assign(export.AbendData);

    Call(EabUpdateCsePerson.Execute, useImport, useExport);

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

    private CsePersonsWorkSet csePersonsWorkSet;
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
