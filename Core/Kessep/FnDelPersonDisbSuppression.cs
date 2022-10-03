// Program: FN_DEL_PERSON_DISB_SUPPRESSION, ID: 371751822, model: 746.
// Short name: SWE00392
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DEL_PERSON_DISB_SUPPRESSION.
/// </summary>
[Serializable]
public partial class FnDelPersonDisbSuppression: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DEL_PERSON_DISB_SUPPRESSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDelPersonDisbSuppression(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDelPersonDisbSuppression.
  /// </summary>
  public FnDelPersonDisbSuppression(IContext context, Import import,
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
    // ******************************************************************
    //                  Developed for KESSEP by MTW
    //                   D. M. Nilsen  09/06/95
    // *******************************************************************
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    Added changes for suppression by court 
    // order number.
    // *******************************************************************
    local.CurrentDate.Date = Now().Date;

    if (ReadDisbSuppressionStatusHistory())
    {
      // ***************************************************************
      // Altered the Delete checks to allow a Delete if the Suppression was 
      // created and effective today. RK 10/15/1998. Or in the future
      // ***************************************************************
      local.ExtractDayFromTimestamp.Date =
        Date(entities.DisbSuppressionStatusHistory.CreatedTimestamp);

      if (Lt(local.CurrentDate.Date,
        entities.DisbSuppressionStatusHistory.EffectiveDate) || Equal
        (entities.DisbSuppressionStatusHistory.EffectiveDate,
        local.CurrentDate.Date) && Equal
        (local.ExtractDayFromTimestamp.Date, local.CurrentDate.Date))
      {
        // *****  changes for WR 040796
        // *****  changes for WR 040796
        DeleteDisbSuppressionStatusHistory();
      }
      else
      {
        ExitState = "FN0000_CANT_DELETE_SUPPRESSION";
      }
    }
    else
    {
      ExitState = "FN0000_DISB_SUPP_STAT_NF";
    }
  }

  private void DeleteDisbSuppressionStatusHistory()
  {
    Update("DeleteDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.DisbSuppressionStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.DisbSuppressionStatusHistory.CspNumber);
        db.SetInt32(
          command, "dssGeneratedId",
          entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
      });
  }

  private bool ReadDisbSuppressionStatusHistory()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "dssGeneratedId",
          import.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
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
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of ExtractDayFromTimestamp.
    /// </summary>
    [JsonPropertyName("extractDayFromTimestamp")]
    public DateWorkArea ExtractDayFromTimestamp
    {
      get => extractDayFromTimestamp ??= new();
      set => extractDayFromTimestamp = value;
    }

    /// <summary>
    /// A value of ExpireEffectiveDateAttributes.
    /// </summary>
    [JsonPropertyName("expireEffectiveDateAttributes")]
    public ExpireEffectiveDateAttributes ExpireEffectiveDateAttributes
    {
      get => expireEffectiveDateAttributes ??= new();
      set => expireEffectiveDateAttributes = value;
    }

    private DateWorkArea currentDate;
    private DateWorkArea extractDayFromTimestamp;
    private ExpireEffectiveDateAttributes expireEffectiveDateAttributes;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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

    private CsePerson csePerson;
    private CsePersonAccount obligee;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private LegalAction legalAction;
  }
#endregion
}
