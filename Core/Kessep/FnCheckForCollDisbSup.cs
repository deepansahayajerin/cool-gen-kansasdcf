// Program: FN_CHECK_FOR_COLL_DISB_SUP, ID: 372544594, model: 746.
// Short name: SWE00314
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CHECK_FOR_COLL_DISB_SUP.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will be used to check to determine if a disbursement 
/// suppresion is turned on at the collection type level.  A table of the
/// suppressed collection types is returned.
/// </para>
/// </summary>
[Serializable]
public partial class FnCheckForCollDisbSup: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_FOR_COLL_DISB_SUP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckForCollDisbSup(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckForCollDisbSup.
  /// </summary>
  public FnCheckForCollDisbSup(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // 97-05-01  ????????  A.Kinney  Changed Current_Date
    // 00-02-15  PR 86861  Fangman  Changes for CR Fee & restructuring.
    // 00-03-06  PR 86768  Fangman  FDSO suppression release date will be based 
    // on collection date instead of process date.
    // 00-04-10  PN  000164  Fangman  Change Read statement to used > instead of
    // >= to not suppress disbursements on the "Date of Discontinuance".
    // ***************************************************
    export.DisbSuppressionStatusHistory.DiscontinueDate =
      local.Initialized.DiscontinueDate;

    if (ReadDisbSuppressionStatusHistoryCollectionType())
    {
      // *****************************************************************
      // The F collection type should have a 6 month from this process date end 
      // date.
      // *****************************************************************
      if (entities.CollectionType.SequentialIdentifier == 3)
      {
        export.DisbSuppressionStatusHistory.DiscontinueDate =
          AddMonths(import.DisbursementTransaction.CollectionDate, 6);
      }
      else
      {
        export.DisbSuppressionStatusHistory.DiscontinueDate =
          entities.DisbSuppressionStatusHistory.DiscontinueDate;
      }
    }
    else
    {
      // No Collection level suppression found for this person and collection 
      // type.
    }
  }

  private bool ReadDisbSuppressionStatusHistoryCollectionType()
  {
    entities.CsePersonAccount.Populated = false;
    entities.CollectionType.Populated = false;
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistoryCollectionType",
      (db, command) =>
      {
        db.SetString(command, "type", import.HardcodeCollectionType.Type1);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableInt32(
          command, "cltSequentialId",
          import.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 6);
        entities.CsePersonAccount.Populated = true;
        entities.CollectionType.Populated = true;
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of HardcodeCollectionType.
    /// </summary>
    [JsonPropertyName("hardcodeCollectionType")]
    public DisbSuppressionStatusHistory HardcodeCollectionType
    {
      get => hardcodeCollectionType ??= new();
      set => hardcodeCollectionType = value;
    }

    private CsePerson csePerson;
    private DisbursementTransaction disbursementTransaction;
    private CollectionType collectionType;
    private ProgramProcessingInfo programProcessingInfo;
    private DisbSuppressionStatusHistory hardcodeCollectionType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DisbSuppressionStatusHistory Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private DisbSuppressionStatusHistory initialized;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    private CollectionType collectionType;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }
#endregion
}
