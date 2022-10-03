// Program: FN_UPDATE_URA_AMOUNT, ID: 374418589, model: 746.
// Short name: SWE02872
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_URA_AMOUNT.
/// </para>
/// <para>
/// This action block will adjust a member of a housholds URA amount.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateUraAmount: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_URA_AMOUNT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateUraAmount(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateUraAmount.
  /// </summary>
  public FnUpdateUraAmount(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************** PURPOSE 
    // ************************************************
    // This action block updates the  ura_monthly_amount or ura_medical_amount 
    // on the IM_HOUSEHOLD_MBR_MNTHLY_SUM entity.   This process will be
    // triggered when there is a collection adjustment, and that adjustment
    // needs to have any associated URA reversed.
    // ***************************************************************************************
    // **************************** MAINTENANCE 
    // LOG
    // ******************************************
    //   Date      Programmer   Problem #   Description
    // ***************************************************************************************
    // 04/20/2000  E. Shirk       new       URA changes to support PRWORA 
    // distribution requirements.
    // ****************************************************************************************
    // Retrieve current collection.
    // ***************************************************************************************
    if (!ReadCollection())
    {
      ExitState = "FN0000_COLLECTION_NF";

      return;
    }

    // ****************************************************************************************
    // Perform URA adjustement only for primary collections.
    // ***************************************************************************************
    if (AsChar(entities.Collection.ConcurrentInd) != 'Y')
    {
    }
    else
    {
      return;
    }

    // ****************************************************************************************
    // Perform URA adjustement only for AF or FC programs.
    // ***************************************************************************************
    if (Equal(entities.Collection.ProgramAppliedTo, "FC") || Equal
      (entities.Collection.ProgramAppliedTo, "AF"))
    {
    }
    else
    {
      return;
    }

    // ****************************************************************************************
    // Read all URA_COLLECTION_APPLICATION 's associated to a collection.
    // ***************************************************************************************
    foreach(var item in ReadUraCollectionApplicationImHouseholdMbrMnthlySum())
    {
      // ****************************************************************************************
      // Determine whether the collection_application is for a regular or
      // medical URA.
      // * Update the appprioate entity attribute.
      // ***************************************************************************************
      if (AsChar(entities.UraCollectionApplication.Type1) == 'M')
      {
        try
        {
          UpdateImHouseholdMbrMnthlySum2();

          // Continue
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        try
        {
          UpdateImHouseholdMbrMnthlySum1();

          // Continue
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // ****************************************************************************************
      // Delete the existing URA_COLLECTION_APPLICATION.  This entity will be
      // recreated when
      // * the collection is recreated.
      // ***************************************************************************************
      DeleteUraCollectionApplication();
    }
  }

  private void DeleteUraCollectionApplication()
  {
    Update("DeleteUraCollectionApplication",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.UraCollectionApplication.CspNumber);
        db.SetString(
          command, "cpaType", entities.UraCollectionApplication.CpaType);
        db.SetInt32(
          command, "otyIdentifier",
          entities.UraCollectionApplication.OtyIdentifier);
        db.SetInt32(
          command, "obgIdentifier",
          entities.UraCollectionApplication.ObgIdentifier);
        db.SetInt32(
          command, "otrIdentifier",
          entities.UraCollectionApplication.OtrIdentifier);
        db.SetString(
          command, "otrType", entities.UraCollectionApplication.OtrType);
        db.SetInt32(
          command, "cstIdentifier",
          entities.UraCollectionApplication.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.UraCollectionApplication.CrvIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.UraCollectionApplication.CrtIdentifier);
        db.SetInt32(
          command, "crdIdentifier",
          entities.UraCollectionApplication.CrdIdentifier);
        db.SetInt32(
          command, "colIdentifier",
          entities.UraCollectionApplication.ColIdentifier);
        db.SetString(
          command, "imhAeCaseNo",
          entities.UraCollectionApplication.ImhAeCaseNo);
        db.SetString(
          command, "cspNumber0", entities.UraCollectionApplication.CspNumber0);
        db.SetInt32(
          command, "imsMonth", entities.UraCollectionApplication.ImsMonth);
        db.SetInt32(
          command, "imsYear", entities.UraCollectionApplication.ImsYear);
        db.SetDateTime(
          command, "createdTstamp",
          entities.UraCollectionApplication.CreatedTstamp.GetValueOrDefault());
      });
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "collId", import.Collection.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.ConcurrentInd = db.GetString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.Amount = db.GetDecimal(reader, 12);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 13);
        entities.Collection.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadUraCollectionApplicationImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.UraCollectionApplication.Populated = false;

    return ReadEach("ReadUraCollectionApplicationImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetInt32(command, "obgIdentifier", entities.Collection.ObgId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otyIdentifier", entities.Collection.OtyId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "otrIdentifier", entities.Collection.OtrId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(
          command, "colIdentifier",
          entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crdIdentifier", entities.Collection.CrdId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
        db.SetString(command, "otrType", entities.Collection.OtrType);
      },
      (db, reader) =>
      {
        entities.UraCollectionApplication.CollectionAmountApplied =
          db.GetDecimal(reader, 0);
        entities.UraCollectionApplication.CspNumber = db.GetString(reader, 1);
        entities.UraCollectionApplication.CpaType = db.GetString(reader, 2);
        entities.UraCollectionApplication.OtyIdentifier =
          db.GetInt32(reader, 3);
        entities.UraCollectionApplication.ObgIdentifier =
          db.GetInt32(reader, 4);
        entities.UraCollectionApplication.OtrIdentifier =
          db.GetInt32(reader, 5);
        entities.UraCollectionApplication.OtrType = db.GetString(reader, 6);
        entities.UraCollectionApplication.CstIdentifier =
          db.GetInt32(reader, 7);
        entities.UraCollectionApplication.CrvIdentifier =
          db.GetInt32(reader, 8);
        entities.UraCollectionApplication.CrtIdentifier =
          db.GetInt32(reader, 9);
        entities.UraCollectionApplication.CrdIdentifier =
          db.GetInt32(reader, 10);
        entities.UraCollectionApplication.ColIdentifier =
          db.GetInt32(reader, 11);
        entities.UraCollectionApplication.ImhAeCaseNo =
          db.GetString(reader, 12);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 12);
        entities.UraCollectionApplication.CspNumber0 = db.GetString(reader, 13);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 13);
        entities.UraCollectionApplication.ImsMonth = db.GetInt32(reader, 14);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 14);
        entities.UraCollectionApplication.ImsYear = db.GetInt32(reader, 15);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 15);
        entities.UraCollectionApplication.CreatedTstamp =
          db.GetDateTime(reader, 16);
        entities.UraCollectionApplication.Type1 =
          db.GetNullableString(reader, 17);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 18);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 19);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 21);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.UraCollectionApplication.Populated = true;

        return true;
      });
  }

  private void UpdateImHouseholdMbrMnthlySum1()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var uraAmount =
      entities.ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault() +
      entities.UraCollectionApplication.CollectionAmountApplied;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("UpdateImHouseholdMbrMnthlySum1",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "uraAmount", uraAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(command, "year0", entities.ImHouseholdMbrMnthlySum.Year);
        db.SetInt32(command, "month0", entities.ImHouseholdMbrMnthlySum.Month);
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
        db.SetString(
          command, "cspNumber", entities.ImHouseholdMbrMnthlySum.CspNumber);
      });

    entities.ImHouseholdMbrMnthlySum.UraAmount = uraAmount;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedBy = lastUpdatedBy;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst = lastUpdatedTmst;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
  }

  private void UpdateImHouseholdMbrMnthlySum2()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var uraMedicalAmount =
      entities.ImHouseholdMbrMnthlySum.UraMedicalAmount.GetValueOrDefault() +
      entities.UraCollectionApplication.CollectionAmountApplied;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("UpdateImHouseholdMbrMnthlySum2",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "uraMedicalAmount", uraMedicalAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(command, "year0", entities.ImHouseholdMbrMnthlySum.Year);
        db.SetInt32(command, "month0", entities.ImHouseholdMbrMnthlySum.Month);
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
        db.SetString(
          command, "cspNumber", entities.ImHouseholdMbrMnthlySum.CspNumber);
      });

    entities.ImHouseholdMbrMnthlySum.UraMedicalAmount = uraMedicalAmount;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedBy = lastUpdatedBy;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst = lastUpdatedTmst;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePerson csePerson;
    private Collection collection;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of UraCollectionApplication.
    /// </summary>
    [JsonPropertyName("uraCollectionApplication")]
    public UraCollectionApplication UraCollectionApplication
    {
      get => uraCollectionApplication ??= new();
      set => uraCollectionApplication = value;
    }

    private CsePersonAccount obligor;
    private Collection collection;
    private CsePerson csePerson;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private UraCollectionApplication uraCollectionApplication;
  }
#endregion
}
