// Program: FN_TRANSFER_URA_COLL_APPL, ID: 374492087, model: 746.
// Short name: SWE02873
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_TRANSFER_URA_COLL_APPL.
/// </para>
/// <para>
/// This action block is responsible for transfering the relationship between an
/// old collection / URA collection application to a new collection.
/// </para>
/// </summary>
[Serializable]
public partial class FnTransferUraCollAppl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_TRANSFER_URA_COLL_APPL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnTransferUraCollAppl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnTransferUraCollAppl.
  /// </summary>
  public FnTransferUraCollAppl(IContext context, Import import, Export export):
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
    // *******************************************
    // This action block reassigns the relationship between collection / URA 
    // collection application to a new collection.  This currently occurs as a
    // result of a case/role change.
    // **********************************************************************************
    // **************************** 
    // MAINTENANCE LOG
    // *************************************
    //   Date      Programmer   Problem #   Description
    // **********************************************************************************
    // 04/20/2000  E. Shirk       new       URA changes to support PRWORA 
    // distribution requirements.
    // 11/30/2000  E. Lyman       PR#108246 Added attribute to collection in the
    // import
    //                                      
    // and export views: ocse34_reporting
    // period.
    // ****************************************************************************************
    // Perform URA collection application transfer only for primary
    // collections.
    // ***************************************************************************************
    if (AsChar(import.PersistentNew.ConcurrentInd) != 'Y')
    {
    }
    else
    {
      return;
    }

    // ****************************************************************************************
    // Perform URA collection application transfer only for AF or FC programs.
    // ***************************************************************************************
    if (Equal(import.PersistentNew.ProgramAppliedTo, "FC") || Equal
      (import.PersistentNew.ProgramAppliedTo, "AF"))
    {
    }
    else
    {
      return;
    }

    // ****************************************************************************************
    // Read all ura collection application occurences related to the current
    // collection.
    // ***************************************************************************************
    foreach(var item in ReadUraCollectionApplicationImHouseholdMbrMnthlySum())
    {
      // ****************************************************************************************
      // Build new ura_collection_application associated to the new collection
      // ***************************************************************************************
      try
      {
        CreateUraCollectionApplication();

        // Continue
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_URA_COLLECTION_APPL_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_URA_COLLECTION_APPL_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // ****************************************************************************************
      // Delete the existing ura_collection_appliation entity related to the
      // old collection
      // ***************************************************************************************
      DeleteUraCollectionApplication();
    }
  }

  private void CreateUraCollectionApplication()
  {
    System.Diagnostics.Debug.Assert(import.PersistentNew.Populated);
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var collectionAmountApplied = entities.Existing.CollectionAmountApplied;
    var createdBy = global.UserId;
    var cspNumber = import.PersistentNew.CspNumber;
    var cpaType = import.PersistentNew.CpaType;
    var otyIdentifier = import.PersistentNew.OtyId;
    var obgIdentifier = import.PersistentNew.ObgId;
    var otrIdentifier = import.PersistentNew.OtrId;
    var otrType = import.PersistentNew.OtrType;
    var cstIdentifier = import.PersistentNew.CstId;
    var crvIdentifier = import.PersistentNew.CrvId;
    var crtIdentifier = import.PersistentNew.CrtType;
    var crdIdentifier = import.PersistentNew.CrdId;
    var colIdentifier = import.PersistentNew.SystemGeneratedIdentifier;
    var imhAeCaseNo = entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo;
    var cspNumber0 = entities.ImHouseholdMbrMnthlySum.CspNumber;
    var imsMonth = entities.ImHouseholdMbrMnthlySum.Month;
    var imsYear = entities.ImHouseholdMbrMnthlySum.Year;
    var createdTstamp = Now();
    var type1 = entities.Existing.Type1;

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

  private void DeleteUraCollectionApplication()
  {
    Update("DeleteUraCollectionApplication",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Existing.CspNumber);
        db.SetString(command, "cpaType", entities.Existing.CpaType);
        db.SetInt32(command, "otyIdentifier", entities.Existing.OtyIdentifier);
        db.SetInt32(command, "obgIdentifier", entities.Existing.ObgIdentifier);
        db.SetInt32(command, "otrIdentifier", entities.Existing.OtrIdentifier);
        db.SetString(command, "otrType", entities.Existing.OtrType);
        db.SetInt32(command, "cstIdentifier", entities.Existing.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", entities.Existing.CrvIdentifier);
        db.SetInt32(command, "crtIdentifier", entities.Existing.CrtIdentifier);
        db.SetInt32(command, "crdIdentifier", entities.Existing.CrdIdentifier);
        db.SetInt32(command, "colIdentifier", entities.Existing.ColIdentifier);
        db.SetString(command, "imhAeCaseNo", entities.Existing.ImhAeCaseNo);
        db.SetString(command, "cspNumber0", entities.Existing.CspNumber0);
        db.SetInt32(command, "imsMonth", entities.Existing.ImsMonth);
        db.SetInt32(command, "imsYear", entities.Existing.ImsYear);
        db.SetDateTime(
          command, "createdTstamp",
          entities.Existing.CreatedTstamp.GetValueOrDefault());
      });
  }

  private IEnumerable<bool>
    ReadUraCollectionApplicationImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(import.PersistentOld.Populated);
    entities.Existing.Populated = false;
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return ReadEach("ReadUraCollectionApplicationImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetInt32(command, "obgIdentifier", import.PersistentOld.ObgId);
        db.SetString(command, "cpaType", import.PersistentOld.CpaType);
        db.SetInt32(command, "otyIdentifier", import.PersistentOld.OtyId);
        db.SetString(command, "cspNumber", import.PersistentOld.CspNumber);
        db.SetInt32(command, "otrIdentifier", import.PersistentOld.OtrId);
        db.SetInt32(command, "crvIdentifier", import.PersistentOld.CrvId);
        db.SetInt32(
          command, "colIdentifier",
          import.PersistentOld.SystemGeneratedIdentifier);
        db.SetInt32(command, "crdIdentifier", import.PersistentOld.CrdId);
        db.SetInt32(command, "cstIdentifier", import.PersistentOld.CstId);
        db.SetInt32(command, "crtIdentifier", import.PersistentOld.CrtType);
        db.SetString(command, "otrType", import.PersistentOld.OtrType);
      },
      (db, reader) =>
      {
        entities.Existing.CollectionAmountApplied = db.GetDecimal(reader, 0);
        entities.Existing.CreatedBy = db.GetString(reader, 1);
        entities.Existing.CspNumber = db.GetString(reader, 2);
        entities.Existing.CpaType = db.GetString(reader, 3);
        entities.Existing.OtyIdentifier = db.GetInt32(reader, 4);
        entities.Existing.ObgIdentifier = db.GetInt32(reader, 5);
        entities.Existing.OtrIdentifier = db.GetInt32(reader, 6);
        entities.Existing.OtrType = db.GetString(reader, 7);
        entities.Existing.CstIdentifier = db.GetInt32(reader, 8);
        entities.Existing.CrvIdentifier = db.GetInt32(reader, 9);
        entities.Existing.CrtIdentifier = db.GetInt32(reader, 10);
        entities.Existing.CrdIdentifier = db.GetInt32(reader, 11);
        entities.Existing.ColIdentifier = db.GetInt32(reader, 12);
        entities.Existing.ImhAeCaseNo = db.GetString(reader, 13);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 13);
        entities.Existing.CspNumber0 = db.GetString(reader, 14);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 14);
        entities.Existing.ImsMonth = db.GetInt32(reader, 15);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 15);
        entities.Existing.ImsYear = db.GetInt32(reader, 16);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 16);
        entities.Existing.CreatedTstamp = db.GetDateTime(reader, 17);
        entities.Existing.Type1 = db.GetNullableString(reader, 18);
        entities.Existing.Populated = true;
        entities.ImHouseholdMbrMnthlySum.Populated = true;

        return true;
      });
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
    /// A value of PersistentOld.
    /// </summary>
    [JsonPropertyName("persistentOld")]
    public Collection PersistentOld
    {
      get => persistentOld ??= new();
      set => persistentOld = value;
    }

    /// <summary>
    /// A value of PersistentNew.
    /// </summary>
    [JsonPropertyName("persistentNew")]
    public Collection PersistentNew
    {
      get => persistentNew ??= new();
      set => persistentNew = value;
    }

    private Collection persistentOld;
    private Collection persistentNew;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public UraCollectionApplication New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public UraCollectionApplication Existing
    {
      get => existing ??= new();
      set => existing = value;
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

    private UraCollectionApplication new1;
    private UraCollectionApplication existing;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
  }
#endregion
}
