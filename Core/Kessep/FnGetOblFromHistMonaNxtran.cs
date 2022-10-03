// Program: FN_GET_OBL_FROM_HIST_MONA_NXTRAN, ID: 372084602, model: 746.
// Short name: SWE01898
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_GET_OBL_FROM_HIST_MONA_NXTRAN.
/// </summary>
[Serializable]
public partial class FnGetOblFromHistMonaNxtran: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_OBL_FROM_HIST_MONA_NXTRAN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetOblFromHistMonaNxtran(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetOblFromHistMonaNxtran.
  /// </summary>
  public FnGetOblFromHistMonaNxtran(IContext context, Import import,
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
    // --------------------------------------------
    // Change log
    // date		author	remarks
    // 01/23/97	HOOKS	initial creation
    // --------------------------------------------
    // ================================================
    // 9-2-98  B Adams	Combined some Read actions into extended Reads
    // 12-29-98 - b adams  -  READ properties set
    // ================================================
    if (ReadInfrastructure())
    {
      // ***  Combined reads of obligation & obligation-type  ***
      // =================================================
      // 4/15/99 - bud adams  -  new relationship between Obligation
      //   and Legal_Action_Detail needs to be used for this read;
      //   otherwise, reading on the relationship between Obligation
      //   and Legal_Action will possibly yield ambiguous results.
      //   1040 is my favorite number.
      // =================================================
      if (ReadObligationObligationType())
      {
        export.Obligation.SystemGeneratedIdentifier =
          entities.Obligation.SystemGeneratedIdentifier;
        MoveObligationType(entities.ObligationType, export.ObligationType);

        // ***  Combined reads of legal-action & legal-action-detail  ***
        // =================================================
        // 4/15/99 - bud adams  -  new relationship between Obligation
        //   and Legal_Action_Detail needs to be used for this read;
        //   otherwise, reading on the relationship between Obligation
        //   and Legal_Action will possibly yield ambiguous results.
        // =================================================
        if (ReadLegalActionLegalActionDetail())
        {
          export.LegalAction.Assign(entities.LegalAction);
          MoveLegalActionDetail(entities.LegalActionDetail,
            export.LegalActionDetail);
        }
        else
        {
          // #################################################
          // #################################################
          // FOR A WHILE, CONVERSION DATA WILL NOT SUPPORT
          // THAT NEW RELATIONSHIP - SO WE HAVE TO USE THE OLD
          // ONE AND HOPE IT DOESN'T BLOW UP.
          // #################################################
          // #################################################
          if (!ReadLegalActionDetailLegalAction())
          {
            if (Equal(entities.Infrastructure.UserId, "OACC") || Equal
              (entities.Infrastructure.UserId, "ONAC"))
            {
              ExitState = "LEGAL_ACTION_NF";
            }

            if (ReadLegalAction())
            {
              ExitState = "LEGAL_ACTION_DETAIL_NF";
            }
            else
            {
              // ***---  legal action not required for voluntary, fee, and
              // ***---  recovery debts
            }
          }
        }
      }
      else
      {
        ExitState = "OBLIGATION_NF";
      }
    }
    else
    {
      ExitState = "INFRASTRUCTURE_NF";
    }
  }

  private static void MoveLegalActionDetail(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.Number = source.Number;
    target.DetailType = source.DetailType;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.NextTranInfo.InfrastructureId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.UserId = db.GetString(reader, 1);
        entities.Infrastructure.OtyId = db.GetNullableInt32(reader, 2);
        entities.Infrastructure.CpaType = db.GetNullableString(reader, 3);
        entities.Infrastructure.CspNo = db.GetNullableString(reader, 4);
        entities.Infrastructure.ObgId = db.GetNullableInt32(reader, 5);
        entities.Infrastructure.Populated = true;
        CheckValid<Infrastructure>("CpaType", entities.Infrastructure.CpaType);
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
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetailLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "laDetailNo",
          entities.Obligation.LadNumber.GetValueOrDefault());
        db.SetInt32(
          command, "lgaIdentifier",
          entities.Obligation.LgaIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "otyId", entities.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 2);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 3);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Classification = db.GetString(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionLegalActionDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadLegalActionLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
        db.SetInt32(
          command, "laDetailNo",
          entities.Obligation.LadNumber.GetValueOrDefault());
        db.SetInt32(
          command, "lgaIdentifier",
          entities.Obligation.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 4);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 8);
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Infrastructure.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", entities.Infrastructure.ObgId.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Infrastructure.CspNo ?? "");
        db.SetString(command, "cpaType", entities.Infrastructure.CpaType ?? "");
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.Infrastructure.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 5);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 6);
        entities.ObligationType.Code = db.GetString(reader, 7);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private Obligation obligation;
    private Infrastructure infrastructure;
  }
#endregion
}
