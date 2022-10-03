// Program: SI_PPFX_FIX_CSE_PERSON, ID: 372885254, model: 746.
// Short name: SWE00254
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_PPFX_FIX_CSE_PERSON.
/// </summary>
[Serializable]
public partial class SiPpfxFixCsePerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PPFX_FIX_CSE_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPpfxFixCsePerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPpfxFixCsePerson.
  /// </summary>
  public SiPpfxFixCsePerson(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.CsePersonsWorkSet.Number = import.CsePerson.Number;
    export.ErrorCode.Text3 = "";
    export.Message.Text80 = "";
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // Check to see if the CSE person is known to the CSE system.
    // Non-case related CSE person (CSE flag = 'N')
    // Case related CSE person (CSE flag = 'Y')
    // Unknown CSE Person (CSE flag = spaces)
    // IF NOT IN TRIAL MODE
    // If known to CSE no action is necessary.
    // If not known (CSE flag = spaces) change in ADABAS
    // to a case related CSE person.
    // If Non-case related this is an error.
    // ---------------------------------------------
    local.ErrOnAdabasUnavail.Flag = "Y";
    UseCabReadAdabasPerson();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
      {
        export.Message.Text80 = "ADABAS UNAVAILABLE";

        return;
      }

      if (IsExitState("ACO_NE0000_CICS_UNAVAILABLE"))
      {
        export.Message.Text80 = "CICS UNAVAILABLE";

        return;
      }

      if (IsExitState("ADABAS_INVALID_RETURN_CODE"))
      {
        export.Message.Text80 = "ADABAS INVALID RETURN CODE";

        return;
      }

      export.ErrorCode.Text3 = "ANF";
      export.Message.Text80 = "PERSON IS UNKNOWN TO ADABAS";
      ExitState = "ACO_NN0000_ALL_OK";

      return;
    }

    switch(AsChar(local.Cse.Flag))
    {
      case 'Y':
        // ---------------------------------------------
        // CSE person is known to the CSE system.
        // ---------------------------------------------
        export.ErrorCode.Text3 = "";
        export.Message.Text80 = "PERSON IS KNOWN TO ADABAS";

        break;
      case 'N':
        // ---------------------------------------------
        // CSE person is NON-case related.
        // This is an error.
        // ---------------------------------------------
        export.ErrorCode.Text3 = "N";
        export.Message.Text80 = "NON CASE RELATED PERSON";

        if (AsChar(import.FixMode.Flag) == 'Y' && AsChar
          (import.FixOverride.Flag) == 'N')
        {
          if (AsChar(import.TrialMode.Flag) == 'Y')
          {
            return;
          }

          // -------------------------------------------------------
          // 	Retreive the person program history from ADABAS
          // -------------------------------------------------------
          UseSiRegiCopyAdabasPersonPgms();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("CHILD_PERSON_PROG_NF"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            export.ErrorCode.Text3 = "NEP";
            export.Message.Text80 =
              "NON CASE RELATED PERSON ERROR CREATING PROGRAM";
            ExitState = "ACO_NN0000_ALL_OK";

            return;
          }

          // ---------------------------------------------
          // Update the ADABAS record with the unique key
          // set to spaces to indicate that the person is now
          // a CASE related person.
          // ---------------------------------------------
          local.CsePersonsWorkSet.UniqueKey = "";
          UseSiAltsCabUpdateAlias();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -->ROLLBACK
            export.ErrorCode.Text3 = "NEA";
            export.Message.Text80 =
              "NON CASE RELATED PERSON ERROR CREATING ADABAS";
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            export.ErrorCode.Text3 = "NOK";
            export.Message.Text80 = "NON CASE RELATED PERSON ADDED OK";
          }
        }
        else
        {
        }

        break;
      case ' ':
        // ---------------------------------------------
        // CSE person is unknown to the CSE system.
        // ---------------------------------------------
        export.ErrorCode.Text3 = "U";
        export.Message.Text80 = "UNKNOWN PERSON";

        if (AsChar(import.TrialMode.Flag) == 'Y')
        {
          return;
        }

        // -------------------------------------------------------
        // 	Retreive the person program history from ADABASE
        // -------------------------------------------------------
        UseSiRegiCopyAdabasPersonPgms();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("CHILD_PERSON_PROG_NF"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          export.ErrorCode.Text3 = "UEP";
          export.Message.Text80 = "UNKNOWN PERSON ERROR CREATING PROGRAM";
          ExitState = "ACO_NN0000_ALL_OK";

          return;
        }

        // ---------------------------------------------
        // Update the ADABAS record with the unique key
        // set to spaces to indicate that the person is now
        // a CASE related person.
        // ---------------------------------------------
        local.CsePersonsWorkSet.UniqueKey = "";
        UseSiAltsCabUpdateAlias();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -->ROLLBACK
          export.ErrorCode.Text3 = "UEA";
          export.Message.Text80 = "UNKNOWN PERSON ERROR CREATING ADABAS";
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          export.ErrorCode.Text3 = "UOK";
          export.Message.Text80 = "UNKNOWN PERSON ADDED OK";
        }

        break;
      default:
        // ---------------------------------------------
        // Error in returned code.
        // ---------------------------------------------
        export.ErrorCode.Text3 = "ERR";
        export.Message.Text80 = "UNKNOWN PERSON TYPE";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavail.Flag;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.Cse.Flag = useExport.Cse.Flag;
  }

  private void UseSiAltsCabUpdateAlias()
  {
    var useImport = new SiAltsCabUpdateAlias.Import();
    var useExport = new SiAltsCabUpdateAlias.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiAltsCabUpdateAlias.Execute, useImport, useExport);
  }

  private void UseSiRegiCopyAdabasPersonPgms()
  {
    var useImport = new SiRegiCopyAdabasPersonPgms.Import();
    var useExport = new SiRegiCopyAdabasPersonPgms.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CseReferralReceived.Date = local.Current.Date;

    Call(SiRegiCopyAdabasPersonPgms.Execute, useImport, useExport);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of FixMode.
    /// </summary>
    [JsonPropertyName("fixMode")]
    public Common FixMode
    {
      get => fixMode ??= new();
      set => fixMode = value;
    }

    /// <summary>
    /// A value of TrialMode.
    /// </summary>
    [JsonPropertyName("trialMode")]
    public Common TrialMode
    {
      get => trialMode ??= new();
      set => trialMode = value;
    }

    /// <summary>
    /// A value of FixOverride.
    /// </summary>
    [JsonPropertyName("fixOverride")]
    public Common FixOverride
    {
      get => fixOverride ??= new();
      set => fixOverride = value;
    }

    private CsePerson csePerson;
    private Common fixMode;
    private Common trialMode;
    private Common fixOverride;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ErrorCode.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public WorkArea ErrorCode
    {
      get => errorCode ??= new();
      set => errorCode = value;
    }

    /// <summary>
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public SpTextWorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    private WorkArea errorCode;
    private SpTextWorkArea message;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
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

    /// <summary>
    /// A value of ErrOnAdabasUnavail.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavail")]
    public Common ErrOnAdabasUnavail
    {
      get => errOnAdabasUnavail ??= new();
      set => errOnAdabasUnavail = value;
    }

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common cse;
    private DateWorkArea current;
    private Common errOnAdabasUnavail;
  }
#endregion
}
