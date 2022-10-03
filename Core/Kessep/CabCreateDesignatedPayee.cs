// Program: CAB_CREATE_DESIGNATED_PAYEE, ID: 371752455, model: 746.
// Short name: SWE00033
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_CREATE_DESIGNATED_PAYEE.
/// </summary>
[Serializable]
public partial class CabCreateDesignatedPayee: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_CREATE_DESIGNATED_PAYEE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabCreateDesignatedPayee(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabCreateDesignatedPayee.
  /// </summary>
  public CabCreateDesignatedPayee(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************
    // PR#82489 SWSRKXD 1/4/2000
    // - Delete views of ob_trn_desig_payee.
    // ***************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (!ReadCsePerson1())
    {
      ExitState = "FN0000_DESIG_PAYEE_CSE_PERSON_NF";

      return;
    }

    // <<< Should set up the Cse-person-desig-payee after all validations are 
    // performed >>>
    if (!ReadCsePerson2())
    {
      ExitState = "FN0000_DESIG_PAYEE_CSE_PERSON_NF";

      return;
    }

    // <<< Check for Duplicate Designated Payee >>>
    if (ReadCsePersonDesigPayee1())
    {
      ExitState = "FN0000_DESIG_PAYEE_AE";

      return;
    }

    // <<< Check for Date Overlap >>>
    // ****************************************************************
    // The check below only worked if the on-screen DP was already in the 
    // database associated to this Payee. A DP with overlapping dates that hadn'
    // t been this Payee's DP before was allowed.
    // Also changed the first where condition to be less than or equal to, from 
    // just less than. RK 9/21/98
    // ****************************************************************
    if (ReadCsePersonDesigPayee2())
    {
      ExitState = "FN0000_DATE_OVERLAP_WITH_DP";

      return;
    }

    // <<< Create the designated payee >>>
    for(local.RetryLoop.Count = 1; local.RetryLoop.Count <= 5; ++
      local.RetryLoop.Count)
    {
      try
      {
        CreateCsePersonDesigPayee();
        import.DpInfoInOneView.SequentialIdentifier =
          entities.CsePersonDesigPayee.SequentialIdentifier;
        import.DpInfoInOneView.CreatedBy =
          entities.CsePersonDesigPayee.CreatedBy;
        import.DpInfoInOneView.CreatedTmst =
          entities.CsePersonDesigPayee.CreatedTmst;

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DESIG_PAYEE_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_PERMITTED_VAL_VIOLATN_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (local.RetryLoop.Count >= 5)
    {
      ExitState = "ACO_RE0000_CREATE_UNSUCCESFUL_RB";
    }
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private void CreateCsePersonDesigPayee()
  {
    var sequentialIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDate = import.DpInfoInOneView.EffectiveDate;
    var discontinueDate = import.DpInfoInOneView.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var notes = import.DpInfoInOneView.Notes ?? "";
    var csePersoNum = entities.Payee.Number;
    var csePersNum = entities.DesignatedPayee.Number;

    entities.CsePersonDesigPayee.Populated = false;
    Update("CreateCsePersonDesigPayee",
      (db, command) =>
      {
        db.SetInt32(command, "sequentialId", sequentialIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "notes", notes);
        db.SetString(command, "csePersoNum", csePersoNum);
        db.SetNullableString(command, "csePersNum", csePersNum);
      });

    entities.CsePersonDesigPayee.SequentialIdentifier = sequentialIdentifier;
    entities.CsePersonDesigPayee.EffectiveDate = effectiveDate;
    entities.CsePersonDesigPayee.DiscontinueDate = discontinueDate;
    entities.CsePersonDesigPayee.CreatedBy = createdBy;
    entities.CsePersonDesigPayee.CreatedTmst = createdTmst;
    entities.CsePersonDesigPayee.LastUpdatedBy = "";
    entities.CsePersonDesigPayee.LastUpdatedTmst = null;
    entities.CsePersonDesigPayee.Notes = notes;
    entities.CsePersonDesigPayee.CsePersoNum = csePersoNum;
    entities.CsePersonDesigPayee.CsePersNum = csePersNum;
    entities.CsePersonDesigPayee.Populated = true;
  }

  private bool ReadCsePerson1()
  {
    entities.DesignatedPayee.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.DesignatedPayee.Number);
      },
      (db, reader) =>
      {
        entities.DesignatedPayee.Number = db.GetString(reader, 0);
        entities.DesignatedPayee.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Payee.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Payee.Number);
      },
      (db, reader) =>
      {
        entities.Payee.Number = db.GetString(reader, 0);
        entities.Payee.Populated = true;
      });
  }

  private bool ReadCsePersonDesigPayee1()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return Read("ReadCsePersonDesigPayee1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DpInfoInOneView.EffectiveDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.DpInfoInOneView.DiscontinueDate.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersNum", entities.DesignatedPayee.Number);
        db.SetString(command, "csePersoNum", entities.Payee.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;
      });
  }

  private bool ReadCsePersonDesigPayee2()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return Read("ReadCsePersonDesigPayee2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          import.DpInfoInOneView.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.DpInfoInOneView.DiscontinueDate.GetValueOrDefault());
        db.SetString(command, "csePersoNum", entities.Payee.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;
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
    /// A value of DpInfoInOneView.
    /// </summary>
    [JsonPropertyName("dpInfoInOneView")]
    public CsePersonDesigPayee DpInfoInOneView
    {
      get => dpInfoInOneView ??= new();
      set => dpInfoInOneView = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePerson Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of DataPassedThruFlow.
    /// </summary>
    [JsonPropertyName("dataPassedThruFlow")]
    public Common DataPassedThruFlow
    {
      get => dataPassedThruFlow ??= new();
      set => dataPassedThruFlow = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    private CsePersonDesigPayee dpInfoInOneView;
    private CsePerson payee;
    private CsePerson designatedPayee;
    private Common dataPassedThruFlow;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Created.
    /// </summary>
    [JsonPropertyName("created")]
    public DateWorkArea Created
    {
      get => created ??= new();
      set => created = value;
    }

    private DateWorkArea created;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of RetryLoop.
    /// </summary>
    [JsonPropertyName("retryLoop")]
    public Common RetryLoop
    {
      get => retryLoop ??= new();
      set => retryLoop = value;
    }

    private DateWorkArea dateWorkArea;
    private Common retryLoop;
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
    /// A value of CsePersonDesigPayee.
    /// </summary>
    [JsonPropertyName("csePersonDesigPayee")]
    public CsePersonDesigPayee CsePersonDesigPayee
    {
      get => csePersonDesigPayee ??= new();
      set => csePersonDesigPayee = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePerson Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    private ObligationTransaction obligationTransaction;
    private CsePersonDesigPayee csePersonDesigPayee;
    private CsePerson designatedPayee;
    private CsePerson payee;
  }
#endregion
}
