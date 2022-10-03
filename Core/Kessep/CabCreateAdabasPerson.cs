// Program: CAB_CREATE_ADABAS_PERSON, ID: 371727799, model: 746.
// Short name: SWE00031
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_CREATE_ADABAS_PERSON.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block creates a new CSE Person on ADABAS and interprets the 
/// abend information returned.
/// </para>
/// </summary>
[Serializable]
public partial class CabCreateAdabasPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_CREATE_ADABAS_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabCreateAdabasPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabCreateAdabasPerson.
  /// </summary>
  public CabCreateAdabasPerson(IContext context, Import import, Export export):
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
    // 		M A I N T E N A N C E   L O G
    // Date		Developer		Request #	Description
    // 07-19-95	Helen Sharland - MTW		0	Initial Dev
    // -------------------------------------------------------------------
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    UseEabCreateCsePerson();
    export.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    switch(AsChar(export.AbendData.Type1))
    {
      case ' ':
        break;
      case 'A':
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
                ExitState = "ADABAS_INVALID_RETURN_CODE";

                break;
            }

            break;
          case "0113":
            ExitState = "ACO_ADABAS_PERSON_NF_113";

            break;
          case "0114":
            ExitState = "ACO_ADABAS_PERSON_NF_114";

            break;
          case "0149":
            if (Equal(export.AbendData.AdabasFileAction, "ADS"))
            {
              ExitState = "ADABAS_DUPLICATE_SSN_W_RB";
            }
            else
            {
              ExitState = "ADABAS_ADD_UNSUCCESSFUL_W_RB";
            }

            break;
          case "0154":
            ExitState = "ADABAS_ADD_UNSUCCESSFUL_W_RB";

            break;
          case "0161":
            ExitState = "ACO_ADABAS_PERSON_NF_161";

            break;
          default:
            ExitState = "ADABAS_INVALID_RETURN_CODE";

            break;
        }

        break;
      case 'C':
        ExitState = "ACO_RE0000_CICS_UNAVAILABLE_RB";

        break;
      default:
        ExitState = "ADABAS_INVALID_RETURN_CODE";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseEabCreateCsePerson()
  {
    var useImport = new EabCreateCsePerson.Import();
    var useExport = new EabCreateCsePerson.Export();

    useImport.New1.Assign(import.CsePersonsWorkSet);
    useExport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useExport.AbendData.Assign(export.AbendData);

    Call(EabCreateCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Number = useExport.CsePersonsWorkSet.Number;
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

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
#endregion
}
