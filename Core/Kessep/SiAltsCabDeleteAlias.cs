// Program: SI_ALTS_CAB_DELETE_ALIAS, ID: 371755384, model: 746.
// Short name: SWE01111
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_ALTS_CAB_DELETE_ALIAS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiAltsCabDeleteAlias: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ALTS_CAB_DELETE_ALIAS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAltsCabDeleteAlias(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAltsCabDeleteAlias.
  /// </summary>
  public SiAltsCabDeleteAlias(IContext context, Import import, Export export):
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
    UseEabDeleteAlias();

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
        switch(TrimEnd(local.AbendData.AdabasResponseCd))
        {
          case "0000":
            switch(TrimEnd(local.AbendData.AdabasFileAction))
            {
              case "ADS":
                ExitState = "ADABAS_DUPLICATE_SSN_W_RB";

                break;
              case "SSN":
                ExitState = "ADABAS_INVALID_SSN_W_RB";

                break;
              case "CAE":
                ExitState = "ADABAS_ALIAS_ALREADY_EXISTS";

                break;
              case "CNF":
                ExitState = "ADABAS_ALIAS_NOT_FOUND";

                break;
              default:
                ExitState = "ADABAS_INVALID_RETURN_CODE";

                break;
            }

            break;
          case "":
            break;
          default:
            ExitState = "ACO_ADABAS_UNAVAILABLE";

            break;
        }

        break;
      case 'C':
        // *********************************************
        // CICS action failed.
        // *********************************************
        if (IsEmpty(local.AbendData.CicsResponseCd))
        {
        }
        else
        {
          ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
        }

        break;
      default:
        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
  }

  private void UseEabDeleteAlias()
  {
    var useImport = new EabDeleteAlias.Import();
    var useExport = new EabDeleteAlias.Export();

    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      
    useExport.AbendData.Assign(local.AbendData);

    Call(EabDeleteAlias.Execute, useImport, useExport);

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
