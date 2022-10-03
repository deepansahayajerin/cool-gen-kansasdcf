// Program: FN_READ_PERSON_DISB_SUPPRESSION, ID: 371755039, model: 746.
// Short name: SWE00579
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_PERSON_DISB_SUPPRESSION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will get the disbursement suppression information for a 
/// CSE Person.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadPersonDisbSuppression: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_PERSON_DISB_SUPPRESSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadPersonDisbSuppression(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadPersonDisbSuppression.
  /// </summary>
  public FnReadPersonDisbSuppression(IContext context, Import import,
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
    // 09/06/95       Developed for KESSEP by MTW
    //                        D. M. Nilsen  09/06/95
    // 10/19/98         Changed Reads to look for the current suppression first 
    // then the nearest future suppression. RK
    // *******************************************************************
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    Added changes for suppression by court 
    // order number.
    // *******************************************************************
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    // *****  changes for WR 040796
    if (!IsEmpty(import.LegalAction.StandardNumber))
    {
      if (ReadLegalAction())
      {
        // continue
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF_RB";

        return;
      }
    }

    // *****  changes for WR 040796
    if (Equal(import.DisbSuppressionStatusHistory.EffectiveDate, null))
    {
      // NO date was entered
      // ************************
      // Find the current record
      // ************************
      // *****  changes for WR 040796
      if (!IsEmpty(import.LegalAction.StandardNumber))
      {
        if (ReadDisbSuppressionStatusHistory2())
        {
          export.DisbSuppressionStatusHistory.Assign(
            entities.DisbSuppressionStatusHistory);
          local.CurrentSuppression.Flag = "Y";
        }
        else
        {
          // **********************************************************
          // Not a problem, go look for a future dated suppression.
          // **********************************************************
        }
      }
      else
      {
        // *****  changes for WR 040796
        if (ReadDisbSuppressionStatusHistory4())
        {
          export.DisbSuppressionStatusHistory.Assign(
            entities.DisbSuppressionStatusHistory);
          local.CurrentSuppression.Flag = "Y";
        }
        else
        {
          // **********************************************************
          // Not a problem, go look for a future dated suppression.
          // **********************************************************
        }
      }

      // *******************************************************
      // Read for the future suppression that has the lowest effective date.
      // *******************************************************
      // *****  changes for WR 040796
      if (!IsEmpty(import.LegalAction.StandardNumber))
      {
        if (ReadDisbSuppressionStatusHistory5())
        {
          local.FutureSuppression.Flag = "Y";

          if (AsChar(local.CurrentSuppression.Flag) == 'Y')
          {
            // ************************************************************
            // Both an active and future suppression have been found
            // ************************************************************
            ExitState = "FN0000_ACTIVE_AND_FUTURE_SUPPRS";
          }
          else
          {
            // ****************************************
            // Only a future suppression was found.
            // ****************************************
            export.DisbSuppressionStatusHistory.Assign(
              entities.DisbSuppressionStatusHistory);
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
      }
      else
      {
        // *****  changes for WR 040796
        if (ReadDisbSuppressionStatusHistory6())
        {
          local.FutureSuppression.Flag = "Y";

          if (AsChar(local.CurrentSuppression.Flag) == 'Y')
          {
            // ************************************************************
            // Both an active and future suppression have been found
            // ************************************************************
            ExitState = "FN0000_ACTIVE_AND_FUTURE_SUPPRS";
          }
          else
          {
            // ****************************************
            // Only a future suppression was found.
            // ****************************************
            export.DisbSuppressionStatusHistory.Assign(
              entities.DisbSuppressionStatusHistory);
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
      }

      if (AsChar(local.FutureSuppression.Flag) != 'Y')
      {
        if (AsChar(local.CurrentSuppression.Flag) == 'Y')
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          ExitState = "FN0000_NO_ACTIVE_OR_FUTURE_SUPPR";

          return;
        }
      }
    }
    else
    {
      // Date was entered
      // ***** Read by effective_date.
      // *****  changes for WR 040796
      if (!IsEmpty(import.LegalAction.StandardNumber))
      {
        if (ReadDisbSuppressionStatusHistory1())
        {
          export.DisbSuppressionStatusHistory.Assign(
            entities.DisbSuppressionStatusHistory);
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          ExitState = "FN0000_DISB_SUPP_NF_FOR_DATE";

          return;
        }
      }
      else
      {
        // *****  changes for WR 040796
        if (ReadDisbSuppressionStatusHistory3())
        {
          export.DisbSuppressionStatusHistory.Assign(
            entities.DisbSuppressionStatusHistory);
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          ExitState = "FN0000_DISB_SUPP_NF_FOR_DATE";

          return;
        }
      }
    }

    local.DisbSuppressionStatusHistory.Assign(
      export.DisbSuppressionStatusHistory);
    local.DisbSuppressionStatusHistory.EffectiveDate = Now().Date;

    // *****  changes for WR 040796
    local.DisbSuppressionStatusHistory.Type1 = "P";

    // *****  changes for WR 040796
    UseFnCheckForOtherDispSupp();

    // now do final display edits
    if (Equal(entities.DisbSuppressionStatusHistory.DiscontinueDate,
      local.Maximum.Date))
    {
      export.DisbSuppressionStatusHistory.DiscontinueDate = null;
    }

    if (IsEmpty(entities.DisbSuppressionStatusHistory.LastUpdatedBy))
    {
      export.DisbSuppressionStatusHistory.LastUpdatedBy =
        entities.DisbSuppressionStatusHistory.CreatedBy;
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Maximum.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCheckForOtherDispSupp()
  {
    var useImport = new FnCheckForOtherDispSupp.Import();
    var useExport = new FnCheckForOtherDispSupp.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonAccount.Type1 = import.CsePersonAccount.Type1;
    useImport.DisbSuppressionStatusHistory.Assign(
      local.DisbSuppressionStatusHistory);

    Call(FnCheckForOtherDispSupp.Execute, useImport, useExport);

    export.SuppressAll.Flag = useExport.SuppressedAll.Flag;
  }

  private bool ReadDisbSuppressionStatusHistory1()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DisbSuppressionStatusHistory.EffectiveDate.
            GetValueOrDefault());
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
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
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 9);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 10);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
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
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
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
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 9);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 10);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory3()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DisbSuppressionStatusHistory.EffectiveDate.
            GetValueOrDefault());
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
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 9);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 10);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory4()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory4",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
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
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 9);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 10);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory5()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory5",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
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
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 9);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 10);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory6()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory6",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
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
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 9);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 10);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
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
    private CsePersonAccount csePersonAccount;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SuppressAll.
    /// </summary>
    [JsonPropertyName("suppressAll")]
    public Common SuppressAll
    {
      get => suppressAll ??= new();
      set => suppressAll = value;
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

    private Common suppressAll;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FutureSuppression.
    /// </summary>
    [JsonPropertyName("futureSuppression")]
    public Common FutureSuppression
    {
      get => futureSuppression ??= new();
      set => futureSuppression = value;
    }

    /// <summary>
    /// A value of CurrentSuppression.
    /// </summary>
    [JsonPropertyName("currentSuppression")]
    public Common CurrentSuppression
    {
      get => currentSuppression ??= new();
      set => currentSuppression = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private Common futureSuppression;
    private Common currentSuppression;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DateWorkArea maximum;
    private DateWorkArea initialized;
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
