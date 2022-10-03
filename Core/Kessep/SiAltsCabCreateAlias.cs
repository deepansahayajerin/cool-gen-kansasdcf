// Program: SI_ALTS_CAB_CREATE_ALIAS, ID: 371755385, model: 746.
// Short name: SWE01110
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_ALTS_CAB_CREATE_ALIAS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiAltsCabCreateAlias: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ALTS_CAB_CREATE_ALIAS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAltsCabCreateAlias(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAltsCabCreateAlias.
  /// </summary>
  public SiAltsCabCreateAlias(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //       M A I N T E N A N C E   L O G
    //   Date    Developer    Description
    // 11-14-95  K Evans      Initial development
    // ---------------------------------------------
    // 	
    // 10/29/99 W.Campbell    Reworked the
    //                        case of local abend_data_type
    //                        statement in order to include
    //                        exit states errors for
    //                        1. invalid ssn and
    //                        2. Duplicate SSN - ssn already
    //                        exist for another person.
    //                        Work done on PR# 00078205.
    // ---------------------------------------------
    UseEabAddAlias();

    // ---------------------------------------------
    // 10/29/99 W.Campbell - Reworked the
    // case of local abend_data_type
    // statement in order to include
    // exit states errors for
    // 1. Invalid ssn and
    // 2. Duplicate SSN - ssn already
    //    exist for another person.
    // Work done on PR# 00078205.
    // ---------------------------------------------
    switch(AsChar(local.AbendData.Type1))
    {
      case ' ':
        // *********************************************
        // Successful read
        // *********************************************
        break;
      case 'A':
        // *********************************************
        // ADABAS read failed.
        // *********************************************
        if (Equal(local.AbendData.AdabasResponseCd, "0000"))
        {
          switch(TrimEnd(local.AbendData.AdabasFileNumber))
          {
            case "0149":
              if (Equal(local.AbendData.AdabasFileAction, "ADS"))
              {
                ExitState = "ADABAS_DUPLICATE_SSN_W_RB";
              }
              else
              {
                ExitState = "ADABAS_INVALID_RETURN_CODE";
              }

              break;
            case "0000":
              if (Equal(local.AbendData.AdabasFileAction, "SSN"))
              {
                ExitState = "ADABAS_INVALID_SSN_W_RB";
              }
              else
              {
                ExitState = "ADABAS_INVALID_RETURN_CODE";
              }

              break;
            default:
              ExitState = "ADABAS_INVALID_RETURN_CODE";

              break;
          }
        }
        else
        {
          ExitState = "ADABAS_INVALID_RETURN_CODE";
        }

        break;
      case 'C':
        // *********************************************
        // CICS action failed.
        // *********************************************
        ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

        break;
      default:
        ExitState = "ADABAS_INVALID_RETURN_CODE";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
  }

  private void UseEabAddAlias()
  {
    var useImport = new EabAddAlias.Import();
    var useExport = new EabAddAlias.Export();

    useImport.New1.Assign(import.CsePersonsWorkSet);
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useExport.Key);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabAddAlias.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.Key, export.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
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

    private AbendData abendData;
  }
#endregion
}
