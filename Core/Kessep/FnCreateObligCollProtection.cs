// Program: FN_CREATE_OBLIG_COLL_PROTECTION, ID: 373387451, model: 746.
// Short name: SWE00491
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_OBLIG_COLL_PROTECTION.
/// </summary>
[Serializable]
public partial class FnCreateObligCollProtection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_OBLIG_COLL_PROTECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateObligCollProtection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateObligCollProtection.
  /// </summary>
  public FnCreateObligCollProtection(IContext context, Import import,
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
    // ----------------------------------------------------------------
    // Date     Developer Name   Request#  Description
    // 12/27/01  Mark Ashworth   WR10504   New Development
    // ----------------------------------------------------------------
    if (ReadObligation())
    {
      // ----------------------------------------------------------------
      // Do not allow the user to add a timeframe if the ura or med ura amount 
      // is negative.
      // ----------------------------------------------------------------
      if (import.ObligationType.SystemGeneratedIdentifier == import
        .HardcodeMedForCash.SystemGeneratedIdentifier || entities
        .ObligationType.SystemGeneratedIdentifier == import
        .HardcodeMedJudgement.SystemGeneratedIdentifier || entities
        .ObligationType.SystemGeneratedIdentifier == import
        .HardcodeMedicalSupport.SystemGeneratedIdentifier)
      {
        if (ReadCollectionImHouseholdMbrMnthlySum2())
        {
          ExitState = "MED_URA_AMT_NEG_TFRAM_NOT_ADDED";

          return;
        }
      }
      else if (ReadCollectionImHouseholdMbrMnthlySum1())
      {
        ExitState = "URA_AMT_NEG_TFRAM_NOT_ADDED";

        return;
      }

      // ----------------------------------------------------------------
      // We are only looking for overlapping dates that are active. If the 
      // protection level is different, overlapping is ok.  If protection is
      // spaces, no overlapping at all. Only check it if its not a secondary
      // obligation.  Secondary obligations will not get timeframes. Only the
      // collection will be updated to "p" for protected
      // ----------------------------------------------------------------
      if (AsChar(entities.Obligation.PrimarySecondaryCode) != 'S')
      {
        if (IsEmpty(import.ObligCollProtectionHist.ProtectionLevel))
        {
          if (ReadObligCollProtectionHist1())
          {
            ExitState = "FN0000_DATE_OVERLAP_RB";

            return;
          }
        }
        else if (ReadObligCollProtectionHist2())
        {
          ExitState = "FN0000_DATE_OVERLAP_RB";

          return;
        }
      }

      // ----------------------------------------------------------------
      // Do not create a Time Frame for Secondaries, just protect the 
      // collection.
      // ----------------------------------------------------------------
      if (AsChar(entities.Obligation.PrimarySecondaryCode) != 'S')
      {
        try
        {
          CreateObligCollProtectionHist();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OB_COLL_PROT_HIST_AE_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OB_COLL_PROT_HIST_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // ----------------------------------------------------------------
      // Set the corresponding collection dist method to P. This indicates 
      // protected so the automatic distribution will not execute for these
      // collections.  There are separate reads based on the protection level.
      // If there are no collections found for that level, do not allow the time
      // frame to be added.
      // ----------------------------------------------------------------
      local.CollectionsFound.Flag = "N";

      if (IsEmpty(import.ObligCollProtectionHist.ProtectionLevel))
      {
        foreach(var item in ReadCollection2())
        {
          local.CollectionsFound.Flag = "Y";

          try
          {
            UpdateCollection();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_COLLECTION_NU_RB";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_COLLECTION_PV_RB";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else
      {
        foreach(var item in ReadCollection1())
        {
          local.CollectionsFound.Flag = "Y";

          try
          {
            UpdateCollection();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_COLLECTION_NU_RB";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_COLLECTION_PV_RB";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      if (AsChar(local.CollectionsFound.Flag) == 'N')
      {
        ExitState = "FN0000_APPLIED_TO_COLL_NF_RB";
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";
    }
  }

  private void CreateObligCollProtectionHist()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var cvrdCollStrtDt = import.ObligCollProtectionHist.CvrdCollStrtDt;
    var cvrdCollEndDt = import.ObligCollProtectionHist.CvrdCollEndDt;
    var deactivationDate = local.Null1.DeactivationDate;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var reasonText = import.ObligCollProtectionHist.ReasonText;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var otyIdentifier = entities.Obligation.DtyGeneratedId;
    var obgIdentifier = entities.Obligation.SystemGeneratedIdentifier;
    var protectionLevel = import.ObligCollProtectionHist.ProtectionLevel;

    CheckValid<ObligCollProtectionHist>("CpaType", cpaType);
    entities.ObligCollProtectionHist.Populated = false;
    Update("CreateObligCollProtectionHist",
      (db, command) =>
      {
        db.SetDate(command, "cvrdCollStrtDt", cvrdCollStrtDt);
        db.SetDate(command, "cvrdCollEndDt", cvrdCollEndDt);
        db.SetDate(command, "deactivationDate", deactivationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetString(command, "reasonText", reasonText);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otyIdentifier", otyIdentifier);
        db.SetInt32(command, "obgIdentifier", obgIdentifier);
        db.SetString(command, "protectionLevel", protectionLevel);
      });

    entities.ObligCollProtectionHist.CvrdCollStrtDt = cvrdCollStrtDt;
    entities.ObligCollProtectionHist.CvrdCollEndDt = cvrdCollEndDt;
    entities.ObligCollProtectionHist.DeactivationDate = deactivationDate;
    entities.ObligCollProtectionHist.CreatedBy = createdBy;
    entities.ObligCollProtectionHist.CreatedTmst = createdTmst;
    entities.ObligCollProtectionHist.LastUpdatedBy = "";
    entities.ObligCollProtectionHist.LastUpdatedTmst = null;
    entities.ObligCollProtectionHist.ReasonText = reasonText;
    entities.ObligCollProtectionHist.CspNumber = cspNumber;
    entities.ObligCollProtectionHist.CpaType = cpaType;
    entities.ObligCollProtectionHist.OtyIdentifier = otyIdentifier;
    entities.ObligCollProtectionHist.ObgIdentifier = obgIdentifier;
    entities.ObligCollProtectionHist.ProtectionLevel = protectionLevel;
    entities.ObligCollProtectionHist.Populated = true;
  }

  private IEnumerable<bool> ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
        db.SetString(
          command, "appliedToCd",
          import.ObligCollProtectionHist.ProtectionLevel);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.Collection.DistributionMethod = db.GetString(reader, 16);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.Collection.DistributionMethod = db.GetString(reader, 16);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private bool ReadCollectionImHouseholdMbrMnthlySum1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.Collection.Populated = false;

    return Read("ReadCollectionImHouseholdMbrMnthlySum1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.Collection.DistributionMethod = db.GetString(reader, 16);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 17);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 18);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 19);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 20);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 21);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 22);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.Collection.Populated = true;
      });
  }

  private bool ReadCollectionImHouseholdMbrMnthlySum2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.Collection.Populated = false;

    return Read("ReadCollectionImHouseholdMbrMnthlySum2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.Collection.DistributionMethod = db.GetString(reader, 16);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 17);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 18);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 19);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 20);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 21);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 22);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.Collection.Populated = true;
      });
  }

  private bool ReadObligCollProtectionHist1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          entities.Obligation.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "otyIdentifier", entities.Obligation.DtyGeneratedId);
          
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "deactivationDate",
          local.Null1.DeactivationDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 1);
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ObligCollProtectionHist.CreatedBy = db.GetString(reader, 3);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 4);
        entities.ObligCollProtectionHist.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ObligCollProtectionHist.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.ObligCollProtectionHist.ReasonText = db.GetString(reader, 7);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 8);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 9);
        entities.ObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 10);
        entities.ObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 12);
        entities.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadObligCollProtectionHist2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          entities.Obligation.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "otyIdentifier", entities.Obligation.DtyGeneratedId);
          
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetString(
          command, "protectionLevel",
          import.ObligCollProtectionHist.ProtectionLevel);
        db.SetDate(
          command, "deactivationDate",
          local.Null1.DeactivationDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 1);
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ObligCollProtectionHist.CreatedBy = db.GetString(reader, 3);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 4);
        entities.ObligCollProtectionHist.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ObligCollProtectionHist.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.ObligCollProtectionHist.ReasonText = db.GetString(reader, 7);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 8);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 9);
        entities.ObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 10);
        entities.ObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 12);
        entities.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.Obligation.Populated = true;
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var distributionMethod = "P";

    CheckValid<Collection>("DistributionMethod", distributionMethod);
    entities.Collection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "distMtd", distributionMethod);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otrId", entities.Collection.OtrId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
      });

    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.DistributionMethod = distributionMethod;
    entities.Collection.Populated = true;
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
    /// A value of HardcodeMedForCash.
    /// </summary>
    [JsonPropertyName("hardcodeMedForCash")]
    public ObligationType HardcodeMedForCash
    {
      get => hardcodeMedForCash ??= new();
      set => hardcodeMedForCash = value;
    }

    /// <summary>
    /// A value of HardcodeMedJudgement.
    /// </summary>
    [JsonPropertyName("hardcodeMedJudgement")]
    public ObligationType HardcodeMedJudgement
    {
      get => hardcodeMedJudgement ??= new();
      set => hardcodeMedJudgement = value;
    }

    /// <summary>
    /// A value of HardcodeMedicalSupport.
    /// </summary>
    [JsonPropertyName("hardcodeMedicalSupport")]
    public ObligationType HardcodeMedicalSupport
    {
      get => hardcodeMedicalSupport ??= new();
      set => hardcodeMedicalSupport = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
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

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private ObligationType hardcodeMedForCash;
    private ObligationType hardcodeMedJudgement;
    private ObligationType hardcodeMedicalSupport;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private ObligationType obligationType;
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
    /// A value of CollectionsFound.
    /// </summary>
    [JsonPropertyName("collectionsFound")]
    public Common CollectionsFound
    {
      get => collectionsFound ??= new();
      set => collectionsFound = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public ObligCollProtectionHist Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private Common collectionsFound;
    private ObligCollProtectionHist null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    /// <summary>
    /// A value of Overlapping.
    /// </summary>
    [JsonPropertyName("overlapping")]
    public ObligCollProtectionHist Overlapping
    {
      get => overlapping ??= new();
      set => overlapping = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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

    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private UraCollectionApplication uraCollectionApplication;
    private ObligCollProtectionHist overlapping;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private ObligationType obligationType;
    private Collection collection;
    private ObligationTransaction obligationTransaction;
  }
#endregion
}
