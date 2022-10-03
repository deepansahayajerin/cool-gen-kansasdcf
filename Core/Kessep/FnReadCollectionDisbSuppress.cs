// Program: FN_READ_COLLECTION_DISB_SUPPRESS, ID: 371751821, model: 746.
// Short name: SWE00541
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_COLLECTION_DISB_SUPPRESS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will get the disbursement suppression information for a 
/// CSE Person.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadCollectionDisbSuppress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_COLLECTION_DISB_SUPPRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCollectionDisbSuppress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCollectionDisbSuppress.
  /// </summary>
  public FnReadCollectionDisbSuppress(IContext context, Import import,
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
    // ****************************************************************
    // Changed the date ranges on the reads to Read for the current record or 
    // the record that the effective date was entered for. Before it was only
    // reading for the record with the highest effective date. This resulted in
    // a future dated record being brought back instead of the current one.
    // Also view matched a flag from 'FN_Check for other Disp Supp to say that 
    // there is more than one Collection  suppressions for this Payee. RK 10/26/
    // 98
    // ***************************************************************
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    Added changes for suppression by court 
    // order number.
    // *******************************************************************
    export.CollectionType.Assign(import.CollectionType);
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    // now read the collection type
    if (ReadCollectionType())
    {
      export.CollectionType.Assign(entities.CollectionType);
    }
    else
    {
      ExitState = "FN0000_COLLECTION_TYPE_NF";

      return;
    }

    if (!IsEmpty(import.LegalAction.StandardNumber))
    {
      if (!ReadLegalAction())
      {
        ExitState = "LEGAL_ACTION_NF_RB";

        return;
      }
    }

    // Date was not entered
    if (Equal(import.DisbSuppressionStatusHistory.EffectiveDate, null))
    {
      // ***** Read for the current record******************
      ExitState = "FN0000_DISB_SUPP_STAT_NF";

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

      // *****  changes for WR 040796
      // ****************************************************************
      // Read for the future suppression that has the lowest effective date.
      // ****************************************************************
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
    local.DisbSuppressionStatusHistory.Type1 = "C";

    // *****  changes for WR 040796
    UseFnCheckForOtherDispSupp();

    // now do output edits
    if (Equal(export.DisbSuppressionStatusHistory.DiscontinueDate,
      local.Maximum.Date))
    {
      export.DisbSuppressionStatusHistory.DiscontinueDate = null;
    }

    if (IsEmpty(export.DisbSuppressionStatusHistory.LastUpdatedBy))
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

    export.OtherCollSupressExist.Flag = useExport.OtherCollSuppressExist.Flag;
    export.SuppressAll.Flag = useExport.SuppressedAll.Flag;
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Name = db.GetString(reader, 2);
        entities.CollectionType.Populated = true;
      });
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
        db.SetNullableInt32(
          command, "cltSequentialId",
          entities.CollectionType.SequentialIdentifier);
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
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 10);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 11);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 12);
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
        db.SetNullableInt32(
          command, "cltSequentialId",
          entities.CollectionType.SequentialIdentifier);
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
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 10);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 11);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 12);
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
        db.SetNullableInt32(
          command, "cltSequentialId",
          entities.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 10);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 11);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 12);
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
        db.SetNullableInt32(
          command, "cltSequentialId",
          entities.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 10);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 11);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 12);
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
        db.SetNullableInt32(
          command, "cltSequentialId",
          entities.CollectionType.SequentialIdentifier);
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
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 10);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 11);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 12);
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
        db.SetNullableInt32(
          command, "cltSequentialId",
          entities.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 10);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 11);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 12);
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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

    private CollectionType collectionType;
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
    /// A value of OtherCollSupressExist.
    /// </summary>
    [JsonPropertyName("otherCollSupressExist")]
    public Common OtherCollSupressExist
    {
      get => otherCollSupressExist ??= new();
      set => otherCollSupressExist = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

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

    private Common otherCollSupressExist;
    private CollectionType collectionType;
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

    private Common futureSuppression;
    private Common currentSuppression;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DateWorkArea maximum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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

    private LegalAction legalAction;
    private CollectionType collectionType;
    private CsePerson csePerson;
    private CsePersonAccount obligee;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }
#endregion
}
