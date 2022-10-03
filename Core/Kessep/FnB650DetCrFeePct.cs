// Program: FN_B650_DET_CR_FEE_PCT, ID: 371189167, model: 746.
// Short name: SWE02544
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B650_DET_CR_FEE_PCT.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnB650DetCrFeePct: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B650_DET_CR_FEE_PCT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB650DetCrFeePct(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB650DetCrFeePct.
  /// </summary>
  public FnB650DetCrFeePct(IContext context, Import import, Export export):
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
    // Date	  By	  IDCR#	 Description
    // 06/30/03  Fangman  302055   Initial code to assess cost recovery fee to 
    // potentioal recovery obligation.
    // 11/24/15  GVandy   CQ50349  Default cost recovery fee rate to 0% and 
    // override with REFI values.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.TribunalFeeInformation.Cap = 0;

    // 11/24/15 GVandy CQ50349 Default cost recovery fee rate to 0% and override
    // with REFI values.
    if (Lt(new DateTime(2017, 6, 30), import.Per.CollectionDt))
    {
      export.TribunalFeeInformation.Rate = 0;
    }
    else if (Lt(new DateTime(1999, 12, 31), import.Per.CollectionDt))
    {
      export.TribunalFeeInformation.Rate = 4;
    }
    else
    {
      export.TribunalFeeInformation.Rate = 2;
    }

    if (ReadObligation())
    {
      // Continue
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    if (ReadLegalAction())
    {
      if (ReadTribunal())
      {
        if (ReadFips())
        {
          if (entities.Fips.State == 20)
          {
            if (ReadTribunalFeeInformation())
            {
              MoveTribunalFeeInformation(entities.TribunalFeeInformation,
                export.TribunalFeeInformation);

              if (AsChar(import.TestDisplay.Flag) == 'Y')
              {
                local.EabReportSend.RptDetail =
                  "Found Trib Fee Info for Legal Action " + NumberToString
                  (entities.LegalAction.Identifier, 15) + "  Tribunal " + NumberToString
                  (entities.Tribunal.Identifier, 15) + "  Rate " + NumberToString
                  ((long)(export.TribunalFeeInformation.Rate.
                    GetValueOrDefault() * 100), 15) + "  Cap " + NumberToString
                  ((long)(export.TribunalFeeInformation.Cap.
                    GetValueOrDefault() * 100), 15);
              }
            }
            else
            {
              local.EabReportSend.RptDetail =
                "Using default CR Fee rate and cap. (tribunal fee information NF)";
                
            }
          }
          else
          {
            local.EabReportSend.RptDetail =
              "Using default CR Fee rate and cap. (Not the state of KS)";
          }
        }
        else
        {
          local.EabReportSend.RptDetail =
            "Using default CR Fee rate and cap.  (FIPS NF)";
        }
      }
      else
      {
        local.EabReportSend.RptDetail =
          "Using default CR Fee rate and cap.  (Tribunal NF)";
      }
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Using default CR Fee rate and cap.  (Legan Action NF)";
    }

    if (AsChar(import.TestDisplay.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
    }
  }

  private static void MoveTribunalFeeInformation(TribunalFeeInformation source,
    TribunalFeeInformation target)
  {
    target.Rate = source.Rate;
    target.Cap = source.Cap;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", import.Per.OtyId);
        db.SetInt32(command, "obId", import.Per.ObgId);
        db.SetString(command, "cspNumber", import.Per.CspNumber);
        db.SetString(command, "cpaType", import.Per.CpaType);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation()
  {
    entities.TribunalFeeInformation.Populated = false;

    return Read("ReadTribunalFeeInformation",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetDate(
          command, "effectiveDate",
          import.Per.CollectionDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.TribunalFeeInformation.TrbId = db.GetInt32(reader, 0);
        entities.TribunalFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.TribunalFeeInformation.EffectiveDate = db.GetDate(reader, 2);
        entities.TribunalFeeInformation.Rate = db.GetNullableDecimal(reader, 3);
        entities.TribunalFeeInformation.Cap = db.GetNullableDecimal(reader, 4);
        entities.TribunalFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.TribunalFeeInformation.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Per.
    /// </summary>
    [JsonPropertyName("per")]
    public Collection Per
    {
      get => per ??= new();
      set => per = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of TestDisplay.
    /// </summary>
    [JsonPropertyName("testDisplay")]
    public Common TestDisplay
    {
      get => testDisplay ??= new();
      set => testDisplay = value;
    }

    private Collection per;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea max;
    private Common testDisplay;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    private TribunalFeeInformation tribunalFeeInformation;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private Fips fips;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private TribunalFeeInformation tribunalFeeInformation;
  }
#endregion
}
