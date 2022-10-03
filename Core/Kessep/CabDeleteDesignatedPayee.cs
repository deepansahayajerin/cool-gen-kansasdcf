// Program: CAB_DELETE_DESIGNATED_PAYEE, ID: 371752457, model: 746.
// Short name: SWE00040
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_DELETE_DESIGNATED_PAYEE.
/// </summary>
[Serializable]
public partial class CabDeleteDesignatedPayee: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_DELETE_DESIGNATED_PAYEE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabDeleteDesignatedPayee(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabDeleteDesignatedPayee.
  /// </summary>
  public CabDeleteDesignatedPayee(IContext context, Import import, Export export)
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

    if (!ReadCsePerson2())
    {
      ExitState = "FN0000_PAYEE_CSE_PERSON_NF";

      return;
    }

    if (ReadCsePersonDesigPayee())
    {
      local.ConvertTimestampToDate.Date =
        Date(entities.CsePersonDesigPayee.CreatedTmst);

      if (Lt(import.CurrentDate.Date, import.BothTypeOfDpIn1View.EffectiveDate) ||
        Equal
        (import.BothTypeOfDpIn1View.EffectiveDate, import.CurrentDate.Date) && Equal
        (local.ConvertTimestampToDate.Date, import.CurrentDate.Date))
      {
        DeleteCsePersonDesigPayee();
      }
      else
      {
        ExitState = "FN0000_CANT_DELETE_PAST_ACTV";
      }
    }
    else
    {
      ExitState = "FN0000_PERS_DESIG_PAYEE_NF_RB";
    }
  }

  private void DeleteCsePersonDesigPayee()
  {
    Update("DeleteCsePersonDesigPayee",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequentialId",
          entities.CsePersonDesigPayee.SequentialIdentifier);
        db.SetString(
          command, "csePersoNum", entities.CsePersonDesigPayee.CsePersoNum);
      });
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

  private bool ReadCsePersonDesigPayee()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return Read("ReadCsePersonDesigPayee",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequentialId",
          import.BothTypeOfDpIn1View.SequentialIdentifier);
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
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
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
    /// A value of BothTypeOfDpIn1View.
    /// </summary>
    [JsonPropertyName("bothTypeOfDpIn1View")]
    public CsePersonDesigPayee BothTypeOfDpIn1View
    {
      get => bothTypeOfDpIn1View ??= new();
      set => bothTypeOfDpIn1View = value;
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

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public Obligation Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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

    private DateWorkArea currentDate;
    private Common dataPassedThruFlow;
    private CsePersonDesigPayee bothTypeOfDpIn1View;
    private ObligationTransaction obligationTransaction;
    private Obligation persistent;
    private CsePerson payee;
    private CsePerson designatedPayee;
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
    /// A value of ConvertTimestampToDate.
    /// </summary>
    [JsonPropertyName("convertTimestampToDate")]
    public DateWorkArea ConvertTimestampToDate
    {
      get => convertTimestampToDate ??= new();
      set => convertTimestampToDate = value;
    }

    private DateWorkArea convertTimestampToDate;
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

    private ObligationTransaction obligationTransaction;
    private CsePersonDesigPayee csePersonDesigPayee;
    private CsePerson payee;
    private CsePerson designatedPayee;
  }
#endregion
}
