// Program: CAB_UPDATE_ADABAS_PERSON_SSN_BAT, ID: 371414605, model: 746.
// Short name: SWE00019
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_UPDATE_ADABAS_PERSON_SSN_BAT.
/// </summary>
[Serializable]
public partial class CabUpdateAdabasPersonSsnBat: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_UPDATE_ADABAS_PERSON_SSN_BAT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabUpdateAdabasPersonSsnBat(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabUpdateAdabasPersonSsnBat.
  /// </summary>
  public CabUpdateAdabasPersonSsnBat(IContext context, Import import,
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
    // *************************************************************************************
    // Maintenance Log:
    //    Date     Request  Name     Description
    // ---------   -------  ----     
    // -------------------------------------------------------
    // 03/27/2009  CQ114    Raj S    Initial Coding.
    // *************************************************************************************
    // *************************************************************************************
    // This Action Block calls External Action Block to update CSE person SSN in
    // ADABAS.
    // This Action Block will be called by the called object, whenever CSE 
    // receives the
    // Verified SSN from FCR and the person NOT belongs to AE system and current
    // SSN value
    // Is spaces or 0 then this action block will be called to update the SSN 
    // value for the
    // Selected person in ADABAS.
    // *************************************************************************************
    local.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (IsEmpty(local.CsePersonsWorkSet.Ssn))
    {
      local.CsePersonsWorkSet.Ssn = "000000000";
    }

    local.Current.Date = Now().Date;
    UseEabUpdateCsePersonBatch();

    // *************************************************************************************
    // Interpret the error code returned from ADABAS and set the appropriate 
    // exit state.
    // *************************************************************************************
    switch(AsChar(export.AbendData.Type1))
    {
      case ' ':
        // --------------------------------------------
        // Successful update of data
        // --------------------------------------------
        break;
      case 'A':
        // *************************************************************************************
        // ADABAS Update failed. A reason code should be interpreted. 
        // *************************************************************************************
        switch(TrimEnd(export.AbendData.AdabasFileNumber))
        {
          case "0000":
            switch(TrimEnd(export.AbendData.AdabasFileAction))
            {
              case "EAT":
                ExitState = "ADABAS_UNABLE_TO_UPDATE";

                break;
              case "INI":
                ExitState = "ACO_ADABAS_UNAVAILABLE";

                break;
              case "SSN":
                ExitState = "ADABAS_DUPLICATE_SSN";

                break;
              default:
                // *************************************************************************************
                // Introduced an EXIT STATE IS adabas_invalid_return_code, into 
                // all the otherwise state-
                // ments within the CASE OF statement to check the return from 
                // the External Action Block
                // (eab_update_cse_person_batch).
                // *************************************************************************************
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
              // *************************************************************************************
              // Introduced an EXIT STATE IS adabas_invalid_return_code, into 
              // all the otherwise state-
              // ments within the CASE OF statement to check the return from the
              // External Action Block
              // (eab_update_cse_person_batch).
              // *************************************************************************************
              ExitState = "ADABAS_INVALID_RETURN_CODE";
            }

            break;
          case "0149":
            switch(TrimEnd(export.AbendData.AdabasFileAction))
            {
              case "ADS":
                ExitState = "ADABAS_DUPLICATE_SSN";

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
                // *************************************************************************************
                // Introduced an EXIT STATE IS adabas_invalid_return_code, into 
                // all the otherwise state-
                // ments within the CASE OF statement to check the return from 
                // the External Action Block
                // (eab_update_cse_person_batch).
                // *************************************************************************************
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
                // *************************************************************************************
                // Introduced an EXIT STATE IS adabas_invalid_return_code, into 
                // all the otherwise state-
                // ments within the CASE OF statement to check the return from 
                // the External Action Block
                // (eab_update_cse_person_batch).
                // *************************************************************************************
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
                // *************************************************************************************
                // Introduced an EXIT STATE IS adabas_invalid_return_code, into 
                // all the otherwise state-
                // ments within the CASE OF statement to check the return from 
                // the External Action Block
                // (eab_update_cse_person_batch).
                // *************************************************************************************
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
              // *************************************************************************************
              // Introduced an EXIT STATE IS adabas_invalid_return_code, into 
              // all the otherwise state-
              // ments within the CASE OF statement to check the return from the
              // External Action Block
              // (eab_update_cse_person_batch).
              // *************************************************************************************
              ExitState = "ADABAS_INVALID_RETURN_CODE";
            }

            break;
          default:
            // *************************************************************************************
            // Introduced an EXIT STATE IS adabas_invalid_return_code, into all 
            // the otherwise state-
            // ments within the CASE OF statement to check the return from the 
            // External Action Block
            // (eab_update_cse_person_batch).
            // *************************************************************************************
            ExitState = "ADABAS_INVALID_RETURN_CODE";

            break;
        }

        break;
      case 'C':
        // *************************************************************************************
        // CICS action failed.  A reason code should be interpreted.
        // *************************************************************************************
        ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

        break;
      default:
        // *************************************************************************************
        // Introduced an EXIT STATE IS adabas_invalid_return_code, into all the 
        // otherwise state-
        // ments within the CASE OF statement to check the return from the 
        // External Action Block
        // (eab_update_cse_person_batch).
        // *************************************************************************************
        ExitState = "ADABAS_INVALID_RETURN_CODE";

        break;
    }
  }

  private void UseEabUpdateCsePersonBatch()
  {
    var useImport = new EabUpdateCsePersonBatch.Import();
    var useExport = new EabUpdateCsePersonBatch.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useExport.AbendData.Assign(export.AbendData);

    Call(EabUpdateCsePersonBatch.Execute, useImport, useExport);

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
