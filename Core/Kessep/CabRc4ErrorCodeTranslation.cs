// Program: CAB_RC4_ERROR_CODE_TRANSLATION, ID: 372814831, model: 746.
// Short name: SWE00231
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_RC4_ERROR_CODE_TRANSLATION.
/// </summary>
[Serializable]
public partial class CabRc4ErrorCodeTranslation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_RC4_ERROR_CODE_TRANSLATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabRc4ErrorCodeTranslation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabRc4ErrorCodeTranslation.
  /// </summary>
  public CabRc4ErrorCodeTranslation(IContext context, Import import,
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
    switch(TrimEnd(import.Report1Composer.Parm1))
    {
      case "":
        // -> No Errors were encountered.
        ExitState = "ACO_NN0000_ALL_OK";

        break;
      case "EO":
        // -> Error on OPEN FILE.
        ExitState = "ACO_RC_AB0001_ERROR_ON_OPEN_FILE";
        export.RcErrorMessage.RptDetail = "ERROR ON OPEN FILE IN RC EAB";

        break;
      case "EC":
        // -> Error on CLOSE FILE.
        ExitState = "ACO_RC_AB0002_ERROR_CLOSING_FILE";
        export.RcErrorMessage.RptDetail = "ERROR ON CLOSE FILE IN RC EAB";

        break;
      case "ER":
        // -> Error on GR ACTION.
        ExitState = "ACO_RC_AB0003_ERROR_ON_GR_ACTION";
        export.RcErrorMessage.RptDetail = "ERROR ON GR ACTION IN RC EAB";

        break;
      case "II":
        // ->Invalid PARM1 code.
        ExitState = "ACO_RC_AB0004_INVALID_PARM1_CODE";
        export.RcErrorMessage.RptDetail = "INVALID PARM1 CODE IN RC EAB";

        break;
      case "RI":
        // -> Invalid Runtime Option (PARM2 code).
        ExitState = "ACO_RC_AB0005_INVALID_PARM2_CODE";
        export.RcErrorMessage.RptDetail = "INVALID PARM2 CODE IN RC EAB";

        break;
      case "EF":
        // ->END OF FILE (File reader only).
        ExitState = "ACO_RC_NN0001_END_OF_FILE";

        break;
      case "EN":
        // -> Error on numeric data field read by file reader.
        ExitState = "ACO_RC_AB0006_ERROR_NUMERIC_INPT";
        export.RcErrorMessage.RptDetail = "ERROR ON NUMERIC INPUT IN RC EAB";

        break;
      case "IS":
        // -> Invalid SUBREPORT code .
        ExitState = "ACO_RC_AB0007_INV_SUBREPORT_PARM";
        export.RcErrorMessage.RptDetail =
          "INVALID SUBREPORT CODE INPUT TO RC EAB";

        break;
      default:
        // -> Not a valid Report Composer return code.
        ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";
        export.RcErrorMessage.RptDetail = "INVALID RC EAB RETURN CODE";

        break;
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
    /// A value of Report1Composer.
    /// </summary>
    [JsonPropertyName("report1Composer")]
    public ReportParms Report1Composer
    {
      get => report1Composer ??= new();
      set => report1Composer = value;
    }

    private ReportParms report1Composer;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RcErrorMessage.
    /// </summary>
    [JsonPropertyName("rcErrorMessage")]
    public EabReportSend RcErrorMessage
    {
      get => rcErrorMessage ??= new();
      set => rcErrorMessage = value;
    }

    private EabReportSend rcErrorMessage;
  }
#endregion
}
