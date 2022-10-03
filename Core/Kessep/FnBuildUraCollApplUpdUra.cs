// Program: FN_BUILD_URA_COLL_APPL_UPD_URA, ID: 374487507, model: 746.
// Short name: SWE02532
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BUILD_URA_COLL_APPL_UPD_URA.
/// </summary>
[Serializable]
public partial class FnBuildUraCollApplUpdUra: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BUILD_URA_COLL_APPL_UPD_URA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBuildUraCollApplUpdUra(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBuildUraCollApplUpdUra.
  /// </summary>
  public FnBuildUraCollApplUpdUra(IContext context, Import import, Export export)
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
    local.ForUpdateUraCollectionApplication.CollectionAmountApplied =
      import.ForUpdate.CollectionAmountApplied;

    if (ReadImHouseholdMbrMnthlySum())
    {
      MoveImHouseholdMbrMnthlySum(entities.ExistingImHouseholdMbrMnthlySum,
        local.ForUpdateImHouseholdMbrMnthlySum);

      if (import.ObligationType.SystemGeneratedIdentifier == import
        .HardcodedMcType.SystemGeneratedIdentifier || import
        .ObligationType.SystemGeneratedIdentifier == import
        .HardcodedMjType.SystemGeneratedIdentifier || import
        .ObligationType.SystemGeneratedIdentifier == import
        .HardcodedMsType.SystemGeneratedIdentifier)
      {
        local.ForUpdateUraCollectionApplication.Type1 = "M";
        local.ForUpdateImHouseholdMbrMnthlySum.UraMedicalAmount =
          local.ForUpdateImHouseholdMbrMnthlySum.UraMedicalAmount.
            GetValueOrDefault() - local
          .ForUpdateUraCollectionApplication.CollectionAmountApplied;
      }
      else
      {
        local.ForUpdateUraCollectionApplication.Type1 = "A";
        local.ForUpdateImHouseholdMbrMnthlySum.UraAmount =
          local.ForUpdateImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault() -
          local.ForUpdateUraCollectionApplication.CollectionAmountApplied;
      }

      try
      {
        UpdateImHouseholdMbrMnthlySum();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OE0000_IM_HH_MBR_MTHLY_SUM_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "OE_IM_HH_MBR_MTHLY_SUM_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!ReadCollection())
      {
        ExitState = "FN0000_COLLECTION_NF_RB";

        return;
      }

      try
      {
        CreateUraCollectionApplication();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_URA_COLL_APPL_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_URA_COLL_APPL_PV_RB";

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
      // : Discover the cause of the error.
      if (ReadImHousehold())
      {
        ExitState = "FN0000_IM_HH_MBR_MTHLY_SUM_NF_RB";
      }
      else
      {
        ExitState = "OE0000_IM_HOUSEHOLD_NF_RB";
      }
    }
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.UraAmount = source.UraAmount;
    target.UraMedicalAmount = source.UraMedicalAmount;
  }

  private void CreateUraCollectionApplication()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingImHouseholdMbrMnthlySum.Populated);

    var collectionAmountApplied =
      local.ForUpdateUraCollectionApplication.CollectionAmountApplied;
    var createdBy = global.UserId;
    var cspNumber = entities.ExistingCollection.CspNumber;
    var cpaType = entities.ExistingCollection.CpaType;
    var otyIdentifier = entities.ExistingCollection.OtyId;
    var obgIdentifier = entities.ExistingCollection.ObgId;
    var otrIdentifier = entities.ExistingCollection.OtrId;
    var otrType = entities.ExistingCollection.OtrType;
    var cstIdentifier = entities.ExistingCollection.CstId;
    var crvIdentifier = entities.ExistingCollection.CrvId;
    var crtIdentifier = entities.ExistingCollection.CrtType;
    var crdIdentifier = entities.ExistingCollection.CrdId;
    var colIdentifier = entities.ExistingCollection.SystemGeneratedIdentifier;
    var imhAeCaseNo = entities.ExistingImHouseholdMbrMnthlySum.ImhAeCaseNo;
    var cspNumber0 = entities.ExistingImHouseholdMbrMnthlySum.CspNumber;
    var imsMonth = entities.ExistingImHouseholdMbrMnthlySum.Month;
    var imsYear = entities.ExistingImHouseholdMbrMnthlySum.Year;
    var createdTstamp = Now();
    var type1 = local.ForUpdateUraCollectionApplication.Type1 ?? "";

    CheckValid<UraCollectionApplication>("CpaType", cpaType);
    CheckValid<UraCollectionApplication>("OtrType", otrType);
    entities.New1.Populated = false;
    Update("CreateUraCollectionApplication",
      (db, command) =>
      {
        db.SetDecimal(command, "collAmtAppld", collectionAmountApplied);
        db.SetString(command, "createdBy", createdBy);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otyIdentifier", otyIdentifier);
        db.SetInt32(command, "obgIdentifier", obgIdentifier);
        db.SetInt32(command, "otrIdentifier", otrIdentifier);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "colIdentifier", colIdentifier);
        db.SetString(command, "imhAeCaseNo", imhAeCaseNo);
        db.SetString(command, "cspNumber0", cspNumber0);
        db.SetInt32(command, "imsMonth", imsMonth);
        db.SetInt32(command, "imsYear", imsYear);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "type", type1);
      });

    entities.New1.CollectionAmountApplied = collectionAmountApplied;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.OtyIdentifier = otyIdentifier;
    entities.New1.ObgIdentifier = obgIdentifier;
    entities.New1.OtrIdentifier = otrIdentifier;
    entities.New1.OtrType = otrType;
    entities.New1.CstIdentifier = cstIdentifier;
    entities.New1.CrvIdentifier = crvIdentifier;
    entities.New1.CrtIdentifier = crtIdentifier;
    entities.New1.CrdIdentifier = crdIdentifier;
    entities.New1.ColIdentifier = colIdentifier;
    entities.New1.ImhAeCaseNo = imhAeCaseNo;
    entities.New1.CspNumber0 = cspNumber0;
    entities.New1.ImsMonth = imsMonth;
    entities.New1.ImsYear = imsYear;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.Type1 = type1;
    entities.New1.Populated = true;
  }

  private bool ReadCollection()
  {
    entities.ExistingCollection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(command, "otrId", import.Debt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "collId", import.Collection.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 1);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 2);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 3);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 4);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 6);
        entities.ExistingCollection.CpaType = db.GetString(reader, 7);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 8);
        entities.ExistingCollection.OtrType = db.GetString(reader, 9);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 10);
        entities.ExistingCollection.Populated = true;
      });
  }

  private bool ReadImHousehold()
  {
    entities.ExistingImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", import.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ExistingImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ExistingImHousehold.Populated = true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum()
  {
    entities.ExistingImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", import.ImHousehold.AeCaseNo);
        db.SetString(command, "cspNumber", import.Member.Number);
        db.SetInt32(command, "year0", import.ImHouseholdMbrMnthlySum.Year);
        db.SetInt32(command, "month0", import.ImHouseholdMbrMnthlySum.Month);
      },
      (db, reader) =>
      {
        entities.ExistingImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ExistingImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ExistingImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 2);
        entities.ExistingImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ExistingImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingImHouseholdMbrMnthlySum.ImhAeCaseNo =
          db.GetString(reader, 6);
        entities.ExistingImHouseholdMbrMnthlySum.CspNumber =
          db.GetString(reader, 7);
        entities.ExistingImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private void UpdateImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingImHouseholdMbrMnthlySum.Populated);

    var uraAmount =
      local.ForUpdateImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
    var uraMedicalAmount =
      local.ForUpdateImHouseholdMbrMnthlySum.UraMedicalAmount.
        GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ExistingImHouseholdMbrMnthlySum.Populated = false;
    Update("UpdateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "uraAmount", uraAmount);
        db.SetNullableDecimal(command, "uraMedicalAmount", uraMedicalAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "year0", entities.ExistingImHouseholdMbrMnthlySum.Year);
        db.SetInt32(
          command, "month0", entities.ExistingImHouseholdMbrMnthlySum.Month);
        db.SetString(
          command, "imhAeCaseNo",
          entities.ExistingImHouseholdMbrMnthlySum.ImhAeCaseNo);
        db.SetString(
          command, "cspNumber",
          entities.ExistingImHouseholdMbrMnthlySum.CspNumber);
      });

    entities.ExistingImHouseholdMbrMnthlySum.UraAmount = uraAmount;
    entities.ExistingImHouseholdMbrMnthlySum.UraMedicalAmount =
      uraMedicalAmount;
    entities.ExistingImHouseholdMbrMnthlySum.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingImHouseholdMbrMnthlySum.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingImHouseholdMbrMnthlySum.Populated = true;
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
    /// A value of Member.
    /// </summary>
    [JsonPropertyName("member")]
    public CsePerson Member
    {
      get => member ??= new();
      set => member = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public UraCollectionApplication ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of HardcodedMsType.
    /// </summary>
    [JsonPropertyName("hardcodedMsType")]
    public ObligationType HardcodedMsType
    {
      get => hardcodedMsType ??= new();
      set => hardcodedMsType = value;
    }

    /// <summary>
    /// A value of HardcodedMjType.
    /// </summary>
    [JsonPropertyName("hardcodedMjType")]
    public ObligationType HardcodedMjType
    {
      get => hardcodedMjType ??= new();
      set => hardcodedMjType = value;
    }

    /// <summary>
    /// A value of HardcodedMcType.
    /// </summary>
    [JsonPropertyName("hardcodedMcType")]
    public ObligationType HardcodedMcType
    {
      get => hardcodedMcType ??= new();
      set => hardcodedMcType = value;
    }

    /// <summary>
    /// A value of PersistantDelMe.
    /// </summary>
    [JsonPropertyName("persistantDelMe")]
    public Collection PersistantDelMe
    {
      get => persistantDelMe ??= new();
      set => persistantDelMe = value;
    }

    private CsePerson member;
    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private UraCollectionApplication forUpdate;
    private CsePerson obligor;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction debt;
    private Collection collection;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMcType;
    private Collection persistantDelMe;
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
    /// A value of ForUpdateUraCollectionApplication.
    /// </summary>
    [JsonPropertyName("forUpdateUraCollectionApplication")]
    public UraCollectionApplication ForUpdateUraCollectionApplication
    {
      get => forUpdateUraCollectionApplication ??= new();
      set => forUpdateUraCollectionApplication = value;
    }

    /// <summary>
    /// A value of ForUpdateImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("forUpdateImHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ForUpdateImHouseholdMbrMnthlySum
    {
      get => forUpdateImHouseholdMbrMnthlySum ??= new();
      set => forUpdateImHouseholdMbrMnthlySum = value;
    }

    private UraCollectionApplication forUpdateUraCollectionApplication;
    private ImHouseholdMbrMnthlySum forUpdateImHouseholdMbrMnthlySum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    /// <summary>
    /// A value of ExistingImHousehold.
    /// </summary>
    [JsonPropertyName("existingImHousehold")]
    public ImHousehold ExistingImHousehold
    {
      get => existingImHousehold ??= new();
      set => existingImHousehold = value;
    }

    /// <summary>
    /// A value of ExistingSuppPrsn.
    /// </summary>
    [JsonPropertyName("existingSuppPrsn")]
    public CsePerson ExistingSuppPrsn
    {
      get => existingSuppPrsn ??= new();
      set => existingSuppPrsn = value;
    }

    /// <summary>
    /// A value of ExistingImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("existingImHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ExistingImHouseholdMbrMnthlySum
    {
      get => existingImHouseholdMbrMnthlySum ??= new();
      set => existingImHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public UraCollectionApplication New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private Collection existingCollection;
    private ImHousehold existingImHousehold;
    private CsePerson existingSuppPrsn;
    private ImHouseholdMbrMnthlySum existingImHouseholdMbrMnthlySum;
    private UraCollectionApplication new1;
  }
#endregion
}
