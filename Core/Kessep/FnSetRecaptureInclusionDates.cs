// Program: FN_SET_RECAPTURE_INCLUSION_DATES, ID: 372127107, model: 746.
// Short name: SWE00609
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_SET_RECAPTURE_INCLUSION_DATES.
/// </para>
/// <para>
/// This action block will set the discontinue and effective dates or create  
/// another recapture inclusion record.
/// </para>
/// </summary>
[Serializable]
public partial class FnSetRecaptureInclusionDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SET_RECAPTURE_INCLUSION_DATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSetRecaptureInclusionDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSetRecaptureInclusionDates.
  /// </summary>
  public FnSetRecaptureInclusionDates(IContext context, Import import,
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
    // ---------------------------------------------
    // FN Set Recapture Inclusion Dates
    // Date Created    Created by
    // 08/29/1995      Terry W. Cooley - MTW
    // 04/29/1997      C. Dasgupta - MTW Changed current Date
    // 06/04/1997      H. Kennedy - fixed setting of local current date.
    //                 Setting wrong date.
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    if (!ReadObligation())
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    if (Equal(global.Command, "NORCP"))
    {
      // Change from Y to N
      if (ReadRecaptureInclusion1())
      {
        try
        {
          UpdateRecaptureInclusion1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "RECAPTURE_INCLUSION_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "RECAPTURE_INCLUSION_NF";
      }
    }
    else if (Equal(global.Command, "YESRCP"))
    {
      // Change from N to Y
      // *** Get the most recent Recapture Inclusion.
      if (ReadRecaptureInclusion2())
      {
        local.Last.SystemGeneratedId =
          entities.RecaptureInclusion.SystemGeneratedId;
      }

      // *** If this most recent inclusion has a create and discontinue date 
      // equal to today's date, simply blank out the discontinue date.
      // Otherwise, continue with create.
      if (Equal(entities.RecaptureInclusion.DiscontinueDate, Now().Date) && Equal
        (entities.RecaptureInclusion.EffectiveDate, Now().Date))
      {
        try
        {
          UpdateRecaptureInclusion2();
          export.RecaptureInclusion.SystemGeneratedId =
            entities.RecaptureInclusion.SystemGeneratedId;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "RECAPTURE_INCLUSION_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "RECAPTURE_INCLUSION_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        return;
      }

      try
      {
        CreateRecaptureInclusion();
        export.RecaptureInclusion.SystemGeneratedId =
          entities.RecaptureInclusion.SystemGeneratedId;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "RECAPTURE_INCLUSION_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateRecaptureInclusion()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otyType = entities.Obligation.DtyGeneratedId;
    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedId = local.Last.SystemGeneratedId + 1;
    var discontinueDate = local.Maximum.Date;
    var effectiveDate = local.Current.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    CheckValid<RecaptureInclusion>("CpaType", cpaType);
    entities.RecaptureInclusion.Populated = false;
    Update("CreateRecaptureInclusion",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "recapInclSysid", systemGeneratedId);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.RecaptureInclusion.OtyType = otyType;
    entities.RecaptureInclusion.ObgGeneratedId = obgGeneratedId;
    entities.RecaptureInclusion.CspNumber = cspNumber;
    entities.RecaptureInclusion.CpaType = cpaType;
    entities.RecaptureInclusion.SystemGeneratedId = systemGeneratedId;
    entities.RecaptureInclusion.DiscontinueDate = discontinueDate;
    entities.RecaptureInclusion.EffectiveDate = effectiveDate;
    entities.RecaptureInclusion.CreatedBy = createdBy;
    entities.RecaptureInclusion.CreatedTimestamp = createdTimestamp;
    entities.RecaptureInclusion.LastUpdatedBy = createdBy;
    entities.RecaptureInclusion.LastUpdatedTmst = createdTimestamp;
    entities.RecaptureInclusion.Populated = true;
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadRecaptureInclusion1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.RecaptureInclusion.Populated = false;

    return Read("ReadRecaptureInclusion1",
      (db, command) =>
      {
        db.SetInt32(
          command, "recapInclSysid",
          import.RecaptureInclusion.SystemGeneratedId);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.RecaptureInclusion.OtyType = db.GetInt32(reader, 0);
        entities.RecaptureInclusion.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.RecaptureInclusion.CspNumber = db.GetString(reader, 2);
        entities.RecaptureInclusion.CpaType = db.GetString(reader, 3);
        entities.RecaptureInclusion.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.RecaptureInclusion.DiscontinueDate = db.GetDate(reader, 5);
        entities.RecaptureInclusion.EffectiveDate = db.GetDate(reader, 6);
        entities.RecaptureInclusion.CreatedBy = db.GetString(reader, 7);
        entities.RecaptureInclusion.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.RecaptureInclusion.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.RecaptureInclusion.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 10);
        entities.RecaptureInclusion.Populated = true;
        CheckValid<RecaptureInclusion>("CpaType",
          entities.RecaptureInclusion.CpaType);
      });
  }

  private bool ReadRecaptureInclusion2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.RecaptureInclusion.Populated = false;

    return Read("ReadRecaptureInclusion2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.RecaptureInclusion.OtyType = db.GetInt32(reader, 0);
        entities.RecaptureInclusion.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.RecaptureInclusion.CspNumber = db.GetString(reader, 2);
        entities.RecaptureInclusion.CpaType = db.GetString(reader, 3);
        entities.RecaptureInclusion.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.RecaptureInclusion.DiscontinueDate = db.GetDate(reader, 5);
        entities.RecaptureInclusion.EffectiveDate = db.GetDate(reader, 6);
        entities.RecaptureInclusion.CreatedBy = db.GetString(reader, 7);
        entities.RecaptureInclusion.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.RecaptureInclusion.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.RecaptureInclusion.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 10);
        entities.RecaptureInclusion.Populated = true;
        CheckValid<RecaptureInclusion>("CpaType",
          entities.RecaptureInclusion.CpaType);
      });
  }

  private void UpdateRecaptureInclusion1()
  {
    System.Diagnostics.Debug.Assert(entities.RecaptureInclusion.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.RecaptureInclusion.Populated = false;
    Update("UpdateRecaptureInclusion1",
      (db, command) =>
      {
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(command, "otyType", entities.RecaptureInclusion.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.RecaptureInclusion.ObgGeneratedId);
        db.
          SetString(command, "cspNumber", entities.RecaptureInclusion.CspNumber);
          
        db.SetString(command, "cpaType", entities.RecaptureInclusion.CpaType);
        db.SetInt32(
          command, "recapInclSysid",
          entities.RecaptureInclusion.SystemGeneratedId);
      });

    entities.RecaptureInclusion.DiscontinueDate = discontinueDate;
    entities.RecaptureInclusion.LastUpdatedBy = lastUpdatedBy;
    entities.RecaptureInclusion.LastUpdatedTmst = lastUpdatedTmst;
    entities.RecaptureInclusion.Populated = true;
  }

  private void UpdateRecaptureInclusion2()
  {
    System.Diagnostics.Debug.Assert(entities.RecaptureInclusion.Populated);

    var discontinueDate = local.Maximum.Date;

    entities.RecaptureInclusion.Populated = false;
    Update("UpdateRecaptureInclusion2",
      (db, command) =>
      {
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(command, "otyType", entities.RecaptureInclusion.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.RecaptureInclusion.ObgGeneratedId);
        db.
          SetString(command, "cspNumber", entities.RecaptureInclusion.CspNumber);
          
        db.SetString(command, "cpaType", entities.RecaptureInclusion.CpaType);
        db.SetInt32(
          command, "recapInclSysid",
          entities.RecaptureInclusion.SystemGeneratedId);
      });

    entities.RecaptureInclusion.DiscontinueDate = discontinueDate;
    entities.RecaptureInclusion.Populated = true;
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
    /// <summary>A ZdelGroup group.</summary>
    [Serializable]
    public class ZdelGroup
    {
      /// <summary>
      /// A value of ZdelImportDetailPrev.
      /// </summary>
      [JsonPropertyName("zdelImportDetailPrev")]
      public DebtDetail ZdelImportDetailPrev
      {
        get => zdelImportDetailPrev ??= new();
        set => zdelImportDetailPrev = value;
      }

      /// <summary>
      /// A value of ZdelImportDetailSelect.
      /// </summary>
      [JsonPropertyName("zdelImportDetailSelect")]
      public Common ZdelImportDetailSelect
      {
        get => zdelImportDetailSelect ??= new();
        set => zdelImportDetailSelect = value;
      }

      /// <summary>
      /// A value of ZdelpImportDetailObligationType.
      /// </summary>
      [JsonPropertyName("zdelpImportDetailObligationType")]
      public ObligationType ZdelpImportDetailObligationType
      {
        get => zdelpImportDetailObligationType ??= new();
        set => zdelpImportDetailObligationType = value;
      }

      /// <summary>
      /// A value of ZdeloupImportDetail.
      /// </summary>
      [JsonPropertyName("zdeloupImportDetail")]
      public DebtDetail ZdeloupImportDetail
      {
        get => zdeloupImportDetail ??= new();
        set => zdeloupImportDetail = value;
      }

      /// <summary>
      /// A value of ZdelpImportDetailObligationTransaction.
      /// </summary>
      [JsonPropertyName("zdelpImportDetailObligationTransaction")]
      public ObligationTransaction ZdelpImportDetailObligationTransaction
      {
        get => zdelpImportDetailObligationTransaction ??= new();
        set => zdelpImportDetailObligationTransaction = value;
      }

      /// <summary>
      /// A value of ZdelImportDetailRecap.
      /// </summary>
      [JsonPropertyName("zdelImportDetailRecap")]
      public Common ZdelImportDetailRecap
      {
        get => zdelImportDetailRecap ??= new();
        set => zdelImportDetailRecap = value;
      }

      /// <summary>
      /// A value of ZdelImportDetailLegalAction.
      /// </summary>
      [JsonPropertyName("zdelImportDetailLegalAction")]
      public LegalAction ZdelImportDetailLegalAction
      {
        get => zdelImportDetailLegalAction ??= new();
        set => zdelImportDetailLegalAction = value;
      }

      /// <summary>
      /// A value of ZdelImportDetailRecapPrev.
      /// </summary>
      [JsonPropertyName("zdelImportDetailRecapPrev")]
      public Common ZdelImportDetailRecapPrev
      {
        get => zdelImportDetailRecapPrev ??= new();
        set => zdelImportDetailRecapPrev = value;
      }

      /// <summary>
      /// A value of ZdelImportDetailObligation.
      /// </summary>
      [JsonPropertyName("zdelImportDetailObligation")]
      public Obligation ZdelImportDetailObligation
      {
        get => zdelImportDetailObligation ??= new();
        set => zdelImportDetailObligation = value;
      }

      /// <summary>
      /// A value of ZdelImportDetailRecaptureInclusion.
      /// </summary>
      [JsonPropertyName("zdelImportDetailRecaptureInclusion")]
      public RecaptureInclusion ZdelImportDetailRecaptureInclusion
      {
        get => zdelImportDetailRecaptureInclusion ??= new();
        set => zdelImportDetailRecaptureInclusion = value;
      }

      private DebtDetail zdelImportDetailPrev;
      private Common zdelImportDetailSelect;
      private ObligationType zdelpImportDetailObligationType;
      private DebtDetail zdeloupImportDetail;
      private ObligationTransaction zdelpImportDetailObligationTransaction;
      private Common zdelImportDetailRecap;
      private LegalAction zdelImportDetailLegalAction;
      private Common zdelImportDetailRecapPrev;
      private Obligation zdelImportDetailObligation;
      private RecaptureInclusion zdelImportDetailRecaptureInclusion;
    }

    /// <summary>
    /// A value of RecaptureInclusion.
    /// </summary>
    [JsonPropertyName("recaptureInclusion")]
    public RecaptureInclusion RecaptureInclusion
    {
      get => recaptureInclusion ??= new();
      set => recaptureInclusion = value;
    }

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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

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
    /// A value of ZdelImportLast.
    /// </summary>
    [JsonPropertyName("zdelImportLast")]
    public RecaptureInclusion ZdelImportLast
    {
      get => zdelImportLast ??= new();
      set => zdelImportLast = value;
    }

    /// <summary>
    /// Gets a value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ZdelGroup Zdel
    {
      get => zdel ?? (zdel = new());
      set => zdel = value;
    }

    private RecaptureInclusion recaptureInclusion;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePerson csePerson;
    private RecaptureInclusion zdelImportLast;
    private ZdelGroup zdel;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RecaptureInclusion.
    /// </summary>
    [JsonPropertyName("recaptureInclusion")]
    public RecaptureInclusion RecaptureInclusion
    {
      get => recaptureInclusion ??= new();
      set => recaptureInclusion = value;
    }

    /// <summary>
    /// A value of Zdelxxx.
    /// </summary>
    [JsonPropertyName("zdelxxx")]
    public Common Zdelxxx
    {
      get => zdelxxx ??= new();
      set => zdelxxx = value;
    }

    private RecaptureInclusion recaptureInclusion;
    private Common zdelxxx;
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
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public RecaptureInclusion Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of TryCreation.
    /// </summary>
    [JsonPropertyName("tryCreation")]
    public Common TryCreation
    {
      get => tryCreation ??= new();
      set => tryCreation = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public SystemGenerated Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    private DateWorkArea current;
    private RecaptureInclusion last;
    private Common tryCreation;
    private DateWorkArea dateWorkArea;
    private SystemGenerated work;
    private DateWorkArea maximum;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of RecaptureInclusion.
    /// </summary>
    [JsonPropertyName("recaptureInclusion")]
    public RecaptureInclusion RecaptureInclusion
    {
      get => recaptureInclusion ??= new();
      set => recaptureInclusion = value;
    }

    /// <summary>
    /// A value of ZdelExistingLast.
    /// </summary>
    [JsonPropertyName("zdelExistingLast")]
    public RecaptureInclusion ZdelExistingLast
    {
      get => zdelExistingLast ??= new();
      set => zdelExistingLast = value;
    }

    private ObligationType obligationType;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private Obligation obligation;
    private RecaptureInclusion recaptureInclusion;
    private RecaptureInclusion zdelExistingLast;
  }
#endregion
}
