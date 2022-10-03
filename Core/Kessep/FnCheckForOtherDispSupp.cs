// Program: FN_CHECK_FOR_OTHER_DISP_SUPP, ID: 371751819, model: 746.
// Short name: SWE00316
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CHECK_FOR_OTHER_DISP_SUPP.
/// </para>
/// <para>
/// RESP: FINANCE
/// This common action block will check for a disbursement from the other type 
/// of disbursement supression in effect.
/// </para>
/// </summary>
[Serializable]
public partial class FnCheckForOtherDispSupp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_FOR_OTHER_DISP_SUPP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckForOtherDispSupp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckForOtherDispSupp.
  /// </summary>
  public FnCheckForOtherDispSupp(IContext context, Import import, Export export):
    
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
    // ****************************************************************
    // Added the bottom Read Each to see if there was more than  one Collection 
    // Suppression tied to this Payee. RK 10/26/98
    // ****************************************************************
    local.CurrentDate.Date = Now().Date;

    // find type of disbursement suppression to read
    if (AsChar(import.DisbSuppressionStatusHistory.Type1) == 'P')
    {
      local.DisbSuppressionStatusHistory.Type1 = "C";
    }
    else
    {
      local.DisbSuppressionStatusHistory.Type1 = "P";
    }

    export.SuppressedAll.Flag = "N";
    export.OtherCollSuppressExist.Flag = "N";

    // ****************************************************************
    // If the Suppression coming in is by Person then check for Collection Type 
    // suppression. And if Collection Type then check for Person suppression.
    // ****************************************************************
    if (ReadDisbSuppressionStatusHistory2())
    {
      // Found one
      export.SuppressedAll.Flag = "Y";
    }

    // ***************************************************
    // Search for other Collection Type suppressions.
    // **************************************************
    if (AsChar(import.DisbSuppressionStatusHistory.Type1) == 'C')
    {
      export.OtherCollSuppressExist.Flag = "N";

      if (ReadDisbSuppressionStatusHistory1())
      {
        // Found one
        export.OtherCollSuppressExist.Flag = "Y";
      }
    }
  }

  private bool ReadDisbSuppressionStatusHistory1()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.
          SetString(command, "type", import.DisbSuppressionStatusHistory.Type1);
          
        db.SetInt32(
          command, "dssGeneratedId",
          import.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
        db.SetDate(
          command, "discontinueDate",
          local.CurrentDate.Date.GetValueOrDefault());
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
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory2()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "type", local.DisbSuppressionStatusHistory.Type1);
        db.SetDate(
          command, "discontinueDate",
          import.DisbSuppressionStatusHistory.EffectiveDate.
            GetValueOrDefault());
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
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 5);
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
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OtherCollSuppressExist.
    /// </summary>
    [JsonPropertyName("otherCollSuppressExist")]
    public Common OtherCollSuppressExist
    {
      get => otherCollSuppressExist ??= new();
      set => otherCollSuppressExist = value;
    }

    /// <summary>
    /// A value of SuppressedAll.
    /// </summary>
    [JsonPropertyName("suppressedAll")]
    public Common SuppressedAll
    {
      get => suppressedAll ??= new();
      set => suppressedAll = value;
    }

    private Common otherCollSuppressExist;
    private Common suppressedAll;
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
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private DateWorkArea currentDate;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
